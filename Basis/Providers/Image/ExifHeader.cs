using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;

using photo.exif;  // use Abnaki fork, or an equally good build by fraxedas after July 24, 2016.
using Afk.ZoneInfo;
using System.Globalization; 

namespace Abnaki.Albiruni.Providers.Image
{
    /// <summary>
    /// Designed for GPS data in a file with a EXIF 2.2 header, such as images
    /// </summary>
    class ExifHeader : IFile
    {
        public void Read(FileInfo fi)
        {
            if (!fi.Exists)
                throw new FileNotFoundException("Failed to find " + fi.FullName);

            photo.exif.Parser parser = new photo.exif.Parser();
            IEnumerable<ExifItem> xitems = parser.Parse(fi.FullName).ToList();

            IEnumerable<ExifItem> gpsItems = xitems.Where(x => 1 <= x.Id && x.Id <= 4)
                .OrderByDescending(x => x.Id) // hemispheres will be handled after numbers
                .ToList();

            PurePoint p = new PurePoint();

            foreach ( ExifItem x in gpsItems )
            {
                switch ( x.Id )
                {
                    case 1:    // GPSLatitudeRef  N or S
                        if (Convert.ToString(x.Value).StartsWith("S"))
                            p.Latitude *= -1;
                        break;
                    case 2:    // GPSLatitude
                        p.Latitude = DegreesFromItem(x);
                        break;
                    case 3:    // GPSLongitudeRef  E or W
                        if (Convert.ToString(x.Value).StartsWith("W"))
                            p.Longitude *= -1;
                        break;
                    case 4:    // GPSLongitude
                        p.Longitude = DegreesFromItem(x); 
                        break;

                    case 5: // GPSAltitudeRef i.e. 0 implies positive, 1 implies negative
                        if (p.Elevation.HasValue && Convert.ToUInt32(x.Value) == 1 )
                            p.Elevation *= -1;

                        break;

                    case 6: // GPSAltitude meters
                        if (x.Value != null )
                            p.Elevation = DecimalFromURational((URational)x.Value);
                        break;
                }
            }

            // also useful, but not found in sample jpg files:  
            // GPSSTimeStamp  Id=7   expect x.Value to be URational[3] of hours, minutes, seconds, UTC
            // GPSDateStamp   Id=29  x.Value is string, believed to be UTC

            // efforts to salvage other non-GPS data
            if (gpsItems.Any() && false == p.Time.HasValue)
            {
                ExifItem xtime = xitems.FirstOrDefault(x => x.Id == 36867); // DateTimeOriginal
                if (xtime != null)
                {
                    // maybe photo.exif should exclude the ending 0 char:
                    string sdate = new string(Convert.ToString(xtime.Value).TakeWhile(c => c != 0).ToArray());
                    // EXIF 2.3 ignores timezone entirely.
                    // Deduce from location, assuming camera clock is accurate as well.
                    DateTime dt;
                    GeoTimeZone.TimeZoneResult tzr = GeoTimeZone.TimeZoneLookup.GetTimeZone((double)p.Latitude, (double)p.Longitude);

                    if (false == string.IsNullOrEmpty(tzr.Result))
                    {
                        TzTimeZone tz = TzTimeInfo.GetZone(tzr.Result);

                        if (DateTime.TryParseExact(sdate, "yyyy:MM:dd HH:mm:ss",
                            CultureInfo.InvariantCulture, DateTimeStyles.AssumeLocal, out dt))
                        {
                            p.Time = tz.ToUniversalTime(dt);
                        }

                    }
                }
            }

            singlePoint = p;
        }

        static decimal DegreesFromItem(ExifItem x)
        {
            return DegreesFromArray((URational[])x.Value);
        }

        static decimal DecimalFromURational(URational r, int furtherDivideBy = 1)
        {
            return decimal.Divide((decimal)r.Numerator, (decimal)r.Denominator * (decimal)furtherDivideBy);
        }

        static decimal DegreesFromArray(URational[] dms)
        {
            URational deg = dms[0];
            URational m= dms[1];
            URational s = dms[2];
            return DecimalFromURational(deg) + DecimalFromURational(m, 60) + DecimalFromURational(s, 3600);
        }

        IPoint singlePoint;

        IEnumerable<IPoint> IFile.WayPoints
        {
            get { yield break; }
        }

        IEnumerable<IPoint> IFile.TrackPoints
        {
            // an image file will be considered like a frame of a video:  
            // and because a video will have a track, 
            // a single image will too, for consistency.
            get { yield return singlePoint; }
        }

        IEnumerable<IPoint> IFile.RoutePoints
        {
            get { yield break; }
        }
    }
}
