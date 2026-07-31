namespace ArcaneVault_Web
{
    /// <summary>
    /// Session access used by every page. Centralised so the shape of the
    /// session (and the meaning of RoleId) is defined in exactly one place.
    /// </summary>
    public static class SessionHelper
    {
        public const string UserNameKey = "UserName";
        public const string RoleIdKey = "RoleId";

        /// <summary>RoleId assigned to staff/administrators.</summary>
        public const int AdminRoleId = 1;

        public static string? GetUserName(this HttpContext context)
        {
            return context.Session.GetString(UserNameKey);
        }

        public static int? GetRoleId(this HttpContext context)
        {
            return context.Session.GetInt32(RoleIdKey);
        }

        public static bool IsSignedIn(this HttpContext context)
        {
            return !string.IsNullOrWhiteSpace(context.GetUserName());
        }

        public static bool IsAdmin(this HttpContext context)
        {
            return context.GetRoleId() == AdminRoleId;
        }
    }
}
