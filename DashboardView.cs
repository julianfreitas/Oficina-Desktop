using System.Drawing.Drawing2D;
using System.Globalization;

namespace Oficina;

public class DashboardView : ViewBase
{
    class ChartPanel : Panel
    {
        public ChartPanel() { DoubleBuffered = true; ResizeRedraw = true; }
    }

    readonly DateTimePicker dtpMes = new()
    {
        Format = DateTimePickerFormat.Custom, CustomFormat = "MMMM 'de' yyyy", ShowUpDown = true
    };
    readonly Label lblFat, lblCustos, lblDesp, lblLucro;
    readonly Label lblResumo = new() { AutoSize = true, ForeColor = UI.Muted };
    readonly DataGridView grid = UI.Grid();
    readonly ChartPanel chart = new() { BackColor = Color.White };
    readonly List<(string Mes, decimal Fat, decimal Lucro)> dados = new();

    public DashboardView()
    {
        var top = TopPanel("Dashboard financeiro", 225);
        UI.Campo(top, "Mês de referência", dtpMes, 24, 60, 220);
        lblResumo.Location = new Point(270, 86);
        top.Controls.Add(lblResumo);

        var cards = new[]
        {
            UI.Card("Faturamento", UI.Accent, out lblFat),
            UI.Card("Custos dos serviços", UI.Orange, out lblCustos),
            UI.Card("Despesas gerais", UI.Orange, out lblDesp),
            UI.Card("Lucro final", UI.Green, out lblLucro),
        };
        for (int i = 0; i < cards.Length; i++)
        {
            cards[i].Location = new Point(24 + i * 212, 124);
            top.Controls.Add(cards[i]);
        }

        // Corpo: tabela dos serviços do mês (esquerda) + gráfico dos últimos 6 meses (direita)
        var body = new Panel();
        chart.Dock = DockStyle.Right;
        chart.Width = 460;
        chart.Paint += (_, e) => DesenharGrafico(e.Graphics, chart.ClientRectangle);
        var espaco = new Panel { Dock = DockStyle.Right, Width = 16 };
        grid.Dock = DockStyle.Fill;
        body.Controls.Add(grid);
        body.Controls.Add(espaco);
        body.Controls.Add(chart);

        Compose(top, body);
        dtpMes.ValueChanged += (_, _) => Atualizar();
        Atualizar();
    }

    static string Ym(DateTime d) => d.ToString("yyyy-MM", CultureInfo.InvariantCulture);

    void Atualizar()
    {
        var mes = dtpMes.Value;
        var ym = Ym(mes);

        var (fat, custos, desp) = Db.Totais(ym);
        var lucro = fat - custos - desp;
        var margem = fat > 0 ? lucro / fat * 100 : 0;
        var qtd = Convert.ToInt32(Db.Scalar(
            "SELECT COUNT(*) FROM OrdensServico WHERE strftime('%Y-%m', Data) = @m", ("@m", ym)));

        lblFat.Text = UI.Moeda(fat);
        lblCustos.Text = UI.Moeda(custos);
        lblDesp.Text = UI.Moeda(desp);
        lblLucro.Text = UI.Moeda(lucro);
        lblLucro.ForeColor = lucro >= 0 ? UI.Green : UI.Red;
        lblResumo.Text = $"{qtd} serviço(s) realizado(s)   •   Margem de lucro: {margem:N1}%";

        grid.DataSource = Db.Query(@"
            SELECT strftime('%d/%m/%Y', o.Data) AS Data, c.Nome AS Cliente, s.Nome AS Servico,
                   o.Valor, o.Custo, (o.Valor - o.Custo) AS Lucro
            FROM OrdensServico o
            JOIN Clientes c ON c.Id = o.ClienteId
            JOIN Servicos s ON s.Id = o.ServicoId
            WHERE strftime('%Y-%m', o.Data) = @m
            ORDER BY o.Data, o.Id", ("@m", ym));
        grid.Columns["Servico"].HeaderText = "Serviço";
        foreach (var c in new[] { "Valor", "Custo", "Lucro" })
            grid.Columns[c].DefaultCellStyle.Format = "C2";
        grid.ClearSelection();

        dados.Clear();
        for (int i = 5; i >= 0; i--)
        {
            var m = mes.AddMonths(-i);
            var t = Db.Totais(Ym(m));
            dados.Add((m.ToString("MMM/yy"), t.Faturamento, t.Faturamento - t.Custos - t.Despesas));
        }
        chart.Invalidate();
    }

    void DesenharGrafico(Graphics g, Rectangle r)
    {
        g.SmoothingMode = SmoothingMode.AntiAlias;
        using var fTitulo = new Font("Segoe UI", 10f, FontStyle.Bold);
        using var fPeq = new Font("Segoe UI", 8.5f);
        g.DrawString("Faturamento x Lucro — últimos 6 meses", fTitulo, Brushes.Black, 16, 12);

        using var bFat = new SolidBrush(UI.Accent);
        using var bPos = new SolidBrush(UI.Green);
        using var bNeg = new SolidBrush(UI.Red);

        g.FillRectangle(bFat, 16, 44, 10, 10);
        g.DrawString("Faturamento", fPeq, Brushes.DimGray, 30, 40);
        g.FillRectangle(bPos, 120, 44, 10, 10);
        g.DrawString("Lucro", fPeq, Brushes.DimGray, 134, 40);

        var plot = new Rectangle(r.Left + 16, r.Top + 76, r.Width - 32, r.Height - 116);
        if (dados.Count == 0 || plot.Width < 60 || plot.Height < 60) return;

        decimal max = Math.Max(1m, dados.Max(d => Math.Max(d.Fat, d.Lucro)));
        decimal min = Math.Min(0m, dados.Min(d => d.Lucro));
        float range = (float)(max - min);
        float Y(decimal v) => plot.Bottom - (float)(v - min) / range * plot.Height;
        float zero = Y(0);

        float groupW = plot.Width / (float)dados.Count;
        float barW = groupW / 3f;

        void Barra(float x, decimal v, Brush b)
        {
            float y = Y(v);
            g.FillRectangle(b, x, Math.Min(y, zero), barW - 3, Math.Max(1f, Math.Abs(zero - y)));
        }

        using var eixo = new Pen(Color.Silver);
        g.DrawLine(eixo, plot.Left, zero, plot.Right, zero);

        using var centro = new StringFormat { Alignment = StringAlignment.Center };
        for (int i = 0; i < dados.Count; i++)
        {
            var d = dados[i];
            float x = plot.Left + i * groupW + barW / 2f;
            Barra(x, d.Fat, bFat);
            Barra(x + barW, d.Lucro, d.Lucro >= 0 ? bPos : bNeg);
            g.DrawString(d.Mes, fPeq, Brushes.DimGray, x + barW, plot.Bottom + 8, centro);
        }
    }
}
