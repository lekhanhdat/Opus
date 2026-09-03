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
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;

    public enum TemplateMappingLevel
    {
        Global=0,//表示查找的时候全部查找，方便全局使用和测试
        Web,//web level template mapping的映射查找
        List,//list Level template的映射查找
    }

    public class TemplateKeyInfo
    {
        public TemplateMappingLevel keyLevel = TemplateMappingLevel.Global;
        public string keyValue = string.Empty;
        public string templateSrcValue = string.Empty;
        public TemplateKeyInfo(TemplateMappingLevel level, string key, string template)
        {
            keyLevel = level;
            keyValue = key;
            templateSrcValue = template;
        }
    }
}
