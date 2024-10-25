using Autodesk.Revit.DB;
using Autodesk.Revit.UI;

namespace FamilyInstanceCut.Core
{
    public static class RevitApi
    {
        public static UIApplication UiApplication { get; set; }
        public static Autodesk.Revit.ApplicationServices.Application Application => UiApplication.Application;
        public static UIDocument UiDocument => UiApplication.ActiveUIDocument;
        public static Document Document => UiDocument.Document;
        public static View ActiveView
        {
            get => UiDocument.ActiveView;
            set => UiDocument.ActiveView = value;
        }
    }
}