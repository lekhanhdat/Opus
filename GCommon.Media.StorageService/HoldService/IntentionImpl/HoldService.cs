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




namespace AvePoint.GCommon.Media.StorageService
{
    #region using directives

    using System;
    using System.IO;
    using System.Text;
    using AvePoint.GCommon.Contract.CodeReview;
    using Storage;

    #endregion using directives

    /// <summary>
    /// The interface is defines the hold logic and data format of the hold data,
    /// When you use the Hold Service, it must be opened.
    ///
    /// <example>
    ///   <code>
    ///    var holdInfo = new HoldInfo();
    ///    var holdService = new HoldServices();
    ///    holdService.Open(HoldInfo);
    ///    holdService.Hold(stream1,metaData1);
    ///    holdService.Hold(stream2,metaData2);
    ///    holdService.Hold(stream3,metaData3);
    ///    holdService.Close();
    ///   </code>
    /// </example>
    /// <remarks>
    /// A hold service is associated with a hold name and job id,
    /// that means, The hold name and job id can be identify a hold
    /// process, in this case , you must open hold service only once.
    /// </remarks>
    /// </summary>

    #region CodeReview

    [AveCodeReview(
    "2012/4/11",
    "dwxue@avepoint.com",
    "xiaofeiwang@avepoint.com",
    new string[] { },
    null,
    true)]

    #endregion CodeReview

    public class HoldService
        : IHoldService
    {
        HoldServiceInfo holdServiceInfo;
        IXSystem holdDevice;
        IPathGenerator pathGenerator = new PathGenerator();
        IMetaDataBuilderFactory metaDataBuilderFactory;

        public void Open(HoldServiceInfo holdServiceInfo)
        {
            this.holdServiceInfo = holdServiceInfo;
            this.metaDataBuilderFactory = new MetaDataBuilderFactory();
            this.holdDevice = XFactory.InstanceLibrary(holdServiceInfo.HoldDevice.ToXRIS());
            this.holdDevice.Open();
        }

        public HoldResult Hold(Stream dataStream, MetaData metaData)
        {
            return this.Hold(dataStream.Read, metaData);
        }

        public HoldResult Hold(IDataReader dataReader, MetaData metaData)
        {
            return this.Hold(dataReader.Read, metaData);
        }

        public HoldResult Hold(DataReadAction read, MetaData metaData)
        {
            Boolean IsReadOnceOnly = true;
            var buffer = new Byte[64 * 1024];
            var holdFileInfo = this.pathGenerator.Generate(new PathParameter(this.holdServiceInfo));
            var dataStorageInfo = XConvert.FromNames(holdFileInfo.FileContainer, holdFileInfo.DataFilePath);
            dataStorageInfo.Length = metaData.ContentSize == 0 ? Encoding.UTF8.GetBytes("stub").Length : metaData.ContentSize;
            using (var deviceDataStream = this.holdDevice.OpenStream(dataStorageInfo, FileMode.OpenOrCreate))
            {
                while (true)
                {
                    var readLen = read(buffer, 0, buffer.Length);
                    if (IsReadOnceOnly == true && readLen <= 0)
                    {
                        buffer = Encoding.UTF8.GetBytes("stub");
                        deviceDataStream.Write(buffer, 0, buffer.Length);
                        break;
                    }
                    else if (readLen <= 0) break;
                    deviceDataStream.Write(buffer, 0, readLen);
                    IsReadOnceOnly = false;
                }
                deviceDataStream.Commit(true);
                holdFileInfo.ContentDataStorageInfo = new DataStorageInfo(deviceDataStream.GetURI().SInfo);
            }
            var metaDataStorageInfo = XConvert.FromNames(holdFileInfo.FileContainer, holdFileInfo.MetaDataFilePath);
            var metaDataBuilder = this.metaDataBuilderFactory.CreateBuilder(metaData.Format);
            var metaDataInBytes = metaDataBuilder.Build(metaData);
            metaDataStorageInfo.Length = metaDataInBytes.Length;
            using (var deviceMetaDataStream = this.holdDevice.OpenStream(metaDataStorageInfo, FileMode.OpenOrCreate))
            {
                deviceMetaDataStream.Write(metaDataInBytes, 0, metaDataInBytes.Length);
                deviceMetaDataStream.Commit(true);
                holdFileInfo.MetaDataStorageInfo = new DataStorageInfo(deviceMetaDataStream.GetURI().SInfo);
            }
            return new HoldResult(holdFileInfo);
        }

        public ReleaseResult Release(HoldFileInfo fileInfo)
        {
            this.holdDevice.DeleteFile(DataStorageInfo.ConvetToStorageInfo(fileInfo.ContentDataStorageInfo));
            this.holdDevice.DeleteFile(DataStorageInfo.ConvetToStorageInfo(fileInfo.MetaDataStorageInfo));
            return new ReleaseResult { };
        }

        public void Close()
        {
            if (this.holdServiceInfo != null)
            {
                this.pathGenerator.Reset(new PathParameter(this.holdServiceInfo));
            }
            if (this.holdDevice != null)
            {
                this.holdDevice.Close();
            }
        }
    }
}