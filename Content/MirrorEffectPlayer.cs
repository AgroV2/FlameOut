using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.ModLoader;

namespace flameout.Content
{
    public class MirrorEffectPlayer : ModPlayer
    {
        private bool  _active      = false;
        private int   _timer       = 0;
        private float _savedMusic  = -1f;
        public  float Darkness     = 0f; 

        private const int EffectDuration = 18000; 
        private const int FadeInTicks    = 180;   
        private const int FadeOutTicks   = 180;

        public void ActivateEffect()
        {
            if (_active) return;
            _active      = true;
            _timer       = 0;
            _savedMusic  = Main.musicVolume;
            Main.musicVolume = 0f;
        }

        public override void PostUpdate()
        {
            if (!_active)
            {
                Darkness = 0f;
                return;
            }

            _timer++;

            if (_timer <= FadeInTicks)
                Darkness = _timer / (float)FadeInTicks;
            else if (_timer >= EffectDuration - FadeOutTicks)
                Darkness = 1f - (_timer - (EffectDuration - FadeOutTicks)) / (float)FadeOutTicks;
            else
                Darkness = 1f;

            if (_timer >= EffectDuration)
            {
                _active  = false;
                _timer   = 0;
                Darkness = 0f;

                if (_savedMusic >= 0f)
                {
                    Main.musicVolume = _savedMusic;
                    _savedMusic = -1f;
                }
            }
        }
    }
}