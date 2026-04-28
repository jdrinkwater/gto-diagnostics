using GtoDiagnostics.Core;
using GtoDiagnostics.Core.Definitions;

var sensor = new SensorDefinition
{
    Id = "coolant_temp",
    Name = "Coolant Temperature",
    Unit = "C",
    ByteIndex = 1,
    Multiplier = 1,
    Offset = -40,
};

var reading = sensor.Decode([0x00, 0x64]);
AssertEqual("coolant_temp", reading.Id);
AssertEqual(60d, reading.Value);

var definition = new VehicleModuleDefinition
{
    Module = DiagnosticModule.EngineEcu,
    VehicleFamily = "gto_mk1",
    Sensors = [sensor],
};

var path = Path.Combine(Path.GetTempPath(), $"gto-core-test-{Guid.NewGuid():N}.json");
await DefinitionLoader.SaveAsync(path, definition);
var loaded = await DefinitionLoader.LoadAsync(path);
AssertEqual("gto_mk1", loaded.VehicleFamily);
AssertEqual(1, loaded.Sensors.Count);

var engineDefinition = KnownModuleDefinitions.CreateProvisionalEngineEcu();
var decoder = new LiveDataDecoder(engineDefinition);
var readings = decoder.Decode([0x90, 0x01, 0x64, 0x80, 0x8E]);
AssertEqual(3, readings.Count);
AssertEqual("coolant_temp", readings[0].Id);
AssertEqual(60d, readings[0].Value);
AssertEqual("throttle_position", readings[1].Id);
AssertEqual(50.19607843137255d, readings[1].Value);
AssertEqual("battery_voltage", readings[2].Id);
AssertEqual(14.200000000000001d, readings[2].Value);

Console.WriteLine("GtoDiagnostics.Core.Tests passed.");

static void AssertEqual<T>(T expected, T actual)
{
    if (!EqualityComparer<T>.Default.Equals(expected, actual))
    {
        throw new InvalidOperationException($"Expected '{expected}', got '{actual}'.");
    }
}
