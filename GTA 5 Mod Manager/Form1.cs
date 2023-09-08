using CSharpExtendedCommands.DataTypeExtensions;
using CSharpExtendedCommands.UI;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Diagnostics.Eventing.Reader;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Xml.Serialization;

namespace GTA_5_Mod_Manager
{
    public partial class Form1 : Form
    {
        bool directoryOpen = false;
        public string GameDir { get => new FileInfo(textBox1.Text).DirectoryName; }
        List<Mod> avaliableMods = new List<Mod>();
        List<Mod> installedMods = new List<Mod>();
        public Form1()
        {
            InitializeComponent();
            LoadAvaliableMods();
        }
        void LoadInstalledMods(string gta5exe)
        {
            var info = new FileInfo(gta5exe);
            var modfile = new FileInfo(Path.Combine(info.DirectoryName, "installed_mods.xml"));
            installedMods = new List<Mod>();
            if (modfile.Exists == false)
            {
                installedMods.SaveXML(modfile.FullName);
                return;
            }
            installedMods = installedMods.LoadXML(modfile.FullName);
            foreach (var mod in installedMods)
                listBox1.Items.Add(mod.Name);
        }

        void LoadAvaliableMods()
        {
            Mods.avaliable = new List<Mod>();
            //avaliableMods = new List<Mod>();

            if (!File.Exists("avaliableMods.xml"))
            {
                avaliableMods.SaveXML("avaliableMods.xml");
                return;
            }

            Mods.avaliable = Mods.avaliable.LoadXML("avaliableMods.xml");
            //avaliableMods = avaliableMods.LoadXML("avaliableMods.xml");
        }
        private void button1_Click(object sender, EventArgs e)
        {
            var ofd = new OpenFileDialog() { Filter = "GTA5.exe|GTA5.exe" };

            if (ofd.ShowDialog() != DialogResult.OK) return;

            textBox1.Text = ofd.FileName;
            listBox1.Items.Clear();
            LoadInstalledMods(ofd.FileName);
        }

        private void listBox1_SelectedIndexChanged(object sender, EventArgs e)
        {

        }

        private void button3_Click(object sender, EventArgs e)
        {
            var fod = new FolderBrowserDialog();
            var res = fod.ShowDialog();
            if (res != DialogResult.OK) return;

            AddNewMod(fod.SelectedPath);
        }
        string RelPath(string root, string path)
        {
            return path.Substring(root.Length);
        }
        private void AddNewMod(string selectedPath)
        {
            var dir = new DirectoryInfo(selectedPath);
            var files = dir.GetFiles("*", SearchOption.AllDirectories);
            string[] _files = new string[files.Length];
            for (int i = 0; i < files.Length; i++)
            {
                _files[i] = RelPath(selectedPath, files[i].FullName);
            }
            AddModConfirm confirm = new AddModConfirm(_files, selectedPath);
            if (confirm.ShowDialog() != DialogResult.OK) return;

            Mods.AddAvaliable(confirm.GetMod());
        }

        void Status(string statusMsg)

        {
            if (statusStrip1.InvokeRequired)
                statusStrip1.Invoke((MethodInvoker)(() => { status.Text = statusMsg; status.Invalidate(); }));
            else
            { status.Text = statusMsg; status.Invalidate(); }
            Application.DoEvents();

        }
        void Bar(int value)
        {
            if (value < 0) value = bar.Maximum;
            if (statusStrip1.InvokeRequired)
                statusStrip1.Invoke((MethodInvoker)(() => { bar.Value = value; bar.Invalidate(); }));
            else
            {

                bar.Value = value; bar.Invalidate();
            }
        }
        void Files(string text)
        {
            if (statusStrip1.InvokeRequired)
                statusStrip1.Invoke((MethodInvoker)(() => { files.Text = text; status.Invalidate(); }));
            else
            { files.Text = text; status.Invalidate(); }
            Application.DoEvents();
        }
        void TotalBar(int value)
        {
            if (value < 0) value = bar.Maximum;
            if (statusStrip1.InvokeRequired)
                statusStrip1.Invoke((MethodInvoker)(() => { totalBar.Value = value; bar.Invalidate(); }));
            else
            {

                totalBar.Value = value; bar.Invalidate();
            }
        }
        void InstallMod(string name, bool silent = false)
        {
            var mod = Mods.ByName(name);
            if (mod == null) throw new Exception("How did this happen?");
            bar.Value = 0;
            Files($"Files (0/{mod.Files.Length})");
            TotalBar(0);
            totalBar.Maximum = mod.Files.Length;
            //bar.Maximum = mod.Files.Length;
            //var t = new Thread(() =>
            //{
            //var barSize = 0l;
            //foreach (var file in mod.Files)
            //{
            //    barSize += new FileInfo(mod.Source + file).Length;
            //}

            //bar.Maximum = (int)barSize;
            //var totalBytesCopied = 0;
            int copiedFiles = 0;
            foreach (var file in mod.Files)
            {
                var inf = new FileInfo(mod.Source + file);
                var dst = new FileInfo(GameDir + file);
                //if (inf.Length > Int32.MaxValue)
                bar.Maximum = (int)(inf.Length / 1024);
                //else
                //bar.Maximum = (int)inf.Length;
                if (!dst.Directory.Exists) dst.Directory.Create();

                var totalBytes = inf.Length;

                Status(file + $"0/{totalBytes}");

                try
                {
                    var reader = new BinaryReader(File.OpenRead(mod.Source + file));
                    var writer = new BinaryWriter(File.OpenWrite(GameDir + file));
                    writer.BaseStream.Position = 0;
                    byte[] buffer = new byte[1024 * 1024];

                    long bytesCopied = 0;
                    while (bytesCopied < totalBytes)
                    {
                        int bytesRead = reader.Read(buffer, 0, buffer.Length);
                        writer.Write(buffer, 0, bytesRead);
                        bytesCopied += bytesRead;
                        //totalBytesCopied = totalBytesCopied + bytesCopied;

                        Status(file + $" {bytesCopied>>10}/{totalBytes>>10} KB");

                        Bar((int)(bytesCopied >>10));
                        //Thread.Sleep(10);
                    }
                    reader.Close();
                    writer.Close();
                    TotalBar(totalBar.Value + 1);
                    copiedFiles++;
                    Files($"Files ({copiedFiles})/{mod.Files.Length}");
                    //File.Copy(mod.Source + file, GameDir + file, true);
                }
                catch (System.IO.IOException ex){ MessageBox.Show(ex.Message, "Failed to install '" + mod.Name + "'", MessageBoxButtons.OK, MessageBoxIcon.Error); break; }
            }

            //});
            //t.Start();
            //while (t.IsAlive)
            //{
            //    Thread.Sleep(500);
            //}
            status.Text = "Done";
            status.Invalidate();
            installedMods.Add(mod);
            installedMods.SaveXML(GameDir + "\\installed_mods.xml");
            listBox1.Items.Add(mod.Name);
            if (!silent)
                MessageBox.Show("Mod installed successfully!", "Install mod", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        private void button2_Click(object sender, EventArgs e)
        {
            DisableControls();
            var m = new AvMod(Mods.avaliable.ToArray(), installedMods);
            if (m.ShowDialog() != DialogResult.OK)
            {
                EnableControls();
                return;
            }

            InstallMod(m.SelectedMod);
            EnableControls();

        }
        void RemoveMod(string name, bool silent = false)
        {
            List<string> ignoreFiles = new List<string>();
            var mod = Mods.ByName(name);

            for (int i = 0; i < Mods.avaliable.Count; i++)
            {
                foreach (var file in Mods.avaliable[i].Files)
                    if (mod.Files.Contains(file))
                        ignoreFiles.Add(file);
            }

            foreach (var file in mod.Files)
            {
                try { File.Delete(GameDir + file); } catch (Exception ex) { MessageBox.Show(ex.Message, "Warning", MessageBoxButtons.OK, MessageBoxIcon.Warning); }
            }
            foreach (var m in installedMods)
            {
                if (m.Name == mod.Name)
                { installedMods.Remove(m); break; }
            }
            //installedMods.Remove(mod);
            installedMods.SaveXML(GameDir + "\\installed_mods.xml");
            var dirs = Directory.GetDirectories(GameDir + "\\scripts", "*", SearchOption.AllDirectories);
            for (int i = dirs.Length - 1; i >= 0; i--)
            {
                if (Directory.GetFiles(dirs[i]).Length == 0 && Directory.GetDirectories(dirs[i]).Length == 0)
                    Directory.Delete(dirs[i]);
            }
            listBox1.Items.Remove(mod.Name);
            if (!silent) MessageBox.Show("Mod Removed", "Remove", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }
        private void button4_Click(object sender, EventArgs e)
        {
            DisableControls();
            var item = listBox1.SelectedItem.ToString();

            RemoveMod(item);
            EnableControls();
        }

        private void button5_Click(object sender, EventArgs e)
        {
            var sfd = new SaveFileDialog() { Filter = "XML Files (*.xml)|*.xml" };
            if (sfd.ShowDialog() != DialogResult.OK) return;
            installedMods.SaveXML(sfd.FileName);
            MessageBox.Show("Mods exported successfully!", "Export mods", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }
        void DisableControls()
        {
            groupBox1.Enabled = false;
            button1.Enabled = false;
            button2.Enabled = false;
            button3.Enabled = false;
            button4.Enabled = false;
            button5.Enabled = false;
            button6.Enabled = false;
            button7.Enabled = false;
            textBox1.Enabled = false;
            Application.DoEvents();
        }
        void EnableControls()
        {
            groupBox1.Enabled = true;
            button1.Enabled = true;
            button2.Enabled = true;
            button3.Enabled = true;
            button4.Enabled = true;
            button5.Enabled = true;
            button6.Enabled = true;
            button7.Enabled = true;
            textBox1.Enabled = true;
            Application.DoEvents();
        }
        private void button6_Click(object sender, EventArgs e)
        {
            DisableControls();   
            var ofd = new OpenFileDialog() { Filter = "XML Files (*.xml)|*.xml" };
            if (ofd.ShowDialog() != DialogResult.OK)
            {
                EnableControls();
                return;
            }
            List<Mod> modPack = new List<Mod>();
            try
            {
                modPack = modPack.LoadXML(ofd.FileName);
            }
            catch { MessageBox.Show("Failed to load mod pack!", "Load pack", MessageBoxButtons.OK, MessageBoxIcon.Error); return; }

            status.Text = "Uninstalling existing mods...";
            status.Invalidate();
            while (installedMods.Count > 0)
            {
                RemoveMod(installedMods[0].Name, true);
                Thread.Sleep(25);
            }

            foreach (var mod in modPack)
            {
                InstallMod(mod.Name, true);
                Thread.Sleep(100);
            }
            EnableControls();
            MessageBox.Show("ModPack imported successfully!", "Import pack", MessageBoxButtons.OK, MessageBoxIcon.Information);

        }

        private void button7_Click(object sender, EventArgs e)
        {
            var res = MessageBox.Show("Are you shure you want to clear all mods?", "Clear mods", MessageBoxButtons.YesNo, MessageBoxIcon.Warning);
            if (res == DialogResult.No) return;

            while(installedMods.Count > 0)
            {
                RemoveMod(installedMods[0].Name, true);
                Thread.Sleep(25);
            }

            MessageBox.Show("All mods removed!", "Clear mods", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }
    }
}
