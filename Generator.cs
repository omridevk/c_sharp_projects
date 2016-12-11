using System;
using System.IO;
using System.Xml.Linq;
using OTTProject.Interfaces;
using OTTProject.Utils.Logging;
using System.Collections.Generic;
using System.Linq;
using OTTProject.Utils;

namespace OTTProject
{
    /// <summary>
    /// Abstract base class for future generators. 
    /// </summary>
    abstract public class Generator : IGenerator
    {

        /// <summary>
        /// XML Namespace to use in queries! possible bugs if not added as prefix all LINQ queries!
        /// </summary>
        protected XNamespace NameSpace { get; set; }
        /// <summary>
        /// Generated root element name
        /// </summary>
        public abstract string RootName { get; }
        /// <summary>
        /// Root element to loaded XML
        /// </summary>
        public abstract XElement RootElement { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public abstract IEnumerable<XElement> Programs { get; }

        public Generator(string file)
        {
            InputPath = file;
            OutputPath = Helpers.GeneratePath(file, "_" + RootName);
        }

        /// <summary>
        /// 
        /// </summary>
        private string _OutputPath { get; set; }
        public string OutputPath
        {
            get
            {
                return _OutputPath;
            }
            set
            {
                _OutputPath = value;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        private string _InputPath { get; set; }
        public string InputPath
        {
            get
            {
                return _InputPath;
            }
            set
            {
                _InputPath = value;
            }
        }

        /// <summary>
        /// Load the XML file.
        /// </summary>
        /// <param name="file"></param>
        /// <returns></returns>
        protected XElement Load(string file)
        {

            XElement xelement = default(XElement);
            try
            {
                xelement = XElement.Load(file);
                NameSpace = xelement.Name.Namespace;
                Logger.Info("loaded file succesfully - {0}", Path.GetFileName(file));
            }
            catch (Exception e)
            {
                Logger.Error(e.Message);
            }
            return xelement;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public abstract IList<Tuple<string, Func<XElement, XElement>>> GetGenerators();
    
        /// <summary>
        /// Generate the XML content using generators functions provided by the children
        /// </summary>
        /// <param name="programs"></param>
        /// <returns></returns>
        public virtual XDocument Generate()
        {
            RootElement = Load(InputPath);
            IList<Tuple<string, Func<XElement, XElement>>> generatorsList = GetGenerators();
            XDocument root = default(XDocument);
            if (generatorsList.Count == 0)
            {
                Logger.Critical("no generators functions were provided for: {0}, returning empty XDocument", GetType().Name);
                return root;
            }
            root =
                new XDocument(
                    new XDeclaration("1.0", "UTF-8", null),
                    new XElement(RootName,
                        generatorsList.Select( tuple => 
                            new XElement(tuple.Item1, 
                                Programs.Select(program => (tuple.Item2(program))
                            ))
                        )
                    )
                );
            try
            {
                root.Save(OutputPath);
            } catch (Exception e)
            {
                Logger.Critical("error saving file: {0}", e.Message);
            }
            return root;
        }
    }
}
