using Microsoft.Phone.Controls;
using Newtonsoft.Json;
using System;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using Windows.ApplicationModel.Activation;
using Windows.Storage;
using Windows.Storage.Pickers;
using Windows.Storage.Streams;
using Windows.Web.Http;
using Windows.Web.Http.Headers;

namespace LeafServiceuserInterfaceSilverlight
{
    public partial class MainPage : PhoneApplicationPage
    {
        // Constructor
        public MainPage()
        {
            InitializeComponent();

            // Sample code to localize the ApplicationBar
            //BuildLocalizedApplicationBar();
        }

        // Sample code for building a localized ApplicationBar
        //private void BuildLocalizedApplicationBar()
        //{
        //    // Set the page's ApplicationBar to a new instance of ApplicationBar.
        //    ApplicationBar = new ApplicationBar();

        //    // Create a new button and set the text value to the localized string from AppResources.
        //    ApplicationBarIconButton appBarButton = new ApplicationBarIconButton(new Uri("/Assets/AppBar/appbar.add.rest.png", UriKind.Relative));
        //    appBarButton.Text = AppResources.AppBarButtonText;
        //    ApplicationBar.Buttons.Add(appBarButton);

        //    // Create a new menu item with the localized string from AppResources.
        //    ApplicationBarMenuItem appBarMenuItem = new ApplicationBarMenuItem(AppResources.AppBarMenuItemText);
        //    ApplicationBar.MenuItems.Add(appBarMenuItem);
        //}
        public void browseFile_Click(object sender, RoutedEventArgs e)
        {
            FileOpenPicker filePicker = new FileOpenPicker();
            filePicker.SuggestedStartLocation = PickerLocationId.PicturesLibrary;
            filePicker.ViewMode = PickerViewMode.Thumbnail;
            filePicker.FileTypeFilter.Add(".jpg");
            filePicker.FileTypeFilter.Add(".png");
            filePicker.PickSingleFileAndContinue();
        }
        
        public void ImageConversion_Click(object sender, RoutedEventArgs e)
        {
           
            byte[] queryImageInBytes  = ImageToBytes(QueryImagePic.Source as BitmapImage);
            queryImage(queryImageInBytes);
            //BitmapImage newImage = new BitmapImage();
            //newImage = ByteArrayToBitmapImage(queryImageInBytes);
            //newimage.Source = newImage;

        }
        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            var app = App.Current as App;
            if (app.FilePickerContinuationArgs != null)
            {
                this.ContinueFileOpenPicker(app.FilePickerContinuationArgs);
            }
        }

        public async void ContinueFileOpenPicker(FileOpenPickerContinuationEventArgs args)
        {
            if (args.Files.Count > 0)
            {
                //displayfilepath.Text = "Picked photo: " + args.Files[0].Name;
                StorageFile file = args.Files[0];
                if (file.Name.EndsWith("jpg"))
                {
                    BitmapImage queryImage = new BitmapImage();
                    IRandomAccessStream fileStream = await file.OpenAsync(Windows.Storage.FileAccessMode.Read);
                    //BitmapImage bitmapImage = new BitmapImage();
                    queryImage.SetSource(fileStream.AsStream());
                    QueryImagePic.Source = queryImage;
                }
            }
            else
            {
                //displayfilepath.Text = "Operation cancelled.";
            }
        }
        
        public BitmapImage ByteArrayToBitmapImage(byte[] byteArray)
        {
            BitmapImage bitmapImage = new BitmapImage();
            
                using (MemoryStream ms = new MemoryStream(byteArray))
                {
                    bitmapImage.SetSource(ms);
                    return bitmapImage;
                }
            
           }

        public static byte[] ImageToBytes(BitmapImage img)
        {
            using (MemoryStream ms = new MemoryStream())
            {
                WriteableBitmap btmMap = new WriteableBitmap(img);
                System.Windows.Media.Imaging.Extensions.SaveJpeg(btmMap, ms, img.PixelWidth, img.PixelHeight, 0, 100);
                img = null;
                return ms.ToArray();
            }
        }

        public async void queryImage(byte[] image)
        {
            QueryImageInfo queryImageInfo = new QueryImageInfo();
            queryImageInfo.image = image;

            HttpClient client = new HttpClient();
            client.DefaultRequestHeaders.Accept.Add(new HttpMediaTypeWithQualityHeaderValue("application/json"));

            HttpStringContent content = new HttpStringContent(
                                                        JsonConvert.SerializeObject(queryImageInfo),
                                                        UnicodeEncoding.Utf8,
                                                        "text/json");

            HttpResponseMessage response = await client.PostAsync(
                                                        new Uri("http://192.168.1.116:49415/api/LeafAnalysis"),
                                                        content);
            if (response.IsSuccessStatusCode)
            {
                string data = response.Content.ReadAsStringAsync().GetResults();
                ResultImages result = JsonConvert.DeserializeObject<ResultImages>(data);
                this.resultCategory.Text = result.topResultsCategory.First();
                this.resultCategory1.Text = result.topResultsCategory[1];
                this.resultCategory2.Text = result.topResultsCategory[2];
            }
            else
            {
                throw new Exception(response.StatusCode.ToString());
            }
        }
    }
}