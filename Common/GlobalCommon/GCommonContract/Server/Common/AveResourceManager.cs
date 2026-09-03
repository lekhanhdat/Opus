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
using System.Globalization;
using System.Reflection;
using System.Resources;
using System.Runtime.Serialization;
using AvePoint.GCommon.Contract.Common;

namespace AvePoint.GCommon.Contract.Server.Common
{
    [DataContract(Namespace = ContractConstants.Namespace)]
    public class AveResourceManager : ResourceManager
    {
        protected I18NMode I18NMode { get; set; }

        public Dictionary<string, I18NMessageDto> I18NMessageDic { get; set; }
        protected AveResourceManager()
            : base()
        {
        }
        public AveResourceManager(string baseName, Assembly assembly)
            : base(baseName, assembly)
        {
        }
        public AveResourceManager(string baseName, Assembly assembly, Type usingResourceSet)
            : base(baseName, assembly, usingResourceSet)
        {
        }

        public AveResourceManager(Dictionary<string, I18NMessageDto> i18nMessageDic)
        {
            if (i18nMessageDic == null)
            {
                throw new ArgumentNullException("i18nMessageDic");
            }
            this.I18NMessageDic = i18nMessageDic;
        }
        public AveResourceManager(List<I18NMessageDto> i18nMessages)
        {
            if (i18nMessages == null)
            {
                throw new ArgumentNullException("i18nMessages");
            }
            this.I18NMode = Common.I18NMode.Default;
            this.I18NMessageDic = new Dictionary<string, I18NMessageDto>();
            foreach (I18NMessageDto item in i18nMessages)
            {
                this.I18NMessageDic[item.Key] = item;
            }
        }
        public override string GetString(string name)
        {
            return GetString(name, "en");
        }
        public override string GetString(string name, CultureInfo culture)
        {
            return GetString(name, culture.Name);
        }
        public string GetString(string name, string culture)
        {
            if (I18NMode == Common.I18NMode.Resource)
            {
                return base.GetString(name, new CultureInfo(culture));
            }
            if (this.I18NMessageDic != null && this.I18NMessageDic.ContainsKey(name))
            {
                return I18NMessageDic[name].GetI18NMessage(culture);
            }
            return null;
        }
    }
}
