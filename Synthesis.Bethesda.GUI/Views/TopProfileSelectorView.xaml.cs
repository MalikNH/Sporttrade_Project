using Noggog.WPF;
using ReactiveUI;
using System.Reactive.Disposables;
using System;
using System.Collections.Generic;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Reactive.Linq;
using Mutagen.Bethesda;
using Synthesis.Bethesda.GUI.ViewModels.Top;

namespace Synthesis.Bethesda.GUI.Views
{
    public class TopProfileSelectorViewBase : NoggogUserControl<MainVm> { }

    /// <summary>
    /// Interaction logic for TopProfileSelectorView.xaml
    /// </summary>
    public partial class TopProfileSelectorView : TopProfileSelectorViewBase
    {
        public TopProfileSelectorView()
        {
            InitializeComponent();
            this.WhenActivated(dispose =>
            {
                this.WhenAnyFallback(x => x.ViewModel!.SelectedProfile!.NameVm.Name, string.Empty)
                    .BindTo(this, x => x.ProfileNameBlock.Text)
                    .DisposeWith(dispose);

                this.WhenAnyFallback(x => x.ViewModel!.SelectedProfile!.Release, GameRelease.SkyrimSE)
                    .ObserveOn(RxApp.TaskpoolScheduler)
                    .Select(gameRelease =>
                    {
                        return ImageUtility.BitmapImageFromResource(ResourceConstants.AssemblyName, ResourceConstants.GetIcon(gameRelease));
                    })
                    .ObserveOnGui()
                    .BindTo(this, x => x.GameIconImage.Source)
                    .DisposeWith(dispose);

                this.WhenAnyValue(x => x.ViewModel!.OpenProfilesPageCommand)
                    .BindTo(this, x => x.OpenProfilesPageButton.Command)
                    .DisposeWith(dispose);

                this.WhenAnyFallback(x => x.ViewModel!.SelectedProfile!.Versioning.UpdateProfileNugetVersionCommand)
                    .Select(x => x as ICommand)
                    .BindTo(this, x => x.UpdateButton.Command)
                    .DisposeWith(dispose);
                Observable.CombineLatest(
                        this.WhenAnyFallback(x => x.ViewModel!.SelectedProfile!.Versioning.UpdateProfileNugetVersionCommand)
                            .Select(x => x?.CanExecute ?? Observable.Return(false))
                            .Switch(),
                        this.WhenAnyFallback(x => x.ViewModel!.SelectedProfile!.LockSetting.Lock),
                        (hasUpdate, locked) => hasUpdate && !locked)
                    .Select(x => x ? Visibility.Visible : Visibility.Collapsed)
                    .BindTo(this, x => x.UpdateButton.Visibility)
                    .DisposeWith(dispose);

                this.WhenAnyValue(x => x.ViewModel!.InModal)
                    .Select(x => !x)
                    .BindTo(this, x => x.IsEnabled)
                    .DisposeWith(dispose);
            });
        }
    }
}
