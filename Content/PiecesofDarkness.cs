

using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace flameout.Content
{
    public abstract class PieceOfDarknessItem : ModItem
    {
        public override string Texture => "flameout/Content/PieceOfDarkness";

        public override void SetDefaults()
        {
            Item.width = 20;
            Item.height = 22;
            Item.maxStack = Item.CommonMaxStack;
            Item.material = true;
            Item.rare = ItemRarityID.Purple;
            Item.value = Item.sellPrice(0, 2, 0, 0);
        }
    }

    public class PieceOfDarkness1 : PieceOfDarknessItem { }
    public class PieceOfDarkness2 : PieceOfDarknessItem { }
    public class PieceOfDarkness3 : PieceOfDarknessItem { }
    public class PieceOfDarkness4 : PieceOfDarknessItem { }
    public class PieceOfDarkness5 : PieceOfDarknessItem { }
    public class PieceOfDarkness6 : PieceOfDarknessItem { }
    public class PieceOfDarkness7 : PieceOfDarknessItem { }
    public class PieceOfDarkness8 : PieceOfDarknessItem { }
    public class PieceOfDarkness9 : PieceOfDarknessItem { }
    public class PieceOfDarkness10 : PieceOfDarknessItem { }
    public class PieceOfDarknessC : PieceOfDarknessItem { }
    public class PieceOfDarknessF : PieceOfDarknessItem { }
}

