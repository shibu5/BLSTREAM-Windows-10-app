using Pat_1.Pomocnicze;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using Windows.ApplicationModel.DataTransfer;
using Windows.ApplicationModel.Email;
using Windows.Foundation;
using Windows.Media.Capture;
using Windows.Media.MediaProperties;
using Windows.Storage;
using Windows.Storage.FileProperties;
using Windows.Storage.Pickers;
using Windows.Storage.Streams;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media.Imaging;

namespace Pat_1.ViewModel
{
    public class ViewModelMain : ViewModelBase
    {
        public ICommand CameraViewCommand { get; set; }
        public ICommand ImageListCommand { get; set; }
        public ICommand ShareCommand { get; set; }
        public ICommand TheTapCommand { get; set; }

        private CaptureElement camera_source;
        private MediaCapture takephotoManager;

        public ViewModelMain(ref CaptureElement ce)
        {
            camera_source = ce;
            takephotoManager = new MediaCapture();

            var task = CameraTest(); 

            CameraViewCommand = new Pomocnicze.RelayCommand(pars => CameraViewButton());
            ImageListCommand = new Pomocnicze.RelayCommand(pars => ImageListButton());
            ShareCommand = new Pomocnicze.RelayCommand(pars => ShareButton());
            TheTapCommand = new Pomocnicze.RelayCommand(pars => NastepnyObrazek());

            var wczytywanie_obr = wczytywanie_obrazka();
            Windows.UI.Core.SystemNavigationManager.GetForCurrentView().BackRequested +=App_BackRequested;

            DataTransferManager dataTransferManager = DataTransferManager.GetForCurrentView();
            dataTransferManager.DataRequested += new TypedEventHandler<DataTransferManager, DataRequestedEventArgs>(this.ShareImageHandler);

            CameraButtonName = "Camera View";
            ImageListButtonName = "Images List";
            ShareButtonName = "Share";

            ShareButtonIsEnable = true;
        }

        private void ShareButton()
        {
            DataTransferManager.ShowShareUI();
        }

        private void CameraViewButton()
        {
            var task = CameraViewButton_async();
        }

        private async Task CameraTest()
        {
            try
            {
                takephotoManager = new MediaCapture();
                await takephotoManager.InitializeAsync();
                camera_source.Source = takephotoManager;
                CameraButtonName = "Camera View";
                CameraButtonIsEnable = true;
            }
            catch
            {
                CameraButtonName = "Camera not found";
                CameraButtonIsEnable = false;
            }
        }

        private async Task CameraViewButton_async()
        {
            if (Data.Data.CameraTurnOn == false)
            {
                try
                {
                    takephotoManager = new MediaCapture();
                    await takephotoManager.InitializeAsync();
                    camera_source.Source = takephotoManager;
                    await takephotoManager.StartPreviewAsync();
                    ImageListButtonName = "Close Camera";
                    Data.Data.CameraTurnOn = true;
                    CameraButtonName = "Take a foto";
                    ShareButtonIsEnable = false;
                }
                catch
                {
                    ImageListButtonName = "Images List";
                    ShareButtonIsEnable = true;
                }
            }
            else
            {

                ImageEncodingProperties imgFormat = ImageEncodingProperties.CreateJpeg();

                StorageFile file;
                int i = 0;
                while (true)
                {
                    try
                    {
                        file = await Windows.Storage.KnownFolders.PicturesLibrary.CreateFileAsync
                             ("Photo" + i.ToString() + ".jpg", CreationCollisionOption.FailIfExists);
                        break;
                    }
                    catch
                    {
                        i++;
                        continue;
                    }
                }

                await takephotoManager.CapturePhotoToStorageFileAsync(imgFormat, file);
                await takephotoManager.StopPreviewAsync();

                CameraButtonName = "Camera View";
                ImageListButtonName = "Images List";
                Data.Data.CameraTurnOn = false;
                ShareButtonIsEnable = true;

                Data.Data.czy_wczytano_dane = false;
                var task = wczytywanie_obrazka();

            }
        }

        private async void ImageListButton()
        {
            if (Data.Data.CameraTurnOn == false)
            {
                Data.Data.ktory_obrazek_zaznaczony = -1;
                Frame rootFrame = Window.Current.Content as Frame;
                rootFrame.Navigate(typeof(View.ImageList));
            }
            else
            {
                await takephotoManager.StopPreviewAsync();

                CameraButtonName = "Camera View";
                ImageListButtonName = "Images List";
                Data.Data.CameraTurnOn = false;
                ShareButtonIsEnable = true;
            }
        }

        public void NastepnyObrazek()
        {
            Data.Data.ktory_obrazek_zaznaczony++;
            var wczytywanie_obr = wczytywanie_obrazka();
        }

        private void ShareImageHandler(DataTransferManager sender, DataRequestedEventArgs e)
        {
            DataRequest request = e.Request;
            request.Data.Properties.Title = "Share Image";

            DataRequestDeferral deferral = request.GetDeferral();

            try
            {
                StorageFile thumbnailFile = Data.Data.fileList[Data.Data.ktory_obrazek_zaznaczony];
                request.Data.Properties.Thumbnail =
                    RandomAccessStreamReference.CreateFromFile(thumbnailFile);
                StorageFile imageFile = Data.Data.fileList[Data.Data.ktory_obrazek_zaznaczony];
                request.Data.SetBitmap(RandomAccessStreamReference.CreateFromFile(imageFile));
            }
            finally
            {
                deferral.Complete();
            }
        }

        private async Task wczytywanie_obrazka()
        {

            var bounds = Window.Current.Bounds;
            

            StorageFolder pictures_folder = Windows.Storage.KnownFolders.PicturesLibrary;

            IReadOnlyList<StorageFile> fileList =
                await pictures_folder.GetFilesAsync();

            if(fileList.Count > 0)
            {
                if (Data.Data.ktory_obrazek_zaznaczony > fileList.Count - 1 || Data.Data.ktory_obrazek_zaznaczony == -1 )
                    Data.Data.ktory_obrazek_zaznaczony = 0;

                ImageProperties metadata_obrazka = await fileList[Data.Data.ktory_obrazek_zaznaczony].Properties.GetImagePropertiesAsync();
                
                using (IRandomAccessStream fileStream = await fileList[Data.Data.ktory_obrazek_zaznaczony].OpenAsync(Windows.Storage.FileAccessMode.Read))
                {
                    BitmapImage bitmapImage = new BitmapImage();

                    await bitmapImage.SetSourceAsync(fileStream);
                    adres_img = bitmapImage;

                    text = "Nazwa obrazka: " + fileList[Data.Data.ktory_obrazek_zaznaczony].DisplayName + Environment.NewLine +
                           "Data stworzenia: " + metadata_obrazka.DateTaken;
                }

                var props = await fileList[Data.Data.ktory_obrazek_zaznaczony].Properties.RetrievePropertiesAsync(null);

                if (Data.Data.czy_wczytano_dane == false)
                {
                    Data.Data.fileList = fileList;
                    Data.Data.czy_wczytano_dane = true;
                }

                try
                {
                    foreach (var prop in props)
                    {

                        if (prop.Key == "System.GPS.LongitudeNumerator" && prop.Value.GetType().IsArray)
                        {

                            try
                            {
                                uint[] res = prop.Value as uint[];
                                text += Environment.NewLine + "System.GPS.LongitudeNumerator: " + res[0] + ", " + res[1] + ", " + res[2];
                            }
                            catch
                            {

                            }

                        }
                        else if (prop.Key == "System.GPS.LongitudeDenominator" && prop.Value.GetType().IsArray)
                        {
                            try
                            {
                                uint[] res = prop.Value as uint[];
                                text += Environment.NewLine + "System.GPS.LongitudeDenominator: " + res[0] + ", " + res[1] + ", " + res[2] + " Cos zle?";
                            }
                            catch
                            {

                            }
                        }
                        else if (prop.Key == "System.GPS.Altitude" && prop.Value.ToString() != "")
                        {
                            try
                            {
                                text += Environment.NewLine + "System.GPS.Altitude: " + prop.Value.ToString();
                            }
                            catch
                            {

                            }
                        }

                    }
                }
                catch
                {

                }
            }

        }

        private void App_BackRequested(object sender, Windows.UI.Core.BackRequestedEventArgs e)
        {
            Frame rootFrame = Window.Current.Content as Frame;
            if (rootFrame == null)
                return;
            
            if (rootFrame.CanGoBack && e.Handled == false)
            {
                e.Handled = true;
                rootFrame.GoBack();
            }
        }



        BitmapImage _adres_img;
        public BitmapImage adres_img
        {
            get
            {
                return _adres_img;
            }
            set
            {
                if (_adres_img != value)
                {
                    _adres_img = value;
                    PropertyChangedUpdate("adres_img");
                }
            }
        }        

        string _text;
        public string text
        {
            get
            {
                return _text;
            }
            set
            {
                if (_text != value)
                {
                    _text = value;
                    PropertyChangedUpdate("text");
                }
            }
        }

        string _CameraButtonName;
        public string CameraButtonName
        {
            get
            {
                return _CameraButtonName;
            }
            set
            {
                if (_CameraButtonName != value)
                {
                    _CameraButtonName = value;
                    PropertyChangedUpdate("CameraButtonName");
                }
            }
        }

        string _ImageListButtonName;
        public string ImageListButtonName
        {
            get
            {
                return _ImageListButtonName;
            }
            set
            {
                if (_ImageListButtonName != value)
                {
                    _ImageListButtonName = value;
                    PropertyChangedUpdate("ImageListButtonName");
                }
            }
        }

        string _ShareButtonName;
        public string ShareButtonName
        {
            get
            {
                return _ShareButtonName;
            }
            set
            {
                if (_ShareButtonName != value)
                {
                    _ShareButtonName = value;
                    PropertyChangedUpdate("ShareButtonName");
                }
            }
        }
        
        bool _CameraButtonIsEnable;
        public bool CameraButtonIsEnable
        {
            get
            {
                return _CameraButtonIsEnable;
            }
            set
            {
                if (_CameraButtonIsEnable != value)
                {
                    _CameraButtonIsEnable = value;
                    PropertyChangedUpdate("CameraButtonIsEnable");
                }
            }
        }

        bool _ShareButtonIsEnable;
        public bool ShareButtonIsEnable
        {
            get
            {
                return _ShareButtonIsEnable;
            }
            set
            {
                if (_ShareButtonIsEnable != value)
                {
                    _ShareButtonIsEnable = value;
                    PropertyChangedUpdate("ShareButtonIsEnable");
                }
            }
        }
        
    }
}
