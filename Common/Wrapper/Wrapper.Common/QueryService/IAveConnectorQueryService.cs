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
using System.Collections;

namespace AvePoint.Wrapper.Common
{
    public interface IAveConnectorQueryService : IAveQueryService
    {
        void SaveThumbnail(Hashtable obj, bool isVideo = false);
        IAveQueryDataReader IsDirty(AveBaseItemInfo info);
        IAveQueryDataReader GetDocInfoForDirty(AveBaseItemInfo info);
        void UpdateDBContent(AveBaseItemInfo info, byte[] buffer, ref bool isFirstUpdate);
        void ClearRbsInfo(AveBaseItemInfo info);
        void ClearEBsInfo(AveBaseItemInfo info);
        void UpdateFileSize(AveBaseItemInfo info, int size, bool isSP1, int oldSize);
        void UpdateSiteUsage(long size, AveBaseItemInfo info, bool isSP1);
        int GetDocFlag(AveBaseItemInfo info, bool isEffectRecycle);
        /// <summary>
        /// Correct the DocFlags if the DocFlags is wrong.
        /// </summary>
        /// <param name="isStub">True if SPFile content is stub.</param>
        /// <returns>True if the DocFlags was wrong, and has been corrected.</returns>
        bool CorrectDocFlags(AveBaseItemInfo info, bool isStub);

        [Obsolete("Please use GetCheckOutUserID(Guid siteID, Guid parentID, Guid itemID) instead")]
        int GetCheckOutUserID(Guid siteID, Guid itemID);
        int GetCheckOutUserID(Guid siteID, Guid parentID, Guid itemID);
        bool UpdateDiskname(string diskName, string columnName, AveBaseItemInfo info);
        int GetCurrentItemInternalVersion(AveBaseItemInfo info, bool isEffectRecbin);
        IAveQueryDataReader GetContentFromDB(AveBaseItemInfo info);
        IAveQueryDataReader GetContentAndRbsIdFromDB(AveBaseItemInfo info);
        byte[] GetBlobIdByRbsId(byte[] rbsId);
        void UpdateTimeInfo(string createTime, string modifyTime, AveBaseItemInfo info);
        void UpdateResolutionAndDuration(string resolution, string duration, AveBaseItemInfo info);
        void UpdateOwnerInfo(int ownId, int modifierId, AveBaseItemInfo info);
        bool IsSP2010SP1(Guid siteID);
        void ClearRbsId(AveBaseItemInfo info);
        object[] GetItemsInRecycleBin(AveBaseItemInfo info);
        Dictionary<Guid, List<AveBaseItemInfo>> GetVersionsInRecycleBin(AveBaseItemInfo info);
        List<AveBaseItemInfo> GetItemVersionsInRecycleBin(AveBaseItemInfo info);
        bool ObjectExists(AveBaseItemInfo info, int objectType);
    }
}
