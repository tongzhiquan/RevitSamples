using Autodesk.Revit.DB;

namespace SequentialSelector.Core
{
    /// <summary>
    ///     The class contains wrapping methods for working with the Revit API.
    /// </summary>
    public static class RevitApi
    {
        /// <summary>
        /// 更改元素的颜色和透明度
        /// </summary>
        /// <param name="element">元素</param>
        /// <param name="r">R, [0,255]</param>
        /// <param name="g">G, [0,255]</param>
        /// <param name="b">B, [0,255]</param>
        /// <param name="t">Transparency, [0,100]</param>
        /// <remarks>
        /// 这个方法在 SubTransaction 中执行。
        /// </remarks>
        public static void ChangeElementColor(Element element, [CanBeNull] Color faceColor, [CanBeNull] Color lineColor, int t)
        {
            Document document = element.Document;

            //using SubTransaction ts = new(document);
            using Transaction ts = new(document, "Change Elelment Color");
            ts.Start();

            OverrideGraphicSettings overrideGraphicSettings = ElementColorOverrideGraphicSettings(document, faceColor, lineColor, t);

            document.ActiveView.SetElementOverrides(element.Id, overrideGraphicSettings);

            ts.Commit();
        }

        private static OverrideGraphicSettings ElementColorOverrideGraphicSettings(Document document, [CanBeNull] Color faceColor, [CanBeNull] Color lineColor, int transparency)
        {
            FilteredElementCollector fillFilter = new FilteredElementCollector(document);
            fillFilter.OfClass(typeof(FillPatternElement));

            FillPatternElement fp = fillFilter.First(m => (m as FillPatternElement).GetFillPattern().IsSolidFill) as FillPatternElement;
            OverrideGraphicSettings overrideGraphicSettings = new OverrideGraphicSettings();

#if R18
            overrideGraphicSettings.SetProjectionFillPatternId(fp.Id);  // 使用这个弃用的方法，否则在Revit 2018中无效
            if (faceColor != null) overrideGraphicSettings.SetProjectionFillColor(faceColor);
            if (transparency >= 0 || transparency <= 100) overrideGraphicSettings.SetSurfaceTransparency(transparency);
            if (lineColor != null) overrideGraphicSettings.SetProjectionLineColor(lineColor);
            overrideGraphicSettings.SetCutFillPatternId(fp.Id);
            if (faceColor != null) overrideGraphicSettings.SetCutFillColor(faceColor);
#else
            overrideGraphicSettings.SetSurfaceForegroundPatternId(fp.Id);
            if (faceColor != null) overrideGraphicSettings.SetSurfaceForegroundPatternColor(faceColor);
            if (transparency >= 0 || transparency <= 100) overrideGraphicSettings.SetSurfaceTransparency(transparency);
            if (lineColor != null) overrideGraphicSettings.SetProjectionLineColor(lineColor);
            overrideGraphicSettings.SetCutForegroundPatternId(fp.Id);
            if (faceColor != null) overrideGraphicSettings.SetCutForegroundPatternColor(faceColor);
#endif
            return overrideGraphicSettings;
        }

        public static void ResetElementColor(Element element)
        {
            Document document = element.Document;

            //using SubTransaction ts = new(document);
            using Transaction ts = new(document, "Reset Elelment Color");
            ts.Start();

            document.ActiveView.SetElementOverrides(element.Id, new OverrideGraphicSettings());

            ts.Commit();
        }
    }
}