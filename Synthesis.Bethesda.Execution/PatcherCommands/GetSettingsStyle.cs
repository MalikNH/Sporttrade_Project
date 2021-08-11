﻿using System;
using System.Threading;
using System.Threading.Tasks;
using Synthesis.Bethesda.DTO;
using Synthesis.Bethesda.Execution.Settings.Json;
using Synthesis.Bethesda.Execution.Utility;

namespace Synthesis.Bethesda.Execution.PatcherCommands
{
    public interface IGetSettingsStyle
    {
        Task<SettingsConfiguration> Get(
            string path,
            bool directExe,
            CancellationToken cancel,
            bool build);
    }

    public class GetSettingsStyle : IGetSettingsStyle
    {
        public ILinesToReflectionConfigsParser LinesToConfigsParser { get; }
        public IProcessRunner ProcessRunner { get; }
        public IRunProcessStartInfoProvider GetRunProcessStartInfoProvider { get; }

        public GetSettingsStyle(
            IProcessRunner processRunner,
            ILinesToReflectionConfigsParser linesToConfigsParser,
            IRunProcessStartInfoProvider getRunProcessStartInfoProvider)
        {
            LinesToConfigsParser = linesToConfigsParser;
            ProcessRunner = processRunner;
            GetRunProcessStartInfoProvider = getRunProcessStartInfoProvider;
        }
        
        public async Task<SettingsConfiguration> Get(
            string path,
            bool directExe,
            CancellationToken cancel,
            bool build)
        {
            var result = await ProcessRunner.RunAndCapture(
                GetRunProcessStartInfoProvider.GetStart(path, directExe, new Synthesis.Bethesda.SettingsQuery(), build: build),
                cancel: cancel);
            
            switch ((Codes)result.Result)
            {
                case Codes.OpensForSettings:
                    return new SettingsConfiguration(SettingsStyle.Open, Array.Empty<ReflectionSettingsConfig>());
                case Codes.AutogeneratedSettingsClass:
                    return new SettingsConfiguration(
                        SettingsStyle.SpecifiedClass,
                        LinesToConfigsParser.Parse(result.Out).Configs);
                default:
                    return new SettingsConfiguration(SettingsStyle.None, Array.Empty<ReflectionSettingsConfig>());
            }
        }
    }
}