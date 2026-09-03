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



using Microsoft.SharePoint;
using AvePoint.Wrapper.Common;

namespace AvePoint.ObjectModel.Server13
{
    class AveFileVersionCollection : AveAbstractCommonCollection<IAveFileVersion>, IAveFileVersionCollection
    {
        private SPFileVersionCollection mFileVersions;
        private AveWeb mWeb;

        public AveFileVersionCollection(AveWeb web, SPFileVersionCollection fileversions)
            : base(fileversions)
        {
            mWeb = web;
            mFileVersions = fileversions;
        }

        #region IAveFileVersionCollection Members

        public void DeleteAll()
        {
            mFileVersions.DeleteAll();
        }

        public void DeleteAllMinorVersions()
        {
            mFileVersions.DeleteAllMinorVersions();
        }

        public void RecycleAll()
        {
            mFileVersions.RecycleAll();
        }

        public void RecycleAllMinorVersions()
        {
            mFileVersions.RecycleAllMinorVersions();
        }

        public void DeleteByID(int vid)
        {
            mFileVersions.DeleteByID(vid);
        }

        public void DeleteByLabel(string versionlabel)
        {
            mFileVersions.DeleteByLabel(versionlabel);
        }

        public void RestoreByLabel(string versionlabel)
        {
            mFileVersions.RestoreByLabel(versionlabel);
        }

        public IAveFileVersion GetVersionFromID(int versionid)
        {
            SPFileVersion fileVersion = mFileVersions.GetVersionFromID(versionid);
            if (fileVersion != null)
            {
                return new AveFileVersion(mWeb, fileVersion);
            }
            return null;
        }
        public IAveFileVersion GetVersionFromID2(int versionid)
        {
            return null;
        }

        public override IAveFileVersion this[int index]
        {
            get
            {
                return new AveFileVersion(mWeb, mFileVersions[index]);
            }
        }

        public bool IsDirty
        {
            get
            {
                object result = AveAssemblyUtility.GetPropertyValue(mFileVersions, "IsDirty");
                return result != null ? (bool)result : false;
            }
        }

        protected override object CreatElementInstance(object t)
        {
            return new AveFileVersion(mWeb, t as SPFileVersion);
        }

        public override int Count
        {
            get { return mFileVersions.Count; }
        }

        public IAveWeb Web
        {
            get { return mWeb; }
        }

        #endregion

        #region IAveFileVersionCollection Members


        public IAveFileVersion GetVersionFromLabel(string versionlabel)
        {
            return new AveFileVersion(mWeb,mFileVersions.GetVersionFromLabel(versionlabel));
        }

        #endregion

        #region IEnumerable Members

        public new System.Collections.IEnumerator GetEnumerator()
        {
            throw new System.NotImplementedException();
        }

        #endregion
       
    }
}
