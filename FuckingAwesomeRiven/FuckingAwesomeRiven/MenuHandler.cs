using System;
using System.Collections.Generic;
using System.Drawing;
using FuckingAwesomeRiven.EvadeUtils;
using LeagueSharp.Common;

namespace FuckingAwesomeRiven
{
    internal class MenuHandler
    {
        public static Orbwalking.Orbwalker Orbwalker;
        public static Menu Config;
        public static List<JumpPosition> J = new List<JumpPosition>();

        public static void InitMenu()
        {
            Config = new Menu("FuckingAwesomeRiven", "KappaChino", true);

            Orbwalker = new Orbwalking.Orbwalker(Config.AddSubMenu(new Menu("Orbwalking", "OW")));
            TargetSelector.AddToMenu(Config.AddSubMenu(new Menu("Target Selector", "Target Selector")));

            var combo = Config.AddSubMenu(new Menu("Combo", "Combo"));
            combo.AddItem(new MenuItem("xdxdxdxd", "-- Normal Combo"));

            combo.AddItem(new MenuItem("QAA", "Q AA Mode").SetValue(new StringList(new[] {"Q -> AA", "AA -> Q"})));
            var gcM = combo.AddSubMenu(new Menu("Gapcloser Combos", "Gapcloser Combos"));
            gcM.AddItem(new MenuItem("CEWHQ", "E->W->Hy->Q").SetValue(true));
            gcM.AddItem(new MenuItem("CQWH", "Q->W->Hy").SetValue(true));
            gcM.AddItem(new MenuItem("CEHQ", "E->Hy->Q").SetValue(true));
            gcM.AddItem(new MenuItem("CEW", "E->W").SetValue(true));

            var r1Combo = combo.AddSubMenu(new Menu("R1 Combos", "R1 Combos"));
            r1Combo.AddItem(new MenuItem("CREWHQ", "R->E->W->Hy->Q").SetValue(true));
            r1Combo.AddItem(new MenuItem("CREWH", "R->E->W->Hy").SetValue(true));
            r1Combo.AddItem(new MenuItem("CREAAHQ", "R->E->AA->Hy->Q").SetValue(true));
            r1Combo.AddItem(new MenuItem("CRWAAHQ", "R->W->AA->Hy-Q").SetValue(true));
            r1Combo.AddItem(new MenuItem("CR1CC", "R").SetValue(true));

            var r2Combo = combo.AddSubMenu(new Menu("R2 Combos", "R2 Combos"));
            r2Combo.AddItem(new MenuItem("CR2WQ", "R2->W->Q").SetValue(true));
            r2Combo.AddItem(new MenuItem("CR2W", "R2->W").SetValue(true));
            r2Combo.AddItem(new MenuItem("CR2Q", "R2->Q").SetValue(true));
            r2Combo.AddItem(new MenuItem("CR2CC", "R2").SetValue(true));

            combo.AddItem(new MenuItem("CQ", "Use Q").SetValue(true));
            combo.AddItem(new MenuItem("CW", "Use W").SetValue(true));
            combo.AddItem(new MenuItem("CE", "Use E").SetValue(true));
            combo.AddItem(new MenuItem("CRnote", "NOTE: Disable R/R2 will disable their combos"));
            combo.AddItem(new MenuItem("CR", "Use R").SetValue(true));
            combo.AddItem(new MenuItem("CR2", "Use R2").SetValue(true));

            var harass = Config.AddSubMenu(new Menu("Harass", "Harass"));
            harass.AddItem(new MenuItem("fdsf", "Harass Combos"));
            harass.AddItem(new MenuItem("HQ3AAWE", "Q3->AA->W->E Back").SetValue(true));
            harass.AddItem(new MenuItem("HQAA3WE", "(Q->AA)x3->W->E Back").SetValue(true));
            harass.AddItem(new MenuItem("sdffsdf", ""));
            harass.AddItem(new MenuItem("HQ", "Use Q").SetValue(true));
            harass.AddItem(new MenuItem("HW", "Use W").SetValue(true));
            harass.AddItem(new MenuItem("HE", "Use E Back | disabled").SetValue(false));

            var burst = Config.AddSubMenu(new Menu("Burst Combos", "Burst Combos"));
            burst.AddItem(new MenuItem("shyCombo", "-- ShyCombo").SetValue(true));
            burst.AddItem(new MenuItem("shyComboinfo1", "E->R->Flash->AA->Hydra->W->R2->Q3"));
            burst.AddItem(new MenuItem("shyComboinfo2", "Hold Burst button on Q1/Q2"));
            burst.AddItem(new MenuItem("kyzerCombo", "-- Kyzer Q3 Combo").SetValue(true));
            burst.AddItem(new MenuItem("kyzerComboinfo1", "E->R->Flash->Q->AA->Hydra->W->R2->Q"));
            burst.AddItem(new MenuItem("kyzerComboinfo2", "Hold Burst button on Q3"));
            burst.AddItem(new MenuItem("flashlessBurst", "-- No Flash Burst").SetValue(true));
            burst.AddItem(new MenuItem("flashlessBurst1", "E->R->W->Hydra->AA->R2->Q"));
            burst.AddItem(new MenuItem("flashlessBurst2", "Hold Burst button on on any Q"));

            var farm = Config.AddSubMenu(new Menu("Farming", "Farming"));
            farm.AddItem(new MenuItem("fnjdsjkn", "          Last Hit"));
            farm.AddItem(new MenuItem("QLH", "Use Q").SetValue(true));
            farm.AddItem(new MenuItem("WLH", "Use W").SetValue(true));
            farm.AddItem(new MenuItem("10010321223", "          Jungle Clear"));
            farm.AddItem(new MenuItem("QJ", "Use Q").SetValue(true));
            farm.AddItem(new MenuItem("WJ", "Use W").SetValue(true));
            farm.AddItem(new MenuItem("EJ", "Use E").SetValue(true));
            farm.AddItem(new MenuItem("5622546001", "          Wave Clear"));
            farm.AddItem(new MenuItem("QWC", "Use Q").SetValue(true));
            farm.AddItem(new MenuItem("QWC-LH", "   Q Lasthit").SetValue(true));
            farm.AddItem(new MenuItem("QWC-AA", "   Q -> AA").SetValue(true));
            farm.AddItem(new MenuItem("WWC", "Use W").SetValue(true));

            var draw = Config.AddSubMenu(new Menu("Draw", "Draw"));
            draw.AddItem(new MenuItem("DALL", "Disable All").SetValue(false));
            draw.AddItem(new MenuItem("DQ", "Draw Q Range").SetValue(new Circle(false, Color.White)));
            draw.AddItem(new MenuItem("DW", "Draw W Range").SetValue(new Circle(false, Color.White)));
            draw.AddItem(new MenuItem("DE", "Draw E Range").SetValue(new Circle(false, Color.White)));
            draw.AddItem(new MenuItem("DR", "Draw R Range").SetValue(new Circle(false, Color.White)));
            draw.AddItem(new MenuItem("DBC", "Draw Burst Combo Range").SetValue(new Circle(false, Color.White)));
            draw.AddItem(new MenuItem("DER", "Draw Engage Range").SetValue(new Circle(true, Color.White)));
            draw.AddItem(new MenuItem("DD", "Draw Damage [soon]").SetValue(new Circle(false, Color.White)));

            var misc = Config.AddSubMenu(new Menu("Misc", "Misc"));
            misc.AddItem(new MenuItem("bonusCancelDelay", "Bonus Cancel Delay (ms)").SetValue(new Slider(0, 0, 500)));
            misc.AddItem(new MenuItem("keepQAlive", "Keep Q Alive").SetValue(true));
            misc.AddItem(new MenuItem("QFlee", "Q Flee").SetValue(true));
            misc.AddItem(new MenuItem("EFlee", "E Flee").SetValue(true));

            var keyBindings = Config.AddSubMenu(new Menu("Key Bindings", "KB"));
            keyBindings.AddItem(
                new MenuItem("normalCombo", "Normal Combo").SetValue(new KeyBind(32, KeyBindType.Press)));
            keyBindings.AddItem(new MenuItem("burstCombo", "Burst Combo").SetValue(new KeyBind('M', KeyBindType.Press)));
            keyBindings.AddItem(
                new MenuItem("jungleCombo", "Jungle Clear").SetValue(new KeyBind('C', KeyBindType.Press)));
            keyBindings.AddItem(new MenuItem("waveClear", "WaveClear").SetValue(new KeyBind('C', KeyBindType.Press)));
            keyBindings.AddItem(new MenuItem("lastHit", "LastHit").SetValue(new KeyBind('X', KeyBindType.Press)));
            keyBindings.AddItem(new MenuItem("harass", "Harass").SetValue(new KeyBind('V', KeyBindType.Press)));
            keyBindings.AddItem(new MenuItem("flee", "Flee").SetValue(new KeyBind('Z', KeyBindType.Press)));
            keyBindings.AddItem(
                new MenuItem("forcedR", "Forced R Enabled in Combo").SetValue(
                    new KeyBind('T', KeyBindType.Toggle)));

            Config.AddSubMenu(new Menu("Anti Spells", "Anti Spells"));

            AutoE.Init();

            Antispells.Init();

            var info = Config.AddSubMenu(new Menu("Information", "info"));
            info.AddItem(new MenuItem("Msddsds", "if you would like to donate via paypal"));
            info.AddItem(new MenuItem("Msdsddsd", "you can do so by sending money to:"));
            info.AddItem(new MenuItem("Msdsadfdsd", "jayyeditsdude@gmail.com"));
            info.AddItem(new MenuItem("debug", "Debug Mode")).SetValue(false);
            info.AddItem(new MenuItem("logPos", "Log Position").SetValue(false));
            info.AddItem(new MenuItem("printPos", "Print Positions").SetValue(false));
            info.AddItem(new MenuItem("clearPrevious", "Clear Previous").SetValue(false));
            info.AddItem(new MenuItem("clearCurrent", "Clear Current").SetValue(false));
            info.AddItem(new MenuItem("drawCirclesforTest", "Draw Circles").SetValue(false));

            Config.AddItem(new MenuItem("urfMode", "Urf Mode").SetValue(false));
            Config.AddItem(new MenuItem("streamMouse", "Stream Mouse Mode").SetValue(false));
            Config.AddItem(new MenuItem("Mgdgdfgsd", "Version: 0.0.9.0 BETA"));
            Config.AddItem(new MenuItem("Msd", "Made By FluxySenpai"));

            Config.AddToMainMenu();
        }

        public static bool GetMenuBool(String s)
        {
            return Config.Item(s).GetValue<bool>();
        }
    }
}