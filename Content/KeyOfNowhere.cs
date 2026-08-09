

using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace flameout.Content
{
    public class KeyOfNowhere : ModItem
    {
        private const float PortalDistanceTiles = 35f;

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
            Item.UseSound = SoundID.Unlock;
        }

        public override bool CanUseItem(Player player)
        {
            if (NPC.AnyNPCs(ModContent.NPCType<ApathyBoss>()))
                return false;

            for (int i = 0; i < Main.maxProjectiles; i++)
            {
                Projectile p = Main.projectile[i];
                if (p.active && p.type == ModContent.ProjectileType<ApathyPortal>())
                    return false;
            }

            return true;
        }

        public override bool? UseItem(Player player)
        {
            if (player.whoAmI == Main.myPlayer)
            {
                float side = Main.rand.NextBool() ? 0f : MathHelper.Pi;
                float angle = side + Main.rand.NextFloatDirection() * MathHelper.ToRadians(50f);
                Vector2 spawnPos = player.Center + angle.ToRotationVector2() * (PortalDistanceTiles * 16f);

                spawnPos.X = MathHelper.Clamp(spawnPos.X, 640f, Main.maxTilesX * 16f - 640f);
                spawnPos.Y = MathHelper.Clamp(spawnPos.Y, 640f, Main.maxTilesY * 16f - 640f);

                Projectile.NewProjectile(
                    player.GetSource_ItemUse(Item),
                    spawnPos,
                    Vector2.Zero,
                    ModContent.ProjectileType<ApathyPortal>(),
                    0, 0f, player.whoAmI);
            }

            return true;
        }

        public override void AddRecipes()
        {
            Recipe recipe = CreateRecipe();
            recipe.AddIngredient(ItemID.LunarBar, 50);
            recipe.AddIngredient(ItemID.GoldenKey, 1);
            recipe.AddTile(TileID.MythrilAnvil);
            recipe.Register();
        }
    }
}

