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
using Microsoft.SharePoint;
using AvePoint.Wrapper.Common;
using System.Diagnostics.CodeAnalysis;

namespace AvePoint.ObjectModel.ServerSE
{
    class AveListItemVersion : IAveListItemVersion
    {
        private SPListItemVersion mListItemVersion;
        private AveFieldUserValue mCreatedBy;
        private AveListItemVersionCollection mListItemVersions;

        public AveListItemVersion(AveListItemVersionCollection listItemVersions, SPListItemVersion listItemVersion)
        {
            mListItemVersions = listItemVersions;
            mListItemVersion = listItemVersion;
        }

        #region IAveListItemVersion Members

        public DateTime Created
        {
            get { return mListItemVersion.Created; }
        }

        public IAveFieldUserValue CreatedBy
        {
            get
            {
                if (mCreatedBy == null)
                {
                    mCreatedBy = new AveFieldUserValue(mListItemVersion.CreatedBy);
                }
                return mCreatedBy;
            }
        }

        public IAveFieldCollection Fields
        {
            get
            {
                return this.ListItem.ParentList.Fields;
            }
        }

        public bool IsCurrentVersion
        {
            get { return mListItemVersion.IsCurrentVersion; }
        }

        public object this[string fieldName]
        {
            get { return mListItemVersion[fieldName]; }
        }

        public object this[int index]
        {
            get { return mListItemVersion[index]; }
        }

        public AveFileLevel Level
        {
            get { return (AveFileLevel)mListItemVersion.Level; }
        }

        public IAveListItem ListItem
        {
            get
            {
                return mListItemVersions.ListItem;
            }
        }

        public string Url
        {
            get { return mListItemVersion.Url; }
        }

        public int VersionId
        {
            get { return mListItemVersion.VersionId; }
        }

        public string VersionLabel
        {
            get { return mListItemVersion.VersionLabel; }
        }
        [SuppressMessage("FxCopCustomRules", "C100003:DoNotUseSpecificSPMethod")]
        public void Delete()
        {
            mListItemVersion.Delete();
        }

        public void Recycle()
        {
            mListItemVersion.Recycle();
        }

        #endregion
    }
}
