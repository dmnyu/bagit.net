# BagIt.NET Roadmap

This roadmap outlines the planned feature development and release milestones for **bagit.net**, a C# implementation of the BagIt specification (RFC 8493). It covers versions **0.2.0** through **0.4.0**, with clear goals and scope for each.


---

## **0.2.7 - Multiple Checksum Support**
**Objective:** Allow multiple checksum algorithms be declared when creating a bag and create manifest files for each algorithm.

## **0.2.8 — Multithreaded Checksum Calculation**
**Objective:** Improve performance of manifest generation using parallelism.

**Features:**
- Parallel hashing across payload files.
- Configurable degree of parallelism (`--threads N`).
- Streamed hashing to minimize memory usage.
- Thread‑safe logging.
- Performance benchmarking and regression tests.

**Notes:**
This release focuses on speed and scalability, especially for large bags.

---

## Future Considerations (Post‑0.3)
These are ideas for after the 0.4.x line:
- Bag repair tools (e.g., regenerate missing checksums).
- Partial rehashing (only changed files).
- Support for bag serialization (ZIP, TAR).
- Plug‑in architecture for custom checksum algorithms.
- Avalonia frontend for Windows/Linux/Mac
- fetch files

---

## Previous updates: ##

---


## **0.2.6 — Tag File Editing**
**Objective:** Add full support for reading, modifying, and generating tag files.

**Features:**
- Complete model for `bag-info.txt` and other tag files.
- TagManager and ManageCommand classes CLI
- Add, edit, or remove metadata fields.
- Deterministic output ordering.
- CLI support for editing metadata during creation or via a dedicated command:
  - `--set "Field=Value"`
  - `--add "Field=Value"`
  - `--delete "Field"`
 
---

## **0.2.5 - Error Handling**
**Objective:** 
- remove all logging from core library
- have validation and creation functions return a List of Exception Records instead of throwing
- log the exception out side of the core library
- quiet mode to only display errors

---

## 0.2.4 completeness only validation

---

## **0.2.3 Validation / Creation Service
**Objective:** 
- remove all functions from the Validator and Bagger classes and move to a services
- move bagger and Validator to CLI project

---

## **0.2.2 — FileManager**
**Objective:** Move all functions in the Bagger class that moves, creates, deletes files or directories to file management service

---

## **0.2.1 -- Service-Oriented-Architecture**
**Objective:** Convert all static classes to services / dependency injection

---

## **0.2.0 — Validation**
**Objective:** Implement full BagIt validation capabilities.

**Features:**
- Validate `bagit.txt` (version, encoding, formatting).
- Validate payload manifests (checksum verification).
- Validate tag manifests, when present.
- Detect missing or extra payload/tag files.
- Structured error reporting (machine-readable + human-readable).
- CLI: Introduce `validate` command with correct exit codes.

**Notes:**
This is the foundational step enabling BagIt.NET to interact safely with existing bags and sets the stage for advanced features.

---

End of roadmap.

