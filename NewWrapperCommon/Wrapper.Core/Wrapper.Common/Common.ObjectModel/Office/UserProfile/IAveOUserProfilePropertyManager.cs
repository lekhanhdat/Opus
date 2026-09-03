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
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Text;

namespace AvePoint.Wrapper.Common.Office
{
    public interface IAveOUserProfilePropertyManager
    {
        /// <summary>
        /// 返回IAveOUserProfileCorePropertyManager接口变量或对象，用于User Profile CoreProperty还原，返回值可能为空
        /// </summary>
        /// <returns></returns>
        IAveOUserProfileCorePropertyManager GetCoreProperties();
        /// <summary>
        /// 返回IAveOUserProfileSubtypePropertyManager接口变量或对象，用于User Profile SubtypeProperty还原，返回值可能为空
        /// </summary>
        /// <returns></returns>
        IAveOUserProfileSubtypePropertyManager GetProfileSubtypeProperties(string name);
        /// <summary>
        /// 返回IAveOUserProfileTypePropertyManager接口变量或对象，用于User Profile TypeProperty还原，返回值可能为空
        /// </summary>
        /// <returns></returns>
        IAveOUserProfileTypePropertyManager GetProfileTypeProperties();
        void Reset();

    }
}
