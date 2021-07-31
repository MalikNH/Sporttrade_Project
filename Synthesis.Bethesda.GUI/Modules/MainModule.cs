﻿using System.IO.Abstractions;
using Autofac;
using Mutagen.Bethesda.Autofac;
using Mutagen.Bethesda.Synthesis.Projects;
using Mutagen.Bethesda.Synthesis.Versioning;
using Mutagen.Bethesda.Synthesis.WPF;
using Noggog.Autofac;
using Noggog.Autofac.Modules;
using Noggog.Reactive;
using Noggog.WPF;
using Serilog;
using Synthesis.Bethesda.Execution.CLI;
using Synthesis.Bethesda.Execution.DotNet;
using Synthesis.Bethesda.Execution.GitRepository;
using Synthesis.Bethesda.Execution.Patchers.Running;
using Synthesis.Bethesda.Execution.Pathing;
using Synthesis.Bethesda.Execution.Profile;
using Synthesis.Bethesda.Execution.Running;
using Synthesis.Bethesda.Execution.Utility;
using Synthesis.Bethesda.Execution.Versioning;
using Synthesis.Bethesda.GUI.Services.Main;
using Synthesis.Bethesda.GUI.Services.Startup;
using Synthesis.Bethesda.GUI.Settings;
using Synthesis.Bethesda.GUI.ViewModels.Profiles;
using Synthesis.Bethesda.GUI.ViewModels.Profiles.Running;
using Synthesis.Bethesda.GUI.ViewModels.Top;

namespace Synthesis.Bethesda.GUI.Modules
{
    public class MainModule : Autofac.Module
    {
        public const string ProfileNickname = "Profile";
        public const string PatcherNickname = "Patcher";
        public const string RunNickname = "Run";
        
        protected override void Load(ContainerBuilder builder)
        {
            TopLevel(builder);

            ProfileLevel(builder);
        }

        private static void TopLevel(ContainerBuilder builder)
        {
            builder.RegisterType<FileSystem>().As<IFileSystem>()
                .SingleInstance();
            builder.RegisterModule<NoggogModule>();
            builder.RegisterInstance(Log.Logger).As<ILogger>();

            // Noggog
            builder.RegisterType<WatchFile>().As<IWatchFile>()
                .SingleInstance();
            builder.RegisterType<WatchDirectory>().As<IWatchDirectory>()
                .SingleInstance();
            builder.RegisterType<SchedulerProvider>().As<ISchedulerProvider>()
                .SingleInstance();

            // Mutagen
            builder.RegisterModule<MutagenModule>();

            // Mutagen.Bethesda.Synthesis
            builder.RegisterAssemblyTypes(typeof(IProvideCurrentVersions).Assembly)
                .InNamespacesOf(
                    typeof(IProvideCurrentVersions),
                    typeof(ICreateProject))
                .AsMatchingInterface()
                .SingleInstance();

            // Mutagen.Bethesda.Synthesis.WPF
            builder.RegisterAssemblyTypes(typeof(IProvideAutogeneratedSettings).Assembly)
                .InNamespaceOf<IProvideAutogeneratedSettings>()
                .AsMatchingInterface();

            // Synthesis.Bethesda.Execution
            builder.RegisterAssemblyTypes(typeof(ICheckOrCloneRepo).Assembly)
                .InNamespacesOf(
                    typeof(ICheckOrCloneRepo),
                    typeof(IQueryNewestLibraryVersions),
                    typeof(IProcessRunner),
                    typeof(IWorkingDirectorySubPaths),
                    typeof(IPatcherRun),
                    typeof(IInstalledSdkFollower),
                    typeof(IExecuteRunnabilityCheck))
                .Except<ProvideWorkingDirectory>()
                .AsMatchingInterface();

            // Top Level
            builder.RegisterAssemblyTypes(typeof(INavigateTo).Assembly)
                .InNamespacesOf(
                    typeof(MainVm),
                    typeof(INavigateTo),
                    typeof(IStartup),
                    typeof(ISynthesisGuiSettings))
                .AsImplementedInterfaces()
                .AsSelf()
                .SingleInstance();
        }

        private static void ProfileLevel(ContainerBuilder builder)
        {
            builder.RegisterAssemblyTypes(typeof(ProfileVm).Assembly)
                .InNamespacesOf(
                    typeof(ProfileVm))
                .NotInNamespacesOf(typeof(PatchersRunVm))
                .InstancePerMatchingLifetimeScope(ProfileNickname)
                .AsImplementedInterfaces()
                .AsSelf();
            
            builder.RegisterAssemblyTypes(typeof(ProfileVm).Assembly)
                .InNamespaceOf<PatchersRunVm>()
                .AsImplementedInterfaces()
                .AsSelf();

            // Execution lib
            builder.RegisterAssemblyTypes(typeof(IRunner).Assembly)
                .InNamespacesOf(
                    typeof(IProfileDirectories))
                .InstancePerMatchingLifetimeScope(ProfileNickname)
                .AsMatchingInterface();
            builder.RegisterAssemblyTypes(typeof(IRunner).Assembly)
                .InNamespacesOf(
                    typeof(IRunner))
                .AsMatchingInterface();
        }
    }
}