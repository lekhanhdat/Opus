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

namespace AvePoint.Media.Storage.Box
{
    #region using directives
    using AvePoint.Media.Storage.Resources.BoxI18N;
    using System;
    using System.Collections.Generic;
    using System.Globalization; 
    #endregion

    class BoxFeature : StorageFeature
    {
        private BoxFeature(int type, string culture)
        {
            this.Init(type, culture);
        }

        protected override void GenerateSingleTypeFeatureUnit(CultureInfo culture)
        {
            StorageFeature sf = new StorageFeature();
            StorageType type = new StorageType();
            type.Value = "Box";
            type.Display = "Box";
            type.Index = 408;
            type.IsSupportMovableRetention = false;
            type.Vim = new List<string>() { "box_vim" };
            sf.Type = type;
            sf.Type.DefaultXris.Add("DOCAVE-XAM://BOX_VIM?".ToLower(CultureInfo.InvariantCulture));
            sf.Type.Vim.Add("box_vim");

            sf.Features = new List<FeatureUnit> {
                new FeatureUnit() {Vim = "box_vim", DisplayName = BoxI18N.ResourceManager.GetString("MediaStorage_Box_RootFolderName", culture), Tag = "Box_Root_Folder_Name".ToLower(CultureInfo.InvariantCulture), Key = "Root_Folder_Name".ToLower(CultureInfo.InvariantCulture), KeyName = "BoxRootFolderName", Visibility="Visible", ValType = "string", GuiType = "TextBox"},
                new FeatureUnit() {Vim = "box_vim", DisplayName = BoxI18N.ResourceManager.GetString("MediaStorage_Box_BoxAPIKey", culture), Tag = "box", Key = "boxAPIKey".ToLower(CultureInfo.InvariantCulture), KeyName = "BoxAPIKey", Visibility="Visible", ValType = "string", GuiType = "TextBox"},
            };
            Add(sf);


        }

        private static string authorizeUrl = String.Format("https://www.box.com/api/oauth2/authorize?response_type=code&client_id={0}", "6wlvcp6l8tujowomdwrbjtqlwhdxzqfq");

        protected override void GenerateDocAveGUIFeatureUnit(CultureInfo culture)
        {
            StorageFeature sf = new StorageFeature();
            StorageType type = new StorageType();
            type.Value = "Box";
            type.Display = BoxI18N.ResourceManager.GetString("MediaStorage_Box_Box", culture);
            type.Index = 9;
            type.Vim = new List<string>() { "box_vim" };
            type.IsSupportMovableRetention = false;
            sf.Type = type;
            sf.Type.DefaultXris.Add("DOCAVE-XAM://BOX_VIM?".ToLower(CultureInfo.InvariantCulture));
            sf.IsNeedSpaceThreshold = true;
            sf.Type.Vim.Add("box_vim");

            sf.Features = new List<FeatureUnit>
            {
                new FeatureUnit() {Vim = "box_vim", DisplayName = BoxI18N.ResourceManager.GetString("MediaStorage_Box_RootFolderName", culture), Tag = "Box_Root_Folder_Name".ToLower(CultureInfo.InvariantCulture), Key = "Root_Folder_Name".ToLower(CultureInfo.InvariantCulture), KeyName = "BoxRootFolderName", Visibility="Visible", ValType = "string", IsRequiredOption = true, GuiType = "TextBox"
                , ValidateRegPats = new List<String>(){@"^([^\\]+\\){0,9}[^\\]+$" + "\t0\t" + BoxI18N.ResourceManager.GetString("MediaStorage_Box_RootFolderName_Can_Not_Exceed_10_Level", culture)}, DemoValue = BoxI18N.ResourceManager.GetString("MediaStorage_Box_RootFolderName_Demo", culture)},
                new FeatureUnit() {Vim = "box_vim", DisplayName = BoxI18N.ResourceManager.GetString("MediaStorage_Box_EmailAddress", culture), Tag = "Box_Email_Address".ToLower(CultureInfo.InvariantCulture), Key = "boxEmailAddress".ToLower(CultureInfo.InvariantCulture), KeyName = "BoxEmailAddress", Visibility="Visible", ValType = "string", IsRequiredOption = true, GuiType = "TextBox", ValidateRegPats = new List<string>()
                {
                    @"\w+([-+.]\w+)*@\w+([-.]\w+)*\.\w+([-.]\w+)*" + "\t0\t" + BoxI18N.ResourceManager.GetString("MediaStorage_Box_Email_format_not_correct", culture)
                }},
                new FeatureUnit() {Vim = "box_vim", DisplayName = BoxI18N.ResourceManager.GetString("MediaStorage_Box_Config_Location", culture), Tag = "Box_Config_Location".ToLower(CultureInfo.InvariantCulture), Key = "boxConfigLocation".ToLower(CultureInfo.InvariantCulture), KeyName = "BoxConfigLocation", Visibility="Visible", ValType = "string", IsRequiredOption = true, GuiType = "TextBox", ValidateRegPats = new List<string>(){
                    @"^\\\\[^\\]+\\[^\\]+(\\[^\\]+)*\\?$|^$" + "\t0\t" + BoxI18N.ResourceManager.GetString("MediaStorage_Box_The_path_format_is_not_correct", culture)}},
                new FeatureUnit() {Vim = "box_vim", DisplayName = BoxI18N.ResourceManager.GetString("MediaStorage_Box_Config_Username", culture), Tag = "Box_Config_Username".ToLower(CultureInfo.InvariantCulture), Key = "boxConfigUsername".ToLower(CultureInfo.InvariantCulture), KeyName = "BoxConfigUsername", Visibility="Visible", ValType = "string", IsRequiredOption = true, GuiType = "TextBox", ValidateRegPats = new List<string>(){
                    @"^[^\\@]+[\\@]{1}[^\\@]+$" + "\t0\t" + BoxI18N.ResourceManager.GetString("MediaStorage_Box_Please_enter_the_username_in_the_format_domain_username", culture),
                }},
                new FeatureUnit() {Vim = "box_vim", DisplayName = BoxI18N.ResourceManager.GetString("MediaStorage_Box_Config_Password", culture), Tag = "Box_Config_Password".ToLower(CultureInfo.InvariantCulture), Key = "boxConfigPasswordSecret".ToLower(CultureInfo.InvariantCulture), KeyName = "BoxConfigPassword", Visibility="Visible", ValType = "string", IsRequiredOption = true, GuiType = "PasswordBox"},
                new FeatureUnit() {Vim = "box_vim", DisplayName = BoxI18N.ResourceManager.GetString("MediaStorage_Box_EnableAsUser", culture), Tag = "Box_Vim_AsUser".ToLower(CultureInfo.InvariantCulture), Key = "boxAsUser".ToLower(CultureInfo.InvariantCulture), Value = "box_vim_login",  KeyName = "BoxAsUser", Visibility="Visible", ValType = "string", GuiType = "CheckBox", ChildFeatures = new List<FeatureUnit>(){
                   new FeatureUnit() {Vim = "box_vim", DisplayName = BoxI18N.ResourceManager.GetString("MediaStorage_Box_AsUser", culture), Tag = "box_vim_login".ToLower(CultureInfo.InvariantCulture), Key = "boxManagedUserName".ToLower(CultureInfo.InvariantCulture), KeyName = "boxManagedUserName", Visibility="Collapsed", ValType = "string", IsRequiredOption = true, GuiType = "TextBox", ValidateRegPats = new List<string>()
                {
                    @"\w+([-+.]\w+)*@\w+([-.]\w+)*\.\w+([-.]\w+)*" + "\t0\t" + BoxI18N.ResourceManager.GetString("MediaStorage_Box_Email_format_not_correct", culture)
                }},
                }},
                new FeatureUnit() {Vim = "box_vim", DisplayName = BoxI18N.ResourceManager.GetString("MediaStorage_Box_Default_Application" ,culture), Tag = "Box_Radio_Button".ToLower(CultureInfo.InvariantCulture), Key = "Box_default".ToLower(CultureInfo.InvariantCulture), KeyName = "Box_DefaultName", Visibility="Visible", ValType = "string", GuiType = "RadioButton", Value = "true", ChildFeatures = new List<FeatureUnit>(){
                    new FeatureUnit() {Vim = "box_vim".ToLower(CultureInfo.InvariantCulture), Tag = "Box_default".ToLower(CultureInfo.InvariantCulture), Key = "Box_Retrieve_Token", DisplayName = BoxI18N.ResourceManager.GetString("MediaStorage_Box_Retrieve_Token" ,culture), Visibility="Visible", ValType = "string", GuiType = "Hyperlink", Value = authorizeUrl},
                }},   
                new FeatureUnit() {Vim = "box_vim", DisplayName = BoxI18N.ResourceManager.GetString("MediaStorage_Box_Customized_Application" ,culture), Tag = "Box_Radio_Button".ToLower(CultureInfo.InvariantCulture), Key = "Box_customized".ToLower(CultureInfo.InvariantCulture), KeyName = "Box_CustomizedName", Visibility="Visible", ValType = "string", GuiType = "RadioButton"}, 
                new FeatureUnit() {Vim = "box_vim", DisplayName = BoxI18N.ResourceManager.GetString("MediaStorage_Box_Proxy", culture), Tag = "box_Proxy".ToLower(CultureInfo.InvariantCulture), Key = "box_Proxy".ToLower(CultureInfo.InvariantCulture), Value = "box_Proxy_setting".ToLower(CultureInfo.InvariantCulture),  KeyName = "boxProxySetting", Visibility="Visible", ValType = "string", GuiType = "CheckBox", ChildFeatures = new List<FeatureUnit>(){
                    new FeatureUnit() {Vim = "box_vim", DisplayName = BoxI18N.ResourceManager.GetString("MediaStorage_Box_Proxy_Host", culture), Tag = "box_Proxy_setting".ToLower(CultureInfo.InvariantCulture), Key = "ProxyIp".ToLower(CultureInfo.InvariantCulture), KeyName = "BoxProxyHost", Visibility="Collapsed", ValType = "string", IsRequiredOption = true, GuiType = "TextBox"},
                    new FeatureUnit() {Vim = "box_vim", DisplayName = BoxI18N.ResourceManager.GetString("MediaStorage_Box_Proxy_Port", culture), Tag = "box_Proxy_setting".ToLower(CultureInfo.InvariantCulture), Key = "ProxyPort".ToLower(CultureInfo.InvariantCulture), KeyName = "BoxProxyPort", Visibility="Collapsed", ValType = "string", IsRequiredOption = true, GuiType = "TextBox"},
                    new FeatureUnit() {Vim = "box_vim", DisplayName = BoxI18N.ResourceManager.GetString("MediaStorage_Box_Proxy_Username", culture), Tag = "box_Proxy_setting".ToLower(CultureInfo.InvariantCulture), Key = "ProxyUsername".ToLower(CultureInfo.InvariantCulture), KeyName = "BoxProxyUsername", Visibility="Collapsed", ValType = "string", IsRequiredOption = false, GuiType = "TextBox", CanNullOrEmpty = "true"},
                    new FeatureUnit() {Vim = "box_vim", DisplayName = BoxI18N.ResourceManager.GetString("MediaStorage_Box_Proxy_Password", culture), Tag = "box_Proxy_setting".ToLower(CultureInfo.InvariantCulture), Key = "ProxyPasswordSecret".ToLower(CultureInfo.InvariantCulture), KeyName = "BoxProxyPasswordSecret", Visibility="Collapsed", ValType = "string", IsRequiredOption = false, GuiType = "PasswordBox", CanNullOrEmpty = "true"},
                }},
                new FeatureUnit() {Vim = "box_vim", DisplayName = BoxI18N.ResourceManager.GetString("MediaStorage_Box_Advanced", culture), Tag = "box_vim_advanced".ToLower(CultureInfo.InvariantCulture), Key = "advanced".ToLower(CultureInfo.InvariantCulture), Value = "box_vim_ExtendedParameters".ToLower(CultureInfo.InvariantCulture),  KeyName = "BoxAdvanced", Visibility="Visible", ValType = "string", GuiType = "CheckBox", ChildFeatures = new List<FeatureUnit>(){
                   new FeatureUnit() {Vim = "box_vim", DisplayName = BoxI18N.ResourceManager.GetString("MediaStorage_Box_Extended_parameters", culture), Tag = "box_vim_ExtendedParameters".ToLower(CultureInfo.InvariantCulture), Key = "ExtendedParameters".ToLower(CultureInfo.InvariantCulture), KeyName = "BoxExtendedParameters", Visibility="Collapsed", ValType = "string", GuiType = "TextArea", CanNullOrEmpty = "true"}
                }}
             };
            Add(sf);
        }

        protected override void GenerateConnectorGUIFeatureUnit(CultureInfo culture)
        {
            StorageFeature sf = new StorageFeature();
            StorageType type = new StorageType();
            type.Value = "Box";
            type.Display = BoxI18N.ResourceManager.GetString("MediaStorage_Box_Box", culture);
            type.Index = 9;
            type.Vim = new List<string>() { "box_vim" };
            sf.Type = type;
            sf.Type.DefaultXris.Add("DOCAVE-XAM://BOX_VIM?".ToLower(CultureInfo.InvariantCulture));
            sf.Type.Vim.Add("box_vim");
            sf.Features = new List<FeatureUnit> 
            {
                new FeatureUnit()
                {
                    Vim = "box_vim", DisplayName = BoxI18N.ResourceManager.GetString("MediaStorage_Connector_Box_RootFolderName", culture), Tag = "Box_Root_Folder_Name".ToLower(CultureInfo.InvariantCulture), Key = "Root_Folder_Name".ToLower(CultureInfo.InvariantCulture), KeyName = "BoxRootFolderName", Visibility="Visible", ValType = "string", IsRequiredOption = true, GuiType = "TextBox", FeatureFlag=(int)(FeatureUnitFlag.CloudContainer|FeatureUnitFlag.Path), CanModifi = false
                , ValidateRegPats = new List<String>(){@"^([^\\]+\\){0,9}[^\\]+$" + "\t0\t" + BoxI18N.ResourceManager.GetString("MediaStorage_Connector_Box_RootFolderName_Can_Not_Exceed_10_Level", culture)}, DemoValue = BoxI18N.ResourceManager.GetString("MediaStorage_Connector_Box_RootFolderName_Demo", culture)},
                new FeatureUnit() {Vim = "box_vim", DisplayName = BoxI18N.ResourceManager.GetString("MediaStorage_Connector_Box_EmailAddress", culture), Tag = "Box_Email_Address".ToLower(CultureInfo.InvariantCulture), Key = "boxEmailAddress".ToLower(CultureInfo.InvariantCulture), KeyName = "BoxEmailAddress", Visibility="Visible", ValType = "string", IsRequiredOption = true, GuiType = "TextBox", CanModifi = false, ValidateRegPats = new List<string>()
                {
                    @"\w+([-+.]\w+)*@\w+([-.]\w+)*\.\w+([-.]\w+)*" + "\t0\t" + BoxI18N.ResourceManager.GetString("MediaStorage_Connector_Box_Email_format_not_correct", culture)
                }},
                new FeatureUnit() {Vim = "box_vim", DisplayName = BoxI18N.ResourceManager.GetString("MediaStorage_Connector_Box_Config_Location", culture), Tag = "Box_Config_Location".ToLower(CultureInfo.InvariantCulture), Key = "boxConfigLocation".ToLower(CultureInfo.InvariantCulture), KeyName = "BoxConfigLocation", Visibility="Visible", ValType = "string", IsRequiredOption = true, GuiType = "TextBox", ValidateRegPats = new List<string>(){
                    @"^\\\\[^\\]+\\[^\\]+(\\[^\\]+)*\\?$|^$" + "\t0\t" + BoxI18N.ResourceManager.GetString("MediaStorage_Connector_Box_The_path_format_is_not_correct", culture)}},
                new FeatureUnit() {Vim = "box_vim", DisplayName = BoxI18N.ResourceManager.GetString("MediaStorage_Connector_Box_Config_Username", culture), Tag = "Box_Config_Username".ToLower(CultureInfo.InvariantCulture), Key = "boxConfigUsername".ToLower(CultureInfo.InvariantCulture), KeyName = "BoxConfigUsername", Visibility="Visible", ValType = "string", IsRequiredOption = true, GuiType = "TextBox", ValidateRegPats = new List<string>(){
                    @"^[^\\@]+[\\@]{1}[^\\@]+$" + "\t0\t" + BoxI18N.ResourceManager.GetString("MediaStorage_Connector_Box_Please_enter_the_username_in_the_format_domain_username", culture),
                }},
                new FeatureUnit() {Vim = "box_vim", DisplayName = BoxI18N.ResourceManager.GetString("MediaStorage_Connector_Box_Config_Password", culture), Tag = "Box_Config_Password".ToLower(CultureInfo.InvariantCulture), Key = "boxConfigPasswordSecret".ToLower(CultureInfo.InvariantCulture), KeyName = "BoxConfigPassword", Visibility="Visible", ValType = "string", IsRequiredOption = true, GuiType = "PasswordBox"},
                new FeatureUnit() {IsRequiredOption = true, Vim = "box_vim", DisplayName = BoxI18N.ResourceManager.GetString("MediaStorage_Connector_Box_EnableAsUser", culture), Tag = "Box_Vim_AsUser".ToLower(CultureInfo.InvariantCulture), Key = "boxAsUser".ToLower(CultureInfo.InvariantCulture), Value = "box_vim_login",  KeyName = "BoxAsUser", Visibility="Visible", ValType = "string", GuiType = "CheckBox", ChildFeatures = new List<FeatureUnit>(){
                   new FeatureUnit() {Vim = "box_vim", DisplayName = BoxI18N.ResourceManager.GetString("MediaStorage_Connector_Box_AsUser", culture), Tag = "box_vim_login".ToLower(CultureInfo.InvariantCulture), Key = "boxManagedUserName".ToLower(CultureInfo.InvariantCulture), KeyName = "boxManagedUserName", Visibility="Collapsed", ValType = "string", IsRequiredOption = true, GuiType = "TextBox", ValidateRegPats = new List<string>()
                {
                    @"\w+([-+.]\w+)*@\w+([-.]\w+)*\.\w+([-.]\w+)*" + "\t0\t" + BoxI18N.ResourceManager.GetString("MediaStorage_Connector_Box_Email_format_not_correct", culture)
                }},
                }},
                new FeatureUnit() {IsRequiredOption = true, Vim = "box_vim", DisplayName = BoxI18N.ResourceManager.GetString("MediaStorage_Connector_Box_Default_Application" ,culture), Tag = "Box_Radio_Button".ToLower(CultureInfo.InvariantCulture), Key = "Box_default".ToLower(CultureInfo.InvariantCulture), KeyName = "Box_DefaultName", Visibility="Visible", ValType = "string", GuiType = "RadioButton", Value = "true", ChildFeatures = new List<FeatureUnit>(){
                    new FeatureUnit() {IsRequiredOption = true, Vim = "box_vim".ToLower(CultureInfo.InvariantCulture), Tag = "Box_default".ToLower(CultureInfo.InvariantCulture), Key = "Box_Retrieve_Token", DisplayName = BoxI18N.ResourceManager.GetString("MediaStorage_Connector_Box_Retrieve_Token" ,culture), Visibility="Visible", ValType = "string", GuiType = "Hyperlink", Value = authorizeUrl},
                }},   
                new FeatureUnit() {IsRequiredOption = true, Vim = "box_vim", DisplayName = BoxI18N.ResourceManager.GetString("MediaStorage_Connector_Box_Customized_Application" ,culture), Tag = "Box_Radio_Button".ToLower(CultureInfo.InvariantCulture), Key = "Box_customized".ToLower(CultureInfo.InvariantCulture), KeyName = "Box_CustomizedName", Visibility="Visible", ValType = "string", GuiType = "RadioButton"}, 
                new FeatureUnit() {Vim = "box_vim", DisplayName = BoxI18N.ResourceManager.GetString("MediaStorage_Connector_Box_Proxy", culture), Tag = "box_Proxy".ToLower(CultureInfo.InvariantCulture), Key = "box_Proxy".ToLower(CultureInfo.InvariantCulture), Value = "box_Proxy_setting".ToLower(CultureInfo.InvariantCulture),  KeyName = "boxProxySetting", Visibility="Visible", ValType = "string", GuiType = "CheckBox", ChildFeatures = new List<FeatureUnit>(){
                    new FeatureUnit() {Vim = "box_vim", DisplayName = BoxI18N.ResourceManager.GetString("MediaStorage_Connector_Box_Proxy_Host", culture), Tag = "box_Proxy_setting".ToLower(CultureInfo.InvariantCulture), Key = "ProxyIp".ToLower(CultureInfo.InvariantCulture), KeyName = "BoxProxyHost", Visibility="Collapsed", ValType = "string", IsRequiredOption = true, GuiType = "TextBox"},
                    new FeatureUnit() {Vim = "box_vim", DisplayName = BoxI18N.ResourceManager.GetString("MediaStorage_Connector_Box_Proxy_Port", culture), Tag = "box_Proxy_setting".ToLower(CultureInfo.InvariantCulture), Key = "ProxyPort".ToLower(CultureInfo.InvariantCulture), KeyName = "BoxProxyPort", Visibility="Collapsed", ValType = "string", IsRequiredOption = true, GuiType = "TextBox",ValidateRegPats = new List<string>(){
                            @"(^[0-9]\d{0,3}$)|(^[1-5]\d{4}$)|(^6[0-4]\d{3}$)|(^65[0-4]\d{2}$)|(^655[0-2]\d$)|(^6553[0-5]$)"+ "\t0\t"+BoxI18N.ResourceManager.GetString("MediaStorage_Connector_Box_Must_be_a_number_between_0_and_65535", culture)
                            }},
                    new FeatureUnit() {Vim = "box_vim", DisplayName = BoxI18N.ResourceManager.GetString("MediaStorage_Connector_Box_Proxy_Username", culture), Tag = "box_Proxy_setting".ToLower(CultureInfo.InvariantCulture), Key = "ProxyUsername".ToLower(CultureInfo.InvariantCulture), KeyName = "BoxProxyUsername", Visibility="Collapsed", ValType = "string", IsRequiredOption = false, GuiType = "TextBox", CanNullOrEmpty = "true"},
                    new FeatureUnit() {Vim = "box_vim", DisplayName = BoxI18N.ResourceManager.GetString("MediaStorage_Connector_Box_Proxy_Password", culture), Tag = "box_Proxy_setting".ToLower(CultureInfo.InvariantCulture), Key = "ProxyPasswordSecret".ToLower(CultureInfo.InvariantCulture), KeyName = "BoxProxyPasswordSecret", Visibility="Collapsed", ValType = "string", IsRequiredOption = false, GuiType = "PasswordBox", CanNullOrEmpty = "true"},
                }},
                new FeatureUnit() {IsRequiredOption = true, Vim = "box_vim", DisplayName = BoxI18N.ResourceManager.GetString("MediaStorage_Connector_Box_Advanced", culture), Tag = "box_vim_advanced".ToLower(CultureInfo.InvariantCulture), Key = "advanced".ToLower(CultureInfo.InvariantCulture), Value = "box_vim_ExtendedParameters".ToLower(CultureInfo.InvariantCulture),  KeyName = "BoxAdvanced", Visibility="Visible", ValType = "string", GuiType = "CheckBox", ChildFeatures = new List<FeatureUnit>(){
                   new FeatureUnit() {IsRequiredOption = true, Vim = "box_vim", DisplayName = BoxI18N.ResourceManager.GetString("MediaStorage_Connector_Box_Extended_parameters", culture), Tag = "box_vim_ExtendedParameters".ToLower(CultureInfo.InvariantCulture), Key = "ExtendedParameters".ToLower(CultureInfo.InvariantCulture), KeyName = "BoxExtendedParameters", Visibility="Collapsed", ValType = "string", GuiType = "TextArea", CanNullOrEmpty = "true"}
                }}
             };
            Add(sf);
        }


        /// <summary>
        /// 供VIM调用, 获取相应的Feature
        /// </summary>
        private static readonly Dictionary<string, BoxFeature> instances = new Dictionary<string, BoxFeature>();
        private static Object locker = new Object();
        public static BoxFeature Getstances(int type, string culture = "en")
        {
            lock (locker)
            {
                if (!instances.ContainsKey(type + culture))
                {
                    BoxFeature box = new BoxFeature(type, culture);

                    foreach (var feature in box.FeatureObjs)
                    {
                        feature.AdvancedOptions.AddRange(new List<string>()
                    {
                        "RetryInterval=30000",//30s
                        "RetryCount=6"
                    });
                    }

                    instances[type + culture] = box;
                    return box;
                }
                else
                {
                    return instances[type + culture];
                }
            }
        }
    }
}
