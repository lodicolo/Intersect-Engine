using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;

namespace Intersect.Server.Prototype
{
    public class PrototypeContainer : IPrototypeEntity
    {
        public class PrototypeContained
        {
            public string Normal { get; set; }

            public string Lower { get; set; }

            public string Upper { get; set; }

            public string Plural { get; set; }

            public string PluralLower { get; set; }

            public string PluralUpper { get; set; }

            public void Merge(PrototypeContained other)
            {
                Normal = other?.Normal?.Trim() ?? string.Empty;
                Lower = other?.Lower?.Trim() ?? string.Empty;
                Upper = other?.Upper?.Trim() ?? string.Empty;
                Plural = other?.Plural?.Trim() ?? string.Empty;
                PluralLower = other?.PluralLower?.Trim() ?? string.Empty;
                PluralUpper = other?.PluralUpper?.Trim() ?? string.Empty;
            }

            public static implicit operator string(PrototypeContained contained) => contained?.Normal;

            public static implicit operator PrototypeContained(string normal) =>
                new PrototypeContained {Normal = normal};
        }

        /// <inheritdoc />
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        [Column(Order = 0)]
        public Guid Id { get; }

        private Dictionary<string, PrototypeContained> mLocalizedVariants = new Dictionary<string, PrototypeContained>();
        private IReadOnlyDictionary<string, PrototypeContained> mReadOnlyLocalizedVariants;

        private Dictionary<string, PrototypeContained> LocalizedVariants
        {
            get => mLocalizedVariants;
            set
            {
                mLocalizedVariants = value;
                mReadOnlyLocalizedVariants = null;
            }
        }

        public IReadOnlyDictionary<string, PrototypeContained> Variants
        {
            get => mReadOnlyLocalizedVariants ??
                   (mReadOnlyLocalizedVariants =
                       new ReadOnlyDictionary<string, PrototypeContained>(mLocalizedVariants));
        }

        public PrototypeContained this[string locale]
        {
            get => mLocalizedVariants.TryGetValue(locale, out var contained)
                ? contained
                : mLocalizedVariants.FirstOrDefault().Value;

            set
            {
                if (value == null)
                {
                    mLocalizedVariants.Remove(locale);

                    return;
                }

                if (mLocalizedVariants.TryGetValue(locale, out var contained))
                {
                    contained.Merge(value);
                }
                else
                {
                    contained = value;
                    mLocalizedVariants[locale] = contained;
                }
            }
        }
    }
}
