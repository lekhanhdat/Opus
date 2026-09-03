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
using AvePoint.RA.Common.Global.Utils;
using AvePoint.RA.Contract.RoleAssignments;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Text;

namespace AvePoint.RA.Web.Common.Filters
{
    public class ParamsApiActionFilter : ActionFilterAttribute
    {
        public override void OnActionExecuting(ActionExecutingContext actionContext)
        {
            base.OnActionExecuting(actionContext);
            //获取请求消息提数据
            Stream stream = actionContext.HttpContext.Request.Body;
            Encoding encoding = Encoding.UTF8;
            stream.Position = 0;
            string responseData = "";
            using (StreamReader reader = new StreamReader(stream, encoding))
            {
                responseData = reader.ReadToEnd().ToString();
            }
            //反序列化进行处理
            var obj = SerializerHelper.DeserializeByJsonConvert<ParamBase>(responseData);
            //在action执行前终止请求时，应该使用填充方法Response，将不返回action方法体。
            if (obj == null)
                actionContext.Result = new ObjectResult(obj) { StatusCode = (int)HttpStatusCode.OK };
            //check if user can access the scopeIds

            //if (permissionOK)
            //{
            //    actionContext.Response = actionContext.Request.CreateResponse(HttpStatusCode.OK, obj);
            //}
        }
    }
}