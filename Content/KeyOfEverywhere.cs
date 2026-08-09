

using Terraria;
using Terraria.Audio;
using Terraria.ID;
using Terraria.ModLoader;

namespace flameout.Content
{
    public class KeyOfEverywhere : ModItem
    {
        public override void SetDefaults()
        {
            Item.width = 28;
            Item.height = 28;
            Item.maxStack = Item.CommonMaxStack;
            Item.useAnimation = 40;
            Item.useTime = 40;
            Item.useStyle = ItemUseStyleID.HoldUp;
            Item.consumable = false;
            Item.rare = ItemRarityID.Purple;
            Item.value = Item.buyPrice(0, 10, 0, 0);
        }

        public override bool CanUseItem(Player player)
        {
            return !NPC.AnyNPCs(ModContent.NPCType<Empathy>());
        }

        public override bool? UseItem(Player player)
        {
            if (player.whoAmI == Main.myPlayer)
            {
                SoundEngine.PlaySound(new SoundStyle("flameout/Content/Sounds/em1"), player.Center);

                int type = ModContent.NPCType<Empathy>();
                if (Main.netMode != NetmodeID.MultiplayerClient)
                {
                    NPC.SpawnOnPlayer(player.whoAmI, type);
                }
                else
                {
                    NetMessage.SendData(MessageID.SpawnBossUseLicenseStartEvent, number: player.whoAmI, number2: type);
                }
            }
            return true;
        }
    }
}

