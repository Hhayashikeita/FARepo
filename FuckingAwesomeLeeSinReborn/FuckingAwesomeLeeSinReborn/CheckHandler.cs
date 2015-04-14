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
using SharpDX;

namespace FuckingAwesomeLeeSinReborn
{
    internal static class CheckHandler
    {
        public static int LastQ, LastQ2, LastW;
        private static int _lastW2;
        public static int LastE, LastE2;
        private static int _lastR;
        public static int LastWard, LastSpell, PassiveStacks;
        public static bool CheckQ = true;
        // ReSharper disable once InconsistentNaming
        public static readonly Dictionary<SpellSlot, Spell> _spells = new Dictionary<SpellSlot, Spell>
        {
            { SpellSlot.Q, new Spell(SpellSlot.Q, 1100) },
            { SpellSlot.W, new Spell(SpellSlot.W, 700) },
            { SpellSlot.E, new Spell(SpellSlot.E, 430) },
            { SpellSlot.R, new Spell(SpellSlot.R, 375) }
        };

        private static readonly int[] SmitePurple = { 3713, 3726, 3725, 3726, 3723 };
        private static readonly int[] SmiteGrey = { 3711, 3722, 3721, 3720, 3719 };
        private static readonly int[] SmiteRed = { 3715, 3718, 3717, 3716, 3714 };
        private static readonly int[] SmiteBlue = { 3706, 3710, 3709, 3708, 3707 };

        private static Obj_AI_Hero Player
        {
            get { return ObjectManager.Player; }
        }

        public static bool QState
        {
            get { return _spells[SpellSlot.Q].Instance.Name == "BlindMonkQOne"; }
        }

        public static bool WState
        {
            get { return _spells[SpellSlot.W].Instance.Name == "BlindMonkWOne"; }
        }

        public static bool EState
        {
            get { return _spells[SpellSlot.E].Instance.Name == "BlindMonkEOne"; }
        }

        public static Obj_AI_Base BuffedEnemy
        {
            get { return ObjectManager.Get<Obj_AI_Base>().FirstOrDefault(unit => unit.IsEnemy && unit.HasQBuff()); }
        }

        public static void Init()
        {
            GameObject.OnDelete += Obj_AI_Hero_OnCreate;
            GameObject.OnCreate += GameObject_OnCreate;
            Orbwalking.AfterAttack += OrbwalkingAfterAttack;
        }

        private static void GameObject_OnCreate(GameObject sender, EventArgs args) {}

        private static void OrbwalkingAfterAttack(AttackableUnit unit, AttackableUnit target)
        {
            if (unit.IsMe)
            {
                if (PassiveStacks == 0)
                {
                    return;
                }
                PassiveStacks = PassiveStacks - 1;
            }
        }

        public static string SmiteSpellName()
        {
            if (SmiteBlue.Any(a => Items.HasItem(a)))
            {
                return "s5_summonersmiteplayerganker";
            }
            if (SmiteRed.Any(a => Items.HasItem(a)))
            {
                return "s5_summonersmiteduel";
            }
            if (SmiteGrey.Any(a => Items.HasItem(a)))
            {
                return "s5_summonersmitequick";
            }
            if (SmitePurple.Any(a => Items.HasItem(a)))
            {
                return "itemsmiteaoe";
            }
            return "summonersmite";
        }

        public static void Obj_AI_Hero_OnProcessSpellCast(Obj_AI_Base sender, GameObjectProcessSpellCastEventArgs args)
        {
            if (!sender.IsMe)
            {
                return;
            }
            if (args.SData.Name.ToLower().Contains("ward") || args.SData.Name.ToLower().Contains("totem"))
            {
                LastWard = Environment.TickCount;
            }
            switch (args.SData.Name)
            {
                case "BlindMonkQOne":
                    LastQ = Environment.TickCount;
                    LastSpell = Environment.TickCount;
                    PassiveStacks = 2;
                    break;
                case "BlindMonkWOne":
                    LastW = Environment.TickCount;
                    LastSpell = Environment.TickCount;
                    PassiveStacks = 2;
                    break;
                case "BlindMonkEOne":
                    LastE = Environment.TickCount;
                    LastSpell = Environment.TickCount;
                    PassiveStacks = 2;
                    break;
                case "blindmonkqtwo":
                    LastQ2 = Environment.TickCount;
                    LastSpell = Environment.TickCount;
                    PassiveStacks = 2;
                    CheckQ = false;
                    break;
                case "blindmonkwtwo":
                    _lastW2 = Environment.TickCount;
                    LastSpell = Environment.TickCount;
                    PassiveStacks = 2;
                    break;
                case "blindmonketwo":
                    LastQ = Environment.TickCount;
                    LastSpell = Environment.TickCount;
                    PassiveStacks = 2;
                    break;
                case "BlindMonkRKick":
                    _lastR = Environment.TickCount;
                    LastSpell = Environment.TickCount;
                    PassiveStacks = 2;
                    if (InsecHandler.FlashR)
                    {
                        Player.Spellbook.CastSpell(Player.GetSpellSlot("summonerflash"), InsecHandler.FlashPos);
                        InsecHandler.FlashPos = new Vector3();
                        InsecHandler.FlashR = false;
                    }
                    break;
            }
        }

        private static void Obj_AI_Hero_OnCreate(GameObject sender, EventArgs args)
        {
            if (sender.Position.Distance(Player.Position) > 200)
            {
                return;
            }
            if (sender.Name == "blindMonk_Q_resonatingStrike_tar_blood.troy")
            {
                CheckQ = true;
            }
        }

        public static double Q2Damage(Obj_AI_Base target, float subHp = 0, bool monster = false)
        {
            var damage = (50 + (_spells[SpellSlot.Q].Level * 30)) + (0.09 * Player.FlatPhysicalDamageMod) +
                         ((target.MaxHealth - (target.Health - subHp)) * 0.08);
            if (monster && damage > 400)
            {
                return Player.CalcDamage(target, Damage.DamageType.Physical, 400);
            }
            return Player.CalcDamage(target, Damage.DamageType.Physical, damage);
        }

        public static bool HasQBuff(this Obj_AI_Base unit)
        {
            return (unit.HasBuff("BlindMonkQOne", true) || unit.HasBuff("blindmonkqonechaos", true));
        }

        public static bool HasEBuff(this Obj_AI_Base unit)
        {
            return (unit.HasBuff("BlindMonkEOne", true) || unit.HasBuff("BlindMonkEOne"));
        }

        public static void UseItems(Obj_AI_Base target, bool minions = false)
        {
            if (ItemData.Ravenous_Hydra_Melee_Only.GetItem().IsReady() &&
                ItemData.Ravenous_Hydra_Melee_Only.Range > Player.Distance(target))
            {
                ItemData.Ravenous_Hydra_Melee_Only.GetItem().Cast();
            }
            if (ItemData.Tiamat_Melee_Only.GetItem().IsReady() &&
                ItemData.Tiamat_Melee_Only.Range > Player.Distance(target))
            {
                ItemData.Tiamat_Melee_Only.GetItem().Cast();
            }
            if (minions)
            {
                return;
            }
            if (ItemData.Blade_of_the_Ruined_King.GetItem().IsReady() &&
                ItemData.Blade_of_the_Ruined_King.Range > Player.Distance(target))
            {
                ItemData.Blade_of_the_Ruined_King.GetItem().Cast(target);
            }
            if (ItemData.Youmuus_Ghostblade.GetItem().IsReady() &&
                Orbwalking.GetRealAutoAttackRange(Player) > Player.Distance(target))
            {
                ItemData.Youmuus_Ghostblade.GetItem().Cast();
            }
        }
    }
}