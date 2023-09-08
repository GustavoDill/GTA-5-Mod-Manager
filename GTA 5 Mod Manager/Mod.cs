using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace GTA_5_Mod_Manager
{
    public class Mod
    {
        [XmlAttribute]
        public string Name { get; set; }
        public string Source { get; set; }

        public string[] Files { get; set; }

    }
}
