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
using AvePoint.RA.Contract.JobMonitor;
using AvePoint.RA.Contract.RMWeb.JobMonitor;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AvePoint.RA.RAPhysical.Report
{
    public class BrowseOptions
    {
        /// <summary>
        /// if false, will shortcut and not browse to lower level.
        /// </summary>
        public bool NeedProcessRootLocation { get; set; } = true;
        /// <summary>
        /// if false, will shortcut and not browse to lower level.
        /// </summary>
        public bool NeedProcessNormalLocation { get; set; } = true;
        /// <summary>
        /// if false, will shortcut and not browse to lower level.
        /// </summary>
        public bool NeedProcessBottomLocation { get; set; } = true;
        /// <summary>
        /// if false, will shortcut and not browse to lower level.
        /// </summary>
        public bool NeedProcessContainer { get; set; } = false;
        /// <summary>
        /// if false, will shortcut and not browse to lower level.
        /// </summary>
        public bool NeedProcessBox { get; set; } = true;
        /// <summary>
        /// if false, will shortcut and not browse to lower level.
        /// </summary>
        public bool NeedProcessFile { get; set; } = true;
        /// <summary>
        /// if false, will not deal with record.
        /// </summary>
        public bool NeedProcessRecord { get; set; } = false;
    }

    public class ReportOptions
    {
        public string JobId { get; set; }
        public JobType JobType { get; set; }
        public string ProfileId { get; set; }

        public bool ProcessRecordItemsInParallel { get; set; } = true;
        public bool IsUseBuildInGetTreeNodesFunc { get; set; } = true;

        /// <summary>
        /// options while browsing tree
        /// </summary>
        public BrowseOptions BrowseOptions { get; set; } = new BrowseOptions();

        public bool IsUseBuiltInRootLocationAction { get; set; } = true;

        public bool IsUseBuiltInNormalLocationAction { get; set; } = true;

        public bool IsUseBuiltInBottomLocationAction { get; set; } = true;

        public bool IsUseBuiltInBoxAction { get; set; } = true;

        public bool IsUseBuiltInFileAction { get; set; } = true;

        public bool IsUseBuiltInRecordsGroupAction { get; set; } = true;
        public List<JMJobDetails> OtherDetails { get; set; }

    }
}
