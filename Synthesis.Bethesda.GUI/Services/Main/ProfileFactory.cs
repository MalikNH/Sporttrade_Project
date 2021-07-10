﻿using System;
using System.Linq;
using System.Reactive.Disposables;
using Autofac;
using DynamicData;
using Mutagen.Bethesda;
using Mutagen.Bethesda.Environments.DI;
using Serilog;
using Synthesis.Bethesda.Execution.Settings;
using Synthesis.Bethesda.GUI.DI;
using Synthesis.Bethesda.GUI.ViewModels.Patchers;
using Synthesis.Bethesda.GUI.ViewModels.Patchers.Solution;

namespace Synthesis.Bethesda.GUI.Services.Main
{
    public interface IProfileFactory
    {
        ProfileVm Get(ISynthesisProfile settings);
        ProfileVm Get(GameRelease release, string id, string nickname);
    }

    public class ProfileFactory : IProfileFactory
    {
        private readonly ILifetimeScope _Scope;
        private readonly ILogger _Logger;

        public ProfileFactory(
            ILifetimeScope scope,
            ILogger logger)
        {
            _Scope = scope;
            _Logger = logger;
        }
        
        public ProfileVm Get(ISynthesisProfile settings)
        {
            _Logger.Information("Loading {Release} Profile {Nickname} with ID {ID}", settings.TargetRelease, settings.Nickname, settings.ID);
            var scope = _Scope.BeginLifetimeScope(
                ProfileModule.ScopeNickname, 
                cfg =>
                {
                    cfg.RegisterInstance(new ProfileIdentifier()
                        {
                            ID = settings.ID,
                            Release = settings.TargetRelease,
                            Nickname = settings.Nickname
                        })
                        .As<IProfileIdentifier>()
                        .As<IGameReleaseContext>();
                });
            var profile = scope.Resolve<ProfileVm>();
            
            scope.DisposeWith(profile);
            profile.Versioning.MutagenVersioning = settings.MutagenVersioning;
            profile.Versioning.ManualMutagenVersion = settings.MutagenManualVersion;
            profile.Versioning.SynthesisVersioning = settings.SynthesisVersioning;
            profile.Versioning.ManualSynthesisVersion = settings.SynthesisManualVersion;
            profile.DataFolderOverride.DataPathOverride = settings.DataPathOverride;
            profile.ConsiderPrereleaseNugets = settings.ConsiderPrereleaseNugets;
            profile.LockSetting.Lock = settings.LockToCurrentVersioning;
            profile.SelectedPersistenceMode = settings.Persistence;

            var gitFactory = scope.Resolve<GitPatcherVm.Factory>();
            var slnFactory = scope.Resolve<SolutionPatcherVm.Factory>();
            var cliFactory = scope.Resolve<CliPatcherVm.Factory>();
            profile.Patchers.AddRange(settings.Patchers.Select(x =>
            {
                PatcherVm ret = x switch
                {
                    GithubPatcherSettings git => gitFactory(git),
                    SolutionPatcherSettings soln => slnFactory(soln),
                    CliPatcherSettings cli => cliFactory(cli),
                    _ => throw new NotImplementedException(),
                };
                return ret;
            }));
            return profile;
        }

        public ProfileVm Get(GameRelease release, string id, string nickname)
        {
            _Logger.Information("Creating {Release} Profile {Nickname} with ID {ID}", release, nickname, id);
            var scope = _Scope.BeginLifetimeScope(
                ProfileModule.ScopeNickname,
                cfg =>
                {
                    cfg.RegisterInstance(new ProfileIdentifier()
                        {
                            ID = id,
                            Release = release,
                            Nickname = nickname
                        })
                        .As<IProfileIdentifier>()
                        .As<IGameReleaseContext>();
                });
            var ret = scope.Resolve<ProfileVm>();
            scope.DisposeWith(ret);
            return ret;
        }
    }
}