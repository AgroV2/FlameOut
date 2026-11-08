using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.Audio;
using Terraria.DataStructures;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.ModLoader;

namespace flameout.Content
{
	public class ApatheticArchStaff : ModItem
	{
		
		public const int BoltCount = 4;
		public const float BoltSpreadDegrees = 9f;
		public const int BaseUseTime = 28;
		public const int BaseMana = 16;

		
		public const int LaserWindup = 120;      
		public const int LaserDuration = 60;     
		public const int LaserCooldown = 300;    
		public const int LaserMana = 60;

		public override void SetStaticDefaults()
		{
			Item.staff[Type] = true;
		}

		public override void SetDefaults()
		{
			Item.damage = 52;
			Item.DamageType = DamageClass.Magic;
			Item.mana = BaseMana;
			Item.width = 40;
			Item.height = 40;
			Item.useTime = BaseUseTime;
			Item.useAnimation = BaseUseTime;
			Item.useStyle = ItemUseStyleID.Shoot;
			Item.noMelee = true;
			Item.knockBack = 3.5f;
			Item.value = Item.sellPrice(gold: 5);
			Item.rare = ItemRarityID.Purple;
			Item.UseSound = SoundID.Item43;
			Item.autoReuse = true;
			Item.shoot = ModContent.ProjectileType<ApatheticArchBolt>();
			Item.shootSpeed = 12f;
		}

		public override bool AltFunctionUse(Player player) => true;

		public override bool CanUseItem(Player player)
		{
			if (player.altFunctionUse == 2)
			{
				
				if (player.GetModPlayer<ApatheticArchStaffPlayer>().LaserCooldown > 0)
					return false;

				if (player.ownedProjectileCounts[ModContent.ProjectileType<ApatheticArchLaser>()] > 0)
					return false;

				Item.useTime = LaserWindup + LaserDuration;
				Item.useAnimation = LaserWindup + LaserDuration;
				Item.mana = LaserMana;
				Item.autoReuse = false;
				Item.UseSound = null;   
				Item.shoot = ModContent.ProjectileType<ApatheticArchLaser>();
				Item.shootSpeed = 1f;
			}
			else
			{
				Item.useTime = BaseUseTime;
				Item.useAnimation = BaseUseTime;
				Item.mana = BaseMana;
				Item.autoReuse = true;
				Item.UseSound = SoundID.Item43;
				Item.shoot = ModContent.ProjectileType<ApatheticArchBolt>();
				Item.shootSpeed = 12f;
			}

			return base.CanUseItem(player);
		}

		public override bool Shoot(Player player, EntitySource_ItemUse_WithAmmo source, Vector2 position,
			Vector2 velocity, int type, int damage, float knockback)
		{
			Vector2 direction = velocity.SafeNormalize(Vector2.UnitX * player.direction);
			Vector2 tipPosition = player.MountedCenter + direction * 46f;
			Vector2 spawnPosition = Collision.CanHitLine(player.MountedCenter, 0, 0, tipPosition, 0, 0)
				? tipPosition
				: position;

			
			if (player.altFunctionUse == 2)
			{
				Projectile.NewProjectile(source, player.MountedCenter, direction,
					ModContent.ProjectileType<ApatheticArchLaser>(),
					(int)(damage * 2.2f), knockback, player.whoAmI);

				player.GetModPlayer<ApatheticArchStaffPlayer>().LaserCooldown = LaserWindup + LaserDuration + LaserCooldown;

				return false;
			}

			
			for (int i = 0; i < BoltCount; i++)
			{
				float offset = MathHelper.Lerp(-BoltSpreadDegrees, BoltSpreadDegrees, i / (float)(BoltCount - 1));

				Vector2 shotVelocity = velocity.RotatedBy(MathHelper.ToRadians(offset));

				Projectile.NewProjectile(source, spawnPosition, shotVelocity,
					ModContent.ProjectileType<ApatheticArchBolt>(), damage, knockback, player.whoAmI);
			}

			return false;
		}

		public override void AddRecipes()
		{
			CreateRecipe()
				.AddIngredient<ApatheticStaff>()
				.AddIngredient<ApatheticEssence>(35)
			 .AddIngredient<ApatheticShards>(20)
				.AddTile(TileID.Anvils)
				.Register();
		}
	}

	

	public class ApatheticArchStaffPlayer : ModPlayer
	{
		public int LaserCooldown;

		public override void PostUpdate()
		{
			if (LaserCooldown > 0)
				LaserCooldown--;
		}
	}

	

	public class ApatheticArchBolt : ModProjectile
	{
		public const float HomingRange = 620f;
		public const float HomingStrength = 0.09f;   
		public const int HomingDelay = 8;            

		public override string Texture => "flameout/Content/ApatheticSpiritParticle";

		public override void SetDefaults()
		{
			Projectile.width = 14;
			Projectile.height = 14;
			Projectile.friendly = true;
			Projectile.hostile = false;
			Projectile.DamageType = DamageClass.Magic;
			Projectile.penetrate = 2;
			Projectile.timeLeft = 300;
			Projectile.tileCollide = true;
			Projectile.ignoreWater = true;
			Projectile.aiStyle = -1;
			Projectile.light = 0.5f;
			Projectile.usesLocalNPCImmunity = true;
			Projectile.localNPCHitCooldown = 10;
		}

		public override void AI()
		{
			Projectile.rotation = Projectile.velocity.ToRotation() + MathHelper.PiOver2;

			Projectile.localAI[0]++;

			
			if (Projectile.localAI[0] > HomingDelay)
			{
				NPC target = FindTarget();

				if (target != null)
				{
					float speed = Projectile.velocity.Length();

					Vector2 desired = (target.Center - Projectile.Center).SafeNormalize(Projectile.velocity) * speed;

					Projectile.velocity = Vector2.Lerp(Projectile.velocity, desired, HomingStrength);
					Projectile.velocity = Projectile.velocity.SafeNormalize(Vector2.UnitY) * speed;
				}
			}

			Lighting.AddLight(Projectile.Center, 0.5f, 0.2f, 0.8f);

			if (Main.rand.NextBool(2))
			{
				Dust dust = Dust.NewDustPerfect(Projectile.Center, DustID.PurpleTorch,
					-Projectile.velocity * 0.15f, 0, default, 1.2f);
				dust.noGravity = true;
			}
		}

		private NPC FindTarget()
		{
			NPC closest = null;
			float closestDistance = HomingRange;

			for (int i = 0; i < Main.maxNPCs; i++)
			{
				NPC npc = Main.npc[i];

				if (!npc.CanBeChasedBy(Projectile))
					continue;

				float distance = Vector2.Distance(npc.Center, Projectile.Center);

				if (distance < closestDistance
					&& Collision.CanHitLine(Projectile.Center, 0, 0, npc.Center, 0, 0))
				{
					closest = npc;
					closestDistance = distance;
				}
			}

			return closest;
		}

		public override void OnKill(int timeLeft)
		{
			SoundEngine.PlaySound(SoundID.Item10 with { Volume = 0.4f }, Projectile.Center);

			for (int i = 0; i < 12; i++)
			{
				Dust dust = Dust.NewDustPerfect(Projectile.Center, DustID.PurpleTorch,
					Main.rand.NextVector2Circular(4f, 4f), 0, default, 1.4f);
				dust.noGravity = true;
			}
		}
	}

	

	public class ApatheticArchLaser : ModProjectile
	{
		public const float BeamLength = 2200f;
		public const float BeamWidth = 26f;
		public const float HoldoutOffset = 40f;

		public override string Texture => "Terraria/Images/Projectile_0";

		public override void SetDefaults()
		{
			Projectile.width = 1;
			Projectile.height = 1;
			Projectile.friendly = true;
			Projectile.hostile = false;
			Projectile.DamageType = DamageClass.Magic;
			Projectile.penetrate = -1;
			Projectile.timeLeft = 2;
			Projectile.tileCollide = false;
			Projectile.ignoreWater = true;
			Projectile.aiStyle = -1;
			Projectile.usesLocalNPCImmunity = true;
			Projectile.localNPCHitCooldown = 8;
			Projectile.netImportant = true;
		}

		public override bool ShouldUpdatePosition() => false;

		
		private float Timer => Projectile.localAI[0];

		private bool Firing => Timer > ApatheticArchStaff.LaserWindup;

		private float CurrentLength
		{
			get
			{
				if (!Firing)
					return 0f;

				float fireProgress = (Timer - ApatheticArchStaff.LaserWindup) / ApatheticArchStaff.LaserDuration;

				
				float scale = fireProgress < 0.12f
					? fireProgress / 0.12f
					: 1f - MathHelper.Clamp((fireProgress - 0.85f) / 0.15f, 0f, 1f);

				return BeamLength * MathHelper.Clamp(scale, 0f, 1f);
			}
		}

		private Vector2 Start => Main.player[Projectile.owner].MountedCenter
			+ Projectile.velocity * HoldoutOffset;

		private Vector2 End => Start + Projectile.velocity * CurrentLength;

		public override void AI()
		{
			Player player = Main.player[Projectile.owner];

			if (!player.active || player.dead || player.noItems || player.CCed)
			{
				Projectile.Kill();
				return;
			}

			Projectile.localAI[0]++;

			if (Timer > ApatheticArchStaff.LaserWindup + ApatheticArchStaff.LaserDuration)
			{
				Projectile.Kill();
				return;
			}

			Projectile.timeLeft = 2;

			
			if (Projectile.owner == Main.myPlayer)
			{
				Vector2 aim = (Main.MouseWorld - player.MountedCenter).SafeNormalize(Vector2.UnitX * player.direction);

				float turn = Firing ? 0.06f : 0.35f;

				Projectile.velocity = Vector2.Lerp(Projectile.velocity, aim, turn)
					.SafeNormalize(Vector2.UnitX * player.direction);

				Projectile.netUpdate = true;
			}

			Projectile.Center = Start;
			Projectile.rotation = Projectile.velocity.ToRotation();

			
			player.heldProj = Projectile.whoAmI;
			player.itemTime = 2;
			player.itemAnimation = 2;
			player.ChangeDir(Projectile.velocity.X > 0f ? 1 : -1);
			player.itemRotation = (Projectile.velocity * player.direction).ToRotation();

			if (Timer == 1f)
				SoundEngine.PlaySound(SoundID.Item15 with { Pitch = -0.3f }, player.Center);

			
			if (!Firing)
			{
				float charge = Timer / ApatheticArchStaff.LaserWindup;

				Lighting.AddLight(Start, 0.9f * charge, 0.3f * charge, 1.4f * charge);

				for (int i = 0; i < 2; i++)
				{
					Vector2 offset = Main.rand.NextVector2CircularEdge(1f, 1f)
						* MathHelper.Lerp(110f, 18f, charge);

					Dust dust = Dust.NewDustPerfect(Start + offset, DustID.PurpleTorch,
						-offset * 0.05f, 0, default, 1f + charge);
					dust.noGravity = true;
				}

				if (Timer == ApatheticArchStaff.LaserWindup)
				{
					SoundEngine.PlaySound(SoundID.Item72 with { Pitch = -0.2f }, player.Center);
					SoundEngine.PlaySound(SoundID.Item122 with { Volume = 0.8f }, player.Center);

					for (int i = 0; i < 30; i++)
					{
						Dust dust = Dust.NewDustPerfect(Start, DustID.PurpleTorch,
							Projectile.velocity.RotatedByRandom(0.5f) * Main.rand.NextFloat(3f, 9f),
							0, default, 1.8f);
						dust.noGravity = true;
					}
				}

				return;
			}

			
			Lighting.AddLight(Start, 1.2f, 0.4f, 1.8f);

			player.velocity -= Projectile.velocity * 0.18f;   

			float length = CurrentLength;

			for (int i = 0; i < 3; i++)
			{
				Vector2 position = Start + Projectile.velocity * Main.rand.NextFloat(0f, length);

				Dust dust = Dust.NewDustPerfect(position, DustID.PurpleTorch,
					Projectile.velocity.RotatedBy(MathHelper.PiOver2) * Main.rand.NextFloat(-2f, 2f),
					0, default, 1.5f);
				dust.noGravity = true;

				Lighting.AddLight(position, 0.6f, 0.2f, 0.9f);
			}
		}

		public override bool? Colliding(Rectangle projHitbox, Rectangle targetHitbox)
		{
			if (!Firing || CurrentLength < 1f)
				return false;

			float collisionPoint = 0f;

			return Collision.CheckAABBvLineCollision(
				targetHitbox.TopLeft(), targetHitbox.Size(),
				Start, End, BeamWidth, ref collisionPoint);
		}

		public override void CutTiles()
		{
			if (!Firing)
				return;

			Utils.PlotTileLine(Start, End, BeamWidth, DelegateMethods.CutTiles);
		}

		public override bool PreDraw(ref Color lightColor)
		{
			if (!Firing)
				return false;

			float length = CurrentLength;

			if (length < 1f)
				return false;

			Texture2D pixel = TextureAssets.MagicPixel.Value;
			Rectangle source = new Rectangle(0, 0, 1, 1);

			Vector2 start = Start - Main.screenPosition;
			float rotation = Projectile.velocity.ToRotation();

			float pulse = 0.85f + 0.15f * (float)Math.Sin(Main.GlobalTimeWrappedHourly * 12f);

			
			for (int i = 4; i >= 0; i--)
			{
				Color layerColor = Color.Lerp(new Color(140, 60, 255), new Color(90, 110, 255), i / 4f)
					* ((0.2f - i * 0.03f) * pulse);
				layerColor.A = 0;

				float width = BeamWidth * (1f + i * 0.6f) * pulse;

				Main.spriteBatch.Draw(pixel, start, source, layerColor, rotation,
					new Vector2(0f, 0.5f), new Vector2(length, width), SpriteEffects.None, 0f);
			}

			
			Color core = Color.Lerp(new Color(235, 205, 255), new Color(190, 140, 255), 0.5f) * pulse;
			core.A = 0;

			Main.spriteBatch.Draw(pixel, start, source, core, rotation,
				new Vector2(0f, 0.5f), new Vector2(length, BeamWidth * 0.35f * pulse),
				SpriteEffects.None, 0f);

			
			Color flash = new Color(220, 180, 255) * (0.8f * pulse);
			flash.A = 0;

			Main.spriteBatch.Draw(pixel, start, source, flash, 0f,
				new Vector2(0.5f), new Vector2(BeamWidth * 2.2f * pulse), SpriteEffects.None, 0f);

			return false;
		}
	}
}