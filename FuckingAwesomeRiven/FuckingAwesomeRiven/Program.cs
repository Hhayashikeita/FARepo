using System;
using System.Collections.Generic;
using System.Threading;
using FuckingAwesomeRiven.EvadeUtils;
using LeagueSharp;
using LeagueSharp.Common;
using SH = FuckingAwesomeRiven.SpellHandler;
using SpellSlot = LeagueSharp.SpellSlot;

namespace FuckingAwesomeRiven
{
    internal class Program
    {
        public static Obj_AI_Hero Player;

        private static void Main(string[] args)
        {
            CustomEvents.Game.OnGameLoad += delegate
            {
                // Loading the Assembly Async
                var onGameStart = new Thread(Game_OnGameStart);
                onGameStart.Start();
            };
        }

        private static void Game_OnGameStart()
        {
            if (ObjectManager.Player.ChampionName != "Riven")
            {
                return;
            }
            
            MenuHandler.InitMenu();
            CheckHandler.Init();
            Player = ObjectManager.Player;
            Game.OnUpdate += Game_OnGameUpdate;
            Game.OnUpdate += eventArgs => StateHandler.Tick();
            Obj_AI_Base.OnProcessSpellCast += CheckHandler.Obj_AI_Hero_OnProcessSpellCast;
            Obj_AI_Base.OnProcessSpellCast += AutoE.autoE;
            Drawing.OnDraw += DrawHandler.Draw;
            JumpHandler.Load();
            SmoothMouse.start();
        }

        private static void Game_OnGameUpdate(EventArgs args)
        {
            if (MenuHandler.Orbwalker.ActiveMode != Orbwalking.OrbwalkingMode.None && MenuHandler.Orbwalker.ActiveMode != Orbwalking.OrbwalkingMode.Combo && MenuHandler.Config.Item("streamMouse").GetValue<bool>())
            {
                var random = new Random().Next(500);
                if (SpellHandler.LastMove + 200 + random < Environment.TickCount && CheckHandler.CanMove)
                {
                    SpellHandler.LastMove = Environment.TickCount;
                    SmoothMouse.doMouseClick();
                }
            }
            if (Queuer.Queue.Count > 0)
            {
                Queuer.DoQueue();
            }

            if (MenuHandler.Config.Item("logPos").GetValue<bool>())
            {
                JumpHandler.AddPos();
                MenuHandler.Config.Item("logPos").SetValue(false);
            }

            if (MenuHandler.Config.Item("printPos").GetValue<bool>())
            {
                JumpHandler.PrintToConsole();
                MenuHandler.Config.Item("printPos").SetValue(false);
            }

            if (MenuHandler.Config.Item("clearCurrent").GetValue<bool>())
            {
                JumpHandler.ClearCurrent();
                MenuHandler.Config.Item("clearCurrent").SetValue(false);
            }

            if (MenuHandler.Config.Item("clearPrevious").GetValue<bool>())
            {
                JumpHandler.ClearPrevious();
                MenuHandler.Config.Item("clearPrevious").SetValue(false);
            }

            CheckHandler.Checks();
            var config = MenuHandler.Config;
            

            if (MenuHandler.GetMenuBool("keepQAlive") && SH.Spells[SpellSlot.Q].IsReady() && CheckHandler.QCount >= 1 &&
                Environment.TickCount - CheckHandler.LastQ > 3650 && !Player.IsRecalling())
            {
                SH.CastQ();
            } 
            if (config.Item("jungleCombo").GetValue<KeyBind>().Active)
            {
                StateHandler.JungleFarm();
            }
            if (config.Item("harass").GetValue<KeyBind>().Active)
            {
                StateHandler.Harass();
            }
            else if (config.Item("normalCombo").GetValue<KeyBind>().Active)
            {
                StateHandler.MainCombo();
            }

            else if (config.Item("burstCombo").GetValue<KeyBind>().Active)
            {
                StateHandler.BurstCombo();
            }
            else if (config.Item("waveClear").GetValue<KeyBind>().Active)
            {
                StateHandler.Laneclear();
            }
            else if (config.Item("lastHit").GetValue<KeyBind>().Active)
            {
                StateHandler.LastHit();
            }
            else if (config.Item("flee").GetValue<KeyBind>().Active)
            {
                StateHandler.Flee();
            }
            else
            {
                MenuHandler.Orbwalker.SetAttack(true);
                MenuHandler.Orbwalker.SetMovement(true);
                SmoothMouse.queuePos.Clear();
                Utility.DelayAction.Add(
                    2000, () =>
                    {
                        if (
                            !(config.Item("flee").GetValue<KeyBind>().Active ||
                              config.Item("lastHit").GetValue<KeyBind>().Active ||
                              config.Item("waveClear").GetValue<KeyBind>().Active ||
                              config.Item("burstCombo").GetValue<KeyBind>().Active ||
                              config.Item("normalCombo").GetValue<KeyBind>().Active ||
                              config.Item("jungleCombo").GetValue<KeyBind>().Active))
                        {
                            Queuer.Queue = new List<string>();
                        }
                    });
            }
        }
    }
}