using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

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
        void Generate();

        /// <summary>
        /// Generated root element name
        /// </summary>
        string RootName
        {
            get;
        }

        /// <summary>
        /// Root element to loaded XML
        /// </summary>
        XElement RootElement
        {
            get;
        }
    }

}
