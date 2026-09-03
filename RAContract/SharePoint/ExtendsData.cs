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

namespace RMContract.SharePoint
{
    public class ExtendsData
    {
        public string Locked { get; set; }
        public string LockedBy { get; set; }
        public string UnLocked { get; set; }
        public string UnLockedBy { get; set; }

        public string KSUClass { get; set; }
        public string Reclassified { get; set; }
        public string ReclassifiedBy { get; set; }

        public string EventCreated { get; set; }
        public string EventModified { get; set; }
        public string EventDeleted { get; set; }
        public string EventCreatedBy { get; set; }
        public string EventModifiedBy { get; set; }
        public string EventDeletedBy { get; set; }
    }
}
