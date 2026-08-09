

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using Terraria;
using Terraria.GameContent;
using Terraria.Graphics.CameraModifiers;
using Terraria.ID;
using Terraria.ModLoader;

namespace flameout.Content
{
    public class ApathyDeathEffect : ModProjectile
    {
        public override string Texture => "Terraria/Images/Projectile_0";

        private const int DURATION = 1200;

        
        private const int CIRCLES_START = 0;
        private const int CIRCLES_END = 200;
        private const int MOONS_START = 150;
        private const int MOONS_END = 400;
        private const int GEO_START = 300;
        private const int GEO_END = 600;
        private const int STARS_START = 400;
        private const int STARFIELD_START = 100;
        private const int COMETS_START = 250;
        private const int SHOCKWAVE_START = 300;
        private const int VORTEX_START = 500;
        private const int VORTEX_END = 800;
        private const int CHAOS_START = 700;   
        private const int CHAOS_PEAK = 1100;   
        private const int FLASH_CIRCLE_START = 1100; 
        private const int FLASH_SCREEN_START = 1150; 

        
        private static readonly Color[] StrobePalette =
        {
            new Color(255, 0, 40),
            new Color(255, 255, 255),
            new Color(120, 0, 255),
            new Color(255, 0, 200),
            new Color(30, 30, 40),
        };

        private static readonly int[] PentagramOrder = { 0, 2, 4, 1, 3, 0 };

        
        private static readonly Vector2[] Pts = new Vector2[192];

        private static Texture2D Pixel => TextureAssets.MagicPixel.Value;

        
        private static float Hash(int n)
        {
            unchecked
            {
                n = (n ^ 61) ^ (n >> 16);
                n *= 9;
                n ^= n << 4;
                n *= 0x27d4eb2d;
                n ^= n >> 15;
                return (n & 0x7FFFFFFF) / 2147483647f;
            }
        }

        public override void SetDefaults()
        {
            Projectile.width = 2;
            Projectile.height = 2;
            Projectile.hostile = false;
            Projectile.friendly = false;
            Projectile.ignoreWater = true;
            Projectile.tileCollide = false;
            Projectile.penetrate = -1;
            Projectile.timeLeft = DURATION + 10;
            Projectile.hide = true;
        }

        public override void SetStaticDefaults()
        {
            ProjectileID.Sets.DrawScreenCheckFluff[Type] = 100000;
        }

        public override void AI()
        {
            float timer = Projectile.ai[0];
            Projectile.ai[0]++;
            Projectile.velocity = Vector2.Zero;

            float chaos = Utils.GetLerpValue(CHAOS_START, CHAOS_PEAK, timer, true);

            
            float lightBoost = 1f + chaos * 0.8f;
            Lighting.AddLight(Projectile.Center, 1.1f * lightBoost, 0.15f * lightBoost, 0.3f * lightBoost);

            if (!Main.dedServ)
            {
                HandleShake((int)timer, chaos);
                SpawnDust(timer, chaos);
            }

            
            
            if (Projectile.ai[0] >= DURATION)
            {
                if (Projectile.ai[1] != 1f && Main.netMode != NetmodeID.MultiplayerClient)
                {
                    NPC.NewNPC(
                        Projectile.GetSource_FromAI(),
                               (int)Projectile.Center.X,
                               (int)Projectile.Center.Y,
                               ModContent.NPCType<Empathy>()
                    );
                }
                Projectile.Kill();
                return;
            }
        }

        
        private void HandleShake(int t, float chaos)
        {
            switch (t)
            {
                case 0: Shake(6f, 20); break;
                case MOONS_START: Shake(4f, 15); break;
                case GEO_START: Shake(5f, 15); break;
                case VORTEX_START: Shake(5f, 18); break;
                case CHAOS_START: Shake(8f, 25); break;
                case FLASH_CIRCLE_START: Shake(14f, 35); break;
                case FLASH_SCREEN_START: Shake(18f, 45); break;
            }

            if (chaos > 0f && t > CHAOS_START && t % 40 == 0)
                Shake(3f + chaos * 6f, 18);
        }

        private void Shake(float strength, int frames)
        {
            Main.instance.CameraModifiers.Add(new PunchCameraModifier(
                Projectile.Center, Main.rand.NextVector2Unit(), strength, 8f, frames, 2400f, "flameout:Apathy"));
        }

        private void SpawnDust(float timer, float chaos)
        {
            
            int per = 2 + (int)(chaos * 5f);
            for (int i = 0; i < per; i++)
            {
                float ang = timer * 0.06f + MathHelper.TwoPi * i / per;
                float dist = 480f + 160f * (float)Math.Sin(timer * 0.04f + i * 1.3f);
                Vector2 pos = Projectile.Center + ang.ToRotationVector2() * dist;
                Vector2 toCenter = (Projectile.Center - pos).SafeNormalize(Vector2.Zero);
                Vector2 vel = toCenter.RotatedBy(0.9f) * (2.5f + chaos * 5f) + toCenter * (1.5f + chaos * 4f);
                int type = (i % 3 == 0) ? DustID.Shadowflame : ((i % 3 == 1) ? DustID.RedTorch : DustID.PurpleTorch);
                Dust d = Dust.NewDustPerfect(pos, type, vel, 0, default, 1.1f + chaos * 0.8f);
                d.noGravity = true;
            }

            
            if (timer >= FLASH_CIRCLE_START)
            {
                for (int i = 0; i < 8; i++)
                {
                    Vector2 pos = Projectile.Center + Main.rand.NextVector2Unit() * Main.rand.NextFloat(120f, 500f);
                    Vector2 vel = (Projectile.Center - pos) * 0.09f;
                    Dust d = Dust.NewDustPerfect(pos, DustID.RedTorch, vel, 0, default, 1.6f);
                    d.noGravity = true;
                }
            }
        }

        
        
        

        private void DrawLine(SpriteBatch sb, Vector2 start, Vector2 end, Color color, float thickness)
        {
            Vector2 edge = end - start;
            float angle = (float)Math.Atan2(edge.Y, edge.X);
            sb.Draw(
                Pixel,
                start,
                new Rectangle(0, 0, 1, 1),
                    color,
                    angle,
                    Vector2.Zero,
                    new Vector2(edge.Length(), thickness),
                    SpriteEffects.None,
                    0f
            );
        }

        private void DrawPolyline(SpriteBatch sb, int count, Color color, float thickness)
        {
            for (int i = 0; i < count - 1; i++)
                DrawLine(sb, Pts[i], Pts[i + 1], color, thickness);
        }

        private void DrawCircle(SpriteBatch sb, Vector2 center, float radius, Color color, float thickness, int segments = 0)
        {
            if (radius <= 1f) return;
            if (segments <= 0)
                segments = (int)MathHelper.Clamp(radius * 0.16f, 14f, 56f);
            for (int i = 0; i <= segments; i++)
            {
                float angle = MathHelper.TwoPi * i / segments;
                Pts[i] = center + angle.ToRotationVector2() * radius;
            }
            DrawPolyline(sb, segments + 1, color, thickness);
        }

        
        private void DrawWobbleCircle(SpriteBatch sb, Vector2 center, float radius, Color color, float thickness, int waves, float amp, float phase)
        {
            if (radius <= 1f) return;
            int segments = (int)MathHelper.Clamp(radius * 0.16f, 18f, 64f);
            for (int i = 0; i <= segments; i++)
            {
                float ang = MathHelper.TwoPi * i / segments;
                float r = radius * (1f + amp * (float)Math.Sin(waves * ang + phase));
                Pts[i] = center + ang.ToRotationVector2() * r;
            }
            DrawPolyline(sb, segments + 1, color, thickness);
        }

        
        private void DrawFilledCircle(SpriteBatch sb, Vector2 center, float radius, Color color)
        {
            if (radius <= 1f) return;
            int strips = (int)MathHelper.Clamp(radius * 0.22f, 10f, 60f);
            for (int i = 0; i < strips; i++)
            {
                float y0 = -radius + 2f * radius * i / strips;
                float y1 = -radius + 2f * radius * (i + 1) / strips;
                float ym = (y0 + y1) * 0.5f;
                float half = (float)Math.Sqrt(Math.Max(0f, radius * radius - ym * ym));
                if (half < 1f) continue;
                int h = Math.Max(1, (int)(y1 - y0 + 0.5f));
                sb.Draw(Pixel, new Rectangle((int)(center.X - half), (int)(center.Y + y0), (int)(half * 2f), h), color);
            }
        }

        private void DrawArc(SpriteBatch sb, Vector2 center, float radius, float startAngle, float sweep, Color color, float thickness)
        {
            if (radius <= 1f) return;
            int segments = (int)MathHelper.Clamp(radius * sweep * 0.08f, 6f, 40f);
            for (int i = 0; i <= segments; i++)
            {
                float ang = startAngle + sweep * i / segments;
                Pts[i] = center + ang.ToRotationVector2() * radius;
            }
            DrawPolyline(sb, segments + 1, color, thickness);
        }

        private void DrawEllipse(SpriteBatch sb, Vector2 center, float a, float b, float rotation, Color color, float thickness)
        {
            if (a <= 1f || b <= 1f) return;
            int segments = (int)MathHelper.Clamp(Math.Max(a, b) * 0.15f, 16f, 48f);
            float cos = (float)Math.Cos(rotation);
            float sin = (float)Math.Sin(rotation);
            for (int i = 0; i <= segments; i++)
            {
                float t = MathHelper.TwoPi * i / segments;
                float x = (float)Math.Cos(t) * a;
                float y = (float)Math.Sin(t) * b;
                Pts[i] = center + new Vector2(x * cos - y * sin, x * sin + y * cos);
            }
            DrawPolyline(sb, segments + 1, color, thickness);
        }

        
        private void DrawSpiral(SpriteBatch sb, Vector2 center, float maxRadius, float turns, float rotation, Color color, float thickness)
        {
            if (maxRadius <= 1f) return;
            const int segments = 80;
            for (int i = 0; i <= segments; i++)
            {
                float t = i / (float)segments;
                float ang = rotation + turns * MathHelper.TwoPi * t;
                Pts[i] = center + ang.ToRotationVector2() * (maxRadius * t);
            }
            DrawPolyline(sb, segments + 1, color, thickness);
        }

        
        private void DrawRose(SpriteBatch sb, Vector2 center, float radius, int k, float rotation, Color color, float thickness)
        {
            if (radius <= 1f) return;
            const int segments = 120;
            for (int i = 0; i <= segments; i++)
            {
                float theta = MathHelper.TwoPi * i / segments;
                float r = radius * (float)Math.Cos(k * theta);
                float ang = theta + rotation;
                Pts[i] = center + ang.ToRotationVector2() * r;
            }
            DrawPolyline(sb, segments + 1, color, thickness);
        }

        
        private void DrawLightning(SpriteBatch sb, Vector2 start, Vector2 end, Color color, float thickness, float amplitude)
        {
            const int segs = 8;
            Vector2 dir = end - start;
            Vector2 normal = dir.SafeNormalize(Vector2.Zero).RotatedBy(MathHelper.PiOver2);
            Vector2 prev = start;
            for (int i = 1; i <= segs; i++)
            {
                float t = i / (float)segs;
                Vector2 p = start + dir * t;
                if (i < segs)
                {
                    float env = (float)Math.Sin(t * MathHelper.Pi); 
                    p += normal * (Main.rand.NextFloat(-1f, 1f) * amplitude * env);
                }
                DrawLine(sb, prev, p, color, thickness);
                prev = p;
            }
        }

        
        private void DrawRuneTicks(SpriteBatch sb, Vector2 center, float radius, int count, float rotation, Color color, float thickness, float length)
        {
            if (radius <= 4f) return;
            for (int i = 0; i < count; i++)
            {
                float ang = MathHelper.TwoPi * i / count + rotation;
                Vector2 dir = ang.ToRotationVector2();
                float len = (i % 4 == 0) ? length * 2.2f : length;
                DrawLine(sb, center + dir * radius, center + dir * (radius + len), color, thickness);
            }
        }

        private void DrawPolygon(SpriteBatch sb, Vector2 center, float radius, int sides, float rotation, Color color, float thickness)
        {
            if (radius <= 1f) return;
            for (int i = 0; i <= sides; i++)
            {
                float angle = MathHelper.TwoPi * i / sides + rotation;
                Pts[i] = center + angle.ToRotationVector2() * radius;
            }
            DrawPolyline(sb, sides + 1, color, thickness);
        }

        private void DrawStar(SpriteBatch sb, Vector2 center, float outerR, float innerR, int points, float rotation, Color color, float thickness)
        {
            if (outerR <= 1f) return;
            int total = points * 2;
            for (int i = 0; i <= total; i++)
            {
                float angle = MathHelper.TwoPi * i / total + rotation;
                float r = (i % 2 == 0) ? outerR : innerR;
                Pts[i] = center + angle.ToRotationVector2() * r;
            }
            DrawPolyline(sb, total + 1, color, thickness);
        }

        
        private void DrawStarPolygon(SpriteBatch sb, Vector2 center, float radius, int n, int k, float rotation, Color color, float thickness)
        {
            if (radius <= 1f) return;
            for (int i = 0; i < n; i++)
            {
                float angle = MathHelper.TwoPi * i / n + rotation - MathHelper.PiOver2;
                Pts[i] = center + angle.ToRotationVector2() * radius;
            }
            for (int i = 0; i < n; i++)
                DrawLine(sb, Pts[i], Pts[(i + k) % n], color, thickness);
        }

        
        private void DrawSparkle(SpriteBatch sb, Vector2 center, float size, float rotation, Color color, float thickness, bool eightPoint = true)
        {
            if (size <= 0.5f) return;
            for (int i = 0; i < 2; i++)
            {
                float ang = rotation + MathHelper.PiOver2 * i;
                Vector2 dir = ang.ToRotationVector2() * size;
                DrawLine(sb, center - dir, center + dir, color, thickness);
            }
            if (eightPoint)
            {
                for (int i = 0; i < 2; i++)
                {
                    float ang = rotation + MathHelper.PiOver4 + MathHelper.PiOver2 * i;
                    Vector2 dir = ang.ToRotationVector2() * (size * 0.45f);
                    DrawLine(sb, center - dir, center + dir, color, thickness * 0.8f);
                }
            }
        }

        private void DrawBigTriangle(SpriteBatch sb, Vector2 center, float radius, float rotation, Color color, float thickness)
        {
            if (radius <= 1f) return;
            for (int i = 0; i < 3; i++)
            {
                float angle = MathHelper.TwoPi * i / 3f + rotation - MathHelper.PiOver2;
                Pts[i] = center + angle.ToRotationVector2() * radius;
            }
            DrawLine(sb, Pts[0], Pts[1], color, thickness);
            DrawLine(sb, Pts[1], Pts[2], color, thickness);
            DrawLine(sb, Pts[2], Pts[0], color, thickness);
        }

        private void DrawHexagram(SpriteBatch sb, Vector2 center, float radius, float rotation, Color color, float thickness)
        {
            DrawPolygon(sb, center, radius, 3, rotation, color, thickness);
            DrawPolygon(sb, center, radius, 3, rotation + MathHelper.Pi, color, thickness);
        }

        private void DrawPentagram(SpriteBatch sb, Vector2 center, float radius, float rotation, Color color, float thickness)
        {
            if (radius <= 1f) return;
            for (int i = 0; i < 5; i++)
            {
                float angle = MathHelper.TwoPi * i / 5f + rotation - MathHelper.PiOver2;
                Pts[i] = center + angle.ToRotationVector2() * radius;
            }
            for (int i = 0; i < PentagramOrder.Length - 1; i++)
                DrawLine(sb, Pts[PentagramOrder[i]], Pts[PentagramOrder[i + 1]], color, thickness);
        }

        private void DrawCrescent(SpriteBatch sb, Vector2 center, float radius, float rotation, Color color, float thickness)
        {
            if (radius <= 1.5f) return;
            int segments = (int)MathHelper.Clamp(radius * 0.6f, 10f, 28f);

            for (int i = 0; i <= segments; i++)
            {
                float angle = MathHelper.Pi * i / segments + rotation;
                Pts[i] = center + angle.ToRotationVector2() * radius;
            }
            Vector2 outerFirst = Pts[0];
            Vector2 outerLast = Pts[segments];
            DrawPolyline(sb, segments + 1, color, thickness);

            float innerRadius = radius * 0.75f;
            Vector2 innerOffset = (rotation + MathHelper.PiOver2).ToRotationVector2() * (radius * 0.25f);
            for (int i = 0; i <= segments; i++)
            {
                float angle = MathHelper.Pi * i / segments + rotation;
                Pts[i] = center + innerOffset + angle.ToRotationVector2() * innerRadius;
            }
            DrawPolyline(sb, segments + 1, color, thickness);

            DrawLine(sb, outerFirst, Pts[0], color, thickness);
            DrawLine(sb, outerLast, Pts[segments], color, thickness);
        }

        private void DrawEyeOfDarkness(SpriteBatch sb, Vector2 center, float alpha, float scale)
        {
            Texture2D tex = ModContent.Request<Texture2D>("flameout/Content/eyeofdarkness", ReLogic.Content.AssetRequestMode.ImmediateLoad).Value;
            Vector2 origin = tex.Size() * 0.5f;
            sb.Draw(tex, center, null, Color.White * alpha, 0f, origin, scale, SpriteEffects.None, 0f);
        }

        
        
        

        
        private Color Strobe(Color baseColor, int salt, float timer, float amount)
        {
            if (amount <= 0.001f) return baseColor;
            int idx = (int)(timer / 3f + salt) % StrobePalette.Length;
            if (idx < 0) idx += StrobePalette.Length;
            return Color.Lerp(baseColor, StrobePalette[idx], amount);
        }

        
        
        

        private void DrawGlowCore(SpriteBatch sb, Vector2 center, float timer, float S, float chaos)
        {
            float glowProgress = Math.Min(timer / 600f, 1f);
            float glowRadius = MathHelper.Lerp(160f, 1500f, glowProgress) * S;
            float glowAlpha = MathHelper.Lerp(0.05f, 0.16f, glowProgress) * (1f + chaos * 0.5f);

            for (int g = 4; g >= 1; g--)
            {
                float r = glowRadius * (0.25f + g * 0.19f);
                float a = glowAlpha * (1.05f - g * 0.18f);
                DrawFilledCircle(sb, center, r, new Color(1f, 0.05f, 0.1f, 0f) * a);
            }
            
            DrawFilledCircle(sb, center, 60f * S * (1f + chaos * 0.8f), new Color(1f, 0.4f, 0.5f, 0f) * (0.25f + 0.15f * chaos));
        }

        private void DrawWaves(SpriteBatch sb, Vector2 center, float timer, float S, float chaos)
        {
            float maxRadius = Math.Max(Main.screenWidth, Main.screenHeight) * 1.35f;

            
            const int total = 56;
            for (int i = 0; i < total; i++)
            {
                float delay = i * 7f;
                float lt = timer - delay;
                if (lt <= 0f) continue;

                float progress = Math.Min(lt / 75f, 1f);
                float radius = maxRadius * ((i + 1f) / total) * progress * S;
                float alpha = MathHelper.Lerp(0.7f, 0.22f, i / (float)total) * progress;

                Color c;
                int mod = i % 4;
                if (mod == 0) c = new Color(220, 0, 30);
                else if (mod == 1) c = Color.White;
                else if (mod == 2) c = new Color(140, 0, 255);
                else c = new Color(25, 25, 25);
                c = Strobe(c, i, timer, chaos * 0.7f) * alpha;

                float thick = MathHelper.Lerp(3f, 1.2f, i / (float)total);
                if (chaos > 0.01f)
                    DrawWobbleCircle(sb, center, radius, c, thick, 3 + i % 6, 0.035f * chaos, timer * 0.05f + i);
                else
                    DrawCircle(sb, center, radius, c, thick);
            }

            
            float inTimer = timer - 90f;
            if (inTimer > 0f)
            {
                const int inRings = 18;
                const float cycle = 200f;
                for (int i = 0; i < inRings; i++)
                {
                    float lt = inTimer - i * 11f;
                    if (lt <= 0f) continue;
                    float p = (lt % cycle) / cycle;
                    float radius = maxRadius * (1f - p) * S;
                    float alpha = (0.05f + 0.4f * p * p) * (0.5f + chaos * 0.5f);
                    Color c = (i % 2 == 0) ? new Color(255, 40, 40) : new Color(255, 220, 255);
                    DrawCircle(sb, center, radius, c * alpha, 1.6f);
                }
            }
        }

        private void DrawExpandingCircles(SpriteBatch sb, Vector2 center, float timer, float S, float chaos, float warped)
        {
            if (timer < CIRCLES_START) return;
            float phase = Math.Min((timer - CIRCLES_START) / (float)(CIRCLES_END - CIRCLES_START), 1f);

            const int count = 20;
            for (int i = 0; i < count; i++)
            {
                float cp = MathHelper.Clamp(phase * count - i, 0f, 1f);
                if (cp <= 0f) continue;

                float baseRadius = MathHelper.Lerp(55f, 680f, i / (count - 1f));
                baseRadius *= 0.9f + 0.2f * Hash(i * 31 + 7); 
                float radius = baseRadius * cp * S;
                float alpha = MathHelper.Lerp(0.25f, 0.8f, cp);

                Color c = Color.Lerp(new Color(190, 0, 80), Color.White, i / (count - 1f));
                if (i % 4 == 2) c = new Color(150, 40, 255);
                c = Strobe(c, i * 3, timer, chaos * 0.8f) * alpha;

                DrawWobbleCircle(sb, center, radius, c, 1.8f, 3 + (i % 5), 0.02f + 0.05f * chaos, warped * 0.03f + i * 1.7f);
            }
        }

        private void DrawMoonRings(SpriteBatch sb, Vector2 center, float timer, float S, float chaos, float warped)
        {
            if (timer < MOONS_START) return;
            float phase = Math.Min((timer - MOONS_START) / (float)(MOONS_END - MOONS_START), 1f);
            float alpha = Math.Min(phase * 2f, 1f);

            
            DrawMoonRing(sb, center, 14, MathHelper.Lerp(90f, 340f, phase) * S, MathHelper.Lerp(8f, 40f, phase),
                         warped * 0.008f, new Color(220, 100, 255), alpha, timer, chaos, warped, 0);
            
            DrawMoonRing(sb, center, 10, MathHelper.Lerp(55f, 205f, phase) * S, MathHelper.Lerp(6f, 26f, phase),
                         -warped * 0.012f, new Color(90, 220, 255), alpha, timer, chaos, warped, 100);
        }

        private void DrawMoonRing(SpriteBatch sb, Vector2 center, int count, float orbit, float size, float spin,
                                  Color color, float alpha, float timer, float chaos, float warped, int salt)
        {
            for (int i = 0; i < count; i++)
            {
                float angle = MathHelper.TwoPi * i / count + spin;
                Vector2 moonCenter = center + angle.ToRotationVector2() * orbit;
                Color mc = Strobe(color, i + salt, timer, chaos * 0.7f) * alpha;
                DrawCrescent(sb, moonCenter, size, angle + MathHelper.PiOver2, mc, 2f);

                
                for (int j = 0; j < 2; j++)
                {
                    float subAngle = warped * 0.045f * (j == 0 ? 1f : -1f) + i * 2.399f + j * MathHelper.Pi;
                    Vector2 subCenter = moonCenter + subAngle.ToRotationVector2() * (size + 13f);
                    DrawCircle(sb, subCenter, 4.5f, mc * 0.9f, 1.3f, 10);
                }
            }
        }

        private void DrawGeometryStorm(SpriteBatch sb, Vector2 center, float timer, float S, float chaos, float warped)
        {
            if (timer < GEO_START) return;
            float phase = Math.Min((timer - GEO_START) / (float)(GEO_END - GEO_START), 1f);
            float alpha = Math.Min(phase * 2f, 1f);
            float rot = warped * 0.005f;
            float sl = chaos * 0.8f;

            
            DrawBigTriangle(sb, center, MathHelper.Lerp(50f, 260f, phase) * S, -rot * 0.6f, Strobe(new Color(255, 50, 50), 1, timer, sl) * alpha, 3f);
            DrawHexagram(sb, center, MathHelper.Lerp(30f, 190f, phase) * S, rot, Strobe(Color.White, 2, timer, sl) * alpha, 2.5f);
            DrawPentagram(sb, center, MathHelper.Lerp(20f, 135f, phase) * S, rot * 1.3f, Strobe(new Color(255, 80, 180), 3, timer, sl) * alpha, 2f);
            DrawPentagram(sb, center, MathHelper.Lerp(40f, 230f, phase) * S, -rot * 0.8f, Strobe(new Color(200, 150, 255), 4, timer, sl) * alpha, 2f);

            
            for (int n = 0; n < 7; n++)
            {
                int sides = 3 + n;
                float r = MathHelper.Lerp(300f, 70f, n / 6f) * phase * S;
                float dir = (n % 2 == 0) ? 1f : -1f;
                Color c = Strobe(Color.Lerp(new Color(255, 30, 60), new Color(160, 120, 255), n / 6f), n * 5 + 20, timer, chaos * 0.85f) * (alpha * 0.8f);
                DrawPolygon(sb, center, r, sides, rot * dir * (1f + n * 0.25f), c, 1.6f);
            }

            
            for (int ring = 0; ring < 3; ring++)
            {
                float r = (150f + ring * 95f) * phase * S;
                float dir = (ring % 2 == 0) ? 1f : -1f;
                int frags = 4 + ring;
                for (int f = 0; f < frags; f++)
                {
                    float start = MathHelper.TwoPi * f / frags + warped * (0.006f + ring * 0.004f) * dir;
                    float sweep = MathHelper.TwoPi / frags * 0.55f;
                    Color c = (ring == 1) ? Color.White : new Color(255, 60, 90);
                    DrawArc(sb, center, r, start, sweep, Strobe(c, ring * 9 + f, timer, chaos * 0.7f) * (alpha * 0.9f), 2.2f);
                }
            }

            
            DrawRuneTicks(sb, center, 355f * phase * S, 48, -warped * 0.003f, Color.White * (alpha * 0.55f), 1.2f, 10f);
            DrawRuneTicks(sb, center, 160f * phase * S, 32, warped * 0.005f, new Color(255, 120, 160) * (alpha * 0.6f), 1.2f, 7f);

            
            for (int e = 0; e < 3; e++)
            {
                float tilt = MathHelper.Pi * e / 3f + warped * 0.004f * ((e % 2 == 0) ? 1f : -1f);
                Color c = Strobe(new Color(255, 200, 255), e * 7 + 40, timer, chaos * 0.6f) * (alpha * 0.5f);
                DrawEllipse(sb, center, 300f * phase * S, 110f * phase * S, tilt, c, 1.5f);
            }
        }

        private void DrawVortex(SpriteBatch sb, Vector2 center, float timer, float S, float chaos, float warped)
        {
            if (timer < VORTEX_START) return;
            float phase = Math.Min((timer - VORTEX_START) / (float)(VORTEX_END - VORTEX_START), 1f);
            float alpha = Math.Min(phase * 2f, 1f);
            float maxR = 520f * phase * S;

            
            for (int i = 0; i < 3; i++)
            {
                float off = MathHelper.TwoPi * i / 3f;
                DrawSpiral(sb, center, maxR, 2.6f, warped * 0.012f + off, Strobe(new Color(255, 40, 70), i + 50, timer, chaos * 0.8f) * (alpha * 0.8f), 1.8f);
                DrawSpiral(sb, center, maxR * 0.8f, 2.1f, -warped * 0.009f + off, Strobe(new Color(200, 220, 255), i + 60, timer, chaos * 0.8f) * (alpha * 0.65f), 1.4f);
            }

            
            DrawRose(sb, center, 190f * phase * S, 5, warped * 0.006f, Strobe(new Color(255, 0, 130), 70, timer, chaos * 0.7f) * (alpha * 0.7f), 1.4f);
            DrawRose(sb, center, 270f * phase * S, 7, -warped * 0.004f, Strobe(new Color(130, 90, 255), 71, timer, chaos * 0.7f) * (alpha * 0.6f), 1.3f);

            
            const int nodes = 10;
            float baseA = warped * 0.01f;
            for (int i = 0; i < nodes; i++)
            {
                float a1 = baseA + MathHelper.TwoPi * i / nodes;
                float a2 = baseA + MathHelper.TwoPi * ((i + 1) % nodes) / nodes;
                float r1 = (i % 2 == 0) ? 210f : 330f;
                float r2 = ((i + 1) % 2 == 0) ? 210f : 330f;
                Vector2 p1 = center + a1.ToRotationVector2() * r1 * phase * S;
                Vector2 p2 = center + a2.ToRotationVector2() * r2 * phase * S;

                Color c = new Color(255, 190, 255) * (alpha * (0.35f + 0.4f * Main.rand.NextFloat()));
                DrawLightning(sb, p1, p2, c, 1.5f, 16f * (0.5f + chaos));

                if (i % 3 == 0)
                {
                    float a3 = baseA + MathHelper.TwoPi * ((i + 4) % nodes) / nodes;
                    float r3 = ((i + 4) % 2 == 0) ? 210f : 330f;
                    Vector2 p3 = center + a3.ToRotationVector2() * r3 * phase * S;
                    DrawLightning(sb, p1, p3, c * 0.7f, 1.2f, 22f * (0.5f + chaos));
                }

                DrawCircle(sb, p1, 6f, Color.White * alpha, 1.5f, 12);
            }
        }

        private void DrawStars(SpriteBatch sb, Vector2 center, float timer, float S, float chaos, float warped)
        {
            if (timer < STARS_START) return;
            float phase = Math.Min((timer - STARS_START) / 120f, 1f);
            float alpha = phase;

            
            DrawStar(sb, center, 240f * phase * S, 96f * phase * S, 7, warped * 0.004f, Strobe(new Color(255, 230, 240), 80, timer, chaos * 0.8f) * (alpha * 0.7f), 1.8f);
            DrawStar(sb, center, 150f * phase * S, 45f * phase * S, 4, -warped * 0.007f, Strobe(new Color(255, 60, 60), 81, timer, chaos * 0.8f) * (alpha * 0.8f), 2f);

            
            for (int i = 0; i < 5; i++)
            {
                float a = warped * 0.006f * ((i % 2 == 0) ? 1f : -1.4f) + MathHelper.TwoPi * i / 5f;
                float orbit = (380f + 40f * (float)Math.Sin(warped * 0.02f + i * 2f)) * phase * S;
                Vector2 p = center + a.ToRotationVector2() * orbit;
                DrawStar(sb, p, 22f, 9f, 5, warped * 0.03f + i, Strobe(new Color(255, 150, 220), 90 + i, timer, chaos) * alpha, 1.4f);
            }
        }

        
        private void DrawStarField(SpriteBatch sb, Vector2 center, float timer, float S, float chaos, float warped)
        {
            if (timer < STARFIELD_START) return;
            float appear = Math.Min((timer - STARFIELD_START) / 90f, 1f);

            const int count = 44;
            for (int i = 0; i < count; i++)
            {
                
                float baseAng = Hash(i * 7 + 1) * MathHelper.TwoPi + warped * 0.0012f * ((i % 2 == 0) ? 1f : -1f);
                float dist = MathHelper.Lerp(120f, 780f, Hash(i * 13 + 5)) * S;
                Vector2 pos = center + baseAng.ToRotationVector2() * dist;

                
                float tw = 0.5f + 0.5f * (float)Math.Sin(timer * (0.05f + 0.07f * Hash(i * 3)) + Hash(i * 11) * 6.28f);
                float size = MathHelper.Lerp(4f, 14f, Hash(i * 17 + 2)) * (0.6f + 0.6f * tw) * (1f + chaos * 0.5f);
                float alpha = appear * (0.15f + 0.55f * tw) * (0.7f + 0.3f * chaos);

                Color c = (i % 3 == 0) ? new Color(255, 120, 150) : ((i % 3 == 1) ? Color.White : new Color(190, 150, 255));
                c = Strobe(c, i, timer, chaos * 0.5f);
                DrawSparkle(sb, pos, size, warped * 0.01f * ((i % 2 == 0) ? 1f : -1f), c * alpha, 1.3f, i % 2 == 0);
            }
        }

        
        private void DrawStarMandala(SpriteBatch sb, Vector2 center, float timer, float S, float chaos, float warped)
        {
            if (timer < STARS_START) return;
            float phase = Math.Min((timer - STARS_START) / 150f, 1f);
            float alpha = phase;
            float sl = chaos * 0.85f;

            DrawStarPolygon(sb, center, 285f * phase * S, 11, 3, warped * 0.0035f, Strobe(new Color(255, 90, 130), 110, timer, sl) * (alpha * 0.75f), 1.7f);
            DrawStarPolygon(sb, center, 225f * phase * S, 9, 4, -warped * 0.005f, Strobe(new Color(255, 255, 255), 111, timer, sl) * (alpha * 0.7f), 1.5f);
            DrawStarPolygon(sb, center, 170f * phase * S, 8, 3, warped * 0.007f, Strobe(new Color(180, 110, 255), 112, timer, sl) * (alpha * 0.8f), 1.5f);
            DrawStarPolygon(sb, center, 120f * phase * S, 7, 2, -warped * 0.009f, Strobe(new Color(255, 60, 90), 113, timer, sl) * (alpha * 0.85f), 1.6f);

            
            float ringR = 420f * phase * S;
            for (int i = 0; i < 16; i++)
            {
                float ang = MathHelper.TwoPi * i / 16f + warped * 0.006f;
                Vector2 p = center + ang.ToRotationVector2() * ringR;
                float twinkle = 0.6f + 0.4f * (float)Math.Sin(warped * 0.08f + i * 1.3f);
                Color c = Strobe((i % 2 == 0) ? Color.White : new Color(255, 150, 200), 120 + i, timer, sl);
                DrawStar(sb, p, 11f * twinkle, 4.5f * twinkle, 4, warped * 0.02f + i, c * (alpha * 0.8f), 1.3f);
            }
        }

        
        private void DrawComets(SpriteBatch sb, Vector2 center, float timer, float S, float chaos, float warped)
        {
            if (timer < COMETS_START) return;
            float appear = Math.Min((timer - COMETS_START) / 100f, 1f);

            const int comets = 8;
            for (int i = 0; i < comets; i++)
            {
                float speed = (0.014f + 0.008f * Hash(i * 5 + 3)) * ((i % 2 == 0) ? 1f : -1f);
                float a = MathHelper.Lerp(180f, 560f, Hash(i * 9 + 1));      
                float b = a * MathHelper.Lerp(0.5f, 0.95f, Hash(i * 9 + 2)); 
                float tilt = Hash(i * 9 + 4) * MathHelper.Pi;
                float phase0 = Hash(i * 9 + 6) * MathHelper.TwoPi;

                
                const int tail = 12;
                Vector2 prev = CometPos(center, warped, speed, a, b, tilt, phase0, 0f, S);
                for (int j = 1; j <= tail; j++)
                {
                    Vector2 p = CometPos(center, warped, speed, a, b, tilt, phase0, j * 3.5f, S);
                    float fade = 1f - j / (float)tail;
                    Color tc = new Color(255, 170, 220) * (appear * 0.5f * fade * fade);
                    DrawLine(sb, prev, p, tc, 2.2f * fade + 0.4f);
                    prev = p;
                }

                
                Vector2 head = CometPos(center, warped, speed, a, b, tilt, phase0, 0f, S);
                Color hc = Strobe(Color.White, 130 + i, timer, chaos * 0.6f) * appear;
                DrawStar(sb, head, 10f, 4f, 4, warped * 0.05f + i, hc * 0.9f, 1.4f);
                DrawSparkle(sb, head, 17f, warped * 0.03f, hc * 0.6f, 1.1f, false);
            }
        }

        private Vector2 CometPos(Vector2 center, float warped, float speed, float a, float b, float tilt, float phase0, float back, float S)
        {
            float t = (warped - back) * speed + phase0;
            float x = (float)Math.Cos(t) * a;
            float y = (float)Math.Sin(t) * b;
            float cos = (float)Math.Cos(tilt);
            float sin = (float)Math.Sin(tilt);
            return center + new Vector2(x * cos - y * sin, x * sin + y * cos) * S;
        }

        
        private void DrawStarShockwaves(SpriteBatch sb, Vector2 center, float timer, float S, float chaos, float warped)
        {
            if (timer < SHOCKWAVE_START) return;

            int slots = 2 + (int)(chaos * 3f);
            for (int slot = 0; slot < slots; slot++)
            {
                float period = 68f + slot * 13f;
                float local = timer - SHOCKWAVE_START - slot * 23f;
                if (local <= 0f) continue;
                int cycleIdx = (int)(local / period);
                float age = (local % period) / period;
                int seed = cycleIdx * 29 + slot * 173;

                int points = 4 + (seed % 5);
                float maxR = MathHelper.Lerp(320f, 620f, Hash(seed + 3));
                float r = MathHelper.Lerp(25f, maxR, age) * S;
                float rot = Hash(seed + 7) * MathHelper.TwoPi + age * 1.6f * ((seed % 2 == 0) ? 1f : -1f);
                float alpha = (1f - age) * (0.5f + 0.3f * chaos);

                Vector2 off = new Vector2(Hash(seed + 11) - 0.5f, Hash(seed + 13) - 0.5f) * 260f * chaos * S;
                Color c = (seed % 3 == 0) ? new Color(255, 80, 110) : ((seed % 3 == 1) ? Color.White : new Color(200, 130, 255));
                DrawStar(sb, center + off, r, r * 0.45f, points, rot, c * alpha, 2.4f * (1f - age) + 0.6f);
            }
        }

        
        private void DrawStarRain(SpriteBatch sb, Vector2 center, float timer, float S, float chaos, float warped)
        {
            if (chaos <= 0f) return;

            int meteors = 4 + (int)(chaos * 8f);
            for (int i = 0; i < meteors; i++)
            {
                float period = 34f + 9f * Hash(i * 7 + 2);
                float local = timer - CHAOS_START - i * 6f;
                if (local <= 0f) continue;
                int cycleIdx = (int)(local / period);
                float age = (local % period) / period; 
                int seed = cycleIdx * 31 + i * 211;

                float ang = Hash(seed) * MathHelper.TwoPi;
                float startR = MathHelper.Lerp(750f, 1000f, Hash(seed + 1));
                float dist = MathHelper.Lerp(startR, 70f, age * age) * S; 
                Vector2 dir = ang.ToRotationVector2();
                Vector2 pos = center + dir * dist;

                float alpha = chaos * Math.Min(age * 4f, 1f) * (0.5f + 0.5f * Hash(seed + 2));
                Color c = (seed % 2 == 0) ? Color.White : new Color(255, 140, 170);

                
                float tailLen = MathHelper.Lerp(60f, 150f, Hash(seed + 3)) * (0.4f + age);
                DrawLine(sb, pos, pos + dir * tailLen, c * (alpha * 0.6f), 1.8f);
                DrawSparkle(sb, pos, 9f + 5f * Hash(seed + 4), warped * 0.06f + i, c * alpha, 1.4f, false);
            }
        }

        
        private void DrawEyeHalo(SpriteBatch sb, Vector2 center, float timer, float chaos, float warped, float collapse)
        {
            float appear = Math.Min(timer / 45f, 1f);
            float k = MathHelper.Lerp(1f, 1.35f, 1f - collapse);

            
            DrawStar(sb, center, 95f * k * (1f + chaos * 0.35f), 22f * k, 12, warped * 0.0025f, new Color(255, 60, 80) * (appear * 0.75f), 1.8f);
            
            DrawStar(sb, center, 65f * k, 18f * k, 6, -warped * 0.006f, Color.White * (appear * 0.6f), 1.5f);
            
            DrawSparkle(sb, center, 30f * k, warped * 0.008f, new Color(255, 200, 220) * (appear * 0.8f), 1.6f, true);
        }

        private void DrawCrescendo(SpriteBatch sb, Vector2 center, float timer, float S, float chaos, float warped)
        {
            if (chaos <= 0f) return;

            
            const int rays = 44;
            int step = (int)(timer / 3f);
            for (int i = 0; i < rays; i++)
            {
                float a = MathHelper.TwoPi * i / rays + warped * 0.0015f + Hash(i * 17) * 0.2f;
                float len = MathHelper.Lerp(220f, 950f, Hash(i * 13 + step * 7)) * S;
                float rayAlpha = (0.1f + 0.3f * Hash(i * 29 + step * 11)) * chaos;
                Color c = (i % 5 == 0) ? new Color(255, 60, 60) : new Color(255, 235, 245);
                Vector2 dir = a.ToRotationVector2();
                DrawLine(sb, center + dir * 70f * S, center + dir * len, c * rayAlpha, 1.4f);
            }

            
            for (int slot = 0; slot < 6; slot++)
            {
                float period = 42f + slot * 7f;
                float local = timer - CHAOS_START - slot * 9f;
                if (local <= 0f) continue;
                int cycleIdx = (int)(local / period);
                float age = (local % period) / period;
                int seed = cycleIdx * 13 + slot * 101;

                Vector2 off = new Vector2(Hash(seed) - 0.5f, Hash(seed + 1) - 0.5f) * 640f * S;
                float radius = MathHelper.Lerp(10f, 260f, age) * S;
                float a = (1f - age) * 0.55f * chaos;
                Color c = (seed % 3 == 0) ? new Color(255, 50, 80) : ((seed % 3 == 1) ? Color.White : new Color(170, 90, 255));
                DrawWobbleCircle(sb, center + off, radius, c * a, 2f, 5, 0.08f, timer * 0.1f + slot);
            }

            
            for (int i = 0; i < 5; i++)
            {
                int step2 = (int)(timer / 5f) + i * 77;
                Vector2 off = new Vector2(Hash(step2 * 3 + 1) - 0.5f, Hash(step2 * 3 + 2) - 0.5f) * 700f * S;
                float r = 14f + Hash(step2) * 46f;
                int sides = 3 + (step2 % 4);
                DrawPolygon(sb, center + off, r, sides, warped * 0.05f + i, Color.White * (0.35f * chaos), 1.3f);
            }
        }

        private void DrawEye(SpriteBatch sb, Vector2 center, float timer, float chaos, float collapse)
        {
            float eyeAppear = Math.Min(timer / 30f, 1f);
            float scale = MathHelper.Lerp(1f, 1.35f, 1f - collapse); 
            DrawEyeOfDarkness(sb, center, eyeAppear, scale);
        }

        private void DrawCollapseFlash(SpriteBatch sb, Vector2 center, float timer)
        {
            if (timer < FLASH_CIRCLE_START) return;
            float p = Math.Min((timer - FLASH_CIRCLE_START) / 50f, 1f);
            float ringR = MathHelper.Lerp(0f, 950f, p * p);

            for (int k = 0; k < 3; k++)
                DrawCircle(sb, center, ringR * (1f - k * 0.07f), Color.White * ((1f - p) * (0.9f - k * 0.2f) + 0.1f), 7f - k * 2f);

            DrawFilledCircle(sb, center, MathHelper.Lerp(20f, 420f, p * p), Color.White * (0.5f * p));
        }

        private void DrawScreenFlash(SpriteBatch sb, float timer)
        {
            if (timer < FLASH_SCREEN_START) return;
            float phase = Math.Min((timer - FLASH_SCREEN_START) / 50f, 1f);
            sb.Draw(Pixel, new Rectangle(-400, -400, Main.screenWidth + 800, Main.screenHeight + 800), Color.White * phase);
        }

        

        public override void DrawBehind(int index, List<int> behindNPCsAndTiles, List<int> behindNPCs, List<int> behindProjectiles, List<int> overPlayers, List<int> overWiresUI)
        {
            overPlayers.Add(index);
        }

        public override bool PreDraw(ref Color lightColor)
        {
            float timer = Projectile.ai[0];
            Vector2 center = Projectile.Center - Main.screenPosition;
            SpriteBatch sb = Main.spriteBatch;

            
            float chaos = Utils.GetLerpValue(CHAOS_START, CHAOS_PEAK, timer, true);
            
            float warped = timer + chaos * chaos * 900f + Math.Max(0f, timer - CHAOS_PEAK) * 3.5f;
            
            float collapse = 1f;
            if (timer >= FLASH_CIRCLE_START)
            {
                float p = Utils.GetLerpValue(FLASH_CIRCLE_START, FLASH_CIRCLE_START + 45f, timer, true);
                collapse = MathHelper.Lerp(1f, 0.03f, p * p);
            }
            float S = collapse;

            sb.End();
            sb.Begin(SpriteSortMode.Deferred, BlendState.Additive, SamplerState.LinearClamp, DepthStencilState.None, RasterizerState.CullNone, null, Main.GameViewMatrix.TransformationMatrix);

            DrawGlowCore(sb, center, timer, S, chaos);
            DrawStarField(sb, center, timer, S, chaos, warped);
            DrawWaves(sb, center, timer, S, chaos);
            DrawExpandingCircles(sb, center, timer, S, chaos, warped);
            DrawMoonRings(sb, center, timer, S, chaos, warped);
            DrawGeometryStorm(sb, center, timer, S, chaos, warped);
            DrawVortex(sb, center, timer, S, chaos, warped);
            DrawStars(sb, center, timer, S, chaos, warped);
            DrawStarMandala(sb, center, timer, S, chaos, warped);
            DrawComets(sb, center, timer, S, chaos, warped);
            DrawStarShockwaves(sb, center, timer, S, chaos, warped);
            DrawCrescendo(sb, center, timer, S, chaos, warped);
            DrawStarRain(sb, center, timer, S, chaos, warped);
            DrawEyeHalo(sb, center, timer, chaos, warped, collapse);
            DrawEye(sb, center, timer, chaos, collapse);
            DrawCollapseFlash(sb, center, timer);
            DrawScreenFlash(sb, timer);

            sb.End();
            sb.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.LinearClamp, DepthStencilState.None, RasterizerState.CullNone, null, Main.GameViewMatrix.TransformationMatrix);

            return false;
        }
    }
}

