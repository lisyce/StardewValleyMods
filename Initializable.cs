using BZP_Allergies.Config;
using StardewModdingAPI;

namespace BZP_Allergies
{
    internal abstract class Initializable
    {
        protected internal static IMonitor Monitor;
        protected internal static ModConfig Config;
        protected internal static IGameContentHelper GameContent;
        protected internal static IModContentHelper ModContent;

        // call in the Entry class
        public static void Initialize(IMonitor monitor, ModConfig config,
            IGameContentHelper gameContentHelper, IModContentHelper modContentHelper)
        {
            Monitor = monitor;
            Config = config;
            GameContent = gameContentHelper;
            ModContent = modContentHelper;
        }
    }
}
