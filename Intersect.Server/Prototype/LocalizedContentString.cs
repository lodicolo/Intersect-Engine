using Intersect.Localization;

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Globalization;

namespace Intersect.Server.Prototype
{
    public class LocalizedContentString
    {
        public sealed class CollectionMap : IDictionary<int, LocalizedContentString>,
            ICollection<LocalizedContentString>
        {
            private static void ValidatePair(KeyValuePair<int, LocalizedContentString> pair)
            {
                if (pair.Key != pair.Value.LocaleId)
                {
                    throw new ArgumentException(
                        $"Invalid {nameof(KeyValuePair<int, LocalizedContentString>)}. Key must be the same as {nameof(LocaleId)} ({pair.Value.LocaleId}) but was {pair.Key}.",
                        nameof(pair)
                    );
                }
            }

            private static void ValidatePair(int key, LocalizedContentString value)
            {
                if (key != value.LocaleId)
                {
                    throw new ArgumentException(
                        $"Invalid key. Key must be the same as {nameof(LocaleId)} ({value.LocaleId}) but was {key}.",
                        nameof(key)
                    );
                }
            }

            private readonly IDictionary<int, LocalizedContentString> backingDictionary =
                new Dictionary<int, LocalizedContentString>();

            public CollectionMap()
            {
            }

            public CollectionMap(IEnumerable<LocalizedContentString> localizedContentStrings)
            {
                if (localizedContentStrings == null)
                {
                    throw new ArgumentNullException(nameof(localizedContentStrings));
                }

                using (var enumerator = localizedContentStrings.GetEnumerator())
                {
                    while (enumerator.MoveNext())
                    {
                        Add(enumerator.Current);
                    }
                }
            }

            public CollectionMap(IEnumerable<KeyValuePair<int, LocalizedContentString>> pairs)
            {
                if (pairs == null)
                {
                    throw new ArgumentNullException(nameof(pairs));
                }

                using (var enumerator = pairs.GetEnumerator())
                {
                    while (enumerator.MoveNext())
                    {
                        Add(enumerator.Current);
                    }
                }
            }

            /// <inheritdoc />
            IEnumerator<LocalizedContentString> IEnumerable<LocalizedContentString>.GetEnumerator() =>
                backingDictionary.Values.GetEnumerator();

            /// <inheritdoc />
            IEnumerator<KeyValuePair<int, LocalizedContentString>>
                IEnumerable<KeyValuePair<int, LocalizedContentString>>.GetEnumerator() =>
                backingDictionary.GetEnumerator();

            /// <inheritdoc />
            IEnumerator IEnumerable.GetEnumerator() => ((ICollection<LocalizedContentString>) this).GetEnumerator();

            /// <inheritdoc />
            public void Add(KeyValuePair<int, LocalizedContentString> pair)
            {
                ValidatePair(pair);
                Add(pair.Value);
            }

            /// <inheritdoc />
            public void Add(LocalizedContentString item) => backingDictionary.Add(item.LocaleId, item);

            /// <inheritdoc />
            void ICollection<LocalizedContentString>.Clear() => backingDictionary.Clear();

            /// <inheritdoc />
            public bool Contains(LocalizedContentString item) => backingDictionary.ContainsKey(item.LocaleId);

            /// <inheritdoc />
            public void CopyTo(LocalizedContentString[] array, int arrayIndex) =>
                backingDictionary.Values.CopyTo(array, arrayIndex);

            /// <inheritdoc />
            public bool Remove(LocalizedContentString item) => backingDictionary.Remove(item.LocaleId);

            /// <inheritdoc />
            int ICollection<LocalizedContentString>.Count => backingDictionary.Count;

            /// <inheritdoc />
            bool ICollection<LocalizedContentString>.IsReadOnly => backingDictionary.IsReadOnly;

            /// <inheritdoc />
            void ICollection<KeyValuePair<int, LocalizedContentString>>.Clear() => backingDictionary.Clear();

            /// <inheritdoc />
            public bool Contains(KeyValuePair<int, LocalizedContentString> pair)
            {
                ValidatePair(pair);

                return backingDictionary.ContainsKey(pair.Key);
            }

            /// <inheritdoc />
            public void CopyTo(KeyValuePair<int, LocalizedContentString>[] array, int arrayIndex) =>
                backingDictionary.CopyTo(array, arrayIndex);

            /// <inheritdoc />
            public bool Remove(KeyValuePair<int, LocalizedContentString> pair)
            {
                ValidatePair(pair);

                return backingDictionary.Remove(pair);
            }

            /// <inheritdoc />
            int ICollection<KeyValuePair<int, LocalizedContentString>>.Count => backingDictionary.Count;

            /// <inheritdoc />
            bool ICollection<KeyValuePair<int, LocalizedContentString>>.IsReadOnly => backingDictionary.IsReadOnly;

            /// <inheritdoc />
            public bool ContainsKey(int key) => backingDictionary.ContainsKey(key);

            /// <inheritdoc />
            public void Add(int key, LocalizedContentString value)
            {
                ValidatePair(key, value);
                backingDictionary.Add(key, value);
            }

            /// <inheritdoc />
            public bool Remove(int key) => backingDictionary.Remove(key);

            /// <inheritdoc />
            public bool TryGetValue(int key, out LocalizedContentString value) =>
                backingDictionary.TryGetValue(key, out value);

            /// <inheritdoc />
            public LocalizedContentString this[int key]
            {
                get => backingDictionary[key];
                set => backingDictionary[key] = value;
            }

            /// <inheritdoc />
            public ICollection<int> Keys => backingDictionary.Keys;

            /// <inheritdoc />
            public ICollection<LocalizedContentString> Values => backingDictionary.Values;
        }

        /// <inheritdoc />
        [ForeignKey(nameof(ContentString)), Column(Order = 0)]
        public Guid ContentStringId { get; private set; }

        public virtual ContentString ContentString { get; private set; }

        [Column(Order = 1)]
        public int LocaleId
        {
            get => Locale.LCID;
            private set
            {
                try
                {
                    Locale = new CultureInfo(value);
                }
                catch (ArgumentOutOfRangeException)
                {
                    Locale = CultureInfo.CurrentCulture;
                }
                catch (CultureNotFoundException)
                {
                    Locale = CultureInfo.CurrentCulture;
                }
            }
        }

        [NotMapped]
        public string LocaleName
        {
            get => Locale.Name;
            private set
            {
                try
                {
                    Locale = new CultureInfo(value);
                }
                catch (ArgumentNullException)
                {
                    Locale = CultureInfo.CurrentCulture;
                }
                catch (CultureNotFoundException)
                {
                    Locale = CultureInfo.CurrentCulture;
                }
            }
        }

        [NotMapped]
        public CultureInfo Locale { get; private set; }

        [Required]
        public LocalizedString Value { get; set; }

        [Column(nameof(Plural))]
        protected virtual LocalizedString PluralOverride { get; set; }

        [NotMapped]
        public LocalizedString Plural
        {
            get => PluralOverride ?? Value;
            set => PluralOverride = value;
        }

        [Column(nameof(Zero))]
        protected virtual LocalizedString ZeroOverride { get; set; }

        [NotMapped]
        public LocalizedString Zero
        {
            get => ZeroOverride ?? Plural;
            set => ZeroOverride = value;
        }

        protected LocalizedContentString() { }

        public LocalizedContentString(
            ContentString contentString,
            int localeId,
            string value,
            string plural = null,
            string zero = null
        )
        {
            ContentString = contentString;
            LocaleId = localeId;
            Value = value;
            Plural = plural;
            Zero = zero;
        }

        public LocalizedContentString(
            ContentString contentString,
            int localeId,
            LocalizedContentString other
        )
        {
            ContentString = contentString;
            LocaleId = localeId;
            Value = other.Value;
            Plural = other.Plural;
            Zero = other.Zero;
        }

        public LocalizedContentString(
            ContentString contentString,
            string localeName,
            string value,
            string plural = null,
            string zero = null
        )
        {
            ContentString = contentString;
            LocaleName = localeName;
            Value = value;
            Plural = plural;
            Zero = zero;
        }

        public LocalizedContentString(
            ContentString contentString,
            string localeName,
            LocalizedContentString other
        )
        {
            ContentString = contentString;
            LocaleName = localeName;
            Value = other.Value;
            Plural = other.Plural;
            Zero = other.Zero;
        }

        public LocalizedContentString(
            ContentString contentString,
            CultureInfo locale,
            string value,
            string plural = null,
            string zero = null
        )
        {
            ContentString = contentString;
            Locale = locale;
            Value = value;
            Plural = plural;
            Zero = zero;
        }

        public LocalizedContentString(
            ContentString contentString,
            CultureInfo locale,
            LocalizedContentString other
        )
        {
            ContentString = contentString;
            Locale = locale;
            Value = other.Value;
            Plural = other.Plural;
            Zero = other.Zero;
        }

        public static void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<LocalizedContentString>().HasKey(e => new {e.ContentStringId, e.LocaleId});

            var localizedStringConverter = new ValueConverter<LocalizedString, string>(v => v, s => s);
            modelBuilder.Entity<LocalizedContentString>()
                .Property(e => e.Value)
                .HasConversion(localizedStringConverter);

            modelBuilder.Entity<LocalizedContentString>()
                .Property(e => e.Plural)
                .HasConversion(localizedStringConverter);

            modelBuilder.Entity<LocalizedContentString>().Property(e => e.Zero).HasConversion(localizedStringConverter);
        }
    }
}
