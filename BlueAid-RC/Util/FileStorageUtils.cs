using BlueAid_RC.Model;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.ApplicationModel;
using Windows.Storage;

namespace BlueAid_RC.Util
{
    public sealed class FileStorageUtils
    {
        private static FileStorageUtils _instance = null;
        private static readonly object padlock = new object();
        private static StorageFolder _captureFolder = null;
        private readonly string VIDEO_ROOT_PATH = "video"; 

        private FileStorageUtils()
        {
        }
        public static FileStorageUtils GetInstance
        {
            get
            {
                lock (padlock)
                {
                    if (_instance == null)
                    {
                        _instance = new FileStorageUtils();
                    }
                    return _instance;
                }
            }
        }
        public async void Init()
        {
            StorageLibrary picturesLibrary = await StorageLibrary.GetLibraryAsync(KnownLibraryId.Pictures);
            // Fall back to the local app storage if the Pictures Library is not available
            _captureFolder = picturesLibrary.SaveFolder ?? ApplicationData.Current.LocalFolder;
        }

        public async void SaveVideoFile(string captureName)
        {
            var videoFile = await _captureFolder.CreateFileAsync(captureName, CreationCollisionOption.ReplaceExisting);
        }

        public async Task<bool> ExistUserFile(User user)
        {
            string newUser = user.userName + "_" + user.userNumber;
            string videoPath = Path.Combine(_captureFolder.Path, VIDEO_ROOT_PATH);
            try
            {
                StorageFolder parentFolder = Package.Current.InstalledLocation;
                StorageFolder childFolder = await parentFolder.GetFolderAsync("Assets");

                StorageFolder storageFolder = await _captureFolder.GetFolderAsync(VIDEO_ROOT_PATH);
                IReadOnlyList<StorageFolder> storageFolders = await storageFolder.GetFoldersAsync();
                foreach (var item in storageFolders)
                {
                    if (item.Name.Equals(newUser))
                    {
                        Debug.WriteLine("중복된 사용자 입니다.");
                        return true;
                    }
                    Debug.WriteLine(item.Name);
                }
            }
            catch (Exception ex)
            {

                Debug.WriteLine(ex.ToString());
            }
            
            return false;
        }
    }
}
