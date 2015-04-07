using System;
using LeagueSharp;
using LeagueSharp.Common;

namespace FuckingAwesomeRiven
{
    internal class DamageHandler
    {
        public static double RBonus
        {
            get { return (ObjectManager.Player.FlatPhysicalDamageMod + ObjectManager.Player.BaseAttackDamage)*0.2; }
        }

        public static double GetComboDmg(bool useR, Obj_AI_Hero target)
        {
            if (MenuHandler.Config.Item("urfMode").GetValue<bool>())
            {
                return 1000;
            }
            if (!target.IsValidTarget())
            {
                return 0;
            }

            double dmg = 0;
            double baseAd = ObjectManager.Player.BaseAttackDamage;
            var bonusAd = ObjectManager.Player.FlatPhysicalDamageMod +
                          (useR && !CheckHandler.RState && SpellHandler.Spells[SpellSlot.R].IsReady()
                              ? 0.2*
                                (ObjectManager.Player.BaseAttackDamage + ObjectManager.Player.FlatPhysicalDamageMod)
                              : 0);
            var passiveCount = 0;
            if (SpellHandler.Spells[SpellSlot.Q].IsReady())
            {
                dmg += (3 - CheckHandler.QCount)*
                       (new double[] {10, 30, 50, 70, 90}[SpellHandler.Spells[SpellSlot.Q].Level - 1] +
                        ((baseAd + bonusAd)/100)*
                        new double[] {40, 45, 50, 55, 60}[SpellHandler.Spells[SpellSlot.Q].Level - 1]);
                passiveCount += 3 - CheckHandler.QCount;
            }

            if (SpellHandler.Spells[SpellSlot.W].IsReady())
            {
                dmg += (new double[] {50, 80, 110, 140, 170}[SpellHandler.Spells[SpellSlot.W].Level - 1]) +
                       1*bonusAd;
                passiveCount++;
            }

            if (SpellHandler.Spells[SpellSlot.E].IsReady())
            {
                passiveCount++;
            }

            if (SpellHandler.Spells[SpellSlot.R].IsReady() && useR)
            {
                passiveCount++;
            }

            dmg += passiveCount*
                   (5 +
                    Math.Max(
                        5*Math.Floor((double) ((ObjectManager.Player.Level + 2)/3)) + 10,
                        10*Math.Floor((double) ((ObjectManager.Player.Level + 2)/3)) - 15)*(baseAd + bonusAd)/
                    100);
            dmg += (baseAd + bonusAd)*passiveCount;

            if (useR && SpellHandler.Spells[SpellSlot.R].IsReady())
            {
                var targethp = (target.MaxHealth - target.Health > 0) ? target.MaxHealth - target.Health : 1;
                dmg += new double[] {80, 120, 160}[SpellHandler.Spells[SpellSlot.R].Level - 1] +
                       0.6*bonusAd*((targethp)/target.MaxHealth*2.67 + 1);
            }

            dmg += (baseAd + bonusAd);
            return ObjectManager.Player.CalcDamage(target, Damage.DamageType.Physical, dmg);
        }

        public static double PassiveDamage(Obj_AI_Base target, bool calcR)
        {
            return ((20 + ((Math.Floor((double) ObjectManager.Player.Level/3))*5))/100)*
                   (ObjectManager.Player.BaseAttackDamage + ObjectManager.Player.FlatPhysicalDamageMod +
                    (calcR ? RBonus : 0));
        }

        public static double QDamage(Obj_AI_Base target, bool calcR)
        {
            return new double[] {10, 30, 50, 70, 90}[SpellHandler.Spells[SpellSlot.Q].Level] +
                   ((ObjectManager.Player.BaseAttackDamage + ObjectManager.Player.FlatPhysicalDamageMod +
                     (calcR ? RBonus : 0))/100)*
                   new double[] {40, 45, 50, 55, 60}[SpellHandler.Spells[SpellSlot.Q].Level];
        }

        public static double WDamage(Obj_AI_Base target, bool calcR)
        {
            return new double[] {50, 80, 110, 140, 170}[SpellHandler.Spells[SpellSlot.W].Level] +
                   1*ObjectManager.Player.FlatPhysicalDamageMod + (calcR ? RBonus : 0);
        }

        public static double RDamage(Obj_AI_Base target, int healthMod = 0)
        {
            var health = (target.MaxHealth - (target.Health - healthMod)) > 0
                ? (target.MaxHealth - (target.Health - healthMod))
                : 1;
            return (new double[] {80, 120, 160}[SpellHandler.Spells[SpellSlot.R].Level] +
                    0.6*ObjectManager.Player.FlatPhysicalDamageMod)*(health/target.MaxHealth*2.67 + 1);
        }
    }
}