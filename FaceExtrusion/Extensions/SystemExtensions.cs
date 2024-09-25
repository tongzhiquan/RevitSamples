using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FaceExtrusion.Extensions
{
    internal static class SystemExtensions
    {
        #region Double

        public static double RadToDeg(this double rad)
        {
            return rad * 180d / Math.PI;
        }

        public static double DegToRad(this double deg)
        {
            return deg * Math.PI / 180d;
        }

        public static double Abs(this double d)
        {
            return Math.Abs(d);
        }

        public static bool IsBetween(this double d, double min, double max, bool includeMin, bool includeMax, int digit = 7)
        {
            double _d = Math.Round(d, digit);
            double _min = Math.Round(min, digit);
            double _max = Math.Round(max, digit);

            if (includeMin && includeMax)
                return _d >= _min && _d <= _max;
            else if (includeMin && !includeMax)
            {
                return _d >= _min && _d < _max;
            }
            else if (!includeMin && includeMax)
            {
                return _d > _min && _d <= _max;
            }
            else
            {
                return _d > _min && _d < _max;
            }
        }

        public static double Clamp(this double d, double min, double max)
        {
            return Math.Max(min, Math.Min(max, d));
        }

        public static bool Approximate(this double d, double value, double tolerance = 1e-6)
        {
            return Math.Abs(d - value) <= tolerance;
        }

        public static bool IsAlmostEqual(this double source, double value, double tolerance = 1e-6)
        {
            return Math.Abs(source - value) <= tolerance;
        }

        public static double Round(this double d, int digits = 6)
        {
            return Math.Round(d, digits);
        }

        #endregion Double


    }

}
