using CommandLine;
using Mutagen.Bethesda;
using Mutagen.Bethesda.Plugins;
using Noggog;

namespace Synthesis.Bethesda.Commands
{
    [Verb("run-patcher", HelpText = "Run the patcher")]
    public class RunSynthesisPatcher
    {
        [Option('s', "SourcePath", Required = false, HelpText = "Optional path pointing to the previous patcher result to build onto.  File name must in ModKey format")]
        public FilePath? SourcePath { get; set; }

        [Option('o', "OutputPath", Required = true, HelpText = "Path where the patcher should place its resulting file.  File name must in ModKey format")]
        public FilePath OutputPath { get; set; } = string.Empty;

        [Option('g', "GameRelease", Required = true, HelpText = "GameRelease data folder is related to.")]
        public GameRelease GameRelease { get; set; }

        [Option('d', "DataFolderPath", Required = true, HelpText = "Path to the data folder.")]
        public DirectoryPath DataFolderPath { get; set; } = string.Empty;

        [Option('l', "LoadOrderFilePath", Required = false, HelpText = "Path to the load order file to use.  This is typically plugins.txt.  This should be the file that the game will use to read in its load order.")]
        public FilePath LoadOrderFilePath { get; set; } = string.Empty;

        [Option('r', "PersistencePath", Required = false, HelpText = "Path to the shared FormKey allocation state")]
        public string? PersistencePath { get; set; }

        [Option('p', "PatcherName", Required = false, HelpText = "Name of the patcher to be recorded in the shared FormKey allocation state")]
        public string? PatcherName { get; set; }
        
        [Option('k', "ModKey", Required = false, HelpText = "ModKey associated with the patch being generated")]
        public string? ModKey { get; set; }
        
        public override string ToString()
        {
            return $"{nameof(RunSynthesisPatcher)} => \n"
                + $"  {nameof(SourcePath)} => {SourcePath} \n"
                + $"  {nameof(OutputPath)} => {OutputPath} \n"
                + $"  {nameof(GameRelease)} => {GameRelease} \n"
                + $"  {nameof(DataFolderPath)} => {DataFolderPath} \n"
                + $"  {nameof(LoadOrderFilePath)} => {LoadOrderFilePath} \n"
                + $"  {nameof(PersistencePath)} => {PersistencePath} \n"
                + $"  {nameof(PatcherName)} => {PatcherName} \n"
                + $"  {nameof(ModKey)} => {ModKey}";
        }
    }
}
