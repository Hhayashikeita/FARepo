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
using System.Collections.Generic;
using System.Linq;
using LeagueSharp;
using LeagueSharp.Common;
using LeagueSharp.Common.Data;

namespace FuckingAwesomeDraven
{
    internal enum Spells
    {
        Q,
        W,
        E,
        R
    }

    internal class Program
    {
        // ReSharper disable once InconsistentNaming
        public static Dictionary<Spells, Spell> _spells = new Dictionary<Spells, Spell>
        {
            { Spells.Q, new Spell(SpellSlot.Q, 0) },
            { Spells.W, new Spell(SpellSlot.W, 0) },
            { Spells.E, new Spell(SpellSlot.E, 1100) },
            { Spells.R, new Spell(SpellSlot.R, 20000) }
        };

        public static Orbwalking.Orbwalker Orbwalker;
        public static Menu Config;

        private static Obj_AI_Hero _player;

        private static void Main(string[] args)
        {
            CustomEvents.Game.OnGameLoad += Game_OnGameLoad;
        }

        private static void Game_OnGameLoad(EventArgs args)
        {
            _player = ObjectManager.Player;
            
            if (ObjectManager.Player.ChampionName != "Draven")
            {
                Notifications.AddNotification(new Notification("Not Draven? Draaaaaaaaaven.", 5));
                return;
            }

            Config = new Menu("FuckingAwesomeDraven", "FuckingAwesomeDraven", true);

            Orbwalker = new Orbwalking.Orbwalker(Config.AddSubMenu(new Menu("Orbwalking", "Orbwalking")));

            TargetSelector.AddToMenu(Config.AddSubMenu(new Menu("Target Selector", "Target Selector")));

            var comboMenu = Config.AddSubMenu(new Menu("Combo", "Combo"));

            comboMenu.AddItem(new MenuItem("UQC", "Use Q").SetValue(true));
            comboMenu.AddItem(new MenuItem("UWC", "Use W").SetValue(true));
            comboMenu.AddItem(new MenuItem("UEC", "Use E").SetValue(true));
            comboMenu.AddItem(new MenuItem("URC", "Use R").SetValue(true));
            comboMenu.AddItem(
                new MenuItem("URCM", "R Mode").SetValue(new StringList(new[] { "Out of Range KS", "KS (any time)" })));
            comboMenu.AddItem(new MenuItem("forceR", "Force R on Target").SetValue(new KeyBind('T', KeyBindType.Press)));

            var harassMenu = Config.AddSubMenu(new Menu("Harass", "Harass"));

            harassMenu.AddItem(new MenuItem("UQH", "Use Q").SetValue(true));
            harassMenu.AddItem(new MenuItem("UWH", "Use W").SetValue(true));
            harassMenu.AddItem(new MenuItem("UEH", "Use E").SetValue(true));

            var jungleMenu = Config.AddSubMenu(new Menu("MinionClear", "MinionClear"));

            jungleMenu.AddItem(new MenuItem("sdfsdf", "Jungle Clear"));
            jungleMenu.AddItem(new MenuItem("UQJ", "Use Q").SetValue(true));
            jungleMenu.AddItem(new MenuItem("UWJ", "Use W").SetValue(true));
            jungleMenu.AddItem(new MenuItem("UEJ", "Use E").SetValue(true));
            jungleMenu.AddItem(new MenuItem("sdffdsdf", "Wave Clear"));
            jungleMenu.AddItem(new MenuItem("UQWC", "Use Q").SetValue(true));
            jungleMenu.AddItem(new MenuItem("WCM", "Min Mana for Waveclear (%)").SetValue(new Slider(20)));

            // Axe Menu
            var axe = Config.AddSubMenu(new Menu("Axe Catching", "Axe Catching"));

            axe.AddItem(new MenuItem("catching", "Catching Enabled").SetValue(new KeyBind('M', KeyBindType.Toggle)));
            axe.AddItem(new MenuItem("useWCatch", "Use W to Catch (smart)").SetValue(false));
            axe.AddItem(
                new MenuItem("catchRadiusMode", "Catch Radius Mode").SetValue(
                    new StringList(new[] { "Mouse Mode", "Sector Mode" })));
            axe.AddItem(new MenuItem("sectorAngle", "Sector Angle").SetValue(new Slider(177, 1, 360)));
            axe.AddItem(new MenuItem("catchRadius", "Catch Radius").SetValue(new Slider(600, 300, 1500)));
            axe.AddItem(new MenuItem("ignoreTowerReticle", "Ignore Tower Reticle").SetValue(true));
            axe.AddItem(new MenuItem("clickRemoveAxes", "Remove Axes With Click").SetValue(true));

            Antispells.Init();

            var draw = Config.AddSubMenu(new Menu("Draw", "Draw"));
            draw.AddItem(new MenuItem("DABR", "Disable All Drawings but Reticle").SetValue(false));
            draw.AddItem(new MenuItem("DE", "Draw E Range").SetValue(new Circle(false, System.Drawing.Color.White)));
            draw.AddItem(new MenuItem("DR", "Draw R Range").SetValue(new Circle(false, System.Drawing.Color.White)));
            draw.AddItem(
                new MenuItem("DCS", "Draw Catching State").SetValue(new Circle(true, System.Drawing.Color.White)));
            draw.AddItem(
                new MenuItem("DCA", "Draw Current Axes").SetValue(new Circle(false, System.Drawing.Color.White)));
            draw.AddItem(
                new MenuItem("DCR", "Draw Catch Radius").SetValue(new Circle(true, System.Drawing.Color.White)));
            draw.AddItem(new MenuItem("DAR", "Draw Axe Spots").SetValue(new Circle(true, System.Drawing.Color.White)));
            draw.AddItem(
                new MenuItem("DKM", "Draw Killable Minion").SetValue(new Circle(true, System.Drawing.Color.White)));

            var info = Config.AddSubMenu(new Menu("Information", "info"));
            info.AddItem(new MenuItem("Msddsds", "if you would like to donate via paypal"));
            info.AddItem(new MenuItem("Msdsddsd", "you can do so by sending money to:"));
            info.AddItem(new MenuItem("Msdsadfdsd", "jayyeditsdude@gmail.com"));

            Config.AddItem(new MenuItem("Mgdgdfgsd", "Version: 0.0.4-0"));
            Config.AddItem(new MenuItem("Msd", "Made By FluxySenpai"));

            Config.AddToMainMenu();

            Notifications.AddNotification(new Notification("Fucking Awesome Draven - Loaded", 5));
            Notifications.AddNotification("Who wants some Draven?", 5);

            _spells[Spells.E].SetSkillshot(250f, 130f, 1400f, false, SkillshotType.SkillshotLine);
            _spells[Spells.R].SetSkillshot(400f, 160f, 2000f, false, SkillshotType.SkillshotLine);

            Orbwalker.SetAttack(false);
            Orbwalker.SetMovement(false);

            GameObject.OnCreate += AxeCatcher.OnCreate;
            GameObject.OnDelete += AxeCatcher.OnDelete;
            Obj_AI_Base.OnProcessSpellCast += AxeCatcher.Obj_AI_Hero_OnProcessSpellCast;
            Drawing.OnDraw += eventArgs => AxeCatcher.Draw();
            Game.OnUpdate += Game_OnGameUpdate;
            Game.OnWndProc += AxeCatcher.GameOnOnWndProc;
        }

        private static void Game_OnGameUpdate(EventArgs args)
        {
            if (Config.Item("forceR").GetValue<KeyBind>().Active &&
                TargetSelector.GetTarget(3000, TargetSelector.DamageType.Physical).IsValidTarget())
            {
                _spells[Spells.R].Cast(TargetSelector.GetTarget(3000, TargetSelector.DamageType.Physical));
            }

            AxeCatcher.CatchAxes();

            switch (Orbwalker.ActiveMode)
            {
                case Orbwalking.OrbwalkingMode.Combo:
                    Combo();
                    break;
                case Orbwalking.OrbwalkingMode.LaneClear:
                    WaveClear();
                    Jungle();
                    break;
                case Orbwalking.OrbwalkingMode.Mixed:
                    Harass();
                    break;
            }
        }

        public static void Combo()
        {
            var q = Config.Item("UQC").GetValue<bool>();
            var w = Config.Item("UWC").GetValue<bool>();
            var e = Config.Item("UEC").GetValue<bool>();
            var r = Config.Item("URC").GetValue<bool>();

            var t = AxeCatcher.GetTarget();
            if (!t.IsValidTarget() || !t.IsValid<Obj_AI_Hero>())
            {
                return;
            }
            var target = (Obj_AI_Hero) t;

            if (target.IsValidTarget(Orbwalking.GetRealAutoAttackRange(_player) + 200))
            {
                if (ItemData.Youmuus_Ghostblade.GetItem().IsReady())
                {
                    ItemData.Youmuus_Ghostblade.GetItem().Cast();
                }
            }

            if (q && AxeCatcher.LastAa + 300 < Environment.TickCount && _spells[Spells.Q].IsReady() &&
                AxeCatcher.AxeSpots.Count + AxeCatcher.CurrentAxes < 2 &&
                target.IsValidTarget(Orbwalking.GetRealAutoAttackRange(_player)))
            {
                _spells[Spells.Q].Cast();
            }

            if (w && !ObjectManager.Player.HasBuff("dravenfurybuff", true) &&
                !ObjectManager.Player.HasBuff("dravenfurybuff") && _spells[Spells.W].IsReady() &&
                target.IsValidTarget(Orbwalking.GetRealAutoAttackRange(_player)))
            {
                _spells[Spells.W].Cast();
            }

            if (e && _spells[Spells.E].IsReady() && AxeCatcher.CanMakeIt(500) &&
                target.IsValidTarget(_spells[Spells.E].Range))
            {
                _spells[Spells.E].Cast(target);
            }

            var t2 = TargetSelector.GetTarget(3000, TargetSelector.DamageType.Physical);
            if (r && _spells[Spells.R].IsReady() && t2.IsValidTarget(_spells[Spells.R].Range))
            {
                switch (Config.Item("URCM").GetValue<StringList>().SelectedIndex)
                {
                    case 1:
                        if (GetRCalc(t2))
                        {
                            _spells[Spells.R].Cast(t2);
                        }
                        break;
                    case 0:
                        if (GetRCalc(t2) && t2.Distance(_player) > 800)
                        {
                            _spells[Spells.R].Cast(t2);
                        }
                        break;
                }
            }
        }

        public static void Harass()
        {
            var q = Config.Item("UQH").GetValue<bool>();
            var w = Config.Item("UWH").GetValue<bool>();
            var e = Config.Item("UEH").GetValue<bool>();

            var t = AxeCatcher.GetTarget();
            if (!t.IsValidTarget() || !t.IsValid<Obj_AI_Hero>())
            {
                return;
            }
            var target = (Obj_AI_Hero) t;

            if (q && AxeCatcher.LastAa + 300 < Environment.TickCount && _spells[Spells.Q].IsReady() &&
                AxeCatcher.AxeSpots.Count + AxeCatcher.CurrentAxes < 2 &&
                target.IsValidTarget(Orbwalking.GetRealAutoAttackRange(_player)))
            {
                _spells[Spells.Q].Cast();
            }
            if (w && !ObjectManager.Player.HasBuff("dravenfurybuff", true) &&
                !ObjectManager.Player.HasBuff("dravenfurybuff") && _spells[Spells.W].IsReady() &&
                target.IsValidTarget(Orbwalking.GetRealAutoAttackRange(_player)))
            {
                _spells[Spells.W].Cast();
            }
            if (e && _spells[Spells.E].IsReady() && AxeCatcher.CanMakeIt(500) &&
                target.IsValidTarget(_spells[Spells.E].Range))
            {
                _spells[Spells.E].Cast(target);
            }
        }

        public static void Jungle()
        {
            var q = Config.Item("UQJ").GetValue<bool>();
            var w = Config.Item("UWJ").GetValue<bool>();
            var e = Config.Item("UEJ").GetValue<bool>();

            var target =
                MinionManager.GetMinions(
                    _player.Position, 700, MinionTypes.All, MinionTeam.Neutral, MinionOrderTypes.MaxHealth)
                    .FirstOrDefault();

            if (q && AxeCatcher.LastAa + 300 < Environment.TickCount && _spells[Spells.Q].IsReady() &&
                AxeCatcher.AxeSpots.Count + AxeCatcher.CurrentAxes < 2 &&
                target.IsValidTarget(Orbwalking.GetRealAutoAttackRange(_player)))
            {
                _spells[Spells.Q].Cast();
            }
            if (w && !ObjectManager.Player.HasBuff("dravenfurybuff", true) &&
                !ObjectManager.Player.HasBuff("dravenfurybuff") && _spells[Spells.W].IsReady() &&
                target.IsValidTarget(Orbwalking.GetRealAutoAttackRange(_player)))
            {
                _spells[Spells.W].Cast();
            }
            if (e && _spells[Spells.E].IsReady() && AxeCatcher.CanMakeIt(500) &&
                target.IsValidTarget(_spells[Spells.E].Range))
            {
                _spells[Spells.E].Cast(target);
            }
        }

        public static void WaveClear()
        {
            var q = Config.Item("UQWC").GetValue<bool>();
            var target =
                MinionManager.GetMinions(
                    _player.Position, 700, MinionTypes.All, MinionTeam.Enemy, MinionOrderTypes.MaxHealth)
                    .FirstOrDefault(a => !a.Name.ToLower().Contains("ward"));
            if (Config.Item("WCM").GetValue<Slider>().Value > (_player.Mana / _player.MaxMana * 100))
            {
                return;
            }
            if (q && AxeCatcher.LastAa + 300 < Environment.TickCount && _spells[Spells.Q].IsReady() &&
                AxeCatcher.AxeSpots.Count + AxeCatcher.CurrentAxes < 2 &&
                target.IsValidTarget(Orbwalking.GetRealAutoAttackRange(_player)))
            {
                _spells[Spells.Q].Cast();
            }
        }

        public static bool GetRCalc(Obj_AI_Hero target)
        {
            return false;
            var totalUnits = _spells[Spells.R].GetPrediction(target).CollisionObjects.Count(a => a.IsValidTarget());
            float distance = ObjectManager.Player.Distance(target);
            var damageReduction = ((totalUnits > 7)) ? 0.4 : (totalUnits == 0) ? 1.0 : (1 - (((totalUnits) * 8) / 100));
            return _spells[Spells.R].GetDamage(target) * damageReduction >=
                   (target.Health + (distance / 2000) * target.HPRegenRate);
        }
    }
}