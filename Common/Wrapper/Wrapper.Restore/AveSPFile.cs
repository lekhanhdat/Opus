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




//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Text;
//using System.IO;
//using AvePoint.Wrapper.Common;

//namespace AvePoint.Wrapper.Restore
//{
//    public class AveSPFile : IDisposable
//    {
//        private AveSPSite mSPSite;
//        private AveSPFolder mSPFolder;
//        private IAveFile mAveFile;
//        private IAveListItem mListItem;
//        private AveItemSecurity mItemSecurity;
//        private AveSPListItem mAveSPListItem;
//        private AveSPItem mAveSPItem;
//        private IAveRestoreStream mRestoreStream;
//        private string mSrcUrl;
//        private string mUrl;
//        private long mSize;

//        public IAveFile File
//        {
//            get
//            {
//                return mAveFile;
//            }
//        }

//        public string SrcUrl
//        {
//            get
//            {
//                return mSrcUrl;
//            }
//        }

//        public string Url
//        {
//            get
//            {
//                return mUrl;
//            }
//        }

//        public long Size
//        {
//            get
//            {
//                return mSize;
//            }
//        }

//        public AveObjectSecurity Security
//        {
//            get
//            {
//                if (mItemSecurity == null)
//                {
//                    mItemSecurity = new AveItemSecurity(mAveSPListItem.AveSPItem);
//                }
//                return mItemSecurity;
//            }
//        }

//        public AveSPFile(AveSPList spList, IAveRestoreStream restoreStream)
//        {
//            mSPSite = spList.ParentSite;
//            mSPFolder = new AveSPFolder(spList, restoreStream);
//            mAveSPItem = new AveSPItem(spList, restoreStream);
//            mRestoreStream = restoreStream;
//        }

//        public AveSPFile(AveSPFolder spFolder, IAveRestoreStream restoreStream)
//        {
//            mSPFolder = spFolder;
//            mSPSite = spFolder.ParentSite;
//            mAveSPItem = new AveSPItem(spFolder.ParentList, restoreStream);
//            mRestoreStream = restoreStream;
//        }

//        public void RestoreFileSelf(AveItemFieldCollectionInfo fileFieldInfo)
//        {
//            mAveSPListItem.IsRestored = true;
//            IAveList list = mAveSPListItem.ParentList.SPList;
//            AveItemCreationInformation aici = new AveItemCreationInformation();
//            aici.FolderUrl = mSPFolder.SPFolder.ServerRelativeUrl;
//            aici.UnderlyingObjectType = AveFileSystemObjectType.File;
//            IAveListItem item = list.AddItem(aici);
//            mAveSPItem.RestoreItemProperty(fileFieldInfo, list, item);
//            mUrl = mAveFile.ServerRelativeUrl;
//            mSrcUrl = fileFieldInfo.SrcUrl;
//            mSize = fileFieldInfo.Size;
//        }

//        public void RestoreFileData(AveItemFieldCollectionInfo fileFieldColInfo)
//        {
//            if (fileFieldColInfo.VersionMode == (int)AveVersionMode.None)
//            {
//                RestoreNoVersionFile(fileFieldColInfo);
//            }
//            else
//            {
//                RestoreHasVersionFile(fileFieldColInfo);
//            }
//            mUrl = mAveSPListItem.ParentList.ParentSite.ApplicationName + mAveFile.ServerRelativeUrl;
//            mSrcUrl = fileFieldColInfo.SrcUrl;
//            mSize = fileFieldColInfo.Size;
//            mListItem = mAveFile.Item;
//            mAveSPItem.SPListItem = mListItem;
//        }

//        public void RestoreNoVersionFile(AveItemFieldCollectionInfo fileFieldColInfo)
//        {
//            mAveFile = mSPFolder.SPFolder.Files[fileFieldColInfo.OriginalName];
//            if (mAveFile == null)
//            {
//                RestoreFileByStream(fileFieldColInfo);
//            }
//        }

//        public void RestoreFileByStream(AveItemFieldCollectionInfo fileFieldColInfo)
//        {
//            mAveSPListItem.IsRestored = true;
//            Stream stream = new AveSPFileStream(mRestoreStream);
//            mAveFile = mSPFolder.SPFolder.Files.Add(fileFieldColInfo.OriginalName, stream, true);
//            mAveSPItem.RestoreItemProperty(fileFieldColInfo, mAveSPListItem.ParentList.SPList, mAveFile.Item, true);
//            mListItem = mAveFile.Item;
//            mAveSPItem.SPListItem = mListItem;
//        }

//        public void RestoreHasVersionFile(AveItemFieldCollectionInfo fileFieldColInfo)
//        {
//            if (fileFieldColInfo.VersionMode == (int)AveVersionMode.MajorVersion)
//            {
//                RestoreMajorVersionSettingFile(fileFieldColInfo);
//            }
//            else
//            {
//                RestoreMinorVersionSettingFile(fileFieldColInfo);
//            }
//        }

//        public void RestoreMajorVersionSettingFile(AveItemFieldCollectionInfo fileFieldColInfo)
//        {
//            mAveSPListItem.ParentList.EnableListVersioning(AveVersionMode.MajorVersion);
//            mAveFile = mSPFolder.SPFolder.Files[fileFieldColInfo.OriginalName];

//            if (mAveFile == null && fileFieldColInfo.Version.Equals("1.0"))
//            {
//                RestoreFileByStream(fileFieldColInfo);
//            }
//            else
//            {
//                string latestFileVersion = null;
//                List<string> skipedVersions = new List<string>();
//                if (mAveFile == null)
//                {
//                    CreateFirstVersionFile(fileFieldColInfo.OriginalName);
//                    skipedVersions.Add(latestFileVersion);
//                }

//                latestFileVersion = mAveFile.UIVersionLabel;
//                if (string.Compare(fileFieldColInfo.Version, latestFileVersion) > 0)
//                {
//                    UpdateFileVersion(mAveFile, GetPreviousMajorVersion(fileFieldColInfo.Version), latestFileVersion, skipedVersions);
//                    RestoreFileByStream(fileFieldColInfo);
//                }
//            }
//        }

//        public void RestoreMinorVersionSettingFile(AveItemFieldCollectionInfo fileFieldColInfo)
//        {
//            //mAveSPList.EnableListVersioning(AveVersionMode.MinorVersion);

//        }

//        public void RemoveSkipedVersion(IAveFile aveFile, List<string> skipedVersions)
//        {
//            foreach (IAveFileVersion fileVersion in aveFile.Versions)
//            {
//                if (skipedVersions.Contains(fileVersion.VersionLabel))
//                {
//                    fileVersion.Delete();
//                }
//            }
//        }

//        private void CreateFirstVersionFile(string fileName)
//        {
//            IAveList list = mAveSPListItem.ParentList.SPList;
//            AveFileCreationInformation fci = new AveFileCreationInformation();
//            fci.Content = new byte[1];
//            fci.Url = mSPFolder.SPFolder.ServerRelativeUrl + "/" + fileName;
//            fci.Overwrite = true;
//            mAveFile = mSPFolder.SPFolder.Files.Add(fci);
//        }

//        private void RestoreFileDataAndUpdateProperty(string fileName, AveItemFieldCollectionInfo fileFieldColInfo)
//        {
//            Stream stream = new AveSPFileStream(mRestoreStream);
//            IAveFile aveFile = mSPFolder.SPFolder.Files.Add(fileName, stream, true);
//            mAveSPItem.RestoreItemProperty(fileFieldColInfo, mAveSPListItem.ParentList.SPList, aveFile.Item);
//        }

//        private void UpdateFileVersion(IAveFile aveFile, string currentFileVersion, string latestFileVersion, List<string> skipedVersions)
//        {
//            if (string.Compare(currentFileVersion, latestFileVersion) > 0)
//            {
//                int[] latestVersion = GetMajorAndMinorVersion(latestFileVersion);
//                int[] currentVersion = GetMajorAndMinorVersion(currentFileVersion);

//                int majorVersionDelta = currentVersion[0] - latestVersion[0];
//                while (majorVersionDelta-- > 0)
//                {
//                    aveFile.CheckOut();
//                    aveFile.CheckIn(string.Empty, AveCheckinType.MajorCheckIn);
//                    skipedVersions.Add(latestVersion[0]++ + Convert.ToString(latestVersion[1]));
//                }

//                aveFile.Update();
//            }
//        }


//        private string GetPreviousMajorVersion(string version)
//        {
//            float fVersion = Convert.ToSingle(version);
//            return Convert.ToString(fVersion - 1);
//        }

//        private int[] GetMajorAndMinorVersion(string versionStr)
//        {
//            int[] version = new int[2];
//            string[] versions = versionStr.Split('.');
//            version[0] = Convert.ToInt32(versions[0]);
//            if (versions.Length == 2)
//            {
//                version[1] = Convert.ToInt32(versions[1]);
//            }
//            else
//            {
//                version[1] = 0;
//            }
//            return version;
//        }

//        #region IDisposable Members

//        public void Dispose()
//        {

//        }

//        #endregion
//    }
//}
