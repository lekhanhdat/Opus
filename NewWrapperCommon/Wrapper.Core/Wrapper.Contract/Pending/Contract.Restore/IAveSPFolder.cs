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
    public interface IAveSPFolder : IAveSPItem
    {
        //IAveSPItem AveSPItem { get; }
        CurrentRestoreDocStatus CurrentDocStatus { get; set; }
        IAveSPItem EnsureCTFieldItem { get; }
        void GetOrCreateFolder();
        bool HasMoveUp { get; }
        void InitSPFolder();
        void InitSPFolder(bool tryCreate);
        void ReloadFolder(bool force = true);
        string ResetAvailableName();
        string ResetAvailableName(string oldName, bool needIncluded);
        void ResetParentFolder(IAveSPFolder parentFolder);
        void ResetParentFolder(bool moveUptoRootFolder, bool moveUptoHighLevelFolder, bool needResetName);
        void ResetParentFolder(int maxUrlLength, bool needResetName);
        AvePoint.Wrapper.Common.AveRestoreResult RestoreSelf(System.Collections.Generic.Dictionary<string, object> allDocData, System.Collections.Generic.Dictionary<string, object> allUserData);
        AvePoint.Wrapper.Common.AveRestoreResult RestoreSelf(System.Collections.Generic.Dictionary<string, object> allDocData,
            System.Collections.Generic.Dictionary<string, object> allUserData,
            System.Collections.Generic.List<System.Collections.Generic.Dictionary<string, object>> allDataJunction);
        AvePoint.Wrapper.Common.RestoringDto RestoringItem { get; }
        string ServerRelativeUrl { get; }
        bool IsRestoreConnectorFolderProperties { get; set; }
        long Size { get; }
        AvePoint.Wrapper.Common.IAveFolder SPFolder { get; set; }
        string SrcUrl { get; }
        string TagUrl { get; }
        string Url { get; }
    }
}
