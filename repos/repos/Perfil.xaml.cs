// FinalLab/Perfil.xaml.cs
using System;
using System.Diagnostics;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Imaging;

namespace FinalLab
{
    public partial class Perfil : UserControl
    {
        private MainWindow? _mainWindowInstance;

        public Perfil(MainWindow mainWindow)
        {
            InitializeComponent();
            _mainWindowInstance = mainWindow;
        }

        public Perfil()
        {
            InitializeComponent();
            if (Application.Current.MainWindow is MainWindow mw)
            {
                _mainWindowInstance = mw;
            }
            else
            {
                Debug.WriteLine("ERRO: Perfil UserControl não conseguiu obter a instância da MainWindow no construtor padrão.");
            }
        }

        private void Perfil_Loaded(object sender, RoutedEventArgs e)
        {
            if (_mainWindowInstance == null && Application.Current.MainWindow is MainWindow mw)
            {
                _mainWindowInstance = mw;
            }

            if (_mainWindowInstance != null)
            {
                LoadUserProfileData();
                Debug.WriteLine("Perfil_Loaded: Dados do perfil carregados.");
            }
            else
            {
                Debug.WriteLine("ERRO em Perfil_Loaded: _mainWindowInstance é nula. Não foi possível carregar dados.");
                NomeUtilizadorTextBlock.Text = "Erro ao carregar nome";
                EmailUtilizadorTextBlock.Text = "Erro ao carregar email";
                ProfileImageBrush.ImageSource = null; // Limpa a imagem se houver erro
            }
        }

        public void LoadUserProfileData()
        {
            if (_mainWindowInstance == null)
            {
                Debug.WriteLine("LoadUserProfileData: Tentativa de carregar dados com _mainWindowInstance nula.");
                return;
            }

            NomeUtilizadorTextBlock.Text = MainWindow.NomePerfilEditavel;
            EmailUtilizadorTextBlock.Text = MainWindow.EmailUtilizadorLogado;
            UpdateProfileImageOnPageFromGlobalPath();
        }

        public void UpdateProfileNameOnPage(string newName)
        {
            NomeUtilizadorTextBlock.Text = newName;
        }

        public void UpdateProfileImageOnPage(BitmapImage? bitmap)
        {
            ProfileImageBrush.ImageSource = bitmap;
        }

        public void UpdateProfileImageOnPageFromGlobalPath()
        {
            try
            {
                if (!string.IsNullOrEmpty(MainWindow.CaminhoFotoUtilizadorLogado) && File.Exists(MainWindow.CaminhoFotoUtilizadorLogado))
                {
                    BitmapImage bitmap = new BitmapImage();
                    bitmap.BeginInit();
                    bitmap.UriSource = new Uri(MainWindow.CaminhoFotoUtilizadorLogado, UriKind.Absolute);
                    bitmap.CacheOption = BitmapCacheOption.OnLoad;
                    bitmap.EndInit();
                    ProfileImageBrush.ImageSource = bitmap;
                }
                else
                {
                    string defaultImagePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Resources", "default_profile.png");
                    if (File.Exists(defaultImagePath))
                    {
                        BitmapImage defaultBitmap = new BitmapImage();
                        defaultBitmap.BeginInit();
                        defaultBitmap.UriSource = new Uri(defaultImagePath, UriKind.Absolute);
                        defaultBitmap.CacheOption = BitmapCacheOption.OnLoad;
                        defaultBitmap.EndInit();
                        ProfileImageBrush.ImageSource = defaultBitmap;
                    }
                    else
                    {
                        ProfileImageBrush.ImageSource = null;
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Erro ao carregar imagem de perfil na página Perfil: {ex.Message}");
                ProfileImageBrush.ImageSource = null;
            }
        }

        private void EditProfileButton_Click(object sender, RoutedEventArgs e)
        {
            if (_mainWindowInstance != null)
            {
                _mainWindowInstance.NavigateToPage("Editar Perfil", new EditarPerfil(_mainWindowInstance));
                Debug.WriteLine("EditProfileButton_Click: Navegando para Editar Perfil.");
            }
            else
            {
                Debug.WriteLine("ERRO em EditProfileButton_Click: _mainWindowInstance é nula.");
                MessageBox.Show("Não é possível editar o perfil neste momento.", "Erro", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
}
