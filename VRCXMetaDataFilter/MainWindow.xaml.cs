using System.DirectoryServices;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.IO;
using System.Net.Mime;
using MetadataExtractor;
using MetadataExtractor.Formats.Png;
using Directory = System.IO.Directory;
using System.Text.Json;

namespace VRCXMetaDataFilter;

/// <summary>
/// Interaction logic for MainWindow.xaml
/// </summary>
public partial class MainWindow : Window
{
    private Dictionary<string, string> PathPair = new Dictionary<string, string>();

    public MainWindow()
    {
        InitializeComponent();
    }

    // TextChangedEventHandler delegate method.
    private void textChangedEventHandler(object sender, TextChangedEventArgs args)
    {
        // Omitted Code: Insert code that does something whenever
        // the text changes...
    } // end textChangedEventHandler

    private void LoadFiles(string folderPath)
    {
        PathPair.Clear(); // Clear the dictionary to avoid duplicate entries

        try
        {
            foreach (string imgFile in Directory.EnumerateFiles(folderPath, "*.png", SearchOption.AllDirectories))
            {
                var directories = ImageMetadataReader.ReadMetadata(imgFile); // Gives MetaDataExtractor each file
                foreach (PngDirectory directory in directories.OfType<PngDirectory>()) // Goes through each piece of Metadata
                {
                    if (directory.Name == "PNG-iTXt") // Filter for text chunk
                    {
                        foreach (Tag tag in directory.Tags)
                        {
                            if (tag.Description.StartsWith("Description")) // Refines filter for metadata made by VRCX
                            {
                                PathPair[imgFile] = tag.Description;
                            }
                        }
                    }
                }
            }
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Error loading files: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    private void BtnFilter_OnClick(object sender, RoutedEventArgs e)
    {
        // Grab the folder path from the input field
        string folderPath = FolderPathInput.Text;

        if (string.IsNullOrWhiteSpace(folderPath) || !Directory.Exists(folderPath))
        {
            MessageBox.Show("Please enter a valid folder path.", "Invalid Path", MessageBoxButton.OK, MessageBoxImage.Warning);
            return;
        }

        // Load files from the folder path
        LoadFiles(folderPath);

        // Grab text from the search field
        string search = SearchRequest.Text;

        PicDisplay.Children.Clear();

        // Filter through all files for matches to the search
        foreach (KeyValuePair<string, string> path in PathPair)
        {
            string imgPath = path.Key;
            string imgMetadata = path.Value;

            if (imgMetadata.Contains(search))
            {
                // Create an image for display
                Image imageResult = new Image();
                imageResult.Width = 150;
                imageResult.Height = 75;
                BitmapImage image = new BitmapImage();
                image.BeginInit();
                image.UriSource = new Uri(imgPath);
                image.DecodePixelWidth = 150;
                image.DecodePixelHeight = 75;
                image.EndInit();
                imageResult.Source = image;

                // Create a button to wrap the image
                Button imgBtn = new Button();
                imgBtn.Content = imageResult;

                // Attach the click event handler
                imgBtn.Click += (s, ev) =>
                {
                    // Open the image file
                    System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo
                    {
                        FileName = imgPath,
                        UseShellExecute = true
                    });
                };

                // Add the button to the PicDisplay panel
                PicDisplay.Children.Add(imgBtn);
            }
        }
    }
}
