using LiveSplit.Model;
using LiveSplit.UI.Components;
using System.Reflection;
using System;
using LiveSplit.AliceASL;

// Indicate to LiveSplit that this class is the ComponentFactory
[assembly: ComponentFactory(typeof(AliceFactory))]

namespace LiveSplit.AliceASL
{
    public class AliceFactory: IComponentFactory
    {
        public string ComponentName => "Alice in Wonderland (2010) AutoSplitter v" + this.Version.ToString();
        public string Description => "Autosplitter for Alice in Wonderland (2010) PC and Wii (Dolphin Emulator)";
        public ComponentCategory Category => ComponentCategory.Control;
        public IComponent Create(LiveSplitState state) { return new AliceComponent(state); }
        public string UpdateName => ComponentName;
        public string UpdateURL => "https://raw.githubusercontent.com/Alice-2010/LiveSplit.AliceASL";
        public string XMLURL => UpdateURL + "/master/Components/LiveSplit.AliceASL.xml";
        public Version Version { get { return Assembly.GetExecutingAssembly().GetName().Version; } }
    }
}
