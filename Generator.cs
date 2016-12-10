using System;
using System.IO;
using System.Xml.Linq;
using OTTProject.Utils;
using OTTProject.Interfaces;
using OTTProject.Utils.Logging;
using System.Collections.Generic;

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
        public abstract string RootName
        {
            get;
        }

        /// <summary>
        /// Root element to loaded XML
        /// </summary>
        public abstract XElement RootElement
        {
            get;
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
        /// Generate the root element, using the RootName the children classes configure.
        /// </summary>
        /// <returns></returns>
        private XDocument GenerateRoot()
        {
            if (String.IsNullOrEmpty(RootName)) {
                throw new Exception("Implementer didn't provide a root document name");
            }
            return new XDocument(
               new XDeclaration("1.0", "UTF-8", null),
               new XElement(RootName)
           );
        }

        /// <summary>
        /// 
        /// </summary>
        abstract public void Generate();

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
        protected XDocument Generate(IEnumerable<XElement> programs, string outputPath)
        {
            XDocument root = default(XDocument);
            try
            {
                root = GenerateRoot();
            } catch(Exception e)
            {
                Logger.Error(e.Message);
                return root;
            }
            // maybe use var?
            IList<Tuple<string, Func<XElement, XElement>>> generatorsList = GetGenerators();
            if (generatorsList.Count == 0)
            {
                Logger.Error("no generators found for {0}", this.GetType().Name);
                return root;
            }
            foreach (var program in programs)
            {
                // need to check if generators returns an empty list
                foreach (var tuple in generatorsList)
                {
                    try
                    {
                        Helpers.FirstOrCreate(tuple.Item1, root.Root)
                            .Add(tuple.Item2(program));
                    } catch (Exception e)
                    {
                        Logger.Error("error invoking child generator: {0} error: {1}", program, e.Message);
                    }
                }
            }
            try
            {
                root.Save(outputPath);
            } catch (Exception e)
            {
                Logger.Error("error saving file: {0}", e.Message);
            }
            return root;
        }
    }
}
