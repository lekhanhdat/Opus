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
    #endregion

    internal static class VersionConditionChecker
    {
        public static bool IsQualified(PolicyCondition policyCondition, VersionedObjectInfoBase verionedObj, PolicyValue policyValue)
        {
            switch (policyCondition)
            {
                case PolicyCondition.OnlyLastNVersions:
                    int lastVersionCount = int.Parse(policyValue.Value1);
                    return verionedObj.VersionSequenceNo <= lastVersionCount;
                case PolicyCondition.ExceptLastNVersions:
                case PolicyCondition.MajorAndMintorVersions:
                    int leaveLastVersionCount = int.Parse(policyValue.Value1);
                    return verionedObj.VersionSequenceNo >= leaveLastVersionCount;
                case PolicyCondition.MajorWithoutMinorVersions:
                    if (verionedObj.MajorVersionSequenceNo == int.MaxValue)
                    {
                        return true;
                    }
                    int leaveMajorVersionCount = int.Parse(policyValue.Value1);
                    return verionedObj.MajorVersionSequenceNo >= leaveMajorVersionCount;
                case PolicyCondition.MinorOfEachMajorVersion:
                    int leaveLastMinorVersionCount = int.Parse(policyValue.Value1);
                    if (verionedObj.MajorVersionSequenceNo != int.MaxValue)
                    {
                        return false;
                    }
                    return verionedObj.CurrentMinorVersionSequenceNo >= leaveLastMinorVersionCount;
                case PolicyCondition.MinorOfTheLatestMajorVersion:
                    int MinorOfLastMajorVersions = int.Parse(policyValue.Value1);
                    if (verionedObj.MajorVersionSequenceNo != int.MaxValue)
                    {
                        return false;
                    }
                    if (!verionedObj.IsLastMajorVersion)
                    {
                        return true;
                    }
                    return verionedObj.CurrentMinorVersionSequenceNo >= MinorOfLastMajorVersions;
                case PolicyCondition.OnlyLastMajorNVersions:
                    int leaveLastMajorVersionCount = int.Parse(policyValue.Value1);
                    return verionedObj.MajorVersionSequenceNo <= leaveLastMajorVersionCount;
                case PolicyCondition.ExceptLastNMajorVersions:
                    int lastMajorVersionCount = int.Parse(policyValue.Value1);
                    return verionedObj.MajorVersionSequenceNo >= lastMajorVersionCount;
                case PolicyCondition.OnlyMajorVersions:
                    return verionedObj.UIVersion % 512 == 0;
                case PolicyCondition.OnlyMionrVersions:
                    return verionedObj.UIVersion % 512 != 0;
                case PolicyCondition.OnlyApproved:
                    return verionedObj.Approved == true;
                case PolicyCondition.Exactly:
                case PolicyCondition.Contains:
                case PolicyCondition.StartWith:
                case PolicyCondition.EndWith:
                case PolicyCondition.LessOrEqualThan:
                case PolicyCondition.GreaterOrEqualThan:
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
