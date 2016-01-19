using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Storage.Streams;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media.Imaging;

namespace Pat_1.ViewModel
{
    public class ViewModel_ImageList : ViewModelBase
    {

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

        /*#region Binding szerokosci i wysokosci tabelki z obrazkami 
        private int wysokosc1_;
        public int wysokosc1
        {
            get { return wysokosc1_; }
            set
            {
                if (wysokosc1_ != value)
                {
                    wysokosc1_ = value;
                    PropertyChangedUpdate("wysokosc1");
                }
            }
        }

        private int szerokosc1_;
        public int szerokosc1
        {
            get { return szerokosc1_; }
            set
            {
                if (szerokosc1_ != value)
                {
                    szerokosc1_ = value;
                    PropertyChangedUpdate("szerokosc1");
                }
            }
        }

        private int szerokosc2_;
        public int szerokosc2
        {
            get { return szerokosc2_; }
            set
            {
                if (szerokosc2_ != value)
                {
                    szerokosc2_ = value;
                    PropertyChangedUpdate("szerokosc2");
                }
            }
        }

#endregion*/

        public void wybrano_obrazek()
        {
            Data.Data.ktory_obrazek_zaznaczony = selecteditem.id;
        }

        public ViewModel_ImageList()
        {

            lista = new ObservableCollection<Model.lista_klasa>();

            if (Data.Data.fileList.Count > 0)
            {

                var wczytywanie_o = wczytywanie_obrazkow();
                
            }

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

    }
}
