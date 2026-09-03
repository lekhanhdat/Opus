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
    using System.Threading;

    using AvePoint.GCommon.Contract.ExchangeOnline.ExchangeOnlineRestore.Object;
    using AvePoint.RA.CommonUtil;

    public class RestoreDataHandlerBase : IRestoreDataHandlerBase
    {
        protected static RALogger logger = RALogger.GetInstance(typeof(RestoreDataHandlerBase));

        public delegate void ProcessException(string message);

        public RestoreConfig Config;

        //public IRestoreService RestoreService { get; set; }

        private EORestoreType restoreType;

        public void Start(RestoreConfig config, IRestoreService RestoreService)
        {
            this.Config = config;
            this.restoreType = config.RestoreType;
            Thread restoreDataThread = new Thread(new ParameterizedThreadStart(RestoreService.Restore));
            restoreDataThread.IsBackground = true;
            restoreDataThread.Name = "RestoreServiceThread";
            restoreDataThread.Start(this);
        }

        public void ProcessEx(string message)
        {
            var dataBlock = new ExchangeDataBlock() { IsException = true, ExceptionMessage = message };
            Add(dataBlock);
        }

        public virtual void Add(ExchangeDataBlock dataBlock)
        {
        }
        public virtual void AddForEXO(ExchangeDataBlock dataBlock)
        {
        }

        public virtual void AddForSite(ExchangeDataBlock dataBlock)
        {

        }

        public virtual ExchangeDataBlock Get()
        {
            throw new NotImplementedException();
        }

        public virtual Int32 GetOutputCollectionCount()
        {
            throw new NotImplementedException();
        }

        public virtual IEnumerable<RestoreDataBlockCollection> GetDateBlockCollection()
        {
            throw new NotImplementedException();
        }

        #region IDisposable Support
        private bool disposedValue = false; // To detect redundant calls

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    // TODO: dispose managed state (managed objects).
                }

                // TODO: free unmanaged resources (unmanaged objects) and override a finalizer below.
                // TODO: set large fields to null.

                disposedValue = true;
            }
        }

        public void Dispose()
        {
            // Do not change this code. Put cleanup code in Dispose(bool disposing) above.
            Dispose(true);
        }
        #endregion
    }
}