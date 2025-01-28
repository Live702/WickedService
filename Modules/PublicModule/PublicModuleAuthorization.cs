using LazyMagic.Shared;
using System.Runtime.CompilerServices;

namespace PublicModule
{
    public partial class PublicModuleAuthorization
    {
        public PublicModuleAuthorization ()
        {
            authenticate = false;
        }
        protected override async Task<List<string>> GetUserPermissionsAsync(string lzUserId, string userName, string table)
        {
            await Task.Delay(0);
            // Since default methods can't access instance state, we call the helper method that can.
            return new List<string>();
        }
        // Public API has no permission checks
        public override async Task<bool> HasPermissionAsync(string methodName, List<string> userPermissions)
        {
            await Task.Delay(0);
            return true;
        }
    }
}
