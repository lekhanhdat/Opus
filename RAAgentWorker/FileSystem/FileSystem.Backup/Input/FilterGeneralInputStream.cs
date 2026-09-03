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
using AvePoint.GCommon.Contract.Server.ControlPanel.Cryptography;
using AvePoint.Media.Service.DomainModel;

namespace AvePoint.Media.Core.IO.Input
{
    public abstract class FilterGeneralInputStream : IMediaGeneralInputStream
    {
        protected IMediaGeneralInputStream InnerInputStream { get; private set; }

        public FilterGeneralInputStream(IMediaGeneralInputStream innerOutput)
        {
            this.InnerInputStream = innerOutput;
        }

        public abstract void Open();
        public abstract void SetEncryptionInfos(Dictionary<string, DataEncryptionInfo> encryptionInfos, Func<string, DataEncryptionInfo> dataEncryptInfoGetter = null);
        public abstract string NextItem(IndexBase itemIndex);
        public abstract int ReadMetaDataPart1(byte[] data, int offset, int count);
        public abstract int ReadContent(byte[] data, int offset, int count);
        public abstract int ReadMetaDataPart2(byte[] data, int offset, int count);
        public abstract void EndItem();
        public abstract void Close();

        public IndexBase CurrentItemIndex { get { return this.InnerInputStream.CurrentItemIndex; } }
        public bool HasMetaDataPart1 { get { return this.InnerInputStream.HasMetaDataPart1; } }
        public bool HasContent { get { return this.InnerInputStream.HasContent; } }
        public bool HasMetaDataPart2 { get { return this.InnerInputStream.HasMetaDataPart2; } }

        public void BeginRead(FileType fileType)
        {
            this.InnerInputStream.BeginRead(fileType);
        }

        public void EndRead(FileType fileType)
        {
            this.InnerInputStream.EndRead(fileType);
        }
    }

}
