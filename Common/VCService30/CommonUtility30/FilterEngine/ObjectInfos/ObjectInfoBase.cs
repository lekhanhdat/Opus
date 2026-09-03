/********************************************************************
 *
 *  PROPRIETARY and CONFIDENTIAL
 *
 *  This file is licensed from, and is a trade secret of:
 *
 *                   AvePoint, Inc.
 *                   525 Washington Blvd, Suite 1400
 *                   Jersey City, NJ 07310
 *                   United States of America
 *                   Telephone: +1-201-793-1111
 *                   WWW: www.avepoint.com
 *
 *  Refer to your License Agreement for restrictions on use,
 *  duplication, or disclosure.
 *
 *  RESTRICTED RIGHTS LEGEND
 *
 *  Use, duplication, or disclosure by the Government is
 *  subject to restrictions as set forth in subdivision
 *  (c)(1)(ii) of the Rights in Technical Data and Computer
 *  Software clause at DFARS 252.227-7013 (Oct. 1988) and
 *  FAR 52.227-19 (C) (June 1987).
 *
 *  Copyright © 2017-2026 AvePoint® Inc. All Rights Reserved. 
 *
 *  Unpublished - All rights reserved under the copyright laws of the United States.
 */




namespace AvePoint.Common.FilterEngine
{
    #region using directives
    using System;
    using System.Collections.Generic;
    using System.Runtime.CompilerServices;
    #endregion

    public interface IPropertyCheckPolicy
    {
        void Check(PropertyCheckContext context);
    }

    public sealed class PropertyCheckContext
    {
        public PropertyCheckContext(ObjectInfoBase target, string propertyName, object propertyValue)
        {
            ArgumentNullException.ThrowIfNull(target);
            ArgumentException.ThrowIfNullOrEmpty(propertyName);

            Target = target;
            PropertyName = propertyName;
            PropertyValue = propertyValue;
            IsAssigned = target.IsPropertyAssigned(propertyName);
        }

        public ObjectInfoBase Target { get; }

        public string PropertyName { get; }

        public object PropertyValue { get; }

        public bool IsAssigned { get; }
    }

    public sealed class PropertyNotAssignedException : InvalidOperationException
    {
        public PropertyNotAssignedException(Type objectType, string propertyName)
            : base($"Property '{propertyName}' has not been assigned on object type '{objectType?.FullName}'.")
        {
            ArgumentNullException.ThrowIfNull(objectType);
            ArgumentException.ThrowIfNullOrEmpty(propertyName);

            ObjectType = objectType;
            PropertyName = propertyName;
        }

        public Type ObjectType { get; }

        public string PropertyName { get; }
    }

    public class ObjectInfoBase
    {
        private static readonly IPropertyCheckPolicy AssignmentPolicy = new PropertyAssignedCheckPolicy();
        private readonly HashSet<string> assignedProperties = new HashSet<string>(StringComparer.Ordinal);
        private readonly List<IPropertyCheckPolicy> customPolicies = new List<IPropertyCheckPolicy>();
        private int propertyCheckScopeDepth;

        public IDisposable BeginPropertyCheck()
        {
            propertyCheckScopeDepth++;
            return new PropertyCheckScope(this);
        }

        public static IDisposable BeginPropertyCheck(ObjectInfoBase target)
        {
            ArgumentNullException.ThrowIfNull(target);
            return target.BeginPropertyCheck();
        }

        public void AddPropertyCheckPolicy(IPropertyCheckPolicy policy)
        {
            ArgumentNullException.ThrowIfNull(policy);

            foreach (IPropertyCheckPolicy registeredPolicy in customPolicies)
            {
                if (ReferenceEquals(registeredPolicy, policy))
                {
                    return;
                }
            }

            customPolicies.Add(policy);
        }

        public bool RemovePropertyCheckPolicy(IPropertyCheckPolicy policy)
        {
            ArgumentNullException.ThrowIfNull(policy);

            for (int index = 0; index < customPolicies.Count; index++)
            {
                if (ReferenceEquals(customPolicies[index], policy))
                {
                    customPolicies.RemoveAt(index);
                    return true;
                }
            }

            return false;
        }

        protected T GetPropertyValue<T>(T propertyValue, [CallerMemberName] string propertyName = null)
        {
            if (propertyCheckScopeDepth == 0)
            {
                return propertyValue;
            }

            var context = new PropertyCheckContext(this, propertyName, propertyValue);
            
            // check built-in policies first, then check custom policies
            AssignmentPolicy.Check(context);

            foreach (IPropertyCheckPolicy policy in customPolicies)
            {
                policy.Check(context);
            }

            return propertyValue;
        }

        protected void MarkPropertyAssigned([CallerMemberName] string propertyName = null)
        {
            ArgumentException.ThrowIfNullOrEmpty(propertyName);
            assignedProperties.Add(propertyName);
        }

        protected void SetPropertyValue<T>(ref T field, T value, [CallerMemberName] string propertyName = null)
        {
            field = value;
            MarkPropertyAssigned(propertyName);
        }

        internal bool IsPropertyAssigned(string propertyName)
        {
            return assignedProperties.Contains(propertyName);
        }

        private void EndPropertyCheck()
        {
            propertyCheckScopeDepth--;
        }

        private sealed class PropertyAssignedCheckPolicy : IPropertyCheckPolicy
        {
            public void Check(PropertyCheckContext context)
            {
                if (!context.IsAssigned)
                {
                    throw new PropertyNotAssignedException(context.Target.GetType(), context.PropertyName);
                }
            }
        }

        private sealed class PropertyCheckScope : IDisposable
        {
            private ObjectInfoBase target;

            public PropertyCheckScope(ObjectInfoBase target)
            {
                this.target = target;
            }

            public void Dispose()
            {
                ObjectInfoBase currentTarget = target;
                if (currentTarget == null)
                {
                    return;
                }

                target = null;
                currentTarget.EndPropertyCheck();
            }
        }
    }
}
