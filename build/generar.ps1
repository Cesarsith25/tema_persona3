# Regenera fuentes e imagenes del tema P3R_Pausa.
# Uso:  powershell -ExecutionPolicy Bypass -File build\generar.ps1
# Coloca tus imagenes oficiales en la carpeta "arte" (ver LEEME.md) antes de ejecutarlo.
$ErrorActionPreference = 'Stop'
$raiz = Split-Path -Parent $PSScriptRoot
Add-Type -Path (Join-Path $PSScriptRoot 'Generador.cs') -ReferencedAssemblies System.Drawing
[P3R]::Run((Join-Path $raiz '.rockbox'), (Join-Path $raiz 'arte'))

# Capturas aproximadas (usan las fuentes .fnt generadas)
Add-Type -Path (Join-Path $PSScriptRoot 'Vista.cs') -ReferencedAssemblies System.Drawing
$vista = Join-Path $raiz 'vista_previa'
New-Item -ItemType Directory -Force $vista | Out-Null
[Vista]::Run((Join-Path $raiz '.rockbox'), $vista)
Write-Host "Listo. Copia la carpeta .rockbox a la raiz de tu Innioasis Y1."
