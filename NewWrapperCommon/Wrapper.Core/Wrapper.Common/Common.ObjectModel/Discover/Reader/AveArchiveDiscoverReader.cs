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
using System.Data.SqlClient;

namespace AvePoint.Wrapper.Common
{
    public class AveArchiveDiscoverReader : AvePlatformRecoveryDiscoverReader
    {
        private static AveArchiveDiscoverReader mReader;
        private static object mLock = new object();

        protected AveArchiveDiscoverReader()
        {

        }

        public static AveArchiveDiscoverReader GetInstance()
        {
            if (mReader == null)
            {
                lock (mLock)
                {
                    if (mReader == null)
                    {
                        mReader = new AveArchiveDiscoverReader();
                    }
                }
            }
            return mReader;
        }

        public override string GetAttachmentsQueryString()
        {
            //doc.Id,doc.DirName,doc.LeafName,doc.Level,doc.UIVersion
            return AveDiscoverQueryString.AllAttachmentsForArchive;
        }

        public override string GetSingleItemAttachmentsQueryString()
        {
            return AveDiscoverQueryString.SingleAttachmentsForArcjover; 
        }

        public override void ReadAttachmentContent(AveItemObject obj, SqlDataReader sr)
        {
            obj.DocID = (Guid)sr["Id"];
            obj.DirName = (string)sr["DirName"];
            obj.SourceName = obj.LeafName = obj.ItemName = (string)sr["LeafName"];
            obj.FullUrl = (obj.DirName + "/" + obj.LeafName).Trim('/');
            obj.Level = (byte)sr["Level"];
            obj.Uiversion = (int)sr["UIVersion"];
            obj.ParentID = (Guid)sr["ParentId"];
        }
    }
}
