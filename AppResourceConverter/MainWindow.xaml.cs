using System;
using System.Collections.Generic;
using System.IO;
using System.Windows;
using Microsoft.Win32;

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
                List<string> duplicated = null;

                if (
                   converter.Convert
                   (
                   filePath,
                   out new_path,
                   out duplicated
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
                List<string> duplicated = null;

                if (
                   converter.Convert
                   (
                   filePath,
                   out new_path,
                   out duplicated
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

        // Convert i18n resource string to unicode string
        private void Button_Click_3(object sender, RoutedEventArgs e)
        {
            this.OpenFolderDialog
            (
            (string filePath) =>
            {
                // get all resource file from the same path
                DirectoryInfo dir = new DirectoryInfo(filePath);
                FileInfo[] files = dir.GetFiles("*.resource");
                I18nResourceConverter converter = new I18nResourceConverter();

                foreach (FileInfo file in files)
                {
                    converter.Convert(file.FullName);
                }

                MessageBox.Show("convert succeed!!");
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

        private void OpenFolderDialog(Action<string> callback)
        {
            using (System.Windows.Forms.FolderBrowserDialog dialog = new System.Windows.Forms.FolderBrowserDialog())
            {
                if (dialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                {
                    callback(dialog.SelectedPath);
                }
            }
        }
    }
}
