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

    internal static class BooleanConditionChecker
    {
        public static bool IsQualified(PolicyCondition policyCondition, bool objectValue, PolicyValue policyValue)
        {
            switch (policyCondition)
            {
                case PolicyCondition.Equals:
                case PolicyCondition.Exactly:
                    bool yes = string.Equals("yes", policyValue.Value1, StringComparison.OrdinalIgnoreCase);
                    return objectValue == yes;
                case PolicyCondition.Before:
                case PolicyCondition.After:
                case PolicyCondition.On:
                case PolicyCondition.WithIn:
                case PolicyCondition.Contains:
                case PolicyCondition.StartWith:
                case PolicyCondition.EndWith:
                case PolicyCondition.LessOrEqualThan:
                case PolicyCondition.GreaterOrEqualThan:
                case PolicyCondition.OnlyLastNVersions:
                case PolicyCondition.OnlyLastMajorNVersions:
                case PolicyCondition.OnlyMajorVersions:
                case PolicyCondition.OnlyApproved:
                default:
                    throw new ConditionNotSupportedException(policyCondition.ToString());
            }
        }
    }
}