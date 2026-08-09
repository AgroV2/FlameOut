

using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.ModLoader;

namespace flameout.Content
{
    
    
    public class ApathyTrackingLaser : ModProjectile
    {
        public override string Texture => "Terraria/Images/Projectile_0";

        private const float BEAM_LENGTH = 2600f;
        private const int LIFETIME = 420;
        private const int TELEGRAPH = 50;
        private const int FADE_OUT = 30;

        
        
        private const float TURN_START = 0.09f;
        private const float TURN_END = 0.021f;

        private float Angle { get => Projectile.ai[0]; set => Projectile.ai[0] = value; }
        private float Age { get => Projectile.localAI[0]; set => Projectile.localAI[0] = value; }
        private float Aimed { get => Projectile.localAI[1]; set => Projectile.localAI[1] = value; }

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
            Projectile.timeLeft = LIFETIME + 60;
            Projectile.light = 0.7f;
        }

        private Vector2 Dir() => Angle.ToRotationVector2();

        private float Alpha()
        {
            float fadeIn = MathHelper.Clamp((Age - TELEGRAPH) / 10f, 0f, 1f);
            float fadeOut = MathHelper.Clamp((LIFETIME - Age) / FADE_OUT, 0f, 1f);
            return fadeIn * fadeOut;
        }

        public override void AI()
        {
            Projectile.velocity = Vector2.Zero;
            Age++;

            int boss = NPC.FindFirstNPC(ModContent.NPCType<ApathyBoss>());
            if (boss < 0)
            {
                Projectile.Kill();
                return;
            }

            NPC owner = Main.npc[boss];
            Projectile.Center = owner.Center;

            Player target = Main.player[owner.target];
            if (target != null && target.active && !target.dead)
            {
                float wanted = Projectile.Center.DirectionTo(target.Center).ToRotation();

                if (Aimed == 0f)
                {
                    
                    Angle = wanted;
                    Aimed = 1f;
                }
                else
                {
                    float t = MathHelper.Clamp(Age / (float)LIFETIME, 0f, 1f);
                    float turn = MathHelper.Lerp(TURN_START, TURN_END, t);

                    
                    float delta = MathHelper.WrapAngle(wanted - Angle);
                    Angle = MathHelper.WrapAngle(Angle + delta * turn);
                }
            }

            Vector2 dir = Dir();
            Lighting.AddLight(Projectile.Center, 0.9f, 0.25f, 1.4f);

            if (Age < TELEGRAPH)
            {
                if (Main.rand.NextBool(2))
                {
                    Dust w = Dust.NewDustPerfect(
                        Projectile.Center + dir * Main.rand.NextFloat(BEAM_LENGTH),
                        DustID.PurpleTorch, Vector2.Zero, 170, Color.White, 0.8f);
                    w.noGravity = true;
                }
                return;
            }

            for (int i = 0; i < 2; i++)
            {
                if (!Main.rand.NextBool(2))
                    continue;

                Vector2 perp = new Vector2(-dir.Y, dir.X);
                Vector2 pos = Projectile.Center + dir * Main.rand.NextFloat(BEAM_LENGTH)
                              + perp * Main.rand.NextFloat(-7f, 7f);
                Dust d = Dust.NewDustPerfect(pos, DustID.PurpleTorch,
                    perp * Main.rand.NextFloat(-1.6f, 1.6f), 90, default, 1.5f);
                d.noGravity = true;
            }

            
            if (Main.rand.NextBool(3))
            {
                Vector2 off = Main.rand.NextVector2CircularEdge(30f, 30f);
                Dust d = Dust.NewDustPerfect(Projectile.Center + off, DustID.PurpleTorch,
                    -off * 0.07f, 60, default, 1.3f);
                d.noGravity = true;
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
                Projectile.Center + Dir() * BEAM_LENGTH,
                20f, ref _);
        }

        public override bool PreDraw(ref Color lightColor)
        {
            Texture2D px = TextureAssets.MagicPixel.Value;
            Vector2 dir = Dir();
            Vector2 origin = Projectile.Center - Main.screenPosition;
            float pulse = (float)Math.Sin(Age * 0.18f) * 0.5f + 0.5f;

            SpriteBatch sb = Main.spriteBatch;
            sb.End();
            sb.Begin(SpriteSortMode.Deferred, BlendState.Additive, SamplerState.LinearClamp,
                DepthStencilState.None, RasterizerState.CullNone, null, Main.GameViewMatrix.TransformationMatrix);

            if (Age < TELEGRAPH)
            {
                float warn = 0.3f + 0.4f * pulse;
                DrawBeam(px, origin, dir, new Color(150, 50, 230) * 0.22f * warn, 12f);
                DrawBeam(px, origin, dir, new Color(240, 210, 255) * warn, 2f);
            }
            else
            {
                float a = Alpha();
                if (a > 0.01f)
                {
                    DrawBeam(px, origin, dir, new Color(90, 20, 180) * 0.28f * a, 76f);
                    DrawBeam(px, origin, dir,
                        Color.Lerp(new Color(160, 60, 245), new Color(210, 140, 255), pulse) * 0.5f * a, 38f);
                    DrawBeam(px, origin, dir, new Color(230, 190, 255) * 0.85f * a, 15f);
                    DrawBeam(px, origin, dir, Color.White * a, 5f);

                    float flare = (30f + pulse * 14f) * a;
                    sb.Draw(px, origin, new Rectangle(0, 0, 1, 1), Color.White * a,
                        0f, new Vector2(0.5f), flare, SpriteEffects.None, 0f);
                }
            }

            sb.End();
            sb.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.LinearClamp,
                DepthStencilState.None, RasterizerState.CullNone, null, Main.GameViewMatrix.TransformationMatrix);

            return false;
        }

        private void DrawBeam(Texture2D px, Vector2 origin, Vector2 dir, Color color, float width)
        {
            Main.spriteBatch.Draw(px, origin + dir * (BEAM_LENGTH / 2f),
                new Rectangle(0, 0, 1, 1), color, dir.ToRotation(),
                new Vector2(0.5f, 0.5f), new Vector2(BEAM_LENGTH, width),
                SpriteEffects.None, 0f);
        }
    }
}

