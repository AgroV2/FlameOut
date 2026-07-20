using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.Audio;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.DataStructures;
using Terraria.GameContent; 

public class DarkScreenSystem : ModSystem
{
    public static float DarkAlpha = 0f;

    public override void PostDrawInterface(SpriteBatch spriteBatch)
    {
        if (DarkAlpha <= 0f) return;

        
        spriteBatch.Draw(
            TextureAssets.MagicPixel.Value,
            new Rectangle(0, 0, Main.screenWidth, Main.screenHeight),
            Color.Black * DarkAlpha);
    }
}