using System;
using System.IO;
using System.Xml.Linq;
using System.Collections.Generic;

namespace OTTProject
{
    /// <summary>
    /// Abstract base class for future generators. 
    /// </summary>
    abstract public class Generator
    {


        /// <summary>
        /// Get A list of generators functions, will be used on the root when generating.
        /// </summary>
        /// <returns></returns>
        protected abstract IList<Tuple<string,Func<XElement, XElement>>> GetGenerators();

        /// <summary>
        /// Generated root element name
        /// </summary>
        protected abstract string RootName
        {
            get;
        }

        /// <summary>
        /// Root element to loaded XML
        /// </summary>
        protected abstract XElement RootElement
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
            XElement xelement;
            try
            {
                xelement = XElement.Load(file);
                Logger.Log("loaded file succesfully - " + Path.GetFileName(file));
            }
            catch (Exception e)
            {
                Logger.Error(e.Message);
                throw new Exception(e.Message);
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
        /// Generate the XML content using generators functions provided by the children
        /// </summary>
        /// <param name="programs"></param>
        /// <returns></returns>
        protected XDocument Generate(IEnumerable<XElement> programs)
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
            var generatorsList = GetGenerators();
            foreach (var program in programs)
            {
                // need to check if generators returns an empty list
                foreach (var tuple in generatorsList)
                {
                    // need to check if element is null
                    try
                    {
                        Helpers.FirstOrCreate(tuple.Item1, root.Root)
                            .Add(tuple.Item2(program));
                        Logger.Log("generated children elements for: " + tuple.Item1);
                    } catch (Exception e)
                    {
                        string message = "error invoking child generator: " + program.ToString() + " error:" + e.Message;
                        Logger.Error(message);
                    }
                }
            }
            return root;
        }
    }
}
