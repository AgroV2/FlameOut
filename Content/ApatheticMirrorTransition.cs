using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using SubworldLibrary;
using Terraria;
using Terraria.Audio;
using Terraria.Graphics.CameraModifiers;
using Terraria.ID;
using Terraria.ModLoader;

namespace flameout.Content
{
    public class ApatheticMirrorTransition : ModProjectile
    {
        private const int TearTime = 30;
        private const int TransitionTime = 90;
        private const int EndTime = 135;
        private const float BaseScale = 0.8f;

        private static readonly Color AuraColor = new Color(105, 40, 190);
        private static readonly Color RimColor = new Color(190, 95, 255);
        private static readonly Color RimColor2 = new Color(140, 60, 255);
        private static readonly Color FlashColor = new Color(235, 210, 255);

        public override string Texture => "Terraria/Images/Projectile_0";

        private float Timer => Projectile.ai[0];

        internal static bool IsActiveFor(Player player)
        {
            int type = ModContent.ProjectileType<ApatheticMirrorTransition>();
            for (int i = 0; i < Main.maxProjectiles; i++)
            {
                Projectile projectile = Main.projectile[i];
                if (projectile.active && projectile.type == type && projectile.owner == player.whoAmI)
                    return true;
            }
            return false;
        }

        public override void SetDefaults()
        {
            Projectile.width = 120;
            Projectile.height = 120;
            Projectile.friendly = false;
            Projectile.hostile = false;
            Projectile.tileCollide = false;
            Projectile.ignoreWater = true;
            Projectile.penetrate = -1;
            Projectile.aiStyle = -1;
            Projectile.timeLeft = EndTime + 10;
            Projectile.netImportant = true;
        }

        public override bool? CanDamage() => false;

        public override void AI()
        {
            Player player = Main.player[Projectile.owner];
            if (!player.active || player.dead)
            {
                Projectile.Kill();
                return;
            }

            Projectile.Center = player.Center;
            Projectile.velocity = Vector2.Zero;
            Projectile.ai[0]++;
            float t = Timer;

            float spinT = MathHelper.Clamp(t / TransitionTime, 0f, 1f);
            Projectile.localAI[0] += MathHelper.Lerp(0.025f, 0.22f, spinT);

            GetShape(t, out float sx, out float sy, out float intensity);
            float rimRadius = 100f * (sx + sy) * 0.5f;

            if (!Main.dedServ)
            {
                if (t == 1f)
                    SoundEngine.PlaySound(SoundID.DD2_EtherianPortalOpen, Projectile.Center);

                EmitDust(t, rimRadius);
                ShakeScreen(t);
                Lighting.AddLight(Projectile.Center, 0.55f * intensity, 0.18f * intensity, 0.95f * intensity);
            }

            if (t == TransitionTime && Projectile.owner == Main.myPlayer)
            {
                SoundEngine.PlaySound(SoundID.DD2_EtherianPortalSpawnEnemy, Projectile.Center);
                Main.instance.CameraModifiers.Add(new PunchCameraModifier(
                    Projectile.Center, Main.rand.NextVector2Unit(), 18f, 9f, 35, 2600f, "ApatheticMirror_Burst"));

                for (int i = 0; i < 90; i++)
                {
                    Dust dust = Dust.NewDustPerfect(Projectile.Center,
                        Main.rand.NextBool(3) ? DustID.PurpleTorch : DustID.Shadowflame,
                        Main.rand.NextVector2Circular(11f, 11f), 0, default, Main.rand.NextFloat(1.4f, 2.5f));
                    dust.noGravity = true;
                    dust.fadeIn = 1.1f;
                }

                if (SubworldSystem.IsActive<ApatheticMirrorSubworld>())
                {
                    SubworldSystem.Exit();
                }
                else if (!SubworldSystem.AnyActive())
                {
                    ApatheticMirrorSubworld.Capture(player);
                    SubworldSystem.Enter<ApatheticMirrorSubworld>();
                }
            }

            if (t >= EndTime)
                Projectile.Kill();
        }

        private void EmitDust(float t, float rimRadius)
        {
            Vector2 center = Projectile.Center;
            if (t < TearTime)
            {
                float height = MathHelper.Lerp(16f, 95f, t / TearTime);
                for (int i = 0; i < 2; i++)
                {
                    Dust dust = Dust.NewDustPerfect(center + new Vector2(Main.rand.NextFloat(-4f, 4f), Main.rand.NextFloat(-height, height)),
                        DustID.Shadowflame, Main.rand.NextVector2Circular(1.5f, 1.5f), 0, default, Main.rand.NextFloat(1f, 1.6f));
                    dust.noGravity = true;
                }
                return;
            }

            bool collapsing = t > TransitionTime;
            if (!collapsing)
            {
                for (int i = 0; i < 3; i++)
                {
                    float angle = Main.rand.NextFloat(MathHelper.TwoPi);
                    Vector2 position = center + angle.ToRotationVector2() * (rimRadius + Main.rand.NextFloat(35f, 90f));
                    Vector2 inward = (center - position).SafeNormalize(Vector2.Zero);
                    Vector2 velocity = inward * Main.rand.NextFloat(2.5f, 5f) + inward.RotatedBy(MathHelper.PiOver2) * Main.rand.NextFloat(2f, 6f);
                    Dust dust = Dust.NewDustPerfect(position, DustID.Shadowflame, velocity, 0, default, Main.rand.NextFloat(1.1f, 1.9f));
                    dust.noGravity = true;
                }
            }

            for (int i = 0; i < (collapsing ? 2 : 3); i++)
            {
                float angle = Main.rand.NextFloat(MathHelper.TwoPi);
                Vector2 position = center + angle.ToRotationVector2() * rimRadius * Main.rand.NextFloat(0.92f, 1.05f);
                Vector2 velocity = (angle + MathHelper.PiOver2).ToRotationVector2() * Main.rand.NextFloat(3f, 7f);
                if (collapsing)
                    velocity += (center - position).SafeNormalize(Vector2.Zero) * 4f;
                Dust dust = Dust.NewDustPerfect(position, Main.rand.NextBool(4) ? DustID.PurpleTorch : DustID.Shadowflame,
                    velocity, 0, default, Main.rand.NextFloat(1.2f, 2f));
                dust.noGravity = true;
            }
        }

        private void ShakeScreen(float t)
        {
            if (t < TransitionTime && t % 8f == 0f)
            {
                float strength = MathHelper.Lerp(1.5f, 7f, t / TransitionTime);
                Main.instance.CameraModifiers.Add(new PunchCameraModifier(
                    Projectile.Center, Main.rand.NextVector2Unit(), strength, 7f, 12, 2200f, "ApatheticMirror_Rumble"));
            }
        }

        private static void GetShape(float t, out float sx, out float sy, out float intensity)
        {
            if (t < TearTime)
            {
                float p = t / TearTime;
                sy = MathHelper.Lerp(0.12f, 1f, EaseOutCubic(p));
                sx = MathHelper.Lerp(0.02f, 0.1f, p);
                intensity = 0.5f + 0.3f * p;
            }
            else if (t < TransitionTime)
            {
                float p = (t - TearTime) / (TransitionTime - TearTime);
                sx = MathHelper.Lerp(0.1f, 1f, EaseOutBack(p));
                sy = 1f;
                intensity = MathHelper.Lerp(0.8f, 1f, p);
            }
            else
            {
                float p = MathHelper.Clamp((t - TransitionTime) / (EndTime - TransitionTime), 0f, 1f);
                float scale = MathHelper.Clamp(1f - EaseInBack(p), 0f, 1.15f);
                sx = scale;
                sy = scale;
                intensity = MathHelper.Clamp(scale, 0f, 1f);
            }

            sx *= BaseScale;
            sy *= BaseScale;
        }

        private static float EaseOutCubic(float p) => 1f - (1f - p) * (1f - p) * (1f - p);

        private static float EaseOutBack(float p)
        {
            const float c1 = 1.70158f;
            const float c3 = c1 + 1f;
            float q = p - 1f;
            return 1f + c3 * q * q * q + c1 * q * q;
        }

        private static float EaseInBack(float p)
        {
            const float c1 = 1.70158f;
            const float c3 = c1 + 1f;
            return c3 * p * p * p - c1 * p * p;
        }

        public override bool PreDraw(ref Color lightColor)
        {
            ApathyPortalAssets.EnsureLoaded();
            Texture2D glow = ApathyPortalAssets.Glow;
            Texture2D ring = ApathyPortalAssets.Ring;
            Texture2D core = ApathyPortalAssets.Core;
            if (glow == null || ring == null || core == null)
                return false;

            float t = Timer;
            GetShape(t, out float sx, out float sy, out float intensity);
            Vector2 position = Projectile.Center - Main.screenPosition;
            float rotation = Projectile.localAI[0];
            float pulse = 1f + 0.06f * MathF.Sin(t * 0.09f);
            float wobble = 0.06f * MathF.Sin(t * 0.11f);
            Vector2 rimScale = new Vector2(sx * (1f + wobble), sy * (1f - wobble));
            float flash = MathHelper.Clamp(1f - MathF.Abs(t - TransitionTime) / 16f, 0f, 1f);

            SpriteBatch spriteBatch = Main.spriteBatch;
            spriteBatch.End();
            spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.Additive, Main.DefaultSamplerState,
                DepthStencilState.None, Main.Rasterizer, null, Main.GameViewMatrix.TransformationMatrix);
            spriteBatch.Draw(glow, position, null, AuraColor * (0.85f * intensity), rotation * 0.4f, glow.Size() / 2f,
                new Vector2(sx, sy) * 2.4f * pulse, SpriteEffects.None, 0f);
            spriteBatch.Draw(glow, position, null, RimColor2 * (0.5f * intensity), -rotation * 0.7f, glow.Size() / 2f,
                new Vector2(sx, sy) * 1.4f, SpriteEffects.None, 0f);

            spriteBatch.End();
            spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, Main.DefaultSamplerState,
                DepthStencilState.None, Main.Rasterizer, null, Main.GameViewMatrix.TransformationMatrix);
            spriteBatch.Draw(core, position, null, Color.Black * MathHelper.Clamp(intensity * 1.1f, 0f, 1f), rotation,
                core.Size() / 2f, new Vector2(sx, sy) * 1.02f, SpriteEffects.None, 0f);

            spriteBatch.End();
            spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.Additive, Main.DefaultSamplerState,
                DepthStencilState.None, Main.Rasterizer, null, Main.GameViewMatrix.TransformationMatrix);
            spriteBatch.Draw(ring, position, null, RimColor * intensity, rotation, ring.Size() / 2f, rimScale, SpriteEffects.None, 0f);
            spriteBatch.Draw(ring, position, null, RimColor2 * (0.75f * intensity), -rotation * 1.7f, ring.Size() / 2f,
                rimScale * 0.82f, SpriteEffects.None, 0f);
            spriteBatch.Draw(glow, position, null, FlashColor * (0.35f * intensity * pulse), 0f, glow.Size() / 2f,
                new Vector2(sx, sy) * 0.4f, SpriteEffects.None, 0f);
            if (flash > 0f)
            {
                spriteBatch.Draw(glow, position, null, FlashColor * flash, 0f, glow.Size() / 2f,
                    new Vector2(4f * flash + 0.5f, 0.4f), SpriteEffects.None, 0f);
                spriteBatch.Draw(glow, position, null, FlashColor * flash, MathHelper.PiOver2, glow.Size() / 2f,
                    new Vector2(3f * flash + 0.5f, 0.32f), SpriteEffects.None, 0f);
            }

            spriteBatch.End();
            spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, Main.DefaultSamplerState,
                DepthStencilState.None, Main.Rasterizer, null, Main.GameViewMatrix.TransformationMatrix);
            return false;
        }
    }
}
