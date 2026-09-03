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

namespace AvePoint.Media.Storage.Cloud.Dropbox
{
    #region using
    using AvePoint.Media.Storage.Resources.DropboxI18N;
    using System;
    using System.Collections.Generic;
    using System.Globalization;
    #endregion

    class DropboxFeature : StorageFeature
    {
        /// <summary>
        /// 供VIM调用, 获取相应的Feature
        /// </summary>
        private static readonly Dictionary<String, DropboxFeature> instances = new Dictionary<String, DropboxFeature>();
        private static Object locker = new Object();
        private static String authorizeUrl = String.Format("https://www.dropbox.com/oauth2/authorize?redirect_uri={0}&client_id={1}&response_type=code", "https://www.avepointonlineservices.com/getcloudtoken/dropbox", "p9kxswndtb7f6gp");

        /// <summary>
        /// 私有构造函数， 此类不允许在外部实例化。 <see cref="RackspaceFeature"/> class.
        /// </summary>
        private DropboxFeature(int type, string culture)
        {
            this.Init(type, culture);
        }

        protected override void GenerateSingleTypeFeatureUnit(CultureInfo culture)
        {
            //TODO
        }

        protected override void GenerateConnectorGUIFeatureUnit(CultureInfo culture)
        {
            var sf = new StorageFeature();
            var type = new StorageType();
            type.Value = "Dropbox";
            type.Display = DropboxI18N.ResourceManager.GetString("MediaStorage_Connector_Dropbox_Dropbox", culture);
            type.Index = 407;
            type.Vim = new List<String>() { "dropbox_vim" };
            sf.Type = type;
            sf.Description = DropboxI18N.ResourceManager.GetString("MediaStorage_Connector_Dropbox_Configure_your_root_folder_App_key_and_token_access", culture);
            sf.Features = new List<FeatureUnit> {
                new FeatureUnit() {IsRequiredOption = true, Vim = "dropbox_vim", DisplayName = DropboxI18N.ResourceManager.GetString("MediaStorage_Connector_Dropbox_Root_folder" ,culture), Tag = "cloud_dropbox", Key = "containerName".ToLower(CultureInfo.InvariantCulture), KeyName = "DropboxContainerName", Visibility="Visible", ValType = "string", GuiType = "TextBox", FeatureFlag=(Int32)(FeatureUnitFlag.CloudContainer|FeatureUnitFlag.Path), CanModifi = false, 
                        ValidateRegPats = new List<String>(){@"^.{0,200}$" + "\t0\t" + DropboxI18N.ResourceManager.GetString("MediaStorage_Connector_Dropbox_root_folder_cannot_exceed_200_characters", culture)
                        , @"^([^\\]+\/){0,9}[^\\]+$" + "\t0\t" + DropboxI18N.ResourceManager.GetString("MediaStorage_Connector_Dropbox_RootFolderName_Invalid_Format", culture)}, DemoValue = DropboxI18N.ResourceManager.GetString("MediaStorage_Connector_Dropbox_RootFolderName_Demo", culture)},
                new FeatureUnit() {IsRequiredOption = true, Vim = "dropbox_vim", DisplayName = DropboxI18N.ResourceManager.GetString("MediaStorage_Connector_Dropbox_Access_Token" ,culture), Tag = "cloud_dropbox", Key = "DropboxAccessTokenSecret".ToLower(CultureInfo.InvariantCulture), KeyName = "DropboxTokenAccess", Visibility="Visible", ValType = "string", GuiType = "PasswordBox"},
                new FeatureUnit() {IsRequiredOption = true, Vim = "dropbox_vim", DisplayName = DropboxI18N.ResourceManager.GetString("MediaStorage_Connector_Dropbox_Default_Application" ,culture), Tag = "cloud_dropbox", Key = "cloud_dropbox_default".ToLower(CultureInfo.InvariantCulture), KeyName = "cloud_dropbox_DefaultName", Visibility="Visible", ValType = "string", GuiType = "RadioButton", Value = "true", ChildFeatures = new List<FeatureUnit>(){
                    new FeatureUnit() {IsRequiredOption = true, Vim = "dropbox_vim".ToLower(CultureInfo.InvariantCulture), Tag = "cloud_dropbox_default", Key = "Dropbox_Retrieve_Token", DisplayName = DropboxI18N.ResourceManager.GetString("MediaStorage_Connector_Dropbox_Retrieve_Token" ,culture), Visibility="Visible", ValType = "string", GuiType = "Hyperlink", Value = authorizeUrl},
                }},   
                new FeatureUnit() {IsRequiredOption = true, Vim = "dropbox_vim", DisplayName = DropboxI18N.ResourceManager.GetString("MediaStorage_Connector_Dropbox_Customized_Application" ,culture), Tag = "cloud_dropbox", Key = "cloud_dropbox_customized".ToLower(CultureInfo.InvariantCulture), KeyName = "cloud_dropbox_CustomizedName", Visibility="Visible", ValType = "string", GuiType = "RadioButton"}, 
                new FeatureUnit() {Vim = "dropbox_vim", DisplayName = DropboxI18N.ResourceManager.GetString("MediaStorage_Connector_Dropbox_Proxy", culture), Tag = "cloud_dropbox_Proxy".ToLower(CultureInfo.InvariantCulture), Key = "cloud_dropbox_Proxy".ToLower(CultureInfo.InvariantCulture), Value = "cloud_dropbox_Proxy_setting".ToLower(CultureInfo.InvariantCulture),  KeyName = "dropboxProxySetting", Visibility="Visible", ValType = "string", GuiType = "CheckBox", ChildFeatures = new List<FeatureUnit>(){
                    new FeatureUnit() {Vim = "dropbox_vim", DisplayName = DropboxI18N.ResourceManager.GetString("MediaStorage_Connector_Dropbox_Proxy_Host", culture), Tag = "cloud_dropbox_Proxy_setting".ToLower(CultureInfo.InvariantCulture), Key = "ProxyIp".ToLower(CultureInfo.InvariantCulture), KeyName = "DropboxProxyHost", Visibility="Collapsed", ValType = "string", IsRequiredOption = true, GuiType = "TextBox"},
                    new FeatureUnit() {Vim = "dropbox_vim", DisplayName = DropboxI18N.ResourceManager.GetString("MediaStorage_Connector_Dropbox_Proxy_Port", culture), Tag = "cloud_dropbox_Proxy_setting".ToLower(CultureInfo.InvariantCulture), Key = "ProxyPort".ToLower(CultureInfo.InvariantCulture), KeyName = "DropboxProxyPort", Visibility="Collapsed", ValType = "string", IsRequiredOption = true, GuiType = "TextBox",ValidateRegPats = new List<string>(){
                            @"(^[0-9]\d{0,3}$)|(^[1-5]\d{4}$)|(^6[0-4]\d{3}$)|(^65[0-4]\d{2}$)|(^655[0-2]\d$)|(^6553[0-5]$)"+ "\t0\t"+DropboxI18N.ResourceManager.GetString("MediaStorage_Connector_Dropbox_Must_be_a_number_between_0_and_65535", culture)
                            }},
                    new FeatureUnit() {Vim = "dropbox_vim", DisplayName = DropboxI18N.ResourceManager.GetString("MediaStorage_Connector_Dropbox_Proxy_Username", culture), Tag = "cloud_dropbox_Proxy_setting".ToLower(CultureInfo.InvariantCulture), Key = "ProxyUsername".ToLower(CultureInfo.InvariantCulture), KeyName = "DropboxProxyUsername", Visibility="Collapsed", ValType = "string", IsRequiredOption = false, GuiType = "TextBox", CanNullOrEmpty = "true"},
                    new FeatureUnit() {Vim = "dropbox_vim", DisplayName = DropboxI18N.ResourceManager.GetString("MediaStorage_Connector_Dropbox_Proxy_Password", culture), Tag = "cloud_dropbox_Proxy_setting".ToLower(CultureInfo.InvariantCulture), Key = "ProxyPasswordSecret".ToLower(CultureInfo.InvariantCulture), KeyName = "DropboxProxyPasswordSecret", Visibility="Collapsed", ValType = "string", IsRequiredOption = false, GuiType = "PasswordBox", CanNullOrEmpty = "true"},
                }},
                new FeatureUnit() {IsRequiredOption = true, Vim = "dropbox_vim", DisplayName = DropboxI18N.ResourceManager.GetString("MediaStorage_Connector_Dropbox_Advanced" ,culture), Tag = "cloud_dropbox", Key = "advanced".ToLower(CultureInfo.InvariantCulture), Value = "dropbox_vim_ExtendedParameters",  KeyName = "DropboxAdvanced", Visibility="Visible", ValType = "string", GuiType = "CheckBox", ChildFeatures = new List<FeatureUnit>(){
                    new FeatureUnit() {IsRequiredOption = true, Vim = "dropbox_vim", DisplayName = DropboxI18N.ResourceManager.GetString("MediaStorage_Connector_Dropbox_Extended_Parameters" ,culture),Tag = "dropbox_vim_ExtendedParameters", Key = "ExtendedParameters".ToLower(CultureInfo.InvariantCulture), KeyName = "DropboxExtendParameters", Visibility="Collapsed", ValType = "string", GuiType = "TextArea", CanNullOrEmpty = "true"}}}
            };
            Add(sf);
        }

        protected override void GenerateDocAveGUIFeatureUnit(CultureInfo culture)
        {
            //将dropbox单独的拿出来
            var sf = new StorageFeature();
            var type = new StorageType();
            type.Value = "Dropbox";
            type.Display = DropboxI18N.ResourceManager.GetString("MediaStorage_Dropbox_Dropbox", culture);
            type.Index = 407;
            type.Vim = new List<String>() { "dropbox_vim" };
            sf.Type = type;
            sf.IsNeedSpaceThreshold = true;
            sf.Type.DefaultXris.Add("DOCAVE-XAM://DROPBOX_VIM?".ToLower(CultureInfo.InvariantCulture));
            sf.Features = new List<FeatureUnit> {
                new FeatureUnit() {Vim = "dropbox_vim", DisplayName = DropboxI18N.ResourceManager.GetString("MediaStorage_Dropbox_Root_folder" ,culture), Tag = "cloud_dropbox", Key = "containerName".ToLower(CultureInfo.InvariantCulture), KeyName = "DropboxContainerName", Visibility="Visible", ValType = "string", IsRequiredOption = true, GuiType = "TextBox", DemoValue = DropboxI18N.ResourceManager.GetString("MediaStorage_Dropbox_RootFolderName_Demo", culture)
                ,ValidateRegPats = new List<String>{@"^([^\\]+\/){0,9}[^\\]+$" + "\t0\t" + DropboxI18N.ResourceManager.GetString("MediaStorage_Dropbox_RootFolderName_Invalid_Format", culture)}}, 
                new FeatureUnit() {Vim = "dropbox_vim", DisplayName = DropboxI18N.ResourceManager.GetString("MediaStorage_Dropbox_Access_Token" ,culture), Tag = "cloud_dropbox", Key = "DropboxAccessTokenSecret".ToLower(CultureInfo.InvariantCulture), KeyName = "DropboxTokenAccess", Visibility="Visible", ValType = "string", IsRequiredOption = true, GuiType = "PasswordBox"},
                new FeatureUnit() {Vim = "dropbox_vim", DisplayName = DropboxI18N.ResourceManager.GetString("MediaStorage_Dropbox_Default_Application" ,culture), Tag = "cloud_dropbox", Key = "cloud_dropbox_default".ToLower(CultureInfo.InvariantCulture), KeyName = "cloud_dropbox_DefaultName", Visibility="Visible", ValType = "string", GuiType = "RadioButton", Value = "true", ChildFeatures = new List<FeatureUnit>(){
                    new FeatureUnit() {Vim = "dropbox_vim".ToLower(CultureInfo.InvariantCulture), Tag = "cloud_dropbox_default", Key = "Dropbox_Retrieve_Token", DisplayName = DropboxI18N.ResourceManager.GetString("MediaStorage_Dropbox_Retrieve_Token" ,culture), Visibility="Visible", ValType = "string", GuiType = "Hyperlink", Value = authorizeUrl},
                }},
                new FeatureUnit() {Vim = "dropbox_vim", DisplayName = DropboxI18N.ResourceManager.GetString("MediaStorage_Dropbox_Customized_Application" ,culture), Tag = "cloud_dropbox", Key = "cloud_dropbox_customized".ToLower(CultureInfo.InvariantCulture), KeyName = "cloud_dropbox_CustomizedName", Visibility="Visible", ValType = "string", GuiType = "RadioButton"}, 
                new FeatureUnit() {Vim = "dropbox_vim", DisplayName = DropboxI18N.ResourceManager.GetString("MediaStorage_Dropbox_Proxy", culture), Tag = "cloud_dropbox_Proxy".ToLower(CultureInfo.InvariantCulture), Key = "cloud_dropbox_Proxy".ToLower(CultureInfo.InvariantCulture), Value = "cloud_dropbox_Proxy_setting".ToLower(CultureInfo.InvariantCulture),  KeyName = "dropboxProxySetting", Visibility="Visible", ValType = "string", GuiType = "CheckBox", ChildFeatures = new List<FeatureUnit>(){
                    new FeatureUnit() {Vim = "dropbox_vim", DisplayName = DropboxI18N.ResourceManager.GetString("MediaStorage_Dropbox_Proxy_Host", culture), Tag = "cloud_dropbox_Proxy_setting".ToLower(CultureInfo.InvariantCulture), Key = "ProxyIp".ToLower(CultureInfo.InvariantCulture), KeyName = "DropboxProxyHost", Visibility="Collapsed", ValType = "string", IsRequiredOption = true, GuiType = "TextBox"},
                    new FeatureUnit() {Vim = "dropbox_vim", DisplayName = DropboxI18N.ResourceManager.GetString("MediaStorage_Dropbox_Proxy_Port", culture), Tag = "cloud_dropbox_Proxy_setting".ToLower(CultureInfo.InvariantCulture), Key = "ProxyPort".ToLower(CultureInfo.InvariantCulture), KeyName = "DropboxProxyPort", Visibility="Collapsed", ValType = "string", IsRequiredOption = true, GuiType = "TextBox"},
                    new FeatureUnit() {Vim = "dropbox_vim", DisplayName = DropboxI18N.ResourceManager.GetString("MediaStorage_Dropbox_Proxy_Username", culture), Tag = "cloud_dropbox_Proxy_setting".ToLower(CultureInfo.InvariantCulture), Key = "ProxyUsername".ToLower(CultureInfo.InvariantCulture), KeyName = "DropboxProxyUsername", Visibility="Collapsed", ValType = "string", IsRequiredOption = false, GuiType = "TextBox", CanNullOrEmpty = "true"},
                    new FeatureUnit() {Vim = "dropbox_vim", DisplayName = DropboxI18N.ResourceManager.GetString("MediaStorage_Dropbox_Proxy_Password", culture), Tag = "cloud_dropbox_Proxy_setting".ToLower(CultureInfo.InvariantCulture), Key = "ProxyPasswordSecret".ToLower(CultureInfo.InvariantCulture), KeyName = "DropboxProxyPasswordSecret", Visibility="Collapsed", ValType = "string", IsRequiredOption = false, GuiType = "PasswordBox", CanNullOrEmpty = "true"},
                }},
                new FeatureUnit() {Vim = "dropbox_vim", DisplayName = DropboxI18N.ResourceManager.GetString("MediaStorage_Dropbox_Advanced" ,culture), Tag = "cloud_dropbox", Key = "advanced".ToLower(CultureInfo.InvariantCulture), Value = "dropbox_vim_ExtendedParameters",  KeyName = "DropboxAdvanced", Visibility="Visible", ValType = "string", GuiType = "CheckBox", ChildFeatures = new List<FeatureUnit>(){
                    new FeatureUnit() {Vim = "dropbox_vim", DisplayName = DropboxI18N.ResourceManager.GetString("MediaStorage_Dropbox_Extended_Parameters" ,culture),Tag = "dropbox_vim_ExtendedParameters", Key = "ExtendedParameters".ToLower(CultureInfo.InvariantCulture), KeyName = "DropboxExtendParameters", Visibility="Collapsed", ValType = "string", GuiType = "TextArea", CanNullOrEmpty = "true"}}}
            };
            Add(sf);
        }

        public static DropboxFeature Getstances(Int32 type, String culture = "en")
        {
            lock (locker)
            {
                if (!instances.ContainsKey(type + culture))
                {
                    var dropBox = new DropboxFeature(type, culture);
                    foreach (var feature in dropBox.FeatureObjs)
                    {
                        feature.AdvancedOptions.AddRange(new List<String>()
                            {
                                "RetryInterval=30000",//30s
                                "RetryCount=6"
                            });
                    }
                    instances[type + culture] = dropBox;
                    return dropBox;
                }
                else
                {
                    return instances[type + culture];
                }
            }
        }
    }
}