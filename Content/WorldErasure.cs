using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.Audio;
using Terraria.DataStructures;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.ModLoader;

namespace flameout.Content
{
    
    public class GlobalEvent5 : HorrorEvent
    {
        public override string Name => "global_event5";
        public override HorrorEventCategory Category => HorrorEventCategory.Global;

        public override void Trigger()
        {
            WorldErasureSystem.Start();
        }
    }

    
    public class WorldErasureSystem : ModSystem
    {
        private enum Phase { Inactive, Erasing, Collapse, Flash }

        private const int CHUNK = 128;              
        private const int RADIUS_START = 70;        
        private const int RADIUS_MIN = 2;           
        private const int COLLAPSE_TIME = 150;      
        private const int FLASH_TIME = 90;          

        private static Phase _phase = Phase.Inactive;
        private static int _timer;
        private static int _failsafe;
        private static float _radius;               
        private static float _shrinkSpeed;
        private static float _whiteFlash;
        private static float _shake;

        public static bool Active => _phase != Phase.Inactive;

        private static readonly SoundStyle Glitch = new SoundStyle("flameout/Content/Sounds/glitch1")
            { Volume = 0.6f, PitchVariance = 0.3f, MaxInstances = 2 };

        public static void Start()
        {
            if (_phase != Phase.Inactive || Main.netMode == NetmodeID.Server || Main.dedServ)
                return;

            _phase = Phase.Erasing;
            _timer = 0;
            _failsafe = 0;
            _radius = RADIUS_START;
            _shrinkSpeed = 0.02f;
            _whiteFlash = 0f;
            _shake = 0f;
            SoundEngine.PlaySound(Glitch);
        }

        private static void ForceEnd()
        {
            _phase = Phase.Inactive;
            _whiteFlash = 0f;
            _shake = 0f;
        }

        public override void OnWorldUnload() => ForceEnd();

        public override void PostUpdateEverything()
        {
            if (!Active)
                return;

            if (++_failsafe > 120 * 60) 
            {
                ForceEnd();
                return;
            }

            _timer++;
            Player player = Main.LocalPlayer;

            switch (_phase)
            {
                case Phase.Erasing:
                    
                    _shrinkSpeed = Math.Min(_shrinkSpeed + 0.00035f, 0.16f);
                    float oldRadius = _radius;
                    _radius -= _shrinkSpeed;

                    
                    if ((int)oldRadius != (int)_radius)
                    {
                        float closeness = 1f - _radius / RADIUS_START;
                        SoundEngine.PlaySound(Glitch with
                        {
                            Volume = 0.3f + 0.5f * closeness,
                            Pitch = -0.4f + 0.8f * closeness
                        });
                    }

                    
                    _shake = MathHelper.Lerp(0f, 6f, 1f - _radius / RADIUS_START);

                    if (_radius <= RADIUS_MIN)
                    {
                        _radius = RADIUS_MIN;
                        _phase = Phase.Collapse;
                        _timer = 0;
                    }
                    break;

                case Phase.Collapse:
                    
                    _shake = MathHelper.Lerp(6f, 22f, (float)_timer / COLLAPSE_TIME);
                    _radius = MathHelper.Lerp(RADIUS_MIN, 0.3f, (float)_timer / COLLAPSE_TIME);

                    if (_timer >= COLLAPSE_TIME)
                    {
                        _phase = Phase.Flash;
                        _timer = 0;
                        _whiteFlash = 1f;
                        _shake = 0f;
                        SoundEngine.PlaySound(SoundID.Item122 with { Volume = 1f, Pitch = -0.5f });

                        
                        player.Spawn(PlayerSpawnContext.RecallFromItem);
                        player.velocity = Vector2.Zero;
                    }
                    break;

                case Phase.Flash:
                    
                    _radius = RADIUS_START * 2f;
                    _whiteFlash = Math.Max(0f, 1f - (float)_timer / FLASH_TIME);

                    if (_timer >= FLASH_TIME)
                        ForceEnd();
                    break;
            }
        }

        public override void ModifyScreenPosition()
        {
            if (Active && _shake > 0.5f)
                Main.screenPosition += new Vector2(
                    Main.rand.NextFloat(-_shake, _shake),
                    Main.rand.NextFloat(-_shake, _shake));
        }

        
        private static int Hash(int x, int y, int t)
        {
            unchecked
            {
                int h = x * 374761393 + y * 668265263 + t * 1274126177;
                h = (h ^ (h >> 13)) * 1103515245;
                return h ^ (h >> 16);
            }
        }

        
        public override void PostDrawTiles()
        {
            if (!Active || _phase == Phase.Flash)
                return;

            SpriteBatch sb = Main.spriteBatch;
            sb.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointClamp,
                DepthStencilState.None, Main.Rasterizer, null, Main.GameViewMatrix.TransformationMatrix);

            Texture2D pixel = TextureAssets.MagicPixel.Value;
            Vector2 center = Main.LocalPlayer.Center;
            int anchorX = (int)(center.X / CHUNK);
            int anchorY = (int)(center.Y / CHUNK);

            
            int left = (int)(Main.screenPosition.X / CHUNK) - 1;
            int top = (int)(Main.screenPosition.Y / CHUNK) - 1;
            int right = (int)((Main.screenPosition.X + Main.screenWidth) / CHUNK) + 1;
            int bottom = (int)((Main.screenPosition.Y + Main.screenHeight) / CHUNK) + 1;

            int noiseTime = (int)(Main.GameUpdateCount / 3); 

            for (int cx = left; cx <= right; cx++)
            {
                for (int cy = top; cy <= bottom; cy++)
                {
                    
                    int dist = Math.Max(Math.Abs(cx - anchorX), Math.Abs(cy - anchorY));
                    if (dist <= _radius)
                        continue;

                    Rectangle rect = new Rectangle(
                        (int)(cx * CHUNK - Main.screenPosition.X),
                        (int)(cy * CHUNK - Main.screenPosition.Y),
                        CHUNK, CHUNK);

                    
                    sb.Draw(pixel, rect, Color.Black);

                    int h = Hash(cx, cy, noiseTime);

                    
                    for (int i = 0; i < 3; i++)
                    {
                        int hh = Hash(cx + i * 17, cy - i * 29, noiseTime);
                        int stripY = rect.Y + Math.Abs(hh % CHUNK);
                        int stripH = 1 + Math.Abs((hh >> 8) % 3);
                        float bright = 0.04f + (Math.Abs((hh >> 16) % 10)) * 0.012f;
                        sb.Draw(pixel, new Rectangle(rect.X, stripY, CHUNK, stripH),
                            Color.White * bright);
                    }

                    
                    if (dist <= _radius + 1.5f && (h & 3) == 0)
                    {
                        Color edge = ((h >> 4) & 1) == 0
                            ? new Color(255, 40, 70, 0)
                            : new Color(255, 100, 180, 0);
                        int stripY = rect.Y + Math.Abs((h >> 6) % CHUNK);
                        sb.Draw(pixel, new Rectangle(rect.X, stripY, CHUNK, 2 + (h & 7)),
                            edge * 0.35f);
                    }
                }
            }

            sb.End();
        }

        
        public override void PostDrawInterface(SpriteBatch sb)
        {
            if (_whiteFlash > 0f)
                sb.Draw(TextureAssets.MagicPixel.Value,
                    new Rectangle(0, 0, Main.screenWidth, Main.screenHeight),
                    Color.White * _whiteFlash);
        }
    }
}