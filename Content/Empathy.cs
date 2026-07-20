
using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.Audio;
using System.Collections.Generic;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.DataStructures;
using flameout.Content;
using Terraria.GameContent;
using Terraria.GameContent.ItemDropRules;
namespace flameout.Content
{
    [AutoloadBossHead]
    public class Empathy : ModNPC
    {
        
        
        
        
        private EmpathyPhase2Controller _p2;
        public bool Phasee2 = false;
        private int _phase = -1;           
        private bool _startedFinalPhase = false;
private bool _canDie = false;
private EmpathyAttackFinal _attackFinal = null;
        private int _phaseTimer = 0;       
        private int _pauseTimer = 0;       
        private const int PauseDuration = 60; 
        private bool _phaseInit = false;   
private EmpathyAttack50Percent _attack50 = null;
private bool _attack50Triggered = false;
private bool _isInvulnerable = false;
        
        private float _desiredDist = 50f * 16f; 

        
        private int _swordProj = -1;

    private int _lastPhase1 = -1; 
private int _lastPhase2 = -1; 

private void ChooseNextPhase()
{
    int next;
    do
    {
        next = Main.rand.Next(1, 16);
    }
    while (next == _lastPhase1 || next == _lastPhase2);

    
    _lastPhase2 = _lastPhase1;
    _lastPhase1 = next;

    _phase = next;
    _phaseTimer = 0;
    _phaseInit = false;
}

public override bool CheckDead()
{
    if (!_startedFinalPhase)
    {
        _startedFinalPhase = true;
        NPC.life = 1;
        NPC.dontTakeDamage = true;
        
        _attackFinal = new EmpathyAttackFinal(NPC);
        _attackFinal.Init();
        
        NPC.netUpdate = true;
        return false; 
    }

    return _canDie; 
}
 public override void ModifyNPCLoot(NPCLoot npcLoot)
{
    npcLoot.Add(ItemDropRule.Common(ModContent.ItemType<EmpathySoul>()));  
}


private void CheckDistance(Player target)
{
    float dist = Vector2.Distance(NPC.Center, target.Center);
    if (dist > 500f * 16f) 
    {
        
        NPC.Center = target.Center + new Vector2(100f, 0f);
        NPC.netUpdate = true;

        SoundEngine.PlaySound(
            new SoundStyle("flameout/Content/Sounds/em1"),
            NPC.Center);
    }
}
        public override void SetStaticDefaults()
{
    NPCID.Sets.BossHeadTextures[NPC.type] = ModContent.GetModBossHeadSlot("flameout/Content/Empathy_Head_Boss");

    Main.npcFrameCount[NPC.type] = 1;
    NPCID.Sets.MPAllowedEnemies[NPC.type] = true;
    NPCID.Sets.BossBestiaryPriority.Add(NPC.type);

    if (Main.netMode != NetmodeID.Server)
    {
        ModContent.Request<Texture2D>("flameout/Content/Empathy", ReLogic.Content.AssetRequestMode.ImmediateLoad);
        ModContent.Request<Texture2D>("flameout/Content/Empathy2", ReLogic.Content.AssetRequestMode.ImmediateLoad);
    }
}
       public override bool PreDraw(SpriteBatch spriteBatch, Vector2 screenPos, Color drawColor)
{

    Texture2D tex = _useAltSprite
        ? ModContent.Request<Texture2D>("flameout/Content/Empathy2", ReLogic.Content.AssetRequestMode.ImmediateLoad).Value
        : ModContent.Request<Texture2D>("flameout/Content/Empathy", ReLogic.Content.AssetRequestMode.ImmediateLoad).Value;

    Vector2 pos = NPC.Center - screenPos;
    SpriteEffects fx = NPC.spriteDirection == -1
        ? SpriteEffects.FlipHorizontally
        : SpriteEffects.None;

    spriteBatch.Draw(tex, pos, null, drawColor, NPC.rotation,
        new Vector2(tex.Width * 0.5f, tex.Height * 0.5f),
        NPC.scale, fx, 0f);
        if (_phase == 12 && NPC.localAI[2] > 0f && NPC.localAI[2] <= 120f)
{
    

if (NPC.localAI[2] > 0f || NPC.localAI[2] <= 120f)
{
    Texture2D pixel = TextureAssets.MagicPixel.Value;
    Vector2 drawPos = NPC.Center - Main.screenPosition;

    float progress = NPC.localAI[2] / 120f;
    float pulse = (float)Math.Sin(progress * MathHelper.Pi * 4f) * 0.5f + 0.5f;
    float radius = 100f + pulse * 60f;
    Color ringColor = new Color(255, 0, 0, 0) * pulse;

    int segments = 32;
    for (int i = 0; i < segments; i++)
    {
        float angle = MathHelper.TwoPi * i / segments;
        float nextAngle = MathHelper.TwoPi * (i + 1) / segments;

        Vector2 p1 = drawPos + new Vector2((float)Math.Cos(angle), (float)Math.Sin(angle)) * radius;
        Vector2 p2 = drawPos + new Vector2((float)Math.Cos(nextAngle), (float)Math.Sin(nextAngle)) * radius;

        Vector2 mid = (p1 + p2) / 2f;
        float segLen = Vector2.Distance(p1, p2);
        float segRot = (p2 - p1).ToRotation();

        Main.spriteBatch.Draw(pixel, mid, new Rectangle(0, 0, 1, 1),
            ringColor, segRot,
            new Vector2(0.5f, 0.5f),
            new Vector2(segLen, 4f),
            SpriteEffects.None, 0f);
    }
}
}
    return false;
}


 

       public override void SetDefaults()
{
    NPC.width = 40;
    NPC.height = 56;
    NPC.lifeMax = 5_000_000;
    NPC.damage = 300;
    NPC.defense = 50;
    NPC.knockBackResist = 0f;
    NPC.noGravity = true;
    NPC.noTileCollide = true;
    NPC.boss = true;
    NPC.npcSlots = 10f;
    NPC.HitSound = SoundID.NPCHit1;
    NPC.DeathSound = SoundID.NPCDeath1;
    NPC.value = Item.buyPrice(0, 50, 0, 0);
    Music = MusicLoader.GetMusicSlot(Mod, "flameout/Content/Music/EmpathyMusic");

    
    NPC.BossBar = ModContent.GetInstance<EmpathyBossBar>();
}
bool anyAlive = false;





 public override void OnKill()
        {
            ApathyWorld.downedEmpathy = true;
            Phasee2 = false;
            _p2?.End();   
            _p2 = null;
        }
        public override void AI()
        {
          if (Main.netMode != NetmodeID.Server)
        Music = MusicLoader.GetMusicSlot(Mod, "Content/Music/EmpathyMusic");
            
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
    if (_attackFinal != null)
    {
        if (_attackFinal.Update())
        {
            _attackFinal = null;
            _canDie = true; 
            NPC.dontTakeDamage = false;
            NPC.StrikeInstantKill(); 
        }
        return; 
    }
           if (!_attack50Triggered && NPC.life <= NPC.lifeMax / 2)
{
    _attack50Triggered = true;
    _attack50 = new EmpathyAttack50Percent(NPC);
    _attack50.Init();
}

if (_attack50 != null)
{
    _isInvulnerable = true;
    bool done = _attack50.Update();
    if (done)
    {
        _attack50 = null;
        _isInvulnerable = false;
    }
    return; 
}
        if (NPC.life <= 0)
{
    NPC.life = 1;
    NPC.netUpdate = true;
}

        int g1Count = 0;
int g2Count = 0;
foreach (NPC n in Main.npc)
{
    if (!n.active) continue;
    if (n.type == ModContent.NPCType<Guardian1>()) g1Count++;
    if (n.type == ModContent.NPCType<Guardian2>()) g2Count++;
}


float damageMult = 1f + g1Count * 0.2f;
NPC.damage = (int)(300 * damageMult); 



NPC.defense = 50 + g2Count * 10; 
            NPC.TargetClosest(true);
           
Player target = Main.player[NPC.target];
HandleDarkScreen(); 
 CheckPlayersAlive();
 CheckDistance(target);


NPC.spriteDirection = target.Center.X < NPC.Center.X ? -1 : 1;


            
            if (_phase == -1)
            {
                NPC.velocity *= 0.9f;
                _pauseTimer++;
                if (_pauseTimer >= PauseDuration)
                {
                    _pauseTimer = 0;
                    ChooseNextPhase();
                }
                return;
            }

            _phaseTimer++;

            switch (_phase)
            {
                case 1:  Phase1(target); break;
                case 2:  Phase2(target); break;
                case 3:  Phase3(target); break;
                case 4:  Phase4(target); break;
                case 5:  Phase5(target); break;
                 case 6: Phase6(target); break;
             case 7:  Phase7(target); break;
                case 8:  Phase8(target); break;
                case 9:  Phase9(target); break;
                 case 10: Phase10(target); break;
                 case 11: Phase11(target); break;
                 case 12: Phase12(target); break;
                 case 13: Phase13(target); break;
                 case 14: Phase14(target); break;
                 case 15: Phase15(target); break;
            }
              
    foreach (Player p in Main.player)
    {
        if (p.active && !p.dead)
        {
            anyAlive = true;
            break;
        }
    }

    if (!anyAlive)
    {
        NPC.life = NPC.lifeMax;
        NPC.netUpdate = true;
    }

            if (_darkActive)
{
    _darkTimer++;

    
    
    if (_darkTimer == 1)
    {
        
        if (Main.netMode != NetmodeID.MultiplayerClient)
        {
            foreach (Player p in Main.player)
            {
                if (p.active && !p.dead)
                {
                    p.KillMe(PlayerDeathReason.ByCustomReason(
                        $"do not."), 9999, 0);
                }
            }
        }
    }
        }
          
    float darkProgress = Math.Min(_darkTimer / 120f, 1f); 
    Main.ColorOfTheSkies = Color.Lerp(Main.ColorOfTheSkies, Color.Black, darkProgress);

    if (_darkTimer >= 120)
    {
        _darkActive = false;
        _darkTimer = 0;
    }
 

}

public override bool? CanBeHitByProjectile(Projectile projectile)
    => _isInvulnerable ? false : null;

public override bool? CanBeHitByItem(Player player, Item item)
    => _isInvulnerable ? false : null;
public int _darkTimer = 0;
public bool _darkActive = false;



private void HandleDarkScreen()
{
    if (!_darkActive) return;

    _darkTimer++;

    float darkProgress = Math.Min(_darkTimer / 60f, 1f);
    DarkScreenSystem.DarkAlpha = (float)Math.Sin(darkProgress * MathHelper.Pi);

    if (_darkTimer >= 120)
    {
        _darkActive = false;
        _darkTimer = 0;
        DarkScreenSystem.DarkAlpha = 0f;
    }
}
private void CheckPlayersAlive()
{
    bool anyAlive = false;
    foreach (Player p in Main.player)
    {
        if (p.active && !p.dead) { anyAlive = true; break; }
    }

    if (!anyAlive)
    {
        NPC.active = false; 
        NPC.netUpdate = true; 
        _attack50Triggered = false; 
    }
}


public class EmpathyBossBar : ModBossBar
{
    public override bool PreDraw(SpriteBatch spriteBatch, NPC npc, ref BossBarDrawParams drawParams)

    {
        return true; 
    }
}
public override bool CheckActive()
{
    return false;
}
      

private void Phase1(Player target)
{
    if (!_phaseInit)
    {
        _phaseInit = true;
        NPC.Center = target.Center - new Vector2(0f, 200f);
        NPC.netUpdate = true;
        SoundEngine.PlaySound(
            new SoundStyle("flameout/Content/Sounds/em1"),
            NPC.Center);

        if (Main.netMode != NetmodeID.MultiplayerClient)
        {
            Projectile.NewProjectile(
                NPC.GetSource_FromAI(),
                NPC.Center,
                Vector2.Zero,
                ModContent.ProjectileType<EmpathyStar>(),
                NPC.damage / 2,
                0f,
                Main.myPlayer,
                NPC.whoAmI);
        }
        
    }

    
    Vector2 toTarget = target.Center - NPC.Center;
    toTarget.Normalize();
    NPC.velocity = Vector2.Lerp(NPC.velocity, toTarget * 18f, 0.06f);

    if (_phaseTimer >= 900)
        EndPhase();
}

      
        private void Phase2(Player target)
        {
            if (!_phaseInit)
            {
                SoundEngine.PlaySound(
            new SoundStyle("flameout/Content/Sounds/em2"),
            NPC.Center);
                _phaseInit = true;
                if (Main.netMode != NetmodeID.MultiplayerClient)
                {
                    Projectile.NewProjectile(NPC.GetSource_FromAI(), NPC.Center, Vector2.Zero,
                        ModContent.ProjectileType<EmpathyPentagram>(),
                        NPC.damage / 3, 0f, Main.myPlayer, NPC.whoAmI, 0f);
                    Projectile.NewProjectile(NPC.GetSource_FromAI(), NPC.Center, Vector2.Zero,
                        ModContent.ProjectileType<EmpathyPentagram>(),
                        NPC.damage / 3, 0f, Main.myPlayer, NPC.whoAmI, MathHelper.Pi);
                }
            }

            float dist = Vector2.Distance(NPC.Center, target.Center);
            Vector2 toTarget = target.Center - NPC.Center;
            toTarget.Normalize();

            if (dist > _desiredDist + 20f)
                NPC.velocity = Vector2.Lerp(NPC.velocity, toTarget * 8f, 0.08f);
            else if (dist < _desiredDist - 20f)
                NPC.velocity = Vector2.Lerp(NPC.velocity, -toTarget * 8f, 0.08f);
            else
                NPC.velocity *= 0.85f;

            if (_phaseTimer >= 600)
                EndPhase();
        }


        private void Phase3(Player target)
        {
            if (!_phaseInit)
            {
                SoundEngine.PlaySound(
            new SoundStyle("flameout/Content/Sounds/em2"),
            NPC.Center);
                _phaseInit = true;
     
               
                float behindAngle = target.direction == 1
                    ? MathHelper.Pi
                    : 0f;
                NPC.Center = target.Center + new Vector2((float)Math.Cos(behindAngle), 0f) * 200f;
                NPC.netUpdate = true;

                if (Main.netMode != NetmodeID.MultiplayerClient)
                {
                    
                    
                    Projectile.NewProjectile(NPC.GetSource_FromAI(), NPC.Center, Vector2.Zero,
                        ModContent.ProjectileType<EmpathyPentagramLaser>(),
                        NPC.damage / 3, 0f, Main.myPlayer, NPC.whoAmI, 0f);
                    Projectile.NewProjectile(NPC.GetSource_FromAI(), NPC.Center, Vector2.Zero,
                        ModContent.ProjectileType<EmpathyPentagramLaser>(),
                        NPC.damage / 3, 0f, Main.myPlayer, NPC.whoAmI, MathHelper.Pi);
                }
            }

            float dist = Vector2.Distance(NPC.Center, target.Center);
Vector2 toTarget = target.Center - NPC.Center;
toTarget.Normalize();

if (dist > _desiredDist + 20f)
    NPC.velocity = Vector2.Lerp(NPC.velocity, toTarget * 8f, 0.08f);
else if (dist < _desiredDist - 20f)
    NPC.velocity = Vector2.Lerp(NPC.velocity, -toTarget * 8f, 0.08f);
else
    NPC.velocity *= 0.85f;

            if (_phaseTimer >= 600)
                EndPhase();
        }


        private bool _useAltSprite = false;
        public override void FindFrame(int frameHeight)
{

}
        private void Phase4(Player target)
        {
            if (!_phaseInit)
            {
                SoundEngine.PlaySound(
            new SoundStyle("flameout/Content/Sounds/em1"),
            NPC.Center);
                _phaseInit = true;
                if (Main.netMode != NetmodeID.MultiplayerClient)
                {
                    _swordProj = Projectile.NewProjectile(
                        NPC.GetSource_FromAI(), NPC.Center, Vector2.Zero,
                        ModContent.ProjectileType<EmpathySword>(),
                        NPC.damage, 0f, Main.myPlayer, NPC.whoAmI);
                }
            }

   
            Vector2 toTarget = target.Center - NPC.Center;
            toTarget.Normalize();
            NPC.velocity = Vector2.Lerp(NPC.velocity, toTarget * 10f, 0.07f);

  
            if (_swordProj >= 0 && (_swordProj >= Main.maxProjectiles || !Main.projectile[_swordProj].active))
                EndPhase();

            if (_phaseTimer >= 300)
                EndPhase();
        }


        private void Phase5(Player target)
        {
            if (!_phaseInit)
            {
                SoundEngine.PlaySound(
            new SoundStyle("flameout/Content/Sounds/em1"),
            NPC.Center);
            SoundEngine.PlaySound(
            new SoundStyle("flameout/Content/Sounds/em3"),
            NPC.Center);
                _phaseInit = true;
                
                Vector2[] offsets = new Vector2[]
                {
                    new Vector2(-300f, 0f),
                    new Vector2(300f, 0f),
                    new Vector2(0f, -300f),
                    new Vector2(0f, 300f)
                };
                NPC.Center = target.Center + offsets[Main.rand.Next(4)];
                NPC.netUpdate = true;

                if (Main.netMode != NetmodeID.MultiplayerClient)
                {
                    
                    float[] angles = new float[]
                    {
                        0f, MathHelper.PiOver2, MathHelper.Pi, MathHelper.Pi * 1.5f
                    };
                    foreach (float a in angles)
                    {
                        Projectile.NewProjectile(
                            NPC.GetSource_FromAI(), NPC.Center, Vector2.Zero,
                            ModContent.ProjectileType<EmpathyLaser>(),
                            NPC.damage / 2, 0f, Main.myPlayer, a, 3000f);
                    }
                }
            }

            NPC.velocity = Vector2.Zero;

            if (_phaseTimer >= 120)
                EndPhase();
        }
private int _scytheProj = -1;
private void Phase6(Player target)
{
    if (!_phaseInit)
    {
        SoundEngine.PlaySound(
            new SoundStyle("flameout/Content/Sounds/em2"),
            NPC.Center);
        _phaseInit = true;
        
        _useAltSprite = true;
        NPC.netUpdate = true;

        if (Main.netMode != NetmodeID.MultiplayerClient)
        {
            _scytheProj = Projectile.NewProjectile(
                NPC.GetSource_FromAI(),
                NPC.Center,
                Vector2.Zero,
                ModContent.ProjectileType<EmpathyScythe>(),
                NPC.damage,
                0f,
                Main.myPlayer,
                NPC.whoAmI);
        }
    }

    
    Vector2 toTarget = target.Center - NPC.Center;
    toTarget.Normalize();
    NPC.velocity = Vector2.Lerp(NPC.velocity, toTarget * 12f, 0.07f);

    if (_phaseTimer >= 480)
    {
        
        if (_scytheProj >= 0 && _scytheProj < Main.maxProjectiles && Main.projectile[_scytheProj].active)
            Main.projectile[_scytheProj].Kill();
        _scytheProj = -1;

        
        _useAltSprite = false;
        NPC.netUpdate = true;

        EndPhase();
    }
}

private void Phase7(Player target)
{
    if (!_phaseInit)
    {
        SoundEngine.PlaySound(
            new SoundStyle("flameout/Content/Sounds/em1"),
            NPC.Center);
        _phaseInit = true;

        
        if (Main.netMode != NetmodeID.MultiplayerClient)
        {
            
            Projectile.NewProjectile(
                NPC.GetSource_FromAI(),
                NPC.Center + new Vector2(-200f, 0f),
                Vector2.Zero,
                ModContent.ProjectileType<EmpathyPortal1>(),
                NPC.damage / 3,
                0f,
                Main.myPlayer,
                NPC.whoAmI);

            
            Projectile.NewProjectile(
                NPC.GetSource_FromAI(),
                NPC.Center + new Vector2(200f, 0f),
                Vector2.Zero,
                ModContent.ProjectileType<EmpathyPortal2>(),
                NPC.damage / 3,
                0f,
                Main.myPlayer,
                NPC.whoAmI);
        }
    }

    
    if (_phaseTimer >= 180) 
    {
        EndPhase();
    }
}

private void Phase8(Player target)
{
    if (!_phaseInit)
    {
        SoundEngine.PlaySound(
            new SoundStyle("flameout/Content/Sounds/em3"),
            NPC.Center);
        _phaseInit = true;

        if (Main.netMode != NetmodeID.MultiplayerClient)
        {
            
            NPC.NewNPC(
                NPC.GetSource_FromAI(),
                (int)(target.Center.X - 50f * 16f),
                (int)target.Center.Y,
                ModContent.NPCType<Guardian1>());

            
            NPC.NewNPC(
                NPC.GetSource_FromAI(),
                (int)(target.Center.X + 50f * 16f),
                (int)target.Center.Y,
                ModContent.NPCType<Guardian2>());
        }
    }

    
    if (_phaseTimer >= 120) 
        EndPhase();
}

private void Phase9(Player target)
{
    if (!_phaseInit)
    {
        _phaseInit = true;

        if (Main.netMode != NetmodeID.MultiplayerClient)
        {
            Projectile.NewProjectile(
                NPC.GetSource_FromAI(),
                NPC.Center,
                Vector2.Zero,
                ModContent.ProjectileType<EmpathyPortal>(),
                0,
                0f,
                Main.myPlayer,
                NPC.whoAmI);
        }
    }

    
    Vector2 dirToTarget = (target.Center - NPC.Center).SafeNormalize(Vector2.UnitX);
    NPC.velocity = dirToTarget * 8f;

    
    bool portalAlive = false;
    for (int i = 0; i < Main.maxProjectiles; i++)
    {
        if (Main.projectile[i].active &&
            Main.projectile[i].type == ModContent.ProjectileType<EmpathyPortal>() &&
            (int)Main.projectile[i].ai[0] == NPC.whoAmI)
        {
            portalAlive = true;
            break;
        }
    }

    if (!portalAlive)
        EndPhase();
}


private void Phase10(Player target)
{
    if (!_phaseInit)
    {
        _phaseInit = true;

        NPC.Center = target.Center + new Vector2(0f, -10 * 16f);
        NPC.velocity = Vector2.Zero;
        NPC.netUpdate = true;

        SoundEngine.PlaySound(
            new SoundStyle("flameout/Content/Sounds/em1"),
            NPC.Center);

        if (Main.netMode != NetmodeID.MultiplayerClient)
            SpawnOrbs();
    }

    NPC.velocity = Vector2.Zero;

    
    if (_phaseTimer >= 120 && NPC.localAI[0] == 0f)
    {
        NPC.localAI[0] = 1f; 

        if (Main.netMode != NetmodeID.MultiplayerClient)
            SpawnLasers();
    }

    
    if (_phaseTimer >= 200)
    {
        NPC.localAI[0] = 0f; 
        EndPhase();
    }
}

private void SpawnOrbs()
{
    Vector2[] directions = GetDirections();

    foreach (Vector2 dir in directions)
    {
        Projectile.NewProjectile(
            NPC.GetSource_FromAI(),
            NPC.Center + dir * (10 * 16f),
            Vector2.Zero,
            ModContent.ProjectileType<EmpathyOrb>(),
            0, 0f, Main.myPlayer,
            dir.X, dir.Y);
    }
}

private void SpawnLasers()
{
    Vector2[] directions = GetDirections();

    foreach (Vector2 dir in directions)
    {
        Color laserColor = new Color(
            Main.rand.Next(100, 255),
            Main.rand.Next(100, 255),
            Main.rand.Next(100, 255));

        int packed = (laserColor.R << 16) | (laserColor.G << 8) | laserColor.B;
        float angle = dir.ToRotation() + MathHelper.PiOver2;

        int index = Projectile.NewProjectile(
            NPC.GetSource_FromAI(),
            NPC.Center + dir * (10 * 16f),
            Vector2.Zero,
            ModContent.ProjectileType<EmpathyDirLaser>(),
            300, 0f, Main.myPlayer,
            NPC.whoAmI, packed);

        if (index >= 0 && index < Main.maxProjectiles)
        {
            Main.projectile[index].localAI[0] = angle;
            NetMessage.SendData(MessageID.SyncProjectile, -1, -1, null, index);
        }
    }
}

private Vector2[] GetDirections()
{
    return new Vector2[]
    {
        new Vector2(1, 0),
        new Vector2(-1, 0),
        new Vector2(0, -1),
        new Vector2(0, 1),
        new Vector2(1, -1).SafeNormalize(Vector2.UnitX),
        new Vector2(1, 1).SafeNormalize(Vector2.UnitX),
        new Vector2(-1, -1).SafeNormalize(Vector2.UnitX),
        new Vector2(-1, 1).SafeNormalize(Vector2.UnitX),
    };
}

private void Phase11(Player target)
{
    if (!_phaseInit)
    {
        _phaseInit = true;

        if (Main.netMode != NetmodeID.MultiplayerClient)
            SpawnMiniBosses();
    }

    NPC.velocity = Vector2.Zero;

    
    if (_phaseTimer >= 180)
        EndPhase();
}

private void SpawnMiniBosses()
{
    int[] bossTypes = new int[]
    {
        NPCID.KingSlime,
        NPCID.QueenSlimeBoss,
        NPCID.QueenBee,
        NPCID.Golem,
        NPCID.Deerclops
    };

    
    List<int> pool = new List<int>(bossTypes);
    int[] chosen = new int[2];

    for (int i = 0; i < 2; i++)
    {
        int pick = Main.rand.Next(pool.Count);
        chosen[i] = pool[pick];
        pool.RemoveAt(pick);
    }

    Vector2[] spawnOffsets = new Vector2[]
    {
        new Vector2(-20 * 16f, 0f),
        new Vector2(20 * 16f, 0f),
    };

    for (int i = 0; i < 2; i++)
    {
        int index = NPC.NewNPC(
            NPC.GetSource_FromAI(),
            (int)(NPC.Center.X + spawnOffsets[i].X),
            (int)(NPC.Center.Y + spawnOffsets[i].Y),
            chosen[i]);

        if (index >= 0 && index < Main.maxNPCs)
        {
            NPC miniBoss = Main.npc[index];

            
            miniBoss.lifeMax = 1_000_000;
            miniBoss.life = 1_000_000;
            miniBoss.damage = 200;
            miniBoss.defDefense = 0;
            miniBoss.defense = 0;

            
            miniBoss.color = new Color(180, 50, 255, 200);

            miniBoss.netUpdate = true;
        }
    }
}
public static bool Phase12Active = false;
private void Phase12(Player target)
{
    if (!_phaseInit)
    {
        _phaseInit = true;
        Phase12Active = true;
        NPC.localAI[1] = 0f; 
    }

    NPC.velocity = Vector2.Zero;

    
    if (_phaseTimer % 120 == 0)
    {
        SoundEngine.PlaySound(
            new SoundStyle("flameout/Content/Sounds/em3") { IsLooped = true },
            NPC.Center);
    }

    
    
    NPC.localAI[2] = Math.Min(_phaseTimer, 120f);

    
    if (_phaseTimer >= 120 || NPC.localAI[1] == 0f)
    {
        NPC.localAI[1] = 1f;

        if (Main.netMode != NetmodeID.MultiplayerClient)
            SpawnCrosses(target);
    }

    
    if (_phaseTimer >= 420)
    {
        Phase12Active = false;
        EndPhase();
    }
}
private void SpawnCrosses(Player target)
{
    int radius = 500 * 16;
    int count = 15; 

    for (int i = 0; i < count; i++)
    {
        Vector2 spawnPos = target.Center + new Vector2(
            Main.rand.NextFloat(-radius, radius),
            Main.rand.NextFloat(-radius, radius));

        Projectile.NewProjectile(
            NPC.GetSource_FromAI(),
            spawnPos,
            Vector2.Zero,
            ModContent.ProjectileType<EmpathyCross>(),
            500, 0f, Main.myPlayer);
    }
}


private void Phase13(Player target)
{
    if (!_phaseInit)
    {
        _phaseInit = true;
        NPC.velocity = Vector2.Zero;
        NPC.localAI[1] = 0f;
        NPC.localAI[2] = 0f;
        NPC.localAI[3] = -1f; 

        if (Main.netMode != NetmodeID.MultiplayerClient)
        {
            int index = Projectile.NewProjectile(
                NPC.GetSource_FromAI(),
                NPC.Center,
                Vector2.Zero,
                ModContent.ProjectileType<EmpathyScytheP13>(),
                0, 0f, Main.myPlayer);

            if (index >= 0 && index < Main.maxProjectiles)
            {
                Main.projectile[index].localAI[0] = 1f;
                Main.projectile[index].ai[0] = NPC.whoAmI;
                NPC.localAI[3] = index; 
                NetMessage.SendData(MessageID.SyncProjectile, -1, -1, null, index);
            }
        }
    }

    NPC.velocity = Vector2.Zero;

    if (_phaseTimer == 60 && NPC.localAI[1] == 0f)
    {
        NPC.localAI[1] = 1f;

        
        int windupIndex = (int)NPC.localAI[3];
        if (windupIndex >= 0 && windupIndex < Main.maxProjectiles)
            Main.projectile[windupIndex].Kill();

        if (Main.netMode != NetmodeID.MultiplayerClient)
        {
            Vector2 dirToPlayer = (target.Center - NPC.Center).SafeNormalize(Vector2.UnitX);
            Vector2 targetPos = target.Center + dirToPlayer * (50 * 16f);
            Vector2 velocity = (targetPos - NPC.Center) / 120f;

            int index = Projectile.NewProjectile(
                NPC.GetSource_FromAI(),
                NPC.Center,
                velocity,
                ModContent.ProjectileType<EmpathyScytheP13>(),
                400, 0f, Main.myPlayer);

            if (index >= 0 && index < Main.maxProjectiles)
            {
                Main.projectile[index].localAI[0] = 2f;
                Main.projectile[index].localAI[1] = 120f;
                Main.projectile[index].ai[0] = NPC.whoAmI;
                NetMessage.SendData(MessageID.SyncProjectile, -1, -1, null, index);
            }
        }
    }

    if (NPC.localAI[2] == 1f)
        EndPhase();
}

private EmpathyAttack14 _attack14;

private void Phase14(Player target)
{
    if (_attack14 == null)
    {
        _attack14 = new EmpathyAttack14(NPC);
        _attack14.Init();
    }

    bool done = _attack14.Update();

    if (done)
    {
        _attack14 = null;
        EndPhase();
    }
}

private EmpathyAttack15 _attack15;

private void Phase15(Player target)
{
    if (_attack15 == null)
    {
        _attack15 = new EmpathyAttack15(NPC);
        _attack15.Init();
    }

    bool done = _attack15.Update();

    if (done)
    {
        _attack15 = null;
        EndPhase();
    }
}


        
        
        
        private void PhaseStub(Player target, int duration)
        {
            NPC.velocity = Vector2.Zero;
            if (_phaseTimer >= duration)
                EndPhase();
        }

        private void EndPhase()
        {
            _phase = -1;
            _pauseTimer = 0;
            _phaseTimer = 0;
            _phaseInit = false;
            _swordProj = -1;
            _phaseInit = false;
    _phaseTimer = 0;
    NPC.localAI[2] = 0f;
        }
    }
}
 public class EmpathyCameraSystem : ModSystem
    {
        public override void ModifyScreenPosition()
        {
            
            for (int i = 0; i < Main.maxNPCs; i++)
            {
                NPC npc = Main.npc[i];
                
                
                
                if (npc.active && npc.type == ModContent.NPCType<Empathy>())
                {
                    
                    
                   if (npc.ai[3] == 1)
                    {
                        
                        Main.screenPosition = npc.Center - new Vector2(Main.screenWidth / 2f, Main.screenHeight / 2f);
                    }
                }
            }
        }
    }
   