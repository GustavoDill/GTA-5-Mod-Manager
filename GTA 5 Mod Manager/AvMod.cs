using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace GTA_5_Mod_Manager
{
    public partial class AvMod : Form
    {
        public AvMod()
        {
            InitializeComponent();
        }
        public AvMod(Mod[] mods, List<Mod> installedMods) : this()
        {
            foreach (var mod in mods)
            {
                for (int i = 0; i < installedMods.Count; i++)
                {
                    if (installedMods[i].Name == mod.Name) goto NEXT_MOD;
                }
                listBox1.Items.Add(mod.Name);
            NEXT_MOD:;
            }
        }
        public string SelectedMod { get => listBox1.SelectedItem.ToString(); }

        private void AvMod_Load(object sender, EventArgs e)
        {

        }

        private void listBox1_KeyUp(object sender, KeyEventArgs e)
        {
            if (e.KeyCode != Keys.Enter) return; 
            if (listBox1.Items.Count <= 0 || listBox1.SelectedItems.Count <= 0) return;

            var mod = listBox1.SelectedItem;
        }

        private void listBox1_MouseDoubleClick(object sender, MouseEventArgs e)
        {
            DialogResult = DialogResult.OK;
        }
    }
}
