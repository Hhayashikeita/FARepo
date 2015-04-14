using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LeagueSharp;
using LeagueSharp.Common;
using SharpDX;

namespace FuckingAwesomeThresh
{
    class ComboStateHandler
    {
        private static Obj_AI_Hero Player { get { return ObjectManager.Player; } }

        public static void Combo()
        {
            if (CheckHandler.GetQUnit() != null || CheckHandler.qTimer > Environment.TickCount)
                return;

            var target = TargetSelector.GetTarget(2000, TargetSelector.DamageType.Magical);

            if (!target.IsValidTarget() || target.HasAntiCc())
                return;

            if (CheckHandler.Spells[SpellSlot.R].IsReady() &&
                target.IsValidTarget(CheckHandler.Spells[SpellSlot.R].Range) && Player.Position.GetEnemiesInRange(CheckHandler.Spells[SpellSlot.R].Range).Where(a => a.IsValidTarget()).ToList().Count >= 2)
            {
                CheckHandler.Spells[SpellSlot.R].Cast();
            }

            if (CheckHandler.Spells[SpellSlot.E].IsReady() &&
                target.IsValidTarget(CheckHandler.Spells[SpellSlot.E].Range))
            {
                var ally =
                    ObjectManager.Get<Obj_AI_Hero>()
                        .FirstOrDefault(ally2 => ally2.IsAlly && !ally2.IsMe && ally2.Distance(Player) < 2000);
                if (ally == null)
                {
                    Pull(target);
                    return;
                }
                if (ally.Distance(Player) < ally.Distance(target))
                {
                    Pull(target);
                    return;
                }
                    Push(target);
            }

            if (CheckHandler.Spells[SpellSlot.Q].IsReady() &&
                target.IsValidTarget(CheckHandler.Spells[SpellSlot.Q].Range))
            {
                CheckHandler.Spells[SpellSlot.Q].Cast(target);
            }
        }

        public static void EngageCombo()
        {
            if (CheckHandler.GetQUnit() != null || CheckHandler.qTimer > Environment.TickCount)
                return;

            var target = TargetSelector.GetTarget(2000, TargetSelector.DamageType.Magical);

            if (!target.IsValidTarget() || target.HasAntiCc())
                return;

            if (GetBestAllyEngage() != null)
            {
                CheckHandler.Spells[SpellSlot.W].Cast(GetBestAllyEngage().Position);
            }

            if (CheckHandler.Spells[SpellSlot.R].IsReady() &&
                target.IsValidTarget(CheckHandler.Spells[SpellSlot.R].Range) && Player.Position.GetEnemiesInRange(CheckHandler.Spells[SpellSlot.R].Range).Where(a => a.IsValidTarget()).ToList().Count >= 2)
            {
                CheckHandler.Spells[SpellSlot.R].Cast();
            }

            if (CheckHandler.Spells[SpellSlot.E].IsReady() &&
                target.IsValidTarget(CheckHandler.Spells[SpellSlot.E].Range))
            {
                var ally =
                    ObjectManager.Get<Obj_AI_Hero>()
                        .FirstOrDefault(ally2 => ally2.IsAlly && !ally2.IsMe && ally2.Distance(Player) < 1000);
                if (ally == null)
                {
                    Push(target);
                    return;
                }
                if (ally.Distance(Player) < ally.Distance(target))
                {
                    Pull(target);
                    return;
                }
                Push(target);
            }

            if (CheckHandler.Spells[SpellSlot.Q].IsReady() &&
                target.IsValidTarget(CheckHandler.Spells[SpellSlot.Q].Range))
            {
                CheckHandler.Spells[SpellSlot.Q].Cast(target);
            }
        }

        public static void Pull(Obj_AI_Base target = null)
        {
            if (target == null)
            {
                target = TargetSelector.GetTarget(400, TargetSelector.DamageType.Physical);
                if (target == null)
                    return;
            }
            var pos = target.Position.Extend(Player.Position, Player.Distance(target) + 200);
            CheckHandler.Spells[SpellSlot.E].Cast(pos);
        }

        public static void Push(Obj_AI_Base target = null)
        {
            if (target == null)
            {
                target = TargetSelector.GetTarget(400, TargetSelector.DamageType.Physical);
                if (target == null)
                    return;
            }
            var pos = target.Position.Extend(Player.Position, Player.Distance(target) - 200);
            CheckHandler.Spells[SpellSlot.E].Cast(pos);
        }

        public static void FlashPull(Obj_AI_Base target = null)
        {
            if (target == null)
            {
                target = TargetSelector.GetTarget(400, TargetSelector.DamageType.Physical);
                if (target != null)
                {
                    Pull(target);
                    return;
                }
                target = TargetSelector.GetTarget(800, TargetSelector.DamageType.Physical);
                if (target == null)
                    return;
            }
            Player.Spellbook.CastSpell(Player.GetSpellSlot("summonerflash"), target.Position);
        }

        private static Obj_AI_Hero GetBestAllyEngage()
        {
            var distance = 0;
            var priorityNo = 0;
            Obj_AI_Hero selectedAlly = null;
            foreach (var ally in ObjectManager.Get<Obj_AI_Hero>().Where(hero => hero.IsAlly && hero.Distance(Player) < CheckHandler.Spells[SpellSlot.W].Range && !hero.IsMe))
            {
               var priority = Program.Config.Item("priority" + ally.ChampionName).GetValue<Slider>().Value;
                if (priority > priorityNo || priority == priorityNo && distance < Player.Distance(ally))
                {
                    distance = (int) Player.Distance(ally);
                    priorityNo = priority;
                    selectedAlly = ally;
                }
            }
            return selectedAlly;
        }

        private static Obj_AI_Hero GetBestAllyForShield()
        {
            return null;
        }
    }
}
