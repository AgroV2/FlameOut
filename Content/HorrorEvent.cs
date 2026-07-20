using Terraria.ModLoader;
using Terraria;
using System.Collections.Generic;
using System;
using Terraria.Audio;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria.GameContent;
using Terraria.ID;
using ReLogic.Content;

namespace flameout.Content
{
    public enum HorrorEventCategory
    {
        Small,   
        Medium,  
        Global   
    }

    public abstract class HorrorEvent
    {
        
        public abstract string Name { get; }
        public abstract HorrorEventCategory Category { get; }

        
        public abstract void Trigger();
    }

    public class TriggerCommand : ModCommand
    {
        public override string Command => "trigger";
        public override CommandType Type => CommandType.Chat;
        public override string Description => "Дебаг: вызвать хоррор-событие по имени";
        public override string Usage => "/trigger <имя_события>";

        public override void Action(CommandCaller caller, string input, string[] args)
        {
            if (args.Length == 0)
            {
                caller.Reply("Укажи имя события. Пример: /trigger example_small");
                return;
            }

            string eventName = args[0];
            bool found = HorrorEventManager.Instance.TriggerByName(eventName);

            if (found)
                caller.Reply($"Событие '{eventName}' запущено.");
            else
                caller.Reply($"Событие '{eventName}' не найдено. Проверь имя.");
        }
    }

    
    public class HorrorPlayerSystem : ModPlayer
    {
        public override void OnEnterWorld()
        {
            RegisterAllEvents();
            HorrorEventManager.Instance.Activate();
        }

        public override void PlayerDisconnect()
        {
            HorrorEventManager.Instance.Deactivate();
        }

        private void RegisterAllEvents()
        {
            
            HorrorEventManager.ClearEvents();

            HorrorEventManager.RegisterEvent(new SmallEvent1());
            HorrorEventManager.RegisterEvent(new SmallEvent2());
            HorrorEventManager.RegisterEvent(new SmallEvent3());
            HorrorEventManager.RegisterEvent(new SmallEvent4());
            HorrorEventManager.RegisterEvent(new SmallEvent5());
            HorrorEventManager.RegisterEvent(new MediumEvent1());
            HorrorEventManager.RegisterEvent(new MediumEvent2());
            HorrorEventManager.RegisterEvent(new MediumEvent3());
            HorrorEventManager.RegisterEvent(new MediumEvent4());
            HorrorEventManager.RegisterEvent(new MediumEvent5());
            HorrorEventManager.RegisterEvent(new GlobalEvent1());
            HorrorEventManager.RegisterEvent(new GlobalEvent2());
            HorrorEventManager.RegisterEvent(new GlobalEvent3());
            HorrorEventManager.RegisterEvent(new GlobalEvent4());
            HorrorEventManager.RegisterEvent(new GlobalEvent5());
        }
    }

    public class HorrorEventManager : ModSystem
    {
        
        private static readonly List<HorrorEvent> _events = new();

        public static void RegisterEvent(HorrorEvent e) => _events.Add(e);

        public static void ClearEvents() => _events.Clear();

        public static HorrorEvent FindEvent(string name)
            => _events.Find(e => e.Name.Equals(name, StringComparison.OrdinalIgnoreCase));

        
        
        
        
        private int _smallTimer;
        private int _mediumTimer;
        private int _globalTimer;

        private static readonly Random _rng = new();

        
        private bool _active;

        public static HorrorEventManager Instance => ModContent.GetInstance<HorrorEventManager>();

        public void Activate()
        {
            _active = true;
            _smallTimer  = RollTimer(18000, 27000);
            _mediumTimer = RollTimer(72000, 90000);
            _globalTimer = RollTimer(216000, 324000);
        }

        public void Deactivate()
        {
            _active = false;
        }

        
        public override void PostUpdateEverything()
        {
            if (!_active || Main.dedServ) return;

            _smallTimer--;
            _mediumTimer--;
            _globalTimer--;

            if (_smallTimer <= 0)
            {
                if (!BossFightActive())
                    FireRandom(HorrorEventCategory.Small);
                _smallTimer = RollTimer(18000, 27000);
            }

            if (_mediumTimer <= 0)
            {
                if (!BossFightActive())
                    FireRandom(HorrorEventCategory.Medium);
                _mediumTimer = RollTimer(72000, 90000);
            }

            if (_globalTimer <= 0)
            {
                if (!BossFightActive())
                    FireRandom(HorrorEventCategory.Global);
                _globalTimer = RollTimer(216000, 324000);
            }
        }

        
        private static bool BossFightActive()
        {
            for (int i = 0; i < Main.maxNPCs; i++)
            {
                NPC npc = Main.npc[i];
                if (!npc.active)
                    continue;

                
                
                if (npc.boss
                    || npc.type == NPCID.EaterofWorldsHead
                    || npc.type == NPCID.EaterofWorldsBody
                    || npc.type == NPCID.EaterofWorldsTail)
                    return true;
            }
            return false;
        }

        
        private void FireRandom(HorrorEventCategory category)
        {
            var pool = _events.FindAll(e => e.Category == category);
            if (pool.Count == 0)
                return;

            var chosen = pool[_rng.Next(pool.Count)];
            chosen.Trigger();
        }

        
        public bool TriggerByName(string name)
        {
            var e = FindEvent(name);
            if (e == null) return false;
            e.Trigger();
            return true;
        }

        
        private static int RollTimer(int min, int max) => _rng.Next(min, max + 1);
    }

    public class HorrorUIState : ModSystem
    {
        public static int timer = 0;
        private Asset<Texture2D> eyeAsset;

        
        public void ShowEyeScreamer()
        {
            timer = 180;
        }

        public override void PostDrawInterface(SpriteBatch spriteBatch)
        {
            if (timer <= 0) return;
            timer--;

            if (eyeAsset == null)
            {
                eyeAsset = ModContent.Request<Texture2D>("flameout/Content/eye1", AssetRequestMode.ImmediateLoad);
            }

            
            spriteBatch.Draw(
                TextureAssets.MagicPixel.Value,
                new Rectangle(0, 0, Main.screenWidth, Main.screenHeight),
                Color.Black
            );

            
            if (eyeAsset != null && eyeAsset.IsLoaded)
            {
                Texture2D texture = eyeAsset.Value;
                Vector2 screenSize = new Vector2(Main.screenWidth, Main.screenHeight);
                Vector2 eyeSize = new Vector2(500, 250);
                Vector2 position = (screenSize - eyeSize) / 2f;

                spriteBatch.Draw(
                    texture,
                    new Rectangle((int)position.X, (int)position.Y, (int)eyeSize.X, (int)eyeSize.Y),
                    Color.White
                );
            }
        }
    }
}