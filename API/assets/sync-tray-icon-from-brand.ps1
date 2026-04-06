# Regenera API/assets/cloudkeep.ico con la misma apariencia que UI/public/favicon.svg.
# Tras cambiar el SVG, ejecuta este script y (opcional) copia cloudkeep.ico a UI/public/favicon.ico.
$ErrorActionPreference = 'Stop'
$dir = Split-Path -Parent $MyInvocation.MyCommand.Path
$iconPath = Join-Path $dir 'cloudkeep.ico'
Add-Type -AssemblyName System.Drawing
Add-Type @"
using System;
using System.Runtime.InteropServices;
public static class NativeMethods {
  [DllImport("user32.dll", CharSet = CharSet.Auto)]
  public static extern bool DestroyIcon(IntPtr handle);
}
"@

$s = 64
$scale = $s / 32.0
$b = New-Object System.Drawing.Bitmap $s, $s
$g = [System.Drawing.Graphics]::FromImage($b)
$g.SmoothingMode = [System.Drawing.Drawing2D.SmoothingMode]::AntiAlias
$g.PixelOffsetMode = [System.Drawing.Drawing2D.PixelOffsetMode]::Half
$g.Clear([System.Drawing.Color]::Transparent)

$radius = [Math]::Max(2, [int](6 * $scale))
$path = New-Object System.Drawing.Drawing2D.GraphicsPath
$path.AddArc(0, 0, (2 * $radius), (2 * $radius), 180, 90)
$path.AddArc(($s - 2 * $radius), 0, (2 * $radius), (2 * $radius), 270, 90)
$path.AddArc(($s - 2 * $radius), ($s - 2 * $radius), (2 * $radius), (2 * $radius), 0, 90)
$path.AddArc(0, ($s - 2 * $radius), (2 * $radius), (2 * $radius), 90, 90)
$path.CloseFigure()
$bg = New-Object System.Drawing.SolidBrush ([System.Drawing.Color]::FromArgb(0x15, 0x17, 0x1c))
$g.FillPath($bg, $path)
$bg.Dispose()
$path.Dispose()

$pts = @(
    [System.Drawing.Point]::new([int][Math]::Round(16 * $scale), [int][Math]::Round(6 * $scale)),
    [System.Drawing.Point]::new([int][Math]::Round(26 * $scale), [int][Math]::Round(16 * $scale)),
    [System.Drawing.Point]::new([int][Math]::Round(16 * $scale), [int][Math]::Round(26 * $scale)),
    [System.Drawing.Point]::new([int][Math]::Round(6 * $scale), [int][Math]::Round(16 * $scale))
)
$fg = New-Object System.Drawing.SolidBrush ([System.Drawing.Color]::FromArgb(0x63, 0x66, 0xf1))
$g.FillPolygon($fg, $pts)
$fg.Dispose()
$g.Dispose()

$h = $b.GetHicon()
$icon = [System.Drawing.Icon]::FromHandle($h)
$fs = [System.IO.File]::Create($iconPath)
$icon.Save($fs)
$fs.Close()
$icon.Dispose()
$b.Dispose()
[void][NativeMethods]::DestroyIcon($h)
Write-Host "Wrote $iconPath (match UI/public/favicon.svg)"
