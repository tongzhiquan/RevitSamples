using Autodesk.Revit.DB;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using SequentialSelector.Views.Utils;

namespace SequentialSelector.ViewModels
{
    public sealed partial class SequentialSelectorViewModel : ObservableObject
    {
        private List<ElementId> SelectedElementIds { get; set; } = [];
        public bool IsFinishBtnClicked { get; set; } = false;

        [ObservableProperty]
        private bool isMultiple;

        [ObservableProperty]
        private bool isCheckboxEnabled;

        [ObservableProperty]
        private bool isFinishBtnEnabled;

        [ObservableProperty]
        private bool isCancelBtnEnabled;

        [RelayCommand]
        private void Finish()
        {
            this.IsFinishBtnEnabled = true;
            // 触发ESC
            KeySimulator.PressEscape();
            RibbonController.HideOptionsBar();
        }

        [RelayCommand]
        private void Cancel()
        {
            this.SelectedElementIds.Clear();
            KeySimulator.PressEscape();
            RibbonController.HideOptionsBar();
        }

        [RelayCommand]
        private void CheckBoxChanged()
        {
            if (this.IsMultiple)
            {
                this.IsFinishBtnEnabled = true;
                this.IsCancelBtnEnabled = true;
            }
            else
            {
                this.IsFinishBtnEnabled = false;
                this.IsCancelBtnEnabled = false;
            }
        }

        public bool SelectElement(ElementId elementId)
        {
            bool hasSelected;

            if (this.SelectedElementIds.Contains(elementId))
            {
                this.SelectedElementIds.Remove(elementId);
                hasSelected = false;
            }
            else
            {
                this.SelectedElementIds.Add(elementId);
                hasSelected = true;
            }

            if (this.SelectedElementIds.Count > 0)
            {
                this.IsCheckboxEnabled = false;
            }
            else
            {
                this.IsCheckboxEnabled = true;
            }

            return hasSelected;
        }

        public List<ElementId> GetSelectedElementIds()
        {
            return this.SelectedElementIds;
        }
    }
}