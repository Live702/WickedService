namespace StoreModule;

public partial class StoreModuleAuthorization
{
    public override async Task<bool> HasPermissionAsync(string methodName, List<string> userPermissions)
    {
        await Task.Delay(0);
        return true;
    }
}
