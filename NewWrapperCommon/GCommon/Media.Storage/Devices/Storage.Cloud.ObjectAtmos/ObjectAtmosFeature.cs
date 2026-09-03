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

namespace AvePoint.Media.Storage.Cloud.ObjectAtmos
{
    class ObjectAtmosFeature : StorageFeature
    {
        private ObjectAtmosFeature(int type, string culture)
        {
            this.Init(type,culture);
        }

        protected override void GenerateDocAveGUIFeatureUnit(CultureInfo culture)
        {
            //StorageFeature sf = new StorageFeature();
            //StorageType type = new StorageType();
            //type.Value = "ObjectAtmos";
            //type.Display = "ObjectAtmos";
            //type.Index = 411;
            //type.Vim = new List<string>() { "atmos_object_vim" };
            //sf.Type = type;
            //sf.Type.DefaultXris.Add("DOCAVE-XAM://ATMOS_OBJECT_VIM?".ToLower());

            //sf.Type.Vim.Add("atmos_object_vim");

            //sf.Features = new List<FeatureUnit> {
            //            new FeatureUnit() {Vim = "atmos_object_vim", DisplayName = "Access Point", Tag = "cloud_objectatmos", Key = "accessPoint".ToLower(CultureInfo.InvariantCulture), KeyName = "AtmosAccessPoint", Visibility="Visible", ValType = "string", GuiType = "TextBox", DefaultValue="http://accessPoint.emccis.com".ToLower(CultureInfo.InvariantCulture)},
            //            new FeatureUnit() {Vim = "atmos_object_vim", DisplayName = "Full Token ID", Tag = "cloud_objectatmos", Key = "name", KeyName = "AtmosUsername", Visibility="Visible", ValType = "string", GuiType = "TextBox"},
            //            new FeatureUnit() {Vim = "atmos_object_vim", DisplayName = "Shared Secret", Tag = "cloud_objectatmos", Key = "secret", KeyName = "AtmosAPIKey", Visibility="Visible", ValType = "string", GuiType = "PasswordBox"},
            //        };
            //Add(sf);
        }

        /// <summary>
        /// 供VIM调用, 获取相应的Feature
        /// </summary>
        private static readonly Dictionary<string, ObjectAtmosFeature> instances = new Dictionary<string, ObjectAtmosFeature>();
        private static Object locker = new Object();
        public static ObjectAtmosFeature Getstances(int type, string culture = "en")
        {
            lock (locker)
            {
                if (!instances.ContainsKey(type + culture))
                {
                    ObjectAtmosFeature atmos = new ObjectAtmosFeature(type, culture);
                    instances[type + culture] = atmos;
                    return atmos;
                }
                else
                {
                    return instances[type + culture];
                }
            }
        }
    }
}
