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



namespace AvePoint.Media.Storage.FTP
{
    #region using directives
    using AvePoint.Media.Storage.Resources.FTPI18N;
    using AvePoint.Media.Storage.Util;
    using System;
    using System.Collections.Generic;
    using System.Globalization;
    #endregion

    /// <summary>
    /// 此类用于描述FTP类型的存储介质的特性
    /// </summary>
    /// 
    sealed class FTPFeature : StorageFeature
    {
        #region FTPFeature 自己负责初始化的部分, 在Application生存周期内只会初始化一次, 外部不需要知道的逻辑
        /// <summary>
        /// 私有构造函数， 此类不允许在外部实例化。 <see cref="FTPFeature"/> class.
        /// </summary>
        private FTPFeature(int type, string culture)
        {
            this.Init(type, culture);
        }

        /// <summary>
        /// 实际初始化DocAve GUI Feature Unit的部分
        /// </summary>
        protected override void GenerateDocAveGUIFeatureUnit(CultureInfo culture)
        {
            StorageFeature sf = new StorageFeature();

            //定义StorageType
            StorageType t = new StorageType();
            t.Display = FTPI18N.ResourceManager.GetString("MediaStorage_FTP_FTP", culture);
            t.Value = "FTP";
            t.Vim = new List<string>() { "ftp_vim" };
            t.Index = 1;
            t.SoExtenderNotSupported = true;

            sf.Type = t;
            sf.Type.DefaultXris = new List<string>() { XConst.MEDIASTORAGE_PROTOCOL + "ftp_vim?" };
            sf.IsNeedSpaceThreshold = false;
            sf.ProgressForeground = new FeatureColor(255, 171, 212, 23);

            sf.Features = new List<FeatureUnit>() {
                new FeatureUnit() {Vim = "ftp_vim", DisplayName = FTPI18N.ResourceManager.GetString("MediaStorage_FTP_Host", culture), Tag = "ftp_host", Key = "host", KeyName = "FTPHost", Visibility = "Visible", ValType="string", IsRequiredOption = true, GuiType="TextBox", DemoValue=@"10.2.207.160"},
                new FeatureUnit() {Vim = "ftp_vim", DisplayName = FTPI18N.ResourceManager.GetString("MediaStorage_FTP_Port", culture), Tag = "ftp_port", Key = "port", KeyName = "FTPPort", Visibility = "Visible", ValType="string", IsRequiredOption = true, GuiType="TextBox", DemoValue=@"21"},
                new FeatureUnit() {Vim = "ftp_vim", DisplayName = FTPI18N.ResourceManager.GetString("MediaStorage_Connector_FTP_Root_Folder",culture), Tag = "ftp_RootFolder".ToLower(CultureInfo.InvariantCulture), Key = "FTPRootFolder".ToLower(CultureInfo.InvariantCulture), KeyName = "FTPRootFolder", Visibility = "Visible", ValType="string", GuiType="TextBox", DemoValue= FTPI18N.ResourceManager.GetString("MediaStorage_Connector_Ftp_RootFolderName_Demo", culture), FeatureFlag=(int)(FeatureUnitFlag.CloudContainer|FeatureUnitFlag.Path), CanModifi = true, CanNullOrEmpty="true",
                    ValidateRegPats = new List<string>(){
                    @"^.{0,200}$" + "\t0\t" + FTPI18N.ResourceManager.GetString("MediaStorage_Connector_FTP_root_folder_cannot_exceed_200_characters", culture),
                    @"^[^\\]+|^.{0,0}$" + "\t0\t" + FTPI18N.ResourceManager.GetString("MediaStorage_Connector_FTP_RootFolderName_Invalid_Format", culture)}},
                new FeatureUnit() {Vim = "ftp_vim", DisplayName = FTPI18N.ResourceManager.GetString("MediaStorage_FTP_Username", culture), Tag = "ftp_username", Key = "name", KeyName = "FTPUsername", Visibility = "Visible", ValType="string", IsRequiredOption = true, GuiType="TextBox", DemoValue = FTPI18N.ResourceManager.GetString("MediaStorage_FTP_Username_Demo", culture)},
                new FeatureUnit() {Vim = "ftp_vim", DisplayName = FTPI18N.ResourceManager.GetString("MediaStorage_FTP_Password", culture), Tag = "ftp_password", Key = "secret", KeyName = "FTPPassword", Visibility = "Visible", ValType="string", IsRequiredOption = true, GuiType="PasswordBox"},
                //Add Advanced Option
                new FeatureUnit() {Vim = "ftp_vim", DisplayName =  FTPI18N.ResourceManager.GetString("MediaStorage_FTP_Advanced", culture), Tag = "ftp_vim_advanced", Key = "advanced", Value = "ftp_vim_ExtendedParameters", KeyName = "FTPAdvanced",Visibility="Visible", ValType = "string", GuiType = "CheckBox",  ChildFeatures = new List<FeatureUnit>() {
                            new FeatureUnit() {Vim = "ftp_vim", DisplayName = FTPI18N.ResourceManager.GetString("MediaStorage_FTP_Extended_Parameters", culture), Tag = "ftp_vim_ExtendedParameters", Key = "ExtendedParameters".ToLower(CultureInfo.InvariantCulture), KeyName = "FTPExtendedParameters",  Visibility="Collapsed", ValType = "string", GuiType = "TextArea", CanNullOrEmpty = "true"}}}
            };

            Add(sf);
        }
        #endregion

        protected override void GenerateConnectorGUIFeatureUnit(CultureInfo culture)
        {
            StorageFeature sf = new StorageFeature();

            //定义StorageType
            StorageType t = new StorageType();
            t.Display = FTPI18N.ResourceManager.GetString("MediaStorage_Connector_FTP_FTP", culture);
            t.Value = "FTP";
            t.Vim = new List<string>() { "ftp_vim" };
            t.Index = 1;
            t.SoExtenderNotSupported = true;
            sf.Description = FTPI18N.ResourceManager.GetString("MediaStorage_Connector_FTP_FTP_Configure_Description", culture);
            sf.Type = t;
            sf.Type.DefaultXris = new List<string>() { XConst.MEDIASTORAGE_PROTOCOL + "ftp_vim?" };
            sf.IsNeedSpaceThreshold = false;
            sf.ProgressForeground = new FeatureColor(255, 161, 18, 24);

            sf.Features = new List<FeatureUnit>() {
                new FeatureUnit() {IsRequiredOption = true, Vim = "ftp_vim", DisplayName = FTPI18N.ResourceManager.GetString("MediaStorage_Connector_FTP_Host",culture), Tag = "ftp_host", Key = "host", KeyName = "FTPHost", Visibility = "Visible", ValType="string", GuiType="TextBox", DemoValue=@"10.2.207.160", CanModifi = false, ValidateRegPats = new List<string>(){@"(^(([0-9]|[1-9]\d{1}|[1]\d{2}|2[0-4]\d|25[012345])\.){3}([0-9]|[1-9]\d{1}|[1]\d{2}|2[0-4]\d|25[012345])$)|(^\s*((([0-9A-Fa-f]{1,4}:){7}(([0-9A-Fa-f]{1,4})|:))|(([0-9A-Fa-f]{1,4}:){6}(:|((25[0-5]|2[0-4]\d|[01]?\d{1,2})(\.(25[0-5]|2[0-4]\d|[01]?\d{1,2})){3})|(:[0-9A-Fa-f]{1,4})))|(([0-9A-Fa-f]{1,4}:){5}((:((25[0-5]|2[0-4]\d|[01]?\d{1,2})(\.(25[0-5]|2[0-4]\d|[01]?\d{1,2})){3})?)|((:[0-9A-Fa-f]{1,4}){1,2})))|(([0-9A-Fa-f]{1,4}:){4}(:[0-9A-Fa-f]{1,4}){0,1}((:((25[0-5]|2[0-4]\d|[01]?\d{1,2})(\.(25[0-5]|2[0-4]\d|[01]?\d{1,2})){3})?)|((:[0-9A-Fa-f]{1,4}){1,2})))|(([0-9A-Fa-f]{1,4}:){3}(:[0-9A-Fa-f]{1,4}){0,2}((:((25[0-5]|2[0-4]\d|[01]?\d{1,2})(\.(25[0-5]|2[0-4]\d|[01]?\d{1,2})){3})?)|((:[0-9A-Fa-f]{1,4}){1,2})))|(([0-9A-Fa-f]{1,4}:){2}(:[0-9A-Fa-f]{1,4}){0,3}((:((25[0-5]|2[0-4]\d|[01]?\d{1,2})(\.(25[0-5]|2[0-4]\d|[01]?\d{1,2})){3})?)|((:[0-9A-Fa-f]{1,4}){1,2})))|(([0-9A-Fa-f]{1,4}:)(:[0-9A-Fa-f]{1,4}){0,4}((:((25[0-5]|2[0-4]\d|[01]?\d{1,2})(\.(25[0-5]|2[0-4]\d|[01]?\d{1,2})){3})?)|((:[0-9A-Fa-f]{1,4}){1,2})))|(:(:[0-9A-Fa-f]{1,4}){0,5}((:((25[0-5]|2[0-4]\d|[01]?\d{1,2})(\.(25[0-5]|2[0-4]\d|[01]?\d{1,2})){3})?)|((:[0-9A-Fa-f]{1,4}){1,2})))|(((25[0-5]|2[0-4]\d|[01]?\d{1,2})(\.(25[0-5]|2[0-4]\d|[01]?\d{1,2})){3})))(%.+)?\s*$)" + "\t0\t" + FTPI18N.ResourceManager.GetString("MediaStorage_FTP_The_host_format_is_not_correct", culture)}},
                new FeatureUnit() {IsRequiredOption = true, Vim = "ftp_vim", DisplayName = FTPI18N.ResourceManager.GetString("MediaStorage_Connector_FTP_Port",culture), Tag = "ftp_port", Key = "port", KeyName = "FTPPort", Visibility = "Visible", ValType="string", GuiType="TextBox", DemoValue=@"21", CanModifi = false, ValidateRegPats = new List<string>(){@"^([1-9]|[1-9]\d{1}|[1-9]\d{2}|[1-9]\d{3}|[1-5]\d{4}|6[0-4]\d{3}|65[0-4]\d{2}|655[0-2]\d{1}|6553[0-5])$" + "\t0\t" + FTPI18N.ResourceManager.GetString("MediaStorage_Connector_FTP_The_port_format_is_not_correct",culture)}},
                new FeatureUnit() {IsRequiredOption = true, Vim = "ftp_vim", DisplayName = FTPI18N.ResourceManager.GetString("MediaStorage_Connector_FTP_Root_Folder",culture), Tag = "ftp_RootFolder".ToLower(CultureInfo.InvariantCulture), Key = "FTPRootFolder".ToLower(CultureInfo.InvariantCulture), KeyName = "FTPRootFolder", Visibility = "Visible", ValType="string", GuiType="TextBox", DemoValue= FTPI18N.ResourceManager.GetString("MediaStorage_Connector_Ftp_RootFolderName_Demo", culture), FeatureFlag=(int)(FeatureUnitFlag.CloudContainer|FeatureUnitFlag.Path), CanModifi = false, ValidateRegPats = new List<string>(){
                    @"^.{0,200}$" + "\t0\t" + FTPI18N.ResourceManager.GetString("MediaStorage_Connector_FTP_root_folder_cannot_exceed_200_characters", culture),
                    @"^[^\\]+|^.{0,0}$" + "\t0\t" + FTPI18N.ResourceManager.GetString("MediaStorage_Connector_FTP_RootFolderName_Invalid_Format", culture)}},

                new FeatureUnit() {IsRequiredOption = true, Vim = "ftp_vim", DisplayName = FTPI18N.ResourceManager.GetString("MediaStorage_Connector_FTP_Username",culture), Tag = "ftp_username", Key = "name", KeyName = "FTPUsername", Visibility = "Visible", ValType="string", GuiType="TextBox", DemoValue = FTPI18N.ResourceManager.GetString("MediaStorage_Connector_FTP_Username_Demo")},
                new FeatureUnit() {IsRequiredOption = true, Vim = "ftp_vim", DisplayName = FTPI18N.ResourceManager.GetString("MediaStorage_Connector_FTP_Password",culture), Tag = "ftp_password", Key = "secret", KeyName = "FTPPassword", Visibility = "Visible", ValType="string", GuiType="PasswordBox"},
                //Add Advanced Option
                new FeatureUnit() {IsRequiredOption = true, Vim = "ftp_vim", DisplayName =  FTPI18N.ResourceManager.GetString("MediaStorage_Connector_FTP_Advanced",culture), Tag = "ftp_vim_advanced", Key = "advanced", Value = "ftp_vim_ExtendedParameters", KeyName = "FTPAdvanced",Visibility="Visible", ValType = "string", GuiType = "CheckBox",  ChildFeatures = new List<FeatureUnit>() {
                            new FeatureUnit() {IsRequiredOption = true, Vim = "ftp_vim", DisplayName = FTPI18N.ResourceManager.GetString("MediaStorage_Connector_FTP_Extended_Parameters",culture),Tag = "ftp_vim_ExtendedParameters", Key = "ExtendedParameters".ToLower(CultureInfo.InvariantCulture), KeyName = "FTPExtendedParameters",  Visibility="Collapsed", ValType = "string", GuiType = "TextArea", CanNullOrEmpty = "true"}}}
            };

            Add(sf);
        }
        /// <summary>
        /// 供VIM调用, 获取相应的Feature
        /// </summary>
        private static readonly Dictionary<string, FTPFeature> instances = new Dictionary<string, FTPFeature>();
        private static Object locker = new Object();
        public static FTPFeature Getstances(int type, string culture = "en")
        {
            lock (locker)
            {
                if (!instances.ContainsKey(type + culture))
                {
                    FTPFeature ftp = new FTPFeature(type, culture);

                    foreach (var feature in ftp.FeatureObjs)
                    {
                        feature.AdvancedOptions.AddRange(new List<string>()
                        {
                            "FtpType=win03",
                            "IsRetry=true",
                            "IsRetry=false",
                            "RetryInterval=30",
                            "RetryCount=6",
                            "Schema=ftp",
                            "Schema=ftps",
                            "UsePassive=true",
                            "UsePassive=false",
                            "UseFluentFTP=true",
                            "UseFluentFTP=false"
                        });
                    }
                    instances[type + culture] = ftp;
                    return ftp;
                }
                else
                {
                    return instances[type + culture];
                }
            }
        }
    }
}
