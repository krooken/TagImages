﻿using System;
using System.Collections.Generic;
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
using System.Windows.Forms;
using System.IO;
using Excel = Microsoft.Office.Interop.Excel; 

namespace TagImages
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {

        private System.Collections.Generic.List<string> fileList = null;
        private int fileListIndex = 0;
        private Excel.Range filenameRange, photoQualityRange;

        public MainWindow()
        {
            // Folder to be scanned recursively for image files
            string scanPath = null;

            // Open a folder selection dialog to choose root folder
            // from which to search for image files.
            // Dialog is initiated with application folder
            // since the program is likely to be at catalog root.
            FolderBrowserDialog dialog = new FolderBrowserDialog();
            dialog.SelectedPath = Directory.GetCurrentDirectory();
            DialogResult result = dialog.ShowDialog();
            
            // Exit if cancelled.
            if (result == System.Windows.Forms.DialogResult.OK)
            {
                scanPath = dialog.SelectedPath;
            } 
            else
            {
                Environment.Exit(1);
            }

            // Create OpenFileDialog to select excel workbook
            Microsoft.Win32.OpenFileDialog fileDialog = new Microsoft.Win32.OpenFileDialog();
            fileDialog.DefaultExt = ".xlsx";
            fileDialog.Filter = "Excel documents (.xlsx)|*.xlsx";
            Nullable<bool> fileResult = fileDialog.ShowDialog();
            string filename = null;
            if (fileResult == true)
            {
                filename = fileDialog.FileName;
            }
            else
            {
                Environment.Exit(1);
            }

            // Open excel sheet in specified workbook
            Excel.Application excelApp = new Excel.Application();
            Excel.Workbook excelWorkbook = excelApp.Workbooks.Open(filename);
            Excel.Sheets excelSheets = excelWorkbook.Worksheets;
            string currentSheet = "ImageData";
            Excel.Worksheet excelWorksheet = (Excel.Worksheet)excelSheets.get_Item(currentSheet);
            Excel.Range usedRange = (Excel.Range)excelWorksheet.UsedRange;
            Excel.Range rows = (Excel.Range)usedRange.Rows;

            Console.WriteLine(rows.AddressLocal);
            Console.WriteLine(rows.Row);

            // Find the columns in which file name and photo quality is stored
            int filenameColumn = -1;
            int photoQualityColumn = -1;
            for (int i = 0; i < rows.Columns.Count; i++)
            {
                Console.WriteLine(rows.Cells[1, i + 1].Value2.ToString());
                string columnHeader = rows.Cells[1, i + 1].Value2.ToString();
                if (columnHeader.ToLower().Equals("filename"))
                {
                    filenameColumn = i;
                }
                if (columnHeader.ToLower().Equals("photo quality"))
                {
                    photoQualityColumn = i;
                }
            }

            Console.WriteLine("filenameColumn = " + filenameColumn);
            Console.WriteLine("photoQualityColumn = " + photoQualityColumn);

            // Get the ranges that include the file name and photo quality
            Console.WriteLine(usedRange.Cells[2, filenameColumn+1].Address);
            Console.WriteLine(usedRange.Cells[usedRange.Rows.Count, filenameColumn+1].Address);
            filenameRange = (Excel.Range)usedRange.get_Range((string)usedRange.Cells[2, filenameColumn + 1].Address, (string)usedRange.Cells[usedRange.Rows.Count, filenameColumn + 1].Address);
            photoQualityRange = (Excel.Range)usedRange.get_Range((string)usedRange.Cells[2, photoQualityColumn + 1].Address, (string)usedRange.Cells[usedRange.Rows.Count, photoQualityColumn + 1].Address);
            Console.WriteLine(filenameRange.Address);
            Console.WriteLine(photoQualityRange.Address);

            // Bring up GUI
            InitializeComponent();

            // What to search for.
            // Mask is catalog root folder plus search expression.
            string mask = scanPath + "\\*.*";
            // Acquire list of all image files.
            fileList = GetAllFiles(mask, (info) => IsImageFile(info)).ToList();

            // Show first image in the list of files.
            BitmapImage img = new BitmapImage(new Uri(fileList[fileListIndex]));
            this.PictureFrame.Source = img;

            // Add event listeners to buttons.
            this.btnPrev.Click += btnPrev_Click;
            this.btnNext.Click += btnNext_Click;
            
        }

        // Change image position in file list and update picture in GUI. 
        // Position is relative the current.
        void TraverseFileList(int deltaPosition)
        {
            fileListIndex = Modulo((fileListIndex + deltaPosition), fileList.Count);
            this.PictureFrame.Source = new BitmapImage(new Uri(fileList[fileListIndex]));
        }

        // Event listener for 'Next' button.
        void btnNext_Click(object sender, RoutedEventArgs e)
        {
            TraverseFileList(+1);
        }

        // Event listener for 'Previous' button.
        void btnPrev_Click(object sender, RoutedEventArgs e)
        {
            TraverseFileList(-1);
        }

        // Calculate modulo. Negative numerator renders positive result.
        int Modulo(int numerator, int denominator)
        {
            return (numerator % denominator + denominator) % denominator;
        }

        // List all files recurively from a root folder defined by path.
        // Check file lambda expression that determines which files to return.
        public static IEnumerable<string> GetAllFiles(string path, Func<FileInfo, bool> checkFile = null)
        {
            // Get mask, i.e., file part of path. ("*.*" if path is "C:\\yadayadayada\\*.*")
            string mask = System.IO.Path.GetFileName(path);
            if (string.IsNullOrEmpty(mask))
                mask = "*.*";
            // Get base path of folder, i.e., C:\\yadayadayada\\
            path = System.IO.Path.GetDirectoryName(path);
            // List all files
            string[] files = Directory.GetFiles(path, mask, SearchOption.AllDirectories);
            // Go though all files and determine whther they are to be included in the list.
            foreach (string file in files)
            {
                if (checkFile == null || checkFile(new FileInfo(file)))
                    yield return file;
            }
        }

        // Checks whether a file is an jpeg or tiff file.
        public static bool IsImageFile(FileInfo info)
        {
            string extension = System.IO.Path.GetExtension(info.Name).ToLower();
            bool isJpeg = extension == ".jpeg" || extension == ".jpg";
            bool isTiff = extension == ".tiff" || extension == ".tif";
            return isJpeg || isTiff;
        }
    }
}
