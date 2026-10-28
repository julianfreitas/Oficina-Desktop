using Microsoft.Data.Sqlite;

namespace Oficina;

public class ServicosView : ViewBase
{
    readonly TextBox txtNome = new();
    readonly NumericUpDown numValor = UI.Money();
    readonly NumericUpDown numCusto = UI.Money();
    readonly DataGridView grid = UI.Grid();
    int? idSel;

    public ServicosView()
    {
        var top = TopPanel("Serviços e valores", 190);
        UI.Campo(top, "Nome do serviço", txtNome, 24, 66, 300);
        UI.Campo(top, "Valor cobrado (R$)", numValor, 340, 66, 150);
        UI.Campo(top, "Custo estimado (R$)", numCusto, 510, 66, 150);
        top.Controls.Add(UI.Btn("Salvar", UI.Green, 24, 130, Salvar));
        top.Controls.Add(UI.Btn("Novo", UI.Accent, 164, 130, Limpar));
        top.Controls.Add(UI.Btn("Excluir", UI.Red, 304, 130, Excluir));
        top.Controls.Add(UI.Lbl("Clique em uma linha da tabela para editar.", 450, 138));

        grid.CellClick += (_, e) => { if (e.RowIndex >= 0) Selecionar(grid.Rows[e.RowIndex]); };
        Compose(top, grid);
        Carregar();
    }

    void Carregar()
    {
        grid.DataSource = Db.Query(
            "SELECT Id, Nome, Valor, Custo, (Valor - Custo) AS Margem FROM Servicos ORDER BY Nome");
        grid.Columns["Id"].Visible = false;
        grid.Columns["Valor"].HeaderText = "Valor cobrado";
        grid.Columns["Custo"].HeaderText = "Custo estimado";
        grid.Columns["Margem"].HeaderText = "Margem";
        foreach (var c in new[] { "Valor", "Custo", "Margem" })
            grid.Columns[c].DefaultCellStyle.Format = "C2";
        grid.ClearSelection();
    }

    void Selecionar(DataGridViewRow r)
    {
        idSel = Convert.ToInt32(r.Cells["Id"].Value);
        txtNome.Text = Convert.ToString(r.Cells["Nome"].Value);
        numValor.Value = Convert.ToDecimal(r.Cells["Valor"].Value);
        numCusto.Value = Convert.ToDecimal(r.Cells["Custo"].Value);
    }

    void Limpar()
    {
        idSel = null;
        txtNome.Clear();
        numValor.Value = 0;
        numCusto.Value = 0;
        grid.ClearSelection();
        txtNome.Focus();
    }

    void Salvar()
    {
        var nome = txtNome.Text.Trim();
        if (nome.Length == 0) { UI.Aviso("Informe o nome do serviço."); return; }

        if (idSel == null)
            Db.Exec("INSERT INTO Servicos (Nome, Valor, Custo) VALUES (@n, @v, @c)",
                ("@n", nome), ("@v", (double)numValor.Value), ("@c", (double)numCusto.Value));
        else
            Db.Exec("UPDATE Servicos SET Nome = @n, Valor = @v, Custo = @c WHERE Id = @id",
                ("@n", nome), ("@v", (double)numValor.Value), ("@c", (double)numCusto.Value), ("@id", idSel.Value));

        Limpar();
        Carregar();
    }

    void Excluir()
    {
        if (idSel == null) { UI.Aviso("Selecione um serviço na tabela."); return; }
        if (!UI.Confirma("Excluir este serviço?")) return;
        try
        {
            Db.Exec("DELETE FROM Servicos WHERE Id = @id", ("@id", idSel.Value));
            Limpar();
            Carregar();
        }
        catch (SqliteException)
        {
            UI.Aviso("Este serviço já foi usado em serviços realizados e não pode ser excluído.");
        }
    }
}
