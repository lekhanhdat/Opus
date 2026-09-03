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
    public interface IAveSPListItem:IAveSPItem
    {
        //IAveSPItem AveSPItem { get; }
        bool? ConflictWithDocument { get; }
        bool DestinationExist();
        AvePoint.Wrapper.Common.IAveListItem GetCurrentSPListItem(System.Collections.Generic.Dictionary<string, object> data);
        bool IsWorkflowTask(System.Collections.Generic.Dictionary<string, object> userData);
        bool NeedAppendNewVersion(DateTime modified);
        bool NeedChangeItemId { get; set; }
        string ResetAvailableName();
        string ResetAvailableName(DateTime modified);
        AvePoint.Wrapper.Common.AveRestoreResult RestoreSelf(System.Collections.Generic.Dictionary<string, object> allDocData, System.Collections.Generic.Dictionary<string, object> allUserData);
        AvePoint.Wrapper.Common.AveRestoreResult RestoreSelf(System.Collections.Generic.Dictionary<string, object> allDocData, 
            System.Collections.Generic.Dictionary<string, object> allUserData, 
            System.Collections.Generic.List<System.Collections.Generic.Dictionary<string, object>> allDataJunction);
        bool RestoreUserInfo(System.Collections.Generic.Dictionary<string, object> userData);
        bool RestoreUserInfo(System.Collections.Generic.Dictionary<string, object> userData, bool forceRestore);
        //IAveObjectSecurity Security { get; }
        long Size { get; }
        string SrcUrl { get; }
        string TagUrl { get; }
        string Url { get; }
    }
}
