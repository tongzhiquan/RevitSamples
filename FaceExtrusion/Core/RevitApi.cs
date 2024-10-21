using Autodesk.Revit.DB;

namespace FaceExtrusion.Core
{
    /// <summary>
    ///     The class contains wrapping methods for working with the Revit API.
    /// </summary>
    internal static class RevitApi
    {
        public static DirectShape CreateDirectShpae(Document document, GeometryObject solid, byte r = 128, byte g = 128, byte b = 128)
        {
            DirectShape ds = null;
            using (TransactionGroup tg = new TransactionGroup(document, "Create DirectShape"))
            {
                tg.Start();

                using (Transaction ts = new(document, "--"))
                {
                    _ = ts.Start();
                    ds = DirectShape.CreateElement(document, new ElementId(BuiltInCategory.OST_GenericModel));
                    ds.SetShape([solid]);
                    _ = ts.Commit();

                    ChangeElementColor(ds, r, g, b);
                }
                tg.Assimilate();
            }

            return ds;
        }

        public static void ChangeElementColor(Element element, byte r, byte g, byte b)
        {
            Document document = element.Document;

            using Transaction ts = new(document, "Change Element Color");
            ts.Start();
            document.ActiveView.SetElementOverrides(element.Id, CreateColor(document, new Color(r, g, b)));
            ts.Commit();
        }

        private static OverrideGraphicSettings CreateColor(Document doc, Color color)
        {
            FilteredElementCollector fillFilter = new FilteredElementCollector(doc);
            fillFilter.OfClass(typeof(FillPatternElement));
            //获取实体
            FillPatternElement fp = fillFilter.First(m => (m as FillPatternElement).GetFillPattern().IsSolidFill) as FillPatternElement;
            OverrideGraphicSettings ogs = new OverrideGraphicSettings();

            //填充图案
            ogs.SetProjectionFillPatternId(fp.Id); //ogs.SetSurfaceForegroundPatternId(fp.Id);
            //颜色
            ogs.SetProjectionFillColor(color); //ogs.SetSurfaceForegroundPatternColor(color);
            return ogs;
        }

        /// <summary>
        /// 获取一个元素的Solid
        /// </summary>
        /// <para></para>
        /// <para><paramref name="element"/>：Revit元素</para>
        /// <para><paramref name="withNestedFamily"/>：是否要包含内嵌族，默认 false</para>
        /// <para><paramref name="excludes"/>可选参数：需要排除的内嵌族，名称中包含该值的内嵌族不计算</para>
        public static List<Solid> GetSolidsFromElement(Element element, bool withNestedFamily = false, params string[] excludes)
        {
            Options options = new()
            {
                ComputeReferences = true,
                IncludeNonVisibleObjects = true,
                DetailLevel = ViewDetailLevel.Fine
            };

            GeometryElement geometryElement = element.get_Geometry(options);

            List<Solid> solids = [];

            if (geometryElement != null)
            {
                GetSolidsFromElement_Recursion(geometryElement, ref solids);
            }

            if (withNestedFamily)
            {
                List<Element> elements = [];
                GetFamilyNestedInstance(element, ref elements, true);

                List<Element> _elements = [];

                foreach (Element ele in elements)
                {
                    bool hasExclude = false;

                    foreach (string name in excludes)
                    {
                        if (ele.Name.Contains(name)) { hasExclude = true; break; }
                    }

                    if (!hasExclude) { _elements.Add(ele); }
                }

                foreach (Element _element in _elements)
                {
                    GeometryElement subGeometryElement = _element.get_Geometry(options);

                    GetSolidsFromElement_Recursion(subGeometryElement, ref solids);
                }
            }

            return solids;
        }

        private static void GetSolidsFromElement_Recursion(GeometryElement geometryElement, ref List<Solid> solids)
        {
            foreach (GeometryObject geomObj in geometryElement)
            {
                if (geomObj is Solid solid)
                {
                    if (solid.Faces != null && solid.Faces.Size > 0) // && solid.Edges != null && solid.Edges.Size > 0  // Edges 可能为空
                    {
                        solids.Add(SolidUtils.Clone(solid));
                    }
                }

                //If this GeometryObject is Instance, call AddCurvesAndSolids
                else if (geomObj is GeometryInstance geometryInstance)
                {
                    //GeometryElement transformedGeomElem = geometryInstance.GetInstanceGeometry();
                    GeometryElement transformedGeomElem = geometryInstance.GetSymbolGeometry(geometryInstance.Transform); // apply the transform to instance
                    GetSolidsFromElement_Recursion(transformedGeomElem, ref solids);
                }
            }
        }

        private static void GetFamilyNestedInstance(Element element, ref List<Element> elements, bool deepFind)
        {
            if (element is FamilyInstance familyInstance)
            {
                ICollection<ElementId> subInstanceIds = familyInstance.GetSubComponentIds();
                if (subInstanceIds != null && subInstanceIds.Count > 0)
                {
                    List<Element> _elements = subInstanceIds.Select(id => element.Document.GetElement(id)).ToList();

                    elements.AddRange(_elements);

                    if (deepFind)
                    {
                        foreach (Element _element in _elements)
                        {
                            GetFamilyNestedInstance(_element, ref elements, deepFind);
                        }
                    }
                }
            }
        }

        public static List<List<Curve>> TransformCurveloops(Face face, Transform transform)
        {
            List<List<Curve>> curveloops = [];

            foreach (CurveLoop loop in face.GetEdgesAsCurveLoops())
            {
                List<Curve> curves = [];
                foreach (Curve curve in loop)
                {
                    Curve newCurve = curve.CreateTransformed(transform);
                    curves.Add(newCurve);
                }
                curveloops.Add(curves);
            }
            return curveloops;
        }
    }
}