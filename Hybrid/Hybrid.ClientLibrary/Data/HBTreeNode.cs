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
using System.Runtime.Serialization;
using System.Text;

namespace AvePoint.Hybrid.ClientLibrary.Data
{
    [DataContract]
    public class HBTreeNode
    {
        [DataMember]
        public string Id { set; get; }
        [DataMember]
        public string Name { set; get; }
        [DataMember]
        public string Url { set; get; }
        [DataMember]
        public string BatchId { set; get; }

        public override string ToString()
        {
            return $"{base.ToString()}\t{Id}\t{Name}\t{Url}\t{BatchId}";
        }
    }
}
