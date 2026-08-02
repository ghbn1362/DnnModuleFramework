namespace DotNetNuke.Modules.Foundation.Services.Contracts
{
    public interface IDnnFacade
    {
        int GetCurrentPortalId();
        string GetHostSetting(string key);
        string GetModuleSetting(int moduleId, string key);
    }
}
