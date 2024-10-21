using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using SequentialSelector.Views.Utils;
using System.Drawing.Drawing2D;

namespace SequentialSelector.ViewModels
{
    public sealed partial class SequentialSelectorViewModel : ObservableObject
    {
        private List<int> SelectedElementIDs { get; set; }

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
            // TODO 触发ESC
            
        }

        [RelayCommand]
        private void Cancel()
        {
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


        public void SelectElement(int elementId)
        {
            if (this.SelectedElementIDs.Contains(elementId))
            {
                this.SelectedElementIDs.Remove(elementId);
            }
            else
            {
                this.SelectedElementIDs.Add(elementId);
            }


            if (this.SelectedElementIDs.Count > 0)
            {
                this.IsCheckboxEnabled = false;
            }
            else
            {
                this.IsCheckboxEnabled = true;
            }
        }

        public List<int> GetElementIds()
        {
            return this.SelectedElementIDs;
        }
    }
}