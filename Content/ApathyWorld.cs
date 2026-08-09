using Microsoft.Xna.Framework;
using System;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using flameout.Content;

using Terraria.ModLoader.IO;


using Microsoft.Xna.Framework.Graphics;
public class ApathyWorld : ModSystem
{
    public static bool downedApathy = false;
 public static bool downedEmpathy = false;
 public static bool downedTrueEmpathy = false;
    public override void SaveWorldData(TagCompound tag)
    {
        tag["downedApathy"] = downedApathy;
        tag["downedEmpathy"] = downedApathy;
         tag["downedTrueEmpathy"] = downedApathy;
    }

    public override void LoadWorldData(TagCompound tag)
    {
        downedApathy = tag.GetBool("downedApathy");
         downedApathy = tag.GetBool("downedEmpathy");
         downedApathy = tag.GetBool("downedTrueEmpathy");
    }
}
