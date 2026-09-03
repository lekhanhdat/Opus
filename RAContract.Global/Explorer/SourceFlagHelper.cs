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
using System.Collections.Generic;

namespace AvePoint.RA.Contract.Explorer
{
    public static class SourceFlagHelper
    {

        public static List<SourceFlag> GetDefaultContainerIdSource()
        {
            return new List<SourceFlag> { SourceFlag.Exchange, SourceFlag.SharePoint, SourceFlag.OneDrive, SourceFlag.Teams, SourceFlag.Physical };
        }

        public static List<SourceFlag> GetDefaultElectricSourceFlags()
        {
            return new List<SourceFlag> { SourceFlag.SharePoint, SourceFlag.Exchange, SourceFlag.FileSystem, SourceFlag.SharePointOnPrem, SourceFlag.OneDrive, SourceFlag.AzureFileShare, SourceFlag.Box, SourceFlag.Google, SourceFlag.Teams };
        }

        public static List<SourceFlag> GetAllSourceFlags()
        {
            return new List<SourceFlag> { SourceFlag.SharePoint, SourceFlag.FileSystem, SourceFlag.Exchange,  SourceFlag.Physical, SourceFlag.SharePointOnPrem, SourceFlag.OneDrive, SourceFlag.AzureFileShare, SourceFlag.Box, SourceFlag.Google, SourceFlag.Teams };
        }
        public static List<SourceFlag> GetNoContainerSourceFlags()
        {
            return new List<SourceFlag> { SourceFlag.FileSystem, SourceFlag.Physical, SourceFlag.SharePointOnPrem, SourceFlag.AzureFileShare, SourceFlag.Box };
        }
    }
}
