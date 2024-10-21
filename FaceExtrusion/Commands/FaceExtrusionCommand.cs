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
            TaskDialog.Show("FaceType", $"{face.GetType().Name}");

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
            // log face info
            UV faceBoundingCenter = face.GetBoundingCenter();
            Transform faceDerivatives = face.ComputeDerivatives(faceBoundingCenter);
            Log.Debug($"Face Type: {face.GetType().Name}");
            Log.Debug($"Center UV: {faceBoundingCenter}");
            Log.Debug($"Origin: {faceDerivatives.Origin}");
            Log.Debug($"BaseX: {faceDerivatives.BasisX}, BaseY: {faceDerivatives.BasisY}, BaseZ: {faceDerivatives.BasisZ}\r\n");

            // TODO：方向有要求，需要是向外侧
            extrusionDir = extrusionDir.Normalize();
            Transform transform = Transform.CreateTranslation(extrusionDir * extrusionDist);

            #region 基础数据

            SurfaceData surfaceData = SurfaceData.Create(face, transform);

            // 面
            Surface surface_base = face.GetSurface();
            Surface surface_new = surfaceData.Surface;
            Log.Debug($"base surface：{BRepBuilder.IsPermittedSurfaceType(surface_base)}");
            Log.Debug($"new surface：{BRepBuilder.IsPermittedSurfaceType(surface_new)}");

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
                Log.Debug($"surfaces_side {i}：{BRepBuilder.IsPermittedSurfaceType(surface_side)}");
            }

            #endregion 基础数据

            #region Brep

            // 添加BRep

            BRepBuilder builder = new BRepBuilder(BRepType.Solid);
            builder.SetAllowShortEdges();

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
                Log.Debug($"========================Botton Face========================");
                for (int i = curveCount - 1; i >= 0; i--)
                {
                    this.LogSurfaceAndCurve(surface_base, curves_base[i]);

                    builder.AddCoEdge(loopId_Bottom, edgeIds_Bottom[i], true); // 面需要翻转，组成面的轮廓，需要遵循右手定则
                }
                builder.FinishLoop(loopId_Bottom);
                builder.FinishFace(faceId_Bottom);
            }

            // top
            {
                Log.Debug($"========================Top Face========================");
                for (int i = 0; i < curveCount; i++)
                {
                    this.LogSurfaceAndCurve(surface_new, curves_new[i]);

                    builder.AddCoEdge(loopId_Top, edgeIds_Top[i], false);
                }
                builder.FinishLoop(loopId_Top);
                builder.FinishFace(faceId_Top);
            }

            // side
            {
                Log.Debug($"========================Side Face========================");
                for (int i = 0, loopCount = loopIds_Side.Count; i < loopCount; i++)
                {
                    Log.Debug($"===========Side Face {i}===========");
                    Log.Debug($"curves_base {i}");
                    this.LogSurfaceAndCurve(surfaces_side[i], curves_base[i]);
                    Log.Debug($"curves_side {(i + 1) % loopCount}");
                    this.LogSurfaceAndCurve(surfaces_side[i], curves_side[(i + 1) % loopCount]);
                    Log.Debug($"curves_new {i}");
                    this.LogSurfaceAndCurve(surfaces_side[i], curves_new[i]);
                    Log.Debug($"curves_side {i}");
                    this.LogSurfaceAndCurve(surfaces_side[i], curves_side[i]);

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

            builder.AllowRemovalOfProblematicFaces();
            bool removed = builder.RemovedSomeFaces();
            builder.Finish();

            TaskDialog.Show("BrepBuild", $"removed: {removed}, builder: {builder.IsResultAvailable()}");

            #endregion Brep

            Solid solid = null;

            //if (builder.IsResultAvailable())
            //{
            solid = builder.GetResult();
            //}
            //else
            //{
            //    TaskDialog.Show("BrepBuild", "创建失败");
            //}

            return solid;
        }

        private void LogSurfaceAndCurve(Surface surface, Curve curve)
        {
            Log.Debug($"Surface Type: {surface.GetType().Name}");

            IList<XYZ> points = curve.Tessellate();
            Log.Debug($"Curve Type: {curve.GetType().Name}\t\tCurve Points Count: {points.Count}");
            for (int j = 0; j < points.Count; j++)
            {
                XYZ point = points[j];
                try
                {
                    surface.Project(point, out UV uv, out double dis);
                    Log.Debug($"Point [{j}]: {point}\tUV: {uv}\t  Dis: {dis.Round()}");
                    if (dis.Round().Abs() > 1e-6) { Log.Error("\t\tThe Curve did not fall on the surface!"); }
                }
                catch
                {
                    Log.Debug($"Point [{j}]: {point}\t\t can't ptoject");
                }
            }
        }
    }
}