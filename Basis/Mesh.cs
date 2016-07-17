using System;
using System.Collections.Generic;
using System.Linq;

namespace Abnaki.Albiruni
{
    /// <summary>
    /// Represents size of a mesh for coordinate system in degrees, adjustable by power of 2.
    /// </summary>
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
        /// Mesh Delta is 90 degrees divided by Power of 2.
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
                Delta = 90 / (decimal)Math.Pow(2d, power);
            }
        }
    }
}
