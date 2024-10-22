using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using SequentialSelector.Views.Utils;

namespace SequentialSelector.ViewModels
{
    public sealed partial class SequentialSelectorViewModel : ObservableObject
    {
        private List<long> SelectedElementIDs { get; set; } = [];

        [ObservableProperty]
        private bool isMultiple;

        [ObservableProperty]
        private bool isCheckboxEnabled;

        [ObservableProperty]
        private bool isPreviousEnabled;

        [ObservableProperty]
        private bool isCancelEnabled;

        [RelayCommand]
        private void Previous()
        {
            // HACK: 触发ESC
            KeySimulator.PressEscape();
            RibbonController.HideOptionsBar();
        }

        [RelayCommand]
        private void Cancel()
        {
            KeySimulator.PressEscape();
            RibbonController.HideOptionsBar();
        }

        [RelayCommand]
        private void CheckBoxChanged()
        {
            if (this.IsMultiple)
            {
                this.IsPreviousEnabled = true;
                this.IsCancelEnabled = true;
            }
            else
            {
                this.IsPreviousEnabled = false;
                this.IsCancelEnabled = false;
            }
        }

        public bool SelectElement(long elementId)
        {
            bool isSelected = false;

            if (this.SelectedElementIDs.Contains(elementId))
            {
                this.SelectedElementIDs.Remove(elementId);
                isSelected = false;
            }
            else
            {
                this.SelectedElementIDs.Add(elementId);
                isSelected = true;
            }

            if (this.SelectedElementIDs.Count > 0)
            {
                this.IsCheckboxEnabled = false;
            }
            else
            {
                this.IsCheckboxEnabled = true;
            }

            return isSelected;
        }

        public List<long> GetElementIds()
        {
            return this.SelectedElementIDs;
        }
    }
}