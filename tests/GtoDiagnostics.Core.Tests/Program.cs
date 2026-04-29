using GtoDiagnostics.Core;
using GtoDiagnostics.Core.Definitions;
using GtoDiagnostics.Core.Logging;

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

var decodedLogPath = Path.Combine(Path.GetTempPath(), $"gto-decoded-readings-test-{Guid.NewGuid():N}.jsonl");
await using (var writer = new DecodedReadingLogWriter(decodedLogPath))
{
    await writer.WriteAsync(DateTimeOffset.UnixEpoch, DiagnosticModule.EngineEcu, readings);
}

var decodedLog = await File.ReadAllTextAsync(decodedLogPath);
AssertContains("coolant_temp", decodedLog);
AssertContains("throttle_position", decodedLog);
AssertContains("battery_voltage", decodedLog);
AssertContains("EngineEcu", decodedLog);

var csvLogPath = Path.Combine(Path.GetTempPath(), $"gto-decoded-readings-test-{Guid.NewGuid():N}.csv");
await using (var writer = new DecodedReadingCsvLogWriter(csvLogPath))
{
    await writer.WriteAsync(DateTimeOffset.UnixEpoch, DiagnosticModule.EngineEcu, readings);
    await writer.WriteAsync(
        DateTimeOffset.UnixEpoch,
        DiagnosticModule.EngineEcu,
        [new SensorReading("quoted_sensor", "Quoted, \"Sensor\"", "V", 12.5)]);
}

var csvLog = await File.ReadAllTextAsync(csvLogPath);
AssertContains("timestamp,module,sensor_id,sensor_name,value,unit", csvLog);
AssertContains("1970-01-01T00:00:00.0000000+00:00,EngineEcu,coolant_temp,Coolant Temperature,60,C", csvLog);
AssertContains("1970-01-01T00:00:00.0000000+00:00,EngineEcu,throttle_position,Throttle Position,50.196078431372548,%", csvLog);
AssertContains("1970-01-01T00:00:00.0000000+00:00,EngineEcu,quoted_sensor,\"Quoted, \"\"Sensor\"\"\",12.5,V", csvLog);

Console.WriteLine("GtoDiagnostics.Core.Tests passed.");

static void AssertEqual<T>(T expected, T actual)
{
    if (!EqualityComparer<T>.Default.Equals(expected, actual))
    {
        throw new InvalidOperationException($"Expected '{expected}', got '{actual}'.");
    }
}

static void AssertContains(string expected, string actual)
{
    if (!actual.Contains(expected, StringComparison.Ordinal))
    {
        throw new InvalidOperationException($"Expected text to contain '{expected}'.");
    }
}
