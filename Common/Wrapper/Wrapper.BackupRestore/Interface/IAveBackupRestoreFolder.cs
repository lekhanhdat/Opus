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

namespace AvePoint.Wrapper.BackupRestore
{
    public interface IAveBackupRestoreFolder : IAveBackupRestoreBase
    {
        event ExportFileAction FileMetadataExporting;
        event ExportFileAction FileMetadataExported;
        event ExportFileAction FileContentExporting;
        event ExportFileAction FileContentExported;
        event ExportFileAction FilteringOutFile;
        event ExportFileAction AddingReport;

        string Name { get; }
        string WebUrl { get; }
        string SeverRelativeUrl { get; }
        int Id { get; }
        Guid UniqueId { get; }
        Guid ParentListId { get; }
        List<IAveBackupRestoreFolder> GetSubFolders();
        void SetStreamConvertor(IStreamConvertor streamConvertor);
        List<ProcessResult> ExportFiles(IAveBackupStream stream, BackupOption options);
        void SetVersionCount(int versionCount);
    }
}
