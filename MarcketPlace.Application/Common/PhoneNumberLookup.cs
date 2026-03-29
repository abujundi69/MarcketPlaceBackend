namespace MarcketPlace.Application.Common
{
    /// <summary>
    /// Builds equivalent phone strings so login and uniqueness checks work when the mobile app
    /// sends E.164 (+970...) but admin panels store local numbers (05...).
    /// </summary>
    public static class PhoneNumberLookup
    {
        public static IReadOnlyList<string> BuildCandidates(string? raw)
        {
            if (string.IsNullOrWhiteSpace(raw))
                return Array.Empty<string>();

            var trimmed = raw.Trim();
            var set = new HashSet<string>(StringComparer.Ordinal)
            {
                trimmed,
            };

            var digits = new string(trimmed.Where(char.IsDigit).ToArray());
            if (digits.Length == 0)
                return set.ToList();

            if (digits.StartsWith("970", StringComparison.Ordinal) && digits.Length >= 12)
                AddNationalVariants(set, digits[3..], "970");

            if (digits.StartsWith("972", StringComparison.Ordinal) && digits.Length >= 11)
                AddNationalVariants(set, digits[3..], "972");

            if (digits[0] == '0' && digits.Length >= 9)
            {
                var withoutLeadingZero = digits.TrimStart('0');
                if (withoutLeadingZero.Length > 0)
                {
                    set.Add(digits);
                    set.Add("+970" + withoutLeadingZero);
                    set.Add("+972" + withoutLeadingZero);
                }
            }

            return set.ToList();
        }

        private static void AddNationalVariants(HashSet<string> set, string national, string countryCode)
        {
            if (national.Length == 0)
                return;

            set.Add("+" + countryCode + national);

            if (national[0] == '0')
                set.Add(national);
            else
                set.Add("0" + national);
        }
    }
}
