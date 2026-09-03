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


namespace AvePoint.RA.Contract.MyHub.Model.Sort
{
    public class RMMyhubDriveSortInfo
    {
        private static readonly IReadOnlyDictionary<string, string> DriveSortsMapping = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
        {
            { "LastSyncTime","c.collectTime"},
            { "RetentionType","c.retentionType"},
            { "Size","c.jpmcFileSize"},
            { "Volume","c.jpmcFileSize"},
            { "ClassCode","c.classCode"},
            { "CountryCode","c.countryCode"},
            { "EventDate","c.eventDate"},
        };
        public static string GetDriveSortColumn(string columnName)
        {
            return DriveSortsMapping[columnName];
        }

    }
}
