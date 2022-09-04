using System;
using System.IO;
using System.Net.Http;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Xml;

namespace MloFinder {
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow {

        public MainWindow() {
            InitializeComponent();
        }

        private void OnDrop(object sender, DragEventArgs e) {
            if (!e.Data.GetDataPresent(DataFormats.FileDrop)) return;
            Cursor = Cursors.Wait;

            try {
                string[] files = e.Data.GetData(DataFormats.FileDrop) as string[];
                var client = new HttpClient();
                foreach (var file in files) {
                    byte[] bytes = File.ReadAllBytes(file);
                    string name = new FileInfo(file).Name;

                    var response = client
                        .PostAsync(new Uri("http://leon-hoppe.de:5123/unpack?name=" + name), new ByteArrayContent(bytes))
                        .GetAwaiter().GetResult();

                    if (!response.IsSuccessStatusCode) continue;

                    var xml = response.Content.ReadAsStringAsync().GetAwaiter().GetResult();

                    var item = new ListViewItem { Content = name, DataContext = xml };
                    item.Selected += OnFileSelect;
                    Files.Items.Add(item);
                }
            }
            catch {}
            finally {
                Cursor = Cursors.Arrow;
            }
        }

        private void OnFileSelect(object sender, RoutedEventArgs e) {
            if (!(sender is ListViewItem item)) return;
            var xml = item.DataContext as string;
            var doc = new XmlDocument();
            doc.LoadXml(xml);
            
            var node = doc.GetElementsByTagName("instancedData")[0];
            node?.ParentNode?.RemoveChild(node);

            var minNode = doc.GetElementsByTagName("streamingExtentsMin")[0];
            var maxNode = doc.GetElementsByTagName("streamingExtentsMax")[0];
            
            OutPos.Text = $"{minNode?.Attributes?.GetNamedItem("x").Value} {minNode?.Attributes?.GetNamedItem("y").Value} {maxNode?.Attributes?.GetNamedItem("z").Value}";
        }
    }
}