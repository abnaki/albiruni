using System;
using System.Collections.Generic;

namespace Abnaki.Albiruni.Providers
{
    /// <summary>
    /// Essential commonly recorded data
    /// </summary>
    /// <remarks>
    /// Does not contain nonessential detail that users can explore in a separate 
    ///  application on the original source file.
    /// </remarks>
    public interface IPoint
    {
        /// <summary>degrees</summary>
        decimal Latitude { get; }

        /// <summary>degrees</summary>
        decimal Longitude { get; }

        /// <summary>meters</summary>
        decimal? Elevation { get; }

        /// <summary>when visited</summary>
        DateTime? Time { get; }
    }
}
