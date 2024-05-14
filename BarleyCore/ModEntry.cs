using StardewModdingAPI;

namespace BarleyCore
{
    internal sealed class ModEntry : Mod
    {
        public override void Entry(IModHelper helper)
        {
            
        }

        public override object GetApi(IModInfo mod)
        {
            return new BarleyCoreApi(mod);
        }
    }
}