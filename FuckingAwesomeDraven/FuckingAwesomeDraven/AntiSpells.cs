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

namespace FuckingAwesomeDraven
{
    internal class Antispells
    {
        public static void Init()
        {
            var mainMenu = Program.Config.AddSubMenu(new Menu("Anti GapCloser", "Anti GapCloser"));
            var spellMenu = mainMenu.AddSubMenu(new Menu("Enabled Spells", "Enabled SpellsAnti GapCloser"));
            mainMenu.AddItem(new MenuItem("EnabledGC", "Enabled").SetValue(false));

            var mainMenuinterrupter = Program.Config.AddSubMenu(new Menu("Interrupter", "Interrupter"));
            mainMenuinterrupter.AddItem(new MenuItem("EnabledInterrupter", "Enabled").SetValue(false));
            mainMenuinterrupter.AddItem(
                new MenuItem("minChannel", "Minimum Channel Priority").SetValue(
                    new StringList(new[] { "HIGH", "MEDIUM", "LOW" })));


            foreach (var champ in ObjectManager.Get<Obj_AI_Hero>().Where(a => a.IsEnemy))
            {
                var champmenu = spellMenu.AddSubMenu(new Menu(champ.ChampionName, champ.ChampionName + "GC"));
                foreach (var gcSpell in AntiGapcloser.Spells)
                {
                    if (gcSpell.ChampionName == champ.ChampionName)
                    {
                        champmenu.AddItem(
                            new MenuItem(gcSpell.SpellName, gcSpell.SpellName + "- " + gcSpell.Slot).SetValue(true));
                    }
                }
            }

            Interrupter2.OnInterruptableTarget += Interrupter2_OnInterruptableTarget;
            AntiGapcloser.OnEnemyGapcloser += AntiGapcloser_OnEnemyGapcloser;
        }

        private static void AntiGapcloser_OnEnemyGapcloser(ActiveGapcloser gapcloser)
        {
            if (Program.Config.Item(gapcloser.Sender.LastCastedSpellName().ToLower()) == null)
            {
                return;
            }
            if (!Program.Config.Item(gapcloser.Sender.LastCastedSpellName().ToLower()).GetValue<bool>() ||
                !gapcloser.Sender.IsValidTarget())
            {
                return;
            }
            if (Program._spells[Spells.E].IsReady() && gapcloser.Sender.IsValidTarget(Program._spells[Spells.E].Range))
            {
                Program._spells[Spells.E].Cast(gapcloser.Sender);
            }
        }

        private static void Interrupter2_OnInterruptableTarget(Obj_AI_Hero sender,
            Interrupter2.InterruptableTargetEventArgs args)
        {
            if (!Program.Config.Item("EnabledInterrupter").GetValue<bool>() || !sender.IsValidTarget())
            {
                return;
            }
            Interrupter2.DangerLevel a;
            switch (Program.Config.Item("minChannel").GetValue<StringList>().SelectedValue)
            {
                case "HIGH":
                    a = Interrupter2.DangerLevel.High;
                    break;
                case "MEDIUM":
                    a = Interrupter2.DangerLevel.Medium;
                    break;
                default:
                    a = Interrupter2.DangerLevel.Low;
                    break;
            }

            if (args.DangerLevel == Interrupter2.DangerLevel.High ||
                args.DangerLevel == Interrupter2.DangerLevel.Medium && a != Interrupter2.DangerLevel.High ||
                args.DangerLevel == Interrupter2.DangerLevel.Medium && a != Interrupter2.DangerLevel.Medium &&
                a != Interrupter2.DangerLevel.High)
            {
                if (Program._spells[Spells.E].IsReady() && sender.IsValidTarget(Program._spells[Spells.E].Range))
                {
                    Program._spells[Spells.E].Cast(sender);
                }
            }
        }
    }
}