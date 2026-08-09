using Microsoft.Xna.Framework;
using SubworldLibrary;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace flameout.Content
{
    public class ApatheticMirror : ModItem
    {
        public override void SetDefaults()
        {
            Item.width = 28;
            Item.height = 34;
            Item.maxStack = 1;
            Item.useStyle = ItemUseStyleID.HoldUp;
            Item.useTime = 45;
            Item.useAnimation = 45;
            Item.rare = ItemRarityID.Pink;
            Item.value = Item.sellPrice(gold: 3);
            Item.consumable = false;
        }

        public override bool CanUseItem(Player player)
        {
            bool validWorld = !SubworldSystem.AnyActive() || SubworldSystem.IsActive<ApatheticMirrorSubworld>();
            return validWorld && !ApatheticMirrorTransition.IsActiveFor(player);
        }

        public override bool? UseItem(Player player)
        {
            if (player.whoAmI != Main.myPlayer || ApatheticMirrorTransition.IsActiveFor(player))
                return true;

            Projectile.NewProjectile(
                player.GetSource_ItemUse(Item),
                player.Center,
                Vector2.Zero,
                ModContent.ProjectileType<ApatheticMirrorTransition>(),
                0,
                0f,
                player.whoAmI);

            return true;
        }

    }
}
