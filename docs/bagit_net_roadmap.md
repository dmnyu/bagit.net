# BagIt.NET Roadmap Checklist

## ✅ Completed (Core Bag Functionality & Tests)
- [x] Create a temp data folder
  - GUID-based folder inside the bag directory to move files safely before renaming to `data`.
- [x] Move top-level files into temp data folder
  - All files are relocated safely.
- [x] Move subdirectories into temp data folder
  - Skip temp folder itself.
  - Rename final temp folder to `data`.
- [x] Unit tests for CreateBag
  - Verify exceptions on null or non-existent directory.
  - Verify no exception on valid directory.
  - Verify that the `data` folder is created.
  - Verify files and subdirectories are moved correctly.

## ⚡ In-Progress / Next Steps
- [ ] Print messages during tests
  - Use `Xunit.Abstractions.ITestOutputHelper` to log info in test runs instead of `Console.WriteLine`.
- [ ] Exception handling in `CreateBag`
  - Wrap `createTempDataDir` and `moveFiles` with `try/catch` for `IOException` and `UnauthorizedAccessException`.
  - Throw `InvalidOperationException` with a helpful message when caught.
- [ ] Test data management
  - Use temp folders per test to isolate changes.
  - Delete temp folders after tests to prevent pollution.
  - Optional: optimize repeated I/O using class fixture or single temp folder per test class.

## 🌟 Future / Advanced Features
- [ ] BagIt specification compliance
  - Validate bag structure according to [RFC 8493](https://datatracker.ietf.org/doc/html/rfc8493).
  - Generate `bagit.txt`, `bag-info.txt`, and manifest files.
  - Support checksums and validation of manifests.
  - Add support for tags, metadata, and optional payloads.
- [ ] Logging & Observability
  - Integrate a logging library (e.g., Serilog) for runtime logs.
  - Optional: support debug/trace levels for bag creation steps.

