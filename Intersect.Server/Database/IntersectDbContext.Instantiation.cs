using System.Diagnostics;
using System.Linq.Expressions;
using System.Reflection;
using Intersect.Config;
using Intersect.Framework.Reflection;
using Intersect.Logging;
using Microsoft.EntityFrameworkCore;

namespace Intersect.Server.Database;

public delegate DbContext DbContextConstructor(DatabaseContextOptions databaseContextOptions);

public partial class IntersectDbContext<TDbContext>
{
    private static readonly Dictionary<(Type, DatabaseType), DbContextConstructor> _constructorCache = new();

    private static readonly ParameterInfo[] _createContextParameters =
        typeof(IntersectDbContext<>)
            .GetConstructors(BindingFlags.NonPublic | BindingFlags.Instance)
            .First()
            .GetParameters()
            .ToArray();

#if DEBUG
    private static readonly List<IntersectDbContext<TDbContext>> _instances = new();
#endif

    protected IntersectDbContext(DatabaseContextOptions? databaseContextOptions)
    {
#if DEBUG
        if (Debugger.IsAttached)
        {
            _instances.Add(this as TDbContext);
        }
#endif
        var safeDatabaseContextOptions = databaseContextOptions ?? _fallbackContextOptions;
        ContextOptions = safeDatabaseContextOptions with
        {
            ConnectionStringBuilder = safeDatabaseContextOptions.ConnectionStringBuilder ??
                                      _fallbackContextOptions.ConnectionStringBuilder
        };

        base.ChangeTracker.AutoDetectChangesEnabled = safeDatabaseContextOptions.AutoDetectChanges || IsReadOnly;
        base.ChangeTracker.LazyLoadingEnabled = safeDatabaseContextOptions.LazyLoading || !IsReadOnly;
    }

    internal List<IntersectDbContext<TDbContext>> instances => _instances;

    public override void Dispose()
    {
#if DEBUG
        if (Debugger.IsAttached && !_instances.Remove(this))
        {
            Debugger.Break();
        }
#endif
        base.Dispose();
    }

    public override ValueTask DisposeAsync()
    {
#if DEBUG
        if (Debugger.IsAttached && !_instances.Remove(this))
        {
            Debugger.Break();
        }
#endif
        return base.DisposeAsync();
    }

    private static DbContextConstructor CreateConstructorDelegate<TContextType>(
        DatabaseType databaseType
    ) where TContextType : IntersectDbContext<TContextType>
    {
        var dbContextAbstractType = typeof(TContextType);
        if (dbContextAbstractType.FindGenericTypeParameters(typeof(IntersectDbContext<>)).First() != dbContextAbstractType)
        {
            throw new ArgumentException(
                $"Invalid generic type, it must directly extend IntersectDbContext<>: {dbContextAbstractType.FullName}"
            );
        }

        var dbContextConcreteType = databaseType switch
        {
            DatabaseType.MySql => dbContextAbstractType.FindConcreteType(type => type.Extends<IMySqlDbContext>()),
            DatabaseType.Sqlite => dbContextAbstractType.FindConcreteType(type => type.Extends<ISqliteDbContext>()),
            _ => throw new ArgumentOutOfRangeException(nameof(databaseType), databaseType, null)
        };

        if (dbContextConcreteType == default)
        {
            throw new ArgumentException(
                $"Failed to find concrete implementation of {dbContextAbstractType.FullName} for {databaseType}.",
                nameof(databaseType)
            );
        }

        var constructor = dbContextConcreteType.GetConstructor(
            BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance,
            _createContextParameters
                .Select(parameterInfo => parameterInfo.ParameterType)
                .ToArray()
        );

        if (constructor == default)
        {
            throw new MissingMethodException(
                $"Unable to find usable constructor with parameter types: {string.Join(", ", _createContextParameters.Select(parameterInfo => parameterInfo.ParameterType.FullName))}",
                dbContextConcreteType.Name
            );
        }

        var parameterExpressions = _createContextParameters.Select(
            parameterInfo => Expression.Parameter(
                parameterInfo.ParameterType,
                parameterInfo.Name
            )
        ).ToArray();

        var newExpression = Expression.New(
            constructor,
            parameterExpressions.OfType<Expression>()
        );
        var stronglyTypedLambdaExpression = Expression.Lambda(
            newExpression,
            parameterExpressions
        );
        var stronglyTypedInvocationExpression = Expression.Invoke(
            stronglyTypedLambdaExpression,
            parameterExpressions.OfType<Expression>()
        );
        var asDbContextExpression = Expression.TypeAs(
            stronglyTypedInvocationExpression,
            typeof(DbContext)
        );
        var weaklyTypedLambdaExpression = Expression.Lambda(
            asDbContextExpression,
            parameterExpressions
        );

        var compiledDelegate = weaklyTypedLambdaExpression.Compile();
        if (compiledDelegate is Func<DatabaseContextOptions, DbContext> castedDelegate)
        {
            return new(castedDelegate);
        }

        throw new InvalidOperationException();
    }

    private static KeyValuePair<DatabaseType, Type>[] DiscoverContextTypes(Assembly assembly)
    {
        try
        {
            return assembly.GetTypes()
                .Where(type => type.Extends<TDbContext>())
                .Select(type =>
                {
                    var providerAttribute = type.GetCustomAttribute<DbContextProviderAttribute>();
                    if (providerAttribute == default)
                    {
                        throw new ArgumentException(
                            $"Type does not have a [DbContextProvider] attribute: {type.FullName}",
                            nameof(type)
                        );
                    }

                    return new KeyValuePair<DatabaseType, Type>(
                        providerAttribute.DatabaseType,
                        type
                    );
                })
                .ToArray();
        }
        catch (Exception exception)
        {
            Log.Error(exception);
            return Array.Empty<KeyValuePair<DatabaseType, Type>>();
        }
    }

    public static void Register<TProviderContext>()
        where TProviderContext : TDbContext
    {
    }

    public static TDbContext Create(DatabaseContextOptions databaseContextOptions)
    {
        var dbContextType = typeof(TDbContext);
        if (!_constructorCache.TryGetValue((dbContextType, databaseContextOptions.DatabaseType),
                out var constructorDelegate))
        {
            constructorDelegate = CreateConstructorDelegate<TDbContext>(databaseContextOptions.DatabaseType);
            _constructorCache[(dbContextType, databaseContextOptions.DatabaseType)] = constructorDelegate;
        }

        return constructorDelegate(databaseContextOptions) as TDbContext ?? throw new InvalidOperationException();
    }
}
