namespace Oficina;

public class HomeView : ViewBase
{
    public HomeView()
    {
        var titulo = UI.Title("Bem-vindo à Oficina");
        titulo.Location = new Point(24, 16);
        Controls.Add(titulo);
        Controls.Add(UI.Lbl(DateTime.Now.ToString("D"), 27, 62));

        var ym = DateTime.Now.ToString("yyyy-MM", System.Globalization.CultureInfo.InvariantCulture);
        var (fat, custos, desp) = Db.Totais(ym);
        var qtdMes = Convert.ToInt32(Db.Scalar(
            "SELECT COUNT(*) FROM OrdensServico WHERE strftime('%Y-%m', Data) = @m", ("@m", ym)));
        var clientes = Convert.ToInt32(Db.Scalar("SELECT COUNT(*) FROM Clientes"));
        var lucro = fat - custos - desp;

        var cards = new[]
        {
            UI.Card("Clientes cadastrados", UI.Accent, out var l1),
            UI.Card("Serviços no mês", UI.Orange, out var l2),
            UI.Card("Faturamento do mês", UI.Accent, out var l3),
            UI.Card("Lucro do mês", lucro >= 0 ? UI.Green : UI.Red, out var l4),
        };
        l1.Text = clientes.ToString();
        l2.Text = qtdMes.ToString();
        l3.Text = UI.Moeda(fat);
        l4.Text = UI.Moeda(lucro);
        l4.ForeColor = lucro >= 0 ? UI.Green : UI.Red;

        for (int i = 0; i < cards.Length; i++)
        {
            cards[i].Location = new Point(24 + i * 212, 110);
            Controls.Add(cards[i]);
        }

        var ajuda = new Label
        {
            AutoSize = true, ForeColor = UI.Muted, Location = new Point(24, 230),
            Text = "Como usar:\n\n" +
                   "•  Serviços e valores — cadastre os serviços, o valor cobrado e o custo estimado.\n" +
                   "•  Clientes — cadastre nome, CPF, marca/modelo do carro e e-mail.\n" +
                   "•  Serviços realizados — registre cada serviço feito para um cliente.\n" +
                   "•  Despesas — lance gastos gerais (aluguel, energia, ferramentas...).\n" +
                   "•  Dashboard — veja faturamento, gastos e lucro final de cada mês."
        };
        Controls.Add(ajuda);
    }
}
