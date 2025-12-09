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

let createTgz sourceDir destTgz =
    if File.Exists destTgz then File.Delete destTgz
    let args = $"-czf {destTgz} -C {sourceDir} ."
    let p = System.Diagnostics.Process.Start("tar", args)
    p.WaitForExit()
    printfn "Created archive: %s" destTgz

// -------------------------------
// Configuration
// -------------------------------

log "bagit.net.cli build system for linux"

let projectDir = "bagit.net.cli"
let installScriptSource = Path.Combine(projectDir, "dist", "install scripts", "install.sh")
let publishDir = Path.Combine(projectDir, "bin", "Release", "net9.0", "linux-x64", "publish")
let exeName = "bagit.net.cli"
let exeDestName = "bagit.net"
let args = fsi.CommandLineArgs |> Array.skip 1  // skip the script name

// -------------------------------
// Steps
// -------------------------------

// 1. Clean the directories
if args |> Array.contains "--clean" then
    log "cleaning directories"
    runProcess "dotnet" "clean" projectDir

    let binPath = Path.Combine(projectDir, "bin")
    if Directory.Exists(binPath) then
        Directory.Delete(binPath, true) 

    let objPath = Path.Combine(projectDir, "obj")
    if Directory.Exists(objPath) then
        Directory.Delete(objPath, true) 

// 1. Publish the project
let selfContained =
    if args |> Array.contains "--include-framework" then "true"
    else "false"

if selfContained = "true" then log "publishing with dotnet framework"
else log "publishing without dotnet framework"

let publishArgs =
    $"publish -c Release -r linux-x64 /p:SelfContained={selfContained} /p:PublishSingleFile=true"

runProcess "dotnet" publishArgs projectDir

// 2. Get the version
let exePath = @"bagit.net.cli/bin/Release/net9.0/linux-x64/publish/bagit.net.cli"
let exeOut = runProcessCaptureOutput exePath "--version" "."
let parts = exeOut.Split(' ')
let version = parts[1]
printfn "Detected version: %s" version

// 3. Setup values
let distPath = Path.Combine(projectDir, $"dist/{version}/linux/bagit.net")
let archivePath = Path.Combine(projectDir, $"dist/{version}/archives")

// 4. Ensure dist and archive directories exist
ensureDir distPath
ensureDir archivePath

// 5. Copy bin and install script
let installScriptDest = Path.Combine(distPath, "install.sh")
copyFile (Path.Combine(publishDir, exeName)) (Path.Combine(distPath, exeDestName))
copyFile installScriptSource installScriptDest

// 6. Compress the dist directory
let tgFilePath = Path.Combine(archivePath, $"bagit.net.cli-{version}-linux-x64.tgz")
createTgz distPath tgFilePath

log "Build completed successfully."

//install if there is a --install flag
if args |> Array.exists ((=) "--install") then
    log "Installing binary"
    let relativeBinPath = $"./bagit.net.cli/dist/{version}/linux/bagit.net"
    let binPath = Path.GetFullPath(relativeBinPath)
    runProcess "bash" "install.sh" binPath
    log "bin installed successfully"
