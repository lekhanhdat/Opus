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
using System.Runtime.Serialization;
using AvePoint.Adonis.CentralAdmin.Object;
using AvePoint.GCommon.Contract.Common;

namespace AvePoint.GCommon.Contract.CentralAdmin.Object
{
    [DataContract(Namespace = ContractConstants.Namespace)]
    public class CAWebCreateSubSiteOperation:CAOperation
    {
        [DataMember]
        public List<CreateSubWebTemplate> Templates { get; set; }
        [DataMember]
        public List<SubWebTemplate> SubWebTemplates { get; set; }
        [DataMember]
        public bool NavigationInheritance { get; set; }
        [DataMember]
        public string Title { get; set; }
        [DataMember]
        public bool Display { get; set; }
        [DataMember]
        public bool UseTopLinkBar { get; set; }
        [DataMember]
        public string Finance { get; set; }
        [DataMember]
        public string SubWebSiteAddress { get; set; }
        [DataMember]
        public int PublishingOnline { get; set; }
        [DataMember]
        public string InfoTechnology { get; set; }
        [DataMember]
        public string ResearchAndDevelop { get; set; }
        [DataMember]
        public string Sales { get; set; }
        [DataMember]
        public string International { get; set; }
        [DataMember]
        public string Local { get; set; }
        [DataMember]
        public string National { get; set; }
        [DataMember]
        public string Templatename { get; set; }
        [DataMember]
        public bool UserPermissons { get; set; }
        [DataMember]
        public string Description { get; set; }
        [DataMember]
        public uint LanguageValue { get; set; }
        [DataMember]
        public uint SiteDefaultLanguage { get; set; }
        [DataMember]
        public Dictionary<uint, string> SiteLanguages { get; set; }
        [DataMember]
        public bool HasDirectorySite { get; set; }
        [DataMember]
        public uint TemplatesCode { get; set; }
        [DataMember]
        public string DisplayName { get; set; }
        [DataMember]
        public bool DisplayQiuckLaunch { get; set; }
        [DataMember]
        public string FullURL { get; set; }
        [DataMember]
        public CAPermSetupGetParam GetNewSubSitePermSetupParam { get; set; }
    }

    [DataContract(Namespace = ContractConstants.Namespace)]
    public class NewCAWebCreateSubSiteOperation : CAOperation
    {
        [DataMember]
        public List<CreateSubWebTemplate> Templates { get; set; }
        [DataMember]
        public List<SubWebTemplate> SubWebTemplates { get; set; }
        [DataMember]
        public bool NavigationInheritance { get; set; }
        [DataMember]
        public string Title { get; set; }
        [DataMember]
        public bool Display { get; set; }
        [DataMember]
        public bool UseTopLinkBar { get; set; }
        [DataMember]
        public string Finance { get; set; }
        [DataMember]
        public string SubWebSiteAddress { get; set; }
        [DataMember]
        public int PublishingOnline { get; set; }
        [DataMember]
        public string InfoTechnology { get; set; }
        [DataMember]
        public string ResearchAndDevelop { get; set; }
        [DataMember]
        public string Sales { get; set; }
        [DataMember]
        public string International { get; set; }
        [DataMember]
        public string Local { get; set; }
        [DataMember]
        public string National { get; set; }
        [DataMember]
        public string Templatename { get; set; }
        [DataMember]
        public bool UserPermissons { get; set; }
        [DataMember]
        public string Description { get; set; }
        [DataMember]
        public uint LanguageValue { get; set; }
        [DataMember]
        public uint SiteDefaultLanguage { get; set; }
        [DataMember]
        public Dictionary<uint, string> SiteLanguages { get; set; }
        [DataMember]
        public bool HasDirectorySite { get; set; }
        [DataMember]
        public uint TemplatesCode { get; set; }
        [DataMember]
        public string DisplayName { get; set; }
        [DataMember]
        public bool DisplayQiuckLaunch { get; set; }
        [DataMember]
        public string FullURL { get; set; }
        [DataMember]
        public NewCAPermSetupGetParam GetNewSubSitePermSetupParam { get; set; }
    }

    [DataContract(Namespace = ContractConstants.Namespace)]
    public class SubWebTemplate
    {
        [DataMember]
        public string Language { get; set; }
        [DataMember]
        public int LCID { get; set; }
        [DataMember]
        public List<CreateSubWebTemplate> Templates { get; set; }
    }

    [DataContract(Namespace = ContractConstants.Namespace)]
    public class CreateSubWebTemplate
    {
        [DataMember]
        public string Name { get; set; }
        [DataMember]
        public List<CreateSubWebSubTemplate> SubTemplates { get; set; }
    }

    [DataContract(Namespace = ContractConstants.Namespace)]
    public class CreateSubWebSubTemplate
    {
        [DataMember]
        public string Name { get; set; }
        [DataMember]
        public string Value { get; set; }
        [DataMember]
        public string TemplateDescription { get; set; }

        [DataMember]
        public WebCategoriesType WebCategoriesType { get; set; }
    }

    [DataContract(Namespace = ContractConstants.Namespace)]
    [Flags]
    public enum WebCategoriesType
    {
        [EnumMember]
        None = 0,
        [EnumMember]
        Collaboration = 1,
        [EnumMember]
        Communication = 2,
        [EnumMember]
        Content = 4,
        [EnumMember]
        Data = 8,
        [EnumMember]
        Mettings = 16,
        [EnumMember]
        Search = 32,
        [EnumMember]
        Tracking = 64,
        [EnumMember]
        BlankAndCustom = 128
    }
}
