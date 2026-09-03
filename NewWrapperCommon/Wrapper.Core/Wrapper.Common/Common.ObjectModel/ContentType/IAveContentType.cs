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
using System.Text;

namespace AvePoint.Wrapper.Common
{
    public interface IAveContentType
    {
        string Name { get; set; }
        IAveContentTypeId ID { get; }
        string Description { get; set; }
        string DisplayFormTemplateName { get; set; }
        string DisplayFormUrl { get; set; }
        string DocumentTemplate { get; set; }
        string DocumentTemplateUrl { get; }
        string EditFormTemplateName { get; set; }
        string EditFormUrl { get; set; }
        IAveFieldLinkCollection FieldLinks { get; }
        IAveFieldCollection Fields { get; }
        string Group { get; set; }
        string MD5 { get; set; }
        string NewFormTemplateName { get; set; }
        string NewFormUrl { get; set; }
        bool Hidden { get; set; }
        string JSLink { get; set; }
        string MobileDisplayFormUrl { get; set; }
        string MobileEditFormUrl { get; set; }
        string MobileNewFormUrl { get; set; }
        Guid FeatureId { get; }
        IAveContentType Parent { get; }
        IAveList ParentList { get; }
        IAveWeb ParentWeb { get; }
        bool ReadOnly { get; set; }
        IAveFolder ResourceFolder { get; }
        bool ResourceFolderExists { get; }
        string SchemaXml { get; }
        string SchemaXmlWithResourceTokens { get; set; }
        string Scope { get; }
        bool Sealed { get; set; }
        IAveXmlDocumentCollection XmlDocuments { get; }
        IAveWorkflowAssociationCollection WorkflowAssociations { get; }
        IAveWorkflowAssociation AddWorkflowAssociation(IAveWorkflowAssociation association);
        IAveList List { get; set; }
        IAveWeb Web { get; set; }
        IAveEventReceiverDefinitionCollection EventReceivers { get; }

        void UpdateWorkflowAssociation(IAveWorkflowAssociation workflowAssociation);
        void UpdateWorkflowAssociationsOnChildren();
        void Update();
        void UpdateIncludingSealedAndReadOnly(bool updateChildren);
        void Update(bool updateChildren);
        void Delete();

        string NewDocumentControl { get; set; }
        bool RequireClientRenderingOnNew { get; set; }
        AveContentTypeInfo GetContentTypeInfo(bool backupParent);
        string GetFieldLinkSchemaXml();
        void Initialize(IAveContentTypeCollection collection);

        #region User Resource
        /// <summary>
        /// only support server 10,13 mode. server 07, client mode will return null.
        /// </summary>
        IAveUserResource NameResource { get; }
        /// <summary>
        /// only support server 10,13 mode. server 07, client mode will return null.
        /// </summary>
        IAveUserResource DescriptionResource { get; }
        #endregion
    }
}
