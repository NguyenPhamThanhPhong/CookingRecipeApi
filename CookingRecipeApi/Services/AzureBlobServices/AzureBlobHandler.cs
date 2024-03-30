using Azure.Storage.Blobs;
using CookingRecipeApi.Configs;

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
                var blobClient = _blobContainerClient.GetBlobClient(fileguid);
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
            var blobClient = _blobContainerClient.GetBlobClient(fileguid);
            using (var stream = file.OpenReadStream())
            {
               await blobClient.UploadAsync(stream, true);
            }
            return blobClient.Uri.AbsoluteUri;
        }
        public async Task<bool> DeleteBlob(string blobName)
        {
            var blobClient = _blobContainerClient.GetBlobClient(blobName);
            return await blobClient.DeleteIfExistsAsync();
        }
    }
}
