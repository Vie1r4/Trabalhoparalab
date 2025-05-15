using Microsoft.Win32;
using System;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Imaging;

namespace FinalLab
{
    public partial class EditarPerfil : UserControl
    {
        private string? _tempImagePath; // Renomeado para seguir convenções e tornado anulável explicitamente
        private MainWindow _mainWindowInstance; // Para guardar a referência à MainWindow

        // CONSTRUTOR MODIFICADO para aceitar MainWindow
        public EditarPerfil(MainWindow mainWindow)
        {
            InitializeComponent();
            _mainWindowInstance = mainWindow; // Guarda a instância da MainWindow
            LoadUserProfileForEditing();
        }

        // Construtor padrão (se necessário para o designer XAML ou outros usos)
        // No entanto, se este UserControl só é criado com a referência da MainWindow,
        // este construtor pode não ser chamado em tempo de execução.
        // Se o designer XAML se queixar, pode adicionar um construtor padrão vazio,
        // mas a lógica que depende de _mainWindowInstance não funcionará.
        /*
        public EditarPerfil()
        {
            InitializeComponent();
            // Se este construtor for chamado, _mainWindowInstance será null.
            // A lógica que depende de _mainWindowInstance pode falhar.
            // LoadUserProfileForEditing(); // Pode não funcionar corretamente sem _mainWindowInstance
        }
        */

        private void LoadUserProfileForEditing()
        {
            // Assume-se que NameTextBoxEdit, EmailTextBoxEdit, e ProfileImageBrushEdit são x:Name no seu EditarPerfil.xaml
            if (this.FindName("NameTextBoxEdit") is TextBox nameTextBox) nameTextBox.Text = MainWindow.NomeUtilizadorLogado;
            if (this.FindName("EmailTextBoxEdit") is TextBox emailTextBox) emailTextBox.Text = MainWindow.EmailUtilizadorLogado;

            _tempImagePath = MainWindow.CaminhoFotoUtilizadorLogado; // Guarda o caminho atual para o caso de o utilizador cancelar

            if (this.FindName("ProfileImageBrushEdit") is System.Windows.Media.ImageBrush profileImageBrush)
            {
                if (!string.IsNullOrEmpty(MainWindow.CaminhoFotoUtilizadorLogado) && File.Exists(MainWindow.CaminhoFotoUtilizadorLogado))
                {
                    try
                    {
                        BitmapImage bitmap = new BitmapImage();
                        bitmap.BeginInit();
                        bitmap.CacheOption = BitmapCacheOption.OnLoad;
                        bitmap.CreateOptions = BitmapCreateOptions.IgnoreImageCache;
                        bitmap.UriSource = new Uri(MainWindow.CaminhoFotoUtilizadorLogado, UriKind.Absolute);
                        bitmap.EndInit();
                        profileImageBrush.ImageSource = bitmap;
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show($"Erro ao carregar imagem para edição: {ex.Message}", "Erro Imagem", MessageBoxButton.OK, MessageBoxImage.Error);
                        SetPlaceholderImageEdit(profileImageBrush);
                    }
                }
                else
                {
                    SetPlaceholderImageEdit(profileImageBrush);
                }
            }
        }

        private void SetPlaceholderImageEdit(System.Windows.Media.ImageBrush? imageBrush)
        {
            if (imageBrush != null)
            {
                imageBrush.ImageSource = null;
                // Adicione aqui a lógica para definir uma cor ou imagem de placeholder no ImageBrush
                // ou na Ellipse que o contém, se necessário.
            }
        }

        private void ChangePictureButton_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog
            {
                Title = "Selecionar Nova Foto",
                Filter = "Ficheiros de Imagem (*.jpg;*.jpeg;*.png)|*.jpg;*.jpeg;*.png|Todos os Ficheiros (*.*)|*.*"
            };

            if (openFileDialog.ShowDialog() == true)
            {
                try
                {
                    BitmapImage bitmap = new BitmapImage();
                    bitmap.BeginInit();
                    bitmap.CacheOption = BitmapCacheOption.OnLoad;
                    bitmap.CreateOptions = BitmapCreateOptions.IgnoreImageCache;
                    bitmap.UriSource = new Uri(openFileDialog.FileName, UriKind.Absolute);
                    bitmap.EndInit();

                    if (this.FindName("ProfileImageBrushEdit") is System.Windows.Media.ImageBrush profileImageBrush)
                    {
                        profileImageBrush.ImageSource = bitmap;
                    }
                    _tempImagePath = openFileDialog.FileName; // Atualiza o caminho temporário com a nova foto selecionada
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Erro ao carregar nova imagem: {ex.Message}", "Erro Imagem", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        private void SaveProfileButton_Click(object sender, RoutedEventArgs e)
        {
            var nameTextBox = this.FindName("NameTextBoxEdit") as TextBox;
            var emailTextBox = this.FindName("EmailTextBoxEdit") as TextBox;

            if (nameTextBox == null || emailTextBox == null)
            {
                MessageBox.Show("Erro: Controlos de edição não encontrados.", "Erro Interno", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            if (string.IsNullOrWhiteSpace(nameTextBox.Text))
            {
                MessageBox.Show("O nome não pode estar vazio.", "Validação", MessageBoxButton.OK, MessageBoxImage.Warning);
                nameTextBox.Focus();
                return;
            }
            if (string.IsNullOrWhiteSpace(emailTextBox.Text) || !emailTextBox.Text.Contains("@"))
            {
                MessageBox.Show("Email inválido.", "Validação", MessageBoxButton.OK, MessageBoxImage.Warning);
                emailTextBox.Focus();
                return;
            }

            // Atualiza os dados estáticos na MainWindow
            MainWindow.NomeUtilizadorLogado = nameTextBox.Text;
            MainWindow.EmailUtilizadorLogado = emailTextBox.Text;
            MainWindow.CaminhoFotoUtilizadorLogado = _tempImagePath; // Usa o caminho da foto selecionada (ou o original se não mudou)

            MessageBox.Show("Alterações guardadas!", "Sucesso", MessageBoxButton.OK, MessageBoxImage.Information);

            // ATUALIZA A BARRA SUPERIOR NA MAINWINDOW!
            _mainWindowInstance?.UpdateTopBarUserName(); // Atualiza o nome na barra superior
            _mainWindowInstance?.UpdateUserProfilePicture(); // Atualiza a foto na barra superior

            // Navegar de volta para a página de visualização do Perfil, passando a instância da MainWindow
            _mainWindowInstance?.NavigateToPage(new Perfil(_mainWindowInstance), "Perfil do Utilizador");
        }

        private void CancelEditButton_Click(object sender, RoutedEventArgs e)
        {
            // Navegar de volta para a página de visualização do Perfil, passando a instância da MainWindow
            _mainWindowInstance?.NavigateToPage(new Perfil(_mainWindowInstance), "Perfil do Utilizador");
        }
    }
}
