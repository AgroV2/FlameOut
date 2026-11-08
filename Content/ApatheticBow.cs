using Microsoft.Xna.Framework;
using Terraria;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;

namespace flameout.Content
{
	public class ApatheticBow : ModItem
	{
		public const int SingleCooldown = 15;   
		public const int BurstInterval = 60;   
		public const int BurstShots = 5;
		public const float SpreadDegrees = 4f;  

		
		private int burstTimer;
		
		private int extraAmmoToConsume;

		public override void SetDefaults()
		{
			Item.damage = 20;
			Item.DamageType = DamageClass.Ranged;
			Item.width = 24;
			Item.height = 44;
			Item.useTime = SingleCooldown;
			Item.useAnimation = SingleCooldown;
			Item.useStyle = ItemUseStyleID.Shoot;
			Item.noMelee = true;
			Item.knockBack = 2f;
			Item.value = Item.sellPrice(silver: 50);
			Item.rare = ItemRarityID.Blue;
			Item.UseSound = SoundID.Item5;
			Item.autoReuse = true;   
			Item.useTurn = false;
			Item.shoot = ProjectileID.WoodenArrowFriendly;
			Item.useAmmo = AmmoID.Arrow;
			Item.shootSpeed = 11f;
		}

		
		public override void HoldItem(Player player)
		{
			if (burstTimer < BurstInterval)
				burstTimer++;
		}

		public override bool Shoot(Player player, EntitySource_ItemUse_WithAmmo source,
			Vector2 position, Vector2 velocity, int type, int damage, float knockback)
		{
			
			SpawnArrow(source, position, velocity, type, damage, knockback, player);

			extraAmmoToConsume = 0;

			
			if (burstTimer >= BurstInterval)
			{
				burstTimer = 0;
				extraAmmoToConsume = BurstShots;

				for (int i = 0; i < BurstShots; i++)
				{
					float offset = MathHelper.Lerp(-SpreadDegrees, SpreadDegrees, i / (float)(BurstShots - 1));
					Vector2 shotVelocity = velocity.RotatedBy(MathHelper.ToRadians(offset));

					SpawnArrow(source, position, shotVelocity, type, damage, knockback, player);
				}
			}

			return false; 
		}

		private static void SpawnArrow(IEntitySource source, Vector2 position, Vector2 velocity,
			int type, int damage, float knockback, Player player)
		{
			Projectile proj = Projectile.NewProjectileDirect(source, position, velocity,
				type, damage, knockback, player.whoAmI);

			proj.GetGlobalProjectile<ApatheticGlobalProjectile>().ApatheticShot = true;
		}

		
		public override void OnConsumeAmmo(Item ammo, Player player)
		{
			for (int i = 0; i < extraAmmoToConsume; i++)
				player.ConsumeItem(ammo.type);

			extraAmmoToConsume = 0;
		}

		public override void AddRecipes()
		{
			CreateRecipe()
			 .AddIngredient<ApatheticEssence>(20)
				.AddIngredient(ItemID.Wood, 10)
				.AddIngredient(ItemID.Cobweb, 20)
				.AddTile(TileID.WorkBenches)
				.Register();
		}
	}

}
