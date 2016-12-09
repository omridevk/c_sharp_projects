using System;
using System.IO;
using System.Linq;
using System.Xml.Linq;
using System.Collections.Generic;   

namespace OTTProject
{
    /// <summary>
    /// Specific class to generate XTVD Class. 
    /// </summary>
    public class XTVDGenerator : Generator
    {

        /// <summary>
        /// Root element to loaded XML
        /// </summary>
        private XElement _RootElement;


        /// <summary>
        /// hold the output XML file name
        /// </summary>
        private string _OutputFilePath;

        private string _InputFilePath;

        /// <summary>
        /// Generated root element name
        /// </summary>
        public override string RootName
        {
            get
            {
                return "XTVD";
            }
        }

        /// <summary>
        /// Reference to the rootElement.
        /// </summary>
        public override XElement RootElement
        {
            get
            {
                return _RootElement;
            }
        }

        /// <summary>
        /// set the file name and namespace attributes
        /// </summary>
        /// <param name="file"></param>
        public XTVDGenerator(string file)
        {
            _InputFilePath = file;
            _OutputFilePath = Helpers.GeneratePath(file, "_XTVD");
        }

        /// <summary>
        /// Get a list that contains a tuple of string and lamdas/delegates that generates 
        /// </summary>
        /// <returns></returns>
        public override IList<Tuple<string, Func<XElement, XElement>>> GetGenerators()
        {
            return new List<Tuple<string, Func<XElement, XElement>>>
            {
                new Tuple<string, Func<XElement, XElement>>("schedules", GenerateteSchedules),
                new Tuple<string, Func<XElement, XElement>>("programs", GeneratePrograms)
            };
        }

        /// <summary>
        /// Public API
        /// Start the generate process, call parent generate
        /// can do more stuff if needed.
        /// </summary>
        public override void Generate()
        {
            _RootElement = Load(_InputFilePath);
            IEnumerable<XElement> programmes = GetProgrammes();
            XDocument generated = Generate(programmes);
            Logger.Log("saving new(or updating existing one) file: " + _OutputFilePath);
            try
            {
                generated.Save(_OutputFilePath);
            }
            catch (Exception e)
            {
                Logger.Error("error saving file: " + e.Message);
            }
        }

        /// <summary>
        /// Get all programmes elements from the original XML.
        /// </summary>
        /// <returns></returns>
        private IEnumerable<XElement> GetProgrammes()
        {
            Logger.Info("fetching programmes from original XML");
            return from programme in _RootElement.Elements(NameSpace + "programme")
                   orderby (string)programme.Attribute("start")
                   select programme;
        }

        public override string ToString()
        {
            return Path.GetFileName(_OutputFilePath);
        }
       
        /// <summary>
        /// Generate a program element with all it's children
        /// </summary>
        /// <param name="program"></param>
        /// <returns></returns>
        private XElement GeneratePrograms(XElement program)
        {
            Logger.Info("generating program XML for program: " + program.Attribute("external_id"));
            IEnumerable<XElement> metaTags = GenerateMeta(program);
            return new XElement("program",
                new XAttribute("id", (string)program.Attribute("external_id")),
                new XElement("series"),
                new XElement("title", (string)program.Element(NameSpace + "title")),
                new XElement("subtitle"),
                new XElement("description", (string)program.Element(NameSpace + "desc")),
                new XElement("showType"),
                new XElement("year"),
                new XElement("mpaaRating"),
                metaTags
            );

        }

        /// <summary>
        /// Generate meta tag for a given program
        /// </summary>
        /// <param name="program"></param>
        /// <returns></returns>
        private IEnumerable<XElement> GenerateMeta(XElement program)
        {
            Logger.Info("generating tags from MetaTags for program: " + program.Attribute("external_id"));
            IEnumerable<XElement> metas = program.Elements(NameSpace + "metas");
            IList<XElement> transformed = new List<XElement>();
            foreach (var meta in metas)
            {
                string metaType = (string)meta.Element(NameSpace + "MetaType");
                string[] words = metaType.Split(' ');
                words[0] = Helpers.FirstLetterToLower(words[0]);
                for (int i = 1; i < words.Length; i++)
                {
                    words[i] = Helpers.FirstLetterToUpper(words[i]);
                }
                transformed.Add(
                    new XElement(String.Join("", words), (string)meta.Element(NameSpace + "MetaValues"))
                );

            }
            return transformed;
        }

        private XElement GenerateteSchedules(XElement program)
        {
            Logger.Info("generating schedules for program: " + program.Attribute("external_id"));
            string start = Helpers.FormatTime(
                (string)program.Attribute("start"),
                XTVDTimeFormatEnum.XTVD_INPUT_TIME_FORMAT,
                XTVDTimeFormatEnum.XTVD_OUTPUT_TIME_FORMAT
            ),
            duration = Helpers.GetDuration(
                program,
                XTVDTimeFormatEnum.XTVD_INPUT_TIME_FORMAT,
                XTVDTimeFormatEnum.XTVD_DURATION_TIME_FORMAT
            ),
            id = (string)program.Attribute("external_id");
            return new XElement("schedule",
                    new XAttribute("program", id),
                    new XAttribute("time", start),
                    new XAttribute("duration", duration)
                    );
        }
        
    }
}