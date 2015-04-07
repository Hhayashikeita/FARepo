using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LeagueSharp;
using LeagueSharp.Common;
using SharpDX;
using Color = System.Drawing.Color;

namespace FuckingAwesomeBard
{
    static class Program
    {

        public static Dictionary<SpellSlot, Spell> Spells = new Dictionary<SpellSlot, Spell>()
        {
                {SpellSlot.Q, new Spell(SpellSlot.Q, 0)},
                {SpellSlot.W, new Spell(SpellSlot.W, 0)},
                {SpellSlot.E, new Spell(SpellSlot.E, 0)},
                {SpellSlot.R, new Spell(SpellSlot.R, 0)}
        };
        private static Obj_AI_Hero Player { get { return ObjectManager.Player; } }
        static void Main(string[] args)
        {
            CustomEvents.Game.OnGameLoad += Game_OnGameLoad;
        }

        static void Game_OnGameLoad(EventArgs args)
        {
            Drawing.OnDraw += Drawing_OnDraw;
        }

        public static void Combo()
        {
            var target = TargetSelector.GetTarget(1200, TargetSelector.DamageType.Magical);
            var QStunOnly = true;
            var Q = true;
            var W = true;
            var E = true;
            var R = true;
            if (Q)
            {
                if (QStunOnly)
                {
                    CastIfStun(target);
                }
                else
                {
                    CastIfStun(target);
                    Spells[SpellSlot.Q].Cast(target);
                }
            }
        }

        public static void CastRAlly()
        {
            foreach (var ally in ObjectManager.Get<Obj_AI_Hero>().Where(a => a.IsAlly && a.HealthPercent < 10 && a.GetEnemiesInRange(1000).Count >= 1))
            {
                Spells[SpellSlot.R].Cast(ally);
            }
        }

        static readonly string[] Epics =
            {
                "SRU_Baron", "SRU_Dragon"
            };

        public static void SaveDrake()
        {
            foreach (var minion in MinionManager.GetMinions(Player.Position, 1100, MinionTypes.All, MinionTeam.Neutral).Where(min => Epics.Contains(min.BaseSkinName)))
            {
                if (minion.Health < 1000 * (1 + (1 * (Player.Level / 10))))
                {
                    Spells[SpellSlot.R].Cast(minion);
                } 
            }
        }

        static void Drawing_OnDraw(EventArgs args)
        {
            var pos = GetFurthestWallPosition((ObjectManager.Player.Position.To2D() + 300 * ObjectManager.Player.Direction.To2D().Perpendicular()).To3D()); // gets wall pos from current position
            if (pos.IsValid())
            {
                Drawing.DrawCircle(GetFurthestWallPosition(pos), 100, Color.White);
            }
        }

        public static void CastIfStun(Obj_AI_Base target)
        {
            int pushBack = 100;
            int CheckNo = 10;
            var pred = Spells[SpellSlot.Q].GetPrediction(target);
            if (pred.CollisionObjects.Count(s => s.IsValidTarget()) == 1)
            {
                Spells[SpellSlot.Q].Cast(pred.CastPosition);
            }
            if (pred.CollisionObjects.Count(s => s.IsValidTarget()) > 0)
                return;
            var EnemyUnits = ObjectManager.Get<Obj_AI_Base>().Where(a => a.IsValidTarget());
            for (int i = 1; i <= CheckNo; i++)
            {
                var pos = pred.UnitPosition.Extend(
                    Player.Position, Player.Position.Distance(pred.UnitPosition) + i * (pushBack / CheckNo));
                if (pos.IsWall() || EnemyUnits.Count(a => pos.Distance(a.Position) < 50) > 0)
                {
                    Spells[SpellSlot.Q].Cast(pred.CastPosition);
                }
            }
        }


        public static List<Vector2> GetPointsInACircle(int points, double radius, Vector2 center)
        {
            List<Vector2> list = new List<Vector2>();
            double slice = 2 * Math.PI / points;
            for (int i = 0; i < points; i++)
            {
                double angle = slice * i;
                int newX = (int)(center.X + radius * Math.Cos(angle));
                int newY = (int)(center.Y + radius * Math.Sin(angle));
                list.Add(new Vector2(newX, newY));
            }
            return list;
        }

        public static Vector3 GetFurthestWallPosition(Vector3 direction)
        {
            Vector3 firstWallPos = new Vector3();
            for (int i = 1; i < 300; i++)
            {
                if (Player.Position.Extend(direction, Player.Distance(direction) + i * 10).IsWall())
                {
                    firstWallPos = Player.Position.Extend(direction, Player.Distance(direction) + i * 10);
                    break;
                }
            }
            if (!firstWallPos.IsValid())
                return new Vector3();
            for (int i = 1;; i++)
            {
                var endPos = Player.Position.Extend(firstWallPos, i * 10);
                if (!endPos.IsWall()) return Player.Position.Extend(firstWallPos, (i-1) * 10);
            }
        }
    }
}
