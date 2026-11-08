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
        () => new MediumEvent10(),
        () => new SmallEvent16(),
        () => new SmallEvent17(),
        () => new SmallEvent18(),
        () => new SmallEvent19(),
        () => new SmallEvent20(),
        () => new MediumEvent11(),
        () => new MediumEvent12(),
        () => new MediumEvent13(),
        () => new MediumEvent14(),
        () => new MediumEvent15()
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



    public class SmallEvent16 : HorrorEvent
    {
        public override string Name => "small_event16";
        public override HorrorEventCategory Category => HorrorEventCategory.Small;

        public override void Trigger()
        {
            if (Main.netMode == NetmodeID.MultiplayerClient)
                return;

            Player player = HorrorScheduler.AnyPlayer();
            if (player == null)
                return;

            int best = -1;
            float bestDist = 960f * 960f;

            for (int i = 0; i < Main.maxChests; i++)
            {
                Chest chest = Main.chest[i];
                if (chest == null || Chest.UsingChest(i) != -1)
                    continue;

                float dist = Vector2.DistanceSquared(new Vector2(chest.x * 16f + 16f, chest.y * 16f + 16f), player.Center);
                if (dist >= bestDist)
                    continue;

                bestDist = dist;
                best = i;
            }

            if (best < 0)
                return;

            Chest target = Main.chest[best];
            List<int> filled = new List<int>();

            for (int k = 0; k < target.item.Length; k++)
            {
                if (target.item[k] != null && !target.item[k].IsAir)
                    filled.Add(k);
            }

            if (filled.Count == 0)
                return;

            if (Main.netMode == NetmodeID.Server || !Main.rand.NextBool(3))
            {
                int a = filled[Main.rand.Next(filled.Count)];
                int b = Main.rand.Next(target.item.Length - 1);
                if (b >= a)
                    b++;

                (target.item[a], target.item[b]) = (target.item[b], target.item[a]);

                if (Main.netMode == NetmodeID.Server)
                {
                    NetMessage.SendData(MessageID.SyncChestItem, -1, -1, null, best, a);
                    NetMessage.SendData(MessageID.SyncChestItem, -1, -1, null, best, b);
                }
                return;
            }

            target.item[filled[Main.rand.Next(filled.Count)]].SetNameOverride("i was here");
        }
    }

    public class SmallEvent17 : HorrorEvent
    {
        public override string Name => "small_event17";
        public override HorrorEventCategory Category => HorrorEventCategory.Small;

        public override void Trigger()
        {
            if (Main.dedServ)
                return;

            Player player = Main.LocalPlayer;

            (string Text, int Icon)[] options =
            {
                (player.name + "'s Heart", ItemID.LifeCrystal),
                ("Nothing", ItemID.BlackLens),
                ("Eye", ItemID.Lens),
                ("Your Name", ItemID.Bone),
                ("It", ItemID.BlackLens)
            };

            (string text, int icon) = options[Main.rand.Next(options.Length)];

            Item fake = new Item();
            fake.SetDefaults(icon);
            fake.SetNameOverride(text);
            fake.Center = player.Top;

            PopupText.NewText(PopupTextContext.RegularItemPickup, fake, Main.rand.Next(1, 3));
            SoundEngine.PlaySound(SoundID.Grab, player.Center);
        }
    }

    public class SmallEvent18 : HorrorEvent
    {
        public override string Name => "small_event18";
        public override HorrorEventCategory Category => HorrorEventCategory.Small;

        private const int StareTicks = 300;

        public override void Trigger()
        {
            if (Main.netMode == NetmodeID.MultiplayerClient)
                return;

            Player player = HorrorScheduler.AnyPlayer();
            if (player == null)
                return;

            List<int> watchers = new List<int>();
            for (int i = 0; i < Main.maxNPCs; i++)
            {
                NPC npc = Main.npc[i];
                if (!npc.active || !npc.townNPC)
                    continue;
                if (Vector2.Distance(npc.Center, player.Center) > 1600f)
                    continue;

                watchers.Add(i);
            }

            if (watchers.Count == 0)
                return;

            Hold(watchers, player.whoAmI, StareTicks);
        }

        private static void Hold(List<int> indices, int playerIndex, int remaining)
        {
            if (remaining <= 0)
                return;

            HorrorScheduler.In(1, () =>
            {
                Player player = Main.player[playerIndex];
                if (!player.active)
                    return;

                foreach (int i in indices)
                {
                    NPC npc = Main.npc[i];
                    if (!npc.active)
                        continue;

                    npc.velocity = Vector2.Zero;

                    int dir = Math.Sign(player.Center.X - npc.Center.X);
                    if (dir != 0)
                    {
                        npc.direction = dir;
                        npc.spriteDirection = dir;
                    }

                    npc.netUpdate = true;
                }

                Hold(indices, playerIndex, remaining - 1);
            });
        }
    }

    public class SmallEvent19 : HorrorEvent
    {
        public override string Name => "small_event19";
        public override HorrorEventCategory Category => HorrorEventCategory.Small;

        private const int Delay = 180;
        private const int Steps = 60;

        public override void Trigger()
        {
            if (Main.dedServ)
                return;

            Player player = Main.LocalPlayer;
            HorrorTrailPlayer trail = player.GetModPlayer<HorrorTrailPlayer>();
            if (!trail.Has(Delay))
                return;

            for (int i = 0; i < Steps; i++)
            {
                int step = i;
                HorrorScheduler.In(1 + step * 3, () =>
                {
                    Vector2 center = trail.PastCenter(Delay);
                    float halfW = player.width * 0.5f;
                    float halfH = player.height * 0.5f;

                    for (int k = 0; k < 5; k++)
                    {
                        Vector2 at = center + new Vector2(Main.rand.NextFloat(-halfW, halfW), Main.rand.NextFloat(-halfH, halfH));
                        Dust d = Dust.NewDustPerfect(at, DustID.Smoke, Vector2.Zero, 110, new Color(15, 15, 15), 1.3f);
                        d.noGravity = true;
                    }

                    if (step != Steps - 1)
                        return;

                    for (int k = 0; k < 30; k++)
                    {
                        Dust d = Dust.NewDustPerfect(center, DustID.Smoke, Main.rand.NextVector2Circular(4f, 4f), 110, new Color(15, 15, 15), 1.4f);
                        d.noGravity = true;
                    }
                });
            }
        }
    }

    public class HorrorTrailPlayer : ModPlayer
    {
        public const int Length = 900;

        private Vector2[] positions;
        private int[] lives;
        private int head;
        private int count;

        public override void Initialize()
        {
            positions = new Vector2[Length];
            lives = new int[Length];
            head = 0;
            count = 0;
        }

        public override void PostUpdate()
        {
            if (Player.whoAmI != Main.myPlayer || positions == null)
                return;

            positions[head] = Player.position;
            lives[head] = Player.statLife;
            head = (head + 1) % Length;
            if (count < Length)
                count++;
        }

        public bool Has(int ticksAgo) => positions != null && count >= ticksAgo;

        private int Index(int ticksAgo)
        {
            ticksAgo = Math.Clamp(ticksAgo, 1, Math.Max(1, count));
            return ((head - ticksAgo) % Length + Length) % Length;
        }

        public Vector2 PastPosition(int ticksAgo) => count == 0 ? Player.position : positions[Index(ticksAgo)];

        public Vector2 PastCenter(int ticksAgo) => PastPosition(ticksAgo) + Player.Size * 0.5f;

        public int PastLife(int ticksAgo) => count == 0 ? Player.statLife : lives[Index(ticksAgo)];

        public void Clear()
        {
            head = 0;
            count = 0;
        }
    }

    public class SmallEvent20 : HorrorEvent
    {
        public override string Name => "small_event20";
        public override HorrorEventCategory Category => HorrorEventCategory.Small;

        public override void Trigger()
        {
            if (Main.netMode == NetmodeID.MultiplayerClient)
                return;

            Player player = HorrorScheduler.AnyPlayer();
            if (player == null)
                return;

            string[] texts = { "turn around", "it's behind the door", player.name, "i see you", "don't sleep" };

            int cx = (int)(player.Center.X / 16f);
            int cy = (int)(player.Center.Y / 16f);
            int radius = 60;

            List<(int Index, string Original, string Fake)> changed = new List<(int, string, string)>();

            for (int i = 0; i < Main.sign.Length; i++)
            {
                Sign sign = Main.sign[i];
                if (sign == null)
                    continue;
                if (Math.Abs(sign.x - cx) > radius || Math.Abs(sign.y - cy) > radius)
                    continue;

                string fake = texts[Main.rand.Next(texts.Length)];
                changed.Add((i, sign.text, fake));
                sign.text = fake;
                Sync(i);
            }

            if (changed.Count == 0)
                return;

            HorrorScheduler.In(3600, () =>
            {
                foreach ((int index, string original, string fake) in changed)
                {
                    Sign sign = Main.sign[index];
                    if (sign == null || sign.text != fake)
                        continue;

                    sign.text = original;
                    Sync(index);
                }
            });
        }

        private static void Sync(int index)
        {
            if (Main.netMode == NetmodeID.Server)
                NetMessage.SendData(MessageID.ReadSign, -1, -1, null, index);
        }
    }

    public class MediumEvent11 : HorrorEvent
    {
        public override string Name => "medium_event11";
        public override HorrorEventCategory Category => HorrorEventCategory.Medium;

        public override void Trigger()
        {
            if (Main.dedServ)
                return;

            HorrorWatcherSystem.Begin();
        }
    }

    public class HorrorWatcherSystem : ModSystem
    {
        private static readonly float[] Distances = { 900f, 720f, 540f, 380f };

        private static bool active;
        private static bool visible;
        private static bool final;
        private static int stage;
        private static int wait;
        private static int shown;
        private static int stare;
        private static float alpha;
        private static Vector2 feet;

        public static void Begin()
        {
            if (active)
                return;

            active = true;
            visible = false;
            final = false;
            stage = 0;
            wait = 60;
        }

        private static void Stop()
        {
            active = false;
            visible = false;
            final = false;
            stage = 0;
        }

        private static void Appear(Player player)
        {
            stage++;
            shown = 0;
            stare = 0;
            alpha = 0f;
            visible = true;

            if (stage > Distances.Length)
            {
                final = true;
                alpha = 1f;
                feet = new Vector2(player.Center.X - player.direction * 40f, player.Bottom.Y);
                SoundEngine.PlaySound(new SoundStyle("flameout/Content/Sounds/hrr2"), player.Center);
                return;
            }

            int side = Main.rand.NextBool() ? 1 : -1;
            feet = Ground(player, player.Center.X + side * Distances[stage - 1]);
        }

        private static Vector2 Ground(Player player, float worldX)
        {
            int tx = Math.Clamp((int)(worldX / 16f), 5, Main.maxTilesX - 5);
            int ty = (int)(player.Bottom.Y / 16f) - 12;

            for (int i = 0; i < 40; i++)
            {
                int y = ty + i;
                if (y < 5 || y >= Main.maxTilesY - 5)
                    continue;

                if (WorldGen.SolidTile(tx, y) && !WorldGen.SolidTile(tx, y - 1) &&
                    !WorldGen.SolidTile(tx, y - 2) && !WorldGen.SolidTile(tx, y - 3))
                    return new Vector2(tx * 16f + 8f, y * 16f);
            }

            return new Vector2(worldX, player.Bottom.Y);
        }

        private static void Vanish()
        {
            visible = false;
            wait = Main.rand.Next(900, 1800);
        }

        public override void PostUpdateEverything()
        {
            if (Main.dedServ || !active)
                return;

            Player player = Main.LocalPlayer;

            if (!visible)
            {
                if (!player.dead && --wait <= 0)
                    Appear(player);
                return;
            }

            shown++;
            alpha = Math.Min(1f, alpha + 0.03f);

            if (final)
            {
                if (shown >= 40)
                    Stop();
                return;
            }

            float dx = feet.X - player.Center.X;
            bool facing = Math.Sign(dx) == player.direction;
            bool approaching = facing && Math.Abs(player.velocity.X) > 1f;

            if (facing)
                stare++;

            if (player.dead || Math.Abs(dx) < 160f || stare > 45 || (approaching && shown > 20) || shown > 900)
                Vanish();
        }

        public override void PostDrawTiles()
        {
            if (!active || !visible || alpha <= 0f)
                return;

            Texture2D tex = ModContent.Request<Texture2D>("flameout/Content/hrrghost", AssetRequestMode.ImmediateLoad).Value;
            SpriteEffects fx = feet.X > Main.LocalPlayer.Center.X ? SpriteEffects.FlipHorizontally : SpriteEffects.None;

            Main.spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, Main.DefaultSamplerState,
                DepthStencilState.None, Main.Rasterizer, null, Main.GameViewMatrix.TransformationMatrix);
            Main.spriteBatch.Draw(tex, feet - Main.screenPosition, null, Color.Black * alpha, 0f,
                new Vector2(tex.Width * 0.5f, tex.Height), 1.5f, fx, 0f);
            Main.spriteBatch.End();
        }

        public override void OnWorldUnload() => Stop();
    }

    public class MediumEvent12 : HorrorEvent
    {
        public override string Name => "medium_event12";
        public override HorrorEventCategory Category => HorrorEventCategory.Medium;

        public override void Trigger()
        {
            if (Main.netMode == NetmodeID.MultiplayerClient)
                return;

            Player player = HorrorScheduler.AnyPlayer();
            if (player == null)
                return;

            int type = ModContent.NPCType<HorrorAngel>();
            if (NPC.AnyNPCs(type))
                return;

            Vector2 at = player.Center + new Vector2(-player.direction * 800f, -60f);
            NPC.NewNPC(new EntitySource_Misc("horror_angel"), (int)at.X, (int)at.Y, type, 0, 0f, 0f, 0f, 0f, player.whoAmI);
        }
    }

    public class HorrorAngel : ModNPC
    {
        public override string Texture => "flameout/Content/hrrghost";

        private const int Lifetime = 4800;

        public override void SetStaticDefaults()
        {
            NPCID.Sets.NPCBestiaryDrawModifiers hide = new NPCID.Sets.NPCBestiaryDrawModifiers() { Hide = true };
            NPCID.Sets.NPCBestiaryDrawOffset.Add(Type, hide);
        }

        public override void SetDefaults()
        {
            NPC.width = 32;
            NPC.height = 32;
            NPC.aiStyle = -1;
            NPC.damage = 30;
            NPC.defense = 0;
            NPC.lifeMax = 1;
            NPC.dontTakeDamage = true;
            NPC.knockBackResist = 0f;
            NPC.noGravity = true;
            NPC.noTileCollide = true;
            NPC.friendly = false;
            NPC.npcSlots = 0f;
        }

        public override bool CheckActive() => false;

        public override void AI()
        {
            NPC.TargetClosest(false);
            Player target = Main.player[NPC.target];
            NPC.ai[0]++;

            if (!target.active || target.dead || NPC.ai[0] > Lifetime)
            {
                for (int i = 0; i < 20; i++)
                {
                    Dust d = Dust.NewDustDirect(NPC.position, NPC.width, NPC.height, DustID.Smoke, 0f, 0f, 110, new Color(15, 15, 15), 1.4f);
                    d.noGravity = true;
                }

                NPC.active = false;
                if (Main.netMode == NetmodeID.Server)
                    NetMessage.SendData(MessageID.SyncNPC, -1, -1, null, NPC.whoAmI);
                return;
            }

            float dx = NPC.Center.X - target.Center.X;
            bool watched = Math.Sign(dx) == target.direction;
            float state = watched ? 1f : 0f;

            if (NPC.ai[1] != state)
            {
                NPC.ai[1] = state;
                NPC.netUpdate = true;
            }

            if (watched)
            {
                NPC.velocity = Vector2.Zero;
                return;
            }

            Vector2 dir = target.Center - NPC.Center;
            if (dir != Vector2.Zero)
                dir.Normalize();

            NPC.velocity = dir * 9f;

            int face = Math.Sign(NPC.velocity.X);
            if (face != 0)
            {
                NPC.direction = face;
                NPC.spriteDirection = face;
            }

            if (!Main.dedServ && Main.rand.NextBool(40))
                SoundEngine.PlaySound(SoundID.Dig with { Volume = 0.4f, Pitch = -0.6f }, NPC.Center);
        }

        public override void OnHitPlayer(Player target, Player.HurtInfo hurtInfo)
        {
            NPC.Center = target.Center + Main.rand.NextVector2CircularEdge(720f, 720f);
            NPC.velocity = Vector2.Zero;
            NPC.netUpdate = true;
        }

        public override Color? GetAlpha(Color drawColor) => new Color(8, 8, 8);
    }

    public class MediumEvent13 : HorrorEvent
    {
        public override string Name => "medium_event13";
        public override HorrorEventCategory Category => HorrorEventCategory.Medium;

        public override void Trigger()
        {
            if (Main.dedServ)
                return;

            HorrorFakeDeathSystem.Begin();
        }
    }

    public class HorrorFakeDeathSystem : ModSystem
    {
        private const int Duration = 360;

        private static int ticks;
        private static float savedMusic;

        public static bool Active => ticks > 0;

        public static void Begin()
        {
            Player player = Main.LocalPlayer;
            if (ticks > 0 || player.dead)
                return;

            ticks = Duration;
            savedMusic = Main.musicVolume;
            Main.musicVolume = 0f;

            SoundEngine.PlaySound(SoundID.PlayerKilled, player.Center);

            for (int i = 0; i < 40; i++)
            {
                Dust d = Dust.NewDustDirect(player.position, player.width, player.height, DustID.Blood, 0f, 0f, 0, default, 1.4f);
                d.velocity = Main.rand.NextVector2Circular(5f, 5f);
            }
        }

        public override void PostUpdateEverything()
        {
            if (Main.dedServ || ticks <= 0)
                return;

            ticks--;
            Main.musicVolume = 0f;

            if (ticks > 0)
                return;

            Main.musicVolume = savedMusic;
            HorrorScheduler.In(90, () => Main.NewText("<null> not yet", new Color(150, 150, 150)));
        }

        public override void OnWorldUnload()
        {
            if (ticks > 0)
                Main.musicVolume = savedMusic;
            ticks = 0;
        }

        public override void PostDrawInterface(SpriteBatch spriteBatch)
        {
            if (ticks <= 0)
                return;

            spriteBatch.Draw(TextureAssets.MagicPixel.Value,
                new Rectangle(0, 0, Main.screenWidth, Main.screenHeight), Color.Black * 0.45f);

            Color red = new Color(225, 25, 25);
            Vector2 center = new Vector2(Main.screenWidth * 0.5f, Main.screenHeight * 0.5f);

            Utils.DrawBorderStringBig(spriteBatch, "You were slain by null.", center - new Vector2(0f, 40f), red, 1f, 0.5f, 0.5f);
            Utils.DrawBorderStringBig(spriteBatch, (ticks / 60 + 1).ToString(), center + new Vector2(0f, 20f), red, 1f, 0.5f, 0.5f);
        }
    }

    public class HorrorFakeDeathPlayer : ModPlayer
    {
        private bool Mine => HorrorFakeDeathSystem.Active && Player.whoAmI == Main.myPlayer;

        public override void SetControls()
        {
            if (!Mine)
                return;

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
            Player.controlQuickHeal = false;
            Player.controlQuickMana = false;
        }

        public override void PostUpdate()
        {
            if (!Mine)
                return;

            Player.immune = true;
            Player.immuneNoBlink = true;
            Player.immuneTime = Math.Max(Player.immuneTime, 2);
            Player.velocity.X = 0f;
        }

        public override void HideDrawLayers(PlayerDrawSet drawInfo)
        {
            if (!Mine)
                return;

            foreach (PlayerDrawLayer layer in PlayerDrawLayerLoader.Layers)
                layer.Hide();
        }
    }

    public class MediumEvent14 : HorrorEvent
    {
        public override string Name => "medium_event14";
        public override HorrorEventCategory Category => HorrorEventCategory.Medium;

        private const int Rewind = 600;

        public override void Trigger()
        {
            if (Main.dedServ)
                return;

            Jump(Main.rand.NextBool(3));
        }

        private static void Jump(bool again)
        {
            Player player = Main.LocalPlayer;
            if (player.dead)
                return;

            HorrorTrailPlayer trail = player.GetModPlayer<HorrorTrailPlayer>();
            if (!trail.Has(Rewind))
                return;

            Vector2 pos = trail.PastPosition(Rewind);
            int life = trail.PastLife(Rewind);

            SoundEngine.PlaySound(SoundID.Item6 with { Pitch = -0.6f }, player.Center);

            player.position = pos;
            player.velocity = Vector2.Zero;
            player.fallStart = (int)(pos.Y / 16f);
            player.statLife = Math.Clamp(life, 1, player.statLifeMax2);

            if (Main.netMode == NetmodeID.MultiplayerClient)
                NetMessage.SendData(MessageID.PlayerLifeMana, -1, -1, null, player.whoAmI);

            Main.instance.CameraModifiers.Add(new PunchCameraModifier(
                player.Center, Main.rand.NextVector2Unit(), 10f, 6f, 20, 2000f, "horror:DejaVu"));

            trail.Clear();

            if (again)
                HorrorScheduler.In(Rewind + 20, () => Jump(false));
        }
    }

    public class MediumEvent15 : HorrorEvent
    {
        public override string Name => "medium_event15";
        public override HorrorEventCategory Category => HorrorEventCategory.Medium;

        public override void Trigger()
        {
            if (Main.dedServ)
                return;

            Main.LocalPlayer.GetModPlayer<HorrorHandsPlayer>().Begin(1200);
        }
    }

    public class HorrorHandsPlayer : ModPlayer
    {
        private int ticks;

        public void Begin(int duration)
        {
            if (ticks <= 0)
                ticks = duration;
        }

        public override void SetControls()
        {
            if (ticks <= 0)
                return;

            ticks--;

            if (ticks == 0)
            {
                Player.selectedItem = Useless();
                return;
            }

            if (Player.itemAnimation > 0)
                return;

            if (ticks % 90 == 0)
                Player.selectedItem = Main.rand.Next(10);

            if (Main.rand.NextBool(120) && SafeToSwing())
                Player.controlUseItem = true;
        }

        private bool SafeToSwing()
        {
            Item held = Player.HeldItem;
            if (held == null || held.IsAir || held.damage <= 0 || held.consumable || held.ammo != 0)
                return false;
            if (held.pick > 0 || held.axe > 0 || held.hammer > 0)
                return false;

            for (int i = 0; i < Main.maxNPCs; i++)
            {
                NPC npc = Main.npc[i];
                if (npc.active && Vector2.Distance(npc.Center, Player.Center) < 320f)
                    return false;
            }

            return true;
        }

        private int Useless()
        {
            int best = Player.selectedItem;
            int bestValue = int.MaxValue;

            for (int i = 0; i < 10; i++)
            {
                Item item = Player.inventory[i];
                if (item == null || item.IsAir || item.value >= bestValue)
                    continue;

                best = i;
                bestValue = item.value;
            }

            return best;
        }
    }

    public class GlobalEvent9 : HorrorEvent
    {
        public override string Name => "global_event9";
        public override HorrorEventCategory Category => HorrorEventCategory.Global;

        public override void Trigger()
        {
            if (Main.dedServ)
                return;

            HorrorEchoSystem.Begin();
        }
    }

    public class HorrorEchoSystem : ModSystem
    {
        private const int RecordTime = 18000;
        private const int ChaseTime = 14400;

        private struct Frame
        {
            public Vector2 Position;
            public int Direction;
            public Rectangle Body;
            public Rectangle Legs;
            public Rectangle Head;
        }

        private static readonly List<Frame> frames = new();

        private static int phase;
        private static int timer;
        private static float cursor;
        private static float savedMusic;

        public static void Begin()
        {
            if (phase != 0)
                return;

            frames.Clear();
            phase = 1;
            timer = RecordTime;
            cursor = 0f;
        }

        private static void StartChase()
        {
            phase = 2;
            timer = ChaseTime;
            cursor = 0f;
            savedMusic = Main.musicVolume;
            Main.musicVolume = 0f;

            SoundEngine.PlaySound(new SoundStyle("flameout/Content/Sounds/hrr1"));
            Main.NewText("<null> my turn", new Color(150, 150, 150));
        }

        private static void Stop()
        {
            if (phase == 2)
                Main.musicVolume = savedMusic;

            phase = 0;
            frames.Clear();
        }

        public override void PostUpdateEverything()
        {
            if (Main.dedServ || phase == 0)
                return;

            Player player = Main.LocalPlayer;

            frames.Add(new Frame
            {
                Position = player.position,
                Direction = player.direction,
                Body = player.bodyFrame,
                Legs = player.legFrame,
                Head = player.headFrame
            });

            timer--;

            if (phase == 1)
            {
                if (timer <= 0)
                    StartChase();
                return;
            }

            Main.musicVolume = 0f;

            float progress = 1f - timer / (float)ChaseTime;
            cursor = Math.Min(cursor + MathHelper.Lerp(1f, 3f, progress), frames.Count - 1);

            Frame ghost = frames[(int)cursor];

            if (!player.dead && Vector2.Distance(ghost.Position, player.position) < 20f)
            {
                SoundEngine.PlaySound(new SoundStyle("flameout/Content/Sounds/hrr2"));
                Stop();
                WorldGen.SaveAndQuit();
                return;
            }

            if (timer <= 0)
            {
                SoundEngine.PlaySound(SoundID.Item104 with { Pitch = -0.9f });
                Stop();
            }
        }

        public override void PostDrawTiles()
        {
            if (phase != 2 || frames.Count == 0)
                return;

            Player player = Main.LocalPlayer;
            Frame ghost = frames[Math.Clamp((int)cursor, 0, frames.Count - 1)];

            int dir = player.direction;
            Rectangle body = player.bodyFrame;
            Rectangle legs = player.legFrame;
            Rectangle head = player.headFrame;

            player.direction = ghost.Direction;
            player.bodyFrame = ghost.Body;
            player.legFrame = ghost.Legs;
            player.headFrame = ghost.Head;

            try
            {
                Main.spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, Main.DefaultSamplerState,
                    DepthStencilState.None, Main.Rasterizer, null, Main.GameViewMatrix.TransformationMatrix);
                Main.PlayerRenderer.DrawPlayer(Main.Camera, player, ghost.Position, 0f, Vector2.Zero, 0.35f);
                Main.spriteBatch.End();
            }
            finally
            {
                player.direction = dir;
                player.bodyFrame = body;
                player.legFrame = legs;
                player.headFrame = head;
            }
        }

        public override void PreSaveAndQuit()
        {
            if (phase != 0)
                Stop();
        }

        public override void OnWorldUnload()
        {
            if (phase != 0)
                Stop();
        }

        public override void ModifyInterfaceLayers(List<GameInterfaceLayer> layers)
        {
            if (phase != 2)
                return;

            layers.Add(new LegacyGameInterfaceLayer("flameout: Echo Timer", () =>
            {
                int seconds = Math.Max(0, timer) / 60;
                string text = (seconds / 60) + ":" + (seconds % 60).ToString("00");

                float urgency = 1f - timer / (float)ChaseTime;
                Color color = Color.Lerp(new Color(190, 190, 190), new Color(255, 40, 40), urgency);

                Utils.DrawBorderStringBig(Main.spriteBatch, text,
                    new Vector2(Main.screenWidth * 0.5f, 60f), color, 1.4f + urgency * 0.6f, 0.5f, 0.5f);

                return true;
            }, InterfaceScaleType.UI));
        }
    }

    public class GlobalEvent10 : HorrorEvent
    {
        public override string Name => "global_event10";
        public override HorrorEventCategory Category => HorrorEventCategory.Global;

        public override void Trigger()
        {
            if (Main.dedServ)
                return;

            HorrorNullSystem.Begin();
        }
    }

    public class HorrorNullSystem : ModSystem
    {
        private const string NullName = "null";
        private const int StageDelay = 2400;
        private const int HoldTime = 3600;
        private const int ReturnDelay = 600;
        private const int Layers = 5;

        private static int level;
        private static int phase;
        private static int timer;
        private static int chatTimer;

        private static readonly Dictionary<int, (int Type, string Name)> npcNames = new();
        private static readonly List<Item> renamed = new();
        private static string savedTitle;
        private static string savedName;

        public static bool PlayerRenamed => level >= 5 && savedName != null;
        public static string SavedName => savedName;

        public static void Begin()
        {
            if (phase != 0)
                return;

            phase = 1;
            level = 0;
            timer = StageDelay;
            chatTimer = 0;
        }

        private static void Raise()
        {
            level++;

            switch (level)
            {
                case 1:
                    for (int i = 0; i < Main.maxNPCs; i++)
                    {
                        NPC npc = Main.npc[i];
                        if (!npc.active || !npc.townNPC)
                            continue;

                        npcNames[i] = (npc.type, npc.GivenName);
                        npc.GivenName = NullName;
                    }
                    break;

                case 2:
                    Player owner = Main.LocalPlayer;
                    for (int i = 0; i < owner.inventory.Length; i++)
                    {
                        Item item = owner.inventory[i];
                        if (item == null || item.IsAir)
                            continue;

                        item.SetNameOverride(NullName);
                        renamed.Add(item);
                    }
                    break;

                case 3:
                    savedTitle = Main.instance.Window.Title;
                    Main.instance.Window.Title = NullName;
                    break;

                case 4:
                    chatTimer = 60;
                    break;

                case 5:
                    savedName = Main.LocalPlayer.name;
                    Main.LocalPlayer.name = NullName;
                    break;
            }

            SoundEngine.PlaySound(SoundID.Item104 with { Pitch = -0.9f });
        }

        private static void Lower()
        {
            switch (level)
            {
                case 5:
                    if (savedName != null && Main.LocalPlayer.name == NullName)
                        Main.LocalPlayer.name = savedName;
                    savedName = null;
                    break;

                case 3:
                    if (savedTitle != null)
                        Main.instance.Window.Title = savedTitle;
                    savedTitle = null;
                    break;

                case 2:
                    foreach (Item item in renamed)
                    {
                        if (item != null && !item.IsAir)
                            item.ClearNameOverride();
                    }
                    renamed.Clear();
                    break;

                case 1:
                    foreach (KeyValuePair<int, (int Type, string Name)> entry in npcNames)
                    {
                        NPC npc = Main.npc[entry.Key];
                        if (npc.active && npc.type == entry.Value.Type && npc.GivenName == NullName)
                            npc.GivenName = entry.Value.Name;
                    }
                    npcNames.Clear();
                    break;
            }

            level--;
        }

        private static void ResetAll()
        {
            if (Main.dedServ)
                return;

            while (level > 0)
                Lower();

            phase = 0;
            timer = 0;
        }

        private static void Whisper()
        {
            string who = savedName ?? Main.LocalPlayer.name;

            string[] lines =
            {
                "null",
                "who is " + who.ToLower(),
                "you were never here",
                "hello null",
                "this world is null",
                who.ToLower() + " is null"
            };

            Main.NewText("<null> " + lines[Main.rand.Next(lines.Length)], new Color(150, 150, 150));
        }

        public override void PostUpdateEverything()
        {
            if (Main.dedServ || phase == 0)
                return;

            if (level >= 3 && Main.instance.Window.Title != NullName)
                Main.instance.Window.Title = NullName;

            if (level >= 4 && --chatTimer <= 0)
            {
                Whisper();
                chatTimer = Main.rand.Next(600, 1200);
            }

            if (--timer > 0)
                return;

            switch (phase)
            {
                case 1:
                    Raise();
                    if (level >= Layers)
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
                    Lower();
                    if (level <= 0)
                        phase = 0;
                    else
                        timer = ReturnDelay;
                    break;
            }
        }

        public override void PreSaveAndQuit() => ResetAll();

        public override void OnWorldUnload() => ResetAll();

        public override void Unload() => ResetAll();
    }

    public class HorrorNullPlayer : ModPlayer
    {
        public override void PreSavePlayer()
        {
            if (Player.whoAmI == Main.myPlayer && HorrorNullSystem.PlayerRenamed)
                Player.name = HorrorNullSystem.SavedName;
        }

        public override void PostSavePlayer()
        {
            if (Player.whoAmI == Main.myPlayer && HorrorNullSystem.PlayerRenamed)
                Player.name = "null";
        }
    }

    public class GlobalEvent11 : HorrorEvent
    {
        public override string Name => "global_event11";
        public override HorrorEventCategory Category => HorrorEventCategory.Global;

        public override void Trigger()
        {
            if (Main.netMode == NetmodeID.MultiplayerClient)
                return;

            HorrorBloodNightSystem.Begin();
        }
    }

    public class HorrorBloodNightSystem : ModSystem
    {
        private const int Duration = 18000;
        private const double Midnight = 16200.0;

        private static int remaining;
        private static int nextEvent;
        private static double savedTime;
        private static bool savedDay;
        private static bool savedBlood;

        private static readonly Func<HorrorEvent>[] Pool =
        {
            () => new SmallEvent1(),
            () => new SmallEvent2(),
            () => new SmallEvent3(),
            () => new SmallEvent5(),
            () => new SmallEvent6(),
            () => new SmallEvent7(),
            () => new SmallEvent8(),
            () => new SmallEvent10(),
            () => new SmallEvent11(),
            () => new SmallEvent12(),
            () => new SmallEvent13(),
            () => new SmallEvent14(),
            () => new SmallEvent15(),
            () => new SmallEvent16(),
            () => new SmallEvent17(),
            () => new SmallEvent18(),
            () => new SmallEvent19(),
            () => new SmallEvent20()
        };

        public static bool Running => remaining > 0;

        public static void Begin()
        {
            if (remaining > 0)
                return;

            savedTime = Main.time;
            savedDay = Main.dayTime;
            savedBlood = Main.bloodMoon;

            remaining = Duration;
            nextEvent = 1800;

            Apply();
            Sync();

            Color color = new Color(50, 255, 130);
            if (Main.netMode == NetmodeID.Server)
                ChatHelper.BroadcastChatMessage(NetworkText.FromKey("LegacyMisc.8"), color);
            else
                Main.NewText(Language.GetTextValue("LegacyMisc.8"), color);

            if (!Main.dedServ)
                SoundEngine.PlaySound(SoundID.Roar);
        }

        private static void Apply()
        {
            Main.dayTime = false;
            Main.time = Midnight;
            Main.bloodMoon = true;
        }

        private static void Sync()
        {
            if (Main.netMode == NetmodeID.Server)
                NetMessage.SendData(MessageID.WorldData);
        }

        private static void End()
        {
            remaining = 0;
            Main.time = savedTime;
            Main.dayTime = savedDay;
            Main.bloodMoon = savedBlood;
            Sync();
        }

        public override void PostUpdateEverything()
        {
            if (Main.netMode == NetmodeID.MultiplayerClient || remaining <= 0)
                return;

            remaining--;
            Apply();

            if (remaining % 120 == 0)
                Sync();

            if (--nextEvent <= 0)
            {
                nextEvent = (int)MathHelper.Lerp(600f, 1800f, remaining / (float)Duration);
                Pool[Main.rand.Next(Pool.Length)]().Trigger();
            }

            if (remaining <= 0)
                End();
        }

        public override void PreSaveAndQuit()
        {
            if (remaining > 0)
                End();
        }

        public override void OnWorldUnload() => remaining = 0;
    }

}


