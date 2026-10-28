namespace Oficina;

/// <summary>Gastos gerais da oficina (aluguel, energia, ferramentas...), descontados do lucro.</summary>
public class DespesasView : ViewBase
{
    readonly TextBox txtDescricao = new();
    readonly NumericUpDown numValor = UI.Money();
    readonly DateTimePicker dtpData = new() { Format = DateTimePickerFormat.Short };
    readonly DataGridView grid = UI.Grid();

    public DespesasView()
    {
        var top = TopPanel("Despesas gerais", 190);
        UI.Campo(top, "Descrição", txtDescricao, 24, 66, 330);
        UI.Campo(top, "Valor (R$)", numValor, 370, 66, 150);
        UI.Campo(top, "Data", dtpData, 536, 66, 140);
        top.Controls.Add(UI.Btn("Adicionar", UI.Green, 24, 130, Adicionar));
        top.Controls.Add(UI.Btn("Excluir selecionada", UI.Red, 164, 130, Excluir, 160));

        Compose(top, grid);
        Carregar();
    }

    void Carregar()
    {
        grid.DataSource = Db.Query(@"
            SELECT Id, strftime('%d/%m/%Y', Data) AS Data, Descricao, Valor
            FROM Despesas ORDER BY Despesas.Data DESC, Id DESC LIMIT 500");
        grid.Columns["Id"].Visible = false;
        grid.Columns["Descricao"].HeaderText = "Descrição";
        grid.Columns["Valor"].DefaultCellStyle.Format = "C2";
        grid.ClearSelection();
    }

    void Adicionar()
    {
        var desc = txtDescricao.Text.Trim();
        if (desc.Length == 0) { UI.Aviso("Informe a descrição da despesa."); return; }
        if (numValor.Value <= 0) { UI.Aviso("Informe um valor maior que zero."); return; }

        Db.Exec("INSERT INTO Despesas (Descricao, Valor, Data) VALUES (@d, @v, @dt)",
            ("@d", desc), ("@v", (double)numValor.Value), ("@dt", UI.Iso(dtpData.Value)));

        txtDescricao.Clear();
        numValor.Value = 0;
        Carregar();
    }

    void Excluir()
    {
        if (grid.SelectedRows.Count == 0) { UI.Aviso("Selecione uma despesa na tabela."); return; }
        var r = grid.SelectedRows[0];
        if (!UI.Confirma($"Excluir a despesa '{r.Cells["Descricao"].Value}'?")) return;

        Db.Exec("DELETE FROM Despesas WHERE Id = @id", ("@id", Convert.ToInt32(r.Cells["Id"].Value)));
        Carregar();
    }
}
