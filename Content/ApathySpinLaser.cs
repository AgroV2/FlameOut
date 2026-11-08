

using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.GameContent;

namespace flameout.Content
{
    public class ApathySpinLaser : ModProjectile
    {
        private const float BEAM_LENGTH = 2400f;
        private const float START_SPEED = 0.01f;
        private const float END_SPEED = 0.07f;

        
        
        
        private const int LIFETIME = 600;

        private const int CHARGE_TIME = 45;
        private const int FADE_IN = 20;
        private const int FADE_OUT = 40;

        private const float CORE_WIDTH = 0.55f;
        private const float MID_WIDTH = 1.25f;
        private const float GLOW_WIDTH = 2.6f;

        private static readonly Color ColorCore = new Color(255, 255, 255);
        private static readonly Color ColorMid = new Color(180, 220, 255);
        private static readonly Color ColorGlow = new Color(70, 130, 255);

        private float Timer { get => Projectile.ai[0]; set => Projectile.ai[0] = value; }
        private float Angle { get => Projectile.ai[1]; set => Projectile.ai[1] = value; }
        private float Intensity { get => Projectile.localAI[0]; set => Projectile.localAI[0] = value; }
        private float BeamLength { get => Projectile.localAI[1]; set => Projectile.localAI[1] = value; }

        public override void SetStaticDefaults()
        {
            ProjectileID.Sets.DrawScreenCheckFluff[Type] = (int)BEAM_LENGTH;
        }

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
            Projectile.light = 0f;
        }

        private Vector2 BeamDir => Angle.ToRotationVector2();

        public override void AI()
        {
            Projectile.velocity = Vector2.Zero;
            Timer++;

            
            float progress = MathHelper.Clamp((Timer - CHARGE_TIME) / (LIFETIME - CHARGE_TIME), 0f, 1f);
            float eased = progress * progress * (3f - 2f * progress);
            float angularSpeed = MathHelper.Lerp(START_SPEED, END_SPEED, eased);

            if (Timer > CHARGE_TIME)
                Angle += angularSpeed;

            float target;
            if (Timer < CHARGE_TIME)
                target = 0.18f + 0.10f * (float)Math.Sin(Timer * 0.35f);
            else if (Timer < CHARGE_TIME + FADE_IN)
                target = MathHelper.Lerp(0.25f, 1f, (Timer - CHARGE_TIME) / FADE_IN);
            else
                target = 1f;

            
            if (Timer > LIFETIME - FADE_OUT)
                target *= MathHelper.Clamp((LIFETIME - Timer) / FADE_OUT, 0f, 1f);

            Intensity = MathHelper.Lerp(Intensity, target, 0.25f);
            Intensity *= 1f + 0.05f * (float)Math.Sin(Timer * 0.25f);

            float lenTarget = Timer < CHARGE_TIME ? BEAM_LENGTH * 0.9f : BEAM_LENGTH;
            BeamLength = MathHelper.Lerp(BeamLength, lenTarget, 0.35f);

            SpawnVisuals();
            CastLight();
        }

        private void SpawnVisuals()
        {
            if (Main.dedServ || Intensity < 0.2f)
                return;

            Vector2 dir = BeamDir;
            Vector2 perp = new Vector2(-dir.Y, dir.X);

            int count = (int)(6 * Intensity);
            for (int i = 0; i < count; i++)
            {
                float t = Main.rand.NextFloat(BeamLength);
                Vector2 pos = Projectile.Center + dir * t + perp * Main.rand.NextFloat(-6f, 6f);

                Dust d = Dust.NewDustPerfect(
                    pos,
                    Main.rand.NextBool(3) ? DustID.PortalBoltTrail : DustID.WhiteTorch,
                    perp * Main.rand.NextFloat(-2.5f, 2.5f) + dir * Main.rand.NextFloat(-1f, 1f),
                    100,
                    Color.Lerp(ColorMid, ColorCore, Main.rand.NextFloat()),
                    Main.rand.NextFloat(0.9f, 1.6f) * Intensity
                );
                d.noGravity = true;
                d.fadeIn = 1.1f;
            }

            if (Main.rand.NextBool(2))
            {
                float t = Main.rand.NextFloat(BeamLength * 0.2f, BeamLength);
                float lag = -0.06f * (t / BeamLength);
                Vector2 lagDir = (Angle + lag).ToRotationVector2();
                Dust d = Dust.NewDustPerfect(Projectile.Center + lagDir * t, DustID.BlueTorch,
                    Vector2.Zero, 150, ColorGlow, 1.2f * Intensity);
                d.noGravity = true;
            }

            for (int i = 0; i < 2; i++)
            {
                Vector2 offset = Main.rand.NextVector2CircularEdge(34f, 34f);
                Dust d = Dust.NewDustPerfect(Projectile.Center + offset, DustID.WhiteTorch,
                    -offset * 0.06f, 80, ColorCore, 1.4f * Intensity);
                d.noGravity = true;
            }
        }

        private void CastLight()
        {
            Vector3 light = ColorGlow.ToVector3() * 1.1f * Intensity;
            Vector2 dir = BeamDir;
            for (float t = 0f; t < BeamLength; t += 120f)
                Lighting.AddLight(Projectile.Center + dir * t, light);

            Lighting.AddLight(Projectile.Center, ColorCore.ToVector3() * 1.6f * Intensity);
        }

        public override bool PreDraw(ref Color lightColor)
        {
            if (Intensity <= 0.01f)
                return false;

            Texture2D tex = ModContent.Request<Texture2D>(Texture).Value;
            Vector2 drawPos = Projectile.Center - Main.screenPosition;
            Vector2 origin = new Vector2(0f, tex.Height / 2f);
            float lengthScale = BeamLength / tex.Width;

            Main.spriteBatch.End();
            Main.spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.Additive,
                Main.DefaultSamplerState, DepthStencilState.None,
                Main.Rasterizer, null, Main.GameViewMatrix.TransformationMatrix);

            DrawLayer(tex, drawPos, origin, lengthScale, GLOW_WIDTH * Intensity, ColorGlow * 0.35f * Intensity);
            DrawLayer(tex, drawPos, origin, lengthScale, GLOW_WIDTH * 0.7f * Intensity, ColorGlow * 0.5f * Intensity);

            DrawLayer(tex, drawPos, origin, lengthScale, MID_WIDTH * Intensity, ColorMid * 0.85f * Intensity);

            DrawLayer(tex, drawPos, origin, lengthScale, CORE_WIDTH * Intensity, ColorCore * Intensity);

            Texture2D flare = TextureAssets.Extra[ExtrasID.SharpTears].Value;
            float flarePulse = 1f + 0.12f * (float)Math.Sin(Timer * 0.4f);
            Main.EntitySpriteDraw(flare, drawPos, null, ColorCore * 0.9f * Intensity,
                Angle, flare.Size() * 0.5f, new Vector2(0.9f, 0.5f) * flarePulse * Intensity,
                SpriteEffects.None, 0);
            Main.EntitySpriteDraw(flare, drawPos, null, ColorGlow * 0.7f * Intensity,
                Angle + MathHelper.PiOver2, flare.Size() * 0.5f, new Vector2(0.6f, 0.35f) * flarePulse * Intensity,
                SpriteEffects.None, 0);

            Main.spriteBatch.End();
            Main.spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend,
                Main.DefaultSamplerState, DepthStencilState.None,
                Main.Rasterizer, null, Main.GameViewMatrix.TransformationMatrix);

            return false;
        }

        private void DrawLayer(Texture2D tex, Vector2 drawPos, Vector2 origin,
            float lengthScale, float widthScale, Color color)
        {
            Main.EntitySpriteDraw(tex, drawPos, null, color, Angle, origin,
                new Vector2(lengthScale, widthScale), SpriteEffects.None, 0);
        }

        public override bool? Colliding(Rectangle projHitbox, Rectangle targetHitbox)
        {
            if (Timer < CHARGE_TIME || Intensity < 0.6f)
                return false;

            float collisionWidth = 18f * MathHelper.Clamp(Intensity, 0f, 1f);
            float _ = 0f;
            return Collision.CheckAABBvLineCollision(
                targetHitbox.TopLeft(),
                targetHitbox.Size(),
                Projectile.Center,
                Projectile.Center + BeamDir * BeamLength,
                collisionWidth,
                ref _);
        }
    }
}

