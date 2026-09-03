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
    public class AveContentManagerDiscoverReader : AveDiscoverReader
    {
        private static AveContentManagerDiscoverReader mReader;
        private static object mLock = new object();

        protected AveContentManagerDiscoverReader()
        {

        }

        public static AveContentManagerDiscoverReader GetInstance()
        {
            if (mReader == null)
            {
                lock (mLock)
                {
                    if (mReader == null)
                    {
                        mReader = new AveContentManagerDiscoverReader();
                    }
                }
            }
            return mReader;
        }
                        
        public override string GetItemColumns()
        {
            return @" doc.Id,doc.LeafName,doc.DoclibRowId,doc.UIVersion as UIVersion,
doc.Level,doc.Type,doc.CheckoutUserId,doc.IsCurrentVersion,doc.TimeLastModified,doc.Size ";
        }

        public override void ReadItemContent(AveItemObject obj, IAveQueryDataReader sr)
        {
            obj.DocID = (Guid)sr["Id"];
            obj.SourceName = obj.LeafName = obj.ItemName = (string)sr["LeafName"];
            if (!(sr["DoclibRowId"] is DBNull))
            {
                obj.ID = (int?)sr["DoclibRowId"];
                obj.Hidden = false;
            }
            else
            {
                obj.Hidden = true;
            }
            obj.Uiversion = (int)sr["UIVersion"];
            obj.Level = (byte)sr["Level"];
            obj.Type = (byte)sr["Type"];
            if (!(sr["CheckoutUserId"] is DBNull))
            {
                obj.CheckoutUserId = (int?)sr["CheckoutUserId"];
            }
            obj.IsCurrentVersion = (bool)sr["IsCurrentVersion"];
            obj.TimeLastModified = (DateTime)sr["TimeLastModified"];
            if (!(sr["Size"] is DBNull))
            {
                obj.Size = long.Parse(sr["Size"].ToString());
            }
        }

        public override void ReadItemContent(AveItemObject obj, SqlDataReader sr)
        {
            obj.DocID = (Guid)sr["Id"];
            obj.SourceName = obj.LeafName = obj.ItemName = (string)sr["LeafName"];
            if (!(sr["DoclibRowId"] is DBNull))
            {
                obj.ID = (int?)sr["DoclibRowId"];
                obj.Hidden = false;
            }
            else
            {
                obj.Hidden = true;
            }
            obj.Uiversion = (int)sr["UIVersion"];
            obj.Level = (byte)sr["Level"];
            obj.Type = (byte)sr["Type"];
            if (!(sr["CheckoutUserId"] is DBNull))
            {
                obj.CheckoutUserId = (int?)sr["CheckoutUserId"];
            }
            obj.IsCurrentVersion = (bool)sr["IsCurrentVersion"];
            obj.TimeLastModified = (DateTime)sr["TimeLastModified"];
            if (!(sr["Size"] is DBNull))
            {
                obj.Size = long.Parse(sr["Size"].ToString());
            }
        }

        public override void ReadItemContentForIB(AveItemObject obj, SqlDataReader sr)
        {
            base.ReadItemContentForIB(obj, sr);
            obj.IsCurrentVersion = (bool)sr["IsCurrentVersion"];
        }
        
        public override string GetAllItemsInAllDocQueryString()
        {
            return AveDiscoverQueryString.AllItemsInDocsForContentManager;
        }
    }
}
