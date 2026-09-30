// Generador del tema "P3R_Pausa" para Rockbox 240p (320x240, igual que iPod Classic / ipod6g)
// Crea fuentes .fnt (formato RB12, antialias 4 bits) y todas las imagenes BMP del tema.
using System;
using System.IO;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.Drawing.Text;

public static class P3R
{
    const int W = 320, H = 240;
    static string Raiz;      // carpeta .rockbox de salida
    static string Arte;      // carpeta con imagenes opcionales del usuario
    static string Img;       // .rockbox/wps/P3R_Pausa

    // Paleta del menu de pausa de P3R
    static readonly Color Navy = Hex("020818");
    static readonly Color Cyan = Hex("3CC8FF");
    static readonly Color Ice = Hex("9EE8FF");
    static readonly Color Lime = Hex("C8FF4A");
    static readonly Color Pink = Hex("FF5AC8");
    static readonly Color Moon = Hex("F2F5CC");
    static readonly Color Key = Color.FromArgb(255, 0, 255);

    static Color Hex(string h) { return Hex(h, 255); }
    static Color Hex(string h, int a)
    {
        return Color.FromArgb(a, Convert.ToInt32(h.Substring(0, 2), 16),
            Convert.ToInt32(h.Substring(2, 2), 16), Convert.ToInt32(h.Substring(4, 2), 16));
    }

    public static string Run(string rockboxDir, string arteDir)
    {
        Raiz = rockboxDir; Arte = arteDir;
        Img = Path.Combine(Raiz, "wps", "P3R_Pausa");
        Directory.CreateDirectory(Img);
        Directory.CreateDirectory(Path.Combine(Raiz, "fonts"));
        Directory.CreateDirectory(Path.Combine(Raiz, "backdrops"));

        string log = "";
        log += Fuente("Bahnschrift SemiBold Condensed", 23f, 0.22f, 1, "23-P3R-Menu.fnt");
        log += Fuente("Bahnschrift SemiBold SemiConden", 15f, 0.14f, 1, "15-P3R-Texto.fnt");
        log += Fuente("Bahnschrift SemiBold SemiConden", 11f, 0.12f, 1, "11-P3R-Mini.fnt");

        FondoMenu();
        FondoWps();
        Seleccion();
        Barra("bat_fill.bmp", 78, 4, Hex("E4FF8A"), Hex("8CD62A"));
        Barra("pb_fill.bmp", 136, 8, Hex("E4FF8A"), Hex("7CCB22"));
        Barra("vol_fill.bmp", 136, 6, Hex("FF9CE0"), Hex("D8288E"));
        Iconos();
        SinPortada();
        return log;
    }

    // ------------------------------------------------------------------ fuentes
    static int[] Codigos()
    {
        var l = new List<int>();
        for (int c = 0x20; c <= 0x7E; c++) l.Add(c);
        for (int c = 0xA0; c <= 0x17F; c++) l.Add(c);
        int[] extra = { 0x2013, 0x2014, 0x2018, 0x2019, 0x201C, 0x201D, 0x2022, 0x2026 };
        l.AddRange(extra);
        return l.ToArray();
    }

    static string Fuente(string familia, float em, float inclinacion, int espaciado, string archivo)
    {
        var ff = new FontFamily(familia);
        FontStyle st = FontStyle.Regular;
        float emH = ff.GetEmHeight(st);
        float asc = ff.GetCellAscent(st) * em / emH;
        float desc = ff.GetCellDescent(st) * em / emH;
        int ascent = (int)Math.Ceiling(asc);
        int alto = ascent + (int)Math.Ceiling(desc);

        int[] codigos = Codigos();
        int primero = codigos[0], ultimo = codigos[codigos.Length - 1];
        var glifos = new Dictionary<int, byte[,]>();   // cobertura [x,y]
        var fmt = StringFormat.GenericTypographic;
        var sesgo = new Matrix(1, 0, -inclinacion, 1, inclinacion * ascent, 0);
        int espacio = (int)Math.Round(em * 0.27f);

        // primera pasada: geometria
        var cajas = new Dictionary<int, RectangleF>();
        var trazos = new Dictionary<int, GraphicsPath>();
        var rectas = new Dictionary<int, RectangleF>();
        // la cursiva desborda la celda; solo reservamos ~40% del desplazamiento para que no se separen las letras
        int extraCursiva = (int)Math.Round(inclinacion * ascent * 0.4f);
        foreach (int c in codigos)
        {
            if (c == 0xA0 || c == 0x20) { cajas[c] = RectangleF.Empty; continue; }
            var p = new GraphicsPath();
            p.AddString(((char)c).ToString(), ff, (int)st, em, new PointF(0, 0), fmt);
            var recta = p.GetBounds();
            p.Transform(sesgo);
            var b = p.GetBounds();
            if (b.Width <= 0 || b.Height <= 0) { cajas[c] = RectangleF.Empty; continue; }
            cajas[c] = b; trazos[c] = p; rectas[c] = recta;
        }
        // digitos del mismo ancho para que el reloj no "baile"
        int anchoDigito = 0;
        for (int c = '0'; c <= '9'; c++)
            anchoDigito = Math.Max(anchoDigito, (int)Math.Ceiling(rectas[c].Width) + extraCursiva);

        var anchos = new Dictionary<int, int>();
        foreach (int c in codigos)
        {
            RectangleF b = cajas[c];
            if (b.IsEmpty)
            {
                anchos[c] = espacio;
                glifos[c] = new byte[espacio, alto];
                continue;
            }
            int w = (int)Math.Ceiling(rectas[c].Width) + extraCursiva;
            if (c >= '0' && c <= '9') w = anchoDigito;
            // centra el glifo inclinado en la celda (recorta por igual ambas puntas)
            float dx = -b.Left + (w - b.Width) / 2f;
            w += espaciado;
            var bmp = new Bitmap(w, alto, PixelFormat.Format32bppArgb);
            using (var g = Graphics.FromImage(bmp))
            {
                g.Clear(Color.Black);
                g.SmoothingMode = SmoothingMode.AntiAlias;
                g.PixelOffsetMode = PixelOffsetMode.HighQuality;
                g.TranslateTransform(dx, 0);
                g.FillPath(Brushes.White, trazos[c]);
            }
            var cov = new byte[w, alto];
            for (int y = 0; y < alto; y++)
                for (int x = 0; x < w; x++)
                    cov[x, y] = (byte)Math.Min(255, bmp.GetPixel(x, y).R * 1.12);
            bmp.Dispose();
            anchos[c] = w; glifos[c] = cov;
        }

        // empaquetado RB12, 4 bits por pixel, fila a fila, nibble bajo primero, 15 = vacio
        int tam = ultimo - primero + 1;
        var datos = new List<byte>();
        var offsets = new int[tam];
        var anchoTabla = new byte[tam];
        int maxAncho = 1;
        int defecto = '?';
        var presentes = new HashSet<int>(codigos);
        foreach (int c in codigos)
        {
            offsets[c - primero] = datos.Count;
            int w = anchos[c];
            anchoTabla[c - primero] = (byte)w;
            maxAncho = Math.Max(maxAncho, w);
            var cov = glifos[c];
            int campo = 0, n = 0;
            for (int y = 0; y < alto; y++)
                for (int x = 0; x < w; x++)
                {
                    int v = (255 - cov[x, y] + 8) / 17;
                    campo |= v << (4 * n);
                    if (++n == 2) { datos.Add((byte)campo); campo = 0; n = 0; }
                }
            if (n != 0) datos.Add((byte)campo);
        }
        for (int c = primero; c <= ultimo; c++)
            if (!presentes.Contains(c))
            {
                offsets[c - primero] = offsets[defecto - primero];
                anchoTabla[c - primero] = anchoTabla[defecto - primero];
            }

        bool largo = datos.Count >= 0xFFDB;
        string ruta = Path.Combine(Raiz, "fonts", archivo);
        using (var bw = new BinaryWriter(File.Create(ruta)))
        {
            bw.Write(new byte[] { (byte)'R', (byte)'B', (byte)'1', (byte)'2' });
            bw.Write((ushort)maxAncho);
            bw.Write((ushort)alto);
            bw.Write((ushort)ascent);
            bw.Write((ushort)1);                 // profundidad: 4 bits (antialias)
            bw.Write((uint)primero);
            bw.Write((uint)defecto);
            bw.Write((uint)tam);
            bw.Write((uint)datos.Count);
            bw.Write((uint)tam);                 // noffset
            bw.Write((uint)tam);                 // nwidth
            bw.Write(datos.ToArray());
            int alinear = largo ? 4 : 2;
            int pad = ((datos.Count + alinear - 1) / alinear) * alinear - datos.Count;
            for (int i = 0; i < pad; i++) bw.Write((byte)0);
            foreach (int o in offsets) { if (largo) bw.Write((uint)o); else bw.Write((ushort)o); }
            bw.Write(anchoTabla);
        }
        return string.Format("{0}: alto={1} ascent={2} maxancho={3} bytes={4}\n", archivo, alto, ascent, maxAncho, new FileInfo(ruta).Length);
    }

    // ------------------------------------------------------------------ utilidades
    static Bitmap Lienzo() { return new Bitmap(W, H, PixelFormat.Format32bppArgb); }

    static Graphics G(Bitmap b)
    {
        var g = Graphics.FromImage(b);
        g.SmoothingMode = SmoothingMode.AntiAlias;
        g.InterpolationMode = InterpolationMode.HighQualityBicubic;
        g.PixelOffsetMode = PixelOffsetMode.HighQuality;
        g.TextRenderingHint = TextRenderingHint.AntiAliasGridFit;
        return g;
    }

    static void Guardar(Bitmap b, string ruta)
    {
        using (var o = new Bitmap(b.Width, b.Height, PixelFormat.Format24bppRgb))
        {
            using (var g = Graphics.FromImage(o)) g.DrawImage(b, 0, 0, b.Width, b.Height);
            o.Save(ruta, ImageFormat.Bmp);
        }
    }

    static Image Opcional(string nombre)
    {
        foreach (var ext in new[] { ".png", ".jpg", ".jpeg", ".bmp" })
        {
            string p = Path.Combine(Arte, nombre + ext);
            if (File.Exists(p)) return Image.FromFile(p);
        }
        return null;
    }

    static void Poli(Graphics g, Color c, params float[] xy)
    {
        var pts = new PointF[xy.Length / 2];
        for (int i = 0; i < pts.Length; i++) pts[i] = new PointF(xy[2 * i], xy[2 * i + 1]);
        using (var br = new SolidBrush(c)) g.FillPolygon(br, pts);
    }

    static void Texto(Graphics g, string s, string familia, float px, Color c, float x, float y, float inclinacion, float espaciado)
    {
        var ff = new FontFamily(familia);
        var st = g.Save();
        g.TranslateTransform(x, y);
        g.MultiplyTransform(new Matrix(1, 0, -inclinacion, 1, inclinacion * px, 0));
        float cx = 0;
        using (var br = new SolidBrush(c))
        using (var f = new Font(ff, px, FontStyle.Regular, GraphicsUnit.Pixel))
            foreach (char ch in s)
            {
                var p = new GraphicsPath();
                p.AddString(ch.ToString(), ff, 0, px, new PointF(cx, 0), StringFormat.GenericTypographic);
                g.FillPath(br, p);
                float adv = ch == ' ' ? px * 0.3f : g.MeasureString(ch.ToString(), f, 100, StringFormat.GenericTypographic).Width;
                cx += adv + espaciado;
            }
        g.Restore(st);
    }

    static void Cubrir(Graphics g, Image im, RectangleF r)
    {
        float s = Math.Max(r.Width / im.Width, r.Height / im.Height);
        float w = im.Width * s, h = im.Height * s;
        g.DrawImage(im, r.X + (r.Width - w) / 2, r.Y + (r.Height - h) / 2, w, h);
    }

    // ------------------------------------------------------------------ fondo comun
    static void Base(Graphics g, int semilla)
    {
        var rect = new Rectangle(0, 0, W, H);
        using (var lg = new LinearGradientBrush(rect, Color.Black, Color.White, 38f))
        {
            var cb = new ColorBlend();
            cb.Colors = new[] { Hex("030A26"), Hex("0A2A78"), Hex("1778D8") };
            cb.Positions = new[] { 0f, 0.55f, 1f };
            lg.InterpolationColors = cb;
            g.FillRectangle(lg, rect);
        }
        var fondo = Opcional("fondo");
        if (fondo != null)
        {
            Cubrir(g, fondo, rect);
            using (var br = new SolidBrush(Hex("041448", 150))) g.FillRectangle(br, rect);
        }
        // resplandor turquesa
        var gp = new GraphicsPath();
        gp.AddEllipse(90, -40, 360, 330);
        using (var pg = new PathGradientBrush(gp))
        {
            pg.CenterColor = Hex("5AD8FF", 120);
            pg.SurroundColors = new[] { Color.FromArgb(0, 30, 120, 220) };
            g.FillPath(pg, gp);
        }
        // haces de luz diagonales (estilo transicion de P3R)
        float[,] haces = { { 40, 18, 22 }, { 96, 6, 30 }, { 170, 34, 16 }, { 232, 10, 28 }, { 300, 22, 18 }, { 360, 8, 26 } };
        for (int i = 0; i < haces.GetLength(0); i++)
        {
            float x = haces[i, 0], w = haces[i, 1];
            Poli(g, Color.FromArgb((int)haces[i, 2], 200, 240, 255), x, 0, x + w, 0, x + w - 110, H, x - 110, H);
        }
        // ondas de agua
        for (int i = 0; i < 9; i++)
        {
            float baseY = 118 + i * 14, amp = 2.5f + i * 0.6f, fase = i * 1.7f;
            var pts = new List<PointF>();
            for (int x = -4; x <= W + 4; x += 4)
                pts.Add(new PointF(x, baseY + amp * (float)Math.Sin(x * 0.035 + fase) + 1.5f * (float)Math.Sin(x * 0.11 + fase * 2)));
            using (var pen = new Pen(Color.FromArgb(18 + i * 5, 158, 232, 255), 1f)) g.DrawCurve(pen, pts.ToArray());
        }
        // particulas
        var rnd = new Random(semilla);
        for (int i = 0; i < 70; i++)
        {
            float x = rnd.Next(W), y = rnd.Next(H), r = (float)rnd.NextDouble() * 1.4f + 0.4f;
            using (var br = new SolidBrush(Color.FromArgb(rnd.Next(40, 150), 220, 248, 255))) g.FillEllipse(br, x, y, r, r);
        }
    }

    // Placas superiores: fecha (izq) y reloj + "HP" bateria (der)
    static void PlacasSuperiores(Graphics g)
    {
        Poli(g, Hex("020818", 228), 0, 0, 172, 0, 158, 34, 0, 34);
        Poli(g, Cyan, 0, 34, 158, 34, 156, 37, 0, 37);
        using (var pen = new Pen(Color.FromArgb(170, 255, 255, 255), 2f)) g.DrawLine(pen, 84, 5, 78, 30);

        Poli(g, Hex("020818", 228), 196, 0, W, 0, W, 34, 184, 34);
        Poli(g, Cyan, 184, 34, W, 34, W, 37, 183, 37);
        Texto(g, "HP", "Bahnschrift SemiBold Condensed", 11, Lime, 208, 19, 0.2f, 0.5f);
        using (var br = new SolidBrush(Hex("0B1330"))) g.FillRectangle(br, 230, 23, 80, 6);
        using (var pen = new Pen(Hex("3A5AA0"))) g.DrawRectangle(pen, 230, 23, 79, 5);
    }

    // Emblema original: luna llena + reloj marcando medianoche (la Hora Oscura)
    static void Emblema(Graphics g, float cx, float cy)
    {
        var gp = new GraphicsPath();
        gp.AddEllipse(cx - 80, cy - 80, 160, 160);
        using (var pg = new PathGradientBrush(gp))
        {
            pg.CenterColor = Color.FromArgb(120, 230, 250, 200);
            pg.SurroundColors = new[] { Color.FromArgb(0, 230, 250, 200) };
            g.FillPath(pg, gp);
        }
        using (var pen = new Pen(Color.FromArgb(80, 60, 200, 255), 2f)) g.DrawEllipse(pen, cx - 86, cy - 86, 172, 172);
        using (var pen = new Pen(Color.FromArgb(60, 255, 255, 255), 1f)) g.DrawEllipse(pen, cx - 74, cy - 74, 148, 148);
        for (int i = 0; i < 12; i++)
        {
            double a = i * Math.PI / 6 - Math.PI / 2;
            float r1 = i == 0 ? 58 : 64, r2 = 74;
            using (var pen = new Pen(i == 0 ? Cyan : Color.FromArgb(140, 255, 255, 255), i % 3 == 0 ? 3f : 1.5f))
                g.DrawLine(pen, cx + r1 * (float)Math.Cos(a), cy + r1 * (float)Math.Sin(a), cx + r2 * (float)Math.Cos(a), cy + r2 * (float)Math.Sin(a));
        }
        using (var br = new SolidBrush(Color.FromArgb(235, Moon))) g.FillEllipse(br, cx - 50, cy - 50, 100, 100);
        float[,] crateres = { { -18, -14, 22 }, { 12, 8, 30 }, { -6, 22, 14 }, { 20, -22, 12 }, { -28, 10, 10 } };
        for (int i = 0; i < crateres.GetLength(0); i++)
        {
            float r = crateres[i, 2];
            using (var br = new SolidBrush(Color.FromArgb(38, 120, 140, 90)))
                g.FillEllipse(br, cx + crateres[i, 0] - r / 2, cy + crateres[i, 1] - r / 2, r, r);
        }
        // manecillas superpuestas en las 12
        using (var pen = new Pen(Hex("0A1440", 220), 5f)) { pen.EndCap = LineCap.Triangle; g.DrawLine(pen, cx, cy + 6, cx, cy - 44); }
        using (var pen = new Pen(Hex("0A1440", 220), 3f)) { pen.EndCap = LineCap.Triangle; g.DrawLine(pen, cx + 1.5f, cy + 6, cx + 1.5f, cy - 56); }
        using (var br = new SolidBrush(Hex("0A1440"))) g.FillEllipse(br, cx - 4, cy - 4, 8, 8);
    }

    static void Personaje(Graphics g, Image im, float maxAncho)
    {
        float s = (float)H / im.Height;
        float w = im.Width * s;
        if (w > maxAncho) { s = maxAncho / im.Width; w = maxAncho; }
        float h = im.Height * s;
        float x = W - w, y = H - h;
        g.DrawImage(im, x, y, w, h);
        using (var lg = new LinearGradientBrush(new RectangleF(x - 1, 0, 46, H), Hex("061A55", 210), Hex("061A55", 0), 0f))
            g.FillRectangle(lg, x, 0, 45, H);
    }

    // ------------------------------------------------------------------ menu (SBS)
    static void FondoMenu()
    {
        var b = Lienzo();
        using (var g = G(b))
        {
            Base(g, 3);
            var pj = Opcional("personaje");
            if (pj != null) Personaje(g, pj, 170);
            else
            {
                Emblema(g, 262, 124);
                Texto(g, "MEMENTO MORI", "Bahnschrift SemiBold Condensed", 10, Color.FromArgb(150, 158, 232, 255), 222, 212, 0.2f, 2.2f);
            }
            // sombreado detras de la lista
            using (var lg = new LinearGradientBrush(new Rectangle(0, 0, 222, H), Hex("020A2A", 190), Hex("020A2A", 0), 0f))
                g.FillRectangle(lg, 0, 0, 221, H);
            PlacasSuperiores(g);
            // placa del titulo de la seccion
            Poli(g, Hex("0A2A6E", 215), 0, 41, 146, 41, 140, 55, 0, 55);
            Poli(g, Cyan, 0, 41, 3, 41, 3, 55, 0, 55);
            // placa inferior (reproduciendo)
            Poli(g, Hex("020818", 215), 0, 226, 250, 226, 244, 240, 0, 240);
            Poli(g, Cyan, 0, 225, 251, 225, 250, 226, 0, 226);
        }
        Guardar(b, Path.Combine(Img, "fondo_menu.bmp"));
        Guardar(b, Path.Combine(Raiz, "backdrops", "P3R_Pausa.bmp"));
        b.Dispose();
    }

    // Barra de seleccion del menu: blanco inclinado con sombra turquesa (magenta = transparente)
    static void Seleccion()
    {
        var b = new Bitmap(212, 28, PixelFormat.Format24bppRgb);
        using (var g = Graphics.FromImage(b))
        {
            g.SmoothingMode = SmoothingMode.None;
            g.Clear(Key);
            Poli(g, Cyan, 12, 5, 212, 5, 206, 28, 6, 28);
            Poli(g, Color.White, 4, 1, 207, 1, 201, 25, 0, 25);
            Poli(g, Hex("0A1440"), 5, 7, 12, 13, 5, 19);
        }
        b.Save(Path.Combine(Img, "sel.bmp"), ImageFormat.Bmp);
        b.Dispose();
    }

    // ------------------------------------------------------------------ reproduccion (WPS)
    static void FondoWps()
    {
        var b = Lienzo();
        using (var g = G(b))
        {
            Base(g, 11);
            var pj = Opcional("personaje_wps");
            if (pj != null)
            {
                var st = g.Save();
                using (var ia = new ImageAttributes())
                {
                    var cm = new ColorMatrix(); cm.Matrix33 = 0.35f; ia.SetColorMatrix(cm);
                    float s = (float)H / pj.Height, w = pj.Width * s;
                    g.DrawImage(pj, new Rectangle((int)(W - w), 0, (int)w, H), 0, 0, pj.Width, pj.Height, GraphicsUnit.Pixel, ia);
                }
                g.Restore(st);
            }
            PlacasSuperiores(g);

            // tarjeta de la portada
            float cx = 76, cy = 120;
            var st2 = g.Save();
            g.TranslateTransform(cx + 5, cy + 5); g.RotateTransform(-9);
            using (var br = new SolidBrush(Color.FromArgb(210, Cyan))) g.FillRectangle(br, -67, -67, 134, 134);
            g.Restore(st2);
            st2 = g.Save();
            g.TranslateTransform(cx, cy); g.RotateTransform(-4);
            g.FillRectangle(Brushes.White, -65, -65, 130, 130);
            g.Restore(st2);
            using (var br = new SolidBrush(Navy)) g.FillRectangle(br, 18, 62, 116, 116);

            // etiqueta NOW PLAYING
            Poli(g, Cyan, 150, 44, 262, 44, 257, 59, 145, 59);
            Texto(g, "NOW PLAYING", "Bahnschrift SemiBold Condensed", 12, Hex("0A1440"), 155, 45, 0.2f, 1.2f);
            // banda del titulo
            Poli(g, Hex("020818", 150), 146, 63, W, 63, W, 96, 140, 96);
            Poli(g, Color.FromArgb(200, Ice), 146, 63, W, 63, W, 64, 146, 64);

            // HP = progreso, SP = volumen
            Texto(g, "HP", "Bahnschrift SemiBold Condensed", 14, Lime, 150, 138, 0.2f, 0.5f);
            using (var br = new SolidBrush(Hex("0B1330"))) g.FillRectangle(br, 176, 143, 136, 8);
            using (var pen = new Pen(Hex("3A5AA0"))) g.DrawRectangle(pen, 175, 142, 137, 9);
            Texto(g, "SP", "Bahnschrift SemiBold Condensed", 14, Pink, 150, 170, 0.2f, 0.5f);
            using (var br = new SolidBrush(Hex("0B1330"))) g.FillRectangle(br, 176, 176, 136, 6);
            using (var pen = new Pen(Hex("3A5AA0"))) g.DrawRectangle(pen, 175, 175, 137, 7);

            // placa inferior: siguiente pista
            Poli(g, Hex("020818", 215), 0, 203, W, 197, W, H, 0, H);
            Poli(g, Cyan, 0, 202, W, 196, W, 198, 0, 204);
            Poli(g, Cyan, 0, 207, 44, 207, 39, 220, 0, 220);
            Texto(g, "NEXT", "Bahnschrift SemiBold Condensed", 11, Hex("0A1440"), 5, 207, 0.2f, 1f);
        }
        Guardar(b, Path.Combine(Img, "fondo_wps.bmp"));
        b.Dispose();
    }

    static void Barra(string nombre, int w, int h, Color arriba, Color abajo)
    {
        var b = new Bitmap(w, h, PixelFormat.Format24bppRgb);
        using (var g = Graphics.FromImage(b))
        using (var lg = new LinearGradientBrush(new Rectangle(0, 0, w, h + 1), arriba, abajo, 90f))
        {
            g.FillRectangle(lg, 0, 0, w, h);
            using (var pen = new Pen(Color.FromArgb(160, 255, 255, 255))) g.DrawLine(pen, 0, 0, w, 0);
        }
        b.Save(Path.Combine(Img, nombre), ImageFormat.Bmp);
        b.Dispose();
    }

    static void Iconos()
    {
        string[] nombres = { "ico_play", "ico_pausa", "ico_stop", "ico_ff", "ico_rew" };
        for (int i = 0; i < nombres.Length; i++)
        {
            var b = new Bitmap(14, 14, PixelFormat.Format24bppRgb);
            using (var g = Graphics.FromImage(b))
            {
                g.SmoothingMode = SmoothingMode.None;
                g.Clear(Key);
                Color c = Color.White;
                switch (i)
                {
                    case 0: Poli(g, c, 2, 1, 13, 7, 2, 13); break;
                    case 1: Poli(g, c, 3, 1, 6, 1, 5, 13, 2, 13); Poli(g, c, 9, 1, 12, 1, 11, 13, 8, 13); break;
                    case 2: Poli(g, c, 3, 2, 13, 2, 11, 12, 1, 12); break;
                    case 3: Poli(g, c, 0, 2, 7, 7, 0, 12); Poli(g, c, 7, 2, 14, 7, 7, 12); break;
                    case 4: Poli(g, c, 7, 2, 0, 7, 7, 12); Poli(g, c, 14, 2, 7, 7, 14, 12); break;
                }
            }
            b.Save(Path.Combine(Img, nombres[i] + ".bmp"), ImageFormat.Bmp);
            b.Dispose();
        }
    }

    // Imagen para pistas sin caratula (116x116)
    static void SinPortada()
    {
        var b = new Bitmap(116, 116, PixelFormat.Format32bppArgb);
        using (var g = G(b))
        {
            var propia = Opcional("sin_portada");
            if (propia != null) Cubrir(g, propia, new RectangleF(0, 0, 116, 116));
            else
            {
                using (var lg = new LinearGradientBrush(new Rectangle(0, 0, 116, 116), Hex("06103A"), Hex("1450B4"), 60f))
                    g.FillRectangle(lg, 0, 0, 116, 116);
                using (var pen = new Pen(Color.FromArgb(160, Ice), 1f)) g.DrawRectangle(pen, 5, 5, 105, 105);
                g.TranslateTransform(58, 52); g.ScaleTransform(0.55f, 0.55f); g.TranslateTransform(-58, -52);
                Emblema(g, 58, 52);
                g.ResetTransform();
                Texto(g, "MEMENTO MORI", "Bahnschrift SemiBold Condensed", 10, Ice, 22, 94, 0.2f, 1.6f);
            }
        }
        Guardar(b, Path.Combine(Img, "sin_portada.bmp"));
        b.Dispose();
    }
}
