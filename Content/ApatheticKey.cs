using Microsoft.Xna.Framework;
using SubworldLibrary;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace flameout.Content
{
    public class ApatheticKey : ModItem
    {
        public override void SetDefaults()
        {
            Item.width = 24;
            Item.height = 26;
            Item.maxStack = 9999;
            Item.useStyle = ItemUseStyleID.HoldUp;
            Item.useTime = 30;
            Item.useAnimation = 30;
            Item.rare = ItemRarityID.Pink;
            Item.value = Item.sellPrice(gold: 2);
            Item.consumable = true;
            Item.UseSound = SoundID.Item44;
        }

        public override bool CanUseItem(Player player)
        {
            return SubworldSystem.IsActive<ApatheticMirrorSubworld>()
                && !NPC.AnyNPCs(ModContent.NPCType<ApatheticSpirit>())
                && !ApatheticSpiritSummon.IsActiveFor(player);
        }

        public override bool? UseItem(Player player)
        {
            if (player.whoAmI != Main.myPlayer)
                return true;

            Projectile.NewProjectile(
                player.GetSource_ItemUse(Item),
                player.Center - new Vector2(0f, 90f),
                Vector2.Zero,
                ModContent.ProjectileType<ApatheticSpiritSummon>(),
                0, 0f, player.whoAmI);

            return true;
        }

        public override void AddRecipes()
        {
            CreateRecipe()
                .AddIngredient<ApatheticEssence>(20)
                .AddTile(TileID.DemonAltar)
                .Register();
        }
    }
}