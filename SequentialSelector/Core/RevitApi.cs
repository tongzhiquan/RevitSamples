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
        [Obsolete]
        public static void ChangeElementColor(Element element, byte r, byte g, byte b, int t)
        {
            Document document = element.Document;

            using Transaction ts = new(document, "Change Element Color");
            ts.Start();

            OverrideGraphicSettings overrideGraphicSettings = ElementColorOverrideGraphicSettings(document, new Color(r, g, b), t);

            document.ActiveView.SetElementOverrides(element.Id, overrideGraphicSettings);

            ts.Commit();
        }

        [Obsolete]
        private static OverrideGraphicSettings ElementColorOverrideGraphicSettings(Document document, Color color, int transparency)
        {
            FilteredElementCollector fillFilter = new FilteredElementCollector(document);
            fillFilter.OfClass(typeof(FillPatternElement));

            FillPatternElement fp = fillFilter.First(m => (m as FillPatternElement).GetFillPattern().IsSolidFill) as FillPatternElement;
            OverrideGraphicSettings overrideGraphicSettings = new OverrideGraphicSettings();

#if R18
            // 三维
            overrideGraphicSettings.SetProjectionFillPatternId(fp.Id);  // 使用这个弃用的方法，否则在Revit 2018中无效
            overrideGraphicSettings.SetProjectionFillColor(color);

            overrideGraphicSettings.SetSurfaceTransparency(transparency);

            // 平面
            overrideGraphicSettings.SetCutFillPatternId(fp.Id);
            overrideGraphicSettings.SetCutFillColor(color);
#else
            overrideGraphicSettings.SetSurfaceForegroundPatternId(fp.Id);
            overrideGraphicSettings.SetSurfaceForegroundPatternColor(color);
            overrideGraphicSettings.SetSurfaceTransparency(transparency);
            overrideGraphicSettings.SetCutForegroundPatternId(fp.Id);
            overrideGraphicSettings.SetCutForegroundPatternColor(color);
#endif
            return overrideGraphicSettings;
        }
    }
}