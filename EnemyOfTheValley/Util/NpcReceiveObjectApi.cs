using StardewModdingAPI;
using StardewValley;

namespace EnemyOfTheValley.Util;

public class NpcReceiveObjectApi
{
    private static NpcReceiveObjectApi? instance;
    
    public delegate void TryReceiveActiveObjectDelegate(NPC npc, Farmer who);
    
    private readonly struct ObjectHandler
    {
        public ObjectHandler(IManifest registeredBy, TryReceiveActiveObjectDelegate tryRecieve)
        {
            RegisteredBy = registeredBy;
            TryReceive = tryRecieve;
        }
        
        public readonly IManifest RegisteredBy;
        public readonly TryReceiveActiveObjectDelegate TryReceive;
    }

    private readonly Dictionary<string, ObjectHandler> handlers;

    private NpcReceiveObjectApi()
    {
        handlers = new Dictionary<string, ObjectHandler>();
    }

    public static NpcReceiveObjectApi Instance
    {
        get
        {
            instance = instance ?? new NpcReceiveObjectApi();
            return instance;
        }
    }

    public void RegisterItemHandler(IManifest registeredBy, string qualifiedItemId, TryReceiveActiveObjectDelegate tryReceive)
    {
        if (handlers.TryGetValue(qualifiedItemId, out var handler)) throw new Exception(qualifiedItemId + " is already registered by " + handler.RegisteredBy.UniqueID);
        
        var data = new ObjectHandler(registeredBy, tryReceive);
        handlers.Add(qualifiedItemId, data);
    }

    public bool HandleItem(string? qualifiedItemId, NPC npc, Farmer who, bool probe)
    {
        if (qualifiedItemId == null) return false;
        if (probe) return handlers.ContainsKey(qualifiedItemId);
        
        if (handlers.TryGetValue(qualifiedItemId, out var handler))
        {
            handler.TryReceive(npc, who);
            return true;
        }

        return false;
    }
}