// FinalLab/App.xaml.cs
using System.Windows;
using System.Diagnostics;
using System.Collections.Generic;
using FinalLab.Models;

namespace FinalLab
{
    public partial class App : Application
    {
        public static List<Grupo> Grupos { get; private set; } = new();
        public static List<Aluno> Alunos { get; private set; } = new();
        public static List<Tarefa> Tarefas { get; set; } = new List<Tarefa>();
        public static List<NotaAlunoTarefa> Notas { get; set; } = new List<NotaAlunoTarefa>();

        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            // Carregar dados persistidos (se existirem)
            Grupos = DataStorage.LoadFromFile<List<Grupo>>(AppDataPaths.GruposFile) ?? new List<Grupo>();
            Alunos = DataStorage.LoadFromFile<List<Aluno>>(AppDataPaths.AlunosFile) ?? new List<Aluno>();
            Tarefas = DataStorage.LoadFromFile<List<Tarefa>>(AppDataPaths.TarefasFile) ?? new List<Tarefa>();
            Notas = DataStorage.LoadFromFile<List<NotaAlunoTarefa>>(AppDataPaths.NotasFile) ?? new List<NotaAlunoTarefa>();
        }

        protected override void OnExit(ExitEventArgs e)
        {
            Debug.WriteLine("A guardar dados...");
            Debug.WriteLine($"A guardar {Grupos.Count} grupos, {Alunos.Count} alunos, {Tarefas.Count} tarefas e {Notas.Count} notas.");
            DataStorage.SaveToFile(AppDataPaths.GruposFile, Grupos);
            DataStorage.SaveToFile(AppDataPaths.AlunosFile, Alunos);
            DataStorage.SaveToFile(AppDataPaths.TarefasFile, Tarefas);
            DataStorage.SaveToFile(AppDataPaths.NotasFile, Notas);
            base.OnExit(e);
        }

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
