// FinalLab/EditarPerfil.xaml.cs
using Microsoft.Win32;
using System;
using System.Diagnostics;
using System.IO;
// using System.Text.RegularExpressions; // Não mais necessário para validação de email aqui
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Imaging;

namespace FinalLab
{
    public partial class EditarPerfil : UserControl
    {
        private MainWindow? _mainWindowInstance;
        private string _tempImagePathForEditing = string.Empty;

        public EditarPerfil(MainWindow mainWindow)
        {
            InitializeComponent();
            _mainWindowInstance = mainWindow;
        }

        public EditarPerfil()
        {
            InitializeComponent();
            if (Application.Current.MainWindow is MainWindow mw)
            {
                _mainWindowInstance = mw;
            }
            else
            {
                Debug.WriteLine("ERRO: EditarPerfil UserControl não conseguiu obter a instância da MainWindow.");
            }
        }

        private void EditarPerfil_Loaded(object sender, RoutedEventArgs e)
        {
            if (_mainWindowInstance == null && Application.Current.MainWindow is MainWindow mw)
            {
                _mainWindowInstance = mw;
            }

            if (_mainWindowInstance != null)
            {
                LoadUserProfileForEditing();
                Debug.WriteLine("EditarPerfil_Loaded: Dados do perfil para edição carregados.");
            }
            else
            {
                Debug.WriteLine("ERRO em EditarPerfil_Loaded: _mainWindowInstance é nula.");
                EditNomePerfilTextBox.IsEnabled = false;
                EditEmailTextBox.IsEnabled = false; // Desabilitar também o email se não houver instância
            }
        }

        private void LoadUserProfileForEditing()
        {
            if (_mainWindowInstance == null) return;

            EditNomePerfilTextBox.Text = MainWindow.NomePerfilEditavel;
            EditEmailTextBox.Text = MainWindow.EmailUtilizadorLogado; // Carrega o email atual
            _tempImagePathForEditing = MainWindow.CaminhoFotoUtilizadorLogado;
            UpdateProfileImageOnPageFromPath(_tempImagePathForEditing);
        }

        private void UpdateProfileImageOnPageFromPath(string imagePath)
        {
            try
            {
                if (!string.IsNullOrEmpty(imagePath) && File.Exists(imagePath))
                {
                    BitmapImage bitmap = new BitmapImage();
                    bitmap.BeginInit();
                    bitmap.UriSource = new Uri(imagePath, UriKind.Absolute);
                    bitmap.CacheOption = BitmapCacheOption.OnLoad;
                    bitmap.EndInit();
                    EditProfileImageBrush.ImageSource = bitmap;
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
                        EditProfileImageBrush.ImageSource = defaultBitmap;
                        // Não definir _tempImagePathForEditing para o default aqui, 
                        // a menos que o utilizador explicitamente o queira (ex: botão "remover foto")
                    }
                    else
                    {
                        EditProfileImageBrush.ImageSource = null;
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Erro ao carregar imagem para edição: {ex.Message}");
                EditProfileImageBrush.ImageSource = null;
            }
        }

        private void ChangePictureButton_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog
            {
                Title = "Selecionar Nova Foto de Perfil",
                Filter = "Ficheiros de Imagem|*.jpg;*.jpeg;*.png;*.gif;*.bmp|Todos os Ficheiros|*.*"
            };

            if (openFileDialog.ShowDialog() == true)
            {
                _tempImagePathForEditing = openFileDialog.FileName;
                UpdateProfileImageOnPageFromPath(_tempImagePathForEditing);
                Debug.WriteLine($"Nova imagem selecionada para perfil: {_tempImagePathForEditing}");
            }
        }

        private void SaveProfileButton_Click(object sender, RoutedEventArgs e)
        {
            if (_mainWindowInstance == null)
            {
                MessageBox.Show("Erro: Não é possível guardar o perfil.", "Erro", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            string novoNome = EditNomePerfilTextBox.Text.Trim();
            string novoEmail = EditEmailTextBox.Text.Trim(); // Ler o email do TextBox

            if (string.IsNullOrWhiteSpace(novoNome))
            {
                MessageBox.Show("O nome de perfil não pode ser vazio.", "Validação Falhou", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            // Validação simples de email (apenas se não está vazio)
            // REMOVIDA a validação de formato específico.
            if (string.IsNullOrWhiteSpace(novoEmail))
            {
                MessageBox.Show("O endereço de email não pode ser vazio.", "Validação Falhou", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            // Tentar uma validação muito básica se quiser (ex: contém '@'), mas o pedido é "aceitar qualquer email"
            // if (!novoEmail.Contains("@") || !novoEmail.Contains("."))
            // {
            //     MessageBox.Show("O formato do email parece inválido. Verifique por favor.", "Email Inválido", MessageBoxButton.OK, MessageBoxImage.Warning);
            //     return; // Opcional: manter esta validação mínima
            // }


            MainWindow.NomePerfilEditavel = novoNome;
            MainWindow.EmailUtilizadorLogado = novoEmail; // Guardar o novo email
            MainWindow.CaminhoFotoUtilizadorLogado = _tempImagePathForEditing;

            MessageBox.Show("Perfil atualizado com sucesso!", "Sucesso", MessageBoxButton.OK, MessageBoxImage.Information);

            _mainWindowInstance.NavigateToPage("Perfil do Utilizador"); // Volta para a página de Perfil para ver as alterações
            Debug.WriteLine("Perfil guardado. Navegando de volta para a página de Perfil.");
        }

        private void CancelEditButton_Click(object sender, RoutedEventArgs e)
        {
            if (_mainWindowInstance != null)
            {
                _mainWindowInstance.NavigateToPage("Perfil do Utilizador");
                Debug.WriteLine("Edição de perfil cancelada. Navegando de volta.");
            }
        }
    }
}
