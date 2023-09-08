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
    public partial class AddModConfirm : Form
    {
        public AddModConfirm()
        {
            InitializeComponent();
        }
        public AddModConfirm(string[] files, string source) : this()
        {
            ModFiles = files;
            Source = source;
            foreach (var file in files)
                listBox1.Items.Add(file);
        }
        private void AddModConfirm_Load(object sender, EventArgs e)
        {

        }
        public Mod GetMod()
        {
            return new Mod() { Files = ModFiles, Name = ModName, Source = this.Source };
        }
        public string[] ModFiles;
        public string Source;
        public string ModName { get => textBox1.Text; set=> textBox1.Text = value; }
        private void button1_Click(object sender, EventArgs e)
        {
            DialogResult = DialogResult.OK;
        }

        private void button2_Click(object sender, EventArgs e)
        {
            DialogResult =DialogResult.Cancel;  
        }
    }
}
