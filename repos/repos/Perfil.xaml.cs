using System;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Imaging;
// Adicione aqui outros 'usings' que a sua página EditarPerfil possa necessitar,
// por exemplo, para Microsoft.Win32.OpenFileDialog se a seleção de ficheiro estiver lá.

namespace FinalLab
{
    public partial class Perfil : UserControl
    {
        // ALTERAÇÃO AQUI: Tornar o campo _mainWindowInstance anulável
        private MainWindow? _mainWindowInstance; // Adicionado '?' para indicar que pode ser null

        // Construtor padrão (pode ser útil para o designer XAML ou se usado noutros contextos)
        public Perfil()
        {
            InitializeComponent();
            LoadUserProfileData();
            // Nota: Se este construtor for chamado, _mainWindowInstance será null.
            // A lógica em EditProfileButton_Click já considera esta possibilidade.
        }

        // CONSTRUTOR para aceitar a instância da MainWindow
        public Perfil(MainWindow mainWindow)
        {
            InitializeComponent();
            _mainWindowInstance = mainWindow; // Guarda a referência
            LoadUserProfileData();
        }

        private void LoadUserProfileData()
        {
            var userNameTextBlock = this.FindName("UserNameTextBlock") as TextBlock;
            var userEmailTextBlock = this.FindName("UserEmailTextBlock") as TextBlock;
            var profileImageBrush = this.FindName("ProfileImageBrush") as System.Windows.Media.ImageBrush;

            if (userNameTextBlock != null)
                userNameTextBlock.Text = MainWindow.NomeUtilizadorLogado;

            if (userEmailTextBlock != null)
                userEmailTextBlock.Text = MainWindow.EmailUtilizadorLogado;

            string? imagePath = MainWindow.CaminhoFotoUtilizadorLogado;
            if (profileImageBrush != null)
            {
                if (!string.IsNullOrEmpty(imagePath) && File.Exists(imagePath))
                {
                    try
                    {
                        BitmapImage bitmap = new BitmapImage();
                        bitmap.BeginInit();
                        bitmap.CacheOption = BitmapCacheOption.OnLoad;
                        bitmap.CreateOptions = BitmapCreateOptions.IgnoreImageCache;
                        bitmap.UriSource = new Uri(imagePath, UriKind.Absolute);
                        bitmap.EndInit();
                        profileImageBrush.ImageSource = bitmap;
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show($"Erro ao carregar foto no perfil: {ex.Message}", "Erro de Imagem", MessageBoxButton.OK, MessageBoxImage.Error);
                        SetPlaceholderImage(profileImageBrush);
                    }
                }
                else
                {
                    SetPlaceholderImage(profileImageBrush);
                }
            }
        }

        private void SetPlaceholderImage(System.Windows.Media.ImageBrush? imageBrush)
        {
            if (imageBrush != null)
            {
                imageBrush.ImageSource = null;
                // Exemplo: se a Ellipse que usa este brush se chama ProfileDisplayEllipse no Perfil.xaml
                // var ellipse = this.FindName("ProfileDisplayEllipse") as Ellipse;
                // if (ellipse != null) ellipse.Fill = new System.Windows.Media.SolidColorBrush(System.Windows.Media.Colors.LightGray);
            }
        }

        private void EditProfileButton_Click(object sender, RoutedEventArgs e)
        {
            MainWindow? hostWindow = _mainWindowInstance ?? Window.GetWindow(this) as MainWindow;

            if (hostWindow != null)
            {
                // Assume-se que EditarPerfil.xaml.cs foi modificado para aceitar MainWindow
                EditarPerfil editarPerfilPage = new EditarPerfil(hostWindow);
                hostWindow.NavigateToPage(editarPerfilPage, "Editar Perfil");
            }
            else
            {
                MessageBox.Show("Não foi possível aceder à janela principal para navegar para editar perfil.", "Erro de Navegação", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
}
