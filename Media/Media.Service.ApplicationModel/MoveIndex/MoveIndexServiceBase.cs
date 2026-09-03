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
    using System.Threading.Tasks;
    #endregion

    public abstract class MoveIndexServiceBase<TParameter, TResult>
        : ApplicationModelServiceBase
        , IMoveIndexService
        where TParameter : class,IMoveIndexInfo
        where TResult : class,IMoveIndexResult
    {
        AveLogger logger = AveLogger.GetInstance(MethodBase.GetCurrentMethod().DeclaringType);

        public async Task<IMoveIndexResult> MoveIndexAsync(IMoveIndexInfo info)
        {
            var tempInfo = info as TParameter;
            return await this.InternalMoveIndexAsync(tempInfo);
        }

        async Task<TResult> InternalMoveIndexAsync(TParameter info)
        {
            var result = Activator.CreateInstance(typeof(TResult)) as TResult;
            try
            {
                this.Open(info);
                result = await this.MoveAsync(info);
            }
            catch (Exception e)
            {
                logger.Error("Internal Move Index Failed,Failed Informations:{0}", e.ToString());
                this.ProcessException(e, result);
                throw;
            }
            finally
            {
                try
                {
                    this.GenerateJobReport();
                }
                catch (Exception)
                {
                    throw;
                }
                this.Close();
            }
            return result;
        }

        public abstract void Open(TParameter info);
        public abstract Task<TResult> MoveAsync(TParameter info);
        public abstract void ProcessException(Exception e, TResult result);
        public virtual void GenerateJobReport()
        {

        }
        public virtual void Close()
        {

        }

        public void Dispose()
        {

        }
    }
}
