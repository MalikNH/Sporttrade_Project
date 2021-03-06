using System;

namespace Synthesis.Bethesda.GUI.Settings
{
    public interface ISynthesisGuiSettings
    {
        bool ShowHelp { get; set; }
        bool OpenIdeAfterCreating { get; set; }
        IDE Ide { get; set; }
        string MainRepositoryFolder { get; set; }
        string SelectedProfile { get; set; }
        string WorkingDirectory { get; set; }
        byte BuildCores { get; set; }
    }

    public record SynthesisGuiSettings : ISynthesisGuiSettings
    {
        public bool ShowHelp { get; set; } = true;
        public bool OpenIdeAfterCreating { get; set; } = true;
        public IDE Ide { get; set; } = IDE.SystemDefault;
        public string MainRepositoryFolder { get; set; } = string.Empty;
        public string SelectedProfile { get; set; } = string.Empty;
        public string WorkingDirectory { get; set; } = string.Empty;
        public byte BuildCores { get; set; } = (byte)Math.Min(byte.MaxValue, Math.Max(Environment.ProcessorCount / 2, 1));
    }
}
