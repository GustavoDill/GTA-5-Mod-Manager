using CSharpExtendedCommands.DataTypeExtensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GTA_5_Mod_Manager
{
    public static class Mods
    {
        public static Mod ByName(string name)
        {
            foreach (var mod in avaliable)
                if (mod.Name == name) return mod;
            return null;
        }
        public static List<Mod> avaliable;
        public static void AddAvaliable(Mod mod)
        {
            avaliable.Add(mod);
            avaliable.SaveXML("avaliableMods.xml");
        }
    }
}
