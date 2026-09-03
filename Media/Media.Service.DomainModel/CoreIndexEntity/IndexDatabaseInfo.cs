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


namespace AvePoint.Media.Service.DomainModel
{
    #region using directives

    using System;
    using Storage;
    using GCommon.Contract.Server.ControlPanel.Cryptography;
    #endregion using directives

    public class IndexDatabaseInfo
    {
        public Boolean IsNeedCreateNewIndex { get; set; }

        public Boolean NeedDownLoad { get; set; }

        public Boolean NeedRenameIndexName { get; set; }

        public String DbFileName { get; set; }

        public String IndexVolume { get; set; }

        public String SourceIndexVolume { get; set; }

        public String StorageInfo { get; set; }

        public String previousPlanId { get; set; }

        public String previousCycleId { get; set; }


        public IXSystem SourceIndexLogicalDevice { get; set; }

        public IXSystem DestinationIndexLogicalDevice { get; set; }

        public byte DataMode { get; set; }
        public DataEncryptionInfo EncryptionInfo { get; set; }
        public bool IsForceUpload { get; set; }

        public IndexDatabaseInfo()
        { }

        public IndexDatabaseInfo(String dbName, IndexServiceOpenParameter openParameter)
        {
            DbFileName = dbName;
            EncryptionInfo = openParameter?.EncryptionInfo;
            DataMode = openParameter?.DataMode ?? 0;
        }

        public IndexDatabaseInfo(String dbName, String storageInfo, IndexServiceOpenParameter openParameter)
        {
            DbFileName = dbName;
            StorageInfo = storageInfo;
            EncryptionInfo = openParameter?.EncryptionInfo;
            DataMode = openParameter?.DataMode ?? 0;
        }

        public IndexDatabaseInfo(IndexServiceOpenParameter openParameter)
        {
            StorageInfo = openParameter.StorageInfo;
            DbFileName = openParameter.IndexDatabaseName;
            IsNeedCreateNewIndex = openParameter.IsNeedCreateNewIndex;
            NeedDownLoad = openParameter.NeedDownLoad;
            IndexVolume = openParameter.IndexVolume;
            EncryptionInfo = openParameter.EncryptionInfo;
            DataMode = openParameter.DataMode;
        }

        public override string ToString()
        {
            return string.Format("IndexDatabaseInfo StorageInfo : {0}, IsNeedCreateNewIndex : {1}, NeedDownLoad : {2}, DbFileName : {3}",
                StorageInfo, IsNeedCreateNewIndex, NeedDownLoad, DbFileName);
        }
    }
}