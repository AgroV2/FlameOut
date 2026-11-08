using Terraria;
using Terraria.GameContent.Creative;
using Terraria.GameContent.ItemDropRules;
using Terraria.ID;
using Terraria.ModLoader;

namespace flameout.Content
{
	public class TreasureBagApatheticSpirit : ModItem
	{
		public const int ShardsMin = 15;
		public const int ShardsMax = 20;

		public override void SetStaticDefaults()
		{
			ItemID.Sets.BossBag[Type] = true;
			ItemID.Sets.PreHardmodeLikeBossBag[Type] = false;

			CreativeItemSacrificesCatalog.Instance.SacrificeCountNeededByItemId[Type] = 3;
		}

		public override void SetDefaults()
		{
			Item.maxStack = Item.CommonMaxStack;
			Item.consumable = true;
			Item.width = 32;
			Item.height = 32;
			Item.rare = ItemRarityID.Expert;
			Item.expert = true;
		}

		public override bool CanRightClick() => true;

		public override void ModifyItemLoot(ItemLoot itemLoot)
		{
			itemLoot.Add(ItemDropRule.Common(ModContent.ItemType<ApatheticShards>(), 1, ShardsMin, ShardsMax));
			itemLoot.Add(ItemDropRule.Common(ModContent.ItemType<ApatheticRapiers>()));
			itemLoot.Add(ItemDropRule.Common(ModContent.ItemType<ApatheticCellStaff>()));
			itemLoot.Add(ItemDropRule.Common(ModContent.ItemType<ApatheticRelic>()));
		}
	}


}