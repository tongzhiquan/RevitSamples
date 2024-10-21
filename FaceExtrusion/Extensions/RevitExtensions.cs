using Autodesk.Revit.DB;

namespace FaceExtrusion.Extensions
{
    internal static class RevitExtensions
    {
        #region Face
        public static UV GetBoundingCenter(this Face face)
        {
            BoundingBoxUV boundBox = face.GetBoundingBox();
            UV min = boundBox.Min;
            UV max = boundBox.Max;
            UV center = (min + max) / 2;
            return center;
        }

        #endregion
    }
}
