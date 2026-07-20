using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.Audio;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.ModLoader;

namespace flameout.Content
{
    
    public class GlobalEvent3 : HorrorEvent
    {
        public override string Name => "global_event3";
        public override HorrorEventCategory Category => HorrorEventCategory.Global;

        public override void Trigger()
        {
            AudioDecaySystem.Start();
        }
    }

    
    public class AudioDecaySystem : ModSystem
    {
        private enum Phase { Inactive, Decay, Silence, Scare, Recovery }

        private const int DECAY_TIME = 60 * 60;    
        private const int SILENCE_TIME = 60 * 10;  
        private const int SCARE_TIME = 90;         
        private const int RECOVERY_TIME = 60 * 5;  
        private const int FACE_FRAMES = 4;         

        private static Phase _phase = Phase.Inactive;
        private static int _timer;
        private static int _failsafe;
        private static int _nextWhisper;
        private static int _nextKnock;

        
        private static float _savedMusic = -1f;
        private static float _savedSound = -1f;
        private static float _savedAmbient = -1f;
        private static float _savedPitch;

        private static float _shake;

        public static bool Active => _phase != Phase.Inactive;

        
        private static readonly SoundStyle Whisper = new SoundStyle("flameout/Content/Sounds/whisper")
            { Volume = 0.7f, PitchVariance = 0.3f, MaxInstances = 1 };
        private static readonly SoundStyle Knock = new SoundStyle("flameout/Content/Sounds/knock")
            { Volume = 0.5f, PitchVariance = 0.2f, MaxInstances = 1 };
        private static readonly SoundStyle Scream = new SoundStyle("flameout/Content/Sounds/scare")
            { Volume = 1f, MaxInstances = 1 };

        public static void Start()
        {
            if (_phase != Phase.Inactive || Main.netMode == NetmodeID.Server || Main.dedServ)
                return;

            _phase = Phase.Decay;
            _timer = 0;
            _failsafe = 0;
            _shake = 0f;
            _savedMusic = Main.musicVolume;
            _savedSound = Main.soundVolume;
            _savedAmbient = Main.ambientVolume;
            _savedPitch = Main.musicPitch;
            ScheduleWhisper();
            ScheduleKnock();
        }

        private static void ScheduleWhisper() => _nextWhisper = _timer + Main.rand.Next(500, 1000);
        private static void ScheduleKnock() => _nextKnock = _timer + Main.rand.Next(300, 700);

        private static void ForceEnd()
        {
            if (_savedMusic >= 0f)
            {
                Main.musicVolume = _savedMusic;
                Main.soundVolume = _savedSound;
                Main.ambientVolume = _savedAmbient;
                Main.musicPitch = _savedPitch;
            }
            _savedMusic = -1f;
            _shake = 0f;
            _phase = Phase.Inactive;
        }

        public override void OnWorldUnload() => ForceEnd(); 

        public override void PostUpdateEverything()
        {
            if (!Active)
                return;

            
            if (++_failsafe > 90 * 60)
            {
                ForceEnd();
                return;
            }

            _timer++;
            Player player = Main.LocalPlayer;

            switch (_phase)
            {
                case Phase.Decay:
                {
                    float progress = (float)_timer / DECAY_TIME;

                    
                    Main.musicPitch = _savedPitch - 0.75f * progress;
                    float wobble = 0.1f * (float)Math.Sin(_timer * 0.01f) * progress;
                    Main.musicVolume = _savedMusic * (1f - 0.6f * progress + wobble);

                    
                    if (_timer >= _nextWhisper)
                    {
                        Vector2 offset = Main.rand.NextVector2Unit() * Main.rand.NextFloat(300f, 700f);
                        SoundEngine.PlaySound(Whisper, player.Center + offset);
                        ScheduleWhisper();
                    }

                    
                    if (_timer >= _nextKnock)
                    {
                        float dist = MathHelper.Lerp(900f, 200f, progress);
                        Vector2 offset = Main.rand.NextVector2Unit() * dist;
                        SoundEngine.PlaySound(Knock, player.Center + offset);
                        ScheduleKnock();
                    }

                    if (_timer >= DECAY_TIME)
                    {
                        _phase = Phase.Silence;
                        _timer = 0;
                    }
                    break;
                }

                case Phase.Silence:
                    
                    Main.musicVolume = 0f;
                    Main.soundVolume = 0f;
                    Main.ambientVolume = 0f;

                    if (_timer >= SILENCE_TIME)
                    {
                        _phase = Phase.Scare;
                        _timer = 0;
                        
                        Main.soundVolume = Math.Max(_savedSound, 0.8f);
                        SoundEngine.PlaySound(Scream, player.Center); 
                        _shake = 20f;
                    }
                    break;

                case Phase.Scare:
                    _shake = MathHelper.Lerp(_shake, 0f, 0.08f);
                    if (_timer >= SCARE_TIME)
                    {
                        _phase = Phase.Recovery;
                        _timer = 0;
                    }
                    break;

                case Phase.Recovery:
                {
                    
                    float t = (float)_timer / RECOVERY_TIME;
                    Main.musicVolume = MathHelper.Lerp(0f, _savedMusic, t);
                    Main.soundVolume = MathHelper.Lerp(Main.soundVolume, _savedSound, t);
                    Main.ambientVolume = MathHelper.Lerp(0f, _savedAmbient, t);
                    Main.musicPitch = MathHelper.Lerp(Main.musicPitch, _savedPitch, t);

                    if (_timer >= RECOVERY_TIME)
                        ForceEnd();
                    break;
                }
            }
        }

        
        public override void ModifyScreenPosition()
        {
            if (_phase == Phase.Scare && _shake > 0.5f)
                Main.screenPosition += new Vector2(
                    Main.rand.NextFloat(-_shake, _shake),
                    Main.rand.NextFloat(-_shake, _shake));
        }

        public override void PostDrawInterface(SpriteBatch sb)
        {
            if (_phase != Phase.Scare)
                return;

            
            if (_timer <= FACE_FRAMES)
            {
                Texture2D face = ModContent.Request<Texture2D>("flameout/Content/EntityFace").Value;
                float scale = Math.Max(
                    (float)Main.screenWidth / face.Width,
                    (float)Main.screenHeight / face.Height);
                sb.Draw(face,
                    new Vector2(Main.screenWidth / 2f, Main.screenHeight / 2f),
                    null, Color.White, 0f,
                    new Vector2(face.Width / 2f, face.Height / 2f),
                    scale, SpriteEffects.None, 0f);
            }
            
            else if (_timer <= 40)
            {
                float fade = 1f - (_timer - FACE_FRAMES) / 36f;
                sb.Draw(TextureAssets.MagicPixel.Value,
                    new Rectangle(0, 0, Main.screenWidth, Main.screenHeight),
                    new Color(150, 0, 20) * (0.5f * fade));
            }
        }
    }
}