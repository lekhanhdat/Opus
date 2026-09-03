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
namespace LS.SPWorkflowProcessor
{
    using System;

    /// <summary>
    /// get from Nintex.Workflow.dll
    /// </summary>
    public class NWSharePointObjects
    {
        public static readonly string FieldAssociatedContentTypeSchema="<Field ID=\"{4A07D815-9C92-4B43-A527-29C3B76A65BF}\" Name=\"AssociatedContentType\" SourceID=\"http://schemas.microsoft.com/sharepoint/v3\" StaticName=\"AssociatedContentType\" Type=\"Text\" Hidden=\"FALSE\" Group=\"Nintex Workflow\" DisplayName=\"Associated Content Type\"></Field>";
        public static readonly string FieldWorkflowCategorySchema = "<Field ID=\"{d0d7bbf9-95cb-4661-b7a0-9bec8e968c3e}\" Name=\"WorkflowCategory\" SourceID=\"http://schemas.microsoft.com/sharepoint/v3\" StaticName=\"WorkflowCategory\" Type=\"Text\" Hidden=\"FALSE\" Group=\"Nintex Workflow\" DisplayName=\"Category\" Customization=\"\" RowOrdinal=\"0\" />";
        public static readonly string ListNameDefaultWorkflowTaskList = "Workflow Tasks";
        public static readonly string LibraryNameSnippets = "NintexSnippets";
        public static readonly string LibraryNameWorkflows = "NintexWorkflows";
        public static readonly string LibraryNameTemplates = "NintexTemplates";
        public static readonly string ListNameWorkflowHistory = "NintexWorkflowHistory";
        public static readonly string ListNameConfigSettings = "NintexWorkflowConfigSettings";
        public static readonly string LibraryNameSupport = "NintexSupportLibrary";
        public static readonly string NintexWorkflowTaskContentTypeId = "0x0108010079DBDE612F7B46928C6A2516BA2CAE37";
        public static readonly string ContentTypeIdSnippet = "0x010100F815D979DC2B4f48A9DBCA64AED3C636";
        public static readonly string ContentTypeIdTemplate = "0x010100F8376F5313D041ef85718B229F4FBFA8";
        public static readonly string ContentTypeIdWorkflow = "0x01010024055591300C45c3B4C2854A24EF05CE";
        public static readonly string ContentTypeIdMultiOutcomeTask = "0x0108010064E42B14ADA442c78E98D686760A8493";
        public static readonly string ContentTypeIdInfoPathMultiOutcomeTask = "0x0108010064E42B14ADA442c78E98D686760A8493006EBD0FA4731041d9804386A7FEA568DC";
        public static readonly string ContentTypeIdInfoPathTask = "0x0108010079DBDE612F7B46928C6A2516BA2CAE3700E0B65C5281234030AA8CA4D8F8910E72";
        public static readonly string ContentTypeIdSupportLibrary = "0x01010000DB14451727D24CADC6093F0C0EB0F4";
        public static readonly string FieldWorkflowTaskIDName = "WorkflowTaskID";
        public static readonly string FieldUserIDName = "UserID";
        public static readonly Guid FieldAssociatedListID = new Guid("{CA9E1FEB-0014-46b0-87C8-08FB9E2FE003}");
        public static readonly Guid FieldComments = new Guid("{819E6CF2-36C3-4013-8AEF-C99712C26036}");
        public static readonly Guid FieldApprovalOutcome = new Guid("{6765859B-8902-469f-A8B6-FAA121304602}");
        public static readonly Guid FieldDecision = new Guid("{A7AE99D0-E5DF-47f4-9D75-560E3F608006}");
        public static readonly Guid FieldDatabaseID = new Guid("{1E39A08D-5F96-494b-AD5A-9096A323B1DA}");
        public static readonly Guid FieldHumanWorkflowID = new Guid("{CABA3010-7526-43c2-B05F-786037B1DCDF}");
        public static readonly Guid FieldApproverTaskID = new Guid("{9CF1474E-C190-4d8d-AD6E-E26CBCBD587B}");
        public static readonly Guid FieldWorkflowPartId = new Guid("{7F3814A0-6EF1-4856-BF1D-1A06F8437DC4}");
        public static readonly Guid FieldWorkflowPartDescription = new Guid("{607EC2F6-48EB-4a14-9E1E-BC48043E157E}");
        public static readonly Guid FieldTemplateCategory = new Guid("{6ABEAA08-87C5-4385-9C81-39CBB72F99A6}");
        public static readonly Guid FieldTemplateLcid = new Guid("{837C2B97-E338-446d-A993-C96FD0C4B5D7}");
        public static readonly Guid FieldMultiOutcomeTaskInfo = new Guid("{F9C2546C-F40B-46d5-8BAC-9AB11CC7D640}");
        public static readonly Guid FieldAssociatedWebId = new Guid("1D5AFADD-D013-4d88-B3B2-38B570DA9B6F");
        public static readonly string FieldNameAssociatedList = "AssociatedListID";
        public static readonly string FieldNameWorkflowId = "NintexWorkflowID";
        public static readonly string FieldNameWorkflowDescription = "NintexWorkflowDescription";
        public static readonly Guid FieldWorkflowCategory = new Guid("{D0D7BBF9-95CB-4661-B7A0-9BEC8E968C3E}");
        public static readonly string FieldNameWorkflowCategory = "WorkflowCategory";
        public static readonly Guid FieldAssociatedContentType = new Guid("{4A07D815-9C92-4B43-A527-29C3B76A65BF}");
        public static readonly string FieldNameAssociatedContentType = "AssociatedContentType";
        public static readonly string ApprovalOutcomeFieldName = "ApprovalOutcome";
        public static readonly string DecisionFieldName = "Decision";
        public static readonly string ApproverCommentsFieldName = "ApproverComments";
        public static readonly string WorkflowTaskIDFieldName = "WorkflowTaskID";
        public static readonly string DatabaseIDFieldName = "DatabaseID";
        public static readonly string HumanWorkflowIDFieldName = "HumanWorkflowID";
        public static readonly string ApproverTaskIDFieldName = "ApproverTaskID";
        public static readonly string TemplateLcidFieldName = "TemplateLcid";
        public static readonly string MultiOutcomeTaskInfoFieldName = "MultiOutcomeTaskInfo";
        public static readonly string ContentTypeIdBiztalkTask = "0x010801005CC0A86910A24687A76ECAC954D3E3F3";
        public static readonly Guid FieldWaitingMessageId = new Guid("{CACA9759-3B72-4bf1-8927-7E101337ECF1}");
        public static readonly string FieldNameWaitingMessageId = "WaitingMessageId";
        public static readonly Guid FieldMessageData = new Guid("{BE604058-3251-4088-AAD6-E702C4FF1905}");
        public static readonly string FieldNameMessageData = "MessageData";
        public static readonly Guid FieldXsd = new Guid("{CB2EAE1E-CAB5-44DC-A9DC-84641EB2ADA6}");
        public static readonly string FieldNameXsd = "Xsd";
        public static readonly string NintexCatalogTemplateName = "NintexCatalog";
        public static readonly string NintexWorkflowSiteColumnGroup = "Nintex Workflow";
        public static readonly Guid FeatureWeb = new Guid("9BF7BF98-5660-498a-9399-BC656A61ED5D");
        public static readonly Guid FeatureSiteCollection = new Guid("0561D315-D5DB-4736-929E-26DA142812C5");
        public static readonly Guid FeatureAdmin = new Guid("F7937973-0CF9-4f2d-A549-BE2D3C25B772");
        public static readonly Guid FeatureSiteCollectionContentTypeUpgrade = new Guid("86C83D16-605D-41b4-BFDD-C75947899AC7");
        public static readonly Guid FeatureInfoPathForms = new Guid("80BF3218-7353-11DF-AF9F-058BDFD72085");
        public static readonly Guid FeatureWebParts = new Guid("EB657559-BE37-4b91-A369-1C201183C779");
        public static readonly Guid FeatureReportingWebParts = new Guid("53164B55-E60F-4bed-B582-A87DA32B92F1");
        public static readonly Guid FeatureLiveWeb = new Guid("54668547-c03f-4bb5-aaab-d9568ebaf9c9");
        public static readonly Guid FeatureLiveAdmin = new Guid("485f5158-4b8a-453f-9eeb-7b33f5112adf");
        public static readonly Guid SolutionIdCore = new Guid("8d48a0ce-daf4-4a8a-87fe-713fc354964e");
        public static readonly Guid NintexLiveSolutionId = new Guid("1ddec2be-094d-4a9b-b9e1-fdca27b07646");
        public static readonly Guid NintexLiveAdminFeatureId = new Guid("29e9a673-31a4-46a3-b0d2-d8e1db1dbd92");
    }
}
