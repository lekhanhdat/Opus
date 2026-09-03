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
namespace AvePoint.Wrapper.Restore
{
    public interface IAveSPItem : IRestoreableObject
    {
        void AddFields(AvePoint.Wrapper.Common.IAveListItem spListItem, System.Collections.Generic.Dictionary<string, object> fieldMap, AvePoint.Wrapper.Common.AveBaseItemInfo info);
        void AddFields(System.Collections.Generic.Dictionary<string, object> fieldMap);
        void AddItemMapping(int rowId);
        AvePoint.Wrapper.Common.AveStorage AveStorage { get; }
        void CreateDatajunctionByNative(AvePoint.Wrapper.Common.IAveListItem item, Guid fieldId, Guid sourceListId, int version, System.Collections.Generic.List<int> values);
        //bool CreateVersionByNative(int version, AvePoint.Wrapper.Common.RestoringDto restoringDto);
        bool EnsureItemSchemaDependency(System.Collections.Generic.Dictionary<string, object> userData, bool skipItemWhenNotFound, bool skipItemWhenConflict, AveContentTypeRestoreOption ctRestoreOption, AveFieldRestoreOption fieldRestoreOption);
        bool EnsureItemSchemaDependency(System.Collections.Generic.Dictionary<string, object> userData, bool skipItemWhenNotFound, bool skipItemWhenConflict, AveContentTypeRestoreOption ctRestoreOption, AveFieldRestoreOption fieldRestoreOption, bool throwException);
        bool EnsureItemSchemaDependency(System.Collections.Generic.Dictionary<string, object> userData, System.Collections.Generic.List<System.Collections.Generic.Dictionary<string, object>> junctionData, bool skipItemWhenNotFound, bool skipItemWhenConflict, AveContentTypeRestoreOption ctRestoreOption, AveFieldRestoreOption fieldRestoreOption, bool throwException);
        int GetCurrentUIVersion(Guid siteId, AvePoint.Wrapper.Common.IAveListItem item);
        AveItemHoldRecord GetHoldRecord(System.Collections.Hashtable metaInfos, byte[] dataMetaInfo, System.Collections.Generic.Dictionary<string, object> userData);
        int GetLookupIdByGUID(Guid lookupListId, Guid GUID);
        int GetLookupIdByGUID(Guid lookupWebId, Guid lookupListId, Guid tpGuid);
        AvePoint.Wrapper.Common.IReport GetReport();
        /// <summary>
        /// 使用column的column internal name和value来获取到被lookup的Item的Row id。
        /// </summary>
        /// <param name="lookupWebId"></param>
        /// <param name="lookupListId"></param>
        /// <param name="lookupColumnDisplayName">Column的Internal Name</param>
        /// <param name="itemLookupColumnDisplayValue"></param>
        /// <returns></returns>
        int GetLookupIdByFieldDisplayNameAndFieldValue(Guid lookupWebId, Guid lookupListId, String lookupColumnDisplayName, String itemLookupColumnDisplayValue);
        bool HasUniqueRoleAssignments { get; }
        Guid Id { get; set; }
        void InitBySPListItem(AvePoint.Wrapper.Common.IAveListItem listItem);
        void InitFieldsInMetaInfo(System.Collections.Generic.Dictionary<string, string> metaInfoDic);
        void InsertIntoAllUserDatajunction(AvePoint.Wrapper.Common.IAveListItem item, Guid fieldId, Guid sourceListId, int id, int ordinal, int version);
        int? InternalVersion { get; set; }
        bool IsCheckOut { get; set; }
        //int IsConflict(AvePoint.Common.AveSqlConnection sqlConn, Guid siteId, Guid parentId, string name);
        bool IsNewCreated { get; set; }
        bool IsNewCreatedDoc { get; set; }
        bool IsStubData { get; }
        bool IsVersion { get; set; }
        int Level { get; set; }
        AvePoint.Wrapper.Common.IAveFile LoadCheckOutFile(AvePoint.Wrapper.Common.IAveWeb mSPWeb, Guid fileId, AvePoint.Wrapper.Common.IAveUser iAveUser);
        bool MoveToConflictFolder(AvePoint.Wrapper.Common.IAveList parentList, AvePoint.Wrapper.Common.IAveFolder parentFolder, AvePoint.Wrapper.Common.IAveListItem listItem, bool isSourceWin);
        string Name { get; set; }
        int OldRowId { get; }
        int OriginalModerationStatus { get; set; }
        string OwnerLoginName { get; }
        IAveSPFolder ParentFolder { get; }
        IAveSPList ParentList { get; }
        IAveSPSite ParentSite { get; }
        IAveSPWeb ParentWeb { get; }
        void PostAction();
        AvePoint.Wrapper.Common.IAveBackupRestoreQueryService QueryService { get; set; }
        void RemoveDatajunctionByNative(AvePoint.Wrapper.Common.IAveListItem item, Guid fieldId, Guid sourceListId, int version);
        void ResetName(string newName);
        void RestoreDataJunction(System.Collections.Generic.List<System.Collections.Generic.Dictionary<string, object>> junctionData);
        void RestoreItemProperty(AvePoint.Wrapper.Common.AveItemFieldCollectionInfo fieldCollection, AvePoint.Wrapper.Common.IAveList list, AvePoint.Wrapper.Common.IAveListItem item);
        void RestoreItemProperty(AvePoint.Wrapper.Common.AveItemFieldCollectionInfo fieldCollection, AvePoint.Wrapper.Common.IAveList list, AvePoint.Wrapper.Common.IAveListItem item, bool overwriteVersion);
        void RestoreLookupFieldGuidValue(System.Collections.Generic.Dictionary<string, string> lookupFieldGuidValue);
        int RestoreVersion { get; set; }
        int RowId { get; set; }
        Guid ScopeId { get; set; }
        string ScopeUrl { get; set; }
        void SetPicProperty(int width, int heigth);
        Guid SiteId { get; set; }
        AvePoint.Wrapper.Common.IAveListItem SPListItem { get; set; }
        AvePoint.Wrapper.Common.IAveWeb SPWeb { get; set; }
        void UpdateColumnByNative(Guid siteId, AvePoint.Wrapper.Common.IAveListItem item, int version, int rowOrdinal, string colName, object colValue);
        int Version { get; set; }
        bool IsMergeToFolder { get; set; }
        void SetForceAddTerm(bool isForceAdd);
    }

    public class CurrentRestoreDocStatus
    {
        public int Status;
        public string Name = null;
        public int UIVersion;
        public bool HasPreCurrentVersion = false;
    }

    [Serializable]
    public class AveItemHoldRecord
    {
        public string ItemHoldRecordStatus { get; set; }
        public string ItemLockHolders { get; set; }
        public string ItemDeleteBlockHolders { get; set; }
        public string HoldsProperty { get; set; }
        public string IconOverlay { get; set; }
        public string ItemDeclaredRecord { get; set; }
        public string RecordRestrictions { get; set; }
        public bool IsHold { get; set; }
        public bool IsRecord { get; set; }
    }
}
