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


#region reference
using AvePoint.GCommon.Contract.CodeReview;
using AvePoint.Media.Storage.Resources.EgnyteI18N;
using System;
using System.Collections.Generic;
using System.Globalization;
#endregion
namespace AvePoint.Media.Storage.Egnyte
{
    #region CodeReview
    [AveCodeReview(
        "2013/10/16",
        "xiao.zhang@avepoint.com",
        "xiao.zhang@avepoint.com",
        new String[] { CodeReviewConstants.CHECK_LIST_ID_EH_2, CodeReviewConstants.CHECK_LIST_ID_BL_1, CodeReviewConstants.CHECK_LIST_ID_CS_2 },
        "ADO-93945",
        true,
        new String[] { CodeReviewConstants.CHECK_LIST_ID_EH_2, CodeReviewConstants.CHECK_LIST_ID_BL_1, CodeReviewConstants.CHECK_LIST_ID_CS_2 }
        )]
    #endregion

    class EgnyteFeature : StorageFeature
    {
        EgnyteFeature(Int32 type, String culture)
        {
            this.Init(type, culture);
        }

        protected override void GenerateSingleTypeFeatureUnit(CultureInfo culture)
        {
            StorageFeature storageFeature = new StorageFeature();
            StorageType type = new StorageType();
            type.Value = "Egnyte";
            type.Display = "Egnyte";
            type.Index = 409;
            type.IsSupportMovableRetention = false;
            type.Vim = new List<String>() { "egnyte_vim" };
            storageFeature.Type = type;
            storageFeature.Type.DefaultXris.Add("DOCAVE-XAM://EGNYTE_VIM?".ToLower(CultureInfo.InvariantCulture));
            storageFeature.Type.Vim.Add("egnyte_vim");
            storageFeature.Features = new List<FeatureUnit> {
                new FeatureUnit() {Vim = "egnyte_vim", DisplayName = EgnyteI18N.ResourceManager.GetString("MediaStorage_Egnyte_RootFolderName", culture), Tag = "Egnyte_Root_Folder_Name".ToLower(CultureInfo.InvariantCulture), Key = "Root_Folder_Name".ToLower(CultureInfo.InvariantCulture), KeyName = "RootFolderName", Visibility="Visible", ValType = "string", GuiType = "TextBox"},
                new FeatureUnit() {Vim = "egnyte_vim", DisplayName = EgnyteI18N.ResourceManager.GetString("MediaStorage_Egnyte_Domain", culture), Tag = "domain", Key = "Domain".ToLower(CultureInfo.InvariantCulture), KeyName = "Domain", Visibility="Visible", ValType = "string", GuiType = "TextBox"},
                new FeatureUnit() {Vim = "egnyte_vim", DisplayName = EgnyteI18N.ResourceManager.GetString("MediaStorage_Egnyte_AccessToken", culture), Tag = "Egnyte_Access_Token".ToLower(CultureInfo.InvariantCulture), Key = "egnyteAccessToken".ToLower(CultureInfo.InvariantCulture), KeyName = "EgnyteAccessToken", Visibility="Visible", ValType = "string", IsRequiredOption = true, GuiType = "TextBox"}
            };
            Add(storageFeature);
        }

        protected override void GenerateConnectorGUIFeatureUnit(CultureInfo culture)
        {
            //StorageFeature storageFeature = new StorageFeature();
            //StorageType type = new StorageType();
            //type.Value = "Egnyte";
            //type.Display = "Egnyte";
            //type.Index = 409;
            //type.Vim = new List<String>() { "egnyte_vim" };
            //type.IsSupportMovableRetention = false;
            //storageFeature.Type = type;
            //storageFeature.Type.DefaultXris.Add("DOCAVE-XAM://EGNYTE_VIM?".ToLower(CultureInfo.InvariantCulture));
            //storageFeature.IsNeedSpaceThreshold = true;
            //storageFeature.Type.Vim.Add("egnyte_vim");
            //storageFeature.Features = new List<FeatureUnit>
            //{
            //    new FeatureUnit() {IsRequiredOption = true, Vim = "egnyte_vim", DisplayName = EgnyteI18N.ResourceManager.GetString("MediaStorage_Connector_Egnyte_RootFolderName", culture), Tag = "Egnyte_Root_Folder_Name".ToLower(CultureInfo.InvariantCulture), Key = "Root_Folder_Name".ToLower(CultureInfo.InvariantCulture), KeyName = "EgnyteRootFolderName", Visibility="Visible", ValType = "string", GuiType = "TextBox", CanModifi = false,
            //    ValidateRegPats = new List<string>(){@"^.{0,200}$" + "\t0\t" + EgnyteI18N.ResourceManager.GetString("MediaStorage_Connector_Egnyte_root_folder_cannot_exceed_200_characters", culture)}},
            //    new FeatureUnit() {IsRequiredOption = true, Vim = "egnyte_vim", DisplayName = EgnyteI18N.ResourceManager.GetString("MediaStorage_Connector_Egnyte_Domain", culture), Tag = "domain", Key = "Domain".ToLower(CultureInfo.InvariantCulture), KeyName = "Domain", Visibility="Visible", ValType = "string", GuiType = "TextBox"},
            //    new FeatureUnit() {Vim = "egnyte_vim", DisplayName = EgnyteI18N.ResourceManager.GetString("MediaStorage_Connector_Egnyte_AccessToken", culture), Tag = "Egnyte_Access_Token".ToLower(CultureInfo.InvariantCulture), Key = "egnyteAccessTokenSecret".ToLower(CultureInfo.InvariantCulture), KeyName = "EgnyteAccessToken", Visibility="Visible", ValType = "string", IsRequiredOption = true, GuiType = "PasswordBox"},
            //    new FeatureUnit() {IsRequiredOption = true, Vim = "egnyte_vim", DisplayName =EgnyteI18N.ResourceManager.GetString("MediaStorage_Connector_Egnyte_Advanced",culture), Tag = "egnyte_vim_advanced", Key = "advanced".ToLower(CultureInfo.InvariantCulture), Value = "egnyte_vim_ExtendedParameters",  KeyName = "EgnyteAdvanced", Visibility="Visible", ValType = "string", GuiType = "CheckBox", ChildFeatures = new List<FeatureUnit>(){
            //       new FeatureUnit() {IsRequiredOption = true, Vim = "egnyte_vim", DisplayName = EgnyteI18N.ResourceManager.GetString("MediaStorage_Connector_Egnyte_Extended_parameters",culture), Tag = "egnyte_vim_ExtendedParameters", Key = "ExtendedParameters".ToLower(CultureInfo.InvariantCulture), KeyName = "EgnyteExtendedParameters", Visibility="Collapsed", ValType = "string", GuiType = "TextArea", CanNullOrEmpty = "true"}
            //    }}
            // };
            //Add(storageFeature);
        }

        /// <summary>
        /// 供VIM调用, 获取相应的Feature
        /// </summary>
        static readonly Dictionary<String, EgnyteFeature> instances = new Dictionary<String, EgnyteFeature>();
        private static Object locker = new Object();
        public static EgnyteFeature Getstances(Int32 type, String culture = "en")
        {
            lock (locker)
            {
                if (!instances.ContainsKey(type + culture))
                {
                    EgnyteFeature egnyte = new EgnyteFeature(type, culture);
                    foreach (var feature in egnyte.FeatureObjs)
                    {
                        feature.AdvancedOptions.AddRange(new List<String>()
                    {
                        "UseShared=true", 
                        "RetryInterval=300",
                        "RetryCount=6"
                    });
                    }
                    instances[type + culture] = egnyte;
                    return egnyte;
                }
                else
                {
                    return instances[type + culture];
                }
            }
        }
    }
}