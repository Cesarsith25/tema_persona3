# P3R_Pausa: tema de Persona 3 Reload para Innioasis Y1 (Rockbox 240p)

Es un tema funcional para la **build 240p** de Rockbox del Y1. Esa build usa la misma
resolución que el iPod Classic (320x240, `ipod6g`). El diseño está inspirado en el
**menú de pausa de Persona 3 Reload**: fondo azul marino y turquesa con haces de luz
y ondas de agua, opciones grandes en cursiva y una barra blanca inclinada con sombra
turquesa para la selección.

| Pantalla | Qué muestra |
|---|---|
| Menús / navegador (`.sbs`) | Fecha "MM/DD", día, **momento del día** (Early Morning, Morning, Lunchtime, Afternoon, After School, Evening, Late Night; a las 00:00 sale **DARK HOUR** en verde), reloj, batería como barra **HP**, título de la sección, lista estilo pausa y la canción que suena abajo |
| Reproducción (`.wps`) | Portada en tarjeta inclinada, NOW PLAYING, título/artista/álbum, **HP** = progreso de la canción, **SP** = volumen, siguiente pista, shuffle/repeat y posición en la lista |

## Instalación

1. Conecta el Y1 al PC.
2. Copia la carpeta **`.rockbox`** de este proyecto a la raíz del Y1 y acepta combinar
   carpetas. También puedes descomprimir `P3R_Pausa_Y1.zip` en la raíz.
3. En el Y1 ve a *Ajustes → Tema → Cargar tema* y elige **P3R_Pausa**.
4. Si el fondo no cambia, reinicia Rockbox. En la build del Y1, a veces hace falta
   reiniciar al cambiar de tema.

Recomendado: *Ajustes → Pantalla → Mostrar iconos: No* (el `.cfg` ya lo desactiva).

## Usar imágenes oficiales del juego

Por derechos de autor, el tema **no incluye** arte oficial de Atlus/SEGA. Trae un
emblema original: una luna llena con un reloj que marca medianoche. Para ponerle
arte oficial, guarda tus imágenes (por ejemplo, renders de personajes de la web
oficial o del press kit de P3R) en la carpeta `arte/` con estos nombres (vale `.png`,
`.jpg` o `.bmp`):

| Archivo | Dónde aparece | Consejo |
|---|---|---|
| `arte/personaje.png` | Menú, lado derecho (sustituye a la luna) | Render vertical con fondo transparente (por ejemplo, el protagonista con el Evoker) |
| `arte/personaje_wps.png` | Fondo tenue de la pantalla de reproducción | Otro render vertical |
| `arte/fondo.png` | Fondo de ambas pantallas (se tiñe de azul) | Captura del menú de pausa o un wallpaper |
| `arte/sin_portada.png` | Canciones sin carátula | Imagen cuadrada |

Después regenera el tema. En Windows no hace falta instalar nada:

```bash
powershell -ExecutionPolicy Bypass -File build/generar.ps1
```

El script también vuelve a crear las fuentes y las capturas de `vista_previa/`.
Vuelve a copiar `.rockbox` al Y1.

## Estructura

```
.rockbox/
  themes/P3R_Pausa.cfg          <- el tema
  wps/P3R_Pausa.sbs             <- menús (barra de estado personalizada)
  wps/P3R_Pausa.wps             <- pantalla de reproducción
  wps/P3R_Pausa/*.bmp           <- fondos, barra de selección, barras HP/SP, iconos
  backdrops/P3R_Pausa.bmp
  fonts/23-P3R-Menu.fnt         <- cursiva condensada grande (menú, títulos)
  fonts/15-P3R-Texto.fnt        <- texto general del sistema
  fonts/11-P3R-Mini.fnt         <- etiquetas pequeñas
build/                          <- generador (C# vía PowerShell, System.Drawing)
arte/                           <- tus imágenes opcionales
vista_previa/                   <- capturas aproximadas
```

## Notas

- Las fuentes se generan desde *Bahnschrift* (incluida en Windows 10/11) en cursiva y
  con antialias. Cubren latín básico y extendido (acentos, ñ, ¿¡, comillas tipográficas).
  **No incluyen japonés**: los títulos en kanji o kana se verán como `?`. Si los
  necesitas, en *Ajustes → Tema → Fuente* elige una fuente CJK del fontpack. El menú
  de pausa seguirá usando la fuente propia.
- Los nombres de los menús salen en el idioma que tengas configurado en Rockbox.
- Los skins se validaron con el parser oficial de Rockbox (`lib/skin_parser`).
- Si usas la build **360p**, convierte el tema con
  [Rockbox-Theme-Rescaler](https://github.com/TmosDHD/Rockbox-Theme-Rescaler).

Fan-art sin ánimo de lucro. Persona 3 Reload © ATLUS / SEGA.
