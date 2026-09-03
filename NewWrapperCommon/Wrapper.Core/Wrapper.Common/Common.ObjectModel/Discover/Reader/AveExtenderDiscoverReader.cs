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
    public class AveExtenderDiscoverReader : AveDiscoverReader
    {
        private static AveExtenderDiscoverReader mReader;
        private static object mLock = new object();

        protected AveExtenderDiscoverReader()
        {

        }

        public static AveExtenderDiscoverReader GetInstance()
        {
            if (mReader == null)
            {
                lock (mLock)
                {
                    if (mReader == null)
                    {
                        mReader = new AveExtenderDiscoverReader();
                    }
                }
            }
            return mReader;
        }

        public override void ReadStubVersionContent(AveVersionObject obj, SqlDataReader sr)
        {
            ReadVersionContent(obj, sr);
            if (!(sr["RbsId"] is DBNull))
            {
                obj.RbsId = (byte[])sr["RbsId"];
            }
        }

        public override string GetAttachmentsQueryString()
        {
            return AveDiscoverQueryString.AllAttachmentsForExternder;
        }

        public override string GetAttachmentsWithRecycleBinQueryString()
        {
            return AveDiscoverQueryString.AllAttachmentsForExternderWithRecycleBin;
        }

        public override string GetAllItemsInAllDocQueryString()
        {
            return AveDiscoverQueryString.AllItemsInDocsForExtender;
        }

        public override string GetItemVersionsWithDocIdsCondition()
        {
            return DiscoverConditionString.ListItemVersionsWithDocIdsForExtender;
        }

        public override string GetItemVersionsWithDocIdCondition()
        {
            return DiscoverConditionString.ListItemVersionsWithDocIdForExtender;
        }

        public override string GetAttachmentStubContentForIB()
        {
            return AveDiscoverQueryString.AttachmentStubExtenderIB;
        }

        public override string GetAllVersionsQueryString(bool includeRecyclebin = false)
        {
            return includeRecyclebin ? AveDiscoverQueryString.AllItemsAndVersionsForExtenderWithRecycleBin : AveDiscoverQueryString.AllItemsAndVersionsForExtender;
        }

        public override string GetWebItemVersionCondition(bool includeRecycleBin)
        {
            return includeRecycleBin ? DiscoverConditionString.WebItemsWithRecycleBin : DiscoverConditionString.WebItems;
        }

        public override string GetListItemVersionCondition(bool includeRecycleBin)
        {
            return includeRecycleBin ? DiscoverConditionString.ListItemsWithRecycleBin : DiscoverConditionString.ListItems;
        }

        public override string GetVersionConditionWithDocIds()
        {
            return DiscoverConditionString.ListItemVersionsWithDocIdsForExtender;
        }

        public override string GetAllItemAndVersionsStubInfoQueryString()
        {
            return AveDiscoverQueryString.AllItemAndVersionsStubInfo;
        }

        public override string GetAllAttachmentsStubInfoQueryString()
        {
            return AveDiscoverQueryString.AllAttachmentsStubInfoForExtender;
        }

        public override string GetDocInfoForIBQueryString()
        {
            return AveDiscoverQueryString.AllDocValueForEventCache_Extender;
        }

        public override string GetItemColumns()
        {
            return " doc.Id,doc.LeafName,doc.DoclibRowId,doc.Type,doc.UIVersion as UIVersion,doc.Type,doc.TimeLastModified,doc.DocFlags,doc.HasStream,doc.Level, 2 AS QueryType";//, NULL AS Content,doc.Size,stream.RbsId ";
        }

        public override void ReadAttachmentContent(AveItemObject obj, SqlDataReader sr)
        {
            obj.DocID = (Guid)sr["Id"];
            obj.DirName = (string)sr["DirName"];
            obj.SourceName = obj.LeafName = obj.ItemName = (string)sr["LeafName"];
            try
            {
                if (!(sr["Size"] is DBNull))
                {
                    obj.Size = long.Parse(sr["Size"].ToString());
                }
                obj.ParentID = (Guid)sr["ParentId"];
                obj.DeleteTransactionId = sr.GetVaule<byte[]>("DeleteTransactionId");
                if (!(sr["DocFlags"] is DBNull))
                {
                    obj.DocFlags = (int?)sr["DocFlags"];
                }
                if (!(sr["Content"] is DBNull))
                {
                    obj.Content = (byte[])sr["Content"];//Stub
                }
                if (!(sr["RbsId"] is DBNull))
                {
                    obj.RbsId = (byte[])sr["RbsId"];
                }
            }
            catch (IndexOutOfRangeException)
            {
                //log.Warn("Read attachment content failed in Discover. Error:{0}", e.ToString());
            }
            catch (AveQueryException)
            {
                //log.Warn("Read attachment content failed in Discover. Error:{0}", e.ToString());
            }
        }

        public override void ReadStubItemContent(AveItemObject obj, SqlDataReader sr)
        {
            base.ReadStubItemContent(obj, sr);
            if (!(sr["RbsId"] is DBNull))
            {
                obj.RbsId = (byte[])sr["RbsId"];
            }
        }

        public override void ReadItemContentForIB(AveItemObject obj, SqlDataReader sr)
        {
            base.ReadItemContentForIB(obj, sr);
            try
            {
                if (!(sr["Size"] is DBNull))
                {
                    obj.Size = long.Parse(sr["Size"].ToString());
                }
                if (!(sr["DocFlags"] is DBNull))
                {
                    obj.DocFlags = (int?)sr["DocFlags"];
                }
                if (!(sr["Content"] is DBNull))
                {
                    obj.Content = (byte[])sr["Content"];//Stub
                }
                if (!(sr["RbsId"] is DBNull))
                {
                    obj.RbsId = (byte[])sr["RbsId"];
                }
            }
            catch (IndexOutOfRangeException)
            {
                //log.Warn("Read item content for ib failed in Discover. Error:{0}", e.ToString());
            }
            catch (AveQueryException)
            {
                //log.Warn("Read item content for ib failed in Discover. Error:{0}", e.ToString());
            }
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
            obj.HasStream = (int)sr["HasStream"] == 1 ? true : false;
            obj.QueryType = (int)sr["QueryType"];
            try
            {
                if (!(sr["Size"] is DBNull))
                {
                    obj.Size = long.Parse(sr["Size"].ToString());
                }
                if (!(sr["DocFlags"] is DBNull))
                {
                    obj.DocFlags = (int?)sr["DocFlags"];
                }
                if (!(sr["Content"] is DBNull))
                {
                    obj.Content = (byte[])sr["Content"];//Stub
                }
                if (!(sr["RbsId"] is DBNull))
                {
                    obj.RbsId = (byte[])sr["RbsId"];
                }
            }
            catch (IndexOutOfRangeException)
            {
                //log.Warn("Read item content failed in Discover. Error:{0}", e.ToString());
            }
            catch (AveQueryException)
            {
                //log.Warn("Read item content failed in Discover. Error:{0}", e.ToString());
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
            obj.HasStream = (int)sr["HasStream"] == 1 ? true : false;
            obj.QueryType = (int)sr["QueryType"];
            try
            {
                if (!(sr["Size"] is DBNull))
                {
                    obj.Size = long.Parse(sr["Size"].ToString());
                }
                if (!(sr["DocFlags"] is DBNull))
                {
                    obj.DocFlags = (int?)sr["DocFlags"];
                }
                if (!(sr["RbsId"] is DBNull))
                {
                    obj.RbsId = (byte[])sr["RbsId"];
                }
                if (!(sr["Content"] is DBNull))
                {
                    obj.Content = (byte[])sr["Content"];//Stub
                }
                if (!(sr["InternalVersion"] is DBNull))
                {
                    obj.InternalVersion = (int)sr["InternalVersion"];
                }
            }
            catch (IndexOutOfRangeException)
            {
                //log.Warn("Read item content failed in Discover. Error:{0}", e.ToString());
            }
            catch (AveQueryException)
            {
                //log.Warn("Read item content failed in Discover. Error:{0}", e.ToString());
            }
            obj.DeleteTransactionId = sr.GetVaule<byte[]>("DeleteTransactionId");
        }

        public override void ReadItemContentForIB(AveItemObject obj, DocObject doc)
        {
            base.ReadItemContentForIB(obj, doc);
            obj.HasStream = doc.HasStream == 1 ? true : false;
            obj.QueryType = doc.QueryType;
            obj.RbsId = doc.RbsId;
            obj.Content = doc.Content;
        }

        public override void ReadVersionContent(AveVersionObject obj, SqlDataReader sr)
        {
            obj.QueryType = (int)sr["QueryType"];
            obj.Level = (byte)sr["Level"];
            obj.Uiversion = (int)sr["UIVersion"];
            obj.HasStream = (int)sr["HasStream"] == 1 ? true : false;
            try
            {
                if (!(sr["Size"] is DBNull))
                {
                    obj.Size = long.Parse(sr["Size"].ToString());
                }
                if (!(sr["DocFlags"] is DBNull))
                {
                    obj.DocFlags = (int?)sr["DocFlags"];
                }
                if (!(sr["Content"] is DBNull))
                {
                    obj.Content = (byte[])sr["Content"];//Stub
                }
                if (!(sr["RbsId"] is DBNull))
                {
                    obj.RbsId = (byte[])sr["RbsId"];
                }
                if (!(sr["InternalVersion"] is DBNull))
                {
                    obj.InternalVersion = (int)sr["InternalVersion"];
                }
            }
            catch (IndexOutOfRangeException)
            {
                // log.Warn("Read version content failed in Discover. Error:{0}", e.ToString());
            }
            catch (AveQueryException)
            {
                //log.Warn("Read version content failed in Discover. Error:{0}", e.ToString());
            }
            obj.DeleteTransactionId = sr.GetVaule<byte[]>("DeleteTransactionId");
        }

        public override void ReadVersionStubInfo(SqlDataReader sr, AveVersionObject obj)
        {
            if (!(sr["DocFlags"] is DBNull))
            {
                obj.DocFlags = (int)sr["DocFlags"];
            }
            if (!(sr["RbsId"] is DBNull))
            {
                obj.RbsId = (byte[])sr["RbsId"];
            }
            if (!(sr["Content"] is DBNull))
            {
                obj.Content = (byte[])sr["Content"];
            }
        }

        public override void ReadAttachmentStubInfo(SqlDataReader sr, AveItemObject obj)
        {
            if (!(sr["DocFlags"] is DBNull))
            {
                obj.DocFlags = (int)sr["DocFlags"];
            }
            if (!(sr["RbsId"] is DBNull))
            {
                obj.RbsId = (byte[])sr["RbsId"];
            }
            if (!(sr["Content"] is DBNull))
            {
                obj.Content = (byte[])sr["Content"];
            }
        }

        public override void OverriteProperties(SqlDataReader sr, AveItemObject item)
        {
            try
            {
                if ((sr["RbsId"] is DBNull))
                {
                    item.RbsId = null;
                }
            }
            catch (IndexOutOfRangeException)
            {
            }
            catch (AveQueryException)
            {
            }
        }

        public override void AddExtentionAttachment(AveItemObject item, AveItemObject attachment, SqlDataReader sr)
        {
            if (!(sr["DocFlags"] is DBNull) && ((int)sr["DocFlags"] & 65536) != 0)
            {
                item.StubAttachmentObjs.Add(attachment);
            }
            else if (sr["ContentLength"] is DBNull || (long)sr["ContentLength"] == 0)
            {
                item.StubAttachmentObjs.Add(attachment);
            }
        }

        public override string GetSingleItemAttachmentsQueryString()
        {
            return AveDiscoverQueryString.SingleAttachmentsForExternder;
        }

        public override bool NeedGetItemStubInfo()
        {
            return true;
        }
    }
}
