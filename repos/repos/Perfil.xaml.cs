// Ficheiro: Perfil.xaml.cs
using System;
using System.IO; // Necessário para File.Exists
using System.Windows.Controls;
using System.Windows.Media.Imaging;
using System.Diagnostics;
using System.Windows;
using System.Windows.Media; // Necessário para Brushes, etc., embora não usado diretamente aqui

namespace FinalLab
{
    public partial class Perfil : UserControl
    {
        private MainWindow? _mainWindowInstance;

        // Construtor que recebe a instância da MainWindow
        public Perfil(MainWindow mainWindow) : this() // Chama o construtor padrão
        {
            _mainWindowInstance = mainWindow;
            Debug.WriteLine("[Perfil.xaml.cs] Construtor com MainWindow chamado.");
        }

        // Construtor padrão (necessário para o XAML e chamado pelo outro construtor)
        public Perfil()
        {
            InitializeComponent();
            Debug.WriteLine("[Perfil.xaml.cs] InitializeComponent Concluído.");
            // Adia o carregamento de dados para o evento Loaded para garantir que a UI está pronta
            this.Loaded += Perfil_Loaded;
        }

        private void Perfil_Loaded(object sender, RoutedEventArgs e)
        {
            Debug.WriteLine("[Perfil.xaml.cs] Perfil_Loaded iniciado.");
            try
            {
                // Tenta obter a MainWindow se não foi passada via construtor principal
                _mainWindowInstance ??= Window.GetWindow(this) as MainWindow;

                if (_mainWindowInstance == null)
                {
                    Debug.WriteLine("[Perfil.xaml.cs] ERRO CRÍTICO: _mainWindowInstance é null em Perfil_Loaded.");
                    MessageBox.Show("Erro fatal ao carregar o perfil: Referência à janela principal perdida.", "Erro no Perfil", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }
                LoadUserProfileData();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Erro ao carregar o perfil: {ex.Message}", "Erro no Perfil", MessageBoxButton.OK, MessageBoxImage.Error);
                Debug.WriteLine($"[Perfil.xaml.cs] ERRO Perfil_Loaded: {ex}");
            }
        }

        private void LoadUserProfileData()
        {
            Debug.WriteLine("[Perfil.xaml.cs] LoadUserProfileData chamado.");
            if (NomeUtilizadorTextBlock == null || EmailUtilizadorTextBlock == null || ProfileImageBrush == null)
            {
                Debug.WriteLine("[Perfil.xaml.cs] ERRO em LoadUserProfileData: Um ou mais controlos XAML do perfil são nulos. Verifique os x:Name em Perfil.xaml.");
                // Poderia mostrar uma mensagem ao utilizador aqui, mas o Debug.WriteLine é para desenvolvimento.
                return;
            }

            NomeUtilizadorTextBlock.Text = MainWindow.NomePerfilEditavel; // Usa a propriedade de nome editável
            EmailUtilizadorTextBlock.Text = MainWindow.EmailUtilizadorLogado;
            Debug.WriteLine($"[Perfil.xaml.cs] Nome: {NomeUtilizadorTextBlock.Text}, Email: {EmailUtilizadorTextBlock.Text}");

            // Atualiza a imagem usando o mesmo método que a MainWindow usa
            UpdateProfileImageOnPageFromGlobalPath();
        }

        // MÉTODO NECESSÁRIO PARA CORRIGIR O ERRO CS1061
        public void UpdateProfileNameOnPage(string newName)
        {
            if (NomeUtilizadorTextBlock != null)
            {
                NomeUtilizadorTextBlock.Text = newName;
                Debug.WriteLine($"[Perfil.xaml.cs] Nome na página de perfil atualizado para: {newName}");
            }
            else
            {
                Debug.WriteLine("[Perfil.xaml.cs] AVISO: NomeUtilizadorTextBlock é null em UpdateProfileNameOnPage.");
            }
        }

        // Método público para ser chamado pela MainWindow para atualizar a imagem
        public void UpdateProfileImageOnPage(BitmapImage? bitmap)
        {
            if (ProfileImageBrush != null)
            {
                ProfileImageBrush.ImageSource = bitmap;
                Debug.WriteLine(bitmap != null ? "[Perfil.xaml.cs] UpdateProfileImageOnPage: Imagem da página de perfil atualizada." : "[Perfil.xaml.cs] UpdateProfileImageOnPage: Imagem da página de perfil definida como null.");
            }
            else
            {
                Debug.WriteLine("[Perfil.xaml.cs] AVISO: ProfileImageBrush na página Perfil é null em UpdateProfileImageOnPage.");
            }
        }

        // Método para carregar a imagem com base no caminho estático global
        private void UpdateProfileImageOnPageFromGlobalPath()
        {
            Debug.WriteLine($"[Perfil.xaml.cs] UpdateProfileImageOnPageFromGlobalPath. Caminho global: {MainWindow.CaminhoFotoUtilizadorLogado}");
            if (ProfileImageBrush == null)
            {
                Debug.WriteLine("[Perfil.xaml.cs] AVISO: ProfileImageBrush (FromGlobalPath) é null.");
                return;
            }

            BitmapImage? newImage = null;
            if (!string.IsNullOrEmpty(MainWindow.CaminhoFotoUtilizadorLogado) && File.Exists(MainWindow.CaminhoFotoUtilizadorLogado))
            {
                try
                {
                    newImage = new BitmapImage();
                    newImage.BeginInit();
                    newImage.UriSource = new Uri(MainWindow.CaminhoFotoUtilizadorLogado, UriKind.Absolute);
                    newImage.CacheOption = BitmapCacheOption.OnLoad;
                    newImage.CreateOptions = BitmapCreateOptions.IgnoreImageCache;
                    newImage.EndInit();
                    newImage.Freeze(); // Opcional, para performance e uso entre threads
                    Debug.WriteLine("[Perfil.xaml.cs] Imagem (FromGlobalPath) carregada com sucesso.");
                }
                catch (Exception ex)
                {
                    Debug.WriteLine($"[Perfil.xaml.cs] ERRO ao carregar imagem de perfil (FromGlobalPath): {ex.Message}");
                    newImage = null; // Garante que não usa uma imagem inválida
                }
            }
            else
            {
                Debug.WriteLine("[Perfil.xaml.cs] Caminho da foto nulo ou ficheiro não existe (FromGlobalPath). Nenhuma imagem carregada.");
            }
            ProfileImageBrush.ImageSource = newImage;
        }


        private void EditProfileButton_Click(object sender, RoutedEventArgs e)
        {
            Debug.WriteLine("[Perfil.xaml.cs] EditProfileButton_Click chamado.");
            try
            {
                MainWindow? mainWindow = _mainWindowInstance ?? Window.GetWindow(this) as MainWindow;
                if (mainWindow != null)
                {
                    Debug.WriteLine("[Perfil.xaml.cs] Navegando para Editar Perfil.");
                    mainWindow.NavigateToPage("Editar Perfil", new EditarPerfil(mainWindow)); // Passa a instância da MainWindow
                }
                else
                {
                    MessageBox.Show("Não foi possível aceder à janela principal para efectuar a navegação.", "Erro de Navegação", MessageBoxButton.OK, MessageBoxImage.Error);
                    Debug.WriteLine("[Perfil.xaml.cs] ERRO EditProfileButton_Click: mainWindow é null.");
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Erro ao tentar abrir a edição de perfil: {ex.Message}", "Erro", MessageBoxButton.OK, MessageBoxImage.Error);
                Debug.WriteLine($"[Perfil.xaml.cs] ERRO EditProfileButton_Click: {ex}");
            }
        }
    }
}
