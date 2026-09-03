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


namespace AvePoint.Item.Restore
{
    #region using
    using System;
    using AvePoint.GCommon.FileTransfer;
    using AvePoint.Wrapper.Common;
    using System.IO;
    using GCommon;
    #endregion

    class CacheWriter
    {
        private static AveLogger mLog = AveLogger.GetInstance(typeof(CacheWriter));

        private AveStreamSegments mStreamSegments;
        private IChunckedCacheWriter mCacheWriter;
        private IFileReceiver mRealReceiver;
        private const int BUFFER_SIZE = 1024;
        private long mContentLength;
        private long mMetadataLength;
        private RestoreContentDto mAveItemDto;

        /// <summary>
        /// 如果出现ContentLength不正确的情况
        /// 以数据块长度为准
        /// </summary>
        private long mDataBeginPosition = 0;

        public CacheWriter(IFileReceiver realReceiver, RestoreContentDto aveItemDto, AveStreamSegments streamSegments)
        {
            mRealReceiver = realReceiver;
            mStreamSegments = streamSegments;
            mAveItemDto = aveItemDto;
            mCacheWriter = new ChunckedCacheWriter(mStreamSegments.Stream);
        }

        public void WriteRestoreContent()
        {
            mStreamSegments.BeginSegment();
            byte[] flags = new byte[18];
            int position = 0;
            flags[position++] = Convert.ToByte(mAveItemDto.RestoreOption.mAveEventReceiverOption.DISABLE_EVENT_RECEIVER);
            flags[position++] = Convert.ToByte(mAveItemDto.RestoreOption.mAveItemRestoreOption.DELETE_ITEM);
            flags[position++] = Convert.ToByte(mAveItemDto.RestoreOption.mAveItemRestoreOption.SKIP_IF_SAME_MODIFIEDTIME);
            flags[position++] = Convert.ToByte(Convert.ToInt32(mAveItemDto.RestoreOption.mAveRestoreMode));
            flags[position] = Convert.ToByte(mAveItemDto.IsChecked);
            position += sizeof(int);
            flags[position++] = Convert.ToByte(mAveItemDto.IsMyProfileList);
            flags[position++] = Convert.ToByte(mAveItemDto.IsAppData);
            int charSize = sizeof(char);
            Array.Copy(BitConverter.GetBytes(mAveItemDto.Type), 0, flags, position, charSize);
            position += charSize;
            Array.Copy(BitConverter.GetBytes(mAveItemDto.ReplaceType), 0, flags, position, charSize);
            position += charSize;        
            Array.Copy(BitConverter.GetBytes(mAveItemDto.RestoreOption.mRequestOption), 0, flags, position, 4);
            mCacheWriter.WriteBytesWithoutLength(flags, 0, flags.Length);
            mCacheWriter.WriteString(mAveItemDto.Name);
            mCacheWriter.WriteString(mAveItemDto.ParentName);
            mCacheWriter.WriteString(mAveItemDto.SrcName);
            mCacheWriter.WriteString(mAveItemDto.OwnerLogin);
            mCacheWriter.WriteString(mAveItemDto.SrcUrl);
            mCacheWriter.WriteString(mAveItemDto.StubType);
            mCacheWriter.WriteString(mAveItemDto.OopSourceUrl);
            mCacheWriter.WriteString(mAveItemDto.Id);
            mCacheWriter.WriteString(mAveItemDto.StorageId);
            mCacheWriter.WriteString(mAveItemDto.BackUpJobId);
            mCacheWriter.WriteString(mAveItemDto.ItemPathMd5);
            mCacheWriter.WriteString(mAveItemDto.ArchiveTime.ToString());
        }
        
        private void ParseLength()
        {
            HeaderV1 header = GetHeader();
            mMetadataLength = header.MetadataLength;
            mContentLength = header.ContentLength;
            mDataBeginPosition = mStreamSegments.Stream.Position;
            var headerBuffer = header.ToBytes();
            mCacheWriter.WriteBytesWithoutLength(headerBuffer, 0, headerBuffer.Length);
        }

        public void WriteMetadata()
        {
            ParseLength();
            WriteDatablock(mMetadataLength);
        }

        private void WriteDatablock(long dataLength)
        {
            byte[] buffer = new byte[BUFFER_SIZE];
            int len = 0;
            long totalLen = 0;
            while (dataLength > 0)
            {
                int readLen = dataLength < BUFFER_SIZE ? (int)dataLength : (int)BUFFER_SIZE;
                len = mRealReceiver.ReadBytes(buffer, readLen);
                mCacheWriter.WriteBytesWithoutLength(buffer, 0, len);
                if (len == 0)
                {
                    throw new EndOfStreamException("Unexpected end of stream.");
                }
                totalLen += len;
                dataLength -= len;
            }
        }

        public void WriteContent()
        {
            byte[] buffer = new byte[BUFFER_SIZE];
            int len = 0;
            long totalLen = 0;
            while ((len = mRealReceiver.ReadBytes(buffer, buffer.Length)) > 0)
            {
                mCacheWriter.WriteBytesWithoutLength(buffer, 0, len);
                totalLen += len;
            }
            if (mContentLength != totalLen)
            {
                mLog.Warn("Invalid content length: {0}, Real data length: {1}", mContentLength, totalLen);
                mContentLength = totalLen;
                long position = mStreamSegments.Stream.Position;

                byte[] header = new HeaderV1(1, 0, mMetadataLength, mContentLength).ToBytes();
                mStreamSegments.Stream.Position = mDataBeginPosition;
                mCacheWriter.WriteBytesWithoutLength(header, 0, header.Length);
                mStreamSegments.Stream.Position = position;
            }
        }

        public void WriteFileTail()
        {            
            byte[] temp = new byte[1024];
            while (mRealReceiver.ReadBytes(temp, temp.Length) != 0);
            mStreamSegments.BeginSegmentTail();
            mCacheWriter.WriteString(mRealReceiver.GetFileTail());
            //mCacheWriter.WriteString(tailStr);
        }

        private HeaderV1 GetHeader()
        {
            byte[] header = new byte[AveWrapperConstants.HEADER_SIZE];
            int readLen = mRealReceiver.ReadBytes(header, 0, header.Length);
            if (readLen > 0)
            {
                byte majorVersion = header[8];
                switch (majorVersion)
                {
                    case 1:
                        byte[] newBuffer = new byte[HeaderV1.HEADER_LENGTH];
                        header.CopyTo(newBuffer, 0);
                        mRealReceiver.ReadBytes(newBuffer, header.Length, HeaderV1.HEADER_LENGTH - header.Length);
                        return new HeaderV1(newBuffer);
                    case 0:
                    default:
                        return new HeaderV0(header).ToV1Header();
                }
            }
            throw new InvalidDataException();
        }
    }
}
