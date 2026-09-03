using AvePoint.Common.FilterEngine;
using AvePoint.Common.FilterEngine.ObjectInfos;
using AvePoint.GCommon.Contract.CommonFilter;

namespace CommonUtility30.Tests;

public sealed class ObjectInfoPropertyGuardTests
{
    [Fact]
    public void UnassignedProperty_WhenCheckingIsDisabled_ReturnsDefaultWithoutRunningPolicies()
    {
        var info = new CommonInfoBase();
        var policy = new RecordingPolicy();
        info.AddPropertyCheckPolicy(policy);

        Assert.Null(info.Title);
        Assert.False(info.IsStub);
        Assert.Empty(policy.Contexts);
    }

    [Fact]
    public void UnassignedProperty_WhenCheckingIsEnabled_ThrowsDedicatedException()
    {
        var info = new CommonInfoBase();

        using IDisposable scope = info.BeginPropertyCheck();
        PropertyNotAssignedException exception = Assert.Throws<PropertyNotAssignedException>(() => _ = info.Title);

        Assert.Equal(typeof(CommonInfoBase), exception.ObjectType);
        Assert.Equal(nameof(CommonInfoBase.Title), exception.PropertyName);
    }

    [Fact]
    public void ExplicitClrDefaults_AreTreatedAsAssigned()
    {
        var info = new CommonInfoBase
        {
            Title = null,
            Name = string.Empty,
            IsStub = false,
            AccessTime = default
        };
        var teamsInfo = new TeamsInfo { Privacy = default };
        var versionInfo = new VersionedObjectInfoBase { VersionSequenceNo = 0 };

        using (info.BeginPropertyCheck())
        {
            Assert.Null(info.Title);
            Assert.Equal(string.Empty, info.Name);
            Assert.False(info.IsStub);
            Assert.Equal(default, info.AccessTime);
        }

        using (teamsInfo.BeginPropertyCheck())
        {
            Assert.Equal(default(PolicyValueUnit), teamsInfo.Privacy);
        }

        using (versionInfo.BeginPropertyCheck())
        {
            Assert.Equal(0, versionInfo.VersionSequenceNo);
        }
    }

    [Fact]
    public void CustomPolicies_RunInRegistrationOrderOnEveryRead()
    {
        var calls = new List<string>();
        var info = new CommonInfoBase { Title = "assigned" };
        var first = new RecordingPolicy(context => calls.Add($"first:{context.PropertyName}"));
        var second = new RecordingPolicy(context => calls.Add($"second:{context.PropertyName}"));
        info.AddPropertyCheckPolicy(first);
        info.AddPropertyCheckPolicy(first);
        info.AddPropertyCheckPolicy(second);

        using (info.BeginPropertyCheck())
        {
            _ = info.Title;
            _ = info.Title;
        }

        Assert.Equal(
            ["first:Title", "second:Title", "first:Title", "second:Title"],
            calls);
    }

    [Fact]
    public void AssignmentPolicy_RunsBeforeCustomPolicies()
    {
        var info = new CommonInfoBase();
        var policy = new RecordingPolicy();
        info.AddPropertyCheckPolicy(policy);

        using IDisposable scope = info.BeginPropertyCheck();
        Assert.Throws<PropertyNotAssignedException>(() => _ = info.Title);

        Assert.Empty(policy.Contexts);
    }

    [Fact]
    public void RemovePropertyCheckPolicy_OnlyRemovesRegisteredCustomInstance()
    {
        var info = new CommonInfoBase { Title = "assigned" };
        var registered = new RecordingPolicy();
        var equivalentButDifferent = new RecordingPolicy();
        info.AddPropertyCheckPolicy(registered);

        Assert.False(info.RemovePropertyCheckPolicy(equivalentButDifferent));
        Assert.True(info.RemovePropertyCheckPolicy(registered));
        Assert.False(info.RemovePropertyCheckPolicy(registered));

        using (info.BeginPropertyCheck())
        {
            Assert.Equal("assigned", info.Title);
        }

        Assert.Empty(registered.Contexts);
    }

    [Fact]
    public void NestedScopes_RestoreTheExactPriorStateAfterSuccessAndException()
    {
        var info = new CommonInfoBase();

        using (info.BeginPropertyCheck())
        {
            using (info.BeginPropertyCheck())
            {
                Assert.Throws<PropertyNotAssignedException>(() => _ = info.Title);
            }

            Assert.Throws<PropertyNotAssignedException>(() => _ = info.Title);
            Assert.Throws<InvalidOperationException>(() => ThrowInsideScope(info));
            Assert.Throws<PropertyNotAssignedException>(() => _ = info.Title);
        }

        Assert.Null(info.Title);
    }

    [Fact]
    public void RepeatedAssignment_RetainsLatestValueAndLifetimeAssignmentState()
    {
        var info = new CommonInfoBase { Title = "first" };
        info.Title = "second";

        using (info.BeginPropertyCheck())
        {
            Assert.Equal("second", info.Title);
        }

        using (info.BeginPropertyCheck())
        {
            Assert.Equal("second", info.Title);
        }
    }

    [Fact]
    public void AssignmentTracking_IsCaseSensitive()
    {
        var info = new CaseSensitiveInfo { Value = "assigned" };

        using IDisposable scope = info.BeginPropertyCheck();
        Assert.Equal("assigned", info.Value);
        PropertyNotAssignedException exception = Assert.Throws<PropertyNotAssignedException>(() => _ = info.value);

        Assert.Equal("value", exception.PropertyName);
    }

    [Fact]
    public void Context_IsImmutableAndContainsAllPolicyInputs()
    {
        var info = new CommonInfoBase { Title = "assigned" };
        var policy = new RecordingPolicy();
        info.AddPropertyCheckPolicy(policy);

        using (info.BeginPropertyCheck())
        {
            _ = info.Title;
        }

        PropertyCheckContext context = Assert.Single(policy.Contexts);
        Assert.Same(info, context.Target);
        Assert.Equal(nameof(CommonInfoBase.Title), context.PropertyName);
        Assert.Equal("assigned", context.PropertyValue);
        Assert.True(context.IsAssigned);
        Assert.True(typeof(PropertyCheckContext).IsSealed);
        Assert.All(typeof(PropertyCheckContext).GetProperties(), property => Assert.Null(property.SetMethod));
    }

    [Fact]
    public void GuardApis_RejectInvalidArguments()
    {
        var info = new CommonInfoBase();

        Assert.Throws<ArgumentNullException>(() => ObjectInfoBase.BeginPropertyCheck(null));
        Assert.Throws<ArgumentNullException>(() => info.AddPropertyCheckPolicy(null));
        Assert.Throws<ArgumentNullException>(() => info.RemovePropertyCheckPolicy(null));
        Assert.Throws<ArgumentNullException>(() => new PropertyCheckContext(null, "Title", null));
        Assert.Throws<ArgumentException>(() => new PropertyCheckContext(info, string.Empty, null));
    }

    private static void ThrowInsideScope(ObjectInfoBase info)
    {
        using IDisposable scope = info.BeginPropertyCheck();
        throw new InvalidOperationException("Expected test exception.");
    }

    private sealed class RecordingPolicy : IPropertyCheckPolicy
    {
        private readonly Action<PropertyCheckContext> onCheck;

        public RecordingPolicy(Action<PropertyCheckContext> onCheck = null)
        {
            this.onCheck = onCheck;
        }

        public List<PropertyCheckContext> Contexts { get; } = [];

        public void Check(PropertyCheckContext context)
        {
            Contexts.Add(context);
            onCheck?.Invoke(context);
        }
    }

    private sealed class CaseSensitiveInfo : ObjectInfoBase
    {
        private string upperValue;
        private string lowerValue;

        public string Value { get => GetPropertyValue(upperValue); set => SetPropertyValue(ref upperValue, value); }

#pragma warning disable IDE1006
        public string value { get => GetPropertyValue(lowerValue); set => SetPropertyValue(ref lowerValue, value); }
#pragma warning restore IDE1006
    }
}