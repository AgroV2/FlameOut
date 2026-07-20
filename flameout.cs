using Terraria.ModLoader;
using System.Collections.Generic;
using System;
using flameout.Content;
namespace flameout
{
    public class flameout : Mod
    {
        // Main mod class
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
    // Используем лямбда-выражение Func<LocalizedText>, чтобы BossChecklist правильно прочитал строку
    ["spawnInfo"] = new Func<Terraria.Localization.LocalizedText>(() => Terraria.Localization.Language.GetOrRegister("Mods.flameout.BossChecklist.ApathySpawn", () => "Think twice before you do this. Some things are better to be hidden."))
}

                );
                 bossChecklist.Call(
                "LogBoss",
                Mod,
                "Empathy", 
                999.9f, // Прогрессия чуть выше, чтобы он шел следом
                new Func<bool>(() => ApathyWorld.downedEmpathy), // Укажите вашу bool переменную для этого босса
                ModContent.NPCType<Empathy>(), // Укажите класс самого босса Empathy
                new Dictionary<string, object>() {
                    ["displayName"] = "Empathy", 
         ["spawnInfo"] = new Func<Terraria.Localization.LocalizedText>(() => Terraria.Localization.Language.GetOrRegister("Mods.flameout.BossChecklist.EmpathySpawn", () => "here it is."))

                }
            );
              
            }
        
        }
    }
}
