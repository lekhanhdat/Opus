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


namespace Office365GroupRestore
{
    #region 
    using System;
    using AvePoint.GCommon.Contract.Media.Object;
    using System.Collections.Generic;
    using System.Linq;
    using AvePoint.Metadata;

    #endregion

    public class ExchangeDataBlock : IDisposable
    {
        public ExchangeFileHeader FileHeader { get; set; }

        public ExchangeRestoreData RestoreData { get; set; }

        public RestoreFileTail FileTail { get; set; }

        public Boolean IsTimeOut { get; set; }

        public Boolean IsException { get; set; }

        public String ExceptionMessage { get; set; }

        // public List<ExchangeDataBlock> Items { get; set; }



        public void Dispose()
        {
            if (this.RestoreData != null)
            {
                this.RestoreData.Dispose();
                this.RestoreData = null;
            }
        }
    }

    public class ExchangeRestoreData : IDisposable
    {
        public AveMetadata Metadata { get { return this.MetadataLists.First(); } }
        //public Byte[] ContentData { get; set; }

        public IEnumerable<AveMetadata> MetadataLists { get; set; }

        public IRestoreStream RestoreStream { get; set; }

        public System.IO.Stream ContentStream { get; set; }

        public void Dispose()
        {
            if (this.RestoreStream != null)
            {
                this.RestoreStream.Dispose();
                this.RestoreStream = null;
            }
        }
    }

}