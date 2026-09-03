using System.Collections;
using AvePoint.Common.FilterEngine;
using AvePoint.GCommon.Contract.CommonFilter;

namespace CommonUtility30.Tests;

public sealed class FilterEnginePropertyGuardTests
{
    [Theory]
    [InlineData("https://contoso.example/sites/records", "contoso", true)]
    [InlineData("https://example.test/sites/records", "contoso", false)]
    public void IsQualified_AssignedProperty_PreservesRuleOutcome(string url, string expectedValue, bool expected)
    {
        var info = new WebAppInfo { Url = url };
        FilterEngine engine = CreateEngine(PolicyLevel.WebApplication, new UrlRule(), PolicyCondition.Contains, expectedValue);

        Assert.Equal(expected, engine.IsQualified(info));
        Assert.Equal(url, info.Url);
    }

    [Fact]
    public void IsQualified_UnassignedExecutedProperty_ThrowsAndRestoresScope()
    {
        var info = new WebAppInfo();
        FilterEngine engine = CreateEngine(PolicyLevel.WebApplication, new UrlRule(), PolicyCondition.Contains, "contoso");

        PropertyNotAssignedException exception = Assert.Throws<PropertyNotAssignedException>(() => engine.IsQualified(info));

        Assert.Equal(typeof(WebAppInfo), exception.ObjectType);
        Assert.Equal(nameof(WebAppInfo.Url), exception.PropertyName);
        Assert.Null(info.Url);
    }

    [Fact]
    public void IsQualified_OrRuleSkipsUnneededGetters()
    {
        var info = new DocumentInfo { ModifiedByLogonName = "matching user" };
        FilterEngine engine = CreateEngine(PolicyLevel.Document, new ModifiedByRule(), PolicyCondition.Contains, "matching");

        Assert.True(engine.IsQualified(info));
    }

    [Fact]
    public void IsQualified_OrRuleChecksTheNextGetterWhenNeeded()
    {
        var info = new DocumentInfo { ModifiedByLogonName = "different user" };
        FilterEngine engine = CreateEngine(PolicyLevel.Document, new ModifiedByRule(), PolicyCondition.Contains, "matching");

        PropertyNotAssignedException exception = Assert.Throws<PropertyNotAssignedException>(() => engine.IsQualified(info));

        Assert.Equal(nameof(DocumentInfo.ModifiedByTitle), exception.PropertyName);
    }

    [Fact]
    public void IsQualified_BroadBusinessCatchUsesExistingFailureResult()
    {
        var info = new ListInfo
        {
            ColumnInfos = new Hashtable { ["number"] = "42" }
        };
        var expected = new PropertyNotAssignedException(typeof(ListInfo), nameof(ListInfo.ColumnInfos));
        info.AddPropertyCheckPolicy(new ThrowOnPropertyReadPolicy(nameof(ListInfo.ColumnInfos), 3, expected));
        FilterEngine engine = CreateEngine(
            PolicyLevel.List,
            new CustomPropertyNumberRule { Value1 = "number" },
            PolicyCondition.Equals,
            "42");

        Assert.False(engine.IsQualified(info));
    }

    [Fact]
    public void IsQualified_ReflectiveBusinessBoundaryUsesExistingFailureResult()
    {
        var info = new DocumentInfo
        {
            ParentSiteColumnInfos = new Hashtable { ["number"] = "42" }
        };
        var expected = new PropertyNotAssignedException(typeof(DocumentInfo), nameof(DocumentInfo.ParentSiteColumnInfos));
        info.AddPropertyCheckPolicy(new ThrowOnPropertyReadPolicy(nameof(DocumentInfo.ParentSiteColumnInfos), 3, expected));
        FilterEngine engine = CreateEngine(
            PolicyLevel.Document,
            new PropertyBagNumberRule { Value1 = "number" },
            PolicyCondition.Equals,
            "42");

        Assert.False(engine.IsQualified(info));
    }

    [Fact]
    public void IsQualified_UnrelatedParseExceptionKeepsExistingFalseResult()
    {
        var info = new ListInfo
        {
            ColumnInfos = new Hashtable { ["number"] = "not-a-number" }
        };
        FilterEngine engine = CreateEngine(
            PolicyLevel.List,
            new CustomPropertyNumberRule { Value1 = "number" },
            PolicyCondition.Equals,
            "42");

        Assert.False(engine.IsQualified(info));
    }

    private static FilterEngine CreateEngine(
        PolicyLevel level,
        PolicyRuleBase rule,
        PolicyCondition condition,
        string value)
    {
        var policy = new FilterPolicy
        {
            SequenceNo = 1,
            Level = level,
            Rule = rule,
            Condition = condition,
            Value = new PolicyValue(value)
        };

        return new FilterEngine(
            [policy],
            new Dictionary<PolicyLevel, string> { [level] = "(1)" });
    }

    private sealed class ThrowOnPropertyReadPolicy : IPropertyCheckPolicy
    {
        private readonly string propertyName;
        private readonly int readNumber;
        private readonly PropertyNotAssignedException exception;
        private int readCount;

        public ThrowOnPropertyReadPolicy(
            string propertyName,
            int readNumber,
            PropertyNotAssignedException exception)
        {
            this.propertyName = propertyName;
            this.readNumber = readNumber;
            this.exception = exception;
        }

        public void Check(PropertyCheckContext context)
        {
            if (context.PropertyName == propertyName && ++readCount == readNumber)
            {
                throw exception;
            }
        }
    }
}