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
    using System.Text;
    using AvePoint.GCommon.Contract.CommonFilter;
    using AvePoint.GCommon;
    #endregion

    internal class NumberConditionChecker
    {
        public static bool IsQualified(PolicyCondition policyCondition, long objectValue, PolicyValue policyValue)
        {
            double criteria = double.Parse(policyValue.Value1);
            switch (policyValue.Value1Unit)
            {
                case PolicyValueUnit.KB:
                    criteria *= 1024;
                    break;
                case PolicyValueUnit.MB:
                    criteria *= 1024 * 1024;
                    break;
                case PolicyValueUnit.GB:
                    criteria *= 1024 * 1024 * 1024;
                    break;
                case PolicyValueUnit.Days:
                    criteria *= 1;
                    break;
                case PolicyValueUnit.None:
                    break;
                case PolicyValueUnit.Weeks:
                case PolicyValueUnit.Months:
                case PolicyValueUnit.Years:
                    throw new PolicyValueUnitNotSupportedException(policyValue.Value1Unit.ToString());
                default:
                    break;
            }
            switch (policyCondition)
            {
                case PolicyCondition.LessOrEqualThan:
                    return ConditionChecker.LessOrEqualThan(objectValue, criteria);
                case PolicyCondition.GreaterOrEqualThan:
                    return ConditionChecker.BiggerOrEqualThan(objectValue, criteria);
                case PolicyCondition.Equals:
                    return ConditionChecker.Equal(objectValue, criteria);
                case PolicyCondition.Exactly:
                case PolicyCondition.Contains:
                case PolicyCondition.StartWith:
                case PolicyCondition.EndWith:
                case PolicyCondition.OnlyLastNVersions:
                case PolicyCondition.OnlyLastMajorNVersions:
                case PolicyCondition.OnlyMajorVersions:
                case PolicyCondition.OnlyApproved:
                case PolicyCondition.FromTo:
                case PolicyCondition.Before:
                case PolicyCondition.After:
                case PolicyCondition.On:
                case PolicyCondition.WithIn:
                default:
                    throw new ConditionNotSupportedException(policyCondition.ToString());
            }
        }

        public static bool IsQualified(PolicyCondition policyCondition, double objectValue, PolicyValue policyValue)
        {
            double criteria = double.Parse(policyValue.Value1);
            switch (policyCondition)
            {
                case PolicyCondition.LessOrEqualThan:
                    return ConditionChecker.LessOrEqualThan(objectValue, criteria);
                case PolicyCondition.GreaterOrEqualThan:
                    return ConditionChecker.BiggerOrEqualThan(objectValue, criteria);
                case PolicyCondition.Equals:
                    return ConditionChecker.Equal(objectValue, criteria);
                case PolicyCondition.Exactly:
                case PolicyCondition.Contains:
                case PolicyCondition.StartWith:
                case PolicyCondition.EndWith:
                case PolicyCondition.OnlyLastNVersions:
                case PolicyCondition.OnlyLastMajorNVersions:
                case PolicyCondition.OnlyMajorVersions:
                case PolicyCondition.OnlyApproved:
                case PolicyCondition.FromTo:
                case PolicyCondition.Before:
                case PolicyCondition.After:
                case PolicyCondition.On:
                case PolicyCondition.WithIn:
                default:
                    throw new ConditionNotSupportedException(policyCondition.ToString());
            }
        }
    }
}
