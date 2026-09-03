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

namespace AvePoint.Media.Storage.Cloud.S3Compatible
{
    using AvePoint.Media.Storage.Resources.AmazonI18N;
    #region using directives
    //using StorageResources.S3CompatibleI18N;
    using AvePoint.Media.Storage.Resources.S3CompatibleI18N;
    using System;
    using System.Collections.Generic;
    using System.Globalization;
    #endregion
    class S3CompatibleFeature : StorageFeature
    {
        #region S3CompatibleFeature 自己负责初始化的部分, 在Application生存周期内只会初始化一次, 外部不需要知道的逻辑
        /// <summary>
        /// 私有构造函数， 此类不允许在外部实例化。 <see cref="S3CompatibleFeature"/> class.
        /// </summary>
        private static Object locker = new Object();
        private S3CompatibleFeature(int type, string culture)
        {
            this.Init(type, culture);
        }

        protected override void GenerateSingleTypeFeatureUnit(CultureInfo culture)
        {
            StorageFeature sf = new StorageFeature();
            StorageType type = new StorageType();
            type.Value = "S3Compatible";
            type.Display = "S3Compatible";
            type.Index = 410;
            type.Vim = new List<string>() { "s3compatible_vim" };
            type.DefaultXris.Add("DOCAVE-XAM://S3COMPATIBLE_VIM?".ToLower(CultureInfo.InvariantCulture));
            sf.Type = type;
            sf.Features = new List<FeatureUnit> {
                        new FeatureUnit() {Vim = "s3compatible_vim", DisplayName = "Bucket Name", Tag = "cloud_s3compatible", Key = "bucketName".ToLower(CultureInfo.InvariantCulture), KeyName = "S3CompatibleBucketName", Visibility="Visible", ValType = "string", GuiType = "TextBox"},
                        new FeatureUnit() {Vim = "s3compatible_vim", DisplayName = "Access Key ID", Tag = "cloud_s3compatible", Key = "name", KeyName = "S3CompatibleUsername", Visibility="Visible", ValType = "string", GuiType = "TextBox"},
                        new FeatureUnit() {Vim = "s3compatible_vim", DisplayName = "Secret Access Key", Tag = "cloud_s3compatible", Key = "secret", KeyName = "S3CompatibleAPIKey", Visibility="Visible", ValType = "string", GuiType = "PasswordBox"},
                        new FeatureUnit() {Vim = "s3compatible_vim", DisplayName = "Endpoint", Tag = "cloud_s3compatible", Key = "endpoint", KeyName = "S3CompatibleEndpoint", Visibility="Visible", ValType = "string", GuiType = "TextBox"},
                        new FeatureUnit() {Vim = "s3compatible_vim", DisplayName = AmazonI18N.ResourceManager.GetString("MediaStorage_Amazon_Proxy", culture), Tag = "cloud_amazon_Proxy".ToLower(CultureInfo.InvariantCulture), Key = "Amazon_Proxy".ToLower(CultureInfo.InvariantCulture), Value = "cloud_amazon_setting".ToLower(CultureInfo.InvariantCulture),  KeyName = "AmazonProxySetting", Visibility="Visible", ValType = "string", GuiType = "CheckBox", ChildFeatures = new List<FeatureUnit>(){
                            new FeatureUnit() {Vim = "s3compatible_vim", DisplayName = AmazonI18N.ResourceManager.GetString("MediaStorage_Amazon_Proxy_Host", culture), Tag = "cloud_amazon_setting".ToLower(CultureInfo.InvariantCulture), Key = "ProxyIp".ToLower(CultureInfo.InvariantCulture), KeyName = "AmazonProxyHost", Visibility="Collapsed", ValType = "string", IsRequiredOption = true, GuiType = "TextBox"},
                            new FeatureUnit() {Vim = "s3compatible_vim", DisplayName = AmazonI18N.ResourceManager.GetString("MediaStorage_Amazon_Proxy_Port", culture), Tag = "cloud_amazon_setting".ToLower(CultureInfo.InvariantCulture), Key = "ProxyPort".ToLower(CultureInfo.InvariantCulture), KeyName = "AmazonProxyPort", Visibility="Collapsed", ValType = "string", IsRequiredOption = true, GuiType = "TextBox"},
                            new FeatureUnit() {Vim = "s3compatible_vim", DisplayName = AmazonI18N.ResourceManager.GetString("MediaStorage_Amazon_Proxy_Username", culture), Tag = "cloud_amazon_setting".ToLower(CultureInfo.InvariantCulture), Key = "ProxyUsername".ToLower(CultureInfo.InvariantCulture), KeyName = "AmazonProxyUsername", Visibility="Collapsed", ValType = "string", IsRequiredOption = false, GuiType = "TextBox", CanNullOrEmpty = "true"},
                            new FeatureUnit() {Vim = "s3compatible_vim", DisplayName = AmazonI18N.ResourceManager.GetString("MediaStorage_Amazon_Proxy_Password", culture), Tag = "cloud_amazon_setting".ToLower(CultureInfo.InvariantCulture), Key = "ProxyPasswordSecret".ToLower(CultureInfo.InvariantCulture), KeyName = "AmazonProxyPasswordSecret", Visibility="Collapsed", ValType = "string", IsRequiredOption = false, GuiType = "PasswordBox", CanNullOrEmpty = "true"},
                        }},
            };
            Add(sf);
        }

        protected override void GenerateDocAveGUIFeatureUnit(CultureInfo culture)
        {
            StorageFeature sf = new StorageFeature();
            StorageType type = new StorageType();
            type.Value = "S3Compatible";
            type.Display = S3CompatibleI18N.ResourceManager.GetString("MediaStorage_S3Compatible_Compatible_Amazon_S3", culture);
            type.Index = 410;
            type.Vim = new List<string>() { "s3compatible_vim" };
            sf.Type = type;
            sf.Type.DefaultXris.Add("DOCAVE-XAM://S3COMPATIBLE_VIM?".ToLower(CultureInfo.InvariantCulture));
            sf.Features = new List<FeatureUnit>
            {
                new FeatureUnit()
                {
                    Vim = "s3compatible_vim",
                    DisplayName = S3CompatibleI18N.ResourceManager.GetString("MediaStorage_S3Compatible_Bucket_Name", culture),
                    Tag = "cloud_s3compatible",
                    Key = "bucketName".ToLower(CultureInfo.InvariantCulture),
                    KeyName = "S3CompatibleBucketName",
                    Visibility = "Visible",
                    ValType = "string",
                    IsRequiredOption = true,
                    GuiType = "TextBox",
                    DemoValue = "docave",
                },
                new FeatureUnit()
                {
                    Vim = "s3compatible_vim",
                    DisplayName = S3CompatibleI18N.ResourceManager.GetString("MediaStorage_S3Compatible_Access_Key_ID", culture),
                    Tag = "cloud_s3compatible",
                    Key = "name",
                    KeyName = "S3CompatibleUsername",
                    Visibility = "Visible",
                    ValType = "string",
                    IsRequiredOption = true,
                    GuiType = "TextBox",
                    DemoValue = S3CompatibleI18N.ResourceManager.GetString("MediaStorage_S3Compatible_S3Compatible_Username", culture)
                },
                new FeatureUnit()
                {
                    Vim = "s3compatible_vim",
                    DisplayName = S3CompatibleI18N.ResourceManager.GetString("MediaStorage_S3Compatible_Secret_Access_Key", culture),
                    Tag = "cloud_s3compatible",
                    Key = "secret",
                    KeyName = "S3CompatibleAPIKey",
                    Visibility = "Visible",
                    ValType = "string",
                    IsRequiredOption = true,
                    GuiType = "PasswordBox"
                },
                new FeatureUnit()
                {
                    Vim = "s3compatible_vim",
                    DisplayName = S3CompatibleI18N.ResourceManager.GetString("MediaStorage_S3Compatible_Endpoint", culture),
                    Tag = "cloud_s3compatible",
                    Key = "endpoint",
                    KeyName = "S3CompatibleEndpoint",
                    Visibility = "Visible",
                    ValType = "string",
                    IsRequiredOption = true,
                    GuiType = "TextBox",
                },
                new FeatureUnit() {Vim = "amazon_vim", DisplayName = AmazonI18N.ResourceManager.GetString("MediaStorage_Amazon_Proxy", culture), Tag = "cloud_amazon_Proxy".ToLower(CultureInfo.InvariantCulture), Key = "Amazon_Proxy".ToLower(CultureInfo.InvariantCulture), Value = "cloud_amazon_setting".ToLower(CultureInfo.InvariantCulture),  KeyName = "AmazonProxySetting", Visibility="Visible", ValType = "string", GuiType = "CheckBox", ChildFeatures = new List<FeatureUnit>(){
                            new FeatureUnit() {Vim = "s3compatible_vim", DisplayName = AmazonI18N.ResourceManager.GetString("MediaStorage_Amazon_Proxy_Host", culture), Tag = "cloud_amazon_setting".ToLower(CultureInfo.InvariantCulture), Key = "ProxyIp".ToLower(CultureInfo.InvariantCulture), KeyName = "AmazonProxyHost", Visibility="Collapsed", ValType = "string", IsRequiredOption = true, GuiType = "TextBox"},
                            new FeatureUnit() {Vim = "s3compatible_vim", DisplayName = AmazonI18N.ResourceManager.GetString("MediaStorage_Amazon_Proxy_Port", culture), Tag = "cloud_amazon_setting".ToLower(CultureInfo.InvariantCulture), Key = "ProxyPort".ToLower(CultureInfo.InvariantCulture), KeyName = "AmazonProxyPort", Visibility="Collapsed", ValType = "string", IsRequiredOption = true, GuiType = "TextBox"},
                            new FeatureUnit() {Vim = "s3compatible_vim", DisplayName = AmazonI18N.ResourceManager.GetString("MediaStorage_Amazon_Proxy_Username", culture), Tag = "cloud_amazon_setting".ToLower(CultureInfo.InvariantCulture), Key = "ProxyUsername".ToLower(CultureInfo.InvariantCulture), KeyName = "AmazonProxyUsername", Visibility="Collapsed", ValType = "string", IsRequiredOption = false, GuiType = "TextBox", CanNullOrEmpty = "true"},
                            new FeatureUnit() {Vim = "s3compatible_vim", DisplayName = AmazonI18N.ResourceManager.GetString("MediaStorage_Amazon_Proxy_Password", culture), Tag = "cloud_amazon_setting".ToLower(CultureInfo.InvariantCulture), Key = "ProxyPasswordSecret".ToLower(CultureInfo.InvariantCulture), KeyName = "AmazonProxyPasswordSecret", Visibility="Collapsed", ValType = "string", IsRequiredOption = false, GuiType = "PasswordBox", CanNullOrEmpty = "true"},
                        }},
                new FeatureUnit()
                {
                    Vim = "s3compatible_vim",
                    DisplayName = S3CompatibleI18N.ResourceManager.GetString("MediaStorage_S3Compatible_Advanced", culture),
                    Tag = "cloud_s3compatible",
                    Key = "advanced".ToLower(CultureInfo.InvariantCulture),
                    Value = "s3compatible_vim_ExtendedParameters",
                    KeyName = "S3CompatibleAdvanced",
                    Visibility = "Visible",
                    ValType = "string",
                    GuiType = "CheckBox",
                    ChildFeatures = new List<FeatureUnit>(){
                        new FeatureUnit()
                        {
                            Vim = "s3compatible_vim",
                            DisplayName = S3CompatibleI18N.ResourceManager.GetString("MediaStorage_S3Compatible_Extended_Parameters", culture),
                            Tag = "s3compatible_vim_ExtendedParameters",
                            Key = "ExtendedParameters".ToLower(CultureInfo.InvariantCulture),
                            KeyName = "S3CompatibleExtendedParameters",
                            Visibility ="Collapsed",
                            ValType = "string",
                            GuiType = "TextArea",
                            CanNullOrEmpty = "true"
                        }
                    }
                }
            };
            Add(sf);
        }

        protected override void GenerateConnectorGUIFeatureUnit(CultureInfo culture)
        {
            StorageFeature sf = new StorageFeature();
            StorageType type = new StorageType();
            type.Value = "S3Compatible";
            type.Display = S3CompatibleI18N.ResourceManager.GetString("MediaStorage_S3Compatible_Compatible_Amazon_S3", culture);
            type.Index = 410;
            type.Vim = new List<string>() { "s3compatible_vim" };
            sf.Type = type;
            sf.Features = new List<FeatureUnit> {
                        new FeatureUnit() {Vim = "s3compatible_vim", DisplayName = S3CompatibleI18N.ResourceManager.GetString("MediaStorage_S3Compatible_Access_Key_ID",culture), Tag = "cloud_s3compatible", Key = "name", KeyName = "S3CompatibleUsername", Visibility="Visible", ValType = "string", IsRequiredOption = true, GuiType = "TextBox"},
                        new FeatureUnit() {Vim = "s3compatible_vim", DisplayName = S3CompatibleI18N.ResourceManager.GetString("MediaStorage_S3Compatible_Bucket_Name",culture), Tag = "cloud_s3compatible", Key = "bucketName", KeyName = "S3CompatibleBucketName", Visibility="Visible", ValType = "string", IsRequiredOption = true, GuiType = "TextBox", FeatureFlag=(int)(FeatureUnitFlag.CloudContainer|FeatureUnitFlag.Path) ,  CanModifi = false},
                        new FeatureUnit() {Vim = "s3compatible_vim", DisplayName = S3CompatibleI18N.ResourceManager.GetString("MediaStorage_S3Compatible_Secret_Access_Key",culture), Tag = "cloud_s3compatible", Key = "secret", KeyName = "S3CompatibleAPIKey", Visibility="Visible", ValType = "string", IsRequiredOption = true, GuiType = "PasswordBox"},
                        new FeatureUnit() {Vim = "s3compatible_vim", DisplayName = S3CompatibleI18N.ResourceManager.GetString("MediaStorage_S3Compatible_Endpoint",culture), Tag = "cloud_s3compatible", Key = "endpoint", KeyName = "S3CompatibleEndpoint", Visibility="Visible", ValType = "string", IsRequiredOption = true, GuiType = "TextBox"},
                        new FeatureUnit() {Vim = "s3compatible_vim", DisplayName = AmazonI18N.ResourceManager.GetString("MediaStorage_Amazon_Proxy", culture), Tag = "cloud_amazon_Proxy".ToLower(CultureInfo.InvariantCulture), Key = "Amazon_Proxy".ToLower(CultureInfo.InvariantCulture), Value = "cloud_amazon_setting".ToLower(CultureInfo.InvariantCulture),  KeyName = "AmazonProxySetting", Visibility="Visible", ValType = "string", GuiType = "CheckBox", ChildFeatures = new List<FeatureUnit>(){
                            new FeatureUnit() {Vim = "s3compatible_vim", DisplayName = AmazonI18N.ResourceManager.GetString("MediaStorage_Amazon_Proxy_Host", culture), Tag = "cloud_amazon_setting".ToLower(CultureInfo.InvariantCulture), Key = "ProxyIp".ToLower(CultureInfo.InvariantCulture), KeyName = "AmazonProxyHost", Visibility="Collapsed", ValType = "string", IsRequiredOption = true, GuiType = "TextBox"},
                            new FeatureUnit() {Vim = "s3compatible_vim", DisplayName = AmazonI18N.ResourceManager.GetString("MediaStorage_Amazon_Proxy_Port", culture), Tag = "cloud_amazon_setting".ToLower(CultureInfo.InvariantCulture), Key = "ProxyPort".ToLower(CultureInfo.InvariantCulture), KeyName = "AmazonProxyPort", Visibility="Collapsed", ValType = "string", IsRequiredOption = true, GuiType = "TextBox"},
                            new FeatureUnit() {Vim = "s3compatible_vim", DisplayName = AmazonI18N.ResourceManager.GetString("MediaStorage_Amazon_Proxy_Username", culture), Tag = "cloud_amazon_setting".ToLower(CultureInfo.InvariantCulture), Key = "ProxyUsername".ToLower(CultureInfo.InvariantCulture), KeyName = "AmazonProxyUsername", Visibility="Collapsed", ValType = "string", IsRequiredOption = false, GuiType = "TextBox", CanNullOrEmpty = "true"},
                            new FeatureUnit() {Vim = "s3compatible_vim", DisplayName = AmazonI18N.ResourceManager.GetString("MediaStorage_Amazon_Proxy_Password", culture), Tag = "cloud_amazon_setting".ToLower(CultureInfo.InvariantCulture), Key = "ProxyPasswordSecret".ToLower(CultureInfo.InvariantCulture), KeyName = "AmazonProxyPasswordSecret", Visibility="Collapsed", ValType = "string", IsRequiredOption = false, GuiType = "PasswordBox", CanNullOrEmpty = "true"},
                        }},
                        new FeatureUnit() {Vim = "s3compatible_vim", DisplayName = S3CompatibleI18N.ResourceManager.GetString("MediaStorage_S3Compatible_Advanced",culture), Tag = "cloud_s3compatible", Key = "advanced".ToLower(CultureInfo.InvariantCulture), Value = "s3compatible_vim_ExtendedParameters",  KeyName = "S3CompatibleWithParamsExtension", Visibility="Visible", ValType = "string", GuiType = "CheckBox", ChildFeatures = new List<FeatureUnit>(){
                            new FeatureUnit() {Vim = "s3compatible_vim", DisplayName = S3CompatibleI18N.ResourceManager.GetString("MediaStorage_S3Compatible_Extended_Parameters", culture),Tag = "s3compatible_vim_ExtendedParameters", Key = "ExtendedParameters".ToLower(CultureInfo.InvariantCulture), KeyName = "S3CompatibleExtendedParameters", Visibility ="Collapsed",ValType = "string",IsRequiredOption = true, GuiType = "TextArea",CanNullOrEmpty = "true"} } }
            };
            Add(sf);
        }
        #endregion

        /// <summary>
        /// 供VIM调用, 获取相应的Feature
        /// </summary>
        private static readonly Dictionary<string, S3CompatibleFeature> instances = new Dictionary<string, S3CompatibleFeature>();

        public static S3CompatibleFeature Getstances(int type, string culture = "en")
        {
            lock (locker)
            {
                if (!instances.ContainsKey(type + culture))
                {
                    S3CompatibleFeature s3Compatible = new S3CompatibleFeature(type, culture);
                    foreach (var feature in s3Compatible.FeatureObjs)
                    {
                        feature.AdvancedOptions.AddRange(new List<string>()
                            {
                                "RetryInterval=30000",
                                "RetryCount=6"
                            });
                    }
                    instances[type + culture] = s3Compatible;
                    return s3Compatible;
                }
                else
                {
                    return instances[type + culture];
                }
            }
        }
    }
}

