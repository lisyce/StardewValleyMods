using BZP_Allergies.Config;
using StardewModdingAPI;

namespace BZP_Allergies
{
    internal abstract class Initializable
    {
        protected internal static IMonitor Monitor;
        protected internal static IGameContentHelper GameContent;
        protected internal static IModContentHelper ModContent;

        // call in the Entry class
        public static void Initialize(IMonitor monitor,
            IGameContentHelper gameContentHelper, IModContentHelper modContentHelper)
        {
            Monitor = monitor;
            GameContent = gameContentHelper;
            ModContent = modContentHelper;
        }
    }
}