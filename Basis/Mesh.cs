using System;
using System.Collections.Generic;
using System.Linq;

namespace Abnaki.Albiruni
{
    /// <summary>
    /// Represents size of a mesh for coordinate system in degrees, adjustable by power of 2.
    /// </summary>
    /// <remarks>
    /// 
    /// From WolframAlpha, east-west distances for longitudinal 360 degrees * 2^-k:
    /// k      at equator      at 45 latitude
    /// 14     2446 m          1730 m
    /// 15     1223 m           865 m
    /// 16      611 m           432 m
    /// 17      306 m           212 m
    /// 18      153 m           108 m
    /// 19       76 m            54 m
    /// 
    /// Also see
    /// https://en.wikipedia.org/wiki/Earth_physical_characteristics_tables
    /// Equatorial and meridional measurements agree to 3 sig figs.
    /// </remarks>
    public struct Mesh
    {
        public Mesh(int pwr) : this()
        {
            power = 0; // temp

            this.Power = pwr;
        }

        /// <summary>
        /// Degrees of latitute or longitude
        /// </summary>
        public decimal Delta { get; private set; }

        int power;

        /// <summary>
        /// Mesh Delta is 360 degrees divided by Power of 2.
        /// </summary>
        public int Power
        {
            get
            {
                return power;
            }
            set
            {
                power = value;
                Delta = 360 / (decimal)Math.Pow(2d, power);
            }
        }

        public override string ToString()
        {
            return GetType().Name + " Power=" + Power + " Delta=" + Delta;
        }
    }
}
