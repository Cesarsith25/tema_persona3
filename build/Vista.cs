// Simulador simple: dibuja capturas aproximadas del tema usando las fuentes .fnt reales
using System;
using System.IO;
using System.Drawing;
using System.Drawing.Imaging;

public class Fnt
{
    public int Alto, Primero, Tam; int Prof;
    byte[] bits; int[] offs; byte[] anchos; int defecto;
    public Fnt(string ruta)
    {
        var d = File.ReadAllBytes(ruta);
        var r = new BinaryReader(new MemoryStream(d));
        r.ReadBytes(4); r.ReadUInt16(); Alto = r.ReadUInt16(); r.ReadUInt16(); Prof = r.ReadUInt16();
        Primero = (int)r.ReadUInt32(); defecto = (int)r.ReadUInt32(); Tam = (int)r.ReadUInt32();
        int nbits = (int)r.ReadUInt32(); int noff = (int)r.ReadUInt32(); r.ReadUInt32();
        bits = r.ReadBytes(nbits);
        bool largo = nbits >= 0xFFDB; int al = largo ? 4 : 2;
        r.ReadBytes(((nbits + al - 1) / al) * al - nbits);
        offs = new int[noff];
        for (int i = 0; i < noff; i++) offs[i] = largo ? (int)r.ReadUInt32() : r.ReadUInt16();
        anchos = r.ReadBytes(Tam);
    }
    public int Dibujar(Bitmap b, string s, int x0, int y0, Color c, Rectangle clip)
    {
        int x = x0;
        foreach (char ch in s)
        {
            int i = ch - Primero; if (i < 0 || i >= Tam) i = defecto - Primero;
            int w = anchos[i], o = offs[i], n = 0;
            for (int y = 0; y < Alto; y++)
                for (int xx = 0; xx < w; xx++, n++)
                {
                    int v = (bits[o + n / 2] >> (4 * (n % 2))) & 15;
                    int a = 255 - v * 17; int px = x + xx, py = y0 + y;
                    if (a == 0 || !clip.Contains(px, py)) continue;
                    var f = b.GetPixel(px, py);
                    b.SetPixel(px, py, Color.FromArgb((c.R * a + f.R * (255 - a)) / 255, (c.G * a + f.G * (255 - a)) / 255, (c.B * a + f.B * (255 - a)) / 255));
                }
            x += w;
        }
        return x - x0;
    }
    public int Ancho(string s) { int t = 0; foreach (char ch in s) { int i = ch - Primero; if (i < 0 || i >= Tam) i = defecto - Primero; t += anchos[i]; } return t; }
}

public static class Vista
{
    static Fnt F2, F3, F4; static string Img;
    static Color Hx(string h) { return Color.FromArgb(Convert.ToInt32(h.Substring(0, 2), 16), Convert.ToInt32(h.Substring(2, 2), 16), Convert.ToInt32(h.Substring(4, 2), 16)); }

    static void Imagen(Bitmap b, string nombre, int x, int y)
    {
        using (var im = new Bitmap(Path.Combine(Img, nombre)))
            for (int j = 0; j < im.Height; j++)
                for (int i = 0; i < im.Width; i++)
                {
                    var c = im.GetPixel(i, j);
                    if (c.R == 255 && c.G == 0 && c.B == 255) continue;
                    if (x + i < b.Width && y + j < b.Height) b.SetPixel(x + i, y + j, c);
                }
    }
    static void Barra(Bitmap b, string nombre, int x, int y, int w, int h, double frac)
    {
        using (var im = new Bitmap(Path.Combine(Img, nombre)))
            for (int j = 0; j < h; j++) for (int i = 0; i < (int)(w * frac); i++) b.SetPixel(x + i, y + j, im.GetPixel(i, j));
    }
    // texto dentro de un viewport; alin: 0 izq, 2 der
    static void T(Bitmap b, Fnt f, string s, int vx, int vy, int vw, int vh, string col, int alin)
    {
        int x = alin == 2 ? vx + vw - f.Ancho(s) : vx;
        f.Dibujar(b, s, x, vy, Hx(col), new Rectangle(vx, vy, vw, vh));
    }

    public static void Run(string rb, string salida)
    {
        Img = Path.Combine(rb, "wps", "P3R_Pausa");
        F2 = new Fnt(Path.Combine(rb, "fonts", "23-P3R-Menu.fnt"));
        F3 = new Fnt(Path.Combine(rb, "fonts", "15-P3R-Texto.fnt"));
        F4 = new Fnt(Path.Combine(rb, "fonts", "11-P3R-Mini.fnt"));

        // ---- menu
        var m = new Bitmap(Path.Combine(Img, "fondo_menu.bmp"));
        m = new Bitmap(m);
        T(m, F2, "09/30", 4, 5, 74, 26, "ffffff", 0);
        T(m, F3, "Mié", 90, 3, 70, 17, "ffffff", 0);
        T(m, F4, "Evening", 89, 19, 68, 13, "3cc8ff", 0);
        T(m, F3, "18:45", 196, 2, 116, 17, "ffffff", 2);
        Barra(m, "bat_fill.bmp", 231, 24, 78, 4, 0.7);
        T(m, F4, "Rockbox", 8, 42, 130, 13, "9ee8ff", 0);
        string[] items = { "Archivos", "Base de datos", "Reproduciendo", "Ajustes", "Grabación", "Plugins" };
        for (int i = 0; i < items.Length; i++)
        {
            int y = 58 + i * 28;
            if (i == 1) { Imagen(m, "sel.bmp", 0, y); T(m, F2, items[i], 15, y + 1, 190, 26, "0a1440", 0); }
            else T(m, F2, items[i], 6, y + 1, 200, 26, "ffffff", 0);
        }
        Imagen(m, "ico_play.bmp", 5, 226);
        T(m, F4, "Burn My Dread · Yumi Kawamura", 22, 227, 218, 13, "bfe9ff", 0);
        m.Save(Path.Combine(salida, "vista_menu.png"), ImageFormat.Png);

        // ---- reproduccion
        var w = new Bitmap(new Bitmap(Path.Combine(Img, "fondo_wps.bmp")));
        T(w, F2, "09/30", 4, 5, 74, 26, "ffffff", 0);
        T(w, F3, "Mié", 90, 3, 70, 17, "ffffff", 0);
        T(w, F4, "DARK HOUR", 89, 19, 68, 13, "7cff6a", 0);
        T(w, F3, "00:00", 196, 2, 116, 17, "ffffff", 2);
        Barra(w, "bat_fill.bmp", 231, 24, 78, 4, 0.45);
        Imagen(w, "sin_portada.bmp", 18, 62);
        Imagen(w, "ico_play.bmp", 266, 45);
        T(w, F2, "Full Moon Full Life", 150, 67, 166, 26, "ffffff", 0);
        T(w, F3, "Lotus Juice & Azumi Takahashi", 150, 98, 166, 17, "9ee8ff", 0);
        T(w, F4, "Persona 3 Reload Original Soundtrack", 150, 117, 166, 13, "7fa8e0", 0);
        Barra(w, "pb_fill.bmp", 176, 143, 136, 8, 0.38);
        T(w, F4, "1:32", 176, 153, 136, 13, "dff6ff", 0);
        T(w, F4, "-2:41", 176, 153, 136, 13, "dff6ff", 2);
        Barra(w, "vol_fill.bmp", 176, 176, 136, 6, 0.7);
        T(w, F4, "It's Going Down Now · Lotus Juice", 50, 207, 264, 13, "dff6ff", 0);
        T(w, F4, "SHUFFLE  REPEAT", 8, 224, 304, 13, "3cc8ff", 0);
        T(w, F4, "3 / 24", 8, 224, 304, 13, "3cc8ff", 2);
        w.Save(Path.Combine(salida, "vista_reproduccion.png"), ImageFormat.Png);
    }
}
