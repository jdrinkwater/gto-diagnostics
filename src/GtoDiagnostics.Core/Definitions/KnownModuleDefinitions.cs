namespace GtoDiagnostics.Core.Definitions;

/// <summary>
/// Built-in module definitions used by the simulator and early UI development.
/// </summary>
public static class KnownModuleDefinitions
{
    /// <summary>
    /// Gets a provisional Mk1 engine ECU live-data definition.
    /// </summary>
    /// <remarks>
    /// These values are simulator-oriented placeholders until protocol captures
    /// confirm command bytes, response positions, and scaling.
    /// </remarks>
    public static VehicleModuleDefinition CreateProvisionalEngineEcu()
    {
        return new VehicleModuleDefinition
        {
            Module = DiagnosticModule.EngineEcu,
            VehicleFamily = "gto_mk1",
            Sensors =
            [
                new SensorDefinition
                {
                    Id = "coolant_temp",
                    Name = "Coolant Temperature",
                    Unit = "C",
                    Command = "10 01",
                    ByteIndex = 2,
                    Multiplier = 1,
                    Offset = -40,
                },
                new SensorDefinition
                {
                    Id = "throttle_position",
                    Name = "Throttle Position",
                    Unit = "%",
                    Command = "10 01",
                    ByteIndex = 3,
                    Multiplier = 100d / 255d,
                },
                new SensorDefinition
                {
                    Id = "battery_voltage",
                    Name = "Battery Voltage",
                    Unit = "V",
                    Command = "10 01",
                    ByteIndex = 4,
                    Multiplier = 0.1,
                },
            ],
        };
    }
}
