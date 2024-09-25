using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using Autodesk.Revit.UI.Selection;
using FaceExtrusion.Core;

namespace FaceExtrusion.Commands
{
    [UsedImplicitly]
    [Transaction(TransactionMode.Manual)]
    public class FaceExtrusionCommand : IExternalCommand
    {
        private UIApplication UIApplication { get; set; }
        private UIDocument UIDocument { get; set; }
        private Document Document { get; set; }
        private Selection Selection { get; set; }

        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            LogUtils.CreateLogger();

            this.UIApplication = commandData.Application;
            this.UIDocument = this.UIApplication.ActiveUIDocument;
            this.Document = this.UIDocument.Document;
            this.Selection = this.UIDocument.Selection;

            //

            Reference reference = this.Selection.PickObject(ObjectType.Face);
            Element element = this.Document.GetElement(reference);
            Face face = element.GetGeometryObjectFromReference(reference) as Face;

            //

            Solid solid = this.CreateExtrusionGeometryWithFace(face, XYZ.BasisZ, 1);

            if (element is FamilyInstance familyInstance)
            {
                Transform transfrom = familyInstance.GetTransform();
                solid = SolidUtils.CreateTransformed(solid, transfrom);
            }

            RevitApi.CreateDirectShpae(this.Document, solid, 233, 109, 0);

            LogUtils.CloseLogger();

            return Result.Succeeded;
        }

        private Solid CreateExtrusionGeometryWithFace(Face face, XYZ extrusionDir, double extrusionDist)
        {
            // TODO：方向有要求，需要是向外侧
            extrusionDir = extrusionDir.Normalize();
            Transform transform = Transform.CreateTranslation(extrusionDir * extrusionDist);

            #region 基础数据

            SurfaceData surfaceData = SurfaceData.Create(face, transform);

            // 面
            Surface surface_base = face.GetSurface();
            Surface surface_new = surfaceData.Surface;

            // 边
            // TODO: 多轮廓面
            List<Curve> curves_base = RevitApi.TransformCurveloops(face, Transform.Identity).First();
            List<Curve> curves_new = surfaceData.Curveloops.First();

            // 侧面、侧边
            List<Curve> curves_side = [];
            List<Surface> surfaces_side = [];

            for (int i = 0; i < curves_base.Count; i++)
            {
                Curve curve_base = curves_base[i];
                Curve curve_new = curves_new[i];

                XYZ startPoint_base = curve_base.GetEndPoint(0);
                XYZ startPoint_new = curve_new.GetEndPoint(0);

                Line line = Line.CreateBound(startPoint_base, startPoint_new);
                curves_side.Add(line);

                Surface surface_side = RuledSurface.Create(curve_base, curve_new);
                surfaces_side.Add(surface_side);
            }

            #endregion 基础数据

            #region Brep

            // 添加BRep

            BRepBuilder builder = new BRepBuilder(BRepType.Solid);

            // 1. surface id
            BRepBuilderGeometryId faceId_Bottom = builder.AddFace(BRepBuilderSurfaceGeometry.Create(surface_base, null), true);  // 基础面方向需要翻转
            BRepBuilderGeometryId faceId_Top = builder.AddFace(BRepBuilderSurfaceGeometry.Create(surface_new, null), false);
            List<BRepBuilderGeometryId> faceIds_side = [];
            foreach (Surface _surface in surfaces_side)
            {
                faceIds_side.Add(builder.AddFace(BRepBuilderSurfaceGeometry.Create(_surface, null), false));
            }

            // 2. edge id
            List<BRepBuilderGeometryId> edgeIds_Bottom = [];
            List<BRepBuilderGeometryId> edgeIds_Top = [];
            List<BRepBuilderGeometryId> edgeIds_Side = [];
            foreach (Curve curve in curves_base)
            {
                edgeIds_Bottom.Add(builder.AddEdge(BRepBuilderEdgeGeometry.Create(curve)));
            }
            foreach (Curve curve in curves_new)
            {
                edgeIds_Top.Add(builder.AddEdge(BRepBuilderEdgeGeometry.Create(curve)));
            }
            foreach (Curve curve in curves_side)
            {
                edgeIds_Side.Add(builder.AddEdge(BRepBuilderEdgeGeometry.Create(curve)));
            }

            // 3. loop id
            BRepBuilderGeometryId loopId_Bottom = builder.AddLoop(faceId_Bottom);
            BRepBuilderGeometryId loopId_Top = builder.AddLoop(faceId_Top);
            List<BRepBuilderGeometryId> loopIds_Side = [];
            foreach (BRepBuilderGeometryId faceId_side in faceIds_side)
            {
                loopIds_Side.Add(builder.AddLoop(faceId_side));
            }

            // 4. coedge

            int curveCount = curves_base.Count;

            // bottom
            {
                for (int i = curveCount - 1; i >= 0; i--)
                {
                    builder.AddCoEdge(loopId_Bottom, edgeIds_Bottom[i], true);
                }
                builder.FinishLoop(loopId_Bottom);
                builder.FinishFace(faceId_Bottom);
            }

            // top
            {
                for (int i = 0; i < curveCount; i++)
                {
                    builder.AddCoEdge(loopId_Top, edgeIds_Top[i], false);  // 面需要翻转，组成面的轮廓，需要遵循右手定则
                }
                builder.FinishLoop(loopId_Top);
                builder.FinishFace(faceId_Top);
            }

            // side
            {
                for (int i = 0, loopCount = loopIds_Side.Count; i < loopCount; i++)
                {
                    BRepBuilderGeometryId loopId = loopIds_Side[i];
                    builder.AddCoEdge(loopId, edgeIds_Bottom[i], false);
                    builder.AddCoEdge(loopId, edgeIds_Side[(i + 1) % loopCount], false);
                    builder.AddCoEdge(loopId, edgeIds_Top[i], true);
                    builder.AddCoEdge(loopId, edgeIds_Side[i], true);

                    builder.FinishLoop(loopId);
                }
                for (int i = 0; i < faceIds_side.Count; i++)
                {
                    builder.FinishFace(faceIds_side[i]);
                }
            }

            BRepBuilderOutcome buildResult = builder.Finish();

            TaskDialog.Show("BrepBuild", buildResult.ToString());

            #endregion Brep

            Solid solid = null;

            //if (buildResult == BRepBuilderOutcome.Success)
            //{
            solid = builder.GetResult();
            //}
            //else
            //{
            //    TaskDialog.Show("BrepBuild", "创建失败");
            //}

            return solid;
        }
    }
}