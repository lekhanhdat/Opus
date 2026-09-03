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
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using AvePoint.GCommon.FileTransfer;
    using AvePoint.Wrapper.Common;
    using System.IO;
    using AvePoint.Wrapper.Restore;
    using AvePoint.GCommon.Network;
    #endregion

    class CacheFileReceiver : IFileReceiver, IInputStreamWrapper
    {
        private AveStreamSegments mStreamSegments;
        private IChunckedCacheReader mStreamReader;

        public long Length => mStreamSegments.Stream.Length;

        public long Position
        {
            get
            {
                return mStreamSegments.Stream.Position;
            }
            set
            {
                mStreamSegments.Stream.Position = value;
            }
        }

        public CacheFileReceiver(AveStreamSegments streamSegments)
        {
            mStreamSegments = streamSegments;
            mStreamReader = new ChunckedCacheReader(mStreamSegments.Stream);
        }

        public RestoreContentDto GetNextItemDto()
        {            
            if (mStreamSegments.NextSegment())
            {
                byte[] flags = mStreamReader.ReadBytes(18);
                int position = 0;
                int charSize = sizeof(char);
                RestoreContentDto aveItemDto = new RestoreContentDto();               
                aveItemDto.RestoreOption.mAveEventReceiverOption.DISABLE_EVENT_RECEIVER = BitConverter.ToBoolean(flags, position++);
                aveItemDto.RestoreOption.mAveItemRestoreOption.DELETE_ITEM = BitConverter.ToBoolean(flags, position++);
                aveItemDto.RestoreOption.mAveItemRestoreOption.SKIP_IF_SAME_MODIFIEDTIME = BitConverter.ToBoolean(flags, position++);
                aveItemDto.RestoreOption.mAveRestoreMode = (AveRestoreMode)BitConverter.ToInt32(flags, position++);
                aveItemDto.IsChecked = BitConverter.ToBoolean(flags, position);
                position += sizeof(int);
                aveItemDto.IsMyProfileList = BitConverter.ToBoolean(flags, position++);
                aveItemDto.IsAppData = BitConverter.ToBoolean(flags, position++);
                aveItemDto.Type = BitConverter.ToChar(flags, position);
                position += charSize;
                if (aveItemDto.Type != AveConstants.TYPE_LISTITEM
                    && aveItemDto.Type != AveConstants.TYPE_DOCUMENT
                    && aveItemDto.Type != AveConstants.TYPE_ATTACHMENTS)
                {
                    return null;    //here the stream is in a wrong position
                }
                aveItemDto.ReplaceType = BitConverter.ToChar(flags, position);
                position += charSize;
                aveItemDto.RestoreOption.mRequestOption = BitConverter.ToChar(flags, position);
                aveItemDto.Name = mStreamReader.ReadMetaString();
                aveItemDto.ParentName = mStreamReader.ReadMetaString();
                aveItemDto.SrcName = mStreamReader.ReadMetaString();
                aveItemDto.OwnerLogin = mStreamReader.ReadMetaString();
                aveItemDto.SrcUrl = mStreamReader.ReadMetaString();
                aveItemDto.StubType = mStreamReader.ReadMetaString();
                aveItemDto.OopSourceUrl = mStreamReader.ReadMetaString();
                aveItemDto.Id = mStreamReader.ReadMetaString();
                aveItemDto.StorageId = mStreamReader.ReadMetaString();
                aveItemDto.BackUpJobId = mStreamReader.ReadMetaString();
                aveItemDto.ItemPathMd5 = mStreamReader.ReadMetaString();
                aveItemDto.ArchiveTime = long.Parse(mStreamReader.ReadMetaString());
                return aveItemDto;
            }
            return null;
        }

        public string Open(string host, int port, string info)
        {
            throw new NotImplementedException();
        }

        public string GetNextFileHead()
        {
            throw new NotImplementedException();
        }

        public string GetFileTail()
        {
            mStreamSegments.ToSegmentTail();
            return mStreamReader.ReadMetaString();
        }

        public int CRC32Match()
        {
            throw new NotImplementedException();
        }

        public int ReadBytes(byte[] buffer, int len)
        {
            return ReadBytes(buffer, 0, len);
        }

        public int ReadBytes(byte[] buffer, int offset, int len)
        {
            return mStreamSegments.Stream.Read(buffer, offset, len);
        }

        public string Close(string errorMsg)
        {
            throw new NotImplementedException();
        }

        public int ReadContent(byte[] buffer, int offset, int len)
        {
            return ReadBytes(buffer, offset, len);
        }

        public int ReadMetadata(byte[] buffer, int offset, int len)
        {
            return ReadBytes(buffer, offset, len);
        }
    }
}
