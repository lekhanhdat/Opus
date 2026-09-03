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

namespace AvePoint.Wrapper.Core.SPRestore
{
    /// <summary>
    /// Language mapping controller
    /// </summary>
    public interface ILanguageMappingController
    {
        /// <summary>
        /// default language mapping is enabled,
        /// 
        /// if yes, need to load mapping from xml or resource file when there is no custom language mapping.
        /// 
        /// 如果存在源端和目的端的language mapping，则直接使用，如果不存在，再判断这个选项是否开启，
        /// 如果开启了，则去从资源文件或者xml中load
        /// 
        /// </summary>
        bool IsDefaultLanguageMappingEnabled { get; set; }

        /// <summary>
        /// 由于现在还存在逻辑可以export language file，所以需要
        /// </summary>
        string TemporaryDirectoryForSPResourceFile { get; set; }

        /// <summary>
        /// 根据Language Mapping的配置来获取mapping LCID
        /// 
        /// 主要使用于Site/Web创建时需要获取mapping的语言。
        /// </summary>
        /// <param name="originalLCID"></param>
        /// <returns></returns>
        uint GetMappingLCID(uint originalLCID);

        /// <summary>
        /// 根据两个语言获取language mapping的实例，主要存储在Web级别
        /// </summary>
        /// <param name="originalLCID"></param>
        /// <param name="currentLCID"></param>
        /// <returns></returns>
        ILanguageMapping GetLanguageMapping(uint originalLCID, uint currentLCID);

        /// <summary>
        /// 添加自定义的Language Mapping
        /// </summary>
        /// <param name="languageMapping"></param>
        void AddLanguageMapping(ILanguageMapping languageMapping);

        /// <summary>
        /// 还原Language File
        /// </summary>
        /// <param name="languageInfo"></param>
        void RestoreLanguageFile(Wrapper.Common.AveLanguageInfo languageInfo);

        /// <summary>
        /// Clean File
        /// </summary>
        void CleanLanguageFile();
    }

    /// <summary>
    /// Language Mapping
    /// </summary>
    public interface ILanguageMapping
    {
        /// <summary>
        /// source LCID
        /// </summary>
        uint SourceLCID { get; }

        /// <summary>
        /// destination LCID
        /// </summary>
        uint DestLCID { get; }

        /// <summary>
        /// is default loaded
        /// </summary>
        bool IsDefaultLoaded { get; }

        /// <summary>
        /// Get mapping list title
        /// </summary>
        /// <param name="listTitle"></param>
        /// <returns></returns>
        string GetMappingListTitle(string listTitle);

        /// <summary>
        /// Get mapping field displayName
        /// </summary>
        /// <param name="fieldDisplayName"></param>
        /// <returns></returns>
        string GetMappingFieldDisplayName(string fieldDisplayName);

        /// <summary>
        /// Get mapping content type name
        /// </summary>
        /// <param name="contentTypeName"></param>
        /// <returns></returns>
        string GetMappingContentTypeName(string contentTypeName);

        /// <summary>
        /// Get mapping navigation title
        /// </summary>
        /// <param name="navigationTitle"></param>
        /// <returns></returns>
        string GetMappingNavigationTitle(string navigationTitle);

        /// <summary>
        /// get mapping permission level name
        /// </summary>
        /// <param name="permissionLevelName"></param>
        /// <returns></returns>
        string GetMappingPermissionLevelName(string permissionLevelName);

        /// <summary>
        /// Get mapping group title
        /// </summary>
        /// <param name="title"></param>
        /// <returns></returns>
        string GetMappingGroupName(string title);

        /// <summary>
        /// export mapping
        /// </summary>
        /// <returns></returns>
        string ExportMapping();

        /// <summary>
        /// Load xml and resource from resource folder
        /// </summary>
        /// <param name="languageMappingXmlFile"></param>
        void LoadWrapperDefaultLanguageMapping(string languageMappingXmlFile);
    }
}
