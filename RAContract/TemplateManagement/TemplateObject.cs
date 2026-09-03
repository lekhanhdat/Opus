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
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;
using AvePoint.RA.Contract.RMWeb.TemplateManagement;

namespace AvePoint.RA.Contract.TemplateManagement
{
    public class TemplateObject
    {
    }

    public static class DefaultColumnIDs
    {
        public const string NameOrTitle = "de5e99cb-4fb4-4e25-b732-a1dce71dd048";
        public const string Description = "dd5b8a0a-96d7-42c0-ba1b-a6cbef4bdb81";
        public const string Status = "eb4e9ab7-c939-425b-9e29-235236c9ce5b";
        public const string Format = "9333da20-6a70-4a4e-9013-33ee8f0539cd";
        public const string ProtectiveMarking = "01aefeb6-6cb1-419f-b476-fee650045778";
        public const string Rights = "e56dcd2e-fbb7-4e31-bfff-5347d538f88e";
        public const string Coverage = "fe3dce03-2e91-400a-8b6d-446b6ef8820e";
        public const string DateClosed = "99b6d3fb-688d-4d19-9cfb-2a3f70c07aa9";
        public const string HomeLocation = "d2568d7d-4891-46d2-8eb2-2e8c032a41bf";
        public const string Capability = "8951a839-c8df-4bfe-8dfb-de204297629d";
        public const string Path = "8951a839-c8df-4bfe-8dfb-de204297629e";

        public const string Classification = "aedcf21f-dfdb-41d3-935a-5c5859187754";
        public const string UniqueId = "c980eb95-ea92-4f07-9f97-1a8ab2a053fa";
        public const string CreatedBy = "cf054564-6482-4ff4-95ad-2a84ab3f8262";
        public const string ModifiedBy = "16aa21f3-88a4-47ed-ae4c-07ea2e5c0e45";
        [Obsolete]
        public const string LoanedBy_Old = "a06982f7-a3cc-46d6-8de3-52ddc094efce";
        //public const string LoanedBy = "a06982f7-a3cc-46d6-8de3-52ddc094efce";
        public const string LoanedBy = "df21d79c-bc37-fdfd-f59e-641f7d630488"; //new id for loaned by
		public const string Barcode = "86c62a72-bb6c-4fea-8c42-a2944c1547c2";

        public static List<string> AllIDs = new List<string>()
        {
			NameOrTitle,
			Description,
			Status,
			Format,
			ProtectiveMarking,
			Rights,
			Coverage,
			DateClosed,
			HomeLocation,
			Capability,
			Path,
			Classification,
			UniqueId,
			CreatedBy,
			ModifiedBy,
			LoanedBy,
            Barcode
        };

		public static List<string> HideForBulkUpdateIDs = new List<string>()
		{
			NameOrTitle,
			//Description,
			Status,
			//Capability,
			Path,
			Classification,
			UniqueId,
			CreatedBy,
			ModifiedBy,
			LoanedBy,
			HomeLocation
		};
	}

    /// <summary>
    /// used for advanced search
    /// </summary>
    public static class QueryCloumnIds
    {
        public const string CreatedBy = "91a08d45-c5dd-43da-b6c4-670f11ac273e";
        public const string TimeCreated = "c55a2cc4-2825-42ff-b1d4-fb72b7be7dc5";
        public const string DeclareAsRecord = "bf4e131c-1d9b-403b-8a9f-a1fa3b63cd15";
        public const string DisposalDueDate = "9117fd6b-4171-4405-b881-cbe139e6ced7";
        public const string FileExtension = "90c0f7ce-ad79-4a9d-a5eb-3b097006b03d";
        public const string HoldBy = "8499e388-9c52-4366-a7b3-df77c70e648f";
        public const string HoldStatus = "f9806a66-1be8-4f85-867e-f0de4fa4c073";
        public const string ModifiedBy = "1f2e8c3f-e49a-473c-bd16-8647258cf15c";
        public const string TimeModified = "3ec9a488-90fa-4d62-835f-0df0cd2e9f97";
        public const string NodeId = "becf61cd-bd6b-440c-8e33-4b6300be58d5"; //FS tree node Id
        public const string RecordOwner = "38e1e287-4077-44a5-ba57-3de64561c51f";
        public const string LoanDate = "73d26ac4-10a1-4292-9bb5-9656883e56b4";
        public const string Loan = "f693dcc8-6e52-423f-849c-1cbac642ad3f";
		public const string ContentArchived = "8cd3ffc4-0ebd-461a-8d63-347851abc60e";
        public const string Id = "220F3019-51F8-4A88-89E4-C7DFF1859B4E";
        public const string DirPath = "a6c8f7d1-8d5f-4eb4-9dcb-2b0f3f9c5d62";

		//public const string Status = "eb4e9ab7-c939-425b-9e29-235236c9ce5b";
		public const string SourceFlag = "edbac887-d4cc-ed92-ad0d-0e68ceb336a0";

        public const string Term = "ce693d2c-ab58-4d29-9db5-3191bfc5c81a";
        public const string TermIdNotContain = "f3b3fbd3-46ef-4a7d-865e-81480561a7df";

        //public const string NameOrUniqueId = "38f015c0-f507-4925-a855-d1546dc0b0f9";
        public const string SPOLocation = "ee86426d-488f-4bdb-a63b-2ef6a61c7bef";
		public const string TeamsLocation = "f0a0d8f6-71f9-4d42-a3bf-ab828eb73ded";
		public const string GoogleLocation = "831d8bcf-fb12-4cce-ba5e-a1a37ccf5234";

        public const string TemplateId = "04a17a80-1bd9-4eaa-9cf1-4bce21c1df01";

        public const string RuleId = "a4d802a2-96af-4e9c-8a48-349375fcb0f6";
        public const string RecordStatus = "56491865-292e-4f6e-9a45-46de1203c130";
        public const string DestroyedTime = "ad0faf5d-52b3-4f10-b2c7-2f013d2c6c26";
        public const string LoanPickStatus = "fbeeeed5-fd23-415f-b977-dc72a0428b71";
        public const string DestructionPickStatus = "d3b182f9-50db-46c5-8048-a59fbfe7e30d";
        public const string TrainingScope = "19bf8d41-f302-47c8-b038-cd4a1caabe84";
        public const string TrainingTermId = "57b5646f-b3b8-46a9-8b04-db1c40d6f561";
        public const string PredictTermId = "ef178a07-892d-4a36-8c54-121de18aabbc";
        public const string PredictTime = "af2af466-e14b-442f-a492-2abeacfeafb3";
        public const string MLApprovalStatus = "64d3150a-c8c4-497e-963c-e38fffdfa3ef";
		public const string NodeType = "f5e94b3d-12b1-4c95-8756-2bca6f1a9d7e";
		public const string ContainerId = "a3f7d9c2-0a55-4c1e-b84c-ec7c4baf9d0d";
		public const string ScopeId = "49deff37-6c96-4f6c-a9b2-126ad88a2c12";
		public const string WebIds = "58a1f401-27f0-4d3d-bb3b-ff91f92fdd86";
		public const string ListId = "c79d0f6f-9c26-4a10-a4e8-7b250019e876";
        public const string TermId = "0f43e3a2-0f84-4e7e-9d4f-8abdd0a8a82b";
		public const string HoldByUsersId = "e2e2e7e2-1c2a-4b7a-9b2e-2e2e7e2e7e2e";
		public const string ManualCollectionTime = "b3c9f1c2-8f0f-4e1c-9a5b-3e8d3dcb43fa";
		public const string ManualRuleName = "f2d4b6c9-3a7e-4b1f-9d2c-8e5a1f0b6c3d";
		public const string ManualRuleDisposalClass = "c1a4f0b7-5d8e-4e32-9c71-2f4d9ebc8f55";
		public const string ManualEscalateFrom = "93f7bb4a-1e4d-4c54-a6d7-3f0cae6c4b9e";
		public const string ManualReviewer = "1a8f9c3e-5b2d-40e1-a7f4-6d9b0c2e3f1a";
		public const string LeafNameArray = "4f1e9b2c-6a7d-4c3e-9f0b-5d8a1c7f6e3d";
		public const string Workspace = "e8a7d1c0-3f2b-4a5d-8b6c-9e0f2d1a4b3c";
		public const string ManualFolderPath = "7b8d4f1a-2c3e-4d5b-9f0a-6e7c8b9d0e1f";
		public const string QuickReason = "8a9f4c3b-1d2e-4f5a-8b0c-6e7d8f9a0b1c";
		public const string ManualModifiedTime = "2c5a08e6-1f3c-4d7b-9e2a-0b4f6d8c1a9e";
		public const string ManualApprovalStatus = "d3a5b8c1-7e6f-4a29-b0c4-1f8e3d2c5a9b";
        public const string ContentManual = "b8c3d9a1-f7e6-4b2c-8a1d-0e9f2c7a5b4d";
		public const string LocationId = "3f9a7c2e-6d4b-4a9c-8b71-5e6a0d2f4c91";
		public const string LockedByRecordLabel = "a8b3c9d1-e2f4-4a5c-9b8d-7e6f5a4c3b21";

    }

    public static class BuildInColumnIDs
    {
        public const string RecordsId = "62AB4A7B-960E-4D34-9D44-ACAD71EC3E13";
        public const string CreatedBy = "BB2CFC11-0DE6-4DAE-8414-0FBAD2EBD8D7";
        public const string CreatedTime = "96CE5E52-3A1B-4E99-9F75-6954D27D2FEE";
        public const string ModifiedBy = "B12C2382-FCFD-4B55-8446-B41A20C25AF0";
        public const string ModifiedTime = "332844A6-DAF6-4488-9BF4-1F36BAD58426";
    }


    public static class DefaultSuiteIds
    {
        public const string RECORD_SUITE_DEFAULT_BOX_SUITE_ID = "6FEECEA2-2076-4557-AE9C-A90F9EB91617";
        public const string RECORD_SUITE_DEFAULT_FOLDER_SUITE_ID = "C7A9A849-C9A3-4C0B-BA38-BA0DB43AF048";
    }

    public static class DefaultTemplateIds
    {
        public const string BOX_TEMPLATE_ID = "F0B53A20-D955-476B-BB83-41488CFB2750";
        public const string FOLDER_TEMPLATE_ID = "B775E3C7-20A8-4141-98FC-49824A028331";
        public const string RECORD_TEMPLATE_ID = "01BD2C27-D4D5-4714-8EF3-E460323A977B";
    }

    public enum ViewMode
    {
        CardView = 1,
        ListView = 2
    }
	[DataContract]
    public enum SuiteStartFromType
    {
		[EnumMember]
        None = 0,
        [EnumMember]
        Box = 1,
        [EnumMember]
        Folder = 2,
        [EnumMember]
        Custom = 3
    }

    [DataContract]
    public enum TemplateType
    {
        [EnumMember]
        Records = 1,
        [EnumMember]
        Folder = 2,
        [EnumMember]
        Box = 3,
        [EnumMember]
        Location = 4,
        [EnumMember]
        Custom = 5,
        [EnumMember]
        Suite = 6
    }
	[DataContract]
    public enum BarcodeTemplateType
    {
        [EnumMember]
        Box = 1,
        [EnumMember]
        Folder = 2
    }
    [DataContract]
    public enum SuiteRootTemplateCreateType
    {
        [EnumMember]
        New = 0,
        [EnumMember]
        ExistingFolder = 1
    }
    [DataContract]
    public enum ColumnType
    {
        [EnumMember]
        SingleText = 1,
        [EnumMember]
        MultipleText = 2,
        [EnumMember]
        DateTime = 3,
        [EnumMember]
        SingleChoice = 4,
        [EnumMember]
        PeopleOrGroup = 5,
        [EnumMember]
        Number = 6,
        [EnumMember]
        MultipleChoice = 7,
        [EnumMember]
        Taxonomy = 10,
        [EnumMember]
        Identifier = 11,
        [EnumMember]
        YesOrNo = 12,
    }

    public enum TemplateInheritSettingEnum
    {
        PushToChild = 32,       //0010_0000
        InheritFromParentFolder = 16, //0001_0000
        ChildInheritsValue = 8, //0000_1000
        InheritFromParentBox = 4,  //0000_0100
        AllowModifyValue = 2,   //0000_0010
        None = 0
    }

    public class TemplateColumnsSchema
    {
        public List<ColumnXmlSchema> Columns { set; get; }
    }

    public class ColumnXmlSchema
    {
        public Guid UniqueId { set; get; }

        public Guid CategoryId { set; get; }

        public string Name { get; set; }

        public ColumnType ColumnType { set; get; }

        public bool Required { get; set; }

        public bool ShowInEditForm { get; set; }

        public int TemplateInheritSetting { get; set; }

        public bool AllowEdit { get; set; }
        public bool? AllowSort { get; set; }

        public string OptionsJSON { get; set; }

        public int OptionsMaxIdReachedValue { get; set; }

        public Guid PushToFolderCategoryId { get; set; }

        public Guid PushToRecordCategoryId { get; set; }
        public List<TemplateIdAndCategoryId> pushFoldTemplateCategoriesId { get; set; }
        public List<TemplateIdAndCategoryId> pushRecordTemplateCategoriesId { get; set; }
    }

    public class DefaultTemplateData
    {
        #region create box template default data
        public const string DEFAULT_DATA_BOX_TEMPLATE_XML = @"
<TemplateColumnsSchema xmlns:i=""http://www.w3.org/2001/XMLSchema-instance"" xmlns=""http://schemas.datacontract.org/2004/07/AvePoint.RA.Contract.TemplateManagement"">
	<Columns>
		<ColumnXmlSchema>
			<AllowEdit>false</AllowEdit>
			<CategoryId>11d303d8-d6fb-4a2b-a87d-3a18e2ac2d9a</CategoryId>
			<ColumnType>SingleText</ColumnType>
			<Name>RM_Template_Column_Name_Title</Name>
			<OptionsJSON i:nil=""true"" />
			<OptionsMaxIdReachedValue>0</OptionsMaxIdReachedValue>
			<PushToFolderCategoryId>00000000-0000-0000-0000-000000000000</PushToFolderCategoryId>
			<PushToRecordCategoryId>00000000-0000-0000-0000-000000000000</PushToRecordCategoryId>
			<Required>true</Required>
			<ShowInEditForm>true</ShowInEditForm>
			<TemplateInheritSetting>0</TemplateInheritSetting>
			<UniqueId>de5e99cb-4fb4-4e25-b732-a1dce71dd048</UniqueId>
		</ColumnXmlSchema>
		<ColumnXmlSchema>
			<AllowEdit>true</AllowEdit>
			<CategoryId>11d303d8-d6fb-4a2b-a87d-3a18e2ac2d9a</CategoryId>
			<ColumnType>MultipleText</ColumnType>
			<Name>RM_Template_Column_Name_Description</Name>
			<OptionsJSON i:nil=""true"" />
			<OptionsMaxIdReachedValue>0</OptionsMaxIdReachedValue>
			<PushToFolderCategoryId>00000000-0000-0000-0000-000000000000</PushToFolderCategoryId>
			<PushToRecordCategoryId>00000000-0000-0000-0000-000000000000</PushToRecordCategoryId>
			<Required>false</Required>
			<ShowInEditForm>true</ShowInEditForm>
			<TemplateInheritSetting>0</TemplateInheritSetting>
			<UniqueId>dd5b8a0a-96d7-42c0-ba1b-a6cbef4bdb81</UniqueId>
		</ColumnXmlSchema>
		<ColumnXmlSchema>
			<AllowEdit>true</AllowEdit>
			<CategoryId>11d303d8-d6fb-4a2b-a87d-3a18e2ac2d9a</CategoryId>
			<ColumnType>Number</ColumnType>
			<Name>RM_Template_Column_Name_Capability</Name>
			<OptionsJSON i:nil=""true"" />
			<OptionsMaxIdReachedValue>0</OptionsMaxIdReachedValue>
			<PushToFolderCategoryId>00000000-0000-0000-0000-000000000000</PushToFolderCategoryId>
			<PushToRecordCategoryId>00000000-0000-0000-0000-000000000000</PushToRecordCategoryId>
			<Required>false</Required>
			<ShowInEditForm>true</ShowInEditForm>
			<TemplateInheritSetting>0</TemplateInheritSetting>
			<UniqueId>8951a839-c8df-4bfe-8dfb-de204297629d</UniqueId>
		</ColumnXmlSchema>
		<ColumnXmlSchema>
			<AllowEdit>false</AllowEdit>
			<CategoryId>11d303d8-d6fb-4a2b-a87d-3a18e2ac2d9a</CategoryId>
			<ColumnType>Taxonomy</ColumnType>
			<Name>RM_Template_Column_Name_HomeLocation</Name>
			<OptionsJSON i:nil=""true"" />
			<OptionsMaxIdReachedValue>0</OptionsMaxIdReachedValue>
			<PushToFolderCategoryId>00000000-0000-0000-0000-000000000000</PushToFolderCategoryId>
			<PushToRecordCategoryId>00000000-0000-0000-0000-000000000000</PushToRecordCategoryId>
			<Required>true</Required>
			<ShowInEditForm>true</ShowInEditForm>
			<TemplateInheritSetting>0</TemplateInheritSetting>
			<UniqueId>d2568d7d-4891-46d2-8eb2-2e8c032a41bf</UniqueId>
		</ColumnXmlSchema>
		<ColumnXmlSchema>
			<AllowEdit>false</AllowEdit>
			<CategoryId>11d303d8-d6fb-4a2b-a87d-3a18e2ac2d9a</CategoryId>
			<ColumnType>SingleChoice</ColumnType>
			<Name>RM_Template_Column_Name_Status</Name>
			<OptionsJSON>{""1"":""RM_Template_Column_Value_Status_Open"",""2"":""RM_Template_Column_Value_Status_Destroyed"",""6"":""RM_Template_Column_Value_Status_Closed"",""7"":""RM_Template_Column_Value_Status_Missing""}</OptionsJSON>
			<OptionsMaxIdReachedValue>0</OptionsMaxIdReachedValue>
			<PushToFolderCategoryId>00000000-0000-0000-0000-000000000000</PushToFolderCategoryId>
			<PushToRecordCategoryId>00000000-0000-0000-0000-000000000000</PushToRecordCategoryId>
			<Required>true</Required>
			<ShowInEditForm>false</ShowInEditForm>
			<TemplateInheritSetting>0</TemplateInheritSetting>
			<UniqueId>eb4e9ab7-c939-425b-9e29-235236c9ce5b</UniqueId>
		</ColumnXmlSchema>
		<ColumnXmlSchema>
			<AllowEdit>false</AllowEdit>
			<CategoryId>11d303d8-d6fb-4a2b-a87d-3a18e2ac2d9a</CategoryId>
			<ColumnType>Taxonomy</ColumnType>
			<Name>RM_Template_Column_Name_Classification</Name>
			<OptionsJSON i:nil=""true"" />
			<OptionsMaxIdReachedValue>0</OptionsMaxIdReachedValue>
			<PushToFolderCategoryId>00000000-0000-0000-0000-000000000000</PushToFolderCategoryId>
			<PushToRecordCategoryId>00000000-0000-0000-0000-000000000000</PushToRecordCategoryId>
			<Required>true</Required>
			<ShowInEditForm>true</ShowInEditForm>
			<TemplateInheritSetting>0</TemplateInheritSetting>
			<UniqueId>aedcf21f-dfdb-41d3-935a-5c5859187754</UniqueId>
		</ColumnXmlSchema>
		<ColumnXmlSchema>
			<AllowEdit>false</AllowEdit>
			<CategoryId>11d303d8-d6fb-4a2b-a87d-3a18e2ac2d9a</CategoryId>
			<ColumnType>PeopleOrGroup</ColumnType>
			<Name>RM_PRM_PRE_Column_LoanBy</Name>
			<OptionsJSON i:nil=""true"" />
			<OptionsMaxIdReachedValue>0</OptionsMaxIdReachedValue>
			<PushToFolderCategoryId>00000000-0000-0000-0000-000000000000</PushToFolderCategoryId>
			<PushToRecordCategoryId>00000000-0000-0000-0000-000000000000</PushToRecordCategoryId>
			<Required>false</Required>
			<ShowInEditForm>true</ShowInEditForm>
			<TemplateInheritSetting>0</TemplateInheritSetting>
			<UniqueId>df21d79c-bc37-fdfd-f59e-641f7d630488</UniqueId>
		</ColumnXmlSchema>
		<ColumnXmlSchema>
			<AllowEdit>false</AllowEdit>
			<CategoryId>11d303d8-d6fb-4a2b-a87d-3a18e2ac2d9a</CategoryId>
			<ColumnType>SingleText</ColumnType>
			<Name>RM_PRM_PRE_Column_Barcode</Name>
			<OptionsJSON i:nil=""true"" />
			<OptionsMaxIdReachedValue>0</OptionsMaxIdReachedValue>
			<PushToFolderCategoryId>00000000-0000-0000-0000-000000000000</PushToFolderCategoryId>
			<PushToRecordCategoryId>00000000-0000-0000-0000-000000000000</PushToRecordCategoryId>
			<Required>false</Required>
			<ShowInEditForm>true</ShowInEditForm>
			<TemplateInheritSetting>0</TemplateInheritSetting>
			<UniqueId>86c62a72-bb6c-4fea-8c42-a2944c1547c2</UniqueId>
		</ColumnXmlSchema>
	</Columns>
</TemplateColumnsSchema>";
        #endregion

        #region create folder template default data
        public const string DEFAULT_DATA_FOLDER_TEMPLATE_XML = @"<TemplateColumnsSchema xmlns:i=""http://www.w3.org/2001/XMLSchema-instance"" xmlns=""http://schemas.datacontract.org/2004/07/AvePoint.RA.Contract.TemplateManagement"">
	<Columns>
		<ColumnXmlSchema>
			<AllowEdit>false</AllowEdit>
			<CategoryId>d192c525-4a1e-48a2-9c00-f864a26571cf</CategoryId>
			<ColumnType>SingleText</ColumnType>
			<Name>RM_Template_Column_Name_Title</Name>
			<OptionsJSON i:nil=""true"" />
			<OptionsMaxIdReachedValue>0</OptionsMaxIdReachedValue>
			<PushToFolderCategoryId>00000000-0000-0000-0000-000000000000</PushToFolderCategoryId>
			<PushToRecordCategoryId>00000000-0000-0000-0000-000000000000</PushToRecordCategoryId>
			<Required>true</Required>
			<ShowInEditForm>true</ShowInEditForm>
			<TemplateInheritSetting>0</TemplateInheritSetting>
			<UniqueId>de5e99cb-4fb4-4e25-b732-a1dce71dd048</UniqueId>
		</ColumnXmlSchema>
		<ColumnXmlSchema>
			<AllowEdit>true</AllowEdit>
			<CategoryId>d192c525-4a1e-48a2-9c00-f864a26571cf</CategoryId>
			<ColumnType>MultipleText</ColumnType>
			<Name>RM_Template_Column_Name_Description</Name>
			<OptionsJSON i:nil=""true"" />
			<OptionsMaxIdReachedValue>0</OptionsMaxIdReachedValue>
			<PushToFolderCategoryId>00000000-0000-0000-0000-000000000000</PushToFolderCategoryId>
			<PushToRecordCategoryId>00000000-0000-0000-0000-000000000000</PushToRecordCategoryId>
			<Required>false</Required>
			<ShowInEditForm>true</ShowInEditForm>
			<TemplateInheritSetting>0</TemplateInheritSetting>
			<UniqueId>dd5b8a0a-96d7-42c0-ba1b-a6cbef4bdb81</UniqueId>
		</ColumnXmlSchema>
		<ColumnXmlSchema>
			<AllowEdit>false</AllowEdit>
			<CategoryId>d192c525-4a1e-48a2-9c00-f864a26571cf</CategoryId>
			<ColumnType>SingleChoice</ColumnType>
			<Name>RM_Template_Column_Name_Status</Name>
			<OptionsJSON>{""1"":""RM_Template_Column_Value_Status_Open"",""2"":""RM_Template_Column_Value_Status_Destroyed"",""6"":""RM_Template_Column_Value_Status_Closed"",""7"":""RM_Template_Column_Value_Status_Missing""}</OptionsJSON>
			<OptionsMaxIdReachedValue>0</OptionsMaxIdReachedValue>
			<PushToFolderCategoryId>00000000-0000-0000-0000-000000000000</PushToFolderCategoryId>
			<PushToRecordCategoryId>00000000-0000-0000-0000-000000000000</PushToRecordCategoryId>
			<Required>true</Required>
			<ShowInEditForm>false</ShowInEditForm>
			<TemplateInheritSetting>0</TemplateInheritSetting>
			<UniqueId>eb4e9ab7-c939-425b-9e29-235236c9ce5b</UniqueId>
		</ColumnXmlSchema>
		<ColumnXmlSchema>
			<AllowEdit>false</AllowEdit>
			<CategoryId>d192c525-4a1e-48a2-9c00-f864a26571cf</CategoryId>
			<ColumnType>Taxonomy</ColumnType>
			<Name>RM_Template_Column_Name_Classification</Name>
			<OptionsJSON i:nil=""true"" />
			<OptionsMaxIdReachedValue>0</OptionsMaxIdReachedValue>
			<PushToFolderCategoryId>00000000-0000-0000-0000-000000000000</PushToFolderCategoryId>
			<PushToRecordCategoryId>00000000-0000-0000-0000-000000000000</PushToRecordCategoryId>
			<Required>true</Required>
			<ShowInEditForm>true</ShowInEditForm>
			<TemplateInheritSetting>0</TemplateInheritSetting>
			<UniqueId>aedcf21f-dfdb-41d3-935a-5c5859187754</UniqueId>
		</ColumnXmlSchema>
		<ColumnXmlSchema>
			<AllowEdit>false</AllowEdit>
			<CategoryId>d192c525-4a1e-48a2-9c00-f864a26571cf</CategoryId>
			<ColumnType>PeopleOrGroup</ColumnType>
			<Name>RM_PRM_PRE_Column_LoanBy</Name>
			<OptionsJSON i:nil=""true"" />
			<OptionsMaxIdReachedValue>0</OptionsMaxIdReachedValue>
			<PushToFolderCategoryId>00000000-0000-0000-0000-000000000000</PushToFolderCategoryId>
			<PushToRecordCategoryId>00000000-0000-0000-0000-000000000000</PushToRecordCategoryId>
			<Required>false</Required>
			<ShowInEditForm>true</ShowInEditForm>
			<TemplateInheritSetting>0</TemplateInheritSetting>
			<UniqueId>df21d79c-bc37-fdfd-f59e-641f7d630488</UniqueId>
		</ColumnXmlSchema>
		<ColumnXmlSchema>
			<AllowEdit>false</AllowEdit>
			<CategoryId>d192c525-4a1e-48a2-9c00-f864a26571cf</CategoryId>
			<ColumnType>SingleText</ColumnType>
			<Name>RM_PRM_PRE_Column_Barcode</Name>
			<OptionsJSON i:nil=""true"" />
			<OptionsMaxIdReachedValue>0</OptionsMaxIdReachedValue>
			<PushToFolderCategoryId>00000000-0000-0000-0000-000000000000</PushToFolderCategoryId>
			<PushToRecordCategoryId>00000000-0000-0000-0000-000000000000</PushToRecordCategoryId>
			<Required>false</Required>
			<ShowInEditForm>true</ShowInEditForm>
			<TemplateInheritSetting>0</TemplateInheritSetting>
			<UniqueId>86c62a72-bb6c-4fea-8c42-a2944c1547c2</UniqueId>
		</ColumnXmlSchema>
		<ColumnXmlSchema>
			<AllowEdit>true</AllowEdit>
			<CategoryId>2d7d5d51-a541-4c18-bd5c-ae5fa633d5cf</CategoryId>
			<ColumnType>SingleChoice</ColumnType>
			<Name>RM_Template_Column_Name_Format</Name>
			<OptionsJSON>{""1"":""RM_Template_Column_Value_Format_Document"",""2"":""RM_Template_Column_Value_Format_Cassette"",""3"":""RM_Template_Column_Value_Format_Map"",""4"":""RM_Template_Column_Value_Format_Play"",""5"":""RM_Template_Column_Value_Format_DVD""}</OptionsJSON>
			<OptionsMaxIdReachedValue>0</OptionsMaxIdReachedValue>
			<PushToFolderCategoryId>00000000-0000-0000-0000-000000000000</PushToFolderCategoryId>
			<PushToRecordCategoryId>00000000-0000-0000-0000-000000000000</PushToRecordCategoryId>
			<Required>true</Required>
			<ShowInEditForm>true</ShowInEditForm>
			<TemplateInheritSetting>0</TemplateInheritSetting>
			<UniqueId>9333da20-6a70-4a4e-9013-33ee8f0539cd</UniqueId>
		</ColumnXmlSchema>
		<ColumnXmlSchema>
			<AllowEdit>true</AllowEdit>
			<CategoryId>2d7d5d51-a541-4c18-bd5c-ae5fa633d5cf</CategoryId>
			<ColumnType>SingleChoice</ColumnType>
			<Name>RM_Template_Column_Name_ProtectiveMarking</Name>
			<OptionsJSON>{""1"":""RM_Template_Column_Value_ProtectiveMarking_InternalUsedOnly"",""2"":""RM_Template_Column_Value_ProtectiveMarking_Public"",""3"":""RM_Template_Column_Value_ProtectiveMarking_Confidential"",""4"":""RM_Template_Column_Value_ProtectiveMarking_HighlyConfidential""}</OptionsJSON>
			<OptionsMaxIdReachedValue>0</OptionsMaxIdReachedValue>
			<PushToFolderCategoryId>00000000-0000-0000-0000-000000000000</PushToFolderCategoryId>
			<PushToRecordCategoryId>00000000-0000-0000-0000-000000000000</PushToRecordCategoryId>
			<Required>true</Required>
			<ShowInEditForm>true</ShowInEditForm>
			<TemplateInheritSetting>0</TemplateInheritSetting>
			<UniqueId>01aefeb6-6cb1-419f-b476-fee650045778</UniqueId>
		</ColumnXmlSchema>
		<ColumnXmlSchema>
			<AllowEdit>true</AllowEdit>
			<CategoryId>5c1875ae-0f81-4249-a036-64f91b29b02d</CategoryId>
			<ColumnType>MultipleText</ColumnType>
			<Name>RM_Template_Column_Name_Rights</Name>
			<OptionsJSON i:nil=""true"" />
			<OptionsMaxIdReachedValue>0</OptionsMaxIdReachedValue>
			<PushToFolderCategoryId>00000000-0000-0000-0000-000000000000</PushToFolderCategoryId>
			<PushToRecordCategoryId>00000000-0000-0000-0000-000000000000</PushToRecordCategoryId>
			<Required>false</Required>
			<ShowInEditForm>true</ShowInEditForm>
			<TemplateInheritSetting>0</TemplateInheritSetting>
			<UniqueId>e56dcd2e-fbb7-4e31-bfff-5347d538f88e</UniqueId>
		</ColumnXmlSchema>
		<ColumnXmlSchema>
			<AllowEdit>true</AllowEdit>
			<CategoryId>5c1875ae-0f81-4249-a036-64f91b29b02d</CategoryId>
			<ColumnType>MultipleText</ColumnType>
			<Name>RM_Template_Column_Name_Coverage</Name>
			<OptionsJSON i:nil=""true"" />
			<OptionsMaxIdReachedValue>0</OptionsMaxIdReachedValue>
			<PushToFolderCategoryId>00000000-0000-0000-0000-000000000000</PushToFolderCategoryId>
			<PushToRecordCategoryId>00000000-0000-0000-0000-000000000000</PushToRecordCategoryId>
			<Required>false</Required>
			<ShowInEditForm>true</ShowInEditForm>
			<TemplateInheritSetting>0</TemplateInheritSetting>
			<UniqueId>fe3dce03-2e91-400a-8b6d-446b6ef8820e</UniqueId>
		</ColumnXmlSchema>
		<ColumnXmlSchema>
			<AllowEdit>true</AllowEdit>
			<CategoryId>5c1875ae-0f81-4249-a036-64f91b29b02d</CategoryId>
			<ColumnType>DateTime</ColumnType>
			<Name>RM_Template_Column_Name_DataClosed</Name>
			<OptionsJSON i:nil=""true"" />
			<OptionsMaxIdReachedValue>0</OptionsMaxIdReachedValue>
			<PushToFolderCategoryId>00000000-0000-0000-0000-000000000000</PushToFolderCategoryId>
			<PushToRecordCategoryId>00000000-0000-0000-0000-000000000000</PushToRecordCategoryId>
			<Required>false</Required>
			<ShowInEditForm>true</ShowInEditForm>
			<TemplateInheritSetting>0</TemplateInheritSetting>
			<UniqueId>99b6d3fb-688d-4d19-9cfb-2a3f70c07aa9</UniqueId>
		</ColumnXmlSchema>
		<ColumnXmlSchema>
			<AllowEdit>false</AllowEdit>
			<CategoryId>5c1875ae-0f81-4249-a036-64f91b29b02d</CategoryId>
			<ColumnType>Taxonomy</ColumnType>
			<Name>RM_Template_Column_Name_HomeLocation</Name>
			<OptionsJSON i:nil=""true"" />
			<OptionsMaxIdReachedValue>0</OptionsMaxIdReachedValue>
			<PushToFolderCategoryId>00000000-0000-0000-0000-000000000000</PushToFolderCategoryId>
			<PushToRecordCategoryId>00000000-0000-0000-0000-000000000000</PushToRecordCategoryId>
			<Required>false</Required>
			<ShowInEditForm>true</ShowInEditForm>
			<TemplateInheritSetting>0</TemplateInheritSetting>
			<UniqueId>d2568d7d-4891-46d2-8eb2-2e8c032a41bf</UniqueId>
		</ColumnXmlSchema>
	</Columns>
</TemplateColumnsSchema>";
        #endregion

        #region create record template default data
        public const string DEFAULT_DATA_RECORD_TEMPLATE_XML = @"<TemplateColumnsSchema xmlns:i=""http://www.w3.org/2001/XMLSchema-instance"" xmlns=""http://schemas.datacontract.org/2004/07/AvePoint.RA.Contract.TemplateManagement"">
	<Columns>
		<ColumnXmlSchema>
			<AllowEdit>false</AllowEdit>
			<CategoryId>5815d70c-1e9d-404f-89bb-933e365a057c</CategoryId>
			<ColumnType>SingleText</ColumnType>
			<Name>RM_Template_Column_Name_Title</Name>
			<OptionsJSON i:nil=""true"" />
			<OptionsMaxIdReachedValue>0</OptionsMaxIdReachedValue>
			<PushToFolderCategoryId>00000000-0000-0000-0000-000000000000</PushToFolderCategoryId>
			<PushToRecordCategoryId>00000000-0000-0000-0000-000000000000</PushToRecordCategoryId>
			<Required>true</Required>
			<ShowInEditForm>true</ShowInEditForm>
			<TemplateInheritSetting>0</TemplateInheritSetting>
			<UniqueId>de5e99cb-4fb4-4e25-b732-a1dce71dd048</UniqueId>
		</ColumnXmlSchema>
		<ColumnXmlSchema>
			<AllowEdit>true</AllowEdit>
			<CategoryId>5815d70c-1e9d-404f-89bb-933e365a057c</CategoryId>
			<ColumnType>MultipleText</ColumnType>
			<Name>RM_Template_Column_Name_Description</Name>
			<OptionsJSON i:nil=""true"" />
			<OptionsMaxIdReachedValue>0</OptionsMaxIdReachedValue>
			<PushToFolderCategoryId>00000000-0000-0000-0000-000000000000</PushToFolderCategoryId>
			<PushToRecordCategoryId>00000000-0000-0000-0000-000000000000</PushToRecordCategoryId>
			<Required>false</Required>
			<ShowInEditForm>true</ShowInEditForm>
			<TemplateInheritSetting>0</TemplateInheritSetting>
			<UniqueId>dd5b8a0a-96d7-42c0-ba1b-a6cbef4bdb81</UniqueId>
		</ColumnXmlSchema>
		<ColumnXmlSchema>
			<AllowEdit>true</AllowEdit>
			<CategoryId>a6fa9703-0cfa-43f0-953b-f22858cb5124</CategoryId>
			<ColumnType>SingleChoice</ColumnType>
			<Name>RM_Template_Column_Name_Format</Name>
			<OptionsJSON>{""1"":""RM_Template_Column_Value_Format_Document"",""2"":""RM_Template_Column_Value_Format_Cassette"",""3"":""RM_Template_Column_Value_Format_Map"",""4"":""RM_Template_Column_Value_Format_Play"",""5"":""RM_Template_Column_Value_Format_DVD""}</OptionsJSON>
			<OptionsMaxIdReachedValue>0</OptionsMaxIdReachedValue>
			<PushToFolderCategoryId>00000000-0000-0000-0000-000000000000</PushToFolderCategoryId>
			<PushToRecordCategoryId>00000000-0000-0000-0000-000000000000</PushToRecordCategoryId>
			<Required>true</Required>
			<ShowInEditForm>true</ShowInEditForm>
			<TemplateInheritSetting>0</TemplateInheritSetting>
			<UniqueId>9333da20-6a70-4a4e-9013-33ee8f0539cd</UniqueId>
		</ColumnXmlSchema>
		<ColumnXmlSchema>
			<AllowEdit>true</AllowEdit>
			<CategoryId>a6fa9703-0cfa-43f0-953b-f22858cb5124</CategoryId>
			<ColumnType>SingleChoice</ColumnType>
			<Name>RM_Template_Column_Name_ProtectiveMarking</Name>
			<OptionsJSON>{""1"":""RM_Template_Column_Value_ProtectiveMarking_InternalUsedOnly"",""2"":""RM_Template_Column_Value_ProtectiveMarking_Public"",""3"":""RM_Template_Column_Value_ProtectiveMarking_Confidential"",""4"":""RM_Template_Column_Value_ProtectiveMarking_HighlyConfidential""}</OptionsJSON>
			<OptionsMaxIdReachedValue>0</OptionsMaxIdReachedValue>
			<PushToFolderCategoryId>00000000-0000-0000-0000-000000000000</PushToFolderCategoryId>
			<PushToRecordCategoryId>00000000-0000-0000-0000-000000000000</PushToRecordCategoryId>
			<Required>true</Required>
			<ShowInEditForm>true</ShowInEditForm>
			<TemplateInheritSetting>0</TemplateInheritSetting>
			<UniqueId>01aefeb6-6cb1-419f-b476-fee650045778</UniqueId>
		</ColumnXmlSchema>
		<ColumnXmlSchema>
			<AllowEdit>true</AllowEdit>
			<CategoryId>9a10fb34-79df-4d45-9eb1-6df44b7a8d4c</CategoryId>
			<ColumnType>MultipleText</ColumnType>
			<Name>RM_Template_Column_Name_Rights</Name>
			<OptionsJSON i:nil=""true"" />
			<OptionsMaxIdReachedValue>0</OptionsMaxIdReachedValue>
			<PushToFolderCategoryId>00000000-0000-0000-0000-000000000000</PushToFolderCategoryId>
			<PushToRecordCategoryId>00000000-0000-0000-0000-000000000000</PushToRecordCategoryId>
			<Required>false</Required>
			<ShowInEditForm>true</ShowInEditForm>
			<TemplateInheritSetting>0</TemplateInheritSetting>
			<UniqueId>e56dcd2e-fbb7-4e31-bfff-5347d538f88e</UniqueId>
		</ColumnXmlSchema>
		<ColumnXmlSchema>
			<AllowEdit>true</AllowEdit>
			<CategoryId>9a10fb34-79df-4d45-9eb1-6df44b7a8d4c</CategoryId>
			<ColumnType>MultipleText</ColumnType>
			<Name>RM_Template_Column_Name_Coverage</Name>
			<OptionsJSON i:nil=""true"" />
			<OptionsMaxIdReachedValue>0</OptionsMaxIdReachedValue>
			<PushToFolderCategoryId>00000000-0000-0000-0000-000000000000</PushToFolderCategoryId>
			<PushToRecordCategoryId>00000000-0000-0000-0000-000000000000</PushToRecordCategoryId>
			<Required>false</Required>
			<ShowInEditForm>true</ShowInEditForm>
			<TemplateInheritSetting>0</TemplateInheritSetting>
			<UniqueId>fe3dce03-2e91-400a-8b6d-446b6ef8820e</UniqueId>
		</ColumnXmlSchema>
		<ColumnXmlSchema>
			<AllowEdit>false</AllowEdit>
			<CategoryId>9a10fb34-79df-4d45-9eb1-6df44b7a8d4c</CategoryId>
			<ColumnType>Taxonomy</ColumnType>
			<Name>RM_Template_Column_Name_HomeLocation</Name>
			<OptionsJSON i:nil=""true"" />
			<OptionsMaxIdReachedValue>0</OptionsMaxIdReachedValue>
			<PushToFolderCategoryId>00000000-0000-0000-0000-000000000000</PushToFolderCategoryId>
			<PushToRecordCategoryId>00000000-0000-0000-0000-000000000000</PushToRecordCategoryId>
			<Required>false</Required>
			<ShowInEditForm>true</ShowInEditForm>
			<TemplateInheritSetting>0</TemplateInheritSetting>
			<UniqueId>d2568d7d-4891-46d2-8eb2-2e8c032a41bf</UniqueId>
		</ColumnXmlSchema>
	</Columns>
</TemplateColumnsSchema>";
        #endregion

        #region create custom template default data
        public const string DEFAULT_DATA_CUSTOM_TEMPLATE_XML = @"
<TemplateColumnsSchema xmlns:i=""http://www.w3.org/2001/XMLSchema-instance"" xmlns=""http://schemas.datacontract.org/2004/07/AvePoint.RA.Contract.TemplateManagement"">
	<Columns>
		<ColumnXmlSchema>
			<AllowEdit>false</AllowEdit>
			<CategoryId>28CB3865-F492-47CD-8D0C-BD26E87ED5FC</CategoryId>
			<ColumnType>SingleText</ColumnType>
			<Name>RM_Template_Column_Name_Title</Name>
			<OptionsJSON i:nil=""true"" />
			<OptionsMaxIdReachedValue>0</OptionsMaxIdReachedValue>
			<PushToFolderCategoryId>00000000-0000-0000-0000-000000000000</PushToFolderCategoryId>
			<PushToRecordCategoryId>00000000-0000-0000-0000-000000000000</PushToRecordCategoryId>
			<Required>true</Required>
			<ShowInEditForm>true</ShowInEditForm>
			<TemplateInheritSetting>0</TemplateInheritSetting>
			<UniqueId>de5e99cb-4fb4-4e25-b732-a1dce71dd048</UniqueId>
		</ColumnXmlSchema>
		<ColumnXmlSchema>
			<AllowEdit>false</AllowEdit>
			<CategoryId>28CB3865-F492-47CD-8D0C-BD26E87ED5FC</CategoryId>
			<ColumnType>Taxonomy</ColumnType>
			<Name>RM_Template_Column_Name_Classification</Name>
			<OptionsJSON i:nil=""true"" />
			<OptionsMaxIdReachedValue>0</OptionsMaxIdReachedValue>
			<PushToFolderCategoryId>00000000-0000-0000-0000-000000000000</PushToFolderCategoryId>
			<PushToRecordCategoryId>00000000-0000-0000-0000-000000000000</PushToRecordCategoryId>
			<Required>false</Required>
			<ShowInEditForm>true</ShowInEditForm>
			<TemplateInheritSetting>0</TemplateInheritSetting>
			<UniqueId>aedcf21f-dfdb-41d3-935a-5c5859187754</UniqueId>
		</ColumnXmlSchema>
	</Columns>
</TemplateColumnsSchema>";
        #endregion
    }

}
