# BagIt.NET

**BagIt.NET** is a C# implementation of the [BagIt specification (RFC 8493)](https://datatracker.ietf.org/doc/html/rfc8493). It allows you to create, validate, and manage BagIt bags—structured file collections with checksums for reliable storage and transfer of digital content.

> ⚠️ **Note:** This project is in early development and currently supports only limited functionality.

---

## Prerequisites

- [.NET 9.0 SDK](https://dotnet.microsoft.com/en-us/download/dotnet/9.0)

---

## Build from source

```bash
cd bagit.net.cli
dotnet build
```

---

## CLI Usage

### Create a Bag

```bash
.\bagit.net.cli.exe create --[hash] <directory-to-bag>
```

**Parameters:**

- `--md5` – Generate MD5 checksums for files.  
- `--sha1` – Generate SHA-1 checksums.  
- `--sha256` – Generate SHA-256 checksums.  
- `--sha384` – Generate SHA-384 checksums.  
- `--sha512` – Generate SHA-512 checksums.  
- `<directory-to-bag>` – Path to the directory you want to package as a BagIt bag.

**Example:**

```bash
.\bagit.net.cli.exe create --md5 C:\Users\Donald\Documents\MyData
```

This creates a BagIt bag in the specified directory with MD5 checksums. If no checksum algorithm is specified, SHA-256 checksums will be generated.
