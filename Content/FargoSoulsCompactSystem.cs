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
        Mod.Logger.Info("FargoCompat: FargowiltasSouls not loaded");
        return;
    }

    
    if (!fargoSouls.TryFind<ModItem>("EternitySoul", out ModItem soulOfEternity)
        && !fargoSouls.TryFind<ModItem>("SoulOfEternity", out soulOfEternity))
    {
        Mod.Logger.Warn("FargoCompat: Soul of Eternity item not found");
        return;
    }

    int added = 0;
    for (int i = 0; i < Recipe.numRecipes; i++)
    {
        Recipe recipe = Main.recipe[i];
        if (recipe.createItem.type == soulOfEternity.Type)
        {
            recipe.AddIngredient(ModContent.ItemType<EmpathySoul>());
            added++;
        }
    }

    Mod.Logger.Info($"FargoCompat: Empathy Soul added to {added} recipe(s)");
}
    }
}