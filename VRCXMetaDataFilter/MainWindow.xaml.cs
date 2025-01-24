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
using Directory = System.IO.Directory;
using System.Text.Json;
using Hjg.Pngcs;
using Hjg.Pngcs.Chunks;

namespace VRCXMetaDataFilter;




/// <summary>
/// Interaction logic for MainWindow.xaml
/// </summary>
public partial class MainWindow : Window
{
    public string name = "VRCXMetaDataFilter";
    public string version = "0.0.6";
    public string author = "Jettsd";
    public string description = "Filter VRCX metadata";
    public string url = "https://github.com/neoth/VRCXMetaDataFilter";
    
    private Dictionary<string, string> PathPair = new Dictionary<string, string>();
    string jsonfile = "config.json";

   

    public MainWindow()
    {
        InitializeComponent();
        
        // Load the folder path from the config file
        if (File.Exists("config.json"))
        {
            string jsonReader = File.ReadAllText(jsonfile);
            string folderPathJson = JsonSerializer.Deserialize<string>(jsonReader);
            FolderPathInput.Text = folderPathJson;
        }
    }
    
    private void LoadFiles(string folderPath)
    {
        PathPair.Clear(); // Clear the dictionary to avoid duplicate entries

        try
        {
            
            foreach (string imgFile in Directory.EnumerateFiles(folderPath, "*.png", SearchOption.AllDirectories))
            {
                PngReader pngReader = new PngReader(new FileStream(imgFile, FileMode.Open, FileAccess.Read, FileShare.Read), imgFile);
                PngMetadata pngMetadata = pngReader.GetMetadata();
                List<PngChunkTextVar> pngChunkTextVars = pngMetadata.GetTxtsForKey("Description");
                foreach (PngChunkTextVar pngChunkTextVar in pngChunkTextVars)
                {
                    PathPair[imgFile] = pngChunkTextVar.GetVal();
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
        bool hasResults = false;
        // Grab the folder path from the input field
        string folderPath = FolderPathInput.Text;
        string jsonstring = JsonSerializer.Serialize(folderPath);
        File.WriteAllText(jsonfile, jsonstring);
        
        // Grab text from the search field
        string search = SearchRequest.Text;
        

        if (string.IsNullOrWhiteSpace(folderPath) || !Directory.Exists(folderPath))
        {
            MessageBox.Show("Please enter a valid folder path.", "Invalid Path", MessageBoxButton.OK, MessageBoxImage.Warning);
            return;
        }
        if (string.IsNullOrWhiteSpace(search))
        {
            MessageBox.Show("You entered nothing!.", ":(", MessageBoxButton.OK, MessageBoxImage.Warning);
            return;
        }

        // Load files from the folder path
        LoadFiles(folderPath);



        PicDisplay.Children.Clear();

        // Filter through all files for matches to the search
        foreach (KeyValuePair<string, string> path in PathPair)
        {
            string imgPath = path.Key;
            string imgMetadata = path.Value;
            

            if (imgMetadata.Contains(search))
            {
                // If we've found a match, set the flag to true'
                hasResults = true;
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
        // If no results were found, display a message
        if (hasResults == false)
        {
            MessageBox.Show("No results found take more pictures!", "No Results", MessageBoxButton.OK, MessageBoxImage.Information);
        }
        
    }
}
