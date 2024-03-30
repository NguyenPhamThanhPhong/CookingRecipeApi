using Azure.Storage.Blobs;

namespace CookingRecipeApi.Configs
{
#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.

    public class AzureBlobConfigs
    {
        public string SAStoken { get; set; }
        public string SASUrl { get; set; }
        public string ContainerName { get; set; }
        public BlobServiceClient BlobServiceClient { get; set; }
        public BlobContainerClient BlobContainerClient { get; set; }
        public void Initialize()
        {
            this.BlobServiceClient = new BlobServiceClient(new Uri(this.SASUrl));
            this.BlobContainerClient = this.BlobServiceClient.GetBlobContainerClient(this.ContainerName);
        }
    }
}
