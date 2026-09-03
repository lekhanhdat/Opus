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
using AvePoint.RA.Contract.RMWeb.Audit;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AvePoint.RA.Contract.Audit
{
    /// <summary>
    /// have reviewed by allen yin
    /// </summary>
    public class AuditArg
    {
        public int Module { get; set; }
        public int Category { get; set; }
        public int Action { get; set; }
        public Type HandlerType { get; set; }
        public bool IsHandled { get; set; }
        public bool StartNewThread { get; set; }
        public object ReturnValue { get; set; }
        public System.Reflection.MethodInfo Method { get; set; }
        public object Target { get; set; }
        public object[] Args { get; set; }
        public RMAuditInfo auditInfo { get; set; }
        public string UserName { get; set; }
        public string ClientIP { get; set; }
        
    }
}
