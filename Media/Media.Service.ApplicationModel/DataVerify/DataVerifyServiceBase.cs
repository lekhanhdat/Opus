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
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using AvePoint.Media.Service.DomainModel;
    using AvePoint.GCommon;
    using System.Reflection;
    #endregion

    public abstract class DataVerifyServiceBase<TParameter, TResult>
        : ApplicationModelServiceBase
        , IDataVerifyService
        where TParameter : class, IDataVerifyInfo
        where TResult : class, IDataVerifyResult
    {
        //AveLogger logger = AveLogger.GetInstance(MethodBase.GetCurrentMethod().DeclaringType);

        public IDataVerifyResult Verify(IDataVerifyInfo dataVerifyInfo)
        {
            var info = dataVerifyInfo as TParameter;
            return this.InternalVerify(info);
        }

        TResult InternalVerify(TParameter dataVerifyInfo)
        {
            var result = default(TResult);
            try
            {
                this.Open(dataVerifyInfo);
                result = this.Verify(dataVerifyInfo);
            }
            catch (Exception e)
            {
                this.ProcessException(e);
                throw;
            }
            finally { this.Close(); }
            return result;
        }

        public abstract void Open(TParameter dataVerifyInfo);
        public abstract TResult Verify(TParameter dataVerifyInfo);
        public abstract void ProcessException(Exception e);
        public virtual void Close()
        {
            this.Dispose();
        }
        public abstract void Dispose();
    }
}
