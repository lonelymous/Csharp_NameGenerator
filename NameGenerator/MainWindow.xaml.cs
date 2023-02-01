using Microsoft.Win32;
using System;
using System.IO;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Collections.ObjectModel;
using System.Xml.Linq;
using System.Security.Cryptography;
using System.Linq;

namespace NameGenerator
{
    public partial class MainWindow : Window
    {

        private OpenFileDialog _openFileDialog;
        private SaveFileDialog _saveFileDialog;

        private ObservableCollection<string> _surnames;
        private ObservableCollection<string> _forenames;
        private ObservableCollection<string> _names;

        private uint _maxNameCount;
        public uint MaxNameCount { get => _maxNameCount;}

        public MainWindow()
        {
            InitializeComponent();
            _surnames = new ObservableCollection<string>();
            _forenames = new ObservableCollection<string>();
            _names = new ObservableCollection<string>();
            _openFileDialog = InitializeOpenFileDialog();
            _saveFileDialog = InitializeSaveFileDialog();
            ReloadLists();
        }
        private string GetDownloadFolderPath()
        {
            return Registry.GetValue(@"HKEY_CURRENT_USER\Software\Microsoft\Windows\CurrentVersion\Explorer\Shell Folders", "{374DE290-123F-4565-9164-39C4925E467B}", String.Empty).ToString();
        }
        private SaveFileDialog InitializeSaveFileDialog() 
        { 
            SaveFileDialog saveFileDialog = new SaveFileDialog();
            saveFileDialog.AddExtension = true;
            saveFileDialog.DefaultExt = "txt";
            saveFileDialog.Filter = "Szöveges fájl (*.txt) | *.txt|CSV fájl (*.csv) |*.csv|Összes fájl (*.*) |*.*";
            saveFileDialog.InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
            saveFileDialog.Title = "Adja meg hová szeretné menteni a fájlt.";
            return saveFileDialog;
        }
        private OpenFileDialog InitializeOpenFileDialog()
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.AddExtension = true;
            openFileDialog.DefaultExt = "txt";
            openFileDialog.Filter = "Szöveges fájl (*.txt) | *.txt|CSV fájl (*.csv) |*.csv|Összes fájl (*.*) |*.*";
            openFileDialog.InitialDirectory = GetDownloadFolderPath();
            openFileDialog.Title = "Adja meg a fájlt.";
            return openFileDialog;
        }
        private ObservableCollection<string> LoadNamesToList(ObservableCollection<string> parent, List<string> names)
        {
            // Add or Overwrite
            parent = parent ?? new ObservableCollection<string>();
            foreach (string name in names)
            {
                parent.Add(name);
            }
            return parent;
        }
        private void ReloadLists()
        {
            SurnamesListBox.ItemsSource = _surnames;
            ForenamesListBox.ItemsSource = _forenames;
            SetMaxNameCount();
            JumpToTheEndOfNameList();
        }
        private void SetMaxNameCount() 
        {
            int a = _surnames.Count;
            int b = _forenames.Count;
            _maxNameCount = Convert.ToUInt32(a < b ? a : b);
            // I have to use this, because if I use the commented xaml code it gets 10 for some reason.
            sliderNameCount.Maximum = MaxNameCount;
        }
        private void JumpToTheEndOfNameList()
        {
            SurnamesListBox.Items.MoveCurrentToLast();
            ForenamesListBox.Items.MoveCurrentToLast();
            NamesListBox.Items.MoveCurrentToLast();
            SurnamesListBox.ScrollIntoView(SurnamesListBox.Items.CurrentItem);
            ForenamesListBox.ScrollIntoView(ForenamesListBox.Items.CurrentItem);
            NamesListBox.ScrollIntoView(NamesListBox.Items.CurrentItem);
        }
        private void GenerateNames(bool middleName)
        {
            Random r = new Random();
            int count = Convert.ToInt32(sliderNameCount.Value);

            if (middleName)
            {
                for (int i = 0; i < count; i++)
                {
                    _names.Add($"{_forenames[r.Next(0, _forenames.Count)]} {_surnames[r.Next(0, _surnames.Count)]} {_surnames[r.Next(0, _surnames.Count)]}");
                }
            }
            else
            {
                for (int i = 0; i < count; i++)
                {
                    _names.Add($"{_forenames[r.Next(0, _forenames.Count)]} {_surnames[r.Next(0, _surnames.Count)]}");
                }
            }
        }
        private void NamesLoader(object sender, RoutedEventArgs e)
        {
            if (_openFileDialog.ShowDialog() == true)
            {
                List<string> temp = new List<string>();
                var source = e.Source as Button;

                // threading + progressBar
                foreach (string filename in _openFileDialog.FileNames)
                {
                    foreach (string line in File.ReadAllLines(filename))
                    {
                        temp.Add(line);
                    }
                }
                switch (source.Name)
                {
                    case "SurnameLoadButton":
                        _surnames = LoadNamesToList(_surnames, temp);
                        break;
                    case "ForenameLoadButton":
                        _forenames = LoadNamesToList(_forenames, temp);
                        break;
                    default:
                        MessageBox.Show("Unhandled source.", "Error in NamesLoader", MessageBoxButton.OK, MessageBoxImage.Error);
                        return;
                }
                ReloadLists();
            }
        }
        private void NameDeleter(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            var source = e.Source as ListBox;
            try
            {
                switch (source.Name)
                {
                    case "SurnamesListBox":
                        _surnames.RemoveAt(SurnamesListBox.SelectedIndex);
                        break;
                    case "ForenamesListBox":
                        _forenames.RemoveAt(ForenamesListBox.SelectedIndex);
                        break;
                    case "NamesListBox":
                        throw new NotImplementedException();
                    default:
                        throw new Exception("Unhandled source");
                }
            }
            catch (ArgumentOutOfRangeException)
            {
                MessageBox.Show("Üres a lista nem tudsz benne törölni", "Error in NameDeleter", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Error in NameDeleter", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            finally
            {
                ReloadLists();
            }
        }
        private void DeleteNamesButton_Click(object sender, RoutedEventArgs e)
        {
            _names.Clear();
            NamesListBox.Items.Clear();
        }
        private void GenerateNamesButton_Click(object sender, RoutedEventArgs e)
        {
            GenerateNames(!(bool)rbSelectionOne.IsChecked);
            ReloadLists();
        }
        private void SortNamesButton_Click(object sender, RoutedEventArgs e)
        {
            ObservableCollection<string> temp = new ObservableCollection<string>(_names.OrderBy(p => p));
            _names.Clear();
            _names = temp;
            StatusBarSort.Content = "Rendezett névsor!";
        }
        private void SaveNamesButton_Click(object sender, RoutedEventArgs e)
        {
            throw new NotImplementedException();
        }
    }
}
