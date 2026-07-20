using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ModLoader;
using Terraria.ObjectData;
using Terraria.DataStructures;
using SubworldSystem = SubworldLibrary.SubworldSystem;

namespace flameout.Content
{
    public class PortalTile : ModTile
    {
        public override string Texture => "flameout/Content/PortalTile";

        public override void SetStaticDefaults()
        {
            Main.tileFrameImportant[Type] = true;
            Main.tileSolid[Type] = false;

            TileObjectData.newTile.CopyFrom(TileObjectData.Style2x2);

            
            TileObjectData.newTile.AnchorBottom = AnchorData.Empty;
            TileObjectData.newTile.AnchorLeft = AnchorData.Empty;
            TileObjectData.newTile.AnchorRight = AnchorData.Empty;
            TileObjectData.newTile.AnchorTop = AnchorData.Empty;

            TileObjectData.addTile(Type);

            AddMapEntry(new Color(0, 255, 255));
        }

        public override void NearbyEffects(int i, int j, bool closer)
        {
            if (closer)
            {
                Player player = Main.LocalPlayer;
                float dist = Vector2.Distance(player.Center, new Vector2(i * 16f, j * 16f));

                if (dist < 40f && SubworldSystem.IsActive<HorrorSubworld>())
                {
                    SubworldSystem.Exit();
                }
            }
        }
    }
}
