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
	

	public class ApatheticCellStaff : ModItem
	{
		public override void SetDefaults()
		{
			Item.damage = 22;
			Item.knockBack = 2f;
			Item.mana = 20;               
			Item.width = 40;
			Item.height = 40;
			Item.useTime = 26;
			Item.useAnimation = 26;
			Item.useStyle = ItemUseStyleID.Shoot;
			Item.value = Item.sellPrice(gold: 1);
			Item.rare = ItemRarityID.Orange;
			Item.UseSound = SoundID.Item44;
			Item.noMelee = true;
			Item.DamageType = DamageClass.Summon;
			Item.buffType = ModContent.BuffType<ApatheticCellBuff>();
			Item.shoot = ModContent.ProjectileType<ApatheticCell>();
		}

		public override bool Shoot(Player player, EntitySource_ItemUse_WithAmmo source,
			Vector2 position, Vector2 velocity, int type, int damage, float knockback)
		{
			player.AddBuff(Item.buffType, 2);

			Projectile proj = Projectile.NewProjectileDirect(source, Main.MouseWorld, Vector2.Zero,
				type, damage, knockback, player.whoAmI);
			proj.originalDamage = Item.damage;

			return false;
		}

		
	}

	public class ApatheticCellBuff : ModBuff
	{
		public override void SetStaticDefaults()
		{
			Main.buffNoSave[Type] = true;
			Main.buffNoTimeDisplay[Type] = true;
		}

		public override void Update(Player player, ref int buffIndex)
		{
			if (player.ownedProjectileCounts[ModContent.ProjectileType<ApatheticCell>()] > 0)
			{
				player.buffTime[buffIndex] = 18000;
			}
			else
			{
				player.DelBuff(buffIndex);
				buffIndex--;
			}
		}
	}

	

		public class ApatheticCell : ModProjectile
	{
		public const float OrbitRadius = 110f;      
		public const float OrbitSpeed = 0.022f;     
		public const int ShootCooldown = 60;        
		public const float ShootRange = 750f;
		public const float LaserSpeed = 12f;

		public const int FrameCount = 4;            
		public const int TicksPerFrame = 6;         

		public static readonly SoundStyle LaserShot =
			new SoundStyle("flameout/Content/Sounds/ApatheticLaserShot")
			{
				Volume = 0.65f,
				PitchVariance = 0.2f
			};

		public override void SetStaticDefaults()
		{
			Main.projFrames[Type] = FrameCount;   
			Main.projPet[Type] = true;
			ProjectileID.Sets.MinionSacrificable[Type] = true;
			ProjectileID.Sets.MinionTargettingFeature[Type] = true;
			ProjectileID.Sets.CultistIsResistantTo[Type] = true;
		}

		public override void SetDefaults()
		{
			Projectile.width = 31;     
			Projectile.height = 31;
			Projectile.friendly = true;
			Projectile.minion = true;
			Projectile.minionSlots = 1f;
			Projectile.penetrate = -1;
			Projectile.timeLeft = 18000;
			Projectile.tileCollide = false;
			Projectile.ignoreWater = true;
			Projectile.netImportant = true;
			Projectile.DamageType = DamageClass.Summon;
		}

		public override bool MinionContactDamage() => false;

			public override void AI()
		{
			Player player = Main.player[Projectile.owner];

			
			if (++Projectile.frameCounter >= TicksPerFrame)
			{
				Projectile.frameCounter = 0;
				Projectile.frame = (Projectile.frame + 1) % Main.projFrames[Projectile.type];
			}

			if (player.dead || !player.active)
			{
				player.ClearBuff(ModContent.BuffType<ApatheticCellBuff>());
				Projectile.Kill();
				return;
			}

			

			if (player.HasBuff<ApatheticCellBuff>())
				Projectile.timeLeft = 2;

			
			GetMinionOrder(out int index, out int count);

			
			float baseAngle = Main.GameUpdateCount * OrbitSpeed;
			float slotAngle = baseAngle + MathHelper.TwoPi * index / count;

			Vector2 desired = player.MountedCenter + slotAngle.ToRotationVector2() * OrbitRadius;
			Vector2 toDesired = desired - Projectile.Center;

			if (toDesired.Length() > 900f)
			{
				Projectile.Center = desired;
				Projectile.velocity = Vector2.Zero;
				Projectile.netUpdate = true;
			}
			else
			{
				Projectile.velocity = Vector2.Lerp(Projectile.velocity, toDesired * 0.25f, 0.5f);
			}

			
			Projectile.ai[0]++;

			NPC target = FindTarget(player);

			if (target != null)
			{
				Projectile.rotation = (target.Center - Projectile.Center).ToRotation();

				if (Projectile.ai[0] >= ShootCooldown)
				{
					Projectile.ai[0] = 0;

					Vector2 shootVelocity =
						(target.Center - Projectile.Center).SafeNormalize(Vector2.UnitX) * LaserSpeed;

					Projectile.NewProjectile(Projectile.GetSource_FromAI(), Projectile.Center,
						shootVelocity, ModContent.ProjectileType<ApatheticLaserProjectile>(),
						Projectile.damage, Projectile.knockBack, Projectile.owner);

					SoundEngine.PlaySound(LaserShot, Projectile.Center);
					Projectile.netUpdate = true;
				}
			}
			else
			{
				Projectile.rotation = slotAngle + MathHelper.PiOver2;

				if (Projectile.ai[0] > ShootCooldown)
					Projectile.ai[0] = ShootCooldown;
			}

			Lighting.AddLight(Projectile.Center, new Vector3(0.5f, 0.15f, 0.7f));
		}

		
		private void GetMinionOrder(out int index, out int count)
		{
			index = 0;
			count = 0;

			for (int i = 0; i < Main.maxProjectiles; i++)
			{
				Projectile other = Main.projectile[i];

				if (!other.active || other.type != Projectile.type || other.owner != Projectile.owner)
					continue;

				if (i < Projectile.whoAmI)
					index++;

				count++;
			}

			if (count < 1)
				count = 1;
		}

		private NPC FindTarget(Player player)
		{
			if (player.HasMinionAttackTargetNPC)
			{
				NPC marked = Main.npc[player.MinionAttackTargetNPC];

				if (marked.CanBeChasedBy(Projectile) &&
					Vector2.Distance(marked.Center, Projectile.Center) <= ShootRange * 1.5f)
					return marked;
			}

			NPC closest = null;
			float closestDist = ShootRange;

			for (int i = 0; i < Main.maxNPCs; i++)
			{
				NPC npc = Main.npc[i];

				if (!npc.CanBeChasedBy(Projectile))
					continue;

				float dist = Vector2.Distance(npc.Center, Projectile.Center);

				if (dist < closestDist)
				{
					closestDist = dist;
					closest = npc;
				}
			}

			return closest;
		}

		
		public override void PostDraw(Color lightColor)
		{
			GetMinionOrder(out int index, out _);

			if (index != 0)
				return;

			Player player = Main.player[Projectile.owner];
			Vector2 center = player.MountedCenter - Main.screenPosition;

			Texture2D pixel = TextureAssets.MagicPixel.Value;
			Rectangle source = new Rectangle(0, 0, 1, 1);

			const int Segments = 90;
			float step = MathHelper.TwoPi / Segments;

			float pulse = 0.75f + 0.25f * (float)Math.Sin(Main.GlobalTimeWrappedHourly * 3f);
			Color ringColor = new Color(165, 70, 255) * 0.55f * pulse;

			for (int i = 0; i < Segments; i++)
			{
				float a = i * step;
				Vector2 p1 = center + a.ToRotationVector2() * OrbitRadius;
				Vector2 p2 = center + (a + step).ToRotationVector2() * OrbitRadius;
				Vector2 diff = p2 - p1;

				Main.spriteBatch.Draw(pixel, p1, source, ringColor, diff.ToRotation(),
					Vector2.Zero, new Vector2(diff.Length() + 1f, 2.5f), SpriteEffects.None, 0f);
			}
		}
	}



	public class ApatheticLaserProjectile : ModProjectile
	{
		public override void SetDefaults()
		{
			Projectile.width = 10;
			Projectile.height = 10;
			Projectile.friendly = true;
			Projectile.hostile = false;
			Projectile.penetrate = 1;
			Projectile.timeLeft = 180;
			Projectile.tileCollide = true;
			Projectile.ignoreWater = true;
			Projectile.DamageType = DamageClass.Summon;
			Projectile.usesLocalNPCImmunity = true;
			Projectile.localNPCHitCooldown = 10;
			Projectile.extraUpdates = 1;
		}

		public override void AI()
		{
			Projectile.rotation = Projectile.velocity.ToRotation() + MathHelper.PiOver2;

			if (Main.rand.NextBool(2))
			{
				Dust dust = Dust.NewDustPerfect(Projectile.Center, DustID.PurpleTorch,
					Vector2.Zero, 0, default, 1.1f);
				dust.noGravity = true;
			}

			Lighting.AddLight(Projectile.Center, new Vector3(0.45f, 0.1f, 0.6f));
		}

		public override void OnKill(int timeLeft)
		{
			for (int i = 0; i < 8; i++)
			{
				Dust dust = Dust.NewDustPerfect(Projectile.Center, DustID.PurpleTorch,
					Main.rand.NextVector2Circular(2f, 2f), 0, default, 1.2f);
				dust.noGravity = true;
			}
		}


 public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
     => target.AddBuff(ModContent.BuffType<ApatheticCurse>(), 60 * 3);
	}
}