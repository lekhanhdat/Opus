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
using Cloud.Sdk.Data.Aos;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AvePoint.RA.Contract.JobMonitor
{
    public class JobExtension
    {
        public SOSCProgress soSCProgress { get; set; }

        public SOProgressFileAndSCCount SOProgressFileAndSCCount { get; set; }
    }

    public class SOSCProgress
    {
        public string fullPath { get;  set; }

    }

    public class SOProgressFileAndSCCount
    {
        public int AllSCCount { get; set; }
        public bool IsNewJob { get; set; } = false;
        #region was discard
        public int ProgressedSCCount { get; set; }
        public int ProgressedFileCount { get; set; }
        #endregion

        public int[] ProgressedSCCountArr { get; set; }
        public int[] ProgressedFileCountArr { get; set; }
        public long TotalArchivedSize { get; set; } = 0;
        public long EstimatedFinishTimeTicks { get; set; } = 0;
    }

    public class SimulateResotreResult
    {
        public string JobId { get; set; }
        public string StartTime { get; set; }
        public string UpdateTime { get; set; }
        public string FinishTime { get; set; }
        public long Size { get; set; }
        public string SizeStr { get => Util.SizeConvertUtil.GetDataSizeToView(Size); set { } }
        public Dictionary<int, long> LevelCountMap { get; set; }
        public string ErrorMessage { get; set; }
        public bool IsCompleted { get; set; }
    }

    // Display-order level used specifically by the preview restore data size feature
    // (ArchiverRestoreService.PreviewRestore -> AveItemPreviewRestoreMain.UpdateSimulateResult). Declared with
    // sequential values in the intended UI display order: SiteCollection, Site, List, Folder, Item, ItemVersion,
    // Document, DocumentVersion, Attachment. Kept separate from PolicyLevel (whose values are bit flags used for
    // filtering/combining elsewhere, and still relied on by the older simulate-restore feature's LevelCountMap
    // consumers) so ascending integer key order already matches the desired display order.
    public enum PreviewRestoreLevel
    {
        Unknown = 0,
        SiteCollection = 1,
        Site = 2,
        List = 3,
        Folder = 4,
        Item = 5,
        ItemVersion = 6,
        Document = 7,
        DocumentVersion = 8,
        Attachment = 9,
    }
}
