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

namespace Media.Service.DomainModel
{
    public class DashBoardInfo
    {
        public long DocumentCount { get; set; }
        public long VersionCount { get; set; }
        public double VersionNumber { get; set; }
        public double FileNumber { get; set; }
        #region Teams&Group props
        public long PlannerCount { get; set; }
        public long PlanTaskCount { get; set; }
        public long PlanTaskAttachmentCount { get; set; }
        public long MessageCount { get; set; }
        public long MessageReplyCount { get; set; }
        public double PlannerNumber { get; set; }
        public double PlanTaskNumber { get; set; }
        public double PlanTaskAttachmentNumber { get; set; }
        public double MessageNumber { get; set; }
        public double MessageReplyNumber { get; set; }
        #endregion
    }
}
