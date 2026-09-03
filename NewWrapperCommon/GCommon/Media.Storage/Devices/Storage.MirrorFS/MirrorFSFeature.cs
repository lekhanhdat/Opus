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

namespace AvePoint.Media.Storage.MirrorFS
{
    #region using directives
    using System;
    using System.Collections.Generic;
    using System.Globalization;
    using System.Text; 
    #endregion

    sealed class MirrorFSFeature : StorageFeature
    {

        #region MirrorFSFeature 自己负责初始化的部分, 在Application生存周期内只会初始化一次, 外部不需要知道的逻辑
        /// <summary>
        /// 私有构造函数， 此类不允许在外部实例化。 <see cref="FSFeature"/> class.
        /// </summary>
        private MirrorFSFeature(int type,string culture)
        {
            this.Init(type,culture);
        }

        protected override void GenerateDocAveGUIFeatureUnit(CultureInfo culture)
        {
            StorageFeature sf = new StorageFeature();
            StorageType fs = new StorageType();
            fs.Value = "MirrorFS";
            fs.Display = "RAID";
            fs.Index = 6;

            fs.Vim = new List<string>() { "mirrorFS_vim".ToLower(CultureInfo.InvariantCulture) };
            sf.Type = fs;

            sf.IsNeedSpaceThreshold = true;
            sf.ProgressForeground = new FeatureColor(255, 255, 0, 255);


            sf.Features = new List<FeatureUnit>() {
                new FeatureUnit() {Vim = "mirrorFS_vim".ToLower(CultureInfo.InvariantCulture), DisplayName = "Primary UNC Path", Tag = "firstFS_path", Key = "PRIMARYLOCATION".ToLower(CultureInfo.InvariantCulture), KeyName = "UNCPath1", Visibility = "Visible", ValType="string", GuiType="TextBox", DemoValue=@"\\10.1.1.10\c$\DocAve_Data"},
                new FeatureUnit() {Vim = "mirrorFS_vim".ToLower(CultureInfo.InvariantCulture), DisplayName = "Username", Tag = "firstFS_username", Key = "PRIMARYNAME".ToLower(CultureInfo.InvariantCulture), KeyName = "UNCUsername1", Visibility = "Visible", ValType="string", GuiType="TextBox", DemoValue=@"storage\administrator"},
                new FeatureUnit() {Vim = "mirrorFS_vim".ToLower(CultureInfo.InvariantCulture), DisplayName = "Password", Tag = "firstFS_password", Key = "PRIMARYSECRET".ToLower(CultureInfo.InvariantCulture), KeyName = "UNCPassword1", Visibility = "Visible", ValType="string", GuiType="PasswordBox"},

                new FeatureUnit() {Vim = "mirrorFS_vim".ToLower(CultureInfo.InvariantCulture), DisplayName = "Mirror UNC Path", Tag = "secondFS_path", Key = "MIRRORLOCATION".ToLower(CultureInfo.InvariantCulture), KeyName = "UNCPath2", Visibility = "Visible", ValType="string", GuiType="TextBox", DemoValue=@"\\10.1.1.10\c$\DocAve_Data"},
                new FeatureUnit() {Vim = "mirrorFS_vim".ToLower(CultureInfo.InvariantCulture), DisplayName = "Username", Tag = "secondFS_username", Key = "MIRRORNAME".ToLower(CultureInfo.InvariantCulture), KeyName = "UNCUsername2", Visibility = "Visible", ValType="string", GuiType="TextBox", DemoValue=@"storage\administrator"},
                new FeatureUnit() {Vim = "mirrorFS_vim".ToLower(CultureInfo.InvariantCulture), DisplayName = "Password", Tag = "secondFS_password", Key = "MIRRORSECRET".ToLower(CultureInfo.InvariantCulture), KeyName = "UNCPassword2", Visibility = "Visible", ValType="string", GuiType="PasswordBox"}
            };

            Add(sf);
        }
        #endregion

        /// <summary>
        /// 供VIM调用, 获取相应的Feature
        /// </summary>
        //private static readonly Dictionary<int, MirrorFSFeature> instances = new Dictionary<int, MirrorFSFeature>() { 
        //    { (int)FeatureType.DocAveGUI, new MirrorFSFeature((int)FeatureType.DocAveGUI) }, 
        //    { (int)FeatureType.ConnectorGUI, new MirrorFSFeature((int)FeatureType.ConnectorGUI) },
        //    { (int)FeatureType.SingleType, new MirrorFSFeature((int)FeatureType.SingleType) },
        //};
        //public static Dictionary<int, MirrorFSFeature> Instances
        //{
        //    get { return instances; }
        //}

        private static readonly Dictionary<string, MirrorFSFeature> instances = new Dictionary<string, MirrorFSFeature>();
        private static Object locker = new Object();
        public static MirrorFSFeature Getstances(int type, string culture = "en")
        {
            lock (locker)
            {
                if (!instances.ContainsKey(type + culture))
                {
                    MirrorFSFeature mirrorFS = new MirrorFSFeature(type, culture);
                    instances[type + culture] = mirrorFS;
                    return mirrorFS;
                }
                else
                {
                    return instances[type + culture];
                }
            }
        }
    }
}
