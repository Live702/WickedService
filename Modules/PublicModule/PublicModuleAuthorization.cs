using LazyMagic.Shared;
using System.Runtime.CompilerServices;

namespace PublicModule
{
    public partial class PublicModuleAuthorization
    {
        /// <summary>
        /// Since this is the public module, we don't need to check permissions. We just return the default permissions
        /// for an anonymous user.
        /// </summary>
        /// <param name="request"></param>
        /// <param name="endpointName"></param>
        /// <returns></returns>
        public override async Task<ICallerInfo> GetCallerInfoAsync(HttpRequest request, [CallerMemberName] string endpointName = "")
        {
            await Task.Delay(0);
            string tenantKey = await GetTenantKeyAsync(request);
            string table = await GetTenantTableAsync(tenantKey);
            string tenantConfigBucket = await GetTenantConfigBucketAsync(request, tenantKey);
            var lzUserId = "";
            var userName = "";
            List<string> permissions = await GetUserPermissionsAsync(lzUserId, userName, table);
            var callerInfo = new CallerInfo() {
                LzUserId = lzUserId,
                UserName = userName,
                Table = table,
                TenantConfigBucket = tenantConfigBucket,
                Permissions = permissions,
                Tenancy = tenantKey
            };
            return callerInfo;
        }
        protected override async Task<List<string>> GetUserPermissionsAsync(string lzUserId, string userName, string table)
        {
            // Since default methods can't access instance state, we call the helper method that can.
            return await GetUserDefaultPermissionsAsync(lzUserId, userName, table);
        }
        public override async Task<bool> HasPermissionAsync(string methodName, List<string> userPermissions)
        {
            await Task.Delay(0);
            return true;
        }

    }
}
