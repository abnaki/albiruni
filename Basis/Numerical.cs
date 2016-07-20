using System;
using System.Collections.Generic;
using System.Linq;

namespace Abnaki.Albiruni
{
    public static class Numerical
    {
        /// <summary>
        /// Of the non-null args, return the extreme
        /// </summary>
        /// <param name="sign">1 implies max, -1 implies min
        /// </param>
        public static decimal? NullableExtreme(decimal? a, decimal? b, int sign)
        {
            if (a.HasValue)
            {
                if (b.HasValue)
                {
                    if (b.Value.CompareTo(a.Value) == sign)
                        return b;
                }
                return a;
            }
            else if (b.HasValue)
            {
                return b;
            }
            return null;
        }

        public static DateTime? MinNullableTime(DateTime? dt, DateTime? ptime)
        {
            if (dt.HasValue)
            {
                if (ptime.HasValue && ptime.Value < dt.Value)
                    return ptime;

                return dt;
            }
            return ptime;
        }

        public static DateTime? MaxNullableTime(DateTime? dt, DateTime? ptime)
        {
            if (dt.HasValue)
            {
                if (ptime.HasValue && dt.Value < ptime.Value)
                    return ptime;

                return dt;
            }
            return ptime;
        }



    }
}
