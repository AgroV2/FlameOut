

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.IO;
using Terraria;
using Terraria.DataStructures;
using Terraria.GameContent.Bestiary;
using Terraria.ID;
using Terraria.ModLoader;
using ReLogic.Content;
using Terraria.GameContent.ItemDropRules;

namespace flameout.Content
{
    [AutoloadBossHead]
    public class ApathyBoss : ModNPC
    {
        public override string Texture => "flameout/Content/ApathyBoss";

        private const int STATE_IDLE = 0;
        private const int STATE_ATTACK1 = 1;
        private const int STATE_ATTACK2 = 2;
        private const int STATE_ATTACK3 = 3;
        private const int STATE_ATTACK4 = 4;

        private const int IDLE_PAUSE = 90;
        private const int ATTACK1_DURATION = 480;
        private const int ATTACK2_DURATION = 360;
        private const int ATTACK3_DURATION = 600;
        private const int ATTACK4_DURATION = 420;
        private const int DESPAWN_TIME = 180;

        private const int BEAM_COUNT = 4;
        private const int PARTICLE_INTERVAL = 4;
        private const int PARTICLE_COUNT = 12;
        private const float PARTICLE_SPEED_MIN = 5f;
        private const float PARTICLE_SPEED_MAX = 11f;

        private Vector2 spawnPosition;
        private bool spawnSet;
        private int despawnTimer;

        
        private int[] attackPattern = new int[4] { STATE_ATTACK1, STATE_ATTACK2, STATE_ATTACK3, STATE_ATTACK4 };
        private int patternIndex;
        private bool patternSet;

        private int[] attack2LaserIndices = new int[BEAM_COUNT] { -1, -1, -1, -1 };
        private int attack3LaserIndex = -1;
        private int attack4LaserIndex = -1;

        private Asset<Texture2D> attack1Tex;
        private Asset<Texture2D> attack2Tex;

        public override void SetStaticDefaults()
        {
            Main.npcFrameCount[NPC.type] = 1;
            NPCID.Sets.MustAlwaysDraw[NPC.type] = true;
            NPCID.Sets.MPAllowedEnemies[NPC.type] = true;
            NPCID.Sets.BossBestiaryPriority.Add(Type);
        }

        public override void SetDefaults()
        {
            NPC.width = 64;
            NPC.height = 64;
            NPC.lifeMax = 3000000;
            NPC.damage = 9999;
            NPC.defense = 0;
            NPC.knockBackResist = 0f;
            NPC.noGravity = true;
            NPC.noTileCollide = true;
            NPC.boss = true;
            NPC.value = Item.buyPrice(0, 20, 0, 0);
            NPC.HitSound = SoundID.NPCHit1;
            NPC.DeathSound = SoundID.NPCDeath1;
            NPC.aiStyle = -1;
            NPC.npcSlots = 15f;
            Music = MusicLoader.GetMusicSlot(Mod, "Content/Music/ApathyMusic");
        }

        public override void SetBestiary(BestiaryDatabase database, BestiaryEntry bestiaryEntry)
        {
            bestiaryEntry.Info.AddRange(new IBestiaryInfoElement[]
            {
                BestiaryDatabaseNPCsPopulator.CommonTags.SpawnConditions.Biomes.TheHallow,
                new FlavorTextBestiaryInfoElement("Apathy.")
            });
        }

        public override void OnSpawn(IEntitySource source)
        {
            spawnPosition = NPC.Center;
            spawnSet = true;
            NPC.netUpdate = true;
        }

        public override void SendExtraAI(BinaryWriter writer)
        {
            writer.WriteVector2(spawnPosition);
            writer.Write(spawnSet);
            writer.Write(patternSet);
            writer.Write(patternIndex);
            for (int i = 0; i < attackPattern.Length; i++)
                writer.Write(attackPattern[i]);
            for (int i = 0; i < BEAM_COUNT; i++)
                writer.Write(attack2LaserIndices[i]);
            writer.Write(attack3LaserIndex);
            writer.Write(attack4LaserIndex);
        }

        public override void ReceiveExtraAI(BinaryReader reader)
        {
            spawnPosition = reader.ReadVector2();
            spawnSet = reader.ReadBoolean();
            patternSet = reader.ReadBoolean();
            patternIndex = reader.ReadInt32();
            for (int i = 0; i < attackPattern.Length; i++)
                attackPattern[i] = reader.ReadInt32();
            for (int i = 0; i < BEAM_COUNT; i++)
                attack2LaserIndices[i] = reader.ReadInt32();
            attack3LaserIndex = reader.ReadInt32();
            attack4LaserIndex = reader.ReadInt32();
        }

        public override void AI()
        {
            if (spawnSet)
                NPC.Center = spawnPosition;

            NPC.velocity = Vector2.Zero;

            if (!NPC.HasValidTarget || Main.player[NPC.target].dead)
                NPC.TargetClosest(false);

            if (!NPC.HasValidTarget || Main.player[NPC.target].dead)
            {
                KillAllAttackProjectiles();
                NPC.ai[3] = 0f;
                NPC.ai[0] = STATE_IDLE;
                NPC.ai[1] = 0f;

                
                patternSet = false;
                patternIndex = 0;
                if (++despawnTimer > DESPAWN_TIME)
                {
                    NPC.active = false;
                    NPC.netUpdate = true;
                }
                return;
            }

            despawnTimer = 0;

            UpdateArena();

            if (Main.netMode != NetmodeID.MultiplayerClient && NPC.life < NPC.lifeMax)
                TryActivate();

            Player target = Main.player[NPC.target];
            int state = (int)NPC.ai[0];
            int timer = (int)NPC.ai[1];

            switch (state)
            {
                case STATE_IDLE:
                    DoIdle(target);
                    break;
                case STATE_ATTACK1:
                    DoAttack1(target, timer);
                    break;
                case STATE_ATTACK2:
                    DoAttack2(target, timer);
                    break;
                case STATE_ATTACK3:
                    DoAttack3(target, timer);
                    break;
                case STATE_ATTACK4:
                    DoAttack4(target, timer);
                    break;
            }

            NPC.ai[1]++;
        }

        
        
        private void UpdateArena()
        {
            int arenaType = ModContent.ProjectileType<ApathyArena>();
            Vector2 center = spawnSet ? spawnPosition : NPC.Center;
            bool found = false;

            for (int i = 0; i < Main.maxProjectiles; i++)
            {
                Projectile p = Main.projectile[i];
                if (!p.active || p.type != arenaType)
                    continue;

                p.Center = center;
                p.timeLeft = 2;
                found = true;
            }

            if (!found && Main.netMode != NetmodeID.MultiplayerClient)
            {
                Projectile.NewProjectile(
                    NPC.GetSource_FromAI(),
                    center,
                    Vector2.Zero,
                    arenaType,
                    0,
                    0f,
                    Main.maxPlayers);
            }
        }

        private void TryActivate()
        {
            if ((int)NPC.ai[0] == STATE_IDLE && (int)NPC.ai[3] == 0)
            {
                
                if (!patternSet)
                    BuildPattern();

                NPC.ai[3] = 1f;
                NPC.ai[1] = 0f;
                NPC.netUpdate = true;
            }
        }

        
        private void BuildPattern()
        {
            attackPattern[0] = STATE_ATTACK1;
            attackPattern[1] = STATE_ATTACK2;
            attackPattern[2] = STATE_ATTACK3;
            attackPattern[3] = STATE_ATTACK4;

            for (int i = attackPattern.Length - 1; i > 0; i--)
            {
                int j = Main.rand.Next(i + 1);
                (attackPattern[i], attackPattern[j]) = (attackPattern[j], attackPattern[i]);
            }

            patternIndex = 0;
            patternSet = true;
            NPC.netUpdate = true;
        }

        private void DoIdle(Player target)
        {
            NPC.rotation = NPC.Center.DirectionTo(target.Center).ToRotation();

            if ((int)NPC.ai[3] != 1)
                return;
            if ((int)NPC.ai[1] < IDLE_PAUSE)
                return;
            if (Main.netMode == NetmodeID.MultiplayerClient)
                return;

            NPC.ai[3] = 0f;
            NPC.ai[1] = -1f;
            NPC.ai[0] = NextPatternAttack();
            NPC.netUpdate = true;
        }

        
        private int NextPatternAttack()
        {
            if (!patternSet)
                BuildPattern();

            int attack = attackPattern[patternIndex % attackPattern.Length];
            patternIndex = (patternIndex + 1) % attackPattern.Length;
            return attack;
        }

        private void ReturnToIdle()
        {
            ClearHostileProjectiles();

            NPC.ai[0] = STATE_IDLE;
            NPC.ai[1] = -1f;
            NPC.ai[2] = 0f;
            NPC.ai[3] = 1f;
            NPC.netUpdate = true;
        }

        private void DoAttack1(Player target, int timer)
        {
            NPC.rotation = NPC.Center.DirectionTo(target.Center).ToRotation();

            if (timer == 0 && Main.netMode != NetmodeID.MultiplayerClient)
            {
                Vector2 vel = NPC.Center.DirectionTo(target.Center) * 12f;
                Projectile.NewProjectile(
                    NPC.GetSource_FromAI(),
                    NPC.Center,
                    vel,
                    ModContent.ProjectileType<ApathyBigLaser>(),
                    9999,
                    0f,
                    Main.maxPlayers);
            }

            if (timer >= ATTACK1_DURATION)
                ReturnToIdle();
        }

        private void DoAttack2(Player target, int timer)
        {
            NPC.rotation = NPC.Center.DirectionTo(target.Center).ToRotation();

            if (timer == 0 && Main.netMode != NetmodeID.MultiplayerClient)
            {
                float baseAngle = Main.rand.NextFloat(MathHelper.TwoPi);
                float sector = MathHelper.TwoPi / BEAM_COUNT;
                for (int i = 0; i < BEAM_COUNT; i++)
                {
                    float angle = baseAngle + sector * i + Main.rand.NextFloat(-sector * 0.35f, sector * 0.35f);
                    Vector2 dir = angle.ToRotationVector2();
                    int idx = Projectile.NewProjectile(
                        NPC.GetSource_FromAI(),
                        NPC.Center,
                        Vector2.Zero,
                        ModContent.ProjectileType<ApathyStaticBeam>(),
                        9999,
                        0f,
                        Main.maxPlayers);
                    if (idx >= 0 && idx < Main.maxProjectiles)
                    {
                        Main.projectile[idx].ai[0] = dir.X;
                        Main.projectile[idx].ai[1] = dir.Y;
                        Main.projectile[idx].netUpdate = true;
                        attack2LaserIndices[i] = idx;
                    }
                }
                NPC.netUpdate = true;
            }

            int beamType = ModContent.ProjectileType<ApathyStaticBeam>();
            for (int i = 0; i < BEAM_COUNT; i++)
            {
                int idx = attack2LaserIndices[i];
                if (idx < 0 || idx >= Main.maxProjectiles)
                    continue;

                Projectile p = Main.projectile[idx];
                if (p.active && p.type == beamType)
                {
                    p.Center = NPC.Center;
                    p.timeLeft = 2;
                }
            }

            if (timer % PARTICLE_INTERVAL == 0 && Main.netMode != NetmodeID.MultiplayerClient)
            {
                for (int i = 0; i < PARTICLE_COUNT; i++)
                {
                    float angle = Main.rand.NextFloat(MathHelper.TwoPi);
                    float speed = Main.rand.NextFloat(PARTICLE_SPEED_MIN, PARTICLE_SPEED_MAX);
                    Vector2 vel = angle.ToRotationVector2() * speed;
                    Vector2 pos = NPC.Center + Main.rand.NextVector2Circular(16f, 16f);
                    Projectile.NewProjectile(
                        NPC.GetSource_FromAI(),
                        pos,
                        vel,
                        ModContent.ProjectileType<ApathyParticle>(),
                        9999,
                        0f,
                        Main.maxPlayers);
                }
            }

            if (timer >= ATTACK2_DURATION)
            {
                for (int i = 0; i < BEAM_COUNT; i++)
                    KillTracked(ref attack2LaserIndices[i], beamType);
                ReturnToIdle();
            }
        }

        private void DoAttack3(Player target, int timer)
        {
            NPC.rotation = NPC.Center.DirectionTo(target.Center).ToRotation();

            if (timer == 0 && Main.netMode != NetmodeID.MultiplayerClient)
            {
                int idx = Projectile.NewProjectile(
                    NPC.GetSource_FromAI(),
                    NPC.Center,
                    Vector2.Zero,
                    ModContent.ProjectileType<ApathySpinLaser>(),
                    9999,
                    0f,
                    Main.maxPlayers);
                if (idx >= 0 && idx < Main.maxProjectiles)
                {
                    attack3LaserIndex = idx;
                    NPC.netUpdate = true;
                }
            }

            int spinType = ModContent.ProjectileType<ApathySpinLaser>();
            if (attack3LaserIndex >= 0 && attack3LaserIndex < Main.maxProjectiles)
            {
                Projectile p = Main.projectile[attack3LaserIndex];
                if (p.active && p.type == spinType)
                {
                    p.Center = NPC.Center;
                    p.timeLeft = 2;
                }
            }

            if (timer >= ATTACK3_DURATION)
            {
                KillTracked(ref attack3LaserIndex, spinType);
                ReturnToIdle();
            }
        }

        
        
        private void DoAttack4(Player target, int timer)
        {
            NPC.rotation = NPC.Center.DirectionTo(target.Center).ToRotation();

            int trackType = ModContent.ProjectileType<ApathyTrackingLaser>();

            if (timer == 0 && Main.netMode != NetmodeID.MultiplayerClient)
            {
                int idx = Projectile.NewProjectile(
                    NPC.GetSource_FromAI(),
                    NPC.Center,
                    Vector2.Zero,
                    trackType,
                    9999,
                    0f,
                    Main.maxPlayers);
                if (idx >= 0 && idx < Main.maxProjectiles)
                {
                    attack4LaserIndex = idx;
                    NPC.netUpdate = true;
                }
            }

            if (attack4LaserIndex >= 0 && attack4LaserIndex < Main.maxProjectiles)
            {
                Projectile p = Main.projectile[attack4LaserIndex];
                if (p.active && p.type == trackType)
                    p.timeLeft = 2;
            }

            if (timer >= ATTACK4_DURATION)
            {
                KillTracked(ref attack4LaserIndex, trackType);
                ReturnToIdle();
            }
        }

        private void KillTracked(ref int index, int type)
        {
            if (index >= 0 && index < Main.maxProjectiles)
            {
                Projectile p = Main.projectile[index];
                if (p.active && p.type == type)
                    p.Kill();
            }
            index = -1;
        }

        
        
        private void ClearHostileProjectiles()
        {
            if (Main.netMode == NetmodeID.MultiplayerClient)
                return;

            int arenaType = ModContent.ProjectileType<ApathyArena>();
            int deathType = ModContent.ProjectileType<ApathyDeathEffect>();

            for (int i = 0; i < Main.maxProjectiles; i++)
            {
                Projectile p = Main.projectile[i];
                if (!p.active || !p.hostile)
                    continue;
                if (p.type == arenaType || p.type == deathType)
                    continue;

                int identity = p.identity;
                int owner = p.owner;
                p.Kill();

                if (Main.netMode == NetmodeID.Server)
                    NetMessage.SendData(MessageID.KillProjectile, -1, -1, null, identity, owner);
            }
        }

        private void KillAllAttackProjectiles()
        {
            int beamType = ModContent.ProjectileType<ApathyStaticBeam>();
            int spinType = ModContent.ProjectileType<ApathySpinLaser>();

            for (int i = 0; i < BEAM_COUNT; i++)
                KillTracked(ref attack2LaserIndices[i], beamType);
            KillTracked(ref attack3LaserIndex, spinType);
            KillTracked(ref attack4LaserIndex, ModContent.ProjectileType<ApathyTrackingLaser>());

            int arenaType = ModContent.ProjectileType<ApathyArena>();
            for (int i = 0; i < Main.maxProjectiles; i++)
            {
                Projectile p = Main.projectile[i];
                if (p.active && p.type == arenaType)
                    p.Kill();
            }
        }

        public override void OnHitByProjectile(Projectile projectile, NPC.HitInfo hit, int damageDone)
        {
            if (Main.netMode != NetmodeID.MultiplayerClient)
                TryActivate();
        }

        public override void OnHitByItem(Player player, Item item, NPC.HitInfo hit, int damageDone)
        {
            if (Main.netMode != NetmodeID.MultiplayerClient)
                TryActivate();
        }

        public override bool PreDraw(SpriteBatch spriteBatch, Vector2 screenPos, Color drawColor)
        {
            attack1Tex ??= ModContent.Request<Texture2D>("flameout/Content/ApathyBoss_Attack1");
            attack2Tex ??= ModContent.Request<Texture2D>("flameout/Content/ApathyBoss_Attack2");

            int state = (int)NPC.ai[0];
            bool useSecond = state == STATE_ATTACK2 || state == STATE_ATTACK4;
            Texture2D tex = (useSecond ? attack2Tex : attack1Tex).Value;
            Vector2 origin = tex.Size() * 0.5f;
            Vector2 drawPos = NPC.Center - screenPos;

            Color glowColor = useSecond
                ? new Color(255, 80, 180) * 0.4f
                : new Color(120, 40, 220) * 0.4f;

            spriteBatch.Draw(tex, drawPos, null, glowColor, NPC.rotation, origin, NPC.scale * 1.3f, SpriteEffects.None, 0f);
            spriteBatch.Draw(tex, drawPos, null, drawColor, NPC.rotation, origin, NPC.scale, SpriteEffects.None, 0f);
            return false;
        }

        public override void ModifyNPCLoot(NPCLoot npcLoot)
        {
            
            npcLoot.Add(ItemDropRule.ByCondition(new ApathySoulNotOwned(), ModContent.ItemType<ApathySoul>()));
            
            npcLoot.Add(ItemDropRule.Common(ModContent.ItemType<KeyOfEverywhere>()));
        }

        public override void BossLoot(ref int potionType)
        {
            potionType = ItemID.None; 
        }

        public override void OnKill()
        {
            KillAllAttackProjectiles();

            if (Main.netMode != NetmodeID.MultiplayerClient)
            {
                Projectile.NewProjectile(
                    NPC.GetSource_Death(),
                    spawnSet ? spawnPosition : NPC.Center,
                    Vector2.Zero,
                    ModContent.ProjectileType<ApathyDeathEffect>(),
                    0,
                    0f,
                    Main.maxPlayers);
            }

            ApathyWorld.downedApathy = true;
            if (Main.netMode == NetmodeID.Server)
                NetMessage.SendData(MessageID.WorldData);
        }

        public override bool CheckActive() => false;
    }
}

