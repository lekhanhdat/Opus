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
using System.Data;
using System.Data.SqlClient;
using System.IO;
using AvePoint.Wrapper.Resource.ServerAPI2010;
using System.Globalization;
using System.Xml;
using AvePoint.GCommon;

namespace AvePoint.Wrapper.QueryService
{
    /// <summary>
    /// 此接口 storagemanager 暂时没有使用
    /// </summary>
    internal partial class AveQueryService
    {
        private const int BLOB_LENGTH = 20;
        private const int RBS_RBSID_LENGTH = 64;
        private const string RBS_PROVUDERNAME = "SP2010RBSProvider";


        private enum SOBlobProviderType : byte
        {//参与了位运算, 请不要修改枚举对应的int值.
            Unknown = 0,
            EBS = 1,
            RBS = 2
        }


        public List<object> GetPoolList(Guid siteId)
        {
            throw new NotImplementedException();
        }

        public void CheckPoolCapacity(ref Dictionary<Guid, bool> results)
        {
            throw new NotImplementedException();
        }

        public void ConvertToContent(Guid siteId, Guid itemId, int level, int uiVersion, int internalVersion, int blobType, Stream stream, int targetTable, bool blobUseCRC32, long stubCRC)
        {
            throw new NotImplementedException();
        }

        public void CreatePool(byte[] blobInfo, bool canStoreNewBlobs, Guid siteId)
        {
            throw new NotImplementedException();
        }

        public byte[] GetBlobIdFromContentDB(Guid siteId, Guid itemId, int level, int uiVersion, int internalVersion, int targetTable)
        {
            throw new NotImplementedException();
        }

        public long EnsureContentSize(Guid siteId, Guid itemId, int internalVersion)
        {
            throw new NotImplementedException();
        }

        public int ReadContentOfItem(Guid siteId, Guid itemId, int internalVersion, long position, byte[] buffer, int offset, int count)
        {
            throw new NotImplementedException();
        }

        public void CreateEBSBlob(Guid siteId, Guid itemId, int level, int uiVersion, int internalVersion, byte[] blobId, int targetTable)
        {
            throw new NotImplementedException();
        }

        public void CreateRBSBlob(Guid siteId, Guid itemId, int level, int uiVersion, int internalVersion, byte[] poolId, byte[] blobId, long blobSize, int targetTable, bool updateOnly)
        {
            throw new NotImplementedException();
        }

        public int EnsureInternalVersion(Guid siteId, Guid itemId, int level, int uiVersion, ref int targetTable, ref int internalVersion)
        {
            throw new NotImplementedException();
        }

        public void ModifyExtDoc(Guid id, Guid siteId)
        {
            throw new NotImplementedException();
        }

        public void ModifyExtDoc(Guid id, Guid parentId, Guid siteId)
        {
            throw new NotImplementedException();
        }
    }
}
