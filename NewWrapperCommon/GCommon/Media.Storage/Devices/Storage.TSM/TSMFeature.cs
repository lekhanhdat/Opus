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

using System.Diagnostics.CodeAnalysis;
[module: SuppressMessage("FxCopCustomRules", "C100007:SpellCheckStringValues", Scope = "member", Target = "AvePoint.Media.Storage.TSM.TSMFeature.#GenerateDocAveGUIFeatureUnit(System.Globalization.CultureInfo)", MessageId = "Comm")]
namespace AvePoint.Media.Storage.TSM
{
    #region using directives
    using AvePoint.Media.Storage.Resources.TSMI18N;
    using System;
    using System.Collections.Generic;
    using System.Globalization;
    using System.Text;
    #endregion

    /// <summary>
    /// 此类用于描述TSM类型的存储介质的特性
    /// </summary>
    /// 
    class TSMFeature : StorageFeature
    {

        #region TSMFeature 自己负责初始化的部分, 在Application生存周期内只会初始化一次, 外部不需要知道的逻辑
        /// <summary>
        /// 私有构造函数， 此类不允许在外部实例化。 <see cref="TSMFeature"/> class.
        /// </summary>
        private TSMFeature(int type, string culture)
        {
            this.Init(type, culture);
        }

        protected override void GenerateSingleTypeFeatureUnit(CultureInfo culture)
        {
            GenerateDocAveGUIFeatureUnit(culture);
        }

        /// <summary>
        /// 实际初始化DocAve GUI Feature Unit的部分
        /// </summary>
        protected override void GenerateDocAveGUIFeatureUnit(CultureInfo culture)
        {
            StorageFeature sf = new StorageFeature();

            //定义StorageType
            StorageType tsm = new StorageType();
            tsm.Display = TSMI18N.ResourceManager.GetString("MediaStorage_TSM_TSM", culture);
            tsm.Index = 2;
            tsm.SoExtenderNotSupported = true;
            tsm.Value = "TSM";
            tsm.Vim = new List<string>() { "tsm_vim" };

            sf.Type = tsm;
            sf.Type.DefaultXris = new List<string>() { "DOCAVE-XAM://TSM_VIM?COMMMETHOD=TCPIP".ToLower(CultureInfo.InvariantCulture) };
            sf.IsNeedSpaceThreshold = false;
            sf.ProgressForeground = new FeatureColor(255, 255, 204, 68);

            string TCPIP_TAG = "TSM_COMMU_TCPIP".ToLower(CultureInfo.InvariantCulture);
            string TCPIPV6_TAG = "TSM_COMMU_V6TCPIP".ToLower(CultureInfo.InvariantCulture);
            sf.Features = new List<FeatureUnit>() { 
                new FeatureUnit() {Vim = "tsm_vim", DisplayName = TSMI18N.ResourceManager.GetString("MediaStorage_TSM_Communication",culture), Value = "COMMMethod".ToLower(CultureInfo.InvariantCulture), Tag = "ComBox_TSMCommMethod", Key = "COMMMethod".ToLower(CultureInfo.InvariantCulture), KeyName="TSMCommunication", Visibility="Visible", ValType="string", GuiType="ComboBox", ChildFeatures = new List<FeatureUnit>() { 
                    new FeatureUnit() {Vim = "tsm_vim", DisplayName = TSMI18N.ResourceManager.GetString("MediaStorage_TSM_TCP_IP",culture), Value = "TCPIP".ToLower(CultureInfo.InvariantCulture), Tag = TCPIP_TAG, Key = "TCPIP".ToLower(CultureInfo.InvariantCulture), GuiType="ComboBoxItem", Visibility="Visible", ValType="string", ChildFeatures = new List<FeatureUnit> {
                        new FeatureUnit() {Vim = "tsm_vim", DisplayName = TSMI18N.ResourceManager.GetString("MediaStorage_TSM_Server_Address",culture), Tag = TCPIP_TAG, Key = "address", KeyName = "TCPIPAddress", Visibility="Visible", ValType = "string", IsRequiredOption = true, GuiType = "TextBox",DemoValue=@"10.2.207.160"},
                        new FeatureUnit() {Vim = "tsm_vim", DisplayName = TSMI18N.ResourceManager.GetString("MediaStorage_TSM_Server_Port",culture), Tag = TCPIP_TAG, Key = "port", KeyName = "TCPIPPort", Visibility="Visible", ValType = "string", IsRequiredOption = true, GuiType = "TextBox",DemoValue=@"1500"},
                        new FeatureUnit() {Vim = "tsm_vim", DisplayName = TSMI18N.ResourceManager.GetString("MediaStorage_TSM_Node_Name",culture), Tag = TCPIP_TAG, Key = "node", KeyName = "TCPIPNode", Visibility="Visible", ValType = "string", IsRequiredOption = true, GuiType = "TextBox",DemoValue = TSMI18N.ResourceManager.GetString("MediaStorage_TSM_Node_Name_Demo", culture)},
                        new FeatureUnit() {Vim = "tsm_vim", DisplayName = TSMI18N.ResourceManager.GetString("MediaStorage_TSM_Management_Class",culture), Tag = TCPIP_TAG, Key = "managementClass".ToLower(CultureInfo.InvariantCulture), KeyName = "TCPIPManagementClass", Visibility="Visible", ValType = "string", GuiType = "TextBox", CanNullOrEmpty = "true"},
                        new FeatureUnit() {Vim = "tsm_vim", DisplayName = TSMI18N.ResourceManager.GetString("MediaStorage_TSM_Node_Password",culture), Tag = TCPIP_TAG, Key = "secret", KeyName = "TCPIPNodePassword", Visibility="Visible", ValType = "string", IsRequiredOption = true, GuiType = "PasswordBox"},
                    }},
                    new FeatureUnit() {Vim = "tsm_vim", DisplayName = TSMI18N.ResourceManager.GetString("MediaStorage_TSM_TCP_IP_V6",culture), Value = "V6TCPIP".ToLower(CultureInfo.InvariantCulture), Tag = TCPIPV6_TAG, Key = "V6TCPIP".ToLower(CultureInfo.InvariantCulture), Visibility="Collapsed", GuiType="ComboBoxItem", ValType="string", ChildFeatures = new List<FeatureUnit> {
                        new FeatureUnit() {Vim = "tsm_vim", DisplayName = TSMI18N.ResourceManager.GetString("MediaStorage_TSM_Server_Address",culture), Tag = TCPIPV6_TAG, Key = "address", KeyName = "V6TCPIPAddress", Visibility="Collapsed", ValType = "string", IsRequiredOption = true, GuiType = "TextBox",DemoValue=@"2404:f800:7003:9:4817:9c54:386b:8000"},
                        new FeatureUnit() {Vim = "tsm_vim", DisplayName = TSMI18N.ResourceManager.GetString("MediaStorage_TSM_Server_Port",culture), Tag = TCPIPV6_TAG, Key = "port", KeyName = "V6TCPIPPort", Visibility="Collapsed", ValType = "string", GuiType = "TextBox", IsRequiredOption = true, DemoValue=@"1500"},
                        new FeatureUnit() {Vim = "tsm_vim", DisplayName = TSMI18N.ResourceManager.GetString("MediaStorage_TSM_Node_Name",culture), Tag = TCPIPV6_TAG, Key = "node", KeyName = "V6TCPIPNode", Visibility="Collapsed", ValType = "string", GuiType = "TextBox", IsRequiredOption = true, DemoValue = TSMI18N.ResourceManager.GetString("MediaStorage_TSM_IP_V6_Node_Name_Demo", culture)},
                        new FeatureUnit() {Vim = "tsm_vim", DisplayName = TSMI18N.ResourceManager.GetString("MediaStorage_TSM_Management_Class",culture), Tag = TCPIPV6_TAG, Key = "managementClass".ToLower(CultureInfo.InvariantCulture), KeyName = "V6TCPIPManagementClass", Visibility="Collapsed", ValType = "string", GuiType = "TextBox", CanNullOrEmpty = "true"},
                        new FeatureUnit() {Vim = "tsm_vim", DisplayName = TSMI18N.ResourceManager.GetString("MediaStorage_TSM_Node_Password",culture), Tag = TCPIPV6_TAG, Key = "secret", KeyName = "V6TCPIPNodePassword", Visibility="Collapsed", ValType = "string", IsRequiredOption = true, GuiType = "PasswordBox"},
                    }},

                    //new FeatureUnit() {Vim = "tsm_vim", DisplayName = "Named Pipe", Value = "namedpipe", Tag = "TSM_COMMU_NAMEDPIPE".ToLower(CultureInfo.InvariantCulture), Key = "namedpipe", GuiType="ComboBoxItem", Visibility="Collapsed", ValType="string", ChildFeatures = new List<FeatureUnit> {
                    //    new FeatureUnit() {Vim = "tsm_vim", DisplayName = "Named Pipe Name", Tag = "TSM_COMMU_NAMEPIPE".ToLower(CultureInfo.InvariantCulture), Key = "pipeN", KeyName = "NamedPipeName", Visibility="Collapsed", ValType = "string", GuiType = "TextBox"},
                    //    new FeatureUnit() {Vim = "tsm_vim", DisplayName = "Node Name", Tag = "TSM_COMMU_NAMEPIPE".ToLower(CultureInfo.InvariantCulture), Key = "node", KeyName = "NamedPipeNode", Visibility="Collapsed", ValType = "string", GuiType = "TextBox"},
                    //    new FeatureUnit() {Vim = "tsm_vim", DisplayName = "Management Class", Tag = "TSM_COMMU_NAMEPIPE".ToLower(CultureInfo.InvariantCulture), Key = "MCLASS".ToLower(CultureInfo.InvariantCulture), KeyName = "NamedPipeManagementClass", Visibility="Collapsed", ValType = "string", GuiType = "TextBox" , CanNullOrEmpty = "true"},
                    //    new FeatureUnit() {Vim = "tsm_vim", DisplayName = "Node Password", Tag = "TSM_COMMU_NAMEPIPE".ToLower(CultureInfo.InvariantCulture), Key = "secret", KeyName = "NamedPipeodePassword", Visibility="Collapsed", ValType = "string", GuiType = "PasswordBox"}
                    //}},
                    //new FeatureUnit() {Vim = "tsm_vim", DisplayName = "Shared Memory", Value = "sharedmemory", Tag = "tsm_commu_sharedmemory", Key = "sharedmemory", Visibility="Collapsed", GuiType="ComboBoxItem", ValType="string", ChildFeatures = new List<FeatureUnit> {
                    //    new FeatureUnit() {Vim = "tsm_vim", DisplayName = "Port", Tag = "tsm_commu_sharedmemory", Key = "port", KeyName = "SharedMemoryPort", Visibility="Collapsed", ValType = "string", GuiType = "TextBox"},
                    //    new FeatureUnit() {Vim = "tsm_vim", DisplayName = "Node Name", Tag = "tsm_commu_sharedmemory", Key = "node", KeyName = "SharedMemoryNode", Visibility="Collapsed", ValType = "string", GuiType = "TextBox"},
                    //    new FeatureUnit() {Vim = "tsm_vim", DisplayName = "Management Class", Tag = "tsm_commu_sharedmemory", Key = "MCLASS".ToLower(CultureInfo.InvariantCulture), KeyName = "SharedMemoryManagementClass", Visibility="Collapsed", ValType = "string", GuiType = "TextBox", CanNullOrEmpty = "true"},
                    //    new FeatureUnit() {Vim = "tsm_vim", DisplayName = "Node Password", Tag = "tsm_commu_sharedmemory", Key = "secret", KeyName = "SharedMemoryNodePassword", Visibility="Collapsed", ValType = "string", GuiType = "PasswordBox"}
                    //}}
                }
                },

                //Add Client Node Proxy
                new FeatureUnit() {Vim = "tsm_vim", DisplayName = TSMI18N.ResourceManager.GetString("MediaStorage_TSM_Client_Node_Proxy",culture), Tag = "tsm_vim_EnableNodeProxy", Key = "enableNodeProxy".ToLower(CultureInfo.InvariantCulture), Value = "tsm_vim_Asnodename".ToLower(CultureInfo.InvariantCulture), KeyName = "TSMEnableNodeProxy",Visibility="Visible", ValType = "string", GuiType = "CheckBox",  ChildFeatures = new List<FeatureUnit>()
                {
                   new FeatureUnit() {Vim = "tsm_vim", DisplayName = TSMI18N.ResourceManager.GetString("MediaStorage_TSM_Asnodename",culture),Tag = "tsm_vim_Asnodename", Key = "Asnodename".ToLower(CultureInfo.InvariantCulture), KeyName = "TSMAsnodename",  Visibility="Collapsed", ValType = "string", GuiType = "TextBox", IsRequiredOption = true}
                }},
                //Add Advanced Option
                new FeatureUnit() {Vim = "tsm_vim", DisplayName = TSMI18N.ResourceManager.GetString("MediaStorage_TSM_Advanced",culture), Tag = "tsm_vim_advanced", Key = "advanced", Value = "tsm_vim_ExtendedParameters".ToLower(CultureInfo.InvariantCulture), KeyName = "TSMAdvanced",Visibility="Visible", ValType = "string", GuiType = "CheckBox",  ChildFeatures = new List<FeatureUnit>()
                {
                   new FeatureUnit() {Vim = "tsm_vim", DisplayName = TSMI18N.ResourceManager.GetString("MediaStorage_TSM_Extended_Parameters",culture),Tag = "tsm_vim_ExtendedParameters", Key = "ExtendedParameters".ToLower(CultureInfo.InvariantCulture), KeyName = "TSMExtendedParameters",  Visibility="Collapsed", ValType = "string", GuiType = "TextArea", CanNullOrEmpty = "true"}
                }}
            };

            Add(sf);
        }
        #endregion

        /// <summary>
        /// 供VIM调用, 获取相应的Feature
        /// </summary>
        private static readonly Dictionary<string, TSMFeature> instances = new Dictionary<string, TSMFeature>();
        private static Object locker = new Object();
        public static TSMFeature Getstances(int type, string culture = "en")
        {
            lock (locker)
            {
                if (!instances.ContainsKey(type + culture))
                {
                    TSMFeature tsm = new TSMFeature(type, culture);
                    foreach (var feature in tsm.FeatureObjs)
                    {
                        feature.AdvancedOptions.AddRange(new List<string>()
                    {
                     "FileSpace=DocAve",
                     "SingleSession=true",
                     "SingleSession=false"
                    });
                    }
                    instances[type + culture] = tsm;
                    return tsm;
                }
                else
                {
                    return instances[type + culture];
                }
            }
        }

    }
}
