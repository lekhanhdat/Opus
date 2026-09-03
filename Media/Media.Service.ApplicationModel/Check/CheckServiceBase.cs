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
    using AvePoint.Media.Service.DomainModel;
    #endregion

    public abstract class CheckServiceBase<TParameter, TResult>
        : ApplicationModelServiceBase
        , ICheckService
        where TParameter : class, ICheckInfo
        where TResult : class, ICheckResult
    {
        public ICheckResult Check(ICheckInfo checkInfo)
        {
            var info = checkInfo as TParameter;
            return this.InternalCheck(info);
        }

        TResult InternalCheck(TParameter checkInfo)
        {
            var result = default(TResult);
            try
            {
                this.Open(checkInfo);
                result = this.Check(checkInfo);
            }
            catch (Exception e)
            {
                this.ProcessException(e);
            }
            finally { this.Close(); }
            return result;
        }

        public abstract void Open(TParameter checkInfo);
        public abstract TResult Check(TParameter checkInfo);
        public abstract void ProcessException(Exception e);
        public virtual void Close()
        {
            this.Dispose();
        }
        public abstract void Dispose();
    }
}