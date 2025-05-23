// Ficheiro: EditarPerfil.xaml.cs
using Microsoft.Win32;
using System;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Imaging;
using System.Diagnostics;
using System.Text.RegularExpressions;
using System.Windows.Media;

namespace FinalLab
{
    public partial class EditarPerfil : UserControl
    {
        private MainWindow? _mainWindowInstance;
        private string? _tempImagePathForEditing;

        public EditarPerfil(MainWindow mainWindow) : this() { _mainWindowInstance = mainWindow; Debug.WriteLine("[EditarPerfil.xaml.cs] Construtor com MainWindow."); }
        public EditarPerfil() { InitializeComponent(); Debug.WriteLine("[EditarPerfil.xaml.cs] InitializeComponent Concluído."); this.Loaded += EditarPerfil_Loaded; }

        private void EditarPerfil_Loaded(object sender, RoutedEventArgs e)
        {
            Debug.WriteLine("[EditarPerfil.xaml.cs] EditarPerfil_Loaded iniciado.");
            try
            {
                _mainWindowInstance ??= Window.GetWindow(this) as MainWindow;
                if (_mainWindowInstance == null) { MessageBox.Show("Erro: Janela principal não encontrada."); Debug.WriteLine("[EditarPerfil.xaml.cs] ERRO: _mainWindowInstance é null."); return; }
                LoadUserProfileForEditing();
            }
            catch (Exception ex) { MessageBox.Show($"Erro ao carregar dados para edição: {ex.Message}", "Erro"); Debug.WriteLine($"[EditarPerfil.xaml.cs] ERRO EditarPerfil_Loaded: {ex}"); }
        }

        private void LoadUserProfileForEditing()
        {
            Debug.WriteLine("[EditarPerfil.xaml.cs] LoadUserProfileForEditing chamado.");
            if (EditNomePerfilTextBox == null || EditEmailTextBlock == null || EditProfileImageBrush == null)
            { Debug.WriteLine("[EditarPerfil.xaml.cs] ERRO: Controlo XAML nulo em LoadUserProfileForEditing."); return; }

            EditNomePerfilTextBox.Text = MainWindow.NomePerfilEditavel;
            EditEmailTextBlock.Text = MainWindow.EmailUtilizadorLogado;
            _tempImagePathForEditing = MainWindow.CaminhoFotoUtilizadorLogado;
            Debug.WriteLine($"[EditarPerfil.xaml.cs] Perfil carregado. Nome: {EditNomePerfilTextBox.Text}, Email: {EditEmailTextBlock.Text}, TempPath: {_tempImagePathForEditing}");
            UpdateProfileImageOnPageFromPath(_tempImagePathForEditing);
        }

        public void UpdateProfileImageOnPage(BitmapImage? bitmap)
        {
            if (EditProfileImageBrush != null) { EditProfileImageBrush.ImageSource = bitmap; Debug.WriteLine(bitmap != null ? "[EditarPerfil.xaml.cs] Imagem da página atualizada." : "[EditarPerfil.xaml.cs] Imagem da página definida como null."); }
            else { Debug.WriteLine("[EditarPerfil.xaml.cs] EditProfileImageBrush na página EditarPerfil é null."); }
        }

        private void UpdateProfileImageOnPageFromPath(string? imagePath)
        {
            Debug.WriteLine($"[EditarPerfil.xaml.cs] UpdateProfileImageFromPath. Path: {imagePath}");
            if (EditProfileImageBrush == null) { Debug.WriteLine("[EditarPerfil.xaml.cs] EditProfileImageBrush (FromPath) é null."); return; }
            BitmapImage? bmp = null;
            if (!string.IsNullOrEmpty(imagePath) && File.Exists(imagePath))
            {
                try
                {
                    bmp = new BitmapImage(); bmp.BeginInit();
                    bmp.UriSource = new Uri(imagePath, UriKind.Absolute);
                    bmp.CacheOption = BitmapCacheOption.OnLoad; bmp.CreateOptions = BitmapCreateOptions.IgnoreImageCache;
                    bmp.EndInit(); bmp.Freeze(); Debug.WriteLine("[EditarPerfil.xaml.cs] Imagem (FromPath) carregada para preview.");
                }
                catch (Exception ex) { Debug.WriteLine($"[EditarPerfil.xaml.cs] ERRO ao carregar imagem para preview (FromPath): {ex.Message}"); bmp = null; }
            }
            else { Debug.WriteLine("[EditarPerfil.xaml.cs] Caminho nulo ou ficheiro não existe para preview (FromPath)."); }
            EditProfileImageBrush.ImageSource = bmp;
        }

        private void ChangePictureButton_Click(object sender, RoutedEventArgs e)
        {
            Debug.WriteLine("[EditarPerfil.xaml.cs] ChangePictureButton_Click chamado.");
            try
            {
                OpenFileDialog openFileDialog = new OpenFileDialog { Filter = "Ficheiros de Imagem (*.jpg;*.jpeg;*.png)|*.jpg;*.jpeg;*.png", Title = "Selecionar Foto de Perfil" };
                if (openFileDialog.ShowDialog() == true)
                { _tempImagePathForEditing = openFileDialog.FileName; Debug.WriteLine($"[EditarPerfil.xaml.cs] Nova imagem selecionada: {_tempImagePathForEditing}"); UpdateProfileImageOnPageFromPath(_tempImagePathForEditing); }
            }
            catch (Exception ex) { MessageBox.Show($"Erro ao selecionar imagem: {ex.Message}", "Erro"); Debug.WriteLine($"[EditarPerfil.xaml.cs] ERRO ChangePictureButton_Click: {ex}"); }
        }

        private void SaveProfileButton_Click(object sender, RoutedEventArgs e)
        {
            Debug.WriteLine("[EditarPerfil.xaml.cs] SaveProfileButton_Click chamado.");
            try
            {
                if (_mainWindowInstance == null || EditEmailTextBlock == null || EditNomePerfilTextBox == null)
                { MessageBox.Show("Erro interno ao guardar perfil.", "Erro", MessageBoxButton.OK, MessageBoxImage.Error); Debug.WriteLine("[EditarPerfil.xaml.cs] ERRO SaveProfile: Instância ou controlo nulo."); return; }

                string novoNome = EditNomePerfilTextBox.Text.Trim();
                if (string.IsNullOrWhiteSpace(novoNome))
                { MessageBox.Show("O nome de perfil não pode ser vazio.", "Validação", MessageBoxButton.OK, MessageBoxImage.Warning); EditNomePerfilTextBox.Focus(); return; }

                string novoEmail = EditEmailTextBlock.Text.Trim();
                if (string.IsNullOrWhiteSpace(novoEmail) || !Regex.IsMatch(novoEmail, @"^al\d{5}@alunos\.utad\.pt$", RegexOptions.IgnoreCase))
                { MessageBox.Show("Formato de email inválido (ex: alxxxxx@alunos.utad.pt).", "Validação", MessageBoxButton.OK, MessageBoxImage.Warning); EditEmailTextBlock.Focus(); return; }

                MainWindow.NomePerfilEditavel = novoNome;
                MainWindow.EmailUtilizadorLogado = novoEmail;
                MainWindow.CaminhoFotoUtilizadorLogado = _tempImagePathForEditing;

                Debug.WriteLine($"[EditarPerfil.xaml.cs] Perfil guardado. Nome: {MainWindow.NomePerfilEditavel}, Email: {MainWindow.EmailUtilizadorLogado}, FotoPath: {MainWindow.CaminhoFotoUtilizadorLogado}");

                _mainWindowInstance.NavigateToPage("Perfil do Utilizador");
                MessageBox.Show("Perfil atualizado com sucesso!", "Sucesso", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex) { MessageBox.Show($"Erro ao guardar perfil: {ex.Message}", "Erro", MessageBoxButton.OK, MessageBoxImage.Error); Debug.WriteLine($"[EditarPerfil.xaml.cs] ERRO SaveProfileButton_Click: {ex}"); }
        }

        private void CancelEditButton_Click(object sender, RoutedEventArgs e) { Debug.WriteLine("[EditarPerfil.xaml.cs] CancelEditButton_Click chamado."); _mainWindowInstance?.NavigateToPage("Perfil do Utilizador"); }
    }
}
