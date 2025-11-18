# bagit.net

**bagit.net** is a C# implementation of the [BagIt specification (RFC 8493)](https://datatracker.ietf.org/doc/html/rfc8493). It allows you to create BagIt bags—structured file collections with checksums for reliable storage and transfer of digital content. It currently consists of a core library `bagit.net` and cli application `bagit.net.cli` for Linux and Windows

[![Release](https://img.shields.io/badge/release-v0.1.0--alpha.1-blue)](https://github.com/dmnyu/bagit.net/releases/v0.1.0-alpha.1)

> ⚠️ **Note:** This project is in early development and currently supports only the creation of bagit bags functionality.



## bagit.net.cli
### Binary Installation


### Linux

```bash
wget https://github.com/dmnyu/bagit.net/releases/v0.1.0-alpha.1/bagit.net.cli-linux-v0.1.0-alpha.1.tgz
tar xvzf bagit.net.cli-linux-v0.1.0-alpha.1.tgz
cd bagit.net.cli
sudo ./install.sh
bagit.net --help
```


### Windows

```powershell
Invoke-WebRequest -Uri https://github.com/dmnyu/bagit.net/releases/v0.1.0-alpha.1/bagit.net.cli-win-v0.1.0-alpha.1.zip -OutFile bagit.net.cli-win-v0.1.0-alpha.1.zip
Expand-Archive bagit.net.cli-win-v0.1.0-alpha.1.zip -DestinationPath .
cd .\bagit.net.cli
"C:\Program Files\BagIt.NET\bagit.net.exe" --help
.\install.ps1
```
> **Tip:** After installing on Windows, you can run `bagit.net` from any directory by adding `%LOCALAPPDATA%\bagit.net` to your user PATH in System Properties.


## Usage Examples
```bash
# Create a bag with default SHA-256 checksums
bagit.net create /path/to/directory

# Create a bag using MD5 checksums
bagit.net create --algorithm md5 /path/to/directory

# Log to a file
bagit.net create --log bagit.net.log /path/to/directory

# View the Help Page
bagit.net --help
```

## Additional Notes

- The CLI is **self-contained**, so no .NET installation is required for end-users.  
- On Linux, the default install path is `/usr/local/bin/bagit.net`.  
- On Windows, the default install path is `%LOCALAPPDATA%\bagit.net\bagit.net.exe`.  
- For CI/CD or scripting, you can run the CLI directly from the extracted directory without installing.