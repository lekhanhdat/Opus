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
using System.Web;
using AvePoint.RA.Contract.RMWeb.JobMonitor;
using AvePoint.RA.DB.Model;

namespace AvePoint.RA.Web.Models.JobMonitor
{
    public class JobMonitorModel
    {
        public List<PermissionElement> PermissionElements = new List<PermissionElement>();
        public string PermissionElementString
        {
            get
            {
                return string.Format("[{0}]", string.Join(",", PermissionElements.Select((s) => s.PermissionString)));
            }
        }
    }
    public class PermissionElement
    {
        public string PermissionString { get; set; }
    }

    public class PagerResultModel
    {
        public JMPager Pager { get; set; }
        public List<RMJobMonitor> Result { get; set; }
    }

}