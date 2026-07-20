using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using flameout.Content;
using Terraria.Graphics.Shaders; 
namespace flameout.Content
{
    public class EmpathySummonPlayer : ModPlayer
    {
        internal static readonly (int Buff, int[] Projs)[] Minions = {
            (BuffID.BabySlime,       new int[]{ ProjectileID.BabySlime }),
            (BuffID.HornetMinion,    new int[]{ ProjectileID.Hornet }),
            (BuffID.ImpMinion,       new int[]{ ProjectileID.FlyingImp }),
            (BuffID.SpiderMinion,    new int[]{ ProjectileID.VenomSpider }),
            (BuffID.TwinEyesMinion,  new int[]{ ProjectileID.Retanimini, ProjectileID.Spazmamini }),
            (BuffID.PirateMinion,    new int[]{ ProjectileID.OneEyedPirate }),
            (BuffID.Pygmies,         new int[]{ ProjectileID.Pygmy }),
            (BuffID.Ravens,          new int[]{ ProjectileID.Raven }),
            (BuffID.UFOMinion,       new int[]{ ProjectileID.UFOMinion }),
            (BuffID.DeadlySphere,    new int[]{ ProjectileID.DeadlySphere }),
            (BuffID.SharknadoMinion, new int[]{ ProjectileID.Tempest }),
            (BuffID.StardustMinion,  new int[]{ ProjectileID.StardustCellMinion }),
            (BuffID.EmpressBlade,    new int[]{ ProjectileID.EmpressBlade }),
            (BuffID.BatOfLight,      new int[]{ ProjectileID.BatOfLight }),
            (BuffID.VampireFrog,     new int[]{ ProjectileID.VampireFrog }),
            (BuffID.FlinxMinion,     new int[]{ ProjectileID.FlinxMinion }),
            (BuffID.AbigailMinion,   new int[]{ ProjectileID.AbigailMinion }),
            (BuffID.Smolstar,        new int[]{ ProjectileID.Smolstar }),
        };

        
        private static readonly Dictionary<int, int> FormToSpawnType = new() {
            { ProjectileID.JumperSpider,    ProjectileID.VenomSpider },
            { ProjectileID.DangerousSpider, ProjectileID.VenomSpider },
        };

        internal static readonly HashSet<int> LegionTypes = BuildTypes();

        private static HashSet<int> BuildTypes()
        {
            var set = new HashSet<int>();
            foreach (var (_, projs) in Minions)
                foreach (int t in projs)
                    set.Add(t);
            foreach (var kv in FormToSpawnType)
                set.Add(kv.Key);
            return set;
        }

   public override void PostUpdateEquips()
{
   bool rage = Player.GetModPlayer<EmpathyRagePlayer>().RageActive;
if (Player.HeldItem?.ModItem is not EmpathySoul soul || (soul.Mode != 3 && !rage))
    return;

    
    int copies = Player.maxMinions;
    
    Player.maxMinions = 0;

    
    var counts = new Dictionary<int, int>();
    for (int i = 0; i < Main.maxProjectiles; i++)
    {
        Projectile proj = Main.projectile[i];
        if (!proj.active || proj.owner != Player.whoAmI || !proj.minion)
            continue;
        if (!LegionTypes.Contains(proj.type))
            continue;

        proj.minionSlots = 0f;
        int spawnType = FormToSpawnType.TryGetValue(proj.type, out int st) ? st : proj.type;
        counts[spawnType] = counts.TryGetValue(spawnType, out int c) ? c + 1 : 1;

        
        if (Main.myPlayer == Player.whoAmI && counts[spawnType] > copies)
            proj.Kill();
    }

    if (Main.myPlayer != Player.whoAmI)
        return;

    
    foreach (var (_, projs) in Minions)
    {
        foreach (int spawnType in projs)
        {
            counts.TryGetValue(spawnType, out int have);
            for (int k = have; k < copies; k++)
            {
                int p = Projectile.NewProjectile(
                    Player.GetSource_ItemUse(Player.HeldItem),
                    Player.Center, Vector2.Zero, spawnType,
                    Player.HeldItem.damage, 2f, Player.whoAmI);

                Main.projectile[p].minionSlots = 0f;
                Main.projectile[p].originalDamage = Player.HeldItem.damage;
                Main.projectile[p].netUpdate = true;
            }
        }
    }
}
    }

    
   public class EmpathyMinionGlobal : GlobalProjectile
{
    
    public override void PostAI(Projectile projectile)
    {
        if (!projectile.minion || !EmpathySummonPlayer.LegionTypes.Contains(projectile.type))
            return;

        Player owner = Main.player[projectile.owner];
        if (owner.active && !owner.dead
            && owner.HeldItem?.ModItem is EmpathySoul soul && soul.Mode == 3)
        {
            projectile.timeLeft = 2;
        }
    }

    

public override bool PreDraw(Projectile projectile, ref Color lightColor)
{
    if (!projectile.minion || !EmpathySummonPlayer.LegionTypes.Contains(projectile.type))
        return true;

    Player owner = Main.player[projectile.owner];
    if (owner.HeldItem?.ModItem is not EmpathySoul soul || soul.Mode != 3)
        return true;

    if (owner.cMinion != 0)
        return true; 

    
    Main.instance.PrepareDrawnEntityDrawing(projectile,
        GameShaders.Armor.GetShaderIdFromItemId(ItemID.PinkDye), null);

    return true; 
}
}
}