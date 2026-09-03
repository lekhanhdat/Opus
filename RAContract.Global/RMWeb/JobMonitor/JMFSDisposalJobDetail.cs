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

namespace AvePoint.RA.Contract.RMWeb.JobMonitor
{
    //TODO HYW  MOVE THIS CLASS OUT OF THIS PROJECT
    public class JMFSDisposalJobDetails : JMJobDetails
    {
        //public string DetailTab { get; set; }
        public string Type { get; set; }
        public string ObjectName { get; set; }
        public string Size { get; set; }
        public string SourceLocation { get; set; }
        public string DestinationLocation { get; set; }
        public string FinishTime { get; set; }
        public string RuleName { get; set; }
        public string Action { get; set; }     
        public string AgentName { get; set; }
    }

    public class JMFSDisposalJobDetailV2 : JMFSDisposalJobDetails
    {
        //public long StartTime { get; set; }
        public long Depth { get; set; }

        public string DirPath { get; set; } // fullPath for folder, parent path for file

        public int DetailAction { get; set; }
    }
}
