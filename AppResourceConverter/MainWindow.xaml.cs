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
using Microsoft.Win32;
using System.IO;

using Resource.Convert;

namespace AppResourceConverter
{
    /// <summary>
    /// MainWindow.xaml 的互動邏輯
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        // format iOS resource string
        private void Button_Click(object sender, RoutedEventArgs e)
        {
            this.OpenFileDialog
            (
            FileDialogType.iOS,
            (string filePath) =>
            {
                string new_path = null;
                IOSResourceFormatter formatter = new IOSResourceFormatter();

                if (!formatter.Format(filePath, out new_path))
                {
                    MessageBox.Show("format failed.");
                }
                else
                {
                    FileInfo file = new FileInfo(new_path);
                    MessageBox.Show("format succeed!!\nnew file is: " + file.Name);
                }
            }
            );
        }

        // convert android string xml to iOS resource string
        private void Button_Click_1(object sender, RoutedEventArgs e)
        {
            this.OpenFileDialog
            (
            FileDialogType.Android,
            (string filePath) =>
            {
                string new_path = null;
                Android2iOSResourceConverter converter = new Android2iOSResourceConverter();

                if (
                   converter.Convert
                   (
                   filePath, 
                   out new_path,
                   (List<string> duplicated) => { }
                   )
                   )
                {
                    FileInfo file = new FileInfo(new_path);
                    MessageBox.Show("convert succeed!!\nnew file is: " + file.Name);
                }
                else
                {
                    MessageBox.Show("convert failed.");
                }
            }
            );
        }

        // convert iOS resource string to android string xml
        private void Button_Click_2(object sender, RoutedEventArgs e)
        {
            this.OpenFileDialog
            (
            FileDialogType.iOS,
            (string filePath) =>
            {
                string new_path = null;
                IOS2AndroidResourceConverter converter = new IOS2AndroidResourceConverter();

                if (
                   converter.Convert
                   (
                   filePath, 
                   out new_path,
                   (List<string> duplicated) =>{}
                   )
                   )
                {
                    FileInfo file = new FileInfo(new_path);
                    MessageBox.Show("convert succeed!!\nnew file is: " + file.Name);
                }
                else
                {
                    MessageBox.Show("convert failed.");
                }
            }
            );
        }


        enum FileDialogType : ushort
        {
            iOS = 0,
            Android
        }

        private void OpenFileDialog(FileDialogType type, Action<string> callback)
        {
            OpenFileDialog dialog = new OpenFileDialog();
            dialog.Filter = type == FileDialogType.iOS ? "select file (*.strings)|*.strings" : "select file (*.xml)|*.xml";

            if (dialog.ShowDialog() == true)
            {
                callback(dialog.FileName);
            }
        }
    }
}
