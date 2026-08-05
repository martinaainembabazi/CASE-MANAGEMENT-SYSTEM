namespace Template.Common.Static
{
    public static class SystemPermissions
    {
        public static class Roles // RoleController Actions
        {
            public const string CreateRole = "RolesController Create";
            public const string ViewRoles = "RolesController Index";
            public const string EditRole = "RolesController Edit";
            public const string DeleteRole = "RolesController Delete";
        }
        
        public static class Account
        {
            public const string ViewApplicationUsers = "AccountController Index";
            public const string CreateApplicationUser = "AccountController Create";
            public const string EditApplicationUser = "AccountController Edit";
        }

        public static class AuditLog // AuditLogController Actions
        {
            public const string ViewAuditLogs = "AuditLogController Index";
        }
	}
}
