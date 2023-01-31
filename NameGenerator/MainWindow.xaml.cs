using Microsoft.Win32;
using System;
using System.IO;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;

namespace NameGenerator
{
    public partial class MainWindow : Window
    {

        private OpenFileDialog _openFileDialog;
        private SaveFileDialog _saveFileDialog;

        private List<string> _surnames;
        private List<string> _forenames;

        public MainWindow()
        {
            _openFileDialog = InitializeOpenFileDialog();
            InitializeComponent();

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
                        SurnamesListBox.ItemsSource = _surnames;
                        break;
                    case "ForenameLoadButton":
                        _forenames = LoadNamesToList(_forenames, temp);
                        ForenamesListBox.ItemsSource = _surnames;
                        break;
                    default:
                        MessageBox.Show("Unhandled source.", "Error in NamesLoader", MessageBoxButton.OK, MessageBoxImage.Error);
                        return;
                }

            }
        }

        private List<string> LoadNamesToList(List<string> parent, List<string> names)
        {
            parent = parent ?? new List<string>();
            foreach (string name in names)
            {
                parent.Add(name);
            }
            return parent;
        }

        private string GetDownloadFolderPath()
        {
            return Registry.GetValue(@"HKEY_CURRENT_USER\Software\Microsoft\Windows\CurrentVersion\Explorer\Shell Folders", "{374DE290-123F-4565-9164-39C4925E467B}", String.Empty).ToString();
        }
    }
}
