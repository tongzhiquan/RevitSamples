using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using Autodesk.Revit.UI.Selection;
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


            try
            {
                SequentialSelectorView view = this.SetupSequentialSelector();
                this.Selection.PickObject(ObjectType.Element);
            }
            catch (Autodesk.Revit.Exceptions.OperationCanceledException)
            {
                // Do nothing
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
                IsPreviousEnabled = true
            };

            SequentialSelectorView view = new SequentialSelectorView(viewModel);

            RibbonController.ShowOptionsBar(view);
            return view;
        }


        //private List<Element> SelectElement()
        //{
        //}



    }
}