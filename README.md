# bagit.net

**bagit.net** is a C# implementation of the [BagIt specification (RFC 8493)](https://datatracker.ietf.org/doc/html/rfc8493).  
It allows you to create **BagIt bags**—structured file collections with checksums for reliable storage and transfer of digital content.  
It currently consists of a core library (`bagit.net`) and a CLI application (`bagit.net.cli`) for Linux, MacOS, and Windows.

[![Release](https://img.shields.io/badge/release-v0.2.3--alpha-blue)](https://github.com/dmnyu/bagit.net/releases/v0.2.3-alpha)
![BagIt.NET CI](https://github.com/dmnyu/bagit.net/actions/workflows/ci.yml/badge.svg)

> ⚠️ **Note:** This project is in early development. It currently supports the creation and validation of **BagIt-formatted bags**.
There is currently no error handling in place and will crash if an error is encountered. See the [roadmap](https://github.com/dmnyu/bagit.net/blob/main/bagitnet_roadmap.md) for current project status

---

## bagit.net.cli

### Commands

#### create
Create a BagIt-formatted bag from a directory.

| Option | Description |
|--------|-------------|
| `--algorithm` | Specify the checksum algorithm to use: `md5`, `sha1`, `sha256` (default), `sha384`, or `sha512`. |
| `--log` | Specify the location to write logging (default: stdout). |

**Usage:**
```bash
# Create a bag with default SHA-256 checksums
bagit.net create /path/to/directory

# Create a bag using MD5 checksums
bagit.net create --algorithm md5 /path/to/directory

# Log to a file
bagit.net create --log bagit.net.log /path/to/directory
```

---

#### validate
Validate a BagIt-formatted bag.

| Option | Description |
|--------|-------------|
| `--fast` | Validate the bag based on payload-oxum only. |
| `--log` | Specify the location to write logging (default: stdout). |

**Usage:**
```bash
# Validate a bag
bagit.net validate /path/to/directory

# Validate a bag with logging to file
bagit.net validate --log bagit.net.log /path/to/directory

# Fast validation
bagit.net validate --fast /path/to/directory

# Completeness Only validation
bagit.net validate --complete /path/to/directory
```
>*note: setting both --fast and --complete flags will cause the application to exit* 

---

#### help
Display help information.

**Usage:**
```bash
bagit.net help
```
> **Tip:** Including `--help` or `-h` with any command will print the help screen.

---

## Binary Installation

### Linux
```bash
wget https://github.com/dmnyu/bagit.net/releases/download/v0.2.4-alpha/bagit.net.cli-v0.2.4-alpha-linux-x64.tgz
tar xvzf bagit.net.cli-v0.2.4-alpha-linux-x64.tgz
cd bagit.net
sudo ./install.sh
bagit.net --help
```

**Linux / SELinux Notes:**  
bagit.net single-file self-contained binaries require the ability to create and execute temporary files at runtime.  
On RHEL/CentOS systems with SELinux or `noexec` restrictions on `/tmp`, these binaries may not run.

---

### Windows
```powershell
Invoke-WebRequest -Uri https://github.com/dmnyu/bagit.net/releases/download/v0.2.4-alpha/bagit.net.cli-v0.2.4-alpha-win-x64.zip -OutFile bagit.net.cli-v0.2.4-alpha-win-x64.zip
Expand-Archive bagit.net.cli-v0.2.4-alpha-win-x64.zip -DestinationPath .
cd .\bagit.net
.\bagit.net.exe --help
.\install.ps1
%LOCALAPPDATA%\bagit.net\bagit.net --version
```
> **Tip:** After installation on Windows, you can run `bagit.net` from any directory by adding `%LOCALAPPDATA%\bagit.net` to your user `PATH`.

---

### MacOS
```bash
wget https://github.com/dmnyu/bagit.net/releases/download/0.2.4-alpha/bagit.net.cli-v0.2.4-alpha-macos-arm64.tgz
tar xvzf bagit.net.cli-v0.2.4-alpha-macos-arm64.tgz
cd bagit.net
sudo ./install.sh
bagit.net --help
```
> **note** This is an automated build from Github Actions, it is not tested.

---

## Additional Notes

- The CLI is **self-contained**, so no .NET installation is required for end-users.  
- On Linux and MacOS, the default install path is `/usr/local/bin/bagit.net`.  
- On Windows, the default install path is `%LOCALAPPDATA%\bagit.net\bagit.net.exe`.  
- For CI/CD or scripting, you can run the CLI directly from the extracted directory without installing.
