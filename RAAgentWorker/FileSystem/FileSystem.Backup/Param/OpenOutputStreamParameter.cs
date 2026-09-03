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
    using AvePoint.GCommon.Contract.Server.ControlPanel.Cryptography;
    using AvePoint.GCommon.Utility;
    using AvePoint.Media.Common;
    using AvePoint.Media.Service.DomainModel;
    #endregion

    public class OpenOutputStreamParameter
    {
        public string PrefixNumber { get; set; }
        public int InitMetaDataFileNumber { get; set; }
        public int InitContentDataFileNumber { get; set; }
        public byte DataMode { get; set; }
        public int MaxBlockSize { get; set; }
        public int SPVersion { get; set; }
        public OutputStreamLevel OutputLevel { get; set; }
        public IOutputDataListener DataListener { get; set; }
        public int CompressionType { get; set; }
        public CompressionMethods CompressionMethod { get; set; }
        public DataEncryptionInfo EncryptionInfo { get; set; }
        public StreamOpenType OpenType { get; set; }

        public OpenOutputStreamParameter()
        {
            InitMetaDataFileNumber = 0;
            InitContentDataFileNumber = 0;
            DataMode = default(byte);
            SPVersion = 0;
            CompressionType = -1;
            PrefixNumber = null;
            DataListener = null;
            MaxBlockSize = 50 * IOConstants.MB;
            CompressionMethod = default(CompressionMethods);
            OpenType = StreamOpenType.Default;
            OutputLevel = OutputStreamLevel.DataBlockLevel;
        }
    }
}
