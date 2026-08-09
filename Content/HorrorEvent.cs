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

    internal sealed class CallbackHorrorEvent : HorrorEvent
    {
        private readonly Action _callback;

        public string Id { get; }
        public string SourceMod { get; }
        public override string Name { get; }
        public override HorrorEventCategory Category { get; }

        public CallbackHorrorEvent(string id, string sourceMod, string name, HorrorEventCategory category, Action callback)
        {
            Id = id;
            SourceMod = sourceMod;
            Name = name;
            Category = category;
            _callback = callback;
        }

        public override void Trigger() => _callback();
    }

    public class TriggerCommand : ModCommand
    {
        public override string Command => "trigger";
        public override CommandType Type => CommandType.Chat;
        public override string Description => "Debug";
        public override string Usage => "/trigger <name>";

        public override void Action(CommandCaller caller, string input, string[] args)
        {
            if (!FlameoutClientConfig.Instance.DebugMode)
            {
                caller.Reply("Debug mode must be enabled to trigger events.");
                return;
            }

            if (args.Length == 0)
            {
                caller.Reply("Empty name");
                return;
            }

            string eventName = args[0];
            bool found = HorrorEventManager.Instance.TriggerByName(eventName);

            if (found)
                caller.Reply($"Event '{eventName}' started.");
            else
                caller.Reply($"No such event");
        }
    }

    
    public class HorrorPlayerSystem : ModPlayer
    {
        public override void OnEnterWorld()
        {
            HorrorEventManager.RegisterBuiltInEvents();
            HorrorEventManager.Instance.Activate();
        }

        public override void PlayerDisconnect()
        {
            HorrorEventManager.Instance.Deactivate();
        }

    }

    public class HorrorEventManager : ModSystem
    {
        
        private static readonly List<HorrorEvent> _events = new();
        private static bool _builtInsRegistered;

        public static void RegisterEvent(HorrorEvent e)
        {
            if (e == null)
                throw new ArgumentNullException(nameof(e));
            if (FindEvent(e.Name) != null)
                throw new InvalidOperationException($"A horror event named '{e.Name}' is already registered.");
            _events.Add(e);
        }

        public static void RegisterBuiltInEvents()
        {
            if (_builtInsRegistered)
                return;

            HorrorEvent[] builtIns = {
                new SmallEvent1(), new SmallEvent2(), new SmallEvent3(), new SmallEvent4(), new SmallEvent5(),
                new SmallEvent6(), new SmallEvent7(), new SmallEvent8(), new SmallEvent9(), new SmallEvent10(),
                new SmallEvent11(), new SmallEvent12(), new SmallEvent13(), new SmallEvent14(), new SmallEvent15(),
                new MediumEvent1(), new MediumEvent2(), new MediumEvent3(), new MediumEvent4(), new MediumEvent5(),
                new MediumEvent6(), new MediumEvent7(), new MediumEvent8(), new MediumEvent9(), new MediumEvent10(),
                new GlobalEvent1(), new GlobalEvent2(), new GlobalEvent3(), new GlobalEvent4(), new GlobalEvent5(),
                new GlobalEvent6(), new GlobalEvent7(), new GlobalEvent8()
            };

            foreach (HorrorEvent horrorEvent in builtIns)
                RegisterEvent(horrorEvent);
            _builtInsRegistered = true;
        }

        public static void ClearEvents()
        {
            _events.Clear();
            _builtInsRegistered = false;
        }

        public override void Unload() => ClearEvents();

        public static HorrorEvent FindEvent(string name)
        {
            if (string.IsNullOrWhiteSpace(name))
                return null;
            return _events.Find(e => GetId(e).Equals(name.Trim(), StringComparison.OrdinalIgnoreCase)
                || e.Name.Equals(name.Trim(), StringComparison.OrdinalIgnoreCase));
        }

        public static string RegisterExternalEvent(string sourceMod, string localName, HorrorEventCategory category, Action callback)
        {
            if (callback == null)
                throw new ArgumentNullException(nameof(callback));
            if (!Enum.IsDefined(typeof(HorrorEventCategory), category))
                throw new ArgumentOutOfRangeException(nameof(category));

            string id = CreateEventId(sourceMod, localName);
            if (FindEvent(id) != null)
                throw new InvalidOperationException($"Horror event ID '{id}' is already registered.");

            _events.Add(new CallbackHorrorEvent(id, sourceMod, localName.Trim(), category, callback));
            return id;
        }

        public static bool UnregisterExternalEvent(string id)
        {
            CallbackHorrorEvent horrorEvent = FindEvent(id) as CallbackHorrorEvent;
            return horrorEvent != null && _events.Remove(horrorEvent);
        }

        public static bool IsRegistered(string id) => FindEvent(id) != null;

        public static string CreateEventId(string sourceMod, string localName)
        {
            string source = NormalizeName(sourceMod, nameof(sourceMod));
            string local = NormalizeName(localName, nameof(localName));
            string prefix = source.Length == 1 ? source : $"{source[0]}{source[source.Length - 1]}";
            return $"{prefix}_{local}";
        }

        private static string NormalizeName(string value, string parameterName)
        {
            if (string.IsNullOrWhiteSpace(value))
                throw new ArgumentException("Name cannot be empty.", parameterName);

            var characters = new List<char>();
            foreach (char character in value.Trim())
            {
                if (character >= 'A' && character <= 'Z')
                    characters.Add(char.ToLowerInvariant(character));
                else if ((character >= 'a' && character <= 'z') || (character >= '0' && character <= '9'))
                    characters.Add(character);
                else if (character == '_' || character == '-')
                    characters.Add(character);
                else
                    throw new ArgumentException("Names may contain only ASCII letters, digits, underscores, and hyphens.", parameterName);
            }

            if (characters.Count == 0 || !IsAsciiLetterOrDigit(characters[0]) || !IsAsciiLetterOrDigit(characters[characters.Count - 1]))
                throw new ArgumentException("Name must start and end with an ASCII letter or digit.", parameterName);
            return new string(characters.ToArray());
        }

        private static bool IsAsciiLetterOrDigit(char character)
            => (character >= 'a' && character <= 'z') || (character >= '0' && character <= '9');

        private static string GetId(HorrorEvent horrorEvent)
            => horrorEvent is CallbackHorrorEvent callbackEvent ? callbackEvent.Id : horrorEvent.Name;

        
        
        
        
        private int _smallTimer;
        private int _mediumTimer;
        private int _globalTimer;

        private static readonly Random _rng = new();

        
        private bool _active;
        private static bool _debugModeWasEnabled;

        public const string DebugModeMessage = "You have now entered debug mode. Natural event happeining has been turned off, instead you can spawn events with trigger command. example: \"/trigger small_event1\"";

        public static HorrorEventManager Instance => ModContent.GetInstance<HorrorEventManager>();

        public void Activate()
        {
            _active = true;
            _debugModeWasEnabled = FlameoutClientConfig.Instance.DebugMode;
            if (_debugModeWasEnabled && !Main.dedServ)
                Main.NewText(DebugModeMessage);

            _smallTimer  = RollTimer(18000, 27000);
            _mediumTimer = RollTimer(72000, 90000);
            _globalTimer = RollTimer(216000, 324000);
        }

        public void Deactivate()
        {
            _active = false;
        }

        
        public static void HandleDebugModeChanged(bool enabled)
        {
            if (Main.dedServ || Main.gameMenu)
            {
                _debugModeWasEnabled = enabled;
                return;
            }

            if (enabled && !_debugModeWasEnabled)
                Main.NewText(DebugModeMessage);

            _debugModeWasEnabled = enabled;
        }

        public override void PostUpdateEverything()
        {
            if (!_active || Main.dedServ || FlameoutClientConfig.Instance.DebugMode) return;

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
