# Roadmap

Near-term work should stay vertical: each slice should connect enough layers to
be useful without depending on unknown vehicle protocol details.

## Next Vertical Slices

1. Raw capture replay - complete
   - Read raw JSONL captures back in.
   - Filter received engine ECU messages.
   - Decode them using the current module definitions.
   - Verify replay through an end-to-end test.

2. CSV decoded logging - complete
   - Add CSV output beside decoded JSONL readings.
   - Keep timestamps, module, sensor id, display name, value, and unit.
   - Cover formatting with focused tests.

3. Definition-driven request plan - complete
   - Group sensors by configured command.
   - Expose request commands from module definitions.
   - Remove hardcoded polling commands from app-level flow where possible.

4. Live polling session abstraction - complete
   - Introduce a service that owns one polling cycle.
   - Use an `IByteTransport`, a module definition, raw capture logging, and decoded logging.
   - Share this path between simulator, tests, and future serial transport.

5. Simulator-backed app session
   - Route the Avalonia simulator button through the live polling session.
   - Keep the UI as a consumer of session results rather than the owner of protocol flow.

6. Capture metadata and session manifest
   - Write a manifest beside capture files.
   - Include vehicle family, module, start time, adapter or port when known, and notes.
   - Preserve reverse-engineering context for future real-vehicle captures.
