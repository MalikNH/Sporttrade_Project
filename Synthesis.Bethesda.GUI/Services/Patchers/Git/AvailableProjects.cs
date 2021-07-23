﻿using System.Linq;
using System.Reactive.Linq;
using DynamicData;
using DynamicData.Binding;
using Noggog.WPF;

namespace Synthesis.Bethesda.GUI.Services.Patchers.Git
{
    public interface IAvailableProjects
    {
        IObservableCollection<string> Projects { get; }
    }

    public class AvailableProjects : ViewModel, IAvailableProjects
    {
        public IObservableCollection<string> Projects { get; }

        public AvailableProjects(
            IDriverRepositoryPreparation driverRepositoryPreparation)
        {
            Projects = driverRepositoryPreparation.DriverInfo
                .Select(x => x.Item?.AvailableProjects ?? Enumerable.Empty<string>())
                .Select(x => x.AsObservableChangeSet<string>())
                .Switch()
                .ToObservableCollection(this);
        }
    }
}