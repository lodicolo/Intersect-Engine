using Intersect.Server.Framework.Database.PlayerData.Security;

namespace Intersect.Server.Database.PlayerData.Security
{
    public class ApiRoles : IApiRoles
    {

        public bool UserQuery { get; set; }

        public bool UserManage { get; set; }

    }

}
