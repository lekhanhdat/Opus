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
using AvePoint.RA.Contract.Myhub.Items.Views;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AvePoint.RA.Contract.MyHub.Items.Views
{
    public class RMMyhubFolderAndFileItem : RMMyhubFolderDetailTableItem
    {
        public string PartitionKeyId { get; set; }
        public string CountryCode { get; set; }
        public string RecordId { get; set; }
        public string EndDate { get; set; }
        public string StartDate { get; set; }
        public string RetentionType { get; set; }
        public bool IsFolder { get; set; }
        public string ExtentionForFile { get; set; }
        public int ManualApprovedStatus { get; set; }
        public bool EnableRecordManagement { get; set; }
        public bool IsAllowDownloadRCC { get; set; }
        public bool IsActive { get; set; }
    }
    public class RMMyhubFolderAndFileItemResult
    {
        public List<RMMyhubFolderAndFileItem> Items { get; set; }
        public bool HasMore { get; set; }
        public string ContinuationToken { get; set; }
        public int Count { get; set; }
    }
}
