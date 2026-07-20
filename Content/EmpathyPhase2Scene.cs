using Terraria;
using Terraria.ModLoader;

namespace flameout.Content
{
    public class EmpathyPhase2Scene : ModSceneEffect
    {
        public override SceneEffectPriority Priority => SceneEffectPriority.BossHigh;
        public override int Music => MusicLoader.GetMusicSlot(Mod, "Content/Music/EmpathyMusic2");
        public override bool IsSceneEffectActive(Player player) => EmpathyPhase2Controller.Active;
    }
}
