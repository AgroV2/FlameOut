using Terraria;
using Terraria.ModLoader;
using flameout.Content;

namespace flameout.Content
{
    public class FargoSoulsCompactSystem : ModSystem
    {
       public override void PostAddRecipes()
{
    if (!ModLoader.TryGetMod("FargowiltasSouls", out Mod fargoSouls))
    {
        return;
    }

    
    if (!fargoSouls.TryFind<ModItem>("EternitySoul", out ModItem soulOfEternity)
        && !fargoSouls.TryFind<ModItem>("SoulOfEternity", out soulOfEternity))
    {
        return;
    }

    int added = 0;
    for (int i = 0; i < Recipe.numRecipes; i++)
    {
        Recipe recipe = Main.recipe[i];
        if (recipe.createItem.type == soulOfEternity.Type)
        {
            recipe.AddIngredient(ModContent.ItemType<SoulOfStability>());
            added++;
        }
    }

}
    }
}