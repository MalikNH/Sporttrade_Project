using System.Reactive.Linq;
using System.Windows.Input;
using DynamicData;
using DynamicData.Binding;
using Noggog;
using Noggog.WPF;
using ReactiveUI;
using Synthesis.Bethesda.Execution.Settings;
using Synthesis.Bethesda.GUI.Services.Main;
using Synthesis.Bethesda.GUI.ViewModels.Top;
using Synthesis.Bethesda.GUI.ViewModels.Top.Settings;

namespace Synthesis.Bethesda.GUI.ViewModels.Profiles
{
    public class ProfileDisplayVm : ViewModel
    {
        public ProfilesDisplayVm Parent { get; }

        public ProfileVm Profile { get; }

        private readonly ObservableAsPropertyHelper<bool> _IsDisplaying;
        public bool IsDisplaying => _IsDisplaying.Value;

        private readonly ObservableAsPropertyHelper<bool> _IsActive;
        public bool IsActive => _IsActive.Value;

        public ICommand DeleteCommand { get; }
        public ICommand SwitchToCommand { get; }
        public ICommand OpenInternalProfileFolderCommand { get; }

        public ObservableCollectionExtended<PersistenceMode> PersistenceModes { get; } = new(EnumExt.GetValues<PersistenceMode>());

        public delegate ProfileDisplayVm Factory(ProfilesDisplayVm parent, ProfileVm profile);
        
        public ProfileDisplayVm(
            ProfilesDisplayVm parent,
            INavigateTo navigate, 
            ISelectedProfileControllerVm selProfile,
            IConfirmationPanelControllerVm confirmation,
            ProfileVm profile)
        {
            Parent = parent;
            Profile = profile;

            _IsDisplaying = parent.WhenAnyValue(x => x.DisplayedProfile)
                .Select(x => x == this)
                .ToGuiProperty(this, nameof(IsDisplaying));

            _IsActive = this.WhenAnyValue(x => x.Profile.IsActive)
                .ToGuiProperty(this, nameof(IsActive));

            DeleteCommand = ReactiveCommand.Create(
                canExecute: this.WhenAnyValue(x => x.Profile!.IsActive)
                    .Select(active => !active),
                execute: () =>
                {
                    var profile = this.Profile;
                    if (profile.IsActive) return;
                    confirmation.TargetConfirmation = new ConfirmationActionVm(
                        "Confirm",
                        $"Are you sure you want to delete {profile.NameVm.Name}?",
                        () =>
                        {
                            parent.Config.Profiles.Remove(profile);
                            Parent.SwitchToActive();
                        });
                });
            SwitchToCommand = ReactiveCommand.Create(
                execute: () =>
                {
                    var profile = this.Profile;
                    if (profile.IsActive) return;
                    selProfile.SelectedProfile = profile;
                });
            OpenInternalProfileFolderCommand = ReactiveCommand.Create(
                execute: () =>
                {
                    navigate.Navigate(Profile!.ProfileDirectory);
                },
                canExecute: this.WhenAnyValue(x => x.Profile)
                    .Select(x => x != null));
        }
    }
}
