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
using Synthesis.Bethesda.GUI.ViewModels.Profiles.Running;

namespace Synthesis.Bethesda.GUI.Views
{
    public class MainRunViewBase : NoggogUserControl<RunVm> { }

    /// <summary>
    /// Interaction logic for MainRunView.xaml
    /// </summary>
    public partial class MainRunView : MainRunViewBase
    {
        public MainRunView()
        {
            InitializeComponent();
            this.WhenActivated(disposable =>
            {
                this.WhenAnyValue(x => x.ViewModel!.Groups)
                    .BindTo(this, x => x.GroupsList.ItemsSource)
                    .DisposeWith(disposable);

                // Wire up patcher config data context and visibility
                this.WhenAnyValue(x => x.ViewModel!.DetailDisplay)
                    .BindTo(this, x => x.PatcherDetail.Content)
                    .DisposeWith(disposable);
                this.WhenAnyValue(x => x.ViewModel!.DetailDisplay)
                    .Select(x => x == null ? Visibility.Collapsed : Visibility.Visible)
                    .BindTo(this, x => x.PatcherDetail.Visibility)
                    .DisposeWith(disposable);

                // Set up top bar
                this.WhenAnyValue(x => x.ViewModel!.Running)
                    .Select(r => r ? Visibility.Visible : Visibility.Collapsed)
                    .BindTo(this, x => x.RunningRingAnimation.Visibility)
                    .DisposeWith(disposable);
                this.WhenAnyValue(x => x.ViewModel!.Running)
                    .Select(r => r ? Visibility.Collapsed : Visibility.Visible)
                    .BindTo(this, x => x.BackButton.Visibility)
                    .DisposeWith(disposable);
                this.WhenAnyValue(x => x.ViewModel!.Running)
                    .Select(r => r ? Visibility.Visible : Visibility.Collapsed)
                    .BindTo(this, x => x.CancelButton.Visibility)
                    .DisposeWith(disposable);
                this.WhenAnyValue(x => x.ViewModel!.BackCommand)
                    .BindTo(this, x => x.BackButton.Command)
                    .DisposeWith(disposable);
                this.WhenAnyValue(x => x.ViewModel!.CancelCommand)
                    .BindTo(this, x => x.CancelButton.Command)
                    .DisposeWith(disposable);
                this.WhenAnyValue(x => x.ViewModel!.Running)
                    .Select(running => running ? "Patching" : "Patch Results")
                    .BindTo(this, x => x.TopTitleBlock.Text)
                    .DisposeWith(disposable);
                this.WhenAnyValue(
                        x => x.ViewModel!.Running,
                        x => x.ViewModel!.ResultError,
                        (running, err) =>
                        {
                            if (running || err == null) return Visibility.Collapsed;
                            return Visibility.Visible;
                        })
                    .BindTo(this, x => x.OverallErrorButton.Visibility)
                    .DisposeWith(disposable);
                this.WhenAnyValue(x => x.ViewModel!.ResultError)
                    .Select(x => x?.ToString() ?? string.Empty)
                    .BindTo(this, x => x.OverallErrorButton.ToolTip)
                    .DisposeWith(disposable);
                this.WhenAnyValue(x => x.ViewModel!.ShowOverallErrorCommand)
                    .BindTo(this, x => x.OverallErrorButton.Command)
                    .DisposeWith(disposable);
            });
        }
    }
}
