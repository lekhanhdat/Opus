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
using AvePoint.RA.Contract.Explorer;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AvePoint.RA.Contract.CustomizeConnector.I18ns
{
    public class BuildInContentSourceI18Ns
    {
        public static readonly ImmutableDictionary<SourceFlag, string> SourceFlagI18ns = new Dictionary<SourceFlag, string>()
        {
            { SourceFlag.SharePoint, "RM_JS_SPS_TabLabel_SP" },
            { SourceFlag.SharePointOnPrem, "RM_JS_SPS_TabLabel_SPLocal" },
            { SourceFlag.Physical, "RM_JS_SPS_TabLabel_Physical" },
            { SourceFlag.OneDrive, "RM_JS_SPS_TabLabel_OneDrive" },
            { SourceFlag.FileSystem, "RM_JS_SPS_TabLabel_FS" },
            { SourceFlag.Exchange, "RM_JS_SPS_TabLabel_EXO" },
            { SourceFlag.AzureFileShare,"RM_JS_SPS_TabLabel_AF"},
            { SourceFlag.Box,"RM_JS_SPS_TabLabel_Box"},
            { SourceFlag.Google, "RM_JS_SPS_TabLabel_Google"},
            { SourceFlag.GGControl, "RM_JS_SPS_TabLabel_Google"},
            { SourceFlag.Teams, "RM_JS_SPS_TabLabel_Teams"},
        }.ToImmutableDictionary();

        public static readonly ImmutableDictionary<SourceFlag, string> SourceFlagIcons = new Dictionary<SourceFlag, string>()
        {
            { SourceFlag.SharePoint, "fi-ms-sharepoint" },
            { SourceFlag.SharePointOnPrem, "fia-sharepoint" },
            { SourceFlag.Physical, "fia-physical-record" },
            { SourceFlag.OneDrive, "fi-ms-onedrive" },
            { SourceFlag.FileSystem, "fia-file-system-c" },
            { SourceFlag.Exchange, "fi-ms-exchange" },
            { SourceFlag.AzureFileShare,"RM_JS_SPS_TabLabel_AF"},
            { SourceFlag.Box,"fia-box-blue-b"},
            { SourceFlag.Google,"fia-google-drive-f"},
            { SourceFlag.GGControl,"fia-google-drive-f"},
            { SourceFlag.Teams, "fi-ms-teams"},
        }.ToImmutableDictionary();
    }
}
