using Azure.Storage.Blobs;
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Cxunicorn.Common.Services.Blob
{
    public interface IBaseBlob
    {
        Task DeleteWithUriAsync(string uri);
        Uri GetServiceSasUriForBlob(BlobClient blobClient, int validForHours = 9000);
        Task<Uri> UploadAsync(IFormFile file, string? fileName = null, string? contentType = null);
        Uri? GetNewUri(string fileName, string? contentType = null);
        Task DeleteWithNameAsync(string name);


    }
}
