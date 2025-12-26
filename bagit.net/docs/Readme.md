## About
bagit.net is a C# implementation of the [BagIt specification (RFC 8493)](https://datatracker.ietf.org/doc/html/rfc8493).
It provides APIs to create, validate and manage metadata in BagIt bags — structured file collections with checksums for reliable storage and transfer of digital content.

## How to Use
### Creating Bags
```csharp
using bagit.net.domain;
using bagit.net.services;
using Microsoft.Extensions.DependencyInjection;

namespace bagit_test;

public class App 
{
    public static async Task Main(string[] args)
    {
        var serviceProvider = BagitServiceProvider.BuildServiceProvider<CreationService>();
        var creationService = serviceProvider.GetRequiredService<CreationService>();
        var checksumAlgorithms = new List<ChecksumAlgorithm>() { ChecksumAlgorithm.SHA256 };
        var numProcesses = 2;
        await creationService.CreateBag(args[0], checksumAlgorithms, null, numProcesses);
    }
}
```
### Validating Bags
```csharp
using bagit.net.domain;
using bagit.net.services;
using Microsoft.Extensions.DependencyInjection;

namespace bagit_test;

public class App 
{
    public static async Task Main(string[] args)
    {
        var serviceProvider = BagitServiceProvider.BuildServiceProvider<ValidationService>();
        var validationService = serviceProvider.GetRequiredService<ValidationService>();
        var numProcesses = 2;
        await validationService.ValidateBag(args[0], numProcesses);
    }
}
```
## Main Types
> All core services are asynchronous and designed for high-performance I/O and concurrent checksum calculation.
- `CreationService` — Creates BagIt bags
- `ValidationService` — Validates bags
- `ChecksumService` — Computes file checksums
- `ManifestService` — Creates and validates manifest files
- `TagFileService` — Creates, validates and manages BagIt tag files (bagit.txt, bag-info.txt)
- `FileManagerService` — File system operations
- `MessageService` — Structured logging and user feedback

## Installation
```powershell
dotnet add package bagit.net
```
## Feedback
bagit.net is released as open source under the [AGPLv3 license](https://github.com/dmnyu/bagit.net/blob/main/LICENSE.txt). Bug reports and contributions are welcome at the [GitHub repository](https://github.com/dmnyu/bagit.net/blob/main/LICENSE).

