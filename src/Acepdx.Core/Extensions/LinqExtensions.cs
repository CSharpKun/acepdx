using Acepdx.Core.Interfaces;
using Fastenshtein;

namespace Acepdx.Core.Extensions;

public static class LinqExtensions
{
    public static IEnumerable<T> GetRelevantLicenses<T>(this IEnumerable<T> licenses, string licenseId, bool includeDeprecated = false) where T : ILicense
    {
        return licenses.Where(license => license.LicenseId.Contains(licenseId, StringComparison.InvariantCultureIgnoreCase)
                        || license.LicenseId.StartsWith(licenseId, StringComparison.InvariantCultureIgnoreCase)
                        || Levenshtein.Distance(licenseId, license.LicenseId) <= 2)
                    .Where(license => includeDeprecated || !(license.IsDeprecatedLicenseId ?? false))
                    .OrderBy(license => license.LicenseId.StartsWith(licenseId, StringComparison.InvariantCultureIgnoreCase) ? 0 : 1)
                    .ThenBy(license => Levenshtein.Distance(licenseId, license.LicenseId))
                    .ThenBy(license =>
                    {
                        char[] digits = [.. license.LicenseId.Where(Char.IsDigit)];
                        if (digits.Length == 0) return int.MaxValue;
                        string stringDigits = new(digits);
                        if (!int.TryParse(digits, out int result)) return int.MaxValue;
                        return result * -1;
                    })
                    .ThenBy(license =>
                    {
                        var id = license.LicenseId;
                        if (id.EndsWith("-only")) return id[..^5];
                        if (id.EndsWith("-or-later")) return id[..^9];
                        return id;
                    })
                    .ThenBy(license =>
                    {
                        if (license.LicenseId.EndsWith("-only")) return 0;
                        if (license.LicenseId.EndsWith("-or-later")) return 1;
                        return 2;
                    })
                    .ThenBy(license => license.LicenseId.Length)
                    .ThenBy(license => license.LicenseId);
    }
}