﻿using System.Reactive.Linq;
using Noggog.WPF;
using ReactiveUI;
using Synthesis.Bethesda.Execution.Patchers.Git;

namespace Synthesis.Bethesda.GUI.Services.Patchers.Git
{
    public interface INugetDiffProviderVm
    {
        NugetVersionDiff MutagenVersionDiff { get; }
        NugetVersionDiff SynthesisVersionDiff { get; }
    }

    public class NugetDiffProviderVm : ViewModel, INugetDiffProviderVm
    {
        private readonly ObservableAsPropertyHelper<NugetVersionDiff> _MutagenVersionDiff;
        public NugetVersionDiff MutagenVersionDiff => _MutagenVersionDiff.Value;

        private readonly ObservableAsPropertyHelper<NugetVersionDiff> _SynthesisVersionDiff;
        public NugetVersionDiff SynthesisVersionDiff => _SynthesisVersionDiff.Value;

        public NugetDiffProviderVm(
            IRunnableStateProvider runnableStateProvider,
            IGitNugetTargetingVm nugetTargetingVm)
        {
            var cleanState = runnableStateProvider.State
                .Select(x => x.Item ?? default(RunnerRepoInfo?));

            _MutagenVersionDiff = Observable.CombineLatest(
                    cleanState.Select(x => x?.ListedMutagenVersion),
                    nugetTargetingVm.ActiveNugetVersion.Select(x => x.Value?.MutagenVersion),
                    (matchVersion, selVersion) => new NugetVersionDiff(matchVersion, selVersion))
                .ToGuiProperty(this, nameof(MutagenVersionDiff), new NugetVersionDiff(null, null));

            _SynthesisVersionDiff = Observable.CombineLatest(
                    cleanState.Select(x => x?.ListedSynthesisVersion),
                    nugetTargetingVm.ActiveNugetVersion.Select(x => x.Value?.SynthesisVersion),
                    (matchVersion, selVersion) => new NugetVersionDiff(matchVersion, selVersion))
                .ToGuiProperty(this, nameof(SynthesisVersionDiff), new NugetVersionDiff(null, null));
        }
    }
}