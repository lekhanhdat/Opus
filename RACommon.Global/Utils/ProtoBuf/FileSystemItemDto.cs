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
using ProtoBuf;

namespace AvePoint.RA.Common.Utils.ProtoBuf
{
    [ProtoContract]
    public class BatchPackage<T>
    {
        [ProtoMember(1)]
        public string BatchId { get; set; }
        [ProtoMember(2)]
        public string BatchFileName { get; set; }
        [ProtoMember(3)]
        public long BatchSize { get; set; }
        [ProtoMember(4)]
        public List<T> Items { get; set; } = new List<T>();
    }

    // NOTE: If using a Class type property:
    // 1. The target class must have [ProtoContract] and its props must have [ProtoMember].
    // 2. Avoid Circular References (Parent -> Child -> Parent).
    // 3. For complex/dynamic objects, can serialize to JSON String first.
}
