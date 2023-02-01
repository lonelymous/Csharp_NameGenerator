using Microsoft.Win32;
using System;
using System.IO;
using System.Linq;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

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
            NamesListBox.ItemsSource = _names;
            SetMaxNameCount();
            JumpToTheEndOfNameList();
            StatusBarSort.Content = "";
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
        private void NameDeleter(object sender, MouseButtonEventArgs e)
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
                        _names.RemoveAt(NamesListBox.SelectedIndex);
                        break;
                    default:
                        throw new Exception("Nincs lekezelve ez a forrás.");
                }
            }
            catch (ArgumentOutOfRangeException)
            {
                MessageBox.Show("Üres a lista nem tudsz benne törölni.", "Error in NameDeleter", MessageBoxButton.OK, MessageBoxImage.Error);
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
            ReloadLists();
        }
        private void GenerateNamesButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                int count = Convert.ToInt32(sliderNameCount.Value);

                if (count <= 0)
                {
                    throw new Exception("0 vagy annál kevesebb nevet nem tudsz generálni.");
                }

                Random r = new Random();
                string surname = String.Empty;
                string forename = String.Empty;
                string middlename = null;
                
                for (int i = 0; i < count; i++)
                {
                    surname = _surnames[r.Next(0, _surnames.Count)];
                    forename = _forenames[r.Next(0, _forenames.Count)];
                    _surnames.Remove(surname);
                    _forenames.Remove(forename);
                    if (!(bool)rbSelectionOne.IsChecked)
                    {
                        middlename = _forenames[r.Next(0, _forenames.Count)];
                        _forenames.Remove(middlename);
                    }
                    _names.Add($"{surname} {forename}{(middlename == null ? "": " " + middlename)}");
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Error in GenerateNames", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            finally
            {
                ReloadLists();
            }
        }
        private void SortNamesButton_Click(object sender, RoutedEventArgs e)
        {
            ObservableCollection<string> temp = new ObservableCollection<string>(_names.OrderBy(p => p));
            _names.Clear();
            _names = temp;
            ReloadLists();
            StatusBarSort.Content = "Rendezett névsor!";
        }
        private void SaveNamesButton_Click(object sender, RoutedEventArgs e)
        {
            if (_saveFileDialog.ShowDialog() == true)
            {
                try
                {
                    if (Path.GetExtension(_saveFileDialog.FileName) == ".csv")
                    {
                        List<string> temp = new List<string>();
                        temp = _names.ToList();
                        for (int i = 0; i < temp.Count(); i++)
                        {
                            temp[i] = temp[i].Replace(" ", ";");
                        }
                        File.WriteAllLines(_saveFileDialog.FileName, temp);
                    }
                    else
                    {
                        File.WriteAllLines(_saveFileDialog.FileName, _names);
                    }
                    MessageBox.Show("Sikeres mentés!", "Mentés", MessageBoxButton.OK, MessageBoxImage.Information);
                }
                catch
                {
                    MessageBox.Show("Sikertelen mentés!");
                }
            }
        }
    }
}
