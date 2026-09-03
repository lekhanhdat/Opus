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
    using System.Reflection;
    using AvePoint.GCommon;
    using AvePoint.Media.Service.DomainModel;

    #endregion using directives

    public abstract class DataCalculatorBase<TParameter, TResult>
        : ApplicationModelServiceBase
        , IDataCalculator
        where TParameter : class, ICalculateInfo
        where TResult : class, ICalculateResult
    {


        public ICalculateResult Calculate(ICalculateInfo calculateInfo)
        {
            var info = calculateInfo as TParameter;
            return this.InternalCalculate(info);
        }

        private TResult InternalCalculate(TParameter calculateInfo)
        {
            var result = default(TResult);
            try
            {
                this.Open(calculateInfo);
                result = this.Calculate(calculateInfo);
            }
            catch (Exception e)
            {
                this.ProcessException(e);
            }
            finally { this.Close(); }
            return result;
        }

        public abstract void Open(TParameter calculateInfo);

        public abstract TResult Calculate(TParameter calculateInfo);

        public abstract void ProcessException(Exception e);

        public virtual void Close()
        {
            this.Dispose();
        }

        #region IDisposable

        public abstract void Dispose();

        #endregion IDisposable
    }
}