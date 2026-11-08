using System.Collections.Generic;
using System.IO;
using flameout.Content.Candle;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.ModLoader.IO;

namespace flameout
{
    public class BurnoutSystem : ModSystem
    {
        public const byte MessageToggleBurnout = 0;

        public static bool Active;
        public static bool DownedApathy;
        public static bool DownedEmpathy;

        public static bool AnyBossAlive()
        {
            for (int i = 0; i < Main.maxNPCs; i++)
            {
                if (Main.npc[i].active && Main.npc[i].boss)
                    return true;
            }
            return false;
        }

        public static void SetActive(bool value)
        {
            Active = value;
            Sync();
        }

        public static void MarkApathyDowned()
        {
            DownedApathy = true;
            Sync();
        }

        public static void MarkEmpathyDowned()
        {
            DownedEmpathy = true;
            Sync();
        }

        static void Sync()
        {
            if (Main.netMode == NetmodeID.Server)
                NetMessage.SendData(MessageID.WorldData);
        }

        public override void ClearWorld()
        {
            Active = false;
            DownedApathy = false;
            DownedEmpathy = false;
        }

        public override void SaveWorldData(TagCompound tag)
        {
            tag["active"] = Active;
            tag["apathy"] = DownedApathy;
            tag["empathy"] = DownedEmpathy;
        }

        public override void LoadWorldData(TagCompound tag)
        {
            Active = tag.GetBool("active");
            DownedApathy = tag.GetBool("apathy");
            DownedEmpathy = tag.GetBool("empathy");
        }

        public override void NetSend(BinaryWriter writer)
        {
            writer.Write(new BitsByte(Active, DownedApathy, DownedEmpathy));
        }

        public override void NetReceive(BinaryReader reader)
        {
            BitsByte flags = reader.ReadByte();
            flags.Retrieve(ref Active, ref DownedApathy, ref DownedEmpathy);
        }
    }

    public class BurnoutNPC : GlobalNPC
    {
        public override void ApplyDifficultyAndPlayerScaling(NPC npc, int numPlayers, float balance, float bossAdjustment)
        {
            if (!BurnoutSystem.Active || npc.friendly || npc.townNPC || npc.lifeMax <= 5)
                return;

            npc.lifeMax = (int)(npc.lifeMax * 1.5f);
        }
    }

    public class BurnoutPlayer : ModPlayer
    {
        public List<string> GivenWorlds = new();

        public override void SaveData(TagCompound tag)
        {
            tag["givenWorlds"] = GivenWorlds;
        }

        public override void LoadData(TagCompound tag)
        {
            GivenWorlds = new List<string>(tag.GetList<string>("givenWorlds"));
        }

        public override void OnEnterWorld()
        {
            TryGiveCandle();
        }

        public override void PostUpdate()
        {
            TryGiveCandle();
            if (ApathyWorld.downedApathy && !BurnoutSystem.DownedApathy)
                BurnoutSystem.MarkApathyDowned();
            if (ApathyWorld.downedEmpathy && !BurnoutSystem.DownedEmpathy)
                BurnoutSystem.MarkEmpathyDowned();
        }

        void TryGiveCandle()
        {
            if (!BurnoutSystem.Active || Main.ActiveWorldFileData == null)
                return;

            int candle = ModContent.ItemType<EternalCandle>();
            if (Player.HasItem(candle))
                return;

            string id = Main.ActiveWorldFileData.UniqueId.ToString();
            if (GivenWorlds.Contains(id))
                return;

            Player.QuickSpawnItem(Player.GetSource_GiftOrReward("flameout:burnout"), candle);
            GivenWorlds.Add(id);
        }

        public override void ModifyHurt(ref Player.HurtModifiers modifiers)
        {
            if (BurnoutSystem.Active)
                modifiers.FinalDamage *= 2f;
        }

        public override void UpdateBadLifeRegen()
        {
            if (BurnoutSystem.Active && Player.lifeRegen < 0)
                Player.lifeRegen *= 2;
        }
    }
}