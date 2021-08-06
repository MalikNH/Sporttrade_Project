﻿namespace Synthesis.Bethesda.Execution.Patchers.Git.Registry
{
    public interface IRemoteRegistryUrlProvider
    {
        string Url { get; }
    }

    public class RemoteRegistryUrlProvider : IRemoteRegistryUrlProvider
    {
        public string Url => "https://github.com/Mutagen-Modding/Synthesis.Registry";
    }
}