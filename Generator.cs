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
        protected abstract IList<Helpers.RefDelegate<XElement, XDocument, bool>> GetTransformations();

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
                Logger.Log(e.Message);
                throw new FileNotFoundException(e.Message);
            }
            return xelement;
        }

        /// <summary>
        /// Generate the root element, using the RootName the children classes configure.
        /// </summary>
        /// <returns></returns>
        private XDocument GenerateRoot()
        {
            return new XDocument(
               new XDeclaration("1.0", "UTF-8", null),
               new XElement(RootName,
                   new XElement("schedules"),
                   new XElement("programs")
               )
           );
        }
    
        /// <summary>
        /// Generate the XML content using generators functions provided by the children
        /// </summary>
        /// <param name="programs"></param>
        /// <returns></returns>
        protected XDocument Generate(IEnumerable<XElement> programs)
        {
            XDocument root = GenerateRoot();
            foreach (var program in programs)
            {
                foreach (var func in GetTransformations())
                {
                    func(program, ref root);
                }
            }
            return root;
        }
    }
}
