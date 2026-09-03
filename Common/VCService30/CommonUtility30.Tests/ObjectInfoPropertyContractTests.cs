using System.Reflection;
using AvePoint.Common.FilterEngine;

namespace CommonUtility30.Tests;

public sealed class ObjectInfoPropertyContractTests
{
    [Fact]
    public void EveryPublicReadWritePropertyInTheInheritanceTree_IsInterceptedAndIndependentlyBacked()
    {
        Type[] objectInfoTypes = typeof(ObjectInfoBase).Assembly
            .GetTypes()
            .Where(type => type.IsClass && !type.IsAbstract && type != typeof(ObjectInfoBase))
            .Where(type => typeof(ObjectInfoBase).IsAssignableFrom(type))
            .OrderBy(type => type.FullName)
            .ToArray();

        Assert.NotEmpty(objectInfoTypes);

        foreach (Type type in objectInfoTypes)
        {
            PropertyInfo[] properties = type
                .GetProperties(BindingFlags.Instance | BindingFlags.Public | BindingFlags.DeclaredOnly)
                .Where(property => property.CanRead && property.CanWrite && property.GetIndexParameters().Length == 0)
                .ToArray();

            foreach (PropertyInfo property in properties)
            {
                AssertIndependentBackingField(property);
                AssertInterceptedProperty(type, property);
            }
        }
    }

    [Fact]
    public void FileSystemTypes_ReuseCommonInfoNameProperty()
    {
        const BindingFlags flags = BindingFlags.Instance | BindingFlags.Public | BindingFlags.DeclaredOnly;

        Assert.Null(typeof(FSFileInfo).GetProperty(nameof(CommonInfoBase.Name), flags));
        Assert.Null(typeof(FSFolderInfo).GetProperty(nameof(CommonInfoBase.Name), flags));
        Assert.Equal(typeof(CommonInfoBase), typeof(FSFileInfo).GetProperty(nameof(CommonInfoBase.Name))?.DeclaringType);
        Assert.Equal(typeof(CommonInfoBase), typeof(FSFolderInfo).GetProperty(nameof(CommonInfoBase.Name))?.DeclaringType);
    }

    [Fact]
    public void InheritedGetter_ReportsTheRuntimeObjectType()
    {
        var info = new DocumentInfo();

        using IDisposable scope = info.BeginPropertyCheck();
        PropertyNotAssignedException exception = Assert.Throws<PropertyNotAssignedException>(() => _ = info.Name);

        Assert.Equal(typeof(DocumentInfo), exception.ObjectType);
        Assert.Equal(nameof(DocumentInfo.Name), exception.PropertyName);
    }

    private static void AssertIndependentBackingField(PropertyInfo property)
    {
        string backingFieldName = $"<{property.Name}>k__BackingField";
        FieldInfo? backingField = property.DeclaringType!.GetField(
            backingFieldName,
            BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.DeclaredOnly);

        Assert.True(
            backingField is not null
                && backingField.FieldType == property.PropertyType
                && backingField.IsDefined(typeof(System.Runtime.CompilerServices.CompilerGeneratedAttribute), false),
            $"{property.DeclaringType.FullName}.{property.Name} must use its compiler-generated backing field.");
    }

    private static void AssertInterceptedProperty(Type type, PropertyInfo property)
    {
        var info = Assert.IsAssignableFrom<ObjectInfoBase>(Activator.CreateInstance(type));

        using (info.BeginPropertyCheck())
        {
            TargetInvocationException invocationException = Assert.Throws<TargetInvocationException>(() => property.GetValue(info));
            var propertyException = Assert.IsType<PropertyNotAssignedException>(invocationException.InnerException);
            Assert.Equal(type, propertyException.ObjectType);
            Assert.Equal(property.Name, propertyException.PropertyName);
        }

        object expected = CreateAssignedValue(property.PropertyType, property.Name);
        property.SetValue(info, expected);

        using (info.BeginPropertyCheck())
        {
            object actual = property.GetValue(info);
            if (property.PropertyType.IsValueType)
            {
                Assert.Equal(expected, actual);
            }
            else
            {
                Assert.Same(expected, actual);
            }
        }
    }

    private static object CreateAssignedValue(Type type, string propertyName)
    {
        if (type == typeof(string))
        {
            return propertyName;
        }

        if (type == typeof(bool))
        {
            return true;
        }

        if (type == typeof(int))
        {
            return 17;
        }

        if (type == typeof(long))
        {
            return 29L;
        }

        if (type == typeof(DateTime))
        {
            return new DateTime(2026, 8, 21, 12, 0, 0, DateTimeKind.Utc);
        }

        if (type.IsEnum)
        {
            Array values = Enum.GetValues(type);
            return values.GetValue(values.Length - 1)!;
        }

        return Activator.CreateInstance(type)
            ?? throw new InvalidOperationException($"Cannot create a test value for {type.FullName}.");
    }
}