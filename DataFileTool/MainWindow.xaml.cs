using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace DataFileTool
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        private void ListBox_Drop(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(DataFormats.FileDrop))
            {
                string[] files = (string[])e.Data.GetData(DataFormats.FileDrop);
                ReadDataFromFile(files[0]);
            }
        }

        private void ReadDataFromFile(string filename)
        {
            var header = FileHeader.FromFile(filename);

            DataList.Items.Clear();
            if (header == null)
            {
                DataList.Items.Add("This file does not appear to be a 51Degrees data file.");
            }
            else
            {
                DataList.Items.Add($"Header structure: {header.READER}");
                DataList.Items.Add($"Dataset format version: {header.DataSetFormatVersion}");
                DataList.Items.Add($"Dataset format name: {header.DataSetFormatName.Value}");
                DataList.Items.Add($"Dataset name: {header.DataSetName.Value}");
                DataList.Items.Add($"Dataset guid: {header.DataSetGuid}");
                DataList.Items.Add($"Dataset guid: {header.ExportTagGuid}");
                DataList.Items.Add($"Publish date: {header.PublishDate.ToShortDateString()}");
                DataList.Items.Add($"Date of next expected update: {header.NextExportDate.ToShortDateString()}");
                if (header.LongestString.HasValue) { DataList.Items.Add($"Longest string: {header.LongestString}"); }
                DataList.Items.Add($"Total number of string values: {header.TotalStringValues}");
                DataList.Items.Add($"Copyright notice: {header.CopyrightNotice.Value}");
            }
        }
    }
}
