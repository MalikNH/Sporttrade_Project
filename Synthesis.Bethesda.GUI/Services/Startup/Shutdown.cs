using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Threading.Tasks;
using System.Windows;
using Autofac;
using Newtonsoft.Json;
using Serilog;
using Synthesis.Bethesda.Execution.Pathing;
using Synthesis.Bethesda.Execution.Settings.Json.Pipeline;
using Synthesis.Bethesda.GUI.Json;
using Synthesis.Bethesda.GUI.Services.Main;
using Synthesis.Bethesda.GUI.Settings;
using Synthesis.Bethesda.GUI.Views;

#if !DEBUG
using System.Diagnostics;
using Noggog.Utility;
#endif

namespace Synthesis.Bethesda.GUI.Services.Startup
{
    public interface IShutdown
    {
        bool IsShutdown { get; }

        void Prepare();
    }

    public class Shutdown : IShutdown
    {
        private readonly ILifetimeScope _scope;
        private readonly ILogger _logger;
        private readonly IStartupTracker _init;
        private readonly IPipelineSettingsPath _pipelineSettingsPath;
        private readonly IGuiSettingsPath _guiPaths;
        private readonly IRetrieveSaveSettings _save;
        private readonly IGuiSettingsExporter _guiSettingsExporter;
        private readonly IPipelineSettingsExporter _pipelineSettingsExporter;
        private readonly ShutDownBuildServer _shutDownBuildServer;
        private readonly IMainWindow _window;
        
        public bool IsShutdown { get; private set; }

        public Shutdown(
            ILifetimeScope scope,
            ILogger logger,
            IStartupTracker init,
            IPipelineSettingsPath paths,
            IGuiSettingsPath guiPaths,
            IRetrieveSaveSettings save,
            IGuiSettingsExporter guiSettingsExporter,
            IPipelineSettingsExporter pipelineSettingsExporter,
            ShutDownBuildServer shutDownBuildServer,
            IMainWindow window)
        {
            _scope = scope;
            _logger = logger;
            _init = init;
            _pipelineSettingsPath = paths;
            _guiPaths = guiPaths;
            _save = save;
            _guiSettingsExporter = guiSettingsExporter;
            _pipelineSettingsExporter = pipelineSettingsExporter;
            _shutDownBuildServer = shutDownBuildServer;
            _window = window;
        }

        public void Prepare()
        {
            _window.Closing += (_, b) =>
            {
                _window.Visibility = Visibility.Collapsed;
                Closing(b);
            };
        }
        
        private async void ExecuteShutdown()
        {
            IsShutdown = true;

            await Task.Run(() =>
            {
                if (!_init.Initialized)
                {
                    _logger.Information("App was unable to start up.  Not saving settings");
                    return;
                }

                try
                {
                    _save.Retrieve(out var gui, out var pipe);
                    _pipelineSettingsExporter.Write(_pipelineSettingsPath.Path, pipe);
                    _guiSettingsExporter.Write(_guiPaths.Path, gui);
                }
                catch (Exception e)
                {
                    _logger.Error(e, "Error saving settings");
                }
            });
            
            var toDo = new List<Task>();
#if !DEBUG
            toDo.Add(Task.Run(_shutDownBuildServer.Shutdown));
#endif

            toDo.Add(Task.Run(() =>
            {
                try
                {
                    _logger.Information("Disposing container");
                    _scope.Dispose();
                }
                catch (Exception e)
                {
                    _logger.Error(e, "Error shutting down container actions");
                }
            }));
            await Task.WhenAll(toDo);
            Application.Current.Shutdown();
        }

        private void Closing(CancelEventArgs args)
        {
            if (IsShutdown) return;
            args.Cancel = true;
            ExecuteShutdown();
        }
    }
}