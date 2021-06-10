using Noggog;
using Noggog.WPF;
using ReactiveUI;
using Serilog;
using System;
using System.Linq;
using System.Reactive;
using System.Reactive.Linq;
using System.Threading;
using DynamicData;
using Synthesis.Bethesda.DTO;
using Mutagen.Bethesda.Synthesis.WPF;
using LibGit2Sharp;
using Mutagen.Bethesda.Plugins.Order;
using Mutagen.Bethesda.WPF.Plugins.Order;
using Synthesis.Bethesda.Execution.CLI;
using Synthesis.Bethesda.Execution.GitRespository;
using Synthesis.Bethesda.GUI.Profiles.Plugins;
using Synthesis.Bethesda.GUI.Services;

namespace Synthesis.Bethesda.GUI
{
    public class PatcherSettingsVM : ViewModel
    {
        private readonly IProvideRepositoryCheckouts _RepoCheckouts;

        public ReactiveCommand<Unit, Unit> OpenSettingsCommand { get; }

        private readonly ObservableAsPropertyHelper<SettingsConfiguration> _SettingsConfiguration;
        public SettingsConfiguration SettingsConfiguration => _SettingsConfiguration.Value;

        private readonly ObservableAsPropertyHelper<bool> _SettingsOpen;
        public bool SettingsOpen => _SettingsOpen.Value;

        private bool _hasBeenRetrieved = false;
        private readonly ObservableAsPropertyHelper<AutogeneratedSettingsVM?> _ReflectionSettings;
        public AutogeneratedSettingsVM? ReflectionSettings
        {
            get
            {
                _hasBeenRetrieved = true;
                return _ReflectionSettings.Value;
            }
        }

        public PatcherSettingsVM(
            ILogger logger,
            PatcherVM parent,
            IProfileIdentifier ident,
            IProfileLoadOrder loadOrder,
            IProfileDataFolder dataFolder,
            IProfileSimpleLinkCache linkCache,
            IObservable<(GetResponse<FilePath> ProjPath, string? SynthVersion)> source,
            bool needBuild,
            IProvideAutogeneratedSettings autoGenSettingsProvider,
            IProvideRepositoryCheckouts repoCheckouts,
            IGetSettingsStyle getSettingsStyle,
            IOpenForSettings openForSettings,
            IOpenSettingsHost openSettingsHost)
        {
            _RepoCheckouts = repoCheckouts;
            _SettingsConfiguration = source
                .Select(i =>
                {
                    return Observable.Create<SettingsConfiguration>(async (observer, cancel) =>
                    {
                        observer.OnNext(new SettingsConfiguration(SettingsStyle.None, Array.Empty<ReflectionSettingsConfig
                            >()));
                        if (i.ProjPath.Failed) return;

                        try
                        {
                            var result = await getSettingsStyle.Get(
                                i.ProjPath.Value,
                                directExe: false,
                                cancel: cancel,
                                build: needBuild);
                            logger.Information($"Settings type: {result}");
                            observer.OnNext(result);
                        }
                        catch (Exception ex)
                        {
                            logger.Error($"Error checking if patcher can open settings: {ex}");
                        }
                        observer.OnCompleted();
                    });
                })
                .Switch()
                .ToGuiProperty(this, nameof(SettingsConfiguration), new SettingsConfiguration(SettingsStyle.None, Array.Empty<ReflectionSettingsConfig>()));

            var windowPlacement = Inject.Container.GetInstance<IProvideWindowPlacement>();
            OpenSettingsCommand = NoggogCommand.CreateFromObject(
                objectSource: Observable.CombineLatest(
                        source.Select(x => x.ProjPath),
                        this.WhenAnyValue(x => x.SettingsConfiguration),
                        (Proj, Conf) => (Proj, Conf)),
                canExecute: x => x.Proj.Succeeded 
                    && (x.Conf.Style == SettingsStyle.Open || x.Conf.Style == SettingsStyle.Host),
                execute: async (o) =>
                {
                    if (o.Conf.Style == SettingsStyle.Open)
                    {
                        await openForSettings.Open(
                            o.Proj.Value,
                            directExe: false,
                            rect: windowPlacement.Rectangle,
                            cancel: CancellationToken.None,
                            release: ident.Release,
                            dataFolderPath: dataFolder.DataFolder,
                            loadOrder: loadOrder.LoadOrder.Items.Select<ReadOnlyModListingVM, IModListingGetter>(lvm => lvm));
                    }
                    else
                    {
                        await openSettingsHost.Open(
                            patcherName: parent.DisplayName,
                            path: o.Proj.Value,
                            rect: windowPlacement.Rectangle,
                            cancel: CancellationToken.None,
                            release: ident.Release,
                            dataFolderPath: dataFolder.DataFolder,
                            loadOrder: loadOrder.LoadOrder.Items.Select<ReadOnlyModListingVM, IModListingGetter>(lvm => lvm));
                    }
                },
                disposable: this.CompositeDisposable);

            _SettingsOpen = OpenSettingsCommand.IsExecuting
                .ToGuiProperty(this, nameof(SettingsOpen));

            _ReflectionSettings = Observable.CombineLatest(
                    this.WhenAnyValue(x => x.SettingsConfiguration),
                    source.Select(x => x.ProjPath),
                    (SettingsConfig, ProjPath) => (SettingsConfig, ProjPath))
                .Select(x =>
                {
                    if (x.ProjPath.Failed
                        || x.SettingsConfig.Style != SettingsStyle.SpecifiedClass
                        || x.SettingsConfig.Targets.Length == 0)
                    {
                        return default(AutogeneratedSettingsVM?);
                    }
                    return autoGenSettingsProvider.Get(x.SettingsConfig,
                        projPath: x.ProjPath.Value,
                        displayName: parent.DisplayName,
                        loadOrder: loadOrder.LoadOrder.Connect().Transform<ReadOnlyModListingVM, IModListingGetter>(x => x),
                        linkCache: linkCache.SimpleLinkCache);
                })
                .ToGuiProperty<AutogeneratedSettingsVM?>(this, nameof(ReflectionSettings), initialValue: null, deferSubscription: true);
        }

        public void Persist()
        {
            if (!_hasBeenRetrieved) return;
            ReflectionSettings?.Bundle?.Settings?.ForEach(vm =>
            {
                vm.Persist();
                if (!Repository.IsValid(vm.SettingsFolder))
                {
                    Repository.Init(vm.SettingsFolder);
                }
                using var repo = _RepoCheckouts.Get(vm.SettingsFolder);
                Commands.Stage(repo.Repository, vm.SettingsSubPath);
                var sig = new Signature("Synthesis", "someEmail@gmail.com", DateTimeOffset.Now);
                try
                {
                    repo.Repository.Commit("Settings changed", sig, sig);
                }
                catch (EmptyCommitException)
                {
                }
            });
        }

        public override void Dispose()
        {
            base.Dispose();
            if (_hasBeenRetrieved)
            {
                ReflectionSettings?.Dispose();
            }
        }
    }
}
