using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using ReLogic.Graphics;
using Terraria;
using Terraria.Audio;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.ModLoader;

namespace flameout.Content
{
    
    public class GlobalEvent4 : HorrorEvent
    {
        public override string Name => "global_event4";
        public override HorrorEventCategory Category => HorrorEventCategory.Global;

        public override void Trigger()
        {
            FalseMenuSystem.Start();
        }
    }

    
    public class FalseMenuSystem : ModSystem
    {
        private enum Phase { Inactive, FadeOut, Menu, Loading, FadeIn }

        private static Phase _phase = Phase.Inactive;
        private static int _timer;
        private static int _failsafe;
        private static float _black;              
        private static float _savedMusicVolume = -1f;
        private static float _loadProgress;
        private static int _hovered = -1;
        private static int _prevHovered = -1;

        public static bool Active => _phase != Phase.Inactive;

        
        private static readonly string[] Buttons = { "Stay Player", "Multiplayer", "Achievements", "Settings" };

        public static void Start()
        {
            if (_phase != Phase.Inactive || Main.netMode == NetmodeID.Server || Main.dedServ)
                return;

            _phase = Phase.FadeOut;
            _timer = 0;
            _failsafe = 0;
            _black = 0f;
            _loadProgress = 0f;
            _savedMusicVolume = Main.musicVolume;
            SoundEngine.PlaySound(SoundID.MenuClose); 
        }

        private static void ForceEnd()
        {
            if (_savedMusicVolume >= 0f)
                Main.musicVolume = _savedMusicVolume;
            _savedMusicVolume = -1f;
            _phase = Phase.Inactive;
        }

        public override void OnWorldUnload() => ForceEnd(); 

        public override void PostUpdateEverything()
        {
            if (!Active)
                return;

            
            if (++_failsafe > 60 * 60)
            {
                ForceEnd();
                return;
            }

            
            Main.playerInventory = false;
            Main.ingameOptionsWindow = false;
            Main.LocalPlayer.mouseInterface = true;

            _timer++;

            switch (_phase)
            {
                case Phase.FadeOut:
                    _black = Math.Min(1f, _black + 1f / 45f);
                    Main.musicVolume = _savedMusicVolume * (1f - _black); 
                    if (_black >= 1f && _timer > 60)
                    {
                        _phase = Phase.Menu;
                        _timer = 0;
                    }
                    break;

                case Phase.Menu:
                    UpdateMenu();
                    break;

                case Phase.Loading:
                    
                    if (Main.rand.NextBool(4))
                        _loadProgress += Main.rand.NextFloat(0.004f, 0.03f);
                    if (_loadProgress >= 1f && _timer > 180)
                    {
                        _phase = Phase.FadeIn;
                        _timer = 0;
                        SoundEngine.PlaySound(SoundID.MenuOpen);
                    }
                    break;

                case Phase.FadeIn:
                    _black = Math.Max(0f, _black - 1f / 30f);
                    Main.musicVolume = _savedMusicVolume * (1f - _black); 
                    if (_black <= 0f)
                        ForceEnd();
                    break;
            }
        }

        private static void UpdateMenu()
        {
            _prevHovered = _hovered;
            _hovered = -1;

            DynamicSpriteFont font = FontAssets.DeathText.Value;
            Vector2 basePos = new Vector2(Main.screenWidth / 2f, Main.screenHeight * 0.42f);

            for (int i = 0; i < Buttons.Length; i++)
            {
                Vector2 size = font.MeasureString(Buttons[i]);
                Vector2 pos = basePos + new Vector2(0, i * 55f);
                Rectangle hitbox = new Rectangle(
                    (int)(pos.X - size.X / 2f), (int)(pos.Y - size.Y / 2f),
                    (int)size.X, (int)size.Y);

                if (hitbox.Contains(Main.mouseX, Main.mouseY))
                    _hovered = i;
            }

            if (_hovered != -1 && _hovered != _prevHovered)
                SoundEngine.PlaySound(SoundID.MenuTick);

            
            if (_hovered != -1 && Main.mouseLeft && Main.mouseLeftRelease)
            {
                SoundEngine.PlaySound(SoundID.MenuOpen);
                _phase = Phase.Loading;
                _timer = 0;
                _loadProgress = 0f;
            }
        }

        public override void PostDrawInterface(SpriteBatch sb)
        {
            if (!Active)
                return;

            Texture2D pixel = TextureAssets.MagicPixel.Value;
            Rectangle screen = new Rectangle(0, 0, Main.screenWidth, Main.screenHeight);

            
            sb.Draw(pixel, screen, Color.Black * _black);

            if (_phase == Phase.Menu)
                DrawMenu(sb);
            else if (_phase == Phase.Loading)
                DrawLoading(sb, pixel);
        }

        private static void DrawMenu(SpriteBatch sb)
        {
            
            if (Main.rand.NextBool(90))
                sb.Draw(TextureAssets.MagicPixel.Value,
                    new Rectangle(0, 0, Main.screenWidth, Main.screenHeight),
                    new Color(120, 0, 20) * 0.15f);

            
            Texture2D logo = TextureAssets.Logo.Value;
            Vector2 logoPos = new Vector2(Main.screenWidth / 2f, Main.screenHeight * 0.16f);
            bool glitching = Main.rand.NextBool(20);
            int strips = 8;
            int stripH = logo.Height / strips;
            for (int i = 0; i < strips; i++)
            {
                float xOff = glitching ? Main.rand.NextFloat(-25f, 25f) : 0f;
                Color tint = glitching && Main.rand.NextBool()
                    ? new Color(255, 60, 90)
                    : Color.White;
                sb.Draw(logo,
                    logoPos + new Vector2(xOff, (i - strips / 2f) * stripH),
                    new Rectangle(0, i * stripH, logo.Width, stripH),
                    tint, 0f, new Vector2(logo.Width / 2f, 0f), 1f, SpriteEffects.None, 0f);
            }

            
            DynamicSpriteFont font = FontAssets.DeathText.Value;
            Vector2 basePos = new Vector2(Main.screenWidth / 2f, Main.screenHeight * 0.42f);
            for (int i = 0; i < Buttons.Length; i++)
            {
                string text = Buttons[i];
                
                if (Main.rand.NextBool(240))
                {
                    char[] chars = text.ToCharArray();
                    chars[Main.rand.Next(chars.Length)] = (char)Main.rand.Next(0x2591, 0x2594); 
                    text = new string(chars);
                }

                bool hover = _hovered == i;
                float scale = hover ? 1.12f : 1f;
                Color col = hover ? new Color(255, 199, 0) : new Color(160, 160, 160);
                Vector2 pos = basePos + new Vector2(0, i * 55f);
                Vector2 size = font.MeasureString(Buttons[i]);
                sb.DrawString(font, text, pos - size * scale / 2f, col, 0f,
                    Vector2.Zero, scale, SpriteEffects.None, 0f);
            }

            
            string ver = "Terraria v1.4.4.9  |  he is still here";
            sb.DrawString(FontAssets.MouseText.Value, ver,
                new Vector2(14f, Main.screenHeight - 30f), new Color(90, 90, 90));
        }

        private static void DrawLoading(SpriteBatch sb, Texture2D pixel)
        {
            DynamicSpriteFont font = FontAssets.DeathText.Value;

            
            string text = "Loading world...";
            if (_loadProgress > 0.6f && Main.rand.NextBool(30))
                text = "Loading you...";

            Vector2 size = font.MeasureString(text);
            sb.DrawString(font, text,
                new Vector2(Main.screenWidth / 2f - size.X / 2f, Main.screenHeight * 0.42f),
                Color.White);

            
            int barW = 400, barH = 20;
            Rectangle bar = new Rectangle(Main.screenWidth / 2 - barW / 2,
                (int)(Main.screenHeight * 0.52f), barW, barH);
            sb.Draw(pixel, new Rectangle(bar.X - 2, bar.Y - 2, bar.Width + 4, bar.Height + 4),
                new Color(70, 70, 70));
            sb.Draw(pixel, bar, Color.Black);
            sb.Draw(pixel, new Rectangle(bar.X, bar.Y,
                (int)(bar.Width * Math.Min(_loadProgress, 1f)), bar.Height),
                new Color(255, 60, 100));
        }
    }

    
    public class FalseMenuPlayer : ModPlayer
    {
        public override void SetControls()
        {
            if (!FalseMenuSystem.Active)
                return;

            Player.controlUp = Player.controlDown = Player.controlLeft = Player.controlRight = false;
            Player.controlJump = Player.controlUseItem = Player.controlUseTile = false;
            Player.controlHook = Player.controlMount = Player.controlThrow = false;
            Player.controlInv = Player.controlMap = false;
        }
    }
}