using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using Autodesk.Revit.UI.Selection;
using SequentialSelector.ViewModels;
using SequentialSelector.Views;
using SequentialSelector.Views.Utils;

namespace SequentialSelector.Core
{
    public static class SelectionUtils
    {
        public static IList<Reference> PickObjectsSequential(UIApplication uIApplication, ISelectionFilter selectionFilter)
        {
            return PickObjectsSequential(uIApplication, ObjectType.Element, selectionFilter, "Select elements");
        }

        public static IList<Reference> PickObjectsSequential(UIApplication uIApplication, string statusPrompt)
        {
            return PickObjectsSequential(uIApplication, ObjectType.Element, new SelectionFilter(), statusPrompt);
        }

        public static IList<Reference> PickObjectsSequential(UIApplication uIApplication, ObjectType objectType, ISelectionFilter selectionFilter, string statusPrompt, [CanBeNull] IList<Reference> pPreSelected = null)
        {
            IList<Reference> result = [];

            Selection selection = uIApplication.ActiveUIDocument.Selection;
            Document document = uIApplication.ActiveUIDocument.Document;

            SequentialSelectorViewModel viewModel = new SequentialSelectorViewModel
            {
                IsMultiple = true,
                IsCancelBtnEnabled = true,
                IsFinishBtnEnabled = true,
                IsCheckboxEnabled = true
            };
            SequentialSelectorView view = new SequentialSelectorView(viewModel);
            RibbonController.ShowOptionsBar(view);

            using Transaction transaction = new Transaction(document);
            using SubTransaction subTransaction = new SubTransaction(document);

            if (document.IsModifiable) { subTransaction.Start(); }
            else { transaction.Start("Select elements"); }

            try
            {
                while (true)
                {
                    Reference referance = selection.PickObject(objectType, selectionFilter, statusPrompt);

                    if (!viewModel.IsMultiple)
                    {
                        result.Add(referance);
                        break;
                    }

                    Element element = document.GetElement(referance);
                    if (viewModel.SelectElement(referance.ElementId))
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
                List<ElementId> elementIDs = viewModel.GetSelectedElementIds();

                if (viewModel.IsFinishBtnClicked && elementIDs.Count > 0)
                {
                    foreach (ElementId elementId in elementIDs)
                    {
                        Element element = document.GetElement(elementId);

                        result.Add(new Reference(element));

                        RevitApi.ResetElementColor(element);
                    }
                    elementIDs.Clear();
                }
            }
            finally
            {
                RibbonController.HideOptionsBar();
            }

            if (document.IsModifiable) { subTransaction.RollBack(); }
            else { transaction.RollBack(); }

            return result;
        }
    }

    internal class SelectionFilter : ISelectionFilter
    {
        public bool AllowElement(Element elem)
        {
            return true;
        }

        public bool AllowReference(Reference reference, XYZ position)
        {
            return true;
        }
    }
}