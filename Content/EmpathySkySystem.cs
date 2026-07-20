using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.ModLoader;
using Terraria.GameContent;
using Terraria.Graphics.Effects;

namespace flameout.Content
{
    
    public class EmpathySkySystem : ModSystem
    {
        public override void Load()
        {
            if (Main.dedServ) return;
            SkyManager.Instance["flameout:EmpathySky"] = new EmpathySky();
        }
    }

    public class EmpathySky : CustomSky
    {
        private bool _active;
        private float _opacity;

        
        private struct Cube { public Vector2 Pos; public float Vel; public float Life; public float Max; public float Size; }
        private readonly List<Cube> _cubes = new();
        private readonly List<Cube> _embers = new();
        private readonly List<Cube> _orbs = new(); 
        private int _spawnTick;

        
        private struct Star
        {
            public Vector2 Norm;
            public float Size;
            public float Phase;
            public float Speed;
            public float Depth;
            public bool BigStar;
        }
        private readonly List<Star> _stars = new();

        
        private struct FallingStar { public Vector2 Pos; public Vector2 Vel; public float Life; public float Max; }
        private readonly List<FallingStar> _falling = new();

        public override void Activate(Vector2 position, params object[] args)
        {
            _active = true;
            GenerateStars();
        }

        public override void Deactivate(params object[] args) => _active = false;
        public override void Reset() => _active = false;
        public override bool IsActive() => _active || _opacity > 0.001f;

        private void GenerateStars()
        {
            _stars.Clear();

            for (int i = 0; i < 160; i++)
            {
                _stars.Add(new Star
                {
                    Norm = new Vector2(Main.rand.NextFloat(), Main.rand.NextFloat(0f, 0.75f)),
                    Size = Main.rand.NextFloat(1.5f, 3.5f),
                    Phase = Main.rand.NextFloat(MathHelper.TwoPi),
                    Speed = Main.rand.NextFloat(1.5f, 4f),
                    Depth = Main.rand.NextBool() ? 0.3f : 0.7f,
                    BigStar = false,
                });
            }

            for (int i = 0; i < 14; i++)
            {
                _stars.Add(new Star
                {
                    Norm = new Vector2(Main.rand.NextFloat(), Main.rand.NextFloat(0f, 0.6f)),
                    Size = Main.rand.NextFloat(4f, 7f),
                    Phase = Main.rand.NextFloat(MathHelper.TwoPi),
                    Speed = Main.rand.NextFloat(0.8f, 1.8f),
                    Depth = 0.5f,
                    BigStar = true,
                });
            }
        }

        public override void Update(GameTime gameTime)
        {
            _opacity = MathHelper.Clamp(_opacity + (_active ? 0.02f : -0.02f), 0f, 1f);
            if (_opacity <= 0f)
            {
                _cubes.Clear();
                _embers.Clear();
                _orbs.Clear();
                _falling.Clear();
                return;
            }

            
            _spawnTick++;
            if (_spawnTick >= 2)
            {
                _spawnTick = 0;
                for (int i = 0; i < 3; i++)
                {
                    _cubes.Add(new Cube
                    {
                        Pos = new Vector2(Main.rand.NextFloat(0f, Main.screenWidth), Main.screenHeight + Main.rand.NextFloat(0f, 40f)),
                        Vel = Main.rand.NextFloat(1.2f, 2.6f),
                        Life = 0f,
                        Max = Main.rand.NextFloat(260f, 440f),
                        Size = Main.rand.NextFloat(6f, 16f)
                    });
                }
                for (int i = 0; i < 5; i++)
                {
                    _embers.Add(new Cube
                    {
                        Pos = new Vector2(Main.rand.NextFloat(0f, Main.screenWidth), Main.screenHeight + Main.rand.NextFloat(0f, 20f)),
                        Vel = Main.rand.NextFloat(3f, 6f),
                        Life = 0f,
                        Max = Main.rand.NextFloat(140f, 260f),
                        Size = Main.rand.NextFloat(1.5f, 4f)
                    });
                }
            }

            
            if (Main.rand.NextBool(40))
            {
                _orbs.Add(new Cube
                {
                    Pos = new Vector2(
                        Main.rand.NextBool() ? -30f : Main.screenWidth + 30f, 
                        Main.rand.NextFloat(Main.screenHeight * 0.25f, Main.screenHeight * 0.6f)),
                    Vel = Main.rand.NextFloat(0.3f, 0.9f),
                    Life = 0f,
                    Max = Main.rand.NextFloat(400f, 700f),
                    Size = Main.rand.NextFloat(8f, 20f)
                });
            }

            UpdateCubes(_cubes, 0.6f);
            UpdateCubes(_embers, 1.4f);

            
            for (int i = _orbs.Count - 1; i >= 0; i--)
            {
                Cube o = _orbs[i];
                float dir = o.Pos.X < Main.screenWidth / 2f && o.Life < 1f ? 1f : (o.Pos.X > Main.screenWidth ? -1f : 1f);
                
                o.Pos.X += (o.Pos.X <= 0f || (o.Life > 0f && o.Vel > 0f && _orbs[i].Pos.X < Main.screenWidth + 40f) ? 1f : -1f) * o.Vel;
                o.Pos.Y += (float)Math.Sin((o.Life + i * 13f) * 0.03f) * 0.5f;
                o.Life++;
                _orbs[i] = o;
                if (o.Life >= o.Max || o.Pos.X < -60f || o.Pos.X > Main.screenWidth + 60f)
                    _orbs.RemoveAt(i);
            }

            
            if (Main.rand.NextBool(160))
            {
                _falling.Add(new FallingStar
                {
                    Pos = new Vector2(Main.rand.NextFloat(0f, Main.screenWidth), Main.rand.NextFloat(0f, Main.screenHeight * 0.35f)),
                    Vel = new Vector2(Main.rand.NextFloat(5f, 9f) * (Main.rand.NextBool() ? 1f : -1f), Main.rand.NextFloat(2.5f, 5f)),
                    Life = 0f,
                    Max = Main.rand.NextFloat(40f, 70f),
                });
            }

            for (int i = _falling.Count - 1; i >= 0; i--)
            {
                FallingStar f = _falling[i];
                f.Pos += f.Vel;
                f.Life++;
                _falling[i] = f;
                if (f.Life >= f.Max) _falling.RemoveAt(i);
            }
        }

        private static void UpdateCubes(List<Cube> list, float sway)
        {
            for (int i = list.Count - 1; i >= 0; i--)
            {
                Cube c = list[i];
                c.Pos.Y -= c.Vel;
                c.Pos.X += (float)Math.Sin((c.Life + i) * 0.1f) * sway;
                c.Life++;
                list[i] = c;
                if (c.Life >= c.Max) list.RemoveAt(i);
            }
        }

        public override void Draw(SpriteBatch sb, float minDepth, float maxDepth)
        {
            if (!(maxDepth >= 0f && minDepth < 0f)) return;
            if (_opacity <= 0f) return;

            Texture2D pixel = TextureAssets.MagicPixel.Value;
            float time = Main.GlobalTimeWrappedHourly;
            int w = Main.screenWidth, h = Main.screenHeight;
            var src = new Rectangle(0, 0, 1, 1);

            
            const int STRIPS = 40;
            Color top = new(255, 222, 240);
            Color bottom = new(244, 136, 188);
            int stripH = h / STRIPS + 1;
            for (int i = 0; i < STRIPS; i++)
            {
                Color c = Color.Lerp(top, bottom, i / (float)(STRIPS - 1)) * _opacity;
                sb.Draw(pixel, new Rectangle(0, i * stripH, w, stripH), c);
            }

            
            foreach (Star s in _stars)
            {
                float par = 0.02f + 0.04f * s.Depth;
                float x = Mod(s.Norm.X * w - Main.screenPosition.X * par, w);
                float y = Mod(s.Norm.Y * h - Main.screenPosition.Y * par * 0.6f, h);

                float tw = 0.4f + 0.6f * (0.5f + 0.5f * (float)Math.Sin(time * s.Speed + s.Phase));
                Color col = Color.Lerp(Color.White, new Color(255, 170, 215), 1f - tw) * (tw * _opacity);

                if (s.BigStar)
                {
                    float ray = s.Size * 3f * tw;
                    sb.Draw(pixel, new Vector2(x, y), src, col * 0.6f, 0f, new Vector2(0.5f), new Vector2(1.5f, ray * 2f), SpriteEffects.None, 0f);
                    sb.Draw(pixel, new Vector2(x, y), src, col * 0.6f, 0f, new Vector2(0.5f), new Vector2(ray * 2f, 1.5f), SpriteEffects.None, 0f);
                    sb.Draw(pixel, new Vector2(x, y), src, col, MathHelper.PiOver4, new Vector2(0.5f), new Vector2(s.Size, s.Size), SpriteEffects.None, 0f);
                }
                else
                {
                    sb.Draw(pixel, new Vector2(x, y), src, col, MathHelper.PiOver4, new Vector2(0.5f), new Vector2(s.Size, s.Size), SpriteEffects.None, 0f);
                }
            }

            
            foreach (FallingStar f in _falling)
            {
                float lifeT = f.Life / f.Max;
                float fade = (float)Math.Sin(lifeT * MathHelper.Pi);
                const int TAIL = 14;
                for (int i = 0; i < TAIL; i++)
                {
                    float t = i / (float)TAIL;
                    Vector2 pos = f.Pos - f.Vel * i * 1.2f;
                    Color col = Color.Lerp(Color.White, new Color(255, 140, 200), t) * ((1f - t) * fade * _opacity * 0.9f);
                    float sz = MathHelper.Lerp(3.5f, 0.5f, t);
                    sb.Draw(pixel, pos, src, col, 0f, new Vector2(0.5f), new Vector2(sz, sz), SpriteEffects.None, 0f);
                }
            }

            
            foreach (Cube o in _orbs)
            {
                float lifeT = o.Life / o.Max;
                float fade = (float)Math.Sin(MathHelper.Clamp(lifeT, 0f, 1f) * MathHelper.Pi); 
                float pulse = 0.85f + 0.15f * (float)Math.Sin(time * 3f + o.Pos.X * 0.01f);
                for (int k = 3; k >= 0; k--)
                {
                    float sz = o.Size * (1f + k * 0.9f) * pulse;
                    Color col = new Color(255, 160, 210) * (fade * _opacity * 0.35f / (k + 1));
                    sb.Draw(pixel, o.Pos, src, col, MathHelper.PiOver4, new Vector2(0.5f), new Vector2(sz, sz), SpriteEffects.None, 0f);
                }
                
                sb.Draw(pixel, o.Pos, src, Color.White * (fade * _opacity * 0.5f), MathHelper.PiOver4,
                    new Vector2(0.5f), new Vector2(o.Size * 0.4f, o.Size * 0.4f), SpriteEffects.None, 0f);
            }

            
            int glowH = (int)(h * 0.4f);
            const int GLOW_STRIPS = 20;
            int glowStripH = glowH / GLOW_STRIPS + 1;
            for (int i = 0; i < GLOW_STRIPS; i++)
            {
                float t = 1f - i / (float)(GLOW_STRIPS - 1);
                float flicker = 0.85f + 0.15f * (float)Math.Sin(time * 7f + i * 0.9f);
                Color c = new Color(255, 90, 160) * (t * t * 0.5f * flicker * _opacity);
                int y = h - glowH + i * glowStripH;
                sb.Draw(pixel, new Rectangle(0, y, w, glowStripH), c);
            }

            
            foreach (Cube c in _cubes)
            {
                float t = c.Life / c.Max;
                float a = (1f - t) * _opacity;
                Color col = Color.Lerp(new Color(255, 70, 150), new Color(255, 180, 210), t) * a;
                float sz = c.Size * (1f - t * 0.5f);
                sb.Draw(pixel, c.Pos, src, col, 0f, new Vector2(0.5f), new Vector2(sz, sz), SpriteEffects.None, 0f);
            }

            
            foreach (Cube c in _embers)
            {
                float t = c.Life / c.Max;
                float a = (1f - t) * _opacity;
                Color col = Color.Lerp(new Color(255, 230, 245), new Color(255, 110, 180), t) * a;
                sb.Draw(pixel, c.Pos, src, col, MathHelper.PiOver4, new Vector2(0.5f), new Vector2(c.Size, c.Size), SpriteEffects.None, 0f);
            }
        }

        private static float Mod(float v, float m) => ((v % m) + m) % m;

        public override Color OnTileColor(Color inColor) => inColor;

        public override float GetCloudAlpha() => 1f - _opacity;
    }
}