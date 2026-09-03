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

namespace AvePoint.GCommon.Contract.ErrorCode
{
    internal partial class ErrorCodeSource
    {
        internal static Dictionary<TroubleshootingErrorCode, Dictionary<SourceType, List<string>>> ErrorCodeSource_Site = new Dictionary<TroubleshootingErrorCode, Dictionary<SourceType, List<string>>>
        {
            {
                TroubleshootingErrorCode.SP_SiteLocked,
                new Dictionary<SourceType, List<string>>
                {
                    { SourceType.ReportCommentKey, new List<string> { "Service.Common_3598a06e-e11e-4b3b-95db-0a7c5778ffb0" } }
                }
            },
            {
                TroubleshootingErrorCode.SP_SiteNotExist,
                new Dictionary<SourceType, List<string>>
                {
                    { SourceType.ReportCommentKey, new List<string> { "Item_SiteNotExist" } }
                }
            },
            {
                TroubleshootingErrorCode.SP_PDFBackupFailedDueToIRM,
                new Dictionary<SourceType, List<string>>
                {
                    { SourceType.ErrorMessage, new List<string> { "The PDF document you tried to upload has a feature that is incompatible with SharePoint IRM and cannot be uploaded to IRMed folder", "アップロードしようとした PDF ドキュメントは SharePoint で暗号化されていないため、このライブラリにアップロードできません" } }
                }
            },
            {
                TroubleshootingErrorCode.SP_FileBackupFailedDueToVirusScanner,
                new Dictionary<SourceType, List<string>>
                {
                    { SourceType.ErrorMessage, new List<string> { "The virus scanner discovered an issue while scanning the file", "ウイルス検索プログラムによってファイルに問題が検出されました" } }
                }
            },
            {
                TroubleshootingErrorCode.SP_WebPartNotExist,
                new Dictionary<SourceType, List<string>>
                {
                    { SourceType.ErrorMessage, new List<string> { "The operation could not be completed because the Web Part is not on this page", "Web パーツがこのページにないので、操作を完了できませんでした" } }
                }
            },
            {
                TroubleshootingErrorCode.SP_SkipBackupRecordingsFolder,
                new Dictionary<SourceType, List<string>>
                {
                    { SourceType.ReportCommentKey, new List<string> { "Wrapper_SkipRecordingFiles" } }
                }
            },
            {
                TroubleshootingErrorCode.SP_IRMProtectedFileFailed,
                new Dictionary<SourceType, List<string>>
                {
                    { SourceType.ReportCommentKey, new List<string> { "Wrapper_IRMUnprotectFileFailed" } }
                }
            },
        };
    }
}