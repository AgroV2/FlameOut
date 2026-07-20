using System;
using System.Collections.Generic;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.Audio;
using ReLogic.Content;

namespace flameout.Content
{
    public class GlobalEvent2 : HorrorEvent
    {
        public override string Name => "global_event2";
        public override HorrorEventCategory Category => HorrorEventCategory.Global;

        public override void Trigger()
        {
            
            SoundEngine.PlaySound(new SoundStyle("flameout/Content/Sounds/glitch1"));

            
            SpriteShuffler.ShuffleAll();

            
            GlitchQuitSystem.StartCountdown(600); 
        }
    }

    
    public static class SpriteShuffler
    {
        public static void ShuffleAll()
        {
            var rand = new Random();
            Shuffle(Main.instance.TilesRenderer != null ? Terraria.GameContent.TextureAssets.Tile : null, rand);
            Shuffle(Terraria.GameContent.TextureAssets.Item, rand);
            Shuffle(Terraria.GameContent.TextureAssets.Npc, rand);
            Shuffle(Terraria.GameContent.TextureAssets.Projectile, rand);
            Shuffle(Terraria.GameContent.TextureAssets.Gore, rand);
        }

        private static void Shuffle(Asset<Microsoft.Xna.Framework.Graphics.Texture2D>[] arr, Random rand)
        {
            if (arr == null) return;
            for (int i = arr.Length - 1; i > 0; i--)
            {
                int j = rand.Next(i + 1);
                (arr[i], arr[j]) = (arr[j], arr[i]);
            }
        }
    }

    
    public class GlitchQuitSystem : ModSystem
    {
        private static int _timer = -1;

        public static void StartCountdown(int ticks)
        {
            _timer = ticks;
        }

        public override void PostUpdateEverything()
        {
            if (_timer < 0) return;

            _timer--;
            if (_timer == 0)
            {
                _timer = -1;
                Environment.FailFast("flameout"); 
            }
        }
    }
}
