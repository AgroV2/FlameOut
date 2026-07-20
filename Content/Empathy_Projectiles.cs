
using System;
using flameout.Content;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.DataStructures;
using Terraria.GameContent;
using Terraria.Graphics.CameraModifiers;
using System.Collections.Generic;
using Terraria.Audio;

namespace flameout.Content
{
    
    
    
    public class Empathy_Projectiles : ModProjectile
    {
        
public override void SetStaticDefaults()
{
    ProjectileID.Sets.DrawScreenCheckFluff[Type] = 10000;
}

        public override string Texture => "flameout/Content/EmpathyScytheShot";
        public override void SetDefaults()
        {
            Projectile.width = 8;
            Projectile.height = 8;
            Projectile.hostile = true;
            Projectile.friendly = false;
            Projectile.tileCollide = true;
            Projectile.ignoreWater = true;
            Projectile.timeLeft = 300;
            Projectile.damage = 300;
            Projectile.penetrate = 1;
            Projectile.alpha = 0;
            
Projectile.oldPos = new Vector2[12];

        }

        
public override bool PreDraw(ref Color lightColor)
{
    Texture2D texture = ModContent.Request<Texture2D>(Texture).Value;
    Vector2 origin = texture.Size() / 2f;
    Texture2D pixel = TextureAssets.MagicPixel.Value;
    Rectangle pixelRect = new Rectangle(0, 0, 1, 1);

    for (int i = 0; i < 12; i++)
    {
        Vector2 pos = Projectile.oldPos[i];
        if (pos == Vector2.Zero) continue;

        float progress = 1f - (i / 12f);
        float size = MathHelper.Lerp(1f, 6f, progress);

        Main.spriteBatch.Draw(
            pixel,
            pos + Projectile.Size / 2f - Main.screenPosition,
            pixelRect,
            Color.White * (progress * 0.6f),
            0f, new Vector2(0.5f), size,
            SpriteEffects.None, 0f);
    }

    for (int i = 0; i < 3; i++)
    {
        float scale = Projectile.scale * (1f + i * 0.2f);
        Main.spriteBatch.Draw(
            texture,
            Projectile.Center - Main.screenPosition,
            null,
            new Color(255, 255, 255, 0) * (0.2f - i * 0.05f),
            Projectile.rotation, origin, scale,
            SpriteEffects.None, 0f);
    }

    Main.spriteBatch.Draw(
        texture,
        Projectile.Center - Main.screenPosition,
        null, lightColor, Projectile.rotation,
        origin, Projectile.scale,
        SpriteEffects.None, 0f);

    return false;
}

    }

    
    
    
     public class EmpathyStar : ModProjectile
    {
         public override string Texture => "Terraria/Images/Projectile_0";
        private const float OuterRadius = 140f;
        private const float InnerRadius = 58f;
        private const float RotSpeed = 0.025f;

        
        private float Angle
        {
            get => Projectile.localAI[0];
            set => Projectile.localAI[0] = value;
        }

        public override void SetDefaults()
        {
            Projectile.width = 16;
            Projectile.height = 16;
            Projectile.hostile = true;
            Projectile.friendly = false;
            Projectile.tileCollide = false;
            Projectile.ignoreWater = true;
            Projectile.timeLeft = 900;
            Projectile.damage = 300;
            Projectile.penetrate = -1;
            Projectile.alpha = 255;
        }

       public override void AI()
{
    int ownerIndex = (int)Projectile.ai[0];

    if (ownerIndex < 0 ||
        ownerIndex >= Main.maxNPCs ||
        !Main.npc[ownerIndex].active)
    {
        Projectile.Kill();
        return;
    }

    NPC boss = Main.npc[ownerIndex];

    Angle += RotSpeed;
    Projectile.Center = boss.Center;
    Projectile.velocity = Vector2.Zero;

    
    if (Main.netMode != NetmodeID.MultiplayerClient)
    {
        Projectile.localAI[1]++;

        if (Projectile.localAI[1] >= 8f * 60f)
        {
            Projectile.localAI[1] = 0f;
             SoundEngine.PlaySound(
            new SoundStyle("flameout/Content/Sounds/em3"),
            Projectile.Center);
            if (boss.target < 0 ||
                boss.target >= Main.maxPlayers ||
                !Main.player[boss.target].active ||
                Main.player[boss.target].dead)
            {
                boss.TargetClosest(false);
            }

            Player target = Main.player[boss.target];

            if (target.active && !target.dead)
            {
                const float teleportDistance = 70f * 16f;

                float angle = Main.rand.NextFloat(MathHelper.TwoPi);
                Vector2 offset = angle.ToRotationVector2() * teleportDistance;

                boss.Center = target.Center + offset;
                boss.velocity = Vector2.Zero;
                boss.netUpdate = true;

                Projectile.Center = boss.Center;
                Projectile.netUpdate = true;
            }
        }
    }
}

        private Vector2[] GetStarPointsWorld()
        {
            Vector2 center = Projectile.Center;
            Vector2[] pts = new Vector2[10];
            for (int i = 0; i < 10; i++)
            {
                float r = (i % 2 == 0) ? OuterRadius : InnerRadius;
                float a = Angle + (MathHelper.TwoPi / 10f) * i - MathHelper.PiOver2;
                pts[i] = center + new Vector2((float)Math.Cos(a), (float)Math.Sin(a)) * r;
            }
            return pts;
        }

        private Vector2[] GetStarPointsScreen()
        {
            Vector2 center = Projectile.Center - Main.screenPosition;
            Vector2[] pts = new Vector2[10];
            for (int i = 0; i < 10; i++)
            {
                float r = (i % 2 == 0) ? OuterRadius : InnerRadius;
                float a = Angle + (MathHelper.TwoPi / 10f) * i - MathHelper.PiOver2;
                pts[i] = center + new Vector2((float)Math.Cos(a), (float)Math.Sin(a)) * r;
            }
            return pts;
        }

        public override bool? Colliding(Rectangle projHitbox, Rectangle targetHitbox)
        {
            Vector2 center = Projectile.Center;
            Vector2[] pts = GetStarPointsWorld();
            float _ = 0f;

            
            for (int i = 0; i < 10; i += 2)
            {
                if (Collision.CheckAABBvLineCollision(
                    targetHitbox.TopLeft(), targetHitbox.Size(),
                    center, pts[i], 18f, ref _))
                    return true;
            }

            
            for (int i = 0; i < 10; i++)
            {
                if (Collision.CheckAABBvLineCollision(
                    targetHitbox.TopLeft(), targetHitbox.Size(),
                    pts[i], pts[(i + 1) % 10], 12f, ref _))
                    return true;
            }

            return false;
            }

        public override bool PreDraw(ref Color lightColor)
        {
            SpriteBatch sb = Main.spriteBatch;
            Texture2D pixel = Terraria.GameContent.TextureAssets.MagicPixel.Value;
            Vector2 center = Projectile.Center - Main.screenPosition;
            Vector2[] pts = GetStarPointsScreen();

            sb.End();
            sb.Begin(SpriteSortMode.Deferred, BlendState.Additive, SamplerState.LinearClamp,
                DepthStencilState.None, RasterizerState.CullNone, null, Main.GameViewMatrix.TransformationMatrix);

            
            for (int i = 6; i >= 1; i--)
            {
                Color gc = new Color(255, 160, 220, 0) * (0.12f * (7 - i));
                sb.Draw(pixel, center, null, gc, 0f,
                    new Vector2(0.5f, 0.5f), i * 14f, SpriteEffects.None, 0f);
            }

            
            for (int i = 0; i < 10; i += 2)
            {
                DrawLine(sb, pixel, center, pts[i], new Color(255, 210, 240, 230), 5f);
                DrawLine(sb, pixel, center, pts[i], new Color(255, 180, 220, 60), 12f);
            }

            
            for (int i = 0; i < 10; i++)
            {
                DrawLine(sb, pixel, pts[i], pts[(i + 1) % 10], new Color(255, 200, 235, 180), 3f);
            }

            
            for (int i = 0; i < 10; i += 2)
            {
                for (int g = 3; g >= 1; g--)
                {
                    Color gc = new Color(255, 220, 245, 0) * (0.25f * (4 - g));
                    sb.Draw(pixel, pts[i], null, gc, 0f,
                        new Vector2(0.5f, 0.5f), g * 8f, SpriteEffects.None, 0f);
                }
            }

            sb.End();
            sb.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.LinearClamp,
                DepthStencilState.None, RasterizerState.CullNone, null, Main.GameViewMatrix.TransformationMatrix);
            return false;
        }

               private void DrawLine(SpriteBatch sb, Texture2D pixel, Vector2 from, Vector2 to, Color color, float thickness)
        {
            Vector2 diff = to - from;
            float angle = (float)Math.Atan2(diff.Y, diff.X);
            float length = diff.Length();
            sb.Draw(pixel, from, null, color,
                angle, new Vector2(0f, 0.5f), new Vector2(length, thickness), SpriteEffects.None, 0f);
        }
        
public override void SetStaticDefaults()
{
    ProjectileID.Sets.DrawScreenCheckFluff[Type] = 10000;
}

    }
}
          public class EmpathyPentagram : ModProjectile
    {
        public override string Texture => "flameout/Content/EmpathyPentagram";
       private float _angle = 0f;
private float _shootTimer = 0f;   
private float _laserTimer = 0f;   
private readonly Queue<Vector2> _playerPosHistory = new Queue<Vector2>();
private const int HistoryLength = 30;

private const float ProjSpeed = 14f;    
private const int ProjInterval = 30;
private const int LaserInterval = 90;  

        public override void SetDefaults()
        {
            Projectile.width = 128;
            Projectile.height = 128;
            Projectile.hostile = true;
            Projectile.friendly = false;
            Projectile.tileCollide = false;
            Projectile.ignoreWater = true;
            Projectile.timeLeft = 600;
            Projectile.damage = 300;
            Projectile.penetrate = -1;
            Projectile.alpha = 0;
        }

      public override void AI()
{
    int ownerIndex = (int)Projectile.ai[0];
    if (ownerIndex < 0 || ownerIndex >= Main.maxNPCs || !Main.npc[ownerIndex].active)
    { Projectile.Kill(); return; }

    _angle += 0.06f;
    float orbitOffset = Projectile.ai[1];
    NPC owner = Main.npc[ownerIndex];
    Projectile.Center = owner.Center + new Vector2(
        (float)Math.Cos(_angle + orbitOffset),
        (float)Math.Sin(_angle + orbitOffset)) * 80f;
    Projectile.velocity = Vector2.Zero;

    
    Player target = Main.player[Player.FindClosest(Projectile.Center, 1, 1)];
    _playerPosHistory.Enqueue(target.Center);
    if (_playerPosHistory.Count > HistoryLength)
        _playerPosHistory.Dequeue();

    if (Main.netMode != NetmodeID.MultiplayerClient)
    {
        
        _shootTimer++;
        if (_shootTimer >= ProjInterval)
        {
            _shootTimer = 0f;
            for (int i = 0; i < 16; i++)
            {
                float sa = (MathHelper.TwoPi / 16f) * i;
                Vector2 vel = new Vector2((float)Math.Cos(sa), (float)Math.Sin(sa)) * ProjSpeed;
                Projectile.NewProjectile(Projectile.GetSource_FromThis(), Projectile.Center, vel,
                    ModContent.ProjectileType<Empathy_Projectiles>(), Projectile.damage / 2, 0f, Main.myPlayer);
            }
        }

        
        _laserTimer++;
        if (_laserTimer >= LaserInterval)
        {
            _laserTimer = 0f;
            Vector2 targetPos = _playerPosHistory.Count > 0 ? _playerPosHistory.Peek() : target.Center;
            float shootAngle = (targetPos - Projectile.Center).ToRotation();
            float laserLength = Main.maxTilesX * 16f; 
            Projectile.NewProjectile(Projectile.GetSource_FromThis(), Projectile.Center, Vector2.Zero,
                ModContent.ProjectileType<EmpathyLaser>(), Projectile.damage, 0f, Main.myPlayer,
                shootAngle, laserLength);
        }
    }
}



        
public override bool PreDraw(ref Color lightColor)
{
    Texture2D texture = ModContent.Request<Texture2D>(Texture).Value;
    Vector2 drawPos = Projectile.Center - Main.screenPosition;
    Color glowColor = new Color(255, 150, 200, 0);

    for (int i = 0; i < 4; i++)
    {
        float scale = Projectile.scale * (1f + i * 0.15f);
        Color layerColor = glowColor * (0.15f - i * 0.03f);
        Main.spriteBatch.Draw(
            texture, drawPos, null,
            layerColor, Projectile.rotation,
            texture.Size() / 2f, scale,
            SpriteEffects.None, 0f);
    }

    Main.spriteBatch.Draw(
        texture, drawPos, null,
        lightColor, Projectile.rotation,
        texture.Size() / 2f, Projectile.scale,
        SpriteEffects.None, 0f);

    return false;
}

public override void SetStaticDefaults()
{
    ProjectileID.Sets.DrawScreenCheckFluff[Type] = 10000;
}

    }
    
public class EmpathyPentagramLaser : ModProjectile
{
   private float _angle = 0f;
private float _shootTimer = 0f;   
private float _projTimer = 0f;    
private readonly Queue<Vector2> _playerPosHistory = new Queue<Vector2>();
private const int HistoryLength = 30;

private const int LaserInterval = 20;   
private const int ProjInterval = 30;
private const float ProjSpeed = 6f;     
    public override string Texture => "flameout/Content/EmpathyPentagram";

    public override void SetDefaults()
    {
        Projectile.width = 64;
        Projectile.height = 64;
        Projectile.hostile = true;
        Projectile.friendly = false;
        Projectile.tileCollide = false;
        Projectile.ignoreWater = true;
        Projectile.timeLeft = 600;
        Projectile.damage = 300;
        Projectile.penetrate = -1;
        Projectile.alpha = 0;
    }

   public override void AI()
{
    int ownerIndex = (int)Projectile.ai[0];
    if (ownerIndex < 0 || ownerIndex >= Main.maxNPCs || !Main.npc[ownerIndex].active)
    { Projectile.Kill(); return; }

    _angle += 0.06f;
    float orbitOffset = Projectile.ai[1];
    NPC owner = Main.npc[ownerIndex];
    Projectile.Center = owner.Center + new Vector2(
        (float)Math.Cos(_angle + orbitOffset),
        (float)Math.Sin(_angle + orbitOffset)) * 120f;
    Projectile.velocity = Vector2.Zero;

    Player target = Main.player[Player.FindClosest(Projectile.Center, 1, 1)];
    _playerPosHistory.Enqueue(target.Center);
    if (_playerPosHistory.Count > HistoryLength)
        _playerPosHistory.Dequeue();

    if (Main.netMode != NetmodeID.MultiplayerClient)
    {
        
        _shootTimer++;
        if (_shootTimer >= LaserInterval)
        {
            _shootTimer = 0f;
            Vector2 targetPos = _playerPosHistory.Count > 0 ? _playerPosHistory.Peek() : target.Center;
            float shootAngle = (targetPos - Projectile.Center).ToRotation();
            float laserLength = Main.maxTilesX * 16f; 
            Projectile.NewProjectile(Projectile.GetSource_FromThis(), Projectile.Center, Vector2.Zero,
                ModContent.ProjectileType<EmpathyLaser>(), Projectile.damage, 0f, Main.myPlayer,
                shootAngle, laserLength);
        }

        
        _projTimer++;
        if (_projTimer >= ProjInterval)
        {
            _projTimer = 0f;
            for (int i = 0; i < 16; i++)
            {
                float sa = (MathHelper.TwoPi / 16f) * i;
                Vector2 vel = new Vector2((float)Math.Cos(sa), (float)Math.Sin(sa)) * ProjSpeed;
                Projectile.NewProjectile(Projectile.GetSource_FromThis(), Projectile.Center, vel,
                    ModContent.ProjectileType<Empathy_Projectiles>(), Projectile.damage / 2, 0f, Main.myPlayer);
            }
        }
    }
}
    
public override void SetStaticDefaults()
{
    ProjectileID.Sets.DrawScreenCheckFluff[Type] = 10000;
}


    public override bool PreDraw(ref Color lightColor)
    {
        Texture2D texture = ModContent.Request<Texture2D>(Texture).Value;
        Vector2 drawPos = Projectile.Center - Main.screenPosition;
        Color glowColor = new Color(255, 150, 200, 0);

        for (int i = 0; i < 4; i++)
        {
            float scale = Projectile.scale * (1f + i * 0.15f);
            Color layerColor = glowColor * (0.15f - i * 0.03f);
            Main.spriteBatch.Draw(
                texture, drawPos, null,
                layerColor, Projectile.rotation,
                texture.Size() / 2f, scale,
                SpriteEffects.None, 0f);
        }

        Main.spriteBatch.Draw(
            texture, drawPos, null,
            lightColor, Projectile.rotation,
            texture.Size() / 2f, Projectile.scale,
            SpriteEffects.None, 0f);

        return false;
    }
}


public class EmpathyLaser : ModProjectile
{
    public override string Texture => "flameout/Content/EmpathyLaser";

    
    
    private float Angle => Projectile.ai[0];
    private float Length => Projectile.ai[1];

    public override void SetDefaults()
    {
        Projectile.width = 16;
        Projectile.height = 16;
        Projectile.hostile = true;
        Projectile.friendly = false;
        Projectile.tileCollide = false;
        Projectile.ignoreWater = true;
        Projectile.timeLeft = 60; 
        Projectile.damage = 300;
        Projectile.penetrate = -1;
        Projectile.alpha = 0;
    }

    public override void OnSpawn(IEntitySource source)
    {
        
        SoundEngine.PlaySound(
            new SoundStyle("flameout/Content/Sounds/em3"),
            Projectile.Center);
        

        
        Projectile.localAI[0] = Projectile.Center.X;
        Projectile.localAI[1] = Projectile.Center.Y;
    }

    private Vector2 SpawnPos => new Vector2(Projectile.localAI[0], Projectile.localAI[1]);

    public override void AI()
    {
        
        Projectile.Center = SpawnPos;
        Projectile.velocity = Vector2.Zero;
    }

    public override bool? Colliding(Rectangle projHitbox, Rectangle targetHitbox)
    {
        Vector2 dir = new Vector2((float)Math.Cos(Angle), (float)Math.Sin(Angle));
        Vector2 end = SpawnPos + dir * Length;
        float _ = 0f;
        return Collision.CheckAABBvLineCollision(
            targetHitbox.TopLeft(), targetHitbox.Size(),
            SpawnPos, end, 8f, ref _);
    }

    public override bool PreDraw(ref Color lightColor)
{
    Texture2D tex = ModContent.Request<Texture2D>(Texture).Value;
    SpriteBatch sb = Main.spriteBatch;

    Vector2 spawnPos = SpawnPos;
    Vector2 dir = new Vector2((float)Math.Cos(Angle), (float)Math.Sin(Angle));

    float fade = (float)Projectile.timeLeft / 60f;
    float pulse = 0.9f + 0.1f * (float)Math.Sin(Main.GlobalTimeWrappedHourly * 30f);

    sb.End();
    sb.Begin(SpriteSortMode.Deferred, BlendState.Additive, SamplerState.LinearClamp,
        DepthStencilState.None, RasterizerState.CullNone, null, Main.GameViewMatrix.TransformationMatrix);

    
    DrawBeam(sb, tex, spawnPos, dir, new Color(255, 80, 160) * (fade * 0.5f), 2.4f * pulse);
    DrawBeam(sb, tex, spawnPos, dir, new Color(255, 140, 200) * (fade * 0.6f), 1.5f * pulse);
    DrawBeam(sb, tex, spawnPos, dir, Color.White * fade, 0.8f * pulse);

    sb.End();
    sb.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.LinearClamp,
        DepthStencilState.None, RasterizerState.CullNone, null, Main.GameViewMatrix.TransformationMatrix);
    return false;
}


private void DrawBeam(SpriteBatch sb, Texture2D tex, Vector2 spawnPos, Vector2 dir, Color color, float thick)
{
    Rectangle srcHead = new Rectangle(0, 0, 16, 32);
    Rectangle srcBody = new Rectangle(16, 0, 48, 32);
    Vector2 drawOrigin = new Vector2(0f, srcHead.Height / 2f);
    Vector2 scale = new Vector2(1f, thick);

    Vector2 currentPos = spawnPos;
    float remainingLength = Length;

    sb.Draw(tex, currentPos - Main.screenPosition, srcHead, color, Angle, drawOrigin, scale, SpriteEffects.None, 0f);
    currentPos += dir * srcHead.Width;
    remainingLength -= srcHead.Width;

    while (remainingLength > 0f)
    {
        float segmentWidth = Math.Min(remainingLength, srcBody.Width);
        Rectangle src = new Rectangle(16, 0, (int)segmentWidth, 32);
        sb.Draw(tex, currentPos - Main.screenPosition, src, color, Angle, drawOrigin, scale, SpriteEffects.None, 0f);
        currentPos += dir * segmentWidth;
        remainingLength -= segmentWidth;
    }
}
    
public override void SetStaticDefaults()
{
    ProjectileID.Sets.DrawScreenCheckFluff[Type] = 10000;
}

}


    
    
    
 public class EmpathySword : ModProjectile
{
    public override string Texture => "flameout/Content/empathysword";

    private int Phase
    {
        get => (int)Projectile.localAI[0];
        set => Projectile.localAI[0] = value;
    }

    private float PhaseTimer
    {
        get => Projectile.localAI[1];
        set => Projectile.localAI[1] = value;
    }

    private float _swingAngle = 0f;
    private float _swingFrom = 0f;      
    private float _swingProgress = 0f;  
    private bool _initDone = false;
    private bool _slashSoundPlayed = false;

    private Vector2 _throwTarget = Vector2.Zero;
    private Vector2 _throwStart = Vector2.Zero;
    private float _throwProgress = 0f;

    
    private const int SlashTime = 45;            
    private const float SwingRange = MathHelper.Pi;
    private const float SwordLen = 480f;         
    private const float SwordScale = 4.5f;       
    private const float HitWidth = 30f;          

    
    private const int TrailLen = 20;
    private const int TrailSubsteps = 3;         
    private Vector2[] _trailPivot = new Vector2[TrailLen];
    private float[] _trailAngle = new float[TrailLen];
    private int _trailIndex = 0;

    public override void SetStaticDefaults()
    {
        ProjectileID.Sets.DrawScreenCheckFluff[Type] = 10000;
    }

    public override void SetDefaults()
    {
        Projectile.width = 300;
        Projectile.height = 300;
        Projectile.hostile = true;
        Projectile.friendly = false;
        Projectile.tileCollide = false;
        Projectile.ignoreWater = true;
        Projectile.timeLeft = 600;
        Projectile.damage = 300;
        Projectile.penetrate = -1;
        Projectile.alpha = 255;
    }

    
    
    private static float EasedSwing(float t)
    {
        const float windupPart = 0.3f;
        if (t < windupPart)
        {
            float w = t / windupPart;
            return -0.12f * (float)Math.Sin(w * MathHelper.PiOver2);
        }
        float s = (t - windupPart) / (1f - windupPart);
        float e = s < 0.5f
            ? 4f * s * s * s
            : 1f - (float)Math.Pow(-2f * s + 2f, 3f) / 2f;
        return MathHelper.Lerp(-0.12f, 1f, e);
    }

    public override void AI()
    {
        int ownerIndex = (int)Projectile.ai[0];
        if (ownerIndex < 0 || ownerIndex >= Main.maxNPCs || !Main.npc[ownerIndex].active)
        { Projectile.Kill(); return; }

        NPC owner = Main.npc[ownerIndex];
        Player target = Main.player[Player.FindClosest(owner.Center, 1, 1)];

        if (Phase != 4)
            Projectile.Center = owner.Center;

        if (!_initDone)
        {
            _initDone = true;
            float toPlayer = (target.Center - owner.Center).ToRotation();
            _swingFrom = toPlayer - SwingRange * 0.5f;
            _swingAngle = _swingFrom;
            Phase = 0;
            PhaseTimer = 0f;
        }

        PhaseTimer++;

        switch (Phase)
        {
            
            case 0:
            {
                float t = PhaseTimer / SlashTime;
                _swingAngle = _swingFrom + SwingRange * EasedSwing(t);
                PlaySlashSound(t);
                RecordTrail();
                SpawnBladeDust(t);

                if (t >= 1f)
                {
                    PhaseTimer = 0f;
                    _slashSoundPlayed = false;
                    Phase = 1;
                }
                break;
            }

            
            case 1:
            {
                if (PhaseTimer >= 20f)
                {
                    float toPlayer2 = (target.Center - owner.Center).ToRotation();
                    _swingFrom = toPlayer2 + SwingRange * 0.5f;
                    _swingAngle = _swingFrom;
                    PhaseTimer = 0f;
                    Phase = 2;
                }
                break;
            }

            
            case 2:
            {
                float t = PhaseTimer / SlashTime;
                _swingAngle = _swingFrom - SwingRange * EasedSwing(t);
                PlaySlashSound(t);
                RecordTrail();
                SpawnBladeDust(t);

                if (t >= 1f)
                {
                    PhaseTimer = 0f;
                    _swingProgress = 0f;
                    _slashSoundPlayed = false;
                    Phase = 3;
                }
                break;
            }

            
            case 3:
            {
                float spinSpeed = MathHelper.Lerp(0.06f, 0.28f,
                    MathHelper.Clamp(_swingProgress / MathHelper.TwoPi, 0f, 1f));
                _swingAngle += spinSpeed;
                _swingProgress += spinSpeed;
                RecordTrail();
                SpawnBladeDust(0.5f);

                if (_swingProgress >= MathHelper.TwoPi)
                {
                    _throwTarget = target.Center;
                    _throwStart = owner.Center;
                    _throwProgress = 0f;
                    PhaseTimer = 0f;
                    SoundEngine.PlaySound(SoundID.Item71 with { Pitch = -0.3f }, owner.Center);
                    Phase = 4;
                }
                break;
            }

            
            case 4:
            {
                Vector2 toTarget = _throwTarget - _throwStart;
                float totalDist = toTarget.Length();
                float t = MathHelper.Clamp(_throwProgress / totalDist, 0f, 1f);
                float step = MathHelper.Lerp(10f, 34f, t); 
                _throwProgress += step;
                t = MathHelper.Clamp(_throwProgress / totalDist, 0f, 1f);
                Projectile.Center = Vector2.Lerp(_throwStart, _throwTarget, t);

                _swingAngle += 0.3f;
                RecordTrail();
                SpawnBladeDust(0.5f);

                if (t >= 1f || Vector2.Distance(Projectile.Center, _throwTarget) < step)
                {
                    Projectile.Center = _throwTarget;
                    SpawnImpactProjectiles();
                    Projectile.Kill();
                }
                break;
            }
        }

        
        Vector2 tip = (Phase == 4 ? Projectile.Center : owner.Center)
            + _swingAngle.ToRotationVector2() * SwordLen;
        Lighting.AddLight(tip, 1.2f, 0.35f, 0.7f);
    }

    private void PlaySlashSound(float t)
    {
        
        if (!_slashSoundPlayed && t >= 0.3f)
        {
            _slashSoundPlayed = true;
            SoundEngine.PlaySound(SoundID.Item71 with { Volume = 1.1f }, Projectile.Center);
        }
    }

    private void SpawnBladeDust(float t)
    {
        
        if (t < 0.3f || !Main.rand.NextBool(2)) return;
        Vector2 pivot = Phase == 4 ? Projectile.Center : Main.npc[(int)Projectile.ai[0]].Center;
        float along = Main.rand.NextFloat(0.4f, 1f);
        Vector2 pos = pivot + _swingAngle.ToRotationVector2() * SwordLen * along;
        Dust d = Dust.NewDustPerfect(pos, DustID.PinkFairy,
            (_swingAngle + MathHelper.PiOver2).ToRotationVector2() * Main.rand.NextFloat(1f, 3f),
            0, default, 1.6f);
        d.noGravity = true;
    }

    private void RecordTrail()
    {
        int ownerIndex = (int)Projectile.ai[0];
        Vector2 pivot = (Phase == 4)
            ? Projectile.Center
            : (ownerIndex >= 0 && ownerIndex < Main.maxNPCs
                ? Main.npc[ownerIndex].Center
                : Projectile.Center);

        _trailPivot[_trailIndex] = pivot;
        _trailAngle[_trailIndex] = _swingAngle;
        _trailIndex = (_trailIndex + 1) % TrailLen;
    }

    private void SpawnImpactProjectiles()
    {
        if (Main.netMode == NetmodeID.MultiplayerClient) return;

        int type = ModContent.ProjectileType<Empathy_Projectiles>();
        float speed = 8f;
        int countPerDir = 8;

        float[] baseAngles = new float[]
        {
            0f, MathHelper.Pi, -MathHelper.PiOver2, MathHelper.PiOver2,
            MathHelper.PiOver4, -MathHelper.PiOver4,
            MathHelper.Pi + MathHelper.PiOver4, MathHelper.Pi - MathHelper.PiOver4,
        };

        float spreadPerDir = MathHelper.PiOver4 * 0.5f;

        foreach (float baseAngle in baseAngles)
        {
            for (int i = 0; i < countPerDir; i++)
            {
                float offset = MathHelper.Lerp(-spreadPerDir, spreadPerDir, i / (float)(countPerDir - 1));
                float angle = baseAngle + offset;
                float proj_speed = speed * (0.8f + Main.rand.NextFloat() * 0.4f);
                Vector2 vel = new Vector2((float)Math.Cos(angle), (float)Math.Sin(angle)) * proj_speed;
                Projectile.NewProjectile(Projectile.GetSource_FromThis(),
                    Projectile.Center, vel, type, Projectile.damage, 0f, Main.myPlayer);
            }
        }

        for (int i = 0; i < 30; i++)
        {
            Vector2 dustVel = Main.rand.NextVector2CircularEdge(5f, 5f);
            Dust.NewDust(Projectile.Center, 0, 0, DustID.PinkFairy, dustVel.X, dustVel.Y, 0, default, 2f);
        }

        SoundEngine.PlaySound(SoundID.Item14, Projectile.Center);
    }

    public override bool? Colliding(Rectangle projHitbox, Rectangle targetHitbox)
    {
        int ownerIndex = (int)Projectile.ai[0];
        if (ownerIndex < 0 || ownerIndex >= Main.maxNPCs) return false;

        Vector2 pivot = Phase == 4 ? Projectile.Center : Main.npc[ownerIndex].Center;
        Vector2 tip = pivot + _swingAngle.ToRotationVector2() * SwordLen;
        float _ = 0f;
        return Collision.CheckAABBvLineCollision(
            targetHitbox.TopLeft(), targetHitbox.Size(), pivot, tip, HitWidth, ref _);
    }

    private static SpriteEffects FlipFor(float angle)
        => Math.Abs(MathHelper.WrapAngle(angle)) > MathHelper.PiOver2
            ? SpriteEffects.FlipVertically
            : SpriteEffects.None;

    public override bool PreDraw(ref Color lightColor)
    {
        int ownerIndex = (int)Projectile.ai[0];
        if (ownerIndex < 0 || ownerIndex >= Main.maxNPCs) return false;

        SpriteBatch sb = Main.spriteBatch;
        Texture2D swordTex = ModContent.Request<Texture2D>("flameout/Content/empathysword").Value;
        Vector2 texOrigin = new Vector2(0f, swordTex.Height * 0.5f);

        sb.End();
        sb.Begin(SpriteSortMode.Deferred, BlendState.Additive, SamplerState.LinearClamp,
            DepthStencilState.None, RasterizerState.CullNone, null,
            Main.GameViewMatrix.TransformationMatrix);

        
        for (int i = TrailLen - 2; i >= 1; i--)
        {
            int idx = (_trailIndex - 1 - i + TrailLen * 2) % TrailLen;
            int idxNext = (idx + 1) % TrailLen;

            Vector2 pivotA = _trailPivot[idx];
            Vector2 pivotB = _trailPivot[idxNext];
            if (pivotA == Vector2.Zero || pivotB == Vector2.Zero) continue;

            for (int s = 0; s < TrailSubsteps; s++)
            {
                float sub = s / (float)TrailSubsteps;
                float progress = (i - sub) / (float)TrailLen; 
                Vector2 pivot = Vector2.Lerp(pivotA, pivotB, sub);
                float angle = MathHelper.Lerp(_trailAngle[idx], _trailAngle[idxNext], sub);

                
                Color col = Color.Lerp(new Color(255, 150, 200), new Color(200, 40, 80), progress)
                    * ((1f - progress) * 0.5f);
                float scale = SwordScale * (1f - 0.06f * progress);

                sb.Draw(swordTex, pivot - Main.screenPosition, null, col,
                    angle, texOrigin, scale, FlipFor(angle), 0f);
            }
        }

        
        Vector2 glowPos = (Phase == 4 ? Projectile.Center : Main.npc[ownerIndex].Center) - Main.screenPosition;
        sb.Draw(swordTex, glowPos, null, new Color(255, 110, 180) * 0.4f,
            _swingAngle, texOrigin, SwordScale * 1.05f, FlipFor(_swingAngle), 0f);

        sb.End();
        sb.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.LinearClamp,
            DepthStencilState.None, RasterizerState.CullNone, null,
            Main.GameViewMatrix.TransformationMatrix);

        
        Vector2 drawPos = Phase == 4
            ? Projectile.Center - Main.screenPosition
            : Main.npc[ownerIndex].Center - Main.screenPosition;

        sb.Draw(swordTex, drawPos, null, Color.White, _swingAngle,
            texOrigin, SwordScale, FlipFor(_swingAngle), 0f);

        return false;
    }
}
     public class EmpathyScythe : ModProjectile
{
    public override string Texture => "flameout/Content/EmpathyScythe";

    public override void SetStaticDefaults()
    {
        ProjectileID.Sets.DrawScreenCheckFluff[Type] = 10000;
    }

    private float _angle = 0f;
    private float _shootTimer = 0f;
    private const float RotSpeed = 0.18f;
    private const float OrbitRadius = 70f;
    private const int ShootInterval = 8;
    private bool _hasTeleported = false;
    private const int ShotCount = 12;

    
    private const int TrailLen = 16;
    private const int TrailSubsteps = 4; 
    private Vector2[] _trailPivot = new Vector2[TrailLen];
    private float[] _trailAngle = new float[TrailLen];
    private int _trailIndex = 0;

    public override void SetDefaults()
    {
        Projectile.width = 280;
        Projectile.height = 256;
        Projectile.hostile = true;
        Projectile.friendly = false;
        Projectile.tileCollide = false;
        Projectile.ignoreWater = true;
        Projectile.timeLeft = 480;
        Projectile.damage = 300;
        Projectile.penetrate = -1;
        Projectile.alpha = 0;
    }

    public override void AI()
    {
        int ownerIndex = (int)Projectile.ai[0];
        if (ownerIndex < 0 || ownerIndex >= Main.maxNPCs || !Main.npc[ownerIndex].active)
        { Projectile.Kill(); return; }

        NPC owner = Main.npc[ownerIndex];

        _angle += RotSpeed;
        Projectile.Center = owner.Center + new Vector2(
            (float)Math.Cos(_angle),
            (float)Math.Sin(_angle)) * OrbitRadius;
        Projectile.rotation = _angle + MathHelper.PiOver2;
        Projectile.velocity = Vector2.Zero;

        
        _trailPivot[_trailIndex] = owner.Center;
        _trailAngle[_trailIndex] = _angle;
        _trailIndex = (_trailIndex + 1) % TrailLen;

        if (Main.netMode != NetmodeID.MultiplayerClient)
        {
            
            if (!_hasTeleported && Projectile.timeLeft <= 240)
            {
                _hasTeleported = true;

                SoundEngine.PlaySound(
                    new SoundStyle("flameout/Content/Sounds/em3"),
                    Projectile.Center);

                owner.TargetClosest(false);
                Player target = Main.player[owner.target];
                if (target.active && !target.dead)
                {
                    const float MaxRadius = 50f * 16f;
                    const float MinRadius = 8f * 16f;

                    float angle = Main.rand.NextFloat(MathHelper.TwoPi);
                    float dist = Main.rand.NextFloat(MinRadius, MaxRadius);
                    Vector2 newPos = target.Center + angle.ToRotationVector2() * dist;

                    for (int i = 0; i < 30; i++)
                        Dust.NewDust(owner.position, owner.width, owner.height, DustID.Shadowflame);

                    owner.Center = newPos;
                    owner.velocity = Vector2.Zero;
                    owner.netUpdate = true;

                    for (int i = 0; i < 30; i++)
                        Dust.NewDust(owner.position, owner.width, owner.height, DustID.Shadowflame);

                    SoundEngine.PlaySound(SoundID.Item8, owner.Center);
                }
            }

            _shootTimer++;
            if (_shootTimer >= ShootInterval)
            {
                _shootTimer = 0f;
                for (int i = 0; i < ShotCount; i++)
                {
                    float sa = (MathHelper.TwoPi / ShotCount) * i;
                    Vector2 vel = new Vector2(
                        (float)Math.Cos(sa),
                        (float)Math.Sin(sa)) * 9f;

                    Projectile.NewProjectile(
                        Projectile.GetSource_FromThis(),
                        Projectile.Center,
                        vel,
                        ModContent.ProjectileType<EmpathyScytheShot>(),
                        Projectile.damage / 2,
                        0f,
                        Main.myPlayer);
                }
            }
        }
    }

    public override bool PreDraw(ref Color lightColor)
    {
        Texture2D tex = ModContent.Request<Texture2D>("flameout/Content/EmpathyScythe").Value;
        SpriteBatch sb = Main.spriteBatch;
        Vector2 origin = new Vector2(tex.Width * 0.5f, tex.Height * 0.5f);

        
        sb.End();
        sb.Begin(SpriteSortMode.Deferred, BlendState.Additive, SamplerState.LinearClamp,
            DepthStencilState.None, RasterizerState.CullNone, null,
            Main.GameViewMatrix.TransformationMatrix);

        for (int i = TrailLen - 2; i >= 1; i--)
        {
            int idx = (_trailIndex - 1 - i + TrailLen * 2) % TrailLen;
            int idxNext = (idx + 1) % TrailLen;

            Vector2 pivotA = _trailPivot[idx];
            Vector2 pivotB = _trailPivot[idxNext];
            if (pivotA == Vector2.Zero || pivotB == Vector2.Zero) continue;

            for (int s = 0; s < TrailSubsteps; s++)
            {
                float sub = s / (float)TrailSubsteps;
                float progress = (i - sub) / (float)TrailLen; 
                Vector2 pivot = Vector2.Lerp(pivotA, pivotB, sub);
                float angle = MathHelper.Lerp(_trailAngle[idx], _trailAngle[idxNext], sub);

                Vector2 pos = pivot + angle.ToRotationVector2() * OrbitRadius;

                
                Color col = Color.Lerp(new Color(255, 150, 200), new Color(200, 40, 80), progress)
                    * ((1f - progress) * 0.45f);
                float scale = Projectile.scale * (1f - 0.08f * progress);

                sb.Draw(tex, pos - Main.screenPosition, null, col,
                    angle + MathHelper.PiOver2, origin, scale, SpriteEffects.None, 0f);
            }
        }

        
        sb.Draw(tex, Projectile.Center - Main.screenPosition, null,
            new Color(255, 110, 180) * 0.35f, Projectile.rotation,
            origin, Projectile.scale * 1.05f, SpriteEffects.None, 0f);

        sb.End();
        sb.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.LinearClamp,
            DepthStencilState.None, RasterizerState.CullNone, null,
            Main.GameViewMatrix.TransformationMatrix);

        
        Vector2 drawPos = Projectile.Center - Main.screenPosition;
        sb.Draw(tex, drawPos, null, Color.White, Projectile.rotation,
            origin, Projectile.scale, SpriteEffects.None, 0f);

        return false;
    }
}
    
    
    
    public class EmpathyScytheShot : ModProjectile
    {
        public override string Texture => "flameout/Content/EmpathyScytheShot";
        public override void SetDefaults()
        {
            Projectile.width = 14;
            Projectile.height = 14;
            Projectile.hostile = true;
            Projectile.friendly = false;
            Projectile.tileCollide = true;
            Projectile.ignoreWater = true;
            Projectile.timeLeft = 200;
            Projectile.damage = 70;
            Projectile.penetrate = 1;
            Projectile.alpha = 0;
        }

        public override void AI()
        {
            Projectile.rotation += 0.25f;
        }

        public override bool PreDraw(ref Color lightColor)
        {
            Texture2D tex = ModContent.Request<Texture2D>("flameout/Content/EmpathyScytheShot").Value;
            SpriteBatch sb = Main.spriteBatch;
            Vector2 pos = Projectile.Center - Main.screenPosition;

            sb.Draw(tex, pos, null, Color.White, Projectile.rotation,
                new Vector2(tex.Width * 0.5f, tex.Height * 0.5f),
                Projectile.scale, SpriteEffects.None, 0f);

            return false;
        }

        public override void OnKill(int timeLeft)
        {
            
            for (int i = 0; i < 6; i++)
            {
                Dust d = Dust.NewDustDirect(Projectile.Center, 0, 0,
                    DustID.PinkFairy, 0f, 0f, 0, default, 1.2f);
                d.velocity = Main.rand.NextVector2Circular(4f, 4f);
                d.noGravity = true;
            }
        }
    }
    public class EmpathyPortal1 : ModProjectile
{
    public override string Texture => "flameout/Content/EmpathyPortal1";

    private float _timer = 0f;
    private const float Lifetime = 15f * 60f; 
    private const float SpawnInterval = 8f;    
    private float _spawnTimer = 0f;

    public override void SetStaticDefaults()
    {
        ProjectileID.Sets.DrawScreenCheckFluff[Type] = 10000;
    }

    public override void SetDefaults()
    {
        Projectile.width = 64;
        Projectile.height = 64;
        Projectile.hostile = false;
        Projectile.friendly = false;
        Projectile.tileCollide = false;
        Projectile.ignoreWater = true;
        Projectile.timeLeft = (int)Lifetime + 10;
        Projectile.penetrate = -1;
        Projectile.alpha = 0;
    }

    public override void AI()
    {
        int ownerIndex = (int)Projectile.ai[0];
        if (ownerIndex < 0 || ownerIndex >= Main.maxNPCs || !Main.npc[ownerIndex].active)
        { Projectile.Kill(); return; }

        NPC owner = Main.npc[ownerIndex];

        
        Projectile.Center = owner.Center + new Vector2(-200f, 0f);

        _timer++;
        _spawnTimer++;

        
        if (_timer <= Lifetime || _spawnTimer >= SpawnInterval)
        {
            _spawnTimer = 0f;
            if (Main.netMode != NetmodeID.MultiplayerClient)
            {
                
                float angle = Main.rand.NextFloat() * MathHelper.TwoPi;
                Projectile.NewProjectile(
                    Projectile.GetSource_FromThis(),
                    Projectile.Center,
                    Vector2.Zero,
                    ModContent.ProjectileType<EmpathyParticle2>(),
                    Projectile.damage,
                    0f,
                    Main.myPlayer,
                    Projectile.whoAmI, 
                    angle);            
            }
        }

        
        if (_timer >= Lifetime)
            Projectile.Kill();
    }

    public override void OnKill(int timeLeft)
    {
        
        for (int i = 0; i < 20; i++)
        {
            Vector2 vel = Main.rand.NextVector2CircularEdge(3f, 3f);
            Dust.NewDust(Projectile.Center, 0, 0, DustID.PinkFairy, vel.X, vel.Y, 0, default, 1.5f);
        }
        SoundEngine.PlaySound(SoundID.Item14, Projectile.Center);
    }
}

public class EmpathyPortal2 : ModProjectile
{
    public override string Texture => "flameout/Content/EmpathyPortal2";

    private float _timer = 0f;
    private const float Lifetime = 15f * 60f;
    private const float SpawnInterval = 8f;
    private float _spawnTimer = 0f;

    public override void SetStaticDefaults()
    {
        ProjectileID.Sets.DrawScreenCheckFluff[Type] = 10000;
    }

    public override void SetDefaults()
    {
        Projectile.width = 64;
        Projectile.height = 64;
        Projectile.hostile = false;
        Projectile.friendly = false;
        Projectile.tileCollide = false;
        Projectile.ignoreWater = true;
        Projectile.timeLeft = (int)Lifetime + 10;
        Projectile.penetrate = -1;
        Projectile.alpha = 0;
    }

    public override void AI()
    {
        int ownerIndex = (int)Projectile.ai[0];
        if (ownerIndex < 0 || ownerIndex >= Main.maxNPCs || !Main.npc[ownerIndex].active)
        { Projectile.Kill(); return; }

        NPC owner = Main.npc[ownerIndex];

        
        Projectile.Center = owner.Center + new Vector2(200f, 0f);

        _timer++;
        _spawnTimer++;

        if (_timer <= Lifetime || _spawnTimer >= SpawnInterval)
{
            _spawnTimer = 0f;
            if (Main.netMode != NetmodeID.MultiplayerClient)
            {
                
                float angle = Main.rand.NextFloat() * MathHelper.TwoPi;
                float speed = 16f * 16f / 60f; 
                Vector2 vel = new Vector2(
                    (float)Math.Cos(angle),
                    (float)Math.Sin(angle)) * speed;

                Projectile.NewProjectile(
                    Projectile.GetSource_FromThis(),
                    Projectile.Center,
                    vel,
                    ModContent.ProjectileType<EmpathyParticle3>(),
                    Projectile.damage,
                    0f,
                    Main.myPlayer);
            }
        }

        if (_timer >= Lifetime)
            Projectile.Kill();
    }

    public override void OnKill(int timeLeft)
    {
        for (int i = 0; i < 20; i++)
        {
            Vector2 vel = Main.rand.NextVector2CircularEdge(3f, 3f);
            Dust.NewDust(Projectile.Center, 0, 0, DustID.PinkFairy, vel.X, vel.Y, 0, default, 1.5f);
        }
        SoundEngine.PlaySound(SoundID.Item14, Projectile.Center);
    }
}


public class EmpathyParticle2 : ModProjectile
{
    public override string Texture => "flameout/Content/EmpathyParticle2";

    private const float Lifetime = 30f * 60f;
    private float _timer = 0f;
    private float _orbitAngle = 0f;
    private float _orbitRadius = 0f;
    private bool _init = false;

    public override void SetStaticDefaults()
    {
        ProjectileID.Sets.DrawScreenCheckFluff[Type] = 10000;
    }

    public override void SetDefaults()
    {
        Projectile.width = 16;
        Projectile.height = 16;
        Projectile.hostile = true;
        Projectile.friendly = false;
        Projectile.tileCollide = false;
        Projectile.ignoreWater = true;
        Projectile.timeLeft = (int)Lifetime + 10;
        Projectile.penetrate = -1;
    }

    public override void AI()
    {
        int portalIndex = (int)Projectile.ai[0];
        if (portalIndex < 0 || portalIndex >= Main.maxProjectiles || !Main.projectile[portalIndex].active)
        { Projectile.Kill(); return; }

        if (!_init)
        {
            _init = true;
            _orbitAngle = Projectile.ai[1]; 
            _orbitRadius = 0f;
        }

        Vector2 portalCenter = Main.projectile[portalIndex].Center;

        _timer++;

        
        _orbitRadius += 4.0f;

        
        _orbitAngle += 0.04f;
        if (_orbitAngle > MathHelper.TwoPi) _orbitAngle -= MathHelper.TwoPi;

        Vector2 offset = new Vector2(
            (float)Math.Cos(_orbitAngle),
            (float)Math.Sin(_orbitAngle)) * _orbitRadius;

        Projectile.Center = portalCenter + offset;
        Projectile.rotation = _orbitAngle + MathHelper.PiOver2;

        
        if (_orbitRadius > 2000f || _timer >= Lifetime)
            Projectile.Kill();
    }

    public override void OnKill(int timeLeft)
    {
        for (int i = 0; i < 10; i++)
        {
            Vector2 vel = Main.rand.NextVector2CircularEdge(2f, 2f);
            Dust.NewDust(Projectile.Center, 0, 0, DustID.PinkFairy, vel.X, vel.Y, 0, default, 1.2f);
        }
    }
}


public class EmpathyParticle3 : ModProjectile
{
    public override string Texture => "flameout/Content/EmpathyParticle3";

    private const float Lifetime = 30f * 60f;
    private float _timer = 0f;

    public override void SetStaticDefaults()
    {
        ProjectileID.Sets.DrawScreenCheckFluff[Type] = 10000;
    }

    public override void SetDefaults()
    {
        Projectile.width = 16;
        Projectile.height = 16;
        Projectile.hostile = true;
        Projectile.friendly = false;
        Projectile.tileCollide = false;
        Projectile.ignoreWater = true;
        Projectile.timeLeft = (int)Lifetime + 10;
        Projectile.penetrate = -1;
        Projectile.alpha = 0;
    }

    public override void AI()
    {
        _timer++;
        Projectile.rotation = Projectile.velocity.ToRotation();

        if (_timer >= Lifetime)
            Projectile.Kill();
    }

    public override void OnKill(int timeLeft)
    {
        for (int i = 0; i < 10; i++)
        {
            Vector2 vel = Main.rand.NextVector2CircularEdge(2f, 2f);
            Dust.NewDust(Projectile.Center, 0, 0, DustID.PinkFairy, vel.X, vel.Y, 0, default, 1.2f);
        }
    }
}
public class Guardian1 : ModNPC
{
    public override string Texture => "flameout/Content/Guardian1";
    private int _attackTimer = 0;
    private int _arrowCount = 0;
    private float _bowAngle;
private int _bowRecoilTimer;
    private const int ArrowDelay = 30; 
    private const int ArrowsPerBurst = 5;

    public override void SetStaticDefaults()
    {
        Main.npcFrameCount[NPC.type] = 1;
        NPCID.Sets.MPAllowedEnemies[NPC.type] = true;
    }

    public override void SetDefaults()
    {
        NPC.width = 40;
        NPC.height = 56;
        NPC.lifeMax = 250_000;
        NPC.damage = 150;
        NPC.defense = 20;
        NPC.knockBackResist = 0f;
        NPC.noGravity = true;
        NPC.noTileCollide = true;
        NPC.HitSound = SoundID.NPCHit1;
        NPC.DeathSound = SoundID.NPCDeath1;
    }

 public override void AI()
{
    NPC.TargetClosest(true);

    Player target = Main.player[NPC.target];

    if (!target.active || target.dead)
    {
        NPC.velocity = Vector2.Zero;
        return;
    }

    
    Vector2 desiredPosition = target.Center + new Vector2(-50f * 16f, 0f);
    NPC.Center = Vector2.Lerp(NPC.Center, desiredPosition, 0.15f);
    NPC.velocity = Vector2.Zero;
    NPC.spriteDirection = 1;

    _attackTimer++;

    if (_bowRecoilTimer > 0)
        _bowRecoilTimer--;

    if (_attackTimer < ArrowDelay)
        return;

    _attackTimer = 0;

    float progress = _arrowCount / (float)(ArrowsPerBurst - 1);

    _bowAngle = MathHelper.Lerp(
        -MathHelper.PiOver2,
        MathHelper.PiOver2,
        progress);

    Vector2 shotDirection = _bowAngle.ToRotationVector2();
    Vector2 shotPosition = NPC.Center + shotDirection * 30f;

    _bowRecoilTimer = 10;
    _arrowCount++;

    if (Main.netMode != NetmodeID.MultiplayerClient)
    {
        Projectile.NewProjectile(
            NPC.GetSource_FromAI(),
            shotPosition,
            shotDirection * 8f,
            ModContent.ProjectileType<GuardianArrow>(),
            NPC.damage / 2,
            0f,
            Main.myPlayer);
    }

    if (_arrowCount >= ArrowsPerBurst)
        _arrowCount = 0;
}
public override void PostDraw(
    SpriteBatch spriteBatch,
    Vector2 screenPos,
    Color drawColor)
{
    Texture2D bowTexture = ModContent.Request<Texture2D>(
        "flameout/Content/GuardianBow"
    ).Value;
    Vector2 direction = _bowAngle.ToRotationVector2();

    float recoil = _bowRecoilTimer > 0
        ? _bowRecoilTimer / 10f * 8f
        : 0f;

    Vector2 bowPosition =
        NPC.Center
        - screenPos
        + direction * (26f - recoil);

    Vector2 origin = bowTexture.Size() * 0.5f;

    SpriteEffects effects = SpriteEffects.None;
    float rotation = _bowAngle;

    if (direction.X < 0f)
    {
        effects = SpriteEffects.FlipVertically;
    }

    spriteBatch.Draw(
        bowTexture,
        bowPosition,
        null,
        drawColor,
        rotation,
        origin,
        1f,
        effects,
        0f);
}
    
}
public class Guardian2 : ModNPC
{
    public override string Texture => "flameout/Content/Guardian2";
    private int _burstTimer = 0;
    private int _bulletCount = 0;
    private bool _inBurst = false;
    private int _burstShootTimer = 0;
    private const int BurstDelay = 60;   
    private const int BulletDelay = 6;   
    private const int BulletsPerBurst = 5;

    public override void SetStaticDefaults()
    {
        Main.npcFrameCount[NPC.type] = 1;
        NPCID.Sets.MPAllowedEnemies[NPC.type] = true;
    }

    public override void SetDefaults()
    {
        NPC.width = 40;
        NPC.height = 56;
        NPC.lifeMax = 250_000;
        NPC.damage = 150;
        NPC.defense = 20;
        NPC.knockBackResist = 0f;
        NPC.noGravity = true;
        NPC.noTileCollide = true;
        NPC.HitSound = SoundID.NPCHit1;
        NPC.DeathSound = SoundID.NPCDeath1;
    }

    public override void AI()
    {
        NPC.TargetClosest(true);
        Player target = Main.player[NPC.target];

        
        Vector2 desiredPos = target.Center + new Vector2(50f * 16f, 0f);
        NPC.Center = Vector2.Lerp(NPC.Center, desiredPos, 0.15f);
        NPC.velocity = Vector2.Zero;

        
        NPC.spriteDirection = -1;

        if (!_inBurst)
        {
            _burstTimer++;
            if (_burstTimer >= BurstDelay)
            {
                _burstTimer = 0;
                _inBurst = true;
                _bulletCount = 0;
                _burstShootTimer = 0;
            }
        }
        else
        {
            _burstShootTimer++;
            if (_burstShootTimer >= BulletDelay)
            {
                _burstShootTimer = 0;
                _bulletCount++;

                if (Main.netMode != NetmodeID.MultiplayerClient)
                {
                    
                    float spread = Main.rand.NextFloat(-0.15f, 0.15f);
                    Vector2 vel = new Vector2(-8f, spread);

                    Projectile.NewProjectile(
                        NPC.GetSource_FromAI(),
                        NPC.Center,
                        vel,
                        ModContent.ProjectileType<GuardianBullet>(),
                        NPC.damage / 2,
                        0f,
                        Main.myPlayer);
                }

                if (_bulletCount >= BulletsPerBurst)
                {
                    _inBurst = false;
                    _bulletCount = 0;
                }
            }
        }
    }
    public override void PostDraw(
    SpriteBatch spriteBatch,
    Vector2 screenPos,
    Color drawColor)
{
    Texture2D gunTexture = ModContent.Request<Texture2D>(
        "flameout/Content/GuardianGun"
    ).Value;

    Vector2 gunPosition =
        NPC.Center
        - screenPos
        + new Vector2(-30f, 4f);

    spriteBatch.Draw(
        gunTexture,
        gunPosition,
        null,
        Color.White,
        0f,
        gunTexture.Size() * 0.5f,
        1f,
        SpriteEffects.FlipHorizontally,
        0f
    );
}
}

public class GuardianArrow : ModProjectile
{
    public override string Texture => "flameout/Content/GuardianArrow";

    public override void SetStaticDefaults()
    {
        ProjectileID.Sets.DrawScreenCheckFluff[Type] = 10000;
    }

    public override void SetDefaults()
    {
        Projectile.width = 10;
        Projectile.height = 30;
        Projectile.hostile = true;
        Projectile.friendly = false;
        Projectile.tileCollide = true;
        Projectile.ignoreWater = true;
        Projectile.timeLeft = 300; 
        Projectile.penetrate = 1;
        Projectile.arrow = true;
    }

    public override void AI()
    {
        
        Projectile.rotation = Projectile.velocity.ToRotation() + MathHelper.PiOver2;

        
        Projectile.velocity.Y += 0.2f;
    }

    public override void OnKill(int timeLeft)
    {
        for (int i = 0; i < 8; i++)
        {
            Vector2 vel = Main.rand.NextVector2CircularEdge(2f, 2f);
            Dust.NewDust(Projectile.Center, 0, 0, DustID.WoodFurniture, vel.X, vel.Y);
        }
    }
}

public class GuardianBullet : ModProjectile
{
    public override string Texture => "flameout/Content/GuardianBullet";

    public override void SetStaticDefaults()
    {
        ProjectileID.Sets.DrawScreenCheckFluff[Type] = 10000;
    }

    public override void SetDefaults()
    {
        Projectile.width = 8;
        Projectile.height = 8;
        Projectile.hostile = true;
        Projectile.friendly = false;
        Projectile.tileCollide = true;
        Projectile.ignoreWater = true;
        Projectile.timeLeft = 240; 
        Projectile.penetrate = 1;
    }

    public override void AI()
    {
        
        Projectile.rotation = Projectile.velocity.ToRotation();

        
        if (Main.rand.NextBool(3))
        {
            Dust.NewDust(Projectile.Center, 0, 0, DustID.Smoke,
                Projectile.velocity.X * 0.2f,
                Projectile.velocity.Y * 0.2f,
                0, default, 0.8f);
        }
    }

    public override void OnKill(int timeLeft)
    {
        for (int i = 0; i < 6; i++)
        {
            Vector2 vel = Main.rand.NextVector2CircularEdge(2f, 2f);
            Dust.NewDust(Projectile.Center, 0, 0, DustID.Smoke, vel.X, vel.Y);
        }
        SoundEngine.PlaySound(SoundID.Item10, Projectile.Center);
    }
}

public class EmpathyPortal : ModProjectile
{
    public override string Texture => "flameout/Content/EmpathyPortal";

    
    
    
    
    

  

    public override void SetStaticDefaults()
    {
        ProjectileID.Sets.DrawScreenCheckFluff[Type] = 10000;
    }

    public override void SetDefaults()
    {
        Projectile.width = 200;
        Projectile.height = 200;
        Projectile.hostile = true;
        Projectile.friendly = false;
        Projectile.tileCollide = false;
        Projectile.ignoreWater = true;
        Projectile.timeLeft = PORTAL_LIFETIME;
        Projectile.penetrate = -1;
        Projectile.damage = 0; 
        Projectile.alpha = 255;
    }

   
private const int PHASE_PARTICLES_END = 180;  
private const int SQUARE_COUNT = 6;
private const int SQUARE_INTERVAL = 20;
private const int SQUARES_END = PHASE_PARTICLES_END + SQUARE_COUNT * SQUARE_INTERVAL; 
private const int LASER_PAUSE = 120; 
private const int LASER_FRAME = SQUARES_END + LASER_PAUSE; 
 private const int PORTAL_LIFETIME = LASER_FRAME + 60; 
public override void AI()
{
    int ownerIndex = (int)Projectile.ai[0];
    if (ownerIndex < 0 || ownerIndex >= Main.maxNPCs || !Main.npc[ownerIndex].active)
    {
        Projectile.Kill();
        return;
    }

    NPC owner = Main.npc[ownerIndex];
    Player target = Main.player[owner.target];

    if (!target.active || target.dead)
    {
        Projectile.Kill();
        return;
    }

    
    if (Projectile.alpha > 0)
        Projectile.alpha = Math.Max(0, Projectile.alpha - 15);

    
    Vector2 dirToPlayer = (target.Center - owner.Center).SafeNormalize(Vector2.UnitX);
    Projectile.Center = owner.Center + dirToPlayer * 80f;
    Projectile.rotation = dirToPlayer.ToRotation() + MathHelper.PiOver2;

    Projectile.localAI[0]++;
    int timer = (int)Projectile.localAI[0];

    
 
    
    if (timer < PHASE_PARTICLES_END)
    {
        Projectile.localAI[1]++;
        if (Projectile.localAI[1] >= 20f && Main.netMode != NetmodeID.MultiplayerClient)
        {
            Projectile.localAI[1] = 0f;
            SpawnScytheShot(target);
        }
        return;
    }

    
    if (timer < SQUARES_END)
    {
        
        Projectile.localAI[1]++;
        if (Projectile.localAI[1] >= 20f && Main.netMode != NetmodeID.MultiplayerClient)
        {
            Projectile.localAI[1] = 0f;
            SpawnScytheShot(target);
        }

        
        int squareTimer = timer - PHASE_PARTICLES_END;
        int squareIndex = squareTimer / SQUARE_INTERVAL;
        int squareLocal = squareTimer % SQUARE_INTERVAL;

        if (squareLocal == 0 && squareIndex < SQUARE_COUNT && Main.netMode != NetmodeID.MultiplayerClient)
        {
            SpawnSquare(dirToPlayer, squareIndex);
            SoundEngine.PlaySound(
                new SoundStyle("flameout/Content/Sounds/em1"),
                Projectile.Center);
        }
        return;
    }

    
    if (timer < LASER_FRAME)
    {
        
        Projectile.localAI[1]++;
        if (Projectile.localAI[1] >= 20f && Main.netMode != NetmodeID.MultiplayerClient)
        {
            Projectile.localAI[1] = 0f;
            SpawnScytheShot(target);
        }

        
        if (timer % 10 == 0)
            Projectile.alpha = Projectile.alpha == 0 ? 30 : 0;

        return;
    }

    
    if (timer == LASER_FRAME)
    {
        Projectile.alpha = 0;
        if (Main.netMode != NetmodeID.MultiplayerClient)
        {
            SpawnLaser(dirToPlayer);
            SoundEngine.PlaySound(
                new SoundStyle("flameout/Content/Sounds/em2"),
                Projectile.Center);
        }
        return;
    }
}


    private void SpawnScytheShot(Player target)
    {
        Vector2 vel = (target.Center - Projectile.Center).SafeNormalize(Vector2.UnitX) * 16f; 
        
        vel = vel.RotatedByRandom(0.15f);

        Projectile.NewProjectile(
            Projectile.GetSource_FromThis(),
            Projectile.Center,
            vel,
            ModContent.ProjectileType<EmpathyScytheShot>(),
            200,
            0f,
            Main.myPlayer);
    }

    
private void SpawnSquare(Vector2 direction, int index)
{
    
    Vector2 vel = direction * 48f;

    Projectile.NewProjectile(
        Projectile.GetSource_FromThis(),
        Projectile.Center,
        vel,
        ModContent.ProjectileType<EmpathySquare>(),
        500,
        0f,
        Main.myPlayer);
}


private void SpawnLaser(Vector2 direction)
{
    int index = Projectile.NewProjectile(
        Projectile.GetSource_FromThis(),
        Projectile.Center,
        Vector2.Zero,
        ModContent.ProjectileType<EmpathyLaser2>(),
        1000,
        0f,
        Main.myPlayer);

    if (index >= 0 && index < Main.maxProjectiles)
    {
        
        Main.projectile[index].localAI[0] = direction.ToRotation() + MathHelper.PiOver2;
        Main.projectile[index].Center = Projectile.Center;
        NetMessage.SendData(MessageID.SyncProjectile, -1, -1, null, index);
    }
}

    public override bool PreDraw(ref Color lightColor)
    {
        Texture2D texture = ModContent.Request<Texture2D>(Texture).Value;
        Vector2 drawPos = Projectile.Center - Main.screenPosition;

        float alpha = 1f - Projectile.alpha / 255f;
        Color portalColor = new Color(255, 150, 220, 0) * alpha;

        
        float rotation = Projectile.rotation + Projectile.localAI[0] * 0.03f;

        
        for (int i = 0; i < 4; i++)
        {
            float scale = Projectile.scale * (1f + i * 0.12f);
            Color layerColor = portalColor * (0.2f - i * 0.04f);
            Main.spriteBatch.Draw(texture, drawPos, null,
                layerColor, rotation,
                texture.Size() / 2f, scale, SpriteEffects.None, 0f);
        }

        
        Main.spriteBatch.Draw(texture, drawPos, null,
            portalColor, rotation,
            texture.Size() / 2f, Projectile.scale, SpriteEffects.None, 0f);

        return false;
    }
}

public class EmpathySquare : ModProjectile
{
    public override string Texture => "flameout/Content/Blank"; 

    public override void SetStaticDefaults()
    {
        ProjectileID.Sets.DrawScreenCheckFluff[Type] = 10000;
    }

   

public override void SetDefaults()
{
    Projectile.width = 50;
    Projectile.height = 50;
    Projectile.hostile = true;
    Projectile.friendly = false;
    Projectile.tileCollide = false;
    Projectile.ignoreWater = true;
    Projectile.timeLeft = 360;
    Projectile.penetrate = -1;
    Projectile.damage = 500;
    Projectile.extraUpdates = 0;
}

public override void AI()
{
    
    
    Projectile.rotation += 0.1f;
    Projectile.velocity.Y = Projectile.velocity.Y; 
}




    public override bool PreDraw(ref Color lightColor)
    {
        Vector2 drawPos = Projectile.Center - Main.screenPosition;

        
        float alpha = 1f;
        Color squareColor = new Color(255, 180, 220) * alpha;

        
        Texture2D pixel = TextureAssets.MagicPixel.Value;

        Main.spriteBatch.Draw(pixel,
            drawPos,
            new Rectangle(0, 0, 1, 1),
            squareColor,
            Projectile.rotation,
            new Vector2(0.5f, 0.5f),
            new Vector2(Projectile.width, Projectile.height),
            SpriteEffects.None, 0f);

        return false;
    }
}

public class EmpathyLaser2 : ModProjectile
{
    public override string Texture => "flameout/Content/Blank";
    private const int LASER_LENGTH = 200 * 16; 
    private const int LASER_WIDTH = 50 * 16;   

    public override void SetStaticDefaults()
    {
        ProjectileID.Sets.DrawScreenCheckFluff[Type] = 10000;
    }

    public override void SetDefaults()
    {
        Projectile.width = LASER_WIDTH;
        Projectile.height = LASER_LENGTH;
        Projectile.hostile = true;
        Projectile.friendly = false;
        Projectile.tileCollide = false;
        Projectile.ignoreWater = true;
        Projectile.timeLeft = 40; 
        Projectile.penetrate = -1;
        Projectile.damage = 1000;
    }

 
public override void AI()
{
    
    Projectile.velocity = Vector2.Zero;
    Projectile.rotation = Projectile.localAI[0]; 
}


    public override bool PreDraw(ref Color lightColor)
    {
        Vector2 drawPos = Projectile.Center - Main.screenPosition;
        Texture2D pixel = TextureAssets.MagicPixel.Value;

        float alpha = (float)Projectile.timeLeft / 40f;
        Color laserColor = new Color(255, 150, 220, 0) * alpha;

        
        for (int i = 0; i < 3; i++)
        {
            float scale = 1f + i * 0.3f;
            Color glowColor = laserColor * (0.4f - i * 0.1f);
            Main.spriteBatch.Draw(pixel,
                drawPos,
                new Rectangle(0, 0, 1, 1),
                glowColor,
                Projectile.rotation,
                new Vector2(0.5f, 0.5f),
                new Vector2((10*16) * scale, (250*16)),
                SpriteEffects.None, 0f);
        }

        
        Main.spriteBatch.Draw(pixel,
            drawPos,
            new Rectangle(0, 0, 1, 1),
            laserColor,
            Projectile.rotation,
            new Vector2(0.5f, 0.5f),
            new Vector2((10*16), (250*16)),
            SpriteEffects.None, 0f);

        return false;
    }

    public override bool? Colliding(Rectangle projHitbox, Rectangle targetHitbox)
    {
        
        float _ = 0f;
        Vector2 start = Projectile.Center - Projectile.velocity.SafeNormalize(Vector2.UnitX) * (LASER_LENGTH / 2f);
        Vector2 end = Projectile.Center + Projectile.velocity.SafeNormalize(Vector2.UnitX) * (LASER_LENGTH / 2f);

        return Collision.CheckAABBvLineCollision(
            targetHitbox.TopLeft(), targetHitbox.Size(),
            start, end,
            LASER_WIDTH, ref _);
    }
}
public class EmpathyOrb : ModProjectile
{
    public override string Texture => "flameout/Content/Blank";

    
    

    public override void SetDefaults()
    {
        Projectile.width = 10 * 16;
        Projectile.height = 10 * 16;
        Projectile.hostile = false;
        Projectile.friendly = false;
        Projectile.tileCollide = false;
        Projectile.ignoreWater = true;
        Projectile.timeLeft = 120; 
        Projectile.penetrate = -1;
        Projectile.damage = 0;
    }

    public override void AI()
    {
        Projectile.velocity = Vector2.Zero;
    }

    public override bool PreDraw(ref Color lightColor)
    {
        Texture2D pixel = TextureAssets.MagicPixel.Value;
        Vector2 drawPos = Projectile.Center - Main.screenPosition;

        float pulse = (float)Math.Sin(Main.GameUpdateCount * 0.15f) * 0.2f + 0.8f;
        float alpha = (float)Projectile.timeLeft / 120f;
        Color orbColor = Color.White * alpha * pulse;

        
        int segments = 32;
        float radius = (Projectile.width / 2f) * pulse;

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
                orbColor, segRot,
                new Vector2(0.5f, 0.5f),
                new Vector2(segLen, 3f),
                SpriteEffects.None, 0f);
        }

        return false;
    }
}


public class EmpathyCross : ModProjectile
{
    public override string Texture => "flameout/Content/Blank";

    public override void SetDefaults()
    {
        Projectile.width = 50 * 16;
        Projectile.height = 50 * 16;
        Projectile.hostile = true;
        Projectile.friendly = false;
        Projectile.tileCollide = false;
        Projectile.ignoreWater = true;
        Projectile.timeLeft = 300; 
        Projectile.penetrate = -1;
        Projectile.damage = 500;
    }

    public override void AI()
    {
        Projectile.velocity = Vector2.Zero;
    }

   public override bool PreDraw(ref Color lightColor)
{
    Texture2D pixel = TextureAssets.MagicPixel.Value;
    Vector2 drawPos = Projectile.Center - Main.screenPosition;
    Rectangle src = new Rectangle(0, 0, 1, 1);
    Vector2 origin = new Vector2(0.5f, 0.5f);

    float alpha = (float)Projectile.timeLeft / 300f;
    float halfSize = Projectile.width / 2f;

    
    float pulse = 0.85f + 0.15f * (float)Math.Sin(Main.GlobalTimeWrappedHourly * 14f + Projectile.whoAmI);

    
   
Color glowRed   = new Color(180, 20, 30)   * (alpha * 0.45f);   
Color midRed    = new Color(255, 40, 60)   * (alpha * 0.65f);   
Color pinkLayer = new Color(255, 100, 180) * (alpha * 0.85f);   
Color coreWhite = new Color(255, 220, 235) * alpha;             

    
    Main.spriteBatch.End();
    Main.spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.Additive,
        Main.DefaultSamplerState, DepthStencilState.None, Main.Rasterizer, null, Main.GameViewMatrix.TransformationMatrix);

    
    void DrawCross(float length, float thickness, Color color, float rotation)
    {
        
        Main.spriteBatch.Draw(pixel, drawPos, src, color, rotation,
            origin, new Vector2(length * 2f, thickness), SpriteEffects.None, 0f);
        
        Main.spriteBatch.Draw(pixel, drawPos, src, color, rotation + MathHelper.PiOver2,
            origin, new Vector2(length * 2f, thickness), SpriteEffects.None, 0f);
    }

    
    DrawCross(halfSize * 0.7f, 6f * pulse, midRed * 0.5f, MathHelper.PiOver4);
    DrawCross(halfSize * 0.65f, 3f * pulse, pinkLayer * 0.6f, MathHelper.PiOver4);

    
    DrawCross(halfSize,          26f * pulse, glowRed, 0f);   
    DrawCross(halfSize,          16f * pulse, midRed, 0f);    
    DrawCross(halfSize * 0.97f,   9f * pulse, pinkLayer, 0f); 
    DrawCross(halfSize * 0.94f,   3.5f,       coreWhite, 0f); 

    
    Color tipColor = new Color(255, 60, 120) * (alpha * 0.35f);
    DrawCross(halfSize * 1.15f, 5f * pulse, tipColor, 0f);

    
    float flareScale = 30f * pulse;
    Main.spriteBatch.Draw(pixel, drawPos, src, midRed * 0.8f, MathHelper.PiOver4,
        origin, new Vector2(flareScale, flareScale), SpriteEffects.None, 0f);
    Main.spriteBatch.Draw(pixel, drawPos, src, coreWhite, MathHelper.PiOver4,
        origin, new Vector2(flareScale * 0.45f, flareScale * 0.45f), SpriteEffects.None, 0f);

    
    Main.spriteBatch.End();
    Main.spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend,
        Main.DefaultSamplerState, DepthStencilState.None, Main.Rasterizer, null, Main.GameViewMatrix.TransformationMatrix);

    return false;
}

    public override bool? Colliding(Rectangle projHitbox, Rectangle targetHitbox)
    {
        
        Rectangle hBar = new Rectangle(
            (int)(Projectile.Center.X - Projectile.width / 2f),
            (int)(Projectile.Center.Y - 8),
            Projectile.width, 16);

        Rectangle vBar = new Rectangle(
            (int)(Projectile.Center.X - 8),
            (int)(Projectile.Center.Y - Projectile.height / 2f),
            16, Projectile.height);

        return hBar.Intersects(targetHitbox) || vBar.Intersects(targetHitbox);
    }
}



public class EmpathySky : ModSystem
{
    public override void PostDrawTiles()
    {
        if (!Empathy.Phase12Active)
            return;

        Main.spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend);

        Main.spriteBatch.Draw(
            TextureAssets.MagicPixel.Value,
            new Rectangle(0, 0, Main.screenWidth, Main.screenHeight),
            new Color(180, 0, 0, 150));

        Main.spriteBatch.End();
    }
}
public class EmpathyScytheP13 : ModProjectile
{
    public override string Texture => "flameout/Content/EmpathyScythe";

    private int _laserTimer = 0;
    private bool _lasersSpawned = false;

    
    private const int TrailLen = 16;
    private const int TrailSubsteps = 3;
    private readonly Vector2[] _trailPos = new Vector2[TrailLen];
    private readonly float[] _trailRot = new float[TrailLen];
    private int _trailIndex = 0;
    private float _trailFade = 0f; 

    public override void SetDefaults()
    {
        Projectile.width = 40;
        Projectile.height = 40;
        Projectile.hostile = true;
        Projectile.friendly = false;
        Projectile.tileCollide = false;
        Projectile.ignoreWater = true;
        Projectile.timeLeft = 9999;
        Projectile.penetrate = -1;
        Projectile.damage = 400;
    }

    private void RecordTrail()
    {
        _trailPos[_trailIndex] = Projectile.Center;
        _trailRot[_trailIndex] = Projectile.rotation;
        _trailIndex = (_trailIndex + 1) % TrailLen;
        _trailFade = 1f;
    }

    public override void AI()
    {
        int bossIndex = (int)Projectile.ai[0];
        
        NPC boss = (bossIndex >= 0 && bossIndex < Main.maxNPCs && Main.npc[bossIndex].active)
            ? Main.npc[bossIndex] : null;

        
        if (Projectile.localAI[0] == 1f)
        {
            if (boss != null)
            {
                float angle = Main.GameUpdateCount * 0.15f;
                Projectile.Center = boss.Center + new Vector2(
                    (float)Math.Cos(angle),
                    (float)Math.Sin(angle)) * 60f;
            }
            RecordTrail();
            return;
        }

        
        if (Projectile.localAI[0] == 2f)
        {
            Projectile.localAI[1]--;
            Projectile.rotation = Projectile.velocity.ToRotation();
            RecordTrail();

            if (Projectile.localAI[1] <= 0f)
            {
                Projectile.velocity = Vector2.Zero;
                Projectile.localAI[0] = 3f;
                Projectile.damage = 0;

                if (Main.netMode != NetmodeID.MultiplayerClient && !_lasersSpawned)
                {
                    _lasersSpawned = true;
                    SpawnLasers(boss);
                }
            }
            return;
        }

        
        if (Projectile.localAI[0] == 3f)
        {
            Projectile.velocity = Vector2.Zero;
            _trailFade = Math.Max(0f, _trailFade - 0.07f);
            _laserTimer++;

            if (_laserTimer >= 180)
            {
                if (boss != null)
                    boss.localAI[2] = 1f;
                Projectile.Kill();
            }
        }
    }

    public override bool PreDraw(ref Color lightColor)
    {
        Texture2D tex = ModContent.Request<Texture2D>(Texture).Value;
        Vector2 origin = tex.Size() / 2f;
        SpriteBatch sb = Main.spriteBatch;

        if (_trailFade > 0.01f)
        {
            sb.End();
            sb.Begin(SpriteSortMode.Deferred, BlendState.Additive, SamplerState.LinearClamp,
                DepthStencilState.None, RasterizerState.CullNone, null,
                Main.GameViewMatrix.TransformationMatrix);

            for (int i = TrailLen - 2; i >= 1; i--)
            {
                int idx = (_trailIndex - 1 - i + TrailLen * 2) % TrailLen;
                int idxNext = (idx + 1) % TrailLen;

                Vector2 posA = _trailPos[idx];
                Vector2 posB = _trailPos[idxNext];
                if (posA == Vector2.Zero || posB == Vector2.Zero) continue;

                for (int s = 0; s < TrailSubsteps; s++)
                {
                    float sub = s / (float)TrailSubsteps;
                    float progress = (i - sub) / (float)TrailLen; 
                    Vector2 pos = Vector2.Lerp(posA, posB, sub);
                    float rot = MathHelper.Lerp(_trailRot[idx], _trailRot[idxNext], sub);

                    
                    Color col = Color.Lerp(new Color(255, 150, 200), new Color(200, 40, 80), progress)
                        * ((1f - progress) * 0.45f * _trailFade);
                    float scale = Projectile.scale * (1f - 0.07f * progress);

                    sb.Draw(tex, pos - Main.screenPosition, null, col,
                        rot, origin, scale, SpriteEffects.None, 0f);
                }
            }

            
            sb.Draw(tex, Projectile.Center - Main.screenPosition, null,
                new Color(255, 110, 180) * (0.35f * _trailFade), Projectile.rotation,
                origin, Projectile.scale * 1.05f, SpriteEffects.None, 0f);

            sb.End();
            sb.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.LinearClamp,
                DepthStencilState.None, RasterizerState.CullNone, null,
                Main.GameViewMatrix.TransformationMatrix);
        }

        
        sb.Draw(tex, Projectile.Center - Main.screenPosition, null,
            lightColor, Projectile.rotation, origin, Projectile.scale, SpriteEffects.None, 0f);

        return false;
    }

    private void SpawnLasers(NPC boss)
    {
        if (boss == null) return;

        Vector2 toBase = boss.Center - Projectile.Center;
        float mainAngle = toBase.ToRotation();
        float mainLength = toBase.Length();
        float oppositeAngle = mainAngle + MathHelper.Pi;

        SpawnSingleLaser2(Projectile.Center, oppositeAngle, mainLength);
        SpawnSingleLaser2(Projectile.Center, mainAngle, mainLength);

        for (int i = 1; i <= 5; i++)
        {
            float t = (float)i / 6f;
            Vector2 pointOnMain = Projectile.Center + toBase * t;

            SpawnSingleLaser(pointOnMain, mainAngle + MathHelper.PiOver2, 300 * 16f);
            SpawnSingleLaser(pointOnMain, mainAngle - MathHelper.PiOver2, 300 * 16f);
        }
    }

    private void SpawnSingleLaser(Vector2 pos, float angle, float length)
    {
        int index = Projectile.NewProjectile(
            Projectile.GetSource_FromAI(),
            pos,
            Vector2.Zero,
            ModContent.ProjectileType<EmpathyDirLaser>(),
            400, 0f, Main.myPlayer,
            -1,
            0);

        
        if (index >= 0 && index < Main.maxProjectiles)
        {
            Color c = length > 500f ? new Color(255, 100, 180) : Color.White;
            int packed = (c.R << 16) | (c.G << 8) | c.B;
            Main.projectile[index].ai[1] = packed;
            Main.projectile[index].localAI[0] = angle + MathHelper.PiOver2;
            Main.projectile[index].localAI[1] = length;
            Main.projectile[index].timeLeft = 180;

            NetMessage.SendData(MessageID.SyncProjectile, -1, -1, null, index);
        }
    }

    private void SpawnSingleLaser2(Vector2 pos, float angle, float length)
    {
        float correctedAngle = angle - MathHelper.PiOver2;
        int index = Projectile.NewProjectile(
            Projectile.GetSource_FromAI(),
            pos,
            Vector2.Zero,
            ModContent.ProjectileType<EmpathyDirLaser>(),
            400,
            0f,
            Main.myPlayer
        );

        if (index >= 0 && index < Main.maxProjectiles)
        {
            Color c = length > 500f ? new Color(255, 100, 180) : Color.White;
            int packed = (c.R << 16) | (c.G << 8) | c.B;

            Main.projectile[index].ai[1] = packed;
            Main.projectile[index].localAI[0] = correctedAngle;
            Main.projectile[index].localAI[1] = length;
            Main.projectile[index].timeLeft = 180;

            if (Main.netMode != NetmodeID.SinglePlayer)
            {
                NetMessage.SendData(MessageID.SyncProjectile, -1, -1, null, index);
            }
        }
    }
}
public class EmpathyDirLaser : ModProjectile
{
    public override string Texture => "flameout/Content/Blank";

    private const int LASER_LENGTH = 250 * 16;
    private const int LASER_WIDTH = 10 * 16;

    
    
    
    

    public override void SetStaticDefaults()
    {
        ProjectileID.Sets.DrawScreenCheckFluff[Type] = 10000;
    }

    public override void SetDefaults()
    {
        Projectile.width = LASER_WIDTH;
        Projectile.height = LASER_LENGTH;
        Projectile.hostile = true;
        Projectile.friendly = false;
        Projectile.tileCollide = false;
        Projectile.ignoreWater = true;
        Projectile.timeLeft = 180; 
        Projectile.penetrate = -1;
        Projectile.damage = 300;
    }

    public override void AI()
    {
        Projectile.velocity = Vector2.Zero;
    }

    
   
public static Color UnpackColor(int packed)
{
    int r = (packed >> 16) & 0xFF;
    int g = (packed >> 8)  & 0xFF;
    int b = packed         & 0xFF;
    return new Color(r, g, b);
}


    public override bool PreDraw(ref Color lightColor)
    {
        Texture2D pixel = TextureAssets.MagicPixel.Value;
        Vector2 drawPos = Projectile.Center - Main.screenPosition;

        float alpha = (float)Projectile.timeLeft / 180f;
        
Color laserColor = EmpathyDirLaser.UnpackColor((int)Projectile.ai[1]) * alpha;

        float rotation = Projectile.localAI[0];

        
        for (int i = 0; i < 3; i++)
        {
            float widthScale = LASER_WIDTH * (1f + i * 0.3f);
            Color glowColor = laserColor * (0.4f - i * 0.1f);
            Main.spriteBatch.Draw(pixel, drawPos, new Rectangle(0, 0, 1, 1),
            glowColor, rotation,
                new Vector2(0.5f, 0f),
                new Vector2(widthScale, LASER_LENGTH),
                SpriteEffects.None, 0f);
        }

        
        Main.spriteBatch.Draw(pixel, drawPos, new Rectangle(0, 0, 1, 1),
            laserColor, rotation,
            new Vector2(0.5f, 0f),
            new Vector2(LASER_WIDTH, LASER_LENGTH),
            SpriteEffects.None, 0f);

        return false;
    }

    public override bool? Colliding(Rectangle projHitbox, Rectangle targetHitbox)
    {
        float _ = 0f;
        float rot = Projectile.localAI[0] - MathHelper.PiOver2;
        Vector2 dir = rot.ToRotationVector2();
        Vector2 start = Projectile.Center;
        Vector2 end = start + dir * LASER_LENGTH;

        return Collision.CheckAABBvLineCollision(
            targetHitbox.TopLeft(), targetHitbox.Size(),
            start, end,
            LASER_WIDTH, ref _);
    }
}
public class EmpathySword2 : ModProjectile
{
    private NPC Owner => Main.npc[(int)Projectile.ai[0]];

    public override string Texture => "flameout/Content/empathysword";

    
    private const int TrailLen = 18;
    private const int TrailSubsteps = 3;          
    private const float TeleportBreakDist = 200f; 
    private readonly Vector2[] _trailPos = new Vector2[TrailLen];
    private readonly float[] _trailRot = new float[TrailLen];
    private int _trailIndex = 0;

    public override void SetDefaults()
    {
        Projectile.width = 64;
        Projectile.height = 64;
        Projectile.hostile = true;
        Projectile.friendly = false;
        Projectile.tileCollide = false;
        Projectile.ignoreWater = true;
        Projectile.timeLeft = 3600;
        Projectile.damage = 200;
        Projectile.penetrate = -1;
        Projectile.alpha = 0;
    }

    public override void SetStaticDefaults()
    {
        ProjectileID.Sets.DrawScreenCheckFluff[Type] = 10000;
    }

    public override void AI()
    {
        NPC owner = Owner;
        if (!owner.active)
        {
            Projectile.Kill();
            return;
        }

        Projectile.Center = owner.Center;
        Projectile.velocity = Vector2.Zero;

        Player target = Main.player[Player.FindClosest(Projectile.Center, 1, 1)];
        Vector2 toPlayer = target.Center - Projectile.Center;
        Projectile.rotation = toPlayer.ToRotation();

        
        Vector2 last = _trailPos[(_trailIndex - 1 + TrailLen) % TrailLen];
        if (last != Vector2.Zero && Vector2.Distance(last, Projectile.Center) > TeleportBreakDist)
        {
            
            for (int i = 0; i < TrailLen; i++)
                _trailPos[i] = Vector2.Zero;
        }
        _trailPos[_trailIndex] = Projectile.Center;
        _trailRot[_trailIndex] = Projectile.rotation;
        _trailIndex = (_trailIndex + 1) % TrailLen;
    }

    public override bool PreDraw(ref Color lightColor)
    {
        Texture2D texture = ModContent.Request<Texture2D>(Texture).Value;
        Vector2 origin = texture.Size() / 2f;
        SpriteBatch sb = Main.spriteBatch;

        
        sb.End();
        sb.Begin(SpriteSortMode.Deferred, BlendState.Additive, SamplerState.LinearClamp,
            DepthStencilState.None, RasterizerState.CullNone, null,
            Main.GameViewMatrix.TransformationMatrix);

        for (int i = TrailLen - 2; i >= 1; i--)
        {
            int idx = (_trailIndex - 1 - i + TrailLen * 2) % TrailLen;
            int idxNext = (idx + 1) % TrailLen;

            Vector2 posA = _trailPos[idx];
            Vector2 posB = _trailPos[idxNext];
            if (posA == Vector2.Zero || posB == Vector2.Zero) continue;

            for (int s = 0; s < TrailSubsteps; s++)
            {
                float sub = s / (float)TrailSubsteps;
                float progress = (i - sub) / (float)TrailLen; 
                Vector2 pos = Vector2.Lerp(posA, posB, sub);
                float rot = MathHelper.Lerp(_trailRot[idx], _trailRot[idxNext], sub);

                
                Color col = Color.Lerp(new Color(255, 150, 200), new Color(200, 40, 80), progress)
                    * ((1f - progress) * 0.45f);
                float scale = Projectile.scale * (1f - 0.06f * progress);

                sb.Draw(texture, pos - Main.screenPosition, null, col,
                    rot, origin, scale, SpriteEffects.None, 0f);
            }
        }

        
        sb.Draw(texture, Projectile.Center - Main.screenPosition, null,
            new Color(255, 110, 180) * 0.35f, Projectile.rotation,
            origin, Projectile.scale * 1.05f, SpriteEffects.None, 0f);

        sb.End();
        sb.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.LinearClamp,
            DepthStencilState.None, RasterizerState.CullNone, null,
            Main.GameViewMatrix.TransformationMatrix);

        
        Vector2 drawPos = Projectile.Center - Main.screenPosition;
        sb.Draw(texture, drawPos, null, lightColor, Projectile.rotation,
            origin, Projectile.scale, SpriteEffects.None, 0f);

        return false;
    }
}
public class EmpathyAttack14
    {
        
        private enum State
        {
            Teleport,
            Dash,
            Finish
        }

        private State _state = State.Teleport;
        private int _dashCount = 0;
        private const int MaxDashes = 3;

        
        private Vector2 _dashTarget;
        
        private const float DashSpeed = 40f;
        
        private const float OvershootDistance = 20 * 16f;

        
        private int _swordIndex = -1;

        private NPC _npc;

        public EmpathyAttack14(NPC npc)
        {
            _npc = npc;
        }

        public void Init()
        {
            
            _npc.frame.Y = 1; 

            
            if (Main.netMode != NetmodeID.MultiplayerClient)
            {
                _swordIndex = Projectile.NewProjectile(
                    _npc.GetSource_FromAI(),
                    _npc.Center,
                    Vector2.Zero,
                    ModContent.ProjectileType<EmpathySword2>(),
                    200, 0f, Main.myPlayer,
                    _npc.whoAmI
                );
            }
        }

        public bool Update()
        {
            if (_state == State.Finish)
                return true; 

            
            

            switch (_state)
            {
                case State.Teleport:
                    DoTeleport();
                    break;

                case State.Dash:
                    if (DoDash())
                    {
                        _dashCount++;
                        if (_dashCount >= MaxDashes)
                        {
                            FinishAttack();
                        }
                        else
                        {
                            _state = State.Teleport;
                        }
                    }
                    break;
            }

            return false;
        }

        
        private void DoTeleport()
        {
            Player target = Main.player[_npc.target];

            
            float angle = (float)(Main.rand.NextDouble() * MathHelper.TwoPi);
            float radius = 50 * 16f; 
            Vector2 teleportPos = target.Center + new Vector2(
                (float)Math.Cos(angle),
                (float)Math.Sin(angle)) * radius;

            _npc.Center = teleportPos;

            
            Vector2 dirToTarget = (target.Center - _npc.Center);
            dirToTarget.Normalize();
            _dashTarget = target.Center + dirToTarget * OvershootDistance;

            _state = State.Dash;

            if (Main.netMode == NetmodeID.Server)
                NetMessage.SendData(MessageID.SyncNPC, -1, -1, null, _npc.whoAmI);
        }

        
        private bool DoDash()
        {
            Vector2 toTarget = _dashTarget - _npc.Center;

            if (toTarget.Length() <= DashSpeed)
            {
                
                _npc.Center = _dashTarget;
                _npc.velocity = Vector2.Zero;
                return true;
            }

            toTarget.Normalize();
            _npc.velocity = toTarget * DashSpeed;
            return false;
        }

        private void FinishAttack()
        {
            _state = State.Finish;

            
            if (_swordIndex >= 0 && _swordIndex < Main.maxProjectiles)
            {
                Main.projectile[_swordIndex].Kill();
                _swordIndex = -1;
            }

            
            _npc.frame.Y = 0; 
        }
    }
 public class EmpathyAttack15
    {
        private NPC _npc;
        private int _portalIndex = -1;
        private bool _done = false;

        public EmpathyAttack15(NPC npc)
        {
            _npc = npc;
        }

        public void Init()
        {
            Player target = Main.player[_npc.target];

            
            _npc.Center = target.Center + new Vector2(-50 * 16f, 0f);

            
            _npc.frame.Y = 1;

            if (Main.netMode != NetmodeID.MultiplayerClient)
            {
                _portalIndex = Projectile.NewProjectile(
                    _npc.GetSource_FromAI(),
                    _npc.Center,
                    Vector2.Zero,
                    ModContent.ProjectileType<EmpathyPortal3>(),
                    0, 0f, Main.myPlayer,
                    _npc.whoAmI
                );

                NetMessage.SendData(MessageID.SyncNPC, -1, -1, null, _npc.whoAmI);
            }
        }

        public bool Update()
        {
            if (_done) return true;

            
            
            if (_portalIndex >= 0 && _portalIndex < Main.maxProjectiles)
            {
                var proj = Main.projectile[_portalIndex];
                if (!proj.active)
                {
                    FinishAttack();
                    return true;
                }
            }
            else
            {
                FinishAttack();
                return true;
            }

            return false;
        }

        private void FinishAttack()
        {
            _done = true;
            _npc.frame.Y = 0; 
        }
    }
 public class EmpathyPortal3 : ModProjectile
    {
        public override string Texture => "flameout/Content/EmpathyPortal";

        
        
        

        private const int TRACK_TIME = 60;
private const int FREEZE_TIME = 60;
private const int LASER_FRAME = TRACK_TIME + FREEZE_TIME;
private const int PORTAL_LIFETIME = LASER_FRAME + 180;

        public override void SetStaticDefaults()
        {
            ProjectileID.Sets.DrawScreenCheckFluff[Type] = 10000;
        }

        public override void SetDefaults()
        {
            Projectile.width = 200;
            Projectile.height = 200;
            Projectile.hostile = true;
            Projectile.friendly = false;
            Projectile.tileCollide = false;
            Projectile.ignoreWater = true;
            Projectile.timeLeft = PORTAL_LIFETIME;
            Projectile.penetrate = -1;
            Projectile.damage = 0;
            Projectile.alpha = 255;
        }

        public override void AI()
        {
            int ownerIndex = (int)Projectile.ai[0];
            if (ownerIndex < 0 || ownerIndex >= Main.maxNPCs || !Main.npc[ownerIndex].active)
            {
                Projectile.Kill();
                return;
            }

            NPC owner = Main.npc[ownerIndex];
            Player target = Main.player[owner.target];

            if (!target.active || target.dead)
            {
                Projectile.Kill();
                return;
            }

            
            if (Projectile.alpha > 0)
    Projectile.alpha = Math.Max(0, Projectile.alpha - 30);

            Projectile.localAI[0]++;
            int timer = (int)Projectile.localAI[0];

            
            Projectile.Center = owner.Center;

            
            if (timer < TRACK_TIME)
            {
                Vector2 dirToPlayer = (target.Center - Projectile.Center).SafeNormalize(Vector2.UnitX);
                Projectile.rotation = dirToPlayer.ToRotation() + MathHelper.PiOver2;

                
                Projectile.localAI[1] = Projectile.rotation;
                return;
            }

            
            if (timer < LASER_FRAME)
            {
                
                Projectile.rotation = Projectile.localAI[1];

                
              if (timer % 5 == 0)
    Projectile.alpha = Projectile.alpha == 0 ? 30 : 0;

                return;
            }

            
            if (timer == LASER_FRAME)
            {
                Projectile.alpha = 0;

                if (Main.netMode != NetmodeID.MultiplayerClient)
                {
                    SpawnLasers();
                    SpawnFlash();
                    SoundEngine.PlaySound(
                        new SoundStyle("flameout/Content/Sounds/em2"),
                        Projectile.Center);
                }
                return;
            }

            
            
        }

        private void SpawnLasers()
        {
            float baseAngle = Projectile.localAI[1] - MathHelper.PiOver2; 

            
            SpawnSingleLaser(baseAngle, 0f, false);

            
            SpawnSingleLaser(baseAngle, 1f, true);  
            SpawnSingleLaser(baseAngle, -1f, true); 
        }

        private void SpawnSingleLaser(float baseAngle, float spreadDir, bool spreads)
        {
            int index = Projectile.NewProjectile(
                Projectile.GetSource_FromThis(),
                Projectile.Center,
                Vector2.Zero,
                ModContent.ProjectileType<EmpathyLaser3>(),
                1000, 0f, Main.myPlayer);

            if (index >= 0 && index < Main.maxProjectiles)
            {
                var laser = Main.projectile[index];
                laser.localAI[0] = baseAngle;   
                laser.localAI[1] = spreadDir;   
                laser.ai[0] = spreads ? 1f : 0f; 
                laser.Center = Projectile.Center;
                NetMessage.SendData(MessageID.SyncProjectile, -1, -1, null, index);
            }
        }

        private void SpawnFlash()
        {
            Projectile.NewProjectile(
                Projectile.GetSource_FromThis(),
                Projectile.Center,
                Vector2.Zero,
                ModContent.ProjectileType<EmpathyScreenFlash>(),
                0, 0f, Main.myPlayer);
        }

        public override bool PreDraw(ref Color lightColor)
        {
            Texture2D texture = ModContent.Request<Texture2D>(Texture).Value;
            Vector2 drawPos = Projectile.Center - Main.screenPosition;

            float alpha = 1f - Projectile.alpha / 255f;
            Color portalColor = new Color(255, 150, 220, 0) * alpha;
          float rotation = Projectile.rotation + Projectile.localAI[0] * 0.06f;
            for (int i = 0; i < 4; i++)
            {
                float scale = Projectile.scale * (1f + i * 0.12f);
                Color layerColor = portalColor * (0.2f - i * 0.04f);
                Main.spriteBatch.Draw(texture, drawPos, null,
                    layerColor, rotation,
                    texture.Size() / 2f, scale, SpriteEffects.None, 0f);
            }

            Main.spriteBatch.Draw(texture, drawPos, null,
                portalColor, rotation,
                texture.Size() / 2f, Projectile.scale, SpriteEffects.None, 0f);

            return false;
        }
    }
  public class EmpathyLaser3 : ModProjectile
    {
        
        
        

        public override string Texture => "flameout/Content/EmpathyLaser";

       private const int LIFETIME = 180;
private const int SPREAD_TIME = 75;
private const float MAX_SPREAD = 2 * MathHelper.PiOver2;

        
        private float _spreadAngle = 0f;

        public override void SetStaticDefaults()
        {
            ProjectileID.Sets.DrawScreenCheckFluff[Type] = 10000;
        }

        public override void SetDefaults()
        {
            Projectile.width = 16;
            Projectile.height = 16;
            Projectile.hostile = true;
            Projectile.friendly = false;
            Projectile.tileCollide = false;
            Projectile.ignoreWater = true;
            Projectile.timeLeft = LIFETIME;
            Projectile.penetrate = -1;
            Projectile.damage = 800;
            Projectile.alpha = 0;
        }

        public override void AI()
        {
            Projectile.velocity = Vector2.Zero;

            int elapsed = LIFETIME - Projectile.timeLeft;

            bool spreads = Projectile.ai[0] > 0.5f;

            if (spreads && elapsed <= SPREAD_TIME)
            {
                
                float t = elapsed / (float)SPREAD_TIME;
                Projectile.localAI[2] = Projectile.localAI[1] * (MAX_SPREAD / 2f) * t;
            }
        }

public override bool? Colliding(Rectangle projHitbox, Rectangle targetHitbox)
{
    float currentAngle = Projectile.localAI[0] + Projectile.localAI[2];
    Vector2 dir = currentAngle.ToRotationVector2();
    Vector2 start = Projectile.Center;
    Vector2 end = start + dir * 2000f;
    float _ = 0f;
    return Collision.CheckAABBvLineCollision(
        targetHitbox.TopLeft(), targetHitbox.Size(),
        start, end, 8f, ref _);
}



public override bool PreDraw(ref Color lightColor)
{
    Texture2D pixel = Terraria.GameContent.TextureAssets.MagicPixel.Value;

    float currentAngle = Projectile.localAI[0] + Projectile.localAI[2];
    Vector2 dir = currentAngle.ToRotationVector2();
    Vector2 origin = Projectile.Center - Main.screenPosition;
    float laserLength = 2000f;

    int elapsed = LIFETIME - Projectile.timeLeft;
   float fadeIn = MathHelper.Clamp(elapsed / 5f, 0f, 1f);
float fadeOut = MathHelper.Clamp(Projectile.timeLeft / 15f, 0f, 1f);
float masterAlpha = fadeIn * fadeOut;
float pulse = (float)Math.Sin(elapsed * 0.16f) * 0.5f + 0.5f;
    
    DrawBeam(pixel, origin, dir, laserLength,
        new Color(255, 80, 180) * 0.25f * masterAlpha, 90f);

    
    DrawBeam(pixel, origin, dir, laserLength,
        new Color(255, 140, 220) * 0.4f * masterAlpha, 50f);

    
    DrawBeam(pixel, origin, dir, laserLength,
        new Color(255, 220, 255) * 0.7f * masterAlpha, 22f);

    
    DrawBeam(pixel, origin, dir, laserLength,
        Color.White * masterAlpha, 8f);

    
    float flareSize = 30f + pulse * 15f;
    Main.spriteBatch.Draw(pixel, origin,
        new Rectangle(0, 0, 1, 1),
        Color.White * masterAlpha,
        0f, new Vector2(0.5f),
        flareSize * masterAlpha,
        SpriteEffects.None, 0f);

    return false;
}

private void DrawBeam(Texture2D pixel, Vector2 origin, Vector2 dir,
    float length, Color color, float width)
{
    Vector2 center = origin + dir * (length / 2f);
    Main.spriteBatch.Draw(
        pixel,
        center,
        new Rectangle(0, 0, 1, 1),
        color,
        dir.ToRotation(),
        new Vector2(0.5f, 0.5f),
        new Vector2(length, width),
        SpriteEffects.None, 0f);
}




    }
 public class EmpathyScreenFlash : ModProjectile
    {
        public override string Texture => "flameout/Content/EmpathyPortal"; 

        private const int LIFETIME = 60; 

        public override void SetDefaults()
        {
            Projectile.width = 1;
            Projectile.height = 1;
            Projectile.hostile = false;
            Projectile.friendly = false;
            Projectile.tileCollide = false;
            Projectile.ignoreWater = true;
            Projectile.timeLeft = LIFETIME;
            Projectile.penetrate = -1;
            Projectile.damage = 0;
            Projectile.alpha = 255;
        }

        public override void AI()
        {
            
            int elapsed = LIFETIME - Projectile.timeLeft;
            if (elapsed < 20)
            {
                float intensity = (1f - elapsed / 20f) * 12f;
                Main.instance.CameraModifiers.Add(
                    new PunchCameraModifier(
                        Projectile.Center,
                        Main.rand.NextVector2Unit(),
                        intensity,
                        6f, 20, 1000f));
            }
        }

        public override bool PreDraw(ref Color lightColor)
        {
            int elapsed = LIFETIME - Projectile.timeLeft;
            float alpha;

            
            
            if (elapsed < 10)
                alpha = elapsed / 10f;
            else
                alpha = 1f - (elapsed - 10f) / 50f;

            alpha = MathHelper.Clamp(alpha, 0f, 1f);

            if (alpha <= 0f) return false;

            
            Texture2D pixel = Terraria.GameContent.TextureAssets.MagicPixel.Value;
            Rectangle screen = new Rectangle(0, 0, Main.screenWidth, Main.screenHeight);
            Main.spriteBatch.Draw(pixel, screen, Color.White * alpha);

            return false;
        }
    }
 public class EmpathyBarrier : ModProjectile
    {
        

        public override string Texture => "flameout/Content/EmpathyPortal"; 

        private const float WALL_LENGTH = 100 * 16f; 
        private const float WALL_WIDTH  = 20f;

        public override void SetStaticDefaults()
        {
            ProjectileID.Sets.DrawScreenCheckFluff[Type] = 999999;
        }

        public override void SetDefaults()
        {
            Projectile.width  = 32;
            Projectile.height = 32;
            Projectile.hostile    = false; 
            Projectile.friendly   = false;
            Projectile.tileCollide = false;
            Projectile.ignoreWater = true;
            Projectile.timeLeft   = 9999;
            Projectile.penetrate  = -1;
            Projectile.damage     = 0;
            Projectile.alpha      = 255;
        }

        public override void AI()
        {
            Projectile.velocity = Vector2.Zero;

            
            if (Projectile.alpha > 0)
                Projectile.alpha -= 10;
        }

        public override bool PreDraw(ref Color lightColor)
        {
            Texture2D pixel = Terraria.GameContent.TextureAssets.MagicPixel.Value;

            float alpha  = 1f - Projectile.alpha / 255f;
            float angle  = Projectile.ai[0];
            Vector2 dir  = angle.ToRotationVector2();
            Vector2 origin = Projectile.Center - Main.screenPosition;

            
            DrawWall(pixel, origin, dir, WALL_LENGTH,
                new Color(255, 200, 240) * 0.2f * alpha, 60f);

            
            DrawWall(pixel, origin, dir, WALL_LENGTH,
                new Color(255, 220, 255) * 0.4f * alpha, 30f);

            
            DrawWall(pixel, origin, dir, WALL_LENGTH,
                Color.White * 0.9f * alpha, 10f);

            
            DrawWall(pixel, origin, dir, WALL_LENGTH,
                Color.White * alpha, 3f);

            return false;
        }

        private void DrawWall(Texture2D pixel, Vector2 origin, Vector2 dir,
            float length, Color color, float width)
        {
            Vector2 center = origin; 
            Main.spriteBatch.Draw(
                pixel,
                center,
                new Rectangle(0, 0, 1, 1),
                color,
                dir.ToRotation(),
                new Vector2(0.5f, 0.5f),
                new Vector2(length, width),
                SpriteEffects.None, 0f);
        }
    }
  public class EmpathyAttack50Percent
    {
        private NPC _npc;
        private bool _done = false;
        private int _timer = 0;
        private const int ATTACK_DURATION = 1200; 

        private int _spawnTimer = 0;
        private const int SPAWN_INTERVAL = 4; 
        private float _angle = 0f;
        private const int PROJECTILES_PER_RING = 20;

        private Vector2 _arenaCenter;
        private const float ARENA_HALF_SIZE = 50 * 16f; 

        public EmpathyAttack50Percent(NPC npc)
        {
            _npc = npc;
        }

        public void Init()
        {
            Player target = Main.player[_npc.target];
            _arenaCenter = target.Center; 

            
            for (int i = 0; i < Main.maxProjectiles; i++)
            {
                if (Main.projectile[i].active && Main.projectile[i].hostile)
                    Main.projectile[i].Kill();
            }

            
            _npc.Center = target.Center - new Vector2(0f, 20 * 16f);
            _npc.velocity = Vector2.Zero;
            _npc.netUpdate = true;

            if (Main.netMode != NetmodeID.MultiplayerClient)
            {
                SpawnBarriers();
            }
        }

       
private void SpawnBarriers()
{
    
    float offset = ARENA_HALF_SIZE; 

    
    Projectile.NewProjectile(_npc.GetSource_FromAI(), _arenaCenter + new Vector2(-offset, 0), Vector2.Zero, 
        ModContent.ProjectileType<EmpathyBarrier>(), 0, 0f, Main.myPlayer, MathHelper.PiOver2);

    
    Projectile.NewProjectile(_npc.GetSource_FromAI(), _arenaCenter + new Vector2(offset, 0), Vector2.Zero, 
        ModContent.ProjectileType<EmpathyBarrier>(), 0, 0f, Main.myPlayer, MathHelper.PiOver2);

    
    Projectile.NewProjectile(_npc.GetSource_FromAI(), _arenaCenter + new Vector2(0, -offset), Vector2.Zero, 
        ModContent.ProjectileType<EmpathyBarrier>(), 0, 0f, Main.myPlayer, 0f);

    
    Projectile.NewProjectile(_npc.GetSource_FromAI(), _arenaCenter + new Vector2(0, offset), Vector2.Zero, 
        ModContent.ProjectileType<EmpathyBarrier>(), 0, 0f, Main.myPlayer, 0f);
}


public bool Update()
{
    if (_done) return true;

    _timer++;
    _npc.velocity = Vector2.Zero;
    _npc.dontTakeDamage = true;

    
    if (Main.netMode != NetmodeID.MultiplayerClient)
    {
        _spawnTimer++;
        if (_spawnTimer >= SPAWN_INTERVAL)
        {
            _spawnTimer = 0;
            float step = MathHelper.TwoPi / PROJECTILES_PER_RING;
            for (int i = 0; i < PROJECTILES_PER_RING; i++)
            {
                Vector2 vel = (_angle + step * i).ToRotationVector2() * 7f;
                Projectile.NewProjectile(_npc.GetSource_FromAI(), _npc.Center, vel, 
                    ModContent.ProjectileType<EmpathyParticle3>(), 150, 0f, Main.myPlayer);
            }
            _angle += 0.2f;
        }
    }

    
    foreach (Player p in Main.player)
    {
        
        if (!p.active || p.dead) continue;

        float diffX = Math.Abs(p.Center.X - _arenaCenter.X);
        float diffY = Math.Abs(p.Center.Y - _arenaCenter.Y);

        
        if (diffX > ARENA_HALF_SIZE || diffY > ARENA_HALF_SIZE)
        {
            p.KillMe(Terraria.DataStructures.PlayerDeathReason.ByCustomReason("it's useless to run."), 9999, 0);
        }
    }

    if (_timer >= ATTACK_DURATION)
    {
        _npc.life = _npc.lifeMax;
        _npc.dontTakeDamage = false;
        _npc.netUpdate = true;

        for (int i = 0; i < Main.maxProjectiles; i++)
        {
            Projectile p = Main.projectile[i];
            if (p.active && p.type == ModContent.ProjectileType<EmpathyBarrier>())
            {
                p.Kill();
            }
        }

        for (int i = 0; i < Main.maxProjectiles; i++)
        {
            Projectile p = Main.projectile[i];
            if (p.active && p.type == ModContent.ProjectileType<EmpathyParticle3>())
            {
                p.Kill();
            }
        }
        if (_npc.ModNPC is Empathy empathyBoss)
        {
            empathyBoss.Phasee2 = true;
        }
        _done = true;
        return true; 
    }

    
    return false; 

    }
    }
 public class EmpathyAttackFinal
{
    private NPC _npc;
    private bool _done = false;
    private int _timer = 0;
    private const int ATTACK_DURATION = 1800;      
    private const int DEATH_TIMER = 120;
    private Vector2 _arenaCenter;
    private const float ARENA_HALF_SIZE = 50 * 16f;
    private float _rotateAngle = 0f;
    private float _rotateDir = 1f;                 

    public EmpathyAttackFinal(NPC npc) => _npc = npc;

    public void Init()
    {
        Player target = Main.player[_npc.target];
        _arenaCenter = target.Center;
        _npc.Center = _arenaCenter - new Vector2(0, 100);
        _npc.velocity = Vector2.Zero;
        _npc.netUpdate = true;
        if (Main.netMode != NetmodeID.MultiplayerClient) SpawnBarriers();
    }

    private void SpawnBarriers()
    {
        float offset = ARENA_HALF_SIZE;
        Vector2[] pos = { _arenaCenter + new Vector2(-offset, 0), _arenaCenter + new Vector2(offset, 0),
                          _arenaCenter + new Vector2(0, -offset), _arenaCenter + new Vector2(0, offset) };
        float[] rots = { MathHelper.PiOver2, MathHelper.PiOver2, 0f, 0f };
        for (int i = 0; i < 4; i++)
            Projectile.NewProjectile(_npc.GetSource_FromAI(), pos[i], Vector2.Zero,
                ModContent.ProjectileType<EmpathyBarrier>(), 0, 0f, Main.myPlayer, rots[i]);
    }

    public bool Update()
    {
        if (_done) return true;

        
        bool anyAlive = false;
        foreach (Player pl in Main.player)
        {
            if (pl.active && !pl.dead) { anyAlive = true; break; }
        }
        if (!anyAlive)
        {
            CleanUp();
            _npc.life = 0;
            _npc.active = false;   
            if (Main.netMode == NetmodeID.Server)
                NetMessage.SendData(MessageID.SyncNPC, -1, -1, null, _npc.whoAmI);
            _done = true;
            return true;
        }

        _timer++;
        _npc.life = 1;
        _npc.dontTakeDamage = true;
        _npc.velocity = Vector2.Zero;

        if (_timer < ATTACK_DURATION)
        {
            if (Main.netMode != NetmodeID.MultiplayerClient)
            {
                
                bool enraged = _timer >= ATTACK_DURATION / 2;
                int ringInterval = enraged ? 60 : 90;
                int randInterval = enraged ? 14 : 20;
                int randCount = enraged ? 3 : 2;

                
                if (_timer % ringInterval == 0)
                    for (int i = 0; i < 6; i++)
                        SpawnLaser(_rotateAngle + (MathHelper.TwoPi / 6f * i));

                
                if (_timer % 240 == 0 && Main.rand.NextBool())
                    _rotateDir *= -1f;
                _rotateAngle += _rotateDir * (0.02f + 0.025f * (float)Math.Sin(_timer * 0.03f));

                
                if (_timer % randInterval == 0)
                    for (int i = 0; i < randCount; i++)
                        SpawnLaserAt(_arenaCenter + new Vector2(Main.rand.NextFloat(-800, 800), Main.rand.NextFloat(-800, 800)),
                            Main.rand.NextFloat(MathHelper.TwoPi));

                
                if (_timer % 300 == 0)
                {
                    Vector2 burstPos = _arenaCenter + new Vector2(Main.rand.NextFloat(-600, 600), Main.rand.NextFloat(-600, 600));
                    float baseAngle = Main.rand.NextFloat(MathHelper.TwoPi);
                    for (int i = 0; i < 8; i++)
                        SpawnLaserAt(burstPos, baseAngle + (MathHelper.TwoPi / 8f * i));
                }
            }
        }
        else
        {
            _npc.ai[3] = 1;
            if (_timer == ATTACK_DURATION + 1) CleanUp();

            Main.SetCameraLerp(0.1f, 10);
            float shakePower = MathHelper.Lerp(5f, 25f, (float)(_timer - ATTACK_DURATION) / DEATH_TIMER);
            Main.screenPosition += new Vector2(Main.rand.NextFloat(-shakePower, shakePower), Main.rand.NextFloat(-shakePower, shakePower));

            for (int i = 0; i < 5; i++)
            {
                Dust d = Dust.NewDustDirect(_npc.position, _npc.width, _npc.height, Terraria.ID.DustID.WhiteTorch);
                d.velocity *= 5f;
                d.scale = 2f;
                d.noGravity = true;
            }

            if (_timer >= ATTACK_DURATION + DEATH_TIMER)
            {
                FinalExplosion();
                _done = true;
                return true;
            }
        }

        foreach (Player p in Main.player)
        {
            if (!p.active || p.dead) continue;
            if (Math.Abs(p.Center.X - _arenaCenter.X) > ARENA_HALF_SIZE || Math.Abs(p.Center.Y - _arenaCenter.Y) > ARENA_HALF_SIZE)
                p.KillMe(Terraria.DataStructures.PlayerDeathReason.ByCustomReason("No escape."), 9999, 0);
        }
        return false;
    }

    private void SpawnLaser(float angle)
    {
        int p = Projectile.NewProjectile(_npc.GetSource_FromAI(), _npc.Center, Vector2.Zero, ModContent.ProjectileType<EmpathyLaser3>(), 200, 0f, Main.myPlayer);
        if (p < 1000) { Main.projectile[p].localAI[0] = angle; Main.projectile[p].timeLeft = 125; }
    }

    private void SpawnLaserAt(Vector2 pos, float angle)
    {
        int p = Projectile.NewProjectile(_npc.GetSource_FromAI(), pos, Vector2.Zero, ModContent.ProjectileType<EmpathyLaser3>(), 200, 0f, Main.myPlayer);
        if (p < 1000) { Main.projectile[p].localAI[0] = angle; Main.projectile[p].timeLeft = 60; }
    }

    private void CleanUp()
    {
        for (int i = 0; i < Main.maxProjectiles; i++)
            if (Main.projectile[i].active && (Main.projectile[i].type == ModContent.ProjectileType<EmpathyBarrier>() || Main.projectile[i].type == ModContent.ProjectileType<EmpathyLaser3>()))
                Main.projectile[i].Kill();
    }

    private void FinalExplosion()
    {
        if (Main.netMode == NetmodeID.MultiplayerClient) return;
        for (int i = 0; i < 200; i++)
        {
            Vector2 speed = Main.rand.NextVector2Unit() * Main.rand.NextFloat(10f, 30f);
            Dust d = Dust.NewDustDirect(_npc.Center, 0, 0, Terraria.ID.DustID.WhiteTorch, speed.X, speed.Y, 100, Color.White, 4f);
            d.noGravity = true;
            d.velocity *= 1.5f;
        }
    }
}


