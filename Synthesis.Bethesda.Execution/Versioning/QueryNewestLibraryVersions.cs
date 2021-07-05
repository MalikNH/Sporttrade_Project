﻿using System;
using System.IO;
using System.IO.Abstractions;
using System.Threading;
using System.Threading.Tasks;
using Mutagen.Bethesda;
using Mutagen.Bethesda.Synthesis;
using Mutagen.Bethesda.Synthesis.Versioning;
using Noggog;
using Serilog;
using Synthesis.Bethesda.Execution.Pathing;

namespace Synthesis.Bethesda.Execution.Versioning
{
    public interface IQueryNewestLibraryVersions
    {
        string PrepLatestVersionProject();
        Task<(string? MutagenVersion, string? SynthesisVersion)> GetLatestVersions(bool includePrerelease, string projPath);
    }

    public class QueryNewestLibraryVersions : IQueryNewestLibraryVersions
    {
        private readonly IFileSystem _FileSystem;
        private readonly ILogger _Logger;
        private readonly IProvideWorkingDirectory _Paths;
        private readonly IProvideCurrentVersions _CurrentVersions;
        private readonly IQueryLibraryVersions _QueryLibraryVersions;

        public QueryNewestLibraryVersions(
            IFileSystem fileSystem,
            ILogger logger,
            IProvideWorkingDirectory paths,
            IProvideCurrentVersions currentVersions,
            IQueryLibraryVersions queryLibraryVersions)
        {
            _FileSystem = fileSystem;
            _Logger = logger;
            _Paths = paths;
            _CurrentVersions = currentVersions;
            _QueryLibraryVersions = queryLibraryVersions;
        }
        
        public string PrepLatestVersionProject()
        {
            var bootstrapProjectDir = new DirectoryPath(Path.Combine(_Paths.WorkingDirectory, "VersionQuery"));
            _FileSystem.Directory.DeleteEntireFolder(bootstrapProjectDir);
            _FileSystem.Directory.CreateDirectory(bootstrapProjectDir);
            var slnPath = Path.Combine(bootstrapProjectDir.Path, "VersionQuery.sln");
            SolutionInitialization.CreateSolutionFile(_FileSystem, slnPath);
            var projPath = Path.Combine(bootstrapProjectDir.Path, "VersionQuery.csproj");
            SolutionInitialization.CreateProject(_FileSystem, projPath, GameCategory.Skyrim, insertOldVersion: true);
            SolutionInitialization.AddProjectToSolution(_FileSystem, slnPath, projPath);
            return projPath;
        }

        public async Task<(string? MutagenVersion, string? SynthesisVersion)> GetLatestVersions(bool includePrerelease, string projPath)
        {
            try
            {
                var ret = await _QueryLibraryVersions.Query(projPath, current: false, includePrerelease: includePrerelease, CancellationToken.None);
                _Logger.Information($"Latest published {(includePrerelease ? " prerelease" : null)} library versions:");
                _Logger.Information($"  Mutagen: {ret.MutagenVersion}");
                _Logger.Information($"  Synthesis: {ret.SynthesisVersion}");
                return (ret.MutagenVersion ?? _CurrentVersions.MutagenVersion, ret.SynthesisVersion ?? _CurrentVersions.SynthesisVersion);
            }
            catch (Exception ex)
            {
                _Logger.Error(ex, "Error querying for latest nuget versions");
                return (null, null);
            }
        }
    }
}