using System;
using System.Text.RegularExpressions;

namespace Speaker.Recorder.Services.IdentificationService
{
    public abstract class BaseIdentificationService : IIdentificationService
    {
        public abstract string GetRawIdentifier();

        public string GetSanitizedIdentifier()
        {
            var identifier = GetRawIdentifier();

            // Convert to lowercase
            identifier = identifier.ToLowerInvariant();

            // Remove invalid characters (valid: lowercase, numbers, hyphens)
            Regex forbidenCharactersRegex = new Regex("[^a-z0-9-]*");
            identifier = forbidenCharactersRegex.Replace(identifier, string.Empty);

            // Remove if two or more hyphens
            Regex twoOrMoreHyphens = new Regex("-{2,}");
            identifier = twoOrMoreHyphens.Replace(identifier, string.Empty);

            // If starts or ends with a hyphen, replace it with a zero
            Regex startsWithAHyphen = new Regex("^[^0-9a-z]");
            Regex endsWithAHyphen = new Regex("[^0-9a-z]$");
            identifier = startsWithAHyphen.Replace(identifier, "0");
            identifier = endsWithAHyphen.Replace(identifier, "0");

            // If it has less than three characters, append some zeros
            if (identifier.Length < 3)
            {
                identifier += "000";
            }

            // If it has more than 63 characters, truncate it
            if (identifier.Length - 63 > 0)
            {
                identifier = identifier.Substring(0, 63);
            }

            return identifier;
        }
    }
}
