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
using System.Linq;
using System.Text;
using AvePoint.GCommon.Contract.AveLicense;

namespace AvePoint.GCommon.Utility.AveLicense
{
    /// <summary>
    ///保存有产品类型、是否发送CEIP、是否试用So、是否使用DocAve这些信息
    /// </summary>
    public class SetupConfig
    {
        /// <summary>
        /// ProductType---产品类型 DocAve NetApp IBM
        /// </summary>
        public ProductType ProductType { get; set; }

        /// <summary>
        /// IsRegisterDocAve---是否发送ceip信息，true 发送，false 不发
        /// </summary>
        public bool IsRegisterDocAve { get; set; }

        /// <summary>
        /// IsSoOnTrial---是否试用So模块，为了扩展，目前不被使用
        /// </summary>
        /// 
        public bool IsSoOnTrial { get; set; }

        /// <summary>
        /// IsDocAveOnTrial---是否使用DocAve模块，为了扩展，目前不被使用
        /// </summary>
        public bool IsDocAveOnTrial { get; set; }
    }
}
