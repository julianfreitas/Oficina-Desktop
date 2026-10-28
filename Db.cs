using System.Data;
using Microsoft.Data.Sqlite;

namespace Oficina;

/// <summary>Acesso ao banco SQLite local (arquivo único, 100% offline).</summary>
public static class Db
{
    static readonly string Pasta = Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "OficinaApp");
    static readonly string Arquivo = Path.Combine(Pasta, "oficina.db");

    static SqliteConnection Abrir()
    {
        var c = new SqliteConnection($"Data Source={Arquivo}");
        c.Open();
        using var cmd = c.CreateCommand();
        cmd.CommandText = "PRAGMA foreign_keys = ON;";
        cmd.ExecuteNonQuery();
        return c;
    }

    static SqliteCommand Comando(SqliteConnection c, string sql, (string, object)[] ps)
    {
        var cmd = c.CreateCommand();
        cmd.CommandText = sql;
        foreach (var (nome, valor) in ps)
            cmd.Parameters.AddWithValue(nome, valor ?? DBNull.Value);
        return cmd;
    }

    public static int Exec(string sql, params (string, object)[] ps)
    {
        using var c = Abrir();
        using var cmd = Comando(c, sql, ps);
        return cmd.ExecuteNonQuery();
    }

    public static DataTable Query(string sql, params (string, object)[] ps)
    {
        using var c = Abrir();
        using var cmd = Comando(c, sql, ps);
        using var r = cmd.ExecuteReader();
        var dt = new DataTable();
        dt.Load(r);
        return dt;
    }

    public static object Scalar(string sql, params (string, object)[] ps)
    {
        using var c = Abrir();
        using var cmd = Comando(c, sql, ps);
        return cmd.ExecuteScalar();
    }

    public static void Init()
    {
        Directory.CreateDirectory(Pasta);

        Exec(@"
CREATE TABLE IF NOT EXISTS Servicos (
    Id    INTEGER PRIMARY KEY AUTOINCREMENT,
    Nome  TEXT NOT NULL,
    Valor REAL NOT NULL DEFAULT 0,
    Custo REAL NOT NULL DEFAULT 0
);
CREATE TABLE IF NOT EXISTS Clientes (
    Id     INTEGER PRIMARY KEY AUTOINCREMENT,
    Nome   TEXT NOT NULL,
    Cpf    TEXT NOT NULL UNIQUE,
    Marca  TEXT NOT NULL,
    Modelo TEXT NOT NULL,
    Email  TEXT
);
CREATE TABLE IF NOT EXISTS OrdensServico (
    Id        INTEGER PRIMARY KEY AUTOINCREMENT,
    ClienteId INTEGER NOT NULL REFERENCES Clientes(Id),
    ServicoId INTEGER NOT NULL REFERENCES Servicos(Id),
    Data      TEXT NOT NULL,
    Valor     REAL NOT NULL,
    Custo     REAL NOT NULL DEFAULT 0,
    Obs       TEXT
);
CREATE TABLE IF NOT EXISTS Despesas (
    Id        INTEGER PRIMARY KEY AUTOINCREMENT,
    Descricao TEXT NOT NULL,
    Valor     REAL NOT NULL,
    Data      TEXT NOT NULL
);");

        // Serviços iniciais de exemplo (só na primeira execução)
        if (Convert.ToInt64(Scalar("SELECT COUNT(*) FROM Servicos")) == 0)
        {
            Exec(@"INSERT INTO Servicos (Nome, Valor, Custo) VALUES
                ('Troca de óleo', 150, 90),
                ('Alinhamento e balanceamento', 120, 20),
                ('Revisão completa', 450, 200),
                ('Troca de pastilhas de freio', 280, 140),
                ('Diagnóstico eletrônico', 100, 0);");
        }
    }

    /// <summary>Totais de um mês (formato yyyy-MM): faturamento, custos dos serviços e despesas gerais.</summary>
    public static (decimal Faturamento, decimal Custos, decimal Despesas) Totais(string ym)
    {
        var os = Query(@"SELECT COALESCE(SUM(Valor),0) AS F, COALESCE(SUM(Custo),0) AS C
                         FROM OrdensServico WHERE strftime('%Y-%m', Data) = @m", ("@m", ym));
        var desp = Scalar(@"SELECT COALESCE(SUM(Valor),0) FROM Despesas
                            WHERE strftime('%Y-%m', Data) = @m", ("@m", ym));
        return (Math.Round(Convert.ToDecimal(os.Rows[0]["F"]), 2),
                Math.Round(Convert.ToDecimal(os.Rows[0]["C"]), 2),
                Math.Round(Convert.ToDecimal(desp), 2));
    }
}
