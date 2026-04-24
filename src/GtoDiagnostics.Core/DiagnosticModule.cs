namespace GtoDiagnostics.Core;

/// <summary>
/// Known diagnostic modules on Mk1 GTO/3000GT/Stealth vehicles.
/// </summary>
public enum DiagnosticModule
{
    /// <summary>The engine ECU, the first supported target module.</summary>
    EngineEcu,

    /// <summary>Anti-lock braking system, research-only for now.</summary>
    Abs,

    /// <summary>Supplemental restraint system, research-only for now.</summary>
    Srs,

    /// <summary>Electronically controlled suspension, research-only for now.</summary>
    Ecs,

    /// <summary>Electronic time and alarm control system, research-only for now.</summary>
    Etacs,

    /// <summary>Air conditioning controller, research-only for now.</summary>
    AirConditioning,

    /// <summary>Cruise control module, research-only for now.</summary>
    CruiseControl,

    /// <summary>Automatic transmission controller, research-only for now.</summary>
    AutomaticTransmission,
}
