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
namespace Microsoft365.SharePoint.WebService.FormsServices
{
    using System.Xml;
    using System.Xml.Serialization;
    using static Microsoft365.Common.SoapClient.NameSpaceConst;

    /// <summary>
    /// need to see if SetFormsForListItemResult should be a complex type
    /// </summary>
    [XmlRoot(ElementName = "SetFormsForListItemResponse", Namespace = nsSharePointForms)]
    public class SetFormsForListItemResponse
    {
        [XmlElement(ElementName = "SetFormsForListItemResult")]
        public DesignCheckerInformation SetFormsForListItemResult { get; set; }
    }

    [XmlType(Namespace = nsSharePointForms)]
    public class DesignCheckerInformation
    {

        [XmlElement(Order = 0)]
        public string ApplicationId { get; set; }

        [XmlElement(Order = 1)]
        public int Lcid { get; set; }

        [XmlArray(Order = 2)]
        [XmlArrayItem("Category")]
        public CategoryType[] Categories { get; set; }

        [XmlArray(Order = 3)]
        public Message[] Messages { get; set; }
    }

    [XmlType(Namespace = nsSharePointForms)]
    public partial class CategoryType
    {
        [XmlElement(Order = 0)]
        public Category Id { get; set; }

        [XmlElement(Order = 1)]
        public string Label { get; set; }

        [XmlElement(Order = 2)]
        public bool HideWarningsByDefault { get; set; }
    }

    [XmlType(Namespace = nsSharePointForms)]
    public enum Category
    {

        BrowserOptimization,

        BrowserCompatibility,
    }

    [XmlType(Namespace = nsSharePointForms)]
    public partial class Message
    {

        [XmlElement(Order = 0)]
        public string ShortMessage { get; set; }

        [XmlElement(Order = 1)]
        public string DetailedMessage { get; set; }

        [XmlElement(Order = 2)]
        public SourceLocation SourceLocation { get; set; }

        [XmlAttribute()]
        public int Id { get; set; }

        [XmlAttribute()]
        public MessageType Type { get; set; }

        [XmlAttribute()]
        public Feature Feature { get; set; }

        [XmlAttribute()]
        public Category Category { get; set; }

    }

    [XmlType(Namespace = nsSharePointForms)]
    public partial class SourceLocation
    {
        [XmlAttribute()]
        public string ControlId { get; set; }

        [XmlAttribute()]
        public string FileName { get; set; }

        [XmlAttribute()]
        public int LineNumber { get; set; }

        [XmlIgnore()]
        public bool LineNumberSpecified { get; set; }

        [XmlAttribute()]
        public int LinePosition { get; set; }

        [XmlIgnore()]
        public bool LinePositionSpecified { get; set; }

    }

    [XmlType(Namespace = nsSharePointForms)]
    public enum MessageType
    {


        Error,


        Information,


        Warning,
    }

    [XmlType(Namespace = nsSharePointForms)]
    public enum Feature
    {
        GenericXsf,
        XsfSchema,
        GenericXsl,
        GenericXPath,
        TemplateXml,
        Layout,
        Controls,
        BusinessLogic,
        Calculations,
        Validation,
        DigitalSignatures,
        DataAdapters,
        Submit,
        Views,
        Rules,
        ConditionalFormatting,
        VersionUpgrade,
    }

}
