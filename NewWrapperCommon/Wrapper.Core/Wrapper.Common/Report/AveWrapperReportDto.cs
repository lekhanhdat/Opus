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


using System.Collections.Generic;
using System.Globalization;
using AvePoint.Wrapper.Resource.ServerAPI2010;
using System;
using AvePoint.Wrapper.Resource.Report;
using AvePoint.Wrapper.Resource.Exception;

namespace AvePoint.Wrapper.Common
{
    public class AveWrapperReportDto
    {
        public AveStatus Status { get; internal set; }
        public string ErrorMessage { get; internal set; }
        public string Name { get; internal set; }
        public string RelatedObjectTitle { get; internal set; }
        public string Type { get; internal set; }
        public string Key { get; internal set; }
        public List<object> Parameters { get; internal set; }
        public AveWrapperReportDto(string name, string objTitle, AveReportObjectType type, AveStatus status, string errorMessage)
        {
            this.Name = name;
            this.Type = type.GetDisplayName();
            this.Status = status;
            this.ErrorMessage = errorMessage;
            this.RelatedObjectTitle = objTitle;
        }

        public AveWrapperReportDto(string name, string objTitle, AveReportObjectType type, AveStatus status, AveReportResource key, params object[] args)
        {
            this.Name = name;
            this.Type = type.GetDisplayName();
            this.Status = status;
            if (key != AveReportResource.Wrapper_Report_None)
            {
                this.Key = key.ToString();
                this.Parameters = new List<object>(args);
                this.ErrorMessage = string.Format(WrapperReportResource.ResourceManager.GetString(key.ToString(), WrapperReportResource.Culture), args);
            }
            this.RelatedObjectTitle = objTitle;
        }

        /// <summary>
        /// used for the i18nKey in wrapper exception resource
        /// </summary>
        /// <param name="i18nKey"></param>
        /// <param name="name"></param>
        /// <param name="objTitle"></param>
        /// <param name="type"></param>
        /// <param name="status"></param>
        /// <param name="args">i18异常参数为List集合</param>
        public AveWrapperReportDto(string i18nKey, string name, string objTitle, AveReportObjectType type, AveStatus status, List<object> args)
        {
            this.Name = name;
            this.Type = type.GetDisplayName();
            this.Status = status;
            this.Key = i18nKey;
            this.Parameters = args;
            this.ErrorMessage = string.Format(WrapperExceptionResource.ResourceManager.GetString(i18nKey, WrapperReportResource.Culture), args.ToArray());
            this.RelatedObjectTitle = objTitle;
        }
    }
    public class AveWrapperWebpartReportDto : AveWrapperReportDto
    {
        #region Webpart Info
        private bool mIsCustomizeWebPart = true;
        public string AssemblyName { get; internal set; }
        public string TypeName { get; internal set; }
        public Guid WebpartId { get; internal set; }
        public Guid WebPartTypeId { get; internal set; }
        public bool IsCustomizeWebPart { get { return mIsCustomizeWebPart; } internal set { mIsCustomizeWebPart = value; } }
        #endregion
        #region Page Info
        public string DesPageUrl { get; internal set; }
        public int PageVersion { get; internal set; }
        public Guid PageId { get; internal set; }
        #endregion
        /// <summary>
        /// Use for Server Mode.
        /// </summary>
        /// <param name="name"></param>
        /// <param name="objTitle"></param>
        /// <param name="webpartBaseInfo"></param>
        /// <param name="assemblyName"></param>
        /// <param name="typeName"></param>
        /// <param name="status"></param>
        /// <param name="key"></param>
        /// <param name="args"></param>
        public AveWrapperWebpartReportDto(string name, string objTitle,
            AveWebPartBaseInfo webpartBaseInfo, string assemblyName, string typeName,AveStatus status, AveReportResource key, params object[] args)
            : base(name, objTitle, AveReportObjectType.WebPart, status, key, args)
        {
            if (webpartBaseInfo != null)
            {
                WebpartId = webpartBaseInfo.ID;
                WebPartTypeId = webpartBaseInfo.WebPartTypeId;
                PageVersion = webpartBaseInfo.PageVersion;
                if(string.IsNullOrEmpty(assemblyName))
                {
                    AveWebPartDefinitionXmlUtility.RetrieveWebPartAssemblyInfo(webpartBaseInfo.DefinitionXml, ref assemblyName, ref typeName);
                }
            }
            AssemblyName = assemblyName;
            TypeName = typeName;
        }
        /// <summary>
        /// Use For Client mode.
        /// </summary>
        /// <param name="name"></param>
        /// <param name="objTitle"></param>
        /// <param name="webpartBaseInfo"></param>
        /// <param name="assemblyName"></param>
        /// <param name="typeName"></param>
        /// <param name="status"></param>
        /// <param name="errorMessage"></param>
        public AveWrapperWebpartReportDto(string name, string objTitle,
            AveWebPartBaseInfo webpartBaseInfo, string assemblyName, string typeName, 
            AveStatus status, string errorMessage)
            : base(name, objTitle, AveReportObjectType.WebPart, status, errorMessage)
        {
            if (webpartBaseInfo != null)
            {
                WebpartId = webpartBaseInfo.ID;
                WebPartTypeId = webpartBaseInfo.WebPartTypeId;
                PageVersion = webpartBaseInfo.PageVersion;
                if (string.IsNullOrEmpty(assemblyName))
                {
                    AveWebPartDefinitionXmlUtility.RetrieveWebPartAssemblyInfo(webpartBaseInfo.DefinitionXml, ref assemblyName, ref typeName);
                }
            }
            AssemblyName = assemblyName;
            TypeName = typeName;
        }
    }
}
