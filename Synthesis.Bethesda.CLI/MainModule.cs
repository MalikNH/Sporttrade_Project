using System.IO.Abstractions;
using System.Threading;
using Autofac;
using Mutagen.Bethesda.Autofac;
using Noggog.Autofac;
using Noggog.Autofac.Modules;
using Serilog;
using Synthesis.Bethesda.CLI.Services;
using Synthesis.Bethesda.Execution.Commands;
using Synthesis.Bethesda.Execution.Reporters;

namespace Synthesis.Bethesda.CLI
{
    public class MainModule : Module
    {
        public RunPatcherPipelineInstructions Settings { get; }

        public MainModule(RunPatcherPipelineInstructions settings)
        {
            Settings = settings;
        }
        
        protected override void Load(ContainerBuilder builder)
        {
            builder.RegisterModule<MutagenModule>();
            builder.RegisterType<FileSystem>().As<IFileSystem>();
            builder.RegisterModule<NoggogModule>();
            builder.RegisterInstance(Log.Logger).As<ILogger>();
            builder.RegisterModule<Execution.Modules.MainModule>();
            builder.RegisterModule<Execution.Modules.ProfileModule>();
            
            builder.Register(_ => CancellationToken.None).AsSelf();
            builder.RegisterInstance(new ConsoleReporter()).As<IRunReporter>();
            
            builder.RegisterAssemblyTypes(typeof(ProfileLoadOrderProvider).Assembly)
                .InNamespacesOf(
                    typeof(ProfileLoadOrderProvider))
                .AsMatchingInterface();
            
            // Settings
            builder.RegisterInstance(Settings)
                .AsSelf()
                .AsImplementedInterfaces();
        }
    }
}