using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using Azure.Storage.Blobs.Specialized;
using Azure.Storage.Sas;
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Cxunicorn.Common.Services.Blob
{
    public abstract class BaseBlob : IBaseBlob
    {
        public BlobContainerClient Container { get; }
        public BaseBlob(BlobServiceClient blobClient,
            string containerName)
        {
            this.Container = blobClient.GetBlobContainerClient(containerName);
            this.Container.CreateIfNotExists();
        }

        public async Task DeleteWithUriAsync(string uri)
        {
            BlobClient blob = this.Container.GetBlobClient(Path.GetFileName(uri));
            await blob.DeleteIfExistsAsync();
        }

        public async Task<Uri> UploadAsync(IFormFile file, string? fileName = null, string? contentType = null)
        {
            if (string.IsNullOrWhiteSpace(fileName))
                fileName = file.FileName;

            if (file?.Length > 0)
            {
                using (MemoryStream fs = new())
                {
                    file.CopyTo(fs);
                    fs.Position = 0;
                    await Container.UploadBlobAsync(fileName, fs);
                }

                var blob = Container.GetBlobClient(fileName);

                //set content type, please test it.
                if(!string.IsNullOrWhiteSpace(contentType))
                {
                    BlobHttpHeaders blobHttpHeaders = new();
                    blobHttpHeaders.ContentType = contentType!;
                    blob.SetHttpHeaders(blobHttpHeaders);
                }
                return GetServiceSasUriForBlob(blob);
            }
            else throw new ArgumentNullException(nameof(file));
        }

        public Uri GetServiceSasUriForBlob(BlobClient blobClient, int validForHours = 9000)
        {
            if (blobClient.CanGenerateSasUri)
            {
                BlobSasBuilder sasBuilder = new BlobSasBuilder()
                {
                    BlobContainerName = blobClient.GetParentBlobContainerClient().Name,
                    BlobName = blobClient.Name,
                    Resource = "b"
                };

                sasBuilder.ExpiresOn = DateTimeOffset.UtcNow.AddHours(validForHours);
                sasBuilder.SetPermissions(BlobSasPermissions.Read);

                return blobClient.GenerateSasUri(sasBuilder);
            }
            else
            {
                throw new Exception("BlobClient must be authorized with Shared Key credentials to create a service SAS.");
            }
        }

        public Uri? GetNewUri(string fileName, string? contentType = null)
        {
            var blob = Container.GetBlobClient(fileName);

            if (blob == null)
                return null;

            if (!string.IsNullOrWhiteSpace(contentType))
            {
                BlobHttpHeaders blobHttpHeaders = new();
                blobHttpHeaders.ContentType = contentType!;
                blob.SetHttpHeaders(blobHttpHeaders);
            }

            return GetServiceSasUriForBlob(blob);
        }

        public async Task DeleteWithNameAsync(string name)
        {
            BlobClient blob = Container.GetBlobClient(name);
            await blob.DeleteIfExistsAsync();
        }
    }
}
