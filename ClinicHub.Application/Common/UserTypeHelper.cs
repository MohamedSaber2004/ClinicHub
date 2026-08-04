using ClinicHub.Domain.Enums;

namespace ClinicHub.Application.Common
{
    public static class UserTypeHelper
    {
        private static readonly UserType[] PriorityOrder =
        {
            UserType.SuperAdmin,
            UserType.ClinicOwner,
            UserType.Doctor,
            UserType.Staff,
            UserType.User
        };

        /// <summary>
        /// Resolves the primary role for a user who may hold multiple roles
        /// (e.g. a clinic owner who is also a doctor). The result is deterministic
        /// so clients can rely on it for layout/feature decisions.
        /// </summary>
        public static string GetPrimaryRole(IList<string> roles)
        {
            var parsed = roles
                .Where(r => Enum.TryParse<UserType>(r, ignoreCase: true, out _))
                .Select(r => Enum.Parse<UserType>(r, ignoreCase: true))
                .ToHashSet();

            foreach (var candidate in PriorityOrder)
            {
                if (parsed.Contains(candidate))
                    return candidate.ToString();
            }

            return roles.FirstOrDefault() ?? UserType.User.ToString();
        }
    }
}
