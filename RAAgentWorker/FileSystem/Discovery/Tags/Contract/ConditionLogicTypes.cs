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
using System.Threading.Tasks;

namespace RAFileSystem.FileSystem.Discovery.Tags.Contract
{
    public enum TextConditionLogicType
    {
        None = 0,
        Contains = 1,
        DoesNotCoain = 2,
        Matches = 3,
        DoesNotMatch = 4,
        Equals = 5,
        DoesNotEqual = 6
    }

    public enum NumberConditionLogicType
    {
        None = 0,
        LessThanEquals = 1,
        GreaterThanEquals = 2,
        LessThan = 3,
        GreaterThan = 4,
        Equals = 5
    }

    public enum DateConditionLogicType
    {
        None = 0,
        Before = 1,
        OlderThan = 2,
        FromTo = 3,
    }

    public enum DateTimeConditionLogicType
    {
        None = 0,
        Before = 1,
        OlderThan = 2,
        FromTo = 3,
    }

    public enum ArrayConditionLogicType
    {
        None = 0,
        In = 1,
        NotIn = 2,
        TextMatchIn = 3,
        TextNotMatchIn = 4,
    }

    public enum BooleanConditionLogicType
    {
        None = 0,
        IsEmpty = 1,
    }

    public enum FileSizeConditionLogicType
    {
        None = 0,
        LessThanEquals = 1,
        GreaterThanEquals = 2,
        LessThan = 3,
        GreaterThan = 4,
        Equals = 5
    }

    public enum DuplicateConditionLogicType
    {
        None = 0,
        InField = 1,
    }

    public enum VersionConditionLogicType
    {
        None = 0,
        MajorAndMinor = 1,
        MajorAndNoMinor = 2,
        MinorVersionOfEachMajor = 3,
        MinorVersionsOfLatestMajor = 4
    }
}
