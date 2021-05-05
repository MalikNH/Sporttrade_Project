using Mutagen.Bethesda.Plugins.Order;
using Mutagen.Bethesda.Plugins.Records;
using Synthesis.Bethesda;
using System.Collections.Generic;

namespace Mutagen.Bethesda.Synthesis
{
    /// <summary>
    /// A class housing all the tools, parameters, and entry points for a typical Synthesis check runnability analysis
    /// </summary>
    public class RunnabilityState : IRunnabilityState
    {
        /// <summary>
        /// Instructions given to the patcher from the Synthesis pipeline
        /// </summary>
        public CheckRunnability Settings { get; }

        /// <summary>
        /// A list of ModKeys as they appeared, and whether they were enabled
        /// </summary>
        public IReadOnlyList<LoadOrderListing> LoadOrder { get; }
        IEnumerable<LoadOrderListing> IRunnabilityState.LoadOrder => this.LoadOrder;

        public string LoadOrderFilePath => Settings.LoadOrderFilePath;

        public string DataFolderPath => Settings.DataFolderPath;

        public GameRelease GameRelease => Settings.GameRelease;

        public RunnabilityState(
            CheckRunnability settings,
            IReadOnlyList<LoadOrderListing> rawLoadOrder)
        {
            Settings = settings;
            LoadOrder = rawLoadOrder;
        }

        public GameEnvironmentState<TModSetter, TModGetter> GetEnvironmentState<TModSetter, TModGetter>()
            where TModSetter : class, IContextMod<TModSetter, TModGetter>, TModGetter
            where TModGetter : class, IContextGetterMod<TModSetter, TModGetter>
        {
            var lo = Plugins.Order.LoadOrder.Import<TModGetter>(DataFolderPath, LoadOrder, GameRelease);
            return new GameEnvironmentState<TModSetter, TModGetter>(
                gameFolderPath: DataFolderPath,
                loadOrder: lo,
                linkCache: lo.ToImmutableLinkCache<TModSetter, TModGetter>());
        }
    }
}
