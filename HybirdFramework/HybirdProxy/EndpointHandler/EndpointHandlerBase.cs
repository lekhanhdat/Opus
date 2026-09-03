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
using CommonModel.MethodInfo;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace HybirdProxy.EndpointHandler
{
    public abstract class EndpointHandlerBase<Func> where Func: RemoteMethod
    {
        public void Handle(Func param)
        {
            this.PreProcess(param);
            Task.Run(() =>
            {
                this.Process(param);
                this.PostProcess(param);
            });
        }

        public abstract void Process(Func param);


        public virtual void PreProcess(Func param)
        { 
        
        }

        public virtual void PostProcess(Func param)
        { 
        
        }
    }

    public abstract class EndpointHandlerBase<Func, Args, Result> where Func: RemoteInvoke<Args, Result>
    {
        public Result Handle(Args param)
        {
            this.PreProcess(param);
            var result = this.Process(param);
            this.PostProcess(param);
            return result;
        }

        public abstract Result Process(Args param);


        public virtual void PreProcess(Args param)
        {

        }

        public virtual void PostProcess(Args param)
        {

        }
    }
}
