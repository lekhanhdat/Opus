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




namespace AvePoint.Media.Storage.SFTP
{
    #region using directives
    using System;
    using System.Collections.Generic;
    using System.Globalization;
    using System.Text;
    using AvePoint.Media.Storage.Resources.SFTPI18N;
    using AvePoint.Media.Storage.Util;
    #endregion

    /// <summary>
    /// 此类用于描述SFTP类型的存储介质的特性
    /// </summary>
    /// 
    sealed class SFTPFeature : StorageFeature
    {
        #region SFTPFeature 自己负责初始化的部分, 在Application生存周期内只会初始化一次, 外部不需要知道的逻辑
        /// <summary>
        /// 私有构造函数， 此类不允许在外部实例化。 <see cref="SFTPFeature"/> class.
        /// </summary>
        private SFTPFeature(int type,string culture)
        {
            this.Init(type,culture);
        }

        /// <summary>
        /// 实际初始化DocAve GUI Feature Unit的部分
        /// </summary>
        protected override void GenerateDocAveGUIFeatureUnit(CultureInfo culture)
        {
            //StorageFeature sf = new StorageFeature();

            ////定义StorageType
            //StorageType t = new StorageType();
            //t.Display = SFTPI18N.ResourceManager.GetString("MediaStorage_SFTP_SFTP", culture);
            //t.Value = "SFTP";
            //t.Vim = new List<string>() { "sftp_vim" };
            //t.Index = 15;
            //t.SoExtenderNotSupported = true;

            //sf.Type = t;
            //sf.Type.DefaultXris = new List<string>() { XConst.MEDIASTORAGE_PROTOCOL + "sftp_vim?" };
            //sf.IsNeedSpaceThreshold = false;
            //sf.ProgressForeground = new FeatureColor(255, 161, 18, 24);

            //sf.Features = new List<FeatureUnit>() { 
            //    new FeatureUnit() {Vim = "sftp_vim", DisplayName = SFTPI18N.ResourceManager.GetString("MediaStorage_SFTP_Host", culture), Tag = "sftp_host", Key = "host", KeyName = "SFTPHost", Visibility = "Visible", ValType="string", IsRequiredOption = true, GuiType="TextBox", DemoValue=@"10.2.207.160"},
            //    new FeatureUnit() {Vim = "sftp_vim", DisplayName = SFTPI18N.ResourceManager.GetString("MediaStorage_SFTP_Port", culture), Tag = "sftp_port", Key = "port", KeyName = "SFTPPort", Visibility = "Visible", ValType="string", IsRequiredOption = true, GuiType="TextBox", DemoValue=@"22"},
            //    new FeatureUnit() {Vim = "sftp_vim", DisplayName = SFTPI18N.ResourceManager.GetString("MediaStorage_SFTP_Root_Folder", culture), Tag = "sftp_rootfolder", Key = "sftprootfolder", KeyName = "SFTPRootFolder", Visibility = "Visible", ValType="string", GuiType="TextBox", DemoValue=@"DocAve", FeatureFlag=(int)(FeatureUnitFlag.CloudContainer|FeatureUnitFlag.Path), CanModifi = true, CanNullOrEmpty = "true"},
            //    new FeatureUnit() {Vim = "sftp_vim", DisplayName = SFTPI18N.ResourceManager.GetString("MediaStorage_SFTP_Username", culture), Tag = "sftp_username", Key = "name", KeyName = "SFTPUsername", Visibility = "Visible", ValType="string", IsRequiredOption = true, GuiType="TextBox", DemoValue=@"admin"},
            //    new FeatureUnit() {Vim = "sftp_vim", DisplayName = SFTPI18N.ResourceManager.GetString("MediaStorage_SFTP_Password", culture), Tag = "sftp_password", Key = "secret", KeyName = "SFTPPassword", Visibility = "Visible", ValType="string", IsRequiredOption = false, CanNullOrEmpty = "true", GuiType="PasswordBox" },
            //    new FeatureUnit() {Vim = "sftp_vim", DisplayName = SFTPI18N.ResourceManager.GetString("MediaStorage_SFTP_PrivateKey", culture), Tag = "sftp_privatekey", Key = "privatekeysecret", KeyName = "SFTPPrivateKey", Visibility = "Collapsed", ValType="string", IsRequiredOption = false, CanNullOrEmpty = "true", GuiType="PasswordBox"},
            //    new FeatureUnit() {Vim = "sftp_vim", DisplayName = SFTPI18N.ResourceManager.GetString("MediaStorage_SFTP_PrivateKeyFile", culture), Tag = "sftp_privatekeyfile", Key = "privatekeyfile", KeyName = "SFTPPrivateKeyFile", Visibility = "Visible", ValType="string", IsRequiredOption = false, GuiType="Button", Value = "sftp_privatekey"},
            //                    new FeatureUnit() {Vim = "sftp_vim", DisplayName = SFTPI18N.ResourceManager.GetString("MediaStorage_SFTP_PrivateKeyPassword", culture), Tag = "sftp_privatekeypassword", Key = "privatekeypasswordsecret", KeyName = "SFTPPrivateKeyPassword", Visibility = "Visible", ValType="string", IsRequiredOption = false, GuiType="PasswordBox", Value = "sftp_privatekeypassword", CanNullOrEmpty = "true"},
            //    //Add Advanced Option
            //    new FeatureUnit() {Vim = "sftp_vim", DisplayName =  "Advanced", Tag = "sftp_vim_advanced", Key = "advanced", Value = "sftp_vim_ExtendedParameters", KeyName = "SFTPAdvanced",Visibility="Visible", ValType = "string", GuiType = "CheckBox",  ChildFeatures = new List<FeatureUnit>() {
            //                new FeatureUnit() {Vim = "sftp_vim", DisplayName = "Extended_Parameters",Tag = "sftp_vim_ExtendedParameters", Key = "ExtendedParameters".ToLower(CultureInfo.InvariantCulture), KeyName = "SFTPExtendedParameters",  Visibility="Collapsed", ValType = "string", GuiType = "TextArea", CanNullOrEmpty = "true"}}} 
            //};

            //Add(sf);
        }
        #endregion

        /// <summary>
        /// 供VIM调用, 获取相应的Feature
        /// </summary>
        private static readonly Dictionary<string, SFTPFeature> instances = new Dictionary<string, SFTPFeature>();

        public static SFTPFeature Getstances(int type, string culture = "en")
        {
            if (!instances.ContainsKey(type + culture))
            {
                SFTPFeature sftp = new SFTPFeature(type, culture);

                foreach (var feature in sftp.FeatureObjs)
                {
                    feature.AdvancedOptions.AddRange(new List<string>()
                    {
                        "Encode=UTF-8"
                    });
                }

                instances[type + culture] = sftp;
                return sftp;
            }
            else
            {
                return instances[type + culture];
            }
        }
    }
}
