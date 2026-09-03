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

namespace AvePoint.Wrapper.Common
{
    public class AveVersionObject
    {
        public int ID { get; set; } //DocLibRowID
        public DateTime TimeLastModified { get; set; }
        public int Uiversion { get; set; }
        public int InternalVersion { get; set; }
        public string UiVersionString { get; set; }
        public bool IsCurrentVersion { get; set; }//该值对应AllUserdata表中的tp_IsCurrent 
        public bool Tp_IsCurrentVersion { get; set; }//该值对应AllUserdata表中的tp_IsCurrentVersion
        public byte Level { get; set; }
        public ItemType ObjType { get; set; }
        public Guid UserDataGuid { get; set; }
        public long Size { get; set; }
        public int? DocFlags { get; set; }
        public int QueryType { get; set; }//Just For Extender. 2 is from Alldocs,3 is from alldocversions
        public byte[] Content { get; set; } //Just For Extender
        public bool HasStream { get; set; }
        public byte[] RbsId { get; set; }//Just For Extender
        public byte[] DeleteTransactionId { get; set; }//Just For Extender
    }
}
