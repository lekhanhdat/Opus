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

namespace AvePoint.Media.Storage.OneDrive
{
    #region using directives
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Globalization;
    using AvePoint.GCommon.Contract.CodeReview;
    using AvePoint.Media.Storage.Resources.SkyDriveI18N;
    using System.Web; 
    #endregion

    #region CodeReview
    [AveCodeReview(
    "2012/9/13",
    "rongbiao.sun@avepoint.com",
    "nan.shen@avepoint.com",
     new string[] { CodeReviewConstants.CHECK_LIST_ID_BL_1 },
     null,
     true)]
    #endregion

    class OneDriveFeature : StorageFeature
    {
        private OneDriveFeature(int type, string culture)
        {
            this.Init(type,culture);
        }

        private static string authorizeUrl = string.Format("https://login.live.com/oauth20_authorize.srf?&client_id={0}&scope={1}&response_type={2}&redirect_uri={3}",
                                     "0000000044122D4D", "wl.offline_access%20wl.skydrive%20wl.skydrive_update", "code", HttpUtility.UrlEncode("https://www.avepointonlineservices.com/getcloudtoken/onedrive"));

        protected override void GenerateSingleTypeFeatureUnit(CultureInfo culture)
        {
            StorageFeature sf = new StorageFeature();
            StorageType type = new StorageType();
            type.Value = "SkyDrive";
            type.Display = SkyDriveI18N.ResourceManager.GetString("MediaStorage_SkyDrive_Type", culture);
            type.Index = 11;
            type.IsSupportMovableRetention = false;
            type.Vim = new List<string>() { "SkyDrive_VIM".ToLower(CultureInfo.InvariantCulture) };
            sf.Type = type;
            sf.IsNeedSpaceThreshold = true;
            sf.Type.DefaultXris.Add("DOCAVE-XAM://SkyDrive_VIM?".ToLower(CultureInfo.InvariantCulture));
            //sf.Type.Vim.Add("SkyDrive_VIM");　
            sf.Features = new List<FeatureUnit> {
                new FeatureUnit() {Vim = "SkyDrive_VIM".ToLower(CultureInfo.InvariantCulture), DisplayName = SkyDriveI18N.ResourceManager.GetString("MediaStorage_SkyDrive_RootFolderName",culture), Tag = "SkyDrive_Root_Folder_Name".ToLower(CultureInfo.InvariantCulture), Key = "Root_Folder_Name".ToLower(CultureInfo.InvariantCulture), KeyName = "SkyDriveRootFolderName", Visibility="Visible", ValType = "string", IsRequiredOption = true, GuiType = "TextBox"},
                new FeatureUnit() {Vim = "SkyDrive_VIM".ToLower(CultureInfo.InvariantCulture), DisplayName = SkyDriveI18N.ResourceManager.GetString("MediaStorage_SkyDrive_RefreshToken",culture), Tag = "SkyDrive".ToLower(CultureInfo.InvariantCulture), Key = "RefreshTokenSecret".ToLower(CultureInfo.InvariantCulture), KeyName = "SkyDriveRefreshTokenSecret", Visibility="Visible", ValType = "string", IsRequiredOption = true, GuiType = "PasswordBox"},
                new FeatureUnit() {Vim = "SkyDrive_VIM".ToLower(CultureInfo.InvariantCulture), DisplayName = SkyDriveI18N.ResourceManager.GetString("MediaStorage_SkyDrive_Default_Application" ,culture), Tag = "SkyDrive_Radio_Button".ToLower(CultureInfo.InvariantCulture), Key = "SkyDrive_default".ToLower(CultureInfo.InvariantCulture), KeyName = "SkyDrive_DefaultName", Visibility="Visible", ValType = "string", GuiType = "RadioButton", Value = "true", ChildFeatures = new List<FeatureUnit>(){
                    new FeatureUnit() {Vim = "SkyDrive_VIM".ToLower(CultureInfo.InvariantCulture), Tag = "SkyDrive_default".ToLower(CultureInfo.InvariantCulture), Key = "SkyDrive_Retrieve_Token", DisplayName = SkyDriveI18N.ResourceManager.GetString("MediaStorage_SkyDrive_Retrieve_Token" ,culture), Visibility="Visible", ValType = "string", GuiType = "Hyperlink", Value = authorizeUrl},
                }},   
                new FeatureUnit() {Vim = "SkyDrive_VIM".ToLower(CultureInfo.InvariantCulture), DisplayName = SkyDriveI18N.ResourceManager.GetString("MediaStorage_SkyDrive_Customized_Application" ,culture), Tag = "SkyDrive_Radio_Button".ToLower(CultureInfo.InvariantCulture), Key = "SkyDrive_Customized".ToLower(CultureInfo.InvariantCulture), KeyName = "SkyDrive_CustomizedName", Visibility="Visible", ValType = "string", GuiType = "RadioButton", ChildFeatures = new List<FeatureUnit>(){
                    new FeatureUnit() {Vim = "SkyDrive_VIM".ToLower(CultureInfo.InvariantCulture), DisplayName = SkyDriveI18N.ResourceManager.GetString("MediaStorage_SkyDrive_ClientID",culture), Tag = "SkyDrive_Customized".ToLower(CultureInfo.InvariantCulture), Key = "Client_ID".ToLower(CultureInfo.InvariantCulture), KeyName = "SkyDriveClientID", Visibility="Collapsed", ValType = "string", IsRequiredOption = true, GuiType = "TextBox"},
                    new FeatureUnit() {Vim = "SkyDrive_VIM".ToLower(CultureInfo.InvariantCulture), DisplayName = SkyDriveI18N.ResourceManager.GetString("MediaStorage_SkyDrive_ClientSecret",culture), Tag = "SkyDrive_Customized".ToLower(CultureInfo.InvariantCulture), Key = "Client_Secret".ToLower(CultureInfo.InvariantCulture), KeyName = "SkyDriveClientSecret", Visibility="Collapsed", ValType = "string", IsRequiredOption = true, GuiType = "PasswordBox"},
                    new FeatureUnit() {Vim = "SkyDrive_VIM".ToLower(CultureInfo.InvariantCulture), DisplayName = SkyDriveI18N.ResourceManager.GetString("MediaStorage_SkyDrive_RedirectDomain",culture), Tag = "SkyDrive_Customized".ToLower(CultureInfo.InvariantCulture), Key = "Redirect_Domain".ToLower(CultureInfo.InvariantCulture), KeyName = "SkyDriveRedirectDomain", Visibility="Collapsed", ValType = "string", IsRequiredOption = true, GuiType = "TextBox"},
                }},
                new FeatureUnit() {Vim = "SkyDrive_VIM".ToLower(CultureInfo.InvariantCulture), DisplayName = SkyDriveI18N.ResourceManager.GetString("MediaStorage_SkyDrive_Proxy", culture), Tag = "SkyDrive_Proxy".ToLower(CultureInfo.InvariantCulture), Key = "SkyDrive_Proxy".ToLower(CultureInfo.InvariantCulture), Value = "SkyDrive_Proxy_setting".ToLower(CultureInfo.InvariantCulture),  KeyName = "skyDriveProxySetting", Visibility="Visible", ValType = "string", GuiType = "CheckBox", ChildFeatures = new List<FeatureUnit>(){
                    new FeatureUnit() {Vim = "SkyDrive_VIM".ToLower(CultureInfo.InvariantCulture), DisplayName = SkyDriveI18N.ResourceManager.GetString("MediaStorage_SkyDrive_Proxy_Host", culture), Tag = "SkyDrive_Proxy_setting".ToLower(CultureInfo.InvariantCulture), Key = "ProxyIp".ToLower(CultureInfo.InvariantCulture), KeyName = "SkyDriveProxyHost", Visibility="Collapsed", ValType = "string", IsRequiredOption = true, GuiType = "TextBox"},
                    new FeatureUnit() {Vim = "SkyDrive_VIM".ToLower(CultureInfo.InvariantCulture), DisplayName = SkyDriveI18N.ResourceManager.GetString("MediaStorage_SkyDrive_Proxy_Port", culture), Tag = "SkyDrive_Proxy_setting".ToLower(CultureInfo.InvariantCulture), Key = "ProxyPort".ToLower(CultureInfo.InvariantCulture), KeyName = "SkyDriveProxyPort", Visibility="Collapsed", ValType = "string", IsRequiredOption = true, GuiType = "TextBox"},
                    new FeatureUnit() {Vim = "SkyDrive_VIM".ToLower(CultureInfo.InvariantCulture), DisplayName = SkyDriveI18N.ResourceManager.GetString("MediaStorage_SkyDrive_Proxy_Username", culture), Tag = "SkyDrive_Proxy_setting".ToLower(CultureInfo.InvariantCulture), Key = "ProxyUsername".ToLower(CultureInfo.InvariantCulture), KeyName = "SkyDriveProxyUsername", Visibility="Collapsed", ValType = "string", IsRequiredOption = false, GuiType = "TextBox", CanNullOrEmpty = "true"},
                    new FeatureUnit() {Vim = "SkyDrive_VIM".ToLower(CultureInfo.InvariantCulture), DisplayName = SkyDriveI18N.ResourceManager.GetString("MediaStorage_SkyDrive_Proxy_Password", culture), Tag = "SkyDrive_Proxy_setting".ToLower(CultureInfo.InvariantCulture), Key = "ProxyPasswordSecret".ToLower(CultureInfo.InvariantCulture), KeyName = "SkyDriveProxyPasswordSecret", Visibility="Collapsed", ValType = "string", IsRequiredOption = false, GuiType = "PasswordBox", CanNullOrEmpty = "true"},
                }},
                new FeatureUnit() {Vim = "SkyDrive_VIM".ToLower(CultureInfo.InvariantCulture), DisplayName = SkyDriveI18N.ResourceManager.GetString("MediaStorage_SkyDrive_Advanced",culture), Tag = "SkyDrive_VIM_ADVANCED".ToLower(CultureInfo.InvariantCulture), Key = "advanced",Value = "SkyDrive_VIM_EXTENDEDPARAMETERS".ToLower(CultureInfo.InvariantCulture),  KeyName = "SkyDriveAdvanced",Visibility="Visible", ValType = "string", GuiType = "CheckBox",  ChildFeatures = new List<FeatureUnit>() {
                    new FeatureUnit() {Vim = "SkyDrive_VIM".ToLower(CultureInfo.InvariantCulture), DisplayName = SkyDriveI18N.ResourceManager.GetString("MediaStorage_SkyDrive_Extended_Parameters",culture),Tag = "SkyDrive_VIM_EXTENDEDPARAMETERS".ToLower(CultureInfo.InvariantCulture), Key = "EXTENDEDPARAMETERS".ToLower(CultureInfo.InvariantCulture), KeyName = "SkyDriveExtendedParameters",  Visibility="Collapsed", ValType = "string", GuiType = "TextArea", CanNullOrEmpty = "true"}
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

        private static readonly Dictionary<string, OneDriveFeature> instances = new Dictionary<string, OneDriveFeature>();
        private static Object locker = new Object();
        public static OneDriveFeature Getstances(int type, string culture = "en")
        {
            lock (locker)
            {
                if (!instances.ContainsKey(type + culture))
                {
                    OneDriveFeature skydrive = new OneDriveFeature(type, culture);

                    foreach (var feature in skydrive.FeatureObjs)
                    {
                        feature.AdvancedOptions.AddRange(new List<string>()
                    {
                        "RetryInterval=30000",//30s
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
