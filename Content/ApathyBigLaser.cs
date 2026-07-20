using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace flameout.Content
{
    public class ApathyBigLaser : ModProjectile
    {
        private const int LIFETIME = 180;
        private const float LASER_LENGTH = 3000f;

        public override void SetDefaults()
        {
            Projectile.width = 2;
            Projectile.height = 2;
            Projectile.hostile = true;
            Projectile.friendly = false;
            Projectile.ignoreWater = true;
            Projectile.tileCollide = false;
            Projectile.penetrate = -1;
            Projectile.timeLeft = LIFETIME;
            Projectile.light = 1f;
        }

        public override void SetStaticDefaults()
        {
            ProjectileID.Sets.DrawScreenCheckFluff[Type] = 100000;
        }

        public override void AI()
        {
            
            
            if (Main.rand.NextBool(3))
            {
                Vector2 randPoint = Projectile.Center + Projectile.velocity.SafeNormalize(Vector2.Zero) 
                    * Main.rand.NextFloat(0f, LASER_LENGTH);
                Dust d = Dust.NewDustDirect(randPoint, 4, 4, DustID.PurpleTorch, 0f, 0f, 100, Color.White, 1.8f);
                d.noGravity = true;
                d.velocity *= 0.3f;
            }
        }

        public override bool? Colliding(Rectangle projHitbox, Rectangle targetHitbox)
        {
            float _ = 0f;
            Vector2 dir = Projectile.velocity.SafeNormalize(Vector2.Zero);
            return Collision.CheckAABBvLineCollision(
                targetHitbox.TopLeft(), targetHitbox.Size(),
                Projectile.Center,
                Projectile.Center + dir * LASER_LENGTH,
                22f, ref _);
        }

        public override bool PreDraw(ref Color lightColor)
        {
            Texture2D pixel = Terraria.GameContent.TextureAssets.MagicPixel.Value;

            Vector2 dir = Projectile.velocity.SafeNormalize(Vector2.Zero);
            Vector2 origin = Projectile.Center - Main.screenPosition;

            int elapsed = LIFETIME - Projectile.timeLeft;
            float fadeIn  = MathHelper.Clamp(elapsed / 8f, 0f, 1f);
            float fadeOut = MathHelper.Clamp(Projectile.timeLeft / 20f, 0f, 1f);
            float alpha   = fadeIn * fadeOut;

            float pulse = (float)System.Math.Sin(elapsed * 0.15f) * 0.5f + 0.5f;

            
            DrawBeam(pixel, origin, dir, LASER_LENGTH,
                new Color(120, 40, 220) * 0.25f * alpha, 90f);

            
            DrawBeam(pixel, origin, dir, LASER_LENGTH,
                Color.Lerp(new Color(180, 80, 255), new Color(220, 160, 255), pulse) * 0.45f * alpha, 45f);

            
            DrawBeam(pixel, origin, dir, LASER_LENGTH,
                new Color(240, 200, 255) * 0.8f * alpha, 18f);

            
            DrawBeam(pixel, origin, dir, LASER_LENGTH,
                Color.White * alpha, 6f);

            
            float flareSize = (35f + pulse * 15f) * alpha;
            Main.spriteBatch.Draw(pixel, origin,
                new Rectangle(0, 0, 1, 1),
                Color.White * alpha,
                0f, new Vector2(0.5f),
                flareSize,
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
}