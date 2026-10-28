using System.Globalization;

namespace Oficina;

static class Program
{
    [STAThread]
    static void Main()
    {
        var pt = new CultureInfo("pt-BR");
        CultureInfo.DefaultThreadCurrentCulture = pt;
        CultureInfo.DefaultThreadCurrentUICulture = pt;
        CultureInfo.CurrentCulture = pt;
        CultureInfo.CurrentUICulture = pt;

        ApplicationConfiguration.Initialize();
        Db.Init();
        Application.Run(new MainForm());
    }
}
