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

namespace AvePoint.RA.Contract.RMWeb.JobMonitor
{
    public class JMTermSyncJobDetails : JMJobDetails
    {
        public string Term { get; set; }
        public string Action { get; set; }
        public string SiteCollectionURL { get; set; }
        public string MMSApplication { get; set; }
        public string AgentName { get; set; }

        //•	Term: 打出来这次 sync job中被操作的term的name
        //•	Action:说明这个term在这次job是什么操作，包括：New/Delete/Update/Deprecate/Expire
        //•	SharePoint Environment: 打出来被同步到了哪个SharePoint Farm下面（问题：后续版本对于SharePoint Online的站点，咱们支持显示到什么级别？）REMOVE
        //•	Managed Metadata Service Application:打出来被同步到了哪个MMS Application下面（问题：后续版本对于SharePoint Online的站点，咱们支持显示到什么级别？）
        //•	State: Successd/Failed
        //•	Comment: 打出来这个该行的详细信息，比如这个term没能成功sync过去的error message。如果无法获取相关信息，就留空

    }
}