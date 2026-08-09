using System.ComponentModel;
using Terraria.ModLoader.Config;

namespace flameout.Content
{
    public class FlameoutClientConfig : ModConfig
    {
        public override ConfigScope Mode => ConfigScope.ClientSide;

        public static FlameoutClientConfig Instance;

        [Label("Debug mode")]
        [DefaultValue(false)]
        public bool DebugMode;

        public override void OnLoaded()
        {
            Instance = this;
        }

        public override void OnChanged()
        {
            HorrorEventManager.HandleDebugModeChanged(DebugMode);
        }
    }
}
