

using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace flameout.Content
{
    public class ApathyStaticBeam : ModProjectile
    {
        private const float BEAM_LENGTH = 2400f;

        
        
        
        private const int LIFETIME = 360;
        private const int FADE_IN = 10;
        private const int FADE_OUT = 25;

        
        private float Age { get => Projectile.localAI[0]; set => Projectile.localAI[0] = value; }

        public override void SetStaticDefaults()
        {
            ProjectileID.Sets.DrawScreenCheckFluff[Type] = 100000;
        }

        public override void SetDefaults()
        {
            Projectile.width = 2;
            Projectile.height = 2;
            Projectile.hostile = true;
            Projectile.friendly = false;
            Projectile.ignoreWater = true;
            Projectile.tileCollide = false;
            Projectile.penetrate = -1;
            Projectile.timeLeft = 600;
            Projectile.light = 0.8f;
        }

        public override void AI()
        {
            Projectile.velocity = Vector2.Zero;
            Age++;

            Vector2 beamDir = GetDir();

            if (Main.rand.NextBool(3))
            {
                Vector2 dustPos = Projectile.Center + beamDir * Main.rand.NextFloat(BEAM_LENGTH);
                Dust d = Dust.NewDustDirect(dustPos, 4, 4, DustID.PinkTorch, 0f, 0f, 100, Color.HotPink, 1.4f);
                d.noGravity = true;
                d.velocity *= 0.2f;
            }
        }

        private Vector2 GetDir()
        {
            Vector2 d = new Vector2(Projectile.ai[0], Projectile.ai[1]);
            return d == Vector2.Zero ? Vector2.UnitX : d.SafeNormalize(Vector2.Zero);
        }

        private float Alpha()
        {
            float fadeIn = MathHelper.Clamp(Age / FADE_IN, 0f, 1f);
            float fadeOut = MathHelper.Clamp((LIFETIME - Age) / FADE_OUT, 0f, 1f);
            return fadeIn * fadeOut;
        }

        public override bool? Colliding(Rectangle projHitbox, Rectangle targetHitbox)
        {
            
            if (Age < FADE_IN)
                return false;

            float _ = 0f;
            return Collision.CheckAABBvLineCollision(
                targetHitbox.TopLeft(), targetHitbox.Size(),
                Projectile.Center,
                Projectile.Center + GetDir() * BEAM_LENGTH,
                18f, ref _);
        }

        public override bool PreDraw(ref Color lightColor)
        {
            Texture2D pixel = Terraria.GameContent.TextureAssets.MagicPixel.Value;

            Vector2 dir = GetDir();
            Vector2 origin = Projectile.Center - Main.screenPosition;

            float alpha = Alpha();
            if (alpha <= 0.01f)
                return false;

            float pulse = (float)System.Math.Sin(Age * 0.12f) * 0.5f + 0.5f;

            DrawBeam(pixel, origin, dir, BEAM_LENGTH,
                new Color(255, 20, 120) * 0.2f * alpha, 80f);

            DrawBeam(pixel, origin, dir, BEAM_LENGTH,
                Color.Lerp(new Color(255, 80, 180), new Color(255, 160, 220), pulse) * 0.45f * alpha, 38f);

            DrawBeam(pixel, origin, dir, BEAM_LENGTH,
                new Color(255, 200, 230) * 0.85f * alpha, 16f);

            DrawBeam(pixel, origin, dir, BEAM_LENGTH,
                Color.White * alpha, 5f);

            float flareSize = (28f + pulse * 12f) * alpha;
            Main.spriteBatch.Draw(pixel, origin,
                new Rectangle(0, 0, 1, 1),
                Color.White * alpha * 0.9f,
                0f, new Vector2(0.5f),
                flareSize,
                SpriteEffects.None, 0f);

            return false;
        }

        private void DrawBeam(Texture2D pixel, Vector2 origin, Vector2 dir,
            float length, Color color, float width)
        {
            Main.spriteBatch.Draw(
                pixel,
                origin + dir * (length / 2f),
                new Rectangle(0, 0, 1, 1),
                color,
                dir.ToRotation(),
                new Vector2(0.5f, 0.5f),
                new Vector2(length, width),
                SpriteEffects.None, 0f);
        }
    }
}

