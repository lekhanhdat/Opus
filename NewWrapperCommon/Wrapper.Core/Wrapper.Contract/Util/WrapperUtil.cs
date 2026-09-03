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

using AvePoint.GCommon.Contract.Server.ControlPanel.ColumnMapping.Object;
using AvePoint.GCommon.Contract.Server.ControlPanel.LanguageMapping.Object;
using AvePoint.GCommon.Contract.Server.ControlPanel.TemplateMapping.Object;
using AvePoint.GCommon.Contract.Server.ControlPanel.UserAndDomainMapping.DomainMapping;
using AvePoint.GCommon.Contract.Server.ControlPanel.UserAndDomainMapping.UserMapping;
using AvePoint.Wrapper.Core.SPRestore;
using AvePoint.Wrapper.Core.SPRestore.Mapping;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;
using AvePoint.GCommon.Contract.Server.ControlPanel.UserAndDomainMapping.Object;

namespace AvePoint.Wrapper.Core
{
    /// <summary>
    /// Wrapper Util
    /// </summary>
    public static class WrapperUtil
    {
        static readonly Dictionary<string, uint> builtInLanguageLCIDMapping = new Dictionary<string, uint>(StringComparer.OrdinalIgnoreCase) 
        { 
            { "English", 1033 }, 
            { "Japanese", 1041 }, 
            { "German", 1031 },
            { "Arabic", 1025 },
            { "ChineseSimplified", 2052 },
            { "ChineseTraditional", 1028 },
            { "Czech", 1029 },
            { "Danish", 1030 },
            { "Dutch", 1043 },
            { "Finnish", 1035 },
            { "French", 1036 },
            { "Greek", 1032 },
            { "Hebrew", 1037 }
        };

        /// <summary>
        /// Create Default User Mapping according to Contract
        /// </summary>
        /// <param name="userMapping"></param>
        /// <param name="domainMapping"></param>
        /// <returns></returns>
        public static IUserMapping CreateDefaultUserMapping(UserMappingDataContract userMapping, DomainMappingDataContract domainMapping)
        {
            return CreateDefaultUserMapping(userMapping, domainMapping, false);
        }

        /// <summary>
        /// Create Default User Mapping according to Contract
        /// </summary>
        /// <param name="userAndDomainMapping"></param>
        /// <returns></returns>
        public static IUserMapping CreateDefaultUserMapping(UserAndDomainMapping userAndDomainMapping)
        {
            return CreateDefaultUserMapping(userAndDomainMapping, false);
        }

        /// <summary>
        /// Create Default User Mapping according to Contract
        /// </summary>
        /// <param name="userAndDomainMapping"></param>
        /// <param name="reverseMapping"></param>
        /// <returns></returns>
        public static IUserMapping CreateDefaultUserMapping(UserAndDomainMapping userAndDomainMapping, bool reverseMapping)
        {
            BuiltInUserMapping mapping = null;
            if (userAndDomainMapping != null)
            {
                mapping = new BuiltInUserMapping();
                if (userAndDomainMapping.UserMappings != null)
                {
                    if (!reverseMapping)
                    {
                        foreach (var item in userAndDomainMapping.UserMappings.UserMapping)
                        {
                            mapping.AddUserMapping(item.sourceUser, item.destinationUser);
                        }
                        mapping.PlaceHolderAccount = userAndDomainMapping.sourcePlaceHolderAccount;
                        mapping.DefaultUserAccount = userAndDomainMapping.sourceDefaultUser;
                    }
                    else
                    {
                        foreach (var item in userAndDomainMapping.UserMappings.UserMapping)
                        {
                            mapping.AddUserMapping(item.destinationUser, item.sourceUser);
                        }
                        mapping.PlaceHolderAccount = userAndDomainMapping.placeHolderAccount;
                        mapping.DefaultUserAccount = userAndDomainMapping.destDefaultUser;
                    }
                }
                if (userAndDomainMapping.DomainMappings != null)
                {
                    if (!reverseMapping)
                    {
                        foreach (var item in userAndDomainMapping.DomainMappings.DomainMapping)
                        {
                            mapping.AddDomainMapping(item.sourceDomain, item.destinationDomain);
                        }
                    }
                    else
                    {
                        foreach (var item in userAndDomainMapping.DomainMappings.DomainMapping)
                        {
                            mapping.AddDomainMapping(item.destinationDomain, item.sourceDomain);
                        }
                    }
                }
            }
            return mapping;
        }

        /// <summary>
        /// Create default user mapping according to contract
        /// </summary>
        /// <param name="userMapping"></param>
        /// <param name="domainMapping"></param>
        /// <param name="reverseMapping"></param>
        /// <returns></returns>
        public static IUserMapping CreateDefaultUserMapping(UserMappingDataContract userMapping, DomainMappingDataContract domainMapping, bool reverseMapping)
        {
            BuiltInUserMapping mapping = null;

            if (userMapping != null || domainMapping != null)
            {
                mapping = new BuiltInUserMapping();

                if (userMapping != null)
                {
                    if (!reverseMapping)
                    {
                        foreach (var item in userMapping.mappings)
                        {
                            mapping.AddUserMapping(item.sourceUserName, item.targetUserName);
                        }

                        mapping.PlaceHolderAccount = userMapping.sourcePlaceHolder;
                        mapping.DefaultUserAccount = userMapping.sourceDefaultUser;
                    }
                    else
                    {
                        foreach (var item in userMapping.mappings)
                        {
                            mapping.AddUserMapping(item.targetUserName, item.sourceUserName);
                        }
                        mapping.PlaceHolderAccount = userMapping.targetPlaceHolder;
                        mapping.DefaultUserAccount = userMapping.targetdefaultUser;
                    }
                }

                if (domainMapping != null)
                {
                    if (!reverseMapping)
                    {
                        foreach (var item in domainMapping.domainMappings)
                        {
                            mapping.AddDomainMapping(item.sourceDomainName, item.targetDomainName);
                        }
                    }
                    else
                    {
                        foreach (var item in domainMapping.domainMappings)
                        {
                            mapping.AddDomainMapping(item.targetDomainName, item.sourceDomainName);
                        }
                    }
                }
            }

            return mapping;
        }

        /// <summary>
        /// Create language mapping controller
        /// </summary>
        /// <returns></returns>
        public static ILanguageMappingController CreateDefaultLanguageMappingController(Common.WrapperSPMode spMode)
        {
            return new BuiltInLanguageMappingController(spMode);
        }

        /// <summary>
        /// create language mapping
        /// </summary>
        /// <param name="languageMappingDto">CP对应的DTO</param>
        /// <param name="reverseMapping">是否反转mapping</param>
        /// <returns></returns>
        public static ILanguageMapping CreateDefaultLanguageMapping(LanguageMappingDto languageMappingDto, bool reverseMapping)
        {
            BuiltInLanguageMapping mapping = null;

            if (languageMappingDto != null)
            {
                mapping = new BuiltInLanguageMapping();

                if (!reverseMapping)
                {
                    mapping.SourceLCID = ConvertNameToLCID(languageMappingDto.sourceLanguage);
                    mapping.DestLCID = ConvertNameToLCID(languageMappingDto.targetLangugae);
                }
                else
                {
                    mapping.SourceLCID = ConvertNameToLCID(languageMappingDto.targetLangugae);
                    mapping.DestLCID = ConvertNameToLCID(languageMappingDto.sourceLanguage);
                }

                var languages = languageMappingDto.languages;

                for (int i = 0; i < languages.Count - 1 ; i += 2 )
                {
                    var item1 = languages[i];
                    var item2 = languages[i + 1];

                    if (item1.sourceName.Equals(item2.sourceName, StringComparison.OrdinalIgnoreCase))
                    {
                        uint itemLCID1 = uint.Parse(item1.destLanguageId);
                        uint itemLCID2 = uint.Parse(item2.destLanguageId);

                        if (itemLCID1 == mapping.SourceLCID && itemLCID2 == mapping.DestLCID)
                        {
                            if (item1.spType.Equals("List", StringComparison.OrdinalIgnoreCase))
                            {
                                mapping.AddListTitleMapping(item1.destName, item2.destName);
                            }
                            else if (item1.spType.Equals("Column", StringComparison.OrdinalIgnoreCase))
                            {
                                mapping.AddColumnDisplayNameMapping(item1.destName, item2.destName);
                            }
                        }
                        else if (itemLCID2 == mapping.SourceLCID && itemLCID1 == mapping.DestLCID)
                        {
                            if (item1.spType.Equals("List", StringComparison.OrdinalIgnoreCase))
                            {
                                mapping.AddListTitleMapping(item2.destName, item1.destName);
                            }
                            else if (item1.spType.Equals("Column", StringComparison.OrdinalIgnoreCase))
                            {
                                mapping.AddColumnDisplayNameMapping(item2.destName, item1.destName);
                            }
                        }
                    }
                }
            }

            return mapping;
        }

        /// <summary>
        /// Intern Use
        /// </summary>
        /// <param name="sourceLCID"></param>
        /// <param name="destLCID"></param>
        /// <returns></returns>
        internal static ILanguageMapping CreateEmptyLanguageMapping(uint sourceLCID, uint destLCID)
        {
            return new XmlLanguageMapping(sourceLCID, destLCID);
        }

        /// <summary>
        /// 根据CP端反馈的language name来转换成LCID
        /// </summary>
        /// <param name="languageName"></param>
        /// <returns></returns>
        private static uint ConvertNameToLCID(string languageName)
        {
            uint lcid = 0;

            if (!builtInLanguageLCIDMapping.TryGetValue(languageName, out lcid))
            {
                throw new ArgumentOutOfRangeException("languageName", languageName, null);
            }

            return lcid;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="xe"></param>
        /// <returns></returns>
        public static ITemplateMapping CreateDefaultTemplateMapping(TemplateMappingContract templateMappingDto, bool reverseMapping)
        {
            BuiltInTemplateMapping mapping = null;

            if (templateMappingDto != null)
            {
                mapping = new BuiltInTemplateMapping();

                if (!reverseMapping)
                {
                    if (templateMappingDto.SiteTemplateMappings != null)
                    {
                        foreach (var item in templateMappingDto.SiteTemplateMappings)
                        {
                            mapping.AddSiteTemplate(item.SourceDomain, item.TargetDomain);
                        }
                    }

                    if (templateMappingDto.ListTemplateMappings != null)
                    {
                        foreach (var item in templateMappingDto.ListTemplateMappings)
                        {
                            mapping.AddListTemplate(item.SourceDomain, item.TargetDomain);
                        }
                    }
                }
                else
                {
                    if (templateMappingDto.SiteTemplateMappings != null)
                    {
                        foreach (var item in templateMappingDto.SiteTemplateMappings)
                        {
                            mapping.AddSiteTemplate(item.TargetDomain, item.SourceDomain);
                        }
                    }

                    if (templateMappingDto.ListTemplateMappings != null)
                    {
                        foreach (var item in templateMappingDto.ListTemplateMappings)
                        {
                            mapping.AddListTemplate(item.TargetDomain, item.SourceDomain);
                        }
                    }
                }
            }

            return mapping;
        }

        /// <summary>
        /// Create default object Manager
        /// </summary>
        /// <returns></returns>
        internal static Internal.Restore.IImportObjectManager CreateDefaultObjectManager()
        {
            return new AvePoint.Wrapper.Core.Internal.Restore.ImportObjectManager();
        }

        /// <summary>
        /// 通过xml string来构造FieldMapping，不依赖于common contract
        /// </summary>
        /// <param name="xml"></param>
        /// <returns></returns>
        public static IFieldMapping CreateDefaultFieldMapping(string xml)
        {
            var doc = new XmlDocument();
            doc.LoadXml(xml);
            return new BuiltinFieldMapping(doc);
        }

        /// <summary>
        /// 构造Excel field mapping
        /// </summary>
        /// <param name="path"></param>
        /// <returns></returns>
        public static IFieldMapping CreateExcelFieldMapping(string path)
        {
            return new ExcelFieldMapping(path);
        }

        /// <summary>
        /// 构造dynamic field mapping
        /// </summary>
        /// <param name="path"></param>
        /// <returns></returns>
        public static IFieldMapping CreateDynamicFieldMapping(byte[] assembly)
        {
            return new DynamicFieldMapping(assembly);
        }

        /// <summary>
        /// 构造dynamic field mapping
        /// </summary>
        /// <param name="path"></param>
        /// <returns></returns>
        public static IFieldMapping CreateDynamicFieldMapping(byte[] assembly, string fullTypeName)
        {
            return new DynamicFieldMapping(assembly, fullTypeName);
        }



        /// <summary>
        /// 通过ColumnMappingDataContract构造FieldMapping,依赖于common contract, 以后可以考虑提出一层adapter     
        /// </summary>
        /// <param name="contract"></param>
        /// <returns></returns>
        public static IFieldMapping CreateDefaultFieldMapping(ColumnMappingDataContract contract)
        {
            return new BuiltinFieldMapping(BuiltinFieldMappingParameterConverter.Convert(contract));
        }

    }
}
