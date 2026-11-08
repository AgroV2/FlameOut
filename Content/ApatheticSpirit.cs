using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.Audio;
using Terraria.GameContent;
using Terraria.GameContent.Bestiary;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.DataStructures;
using Terraria.Localization;
using Terraria.GameContent.ItemDropRules;
namespace flameout.Content
{
	

	public class ApatheticSpirit : ModNPC
	{
		
		public const float RingRadius = ApatheticCell.OrbitRadius + 3f * 16f; 
		public const float RingThickness = 5f;
		public const float RingSpeed = 0.018f;

		
		public const float HoverDistance = 50f * 16f;   
		public const float HoverHeight = 60f;
		public const float OverPlayerHeight = 320f;     
		public const int SideSwitchInterval = 420;      
		public const float HoverSpeed = 13f;
		public const float HoverAccel = 0.22f;

		
		public const int AttackDelay = 180;             
		public const int Phase2AttackDelay = 120;
		public const int RapiersPerType = 2;
		public const int RapierTelegraph = 32;
		public const int RapierRecover = 26;
		public const float SlashDashSpeed = 22f;
		public const float ThrustDashSpeed = 30f;
		public const float DashOvershoot = 20f * 16f;   
		public const float BowOrbitRadius = 55f * 12f;
		

		public const int BowApproachTime = 60;
		public const int BowApproachTimeP2 = 45;      

		public const int BowOrbitTime = 100;
		public const int BowOrbitTimeP2 = 90;         

		public const int BowPauseTime = 100;
		public const int BowPauseTimeP2 = 55;         

		public const int BowArrowCount = 15;
		public const int BowArrowCountP2 = 24;        

		public const float BowArrowSpeed = 16f;
		public const float BowArrowSpeedP2 = 21f;

		

		public const float StaffTeleportDistance = 30f * 16f;
		public const float StaffTeleportDistanceP2 = 38f * 16f;

		public const int StaffStepTime = 30;
		public const int StaffStepTimeP2 = 22;        

		public const float StaffParticleSpeed = 12f;
		public const float StaffParticleSpeedP2 = 16f;

		public const float StaffSpreadDegrees = 12f;
		public const float StaffSpreadDegreesP2 = 18f;

		public const int StaffProjectiles = 3;
		public const int StaffProjectilesP2 = 5;      

		

		public const int CellCount = 5;
		public const int CellCountP2 = 7;             

		public const int CellSpawnDelay = 60;
		public const int CellSpawnDelayP2 = 45;

		public const int CellBeamTime = 300;
		public const int CellBeamTimeP2 = 390;        

		public const float CellHoverDistance = 200f;
		public const float CellHoverDistanceP2 = 150f; 

		

		public int CurrentBowApproachTime => phaseTwo ? BowApproachTimeP2 : BowApproachTime;
		public int CurrentBowOrbitTime => phaseTwo ? BowOrbitTimeP2 : BowOrbitTime;
		public int CurrentBowPauseTime => phaseTwo ? BowPauseTimeP2 : BowPauseTime;
		public int CurrentBowArrowCount => phaseTwo ? BowArrowCountP2 : BowArrowCount;
		public float CurrentBowArrowSpeed => phaseTwo ? BowArrowSpeedP2 : BowArrowSpeed;

		public float CurrentStaffTeleportDistance => phaseTwo ? StaffTeleportDistanceP2 : StaffTeleportDistance;
		public int CurrentStaffStepTime => phaseTwo ? StaffStepTimeP2 : StaffStepTime;
		public float CurrentStaffParticleSpeed => phaseTwo ? StaffParticleSpeedP2 : StaffParticleSpeed;
		public float CurrentStaffSpreadDegrees => phaseTwo ? StaffSpreadDegreesP2 : StaffSpreadDegrees;
		public int CurrentStaffProjectiles => phaseTwo ? StaffProjectilesP2 : StaffProjectiles;

		public int CurrentCellCount => phaseTwo ? CellCountP2 : CellCount;
		public int CurrentCellSpawnDelay => phaseTwo ? CellSpawnDelayP2 : CellSpawnDelay;
		public int CurrentCellBeamTime => phaseTwo ? CellBeamTimeP2 : CellBeamTime;
		public float CurrentCellHoverDistance => phaseTwo ? CellHoverDistanceP2 : CellHoverDistance;
		

	   
		public const int FinalRapierCount = 5;
			public const float FinalArenaRadius = 100f * 16f;
		public const float FinalRapierSpinSpeed = 0.018f;   
		public const int FinalRapierHopSteps = 4;
		public const int FinalRapierHopDelay = 45;
		public const int FinalRapierHopTravel = 12;    

		public const int FinalArrowInterval = 300;          
		public const int FinalProjectileInterval = 120;     
		public const float FinalArrowSpeed = 16f;
		public const float FinalProjectileSpeed = 13f;

		private void ApatheticSpiritFinalPhase(Player player)
		{
			NPC.ai[1]++;

			
			MoveTowards(arenaCenter, 9f, 0.1f);

			NPC.direction = player.Center.X >= NPC.Center.X ? 1 : -1;
			NPC.spriteDirection = NPC.direction;

			
			if (NPC.ai[2] == 0f)
			{
				if (Main.netMode != NetmodeID.MultiplayerClient)
				{
					for (int slot = 0; slot < FinalRapierCount; slot++)
					{
						Projectile.NewProjectile(NPC.GetSource_FromAI(), NPC.Center, Vector2.Zero,
							ModContent.ProjectileType<ApatheticSpiritRapierFinal>(),
							52, 0f, Main.myPlayer, NPC.whoAmI, slot);
					}
				}

				SoundEngine.PlaySound(SoundID.Item71, NPC.Center);

				NPC.ai[2] = 1f;
				NPC.netUpdate = true;
			}

			int timer = (int)NPC.ai[1];

			
			if (timer % FinalArrowInterval == 0 && Main.netMode != NetmodeID.MultiplayerClient)
			{
				for (int i = 0; i < 8; i++)
				{
					Vector2 velocity = (MathHelper.TwoPi * i / 8f).ToRotationVector2() * FinalArrowSpeed;

					Projectile.NewProjectile(
						NPC.GetSource_FromAI(),
						NPC.Center,
						velocity,
						ModContent.ProjectileType<ApatheticSpiritArrow>(),
						42,
						0f,
						Main.myPlayer);
				}

				if (!Main.dedServ)
					SoundEngine.PlaySound(SoundID.Item5, NPC.Center);
			}

			
			if (timer % FinalProjectileInterval == 0 && Main.netMode != NetmodeID.MultiplayerClient)
			{
				Vector2 velocity = (player.Center - NPC.Center)
					.SafeNormalize(Vector2.UnitY) * FinalProjectileSpeed;

				Projectile.NewProjectile(
					NPC.GetSource_FromAI(),
					NPC.Center,
					velocity,
					ModContent.ProjectileType<ApatheticSpiritProjectile>(),
					48,
					0f,
					Main.myPlayer);

				if (!Main.dedServ)
					SoundEngine.PlaySound(SoundID.Item72, NPC.Center);
			}

			
			if (Main.netMode != NetmodeID.MultiplayerClient && timer % 60 == 0)
			{
				int rapierType = ModContent.ProjectileType<ApatheticSpiritRapierFinal>();
				int alive = 0;

				for (int i = 0; i < Main.maxProjectiles; i++)
				{
					Projectile proj = Main.projectile[i];

					if (proj.active && proj.type == rapierType && (int)proj.ai[0] == NPC.whoAmI)
						alive++;
				}

				for (int slot = alive; slot < FinalRapierCount; slot++)
				{
					Projectile.NewProjectile(NPC.GetSource_FromAI(), NPC.Center, Vector2.Zero,
						rapierType, 52, 0f, Main.myPlayer, NPC.whoAmI, slot);
				}
			}
		}

		private void KillFinalMinions()
		{
			int rapierType = ModContent.ProjectileType<ApatheticSpiritRapierFinal>();
			int beamType = ModContent.ProjectileType<ApatheticSpiritFinalBeam>();
			int cellType = ModContent.ProjectileType<ApatheticSpiritCell>();

			for (int i = 0; i < Main.maxProjectiles; i++)
			{
				Projectile proj = Main.projectile[i];

				if (proj.active && (proj.type == rapierType || proj.type == beamType || proj.type == cellType))
					proj.Kill();
			}
		}
		
				
		public static ApatheticSpirit Get(NPC npc) => npc.ModNPC as ApatheticSpirit;

		public static bool IsPhaseTwo(NPC npc) => Get(npc)?.PhaseTwo ?? false;
			private void ApatheticSpiritCellAttack(Player player)
		{
			
			int cellCount = phaseTwo ? 7 : CellCount;
			int spawnDelay = phaseTwo ? 45 : CellSpawnDelay;
			int beamDuration = phaseTwo ? 390 : CellBeamTime;
			float hoverDistance = phaseTwo ? 150f : CellHoverDistance;

			NPC.ai[1]++;

			if (NPC.ai[2] == 0f)
			{
				
				Vector2 hover = player.Center
					+ new Vector2(NPC.direction * -hoverDistance, -hoverDistance * 0.6f);
				MoveTowards(hover, 12f, 0.12f);

				if (Main.netMode != NetmodeID.MultiplayerClient)
				{
					for (int slot = 0; slot < cellCount; slot++)
					{
						Projectile.NewProjectile(NPC.GetSource_FromAI(), NPC.Center, Vector2.Zero,
							ModContent.ProjectileType<ApatheticSpiritCell>(),
							phaseTwo ? 44 : 32, 0f, Main.myPlayer, NPC.whoAmI, slot);
					}
				}

				SoundEngine.PlaySound(SoundID.Item29, NPC.Center);

				NPC.ai[2] = 1f;
				NPC.ai[1] = 0f;
				NPC.netUpdate = true;
				return;
			}

			
			NPC.velocity = Vector2.Lerp(NPC.velocity, Vector2.Zero, 0.2f);

			bool cellsAlive = false;
			int cellType = ModContent.ProjectileType<ApatheticSpiritCell>();

			for (int i = 0; i < Main.maxProjectiles; i++)
			{
				Projectile proj = Main.projectile[i];

				if (proj.active && proj.type == cellType && (int)proj.ai[0] == NPC.whoAmI)
				{
					cellsAlive = true;
					break;
				}
			}

			if (!cellsAlive || NPC.ai[1] > spawnDelay + beamDuration + 120)
				EndAttack();
		}

		
		public const int PhaseTransitionTime = 150;     

			private enum AIState
		{
			Hover = 0,
			RapierAttack = 1,
			BowAttack = 2,
			StaffAttack = 3,
			CellsAttack = 4,
			PhaseTransition = 5,
			FinalPhase = 6
		}
			public override void ModifyNPCLoot(NPCLoot npcLoot)
		{
			
			npcLoot.Add(ItemDropRule.BossBag(ModContent.ItemType<TreasureBagApatheticSpirit>()));
		}

		public override void BossLoot(ref string name, ref int potionType)
		{
			potionType = ItemID.GreaterHealingPotion;
		}
		
		private static readonly int[] AttackPattern =
{
	(int)AIState.RapierAttack,
	(int)AIState.BowAttack,
	(int)AIState.StaffAttack,
    (int)AIState.CellsAttack,
};

		
		

		private float dashLength;
		private float dashTravel;
		private int attackIndex;
		private bool phaseTwo;
		private bool transitionDone;
		private bool finalPhase;
		private Vector2 arenaCenter;

	public bool FinalPhaseActive => finalPhase;
		public Vector2 ArenaCenter => arenaCenter;
		public bool PhaseTwo => phaseTwo;

		public override void SetStaticDefaults()
		{
			Main.npcFrameCount[Type] = 4;                       
			NPCID.Sets.MPAllowedEnemies[Type] = true;
			NPCID.Sets.BossBestiaryPriority.Add(Type);
			NPCID.Sets.TrailCacheLength[Type] = 8;
			NPCID.Sets.TrailingMode[Type] = 3;
		}

		public override void SetDefaults()
		{
			NPC.width = 42;
			NPC.height = 50;
			NPC.damage = 40;
			NPC.defense = 22;
			NPC.lifeMax = 7500;
			NPC.HitSound = SoundID.NPCHit1;
			NPC.DeathSound = SoundID.NPCDeath6;
			NPC.knockBackResist = 0f;
			NPC.noGravity = true;
			NPC.noTileCollide = true;       
			NPC.boss = true;
			NPC.npcSlots = 10f;
			NPC.lavaImmune = true;
			NPC.value = Item.buyPrice(gold: 12);
			NPC.netAlways = true;         
			NPC.SpawnWithHigherTime(30);
			Music = MusicLoader.GetMusicSlot(Mod, "Content/Music/ApatheticSpirit");
		}

		
		public override bool? DrawHealthBar(byte hbPosition, ref float scale, ref Vector2 position)
		{
			scale = 1.2f;
			return true;
		}

		public override void SetBestiary(BestiaryDatabase database, BestiaryEntry bestiaryEntry)
		{
			bestiaryEntry.Info.Add(new FlavorTextBestiaryInfoElement("Mods.flameout.Bestiary.ApatheticSpirit"));
		}

		
		public override float SpawnChance(NPCSpawnInfo spawnInfo)
		{
			if (!ApatheticMirrorSubworld.Active)
				return 0f;

			if (NPC.AnyNPCs(Type))
				return 0f;

			return 0.0006f;
		}

	

		

		public override void AI()
		{
			if (NPC.target < 0 || NPC.target == 255 || Main.player[NPC.target].dead || !Main.player[NPC.target].active)
				NPC.TargetClosest();

			Player player = Main.player[NPC.target];

			if (player.dead || !player.active)
			{
				NPC.velocity.Y -= 0.4f;
				NPC.EncourageDespawn(30);
				return;
			}
						
			if (!finalPhase && NPC.life <= NPC.lifeMax / 10)
			{
				finalPhase = true;
				phaseTwo = true;
				transitionDone = true;

				arenaCenter = NPC.Center;

				KillAllRapiers();
				KillFinalMinions();

				NPC.ai[0] = (float)AIState.FinalPhase;
				NPC.ai[1] = 0f;
				NPC.ai[2] = 0f;
				NPC.ai[3] = 0f;
				NPC.netUpdate = true;

				SoundEngine.PlaySound(SoundID.Roar with { Pitch = -0.4f }, NPC.Center);

				for (int i = 0; i < 60; i++)
				{
					Dust dust = Dust.NewDustPerfect(NPC.Center, DustID.PurpleTorch,
						Main.rand.NextVector2Circular(14f, 14f), 0, default, 2.2f);
					dust.noGravity = true;
				}
			}

			
			if (!phaseTwo && !transitionDone && NPC.life < NPC.lifeMax / 2)
			{
				transitionDone = true;
				KillAllRapiers();

				NPC.ai[0] = (float)AIState.PhaseTransition;
				NPC.ai[1] = 0f;
				NPC.ai[2] = 0f;
				NPC.ai[3] = 0f;
				NPC.netUpdate = true;
			}

			
			NPC.direction = player.Center.X > NPC.Center.X ? 1 : -1;
			NPC.spriteDirection = NPC.direction;

			switch ((AIState)(int)NPC.ai[0])
			{
				case AIState.Hover:
					HoverAI(player);
					break;
				case AIState.RapierAttack:
					RapierAttackAI(player);
					break;
					case AIState.BowAttack:
	ApatheticSpiritBowAttack(player);
	break;

case AIState.StaffAttack:
	ApatheticSpiritStaffAttack(player);
	break;
					case AIState.CellsAttack:
					ApatheticSpiritCellAttack(player);
					break;
									case AIState.FinalPhase:
					ApatheticSpiritFinalPhase(player);
					break;
				case AIState.PhaseTransition:
					PhaseTransitionAI(player);
					break;
				default:
					
					EndAttack();
					break;
			}

			Lighting.AddLight(NPC.Center, new Vector3(0.55f, 0.2f, 0.75f));
		}

		



private static readonly SoundStyle StaffTeleportSound =
	new SoundStyle("flameout/Content/Sounds/em1")
	{
		Volume = 0.85f
	};











		private void ApatheticSpiritBowAttack(Player player)
		{
			
			int approachTime = phaseTwo ? 45 : BowApproachTime;
			int orbitTime = phaseTwo ? 90 : BowOrbitTime;
			int pauseTime = phaseTwo ? 55 : BowPauseTime;
			int arrowCount = phaseTwo ? 20 : BowArrowCount;
			float arrowSpeed = phaseTwo ? 21f : BowArrowSpeed;

			int stage = (int)NPC.ai[2];

			if (stage == 0)
			{
				NPC.velocity *= 0.9f;

				if (Main.netMode == NetmodeID.MultiplayerClient)
					return;

				SpawnOrbitWeapons(ModContent.ProjectileType<ApatheticSpiritBow>());

				NPC.ai[3] = (NPC.Center - player.Center)
					.SafeNormalize(-Vector2.UnitX)
					.ToRotation();

				NPC.ai[1] = 0f;
				NPC.ai[2] = 1f;
				NPC.netUpdate = true;
				return;
			}

			float startingAngle = NPC.ai[3];

			
			if (stage == 1)
			{
				Vector2 destination = player.Center
					+ startingAngle.ToRotationVector2() * BowOrbitRadius;

				float remainingTicks = Math.Max(1f, approachTime - NPC.ai[1]);

				NPC.velocity = Vector2.Zero;
				NPC.Center = Vector2.Lerp(NPC.Center, destination, 1f / remainingTicks);

				NPC.ai[1]++;

				if (NPC.ai[1] >= approachTime && Main.netMode != NetmodeID.MultiplayerClient)
				{
					NPC.ai[1] = 0f;
					NPC.ai[2] = 2f;
					NPC.netUpdate = true;
				}

				return;
			}

			bool orbiting = stage % 2 == 0;

			
			if (!orbiting)
			{
				NPC.velocity = Vector2.Zero;
				NPC.Center = player.Center + startingAngle.ToRotationVector2() * BowOrbitRadius;

				NPC.ai[1]++;

				if (NPC.ai[1] >= pauseTime && Main.netMode != NetmodeID.MultiplayerClient)
				{
					NPC.ai[1] = 0f;
					NPC.ai[2]++;
					NPC.netUpdate = true;
				}

				return;
			}

			int timer = (int)NPC.ai[1];
			int bowIndex = (stage - 2) / 2;

			float progress = MathHelper.Clamp((timer + 1f) / orbitTime, 0f, 1f);
			float angle = startingAngle + MathHelper.TwoPi * progress;

			NPC.velocity = Vector2.Zero;
			NPC.Center = player.Center + angle.ToRotationVector2() * BowOrbitRadius;

			
			bool fire = timer < orbitTime
				&& (timer == 0
					|| timer * arrowCount / orbitTime != (timer - 1) * arrowCount / orbitTime);

			if (fire)
			{
				if (!Main.dedServ)
					SoundEngine.PlaySound(SoundID.Item5, NPC.Center);

				if (Main.netMode != NetmodeID.MultiplayerClient)
				{
					int bowType = ModContent.ProjectileType<ApatheticSpiritBow>();
					Projectile bow = FindOrbitWeapon(bowType, bowIndex);

					Vector2 origin = bow != null
						? ApatheticSpiritOrbitWeapon.GetOrbitPosition(bow, NPC)
						: NPC.Center;

					Vector2 velocity = (player.Center - origin)
						.SafeNormalize(Vector2.UnitY) * arrowSpeed;

					Projectile.NewProjectile(
						NPC.GetSource_FromAI(),
						origin,
						velocity,
						ModContent.ProjectileType<ApatheticSpiritArrow>(),
						phaseTwo ? 42 : 30,
						0f,
						Main.myPlayer);
				}
			}

			NPC.ai[1]++;

			if (NPC.ai[1] >= orbitTime && Main.netMode != NetmodeID.MultiplayerClient)
			{
				ConsumeOrbitWeapon(ModContent.ProjectileType<ApatheticSpiritBow>(), bowIndex);

				if (bowIndex >= 3)
				{
					EndAttack();
				}
				else
				{
					NPC.ai[1] = 0f;
					NPC.ai[2]++;
					NPC.netUpdate = true;
				}
			}
		}






		private void ApatheticSpiritStaffAttack(Player player)
		{
			
			float teleportDistance = phaseTwo ? 38f * 16f : StaffTeleportDistance;
			int stepTime = phaseTwo ? 22 : StaffStepTime;
			float particleSpeed = phaseTwo ? 16f : StaffParticleSpeed;
			float spread = phaseTwo ? 18f : StaffSpreadDegrees;
			int shots = phaseTwo ? 5 : 3;

			NPC.velocity = Vector2.Zero;

			if (NPC.ai[2] == 0f)
			{
				if (Main.netMode == NetmodeID.MultiplayerClient)
					return;

				SpawnOrbitWeapons(ModContent.ProjectileType<ApatheticSpiritStaff>());

				NPC.ai[1] = 0f;
				NPC.ai[2] = 1f;
				NPC.netUpdate = true;
				return;
			}

			int timer = (int)NPC.ai[1];

			
			if (timer < stepTime * 4 && timer % stepTime == 0)
			{
				int step = timer / stepTime;

				Vector2 offset;

				switch (step)
				{
					case 0:
						offset = new Vector2(-teleportDistance, 0f);
						break;

					case 1:
						offset = new Vector2(0f, -teleportDistance);
						break;

					case 2:
						offset = new Vector2(0f, teleportDistance);
						break;

					default:
						offset = new Vector2(teleportDistance, 0f);
						break;
				}

				Vector2 oldCenter = NPC.Center;

				NPC.Center = player.Center + offset;

				NPC.direction = player.Center.X >= NPC.Center.X ? 1 : -1;
				NPC.spriteDirection = NPC.direction;

				if (!Main.dedServ)
				{
					StaffTeleportDust(oldCenter);
					StaffTeleportDust(NPC.Center);
					SoundEngine.PlaySound(StaffTeleportSound, NPC.Center);
				}

				if (Main.netMode != NetmodeID.MultiplayerClient)
				{
					int staffType = ModContent.ProjectileType<ApatheticSpiritStaff>();
					Projectile staff = FindOrbitWeapon(staffType, step);

					Vector2 origin = staff != null
						? ApatheticSpiritOrbitWeapon.GetOrbitPosition(staff, NPC)
						: NPC.Center;

					Vector2 direction = (player.Center - origin).SafeNormalize(Vector2.UnitY);

					for (int i = 0; i < shots; i++)
					{
						float offsetDegrees = shots > 1
							? MathHelper.Lerp(-spread, spread, i / (float)(shots - 1))
							: 0f;

						Vector2 velocity = direction
							.RotatedBy(MathHelper.ToRadians(offsetDegrees)) * particleSpeed;

						Projectile.NewProjectile(
							NPC.GetSource_FromAI(),
							origin,
							velocity,
							ModContent.ProjectileType<ApatheticSpiritParticle>(),
							phaseTwo ? 46 : 34,
							0f,
							Main.myPlayer);
					}

					ConsumeOrbitWeapon(staffType, step);
					NPC.netUpdate = true;
				}
			}

			NPC.ai[1]++;

			if (NPC.ai[1] >= stepTime * 4 && Main.netMode != NetmodeID.MultiplayerClient)
				EndAttack();
		}

private static void StaffTeleportDust(Vector2 center)
{
	for (int i = 0; i < 24; i++)
	{
		Dust dust = Dust.NewDustPerfect(
			center,
			DustID.PurpleTorch,
			Main.rand.NextVector2Circular(6f, 6f),
			0,
			default,
			1.7f);

		dust.noGravity = true;
	}
}



private void SpawnOrbitWeapons(int type)
{
	for (int slot = 0; slot < 4; slot++)
	{
		Projectile.NewProjectile(
			NPC.GetSource_FromAI(),
			NPC.Center,
			Vector2.Zero,
			type,
			0,
			0f,
			Main.myPlayer,
			NPC.whoAmI, 
			slot);      
	}
}

private Projectile FindOrbitWeapon(int type, int slot)
{
	for (int i = 0; i < Main.maxProjectiles; i++)
	{
		Projectile projectile = Main.projectile[i];

		if (projectile.active
			&& projectile.type == type
			&& (int)projectile.ai[0] == NPC.whoAmI
			&& (int)projectile.ai[1] == slot)
		{
			return projectile;
		}
	}

	return null;
}

private void ConsumeOrbitWeapon(int type, int slot)
{
	Projectile projectile = FindOrbitWeapon(type, slot);

	if (projectile != null)
		projectile.Kill();
}
		private void HoverAI(Player player)
		{
			NPC.ai[1]++;

			if (NPC.localAI[0] == 0f)
				NPC.localAI[0] = -1f; 

			float side = NPC.localAI[0];
			NPC.localAI[1]++;

			bool switching = NPC.localAI[2] == 1f;

			if (!switching && NPC.localAI[1] >= SideSwitchInterval)
			{
				NPC.localAI[2] = 1f;
				switching = true;
			}

			Vector2 target;

			if (switching)
			{
				
				target = player.Center + new Vector2(side * 220f, -OverPlayerHeight);

				bool aboveEnough = NPC.Center.Y < player.Center.Y - OverPlayerHeight * 0.75f;
				bool closeX = Math.Abs(NPC.Center.X - player.Center.X) < 320f;

				if (aboveEnough && closeX)
				{
					NPC.localAI[0] = -side;
					NPC.localAI[1] = 0f;
					NPC.localAI[2] = 0f;
					NPC.netUpdate = true;
				}
			}
			else
			{
				target = player.Center + new Vector2(side * HoverDistance, -HoverHeight);
			}

			MoveTowards(target, HoverSpeed, HoverAccel);

			int delay = phaseTwo ? Phase2AttackDelay : AttackDelay;

			if (NPC.ai[1] >= delay)
				StartNextAttack();
		}

		private void StartNextAttack()
		{
			int next = AttackPattern[attackIndex % AttackPattern.Length];
			attackIndex++;

			NPC.ai[0] = next;
			NPC.ai[1] = 0f;
			NPC.ai[2] = 0f;
			NPC.ai[3] = 0f;
			NPC.netUpdate = true;
		}

		private void EndAttack()
		{
			NPC.ai[0] = (float)AIState.Hover;
			NPC.ai[1] = 0f;
			NPC.ai[2] = 0f;
			NPC.ai[3] = 0f;
			NPC.netUpdate = true;
		}

		
		private void PhaseTransitionAI(Player player)
		{
			NPC.ai[1]++;
			NPC.dontTakeDamage = true;

			
			Vector2 target = player.Center - new Vector2(0f, OverPlayerHeight);
			MoveTowards(target, 9f, 0.15f);
			NPC.velocity *= 0.96f;

			if (NPC.ai[1] == 1f)
				SoundEngine.PlaySound(SoundID.Roar with { Pitch = -0.3f }, NPC.Center);

			
			float progress = NPC.ai[1] / PhaseTransitionTime;

			for (int i = 0; i < 4; i++)
			{
				float angle = Main.rand.NextFloat(MathHelper.TwoPi);
				float radius = MathHelper.Lerp(RingRadius * 2.4f, RingRadius, progress);

				Dust dust = Dust.NewDustPerfect(NPC.Center + angle.ToRotationVector2() * radius,
					DustID.PurpleTorch, -angle.ToRotationVector2() * 4f, 0, default, 1.6f);
				dust.noGravity = true;
			}

			

			if (NPC.ai[1] >= PhaseTransitionTime)
			{
				phaseTwo = true;
				NPC.dontTakeDamage = false;

				SoundEngine.PlaySound(SoundID.Item74, NPC.Center);

				for (int i = 0; i < 40; i++)
				{
					Dust dust = Dust.NewDustPerfect(NPC.Center, DustID.PurpleTorch,
						Main.rand.NextVector2Circular(9f, 9f), 0, default, 2f);
					dust.noGravity = true;
				}

				EndAttack();
			}
		}

		
		private void RapierAttackAI(Player player)
		{
			NPC.ai[1]++;

			switch ((int)NPC.ai[2])
			{
				
				case 0:
				{
					if (Main.netMode != NetmodeID.MultiplayerClient)
					{
						int perType = phaseTwo ? RapiersPerType + 1 : RapiersPerType;

						for (int i = 0; i < perType; i++)
						{
							SpawnRapier(ModContent.ProjectileType<ApatheticSpiritRapierSlashProjectile>());
							SpawnRapier(ModContent.ProjectileType<ApatheticSpiritRapierDashProjectile>());
						}
					}

					SoundEngine.PlaySound(SoundID.Item71, NPC.Center);

					NPC.ai[2] = 1f;
					NPC.ai[1] = 0f;
					NPC.ai[3] = 0f; 
					break;
				}

				
				case 1:
				{
					Vector2 hover = player.Center + new Vector2(NPC.direction * -260f, -140f);
					MoveTowards(hover, 11f, 0.3f);

					if (NPC.ai[1] < RapierTelegraph)
						break;

					bool isSlash = NPC.ai[3] == 0f;

					int rapierType = isSlash
						? ModContent.ProjectileType<ApatheticSpiritRapierSlashProjectile>()
						: ModContent.ProjectileType<ApatheticSpiritRapierDashProjectile>();

					if (ApatheticRapier.CountOrbiting(NPC.whoAmI, rapierType) == 0)
					{
						int otherType = isSlash
							? ModContent.ProjectileType<ApatheticSpiritRapierDashProjectile>()
							: ModContent.ProjectileType<ApatheticSpiritRapierSlashProjectile>();

						if (ApatheticRapier.CountOrbiting(NPC.whoAmI, otherType) == 0)
						{
							EndAttack();
							return;
						}

						NPC.ai[3] = isSlash ? 1f : 0f;
						NPC.ai[1] = 0f;
						break;
					}

					Vector2 dir = (player.Center - NPC.Center).SafeNormalize(Vector2.UnitX * NPC.direction);
					float speed = (isSlash ? SlashDashSpeed : ThrustDashSpeed) * (phaseTwo ? 1.25f : 1f);

					dashLength = Vector2.Distance(NPC.Center, player.Center) + DashOvershoot;
					dashTravel = 0f;
					NPC.velocity = dir * speed;

					ApatheticRapier.Activate(NPC.whoAmI, rapierType);

					SoundEngine.PlaySound(isSlash ? SoundID.Item1 : SoundID.Item8, NPC.Center);

					NPC.ai[2] = 2f;
					NPC.ai[1] = 0f;
					NPC.netUpdate = true;
					break;
				}

				
				case 2:
				{
					dashTravel += NPC.velocity.Length();

					if (dashTravel >= dashLength || NPC.ai[1] > 90f)
					{
						NPC.ai[2] = 3f;
						NPC.ai[1] = 0f;
					}
					break;
				}

				
				case 3:
				{
					NPC.velocity *= 0.9f;

					int recover = phaseTwo ? RapierRecover - 8 : RapierRecover;

					if (NPC.ai[1] >= recover)
					{
						NPC.ai[3] = NPC.ai[3] == 0f ? 1f : 0f;

						if (ApatheticRapier.CountOrbiting(NPC.whoAmI) == 0)
							EndAttack();
						else
						{
							NPC.ai[2] = 1f;
							NPC.ai[1] = 0f;
						}
					}
					break;
				}
			}
		}

		private void SpawnRapier(int type)
		{
			Projectile.NewProjectile(NPC.GetSource_FromAI(), NPC.Center, Vector2.Zero,
				type, RapierDamage(), 0f, Main.myPlayer, 0f, NPC.whoAmI);
		}

		private void KillAllRapiers()
		{
			for (int i = 0; i < Main.maxProjectiles; i++)
			{
				Projectile proj = Main.projectile[i];

				if (proj.active && ApatheticRapier.IsRapier(proj) && (int)proj.ai[1] == NPC.whoAmI)
					proj.Kill();
			}
		}

		private int RapierDamage() => phaseTwo ? 42 : 30;

		private void MoveTowards(Vector2 target, float speed, float accel)
		{
			Vector2 desired = (target - NPC.Center).SafeNormalize(Vector2.Zero) * speed;
			NPC.velocity = Vector2.Lerp(NPC.velocity, desired, accel);
		}

		

		public override void SendExtraAI(System.IO.BinaryWriter writer)
		{
			writer.Write(phaseTwo);
			writer.Write(transitionDone);
			writer.Write(attackIndex);
			writer.Write(dashLength);
			writer.Write(dashTravel);
						writer.Write(finalPhase);
			writer.WriteVector2(arenaCenter);
		}

		public override void ReceiveExtraAI(System.IO.BinaryReader reader)
		{
			phaseTwo = reader.ReadBoolean();
			transitionDone = reader.ReadBoolean();
			attackIndex = reader.ReadInt32();
			dashLength = reader.ReadSingle();
			dashTravel = reader.ReadSingle();
						finalPhase = reader.ReadBoolean();
			arenaCenter = reader.ReadVector2();
		}

		

		public override void FindFrame(int frameHeight)
		{
			NPC.frameCounter += 0.18f;
			NPC.frameCounter %= Main.npcFrameCount[Type];

			NPC.frame.Y = (int)NPC.frameCounter * frameHeight;
		}

		

		public override void PostDraw(SpriteBatch spriteBatch, Vector2 screenPos, Color drawColor)
		{
						if (finalPhase)
				DrawArena(arenaCenter);
			ApatheticRapier.DrawRing(spriteBatch, NPC.Center - screenPos, RingRadius,
				RingThickness, phaseTwo ? new Color(220, 60, 255) : new Color(165, 70, 255));
		}
				private static void DrawArena(Vector2 center)
		{
			Texture2D pixel = TextureAssets.MagicPixel.Value;

			float pulse = 0.75f + 0.25f * (float)Math.Sin(Main.GlobalTimeWrappedHourly * 2.5f);
			Color color = new Color(200, 70, 255) * (0.75f * pulse);
			color.A = 0;

			const int segments = 240;

			for (int i = 0; i < segments; i++)
			{
				float angle = MathHelper.TwoPi * i / segments;
				Vector2 pos = center + angle.ToRotationVector2() * ApatheticSpirit.FinalArenaRadius;

				Main.spriteBatch.Draw(pixel, pos - Main.screenPosition,
					new Rectangle(0, 0, 1, 1), color, angle + MathHelper.PiOver2,
					new Vector2(0.5f), new Vector2(8f, 60f), SpriteEffects.None, 0f);
			}
		}
	}

	

	public static class ApatheticRapier
	{
		public static bool IsRapier(Projectile projectile)
			=> projectile.type == ModContent.ProjectileType<ApatheticSpiritRapierSlashProjectile>()
			|| projectile.type == ModContent.ProjectileType<ApatheticSpiritRapierDashProjectile>();

		
		public static void GetOrder(Projectile projectile, out int index, out int count)
		{
			index = 0;
			count = 0;

			for (int i = 0; i < Main.maxProjectiles; i++)
			{
				Projectile other = Main.projectile[i];

				if (!other.active || !IsRapier(other))
					continue;

				if ((int)other.ai[1] != (int)projectile.ai[1] || other.ai[0] != 0f)
					continue;

				if (i < projectile.whoAmI)
					index++;

				count++;
			}

			if (count < 1)
				count = 1;
		}

		public static int CountOrbiting(int bossIndex, int type = -1)
		{
			int count = 0;

			for (int i = 0; i < Main.maxProjectiles; i++)
			{
				Projectile other = Main.projectile[i];

				if (!other.active || !IsRapier(other))
					continue;

				if ((int)other.ai[1] != bossIndex || other.ai[0] != 0f)
					continue;

				if (type >= 0 && other.type != type)
					continue;

				count++;
			}

			return count;
		}

		
		public static void Activate(int bossIndex, int type)
		{
			for (int i = 0; i < Main.maxProjectiles; i++)
			{
				Projectile other = Main.projectile[i];

				if (!other.active || other.type != type)
					continue;

				if ((int)other.ai[1] != bossIndex || other.ai[0] != 0f)
					continue;

				other.ai[0] = 1f;
				other.timeLeft = 120;
				other.netUpdate = true;
				return;
			}
		}

		
		public static float AngleToTarget(NPC boss, Vector2 from)
		{
			Player target = Main.player[boss.target];

			if (!target.active || target.dead)
				return (boss.Center - from).ToRotation();

			return (target.Center - from).ToRotation();
		}

		public static void DrawRing(SpriteBatch spriteBatch, Vector2 screenCenter, float radius,
			float thickness, Color baseColor)
		{
			Texture2D pixel = TextureAssets.MagicPixel.Value;
			Rectangle source = new Rectangle(0, 0, 1, 1);

			const int Segments = 120;
			float step = MathHelper.TwoPi / Segments;

			float pulse = 0.75f + 0.25f * (float)Math.Sin(Main.GlobalTimeWrappedHourly * 3f);
			Color color = baseColor * 0.6f * pulse;

			for (int i = 0; i < Segments; i++)
			{
				float a = i * step;
				Vector2 p1 = screenCenter + a.ToRotationVector2() * radius;
				Vector2 p2 = screenCenter + (a + step).ToRotationVector2() * radius;
				Vector2 diff = p2 - p1;

				spriteBatch.Draw(pixel, p1, source, color, diff.ToRotation(), Vector2.Zero,
					new Vector2(diff.Length() + 1f, thickness), SpriteEffects.None, 0f);
			}
		}
	}

	

	public class ApatheticSpiritRapierSlashProjectile : ModProjectile
	{
		public override string Texture => "flameout/Content/ApatheticRapierSlash";

		public const int SlashDuration = 26;
		public const float SwingArc = MathHelper.Pi * 0.85f;
		public const float SlashReach = 62f;

		
		

		public override void SetStaticDefaults()
		{
			ProjectileID.Sets.TrailCacheLength[Type] = 14;
			ProjectileID.Sets.TrailingMode[Type] = 2;
		}

		public override void SetDefaults()
		{
			Projectile.width = 44;
			Projectile.height = 44;
			Projectile.hostile = false;
			Projectile.friendly = false;
			Projectile.tileCollide = false;
			Projectile.ignoreWater = true;
			Projectile.penetrate = -1;
			Projectile.timeLeft = 60 * 60;
			Projectile.netImportant = true;
		}

		public override void AI()
		{
			NPC boss = Main.npc[(int)Projectile.ai[1]];

			if (!boss.active || boss.type != ModContent.NPCType<ApatheticSpirit>())
			{
				Projectile.Kill();
				return;
			}

			if (Projectile.ai[0] == 0f)
			{
				OrbitRing(boss);
				return;
			}

			
			if (Projectile.localAI[0] == 0f)
			{
				Projectile.localAI[1] = ApatheticRapier.AngleToTarget(boss, boss.Center);
				Projectile.hostile = true;
			}

			Projectile.localAI[0]++;

			float progress = MathHelper.Clamp(Projectile.localAI[0] / SlashDuration, 0f, 1f);
			float eased = (float)Math.Sin(progress * MathHelper.PiOver2);
			float angle = Projectile.localAI[1] - SwingArc / 2f + SwingArc * eased;

			Projectile.Center = boss.Center + angle.ToRotationVector2() * SlashReach;
			
			Projectile.rotation = ApatheticRapier.AngleToTarget(boss, Projectile.Center) + MathHelper.PiOver4;

			for (int i = 0; i < 2; i++)
			{
				Dust dust = Dust.NewDustPerfect(Projectile.Center, DustID.PurpleTorch,
					Main.rand.NextVector2Circular(1.5f, 1.5f), 0, default, 1.2f);
				dust.noGravity = true;
			}

			if (progress >= 1f)
				Projectile.Kill();
		}

		private void OrbitRing(NPC boss)
		{
			Projectile.hostile = false;
			Projectile.timeLeft = 60 * 60;

			ApatheticRapier.GetOrder(Projectile, out int index, out int count);

			float baseAngle = Main.GameUpdateCount * ApatheticSpirit.RingSpeed;
			float slotAngle = baseAngle + MathHelper.TwoPi * index / count;

			Projectile.Center = boss.Center + slotAngle.ToRotationVector2() * ApatheticSpirit.RingRadius;
			
			Projectile.rotation = ApatheticRapier.AngleToTarget(boss, Projectile.Center) + MathHelper.PiOver4;
			Projectile.velocity = Vector2.Zero;
		}

		public override bool PreDraw(ref Color lightColor)
		{
			Texture2D texture = TextureAssets.Projectile[Type].Value;
			Vector2 origin = texture.Size() / 2f;

			if (Projectile.ai[0] == 1f)
			{
				for (int i = Projectile.oldPos.Length - 1; i > 0; i--)
				{
					if (Projectile.oldPos[i] == Vector2.Zero)
						continue;

					float fade = 1f - i / (float)Projectile.oldPos.Length;
					Color trail = new Color(175, 70, 255) * fade * 0.6f;
					Vector2 pos = Projectile.oldPos[i] + Projectile.Size / 2f - Main.screenPosition;

					Main.EntitySpriteDraw(texture, pos, null, trail, Projectile.oldRot[i], origin,
						Projectile.scale * (0.6f + 0.4f * fade), SpriteEffects.None);
				}
			}

			Main.EntitySpriteDraw(texture, Projectile.Center - Main.screenPosition, null,
				Color.White, Projectile.rotation, origin, Projectile.scale, SpriteEffects.None);

			return false;
		}
	}

	

	public class ApatheticSpiritRapierDashProjectile : ModProjectile
	{
		public override string Texture => "flameout/Content/ApatheticRapierDash";

		public const float ThrustReach = 54f;

		

		public override void SetStaticDefaults()
		{
			ProjectileID.Sets.TrailCacheLength[Type] = 12;
			ProjectileID.Sets.TrailingMode[Type] = 2;
		}

		public override void SetDefaults()
		{
			Projectile.width = 40;
			Projectile.height = 40;
			Projectile.hostile = false;
			Projectile.friendly = false;
			Projectile.tileCollide = false;
			Projectile.ignoreWater = true;
			Projectile.penetrate = -1;
			Projectile.timeLeft = 60 * 60;
			Projectile.netImportant = true;
		}

		public override void AI()
		{
			NPC boss = Main.npc[(int)Projectile.ai[1]];

			if (!boss.active || boss.type != ModContent.NPCType<ApatheticSpirit>())
			{
				Projectile.Kill();
				return;
			}

			if (Projectile.ai[0] == 0f)
			{
				Projectile.hostile = false;
				Projectile.timeLeft = 60 * 60;

				ApatheticRapier.GetOrder(Projectile, out int index, out int count);

				float baseAngle = Main.GameUpdateCount * ApatheticSpirit.RingSpeed;
				float slotAngle = baseAngle + MathHelper.TwoPi * index / count;

				Projectile.Center = boss.Center + slotAngle.ToRotationVector2() * ApatheticSpirit.RingRadius;
				Projectile.rotation = ApatheticRapier.AngleToTarget(boss, Projectile.Center) + MathHelper.PiOver4;
				return;
			}

			
			Projectile.hostile = true;
			Projectile.localAI[0]++;

			float angle = boss.velocity.LengthSquared() > 1f
				? boss.velocity.ToRotation()
				: ApatheticRapier.AngleToTarget(boss, boss.Center);

			Projectile.Center = boss.Center + angle.ToRotationVector2() * ThrustReach;
			Projectile.rotation = ApatheticRapier.AngleToTarget(boss, Projectile.Center) + MathHelper.PiOver4;

			Dust dust = Dust.NewDustPerfect(Projectile.Center, DustID.PurpleTorch,
				Main.rand.NextVector2Circular(1.2f, 1.2f), 0, default, 1.1f);
			dust.noGravity = true;

			if (Projectile.localAI[0] > 12f && boss.velocity.Length() < 6f)
				Projectile.Kill();
		}

		public override bool PreDraw(ref Color lightColor)
		{
			Texture2D texture = TextureAssets.Projectile[Type].Value;
			Vector2 origin = texture.Size() / 2f;

			if (Projectile.ai[0] == 1f)
			{
				for (int i = Projectile.oldPos.Length - 1; i > 0; i--)
				{
					if (Projectile.oldPos[i] == Vector2.Zero)
						continue;

					float fade = 1f - i / (float)Projectile.oldPos.Length;
					Color trail = new Color(175, 70, 255) * fade * 0.55f;
					Vector2 pos = Projectile.oldPos[i] + Projectile.Size / 2f - Main.screenPosition;

					Main.EntitySpriteDraw(texture, pos, null, trail, Projectile.oldRot[i], origin,
						Projectile.scale * (0.6f + 0.4f * fade), SpriteEffects.None);
				}
			}

			Main.EntitySpriteDraw(texture, Projectile.Center - Main.screenPosition, null,
				Color.White, Projectile.rotation, origin, Projectile.scale, SpriteEffects.None);

			return false;
		}
	}
	

public abstract class ApatheticSpiritOrbitWeapon : ModProjectile
{
	protected abstract int RequiredBossState { get; }

	
	
	
	protected virtual float AimRotationOffset => MathHelper.PiOver2;

	public override void SetDefaults()
	{
		Projectile.width = 32;
		Projectile.height = 32;
		Projectile.friendly = false;
		Projectile.hostile = false;
		Projectile.tileCollide = false;
		Projectile.ignoreWater = true;
		Projectile.penetrate = -1;
		Projectile.timeLeft = 2;
		Projectile.netImportant = true;
	}

	public override bool? CanDamage() => false;

	public override bool ShouldUpdatePosition() => false;

	public override void AI()
	{
		int bossIndex = (int)Projectile.ai[0];

		if (bossIndex < 0 || bossIndex >= Main.maxNPCs)
		{
			Projectile.Kill();
			return;
		}

		NPC boss = Main.npc[bossIndex];

		if (!boss.active
			|| boss.type != ModContent.NPCType<ApatheticSpirit>())
		{
			Projectile.Kill();
			return;
		}

		
		if ((int)boss.ai[0] != RequiredBossState)
		{
			if (Main.netMode != NetmodeID.MultiplayerClient)
				Projectile.Kill();

			return;
		}

		Projectile.timeLeft = 2;
		Projectile.velocity = Vector2.Zero;
		Projectile.Center = GetOrbitPosition(Projectile, boss);

		if (boss.target >= 0 && boss.target < Main.maxPlayers)
		{
			Player player = Main.player[boss.target];

			Projectile.rotation =
				(player.Center - Projectile.Center).ToRotation()
				+ AimRotationOffset;
		}

		Lighting.AddLight(Projectile.Center, 0.4f, 0.1f, 0.6f);
	}

	public static Vector2 GetOrbitPosition(Projectile projectile, NPC boss)
	{
		int index = 0;
		int count = 0;

		for (int i = 0; i < Main.maxProjectiles; i++)
		{
			Projectile other = Main.projectile[i];

			if (!other.active
				|| other.type != projectile.type
				|| (int)other.ai[0] != boss.whoAmI)
			{
				continue;
			}

			if (other.ai[1] < projectile.ai[1])
				index++;

			count++;
		}

		count = Math.Max(count, 1);

		
		float angle = boss.ai[1] * ApatheticSpirit.RingSpeed
			+ MathHelper.TwoPi * index / count;

		return boss.Center
			+ angle.ToRotationVector2() * ApatheticSpirit.RingRadius;
	}

	public override Color? GetAlpha(Color lightColor) => Color.White;
}



public class ApatheticSpiritBow : ApatheticSpiritOrbitWeapon
{
	public override string Texture => "flameout/Content/ApatheticBow";

	protected override int RequiredBossState => 2;

	
	protected override float AimRotationOffset => 0f;
}



public class ApatheticSpiritStaff : ApatheticSpiritOrbitWeapon
{
	public override string Texture => "flameout/Content/ApatheticStaff";

	protected override int RequiredBossState => 3;

	
	protected override float AimRotationOffset => MathHelper.PiOver4;
}



public class ApatheticSpiritArrow : ModProjectile
{
	public override string Texture =>
		"flameout/Content/ApatheticSpiritArrow";

	public override void SetDefaults()
	{
		Projectile.width = 12;
		Projectile.height = 12;
		Projectile.friendly = false;
		Projectile.hostile = true;
		Projectile.penetrate = 1;

		
		Projectile.timeLeft = 60 * 10;
		Projectile.extraUpdates = 0;

		Projectile.tileCollide = false;
		Projectile.ignoreWater = true;

		
		Projectile.aiStyle = -1;
	}

	public override void AI()
	{
		
		Projectile.rotation =
			Projectile.velocity.ToRotation() + MathHelper.PiOver2;

		Lighting.AddLight(Projectile.Center, 0.35f, 0.08f, 0.5f);

		if (!Main.dedServ && Main.rand.NextBool(3))
		{
			Dust dust = Dust.NewDustPerfect(
				Projectile.Center,
				DustID.PurpleTorch,
				-Projectile.velocity * 0.08f,
				0,
				default,
				1f);

			dust.noGravity = true;
		}
	}

	public override Color? GetAlpha(Color lightColor) => Color.White;
}



public class ApatheticSpiritParticle : ModProjectile
{
	public override string Texture =>
		"flameout/Content/ApatheticSpiritParticle";

	public override void SetStaticDefaults()
	{
		ProjectileID.Sets.TrailCacheLength[Type] = 10;
		ProjectileID.Sets.TrailingMode[Type] = 0;
	}

	public override void SetDefaults()
	{
		Projectile.width = 18;
		Projectile.height = 18;
		Projectile.friendly = false;
		Projectile.hostile = true;
		Projectile.penetrate = 1;
		Projectile.timeLeft = 300;
		Projectile.tileCollide = false;
		Projectile.ignoreWater = true;
		Projectile.aiStyle = -1;
	}

	public override void AI()
	{
		Projectile.rotation += 0.12f;

		Lighting.AddLight(Projectile.Center, 0.55f, 0.1f, 0.7f);

		if (!Main.dedServ)
		{
			Dust dust = Dust.NewDustPerfect(
				Projectile.Center,
				DustID.PurpleTorch,
				-Projectile.velocity * 0.1f,
				0,
				default,
				1.3f);

			dust.noGravity = true;
		}
	}

	public override bool PreDraw(ref Color lightColor)
	{
		Texture2D texture = TextureAssets.Projectile[Type].Value;
		Vector2 origin = texture.Size() / 2f;

		for (int i = Projectile.oldPos.Length - 1; i >= 1; i--)
		{
			if (Projectile.oldPos[i] == Vector2.Zero)
				continue;

			float fade = 1f - i / (float)Projectile.oldPos.Length;

			Vector2 position = Projectile.oldPos[i]
				+ Projectile.Size / 2f
				- Main.screenPosition;

			Main.EntitySpriteDraw(
				texture,
				position,
				null,
				new Color(180, 65, 255) * fade * 0.6f,
				Projectile.rotation,
				origin,
				Projectile.scale * fade,
				SpriteEffects.None);
		}

		Main.EntitySpriteDraw(
			texture,
			Projectile.Center - Main.screenPosition,
			null,
			Color.White,
			Projectile.rotation,
			origin,
			Projectile.scale,
			SpriteEffects.None);

		return false;
	}
}
	

	public class ApatheticSpiritCell : ModProjectile
	{
		public override string Texture => "flameout/Content/ApatheticCell";

		public const int FrameCount = 4;      
		public const int TicksPerFrame = 6;
		public const int BurstCount = 5;
		public const float BurstSpeed = 11f;

		
		

		public override void SetStaticDefaults()
		{
			Main.projFrames[Type] = FrameCount;
			ProjectileID.Sets.TrailCacheLength[Type] = 6;
			ProjectileID.Sets.TrailingMode[Type] = 0;
		}

		public override void SetDefaults()
		{
			Projectile.width = 31;
			Projectile.height = 31;
			Projectile.friendly = false;
			Projectile.hostile = true;
			Projectile.tileCollide = false;
			Projectile.ignoreWater = true;
			Projectile.penetrate = -1;
			Projectile.timeLeft = 60 * 60;
			Projectile.netImportant = true;
		}

		public override bool ShouldUpdatePosition() => false;

			public override void AI()
		{
			NPC boss = Main.npc[(int)Projectile.ai[0]];

			if (!boss.active || boss.type != ModContent.NPCType<ApatheticSpirit>())
			{
				KillBeam();
				Projectile.Kill();
				return;
			}

			
			bool phaseTwo = ApatheticSpirit.IsPhaseTwo(boss);

			int cellCount = phaseTwo ? 7 : ApatheticSpirit.CellCount;
			int spawnDelay = phaseTwo ? 45 : ApatheticSpirit.CellSpawnDelay;
			int beamDuration = phaseTwo ? 390 : ApatheticSpirit.CellBeamTime;

			Player player = Main.player[boss.target];
			int slot = (int)Projectile.ai[1];

			
			if (++Projectile.frameCounter >= TicksPerFrame)
			{
				Projectile.frameCounter = 0;
				Projectile.frame = (Projectile.frame + 1) % Main.projFrames[Type];
			}

			Projectile.localAI[0]++;
			float t = Projectile.localAI[0];

			
			if (t <= spawnDelay)
			{
				float freeAngle = Main.GameUpdateCount * ApatheticSpirit.RingSpeed
					+ MathHelper.TwoPi * slot / cellCount;

				Vector2 freePos = boss.Center + freeAngle.ToRotationVector2() * ApatheticSpirit.RingRadius;

				float lerp = MathHelper.Clamp((t - spawnDelay * 0.6f) / (spawnDelay * 0.4f), 0f, 1f);

				Projectile.Center = Vector2.Lerp(freePos, FormationPosition(boss, slot, cellCount, 0f), lerp);
				Projectile.rotation = (player.Center - Projectile.Center).ToRotation() + MathHelper.PiOver2;

				if (t == spawnDelay && Main.netMode != NetmodeID.MultiplayerClient)
				{
					int beam = Projectile.NewProjectile(Projectile.GetSource_FromAI(),
						Projectile.Center, Vector2.UnitX,
						ModContent.ProjectileType<ApatheticSpiritCellLaser>(),
						Projectile.damage, 0f, Projectile.owner, Projectile.whoAmI);

					Projectile.localAI[1] = beam;
					Projectile.netUpdate = true;

					SoundEngine.PlaySound(SoundID.Item72, Projectile.Center);
				}

				return;
			}

			
			float beamTime = t - spawnDelay;
			float progress = MathHelper.Clamp(beamTime / beamDuration, 0f, 1f);
			float spin = MathHelper.TwoPi * progress;

			Projectile.Center = FormationPosition(boss, slot, cellCount, spin);

			float outward = SlotAngle(slot, cellCount) + spin;
			Projectile.rotation = outward + MathHelper.PiOver2;

			Lighting.AddLight(Projectile.Center, 0.5f, 0.2f, 0.8f);

			
			if (beamTime >= beamDuration)
			{
				KillBeam();

				if (Main.netMode != NetmodeID.MultiplayerClient)
				{
					int burstCount = cellCount;

					for (int i = 0; i < burstCount; i++)
					{
						float angle = outward + MathHelper.TwoPi * i / burstCount;

						Projectile.NewProjectile(Projectile.GetSource_Death(), Projectile.Center,
							angle.ToRotationVector2() * BurstSpeed,
							ModContent.ProjectileType<ApatheticSpiritCellBurst>(),
							Projectile.damage, 0f, Projectile.owner);
					}
				}

				SoundEngine.PlaySound(SoundID.Item14, Projectile.Center);

				for (int i = 0; i < 26; i++)
				{
					Dust dust = Dust.NewDustPerfect(Projectile.Center, DustID.PurpleTorch,
						Main.rand.NextVector2Circular(7f, 7f), 0, default, 1.7f);
					dust.noGravity = true;
				}

				Projectile.Kill();
			}
		}

		private static float SlotAngle(int slot, int cellCount)
			=> MathHelper.TwoPi * slot / cellCount - MathHelper.PiOver2;

		private static Vector2 FormationPosition(NPC boss, int slot, int cellCount, float spin)
			=> boss.Center + (SlotAngle(slot, cellCount) + spin).ToRotationVector2() * ApatheticSpirit.RingRadius;

		private void KillBeam()
		{
			int beamIndex = (int)Projectile.localAI[1];

			if (beamIndex <= 0 || beamIndex >= Main.maxProjectiles)
				return;

			Projectile beam = Main.projectile[beamIndex];

			if (beam.active && beam.type == ModContent.ProjectileType<ApatheticSpiritCellLaser>())
				beam.Kill();
		}

		public override bool PreDraw(ref Color lightColor)
		{
			Texture2D texture = TextureAssets.Projectile[Type].Value;

			int frameHeight = texture.Height / Main.projFrames[Type];
			Rectangle frame = new Rectangle(0, Projectile.frame * frameHeight, texture.Width, frameHeight);
			Vector2 origin = frame.Size() / 2f;

			Main.EntitySpriteDraw(texture, Projectile.Center - Main.screenPosition, frame,
				Color.White, Projectile.rotation, origin, Projectile.scale, SpriteEffects.None);

			return false;
		}
	}

	

	public class ApatheticSpiritCellLaser : ModProjectile
	{
		public const float BeamLength = 2400f;   
		public const float BeamWidth = 22f;

		

		public override void SetDefaults()
		{
			Projectile.width = 22;
			Projectile.height = 22;
			Projectile.friendly = false;
			Projectile.hostile = true;
			Projectile.tileCollide = false;
			Projectile.ignoreWater = true;
			Projectile.penetrate = -1;
			Projectile.timeLeft = 2;
			Projectile.netImportant = true;
			Projectile.usesLocalNPCImmunity = true;
			Projectile.localNPCHitCooldown = 12;
		}
public override string Texture => "flameout/Content/ApatheticLaserProjectile";
		public override bool ShouldUpdatePosition() => false;

		public override void AI()
		{
			int cellIndex = (int)Projectile.ai[0];

			if (cellIndex < 0 || cellIndex >= Main.maxProjectiles)
			{
				Projectile.Kill();
				return;
			}

			Projectile cell = Main.projectile[cellIndex];

			if (!cell.active || cell.type != ModContent.ProjectileType<ApatheticSpiritCell>())
			{
				Projectile.Kill();
				return;
			}

			Projectile.timeLeft = 2;

			
			float angle = cell.rotation - MathHelper.PiOver2;

			Projectile.Center = cell.Center;
			Projectile.velocity = angle.ToRotationVector2();
			Projectile.rotation = angle;

			Projectile.localAI[0]++;

			
			for (int i = 0; i < 12; i++)
			{
				Vector2 point = Projectile.Center + Projectile.velocity * (BeamLength / 12f * i);
				Lighting.AddLight(point, 0.4f, 0.15f, 0.75f);
			}

			if (Main.rand.NextBool())
			{
				Vector2 point = Projectile.Center + Projectile.velocity * Main.rand.NextFloat(BeamLength);

				Dust dust = Dust.NewDustPerfect(point, DustID.PurpleTorch,
					Main.rand.NextVector2Circular(1.5f, 1.5f), 0, default, 1.3f);
				dust.noGravity = true;
			}
		}

		public override bool? Colliding(Rectangle projHitbox, Rectangle targetHitbox)
		{
			float collisionPoint = 0f;
			Vector2 start = Projectile.Center;
			Vector2 end = start + Projectile.velocity * BeamLength;

			return Collision.CheckAABBvLineCollision(targetHitbox.TopLeft(), targetHitbox.Size(),
				start, end, BeamWidth, ref collisionPoint);
		}

		public override bool PreDraw(ref Color lightColor)
		{
			Texture2D pixel = TextureAssets.MagicPixel.Value;
			Rectangle source = new Rectangle(0, 0, 1, 1);

			Vector2 start = Projectile.Center - Main.screenPosition;
			float rotation = Projectile.rotation;

			
			float pulse = 0.8f + 0.2f * (float)Math.Sin(Main.GlobalTimeWrappedHourly * 9f);

			
			for (int i = 4; i >= 0; i--)
			{
				float width = BeamWidth * (0.35f + i * 0.42f) * pulse;

				Color layer = Color.Lerp(new Color(120, 60, 255), new Color(60, 120, 255), i / 4f)
					* (0.18f - i * 0.028f);
				layer.A = 0;

				Main.spriteBatch.Draw(pixel, start, source, layer, rotation,
					new Vector2(0f, 0.5f), new Vector2(BeamLength, width), SpriteEffects.None, 0f);
			}

			
			Color core = Color.Lerp(new Color(230, 200, 255), new Color(180, 130, 255), 0.5f) * pulse;
			core.A = 0;

			Main.spriteBatch.Draw(pixel, start, source, core, rotation,
				new Vector2(0f, 0.5f), new Vector2(BeamLength, BeamWidth * 0.28f * pulse),
				SpriteEffects.None, 0f);

			return false;
		}
	}

	

	public class ApatheticSpiritCellBurst : ModProjectile
	{
		public override string Texture => "flameout/Content/ApatheticSpiritParticle";

		public override void SetDefaults()
		{
			Projectile.width = 16;
			Projectile.height = 16;
			Projectile.friendly = false;
			Projectile.hostile = true;
			Projectile.penetrate = -1;
			Projectile.timeLeft = 240;
			Projectile.tileCollide = false;
			Projectile.ignoreWater = true;
			Projectile.aiStyle = -1;
		}

		public override void AI()
		{
			Projectile.rotation = Projectile.velocity.ToRotation() + MathHelper.PiOver2;

			Lighting.AddLight(Projectile.Center, 0.45f, 0.15f, 0.8f);

			Dust dust = Dust.NewDustPerfect(Projectile.Center, DustID.PurpleTorch,
				-Projectile.velocity * 0.08f, 0, default, 1.2f);
			dust.noGravity = true;
		}

		public override bool PreDraw(ref Color lightColor)
		{
			Texture2D texture = TextureAssets.Projectile[Type].Value;
			Vector2 drawPos = Projectile.Center - Main.screenPosition;
			Vector2 origin = texture.Size() / 2f;

			float pulse = 0.85f + 0.15f * (float)Math.Sin(Main.GlobalTimeWrappedHourly * 10f);

			for (int i = 0; i < 4; i++)
			{
				float scale = Projectile.scale * (1f + i * 0.15f) * pulse;

				Color layerColor = Color.Lerp(new Color(150, 80, 255), new Color(70, 130, 255), i / 3f)
					* (0.18f - i * 0.035f);
				layerColor.A = 0;

				Main.spriteBatch.Draw(texture, drawPos, null, layerColor, Projectile.rotation,
					origin, scale, SpriteEffects.None, 0f);
			}

			Color core = new Color(220, 190, 255) * pulse;
			core.A = 0;

			Main.spriteBatch.Draw(texture, drawPos, null, core, Projectile.rotation,
				origin, Projectile.scale, SpriteEffects.None, 0f);

			return false;
		}
	}
		public class ApatheticSpiritRapierFinal : ModProjectile
	{
		public override string Texture => "flameout/Content/ApatheticRapierDash";

		public override void SetDefaults()
		{
			Projectile.width = 40;
			Projectile.height = 40;
			Projectile.hostile = true;
			Projectile.friendly = false;
			Projectile.tileCollide = false;
			Projectile.ignoreWater = true;
			Projectile.penetrate = -1;
			Projectile.timeLeft = 2;
			Projectile.aiStyle = -1;
			Projectile.netImportant = true;
		}

		public override bool ShouldUpdatePosition() => false;

		public override void AI()
		{
			NPC boss = Main.npc[(int)Projectile.ai[0]];

			if (!boss.active || boss.type != ModContent.NPCType<ApatheticSpirit>()
				|| ApatheticSpirit.Get(boss)?.FinalPhaseActive != true)
			{
				KillBeam();
				Projectile.Kill();
				return;
			}

			Projectile.timeLeft = 2;

			Player player = Main.player[boss.target];

			Projectile.localAI[0]++;
			float t = Projectile.localAI[0];

			int slot = (int)Projectile.ai[1];

			
			float angle = t * ApatheticSpirit.FinalRapierSpinSpeed
				+ MathHelper.TwoPi * slot / ApatheticSpirit.FinalRapierCount;

			
			int cycle = ApatheticSpirit.FinalRapierHopDelay + ApatheticSpirit.FinalRapierHopTravel;
			int totalSteps = ApatheticSpirit.FinalRapierHopSteps;

			int phase = (int)(t % (cycle * totalSteps));
			int step = phase / cycle;
			int local = phase % cycle;

			float stepLength = ApatheticSpirit.FinalArenaRadius / totalSteps;
			float fromDistance = stepLength * step;
			float toDistance = stepLength * (step + 1);

			float distance;

			if (local < ApatheticSpirit.FinalRapierHopDelay)
			{
				distance = fromDistance;
			}
			else
			{
				float hop = (local - ApatheticSpirit.FinalRapierHopDelay)
					/ (float)ApatheticSpirit.FinalRapierHopTravel;

				
				hop = 1f - (1f - hop) * (1f - hop);
				distance = MathHelper.Lerp(fromDistance, toDistance, hop);

				if (local == ApatheticSpirit.FinalRapierHopDelay && !Main.dedServ)
				{
					SoundEngine.PlaySound(SoundID.Item1 with { Volume = 0.5f }, Projectile.Center);

					for (int i = 0; i < 12; i++)
					{
						Dust dust = Dust.NewDustPerfect(Projectile.Center, DustID.PurpleTorch,
							Main.rand.NextVector2Circular(5f, 5f), 0, default, 1.5f);
						dust.noGravity = true;
					}
				}
			}

			Projectile.Center = boss.Center + angle.ToRotationVector2() * distance;

			
			Projectile.rotation = (player.Center - Projectile.Center).ToRotation() + MathHelper.PiOver4;

			Lighting.AddLight(Projectile.Center, 0.6f, 0.2f, 0.9f);

			
			if (Main.netMode != NetmodeID.MultiplayerClient)
			{
				int beamIndex = (int)Projectile.localAI[1];
				bool needsBeam = true;

				if (beamIndex >= 0 && beamIndex < Main.maxProjectiles)
				{
					Projectile beam = Main.projectile[beamIndex];

					if (beam.active
						&& beam.type == ModContent.ProjectileType<ApatheticSpiritFinalBeam>()
						&& (int)beam.ai[0] == Projectile.whoAmI)
					{
						needsBeam = false;
					}
				}

				if (needsBeam)
				{
					int beam = Projectile.NewProjectile(Projectile.GetSource_FromAI(),
						Projectile.Center, Vector2.Zero,
						ModContent.ProjectileType<ApatheticSpiritFinalBeam>(),
						Projectile.damage, 0f, Projectile.owner, Projectile.whoAmI);

					Projectile.localAI[1] = beam;
					Projectile.netUpdate = true;
				}
			}
		}

		private void KillBeam()
		{
			int beamIndex = (int)Projectile.localAI[1];

			if (beamIndex >= 0 && beamIndex < Main.maxProjectiles)
			{
				Projectile beam = Main.projectile[beamIndex];

				if (beam.active && beam.type == ModContent.ProjectileType<ApatheticSpiritFinalBeam>())
					beam.Kill();
			}
		}

		public override bool PreDraw(ref Color lightColor)
		{
			Texture2D texture = ModContent.Request<Texture2D>(Texture).Value;
			Vector2 drawPos = Projectile.Center - Main.screenPosition;

			Color glowColor = new Color(170, 90, 255, 0);

			for (int i = 0; i < 4; i++)
			{
				float scale = Projectile.scale * (1f + i * 0.12f);
				Color layerColor = glowColor * (0.2f - i * 0.04f);

				Main.spriteBatch.Draw(texture, drawPos, null, layerColor, Projectile.rotation,
					texture.Size() / 2f, scale, SpriteEffects.None, 0f);
			}

			Main.spriteBatch.Draw(texture, drawPos, null, lightColor, Projectile.rotation,
				texture.Size() / 2f, Projectile.scale, SpriteEffects.None, 0f);

			return false;
		}
	}
		public class ApatheticSpiritFinalBeam : ModProjectile
	{
		public const float BeamWidth = 20f;

		public override string Texture => "Terraria/Images/Projectile_0";

		public override void SetDefaults()
		{
			Projectile.width = 1;
			Projectile.height = 1;
			Projectile.hostile = true;
			Projectile.friendly = false;
			Projectile.tileCollide = false;
			Projectile.ignoreWater = true;
			Projectile.penetrate = -1;
			Projectile.timeLeft = 2;
			Projectile.aiStyle = -1;
			Projectile.usesLocalNPCImmunity = true;
			Projectile.localNPCHitCooldown = 12;
			Projectile.netImportant = true;
		}

		public override bool ShouldUpdatePosition() => false;

		private Vector2 Start => Projectile.Center;
		private Vector2 End => Projectile.Center + new Vector2(Projectile.localAI[0], Projectile.localAI[1]);

		public override void AI()
		{
			int rapierIndex = (int)Projectile.ai[0];
			Projectile rapier = Main.projectile[rapierIndex];

			if (!rapier.active || rapier.type != ModContent.ProjectileType<ApatheticSpiritRapierFinal>())
			{
				Projectile.Kill();
				return;
			}

			NPC boss = Main.npc[(int)rapier.ai[0]];

			if (!boss.active || boss.type != ModContent.NPCType<ApatheticSpirit>())
			{
				Projectile.Kill();
				return;
			}

			Projectile.timeLeft = 2;

			Projectile.Center = boss.Center;

			Vector2 offset = rapier.Center - boss.Center;
			Projectile.localAI[0] = offset.X;
			Projectile.localAI[1] = offset.Y;

			Projectile.rotation = offset.ToRotation();
		}

		public override bool? Colliding(Rectangle projHitbox, Rectangle targetHitbox)
		{
			float collisionPoint = 0f;

			return Collision.CheckAABBvLineCollision(
				targetHitbox.TopLeft(), targetHitbox.Size(),
				Start, End, BeamWidth, ref collisionPoint);
		}

		public override bool PreDraw(ref Color lightColor)
		{
			Texture2D pixel = TextureAssets.MagicPixel.Value;

			Vector2 start = Start - Main.screenPosition;
			Vector2 offset = End - Start;

			float length = offset.Length();

			if (length < 1f)
				return false;

			float rotation = offset.ToRotation();
			float pulse = 0.8f + 0.2f * (float)Math.Sin(Main.GlobalTimeWrappedHourly * 8f);

			for (int i = 4; i >= 0; i--)
			{
				Color layerColor = Color.Lerp(new Color(120, 60, 255), new Color(60, 120, 255), i / 4f)
					* ((0.2f - i * 0.03f) * pulse);
				layerColor.A = 0;

				float width = BeamWidth * (1f + i * 0.55f) * pulse;

				Main.spriteBatch.Draw(pixel, start, new Rectangle(0, 0, 1, 1), layerColor,
					rotation, new Vector2(0f, 0.5f), new Vector2(length, width),
					SpriteEffects.None, 0f);
			}

			Color core = Color.Lerp(new Color(230, 200, 255), new Color(180, 130, 255), 0.5f) * pulse;
			core.A = 0;

			Main.spriteBatch.Draw(pixel, start, new Rectangle(0, 0, 1, 1), core,
				rotation, new Vector2(0f, 0.5f), new Vector2(length, BeamWidth * 0.35f * pulse),
				SpriteEffects.None, 0f);

			return false;
		}
	}
		public class ApatheticSpiritProjectile : ModProjectile
	{
		public override string Texture => "flameout/Content/ApatheticSpiritParticle";

		public override void SetDefaults()
		{
			Projectile.width = 18;
			Projectile.height = 18;
			Projectile.hostile = true;
			Projectile.friendly = false;
			Projectile.tileCollide = false;
			Projectile.ignoreWater = true;
			Projectile.penetrate = -1;
			Projectile.timeLeft = 60 * 8;
			Projectile.aiStyle = -1;
			Projectile.scale = 1.3f;
		}

		public override void AI()
		{
			Projectile.rotation = Projectile.velocity.ToRotation() + MathHelper.PiOver2;

			Lighting.AddLight(Projectile.Center, 0.5f, 0.2f, 0.8f);

			if (Main.rand.NextBool(2))
			{
				Dust dust = Dust.NewDustPerfect(Projectile.Center, DustID.PurpleTorch,
					-Projectile.velocity * 0.15f, 0, default, 1.3f);
				dust.noGravity = true;
			}
		}

		public override bool PreDraw(ref Color lightColor)
		{
			Texture2D texture = ModContent.Request<Texture2D>(Texture).Value;
			Vector2 drawPos = Projectile.Center - Main.screenPosition;

			float pulse = 0.85f + 0.15f * (float)Math.Sin(Main.GlobalTimeWrappedHourly * 10f);
			Color glowColor = new Color(150, 80, 255, 0);

			for (int i = 0; i < 4; i++)
			{
				float scale = Projectile.scale * (1f + i * 0.15f) * pulse;
				Color layerColor = Color.Lerp(glowColor, new Color(70, 130, 255, 0), i / 3f)
					* (0.18f - i * 0.035f);

				Main.spriteBatch.Draw(texture, drawPos, null, layerColor, Projectile.rotation,
					texture.Size() / 2f, scale, SpriteEffects.None, 0f);
			}

			Color core = new Color(220, 190, 255) * pulse;
			core.A = 0;

			Main.spriteBatch.Draw(texture, drawPos, null, core, Projectile.rotation,
				texture.Size() / 2f, Projectile.scale, SpriteEffects.None, 0f);

			return false;
		}
	}
		public class ApatheticSpiritArenaPlayer : ModPlayer
	{
		private int drainTimer;

		public override void PostUpdate()
		{
			ApatheticSpirit boss = null;

			for (int i = 0; i < Main.maxNPCs; i++)
			{
				NPC npc = Main.npc[i];

				if (npc.active && npc.type == ModContent.NPCType<ApatheticSpirit>())
				{
					ApatheticSpirit spirit = ApatheticSpirit.Get(npc);

					if (spirit != null && spirit.FinalPhaseActive)
					{
						boss = spirit;
						break;
					}
				}
			}

			if (boss == null || Player.dead)
			{
				drainTimer = 0;
				return;
			}

			float distance = Vector2.Distance(Player.Center, boss.ArenaCenter);

			if (distance <= ApatheticSpirit.FinalArenaRadius)
			{
				drainTimer = 0;
				return;
			}

			drainTimer++;

			
			if (drainTimer % 20 == 0)
			{
				for (int i = 0; i < 6; i++)
				{
					Dust dust = Dust.NewDustPerfect(Player.Center + Main.rand.NextVector2Circular(30f, 30f),
						DustID.PurpleTorch, Vector2.Zero, 0, default, 1.8f);
					dust.noGravity = true;
				}
			}

			
			if (drainTimer % 10 == 0)
			{
				int damage = 25 + drainTimer / 6;

				Player.Hurt(
					PlayerDeathReason.ByCustomReason(
						Language.GetTextValue("Mods.flameout.DeathReason.ApatheticSpiritArena", Player.name)),
					damage, 0, dodgeable: false, armorPenetration: 100);
			}
		}
	}
}