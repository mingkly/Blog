namespace MyWeb.Models
{
    public class UserChatMessageView
    {
        public string Name { get; set; }
        public string Avatar { get; set; }
        public string Message { get; set; }
        public string Image { get; set; }
        public bool IsCurrentUser { get; set; }
        public string ViewId { get; set; }
    }
}
