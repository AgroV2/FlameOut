

using Terraria;
using Terraria.ModLoader;

namespace flameout.Content
{
    public class BossHpScalingSystem : ModSystem
    {
        private const int EmpathyBaseLife = 1_000_000;
        private const int GuardianBaseLife = 50_000;

        private const int EmpathyBonusPerMod = 2_000_000;
        private const int GuardianBonusPerMod = 100_000;

        private static readonly int[] ApathyLifeByModCount = { 300_000, 1_000_000, 3_000_000 };

        private const string CalamityName = "CalamityMod";
        private const string FargoSoulsName = "FargowiltasSouls";


        public static bool CalamityLoaded { get; private set; }
        public static bool FargoSoulsLoaded { get; private set; }

        public static int EmpathyLifeMax { get; private set; } = EmpathyBaseLife;
        public static int GuardianLifeMax { get; private set; } = GuardianBaseLife;
        public static int ApathyLifeMax { get; private set; } = 300_000;

        public override void PostSetupContent()
        {
            CalamityLoaded = ModLoader.HasMod(CalamityName);
            FargoSoulsLoaded = ModLoader.HasMod(FargoSoulsName);

            int modCount = (CalamityLoaded ? 1 : 0) + (FargoSoulsLoaded ? 1 : 0);

            EmpathyLifeMax = EmpathyBaseLife + modCount * EmpathyBonusPerMod;
            GuardianLifeMax = GuardianBaseLife + modCount * GuardianBonusPerMod;
            ApathyLifeMax = ApathyLifeByModCount[modCount];

            Mod.Logger.Info($"[flameout] Calamity: {CalamityLoaded}, FargoSouls: {FargoSoulsLoaded} -> " +
                            $"Apathy HP = {ApathyLifeMax:N0}, Empathy HP = {EmpathyLifeMax:N0}, Guardians HP = {GuardianLifeMax:N0}");
        }

        public override void Unload()
        {
            CalamityLoaded = false;
            FargoSoulsLoaded = false;
            EmpathyLifeMax = EmpathyBaseLife;
            GuardianLifeMax = GuardianBaseLife;
            ApathyLifeMax = 300_000;
        }
    }

    public class BossHpScalingNPC : GlobalNPC
    {
        public override bool AppliesToEntity(NPC npc, bool lateInstantiation)
        {
            return lateInstantiation &&
                   (npc.type == ModContent.NPCType<Empathy>()
                 || npc.type == ModContent.NPCType<ApathyBoss>()
                 || npc.type == ModContent.NPCType<Guardian1>()
                 || npc.type == ModContent.NPCType<Guardian2>());
        }

        public override void SetDefaults(NPC npc)
        {
            if (npc.type == ModContent.NPCType<Empathy>())
            {
                npc.lifeMax = BossHpScalingSystem.EmpathyLifeMax;
                npc.life = npc.lifeMax;
            }
            else if (npc.type == ModContent.NPCType<ApathyBoss>())
            {
                npc.lifeMax = BossHpScalingSystem.ApathyLifeMax;
                npc.life = npc.lifeMax;
            }
            else if (npc.type == ModContent.NPCType<Guardian1>() || npc.type == ModContent.NPCType<Guardian2>())
            {
                npc.lifeMax = BossHpScalingSystem.GuardianLifeMax;
                npc.life = npc.lifeMax;
            }
        }
    }
}

