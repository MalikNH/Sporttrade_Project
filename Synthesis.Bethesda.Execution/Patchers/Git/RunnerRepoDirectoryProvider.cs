﻿using Noggog;
using Synthesis.Bethesda.Execution.Pathing;
using Synthesis.Bethesda.Execution.Profile;

namespace Synthesis.Bethesda.Execution.Patchers.Git
{
    public interface IRunnerRepoDirectoryProvider
    {
        DirectoryPath Path { get; }
    }

    public class RunnerRepoDirectoryProvider : IRunnerRepoDirectoryProvider
    {
        private readonly IBaseRepoDirectoryProvider _baseRepoDir;

        public DirectoryPath Path => System.IO.Path.Combine(_baseRepoDir.Path, "Runner");
        
        public RunnerRepoDirectoryProvider(IBaseRepoDirectoryProvider baseRepoDir)
        {
            _baseRepoDir = baseRepoDir;
        }
    }
}