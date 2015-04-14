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

namespace iFuckingAwesomeGraves
{
    internal static class Program
    {
        private static Orbwalking.Orbwalker _orbwalker;
        private static Menu _menu;
        private static Obj_AI_Hero _player;
        private static Spell _r2;
        // ReSharper disable once InconsistentNaming
        private static readonly Dictionary<SpellSlot, Spell> _spells = new Dictionary<SpellSlot, Spell>
        {
            { SpellSlot.Q, new Spell(SpellSlot.Q, 800f) },
            { SpellSlot.W, new Spell(SpellSlot.W, 950f) },
            { SpellSlot.E, new Spell(SpellSlot.E, 425f) },
            { SpellSlot.R, new Spell(SpellSlot.R, 1100f) }
        };

        private static void Main(string[] args)
        {
            CustomEvents.Game.OnGameLoad += Game_OnGameLoad;
        }

        #region Events

        private static void Game_OnGameLoad(EventArgs args)
        {
            _player = ObjectManager.Player;
            Notifications.AddNotification(new Notification("THIS IS NOT DONE", 2));

            LoadSpells();
            CreateMenu();

            Game.OnUpdate += Game_OnUpdate;
            Drawing.OnDraw += OnDraw;
        }

        private static void Game_OnUpdate(EventArgs args)
        {
            if (_player.IsDead)
            {
                return;
            }

            CastImmobileSpells();

            switch (_orbwalker.ActiveMode)
            {
                case Orbwalking.OrbwalkingMode.Combo:
                    DoCombo();
                    break;
                case Orbwalking.OrbwalkingMode.Mixed:
                    DoHarass();
                    break;
                case Orbwalking.OrbwalkingMode.LaneClear:
                    DoLaneclear();
                    break;
            }
        }

        private static void OnDraw(EventArgs args)
        {
            //TODO drawing
        }

        #endregion

        #region ActiveModes

        private static void DoCombo()
        {
            if (_menu.Item("useQC").GetValue<bool>())
            {
                CastBuckshot();
            }
            if (_menu.Item("useWC").GetValue<bool>())
            {
                CastSmokeScreen();
            }
            CastQuickdraw();
            CastCollateralDamage();
            //CollateralDamageKs();
        }

        private static void DoHarass()
        {
            if (_menu.Item("useQH").GetValue<bool>())
            {
                CastBuckshot();
            }
            if (_menu.Item("useWH").GetValue<bool>())
            {
                CastSmokeScreen();
            }
        }

        private static void DoLaneclear() {}

        #endregion

        #region spell casting

        private static void CastImmobileSpells()
        {
            var qTarget = TargetSelector.GetTarget(_spells[SpellSlot.Q].Range, TargetSelector.DamageType.Physical);
            var wTarget = TargetSelector.GetTarget(_spells[SpellSlot.W].Range, TargetSelector.DamageType.Physical);

            if (_menu.Item("qImmobile").GetValue<bool>() && _spells[SpellSlot.Q].IsReady() &&
                _spells[SpellSlot.Q].IsInRange(qTarget) && qTarget.IsValidTarget(_spells[SpellSlot.Q].Range))
            {
                _spells[SpellSlot.Q].CastIfHitchanceEquals(qTarget, HitChance.Immobile);
            }

            if (_menu.Item("wImmobile").GetValue<bool>() && _spells[SpellSlot.W].IsReady() &&
                _spells[SpellSlot.W].GetPrediction(wTarget, true).AoeTargetsHitCount >=
                _menu.Item("wCount").GetValue<Slider>().Value)
            {
                _spells[SpellSlot.W].CastIfHitchanceEquals(wTarget, HitChance.Immobile);
            }
        }

        private static void CastBuckshot()
        {
            var qTarget = TargetSelector.GetTarget(_spells[SpellSlot.Q].Range, TargetSelector.DamageType.Physical);
            if (qTarget.IsValidTarget(_spells[SpellSlot.Q].Range))
            {
                if (_spells[SpellSlot.Q].IsReady() && _spells[SpellSlot.Q].IsInRange(qTarget))
                {
                    _spells[SpellSlot.Q].CastIfHitchanceEquals(qTarget, GetCustomHitChance());
                }
            }
        }

        private static void CastSmokeScreen()
        {
            var wTarget = TargetSelector.GetTarget(_spells[SpellSlot.W].Range, TargetSelector.DamageType.Physical);
            if (wTarget.IsValidTarget(_spells[SpellSlot.W].Range))
            {
                if (_spells[SpellSlot.W].IsReady() && _spells[SpellSlot.W].IsInRange(wTarget))
                {
                    _spells[SpellSlot.W].CastIfHitchanceEquals(wTarget, GetCustomHitChance());
                }
            }
            //TODO
        }

        private static void CastQuickdraw()
        {
            //who needs to use E anyway :S
        }

        /// <summary>
        ///     Matey, If target is killable and higher then inital R Range, then take into account the extra 800 - cone range if
        ///     the ult collides with a minion / champion / yasuo wall.
        /// </summary>
        private static void CastCollateralDamage()
        {
            var rTarget = TargetSelector.GetTarget(_spells[SpellSlot.R].Range, TargetSelector.DamageType.Physical);
            if (_menu.Item("useRC").GetValue<bool>() && rTarget.IsValidTarget(_spells[SpellSlot.R].Range))
            {
                if (_menu.Item("overkillCheck").GetValue<bool>() && _spells[SpellSlot.Q].IsReady() &&
                    _spells[SpellSlot.Q].IsInRange(rTarget))
                {
                    return;
                }
                if (_spells[SpellSlot.R].IsReady() && _spells[SpellSlot.R].IsInRange(rTarget))
                {
                    if (_spells[SpellSlot.R].GetDamage(rTarget) > rTarget.Health + 10)
                    {
                        _spells[SpellSlot.R].CastIfHitchanceEquals(rTarget, GetCustomHitChance());
                    }
                    else
                    {
                        foreach (Obj_AI_Hero source in
                            from source in
                                HeroManager.Enemies.Where(hero => hero.IsValidTarget(_spells[SpellSlot.R].Range))
                            let prediction = _spells[SpellSlot.R].GetPrediction(source, true)
                            where
                                _player.Distance(source) <= _spells[SpellSlot.R].Range &&
                                prediction.AoeTargetsHitCount >= _menu.Item("rCount").GetValue<Slider>().Value
                            select source)
                        {
                            _spells[SpellSlot.R].Cast(source);
                        }
                    }
                }
            }
        }

        private static void CollateralDamageKs()
        {
            foreach (Obj_AI_Hero target in _player.Position.GetEnemiesInRange(1900).Where(e => e.IsValidTarget()))
            {
                if (R2Damage(target) < target.Health)
                {
                    return;
                }
                var pred = _r2.GetPrediction(target);
                if (
                    pred.CollisionObjects.Count(
                        a => a.IsEnemy && a.IsValid<Obj_AI_Hero>() && _player.Distance(a) < 1100) > 0)
                {
                    _r2.Cast(pred.CastPosition);
                }
                else
                {
                    foreach (var target2 in _player.Position.GetEnemiesInRange(1100).Where(e => e.IsValidTarget()))
                    {
                        var sector =
                            new Geometry.Sector(
                                _spells[SpellSlot.R].GetPrediction(target2).UnitPosition.To2D(),
                                _player.Position.To2D()
                                    .Extend(
                                        _spells[SpellSlot.R].GetPrediction(target2).UnitPosition.To2D(),
                                        _player.Distance(_spells[SpellSlot.R].GetPrediction(target2).UnitPosition) + 100),
                                60 * (float) Math.PI / 180, 800).ToPolygon();
                        if (!sector.IsOutside(target2.Position.To2D()))
                        {
                            _r2.Cast(_r2.GetPrediction(target2).CastPosition);
                        }
                    }
                }
            }
        }

        #endregion

        #region menu and spells

        private static void LoadSpells()
        {
            _spells[SpellSlot.Q].SetSkillshot(
                0.26f, 10f * 2 * (float) Math.PI / 180, 1950f, false, SkillshotType.SkillshotCone);
            _spells[SpellSlot.W].SetSkillshot(0.30f, 250f, 1650f, false, SkillshotType.SkillshotCircle);
            _spells[SpellSlot.R].SetSkillshot(0.22f, 150f, 2100f, true, SkillshotType.SkillshotLine);
            _r2 = new Spell(SpellSlot.R, 1900);
            _r2.SetSkillshot(0.22f, 150f, 2100f, true, SkillshotType.SkillshotLine);
        }

        private static void CreateMenu()
        {
            _menu = new Menu("iFuckingAwesomeGraves", "ifag", true);

            TargetSelector.AddToMenu(_menu.AddSubMenu(new Menu("Target Selector", "Target Selector")));

            _orbwalker = new Orbwalking.Orbwalker(_menu.AddSubMenu(new Menu("Orbwalker", "Orbwalker")));

            var comboMenu = new Menu("Graves - Combo", "com.ifag.combo");
            {
                var buckshotMenu = new Menu("Buckshot (Q)", "com.ifag.combo.buckshot");
                {
                    buckshotMenu.AddItem(new MenuItem("useQC", "Use Q Combo").SetValue(true));
                    buckshotMenu.AddItem(new MenuItem("qImmobile", "Auto Q Immobile").SetValue(true));
                    // TODO: make it workerino
                    //TODO auto Q if x enemies will be hit
                    comboMenu.AddSubMenu(buckshotMenu);
                }
                //
                var smokescreenMenu = new Menu("Smokescreen (W)", "com.ifag.combo.smokescreen");
                {
                    smokescreenMenu.AddItem(new MenuItem("useWC", "Use W Combo").SetValue(true));
                    smokescreenMenu.AddItem(new MenuItem("wImmobile", "Use W on immobile").SetValue(false));
                    smokescreenMenu.AddItem(
                        new MenuItem("wCount", "Min Enemies to auto W").SetValue(new Slider(3, 0, 5)));
                    comboMenu.AddSubMenu(smokescreenMenu);
                }
                var collateralMenu = new Menu("Collateral Damage (R)", "com.ifag.combo.collateral");
                {
                    collateralMenu.AddItem(new MenuItem("useRC", "Use R Combo").SetValue(true));
                    collateralMenu.AddItem(new MenuItem("rCount", "Min Enemies for R").SetValue(new Slider(3, 0, 5)));
                    //TODO more options for R?
                    comboMenu.AddSubMenu(collateralMenu);
                }
                _menu.AddSubMenu(comboMenu);
            }

            var harassMenu = new Menu("Graves - Harass", "com.ifag.harass");
            {
                harassMenu.AddItem(new MenuItem("useQH", "Use Q Harass").SetValue(true));
                harassMenu.AddItem(new MenuItem("useWH", "Use W Harass").SetValue(true));
                _menu.AddSubMenu(harassMenu);
                //TODO harass mana
            }

            var farmMenu = new Menu("Graves - Farm", "com.ifag.farm");
            {
                var laneclearMenu = new Menu("Laneclear", "com.ifag.farm.lc");
                {
                    laneclearMenu.AddItem(new MenuItem("useQLC", "Use Q Laneclear").SetValue(false));
                    laneclearMenu.AddItem(new MenuItem("qlcCount", "Min minions for Q").SetValue(new Slider(3, 0, 10)));
                }
                var lasthitMenu = new Menu("Lasthit", "com.ifag.farm.lh");
                {
                    lasthitMenu.AddItem(new MenuItem("useQLH", "Use Q Last hit").SetValue(false));
                    lasthitMenu.AddItem(new MenuItem("qlhCount", "Min Minions for Q").SetValue(new Slider(3, 0, 10)));
                }
                farmMenu.AddSubMenu(laneclearMenu);
                farmMenu.AddSubMenu(lasthitMenu);
                _menu.AddSubMenu(farmMenu);
            }

            var miscMenu = new Menu("Graves - Misc", "com.ifag.misc");
            {
                miscMenu.AddItem(new MenuItem("eCheck", "Turret Safety for E").SetValue(true));
                miscMenu.AddItem(
                    new MenuItem("hitchance", "Hitchance").SetValue(
                        new StringList(new[] { "Low", "Medium", "High", "Very High" }, 2)));
                miscMenu.AddItem(new MenuItem("overkillCheck", "Check Overkill").SetValue(true));
                _menu.AddSubMenu(miscMenu);
            }


            _menu.AddToMainMenu();
        }

        private static double R2Damage(Obj_AI_Hero target)
        {
            if (_spells[SpellSlot.R].Level == 0)
            {
                return 0;
            }
            return _player.CalcDamage(
                target, Damage.DamageType.Physical,
                new double[] { 200, 320, 440 }[_spells[SpellSlot.R].Level - 1] + 1.2 * _player.FlatPhysicalDamageMod);
        }

        private static HitChance GetCustomHitChance()
        {
            switch (_menu.Item("hitchance").GetValue<StringList>().SelectedIndex)
            {
                case 0:
                    return HitChance.Low;
                case 1:
                    return HitChance.Medium;
                case 2:
                    return HitChance.High;
                case 3:
                    return HitChance.VeryHigh;
                default:
                    return HitChance.High;
            }
        }

        #endregion
    }
}