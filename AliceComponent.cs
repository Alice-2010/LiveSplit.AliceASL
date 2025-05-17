using LiveSplit.Model;
using LiveSplit.UI;
using LiveSplit.UI.Components;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;
using System.Xml;
using LiveSplit.AliceASL.Settings;
using LiveSplit.AliceASL.Memory;
using System.IO;
using System.Diagnostics;
using System.Linq;
using System.Reflection;

namespace LiveSplit.AliceASL
{
    public class AliceComponent: IComponent
    {
        public string ComponentName => "Alice in Wonderland (2010) AutoSplitter";
        public IDictionary<string, Action> ContextMenuControls => null;

        private readonly string logPath = "alice2010.log";

        public readonly AliceMemory Memory;
        private readonly TimerModel Timer;
        private readonly AliceSettings Settings;

        private readonly List<string> SplitsDone = new List<string>();

        public AliceComponent(LiveSplitState state)
        {
            this.Memory = new AliceMemory();
            this.Settings = new AliceSettings(this);
            if (state != null)
            {
                this.Timer = new TimerModel { CurrentState = state };
                this.Timer.CurrentState.OnStart += this.OnStart;
                this.Timer.CurrentState.OnReset += this.OnReset;
                this.Timer.CurrentState.OnUndoSplit += this.OnUndoSplit;
            }
        }

        public void Update(IInvalidator invalidator, LiveSplitState state, float width, float height, LayoutMode mode)
        {
            if (!this.Memory.HookProcess())
                return;

            this.Memory.UpdatePointerValues();
            Dictionary<string, object> previousValues = this.Memory.PreviousValues;
            Dictionary<string, object> values = this.Memory.CurrentValues;

            // need both current and previous values for these
            int mapIDCurrent = Convert.ToInt32(values["MapID"]);
            int mapIDPrevious = Convert.ToInt32(previousValues["MapID"]);
            int mapSectorCurrent = Convert.ToInt32(values["MapSector"]);
            int mapSectorPrevious = Convert.ToInt32(previousValues["MapSector"]);
            uint audioStatusCurrent = Convert.ToUInt32(values["AudioStatus"]);
            uint audioStatusPrevious = Convert.ToUInt32(previousValues["AudioStatus"]);
            uint aliceIDCurrent = Convert.ToUInt32(values["AliceID"]);
            uint aliceIDPrevious = Convert.ToUInt32(previousValues["AliceID"]);

            // only need current values for these
            float gameTimeCurrent = Convert.ToSingle(values["GameTime"]);
            uint bandersnatchPhaseCurrent = Convert.ToUInt32(values["BandersnatchPhase"]);
            float stayneHealthCurrent = Convert.ToSingle(values["StayneHealth"]);
            uint jabberwockyPhaseCurrent = Convert.ToUInt32(values["JabberwockyPhase"]);
            uint jabberwockyPhase4CounterCurrent = Convert.ToUInt32(values["JabberwockyP4Counter"]);

            // pause LRT if game is loading
            if (this.Settings["lrt"].Enabled)
                state.IsGameTimePaused = mapIDCurrent == -1;

            // if map is changed
            if (mapIDCurrent != mapIDPrevious)
                this.WriteLog($"Changed map from {mapIDPrevious} to {mapIDCurrent}");

            // if not on main menu
            if (mapIDCurrent != 0)
            {
                if (mapSectorCurrent != mapSectorPrevious)
                    this.WriteLog($"Changed sector from {mapSectorPrevious} to {mapSectorCurrent} (map {mapIDCurrent})");

                // display game time in layout
                if (this.Settings["igt"].Enabled)
                    this.SetTextComponent("Game Time", TimeSpan.FromSeconds(gameTimeCurrent).ToString(@"hh\:mm\:ss\.fff"));

                // TODO: Track Achievements + display on layout (depending on settings)
            }

            // check for auto start
            if (state.CurrentPhase == TimerPhase.NotRunning && this.Settings["start"].Enabled)
            {
                if (this.Settings["il"].Enabled)
                {
                    // boss ILs
                    // check bandersnatch
                    if (mapIDCurrent == 20 && audioStatusCurrent == 1 && audioStatusPrevious == 4 && bandersnatchPhaseCurrent == 3)
                    {
                        this.WriteLog("Splitting `Bandersnatch Start` (IL)", false);
                        this.SplitsDone.Add("bander0");
                        this.Timer.Start();
                        return;
                    }

                    // check stayne
                    if (mapIDCurrent == 85 && audioStatusCurrent == 1 && audioStatusPrevious == 4 && stayneHealthCurrent == 1500f)
                    {
                        this.WriteLog("Splitting `Stayne Start` (IL)", false);
                        this.SplitsDone.Add("stayne0");
                        this.Timer.Start();
                        return;
                    }

                    // check jabberwocky
                    if (mapIDCurrent == 100 && audioStatusCurrent == 1 && audioStatusPrevious == 4 && jabberwockyPhaseCurrent == 1 && mapSectorCurrent == 2)
                    {
                        this.WriteLog("Splitting `Jabberwocky Start` (IL)", false);
                        this.SplitsDone.Add("jabber0");
                        this.Timer.Start();
                        return;
                    }
                }
                else
                {
                    // full game run
                    if (mapIDCurrent == 10 && audioStatusCurrent == 1 && audioStatusPrevious == 4)
                    {
                        // Clear the log file
                        this.WriteLog("Beginning Full Game Run", false);
                        this.Timer.Start();
                        return;
                    }
                }
            }
            else if (state.CurrentPhase != TimerPhase.NotRunning)
            {
                // check for auto reset
                if (this.Settings["reset"].Enabled)
                {
                    if (this.Settings["il"].Enabled)
                    {
                        // TODO: find better addresses for ILs - maybe video number?
                        //// bandersnatch
                        //if (mapIDCurrent == 20 && audioStatusCurrent == 4 && mapSectorCurrent == 3)
                        //{
                        //    this.WriteLog("Starting New Bandersnatch Run - Resetting");
                        //    this.Timer.Reset();
                        //    return;
                        //}

                        //// stayne
                        //if (mapIDCurrent == 85 && audioStatusCurrent == 4)
                        //{
                        //    this.WriteLog("Starting New Stayne Run - Resetting");
                        //    this.Timer.Reset();
                        //    return;
                        //}

                        //// jabberwocky
                        //if (mapIDCurrent == 100 && audioStatusCurrent == 4 && mapSectorCurrent == 2)
                        //{
                        //    this.WriteLog("Starting New Jabberwocky Run - Resetting");
                        //    this.Timer.Reset();
                        //    return;
                        //}
                    }
                    else
                    {
                        //// full game run
                        //if (mapIDCurrent == 10 && audioStatusCurrent == 4)
                        //{
                        //    this.WriteLog("Starting New Full Game Run - Resetting");
                        //    this.Timer.Reset();
                        //    return;
                        //}
                    }
                }

                // check for auto split
                if (this.Settings["split"].Enabled)
                {
                    // LVL020 Strange Garden
                    if (mapIDCurrent == 20)
                    {
                        if (!this.SplitsDone.Contains("gardenCake") && aliceIDCurrent == 5 && aliceIDPrevious == 4)
                        {
                            this.Split("gardenCake");
                            return;
                        }

                        if (!this.SplitsDone.Contains("gardenPishsalver") && aliceIDCurrent == 4 && aliceIDPrevious == 5)
                        {
                            this.Split("gardenPishsalver");
                            return;
                        }

                        if (this.SplitsDone.Contains("gardenCake") && !this.SplitsDone.Contains("bander0") && audioStatusCurrent == 1 && audioStatusPrevious == 4 && bandersnatchPhaseCurrent == 3)
                        {
                            this.Split("bander0");
                            return;
                        }

                        if (this.SplitsDone.Contains("bander0") && !this.SplitsDone.Contains("bander1") && bandersnatchPhaseCurrent == 2)
                        {
                            this.Split("bander1");
                            return;
                        }

                        if (this.SplitsDone.Contains("bander1") && !this.SplitsDone.Contains("bander2") && bandersnatchPhaseCurrent == 1)
                        {
                            this.Split("bander2");
                            return;
                        }

                        if (this.SplitsDone.Contains("bander2") && !this.SplitsDone.Contains("bander3") && bandersnatchPhaseCurrent == 1 && audioStatusCurrent == 4 && audioStatusPrevious == 1)
                        {
                            this.Split("bander3");
                            return;
                        }
                    }

                    // LVL030 Tulgey Woods
                    if (mapIDCurrent == 30)
                    {

                    }

                    // LVL040 March Hare House
                    if (mapIDCurrent == 40)
                    {

                    }

                    // LVL050 Hightopps
                    if (mapIDCurrent == 50)
                    {

                    }

                    // LVL060 Cabin
                    if (mapIDCurrent == 60)
                    {
                        if (!this.SplitsDone.Contains("cabinPishsalver") && aliceIDCurrent == 5 && aliceIDPrevious == 4)
                        {
                            this.Split("cabinPishsalver");
                            return;
                        }
                    }

                    // LVL070 Red Desert
                    if (mapIDCurrent == 70)
                    {

                    }

                    // LVL075 Moat
                    if (mapIDCurrent == 75)
                    {

                    }

                    // LVL080 Salazen Grum
                    if (mapIDCurrent == 80)
                    {
                        //if (!this.SplitsDone.Contains("sgPishsalver") && aliceIDCurrent == 5 && aliceIDPrevious == 4)
                        //{
                        //    this.Split("sgPishsalver");
                        //    return;
                        //}
                    }

                    // LVL085 Bandersnatch Stables
                    if (mapIDCurrent == 85)
                    {
                        if (this.SplitsDone.Contains("sgPishsalver") && !this.SplitsDone.Contains("stayne0") && audioStatusCurrent == 1 && audioStatusPrevious == 4 && stayneHealthCurrent == 1500f)
                        {
                            this.Split("stayne0");
                            return;
                        }

                        if (this.SplitsDone.Contains("stayne0") && !this.SplitsDone.Contains("stayne1") && stayneHealthCurrent <= 1000f)
                        {
                            this.Split("stayne1");
                            return;
                        }

                        if (this.SplitsDone.Contains("stayne1") && !this.SplitsDone.Contains("stayne2") && stayneHealthCurrent <= 500f)
                        {
                            this.Split("stayne2");
                            return;
                        }

                        if (this.SplitsDone.Contains("stayne2") && !this.SplitsDone.Contains("stayne3") && audioStatusCurrent == 4 && stayneHealthCurrent == 0f)
                        {
                            this.Split("stayne3");
                            return;
                        }
                    }

                    // LVL090 Marmoreal
                    if (mapIDCurrent == 90)
                    {
                        //if (this.SplitsDone.Contains("stayne3") && !this.SplitsDone.Contains("wq") && aliceIDCurrent == 5 && aliceIDPrevious == 4)
                        //{
                        //    this.Split("wq");
                        //    return;
                        //}
                    }

                    // LVL100 Frabjous Day
                    if (mapIDCurrent == 100)
                    {
                        if (this.SplitsDone.Contains("wq") && !this.SplitsDone.Contains("jabber0") && mapSectorCurrent == 2 && audioStatusCurrent == 1 && audioStatusPrevious == 4 && jabberwockyPhaseCurrent == 1)
                        {
                            this.Split("jabber0");
                            return;
                        }

                        if (this.SplitsDone.Contains("jabber0") && !this.SplitsDone.Contains("jabber1") && mapSectorCurrent == 2 && jabberwockyPhaseCurrent == 2)
                        {
                            this.Split("jabber1");
                            return;
                        }

                        if (this.SplitsDone.Contains("jabber1") && !this.SplitsDone.Contains("jabber2") && mapSectorCurrent == 2 && jabberwockyPhaseCurrent == 3)
                        {
                            this.Split("jabber2");
                            return;
                        }

                        if (this.SplitsDone.Contains("jabber2") && !this.SplitsDone.Contains("jabber3") && mapSectorCurrent == 2 && jabberwockyPhaseCurrent == 4)
                        {
                            this.Split("jabber3");
                            return;
                        }

                        if (this.SplitsDone.Contains("jabber3") && !this.SplitsDone.Contains("jabber4") && mapSectorCurrent == 2 && jabberwockyPhase4CounterCurrent == 3)
                        {
                            this.Split("jabber4");
                            return;
                        }
                    }
                }
            }
        }

        private void Split(string splitID)
        {
            AliceSplit split = this.Settings[splitID];
            if (split == null)
            {
                this.WriteLog($"Split `{splitID}` not found in settings");
                return;
            }
            this.SplitsDone.Add(split.ID);
            this.WriteLog($"Splitting `{split.Name}`");
            if (split.Enabled)
                this.Timer.Split();
        }

        private void OnStart(object sender, EventArgs e)
        {
            if (this.Memory.Proc?.ProcessName == "Dolphin")
            {
                this.WriteLog("Mem1 Base Address: 0x" + this.Memory.Mem1.ToString("X"));
                this.WriteLog("Mem2 Base Address: 0x" + this.Memory.Mem2.ToString("X"));
            }
        }
        private void OnReset(object sender, TimerPhase e)
        {
            this.SplitsDone.Clear();
        }

        private void OnUndoSplit(object sender, EventArgs e)
        {
            // remove the last split from the list
            // this will allow the ASL to resplit if a false positive happens and the user undoes the split
            this.SplitsDone.RemoveAt(this.SplitsDone.Count - 1);
        }

        private void SetTextComponent(string id, string text)
        {
            IEnumerable<object> textSettings = this.Timer.CurrentState.Layout.Components.Where((x) => x.GetType().Name == "TextComponent").Select(x => x.GetType().GetProperty("Settings").GetValue(x, null));
            object textSetting = textSettings.FirstOrDefault(x => (x.GetType().GetProperty("Text1").GetValue(x, null) as string) == id);
            if (textSetting == null)
            {
                // create the text component if it doesn't exist
                Assembly textComponentAssembly = Assembly.LoadFrom("Components\\LiveSplit.Text.dll");
                IComponent textComponent = Activator.CreateInstance(textComponentAssembly.GetType("LiveSplit.UI.Components.TextComponent"), this.Timer.CurrentState) as IComponent;
                this.Timer.CurrentState.Layout.LayoutComponents.Add(new LayoutComponent("LiveSplit.Text.dll", textComponent));
                textSetting = textComponent.GetType().GetProperty("Settings", BindingFlags.Instance | BindingFlags.Public).GetValue(textComponent, null);
                textSetting.GetType().GetProperty("Text1").SetValue(textSetting, id);
            }
            // set the text value
            textSetting?.GetType().GetProperty("Text2").SetValue(textSetting, text);
        }

        private void WriteLog(string text, bool append = true)
        {
            string log = "[Debug";
            // NOTE: If the timer has started, then show the LRT time in log
            if (this.Timer.CurrentState.CurrentPhase != TimerPhase.NotRunning)
            {
                // If comparing against IGT, get IGT/LRT time, else use RTA
                string time = this.Timer.CurrentState.CurrentTimingMethod == TimingMethod.GameTime
                    ? this.Timer.CurrentState.CurrentTime.GameTime.ToString()
                    : this.Timer.CurrentState.CurrentTime.RealTime.ToString();
                if (time != "")
                    log += $" {time} {(this.Timer.CurrentState.CurrentTimingMethod == TimingMethod.RealTime ? "RTA" : "LRT")}";
            }
            log += "]: " + text;
            Trace.WriteLine(log);
            if (this.Settings["log"].Enabled)
                using (StreamWriter wr = new StreamWriter(logPath, append)) wr.WriteLine(log);
        }

        public XmlNode GetSettings(XmlDocument document)
        {
            return this.Settings.UpdateSettings(document);
        }
        public void SetSettings(XmlNode document)
        {
            this.Settings.SetSettings(document);
        }

        public Control GetSettingsControl(LayoutMode mode) { return this.Settings; }
        public float HorizontalWidth => 0;
        public float VerticalHeight => 0;
        public float MinimumHeight => 0;
        public float MinimumWidth => 0;
        public float PaddingTop => 0;
        public float PaddingLeft => 0;
        public float PaddingBottom => 0;
        public float PaddingRight => 0;
        public void DrawVertical(Graphics g, LiveSplitState state, float width, Region clipRegion) {}
        public void DrawHorizontal(Graphics g, LiveSplitState state, float height, Region clipRegion) {}
        public void Dispose() {}
    }
}
