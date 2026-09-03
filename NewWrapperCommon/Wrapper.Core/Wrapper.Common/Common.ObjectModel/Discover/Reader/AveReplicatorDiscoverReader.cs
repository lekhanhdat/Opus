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
    public class AveReplicatorDiscoverReader : AveDiscoverReader
    {
        private static AveReplicatorDiscoverReader mReader;
        private static object mLock = new object();

        protected AveReplicatorDiscoverReader()
        {

        }

        public static AveReplicatorDiscoverReader GetInstance()
        {
            if (mReader == null)
            {
                lock (mLock)
                {
                    if (mReader == null)
                    {
                        mReader = new AveReplicatorDiscoverReader();
                    }
                }
            }
            return mReader;
        }
        
        public override string GetAllItemsInAllDocQueryString()
        {
            return AveDiscoverQueryString.AllItemsInDocsForReplicator;
        }

        public override string GetItemColumns()
        {
            return @" doc.Id,doc.LeafName,doc.DoclibRowId,doc.Type,doc.UIVersion,doc.Level,doc.DocFlags,
            doc.Size,doc.IsCurrentVersion,doc.TimeLastModified,NULL AS tp_Guid,doc.CheckoutUserId ";//NULL is tp_Guid
        }

        public override void ReadItemContent(AveItemObject obj, IAveQueryDataReader sr)
        {
            obj.DocID = (Guid)sr["Id"];
            obj.SourceName = obj.LeafName = obj.ItemName = (string)sr["LeafName"];
            if (!(sr["DoclibRowId"] is DBNull))
            {
                obj.ID = (int?)sr["DoclibRowId"];
            }
            obj.Type = (byte)sr["Type"];
            obj.Uiversion = (int)sr["UIVersion"];
            obj.Level = (byte)sr["Level"];
            if (!(sr["DocFlags"] is DBNull))
            {
                obj.DocFlags = (int?)sr["DocFlags"];
            }
            if (!(sr["Size"] is DBNull))
            {
                obj.Size = long.Parse(sr["Size"].ToString());
            }
            obj.IsCurrentVersion = (bool)sr["IsCurrentVersion"];
            obj.TimeLastModified = (DateTime)sr["TimeLastModified"];
            if (!(sr["tp_Guid"] is DBNull))
            {
                obj.tp_GUID = (Guid)sr["tp_Guid"];
            }
        }

        public override void ReadItemContent(AveItemObject obj, SqlDataReader sr)
        {
            obj.DocID = (Guid)sr["Id"];
            obj.SourceName = obj.LeafName = obj.ItemName = (string)sr["LeafName"];
            if (!(sr["DoclibRowId"] is DBNull))
            {
                obj.ID = (int?)sr["DoclibRowId"];
            }
            obj.Type = (byte)sr["Type"];
            obj.Uiversion = (int)sr["UIVersion"];
            obj.Level = (byte)sr["Level"];
            if (!(sr["DocFlags"] is DBNull))
            {
                obj.DocFlags = (int?)sr["DocFlags"];
            }
            if (!(sr["Size"] is DBNull))
            {
                obj.Size = long.Parse(sr["Size"].ToString());
            }
            obj.IsCurrentVersion = (bool)sr["IsCurrentVersion"];
            obj.TimeLastModified = (DateTime)sr["TimeLastModified"];
            if (!(sr["tp_Guid"] is DBNull))
            {
                obj.tp_GUID = (Guid)sr["tp_Guid"];
            }
            obj.CheckoutUserId = sr["CheckoutUserId"] is DBNull ? null : (int?)sr["CheckoutUserId"];
        }

        public override void ReadItemContentForIB(AveItemObject obj, SqlDataReader sr)
        {
            base.ReadItemContentForIB(obj, sr);
            obj.IsCurrentVersion = (bool)sr["IsCurrentVersion"];
        }

        public override void OverriteProperties(SqlDataReader sr, AveItemObject item)
        {
            base.OverriteProperties(sr, item);
            if (!(sr["tp_Guid"] is DBNull))
            {
                item.tp_GUID = (Guid)sr["tp_Guid"];
            }
        }

        public override void ReadVersionContent(AveVersionObject obj, SqlDataReader sr)
        {
            base.ReadVersionContent(obj, sr);
            if (!(sr["Size"] is DBNull))
            {
                obj.Size = long.Parse(sr["Size"].ToString());
            }
        }

        public override string GetAllVersionsQueryString(bool includeRecyclebin = false)
        {
            return AveDiscoverQueryString.AllVersionsForReplicator;
        }

        public virtual bool IsUnusedFolder(string url, bool noList)
        {
            return DiscoverUtility.IsUnusedFolder(url, noList);
        }
    }
}
