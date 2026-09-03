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

namespace AvePoint.Media.Storage.Cloud.HCP
{
    #region using directives
    using AvePoint.Media.Storage.Resources.HCPI18N;
    using System;
    using System.Collections.Generic;
    using System.Globalization;
    #endregion

    /// <summary>
    /// 此类用于描述Atmos Cloud类型的存储介质的特性
    /// </summary>
    /// 
    class HCPFeature : StorageFeature
    {
        #region AtmosFeature 自己负责初始化的部分, 在Application生存周期内只会初始化一次, 外部不需要知道的逻辑
        /// <summary>
        /// 私有构造函数， 此类不允许在外部实例化。 <see cref="AtmosFeature"/> class.
        /// </summary>
        private HCPFeature(int type, string culture)
        {
            this.Init(type, culture);
        }

        protected override void GenerateSingleTypeFeatureUnit(CultureInfo culture)
        {
            StorageFeature sf = new StorageFeature();
            StorageType type = new StorageType();
            type.Value = "HDS";
             type.Display = HCPI18N.ResourceManager.GetString("MediaStorage_HCP_HDS_Hitachi_Content_Platform",culture);
            type.Index = 406;
            type.Vim = new List<string>() { "HCP_VIM".ToLower(CultureInfo.InvariantCulture) };
            sf.Type = type;
            sf.IsNeedSpaceThreshold = true;
            sf.Type.DefaultXris.Add("DOCAVE-XAM://HCP_VIM?".ToLower(CultureInfo.InvariantCulture));
            //sf.Type.Vim.Add("hcp_vim");　
            sf.Features = new List<FeatureUnit> {
                        new FeatureUnit() {Vim = "HCP_VIM".ToLower(CultureInfo.InvariantCulture), DisplayName = HCPI18N.ResourceManager.GetString("MediaStorage_HCP_Primary_Namespace_Address",culture), Tag = "HDC_HCP".ToLower(CultureInfo.InvariantCulture), Key = "host", KeyName = "HDSPrimaryHost", Visibility="Visible", ValType = "string", IsRequiredOption = true, GuiType = "TextBox", DemoValue="HTTP://NS0.TEN1.HCP.STORAGE4.COM".ToLower(CultureInfo.InvariantCulture)},
                        new FeatureUnit() {Vim = "HCP_VIM".ToLower(CultureInfo.InvariantCulture), DisplayName = HCPI18N.ResourceManager.GetString("MediaStorage_HCP_Secondary_Namespace_Address_Optional",culture), Tag = "HDC_HCP".ToLower(CultureInfo.InvariantCulture), Key = "secondHost".ToLower(CultureInfo.InvariantCulture), KeyName = "HDSSecondHost", Visibility="Visible", ValType = "string", GuiType = "TextBox", DemoValue="HTTP://NS0.TEN2.HCP.STORAGE1.COM".ToLower(CultureInfo.InvariantCulture), CanNullOrEmpty="true"},
                        new FeatureUnit() {Vim = "HCP_VIM".ToLower(CultureInfo.InvariantCulture), DisplayName = HCPI18N.ResourceManager.GetString("MediaStorage_HCP_Root_Folder",culture), Tag ="HDC_HCP".ToLower(CultureInfo.InvariantCulture), Key = "LIB".ToLower(CultureInfo.InvariantCulture), KeyName = "HDSNameSpace", Visibility="Visible", ValType = "string", 
                            ValidateRegPats = new List<string>(){@"^(.*\..*)|(.*\\.*)|(.*/.*)$" + "\t1\t" + HCPI18N.ResourceManager.GetString("MediaStorage_HCP_Cannot_Contain_Periods_Or_Slash", culture)},
                            IsRequiredOption = true, GuiType = "TextBox", DemoValue="DOCAVE".ToLower(CultureInfo.InvariantCulture)},
                        new FeatureUnit() {Vim = "HCP_VIM".ToLower(CultureInfo.InvariantCulture), DisplayName = HCPI18N.ResourceManager.GetString("MediaStorage_HCP_Username",culture), Tag = "HDC_HCP".ToLower(CultureInfo.InvariantCulture), Key = "name", KeyName = "HDSUsername", Visibility="Visible", ValType = "string", IsRequiredOption = true, GuiType = "TextBox"},
                        new FeatureUnit() {Vim = "HCP_VIM".ToLower(CultureInfo.InvariantCulture), DisplayName = HCPI18N.ResourceManager.GetString("MediaStorage_HCP_Password",culture), Tag = "HDC_HCP".ToLower(CultureInfo.InvariantCulture), Key = "secret", KeyName = "HDSPassword", Visibility="Visible", ValType = "string", IsRequiredOption = true, GuiType = "PasswordBox"},
                       // new FeatureUnit() {Vim = "hcp_vim", DisplayName = "Namespace", Tag = "hdc_hcp", Key = "ns", KeyName = "HDSNameSpace", Visibility="Visible", ValType = "string", GuiType = "TextBox", DefaultValue="ns0"},
                        
                        new FeatureUnit() {Vim = "HCP_VIM".ToLower(CultureInfo.InvariantCulture), DisplayName = HCPI18N.ResourceManager.GetString("MediaStorage_HCP_Advanced",culture), Tag = "HDC_VIM_ADVANCED".ToLower(CultureInfo.InvariantCulture), Key = "advanced",Value = "HDC_VIM_EXTENDEDPARAMETERS".ToLower(CultureInfo.InvariantCulture),  KeyName = "HDSAdvanced",Visibility="Visible", ValType = "string", GuiType = "CheckBox",  ChildFeatures = new List<FeatureUnit>() {
                            new FeatureUnit() {Vim = "HCP_VIM".ToLower(CultureInfo.InvariantCulture), DisplayName = HCPI18N.ResourceManager.GetString("MediaStorage_HCP_Extended_Parameters",culture),Tag = "HDC_VIM_EXTENDEDPARAMETERS".ToLower(CultureInfo.InvariantCulture), Key = "EXTENDEDPARAMETERS".ToLower(CultureInfo.InvariantCulture), KeyName = "HDSExtendedParameters",  Visibility="Collapsed", ValType = "string", GuiType = "TextArea", CanNullOrEmpty = "true"}
                            }
                        }        
            
            };
            Add(sf);
        }

        protected override void GenerateDocAveGUIFeatureUnit(CultureInfo culture)
        {
            GenerateSingleTypeFeatureUnit(culture);
        }

        protected override void GenerateConnectorGUIFeatureUnit(CultureInfo culture)
        {
            StorageFeature sf = new StorageFeature();
            StorageType type = new StorageType();
            type.Display = HCPI18N.ResourceManager.GetString("MediaStorage_Connector_HCP_HDS_Hitachi_Content_Platform",culture);
            type.Value = "HDS Hitachi Content Platform";
            type.Index = 406;
            type.Vim = new List<string>() { "HCP_VIM".ToLower(CultureInfo.InvariantCulture) };
            sf.Type = type;
            //sf.IsNeedSpaceThreshold = true; 
            sf.Type.DefaultXris.Add("DOCAVE-XAM://HCP_VIM?".ToLower(CultureInfo.InvariantCulture));
            sf.Description = HCPI18N.ResourceManager.GetString("MediaStorage_Connector_HCP_HCP_Configure_Description", culture);
            //sf.Type.Vim.Add("hcp_vim");　
            sf.Features = new List<FeatureUnit> {
                        new FeatureUnit() {IsRequiredOption = true, Vim = "HCP_VIM".ToLower(CultureInfo.InvariantCulture),  DisplayName = HCPI18N.ResourceManager.GetString("MediaStorage_Connector_HCP_Primary_Namespace_Address",culture), Tag = "HDC_HCP".ToLower(CultureInfo.InvariantCulture), Key = "host", KeyName = "HDSPrimaryHost", Visibility="Visible", ValType = "string", GuiType = "TextBox", DemoValue="HTTP://NS0.TEN1.HCP.STORAGE4.COM".ToLower(CultureInfo.InvariantCulture), CanModifi = false},
                        new FeatureUnit() {Vim = "HCP_VIM".ToLower(CultureInfo.InvariantCulture), DisplayName = HCPI18N.ResourceManager.GetString("MediaStorage_Connector_HCP_Secondary_Namespace_Address_Optional",culture), Tag = "HDC_HCP".ToLower(CultureInfo.InvariantCulture), Key = "secondHost".ToLower(CultureInfo.InvariantCulture), KeyName = "HDSSecondHost", Visibility="Visible", ValType = "string", GuiType = "TextBox", DemoValue="HTTP://NS0.TEN2.HCP.STORAGE1.COM".ToLower(CultureInfo.InvariantCulture), CanNullOrEmpty="true", CanModifi = false},
                        new FeatureUnit() {IsRequiredOption = true, Vim = "HCP_VIM".ToLower(CultureInfo.InvariantCulture), DisplayName = HCPI18N.ResourceManager.GetString("MediaStorage_Connector_HCP_Root_Folder",culture), Tag = "HDC_HCP".ToLower(CultureInfo.InvariantCulture), Key = "LIB".ToLower(CultureInfo.InvariantCulture), KeyName = "HDSNameSpace", Visibility="Visible", ValidateRegPats = new List<string>(){@"^(.*\..*)|(.*\\.*)|(.*/.*)$" + "\t1\t" + HCPI18N.ResourceManager.GetString("MediaStorage_HCP_Cannot_Contain_Periods_Or_Slash", culture)}, ValType = "string", GuiType = "TextBox", DemoValue="DOCAVE".ToLower(CultureInfo.InvariantCulture), FeatureFlag=(int)(FeatureUnitFlag.CloudContainer|FeatureUnitFlag.Path) , CanModifi = false},
                        new FeatureUnit() {IsRequiredOption = true, Vim = "HCP_VIM".ToLower(CultureInfo.InvariantCulture), DisplayName = HCPI18N.ResourceManager.GetString("MediaStorage_Connector_HCP_Username",culture), Tag = "HDC_HCP".ToLower(CultureInfo.InvariantCulture), Key = "name", KeyName = "HDSUsername", Visibility="Visible", ValType = "string", GuiType = "TextBox" },
                        new FeatureUnit() {IsRequiredOption = true, Vim = "HCP_VIM".ToLower(CultureInfo.InvariantCulture), DisplayName = HCPI18N.ResourceManager.GetString("MediaStorage_Connector_HCP_Password",culture), Tag = "HDC_HCP".ToLower(CultureInfo.InvariantCulture), Key = "secret", KeyName = "HDSPassword", Visibility="Visible", ValType = "string", GuiType = "PasswordBox"},
                       // new FeatureUnit() {Vim = "hcp_vim", DisplayName = "Namespace", Tag = "hdc_hcp", Key = "ns", KeyName = "HDSNameSpace", Visibility="Visible", ValType = "string", GuiType = "TextBox", DefaultValue="ns0"},
                        
                        new FeatureUnit() {IsRequiredOption = true, Vim = "HCP_VIM".ToLower(CultureInfo.InvariantCulture), DisplayName = HCPI18N.ResourceManager.GetString("MediaStorage_Connector_HCP_Advanced",culture), Tag = "HDC_VIM_ADVANCED".ToLower(CultureInfo.InvariantCulture), Key = "advanced",Value = "HDC_VIM_EXTENDEDPARAMETERS".ToLower(CultureInfo.InvariantCulture),  KeyName = "HDSAdvanced",Visibility="Visible", ValType = "string", GuiType = "CheckBox",  ChildFeatures = new List<FeatureUnit>() {
                            new FeatureUnit() {IsRequiredOption = true, Vim = "HCP_VIM".ToLower(CultureInfo.InvariantCulture), DisplayName = HCPI18N.ResourceManager.GetString("MediaStorage_Connector_HCP_Extended_Parameters",culture),Tag = "HDC_VIM_EXTENDEDPARAMETERS".ToLower(CultureInfo.InvariantCulture), Key = "ExtendedParameters".ToLower(CultureInfo.InvariantCulture), KeyName = "HDSExtendedParameters",  Visibility="Collapsed", ValType = "string", GuiType = "TextArea", CanNullOrEmpty = "true"}
                            }
                        }        
            
            };
            Add(sf);
        }
        #endregion

        /// <summary>
        /// 供VIM调用, 获取相应的Feature
        /// </summary>
        private static readonly Dictionary<string, HCPFeature> instances = new Dictionary<string, HCPFeature>();
        private static Object locker = new Object();
        public static HCPFeature Getstances(int type, string culture = "en")
        {
            lock (locker)
            {
                if (!instances.ContainsKey(type + culture))
                {
                    HCPFeature hcp = new HCPFeature(type, culture);
                    foreach (var feature in hcp.FeatureObjs)
                    {
                        feature.AdvancedOptions.AddRange(new List<string>()
                    {
                        "RetryInterval=30000",//30s
                        "RetryCount=6",
                        "FlushDNS=true",
                        "FlushDNS=false",
                        "FailoverMode=Off",
                        "FailoverMode=ReadWrite",
                        "FailoverMode=Read"
                    });
                    }
                    instances[type + culture] = hcp;
                    return hcp;
                }
                else
                {
                    return instances[type + culture];
                }
            }
        }
    }
}
