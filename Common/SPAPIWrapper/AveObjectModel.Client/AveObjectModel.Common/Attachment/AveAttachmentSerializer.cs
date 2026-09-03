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
using AvePoint.Wrapper.Common;
using AveClientRequest.Common;

namespace AvePoint.ObjectModel.Common
{
    class AveAttachmentSerializer : IAveAttachmentSerializer
    {
        private AveList mParentList;
        private IAveRequest mRequest;
        private AveRestoreOption mRestoreOption;

        public AveAttachmentSerializer(AveList parentList, IAveRequest request, AveRestoreOption restoreOption)
        {
            mParentList = parentList;
            mRequest = request;
            mRestoreOption = restoreOption;
        }

        public IAveAttachment RestoreAttachment(AveAttachmentInfo info, IAveRestoreStream receiver, bool inplaceRestore = false)
        {
            info.ParentWebRelativeUrl = mParentList.ParentWeb.ServerRelativeUrl;
            info.ParentListTitle = mParentList.Title;
            info.ParentListId = mParentList.ID;
            AveAttachment newAttach = null;
            Dictionary<string, object> docData = AveList.AssembleBaseItemInfo(info, mParentList);
            if (inplaceRestore && Convert.ToInt32(docData["DestRowId"]) == -1)
            {
                docData["DestRowId"] = info.OriginalRowId;
            }

            if (Convert.ToInt32(docData["DestRowId"]) != -1)
            {
                docData["Name"] = info.RealName;
                docData["RestoreOption"] = mRestoreOption;
                this.GetAttachmentStorageInfo(receiver);
                Dictionary<string, object> restoreResult = mRequest.RestoreAttachment(mParentList.ParentWeb.Url,docData, new AveSPFileStream(receiver));
                newAttach = new AveAttachment(restoreResult, null);
            }
            else
            {
                throw new AveRestoreException(AveRestoreResult.Omit, AveRestoreResult.Omit.ToString());
            }
            return newAttach;
        }

        internal void GetAttachmentStorageInfo(IAveRestoreStream stream)
        {
            AveStorageInfo mStorageInfo = null;
            AveMetadata metadata = stream.TryReadMetadata(AveMetadataType.DocStorageInfo);
            if (null != metadata)
            {
                mStorageInfo = metadata.GetMetadata<AveStorageInfo>();
            }
        }
    }
}
