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
using System.Diagnostics;
using AvePoint.GCommon;
using AvePoint.Media.Service.ArchiverBackup.Backup;
using AvePoint.Media.Service.DomainModel;
using AvePoint.RA.CommonUtil;
using AvePoint.Wrapper.Common;

namespace AvePoint.RA.SharePoint.Archiver
{
    internal class ArchiverBackupStreamWriter : IArchiverBackupDataWriter
    {
        private static readonly RALogger Logger = RALogger.GetInstance(typeof(ArchiverBackupStreamWriter));
        private readonly AveCoordinatedStream stream;
        private readonly IChunckedCacheWriter writer;

        public ArchiverBackupStreamWriter()
        {
            stream = new AveCoordinatedStream();
            writer = new ChunckedCacheWriter(stream);
        }

        public void HandleHeader(string xml)
        {
            //ArchiverUtility.Logger(AveLogLevel.DEBUG, "write header", xml);
            writer.WriteHeader((byte)FileSenderCacheType.Head);
            writer.WriteString(xml);
        }

        public void HandleData(byte[] buf, int offset, int length)
        {
            writer.WriteHeader((byte)FileSenderCacheType.Data);
            writer.WriteBytes(buf, offset, length);
        }

        public void HandleContentData(byte[] buf, int offset, int length)
        {
            writer.WriteHeader((byte)FileSenderCacheType.Content);
            writer.WriteBytes(buf, offset, length);
        }

        public void HandleTail(string xml)
        {
            WriteTail(xml, true);
        }

        public long WriteTail(string xml, bool isOK)
        {
            //ArchiverUtility.Logger(AveLogLevel.DEBUG, "write tail", xml);
            writer.WriteHeader((byte)FileSenderCacheType.Tail);
            return writer.WriteString(xml);
        }


        public void Close(string message)
        {
            writer.WriteHeader((byte)FileSenderCacheType.Close);
            writer.WriteString(message);
        }

        public void Close()
        {
            Dispose();
        }


        public void Dispose()
        {
            //ArchiverUtility.Logger(AveLogLevel.DEBUG, "close stream {0}", new StackTrace());
            this.stream.Dispose();
        }

        public void Open(AvePoint.Media.Service.DomainModel.ArchiverBackupJob backupJob)
        {
        }

        public void Close(AvePoint.Media.Service.DomainModel.BackupCloseInfo info)
        {
            
        }

        public void WriteToAnotherWriter(IArchiverBackupDataWriter targetWriter)
        {
            try
            {
                stream.Position = 0;
                var cacheReader = new ChunckedCacheReader(stream);
                var header = FileSenderCacheType.End;
                do
                {
                    byte[] bytes = null;
                    int headerByte = cacheReader.ReadHeader();
                    header = headerByte == -1 ? FileSenderCacheType.End : (FileSenderCacheType)headerByte;
                    switch (header)
                    {
                        case FileSenderCacheType.Head:
                            targetWriter.HandleHeader(cacheReader.ReadString());
                            break;
                        case FileSenderCacheType.Data:
                            bytes = cacheReader.ReadBytes();
                            targetWriter.HandleData(bytes, 0, bytes.Length);
                            break;
                        case FileSenderCacheType.Content:
                            bytes = cacheReader.ReadBytes();
                            targetWriter.HandleContentData(bytes, 0, bytes.Length);
                            break;
                        case FileSenderCacheType.Close:
                            cacheReader.ReadString();
                            targetWriter.Close(new BackupCloseInfo());
                            break;
                        case FileSenderCacheType.Tail:
                            targetWriter.HandleTail(cacheReader.ReadString());
                            break;
                        default:
                            break;
                    }

                } while (header != FileSenderCacheType.End);
            }
            catch (Exception e)
            {
                Logger.Error("An error occurred when sending cache to media, message: {0}, stackTrace: {1}", e.Message, e.StackTrace);
                throw;
            }
        }

        public ArchiverBasicIndex GetArchiverIndex(string md5)
        {
            throw new NotImplementedException();
        }

        public void OpenEXO(ExchangeBackupJob backupJob)
        {
            throw new NotImplementedException();
        }
        public void OpenGDrive(GDriveBackupJob backupJob)
        {
            throw new NotImplementedException();
        }
    }

    internal enum FileSenderCacheType : byte
    {
        Head = 1,
        Data = 2,
        Content = 3,
        Tail = 4,
        Close = 5,
        End = 6
    }
}