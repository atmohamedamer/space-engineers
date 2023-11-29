private string elevatorGroupName = "Workshop-Elevator";
private List<IMyPistonBase> pistons = new List<IMyPistonBase>();
private Dictionary<string, float> floorHeights = new Dictionary<string, float>
{
    {"1", 0.0f}, // Height for floor 1
    {"2", 2.51f}, // Height for floor 2
    {"3", 4.89f}, // Height for floor 3
    {"4", 9.9f}  // Height for floor 4
};
private const float Velocity = 0.5f; // Speed of piston movement
private const float Tolerance = 0.1f; // Tolerance for reaching the target height
private string targetFloor = null;

public Program()
{
    GridTerminalSystem.GetBlockGroupWithName(elevatorGroupName)?.GetBlocksOfType(pistons);
    
    if (pistons.Count == 0)
    {
        Echo("No pistons found in the group 'Elevator Pistons'.");
        return;
    }

    Runtime.UpdateFrequency = UpdateFrequency.Update10;
}

public void Main(string argument, UpdateType updateSource)
{
    if (floorHeights.ContainsKey(argument))
    {
        targetFloor = argument;
    }

    if (targetFloor != null)
    {
        MoveElevatorToFloor(targetFloor);
    }
}

private void MoveElevatorToFloor(string floor)
{
    float targetHeight = floorHeights[floor];
    bool allPistonsReached = true;

    foreach (var piston in pistons)
    {
        float heightDifference = Math.Abs(piston.CurrentPosition - targetHeight);

        if (heightDifference > Tolerance)
        {
            allPistonsReached = false;
            piston.Velocity = (piston.CurrentPosition < targetHeight) ? Velocity : -Velocity;
        }
        else
        {
            piston.Velocity = 0;
        }
    }

    if (allPistonsReached)
    {
        targetFloor = null; // Reset target floor after all pistons have reached the destination
    }
}
