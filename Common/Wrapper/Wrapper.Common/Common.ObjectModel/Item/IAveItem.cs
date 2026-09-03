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
using System.Text;

namespace AvePoint.Wrapper.Common
{
    public interface IAveItem
    {
        Dictionary<string, object> GetDocInfo(AveBaseItemInfo baseItemInfo, Dictionary<string, object> currentVersionDocData);
        Dictionary<string, object> GetAttachmentInfo(AveBaseItemInfo baseItemInfo);
        Dictionary<string, object> GetUserData(AveBaseItemInfo baseItemInfo);
        List<Dictionary<string, object>> GetUserDataJunction(AveBaseItemInfo baseItemInfo);
        //int GetParnetIdByThreadIndex(Guid listId, byte[] threadIndex);
        int? GetInternalVersion(AveBaseItemInfo info);
        int GetDocFlag(AveBaseItemInfo info);
        byte[] GetRbsIdByNative(AveBaseItemInfo info);
        IAveFile File{get;set;}
        IAveListItem ListItem { get;set;}
        IAveView View { get; set; }
        //AveStorageInfo GetStorageInfo(AveBaseItemInfo itemInfo, byte[] rbsId, bool IsBackupLinkForArchivedData, string activeProviderName);
        //AveStubDataType GetEBSDataType(AveBaseItemInfo info);
        //AveStubDataType GetRBSDataType(AveBaseItemInfo itemInfo, byte[] rbsId);
        //AveStubDataType GetRBSDataType(byte[] RBSBlobId, byte[] rbsId);
        string GetStubInfoByNative(AveBaseItemInfo info);
        int GetCheckOutUserId(AveBaseItemInfo info);
        List<int> GetDocVersions(AveBaseItemInfo info);
        int SetAttachmentSize(AveBaseItemInfo info);
        Dictionary<string, string> GetItemViewFields(AveBaseItemInfo info, Dictionary<string, object> tempUserData, IAveListItem listItem);
        Dictionary<string, object> GetItemCurrentVersionDocData(AveBaseItemInfo baseItemInfo);
        //AveStubDataType GetStubDataType(AveBaseItemInfo info, byte[] rbsId);


        int ChangeItemId(Guid siteId, Guid id, Guid rootFolderId, int itemType, int fromId, int toId);

        void UpdateAllDocsPropertyByNative(AveBaseItemInfo mBaseItemInfo, DateTime timeCreated, DateTime timeLastModified, int version);

        bool CreateVersionByNative(AveBaseItemInfo mBaseItemInfo, int version, RestoringDto restoringDto);

        void InitBySPListItem(IAveListItem listItem);
        void UpdateFields(Dictionary<string, object> fieldData, AveBaseItemInfo info);
        void UpdateFields(Dictionary<string, object> fieldMap, AveBaseItemInfo info, bool ThrowWhenUpdateFailed);
        IAveFile LoadCheckOutFile(IAveWeb web, Guid fileId, IAveUser user);

        IAveFile GetFile(string name);
        IAveFile GetFile();
        IAveFile GetVirtualFile();

        IAveFolder Folder { get; set; }

        void ReloadFile();
        void ReloadFile(bool fakeDeletedUser);
        void AddFields(IAveListItem spListItem, Dictionary<string, object> fieldMap, AveBaseItemInfo info);

        IAveWeb Web { get; set; }
        string OwnerLoginName { get; }
        int GetCurrentUIVersion(Guid siteId, IAveListItem item);
        void InsertIntoAllUserDatajunction(IAveListItem item, Guid fieldId, Guid sourceListId, int id, int ordinal, int version);
        void UpdateColumnByNative(Guid siteId, IAveListItem item, int version, int rowOrdinal, string colName, object colValue);
        List<AveRoleAssignmentInfo> GetItemRoleAssignments(Guid siteId, Guid scopeId);

        int GetLookupIdByGUID(Guid lookupListId,Guid tempGuid);

        bool IsCheckOutFile(Guid siteId, Guid listId, int fileId, out int checkId, out Guid id);

        void ChangeCheckoutUserID(Guid siteId, Guid uniqueID, int newUserID);

        bool MoveToConflictFolder(IAveList parentList, IAveFolder parentFolder, IAveListItem listItem, bool isSourceWin);

        void SkipRestoreSpecialListColumnValues(AveBaseItemInfo info, List<string> fieldInternalNames);

        List<AveTermStoreInfo> GetRelatedMetadataInfo(List<AveTaxFieldInfo> infos, AveBackupOption backupOption);
    }
}
