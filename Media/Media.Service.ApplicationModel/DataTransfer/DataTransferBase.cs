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

    using AvePoint.GCommon;
    using AvePoint.Media.Service.DomainModel;
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Reflection;
    using System.Text;

    #endregion

    public abstract class DataTransferBase<TParameter, TResult>
        : ApplicationModelServiceBase,
        IDataTransferService
        where TParameter : class,IDataTransferJobInfo
        where TResult : class,IDataTransferJobResult
    {


        public IDataTransferJobResult Transfer(IDataTransferJobInfo transferInfo)
        {
            var info = transferInfo as TParameter;
            return this.InternalTransfer(info);
        }

        private TResult InternalTransfer(TParameter transferInfo)
        {
            var result = Activator.CreateInstance(typeof(TResult)) as TResult;
            try
            {
                this.Open(transferInfo);
                result = this.Transfer(transferInfo);
            }
            catch (Exception e)
            {
                this.ProcessException(e, result);
            }
            this.Close();
            return result;
        }

        public abstract void Open(TParameter transferInfo);

        public abstract TResult Transfer(TParameter transferInfo);

        public abstract void ProcessException(Exception e, TResult result);

        public virtual void Close()
        {
            this.Dispose();
        }

        public abstract void Dispose();
    }
}