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



using System.Diagnostics.CodeAnalysis;

[module: SuppressMessage("FxCopCustomRules", "C100007:SpellCheckStringValues", Scope = "member", Target = "AvePoint.Media.Storage.Cloud.Atmos.AtmosFeature.#GenerateConnectorGUIFeatureUnit(System.Globalization.CultureInfo)", MessageId = "atmosonline")]
namespace AvePoint.Media.Storage.Cloud.Atmos
{
    #region using directives
    using AvePoint.Media.Storage.Resources.AtmosI18N;
    using System;
    using System.Collections.Generic;
    using System.Diagnostics.CodeAnalysis;
    using System.Globalization;
    #endregion

    /// <summary>
    /// 此类用于描述Atmos Cloud类型的存储介质的特性
    /// </summary>
    /// 
    class AtmosFeature : StorageFeature
    {
        #region AtmosFeature 自己负责初始化的部分, 在Application生存周期内只会初始化一次, 外部不需要知道的逻辑
        /// <summary>
        /// 私有构造函数， 此类不允许在外部实例化。 <see cref="AtmosFeature"/> class.
        /// </summary>
        private AtmosFeature(int type, string culture)
        {
            this.Init(type, culture);
        }

        protected override void GenerateSingleTypeFeatureUnit(CultureInfo culture)
        {
            StorageFeature sf = new StorageFeature();
            StorageType type = new StorageType();
            type.Value = "Atmos";
            type.Display = "EMC Atmos";
            type.Index = 404;
            type.Vim = new List<string>() { "atmos_vim" };
            sf.Type = type;
            sf.Type.DefaultXris.Add("DOCAVE-XAM://ATMOS_VIM?ACCESSPOINT=HTTP://ACCESSPOINT.EMCCIS.COM".ToLower(CultureInfo.InvariantCulture));
            sf.Type.Vim.Add("atmos_vim");

            sf.Features = new List<FeatureUnit> {
                        new FeatureUnit() {Vim = "atmos_vim", DisplayName = "Access Point", Tag = "cloud_atmos", Key = "accessPoint".ToLower(CultureInfo.InvariantCulture), KeyName = "AtmosAccessPoint", Visibility="Visible", ValType = "string", GuiType = "TextBox"},
                        new FeatureUnit() {Vim = "atmos_vim", DisplayName = "Root Folder", Tag = "cloud_atmos", Key = "containerName".ToLower(CultureInfo.InvariantCulture), KeyName = "AtmosContainerName", Visibility="Visible", ValType = "string", GuiType = "TextBox"},
                        new FeatureUnit() {Vim = "atmos_vim", DisplayName = "Full Token ID", Tag = "cloud_atmos", Key = "name", KeyName = "AtmosUsername", Visibility="Visible", ValType = "string", GuiType = "TextBox"},
                        new FeatureUnit() {Vim = "atmos_vim", DisplayName = "Shared Secret", Tag = "cloud_atmos", Key = "secret", KeyName = "AtmosAPIKey", Visibility="Visible", ValType = "string", GuiType = "PasswordBox"},
                    };
            Add(sf);
            StorageFeature attSf = new StorageFeature();
            StorageType attType = new StorageType();
            attType.Value = "Att";
            attType.Display = "AT&T Synaptic";
            attType.Index = 405;
            attType.Vim = new List<string>() { "att_vim" };
            attSf.Type = attType;
            attSf.Type.DefaultXris.Add("DOCAVE-XAM://ATT_VIM?".ToLower(CultureInfo.InvariantCulture));
            attSf.Type.Vim.Add("att_vim");
            attSf.Features = new List<FeatureUnit> {
                        new FeatureUnit() {Vim = "att_vim", DisplayName = "Root Folder", Tag = "cloud_att", Key = "containerName".ToLower(CultureInfo.InvariantCulture), KeyName = "AT&TContainerName", Visibility="Visible", ValType = "string", GuiType = "TextBox"},
                        new FeatureUnit() {Vim = "att_vim", DisplayName = "Full Token ID", Tag = "cloud_att", Key = "name", KeyName = "AT&TUsername", Visibility="Visible", ValType = "string", GuiType = "TextBox"},
                        new FeatureUnit() {Vim = "att_vim", DisplayName = "Shared Secret", Tag = "cloud_att", Key = "secret", KeyName = "AT&TAPIKey", Visibility="Visible", ValType = "string", GuiType = "PasswordBox"},
                    };
            Add(attSf);
        }

        [SuppressMessage("FxCopCustomRules", "C100007:SpellCheckStringValues", MessageId = "atmosonline")]
        protected override void GenerateDocAveGUIFeatureUnit(CultureInfo culture)
        {
            StorageFeature sf = new StorageFeature();
            StorageType type = new StorageType();
            type.Value = "Atmos";
            type.Display = AtmosI18N.ResourceManager.GetString("MediaStorage_Atmos_EMC_Atmos", culture);
            type.Index = 404;
            type.Vim = new List<string>() { "atmos_vim" };
            sf.Type = type;
            sf.Type.DefaultXris.Add("DOCAVE-XAM://ATMOS_VIM?ACCESSPOINT=HTTP://ACCESSPOINT.EMCCIS.COM".ToLower(CultureInfo.InvariantCulture));

            sf.Description = AtmosI18N.ResourceManager.GetString("MediaStorage_Atmos_Configure_your_root_folder_user_ID_and_secret", culture);
            sf.Features = new List<FeatureUnit> {
                        new FeatureUnit() {Vim = "atmos_vim", DisplayName = AtmosI18N.ResourceManager.GetString("MediaStorage_Atmos_Interface_Type",culture), Value = "StorageType".ToLower(CultureInfo.InvariantCulture),Tag = "cloud_atmos", Key = "StorageType".ToLower(CultureInfo.InvariantCulture), KeyName="StorageTypeCOMM", Visibility="Visible", ValType="string", GuiType="ComboBox", ChildFeatures = new List<FeatureUnit>() {
                        new FeatureUnit() {Vim = "atmos_vim", DisplayName = AtmosI18N.ResourceManager.GetString("MediaStorage_Atmos_Namepace_Type",culture), Value = "namespace".ToLower(CultureInfo.InvariantCulture),Tag = "atmos_namespace", Key = "NAMESPACE".ToLower(CultureInfo.InvariantCulture), GuiType="ComboBoxItem", Visibility="Visible", ValType="string", ChildFeatures = new List<FeatureUnit> {
                            new FeatureUnit() {Vim = "atmos_vim", DisplayName = AtmosI18N.ResourceManager.GetString("MediaStorage_Atmos_Access_Point",culture),IsRequiredOption = true, Tag = "atmos_namespace", Key = "accessPoint".ToLower(CultureInfo.InvariantCulture), KeyName = "NamespaceAtmosAccessPoint", Visibility="Visible", ValType = "string", GuiType = "TextBox", DefaultValue="http://portal.atmosonline.com".ToLower(CultureInfo.InvariantCulture)},
                            new FeatureUnit() {Vim = "atmos_vim", DisplayName = AtmosI18N.ResourceManager.GetString("MediaStorage_Atmos_Root_Folder",culture),IsRequiredOption = true, Tag = "atmos_namespace", Key = "containerName".ToLower(CultureInfo.InvariantCulture), KeyName = "NamespaceAtmosContainerName", Visibility="Visible", ValType = "string", GuiType = "TextBox", DemoValue="DocAve",ValidateRegPats = new List<string>{
                            @"^.{0,200}$" + "\t0\t" + AtmosI18N.ResourceManager.GetString("MediaStorage_Atmos_The_root_folder_name_cannot_exceed_200_characters", culture),
                            @"^[^\\]+|^.{0,0}$" + "\t0\t" + AtmosI18N.ResourceManager.GetString("MediaStorage_Atmos_RootFolderName_Invalid_Format", culture)}},
                            new FeatureUnit() {Vim = "atmos_vim", DisplayName = AtmosI18N.ResourceManager.GetString("MediaStorage_Atmos_Full_Token_ID",culture),IsRequiredOption = true, Tag = "atmos_namespace", Key = "name", KeyName = "NamespaceAtmosUsername", Visibility="Visible", ValType = "string", GuiType = "TextBox",DemoValue = AtmosI18N.ResourceManager.GetString("MediaStorage_Atmos_Full_Token_ID_Demo", culture)},
                            new FeatureUnit() {Vim = "atmos_vim", DisplayName = AtmosI18N.ResourceManager.GetString("MediaStorage_Atmos_Shared_Secret",culture),IsRequiredOption = true, Tag = "atmos_namespace", Key = "secret", KeyName = "NamespaceAtmosAPIKey", Visibility="Visible", ValType = "string", GuiType = "PasswordBox"},
                            }},
                        new FeatureUnit() {Vim = "atmos_vim", DisplayName = AtmosI18N.ResourceManager.GetString("MediaStorage_Atmos_Object_Type",culture), Value = "object".ToLower(CultureInfo.InvariantCulture),Tag = "atmos_object", Key = "OBJECT".ToLower(CultureInfo.InvariantCulture), Visibility="Collapsed", GuiType="ComboBoxItem", ValType="string", ChildFeatures = new List<FeatureUnit> {
                            new FeatureUnit() {Vim = "atmos_vim", DisplayName = AtmosI18N.ResourceManager.GetString("MediaStorage_Atmos_Access_Point",culture),IsRequiredOption = true, Tag = "atmos_object", Key = "accessPoint".ToLower(CultureInfo.InvariantCulture), KeyName = "ObjectAtmosAccessPoint", Visibility="Collapsed", ValType = "string", GuiType = "TextBox", DefaultValue="http://portal.atmosonline.com".ToLower(CultureInfo.InvariantCulture)},
                            new FeatureUnit() {Vim = "atmos_vim", DisplayName = AtmosI18N.ResourceManager.GetString("MediaStorage_Atmos_Full_Token_ID",culture),IsRequiredOption = true, Tag = "atmos_object", Key = "name", KeyName = "ObjectAtmosUsername", Visibility="Collapsed", ValType = "string", GuiType = "TextBox",DemoValue = AtmosI18N.ResourceManager.GetString("MediaStorage_Atmos_Full_Token_ID_Demo", culture)},
                            new FeatureUnit() {Vim = "atmos_vim", DisplayName = AtmosI18N.ResourceManager.GetString("MediaStorage_Atmos_Shared_Secret",culture),IsRequiredOption = true, Tag = "atmos_object", Key = "secret", KeyName = "ObjectAtmosAPIKey", Visibility="Collapsed", ValType = "string", GuiType = "PasswordBox"},
                            }},
                        }
                    },
                    new FeatureUnit() {Vim = "atmos_vim", DisplayName = AtmosI18N.ResourceManager.GetString("MediaStorage_Atmos_Proxy", culture), Tag = "cloud_atmos_Proxy".ToLower(CultureInfo.InvariantCulture), Key = "cloud_atmos_Proxy".ToLower(CultureInfo.InvariantCulture), Value = "cloud_atmos_Proxy_setting".ToLower(CultureInfo.InvariantCulture),  KeyName = "atmosProxySetting", Visibility="Visible", ValType = "string", GuiType = "CheckBox", ChildFeatures = new List<FeatureUnit>(){
                        new FeatureUnit() {Vim = "atmos_vim", DisplayName = AtmosI18N.ResourceManager.GetString("MediaStorage_Atmos_Proxy_Host", culture), Tag = "cloud_atmos_Proxy_setting".ToLower(CultureInfo.InvariantCulture), Key = "ProxyIp".ToLower(CultureInfo.InvariantCulture), KeyName = "AtmosProxyHost", Visibility="Collapsed", ValType = "string", IsRequiredOption = true, GuiType = "TextBox"},
                        new FeatureUnit() {Vim = "atmos_vim", DisplayName = AtmosI18N.ResourceManager.GetString("MediaStorage_Atmos_Proxy_Port", culture), Tag = "cloud_atmos_Proxy_setting".ToLower(CultureInfo.InvariantCulture), Key = "ProxyPort".ToLower(CultureInfo.InvariantCulture), KeyName = "AtmosProxyPort", Visibility="Collapsed", ValType = "string", IsRequiredOption = true, GuiType = "TextBox"},
                        new FeatureUnit() {Vim = "atmos_vim", DisplayName = AtmosI18N.ResourceManager.GetString("MediaStorage_Atmos_Proxy_Username", culture), Tag = "cloud_atmos_Proxy_setting".ToLower(CultureInfo.InvariantCulture), Key = "ProxyUsername".ToLower(CultureInfo.InvariantCulture), KeyName = "AtmosProxyUsername", Visibility="Collapsed", ValType = "string", IsRequiredOption = false, GuiType = "TextBox", CanNullOrEmpty = "true"},
                        new FeatureUnit() {Vim = "atmos_vim", DisplayName = AtmosI18N.ResourceManager.GetString("MediaStorage_Atmos_Proxy_Password", culture), Tag = "cloud_atmos_Proxy_setting".ToLower(CultureInfo.InvariantCulture), Key = "ProxyPasswordSecret".ToLower(CultureInfo.InvariantCulture), KeyName = "AtmosProxyPasswordSecret", Visibility="Collapsed", ValType = "string", IsRequiredOption = false, GuiType = "PasswordBox", CanNullOrEmpty = "true"},
                    }},
                    new FeatureUnit() {Vim = "atmos_vim", DisplayName = AtmosI18N.ResourceManager.GetString("MediaStorage_Atmos_Advanced",culture), Tag = "cloud_atmos_advanced", Key = "advanced".ToLower(CultureInfo.InvariantCulture), Value = "atmos_vim_ExtendedParameters",  KeyName = "AtmosAdvanced", Visibility="Visible", ValType = "string", GuiType = "CheckBox", ChildFeatures = new List<FeatureUnit>(){
                        new FeatureUnit() {Vim = "atmos_vim", DisplayName = AtmosI18N.ResourceManager.GetString("MediaStorage_Atmos_Extended_Parameters",culture), Tag = "atmos_vim_ExtendedParameters", Key = "ExtendedParameters".ToLower(CultureInfo.InvariantCulture), KeyName = "AtmosExtendedParameters", Visibility="Collapsed", ValType = "string", GuiType = "TextArea", CanNullOrEmpty = "true"}}}
            };
            Add(sf);
            StorageFeature attSf = new StorageFeature();
            StorageType attType = new StorageType();
            attType.Value = "Att";
            attType.Display = AtmosI18N.ResourceManager.GetString("MediaStorage_Atmos_ATT_Synaptic", culture);
            attType.Index = 405;
            attType.Vim = new List<string>() { "att_vim" };
            attSf.Type = attType;
            attSf.Type.DefaultXris.Add("DOCAVE-XAM://ATT_VIM?".ToLower(CultureInfo.InvariantCulture));
            attSf.Description = AtmosI18N.ResourceManager.GetString("MediaStorage_Atmos_Configure_your_root_folder_user_ID_and_secret", culture);
            attSf.Features = new List<FeatureUnit> {
                        new FeatureUnit() {Vim = "att_vim", DisplayName = AtmosI18N.ResourceManager.GetString("MediaStorage_Atmos_Root_Folder",culture), Tag = "cloud_att", Key = "containerName".ToLower(CultureInfo.InvariantCulture), KeyName = "AT&TContainerName", Visibility="Visible", ValType = "string", IsRequiredOption = true, GuiType = "TextBox", DemoValue="DOCAVE".ToLower(CultureInfo.InvariantCulture),ValidateRegPats = new List<string>{
                        @"^.{0,200}$" + "\t0\t" + AtmosI18N.ResourceManager.GetString("MediaStorage_Atmos_The_root_folder_name_cannot_exceed_200_characters", culture),
                        @"^[^\\/]+|^.{0,0}$" + "\t0\t" + AtmosI18N.ResourceManager.GetString("MediaStorage_Atmos_RootFolderName_Invalid_Format", culture)}},
                        new FeatureUnit() {Vim = "att_vim", DisplayName = AtmosI18N.ResourceManager.GetString("MediaStorage_Atmos_Full_Token_ID",culture), Tag = "cloud_att", Key = "name", KeyName = "AT&TUsername", Visibility="Visible", ValType = "string", IsRequiredOption = true, GuiType = "TextBox",DemoValue = AtmosI18N.ResourceManager.GetString("MediaStorage_Atmos_Full_Token_ID_Demo", culture)},
                        new FeatureUnit() {Vim = "att_vim", DisplayName = AtmosI18N.ResourceManager.GetString("MediaStorage_Atmos_Shared_Secret",culture), Tag = "cloud_att", Key = "secret", KeyName = "AT&TAPIKey", Visibility="Visible", ValType = "string", IsRequiredOption = true, GuiType = "PasswordBox"},
                        new FeatureUnit() {Vim = "att_vim", DisplayName = AtmosI18N.ResourceManager.GetString("MediaStorage_Atmos_Proxy", culture), Tag = "cloud_att_Proxy".ToLower(CultureInfo.InvariantCulture), Key = "cloud_att_Proxy".ToLower(CultureInfo.InvariantCulture), Value = "cloud_att_Proxy_setting".ToLower(CultureInfo.InvariantCulture),  KeyName = "attProxySetting", Visibility="Visible", ValType = "string", GuiType = "CheckBox", ChildFeatures = new List<FeatureUnit>(){
                            new FeatureUnit() {Vim = "att_vim", DisplayName = AtmosI18N.ResourceManager.GetString("MediaStorage_Atmos_Proxy_Host", culture), Tag = "cloud_att_Proxy_setting".ToLower(CultureInfo.InvariantCulture), Key = "ProxyIp".ToLower(CultureInfo.InvariantCulture), KeyName = "AttProxyHost", Visibility="Collapsed", ValType = "string", IsRequiredOption = true, GuiType = "TextBox"},
                            new FeatureUnit() {Vim = "att_vim", DisplayName = AtmosI18N.ResourceManager.GetString("MediaStorage_Atmos_Proxy_Port", culture), Tag = "cloud_att_Proxy_setting".ToLower(CultureInfo.InvariantCulture), Key = "ProxyPort".ToLower(CultureInfo.InvariantCulture), KeyName = "AttProxyPort", Visibility="Collapsed", ValType = "string", IsRequiredOption = true, GuiType = "TextBox"},
                            new FeatureUnit() {Vim = "att_vim", DisplayName = AtmosI18N.ResourceManager.GetString("MediaStorage_Atmos_Proxy_Username", culture), Tag = "cloud_att_Proxy_setting".ToLower(CultureInfo.InvariantCulture), Key = "ProxyUsername".ToLower(CultureInfo.InvariantCulture), KeyName = "AttProxyUsername", Visibility="Collapsed", ValType = "string", IsRequiredOption = false, GuiType = "TextBox", CanNullOrEmpty = "true"},
                            new FeatureUnit() {Vim = "att_vim", DisplayName = AtmosI18N.ResourceManager.GetString("MediaStorage_Atmos_Proxy_Password", culture), Tag = "cloud_att_Proxy_setting".ToLower(CultureInfo.InvariantCulture), Key = "ProxyPasswordSecret".ToLower(CultureInfo.InvariantCulture), KeyName = "AttProxyPasswordSecret", Visibility="Collapsed", ValType = "string", IsRequiredOption = false, GuiType = "PasswordBox", CanNullOrEmpty = "true"},
                        }},
                        new FeatureUnit() {Vim = "att_vim", DisplayName = AtmosI18N.ResourceManager.GetString("MediaStorage_Atmos_Advanced",culture), Tag = "cloud_att_advanced", Key = "advanced".ToLower(CultureInfo.InvariantCulture), Value = "att_vim_ExtendedParameters",  KeyName = "AttAdvanced", Visibility="Visible", ValType = "string", GuiType = "CheckBox", ChildFeatures = new List<FeatureUnit>(){
                            new FeatureUnit() {Vim = "att_vim", DisplayName = AtmosI18N.ResourceManager.GetString("MediaStorage_Atmos_Extended_Parameters",culture), Tag = "att_vim_ExtendedParameters", Key = "ExtendedParameters".ToLower(CultureInfo.InvariantCulture), KeyName = "AttExtendedParameters", Visibility="Collapsed", ValType = "string", GuiType = "TextArea", CanNullOrEmpty = "true"}}}
            };
            Add(attSf);
        }

        protected override void GenerateConnectorGUIFeatureUnit(CultureInfo culture)
        {
            StorageFeature sf = new StorageFeature();
            StorageType type = new StorageType();
            type.Value = "Atmos";
            type.Display = AtmosI18N.ResourceManager.GetString("MediaStorage_Atmos_EMC_Atmos", culture);
            type.Index = 404;
            type.Vim = new List<string>() { "atmos_vim" };
            sf.Type = type;
            sf.Description = AtmosI18N.ResourceManager.GetString("MediaStorage_Atmos_Configure_your_root_folder_user_ID_and_secret", culture);
            sf.Features = new List<FeatureUnit> {
                        new FeatureUnit() {IsRequiredOption = true, Vim = "atmos_vim", DisplayName = AtmosI18N.ResourceManager.GetString("MediaStorage_Connector_Atmos_Access_Point",culture), Tag = "cloud_atmos", Key = "accessPoint".ToLower(CultureInfo.InvariantCulture), KeyName = "AtmosAccessPoint", Visibility="Visible", ValType = "string", GuiType = "TextBox", DefaultValue="http://portal.atmosonline.com".ToLower(CultureInfo.InvariantCulture)},
                        new FeatureUnit() {IsRequiredOption = true, Vim = "atmos_vim", DisplayName = AtmosI18N.ResourceManager.GetString("MediaStorage_Connector_Atmos_Root_Folder",culture), Tag = "cloud_atmos", Key = "containerName".ToLower(CultureInfo.InvariantCulture), KeyName = "AtmosContainerName", Visibility="Visible", ValType = "string", GuiType = "TextBox",  FeatureFlag=(int)(FeatureUnitFlag.CloudContainer|FeatureUnitFlag.Path) ,  CanModifi = false,ValidateRegPats = new List<string>{
                        @"^.{0,200}$" + "\t0\t" + AtmosI18N.ResourceManager.GetString("MediaStorage_Connector_Atmos_The_root_folder_name_cannot_exceed_200_characters", culture),
                        @"^[^\\]+|^.{0,0}$" + "\t0\t" + AtmosI18N.ResourceManager.GetString("MediaStorage_Connector_Atmos_RootFolderName_Invalid_Format", culture)}, DemoValue = AtmosI18N.ResourceManager.GetString("MediaStorage_Connector_Atmos_RootFolderName_Demo", culture)},
                        new FeatureUnit() {IsRequiredOption = true, Vim = "atmos_vim", DisplayName = AtmosI18N.ResourceManager.GetString("MediaStorage_Connector_Atmos_Full_Token_ID",culture), Tag = "cloud_atmos", Key = "name", KeyName = "AtmosUsername", Visibility="Visible", ValType = "string", GuiType = "TextBox"},
                        new FeatureUnit() {IsRequiredOption = true, Vim = "atmos_vim", DisplayName = AtmosI18N.ResourceManager.GetString("MediaStorage_Connector_Atmos_Shared_Secret",culture), Tag = "cloud_atmos", Key = "secret", KeyName = "AtmosAPIKey", Visibility="Visible", ValType = "string", GuiType = "PasswordBox"},
                        new FeatureUnit() {Vim = "atmos_vim", DisplayName = AtmosI18N.ResourceManager.GetString("MediaStorage_Connector_Atmos_Proxy", culture), Tag = "cloud_atmos_Proxy".ToLower(CultureInfo.InvariantCulture), Key = "cloud_atmos_Proxy".ToLower(CultureInfo.InvariantCulture), Value = "cloud_atmos_Proxy_setting".ToLower(CultureInfo.InvariantCulture),  KeyName = "atmosProxySetting", Visibility="Visible", ValType = "string", GuiType = "CheckBox", ChildFeatures = new List<FeatureUnit>(){
                            new FeatureUnit() {Vim = "atmos_vim", DisplayName = AtmosI18N.ResourceManager.GetString("MediaStorage_Connector_Atmos_Proxy_Host", culture), Tag = "cloud_atmos_Proxy_setting".ToLower(CultureInfo.InvariantCulture), Key = "ProxyIp".ToLower(CultureInfo.InvariantCulture), KeyName = "AtmosProxyHost", Visibility="Collapsed", ValType = "string", IsRequiredOption = true, GuiType = "TextBox"},
                            new FeatureUnit() {Vim = "atmos_vim", DisplayName = AtmosI18N.ResourceManager.GetString("MediaStorage_Connector_Atmos_Proxy_Port", culture), Tag = "cloud_atmos_Proxy_setting".ToLower(CultureInfo.InvariantCulture), Key = "ProxyPort".ToLower(CultureInfo.InvariantCulture), KeyName = "AtmosProxyPort", Visibility="Collapsed", ValType = "string", IsRequiredOption = true, GuiType = "TextBox",ValidateRegPats = new List<string>(){
                            @"(^[0-9]\d{0,3}$)|(^[1-5]\d{4}$)|(^6[0-4]\d{3}$)|(^65[0-4]\d{2}$)|(^655[0-2]\d$)|(^6553[0-5]$)"+ "\t0\t"+AtmosI18N.ResourceManager.GetString("MediaStorage_Connector_Atmos_Must_be_a_number_between_0_and_65535", culture)
                            }},
                            new FeatureUnit() {Vim = "atmos_vim", DisplayName = AtmosI18N.ResourceManager.GetString("MediaStorage_Connector_Atmos_Proxy_Username", culture), Tag = "cloud_atmos_Proxy_setting".ToLower(CultureInfo.InvariantCulture), Key = "ProxyUsername".ToLower(CultureInfo.InvariantCulture), KeyName = "AtmosProxyUsername", Visibility="Collapsed", ValType = "string", IsRequiredOption = false, GuiType = "TextBox", CanNullOrEmpty = "true"},
                            new FeatureUnit() {Vim = "atmos_vim", DisplayName = AtmosI18N.ResourceManager.GetString("MediaStorage_Connector_Atmos_Proxy_Password", culture), Tag = "cloud_atmos_Proxy_setting".ToLower(CultureInfo.InvariantCulture), Key = "ProxyPasswordSecret".ToLower(CultureInfo.InvariantCulture), KeyName = "AtmosProxyPasswordSecret", Visibility="Collapsed", ValType = "string", IsRequiredOption = false, GuiType = "PasswordBox", CanNullOrEmpty = "true"},
                        }},
                        new FeatureUnit() {IsRequiredOption = true, Vim = "atmos_vim", DisplayName = AtmosI18N.ResourceManager.GetString("MediaStorage_Connector_Atmos_Advanced",culture), Tag = "cloud_atmos", Key = "advanced".ToLower(CultureInfo.InvariantCulture), Value = "atmos_vim_ExtendedParameters",  KeyName = "AtmosWithParamsExtension", Visibility="Visible", ValType = "string", GuiType = "CheckBox", ChildFeatures = new List<FeatureUnit>(){
                            new FeatureUnit() {IsRequiredOption = true, Vim = "atmos_vim", DisplayName = AtmosI18N.ResourceManager.GetString("MediaStorage_Connector_Atmos_Extended_Parameters",culture), Tag = "atmos_vim_ExtendedParameters", Key = "ExtendedParameters".ToLower(CultureInfo.InvariantCulture), KeyName = "AtmosExtendParams", Visibility="Collapsed", ValType = "string", GuiType = "TextArea", CanNullOrEmpty = "true"}}}
            };
            Add(sf);
            StorageFeature attSf = new StorageFeature();
            StorageType attType = new StorageType();
            attType.Value = "Att";
            attType.Display = AtmosI18N.ResourceManager.GetString("MediaStorage_Connector_Atmos_ATT_Synaptic", culture);
            attType.Index = 405;
            attType.Vim = new List<string>() { "att_vim" };
            attSf.Type = attType;
            attSf.Description = AtmosI18N.ResourceManager.GetString("MediaStorage_Atmos_Configure_your_root_folder_user_ID_and_secret", culture);
            attSf.Features = new List<FeatureUnit> {
                        new FeatureUnit() {IsRequiredOption = true, Vim = "att_vim", DisplayName = AtmosI18N.ResourceManager.GetString("MediaStorage_Connector_Atmos_Root_Folder",culture), Tag = "cloud_att", Key = "containerName", KeyName = "AttContainerName", Visibility="Visible", ValType = "string", GuiType = "TextBox", FeatureFlag=(int)(FeatureUnitFlag.CloudContainer|FeatureUnitFlag.Path) , CanModifi = false,ValidateRegPats = new List<string>{
                        @"^.{0,200}$" + "\t0\t" + AtmosI18N.ResourceManager.GetString("MediaStorage_Connector_Atmos_The_root_folder_name_cannot_exceed_200_characters", culture),
                        @"^[^\\/]+|^.{0,0}$" + "\t0\t" + AtmosI18N.ResourceManager.GetString("MediaStorage_Connector_Atmos_RootFolderName_Invalid_Format", culture)}},
                        new FeatureUnit() {IsRequiredOption = true, Vim = "att_vim", DisplayName = AtmosI18N.ResourceManager.GetString("MediaStorage_Connector_Atmos_Full_Token_ID",culture), Tag = "cloud_att", Key = "name", KeyName = "AttUsername", Visibility="Visible", ValType = "string", GuiType = "TextBox"},
                        new FeatureUnit() {IsRequiredOption = true, Vim = "att_vim", DisplayName = AtmosI18N.ResourceManager.GetString("MediaStorage_Connector_Atmos_Shared_Secret",culture), Tag = "cloud_att", Key = "secret", KeyName = "AttAPIKey", Visibility="Visible", ValType = "string", GuiType = "PasswordBox"},
                        new FeatureUnit() {Vim = "att_vim", DisplayName = AtmosI18N.ResourceManager.GetString("MediaStorage_Connector_Atmos_Proxy", culture), Tag = "cloud_att_Proxy".ToLower(CultureInfo.InvariantCulture), Key = "cloud_att_Proxy".ToLower(CultureInfo.InvariantCulture), Value = "cloud_att_Proxy_setting".ToLower(CultureInfo.InvariantCulture),  KeyName = "attProxySetting", Visibility="Visible", ValType = "string", GuiType = "CheckBox", ChildFeatures = new List<FeatureUnit>(){
                            new FeatureUnit() {Vim = "att_vim", DisplayName = AtmosI18N.ResourceManager.GetString("MediaStorage_Connector_Atmos_Proxy_Host", culture), Tag = "cloud_att_Proxy_setting".ToLower(CultureInfo.InvariantCulture), Key = "ProxyIp".ToLower(CultureInfo.InvariantCulture), KeyName = "AttProxyHost", Visibility="Collapsed", ValType = "string", IsRequiredOption = true, GuiType = "TextBox"},
                            new FeatureUnit() {Vim = "att_vim", DisplayName = AtmosI18N.ResourceManager.GetString("MediaStorage_Connector_Atmos_Proxy_Port", culture), Tag = "cloud_att_Proxy_setting".ToLower(CultureInfo.InvariantCulture), Key = "ProxyPort".ToLower(CultureInfo.InvariantCulture), KeyName = "AttProxyPort", Visibility="Collapsed", ValType = "string", IsRequiredOption = true, GuiType = "TextBox",ValidateRegPats = new List<string>(){
                            @"(^[0-9]\d{0,3}$)|(^[1-5]\d{4}$)|(^6[0-4]\d{3}$)|(^65[0-4]\d{2}$)|(^655[0-2]\d$)|(^6553[0-5]$)"+ "\t0\t"+AtmosI18N.ResourceManager.GetString("MediaStorage_Connector_Atmos_Must_be_a_number_between_0_and_65535", culture)
                            }},
                            new FeatureUnit() {Vim = "att_vim", DisplayName = AtmosI18N.ResourceManager.GetString("MediaStorage_Connector_Atmos_Proxy_Username", culture), Tag = "cloud_att_Proxy_setting".ToLower(CultureInfo.InvariantCulture), Key = "ProxyUsername".ToLower(CultureInfo.InvariantCulture), KeyName = "AttProxyUsername", Visibility="Collapsed", ValType = "string", IsRequiredOption = false, GuiType = "TextBox", CanNullOrEmpty = "true"},
                            new FeatureUnit() {Vim = "att_vim", DisplayName = AtmosI18N.ResourceManager.GetString("MediaStorage_Connector_Atmos_Proxy_Password", culture), Tag = "cloud_att_Proxy_setting".ToLower(CultureInfo.InvariantCulture), Key = "ProxyPasswordSecret".ToLower(CultureInfo.InvariantCulture), KeyName = "AttProxyPasswordSecret", Visibility="Collapsed", ValType = "string", IsRequiredOption = false, GuiType = "PasswordBox", CanNullOrEmpty = "true"},
                        }},
                        new FeatureUnit() {IsRequiredOption = true, Vim = "att_vim", DisplayName = AtmosI18N.ResourceManager.GetString("MediaStorage_Connector_Atmos_Advanced",culture), Tag = "cloud_att", Key = "advanced".ToLower(CultureInfo.InvariantCulture), Value = "att_vim_ExtendedParameters",  KeyName = "AttWithParamsExtension", Visibility="Visible", ValType = "string", GuiType = "CheckBox", ChildFeatures = new List<FeatureUnit>(){
                            new FeatureUnit() {IsRequiredOption = true, Vim = "att_vim", DisplayName = AtmosI18N.ResourceManager.GetString("MediaStorage_Connector_Atmos_Extended_Parameters",culture), Tag = "att_vim_ExtendedParameters", Key = "ExtendedParameters".ToLower(CultureInfo.InvariantCulture), KeyName = "AttExtendParams", Visibility="Collapsed", ValType = "string", GuiType = "TextArea", CanNullOrEmpty = "true"}}}
            };
            Add(attSf);
        }
        #endregion

        /// <summary>
        /// 供VIM调用, 获取相应的Feature
        /// </summary>
        private static readonly Dictionary<string, AtmosFeature> instances = new Dictionary<string, AtmosFeature>();
        private static Object locker = new Object();
        public static AtmosFeature Getstances(int type, string culture = "en")
        {
            lock (locker)
            {
                if (!instances.ContainsKey(type + culture))
                {
                    AtmosFeature atmos = new AtmosFeature(type, culture);
                    foreach (var feature in atmos.FeatureObjs)
                    {
                        feature.AdvancedOptions.AddRange(new List<string>()
                            {
                                "RetryInterval=30000",
                                "RetryCount=6",
                                "CustomizedMetadata={[testKey1,testValue1],[testKey2,testValue2],[testKey3,testValue3]}}",
                                "CustomizedMode=Close",
                                "CustomizedMode=SupportAll",
                                "CustomizedMode=DocAveOnly",
                                "CustomizedMode=CustomizedOnly",
                                "EnableChecksumForCreate=true",
                                "VerifyChecksumAtRead=true"
                            });
                    }
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
