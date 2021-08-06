﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Synthesis.Bethesda.Execution.Patchers.Running;
using Synthesis.Bethesda.Execution.Reporters;

namespace Synthesis.Bethesda.Execution.Running.Runner
{
    public interface IPrepPatchersForRun
    {
        Task<Exception?>[] PrepPatchers(
            IEnumerable<(int Key, IPatcherRun Run)> patchers,
            CancellationToken cancellation);
    }

    public class PrepPatchersForRun : IPrepPatchersForRun
    {
        public IRunReporter Reporter { get; }

        public PrepPatchersForRun(IRunReporter reporter)
        {
            Reporter = reporter;
        }
        
        public Task<Exception?>[] PrepPatchers(
            IEnumerable<(int Key, IPatcherRun Run)> patchers,
            CancellationToken cancellation)
        {
            return patchers.Select(patcher => Task.Run(async () =>
            {
                try
                {
                    await patcher.Run.Prep(cancellation);
                }
                catch (TaskCanceledException)
                {
                }
                catch (Exception ex)
                {
                    Reporter.ReportPrepProblem(patcher.Key, patcher.Run.Name, ex);
                    return ex;
                }
                return default(Exception?);
            })).ToArray();
        }
    }
}