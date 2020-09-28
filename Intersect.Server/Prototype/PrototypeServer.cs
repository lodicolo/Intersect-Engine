using System;
using System.Diagnostics;
using System.IO;
using System.Linq;

using Intersect.Logging;
using Intersect.Utilities;

using Microsoft.EntityFrameworkCore;

namespace Intersect.Server.Prototype
{
    public class PrototypeServer
    {
        public static void Start(string[] args)
        {
            ExportDependencies();

            Initialize();

            Create();

            Read();

            Update();

            Reread();

            Console.ReadLine();
        }

        public static void InContext(Action<PrototypeContext> action)
        {
            using (var context = new PrototypeContext())
            {
                try
                {
                    action(context);
                }
                catch (Exception exception)
                {
                    DumpException(exception);
                }
            }
        }

        public static void Initialize()
        {
            InContext(
                context =>
                {
                    context.Database.EnsureDeleted();
                    context.Database.Migrate();
                    Console.WriteLine("Initialized.");
                }
            );
        }

        public static void Create()
        {
            InContext(
                context =>
                {
                    var set = new PrototypeSetEntity { Name = "set" };
                    var simple = new PrototypeSimpleEntity { Name = "simple" };

                    var setOneToOne = new PrototypeSetEntity { Name = "setOneToOne" };
                    var simpleOneToOne = new PrototypeSimpleEntity { Name = "simpleOneToOne" };
                    setOneToOne.Add(simpleOneToOne);

                    var setOneToMany = new PrototypeSetEntity { Name = "setOneToMany" };
                    var simpleOneToMany1 = new PrototypeSimpleEntity { Name = "simpleOneToMany1" };
                    var simpleOneToMany2 = new PrototypeSimpleEntity { Name = "simpleOneToMany2" };
                    setOneToMany.Add(simpleOneToMany1);
                    setOneToMany.Add(simpleOneToMany2);

                    var setManyToOne1 = new PrototypeSetEntity { Name = "setManyToOne1" };
                    var setManyToOne2 = new PrototypeSetEntity { Name = "setManyToOne2" };
                    var simpleManyToOne = new PrototypeSimpleEntity { Name = "simpleManyToOne" };
                    setManyToOne1.Add(simpleManyToOne);
                    setManyToOne2.Add(simpleManyToOne);

                    var setManyToMany1 = new PrototypeSetEntity { Name = "setManyToMany1" };
                    var setManyToMany2 = new PrototypeSetEntity { Name = "setManyToMany2" };
                    var simpleManyToMany1 = new PrototypeSimpleEntity { Name = "simpleManyToMany1" };
                    var simpleManyToMany2 = new PrototypeSimpleEntity { Name = "simpleManyToMany2" };
                    setManyToMany1.Add(simpleManyToMany1);
                    setManyToMany1.Add(simpleManyToMany2);
                    setManyToMany2.Add(simpleManyToMany1);
                    setManyToMany2.Add(simpleManyToMany2);

                    context.Simples.AddRange(
                        simple, simpleManyToMany1, simpleManyToMany2, simpleManyToOne, simpleOneToMany1,
                        simpleOneToMany2, simpleOneToOne
                    );

                    context.Sets.AddRange(
                        set, setManyToMany1, setManyToMany2, setManyToOne1, setManyToOne2, setOneToMany, setOneToOne
                    );

                    context.Junctions.AddRange(setManyToMany1.Junctions);
                    context.Junctions.AddRange(setManyToMany2.Junctions);
                    context.Junctions.AddRange(setManyToOne1.Junctions);
                    context.Junctions.AddRange(setManyToOne2.Junctions);
                    context.Junctions.AddRange(setOneToMany.Junctions);
                    context.Junctions.AddRange(setOneToOne.Junctions);

                    context.SaveChanges();

                    var contentString = new ContentString("test");
                    contentString["es-ES"] = new LocalizedContentString(contentString, "es-ES", "test_es-ES");
                    contentString["it-IT"] = new LocalizedContentString(contentString, "it-IT", "test_it-IT");
                    context.ContentStrings.Add(contentString);

                    context.SaveChanges();

                    Console.WriteLine("Created.");
                }
            );
        }

        public static void Read()
        {
            InContext(
                context =>
                {
                    var contentStrings = context.ContentStrings.Include(c => c.AvailableLocalizations).ToList();
                    foreach (var contentString in contentStrings)
                    {
                        Console.WriteLine($"CSID: {contentString.Id}");
                        var localizations = contentString.AvailableLocalizations;
                        Console.WriteLine($"Localizations: {localizations.Count}");
                        foreach (var localization in localizations)
                        {
                            Console.WriteLine($"{localization.LocaleName}:");
                            Console.WriteLine($"Value: {localization.Value}");
                            Console.WriteLine($"Plural: {localization.Plural}");
                            Console.WriteLine($"Zero: {localization.Zero}");
                        }
                    }
                    Console.WriteLine($"Read {contentStrings.Count} {nameof(ContentString)}s from the database.");
                });
        }

        public static void Update()
        {
            InContext(
                context =>
                {
                    var contentStrings = context.ContentStrings.Include(c => c.AvailableLocalizations).ToList();
                    foreach (var contentString in contentStrings)
                    {
                        Console.WriteLine($"CSID: {contentString.Id}");
                        var localizations = contentString.AvailableLocalizations;
                        Console.WriteLine($"Localizations: {localizations.Count}");
                        foreach (var localization in localizations)
                        {
                            Console.WriteLine($"{localization.LocaleName}:");
                            Console.WriteLine($"Value: {localization.Value}");
                            Console.WriteLine($"Plural: {localization.Plural}");
                            Console.WriteLine($"Zero: {localization.Zero}");
                        }
                    }
                    Console.WriteLine($"Read {contentStrings.Count} {nameof(ContentString)}s from the database.");
                });
        }

        public static void Reread()
        {

        }

        public static void DumpException(Exception exception)
        {
            var currentException = exception;
            while (currentException != null)
            {
                if (currentException != exception)
                {
                    Console.WriteLine("Caused by:");
                }

                Console.WriteLine(currentException.Message);
                Console.WriteLine(currentException.StackTrace);
                currentException = currentException.InnerException;
            }
        }

        private static void ClearDlls()
        {
            DeleteIfExists("libe_sqlite3.so");
            DeleteIfExists("e_sqlite3.dll");
            DeleteIfExists("libe_sqlite3.dylib");
        }

        private static string ReadProcessOutput(string name)
        {
            try
            {
                Debug.Assert(name != null, "name != null");
                var p = new Process
                {
                    StartInfo =
                    {
                        UseShellExecute = false,
                        RedirectStandardOutput = true,
                        FileName = name
                    }
                };

                p.Start();

                // Do not wait for the child process to exit before
                // reading to the end of its redirected stream.
                // p.WaitForExit();
                // Read the output stream first and then wait.
                var output = p.StandardOutput.ReadToEnd();
                p.WaitForExit();
                output = output.Trim();

                return output;
            }
            catch
            {
                return "";
            }
        }

        internal static bool DeleteIfExists(string filename)
        {
            try
            {
                Debug.Assert(filename != null, "filename != null");
                if (File.Exists(filename))
                {
                    File.Delete(filename);
                }

                return true;
            }
            catch
            {
                return false;
            }
        }

        private static void ExportDependencies()
        {
            ClearDlls();

            var platformId = Environment.OSVersion.Platform;
            if (platformId == PlatformID.Unix)
            {
                var unixName = ReadProcessOutput("uname") ?? "";
                if (unixName.Contains("Darwin"))
                {
                    platformId = PlatformID.MacOSX;
                }
            }

            string sqliteResourceName = null;
            string sqliteFileName = null;
            switch (platformId)
            {
                case PlatformID.Win32NT:
                case PlatformID.Win32S:
                case PlatformID.Win32Windows:
                case PlatformID.WinCE:
                    sqliteResourceName = Environment.Is64BitProcess ? "e_sqlite3x64.dll" : "e_sqlite3x86.dll";
                    sqliteFileName = "e_sqlite3.dll";

                    break;

                case PlatformID.MacOSX:
                    sqliteResourceName = "libe_sqlite3.dylib";
                    sqliteFileName = "libe_sqlite3.dylib";

                    break;

                case PlatformID.Unix:
                    sqliteResourceName = Environment.Is64BitProcess ? "libe_sqlite3_x64.so" : "libe_sqlite3_x86.so";
                    sqliteFileName = "libe_sqlite3.so";

                    break;

                case PlatformID.Xbox:
                    break;

                default:
                    throw new ArgumentOutOfRangeException(nameof(platformId));
            }

            if (string.IsNullOrWhiteSpace(sqliteResourceName) || string.IsNullOrWhiteSpace(sqliteFileName))
            {
                return;
            }

            sqliteResourceName = $"Intersect.Server.Resources.{sqliteResourceName}";
            if (ReflectionUtils.ExtractResource(sqliteResourceName, sqliteFileName))
            {
                return;
            }

            Log.Error($"Failed to extract {sqliteFileName} library, terminating startup.");
            Environment.Exit(-0x1000);
        }
    }
}
