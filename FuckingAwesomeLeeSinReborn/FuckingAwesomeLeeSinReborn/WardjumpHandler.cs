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
using CH = FuckingAwesomeLeeSinReborn.CheckHandler;
using Color = System.Drawing.Color;

namespace FuckingAwesomeLeeSinReborn
{
    internal static class WardjumpHandler
    {
        public static bool DrawEnabled;
        private static Vector3 _drawPos;

        private static Obj_AI_Hero Player
        {
            get { return ObjectManager.Player; }
        }

        public static void Draw()
        {
            if (Program.Config.Item("escapeMode").GetValue<bool>() && JumpHandler.IsJumpable() &&
                CheckHandler._spells[SpellSlot.Q].IsReady())
            {
                return;
            }
            if (!DrawEnabled)
            {
                return;
            }
            var lowFps = Program.Config.Item("LowFPS").GetValue<bool>();
            var lowFpsMode = Program.Config.Item("LowFPSMode").GetValue<StringList>().SelectedIndex + 1;
            if (_drawPos.IsValid())
            {
                Render.Circle.DrawCircle(_drawPos, 70, Color.RoyalBlue, lowFps ? lowFpsMode : 5);
                Render.Circle.DrawCircle(Player.Position, 600, Color.White, lowFps ? lowFpsMode : 5);
            }
        }

        private static Obj_AI_Base WardJumpUnit(Vector3 pos, bool onlyPos = false)
        {
            var minions = Program.Config.Item("jumpMinions").GetValue<bool>();
            var champions = Program.Config.Item("jumpChampions").GetValue<bool>();
            var wards = Program.Config.Item("jumpWards").GetValue<bool>();

            var poly =
                new Geometry.Rectangle(
                    Player.Position.To2D().Extend(pos.To2D(), 200), Player.Position.To2D().Extend(pos.To2D(), 700), 60)
                    .ToPolygon();

            if (minions)
            {
                Obj_AI_Minion selectedMinion =
                    ObjectManager.Get<Obj_AI_Minion>()
                        .Where(
                            minion =>
                                minion != null && minion.Distance(Player) <= 700 && minion.IsAlly &&
                                !poly.IsOutside(minion.Position.To2D()) && !minion.Name.ToLower().Contains("ward") &&
                                !minion.IsMe && (!onlyPos || minion.Distance(pos) < 70))
                        .OrderByDescending(a => Player.Distance(a))
                        .FirstOrDefault();
                if (selectedMinion != null)
                {
                    return selectedMinion;
                }
            }
            if (champions)
            {
                Obj_AI_Hero selectedHero =
                    ObjectManager.Get<Obj_AI_Hero>()
                        .Where(
                            minion =>
                                minion != null && minion.Distance(Player) <= 700 && minion.IsAlly &&
                                !poly.IsOutside(minion.Position.To2D()) && !minion.IsMe &&
                                (!onlyPos || minion.Distance(pos) < 70))
                        .OrderByDescending(a => Player.Distance(a))
                        .FirstOrDefault();
                if (selectedHero != null)
                {
                    return selectedHero;
                }
            }

            if (wards)
            {
                Obj_AI_Minion selectedMinion =
                    ObjectManager.Get<Obj_AI_Minion>()
                        .Where(
                            minion =>
                                minion != null && minion.Distance(Player) <= 700 && minion.IsAlly &&
                                !poly.IsOutside(minion.Position.To2D()) && minion.Name.ToLower().Contains("ward") &&
                                !minion.IsMe && (!onlyPos || minion.Distance(pos) < 70))
                        .OrderByDescending(a => Player.Distance(a))
                        .FirstOrDefault();
                // ReSharper disable once UseNullPropagation
                if (selectedMinion != null)
                {
                    return selectedMinion;
                }
            }

            return null;
        }

        public static void Jump(Vector3 pos, bool maxRange = false, bool moveToMouse = false, bool onlyPos = false)
        {
            if (moveToMouse)
            {
                Player.IssueOrder(GameObjectOrder.MoveTo, Player.Position.Extend(Game.CursorPos, 150));
            }

            _drawPos = new Vector3();

            if (JumpHandler.InitQ || JumpHandler.IsJumpable())
            {
                return;
            }

            if (maxRange && pos.Distance(Player.Position) > 600)
            {
                pos = Player.Position.Extend(pos, 600);
            }

            _drawPos = pos;
            var unit = WardJumpUnit(pos, onlyPos);
            if (unit != null && CheckHandler.WState)
            {
                CheckHandler._spells[SpellSlot.W].Cast(unit);
                Console.WriteLine("casting W for WJ");
                return;
            }
            if (pos.Distance(Player.Position) > 600)
            {
                Console.WriteLine("too far for wardjump");
                return;
            }
            if (pos.Distance(Player.Position) < 600 && CheckHandler.LastWard + 600 < Environment.TickCount &&
                Items.GetWardSlot() != null && CH.WState && CH._spells[SpellSlot.W].IsReady())
            {
                Player.Spellbook.CastSpell(Items.GetWardSlot().SpellSlot, pos);
                Console.WriteLine("Warding");
            }
        }
    }
}