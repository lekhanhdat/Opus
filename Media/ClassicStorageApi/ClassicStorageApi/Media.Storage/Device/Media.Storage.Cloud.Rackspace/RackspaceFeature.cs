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




namespace AvePoint.Media.ClassicStorage.Cloud.Rackspace
{
    #region using directives
    using System;
    using System.Collections.Generic;
    using System.Globalization;
    using System.Text;
    using AvePoint.Media.ClassicStorage.Cloud.Common;
    using AvePoint.Media.ClassicStorage.Resources.RackspaceI18N;
    #endregion

    class RackspaceFeature : StorageFeature
    {
        #region RackspaceFeature 自己负责初始化的部分, 在Application生存周期内只会初始化一次, 外部不需要知道的逻辑
        /// <summary>
        /// 私有构造函数， 此类不允许在外部实例化。 <see cref="RackspaceFeature"/> class.
        /// </summary>
        private RackspaceFeature(int type,string culture)
        {
            this.Init(type,culture);
        }

        protected override void GenerateSingleTypeFeatureUnit(CultureInfo culture)
        {
            /*StorageFeature sf = new StorageFeature();
            StorageType type = new StorageType();
            type.Value = "Rackspace";
            type.Display = "Rackspace Cloud File";
            type.Index = 402;
            type.Vim = new List<string>() { "rackspace_vim" };
            sf.Type = type;
            sf.Type.DefaultXris.Add("DOCAVE-XAM://RACKSPACE_VIM?CDN=FALSE".ToLower(CultureInfo.InvariantCulture));
            sf.Description = RackspaceI18N.ResourceManager.GetString("Configure_your_container_name_storage_account_and_API_key", culture);
            sf.Features = new List<FeatureUnit> {
                         new FeatureUnit() {Vim = "rackspace_vim", DisplayName = "Container Name", Tag = "cloud_rackspace", Key = "containerName".ToLower(CultureInfo.InvariantCulture), KeyName = "RackspaceContainerName", Visibility="Visible", ValType = "string", GuiType = "TextBox",  ValidateRegPats = new List<string>(){
                            @"^[^\/]{1,256}$" + "\t0\t" + RackspaceI18N.ResourceManager.GetString("Container_names_cannot_contain_a_forward_slash__and_must_be_less_than_256_bytes_in_length", culture),
                        }},
                        new FeatureUnit() {Vim = "rackspace_vim", DisplayName = "Username", Tag = "cloud_rackspace", Key = "name", KeyName = "RackspaceUsername", Visibility="Visible", ValType = "string", GuiType = "TextBox"},
                        new FeatureUnit() {Vim = "rackspace_vim", DisplayName = "CDN Enabled", Tag = "cloud_rackspace", Value="cdn_cloud_rackspace", Key = "cdn", KeyName = "RackspaceCDN", Visibility="Visible", ValType = "string", GuiType = "CheckBox"}
                    };
            Add(sf);*/
        }

        protected override void GenerateDocAveGUIFeatureUnit(CultureInfo culture)
        {
            //StorageFeature sf = CommCloudFeature.Getstances(culture.ToString());
            StorageFeature sf = new StorageFeature();
            StorageType type = new StorageType();
            type.Value = "Rackspace";
            type.Display = "Rackspace Cloud File";
            type.Index = 402;
            type.Vim = new List<string>() { "rackspace_vim" };
            sf.Type = type;
            sf.Type.DefaultXris.Add("DOCAVE-XAM://RACKSPACE_VIM?CDN=FALSE".ToLower(CultureInfo.InvariantCulture));
            //sf.Description = RackspaceI18N.ResourceManager.GetString("MediaStorage_Rackspace_Configure_your_container_name_storage_account_and_API_key", culture);
            sf.Features = new List<FeatureUnit>
            {
                new FeatureUnit()
                {
                    Vim = "rackspace_vim",
                    DisplayName =
                        RackspaceI18N.ResourceManager.GetString("MediaStorage_RackSpace_Container_Name", culture),
                    Tag = "cloud_rackspace",
                    Key = "containerName".ToLower(CultureInfo.InvariantCulture),
                    KeyName = "RackspaceContainerName",
                    Visibility = "Visible",
                    ValType = "string",
                    IsRequiredOption = true,
                    GuiType = "TextBox",
                    DemoValue = "DocAve",
                    ValidateRegPats = new List<string>()
                    {
                        @"^[^\/]{0,256}$" + "\t0\t" +
                        RackspaceI18N.ResourceManager.GetString(
                            "MediaStorage_RackSpace_Container_names_cannot_contain_a_forward_slash_and_must_be_less_than_256_bytes_in_length",
                            culture),
                    }
                },
                new FeatureUnit()
                {
                    Vim = "rackspace_vim",
                    DisplayName = RackspaceI18N.ResourceManager.GetString("MediaStorage_RackSpace_Username", culture),
                    Tag = "cloud_rackspace",
                    Key = "name",
                    KeyName = "RackspaceUsername",
                    Visibility = "Visible",
                    ValType = "string",
                    IsRequiredOption = true,
                    GuiType = "TextBox",
                    DemoValue = @"Username"
                },
                new FeatureUnit()
                {
                    Vim = "rackspace_vim",
                    DisplayName = RackspaceI18N.ResourceManager.GetString("MediaStorage_RackSpace_API_Key", culture),
                    Tag = "cloud_rackspace",
                    Key = "secret",
                    KeyName = "RackspaceAPIKey",
                    Visibility = "Visible",
                    ValType = "string",
                    IsRequiredOption = true,
                    GuiType = "PasswordBox"
                },
                new FeatureUnit()
                {
                    Vim = "rackspace_vim",
                    DisplayName = RackspaceI18N.ResourceManager.GetString("MediaStorage_RackSpace_CDN_Enabled", culture),
                    Tag = "cloud_rackspace",
                    Value = "cdn_cloud_rackspace",
                    Key = "cdn",
                    KeyName = "RackspaceCDN",
                    Visibility = "Visible",
                    ValType = "string",
                    GuiType = "CheckBox"
                },
                new FeatureUnit()
                {
                    Vim = "rackspace_vim",
                    DisplayName = RackspaceI18N.ResourceManager.GetString("MediaStorage_RackSpace_Advanced", culture),
                    Tag = "cloud_rackspace",
                    Key = "advanced".ToLower(CultureInfo.InvariantCulture),
                    Value = "rackspace_vim_ExtendedParameters",
                    KeyName = "RackWithParamsExtension",
                    Visibility = "Visible",
                    ValType = "string",
                    GuiType = "CheckBox",
                    ChildFeatures = new List<FeatureUnit>()
                    {
                        new FeatureUnit()
                        {
                            Vim = "rackspace_vim",
                            DisplayName =
                                RackspaceI18N.ResourceManager.GetString("MediaStorage_RackSpace_Extended_Parameters",
                                    culture),
                            Tag = "rackspace_vim_ExtendedParameters",
                            Key = "ExtendedParameters".ToLower(CultureInfo.InvariantCulture),
                            KeyName = "RackExtendParams",
                            Visibility = "Collapsed",
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
            type.Value = "Rackspace";
            type.Display = RackspaceI18N.ResourceManager.GetString("MediaStorage_RackSpace_Cloud_Files", culture);
            type.Index = 402;
            type.Vim = new List<string>() { "rackspace_vim" };
            sf.Type = type;
            sf.Description = RackspaceI18N.ResourceManager.GetString("MediaStorage_RackSpace_Connector_Rackspace_Description", culture);
            sf.Features = new List<FeatureUnit> {
                        new FeatureUnit() {Vim = "rackspace_vim", DisplayName = RackspaceI18N.ResourceManager.GetString("MediaStorage_RackSpace_Container_Name", culture), Tag = "cloud_rackspace", Key = "containerName", KeyName = "RackspaceContainerName", Visibility="Visible", ValType = "string", GuiType = "TextBox", FeatureFlag=(int)(FeatureUnitFlag.CloudContainer|FeatureUnitFlag.Path), CanModifi = false,  ValidateRegPats = new List<string>(){
                            @"^[^\/]{0,256}$" + "\t0\t" + RackspaceI18N.ResourceManager.GetString("MediaStorage_RackSpace_Container_names_cannot_contain_a_forward_slash_and_must_be_less_than_256_bytes_in_length", culture),
                        }},
                        new FeatureUnit() {Vim = "rackspace_vim", DisplayName = RackspaceI18N.ResourceManager.GetString("MediaStorage_RackSpace_Username", culture), Tag = "cloud_rackspace", Key = "name", KeyName = "RackspaceUsername", Visibility="Visible", ValType = "string", GuiType = "TextBox"},
                        new FeatureUnit() {Vim = "rackspace_vim", DisplayName = RackspaceI18N.ResourceManager.GetString("MediaStorage_RackSpace_API_Key", culture), Tag = "cloud_rackspace", Key = "secret", KeyName = "RackspaceAPIKey", Visibility="Visible", ValType = "string", GuiType = "PasswordBox"},
                        new FeatureUnit() {Vim = "rackspace_vim", DisplayName = RackspaceI18N.ResourceManager.GetString("MediaStorage_RackSpace_CDN_Enabled", culture), Tag = "cloud_rackspace", Value="cdn_cloud_rackspace", Key = "cdn", KeyName = "RackspaceCDN", Visibility="Visible", ValType = "string", GuiType = "CheckBox"},
                        new FeatureUnit() {Vim = "rackspace_vim", DisplayName = RackspaceI18N.ResourceManager.GetString("MediaStorage_RackSpace_Advanced", culture), Tag = "cloud_rackspace_advanced", Key = "advanced".ToLower(CultureInfo.InvariantCulture), Value = "rackspace_vim_ExtendedParameters",  KeyName = "RackSpaceAdvanced", Visibility="Visible", ValType = "string", GuiType = "CheckBox", ChildFeatures = new List<FeatureUnit>(){
                            new FeatureUnit() {Vim = "rackspace_vim", DisplayName = RackspaceI18N.ResourceManager.GetString("MediaStorage_RackSpace_Extended_Parameters", culture), Tag = "rackspace_vim_ExtendedParameters", Key = "ExtendedParameters".ToLower(CultureInfo.InvariantCulture), KeyName = "RackExtendParams", Visibility="Collapsed", ValType = "string", GuiType = "TextArea", CanNullOrEmpty = "true"}}}
            };
            Add(sf);
        }
        #endregion

        /// <summary>
        /// 供VIM调用, 获取相应的Feature
        /// </summary>
        private static readonly Dictionary<string, RackspaceFeature> instances = new Dictionary<string, RackspaceFeature>();

        public static RackspaceFeature Getstances(int type, string culture = "en")
        {
            if (!instances.ContainsKey(type + culture))
            {
                RackspaceFeature rackspace = new RackspaceFeature(type, culture);
                foreach (var feature in rackspace.FeatureObjs)
                {
                    feature.AdvancedOptions.AddRange(new List<string>()
                    {
                        "RetryInterval=30",
                        "RetryCount=6",
                        "CustomizedMetadata={[testKey1,testValue1],[testKey2,testValue2],[testKey3,testValue3]}}",
                        "CustomizedMode=Close",
                        "CustomizedMode=SupportAll",
                        "CustomizedMode=DocAveOnly",
                        "CustomizedMode=CustomizedOnly"
                    });
                }
                instances[type + culture] = rackspace;
                return rackspace;
            }
            else
            {
                return instances[type + culture];
            }
        }
    }
}
