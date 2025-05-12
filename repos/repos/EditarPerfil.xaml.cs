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
        private string? tempImagePath;

        public EditarPerfil()
        {
            InitializeComponent();
            LoadUserProfileForEditing();
        }

        private void LoadUserProfileForEditing()
        {
            if (NameTextBoxEdit != null) NameTextBoxEdit.Text = MainWindow.NomeUtilizadorLogado;
            if (EmailTextBoxEdit != null) EmailTextBoxEdit.Text = MainWindow.EmailUtilizadorLogado;
            tempImagePath = MainWindow.CaminhoFotoUtilizadorLogado;
            if (ProfileImageBrushEdit != null)
            {
                if (!string.IsNullOrEmpty(tempImagePath) && File.Exists(tempImagePath))
                {
                    try
                    {
                        BitmapImage bitmap = new BitmapImage();
                        bitmap.BeginInit();
                        bitmap.CacheOption = BitmapCacheOption.OnLoad;
                        bitmap.UriSource = new Uri(tempImagePath, UriKind.Absolute);
                        bitmap.EndInit();
                        ProfileImageBrushEdit.ImageSource = bitmap;
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show($"Erro ao carregar imagem (edição): {ex.Message}", "Erro Imagem", MessageBoxButton.OK, MessageBoxImage.Error);
                        SetPlaceholderImageEdit();
                    }
                }
                else
                {
                    SetPlaceholderImageEdit();
                }
            }
        }

        private void SetPlaceholderImageEdit() { /* Lógica para placeholder */ }

        private void ChangePictureButton_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog { Title = "Selecionar Nova Foto", Filter = "Imagens (*.jpg;*.jpeg;*.png)|*.jpg;*.jpeg;*.png" };
            if (openFileDialog.ShowDialog() == true)
            {
                try
                {
                    BitmapImage bitmap = new BitmapImage();
                    bitmap.BeginInit();
                    bitmap.CacheOption = BitmapCacheOption.OnLoad;
                    bitmap.UriSource = new Uri(openFileDialog.FileName, UriKind.Absolute);
                    bitmap.EndInit();
                    if (ProfileImageBrushEdit != null) ProfileImageBrushEdit.ImageSource = bitmap;
                    tempImagePath = openFileDialog.FileName;
                }
                catch (Exception ex) { MessageBox.Show($"Erro ao carregar nova imagem: {ex.Message}", "Erro Imagem", MessageBoxButton.OK, MessageBoxImage.Error); }
            }
        }

        private void SaveProfileButton_Click(object sender, RoutedEventArgs e)
        {
            if (NameTextBoxEdit == null || EmailTextBoxEdit == null) return;
            if (string.IsNullOrWhiteSpace(NameTextBoxEdit.Text)) { MessageBox.Show("O nome não pode estar vazio.", "Validação", MessageBoxButton.OK, MessageBoxImage.Warning); NameTextBoxEdit.Focus(); return; }
            if (string.IsNullOrWhiteSpace(EmailTextBoxEdit.Text) || !EmailTextBoxEdit.Text.Contains("@")) { MessageBox.Show("Email inválido.", "Validação", MessageBoxButton.OK, MessageBoxImage.Warning); EmailTextBoxEdit.Focus(); return; }

            MainWindow.NomeUtilizadorLogado = NameTextBoxEdit.Text;
            MainWindow.EmailUtilizadorLogado = EmailTextBoxEdit.Text;
            MainWindow.CaminhoFotoUtilizadorLogado = tempImagePath;
            MessageBox.Show("Alterações guardadas! (Simulado)", "Sucesso", MessageBoxButton.OK, MessageBoxImage.Information);

            // ATUALIZA A BARRA SUPERIOR!
            if (Window.GetWindow(this) is MainWindow hostWindow) { hostWindow.UpdateTopBarUserName(); hostWindow.NavigateToPage(new Perfil()); }
        }

        private void CancelEditButton_Click(object sender, RoutedEventArgs e)
        {
            if (Window.GetWindow(this) is MainWindow hostWindow) { hostWindow.NavigateToPage(new Perfil()); }
        }
    }
}
