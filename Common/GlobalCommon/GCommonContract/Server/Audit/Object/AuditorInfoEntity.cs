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

namespace AvePoint.GCommon.Contract.Server.Audit.Object
{
    public class AuditorInfoEntity
    {
        public string PartitionKey { get; set; }
        public string RowKey { get; set; }

        public int Module { get; set; }

        public int ObjectType { get; set; }

        public int Action { get; set; }

        public string ObjectDetail { get; set; }

        public int Status { get; set; }

        public string Comment { get; set; }

        public string UserName { get; set; }

        public DateTime Time { get; set; }

        public string UserHostAddress { get; set; }

        /// <summary>
        /// AuditorEntityExtension的JSON序列化对象
        /// </summary>
        public string Extension { get; set; }

    }
}
