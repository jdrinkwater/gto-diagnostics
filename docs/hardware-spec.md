# Hardware Spec

Initial hardware target: a safe USB-presented serial interface suitable for Mk1 GTO/3000GT/Stealth diagnostics.

Open questions:

- exact electrical signalling used by the engine ECU diagnostic line
- safe voltage levels and protection requirements
- whether non-engine modules need different pin selection or electrical handling
- whether bidirectional communication is required for each module

Any write, actuator, or fault-clearing feature is out of scope until the protocol and safety implications are well understood.
