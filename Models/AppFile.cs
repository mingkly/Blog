using System;
using System.ComponentModel.DataAnnotations;

namespace MyWeb.Models
{
    public class AppFile
    {
        public long Id { get; set; }
        public byte[] FileContent { get; set; }
        public DateTime CreateTime { get; set; }
        public DateTime LastModifiedTime { get; set; }
        [MaxLength(100)]
        public string Note { get; set; }
        public long UploaderId { get; set; }
        public MyUser Uploader { get; set; }
        public string FileType { get; set; }
    }
    public class ArticleExamineBind
    {
        public long Id { get; set; }
        public int Action { get; set; }
    }
}
