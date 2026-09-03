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
using AvePoint.GCommon.Contract.Server.GranularRestore.Object;

namespace AvePoint.RA.Contract.RMWeb.JobMonitor
{
    public class JMRestoreActionJobDetailes: JMJobDetails
    {
        private string sourceLocation;
        private string path;
        public string Level { get; set; }
        public string SourceLocation
        {
            get
            {
                return sourceLocation.Replace("\\", "/");
            }
            set
            {
                sourceLocation = value;
            }
        }
        public string Path
        {
            get
            {
                return path.Replace("\\", "/");
            }
            set
            {
                path = value == null?string.Empty:value;
            }
        }

        public int ConflictResolution { get; set; }
        public string Size { get; set; }
        public string SizeStr { get; set; }
        public long FinishTime { get; set; }
        public string FinishTimeStr { get; set; }
        public string PolicyLevel { get; set; }
        public string PathMd5 { get; set; }
        public string DestinationUrl { get; set; }
    }

    public class JMMigrationRestoreActionJobDetailes : JMRestoreActionJobDetailes
    {
        public long StartTime { get; set; } // support sort job detail for job that have inconsistent finish time
    }

    public class JMAOSPRestoreActionJobDetailes : JMJobDetails
    {
        private string sourceLocation;
        private string path;
        public string Level { get; set; }
        public ActionTab ActionTab { get; set; }
        public RestoreConflictResolution ConflictResolution { get; set; }
        public string SourceLocation
        {
            get
            {
                return sourceLocation.Replace("\\", "/");
            }
            set
            {
                sourceLocation = value;
            }
        }
        public string Path
        {
            get
            {
                return path?.Replace("\\", "/");
            }
            set
            {
                path = value == null ? string.Empty : value;
            }
        }
        public string Size { get; set; }
        public string SizeStr { get; set; }
        public long FinishTime { get; set; }
        public string FinishTimeStr { get; set; }
    }

    public class JMGDriveRestoreActionJobDetail : JMRestoreActionJobDetailes
    {
        public string DriveId { get; set; }
    }

    public class JMHSMArchiverJobDetailes : JMJobDetails
    {
        private string sourceLocation;
        public string SourceLocation
        {
            get
            {
                return sourceLocation.Replace("\\", "/");
            }
            set
            {
                sourceLocation = value;
            }
        }
    }
}
