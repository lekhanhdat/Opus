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




using System.Collections.Generic;
using System;
using System.Data.SqlClient;
using System.Diagnostics.CodeAnalysis;
namespace AvePoint.Wrapper.Common
{
    public class DiscoverUtility
    {
        public const long EnableVersion = 0x0000000000000080;
        public const long DisableAttachment = 0x0000000000000008;
        private static List<string> SYSTEM_LIST_EXCLUDE_NAMES = new List<string>();

        static DiscoverUtility()
        {
            SYSTEM_LIST_EXCLUDE_NAMES.Add("_catalogs");
            SYSTEM_LIST_EXCLUDE_NAMES.Add("_vti_pvt");
            SYSTEM_LIST_EXCLUDE_NAMES.Add("_cts");
            SYSTEM_LIST_EXCLUDE_NAMES.Add("_private");
            SYSTEM_LIST_EXCLUDE_NAMES.Add("_themes");
            SYSTEM_LIST_EXCLUDE_NAMES.Add("Lists");
            SYSTEM_LIST_EXCLUDE_NAMES.Add("m");
        }

        public static bool IsEnableVersion(long flag)
        {
            return (flag & EnableVersion) != 0;
        }

        public static bool IsEnableAttachment(long flag)
        {
            return (flag & DisableAttachment) == 0;
        }

        public static SecurityType GetSecurityObjectType(NativeChangeType nativeType)
        {
            switch (nativeType)
            {
                case NativeChangeType.RoleAdd:
                case NativeChangeType.RoleDelete:
                case NativeChangeType.RoleUpdate:
                    return SecurityType.Role;
                case NativeChangeType.AssignmentAdd:
                case NativeChangeType.AssignmentDelete:
                    return SecurityType.Assignment;
                case NativeChangeType.ScopeAdd:
                case NativeChangeType.ScopeDelete:
                    return SecurityType.Scope;
                default:
                    return SecurityType.None;
            }
        }

        public static ChangeType GetSecurityChangeType(NativeChangeType nativeType)
        {
            switch (nativeType)
            {
                case NativeChangeType.RoleAdd:
                case NativeChangeType.AssignmentAdd:
                case NativeChangeType.ScopeAdd:
                    return ChangeType.Add;
                case NativeChangeType.RoleDelete:
                case NativeChangeType.ScopeDelete:
                case NativeChangeType.AssignmentDelete:
                    return ChangeType.Delete;
                case NativeChangeType.RoleUpdate:
                    return ChangeType.Edit;
                default:
                    return ChangeType.None;
            }
        }

        public static ChangeType GetChangeType(NativeChangeType nativeType)
        {
            switch (nativeType)
            {
                case NativeChangeType.ItemAdd:
                case NativeChangeType.ChangeAdd:
                case NativeChangeType.DiscAdd:
                case NativeChangeType.ItemAdd | NativeChangeType.ChangeAdd:
                    return ChangeType.Add;
                case NativeChangeType.ChangeDelete:
                case NativeChangeType.ItemDelete:
                case NativeChangeType.ChangeDelete | NativeChangeType.ItemDelete:
                    return ChangeType.Delete;
                case NativeChangeType.ItemModify:
                case NativeChangeType.ChangeModify:
                case NativeChangeType.ChangeSystemModify:
                case NativeChangeType.Navigation:
                case NativeChangeType.ChangeSystemModify | NativeChangeType.ChangeModify:
                case NativeChangeType.ItemModify | NativeChangeType.ChangeModify:
                    return ChangeType.Edit;
                case NativeChangeType.ItemRestore:
                case NativeChangeType.ChangeRestore:
                case NativeChangeType.ItemRestore | NativeChangeType.ChangeRestore:
                    return ChangeType.Restore;
                default:
                    return ChangeType.None;
            }
        }

        public static void FillWebPartDicFromAllWebParts(AveViewObject viewObj, SqlDataReader sr)
        {
            viewObj.ViewID = (Guid)sr.GetValue(ViewColumn.Id);
            viewObj.ViewType = (int)sr.GetValue(ViewColumn.Flags);
            viewObj.IsPersonalView = (sr.GetInt32(ViewColumn.Flags) & 262144) == 262144 ? true : false;
            if (!sr.IsDBNull(ViewColumn.BaseViewID))
            {
                viewObj.BaseViewId = (byte)sr.GetValue(ViewColumn.BaseViewID);
            }
            if (!sr.IsDBNull(ViewColumn.DisplayName))
            {
                viewObj.ViewTitle = (string)sr.GetValue(ViewColumn.DisplayName);
            }
            viewObj.PageUrlID = (Guid)sr.GetValue(ViewColumn.PageUrlID);
            if (!sr.IsDBNull(ViewColumn.UserID))
            {
                viewObj.ViewUserID = (int?)sr.GetValue(ViewColumn.UserID);
            }
        }

        internal static bool IsUnusedFolder(string leafName, bool noList)
        {
            if (!noList)
            {
                return false;
            }
            return SYSTEM_LIST_EXCLUDE_NAMES.Contains(leafName);
        }
    }

    public class AveDiscoverReader
    {        
        protected AveDiscoverReader()
        {

        }

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
                case DiscoverModule.None:
                default:
                    return AveDiscoverReader.GetInstance();
            }
        }

        private static AveDiscoverReader mReader;
        private readonly static object mLock = new object();

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

        public virtual string GetAllItemsAndVersionsQueryString()
        {
            
            return AveDiscoverQueryString.AllItemAndVersionForCommon;
        }

        public virtual string GetAllItemsAndVersionsQueryString07()
        {
            
           
            return AveDiscoverQueryString.AllItemAndVersionForCommon07;
           
        }

        public virtual string GetItemColumns()
        {
            return " doc.Id,doc.LeafName,doc.DoclibRowId,doc.Type,doc.TimeLastModified,doc.UIVersion ";
        }

        public virtual void ReadAttachmentContent(AveItemObject obj, SqlDataReader sr)
        {
            obj.DocID = (Guid)sr["Id"];
            obj.DirName=(string)sr["DirName"];
            obj.SourceName = obj.LeafName = obj.ItemName = (string)sr["LeafName"];
            obj.FullUrl = (obj.DirName + "/" + obj.LeafName).Trim('/');
            obj.TimeLastModified = (DateTime)sr["TimeLastModified"];
            obj.Uiversion = (int)sr["UIVersion"];
            if (!(sr["Size"] is DBNull))
            {
                obj.Size = (int)sr["Size"];
            }
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

        public virtual void ReadVersionContent(AveVersionObject obj, SqlDataReader sr)
        {
            obj.Uiversion = (int)sr["UIVersion"];
            obj.TimeLastModified = (DateTime)sr["TimeLastModified"];
            obj.Level = (byte)sr["Level"];
        }

        [SuppressMessage("FxCopCustomRules", "C100007:SpellCheckStringValues", Justification = "Uiversion used as a key, modifiy in DA6.2.")] 
        public virtual void GenerateVersionObject(Dictionary<string, object> container, AveVersionObject obj)
        {
            obj.Uiversion = (int)container["Uiversion"];
            obj.TimeLastModified = (DateTime)container["TimeLastModified"];
        }

        public virtual bool IsUnusedFolder(string url, bool noList)
        {
            return false;
        }
    }

    public class AveReplicatorDiscoverReader : AveDiscoverReader
    {
        private static AveReplicatorDiscoverReader mReader;
        private readonly static object mLock = new object();

        private AveReplicatorDiscoverReader()
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

        public override string GetAllItemsAndVersionsQueryString()
        {
            return AveDiscoverQueryString.AllItemsAndVersionsForReplicator;
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
                obj.Size = (int)sr["Size"];
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
                obj.Size = (int)sr["Size"];
            }
            obj.IsCurrentVersion = (bool)sr["IsCurrentVersion"];
            obj.TimeLastModified = (DateTime)sr["TimeLastModified"];
            if (!(sr["tp_Guid"] is DBNull))
            {
                obj.tp_GUID = (Guid)sr["tp_Guid"];
            }
            obj.CheckoutUserId = sr["CheckoutUserId"] is DBNull ? null : (int?)sr["CheckoutUserId"];
        }

        public override void ReadVersionContent(AveVersionObject obj, SqlDataReader sr)
        {
            obj.Uiversion = (int)sr["UIVersion"];
            obj.Level = (byte)sr["Level"];
            obj.IsCurrentVersion = (bool)sr["IsCurrentVersion"];
            obj.TimeLastModified = (DateTime)sr["TimeLastModified"];
            if (!(sr["Size"] is DBNull))
            {
                obj.Size = (int)sr["Size"];
            }
        }

        public virtual bool IsUnusedFolder(string url, bool noList)
        {
            return DiscoverUtility.IsUnusedFolder(url, noList);
        }
    }

    public class AveItemDiscoverReader : AveDiscoverReader
    {
        private static AveItemDiscoverReader mReader;
        private readonly static object mLock = new object();

        public static AveItemDiscoverReader GetInstance()
        {
            if (mReader == null)
            {
                lock (mLock)
                {
                    if (mReader == null)
                    {
                        mReader = new AveItemDiscoverReader();
                    }
                }
            }
            return mReader;
        }

        private AveItemDiscoverReader()
        {

        }

        public override string GetAllItemsAndVersionsQueryString()
        { 
            return AveDiscoverQueryString.AllItemsAndVersionsForItem;
        }

        public override string GetAllItemsAndVersionsQueryString07()
        { 
            return AveDiscoverQueryString.AllItemsAndVersionsForItem07;           
        }

        public override string GetItemColumns()
        {
            return " doc.Id,doc.LeafName,doc.DoclibRowId,doc.Type,doc.UIVersion,doc.TimeLastModified ";
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
            obj.TimeLastModified = (DateTime)sr["TimeLastModified"];
            obj.Level = (byte)sr["Level"];
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
            obj.TimeLastModified = (DateTime)sr["TimeLastModified"];
            obj.Level = (byte)sr["Level"];
        }

        public override void ReadVersionContent(AveVersionObject obj, SqlDataReader sr)
        {
            obj.Uiversion = (int)sr["UIVersion"];
            obj.TimeLastModified = (DateTime)sr["TimeLastModified"];
            obj.Level = (byte)sr["Level"];
        }

        public override bool IsUnusedFolder(string url, bool noList)
        {
            return DiscoverUtility.IsUnusedFolder(url, noList);
        }
    }

    public class AveExtenderDiscoverReader : AveDiscoverReader
    {
        private static AveExtenderDiscoverReader mReader;
        private readonly static object mLock = new object();

        private AveExtenderDiscoverReader()
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

        public override string GetAttachmentsQueryString()
        {
            return AveDiscoverQueryString.AllAttachmentsForExternder;
        }

        public override string GetAttachmentsWithRecycleBinQueryString()
        {
            return AveDiscoverQueryString.AllAttachmentsForExternderWithRecycleBin;
        }

        public override string GetAllItemsAndVersionsQueryString()
        {
            return AveDiscoverQueryString.AllItemsAndVersionsForExtender;
        }

        public override string GetItemColumns()
        {
            return " doc.Id,doc.LeafName,doc.DoclibRowId,doc.Type,doc.UIVersion as UIVersion,doc.DocFlags,doc.HasStream,doc.Level, 2 AS QueryType, NULL AS Content,doc.Size "; 
        }

        public override void ReadAttachmentContent(AveItemObject obj, SqlDataReader sr)
        {
            obj.DocID = (Guid)sr["Id"];
            obj.DirName = (string)sr["DirName"];
            obj.SourceName = obj.LeafName = obj.ItemName = (string)sr["LeafName"];
            obj.ParentID = (Guid)sr["ParentId"];
            if (!(sr["Content"] is DBNull))
            {
                obj.Content = (byte[])sr["Content"];//Stub
            }
            if (!(sr["DocFlags"] is DBNull))
            {
                obj.DocFlags = (int)sr["DocFlags"];
            }
            if (!(sr["RbsId"] is DBNull))
            {
                obj.RbsId = (byte[])sr["RbsId"];
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
            obj.HasStream = (int)sr["HasStream"] == 1 ? true : false;
            obj.QueryType = (int)sr["QueryType"];
            if (!(sr["Content"] is DBNull))
            {
                obj.Content = (byte[])sr["Content"];//Stub
            }
            if (!(sr["Size"] is DBNull))
            {
                obj.Size = (int)sr["Size"];
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
            obj.HasStream = (int)sr["HasStream"] == 1 ? true : false;
            obj.QueryType = (int)sr["QueryType"];
            if (!(sr["Content"] is DBNull))
            {
                obj.Content = (byte[])sr["Content"];//Stub
            }
            if (!(sr["Size"] is DBNull))
            {
                obj.Size = (int)sr["Size"];
            }
        }

        public override void ReadVersionContent(AveVersionObject obj, SqlDataReader sr)
        {
            obj.QueryType = (int)sr["QueryType"];
            obj.Level = (byte)sr["Level"];
            obj.Uiversion = (int)sr["UIVersion"];
            if (!(sr["Content"] is DBNull))
            {
                obj.Content = (byte[])sr["Content"];//Stub
            }
            obj.HasStream = (int)sr["HasStream"] == 1 ? true : false;
            if (!(sr["DocFlags"] is DBNull))
            {
                obj.DocFlags = (int)sr["DocFlags"];
            }
            if (!(sr["Size"] is DBNull))
            {
                obj.Size = (int)sr["Size"];
            }
        }
    }

    public class AvePlatformRecoveryDiscoverReader : AveDiscoverReader
    {
        private static AvePlatformRecoveryDiscoverReader mReader;
        private readonly static object mLock = new object();

        protected AvePlatformRecoveryDiscoverReader()
        {

        }

        public static AvePlatformRecoveryDiscoverReader GetInstance()
        {
            if (mReader == null)
            {
                lock (mLock)
                {
                    if (mReader == null)
                    {
                        mReader = new AvePlatformRecoveryDiscoverReader();
                    }
                }
            }
            return mReader;
        }
    }

    public class AveArchiveDiscoverReader : AvePlatformRecoveryDiscoverReader
    {
        private static AveArchiveDiscoverReader mReader;
        private readonly static object mLock = new object();

        private AveArchiveDiscoverReader()
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

    public class AveContentManagerDiscoverReader : AveDiscoverReader
    {
        private static AveContentManagerDiscoverReader mReader;
        private readonly static object mLock = new object();

        private AveContentManagerDiscoverReader()
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
        
        public override string GetAllItemsAndVersionsQueryString()
        {
            return AveDiscoverQueryString.AllItemsAndVersionsForContentManager;
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
                obj.Size = (int)sr["Size"];
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
                obj.Size = (int)sr["Size"];
            }
        }

        public override void ReadVersionContent(AveVersionObject obj, SqlDataReader sr)
        {
            obj.Level = (byte)sr["Level"];
            obj.Uiversion = (int)sr["UIVersion"];
            if (!(sr["Size"] is DBNull))
            {
                obj.Size = (int)sr["Size"];
            }
        }
    }

    public enum DiscoverModule
    {
        None,
        Item,
        Replicator,
        Extender,
        PlatformRecovery,
        Archive,
        ContentManager
    }
}
