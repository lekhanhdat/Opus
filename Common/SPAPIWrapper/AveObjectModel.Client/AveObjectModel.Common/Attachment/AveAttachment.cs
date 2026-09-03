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
namespace AvePoint.ObjectModel.Common
{
    class AveAttachment : AveClientObject, IAveAttachment
    {
        private AveAttachmentInfo mAttachmentInfo;
        private AveListItem mListItem;

        public AveAttachment(AveAttachmentInfo info, IAveListItem listItem)
        {
            mAttachmentInfo = info;
            mListItem = listItem as AveListItem;
            if (!string.IsNullOrEmpty(info.RealName))
            {
                base.DataCache.AddProperty("FileName",info.RealName);
            }
        }

        public AveAttachment(IDictionary<string, object> attachmentProperties, IAveListItem listItem)
        {
            base.DataCache.AddPropertyies(attachmentProperties);
            mListItem = listItem as AveListItem;
        }


        #region IAveAttachment Members

        public string FileName
        {
            get
            {
                return base.DataCache.GetProperty<string>("FileName");
            }
        }

        public Guid ROWID
        {
            get 
            {
                return base.DataCache.GetProperty<Guid>("ROWID");
            }
        }

        public string ServerRelativeUrl
        {
            get 
            {
                return base.DataCache.GetProperty<string>("ServerRelativeUrl");
            }
        }

        #endregion


        public Guid GetParentId()
        {
            throw new NotImplementedException();
        }

        public void Delete()
        {
            mListItem.Attachments.Delete(this.FileName);
            //throw new NotImplementedException();
        }

        #region IAveAttachment Members


        public bool Exists(AveAttachmentInfo mAttachmentInfo)
        {
            throw new NotImplementedException();
        }

        #endregion
    }
}
