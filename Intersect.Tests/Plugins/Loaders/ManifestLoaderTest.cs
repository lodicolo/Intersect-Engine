﻿
using Intersect.Plugins.Manifests;
using Moq;
using NUnit.Framework;
using Semver;
using System.Reflection;
using Intersect.Core;
using Intersect.Plugins.Interfaces;
using Intersect.Plugins.Manifests.Types;
using Microsoft.Extensions.Logging;
using Serilog;

namespace Intersect.Plugins.Loaders
{
    internal partial class MockAssembly : Assembly
    {
        public override string FullName => nameof(MockAssembly);

        public Exception ExceptionGetTypes { get; set; } = null;

        public Exception ExceptionGetManifestResourceInfo { get; set; } = null;

        public Exception ExceptionGetManifestResourceStream { get; set; } = null;

        public Type[] MockTypes { get; set; } = [];

        public Dictionary<string, ManifestResourceInfo> MockManifestResourceInfo { get; set; } =
            new Dictionary<string, ManifestResourceInfo>();

        public Dictionary<string, Stream> MockManifestResourceStream { get; set; } = new Dictionary<string, Stream>();

        public override Type[] GetTypes() => ExceptionGetTypes != null ? throw ExceptionGetTypes : MockTypes;

        public override ManifestResourceInfo GetManifestResourceInfo(string resourceName) =>
            ExceptionGetManifestResourceInfo != null
                ? throw ExceptionGetManifestResourceInfo
                : (MockManifestResourceInfo.ContainsKey(resourceName)
                    ? MockManifestResourceInfo[resourceName]
                    : default);

        public override Stream GetManifestResourceStream(string name) => ExceptionGetManifestResourceStream != null
            ? throw ExceptionGetManifestResourceStream
            : (MockManifestResourceStream.ContainsKey(name) ? MockManifestResourceStream[name] : default);
    }

    internal interface IllegalVirtualManifestInterface
    {
    }

    internal abstract partial class IllegalVirtualManifestAbstractClass
    {
    }

    internal partial class IllegalVirtualManifestGenericClass<T>
    {
    }

    internal partial class IllegalVirtualManifestDefinedClass
    {
    }

    internal partial class IllegalVirtualManifestNoSupportedConstructorsClass : IManifestHelper
    {
        public IllegalVirtualManifestNoSupportedConstructorsClass(string name)
        {
            Name = name;
        }

        /// <inheritdoc />
        public string Name { get; }

        /// <inheritdoc />
        public string Key { get; }

        /// <inheritdoc />
        public SemVersion Version { get; }

        /// <inheritdoc />
        public Authors Authors { get; }

        /// <inheritdoc />
        public string Homepage { get; }
    }

    internal partial struct VirtualManifestValueType : IManifestHelper
    {
        /// <inheritdoc />
        public string Name { get; }

        /// <inheritdoc />
        public string Key { get; }

        /// <inheritdoc />
        public SemVersion Version { get; }

        /// <inheritdoc />
        public Authors Authors { get; }

        /// <inheritdoc />
        public string Homepage { get; }
    }

    [TestFixture]
    public partial class ManifestLoaderTest
    {
        [SetUp]
        public void SetUp()
        {
            ApplicationContext.Context.Value = new TestApplicationContext();
            ManifestLoader.ManifestLoaderDelegates.Clear();
        }

        [Test]
        public void IsVirtualManifestType()
        {
            Assert.IsFalse(ManifestLoader.IsVirtualManifestType(typeof(IllegalVirtualManifestAbstractClass)));
            Assert.IsFalse(ManifestLoader.IsVirtualManifestType(typeof(IllegalVirtualManifestDefinedClass)));
            Assert.IsFalse(ManifestLoader.IsVirtualManifestType(typeof(IllegalVirtualManifestGenericClass<string>)));
            Assert.IsFalse(ManifestLoader.IsVirtualManifestType(typeof(IllegalVirtualManifestInterface)));
            Assert.IsFalse(
                ManifestLoader.IsVirtualManifestType(typeof(IllegalVirtualManifestNoSupportedConstructorsClass))
            );

            Assert.IsTrue(ManifestLoader.IsVirtualManifestType(typeof(VirtualManifestValueType)));
            Assert.IsTrue(ManifestLoader.IsVirtualManifestType(typeof(VirtualTestManifest)));
        }

        [Test]
        public void FindManifest_LoadsJsonManifest_WellFormed()
        {
            ManifestLoader.ManifestLoaderDelegates.Add(ManifestLoader.LoadJsonManifestFrom);
            var mockAssembly = new MockAssembly
            {
                MockManifestResourceInfo = new Dictionary<string, ManifestResourceInfo>
                {
                    {
                        @"manifest.json",
                        typeof(ManifestLoaderTest).Assembly.GetManifestResourceInfo(
                            $@"{VirtualTestManifest.Namespace}.manifest.well-formed.json"
                        )
                    }
                },
                MockManifestResourceStream = new Dictionary<string, Stream>
                {
                    {
                        @"manifest.json",
                        typeof(ManifestLoaderTest).Assembly.GetManifestResourceStream(
                            $@"{VirtualTestManifest.Namespace}.manifest.well-formed.json"
                        )
                    }
                }
            };

            var manifest = ManifestLoader.FindManifest(mockAssembly);
            Assert.IsTrue(manifest is JsonManifest);
            Assert.NotNull(manifest);
            Assert.AreEqual("Test Manifest", manifest.Name);
            Assert.AreEqual("AscensionGameDev.Intersect.Tests", manifest.Key);
            Assert.AreEqual(new SemVersion(1), manifest.Version);
            Assert.AreEqual("https://github.com/AscensionGameDev/Intersect-Engine", manifest.Homepage);
        }

        [Test]
        public void FindManifest_ReturnsNullWhenNoManifestsFound()
        {
            ManifestLoader.ManifestLoaderDelegates.Add(ManifestLoader.LoadJsonManifestFrom);
            ManifestLoader.ManifestLoaderDelegates.Add(ManifestLoader.LoadVirtualManifestFrom);
            var mockAssembly = new MockAssembly
            {
            };

            var manifest = ManifestLoader.FindManifest(mockAssembly);
            Assert.Null(manifest);
        }

        [Test]
        public void FindManifest_LogsErrorsIfAnExceptionIsThrownByDelegate()
        {
            var mockException = new Exception("Delegate exception");
            ManifestLoader.ManifestLoaderDelegates.Add(_ => throw mockException);

            var mockAssembly = new MockAssembly();
            var manifest = ManifestLoader.FindManifest(mockAssembly);

            Assert.Multiple(
                () =>
                {
                    Assert.That(manifest, Is.Null);
                    ApplicationContext.GetCurrentContext<TestApplicationContext>()
                        .LoggerMock.Verify(
                            logger => logger.Log(
                                It.Is<LogLevel>(logLevel => logLevel == LogLevel.Error),
                                It.IsAny<EventId>(),
                                It.Is<It.IsAnyType>((@object, type) => @object.ToString() == "Exception thrown by manifest loader delegate"),
                                It.Is<Exception>(e => e == mockException),
                                It.Is<Func<It.IsAnyType, Exception?, string>>((_, _) => true)
                            )
                        );
                }
            );
        }

        [Test]
        public void FindManifest_LoadsJsonManifest_Lowercase()
        {
            ManifestLoader.ManifestLoaderDelegates.Add(ManifestLoader.LoadJsonManifestFrom);
            var mockAssembly = new MockAssembly
            {
                MockManifestResourceInfo = new Dictionary<string, ManifestResourceInfo>
                {
                    {
                        @"manifest.json",
                        typeof(ManifestLoaderTest).Assembly.GetManifestResourceInfo(
                            $@"{VirtualTestManifest.Namespace}.manifest.lowercase.json"
                        )
                    }
                },
                MockManifestResourceStream = new Dictionary<string, Stream>
                {
                    {
                        @"manifest.json",
                        typeof(ManifestLoaderTest).Assembly.GetManifestResourceStream(
                            $@"{VirtualTestManifest.Namespace}.manifest.lowercase.json"
                        )
                    }
                }
            };

            var manifest = ManifestLoader.FindManifest(mockAssembly);
            Assert.IsTrue(manifest is JsonManifest);
            Assert.NotNull(manifest);
            Assert.AreEqual("Test Manifest", manifest.Name);
            Assert.AreEqual("AscensionGameDev.Intersect.Tests", manifest.Key);
            Assert.AreEqual(new SemVersion(1), manifest.Version);
            Assert.AreEqual("https://github.com/AscensionGameDev/Intersect-Engine", manifest.Homepage);
        }

        [Test]
        public void FindManifest_LoadsVirtualManifest()
        {
            ManifestLoader.ManifestLoaderDelegates.Add(ManifestLoader.LoadVirtualManifestFrom);
            var mockAssembly = new MockAssembly
            {
                MockTypes = [typeof(VirtualTestManifest)]
            };

            var manifest = ManifestLoader.FindManifest(mockAssembly);
            Assert.IsTrue(manifest is VirtualTestManifest);
            Assert.NotNull(manifest);
            Assert.AreEqual("Test Manifest", manifest.Name);
            Assert.AreEqual("AscensionGameDev.Intersect.Tests", manifest.Key);
            Assert.AreEqual(new SemVersion(1), manifest.Version);
            Assert.AreEqual("https://github.com/AscensionGameDev/Intersect-Engine", manifest.Homepage);
        }

        [Test]
        public void FindManifest_LoadsVirtualManifestWhenJsonManifestNotFound()
        {
            ManifestLoader.ManifestLoaderDelegates.Add(ManifestLoader.LoadJsonManifestFrom);
            ManifestLoader.ManifestLoaderDelegates.Add(ManifestLoader.LoadVirtualManifestFrom);
            var mockAssembly = new MockAssembly
            {
                MockTypes = [typeof(VirtualTestManifest)]
            };

            var manifest = ManifestLoader.FindManifest(mockAssembly);
            Assert.IsTrue(manifest is VirtualTestManifest);
            Assert.NotNull(manifest);
            Assert.AreEqual("Test Manifest", manifest.Name);
            Assert.AreEqual("AscensionGameDev.Intersect.Tests", manifest.Key);
            Assert.AreEqual(new SemVersion(1), manifest.Version);
            Assert.AreEqual("https://github.com/AscensionGameDev/Intersect-Engine", manifest.Homepage);
        }

        [Test]
        public void FindManifest_LoadsJsonManifestWhenFoundInsteadOfVirtualManifest()
        {
            ManifestLoader.ManifestLoaderDelegates.Add(ManifestLoader.LoadJsonManifestFrom);
            ManifestLoader.ManifestLoaderDelegates.Add(ManifestLoader.LoadVirtualManifestFrom);
            var mockAssembly = new MockAssembly
            {
                MockTypes = [typeof(VirtualTestManifest)],
                MockManifestResourceInfo = new Dictionary<string, ManifestResourceInfo>
                {
                    {
                        @"manifest.json",
                        typeof(ManifestLoaderTest).Assembly.GetManifestResourceInfo(
                            $@"{VirtualTestManifest.Namespace}.manifest.lowercase.json"
                        )
                    }
                },
                MockManifestResourceStream = new Dictionary<string, Stream>
                {
                    {
                        @"manifest.json",
                        typeof(ManifestLoaderTest).Assembly.GetManifestResourceStream(
                            $@"{VirtualTestManifest.Namespace}.manifest.lowercase.json"
                        )
                    }
                }
            };

            var manifest = ManifestLoader.FindManifest(mockAssembly);
            Assert.IsTrue(manifest is JsonManifest);
            Assert.NotNull(manifest);
            Assert.AreEqual("Test Manifest", manifest.Name);
            Assert.AreEqual("AscensionGameDev.Intersect.Tests", manifest.Key);
            Assert.AreEqual(new SemVersion(1), manifest.Version);
            Assert.AreEqual("https://github.com/AscensionGameDev/Intersect-Engine", manifest.Homepage);
        }

        [Test]
        public void FindManifest_LoadsVirtualManifestWhenFoundInsteadOfJsonManifest()
        {
            ManifestLoader.ManifestLoaderDelegates.Add(ManifestLoader.LoadVirtualManifestFrom);
            ManifestLoader.ManifestLoaderDelegates.Add(ManifestLoader.LoadJsonManifestFrom);
            var mockAssembly = new MockAssembly
            {
                MockTypes = [typeof(VirtualTestManifest)],
                MockManifestResourceInfo = new Dictionary<string, ManifestResourceInfo>
                {
                    {
                        @"manifest.json",
                        typeof(ManifestLoaderTest).Assembly.GetManifestResourceInfo(
                            $@"{VirtualTestManifest.Namespace}.manifest.lowercase.json"
                        )
                    }
                },
                MockManifestResourceStream = new Dictionary<string, Stream>
                {
                    {
                        @"manifest.json",
                        typeof(ManifestLoaderTest).Assembly.GetManifestResourceStream(
                            $@"{VirtualTestManifest.Namespace}.manifest.lowercase.json"
                        )
                    }
                }
            };

            var manifest = ManifestLoader.FindManifest(mockAssembly);
            Assert.IsTrue(manifest is VirtualTestManifest);
            Assert.NotNull(manifest);
            Assert.AreEqual("Test Manifest", manifest.Name);
            Assert.AreEqual("AscensionGameDev.Intersect.Tests", manifest.Key);
            Assert.AreEqual(new SemVersion(1), manifest.Version);
            Assert.AreEqual("https://github.com/AscensionGameDev/Intersect-Engine", manifest.Homepage);
        }

        [Test]
        public void FindManifest_ThrowsIfNoDelegates()
        {
            Assert.Throws<InvalidOperationException>(
                () => ManifestLoader.FindManifest(new MockAssembly()),
                $"{nameof(ManifestLoader.ManifestLoaderDelegates)} was initialized with no pre-registered delegates, or the pre-defined delegates were removed and no alternatives were added."
            );
        }

        [Test]
        public void LoadJsonManifestFrom_ReturnsNullIfNoManifest()
        {
            Assert.IsNull(ManifestLoader.LoadJsonManifestFrom(new MockAssembly()));
        }

        [Test]
        public void LoadJsonManifestFrom_ThrowsExceptionIfExceptionThrownFromResourceInfo()
        {
            var mockException = new Exception(nameof(MockAssembly.ExceptionGetManifestResourceInfo));

            var thrownException = Assert.Catch<Exception>(() =>
                ManifestLoader.LoadJsonManifestFrom(
                    new MockAssembly
                    {
                        ExceptionGetManifestResourceInfo = mockException
                    }
                )
            );

            Assert.AreEqual("Failed to load manifest.json from MockAssembly.", thrownException.Message);
            Assert.AreSame(mockException.GetType(), thrownException.InnerException?.GetType());
            Assert.AreEqual(mockException.Message, thrownException.InnerException?.Message);
        }

        [Test]
        public void LoadJsonManifestFrom_ThrowsExceptionIfStreamIsNull()
        {
            var mockException = new InvalidDataException("Manifest resource stream null when info exists.");

            var thrownException = Assert.Catch<Exception>(() =>
                ManifestLoader.LoadJsonManifestFrom(
                    new MockAssembly
                    {
                        MockManifestResourceInfo = new Dictionary<string, ManifestResourceInfo>
                        {
                            {
                                @"manifest.json",
                                typeof(ManifestLoaderTest).Assembly.GetManifestResourceInfo(
                                    $@"{VirtualTestManifest.Namespace}.manifest.lowercase.json"
                                )
                            }
                        }
                    }
                )
            );

            Assert.AreEqual("Failed to load manifest.json from MockAssembly.", thrownException.Message);
            Assert.AreSame(mockException.GetType(), thrownException.InnerException?.GetType());
            Assert.AreEqual(mockException.Message, thrownException.InnerException?.Message);
        }

        [Test]
        public void LoadJsonManifestFrom_ThrowsExceptionIfStreamIsEmpty()
        {
            var mockException = new InvalidDataException("Manifest is empty or failed to load and is null.");

            var thrownException = Assert.Catch<Exception>(() =>
                ManifestLoader.LoadJsonManifestFrom(
                    new MockAssembly
                    {
                        MockManifestResourceInfo = new Dictionary<string, ManifestResourceInfo>
                        {
                            {
                                @"manifest.json",
                                typeof(ManifestLoaderTest).Assembly.GetManifestResourceInfo(
                                    $@"{VirtualTestManifest.Namespace}.manifest.lowercase.json"
                                )
                            }
                        },
                        MockManifestResourceStream = new Dictionary<string, Stream>
                        {
                            {@"manifest.json", new MemoryStream()}
                        }
                    }
                )
            );

            Assert.AreEqual("Failed to load manifest.json from MockAssembly.", thrownException.Message);
            Assert.AreSame(mockException.GetType(), thrownException.InnerException?.GetType());
            Assert.AreEqual(mockException.Message, thrownException.InnerException?.Message);
        }

        [Test]
        public void LoadJsonManifestFrom_ThrowsExceptionIfExceptionThrownFromResourceStream()
        {
            var mockException = new Exception(nameof(MockAssembly.ExceptionGetManifestResourceStream));

            var thrownException = Assert.Catch<Exception>(() =>
                ManifestLoader.LoadJsonManifestFrom(
                    new MockAssembly
                    {
                        ExceptionGetManifestResourceStream = mockException,
                        MockManifestResourceInfo = new Dictionary<string, ManifestResourceInfo>
                        {
                            {
                                @"manifest.json",
                                typeof(ManifestLoaderTest).Assembly.GetManifestResourceInfo(
                                    $@"{VirtualTestManifest.Namespace}.manifest.lowercase.json"
                                )
                            }
                        }
                    }
                )
            );

            Assert.AreEqual("Failed to load manifest.json from MockAssembly.", thrownException.Message);
            Assert.AreSame(mockException.GetType(), thrownException.InnerException?.GetType());
            Assert.AreEqual(mockException.Message, thrownException.InnerException?.Message);
        }

        [Test]
        public void LoadVirtualManifestFrom_ReturnsNullIfNoTypes()
        {
            Assert.IsNull(ManifestLoader.LoadVirtualManifestFrom(new MockAssembly()));
        }

        [Test]
        public void LoadVirtualManifestFrom_ThrowsExceptionIfExceptionThrown()
        {
            var mockException = new Exception(nameof(MockAssembly.ExceptionGetTypes));

            var thrownException = Assert.Catch<Exception>(
                () => ManifestLoader.LoadVirtualManifestFrom(new MockAssembly { ExceptionGetTypes = mockException })
            );

            Assert.AreEqual("Failed to load virtual manifest from MockAssembly.", thrownException.Message);
            Assert.AreSame(mockException, thrownException.InnerException);
        }
    }
}
