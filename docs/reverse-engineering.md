# Reverse Engineering

Raw capture requirements:

- timestamp transmitted and received bytes
- include module and pin context where known
- save captures in a readable format
- allow comparison between existing tools, modules, and cable designs

Suggested workflow:

1. Confirm the vehicle, pin, and module under test.
2. Capture known-good traffic where possible.
3. Identify baud rate and serial settings.
4. Identify request/response framing.
5. Map commands and byte positions.
6. Validate decoded values against real vehicle behaviour.
7. Add definitions and tests using captured data.
