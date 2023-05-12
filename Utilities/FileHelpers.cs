using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Net.Http.Headers;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.IO;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Threading.Tasks;

namespace MyWeb.Utilities
{
    public static class FileHelpers
    {
        private static readonly Dictionary<string, List<byte[]>> _signatures = new Dictionary<string, List<byte[]>>
        {
            { ".gif", new List<byte[]> { new byte[] { 0x47, 0x49, 0x46, 0x38 } } },
            { ".png", new List<byte[]> { new byte[] { 0x89, 0x50, 0x4E, 0x47, 0x0D, 0x0A, 0x1A, 0x0A } } },
            { ".jpeg", new List<byte[]>
                {
                    new byte[] { 0xFF, 0xD8, 0xFF, 0xE0 },
                    new byte[] { 0xFF, 0xD8, 0xFF, 0xE2 },
                    new byte[] { 0xFF, 0xD8, 0xFF, 0xE3 },
                }
            },
            { ".jpg", new List<byte[]>
                {
                    new byte[] { 0xFF, 0xD8, 0xFF, 0xE0 },
                    new byte[] { 0xFF, 0xD8, 0xFF, 0xE1 },
                    new byte[] { 0xFF, 0xD8, 0xFF, 0xE8 },
                }
            },
            { ".zip", new List<byte[]>
                {
                    new byte[] { 0x50, 0x4B, 0x03, 0x04 },
                    new byte[] { 0x50, 0x4B, 0x4C, 0x49, 0x54, 0x45 },
                    new byte[] { 0x50, 0x4B, 0x53, 0x70, 0x58 },
                    new byte[] { 0x50, 0x4B, 0x05, 0x06 },
                    new byte[] { 0x50, 0x4B, 0x07, 0x08 },
                    new byte[] { 0x57, 0x69, 0x6E, 0x5A, 0x69, 0x70 },
                }
            },
        };
        public static async Task<byte[]> ProcessFormFileAsync<T>(IFormFile formFile,
            ModelStateDictionary modelState, string[] permittedExtensions
            , long limitLength)
        {
            string fieldFileName = string.Empty;
            MemberInfo property = typeof(T).GetProperty(
                formFile.Name.Substring(formFile.Name.IndexOf(".",
                StringComparison.Ordinal
                ) + 1));
            if (property != null)
            {
                if (property.GetCustomAttribute(typeof(DisplayAttribute))
                    is DisplayAttribute displayAttribute)
                {
                    fieldFileName = $"{displayAttribute.Name}";
                }
            }
            var trustedFileName = WebUtility.HtmlEncode(formFile.FileName);
            if (formFile.Length <= 0)
            {
                modelState.AddModelError(formFile.Name,
                    $"{fieldFileName}({trustedFileName})是空的");
                return Array.Empty<byte>();
            }
            if (formFile.Length > limitLength)
            {
                var mbSize = limitLength / 1048576;
                modelState.AddModelError(formFile.Name,
                    $"{fieldFileName}({trustedFileName})" +
                    $"必须在{mbSize}MB以下 ");
                return Array.Empty<byte>();
            }
            try
            {
                using (var ms = new MemoryStream())
                {
                    await formFile.CopyToAsync(ms);
                    if (ms.Length == 0)
                    {
                        modelState.AddModelError(formFile.Name,
                            $"{fieldFileName}({trustedFileName})在移除字节序后是空的");
                        return Array.Empty<byte>();
                    }
                    if (!IsValidFileExtensionAndSinature(ms, formFile.FileName, permittedExtensions))
                    {
                        var ext = Path.GetExtension(formFile.FileName);
                        modelState.AddModelError(formFile.Name,
                            $"{fieldFileName}({trustedFileName})-{ext}-{string.Join(";", permittedExtensions)}格式不支持或损坏");
                        return Array.Empty<byte>();
                    }
                    return ms.ToArray();
                }
            }
            catch (Exception ex)
            {
                modelState.AddModelError(formFile.Name,
                    $"{fieldFileName}({trustedFileName})上传失败" +
                    $"错误信息：{ex.Message}");
                return Array.Empty<byte>();
            }
        }
        public static async Task<byte[]> ProcessStreamedFile(
                 MultipartSection section, ContentDispositionHeaderValue contentDisposition,
                 ModelStateDictionary modelState, string[] permittedExtensions, long sizeLimit)
        {
            try
            {
                using (var ms = new MemoryStream())
                {
                    await section.Body.CopyToAsync(ms);
                    if (ms.Length == 0)
                    {
                        modelState.AddModelError("file", "file is empty");
                    }
                    if (ms.Length > sizeLimit)
                    {
                        var MbSize = sizeLimit / 1048576;
                        modelState.AddModelError("file", $"file size limit{MbSize}Mb exceed");
                    }
                    if (!IsValidFileExtensionAndSinature(ms, contentDisposition.FileName.Value, permittedExtensions))
                    {
                        modelState.AddModelError("file", "wrong file formmat");
                    }
                    else
                    {
                        return ms.ToArray();
                    }
                }
            }
            catch (Exception ex)
            {
                modelState.AddModelError("error file", $"mmessage{ex.Message}");
            }
            return Array.Empty<byte>();
        }
        public static bool IsValidFileExtensionAndSinature(Stream data, string FileName, string[] permittedExtensions)
        {
            if (FileName == null || permittedExtensions == null || data == null)
            {
                return false;
            }
            if (string.IsNullOrEmpty(FileName) || data.Length == 0 || permittedExtensions.Length == 0)
            {
                return false;
            }
            var ext = Path.GetExtension(FileName);
            if (string.IsNullOrEmpty(ext) || !permittedExtensions.Contains(ext))
            {
                return false;
            }
            var signatures = _signatures[ext];
            data.Position = 0;
            using (var reader = new BinaryReader(data))
            {
                var dataHeader = reader.ReadBytes(signatures.Max(s => s.Length));
                // return signatures.Any(s => dataHeader.Take(s.Length).SequenceEqual(s));
            }
            return true;
        }
        public static string GetImgExtesions(string fileName, string[] permittedExtensions)
        {
            var ext = Path.GetExtension(fileName);
            if (ext != null)
            {
                if (permittedExtensions.Contains(ext))
                {
                    return ext;
                }
            }
            return null;
        }
        public static string[] GetFilePaths(string paths)
        {
            return paths?.Split(";", StringSplitOptions.RemoveEmptyEntries);
        }
        public static string GetCoverPath(string paths)
        {
            var ps = GetFilePaths(paths);
            return (ps == null || ps.Length == 0) ? "images/cover.jpg" : ps[0];
        }
    }

}
