using Microsoft.Data.Sqlite;

namespace Oficina;

public class ClientesView : ViewBase
{
    static readonly string[] Marcas =
    {
        "Chevrolet", "Citroën", "Fiat", "Ford", "Honda", "Hyundai", "Jeep", "Kia", "Mitsubishi",
        "Nissan", "Peugeot", "Renault", "Toyota", "Volkswagen", "Outra"
    };

    readonly TextBox txtNome = new(), txtModelo = new(), txtEmail = new();
    readonly TextBox txtBusca = new() { PlaceholderText = "Buscar por nome, CPF ou carro..." };
    readonly MaskedTextBox txtCpf = new() { Mask = @"000\.000\.000\-00" };
    readonly ComboBox cmbMarca = new() { DropDownStyle = ComboBoxStyle.DropDown };
    readonly DataGridView grid = UI.Grid();
    int? idSel;

    public ClientesView()
    {
        cmbMarca.Items.AddRange(Marcas);

        var top = TopPanel("Clientes", 255);
        UI.Campo(top, "Nome completo", txtNome, 24, 66, 300);
        UI.Campo(top, "CPF", txtCpf, 340, 66, 150);
        UI.Campo(top, "E-mail", txtEmail, 510, 66, 260);
        UI.Campo(top, "Marca do carro", cmbMarca, 24, 130, 200);
        UI.Campo(top, "Modelo do carro", txtModelo, 240, 130, 200);

        top.Controls.Add(UI.Btn("Salvar", UI.Green, 24, 196, Salvar));
        top.Controls.Add(UI.Btn("Novo", UI.Accent, 164, 196, Limpar));
        top.Controls.Add(UI.Btn("Excluir", UI.Red, 304, 196, Excluir));
        txtBusca.Location = new Point(460, 200);
        txtBusca.Width = 310;
        txtBusca.TextChanged += (_, _) => Carregar();
        top.Controls.Add(txtBusca);

        grid.CellClick += (_, e) => { if (e.RowIndex >= 0) Selecionar(grid.Rows[e.RowIndex]); };
        grid.CellFormatting += (_, e) =>
        {
            if (e.ColumnIndex >= 0 && grid.Columns[e.ColumnIndex].Name == "Cpf" && e.Value is string v)
            {
                e.Value = UI.FormatCpf(v);
                e.FormattingApplied = true;
            }
        };

        Compose(top, grid);
        Carregar();
    }

    void Carregar()
    {
        var busca = txtBusca.Text.Trim();
        var digitos = UI.SoDigitos(busca);
        grid.DataSource = Db.Query(@"
            SELECT Id, Nome, Cpf, Marca, Modelo, Email FROM Clientes
            WHERE Nome LIKE @b OR Marca LIKE @b OR Modelo LIKE @b OR (@d <> '' AND Cpf LIKE @cd)
            ORDER BY Nome",
            ("@b", "%" + busca + "%"), ("@d", digitos), ("@cd", "%" + digitos + "%"));
        grid.Columns["Id"].Visible = false;
        grid.Columns["Cpf"].HeaderText = "CPF";
        grid.Columns["Email"].HeaderText = "E-mail";
        grid.ClearSelection();
    }

    void Selecionar(DataGridViewRow r)
    {
        idSel = Convert.ToInt32(r.Cells["Id"].Value);
        txtNome.Text = Convert.ToString(r.Cells["Nome"].Value);
        txtCpf.Text = UI.FormatCpf(Convert.ToString(r.Cells["Cpf"].Value));
        cmbMarca.Text = Convert.ToString(r.Cells["Marca"].Value);
        txtModelo.Text = Convert.ToString(r.Cells["Modelo"].Value);
        txtEmail.Text = Convert.ToString(r.Cells["Email"].Value);
    }

    void Limpar()
    {
        idSel = null;
        txtNome.Clear();
        txtCpf.Clear();
        cmbMarca.Text = "";
        txtModelo.Clear();
        txtEmail.Clear();
        grid.ClearSelection();
        txtNome.Focus();
    }

    void Salvar()
    {
        var nome = txtNome.Text.Trim();
        var cpf = UI.SoDigitos(txtCpf.Text);
        var marca = cmbMarca.Text.Trim();
        var modelo = txtModelo.Text.Trim();
        var email = txtEmail.Text.Trim();

        if (nome.Length == 0) { UI.Aviso("Informe o nome do cliente."); return; }
        if (!UI.CpfValido(cpf)) { UI.Aviso("CPF inválido."); return; }
        if (marca.Length == 0 || modelo.Length == 0) { UI.Aviso("Informe a marca e o modelo do carro."); return; }
        if (email.Length > 0 && !UI.EmailValido(email)) { UI.Aviso("E-mail inválido."); return; }

        try
        {
            if (idSel == null)
                Db.Exec(@"INSERT INTO Clientes (Nome, Cpf, Marca, Modelo, Email)
                          VALUES (@n, @c, @ma, @mo, @e)",
                    ("@n", nome), ("@c", cpf), ("@ma", marca), ("@mo", modelo), ("@e", email));
            else
                Db.Exec(@"UPDATE Clientes SET Nome = @n, Cpf = @c, Marca = @ma, Modelo = @mo, Email = @e
                          WHERE Id = @id",
                    ("@n", nome), ("@c", cpf), ("@ma", marca), ("@mo", modelo), ("@e", email), ("@id", idSel.Value));
        }
        catch (SqliteException)
        {
            UI.Aviso("Já existe um cliente cadastrado com este CPF.");
            return;
        }

        Limpar();
        Carregar();
    }

    void Excluir()
    {
        if (idSel == null) { UI.Aviso("Selecione um cliente na tabela."); return; }
        if (!UI.Confirma("Excluir este cliente?")) return;
        try
        {
            Db.Exec("DELETE FROM Clientes WHERE Id = @id", ("@id", idSel.Value));
            Limpar();
            Carregar();
        }
        catch (SqliteException)
        {
            UI.Aviso("Este cliente possui serviços registrados e não pode ser excluído.");
        }
    }
}
