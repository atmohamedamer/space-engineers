/**
 * This scripts controls the below blocks to make a full rotation and drill a semi-spherical hole.
 */

private IMyPistonBase piston;
private IMyMotorStator rotor;
private IMyMotorStator hinge;
private IMyShipDrill drill;

private bool isDrilling = false;
private double currentHingeAngle = 0;
private double currentPistonLength = 0;
private const double HingeSpeedRadPerTick = Math.PI / 1800; // Hinge speed in radians per tick (assuming 60 ticks per second)
private const double PistonSpeedPerTick = 0.02 / 60; // Piston speed per tick (assuming 60 ticks per second)
private const double MaxPistonLength = 10; // Maximum piston length, adjust as necessary

public Program()
{
    // Set the update frequency to once per tick.
    Runtime.UpdateFrequency = UpdateFrequency.Update1;
}

public void Save()
{
    // Save state if needed.
}

public void Main(string argument, UpdateType updateSource)
{
    Setup();

    switch (argument.ToLowerInvariant())
    {
        case "start":
            Start();
            break;
        case "stop":
            Stop();
            break;
        case "reset":
            Reset();
            break;
    }

    if (isDrilling)
    {
        PerformDrillingOperation();
    }
}

public void Setup()
{
    // Get block references.
    piston = piston ?? GridTerminalSystem.GetBlockWithName("RPiston") as IMyPistonBase;
    rotor = rotor ?? GridTerminalSystem.GetBlockWithName("RRotor") as IMyMotorStator;
    hinge = hinge ?? GridTerminalSystem.GetBlockWithName("RHinge") as IMyMotorStator;
    drill = drill ?? GridTerminalSystem.GetBlockWithName("RDrill") as IMyShipDrill;

    // Ensure all blocks are found.
    if (piston == null || rotor == null || hinge == null || drill == null)
    {
        Echo("Missing block reference.");
        isDrilling = false; // Stop the drilling if there's an error.
    }
}

public void Start()
{
    isDrilling = true;
    drill.Enabled = true;
    rotor.TargetVelocityRPM = 1f;
    hinge.TargetVelocityRad = (float)HingeSpeedRadPerTick;
}

public void Stop()
{
    isDrilling = false;
    drill.Enabled = false;
    rotor.TargetVelocityRPM = 0f;
    hinge.TargetVelocityRad = 0f;
    piston.Velocity = 0f;
}

public void Reset()
{
    // Reset the drill, rotor, hinge, and piston to initial positions.
    Stop(); // Stop all operations first.

    // Retract the piston.
    piston.Velocity = -1f;
    if (piston.CurrentPosition <= piston.MinLimit)
    {
        piston.Velocity = 0f;
    }

    // Reset the angles of rotor and hinge to 0 degrees (if your setup allows).
    rotor.RotorLock = true; // Lock rotor in place before trying to reset.
    rotor.TargetVelocityRPM = 0f;

    hinge.RotorLock = true; // Lock hinge in place before trying to reset.
    hinge.TargetVelocityRPM = 0f;

    // Additional logic may be required to move rotor and hinge to initial positions safely.
}

public void PerformDrillingOperation()
{
    drill.Enabled = true;
    
    // Convert angles to radians for comparison
    currentHingeAngle = hinge.Angle;
    currentPistonLength = piston.CurrentPosition;

    // Hinge control: make a full rotation
    hinge.TargetVelocityRad = (float)HingeSpeedRadPerTick;
    
    if (currentHingeAngle >= 2 * Math.PI) // If hinge has made a full rotation (2*pi radians)
    {
        // Reset hinge angle for the next cycle
        currentHingeAngle -= 2 * Math.PI;
        hinge.Angle = 0f;
        
        // Extend piston if not fully extended
        if (currentPistonLength < MaxPistonLength)
        {
            currentPistonLength += PistonSpeedPerTick;
            piston.Velocity = (float)PistonSpeedPerTick * 60; // Convert per tick speed to per second
        }
        else
        {
            // If piston is fully extended, stop drilling
            Stop();
            return;
        }
    }

    // Rotor control: continuous rotation
    rotor.TargetVelocityRPM = 1f;
}
