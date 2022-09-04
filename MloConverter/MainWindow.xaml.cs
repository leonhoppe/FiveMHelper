using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using Microsoft.WindowsAPICodePack.Dialogs;

namespace MloConverter {
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow {
        private readonly string _optionFilePath = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) +
                                                  Path.DirectorySeparatorChar + "MloConverter" +
                                                  Path.DirectorySeparatorChar + "settings.ini";
        private const string TempFolder = "./Temp/Mlo/";
        private readonly List<FileInfo> _streamFiles = new List<FileInfo>();
        
        public MainWindow() {
            InitializeComponent();
            
            if (File.Exists(_optionFilePath))
                OutPath.Content = File.ReadAllText(_optionFilePath);
            else {
                Directory.CreateDirectory(_optionFilePath.Replace("settings.ini", ""));
                OutPath.Content = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);
            }
        }

        private void OnDrop(object sender, DragEventArgs e) {
            if (!e.Data.GetDataPresent(DataFormats.FileDrop)) return;

            Clear(null, null);
            string dlcFile = ((string[])e.Data.GetData(DataFormats.FileDrop))[0];
            Utils.UnpackDlcFile(dlcFile, TempFolder, Compress.IsChecked.Value);
            UpdateUi(TempFolder, "dlc.rpf");
        }
        
        private void UpdateUi(string folder, string rootFolderName) {
            var info = new DirectoryInfo(folder);
            var root = new TreeViewItem { Header = rootFolderName, IsExpanded = true };
            AddSubFiles(info, root);
            FolderView.Items.Add(root);
            UpdateOutList();
        }

        private void AddSubFiles(DirectoryInfo info, TreeViewItem root) {
            foreach (var dir in info.EnumerateDirectories()) {
                var item = new TreeViewItem { Header = dir.Name, DataContext = dir.FullName };
                AddSubFiles(dir, item);
                root.Items.Add(item);
            }
            
            foreach (var file in info.EnumerateFiles()) {
                
                if (!new []{".meta", ".xml"}.Contains(file.Extension))
                    _streamFiles.Add(file);
                
                var item = new TreeViewItem { Header = file.Name, DataContext = file.FullName };
                item.MouseDoubleClick += (sender, args) => ListInteraction(sender, null);
                root.Items.Add(item);
            }
        }

        private void UpdateOutList() {
            StreamFiles.Items.Clear();
            
            foreach (var stream in _streamFiles) {
                var item = new ListViewItem { Content = stream.Name, DataContext = stream.FullName };
                item.KeyDown += ListInteraction;
                StreamFiles.Items.Add(item);
            }
        }
        
        private void ListInteraction(object sender, KeyEventArgs e) {
            if (sender is ListViewItem) {
                if (!new[] { Key.Return, Key.Delete }.Contains(e.Key)) return;
                var item = sender as ListViewItem;
                
                if (item.Parent.Equals(StreamFiles)) {
                    _streamFiles.Remove(_streamFiles.Single(stream => stream.FullName.Equals(item.DataContext)));
                    StreamFiles.Items.Remove(item);
                    return;
                }
            }

            if (sender is TreeViewItem) {
                var item = sender as TreeViewItem;

                if (!_streamFiles.Any(file => file.FullName.Equals(item.DataContext as string)))
                    _streamFiles.Add(new FileInfo(item.DataContext as string));
                
                UpdateOutList();
            }
        }
        
        private void Clear(object sender, RoutedEventArgs e) {
            _streamFiles.Clear();
            StreamFiles.Items.Clear();
            FolderView.Items.Clear();
            
            if (Directory.Exists(TempFolder))
                Directory.Delete(TempFolder, true);
        }
        
        private async Task AnimateBuildButton(Button button) {
            Brush origColor = button.Foreground;
            object origContent = button.Content;
            
            button.Foreground = Brushes.GreenYellow;
            button.Content = "Done!";

            await Task.Delay(1000);

            button.Content = origContent;
            button.Foreground = origColor;
        }

        private void ChooseOutDir(object sender, RoutedEventArgs e) {
            var dialog = new CommonOpenFileDialog();
            dialog.IsFolderPicker = true;
            dialog.Multiselect = false;
            var result = dialog.ShowDialog();

            if (result == CommonFileDialogResult.Ok) {
                var path = dialog.FileNames.First();
                File.WriteAllText(_optionFilePath, path);
                OutPath.Content = path;
            }
        }

        private void Build(object sender, RoutedEventArgs e) {
            var info = Directory.CreateDirectory((string)OutPath.Content + Path.DirectorySeparatorChar + CarName.Text);
            File.WriteAllText(info.FullName + Path.DirectorySeparatorChar + "fxmanifest.lua", Utils.ManifestContent);

            var streamRoot = info.CreateSubdirectory("stream");

            foreach (var stream in _streamFiles)
                stream.CopyTo(streamRoot.FullName + Path.DirectorySeparatorChar + stream.Name);

            AnimateBuildButton(sender as Button);
        }
    }
}