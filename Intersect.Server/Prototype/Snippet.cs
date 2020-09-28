using System;
using System.Collections.Generic;
using System.Globalization;

namespace Intersect.Server.Prototype
{
    // The ideal row shape
    public class IndividualLocaleString
    {
        // First half of pk
        public Guid Id { get; set; }

        // Second half of pk
        public string Locale { get; set; }

        // This is the "default" version of the string
        public string Singular { get; set; }

        // If it DNE, it should fall back to Singular
        public string SingularLower { get; set; }

        // If it DNE, it should fall back to Singular
        public string SingularProper { get; set; }

        // If it DNE, it should fall back to Singular
        public string SingularUpper { get; set; }

        // If it DNE, it should fall back to Singular
        public string Plural { get; set; }

        // If it DNE, it should fall back to Plural
        public string PluralLower { get; set; }

        // If it DNE, it should fall back to Plural
        public string PluralProper { get; set; }

        // If it DNE, it should fall back to Plural
        public string PluralUpper { get; set; }
    }

    // Set of strings, the key of the dictionary is the locale code -- it actually maps to multiple rows
    public class LocalizesdString
    {
        // PK of the set, first half of the PK of the row
        public Guid Id { get; set; }

        public Dictionary<string, IndividualLocaleString> Localizations { get; set; }

        // This is overly simplified, there's actually a lot of desired fallback logic here
        public IndividualLocaleString Get(CultureInfo cultureInfo) => Localizations[cultureInfo.Name];
    }

    // A referring entity that EF *is* aware of via a DbSet<Item>
    // The question is how does EF map the set of translated data
    public class Item
    {
        public LocalizesdString Name { get; set; }

        public LocalizesdString Description { get; set; }
    }
}
