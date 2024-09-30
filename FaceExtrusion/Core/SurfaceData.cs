using Autodesk.Revit.DB;

namespace FaceExtrusion.Core
{
    internal class SurfaceData
    {
        public Surface Surface { get; set; }
        public List<List<Curve>> Curveloops { get; set; }
        public bool OrientationMatchesSurfaceOrientation { get; set; }

        public SurfaceData(Surface surface, List<List<Curve>> curveloops, bool orientationMatches)
        {
            this.Surface = surface;
            this.Curveloops = curveloops;
            this.OrientationMatchesSurfaceOrientation = orientationMatches;
        }

        #region Create With Face

        public static SurfaceData Create(Face face, Transform transform)
        {
            if (face is PlanarFace planar) { return CreateWithPlanarFace(planar, transform); }
            else if (face is CylindricalFace cylindrical) { return CreateWithCylindricalFace(cylindrical, transform); }
            else if (face is ConicalFace conical) { return CreateWithConicalFace(conical, transform); }
            else if (face is RevolvedFace revolved) { return CreateWithRevolvedFace(revolved, transform); }
            else if (face is RuledFace ruled) { return CreateWithRuledFace(ruled, transform); }
            //else if (face is HermiteFace hermite) { return CreateWithHermiteFace(hermite, transaction); }
            else { return null; }
        }

        public static SurfaceData CreateWithPlanarFace(PlanarFace face, Transform transform)
        {
            // surface
            Plane surface = face.GetSurface() as Plane;
            bool matches = face.OrientationMatchesSurfaceOrientation;

            XYZ origin = transform.OfPoint(surface.Origin);
            XYZ noral = transform.OfVector(surface.Normal);

            Plane plane = Plane.CreateByNormalAndOrigin(noral, origin);

            // curveloops

            List<List<Curve>> curveloops = RevitApi.TransformCurveloops(face, transform);

            return new SurfaceData(plane, curveloops, matches);
        }

        public static SurfaceData CreateWithCylindricalFace(CylindricalFace face, Transform transform)
        {
            // surface
            CylindricalSurface surface = face.GetSurface() as CylindricalSurface;
            bool matches = face.OrientationMatchesSurfaceOrientation;

            double radius = surface.Radius;

            XYZ origin = transform.OfPoint(surface.Origin);
            XYZ xDir = transform.OfVector(surface.XDir);
            XYZ yDir = transform.OfVector(surface.YDir);
            XYZ axis = transform.OfVector(surface.Axis);

            Log.Debug($"origin:{origin}, xDir:{xDir}, yDir:{yDir}, axis:{axis}");
            Log.Debug($"cross_xy: {xDir.CrossProduct(yDir)}");
            Log.Debug("\tused axis!\r\n");

            Frame frame = new Frame(origin, xDir, yDir, axis);

            CylindricalSurface cylindricalSurface = CylindricalSurface.Create(frame, radius);

            // curveloops

            List<List<Curve>> curveloops = RevitApi.TransformCurveloops(face, transform);

            return new SurfaceData(cylindricalSurface, curveloops, matches);
        }

        public static SurfaceData CreateWithConicalFace(ConicalFace face, Transform transform)
        {
            // surface
            ConicalSurface surface = face.GetSurface() as ConicalSurface;
            bool matches = face.OrientationMatchesSurfaceOrientation;

            XYZ origin = transform.OfPoint(surface.Origin);
            double angle = surface.HalfAngle;
            XYZ xDir = transform.OfVector(surface.XDir);
            XYZ yDir = transform.OfVector(surface.YDir);
            XYZ axis = transform.OfVector(surface.Axis);

            Log.Debug($"origin:{origin}, xDir:{xDir}, yDir:{yDir}, axis:{axis}");
            Log.Debug($"cross_xy: {xDir.CrossProduct(yDir)}");
            Log.Debug("\tused axis!\r\n");

            //Frame frame = new Frame(origin, xDir, yDir, xDir.CrossProduct(yDir));
            Frame frame = new Frame(origin, xDir, yDir, axis);

            ConicalSurface conicalSurface = ConicalSurface.Create(frame, angle);

            // curveloops

            List<List<Curve>> curveloops = RevitApi.TransformCurveloops(face, transform);

            return new SurfaceData(conicalSurface, curveloops, matches);
        }

        public static SurfaceData CreateWithRevolvedFace(RevolvedFace face, Transform transform)
        {
            // surface
            RevolvedSurface surface = face.GetSurface() as RevolvedSurface;
            bool matches = face.OrientationMatchesSurfaceOrientation;

            Curve curve = surface.GetProfileCurveInWorldCoordinates();
            Curve newCurve = curve.CreateTransformed(transform);

            XYZ origin = transform.OfPoint(surface.Origin);
            XYZ xDir = transform.OfVector(surface.XDir);
            XYZ yDir = transform.OfVector(surface.YDir);
            XYZ axis = transform.OfVector(surface.Axis);

            Log.Debug($"origin:{origin}, xDir:{xDir}, yDir:{yDir}, axis:{axis}");
            Log.Debug($"cross_xy: {xDir.CrossProduct(yDir)}");
            Log.Debug("\tused axis!\r\n");

            Frame frame = new Frame(origin, xDir, yDir, axis);

            Surface revolvedSurface = RevolvedSurface.Create(frame, newCurve);

            // curveloops

            List<List<Curve>> curveloops = RevitApi.TransformCurveloops(face, transform);

            return new SurfaceData(revolvedSurface, curveloops, matches);
        }

        // TODO: 存在反向问题
        public static SurfaceData CreateWithRuledFace(RuledFace face, Transform transform)
        {
            // surface
            RuledSurface surface = face.GetSurface() as RuledSurface;
            bool matches = face.OrientationMatchesSurfaceOrientation;

            Curve curve, curve1, curve2;
            XYZ point = null;

            curve1 = surface.GetFirstProfileCurve();
            curve2 = surface.GetSecondProfileCurve();

            if (surface.HasFirstProfilePoint()) { point = surface.GetFirstProfilePoint(); }
            if (surface.HasSecondProfilePoint()) { point = surface.GetSecondProfilePoint(); }

            Surface ruledSurface;

            if (point != null)
            {
                curve = curve1;
                curve ??= curve2;

                XYZ newPoint = transform.OfPoint(point);
                Curve newCurve = curve.CreateTransformed(transform);

                ruledSurface = RuledSurface.Create(newCurve, newPoint);
            }
            else
            {
                Curve newCurve1 = curve1.CreateTransformed(transform);
                Curve newCurve2 = curve2.CreateTransformed(transform);

                ruledSurface = RuledSurface.Create(newCurve1, newCurve2);
            }

            // curveloops

            List<List<Curve>> curveloops = RevitApi.TransformCurveloops(face, transform);

            return new SurfaceData(ruledSurface, curveloops, matches);
        }

        //public static SurfaceData CreateWithHermiteFace(HermiteFace face, Transform transform)
        //{
        //    // surface
        //    HermiteSurface surface = face.GetSurface() as HermiteSurface;
        //    bool matches = face.OrientationMatchesSurfaceOrientation;

        //    XYZ origin = transform.OfPoint(surface.Origin);
        //    XYZ xDir = transform.OfVector(surface.XDir);
        //    XYZ yDir = transform.OfVector(surface.YDir);
        //    XYZ zDir = transform.OfVector(surface.ZDir);

        //    Frame frame = new Frame(origin, xDir, yDir, zDir);

        //    Curve[] curves = surface.GetProfileCurves();
        //    Curve[] newCurves = curves.Select(c => c.CreateTransformed(transform)).ToArray();

        //    HermiteSurface hermiteSurface = HermiteSurface.Create(frame, newCurves);

        //    // curveloops

        //    List<List<Curve>> curveloops = TransformCurveloops(face, transform);

        //    return new SurfaceData(hermiteSurface, curveloops, matches);
        //}

        #endregion Create With Face
    }
}