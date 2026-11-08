using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using SubworldLibrary;
using Terraria;
using Terraria.DataStructures;
using Terraria.GameContent.ItemDropRules;
using Terraria.ID;
using Terraria.IO;
using Terraria.ModLoader;
using Terraria.WorldBuilding;

namespace flameout.Content
{
    public class ApatheticMirrorSubworld : Subworld
    {
        private struct TileSnapshot
        {
            public bool HasTile;
            public ushort TileType;
            public ushort WallType;
            public byte LiquidAmount;
            public int LiquidType;
            public SlopeType Slope;
            public bool IsHalfBlock;
            public short TileFrameX;
            public short TileFrameY;
            public bool RedWire;
            public bool BlueWire;
            public bool GreenWire;
            public bool YellowWire;
            public bool HasActuator;
            public bool IsActuated;
            public byte TileColor;
            public byte WallColor;

            public TileSnapshot(Tile tile)
            {
                HasTile = tile.HasTile;
                TileType = tile.TileType;
                WallType = tile.WallType;
                LiquidAmount = tile.LiquidAmount;
                LiquidType = tile.LiquidType;
                Slope = tile.Slope;
                IsHalfBlock = tile.IsHalfBlock;
                TileFrameX = tile.TileFrameX;
                TileFrameY = tile.TileFrameY;
                RedWire = tile.RedWire;
                BlueWire = tile.BlueWire;
                GreenWire = tile.GreenWire;
                YellowWire = tile.YellowWire;
                HasActuator = tile.HasActuator;
                IsActuated = tile.IsActuated;
                TileColor = tile.TileColor;
                WallColor = tile.WallColor;
            }

            public void Apply(Tile tile)
            {
                tile.ClearEverything();
                tile.HasTile = HasTile;
                tile.TileType = TileType;
                tile.WallType = WallType;
                tile.LiquidAmount = LiquidAmount;
                tile.LiquidType = LiquidType;
                tile.Slope = Slope;
                tile.IsHalfBlock = IsHalfBlock;
                tile.TileFrameX = TileFrameX;
                tile.TileFrameY = TileFrameY;
                tile.RedWire = RedWire;
                tile.BlueWire = BlueWire;
                tile.GreenWire = GreenWire;
                tile.YellowWire = YellowWire;
                tile.HasActuator = HasActuator;
                tile.IsActuated = IsActuated;
                tile.TileColor = TileColor;
                tile.WallColor = WallColor;
            }
        }

        private static TileSnapshot[,] snapshot;
        private static int sourceWidth;
        private static int sourceHeight;
        private static int mirroredPlayerX;
        private static int playerY;
        private static double surface;
        private static double rockLayer;

        public override int Width => sourceWidth > 0 ? sourceWidth : 4200;
        public override int Height => sourceHeight > 0 ? sourceHeight : 1200;
        public override bool ShouldSave => true;
        public override bool NoPlayerSaving => false;

        internal static void Capture(Player player)
        {
            sourceWidth = Main.maxTilesX;
            sourceHeight = Main.maxTilesY;
            surface = Main.worldSurface;
            rockLayer = Main.rockLayer;
            mirroredPlayerX = sourceWidth - 1 - (int)(player.Center.X / 16f);
            playerY = (int)(player.Center.Y / 16f);
            snapshot = new TileSnapshot[sourceWidth, sourceHeight];

            for (int x = 0; x < sourceWidth; x++)
                for (int y = 0; y < sourceHeight; y++)
                    snapshot[x, y] = new TileSnapshot(Main.tile[x, y]);
        }

        public override List<GenPass> Tasks => new() { new MirrorCopyPass() };

        public override void OnLoad()
        {
            Main.worldSurface = surface;
            Main.rockLayer = rockLayer;
            Main.spawnTileX = mirroredPlayerX;
            Main.spawnTileY = playerY;
        }

        private sealed class MirrorCopyPass : GenPass
        {
            public MirrorCopyPass() : base("Mirroring the world", 1f) { }

            protected override void ApplyPass(GenerationProgress progress, GameConfiguration configuration)
            {
                TileSnapshot[,] captured = snapshot;
                int width = sourceWidth;
                int height = sourceHeight;
                if (captured == null || width <= 0 || height <= 0)
                    throw new System.InvalidOperationException("The mirror-world snapshot was not captured before generation.");

                int copyWidth = System.Math.Min(width, Main.maxTilesX);
                int copyHeight = System.Math.Min(height, Main.maxTilesY);
                for (int x = 0; x < copyWidth; x++)
                {
                    progress.Value = x / (float)copyWidth;
                    for (int y = 0; y < copyHeight; y++)
                        captured[width - 1 - x, y].Apply(Main.tile[x, y]);
                }

                Main.worldSurface = surface;
                Main.rockLayer = rockLayer;
                Main.spawnTileX = mirroredPlayerX;
                Main.spawnTileY = playerY;
            }
        }

        internal static bool Active => SubworldSystem.IsActive<ApatheticMirrorSubworld>();
    }

    public class ApatheticMirrorPlayer : ModPlayer
    {
        private bool placePlayer;

        public override void OnEnterWorld() => placePlayer = true;

        public override void PostUpdate()
        {
            if (!placePlayer || !ApatheticMirrorSubworld.Active)
                return;

            placePlayer = false;
            int x = Utils.Clamp(Main.spawnTileX, 10, Main.maxTilesX - 10);
            int y = Utils.Clamp(Main.spawnTileY, 10, Main.maxTilesY - 10);
            while (y > 10 && Collision.SolidCollision(new Vector2(x * 16f, y * 16f), Player.width, Player.height))
                y--;
            Player.Center = new Vector2(x * 16f + 8f, y * 16f);
            Player.velocity = Vector2.Zero;
        }
    }

    public class ApatheticMirrorNPC : GlobalNPC
    {
        public override bool InstancePerEntity => false;

      		internal static bool Allowed(NPC npc) =>
			npc.type == ModContent.NPCType<ApatheticSpirit>() || 
			NPCID.Sets.Zombies[npc.type] ||
			NPCID.Sets.DemonEyes[npc.type] ||
			npc.aiStyle == NPCAIStyleID.Slime;
		public override void EditSpawnPool(IDictionary<int, float> pool, NPCSpawnInfo spawnInfo)
		{
			if (!ApatheticMirrorSubworld.Active)
				return;

			pool.Clear();
			pool[NPCID.Zombie] = 1f;
			pool[NPCID.DemonEye] = 1f;
			pool[NPCID.BlueSlime] = 1f;
			pool[NPCID.GreenSlime] = 0.75f;
			pool[NPCID.RedSlime] = 0.5f;
			pool[NPCID.PurpleSlime] = 0.5f;

		
		}

        public override void EditSpawnRate(Player player, ref int spawnRate, ref int maxSpawns)
        {
            if (!ApatheticMirrorSubworld.Active)
                return;

            spawnRate = System.Math.Min(spawnRate, 300);
            maxSpawns = System.Math.Max(maxSpawns, 8);
        }

        public override bool PreAI(NPC npc)
        {
            if (ApatheticMirrorSubworld.Active && npc.active && !Allowed(npc))
            {
                if (Main.netMode != NetmodeID.MultiplayerClient)
                {
                    npc.active = false;
                    npc.netUpdate = true;
                }
                return false;
            }
            return true;
        }

        public override void ModifyNPCLoot(NPC npc, NPCLoot npcLoot)
        {
            if (npc.type == NPCID.SkeletronHead)
                npcLoot.Add(ItemDropRule.Common(ModContent.ItemType<ApatheticMirror>()));

            if (Allowed(npc))
                npcLoot.Add(ItemDropRule.ByCondition(new MirrorDimensionDropCondition(), ModContent.ItemType<ApatheticEssence>(), 1, 1, 3));
        }

        public override Color? GetAlpha(NPC npc, Color drawColor)
        {
            if (!ApatheticMirrorSubworld.Active || !Allowed(npc))
                return null;
            return Color.Lerp(drawColor, new Color(190, 80, 255), 0.55f);
        }
    }

    public sealed class MirrorDimensionDropCondition : IItemDropRuleCondition
    {
        public bool CanDrop(DropAttemptInfo info) => ApatheticMirrorSubworld.Active && ApatheticMirrorNPC.Allowed(info.npc);
        public bool CanShowItemDropInUI() => false;
        public string GetConditionDescription() => "Drops in the apathetic mirror world";
    }

    public class ApatheticMirrorScene : ModSceneEffect
    {
        public override SceneEffectPriority Priority => SceneEffectPriority.BossHigh;
        public override int Music => MusicLoader.GetMusicSlot(Mod, "Content/Music/ApathyWorld");
     public override bool IsSceneEffectActive(Player player)
{
	return ApatheticMirrorSubworld.Active
		&& !NPC.AnyNPCs(ModContent.NPCType<ApatheticSpirit>());
}
    }

    public class ApatheticMirrorOverlay : ModSystem
    {
        public override void PostDrawInterface(SpriteBatch spriteBatch)
        {
            if (!ApatheticMirrorSubworld.Active || Main.gameMenu)
                return;
            spriteBatch.Draw(Terraria.GameContent.TextureAssets.MagicPixel.Value,
                new Rectangle(0, 0, Main.screenWidth, Main.screenHeight), new Color(90, 0, 130, 38));
        }
    }
}
