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

namespace AvePoint.Media.Storage.Cloud.OpenStack
{
    #region using directives

    using System;
    using System.Collections.Generic;
    using System.Diagnostics.CodeAnalysis;
    using System.Globalization;
    using AvePoint.Media.Storage.Resources.OpenStackI18N;
    #endregion

    class OpenStackFeature : StorageFeature
    {
        #region OpenStackFeature 自己负责初始化的部分, 在Application生存周期内只会初始化一次, 外部不需要知道的逻辑
        /// <summary>
        /// 私有构造函数， 此类不允许在外部实例化。 <see cref="OpenStackFeature"/> class.
        /// </summary>
        private OpenStackFeature(int type, string culture)
        {
            this.Init(type, culture);
        }

        [SuppressMessage("FxCopCustomRules", "C100007:SpellCheckStringValues", MessageId = "openstack")]
        [SuppressMessage("FxCopCustomRules", "C100007:SpellCheckStringValues", MessageId = "containername")]
        [SuppressMessage("FxCopCustomRules", "C100007:SpellCheckStringValues", MessageId = "Tenantname")]
        protected override void GenerateSingleTypeFeatureUnit(CultureInfo culture)
        {
            StorageFeature sf = new StorageFeature();
            StorageType type = new StorageType();
            type.Value = "OpenStack";
            type.Display = "OpenStack Object Storage";
            type.Index = 501;
            type.Vim = new List<string>() { "OpenStack_vim" };
            sf.Type = type;
            sf.Type.DefaultXris.Add("DOCAVE-XAM://OpenStack_VIM?CDN=FALSE".ToLower(CultureInfo.InvariantCulture));
            sf.Description = OpenStackI18N.ResourceManager.GetString("MediaStorage_OpenStack_Configure_your_container_name_storage_account_and_API_key", culture);
            sf.Features = new List<FeatureUnit> {
                         new FeatureUnit() {Vim = "openstack_vim", DisplayName = "Container name", Tag = "cloud_OpenStack", Key = "containername".ToLower(CultureInfo.InvariantCulture), KeyName = "OpenStackContainerName", Visibility="Visible", ValType = "string", GuiType = "TextBox",  ValidateRegPats = new List<string>(){
                            @"^[^\/]{1,256}$" + "\t0\t" + OpenStackI18N.ResourceManager.GetString("MediaStorage_OpenStack_Container_names_cannot_contain_a_forward_slash_and_must_be_less_than_256_bytes_in_length", culture),
                        }},
                        new FeatureUnit() {Vim = "openstack_vim", DisplayName = "Tenant name", Tag = "cloud_OpenStack", Key = "tenant", KeyName = "OpenStackTenantname", Visibility="Visible", ValType = "string", GuiType = "TextBox", CanNullOrEmpty = "true", IsRequiredOption = false},
                        new FeatureUnit() {Vim = "openstack_vim", DisplayName = "Username", Tag = "cloud_OpenStack", Key = "username", KeyName = "OpenStackUsername", Visibility="Visible", ValType = "string", GuiType = "TextBox"},
                        new FeatureUnit() {Vim = "openstack_vim", DisplayName = "Password", Tag = "cloud_OpenStack", Key = "secret", KeyName = "OpenStackPassword", Visibility="Visible", ValType = "string", GuiType = "PasswordBox"},
                        new FeatureUnit() {Vim = "openstack_vim", DisplayName = "CDN enabled", Tag = "cloud_OpenStack", Value="cdn_cloud_OpenStack", Key = "cdn", KeyName = "OpenStackCDN", Visibility="Visible", ValType = "string", GuiType = "CheckBox"}
                    };
            Add(sf);
        }

        [SuppressMessage("FxCopCustomRules", "C100007:SpellCheckStringValues", MessageId = "Tenantname")]
        [SuppressMessage("FxCopCustomRules", "C100007:SpellCheckStringValues", MessageId = "Tenantnamevalue")]
        [SuppressMessage("FxCopCustomRules", "C100007:SpellCheckStringValues", MessageId = "tenantidbtn")]
        [SuppressMessage("FxCopCustomRules", "C100007:SpellCheckStringValues", MessageId = "containername")]
        [SuppressMessage("FxCopCustomRules", "C100007:SpellCheckStringValues", MessageId = "tenantid")]
        [SuppressMessage("FxCopCustomRules", "C100007:SpellCheckStringValues", MessageId = "tenantname")]
        [SuppressMessage("FxCopCustomRules", "C100007:SpellCheckStringValues", MessageId = "defaulttenantnamebtn")]
        [SuppressMessage("FxCopCustomRules", "C100007:SpellCheckStringValues", MessageId = "Tenantid")]
        [SuppressMessage("FxCopCustomRules", "C100007:SpellCheckStringValues", MessageId = "ibmsso")]
        [SuppressMessage("FxCopCustomRules", "C100007:SpellCheckStringValues", MessageId = "Tenantidvalue")]
        [SuppressMessage("FxCopCustomRules", "C100007:SpellCheckStringValues", MessageId = "authenticationurl")]
        protected override void GenerateDocAveGUIFeatureUnit(CultureInfo culture)
        {
            StorageFeature sf = new StorageFeature();
            StorageType type = new StorageType();
            type.Value = "IBMSpectrumScaleObject";
            type.Display = OpenStackI18N.ResourceManager.GetString("MediaStorage_OpenStack_Name", culture); 
            type.Index = 502;
            type.Vim = new List<string>() { "ibmsso_vim" };
            sf.Type = type;
            sf.Type.DefaultXris.Add("DOCAVE-XAM://ibmsso_vim?CDN=FALSE".ToLower(CultureInfo.InvariantCulture));
            sf.Description = OpenStackI18N.ResourceManager.GetString("MediaStorage_OpenStack_Configure_your_container_name_storage_account_and_API_key", culture);
            sf.Features = new List<FeatureUnit> {
                        new FeatureUnit() {Vim = "ibmsso_vim", DisplayName = OpenStackI18N.ResourceManager.GetString("MediaStorage_OpenStack_Container_Name", culture), Tag = "OpenStackContainerName", Key = "containername".ToLower(CultureInfo.InvariantCulture), KeyName = "OpenStackContainerName", Visibility="Visible", ValType = "string", IsRequiredOption = true, GuiType = "TextBox", DemoValue="DocAve", ValidateRegPats = new List<string>(){
                            @"^[^\/]{0,256}$" + "\t0\t" + OpenStackI18N.ResourceManager.GetString("MediaStorage_OpenStack_Container_names_cannot_contain_a_forward_slash_and_must_be_less_than_256_bytes_in_length", culture),
                        }},

                        //new FeatureUnit() {Vim = "openstack_vim", DisplayName = "1", Tag = "ComBox_OpenStackTenantType", Value = "OpenStackTenantType".ToLower(CultureInfo.InvariantCulture), Key = "tenantType".ToLower(CultureInfo.InvariantCulture), KeyName="OpenStackTenantType", Visibility="Visible", ValType="string", GuiType="NoLableComboBox", ChildFeatures = new List<FeatureUnit>() { 
                        //    new FeatureUnit() {Vim = "openstack_vim", DisplayName = OpenStackI18N.ResourceManager.GetString("MediaStorage_OpenStack_TenantName",culture), Value = "tenantname".ToLower(CultureInfo.InvariantCulture), Tag = "OpenStackTenantName", Key = "tenantNameBtn".ToLower(CultureInfo.InvariantCulture), Visibility="Visible", GuiType="ComboBoxItem", ValType="string", ChildFeatures = new List<FeatureUnit> {
                        //        new FeatureUnit() {Vim = "openstack_vim", DisplayName = OpenStackI18N.ResourceManager.GetString("MediaStorage_OpenStack_TenantName", culture), Tag = "OpenStackTenantName", Key = "tenantname", KeyName = "OpenStackTenantname", Visibility="Visible", ValType = "string", IsRequiredOption = false, GuiType = "NoLableTextBox"},
                        //    }},
                        //    new FeatureUnit() {Vim = "openstack_vim", DisplayName = OpenStackI18N.ResourceManager.GetString("MediaStorage_OpenStack_TenantID",culture), Value="pea", Tag = "OpenStackTenantID", Key = "tenantIDBtn".ToLower(CultureInfo.InvariantCulture), GuiType="ComboBoxItem", Visibility="Visible", ValType="string", ChildFeatures = new List<FeatureUnit> {
                        //        new FeatureUnit() {Vim = "openstack_vim", DisplayName = OpenStackI18N.ResourceManager.GetString("MediaStorage_OpenStack_TenantID", culture), Tag = "OpenStackTenantID", Key = "tenantid", KeyName = "OpenStackTenantid", Visibility="Visible", ValType = "string", IsRequiredOption = false, GuiType = "NoLableTextBox"},
                        //    }},
                    
                        //}},
                        new FeatureUnit() {Vim = "ibmsso_vim", DisplayName = OpenStackI18N.ResourceManager.GetString("MediaStorage_OpenStack_TenantName" ,culture), Tag = "OpenStackTenantType", Key = "defaulttenantnamebtn".ToLower(CultureInfo.InvariantCulture), KeyName = "OpenStackTenantname", Visibility="Visible", ValType = "string", IsRequiredOption = true, GuiType = "RadioButton", Value = "true"},  
                        new FeatureUnit() {Vim = "ibmsso_vim", DisplayName = OpenStackI18N.ResourceManager.GetString("MediaStorage_OpenStack_TenantID" ,culture), Tag = "OpenStackTenantType", Key = "tenantidbtn".ToLower(CultureInfo.InvariantCulture), KeyName = "OpenStackTenantid", Visibility="Visible", ValType = "string", GuiType = "RadioButton" },  
                        
                        new FeatureUnit() {Vim = "ibmsso_vim", DisplayName = OpenStackI18N.ResourceManager.GetString("MediaStorage_OpenStack_TenantName" ,culture), Tag = "defaulttenantnamebtn", Key = "tenantname", KeyName = "OpenStackTenantnamevalue", Visibility="Visible", ValType = "string", IsRequiredOption = true, GuiType = "TextBox", NoLable = true},
                        new FeatureUnit() {Vim = "ibmsso_vim", DisplayName = OpenStackI18N.ResourceManager.GetString("MediaStorage_OpenStack_TenantID" ,culture), Tag = "tenantidbtn", Key = "tenantid", KeyName = "OpenStackTenantidvalue", Visibility="Collapsed", ValType = "string", IsRequiredOption = true, GuiType = "TextBox", NoLable = true},
                        
                        new FeatureUnit() {Vim = "ibmsso_vim", DisplayName = OpenStackI18N.ResourceManager.GetString("MediaStorage_OpenStack_Username", culture), Tag = "OpenStackUsername", Key = "username", KeyName = "OpenStackUsername", Visibility="Visible", ValType = "string", IsRequiredOption = true, GuiType = "TextBox"},                  
                        new FeatureUnit() {Vim = "ibmsso_vim", DisplayName = OpenStackI18N.ResourceManager.GetString("MediaStorage_OpenStack_Password", culture), Tag = "OpenStackPassword", Key = "secret", KeyName = "OpenStackPassword", Visibility="Visible", ValType = "string", IsRequiredOption = true, GuiType = "PasswordBox"},
                        new FeatureUnit() {Vim = "ibmsso_vim", DisplayName = OpenStackI18N.ResourceManager.GetString("MediaStorage_OpenStack_Authentication_URL", culture), Tag = "OpenStackAuthenticationURL", Key = "authenticationurl", KeyName = "OpenStackAuthenticationURL", Visibility="Visible", ValType = "string", IsRequiredOption = true, GuiType = "TextBox"},                      
                        new FeatureUnit() {Vim = "ibmsso_vim", DisplayName = OpenStackI18N.ResourceManager.GetString("MediaStorage_OpenStack_Advanced", culture), Tag = "OpenStackWithParamsExtension", Key = "advanced".ToLower(CultureInfo.InvariantCulture), Value = "OpenStackExtendParams",  KeyName = "OpenStackAdvanced", Visibility="Visible", ValType = "string", GuiType = "CheckBox", ChildFeatures = new List<FeatureUnit>() {
                            new FeatureUnit() {Vim = "ibmsso_vim", DisplayName = OpenStackI18N.ResourceManager.GetString("MediaStorage_OpenStack_Extended_Parameters", culture), Tag = "OpenStackExtendParams", Key = "ExtendedParameters".ToLower(CultureInfo.InvariantCulture), KeyName = "OpenStackExtendParams", Visibility="Collapsed", ValType = "string", GuiType = "TextArea", CanNullOrEmpty = "true"}
                        }}
                    };
            Add(sf);
        }

        [SuppressMessage("FxCopCustomRules", "C100007:SpellCheckStringValues", MessageId = "openstack")]
        [SuppressMessage("FxCopCustomRules", "C100007:SpellCheckStringValues", MessageId = "authenticationurl")]
        [SuppressMessage("FxCopCustomRules", "C100007:SpellCheckStringValues", MessageId = "containername")]
        [SuppressMessage("FxCopCustomRules", "C100007:SpellCheckStringValues", MessageId = "Tenantname")]
        //protected override void GenerateConnectorGUIFeatureUnit(CultureInfo culture)
        //{
        //    StorageFeature sf = new StorageFeature();
        //    StorageType type = new StorageType();
        //    type.Value = "OpenStack";
        //    type.Display = OpenStackI18N.ResourceManager.GetString("MediaStorage_Connector_OpenStack_Object_Storage", culture);
        //    type.Index = 501;
        //    type.Vim = new List<string>() { "openstack_vim" };
        //    sf.Type = type;
        //    sf.Description = OpenStackI18N.ResourceManager.GetString("MediaStorage_OpenStack_Configure_your_container_name_storage_account_and_API_key", culture);
        //    sf.Features = new List<FeatureUnit> {
        //                new FeatureUnit() {Vim = "openstack_vim", DisplayName = OpenStackI18N.ResourceManager.GetString("MediaStorage_OpenStack_Container_Name", culture), Tag = "OpenStackContainerName", Key = "containername".ToLower(CultureInfo.InvariantCulture), KeyName = "OpenStackContainerName", Visibility="Visible", ValType = "string", IsRequiredOption = true, GuiType = "TextBox", DemoValue="DocAve", ValidateRegPats = new List<string>(){
        //                    @"^[^\/]{0,256}$" + "\t0\t" + OpenStackI18N.ResourceManager.GetString("MediaStorage_OpenStack_Container_names_cannot_contain_a_forward_slash_and_must_be_less_than_256_bytes_in_length", culture),
        //                }},
        //                new FeatureUnit() {Vim = "openstack_vim", DisplayName = OpenStackI18N.ResourceManager.GetString("MediaStorage_OpenStack_TenantName", culture), Tag = "OpenStackTenantname", Key = "tenant", KeyName = "OpenStackTenantname", Visibility="Visible", ValType = "string", IsRequiredOption = false, GuiType = "TextBox"},
        //                new FeatureUnit() {Vim = "openstack_vim", DisplayName = OpenStackI18N.ResourceManager.GetString("MediaStorage_OpenStack_Username", culture), Tag = "OpenStackUsername", Key = "username", KeyName = "OpenStackUsername", Visibility="Visible", ValType = "string", IsRequiredOption = true, GuiType = "TextBox"},                  
        //                new FeatureUnit() {Vim = "openstack_vim", DisplayName = OpenStackI18N.ResourceManager.GetString("MediaStorage_OpenStack_Password", culture), Tag = "OpenStackPassword", Key = "secret", KeyName = "OpenStackPassword", Visibility="Visible", ValType = "string", IsRequiredOption = true, GuiType = "PasswordBox"},
        //                new FeatureUnit() {Vim = "openstack_vim", DisplayName = "Authentication URL", Tag = "OpenStackAuthenticationUrl", Key = "authenticationurl", KeyName = "OpenStackAuthenticationUrl", Visibility="Visible", ValType = "string", IsRequiredOption = false, GuiType = "TextBox"},                      
        //                new FeatureUnit() {Vim = "openstack_vim", DisplayName = OpenStackI18N.ResourceManager.GetString("MediaStorage_OpenStack_CDN_Enabled", culture), Tag = "OpenStackCDN", Value="cdn_cloud_OpenStack", Key = "cdn", KeyName = "OpenStackCDN", Visibility="Visible", ValType = "string", GuiType = "CheckBox"},
        //                new FeatureUnit() {Vim = "openstack_vim", DisplayName = OpenStackI18N.ResourceManager.GetString("MediaStorage_OpenStack_Advanced", culture), Tag = "OpenStackWithParamsExtension", Key = "advanced".ToLower(CultureInfo.InvariantCulture), Value = "OpenStack_vim_ExtendedParameters",  KeyName = "OpenStackWithParamsExtension", Visibility="Visible", ValType = "string", GuiType = "CheckBox", ChildFeatures = new List<FeatureUnit>(){
        //                    new FeatureUnit() {Vim = "openstack_vim", DisplayName = OpenStackI18N.ResourceManager.GetString("MediaStorage_OpenStack_Extended_Parameters", culture), Tag = "OpenStack_vim_ExtendedParameters", Key = "ExtendedParameters".ToLower(CultureInfo.InvariantCulture), KeyName = "OpenStackExtendParams", Visibility="Collapsed", ValType = "string", GuiType = "TextArea", CanNullOrEmpty = "true"}}}
        //            };
        //    Add(sf);
        //}
        #endregion

        /// <summary>
        /// 供VIM调用, 获取相应的Feature
        /// </summary>
        private static readonly Dictionary<string, OpenStackFeature> instances = new Dictionary<string, OpenStackFeature>();
        private static Object locker = new Object();
        public static OpenStackFeature Getstances(int type, string culture = "en")
        {
            lock (locker)
            {
                if (!instances.ContainsKey(type + culture))
                {
                    OpenStackFeature OpenStack = new OpenStackFeature(type, culture);
                    foreach (var feature in OpenStack.FeatureObjs)
                    {
                        feature.AdvancedOptions.AddRange(new List<string>()
                            {
                                "RetryInterval=30000", //30s
                                "RetryCount=6",
                                "CustomizedMetadata={[testKey1,testValue1],[testKey2,testValue2],[testKey3,testValue3]}}",
                                "CustomizedMode=Close",
                                "CustomizedMode=SupportAll",
                                "CustomizedMode=DocAveOnly",
                                "CustomizedMode=CustomizedOnly"
                            });
                    }
                    instances[type + culture] = OpenStack;
                    return OpenStack;
                }
                else
                {
                    return instances[type + culture];
                }
            }
        }
    }
}
