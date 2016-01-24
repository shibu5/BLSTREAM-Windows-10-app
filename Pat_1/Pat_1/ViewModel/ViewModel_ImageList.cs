using System;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using System.Windows.Input;
using Windows.Storage.Streams;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media.Imaging;

namespace Pat_1.ViewModel
{
    public class ViewModel_ImageList : ViewModelBase
    {

        public ICommand ImagesListSelectionChangedCommand { get; set; }

        public ViewModel_ImageList()
        {

            lista = new ObservableCollection<Model.ClassList>();

            if (Data.Data.PhotoList.Count > 0)
            {

                var wczytywanie_o = ImagesLoading_task();

            }

            ImagesListSelectionChangedCommand = new Pomocnicze.RelayCommand(pars => NextPicture());

        }

        public void NextPicture()
        {
            if (selecteditem.id != Data.Data.CurrentlySelectedImageId)
            {
                Data.Data.CurrentlySelectedImageId = selecteditem.id;
                Frame rootFrame = Window.Current.Content as Frame;
                rootFrame.Navigate(typeof(MainPage));
            }
        }

        private async Task ImagesLoading_task()
        {

            int i = 0;
            foreach (var x in Data.Data.PhotoList)
            {
                using (IRandomAccessStream fileStream = await x.OpenAsync(Windows.Storage.FileAccessMode.Read))
                {

                    BitmapImage bitmapImage = new BitmapImage();

                    await bitmapImage.SetSourceAsync(fileStream);

                    lista.Add(new Model.ClassList { Nazwa = x.Name, Image = bitmapImage, id = i });

                }
                i++;
            }
        }

        private Model.ClassList _selecteditem;
        public Model.ClassList selecteditem
        {
            get { return _selecteditem; }
            set
            {
                if (_selecteditem != value)
                {
                    _selecteditem = value;
                    PropertyChangedUpdate("selecteditem");
                }
            }
        }

        private ObservableCollection<Model.ClassList> _lista;
        public ObservableCollection<Model.ClassList> lista
        {
            get { return _lista; }
            set
            {
                if (_lista != value)
                {
                    _lista = value;
                    PropertyChangedUpdate("lista");
                }
            }
        }

    }
}
