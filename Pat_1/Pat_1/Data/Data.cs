using System.Collections.Generic;
using Windows.Storage;

namespace Pat_1.Data
{
    public static class Data
    {
        static public int CurrentlySelectedImageId = 0;
        static public bool IsDataLoaded = false;

        static public IReadOnlyList<StorageFile> PhotoList;
        static public bool CameraIsTurnOn = false;
    }
}
