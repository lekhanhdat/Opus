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
using AvePoint.Media.Service.DomainModel;

namespace RAArchiverCommon.Utility
{
    public class MigrationRecord
    {
        [CsvColumn("Site URL")]
        public required string SiteUrl { get; set; }

        [CsvColumn("SharePoint URL")]
        public required string SharePointUrl { get; set; }

        [CsvColumn("Source Storage Name")]
        public required string SourceStorageName { get; set; }

        [CsvColumn("Target Storage Name")]
        public required string TargetStorageName { get; set; }

        [CsvColumn("Blob Path")]
        public required string BlobPath { get; set; }

        [CsvColumn("Status")]
        public required string Status { get; set; }

        [CsvColumn("Size (Byte)")]
        public required long Size { get; set; }

        [CsvColumn("Action")]
        public required string Action { get; set; }

        [CsvColumn("Execution Date")]
        public DateOnly ExecutionDate { get; set; } = DateOnly.FromDateTime(DateTime.UtcNow);

        [CsvColumn("JobID")]
        public required string JobId { get; set; }

        [CsvColumn("Message")]
        public required string Message { get; set; }
    }
}
