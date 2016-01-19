using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.ApplicationModel;
using Windows.ApplicationModel.DataTransfer;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Media.Capture;
using Windows.Media.MediaProperties;
using Windows.Storage;
using Windows.Storage.Streams;
using Windows.UI.Core;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

// 空白ページのアイテム テンプレートについては、http://go.microsoft.com/fwlink/?LinkId=402352&clcid=0x409 を参照してください

namespace Pat_1
{
    /// <summary>
    /// それ自体で使用できる空白ページまたはフレーム内に移動できる空白ページ。
    /// </summary>
    public sealed partial class MainPage : Page
    {
        public MainPage()
        {

            this.InitializeComponent();
            this.DataContext = new ViewModel.ViewModelMain();

        }

        public ViewModel.ViewModelMain ViewModel
        {
            get { return DataContext as ViewModel.ViewModelMain; }
        }

        private void Image_Tapped(object sender, TappedRoutedEventArgs e)
        {
            ViewModel.NastepnyObrazek();
        }

        async private void Button_Click(object sender, RoutedEventArgs e)
        {
            if (Data.Data.CameraTurnOn == false)
            {
                try
                {
                    Data.Data._MediaCapture = new MediaCapture();
                    await Data.Data._MediaCapture.InitializeAsync();
                    kamera.Source = Data.Data._MediaCapture;
                    await Data.Data._MediaCapture.StartPreviewAsync();
                    ImageListButton.Content = "Close Camera";
                    Data.Data.CameraTurnOn = true;
                    KameraButton.Content = "Take a foto";
                }
                catch
                {
                    ImageListButton.Content = "Images List";
                }
            }
            else
            {
                kamera.Source = null;
                MediaCapture takephotoManager = new MediaCapture();
                await takephotoManager.InitializeAsync();

                ImageEncodingProperties imgFormat = ImageEncodingProperties.CreateJpeg();

                StorageFile file;
                int i = 0;
                while (true)
                {
                    try
                    {
                        file = await Windows.Storage.KnownFolders.PicturesLibrary.CreateFileAsync
                             ("Photo" + i.ToString() +".jpg", CreationCollisionOption.FailIfExists);
                        break;
                    }
                    catch
                    {
                        i++;
                        continue;
                    }
                }

                var task = ViewModel.reflesh_po_zdjeciu();

                await takephotoManager.CapturePhotoToStorageFileAsync(imgFormat, file);

                KameraButton.Content = "Camera View";
                ImageListButton.Content = "Images List";
                Data.Data.CameraTurnOn = false;
            }
        }

        async private void Button_Click_1(object sender, RoutedEventArgs e)
        {
            if(Data.Data.CameraTurnOn == false)
                this.Frame.Navigate(typeof(View.ImageList));
            else
            {
                kamera.Source = null;
                await Data.Data._MediaCapture.StopPreviewAsync();

                KameraButton.Content = "Camera View";
                ImageListButton.Content = "Images List";
                Data.Data.CameraTurnOn = false;
            }
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            Frame rootFrame = Window.Current.Content as Frame;

            string myPages = "";
            foreach (PageStackEntry page in rootFrame.BackStack)
            {
                myPages += page.SourcePageType.ToString() + "\n";
            }

            if (rootFrame.CanGoBack)
            {
                
                SystemNavigationManager.GetForCurrentView().AppViewBackButtonVisibility =
                    AppViewBackButtonVisibility.Visible;
            }
            else
            {
                
                SystemNavigationManager.GetForCurrentView().AppViewBackButtonVisibility =
                    AppViewBackButtonVisibility.Collapsed;
            }
        }

        private async void Button_Loaded(object sender, RoutedEventArgs e)
        {
            try
            {
                Data.Data._MediaCapture = new MediaCapture();
                await Data.Data._MediaCapture.InitializeAsync();
                kamera.Source = Data.Data._MediaCapture;
                KameraButton.Content = "Camera View";
                KameraButton.IsEnabled = true;
            }
            catch
            {
                KameraButton.Content = "Can't find camera";
                KameraButton.IsEnabled = false;
            }
        }

        private void Share(object sender, RoutedEventArgs e)
        {
            DataTransferManager.ShowShareUI();
        }

        private void main_grid_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            ViewModel.resolution_update();
        }
    }
}
