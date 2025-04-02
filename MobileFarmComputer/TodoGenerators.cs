using StardewValley;
using StardewValley.Buildings;
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

        public TodoItem(string taskName, bool completed, int remaining, List<(string name, int remaining)>? uncompletedSubtasks = null)
        {
            TaskName = "* " + taskName;
            if (!completed)
            {
                TaskName += $" ({remaining} remaining)";
            }
            
            Completed = completed;
            uncompletedSubtasks ??= new List<(string name, int remaining)>();
            UncompletedSubtasks = uncompletedSubtasks.Select(x => ($"* {x.name} ({x.remaining} remaining)")).ToList();
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

    // TODO subtasks that show where each machine is
    private static TodoItem GetMachinesTask(GameLocation location)
    {
        var ready = location.getNumberOfMachinesReadyForHarvest();
        return new TodoItem("Collect from machines", ready == 0, ready);
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
        return new TodoItem("Water crops", crops == 0, crops);
    }
    
    // TODO should include crops/garden pots both in and outside
    private static TodoItem GetCropsWaterTask(GameLocation location)
    {
        var crops = location.getTotalUnwateredCrops();
        return new TodoItem("Water crops", crops == 0, crops);
    }
    
    // helpers
}