using Microsoft;
using Microsoft.EntityFrameworkCore;

using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Globalization;
using System.Linq;

namespace Intersect.Server.Prototype
{
    public class ContentString : IPrototypeEntity
    {
        public static CultureInfo FallbackCulture { get; set; } = CultureInfo.CurrentCulture;

        /// <inheritdoc />
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        [Key, Column(Order = 0)]
        public Guid Id { get; private set; }

        public string Notes { get; set; }

        private LocalizedContentString.CollectionMap LocalizationMap { get; set; } =
            new LocalizedContentString.CollectionMap();

        public ICollection<LocalizedContentString> AvailableLocalizations => LocalizationMap;

        public ContentString()
        {
        }

        public ContentString(string initial, CultureInfo cultureInfo = default)
        {
            var localizedContentString = new LocalizedContentString(
                this, cultureInfo ?? CultureInfo.CurrentCulture, initial
            );

            LocalizationMap[localizedContentString.LocaleId] = localizedContentString;
        }

        public LocalizedContentString this[int localeId]
        {
            get => ForCulture(new CultureInfo(localeId));
            set
            {
                if (value == null)
                {
                    Remove(localeId);
                }
                else
                {
                    Set(localeId, value);
                }
            }
        }

        public LocalizedContentString this[string localeName]
        {
            get => ForCulture(new CultureInfo(localeName));
            set
            {
                if (value == null)
                {
                    Remove(localeName);
                }
                else
                {
                    Set(localeName, value);
                }
            }
        }

        public LocalizedContentString ForCulture([ValidatedNotNull] CultureInfo cultureInfo)
        {
            if (LocalizationMap.TryGetValue(cultureInfo.LCID, out var localizedContentString))
            {
                return localizedContentString;
            }

            if (cultureInfo.Parent != null &&
                LocalizationMap.TryGetValue(cultureInfo.Parent.LCID, out localizedContentString))
            {
                return localizedContentString;
            }

            if (LocalizationMap.TryGetValue(FallbackCulture.LCID, out localizedContentString))
            {
                return localizedContentString;
            }

            return AvailableLocalizations.FirstOrDefault();
        }

        public LocalizedContentString Set(int localeId, string value) =>
            LocalizationMap[localeId] = new LocalizedContentString(this, localeId, value);

        public LocalizedContentString Set(int localeId, LocalizedContentString localizedContentString) =>
            LocalizationMap[localeId] = new LocalizedContentString(this, localeId, localizedContentString);

        public LocalizedContentString Set(string localeName, string value) => Set(new CultureInfo(localeName), value);

        public LocalizedContentString Set(string localeName, LocalizedContentString localizedContentString) =>
            Set(new CultureInfo(localeName), localizedContentString);

        public LocalizedContentString Set([ValidatedNotNull] CultureInfo locale, string value) =>
            LocalizationMap[locale.LCID] = new LocalizedContentString(this, locale, value);

        public LocalizedContentString Set(
            [ValidatedNotNull] CultureInfo locale,
            LocalizedContentString localizedContentString
        ) =>
            LocalizationMap[locale.LCID] = new LocalizedContentString(this, locale, localizedContentString);

        public bool Remove(int localeId) => LocalizationMap.Remove(localeId);

        public bool Remove(string localeName) => Remove(new CultureInfo(localeName));

        public bool Remove([ValidatedNotNull] CultureInfo locale) =>
            LocalizationMap.Remove(locale.LCID);

        public static void OnModelCreating([ValidatedNotNull] ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<ContentString>(
                e => e.HasMany(typeof(LocalizedContentString), nameof(AvailableLocalizations))
                    .WithOne(nameof(LocalizedContentString.ContentString))
                    .HasForeignKey(nameof(LocalizedContentString.ContentStringId))
            );

            LocalizedContentString.OnModelCreating(modelBuilder);
        }
    }
}
