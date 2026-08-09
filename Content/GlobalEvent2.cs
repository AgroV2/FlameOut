

using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework.Graphics;
using ReLogic.Content;
using Terraria;
using Terraria.Audio;
using Terraria.GameContent;
using Terraria.ModLoader;

namespace flameout.Content
{
    public class GlobalEvent2 : HorrorEvent
    {
        public override string Name => "global_event2";
        public override HorrorEventCategory Category => HorrorEventCategory.Global;

        public override void Trigger()
        {
            
            if (Main.dedServ)
                return;

            
            SoundEngine.PlaySound(new SoundStyle("flameout/Content/Sounds/glitch1"));

            
            SpriteShuffler.ShuffleAll();

            
            GlitchQuitSystem.StartCountdown(600); 
        }
    }

    
    public static class SpriteShuffler
    {
        
        
        
        
        
        
        private const bool PreloadEverything = true;

        private static bool _shuffled;

        
        private static Asset<Texture2D>[] _tileBackup;
        private static Asset<Texture2D>[] _itemBackup;
        private static Asset<Texture2D>[] _npcBackup;
        private static Asset<Texture2D>[] _projectileBackup;
        private static Asset<Texture2D>[] _goreBackup;

        public static bool IsShuffled => _shuffled;

        public static void ShuffleAll()
        {
            
            
            if (Main.dedServ || _shuffled)
                return;

            if (PreloadEverything)
                PreloadAll();

            
            _tileBackup = Snapshot(TextureAssets.Tile);
            _itemBackup = Snapshot(TextureAssets.Item);
            _npcBackup = Snapshot(TextureAssets.Npc);
            _projectileBackup = Snapshot(TextureAssets.Projectile);
            _goreBackup = Snapshot(TextureAssets.Gore);

            
            var rand = new Random();
            Shuffle(TextureAssets.Tile, rand);
            Shuffle(TextureAssets.Item, rand);
            Shuffle(TextureAssets.Npc, rand);
            Shuffle(TextureAssets.Projectile, rand);
            Shuffle(TextureAssets.Gore, rand);
            
            

            _shuffled = true;
        }

        
        
        
        public static void RestoreAll()
        {
            if (!_shuffled)
                return;

            Restore(TextureAssets.Tile, _tileBackup);
            Restore(TextureAssets.Item, _itemBackup);
            Restore(TextureAssets.Npc, _npcBackup);
            Restore(TextureAssets.Projectile, _projectileBackup);
            Restore(TextureAssets.Gore, _goreBackup);

            _tileBackup = null;
            _itemBackup = null;
            _npcBackup = null;
            _projectileBackup = null;
            _goreBackup = null;

            _shuffled = false;
        }

        private static Asset<Texture2D>[] Snapshot(Asset<Texture2D>[] arr)
        {
            return arr == null ? null : (Asset<Texture2D>[])arr.Clone();
        }

        private static void Restore(Asset<Texture2D>[] target, Asset<Texture2D>[] backup)
        {
            if (target == null || backup == null)
                return;

            
            
            Array.Copy(backup, target, Math.Min(target.Length, backup.Length));
        }

        private static void Shuffle(Asset<Texture2D>[] arr, Random rand)
        {
            if (arr == null)
                return;

            
            
            
            var indices = new List<int>();
            for (int i = 0; i < arr.Length; i++)
            {
                if (arr[i] != null && arr[i].IsLoaded)
                    indices.Add(i);
            }

            
            for (int k = indices.Count - 1; k > 0; k--)
            {
                int m = rand.Next(k + 1);
                int a = indices[k];
                int b = indices[m];
                (arr[a], arr[b]) = (arr[b], arr[a]);
            }
        }

        private static void PreloadAll()
        {
            var main = Main.instance;
            if (main == null)
                return;

            PreloadRange(TextureAssets.Item, main.LoadItem);
            PreloadRange(TextureAssets.Npc, main.LoadNPC);
            PreloadRange(TextureAssets.Projectile, main.LoadProjectile);
            PreloadRange(TextureAssets.Gore, main.LoadGore);
            PreloadRange(TextureAssets.Tile, main.LoadTiles);
        }

        private static void PreloadRange(Asset<Texture2D>[] arr, Action<int> loader)
        {
            if (arr == null)
                return;

            for (int i = 0; i < arr.Length; i++)
            {
                try
                {
                    loader(i);
                }
                catch
                {
                    
                    
                }
            }
        }
    }

    
    public class GlitchQuitSystem : ModSystem
    {
        private static int _timer = -1;

        public static bool IsCounting => _timer >= 0;

        public static void StartCountdown(int ticks)
        {
            if (Main.dedServ)
                return;

            _timer = ticks;
        }

        public static void CancelCountdown()
        {
            _timer = -1;
        }

        public override void PostUpdateEverything()
        {
            if (Main.dedServ || _timer < 0)
                return;

            _timer--;
            if (_timer > 0)
                return;

            _timer = -1;

            
            
            SpriteShuffler.RestoreAll();

            
            
            WorldGen.SaveAndQuit();
        }

        
        public override void PreSaveAndQuit()
        {
            _timer = -1;
            SpriteShuffler.RestoreAll();
        }

        public override void OnWorldUnload()
        {
            _timer = -1;
            SpriteShuffler.RestoreAll();
        }

        
        
        public override void Unload()
        {
            _timer = -1;
            SpriteShuffler.RestoreAll();
        }
    }
}

