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


using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using AvePoint.Media.ClassicStorage;
using AvePoint.GCommon.Contract.Common;
using System.Globalization;
using AvePoint.Media.ClassicStorage.Resources.BoxI18N;

namespace AvePoint.Media.ClassicStorage.Box
{
    public class BoxFeature: StorageFeature
    {
           private BoxFeature(int type,string culture)
        {
            this.Init(type,culture);
        }

        protected override void GenerateSingleTypeFeatureUnit(CultureInfo culture)
        {
            StorageFeature sf = new StorageFeature();
            StorageType type = new StorageType();
            type.Value = "Box";
            type.Display = "Box";
            type.Index = 408;
            type.IsSupportMovableRetention = false;
            type.Vim = new List<string>() { "box_vim" };
            sf.Type = type;
            sf.Type.DefaultXris.Add("DOCAVE-XAM://BOX_VIM?".ToLower(CultureInfo.InvariantCulture));
            sf.Type.Vim.Add("box_vim");

            sf.Features = new List<FeatureUnit> {
                new FeatureUnit() {Vim = "box_vim", DisplayName = BoxI18N.ResourceManager.GetString("MediaStorage_Box_RootFolderName", culture), Tag = "Box_Root_Folder_Name".ToLower(CultureInfo.InvariantCulture), Key = "Root_Folder_Name".ToLower(CultureInfo.InvariantCulture), KeyName = "BoxRootFolderName", Visibility="Visible", ValType = "string", GuiType = "TextBox"},
                new FeatureUnit() {Vim = "box_vim", DisplayName = BoxI18N.ResourceManager.GetString("MediaStorage_Box_BoxAPIKey", culture), Tag = "box", Key = "boxAPIKey".ToLower(CultureInfo.InvariantCulture), KeyName = "BoxAPIKey", Visibility="Visible", ValType = "string", GuiType = "TextBox"},
            };
            Add(sf);

         
        }

        protected override void GenerateDocAveGUIFeatureUnit(CultureInfo culture)
        {
            StorageFeature sf = new StorageFeature();
            StorageType type = new StorageType();
            type.Value = "Box";
            type.Display = BoxI18N.ResourceManager.GetString("MediaStorage_Box_Type", culture);
            type.Index = 9;
            type.Vim = new List<string>() {"box_vim"};
            type.IsSupportMovableRetention = false;
            sf.Type = type;
            sf.Type.DefaultXris.Add("DOCAVE-XAM://BOX_VIM?".ToLower(CultureInfo.InvariantCulture));
            sf.IsNeedSpaceThreshold = true;
            sf.Type.Vim.Add("box_vim");
            sf.Features = new List<FeatureUnit>
            {
                new FeatureUnit()
                {
                    Vim = "box_vim",
                    DisplayName = BoxI18N.ResourceManager.GetString("MediaStorage_Box_RootFolderName", culture),
                    Tag = "Box_Root_Folder_Name".ToLower(CultureInfo.InvariantCulture),
                    Key = "boxRootFolderName".ToLower(CultureInfo.InvariantCulture), 
                    KeyName = "BoxRootFolderName", 
                    Visibility="Visible", 
                    ValType = "string", 
                    IsRequiredOption = true, 
                    GuiType = "TextBox"
                },
                new FeatureUnit()
                {
                    Vim = "box_vim",
                    DisplayName = BoxI18N.ResourceManager.GetString("MediaStorage_Box_BoxClientID", culture),
                    Tag = "Box_Client_ID".ToLower(CultureInfo.InvariantCulture),
                    Key = "boxClientId".ToLower(CultureInfo.InvariantCulture),
                    KeyName = "BoxClientSecret",
                    Visibility = "Visible",
                    ValType = "string",
                    IsRequiredOption = true,
                    GuiType = "TextBox"
                },
                new FeatureUnit()
                {
                    Vim = "box_vim",
                    DisplayName = BoxI18N.ResourceManager.GetString("MediaStorage_Box_BoxClientSecret", culture),
                    //Tag = "Box_Client_Secret",
                    //Key = "boxClientSecret",
                    //KeyName = "BoxClientSecret",
                    //Visibility = "Visible",
                    //ValType = "string",
                    //IsRequiredOption = true,
                    Tag = "Box_Refresh_Secret".ToLower(CultureInfo.InvariantCulture), 
                    Key = "boxRefreshSecret".ToLower(CultureInfo.InvariantCulture), 
                    KeyName = "BoxRefreshSecret", Visibility="Visible", 
                    ValType = "string", 
                    IsRequiredOption = true, 
                    GuiType = "PasswordBox"
                },
                new FeatureUnit()
                {
                    Vim = "box_vim",
                    DisplayName = BoxI18N.ResourceManager.GetString("MediaStorage_Box_BoxRetrieveTokenLink", culture),
                    Tag = "Box_Retrieve_Token_Link".ToLower(CultureInfo.InvariantCulture),
                    Visibility = "Visible",
                    ValType = "string",
                    IsRequiredOption = true,
                    GuiType = "Hyperlink"
                },
                new FeatureUnit()
                {
                    Vim = "box_vim",
                    DisplayName = BoxI18N.ResourceManager.GetString("MediaStorage_Box_BoxRefreshToken", culture),
                    //Tag = "Box_Refresh_Token",
                    //Key = "BoxRefreshTokenSecret".ToLower(CultureInfo.InvariantCulture),
                    //KeyName = "BoxRefreshTokenSecret",
                    //Visibility = "Visible",
                    //ValType = "string",
                    //IsRequiredOption = true,
                    Tag = "Box_Refresh_Token_Secret".ToLower(CultureInfo.InvariantCulture), 
                    Key = "boxRefreshTokenSecret".ToLower(CultureInfo.InvariantCulture), 
                    KeyName = "BoxRefreshTokenSecret", 
                    Visibility="Visible", 
                    ValType = "string", 
                    IsRequiredOption = true, 
                    GuiType = "PasswordBox"
                }
            };
            Add(sf);
        }

        /// <summary>
        /// 供VIM调用, 获取相应的Feature
        /// </summary>
        private static readonly Dictionary<string, BoxFeature> instances = new Dictionary<string, BoxFeature>();

        public static BoxFeature Getstances(int type, string culture = "en")
        {
            if (!instances.ContainsKey(type + culture))
            {
                BoxFeature box = new BoxFeature(type, culture);

                foreach (var feature in box.FeatureObjs)
                {
                    feature.AdvancedOptions.AddRange(new List<string>()
                    {
                        "RetryInterval=30",
                        "RetryCount=6"
                    });
                }

                instances[type + culture] = box;
                return box;
            }
            else
            {
                return instances[type + culture];
            }
        }
    }
}
