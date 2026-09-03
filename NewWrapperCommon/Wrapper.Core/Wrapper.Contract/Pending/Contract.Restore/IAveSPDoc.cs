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
using System.Collections;
using System.Collections.Generic;
using AvePoint.Wrapper.Common;

namespace AvePoint.Wrapper.Restore
{
    public interface IAveSPDoc:IAveSPItem
    {
        //IAveSPItem AveSPItem { get; }
        AveViewDocInfo AveView { get; }
        bool CheckIfOnlyDoDiscard();
        bool? ConflictWithDocument { get; }
        void CreateSouAndDesDefaultValueWithStream(IAveList list, IAveFile spFile, IAveSPList aveSPList, bool overWrite);
        bool DestinationExist();
        bool HasStream { get; set; }
        bool IsCurrentVersion { get; }
        bool IsNewCreateView { get; }
        void MergeSouAndDesDefaultValueWithStream(IAveList list, IAveFile spFile, IAveSPList aveSPList, bool overWrite);
        bool NeedAppendNewVersion(DateTime timeLastModified);
        bool NeedChangeItemId { get; set; }
        void OverWriteRetionStream(IAveList list, IAveFile spFile, IAveSPList aveSPList, bool overWrite);
        void ProcessGhostInfo(Dictionary<string, object> allDocData);
        void ProcessViewInfo(Dictionary<string, object> allDocData);
        IAveRestoreStream Receiver { get; set; }
        string ResetAvailableName();
        string ResetAvailableName(DateTime timeLastModified);
        string ResetAvailableName(DateTime timeLastModified,bool isLinkFile);
        string ResetAvailableName(string oldName, bool needIncluded);
        void ResetParentFolder(IAveSPFolder parentFolder);
        void ResetParentFolder(bool moveUptoRootFolder, bool moveUptoHighLevelFolder);
        void ResetParentFolder(int maxUrlLength);
        void RestoreAlert(Dictionary<string, object> data, bool isSchedAlert);
        void RestoreSPComments(List<AveCommentInfo> comments, bool overwrite);
        AveRestoreResult RestoreSelf(Dictionary<string, object> allDocData, Dictionary<string, object> allUserData);
        AveRestoreResult RestoreSelf(Dictionary<string, object> allDocData, Dictionary<string, object> allUserData,
            List<Dictionary<string, object>> junctionData, List<AveWebPartBaseInfo> webParts);
        void RestoreWebPart(IList webPartList, bool clearAllBeforeRestore);
        void SetStream(IAveRestoreStream stream);
        long Size { get; }
        IAveFile SPFile { get; set; }
        IAveView SPView { get; }
        string TagUrl { get; }
        string Url { get; }
        IAveWeb Web { get; set; }
    }
}
