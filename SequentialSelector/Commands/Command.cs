using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using Autodesk.Revit.UI.Selection;
using SequentialSelector.Core;
using SequentialSelector.ViewModels;
using SequentialSelector.Views;
using SequentialSelector.Views.Utils;

namespace SequentialSelector.Commands
{
    [UsedImplicitly]
    [Transaction(TransactionMode.Manual)]
    public class Command : IExternalCommand
    {
        private UIApplication UIApplication { get; set; }
        private UIDocument UIDocument { get; set; }
        private Document Document { get; set; }
        private Selection Selection { get; set; }

        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            this.UIApplication = commandData.Application;
            this.UIDocument = this.UIApplication.ActiveUIDocument;
            this.Document = this.UIDocument.Document;
            this.Selection = this.UIDocument.Selection;

            SequentialSelectorView view = this.SetupSequentialSelector();
            SequentialSelectorViewModel vm = (SequentialSelectorViewModel)view.DataContext;

            try
            {
                while (true)
                {
                    Reference referance = this.Selection.PickObject(ObjectType.Element);

                    Element element = this.Document.GetElement(referance);
                    if (vm.SelectElement(referance.ElementId.Value))
                    {
                        RevitApi.ChangeElementColor(
                            element,
                            new Color(92, 129, 212),
                            new Color(0, 0, 250),
                            25);
                    }
                    else
                    {
                        RevitApi.ResetElementColor(element);
                    }
                }

            }
            catch (Autodesk.Revit.Exceptions.OperationCanceledException)
            {
                // Do nothing

                var elementIDs = vm.GetElementIds();

                if (elementIDs.Count > 0)
                {
                    string sequence = "";

                    foreach (var elementId in elementIDs)
                    {
                        Element element = this.Document.GetElement(new ElementId(elementId));
                        RevitApi.ResetElementColor(element);
                        sequence += element.Name + "\n";
                    }
                    elementIDs.Clear();

                    TaskDialog.Show("Info", sequence);
                }
            }
            finally
            {
                RibbonController.HideOptionsBar();
            }

            return Result.Succeeded;
        }


        private SequentialSelectorView SetupSequentialSelector()
        {
            SequentialSelectorViewModel viewModel = new SequentialSelectorViewModel
            {
                IsMultiple = true,
                IsCancelEnabled = true,
                IsPreviousEnabled = true,
                IsCheckboxEnabled = true
            };

            SequentialSelectorView view = new SequentialSelectorView(viewModel);

            RibbonController.ShowOptionsBar(view);
            return view;
        }

    }
}