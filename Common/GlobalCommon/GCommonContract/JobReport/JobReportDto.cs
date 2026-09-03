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

namespace Job.ModernManagement.Report;

using System;
using AvePoint.GCommon.Contract.Server.Job.Object;

public class JobReportDto
{
    public string Path { get; set; }

    public char Type { get; set; }

    public long Size { get; set; }

    public ReportStatus Status { get; set; }

    public string ErrorMessage { get; set; }

    public string Title { get; set; }

    public string Option { get; set; }

    public string ErrorCode { get; set; }

    public string Name { get; set; }

    public JobReportDetailEntityType EntityType { get; set; } = JobReportDetailEntityType.NormalInfo;

    public string ObjectTitle { get; set; }

    public string SourcePath { get; set; }

    public PropertyItem ErrorItem { get; set; }

    public long StartTime { get; set; }

    /// <summary>
    /// only used for calculate report time
    /// </summary>
    public DateTime BackupStartTime { get; set; } = DateTime.UtcNow;

    public string Id { get; set; }



    public override string ToString()
    {
#if DEBUG
        return $"[{Path}][{Type}][{Status}][{ErrorMessage}][{Id}][{Title}][{Size}]";
#else
            return ToDesensitizationString();
#endif

    }

    private string ToDesensitizationString()
    {
        return $"[{Type}][{Status}][{ErrorMessage}][{Id}][{Size}]";
    }
}
