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
    class AveFileVersionCollection : AveAbstractCommonCollection<IAveFileVersion>, IAveFileVersionCollection
    {
        private IAveRequest mRequest;
        private AveWeb mParentWeb;
        private AveFile mFile;

        public AveFileVersionCollection(IAveRequest request, IAveWeb parentWeb, AveFile file, Dictionary<string, object> prop)
        {
            mRequest = request;
            mParentWeb = parentWeb as AveWeb;
            mFile = file;
            base.DataCache.AddPropertyies(prop);
            InitFileVersions();
        }

        private void InitFileVersions() 
        {
            mListData = new List<IAveFileVersion>();
            foreach (var dic in base.DataCache.GetChildren())
            {
                AveFileVersion version = new AveFileVersion(mParentWeb, this, mRequest, dic);
                mListData.Add(version);
            }
        }

        public void DeleteAll()
        {
            this.mRequest.DeleteFileVersions(this.mParentWeb.ServerRelativeUrl, this.mFile.ServerRelativeUrl);
            mListData.Clear();
        }
        public void DeleteByID(int vid)
        {
            this.mRequest.DeleteFileVersion(this.mParentWeb.ServerRelativeUrl, this.mFile.ServerRelativeUrl, vid);
            mListData.Remove(this.GetVersionFromID(vid));          
        }
        public List<int> DeleteByIDs(List<int> vid)
        {
            var failedIds = this.mRequest.DeleteFileVersionSpecificNumber(this.mParentWeb.ServerRelativeUrl, this.mFile.ServerRelativeUrl, vid);
            foreach (int id in vid)
            {
                mListData.Remove(this.GetVersionFromID(id));
            }
            return failedIds;
        }
        public void RecycleByID(int vid)
        {
            this.mRequest.RecycleFileVersion(this.mParentWeb.ServerRelativeUrl, this.mFile.ServerRelativeUrl, vid);
            mListData.Remove(this.GetVersionFromID(vid));
        }
        public void DeleteByLabel(string versionlabel)
        {
            this.mRequest.DeleteFileVersion(this.mFile.ServerRelativeUrl, this.mParentWeb.ServerRelativeUrl, versionlabel);
            mListData.Remove(this.GetVersionByVersionLabel(versionlabel));          
        }
        public IAveFileVersion GetVersionFromID(int versionid)
        {
            return mListData.Find(
                    delegate(IAveFileVersion fileVersion)
                    {
                        return fileVersion.ID.Equals(versionid);
                    });
        }
        public void RestoreByLabel(string versionlabel)
        {
            this.mRequest.RestoreFileVersion(versionlabel, this.mParentWeb.ServerRelativeUrl, this.mFile.ServerRelativeUrl);
        }
        public IAveFileVersion this[int index]
        {
            get
            {
                return mListData[index];
            }
        }
        public IAveFileVersion GetVersionByVersionLabel(string versionlabel)
        {
            return mListData.Find(
                        delegate(IAveFileVersion fileVersion)
                        {
                            return fileVersion.VersionLabel.Equals(versionlabel);
                        });
        }


        public IAveWeb Web
        {
            get { return mParentWeb; }
        }

        internal AveFile File
        {
            get { return mFile; }
        }

        #region IAveFileVersionCollection Members


        public IAveFileVersion GetVersionFromLabel(string versionlabel)
        {
            return GetVersionByVersionLabel(versionlabel);
        }

        #endregion

        #region IEnumerable Members

        public System.Collections.IEnumerator GetEnumerator()
        {
            throw new NotImplementedException();
        }

        #endregion
    }
}
