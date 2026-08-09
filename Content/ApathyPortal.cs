

using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.Audio;
using Terraria.Chat;
using Terraria.Graphics.CameraModifiers;
using Terraria.ID;
using Terraria.Localization;
using Terraria.ModLoader;

namespace flameout.Content
{
    public class ApathyPortal : ModProjectile
    {
        private const int TearTime = 40;
        private const int OpenTime = 120;
        private const int BossTime = 120;
        private const int EndTime  = 180;

        private const float BaseScale = 1.05f;

        private static readonly Color AuraColor  = new Color(105, 40, 190);
        private static readonly Color RimColor   = new Color(190, 95, 255);
        private static readonly Color RimColor2  = new Color(140, 60, 255);
        private static readonly Color FlashColor = new Color(235, 210, 255);

        public override string Texture => "Terraria/Images/Projectile_0";

        private float Timer => Projectile.ai[0];

        public override void SetDefaults()
        {
            Projectile.width = 150;
            Projectile.height = 150;
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
            Projectile.ai[0]++;
            float t = Timer;

            float spinT = MathHelper.Clamp(t / OpenTime, 0f, 1f);
            Projectile.localAI[0] += MathHelper.Lerp(0.02f, 0.2f, spinT);

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

            if (t == BossTime)
            {
                if (!Main.dedServ)
                {
                    SoundEngine.PlaySound(SoundID.DD2_EtherianPortalSpawnEnemy, Projectile.Center);
                    SoundEngine.PlaySound(SoundID.Roar, Projectile.Center);

                    Main.instance.CameraModifiers.Add(new PunchCameraModifier(
                        Projectile.Center, Main.rand.NextVector2Unit(), 22f, 9f, 45, 3200f, "ApathyPortal_Burst"));

                    for (int i = 0; i < 110; i++)
                    {
                        Vector2 vel = Main.rand.NextVector2Circular(13f, 13f);
                        Dust d = Dust.NewDustPerfect(Projectile.Center,
                            Main.rand.NextBool(3) ? DustID.PurpleTorch : DustID.Shadowflame,
                            vel, 0, default, Main.rand.NextFloat(1.6f, 2.8f));
                        d.noGravity = true;
                        d.fadeIn = 1.1f;
                    }
                }

                SpawnBoss();
            }

            if (t >= EndTime)
            {
                if (!Main.dedServ)
                {
                    for (int i = 0; i < 30; i++)
                    {
                        Dust d = Dust.NewDustPerfect(Projectile.Center, DustID.Shadowflame,
                            Main.rand.NextVector2Circular(5f, 5f), 0, default, Main.rand.NextFloat(1f, 1.8f));
                        d.noGravity = true;
                    }
                }
                Projectile.Kill();
            }
        }

        private void EmitDust(float t, float rimRadius)
        {
            Vector2 center = Projectile.Center;

            if (t < TearTime)
            {
                float h = MathHelper.Lerp(20f, 120f, t / TearTime);
                for (int i = 0; i < 2; i++)
                {
                    Vector2 pos = center + new Vector2(Main.rand.NextFloat(-4f, 4f), Main.rand.NextFloat(-h, h));
                    Dust d = Dust.NewDustPerfect(pos, DustID.Shadowflame,
                        Main.rand.NextVector2Circular(1.5f, 1.5f), 0, default, Main.rand.NextFloat(1f, 1.6f));
                    d.noGravity = true;
                }
                return;
            }

            bool collapsing = t > BossTime;

            if (!collapsing)
            {
                for (int i = 0; i < 3; i++)
                {
                    float ang = Main.rand.NextFloat(MathHelper.TwoPi);
                    Vector2 pos = center + ang.ToRotationVector2() * (rimRadius + Main.rand.NextFloat(40f, 110f));
                    Vector2 toCenter = (center - pos).SafeNormalize(Vector2.Zero);
                    Vector2 tangent = toCenter.RotatedBy(MathHelper.PiOver2);
                    Vector2 vel = toCenter * Main.rand.NextFloat(2.5f, 5f) + tangent * Main.rand.NextFloat(2f, 6f);

                    Dust d = Dust.NewDustPerfect(pos, DustID.Shadowflame, vel, 0, default, Main.rand.NextFloat(1.1f, 1.9f));
                    d.noGravity = true;
                }
            }

            int rimCount = collapsing ? 2 : 3;
            for (int i = 0; i < rimCount; i++)
            {
                float ang = Main.rand.NextFloat(MathHelper.TwoPi);
                Vector2 rim = center + ang.ToRotationVector2() * rimRadius * Main.rand.NextFloat(0.92f, 1.05f);
                Vector2 vel = (ang + MathHelper.PiOver2).ToRotationVector2() * Main.rand.NextFloat(3f, 7f);
                if (collapsing)
                    vel += (center - rim).SafeNormalize(Vector2.Zero) * 4f;

                Dust d = Dust.NewDustPerfect(rim,
                    Main.rand.NextBool(4) ? DustID.PurpleTorch : DustID.Shadowflame,
                    vel, 0, default, Main.rand.NextFloat(1.2f, 2f));
                d.noGravity = true;
            }

            if (Main.rand.NextBool(3))
            {
                Dust smoke = Dust.NewDustPerfect(
                    center + Main.rand.NextVector2Circular(rimRadius * 0.4f, rimRadius * 0.4f),
                    DustID.Smoke,
                    Main.rand.NextVector2Circular(1f, 1f) - Vector2.UnitY * 0.5f,
                    160, Color.Black, Main.rand.NextFloat(1.2f, 2f));
                smoke.noGravity = true;
            }
        }

        private void ShakeScreen(float t)
        {
            if (t < BossTime && t % 8f == 0f)
            {
                float strength = MathHelper.Lerp(1.5f, 8f, t / BossTime);
                Main.instance.CameraModifiers.Add(new PunchCameraModifier(
                    Projectile.Center, Main.rand.NextVector2Unit(), strength, 7f, 12, 2600f, "ApathyPortal_Rumble"));
            }

            if (t > BossTime && t % 12f == 0f)
            {
                float p = (t - BossTime) / (float)(EndTime - BossTime);
                Main.instance.CameraModifiers.Add(new PunchCameraModifier(
                    Projectile.Center, Main.rand.NextVector2Unit(), MathHelper.Lerp(5f, 1f, p), 7f, 10, 2600f, "ApathyPortal_Rumble"));
            }
        }

        private void SpawnBoss()
        {
            if (Main.netMode == NetmodeID.MultiplayerClient)
                return;

            int type = ModContent.NPCType<ApathyBoss>();
            if (NPC.AnyNPCs(type))
                return;

            int index = NPC.NewNPC(NPC.GetBossSpawnSource(Projectile.owner),
                (int)Projectile.Center.X, (int)Projectile.Center.Y, type);

            if (index == Main.maxNPCs)
                return;

            NPC boss = Main.npc[index];
            boss.Center = Projectile.Center;
            boss.velocity = new Vector2(0f, -3f);
            boss.netUpdate = true;

            if (Main.netMode == NetmodeID.Server)
            {
                NetMessage.SendData(MessageID.SyncNPC, number: index);
                ChatHelper.BroadcastChatMessage(
                    NetworkText.FromKey("Announcement.HasAwoken", boss.GetTypeNetName()),
                    new Color(175, 75, 255));
            }
            else
            {
                Main.NewText(Language.GetTextValue("Announcement.HasAwoken", boss.TypeName), 175, 75, 255);
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
            else if (t < OpenTime)
            {
                float p = (t - TearTime) / (float)(OpenTime - TearTime);
                float e = EaseOutBack(p);
                sx = MathHelper.Lerp(0.1f, 1f, e);
                sy = 1f;
                intensity = MathHelper.Lerp(0.8f, 1f, p);
            }
            else
            {
                float p = MathHelper.Clamp((t - BossTime) / (float)(EndTime - BossTime), 0f, 1f);
                float k = MathHelper.Clamp(1f - EaseInBack(p), 0f, 1.15f);
                sx = k;
                sy = k;
                intensity = MathHelper.Clamp(k, 0f, 1f);
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

            Vector2 pos = Projectile.Center - Main.screenPosition;
            float rot = Projectile.localAI[0];
            float pulse = 1f + 0.06f * MathF.Sin(t * 0.09f);
            float wobble = 0.06f * MathF.Sin(t * 0.11f);
            Vector2 rimScale = new Vector2(sx * (1f + wobble), sy * (1f - wobble));

            float flash = MathHelper.Clamp(1f - MathF.Abs(t - BossTime) / 18f, 0f, 1f);

            Vector2 glowOrigin = glow.Size() / 2f;
            Vector2 ringOrigin = ring.Size() / 2f;
            Vector2 coreOrigin = core.Size() / 2f;

            SpriteBatch sb = Main.spriteBatch;

            sb.End();
            sb.Begin(SpriteSortMode.Deferred, BlendState.Additive, Main.DefaultSamplerState,
                DepthStencilState.None, Main.Rasterizer, null, Main.GameViewMatrix.TransformationMatrix);

            sb.Draw(glow, pos, null, AuraColor * (0.85f * intensity), rot * 0.4f, glowOrigin,
                new Vector2(sx, sy) * 2.4f * pulse, SpriteEffects.None, 0f);
            sb.Draw(glow, pos, null, RimColor2 * (0.5f * intensity), -rot * 0.7f, glowOrigin,
                new Vector2(sx, sy) * 1.4f, SpriteEffects.None, 0f);

            sb.End();
            sb.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, Main.DefaultSamplerState,
                DepthStencilState.None, Main.Rasterizer, null, Main.GameViewMatrix.TransformationMatrix);

            sb.Draw(core, pos, null, Color.Black * MathHelper.Clamp(intensity * 1.1f, 0f, 1f), rot, coreOrigin,
                new Vector2(sx, sy) * 1.02f, SpriteEffects.None, 0f);

            sb.End();
            sb.Begin(SpriteSortMode.Deferred, BlendState.Additive, Main.DefaultSamplerState,
                DepthStencilState.None, Main.Rasterizer, null, Main.GameViewMatrix.TransformationMatrix);

            sb.Draw(ring, pos, null, RimColor * intensity, rot, ringOrigin, rimScale, SpriteEffects.None, 0f);
            sb.Draw(ring, pos, null, RimColor2 * (0.75f * intensity), -rot * 1.7f, ringOrigin,
                rimScale * 0.82f, SpriteEffects.None, 0f);
            sb.Draw(glow, pos, null, FlashColor * (0.35f * intensity * pulse), 0f, glowOrigin,
                new Vector2(sx, sy) * 0.4f, SpriteEffects.None, 0f);

            if (flash > 0f)
            {
                sb.Draw(glow, pos, null, FlashColor * flash, 0f, glowOrigin,
                    new Vector2(4.5f * flash + 0.5f, 0.45f), SpriteEffects.None, 0f);
                sb.Draw(glow, pos, null, FlashColor * flash, MathHelper.PiOver2, glowOrigin,
                    new Vector2(3.5f * flash + 0.5f, 0.35f), SpriteEffects.None, 0f);
                sb.Draw(glow, pos, null, FlashColor * (0.9f * flash), 0f, glowOrigin,
                    new Vector2(sx, sy) * (1.5f + 2f * flash), SpriteEffects.None, 0f);
            }

            sb.End();
            sb.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, Main.DefaultSamplerState,
                DepthStencilState.None, Main.Rasterizer, null, Main.GameViewMatrix.TransformationMatrix);

            return false;
        }
    }

    public class ApathyPortalAssets : ModSystem
    {
        public static Texture2D Glow { get; private set; }
        public static Texture2D Ring { get; private set; }
        public static Texture2D Core { get; private set; }

        internal static void EnsureLoaded()
        {
            if (Glow != null || Main.dedServ)
                return;

            Glow = MakeGlow(256);
            Ring = MakeRing(256, 0.78f, 0.16f);
            Core = MakeCore(256);
        }

        public override void Unload()
        {
            Texture2D g = Glow, r = Ring, c = Core;
            Glow = Ring = Core = null;
            Main.QueueMainThreadAction(() =>
            {
                g?.Dispose();
                r?.Dispose();
                c?.Dispose();
            });
        }

        private static Texture2D MakeGlow(int size)
        {
            return Generate(size, r =>
            {
                float a = MathHelper.Clamp(1f - r, 0f, 1f);
                a = a * a * (3f - 2f * a);
                return MathF.Pow(a, 1.5f);
            });
        }

        private static Texture2D MakeRing(int size, float radius, float thickness)
        {
            return Generate(size, r =>
            {
                float a = MathHelper.Clamp(1f - MathF.Abs(r - radius) / thickness, 0f, 1f);
                return a * a * (3f - 2f * a);
            });
        }

        private static Texture2D MakeCore(int size)
        {
            return Generate(size, r =>
            {
                float a = 1f - MathHelper.Clamp((r - 0.55f) / 0.4f, 0f, 1f);
                return a * a * (3f - 2f * a);
            });
        }

        private static Texture2D Generate(int size, Func<float, float> alphaByRadius)
        {
            var tex = new Texture2D(Main.instance.GraphicsDevice, size, size);
            var data = new Color[size * size];
            float half = size / 2f;

            for (int y = 0; y < size; y++)
            {
                for (int x = 0; x < size; x++)
                {
                    float dx = (x + 0.5f - half) / half;
                    float dy = (y + 0.5f - half) / half;
                    float r = MathF.Sqrt(dx * dx + dy * dy);
                    float a = MathHelper.Clamp(alphaByRadius(r), 0f, 1f);
                    data[y * size + x] = new Color(a, a, a, a);
                }
            }

            tex.SetData(data);
            return tex;
        }
    }
}

