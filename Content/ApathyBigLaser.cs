

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

        private const int TELEGRAPH = 35;
        private const int FADE_OUT = 20;

        private float DirX { get => Projectile.ai[0]; set => Projectile.ai[0] = value; }
        private float DirY { get => Projectile.ai[1]; set => Projectile.ai[1] = value; }
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
            Projectile.timeLeft = LIFETIME;
            Projectile.light = 1f;
        }

        public override void OnSpawn(Terraria.DataStructures.IEntitySource source)
        {
            Vector2 dir = Projectile.velocity.SafeNormalize(Vector2.UnitY);
            DirX = dir.X;
            DirY = dir.Y;
            Projectile.velocity = Vector2.Zero;
            Projectile.netUpdate = true;
        }

        private Vector2 Dir()
        {
            Vector2 d = new Vector2(DirX, DirY);
            return d == Vector2.Zero ? Vector2.UnitY : d.SafeNormalize(Vector2.UnitY);
        }

        private float Alpha()
        {
            float fadeIn = MathHelper.Clamp((Age - TELEGRAPH) / 8f, 0f, 1f);
            float fadeOut = MathHelper.Clamp((LIFETIME - Age) / FADE_OUT, 0f, 1f);
            return MathHelper.Max(fadeIn, 0f) * fadeOut;
        }

        public override void AI()
        {
            Projectile.velocity = Vector2.Zero;
            Age++;

            int boss = NPC.FindFirstNPC(ModContent.NPCType<ApathyBoss>());
            if (boss >= 0)
                Projectile.Center = Main.npc[boss].Center;
            else
            {
                Projectile.Kill();
                return;
            }

            Vector2 dir = Dir();

            if (Age < TELEGRAPH)
            {
                if (Main.rand.NextBool(2))
                {
                    Vector2 pos = Projectile.Center + dir * Main.rand.NextFloat(0f, LASER_LENGTH);
                    Dust w = Dust.NewDustPerfect(pos, DustID.PurpleTorch, Vector2.Zero, 150, Color.White, 0.9f);
                    w.noGravity = true;
                }
                return;
            }

            if (Main.rand.NextBool(3))
            {
                Vector2 randPoint = Projectile.Center + dir * Main.rand.NextFloat(0f, LASER_LENGTH);
                Dust d = Dust.NewDustDirect(randPoint, 4, 4, DustID.PurpleTorch, 0f, 0f, 100, Color.White, 1.8f);
                d.noGravity = true;
                d.velocity *= 0.3f;
            }
        }

        public override bool? Colliding(Rectangle projHitbox, Rectangle targetHitbox)
        {
            if (Age < TELEGRAPH)
                return false;

            float _ = 0f;
            return Collision.CheckAABBvLineCollision(
                targetHitbox.TopLeft(), targetHitbox.Size(),
                Projectile.Center,
                Projectile.Center + Dir() * LASER_LENGTH,
                22f, ref _);
        }

        public override bool PreDraw(ref Color lightColor)
        {
            Texture2D pixel = Terraria.GameContent.TextureAssets.MagicPixel.Value;

            Vector2 dir = Dir();
            Vector2 origin = Projectile.Center - Main.screenPosition;
            float pulse = (float)System.Math.Sin(Age * 0.15f) * 0.5f + 0.5f;

            SpriteBatch sb = Main.spriteBatch;
            sb.End();
            sb.Begin(SpriteSortMode.Deferred, BlendState.Additive, SamplerState.LinearClamp,
                DepthStencilState.None, RasterizerState.CullNone, null, Main.GameViewMatrix.TransformationMatrix);

            if (Age < TELEGRAPH)
            {
                float warn = 0.35f + 0.35f * pulse;
                DrawBeam(pixel, origin, dir, LASER_LENGTH, new Color(180, 80, 255) * 0.25f * warn, 10f);
                DrawBeam(pixel, origin, dir, LASER_LENGTH, new Color(255, 220, 255) * warn, 2f);
            }
            else
            {
                float alpha = Alpha();
                if (alpha > 0.01f)
                {
                    DrawBeam(pixel, origin, dir, LASER_LENGTH,
                        new Color(120, 40, 220) * 0.25f * alpha, 90f);

                    DrawBeam(pixel, origin, dir, LASER_LENGTH,
                        Color.Lerp(new Color(180, 80, 255), new Color(220, 160, 255), pulse) * 0.45f * alpha, 45f);

                    DrawBeam(pixel, origin, dir, LASER_LENGTH,
                        new Color(240, 200, 255) * 0.8f * alpha, 18f);

                    DrawBeam(pixel, origin, dir, LASER_LENGTH,
                        Color.White * alpha, 6f);

                    float flareSize = (35f + pulse * 15f) * alpha;
                    sb.Draw(pixel, origin,
                        new Rectangle(0, 0, 1, 1),
                        Color.White * alpha,
                        0f, new Vector2(0.5f),
                        flareSize,
                        SpriteEffects.None, 0f);
                }
            }

            sb.End();
            sb.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.LinearClamp,
                DepthStencilState.None, RasterizerState.CullNone, null, Main.GameViewMatrix.TransformationMatrix);

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

