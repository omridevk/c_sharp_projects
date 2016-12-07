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
        private string _file;
        /// <summary>
        /// XML Namespace to use in queries! possible bugs if not added as prefix!
        /// </summary>
        private XNamespace _ns { get; }
        /// <summary>
        /// Generated root element name
        /// </summary>
        protected override string RootName
        {
            get
            {
                return "XTVD";
            }
        }
        protected override XElement RootElement
        {
            get
            {
                return _RootElement;
            }
        }

 

        public XTVDGenerator(string file)
        {
            _RootElement = Load(file);
            // could make ns and file static.
            _ns = (string) _RootElement.Attribute("xmlns");
            _file = Helpers.GetPath(file, "_XTVD");
        }

        protected override IList<Helpers.RefDelegate<XElement, XDocument, bool>> GetTransformations()
        {
            return new List<Helpers.RefDelegate<XElement, XDocument, bool>>
            {
                GeneratePrograms,
                GenerateteSchedules
            };
        }

        public void Generate()
        {
            IEnumerable<XElement> programmes = GetProgrammes();
            XDocument generated = Generate(programmes);
            Logger.Log("saving new(or updating existing one) file: " + _file);
            try
            {
                generated.Save(_file);
            }
            catch (Exception e)
            {
                Logger.Log("error saving file: " + e.Message);
            }
        }

        private IEnumerable<XElement> GetProgrammes()
        {
            return from programme in _RootElement.Elements(_ns + "programme")
                   orderby (string)programme.Attribute("start")
                   select programme;
        }

        public override string ToString()
        {
            return Path.GetFileName(_file);
        }
       
        private bool GeneratePrograms(XElement program, ref XDocument xdoc)
        {
            IEnumerable<XElement> metaTags = GenerateMeta(program);
            xdoc.Root.Element("programs")
                .Add(
                    new XElement("program",
                        new XAttribute("id", (string)program.Attribute("external_id")),
                        new XElement("series"),
                        new XElement("title", (string)program.Element(_ns + "title")),
                        new XElement("subtitle"),
                        new XElement("description", (string)program.Element(_ns + "desc")),
                        new XElement("showType"),
                        new XElement("year"),
                        new XElement("mpaaRating"),
                        metaTags
                    )
                );
            return true;
        }

        private IEnumerable<XElement> GenerateMeta(XElement program)
        {
            IEnumerable<XElement> metas = program.Elements(_ns + "metas");
            IList<XElement> transformed = new List<XElement>();
            foreach (var meta in metas)
            {
                string metaType = (string)meta.Element(_ns + "MetaType");
                string[] words = metaType.Split(' ');
                words[0] = Helpers.FirstLetterToLower(words[0]);
                for (int i = 1; i < words.Length; i++)
                {
                    words[i] = Helpers.FirstLetterToUpper(words[i]);
                }
                transformed.Add(
                    new XElement(String.Join("", words), (string)meta.Element(_ns + "MetaValues"))
                );

            }
            return transformed;
        }

        private bool GenerateteSchedules(XElement program, ref XDocument root)
        {
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
            root.Root.Element("schedules").Add(
                new XElement("schedule",
                    new XAttribute("program",id),
                    new XAttribute("time", start),
                    new XAttribute("duration", duration)
                    )
            );
            return true;
        }
        
    }
}