using Terraria.ModLoader;

namespace flameout.Content
{
    public class ApathyMusic : ModSceneEffect
    {
        public override int Music => MusicLoader.GetMusicSlot(Mod, "Content/Music/ApathyMusic");

        public override SceneEffectPriority Priority => SceneEffectPriority.BossHigh;

        public override bool IsSceneEffectActive(Terraria.Player player)
        {
            
            return Terraria.NPC.AnyNPCs(ModContent.NPCType<ApathyBoss>());
        }
    }
}