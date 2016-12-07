using System;
using System.IO;
using System.Xml.Linq;
using System.Globalization;

namespace OTTProject
{
    public static class Helpers
    {

        /// <summary>
        /// Delegate since the normal Func(Delegate/Lambda) does not allow ref/out type.
        /// </summary>
        /// <typeparam name="T">Input Type of the delegate</typeparam>
        /// <typeparam name="U">Output Type of the delegate</typeparam>
        /// <typeparam name="V">Return Type of the delegate</typeparam>
        /// <param name="input">Will be used as first parameter for the lambda</param>
        /// <param name="output">Will be used as the second parameter for the lambda</param>
        /// <returns></returns>
        public delegate V RefDelegate<T, U, V>(T input, ref U output);

        public static string FirstLetterToUpper(string str)
        {
            if (str == null)
            {
                return null;
            }
            if (str.Length > 1)
            {
                return char.ToUpper(str[0]) + str.Substring(1);
            }

            return str.ToUpper();
        }
        public static string FirstLetterToLower(string str)
        {
            if (str == null)
            {
                return null;
            }

            if (str.Length > 1)
            {
                return char.ToLower(str[0]) + str.Substring(1);
            }
            return str.ToLower();
        }

        public static string GetPath(string file, string postfix)
        {
            string path = "xml\\..\\..\\" + Path.GetFileNameWithoutExtension(file);
            path += postfix + Path.GetExtension(file);
            return path;
        }

        public static string FormatTime(string time, string inputPattern, string outputPattern)
        {
            DateTime dt;

            if (DateTime.TryParseExact(time, inputPattern, CultureInfo.InvariantCulture,
                           DateTimeStyles.None,
                           out dt))
            {
                return dt.ToString(outputPattern);
            }

            return "";
        }

        public static string GetDuration(XElement program, string timePattern, string durationPattern)
        {
            DateTime endDt;
            DateTime startDt = new DateTime();
            string end = (string)program.Attribute("stop");
            string start = (string)program.Attribute("start");
            bool result = (DateTime.TryParseExact(end, timePattern, CultureInfo.InvariantCulture,
                           DateTimeStyles.None,
                           out endDt) && DateTime.TryParseExact(start, timePattern, CultureInfo.InvariantCulture,
                           DateTimeStyles.None,
                           out startDt));


            if (result)
            {
                TimeSpan duration = endDt.Subtract(startDt);
                return duration.ToString(durationPattern);
            }
            string message = "error parsing the start/time program id: " + (string)program.Attribute("external_id");
            Logger.Log(message);
            return "";
        }
    }
}
