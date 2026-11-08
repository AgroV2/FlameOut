using Terraria.ModLoader;
using System.Collections.Generic;
using System;
using System.IO;
using flameout.Content;
namespace flameout
{
    public class flameout : Mod
    {
        public override void HandlePacket(BinaryReader reader, int whoAmI)
        {
            byte message = reader.ReadByte();
            switch (message)
            {
                case BurnoutSystem.MessageToggleBurnout:
                    if (Terraria.Main.netMode == Terraria.ID.NetmodeID.Server && !BurnoutSystem.AnyBossAlive())
                        BurnoutSystem.SetActive(!BurnoutSystem.Active);
                    break;

                default:
                    throw new ArgumentException($"Unknown flameout packet message '{message}'.");
            }
        }

        public override object Call(params object[] args)
        {
            if (args == null || args.Length == 0 || args[0] is not string message)
                throw new ArgumentException("The first argument must be a HorrorEvent API message name.");

            switch (message)
            {
                case "RegisterHorrorEvent":
                    if (args.Length != 5 || args[1] is not Mod sourceMod || args[2] is not string localName || args[4] is not Action callback)
                        throw new ArgumentException("RegisterHorrorEvent expects: source Mod, local name, category, Action callback.");
                    return HorrorEventManager.RegisterExternalEvent(sourceMod.Name, localName, ParseCategory(args[3]), callback);

                case "UnregisterHorrorEvent":
                    if (args.Length != 2 || args[1] is not string unregisterId)
                        throw new ArgumentException("UnregisterHorrorEvent expects a registered event ID.");
                    return HorrorEventManager.UnregisterExternalEvent(unregisterId);

                case "TriggerHorrorEvent":
                    if (args.Length != 2 || args[1] is not string triggerId)
                        throw new ArgumentException("TriggerHorrorEvent expects a registered event ID or built-in name.");
                    return HorrorEventManager.Instance.TriggerByName(triggerId);

                case "HasHorrorEvent":
                    if (args.Length != 2 || args[1] is not string queryId)
                        throw new ArgumentException("HasHorrorEvent expects an event ID or built-in name.");
                    return HorrorEventManager.IsRegistered(queryId);

                default:
                    throw new ArgumentException($"Unknown flameout Mod.Call message '{message}'.");
            }
        }

        private static HorrorEventCategory ParseCategory(object value)
        {
            if (value is HorrorEventCategory category)
                return category;
            if (value is string text && Enum.TryParse(text, true, out HorrorEventCategory parsed))
                return parsed;
            if (value is int number && Enum.IsDefined(typeof(HorrorEventCategory), number))
                return (HorrorEventCategory)number;
            throw new ArgumentException("Category must be Small, Medium, or Global (string, integer, or HorrorEventCategory).");
        }
    }

    public class ApathyBossChecklistSystem : ModSystem
    {
        public override void PostSetupContent()
        {
            
            if (ModLoader.TryGetMod("BossChecklist", out Mod bossChecklist))
            {
                

                bossChecklist.Call(
                    "LogBoss",
                    Mod,
                    "ApathyBoss", 
                    30.0f,
                    new Func<bool>(() => ApathyWorld.downedApathy), 
                    ModContent.NPCType<ApathyBoss>(),
                    new Dictionary<string, object>() {
    ["spawnItems"] = ModContent.ItemType<KeyOfNowhere>(),
    ["displayName"] = "Apathy", 
   
    ["spawnInfo"] = new Func<Terraria.Localization.LocalizedText>(() => Terraria.Localization.Language.GetOrRegister("Mods.flameout.BossChecklist.ApathySpawn", () => "Think twice before you do this. Some things are better to be hidden."))
}

                );
                 bossChecklist.Call(
                "LogBoss",
                Mod,
                "Empathy", 
                999.9f, 
                new Func<bool>(() => ApathyWorld.downedEmpathy),
                ModContent.NPCType<Empathy>(),
                new Dictionary<string, object>() {
                    ["displayName"] = "Empathy", 
         ["spawnInfo"] = new Func<Terraria.Localization.LocalizedText>(() => Terraria.Localization.Language.GetOrRegister("Mods.flameout.BossChecklist.EmpathySpawn", () => "here it is."))

                }
            );
                            bossChecklist.Call(
                    "LogBoss",
                    Mod,
                    "ApatheticSpirit",
                    9.6f,
                    new Func<bool>(() => ApathyWorld.downedApatheticSpirit),
                    ModContent.NPCType<ApatheticSpirit>(),
                    new Dictionary<string, object>() {
                        ["spawnItems"] = ModContent.ItemType<ApatheticKey>(),
                        ["displayName"] = "Apathetic Spirit",
                        ["spawnInfo"] = new Func<Terraria.Localization.LocalizedText>(() =>
                            Terraria.Localization.Language.GetOrRegister(
                                "Mods.flameout.BossChecklist.ApatheticSpiritSpawn",
                                () => "Use an apathetic key to summon."))
                    }
                );
              
            }
        
        }
    }
}
