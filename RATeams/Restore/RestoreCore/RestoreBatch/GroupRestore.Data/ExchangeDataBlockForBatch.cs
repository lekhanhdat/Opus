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
    using System;
    using System.Collections.Generic;
    using System.Linq;

    
    using AvePoint.GCommon.Contract.Media.Object;
    using AvePoint.GCommon.Utility;
    using AvePoint.Metadata;
    using AvePoint.RA.CommonUtil;
    //using AvePoint.Wrapper.Common;

    using ExchangeCommonWrapper;

    

    public class ExchangeDataBlockForBatch : IDisposable
    {
        public ExchangeFileHeader FileHeader { get; set; }

        public ExchangeRestoreDataForBatch RestoreData { get; set; }

        public RestoreFileTail FileTail { get; set; }

        public Boolean IsFinish { get; set; }

        public Boolean IsException { get; set; }

        public String ExceptionMessage { get; set; }

        public void Dispose()
        {
            if (this.RestoreData != null)
            {
                this.RestoreData.Dispose();
                this.RestoreData = null;
            }
        }
    }

    public class ExchangeRestoreDataForBatch : IDisposable
    {
        protected static RALogger logger = RALogger.GetInstance(typeof(ExchangeRestoreDataForBatch));

        public MetadataEntity Metadata
        {
            get
            {
                return HandleMetaData(this.MetadataLists.First());
            }
        }

        public IEnumerable<AveMetadata> MetadataLists { get; set; }

        public String SourceUrlPath { get { return this.Metadata.DisplayPath; } }

        public IRestoreStream RestoreStream { get; set; }
        public System.IO.Stream ContentStream { get; set; }

        public T TryGetMetadata<T>(AveMetadataType type) where T : class
        {
            try
            {
                var md = this.MetadataLists.FirstOrDefault(m => m.MetadataType == type);
                if (md != null)
                {
                    return SerializerHelper.DeserializeByDataContractSerializer<T>(md.GetMetadata<string>());
                }
                else
                {
                    return null;
                }
            }
            catch (Exception ex)
            {
                logger.Warn("Failed to get metadata, error: {0}", ex);
                return null;
            }
        }

        private MetadataEntity HandleMetaData(AveMetadata metadata)
        {
            if (metadata.MetadataType != AveMetadataType.ExchangeMailBox && metadata.MetadataType != AveMetadataType.ExchangeMicrosoftTeams)
            {
                var entityString = metadata.GetMetadata<string>();
                return RestoreCommonUtility.ConvertToBaseEntity(entityString);
            }
            else
            {
                return new MetadataEntity() { DisplayPath = string.Empty };
            }
        }

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