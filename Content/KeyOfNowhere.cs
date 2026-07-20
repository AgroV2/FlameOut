using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Microsoft.Xna.Framework;

namespace flameout.Content
{
    public class KeyOfNowhere : ModItem
    {
        public override void SetDefaults()
        {
            Item.width = 32;
            Item.height = 32;
            Item.useTime = 45;
            Item.useAnimation = 45;
            Item.useStyle = ItemUseStyleID.HoldUp;
            Item.noMelee = true;
            Item.value = 0;
            Item.rare = ItemRarityID.Purple;
        }

        public override void AddRecipes()
        {
            Recipe recipe = CreateRecipe();
            recipe.AddIngredient(ItemID.LunarBar, 50);
            recipe.AddIngredient(ItemID.GoldenKey, 1);
            recipe.AddTile(TileID.MythrilAnvil);
            recipe.Register();
        }

        public override bool? UseItem(Player player)
        {
            if (player.whoAmI == Main.myPlayer)
            {
                
                float angle = Main.rand.NextFloat(MathHelper.TwoPi);
                float dist = 75 * 16f; 
                Vector2 spawnPos = player.Center + new Vector2(
                    (float)System.Math.Cos(angle),
                    (float)System.Math.Sin(angle)
                ) * dist;

                int bossIndex = NPC.NewNPC(
                    NPC.GetBossSpawnSource(player.whoAmI),
                    (int)spawnPos.X,
                    (int)spawnPos.Y,
                    ModContent.NPCType<ApathyBoss>()
                );

                if (bossIndex < Main.maxNPCs)
                {
                    Main.npc[bossIndex].Center = spawnPos;
                }
            }
            return true;
        }
    }
}