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
using SharpDX;
using Collision = LeagueSharp.Common.Collision;

namespace FuckingAwesomeLeeSinReborn
{
    internal static class StateHandler
    {
        private static Obj_AI_Hero Player
        {
            get { return ObjectManager.Player; }
        }

        public static void Combo()
        {
            var target = TargetSelector.GetTarget(1200, TargetSelector.DamageType.Physical);

            if (!target.IsValidTarget())
            {
                return;
            }

            var useQ = Program.Config.Item("CQ").GetValue<bool>();
            var useE = Program.Config.Item("CE").GetValue<bool>();
            var useR = Program.Config.Item("CR").GetValue<bool>();
            var forcePassive = Program.Config.Item("CpassiveCheck").GetValue<bool>();
            var minPassive = Program.Config.Item("CpassiveCheckCount").GetValue<Slider>().Value;

            CheckHandler.UseItems(target);

            if (useR && useQ && CheckHandler._spells[SpellSlot.R].IsReady() &&
                CheckHandler._spells[SpellSlot.Q].IsReady() && (CheckHandler.QState || target.HasQBuff()) &&
                CheckHandler._spells[SpellSlot.R].GetDamage(target) +
                (CheckHandler.QState ? CheckHandler._spells[SpellSlot.Q].GetDamage(target) : 0) +
                CheckHandler.Q2Damage(
                    target,
                    CheckHandler._spells[SpellSlot.R].GetDamage(target) +
                    (CheckHandler.QState ? CheckHandler._spells[SpellSlot.Q].GetDamage(target) : 0)) > target.Health)
            {
                if (CheckHandler.QState)
                {
                    CheckHandler._spells[SpellSlot.Q].CastIfHitchanceEquals(target, HitChance.High);
                    return;
                }
                CheckHandler._spells[SpellSlot.R].CastOnUnit(target);
                Utility.DelayAction.Add(300, () => CheckHandler._spells[SpellSlot.Q].Cast());
            }

            if (useR && CheckHandler._spells[SpellSlot.R].IsReady() &&
                CheckHandler._spells[SpellSlot.R].GetDamage(target) > target.Health)
            {
                CheckHandler._spells[SpellSlot.R].CastOnUnit(target);
                return;
            }

            if (useQ && !CheckHandler.QState && CheckHandler._spells[SpellSlot.Q].IsReady() && target.HasQBuff() &&
                (CheckHandler.LastQ + 2700 < Environment.TickCount ||
                 CheckHandler._spells[SpellSlot.Q].GetDamage(target, 1) > target.Health ||
                 target.Distance(Player) > Orbwalking.GetRealAutoAttackRange(Player) + 50))
            {
                CheckHandler._spells[SpellSlot.Q].Cast();
                return;
            }

            if (forcePassive && CheckHandler.PassiveStacks > minPassive &&
                Orbwalking.GetRealAutoAttackRange(Player) > Player.Distance(target))
            {
                return;
            }

            if (CheckHandler._spells[SpellSlot.Q].IsReady() && useQ)
            {
                if (CheckHandler.QState && target.Distance(Player) < CheckHandler._spells[SpellSlot.Q].Range)
                {
                    CastQ(target, Program.Config.Item("smiteQ").GetValue<bool>());
                    return;
                }
            }

            if (CheckHandler._spells[SpellSlot.E].IsReady() && useE)
            {
                if (CheckHandler.EState && target.Distance(Player) < CheckHandler._spells[SpellSlot.E].Range - 50)
                {
                    CheckHandler._spells[SpellSlot.E].Cast();
                    return;
                }
                if (!CheckHandler.EState && target.Distance(Player) > Orbwalking.GetRealAutoAttackRange(Player) + 50)
                {
                    CheckHandler._spells[SpellSlot.E].Cast();
                }
            }
        }

        public static void StarCombo()
        {
            var target = TargetSelector.GetTarget(1200, TargetSelector.DamageType.Physical);
            if (target == null)
            {
                Orbwalking.Orbwalk(null, Game.CursorPos);
                return;
            }

            Orbwalking.Orbwalk(Orbwalking.InAutoAttackRange(target) ? target : null, Game.CursorPos);

            CheckHandler.UseItems(target);

            if (!target.IsValidTarget())
            {
                return;
            }

            if (target.HasBuffOfType(BuffType.Knockback) && target.Distance(Player) > 300 && target.HasQBuff() &&
                !CheckHandler.QState)
            {
                CheckHandler._spells[SpellSlot.Q].Cast();
                return;
            }

            if (!CheckHandler._spells[SpellSlot.R].IsReady())
            {
                return;
            }

            if (CheckHandler._spells[SpellSlot.Q].IsReady() && CheckHandler.QState)
            {
                CastQ(target, Program.Config.Item("smiteQ").GetValue<bool>());
                return;
            }
            if (target.HasQBuff() && !target.HasBuffOfType(BuffType.Knockback))
            {
                if (target.Distance(Player) < CheckHandler._spells[SpellSlot.R].Range &&
                    CheckHandler._spells[SpellSlot.R].IsReady())
                {
                    CheckHandler._spells[SpellSlot.R].CastOnUnit(target);
                    return;
                }
                if (target.Distance(Player) < 600 && CheckHandler.WState)
                {
                    WardjumpHandler.Jump(
                        Player.Position.Extend(target.Position, Player.Position.Distance(target.Position) - 50));
                }
            }
        }

        public static void Harass()
        {
            var target = TargetSelector.GetTarget(1200, TargetSelector.DamageType.Physical);

            if (!target.IsValidTarget())
            {
                return;
            }

            var useQ = Program.Config.Item("HQ").GetValue<bool>();
            var useE = Program.Config.Item("HE").GetValue<bool>();
            var forcePassive = Program.Config.Item("HpassiveCheck").GetValue<bool>();
            var minPassive = Program.Config.Item("HpassiveCheckCount").GetValue<Slider>().Value;


            if (!CheckHandler.QState && CheckHandler.LastQ + 200 < Environment.TickCount && useQ && !CheckHandler.QState &&
                CheckHandler._spells[SpellSlot.Q].IsReady() && target.HasQBuff() &&
                (CheckHandler.LastQ + 2700 < Environment.TickCount ||
                 CheckHandler._spells[SpellSlot.Q].GetDamage(target, 1) > target.Health ||
                 target.Distance(Player) > Orbwalking.GetRealAutoAttackRange(Player) + 50))
            {
                CheckHandler._spells[SpellSlot.Q].Cast();
                return;
            }

            if (forcePassive && CheckHandler.PassiveStacks > minPassive &&
                Orbwalking.GetRealAutoAttackRange(Player) > Player.Distance(target))
            {
                return;
            }

            if (CheckHandler._spells[SpellSlot.Q].IsReady() && useQ && CheckHandler.LastQ + 200 < Environment.TickCount)
            {
                if (CheckHandler.QState && target.Distance(Player) < CheckHandler._spells[SpellSlot.Q].Range)
                {
                    CastQ(target);
                    return;
                }
            }

            if (CheckHandler._spells[SpellSlot.E].IsReady() && useE && CheckHandler.LastE + 200 < Environment.TickCount)
            {
                if (CheckHandler.EState && target.Distance(Player) < CheckHandler._spells[SpellSlot.E].Range)
                {
                    CheckHandler._spells[SpellSlot.E].Cast();
                    return;
                }
                if (!CheckHandler.EState && target.Distance(Player) > Orbwalking.GetRealAutoAttackRange(Player) + 50)
                {
                    CheckHandler._spells[SpellSlot.E].Cast();
                }
            }
        }

        private static void Wave()
        {
            Obj_AI_Base target = MinionManager.GetMinions(1100).FirstOrDefault();

            if (!target.IsValidTarget() || target == null)
            {
                return;
            }


            CheckHandler.UseItems(target, true);

            var useQ = Program.Config.Item("QWC").GetValue<bool>();
            var useE = Program.Config.Item("EWC").GetValue<bool>();

            if (useQ && !CheckHandler.QState && CheckHandler._spells[SpellSlot.Q].IsReady() && target.HasQBuff() &&
                (CheckHandler.LastQ + 2700 < Environment.TickCount ||
                 CheckHandler._spells[SpellSlot.Q].GetDamage(target, 1) > target.Health ||
                 target.Distance(Player) > Orbwalking.GetRealAutoAttackRange(Player) + 50))
            {
                CheckHandler._spells[SpellSlot.Q].Cast();
                return;
            }

            if (CheckHandler._spells[SpellSlot.Q].IsReady() && useQ && CheckHandler.LastQ + 200 < Environment.TickCount)
            {
                if (CheckHandler.QState && target.Distance(Player) < CheckHandler._spells[SpellSlot.Q].Range)
                {
                    CheckHandler._spells[SpellSlot.Q].Cast(target);
                    return;
                }
            }

            if (CheckHandler._spells[SpellSlot.E].IsReady() && useE && CheckHandler.LastE + 200 < Environment.TickCount)
            {
                if (CheckHandler.EState && target.Distance(Player) < CheckHandler._spells[SpellSlot.E].Range)
                {
                    CheckHandler._spells[SpellSlot.E].Cast();
                }
            }
        }

        public static void JungleClear()
        {
            Obj_AI_Base target =
                MinionManager.GetMinions(1100, MinionTypes.All, MinionTeam.Neutral, MinionOrderTypes.MaxHealth)
                    .FirstOrDefault();

            if (!target.IsValidTarget() || target == null)
            {
                Wave();
                return;
            }

            var useQ = Program.Config.Item("QJ").GetValue<bool>();
            var useW = Program.Config.Item("WJ").GetValue<bool>();
            var useE = Program.Config.Item("EJ").GetValue<bool>();

            CheckHandler.UseItems(target, true);

            if (CheckHandler.PassiveStacks > 0 || CheckHandler.LastSpell + 400 > Environment.TickCount)
            {
                return;
            }

            if (CheckHandler._spells[SpellSlot.Q].IsReady() && useQ)
            {
                if (CheckHandler.QState && target.Distance(Player) < CheckHandler._spells[SpellSlot.Q].Range &&
                    CheckHandler.LastQ + 200 < Environment.TickCount)
                {
                    CheckHandler._spells[SpellSlot.Q].Cast(target);
                    CheckHandler.LastSpell = Environment.TickCount;
                    return;
                }
                CheckHandler._spells[SpellSlot.Q].Cast();
                CheckHandler.LastSpell = Environment.TickCount;
                return;
            }

            if (CheckHandler._spells[SpellSlot.W].IsReady() && useW)
            {
                if (CheckHandler.WState && target.Distance(Player) < Orbwalking.GetRealAutoAttackRange(Player))
                {
                    CheckHandler._spells[SpellSlot.W].CastOnUnit(Player);
                    CheckHandler.LastSpell = Environment.TickCount;
                    return;
                }
                if (CheckHandler.WState)
                {
                    return;
                }
                CheckHandler._spells[SpellSlot.W].Cast();
                CheckHandler.LastSpell = Environment.TickCount;
                return;
            }

            if (CheckHandler._spells[SpellSlot.E].IsReady() && useE)
            {
                if (CheckHandler.EState && target.Distance(Player) < CheckHandler._spells[SpellSlot.E].Range)
                {
                    CheckHandler._spells[SpellSlot.E].Cast();
                    CheckHandler.LastSpell = Environment.TickCount;
                    return;
                }
                if (CheckHandler.EState)
                {
                    return;
                }
                CheckHandler._spells[SpellSlot.E].Cast();
                CheckHandler.LastSpell = Environment.TickCount;
            }
        }

        private static void CastQ(Obj_AI_Base target, bool smiteQ = false)
        {
            var qData = CheckHandler._spells[SpellSlot.Q].GetPrediction(target);
            if (CheckHandler._spells[SpellSlot.Q].IsReady() &&
                target.IsValidTarget(CheckHandler._spells[SpellSlot.Q].Range))
            {
                if (qData.Hitchance >= GetHitChance())
                {
                    CheckHandler._spells[SpellSlot.Q].Cast(qData.CastPosition);
                }
            }

            if (smiteQ && CheckHandler._spells[SpellSlot.Q].IsReady() &&
                target.IsValidTarget(CheckHandler._spells[SpellSlot.Q].Range) && qData.Hitchance == HitChance.Collision && Player.Distance(target) > Orbwalking.GetRealAutoAttackRange(target))
            {
                Obj_AI_Base firstMinion = GetFirstCollisionMinion(Player, target);
                if (firstMinion.Distance(Player) <= 600 && firstMinion != null &&
                    Player.GetSummonerSpellDamage(firstMinion, Damage.SummonerSpell.Smite) >= firstMinion.Health)
                {
                    Player.Spellbook.CastSpell(Player.GetSpellSlot(CheckHandler.SmiteSpellName()), firstMinion);
                    CheckHandler._spells[SpellSlot.Q].Cast(qData.CastPosition);
                }

            }

            /*else if (smiteQ && CheckHandler._spells[SpellSlot.Q].IsReady() &&
                     target.IsValidTarget(CheckHandler._spells[SpellSlot.Q].Range) &&
                     qData.CollisionObjects.Count(a => a.NetworkId != target.NetworkId && a.IsMinion) == 1 &&
                     Player.GetSpellSlot(CheckHandler.SmiteSpellName()).IsReady())
            {
                Obj_AI_Base minionSmite =
                    qData.CollisionObjects.Where(a => a.NetworkId != target.NetworkId && a.IsMinion).ToList()[0];
                Player.Spellbook.CastSpell(
                    Player.GetSpellSlot(CheckHandler.SmiteSpellName()), minionSmite);
                CheckHandler._spells[SpellSlot.Q].Cast(qData.CastPosition);
            }*/
        }

        /// <summary>
        ///     Gets the minions in the Collision path from the source target to the given Position then creates a new prediction
        ///     input based on those details and compiles to list. thanks bye
        /// </summary>
        /// <param name="source"> the source mate </param>
        /// <param name="target"> the target mate </param>
        /// <returns> A Nice List of minions currently blocking your Q HIT M8 </returns>
        public static Obj_AI_Base GetFirstCollisionMinion(Obj_AI_Hero source, Obj_AI_Base target)
        {
            // ReSharper disable once UseObjectOrCollectionInitializer
            PredictionInput input = new PredictionInput
            {
                Unit = source,
                Radius = CheckHandler._spells[SpellSlot.Q].Width,
                Delay = CheckHandler._spells[SpellSlot.Q].Delay,
                Speed = CheckHandler._spells[SpellSlot.Q].Speed,
                Range = CheckHandler._spells[SpellSlot.Q].Range
            };

            input.CollisionObjects[0] = CollisionableObjects.Minions;

            return
                Collision.GetCollision(new List<Vector3> { target.Position }, input).FirstOrDefault();
        }

        private static HitChance GetHitChance()
        {
            switch (Program.Config.Item("qHitchance").GetValue<StringList>().SelectedIndex)
            {
                case 0: // LOW
                    return HitChance.Low;
                case 1: // medium
                    return HitChance.Medium;
                case 2: // high
                    return HitChance.High;
                case 3: // veryhigh
                    return HitChance.VeryHigh;
                default:
                    return HitChance.High;
            }
        }
    }
}