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



namespace AvePoint.Media.Storage.Cloud.Amazon
{
    #region using directives
    using System;
    using System.Collections.Generic;
    using System.Diagnostics.CodeAnalysis;
    using System.Globalization;
    using AvePoint.Media.Storage.Resources.AmazonI18N;
    #endregion

    /// <summary>
    /// 此类用于描述AmazonFeature类型的存储介质的特性
    /// </summary>
    /// 
    class AmazonFeature : StorageFeature
    {
        #region AmazonFeature 自己负责初始化的部分, 在Application生存周期内只会初始化一次, 外部不需要知道的逻辑
        /// <summary>
        /// 私有构造函数， 此类不允许在外部实例化。 <see cref="AmazonFeature"/> class.
        /// </summary>
        private AmazonFeature(int type, string culture)
        {
            this.Init(type, culture);
        }

        protected override void GenerateSingleTypeFeatureUnit(CultureInfo culture)
        {
            StorageFeature sf = new StorageFeature();
            StorageType type = new StorageType();
            type.Value = "Amazon";
            type.Display = "Amazon S3";
            type.Index = 401;
            type.Vim = new List<string>() { "amazon_vim" };

            type.DefaultXris.Add("DOCAVE-XAM://AMAZON_VIM?REGION=".ToLower(CultureInfo.InvariantCulture) + "USSTANDARD");
            sf.Type = type;
            sf.Description = AmazonI18N.ResourceManager.GetString("MediaStorage_Amazon_Configure_your_bucket_name_Access_Key_ID_and_Secret_Access_Key", culture);
            sf.Features = new List<FeatureUnit> {
                        new FeatureUnit() {Vim = "amazon_vim", DisplayName = "Bucket Name", Tag = "cloud_amazon", Key = "bucketName".ToLower(CultureInfo.InvariantCulture), KeyName = "AmazonBucketName", Visibility="Visible", ValType = "string", GuiType = "TextBox", ValidateRegPats = new List<string>(){
                            @"^[_a-z0-9\.\-]+$" + "\t0\t"+AmazonI18N.ResourceManager.GetString("MediaStorage_Amazon_Can_contain_lowercase_letters_numbers_periods_underscores_and_dashes", culture),
                            "^[a-z0-9]+.*$\t0\t"+AmazonI18N.ResourceManager.GetString("MediaStorage_Amazon_Must_start_with_a_number_or_letter", culture),
                            @"^[_a-z0-9\.\-]{3,255}$" + "\t0\t"+AmazonI18N.ResourceManager.GetString("MediaStorage_Amazon_Must_be_between_3_and_255_characters_long", culture),
                            @".*\d{1,3}\.\d{1,3}\.\d{1,3}\.\d{1,3}.*" + "\t1\t"+AmazonI18N.ResourceManager.GetString("MediaStorage_Amazon_Must_not_be_formatted_as_an_IP_address", culture)
                        }},
                        new FeatureUnit() {Vim = "amazon_vim", DisplayName = "Access Key ID", Tag = "cloud_amazon", Key = "name", KeyName = "AmazonUsername", Visibility="Visible", ValType = "string", GuiType = "TextBox"},
                        new FeatureUnit() {Vim = "amazon_vim", DisplayName = "Secret Access Key", Tag = "cloud_amazon", Key = "secret", KeyName = "AmazonAPIKey", Visibility="Visible", ValType = "string", GuiType = "PasswordBox"},
                        new FeatureUnit() {Vim = "amazon_vim", DisplayName = "Storage Region", Tag = "cloud_amazon", Value = "region", Key = "region", KeyName = "AmazonRegion", Visibility="Visible", ValType = "string", GuiType = "ComboBox", ChildFeatures = new List<FeatureUnit>() {
                            new FeatureUnit() {Vim = "amazon_vim", DisplayName = "US Standard", Tag = "cloud_amazon_region", Value = "USSTANDARD".ToLower(CultureInfo.InvariantCulture), Key = "USSTANDARD".ToLower(CultureInfo.InvariantCulture), KeyName = "AmazonRegionUSStandard", Visibility="Visible", ValType = "string", GuiType = "TextBox"},
                            new FeatureUnit() {Vim = "amazon_vim", DisplayName = "US West (Northern California)", Tag = "cloud_amazon_region", Value = "USWEST".ToLower(CultureInfo.InvariantCulture), Key = "USWEST".ToLower(CultureInfo.InvariantCulture), KeyName = "AmazonRegionUSWest", Visibility="Visible", ValType = "string", GuiType = "TextBox"},
                            new FeatureUnit() {Vim = "amazon_vim", DisplayName = "EU (Ireland)", Tag = "cloud_amazon_region", Key = "EU".ToLower(CultureInfo.InvariantCulture), Value = "EU".ToLower(CultureInfo.InvariantCulture), KeyName = "AmazonRegionEU", Visibility="Visible", ValType = "string", GuiType = "TextBox"},
                            new FeatureUnit() {Vim = "amazon_vim", DisplayName = "Asia Pacific (Singapore)", Tag = "cloud_amazon_region", Key = "APAC".ToLower(CultureInfo.InvariantCulture), Value = "APAC".ToLower(CultureInfo.InvariantCulture), KeyName = "AmazonRegionAPAC", Visibility="Visible", ValType = "string", GuiType = "TextBox"},
                            new FeatureUnit() {Vim = "amazon_vim", DisplayName = "Asia Pacific (Tokyo)", Tag = "cloud_amazon_region", Key = "TOKYO".ToLower(CultureInfo.InvariantCulture), Value = "TOKYO".ToLower(CultureInfo.InvariantCulture), KeyName = "AmazonRegionTokyo", Visibility="Visible", ValType = "string", GuiType = "TextBox"},
                        }},
            };

            Add(sf);
        }

        [SuppressMessage("FxCopCustomRules", "C100007:SpellCheckStringValues", MessageId = "frankfurt")]
        protected override void GenerateDocAveGUIFeatureUnit(CultureInfo culture)
        {
            StorageFeature sf = new StorageFeature();
            StorageType type = new StorageType();
            type.Value = "Amazon";
            type.Display = AmazonI18N.ResourceManager.GetString("MediaStorage_Amazon_Amazon_S3", culture);
            type.Index = 401;
            type.Vim = new List<string>() { "amazon_vim" };
            sf.Type = type;
            sf.Type.DefaultXris.Add("DOCAVE-XAM://AMAZON_VIM?REGION=".ToLower(CultureInfo.InvariantCulture) + "USSTANDARD");
            sf.Description = AmazonI18N.ResourceManager.GetString("MediaStorage_Amazon_Configure_your_bucket_name_Access_Key_ID_and_Secret_Access_Key", culture);
            sf.Features = new List<FeatureUnit> {
                        new FeatureUnit() {Vim = "amazon_vim", DisplayName = AmazonI18N.ResourceManager.GetString("MediaStorage_Amazon_Bucket_Name",culture), Tag = "cloud_amazon", Key = "bucketName".ToLower(CultureInfo.InvariantCulture), KeyName = "AmazonBucketName", Visibility="Visible", ValType = "string", IsRequiredOption = true, GuiType = "TextBox", DemoValue="docave", ValidateRegPats = new List<string>(){
                            @"^[_a-z0-9\.\-]+$|^$" + "\t0\t"+AmazonI18N.ResourceManager.GetString("MediaStorage_Amazon_Can_contain_lowercase_letters_numbers_periods_underscores_and_dashes", culture),
                            "^[a-z0-9]+.*$|^$" + "\t0\t"+AmazonI18N.ResourceManager.GetString("MediaStorage_Amazon_Must_start_with_a_number_or_letter", culture),
                            @"^[_a-z0-9\.\-]{3,255}$|^$" + "\t0\t"+AmazonI18N.ResourceManager.GetString("MediaStorage_Amazon_Must_be_between_3_and_255_characters_long", culture),
                            @".*\d{1,3}\.\d{1,3}\.\d{1,3}\.\d{1,3}.*" + "\t1\t"+AmazonI18N.ResourceManager.GetString("MediaStorage_Amazon_Must_not_be_formatted_as_an_IP_address", culture)
                        }},
                        new FeatureUnit() {Vim = "amazon_vim", DisplayName = AmazonI18N.ResourceManager.GetString("MediaStorage_Amazon_Access_Key_ID",culture), Tag = "cloud_amazon", Key = "name", KeyName = "AmazonUsername", Visibility="Visible", ValType = "string", IsRequiredOption = true, GuiType = "TextBox", DemoValue=AmazonI18N.ResourceManager.GetString("MediaStorage_Amazon_Access_Key_ID_Demo", culture)},
                        new FeatureUnit() {Vim = "amazon_vim", DisplayName = AmazonI18N.ResourceManager.GetString("MediaStorage_Amazon_Secret_Access_Key",culture), Tag = "cloud_amazon", Key = "secret", KeyName = "AmazonAPIKey", Visibility="Visible", ValType = "string", IsRequiredOption = true, GuiType = "PasswordBox"},
                        new FeatureUnit() {Vim = "amazon_vim", DisplayName = AmazonI18N.ResourceManager.GetString("MediaStorage_Amazon_Storage_Region",culture), Tag = "cloud_amazon", Value = "region", Key = "region", KeyName = "AmazonRegion", Visibility="Visible", ValType = "string", GuiType = "ComboBox", ChildFeatures = new List<FeatureUnit>() {
                            new FeatureUnit() {Vim = "amazon_vim", DisplayName = AmazonI18N.ResourceManager.GetString("MediaStorage_Amazon_US_Standard",culture), Tag = "cloud_amazon_region", Value = "USSTANDARD".ToLower(CultureInfo.InvariantCulture), Key = "USSTANDARD".ToLower(CultureInfo.InvariantCulture), KeyName = "AmazonRegionUSStandard", Visibility="Collapsed", ValType = "string", GuiType = "TextBox"},
                            new FeatureUnit() {Vim = "amazon_vim", DisplayName = AmazonI18N.ResourceManager.GetString("MediaStorage_Amazon_US_West_Northern_California",culture), Tag = "cloud_amazon_region", Value = "USWEST".ToLower(CultureInfo.InvariantCulture), Key = "USWEST".ToLower(CultureInfo.InvariantCulture), KeyName = "AmazonRegionUSWest", Visibility="Collapsed", ValType = "string", GuiType = "TextBox"},
                            new FeatureUnit() {Vim = "amazon_vim", DisplayName = AmazonI18N.ResourceManager.GetString("MediaStorage_Amazon_EU_Ireland",culture), Tag = "cloud_amazon_region", Key = "EU".ToLower(CultureInfo.InvariantCulture), Value = "EU".ToLower(CultureInfo.InvariantCulture), KeyName = "AmazonRegionEU", Visibility="Collapsed", ValType = "string", GuiType = "TextBox"},
                            new FeatureUnit() {Vim = "amazon_vim", DisplayName = AmazonI18N.ResourceManager.GetString("MediaStorage_Amazon_EU_Frankfurt",culture), Tag = "cloud_amazon_region", Key = "frankfurt".ToLower(CultureInfo.InvariantCulture), Value = "frankfurt".ToLower(CultureInfo.InvariantCulture), KeyName = "AmazonRegionEUFrankfurt", Visibility="Collapsed", ValType = "string", GuiType = "TextBox"},
                            new FeatureUnit() {Vim = "amazon_vim", DisplayName = AmazonI18N.ResourceManager.GetString("MediaStorage_Amazon_Asia_Pacific_Singapore",culture), Tag = "cloud_amazon_region", Key = "APAC".ToLower(CultureInfo.InvariantCulture), Value = "APAC".ToLower(CultureInfo.InvariantCulture), KeyName = "AmazonRegionAPAC", Visibility="Collapsed", ValType = "string", GuiType = "TextBox"},
                            new FeatureUnit() {Vim = "amazon_vim", DisplayName = AmazonI18N.ResourceManager.GetString("MediaStorage_Amazon_Asia_Pacific_Tokyo",culture), Tag = "cloud_amazon_region", Key = "TOKYO".ToLower(CultureInfo.InvariantCulture), Value = "TOKYO".ToLower(CultureInfo.InvariantCulture), KeyName = "AmazonRegionTokyo", Visibility="Collapsed", ValType = "string", GuiType = "TextBox"},
                            new FeatureUnit() {Vim = "amazon_vim", DisplayName = AmazonI18N.ResourceManager.GetString("MediaStorage_Amazon_Asia_Pacific_Sydney", culture), Tag = "cloud_amazon_region", Key = "SYDNEY".ToLower(CultureInfo.InvariantCulture), Value = "SYDNEY".ToLower(CultureInfo.InvariantCulture), KeyName = "AmazonRegionSydney", Visibility="Collapsed", ValType = "string", GuiType = "TextBox"},
                            new FeatureUnit() {Vim = "amazon_vim", DisplayName = AmazonI18N.ResourceManager.GetString("MediaStorage_Amazon_US_West_Oregon", culture), Tag = "cloud_amazon_region", Value = "OREGON".ToLower(CultureInfo.InvariantCulture), Key = "OREGON".ToLower(CultureInfo.InvariantCulture), KeyName = "AmazonRegionUSWestOregon", Visibility="Collapsed", ValType = "string", GuiType = "TextBox"},
                            new FeatureUnit() {Vim = "amazon_vim", DisplayName = AmazonI18N.ResourceManager.GetString("MediaStorage_Amazon_South_America_Saopaulo", culture), Tag = "cloud_amazon_region", Key = "SAOPAULO".ToLower(CultureInfo.InvariantCulture), Value = "SAOPAULO".ToLower(CultureInfo.InvariantCulture), KeyName = "AmazonRegionSaopaulo", Visibility="Collapsed", ValType = "string", GuiType = "TextBox"},
                        }},
                        new FeatureUnit() {Vim = "amazon_vim", DisplayName = AmazonI18N.ResourceManager.GetString("MediaStorage_Amazon_Proxy", culture), Tag = "cloud_amazon_Proxy".ToLower(CultureInfo.InvariantCulture), Key = "Amazon_Proxy".ToLower(CultureInfo.InvariantCulture), Value = "cloud_amazon_setting".ToLower(CultureInfo.InvariantCulture),  KeyName = "AmazonProxySetting", Visibility="Visible", ValType = "string", GuiType = "CheckBox", ChildFeatures = new List<FeatureUnit>(){
                            new FeatureUnit() {Vim = "amazon_vim", DisplayName = AmazonI18N.ResourceManager.GetString("MediaStorage_Amazon_Proxy_Host", culture), Tag = "cloud_amazon_setting".ToLower(CultureInfo.InvariantCulture), Key = "ProxyIp".ToLower(CultureInfo.InvariantCulture), KeyName = "AmazonProxyHost", Visibility="Collapsed", ValType = "string", IsRequiredOption = true, GuiType = "TextBox"},
                            new FeatureUnit() {Vim = "amazon_vim", DisplayName = AmazonI18N.ResourceManager.GetString("MediaStorage_Amazon_Proxy_Port", culture), Tag = "cloud_amazon_setting".ToLower(CultureInfo.InvariantCulture), Key = "ProxyPort".ToLower(CultureInfo.InvariantCulture), KeyName = "AmazonProxyPort", Visibility="Collapsed", ValType = "string", IsRequiredOption = true, GuiType = "TextBox"},
                            new FeatureUnit() {Vim = "amazon_vim", DisplayName = AmazonI18N.ResourceManager.GetString("MediaStorage_Amazon_Proxy_Username", culture), Tag = "cloud_amazon_setting".ToLower(CultureInfo.InvariantCulture), Key = "ProxyUsername".ToLower(CultureInfo.InvariantCulture), KeyName = "AmazonProxyUsername", Visibility="Collapsed", ValType = "string", IsRequiredOption = false, GuiType = "TextBox", CanNullOrEmpty = "true"},
                            new FeatureUnit() {Vim = "amazon_vim", DisplayName = AmazonI18N.ResourceManager.GetString("MediaStorage_Amazon_Proxy_Password", culture), Tag = "cloud_amazon_setting".ToLower(CultureInfo.InvariantCulture), Key = "ProxyPasswordSecret".ToLower(CultureInfo.InvariantCulture), KeyName = "AmazonProxyPasswordSecret", Visibility="Collapsed", ValType = "string", IsRequiredOption = false, GuiType = "PasswordBox", CanNullOrEmpty = "true"},
                        }},
                        new FeatureUnit() {Vim = "amazon_vim", DisplayName = AmazonI18N.ResourceManager.GetString("MediaStorage_Amazon_Advanced",culture), Tag = "cloud_amazon", Key = "advanced".ToLower(CultureInfo.InvariantCulture), Value = "amazon_vim_ExtendedParameters",  KeyName = "AmazonAdvanced", Visibility="Visible", ValType = "string", GuiType = "CheckBox", ChildFeatures = new List<FeatureUnit>(){
                            new FeatureUnit() {Vim = "amazon_vim", DisplayName = AmazonI18N.ResourceManager.GetString("MediaStorage_Amazon_Extended_Parameters",culture), Tag = "amazon_vim_ExtendedParameters", Key = "ExtendedParameters".ToLower(CultureInfo.InvariantCulture), KeyName = "AmazonExtendedParameters", Visibility="Collapsed", ValType = "string", GuiType = "TextArea", CanNullOrEmpty = "true"}}}
            };

            Add(sf);
        }

        [SuppressMessage("FxCopCustomRules", "C100007:SpellCheckStringValues", MessageId = "frankfurt")]
        protected override void GenerateConnectorGUIFeatureUnit(CultureInfo culture)
        {
            StorageFeature sf = new StorageFeature();
            StorageType type = new StorageType();
            type.Value = "Amazon";
            type.Display = AmazonI18N.ResourceManager.GetString("MediaStorage_Amazon_Amazon_S3", culture);
            type.Index = 401;
            type.Vim = new List<string>() { "amazon_vim" };
            sf.Type = type;
            sf.Description = AmazonI18N.ResourceManager.GetString("MediaStorage_Connector_Amazon_Configure_your_bucket_name_Access_Key_ID_and_Secret_Access_Key", culture);
            sf.Features = new List<FeatureUnit> {
                        new FeatureUnit() {IsRequiredOption = true, Vim = "amazon_vim", DisplayName = AmazonI18N.ResourceManager.GetString("MediaStorage_Connector_Amazon_Access_Key_ID",culture), Tag = "cloud_amazon", Key = "name", KeyName = "AmazonUsername", Visibility="Visible", ValType = "string", GuiType = "TextBox"},
                        new FeatureUnit() {IsRequiredOption = true, Vim = "amazon_vim", DisplayName = AmazonI18N.ResourceManager.GetString("MediaStorage_Connector_Amazon_Bucket_Name",culture), Tag = "cloud_amazon", Key = "bucketName", KeyName = "AmazonBucketName", Visibility="Visible", ValType = "string", GuiType = "TextBox", FeatureFlag=(int)(FeatureUnitFlag.CloudContainer|FeatureUnitFlag.Path) ,  CanModifi = false, ValidateRegPats = new List<string>(){
                            @"^[_a-z0-9\.\-]+$|^$" + "\t0\t"+AmazonI18N.ResourceManager.GetString("MediaStorage_Connector_Amazon_Can_contain_lowercase_letters_numbers_periods_underscores_and_dashes", culture),
                            "^[a-z0-9]+.*$|^$" + "\t0\t"+AmazonI18N.ResourceManager.GetString("MediaStorage_Connector_Amazon_Must_start_with_a_number_or_letter", culture),
                            @"^[_a-z0-9\.\-]{3,255}$|^$" + "\t0\t"+AmazonI18N.ResourceManager.GetString("MediaStorage_Connector_Amazon_Must_be_between_3_and_255_characters_long", culture),
                            @".*\d{1,3}\.\d{1,3}\.\d{1,3}\.\d{1,3}.*" + "\t1\t"+AmazonI18N.ResourceManager.GetString("MediaStorage_Connector_Amazon_Must_not_be_formatted_as_an_IP_address", culture)
                        }},
                        new FeatureUnit() {IsRequiredOption = true, Vim = "amazon_vim", DisplayName = AmazonI18N.ResourceManager.GetString("MediaStorage_Connector_Amazon_Secret_Access_Key",culture), Tag = "cloud_amazon", Key = "secret", KeyName = "AmazonAPIKey", Visibility="Visible", ValType = "string", GuiType = "PasswordBox"},
                        new FeatureUnit() {IsRequiredOption = true, Vim = "amazon_vim", DisplayName = AmazonI18N.ResourceManager.GetString("MediaStorage_Connector_Amazon_Storage_Region",culture), Tag = "cloud_amazon", Value = "region", Key = "region", KeyName = "AmazonRegion", Visibility="Visible", ValType = "string", GuiType = "ComboBox", ChildFeatures = new List<FeatureUnit>() {
                            new FeatureUnit() {IsRequiredOption = true, Vim = "amazon_vim", DisplayName = AmazonI18N.ResourceManager.GetString("MediaStorage_Connector_Amazon_US_Standard",culture), Tag = "cloud_amazon_region", Value = "USSTANDARD".ToLower(CultureInfo.InvariantCulture), Key = "USSTANDARD".ToLower(CultureInfo.InvariantCulture), KeyName = "AmazonRegionUSStandard", Visibility="Collapsed", ValType = "string", GuiType = "TextBox"},
                            new FeatureUnit() {IsRequiredOption = true, Vim = "amazon_vim", DisplayName = AmazonI18N.ResourceManager.GetString("MediaStorage_Connector_Amazon_US_West_Northern_California",culture), Tag = "cloud_amazon_region", Value = "USWEST".ToLower(CultureInfo.InvariantCulture), Key = "USWEST".ToLower(CultureInfo.InvariantCulture), KeyName = "AmazonRegionUSWest", Visibility="Collapsed", ValType = "string", GuiType = "TextBox"},
                            new FeatureUnit() {IsRequiredOption = true, Vim = "amazon_vim", DisplayName = AmazonI18N.ResourceManager.GetString("MediaStorage_Connector_Amazon_EU_Ireland",culture), Tag = "cloud_amazon_region", Key = "EU".ToLower(CultureInfo.InvariantCulture), Value = "EU".ToLower(CultureInfo.InvariantCulture), KeyName = "AmazonRegionEU", Visibility="Collapsed", ValType = "string", GuiType = "TextBox"},
                            new FeatureUnit() {IsRequiredOption = true, Vim = "amazon_vim", DisplayName = AmazonI18N.ResourceManager.GetString("MediaStorage_Connector_Amazon_EU_Frankfurt",culture), Tag = "cloud_amazon_region", Key = "frankfurt".ToLower(CultureInfo.InvariantCulture), Value = "frankfurt".ToLower(CultureInfo.InvariantCulture), KeyName = "AmazonRegionEUFrankfurt", Visibility="Collapsed", ValType = "string", GuiType = "TextBox"},
                            new FeatureUnit() {IsRequiredOption = true, Vim = "amazon_vim", DisplayName = AmazonI18N.ResourceManager.GetString("MediaStorage_Connector_Amazon_Asia_Pacific_Singapore",culture), Tag = "cloud_amazon_region", Key = "APAC".ToLower(CultureInfo.InvariantCulture), Value = "APAC".ToLower(CultureInfo.InvariantCulture), KeyName = "AmazonRegionAPAC", Visibility="Collapsed", ValType = "string", GuiType = "TextBox"},
                            new FeatureUnit() {IsRequiredOption = true, Vim = "amazon_vim", DisplayName = AmazonI18N.ResourceManager.GetString("MediaStorage_Connector_Amazon_Asia_Pacific_Tokyo",culture), Tag = "cloud_amazon_region", Key = "TOKYO".ToLower(CultureInfo.InvariantCulture), Value = "TOKYO".ToLower(CultureInfo.InvariantCulture), KeyName = "AmazonRegionTokyo", Visibility="Collapsed", ValType = "string", GuiType = "TextBox"},
                            new FeatureUnit() {IsRequiredOption = true, Vim = "amazon_vim", DisplayName = AmazonI18N.ResourceManager.GetString("MediaStorage_Connector_Amazon_Asia_Pacific_Sydney", culture), Tag = "cloud_amazon_region", Key = "SYDNEY".ToLower(CultureInfo.InvariantCulture), Value = "SYDNEY".ToLower(CultureInfo.InvariantCulture), KeyName = "AmazonRegionSydney", Visibility="Visible", ValType = "string", GuiType = "TextBox"},
                            new FeatureUnit() {IsRequiredOption = true, Vim = "amazon_vim", DisplayName = AmazonI18N.ResourceManager.GetString("MediaStorage_Connector_Amazon_US_West_Oregon", culture), Tag = "cloud_amazon_region", Value = "OREGON".ToLower(CultureInfo.InvariantCulture), Key = "OREGON".ToLower(CultureInfo.InvariantCulture), KeyName = "AmazonRegionUSWestOregon", Visibility="Visible", ValType = "string", GuiType = "TextBox"},
                            new FeatureUnit() {IsRequiredOption = true, Vim = "amazon_vim", DisplayName = AmazonI18N.ResourceManager.GetString("MediaStorage_Connector_Amazon_South_America_Saopaulo", culture), Tag = "cloud_amazon_region", Key = "SAOPAULO".ToLower(CultureInfo.InvariantCulture), Value = "SAOPAULO".ToLower(CultureInfo.InvariantCulture), KeyName = "AmazonRegionSaopaulo", Visibility="Visible", ValType = "string", GuiType = "TextBox"},
                        }},
                        new FeatureUnit() {Vim = "amazon_vim", DisplayName = AmazonI18N.ResourceManager.GetString("MediaStorage_Connector_Amazon_Proxy", culture), Tag = "cloud_amazon_Proxy".ToLower(CultureInfo.InvariantCulture), Key = "Amazon_Proxy".ToLower(CultureInfo.InvariantCulture), Value = "cloud_amazon_setting".ToLower(CultureInfo.InvariantCulture),  KeyName = "AmazonProxySetting", Visibility="Visible", ValType = "string", GuiType = "CheckBox", ChildFeatures = new List<FeatureUnit>(){
                            new FeatureUnit() {Vim = "amazon_vim", DisplayName = AmazonI18N.ResourceManager.GetString("MediaStorage_Connector_Amazon_Proxy_Host", culture), Tag = "cloud_amazon_setting".ToLower(CultureInfo.InvariantCulture), Key = "ProxyIp".ToLower(CultureInfo.InvariantCulture), KeyName = "AmazonProxyHost", Visibility="Collapsed", ValType = "string", IsRequiredOption = true, GuiType = "TextBox"},
                            new FeatureUnit() {Vim = "amazon_vim", DisplayName = AmazonI18N.ResourceManager.GetString("MediaStorage_Connector_Amazon_Proxy_Port", culture), Tag = "cloud_amazon_setting".ToLower(CultureInfo.InvariantCulture), Key = "ProxyPort".ToLower(CultureInfo.InvariantCulture), KeyName = "AmazonProxyPort", Visibility="Collapsed", ValType = "string", IsRequiredOption = true, GuiType = "TextBox",ValidateRegPats = new List<string>(){
                            @"(^[0-9]\d{0,3}$)|(^[1-5]\d{4}$)|(^6[0-4]\d{3}$)|(^65[0-4]\d{2}$)|(^655[0-2]\d$)|(^6553[0-5]$)"+ "\t0\t"+AmazonI18N.ResourceManager.GetString("MediaStorage_Connector_Amazon_Must_be_a_number_between_0_and_65535", culture)
                            }},
                            new FeatureUnit() {Vim = "amazon_vim", DisplayName = AmazonI18N.ResourceManager.GetString("MediaStorage_Connector_Amazon_Proxy_Username", culture), Tag = "cloud_amazon_setting".ToLower(CultureInfo.InvariantCulture), Key = "ProxyUsername".ToLower(CultureInfo.InvariantCulture), KeyName = "AmazonProxyUsername", Visibility="Collapsed", ValType = "string", IsRequiredOption = false, GuiType = "TextBox", CanNullOrEmpty = "true"},
                            new FeatureUnit() {Vim = "amazon_vim", DisplayName = AmazonI18N.ResourceManager.GetString("MediaStorage_Connector_Amazon_Proxy_Password", culture), Tag = "cloud_amazon_setting".ToLower(CultureInfo.InvariantCulture), Key = "ProxyPasswordSecret".ToLower(CultureInfo.InvariantCulture), KeyName = "AmazonProxyPasswordSecret", Visibility="Collapsed", ValType = "string", IsRequiredOption = false, GuiType = "PasswordBox", CanNullOrEmpty = "true"},
                        }},
                        new FeatureUnit() {IsRequiredOption = true, Vim = "amazon_vim", DisplayName = AmazonI18N.ResourceManager.GetString("MediaStorage_Connector_Amazon_Advanced",culture), Tag = "cloud_amazon", Key = "advanced".ToLower(CultureInfo.InvariantCulture), Value = "amazon_vim_ExtendedParameters",  KeyName = "AmazonWithParamsExtension", Visibility="Visible", ValType = "string", GuiType = "CheckBox", ChildFeatures = new List<FeatureUnit>(){
                            new FeatureUnit() {IsRequiredOption = true, Vim = "amazon_vim", DisplayName = AmazonI18N.ResourceManager.GetString("MediaStorage_Connector_Amazon_Extended_Parameters",culture), Tag = "amazon_vim_ExtendedParameters", Key = "ExtendedParameters".ToLower(CultureInfo.InvariantCulture), KeyName = "AmazonExtendParams", Visibility="Collapsed", ValType = "string", GuiType = "TextArea", CanNullOrEmpty = "true"}}}
            };

            Add(sf);
        }
        #endregion

        /// <summary>
        /// 供VIM调用, 获取相应的Feature
        /// </summary>
        private static readonly Dictionary<string, AmazonFeature> instances = new Dictionary<string, AmazonFeature>();
        private static Object locker = new Object();
        public static AmazonFeature Getstances(int type, string culture = "en")
        {
            lock (locker)
            {
                if (!instances.ContainsKey(type + culture))
                {
                    AmazonFeature amazon = new AmazonFeature(type, culture);
                    foreach (var feature in amazon.FeatureObjs)
                    {
                        feature.AdvancedOptions.AddRange(new List<string>()
                            {
                                "RetryInterval=30000",
                                "RetryCount=6",
                                "CustomizedMetadata={[testKey1,testValue1],[testKey2,testValue2],[testKey3,testValue3]}}",
                                "CustomizedMode=Close",
                                "CustomizedMode=SupportAll",
                                "CustomizedMode=DocAveOnly",
                                "CustomizedMode=CustomizedOnly"
                            });
                    }
                    instances[type + culture] = amazon;
                    return amazon;
                }
                else
                {
                    return instances[type + culture];
                }
            }
        }
    }
}
