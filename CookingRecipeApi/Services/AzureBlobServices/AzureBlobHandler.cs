using Azure.Storage.Blobs;
using CookingRecipeApi.Configs;
using System.Text.Json;

namespace CookingRecipeApi.Services.AzureBlobServices
{
    public class AzureBlobHandler
    {
        private readonly BlobServiceClient _blobServiceClient;
        private readonly BlobContainerClient _blobContainerClient;
        public AzureBlobHandler(AzureBlobConfigs azureBlobConfigs)
        {
            _blobServiceClient = azureBlobConfigs.BlobServiceClient;
            _blobContainerClient = azureBlobConfigs.BlobContainerClient;
        }

        public async Task<List<string>> UploadMultipleBlobs(List<IFormFile> files)
        {
            List<string> blobUrls = new List<string>();
            foreach (var file in files)
            {
                var fileguid = Guid.NewGuid().ToString();
                var fileName = $"{fileguid}.{file.FileName.Split('.').Last()}";
                var blobClient = _blobContainerClient.GetBlobClient(fileName);
                using (var stream = file.OpenReadStream())
                {
                    await blobClient.UploadAsync(stream, true);
                }
                blobUrls.Add(blobClient.Uri.AbsoluteUri);
            }
            return blobUrls;
        }
        public async Task<string?> UploadSingleBlob(IFormFile file)
        {
            var fileguid = Guid.NewGuid().ToString();
            var fileName = $"{fileguid}.{file.FileName.Split('.').Last()}"; // concatenate guid with file extension
            var blobClient = _blobContainerClient.GetBlobClient(fileName);
            using (var stream = file.OpenReadStream())
            {
               var result = await blobClient.UploadAsync(stream, true);
                Console.WriteLine(JsonSerializer.Serialize(result));
            }
            return blobClient.Uri.AbsoluteUri;
        }
        public async Task<bool> DeleteBlob(string blobUrl)
        {
            var blobName = blobUrl.Split('/').Last();
            var blobClient = _blobContainerClient.GetBlobClient(blobName);
            return await blobClient.DeleteIfExistsAsync();
        }
        public async Task<bool> DeleteMultipleBlobs(IEnumerable<string> blobUrls)
        {
            try
            {
                Task[] tasks = new Task[blobUrls.Count()];
                for (int i = 0; i < blobUrls.Count(); i++)
                {
                    var blobName = blobUrls.ElementAt(i).Split('/').Last();
                    var blobClient = _blobContainerClient.GetBlobClient(blobName);
                    tasks[i] = blobClient.DeleteIfExistsAsync();
                }
                await Task.WhenAll(tasks);
                return true;
            }
            catch(Exception ex)
            {
                Console.WriteLine("Error deleting blobs: " + ex.Message);
                return false;
            }
        }
    }
}
