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

using AvePoint.Wrapper.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace AvePoint.Wrapper.Core.SPBackup
{
    public delegate void SetWebInfoAction(AveWebInfo info);

    public interface ISPWebExport : ISPSocialExport, ISPRoleAssignmentsExport, IDisposable
    {
        void ExportBaseInfo(IAveBackupStream stream);
        /// <summary>在setListInfo里面修改ListInfo的数据</summary>
        void ExportBaseInfo(IAveBackupStream stream, SetWebInfoAction setWebInfo);
        void ExportFeatures(IAveBackupStream stream);
        void ExportSettings(IAveBackupStream stream);
        void ExportLanguageInfo(IAveBackupStream stream);
        void ExportFields(IAveBackupStream stream, SPWebFieldBackupOption backupColumnOption);
        void ExportContentTypes(IAveBackupStream stream, SPContentTypeBackupOption backupContentTypeOption);
        void ExportEventReceivers(IAveBackupStream stream);
        void ExportSearchInfo(IAveBackupStream stream);
        void ExportNavigation(IAveBackupStream stream, SPNavigationOption backupNavigationOption);
        void ExportRoles(IAveBackupStream stream);
    }
}
