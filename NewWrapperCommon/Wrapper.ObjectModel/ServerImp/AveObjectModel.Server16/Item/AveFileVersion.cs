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
using System.IO;
using Microsoft.SharePoint;
using AvePoint.Wrapper.Common;

namespace AvePoint.ObjectModel.Server16
{
    class AveFileVersion : AveServerObject, IAveFileVersion
    {
        private SPFileVersion mFileVersion;
        private AveUser mCreatedBy;
        private AveWeb mWeb;

        public AveFileVersion(AveWeb web, SPFileVersion fileVersion)
        {
            mWeb = web;
            mFileVersion = fileVersion;
        }

        #region IAveFileVersion Members

        public string CheckInComment
        {
            get
            {
                return mFileVersion.CheckInComment;
            }
        }

        public DateTime Created
        {
            get
            {
                return mFileVersion.Created;
            }
        }

        public IAveUser CreatedBy
        {
            get
            {
                if (mCreatedBy == null)
                {
                    SPUser user = mFileVersion.CreatedBy;
                    if (user != null)
                    {
                        mCreatedBy = new AveUser(mWeb, user);
                    }
                }
                return mCreatedBy;
            }
        }

        public int ID
        {
            get
            {
                return mFileVersion.ID;
            }
        }

        public bool IsCurrentVersion
        {
            get
            {
                return mFileVersion.IsCurrentVersion;
            }
        }

        public int Size
        {
            get { return mFileVersion.Size; }
        }

        public string Url
        {
            get
            {
                return mFileVersion.Url;
            }
        }

        public string VersionLabel
        {
            get { return mFileVersion.VersionLabel; }
        }

        public void Delete()
        {
            mFileVersion.Delete();
        }

        public Stream OpenBinaryStream()
        {
            return mFileVersion.OpenBinaryStream();
        }

        public void Recycle()
        {
            mFileVersion.Recycle();
        }

        #endregion

        #region IAveFileVersion Members


        public byte[] OpenBinary()
        {
            return mFileVersion.OpenBinary();
        }

        #endregion


        public AveFileLevel Level
        {
            get { return (AveFileLevel)mFileVersion.Level; }
        }

        public System.Collections.Hashtable Properties
        {
            get { return mFileVersion.Properties; }
        }
    }
}
