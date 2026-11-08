using Microsoft.Xna.Framework;
using Terraria;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;

namespace flameout.Content
{
    public class ApatheticStaff : ModItem
    {
       

        public override void SetStaticDefaults()
        {
            Item.staff[Type] = true;
        }

        public override void SetDefaults()
        {
            Item.damage = 34;
            Item.DamageType = DamageClass.Magic;
            Item.mana = 12;
            Item.width = 40;
            Item.height = 40;
            Item.useTime = 28;
            Item.useAnimation = 28;
            Item.useStyle = ItemUseStyleID.Shoot;
            Item.noMelee = true;
            Item.knockBack = 3f;
            Item.value = Item.sellPrice(gold: 2);
            Item.rare = ItemRarityID.Pink;
            Item.UseSound = SoundID.Item43;
            Item.autoReuse = true;
            Item.shoot = ModContent.ProjectileType<ApatheticBolt>();
            Item.shootSpeed = 11f;
        }

        public override bool Shoot(Player player, EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockback)
        {
            Vector2 direction = velocity.SafeNormalize(Vector2.UnitX * player.direction);
            Vector2 tipPosition = player.MountedCenter + direction * 46f;
            Vector2 spawnPosition = Collision.CanHitLine(player.MountedCenter, 0, 0, tipPosition, 0, 0) ? tipPosition : position;

            for (int i = -1; i <= 1; i++)
            {
                Vector2 shotVelocity = velocity.RotatedBy(MathHelper.ToRadians(6f * i));
                Projectile.NewProjectile(source, spawnPosition, shotVelocity, type, damage, knockback, player.whoAmI);
            }

            return false;
        }

        public override void AddRecipes()
        {
            CreateRecipe()
                .AddIngredient<ApatheticEssence>(20)
                .AddIngredient(ItemID.DemoniteBar, 20)
                .AddTile(TileID.Anvils)
                .Register();

            CreateRecipe()
                .AddIngredient<ApatheticEssence>(20)
                .AddIngredient(ItemID.CrimtaneBar, 20)
                .AddTile(TileID.Anvils)
                .Register();
        }
    }
}
