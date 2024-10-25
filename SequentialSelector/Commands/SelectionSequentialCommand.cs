using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using Autodesk.Revit.UI.Selection;
using SequentialSelector.Core;

namespace SequentialSelector.Commands
{
    [UsedImplicitly]
    [Transaction(TransactionMode.Manual)]
    public class SelectionSequentialCommand : IExternalCommand
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

            IList<Reference> referances = SelectionUtils.PickObjectsSequential(this.UIApplication, "Select elements");

            Reference referance = this.Selection.PickObject(ObjectType.Element, new SelectionFilter(), "Select elements");

            return Result.Succeeded;
        }
    }
}