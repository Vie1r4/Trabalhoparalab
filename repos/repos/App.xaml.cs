// FinalLab/App.xaml.cs
using System.Windows;
using System.Diagnostics;

namespace FinalLab
{
    public partial class App : Application
    {
        private void Application_Startup(object sender, StartupEventArgs e)
        {
            Debug.WriteLine("App: Evento Application_Startup acionado.");
            // Lógica de arranque, se houver, mas sem carregar dados de ficheiro.
        }

        private void Application_Exit(object sender, ExitEventArgs e)
        {
            Debug.WriteLine("App: Evento Application_Exit acionado.");
            // Sem guardar dados em ficheiro.
            // if (Application.Current.MainWindow is MainWindow mainWindow)
            // {
            //    // mainWindow.GuardarDadosApp(NomeFicheiroDados); // REMOVIDO
            // }
        }
    }
}
