﻿using Mutagen.Bethesda.Environments.DI;
using Mutagen.Bethesda.Plugins.Order.DI;
using StructureMap;
using StructureMap.AutoFactory;
using Synthesis.Bethesda.GUI.Profiles.Plugins;
using Synthesis.Bethesda.GUI.Services.Profile;

namespace Synthesis.Bethesda.GUI.Registers
{
    public class ProfileRegister : Registry
    {
        public ProfileRegister()
        {
            ForConcreteType<ProfilePatchersList>().Configure.Singleton();
            Forward<ProfilePatchersList, IRemovePatcherFromProfile>();
            Forward<ProfilePatchersList, IProfilePatchersList>();
            ForConcreteType<ProfileLoadOrder>().Configure.Singleton();
            Forward<ProfileLoadOrder, IProfileLoadOrder>();
            Forward<ProfileLoadOrder, ILoadOrderListingsProvider>();
            ForSingletonOf<IProfileDirectories>().Use<ProfileDirectories>();
            ForConcreteType<ProfileDataFolder>().Configure.Singleton();
            Forward<ProfileDataFolder, IProfileDataFolder>();
            Forward<ProfileDataFolder, IDataDirectoryProvider>();
            ForSingletonOf<IProfileVersioning>().Use<ProfileVersioning>();
            ForSingletonOf<IProfileSimpleLinkCache>().Use<ProfileSimpleLinkCache>();
            For<IPatchersRunFactory>().CreateFactory();
            For<IPatcherSettingsVmFactory>().CreateFactory();
            For<PatchersRunVM>();
            For<PatcherSettingsVM>();
            
            Scan(s =>
            {
                s.AssemblyContainingType<IPatcherFactory>();
                s.IncludeNamespaceContainingType<IPatcherFactory>();
                s.WithDefaultConventions();
            });
        }
    }
}