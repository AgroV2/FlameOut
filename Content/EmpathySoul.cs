using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.Audio;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.ModLoader.IO;
using flameout.Content;
using System;
using System.Linq;
namespace flameout.Content
{
    public class EmpathySoul : ModItem
    {
        public int Mode = 0;
        private static readonly string[] ModeNames = { "Empathy Blade", "Empathy Bow", "Empathy Staff", "Empathy Whip" };
        private static readonly Color[] ModeColors = {
            new Color(255, 100, 180), new Color(255, 60, 70),
            new Color(200, 120, 255), new Color(255, 255, 255) };
            private static readonly int[] VanillaWhips = {
    ProjectileID.BlandWhip,   
    ProjectileID.ThornWhip,   
    ProjectileID.BoneWhip,    
    ProjectileID.FireWhip,    
    ProjectileID.CoolWhip,    
    ProjectileID.SwordWhip,   
    ProjectileID.ScytheWhip,  
    ProjectileID.MaceWhip,    
    ProjectileID.RainbowWhip, 
};
private static readonly Color GradPink = new(255, 130, 200);
private static readonly Color GradWhite = new(255, 245, 252);
        public override void SetDefaults()
        {
            Item.damage = 2800;
            Item.DamageType = DamageClass.Melee;
            Item.width = 40;
            Item.height = 40;
            Item.useTime = 20;
            Item.useAnimation = 20;
            Item.useStyle = ItemUseStyleID.Shoot;
            Item.channel = true;       
            Item.noMelee = true;       
            Item.noUseGraphic = true;  
            Item.knockBack = 7f;
            Item.value = Item.sellPrice(gold: 25);
            Item.rare = ItemRarityID.Purple;
            Item.autoReuse = false;
            Item.shoot = ModContent.ProjectileType<EmpathySwordHeld>();
            Item.shootSpeed = 1f;
        }

        public override bool AltFunctionUse(Player player) => true;

      public override bool CanUseItem(Player player)
{
    var rage = player.GetModPlayer<EmpathyRagePlayer>();
if (rage.RageActive)
{
    Item.useStyle = ItemUseStyleID.Swing;
    Item.useTime = Item.useAnimation = 12;
    Item.shoot = ModContent.ProjectileType<EmpathyWhipProj>();
    Item.shootSpeed = 4f;
    Item.UseSound = SoundID.Item153;
    Item.channel = true;
    return true; 
}
    if (player.altFunctionUse == 2)
    {
        Mode = (Mode + 1) % 4;
        CombatText.NewText(player.getRect(), ModeColors[Mode], ModeNames[Mode]);
        return false;
    }

    if (Mode == 3) 
    {
        Item.useStyle = ItemUseStyleID.Swing;
        Item.useTime = Item.useAnimation = 12;
        Item.shootSpeed = 16f;
        Item.UseSound = SoundID.Item153;
        return player.ownedProjectileCounts[ModContent.ProjectileType<EmpathyWhipProj>()] == 0;
    }

    Item.useStyle = ItemUseStyleID.Shoot;
    Item.useTime = Item.useAnimation = 20;
    Item.shootSpeed = 6f;
    Item.UseSound = null;

    switch (Mode)
    {
        case 0: return player.ownedProjectileCounts[ModContent.ProjectileType<EmpathySwordHeld>()] == 0;
        case 1: return player.ownedProjectileCounts[ModContent.ProjectileType<GuardianBowHeld>()] == 0;
        case 2: return player.ownedProjectileCounts[ModContent.ProjectileType<EmpathyStaffHeld>()] == 0;
        default: return false;
    }
}

        public override bool Shoot(Player player, Terraria.DataStructures.EntitySource_ItemUse_WithAmmo source,
            Vector2 position, Vector2 velocity, int type, int damage, float knockback)
        {
            var rage = player.GetModPlayer<EmpathyRagePlayer>();
if (rage.RageActive)
{
    
    Projectile.NewProjectile(source, position, velocity,
        ModContent.ProjectileType<EmpathyWhipProj>(), damage, knockback, player.whoAmI);
    for (int i = 0; i < 3; i++)
    {
        int whipType = VanillaWhips[Main.rand.Next(VanillaWhips.Length)];
        int w = Projectile.NewProjectile(source, position, velocity.RotatedByRandom(0.2f),
            whipType, (int)(damage * 0.5f), knockback, player.whoAmI);
        Main.projectile[w].WhipSettings.RangeMultiplier = EmpathyWhipProj.RANGE_MULT;
    }

    
    int[] held = {
        ModContent.ProjectileType<EmpathySwordHeld>(),
        ModContent.ProjectileType<GuardianBowHeld>(),
        ModContent.ProjectileType<EmpathyStaffHeld>(),
    };
    foreach (int t in held)
        if (player.ownedProjectileCounts[t] == 0)
            Projectile.NewProjectile(source, player.Center, velocity, t, damage, knockback, player.whoAmI);

    return false;
}
            if (Mode == 0)
            {
                Projectile.NewProjectile(source, player.Center, Vector2.Zero,
                    ModContent.ProjectileType<EmpathySwordHeld>(), damage, knockback, player.whoAmI);
            }
            else if (Mode == 1)
{
    Projectile.NewProjectile(source, player.Center, Vector2.Zero,
        ModContent.ProjectileType<GuardianBowHeld>(), damage, knockback, player.whoAmI);
}
else if (Mode == 2)
{
    Projectile.NewProjectile(source, player.Center, Vector2.Zero,
        ModContent.ProjectileType<EmpathyStaffHeld>(), damage, knockback, player.whoAmI);
}
          else if (Mode == 3)
{
    
    Projectile.NewProjectile(source, position, velocity,
        ModContent.ProjectileType<EmpathyWhipProj>(), damage, knockback, player.whoAmI);

    
    int[] vanillaWhips = {
        ProjectileID.BlandWhip, ProjectileID.ThornWhip, ProjectileID.BoneWhip,
        ProjectileID.FireWhip, ProjectileID.CoolWhip, ProjectileID.SwordWhip,
        ProjectileID.ScytheWhip, ProjectileID.MaceWhip, ProjectileID.RainbowWhip
    };
  for (int i = 0; i < 3; i++)
{
    int whipType = vanillaWhips[Main.rand.Next(vanillaWhips.Length)];
    int p = Projectile.NewProjectile(source, position, velocity.RotatedByRandom(0.2f),
        whipType, (int)(damage * 0.5f), knockback, player.whoAmI);

    
    Main.projectile[p].WhipSettings.RangeMultiplier = Content.EmpathyWhipProj.RANGE_MULT;
}
}
return false;
        }
private static string Gradient(string text, float wave = 0.35f, float speed = 2.5f)
{
    var sb = new System.Text.StringBuilder();
    for (int i = 0; i < text.Length; i++)
    {
        char ch = text[i];
        if (ch == ' ')
        {
            sb.Append(' ');
            continue;
        }
        float t = 0.5f + 0.5f * (float)Math.Sin(Main.GlobalTimeWrappedHourly * speed - i * wave);
        Color c = Color.Lerp(GradPink, GradWhite, t);
        sb.Append($"[c/{c.R:X2}{c.G:X2}{c.B:X2}:{ch}]");
    }
    return sb.ToString();
}

   public override void ModifyTooltips(List<TooltipLine> tooltips)
{
    
    TooltipLine nameLine = tooltips.FirstOrDefault(
        t => t.Mod == "Terraria" && t.Name == "ItemName");
    if (nameLine != null)
        nameLine.Text = Gradient(nameLine.Text, 0.5f, 3f);

    
    tooltips.Add(new TooltipLine(Mod, "EmpathyMode", $"Mode: {ModeNames[Mode]}")
        { OverrideColor = ModeColors[Mode] });
    tooltips.Add(new TooltipLine(Mod, "EmpathyHint", "Right click to switch attack variation")
        { OverrideColor = new Color(255, 180, 220) });

    
    tooltips.Add(new TooltipLine(Mod, "EmpathyFlavor1",
        Gradient("A soul that has known all the pain of this world")));
    tooltips.Add(new TooltipLine(Mod, "EmpathyFlavor2",
        Gradient("and turned it into compassion")));

    
    tooltips.Add(new TooltipLine(Mod, "EmpathyRageInfo",
        Gradient("Deal 1.2 million damage to overflow with empathy")));

    var rage = Main.LocalPlayer.GetModPlayer<EmpathyRagePlayer>();
    string status = rage.RageActive
        ? $"OVERFLOW! {rage.RageTime / 60 + 1} sec left"
        : $"Charged: {(int)(rage.Charge / EmpathyRagePlayer.CHARGE_MAX * 100f)}%";
    tooltips.Add(new TooltipLine(Mod, "EmpathyRageStatus", status)
        { OverrideColor = rage.RageActive ? Color.White : new Color(230, 120, 180) });

    tooltips.Add(new TooltipLine(Mod, "EmpathyRageDesc",
        "During overflow all variations attack at once")
        { OverrideColor = new Color(255, 160, 210) });
}

        
        public override void SaveData(TagCompound tag) => tag["mode"] = Mode;
        public override void LoadData(TagCompound tag) => Mode = tag.GetInt("mode");
    }
}