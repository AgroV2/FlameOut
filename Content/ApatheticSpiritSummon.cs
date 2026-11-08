using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.Audio;
using Terraria.Graphics.CameraModifiers;
using Terraria.ID;
using Terraria.ModLoader;

namespace flameout.Content
{
    public class ApatheticSpiritSummon : ModProjectile
    {
        private const int SummonTime = 120;   
        private const int EndTime = 150;

        private static readonly Color AuraColor = new Color(110, 35, 185);
        private static readonly Color RimColor = new Color(195, 100, 255);
        private static readonly Color CoreColor = new Color(60, 10, 110);
        private static readonly Color FlashColor = new Color(240, 215, 255);

        public override string Texture => "Terraria/Images/Projectile_0";

        private float Timer => Projectile.ai[0];

        internal static bool IsActiveFor(Player player)
        {
            int type = ModContent.ProjectileType<ApatheticSpiritSummon>();
            for (int i = 0; i < Main.maxProjectiles; i++)
            {
                Projectile p = Main.projectile[i];
                if (p.active && p.type == type && p.owner == player.whoAmI)
                    return true;
            }
            return false;
        }

        public override void SetDefaults()
        {
            Projectile.width = 100;
            Projectile.height = 100;
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
            Projectile.velocity = Vector2.Zero;
            Projectile.ai[0]++;
            float t = Timer;

            float grow = MathHelper.Clamp(t / SummonTime, 0f, 1f);
            Projectile.localAI[0] += MathHelper.Lerp(0.03f, 0.26f, grow);

            float scale = GetScale(t);
            float radius = 90f * scale;

            if (!Main.dedServ)
            {
                if (t == 1f)
                    SoundEngine.PlaySound(SoundID.Item122, Projectile.Center);

                EmitDust(t, radius);

                if (t < SummonTime && t % 10f == 0f)
                {
                    Main.instance.CameraModifiers.Add(new PunchCameraModifier(
                        Projectile.Center, Main.rand.NextVector2Unit(),
                        MathHelper.Lerp(1f, 6f, grow), 7f, 12, 2000f, "ApatheticSpirit_Rumble"));
                }

                Lighting.AddLight(Projectile.Center, 0.6f * scale, 0.2f * scale, 1f * scale);
            }

            if (t == SummonTime)
            {
                if (!Main.dedServ)
                {
                    SoundEngine.PlaySound(SoundID.Zombie105, Projectile.Center);
                    Main.instance.CameraModifiers.Add(new PunchCameraModifier(
                        Projectile.Center, Main.rand.NextVector2Unit(), 16f, 9f, 30, 2600f, "ApatheticSpirit_Burst"));

                    for (int i = 0; i < 80; i++)
                    {
                        Dust dust = Dust.NewDustPerfect(Projectile.Center,
                            Main.rand.NextBool(3) ? DustID.PurpleTorch : DustID.Shadowflame,
                            Main.rand.NextVector2Circular(10f, 10f), 0, default,
                            Main.rand.NextFloat(1.4f, 2.4f));
                        dust.noGravity = true;
                        dust.fadeIn = 1.1f;
                    }
                }

                if (Main.netMode != NetmodeID.MultiplayerClient)
                {
                    int npc = NPC.NewNPC(Projectile.GetSource_FromAI(),
                        (int)Projectile.Center.X, (int)Projectile.Center.Y,
                        ModContent.NPCType<ApatheticSpirit>());

                    if (npc < Main.maxNPCs && Main.netMode == NetmodeID.Server)
                        NetMessage.SendData(MessageID.SyncNPC, number: npc);
                }
            }

            if (t >= EndTime)
                Projectile.Kill();
        }

        private void EmitDust(float t, float radius)
        {
            Vector2 center = Projectile.Center;

            if (t < SummonTime)
            {
                
                for (int i = 0; i < 3; i++)
                {
                    float angle = Main.rand.NextFloat(MathHelper.TwoPi);
                    Vector2 pos = center + angle.ToRotationVector2() * (radius + Main.rand.NextFloat(40f, 130f));
                    Vector2 inward = (center - pos).SafeNormalize(Vector2.Zero);
                    Vector2 vel = inward * Main.rand.NextFloat(3f, 6.5f)
                                + inward.RotatedBy(MathHelper.PiOver2) * Main.rand.NextFloat(1.5f, 4f);
                    Dust d = Dust.NewDustPerfect(pos, DustID.Shadowflame, vel, 0, default,
                        Main.rand.NextFloat(1.1f, 1.9f));
                    d.noGravity = true;
                }

                
                for (int i = 0; i < 2; i++)
                {
                    float angle = Main.rand.NextFloat(MathHelper.TwoPi);
                    Vector2 pos = center + angle.ToRotationVector2() * radius * Main.rand.NextFloat(0.9f, 1.05f);
                    Vector2 vel = (angle + MathHelper.PiOver2).ToRotationVector2() * Main.rand.NextFloat(3f, 6f);
                    Dust d = Dust.NewDustPerfect(pos,
                        Main.rand.NextBool(4) ? DustID.PurpleTorch : DustID.Shadowflame,
                        vel, 0, default, Main.rand.NextFloat(1.2f, 2f));
                    d.noGravity = true;
                }
                return;
            }

            
            for (int i = 0; i < 2; i++)
            {
                Dust d = Dust.NewDustPerfect(center + Main.rand.NextVector2Circular(radius, radius),
                    DustID.Shadowflame, Main.rand.NextVector2Circular(2f, 2f), 0, default,
                    Main.rand.NextFloat(0.9f, 1.5f));
                d.noGravity = true;
            }
        }

        private static float GetScale(float t)
        {
            if (t < SummonTime)
                return MathHelper.Lerp(0.05f, 1f, EaseOutCubic(t / SummonTime));

            float p = MathHelper.Clamp((t - SummonTime) / (EndTime - SummonTime), 0f, 1f);
            return MathHelper.Clamp(1f - EaseInCubic(p), 0f, 1f);
        }

        private static float EaseOutCubic(float p) => 1f - (1f - p) * (1f - p) * (1f - p);
        private static float EaseInCubic(float p) => p * p * p;

        public override bool PreDraw(ref Color lightColor)
        {
            ApathyPortalAssets.EnsureLoaded();
            Texture2D glow = ApathyPortalAssets.Glow;
            Texture2D ring = ApathyPortalAssets.Ring;
            Texture2D core = ApathyPortalAssets.Core;
            if (glow == null || ring == null || core == null)
                return false;

            float t = Timer;
            float scale = GetScale(t);
            Vector2 position = Projectile.Center - Main.screenPosition;
            float rotation = Projectile.localAI[0];
            float pulse = 1f + 0.07f * MathF.Sin(t * 0.13f);
            float flash = MathHelper.Clamp(1f - MathF.Abs(t - SummonTime) / 18f, 0f, 1f);

            SpriteBatch sb = Main.spriteBatch;
            sb.End();
            sb.Begin(SpriteSortMode.Deferred, BlendState.Additive, Main.DefaultSamplerState,
                DepthStencilState.None, Main.Rasterizer, null, Main.GameViewMatrix.TransformationMatrix);

            sb.Draw(glow, position, null, AuraColor * (0.9f * scale), rotation * 0.35f,
                glow.Size() / 2f, Vector2.One * scale * 2.2f * pulse, SpriteEffects.None, 0f);
            sb.Draw(glow, position, null, RimColor * (0.45f * scale), -rotation * 0.6f,
                glow.Size() / 2f, Vector2.One * scale * 1.2f, SpriteEffects.None, 0f);

            sb.End();
            sb.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, Main.DefaultSamplerState,
                DepthStencilState.None, Main.Rasterizer, null, Main.GameViewMatrix.TransformationMatrix);
            sb.Draw(core, position, null, CoreColor * MathHelper.Clamp(scale, 0f, 1f), rotation,
                core.Size() / 2f, Vector2.One * scale * 0.85f, SpriteEffects.None, 0f);

            sb.End();
            sb.Begin(SpriteSortMode.Deferred, BlendState.Additive, Main.DefaultSamplerState,
                DepthStencilState.None, Main.Rasterizer, null, Main.GameViewMatrix.TransformationMatrix);

            sb.Draw(ring, position, null, RimColor * scale, rotation, ring.Size() / 2f,
                Vector2.One * scale * pulse, SpriteEffects.None, 0f);
            sb.Draw(ring, position, null, AuraColor * (0.7f * scale), -rotation * 1.6f, ring.Size() / 2f,
                Vector2.One * scale * 0.78f, SpriteEffects.None, 0f);

            if (flash > 0f)
            {
                sb.Draw(glow, position, null, FlashColor * flash, 0f, glow.Size() / 2f,
                    new Vector2(4.5f * flash + 0.5f, 0.45f), SpriteEffects.None, 0f);
                sb.Draw(glow, position, null, FlashColor * flash, MathHelper.PiOver2, glow.Size() / 2f,
                    new Vector2(3.5f * flash + 0.5f, 0.35f), SpriteEffects.None, 0f);
                sb.Draw(glow, position, null, FlashColor * (flash * 0.8f), 0f, glow.Size() / 2f,
                    Vector2.One * (1.5f + flash * 2f), SpriteEffects.None, 0f);
            }

            sb.End();
            sb.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, Main.DefaultSamplerState,
                DepthStencilState.None, Main.Rasterizer, null, Main.GameViewMatrix.TransformationMatrix);
            return false;
        }
    }
}