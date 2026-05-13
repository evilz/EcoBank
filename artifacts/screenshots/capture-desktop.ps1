$ErrorActionPreference = "Stop"

Add-Type -AssemblyName System.Drawing
Add-Type @"
using System;
using System.Runtime.InteropServices;

public static class Win32Capture {
    [DllImport("user32.dll")] public static extern bool SetForegroundWindow(IntPtr hWnd);
    [DllImport("user32.dll")] public static extern bool ShowWindow(IntPtr hWnd, int nCmdShow);
    [DllImport("user32.dll")] public static extern bool GetWindowRect(IntPtr hWnd, out RECT rect);

    [StructLayout(LayoutKind.Sequential)]
    public struct RECT {
        public int Left;
        public int Top;
        public int Right;
        public int Bottom;
    }
}
"@

$root = "E:\PROJECTS\GITHUB\EcoBank"
$output = Join-Path $root "artifacts\screenshots\desktop-windows-login.png"
$outLog = Join-Path $root "artifacts\screenshots\desktop-run.out.log"
$errLog = Join-Path $root "artifacts\screenshots\desktop-run.err.log"

$desktopExe = Join-Path $root "src\Desktop\bin\Debug\net10.0\EcoBank.Desktop.exe"
$process = Start-Process -FilePath $desktopExe `
    -WorkingDirectory (Split-Path $desktopExe) `
    -RedirectStandardOutput $outLog `
    -RedirectStandardError $errLog `
    -PassThru

try {
    $handle = [IntPtr]::Zero
    $deadline = (Get-Date).AddSeconds(30)
    while ((Get-Date) -lt $deadline) {
        $process.Refresh()
        if ($process.HasExited) {
            throw "EcoBank Desktop exited before a window was available. See $outLog and $errLog"
        }
        if ($process.MainWindowHandle -ne [IntPtr]::Zero) {
            $handle = $process.MainWindowHandle
            break
        }
        Start-Sleep -Milliseconds 500
    }

    if ($handle -eq [IntPtr]::Zero) {
        throw "Timed out waiting for EcoBank Desktop main window."
    }

    [Win32Capture]::ShowWindow($handle, 9) | Out-Null
    [Win32Capture]::SetForegroundWindow($handle) | Out-Null
    Start-Sleep -Seconds 3

    $rect = New-Object Win32Capture+RECT
    [Win32Capture]::GetWindowRect($handle, [ref]$rect) | Out-Null
    $width = $rect.Right - $rect.Left
    $height = $rect.Bottom - $rect.Top
    if ($width -le 0 -or $height -le 0) {
        throw "EcoBank Desktop window has invalid bounds: ${width}x${height}."
    }

    $bitmap = New-Object System.Drawing.Bitmap($width, $height)
    $graphics = [System.Drawing.Graphics]::FromImage($bitmap)
    $graphics.CopyFromScreen($rect.Left, $rect.Top, 0, 0, $bitmap.Size)
    $bitmap.Save($output, [System.Drawing.Imaging.ImageFormat]::Png)
    $graphics.Dispose()
    $bitmap.Dispose()

    Write-Output $output
}
finally {
    if ($process -and -not $process.HasExited) {
        $process.CloseMainWindow() | Out-Null
        Start-Sleep -Seconds 1
        if (-not $process.HasExited) {
            $process.Kill()
        }
    }
}
