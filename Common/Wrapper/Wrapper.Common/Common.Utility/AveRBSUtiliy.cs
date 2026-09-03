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
using System.Xml;
using System.Data;
using System.Data.SqlClient;
using System.Xml.Serialization;
using System.Globalization;
using AvePoint.Common;
using AvePoint.GCommon;
using AvePoint.Wrapper.Resource;

namespace AvePoint.Wrapper.Common
{
    #region AveRBSModuleType
    public enum AveRBSModuleType
    {
        Invalid = 0,
        Archiver = 1,
        Extender = 2,
        Connector = 4
    }
    #endregion

    public class AveRBSBackup
    {
        private IAveBackupRestoreQueryService mQueryService = null;
        private int mCollectionId = 0;
        private short mBlobStoreId = 0;
        private Guid mSiteId;
        public AveRBSBackup(Guid siteId, IAveBackupRestoreQueryService queryService)
        {
            mSiteId = siteId;
            mQueryService = queryService;
            int[] tem = AveRBSCommon.GetCollectionIdAndProviderId(queryService, siteId);
            mCollectionId = tem[0];
            mBlobStoreId = (short)tem[1];
        }       

        /// <summary>
        /// 备份RBS Stub的信息，主要是store_blob_id和store_pool_id的数据
        /// </summary>
        /// <param name="rbs_id">从alldocstreams中获取到的某个RBS文件对应的Rbs_id</param>
        /// <returns>RBSStubInfo对象，保存了当前rbs_id对应的RBS Stub信息</returns>
        public AveRBSStubInfo BackupRBSStub(byte[] rbs_id)
        {
            //int collectionId = rbs_id[8] | rbs_id[9] << 8 | rbs_id[10] << 16 | rbs_id[11] << 32;
            return mQueryService.BackupRBSStub(rbs_id, mBlobStoreId, 0);
        }
    }

    public class AveRBSRestore
    {
        private IAveBackupRestoreQueryService mQueryService = null;
        private int mCollectionId = 0;
        private short mBlobStoreId = 0;
        private List<Guid> mPoolsOfDB = null;
        private Guid mSiteId;

        public AveRBSRestore(Guid siteId, IAveBackupRestoreQueryService queryService)
        {
            mSiteId = siteId;
            mQueryService = queryService;
            int[] tem = AveRBSCommon.GetCollectionIdAndProviderId(queryService, siteId);
            mCollectionId = tem[0];
            mBlobStoreId = (short)tem[1];
            mPoolsOfDB = AveRBSCommon.GetPoolsOfDB(queryService);
        }
              
        /// <summary>
        /// 利用RBSStubInfo对象，将RBSStub信息还原到RBS对应的数据库表中
        /// </summary>
        /// <param name="stubinfo">备份的RBSStubInfo对象</param>
        /// <returns>Rbs_Id,需要将该值还原到alldocstreams中对应的记录中Rbs_Id字段中去</returns>
        public byte[] RestoreRBSStub(AveRBSStubInfo stubinfo)
        {
            //int collectionId = stubinfo.RBSId[8] | stubinfo.RBSId[9] << 8 | stubinfo.RBSId[10] << 16 | stubinfo.RBSId[11] << 32;
            return mQueryService.RestoreRBSStub(stubinfo, mPoolsOfDB, mBlobStoreId, 0);
        }
    }

    public class AveRBSCommon
    {
        private static AveLogger logger = AveLogger.GetInstance(typeof(AveRBSCommon));
        public const string COLLECTION_OWNING_APPLICATION = "Microsoft.SharePoint";
        public const string RBS_PROVIDER_NAME = "SP2010RBSProvider";
        public const string CMD_FETCH_RBS_BLOBID_AND_POOLID = @"[mssqlrbs].[rbs_sp_get_blob_details]";

        /// <summary>
        /// 获取某个DB的ProviderId和CollectionId
        /// </summary>
        /// <param name="sqlconn">连接字符串</param>
        /// <returns>int[2]数组，int[0]=CollectionId，int[1]=ProviderId</returns>
        public static int[] GetCollectionIdAndProviderId(IAveBackupRestoreQueryService queryService, Guid siteId)
        {
            return queryService.GetCollectionIdAndProviderId(siteId);
        }

        public static List<Guid> GetPoolsOfDB(IAveBackupRestoreQueryService queryService)
        {
            List<Guid> temList = new List<Guid>();
            try
            {
                temList = queryService.GetPoolsOfDB();
            }
            catch (Exception ex)
            {
                logger.Log(AveLogLevel.WARN, WrapperCommonResource.AWCDBPoolGetFailed, ex);
            }
            return temList;
        }

        public static XmlElement RBSArchiverPoolIdToXML(byte[] storeBlobPoolIDBinary)
        {
            /*
ave+GUID
*/
            XmlDocument doc = new XmlDocument();
            XmlElement poolInfo = doc.CreateElement("StorePoolInfo");

            int position = 5;
            int version = 0;
            int moduleType = 0;
            Guid poolGuid = Guid.Empty;
            Guid siteID = Guid.Empty;

            moduleType = (int)storeBlobPoolIDBinary[3];
            version = (int)storeBlobPoolIDBinary[4];
            byte[] poolIdBinary = new byte[16];
            GetBinaryFromBinary(ref storeBlobPoolIDBinary, ref poolIdBinary, ref position);
            poolGuid = new Guid(poolIdBinary);
            byte[] siteIdBinary = new byte[16];
            GetBinaryFromBinary(ref storeBlobPoolIDBinary, ref siteIdBinary, ref position);
            siteID = new Guid(siteIdBinary);

            poolInfo.SetAttribute("poolGUID", ToControlGUIDFormat(poolGuid.ToString()));
            poolInfo.SetAttribute("version", version.ToString());
            poolInfo.SetAttribute("moduleType", moduleType.ToString());
            poolInfo.SetAttribute("siteID", siteID.ToString());

            return poolInfo;
        }

        public static XmlElement RBSExtenderPoolIdToXML(byte[] storeBlobPoolIDBinary)
        {
            /*
 guid  \0  <RealTimeRestore storeType="2">
<StorePoolInfo  poolGUID="" version=""/>
<StoreBlobInfo  convertFromEBS="" fileGUID="" version="" logicalDriveType="" logicalDriveID=""/>
</RealTimeRestore>
 */
            XmlDocument doc = new XmlDocument();
            XmlElement poolInfo = doc.CreateElement("StorePoolInfo");

            int position = 5;
            int version = 0;
            int moduleType = 0;
            Guid poolGuid = Guid.Empty;

            moduleType = (int)storeBlobPoolIDBinary[3];
            version = (int)storeBlobPoolIDBinary[4];
            byte[] poolIdBinary = new byte[16];
            GetBinaryFromBinary(ref storeBlobPoolIDBinary, ref poolIdBinary, ref position);
            poolGuid = new Guid(poolIdBinary);

            poolInfo.SetAttribute("poolGUID", ToControlGUIDFormat(poolGuid.ToString()));
            poolInfo.SetAttribute("version", version.ToString());
            poolInfo.SetAttribute("moduleType", moduleType.ToString());

            return poolInfo;
        }

        public static void GetBinaryFromBinary(ref byte[] container, ref byte[] fetcher, ref int position)
        {
            Array.Copy(container, position, fetcher, 0, fetcher.Length);
            position += fetcher.Length;
        }

        public static string ToControlGUIDFormat(string guid)
        {
            guid = guid.ToUpper();
            guid = guid.Replace("-", "");
            return guid;
        }

        public static Guid GetPoolGuid(byte[] storeBlobPoolIDBinary)
        {
            StringBuilder blobPoolHeader = new StringBuilder();
            for (int i = 0; i < 3; i++)
            {
                blobPoolHeader.Append((char)storeBlobPoolIDBinary[i]);
            }
            if (storeBlobPoolIDBinary.Length != 20 || !blobPoolHeader.ToString().Equals("doc", StringComparison.OrdinalIgnoreCase))
            {
                throw new Exception("The blob pool id binary is not generated by DocAve 6 Extender");
            }
            byte[] poolIdBinary = new byte[16];
            int position = 4;
            GetBinaryFromBinary(ref storeBlobPoolIDBinary, ref poolIdBinary, ref position);
            return new Guid(poolIdBinary);
        }
    }

    public class RBSPoolId
    {
        private Guid mSiteId = Guid.Empty;
        public byte[] ExtendData = new byte[0];

        #region  Property
        public Guid SiteId
        {
            get
            {
                return mSiteId;
            }
        }

        public int Extend
        {
            get
            {
                return ExtendData.Length;
            }
        }
        #endregion

        #region Constructure
        public RBSPoolId(byte[] mPoolId)
        {
            int position = 4;
            byte[] poolId = mPoolId;
            byte[] siteIdByte = new byte[16];
            GetBinaryFromBinary(ref poolId, ref siteIdByte, ref position);
            mSiteId = new Guid(siteIdByte);
            byte extend = poolId[position];
            position++;
            if ((int)extend > 0)
            {
                ExtendData = new byte[(int)extend];
                GetBinaryFromBinary(ref poolId, ref ExtendData, ref position);
            }
        }

        public RBSPoolId()
        {

        }

        public RBSPoolId(Guid siteid)
        {
            this.mSiteId = siteid;
        }

        public RBSPoolId(Guid siteid, byte[] extend)
            : this(siteid)
        {
            ExtendData = extend;
        }
        #endregion

        #region Methods
        private void GetBinaryFromBinary(ref byte[] container, ref byte[] fetcher, ref int position)
        {
            Array.Copy(container, position, fetcher, 0, fetcher.Length);
            position += fetcher.Length;
        }

        public byte[] ToArray()
        {
            return ToArray(false);
        }

        public byte[] ToArray(bool isFillFull)
        {
            byte[] poolId = new byte[0];
            byte[] header = System.Text.Encoding.UTF8.GetBytes("AVE");
            byte[] moduleType = new byte[] { (byte)AveRBSModuleType.Connector };
            byte[] siteId = SiteId.ToByteArray();// new byte[16];
            byte[] extend = new byte[1] { 0x0 };
            //extend settings like this
            //byte[] extendSettings = new byte[x];
            //extend[0]=(byte)extendSettings.Length
            LSAppendBytes(ref poolId, header, 0, header.Length);
            LSAppendBytes(ref poolId, moduleType, 0, moduleType.Length);
            LSAppendBytes(ref poolId, siteId, 0, siteId.Length);
            LSAppendBytes(ref poolId, new byte[] { (byte)Extend }, 0, 1);
            if (Extend > 0)
            {
                LSAppendBytes(ref poolId, ExtendData, 0, Extend);
            }
            if (isFillFull)
            {
                LSAppendBytes(ref poolId, new byte[255 - poolId.Length], 0, 255 - poolId.Length);
            }
            return poolId;
        }

        public void LSAppendBytes(ref byte[] source, byte[] additional, int startIndex, int length)
        {
            int oldLen = source.Length;
            Array.Resize<byte>(ref source, source.Length + length);
            Array.Copy(additional, startIndex, source, oldLen, length);
        }

        #endregion
    }

    public class RBSBlobId
    {
        private Guid mWebId = Guid.Empty;
        private Guid mListId = Guid.Empty;
        private Guid mItemId = Guid.Empty;
        private int mUIVersion = 1;
        // private DateTime mCreateTime = DateTime.MinValue;

        public byte[] ExtendData = new byte[0];

        #region Property

        public int UIVersion
        {
            get
            {
                return mUIVersion;
            }
        }

        public Guid ItemId
        {
            get
            {
                return mItemId;
            }
        }

        public Guid ListId
        {
            get
            {
                return mListId;
            }
        }

        public Guid WebId
        {
            get
            {
                return mWebId;
            }
        }

        public int Extend
        {
            get
            {
                return ExtendData.Length;
            }
        }
        #endregion

        #region Constructure
        public RBSBlobId(byte[] blobid)
        {
            ConvertRBSStubInfo(blobid);
        }

        public RBSBlobId()
        {
        }

        public RBSBlobId(Guid webid, Guid listid, Guid itemid, int filelength, DateTime createtime, int uiversion)
        {
            this.mWebId = webid;
            this.mListId = listid;
            this.mItemId = itemid;
            //this.mFileLength = filelength;
            //this.mCreateTime = createtime;
            this.mUIVersion = uiversion;
        }

        public RBSBlobId(Guid webid, Guid listid, Guid itemid, int filelength, DateTime createtime, int uiversion, byte[] extend)
            : this(webid, listid, itemid, filelength, createtime, uiversion)
        {
            this.ExtendData = extend;
        }
        #endregion

        #region Blob Operation  Methods
        private void ConvertRBSStubInfo(byte[] mBlobId)
        {
            int position = 4;
            byte[] blobId = mBlobId;
            byte[] fileId = new byte[16];
            byte[] listIdByte = new byte[16];
            byte[] webIdByte = new byte[16];
            byte[] uiVersion = new byte[4];
            //byte[] fileSize = new byte[8];
            //byte[] createTime = new byte[8];
            GetBinaryFromBinary(ref blobId, ref fileId, ref position);
            GetBinaryFromBinary(ref blobId, ref listIdByte, ref position);
            GetBinaryFromBinary(ref blobId, ref webIdByte, ref position);
            GetBinaryFromBinary(ref blobId, ref uiVersion, ref position);
            //GetBinaryFromBinary(ref blobId, ref fileSize, ref position);
            //GetBinaryFromBinary(ref blobId, ref createTime, ref position);
            mWebId = new Guid(webIdByte);
            mListId = new Guid(listIdByte);
            mItemId = new Guid(fileId);
            mUIVersion = AvePoint.Wrapper.Common.AveConvert.ToBigInt(uiVersion, 0);
            //mFileLength = (int)AvePoint.Common.AveConverter.ToBigInt(fileSize, 0);
            //mCreateTime = new DateTime(BytesToLong(createTime));
            byte extend = blobId[position];
            position++;
            if ((int)extend > 0)
            {
                ExtendData = new byte[(int)extend];
                GetBinaryFromBinary(ref blobId, ref ExtendData, ref position);
            }
        }

        private void GetBinaryFromBinary(ref byte[] container, ref byte[] fetcher, ref int position)
        {
            Array.Copy(container, position, fetcher, 0, fetcher.Length);
            position += fetcher.Length;
        }

        public byte[] ToArray()
        {
            return ToArray(false);
        }

        public byte[] ToArray(bool isFillFull)
        {
            //模块版本标识（AVE + 3 byte标识（1.Archive、Extender、Connector，2.各个模块的版本，3.Media需要的标识），一共6个bytes），FarmId(16)，FileGUID(16)，LogicalDriveId(32)，ArchiveTime(8)，SiteUrlPathMD5，SiteId(16)，Size(8bytes， 用于enum）。
            byte[] blobId = new byte[0];
            byte[] header = System.Text.Encoding.UTF8.GetBytes("AVE");
            byte[] moduleType = new byte[] { (byte)AveRBSModuleType.Connector };
            byte[] fileId = ItemId.ToByteArray();// new byte[16];
            byte[] listId = ListId.ToByteArray();// new byte[16];
            byte[] webId = WebId.ToByteArray();// new byte[16];

            byte[] uiVersion = new byte[4];
            AveConvert.ToBigBytes(UIVersion, uiVersion, 0);

            //byte[] blobsize = new byte[8];
            //AvePoint.Common.AveConverter.ToBigBytes((int)FileLength, blobsize, 0);

            //byte[] createTime = LongToBytes(mCreateTime.Ticks);// new byte[8];

            LSAppendBytes(ref blobId, header, 0, header.Length);
            LSAppendBytes(ref blobId, moduleType, 0, moduleType.Length);
            LSAppendBytes(ref blobId, fileId, 0, fileId.Length);
            LSAppendBytes(ref blobId, listId, 0, listId.Length);
            LSAppendBytes(ref blobId, webId, 0, webId.Length);
            LSAppendBytes(ref blobId, uiVersion, 0, uiVersion.Length);
            //BytesUtility.LSAppendBytes(ref blobId, blobsize, 0, blobsize.Length);
            //BytesUtility.LSAppendBytes(ref blobId, createTime, 0, createTime.Length);
            LSAppendBytes(ref blobId, new byte[] { (byte)Extend }, 0, 1);
            if (Extend > 0)
            {
                LSAppendBytes(ref blobId, ExtendData, 0, Extend);
            }
            if (isFillFull)
            {
                LSAppendBytes(ref blobId, new byte[255 - blobId.Length], 0, 255 - blobId.Length);
            }
            return blobId;
        }

        public void LSAppendBytes(ref byte[] source, byte[] additional, int startIndex, int length)
        {
            int oldLen = source.Length;
            Array.Resize<byte>(ref source, source.Length + length);
            Array.Copy(additional, startIndex, source, oldLen, length);
        }

        public static byte[] LongToBytes(long l)
        {
            byte[] buf = new byte[8];
            buf[7] = (byte)l;
            l >>= 8;
            buf[6] = (byte)l;
            l >>= 8;
            buf[5] = (byte)l;
            l >>= 8;
            buf[4] = (byte)l;
            l >>= 8;
            buf[3] = (byte)l;
            l >>= 8;
            buf[2] = (byte)l;
            l >>= 8;
            buf[1] = (byte)l;
            l >>= 8;
            buf[0] = (byte)l;
            l >>= 8;
            return buf;
        }

        public static long BytesToLong(byte[] buf)
        {
            long a = 0;
            for (int i = 0; i < 8; i++)
            {
                a <<= 8;
                a += buf[i];
            }
            return a;
        }
        #endregion

    }
}