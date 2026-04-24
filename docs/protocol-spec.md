# Protocol Spec

Initial wording should remain neutral: Mitsubishi OBD1, MUT-era, or Mk1 diagnostic protocol.

The implementation is split into:

- transport: serial open/close, read/write, timeouts, raw bytes
- protocol: command/response parsing, framing, checksums once known
- data definitions: sensor names, scaling, units, enumerations, DTC decoding
- application: live data, fault codes, logging, reports, UI state

Protocol details are currently expected to be discovered through capture and validation.
