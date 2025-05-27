using FinalLab.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using System.Windows.Threading;
using System.IO;
using System.Printing;
using System.Windows.Documents;
using System.Windows.Markup;

namespace FinalLab
{
    public partial class HistogramWindow : Window, INotifyPropertyChanged
    {
        private readonly MainWindow _mainWindowInstance;
        private const int NUM_BINS = 21; // 0 a 20 inclusive
        private const int X_AXIS_PADDING = 50;
        private const int Y_AXIS_PADDING = 40;
        private const int RIGHT_PADDING = 20;
        private const int TOP_PADDING = 20;

        private List<double> _notasFiltradas = new List<double>();
        private int[] _frequencias = new int[NUM_BINS];
        private int _maxFrequencia = 0;

        public event PropertyChangedEventHandler? PropertyChanged;

        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        public HistogramWindow(MainWindow mainWindow)
        {
            InitializeComponent();
            _mainWindowInstance = mainWindow ?? throw new ArgumentNullException(nameof(mainWindow));

            CarregarFiltros();
            AtualizarHistograma();
        }

        private void CarregarFiltros()
        {
            GrupoFilterComboBox.Items.Clear();
            GrupoFilterComboBox.Items.Add(new Grupo("Todos os Grupos"));
            foreach (var grupo in _mainWindowInstance.listaDeGruposPrincipal.OrderBy(g => g.Nome))
            {
                GrupoFilterComboBox.Items.Add(grupo);
            }
            if (GrupoFilterComboBox.Items.Count > 0)
                GrupoFilterComboBox.SelectedIndex = 0;

            TarefaFilterComboBox.Items.Clear();
            TarefaFilterComboBox.Items.Add(new Tarefa(-2, "Todas as Tarefas", null, DateTime.MinValue, DateTime.MinValue, 0));
            TarefaFilterComboBox.Items.Add(new Tarefa(-3, "Média Final", null, DateTime.MinValue, DateTime.MinValue, 0));

            if (_mainWindowInstance.listaDeTarefasPrincipal.Any())
            {
                foreach (var tarefa in _mainWindowInstance.listaDeTarefasPrincipal.OrderBy(t => t.Titulo))
                {
                    TarefaFilterComboBox.Items.Add(tarefa);
                }
                TarefaFilterComboBox.SelectedIndex = 0;
                TarefaFilterComboBox.IsEnabled = true;
            }
            else
            {
                TarefaFilterComboBox.Items.Add(new Tarefa(-1, "-- Sem Tarefas --", null, DateTime.MinValue, DateTime.MinValue, 0));
                if (TarefaFilterComboBox.Items.Count > 1)
                    TarefaFilterComboBox.SelectedIndex = 1;
                else
                    TarefaFilterComboBox.SelectedIndex = 0;
                TarefaFilterComboBox.IsEnabled = false;
            }
        }

        private void AtualizarHistograma()
        {
            var grupoSelecionado = GrupoFilterComboBox.SelectedItem as Grupo;
            var tarefaSelecionada = TarefaFilterComboBox.SelectedItem as Tarefa;

            // Filtrar notas com base no grupo e tarefa selecionados
            _notasFiltradas = ObterNotasFiltradas(grupoSelecionado, tarefaSelecionada);

            // Calcular frequências para o histograma
            CalcularFrequencias();

            // Redesenhar o histograma
            DesenharHistograma();
        }
        private List<double> ObterNotasFiltradas(Grupo? grupoSelecionado, Tarefa? tarefaSelecionada)
        {
            var notas = new List<double>();
            bool mostrarTodasAsTarefas = (tarefaSelecionada != null && tarefaSelecionada.Id == -2);
            bool mostrarMediaFinal = (tarefaSelecionada != null && tarefaSelecionada.Id == -3);

            // Filter students by group first
            var alunosFiltrados = _mainWindowInstance.listaDeAlunosPrincipal
                .Where(aluno => grupoSelecionado == null ||
                       grupoSelecionado.Id == Pauta.TODOS_GRUPOS_ID ||
                       aluno.Grupo == grupoSelecionado.Nome)
                .ToList();

            if (mostrarMediaFinal)
            {
                // Calculate and add final average for each student
                foreach (var aluno in alunosFiltrados)
                {
                    var notasAluno = _mainWindowInstance.listaDeNotasPrincipal
                        .Where(n => n.NumeroAluno == aluno.NumeroAluno && n.Valor.HasValue)
                        .Select(n => n.Valor!.Value)
                        .ToList();

                    if (notasAluno.Any())  // Only add average if student has any grades
                    {
                        double mediaAluno = notasAluno.Average();
                        notas.Add(mediaAluno);
                    }
                }
            }
            else  // Original logic for single task or all tasks
            {
                foreach (var aluno in alunosFiltrados)
                {
                    var notasAlunoQuery = _mainWindowInstance.listaDeNotasPrincipal
                        .Where(n => n.NumeroAluno == aluno.NumeroAluno && n.Valor.HasValue);

                    if (!mostrarTodasAsTarefas && tarefaSelecionada != null && tarefaSelecionada.Id >= 0)
                    {
                        notasAlunoQuery = notasAlunoQuery.Where(n => n.IdTarefa == tarefaSelecionada.Id);
                    }

                    var notasAluno = notasAlunoQuery.Select(n => n.Valor!.Value);
                    notas.AddRange(notasAluno);
                }
            }

            return notas;
        }

        private void CalcularFrequencias()
        {
            // Reiniciar contadores
            Array.Clear(_frequencias, 0, _frequencias.Length);
            _maxFrequencia = 0;

            // Calcular frequências para cada nota
            foreach (var nota in _notasFiltradas)
            {
                // Arredondar a nota para baixo para o inteiro mais próximo e usar como índice
                int index = (int)Math.Floor(nota);
                // Garantir que o índice está dentro dos limites
                if (index >= 0 && index < NUM_BINS)
                {
                    _frequencias[index]++;
                    if (_frequencias[index] > _maxFrequencia)
                    {
                        _maxFrequencia = _frequencias[index];
                    }
                }
            }
        }

        private void DesenharHistograma()
        {
            HistogramCanvas.Children.Clear();

            if (_notasFiltradas.Count == 0)
            {
                // Mostrar mensagem se não houver dados
                TextBlock mensagemVazia = new TextBlock
                {
                    Text = "Não há notas disponíveis para os filtros selecionados.",
                    HorizontalAlignment = HorizontalAlignment.Center,
                    VerticalAlignment = VerticalAlignment.Center,
                    FontSize = 16,
                    Foreground = Brushes.Gray
                };

                Canvas.SetLeft(mensagemVazia, HistogramCanvas.ActualWidth / 2 - 150);
                Canvas.SetTop(mensagemVazia, HistogramCanvas.ActualHeight / 2);
                HistogramCanvas.Children.Add(mensagemVazia);
                return;
            }

            double canvasWidth = HistogramCanvas.ActualWidth;
            double canvasHeight = HistogramCanvas.ActualHeight;
            double graphWidth = canvasWidth - X_AXIS_PADDING - RIGHT_PADDING;
            double graphHeight = canvasHeight - Y_AXIS_PADDING - TOP_PADDING;

            // Definir a cor das barras
            SolidColorBrush barBrush = new SolidColorBrush(Colors.CornflowerBlue);

            // Calcular a largura de cada barra
            double barWidth = graphWidth / NUM_BINS;

            // Desenhar as barras do histograma
            for (int i = 0; i < NUM_BINS; i++)
            {
                if (_frequencias[i] > 0 && _maxFrequencia > 0) // Add check for _maxFrequencia > 0
                {
                    // Calculate proportional height and ensure it's at least 1 pixel if there's a frequency
                    double barHeight = (_frequencias[i] / (double)_maxFrequencia) * graphHeight;
                    barHeight = Math.Max(1, Math.Min(barHeight, graphHeight)); // Ensure height is between 1 and graphHeight

                    // Calculate position ensuring the bar stays within the canvas bounds
                    double x = X_AXIS_PADDING + (i * barWidth);
                    double y = canvasHeight - Y_AXIS_PADDING - barHeight;

                    Rectangle bar = new Rectangle
                    {
                        Width = Math.Max(1, barWidth - 2), // Deixar um pequeno espaço entre as barras
                        Height = barHeight,
                        Fill = barBrush,
                        Stroke = Brushes.Black,
                        StrokeThickness = 0.5
                    };

                    // Ensure the bar position is valid
                    if (y >= 0 && y <= canvasHeight - Y_AXIS_PADDING)
                    {
                        Canvas.SetLeft(bar, x);
                        Canvas.SetTop(bar, y);
                        HistogramCanvas.Children.Add(bar);

                        // Adicionar valor da frequência acima da barra
                        TextBlock freqText = new TextBlock
                        {
                            Text = _frequencias[i].ToString(),
                            FontSize = 10
                        };

                        Canvas.SetLeft(freqText, x + (barWidth / 2) - 5);
                        Canvas.SetTop(freqText, Math.Max(0, y - 15));
                        HistogramCanvas.Children.Add(freqText);
                    }
                }
            }

            // Desenhar eixo X
            Line xAxis = new Line
            {
                X1 = X_AXIS_PADDING,
                Y1 = canvasHeight - Y_AXIS_PADDING,
                X2 = canvasWidth - RIGHT_PADDING,
                Y2 = canvasHeight - Y_AXIS_PADDING,
                Stroke = Brushes.Black,
                StrokeThickness = 1
            };
            HistogramCanvas.Children.Add(xAxis);

            // Desenhar eixo Y
            Line yAxis = new Line
            {
                X1 = X_AXIS_PADDING,
                Y1 = TOP_PADDING,
                X2 = X_AXIS_PADDING,
                Y2 = canvasHeight - Y_AXIS_PADDING,
                Stroke = Brushes.Black,
                StrokeThickness = 1
            };
            HistogramCanvas.Children.Add(yAxis);

            // Adicionar rótulos no eixo X
            for (int i = 0; i <= 20; i += 2) // Mostrar só números pares para não ficar apertado
            {
                TextBlock label = new TextBlock
                {
                    Text = i.ToString(),
                    FontSize = 10
                };

                double x = X_AXIS_PADDING + (i * barWidth);
                Canvas.SetLeft(label, x - 5);
                Canvas.SetTop(label, canvasHeight - Y_AXIS_PADDING + 5);
                HistogramCanvas.Children.Add(label);
            }

            // Adicionar rótulos no eixo Y
            int yStep = CalcularPassoEixoY();
            for (int i = 0; i <= _maxFrequencia; i += yStep)
            {
                TextBlock label = new TextBlock
                {
                    Text = i.ToString(),
                    FontSize = 10
                };

                double y = canvasHeight - Y_AXIS_PADDING - (i / (double)_maxFrequencia * graphHeight);
                Canvas.SetLeft(label, X_AXIS_PADDING - 25);
                Canvas.SetTop(label, y - 7);
                HistogramCanvas.Children.Add(label);

                // Adicionar linha horizontal de grade
                Line gridLine = new Line
                {
                    X1 = X_AXIS_PADDING,
                    Y1 = y,
                    X2 = canvasWidth - RIGHT_PADDING,
                    Y2 = y,
                    Stroke = Brushes.LightGray,
                    StrokeThickness = 0.5,
                    StrokeDashArray = new DoubleCollection { 4, 2 }
                };
                HistogramCanvas.Children.Add(gridLine);
            }

            // Adicionar título dos eixos
            TextBlock xTitle = new TextBlock
            {
                Text = "Notas",
                FontSize = 12,
                FontWeight = FontWeights.SemiBold
            };
            Canvas.SetLeft(xTitle, canvasWidth / 2 - 20);
            Canvas.SetTop(xTitle, canvasHeight - 15);
            HistogramCanvas.Children.Add(xTitle);

            TextBlock yTitle = new TextBlock
            {
                Text = "Alunos",
                FontSize = 12,
                FontWeight = FontWeights.SemiBold
            };
            RotateTransform rotateTransform = new RotateTransform(-90);
            yTitle.RenderTransform = rotateTransform;
            Canvas.SetLeft(yTitle, 15);
            Canvas.SetTop(yTitle, canvasHeight / 2 + 20);
            HistogramCanvas.Children.Add(yTitle);

            // Adicionar informações estatísticas
            AdicionarEstatisticas();
        }

        private int CalcularPassoEixoY()
        {
            // Determinar um passo apropriado para o eixo Y baseado no valor máximo
            if (_maxFrequencia <= 5) return 1;
            if (_maxFrequencia <= 10) return 2;
            if (_maxFrequencia <= 20) return 4;
            if (_maxFrequencia <= 50) return 10;
            return 20;
        }

        private void AdicionarEstatisticas()
        {
            if (_notasFiltradas.Count == 0) return;

            // Calcular estatísticas básicas
            double media = _notasFiltradas.Average();
            double mediana = CalcularMediana(_notasFiltradas);
            double desvioPadrao = CalcularDesvioPadrao(_notasFiltradas, media);

            string grupoNome = (GrupoFilterComboBox.SelectedItem as Grupo)?.Nome ?? "Todos os Grupos";
            string tarefaTitulo = (TarefaFilterComboBox.SelectedItem as Tarefa)?.Titulo ?? "Todas as Tarefas";

            // Adicionar texto com estatísticas
            TextBlock statsText = new TextBlock
            {
                Text = $"Estatísticas ({grupoNome} - {tarefaTitulo}):\n" +
                       $"Total de notas: {_notasFiltradas.Count}\n" +
                       $"Média: {media:F2}\n" +
                       $"Mediana: {mediana:F2}\n" +
                       $"Desvio Padrão: {desvioPadrao:F2}",
                FontSize = 11,
                Background = new SolidColorBrush(Color.FromArgb(220, 255, 255, 255)),
                Padding = new Thickness(5)
            };

            Border statsBorder = new Border
            {
                Child = statsText,
                BorderBrush = Brushes.Gray,
                BorderThickness = new Thickness(1),
                Background = new SolidColorBrush(Color.FromArgb(220, 255, 255, 255))
            };

            Canvas.SetLeft(statsBorder, HistogramCanvas.ActualWidth - 200);
            Canvas.SetTop(statsBorder, 10);
            HistogramCanvas.Children.Add(statsBorder);
        }

        private double CalcularMediana(List<double> valores)
        {
            var ordenados = valores.OrderBy(v => v).ToList();
            int meio = ordenados.Count / 2;

            if (ordenados.Count % 2 == 0)
                return (ordenados[meio - 1] + ordenados[meio]) / 2;
            else
                return ordenados[meio];
        }

        private double CalcularDesvioPadrao(List<double> valores, double media)
        {
            if (valores.Count <= 1) return 0;

            double somaQuadradosDiferencas = valores.Sum(v => Math.Pow(v - media, 2));
            return Math.Sqrt(somaQuadradosDiferencas / (valores.Count - 1)); // Desvio padrão amostral
        }

        private void GrupoFilterComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (IsLoaded) AtualizarHistograma();
        }

        private void TarefaFilterComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (IsLoaded) AtualizarHistograma();
        }

        private void AtualizarButton_Click(object sender, RoutedEventArgs e)
        {
            AtualizarHistograma();
        }

        private void HistogramCanvas_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            DesenharHistograma();
        }

        private void ExportarButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                // Criar um diálogo para salvar arquivo
                Microsoft.Win32.SaveFileDialog saveDialog = new Microsoft.Win32.SaveFileDialog
                {
                    Filter = "Imagem PNG (*.png)|*.png",
                    DefaultExt = ".png",
                    Title = "Salvar Histograma Como Imagem"
                };

                if (saveDialog.ShowDialog() == true)
                {
                    // Renderizar o Canvas em um bitmap
                    RenderTargetBitmap renderBitmap = new RenderTargetBitmap(
                        (int)HistogramCanvas.ActualWidth,
                        (int)HistogramCanvas.ActualHeight,
                        96, 96, PixelFormats.Pbgra32);

                    renderBitmap.Render(HistogramCanvas);

                    // Criar um codificador PNG e salvar a imagem
                    PngBitmapEncoder encoder = new PngBitmapEncoder();
                    encoder.Frames.Add(BitmapFrame.Create(renderBitmap));

                    using (FileStream fs = new FileStream(saveDialog.FileName, FileMode.Create))
                    {
                        encoder.Save(fs);
                    }

                    MessageBox.Show($"Histograma exportado com sucesso para {saveDialog.FileName}",
                        "Exportação Concluída", MessageBoxButton.OK, MessageBoxImage.Information);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Erro ao exportar o histograma: {ex.Message}",
                    "Erro", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void ImprimirButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                PrintDialog printDialog = new PrintDialog();
                if (printDialog.ShowDialog() == true)
                {
                    // Criar um Grid para conter todo o conteúdo a ser impresso
                    Grid printGrid = new Grid();
                    printGrid.Width = printDialog.PrintableAreaWidth;
                    printGrid.Height = printDialog.PrintableAreaHeight;
                    printGrid.Background = Brushes.White;

                    // Adicionar título
                    StackPanel content = new StackPanel();
                    content.Margin = new Thickness(20);

                    TextBlock title = new TextBlock
                    {
                        Text = "Histograma de Distribuição de Notas",
                        FontSize = 18,
                        FontWeight = FontWeights.Bold,
                        TextAlignment = TextAlignment.Center,
                        Margin = new Thickness(0, 0, 0, 20)
                    };
                    content.Children.Add(title);

                    // Adicionar informações de filtro
                    string grupoNome = (GrupoFilterComboBox.SelectedItem as Grupo)?.Nome ?? "Todos os Grupos";
                    string tarefaTitulo = (TarefaFilterComboBox.SelectedItem as Tarefa)?.Titulo ?? "Todas as Tarefas";
                    TextBlock info = new TextBlock
                    {
                        Text = $"Grupo: {grupoNome}\nTarefa: {tarefaTitulo}\nData: {DateTime.Now:dd/MM/yyyy HH:mm}",
                        Margin = new Thickness(0, 0, 0, 20)
                    };
                    content.Children.Add(info);

                    // Criar uma cópia do Canvas para impressão
                    Canvas printCanvas = new Canvas
                    {
                        Width = HistogramCanvas.ActualWidth,
                        Height = HistogramCanvas.ActualHeight
                    };

                    // Copiar o conteúdo do Canvas original
                    UIElement clone = CloneCanvas();
                    printCanvas.Children.Add(clone);

                    // Redimensionar o Canvas para caber na página
                    double scale = Math.Min(
                        (printDialog.PrintableAreaWidth - 40) / printCanvas.Width,
                        (printDialog.PrintableAreaHeight - 200) / printCanvas.Height);

                    ScaleTransform scaleTransform = new ScaleTransform(scale, scale);
                    printCanvas.LayoutTransform = scaleTransform;

                    content.Children.Add(printCanvas);
                    printGrid.Children.Add(content);

                    // Imprimir
                    printDialog.PrintVisual(printGrid, "Histograma de Notas");

                    MessageBox.Show("Documento enviado para impressão.",
                        "Impressão", MessageBoxButton.OK, MessageBoxImage.Information);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Erro ao imprimir: {ex.Message}",
                    "Erro", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private UIElement CloneCanvas()
        {
            // Criar um novo Canvas com o mesmo conteúdo
            Canvas newCanvas = new Canvas
            {
                Width = HistogramCanvas.ActualWidth,
                Height = HistogramCanvas.ActualHeight,
                Background = Brushes.White
            };

            // Renderizar o Canvas original em um bitmap
            RenderTargetBitmap rtb = new RenderTargetBitmap(
                (int)HistogramCanvas.ActualWidth,
                (int)HistogramCanvas.ActualHeight,
                96, 96, PixelFormats.Pbgra32);

            rtb.Render(HistogramCanvas);

            // Criar uma imagem do bitmap
            Image img = new Image
            {
                Source = rtb,
                Width = HistogramCanvas.ActualWidth,
                Height = HistogramCanvas.ActualHeight
            };

            newCanvas.Children.Add(img);
            return newCanvas;
        }

        private void FecharButton_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }
    }
}

