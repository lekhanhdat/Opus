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




namespace AvePoint.Media.ClassicStorage.Cloud.Common
{
    #region using directives
    using System;
    using System.Collections.Generic;
    using System.Text;
    using System.Globalization;
    using AvePoint.Media.ClassicStorage.Resources.CloudCommonI18N;
    using AvePoint.GCommon;
    #endregion

    public class CommCloudFeature 
    {
        private static readonly Dictionary<string, CommCloudFeature> instances = new Dictionary<string, CommCloudFeature>();

        public static StorageFeature Getstances(string culture = "en")
        {
            if (!instances.ContainsKey(culture))
            {
                CommCloudFeature commCloud = new CommCloudFeature(culture);
                instances[culture] = commCloud;
                return commCloud.sf;
            }
            else
            {
                return instances[culture].sf;
            }
        }

        private StorageFeature sf;

        private CommCloudFeature(string culture)
        {
            this.Init(culture);
        }

        protected void Init(string culture)
        {
            sf = new StorageFeature();
            //定义StorageType
            CultureInfo cultureInfo = new CultureInfo(culture);
            StorageType cloud = new StorageType();
            cloud.Index = 4;
            cloud.Display = CloudCommonI18N.ResourceManager.GetString("MediaStorage_CloudCommon_Cloud_Storage", cultureInfo);
            cloud.Value = "Cloud";
            cloud.Vim = new List<string>();
            cloud.DefaultXris = new List<string>();

            sf.Type = cloud;

            sf.IsNeedSpaceThreshold = false;
            sf.ProgressForeground = new FeatureColor(255, 94, 137, 47);

            sf.Features = new List<FeatureUnit>() {
                  new FeatureUnit() {Vim = "rackspace_vim", HasSparrow=true, DisplayName = CloudCommonI18N.ResourceManager.GetString("MediaStorage_CloudCommon_Cloud_Type", cultureInfo), Tag = "ComBox_CloudType", Value = "cType".ToLower(CultureInfo.InvariantCulture), Key = "cType".ToLower(CultureInfo.InvariantCulture), KeyName="CloudType", GuiType="ComboBox", Visibility="Visible", ValType="string", ChildFeatures = new List<FeatureUnit>()}
            };
            sf.Type.Display = CloudCommonI18N.ResourceManager.GetString("MediaStorage_CloudCommon_Cloud_Storage", cultureInfo);
            sf.Features[0].DisplayName = CloudCommonI18N.ResourceManager.GetString("MediaStorage_CloudCommon_Cloud_Type", cultureInfo);
        }
    }
}
