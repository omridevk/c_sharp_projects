using System;
using System.IO;
using System.Linq;
using System.Xml.Linq;
using OTTProject.Utils;
using OTTProject.Utils.Logging;
using System.Collections.Generic;



namespace OTTProject
{
    /// <summary>
    /// Specific class to generate XTVD Class. 
    /// </summary>
    public class XTVDGenerator : Generator
    {

        public XTVDGenerator(string file) : base(file) { }

        /// <summary>
        /// Root element to loaded XML
        /// </summary>
        private XElement _RootElement;

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

        public override IEnumerable<XElement> Programs
        {
            get
            {
                return _RootElement.Elements(NameSpace + "programme")
                    .OrderBy(program => (string)program.Attribute("start"))
                    .Select(program => program);
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
            set
            {
                _RootElement = value;
            }
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
        public override XDocument Generate()
        {
  
            XDocument generated = base.Generate();
            Logger.Info("done generating {0}, saved new file: {1}", InputPath, OutputPath);
            return generated;
        }

        public override string ToString()
        {
            return Path.GetFileName(OutputPath);
        }
       
        /// <summary>
        /// Generate a program element with all it's children
        /// </summary>
        /// <param name="program"></param>
        /// <returns></returns>
        private XElement GeneratePrograms(XElement program)
        {
            string id = (string) program.Attribute("external_id");
            string title = (string) program.Element(NameSpace + "title");
            string desc = (string)program.Element(NameSpace + "desc");
            Logger.Debug("generating program XML for program: {0}", id);
            IEnumerable<XElement> metaTags = GenerateMeta(program);
            return new XElement("program",
                new XAttribute("id", id),
                new XElement("series"),
                new XElement("title", title),
                new XElement("subtitle"),
                new XElement("description", desc),
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
            Logger.Debug("generating tags from MetaTags for program: {0}", (string) program.Attribute("external_id"));
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
            Logger.Debug("generating schedules for program: {0}", (string) program.Attribute("external_id"));
            
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