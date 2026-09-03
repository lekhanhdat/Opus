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




namespace AvePoint.Media.Core.IO
{
    #region using directives
    using System;

    #endregion

    public class DataHeader
    {
        public String GUID { get; set; }
        public Int32 HeaderXmlLength { get; set; }
        public Int32 TailXmlLength { get; set; }
        public Int32 DataLength { get; set; }
        public Int32 InnerOffset { get; set; }
        public UInt32 Crc { get; set; }
        public Int32 SequenceNO { get; set; }
        public Byte HasMoreData { get; set; }
        public Byte DataMode { get; set; }

        public DataHeader()
        {
            GUID = "guid1111-2222-3333-4444-555566667777";
        }
    }
}
