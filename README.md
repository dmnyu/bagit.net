# bagit.net

**bagit.net** is a C# implementation of the [BagIt specification (RFC 8493)](https://datatracker.ietf.org/doc/html/rfc8493). It allows you to create BagIt bags—structured file collections with checksums for reliable storage and transfer of digital content. It currently consists of a core library `bagit.net` and CLI application `bagit.net.cli` for Linux and Windows

[![Release](https://img.shields.io/badge/release-v0.2.1--alpha-blue)](https://github.com/dmnyu/bagit.net/releases/v0.2.1-alpha)

> ⚠️ **Note:** This project is in early development and currently support the creation and validation of BagIt formatted bags.

## bagit.net.cli
## bagit.net Commands
### create
Create a Bagit Formatted Bag from a directory.
#### options
- **--algorithm**: specify the checksum algorithm to use: md5, sha1, sha256 (default), sha384, and sha512
- **--log**: specify the location to write logging to (default stdout).
#### usage
```bash
# Create a bag with default SHA-256 checksums
bagit.net create /path/to/directory

# Create a bag using MD5 checksums
bagit.net create --algorithm md5 /path/to/directory

# Log to a file
bagit.net create --log bagit.net.log /path/to/directory
```

### validate
Validate a Bagit Formatted Bag
#### options
- **--fast**: specify to validate a bag based on payload-oxum only 
- **--log**: specify the location to write logging to (default stdout).
#### usage
```bash
# Validate a bag
bagit.net validate /path/to/directory

# Validate a bag with logging to file
bagit.net validate --log bagit.net.log /path/to/directory

# Fast Validate
bagit.net validate --fast /path/to/directory
```
### help
Print the help message

#### usage
```bash
bagit.net help
```
> **Tip:** including --help or -h in any command will print the help screen

### Binary Installation

#### Linux
```bash
wget https://github.com/dmnyu/bagit.net/releases/download/v0.2.1-alpha/bagit.net.cli-linux-v0.2.1-alpha.tgz
tar xvzf bagit.net.cli-linux-v0.2.1-alpha.tgz
cd bagit.net.cli
sudo ./install.sh
bagit.net --help
```

**Linux / SELinux Notes**
bagit.net single-file self-contained binaries require the ability to create and execute temporary files. On RHEL/CentOS systems with SELinux or noexec restrictions on /tmp, these binaries will not run.

#### Windows
```powershell
Invoke-WebRequest -Uri https://github.com/dmnyu/bagit.net/releases/download/v0.2.1-alpha/bagit.net.cli-win-v0.2.1-alpha.zip -OutFile bagit.net.cli-win-v0.2.1-alpha.zip
Expand-Archive bagit.net.cli-win-v0.2.1-alpha.zip -DestinationPath .
cd .\bagit.net.cli
.\bagit.net.exe" --help
.\install.ps1
%LOCALAPPDATA%\bagit.net\bagit.net --version 
```
> **Tip:** After installing on Windows, you can run `bagit.net` from any directory by adding `%LOCALAPPDATA%\bagit.net` to your user PATH in System Properties.

## Additional Notes

- The CLI is **self-contained**, so no .NET installation is required for end-users.  
- On Linux, the default install path is `/usr/local/bin/bagit.net`.  
- On Windows, the default install path is `%LOCALAPPDATA%\bagit.net\bagit.net.exe`.  
- For CI/CD or scripting, you can run the CLI directly from the extracted directory without installing.
