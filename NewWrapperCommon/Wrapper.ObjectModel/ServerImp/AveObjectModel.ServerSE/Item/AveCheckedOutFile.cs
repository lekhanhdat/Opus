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

namespace AvePoint.ObjectModel.ServerSE
{
    class AveCheckedOutFile : IAveCheckedOutFile, IDisposable
    {
        private SPCheckedOutFile mCheckedOutFile;
        private AveWeb mWeb;
        private AveFile mFile;

        public AveCheckedOutFile(AveWeb web, SPCheckedOutFile checkedOutFile)
        {
            mWeb = web;
            mCheckedOutFile = checkedOutFile;
        }

        #region IAveCheckedOutFile Members

        public string CheckedOutByName
        {
            get
            {
                return mCheckedOutFile.CheckedOutByName;
            }
        }

        public string DirName
        {
            get
            {
                return mCheckedOutFile.DirName;
            }
        }

        public string LeafName
        {
            get
            {
                return mCheckedOutFile.LeafName;
            }
        }

        public long Length
        {
            get
            {
                return mCheckedOutFile.Length;
            }
        }

        public DateTime TimeLastModified
        {
            get
            {
                return mCheckedOutFile.TimeLastModified;
            }
        }

        public IAveUser CheckedOutBy
        {
            get
            {
                return new AveUser(null, mCheckedOutFile.CheckedOutBy);
            }
        }

        public void TakeOverCheckOut()
        {
            mCheckedOutFile.TakeOverCheckOut();
        }

        public IAveFile File
        {
            get
            {
                if (mFile == null)
                {
                    mFile = new AveFile(mWeb, (SPFile)AveAssemblyUtility.GetPropertyValue(mCheckedOutFile, "File"));
                }
                return mFile;
            }
        }

        public int ListItemId
        {
            get
            {
                return mCheckedOutFile.ListItemId;
            }
        }

        public string Url
        {
            get { return mCheckedOutFile.Url; }
        }

        public int CheckedOutById
        {
            get { return mCheckedOutFile.CheckedOutById; }
        }

        #endregion

        public void Dispose()
        {
            if (mFile != null)
                mFile.Dispose();
        }
    }

}
