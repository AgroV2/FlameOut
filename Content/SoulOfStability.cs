

using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace flameout.Content
{
    
    public class SoulOfStability : ModItem
    {
        
        
        public override bool IsLoadingEnabled(Mod mod)
        {
            return ModLoader.HasMod("FargowiltasSouls");
        }

        public override void SetDefaults()
        {
            Item.width = 32;
            Item.height = 32;
            Item.maxStack = Item.CommonMaxStack;
            Item.rare = ItemRarityID.Purple;
            Item.value = Item.buyPrice(0, 75, 0, 0);
            Item.material = true; 
        }

        public override void ModifyTooltips(List<TooltipLine> tooltips)
        {
            TooltipLine lore = new TooltipLine(Mod, "SoulOfStabilityLore",
                "The last chance of hope");
            lore.OverrideColor = new Color(190, 120, 255);
            tooltips.Add(lore);
        }

        public override void AddRecipes()
        {
            CreateRecipe()
                .AddIngredient(ModContent.ItemType<ApathySoul>())
                .AddIngredient(ModContent.ItemType<EmpathySoul>())
                .Register();
        }
    }
}

