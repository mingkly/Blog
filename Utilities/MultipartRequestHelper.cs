using Microsoft.Net.Http.Headers;
using System;
using System.IO;

namespace MyWeb.Utilities
{
    public static class MultipartRequestHelper
    {
        public static string GetBoundary(MediaTypeHeaderValue contentType, int lengthLimit)
        {
            var boundary = HeaderUtilities.RemoveQuotes(contentType.Boundary).Value;
            if (string.IsNullOrEmpty(boundary))
            {
                throw new InvalidDataException("content has no boundary");
            }
            if (boundary.Length > lengthLimit)
            {
                throw new InvalidDataException($"multipart request boundary limit {lengthLimit} excced");
            }
            return boundary;
        }
        public static bool IsMultipart(string contentType)
        {
            return !string.IsNullOrEmpty(contentType) && contentType.IndexOf("multipart/", StringComparison.OrdinalIgnoreCase) >= 0;
        }
        public static bool HasFormDataDispostion(ContentDispositionHeaderValue contentDisposition)
        {
            return contentDisposition != null
                && contentDisposition.DispositionType.Value == "form-data"
                && string.IsNullOrEmpty(contentDisposition.FileName.Value)
                && string.IsNullOrEmpty(contentDisposition.FileNameStar.Value);
        }
        public static bool HasFileDispostion(ContentDispositionHeaderValue contentDisposition)
        {
            return contentDisposition != null
                && contentDisposition.DispositionType.Value == "form-data"
                && (!string.IsNullOrEmpty(contentDisposition.FileName.Value)
                || !string.IsNullOrEmpty(contentDisposition.FileNameStar.Value));
        }
    }

}
