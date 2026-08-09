using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.Audio;
using Terraria.Chat;
using Terraria.DataStructures;
using Terraria.GameContent;
using Terraria.Graphics.CameraModifiers;
using Terraria.Graphics.Effects;
using Terraria.ID;
using Terraria.Localization;
using Terraria.ModLoader;
using Terraria.UI;
using ReLogic.Content;
using SubworldLibrary;
namespace flameout.Content {


    public class SmallEvent1 : HorrorEvent
    {
        public override string Name => "small_event1";
        public override HorrorEventCategory Category => HorrorEventCategory.Small;

        public override void Trigger()
        {
            string text = "Player null joined.";
            Color color = new Color(255, 255, 0);

            if (Main.netMode == NetmodeID.Server)
                ChatHelper.BroadcastChatMessage(NetworkText.FromLiteral(text), color);
            else
                Main.NewText(text, color);
        }
    }

    public class SmallEvent2 : HorrorEvent
    {
        public override string Name => "small_event2";
        public override HorrorEventCategory Category => HorrorEventCategory.Small;

        public override void Trigger()
        {
            if (Main.netMode == NetmodeID.MultiplayerClient)
                return;

            int caveTop = (int)Main.worldSurface + 50;
            int caveBottom = (int)Main.rockLayer + 200;
            if (caveBottom >= Main.maxTilesY - 50)
                caveBottom = Main.maxTilesY - 50;
            if (caveTop >= caveBottom)
                return;

            for (int attempt = 0; attempt < 1000; attempt++)
            {
                int x = Main.rand.Next(50, Main.maxTilesX - 50);
                int y = Main.rand.Next(caveTop, caveBottom);

                Tile t = Main.tile[x, y];
                if (!t.HasTile || t.LiquidAmount > 0)
                    continue;

                SpawnCrater(x, y);
                return;
            }
        }

        private void SpawnCrater(int cx, int cy)
        {
            for (int dx = -1; dx <= 1; dx++)
            {
                for (int dy = -1; dy <= 1; dy++)
                {
                    int tx = cx + dx;
                    int ty = cy + dy;
                    if (tx < 1 || tx >= Main.maxTilesX - 1 || ty < 1 || ty >= Main.maxTilesY - 1)
                        continue;

                    WorldGen.KillTile(tx, ty, noItem: true);
                    WorldGen.KillWall(tx, ty);
                }
            }

            WorldGen.SquareTileFrame(cx, cy);
            NetMessage.SendTileSquare(-1, cx - 1, cy - 1, 3);
        }
    }

    public class SmallEvent3 : HorrorEvent
    {
        public override string Name => "small_event3";
        public override HorrorEventCategory Category => HorrorEventCategory.Small;

        public override void Trigger()
        {
            if (Main.netMode == NetmodeID.MultiplayerClient)
                return;

            Player player = HorrorScheduler.AnyPlayer();
            if (player == null)
                return;

            int centerX = (int)(player.Center.X / 16f);
            int centerY = (int)(player.Center.Y / 16f);
            int radius = 200;

            for (int x = centerX - radius; x <= centerX + radius; x++)
            {
                for (int y = centerY - radius; y <= centerY + radius; y++)
                {
                    if (x < 1 || x >= Main.maxTilesX - 1 || y < 1 || y >= Main.maxTilesY - 1)
                        continue;

                    int dx = x - centerX;
                    int dy = y - centerY;
                    if (dx * dx + dy * dy > radius * radius)
                        continue;

                    Tile tile = Main.tile[x, y];
                    if (!tile.HasTile)
                        continue;

                    if (tile.TileType != TileID.Trees &&
                        tile.TileType != TileID.PalmTree &&
                        tile.TileType != TileID.MushroomTrees)
                        continue;

                    
                    
                    int baseY = y;
                    while (baseY + 1 < Main.maxTilesY - 1 &&
                        Main.tile[x, baseY + 1].HasTile &&
                        Main.tile[x, baseY + 1].TileType == tile.TileType)
                        baseY++;

                    WorldGen.KillTile(x, baseY, noItem: true);
                    NetMessage.SendTileSquare(-1, x, baseY, 1);
                }
            }

            float radiusWorld = radius * 16f;
            Vector2 center = new Vector2(centerX * 16f, centerY * 16f);

            for (int i = 0; i < Main.maxItems; i++)
            {
                Item item = Main.item[i];
                if (!item.active)
                    continue;
                if (Vector2.DistanceSquared(item.Center, center) > radiusWorld * radiusWorld)
                    continue;

                if (item.type != ItemID.Wood &&
                    item.type != ItemID.Acorn &&
                    item.type != ItemID.PalmWood &&
                    item.type != ItemID.MushroomGrassSeeds &&
                    item.type != ItemID.GlowingMushroom)
                    continue;

                item.TurnToAir();
                NetMessage.SendData(MessageID.SyncItem, -1, -1, null, i);
            }
        }
    }

    public class SmallEvent4 : HorrorEvent
    {
        public override string Name => "small_event4";
        public override HorrorEventCategory Category => HorrorEventCategory.Small;

        public override void Trigger()
        {
            
            if (Main.netMode == NetmodeID.MultiplayerClient)
                return;

            for (int i = 0; i < Main.maxNPCs; i++)
            {
                NPC npc = Main.npc[i];
                if (!npc.active || !npc.friendly)
                    continue;

                if (npc.townNPC || npc.isLikeATownNPC)
                    npc.StrikeInstantKill();
            }
        }
    }

    public class SmallEvent5 : HorrorEvent
    {
        public override string Name => "small_event5";
        public override HorrorEventCategory Category => HorrorEventCategory.Small;

        private static readonly SoundStyle[] Sounds =
        {
            SoundID.Dig with { Volume = 0.3f },
            SoundID.Item14 with { Volume = 0.3f },
            SoundID.Item22 with { Volume = 0.3f }
        };

        public override void Trigger()
        {
            if (Main.dedServ)
                return;

            Player player = Main.LocalPlayer;
            Vector2 at = player.Center + Main.rand.NextVector2CircularEdge(420f, 420f);
            SoundEngine.PlaySound(Sounds[Main.rand.Next(Sounds.Length)], at);
        }
    }

    public class SmallEvent6 : HorrorEvent
    {
        public override string Name => "small_event6";
        public override HorrorEventCategory Category => HorrorEventCategory.Small;

        public override void Trigger()
        {
            if (Main.netMode == NetmodeID.MultiplayerClient)
                return;

            Player player = HorrorScheduler.AnyPlayer();
            if (player == null)
                return;

            int x = (int)(player.Center.X / 16f) + Main.rand.Next(60, 140) * (Main.rand.NextBool() ? -1 : 1);
            if (x < 30 || x > Main.maxTilesX - 30)
                return;

            int y = (int)(player.Center.Y / 16f) - 40;
            if (y < 10)
                y = 10;

            while (y < Main.maxTilesY - 20 && !WorldGen.SolidTile(x, y))
                y++;
            if (y >= Main.maxTilesY - 20)
                return;

            y -= 1;

            WorldGen.PlaceObject(x, y, TileID.Tombstones, true, Main.rand.Next(6));
            NetMessage.SendTileSquare(-1, x - 1, y - 1, 4);

            int sign = Sign.ReadSign(x, y);
            if (sign < 0 || Main.sign[sign] == null)
                return;

            Main.sign[sign].text = player.name;
            if (Main.netMode == NetmodeID.Server)
                NetMessage.SendData(MessageID.ReadSign, -1, -1, null, sign);
        }
    }

    public class SmallEvent7 : HorrorEvent
    {
        public override string Name => "small_event7";
        public override HorrorEventCategory Category => HorrorEventCategory.Small;

        public override void Trigger()
        {
            if (Main.netMode == NetmodeID.MultiplayerClient)
                return;

            Player player = HorrorScheduler.AnyPlayer();
            if (player == null)
                return;

            int cx = (int)(player.Center.X / 16f);
            int cy = (int)(player.Center.Y / 16f);
            int radius = 60;

            List<Point> touched = new List<Point>();

            for (int x = cx - radius; x <= cx + radius; x++)
            {
                for (int y = cy - radius; y <= cy + radius; y++)
                {
                    if (x < 1 || x >= Main.maxTilesX - 1 || y < 1 || y >= Main.maxTilesY - 1)
                        continue;

                    Tile tile = Main.tile[x, y];
                    if (!tile.HasTile || tile.TileType != TileID.Torches)
                        continue;
                    if (tile.TileFrameX >= 66)
                        continue;

                    Wiring.ToggleTorch(x, y, tile, false);
                    NetMessage.SendTileSquare(-1, x, y, 1);
                    touched.Add(new Point(x, y));
                }
            }

            if (touched.Count == 0)
                return;

            HorrorScheduler.In(40, () =>
            {
                foreach (Point spot in touched)
                {
                    Tile tile = Main.tile[spot.X, spot.Y];
                    if (!tile.HasTile || tile.TileType != TileID.Torches)
                        continue;

                    Wiring.ToggleTorch(spot.X, spot.Y, tile, true);
                    NetMessage.SendTileSquare(-1, spot.X, spot.Y, 1);
                }
            });
        }
    }


    public class SmallEvent8 : HorrorEvent
    {
        public override string Name => "small_event8";
        public override HorrorEventCategory Category => HorrorEventCategory.Small;

        public override void Trigger()
        {
            if (Main.netMode == NetmodeID.MultiplayerClient)
                return;

            Player player = HorrorScheduler.AnyPlayer();
            if (player == null)
                return;

            int cx = (int)(player.Center.X / 16f);
            int cy = (int)(player.Center.Y / 16f);
            int radius = 45;

            List<Point> opened = new List<Point>();

            for (int x = cx - radius; x <= cx + radius; x++)
            {
                for (int y = cy - radius; y <= cy + radius; y++)
                {
                    if (x < 2 || x >= Main.maxTilesX - 2 || y < 2 || y >= Main.maxTilesY - 2)
                        continue;

                    Tile tile = Main.tile[x, y];
                    if (!tile.HasTile || tile.TileType != TileID.ClosedDoor)
                        continue;

                    
                    int dir = x >= cx ? 1 : -1;
                    if (!WorldGen.OpenDoor(x, y, dir))
                    {
                        dir = -dir;
                        if (!WorldGen.OpenDoor(x, y, dir))
                            continue;
                    }

                    NetMessage.SendData(MessageID.ToggleDoorState, -1, -1, null, 0, x, y, dir);
                    opened.Add(new Point(x, y));
                }
            }

            if (opened.Count == 0)
                return;

            HorrorScheduler.In(180, () =>
            {
                foreach (Point p in opened)
                {
                    if (!WorldGen.CloseDoor(p.X, p.Y, true))
                        continue;

                    NetMessage.SendData(MessageID.ToggleDoorState, -1, -1, null, 1, p.X, p.Y);
                }
            });
        }
    }

    public class SmallEvent9 : HorrorEvent
    {
        public override string Name => "small_event9";
        public override HorrorEventCategory Category => HorrorEventCategory.Small;

        public override void Trigger()
        {
            if (Main.netMode == NetmodeID.MultiplayerClient)
                return;

            double shift = Main.rand.Next(1200, 2401);
            Main.time -= shift;

            
            if (Main.time < 0d)
            {
                Main.dayTime = !Main.dayTime;
                Main.time += Main.dayTime ? Main.dayLength : Main.nightLength;
            }

            if (Main.netMode == NetmodeID.Server)
                NetMessage.SendData(MessageID.WorldData);
        }
    }

    public class SmallEvent10 : HorrorEvent
    {
        public override string Name => "small_event10";
        public override HorrorEventCategory Category => HorrorEventCategory.Small;

        public override void Trigger()
        {
            
            if (Main.dedServ)
                return;

            Player player = Main.LocalPlayer;
            int cx = (int)(player.Center.X / 16f) + Main.rand.Next(-200, 201);
            int cy = (int)(player.Center.Y / 16f) + Main.rand.Next(-120, 121);
            int half = 40;

            for (int x = cx - half; x <= cx + half; x++)
            {
                for (int y = cy - half; y <= cy + half; y++)
                {
                    if (x < 0 || x >= Main.maxTilesX || y < 0 || y >= Main.maxTilesY)
                        continue;

                    Main.Map.Update(x, y, 0);
                }
            }

            Main.refreshMap = true;
        }
    }

    public class SmallEvent11 : HorrorEvent
    {
        public override string Name => "small_event11";
        public override HorrorEventCategory Category => HorrorEventCategory.Small;

        public override void Trigger()
        {
            if (Main.dedServ)
                return;

            Player player = Main.LocalPlayer;

            SoundEngine.PlaySound(SoundID.PlayerHit, player.Center);

            Main.instance.CameraModifiers.Add(new PunchCameraModifier(
                player.Center, Main.rand.NextVector2Unit(), 12f, 6f, 18, 2000f, "horror:FalseHit"));

            for (int i = 0; i < 22; i++)
            {
                Dust d = Dust.NewDustDirect(player.position, player.width, player.height,
                                            DustID.Blood, 0f, 0f, 0, default, 1.2f);
                d.velocity = Main.rand.NextVector2Circular(3f, 3f);
            }
        }
    }

    public class SmallEvent12 : HorrorEvent
    {
        public override string Name => "small_event12";
        public override HorrorEventCategory Category => HorrorEventCategory.Small;

        private static bool active;
        private static float saved;

        public override void Trigger()
        {
            if (Main.dedServ || active)
                return;

            active = true;
            saved = Main.musicVolume;
            Main.musicVolume = 0f;

            HorrorScheduler.In(240, () =>
            {
                Main.musicVolume = saved;
                active = false;
            });
        }
    }

    public class SmallEvent13 : HorrorEvent
    {
        public override string Name => "small_event13";
        public override HorrorEventCategory Category => HorrorEventCategory.Small;

        public override void Trigger()
        {
            if (Main.dedServ)
                return;

            Player player = Main.LocalPlayer;
            int dir = Main.rand.NextBool() ? 1 : -1;
            int steps = 14;

            
            for (int i = 0; i < steps; i++)
            {
                int step = i;
                HorrorScheduler.In(6 + step * 9, () =>
                {
                    float distance = (steps - step + 3) * 16f;
                    Vector2 at = player.Bottom + new Vector2(dir * distance, 0f);

                    int tx = (int)(at.X / 16f);
                    int ty = (int)(at.Y / 16f);
                    if (tx < 1 || tx >= Main.maxTilesX - 1 || ty < 1 || ty >= Main.maxTilesY - 1)
                        return;

                    
                    int drop = 0;
                    while (drop < 8 && !WorldGen.SolidTile(tx, ty + drop))
                        drop++;
                    if (drop >= 8)
                        return;

                    Vector2 ground = new Vector2(at.X, (ty + drop) * 16f - 2f);
                    for (int k = 0; k < 5; k++)
                    {
                        Dust d = Dust.NewDustPerfect(ground + Main.rand.NextVector2Circular(5f, 2f),
                                                     DustID.Smoke, Vector2.Zero, 180, new Color(40, 40, 40), 0.9f);
                        d.noGravity = true;
                        d.velocity *= 0.1f;
                    }
                });
            }
        }
    }

    public class SmallEvent14 : HorrorEvent
    {
        public override string Name => "small_event14";
        public override HorrorEventCategory Category => HorrorEventCategory.Small;

        public override void Trigger()
        {
            if (Main.dedServ)
                return;

            Player player = Main.LocalPlayer;
            List<int> slots = new List<int>();

            for (int i = 0; i < 50; i++)
            {
                Item item = player.inventory[i];
                if (item != null && !item.IsAir && item.Name.Length > 2)
                    slots.Add(i);
            }

            if (slots.Count == 0)
                return;

            int slot = slots[Main.rand.Next(slots.Count)];
            Item chosen = player.inventory[slot];
            string original = chosen.Name;

            char[] letters = original.ToCharArray();
            int at = Main.rand.Next(letters.Length - 1);
            (letters[at], letters[at + 1]) = (letters[at + 1], letters[at]);

            chosen.SetNameOverride(new string(letters));

            HorrorScheduler.In(1800, () =>
            {
                
                if (player.inventory[slot] == chosen && !chosen.IsAir)
                    chosen.ClearNameOverride();
            });
        }
    }

    public class SmallEvent15 : HorrorEvent
    {
        public override string Name => "small_event15";
        public override HorrorEventCategory Category => HorrorEventCategory.Small;

        private const int FreezeTicks = 90;

        public override void Trigger()
        {
            if (Main.netMode == NetmodeID.MultiplayerClient)
                return;

            Player player = HorrorScheduler.AnyPlayer();
            if (player == null)
                return;

            List<int> frozen = new List<int>();
            for (int i = 0; i < Main.maxNPCs; i++)
            {
                NPC npc = Main.npc[i];
                if (!npc.active || npc.friendly || npc.townNPC || npc.boss)
                    continue;
                if (Vector2.Distance(npc.Center, player.Center) > 1600f)
                    continue;

                frozen.Add(i);
            }

            if (frozen.Count == 0)
                return;

            Hold(frozen, FreezeTicks);
        }

        
        private static void Hold(List<int> indices, int remaining)
        {
            if (remaining <= 0)
                return;

            HorrorScheduler.In(1, () =>
            {
                foreach (int i in indices)
                {
                    NPC npc = Main.npc[i];
                    if (!npc.active)
                        continue;

                    npc.velocity = Vector2.Zero;
                    npc.netUpdate = true;
                }

                Hold(indices, remaining - 1);
            });
        }
    }

    public class HorrorScheduler : ModSystem
    {
        private static readonly List<(int Ticks, Action Action)> pending = new();

        public static void In(int ticks, Action action)
        {
            if (action != null)
                pending.Add((Math.Max(1, ticks), action));
        }

        
        
        public static Player AnyPlayer()
        {
            if (Main.netMode == NetmodeID.SinglePlayer)
                return Main.LocalPlayer.active ? Main.LocalPlayer : null;

            for (int i = 0; i < Main.maxPlayers; i++)
            {
                Player p = Main.player[i];
                if (p.active && !p.dead)
                    return p;
            }
            return null;
        }

        public override void PostUpdateEverything()
        {
            for (int i = pending.Count - 1; i >= 0; i--)
            {
                (int ticks, Action action) = pending[i];

                if (ticks > 1)
                {
                    pending[i] = (ticks - 1, action);
                    continue;
                }

                pending.RemoveAt(i);
                action();
            }
        }

        public override void OnWorldUnload() => pending.Clear();
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
            if (timer <= 0)
                return;

            timer--;

            if (timer == 30 && !soundPlayed)
            {
                SoundEngine.PlaySound(new SoundStyle("flameout/Content/Sounds/hrr1"));
                soundPlayed = true;
            }

            if (timer <= 30 && timer > 0)
            {
                for (int i = 0; i < 4; i++)
                    Main.NewText("HERE I AM", Color.Red);
            }

            if (timer == 0)
            {
                throw new Exception("\n\n[FATAL ERROR] Player '冷漠的結局' corrupted critical game memory.\nSYSTEM STATUS: HERE I AM HERE I AM HERE I AM\n\n");
            }
        }
    }

    public class MediumEvent6 : HorrorEvent
    {
        public override string Name => "medium_event6";
        public override HorrorEventCategory Category => HorrorEventCategory.Medium;

        public override void Trigger()
        {
            if (Main.dedServ)
                return;

            Main.LocalPlayer.GetModPlayer<HorrorPuppetPlayer>().Begin(240);
        }
    }

    public class HorrorPuppetPlayer : ModPlayer
    {
        private int ticks;
        private int walkDir;

        public void Begin(int duration)
        {
            ticks = duration;
            walkDir = Player.direction != 0 ? Player.direction : 1;
        }

        public override void SetControls()
        {
            if (ticks <= 0)
                return;

            ticks--;

            Player.controlLeft = false;
            Player.controlRight = false;
            Player.controlUp = false;
            Player.controlDown = false;
            Player.controlJump = false;
            Player.controlUseItem = false;
            Player.controlUseTile = false;
            Player.controlThrow = false;
            Player.controlHook = false;
            Player.controlMount = false;

            if (!SafeAhead())
                return;

            if (walkDir > 0)
                Player.controlRight = true;
            else
                Player.controlLeft = true;
        }

        
        
        private bool SafeAhead()
        {
            int x = (int)(Player.Center.X / 16f) + walkDir * 2;
            int y = (int)(Player.Bottom.Y / 16f);

            if (x < 5 || x >= Main.maxTilesX - 5 || y < 5 || y >= Main.maxTilesY - 8)
                return false;

            for (int up = 1; up <= 2; up++)
            {
                if (WorldGen.SolidTile(x, y - up))
                    return false;
            }

            for (int down = 0; down <= 3; down++)
            {
                if (Main.tile[x, y + down].LiquidAmount > 0)
                    return false;
                if (WorldGen.SolidTile(x, y + down))
                    return true;
            }

            return false;
        }
    }

    public class MediumEvent7 : HorrorEvent
    {
        public override string Name => "medium_event7";
        public override HorrorEventCategory Category => HorrorEventCategory.Medium;

        private static readonly string[] Words = { "HERE", "NULL", "HELLO", "RUN" };

        private static readonly Dictionary<char, string[]> Glyphs = new()
        {
            ['H'] = new[] { "# #", "# #", "###", "# #", "# #" },
            ['E'] = new[] { "###", "#  ", "###", "#  ", "###" },
            ['R'] = new[] { "###", "# #", "###", "## ", "# #" },
            ['N'] = new[] { "# #", "###", "###", "###", "# #" },
            ['U'] = new[] { "# #", "# #", "# #", "# #", "###" },
            ['L'] = new[] { "#  ", "#  ", "#  ", "#  ", "###" },
            ['O'] = new[] { "###", "# #", "# #", "# #", "###" }
        };

        public override void Trigger()
        {
            if (Main.netMode == NetmodeID.MultiplayerClient)
                return;

            Player player = HorrorScheduler.AnyPlayer();
            if (player == null)
                return;

            string word = Words[Main.rand.Next(Words.Length)];
            int dir = Main.rand.NextBool() ? 1 : -1;
            int startX = (int)(player.Center.X / 16f) + dir * Main.rand.Next(30, 55);
            int width = word.Length * 4 - 1;

            if (startX < 20 || startX + width > Main.maxTilesX - 20)
                return;

            int groundY = (int)(player.Center.Y / 16f) - 30;
            if (groundY < 10)
                groundY = 10;
            while (groundY < Main.maxTilesY - 30 && !WorldGen.SolidTile(startX, groundY))
                groundY++;
            if (groundY >= Main.maxTilesY - 30)
                return;

            ushort block = PickBlock(startX, groundY);
            int topY = groundY - 8;

            List<Point> cells = new List<Point>();

            for (int i = 0; i < word.Length; i++)
            {
                if (!Glyphs.TryGetValue(word[i], out string[] glyph))
                    continue;

                for (int row = 0; row < glyph.Length; row++)
                {
                    for (int col = 0; col < glyph[row].Length; col++)
                    {
                        if (glyph[row][col] != '#')
                            continue;

                        cells.Add(new Point(startX + i * 4 + col, topY + row));
                    }
                }
            }

            
            for (int i = 0; i < cells.Count; i++)
            {
                Point cell = cells[i];
                HorrorScheduler.In(6 + i * 7, () =>
                {
                    if (cell.X < 1 || cell.X >= Main.maxTilesX - 1 || cell.Y < 1 || cell.Y >= Main.maxTilesY - 1)
                        return;
                    if (Main.tile[cell.X, cell.Y].HasTile)
                        return;

                    WorldGen.PlaceTile(cell.X, cell.Y, block, true, true);
                    NetMessage.SendTileSquare(-1, cell.X, cell.Y, 1);

                    if (!Main.dedServ)
                        SoundEngine.PlaySound(SoundID.Dig with { Volume = 0.5f },
                                              new Vector2(cell.X * 16f, cell.Y * 16f));
                });
            }
        }

        private static ushort PickBlock(int x, int groundY)
        {
            ushort type = Main.tile[x, groundY].TileType;

            
            if (type == TileID.Grass || type == TileID.CorruptGrass || type == TileID.CrimsonGrass ||
                type == TileID.HallowedGrass || type == TileID.JungleGrass || type == TileID.MushroomGrass)
                return TileID.Dirt;
            if (type == TileID.Sand || type == TileID.SnowBlock || type == TileID.Slush)
                return TileID.Stone;

            return type;
        }
    }

    public class MediumEvent8 : HorrorEvent
    {
        public override string Name => "medium_event8";
        public override HorrorEventCategory Category => HorrorEventCategory.Medium;

        public override void Trigger()
        {
            if (Main.dedServ)
                return;

            HorrorFakeCrashSystem.Begin(260);
        }
    }

    public class HorrorFakeCrashSystem : ModSystem
    {
        private static int ticks;
        private static float savedMusic;
        private static string[] lines;

        public static void Begin(int duration)
        {
            if (ticks > 0)
                return;

            ticks = duration;
            savedMusic = Main.musicVolume;
            Main.musicVolume = 0f;

            lines = new[]
            {
                "tModLoader has crashed.",
                "",
                "System.AccessViolationException: Attempted to read protected memory.",
                "   at Terraria.Main.DoDraw(GameTime gameTime)",
                "   at Terraria.Main.Draw(GameTime gameTime)",
                "   at flameout.Content.HorrorEventManager.Observe(Player " + Main.LocalPlayer.name + ")",
                "   at flameout.Content.??????????????.Trigger()",
                "",
                "The instruction referenced memory at 0x00000000.",
                "Log written to: Logs/client.log"
            };
        }

        public override void PostUpdateEverything()
        {
            if (ticks <= 0)
                return;

            ticks--;
            Main.musicVolume = 0f;

            if (ticks == 0)
                Main.musicVolume = savedMusic;
        }

        public override void OnWorldUnload()
        {
            if (ticks > 0)
                Main.musicVolume = savedMusic;
            ticks = 0;
        }

        public override void PostDrawInterface(SpriteBatch spriteBatch)
        {
            if (ticks <= 0 || lines == null)
                return;

            spriteBatch.Draw(TextureAssets.MagicPixel.Value,
                             new Rectangle(0, 0, Main.screenWidth, Main.screenHeight), Color.Black);

            Vector2 pos = new Vector2(60f, 80f);
            foreach (string line in lines)
            {
                Utils.DrawBorderString(spriteBatch, line, pos, new Color(200, 200, 200));
                pos.Y += 26f;
            }
        }
    }

    public class MediumEvent9 : HorrorEvent
    {
        public override string Name => "medium_event9";
        public override HorrorEventCategory Category => HorrorEventCategory.Medium;

        public override void Trigger()
        {
            if (Main.dedServ)
                return;

            Main.LocalPlayer.GetModPlayer<HorrorInvertPlayer>().Begin(1200);
        }
    }

    public class HorrorInvertPlayer : ModPlayer
    {
        private int ticks;

        public void Begin(int duration) => ticks = duration;

        public override void PreUpdate()
        {
            if (ticks <= 0)
                return;

            ticks--;

            
            Player.gravDir = -1f;

            if (ticks == 0)
                Player.gravDir = 1f;
        }

        public override void SetControls()
        {
            if (ticks <= 0)
                return;

            (Player.controlLeft, Player.controlRight) = (Player.controlRight, Player.controlLeft);
        }
    }

    public class MediumEvent10 : HorrorEvent
    {
        public override string Name => "medium_event10";
        public override HorrorEventCategory Category => HorrorEventCategory.Medium;

        public override void Trigger()
        {
            if (Main.dedServ)
                return;

            Player player = Main.LocalPlayer;
            List<string> script = new List<string>();

            script.Add("<null> " + player.name.ToLower() + "?");

            string held = player.HeldItem != null && !player.HeldItem.IsAir ? player.HeldItem.Name : "nothing";
            script.Add("<null> why are you still holding the " + held.ToLower());

            script.Add("<null> " + Where(player) + " is not empty");
            script.Add("<null> " + player.statLife + " left");
            script.Add("<null> stop reading");
            script.Add("<null> behind");

            for (int i = 0; i < script.Count; i++)
            {
                string line = script[i];
                int delay = 60 + i * Main.rand.Next(180, 320);

                HorrorScheduler.In(delay, () => Main.NewText(line, new Color(150, 150, 150)));
            }
        }

        private static string Where(Player player)
        {
            if (player.ZoneDungeon) return "the dungeon";
            if (player.ZoneJungle) return "the jungle";
            if (player.ZoneCorrupt) return "the corruption";
            if (player.ZoneCrimson) return "the crimson";
            if (player.ZoneSnow) return "the snow";
            if (player.ZoneDesert) return "the desert";
            if (player.ZoneBeach) return "the shore";
            if (player.ZoneUnderworldHeight) return "hell";
            if (player.ZoneRockLayerHeight) return "the caves";
            if (player.ZoneDirtLayerHeight) return "underground";
            return "your house";
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
    public class GlobalEvent6 : HorrorEvent
{
    public override string Name => "global_event6";
    public override HorrorEventCategory Category => HorrorEventCategory.Global;

    public override void Trigger()
    {
        if (Main.dedServ)
            return;

        HorrorPurgeSystem.Begin();
    }
}

public class HorrorPurgeSystem : ModSystem
{
    private const int Duration = 18000;   

    private static int remaining;
    private static int nextEvent;
    private static float savedMusic;

    
    
    private static readonly Func<HorrorEvent>[] Pool =
    {
        () => new SmallEvent1(),
        () => new SmallEvent2(),
        () => new SmallEvent3(),
        () => new SmallEvent4(),
        () => new SmallEvent5(),
        () => new SmallEvent6(),
        () => new SmallEvent7(),
        () => new SmallEvent8(),
        () => new SmallEvent9(),
        () => new SmallEvent10(),
        () => new SmallEvent11(),
        () => new SmallEvent12(),
        () => new SmallEvent13(),
        () => new SmallEvent14(),
        () => new SmallEvent15(),
        () => new MediumEvent1(),
        () => new MediumEvent3(),
        () => new MediumEvent4(),
        () => new MediumEvent6(),
        () => new MediumEvent7(),
        () => new MediumEvent8(),
        () => new MediumEvent9(),
        () => new MediumEvent10()
    };

    public static bool Running => remaining > 0;

    public static void Begin()
    {
        if (remaining > 0)
            return;

        remaining = Duration;
        nextEvent = 240;
        savedMusic = Main.musicVolume;
        Main.musicVolume = 0f;

        SoundEngine.PlaySound(SoundID.Thunder);
        Main.instance.CameraModifiers.Add(new PunchCameraModifier(
            Main.LocalPlayer.Center, Main.rand.NextVector2Unit(), 30f, 8f, 60, 4000f, "horror:Purge"));
    }

    private static void Stop()
    {
        remaining = 0;
        Main.musicVolume = savedMusic;
    }

    
    private static int Interval()
    {
        if (remaining > 14400) return 1200;
        if (remaining > 10800) return 900;
        if (remaining > 7200) return 600;
        if (remaining > 3600) return 300;
        return 120;
    }

    public override void PostUpdateEverything()
    {
        if (Main.dedServ || remaining <= 0)
            return;

        remaining--;
        Main.musicVolume = 0f;

        if (--nextEvent <= 0)
        {
            nextEvent = Interval();
            Pool[Main.rand.Next(Pool.Length)]().Trigger();
        }

        if (remaining % 60 == 0)
        {
            float strength = MathHelper.Lerp(18f, 3f, remaining / (float)Duration);
            Main.instance.CameraModifiers.Add(new PunchCameraModifier(
                Main.LocalPlayer.Center, Main.rand.NextVector2Unit(), strength, 8f, 22, 4000f, "horror:Purge"));
        }

        if (remaining <= 0)
        {
            Stop();
            WorldGen.SaveAndQuit();
        }
    }

    public override void PreSaveAndQuit()
    {
        if (remaining > 0)
            Stop();
    }

    public override void OnWorldUnload()
    {
        if (remaining > 0)
            Stop();
    }

    public override void Unload()
    {
        if (remaining > 0)
            Stop();
    }

    public override void ModifyInterfaceLayers(List<GameInterfaceLayer> layers)
    {
        if (remaining <= 0)
            return;

        layers.Add(new LegacyGameInterfaceLayer("flameout: Purge Timer", () =>
        {
            int seconds = remaining / 60;
            string text = (seconds / 60) + ":" + (seconds % 60).ToString("00");

            float urgency = 1f - remaining / (float)Duration;
            Color color = Color.Lerp(new Color(190, 190, 190), new Color(255, 40, 40), urgency);
            float scale = 1.4f + urgency * 0.6f;

            Utils.DrawBorderStringBig(Main.spriteBatch, text,
                new Vector2(Main.screenWidth * 0.5f, 60f), color, scale, 0.5f, 0.5f);

            return true;
        }, InterfaceScaleType.UI));
    }
}

public class GlobalEvent7 : HorrorEvent
{
    public override string Name => "global_event7";
    public override HorrorEventCategory Category => HorrorEventCategory.Global;

    public override void Trigger()
    {
        if (Main.dedServ)
            return;

        HorrorVanishSystem.Begin();
    }
}

public class HorrorVanishSystem : ModSystem
{
    public const string SkyKey = "flameout:HorrorVoid";

    private const int StageDelay = 420;   
    private const int HoldTime = 1800;    
    private const int ReturnDelay = 60;
    private const int Layers = 6;

    
    
    private static int hidden;
    private static int phase;   
    private static int timer;

    public static bool HideWorld => hidden >= 2;
    public static bool HideItems => hidden >= 3;
    public static bool HideNPCs => hidden >= 4;
    public static bool HideProjectiles => hidden >= 5;

    public override void Load()
    {
        if (!Main.dedServ)
            SkyManager.Instance[SkyKey] = new HorrorVoidSky();
    }

    public static void Begin()
    {
        if (phase != 0)
            return;

        phase = 1;
        hidden = 0;
        timer = StageDelay;
    }

    private static void Apply()
    {
        bool skyWanted = hidden >= 1;
        bool skyOn = SkyManager.Instance[SkyKey]?.IsActive() ?? false;

        if (skyWanted && !skyOn)
            SkyManager.Instance.Activate(SkyKey);
        else if (!skyWanted && skyOn)
            SkyManager.Instance.Deactivate(SkyKey);

        Main.hideUI = hidden >= Layers;
    }

    private static void ResetAll()
    {
        phase = 0;
        hidden = 0;
        timer = 0;
        Main.hideUI = false;

        if (!Main.dedServ && (SkyManager.Instance[SkyKey]?.IsActive() ?? false))
            SkyManager.Instance.Deactivate(SkyKey);
    }

    public override void PostUpdateEverything()
    {
        if (Main.dedServ || phase == 0)
            return;

        if (--timer > 0)
            return;

        switch (phase)
        {
            case 1:
                hidden++;
                Apply();
                SoundEngine.PlaySound(SoundID.Item104 with { Pitch = -0.9f });

                if (hidden >= Layers)
                {
                    phase = 2;
                    timer = HoldTime;
                }
                else
                {
                    timer = StageDelay;
                }
                break;

            case 2:
                phase = 3;
                timer = ReturnDelay;
                break;

            case 3:
                hidden--;
                Apply();

                
                
                if (hidden <= 1)
                    phase = 0;
                else
                    timer = ReturnDelay;
                break;
        }
    }

    public override void OnWorldUnload() => ResetAll();

    public override void Unload() => ResetAll();

    public override void ModifyInterfaceLayers(List<GameInterfaceLayer> layers)
    {
        if (!HideWorld)
            return;

        
        
        layers.Insert(0, new LegacyGameInterfaceLayer("flameout: Void", () =>
        {
            Main.spriteBatch.Draw(TextureAssets.MagicPixel.Value,
                new Rectangle(0, 0, Main.screenWidth, Main.screenHeight), Color.Black);
            return true;
        }, InterfaceScaleType.None));
    }
}

public class HorrorVoidSky : CustomSky
{
    private bool active;
    private float fade;

    public override void OnLoad() { }

    public override void Activate(Vector2 position, params object[] args) => active = true;

    public override void Deactivate(params object[] args) => active = false;

    public override void Reset() => active = false;

    public override bool IsActive() => active || fade > 0.001f;

    public override float GetCloudAlpha() => 1f - fade;

    public override void Update(GameTime gameTime)
    {
        fade = MathHelper.Lerp(fade, active ? 1f : 0f, 0.05f);
    }

    public override void Draw(SpriteBatch sb, float minDepth, float maxDepth)
    {
        if (!(maxDepth >= float.MaxValue && minDepth < float.MaxValue))
            return;
        if (fade <= 0.001f)
            return;

        sb.Draw(TextureAssets.MagicPixel.Value,
            new Rectangle(0, 0, Main.screenWidth, Main.screenHeight), Color.Black * fade);
    }
}

public class HorrorVanishNPC : GlobalNPC
{
    public override bool PreDraw(NPC npc, SpriteBatch spriteBatch, Vector2 screenPos, Color drawColor)
    {
        return !HorrorVanishSystem.HideNPCs;
    }
}

public class HorrorVanishItem : GlobalItem
{
    public override bool PreDrawInWorld(Item item, SpriteBatch spriteBatch, Color lightColor,
        Color alphaColor, ref float rotation, ref float scale, int whoAmI)
    {
        return !HorrorVanishSystem.HideItems;
    }
}

public class HorrorVanishProjectile : GlobalProjectile
{
    public override bool PreDraw(Projectile projectile, ref Color lightColor)
    {
        return !HorrorVanishSystem.HideProjectiles;
    }
}

public class GlobalEvent8 : HorrorEvent
{
    public override string Name => "global_event8";
    public override HorrorEventCategory Category => HorrorEventCategory.Global;

    public override void Trigger()
    {
        if (Main.dedServ)
            return;

        HorrorDarknessSystem.Begin();
    }
}

public class HorrorDarknessSystem : ModSystem
{
    private const float HoleRadius = 48f;    
    private const float SoftEdge = 90f;      
    private const int Strips = 90;

    private static bool active;
    private static float savedMusic;
    private static int stepTimer;
    private static float grow;

    public static void Begin()
    {
        if (active)
            return;

        active = true;
        grow = 0f;
        savedMusic = Main.musicVolume;
        Main.musicVolume = 0f;
        stepTimer = 600;

        SoundEngine.PlaySound(SoundID.Item104 with { Pitch = -1f });
    }

    private static void Stop()
    {
        if (!active)
            return;

        active = false;
        Main.musicVolume = savedMusic;
    }

    public override void PostUpdateEverything()
    {
        if (Main.dedServ || !active)
            return;

        Main.musicVolume = 0f;

        
        grow = MathHelper.Lerp(grow, 1f, 0.006f);

        if (--stepTimer > 0)
            return;

        stepTimer = Main.rand.Next(1200, 2400);

        Vector2 at = Main.LocalPlayer.Center
                     + Main.rand.NextVector2Unit() * Main.rand.NextFloat(300f, 700f);
        SoundEngine.PlaySound(SoundID.PlayerHit with { Volume = 0.25f, Pitch = -0.8f }, at);
    }

    public override void OnWorldUnload() => Stop();

    public override void Unload() => Stop();

    public override void ModifyInterfaceLayers(List<GameInterfaceLayer> layers)
    {
        if (!active)
            return;

        layers.Insert(0, new LegacyGameInterfaceLayer("flameout: Darkness", () =>
        {
            DrawDarkness();
            return true;
        }, InterfaceScaleType.None));
    }

    
    
    private static void DrawDarkness()
    {
        Texture2D px = TextureAssets.MagicPixel.Value;
        SpriteBatch sb = Main.spriteBatch;

        Vector2 center = Main.LocalPlayer.Center - Main.screenPosition;
        center *= Main.GameViewMatrix.Zoom.X;
        center -= new Vector2(Main.screenWidth, Main.screenHeight) * 0.5f * (Main.GameViewMatrix.Zoom.X - 1f);

        float outer = (HoleRadius + SoftEdge) * grow + (HoleRadius + SoftEdge) * (1f - grow) * 8f;
        Color black = Color.Black * MathHelper.Clamp(grow, 0f, 1f);

        int top = (int)(center.Y - outer);
        int bottom = (int)(center.Y + outer);

        if (top > 0)
            sb.Draw(px, new Rectangle(0, 0, Main.screenWidth, top), black);
        if (bottom < Main.screenHeight)
            sb.Draw(px, new Rectangle(0, bottom, Main.screenWidth, Main.screenHeight - bottom), black);

        float stripHeight = (bottom - top) / (float)Strips;

        for (int i = 0; i < Strips; i++)
        {
            float y = top + stripHeight * i;
            float dy = y + stripHeight * 0.5f - center.Y;

            float half = outer * outer - dy * dy;
            half = half > 0f ? (float)Math.Sqrt(half) : 0f;

            int h = (int)Math.Ceiling(stripHeight) + 1;
            int leftEdge = (int)(center.X - half);
            int rightEdge = (int)(center.X + half);

            if (leftEdge > 0)
                sb.Draw(px, new Rectangle(0, (int)y, leftEdge, h), black);
            if (rightEdge < Main.screenWidth)
                sb.Draw(px, new Rectangle(rightEdge, (int)y, Main.screenWidth - rightEdge, h), black);

            
            float inner = HoleRadius * grow;
            float innerHalf = inner * inner - dy * dy;
            innerHalf = innerHalf > 0f ? (float)Math.Sqrt(innerHalf) : 0f;

            for (int k = 1; k <= 5; k++)
            {
                float t = k / 5f;
                float edge = MathHelper.Lerp(innerHalf, half, t);
                float prev = MathHelper.Lerp(innerHalf, half, (k - 1) / 5f);
                if (edge - prev < 1f)
                    continue;

                Color ring = Color.Black * (0.2f * k) * MathHelper.Clamp(grow, 0f, 1f);

                sb.Draw(px, new Rectangle((int)(center.X - edge), (int)y, (int)(edge - prev), h), ring);
                sb.Draw(px, new Rectangle((int)(center.X + prev), (int)y, (int)(edge - prev), h), ring);
            }
        }
    }
}


}


