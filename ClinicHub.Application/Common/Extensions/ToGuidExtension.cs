namespace ClinicHub.Application.Common.Extensions
{
    public static class ToGuidExtension
    {
        public static Guid ToGuid(this Guid? nullableGuid)
        {
            return nullableGuid.ToGuidOrDefault(Guid.Empty);
        }

        public static Guid ToGuidOrDefault(this Guid? nullableGuid, Guid defaultValue)
        {
            return nullableGuid.HasValue ? nullableGuid.Value : defaultValue;
        }


        public static Guid ToGuid(this string? str)
        {
            Guid guid;
            if (string.IsNullOrEmpty(str) || !Guid.TryParse(str, out guid))
            {
                guid = Guid.Empty;
            }

            return guid;
        }

        public static Guid? ToGuid(this object? obj)
        {
            if (obj == null || !Guid.TryParse(obj?.ToString(), out var guid))
            {
                return Guid.Empty;
            }

            return guid;
        }

        public static bool IsGuid(this string? input)
        {
            return Guid.TryParse(input, out _);
        }
    }
}
