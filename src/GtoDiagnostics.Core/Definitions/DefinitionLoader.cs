using System.Text.Json;
using System.Text.Json.Serialization;

namespace GtoDiagnostics.Core.Definitions;

/// <summary>
/// Loads module definitions from JSON files.
/// </summary>
public static class DefinitionLoader
{
    private static readonly JsonSerializerOptions Options = new(JsonSerializerDefaults.Web)
    {
        WriteIndented = true,
        Converters = { new JsonStringEnumConverter() },
    };

    /// <summary>
    /// Loads a module definition from disk.
    /// </summary>
    /// <param name="path">Path to a JSON definition file.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The parsed definition.</returns>
    public static async Task<VehicleModuleDefinition> LoadAsync(string path, CancellationToken cancellationToken = default)
    {
        await using var stream = File.OpenRead(path);
        return await JsonSerializer.DeserializeAsync<VehicleModuleDefinition>(stream, Options, cancellationToken)
            ?? throw new InvalidDataException($"Definition file '{path}' did not contain a module definition.");
    }

    /// <summary>
    /// Saves a module definition to disk.
    /// </summary>
    /// <param name="path">Destination JSON path.</param>
    /// <param name="definition">Definition to save.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    public static async Task SaveAsync(string path, VehicleModuleDefinition definition, CancellationToken cancellationToken = default)
    {
        await using var stream = File.Create(path);
        await JsonSerializer.SerializeAsync(stream, definition, Options, cancellationToken);
    }
}
