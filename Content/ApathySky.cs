

using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.Graphics.Effects;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.Utilities;

namespace flameout.Content
{
    public class ApathySkySystem : ModSystem
    {
        public const string Key = "flameout:ApathySky";

        public override void Load()
        {
            if (Main.dedServ)
                return;

            SkyManager.Instance[Key] = new ApathySky();
        }

        public override void PostUpdateEverything()
        {
            if (Main.dedServ || Main.gameMenu)
                return;

            bool bossHere = NPC.AnyNPCs(ModContent.NPCType<ApathyBoss>());
            bool skyOn = SkyManager.Instance[Key]?.IsActive() ?? false;

            if (bossHere && !skyOn)
                SkyManager.Instance.Activate(Key);
            else if (!bossHere && skyOn)
                SkyManager.Instance.Deactivate(Key);
        }
    }

    public class ApathySky : CustomSky
    {
        private bool active;
        private float intensity;
        private float time;

        private const int FireCount = 520;
        private const int RayCount = 6;
        private const int StarCount = 26;
        private const int ShardCount = 120;
        private const int CrackCount = 7;
        private const int AshCount = 260;

        private struct Flame
        {
            public float X, Y, Life, MaxLife, Size, Sway, Speed;
        }

        private struct Ray
        {
            public float X, Width, Tilt, Phase, Speed, Length;
        }

        private struct Star
        {
            public float X, Y, VX, VY, Rot, Spin, Size, Life, MaxLife;
            public bool Broken;
        }

        private struct Shard
        {
            public float X, Y, VX, VY, Rot, Spin, Size, Life;
        }

        
        private class Crack
        {
            public Vector2[] Spine;
            public Vector2[][] Branches;
            public float Life, MaxLife, Grow;
        }

        private struct Ash
        {
            public float X, Y, VY, Sway, SwaySpeed, Size, Dim;
        }

        private readonly Flame[] flames = new Flame[FireCount];
        private readonly Ray[] rays = new Ray[RayCount];
        private readonly Star[] stars = new Star[StarCount];
        private readonly Shard[] shards = new Shard[ShardCount];
        private readonly Crack[] cracks = new Crack[CrackCount];
        private readonly Ash[] ashes = new Ash[AshCount];
        private readonly UnifiedRandom rng = new UnifiedRandom(0x1EAF);

        

        public override void OnLoad() { }

        public override void Activate(Vector2 position, params object[] args)
        {
            active = true;
            for (int i = 0; i < FireCount; i++)
                ResetFlame(ref flames[i], true);
            for (int i = 0; i < RayCount; i++)
                ResetRay(ref rays[i]);
            for (int i = 0; i < StarCount; i++)
                ResetStar(ref stars[i], true);
            for (int i = 0; i < ShardCount; i++)
                shards[i].Life = 0f;
            for (int i = 0; i < CrackCount; i++)
                cracks[i] = MakeCrack(rng.NextFloat() * 240f);
            for (int i = 0; i < AshCount; i++)
                ResetAsh(ref ashes[i], true);
        }

        public override void Deactivate(params object[] args) => active = false;

        public override void Reset() => active = false;

        public override bool IsActive() => active || intensity > 0.001f;

        public override float GetCloudAlpha() => 1f - intensity;

        public override Color OnTileColor(Color inColor)
        {
            
            Color tinted = new Color(
                (byte)(inColor.R * 0.72f + 26),
                (byte)(inColor.G * 0.55f),
                (byte)(inColor.B * 0.85f + 30),
                inColor.A);
            return Color.Lerp(inColor, tinted, intensity);
        }

        public override void Update(GameTime gameTime)
        {
            float target = active ? 1f : 0f;
            intensity = MathHelper.Lerp(intensity, target, 0.03f);
            if (intensity < 0.001f && !active)
                return;

            time += 1f;

            for (int i = 0; i < FireCount; i++)
                UpdateFlame(ref flames[i]);
            for (int i = 0; i < RayCount; i++)
                UpdateRay(ref rays[i]);
            for (int i = 0; i < StarCount; i++)
                UpdateStar(ref stars[i]);
            for (int i = 0; i < ShardCount; i++)
                UpdateShard(ref shards[i]);
            for (int i = 0; i < CrackCount; i++)
                UpdateCrack(i);
            for (int i = 0; i < AshCount; i++)
                UpdateAsh(ref ashes[i]);
        }

        

        private float W => Main.screenWidth;
        private float H => Main.screenHeight;

        private void ResetFlame(ref Flame f, bool scatter)
        {
            f.X = rng.NextFloat() * W;
            f.MaxLife = 70f + rng.NextFloat() * 130f;
            f.Life = scatter ? rng.NextFloat() * f.MaxLife : 0f;
            f.Y = H - rng.NextFloat() * 60f;
            f.Size = 2.5f + rng.NextFloat() * 6f;
            f.Sway = rng.NextFloat() * MathHelper.TwoPi;
            f.Speed = 0.9f + rng.NextFloat() * 2.4f;
        }

        private void UpdateFlame(ref Flame f)
        {
            f.Life++;
            f.Y -= f.Speed;
            f.Sway += 0.05f;
            f.X += (float)Math.Sin(f.Sway) * 0.9f;
            if (f.Life >= f.MaxLife || f.Y < -f.Size)
                ResetFlame(ref f, false);
        }

        private void ResetRay(ref Ray r)
        {
            r.X = rng.NextFloat() * (W + 400f) - 200f;
            r.Width = 40f + rng.NextFloat() * 170f;
            r.Tilt = -0.32f + rng.NextFloat() * 0.64f;
            r.Phase = rng.NextFloat() * MathHelper.TwoPi;
            r.Speed = 0.006f + rng.NextFloat() * 0.016f;
            
            r.Length = H + 400f;
        }

        private void UpdateRay(ref Ray r)
        {
            r.Phase += r.Speed;
            r.X += r.Tilt * 0.25f;
            if (r.X < -300f || r.X > W + 300f)
                ResetRay(ref r);
        }

        private void ResetStar(ref Star s, bool scatter)
        {
            s.X = rng.NextFloat() * (W + 300f) - 150f;
            s.Y = scatter ? rng.NextFloat() * H * 0.7f : -60f - rng.NextFloat() * 200f;
            s.VX = -1.6f + rng.NextFloat() * 3.2f;
            s.VY = 1.1f + rng.NextFloat() * 2.6f;
            s.Rot = rng.NextFloat() * MathHelper.TwoPi;
            s.Spin = -0.05f + rng.NextFloat() * 0.1f;
            s.Size = 9f + rng.NextFloat() * 22f;
            s.MaxLife = 200f + rng.NextFloat() * 260f;
            s.Life = 0f;
            s.Broken = false;
        }

        private void UpdateStar(ref Star s)
        {
            s.Life++;
            s.X += s.VX;
            s.Y += s.VY;
            s.VY += 0.008f;
            s.Rot += s.Spin;

            
            if (!s.Broken && (s.Life >= s.MaxLife || s.Y > H * 0.92f))
            {
                s.Broken = true;
                int pieces = 6 + rng.Next(6);
                for (int k = 0; k < pieces; k++)
                    SpawnShard(s.X, s.Y, s.Size);
                ResetStar(ref s, false);
            }
        }

        private void SpawnShard(float x, float y, float parentSize)
        {
            for (int i = 0; i < ShardCount; i++)
            {
                if (shards[i].Life > 0f)
                    continue;

                float a = rng.NextFloat() * MathHelper.TwoPi;
                float sp = 0.7f + rng.NextFloat() * 3.4f;
                shards[i].X = x;
                shards[i].Y = y;
                shards[i].VX = (float)Math.Cos(a) * sp;
                shards[i].VY = (float)Math.Sin(a) * sp - 0.6f;
                shards[i].Rot = rng.NextFloat() * MathHelper.TwoPi;
                shards[i].Spin = -0.2f + rng.NextFloat() * 0.4f;
                shards[i].Size = parentSize * (0.14f + rng.NextFloat() * 0.24f);
                shards[i].Life = 45f + rng.NextFloat() * 60f;
                return;
            }
        }

        
        
        private Crack MakeCrack(float startLife)
        {
            Crack c = new Crack();
            c.MaxLife = 260f + rng.NextFloat() * 320f;
            c.Grow = 40f + rng.NextFloat() * 50f;
            c.Life = startLife;

            Vector2 pos = new Vector2(rng.NextFloat() * W, rng.NextFloat() * H * 0.75f);
            float dir = rng.NextFloat() * MathHelper.TwoPi;
            int steps = 7 + rng.Next(8);
            float step = 26f + rng.NextFloat() * 60f;

            Vector2[] spine = new Vector2[steps + 1];
            spine[0] = pos;
            for (int i = 1; i <= steps; i++)
            {
                dir += -0.45f + rng.NextFloat() * 0.9f;
                pos += dir.ToRotationVector2() * step * (0.7f + rng.NextFloat() * 0.6f);
                spine[i] = pos;
            }
            c.Spine = spine;

            int branchCount = 2 + rng.Next(4);
            c.Branches = new Vector2[branchCount][];
            for (int b = 0; b < branchCount; b++)
            {
                int anchor = 1 + rng.Next(steps - 1);
                Vector2 bp = spine[anchor];
                float bdir = (spine[anchor + 1] - spine[anchor]).ToRotation()
                             + (rng.NextBool() ? 1f : -1f) * (0.5f + rng.NextFloat() * 0.7f);
                int bsteps = 2 + rng.Next(4);
                Vector2[] br = new Vector2[bsteps + 1];
                br[0] = bp;
                for (int i = 1; i <= bsteps; i++)
                {
                    bdir += -0.35f + rng.NextFloat() * 0.7f;
                    bp += bdir.ToRotationVector2() * step * (0.4f + rng.NextFloat() * 0.4f);
                    br[i] = bp;
                }
                c.Branches[b] = br;
            }
            return c;
        }

        private void UpdateCrack(int i)
        {
            Crack c = cracks[i];
            if (c == null)
            {
                cracks[i] = MakeCrack(0f);
                return;
            }

            c.Life++;
            if (c.Life >= c.MaxLife)
                cracks[i] = MakeCrack(0f);
        }

        private void ResetAsh(ref Ash a, bool scatter)
        {
            a.X = rng.NextFloat() * W;
            a.Y = scatter ? rng.NextFloat() * H : -rng.NextFloat() * 120f;
            a.VY = 0.25f + rng.NextFloat() * 1.1f;
            a.Sway = rng.NextFloat() * MathHelper.TwoPi;
            a.SwaySpeed = 0.008f + rng.NextFloat() * 0.03f;
            a.Size = 1f + rng.NextFloat() * 2.6f;
            a.Dim = 0.25f + rng.NextFloat() * 0.75f;
        }

        private void UpdateAsh(ref Ash a)
        {
            a.Y += a.VY;
            a.Sway += a.SwaySpeed;
            a.X += (float)Math.Sin(a.Sway) * 0.55f;
            if (a.Y > H + 10f)
                ResetAsh(ref a, false);
        }

        private void UpdateShard(ref Shard s)
        {
            if (s.Life <= 0f)
                return;

            s.Life--;
            s.X += s.VX;
            s.Y += s.VY;
            s.VY += 0.05f;
            s.VX *= 0.99f;
            s.Rot += s.Spin;
        }

        

        public override void Draw(SpriteBatch sb, float minDepth, float maxDepth)
        {
            
            if (!(maxDepth >= float.MaxValue && minDepth < float.MaxValue))
                return;
            if (intensity <= 0.001f)
                return;

            Texture2D px = Terraria.GameContent.TextureAssets.MagicPixel.Value;

            DrawBackdrop(sb, px);

            sb.End();
            sb.Begin(SpriteSortMode.Deferred, BlendState.Additive, SamplerState.LinearClamp,
                DepthStencilState.None, RasterizerState.CullNone, null, Main.BackgroundViewMatrix.TransformationMatrix);

            DrawRays(sb, px);
            DrawCracks(sb, px);
            DrawStars(sb, px);
            DrawShards(sb, px);
            DrawAsh(sb, px);
            DrawFire(sb, px);

            sb.End();
            sb.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.LinearClamp,
                DepthStencilState.None, RasterizerState.CullNone, null, Main.BackgroundViewMatrix.TransformationMatrix);
        }

        private void DrawBackdrop(SpriteBatch sb, Texture2D px)
        {
            
            const int bands = 32;
            int h = (int)Math.Ceiling(H / bands) + 1;
            for (int i = 0; i < bands; i++)
            {
                float t = i / (float)(bands - 1);
                Color top = new Color(28, 6, 48);
                Color mid = new Color(13, 3, 26);
                Color low = new Color(4, 1, 8);
                Color c = t < 0.6f
                    ? Color.Lerp(top, mid, t / 0.6f)
                    : Color.Lerp(mid, low, (t - 0.6f) / 0.4f);

                sb.Draw(px, new Rectangle(0, (int)(H * t) - 1, (int)W, h), c * intensity);
            }

            
            float breathe = 0.5f + 0.5f * (float)Math.Sin(time * 0.012f);
            int glowH = (int)(H * 0.45f);
            for (int i = 0; i < 24; i++)
            {
                float t = i / 23f;
                int y = (int)(H - glowH * (1f - t));
                sb.Draw(px, new Rectangle(0, y, (int)W, glowH / 24 + 2),
                    new Color(90, 20, 150) * (0.05f + 0.05f * breathe) * (1f - t) * intensity);
            }
        }

        private void DrawQuad(SpriteBatch sb, Texture2D px, Vector2 center, float rot,
            float length, float width, Color color)
        {
            sb.Draw(px, center, new Rectangle(0, 0, 1, 1), color, rot,
                new Vector2(0.5f, 0.5f), new Vector2(length, width), SpriteEffects.None, 0f);
        }

        private void DrawLine(SpriteBatch sb, Texture2D px, Vector2 a, Vector2 b, float thickness, Color color)
        {
            Vector2 edge = b - a;
            sb.Draw(px, a, new Rectangle(0, 0, 1, 1), color, edge.ToRotation(),
                new Vector2(0f, 0.5f), new Vector2(edge.Length(), thickness), SpriteEffects.None, 0f);
        }

        private void DrawRays(SpriteBatch sb, Texture2D px)
        {
            foreach (Ray r in rays)
            {
                float flicker = 0.35f + 0.65f * (0.5f + 0.5f * (float)Math.Sin(r.Phase));
                float a = flicker * intensity;

                Vector2 top = new Vector2(r.X, -40f);
                Vector2 bottom = top + new Vector2(r.Tilt, 1f).SafeNormalize(Vector2.UnitY) * r.Length;
                Vector2 mid = (top + bottom) * 0.5f;
                float rot = (bottom - top).ToRotation();
                float len = (bottom - top).Length();

                DrawQuad(sb, px, mid, rot, len, r.Width, new Color(110, 35, 195) * 0.06f * a);
                DrawQuad(sb, px, mid, rot, len, r.Width * 0.45f, new Color(165, 80, 240) * 0.10f * a);
                DrawQuad(sb, px, mid, rot, len, r.Width * 0.12f, new Color(215, 185, 250) * 0.17f * a);
            }
        }

        
        private void DrawCracks(SpriteBatch sb, Texture2D px)
        {
            foreach (Crack c in cracks)
            {
                if (c == null)
                    continue;

                float grow = MathHelper.Clamp(c.Life / c.Grow, 0f, 1f);
                float fade = MathHelper.Clamp((c.MaxLife - c.Life) / 90f, 0f, 1f);
                float a = grow * fade * intensity * 0.5f;
                if (a <= 0.01f)
                    continue;

                DrawPolyline(sb, px, c.Spine, grow, 3.2f, new Color(120, 40, 200) * 0.30f * a);
                DrawPolyline(sb, px, c.Spine, grow, 1.1f, new Color(220, 190, 255) * 0.55f * a);

                foreach (Vector2[] br in c.Branches)
                {
                    float bg = MathHelper.Clamp((c.Life - c.Grow * 0.5f) / c.Grow, 0f, 1f);
                    if (bg <= 0f)
                        continue;
                    DrawPolyline(sb, px, br, bg, 2.0f, new Color(110, 35, 190) * 0.22f * a);
                    DrawPolyline(sb, px, br, bg, 0.9f, new Color(210, 175, 255) * 0.40f * a);
                }
            }
        }

        
        private void DrawPolyline(SpriteBatch sb, Texture2D px, Vector2[] pts,
            float progress, float thickness, Color color)
        {
            if (pts == null || pts.Length < 2)
                return;

            float shown = (pts.Length - 1) * MathHelper.Clamp(progress, 0f, 1f);
            int full = (int)shown;
            float partial = shown - full;

            for (int i = 0; i < full && i < pts.Length - 1; i++)
                DrawLine(sb, px, pts[i], pts[i + 1], thickness, color);

            if (full < pts.Length - 1 && partial > 0f)
                DrawLine(sb, px, pts[full], Vector2.Lerp(pts[full], pts[full + 1], partial), thickness, color);
        }

        private void DrawAsh(SpriteBatch sb, Texture2D px)
        {
            foreach (Ash a in ashes)
            {
                DrawQuad(sb, px, new Vector2(a.X, a.Y), 0f, a.Size, a.Size,
                    new Color(150, 95, 200) * 0.16f * a.Dim * intensity);
            }
        }

        private void DrawStarShape(SpriteBatch sb, Texture2D px, Vector2 c, float radius,
            float rot, float thickness, Color color)
        {
            const int points = 5;
            Vector2[] pts = new Vector2[points];
            for (int i = 0; i < points; i++)
            {
                float ang = -MathHelper.PiOver2 + MathHelper.TwoPi * i / points + rot;
                pts[i] = c + ang.ToRotationVector2() * radius;
            }
            for (int i = 0; i < points; i++)
                DrawLine(sb, px, pts[i], pts[(i + 2) % points], thickness, color);
        }

        private void DrawStars(SpriteBatch sb, Texture2D px)
        {
            foreach (Star s in stars)
            {
                if (s.Broken)
                    continue;

                float fade = MathHelper.Clamp(1f - s.Life / s.MaxLife, 0f, 1f);
                float a = intensity * (0.45f + 0.55f * fade) * 0.35f;
                Vector2 c = new Vector2(s.X, s.Y);

                
                Vector2 tail = c - new Vector2(s.VX, s.VY) * 14f;
                DrawLine(sb, px, tail, c, s.Size * 0.22f, new Color(150, 60, 230) * 0.25f * a);

                DrawStarShape(sb, px, c, s.Size, s.Rot, s.Size * 0.30f, new Color(110, 30, 190) * 0.40f * a);
                DrawStarShape(sb, px, c, s.Size, s.Rot, s.Size * 0.14f, new Color(190, 120, 240) * 0.55f * a);
                DrawStarShape(sb, px, c, s.Size, s.Rot, s.Size * 0.05f, new Color(235, 215, 255) * 0.7f * a);

                DrawQuad(sb, px, c, 0f, s.Size * 0.45f, s.Size * 0.45f, Color.White * 0.35f * a);
            }
        }

        private void DrawShards(SpriteBatch sb, Texture2D px)
        {
            foreach (Shard s in shards)
            {
                if (s.Life <= 0f)
                    continue;

                float a = MathHelper.Clamp(s.Life / 60f, 0f, 1f) * intensity * 0.4f;
                Vector2 c = new Vector2(s.X, s.Y);

                DrawQuad(sb, px, c, s.Rot, s.Size * 3.4f, s.Size * 0.7f, new Color(180, 100, 240) * 0.45f * a);
                DrawQuad(sb, px, c, s.Rot + MathHelper.PiOver2, s.Size * 2.0f, s.Size * 0.5f,
                    new Color(230, 200, 255) * 0.55f * a);
            }
        }

        private void DrawFire(SpriteBatch sb, Texture2D px)
        {
            foreach (Flame f in flames)
            {
                float t = MathHelper.Clamp(f.Life / f.MaxLife, 0f, 1f);
                float a = (1f - t) * intensity;
                float size = f.Size * (0.4f + 0.6f * (1f - t));
                Vector2 c = new Vector2(f.X, f.Y);

                
                Color hot = Color.Lerp(new Color(225, 190, 250), new Color(105, 20, 185), t);
                DrawQuad(sb, px, c, 0f, size, size, hot * 0.40f * a);
            }

            
            float flick = 0.75f + 0.25f * (float)Math.Sin(time * 0.09f);
            for (int i = 0; i < 18; i++)
            {
                float t = i / 17f;
                int bandH = (int)(H * 0.22f / 18f) + 2;
                int y = (int)(H - H * 0.22f * (1f - t));
                sb.Draw(px, new Rectangle(0, y, (int)W, bandH),
                    new Color(110, 22, 185) * (0.03f + 0.055f * t) * flick * intensity);
            }
        }
    }
}

