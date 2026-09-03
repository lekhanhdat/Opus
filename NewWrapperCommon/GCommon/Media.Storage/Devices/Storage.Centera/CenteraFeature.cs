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



namespace AvePoint.Media.Storage.Centera
{
    #region using directives
    using System;
    using System.Collections.Generic;
    using System.Globalization;
    using AvePoint.Media.Storage.Resources.CenteraI18N;
    #endregion

    /// <summary>
    /// 此类用于描述EMC Centera类型的存储介质的特性
    /// </summary>
    /// 
    class CenteraFeature : StorageFeature
    {
        private static Object locker = new Object();
        private static readonly Dictionary<String, CenteraFeature> instances = new Dictionary<String, CenteraFeature>();

        #region CenteraFeature 自己负责初始化的部分, 在Application生存周期内只会初始化一次, 外部不需要知道的逻辑
        /// <summary>
        /// 私有构造函数， 此类不允许在外部实例化class。 <see cref="CenteraFeature"/> 
        /// </summary>
        private CenteraFeature(Int32 type, String culture)
        {
            this.Init(type, culture);
        }

        protected override void GenerateSingleTypeFeatureUnit(CultureInfo culture)
        {
            var centera = new StorageType();
            var feature = new StorageFeature();
            centera.Value = "EMCCentera";
            centera.Display = CenteraI18N.ResourceManager.GetString("MediaStorage_Centera_EMC_Centera", culture);
            centera.Index = 3;
            centera.Vim = new List<String>() { "centera_vim" };
            centera.IsSupportMovableRetention = false;
            feature.Type = centera;
            feature.Type.DefaultXris = new List<String>() { "DOCAVE-XAM://".ToLower(CultureInfo.InvariantCulture) + "centera_vim?authType=n/sAuth".ToLower(CultureInfo.InvariantCulture) };
            feature.IsNeedSpaceThreshold = true;
            feature.IsObjectType = true;
            feature.ProgressForeground = new FeatureColor(255, 190, 190, 190);
            feature.Features = new List<FeatureUnit>
            { 
                new FeatureUnit() {Vim = "centera_vim", DisplayName = CenteraI18N.ResourceManager.GetString("MediaStorage_Centera_Centera_Cluster_Address",culture), Tag = "centera_address", Key="address", KeyName="CenteraClusterAddress", Visibility="Visible", ValType="string", GuiType="TextBox", DemoValue=@"10.2.6.179,10.2.6.180"},
                new FeatureUnit() {Vim = "centera_vim",DisplayName = CenteraI18N.ResourceManager.GetString("MediaStorage_Centera_Authentication",culture), Tag = "ComBox_CenteraAuthType", Value = "authType".ToLower(CultureInfo.InvariantCulture), Key = "authType".ToLower(CultureInfo.InvariantCulture), KeyName="CenteraAuthentication", Visibility="Visible", ValType="string", GuiType="ComboBox", ChildFeatures = new List<FeatureUnit>() { 
                    new FeatureUnit() {Vim = "centera_vim",DisplayName = CenteraI18N.ResourceManager.GetString("MediaStorage_Centera_Name_Secret_Authentication",culture), Value = "n/sAuth".ToLower(CultureInfo.InvariantCulture), Tag = "centera_auth_ns", Key = "n/sAuth".ToLower(CultureInfo.InvariantCulture), Visibility="Visible", GuiType="ComboBoxItem", ValType="string", ChildFeatures = new List<FeatureUnit> {
                        new FeatureUnit() {Vim = "centera_vim",DisplayName = CenteraI18N.ResourceManager.GetString("MediaStorage_Centera_Name",culture), ValType = "string", Tag = "centera_auth_ns", Key = "name", Visibility="Visible", KeyName = "AuthName", DemoValue = CenteraI18N.ResourceManager.GetString("MediaStorage_Centera_Name_Demo", culture), GuiType = "TextBox"},
                        new FeatureUnit() {Vim = "centera_vim",DisplayName = CenteraI18N.ResourceManager.GetString("MediaStorage_Centera_Secret",culture), ValType = "string", Tag = "centera_auth_ns", Key = "secret", Visibility="Visible", KeyName = "AuthSecret", GuiType = "PasswordBox"},
                    }},
                    new FeatureUnit() {Vim = "centera_vim",DisplayName = CenteraI18N.ResourceManager.GetString("MediaStorage_Centera_PEA_files_Authentication",culture), Value="pea", Tag = "centera_auth_pea", Key = "PAE".ToLower(CultureInfo.InvariantCulture), GuiType="ComboBoxItem", Visibility="Visible", ValType="string", ChildFeatures = new List<FeatureUnit> {
                        new FeatureUnit() {Vim = "centera_vim",DisplayName = CenteraI18N.ResourceManager.GetString("MediaStorage_Centera_PEA_file_Location",culture), Tag = "centera_auth_pea", Key = "PAEAuth".ToLower(CultureInfo.InvariantCulture), KeyName = "PEAFileLocation", Visibility="Collapsed", ValType = "string", DemoValue = @"\\server\c$\data\*.pea", GuiType = "TextBox"},
                        new FeatureUnit() {Vim = "centera_vim",DisplayName = CenteraI18N.ResourceManager.GetString("MediaStorage_Centera_Location_Username",culture), Tag = "centera_auth_pea", Key = "PAEU".ToLower(CultureInfo.InvariantCulture), KeyName = "LocationUsername", Visibility="Collapsed", ValType = "string", DemoValue = CenteraI18N.ResourceManager.GetString("MediaStorage_Centera_Location_Username_Demo", culture), GuiType = "TextBox"},
                        new FeatureUnit() {Vim = "centera_vim",DisplayName = CenteraI18N.ResourceManager.GetString("MediaStorage_Centera_Location_Password",culture), Tag = "centera_auth_pea", Key = "PAEPSECRET".ToLower(CultureInfo.InvariantCulture), KeyName = "LocationPassword", Visibility="Collapsed", ValType = "string", GuiType = "PasswordBox"},
                    }},
                    
                }}
            };
            this.Add(feature);
        }
        /// <summary>
        /// 实际初始化DocAve GUI Feature Unit的部分
        /// </summary>
        protected override void GenerateDocAveGUIFeatureUnit(CultureInfo culture)
        {
            StorageType centera = new StorageType();
            StorageFeature feature = new StorageFeature();
            centera.Value = "EMCCentera";
            centera.Display = CenteraI18N.ResourceManager.GetString("MediaStorage_Centera_EMC_Centera", culture);
            centera.Index = 3;
            centera.Vim = new List<String>() { "centera_vim" };
            centera.IsSupportMovableRetention = false;
            feature.Type = centera;
            feature.Type.DefaultXris = new List<String>() { "docave-xam://centera_vim?authType=n/sAuth".ToLower(CultureInfo.InvariantCulture) };
            feature.IsNeedSpaceThreshold = true;
            feature.IsObjectType = true;
            feature.ProgressForeground = new FeatureColor(255, 243, 190, 24);
            feature.Features = new List<FeatureUnit>
            { 
                new FeatureUnit() {Vim = "centera_vim", DisplayName = CenteraI18N.ResourceManager.GetString("MediaStorage_Centera_Centera_Cluster_Address",culture), Tag = "centera_address", Key="address", KeyName="CenteraClusterAddress", Visibility="Visible", ValType="string", IsRequiredOption = true, GuiType="TextBox", DemoValue=@"10.2.6.179,10.2.6.180"},
                new FeatureUnit() {Vim = "centera_vim",DisplayName = CenteraI18N.ResourceManager.GetString("MediaStorage_Centera_Authentication",culture), Tag = "ComBox_CenteraAuthType", Value = "authType".ToLower(CultureInfo.InvariantCulture), Key = "authType".ToLower(CultureInfo.InvariantCulture), KeyName="CenteraAuthentication", Visibility="Visible", ValType="string", GuiType="ComboBox", ChildFeatures = new List<FeatureUnit>() { 
                    new FeatureUnit() {Vim = "centera_vim",DisplayName = CenteraI18N.ResourceManager.GetString("MediaStorage_Centera_Name_Secret_Authentication",culture), Value = "n/sAuth".ToLower(CultureInfo.InvariantCulture), Tag = "centera_auth_ns", Key = "n/sAuth".ToLower(CultureInfo.InvariantCulture), Visibility="Visible", GuiType="ComboBoxItem", ValType="string", ChildFeatures = new List<FeatureUnit> {
                        new FeatureUnit() {Vim = "centera_vim",DisplayName =CenteraI18N.ResourceManager.GetString("MediaStorage_Centera_Name",culture), ValType = "string", Tag = "centera_auth_ns", Key = "name", Visibility="Visible", KeyName = "AuthName", DemoValue = CenteraI18N.ResourceManager.GetString("MediaStorage_Centera_Name_Demo", culture), IsRequiredOption = true, GuiType = "TextBox"},
                        new FeatureUnit() {Vim = "centera_vim",DisplayName = CenteraI18N.ResourceManager.GetString("MediaStorage_Centera_Secret",culture), ValType = "string", Tag = "centera_auth_ns", Key = "secret", Visibility="Visible", KeyName = "AuthSecret", IsRequiredOption = true, GuiType = "PasswordBox"},
                    }},
                    new FeatureUnit() {Vim = "centera_vim",DisplayName = CenteraI18N.ResourceManager.GetString("MediaStorage_Centera_PEA_files_Authentication",culture), Value="pea", Tag = "centera_auth_pea", Key = "PAE".ToLower(CultureInfo.InvariantCulture), GuiType="ComboBoxItem", Visibility="Visible", ValType="string", ChildFeatures = new List<FeatureUnit> {
                        new FeatureUnit() {Vim = "centera_vim",DisplayName =CenteraI18N.ResourceManager.GetString("MediaStorage_Centera_PEA_file_Location",culture), Tag = "centera_auth_pea", Key = "PAEAuth".ToLower(CultureInfo.InvariantCulture), KeyName = "PEAFileLocation", Visibility="Collapsed", ValType = "string", DemoValue = @"\\server\c$\data\*.pea", IsRequiredOption = true, GuiType = "TextBox"},
                        new FeatureUnit() {Vim = "centera_vim",DisplayName =CenteraI18N.ResourceManager.GetString("MediaStorage_Centera_Location_Username",culture), Tag = "centera_auth_pea", Key = "PAEU".ToLower(CultureInfo.InvariantCulture), KeyName = "LocationUsername", Visibility="Collapsed", ValType = "string", DemoValue = CenteraI18N.ResourceManager.GetString("MediaStorage_Centera_Location_Username_Demo", culture), IsRequiredOption = true, GuiType = "TextBox"},
                        new FeatureUnit() {Vim = "centera_vim",DisplayName =CenteraI18N.ResourceManager.GetString("MediaStorage_Centera_Location_Password",culture), Tag = "centera_auth_pea", Key = "PAEPSECRET".ToLower(CultureInfo.InvariantCulture), KeyName = "LocationPassword", Visibility="Collapsed", ValType = "string", IsRequiredOption = true, GuiType = "PasswordBox"},
                    }},
                }},
                new FeatureUnit() {Vim = "centera_vim", DisplayName = CenteraI18N.ResourceManager.GetString("MediaStorage_Centera_Advanced",culture), Tag = "centera_vim_advanced", Key = "advanced".ToLower(CultureInfo.InvariantCulture), Value = "centera_vim_ExtendedParameters",  KeyName = "CenteraAdvanced", Visibility="Visible", ValType = "string", GuiType = "CheckBox", ChildFeatures = new List<FeatureUnit>(){
                   new FeatureUnit() {Vim = "centera_vim", DisplayName = CenteraI18N.ResourceManager.GetString("MediaStorage_Centera_Extended_parameters",culture), Tag = "centera_vim_ExtendedParameters", Key = "ExtendedParameters".ToLower(CultureInfo.InvariantCulture), KeyName = "CenteraExtendedParameters", Visibility="Collapsed", ValType = "string", GuiType = "TextArea", CanNullOrEmpty = "true"}
                }},
            };
            this.Add(feature);
        }
        #endregion

        /// <summary>
        /// 供VIM调用, 获取相应的Feature
        /// </summary>
        public static CenteraFeature Getstances(Int32 type, String culture = "en")
        {
            lock (locker)
            {
                CenteraFeature centera;
                if (instances.ContainsKey(type + culture))
                {
                    centera = instances[type + culture];
                }
                else
                {
                    centera = new CenteraFeature(type, culture);
                    foreach (var feature in centera.FeatureObjs)
                    {
                        feature.AdvancedOptions.AddRange(new List<String>()
                        {
                          "RetentionDays=0", 
                        });
                    }
                    instances[type + culture] = centera;
                }
                return centera;
            }
        }
    }
}
