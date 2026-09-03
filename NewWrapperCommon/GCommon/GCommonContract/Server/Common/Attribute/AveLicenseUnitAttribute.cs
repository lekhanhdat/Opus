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
using System.Text;
using AvePoint.GCommon.Contract.AveLicense;
using AvePoint.GCommon.Contract.Server.Audit;
using AvePoint.GCommon.Contract.Server.Common.Profile.Object;

namespace AvePoint.GCommon.Contract.Server.Common.Attribute
{
    [AttributeUsage(AttributeTargets.All, AllowMultiple = false, Inherited = true)]
    public class AveLicenseUnitAttribute : System.Attribute
    {
        //方法模块
        public Type Module { get; set; }

        /// <summary>
        /// unit在lic文件里对应的string,需要与LCEIP同事协商
        /// </summary>
        public string LicFileConstants { get; set; }

        /// <summary>
        /// unit在lic文件里扩展的string，模块的扩展功能
        /// </summary>
        public string ExtensionFileConstants { get; set; }

        /// <summary>
        /// 用于local 模块，用type判断模块属于哪个版本的farm
        /// </summary>
        public LicenseModuleType Type { get; set; }

        /// <summary>
        /// 用于Migration模块，用type获取流量，需要询问Migration开发具体值
        /// </summary>
        public ProfileType MGProfileType { get; set; }

        public AveLicenseUnitAttribute() : base() { }

        public AveLicenseUnitAttribute(Type Module, string LicFileConstants)
        {
            this.Module = Module;
            this.LicFileConstants = LicFileConstants;
        }

        public AveLicenseUnitAttribute(Type Module, string LicFileConstants, LicenseModuleType Type)
        {
            this.Module = Module;
            this.LicFileConstants = LicFileConstants;
            this.Type = Type;
        }

        public AveLicenseUnitAttribute(Type Module, string LicFileConstants, ProfileType MGProfileType)
        {
            this.Module = Module;
            this.LicFileConstants = LicFileConstants;
            this.MGProfileType = MGProfileType;
        }

        public AveLicenseUnitAttribute(Type Module, string LicFileConstants, string ExtensionFileConstants)
        {
            this.Module = Module;
            this.LicFileConstants = LicFileConstants;
            this.ExtensionFileConstants = ExtensionFileConstants;
        }

        public AveLicenseUnitAttribute(Type Module, string LicFileConstants, string ExtensionFileConstants, LicenseModuleType Type)
        {
            this.Module = Module;
            this.LicFileConstants = LicFileConstants;
            this.ExtensionFileConstants = ExtensionFileConstants;
            this.Type = Type;
        }
    }
}