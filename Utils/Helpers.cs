using System;
using System.IO;
using System.Xml.Linq;
using System.Globalization;
using OTTProject.Utils.Logging;

namespace OTTProject.Utils
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

        public static string GeneratePath(string file, string postfix)
        {
            string fileName = Path.GetFileNameWithoutExtension(file) + postfix + Path.GetExtension(file);
            string path = Directory.GetCurrentDirectory();
            Directory.CreateDirectory(Path.Combine(path, "output"));
            path = Path.Combine(path,"output", fileName);
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
            Logger.Error("error parsing time string: " + time + "input pattern: " + inputPattern);
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
            Logger.Error(message);
            return "";
        }

        public static XElement FirstOrCreate(string name, XElement root)
        {
            XElement result = root.Element(name);
            if (result != null)
            {
                return result;
            }
            Logger.Log("element: " + name + " not found, creating new");
            root.Add(new XElement(name));
            return root.Element(name);

        }
    }
}
