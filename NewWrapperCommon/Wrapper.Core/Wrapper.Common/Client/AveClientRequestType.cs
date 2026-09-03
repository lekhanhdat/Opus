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
namespace AvePoint.Wrapper.Common
{
    /// <summary>
    /// 继承IAveRequest的所有类的枚举，request的Type属性使用该枚举赋值，用来判断当前request的类型
    /// </summary>
    public enum AveClientRequestType
    {
        AveWebServiceRequest = 0,//web service request
        AveClientExtensionRequest = 1,//目前已经不进行维护，不需要考虑
        AveClientOMRequest = 2,
        AveClientCompoundRequest = 3,//10模拟使用的request(14)
        AveClientOM2013Request = 4,//13模拟使用的request(15)
        AveClientOMOffice365Request = 5,//真实365使用的request(16)
        AveClientOM2016Request = 6,//16模拟使用的request(16)
        AveClientOM2019Request = 7//19模拟使用的request(16)
    }
}
