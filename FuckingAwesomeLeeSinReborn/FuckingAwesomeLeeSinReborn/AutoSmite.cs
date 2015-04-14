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

using System.Linq;
using LeagueSharp;
using LeagueSharp.Common;

namespace FuckingAwesomeLeeSinReborn
{
    internal static class AutoSmite
    {
        private static bool _checkForSmite;

        private static Obj_AI_Hero Player
        {
            get { return ObjectManager.Player; }
        }

        private static double GetSmiteDamage()
        {
            int[] dmg =
            {
                20 * Player.Level + 370, 30 * Player.Level + 330, 40 * +Player.Level + 240,
                50 * Player.Level + 100
            };
            return Player.GetSpellSlot(CheckHandler.SmiteSpellName()).IsReady() ? dmg.Max() : 0;
        }

        public static void Init()
        {
            Game.OnUpdate += args => Tick();
            Drawing.OnDraw += args => Draw();
        }

        private static void Tick()
        {
            if (!Program.Config.Item("smiteEnabled").GetValue<KeyBind>().Active)
            {
                return;
            }

            Obj_AI_Base selectedMinion =
                MinionManager.GetMinions(1100, MinionTypes.All, MinionTeam.Neutral, MinionOrderTypes.MaxHealth)
                    .FirstOrDefault(
                        minion =>
                            minion.Health > 0 && Program.Config.Item(minion.BaseSkinName) != null &&
                            Program.Config.Item(minion.BaseSkinName).GetValue<bool>());
            if (selectedMinion == null)
            {
                return;
            }
            if (GetSmiteDamage() >= selectedMinion.Health && Player.Distance(selectedMinion) <= 700 ||
                _checkForSmite && Player.Distance(selectedMinion) < 100)
            {
                Player.Spellbook.CastSpell(Player.GetSpellSlot(CheckHandler.SmiteSpellName()), selectedMinion);
                _checkForSmite = false;
            }
            if (!CheckHandler._spells[SpellSlot.Q].IsReady())
            {
                return;
            }

            if (selectedMinion.HasQBuff() &&
                CheckHandler.Q2Damage(selectedMinion, (float) GetSmiteDamage(), true) + GetSmiteDamage() > selectedMinion.Health &&
                !CheckHandler.QState)
            {
                CheckHandler._spells[SpellSlot.Q].Cast();
                _checkForSmite = true;
            }
            if (
                CheckHandler.Q2Damage(
                    selectedMinion, (float) GetSmiteDamage() + CheckHandler._spells[SpellSlot.Q].GetDamage(selectedMinion),
                    true) + GetSmiteDamage() + CheckHandler._spells[SpellSlot.Q].GetDamage(selectedMinion) >
                selectedMinion.Health && CheckHandler.QState)
            {
                CheckHandler._spells[SpellSlot.Q].Cast(selectedMinion);
            }
        }

        private static void Draw()
        {
            if (!Program.Config.Item("smiteEnabled").GetValue<KeyBind>().Active ||
                !Program.Config.Item("DS").GetValue<Circle>().Active)
            {
                return;
            }
            var lowFps = Program.Config.Item("LowFPS").GetValue<bool>();
            var lowFpsMode = Program.Config.Item("LowFPSMode").GetValue<StringList>().SelectedIndex + 1;
            Render.Circle.DrawCircle(
                Player.Position, 700, Program.Config.Item("DS").GetValue<Circle>().Color, lowFps ? lowFpsMode : 5);
        }
    }
}