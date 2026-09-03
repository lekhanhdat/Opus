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
using System.Xml;
using Microsoft.SharePoint;
using AvePoint.Wrapper.Common;

namespace AvePoint.ObjectModel.Server13
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

        private int mCollectionId = 0;
        private short mBlobStoreId = 0;
        private Guid mSiteId;
        private AveRBSBackup(Guid siteId)
        {
            mSiteId = siteId;

            int[] tem = AveRBSCommon.GetCollectionIdAndProviderId();
            mCollectionId = tem[0];
            mBlobStoreId = (short)tem[1];
        }

        private static AveRBSBackup mBackup;
        public static AveRBSStubInfo BackupRBSStub(Guid siteId, byte[] rbs_id)
        {
            if (mBackup == null || !mBackup.mSiteId.Equals(siteId))
            {
                mBackup = new AveRBSBackup(siteId);
            }
            return mBackup.BackupRBSStub(rbs_id);
        }

        /// <summary>
        /// 备份RBS Stub的信息，主要是store_blob_id和store_pool_id的数据
        /// </summary>
        /// <param name="rbs_id">从alldocstreams中获取到的某个RBS文件对应的Rbs_id</param>
        /// <returns>RBSStubInfo对象，保存了当前rbs_id对应的RBS Stub信息</returns>
        private AveRBSStubInfo BackupRBSStub(byte[] rbs_id)
        {
            long blob_num = GenerateBlobNumber(rbs_id);
            if (0 == blob_num)
                throw new Exception("Get blob number error, check the sqlconnection of RBSSharedOpsBackup Object is available");
            return AveDBQueryService.AveRBSBackup_BackupRBSStub(mCollectionId, blob_num, mBlobStoreId);

        }

        private long GenerateBlobNumber(byte[] rbs_id)
        {
            return AveDBQueryService.AveRBSBackup_GenerateBlobNumber(rbs_id);
        }
    }

    public class AveRBSExtenderRestore
    {
        private int mCollectionId = 0;
        private short mBlobStoreId = 0;
        private List<Guid> mPoolsOfDB = null;
        private Guid mSiteId;

        private AveRBSExtenderRestore(Guid siteId)
        {
            mSiteId = siteId;
            int[] tem = AveRBSCommon.GetCollectionIdAndProviderId();
            mCollectionId = tem[0];
            mBlobStoreId = (short)tem[1];
            mPoolsOfDB = AveRBSCommon.GetPoolsOfDB();
        }

        private static AveRBSExtenderRestore mRestore;

        public static byte[] RestoreRBSStub(Guid siteId, AveRBSStubInfo stubinfo)
        {
            if (mRestore == null || !mRestore.mSiteId.Equals(siteId))
            {
                mRestore = new AveRBSExtenderRestore(siteId);
            }
            return mRestore.RestoreRBSStub(stubinfo);
        }

        /// <summary>
        /// 利用RBSStubInfo对象，将RBSStub信息还原到RBS对应的数据库表中
        /// </summary>
        /// <param name="stubinfo">备份的RBSStubInfo对象</param>
        /// <returns>Rbs_Id,需要将该值还原到alldocstreams中对应的记录中Rbs_Id字段中去</returns>
        private byte[] RestoreRBSStub(AveRBSStubInfo stubinfo)
        {
            byte[] rbs_id = null;

            //如果要还原的STUB的PoolId在当前的DB中不存在，则添加一个PoolId到这个DB
            if (!mPoolsOfDB.Contains(AveRBSCommon.GetPoolGuid(stubinfo.StorePoolId)))
                CreatePool(stubinfo.StorePoolId, false);
            //还原RBS的STUB，如果成功，则返回一个大于0的整数；
            long blobNumber = WriteBlobInformationToDB(stubinfo);
            if (blobNumber == -1)
                throw new Exception("generate blob record error");

            rbs_id = GenerateRbsId(blobNumber);
            return rbs_id;
        }

        private long WriteBlobInformationToDB(AveRBSStubInfo stubinfo)
        {
            return AveDBQueryService.AveRBSBackup_WriteBlobInformationToDB(stubinfo, mCollectionId, mBlobStoreId);
        }



        /// <summary>
        /// 通过BlobNumber生成RbsId的函数
        /// </summary>
        /// <param name="blob_num">需要转换的BlobNumber</param>
        /// <returns>生成的RbsId</returns>
        private byte[] GenerateRbsId(long blob_num)
        {
            return AveDBQueryService.AveRBSExtenderRestore_GenerateRbsId(mCollectionId, blob_num);
        }

        private void CreatePool(byte[] poolId, bool canStoreNewBlobs)
        {
            AveDBQueryService.AveRBSExtenderRestore_CreatePool(poolId, canStoreNewBlobs, mCollectionId, mBlobStoreId);
        }

        private long GetBlobNumber(AveRBSStubInfo stubInfo)
        {
            return AveDBQueryService.AveRBSExtenderRestore_GetBlobNumber(stubInfo, mBlobStoreId);
        }
    }

    public class AveRBSCommon
    {
        public const string COLLECTION_OWNING_APPLICATION = "Microsoft.SharePoint";
        public const string RBS_PROVIDER_NAME = "DocAve.SP2010.Storage.RBSProvider";
        public const string CMD_FETCH_RBS_BLOBID_AND_POOLID = @"[mssqlrbs].[rbs_sp_get_blob_details]";

        /// <summary>
        /// 获取某个DB的ProviderId和CollectionId
        /// </summary>
        /// <param name="sqlconn">连接字符串</param>
        /// <returns>int[2]数组，int[0]=CollectionId，int[1]=ProviderId</returns>
        public static int[] GetCollectionIdAndProviderId()
        {
            return AveDBQueryService.AveRBSCommon_GetCollectionIdAndProviderId();
        }

        public static List<Guid> GetPoolsOfDB()
        {
            return AveDBQueryService.AveRBSCommon_GetPoolsOfDB();
        }

        public static XmlElement RBSArchiverPoolIdToXML(byte[] storeBlobPoolIDBinary)
        {
            /*
guid  \0  <RealTimeRestore storeType="2">
<StorePoolInfo  poolGUID="" version="" storeType=""/>
<StoreBlobInfo  convertFromEBS="" fileGUID="" version="" logicalDriveType="" logicalDriveID="" storeType=""/>
</RealTimeRestore>
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
            byte moduleType = storeBlobPoolIDBinary[3];
            XmlElement poolIdXml = null;
            if (moduleType == 2)
                poolIdXml = RBSExtenderPoolIdToXML(storeBlobPoolIDBinary);
            else if (moduleType == 1)
                poolIdXml = RBSArchiverPoolIdToXML(storeBlobPoolIDBinary);
            else//Connector Type=4
                throw new Exception("Maybe this is a connector stub");
            return new Guid(poolIdXml.GetAttribute("poolGUID"));
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
        private long mFileLength = 0L;

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
            mUIVersion = AveConverter.ToBigInt(uiVersion, 0);
            //mFileLength = (int)Avepoint.Common.AveConverter.ToBigInt(fileSize, 0);
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
            AveConverter.ToBigBytes(UIVersion, uiVersion, 0);
      
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

    public class AveRBSConnectorRestore
    {
        #region properties
        private Guid mSiteId = Guid.Empty;
        private Guid mWebId = Guid.Empty;
        private Guid mListId = Guid.Empty;
        private int mFileLength = 0;
        private SPListItem mItem = null;
        private SPFile mFile = null;       
        private int mCollectionId = -1;
        public short mBlobStoreId = -1;
        private byte[] mPoolId = new byte[0];
        private byte[] mBlobId;

        #endregion

        public AveRBSConnectorRestore(SPFile file, int size)
        {
            mFile = file;
            mSiteId = file.Web.Site.ID;
            mWebId = file.Web.ID;
            mListId = file.Item.ParentList.ID;
            mItem = file.Item;
            mFileLength = size;
            int[] tem = AveRBSCommon.GetCollectionIdAndProviderId();
            mCollectionId = tem[0];
            mBlobStoreId = (short)tem[1];
            GetOrCreateValidArchivePool(false);
        }

        private void GetOrCreateValidArchivePool(bool canStoreNewBlobs)
        {
            byte[] poolId = new byte[0];
            poolId = GetPoolId(mBlobStoreId, mCollectionId, canStoreNewBlobs, mSiteId);
            if (poolId.Length == 0)
            {
                poolId = CreateArchivePool(false);
            }
            if (poolId.Length == 0)
            {
                throw new Exception(string.Format("Cannot get archive pool in collection {0} for provider {1}.", 0, 0));
            }
            mPoolId = poolId;
        }

        private byte[] CreateArchivePool(bool canStoreNewBlobs)
        {
            byte[] poolId = GeneratePoolId();
            try
            {
                int poolIndex = AddPool(mBlobStoreId, poolId, mCollectionId, 0);
                ClosePool(mBlobStoreId, poolId, poolIndex, canStoreNewBlobs);
            }
            catch (Exception e)
            {
                poolId = new byte[0];
                throw new Exception(string.Format("Cannot create archive pool in collection {0} for provider {1}. Exception: {2}", mCollectionId, mBlobStoreId, e.Message));
            }
            return poolId;
        }

        private int ClosePool(int blobStoreId, byte[] storePoolId, int poolId, bool canStoreNewBlobs)
        {
            return AveDBQueryService.AveRBSConnectorRestore_ClosePool(blobStoreId, storePoolId, poolId, canStoreNewBlobs);
        }

        private byte[] GetPoolId(int blobStoreId, int collectionId, bool canStoreNewBlobs, Guid siteID)
        {
            return AveDBQueryService.AveRBSConnectorRestore_GetPoolId(blobStoreId, collectionId, canStoreNewBlobs, siteID);
        }

        public static bool CheckSiteIDIsPoolID(byte[] temp, Guid siteID)
        {
            bool flag = false;
            try
            {
                RBSPoolId pool = new RBSPoolId(temp);
                if (pool.SiteId.ToString().Equals(siteID.ToString(), StringComparison.OrdinalIgnoreCase))
                {
                    flag = true;
                }
            }
            catch
            {
            }
            return flag;
        }

        private byte[] GeneratePoolId()
        {
            byte[] poolId = new byte[0];
            RBSPoolId pool = new RBSPoolId(mSiteId);
            poolId = pool.ToArray(true);
            return poolId;
        }

        private byte[] GenerateBlobId()
        {
            byte[] blobId = new byte[0];
            RBSBlobId blob = new RBSBlobId(mWebId, mListId, mItem.UniqueId, mFileLength, DateTime.Now, mFile.UIVersion);
            blobId = blob.ToArray(true);
            mBlobId = blobId;
            return blobId;
        }

        private int AddPool(int blobSotreId, byte[] storePoolId, int collectionId, int clientVersion)
        {
            return AveDBQueryService.AveRBSConnectorRestore_AddPool(blobSotreId, storePoolId, collectionId, clientVersion);
        }

        private bool CheckBlobExist(byte[] storePoolId, byte[] storeBlobId, int blobStoreId, int collectionId, ref long blobNumber)
        {
            return AveDBQueryService.AveRBSConnectorRestore_CheckBlobExist(storePoolId, storeBlobId, blobStoreId, collectionId, ref blobNumber);
        }

        private long RegisterBlob(int collectionId, int blobStoreId, byte[] storePoolId, byte[] storeBlobId, DateTime createTime, long blobSize)
        {
            return AveDBQueryService.AveRBSConnectorRestore_RegisterBlob(collectionId, blobStoreId, storePoolId, storeBlobId, createTime, blobSize);
        }      

        private byte[] GetRbsId(int collectionId, long blobNumber)
        {
            return AveDBQueryService.AveRBSConnectorRestore_GetRbsId(collectionId, blobNumber);
        }

        public byte[] RestoreRBSStub()
        {
            try
            {
                long blobNumber = -1;

                #region WriteBlobInformationToDB
                if (mFileLength == -1)
                {
                    throw new Exception("Cannot generate blob record.Because the file length is less than zero.");
                }
                byte[] blobId = GenerateBlobId();
                if (!CheckBlobExist(mPoolId, blobId, mBlobStoreId, mCollectionId, ref blobNumber))
                {
                    blobNumber = RegisterBlob(mCollectionId, mBlobStoreId, mPoolId, blobId, DateTime.Now, (long)mFileLength);
                }
                #endregion

                if (blobNumber == -1)
                {
                    throw new Exception("Cannot generate blob record");
                }

                #region
                byte[] RbsId = null;
                RbsId = GetRbsId(mCollectionId, blobNumber);

                return RbsId;

                #endregion
                throw new Exception("Register blob failed.Write token to doc streams failed.");
            }
            catch (Exception e)
            {
                throw new NotImplementedException(); ;
            }
        }

    }
}