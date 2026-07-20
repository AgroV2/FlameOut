using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.DataStructures;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.ModLoader;

namespace flameout.Content
{
    public class PlayerMirror : ModNPC
    {
        public override string Texture => "flameout/Content/Blank";

        
        private int _targetPlayerIndex = 0;

        public override void SetDefaults()
        {
            NPC.width       = 20;
            NPC.height      = 42;
            NPC.lifeMax     = 1;
            NPC.life        = 1;
            NPC.damage      = 0;
            NPC.defense     = 0;
            NPC.noGravity   = true;
            NPC.noTileCollide = true;  
            NPC.dontTakeDamage = true;
            NPC.friendly    = false;
            NPC.immortal    = true;
            NPC.aiStyle     = -1;
            NPC.HitSound    = null;
            NPC.DeathSound  = null;
        }

        public override void OnSpawn(IEntitySource source)
        {
            _targetPlayerIndex = NPC.FindClosestPlayer();
        }

     public override void AI()
{
    Player target = Main.player[_targetPlayerIndex];

    if (!target.active || target.dead)
    {
        NPC.active = false;
        return;
    }

    
    
    NPC.velocity.X = -target.velocity.X * 1.2f;
    NPC.velocity.Y = -target.velocity.Y * 1.2f;

    
    float dist = Vector2.Distance(NPC.Center, target.Center);
    if (dist > 2000f)
    {
        NPC.position = new Vector2(
            target.position.X + target.direction * 600f,
            target.position.Y
        );
        NPC.velocity = Vector2.Zero;
    }

    NPC.direction = -target.direction;
    NPC.spriteDirection = NPC.direction;

    Rectangle npcRect    = NPC.getRect();
    Rectangle playerRect = target.getRect();

    if (npcRect.Intersects(playerRect))
    {
        MirrorEffectPlayer effect = target.GetModPlayer<MirrorEffectPlayer>();
        effect.ActivateEffect();
        NPC.active = false;
    }
}
public override bool PreDraw(SpriteBatch spriteBatch, Vector2 screenPos, Color drawColor)
{
    Player target = Main.player[_targetPlayerIndex];
    if (!target.active) return false;

    
    Vector2 realPosition = target.position;

    
    target.position = NPC.position;

    
    Main.PlayerRenderer.DrawPlayer(Main.Camera, target, target.position, 0f, Vector2.Zero);

    
    target.position = realPosition;

    return false;
}



    }
}