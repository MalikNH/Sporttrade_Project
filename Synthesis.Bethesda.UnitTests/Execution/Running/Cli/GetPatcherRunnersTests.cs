﻿using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FluentAssertions;
using Noggog.NSubstitute;
using NSubstitute;
using Synthesis.Bethesda.Execution.Patchers.Running;
using Synthesis.Bethesda.Execution.Running.Cli;
using Synthesis.Bethesda.Execution.Settings;
using Synthesis.Bethesda.UnitTests.AutoData;
using Xunit;

namespace Synthesis.Bethesda.UnitTests.Execution.Running.Cli
{
    public class GetPatcherRunnersTests
    {
        [Theory, SynthAutoData]
        public void PassesPatchersToRunnerFactory(
            List<PatcherSettings> settings,
            GetPatcherRunners sut)
        {
            settings.ForEach(x => x.On = true);
            sut.Profile.Patchers.Returns(settings);
            sut.Get();
            foreach (var setting in settings)
            {
                sut.PatcherSettingsToRunnerFactory.Received(1).Convert(setting);
            }
        }
        
        [Theory, SynthAutoData]
        public void OnlyIncludesOnPatchers(
            List<PatcherSettings> settings,
            GetPatcherRunners sut)
        {
            settings.ForEach(x => x.On = true);
            settings[^1].On = false;
            sut.Profile.Patchers.Returns(settings);
            sut.Get();
            foreach (var setting in settings.Where(x => x.On))
            {
                sut.PatcherSettingsToRunnerFactory.Received(1).Convert(setting);
            }
        }
        
        [Theory, SynthAutoData]
        public void ReturnsPatchers(
            List<PatcherSettings> settings,
            IPatcherRun[] patcherRuns,
            GetPatcherRunners sut)
        {
            settings.ForEach(x => x.On = true);
            sut.Profile.Patchers.Returns(settings);
            sut.PatcherSettingsToRunnerFactory.Convert(default!)
                .ReturnsSeriallyForAnyArgs(patcherRuns);
            sut.Get()
                .Select(x => x.Run)
                .Should().Equal(patcherRuns);
        }
        
        [Theory, SynthAutoData]
        public void ReturnsNonZeroKeys(
            List<PatcherSettings> settings,
            IPatcherRun[] patcherRuns,
            GetPatcherRunners sut)
        {
            settings.ForEach(x => x.On = true);
            sut.Profile.Patchers.Returns(settings);
            sut.PatcherSettingsToRunnerFactory.Convert(default!)
                .ReturnsSeriallyForAnyArgs(patcherRuns);
            var expectedKeys = patcherRuns.Select((_, i) => i + 1);
            sut.Get()
                .Select(x => x.Key)
                .Should().Equal(expectedKeys);
        }
    }
}