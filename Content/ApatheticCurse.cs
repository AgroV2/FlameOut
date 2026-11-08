using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace flameout.Content
{
    public class ApatheticCurse : ModBuff
    {
        

        public override void SetStaticDefaults()
        {
            Main.debuff[Type] = true;
            Main.pvpBuff[Type] = true;
            BuffID.Sets.LongerExpertDebuff[Type] = true;
        }

        public override void Update(NPC npc, ref int buffIndex)
        {
            npc.GetGlobalNPC<ApatheticCurseGlobalNPC>().Cursed = true;

            if (Main.rand.NextBool(4))
            {
                Dust dust = Dust.NewDustDirect(npc.position, npc.width, npc.height, DustID.PurpleTorch, 0f, -0.5f, 100, default, 1.1f);
                dust.noGravity = true;
            }
        }
    }

    public class ApatheticCurseGlobalNPC : GlobalNPC
    {
        public override bool InstancePerEntity => true;

        public bool Cursed;

        public override void ResetEffects(NPC npc)
        {
            Cursed = false;
        }

        public override void ModifyIncomingHit(NPC npc, ref NPC.HitModifiers modifiers)
        {
            if (!Cursed || npc.defense <= 0)
            {
                return;
            }

            int reduction = System.Math.Max(1, (int)System.Math.Ceiling(npc.defense * 0.02f));
            modifiers.Defense.Flat -= reduction;
        }
    }
}
