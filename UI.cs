using System.Globalization;
using System.Net.Mail;

namespace Oficina;

/// <summary>Cores, componentes visuais e utilitários compartilhados.</summary>
public static class UI
{
    public static readonly Color SidebarColor = Color.FromArgb(30, 41, 59);
    public static readonly Color SidebarHover = Color.FromArgb(51, 65, 85);
    public static readonly Color Accent = Color.FromArgb(37, 99, 235);
    public static readonly Color Green = Color.FromArgb(22, 163, 74);
    public static readonly Color Red = Color.FromArgb(220, 38, 38);
    public static readonly Color Orange = Color.FromArgb(234, 88, 12);
    public static readonly Color Bg = Color.FromArgb(241, 245, 249);
    public static readonly Color Ink = Color.FromArgb(15, 23, 42);
    public static readonly Color Muted = Color.FromArgb(100, 116, 139);
    public static readonly Font BaseFont = new("Segoe UI", 10f);

    // ---------- Componentes ----------
    public static Label Title(string text) => new()
    {
        Text = text, AutoSize = true, ForeColor = Ink,
        Font = new Font("Segoe UI", 18f, FontStyle.Bold)
    };

    public static Label Lbl(string text, int x, int y) => new()
    {
        Text = text, AutoSize = true, ForeColor = Muted, Location = new Point(x, y)
    };

    public static Button Btn(string text, Color cor, int x, int y, Action onClick, int w = 130)
    {
        var b = new Button
        {
            Text = text, Location = new Point(x, y), Size = new Size(w, 34),
            FlatStyle = FlatStyle.Flat, BackColor = cor, ForeColor = Color.White, Cursor = Cursors.Hand
        };
        b.FlatAppearance.BorderSize = 0;
        b.Click += (_, _) => onClick();
        return b;
    }

    public static NumericUpDown Money() => new()
    {
        DecimalPlaces = 2, Maximum = 10_000_000m, ThousandsSeparator = true, Width = 140
    };

    /// <summary>Rótulo acima + campo abaixo, posicionados em (x, y).</summary>
    public static void Campo(Control parent, string label, Control input, int x, int y, int w)
    {
        parent.Controls.Add(Lbl(label, x, y));
        input.Location = new Point(x, y + 22);
        input.Width = w;
        parent.Controls.Add(input);
    }

    public static DataGridView Grid()
    {
        var g = new DataGridView
        {
            ReadOnly = true,
            AllowUserToAddRows = false,
            AllowUserToDeleteRows = false,
            AllowUserToResizeRows = false,
            MultiSelect = false,
            SelectionMode = DataGridViewSelectionMode.FullRowSelect,
            AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill,
            BackgroundColor = Color.White,
            BorderStyle = BorderStyle.None,
            CellBorderStyle = DataGridViewCellBorderStyle.SingleHorizontal,
            RowHeadersVisible = false,
            EnableHeadersVisualStyles = false,
            ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.DisableResizing,
            ColumnHeadersHeight = 36
        };
        g.ColumnHeadersDefaultCellStyle.BackColor = SidebarColor;
        g.ColumnHeadersDefaultCellStyle.ForeColor = Color.White;
        g.ColumnHeadersDefaultCellStyle.SelectionBackColor = SidebarColor;
        g.ColumnHeadersDefaultCellStyle.Font = new Font("Segoe UI", 10f, FontStyle.Bold);
        g.DefaultCellStyle.SelectionBackColor = Color.FromArgb(219, 234, 254);
        g.DefaultCellStyle.SelectionForeColor = Ink;
        g.AlternatingRowsDefaultCellStyle.BackColor = Color.FromArgb(248, 250, 252);
        g.RowTemplate.Height = 30;
        return g;
    }

    public static Panel Card(string titulo, Color cor, out Label valor, int w = 200, int h = 86)
    {
        var p = new Panel { Size = new Size(w, h), BackColor = Color.White };
        p.Controls.Add(new Panel { Dock = DockStyle.Left, Width = 5, BackColor = cor });
        p.Controls.Add(new Label
        {
            Text = titulo, AutoSize = true, ForeColor = Muted, Location = new Point(16, 12)
        });
        valor = new Label
        {
            Text = "—", AutoSize = true, ForeColor = Ink, Location = new Point(16, 40),
            Font = new Font("Segoe UI", 15f, FontStyle.Bold)
        };
        p.Controls.Add(valor);
        return p;
    }

    // ---------- Mensagens ----------
    public static void Aviso(string msg) =>
        MessageBox.Show(msg, "Oficina", MessageBoxButtons.OK, MessageBoxIcon.Information);

    public static bool Confirma(string msg) =>
        MessageBox.Show(msg, "Oficina", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes;

    // ---------- Utilitários ----------
    public static string Iso(DateTime d) => d.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture);
    public static string Moeda(decimal v) => v.ToString("C2");

    public static string SoDigitos(string s) => new string((s ?? "").Where(char.IsDigit).ToArray());

    public static string FormatCpf(string d) =>
        d != null && d.Length == 11
            ? $"{d.Substring(0, 3)}.{d.Substring(3, 3)}.{d.Substring(6, 3)}-{d.Substring(9, 2)}"
            : d;

    public static bool CpfValido(string cpf)
    {
        cpf = SoDigitos(cpf);
        if (cpf.Length != 11 || cpf.Distinct().Count() == 1) return false;
        for (int t = 9; t < 11; t++)
        {
            int soma = 0;
            for (int i = 0; i < t; i++) soma += (cpf[i] - '0') * (t + 1 - i);
            int dv = (soma * 10) % 11 % 10;
            if (dv != cpf[t] - '0') return false;
        }
        return true;
    }

    public static bool EmailValido(string email)
    {
        try { return new MailAddress(email).Address == email; }
        catch { return false; }
    }
}

/// <summary>Base das telas: ocupa todo o conteúdo e monta cabeçalho + corpo.</summary>
public abstract class ViewBase : UserControl
{
    protected ViewBase()
    {
        Dock = DockStyle.Fill;
        BackColor = UI.Bg;
        Font = UI.BaseFont;
    }

    protected Panel TopPanel(string titulo, int altura)
    {
        var p = new Panel { Dock = DockStyle.Top, Height = altura, BackColor = UI.Bg };
        var t = UI.Title(titulo);
        t.Location = new Point(24, 16);
        p.Controls.Add(t);
        return p;
    }

    protected void Compose(Panel topo, Control corpo)
    {
        var host = new Panel { Dock = DockStyle.Fill, Padding = new Padding(24, 0, 24, 24) };
        corpo.Dock = DockStyle.Fill;
        host.Controls.Add(corpo);
        Controls.Add(host);   // Fill primeiro...
        Controls.Add(topo);   // ...depois o Top
    }
}
