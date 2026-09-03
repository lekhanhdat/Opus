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




namespace AvePoint.Wrapper.Common
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;

    public class AveCustomContentTypeInfo
    {
        public string Name { get; set; }
    }
    /// <summary>
    /// 该类用来存储contenttype还原后源端和目的端的name的对应关系，保存在目的端的Schema中
    /// 在第二次还原时，会把Schemaload出来，拿该对应关系中的DestName与目的端的contenttype
    /// 进行比较，而不是用源端的name进行比较
    /// </summary>
    public class NameMapping
    {
        public string SourceName { get; set; }
        public string DestName { get; set; }
    }
}
