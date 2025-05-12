using System;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Imaging;

namespace FinalLab
{
    public partial class Perfil : UserControl
    {
        public Perfil()
        {
            InitializeComponent();
            LoadUserProfileData();
        }

        private void LoadUserProfileData()
        {
            if (UserNameTextBlock != null)
                UserNameTextBlock.Text = MainWindow.NomeUtilizadorLogado;

            if (UserEmailTextBlock != null)
                UserEmailTextBlock.Text = MainWindow.EmailUtilizadorLogado;

            string? imagePath = MainWindow.CaminhoFotoUtilizadorLogado;
            if (ProfileImageBrush != null)
            {
                if (!string.IsNullOrEmpty(imagePath) && File.Exists(imagePath))
                {
                    try
                    {
                        BitmapImage bitmap = new BitmapImage();
                        bitmap.BeginInit();
                        bitmap.CacheOption = BitmapCacheOption.OnLoad;
                        bitmap.UriSource = new Uri(imagePath, UriKind.Absolute);
                        bitmap.EndInit();
                        ProfileImageBrush.ImageSource = bitmap;
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show($"Erro ao carregar foto (visualização): {ex.Message}", "Erro de Imagem", MessageBoxButton.OK, MessageBoxImage.Error);
                        SetPlaceholderImage();
                    }
                }
                else
                {
                    SetPlaceholderImage();
                }
            }
        }

        private void SetPlaceholderImage()
        {
            // Implementação da lógica do placeholder aqui, se necessário
        }

        private void EditProfileButton_Click(object sender, RoutedEventArgs e)
        {
            if (Window.GetWindow(this) is MainWindow hostWindow)
            {
                hostWindow.NavigateToPage(new EditarPerfil());
            }
            else
            {
                MessageBox.Show("Não foi possível aceder à janela principal.", "Erro", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
}
