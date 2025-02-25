using System;
using System.Collections.Generic;
using System.Windows.Forms;
using System.Xml;
using System.Drawing;
using System.Linq;
using System.Diagnostics;
using LiveSplit.AliceASL.Memory;

namespace LiveSplit.AliceASL.Settings
{
    public class AliceSplit
    {
        public string ID { get; private set; }
        public string Name { get; private set; }
        public string Description { get; set; }
        public string Category { get; set; }
        public bool Enabled { get; set; }
        public AliceSplit(string id, string name, string descrip, string category, bool enabled)
        {
            this.ID = id;
            this.Name = name;
            this.Description = descrip;
            this.Category = category;
            this.Enabled = enabled;
        }
    }

    public partial class AliceSettings : UserControl
    {
        private readonly AliceComponent Component;
        public List<AliceSplit> Settings { get; set; } = new List<AliceSplit>();

        public AliceSettings(AliceComponent comp)
        {
            InitializeComponent();
            this.Component = comp;
            this.LoadSplits();
            this.Load += AliceSettings_Load;
        }

        private void AliceSettings_Load(object sender, EventArgs e)
        {
            this.Size = new Size(450, 550);
            this.LoadSplitComponents();
        }

        private string GameVersionToString(GameVersion version)
        {
            switch (version)
            {
                case GameVersion.Steam:
                    return "PC Steam";
                case GameVersion.DolphinPAL:
                    return "Dolphin PAL";
                case GameVersion.DolphinNTSC:
                    return "Dolphin NTSC";
                case GameVersion.Invalid:
                default:
                    return "Invalid";
            }
        }

        private void LoadSplitComponents()
        {
            Label lblVersion = new Label
            {
                Location = new Point(40, 5),
                Text = this.GameVersionToString(this.Component.Memory.Version),
                AutoSize = true,
            };

            TreeView tv = new TreeView
            {
                Location = new Point(5, 10),
                Size = new Size(400, 500),
                CheckBoxes = true,
                ShowNodeToolTips = true,
            };

            for (int i = 0; i < this.Settings.Count; i++)
            {
                AliceSplit split = this.Settings[i];
                string[] catNames = split.Category.Split('/');
                if (catNames.Length == 0)
                {
                    Trace.WriteLine($"Split {split.Name} has no categories");
                    continue;
                }
                TreeNode node;
                if (tv.Nodes.ContainsKey(catNames[0]))
                    node = tv.Nodes[catNames[0]];
                else
                    node = tv.Nodes.Add(catNames[0], catNames[0]);

                // skip the first one, we already have it - we need to get the first one from treeview rather than treenode
                foreach (string catName in catNames.Skip(1))
                {
                    if (node.Nodes.ContainsKey(catName))
                        node = node.Nodes[catName];
                    else
                        node = node.Nodes.Add(catName, catName);
                }
                node = node.Nodes.Add(split.Name);
                node.ToolTipText = split.Description;
                node.Checked = split.Enabled;
                node.Tag = i.ToString();
            }
            tv.AfterCheck += this.TreeView_AfterCheck;

            this.Controls.Add(tv);
        }

        private void TreeView_AfterCheck(object sender, TreeViewEventArgs e)
        {
            TreeNode node = e.Node;
            if (node.Tag == null)
                return;
            if (!int.TryParse(node.Tag.ToString(), out int index))
                return;
            AliceSplit split = this.Settings[index];
            if (split == null)
                return;
            split.Enabled = node.Checked;
            this.Settings[index] = split;
        }

        public XmlNode UpdateSettings(XmlDocument document)
        {
            XmlElement xmlSettings = document.CreateElement("Settings");

            XmlElement xmlSplits = document.CreateElement("Splits");
            foreach (AliceSplit split in this.Settings)
            {
                XmlElement xmlSplit = document.CreateElement("Split");
                XmlAttribute splitID = document.CreateAttribute("ID");
                XmlAttribute splitName = document.CreateAttribute("Name");
                XmlAttribute splitDesc = document.CreateAttribute("Description");
                XmlAttribute splitCat = document.CreateAttribute("Category");
                splitID.Value = split.ID;
                splitName.Value = split.Name;
                splitDesc.Value = split.Description;
                splitCat.Value = split.Category;
                xmlSplit.Attributes.Append(splitID);
                xmlSplit.Attributes.Append(splitName);
                xmlSplit.Attributes.Append(splitDesc);
                xmlSplit.Attributes.Append(splitCat);
                xmlSplit.InnerText = split.Enabled.ToString();
                xmlSplits.AppendChild(xmlSplit);
            }
            xmlSettings.AppendChild(xmlSplits);
            return xmlSettings;
        }

        public void SetSettings(XmlNode settings)
        {
            XmlNode xmlSplits = settings.SelectSingleNode("Splits");

            foreach (XmlNode xmlSplit in xmlSplits.SelectNodes("Split"))
            {
                string id = xmlSplit.Attributes["ID"].Value;
                string name = xmlSplit.Attributes["Name"].Value;
                string desc = xmlSplit.Attributes["Description"].Value;
                string cat = xmlSplit.Attributes["Category"].Value;
                bool enabled = bool.Parse(xmlSplit.InnerText);
                AliceSplit split = this[id];
                int index = this.Settings.IndexOf(split);
                if (index == -1)
                {
                    split = new AliceSplit(id, name, desc, cat, enabled);
                    this.Settings.Add(split);
                    continue;
                }
                split.Description = desc;
                split.Category = cat;
                split.Enabled = enabled;
                this.Settings[index] = split;
            }
        }

        private void LoadSplits()
        {
            List<AliceSplit> splits = new List<AliceSplit>
            {
                // Utility
                new AliceSplit("start", "Start", "Auto Start the timer", "Utility", true),
                new AliceSplit("split", "Split", "Auto Split the timer", "Utility", true),
                new AliceSplit("reset", "Reset", "Auto Reset the timer", "Utility", true),
                new AliceSplit("log", "Debug Log", "Log debug info to a log file", "Utility", true),
                new AliceSplit("lrt", "Load Removal", "Remove load times from the timer", "Utility", true),
                new AliceSplit("igt", "Game Time", "Display In Game Time in LiveSplit Layout (CAUTION: May cause lag to LiveSplit)", "Utility", false),
                new AliceSplit("il", "Boss Level", "Enable when running an Individual Level run", "Utility", false),

                // LVL020 Strange Garden
                new AliceSplit("gardenCake", "Cake", "Split on Garden Cake", "Strange Garden", true),
                new AliceSplit("gardenPishsalver", "Pishsalver", "Split on Garden Pishsalver", "Strange Garden", true),
                new AliceSplit("bander0", "Bandersnatch Start", "Split on Bandersnatch Start (Full Game run only)", "Strange Garden/Bandersnatch", false),
                new AliceSplit("bander1", "Bandersnatch Phase 1", "Split on end of Bandersnatch phase 1", "Strange Garden/Bandersnatch", false),
                new AliceSplit("bander2", "Bandersnatch Phase 2", "Split on end of Bandersnatch phase 2", "Strange Garden/Bandersnatch", false),
                new AliceSplit("bander3", "Bandersnatch Phase 3 (End of Fight)", "Split on end of Bandersnatch phase 3", "Strange Garden/Bandersnatch", true),

                // LVL030 Tulgey Woods

                // LVL040 March Hare House

                // LVL050 Hightopps

                // LVL060 Cabin
                new AliceSplit("cabinPishsalver", "Pishsalver", "Split on Pishsalver", "Cabin", true),

                // LVL070 Red Desert

                // LVL075 Moat

                // LVL080 Salazen Grum
                // new AliceSplit("sgPishsalver", "Pishsalver", "Split on Pishsalver", "Salazen Grum", true),

                // LVL085 Bandersnatch Stables
                new AliceSplit("stayne0", "Stayne Start", "Split on Stayne Start (Full Game run only)", "Bandersnatch Stables/Stayne", false),
                new AliceSplit("stayne1", "Stayne Phase 1", "Split on end of Stayne phase 1", "Bandersnatch Stables/Stayne", false),
                new AliceSplit("stayne2", "Stayne Phase 2", "Split on end of Stayne phase 2", "Bandersnatch Stables/Stayne", false),
                new AliceSplit("stayne3", "Stayne Phase 3", "Split on end of Stayne phase 3", "Bandersnatch Stables/Stayne", true),

                // LVL090 Marmoreal
                // TODO: If the bug where Alice stays small happens, do we want to check for and update this?
                // new AliceSplit("wq", "Visit White Queen", "Split on visiting the White Queen", "Marmoreal", true),

                // LVL100 Frabjous Day
                new AliceSplit("jabber0", "Jabberwocky Start (Full Game run only)", "Split on Jabberwocky Start", "Frabjous Day/Jabberwocky", true),
                new AliceSplit("jabber1", "Jabberwocky 1", "Split on end of Jabberwocky phase 1", "Frabjous Day/Jabberwocky", false),
                new AliceSplit("jabber2", "Jabberwocky 2", "Split on end of Jabberwocky phase 2", "Frabjous Day/Jabberwocky", false),
                new AliceSplit("jabber3", "Jabberwocky 3", "Split on end of Jabberwocky phase 3", "Frabjous Day/Jabberwocky", false),
                new AliceSplit("jabber4", "Jabberwocky 4", "Split on end of Jabberwocky phase 4", "Frabjous Day/Jabberwocky", true)
            };
            this.Settings = splits;
        }

        public AliceSplit this[string splitID]
        {
            get { return this.Settings.Find((split) => split.ID == splitID); }
        }
    }
}
