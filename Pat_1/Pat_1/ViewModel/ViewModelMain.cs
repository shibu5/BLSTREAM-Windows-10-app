using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.ApplicationModel.DataTransfer;
using Windows.ApplicationModel.Email;
using Windows.Foundation;
using Windows.Media.Capture;
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

        public ViewModelMain()
        {
            resolution_update();
            var wczytywanie_obr = wczytywanie_obrazka();
            Windows.UI.Core.SystemNavigationManager.GetForCurrentView().BackRequested +=App_BackRequested;

            DataTransferManager dataTransferManager = DataTransferManager.GetForCurrentView();
            dataTransferManager.DataRequested += new TypedEventHandler<DataTransferManager, DataRequestedEventArgs>(this.ShareImageHandler);

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

        public async Task reflesh_po_zdjeciu()
        {
            StorageFolder pictures_folder = Windows.Storage.KnownFolders.PicturesLibrary;

            Data.Data.fileList = await pictures_folder.GetFilesAsync();

        }

        public void resolution_update()
        {
            var bounds = Window.Current.Bounds;

            picture_height = bounds.Height * 0.65;
            border_height = bounds.Height * 0.15;
            button_height = bounds.Height * 0.1;

            picture_width = bounds.Width * 0.95;
            border_width = bounds.Width * 0.75;
            button_width = bounds.Width * 0.28;
        }

        private async Task wczytywanie_obrazka()
        {

            var bounds = Window.Current.Bounds;
            

            StorageFolder pictures_folder = Windows.Storage.KnownFolders.PicturesLibrary;

            IReadOnlyList<StorageFile> fileList =
                await pictures_folder.GetFilesAsync();

            if(fileList.Count > 0)
            {
                if (Data.Data.ktory_obrazek_zaznaczony > fileList.Count - 1 )
                    Data.Data.ktory_obrazek_zaznaczony = 0;

                ImageProperties metadata_obrazka = await fileList[Data.Data.ktory_obrazek_zaznaczony].Properties.GetImagePropertiesAsync();
                
                using (IRandomAccessStream fileStream = await fileList[Data.Data.ktory_obrazek_zaznaczony].OpenAsync(Windows.Storage.FileAccessMode.Read))
                {
                    BitmapImage bitmapImage = new BitmapImage();

                    await bitmapImage.SetSourceAsync(fileStream);
                    adres_img = bitmapImage;

                    picture_height = bounds.Height * 0.65;
                    border_height = bounds.Height * 0.15;
                    button_height = bounds.Height * 0.1;

                    picture_width = bounds.Width * 0.95;
                    border_width = bounds.Width * 0.75;
                    button_width = bounds.Width * 0.2;

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

            // Navigate back if possible, and if the event has not 
            // already been handled .
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

        double _picture_height;
        public double picture_height
        {
            get
            {
                return _picture_height;
            }
            set
            {
                if (_picture_height != value)
                {
                    _picture_height = value;
                    PropertyChangedUpdate("picture_height");
                }
            }
        }

        double _picture_width;
        public double picture_width
        {
            get
            {
                return _picture_width;
            }
            set
            {
                if (_picture_width != value)
                {
                    _picture_width = value;
                    PropertyChangedUpdate("picture_width");
                }
            }
        }

        double _border_width;
        public double border_width
        {
            get
            {
                return _border_width;
            }
            set
            {
                if (_border_width != value)
                {
                    _border_width = value;
                    PropertyChangedUpdate("border_width");
                }
            }
        }
        
        double _border_height;
        public double border_height
        {
            get
            {
                return _border_height;
            }
            set
            {
                if (_border_height != value)
                {
                    _border_height = value;
                    PropertyChangedUpdate("border_height");
                }
            }
        }

        double _button_width;
        public double button_width
        {
            get
            {
                return _button_width;
            }
            set
            {
                if (_button_width != value)
                {
                    _button_width = value;
                    PropertyChangedUpdate("button_width");
                }
            }
        }

        double _button_height;
        public double button_height
        {
            get
            {
                return _button_height;
            }
            set
            {
                if (_button_height != value)
                {
                    _button_height = value;
                    PropertyChangedUpdate("button_height");
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
        
    }
}
