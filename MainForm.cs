namespace Oficina;

public class MainForm : Form
{
    readonly Panel sidebar = new() { Dock = DockStyle.Left, Width = 230, BackColor = UI.SidebarColor };
    readonly Panel content = new() { Dock = DockStyle.Fill, BackColor = UI.Bg };
    readonly List<Button> botoes = new();

    public MainForm()
    {
        Text = "Oficina Mecânica — Gestão";
        MinimumSize = new Size(1100, 680);
        Size = new Size(1280, 760);
        StartPosition = FormStartPosition.CenterScreen;
        Font = UI.BaseFont;

        var itens = new (string Texto, Func<Control> Criar)[]
        {
            ("🏠   Home",                () => new HomeView()),
            ("🛠   Serviços e valores",  () => new ServicosView()),
            ("👤   Clientes",            () => new ClientesView()),
            ("📋   Serviços realizados", () => new OrdensView()),
            ("💸   Despesas",            () => new DespesasView()),
            ("📊   Dashboard",           () => new DashboardView()),
        };

        foreach (var (texto, criar) in itens)
        {
            var b = new Button
            {
                Text = texto, Dock = DockStyle.Top, Height = 52,
                FlatStyle = FlatStyle.Flat, BackColor = UI.SidebarColor, ForeColor = Color.White,
                TextAlign = ContentAlignment.MiddleLeft, Padding = new Padding(18, 0, 0, 0),
                Font = new Font("Segoe UI", 11f), Cursor = Cursors.Hand, Tag = criar
            };
            b.FlatAppearance.BorderSize = 0;
            b.FlatAppearance.MouseOverBackColor = UI.SidebarHover;
            b.Click += (_, _) => Navegar(b);
            botoes.Add(b);
        }

        // Dock.Top empilha o último adicionado no topo: adiciona ao contrário
        for (int i = botoes.Count - 1; i >= 0; i--) sidebar.Controls.Add(botoes[i]);

        sidebar.Controls.Add(new Label
        {
            Text = "🔧  Oficina", Dock = DockStyle.Top, Height = 84, ForeColor = Color.White,
            Font = new Font("Segoe UI", 18f, FontStyle.Bold), TextAlign = ContentAlignment.MiddleCenter
        });

        Controls.Add(content);
        Controls.Add(sidebar);

        Navegar(botoes[0]);
    }

    void Navegar(Button ativo)
    {
        foreach (var b in botoes) b.BackColor = UI.SidebarColor;
        ativo.BackColor = UI.Accent;

        foreach (var c in content.Controls.Cast<Control>().ToList())
        {
            content.Controls.Remove(c);
            c.Dispose();
        }
        content.Controls.Add(((Func<Control>)ativo.Tag)());   // tela nova = dados sempre atualizados
    }
}
