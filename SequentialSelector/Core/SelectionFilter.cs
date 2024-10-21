using Autodesk.Revit.DB;
using Autodesk.Revit.UI.Selection;

namespace SequentialSelector.Core
{
    internal class SelectionFilter : ISelectionFilter
    {
        public bool AllowElement(Element elem) => throw new NotImplementedException();

        public bool AllowReference(Reference reference, XYZ position) => throw new NotImplementedException();
    }
}