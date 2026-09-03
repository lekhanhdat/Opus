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
namespace AvePoint.ObjectModel.Common
{
    using System;
    using System.Collections.Generic;
    using AvePoint.Wrapper.Common;
    class AveCheckedOutFile : AveClientObject, IAveCheckedOutFile
    {
        private IAveRequest m_Request;
        private AveDocumentLibrary m_AveDocumentLibrary;

        public AveCheckedOutFile(IAveRequest mRequest, AveDocumentLibrary aveDocumentLibrary, Dictionary<string, object> fileProperties)
        {
            this.m_Request = mRequest;
            this.m_AveDocumentLibrary = aveDocumentLibrary;
            base.DataCache.AddPropertyies(fileProperties);
        }

        public string CheckedOutByName
        {
            get { return base.DataCache.GetProperty<string>("CheckedOutByName"); }
        }

        public string DirName
        {
            get { return base.DataCache.GetProperty<string>("DirName"); }
        }

        public string LeafName
        {
            get { return base.DataCache.GetProperty<string>("LeafName"); }
        }

        public long Length
        {
            get
            {
                if (base.DataCache.IsPropertyNotLoaded("Length") && base.DataCache.IsPropertyAvailable("FileSize"))
                {
                    string mFileSize = base.DataCache.GetProperty<string>("FileSize");
                    double sizeBytes = 0;
                    if (mFileSize.StartsWith("LT"))
                    {
                        sizeBytes = 1;
                    }
                    else if (mFileSize.EndsWith("KB"))
                    {
                        mFileSize = mFileSize.TrimEnd('B').TrimEnd('K');
                        sizeBytes = Convert.ToDouble(mFileSize) * 1024;
                    }
                    else if (mFileSize.EndsWith("MB"))
                    {
                        mFileSize = mFileSize.TrimEnd('B').TrimEnd('M');
                        sizeBytes = Convert.ToDouble(mFileSize) * 1024 * 1024;
                    }
                    else if (mFileSize.EndsWith("GB"))
                    {
                        mFileSize = mFileSize.TrimEnd('B').TrimEnd('G');
                        sizeBytes = Convert.ToDouble(mFileSize) * 1024 * 1024 * 1024;
                    }
                    base.DataCache.AddProperty("Length", Convert.ToInt64(sizeBytes));
                }
                return base.DataCache.GetProperty<long>("Length");
            }
        }

        public DateTime TimeLastModified
        {
            get { return base.DataCache.GetProperty<DateTime>("TimeLastModified"); }
        }

        public IAveUser CheckedOutBy//attention!
        {
            get { return base.DataCache.GetProperty<AveUser>("CheckedOutBy"); }
        }

        #region IAveCheckedOutFile Members


        public void TakeOverCheckOut()
        {
            throw new NotImplementedException();
        }

        public IAveFile File
        {
            get
            { 
                throw new NotImplementedException();
            }
        }

        #endregion


        public int ListItemId
        {
            get { return base.DataCache.GetProperty<int>("ListItemId"); }
        }
    }
}
