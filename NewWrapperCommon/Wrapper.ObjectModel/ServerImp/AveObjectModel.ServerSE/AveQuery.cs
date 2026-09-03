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



using AvePoint.Wrapper.Common;
using Microsoft.SharePoint;
using System;

namespace AvePoint.ObjectModel.ServerSE
{
    class AveQuery : IAveQuery, IDisposable
    {
        private SPQuery mQuery;
        private AveFolder mFolder;
        private AveList mList;

        internal SPQuery spQuery
        {
            get { return mQuery; }
        }

        public AveQuery()
        {
            mQuery = new SPQuery();
        }

        public AveQuery(AveList list, SPQuery query)
        {
            mList = list;
            mQuery = query;
        }

        #region IAveQuery Members

        public IAveFolder Folder
        {
            get
            {
                SPFolder folder = mQuery.Folder;
                if (folder == null)
                {
                    return null;
                }
                else if (mFolder == null)
                {
                    mFolder = new AveFolder(mList.ParentWeb as AveWeb, folder);
                }
                return mFolder;
            }
            set
            {
                mFolder = value as AveFolder;
                if (mFolder != null)
                {
                    mQuery.Folder = mFolder.Folder;
                }
                else
                {
                    mQuery.Folder = null;
                }
            }
        }

        public IAveListItemCollectionPosition ListItemCollectionPosition
        {
            get
            {
                if (mQuery.ListItemCollectionPosition != null)
                {
                    return new AveListItemCollectionPosition(mQuery.ListItemCollectionPosition);
                }
                return null;
            }
            set
            {
                if (value != null)
                {
                    mQuery.ListItemCollectionPosition = new SPListItemCollectionPosition(value.PagingInfo);
                }
                else
                {
                    mQuery.ListItemCollectionPosition = null;
                }
            }
        }

        public uint RowLimit
        {
            get
            {
                return mQuery.RowLimit;
            }
            set
            {
                mQuery.RowLimit = value;
            }
        }

        public string QueryString
        {
            get
            {
                return mQuery.Query;
            }
            set
            {
                mQuery.Query = value;
            }
        }

        public string ViewXml
        {
            get
            {
                return mQuery.ViewXml;
            }
            set
            {
                mQuery.ViewXml = value;
            }
        }

        public string ViewFields
        {
            get
            {
                return mQuery.ViewFields;
            }
            set
            {
                mQuery.ViewFields = value;
            }
        }

        public string Query
        {
            get
            {
                return mQuery.Query;
            }
            set
            {
                mQuery.Query = value;
            }
        }

        public string ViewAttributes
        {
            get
            {
                return mQuery.ViewAttributes;
            }
            set
            {
                mQuery.ViewAttributes = value;
            }
        }
		
		public bool ViewFieldsOnly 
        {
           get
            {
                return mQuery.ViewFieldsOnly;
            }
            set
            {
                mQuery.ViewFieldsOnly = value;
            }
        }

        public AveQueryThrottleOption QueryThrottleMode
        {
            get
            {
                return (AveQueryThrottleOption)mQuery.QueryThrottleMode;
            }
            set
            {
                mQuery.QueryThrottleMode = (SPQueryThrottleOption)value;
            }
        }

        #endregion

        #region IDisposable Members

        public void Dispose()
        {
            if (mFolder != null)
            {
                mFolder.Dispose();
                mFolder = null;
            }
        }

        #endregion
    }
}
