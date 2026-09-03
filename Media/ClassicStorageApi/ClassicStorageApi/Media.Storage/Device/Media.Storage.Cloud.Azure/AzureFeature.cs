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




namespace AvePoint.Media.ClassicStorage.Cloud.Azure
{
    #region using directives
    using System;
    using System.Collections.Generic;
    using System.Globalization;
    using System.Text;
    using AvePoint.Media.ClassicStorage.Cloud.Common;
    using AvePoint.Media.ClassicStorage.Resources.AzureI18N;
    #endregion

    /// <summary>
    /// 此类用于描述Azure Cloud类型的存储介质的特性
    /// </summary>
    /// 
    class AzureFeature : StorageFeature
    {
        #region AzureFeature 自己负责初始化的部分, 在Application生存周期内只会初始化一次, 外部不需要知道的逻辑
        /// <summary>
        /// 私有构造函数， 此类不允许在外部实例化。 <see cref="AzureFeature"/> class.
        /// </summary>
        private AzureFeature(int type,string culture)
        {
            this.Init(type,culture);
        }

        protected override void GenerateSingleTypeFeatureUnit(CultureInfo culture)
        {
            StorageFeature sf = new StorageFeature();
            StorageType type = new StorageType();
            type.Value = "Azure";
            type.Display = AzureI18N.ResourceManager.GetString("MediaStorage_Azure_Windows_Azure_Storage", culture);
            type.Index = 403;
            type.Vim = new List<string>() { "azure_vim" };
            sf.Type = type;
            sf.Type.DefaultXris.Add("DOCAVE-XAM://AZURE_VIM?ACCESSPOINT=HTTP://BLOB.CORE.WINDOWS.NET&CDNED=FALSE".ToLower(CultureInfo.InvariantCulture));
            sf.Description = AzureI18N.ResourceManager.GetString("MediaStorage_Azure_Configure_your_container_name_account_name_and_account_key", culture);
            sf.Features = new List<FeatureUnit> {
                        new FeatureUnit() {Vim = "azure_vim", DisplayName = "Access Point", Tag = "cloud_azure", Key = "accessPoint".ToLower(CultureInfo.InvariantCulture), KeyName = "AzureAccessPoint", Visibility="Visible", ValType = "string", GuiType = "TextBox", DefaultValue="http://blob.core.windows.net"},
                        new FeatureUnit() {Vim = "azure_vim", DisplayName = "Container Name", Tag = "cloud_azure", Key = "containerName".ToLower(CultureInfo.InvariantCulture), KeyName = "AzureContainerName", Visibility="Visible", ValType = "string", GuiType = "TextBox", ValidateRegPats = new List<string>(){
                            @"^[a-z0-9]+[a-z0-9\-]*$" + "\t0\t" + AzureI18N.ResourceManager.GetString("MediaStorage_Azure_Container_names_must_start_with_a_letter_or_number_and_can_contain_only_letters_numbers_and_the_dash_character", culture),
                            @".*\-\-.*" + "\t0\t" + AzureI18N.ResourceManager.GetString("MediaStorage_Azure_Every_dash_character_must_be_immediately_preceded_and_followed_by_a_letter_or_number", culture),
                            @"^[a-z0-9\-]{3,63}$" + "\t0\t" + AzureI18N.ResourceManager.GetString("MediaStorage_Azure_Container_names_must_be_from_3_through_63_characters_long", culture)
                        }},
                        new FeatureUnit() {Vim = "azure_vim", DisplayName = "Account Name", Tag = "cloud_azure", Key = "name", KeyName = "AzureUsername", Visibility="Visible", ValType = "string", GuiType = "TextBox"},
                        new FeatureUnit() {Vim = "azure_vim", DisplayName = "Account Key", Tag = "cloud_azure", Key = "secret", KeyName = "AzureAPIKey", Visibility="Visible", ValType = "string", GuiType = "PasswordBox"},
            };

            Add(sf);
        }

        protected override void GenerateDocAveGUIFeatureUnit(CultureInfo culture)
        {

            StorageFeature sf = new StorageFeature();
            StorageType type = new StorageType();
            type.Value = "Azure";
            type.Display = AzureI18N.ResourceManager.GetString("MediaStorage_Azure_Windows_Azure_Storage", culture);
            type.Index = 403;
            type.Vim = new List<string>() { "azure_vim" };
            sf.Type = type;
            sf.Type.DefaultXris.Add("DOCAVE-XAM://AZURE_VIM?ACCESSPOINT=HTTP://BLOB.CORE.WINDOWS.NET&CDNED=FALSE".ToLower(CultureInfo.InvariantCulture));
            //sf.Description = AzureI18N.ResourceManager.GetString("MediaStorage_Azure_Configure_your_container_name_account_name_and_account_key", culture);//Configure_your_container_name_storage_account_and_access_key
            sf.Features = new List<FeatureUnit>
            {
                //http://myaccount.blob.core.windows.net
                new FeatureUnit()
                {
                    Vim = "azure_vim",
                    DisplayName = AzureI18N.ResourceManager.GetString("MediaStorage_Azure_Access_Point", culture),
                    Tag = "cloud_azure",
                    Key = "accessPoint".ToLower(CultureInfo.InvariantCulture),
                    KeyName = "AzureAccessPoint",
                    Visibility = "Visible",
                    ValType = "string",
                    GuiType = "TextBox",
                    DefaultValue = "http://blob.core.windows.net"
                },
                new FeatureUnit()
                {
                    Vim = "azure_vim",
                    DisplayName = AzureI18N.ResourceManager.GetString("MediaStorage_Azure_Container_Name", culture),
                    Tag = "cloud_azure",
                    Key = "containerName".ToLower(CultureInfo.InvariantCulture),
                    KeyName = "AzureContainerName",
                    Visibility = "Visible",
                    ValType = "string",
                    IsRequiredOption = true,
                    GuiType = "TextBox",
                    DemoValue = "docave",
                    ValidateRegPats = new List<string>()
                    {
                        @"^[a-z0-9]+[a-z0-9\-]*$|^$" + "\t0\t" +
                        AzureI18N.ResourceManager.GetString(
                            "MediaStorage_Azure_Container_names_must_start_with_a_letter_or_number_and_can_contain_only_letters_numbers_and_the_dash_character",
                            culture),
                        @".*\-\-.*" + "\t1\t" +
                        AzureI18N.ResourceManager.GetString(
                            "MediaStorage_Azure_Every_dash_character_must_be_immediately_preceded_and_followed_by_a_letter_or_number",
                            culture),
                        @"^[a-z0-9\-]{3,63}$|^$" + "\t0\t" +
                        AzureI18N.ResourceManager.GetString(
                            "MediaStorage_Azure_Container_names_must_be_from_3_through_63_characters_long", culture)
                    }
                },
                new FeatureUnit()
                {
                    Vim = "azure_vim",
                    DisplayName = AzureI18N.ResourceManager.GetString("MediaStorage_Azure_Account_Name", culture),
                    Tag = "cloud_azure",
                    Key = "name",
                    KeyName = "AzureUsername",
                    Visibility = "Visible",
                    ValType = "string",
                    IsRequiredOption = true,
                    GuiType = "TextBox",
                    DemoValue = @"Username"
                },
                new FeatureUnit()
                {
                    Vim = "azure_vim",
                    DisplayName = AzureI18N.ResourceManager.GetString("MediaStorage_Azure_Account_Key", culture),
                    Tag = "cloud_azure",
                    Key = "secret",
                    KeyName = "AzureAPIKey",
                    Visibility = "Visible",
                    ValType = "string",
                    IsRequiredOption = true,
                    GuiType = "PasswordBox"
                },
                new FeatureUnit()
                {
                    Vim = "azure_vim",
                    DisplayName = AzureI18N.ResourceManager.GetString("MediaStorage_Azure_Advanced", culture),
                    Tag = "cloud_azure",
                    Key = "advanced".ToLower(CultureInfo.InvariantCulture),
                    Value = "azure_vim_ExtendedParameters",
                    KeyName = "AzureAdvanced",
                    Visibility = "Visible",
                    ValType = "string",
                    GuiType = "CheckBox",
                    ChildFeatures = new List<FeatureUnit>()
                    {
                        new FeatureUnit()
                        {
                            Vim = "azure_vim",
                            DisplayName =
                                AzureI18N.ResourceManager.GetString("MediaStorage_Azure_Extended_Parameters", culture),
                            Tag = "azure_vim_ExtendedParameters",
                            Key = "ExtendedParameters".ToLower(CultureInfo.InvariantCulture),
                            KeyName = "AzureExtendParameters",
                            Visibility = "Collapsed",
                            ValType = "string",
                            GuiType = "TextArea",
                            CanNullOrEmpty = "true"
                        }
                    }
                }
            };
            Add(sf);
        }

        protected override void GenerateConnectorGUIFeatureUnit(CultureInfo culture)
        {
            StorageFeature sf = new StorageFeature();
            StorageType type = new StorageType();
            type.Value = "Azure";
            type.Display = AzureI18N.ResourceManager.GetString("MediaStorage_Azure_Windows_Azure_Storage", culture);
            type.Index = 403;
            type.Vim = new List<string>() { "azure_vim" };
            sf.Type = type;
            sf.Description = AzureI18N.ResourceManager.GetString("MediaStorage_Azure_Configure_your_container_name_account_name_and_account_key", culture);//Configure_your_container_name_storage_account_and_access_key
            sf.Features = new List<FeatureUnit> {
                        new FeatureUnit() {Vim = "azure_vim", DisplayName = AzureI18N.ResourceManager.GetString("MediaStorage_Azure_Container_Name",culture), Tag = "cloud_azure", Key = "containerName", KeyName = "AzureContainerName", Visibility="Visible", ValType = "string", GuiType = "TextBox", FeatureFlag=(int)(FeatureUnitFlag.CloudContainer|FeatureUnitFlag.Path) ,  CanModifi = false,   ValidateRegPats = new List<string>(){
                            @"^[a-z0-9]+[a-z0-9\-]*$|^$" + "\t0\t" + AzureI18N.ResourceManager.GetString("MediaStorage_Azure_Container_names_must_start_with_a_letter_or_number_and_can_contain_only_letters_numbers_and_the_dash_character", culture),
                            @".*\-\-.*" + "\t1\t" + AzureI18N.ResourceManager.GetString("MediaStorage_Azure_Every_dash_character_must_be_immediately_preceded_and_followed_by_a_letter_or_number", culture),
                            @"^[a-z0-9\-]{3,63}$|^$" + "\t0\t" + AzureI18N.ResourceManager.GetString("MediaStorage_Azure_Container_names_must_be_from_3_through_63_characters_long", culture)
                        }},
                        new FeatureUnit() {Vim = "azure_vim", DisplayName = AzureI18N.ResourceManager.GetString("MediaStorage_Azure_Account_Name",culture), Tag = "cloud_azure", Key = "name", KeyName = "AzureUsername", Visibility="Visible", ValType = "string", GuiType = "TextBox"},
                        new FeatureUnit() {Vim = "azure_vim", DisplayName = AzureI18N.ResourceManager.GetString("MediaStorage_Azure_Account_Key",culture), Tag = "cloud_azure", Key = "secret", KeyName = "AzureAPIKey", Visibility="Visible", ValType = "string", GuiType = "PasswordBox"},
                        new FeatureUnit() {Vim = "azure_vim", DisplayName = AzureI18N.ResourceManager.GetString("MediaStorage_Azure_Advanced",culture), Tag = "cloud_azure", Key = "advanced".ToLower(CultureInfo.InvariantCulture), Value = "azure_vim_ExtendedParameters",  KeyName = "AzureWithParamsExtension", Visibility="Visible", ValType = "string", GuiType = "CheckBox", ChildFeatures = new List<FeatureUnit>(){
                            new FeatureUnit() {Vim = "azure_vim", DisplayName = AzureI18N.ResourceManager.GetString("MediaStorage_Azure_Extended_Parameters",culture), Tag = "azure_vim_ExtendedParameters", Key = "ExtendedParameters".ToLower(CultureInfo.InvariantCulture), KeyName = "AzureExtendParams", Visibility="Collapsed", ValType = "string", GuiType = "TextArea", CanNullOrEmpty = "true"}}}
            };

            Add(sf);
        }

        #endregion

        /// <summary>
        /// 供VIM调用, 获取相应的Feature
        /// </summary>
        private static readonly Dictionary<string, AzureFeature> instances = new Dictionary<string, AzureFeature>();

        public static AzureFeature Getstances(int type, string culture = "en")
        {
            if (!instances.ContainsKey(type + culture))
            {
                AzureFeature azure = new AzureFeature(type, culture);
                instances[type + culture] = azure;
                return azure;
            }
            else
            {
                return instances[type + culture];
            }
        }
    }
}
