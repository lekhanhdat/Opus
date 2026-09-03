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



namespace AvePoint.Media.Storage.FS
{
    #region using directives
    using AvePoint.Media.Storage.Resources.FSI18N;
    using System;
    using System.Collections.Generic;
    using System.Globalization;

    #endregion

    /// <summary>
    /// 此类用于描述Net Share类型的存储介质的特性
    /// </summary>
    /// 
    sealed class FSFeature : StorageFeature
    {

        #region FSFeature 自己负责初始化的部分, 在Application生存周期内只会初始化一次, 外部不需要知道的逻辑
        /// <summary>
        /// 私有构造函数， 此类不允许在外部实例化。 <see cref="FSFeature"/> class.
        /// </summary>
        private FSFeature(int type, string culture)
        {
            this.Init(type, culture);
        }

        protected override void GenerateSingleTypeFeatureUnit(CultureInfo culture)
        {
            StorageFeature sf = new StorageFeature();
            StorageType fs = new StorageType();
            fs.Value = "FS";
            fs.Display = FSI18N.ResourceManager.GetString("MediaStorage_FS_Net_Share", culture);
            fs.Index = 0;
            fs.IsSupportCustomAction = true;
            fs.Vim = new List<string>() { "fs_vim" };
            sf.Type = fs;
            sf.Type.DefaultXris = new List<string>() { "docave-xam://fs_vim?" };
            sf.IsNeedSpaceThreshold = true;
            sf.ProgressForeground = new FeatureColor(255, 11, 145, 146);

            sf.Features = new List<FeatureUnit>() {
                new FeatureUnit() {Vim = "fs_vim", DisplayName = FSI18N.ResourceManager.GetString("MediaStorage_FS_UNC_Path",culture), Tag = "fs_path", Key = "location", KeyName = "UNCPath", Visibility = "Visible", ValType="string", GuiType="TextBox" ,ValidateRegPats = new List<string>(){
                    @"^\\\\[^\\]+\\[^\\]+(\\[^\\]+)*\\?$|^$" + "\t0\t" + FSI18N.ResourceManager.GetString("MediaStorage_FS_The_path_format_is_not_correct", culture),
                    @"^.{0,200}$" + "\t0\t" + FSI18N.ResourceManager.GetString("MediaStorage_FS_The_UNC_path_cannot_exceed_200_characters", culture),
                    @"^.*/.*$" + "\t1\t"+FSI18N.ResourceManager.GetString("MediaStorage_FS_The_UNC_path_cannot_contain_a_forward_slash", culture)
                }},

                new FeatureUnit() {Vim = "fs_vim", DisplayName = FSI18N.ResourceManager.GetString("MediaStorage_FS_Username",culture), Tag = "fs_username", Key = "name", KeyName = "UNCUsername", Visibility = "Visible", ValType="string", GuiType="TextBox",ValidateRegPats = new List<string>(){
                    @"^[^\\@]+[\\@]{1}[^\\@]+$" + "\t0\t" + FSI18N.ResourceManager.GetString("MediaStorage_FS_Please_enter_the_username_in_the_format_domain_username", culture)
                }},
                new FeatureUnit() {Vim = "fs_vim", DisplayName = FSI18N.ResourceManager.GetString("MediaStorage_FS_Password",culture), Tag = "fs_password", Key = "secret", KeyName = "UNCPassword", Visibility = "Visible", ValType="string", GuiType="PasswordBox"}
            };
            Add(sf);
        }

        protected override void GenerateDocAveGUIFeatureUnit(CultureInfo culture)
        {
            StorageFeature sf = new StorageFeature();
            StorageType fs = new StorageType();
            fs.Value = "FS";
            fs.Display = FSI18N.ResourceManager.GetString("MediaStorage_FS_Net_Share", culture);
            fs.Index = 0;
            fs.IsSupportCustomAction = true;
            fs.Vim = new List<string>() { "fs_vim" };
            sf.Type = fs;
            sf.Type.DefaultXris = new List<string>() { "docave-xam://fs_vim?" };
            sf.IsNeedSpaceThreshold = true;
            sf.ProgressForeground = new FeatureColor(255, 85, 204, 204);

            sf.Features = new List<FeatureUnit>() {
                new FeatureUnit() {Vim = "fs_vim", DisplayName = FSI18N.ResourceManager.GetString("MediaStorage_FS_UNC_Path",culture), Tag = "fs_path", Key = "location", KeyName = "UNCPath", Visibility = "Visible", ValType="string",FeatureFlag=1, IsRequiredOption = true, GuiType="TextBox" ,ValidateRegPats = new List<string>(){
                    @"^\\\\[^\\]+\\[^\\]+(\\[^\\]+)*\\?$|^$" + "\t0\t" + FSI18N.ResourceManager.GetString("MediaStorage_FS_The_path_format_is_not_correct", culture),
                    @"^.{0,200}$" + "\t0\t" + FSI18N.ResourceManager.GetString("MediaStorage_FS_The_UNC_path_cannot_exceed_200_characters", culture),
                    @"^.{1,}$" + "\t0\t" + FSI18N.ResourceManager.GetString("MediaStorage_FS_The_UNC_Path_cannot_be_empty", culture),
                    @"^.*/.*$" + "\t1\t"+FSI18N.ResourceManager.GetString("MediaStorage_FS_The_UNC_path_cannot_contain_a_forward_slash", culture)
                }},
                new FeatureUnit() {
                    Vim = "fs_vim",
                    DisplayName = FSI18N.ResourceManager.GetString("MediaStorage_FS_Account_Profile", culture),
                    Tag = "fs_account",
                    Key = "profile",
                    KeyName = "FSAccountProfile",
                    Visibility = "Visible",
                    ValType = "string",
                    Value = "FS_AccountProfile",
                    IsRequiredOption = true,
                    GuiType = "AUICreateNewComboBox",
                },
                new FeatureUnit() {
                    Vim = "fs_vim",
                    DisplayName = FSI18N.ResourceManager.GetString("MediaStorage_FS_System_Profile", culture),
                    Tag = "fs_system_profile",
                    Key = "systemProfile",
                    KeyName = "FSSystemProfile",
                    Visibility = "Visible",
                    ValType = "string",
                    Value = "FS_SystemProfile",
                    IsRequiredOption = false,
                    GuiType = "AUICreateNewComboBox",
                },
                //new FeatureUnit() {Vim = "fs_vim", DisplayName = FSI18N.ResourceManager.GetString("MediaStorage_FS_Username",culture), Tag = "fs_username", Key = "name", KeyName = "UNCUsername", Visibility = "Visible", ValType="string", IsRequiredOption = true, GuiType="TextBox",ValidateRegPats = new List<string>(){
                //    @"^[^\\@]+[\\@]{1}[^\\@]+$" + "\t0\t" + FSI18N.ResourceManager.GetString("MediaStorage_FS_Please_enter_the_username_in_the_format_domain_username", culture)
                //}},
                //new FeatureUnit() {Vim = "fs_vim", DisplayName = FSI18N.ResourceManager.GetString("MediaStorage_FS_Password",culture), Tag = "fs_password", Key = "secret", KeyName = "UNCPassword", Visibility = "Visible", ValType="string", IsRequiredOption = true, GuiType="PasswordBox"},

                new FeatureUnit() {Vim = "fs_vim", DisplayName = FSI18N.ResourceManager.GetString("MediaStorage_FS_Advanced",culture), Tag = "fs_vim_advanced", Key = "advanced".ToLower(CultureInfo.InvariantCulture), Value = "fs_vim_ExtendedParameters",  KeyName = "FSAdvanced", Visibility="Visible", ValType = "string", GuiType = "CheckBox", ChildFeatures = new List<FeatureUnit>(){
                   new FeatureUnit() {Vim = "fs_vim", DisplayName = FSI18N.ResourceManager.GetString("MediaStorage_FS_Extended_parameters",culture), Tag = "fs_vim_ExtendedParameters", Key = "ExtendedParameters".ToLower(CultureInfo.InvariantCulture), KeyName = "FSExtendedParameters", Visibility="Collapsed", ValType = "string", GuiType = "TextArea", CanNullOrEmpty = "true"}
                }}
            };
            Add(sf);
            AddIBMStorwizeFamily(culture);
            AddNFS(culture);
        }

        private void AddNFS(CultureInfo culture)
        {
            var storageFeature = new StorageFeature();
            var nfsType = new StorageType();
            nfsType.Value = "NFS";
            nfsType.Display = FSI18N.ResourceManager.GetString("MediaStorage_NFS_Net_Share", culture);
            nfsType.Index = 13;
            nfsType.Vim = new List<string>() { "nfs_vim" };
            storageFeature.Type = nfsType;
            storageFeature.Type.DefaultXris = new List<string>() { "docave-xam://nfs_vim?" };
            storageFeature.Features = new List<FeatureUnit>() {
                new FeatureUnit() {Vim = "nfs_vim", DisplayName = FSI18N.ResourceManager.GetString("MediaStorage_NFS_UNC_Path",culture), Tag = "nfs_path", Key = "location", KeyName = "NFS_UNCPath", Visibility = "Visible", ValType="string",IsRequiredOption = true, GuiType="TextBox", FeatureFlag=(int)FeatureUnitFlag.Path, ValidateRegPats = new List<string>(){
                    @"^\\\\[^\\]+\\[^\$]+$" + "\t0\t" + FSI18N.ResourceManager.GetString("MediaStorage_NFS_The_path_format_is_not_correct", culture),
                    @"^.{0,200}$" + "\t0\t" + FSI18N.ResourceManager.GetString("MediaStorage_NFS_The_UNC_path_cannot_exceed_200_characters", culture),
                    @"^.{1,}$" + "\t0\t" + FSI18N.ResourceManager.GetString("MediaStorage_NFS_The_UNC_Path_cannot_be_empty", culture),
                    @"^.*/.*$" + "\t1\t"+FSI18N.ResourceManager.GetString("MediaStorage_NFS_The_UNC_path_cannot_contain_a_forward_slash", culture)
                }},
                new FeatureUnit() {
                    Vim = "nfs_vim",
                    DisplayName = FSI18N.ResourceManager.GetString("MediaStorage_NFS_Account_Profile", culture),
                    Tag = "nfs_account",
                    Key = "profile",
                    KeyName = "NFSAccountProfile",
                    Visibility = "Visible",
                    ValType = "string",
                    Value = "NFS_AccountProfile",
                    IsRequiredOption = false,
                    GuiType = "AUICreateNewComboBox"
                },
                new FeatureUnit() {Vim = "nfs_vim", DisplayName =FSI18N.ResourceManager.GetString("MediaStorage_NFS_Advanced",culture), Tag = "nfs_vim_advanced", Key = "advanced".ToLower(CultureInfo.InvariantCulture), Value = "nfs_vim_ExtendedParameters",  KeyName = "NFSAdvanced", Visibility="Visible", ValType = "string", GuiType = "CheckBox", ChildFeatures = new List<FeatureUnit>(){
                   new FeatureUnit() {Vim = "nfs_vim", DisplayName = FSI18N.ResourceManager.GetString("MediaStorage_NFS_Extended_parameters",culture), Tag = "nfs_vim_ExtendedParameters", Key = "ExtendedParameters".ToLower(CultureInfo.InvariantCulture), KeyName = "NFSExtendedParameters", Visibility="Collapsed", ValType = "string", GuiType = "TextArea", CanNullOrEmpty = "true"}
                }}
            };
            Add(storageFeature);
        }

        private void AddIBMStorwizeFamily(CultureInfo culture)
        {
            StorageFeature sf = new StorageFeature();
            StorageType fs = new StorageType();
            fs.Value = "IBMStorwizeFamily";
            fs.Display = FSI18N.ResourceManager.GetString("MediaStorage_FS_IBM_Storwize_Family", culture);
            fs.Index = 12;
            fs.IsSupportCustomAction = true;
            fs.Vim = new List<string>() { "ibm_vim" };
            sf.Type = fs;
            sf.Type.DefaultXris = new List<string>() { "docave-xam://ibm_vim?" };
            sf.IsNeedSpaceThreshold = true;
            sf.ProgressForeground = new FeatureColor(255, 85, 204, 204);

            sf.Features = new List<FeatureUnit>() {
                new FeatureUnit() {Vim = "ibm_vim", DisplayName = FSI18N.ResourceManager.GetString("MediaStorage_FS_UNC_Path",culture), Tag = "ibm_path", Key = "location", KeyName = "IBM_UNCPath", Visibility = "Visible", ValType="string",FeatureFlag=1, IsRequiredOption = true, GuiType="TextBox" ,ValidateRegPats = new List<string>(){
                    @"^\\\\[^\\]+\\[^\\]+(\\[^\\]+)*\\?$|^$" + "\t0\t" + FSI18N.ResourceManager.GetString("MediaStorage_FS_The_path_format_is_not_correct", culture),
                    @"^.{0,200}$" + "\t0\t" + FSI18N.ResourceManager.GetString("MediaStorage_FS_The_UNC_path_cannot_exceed_200_characters", culture),
                    @"^.{1,}$" + "\t0\t" + FSI18N.ResourceManager.GetString("MediaStorage_FS_The_UNC_Path_cannot_be_empty", culture),
                    @"^.*/.*$" + "\t1\t"+FSI18N.ResourceManager.GetString("MediaStorage_FS_The_UNC_path_cannot_contain_a_forward_slash", culture)
                }},
                new FeatureUnit() {
                    Vim = "ibm_vim",
                    DisplayName = FSI18N.ResourceManager.GetString("MediaStorage_FS_Account_Profile", culture),
                    Tag = "ibm_account",
                    Key = "profile",
                    KeyName = "IBMAccountProfile",
                    Visibility = "Visible",
                    ValType = "string",
                    Value = "IBM_AccountProfile",
                    IsRequiredOption = true,
                    GuiType = "AUICreateNewComboBox",
                },
                new FeatureUnit() {Vim = "ibm_vim", DisplayName = FSI18N.ResourceManager.GetString("MediaStorage_FS_Advanced",culture), Tag = "ibm_vim_advanced", Key = "advanced".ToLower(CultureInfo.InvariantCulture), Value = "ibm_vim_ExtendedParameters",  KeyName = "IBMAdvanced", Visibility="Visible", ValType = "string", GuiType = "CheckBox", ChildFeatures = new List<FeatureUnit>(){
                   new FeatureUnit() {Vim = "ibm_vim", DisplayName = FSI18N.ResourceManager.GetString("MediaStorage_FS_Extended_parameters",culture), Tag = "ibm_vim_ExtendedParameters", Key = "ExtendedParameters".ToLower(CultureInfo.InvariantCulture), KeyName = "IBMExtendedParameters", Visibility="Collapsed", ValType = "string", GuiType = "TextArea", CanNullOrEmpty = "true"}
                }}
            };
            Add(sf);
        }

        private void AddConnectorIBMStorwizeFamily(CultureInfo culture)
        {
            StorageFeature sf = new StorageFeature();
            StorageType connFs = new StorageType();
            connFs.Value = "IBMStorwizeFamily";
            connFs.Display = FSI18N.ResourceManager.GetString("MediaStorage_Connector_FS_IBM_Storwize_Family", culture);
            connFs.Index = 12;
            connFs.Vim = new List<string>() { "ibm_vim" };
            sf.Type = connFs;
            sf.Description = FSI18N.ResourceManager.GetString("MediaStorage_Connector_FS_Configure_your_net_share_path_username_and_password");
            sf.Features = new List<FeatureUnit>() {
                new FeatureUnit() {IsRequiredOption = true, Vim = "ibm_vim", DisplayName = FSI18N.ResourceManager.GetString("MediaStorage_Connector_FS_UNC_Path",culture), Tag = "ibm_path", Key = "location", KeyName = "IBM_UNCPath", Visibility = "Visible", ValType="string", GuiType="TextBox", DemoValue = FSI18N.ResourceManager.GetString("MediaStorage_Connector_FS_Path_Demo",culture), FeatureFlag=(int)FeatureUnitFlag.Path ,  CanModifi = false, ValidateRegPats = new List<string>(){
                    @"^\\\\[^\\]+\\[^\\]+(\\[^\\]+)*\\?$|^$" + "\t0\t" + FSI18N.ResourceManager.GetString("MediaStorage_Connector_FS_The_path_format_is_not_correct", culture),
                    @"^.{0,200}$" + "\t0\t" + FSI18N.ResourceManager.GetString("MediaStorage_Connector_FS_The_UNC_path_cannot_exceed_200_characters", culture),
                    @"^.*/.*$" + "\t1\t"+FSI18N.ResourceManager.GetString("MediaStorage_Connector_FS_The_UNC_path_cannot_contain_a_forward_slash", culture)
                }},
                new FeatureUnit() {IsRequiredOption = true, Vim = "ibm_vim", DisplayName = FSI18N.ResourceManager.GetString("MediaStorage_Connector_FS_Username",culture), Tag = "ibm_username", Key = "name", KeyName = "IBM_UNCUsername", Visibility = "Visible", ValType="string", GuiType="TextBox", DemoValue = FSI18N.ResourceManager.GetString("MediaStorage_Connector_FS_Username_Demo",culture), ValidateRegPats = new List<string>(){
                    @"^[^\\@]+[\\@]{1}[^\\@]+$" + "\t0\t" + FSI18N.ResourceManager.GetString("MediaStorage_Connector_FS_Please_enter_the_username_in_the_format_domain_username", culture),
                }},
                new FeatureUnit() {IsRequiredOption = true, Vim = "ibm_vim", DisplayName =FSI18N.ResourceManager.GetString("MediaStorage_Connector_FS_Password",culture), Tag = "ibm_password", Key = "secret", KeyName = "IBM_UNCPassword", Visibility = "Visible", ValType="string", GuiType="PasswordBox"},
                new FeatureUnit() {IsRequiredOption = true, Vim = "ibm_vim", DisplayName =FSI18N.ResourceManager.GetString("MediaStorage_Connector_FS_Advanced",culture), Tag = "ibm_vim_advanced", Key = "advanced".ToLower(CultureInfo.InvariantCulture), Value = "ibm_vim_ExtendedParameters",  KeyName = "IBMAdvanced", Visibility="Visible", ValType = "string", GuiType = "CheckBox", ChildFeatures = new List<FeatureUnit>(){
                   new FeatureUnit() {IsRequiredOption = true, Vim = "ibm_vim", DisplayName = FSI18N.ResourceManager.GetString("MediaStorage_Connector_FS_Extended_parameters",culture), Tag = "ibm_vim_ExtendedParameters", Key = "ExtendedParameters".ToLower(CultureInfo.InvariantCulture), KeyName = "IBMExtendedParameters", Visibility="Collapsed", ValType = "string", GuiType = "TextArea", CanNullOrEmpty = "true"}
                }}
            };
            Add(sf);
        }

        protected override void GenerateConnectorGUIFeatureUnit(CultureInfo culture)
        {
            StorageFeature sf = new StorageFeature();
            StorageType connFs = new StorageType();
            connFs.Value = "FS";
            connFs.Display = FSI18N.ResourceManager.GetString("MediaStorage_Connector_FS_Net_Share", culture);
            connFs.Index = 0;
            connFs.Vim = new List<string>() { "fs_vim" };
            sf.Type = connFs;
            sf.Description = FSI18N.ResourceManager.GetString("MediaStorage_Connector_FS_Configure_your_net_share_path_username_and_password");
            sf.Features = new List<FeatureUnit>() {
                new FeatureUnit() {IsRequiredOption = true, Vim = "fs_vim", DisplayName = FSI18N.ResourceManager.GetString("MediaStorage_Connector_FS_UNC_Path",culture), Tag = "fs_path", Key = "location", KeyName = "UNCPath", Visibility = "Visible", ValType="string", GuiType="TextBox", DemoValue = FSI18N.ResourceManager.GetString("MediaStorage_Connector_FS_Path_Demo",culture), FeatureFlag=(int)FeatureUnitFlag.Path ,  CanModifi = false, ValidateRegPats = new List<string>(){
                    @"^\\\\[^\\]+\\[^\\]+(\\[^\\]+)*\\?$|^$" + "\t0\t" + FSI18N.ResourceManager.GetString("MediaStorage_Connector_FS_The_path_format_is_not_correct", culture),
                    @"^.{0,200}$" + "\t0\t" + FSI18N.ResourceManager.GetString("MediaStorage_Connector_FS_The_UNC_path_cannot_exceed_200_characters", culture),
                    @"^.*/.*$" + "\t1\t"+FSI18N.ResourceManager.GetString("MediaStorage_Connector_FS_The_UNC_path_cannot_contain_a_forward_slash", culture)
                }},
                new FeatureUnit() {IsRequiredOption = true, Vim = "fs_vim", DisplayName = FSI18N.ResourceManager.GetString("MediaStorage_Connector_FS_Username",culture), Tag = "fs_username", Key = "name", KeyName = "UNCUsername", Visibility = "Visible", ValType="string", GuiType="TextBox", DemoValue = FSI18N.ResourceManager.GetString("MediaStorage_Connector_FS_Username_Demo",culture), ValidateRegPats = new List<string>(){
                    @"^[^\\@]+[\\@]{1}[^\\@]+$" + "\t0\t" + FSI18N.ResourceManager.GetString("MediaStorage_Connector_FS_Please_enter_the_username_in_the_format_domain_username", culture),
                }},
                new FeatureUnit() {IsRequiredOption = true, Vim = "fs_vim", DisplayName =FSI18N.ResourceManager.GetString("MediaStorage_Connector_FS_Password",culture), Tag = "fs_password", Key = "secret", KeyName = "UNCPassword", Visibility = "Visible", ValType="string", GuiType="PasswordBox"},
                new FeatureUnit() {Vim = "fs_vim", DisplayName =FSI18N.ResourceManager.GetString("MediaStorage_Connector_FS_Advanced",culture), Tag = "fs_vim_advanced", Key = "advanced".ToLower(CultureInfo.InvariantCulture), Value = "fs_vim_ExtendedParameters",  KeyName = "FSAdvanced", Visibility="Visible", ValType = "string", GuiType = "CheckBox", ChildFeatures = new List<FeatureUnit>(){
                   new FeatureUnit() {Vim = "fs_vim", DisplayName = FSI18N.ResourceManager.GetString("MediaStorage_Connector_FS_Extended_parameters",culture), Tag = "fs_vim_ExtendedParameters", Key = "ExtendedParameters".ToLower(CultureInfo.InvariantCulture), KeyName = "FSExtendedParameters", Visibility="Collapsed", ValType = "string", GuiType = "TextArea", CanNullOrEmpty = "true"}
                }}
            };
            Add(sf);
            GenerateConnectorWMSFeatureUnit(sf.Features, culture);
            var storageFeature = new StorageFeature();
            var nfsType = new StorageType();
            nfsType.Value = "NFS";
            nfsType.Display = FSI18N.ResourceManager.GetString("MediaStorage_Connector_NFS_Net_Share", culture);
            nfsType.Index = 13;
            nfsType.Vim = new List<string>() { "nfs_vim" };
            storageFeature.Type = nfsType;
            storageFeature.Description = "Specify the Net Share path.";
            storageFeature.Features = new List<FeatureUnit>() {
                new FeatureUnit() {Vim = "nfs_vim", DisplayName = FSI18N.ResourceManager.GetString("MediaStorage_Connector_FS_UNC_Path",culture), Tag = "nfs_path", Key = "location", KeyName = "UNCPath", Visibility = "Visible", ValType="string", GuiType="TextBox", IsRequiredOption = true, DemoValue = FSI18N.ResourceManager.GetString("MediaStorage_Connector_NFS_Path_Demo_Description",culture), FeatureFlag=(int)FeatureUnitFlag.Path ,  CanModifi = false, ValidateRegPats = new List<string>(){
                    @"^\\\\[^\\]+\\[^\$]+$" + "\t0\t" + FSI18N.ResourceManager.GetString("MediaStorage_Connector_FS_The_path_format_is_not_correct", culture),
                    @"^.{0,200}$" + "\t0\t" + FSI18N.ResourceManager.GetString("MediaStorage_Connector_FS_The_UNC_path_cannot_exceed_200_characters", culture),
                    @"^.*/.*$" + "\t1\t"+FSI18N.ResourceManager.GetString("MediaStorage_Connector_FS_The_UNC_path_cannot_contain_a_forward_slash", culture)
                }},
                new FeatureUnit() {Vim = "nfs_vim", DisplayName = FSI18N.ResourceManager.GetString("MediaStorage_Connector_FS_Username",culture), Tag = "nfs_username", Key = "name", KeyName = "UNCUsername", Visibility = "Visible", ValType="string", GuiType="TextBox", CanNullOrEmpty = "true", ValidateRegPats = new List<string>(){
                    @"^[^\\@]+[\\@]{1}[^\\@]+$" + "\t0\t" + FSI18N.ResourceManager.GetString("MediaStorage_Connector_FS_Please_enter_the_username_in_the_format_domain_username", culture),
                }},
                new FeatureUnit() {Vim = "nfs_vim", DisplayName =FSI18N.ResourceManager.GetString("MediaStorage_Connector_FS_Password",culture), Tag = "nfs_password", Key = "secret", KeyName = "UNCPassword", Visibility = "Visible", ValType="string", GuiType="PasswordBox", CanNullOrEmpty = "true"},
                new FeatureUnit() {Vim = "nfs_vim", DisplayName =FSI18N.ResourceManager.GetString("MediaStorage_Connector_FS_Advanced",culture), Tag = "nfs_vim_advanced", Key = "advanced".ToLower(CultureInfo.InvariantCulture), Value = "nfs_vim_ExtendedParameters",  KeyName = "NFSAdvanced", Visibility="Visible", ValType = "string", GuiType = "CheckBox", ChildFeatures = new List<FeatureUnit>(){
                   new FeatureUnit() {Vim = "nfs_vim", DisplayName = FSI18N.ResourceManager.GetString("MediaStorage_Connector_FS_Extended_parameters",culture), Tag = "nfs_vim_ExtendedParameters", Key = "ExtendedParameters".ToLower(CultureInfo.InvariantCulture), KeyName = "NFSExtendedParameters", Visibility="Collapsed", ValType = "string", GuiType = "TextArea", CanNullOrEmpty = "true"}
                }}
            };
            Add(storageFeature);
            AddConnectorIBMStorwizeFamily(culture);
            //GenerateConnectorWMSFeatureUnit(storageFeature.Features, culture);
        }
        private void GenerateConnectorWMSFeatureUnit(List<FeatureUnit> fsFeatureUnits, CultureInfo culture)
        {
            StorageFeature sf = new StorageFeature();

            StorageType type = new StorageType();
            type.Value = "WMS_vim".ToLower(CultureInfo.InvariantCulture);
            type.Display = FSI18N.ResourceManager.GetString("MediaStorage_FS_Net_Share_with_WMS", culture);
            type.Index = 14;
            type.Vim = new List<string>() { "WMS_vim".ToLower(CultureInfo.InvariantCulture) };

            sf.Type = type;
            sf.Description = FSI18N.ResourceManager.GetString("MediaStorage_FS_WMS_Configure_Description", culture);
            sf.Features = new List<FeatureUnit>();

            //Add the wms feature units
            sf.Features.Add(new FeatureUnit() { Vim = "WMS_vim".ToLower(CultureInfo.InvariantCulture), DisplayName = FSI18N.ResourceManager.GetString("MediaStorage_FS_WMS_Server_Name", culture), Tag = "WMS_serverName".ToLower(CultureInfo.InvariantCulture), Key = "WMS_serverName".ToLower(CultureInfo.InvariantCulture), KeyName = "WMS_serverName".ToLower(CultureInfo.InvariantCulture), Visibility = "Visible", ValType = "string", IsRequiredOption = true, GuiType = "TextBox" });
            sf.Features.Add(new FeatureUnit()
            {
                Vim = "WMS_vim".ToLower(CultureInfo.InvariantCulture),
                DisplayName = FSI18N.ResourceManager.GetString("MediaStorage_FS_Publishing_Point_Name", culture),
                Tag = "WMS_publish_point".ToLower(CultureInfo.InvariantCulture),
                Key = "WMS_publish_point".ToLower(CultureInfo.InvariantCulture),
                KeyName = "WMS_publish_point".ToLower(CultureInfo.InvariantCulture),
                Visibility = "Visible",
                ValType = "string",
                GuiType = "TextBox",
                IsRequiredOption = true,
                ValidateRegPats = new List<string> {
                @"^.*([<>\\\?%&'#""\{\}\|\^\[\]\*])+.*$" + "\t1\t" + FSI18N.ResourceManager.GetString("MediaStorage_FS_WMS_The_Publishing_Point_Name_Can_Not_Content_Forbidden_Character", culture) }
            });

            //Clone the feature units from fs.
            foreach (FeatureUnit fu in fsFeatureUnits)
            {
                FeatureUnit clone = new FeatureUnit() { Vim = "WMS_vim".ToLower(CultureInfo.InvariantCulture), DisplayName = fu.DisplayName, Tag = fu.Tag, Key = fu.Key, KeyName = fu.KeyName, Visibility = fu.Visibility, ValType = fu.ValType, GuiType = fu.GuiType, DemoValue = fu.DemoValue, FeatureFlag = fu.FeatureFlag, CanModifi = fu.CanModifi, ValidateRegPats = fu.ValidateRegPats, Value = fu.Value, IsRequiredOption = fu.IsRequiredOption };
                if (fu.ChildFeatures != null)
                {
                    clone.ChildFeatures = new List<FeatureUnit>();
                    clone.ChildFeatures.Add(new FeatureUnit()
                    {
                        Vim = "WMS_vim".ToLower(CultureInfo.InvariantCulture),
                        DisplayName = fu.ChildFeatures[0].DisplayName,
                        Tag = fu.ChildFeatures[0].Tag,
                        Key = fu.ChildFeatures[0].Key,
                        KeyName = fu.ChildFeatures[0].KeyName,
                        Visibility = fu.ChildFeatures[0].Visibility,
                        ValType = fu.ChildFeatures[0].ValType,
                        GuiType = fu.ChildFeatures[0].GuiType,
                        DemoValue = fu.ChildFeatures[0].DemoValue,
                        FeatureFlag = fu.ChildFeatures[0].FeatureFlag,
                        CanModifi = fu.ChildFeatures[0].CanModifi,
                        CanNullOrEmpty = fu.ChildFeatures[0].CanNullOrEmpty,
                        IsRequiredOption = fu.ChildFeatures[0].IsRequiredOption,
                    });
                }
                sf.Features.Add(clone);
            }

            Add(sf);
        }
        #endregion

        /// <summary>
        /// 供VIM调用, 获取相应的Feature
        /// </summary>
        private static readonly Dictionary<string, FSFeature> instances = new Dictionary<string, FSFeature>();
        private static Object locker = new Object();
        public static FSFeature Getstances(int type, string culture = "en")
        {
            lock (locker)
            {
                if (!instances.ContainsKey(type + culture))
                {
                    FSFeature fs = new FSFeature(type, culture);
                    foreach (var feature in fs.FeatureObjs)
                    {
                        feature.AdvancedOptions.AddRange(new List<string>()
                    {
                        "IsRetry=true",
                        "IsRetry=false",
                        //1M
                        "BufferSize=1",
                        "SecurelyDelete=true",
                        "SecurelyDelete=false",
                        "AuthMethod=LogonUser",
                        "AuthMethod=NetUse",
                        "AuthMethod=NetUse_DeleteOld",
                        //Control cache manager in device
                        "FileOptions=NoBuffering"
                    });
                    }
                    instances[type + culture] = fs;
                    return fs;
                }
                else
                {
                    return instances[type + culture];
                }
            }
        }
    }
}
