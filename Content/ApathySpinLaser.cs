
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;

namespace flameout.Content
{
    
    public class ApathySpinLaser : ModProjectile
    {
       
        private const float BEAM_LENGTH = 2400f;
        private const float START_SPEED = 0.01f;  
        private const float END_SPEED = 0.15f;    
        private const int DURATION = 600;          

        
        

        public override void SetDefaults()
        {
            Projectile.width = 20;
            Projectile.height = 20;
            Projectile.hostile = true;
            Projectile.friendly = false;
            Projectile.ignoreWater = true;
            Projectile.tileCollide = false;
            Projectile.penetrate = -1;
            Projectile.timeLeft = 700;
            Projectile.light = 1f;
        }

        public override void AI()
        {
            Projectile.velocity = Vector2.Zero;

            float timer = Projectile.ai[0];
            float progress = Math.Min(timer / DURATION, 1f);

            
            float angularSpeed = MathHelper.Lerp(START_SPEED, END_SPEED, progress);
            Projectile.ai[1] += angularSpeed;

            
            if (Main.rand.NextBool(2))
            {
                float t = Main.rand.NextFloat(BEAM_LENGTH);
                Vector2 beamDir = new Vector2(
                    (float)Math.Cos(Projectile.ai[1]),
                    (float)Math.Sin(Projectile.ai[1])
                );
                Vector2 dustPos = Projectile.Center + beamDir * t;
                Dust d = Dust.NewDustDirect(
                    dustPos, 4, 4,
                    DustID.WhiteTorch, 0f, 0f, 100, Color.White, 1.3f
                );
                d.noGravity = true;
            }
        }

        public override bool PreDraw(ref Color lightColor)
        {
            Texture2D tex = ModContent.Request<Texture2D>(Texture).Value;

            float angle = Projectile.ai[1];
            Vector2 beamDir = new Vector2((float)Math.Cos(angle), (float)Math.Sin(angle));

            Vector2 origin = new Vector2(0f, tex.Height / 2f);
            Vector2 scale = new Vector2(BEAM_LENGTH / tex.Width, 1f);

            Main.EntitySpriteDraw(
                tex,
                Projectile.Center - Main.screenPosition,
                null,
                Color.White * 0.95f,
                angle,
                origin,
                scale,
                SpriteEffects.None,
                0
            );
            return false;
        }

               public override bool? Colliding(Rectangle projHitbox, Rectangle targetHitbox)
        {
            float angle = Projectile.ai[1];
            Vector2 beamDir = new Vector2((float)Math.Cos(angle), (float)Math.Sin(angle));

            float _ = 0f;
            return Collision.CheckAABBvLineCollision(
                targetHitbox.TopLeft(),
                targetHitbox.Size(),
                Projectile.Center,
                Projectile.Center + beamDir * BEAM_LENGTH,
                16f,
                ref _
            );
        }
    }
}