

using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.Audio;
using Terraria.GameContent;
using Terraria.GameContent.ItemDropRules;
using Terraria.Graphics.CameraModifiers;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.UI;

namespace flameout.Content
{
    [AutoloadBossHead]
    public class TrueEmpathy : ModNPC
    {
        public override string Texture => "flameout/Content/Empathy";

        private const int Slots = 3;
        private const int PhaseCount = 15;

        private readonly int[] _phase = new int[Slots];
        private readonly int[] _timer = new int[Slots];
        private readonly int[] _duration = new int[Slots];
        private readonly bool[] _init = new bool[Slots];
        private readonly float[] _state = new float[Slots];

        private bool _useAltSprite;
        private int _uiStage;
        private int _teleportCooldown;

        private EmpathyPhase2Controller _p2;
        public bool Phasee2;

        
        private const int ProjectileHeadroom = 60;
        private int _wipeCooldown;

        public override void SetStaticDefaults()
        {
            NPCID.Sets.BossHeadTextures[NPC.type] = ModContent.GetModBossHeadSlot("flameout/Content/Empathy_Head_Boss");
            Main.npcFrameCount[NPC.type] = 1;
            NPCID.Sets.MPAllowedEnemies[NPC.type] = true;
            NPCID.Sets.BossBestiaryPriority.Add(NPC.type);
            NPCID.Sets.ShouldBeCountedAsBoss[NPC.type] = true;
            NPCID.Sets.CantTakeLunchMoney[NPC.type] = true;
        }

        public override void SetDefaults()
        {
            NPC.width = 40;
            NPC.height = 56;
            NPC.lifeMax = int.MaxValue;
            NPC.life = int.MaxValue;
            NPC.damage = int.MaxValue;
            NPC.defDamage = int.MaxValue;
            NPC.defense = 9999;
            NPC.defDefense = 9999;
            NPC.knockBackResist = 0f;
            NPC.noGravity = true;
            NPC.noTileCollide = true;
            NPC.boss = true;
            NPC.npcSlots = 20f;
            NPC.HitSound = SoundID.NPCHit1;
            NPC.DeathSound = SoundID.NPCDeath1;
            NPC.value = 0f;
            
            
            Music = MusicLoader.GetMusicSlot(Mod, "Content/Music/EmpathyMusic");
        }

        
        public override void ApplyDifficultyAndPlayerScaling(int numPlayers, float balance, float bossAdjustment)
        {
            NPC.lifeMax = int.MaxValue;
            NPC.life = int.MaxValue;
            NPC.damage = int.MaxValue;
            NPC.defDamage = int.MaxValue;
        }

        
        
        public override void ModifyHitPlayer(Player target, ref Player.HurtModifiers modifiers)
        {
            modifiers.ScalingArmorPenetration += 1f;
            modifiers.SetMaxDamage(999999);
        }

        public override void ModifyNPCLoot(NPCLoot npcLoot)
        {
            npcLoot.Add(ItemDropRule.Common(ModContent.ItemType<Empathium>(), 1, 10, 10));
        }

        public override void BossLoot(ref int potionType)
        {
            potionType = ItemID.None;
        }

        public override bool CheckActive() => false;

        public override void OnKill()
        {
            ApathyWorld.downedTrueEmpathy = true;
            if (Main.netMode == NetmodeID.Server)
                NetMessage.SendData(MessageID.WorldData);

            Phasee2 = false;
            _p2?.End();
            _p2 = null;

            
            TrueEmpathyUI.Stage = 0;
            TrueEmpathyHunt.Hunting = false;
        }

        private void Shake(float strength, int frames)
        {
            if (Main.dedServ)
                return;
            Main.instance.CameraModifiers.Add(new PunchCameraModifier(
                NPC.Center, Main.rand.NextVector2Unit(), strength, 8f, frames, 4000f, "flameout:TrueEmpathy"));
        }

        public override void AI()
        {
            UpdatePhase2();

            NPC.TargetClosest(true);
            Player target = Main.player[NPC.target];

            if (!target.active || target.dead)
            {
                NPC.TargetClosest(false);
                target = Main.player[NPC.target];
            }

            NPC.spriteDirection = target.Center.X < NPC.Center.X ? -1 : 1;
            Lighting.AddLight(NPC.Center, 1.4f, 0.1f, 0.2f);

            FollowAcrossDistance(target);
            UpdateUiStage();
            CheckProjectileFlood();

            for (int i = 0; i < Slots; i++)
            {
                if (_phase[i] == 0)
                    PickPhase(i);

                _timer[i]++;
                RunPhase(i, target, i == 0);

                if (_timer[i] >= _duration[i])
                    PickPhase(i);
            }

            if (Main.rand.NextBool(20))
                Shake(3f, 12);
        }

        
        private void UpdatePhase2()
        {
            if (!Phasee2 && NPC.life <= NPC.lifeMax / 2)
            {
                Phasee2 = true;
                NPC.netUpdate = true;

                if (!Main.dedServ)
                {
                    SoundEngine.PlaySound(new SoundStyle("flameout/Content/Sounds/em2"), NPC.Center);
                    Shake(22f, 50);
                }
            }

            if (Phasee2)
            {
                _p2 ??= new EmpathyPhase2Controller(NPC);
                _p2.Update();
            }
            else if (_p2 != null)
            {
                _p2.End();
                _p2 = null;
            }
        }

        private void FollowAcrossDistance(Player target)
        {
            if (_teleportCooldown > 0)
                _teleportCooldown--;

            float dist = Vector2.Distance(NPC.Center, target.Center);
            if (dist > 120f * 16f && _teleportCooldown <= 0)
            {
                NPC.Center = target.Center - new Vector2(0f, 240f);
                NPC.velocity = Vector2.Zero;
                NPC.netUpdate = true;
                _teleportCooldown = 60;
                SoundEngine.PlaySound(new SoundStyle("flameout/Content/Sounds/em1"), NPC.Center);
            }
        }

        private void UpdateUiStage()
        {
            float ratio = NPC.life / (float)NPC.lifeMax;
            int stage = 0;
            if (ratio <= 0.75f) stage = 1;
            if (ratio <= 0.50f) stage = 2;
            if (ratio <= 0.25f) stage = 3;
            if (ratio <= 0.05f) stage = 4;

            if (stage != _uiStage)
            {
                _uiStage = stage;
                if (!Main.dedServ)
                {
                    SoundEngine.PlaySound(SoundID.Item14, NPC.Center);
                    Shake(16f, 40);
                }
            }

            TrueEmpathyUI.Stage = Math.Max(TrueEmpathyUI.Stage, stage);
        }

        
        
        private void CheckProjectileFlood()
        {
            if (_wipeCooldown > 0)
            {
                _wipeCooldown--;
                return;
            }

            if (Main.netMode == NetmodeID.MultiplayerClient || (int)Main.GameUpdateCount % 5 != 0)
                return;

            int active = 0;
            for (int i = 0; i < Main.maxProjectiles; i++)
            {
                if (Main.projectile[i].active)
                    active++;
            }

            if (active < Main.maxProjectiles - ProjectileHeadroom)
                return;

            WipeProjectiles();

            _useAltSprite = false;
            for (int i = 0; i < Slots; i++)
            {
                _phase[i] = 0;
                PickPhase(i);
            }

            _wipeCooldown = 120;

            SoundEngine.PlaySound(SoundID.Item14, NPC.Center);
            SoundEngine.PlaySound(new SoundStyle("flameout/Content/Sounds/em3"), NPC.Center);
            Shake(28f, 45);

            if (Main.dedServ)
                return;

            for (int i = 0; i < 90; i++)
            {
                Vector2 dir = Main.rand.NextVector2Unit();
                Dust d = Dust.NewDustPerfect(NPC.Center + dir * Main.rand.NextFloat(40f, 260f),
                    DustID.RedTorch, dir * Main.rand.NextFloat(6f, 16f), 60, default, 2f);
                d.noGravity = true;
            }
        }

        private void WipeProjectiles()
        {
            for (int i = 0; i < Main.maxProjectiles; i++)
            {
                Projectile p = Main.projectile[i];
                if (!p.active)
                    continue;

                
                p.penetrate = 1;
                p.timeLeft = 0;
                p.Kill();
            }
        }

        private void PickPhase(int slot)
        {
            int next;
            int guard = 0;
            do
            {
                next = Main.rand.Next(1, PhaseCount + 1);
                guard++;
            }
            while (guard < 60 && (next == _phase[slot]
                                  || next == _phase[(slot + 1) % Slots]
                                  || next == _phase[(slot + 2) % Slots]));

            _phase[slot] = next;
            _timer[slot] = 0;
            _duration[slot] = 120;
            _init[slot] = false;
            _state[slot] = 0f;
        }

        private bool Begin(int slot, int duration)
        {
            if (_init[slot])
                return false;
            _init[slot] = true;
            _duration[slot] = duration;
            return true;
        }

        private void Chase(Player target, float speed, float lerp)
        {
            Vector2 dir = (target.Center - NPC.Center).SafeNormalize(Vector2.UnitX);
            NPC.velocity = Vector2.Lerp(NPC.velocity, dir * speed, lerp);
        }

        private void Sound(string name)
        {
            SoundEngine.PlaySound(new SoundStyle("flameout/Content/Sounds/" + name), NPC.Center);
        }

        private bool Server => Main.netMode != NetmodeID.MultiplayerClient;

        private void RunPhase(int slot, Player target, bool primary)
        {
            switch (_phase[slot])
            {
                case 1: PStar(slot, target, primary); break;
                case 2: PPentagrams(slot, target, primary); break;
                case 3: PPentagramLasers(slot, target, primary); break;
                case 4: PSword(slot, target, primary); break;
                case 5: PCrossLasers(slot, target, primary); break;
                case 6: PScythe(slot, target, primary); break;
                case 7: PPortals(slot, target, primary); break;
                case 8: PGuardians(slot, target, primary); break;
                case 9: PPortalChase(slot, target, primary); break;
                case 10: POrbLasers(slot, target, primary); break;
                case 11: PMiniBosses(slot, target, primary); break;
                case 12: PCrosses(slot, target, primary); break;
                case 13: PBigScythe(slot, target, primary); break;
                case 14: PDiagonalLasers(slot, target, primary); break;
                case 15: PStarRing(slot, target, primary); break;
            }
        }

        private void PStar(int slot, Player target, bool primary)
        {
            if (Begin(slot, 900))
            {
                Sound("em1");
                if (Server)
                    Projectile.NewProjectile(NPC.GetSource_FromAI(), NPC.Center, Vector2.Zero,
                        ModContent.ProjectileType<EmpathyStar>(), NPC.damage / 2, 0f, Main.myPlayer, NPC.whoAmI);
            }
            if (primary)
                Chase(target, 20f, 0.07f);
        }

        private void PPentagrams(int slot, Player target, bool primary)
        {
            if (Begin(slot, 600))
            {
                Sound("em2");
                if (Server)
                {
                    Projectile.NewProjectile(NPC.GetSource_FromAI(), NPC.Center, Vector2.Zero,
                        ModContent.ProjectileType<EmpathyPentagram>(), NPC.damage / 3, 0f, Main.myPlayer, NPC.whoAmI, 0f);
                    Projectile.NewProjectile(NPC.GetSource_FromAI(), NPC.Center, Vector2.Zero,
                        ModContent.ProjectileType<EmpathyPentagram>(), NPC.damage / 3, 0f, Main.myPlayer, NPC.whoAmI, MathHelper.Pi);
                }
            }
            if (primary)
                Chase(target, 10f, 0.06f);
        }

        private void PPentagramLasers(int slot, Player target, bool primary)
        {
            if (Begin(slot, 600))
            {
                Sound("em2");
                if (Server)
                {
                    Projectile.NewProjectile(NPC.GetSource_FromAI(), NPC.Center, Vector2.Zero,
                        ModContent.ProjectileType<EmpathyPentagramLaser>(), NPC.damage / 3, 0f, Main.myPlayer, NPC.whoAmI, 0f);
                    Projectile.NewProjectile(NPC.GetSource_FromAI(), NPC.Center, Vector2.Zero,
                        ModContent.ProjectileType<EmpathyPentagramLaser>(), NPC.damage / 3, 0f, Main.myPlayer, NPC.whoAmI, MathHelper.Pi);
                }
            }
            if (primary)
                Chase(target, 9f, 0.06f);
        }

        private void PSword(int slot, Player target, bool primary)
        {
            if (Begin(slot, 300))
            {
                Sound("em1");
                if (Server)
                    Projectile.NewProjectile(NPC.GetSource_FromAI(), NPC.Center, Vector2.Zero,
                        ModContent.ProjectileType<EmpathySword>(), NPC.damage, 0f, Main.myPlayer, NPC.whoAmI);
            }
            if (primary)
                Chase(target, 14f, 0.08f);
        }

        private void PCrossLasers(int slot, Player target, bool primary)
        {
            if (Begin(slot, 150))
            {
                Sound("em3");
                if (Server)
                {
                    float[] angles = { 0f, MathHelper.PiOver2, MathHelper.Pi, MathHelper.Pi * 1.5f };
                    foreach (float a in angles)
                        Projectile.NewProjectile(NPC.GetSource_FromAI(), NPC.Center, Vector2.Zero,
                            ModContent.ProjectileType<EmpathyLaser>(), NPC.damage / 2, 0f, Main.myPlayer, a, 3000f);
                }
            }
            if (primary)
                NPC.velocity *= 0.9f;
        }

        private void PDiagonalLasers(int slot, Player target, bool primary)
        {
            if (Begin(slot, 150))
            {
                Sound("em3");
                if (Server)
                {
                    for (int i = 0; i < 4; i++)
                        Projectile.NewProjectile(NPC.GetSource_FromAI(), NPC.Center, Vector2.Zero,
                            ModContent.ProjectileType<EmpathyLaser>(), NPC.damage / 2, 0f, Main.myPlayer,
                            MathHelper.PiOver4 + MathHelper.PiOver2 * i, 3000f);
                }
            }
            if (primary)
                NPC.velocity *= 0.9f;
        }

        private void PScythe(int slot, Player target, bool primary)
        {
            if (Begin(slot, 480))
            {
                Sound("em2");
                _useAltSprite = true;
                if (Server)
                {
                    int index = Projectile.NewProjectile(NPC.GetSource_FromAI(), NPC.Center, Vector2.Zero,
                        ModContent.ProjectileType<EmpathyScythe>(), NPC.damage, 0f, Main.myPlayer, NPC.whoAmI);
                    _state[slot] = index;
                }
            }

            if (primary)
                Chase(target, 16f, 0.08f);

            if (_timer[slot] >= _duration[slot] - 1)
            {
                int index = (int)_state[slot];
                if (index >= 0 && index < Main.maxProjectiles && Main.projectile[index].active)
                    Main.projectile[index].Kill();
                _useAltSprite = false;
            }
        }

        private void PPortals(int slot, Player target, bool primary)
        {
            if (Begin(slot, 240))
            {
                Sound("em1");
                if (Server)
                {
                    Projectile.NewProjectile(NPC.GetSource_FromAI(), NPC.Center + new Vector2(-200f, 0f), Vector2.Zero,
                        ModContent.ProjectileType<EmpathyPortal1>(), NPC.damage / 3, 0f, Main.myPlayer, NPC.whoAmI);
                    Projectile.NewProjectile(NPC.GetSource_FromAI(), NPC.Center + new Vector2(200f, 0f), Vector2.Zero,
                        ModContent.ProjectileType<EmpathyPortal2>(), NPC.damage / 3, 0f, Main.myPlayer, NPC.whoAmI);
                }
            }
            if (primary)
                Chase(target, 8f, 0.05f);
        }

        private void PGuardians(int slot, Player target, bool primary)
        {
            if (Begin(slot, 180))
            {
                Sound("em3");
                if (Server)
                {
                    NPC.NewNPC(NPC.GetSource_FromAI(), (int)(target.Center.X - 50f * 16f), (int)target.Center.Y,
                        ModContent.NPCType<Guardian1>());
                    NPC.NewNPC(NPC.GetSource_FromAI(), (int)(target.Center.X + 50f * 16f), (int)target.Center.Y,
                        ModContent.NPCType<Guardian2>());
                }
            }
            if (primary)
                Chase(target, 10f, 0.05f);
        }

        private void PPortalChase(int slot, Player target, bool primary)
        {
            if (Begin(slot, 300))
            {
                if (Server)
                    Projectile.NewProjectile(NPC.GetSource_FromAI(), NPC.Center, Vector2.Zero,
                        ModContent.ProjectileType<EmpathyPortal>(), 0, 0f, Main.myPlayer, NPC.whoAmI);
            }
            if (primary)
            {
                Vector2 dir = (target.Center - NPC.Center).SafeNormalize(Vector2.UnitX);
                NPC.velocity = dir * 12f;
            }
        }

        private void POrbLasers(int slot, Player target, bool primary)
        {
            if (Begin(slot, 220))
            {
                Sound("em1");
                if (Server)
                {
                    foreach (Vector2 dir in Directions())
                        Projectile.NewProjectile(NPC.GetSource_FromAI(), NPC.Center + dir * (10 * 16f), Vector2.Zero,
                            ModContent.ProjectileType<EmpathyOrb>(), 0, 0f, Main.myPlayer, dir.X, dir.Y);
                }
            }

            if (primary)
                NPC.velocity *= 0.88f;

            if (_timer[slot] == 120 && Server)
            {
                foreach (Vector2 dir in Directions())
                {
                    Color c = new Color(Main.rand.Next(100, 255), Main.rand.Next(100, 255), Main.rand.Next(100, 255));
                    int packed = (c.R << 16) | (c.G << 8) | c.B;
                    int index = Projectile.NewProjectile(NPC.GetSource_FromAI(), NPC.Center + dir * (10 * 16f), Vector2.Zero,
                        ModContent.ProjectileType<EmpathyDirLaser>(), 300, 0f, Main.myPlayer, NPC.whoAmI, packed);
                    if (index >= 0 && index < Main.maxProjectiles)
                    {
                        Main.projectile[index].localAI[0] = dir.ToRotation() + MathHelper.PiOver2;
                        NetMessage.SendData(MessageID.SyncProjectile, -1, -1, null, index);
                    }
                }
            }
        }

        private static Vector2[] Directions()
        {
            return new[]
            {
                new Vector2(1, 0), new Vector2(-1, 0), new Vector2(0, -1), new Vector2(0, 1),
                new Vector2(1, -1).SafeNormalize(Vector2.UnitX), new Vector2(1, 1).SafeNormalize(Vector2.UnitX),
                new Vector2(-1, -1).SafeNormalize(Vector2.UnitX), new Vector2(-1, 1).SafeNormalize(Vector2.UnitX),
            };
        }

        private void PMiniBosses(int slot, Player target, bool primary)
        {
            if (Begin(slot, 200))
            {
                if (Server)
                {
                    int[] types = { NPCID.KingSlime, NPCID.QueenSlimeBoss, NPCID.QueenBee, NPCID.Golem, NPCID.Deerclops };
                    List<int> pool = new(types);
                    for (int i = 0; i < 2; i++)
                    {
                        int pick = Main.rand.Next(pool.Count);
                        int index = NPC.NewNPC(NPC.GetSource_FromAI(),
                            (int)(NPC.Center.X + (i == 0 ? -20 : 20) * 16f), (int)NPC.Center.Y, pool[pick]);
                        pool.RemoveAt(pick);

                        if (index >= 0 && index < Main.maxNPCs)
                        {
                            NPC mini = Main.npc[index];
                            mini.lifeMax = 2_000_000;
                            mini.life = 2_000_000;
                            mini.damage = 400;
                            mini.defDefense = 0;
                            mini.defense = 0;
                            mini.color = new Color(255, 40, 40, 200);
                            mini.netUpdate = true;
                        }
                    }
                }
            }
            if (primary)
                NPC.velocity *= 0.9f;
        }

        private void PCrosses(int slot, Player target, bool primary)
        {
            if (Begin(slot, 300))
            {
                if (Server)
                {
                    int radius = 500 * 16;
                    for (int i = 0; i < 25; i++)
                    {
                        Vector2 pos = target.Center + new Vector2(
                            Main.rand.NextFloat(-radius, radius), Main.rand.NextFloat(-radius, radius));
                        Projectile.NewProjectile(NPC.GetSource_FromAI(), pos, Vector2.Zero,
                            ModContent.ProjectileType<EmpathyCross>(), 500, 0f, Main.myPlayer);
                    }
                }
            }
            if (primary)
                Chase(target, 11f, 0.05f);
        }

        private void PBigScythe(int slot, Player target, bool primary)
        {
            if (Begin(slot, 220))
            {
                if (Server)
                {
                    int index = Projectile.NewProjectile(NPC.GetSource_FromAI(), NPC.Center, Vector2.Zero,
                        ModContent.ProjectileType<EmpathyScytheP13>(), 0, 0f, Main.myPlayer);
                    if (index >= 0 && index < Main.maxProjectiles)
                    {
                        Main.projectile[index].localAI[0] = 1f;
                        Main.projectile[index].ai[0] = NPC.whoAmI;
                        _state[slot] = index;
                        NetMessage.SendData(MessageID.SyncProjectile, -1, -1, null, index);
                    }
                }
            }

            if (primary)
                NPC.velocity *= 0.9f;

            if (_timer[slot] == 60)
            {
                int windup = (int)_state[slot];
                if (windup >= 0 && windup < Main.maxProjectiles && Main.projectile[windup].active)
                    Main.projectile[windup].Kill();

                if (Server)
                {
                    Vector2 dir = (target.Center - NPC.Center).SafeNormalize(Vector2.UnitX);
                    Vector2 aim = target.Center + dir * (50 * 16f);
                    int index = Projectile.NewProjectile(NPC.GetSource_FromAI(), NPC.Center,
                        (aim - NPC.Center) / 120f, ModContent.ProjectileType<EmpathyScytheP13>(),
                        800, 0f, Main.myPlayer);
                    if (index >= 0 && index < Main.maxProjectiles)
                    {
                        Main.projectile[index].localAI[0] = 2f;
                        Main.projectile[index].localAI[1] = 120f;
                        Main.projectile[index].ai[0] = NPC.whoAmI;
                        NetMessage.SendData(MessageID.SyncProjectile, -1, -1, null, index);
                    }
                }
            }
        }

        private void PStarRing(int slot, Player target, bool primary)
        {
            if (Begin(slot, 400))
                Sound("em2");

            if (primary)
                Chase(target, 7f, 0.05f);

            if (_timer[slot] % 60 == 0 && Server)
            {
                float baseAngle = _timer[slot] * 0.05f;
                for (int i = 0; i < 5; i++)
                {
                    float a = baseAngle + MathHelper.TwoPi * i / 5f;
                    Projectile.NewProjectile(NPC.GetSource_FromAI(),
                        target.Center + a.ToRotationVector2() * 520f, Vector2.Zero,
                        ModContent.ProjectileType<EmpathyCross>(), 500, 0f, Main.myPlayer);
                }
            }
        }

        public override bool PreDraw(SpriteBatch spriteBatch, Vector2 screenPos, Color drawColor)
        {
            Texture2D tex = _useAltSprite
                ? ModContent.Request<Texture2D>("flameout/Content/Empathy2", ReLogic.Content.AssetRequestMode.ImmediateLoad).Value
                : ModContent.Request<Texture2D>("flameout/Content/Empathy", ReLogic.Content.AssetRequestMode.ImmediateLoad).Value;

            Vector2 origin = new Vector2(tex.Width * 0.5f, tex.Height * 0.5f);
            SpriteEffects fx = NPC.spriteDirection == -1 ? SpriteEffects.FlipHorizontally : SpriteEffects.None;

            for (int i = 4; i >= 1; i--)
            {
                Vector2 ghost = NPC.Center - NPC.velocity * i * 1.6f - screenPos;
                Color c = new Color(255, 30, 40) * (0.18f * (5 - i) / 4f);
                spriteBatch.Draw(tex, ghost, null, c, NPC.rotation, origin, NPC.scale * (1f + i * 0.05f), fx, 0f);
            }

            float pulse = 0.5f + 0.5f * (float)Math.Sin(Main.GlobalTimeWrappedHourly * 6f);
            Color tint = Color.Lerp(new Color(255, 90, 90), Color.White, pulse * 0.4f);
            spriteBatch.Draw(tex, NPC.Center - screenPos, null, tint, NPC.rotation, origin, NPC.scale * 1.15f, fx, 0f);

            return false;
        }

        public override void FindFrame(int frameHeight) { }
    }

    public class Empathium : ModItem
    {
        public override void SetDefaults()
        {
            Item.width = 22;
            Item.height = 22;
            Item.maxStack = Item.CommonMaxStack;
            Item.material = true;
            Item.rare = ItemRarityID.Red;
            Item.value = Item.sellPrice(0, 20, 0, 0);
        }
    }

    
    
    public class TrueEmpathyHunt : ModSystem
    {
        public static bool HuntEnabled = false;

        public static bool Hunting;

        private int _delay;

        public override void OnWorldLoad()
        {
            _delay = Hunting ? 420 : 0;
            TrueEmpathyUI.Stage = 0;
        }

        public override void OnWorldUnload()
        {
            TrueEmpathyUI.Stage = 0;
        }

        public override void Unload()
        {
            Hunting = false;
            TrueEmpathyUI.Stage = 0;
        }

        public override void PostUpdateWorld()
        {
            
            if (!NPC.AnyNPCs(ModContent.NPCType<TrueEmpathy>()))
                TrueEmpathyUI.Stage = 0;

            if (!HuntEnabled || !Hunting || Main.netMode == NetmodeID.MultiplayerClient)
                return;

            if (NPC.AnyNPCs(ModContent.NPCType<TrueEmpathy>()))
                return;

            if (_delay > 0)
            {
                _delay--;
                if (_delay == 120 && !Main.dedServ)
                {
                    SoundEngine.PlaySound(SoundID.Thunder);
                    Main.NewText("It found you again.", 255, 40, 40);
                }
                return;
            }

            for (int i = 0; i < Main.maxPlayers; i++)
            {
                Player p = Main.player[i];
                if (!p.active || p.dead)
                    continue;

                NPC.NewNPC(new Terraria.DataStructures.EntitySource_WorldEvent(),
                    (int)p.Center.X, (int)(p.Center.Y - 240f), ModContent.NPCType<TrueEmpathy>());
                _delay = 600;
                break;
            }
        }
    }

    public class TrueEmpathyUI : ModSystem
    {
        public static int Stage;

        private static readonly string[] Order =
        {
            "Vanilla: Map / Minimap",
            "Vanilla: Info Accessories Bar",
            "Vanilla: Resource Bars",
            "Vanilla: Inventory",
        };

        public override void ModifyInterfaceLayers(List<GameInterfaceLayer> layers)
        {
            int count = Math.Min(Stage, Order.Length);
            for (int i = 0; i < count; i++)
            {
                string name = Order[i];
                layers.RemoveAll(l => l.Name == name);
            }
        }
    }
}

