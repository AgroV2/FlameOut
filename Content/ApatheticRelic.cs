	using Terraria;
using Terraria.GameContent.Creative;
using Terraria.GameContent.ItemDropRules;
using Terraria.ID;
using Terraria.ModLoader;
namespace flameout.Content {
    public class ApatheticRelic : ModItem
	{
		public const float DamageBonus = 0.1f;        
		public const float DamageTakenBonus = 0.1f;   
		public const int ApathyArmorPenetration = 5;  

		public override void SetStaticDefaults()
		{
			CreativeItemSacrificesCatalog.Instance.SacrificeCountNeededByItemId[Type] = 1;
		}

		public override void SetDefaults()
		{
			Item.width = 28;
			Item.height = 28;
			Item.accessory = true;
			Item.value = Item.sellPrice(gold: 8);
			Item.rare = ItemRarityID.Expert;
			Item.expert = true;
		}

		public override void UpdateAccessory(Player player, bool hideVisual)
		{
			player.GetDamage(DamageClass.Generic) += DamageBonus;

			player.GetModPlayer<ApatheticRelicPlayer>().Equipped = true;
		}
	}

	public class ApatheticRelicPlayer : ModPlayer
	{
		public bool Equipped;

		public override void ResetEffects()
		{
			Equipped = false;
		}

		
		public override void ModifyHurt(ref Player.HurtModifiers modifiers)
		{
			if (!Equipped)
				return;

			modifiers.FinalDamage *= 1f + ApatheticRelic.DamageTakenBonus;
		}

		
		public override void ModifyHitNPCWithItem(Item item, NPC target, ref NPC.HitModifiers modifiers)
		{
			if (Equipped && IsApatheticItem(item))
				modifiers.ArmorPenetration += ApatheticRelic.ApathyArmorPenetration;
		}

		
		public override void ModifyHitNPCWithProj(Projectile proj, NPC target, ref NPC.HitModifiers modifiers)
		{
			if (!Equipped)
				return;

			if (IsApatheticProjectile(proj) || IsApatheticItem(Player.HeldItem))
				modifiers.ArmorPenetration += ApatheticRelic.ApathyArmorPenetration;
		}

		private bool IsApatheticItem(Item item)
		{
			return item?.ModItem != null
				&& item.ModItem.Mod == Mod
				&& item.ModItem.Name.StartsWith("Apathetic");
		}

		private bool IsApatheticProjectile(Projectile proj)
		{
			return proj.ModProjectile != null
				&& proj.ModProjectile.Mod == Mod
				&& proj.ModProjectile.Name.StartsWith("Apathetic");
		}
	}
}