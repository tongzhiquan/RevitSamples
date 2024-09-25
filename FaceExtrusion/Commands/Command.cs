using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using Autodesk.Revit.UI.Selection;

namespace FaceExtrusion.Commands
{
    [UsedImplicitly]
    [Transaction(TransactionMode.Manual)]
    public class Command : IExternalCommand
    {
        UIApplication UIApplication { get; set; }
        UIDocument UIDocument { get; set; }
        Document Document { get; set; }
        Selection Selection { get; set; }


        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            this.UIApplication = commandData.Application;
            this.UIDocument = this.UIApplication.ActiveUIDocument;
            this.Document = this.UIDocument.Document;
            this.Selection = this.UIDocument.Selection;



            return Result.Succeeded;

        }





    }
}