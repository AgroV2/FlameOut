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
        public override int Width => 150;
        public override int Height => 1000;
        public override bool ShouldSave => false;
        public override bool NoPlayerSaving => true;

        public override void OnEnter()
        {
            
            Main.worldSurface = 0;
            Main.rockLayer = 0;

            
            Main.spawnTileX = 75;
            Main.spawnTileY = 945;
        }

        public override void OnExit()
        {
            
            
            
            Main.worldSurface = Main.maxTilesY * 0.3;
            Main.rockLayer = Main.maxTilesY * 0.5;
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

            
            Main.spawnTileX = 75;
            Main.spawnTileY = 945;
            Main.worldSurface = 0;
            Main.rockLayer = 0;

            for (int x = 0; x < 150; x++)
            {
                for (int y = 0; y < 1000; y++)
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

    public class HorrorPlayer : ModPlayer
    {
        private bool _needTeleport = false;

        public override void OnEnterWorld()
        {
            
            _needTeleport = true;
        }

        public override void PostUpdate()
        {
            
            if (_needTeleport && SubworldSystem.IsActive<HorrorSubworld>())
            {
                Player.Center = new Vector2(75 * 16, 940 * 16);
                Player.velocity = Vector2.Zero;
                _needTeleport = false;
            }

            
            if (_needTeleport && !SubworldSystem.IsActive<HorrorSubworld>())
            {
                _needTeleport = false;
            }
        }
    }

    public class HorrorSubworldSceneEffect : ModSceneEffect
    {
        public override SceneEffectPriority Priority => SceneEffectPriority.BossHigh;
        public override int Music => MusicLoader.GetMusicSlot(Mod, "Content/Music/hrrm3");

        


        public override bool IsSceneEffectActive(Player player) => SubworldSystem.IsActive<HorrorSubworld>();


    }
}
