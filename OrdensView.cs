using System.Data;

namespace Oficina;

/// <summary>Registro dos serviços realizados (base do faturamento do dashboard).</summary>
public class OrdensView : ViewBase
{
    readonly ComboBox cmbCliente = new() { DropDownStyle = ComboBoxStyle.DropDownList };
    readonly ComboBox cmbServico = new() { DropDownStyle = ComboBoxStyle.DropDownList };
    readonly DateTimePicker dtpData = new() { Format = DateTimePickerFormat.Short };
    readonly NumericUpDown numValor = UI.Money();
    readonly NumericUpDown numCusto = UI.Money();
    readonly TextBox txtObs = new();
    readonly DataGridView grid = UI.Grid();

    public OrdensView()
    {
        var top = TopPanel("Serviços realizados", 250);
        UI.Campo(top, "Cliente", cmbCliente, 24, 66, 330);
        UI.Campo(top, "Serviço", cmbServico, 370, 66, 260);
        UI.Campo(top, "Data", dtpData, 646, 66, 130);
        UI.Campo(top, "Valor cobrado (R$)", numValor, 24, 130, 150);
        UI.Campo(top, "Custo / peças (R$)", numCusto, 190, 130, 170);
        UI.Campo(top, "Observação", txtObs, 376, 130, 400);

        top.Controls.Add(UI.Btn("Registrar serviço", UI.Green, 24, 196, Registrar, 160));
        top.Controls.Add(UI.Btn("Excluir selecionado", UI.Red, 200, 196, Excluir, 170));

        CarregarCombos();
        cmbServico.SelectedIndexChanged += (_, _) => PreencherValores();
        PreencherValores();

        Compose(top, grid);
        Carregar();
    }

    void CarregarCombos()
    {
        cmbCliente.DisplayMember = "Descricao";
        cmbCliente.ValueMember = "Id";
        cmbCliente.DataSource = Db.Query(
            "SELECT Id, Nome || ' - ' || Marca || ' ' || Modelo AS Descricao FROM Clientes ORDER BY Nome");

        cmbServico.DisplayMember = "Nome";
        cmbServico.ValueMember = "Id";
        cmbServico.DataSource = Db.Query("SELECT Id, Nome, Valor, Custo FROM Servicos ORDER BY Nome");
    }

    // Ao escolher o serviço, sugere o valor e o custo cadastrados (podem ser editados, ex.: desconto)
    void PreencherValores()
    {
        if (cmbServico.SelectedItem is DataRowView r)
        {
            numValor.Value = Convert.ToDecimal(r["Valor"]);
            numCusto.Value = Convert.ToDecimal(r["Custo"]);
        }
    }

    void Carregar()
    {
        grid.DataSource = Db.Query(@"
            SELECT o.Id, strftime('%d/%m/%Y', o.Data) AS Data, c.Nome AS Cliente,
                   c.Marca || ' ' || c.Modelo AS Veiculo, s.Nome AS Servico,
                   o.Valor, o.Custo, (o.Valor - o.Custo) AS Lucro, o.Obs
            FROM OrdensServico o
            JOIN Clientes c ON c.Id = o.ClienteId
            JOIN Servicos s ON s.Id = o.ServicoId
            ORDER BY o.Data DESC, o.Id DESC
            LIMIT 500");
        grid.Columns["Id"].Visible = false;
        grid.Columns["Veiculo"].HeaderText = "Veículo";
        grid.Columns["Servico"].HeaderText = "Serviço";
        grid.Columns["Obs"].HeaderText = "Observação";
        foreach (var c in new[] { "Valor", "Custo", "Lucro" })
            grid.Columns[c].DefaultCellStyle.Format = "C2";
        grid.ClearSelection();
    }

    void Registrar()
    {
        if (cmbCliente.SelectedValue == null || cmbServico.SelectedValue == null)
        {
            UI.Aviso("Cadastre ao menos um cliente e um serviço antes de registrar.");
            return;
        }

        Db.Exec(@"INSERT INTO OrdensServico (ClienteId, ServicoId, Data, Valor, Custo, Obs)
                  VALUES (@c, @s, @d, @v, @k, @o)",
            ("@c", cmbCliente.SelectedValue), ("@s", cmbServico.SelectedValue),
            ("@d", UI.Iso(dtpData.Value)),
            ("@v", (double)numValor.Value), ("@k", (double)numCusto.Value),
            ("@o", txtObs.Text.Trim()));

        txtObs.Clear();
        Carregar();
    }

    void Excluir()
    {
        if (grid.SelectedRows.Count == 0) { UI.Aviso("Selecione um registro na tabela."); return; }
        var r = grid.SelectedRows[0];
        if (!UI.Confirma($"Excluir o serviço '{r.Cells["Servico"].Value}' de {r.Cells["Cliente"].Value}?")) return;

        Db.Exec("DELETE FROM OrdensServico WHERE Id = @id", ("@id", Convert.ToInt32(r.Cells["Id"].Value)));
        Carregar();
    }
}
