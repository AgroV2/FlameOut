using Microsoft.Xna.Framework;
using System;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using flameout.Content;

using Microsoft.Xna.Framework.Graphics;


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

        
        
        
        

        private Vector2 spawnPosition;
        private bool spawnSet = false;

        private int[] attack2LaserIndices = new int[4] { -1, -1, -1, -1 };
        private int attack3LaserIndex = -1;

        
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
            NPC.HitSound = SoundID.NPCHit1;
            NPC.DeathSound = SoundID.NPCDeath1;
            NPC.aiStyle = -1;
            NPC.npcSlots = 15f;
            
             Music = MusicLoader.GetMusicSlot(Mod, "flameout/Content/Music/ApathyMusic");
        }

        public override void OnSpawn(Terraria.DataStructures.IEntitySource source)
        {
            spawnPosition = NPC.Center;
            spawnSet = true;
        }

        
        public override void SendExtraAI(System.IO.BinaryWriter writer)
        {
            for (int i = 0; i < 4; i++)
                writer.Write(attack2LaserIndices[i]);
            writer.Write(attack3LaserIndex);
        }

        public override void ReceiveExtraAI(System.IO.BinaryReader reader)
        {
            for (int i = 0; i < 4; i++)
                attack2LaserIndices[i] = reader.ReadInt32();
            attack3LaserIndex = reader.ReadInt32();
        }

        public override void AI()
        {
              if (Main.netMode != NetmodeID.Server)
        Music = MusicLoader.GetMusicSlot(Mod, "Content/Music/ApathyMusic");
            
            if (spawnSet)
                NPC.Center = spawnPosition;
            NPC.velocity = Vector2.Zero;

            NPC.TargetClosest(false);
            Player target = Main.player[NPC.target];

            if (!target.active || target.dead)
                return;
                 if (!target.active || target.dead)
    {
        NPC.active = false; 
        return;
    }

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
            }

            NPC.ai[1]++;
        }

        
        
        
      

private const int IDLE_PAUSE = 90; 

private void DoIdle(Player target)
{
    NPC.rotation = NPC.Center.DirectionTo(target.Center).ToRotation();

    if ((int)NPC.ai[3] == 1)
    {
        if ((int)NPC.ai[1] >= IDLE_PAUSE)
        {
            NPC.ai[3] = 0;
            NPC.ai[1] = -1; 
            NPC.ai[0] = Main.rand.Next(1, 4);
            NPC.netUpdate = true;
        }
    }
}

private void ReturnToIdle()
{
    NPC.ai[0] = STATE_IDLE;
    NPC.ai[1] = 0;
    NPC.ai[2] = 0;
    NPC.ai[3] = 1; 
    NPC.netUpdate = true;
}




public override bool PreDraw(SpriteBatch spriteBatch, Vector2 screenPos, Color drawColor)
{
    int state = (int)NPC.ai[0];

    
    string texPath = (state == STATE_ATTACK2)
        ? "flameout/Content/ApathyBoss_Attack2"
        : "flameout/Content/ApathyBoss_Attack1";

    Texture2D tex = ModContent.Request<Texture2D>(texPath).Value;
    Vector2 origin = new Vector2(tex.Width / 2f, tex.Height / 2f);
    Vector2 drawPos = NPC.Center - screenPos;

    
    Color glowColor = (state == STATE_ATTACK2)
        ? new Color(255, 80, 180) * 0.4f   
        : new Color(120, 40, 220) * 0.4f;  

    spriteBatch.Draw(tex, drawPos, null, glowColor, NPC.rotation, origin, NPC.scale * 1.3f, SpriteEffects.None, 0f);

    
    spriteBatch.Draw(tex, drawPos, null, drawColor, NPC.rotation, origin, NPC.scale, SpriteEffects.None, 0f);

    return false;
}

private void TryActivate()
{
    if ((int)NPC.ai[0] == STATE_IDLE && (int)NPC.ai[3] == 0)
    {
        NPC.ai[3] = 1;
        NPC.ai[1] = 0;
        NPC.netUpdate = true;
    }
}

        
        
        
       
        private void PickRandomAttack()
        {
            NPC.ai[0] = Main.rand.Next(1, 4); 
            NPC.ai[1] = 0; 
            NPC.ai[2] = 0;
            NPC.netUpdate = true;
        }

        
        
        
        private void DoAttack1(Player target, int timer)
        {
            NPC.rotation = NPC.Center.DirectionTo(target.Center).ToRotation();

            if (timer == 0)
            {
                if (Main.netMode != NetmodeID.MultiplayerClient)
                {
                    Vector2 dir = NPC.Center.DirectionTo(target.Center);
                    Vector2 vel = dir * 12f;
                    Projectile.NewProjectile(
                        NPC.GetSource_FromAI(),
                        NPC.Center,
                        vel,
                        ModContent.ProjectileType<ApathyBigLaser>(),
                        9999,
                        0f,
                        Main.myPlayer
                    );
                }
            }

            if (timer >= 480)
            {
                ReturnToIdle();
               
            }
        }
        
public override void OnHitByProjectile(Projectile projectile, NPC.HitInfo hit, int damageDone)
{
    TryActivate();
}

public override void OnHitByItem(Player player, Item item, NPC.HitInfo hit, int damageDone)
{
    TryActivate();
}


        
        
        
        private void DoAttack2(Player target, int timer)
        {
            NPC.rotation = NPC.Center.DirectionTo(target.Center).ToRotation();

            if (timer == 0)
            {
                if (Main.netMode != NetmodeID.MultiplayerClient)
                {
                    Vector2[] directions = new Vector2[]
                    {
                        new Vector2(0f, -1f),
                        new Vector2(0f,  1f),
                        new Vector2(-1f, 0f),
                        new Vector2(1f,  0f)
                    };

                    for (int i = 0; i < 4; i++)
                    {
                        int idx = Projectile.NewProjectile(
                            NPC.GetSource_FromAI(),
                            NPC.Center,
                            Vector2.Zero,
                            ModContent.ProjectileType<ApathyStaticBeam>(),
                            9999,
                            0f,
                            Main.myPlayer
                        );
                        if (idx < Main.maxProjectiles)
                        {
                            Main.projectile[idx].ai[0] = directions[i].X;
                            Main.projectile[idx].ai[1] = directions[i].Y;
                            attack2LaserIndices[i] = idx;
                        }
                    }
                }
            }

            
            if (timer % 4 == 0 && Main.netMode != NetmodeID.MultiplayerClient)
            {
                int count = 12;
                for (int i = 0; i < count; i++)
                {
                    float angle = MathHelper.TwoPi / count * i;
                    Vector2 vel = new Vector2((float)Math.Cos(angle), (float)Math.Sin(angle)) * 8f;
                    Projectile.NewProjectile(
                        NPC.GetSource_FromAI(),
                        NPC.Center,
                        vel,
                        ModContent.ProjectileType<ApathyParticle>(),
                        9999,
                        0f,
                        Main.myPlayer
                    );
                }
            }

            if (timer >= 360)
            {
                
                if (Main.netMode != NetmodeID.MultiplayerClient)
                {
                    for (int i = 0; i < 4; i++)
                    {
                        if (attack2LaserIndices[i] >= 0 && attack2LaserIndices[i] < Main.maxProjectiles)
                            Main.projectile[attack2LaserIndices[i]].Kill();
                        attack2LaserIndices[i] = -1;
                    }
                }
                ReturnToIdle();
              
            }
        }

        
        
        
        private void DoAttack3(Player target, int timer)
        {
            NPC.rotation = NPC.Center.DirectionTo(target.Center).ToRotation();

            if (timer == 0)
            {
                NPC.ai[2] = 0f; 
                if (Main.netMode != NetmodeID.MultiplayerClient)
                {
                    int idx = Projectile.NewProjectile(
                        NPC.GetSource_FromAI(),
                        NPC.Center,
                        Vector2.Zero,
                        ModContent.ProjectileType<ApathySpinLaser>(),
                        9999,
                        0f,
                        Main.myPlayer
                    );
                    if (idx < Main.maxProjectiles)
                        attack3LaserIndex = idx;
                }
            }

            
            float speed = MathHelper.Lerp(0.02f, 0.10f, timer / 600f);
            NPC.ai[2] += speed;

            if (attack3LaserIndex >= 0 && attack3LaserIndex < Main.maxProjectiles)
            {
                Projectile p = Main.projectile[attack3LaserIndex];
                if (p.active && p.type == ModContent.ProjectileType<ApathySpinLaser>())
                {
                    p.ai[0] = NPC.ai[2]; 
                    p.Center = NPC.Center;
                }
            }

            if (timer >= 600)
            {
                if (Main.netMode != NetmodeID.MultiplayerClient)
                {
                    if (attack3LaserIndex >= 0 && attack3LaserIndex < Main.maxProjectiles)
                        Main.projectile[attack3LaserIndex].Kill();
                    attack3LaserIndex = -1;
                }
                ReturnToIdle();
                
            }
        }


        public override void OnKill()
        {
            if (Main.netMode != NetmodeID.MultiplayerClient)
            {
                Projectile.NewProjectile(
                    NPC.GetSource_Death(),
                    spawnPosition,
                    Vector2.Zero,
                    ModContent.ProjectileType<ApathyDeathEffect>(),
                    0,
                    0f,
                    Main.myPlayer
                );
            }
            ApathyWorld.downedApathy = true;
        }

        public override bool CheckActive() => false;
    }
}