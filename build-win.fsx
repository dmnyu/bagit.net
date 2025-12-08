open System
open System.Diagnostics
open System.IO
open System.IO.Compression

// -------------------------------
// Helpers
// -------------------------------

let log msg = printfn "[%s] %s" (DateTime.Now.ToString("HH:mm:ss")) msg

/// Run a process and fail if it returns non-zero exit code
let runProcess (fileName: string) (args: string) (workingDir: string) =
    use proc = new Process()
    proc.StartInfo <- ProcessStartInfo(
        FileName = fileName,
        Arguments = args,
        WorkingDirectory = workingDir,
        RedirectStandardOutput = true,
        RedirectStandardError = true,
        UseShellExecute = false,
        CreateNoWindow = true
    )

    proc.OutputDataReceived.Add(fun data -> if data.Data <> null then Console.WriteLine(data.Data))
    proc.ErrorDataReceived.Add(fun data -> if data.Data <> null then Console.Error.WriteLine(data.Data))

    proc.Start() |> ignore
    proc.BeginOutputReadLine()
    proc.BeginErrorReadLine()
    proc.WaitForExit()

    if proc.ExitCode <> 0 then
        failwithf "Process failed with exit code %d" proc.ExitCode

let runProcessCaptureOutput (fileName: string) (args: string) (workingDir: string) =
    use proc = new Process()
    proc.StartInfo <- ProcessStartInfo(
        FileName = fileName,
        Arguments = args,
        WorkingDirectory = workingDir,
        RedirectStandardOutput = true,
        RedirectStandardError = true,
        UseShellExecute = false,
        CreateNoWindow = true
    )

    proc.Start() |> ignore

    let output = proc.StandardOutput.ReadToEnd()
    let error = proc.StandardError.ReadToEnd()

    proc.WaitForExit()
    if proc.ExitCode <> 0 then
        failwithf "Process failed with exit code %d: %s" proc.ExitCode error

    output.Trim()  // return the stdout trimmed

/// Ensure directory exists
let ensureDir path =
    if not (Directory.Exists path) then
        Directory.CreateDirectory(path) |> ignore
        log $"Created directory: {path}"
    else
        log $"Directory exists: {path}"

/// Copy a file (overwrite if exists)
let copyFile src dest =
    File.Copy(src, dest, true)
    log $"Copied {src} -> {dest}"

/// Zip a directory to a destination file
let zipDirectory sourceDir destZip =
    if File.Exists destZip then File.Delete destZip
    ZipFile.CreateFromDirectory(sourceDir, destZip, CompressionLevel.Optimal, false)
    log $"Created archive: {destZip}"

// -------------------------------
// Configuration
// -------------------------------

log "bagit.net.cli build system"

let projectDir = ".\\bagit.net.cli"
let installScriptSource = Path.Combine(projectDir, "dist", "install scripts", "install.ps1")
let publishDir = Path.Combine(projectDir, "bin", "Release", "net9.0", "win-x64", "publish")
let exeName = "bagit.net.cli.exe"
let exeDestName = "bagit.net.exe"
let args = fsi.CommandLineArgs |> Array.skip 1



// -------------------------------
// Steps
// -------------------------------

// 1. Publish the project

let selfContained =
    if args |> Array.contains "--framework" then "true"
    else "false"

let publishArgs =
    $"publish -c Release -r win-x64 --self-contained {selfContained} /p:PublishSingleFile=true"

runProcess "dotnet" publishArgs projectDir


//2. get the version
let exePath = @".\bagit.net.cli\bin\Release\net9.0\win-x64\publish\bagit.net.cli.exe"
let exeOut = runProcessCaptureOutput exePath "--version" "."
let parts = exeOut.Split(' ')
let version = parts[1]
printfn "Detected version: %s" version

let distPath = Path.Combine(projectDir, $"dist\\{version}\\windows\\bagit.net")
let archivePath = Path.Combine(projectDir, $"dist\\{version}\\archives")
let installScriptDest = Path.Combine(distPath, "install.ps1")
let zipFilePath = Path.Combine(archivePath, $"bagit.net.cli-{version}-windows-x64.zip")

// 2. Ensure dist and archive directories exist
ensureDir distPath
ensureDir archivePath

// 3. Copy exe and install script
copyFile (Path.Combine(publishDir, exeName)) (Path.Combine(distPath, exeDestName))
copyFile installScriptSource installScriptDest

// 4. Zip the dist directory
if args |> Array.contains "--archive" then
    zipDirectory distPath zipFilePath

log "Build completed successfully."
//install if there is a --install flag  // skip the script name
if args |> Array.exists ((=) "--install") then
    log "Installing binary"
    let binPath = $"bagit.net.cli\\dist\\{version}\\windows\\bagit.net"
    runProcess "powershell.exe" "-ExecutionPolicy Bypass -File .\\install.ps1" binPath
    log "bin installed successfully"
