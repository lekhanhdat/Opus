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

namespace AvePoint.Wrapper.Common
{
    public class EventObject
    {
        public DateTime EventTime;

        public long Id;

        public Guid SiteId;

        public Guid WebId;

        public Guid ListId;

        public int ItemId;

        public Guid DocId;

        public Guid Guid0;

        public int Int0;

        public int Int1;

        public string ContentTypeId;

        public string ItemName;

        public string ItemFullUrl;

        public int EventType;

        public int ObjectType;

        public string ModifiedBy;

        public DateTime TimeLastModified;

        public byte[] EventData;

        public byte[] ACL;

        public byte[] DocClientId;

    }

    public class DocObject : IEquatable<DocObject> 
    { 
        public Guid Id;

        public string LeafName;

        public int DoclibRowId;

        public byte Type;

        public string DirName;

        public DateTime TimeLastModified;

        public int UIVersion;

        public long Size;

        public byte Level;

        public int? CheckoutUserId;

        public int DocFlags;

        public bool IsCurrentVersion;

        public Guid ParentId;

        public Guid ListId;

        public int HasStream; //Just For Extender

        public int QueryType; //Just For Extender

        public byte[] RbsId; //Just For Extender

        public byte[] Content; //Just For Extender

        public bool Equals(DocObject other)
        {
            return this.Id == other.Id;
        }

        public override int GetHashCode()
        {
            int nameHashCode = this.LeafName == null ? 0 : this.LeafName.GetHashCode();

            int idHashCode = this.Id.GetHashCode();

            return nameHashCode ^ idHashCode;
        }  
    }
}
