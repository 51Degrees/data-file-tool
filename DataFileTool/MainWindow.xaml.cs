using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
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
            string uncompressedFilename = null;

            try
            {
                DataList.Items.Clear();

                // Attempt to read data from file.
                var header = FileHeader.FromFile(filename);

                if (header == null)
                {
                    // Did not match expected format so check if it's compressed.
                    if (TryExract(filename, out uncompressedFilename))
                    {
                        header = FileHeader.FromFile(uncompressedFilename);
                        DataList.Items.Add(new ListViewItem() { Content = "THIS FILE IS COMPRESSED. IT MUST BE EXTRACTED BEFORE IT CAN BE USED BY THE 51DEGREES API.", Foreground = Brushes.Red });
                    }
                }

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
                    DataList.Items.Add($"Datafile guid: {header.ExportTagGuid}");
                    DataList.Items.Add($"Publish date: {header.PublishDate.ToShortDateString()}");
                    DataList.Items.Add($"Date of next expected update: {header.NextExportDate.ToShortDateString()}");
                    if (header.LongestString.HasValue) { DataList.Items.Add($"Longest string: {header.LongestString}"); }
                    DataList.Items.Add($"Total number of string values: {header.TotalStringValues}");
                    DataList.Items.Add($"Copyright notice: {header.CopyrightNotice.Value}");
                }
            }
            finally
            {
                try
                {
                    if (uncompressedFilename != null)
                    {
                        File.Delete(uncompressedFilename);
                    }
                }
                catch { /* Cannot delete the temp file. Can't do anything about it so just leave it. */ }
            }
        }


        private const int ZIP_LEAD_BYTES = 0x04034b50;
        private const ushort GZIP_LEAD_BYTES = 0x8b1f;

        private bool TryExract(string filename, out string uncompressedFilename)
        {
            uncompressedFilename = null;
            bool gzip = isGZip(filename);
            bool pkZip = isPkZip(filename);

            if (gzip)
            {
                try
                {
                    uncompressedFilename = Path.GetTempFileName();
                    using (var source = new GZipStream(File.OpenRead(filename), CompressionMode.Decompress))
                    using (var dest = File.OpenWrite(uncompressedFilename))
                    {
                        source.CopyTo(dest);
                    }
                }
                catch
                {
                    uncompressedFilename = null;
                }
            }

            return gzip || pkZip;
        }

        private bool isGZip(string filename)
        {
            using (var reader = new BinaryReader(File.OpenRead(filename)))
            {
                return GZIP_LEAD_BYTES == reader.ReadUInt16();
            }
        }
        private bool isPkZip(string filename)
        {
            using (var reader = new BinaryReader(File.OpenRead(filename)))
            {
                return ZIP_LEAD_BYTES == reader.ReadInt32();
            }
        }
    }
}
