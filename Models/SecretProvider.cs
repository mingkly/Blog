using Microsoft.AspNetCore.DataProtection;

namespace MyWeb.Models
{
    public class SecretProvider
    {

        IDataProtectionProvider _protectionProvider;
        public SecretProvider(IDataProtectionProvider protectionProvider)
        {
            _protectionProvider = protectionProvider;
        }
        public string ProtectId(string id)
        {
            var protector = _protectionProvider.CreateProtector("IdProtection.v1");
            return protector.Protect(id);
        }
        public string UnProtectId(string protectId)
        {
            var protector = _protectionProvider.CreateProtector("IdProtection.v1");
            return protector.Unprotect(protectId);
        }
        public long ProtectId(long id)
        {
            return (id * 43534 + 34321) ^ 3784393;
        }
        public long UnProtectId(long id)
        {
            return ((id ^ 3784393) - 34321) / 43534;
        }
    }
}
