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

            //SuperDuperUlt();
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

        public static void CastQ(Obj_AI_Base target, bool smiteQ = false)
        {
            PredictionOutput qData = CheckHandler._spells[SpellSlot.Q].GetPrediction(target);
            if (CheckHandler._spells[SpellSlot.Q].IsReady() &&
                target.IsValidTarget(CheckHandler._spells[SpellSlot.Q].Range))
            {
                if (qData.Hitchance >= GetHitChance())
                {
                    CheckHandler._spells[SpellSlot.Q].Cast(qData.CastPosition);
                }
            }

            if (smiteQ && CheckHandler._spells[SpellSlot.Q].IsReady() &&
                target.IsValidTarget(CheckHandler._spells[SpellSlot.Q].Range) && qData.Hitchance == HitChance.Collision)
            {
                foreach (Obj_AI_Base minion in
                    from minion in
                        MinionManager.GetMinions(
                            Player.Position, CheckHandler._spells[SpellSlot.Q].Range, MinionTypes.All,
                            MinionTeam.NotAlly)
                    let projection =
                        minion.Position.To2D().ProjectOn(Player.ServerPosition.To2D(), target.ServerPosition.To2D())
                    where
                        projection.IsOnSegment &&
                        projection.SegmentPoint.Distance(minion) <=
                        minion.BoundingRadius + CheckHandler._spells[SpellSlot.Q].Width &&
                        Player.GetSpellSlot(CheckHandler.SmiteSpellName()).IsReady() &&
                        Player.GetSummonerSpellDamage(minion, Damage.SummonerSpell.Smite) > minion.Health
                    select minion)
                {
                    Player.Spellbook.CastSpell(Player.GetSpellSlot(CheckHandler.SmiteSpellName()), minion);
                    CheckHandler._spells[SpellSlot.Q].Cast(target);
                }
            }
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

            return Collision.GetCollision(new List<Vector3> { target.Position }, input).FirstOrDefault();
        }

        public static void SuperDuperUlt()
        {
            Obj_AI_Hero initialTarget = HeroManager.Enemies.FirstOrDefault(x => Player.Distance(x) <= CheckHandler._spells[SpellSlot.R].Range);
            Vector2 startPosition = Player.ServerPosition.To2D();
            if (initialTarget != null) {
                var projectionInfo = Player.ServerPosition.To2D()
                    .ProjectOn(
                        startPosition, Player.ServerPosition.To2D().Extend(initialTarget.ServerPosition.To2D(), 1200));

                int count = 1; // always 1 for the inital target

                // ReSharper disable once LoopCanBeConvertedToQuery disabled for now..
                foreach (Obj_AI_Hero hero in
                    HeroManager.Enemies.Where(x => x.NetworkId != initialTarget.NetworkId && x.IsValidTarget(1200)))
                {
                    if (projectionInfo.IsOnSegment &&
                        projectionInfo.LinePoint.Distance(hero, true) <=
                        CheckHandler._spells[SpellSlot.R].Range * CheckHandler._spells[SpellSlot.R].Range)
                    {
                        count = count + 1;
                    }
                }

                if (count >= 2)
                {
                    if (CheckHandler._spells[SpellSlot.R].IsReady())
                    {
                        CheckHandler._spells[SpellSlot.R].Cast(initialTarget);
                    }
                }
            }
        }

        public static void DrawProjection()
        {
            Obj_AI_Hero target = TargetSelector.GetTarget(1200f, TargetSelector.DamageType.Physical);
            Vector2 startPosition =
                Player.ServerPosition.Extend(target.ServerPosition, CheckHandler._spells[SpellSlot.R].Range).To2D();
            Vector3 endPosition = Player.ServerPosition.Extend(target.ServerPosition, 1200);
            // 1200 is knockback distance

            foreach (Obj_AI_Hero hero in HeroManager.Enemies.Where(x => x.NetworkId != target.NetworkId))
            {
                var projection = Player.ServerPosition.To2D()
                 .ProjectOn(
                     startPosition, Player.ServerPosition.To2D().Extend(target.ServerPosition.To2D(), 1200));
                Vector2 wtsPlayer = Drawing.WorldToScreen(Player.Position);
                Vector2 wtsPred = Drawing.WorldToScreen(endPosition);
                Render.Circle.DrawCircle(
                    startPosition.To3D(), CheckHandler._spells[SpellSlot.R].Width, System.Drawing.Color.Aquamarine);
                Render.Circle.DrawCircle(
                    startPosition.To3D(), CheckHandler._spells[SpellSlot.R].Width, System.Drawing.Color.SpringGreen);
                Drawing.DrawLine(wtsPlayer, wtsPred, 1, System.Drawing.Color.Red);
                Render.Circle.DrawCircle(projection.LinePoint.To3D(), 100, System.Drawing.Color.LawnGreen);
            }
            /*LeagueSharp.Common.Geometry.ProjectionInfo projection =
                Player.ServerPosition.To2D().ProjectOn(startPosition.To2D(), endPosition.To2D());

            Vector2 wtsPlayer = Drawing.WorldToScreen(Player.Position);
            Vector2 wtsPred = Drawing.WorldToScreen(endPosition);
            Render.Circle.DrawCircle(
                startPosition, CheckHandler._spells[SpellSlot.R].Width, System.Drawing.Color.Aquamarine);
            Render.Circle.DrawCircle(
                startPosition, CheckHandler._spells[SpellSlot.R].Width, System.Drawing.Color.SpringGreen);
            Drawing.DrawLine(wtsPlayer, wtsPred, 1, System.Drawing.Color.Red);
            Render.Circle.DrawCircle(projection.LinePoint.To3D(), 100, System.Drawing.Color.LawnGreen);
            Render.Circle.DrawCircle(projection.SegmentPoint.To3D(), 100, System.Drawing.Color.Indigo);*/
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