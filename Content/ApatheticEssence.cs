using Terraria;
using Terraria.DataStructures;
using Terraria.GameContent.Creative;
using Terraria.ID;
using Terraria.ModLoader;

namespace flameout.Content
{
    public class ApatheticEssence : ModItem
    {
        public override void SetStaticDefaults()
        {
            Main.RegisterItemAnimation(Type, new DrawAnimationVertical(6, 4));
            ItemID.Sets.AnimatesAsSoul[Type] = true;
            CreativeItemSacrificesCatalog.Instance.SacrificeCountNeededByItemId[Type] = 25;
        }

        public override void SetDefaults()
        {
            Item.width = 22;
            Item.height = 22;
            Item.maxStack = Item.CommonMaxStack;
            Item.rare = ItemRarityID.LightPurple;
            Item.value = Item.sellPrice(silver: 20);
        }
    }
}
