// This file is part of LeagueSharp.Common.
// 
// LeagueSharp.Common is free software: you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.
// 
// LeagueSharp.Common is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
// GNU General Public License for more details.
// 
// You should have received a copy of the GNU General Public License
// along with LeagueSharp.Common.  If not, see <http://www.gnu.org/licenses/>.

using System;
using System.Linq;
using LeagueSharp;
using LeagueSharp.Common;
using SharpDX;
using Color = System.Drawing.Color;

namespace FuckingAwesomeLeeSinReborn
{
    internal static class Program
    {
        private static Orbwalking.Orbwalker _orbwalker;
        public static Menu Config;

        private static void Main(string[] args)
        {
            CustomEvents.Game.OnGameLoad += Game_OnGameStart;
        }

        private static void Game_OnGameStart(EventArgs args)
        {
            if (ObjectManager.Player.ChampionName != "LeeSin")
            {
                Notifications.AddNotification(new Notification("not lee sin huh? wanna go m9?", 2));
                return;
            }
            Config = new Menu("FA LeeSin: Reborn", "you-stealing-me-src-m9?", true);

            _orbwalker = new Orbwalking.Orbwalker(Config.AddSubMenu(new Menu("Orbwalker", "Orbwalker")));
            TargetSelector.AddToMenu(Config.AddSubMenu(new Menu("Target Selector", "Target Selector")));

            var combo = Config.AddSubMenu(new Menu("Combo", "Combo"));
            combo.AddItem(new MenuItem("CQ", "Use Q").SetValue(true));
            combo.AddItem(new MenuItem("smiteQ", "Smite to hit Q").SetValue(false));
            combo.AddItem(new MenuItem("CE", "Use E").SetValue(true));
            combo.AddItem(new MenuItem("CR", "Use R (kill)").SetValue(false));
            combo.AddItem(new MenuItem("CpassiveCheck", "Passive Check").SetValue(false));
            combo.AddItem(new MenuItem("CpassiveCheckCount", "Min Stacks").SetValue(new Slider(1, 1, 2)));
            combo.AddItem(new MenuItem("starCombo", "Star Combo").SetValue(new KeyBind('T', KeyBindType.Press)));
            combo.AddItem(new MenuItem("starsadasCombo", "Q -> Ward -> W -> R -> Q2"));
            combo.AddItem(
                new MenuItem("qHitchance", "Q Hitchance").SetValue(
                    new StringList(new[] { "Low", "Medium", "High", "Very High" }, 2)));

            var harass = Config.AddSubMenu(new Menu("Harass", "Harass"));
            harass.AddItem(new MenuItem("HQ", "Use Q").SetValue(true));
            harass.AddItem(new MenuItem("HE", "Use E").SetValue(false));
            harass.AddItem(new MenuItem("HpassiveCheck", "Passive Check").SetValue(false));
            harass.AddItem(new MenuItem("HpassiveCheckCount", "Min Stacks").SetValue(new Slider(1, 1, 2)));

            var insec = Config.AddSubMenu(new Menu("Insec", "Insec"));
            insec.AddItem(new MenuItem("insecOrbwalk", "Orbwalking").SetValue(true));
            insec.AddItem(new MenuItem("clickInsec", "Click Insec").SetValue(true));
            insec.AddItem(new MenuItem("sdgdsgsg", "Click Enemy then click ally"));
            insec.AddItem(new MenuItem("ddfhdhdg", "Tower/Minion/Champion"));
            insec.AddItem(new MenuItem("easyInsec", "Easy Insec").SetValue(true));
            insec.AddItem(new MenuItem("q2InsecRange", "Use Q2 if buffed unit in range (all)").SetValue(true));
            insec.AddItem(new MenuItem("q1InsecRange", "Use Q1 on units in insec range").SetValue(false));
            insec.AddItem(new MenuItem("flashInsec", "Flash if ward down").SetValue(false));
            insec.AddItem(new MenuItem("insec", "Insec Active").SetValue(new KeyBind('Y', KeyBindType.Press)));

            var autoSmite = Config.AddSubMenu(new Menu("Auto Smite", "Auto Smite"));
            autoSmite.AddItem(
                new MenuItem("smiteEnabled", "Enabled").SetValue(new KeyBind("M".ToCharArray()[0], KeyBindType.Toggle)));
            autoSmite.AddItem(new MenuItem("SRU_Red", "Red Buff").SetValue(true));
            autoSmite.AddItem(new MenuItem("SRU_Blue", "Blue Buff").SetValue(true));
            autoSmite.AddItem(new MenuItem("SRU_Dragon", "Dragon").SetValue(true));
            autoSmite.AddItem(new MenuItem("SRU_Baron", "Baron").SetValue(true));

            var farm = Config.AddSubMenu(new Menu("Farming", "Farming"));
            farm.AddItem(new MenuItem("10010321223", "          Jungle Clear"));
            farm.AddItem(new MenuItem("QJ", "Use Q").SetValue(true));
            farm.AddItem(new MenuItem("WJ", "Use W").SetValue(true));
            farm.AddItem(new MenuItem("EJ", "Use E").SetValue(true));
            farm.AddItem(new MenuItem("5622546001", "          Wave Clear"));
            farm.AddItem(new MenuItem("QWC", "Use Q").SetValue(true));
            farm.AddItem(new MenuItem("EWC", "Use E").SetValue(true));

            var draw = Config.AddSubMenu(new Menu("Draw", "Draw"));
            draw.AddItem(new MenuItem("LowFPS", "Low Fps Mode").SetValue(false));
            draw.AddItem(
                new MenuItem("LowFPSMode", "Low FPS Settings").SetValue(
                    new StringList(new[] { "EXTREME", "MEDIUM", "LOW" }, 2)));
            draw.AddItem(new MenuItem("DQ", "Draw Q Range").SetValue(new Circle(false, Color.White)));
            draw.AddItem(new MenuItem("DW", "Draw W Range").SetValue(new Circle(false, Color.White)));
            draw.AddItem(new MenuItem("DE", "Draw E Range").SetValue(new Circle(false, Color.White)));
            draw.AddItem(new MenuItem("DR", "Draw R Range").SetValue(new Circle(false, Color.White)));
            draw.AddItem(new MenuItem("DS", "Draw Smite Range").SetValue(new Circle(false, Color.PowderBlue)));
            draw.AddItem(new MenuItem("DWJ", "Draw Wardjump").SetValue(true));
            draw.AddItem(new MenuItem("DES", "Draw Escape Spots").SetValue(true));

            var escape = Config.AddSubMenu(new Menu("Escape Settings", "Escape Settings"));
            escape.AddItem(new MenuItem("escapeMode", "Enable Jungle Escape").SetValue(true));
            escape.AddItem(new MenuItem("Wardjump", "Escape/Wardjump").SetValue(new KeyBind('Z', KeyBindType.Press)));
            escape.AddItem(new MenuItem("alwaysJumpMaxRange", "Always Jump Max Range").SetValue(true));
            escape.AddItem(new MenuItem("jumpChampions", "Jump to Champions").SetValue(true));
            escape.AddItem(new MenuItem("jumpMinions", "Jump to Minions").SetValue(true));
            escape.AddItem(new MenuItem("jumpWards", "Jump to wards").SetValue(true));

            var info = Config.AddSubMenu(new Menu("Information", "info"));
            info.AddItem(new MenuItem("Msddsds", "if you would like to donate via paypal"));
            info.AddItem(new MenuItem("Msdsddsd", "you can do so by sending money to:"));
            info.AddItem(new MenuItem("Msdsadfdsd", "jayyeditsdude@gmail.com"));

            Config.AddItem(new MenuItem("Mgdgdfgsd", "Version: 0.0.1-5 BETA"));
            Config.AddItem(new MenuItem("Msd", "Made By FluxySenpai"));


            Config.AddToMainMenu();

            CheckHandler._spells[SpellSlot.Q].SetSkillshot(0.25f, 65f, 1800f, true, SkillshotType.SkillshotLine);

            CheckHandler.Init();
            JumpHandler.Load();
            Game.OnUpdate += Game_OnGameUpdate;
            Drawing.OnDraw += Drawing_OnDraw;
            Game.OnWndProc += InsecHandler.OnClick;
            AutoSmite.Init();
            Obj_AI_Base.OnProcessSpellCast += CheckHandler.Obj_AI_Hero_OnProcessSpellCast;
            Notifications.AddNotification(new Notification("Fucking Awesome Lee Sin:", 2));
            Notifications.AddNotification(new Notification("REBORN", 2));
        }

        private static void Drawing_OnDraw(EventArgs args)
        {
            StateHandler.DrawProjection();
            var lowFps = Config.Item("LowFPS").GetValue<bool>();
            var lowFpsMode = Config.Item("LowFPSMode").GetValue<StringList>().SelectedIndex + 1;
            if (Config.Item("DQ").GetValue<Circle>().Active)
            {
                Render.Circle.DrawCircle(
                    ObjectManager.Player.Position, CheckHandler._spells[SpellSlot.Q].Range,
                    Config.Item("DQ").GetValue<Circle>().Color, lowFps ? lowFpsMode : 5);
            }
            if (Config.Item("DW").GetValue<Circle>().Active)
            {
                Render.Circle.DrawCircle(
                    ObjectManager.Player.Position, CheckHandler._spells[SpellSlot.W].Range,
                    Config.Item("DW").GetValue<Circle>().Color, lowFps ? lowFpsMode : 5);
            }
            if (Config.Item("DE").GetValue<Circle>().Active)
            {
                Render.Circle.DrawCircle(
                    ObjectManager.Player.Position, CheckHandler._spells[SpellSlot.E].Range,
                    Config.Item("DE").GetValue<Circle>().Color, lowFps ? lowFpsMode : 5);
            }
            if (Config.Item("DR").GetValue<Circle>().Active)
            {
                Render.Circle.DrawCircle(
                    ObjectManager.Player.Position, CheckHandler._spells[SpellSlot.R].Range,
                    Config.Item("DR").GetValue<Circle>().Color, lowFps ? lowFpsMode : 5);
            }
            //Render.Circle.DrawCircle(StateHandler.GetFirstCollisionMinion(ObjectManager.Player, TargetSelector.GetTarget(1200f, TargetSelector.DamageType.Physical)).Position, 100f, Color.OrangeRed);
            WardjumpHandler.Draw();
            InsecHandler.Draw();
        }

        private static void Game_OnGameUpdate(EventArgs args)
        {
            //StateHandler.SuperDuperUlt();

            if (CheckHandler.LastSpell + 3000 <= Environment.TickCount)
            {
                CheckHandler.PassiveStacks = 0;
            }
            if (Config.Item("starCombo").GetValue<KeyBind>().Active)
            {
                StateHandler.StarCombo();
                return;
            }
            if (Config.Item("insec").GetValue<KeyBind>().Active)
            {
                InsecHandler.DoInsec();
                return;
            }
            InsecHandler.FlashPos = new Vector3();
            InsecHandler.FlashR = false;

            if (Config.Item("Wardjump").GetValue<KeyBind>().Active)
            {
                WardjumpHandler.DrawEnabled = Config.Item("DWJ").GetValue<bool>();
                WardjumpHandler.Jump(Game.CursorPos, Config.Item("alwaysJumpMaxRange").GetValue<bool>(), true);
                return;
            }
            WardjumpHandler.DrawEnabled = false;

            switch (_orbwalker.ActiveMode)
            {
                case Orbwalking.OrbwalkingMode.Combo:
                    StateHandler.Combo();
                    return;
                case Orbwalking.OrbwalkingMode.LaneClear:
                    StateHandler.JungleClear();
                    return;
                case Orbwalking.OrbwalkingMode.Mixed:
                    StateHandler.Harass();
                    return;
            }
        }
    }
}
