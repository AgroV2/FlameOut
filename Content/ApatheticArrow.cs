using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace flameout.Content
{
	public class ApatheticArrow : ModItem
	{
		
		public const float SpeedBonus = 1.6f;

		public override void SetDefaults()
		{
			Item.damage = 18;
			Item.DamageType = DamageClass.Ranged;
			Item.width = 16;
			Item.height = 32;
			Item.maxStack = Item.CommonMaxStack;
			Item.consumable = true;
			Item.knockBack = 2.5f;
			Item.value = Item.sellPrice(copper: 60);
			Item.rare = ItemRarityID.Pink;
			Item.shoot = ModContent.ProjectileType<ApatheticArrowProjectile>();
			Item.shootSpeed = 4f;
			Item.ammo = AmmoID.Arrow;
		}

		
			
		public override void PickAmmo(Item weapon, Player player, ref int type, ref float speed,
			ref StatModifier damage, ref float knockback)
		{
			speed *= SpeedBonus;
		}
		public override void AddRecipes()
		{
			CreateRecipe(100)
				.AddIngredient(ItemID.WoodenArrow, 100)
				.AddIngredient<ApatheticShards>(5)
				.AddTile(TileID.Anvils)
				.Register();
		}
	}

	public class ApatheticArrowProjectile : ModProjectile
	{
		public override string Texture => "flameout/Content/ApatheticSpiritArrow";

		public override void SetDefaults()
		{
			Projectile.width = 12;
			Projectile.height = 32;
			Projectile.friendly = true;
			Projectile.hostile = false;
			Projectile.DamageType = DamageClass.Ranged;
			Projectile.arrow = true;
			Projectile.penetrate = 1;
			Projectile.timeLeft = 60 * 10;   
			Projectile.tileCollide = false;  
			Projectile.ignoreWater = true;
			Projectile.aiStyle = -1;         
			Projectile.light = 0.4f;
			Projectile.extraUpdates = 1;     
		}

		public override void OnSpawn(Terraria.DataStructures.IEntitySource source)
		{
			
			Projectile.GetGlobalProjectile<ApatheticGlobalProjectile>().ApatheticShot = true;
		}

		public override void AI()
		{
			Projectile.rotation = Projectile.velocity.ToRotation() + MathHelper.PiOver2;

			Lighting.AddLight(Projectile.Center, 0.4f, 0.15f, 0.7f);

			if (Main.rand.NextBool(3))
			{
				Dust dust = Dust.NewDustPerfect(Projectile.Center, DustID.PurpleTorch,
					-Projectile.velocity * 0.1f, 0, default, 1.1f);
				dust.noGravity = true;
			}
		}

		public override void OnKill(int timeLeft)
		{
			for (int i = 0; i < 8; i++)
			{
				Dust dust = Dust.NewDustPerfect(Projectile.Center, DustID.PurpleTorch,
					Main.rand.NextVector2Circular(3f, 3f), 0, default, 1.2f);
				dust.noGravity = true;
			}
		}
	}
}