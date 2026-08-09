

using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using SubworldLibrary;
using Terraria.ModLoader;
using Terraria.WorldBuilding;
using Terraria.IO;

namespace flameout.Content
{
    public class HorrorSubworld : Subworld
    {
        public const int SubWidth = 150;
        public const int SubHeight = 1000;
        public const int SpawnX = 75;
        public const int SpawnY = 945;

        public override int Width => SubWidth;
        public override int Height => SubHeight;
        public override bool ShouldSave => false;
        public override bool NoPlayerSaving => true;

        public override void OnEnter()
        {
        }

        public override void OnLoad()
        {
            ApplyAbyssLayers();
        }

        internal static void ApplyAbyssLayers()
        {
            Main.worldSurface = 0;
            Main.rockLayer = 0;
            Main.spawnTileX = SpawnX;
            Main.spawnTileY = SpawnY;
        }

        public override List<GenPass> Tasks => new List<GenPass>()
        {
            new AbyssGenPass()
        };
    }

    public class AbyssGenPass : GenPass
    {
        public AbyssGenPass() : base("AbyssGeneration", 1f) { }

        protected override void ApplyPass(GenerationProgress progress, GameConfiguration configuration)
        {
            progress.Message = "Creating the Abyss...";

            HorrorSubworld.ApplyAbyssLayers();

            for (int x = 0; x < HorrorSubworld.SubWidth; x++)
            {
                for (int y = 0; y < HorrorSubworld.SubHeight; y++)
                {
                    Tile tile = Main.tile[x, y];
                    if (x < 40 || x >= 110 || y >= 950)
                    {
                        tile.HasTile = true;
                        tile.TileType = TileID.Stone;
                    }
                    else
                    {
                        tile.HasTile = false;
                        tile.WallType = WallID.Stone;
                    }
                }
            }

            for (int px = 70; px <= 80; px++)
            {
                Tile platformTile = Main.tile[px, 32];
                platformTile.HasTile = true;
                platformTile.TileType = TileID.Stone;
            }

            ushort handsTile = (ushort)ModContent.TileType<HandsTile>();
            for (int y = 60; y < 940; y += 15)
            {
                for (int dx = 0; dx < 2; dx++)
                    for (int dy = 0; dy < 2; dy++)
                    {
                        Tile t = Main.tile[40 + dx, y + dy];
                        t.HasTile = true;
                        t.TileType = handsTile;
                        t.TileFrameX = (short)(dx * 18);
                        t.TileFrameY = (short)(dy * 18);
                    }

                    for (int dx = 0; dx < 2; dx++)
                        for (int dy = 0; dy < 2; dy++)
                        {
                            Tile t = Main.tile[108 + dx, y + dy];
                            t.HasTile = true;
                            t.TileType = handsTile;
                            t.TileFrameX = (short)(dx * 18);
                            t.TileFrameY = (short)(dy * 18);
                        }
            }

            ushort portalTile = (ushort)ModContent.TileType<PortalTile>();
            WorldGen.PlaceObject(74, 30, portalTile);
        }
    }

    public class MainWorldLayerRepair : ModSystem
    {
        public override void OnWorldLoad()
        {
            if (SubworldSystem.AnyActive())
                return;

            bool layersBroken = Main.worldSurface < 1
            || Main.rockLayer < 1
            || Main.rockLayer <= Main.worldSurface;

            if (!layersBroken)
                return;

            double oldSurface = Main.worldSurface;
            double oldRock = Main.rockLayer;

            Main.worldSurface = Main.maxTilesY * 0.3;
            Main.rockLayer = Main.maxTilesY * 0.5;

            bool spawnRepaired = RepairSpawnIfNeeded();

            Mod.Logger.Warn(
                $"Main world layers were corrupted (worldSurface={oldSurface}, rockLayer={oldRock}). " +
                $"Repaired to worldSurface={Main.worldSurface}, rockLayer={Main.rockLayer}" +
                (spawnRepaired ? $"; spawn moved to {Main.spawnTileX}/{Main.spawnTileY}." : "."));
        }

        private static bool RepairSpawnIfNeeded()
        {
            bool looksLikeSubworldSpawn = Main.spawnTileX == HorrorSubworld.SpawnX
            && Main.spawnTileY == HorrorSubworld.SpawnY;

            bool outOfBounds = Main.spawnTileX < 10
            || Main.spawnTileX > Main.maxTilesX - 10
            || Main.spawnTileY < 10
            || Main.spawnTileY > Main.maxTilesY - 10;

            if (!looksLikeSubworldSpawn && !outOfBounds)
                return false;

            int x = Main.maxTilesX / 2;
            for (int y = 10; y < Main.maxTilesY - 10; y++)
            {
                Tile tile = Main.tile[x, y];
                if (tile.HasTile && Main.tileSolid[tile.TileType] && !Main.tileSolidTop[tile.TileType])
                {
                    Main.spawnTileX = x;
                    Main.spawnTileY = y;
                    return true;
                }
            }

            Main.spawnTileX = x;
            Main.spawnTileY = (int)Main.worldSurface;
            return true;
        }
    }

    public class HorrorPlayer : ModPlayer
    {
        private bool _needTeleport = false;

        public override void OnEnterWorld()
        {
            _needTeleport = true;
        }

        public override void PostUpdate()
        {
            if (!_needTeleport)
                return;

            if (SubworldSystem.IsActive<HorrorSubworld>())
            {
                Player.Center = new Vector2(
                    HorrorSubworld.SpawnX * 16 + 8,
                    (HorrorSubworld.SpawnY - 2) * 16);
                Player.velocity = Vector2.Zero;
            }

            _needTeleport = false;
        }
    }

    public class HorrorSubworldSceneEffect : ModSceneEffect
    {
        public override SceneEffectPriority Priority => SceneEffectPriority.BossHigh;
        public override int Music => MusicLoader.GetMusicSlot(Mod, "Content/Music/hrrm3");

        public override bool IsSceneEffectActive(Player player) => SubworldSystem.IsActive<HorrorSubworld>();
    }
}

