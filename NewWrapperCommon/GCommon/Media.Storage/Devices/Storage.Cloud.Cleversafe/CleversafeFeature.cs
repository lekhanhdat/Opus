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
namespace AvePoint.Media.Storage.Cloud.Cleversafe
{
    #region using directives
    using AvePoint.Media.Storage.Resources.CleversafeI18N;
    using System;
    using System.Collections.Generic;
    using System.Globalization;
    using System.Linq;
    using System.Text;
    #endregion
    class CleversafeFeature : StorageFeature
    {

        #region CleversafeFeature 自己负责初始化的部分, 在Application生存周期内只会初始化一次, 外部不需要知道的逻辑
        /// <summary>
        /// 私有构造函数， 此类不允许在外部实例化。 <see cref="CleversafeFeature"/> class.
        /// </summary>

        private CleversafeFeature(int type, string culture)
        {
            this.Init(type, culture);
        }

        protected override void GenerateDocAveGUIFeatureUnit(CultureInfo culture)
        {
            StorageFeature sf = new StorageFeature();
            StorageType type = new StorageType();
            type.Value = "Cleversafe";
            type.Display = CleversafeI18N.ResourceManager.GetString("MediaStorage_Cleversafe_Cleversafe", culture);
            type.Index = 601;
            type.Vim = new List<string>() { "cleversafe_vim" };
            sf.Type = type;
            sf.Type.DefaultXris.Add("DOCAVE-XAM://CLEVERSAFE_VIM?".ToLower(CultureInfo.InvariantCulture));
            sf.Description = CleversafeI18N.ResourceManager.GetString("MediaStorage_Cleversafe_Configure_your_bucket_name_Access_Key_ID_and_Secret_Access_Key", culture);
            sf.Features = new List<FeatureUnit> {
                        new FeatureUnit() {Vim = "cleversafe_vim", DisplayName = CleversafeI18N.ResourceManager.GetString("MediaStorage_Cleversafe_Vault_Name",culture), Tag = "cloud_cleversafe", Key = "vaultName".ToLower(CultureInfo.InvariantCulture), KeyName = "CleversafeBucketName", Visibility="Visible", ValType = "string", IsRequiredOption = true, GuiType = "TextBox", DemoValue="DocAve", ValidateRegPats = new List<string>(){
                            @"^[_A-Za-z0-9\.\-]+$|^$" + "\t0\t"+CleversafeI18N.ResourceManager.GetString("MediaStorage_Cleversafe_Can_contain_letters_numbers_periods_underscores_and_dashes", culture),
                            "^[A-Za-z0-9]+.*$|^$" + "\t0\t"+CleversafeI18N.ResourceManager.GetString("MediaStorage_Cleversafe_Must_start_with_a_number_or_letter", culture),
                            @"^[_A-Za-z0-9\.\-]{3,255}$|^$" + "\t0\t"+CleversafeI18N.ResourceManager.GetString("MediaStorage_Cleversafe_Must_be_between_3_and_255_characters_long", culture),
                            @".*\d{1,3}\.\d{1,3}\.\d{1,3}\.\d{1,3}.*" + "\t1\t"+CleversafeI18N.ResourceManager.GetString("MediaStorage_Cleversafe_Must_not_be_formatted_as_an_IP_address", culture)
                        }},
                        new FeatureUnit() {Vim = "cleversafe_vim", DisplayName = CleversafeI18N.ResourceManager.GetString("MediaStorage_Cleversafe_Access_Key_ID",culture), Tag = "cloud_cleversafe", Key = "name", KeyName = "CleversafeUsername", Visibility="Visible", ValType = "string", IsRequiredOption = true, GuiType = "TextBox", DemoValue=CleversafeI18N.ResourceManager.GetString("MediaStorage_Cleversafe_Access_Key_ID_Demo", culture)},
                        new FeatureUnit() {Vim = "cleversafe_vim", DisplayName = CleversafeI18N.ResourceManager.GetString("MediaStorage_Cleversafe_Secret_Access_Key",culture), Tag = "cloud_cleversafe", Key = "secret", KeyName = "CleversafeAPIKey", Visibility="Visible", ValType = "string", IsRequiredOption = true, GuiType = "PasswordBox"},
                        new FeatureUnit() {Vim = "cleversafe_vim", DisplayName = CleversafeI18N.ResourceManager.GetString("MediaStorage_Cleversafe_Storage_Accesser_IPs",culture), Tag = "cloud_cleversafe", Value = "accesser_ip", Key = "accesser_ip", KeyName = "cleversafeAccesserIPs", Visibility="Visible", ValType = "string",  IsRequiredOption = true, GuiType = "TextBox", ValidateRegPats = new List<string>(){
                            @"^((?:(25[0-5]|2[0-4][0-9]|[01]?[0-9][0-9]?)\.){3}(?:(25[0-5]|2[0-4][0-9]|[01]?[0-9][0-9]?))\;)*(?:(25[0-5]|2[0-4][0-9]|[01]?[0-9][0-9]?)\.){3}(?:(25[0-5]|2[0-4][0-9]|[01]?[0-9][0-9]?))$"+ "\t0\t" + CleversafeI18N.ResourceManager.GetString("MediaStorage_Cleversafe_AccesserIPs_Must_be_separated_with_semicolon", culture)
                        }},
                        new FeatureUnit() {Vim = "cleversafe_vim", DisplayName = CleversafeI18N.ResourceManager.GetString("MediaStorage_Cleversafe_Advanced",culture), Tag = "cloud_cleversafe", Key = "advanced".ToLower(CultureInfo.InvariantCulture), Value = "cleversafe_vim_ExtendedParameters",  KeyName = "cleversafeAdvanced", Visibility="Visible", ValType = "string", GuiType = "CheckBox", ChildFeatures = new List<FeatureUnit>(){
                        new FeatureUnit() {Vim = "cleversafe_vim", DisplayName = CleversafeI18N.ResourceManager.GetString("MediaStorage_Cleversafe_Extended_Parameters",culture), Tag = "cleversafe_vim_ExtendedParameters", Key = "ExtendedParameters".ToLower(CultureInfo.InvariantCulture), KeyName = "cleversafeExtendedParameters", Visibility="Collapsed", ValType = "string", GuiType = "TextArea", CanNullOrEmpty = "true"}}}
            };

            Add(sf);
        }

        #endregion

        private static Object locker = new object();
        private static readonly Dictionary<string, CleversafeFeature> instances = new Dictionary<string, CleversafeFeature>();

        public static CleversafeFeature Getstances(Int32 type, String culture = "en")
        {
            lock (locker)
            {
                if (!instances.ContainsKey(type + culture))
                {
                    CleversafeFeature cleversafe = new CleversafeFeature(type, culture);
                    foreach (var feature in cleversafe.FeatureObjs)
                    {
                        feature.AdvancedOptions.AddRange(new List<string>()
                            {
                                "RetryInterval=30000",
                                "RetryCount=6",
                                "CustomizedMetadata={[testKey1,testValue1],[testKey2,testValue2],[testKey3,testValue3]}}",
                                "CustomizedMode=Close",
                                "CustomizedMode=SupportAll",
                                "CustomizedMode=DocAveOnly",
                                "CustomizedMode=CustomizedOnly"
                            });
                    }
                    instances[type + culture] = cleversafe;
                    return cleversafe;
                }
                else
                {
                    return instances[type + culture];
                }


            }
        }
    }
}
