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

namespace AvePoint.RA.Contract.RMWeb.JobMonitor
{
    public class JMSOJobSizeStatistics: JMJobDetails
    {
        private string sourceLocation;
        //private string path;
        //public string Level { get; set; }
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
        //public string Path
        //{
        //    get
        //    {
        //        return path.Replace("\\", "/");
        //    }
        //    set
        //    {
        //        path = value == null ? string.Empty : value;
        //    }
        //}
        public string Size { get; set; }
        //public string SizeStr { get; set; }
        public string FinishTime { get; set; }
        public int KeepDataOption { get; set; }
        public string Action { get; set; }
        public int AuthorID { get; set; }
        public string AuthorEmail { get; set; }
        public int ModifiedID { get; set; }
        public string ModifiedEmail { get; set; }
        public string CreateTime { get; set; }
        public string ModifiedTime { get; set; }
        public int VersionCount { get; set; }
        //public string FinishTimeStr { get; set; }
    }
}
