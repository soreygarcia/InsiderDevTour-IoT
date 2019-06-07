using InsiderDevTour.Sensors.Constants;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Auth;
using Microsoft.WindowsAzure.Storage.Blob;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Media.Capture;
using Windows.Media.MediaProperties;
using Windows.Storage;

namespace InsiderDevTour.Sensors.Services
{
    public static class Webcam
    {
        private static MediaCapture mediaCapture;

        private static StorageFolder localFolder = ApplicationData.Current.LocalFolder;

        public static async Task<StorageFile> TakePhoto()
        {
            // initialize webcam ready for use
            mediaCapture = new MediaCapture();
            await mediaCapture.InitializeAsync();

            // create a new photo file in the device's Pictures directory
            StorageFile photoFile = await localFolder.CreateFileAsync("photo.jpg", CreationCollisionOption.GenerateUniqueName);
            // set the file type as a JPEG
            ImageEncodingProperties imageProperties = ImageEncodingProperties.CreateJpeg();

            // capture and store the JPEG photo from the webcam
            await mediaCapture.CapturePhotoToStorageFileAsync(imageProperties, photoFile);

            return photoFile;
        }

        public static async Task<string> TakePhotoAndUploadToTeBlobStorage()
        {
            // initialize webcam ready for use
            mediaCapture = new MediaCapture();
            await mediaCapture.InitializeAsync();

            // create a new photo file in the device's Pictures directory
            StorageFile photoFile = await localFolder.CreateFileAsync("photo.jpg", CreationCollisionOption.GenerateUniqueName);
            // set the file type as a JPEG
            ImageEncodingProperties imageProperties = ImageEncodingProperties.CreateJpeg();

            // capture and store the JPEG photo from the webcam
            await mediaCapture.CapturePhotoToStorageFileAsync(imageProperties, photoFile);

            string photoGuid = Guid.NewGuid().ToString() + ".jpg";

            using (FileStream photoStream = new FileStream(photoFile.Path, FileMode.Open))
            {
                await UploadFileToStorage(photoStream, photoGuid);
            }

            return "https://insiderdevtourmed.blob.core.windows.net/justme/" + photoGuid;
        }

        public static async Task<bool> UploadFileToStorage(Stream fileStream, string fileName)
        {
            // Create storagecredentials object by reading the values from the configuration (appsettings.json)
            StorageCredentials storageCredentials = new StorageCredentials(AzureCredentials.AccountName, AzureCredentials.AccountKey);

            // Create cloudstorage account by passing the storagecredentials
            CloudStorageAccount storageAccount = new CloudStorageAccount(storageCredentials, true);

            // Create the blob client.
            CloudBlobClient blobClient = storageAccount.CreateCloudBlobClient();

            // Get reference to the blob container by passing the name by reading the value from the configuration (appsettings.json)
            CloudBlobContainer container = blobClient.GetContainerReference(AzureCredentials.ImageContainer);

            // Get the reference to the block blob from the container
            CloudBlockBlob blockBlob = container.GetBlockBlobReference(fileName);

            // Upload the file
            await blockBlob.UploadFromStreamAsync(fileStream);

            return await Task.FromResult(true);
        }
    }
}
