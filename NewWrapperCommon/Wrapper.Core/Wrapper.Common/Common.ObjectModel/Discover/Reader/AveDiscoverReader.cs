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
    using GCommon;
    using System;
    using System.Collections.Generic;
    using System.Data.SqlClient;
    using System.Diagnostics.CodeAnalysis;
    using System.Reflection;
    public class AveDiscoverReader : IAveDiscoverReader
    {

        protected static AveLogger log = AveLogger.GetInstance(MethodBase.GetCurrentMethod().DeclaringType);

        protected AveDiscoverReader()
        {

        }
        
        /// <summary>
        /// 由于历史原因，该方法只能07和10调用，13在server层有自己的factory
        /// </summary>
        /// <param name="module"></param>
        /// <returns></returns>
        public static AveDiscoverReader GetAveDiscoverReader(DiscoverModule module)
        {
            switch (module)
            {
                case DiscoverModule.Item:
                    return AveItemDiscoverReader.GetInstance();
                case DiscoverModule.Replicator:
                    return AveReplicatorDiscoverReader.GetInstance();
                case DiscoverModule.Extender:
                    return AveExtenderDiscoverReader.GetInstance();
                case DiscoverModule.PlatformRecovery:
                    return AvePlatformRecoveryDiscoverReader.GetInstance();
                case DiscoverModule.Archive:
                    return AveArchiveDiscoverReader.GetInstance();
                case DiscoverModule.ContentManager:
                    return AveContentManagerDiscoverReader.GetInstance();
                default:
                    return GetInstance();
            }
        }

        private static AveDiscoverReader mReader;
        private static object mLock = new object();

        public static AveDiscoverReader GetInstance()
        {
            if (mReader == null)
            {
                lock (mLock)
                {
                    if (mReader == null)
                    {
                        mReader = new AveDiscoverReader();
                    }
                }
            }
            return mReader;
        }

        public virtual string GetSingleItemAttachmentsQueryString()
        {
            return AveDiscoverQueryString.SingleAttachmentsForCommon; ;
        }

        public virtual string GetAttachmentsQueryString()
        {
            return AveDiscoverQueryString.AllAttachmentsForCommon;
        }

        public virtual string GetAttachmentsWithRecycleBinQueryString()
        {//Only for Extender
            return GetAttachmentsQueryString();
        }

        public virtual string GetAllItemsInAllDocQueryString()
        {
            return AveDiscoverQueryString.AllItemsInDocs;
        }
        
        public virtual string GetAllVersionsQueryString(bool includeRecyclebin = false)
        {
            return AveDiscoverQueryString.AllVersionsForCommon;
        }
        
        
        public virtual string GetAttachmentStubContentForIB()
        {
            return string.Empty;
        }
        
        public string GetAllVersionsQueryStringFor07()
        {
            return AveDiscoverQueryString.AllVersionsFor07;
        }

        public virtual string GetAllItemAndVersionsStubInfoQueryString()
        {
            return string.Empty;
        }
        
        public virtual string GetAllAttachmentsStubInfoQueryString()
        {
            return string.Empty;
        }

        public virtual string GetAllItemsInUserDataQueryString()
        {
            return AveDiscoverQueryString.AllItemsInUserData;
        }

        public virtual string GetDocInfoForIBQueryString()
        {
            return AveDiscoverQueryString.AllDocValueForEventCache;
        }

        public virtual string GetItemColumns()
        {
            return " doc.Id,doc.LeafName,doc.DoclibRowId,doc.Type,doc.TimeLastModified,doc.UIVersion,doc.CheckoutUserId ";
        }

        public virtual void ReadAttachmentContent(AveItemObject obj, SqlDataReader sr)
        {
            obj.DocID = (Guid)sr["Id"];
            obj.DirName = (string)sr["DirName"];
            obj.SourceName = obj.LeafName = obj.ItemName = (string)sr["LeafName"];
            obj.FullUrl = (obj.DirName + "/" + obj.LeafName).Trim('/');
            obj.TimeLastModified = (DateTime)sr["TimeLastModified"];
            obj.Uiversion = (int)sr["UIVersion"];
            if (!(sr["Size"] is DBNull))
            {
                obj.Size = long.Parse(sr["Size"].ToString());
            }
        }

        public virtual void ReadAttachmentContent(AveItemObject obj, DocObject tempDoc)
        {
            obj.DocID = tempDoc.Id;
            obj.DirName = tempDoc.DirName;
            obj.SourceName = obj.LeafName = obj.ItemName = tempDoc.LeafName;
            obj.FullUrl = (obj.DirName + "/" + obj.LeafName).Trim('/');
            obj.TimeLastModified = tempDoc.TimeLastModified;
            obj.Uiversion = tempDoc.UIVersion;
            obj.Size = tempDoc.Size;
            obj.DocFlags = tempDoc.DocFlags;
        }

        public virtual void OverriteProperties(SqlDataReader sr, AveItemObject item)
        {
            item.TimeLastModified = (DateTime)sr["TimeLastModified"];
            item.Level = (byte)sr["Level"];
        }

        public virtual void ReadStubItemContent(AveItemObject obj, SqlDataReader sr)
        {
            ReadItemContent(obj, sr);
        }

        public virtual void ReadItemContent(AveItemObject obj, IAveQueryDataReader sr)
        {
            obj.DocID = (Guid)sr["Id"];
            obj.SourceName = obj.LeafName = obj.ItemName = (string)sr["LeafName"];
            obj.TimeLastModified = (DateTime)sr["TimeLastModified"];
            obj.Uiversion = (int)sr["UIVersion"];
            if (!(sr["DoclibRowId"] is DBNull))
            {
                obj.ID = (int?)sr["DoclibRowId"];
            }
            obj.Type = (byte)sr["Type"];
            obj.Level = (byte)sr["Level"];
        }

        public virtual string GetItemVersionsWithDocIdsCondition()
        {
            return DiscoverConditionString.ListItemUserdataWithDocIds;
        }

        public virtual void ReadItemContent(AveItemObject obj, SqlDataReader sr)
        {
            obj.DocID = (Guid)sr["Id"];
            obj.SourceName = obj.LeafName = obj.ItemName = (string)sr["LeafName"];
            obj.TimeLastModified = (DateTime)sr["TimeLastModified"];
            obj.Uiversion = (int)sr["UIVersion"];
            if (!(sr["DoclibRowId"] is DBNull))
            {
                obj.ID = (int?)sr["DoclibRowId"];
            }
            if (!(sr["CheckoutUserId"] is DBNull))
            {
                obj.CheckoutUserId = (int?)sr["CheckoutUserId"];
            }
            obj.Type = (byte)sr["Type"];
            obj.Level = (byte)sr["Level"];
        }

        public virtual void ReadItemContentForIB(AveItemObject obj, SqlDataReader sr)
        {
            obj.DocID = (Guid)sr["Id"];
            obj.DirName = (string)sr["DirName"];
            obj.SourceName = obj.LeafName = obj.ItemName = (string)sr["LeafName"];
            obj.TimeLastModified = (DateTime)sr["TimeLastModified"];
            obj.Uiversion = (int)sr["UIVersion"];
            if (!(sr["DoclibRowId"] is DBNull))
            {
                obj.ID = (int?)sr["DoclibRowId"];
            }
            obj.Type = (byte)sr["Type"];
            obj.Level = (byte)sr["Level"];
            obj.CheckoutUserId = sr["CheckoutUserId"] is DBNull ? null : (int?)sr["CheckoutUserId"];
        }

        public virtual void ReadItemContentForIB(AveItemObject obj, DocObject doc)
        {
            obj.DocID = doc.Id;
            obj.DirName = doc.DirName;
            obj.SourceName = obj.LeafName = obj.ItemName = doc.LeafName;
            obj.TimeLastModified = doc.TimeLastModified;
            obj.Uiversion = doc.UIVersion;
            obj.ID = doc.DoclibRowId;
            obj.Type = doc.Type;
            obj.Level = doc.Level;
            obj.CheckoutUserId = doc.CheckoutUserId;
            obj.IsCurrentVersion = doc.IsCurrentVersion;
            obj.Size = doc.Size;
            obj.DocFlags = doc.DocFlags;
        }

        public virtual void ReadVersionContent(AveVersionObject obj, SqlDataReader sr)
        {
            obj.Uiversion = (int)sr["UIVersion"];
            if (!(sr["TimeLastModified"] is DBNull))
            {
                obj.TimeLastModified = (DateTime)sr["TimeLastModified"];
            }
            obj.Level = (byte)sr["Level"];
            obj.IsCurrentVersion = (bool)sr["IsCurrentVersion"];
        }

        public virtual void ReadVersionStubInfo(SqlDataReader sr, AveVersionObject obj)
        {
        }

        public virtual void ReadAttachmentStubInfo(SqlDataReader sr, AveItemObject obj)
        {
        }

        public virtual void ReadVersionContentWithDeleteState(AveVersionObject obj, SqlDataReader sr)
        {
            ReadVersionContent(obj, sr);
            obj.DeleteTransactionId = sr.GetVaule<byte[]>("DeleteTransactionId");
        }

        public virtual void ReadStubVersionContent(AveVersionObject obj, SqlDataReader sr)
        {
            ReadVersionContent(obj, sr);
        }

        [SuppressMessage("FxCopCustomRules", "C100007:SpellCheckStringValues", Justification = "Uiversion used as a key, modifiy in DA6.2.")]
        public virtual void GenerateVersionObject(Dictionary<string, object> container, AveVersionObject obj)
        {
            obj.Uiversion = (int)container["Uiversion"];
            obj.TimeLastModified = (DateTime)container["TimeLastModified"];
        }

        public virtual string GetVersionConditionWithDocIds()
        {
            return DiscoverConditionString.VersionConditionWithDocIds;
        }

        public virtual bool IsUnusedFolder(string url, bool noList)
        {
            return false;
        }

        /// <summary>
        /// Only For extender
        /// </summary>
        /// <param name="item"></param>
        /// <param name="attachment"></param>
        /// <param name="sr"></param>
        public virtual void AddExtentionAttachment(AveItemObject item, AveItemObject attachment, SqlDataReader sr)
        {
        }

        public virtual string GetWebItemVersionCondition(bool includeRecycleBin)
        {
            return includeRecycleBin ? DiscoverConditionString.ListItemUserdataWithRecycleBin : DiscoverConditionString.ListItemUserdata;
        }

        public virtual string GetListItemVersionCondition(bool includeRecycleBin)
        {
            return includeRecycleBin ? DiscoverConditionString.ListItemUserdataWithRecycleBin : DiscoverConditionString.ListItemUserdata;
        }

        public virtual string GetItemVersionsWithDocIdCondition()
        {
            return DiscoverConditionString.ListItemUserdataWithDocId;
        }

        public virtual bool NeedGetItemStubInfo()
        {
            return false;
        }
    }
}
