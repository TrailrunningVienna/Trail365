using System;
using System.IO;

namespace Trail365.Internal
{
    public static class OutputFormatterExtension
    {
        //Lessons learned about readability
        //different cultures have different symbols for decimal and thousand separators, hard to read other cultures, and i prefer to noct customize culture on my dev machine
        //3 characters after a symbol brings me in doubth it is decimal or thousands ?
        // solution use only 1-2 characters for decimal then it should be clear that it is not thousand because thousend MUST have 3 following characters

        public const long BitsInByte = 8;
        public const long BytesInKiloByte = 1024;
        public const long BytesInMegaByte = 1048576;
        public const long BytesInGigaByte = 1073741824;
        public const long BytesInTeraByte = 1099511627776;
        public const long BytesInPetaByte = 1125899906842624;

        public const string BitSymbol = "b";
        public const string ByteSymbol = "B";
        public const string KiloByteSymbol = "KB";
        public const string MegaByteSymbol = "MB";
        public const string GigaByteSymbol = "GB";
        public const string TeraByteSymbol = "TB";
        public const string PetaByteSymbol = "PB";

        public const string BitShortname = "bit";
        public const string ByteShortname = "Byte";
        public const string KiloByteShortname = "KByte";
        public const string MegaByteShortname = "MByte";
        public const string GigaByteShortname = "GByte";
        public const string TeraByteShortname = "TByte";
        public const string PetaByteShortname = "PByte";



        public static string ToFormattedBandwidth(this double bytesPerSecond)
        {
            double rate = bytesPerSecond;
            string unit = ByteShortname + "/Sec";

            if (rate >= 1000)
            {
                rate = rate / 1024;
                unit = KiloByteShortname + "/Sec";
            }

            if (rate >= 1000)
            {
                rate = rate / 1024;
                unit = MegaByteShortname + "/Sec";
            }

            if (rate >= 1000)
            {
                rate = rate / 1024;
                unit = GigaByteShortname + "/Sec";
            }

            if (rate >= 1000)
            {
                rate = rate / 1024;
                unit = TeraByteShortname + "/Sec";
            }

            if (rate >= 1000)
            {
                rate = rate / 1024;
                unit = PetaByteShortname + "/Sec";
            }

            return string.Format("{0:0.##} {1}", rate, unit);
        }


        public static string ToFormattedBandwidth(this TimeSpan duration, long bytes)
        {
            double rate = bytes / duration.TotalSeconds;
            return ToFormattedBandwidth(rate);
        }

        public static string ToFormattedDuration(this System.Diagnostics.Stopwatch stopwatch)
        {
            if (stopwatch == null)
            {
                throw new ArgumentNullException(nameof(stopwatch));
            }
            return ToFormattedDuration(stopwatch.Elapsed);
        }

        public static string ToFormattedTimeString(this DateTime now)
        {
            return now.ToString("HH:mm:ss.ffff");
        }

        public static string ToFormattedDuration(this TimeSpan elapsed)
        {
            if (elapsed == TimeSpan.MaxValue)
            {
                return "N/A (MaxValue)";
            }
            if (elapsed == TimeSpan.MinValue)
            {
                return "N/A (MinValue)";
            }


            if (elapsed.TotalSeconds > 999) //thousands point!
            {
                if (elapsed.TotalHours < 24)
                {
                    return string.Format(@"{0:hh\:mm\:ss} Hours", elapsed); //full timestamp!
                }
                return string.Format(@"{0:dd\.hh\:mm\:ss} Days", elapsed); //full timestamp!
            }

            if (elapsed.TotalSeconds > 99)
            {
                return string.Format(@"{0:mm\:ss} Minutes", elapsed);
            }



            double duration = elapsed.Ticks;
            string unit = nameof(elapsed.Ticks);
            if (elapsed.TotalMilliseconds > 0.1)
            {
                duration = elapsed.TotalMilliseconds;
                unit = "ms";
            }

            if (elapsed.TotalMilliseconds >= 1000)
            {
                duration = elapsed.TotalSeconds;
                unit = "Seconds";
            }
            //0.## 
            //n0
            return string.Format("{0:0.##} {1}", duration, unit).Trim();
        }

        public static string ToFormattedFileSize(this FileInfo file)
        {
            if (file == null)
            {
                throw new ArgumentNullException(nameof(file));
            }

            if (file.Exists == false)
            {
                return "N/A";
            }
            long size = file.Length;
            return ToFormattedFileSize(file.Length);
        }

        public static string ToFormattedFileSize(this long size)
        {
            double d = size;
            return ToFormattedFileSize(d);
        }


        public static string ToFormattedFileSize(this long size, int digits)
        {
            double d = size;
            return ToFormattedFileSize(d, digits);
        }


        public static string ToFormattedFileSize(this double size)
        {
            return ToFormattedFileSize(size, 2);
        }

        public static string ToFormattedFileSize(this double size, int digits)
        {
            double original = size;
            double value = original;
            string unit = ByteShortname;

            if (original > BytesInPetaByte)
            {
                value = original / BytesInPetaByte;
                unit = PetaByteShortname;
            }
            else

            if (original > BytesInTeraByte)
            {
                value = original / BytesInTeraByte;
                unit = TeraByteShortname;
            }
            else
            if (original > BytesInGigaByte)
            {
                value = original / BytesInGigaByte;
                unit = GigaByteShortname;
            }
            else
            if (original > (BytesInMegaByte))
            {
                value = original / (1024 * 1024);
                unit = MegaByteShortname;
            }
            else
            {
                if (original >= BytesInKiloByte)
                {
                    value = original / 1024;
                    unit = KiloByteShortname;
                }
            }

            string pattern = "{0:0.##} {1}"; //digits =2;

            if (digits == 1)
            {
                pattern = "{0:0.#} {1}";
            }
            if (digits == 3)
            {
                pattern = "{0:0.###} {1}";
            }
            if (digits == 4)
            {
                pattern = "{0:0.####} {1}";
            }

            if (digits < 1)
            {
                pattern = "{0:0} {1}";
            }

            return string.Format(pattern, value, unit);
        }


        public static string ToFormattedFileSize(this string path)
        {
            if (path == null)
            {
                throw new ArgumentNullException(nameof(path));
            }

            FileInfo fi = new FileInfo(path);
            return fi.ToFormattedFileSize();
        }

    }
}
