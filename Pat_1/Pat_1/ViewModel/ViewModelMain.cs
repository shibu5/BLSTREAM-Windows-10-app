using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Windows.Input;
using Windows.ApplicationModel.DataTransfer;
using Windows.Foundation;
using Windows.Media.Capture;
using Windows.Media.MediaProperties;
using Windows.Storage;
using Windows.Storage.FileProperties;
using Windows.Storage.Streams;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media.Imaging;

namespace Pat_1.ViewModel
{
    public class ViewModelMain : ViewModelBase
    {
        public ICommand CameraViewCommand { get; set; }
        public ICommand ImageListCommand { get; set; }
        public ICommand ShareCommand { get; set; }
        public ICommand PictureTapCommand { get; set; }

        private CaptureElement CameraControl;
        private MediaCapture TakePhotoManager;

        public ViewModelMain(ref CaptureElement cameraControlView)
        {
            CameraControl = cameraControlView;
            TakePhotoManager = new MediaCapture();

            var task = CameraTest_task(); 
            var task2 = PictureLoading_task();

            CameraViewCommand = new Pomocnicze.RelayCommand(pars => CameraViewButton());
            ImageListCommand = new Pomocnicze.RelayCommand(pars => ShowImagesList_async());
            ShareCommand = new Pomocnicze.RelayCommand(pars => ShareButton());
            PictureTapCommand = new Pomocnicze.RelayCommand(pars => SetNextPicture());

            Windows.UI.Core.SystemNavigationManager.GetForCurrentView().BackRequested +=BackRequested;


            DataTransferManager ShareManager = DataTransferManager.GetForCurrentView();
            ShareManager.DataRequested += new TypedEventHandler<DataTransferManager, DataRequestedEventArgs>(this.ShareImageHandler);

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
            var task = CameraViewButton_task();
        }

        private void SetNextPicture()
        {
            Data.Data.CurrentlySelectedImageId++;

            var task = PictureLoading_task();
        }

        private void ShareImageHandler(DataTransferManager sender, DataRequestedEventArgs e)
        {
            DataRequest request = e.Request;
            request.Data.Properties.Title = "Share Image";

            DataRequestDeferral deferral = request.GetDeferral();

            try
            {
                StorageFile thumbnailFile = Data.Data.PhotoList[Data.Data.CurrentlySelectedImageId];
                request.Data.Properties.Thumbnail =
                    RandomAccessStreamReference.CreateFromFile(thumbnailFile);
                StorageFile imageFile = Data.Data.PhotoList[Data.Data.CurrentlySelectedImageId];
                request.Data.SetBitmap(RandomAccessStreamReference.CreateFromFile(imageFile));
            }
            finally
            {
                deferral.Complete();
            }
        }

        private void BackRequested(object sender, Windows.UI.Core.BackRequestedEventArgs e)
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

        private async Task CameraTest_task()
        {
            try
            {
                TakePhotoManager = new MediaCapture();
                await TakePhotoManager.InitializeAsync();
                CameraControl.Source = TakePhotoManager;
                CameraButtonName = "Camera View";
                CameraButtonIsEnable = true;
            }
            catch
            {
                CameraButtonName = "Camera not found";
                CameraButtonIsEnable = false;
            }
        }

        private async Task CameraViewButton_task()
        {
            if (Data.Data.CameraIsTurnOn == false)
            {
                try
                {
                    TakePhotoManager = new MediaCapture();
                    await TakePhotoManager.InitializeAsync();
                    CameraControl.Source = TakePhotoManager;
                    await TakePhotoManager.StartPreviewAsync();
                    ImageListButtonName = "Close Camera";
                    Data.Data.CameraIsTurnOn = true;
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

                await TakePhotoManager.CapturePhotoToStorageFileAsync(imgFormat, file);
                await TakePhotoManager.StopPreviewAsync();

                CameraButtonName = "Camera View";
                ImageListButtonName = "Images List";
                Data.Data.CameraIsTurnOn = false;
                ShareButtonIsEnable = true;

                Data.Data.IsDataLoaded = false;

                var task = PictureLoading_task();

            }
        }

        private async Task PictureLoading_task()
        {

            var bounds = Window.Current.Bounds;


            StorageFolder pictures_folder = Windows.Storage.KnownFolders.PicturesLibrary;

            IReadOnlyList<StorageFile> PhotoList =
                await pictures_folder.GetFilesAsync();

            if (PhotoList.Count > 0)
            {
                if (Data.Data.CurrentlySelectedImageId > PhotoList.Count - 1 || Data.Data.CurrentlySelectedImageId == -1)
                    Data.Data.CurrentlySelectedImageId = 0;

                ImageProperties metadata_obrazka = await PhotoList[Data.Data.CurrentlySelectedImageId].Properties.GetImagePropertiesAsync();

                using (IRandomAccessStream fileStream = await PhotoList[Data.Data.CurrentlySelectedImageId].OpenAsync(Windows.Storage.FileAccessMode.Read))
                {
                    BitmapImage bitmapImage = new BitmapImage();

                    await bitmapImage.SetSourceAsync(fileStream);
                    adres_img = bitmapImage;

                    text = "Nazwa obrazka: " + PhotoList[Data.Data.CurrentlySelectedImageId].DisplayName + Environment.NewLine +
                           "Data stworzenia: " + metadata_obrazka.DateTaken;
                }

                var props = await PhotoList[Data.Data.CurrentlySelectedImageId].Properties.RetrievePropertiesAsync(null);

                if (Data.Data.IsDataLoaded == false)
                {
                    Data.Data.PhotoList = PhotoList;
                    Data.Data.IsDataLoaded = true;
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

        private async void ShowImagesList_async()
        {
            if (Data.Data.CameraIsTurnOn == false)
            {
                Data.Data.CurrentlySelectedImageId = -1;
                Frame rootFrame = Window.Current.Content as Frame;
                rootFrame.Navigate(typeof(View.ImageList));
            }
            else
            {
                await TakePhotoManager.StopPreviewAsync();

                CameraButtonName = "Camera View";
                ImageListButtonName = "Images List";
                Data.Data.CameraIsTurnOn = false;
                ShareButtonIsEnable = true;
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
