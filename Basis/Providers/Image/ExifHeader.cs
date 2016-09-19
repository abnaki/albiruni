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

            IEnumerable<ExifItem> gpsItems = xitems.Where(x => (1 <= x.Id && x.Id <= 7) || x.Id == 29)
                .OrderByDescending(x => x.Id) // hemispheres will be handled after numbers
                .ToList();

            PurePoint p = new PurePoint();

            TimeSpan? gpsUtcTimeOfDay = null;
            DateTime? gpsUtcDate = null;

            // minimal necessities
            bool validLatRef = false, validLongRef = false, validLat = false, validLon = false;

            foreach ( ExifItem x in gpsItems )
            {
                switch ( x.Id )
                {
                    case 1:    // GPSLatitudeRef  N or S
                        if (Convert.ToString(x.Value).StartsWith("S"))
                        {
                            p.Latitude *= -1;
                        }
                        validLatRef = true;
                        break;
                    case 2:    // GPSLatitude
                        p.Latitude = DegreesFromItem(x);
                        validLat = true;
                        break;
                    case 3:    // GPSLongitudeRef  E or W
                        if (Convert.ToString(x.Value).StartsWith("W"))
                        {
                            p.Longitude *= -1;
                        }
                        validLongRef = true;
                        break;
                    case 4:    // GPSLongitude
                        p.Longitude = DegreesFromItem(x);
                        validLon = true;
                        break;

                    case 5: // GPSAltitudeRef  byte[1] of which 0 implies positive, 1 implies negative
                        if (p.Elevation.HasValue)
                        {
                            byte[] ba = (byte[])x.Value;
                            if ( Convert.ToInt32(ba[0]) == 1 )
                                p.Elevation *= -1;
                        }

                        break;

                    case 6: // GPSAltitude meters
                        if (x.Value != null )
                            p.Elevation = DecimalFromURational((URational)x.Value);
                        break;

                    // GPSSTimeStamp  Id=7   expect x.Value to be URational[3] of hours, minutes, seconds, UTC
                    case 7:
                        URational[] uar = (URational[])x.Value;
                        gpsUtcTimeOfDay = TimeSpan.FromHours(DoubleFromURational(uar[0]))
                             + TimeSpan.FromMinutes(DoubleFromURational(uar[1]));
                        TimeSpan tsec = TimeSpan.FromSeconds(DoubleFromURational(uar[2]));
                        if (tsec.TotalMinutes < 1)
                        {
                            gpsUtcTimeOfDay += tsec;
                        }
                        else
                        {
                            // maybe some cameras have the denominator wrong, 
                            // e.g. iphone 4 has denominator=1 yet it appears it should be 100.
                            Abnaki.Windows.AbnakiLog.Comment("GPSSTimeStamp has bad seconds " + uar[2], fi.Name);
                        }
                        break;
                   
                   // GPSDateStamp   Id=29  x.Value is string, believed to be UTC
                    case 29:
                        DateTime dtgps;
                        string sdt = StringFromExif(x.Value);
                        if (DateTime.TryParseExact(sdt, "yyyy:MM:dd",CultureInfo.InvariantCulture, DateTimeStyles.AdjustToUniversal, out dtgps))
                        {
                            gpsUtcDate = dtgps;
                        }

                        break;
                }
            }

            if ( gpsUtcDate.HasValue && gpsUtcTimeOfDay.HasValue )
            {
                p.Time = gpsUtcDate.Value + gpsUtcTimeOfDay.Value;
                p.TimeReliable = true;
            }

            // efforts to salvage other non-GPS data
            if (gpsItems.Any() && false == p.Time.HasValue)
            {
                ExifItem xtime = xitems.FirstOrDefault(x => x.Id == 36867); // DateTimeOriginal
                if (xtime != null)
                {
                    string sdate = StringFromExif(xtime.Value);
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
                            // lacking GPSDateStamp, this is the last hope for proper UTC date
                            DateTime dtApproxUtc = tz.ToUniversalTime(dt);
                            if (gpsUtcTimeOfDay.HasValue)
                                p.Time = dtApproxUtc.Date + gpsUtcTimeOfDay.Value; // supposed to be best
                            else
                                p.Time = dtApproxUtc;
                        }

                    }
                }
            }

            if (validLat && validLatRef && validLon && validLongRef)
            {
                singlePoint = p;
            }
        }

        static string StringFromExif(object v)
        {
            // maybe photo.exif should exclude the ending 0 char:
            return new string(Convert.ToString(v).TakeWhile(c => c != 0).ToArray());
        }

        static decimal DegreesFromItem(ExifItem x)
        {
            return DegreesFromArray((URational[])x.Value);
        }

        static decimal DecimalFromURational(URational r, int furtherDivideBy = 1)
        {
            return decimal.Divide((decimal)r.Numerator, (decimal)r.Denominator * (decimal)furtherDivideBy);
        }

        static double DoubleFromURational(URational r, int furtherDivideBy = 1)
        {
            return (double)DecimalFromURational(r, 1);
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
            // if users demand it, this could yield return singlePoint.
            get { yield break; }
        }

        IEnumerable<ITrack> IFile.Tracks
        {
            // an image file will be considered like a frame of a video:  
            // and because a video will have a track, 
            // a single image will too, for consistency.
            get
            {
                if (singlePoint == null)
                    yield break;
                else
                    yield return new PureTrack()
                    {
                        Points = new IPoint[] { singlePoint }
                    };
            }
        }

        IEnumerable<IRoute> IFile.Routes
        {
            get { yield break; }
        }
    }
}
