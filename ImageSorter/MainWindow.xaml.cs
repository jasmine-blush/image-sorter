using System;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media.Imaging;
using Ookii.Dialogs.Wpf;
using WpfAnimatedGif;
using System.Diagnostics;

namespace ImageSorter {
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window {
        public string SelectedImagePath { get; set; }

        private readonly object _dummyNode = null;
        private string _sortDirectory;
        private string _imageDirectory;
        private string _selectedPath = "";
        private MediaElement _videoElement = new MediaElement();
        private string _currentFile;
        private Random _rng;

        public MainWindow() {
            InitializeComponent();

            _rng = new Random();

            MessageBox.Show("Two file browsers will pop up one after another. \n" +
                "In the first one, please select the root directory of where you want to sort your media into. \n" +
                "After that, select the root directory of your unsorted media files.",
                "How to Start",
                MessageBoxButton.OK,
                MessageBoxImage.Information);

            //Destination directory selection
            VistaFolderBrowserDialog dialog = new VistaFolderBrowserDialog();
            dialog.Description = "Please select the destination root-directory:";
            dialog.Multiselect = false;
            dialog.UseDescriptionForTitle = true;
            dialog.ShowDialog(this);
            while(dialog.SelectedPath == "") {
                Application.Current.Shutdown();
                return;
            }
            _sortDirectory = dialog.SelectedPath;

            //Source directory selection
            dialog.SelectedPath = "";
            dialog.Description = "Please select the source root-directory:";
            dialog.ShowDialog(this);
            while(dialog.SelectedPath == "") {
                Application.Current.Shutdown();
                return;
            }
            _imageDirectory = dialog.SelectedPath;

            //Setup the video player inside MainWindow
            _videoElement = new MediaElement();
            _videoElement.SetValue(Grid.ColumnProperty, 2);
            _videoElement.SetValue(Grid.RowProperty, 1);
            _videoElement.LoadedBehavior = MediaState.Manual;
            _videoElement.MediaEnded += new RoutedEventHandler(VideoElement_MediaEnded);
            _videoElement.Volume = VolumeSlider.Value;

            _currentFile = Directory.GetFiles(_imageDirectory, "*", SearchOption.AllDirectories)[0];
            ShowCurrentFile();
        }

        /// <summary>
        /// Resets the currently showed file view and loads and shows the next _currentFile. Supports images, gif and mp4 files.
        /// </summary>
        private void ShowCurrentFile() {
            _videoElement.Stop();
            _videoElement.Close();
            _videoElement.Source = null;
            MainGrid.Children.Remove(_videoElement);

            imageElement.Source = null;
            ImageBehavior.SetAnimatedSource(imageElement, null);

            FileNameField.Text = _currentFile;

            string extension = _currentFile.Split('.')[1];
            if(extension == "mp4") {
                MainGrid.Children.Add(_videoElement);
                _videoElement.Source = new Uri(_currentFile);
                _videoElement.Volume = VolumeSlider.Value;
                _videoElement.Play();
            } else if(extension == "gif"){
                var image = new BitmapImage();
                image.BeginInit();
                image.CacheOption = BitmapCacheOption.OnLoad;
                image.UriSource = new Uri(_currentFile);
                image.EndInit();
                ImageBehavior.SetAnimatedSource(imageElement, image);
            } else {
                var image = new BitmapImage();
                image.BeginInit();
                image.CacheOption = BitmapCacheOption.OnLoad;
                image.UriSource = new Uri(_currentFile);
                image.EndInit();
                imageElement.Source = image;
            }
        }

        /// <summary>
        /// Helper method for file deletion
        /// </summary>
        /// <param name="filePath">Path of the file to be deleted.</param>
        public void DeleteFile(string filePath) {
            File.Delete(filePath);
        }

        /// <summary>
        /// Adds directory tree to Treeview upon window load
        /// </summary>
        private void Window_Loaded(object sender, RoutedEventArgs e) {
            foreach(string s in Directory.GetDirectories(_sortDirectory)) {
                TreeViewItem item = new TreeViewItem();
                item.Header = s.Substring(s.LastIndexOf("\\") + 1);
                item.Tag = s;
                item.FontWeight = FontWeights.Normal;
                item.Items.Add(_dummyNode);
                item.Expanded += new RoutedEventHandler(Folder_Expanded);
                foldersItem.Items.Add(item);
            }
        }

        /// <summary>
        /// Repeats video when video ends
        /// </summary>
        private void VideoElement_MediaEnded(object sender, RoutedEventArgs e) {
            _videoElement.Source = new Uri(_currentFile);
            _videoElement.Volume = VolumeSlider.Value;
            _videoElement.Play();
        }

        /// <summary>
        /// Adds subfolders to the directory tree when a folder is expanded
        /// </summary>
        void Folder_Expanded(object sender, RoutedEventArgs e) {
            TreeViewItem item = (TreeViewItem)sender;
            if(item.Items.Count == 1 && item.Items[0] == _dummyNode) {
                item.Items.Clear();
                try {
                    foreach(string s in Directory.GetDirectories(item.Tag.ToString())) {
                        TreeViewItem subitem = new TreeViewItem();
                        subitem.Header = s.Substring(s.LastIndexOf("\\") + 1);
                        subitem.Tag = s;
                        subitem.FontWeight = FontWeights.Normal;
                        subitem.Items.Add(_dummyNode);
                        subitem.Expanded += new RoutedEventHandler(Folder_Expanded);
                        item.Items.Add(subitem);
                    }
                } catch(Exception) { }
            }
        }

        /// <summary>
        /// Gets the currently selected directory from directory tree when new item is selected
        /// </summary>
        private void FoldersItem_SelectedItemChanged(object sender, RoutedPropertyChangedEventArgs<object> e) {
            TreeView tree = (TreeView)sender;
            TreeViewItem temp = ((TreeViewItem)tree.SelectedItem);

            if(temp == null)
                return;
            SelectedImagePath = "";
            string temp1 = "";
            string temp2 = "";
            while(true) {
                temp1 = temp.Header.ToString();
                if(temp1.Contains(@"\")) {
                    temp2 = "";
                }
                SelectedImagePath = temp1 + temp2 + SelectedImagePath;
                if(temp.Parent.GetType().Equals(typeof(TreeView))) {
                    break;
                }
                temp = ((TreeViewItem)temp.Parent);
                temp2 = @"\";
            }

            //show user selected path
            StatusText.Text = "Selected Folder: " + SelectedImagePath;
            _selectedPath = _sortDirectory + "\\" + SelectedImagePath;
        }

        /// <summary>
        /// Deletes the _currentFile and sets the next one when the delete button is clicked
        /// </summary>
        private void Delete_Click(object sender, ExecutedRoutedEventArgs e) {
            try {
                var scope = FocusManager.GetFocusScope((DependencyObject)sender);
                FocusManager.SetFocusedElement(scope, null);
                Keyboard.ClearFocus(); // remove keyboard focus
                Keyboard.Focus(this);

                string lastFile = _currentFile;
                _currentFile = Directory.GetFiles(_imageDirectory, "*", SearchOption.AllDirectories)[1];

                ShowCurrentFile();
                Microsoft.VisualBasic.FileIO.FileSystem.DeleteFile(lastFile,
                    Microsoft.VisualBasic.FileIO.UIOption.OnlyErrorDialogs,
                    Microsoft.VisualBasic.FileIO.RecycleOption.SendToRecycleBin);
            }catch(Exception ex) {
                MessageBox.Show(ex.Message + "\n" + _currentFile);
                throw ex;
            }
        }

        /// <summary>
        /// Moves the _currentFile to the selected destination directory and sets a new _currentFile
        /// </summary>
        private void Move_Click(object sender, ExecutedRoutedEventArgs e) {
            try {
                var scope = FocusManager.GetFocusScope((DependencyObject)sender);
                FocusManager.SetFocusedElement(scope, null);
                Keyboard.ClearFocus(); // remove keyboard focus
                Keyboard.Focus(this);

                if(_selectedPath != "") {
                    string lastFile = _currentFile;
                    _currentFile = Directory.GetFiles(_imageDirectory, "*", SearchOption.AllDirectories)[1];
                    ShowCurrentFile();

                    string[] split = lastFile.Split('\\');
                    string filename = split[split.Length-1];
                    if(!File.Exists(_selectedPath + "\\" + filename)) {
                        File.Move(lastFile, _selectedPath + "\\" + filename);
                    } else {
                        string[] namesplit = filename.Split('.');
                        string rawname = namesplit[0] + _rng.Next(10000, 100000).ToString();
                        string extension = namesplit[1];
                        string new_name = _selectedPath + "\\" + rawname + "." + extension;
                        File.Move(lastFile, new_name);
                    }
                } else {
                    MessageBox.Show(this, "Please select a folder in the left column. This is where the current file will be moved to.",
                        "No destination selected", MessageBoxButton.OK, MessageBoxImage.Warning);
                }
            } catch(Exception ex) {
                MessageBox.Show(ex.Message + "\n" + _currentFile);
                throw ex;
            }
        }

        /// <summary>
        /// Opens the current file in explorer
        /// </summary>
        private void Explorer_Click(object sender, RoutedEventArgs e) {
            var scope = FocusManager.GetFocusScope((DependencyObject)sender);
            FocusManager.SetFocusedElement(scope, null);
            Keyboard.ClearFocus(); // remove keyboard focus
            Keyboard.Focus(this);
            Process.Start("explorer.exe", "/select," + _currentFile);
        }

        /// <summary>
        /// Sets the video player volume to current slider value
        /// </summary>
        private void Slider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e) {
            _videoElement.Volume = VolumeSlider.Value;
        }
    }
}
