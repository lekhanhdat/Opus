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

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using AvePoint.Wrapper.CustomUtility;
using AvePoint.GCommon.Contract.CommonFilter;
using System.Reflection;

namespace AvePoint.Wrapper.Common
{
    /// <summary>
    /// This is to implement the custom filter by dynamic code.
    /// </summary>
    public static class AveCustomFilter
    {
        /// <summary>
        /// Get the custom filter policy from the specified assembly according to the level (item or document)
        /// </summary>
        /// <param name="assembly">The byte[] of the assembly content</param>
        /// <param name="level">The filter level</param>
        /// <returns>The custom filter policy object</returns>
        /// <exception cref="ArgumentException">Will throw Will throw ArgumentException if something wrong when we try to get the custom filter policy</exception>
        public static AveCustomFilterPolicy GetCustomFilterPolicy(byte[] assembly, FilterLevel level)
        {
            return GetCustomFilterPolicy(assembly, string.Empty, level);
        }

        /// <summary>
        /// Get the custom filter policy from the specified assembly according to the full type name and the level (item or document)
        /// </summary>
        /// <param name="assembly">The byte[] of the assembly content</param>
        /// <param name="fullTypeName">Full type name of the custom class</param>
        /// <param name="level">The filter level</param>
        /// <returns>The custom filter policy object</returns>
        /// <exception cref="ArgumentException">Will throw ArgumentException if something wrong when we try to get the custom filter policy</exception>
        public static AveCustomFilterPolicy GetCustomFilterPolicy(byte[] assembly, string fullTypeName, FilterLevel level)
        {
            Assembly ass = Assembly.Load(assembly);
            if (ass == null)
            {
                throw new ArgumentException("This is not a valid assembly.");
            }

            ConstructorInfo ci = null;
            if (!string.IsNullOrEmpty(fullTypeName))
            {
                Type t = ass.GetType(fullTypeName);
                if (t == null)
                {
                    throw new ArgumentException(string.Format("There is no expected type ({0}) in the assembly.", fullTypeName));
                }

                Type baseIntr = t.GetInterface(typeof(IAveCustomFilter).ToString());
                if (baseIntr == null)
                {
                    throw new ArgumentException("The base interface of the class is not expected.");
                }
                ci = t.GetConstructor(new Type[0]);
            }
            else
            {
                foreach (Type t in ass.GetTypes())
                {
                    Type baseIntr = t.GetInterface(typeof(IAveCustomFilter).ToString());
                    if (baseIntr != null)
                    {
                        ci = t.GetConstructor(new Type[0]);
                        break;
                    }
                }
            }
            return ci != null ? (ci.Invoke(null) as IAveCustomFilter).GetCustomFilters(level) : null;
        }

        /// <summary>
        /// Convert the AveCustomFilterPolicy to List<FilterPolicy>
        /// </summary>
        /// <param name="customFilterPolicy">The AveCustomFilterPolicy object</param>
        /// <returns>The collection of filter policy objects</returns>
        /// <exception cref="ArgumentNullException">Will throw ArgumentNullException if the parameter is null</exception> 
        public static List<FilterPolicy> ConvertCustomFilterPolicyToDocAve(AveCustomFilterPolicy customFilterPolicy)
        {
            List<FilterPolicy> policies = new List<FilterPolicy>();

            foreach (var condition in customFilterPolicy.Conditions)
            {
                FilterPolicy fp = new FilterPolicy();
                if (condition is AveContentTypeCondition)
                {
                    AveContentTypeCondition ctc = condition as AveContentTypeCondition;
                    if(ctc == null)
                    {
                        continue;
                    }
                    CustomContentTypeRule rule = new CustomContentTypeRule()
                    {
                        Value1 = ctc.ContentTypeName,
                        //FilterOut = ctc.FilterOut,
                    };
                    fp.Value = new PolicyValue(rule.Value1);
                    fp.Level = (PolicyLevel)customFilterPolicy.Level;
                    fp.SequenceNo = ctc.SequenceNo;
                    fp.Condition = (PolicyCondition)Enum.Parse(typeof(PolicyCondition), ctc.CmpAction.ToString(), true);
                    fp.Rule = rule;
                }
                else if (condition is AveFieldCondition)
                {
                    AveFieldCondition fc = condition as AveFieldCondition;
                    if (fc == null)
                    {
                        continue;
                    }
                    CustomColumnRule rule = new CustomColumnRule()
                    {
                        InternalName = fc.FieldInternalName,
                        DisplayName = fc.FieldDisplayName,
                        FieldType = fc.ColumnType != null ? fc.ColumnType.ToString() : null,
                        Value1 = string.IsNullOrEmpty(fc.FieldInternalName) ? fc.FieldDisplayName : fc.FieldInternalName,
                        //FilterOut = fc.FilterOut
                    };
                    fp.Value = new PolicyValue(fc.FdValue.Value1, (PolicyValueUnit)fc.FdValue.Value1Unit, fc.FdValue.Value2, (PolicyValueUnit)fc.FdValue.Value2Unit);
                    fp.Level = (PolicyLevel)customFilterPolicy.Level;
                    fp.SequenceNo = fc.SequenceNo;
                    fp.Condition = (PolicyCondition)Enum.Parse(typeof(PolicyCondition), fc.CmpAction.ToString(), true);
                    fp.Rule = rule;
                }
                else
                {
                    continue;
                }
                policies.Add(fp);
            }

            return policies;
        }

        public static Dictionary<PolicyLevel, string> GetCustomFilterExpression(AveCustomFilterPolicy customFilterPolicy)
        {
            Dictionary<PolicyLevel, string> expression = new Dictionary<PolicyLevel, string>();
            if (customFilterPolicy != null && !string.IsNullOrEmpty(customFilterPolicy.ExpressionString))
            {
                expression.Add((PolicyLevel)customFilterPolicy.Level, customFilterPolicy.ExpressionString);
            }
            return expression;
        }
    }
}
