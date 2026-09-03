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

namespace AvePoint.Wrapper.Common
{
    [System.AttributeUsage(System.AttributeTargets.Class, Inherited = false, AllowMultiple = true)]
    sealed class QueryCommandStringAttribute : System.Attribute
    {
        public QueryCommandType CommandType { get; private set; }
        public SPDatabaseVersion DatabaseVersion { get; private set; }

        /// <summary>
        /// QueryService中SQL Command的静态类
        /// </summary>
        /// <param name="version">SharePoint Version</param>
        /// <param name="type">操作类型</param>
        public QueryCommandStringAttribute(SPDatabaseVersion version,QueryCommandType type)
        {
            this.CommandType = type;
            this.DatabaseVersion = version;
        }


    }

    enum QueryCommandType : byte
    {
        None = 0,
        Select = 1,
        Insert = 2,
        Delete = 3,
        Update = 4,
    }

    enum SPDatabaseVersion : byte
    {
        None = 0,
        SharePoint2007 = 0x1,
        SharePoint2010 = 0x11,
        SharePoint2010WithSP1 = 0x12,
        SharePoint2013 = 0x21,
        SharePoint2016 = 0x31,
        SharePoint2016TAP1 = 0x3B,
    }
}
