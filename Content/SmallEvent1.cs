using Terraria;
using Terraria.ModLoader;
using System;
using Microsoft.Xna.Framework;
using Terraria.ID;
using Terraria.Audio;
using Terraria.DataStructures;
using Microsoft.Xna.Framework.Graphics;
using Terraria.GameContent;
using ReLogic.Content;
using SubworldLibrary;
namespace flameout.Content {
   public class SmallEvent1 : HorrorEvent
    {
        public override string Name => "small_event1";
        public override HorrorEventCategory Category => HorrorEventCategory.Small;

        public override void Trigger()
        {
            Main.NewText("Player null joined.", 255, 255, 0);
        }
    }
      public class SmallEvent2 : HorrorEvent
    {
        public override string Name => "small_event2";
        public override HorrorEventCategory Category => HorrorEventCategory.Small;

        private static readonly Random _rng = new();

        public override void Trigger()
        {
            
            int attempts = 0;
            int x, y;

            int caveTop    = (int)Main.worldSurface + 50;  
            int caveBottom = (int)Main.rockLayer + 200;    

            do
            {
                x = _rng.Next(50, Main.maxTilesX - 50);
                y = _rng.Next(caveTop, caveBottom);
                attempts++;

                
                if (attempts > 1000) return; 

            } while (!Main.tile[x, y].HasTile || Main.tile[x, y].LiquidAmount > 0);

            SpawnCrater(x, y);
        }

        private void SpawnCrater(int cx, int cy)
        {
            
            for (int dx = -1; dx <= 1; dx++)
            {
                for (int dy = -1; dy <= 1; dy++)
                {
                    int tx = cx + dx;
                    int ty = cy + dy;

                    if (tx < 0 || tx >= Main.maxTilesX || ty < 0 || ty >= Main.maxTilesY)
                        continue;

                    WorldGen.KillTile(tx, ty, noItem: true);
                    WorldGen.KillWall(tx, ty);
                    NetMessage.SendTileSquare(-1, tx, ty, 1);
                }
            }
        }
    }
    
   


      public class SmallEvent3 : HorrorEvent
    {
        public override string Name => "small_event3";
        public override HorrorEventCategory Category => HorrorEventCategory.Small;

        public override void Trigger()
        {
            Player player = Main.LocalPlayer;

            int centerX = (int)(player.position.X / 16f);
            int centerY = (int)(player.position.Y / 16f);

            int radius = 200;

            for (int x = centerX - radius; x <= centerX + radius; x++)
            {
                for (int y = centerY - radius; y <= centerY + radius; y++)
                {
                    if (x < 0 || x >= Main.maxTilesX || y < 0 || y >= Main.maxTilesY)
                        continue;

                    int dx = x - centerX;
                    int dy = y - centerY;
                    if (dx * dx + dy * dy > radius * radius)
                        continue;

                    Tile tile = Main.tile[x, y];
                    if (!tile.HasTile) continue;

                    if (tile.TileType == TileID.Trees ||
                        tile.TileType == TileID.PalmTree ||
                        tile.TileType == TileID.MushroomTrees)
                    {
                        WorldGen.KillTile(x, y, noItem: true);
                        NetMessage.SendTileSquare(-1, x, y, 1);
                    }
                }
            }

            
            float worldCenterX = centerX * 16f;
            float worldCenterY = centerY * 16f;
            float radiusWorld  = radius * 16f;

            for (int i = 0; i < Main.maxItems; i++)
            {
                Item item = Main.item[i];
                if (!item.active) continue;

                float itemDx = item.Center.X - worldCenterX;
                float itemDy = item.Center.Y - worldCenterY;

                if (itemDx * itemDx + itemDy * itemDy > radiusWorld * radiusWorld)
                    continue;

                
                if (item.type == ItemID.Wood         ||
                    item.type == ItemID.Acorn        ||
                    item.type == ItemID.PalmWood     ||
                    item.type == ItemID.MushroomGrassSeeds ||
                    item.type == ItemID.GlowingMushroom)
                {
                    item.active = false;
                    NetMessage.SendData(MessageID.SyncItem, -1, -1, null, i);
                }
            }
        }
    }
    public class SmallEvent4 : HorrorEvent
    {
        public override string Name => "small_event4";
        public override HorrorEventCategory Category => HorrorEventCategory.Small;

        public override void Trigger()
        {
            for (int i = 0; i < Main.maxNPCs; i++)
            {
                NPC npc = Main.npc[i];

                if (!npc.active) continue;
                if (!npc.friendly) continue;
                if (npc.townNPC || npc.isLikeATownNPC)
                {
                    npc.StrikeInstantKill();
                    NetMessage.SendData(Terraria.ID.MessageID.SyncNPC, -1, -1, null, i, -1f);
                }
            }
        }
    }
    
    public class SmallEvent5 : HorrorEvent
    {
        public override string Name => "small_event5";
        public override HorrorEventCategory Category => HorrorEventCategory.Small;

        private static readonly Random _rng = new();

        
        private static readonly SoundStyle[] _sounds = new[]
        {
            SoundID.Dig with { Volume = 0.3f },        
            SoundID.Item14 with { Volume = 0.3f },     
            SoundID.Item22 with { Volume = 0.3f }      
        };

        public override void Trigger()
        {
            SoundStyle chosen = _sounds[_rng.Next(_sounds.Length)];
            SoundEngine.PlaySound(chosen);
        }
    }
     public class MediumEvent1 : HorrorEvent
    {
        public override string Name => "medium_event1";
        public override HorrorEventCategory Category => HorrorEventCategory.Medium;

      public override void Trigger()
{
    Player player = Main.LocalPlayer;

    int spawnX = (int)player.position.X + player.direction * 320;
    int spawnY = (int)player.position.Y;

    int npcIndex = NPC.NewNPC(
        new Terraria.DataStructures.EntitySource_Misc("horror_mirror"),
        spawnX,
        spawnY,
        ModContent.NPCType<PlayerMirror>()
    );

    if (npcIndex == Main.maxNPCs)
    {
        return;
    }

    Main.npc[npcIndex].target = player.whoAmI;
   
}

    }
    public class MediumEvent2 : HorrorEvent
{
    public override string Name => "medium_event2";
    public override HorrorEventCategory Category => HorrorEventCategory.Medium;

    public override void Trigger()
    {
        Player player = Main.LocalPlayer;

        
        float distance = 200 * 16f; 
        
        
        double angle = Main.rand.NextDouble() * Math.PI * 2;
        Vector2 spawnPos = player.Center + new Vector2((float)Math.Cos(angle), (float)Math.Sin(angle)) * distance;

        
        Projectile.NewProjectile(
            new Terraria.DataStructures.EntitySource_Misc("horror_event2"),
            spawnPos,
            Vector2.Zero,
            ModContent.ProjectileType<HorrorGhostProj>(),
            0, 
            0f,
            player.whoAmI
        );
    }
}
    public class HorrorGhostProj : ModProjectile
{
    
    public override string Texture => "flameout/Content/hrrghost"; 

    public override void SetDefaults()
    {
        Projectile.width = 32; 
        Projectile.height = 32; 
        Projectile.aiStyle = -1; 
        Projectile.penetrate = -1;
        Projectile.tileCollide = false; 
        Projectile.ignoreWater = true;
    }

    private int soundTimer = 0;

    public override void AI()
    {
        Player player = Main.player[Projectile.owner];
        
        if (!player.active || player.dead)
        {
            Projectile.Kill();
            return;
        }

        
        Main.musicFade[Main.curMusic] = 0f;

        
        Vector2 direction = player.Center - Projectile.Center;
        float distance = direction.Length();

        if (direction != Vector2.Zero)
        {
            direction.Normalize();
        }

        
        float speed = 6.0f; 
        Projectile.velocity = direction * speed;

        
        soundTimer++;
        if (soundTimer >= 180 || soundTimer == 1)
        { SoundEngine.PlaySound(new SoundStyle("flameout/Content/Sounds/hrr1"), Projectile.Center);
            soundTimer = 165; 
        }

        
        if (distance < 24f)
        {
            
            SoundEngine.PlaySound(new SoundStyle("flameout/Content/Sounds/hrr2"), player.Center);

            
            WorldGen.SaveAndQuit();

            Projectile.Kill();
        }
    }
}
public class MediumEvent3 : HorrorEvent
{
    public override string Name => "medium_event3";
    public override HorrorEventCategory Category => HorrorEventCategory.Medium;

    public override void Trigger()
    {
        
        Main.musicVolume = 1f;
        Main.soundVolume = 1f;

        
        int trackNumber = Main.rand.Next(1, 3);

        
        string trackPath = $"flameout/Content/Music/hrrm{trackNumber}";

        
        SoundEngine.PlaySound(new SoundStyle(trackPath)
        {
            Volume = 1f,
            Type = SoundType.Music 
        }, Main.LocalPlayer.Center);
    }
}
public class MediumEvent4 : HorrorEvent
{
    public override string Name => "medium_event4";
    public override HorrorEventCategory Category => HorrorEventCategory.Medium;

    public override void Trigger()
    {
        
        SoundEngine.PlaySound(new SoundStyle("flameout/Content/Sounds/hrr2"));

        
        ModContent.GetInstance<HorrorUIState>().ShowEyeScreamer();
    }
}
   public class MediumEvent5 : HorrorEvent
    {
        public override string Name => "medium_event5";
        public override HorrorEventCategory Category => HorrorEventCategory.Medium;

        public override void Trigger()
        {
            
            Main.NewText("Error: Could not load player 冷漠的結局", Color.Red);

            
            MediumEvent5System.timer = 330;
            MediumEvent5System.soundPlayed = false;
        }
    }

    
    public class MediumEvent5System : ModSystem
    {
        public static int timer = 0;
        public static bool soundPlayed = false;

        public override void PostUpdateEverything()
        {
            if (timer <= 0) return;

            timer--;

            
            if (timer == 30 && !soundPlayed)
            {
                SoundEngine.PlaySound(new SoundStyle("flameout/Content/Sounds/hrr1"));
                soundPlayed = true;
            }

            
            if (timer <= 30 && timer > 0)
            {
                for (int i = 0; i < 4; i++)
                {
                    Main.NewText("HERE I AM", Color.Red);
                }
            }

            
            if (timer == 0)
            {
                throw new Exception("\n\n[FATAL ERROR] Player '冷漠的結局' corrupted critical game memory.\nSYSTEM STATUS: HERE I AM HERE I AM HERE I AM\n\n");
                System.Diagnostics.Process.GetCurrentProcess().Kill();
            }
        }
    }
    public class GlobalEvent1 : HorrorEvent
    {
        public override string Name => "global_event1";
        public override HorrorEventCategory Category => HorrorEventCategory.Medium; 

        public override void Trigger()
        {
            
            SubworldSystem.Enter<HorrorSubworld>();
        }
    }
}
