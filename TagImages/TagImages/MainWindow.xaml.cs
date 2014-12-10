using System;
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

namespace TagImages
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
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

            // Bring up GUI
            InitializeComponent();

            // What to search for.
            // Mask is catalog root folder plus search expression.
            string mask = scanPath + "\\*.*";
            // Acquire list of all image files.
            System.Collections.Generic.List<string> list = GetAllFiles(mask, (info) => IsImageFile(info)).ToList();

            // Show first image in the list of files.
            BitmapImage img = new BitmapImage(new Uri(list[0]));
            this.PictureFrame.Source = img;
            
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
