using Noggog.WPF;
using ReactiveUI;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using Noggog;
using Synthesis.Bethesda.GUI.ViewModels.Patchers.Initialization;
using Synthesis.Bethesda.GUI.ViewModels.Patchers.Initialization.Cli;

namespace Synthesis.Bethesda.GUI.Views
{
    public class CliInitViewBase : NoggogUserControl<CliPatcherInitVm> { }

    /// <summary>
    /// Interaction logic for CliInitView.xaml
    /// </summary>
    public partial class CliInitView : CliInitViewBase
    {
        public CliInitView()
        {
            InitializeComponent();
            this.WhenActivated(disposable =>
            {
                this.WhenAnyValue(x => x.ViewModel!.NameVm.Name)
                    .Select(x => x.IsNullOrWhitespace() ? "Patcher Name" : x)
                    .BindTo(this, view => view.PatcherDetailName.Text)
                    .DisposeWith(disposable);
                this.WhenAnyValue(x => x.ViewModel!.NameVm.Name)
                    .Select(x => x.IsNullOrWhitespace() ? 0.6d : 1d)
                    .BindTo(this, view => view.PatcherDetailName.Opacity)
                    .DisposeWith(disposable);
                this.WhenAnyValue(x => x.ViewModel)
                    .BindTo(this, x => x.PatcherIconDisplay.DataContext)
                    .DisposeWith(disposable);

                // Set up discard/confirm clicks
                this.WhenAnyValue(x => x.ViewModel!.CancelConfiguration)
                    .BindTo(this, x => x.CancelAdditionButton.Command)
                    .DisposeWith(disposable);
                this.WhenAnyValue(x => x.ViewModel!.CompleteConfiguration)
                    .BindTo(this, x => x.ConfirmButton.ConfirmAdditionButton.Command)
                    .DisposeWith(disposable);
            });
        }
    }
}
