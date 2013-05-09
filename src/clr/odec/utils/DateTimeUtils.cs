using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;

namespace de.mastersign.odec.utils
{
    internal class DateTimeUtils
    {
        public static string FormatDateTime(DateTime value)
        {
            return value.ToString("O", CultureInfo.InvariantCulture);
        }

        public static DateTime ParseDateTime(string value)
        {
            return DateTime.Parse(value, CultureInfo.InvariantCulture);
        }
    }
}
