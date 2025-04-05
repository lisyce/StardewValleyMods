using StardewValley;
using StardewValley.Buildings;
using StardewValley.Extensions;
using StardewValley.Locations;
using StardewValley.Objects;
using SObject = StardewValley.Object;

namespace MobileFarmComputer;

public class TodoGenerators
{
    public class TodoItem
    {
        public string TaskName;
        public bool Completed;
        public List<string> UncompletedSubtasks;
        public bool RenderSubtasks;

        public TodoItem(string taskName, bool completed, int remaining, Dictionary<string, int>? uncompletedSubtasks = null)
        {
            TaskName = "* " + taskName;
            if (!completed)
            {
                TaskName += $" ({remaining} remaining)";
            }
            
            Completed = completed;
            uncompletedSubtasks ??= new Dictionary<string, int>();
            UncompletedSubtasks = uncompletedSubtasks.Select(x => ($"* {x.Key} ({x.Value} remaining)")).ToList();
            RenderSubtasks = UncompletedSubtasks.Any();
        }
    }

    public static (List<TodoItem> todo, List<TodoItem> done) GetTodos()
    {
        var allTasks = new List<TodoItem>();
        
        var location = Game1.player.currentLocation.GetRootLocation();
        if (location is Farm farm)
        {
            allTasks.Add(GetFarmCaveTodoItem(farm));
        }
        
        allTasks.Add(GetMachinesTask(location));
        allTasks.Add(GetPetBowlsTask(location));
        allTasks.Add(GetForageTask(location));
        allTasks.Add(GetCropsHarvestTask(location));
        allTasks.Add(GetCropsWaterTask(location));
        
        var todo = allTasks.Where(t => !t.Completed).OrderBy(t => t.TaskName).ToList();
        var done = allTasks.Where(t => t.Completed).OrderBy(t => t.TaskName).ToList();
        
        return (todo, done);
    }

    // task generators
    private static TodoItem GetFarmCaveTodoItem(Farm farm)
    {
        var completed = !farm.doesFarmCaveNeedHarvesting();
        return new TodoItem("Harvest farm cave", completed, 1);
    }

    private static TodoItem GetMachinesTask(GameLocation location)
    {
        var toCollect = MachinesToCollect(location);
        var ready = toCollect.Values.Sum();
        
        return new TodoItem("Collect from machines", ready == 0, ready, toCollect);
    }

    private static TodoItem GetPetBowlsTask(GameLocation location)
    {
        var totalBowls = 0;
        var emptyBowls = 0;

        foreach (var building in location.buildings)
        {
            if (building is PetBowl bowl)
            {
                totalBowls++;
                if (!bowl.watered.Value) emptyBowls++;
            }
        }

        return new TodoItem("Fill pet water bowls", emptyBowls == 0, emptyBowls);
    }

    private static TodoItem GetForageTask(GameLocation location)
    {
        var forage = location.getTotalForageItems();
        return new TodoItem("Pick up forage", forage == 0, forage);
    }

    // TODO should include crops/garden pots both in and outside
    private static TodoItem GetCropsHarvestTask(GameLocation location)
    {
        var crops = location.getTotalCropsReadyForHarvest();
        return new TodoItem("Harvests crops", crops == 0, crops);
    }
    
    // TODO should include crops/garden pots both in and outside
    private static TodoItem GetCropsWaterTask(GameLocation location)
    {
        var crops = location.getTotalUnwateredCrops();
        return new TodoItem("Water crops", crops == 0, crops);
    }
    
    // helpers
    private static Dictionary<string, int> MachinesToCollect(GameLocation location)
    {
        var result = new Dictionary<string, int>();
        
        result["This location"] = location.Objects.Values.Count(x => x.IsConsideredReadyMachineForComputer());

        var houseName = location is Farm ? "FarmHouse" :
            location is IslandWest iw && iw.farmhouseRestored.Value ? "IslandFarmHouse" : null;
        if (houseName != null)
        {
            result["Farmhouse"] = Game1.RequireLocation(houseName).Objects.Values
                .Count(x => x.IsConsideredReadyMachineForComputer());
        }

        foreach (var building in location.buildings)
        {
            var indoors = building.GetIndoors();
            if (indoors == null) continue;
            
            result.TryAdd(building.buildingType.Value + "s", 0);
            result[building.buildingType.Value + "s"] +=
                indoors.Objects.Values.Count(x => x.IsConsideredReadyMachineForComputer());
        }

        result.RemoveWhere(x => x.Value == 0);

        return result;
    }
}