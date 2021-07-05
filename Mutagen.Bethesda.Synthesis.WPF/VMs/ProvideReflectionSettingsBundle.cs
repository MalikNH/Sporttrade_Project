﻿using System;
using System.IO.Abstractions;
using Path = System.IO.Path;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using DynamicData;
using Mutagen.Bethesda.Plugins.Cache;
using Mutagen.Bethesda.Plugins.Order;
using Mutagen.Bethesda.WPF.Reflection;
using Noggog;
using Serilog;
using Synthesis.Bethesda.DTO;
using Synthesis.Bethesda.Execution;
using Synthesis.Bethesda.Execution.Pathing;

namespace Mutagen.Bethesda.Synthesis.WPF
{
    public interface IProvideReflectionSettingsBundle
    {
        Task<GetResponse<ReflectionSettingsBundleVM>> ExtractBundle(
            string projPath,
            string displayName,
            ReflectionSettingsConfig[] targets,
            IObservable<IChangeSet<IModListingGetter>> detectedLoadOrder,
            IObservable<ILinkCache?> linkCache,
            CancellationToken cancel);
    }

    public class ProvideReflectionSettingsBundle : IProvideReflectionSettingsBundle
    {
        private readonly ILogger _Logger;
        private readonly IWorkingDirectorySubPaths _Paths;
        private readonly IFileSystem _FileSystem;
        private readonly IExtractInfoFromProject _Extract;

        public ProvideReflectionSettingsBundle(
            ILogger logger,
            IWorkingDirectorySubPaths paths,
            IFileSystem fileSystem,
            IExtractInfoFromProject extract)
        {
            _Logger = logger;
            _Paths = paths;
            _FileSystem = fileSystem;
            _Extract = extract;
        }
        
        public async Task<GetResponse<ReflectionSettingsBundleVM>> ExtractBundle(
            string projPath,
            string displayName,
            ReflectionSettingsConfig[] targets,
            IObservable<IChangeSet<IModListingGetter>> detectedLoadOrder,
            IObservable<ILinkCache?> linkCache,
            CancellationToken cancel)
        {
            var vms = await _Extract.Extract<ReflectionSettingsVM[]>(
                projPath: projPath,
                cancel: cancel,
                getter: (assemb) =>
                {
                    return targets
                        .Select((s, index) =>
                        {
                            try
                            {
                                var t = assemb.GetType(s.TypeName);
                                if (t == null) return null;
                                return new ReflectionSettingsVM(
                                    ReflectionSettingsParameters.FromType(
                                        detectedLoadOrder,
                                        linkCache,
                                        t,
                                        Activator.CreateInstance(t)),
                                    nickname: targets[index].Nickname,
                                    settingsFolder: Path.Combine(_Paths.TypicalExtraData, displayName),
                                    settingsSubPath: targets[index].Path,
                                    logger: _Logger,
                                    fileSystem: _FileSystem);
                            }
                            catch (Exception ex)
                            {
                                _Logger.Error($"Error creating reflected settings:", ex);
                                throw new ArgumentException($"Error creating reflected settings: {ex.Message}");
                            }
                        })
                        .NotNull()
                        .ToArray();
                });
            if (vms.Failed)
            {
                return vms.BubbleFailure<ReflectionSettingsBundleVM>();
            }
            await Task.WhenAll(vms.Value.Item.Select(vm => vm.Import(cancel)));
            return new ReflectionSettingsBundleVM(vms.Value.Item, vms.Value.Temp, _Logger);
        }
    }
}