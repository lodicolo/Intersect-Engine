namespace Intersect.Server.Framework.Database.PlayerData.Security
{
    public interface IApiRoles
    {
        bool UserManage { get; set; }
        bool UserQuery { get; set; }
    }
}