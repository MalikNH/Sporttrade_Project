using Noggog.WPF;
using ReactiveUI;
using System;
using System.Collections.Generic;
using System.Reactive.Disposables;
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
using Synthesis.Bethesda.GUI.ViewModels.Profiles.Running;

namespace Synthesis.Bethesda.GUI.Views
{
    public class PatcherErrorViewBase : NoggogUserControl<ErrorVM> { }

    /// <summary>
    /// Interaction logic for PatcherErrorView.xaml
    /// </summary>
    public partial class PatcherErrorView : PatcherErrorViewBase
    {
        public PatcherErrorView()
        {
            InitializeComponent();
            this.WhenActivated(disposable =>
            {
                this.WhenAnyFallback(x => x.ViewModel!.Title)
                    .BindTo(this, x => x.TitleBlock.Text)
                    .DisposeWith(disposable);
                this.WhenAnyFallback(x => x.ViewModel!.String)
                    .BindTo(this, x => x.ErrorOutputBox.Text)
                    .DisposeWith(disposable);
                this.WhenAnyValue(x => x.ViewModel!.BackCommand)
                    .BindTo(this, x => x.CloseErrorButton.Command)
                    .DisposeWith(disposable);
            });
        }
    }
}
