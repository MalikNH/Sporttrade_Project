using DynamicData;
using Mutagen.Bethesda.Plugins.Cache;
using Noggog;
using Noggog.WPF;
using ReactiveUI;
using ReactiveUI.Fody.Helpers;
using Synthesis.Bethesda;
using System;
using System.Linq;
using System.Reactive;
using System.Reactive.Linq;

namespace Mutagen.Bethesda.Synthesis.WPF
{
    public class AutogeneratedSettingsVM : ViewModel
    {
        private readonly ObservableAsPropertyHelper<bool> _SettingsLoading;
        public bool SettingsLoading => _SettingsLoading.Value;

        private readonly ObservableAsPropertyHelper<ErrorResponse> _Status;
        public ErrorResponse Error => _Status.Value;

        [Reactive]
        public ReflectionSettingsVM? SelectedSettings { get; set; }

        private readonly ObservableAsPropertyHelper<ReflectionSettingsBundleVM?> _Bundle;
        public ReflectionSettingsBundleVM? Bundle => _Bundle.Value;

        public AutogeneratedSettingsVM(
            SettingsConfiguration config,
            string projPath,
            string displayName,
            IObservable<IChangeSet<LoadOrderEntryVM>> loadOrder,
            IObservable<ILinkCache> linkCache,
            Action<string> log)
        {
            var targetSettingsVM = Observable.Return(Unit.Default)
                .ObserveOn(RxApp.TaskpoolScheduler)
                .Select(_ =>
                {
                    return Observable.Create<(bool Processing, GetResponse<ReflectionSettingsBundleVM> SettingsVM)>(async (observer, cancel) =>
                    {
                        try
                        {
                            observer.OnNext((true, GetResponse<ReflectionSettingsBundleVM>.Fail("Loading")));
                            var reflectionBundle = await ReflectionSettingsBundleVM.ExtractBundle(
                                projPath,
                                targets: config.Targets,
                                detectedLoadOrder: loadOrder,
                                linkCache: linkCache,
                                displayName: displayName,
                                log: log,
                                cancel: cancel);
                            if (reflectionBundle.Failed)
                            {
                                observer.OnNext((false, reflectionBundle));
                                return;
                            }
                            observer.OnNext((false, reflectionBundle.Value));
                        }
                        catch (Exception ex)
                        {
                            observer.OnNext((false, GetResponse<ReflectionSettingsBundleVM>.Fail(ex)));
                        }
                        observer.OnCompleted();
                    });
                })
                .Switch()
                .DisposePrevious()
                .Replay(1)
                .RefCount();

            _SettingsLoading = targetSettingsVM
                .Select(t => t.Processing)
                .ToGuiProperty(this, nameof(SettingsLoading), deferSubscription: true);

            _Bundle = targetSettingsVM
                .Select(x =>
                {
                    if (x.Processing || x.SettingsVM.Failed)
                    {
                        return new ReflectionSettingsBundleVM();
                    }
                    return x.SettingsVM.Value;
                })
                .ObserveOnGui()
                .Select(x =>
                {
                    SelectedSettings = x.Settings?.FirstOrDefault();
                    return x;
                })
                .DisposePrevious()
                .ToGuiProperty<ReflectionSettingsBundleVM?>(this, nameof(Bundle), initialValue: null, deferSubscription: true);

            _Status = targetSettingsVM
                .Select(x => (ErrorResponse)x.SettingsVM)
                .ToGuiProperty(this, nameof(Error), ErrorResponse.Success, deferSubscription: true);
        }
    }
}
