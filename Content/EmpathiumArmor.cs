using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace flameout.Content
{
    public static class EmpathiumSet
    {
        
        public static bool Equipped(Player player)
        {
            if (player == null)
                return false;

            return player.armor[0].type == ModContent.ItemType<Will>()
            && player.armor[1].type == ModContent.ItemType<Faith>()
            && player.armor[2].type == ModContent.ItemType<Strength>();
        }

        public static void DefaultsFor(Item item, int defense)
        {
            item.width = 32;
            item.height = 32;
            item.defense = defense;
            item.value = Item.sellPrice(0, 200, 0, 0);
            item.rare = ItemRarityID.Master;
        }

        
        public static void MakeTooltipsRainbow(List<TooltipLine> tooltips)
        {
            foreach (TooltipLine line in tooltips)
            {
                line.OverrideColor = Main.DiscoColor;
            }
        }
    }

    public class Will : ModItem
    {
        public override void SetDefaults()
        {
            EmpathiumSet.DefaultsFor(Item, 1000);
            
            
            
            Item.headSlot = ArmorIDs.Head.FamiliarWig;
        }

        
        public override void ModifyTooltips(List<TooltipLine> tooltips)
        {
            EmpathiumSet.MakeTooltipsRainbow(tooltips);
        }

        public override void UpdateEquip(Player player)
        {
            
            player.lifeRegen += 2000;
        }

        public override bool IsArmorSet(Item head, Item body, Item legs)
        => body.type == ModContent.ItemType<Faith>() && legs.type == ModContent.ItemType<Strength>();

        public override void UpdateArmorSet(Player player)
        {
            player.setBonus = "You feel much lighter";
        }

        public override void AddRecipes()
        {
            CreateRecipe()
            .AddIngredient(ModContent.ItemType<Empathium>(), 3)
            .AddTile(TileID.LunarCraftingStation)
            .Register();
        }
    }

    public class Faith : ModItem
    {
        public override void SetDefaults()
        {
            EmpathiumSet.DefaultsFor(Item, 1500);
            Item.bodySlot = ArmorIDs.Body.FamiliarShirt;
        }

        
        public override void ModifyTooltips(List<TooltipLine> tooltips)
        {
            EmpathiumSet.MakeTooltipsRainbow(tooltips);
        }

        public override void UpdateEquip(Player player)
        {
            player.GetAttackSpeed(DamageClass.Generic) += 15f;   
        }

        public override void AddRecipes()
        {
            CreateRecipe()
            .AddIngredient(ModContent.ItemType<Empathium>(), 3)
            .AddTile(TileID.LunarCraftingStation)
            .Register();
        }
    }

    public class Strength : ModItem
    {
        public override void SetDefaults()
        {
            EmpathiumSet.DefaultsFor(Item, 1250);
            Item.legSlot = ArmorIDs.Legs.FamiliarPants;
        }

        
        public override void ModifyTooltips(List<TooltipLine> tooltips)
        {
            EmpathiumSet.MakeTooltipsRainbow(tooltips);
        }

        public override void UpdateEquip(Player player)
        {
            player.GetDamage(DamageClass.Generic) += 12.5f;      
        }

        public override void AddRecipes()
        {
            CreateRecipe()
            .AddIngredient(ModContent.ItemType<Empathium>(), 3)
            .AddTile(TileID.LunarCraftingStation)
            .Register();
        }
    }

    
    
    public abstract class EmpathiumAccessorySlot : ModAccessorySlot
    {
        public override bool IsEnabled() => EmpathiumSet.Equipped(Player);

        public override bool IsHidden() => !EmpathiumSet.Equipped(Player);
    }

    public class EmpathiumSlot1 : EmpathiumAccessorySlot { }
    public class EmpathiumSlot2 : EmpathiumAccessorySlot { }
    public class EmpathiumSlot3 : EmpathiumAccessorySlot { }
}
