using System;
using System.Xml.Linq;
using System.Collections.Generic;

namespace OTTProject.Interfaces
{
    public interface IGenerator
    {
        /// <summary>
        /// Get A list of generators functions, will be used on the root when generating.
        /// </summary>
        /// <returns></returns>
        IList<Tuple<string, Func<XElement, XElement>>> GetGenerators();

        /// <summary>
        /// 
        /// </summary>
        XDocument Generate();

        /// <summary>
        /// Generated root element name
        /// </summary>
        string RootName
        {
            get;
        }

        string OutputPath
        {
            get;
            set;
        }

        string InputPath
        {
            get;
            set;
        }

        /// <summary>
        /// 
        /// </summary>
        IEnumerable<XElement> Programs
        {
            get;
        }

        /// <summary>
        /// Root element to loaded XML
        /// </summary>
        XElement RootElement
        {
            get;
            set;
        }
    }

}
