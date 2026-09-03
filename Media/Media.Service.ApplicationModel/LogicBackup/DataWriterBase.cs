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
    using AvePoint.GCommon.Network;
    using Merged18NResources.MediaServiceApplicationModel;
    using AvePoint.Media.Service.DomainModel;
    #endregion

    /// <summary>
    /// Provide the main logic of the data writer
    /// </summary>
    /// <typeparam name="TBackupJob"></typeparam>
    public abstract class DataWriterBase<TBackupJob>
        : ApplicationModelServiceBase
        , IDataWriter<TBackupJob> where TBackupJob : BackupJobBase
    {
        readonly static Object syncRootOpened = new Object();
        readonly static Object syncRootWritten = new Object();
        readonly static Object syncRootClosed = new Object();

        EventHandler<DataWriterOpenedEventArgs> opened;
        EventHandler<DataWriterWrittenEventArgs> written;
        EventHandler<DataWriterClosedEventArgs> closed;

        #region IDataWriter Methods

        #region Event Members
        public event EventHandler<DataWriterOpenedEventArgs> Opened
        {
            add
            {
                lock (syncRootOpened)
                    opened += value;
            }
            remove
            {
                lock (syncRootOpened)
                    opened -= value;
            }
        }

        public event EventHandler<DataWriterWrittenEventArgs> Written
        {
            add
            {
                lock (syncRootWritten)
                    written += value;
            }
            remove
            {
                lock (syncRootWritten)
                    written -= value;
            }
        }

        public event EventHandler<DataWriterClosedEventArgs> Closed
        {
            add
            {
                lock (syncRootClosed)
                    closed += value;
            }
            remove
            {
                lock (syncRootClosed)
                    closed -= value;
            }
        }

        protected virtual void OnClosed(DataWriterClosedEventArgs closedEventArgs)
        {
            var temp = closed;
            if (temp != null)
            {
                temp(null, closedEventArgs);
            }
        }
        #endregion

        public abstract void Open(TBackupJob backupJob);

        public void Write(AveDataBlock dataBlock)
        {
            switch (dataBlock.Type)
            {
                case AveDataBlockType.HEADER_TYPE:
                    HandleHeader(dataBlock);
                    break;
                case AveDataBlockType.DATA_TYPE:
                    HandleData(dataBlock);
                    break;
                case AveDataBlockType.CONTENTDATA_TYPE:
                    HandleContentData(dataBlock);
                    break;
                case AveDataBlockType.TAIL_TYPE:
                    HandleTail(dataBlock);
                    break;
                default:
                    throw new System.NotSupportedException(String.Format(MediaServiceApplicationModelResource.DataWriterBaseWriteNotSupportedException, dataBlock.Type));
            }
        }

        public virtual void Close(BackupCloseInfo info)
        {
            this.Dispose();
        }

        #endregion

        #region Handle Methods

        public abstract void HandleHeader(AveDataBlock dataBlock);
        public abstract void HandleData(AveDataBlock dataBlock);
        public abstract void HandleContentData(AveDataBlock dataBlock);
        public abstract void HandleTail(AveDataBlock dataBlock);

        #endregion

        #region IDisposable

        public abstract void Dispose();

        #endregion
    }
}