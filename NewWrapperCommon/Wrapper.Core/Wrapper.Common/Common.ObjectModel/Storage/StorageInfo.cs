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
using System.Text;

namespace AvePoint.Wrapper.Common
{
    public class AveStorageInfo
    {
        public long Size;
        public AveStorageType StorageType = AveStorageType.None;
        public bool IsBackupLinkForArchivedData = false;
        public AveRBSStubInfo RBSInfo;
        public AveStubDataType StubDataType = AveStubDataType.UnKnown;
        public string StubDBInfoBase64;
    }

    public class AveStorageInfo13
    {
        public long Size;
        public AveStorageType StorageType = AveStorageType.None;
        public bool IsBackupLinkForArchivedData = false;
        public List<AveShredStubInfo> ShredStubInfoList;
    }

    public class AveShredStubInfo
    {
        public AveRBSStubInfo13 RBSInfo;
        public AveStubDataType StubDataType = AveStubDataType.UnKnown;
        public string StubDBInfoBase64s;
    }

    //public class BSNPartition
    //{
    //    public long BSN;
    //    public byte Partition;
    //}

    public enum AveStorageType
    {
        None = 0,
        EBS = 1,
        RBS = 2
    }

    public class AveRBSStubInfo
    {
        public byte[] RBSId;
        public byte[] StoreBlobId;
        public byte[] StorePoolId;
        public string ProviderName = string.Empty;
        public long DataLength = 0;

        public AveRBSStubInfo()
        {

        }
        public AveRBSStubInfo(byte[] blobId, byte[] poolId, string providerName, long dataLength)
        {
            StoreBlobId = blobId;
            StorePoolId = poolId;
            ProviderName = providerName;
            DataLength = dataLength;
        }
    }

    public class AveRBSStubInfo13 : AveRBSStubInfo
    {
        //DocStreams table
        public long BSN;
        public byte partition;
        public int size;
        public byte type;
        public DateTime ExpirationUTC;

        //DocsToStreams table
        public int histVersion;
        public long streamId;
        public byte level;

        public byte[] newRBSId;
        public AveRBSStubInfo13(AveRBSStubInfo info)
        {
            this.RBSId = info.RBSId;
            this.StoreBlobId = info.StoreBlobId;
            this.StorePoolId = info.StorePoolId;
            this.ProviderName = info.ProviderName;
            this.DataLength = info.DataLength;
        }

        public AveRBSStubInfo13()
        {

        }

        public void CopyValue(AveRBSStubInfo13 stubInfo)
        {
            this.BSN = stubInfo.BSN;
            this.partition = stubInfo.partition;
            this.size = stubInfo.size;
            this.type = stubInfo.type;
            this.ExpirationUTC = stubInfo.ExpirationUTC;
            this.histVersion = stubInfo.histVersion;
            this.streamId = stubInfo.streamId;
            this.level = stubInfo.level;
        }
    }

    public enum AveStubDataType
    {
        Archiver = 1,
        Extender = 2,
        Connector = 4,
        UnKnown = -1
    }
}
