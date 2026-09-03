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
using AvePoint.Wrapper.Common;

namespace AvePoint.Wrapper.Backup
{
    public interface IAveSPDoc
    {
        IAveSPSite AveSPSite { get; }
        IAveSPWeb AveSPWeb { get; }
        IAveSPFolder ParentFolder { get; }
        IAveSPItem AveSPItem { get; }
        string Url { get; }
        void ExportDocInfo(IAveBackupStream output);

        void ExportToExcel();
        void ExportStorgeInfo(IAveBackupStream output);
        /// <summary>
        /// 备份WebPart，以及WebPart关联User。
        /// </summary>
        /// <param name="output"></param>
        /// <param name="includeUsers">用于控制是否备份WebPart相关的User，例如Personal WebPart的User</param>
        /// <param name="onlyUnAvaiableUser">控制是否只备份无效User,只用于Granular Backup模块</param>
        void ExportWebParts(IAveBackupStream output, bool includeUsers = true, bool onlyUnAvaiableUser = false);
        /// <summary>
        /// 备份WebPart，WebPart关联User，以及WebPart属性中包含的Managed MetaData Service数据。
        /// </summary>
        /// <param name="output"></param>
        /// <param name="termBackupOption">只使用backupRelatedTermSets以及backupRelatedTermsOnly两个选项，其中一个为True时，备份WebPart相关的MMS data(ContentByQueryWebPart)</param>
        /// <param name="includeUsers">用于控制是否备份WebPart相关的User，例如Personal WebPart的User</param>
        /// <param name="onlyUnAvaiableUser">控制是否只备份无效User,只用于Granular Backup模块</param>
        void ExportWebParts(IAveBackupStream output, AveBackupOption backupOption, bool includeUsers = true, bool onlyUnAvaiableUser = false);
        void ExportAlerts(IAveBackupStream output, bool includeUsers = true, bool onlyUnAvaiableUser = false);
        void ExportSocialTags(IAveBackupStream output);
        void ExportSocialComments(IAveBackupStream output);
        void ExportContent(IAveBackupStream output, bool forceBackup = false);
        void ExportContent(IAveBackupStream output, IStreamConvertor streamConvertor, bool forceBackup = false);

        List<AveAlertInfo> GetAlerts();
        List<AveSocialTagInfo> GetSocialTags();
        List<AveSocialCommentInfo> GetSocialComments();

        void ExportSPComments(IAveBackupStream stream);

    }
}
