using System;
using Autofac;
using Noggog.Utility;
using Synthesis.Bethesda.Execution.DotNet;

namespace Synthesis.Bethesda.ImpactTester
{
    public class MainModule : Module
    {
        protected override void Load(ContainerBuilder builder)
        {
            builder.RegisterType<ProcessFactory>().As<IProcessFactory>();
            
            builder.RegisterAssemblyTypes(typeof(IBuild).Assembly)
                .InNamespaceOf<IBuild>()
                .AsImplementedInterfaces();
        }
    }
}