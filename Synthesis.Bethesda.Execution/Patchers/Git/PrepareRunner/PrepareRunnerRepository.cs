using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Noggog;
using Serilog;
using Synthesis.Bethesda.Execution.GitRepository;
using Synthesis.Bethesda.Execution.Patchers.Git.ModifyProject;
using Synthesis.Bethesda.Execution.Patchers.Solution;

namespace Synthesis.Bethesda.Execution.Patchers.Git.PrepareRunner
{
    public interface IPrepareRunnerRepository
    {
        Task<ConfigurationState<RunnerRepoInfo>> Checkout(
            CheckoutInput checkoutInput,
            CancellationToken cancel);
    }

    public class PrepareRunnerRepository : IPrepareRunnerRepository
    {
        private readonly ILogger _logger;
        public IResetToTarget ResetToTarget { get; }
        public ISolutionFileLocator SolutionFileLocator { get; }
        public IFullProjectPathRetriever FullProjectPathRetriever { get; }
        public IModifyRunnerProjects ModifyRunnerProjects { get; }
        public IRunnerRepoDirectoryProvider RunnerRepoDirectoryProvider { get; }
        public IProvideRepositoryCheckouts RepoCheckouts { get; }

        public PrepareRunnerRepository(
            ILogger logger,
            ISolutionFileLocator solutionFileLocator,
            IFullProjectPathRetriever fullProjectPathRetriever,
            IModifyRunnerProjects modifyRunnerProjects,
            IResetToTarget resetToTarget,
            IRunnerRepoDirectoryProvider runnerRepoDirectoryProvider,
            IProvideRepositoryCheckouts repoCheckouts)
        {
            _logger = logger;
            ResetToTarget = resetToTarget;
            SolutionFileLocator = solutionFileLocator;
            FullProjectPathRetriever = fullProjectPathRetriever;
            ModifyRunnerProjects = modifyRunnerProjects;
            RunnerRepoDirectoryProvider = runnerRepoDirectoryProvider;
            RepoCheckouts = repoCheckouts;
        }
        
        public async Task<ConfigurationState<RunnerRepoInfo>> Checkout(
            CheckoutInput checkoutInput,
            CancellationToken cancel)
        {
            try
            {
                cancel.ThrowIfCancellationRequested();

                _logger.Information("Targeting {PatcherVersioning}", checkoutInput.PatcherVersioning);

                using var repoCheckout = RepoCheckouts.Get(RunnerRepoDirectoryProvider.Path);

                var target = ResetToTarget.Reset(repoCheckout.Repository, checkoutInput.PatcherVersioning, cancel);
                if (target.Failed) return target.BubbleFailure<RunnerRepoInfo>();

                cancel.ThrowIfCancellationRequested();

                checkoutInput.LibraryNugets.Log(_logger);
                
                var slnPath = SolutionFileLocator.GetPath(RunnerRepoDirectoryProvider.Path);
                if (slnPath == null) return GetResponse<RunnerRepoInfo>.Fail("Could not locate solution to run.");

                var foundProjSubPath = FullProjectPathRetriever.Get(slnPath.Value, checkoutInput.Proj);
                if (foundProjSubPath == null) return GetResponse<RunnerRepoInfo>.Fail($"Could not locate target project file: {checkoutInput.Proj}.");

                cancel.ThrowIfCancellationRequested();
                
                ModifyRunnerProjects.Modify(
                    slnPath.Value,
                    drivingProjSubPath: foundProjSubPath.SubPath,
                    versions: checkoutInput.LibraryNugets.ReturnIfMatch(new NugetVersionPair(null, null)),
                    listedVersions: out var listedVersions);

                var runInfo = new RunnerRepoInfo(
                    SolutionPath: slnPath,
                    ProjPath: foundProjSubPath.FullPath,
                    Target: target.Value.Target,
                    CommitMessage: target.Value.CommitMessage,
                    CommitDate: target.Value.CommitDate,
                    ListedVersions: listedVersions,
                    TargetVersions: checkoutInput.LibraryNugets.ReturnIfMatch(listedVersions));

                return GetResponse<RunnerRepoInfo>.Succeed(runInfo);
            }
            catch (OperationCanceledException)
            {
                throw;
            }
            catch (Exception ex)
            {
                return GetResponse<RunnerRepoInfo>.Fail(ex);
            }
        }
    }
}