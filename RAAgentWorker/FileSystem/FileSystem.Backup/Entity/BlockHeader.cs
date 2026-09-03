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




namespace AvePoint.Media.Core.IO
{
    #region using directives
    using System;

    #endregion

    public class BlockHeader
    {
        public Int32 Version { get; set; }
        public BlockType Type { get; set; }
        public Int32 BlockNum { get; set; }
        public Int32 NextBlockNum { get; set; }
        public Int32 BlockSize { get; set; }
        public Int32 PageSize { get; set; }
        public Int32 NextHeaderOffset { get; set; }
        public Int32 SPVersion { get; set; }
        public Byte[] ContentMD5 { get; set; }

        /// <summary>
        /// The length of the encryption info, New for 6.0
        /// </summary>
        public Int32 EncryptionInfoSize { get; set; }

        /// <summary>
        /// Max length of the encryption info is 4096-62-4-512, New for 6.0
        /// </summary>
        public Byte[] EncryptionInfo { get; set; }
    }

    [Flags]
    public enum BlockType
    {
        MetaData = 1,
        ContentData = 1 << 1,
        Encrypted = 1 << 2,
        Compressed = 1 << 3,
    }
}
