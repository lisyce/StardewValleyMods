using StardewValley;

namespace BarleyCore
{
    public interface IBarleyCoreApi
    {
        public void ModData_SetTarget(IHaveModData target);
        public TModel? ModData_Read<TModel>(string key, bool local = true) where TModel : class;
        public void ModData_Write<TModel>(string key, TModel value, bool local = true) where TModel: class;
    }
}
