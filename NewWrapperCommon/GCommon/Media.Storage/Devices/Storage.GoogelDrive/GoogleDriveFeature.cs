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

namespace AvePoint.Media.Storage.GoogleDrive
{
    #region using directives
    using AvePoint.Media.Storage.Resources.GoogleDriveI18N;
    using System;
    using System.Collections.Generic;
    using System.Globalization;
    #endregion

    class GoogleDriveFeature : StorageFeature
    {
        private GoogleDriveFeature(int type, string culture)
        {
            this.Init(type, culture);
        }

        private static string authorizeUrl = string.Format("https://accounts.google.com/o/oauth2/auth?scope=https%3A%2F%2Fwww.googleapis.com%2Fauth%2Fdrive&redirect_uri={0}&response_type=code&client_id={1}&access_type=offline", "https://www.avepointonlineservices.com/getcloudtoken/googledrive", "145176449998-bhscqara60tsb75a7gbcrfc9g02793m2.apps.googleusercontent.com");
        protected override void GenerateSingleTypeFeatureUnit(CultureInfo culture)
        {
            StorageFeature sf = new StorageFeature();
            StorageType type = new StorageType();
            type.Value = "GoogleDrive";
            type.Display = GoogleDriveI18N.ResourceManager.GetString("MediaStorage_GoogleDrive_GoogleDrive", culture);
            type.IsSupportMovableRetention = false;
            type.Index = 10;
            type.Vim = new List<string>() { "GoogleDrive_VIM".ToLower(CultureInfo.InvariantCulture) };
            sf.Type = type;
            sf.IsNeedSpaceThreshold = true;
            sf.Type.DefaultXris.Add("DOCAVE-XAM://GoogleDrive_VIM?".ToLower(CultureInfo.InvariantCulture));
            //sf.Type.Vim.Add("SkyDrive_VIM");　
            sf.Features = new List<FeatureUnit> {
                new FeatureUnit() {Vim = "GoogleDrive_VIM".ToLower(CultureInfo.InvariantCulture), DisplayName = GoogleDriveI18N.ResourceManager.GetString("MediaStorage_GoogleDrive_RootFolderName",culture), Tag = "GoogleDrive_Root_Folder_Name".ToLower(CultureInfo.InvariantCulture), Key = "Root_Folder_Name".ToLower(CultureInfo.InvariantCulture), KeyName = "GoogleDriveRootFolderName", Visibility="Visible", ValType = "string", IsRequiredOption = true, GuiType = "TextBox"},
                new FeatureUnit() {Vim = "GoogleDrive_VIM".ToLower(CultureInfo.InvariantCulture), DisplayName = GoogleDriveI18N.ResourceManager.GetString("MediaStorage_GoogleDrive_RefreshToken",culture), Tag = "GoogleDrive_RefreshToken".ToLower(CultureInfo.InvariantCulture), Key = "RefreshTokenSecret".ToLower(CultureInfo.InvariantCulture), KeyName = "GoogleDriveRefreshToken", Visibility="Visible", ValType = "string", IsRequiredOption = true, GuiType = "PasswordBox"},
                new FeatureUnit() {Vim = "GoogleDrive_VIM".ToLower(CultureInfo.InvariantCulture), DisplayName = GoogleDriveI18N.ResourceManager.GetString("MediaStorage_GoogleDrive_Default_Application" ,culture), Tag = "GoogleDrive_Radio_Button".ToLower(CultureInfo.InvariantCulture), Key = "GoogleDrive_default".ToLower(CultureInfo.InvariantCulture), KeyName = "GoogleDrive_DefaultName", Visibility="Visible", ValType = "string", GuiType = "RadioButton", Value = "true", ChildFeatures = new List<FeatureUnit>(){
                    new FeatureUnit() {Vim = "GoogleDrive_VIM".ToLower(CultureInfo.InvariantCulture), Tag = "GoogleDrive_default".ToLower(CultureInfo.InvariantCulture), Key = "GoogleDrive_Retrieve_Token".ToLower(CultureInfo.InvariantCulture), DisplayName = GoogleDriveI18N.ResourceManager.GetString("MediaStorage_GoogleDrive_Retrieve_Token" ,culture), Visibility="Visible", ValType = "string", GuiType = "Hyperlink", Value = authorizeUrl},
                }},   
                new FeatureUnit() {Vim = "GoogleDrive_VIM".ToLower(CultureInfo.InvariantCulture), DisplayName = GoogleDriveI18N.ResourceManager.GetString("MediaStorage_GoogleDrive_Customized_Application" ,culture), Tag = "GoogleDrive_Radio_Button".ToLower(CultureInfo.InvariantCulture), Key = "GoogleDrive_Customized".ToLower(CultureInfo.InvariantCulture), KeyName = "GoogleDrive_CustomizedName", Visibility="Visible", ValType = "string", GuiType = "RadioButton", ChildFeatures = new List<FeatureUnit>(){
                    new FeatureUnit() {Vim = "GoogleDrive_VIM".ToLower(CultureInfo.InvariantCulture), DisplayName = GoogleDriveI18N.ResourceManager.GetString("MediaStorage_GoogleDrive_ClientID",culture), Tag = "GoogleDrive_Customized".ToLower(CultureInfo.InvariantCulture), Key = "Client_ID".ToLower(CultureInfo.InvariantCulture), KeyName = "GoogleDriveClientID", Visibility="Collapsed", ValType = "string",  IsRequiredOption = true, GuiType = "TextBox"},
                    new FeatureUnit() {Vim = "GoogleDrive_VIM".ToLower(CultureInfo.InvariantCulture), DisplayName = GoogleDriveI18N.ResourceManager.GetString("MediaStorage_GoogleDrive_ClientSecret",culture), Tag = "GoogleDrive_Customized".ToLower(CultureInfo.InvariantCulture), Key = "Client_Secret".ToLower(CultureInfo.InvariantCulture), KeyName = "GoogleDriveClientSecret", Visibility="Collapsed", ValType = "string", IsRequiredOption = true, GuiType = "PasswordBox"},
                }},
                new FeatureUnit() {Vim = "GoogleDrive_VIM".ToLower(CultureInfo.InvariantCulture), DisplayName = GoogleDriveI18N.ResourceManager.GetString("MediaStorage_GoogleDrive_Proxy", culture), Tag = "GoogleDrive_Proxy".ToLower(CultureInfo.InvariantCulture), Key = "GoogleDrive_Proxy".ToLower(CultureInfo.InvariantCulture), Value = "GoogleDrive_Proxy_setting".ToLower(CultureInfo.InvariantCulture),  KeyName = "googleDriveProxySetting", Visibility="Visible", ValType = "string", GuiType = "CheckBox", ChildFeatures = new List<FeatureUnit>(){
                    new FeatureUnit() {Vim = "GoogleDrive_VIM".ToLower(CultureInfo.InvariantCulture), DisplayName = GoogleDriveI18N.ResourceManager.GetString("MediaStorage_GoogleDrive_Proxy_Host", culture), Tag = "GoogleDrive_Proxy_setting".ToLower(CultureInfo.InvariantCulture), Key = "ProxyIp".ToLower(CultureInfo.InvariantCulture), KeyName = "GoogleDriveProxyHost", Visibility="Collapsed", ValType = "string", IsRequiredOption = true, GuiType = "TextBox"},
                    new FeatureUnit() {Vim = "GoogleDrive_VIM".ToLower(CultureInfo.InvariantCulture), DisplayName = GoogleDriveI18N.ResourceManager.GetString("MediaStorage_GoogleDrive_Proxy_Port", culture), Tag = "GoogleDrive_Proxy_setting".ToLower(CultureInfo.InvariantCulture), Key = "ProxyPort".ToLower(CultureInfo.InvariantCulture), KeyName = "GoogleDriveProxyPort", Visibility="Collapsed", ValType = "string", IsRequiredOption = true, GuiType = "TextBox"},
                    new FeatureUnit() {Vim = "GoogleDrive_VIM".ToLower(CultureInfo.InvariantCulture), DisplayName = GoogleDriveI18N.ResourceManager.GetString("MediaStorage_GoogleDrive_Proxy_Username", culture), Tag = "GoogleDrive_Proxy_setting".ToLower(CultureInfo.InvariantCulture), Key = "ProxyUsername".ToLower(CultureInfo.InvariantCulture), KeyName = "GoogleDriveProxyUsername", Visibility="Collapsed", ValType = "string", IsRequiredOption = false, GuiType = "TextBox", CanNullOrEmpty = "true"},
                    new FeatureUnit() {Vim = "GoogleDrive_VIM".ToLower(CultureInfo.InvariantCulture), DisplayName = GoogleDriveI18N.ResourceManager.GetString("MediaStorage_GoogleDrive_Proxy_Password", culture), Tag = "GoogleDrive_Proxy_setting".ToLower(CultureInfo.InvariantCulture), Key = "ProxyPasswordSecret".ToLower(CultureInfo.InvariantCulture), KeyName = "GoogleDriveProxyPasswordSecret", Visibility="Collapsed", ValType = "string", IsRequiredOption = false, GuiType = "PasswordBox", CanNullOrEmpty = "true"},
                }},
                new FeatureUnit() {Vim = "GoogleDrive_VIM".ToLower(CultureInfo.InvariantCulture), DisplayName = GoogleDriveI18N.ResourceManager.GetString("MediaStorage_GoogleDrive_Advanced",culture), Tag = "GoogleDrive_VIM_ADVANCED".ToLower(CultureInfo.InvariantCulture), Key = "advanced",Value = "GoogleDrive_VIM_EXTENDEDPARAMETERS".ToLower(CultureInfo.InvariantCulture),  KeyName = "GoogleDriveAdvanced",Visibility="Visible", ValType = "string", GuiType = "CheckBox",  ChildFeatures = new List<FeatureUnit>() {
                    new FeatureUnit() {Vim = "GoogleDrive_VIM".ToLower(CultureInfo.InvariantCulture), DisplayName = GoogleDriveI18N.ResourceManager.GetString("MediaStorage_GoogleDrive_Extended_Parameters",culture),Tag = "GoogleDrive_VIM_EXTENDEDPARAMETERS".ToLower(CultureInfo.InvariantCulture), Key = "EXTENDEDPARAMETERS".ToLower(CultureInfo.InvariantCulture), KeyName = "GoogleDriveExtendedParameters",  Visibility="Collapsed", ValType = "string", GuiType = "TextArea", CanNullOrEmpty = "true"}
                    }
                }        
            
            };
            Add(sf);
        }

        protected override void GenerateDocAveGUIFeatureUnit(CultureInfo culture)
        {
            GenerateSingleTypeFeatureUnit(culture);
        }

        //protected override void GenerateConnectorGUIFeatureUnit(CultureInfo culture)
        //{
        //    GenerateSingleTypeFeatureUnit(culture);
        //}

        private static readonly Dictionary<string, GoogleDriveFeature> instances = new Dictionary<string, GoogleDriveFeature>();
        private static Object locker = new Object();
        public static GoogleDriveFeature Getstances(int type, string culture = "en")
        {
            lock (locker)
            {
                if (!instances.ContainsKey(type + culture))
                {
                    GoogleDriveFeature skydrive = new GoogleDriveFeature(type, culture);

                    foreach (var feature in skydrive.FeatureObjs)
                    {
                        feature.AdvancedOptions.AddRange(new List<string>()
                    {
                        "RetryInterval=30000",    //30s
                        "RetryCount=6"
                    });
                    }
                    instances[type + culture] = skydrive;
                    return skydrive;
                }
                else
                {
                    return instances[type + culture];
                }
            }
        }
    }
}
