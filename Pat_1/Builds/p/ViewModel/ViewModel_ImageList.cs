using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
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

        public ICommand TheTapCommand { get; set; }

        public ViewModel_ImageList()
        {

            lista = new ObservableCollection<Model.lista_klasa>();

            if (Data.Data.fileList.Count > 0)
            {

                var wczytywanie_o = wczytywanie_obrazkow();
                
            }

            TheTapCommand = new Pomocnicze.RelayCommand(pars => wybrano_obrazek());

        }

        private async Task wczytywanie_obrazkow()
        {
            var bounds = Window.Current.Bounds;

            int i = 0;
            foreach (var x in Data.Data.fileList)
            {
                using (IRandomAccessStream fileStream = await x.OpenAsync(Windows.Storage.FileAccessMode.Read))
                {

                    BitmapImage bitmapImage = new BitmapImage();

                    await bitmapImage.SetSourceAsync(fileStream);

                    int wysokosc1 = Convert.ToInt32(bounds.Height * 0.25);

                    int szerokosc1 = Convert.ToInt32(bounds.Width * 0.4);
                    int szerokosc2 = Convert.ToInt32(bounds.Width * 0.55);

                    lista.Add(new Model.lista_klasa { Nazwa = x.Name, Image = bitmapImage, id = i, wysokosc1 = wysokosc1, szerokosc1 = szerokosc1, szerokosc2 = szerokosc2 });

                }
                i++;
            }
        }

        public void wybrano_obrazek()
        {
            if (selecteditem.id != Data.Data.ktory_obrazek_zaznaczony)
            {
                Data.Data.ktory_obrazek_zaznaczony = selecteditem.id;
                Frame rootFrame = Window.Current.Content as Frame;
                rootFrame.Navigate(typeof(MainPage));
            }
        }

        private Model.lista_klasa _selecteditem;
        public Model.lista_klasa selecteditem
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

        private ObservableCollection<Model.lista_klasa> lista_;
        public ObservableCollection<Model.lista_klasa> lista
        {
            get { return lista_; }
            set
            {
                if (lista_ != value)
                {
                    lista_ = value;
                    PropertyChangedUpdate("lista");
                }
            }
        }

    }
}
