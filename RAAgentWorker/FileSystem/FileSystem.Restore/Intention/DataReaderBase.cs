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




namespace AvePoint.Media.Service
{
    #region using directives
    using System;
    using AvePoint.GCommon.Contract.Server.ControlPanel.Cryptography;
    using AvePoint.Media.Common;
    using AvePoint.Media.Core.IO.Input;
    using AvePoint.Media.Service.DomainModel;
    using AvePoint.GCommon.Network;
    #endregion

    /// <summary>
    /// Provide the main logic of the data reader
    /// </summary>
    /// <typeparam name="T">job type</typeparam>
    public abstract class DataReaderBase<T>
        : IDataReader<T>
        where T : RestoreJobBase
    {
        Byte[] buffer = new Byte[1048576];
        readonly static Object syncRootOpened = new Object();
        readonly static Object syncRootClosed = new Object();


        protected Byte[] Buffer
        {
            get { return this.buffer; }
            set { this.buffer = value; }
        }

        public abstract IMediaGeneralInputStream Input { get; }
        public abstract void Open(T restoreJob);
        public virtual void Close()
        {
            this.Dispose();
        }

        public virtual string GetNextItem(IndexBase basicIndex)
        {
            return Input.NextItem(basicIndex);
        }

        public virtual void SendData(IMediaFileSender fileSender)
        {
            if (Input.HasMetaDataPart1)
            {
                Input.BeginRead(FileType.MetaData);
                while (true)
                {
                    int readLen = Input.ReadMetaDataPart1(Buffer, 0, Buffer.Length);
                    if (readLen <= 0) break;

                    fileSender.WriteData(AveDataBlockType.DATA_TYPE, Buffer, 0, readLen);
                }
                Input.EndRead(FileType.MetaData);
            }
            if (Input.HasContent)
            {
                Input.BeginRead(FileType.Content);
                while (true)
                {
                    int readLen = Input.ReadContent(Buffer, 0, Buffer.Length);
                    if (readLen <= 0) break;
                    fileSender.WriteData(AveDataBlockType.CONTENTDATA_TYPE, Buffer, 0, readLen);
                }
                Input.EndRead(FileType.Content);
            }
            if (Input.HasMetaDataPart2)
            {
                Input.BeginRead(FileType.MetaData);
                while (true)
                {
                    int readLen = Input.ReadMetaDataPart2(Buffer, 0, Buffer.Length);
                    if (readLen <= 0) break;
                    fileSender.WriteData(AveDataBlockType.DATA_TYPE, Buffer, 0, readLen);
                }
                Input.EndRead(FileType.MetaData);
            }
            Input.EndItem();
        }

        #region IDisposable
        public abstract void Dispose();
        #endregion

        public void SetEncryptionInfos(System.Collections.Generic.Dictionary<string, DataEncryptionInfo> encryptionInfos)
        {
            Input.SetEncryptionInfos(encryptionInfos);
        }
    }
}