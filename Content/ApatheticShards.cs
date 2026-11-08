using Terraria;
using Terraria.GameContent.Creative;
using Terraria.ID;
using Terraria.ModLoader;

namespace flameout.Content
{
	public class ApatheticShards : ModItem
	{
		public override void SetStaticDefaults()
		{
			CreativeItemSacrificesCatalog.Instance.SacrificeCountNeededByItemId[Type] = 25;

			
			ItemID.Sets.ItemIconPulse[Type] = true;
			ItemID.Sets.ItemNoGravity[Type] = false;
		}

		public override void SetDefaults()
		{
			Item.width = 20;
			Item.height = 20;
			Item.maxStack = Item.CommonMaxStack;
			Item.value = Item.sellPrice(silver: 50);
			Item.rare = ItemRarityID.Pink;
			Item.material = true;
		}

		public override void PostUpdate()
		{
			
			Lighting.AddLight(Item.Center, 0.4f, 0.15f, 0.6f);
		}
	}
}