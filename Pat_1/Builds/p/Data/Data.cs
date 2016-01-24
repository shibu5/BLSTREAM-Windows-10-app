using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Media.Capture;
using Windows.Storage;

namespace Pat_1.Data
{
    public static class Data
    {
        static public int ktory_obrazek_zaznaczony = 0;
        static public bool czy_wczytano_dane = false;

        static public IReadOnlyList<StorageFile> fileList;
        static public MediaCapture _MediaCapture;
        static public bool CameraTurnOn = false;
    }
}
