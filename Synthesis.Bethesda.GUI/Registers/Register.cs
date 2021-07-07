﻿using System.IO.Abstractions;
using Mutagen.Bethesda.Environments.DI;
using Mutagen.Bethesda.StructureMap;
using Mutagen.Bethesda.Synthesis.Projects;
using Mutagen.Bethesda.Synthesis.Versioning;
using Mutagen.Bethesda.Synthesis.WPF;
using Noggog;
using Noggog.Reactive;
using Noggog.WPF;
using Serilog;
using StructureMap;
using Synthesis.Bethesda.Execution.CLI;
using Synthesis.Bethesda.Execution.DotNet;
using Synthesis.Bethesda.Execution.GitRespository;
using Synthesis.Bethesda.Execution.Patchers.Git;
using Synthesis.Bethesda.Execution.Pathing;
using Synthesis.Bethesda.Execution.Running;
using Synthesis.Bethesda.Execution.Versioning;
using Synthesis.Bethesda.GUI.Services.Main;
using Synthesis.Bethesda.GUI.Services.Startup;
using Synthesis.Bethesda.GUI.Settings;

namespace Synthesis.Bethesda.GUI.Registers
{
    public class Register : Registry
    {
        public Register()
        {
            RegisterMutagenSynthesis();
            RegisterCurrentLib();
            RegisterWpfLib();
            RegisterExecutionLib();
            RegisterOther();
            RegisterMutagen();
            RegisterCSharpExt();
        }

        private void RegisterMutagenSynthesis()
        {
            Scan(s =>
            {
                s.AssemblyContainingType<IProvideCurrentVersions>(); 
                s.IncludeNamespaceContainingType<IProvideCurrentVersions>();
                s.IncludeNamespaceContainingType<ICreateProject>();
                s.Convention<SingletonConvention>();
            });
        }

        private void RegisterCurrentLib()
        {
            ForConcreteType<MainVM>().Configure.Singleton();
            ForConcreteType<ConfigurationVM>().Configure.Singleton();
            ForConcreteType<PatcherInitializationVM>().Configure.Singleton();
            ForSingletonOf<ILogger>().Use(Log.Logger);
            ForSingletonOf<ISettingsSingleton>().Use<SettingsSingleton>();
            ForSingletonOf<IShowHelpSetting>().Use<ShowHelpSetting>();
            ForSingletonOf<IConsiderPrereleasePreference>().Use<ConsiderPrereleasePreference>();
            ForSingletonOf<IConfirmationPanelControllerVm>().Use<ConfirmationPanelControllerVm>();
            ForSingletonOf<ISelectedProfileControllerVm>().Use<SelectedProfileControllerVm>();
            ForSingletonOf<IActivePanelControllerVm>().Use<ActivePanelControllerVm>();
            ForConcreteType<RetrieveSaveSettings>().Configure.Singleton();
            Forward<RetrieveSaveSettings, IRetrieveSaveSettings>();
            Forward<RetrieveSaveSettings, ISaveSignal>();
            
            Scan(s =>
            {
                s.AssemblyContainingType<IEnvironmentErrorVM>();
                s.AddAllTypesOf<IEnvironmentErrorVM>();
            });
            
            Scan(s =>
            {
                s.AssemblyContainingType<INavigateTo>();
                s.IncludeNamespaceContainingType<INavigateTo>();
                s.WithDefaultConventions();
            });
            
            Scan(s =>
            {
                s.AssemblyContainingType<IStartup>();
                s.IncludeNamespaceContainingType<IStartup>();
                s.Convention<SingletonConvention>();
            });
            
            For<ILockToCurrentVersioning>().Use<LockToCurrentVersioning>();
            For<IProfileDisplayControllerVm>().Use<ProfileDisplayControllerVm>();
            For<IEnvironmentErrorsVM>().Use<EnvironmentErrorsVM>();
            
            // Overrides
            ForSingletonOf<IProvideWorkingDirectory>().Use<WorkingDirectoryOverride>();
        }

        private void RegisterWpfLib()
        {
            Scan(s =>
            {
                s.AssemblyContainingType<IProvideAutogeneratedSettings>();
                s.IncludeNamespaceContainingType<IProvideAutogeneratedSettings>();
                s.WithDefaultConventions();
            });
        }

        private void RegisterExecutionLib()
        {
            Scan(s =>
            {
                s.AssemblyContainingType<ICheckOrCloneRepo>();
                s.IncludeNamespaceContainingType<ICheckOrCloneRepo>();
                s.IncludeNamespaceContainingType<IQueryNewestLibraryVersions>();
                s.IncludeNamespaceContainingType<IProvideInstalledSdk>();
                s.Convention<SingletonConvention>();
            });
            Scan(s =>
            {
                s.AssemblyContainingType<IWorkingDirectorySubPaths>();
                s.IncludeNamespaceContainingType<IWorkingDirectorySubPaths>();
                s.ExcludeType<ProvideWorkingDirectory>();
                s.IncludeNamespaceContainingType<IRunner>();
                s.IncludeNamespaceContainingType<ICheckoutRunnerRepository>();
                s.IncludeNamespaceContainingType<ICheckRunnability>();
                s.WithDefaultConventions();
            });
        }

        private void RegisterOther()
        {
            For<IFileSystem>().Use<FileSystem>();
        }

        private void RegisterMutagen()
        {
            IncludeRegistry<MutagenRegister>();
            For<IGameReleaseContext>().UseIfNone<GameReleasePlaceholder>();
        }

        private void RegisterCSharpExt()
        {
            For<ISchedulerProvider>().Use<SchedulerProvider>();
            Scan(s =>
            {
                s.AssemblyContainingType<IWatchFile>();
                s.AssemblyContainingType<ISchedulerProvider>();
                s.ExcludeType<BinaryReadStream>();
                s.ExcludeType<BinaryWriteStream>();
                s.WithDefaultConventions();
            });
        }
    }
}