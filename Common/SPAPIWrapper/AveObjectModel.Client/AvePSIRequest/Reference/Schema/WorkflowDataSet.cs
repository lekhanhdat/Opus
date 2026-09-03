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
using System.CodeDom.Compiler;
using System.Collections;
using System.ComponentModel;
using System.ComponentModel.Design;
using System.Data;
using System.Diagnostics;
using System.IO;
using System.Runtime.Serialization;
using System.Xml;
using System.Xml.Schema;
using System.Xml.Serialization;

namespace Microsoft.Office.Project.Server.Schema
{
	// Token: 0x0200079D RID: 1949
	[HelpKeyword("vs.data.DataSet")]
	[DesignerCategory("code")]
	[ToolboxItem(true)]
	[XmlSchemaProvider("GetTypedDataSetSchema")]
	[XmlRoot("WorkflowDataSet")]
	[Serializable]
	public class WorkflowDataSet : DataSet
	{
		// Token: 0x0600BD00 RID: 48384 RVA: 0x0024D364 File Offset: 0x0024B564
		public new void EndInit()
		{
			base.EndInit();
			TypedDataSetUtilities.AllowNullsInNonTypedColumns(this.WorkflowAssociation, new string[]
			{
				"WORKFLOW_ASSOCIATION_NAME",
				"WORKFLOW_ASSOCIATION_DESCRIPTION",
				"WORKFLOW_ASSOCIATION_UID"
			});
			TypedDataSetUtilities.AllowNullsInNonTypedColumns(this.UpdateProjectWorkflows, new string[]
			{
				"ENTERPRISE_PROJECT_TYPE_UID",
				"JOB_UID",
				"PROJ_UID",
				"STAGE_UID",
				"SKIP_TO_CURRENT_STAGE"
			});
			TypedDataSetUtilities.AllowNullsInNonTypedColumns(this.EnterpriseProjectTypePDPs, new string[]
			{
				"ENTERPRISE_PROJECT_TYPE_UID",
				"PDP_ID",
				"PDP_NAME",
				"IS_CREATE_PDP",
				"PDP_POSITION",
				"PDP_UID"
			});
			TypedDataSetUtilities.AllowNullsInNonTypedColumns(this.WorkflowInstance, new string[]
			{
				"ENTERPRISE_PROJECT_TYPE_NAME",
				"ENTERPRISE_PROJECT_TYPE_UID",
				"WORKFLOW_INSTANCE_UID",
				"PROJ_UID",
				"WORKFLOW_ENGINE_VERSION"
			});
			TypedDataSetUtilities.AllowNullsInNonTypedColumns(this.WorkflowPhase, new string[]
			{
				"PHASE_DESCRIPTION",
				"PHASE_NAME",
				"PHASE_UID"
			});
			TypedDataSetUtilities.AllowNullsInNonTypedColumns(this.WorkflowStagePDPs, new string[]
			{
				"PDP_REQUIRES_ATTENTION",
				"PDP_ID",
				"PDP_STAGE_DESCRIPTION",
				"PDP_NAME",
				"STAGE_UID",
				"PDP_POSITION",
				"PDP_UID"
			});
			TypedDataSetUtilities.AllowNullsInNonTypedColumns(this.EnterpriseProjectTypeDepartments, new string[]
			{
				"ENTERPRISE_PROJECT_TYPE_UID",
				"DEPARTMENT_UID"
			});
			TypedDataSetUtilities.AllowNullsInNonTypedColumns(this.WorkflowStageStrategicImpact, new string[]
			{
				"BEHAVIOR",
				"STAGE_UID"
			});
			TypedDataSetUtilities.AllowNullsInNonTypedColumns(this.EnterpriseProjectType, new string[]
			{
				"ENTERPRISE_PROJECT_TYPE_NAME",
				"ENTERPRISE_PROJECT_TYPE_UID",
				"PROJ_IDENTIFIER_MINDIGIT",
				"PROJ_IDENTIFIER_SEED",
				"IS_MANAGED_PROJECT",
				"PROJ_IDENTIFIER_POSTFIX",
				"WORKFLOW_ASSOCIATION_NAME",
				"PROJ_IDENTIFIER_PREFIX",
				"ENTERPRISE_PROJECT_PLAN_TEMPLATE_UID",
				"ENTERPRISE_PROJECT_TYPE_IMAGE_URL",
				"IS_DEFAULT_PROJECT_TYPE",
				"ENTERPRISE_PROJECT_WORKSPACE_TEMPLATE_NAME",
				"ENTERPRISE_PROJECT_TYPE_ORDER",
				"WORKFLOW_ASSOCIATION_UID",
				"ENTERPRISE_PROJECT_TYPE_DESCRIPTION"
			});
			TypedDataSetUtilities.AllowNullsInNonTypedColumns(this.WorkflowStageCustomFields, new string[]
			{
				"REQUIRED",
				"MD_PROP_NAME",
				"MD_PROP_UID",
				"READ_ONLY",
				"STAGE_UID"
			});
			TypedDataSetUtilities.AllowNullsInNonTypedColumns(this.WorkflowStatus, new string[]
			{
				"STAGE_STATUS",
				"CREATED_DATE",
				"PHASE_UID",
				"STAGE_ENTRY_DATE",
				"WORKFLOW_INSTANCE_UID",
				"PHASE_NAME",
				"STAGE_ORDER",
				"SUBMITTED_DATE",
				"STAGE_UID",
				"NEXT_STAGE2",
				"STAGE_INFO",
				"STAGE_NAME",
				"MOD_DATE",
				"PROJ_UID",
				"STAGE_COMPLETION_DATE",
				"WORKFLOW_MOD_DATE",
				"NEXT_STAGE1"
			});
			TypedDataSetUtilities.AllowNullsInNonTypedColumns(this.WorkflowStage, new string[]
			{
				"STATUS_PDP_UID",
				"PHASE_NAME",
				"STAGE_SUBMIT_DESCRIPTION",
				"STAGE_UID",
				"PHASE_UID",
				"CHECKIN_REQUIRED",
				"STAGE_DESCRIPTION",
				"STAGE_NAME"
			});
		}

		// Token: 0x0600BD01 RID: 48385 RVA: 0x0024D728 File Offset: 0x0024B928
		[DebuggerNonUserCode]
		[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
		public WorkflowDataSet()
		{
			base.BeginInit();
			this.InitClass();
			CollectionChangeEventHandler value = new CollectionChangeEventHandler(this.SchemaChanged);
			base.Tables.CollectionChanged += value;
			base.Relations.CollectionChanged += value;
			this.EndInit();
		}

		// Token: 0x0600BD02 RID: 48386 RVA: 0x0024D77C File Offset: 0x0024B97C
		[DebuggerNonUserCode]
		[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
		protected WorkflowDataSet(SerializationInfo info, StreamingContext context) : base(info, context, false)
		{
			if (base.IsBinarySerialized(info, context))
			{
				this.InitVars(false);
				CollectionChangeEventHandler value = new CollectionChangeEventHandler(this.SchemaChanged);
				this.Tables.CollectionChanged += value;
				this.Relations.CollectionChanged += value;
				return;
			}
			string s = (string)info.GetValue("XmlSchema", typeof(string));
			if (base.DetermineSchemaSerializationMode(info, context) == SchemaSerializationMode.IncludeSchema)
			{
				DataSet dataSet = new DataSet();
				dataSet.ReadXmlSchema(new XmlTextReader(new StringReader(s)));
				if (dataSet.Tables["WorkflowPhase"] != null)
				{
					base.Tables.Add(new WorkflowDataSet.WorkflowPhaseDataTable(dataSet.Tables["WorkflowPhase"]));
				}
				if (dataSet.Tables["WorkflowStage"] != null)
				{
					base.Tables.Add(new WorkflowDataSet.WorkflowStageDataTable(dataSet.Tables["WorkflowStage"]));
				}
				if (dataSet.Tables["WorkflowStageCustomFields"] != null)
				{
					base.Tables.Add(new WorkflowDataSet.WorkflowStageCustomFieldsDataTable(dataSet.Tables["WorkflowStageCustomFields"]));
				}
				if (dataSet.Tables["WorkflowStageStrategicImpact"] != null)
				{
					base.Tables.Add(new WorkflowDataSet.WorkflowStageStrategicImpactDataTable(dataSet.Tables["WorkflowStageStrategicImpact"]));
				}
				if (dataSet.Tables["WorkflowStagePDPs"] != null)
				{
					base.Tables.Add(new WorkflowDataSet.WorkflowStagePDPsDataTable(dataSet.Tables["WorkflowStagePDPs"]));
				}
				if (dataSet.Tables["WorkflowInstance"] != null)
				{
					base.Tables.Add(new WorkflowDataSet.WorkflowInstanceDataTable(dataSet.Tables["WorkflowInstance"]));
				}
				if (dataSet.Tables["WorkflowAssociation"] != null)
				{
					base.Tables.Add(new WorkflowDataSet.WorkflowAssociationDataTable(dataSet.Tables["WorkflowAssociation"]));
				}
				if (dataSet.Tables["WorkflowStatus"] != null)
				{
					base.Tables.Add(new WorkflowDataSet.WorkflowStatusDataTable(dataSet.Tables["WorkflowStatus"]));
				}
				if (dataSet.Tables["EnterpriseProjectType"] != null)
				{
					base.Tables.Add(new WorkflowDataSet.EnterpriseProjectTypeDataTable(dataSet.Tables["EnterpriseProjectType"]));
				}
				if (dataSet.Tables["EnterpriseProjectTypeDepartments"] != null)
				{
					base.Tables.Add(new WorkflowDataSet.EnterpriseProjectTypeDepartmentsDataTable(dataSet.Tables["EnterpriseProjectTypeDepartments"]));
				}
				if (dataSet.Tables["EnterpriseProjectTypePDPs"] != null)
				{
					base.Tables.Add(new WorkflowDataSet.EnterpriseProjectTypePDPsDataTable(dataSet.Tables["EnterpriseProjectTypePDPs"]));
				}
				if (dataSet.Tables["UpdateProjectWorkflows"] != null)
				{
					base.Tables.Add(new WorkflowDataSet.UpdateProjectWorkflowsDataTable(dataSet.Tables["UpdateProjectWorkflows"]));
				}
				base.DataSetName = dataSet.DataSetName;
				base.Prefix = dataSet.Prefix;
				base.Namespace = dataSet.Namespace;
				base.Locale = dataSet.Locale;
				base.CaseSensitive = dataSet.CaseSensitive;
				base.EnforceConstraints = dataSet.EnforceConstraints;
				base.Merge(dataSet, false, MissingSchemaAction.Add);
				this.InitVars();
			}
			else
			{
				base.ReadXmlSchema(new XmlTextReader(new StringReader(s)));
			}
			base.GetSerializationData(info, context);
			CollectionChangeEventHandler value2 = new CollectionChangeEventHandler(this.SchemaChanged);
			base.Tables.CollectionChanged += value2;
			this.Relations.CollectionChanged += value2;
		}

		// Token: 0x170039E8 RID: 14824
		// (get) Token: 0x0600BD03 RID: 48387 RVA: 0x0024DAFF File Offset: 0x0024BCFF
		[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Content)]
		[Browsable(false)]
		[DebuggerNonUserCode]
		public WorkflowDataSet.WorkflowPhaseDataTable WorkflowPhase
		{
			get
			{
				return this.tableWorkflowPhase;
			}
		}

		// Token: 0x170039E9 RID: 14825
		// (get) Token: 0x0600BD04 RID: 48388 RVA: 0x0024DB07 File Offset: 0x0024BD07
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Content)]
		[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
		[Browsable(false)]
		[DebuggerNonUserCode]
		public WorkflowDataSet.WorkflowStageDataTable WorkflowStage
		{
			get
			{
				return this.tableWorkflowStage;
			}
		}

		// Token: 0x170039EA RID: 14826
		// (get) Token: 0x0600BD05 RID: 48389 RVA: 0x0024DB0F File Offset: 0x0024BD0F
		[Browsable(false)]
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Content)]
		[DebuggerNonUserCode]
		[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
		public WorkflowDataSet.WorkflowStageCustomFieldsDataTable WorkflowStageCustomFields
		{
			get
			{
				return this.tableWorkflowStageCustomFields;
			}
		}

		// Token: 0x170039EB RID: 14827
		// (get) Token: 0x0600BD06 RID: 48390 RVA: 0x0024DB17 File Offset: 0x0024BD17
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Content)]
		[DebuggerNonUserCode]
		[Browsable(false)]
		[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
		public WorkflowDataSet.WorkflowStageStrategicImpactDataTable WorkflowStageStrategicImpact
		{
			get
			{
				return this.tableWorkflowStageStrategicImpact;
			}
		}

		// Token: 0x170039EC RID: 14828
		// (get) Token: 0x0600BD07 RID: 48391 RVA: 0x0024DB1F File Offset: 0x0024BD1F
		[Browsable(false)]
		[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Content)]
		[DebuggerNonUserCode]
		public WorkflowDataSet.WorkflowStagePDPsDataTable WorkflowStagePDPs
		{
			get
			{
				return this.tableWorkflowStagePDPs;
			}
		}

		// Token: 0x170039ED RID: 14829
		// (get) Token: 0x0600BD08 RID: 48392 RVA: 0x0024DB27 File Offset: 0x0024BD27
		[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
		[Browsable(false)]
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Content)]
		[DebuggerNonUserCode]
		public WorkflowDataSet.WorkflowInstanceDataTable WorkflowInstance
		{
			get
			{
				return this.tableWorkflowInstance;
			}
		}

		// Token: 0x170039EE RID: 14830
		// (get) Token: 0x0600BD09 RID: 48393 RVA: 0x0024DB2F File Offset: 0x0024BD2F
		[DebuggerNonUserCode]
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Content)]
		[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
		[Browsable(false)]
		public WorkflowDataSet.WorkflowAssociationDataTable WorkflowAssociation
		{
			get
			{
				return this.tableWorkflowAssociation;
			}
		}

		// Token: 0x170039EF RID: 14831
		// (get) Token: 0x0600BD0A RID: 48394 RVA: 0x0024DB37 File Offset: 0x0024BD37
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Content)]
		[DebuggerNonUserCode]
		[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
		[Browsable(false)]
		public WorkflowDataSet.WorkflowStatusDataTable WorkflowStatus
		{
			get
			{
				return this.tableWorkflowStatus;
			}
		}

		// Token: 0x170039F0 RID: 14832
		// (get) Token: 0x0600BD0B RID: 48395 RVA: 0x0024DB3F File Offset: 0x0024BD3F
		[DebuggerNonUserCode]
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Content)]
		[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
		[Browsable(false)]
		public WorkflowDataSet.EnterpriseProjectTypeDataTable EnterpriseProjectType
		{
			get
			{
				return this.tableEnterpriseProjectType;
			}
		}

		// Token: 0x170039F1 RID: 14833
		// (get) Token: 0x0600BD0C RID: 48396 RVA: 0x0024DB47 File Offset: 0x0024BD47
		[DebuggerNonUserCode]
		[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
		[Browsable(false)]
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Content)]
		public WorkflowDataSet.EnterpriseProjectTypeDepartmentsDataTable EnterpriseProjectTypeDepartments
		{
			get
			{
				return this.tableEnterpriseProjectTypeDepartments;
			}
		}

		// Token: 0x170039F2 RID: 14834
		// (get) Token: 0x0600BD0D RID: 48397 RVA: 0x0024DB4F File Offset: 0x0024BD4F
		[DebuggerNonUserCode]
		[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
		[Browsable(false)]
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Content)]
		public WorkflowDataSet.EnterpriseProjectTypePDPsDataTable EnterpriseProjectTypePDPs
		{
			get
			{
				return this.tableEnterpriseProjectTypePDPs;
			}
		}

		// Token: 0x170039F3 RID: 14835
		// (get) Token: 0x0600BD0E RID: 48398 RVA: 0x0024DB57 File Offset: 0x0024BD57
		[DebuggerNonUserCode]
		[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
		[Browsable(false)]
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Content)]
		public WorkflowDataSet.UpdateProjectWorkflowsDataTable UpdateProjectWorkflows
		{
			get
			{
				return this.tableUpdateProjectWorkflows;
			}
		}

		// Token: 0x170039F4 RID: 14836
		// (get) Token: 0x0600BD0F RID: 48399 RVA: 0x0024DB5F File Offset: 0x0024BD5F
		// (set) Token: 0x0600BD10 RID: 48400 RVA: 0x0024DB67 File Offset: 0x0024BD67
		[DebuggerNonUserCode]
		[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
		[Browsable(true)]
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Visible)]
		public override SchemaSerializationMode SchemaSerializationMode
		{
			get
			{
				return this._schemaSerializationMode;
			}
			set
			{
				this._schemaSerializationMode = value;
			}
		}

		// Token: 0x170039F5 RID: 14837
		// (get) Token: 0x0600BD11 RID: 48401 RVA: 0x0024DB70 File Offset: 0x0024BD70
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
		[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
		[DebuggerNonUserCode]
		public new DataTableCollection Tables
		{
			get
			{
				return base.Tables;
			}
		}

		// Token: 0x170039F6 RID: 14838
		// (get) Token: 0x0600BD12 RID: 48402 RVA: 0x0024DB78 File Offset: 0x0024BD78
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
		[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
		[DebuggerNonUserCode]
		public new DataRelationCollection Relations
		{
			get
			{
				return base.Relations;
			}
		}

		// Token: 0x0600BD13 RID: 48403 RVA: 0x0024DB80 File Offset: 0x0024BD80
		[DebuggerNonUserCode]
		[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
		protected override void InitializeDerivedDataSet()
		{
			base.BeginInit();
			this.InitClass();
			this.EndInit();
		}

		// Token: 0x0600BD14 RID: 48404 RVA: 0x0024DB94 File Offset: 0x0024BD94
		[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
		[DebuggerNonUserCode]
		public override DataSet Clone()
		{
			WorkflowDataSet workflowDataSet = (WorkflowDataSet)base.Clone();
			workflowDataSet.InitVars();
			workflowDataSet.SchemaSerializationMode = this.SchemaSerializationMode;
			return workflowDataSet;
		}

		// Token: 0x0600BD15 RID: 48405 RVA: 0x0024DBC0 File Offset: 0x0024BDC0
		[DebuggerNonUserCode]
		[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
		protected override bool ShouldSerializeTables()
		{
			return false;
		}

		// Token: 0x0600BD16 RID: 48406 RVA: 0x0024DBC3 File Offset: 0x0024BDC3
		[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
		[DebuggerNonUserCode]
		protected override bool ShouldSerializeRelations()
		{
			return false;
		}

		// Token: 0x0600BD17 RID: 48407 RVA: 0x0024DBC8 File Offset: 0x0024BDC8
		[DebuggerNonUserCode]
		[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
		protected override void ReadXmlSerializable(XmlReader reader)
		{
			if (base.DetermineSchemaSerializationMode(reader) == SchemaSerializationMode.IncludeSchema)
			{
				this.Reset();
				DataSet dataSet = new DataSet();
				dataSet.ReadXml(reader);
				if (dataSet.Tables["WorkflowPhase"] != null)
				{
					base.Tables.Add(new WorkflowDataSet.WorkflowPhaseDataTable(dataSet.Tables["WorkflowPhase"]));
				}
				if (dataSet.Tables["WorkflowStage"] != null)
				{
					base.Tables.Add(new WorkflowDataSet.WorkflowStageDataTable(dataSet.Tables["WorkflowStage"]));
				}
				if (dataSet.Tables["WorkflowStageCustomFields"] != null)
				{
					base.Tables.Add(new WorkflowDataSet.WorkflowStageCustomFieldsDataTable(dataSet.Tables["WorkflowStageCustomFields"]));
				}
				if (dataSet.Tables["WorkflowStageStrategicImpact"] != null)
				{
					base.Tables.Add(new WorkflowDataSet.WorkflowStageStrategicImpactDataTable(dataSet.Tables["WorkflowStageStrategicImpact"]));
				}
				if (dataSet.Tables["WorkflowStagePDPs"] != null)
				{
					base.Tables.Add(new WorkflowDataSet.WorkflowStagePDPsDataTable(dataSet.Tables["WorkflowStagePDPs"]));
				}
				if (dataSet.Tables["WorkflowInstance"] != null)
				{
					base.Tables.Add(new WorkflowDataSet.WorkflowInstanceDataTable(dataSet.Tables["WorkflowInstance"]));
				}
				if (dataSet.Tables["WorkflowAssociation"] != null)
				{
					base.Tables.Add(new WorkflowDataSet.WorkflowAssociationDataTable(dataSet.Tables["WorkflowAssociation"]));
				}
				if (dataSet.Tables["WorkflowStatus"] != null)
				{
					base.Tables.Add(new WorkflowDataSet.WorkflowStatusDataTable(dataSet.Tables["WorkflowStatus"]));
				}
				if (dataSet.Tables["EnterpriseProjectType"] != null)
				{
					base.Tables.Add(new WorkflowDataSet.EnterpriseProjectTypeDataTable(dataSet.Tables["EnterpriseProjectType"]));
				}
				if (dataSet.Tables["EnterpriseProjectTypeDepartments"] != null)
				{
					base.Tables.Add(new WorkflowDataSet.EnterpriseProjectTypeDepartmentsDataTable(dataSet.Tables["EnterpriseProjectTypeDepartments"]));
				}
				if (dataSet.Tables["EnterpriseProjectTypePDPs"] != null)
				{
					base.Tables.Add(new WorkflowDataSet.EnterpriseProjectTypePDPsDataTable(dataSet.Tables["EnterpriseProjectTypePDPs"]));
				}
				if (dataSet.Tables["UpdateProjectWorkflows"] != null)
				{
					base.Tables.Add(new WorkflowDataSet.UpdateProjectWorkflowsDataTable(dataSet.Tables["UpdateProjectWorkflows"]));
				}
				base.DataSetName = dataSet.DataSetName;
				base.Prefix = dataSet.Prefix;
				base.Namespace = dataSet.Namespace;
				base.Locale = dataSet.Locale;
				base.CaseSensitive = dataSet.CaseSensitive;
				base.EnforceConstraints = dataSet.EnforceConstraints;
				base.Merge(dataSet, false, MissingSchemaAction.Add);
				this.InitVars();
				return;
			}
			base.ReadXml(reader);
			this.InitVars();
		}

		// Token: 0x0600BD18 RID: 48408 RVA: 0x0024DEB4 File Offset: 0x0024C0B4
		[DebuggerNonUserCode]
		[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
		protected override XmlSchema GetSchemaSerializable()
		{
			MemoryStream memoryStream = new MemoryStream();
			base.WriteXmlSchema(new XmlTextWriter(memoryStream, null));
			memoryStream.Position = 0L;
			return XmlSchema.Read(new XmlTextReader(memoryStream), null);
		}

		// Token: 0x0600BD19 RID: 48409 RVA: 0x0024DEE8 File Offset: 0x0024C0E8
		[DebuggerNonUserCode]
		[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
		internal void InitVars()
		{
			this.InitVars(true);
		}

		// Token: 0x0600BD1A RID: 48410 RVA: 0x0024DEF4 File Offset: 0x0024C0F4
		[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
		[DebuggerNonUserCode]
		internal void InitVars(bool initTable)
		{
			this.tableWorkflowPhase = (WorkflowDataSet.WorkflowPhaseDataTable)base.Tables["WorkflowPhase"];
			if (initTable && this.tableWorkflowPhase != null)
			{
				this.tableWorkflowPhase.InitVars();
			}
			this.tableWorkflowStage = (WorkflowDataSet.WorkflowStageDataTable)base.Tables["WorkflowStage"];
			if (initTable && this.tableWorkflowStage != null)
			{
				this.tableWorkflowStage.InitVars();
			}
			this.tableWorkflowStageCustomFields = (WorkflowDataSet.WorkflowStageCustomFieldsDataTable)base.Tables["WorkflowStageCustomFields"];
			if (initTable && this.tableWorkflowStageCustomFields != null)
			{
				this.tableWorkflowStageCustomFields.InitVars();
			}
			this.tableWorkflowStageStrategicImpact = (WorkflowDataSet.WorkflowStageStrategicImpactDataTable)base.Tables["WorkflowStageStrategicImpact"];
			if (initTable && this.tableWorkflowStageStrategicImpact != null)
			{
				this.tableWorkflowStageStrategicImpact.InitVars();
			}
			this.tableWorkflowStagePDPs = (WorkflowDataSet.WorkflowStagePDPsDataTable)base.Tables["WorkflowStagePDPs"];
			if (initTable && this.tableWorkflowStagePDPs != null)
			{
				this.tableWorkflowStagePDPs.InitVars();
			}
			this.tableWorkflowInstance = (WorkflowDataSet.WorkflowInstanceDataTable)base.Tables["WorkflowInstance"];
			if (initTable && this.tableWorkflowInstance != null)
			{
				this.tableWorkflowInstance.InitVars();
			}
			this.tableWorkflowAssociation = (WorkflowDataSet.WorkflowAssociationDataTable)base.Tables["WorkflowAssociation"];
			if (initTable && this.tableWorkflowAssociation != null)
			{
				this.tableWorkflowAssociation.InitVars();
			}
			this.tableWorkflowStatus = (WorkflowDataSet.WorkflowStatusDataTable)base.Tables["WorkflowStatus"];
			if (initTable && this.tableWorkflowStatus != null)
			{
				this.tableWorkflowStatus.InitVars();
			}
			this.tableEnterpriseProjectType = (WorkflowDataSet.EnterpriseProjectTypeDataTable)base.Tables["EnterpriseProjectType"];
			if (initTable && this.tableEnterpriseProjectType != null)
			{
				this.tableEnterpriseProjectType.InitVars();
			}
			this.tableEnterpriseProjectTypeDepartments = (WorkflowDataSet.EnterpriseProjectTypeDepartmentsDataTable)base.Tables["EnterpriseProjectTypeDepartments"];
			if (initTable && this.tableEnterpriseProjectTypeDepartments != null)
			{
				this.tableEnterpriseProjectTypeDepartments.InitVars();
			}
			this.tableEnterpriseProjectTypePDPs = (WorkflowDataSet.EnterpriseProjectTypePDPsDataTable)base.Tables["EnterpriseProjectTypePDPs"];
			if (initTable && this.tableEnterpriseProjectTypePDPs != null)
			{
				this.tableEnterpriseProjectTypePDPs.InitVars();
			}
			this.tableUpdateProjectWorkflows = (WorkflowDataSet.UpdateProjectWorkflowsDataTable)base.Tables["UpdateProjectWorkflows"];
			if (initTable && this.tableUpdateProjectWorkflows != null)
			{
				this.tableUpdateProjectWorkflows.InitVars();
			}
			this.relationFK_WorkflowPhase_WorkflowStage = this.Relations["FK_WorkflowPhase_WorkflowStage"];
			this.relationFK_WorkflowStage_WorkflowStageCustomFields = this.Relations["FK_WorkflowStage_WorkflowStageCustomFields"];
			this.relationFK_WorkflowStage_WorkflowStageStrategicImpact = this.Relations["FK_WorkflowStage_WorkflowStageStrategicImpact"];
			this.relationFK_WorkflowStage_WorkflowStageEDPs = this.Relations["FK_WorkflowStage_WorkflowStageEDPs"];
			this.relationEnterpriseProjectType_EnterpriseProjectTypeDepartments = this.Relations["EnterpriseProjectType_EnterpriseProjectTypeDepartments"];
			this.relationFK_EnterpriseProjectType_EnterpriseProjectTypePDPs = this.Relations["FK_EnterpriseProjectType_EnterpriseProjectTypePDPs"];
		}

		// Token: 0x0600BD1B RID: 48411 RVA: 0x0024E1D4 File Offset: 0x0024C3D4
		[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
		[DebuggerNonUserCode]
		private void InitClass()
		{
			base.DataSetName = "WorkflowDataSet";
			base.Prefix = "";
			base.Namespace = "http://schemas.microsoft.com/office/project/server/webservices/WorkflowDataSet/";
			base.EnforceConstraints = true;
			this.SchemaSerializationMode = SchemaSerializationMode.IncludeSchema;
			this.tableWorkflowPhase = new WorkflowDataSet.WorkflowPhaseDataTable();
			base.Tables.Add(this.tableWorkflowPhase);
			this.tableWorkflowStage = new WorkflowDataSet.WorkflowStageDataTable();
			base.Tables.Add(this.tableWorkflowStage);
			this.tableWorkflowStageCustomFields = new WorkflowDataSet.WorkflowStageCustomFieldsDataTable();
			base.Tables.Add(this.tableWorkflowStageCustomFields);
			this.tableWorkflowStageStrategicImpact = new WorkflowDataSet.WorkflowStageStrategicImpactDataTable();
			base.Tables.Add(this.tableWorkflowStageStrategicImpact);
			this.tableWorkflowStagePDPs = new WorkflowDataSet.WorkflowStagePDPsDataTable();
			base.Tables.Add(this.tableWorkflowStagePDPs);
			this.tableWorkflowInstance = new WorkflowDataSet.WorkflowInstanceDataTable();
			base.Tables.Add(this.tableWorkflowInstance);
			this.tableWorkflowAssociation = new WorkflowDataSet.WorkflowAssociationDataTable();
			base.Tables.Add(this.tableWorkflowAssociation);
			this.tableWorkflowStatus = new WorkflowDataSet.WorkflowStatusDataTable();
			base.Tables.Add(this.tableWorkflowStatus);
			this.tableEnterpriseProjectType = new WorkflowDataSet.EnterpriseProjectTypeDataTable();
			base.Tables.Add(this.tableEnterpriseProjectType);
			this.tableEnterpriseProjectTypeDepartments = new WorkflowDataSet.EnterpriseProjectTypeDepartmentsDataTable();
			base.Tables.Add(this.tableEnterpriseProjectTypeDepartments);
			this.tableEnterpriseProjectTypePDPs = new WorkflowDataSet.EnterpriseProjectTypePDPsDataTable();
			base.Tables.Add(this.tableEnterpriseProjectTypePDPs);
			this.tableUpdateProjectWorkflows = new WorkflowDataSet.UpdateProjectWorkflowsDataTable();
			base.Tables.Add(this.tableUpdateProjectWorkflows);
			ForeignKeyConstraint foreignKeyConstraint = new ForeignKeyConstraint("FK_WorkflowPhase_WorkflowStage", new DataColumn[]
			{
				this.tableWorkflowPhase.PHASE_UIDColumn
			}, new DataColumn[]
			{
				this.tableWorkflowStage.PHASE_UIDColumn
			});
			this.tableWorkflowStage.Constraints.Add(foreignKeyConstraint);
			foreignKeyConstraint.AcceptRejectRule = AcceptRejectRule.None;
			foreignKeyConstraint.DeleteRule = Rule.None;
			foreignKeyConstraint.UpdateRule = Rule.None;
			foreignKeyConstraint = new ForeignKeyConstraint("FK_WorkflowStage_WorkflowStageCustomFields", new DataColumn[]
			{
				this.tableWorkflowStage.STAGE_UIDColumn
			}, new DataColumn[]
			{
				this.tableWorkflowStageCustomFields.STAGE_UIDColumn
			});
			this.tableWorkflowStageCustomFields.Constraints.Add(foreignKeyConstraint);
			foreignKeyConstraint.AcceptRejectRule = AcceptRejectRule.None;
			foreignKeyConstraint.DeleteRule = Rule.None;
			foreignKeyConstraint.UpdateRule = Rule.None;
			foreignKeyConstraint = new ForeignKeyConstraint("FK_WorkflowStage_WorkflowStageStrategicImpact", new DataColumn[]
			{
				this.tableWorkflowStage.STAGE_UIDColumn
			}, new DataColumn[]
			{
				this.tableWorkflowStageStrategicImpact.STAGE_UIDColumn
			});
			this.tableWorkflowStageStrategicImpact.Constraints.Add(foreignKeyConstraint);
			foreignKeyConstraint.AcceptRejectRule = AcceptRejectRule.None;
			foreignKeyConstraint.DeleteRule = Rule.None;
			foreignKeyConstraint.UpdateRule = Rule.None;
			foreignKeyConstraint = new ForeignKeyConstraint("FK_WorkflowStage_WorkflowStageEDPs", new DataColumn[]
			{
				this.tableWorkflowStage.STAGE_UIDColumn
			}, new DataColumn[]
			{
				this.tableWorkflowStagePDPs.STAGE_UIDColumn
			});
			this.tableWorkflowStagePDPs.Constraints.Add(foreignKeyConstraint);
			foreignKeyConstraint.AcceptRejectRule = AcceptRejectRule.None;
			foreignKeyConstraint.DeleteRule = Rule.Cascade;
			foreignKeyConstraint.UpdateRule = Rule.Cascade;
			foreignKeyConstraint = new ForeignKeyConstraint("EnterpriseProjectType_EnterpriseProjectTypeDepartments", new DataColumn[]
			{
				this.tableEnterpriseProjectType.ENTERPRISE_PROJECT_TYPE_UIDColumn
			}, new DataColumn[]
			{
				this.tableEnterpriseProjectTypeDepartments.ENTERPRISE_PROJECT_TYPE_UIDColumn
			});
			this.tableEnterpriseProjectTypeDepartments.Constraints.Add(foreignKeyConstraint);
			foreignKeyConstraint.AcceptRejectRule = AcceptRejectRule.None;
			foreignKeyConstraint.DeleteRule = Rule.None;
			foreignKeyConstraint.UpdateRule = Rule.None;
			foreignKeyConstraint = new ForeignKeyConstraint("FK_EnterpriseProjectType_EnterpriseProjectTypePDPs", new DataColumn[]
			{
				this.tableEnterpriseProjectType.ENTERPRISE_PROJECT_TYPE_UIDColumn
			}, new DataColumn[]
			{
				this.tableEnterpriseProjectTypePDPs.ENTERPRISE_PROJECT_TYPE_UIDColumn
			});
			this.tableEnterpriseProjectTypePDPs.Constraints.Add(foreignKeyConstraint);
			foreignKeyConstraint.AcceptRejectRule = AcceptRejectRule.None;
			foreignKeyConstraint.DeleteRule = Rule.None;
			foreignKeyConstraint.UpdateRule = Rule.None;
			this.relationFK_WorkflowPhase_WorkflowStage = new DataRelation("FK_WorkflowPhase_WorkflowStage", new DataColumn[]
			{
				this.tableWorkflowPhase.PHASE_UIDColumn
			}, new DataColumn[]
			{
				this.tableWorkflowStage.PHASE_UIDColumn
			}, false);
			this.Relations.Add(this.relationFK_WorkflowPhase_WorkflowStage);
			this.relationFK_WorkflowStage_WorkflowStageCustomFields = new DataRelation("FK_WorkflowStage_WorkflowStageCustomFields", new DataColumn[]
			{
				this.tableWorkflowStage.STAGE_UIDColumn
			}, new DataColumn[]
			{
				this.tableWorkflowStageCustomFields.STAGE_UIDColumn
			}, false);
			this.Relations.Add(this.relationFK_WorkflowStage_WorkflowStageCustomFields);
			this.relationFK_WorkflowStage_WorkflowStageStrategicImpact = new DataRelation("FK_WorkflowStage_WorkflowStageStrategicImpact", new DataColumn[]
			{
				this.tableWorkflowStage.STAGE_UIDColumn
			}, new DataColumn[]
			{
				this.tableWorkflowStageStrategicImpact.STAGE_UIDColumn
			}, false);
			this.Relations.Add(this.relationFK_WorkflowStage_WorkflowStageStrategicImpact);
			this.relationFK_WorkflowStage_WorkflowStageEDPs = new DataRelation("FK_WorkflowStage_WorkflowStageEDPs", new DataColumn[]
			{
				this.tableWorkflowStage.STAGE_UIDColumn
			}, new DataColumn[]
			{
				this.tableWorkflowStagePDPs.STAGE_UIDColumn
			}, false);
			this.Relations.Add(this.relationFK_WorkflowStage_WorkflowStageEDPs);
			this.relationEnterpriseProjectType_EnterpriseProjectTypeDepartments = new DataRelation("EnterpriseProjectType_EnterpriseProjectTypeDepartments", new DataColumn[]
			{
				this.tableEnterpriseProjectType.ENTERPRISE_PROJECT_TYPE_UIDColumn
			}, new DataColumn[]
			{
				this.tableEnterpriseProjectTypeDepartments.ENTERPRISE_PROJECT_TYPE_UIDColumn
			}, false);
			this.Relations.Add(this.relationEnterpriseProjectType_EnterpriseProjectTypeDepartments);
			this.relationFK_EnterpriseProjectType_EnterpriseProjectTypePDPs = new DataRelation("FK_EnterpriseProjectType_EnterpriseProjectTypePDPs", new DataColumn[]
			{
				this.tableEnterpriseProjectType.ENTERPRISE_PROJECT_TYPE_UIDColumn
			}, new DataColumn[]
			{
				this.tableEnterpriseProjectTypePDPs.ENTERPRISE_PROJECT_TYPE_UIDColumn
			}, false);
			this.Relations.Add(this.relationFK_EnterpriseProjectType_EnterpriseProjectTypePDPs);
		}

		// Token: 0x0600BD28 RID: 48424 RVA: 0x0024E7C5 File Offset: 0x0024C9C5
		[DebuggerNonUserCode]
		[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
		private void SchemaChanged(object sender, CollectionChangeEventArgs e)
		{
			if (e.Action == CollectionChangeAction.Remove)
			{
				this.InitVars();
			}
		}

		// Token: 0x0600BD29 RID: 48425 RVA: 0x0024E7D8 File Offset: 0x0024C9D8
		/*[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
		[DebuggerNonUserCode]
		public static XmlSchemaComplexType GetTypedDataSetSchema(XmlSchemaSet xs)
		{
			WorkflowDataSet workflowDataSet = new WorkflowDataSet();
			XmlSchemaComplexType xmlSchemaComplexType = new XmlSchemaComplexType();
			XmlSchemaSequence xmlSchemaSequence = new XmlSchemaSequence();
			XmlSchemaAny xmlSchemaAny = new XmlSchemaAny();
			xmlSchemaAny.Namespace = workflowDataSet.Namespace;
			xmlSchemaSequence.Items.Add(xmlSchemaAny);
			xmlSchemaComplexType.Particle = xmlSchemaSequence;
			XmlSchema schemaSerializable = workflowDataSet.GetSchemaSerializable();
			if (xs.Contains(schemaSerializable.TargetNamespace))
			{
				MemoryStream memoryStream = new MemoryStream();
				MemoryStream memoryStream2 = new MemoryStream();
				try
				{
					schemaSerializable.Write(memoryStream);
					foreach (object obj in xs.Schemas(schemaSerializable.TargetNamespace))
					{
						XmlSchema xmlSchema = (XmlSchema)obj;
						memoryStream2.SetLength(0L);
						xmlSchema.Write(memoryStream2);
						if (memoryStream.Length == memoryStream2.Length)
						{
							memoryStream.Position = 0L;
							memoryStream2.Position = 0L;
							while (memoryStream.Position != memoryStream.Length && memoryStream.ReadByte() == memoryStream2.ReadByte())
							{
							}
							if (memoryStream.Position == memoryStream.Length)
							{
								return xmlSchemaComplexType;
							}
						}
					}
				}
				finally
				{
					if (memoryStream != null)
					{
						memoryStream.Close();
					}
					if (memoryStream2 != null)
					{
						memoryStream2.Close();
					}
				}
			}
			xs.Add(schemaSerializable);
			return xmlSchemaComplexType;
		}*/

		// Token: 0x04002626 RID: 9766
		private WorkflowDataSet.WorkflowPhaseDataTable tableWorkflowPhase;

		// Token: 0x04002627 RID: 9767
		private WorkflowDataSet.WorkflowStageDataTable tableWorkflowStage;

		// Token: 0x04002628 RID: 9768
		private WorkflowDataSet.WorkflowStageCustomFieldsDataTable tableWorkflowStageCustomFields;

		// Token: 0x04002629 RID: 9769
		private WorkflowDataSet.WorkflowStageStrategicImpactDataTable tableWorkflowStageStrategicImpact;

		// Token: 0x0400262A RID: 9770
		private WorkflowDataSet.WorkflowStagePDPsDataTable tableWorkflowStagePDPs;

		// Token: 0x0400262B RID: 9771
		private WorkflowDataSet.WorkflowInstanceDataTable tableWorkflowInstance;

		// Token: 0x0400262C RID: 9772
		private WorkflowDataSet.WorkflowAssociationDataTable tableWorkflowAssociation;

		// Token: 0x0400262D RID: 9773
		private WorkflowDataSet.WorkflowStatusDataTable tableWorkflowStatus;

		// Token: 0x0400262E RID: 9774
		private WorkflowDataSet.EnterpriseProjectTypeDataTable tableEnterpriseProjectType;

		// Token: 0x0400262F RID: 9775
		private WorkflowDataSet.EnterpriseProjectTypeDepartmentsDataTable tableEnterpriseProjectTypeDepartments;

		// Token: 0x04002630 RID: 9776
		private WorkflowDataSet.EnterpriseProjectTypePDPsDataTable tableEnterpriseProjectTypePDPs;

		// Token: 0x04002631 RID: 9777
		private WorkflowDataSet.UpdateProjectWorkflowsDataTable tableUpdateProjectWorkflows;

		// Token: 0x04002632 RID: 9778
		private DataRelation relationFK_WorkflowPhase_WorkflowStage;

		// Token: 0x04002633 RID: 9779
		private DataRelation relationFK_WorkflowStage_WorkflowStageCustomFields;

		// Token: 0x04002634 RID: 9780
		private DataRelation relationFK_WorkflowStage_WorkflowStageStrategicImpact;

		// Token: 0x04002635 RID: 9781
		private DataRelation relationFK_WorkflowStage_WorkflowStageEDPs;

		// Token: 0x04002636 RID: 9782
		private DataRelation relationEnterpriseProjectType_EnterpriseProjectTypeDepartments;

		// Token: 0x04002637 RID: 9783
		private DataRelation relationFK_EnterpriseProjectType_EnterpriseProjectTypePDPs;

		// Token: 0x04002638 RID: 9784
		private SchemaSerializationMode _schemaSerializationMode = SchemaSerializationMode.IncludeSchema;

		// Token: 0x0200079E RID: 1950
		// (Invoke) Token: 0x0600BD2B RID: 48427
		[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
		public delegate void WorkflowPhaseRowChangeEventHandler(object sender, WorkflowDataSet.WorkflowPhaseRowChangeEvent e);

		// Token: 0x0200079F RID: 1951
		// (Invoke) Token: 0x0600BD2F RID: 48431
		[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
		public delegate void WorkflowStageRowChangeEventHandler(object sender, WorkflowDataSet.WorkflowStageRowChangeEvent e);

		// Token: 0x020007A0 RID: 1952
		// (Invoke) Token: 0x0600BD33 RID: 48435
		[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
		public delegate void WorkflowStageCustomFieldsRowChangeEventHandler(object sender, WorkflowDataSet.WorkflowStageCustomFieldsRowChangeEvent e);

		// Token: 0x020007A1 RID: 1953
		// (Invoke) Token: 0x0600BD37 RID: 48439
		[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
		public delegate void WorkflowStageStrategicImpactRowChangeEventHandler(object sender, WorkflowDataSet.WorkflowStageStrategicImpactRowChangeEvent e);

		// Token: 0x020007A2 RID: 1954
		// (Invoke) Token: 0x0600BD3B RID: 48443
		[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
		public delegate void WorkflowStagePDPsRowChangeEventHandler(object sender, WorkflowDataSet.WorkflowStagePDPsRowChangeEvent e);

		// Token: 0x020007A3 RID: 1955
		// (Invoke) Token: 0x0600BD3F RID: 48447
		[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
		public delegate void WorkflowInstanceRowChangeEventHandler(object sender, WorkflowDataSet.WorkflowInstanceRowChangeEvent e);

		// Token: 0x020007A4 RID: 1956
		// (Invoke) Token: 0x0600BD43 RID: 48451
		[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
		public delegate void WorkflowAssociationRowChangeEventHandler(object sender, WorkflowDataSet.WorkflowAssociationRowChangeEvent e);

		// Token: 0x020007A5 RID: 1957
		// (Invoke) Token: 0x0600BD47 RID: 48455
		[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
		public delegate void WorkflowStatusRowChangeEventHandler(object sender, WorkflowDataSet.WorkflowStatusRowChangeEvent e);

		// Token: 0x020007A6 RID: 1958
		// (Invoke) Token: 0x0600BD4B RID: 48459
		[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
		public delegate void EnterpriseProjectTypeRowChangeEventHandler(object sender, WorkflowDataSet.EnterpriseProjectTypeRowChangeEvent e);

		// Token: 0x020007A7 RID: 1959
		// (Invoke) Token: 0x0600BD4F RID: 48463
		[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
		public delegate void EnterpriseProjectTypeDepartmentsRowChangeEventHandler(object sender, WorkflowDataSet.EnterpriseProjectTypeDepartmentsRowChangeEvent e);

		// Token: 0x020007A8 RID: 1960
		// (Invoke) Token: 0x0600BD53 RID: 48467
		[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
		public delegate void EnterpriseProjectTypePDPsRowChangeEventHandler(object sender, WorkflowDataSet.EnterpriseProjectTypePDPsRowChangeEvent e);

		// Token: 0x020007A9 RID: 1961
		// (Invoke) Token: 0x0600BD57 RID: 48471
		[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
		public delegate void UpdateProjectWorkflowsRowChangeEventHandler(object sender, WorkflowDataSet.UpdateProjectWorkflowsRowChangeEvent e);

		// Token: 0x020007AA RID: 1962
		[XmlSchemaProvider("GetTypedTableSchema")]
		[Serializable]
		public class WorkflowPhaseDataTable : DataTable, IEnumerable
		{
			// Token: 0x0600BD5A RID: 48474 RVA: 0x0024E920 File Offset: 0x0024CB20
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public WorkflowPhaseDataTable()
			{
				base.TableName = "WorkflowPhase";
				this.BeginInit();
				this.InitClass();
				this.EndInit();
			}

			// Token: 0x0600BD5B RID: 48475 RVA: 0x0024E948 File Offset: 0x0024CB48
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			internal WorkflowPhaseDataTable(DataTable table)
			{
				base.TableName = table.TableName;
				if (table.CaseSensitive != table.DataSet.CaseSensitive)
				{
					base.CaseSensitive = table.CaseSensitive;
				}
				if (table.Locale.ToString() != table.DataSet.Locale.ToString())
				{
					base.Locale = table.Locale;
				}
				if (table.Namespace != table.DataSet.Namespace)
				{
					base.Namespace = table.Namespace;
				}
				base.Prefix = table.Prefix;
				base.MinimumCapacity = table.MinimumCapacity;
			}

			// Token: 0x0600BD5C RID: 48476 RVA: 0x0024E9F0 File Offset: 0x0024CBF0
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			protected WorkflowPhaseDataTable(SerializationInfo info, StreamingContext context) : base(info, context)
			{
				this.InitVars();
			}

			// Token: 0x170039F7 RID: 14839
			// (get) Token: 0x0600BD5D RID: 48477 RVA: 0x0024EA00 File Offset: 0x0024CC00
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public DataColumn PHASE_UIDColumn
			{
				get
				{
					return this.columnPHASE_UID;
				}
			}

			// Token: 0x170039F8 RID: 14840
			// (get) Token: 0x0600BD5E RID: 48478 RVA: 0x0024EA08 File Offset: 0x0024CC08
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public DataColumn PHASE_NAMEColumn
			{
				get
				{
					return this.columnPHASE_NAME;
				}
			}

			// Token: 0x170039F9 RID: 14841
			// (get) Token: 0x0600BD5F RID: 48479 RVA: 0x0024EA10 File Offset: 0x0024CC10
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public DataColumn PHASE_DESCRIPTIONColumn
			{
				get
				{
					return this.columnPHASE_DESCRIPTION;
				}
			}

			// Token: 0x170039FA RID: 14842
			// (get) Token: 0x0600BD60 RID: 48480 RVA: 0x0024EA18 File Offset: 0x0024CC18
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[Browsable(false)]
			public int Count
			{
				get
				{
					return base.Rows.Count;
				}
			}

			// Token: 0x140006A9 RID: 1705
			// (add) Token: 0x0600BD62 RID: 48482 RVA: 0x0024EA38 File Offset: 0x0024CC38
			// (remove) Token: 0x0600BD63 RID: 48483 RVA: 0x0024EA70 File Offset: 0x0024CC70
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public event WorkflowDataSet.WorkflowPhaseRowChangeEventHandler WorkflowPhaseRowChanging;

			// Token: 0x140006AA RID: 1706
			// (add) Token: 0x0600BD64 RID: 48484 RVA: 0x0024EAA8 File Offset: 0x0024CCA8
			// (remove) Token: 0x0600BD65 RID: 48485 RVA: 0x0024EAE0 File Offset: 0x0024CCE0
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public event WorkflowDataSet.WorkflowPhaseRowChangeEventHandler WorkflowPhaseRowChanged;

			// Token: 0x140006AB RID: 1707
			// (add) Token: 0x0600BD66 RID: 48486 RVA: 0x0024EB18 File Offset: 0x0024CD18
			// (remove) Token: 0x0600BD67 RID: 48487 RVA: 0x0024EB50 File Offset: 0x0024CD50
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public event WorkflowDataSet.WorkflowPhaseRowChangeEventHandler WorkflowPhaseRowDeleting;

			// Token: 0x140006AC RID: 1708
			// (add) Token: 0x0600BD68 RID: 48488 RVA: 0x0024EB88 File Offset: 0x0024CD88
			// (remove) Token: 0x0600BD69 RID: 48489 RVA: 0x0024EBC0 File Offset: 0x0024CDC0
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public event WorkflowDataSet.WorkflowPhaseRowChangeEventHandler WorkflowPhaseRowDeleted;

			// Token: 0x0600BD6A RID: 48490 RVA: 0x0024EBF5 File Offset: 0x0024CDF5
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			public void AddWorkflowPhaseRow(WorkflowDataSet.WorkflowPhaseRow row)
			{
				base.Rows.Add(row);
			}

			// Token: 0x0600BD6B RID: 48491 RVA: 0x0024EC04 File Offset: 0x0024CE04
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public WorkflowDataSet.WorkflowPhaseRow AddWorkflowPhaseRow(Guid PHASE_UID, string PHASE_NAME, string PHASE_DESCRIPTION)
			{
				WorkflowDataSet.WorkflowPhaseRow workflowPhaseRow = (WorkflowDataSet.WorkflowPhaseRow)base.NewRow();
				object[] itemArray = new object[]
				{
					PHASE_UID,
					PHASE_NAME,
					PHASE_DESCRIPTION
				};
				workflowPhaseRow.ItemArray = itemArray;
				base.Rows.Add(workflowPhaseRow);
				return workflowPhaseRow;
			}

			// Token: 0x0600BD6C RID: 48492 RVA: 0x0024EC4C File Offset: 0x0024CE4C
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public WorkflowDataSet.WorkflowPhaseRow FindByPHASE_UID(Guid PHASE_UID)
			{
				return (WorkflowDataSet.WorkflowPhaseRow)base.Rows.Find(new object[]
				{
					PHASE_UID
				});
			}

			// Token: 0x0600BD6D RID: 48493 RVA: 0x0024EC7A File Offset: 0x0024CE7A
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			public virtual IEnumerator GetEnumerator()
			{
				return base.Rows.GetEnumerator();
			}

			// Token: 0x0600BD6E RID: 48494 RVA: 0x0024EC88 File Offset: 0x0024CE88
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public override DataTable Clone()
			{
				WorkflowDataSet.WorkflowPhaseDataTable workflowPhaseDataTable = (WorkflowDataSet.WorkflowPhaseDataTable)base.Clone();
				workflowPhaseDataTable.InitVars();
				return workflowPhaseDataTable;
			}

			// Token: 0x0600BD6F RID: 48495 RVA: 0x0024ECA8 File Offset: 0x0024CEA8
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			protected override DataTable CreateInstance()
			{
				return new WorkflowDataSet.WorkflowPhaseDataTable();
			}

			// Token: 0x0600BD70 RID: 48496 RVA: 0x0024ECB0 File Offset: 0x0024CEB0
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			internal void InitVars()
			{
				this.columnPHASE_UID = base.Columns["PHASE_UID"];
				this.columnPHASE_NAME = base.Columns["PHASE_NAME"];
				this.columnPHASE_DESCRIPTION = base.Columns["PHASE_DESCRIPTION"];
			}

			// Token: 0x0600BD71 RID: 48497 RVA: 0x0024ED00 File Offset: 0x0024CF00
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			private void InitClass()
			{
				this.columnPHASE_UID = new DataColumn("PHASE_UID", typeof(Guid), null, MappingType.Element);
				base.Columns.Add(this.columnPHASE_UID);
				this.columnPHASE_NAME = new DataColumn("PHASE_NAME", typeof(string), null, MappingType.Element);
				base.Columns.Add(this.columnPHASE_NAME);
				this.columnPHASE_DESCRIPTION = new DataColumn("PHASE_DESCRIPTION", typeof(string), null, MappingType.Element);
				base.Columns.Add(this.columnPHASE_DESCRIPTION);
				base.Constraints.Add(new UniqueConstraint("Constraint1", new DataColumn[]
				{
					this.columnPHASE_UID
				}, true));
				this.columnPHASE_UID.AllowDBNull = false;
				this.columnPHASE_UID.Unique = true;
				this.columnPHASE_NAME.AllowDBNull = false;
			}

			// Token: 0x0600BD72 RID: 48498 RVA: 0x0024EDDF File Offset: 0x0024CFDF
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public WorkflowDataSet.WorkflowPhaseRow NewWorkflowPhaseRow()
			{
				return (WorkflowDataSet.WorkflowPhaseRow)base.NewRow();
			}

			// Token: 0x0600BD73 RID: 48499 RVA: 0x0024EDEC File Offset: 0x0024CFEC
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			protected override DataRow NewRowFromBuilder(DataRowBuilder builder)
			{
				return new WorkflowDataSet.WorkflowPhaseRow(builder);
			}

			// Token: 0x0600BD74 RID: 48500 RVA: 0x0024EDF4 File Offset: 0x0024CFF4
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			protected override Type GetRowType()
			{
				return typeof(WorkflowDataSet.WorkflowPhaseRow);
			}

			// Token: 0x0600BD75 RID: 48501 RVA: 0x0024EE00 File Offset: 0x0024D000
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			protected override void OnRowChanged(DataRowChangeEventArgs e)
			{
				base.OnRowChanged(e);
				if (this.WorkflowPhaseRowChanged != null)
				{
					this.WorkflowPhaseRowChanged(this, new WorkflowDataSet.WorkflowPhaseRowChangeEvent((WorkflowDataSet.WorkflowPhaseRow)e.Row, e.Action));
				}
			}

			// Token: 0x0600BD76 RID: 48502 RVA: 0x0024EE33 File Offset: 0x0024D033
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			protected override void OnRowChanging(DataRowChangeEventArgs e)
			{
				base.OnRowChanging(e);
				if (this.WorkflowPhaseRowChanging != null)
				{
					this.WorkflowPhaseRowChanging(this, new WorkflowDataSet.WorkflowPhaseRowChangeEvent((WorkflowDataSet.WorkflowPhaseRow)e.Row, e.Action));
				}
			}

			// Token: 0x0600BD77 RID: 48503 RVA: 0x0024EE66 File Offset: 0x0024D066
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			protected override void OnRowDeleted(DataRowChangeEventArgs e)
			{
				base.OnRowDeleted(e);
				if (this.WorkflowPhaseRowDeleted != null)
				{
					this.WorkflowPhaseRowDeleted(this, new WorkflowDataSet.WorkflowPhaseRowChangeEvent((WorkflowDataSet.WorkflowPhaseRow)e.Row, e.Action));
				}
			}

			// Token: 0x0600BD78 RID: 48504 RVA: 0x0024EE99 File Offset: 0x0024D099
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			protected override void OnRowDeleting(DataRowChangeEventArgs e)
			{
				base.OnRowDeleting(e);
				if (this.WorkflowPhaseRowDeleting != null)
				{
					this.WorkflowPhaseRowDeleting(this, new WorkflowDataSet.WorkflowPhaseRowChangeEvent((WorkflowDataSet.WorkflowPhaseRow)e.Row, e.Action));
				}
			}

			// Token: 0x0600BD79 RID: 48505 RVA: 0x0024EECC File Offset: 0x0024D0CC
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			public void RemoveWorkflowPhaseRow(WorkflowDataSet.WorkflowPhaseRow row)
			{
				base.Rows.Remove(row);
			}

			// Token: 0x0600BD7A RID: 48506 RVA: 0x0024EEDC File Offset: 0x0024D0DC
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			public static XmlSchemaComplexType GetTypedTableSchema(XmlSchemaSet xs)
			{
				XmlSchemaComplexType xmlSchemaComplexType = new XmlSchemaComplexType();
				XmlSchemaSequence xmlSchemaSequence = new XmlSchemaSequence();
				WorkflowDataSet workflowDataSet = new WorkflowDataSet();
				XmlSchemaAny xmlSchemaAny = new XmlSchemaAny();
				xmlSchemaAny.Namespace = "http://www.w3.org/2001/XMLSchema";
				xmlSchemaAny.MinOccurs = 0m;
				xmlSchemaAny.MaxOccurs = decimal.MaxValue;
				xmlSchemaAny.ProcessContents = XmlSchemaContentProcessing.Lax;
				xmlSchemaSequence.Items.Add(xmlSchemaAny);
				XmlSchemaAny xmlSchemaAny2 = new XmlSchemaAny();
				xmlSchemaAny2.Namespace = "urn:schemas-microsoft-com:xml-diffgram-v1";
				xmlSchemaAny2.MinOccurs = 1m;
				xmlSchemaAny2.ProcessContents = XmlSchemaContentProcessing.Lax;
				xmlSchemaSequence.Items.Add(xmlSchemaAny2);
				XmlSchemaAttribute xmlSchemaAttribute = new XmlSchemaAttribute();
				xmlSchemaAttribute.Name = "namespace";
				xmlSchemaAttribute.FixedValue = workflowDataSet.Namespace;
				xmlSchemaComplexType.Attributes.Add(xmlSchemaAttribute);
				XmlSchemaAttribute xmlSchemaAttribute2 = new XmlSchemaAttribute();
				xmlSchemaAttribute2.Name = "tableTypeName";
				xmlSchemaAttribute2.FixedValue = "WorkflowPhaseDataTable";
				xmlSchemaComplexType.Attributes.Add(xmlSchemaAttribute2);
				xmlSchemaComplexType.Particle = xmlSchemaSequence;
				XmlSchema schemaSerializable = workflowDataSet.GetSchemaSerializable();
				if (xs.Contains(schemaSerializable.TargetNamespace))
				{
					MemoryStream memoryStream = new MemoryStream();
					MemoryStream memoryStream2 = new MemoryStream();
					try
					{
						schemaSerializable.Write(memoryStream);
						foreach (object obj in xs.Schemas(schemaSerializable.TargetNamespace))
						{
							XmlSchema xmlSchema = (XmlSchema)obj;
							memoryStream2.SetLength(0L);
							xmlSchema.Write(memoryStream2);
							if (memoryStream.Length == memoryStream2.Length)
							{
								memoryStream.Position = 0L;
								memoryStream2.Position = 0L;
								while (memoryStream.Position != memoryStream.Length && memoryStream.ReadByte() == memoryStream2.ReadByte())
								{
								}
								if (memoryStream.Position == memoryStream.Length)
								{
									return xmlSchemaComplexType;
								}
							}
						}
					}
					finally
					{
						if (memoryStream != null)
						{
							memoryStream.Close();
						}
						if (memoryStream2 != null)
						{
							memoryStream2.Close();
						}
					}
				}
				xs.Add(schemaSerializable);
				return xmlSchemaComplexType;
			}

			// Token: 0x04002639 RID: 9785
			private DataColumn columnPHASE_UID;

			// Token: 0x0400263A RID: 9786
			private DataColumn columnPHASE_NAME;

			// Token: 0x0400263B RID: 9787
			private DataColumn columnPHASE_DESCRIPTION;
		}

		// Token: 0x020007AB RID: 1963
		[XmlSchemaProvider("GetTypedTableSchema")]
		[Serializable]
		public class WorkflowStageDataTable : DataTable, IEnumerable
		{
			// Token: 0x0600BD7B RID: 48507 RVA: 0x0024F0D4 File Offset: 0x0024D2D4
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			public WorkflowStageDataTable()
			{
				base.TableName = "WorkflowStage";
				this.BeginInit();
				this.InitClass();
				this.EndInit();
			}

			// Token: 0x0600BD7C RID: 48508 RVA: 0x0024F0FC File Offset: 0x0024D2FC
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			internal WorkflowStageDataTable(DataTable table)
			{
				base.TableName = table.TableName;
				if (table.CaseSensitive != table.DataSet.CaseSensitive)
				{
					base.CaseSensitive = table.CaseSensitive;
				}
				if (table.Locale.ToString() != table.DataSet.Locale.ToString())
				{
					base.Locale = table.Locale;
				}
				if (table.Namespace != table.DataSet.Namespace)
				{
					base.Namespace = table.Namespace;
				}
				base.Prefix = table.Prefix;
				base.MinimumCapacity = table.MinimumCapacity;
			}

			// Token: 0x0600BD7D RID: 48509 RVA: 0x0024F1A4 File Offset: 0x0024D3A4
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			protected WorkflowStageDataTable(SerializationInfo info, StreamingContext context) : base(info, context)
			{
				this.InitVars();
			}

			// Token: 0x170039FC RID: 14844
			// (get) Token: 0x0600BD7E RID: 48510 RVA: 0x0024F1B4 File Offset: 0x0024D3B4
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			public DataColumn STAGE_UIDColumn
			{
				get
				{
					return this.columnSTAGE_UID;
				}
			}

			// Token: 0x170039FD RID: 14845
			// (get) Token: 0x0600BD7F RID: 48511 RVA: 0x0024F1BC File Offset: 0x0024D3BC
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public DataColumn STAGE_NAMEColumn
			{
				get
				{
					return this.columnSTAGE_NAME;
				}
			}

			// Token: 0x170039FE RID: 14846
			// (get) Token: 0x0600BD80 RID: 48512 RVA: 0x0024F1C4 File Offset: 0x0024D3C4
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			public DataColumn PHASE_UIDColumn
			{
				get
				{
					return this.columnPHASE_UID;
				}
			}

			// Token: 0x170039FF RID: 14847
			// (get) Token: 0x0600BD81 RID: 48513 RVA: 0x0024F1CC File Offset: 0x0024D3CC
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			public DataColumn PHASE_NAMEColumn
			{
				get
				{
					return this.columnPHASE_NAME;
				}
			}

			// Token: 0x17003A00 RID: 14848
			// (get) Token: 0x0600BD82 RID: 48514 RVA: 0x0024F1D4 File Offset: 0x0024D3D4
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public DataColumn STAGE_DESCRIPTIONColumn
			{
				get
				{
					return this.columnSTAGE_DESCRIPTION;
				}
			}

			// Token: 0x17003A01 RID: 14849
			// (get) Token: 0x0600BD83 RID: 48515 RVA: 0x0024F1DC File Offset: 0x0024D3DC
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			public DataColumn CHECKIN_REQUIREDColumn
			{
				get
				{
					return this.columnCHECKIN_REQUIRED;
				}
			}

			// Token: 0x17003A02 RID: 14850
			// (get) Token: 0x0600BD84 RID: 48516 RVA: 0x0024F1E4 File Offset: 0x0024D3E4
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			public DataColumn STAGE_SUBMIT_DESCRIPTIONColumn
			{
				get
				{
					return this.columnSTAGE_SUBMIT_DESCRIPTION;
				}
			}

			// Token: 0x17003A03 RID: 14851
			// (get) Token: 0x0600BD85 RID: 48517 RVA: 0x0024F1EC File Offset: 0x0024D3EC
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public DataColumn STATUS_PDP_UIDColumn
			{
				get
				{
					return this.columnSTATUS_PDP_UID;
				}
			}

			// Token: 0x17003A04 RID: 14852
			// (get) Token: 0x0600BD86 RID: 48518 RVA: 0x0024F1F4 File Offset: 0x0024D3F4
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			[Browsable(false)]
			public int Count
			{
				get
				{
					return base.Rows.Count;
				}
			}

			// Token: 0x140006AD RID: 1709
			// (add) Token: 0x0600BD88 RID: 48520 RVA: 0x0024F214 File Offset: 0x0024D414
			// (remove) Token: 0x0600BD89 RID: 48521 RVA: 0x0024F24C File Offset: 0x0024D44C
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public event WorkflowDataSet.WorkflowStageRowChangeEventHandler WorkflowStageRowChanging;

			// Token: 0x140006AE RID: 1710
			// (add) Token: 0x0600BD8A RID: 48522 RVA: 0x0024F284 File Offset: 0x0024D484
			// (remove) Token: 0x0600BD8B RID: 48523 RVA: 0x0024F2BC File Offset: 0x0024D4BC
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public event WorkflowDataSet.WorkflowStageRowChangeEventHandler WorkflowStageRowChanged;

			// Token: 0x140006AF RID: 1711
			// (add) Token: 0x0600BD8C RID: 48524 RVA: 0x0024F2F4 File Offset: 0x0024D4F4
			// (remove) Token: 0x0600BD8D RID: 48525 RVA: 0x0024F32C File Offset: 0x0024D52C
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public event WorkflowDataSet.WorkflowStageRowChangeEventHandler WorkflowStageRowDeleting;

			// Token: 0x140006B0 RID: 1712
			// (add) Token: 0x0600BD8E RID: 48526 RVA: 0x0024F364 File Offset: 0x0024D564
			// (remove) Token: 0x0600BD8F RID: 48527 RVA: 0x0024F39C File Offset: 0x0024D59C
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public event WorkflowDataSet.WorkflowStageRowChangeEventHandler WorkflowStageRowDeleted;

			// Token: 0x0600BD90 RID: 48528 RVA: 0x0024F3D1 File Offset: 0x0024D5D1
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public void AddWorkflowStageRow(WorkflowDataSet.WorkflowStageRow row)
			{
				base.Rows.Add(row);
			}

			// Token: 0x0600BD91 RID: 48529 RVA: 0x0024F3E0 File Offset: 0x0024D5E0
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public WorkflowDataSet.WorkflowStageRow AddWorkflowStageRow(Guid STAGE_UID, string STAGE_NAME, WorkflowDataSet.WorkflowPhaseRow parentWorkflowPhaseRowByFK_WorkflowPhase_WorkflowStage, string PHASE_NAME, string STAGE_DESCRIPTION, bool CHECKIN_REQUIRED, string STAGE_SUBMIT_DESCRIPTION, Guid STATUS_PDP_UID)
			{
				WorkflowDataSet.WorkflowStageRow workflowStageRow = (WorkflowDataSet.WorkflowStageRow)base.NewRow();
				object[] array = new object[]
				{
					STAGE_UID,
					STAGE_NAME,
					null,
					PHASE_NAME,
					STAGE_DESCRIPTION,
					CHECKIN_REQUIRED,
					STAGE_SUBMIT_DESCRIPTION,
					STATUS_PDP_UID
				};
				if (parentWorkflowPhaseRowByFK_WorkflowPhase_WorkflowStage != null)
				{
					array[2] = parentWorkflowPhaseRowByFK_WorkflowPhase_WorkflowStage[0];
				}
				workflowStageRow.ItemArray = array;
				base.Rows.Add(workflowStageRow);
				return workflowStageRow;
			}

			// Token: 0x0600BD92 RID: 48530 RVA: 0x0024F454 File Offset: 0x0024D654
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			public WorkflowDataSet.WorkflowStageRow FindBySTAGE_UID(Guid STAGE_UID)
			{
				return (WorkflowDataSet.WorkflowStageRow)base.Rows.Find(new object[]
				{
					STAGE_UID
				});
			}

			// Token: 0x0600BD93 RID: 48531 RVA: 0x0024F482 File Offset: 0x0024D682
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public virtual IEnumerator GetEnumerator()
			{
				return base.Rows.GetEnumerator();
			}

			// Token: 0x0600BD94 RID: 48532 RVA: 0x0024F490 File Offset: 0x0024D690
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public override DataTable Clone()
			{
				WorkflowDataSet.WorkflowStageDataTable workflowStageDataTable = (WorkflowDataSet.WorkflowStageDataTable)base.Clone();
				workflowStageDataTable.InitVars();
				return workflowStageDataTable;
			}

			// Token: 0x0600BD95 RID: 48533 RVA: 0x0024F4B0 File Offset: 0x0024D6B0
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			protected override DataTable CreateInstance()
			{
				return new WorkflowDataSet.WorkflowStageDataTable();
			}

			// Token: 0x0600BD96 RID: 48534 RVA: 0x0024F4B8 File Offset: 0x0024D6B8
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			internal void InitVars()
			{
				this.columnSTAGE_UID = base.Columns["STAGE_UID"];
				this.columnSTAGE_NAME = base.Columns["STAGE_NAME"];
				this.columnPHASE_UID = base.Columns["PHASE_UID"];
				this.columnPHASE_NAME = base.Columns["PHASE_NAME"];
				this.columnSTAGE_DESCRIPTION = base.Columns["STAGE_DESCRIPTION"];
				this.columnCHECKIN_REQUIRED = base.Columns["CHECKIN_REQUIRED"];
				this.columnSTAGE_SUBMIT_DESCRIPTION = base.Columns["STAGE_SUBMIT_DESCRIPTION"];
				this.columnSTATUS_PDP_UID = base.Columns["STATUS_PDP_UID"];
			}

			// Token: 0x0600BD97 RID: 48535 RVA: 0x0024F578 File Offset: 0x0024D778
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			private void InitClass()
			{
				this.columnSTAGE_UID = new DataColumn("STAGE_UID", typeof(Guid), null, MappingType.Element);
				base.Columns.Add(this.columnSTAGE_UID);
				this.columnSTAGE_NAME = new DataColumn("STAGE_NAME", typeof(string), null, MappingType.Element);
				base.Columns.Add(this.columnSTAGE_NAME);
				this.columnPHASE_UID = new DataColumn("PHASE_UID", typeof(Guid), null, MappingType.Element);
				base.Columns.Add(this.columnPHASE_UID);
				this.columnPHASE_NAME = new DataColumn("PHASE_NAME", typeof(string), null, MappingType.Element);
				base.Columns.Add(this.columnPHASE_NAME);
				this.columnSTAGE_DESCRIPTION = new DataColumn("STAGE_DESCRIPTION", typeof(string), null, MappingType.Element);
				base.Columns.Add(this.columnSTAGE_DESCRIPTION);
				this.columnCHECKIN_REQUIRED = new DataColumn("CHECKIN_REQUIRED", typeof(bool), null, MappingType.Element);
				base.Columns.Add(this.columnCHECKIN_REQUIRED);
				this.columnSTAGE_SUBMIT_DESCRIPTION = new DataColumn("STAGE_SUBMIT_DESCRIPTION", typeof(string), null, MappingType.Element);
				base.Columns.Add(this.columnSTAGE_SUBMIT_DESCRIPTION);
				this.columnSTATUS_PDP_UID = new DataColumn("STATUS_PDP_UID", typeof(Guid), null, MappingType.Element);
				base.Columns.Add(this.columnSTATUS_PDP_UID);
				base.Constraints.Add(new UniqueConstraint("Constraint1", new DataColumn[]
				{
					this.columnSTAGE_UID
				}, true));
				this.columnSTAGE_UID.AllowDBNull = false;
				this.columnSTAGE_UID.Unique = true;
				this.columnSTAGE_NAME.AllowDBNull = false;
				this.columnPHASE_UID.AllowDBNull = false;
				this.columnPHASE_NAME.ReadOnly = true;
				this.columnCHECKIN_REQUIRED.AllowDBNull = false;
				this.columnCHECKIN_REQUIRED.DefaultValue = false;
			}

			// Token: 0x0600BD98 RID: 48536 RVA: 0x0024F76D File Offset: 0x0024D96D
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public WorkflowDataSet.WorkflowStageRow NewWorkflowStageRow()
			{
				return (WorkflowDataSet.WorkflowStageRow)base.NewRow();
			}

			// Token: 0x0600BD99 RID: 48537 RVA: 0x0024F77A File Offset: 0x0024D97A
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			protected override DataRow NewRowFromBuilder(DataRowBuilder builder)
			{
				return new WorkflowDataSet.WorkflowStageRow(builder);
			}

			// Token: 0x0600BD9A RID: 48538 RVA: 0x0024F782 File Offset: 0x0024D982
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			protected override Type GetRowType()
			{
				return typeof(WorkflowDataSet.WorkflowStageRow);
			}

			// Token: 0x0600BD9B RID: 48539 RVA: 0x0024F78E File Offset: 0x0024D98E
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			protected override void OnRowChanged(DataRowChangeEventArgs e)
			{
				base.OnRowChanged(e);
				if (this.WorkflowStageRowChanged != null)
				{
					this.WorkflowStageRowChanged(this, new WorkflowDataSet.WorkflowStageRowChangeEvent((WorkflowDataSet.WorkflowStageRow)e.Row, e.Action));
				}
			}

			// Token: 0x0600BD9C RID: 48540 RVA: 0x0024F7C1 File Offset: 0x0024D9C1
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			protected override void OnRowChanging(DataRowChangeEventArgs e)
			{
				base.OnRowChanging(e);
				if (this.WorkflowStageRowChanging != null)
				{
					this.WorkflowStageRowChanging(this, new WorkflowDataSet.WorkflowStageRowChangeEvent((WorkflowDataSet.WorkflowStageRow)e.Row, e.Action));
				}
			}

			// Token: 0x0600BD9D RID: 48541 RVA: 0x0024F7F4 File Offset: 0x0024D9F4
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			protected override void OnRowDeleted(DataRowChangeEventArgs e)
			{
				base.OnRowDeleted(e);
				if (this.WorkflowStageRowDeleted != null)
				{
					this.WorkflowStageRowDeleted(this, new WorkflowDataSet.WorkflowStageRowChangeEvent((WorkflowDataSet.WorkflowStageRow)e.Row, e.Action));
				}
			}

			// Token: 0x0600BD9E RID: 48542 RVA: 0x0024F827 File Offset: 0x0024DA27
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			protected override void OnRowDeleting(DataRowChangeEventArgs e)
			{
				base.OnRowDeleting(e);
				if (this.WorkflowStageRowDeleting != null)
				{
					this.WorkflowStageRowDeleting(this, new WorkflowDataSet.WorkflowStageRowChangeEvent((WorkflowDataSet.WorkflowStageRow)e.Row, e.Action));
				}
			}

			// Token: 0x0600BD9F RID: 48543 RVA: 0x0024F85A File Offset: 0x0024DA5A
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public void RemoveWorkflowStageRow(WorkflowDataSet.WorkflowStageRow row)
			{
				base.Rows.Remove(row);
			}

			// Token: 0x0600BDA0 RID: 48544 RVA: 0x0024F868 File Offset: 0x0024DA68
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			public static XmlSchemaComplexType GetTypedTableSchema(XmlSchemaSet xs)
			{
				XmlSchemaComplexType xmlSchemaComplexType = new XmlSchemaComplexType();
				XmlSchemaSequence xmlSchemaSequence = new XmlSchemaSequence();
				WorkflowDataSet workflowDataSet = new WorkflowDataSet();
				XmlSchemaAny xmlSchemaAny = new XmlSchemaAny();
				xmlSchemaAny.Namespace = "http://www.w3.org/2001/XMLSchema";
				xmlSchemaAny.MinOccurs = 0m;
				xmlSchemaAny.MaxOccurs = decimal.MaxValue;
				xmlSchemaAny.ProcessContents = XmlSchemaContentProcessing.Lax;
				xmlSchemaSequence.Items.Add(xmlSchemaAny);
				XmlSchemaAny xmlSchemaAny2 = new XmlSchemaAny();
				xmlSchemaAny2.Namespace = "urn:schemas-microsoft-com:xml-diffgram-v1";
				xmlSchemaAny2.MinOccurs = 1m;
				xmlSchemaAny2.ProcessContents = XmlSchemaContentProcessing.Lax;
				xmlSchemaSequence.Items.Add(xmlSchemaAny2);
				XmlSchemaAttribute xmlSchemaAttribute = new XmlSchemaAttribute();
				xmlSchemaAttribute.Name = "namespace";
				xmlSchemaAttribute.FixedValue = workflowDataSet.Namespace;
				xmlSchemaComplexType.Attributes.Add(xmlSchemaAttribute);
				XmlSchemaAttribute xmlSchemaAttribute2 = new XmlSchemaAttribute();
				xmlSchemaAttribute2.Name = "tableTypeName";
				xmlSchemaAttribute2.FixedValue = "WorkflowStageDataTable";
				xmlSchemaComplexType.Attributes.Add(xmlSchemaAttribute2);
				xmlSchemaComplexType.Particle = xmlSchemaSequence;
				XmlSchema schemaSerializable = workflowDataSet.GetSchemaSerializable();
				if (xs.Contains(schemaSerializable.TargetNamespace))
				{
					MemoryStream memoryStream = new MemoryStream();
					MemoryStream memoryStream2 = new MemoryStream();
					try
					{
						schemaSerializable.Write(memoryStream);
						foreach (object obj in xs.Schemas(schemaSerializable.TargetNamespace))
						{
							XmlSchema xmlSchema = (XmlSchema)obj;
							memoryStream2.SetLength(0L);
							xmlSchema.Write(memoryStream2);
							if (memoryStream.Length == memoryStream2.Length)
							{
								memoryStream.Position = 0L;
								memoryStream2.Position = 0L;
								while (memoryStream.Position != memoryStream.Length && memoryStream.ReadByte() == memoryStream2.ReadByte())
								{
								}
								if (memoryStream.Position == memoryStream.Length)
								{
									return xmlSchemaComplexType;
								}
							}
						}
					}
					finally
					{
						if (memoryStream != null)
						{
							memoryStream.Close();
						}
						if (memoryStream2 != null)
						{
							memoryStream2.Close();
						}
					}
				}
				xs.Add(schemaSerializable);
				return xmlSchemaComplexType;
			}

			// Token: 0x04002640 RID: 9792
			private DataColumn columnSTAGE_UID;

			// Token: 0x04002641 RID: 9793
			private DataColumn columnSTAGE_NAME;

			// Token: 0x04002642 RID: 9794
			private DataColumn columnPHASE_UID;

			// Token: 0x04002643 RID: 9795
			private DataColumn columnPHASE_NAME;

			// Token: 0x04002644 RID: 9796
			private DataColumn columnSTAGE_DESCRIPTION;

			// Token: 0x04002645 RID: 9797
			private DataColumn columnCHECKIN_REQUIRED;

			// Token: 0x04002646 RID: 9798
			private DataColumn columnSTAGE_SUBMIT_DESCRIPTION;

			// Token: 0x04002647 RID: 9799
			private DataColumn columnSTATUS_PDP_UID;
		}

		// Token: 0x020007AC RID: 1964
		[XmlSchemaProvider("GetTypedTableSchema")]
		[Serializable]
		public class WorkflowStageCustomFieldsDataTable : DataTable, IEnumerable
		{
			// Token: 0x0600BDA1 RID: 48545 RVA: 0x0024FA60 File Offset: 0x0024DC60
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			public WorkflowStageCustomFieldsDataTable()
			{
				base.TableName = "WorkflowStageCustomFields";
				this.BeginInit();
				this.InitClass();
				this.EndInit();
			}

			// Token: 0x0600BDA2 RID: 48546 RVA: 0x0024FA88 File Offset: 0x0024DC88
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			internal WorkflowStageCustomFieldsDataTable(DataTable table)
			{
				base.TableName = table.TableName;
				if (table.CaseSensitive != table.DataSet.CaseSensitive)
				{
					base.CaseSensitive = table.CaseSensitive;
				}
				if (table.Locale.ToString() != table.DataSet.Locale.ToString())
				{
					base.Locale = table.Locale;
				}
				if (table.Namespace != table.DataSet.Namespace)
				{
					base.Namespace = table.Namespace;
				}
				base.Prefix = table.Prefix;
				base.MinimumCapacity = table.MinimumCapacity;
			}

			// Token: 0x0600BDA3 RID: 48547 RVA: 0x0024FB30 File Offset: 0x0024DD30
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			protected WorkflowStageCustomFieldsDataTable(SerializationInfo info, StreamingContext context) : base(info, context)
			{
				this.InitVars();
			}

			// Token: 0x17003A06 RID: 14854
			// (get) Token: 0x0600BDA4 RID: 48548 RVA: 0x0024FB40 File Offset: 0x0024DD40
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public DataColumn STAGE_UIDColumn
			{
				get
				{
					return this.columnSTAGE_UID;
				}
			}

			// Token: 0x17003A07 RID: 14855
			// (get) Token: 0x0600BDA5 RID: 48549 RVA: 0x0024FB48 File Offset: 0x0024DD48
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			public DataColumn MD_PROP_UIDColumn
			{
				get
				{
					return this.columnMD_PROP_UID;
				}
			}

			// Token: 0x17003A08 RID: 14856
			// (get) Token: 0x0600BDA6 RID: 48550 RVA: 0x0024FB50 File Offset: 0x0024DD50
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public DataColumn MD_PROP_NAMEColumn
			{
				get
				{
					return this.columnMD_PROP_NAME;
				}
			}

			// Token: 0x17003A09 RID: 14857
			// (get) Token: 0x0600BDA7 RID: 48551 RVA: 0x0024FB58 File Offset: 0x0024DD58
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			public DataColumn REQUIREDColumn
			{
				get
				{
					return this.columnREQUIRED;
				}
			}

			// Token: 0x17003A0A RID: 14858
			// (get) Token: 0x0600BDA8 RID: 48552 RVA: 0x0024FB60 File Offset: 0x0024DD60
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			public DataColumn READ_ONLYColumn
			{
				get
				{
					return this.columnREAD_ONLY;
				}
			}

			// Token: 0x17003A0B RID: 14859
			// (get) Token: 0x0600BDA9 RID: 48553 RVA: 0x0024FB68 File Offset: 0x0024DD68
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[Browsable(false)]
			[DebuggerNonUserCode]
			public int Count
			{
				get
				{
					return base.Rows.Count;
				}
			}

			// Token: 0x140006B1 RID: 1713
			// (add) Token: 0x0600BDAB RID: 48555 RVA: 0x0024FB88 File Offset: 0x0024DD88
			// (remove) Token: 0x0600BDAC RID: 48556 RVA: 0x0024FBC0 File Offset: 0x0024DDC0
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public event WorkflowDataSet.WorkflowStageCustomFieldsRowChangeEventHandler WorkflowStageCustomFieldsRowChanging;

			// Token: 0x140006B2 RID: 1714
			// (add) Token: 0x0600BDAD RID: 48557 RVA: 0x0024FBF8 File Offset: 0x0024DDF8
			// (remove) Token: 0x0600BDAE RID: 48558 RVA: 0x0024FC30 File Offset: 0x0024DE30
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public event WorkflowDataSet.WorkflowStageCustomFieldsRowChangeEventHandler WorkflowStageCustomFieldsRowChanged;

			// Token: 0x140006B3 RID: 1715
			// (add) Token: 0x0600BDAF RID: 48559 RVA: 0x0024FC68 File Offset: 0x0024DE68
			// (remove) Token: 0x0600BDB0 RID: 48560 RVA: 0x0024FCA0 File Offset: 0x0024DEA0
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public event WorkflowDataSet.WorkflowStageCustomFieldsRowChangeEventHandler WorkflowStageCustomFieldsRowDeleting;

			// Token: 0x140006B4 RID: 1716
			// (add) Token: 0x0600BDB1 RID: 48561 RVA: 0x0024FCD8 File Offset: 0x0024DED8
			// (remove) Token: 0x0600BDB2 RID: 48562 RVA: 0x0024FD10 File Offset: 0x0024DF10
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public event WorkflowDataSet.WorkflowStageCustomFieldsRowChangeEventHandler WorkflowStageCustomFieldsRowDeleted;

			// Token: 0x0600BDB3 RID: 48563 RVA: 0x0024FD45 File Offset: 0x0024DF45
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public void AddWorkflowStageCustomFieldsRow(WorkflowDataSet.WorkflowStageCustomFieldsRow row)
			{
				base.Rows.Add(row);
			}

			// Token: 0x0600BDB4 RID: 48564 RVA: 0x0024FD54 File Offset: 0x0024DF54
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public WorkflowDataSet.WorkflowStageCustomFieldsRow AddWorkflowStageCustomFieldsRow(WorkflowDataSet.WorkflowStageRow parentWorkflowStageRowByFK_WorkflowStage_WorkflowStageCustomFields, Guid MD_PROP_UID, string MD_PROP_NAME, bool REQUIRED, bool READ_ONLY)
			{
				WorkflowDataSet.WorkflowStageCustomFieldsRow workflowStageCustomFieldsRow = (WorkflowDataSet.WorkflowStageCustomFieldsRow)base.NewRow();
				object[] array = new object[]
				{
					null,
					MD_PROP_UID,
					MD_PROP_NAME,
					REQUIRED,
					READ_ONLY
				};
				if (parentWorkflowStageRowByFK_WorkflowStage_WorkflowStageCustomFields != null)
				{
					array[0] = parentWorkflowStageRowByFK_WorkflowStage_WorkflowStageCustomFields[0];
				}
				workflowStageCustomFieldsRow.ItemArray = array;
				base.Rows.Add(workflowStageCustomFieldsRow);
				return workflowStageCustomFieldsRow;
			}

			// Token: 0x0600BDB5 RID: 48565 RVA: 0x0024FDB8 File Offset: 0x0024DFB8
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public WorkflowDataSet.WorkflowStageCustomFieldsRow FindBySTAGE_UIDMD_PROP_UID(Guid STAGE_UID, Guid MD_PROP_UID)
			{
				return (WorkflowDataSet.WorkflowStageCustomFieldsRow)base.Rows.Find(new object[]
				{
					STAGE_UID,
					MD_PROP_UID
				});
			}

			// Token: 0x0600BDB6 RID: 48566 RVA: 0x0024FDEF File Offset: 0x0024DFEF
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public virtual IEnumerator GetEnumerator()
			{
				return base.Rows.GetEnumerator();
			}

			// Token: 0x0600BDB7 RID: 48567 RVA: 0x0024FDFC File Offset: 0x0024DFFC
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			public override DataTable Clone()
			{
				WorkflowDataSet.WorkflowStageCustomFieldsDataTable workflowStageCustomFieldsDataTable = (WorkflowDataSet.WorkflowStageCustomFieldsDataTable)base.Clone();
				workflowStageCustomFieldsDataTable.InitVars();
				return workflowStageCustomFieldsDataTable;
			}

			// Token: 0x0600BDB8 RID: 48568 RVA: 0x0024FE1C File Offset: 0x0024E01C
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			protected override DataTable CreateInstance()
			{
				return new WorkflowDataSet.WorkflowStageCustomFieldsDataTable();
			}

			// Token: 0x0600BDB9 RID: 48569 RVA: 0x0024FE24 File Offset: 0x0024E024
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			internal void InitVars()
			{
				this.columnSTAGE_UID = base.Columns["STAGE_UID"];
				this.columnMD_PROP_UID = base.Columns["MD_PROP_UID"];
				this.columnMD_PROP_NAME = base.Columns["MD_PROP_NAME"];
				this.columnREQUIRED = base.Columns["REQUIRED"];
				this.columnREAD_ONLY = base.Columns["READ_ONLY"];
			}

			// Token: 0x0600BDBA RID: 48570 RVA: 0x0024FEA0 File Offset: 0x0024E0A0
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			private void InitClass()
			{
				this.columnSTAGE_UID = new DataColumn("STAGE_UID", typeof(Guid), null, MappingType.Element);
				base.Columns.Add(this.columnSTAGE_UID);
				this.columnMD_PROP_UID = new DataColumn("MD_PROP_UID", typeof(Guid), null, MappingType.Element);
				base.Columns.Add(this.columnMD_PROP_UID);
				this.columnMD_PROP_NAME = new DataColumn("MD_PROP_NAME", typeof(string), null, MappingType.Element);
				base.Columns.Add(this.columnMD_PROP_NAME);
				this.columnREQUIRED = new DataColumn("REQUIRED", typeof(bool), null, MappingType.Element);
				base.Columns.Add(this.columnREQUIRED);
				this.columnREAD_ONLY = new DataColumn("READ_ONLY", typeof(bool), null, MappingType.Element);
				base.Columns.Add(this.columnREAD_ONLY);
				base.Constraints.Add(new UniqueConstraint("Constraint1", new DataColumn[]
				{
					this.columnSTAGE_UID,
					this.columnMD_PROP_UID
				}, true));
				this.columnSTAGE_UID.AllowDBNull = false;
				this.columnMD_PROP_UID.AllowDBNull = false;
				this.columnMD_PROP_NAME.AllowDBNull = false;
				this.columnMD_PROP_NAME.ReadOnly = true;
				this.columnMD_PROP_NAME.DefaultValue = "";
				this.columnREQUIRED.AllowDBNull = false;
				this.columnREQUIRED.DefaultValue = false;
				this.columnREAD_ONLY.AllowDBNull = false;
				this.columnREAD_ONLY.DefaultValue = false;
			}

			// Token: 0x0600BDBB RID: 48571 RVA: 0x00250038 File Offset: 0x0024E238
			/*[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public WorkflowDataSet.WorkflowStageCustomFieldsRow NewWorkflowStageCustomFieldsRow()
			{
				return (WorkflowDataSet.WorkflowStageCustomFieldsRow)base.NewRow();
			}*/

			// Token: 0x0600BDBC RID: 48572 RVA: 0x00250045 File Offset: 0x0024E245
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			protected override DataRow NewRowFromBuilder(DataRowBuilder builder)
			{
				return new WorkflowDataSet.WorkflowStageCustomFieldsRow(builder);
			}

			// Token: 0x0600BDBD RID: 48573 RVA: 0x0025004D File Offset: 0x0024E24D
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			protected override Type GetRowType()
			{
				return typeof(WorkflowDataSet.WorkflowStageCustomFieldsRow);
			}

			// Token: 0x0600BDBE RID: 48574 RVA: 0x00250059 File Offset: 0x0024E259
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			protected override void OnRowChanged(DataRowChangeEventArgs e)
			{
				base.OnRowChanged(e);
				if (this.WorkflowStageCustomFieldsRowChanged != null)
				{
					this.WorkflowStageCustomFieldsRowChanged(this, new WorkflowDataSet.WorkflowStageCustomFieldsRowChangeEvent((WorkflowDataSet.WorkflowStageCustomFieldsRow)e.Row, e.Action));
				}
			}

			// Token: 0x0600BDBF RID: 48575 RVA: 0x0025008C File Offset: 0x0024E28C
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			protected override void OnRowChanging(DataRowChangeEventArgs e)
			{
				base.OnRowChanging(e);
				if (this.WorkflowStageCustomFieldsRowChanging != null)
				{
					this.WorkflowStageCustomFieldsRowChanging(this, new WorkflowDataSet.WorkflowStageCustomFieldsRowChangeEvent((WorkflowDataSet.WorkflowStageCustomFieldsRow)e.Row, e.Action));
				}
			}

			// Token: 0x0600BDC0 RID: 48576 RVA: 0x002500BF File Offset: 0x0024E2BF
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			protected override void OnRowDeleted(DataRowChangeEventArgs e)
			{
				base.OnRowDeleted(e);
				if (this.WorkflowStageCustomFieldsRowDeleted != null)
				{
					this.WorkflowStageCustomFieldsRowDeleted(this, new WorkflowDataSet.WorkflowStageCustomFieldsRowChangeEvent((WorkflowDataSet.WorkflowStageCustomFieldsRow)e.Row, e.Action));
				}
			}

			// Token: 0x0600BDC1 RID: 48577 RVA: 0x002500F2 File Offset: 0x0024E2F2
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			protected override void OnRowDeleting(DataRowChangeEventArgs e)
			{
				base.OnRowDeleting(e);
				if (this.WorkflowStageCustomFieldsRowDeleting != null)
				{
					this.WorkflowStageCustomFieldsRowDeleting(this, new WorkflowDataSet.WorkflowStageCustomFieldsRowChangeEvent((WorkflowDataSet.WorkflowStageCustomFieldsRow)e.Row, e.Action));
				}
			}

			// Token: 0x0600BDC2 RID: 48578 RVA: 0x00250125 File Offset: 0x0024E325
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			public void RemoveWorkflowStageCustomFieldsRow(WorkflowDataSet.WorkflowStageCustomFieldsRow row)
			{
				base.Rows.Remove(row);
			}

			// Token: 0x0600BDC3 RID: 48579 RVA: 0x00250134 File Offset: 0x0024E334
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			public static XmlSchemaComplexType GetTypedTableSchema(XmlSchemaSet xs)
			{
				XmlSchemaComplexType xmlSchemaComplexType = new XmlSchemaComplexType();
				XmlSchemaSequence xmlSchemaSequence = new XmlSchemaSequence();
				WorkflowDataSet workflowDataSet = new WorkflowDataSet();
				XmlSchemaAny xmlSchemaAny = new XmlSchemaAny();
				xmlSchemaAny.Namespace = "http://www.w3.org/2001/XMLSchema";
				xmlSchemaAny.MinOccurs = 0m;
				xmlSchemaAny.MaxOccurs = decimal.MaxValue;
				xmlSchemaAny.ProcessContents = XmlSchemaContentProcessing.Lax;
				xmlSchemaSequence.Items.Add(xmlSchemaAny);
				XmlSchemaAny xmlSchemaAny2 = new XmlSchemaAny();
				xmlSchemaAny2.Namespace = "urn:schemas-microsoft-com:xml-diffgram-v1";
				xmlSchemaAny2.MinOccurs = 1m;
				xmlSchemaAny2.ProcessContents = XmlSchemaContentProcessing.Lax;
				xmlSchemaSequence.Items.Add(xmlSchemaAny2);
				XmlSchemaAttribute xmlSchemaAttribute = new XmlSchemaAttribute();
				xmlSchemaAttribute.Name = "namespace";
				xmlSchemaAttribute.FixedValue = workflowDataSet.Namespace;
				xmlSchemaComplexType.Attributes.Add(xmlSchemaAttribute);
				XmlSchemaAttribute xmlSchemaAttribute2 = new XmlSchemaAttribute();
				xmlSchemaAttribute2.Name = "tableTypeName";
				xmlSchemaAttribute2.FixedValue = "WorkflowStageCustomFieldsDataTable";
				xmlSchemaComplexType.Attributes.Add(xmlSchemaAttribute2);
				xmlSchemaComplexType.Particle = xmlSchemaSequence;
				XmlSchema schemaSerializable = workflowDataSet.GetSchemaSerializable();
				if (xs.Contains(schemaSerializable.TargetNamespace))
				{
					MemoryStream memoryStream = new MemoryStream();
					MemoryStream memoryStream2 = new MemoryStream();
					try
					{
						schemaSerializable.Write(memoryStream);
						foreach (object obj in xs.Schemas(schemaSerializable.TargetNamespace))
						{
							XmlSchema xmlSchema = (XmlSchema)obj;
							memoryStream2.SetLength(0L);
							xmlSchema.Write(memoryStream2);
							if (memoryStream.Length == memoryStream2.Length)
							{
								memoryStream.Position = 0L;
								memoryStream2.Position = 0L;
								while (memoryStream.Position != memoryStream.Length && memoryStream.ReadByte() == memoryStream2.ReadByte())
								{
								}
								if (memoryStream.Position == memoryStream.Length)
								{
									return xmlSchemaComplexType;
								}
							}
						}
					}
					finally
					{
						if (memoryStream != null)
						{
							memoryStream.Close();
						}
						if (memoryStream2 != null)
						{
							memoryStream2.Close();
						}
					}
				}
				xs.Add(schemaSerializable);
				return xmlSchemaComplexType;
			}

			// Token: 0x0400264C RID: 9804
			private DataColumn columnSTAGE_UID;

			// Token: 0x0400264D RID: 9805
			private DataColumn columnMD_PROP_UID;

			// Token: 0x0400264E RID: 9806
			private DataColumn columnMD_PROP_NAME;

			// Token: 0x0400264F RID: 9807
			private DataColumn columnREQUIRED;

			// Token: 0x04002650 RID: 9808
			private DataColumn columnREAD_ONLY;
		}

		// Token: 0x020007AD RID: 1965
		[XmlSchemaProvider("GetTypedTableSchema")]
		[Serializable]
		public class WorkflowStageStrategicImpactDataTable : DataTable, IEnumerable
		{
			// Token: 0x0600BDC4 RID: 48580 RVA: 0x0025032C File Offset: 0x0024E52C
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			public WorkflowStageStrategicImpactDataTable()
			{
				base.TableName = "WorkflowStageStrategicImpact";
				this.BeginInit();
				this.InitClass();
				this.EndInit();
			}

			// Token: 0x0600BDC5 RID: 48581 RVA: 0x00250354 File Offset: 0x0024E554
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			internal WorkflowStageStrategicImpactDataTable(DataTable table)
			{
				base.TableName = table.TableName;
				if (table.CaseSensitive != table.DataSet.CaseSensitive)
				{
					base.CaseSensitive = table.CaseSensitive;
				}
				if (table.Locale.ToString() != table.DataSet.Locale.ToString())
				{
					base.Locale = table.Locale;
				}
				if (table.Namespace != table.DataSet.Namespace)
				{
					base.Namespace = table.Namespace;
				}
				base.Prefix = table.Prefix;
				base.MinimumCapacity = table.MinimumCapacity;
			}

			// Token: 0x0600BDC6 RID: 48582 RVA: 0x002503FC File Offset: 0x0024E5FC
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			protected WorkflowStageStrategicImpactDataTable(SerializationInfo info, StreamingContext context) : base(info, context)
			{
				this.InitVars();
			}

			// Token: 0x17003A0D RID: 14861
			// (get) Token: 0x0600BDC7 RID: 48583 RVA: 0x0025040C File Offset: 0x0024E60C
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			public DataColumn STAGE_UIDColumn
			{
				get
				{
					return this.columnSTAGE_UID;
				}
			}

			// Token: 0x17003A0E RID: 14862
			// (get) Token: 0x0600BDC8 RID: 48584 RVA: 0x00250414 File Offset: 0x0024E614
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			public DataColumn BEHAVIORColumn
			{
				get
				{
					return this.columnBEHAVIOR;
				}
			}

			// Token: 0x17003A0F RID: 14863
			// (get) Token: 0x0600BDC9 RID: 48585 RVA: 0x0025041C File Offset: 0x0024E61C
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[Browsable(false)]
			public int Count
			{
				get
				{
					return base.Rows.Count;
				}
			}

			// Token: 0x140006B5 RID: 1717
			// (add) Token: 0x0600BDCB RID: 48587 RVA: 0x0025043C File Offset: 0x0024E63C
			// (remove) Token: 0x0600BDCC RID: 48588 RVA: 0x00250474 File Offset: 0x0024E674
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public event WorkflowDataSet.WorkflowStageStrategicImpactRowChangeEventHandler WorkflowStageStrategicImpactRowChanging;

			// Token: 0x140006B6 RID: 1718
			// (add) Token: 0x0600BDCD RID: 48589 RVA: 0x002504AC File Offset: 0x0024E6AC
			// (remove) Token: 0x0600BDCE RID: 48590 RVA: 0x002504E4 File Offset: 0x0024E6E4
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public event WorkflowDataSet.WorkflowStageStrategicImpactRowChangeEventHandler WorkflowStageStrategicImpactRowChanged;

			// Token: 0x140006B7 RID: 1719
			// (add) Token: 0x0600BDCF RID: 48591 RVA: 0x0025051C File Offset: 0x0024E71C
			// (remove) Token: 0x0600BDD0 RID: 48592 RVA: 0x00250554 File Offset: 0x0024E754
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public event WorkflowDataSet.WorkflowStageStrategicImpactRowChangeEventHandler WorkflowStageStrategicImpactRowDeleting;

			// Token: 0x140006B8 RID: 1720
			// (add) Token: 0x0600BDD1 RID: 48593 RVA: 0x0025058C File Offset: 0x0024E78C
			// (remove) Token: 0x0600BDD2 RID: 48594 RVA: 0x002505C4 File Offset: 0x0024E7C4
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public event WorkflowDataSet.WorkflowStageStrategicImpactRowChangeEventHandler WorkflowStageStrategicImpactRowDeleted;

			// Token: 0x0600BDD3 RID: 48595 RVA: 0x002505F9 File Offset: 0x0024E7F9
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			public void AddWorkflowStageStrategicImpactRow(WorkflowDataSet.WorkflowStageStrategicImpactRow row)
			{
				base.Rows.Add(row);
			}

			// Token: 0x0600BDD4 RID: 48596 RVA: 0x00250608 File Offset: 0x0024E808
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public WorkflowDataSet.WorkflowStageStrategicImpactRow AddWorkflowStageStrategicImpactRow(WorkflowDataSet.WorkflowStageRow parentWorkflowStageRowByFK_WorkflowStage_WorkflowStageStrategicImpact, byte BEHAVIOR)
			{
				WorkflowDataSet.WorkflowStageStrategicImpactRow workflowStageStrategicImpactRow = (WorkflowDataSet.WorkflowStageStrategicImpactRow)base.NewRow();
				object[] array = new object[]
				{
					null,
					BEHAVIOR
				};
				if (parentWorkflowStageRowByFK_WorkflowStage_WorkflowStageStrategicImpact != null)
				{
					array[0] = parentWorkflowStageRowByFK_WorkflowStage_WorkflowStageStrategicImpact[0];
				}
				workflowStageStrategicImpactRow.ItemArray = array;
				base.Rows.Add(workflowStageStrategicImpactRow);
				return workflowStageStrategicImpactRow;
			}

			// Token: 0x0600BDD5 RID: 48597 RVA: 0x00250654 File Offset: 0x0024E854
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public WorkflowDataSet.WorkflowStageStrategicImpactRow FindBySTAGE_UID(Guid STAGE_UID)
			{
				return (WorkflowDataSet.WorkflowStageStrategicImpactRow)base.Rows.Find(new object[]
				{
					STAGE_UID
				});
			}

			// Token: 0x0600BDD6 RID: 48598 RVA: 0x00250682 File Offset: 0x0024E882
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			public virtual IEnumerator GetEnumerator()
			{
				return base.Rows.GetEnumerator();
			}

			// Token: 0x0600BDD7 RID: 48599 RVA: 0x00250690 File Offset: 0x0024E890
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public override DataTable Clone()
			{
				WorkflowDataSet.WorkflowStageStrategicImpactDataTable workflowStageStrategicImpactDataTable = (WorkflowDataSet.WorkflowStageStrategicImpactDataTable)base.Clone();
				workflowStageStrategicImpactDataTable.InitVars();
				return workflowStageStrategicImpactDataTable;
			}

			// Token: 0x0600BDD8 RID: 48600 RVA: 0x002506B0 File Offset: 0x0024E8B0
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			protected override DataTable CreateInstance()
			{
				return new WorkflowDataSet.WorkflowStageStrategicImpactDataTable();
			}

			// Token: 0x0600BDD9 RID: 48601 RVA: 0x002506B7 File Offset: 0x0024E8B7
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			internal void InitVars()
			{
				this.columnSTAGE_UID = base.Columns["STAGE_UID"];
				this.columnBEHAVIOR = base.Columns["BEHAVIOR"];
			}

			// Token: 0x0600BDDA RID: 48602 RVA: 0x002506E8 File Offset: 0x0024E8E8
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			private void InitClass()
			{
				this.columnSTAGE_UID = new DataColumn("STAGE_UID", typeof(Guid), null, MappingType.Element);
				base.Columns.Add(this.columnSTAGE_UID);
				this.columnBEHAVIOR = new DataColumn("BEHAVIOR", typeof(byte), null, MappingType.Element);
				base.Columns.Add(this.columnBEHAVIOR);
				base.Constraints.Add(new UniqueConstraint("Constraint1", new DataColumn[]
				{
					this.columnSTAGE_UID
				}, true));
				this.columnSTAGE_UID.AllowDBNull = false;
				this.columnSTAGE_UID.Unique = true;
			}

			// Token: 0x0600BDDB RID: 48603 RVA: 0x0025078E File Offset: 0x0024E98E
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			public WorkflowDataSet.WorkflowStageStrategicImpactRow NewWorkflowStageStrategicImpactRow()
			{
				return (WorkflowDataSet.WorkflowStageStrategicImpactRow)base.NewRow();
			}

			// Token: 0x0600BDDC RID: 48604 RVA: 0x0025079B File Offset: 0x0024E99B
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			protected override DataRow NewRowFromBuilder(DataRowBuilder builder)
			{
				return new WorkflowDataSet.WorkflowStageStrategicImpactRow(builder);
			}

			// Token: 0x0600BDDD RID: 48605 RVA: 0x002507A3 File Offset: 0x0024E9A3
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			protected override Type GetRowType()
			{
				return typeof(WorkflowDataSet.WorkflowStageStrategicImpactRow);
			}

			// Token: 0x0600BDDE RID: 48606 RVA: 0x002507AF File Offset: 0x0024E9AF
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			protected override void OnRowChanged(DataRowChangeEventArgs e)
			{
				base.OnRowChanged(e);
				if (this.WorkflowStageStrategicImpactRowChanged != null)
				{
					this.WorkflowStageStrategicImpactRowChanged(this, new WorkflowDataSet.WorkflowStageStrategicImpactRowChangeEvent((WorkflowDataSet.WorkflowStageStrategicImpactRow)e.Row, e.Action));
				}
			}

			// Token: 0x0600BDDF RID: 48607 RVA: 0x002507E2 File Offset: 0x0024E9E2
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			protected override void OnRowChanging(DataRowChangeEventArgs e)
			{
				base.OnRowChanging(e);
				if (this.WorkflowStageStrategicImpactRowChanging != null)
				{
					this.WorkflowStageStrategicImpactRowChanging(this, new WorkflowDataSet.WorkflowStageStrategicImpactRowChangeEvent((WorkflowDataSet.WorkflowStageStrategicImpactRow)e.Row, e.Action));
				}
			}

			// Token: 0x0600BDE0 RID: 48608 RVA: 0x00250815 File Offset: 0x0024EA15
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			protected override void OnRowDeleted(DataRowChangeEventArgs e)
			{
				base.OnRowDeleted(e);
				if (this.WorkflowStageStrategicImpactRowDeleted != null)
				{
					this.WorkflowStageStrategicImpactRowDeleted(this, new WorkflowDataSet.WorkflowStageStrategicImpactRowChangeEvent((WorkflowDataSet.WorkflowStageStrategicImpactRow)e.Row, e.Action));
				}
			}

			// Token: 0x0600BDE1 RID: 48609 RVA: 0x00250848 File Offset: 0x0024EA48
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			protected override void OnRowDeleting(DataRowChangeEventArgs e)
			{
				base.OnRowDeleting(e);
				if (this.WorkflowStageStrategicImpactRowDeleting != null)
				{
					this.WorkflowStageStrategicImpactRowDeleting(this, new WorkflowDataSet.WorkflowStageStrategicImpactRowChangeEvent((WorkflowDataSet.WorkflowStageStrategicImpactRow)e.Row, e.Action));
				}
			}

			// Token: 0x0600BDE2 RID: 48610 RVA: 0x0025087B File Offset: 0x0024EA7B
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			public void RemoveWorkflowStageStrategicImpactRow(WorkflowDataSet.WorkflowStageStrategicImpactRow row)
			{
				base.Rows.Remove(row);
			}

			// Token: 0x0600BDE3 RID: 48611 RVA: 0x0025088C File Offset: 0x0024EA8C
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public static XmlSchemaComplexType GetTypedTableSchema(XmlSchemaSet xs)
			{
				XmlSchemaComplexType xmlSchemaComplexType = new XmlSchemaComplexType();
				XmlSchemaSequence xmlSchemaSequence = new XmlSchemaSequence();
				WorkflowDataSet workflowDataSet = new WorkflowDataSet();
				XmlSchemaAny xmlSchemaAny = new XmlSchemaAny();
				xmlSchemaAny.Namespace = "http://www.w3.org/2001/XMLSchema";
				xmlSchemaAny.MinOccurs = 0m;
				xmlSchemaAny.MaxOccurs = decimal.MaxValue;
				xmlSchemaAny.ProcessContents = XmlSchemaContentProcessing.Lax;
				xmlSchemaSequence.Items.Add(xmlSchemaAny);
				XmlSchemaAny xmlSchemaAny2 = new XmlSchemaAny();
				xmlSchemaAny2.Namespace = "urn:schemas-microsoft-com:xml-diffgram-v1";
				xmlSchemaAny2.MinOccurs = 1m;
				xmlSchemaAny2.ProcessContents = XmlSchemaContentProcessing.Lax;
				xmlSchemaSequence.Items.Add(xmlSchemaAny2);
				XmlSchemaAttribute xmlSchemaAttribute = new XmlSchemaAttribute();
				xmlSchemaAttribute.Name = "namespace";
				xmlSchemaAttribute.FixedValue = workflowDataSet.Namespace;
				xmlSchemaComplexType.Attributes.Add(xmlSchemaAttribute);
				XmlSchemaAttribute xmlSchemaAttribute2 = new XmlSchemaAttribute();
				xmlSchemaAttribute2.Name = "tableTypeName";
				xmlSchemaAttribute2.FixedValue = "WorkflowStageStrategicImpactDataTable";
				xmlSchemaComplexType.Attributes.Add(xmlSchemaAttribute2);
				xmlSchemaComplexType.Particle = xmlSchemaSequence;
				XmlSchema schemaSerializable = workflowDataSet.GetSchemaSerializable();
				if (xs.Contains(schemaSerializable.TargetNamespace))
				{
					MemoryStream memoryStream = new MemoryStream();
					MemoryStream memoryStream2 = new MemoryStream();
					try
					{
						schemaSerializable.Write(memoryStream);
						foreach (object obj in xs.Schemas(schemaSerializable.TargetNamespace))
						{
							XmlSchema xmlSchema = (XmlSchema)obj;
							memoryStream2.SetLength(0L);
							xmlSchema.Write(memoryStream2);
							if (memoryStream.Length == memoryStream2.Length)
							{
								memoryStream.Position = 0L;
								memoryStream2.Position = 0L;
								while (memoryStream.Position != memoryStream.Length && memoryStream.ReadByte() == memoryStream2.ReadByte())
								{
								}
								if (memoryStream.Position == memoryStream.Length)
								{
									return xmlSchemaComplexType;
								}
							}
						}
					}
					finally
					{
						if (memoryStream != null)
						{
							memoryStream.Close();
						}
						if (memoryStream2 != null)
						{
							memoryStream2.Close();
						}
					}
				}
				xs.Add(schemaSerializable);
				return xmlSchemaComplexType;
			}

			// Token: 0x04002655 RID: 9813
			private DataColumn columnSTAGE_UID;

			// Token: 0x04002656 RID: 9814
			private DataColumn columnBEHAVIOR;
		}

		// Token: 0x020007AE RID: 1966
		[XmlSchemaProvider("GetTypedTableSchema")]
		[Serializable]
		public class WorkflowStagePDPsDataTable : DataTable, IEnumerable
		{
			// Token: 0x0600BDE4 RID: 48612 RVA: 0x00250A84 File Offset: 0x0024EC84
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public WorkflowStagePDPsDataTable()
			{
				base.TableName = "WorkflowStagePDPs";
				this.BeginInit();
				this.InitClass();
				this.EndInit();
			}

			// Token: 0x0600BDE5 RID: 48613 RVA: 0x00250AAC File Offset: 0x0024ECAC
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			internal WorkflowStagePDPsDataTable(DataTable table)
			{
				base.TableName = table.TableName;
				if (table.CaseSensitive != table.DataSet.CaseSensitive)
				{
					base.CaseSensitive = table.CaseSensitive;
				}
				if (table.Locale.ToString() != table.DataSet.Locale.ToString())
				{
					base.Locale = table.Locale;
				}
				if (table.Namespace != table.DataSet.Namespace)
				{
					base.Namespace = table.Namespace;
				}
				base.Prefix = table.Prefix;
				base.MinimumCapacity = table.MinimumCapacity;
			}

			// Token: 0x0600BDE6 RID: 48614 RVA: 0x00250B54 File Offset: 0x0024ED54
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			protected WorkflowStagePDPsDataTable(SerializationInfo info, StreamingContext context) : base(info, context)
			{
				this.InitVars();
			}

			// Token: 0x17003A11 RID: 14865
			// (get) Token: 0x0600BDE7 RID: 48615 RVA: 0x00250B64 File Offset: 0x0024ED64
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			public DataColumn STAGE_UIDColumn
			{
				get
				{
					return this.columnSTAGE_UID;
				}
			}

			// Token: 0x17003A12 RID: 14866
			// (get) Token: 0x0600BDE8 RID: 48616 RVA: 0x00250B6C File Offset: 0x0024ED6C
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			public DataColumn PDP_UIDColumn
			{
				get
				{
					return this.columnPDP_UID;
				}
			}

			// Token: 0x17003A13 RID: 14867
			// (get) Token: 0x0600BDE9 RID: 48617 RVA: 0x00250B74 File Offset: 0x0024ED74
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public DataColumn PDP_IDColumn
			{
				get
				{
					return this.columnPDP_ID;
				}
			}

			// Token: 0x17003A14 RID: 14868
			// (get) Token: 0x0600BDEA RID: 48618 RVA: 0x00250B7C File Offset: 0x0024ED7C
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			public DataColumn PDP_NAMEColumn
			{
				get
				{
					return this.columnPDP_NAME;
				}
			}

			// Token: 0x17003A15 RID: 14869
			// (get) Token: 0x0600BDEB RID: 48619 RVA: 0x00250B84 File Offset: 0x0024ED84
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			public DataColumn PDP_POSITIONColumn
			{
				get
				{
					return this.columnPDP_POSITION;
				}
			}

			// Token: 0x17003A16 RID: 14870
			// (get) Token: 0x0600BDEC RID: 48620 RVA: 0x00250B8C File Offset: 0x0024ED8C
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			public DataColumn PDP_STAGE_DESCRIPTIONColumn
			{
				get
				{
					return this.columnPDP_STAGE_DESCRIPTION;
				}
			}

			// Token: 0x17003A17 RID: 14871
			// (get) Token: 0x0600BDED RID: 48621 RVA: 0x00250B94 File Offset: 0x0024ED94
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public DataColumn PDP_REQUIRES_ATTENTIONColumn
			{
				get
				{
					return this.columnPDP_REQUIRES_ATTENTION;
				}
			}

			// Token: 0x17003A18 RID: 14872
			// (get) Token: 0x0600BDEE RID: 48622 RVA: 0x00250B9C File Offset: 0x0024ED9C
			[DebuggerNonUserCode]
			[Browsable(false)]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public int Count
			{
				get
				{
					return base.Rows.Count;
				}
			}

			// Token: 0x140006B9 RID: 1721
			// (add) Token: 0x0600BDF0 RID: 48624 RVA: 0x00250BBC File Offset: 0x0024EDBC
			// (remove) Token: 0x0600BDF1 RID: 48625 RVA: 0x00250BF4 File Offset: 0x0024EDF4
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public event WorkflowDataSet.WorkflowStagePDPsRowChangeEventHandler WorkflowStagePDPsRowChanging;

			// Token: 0x140006BA RID: 1722
			// (add) Token: 0x0600BDF2 RID: 48626 RVA: 0x00250C2C File Offset: 0x0024EE2C
			// (remove) Token: 0x0600BDF3 RID: 48627 RVA: 0x00250C64 File Offset: 0x0024EE64
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public event WorkflowDataSet.WorkflowStagePDPsRowChangeEventHandler WorkflowStagePDPsRowChanged;

			// Token: 0x140006BB RID: 1723
			// (add) Token: 0x0600BDF4 RID: 48628 RVA: 0x00250C9C File Offset: 0x0024EE9C
			// (remove) Token: 0x0600BDF5 RID: 48629 RVA: 0x00250CD4 File Offset: 0x0024EED4
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public event WorkflowDataSet.WorkflowStagePDPsRowChangeEventHandler WorkflowStagePDPsRowDeleting;

			// Token: 0x140006BC RID: 1724
			// (add) Token: 0x0600BDF6 RID: 48630 RVA: 0x00250D0C File Offset: 0x0024EF0C
			// (remove) Token: 0x0600BDF7 RID: 48631 RVA: 0x00250D44 File Offset: 0x0024EF44
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public event WorkflowDataSet.WorkflowStagePDPsRowChangeEventHandler WorkflowStagePDPsRowDeleted;

			// Token: 0x0600BDF8 RID: 48632 RVA: 0x00250D79 File Offset: 0x0024EF79
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			public void AddWorkflowStagePDPsRow(WorkflowDataSet.WorkflowStagePDPsRow row)
			{
				base.Rows.Add(row);
			}

			// Token: 0x0600BDF9 RID: 48633 RVA: 0x00250D88 File Offset: 0x0024EF88
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public WorkflowDataSet.WorkflowStagePDPsRow AddWorkflowStagePDPsRow(WorkflowDataSet.WorkflowStageRow parentWorkflowStageRowByFK_WorkflowStage_WorkflowStageEDPs, Guid PDP_UID, int PDP_ID, string PDP_NAME, int PDP_POSITION, string PDP_STAGE_DESCRIPTION, bool PDP_REQUIRES_ATTENTION)
			{
				WorkflowDataSet.WorkflowStagePDPsRow workflowStagePDPsRow = (WorkflowDataSet.WorkflowStagePDPsRow)base.NewRow();
				object[] array = new object[]
				{
					null,
					PDP_UID,
					PDP_ID,
					PDP_NAME,
					PDP_POSITION,
					PDP_STAGE_DESCRIPTION,
					PDP_REQUIRES_ATTENTION
				};
				if (parentWorkflowStageRowByFK_WorkflowStage_WorkflowStageEDPs != null)
				{
					array[0] = parentWorkflowStageRowByFK_WorkflowStage_WorkflowStageEDPs[0];
				}
				workflowStagePDPsRow.ItemArray = array;
				base.Rows.Add(workflowStagePDPsRow);
				return workflowStagePDPsRow;
			}

			// Token: 0x0600BDFA RID: 48634 RVA: 0x00250DFC File Offset: 0x0024EFFC
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			public WorkflowDataSet.WorkflowStagePDPsRow FindBySTAGE_UIDPDP_UID(Guid STAGE_UID, Guid PDP_UID)
			{
				return (WorkflowDataSet.WorkflowStagePDPsRow)base.Rows.Find(new object[]
				{
					STAGE_UID,
					PDP_UID
				});
			}

			// Token: 0x0600BDFB RID: 48635 RVA: 0x00250E33 File Offset: 0x0024F033
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public virtual IEnumerator GetEnumerator()
			{
				return base.Rows.GetEnumerator();
			}

			// Token: 0x0600BDFC RID: 48636 RVA: 0x00250E40 File Offset: 0x0024F040
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public override DataTable Clone()
			{
				WorkflowDataSet.WorkflowStagePDPsDataTable workflowStagePDPsDataTable = (WorkflowDataSet.WorkflowStagePDPsDataTable)base.Clone();
				workflowStagePDPsDataTable.InitVars();
				return workflowStagePDPsDataTable;
			}

			// Token: 0x0600BDFD RID: 48637 RVA: 0x00250E60 File Offset: 0x0024F060
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			protected override DataTable CreateInstance()
			{
				return new WorkflowDataSet.WorkflowStagePDPsDataTable();
			}

			// Token: 0x0600BDFE RID: 48638 RVA: 0x00250E68 File Offset: 0x0024F068
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			internal void InitVars()
			{
				this.columnSTAGE_UID = base.Columns["STAGE_UID"];
				this.columnPDP_UID = base.Columns["PDP_UID"];
				this.columnPDP_ID = base.Columns["PDP_ID"];
				this.columnPDP_NAME = base.Columns["PDP_NAME"];
				this.columnPDP_POSITION = base.Columns["PDP_POSITION"];
				this.columnPDP_STAGE_DESCRIPTION = base.Columns["PDP_STAGE_DESCRIPTION"];
				this.columnPDP_REQUIRES_ATTENTION = base.Columns["PDP_REQUIRES_ATTENTION"];
			}

			// Token: 0x0600BDFF RID: 48639 RVA: 0x00250F10 File Offset: 0x0024F110
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			private void InitClass()
			{
				this.columnSTAGE_UID = new DataColumn("STAGE_UID", typeof(Guid), null, MappingType.Element);
				base.Columns.Add(this.columnSTAGE_UID);
				this.columnPDP_UID = new DataColumn("PDP_UID", typeof(Guid), null, MappingType.Element);
				base.Columns.Add(this.columnPDP_UID);
				this.columnPDP_ID = new DataColumn("PDP_ID", typeof(int), null, MappingType.Element);
				base.Columns.Add(this.columnPDP_ID);
				this.columnPDP_NAME = new DataColumn("PDP_NAME", typeof(string), null, MappingType.Element);
				base.Columns.Add(this.columnPDP_NAME);
				this.columnPDP_POSITION = new DataColumn("PDP_POSITION", typeof(int), null, MappingType.Element);
				base.Columns.Add(this.columnPDP_POSITION);
				this.columnPDP_STAGE_DESCRIPTION = new DataColumn("PDP_STAGE_DESCRIPTION", typeof(string), null, MappingType.Element);
				base.Columns.Add(this.columnPDP_STAGE_DESCRIPTION);
				this.columnPDP_REQUIRES_ATTENTION = new DataColumn("PDP_REQUIRES_ATTENTION", typeof(bool), null, MappingType.Element);
				base.Columns.Add(this.columnPDP_REQUIRES_ATTENTION);
				base.Constraints.Add(new UniqueConstraint("Constraint1", new DataColumn[]
				{
					this.columnSTAGE_UID,
					this.columnPDP_UID
				}, true));
				this.columnSTAGE_UID.AllowDBNull = false;
				this.columnPDP_UID.AllowDBNull = false;
				this.columnPDP_UID.Caption = "MD_PROP_UID";
				this.columnPDP_ID.DefaultValue = 0;
				this.columnPDP_NAME.AllowDBNull = false;
				this.columnPDP_REQUIRES_ATTENTION.AllowDBNull = false;
				this.columnPDP_REQUIRES_ATTENTION.DefaultValue = false;
			}

			// Token: 0x0600BE00 RID: 48640 RVA: 0x002510EA File Offset: 0x0024F2EA
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			public WorkflowDataSet.WorkflowStagePDPsRow NewWorkflowStagePDPsRow()
			{
				return (WorkflowDataSet.WorkflowStagePDPsRow)base.NewRow();
			}

			// Token: 0x0600BE01 RID: 48641 RVA: 0x002510F7 File Offset: 0x0024F2F7
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			protected override DataRow NewRowFromBuilder(DataRowBuilder builder)
			{
				return new WorkflowDataSet.WorkflowStagePDPsRow(builder);
			}

			// Token: 0x0600BE02 RID: 48642 RVA: 0x002510FF File Offset: 0x0024F2FF
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			protected override Type GetRowType()
			{
				return typeof(WorkflowDataSet.WorkflowStagePDPsRow);
			}

			// Token: 0x0600BE03 RID: 48643 RVA: 0x0025110B File Offset: 0x0024F30B
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			protected override void OnRowChanged(DataRowChangeEventArgs e)
			{
				base.OnRowChanged(e);
				if (this.WorkflowStagePDPsRowChanged != null)
				{
					this.WorkflowStagePDPsRowChanged(this, new WorkflowDataSet.WorkflowStagePDPsRowChangeEvent((WorkflowDataSet.WorkflowStagePDPsRow)e.Row, e.Action));
				}
			}

			// Token: 0x0600BE04 RID: 48644 RVA: 0x0025113E File Offset: 0x0024F33E
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			protected override void OnRowChanging(DataRowChangeEventArgs e)
			{
				base.OnRowChanging(e);
				if (this.WorkflowStagePDPsRowChanging != null)
				{
					this.WorkflowStagePDPsRowChanging(this, new WorkflowDataSet.WorkflowStagePDPsRowChangeEvent((WorkflowDataSet.WorkflowStagePDPsRow)e.Row, e.Action));
				}
			}

			// Token: 0x0600BE05 RID: 48645 RVA: 0x00251171 File Offset: 0x0024F371
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			protected override void OnRowDeleted(DataRowChangeEventArgs e)
			{
				base.OnRowDeleted(e);
				if (this.WorkflowStagePDPsRowDeleted != null)
				{
					this.WorkflowStagePDPsRowDeleted(this, new WorkflowDataSet.WorkflowStagePDPsRowChangeEvent((WorkflowDataSet.WorkflowStagePDPsRow)e.Row, e.Action));
				}
			}

			// Token: 0x0600BE06 RID: 48646 RVA: 0x002511A4 File Offset: 0x0024F3A4
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			protected override void OnRowDeleting(DataRowChangeEventArgs e)
			{
				base.OnRowDeleting(e);
				if (this.WorkflowStagePDPsRowDeleting != null)
				{
					this.WorkflowStagePDPsRowDeleting(this, new WorkflowDataSet.WorkflowStagePDPsRowChangeEvent((WorkflowDataSet.WorkflowStagePDPsRow)e.Row, e.Action));
				}
			}

			// Token: 0x0600BE07 RID: 48647 RVA: 0x002511D7 File Offset: 0x0024F3D7
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public void RemoveWorkflowStagePDPsRow(WorkflowDataSet.WorkflowStagePDPsRow row)
			{
				base.Rows.Remove(row);
			}

			// Token: 0x0600BE08 RID: 48648 RVA: 0x002511E8 File Offset: 0x0024F3E8
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			public static XmlSchemaComplexType GetTypedTableSchema(XmlSchemaSet xs)
			{
				XmlSchemaComplexType xmlSchemaComplexType = new XmlSchemaComplexType();
				XmlSchemaSequence xmlSchemaSequence = new XmlSchemaSequence();
				WorkflowDataSet workflowDataSet = new WorkflowDataSet();
				XmlSchemaAny xmlSchemaAny = new XmlSchemaAny();
				xmlSchemaAny.Namespace = "http://www.w3.org/2001/XMLSchema";
				xmlSchemaAny.MinOccurs = 0m;
				xmlSchemaAny.MaxOccurs = decimal.MaxValue;
				xmlSchemaAny.ProcessContents = XmlSchemaContentProcessing.Lax;
				xmlSchemaSequence.Items.Add(xmlSchemaAny);
				XmlSchemaAny xmlSchemaAny2 = new XmlSchemaAny();
				xmlSchemaAny2.Namespace = "urn:schemas-microsoft-com:xml-diffgram-v1";
				xmlSchemaAny2.MinOccurs = 1m;
				xmlSchemaAny2.ProcessContents = XmlSchemaContentProcessing.Lax;
				xmlSchemaSequence.Items.Add(xmlSchemaAny2);
				XmlSchemaAttribute xmlSchemaAttribute = new XmlSchemaAttribute();
				xmlSchemaAttribute.Name = "namespace";
				xmlSchemaAttribute.FixedValue = workflowDataSet.Namespace;
				xmlSchemaComplexType.Attributes.Add(xmlSchemaAttribute);
				XmlSchemaAttribute xmlSchemaAttribute2 = new XmlSchemaAttribute();
				xmlSchemaAttribute2.Name = "tableTypeName";
				xmlSchemaAttribute2.FixedValue = "WorkflowStagePDPsDataTable";
				xmlSchemaComplexType.Attributes.Add(xmlSchemaAttribute2);
				xmlSchemaComplexType.Particle = xmlSchemaSequence;
				XmlSchema schemaSerializable = workflowDataSet.GetSchemaSerializable();
				if (xs.Contains(schemaSerializable.TargetNamespace))
				{
					MemoryStream memoryStream = new MemoryStream();
					MemoryStream memoryStream2 = new MemoryStream();
					try
					{
						schemaSerializable.Write(memoryStream);
						foreach (object obj in xs.Schemas(schemaSerializable.TargetNamespace))
						{
							XmlSchema xmlSchema = (XmlSchema)obj;
							memoryStream2.SetLength(0L);
							xmlSchema.Write(memoryStream2);
							if (memoryStream.Length == memoryStream2.Length)
							{
								memoryStream.Position = 0L;
								memoryStream2.Position = 0L;
								while (memoryStream.Position != memoryStream.Length && memoryStream.ReadByte() == memoryStream2.ReadByte())
								{
								}
								if (memoryStream.Position == memoryStream.Length)
								{
									return xmlSchemaComplexType;
								}
							}
						}
					}
					finally
					{
						if (memoryStream != null)
						{
							memoryStream.Close();
						}
						if (memoryStream2 != null)
						{
							memoryStream2.Close();
						}
					}
				}
				xs.Add(schemaSerializable);
				return xmlSchemaComplexType;
			}

			// Token: 0x0400265B RID: 9819
			private DataColumn columnSTAGE_UID;

			// Token: 0x0400265C RID: 9820
			private DataColumn columnPDP_UID;

			// Token: 0x0400265D RID: 9821
			private DataColumn columnPDP_ID;

			// Token: 0x0400265E RID: 9822
			private DataColumn columnPDP_NAME;

			// Token: 0x0400265F RID: 9823
			private DataColumn columnPDP_POSITION;

			// Token: 0x04002660 RID: 9824
			private DataColumn columnPDP_STAGE_DESCRIPTION;

			// Token: 0x04002661 RID: 9825
			private DataColumn columnPDP_REQUIRES_ATTENTION;
		}

		// Token: 0x020007AF RID: 1967
		[XmlSchemaProvider("GetTypedTableSchema")]
		[Serializable]
		public class WorkflowInstanceDataTable : DataTable, IEnumerable
		{
			// Token: 0x0600BE09 RID: 48649 RVA: 0x002513E0 File Offset: 0x0024F5E0
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public WorkflowInstanceDataTable()
			{
				base.TableName = "WorkflowInstance";
				this.BeginInit();
				this.InitClass();
				this.EndInit();
			}

			// Token: 0x0600BE0A RID: 48650 RVA: 0x00251408 File Offset: 0x0024F608
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			internal WorkflowInstanceDataTable(DataTable table)
			{
				base.TableName = table.TableName;
				if (table.CaseSensitive != table.DataSet.CaseSensitive)
				{
					base.CaseSensitive = table.CaseSensitive;
				}
				if (table.Locale.ToString() != table.DataSet.Locale.ToString())
				{
					base.Locale = table.Locale;
				}
				if (table.Namespace != table.DataSet.Namespace)
				{
					base.Namespace = table.Namespace;
				}
				base.Prefix = table.Prefix;
				base.MinimumCapacity = table.MinimumCapacity;
			}

			// Token: 0x0600BE0B RID: 48651 RVA: 0x002514B0 File Offset: 0x0024F6B0
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			protected WorkflowInstanceDataTable(SerializationInfo info, StreamingContext context) : base(info, context)
			{
				this.InitVars();
			}

			// Token: 0x17003A1A RID: 14874
			// (get) Token: 0x0600BE0C RID: 48652 RVA: 0x002514C0 File Offset: 0x0024F6C0
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public DataColumn WORKFLOW_INSTANCE_UIDColumn
			{
				get
				{
					return this.columnWORKFLOW_INSTANCE_UID;
				}
			}

			// Token: 0x17003A1B RID: 14875
			// (get) Token: 0x0600BE0D RID: 48653 RVA: 0x002514C8 File Offset: 0x0024F6C8
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public DataColumn WORKFLOW_ENGINE_VERSIONColumn
			{
				get
				{
					return this.columnWORKFLOW_ENGINE_VERSION;
				}
			}

			// Token: 0x17003A1C RID: 14876
			// (get) Token: 0x0600BE0E RID: 48654 RVA: 0x002514D0 File Offset: 0x0024F6D0
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public DataColumn PROJ_UIDColumn
			{
				get
				{
					return this.columnPROJ_UID;
				}
			}

			// Token: 0x17003A1D RID: 14877
			// (get) Token: 0x0600BE0F RID: 48655 RVA: 0x002514D8 File Offset: 0x0024F6D8
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public DataColumn ENTERPRISE_PROJECT_TYPE_UIDColumn
			{
				get
				{
					return this.columnENTERPRISE_PROJECT_TYPE_UID;
				}
			}

			// Token: 0x17003A1E RID: 14878
			// (get) Token: 0x0600BE10 RID: 48656 RVA: 0x002514E0 File Offset: 0x0024F6E0
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			public DataColumn ENTERPRISE_PROJECT_TYPE_NAMEColumn
			{
				get
				{
					return this.columnENTERPRISE_PROJECT_TYPE_NAME;
				}
			}

			// Token: 0x17003A1F RID: 14879
			// (get) Token: 0x0600BE11 RID: 48657 RVA: 0x002514E8 File Offset: 0x0024F6E8
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			[Browsable(false)]
			public int Count
			{
				get
				{
					return base.Rows.Count;
				}
			}

			// Token: 0x140006BD RID: 1725
			// (add) Token: 0x0600BE13 RID: 48659 RVA: 0x00251508 File Offset: 0x0024F708
			// (remove) Token: 0x0600BE14 RID: 48660 RVA: 0x00251540 File Offset: 0x0024F740
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public event WorkflowDataSet.WorkflowInstanceRowChangeEventHandler WorkflowInstanceRowChanging;

			// Token: 0x140006BE RID: 1726
			// (add) Token: 0x0600BE15 RID: 48661 RVA: 0x00251578 File Offset: 0x0024F778
			// (remove) Token: 0x0600BE16 RID: 48662 RVA: 0x002515B0 File Offset: 0x0024F7B0
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public event WorkflowDataSet.WorkflowInstanceRowChangeEventHandler WorkflowInstanceRowChanged;

			// Token: 0x140006BF RID: 1727
			// (add) Token: 0x0600BE17 RID: 48663 RVA: 0x002515E8 File Offset: 0x0024F7E8
			// (remove) Token: 0x0600BE18 RID: 48664 RVA: 0x00251620 File Offset: 0x0024F820
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public event WorkflowDataSet.WorkflowInstanceRowChangeEventHandler WorkflowInstanceRowDeleting;

			// Token: 0x140006C0 RID: 1728
			// (add) Token: 0x0600BE19 RID: 48665 RVA: 0x00251658 File Offset: 0x0024F858
			// (remove) Token: 0x0600BE1A RID: 48666 RVA: 0x00251690 File Offset: 0x0024F890
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public event WorkflowDataSet.WorkflowInstanceRowChangeEventHandler WorkflowInstanceRowDeleted;

			// Token: 0x0600BE1B RID: 48667 RVA: 0x002516C5 File Offset: 0x0024F8C5
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public void AddWorkflowInstanceRow(WorkflowDataSet.WorkflowInstanceRow row)
			{
				base.Rows.Add(row);
			}

			// Token: 0x0600BE1C RID: 48668 RVA: 0x002516D4 File Offset: 0x0024F8D4
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public WorkflowDataSet.WorkflowInstanceRow AddWorkflowInstanceRow(Guid WORKFLOW_INSTANCE_UID, int WORKFLOW_ENGINE_VERSION, Guid PROJ_UID, Guid ENTERPRISE_PROJECT_TYPE_UID, string ENTERPRISE_PROJECT_TYPE_NAME)
			{
				WorkflowDataSet.WorkflowInstanceRow workflowInstanceRow = (WorkflowDataSet.WorkflowInstanceRow)base.NewRow();
				object[] itemArray = new object[]
				{
					WORKFLOW_INSTANCE_UID,
					WORKFLOW_ENGINE_VERSION,
					PROJ_UID,
					ENTERPRISE_PROJECT_TYPE_UID,
					ENTERPRISE_PROJECT_TYPE_NAME
				};
				workflowInstanceRow.ItemArray = itemArray;
				base.Rows.Add(workflowInstanceRow);
				return workflowInstanceRow;
			}

			// Token: 0x0600BE1D RID: 48669 RVA: 0x00251734 File Offset: 0x0024F934
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public WorkflowDataSet.WorkflowInstanceRow FindByPROJ_UID(Guid PROJ_UID)
			{
				return (WorkflowDataSet.WorkflowInstanceRow)base.Rows.Find(new object[]
				{
					PROJ_UID
				});
			}

			// Token: 0x0600BE1E RID: 48670 RVA: 0x00251762 File Offset: 0x0024F962
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			public virtual IEnumerator GetEnumerator()
			{
				return base.Rows.GetEnumerator();
			}

			// Token: 0x0600BE1F RID: 48671 RVA: 0x00251770 File Offset: 0x0024F970
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public override DataTable Clone()
			{
				WorkflowDataSet.WorkflowInstanceDataTable workflowInstanceDataTable = (WorkflowDataSet.WorkflowInstanceDataTable)base.Clone();
				workflowInstanceDataTable.InitVars();
				return workflowInstanceDataTable;
			}

			// Token: 0x0600BE20 RID: 48672 RVA: 0x00251790 File Offset: 0x0024F990
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			protected override DataTable CreateInstance()
			{
				return new WorkflowDataSet.WorkflowInstanceDataTable();
			}

			// Token: 0x0600BE21 RID: 48673 RVA: 0x00251798 File Offset: 0x0024F998
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			internal void InitVars()
			{
				this.columnWORKFLOW_INSTANCE_UID = base.Columns["WORKFLOW_INSTANCE_UID"];
				this.columnWORKFLOW_ENGINE_VERSION = base.Columns["WORKFLOW_ENGINE_VERSION"];
				this.columnPROJ_UID = base.Columns["PROJ_UID"];
				this.columnENTERPRISE_PROJECT_TYPE_UID = base.Columns["ENTERPRISE_PROJECT_TYPE_UID"];
				this.columnENTERPRISE_PROJECT_TYPE_NAME = base.Columns["ENTERPRISE_PROJECT_TYPE_NAME"];
			}

			// Token: 0x0600BE22 RID: 48674 RVA: 0x00251814 File Offset: 0x0024FA14
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			private void InitClass()
			{
				this.columnWORKFLOW_INSTANCE_UID = new DataColumn("WORKFLOW_INSTANCE_UID", typeof(Guid), null, MappingType.Element);
				base.Columns.Add(this.columnWORKFLOW_INSTANCE_UID);
				this.columnWORKFLOW_ENGINE_VERSION = new DataColumn("WORKFLOW_ENGINE_VERSION", typeof(int), null, MappingType.Element);
				base.Columns.Add(this.columnWORKFLOW_ENGINE_VERSION);
				this.columnPROJ_UID = new DataColumn("PROJ_UID", typeof(Guid), null, MappingType.Element);
				base.Columns.Add(this.columnPROJ_UID);
				this.columnENTERPRISE_PROJECT_TYPE_UID = new DataColumn("ENTERPRISE_PROJECT_TYPE_UID", typeof(Guid), null, MappingType.Element);
				base.Columns.Add(this.columnENTERPRISE_PROJECT_TYPE_UID);
				this.columnENTERPRISE_PROJECT_TYPE_NAME = new DataColumn("ENTERPRISE_PROJECT_TYPE_NAME", typeof(string), null, MappingType.Element);
				base.Columns.Add(this.columnENTERPRISE_PROJECT_TYPE_NAME);
				base.Constraints.Add(new UniqueConstraint("Constraint1", new DataColumn[]
				{
					this.columnPROJ_UID
				}, true));
				this.columnPROJ_UID.AllowDBNull = false;
				this.columnPROJ_UID.Unique = true;
				this.columnENTERPRISE_PROJECT_TYPE_UID.AllowDBNull = false;
				this.columnENTERPRISE_PROJECT_TYPE_NAME.AllowDBNull = false;
				this.columnENTERPRISE_PROJECT_TYPE_NAME.ReadOnly = true;
			}

			// Token: 0x0600BE23 RID: 48675 RVA: 0x00251965 File Offset: 0x0024FB65
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public WorkflowDataSet.WorkflowInstanceRow NewWorkflowInstanceRow()
			{
				return (WorkflowDataSet.WorkflowInstanceRow)base.NewRow();
			}

			// Token: 0x0600BE24 RID: 48676 RVA: 0x00251972 File Offset: 0x0024FB72
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			protected override DataRow NewRowFromBuilder(DataRowBuilder builder)
			{
				return new WorkflowDataSet.WorkflowInstanceRow(builder);
			}

			// Token: 0x0600BE25 RID: 48677 RVA: 0x0025197A File Offset: 0x0024FB7A
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			protected override Type GetRowType()
			{
				return typeof(WorkflowDataSet.WorkflowInstanceRow);
			}

			// Token: 0x0600BE26 RID: 48678 RVA: 0x00251986 File Offset: 0x0024FB86
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			protected override void OnRowChanged(DataRowChangeEventArgs e)
			{
				base.OnRowChanged(e);
				if (this.WorkflowInstanceRowChanged != null)
				{
					this.WorkflowInstanceRowChanged(this, new WorkflowDataSet.WorkflowInstanceRowChangeEvent((WorkflowDataSet.WorkflowInstanceRow)e.Row, e.Action));
				}
			}

			// Token: 0x0600BE27 RID: 48679 RVA: 0x002519B9 File Offset: 0x0024FBB9
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			protected override void OnRowChanging(DataRowChangeEventArgs e)
			{
				base.OnRowChanging(e);
				if (this.WorkflowInstanceRowChanging != null)
				{
					this.WorkflowInstanceRowChanging(this, new WorkflowDataSet.WorkflowInstanceRowChangeEvent((WorkflowDataSet.WorkflowInstanceRow)e.Row, e.Action));
				}
			}

			// Token: 0x0600BE28 RID: 48680 RVA: 0x002519EC File Offset: 0x0024FBEC
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			protected override void OnRowDeleted(DataRowChangeEventArgs e)
			{
				base.OnRowDeleted(e);
				if (this.WorkflowInstanceRowDeleted != null)
				{
					this.WorkflowInstanceRowDeleted(this, new WorkflowDataSet.WorkflowInstanceRowChangeEvent((WorkflowDataSet.WorkflowInstanceRow)e.Row, e.Action));
				}
			}

			// Token: 0x0600BE29 RID: 48681 RVA: 0x00251A1F File Offset: 0x0024FC1F
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			protected override void OnRowDeleting(DataRowChangeEventArgs e)
			{
				base.OnRowDeleting(e);
				if (this.WorkflowInstanceRowDeleting != null)
				{
					this.WorkflowInstanceRowDeleting(this, new WorkflowDataSet.WorkflowInstanceRowChangeEvent((WorkflowDataSet.WorkflowInstanceRow)e.Row, e.Action));
				}
			}

			// Token: 0x0600BE2A RID: 48682 RVA: 0x00251A52 File Offset: 0x0024FC52
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			public void RemoveWorkflowInstanceRow(WorkflowDataSet.WorkflowInstanceRow row)
			{
				base.Rows.Remove(row);
			}

			// Token: 0x0600BE2B RID: 48683 RVA: 0x00251A60 File Offset: 0x0024FC60
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			public static XmlSchemaComplexType GetTypedTableSchema(XmlSchemaSet xs)
			{
				XmlSchemaComplexType xmlSchemaComplexType = new XmlSchemaComplexType();
				XmlSchemaSequence xmlSchemaSequence = new XmlSchemaSequence();
				WorkflowDataSet workflowDataSet = new WorkflowDataSet();
				XmlSchemaAny xmlSchemaAny = new XmlSchemaAny();
				xmlSchemaAny.Namespace = "http://www.w3.org/2001/XMLSchema";
				xmlSchemaAny.MinOccurs = 0m;
				xmlSchemaAny.MaxOccurs = decimal.MaxValue;
				xmlSchemaAny.ProcessContents = XmlSchemaContentProcessing.Lax;
				xmlSchemaSequence.Items.Add(xmlSchemaAny);
				XmlSchemaAny xmlSchemaAny2 = new XmlSchemaAny();
				xmlSchemaAny2.Namespace = "urn:schemas-microsoft-com:xml-diffgram-v1";
				xmlSchemaAny2.MinOccurs = 1m;
				xmlSchemaAny2.ProcessContents = XmlSchemaContentProcessing.Lax;
				xmlSchemaSequence.Items.Add(xmlSchemaAny2);
				XmlSchemaAttribute xmlSchemaAttribute = new XmlSchemaAttribute();
				xmlSchemaAttribute.Name = "namespace";
				xmlSchemaAttribute.FixedValue = workflowDataSet.Namespace;
				xmlSchemaComplexType.Attributes.Add(xmlSchemaAttribute);
				XmlSchemaAttribute xmlSchemaAttribute2 = new XmlSchemaAttribute();
				xmlSchemaAttribute2.Name = "tableTypeName";
				xmlSchemaAttribute2.FixedValue = "WorkflowInstanceDataTable";
				xmlSchemaComplexType.Attributes.Add(xmlSchemaAttribute2);
				xmlSchemaComplexType.Particle = xmlSchemaSequence;
				XmlSchema schemaSerializable = workflowDataSet.GetSchemaSerializable();
				if (xs.Contains(schemaSerializable.TargetNamespace))
				{
					MemoryStream memoryStream = new MemoryStream();
					MemoryStream memoryStream2 = new MemoryStream();
					try
					{
						schemaSerializable.Write(memoryStream);
						foreach (object obj in xs.Schemas(schemaSerializable.TargetNamespace))
						{
							XmlSchema xmlSchema = (XmlSchema)obj;
							memoryStream2.SetLength(0L);
							xmlSchema.Write(memoryStream2);
							if (memoryStream.Length == memoryStream2.Length)
							{
								memoryStream.Position = 0L;
								memoryStream2.Position = 0L;
								while (memoryStream.Position != memoryStream.Length && memoryStream.ReadByte() == memoryStream2.ReadByte())
								{
								}
								if (memoryStream.Position == memoryStream.Length)
								{
									return xmlSchemaComplexType;
								}
							}
						}
					}
					finally
					{
						if (memoryStream != null)
						{
							memoryStream.Close();
						}
						if (memoryStream2 != null)
						{
							memoryStream2.Close();
						}
					}
				}
				xs.Add(schemaSerializable);
				return xmlSchemaComplexType;
			}

			// Token: 0x04002666 RID: 9830
			private DataColumn columnWORKFLOW_INSTANCE_UID;

			// Token: 0x04002667 RID: 9831
			private DataColumn columnWORKFLOW_ENGINE_VERSION;

			// Token: 0x04002668 RID: 9832
			private DataColumn columnPROJ_UID;

			// Token: 0x04002669 RID: 9833
			private DataColumn columnENTERPRISE_PROJECT_TYPE_UID;

			// Token: 0x0400266A RID: 9834
			private DataColumn columnENTERPRISE_PROJECT_TYPE_NAME;
		}

		// Token: 0x020007B0 RID: 1968
		[XmlSchemaProvider("GetTypedTableSchema")]
		[Serializable]
		public class WorkflowAssociationDataTable : DataTable, IEnumerable
		{
			// Token: 0x0600BE2C RID: 48684 RVA: 0x00251C58 File Offset: 0x0024FE58
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			public WorkflowAssociationDataTable()
			{
				base.TableName = "WorkflowAssociation";
				this.BeginInit();
				this.InitClass();
				this.EndInit();
			}

			// Token: 0x0600BE2D RID: 48685 RVA: 0x00251C80 File Offset: 0x0024FE80
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			internal WorkflowAssociationDataTable(DataTable table)
			{
				base.TableName = table.TableName;
				if (table.CaseSensitive != table.DataSet.CaseSensitive)
				{
					base.CaseSensitive = table.CaseSensitive;
				}
				if (table.Locale.ToString() != table.DataSet.Locale.ToString())
				{
					base.Locale = table.Locale;
				}
				if (table.Namespace != table.DataSet.Namespace)
				{
					base.Namespace = table.Namespace;
				}
				base.Prefix = table.Prefix;
				base.MinimumCapacity = table.MinimumCapacity;
			}

			// Token: 0x0600BE2E RID: 48686 RVA: 0x00251D28 File Offset: 0x0024FF28
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			protected WorkflowAssociationDataTable(SerializationInfo info, StreamingContext context) : base(info, context)
			{
				this.InitVars();
			}

			// Token: 0x17003A21 RID: 14881
			// (get) Token: 0x0600BE2F RID: 48687 RVA: 0x00251D38 File Offset: 0x0024FF38
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public DataColumn WORKFLOW_ASSOCIATION_UIDColumn
			{
				get
				{
					return this.columnWORKFLOW_ASSOCIATION_UID;
				}
			}

			// Token: 0x17003A22 RID: 14882
			// (get) Token: 0x0600BE30 RID: 48688 RVA: 0x00251D40 File Offset: 0x0024FF40
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			public DataColumn WORKFLOW_ASSOCIATION_NAMEColumn
			{
				get
				{
					return this.columnWORKFLOW_ASSOCIATION_NAME;
				}
			}

			// Token: 0x17003A23 RID: 14883
			// (get) Token: 0x0600BE31 RID: 48689 RVA: 0x00251D48 File Offset: 0x0024FF48
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public DataColumn WORKFLOW_ASSOCIATION_DESCRIPTIONColumn
			{
				get
				{
					return this.columnWORKFLOW_ASSOCIATION_DESCRIPTION;
				}
			}

			// Token: 0x17003A24 RID: 14884
			// (get) Token: 0x0600BE32 RID: 48690 RVA: 0x00251D50 File Offset: 0x0024FF50
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[Browsable(false)]
			public int Count
			{
				get
				{
					return base.Rows.Count;
				}
			}

			// Token: 0x140006C1 RID: 1729
			// (add) Token: 0x0600BE34 RID: 48692 RVA: 0x00251D70 File Offset: 0x0024FF70
			// (remove) Token: 0x0600BE35 RID: 48693 RVA: 0x00251DA8 File Offset: 0x0024FFA8
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public event WorkflowDataSet.WorkflowAssociationRowChangeEventHandler WorkflowAssociationRowChanging;

			// Token: 0x140006C2 RID: 1730
			// (add) Token: 0x0600BE36 RID: 48694 RVA: 0x00251DE0 File Offset: 0x0024FFE0
			// (remove) Token: 0x0600BE37 RID: 48695 RVA: 0x00251E18 File Offset: 0x00250018
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public event WorkflowDataSet.WorkflowAssociationRowChangeEventHandler WorkflowAssociationRowChanged;

			// Token: 0x140006C3 RID: 1731
			// (add) Token: 0x0600BE38 RID: 48696 RVA: 0x00251E50 File Offset: 0x00250050
			// (remove) Token: 0x0600BE39 RID: 48697 RVA: 0x00251E88 File Offset: 0x00250088
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public event WorkflowDataSet.WorkflowAssociationRowChangeEventHandler WorkflowAssociationRowDeleting;

			// Token: 0x140006C4 RID: 1732
			// (add) Token: 0x0600BE3A RID: 48698 RVA: 0x00251EC0 File Offset: 0x002500C0
			// (remove) Token: 0x0600BE3B RID: 48699 RVA: 0x00251EF8 File Offset: 0x002500F8
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public event WorkflowDataSet.WorkflowAssociationRowChangeEventHandler WorkflowAssociationRowDeleted;

			// Token: 0x0600BE3C RID: 48700 RVA: 0x00251F2D File Offset: 0x0025012D
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			public void AddWorkflowAssociationRow(WorkflowDataSet.WorkflowAssociationRow row)
			{
				base.Rows.Add(row);
			}

			// Token: 0x0600BE3D RID: 48701 RVA: 0x00251F3C File Offset: 0x0025013C
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			public WorkflowDataSet.WorkflowAssociationRow AddWorkflowAssociationRow(Guid WORKFLOW_ASSOCIATION_UID, string WORKFLOW_ASSOCIATION_NAME, string WORKFLOW_ASSOCIATION_DESCRIPTION)
			{
				WorkflowDataSet.WorkflowAssociationRow workflowAssociationRow = (WorkflowDataSet.WorkflowAssociationRow)base.NewRow();
				object[] itemArray = new object[]
				{
					WORKFLOW_ASSOCIATION_UID,
					WORKFLOW_ASSOCIATION_NAME,
					WORKFLOW_ASSOCIATION_DESCRIPTION
				};
				workflowAssociationRow.ItemArray = itemArray;
				base.Rows.Add(workflowAssociationRow);
				return workflowAssociationRow;
			}

			// Token: 0x0600BE3E RID: 48702 RVA: 0x00251F84 File Offset: 0x00250184
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public WorkflowDataSet.WorkflowAssociationRow FindByWORKFLOW_ASSOCIATION_UID(Guid WORKFLOW_ASSOCIATION_UID)
			{
				return (WorkflowDataSet.WorkflowAssociationRow)base.Rows.Find(new object[]
				{
					WORKFLOW_ASSOCIATION_UID
				});
			}

			// Token: 0x0600BE3F RID: 48703 RVA: 0x00251FB2 File Offset: 0x002501B2
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public virtual IEnumerator GetEnumerator()
			{
				return base.Rows.GetEnumerator();
			}

			// Token: 0x0600BE40 RID: 48704 RVA: 0x00251FC0 File Offset: 0x002501C0
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			public override DataTable Clone()
			{
				WorkflowDataSet.WorkflowAssociationDataTable workflowAssociationDataTable = (WorkflowDataSet.WorkflowAssociationDataTable)base.Clone();
				workflowAssociationDataTable.InitVars();
				return workflowAssociationDataTable;
			}

			// Token: 0x0600BE41 RID: 48705 RVA: 0x00251FE0 File Offset: 0x002501E0
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			protected override DataTable CreateInstance()
			{
				return new WorkflowDataSet.WorkflowAssociationDataTable();
			}

			// Token: 0x0600BE42 RID: 48706 RVA: 0x00251FE8 File Offset: 0x002501E8
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			internal void InitVars()
			{
				this.columnWORKFLOW_ASSOCIATION_UID = base.Columns["WORKFLOW_ASSOCIATION_UID"];
				this.columnWORKFLOW_ASSOCIATION_NAME = base.Columns["WORKFLOW_ASSOCIATION_NAME"];
				this.columnWORKFLOW_ASSOCIATION_DESCRIPTION = base.Columns["WORKFLOW_ASSOCIATION_DESCRIPTION"];
			}

			// Token: 0x0600BE43 RID: 48707 RVA: 0x00252038 File Offset: 0x00250238
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			private void InitClass()
			{
				this.columnWORKFLOW_ASSOCIATION_UID = new DataColumn("WORKFLOW_ASSOCIATION_UID", typeof(Guid), null, MappingType.Element);
				base.Columns.Add(this.columnWORKFLOW_ASSOCIATION_UID);
				this.columnWORKFLOW_ASSOCIATION_NAME = new DataColumn("WORKFLOW_ASSOCIATION_NAME", typeof(string), null, MappingType.Element);
				base.Columns.Add(this.columnWORKFLOW_ASSOCIATION_NAME);
				this.columnWORKFLOW_ASSOCIATION_DESCRIPTION = new DataColumn("WORKFLOW_ASSOCIATION_DESCRIPTION", typeof(string), null, MappingType.Element);
				base.Columns.Add(this.columnWORKFLOW_ASSOCIATION_DESCRIPTION);
				base.Constraints.Add(new UniqueConstraint("Constraint1", new DataColumn[]
				{
					this.columnWORKFLOW_ASSOCIATION_UID
				}, true));
				this.columnWORKFLOW_ASSOCIATION_UID.AllowDBNull = false;
				this.columnWORKFLOW_ASSOCIATION_UID.Unique = true;
				this.columnWORKFLOW_ASSOCIATION_NAME.AllowDBNull = false;
			}

			// Token: 0x0600BE44 RID: 48708 RVA: 0x00252117 File Offset: 0x00250317
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			public WorkflowDataSet.WorkflowAssociationRow NewWorkflowAssociationRow()
			{
				return (WorkflowDataSet.WorkflowAssociationRow)base.NewRow();
			}

			// Token: 0x0600BE45 RID: 48709 RVA: 0x00252124 File Offset: 0x00250324
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			protected override DataRow NewRowFromBuilder(DataRowBuilder builder)
			{
				return new WorkflowDataSet.WorkflowAssociationRow(builder);
			}

			// Token: 0x0600BE46 RID: 48710 RVA: 0x0025212C File Offset: 0x0025032C
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			protected override Type GetRowType()
			{
				return typeof(WorkflowDataSet.WorkflowAssociationRow);
			}

			// Token: 0x0600BE47 RID: 48711 RVA: 0x00252138 File Offset: 0x00250338
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			protected override void OnRowChanged(DataRowChangeEventArgs e)
			{
				base.OnRowChanged(e);
				if (this.WorkflowAssociationRowChanged != null)
				{
					this.WorkflowAssociationRowChanged(this, new WorkflowDataSet.WorkflowAssociationRowChangeEvent((WorkflowDataSet.WorkflowAssociationRow)e.Row, e.Action));
				}
			}

			// Token: 0x0600BE48 RID: 48712 RVA: 0x0025216B File Offset: 0x0025036B
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			protected override void OnRowChanging(DataRowChangeEventArgs e)
			{
				base.OnRowChanging(e);
				if (this.WorkflowAssociationRowChanging != null)
				{
					this.WorkflowAssociationRowChanging(this, new WorkflowDataSet.WorkflowAssociationRowChangeEvent((WorkflowDataSet.WorkflowAssociationRow)e.Row, e.Action));
				}
			}

			// Token: 0x0600BE49 RID: 48713 RVA: 0x0025219E File Offset: 0x0025039E
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			protected override void OnRowDeleted(DataRowChangeEventArgs e)
			{
				base.OnRowDeleted(e);
				if (this.WorkflowAssociationRowDeleted != null)
				{
					this.WorkflowAssociationRowDeleted(this, new WorkflowDataSet.WorkflowAssociationRowChangeEvent((WorkflowDataSet.WorkflowAssociationRow)e.Row, e.Action));
				}
			}

			// Token: 0x0600BE4A RID: 48714 RVA: 0x002521D1 File Offset: 0x002503D1
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			protected override void OnRowDeleting(DataRowChangeEventArgs e)
			{
				base.OnRowDeleting(e);
				if (this.WorkflowAssociationRowDeleting != null)
				{
					this.WorkflowAssociationRowDeleting(this, new WorkflowDataSet.WorkflowAssociationRowChangeEvent((WorkflowDataSet.WorkflowAssociationRow)e.Row, e.Action));
				}
			}

			// Token: 0x0600BE4B RID: 48715 RVA: 0x00252204 File Offset: 0x00250404
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public void RemoveWorkflowAssociationRow(WorkflowDataSet.WorkflowAssociationRow row)
			{
				base.Rows.Remove(row);
			}

			// Token: 0x0600BE4C RID: 48716 RVA: 0x00252214 File Offset: 0x00250414
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			public static XmlSchemaComplexType GetTypedTableSchema(XmlSchemaSet xs)
			{
				XmlSchemaComplexType xmlSchemaComplexType = new XmlSchemaComplexType();
				XmlSchemaSequence xmlSchemaSequence = new XmlSchemaSequence();
				WorkflowDataSet workflowDataSet = new WorkflowDataSet();
				XmlSchemaAny xmlSchemaAny = new XmlSchemaAny();
				xmlSchemaAny.Namespace = "http://www.w3.org/2001/XMLSchema";
				xmlSchemaAny.MinOccurs = 0m;
				xmlSchemaAny.MaxOccurs = decimal.MaxValue;
				xmlSchemaAny.ProcessContents = XmlSchemaContentProcessing.Lax;
				xmlSchemaSequence.Items.Add(xmlSchemaAny);
				XmlSchemaAny xmlSchemaAny2 = new XmlSchemaAny();
				xmlSchemaAny2.Namespace = "urn:schemas-microsoft-com:xml-diffgram-v1";
				xmlSchemaAny2.MinOccurs = 1m;
				xmlSchemaAny2.ProcessContents = XmlSchemaContentProcessing.Lax;
				xmlSchemaSequence.Items.Add(xmlSchemaAny2);
				XmlSchemaAttribute xmlSchemaAttribute = new XmlSchemaAttribute();
				xmlSchemaAttribute.Name = "namespace";
				xmlSchemaAttribute.FixedValue = workflowDataSet.Namespace;
				xmlSchemaComplexType.Attributes.Add(xmlSchemaAttribute);
				XmlSchemaAttribute xmlSchemaAttribute2 = new XmlSchemaAttribute();
				xmlSchemaAttribute2.Name = "tableTypeName";
				xmlSchemaAttribute2.FixedValue = "WorkflowAssociationDataTable";
				xmlSchemaComplexType.Attributes.Add(xmlSchemaAttribute2);
				xmlSchemaComplexType.Particle = xmlSchemaSequence;
				XmlSchema schemaSerializable = workflowDataSet.GetSchemaSerializable();
				if (xs.Contains(schemaSerializable.TargetNamespace))
				{
					MemoryStream memoryStream = new MemoryStream();
					MemoryStream memoryStream2 = new MemoryStream();
					try
					{
						schemaSerializable.Write(memoryStream);
						foreach (object obj in xs.Schemas(schemaSerializable.TargetNamespace))
						{
							XmlSchema xmlSchema = (XmlSchema)obj;
							memoryStream2.SetLength(0L);
							xmlSchema.Write(memoryStream2);
							if (memoryStream.Length == memoryStream2.Length)
							{
								memoryStream.Position = 0L;
								memoryStream2.Position = 0L;
								while (memoryStream.Position != memoryStream.Length && memoryStream.ReadByte() == memoryStream2.ReadByte())
								{
								}
								if (memoryStream.Position == memoryStream.Length)
								{
									return xmlSchemaComplexType;
								}
							}
						}
					}
					finally
					{
						if (memoryStream != null)
						{
							memoryStream.Close();
						}
						if (memoryStream2 != null)
						{
							memoryStream2.Close();
						}
					}
				}
				xs.Add(schemaSerializable);
				return xmlSchemaComplexType;
			}

			// Token: 0x0400266F RID: 9839
			private DataColumn columnWORKFLOW_ASSOCIATION_UID;

			// Token: 0x04002670 RID: 9840
			private DataColumn columnWORKFLOW_ASSOCIATION_NAME;

			// Token: 0x04002671 RID: 9841
			private DataColumn columnWORKFLOW_ASSOCIATION_DESCRIPTION;
		}

		// Token: 0x020007B1 RID: 1969
		[XmlSchemaProvider("GetTypedTableSchema")]
		[Serializable]
		public class WorkflowStatusDataTable : DataTable, IEnumerable
		{
			// Token: 0x0600BE4D RID: 48717 RVA: 0x0025240C File Offset: 0x0025060C
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public WorkflowStatusDataTable()
			{
				base.TableName = "WorkflowStatus";
				this.BeginInit();
				this.InitClass();
				this.EndInit();
			}

			// Token: 0x0600BE4E RID: 48718 RVA: 0x00252434 File Offset: 0x00250634
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			internal WorkflowStatusDataTable(DataTable table)
			{
				base.TableName = table.TableName;
				if (table.CaseSensitive != table.DataSet.CaseSensitive)
				{
					base.CaseSensitive = table.CaseSensitive;
				}
				if (table.Locale.ToString() != table.DataSet.Locale.ToString())
				{
					base.Locale = table.Locale;
				}
				if (table.Namespace != table.DataSet.Namespace)
				{
					base.Namespace = table.Namespace;
				}
				base.Prefix = table.Prefix;
				base.MinimumCapacity = table.MinimumCapacity;
			}

			// Token: 0x0600BE4F RID: 48719 RVA: 0x002524DC File Offset: 0x002506DC
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			protected WorkflowStatusDataTable(SerializationInfo info, StreamingContext context) : base(info, context)
			{
				this.InitVars();
			}

			// Token: 0x17003A26 RID: 14886
			// (get) Token: 0x0600BE50 RID: 48720 RVA: 0x002524EC File Offset: 0x002506EC
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			public DataColumn WORKFLOW_INSTANCE_UIDColumn
			{
				get
				{
					return this.columnWORKFLOW_INSTANCE_UID;
				}
			}

			// Token: 0x17003A27 RID: 14887
			// (get) Token: 0x0600BE51 RID: 48721 RVA: 0x002524F4 File Offset: 0x002506F4
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public DataColumn PROJ_UIDColumn
			{
				get
				{
					return this.columnPROJ_UID;
				}
			}

			// Token: 0x17003A28 RID: 14888
			// (get) Token: 0x0600BE52 RID: 48722 RVA: 0x002524FC File Offset: 0x002506FC
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public DataColumn STAGE_UIDColumn
			{
				get
				{
					return this.columnSTAGE_UID;
				}
			}

			// Token: 0x17003A29 RID: 14889
			// (get) Token: 0x0600BE53 RID: 48723 RVA: 0x00252504 File Offset: 0x00250704
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public DataColumn STAGE_NAMEColumn
			{
				get
				{
					return this.columnSTAGE_NAME;
				}
			}

			// Token: 0x17003A2A RID: 14890
			// (get) Token: 0x0600BE54 RID: 48724 RVA: 0x0025250C File Offset: 0x0025070C
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			public DataColumn PHASE_UIDColumn
			{
				get
				{
					return this.columnPHASE_UID;
				}
			}

			// Token: 0x17003A2B RID: 14891
			// (get) Token: 0x0600BE55 RID: 48725 RVA: 0x00252514 File Offset: 0x00250714
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public DataColumn PHASE_NAMEColumn
			{
				get
				{
					return this.columnPHASE_NAME;
				}
			}

			// Token: 0x17003A2C RID: 14892
			// (get) Token: 0x0600BE56 RID: 48726 RVA: 0x0025251C File Offset: 0x0025071C
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			public DataColumn STAGE_STATUSColumn
			{
				get
				{
					return this.columnSTAGE_STATUS;
				}
			}

			// Token: 0x17003A2D RID: 14893
			// (get) Token: 0x0600BE57 RID: 48727 RVA: 0x00252524 File Offset: 0x00250724
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			public DataColumn STAGE_INFOColumn
			{
				get
				{
					return this.columnSTAGE_INFO;
				}
			}

			// Token: 0x17003A2E RID: 14894
			// (get) Token: 0x0600BE58 RID: 48728 RVA: 0x0025252C File Offset: 0x0025072C
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public DataColumn STAGE_ORDERColumn
			{
				get
				{
					return this.columnSTAGE_ORDER;
				}
			}

			// Token: 0x17003A2F RID: 14895
			// (get) Token: 0x0600BE59 RID: 48729 RVA: 0x00252534 File Offset: 0x00250734
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			public DataColumn CREATED_DATEColumn
			{
				get
				{
					return this.columnCREATED_DATE;
				}
			}

			// Token: 0x17003A30 RID: 14896
			// (get) Token: 0x0600BE5A RID: 48730 RVA: 0x0025253C File Offset: 0x0025073C
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public DataColumn MOD_DATEColumn
			{
				get
				{
					return this.columnMOD_DATE;
				}
			}

			// Token: 0x17003A31 RID: 14897
			// (get) Token: 0x0600BE5B RID: 48731 RVA: 0x00252544 File Offset: 0x00250744
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public DataColumn STAGE_ENTRY_DATEColumn
			{
				get
				{
					return this.columnSTAGE_ENTRY_DATE;
				}
			}

			// Token: 0x17003A32 RID: 14898
			// (get) Token: 0x0600BE5C RID: 48732 RVA: 0x0025254C File Offset: 0x0025074C
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			public DataColumn STAGE_COMPLETION_DATEColumn
			{
				get
				{
					return this.columnSTAGE_COMPLETION_DATE;
				}
			}

			// Token: 0x17003A33 RID: 14899
			// (get) Token: 0x0600BE5D RID: 48733 RVA: 0x00252554 File Offset: 0x00250754
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public DataColumn WORKFLOW_MOD_DATE
			{
				get
				{
					return this.columnWORKFLOW_MOD_DATE;
				}
			}

			// Token: 0x17003A34 RID: 14900
			// (get) Token: 0x0600BE5E RID: 48734 RVA: 0x0025255C File Offset: 0x0025075C
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			public DataColumn SUBMITTED_DATE
			{
				get
				{
					return this.columnSUBMITTED_DATE;
				}
			}

			// Token: 0x17003A35 RID: 14901
			// (get) Token: 0x0600BE5F RID: 48735 RVA: 0x00252564 File Offset: 0x00250764
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public DataColumn NEXT_STAGE1Column
			{
				get
				{
					return this.columnNEXT_STAGE1;
				}
			}

			// Token: 0x17003A36 RID: 14902
			// (get) Token: 0x0600BE60 RID: 48736 RVA: 0x0025256C File Offset: 0x0025076C
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public DataColumn NEXT_STAGE2Column
			{
				get
				{
					return this.columnNEXT_STAGE2;
				}
			}

			// Token: 0x17003A37 RID: 14903
			// (get) Token: 0x0600BE61 RID: 48737 RVA: 0x00252574 File Offset: 0x00250774
			[DebuggerNonUserCode]
			[Browsable(false)]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public int Count
			{
				get
				{
					return base.Rows.Count;
				}
			}

			// Token: 0x140006C5 RID: 1733
			// (add) Token: 0x0600BE63 RID: 48739 RVA: 0x00252594 File Offset: 0x00250794
			// (remove) Token: 0x0600BE64 RID: 48740 RVA: 0x002525CC File Offset: 0x002507CC
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public event WorkflowDataSet.WorkflowStatusRowChangeEventHandler WorkflowStatusRowChanging;

			// Token: 0x140006C6 RID: 1734
			// (add) Token: 0x0600BE65 RID: 48741 RVA: 0x00252604 File Offset: 0x00250804
			// (remove) Token: 0x0600BE66 RID: 48742 RVA: 0x0025263C File Offset: 0x0025083C
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public event WorkflowDataSet.WorkflowStatusRowChangeEventHandler WorkflowStatusRowChanged;

			// Token: 0x140006C7 RID: 1735
			// (add) Token: 0x0600BE67 RID: 48743 RVA: 0x00252674 File Offset: 0x00250874
			// (remove) Token: 0x0600BE68 RID: 48744 RVA: 0x002526AC File Offset: 0x002508AC
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public event WorkflowDataSet.WorkflowStatusRowChangeEventHandler WorkflowStatusRowDeleting;

			// Token: 0x140006C8 RID: 1736
			// (add) Token: 0x0600BE69 RID: 48745 RVA: 0x002526E4 File Offset: 0x002508E4
			// (remove) Token: 0x0600BE6A RID: 48746 RVA: 0x0025271C File Offset: 0x0025091C
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public event WorkflowDataSet.WorkflowStatusRowChangeEventHandler WorkflowStatusRowDeleted;

			// Token: 0x0600BE6B RID: 48747 RVA: 0x00252751 File Offset: 0x00250951
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			public void AddWorkflowStatusRow(WorkflowDataSet.WorkflowStatusRow row)
			{
				base.Rows.Add(row);
			}

			// Token: 0x0600BE6C RID: 48748 RVA: 0x00252760 File Offset: 0x00250960
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public WorkflowDataSet.WorkflowStatusRow AddWorkflowStatusRow(Guid WORKFLOW_INSTANCE_UID, Guid PROJ_UID, Guid STAGE_UID, string STAGE_NAME, Guid PHASE_UID, string PHASE_NAME, int STAGE_STATUS, string STAGE_INFO, int STAGE_ORDER, DateTime CREATED_DATE, DateTime MOD_DATE, DateTime STAGE_ENTRY_DATE, DateTime STAGE_COMPLETION_DATE, DateTime WORKFLOW_MOD_DATE, DateTime SUBMITTED_DATE, Guid NEXT_STAGE1, Guid NEXT_STAGE2)
			{
				WorkflowDataSet.WorkflowStatusRow workflowStatusRow = (WorkflowDataSet.WorkflowStatusRow)base.NewRow();
				object[] itemArray = new object[]
				{
					WORKFLOW_INSTANCE_UID,
					PROJ_UID,
					STAGE_UID,
					STAGE_NAME,
					PHASE_UID,
					PHASE_NAME,
					STAGE_STATUS,
					STAGE_INFO,
					STAGE_ORDER,
					CREATED_DATE,
					MOD_DATE,
					STAGE_ENTRY_DATE,
					STAGE_COMPLETION_DATE,
					WORKFLOW_MOD_DATE,
					SUBMITTED_DATE,
					NEXT_STAGE1,
					NEXT_STAGE2
				};
				workflowStatusRow.ItemArray = itemArray;
				base.Rows.Add(workflowStatusRow);
				return workflowStatusRow;
			}

			// Token: 0x0600BE6D RID: 48749 RVA: 0x00252838 File Offset: 0x00250A38
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public WorkflowDataSet.WorkflowStatusRow FindByWORKFLOW_INSTANCE_UIDPROJ_UIDSTAGE_UID(Guid WORKFLOW_INSTANCE_UID, Guid PROJ_UID, Guid STAGE_UID)
			{
				return (WorkflowDataSet.WorkflowStatusRow)base.Rows.Find(new object[]
				{
					WORKFLOW_INSTANCE_UID,
					PROJ_UID,
					STAGE_UID
				});
			}

			// Token: 0x0600BE6E RID: 48750 RVA: 0x00252878 File Offset: 0x00250A78
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			public virtual IEnumerator GetEnumerator()
			{
				return base.Rows.GetEnumerator();
			}

			// Token: 0x0600BE6F RID: 48751 RVA: 0x00252888 File Offset: 0x00250A88
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public override DataTable Clone()
			{
				WorkflowDataSet.WorkflowStatusDataTable workflowStatusDataTable = (WorkflowDataSet.WorkflowStatusDataTable)base.Clone();
				workflowStatusDataTable.InitVars();
				return workflowStatusDataTable;
			}

			// Token: 0x0600BE70 RID: 48752 RVA: 0x002528A8 File Offset: 0x00250AA8
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			protected override DataTable CreateInstance()
			{
				return new WorkflowDataSet.WorkflowStatusDataTable();
			}

			// Token: 0x0600BE71 RID: 48753 RVA: 0x002528B0 File Offset: 0x00250AB0
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			internal void InitVars()
			{
				this.columnWORKFLOW_INSTANCE_UID = base.Columns["WORKFLOW_INSTANCE_UID"];
				this.columnPROJ_UID = base.Columns["PROJ_UID"];
				this.columnSTAGE_UID = base.Columns["STAGE_UID"];
				this.columnSTAGE_NAME = base.Columns["STAGE_NAME"];
				this.columnPHASE_UID = base.Columns["PHASE_UID"];
				this.columnPHASE_NAME = base.Columns["PHASE_NAME"];
				this.columnSTAGE_STATUS = base.Columns["STAGE_STATUS"];
				this.columnSTAGE_INFO = base.Columns["STAGE_INFO"];
				this.columnSTAGE_ORDER = base.Columns["STAGE_ORDER"];
				this.columnCREATED_DATE = base.Columns["CREATED_DATE"];
				this.columnMOD_DATE = base.Columns["MOD_DATE"];
				this.columnSTAGE_ENTRY_DATE = base.Columns["STAGE_ENTRY_DATE"];
				this.columnSTAGE_COMPLETION_DATE = base.Columns["STAGE_COMPLETION_DATE"];
				this.columnWORKFLOW_MOD_DATE = base.Columns["WORKFLOW_MOD_DATE"];
				this.columnSUBMITTED_DATE = base.Columns["SUBMITTED_DATE"];
				this.columnNEXT_STAGE1 = base.Columns["NEXT_STAGE1"];
				this.columnNEXT_STAGE2 = base.Columns["NEXT_STAGE2"];
			}

			// Token: 0x0600BE72 RID: 48754 RVA: 0x00252A34 File Offset: 0x00250C34
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			private void InitClass()
			{
				this.columnWORKFLOW_INSTANCE_UID = new DataColumn("WORKFLOW_INSTANCE_UID", typeof(Guid), null, MappingType.Element);
				base.Columns.Add(this.columnWORKFLOW_INSTANCE_UID);
				this.columnPROJ_UID = new DataColumn("PROJ_UID", typeof(Guid), null, MappingType.Element);
				base.Columns.Add(this.columnPROJ_UID);
				this.columnSTAGE_UID = new DataColumn("STAGE_UID", typeof(Guid), null, MappingType.Element);
				base.Columns.Add(this.columnSTAGE_UID);
				this.columnSTAGE_NAME = new DataColumn("STAGE_NAME", typeof(string), null, MappingType.Element);
				base.Columns.Add(this.columnSTAGE_NAME);
				this.columnPHASE_UID = new DataColumn("PHASE_UID", typeof(Guid), null, MappingType.Element);
				base.Columns.Add(this.columnPHASE_UID);
				this.columnPHASE_NAME = new DataColumn("PHASE_NAME", typeof(string), null, MappingType.Element);
				base.Columns.Add(this.columnPHASE_NAME);
				this.columnSTAGE_STATUS = new DataColumn("STAGE_STATUS", typeof(int), null, MappingType.Element);
				base.Columns.Add(this.columnSTAGE_STATUS);
				this.columnSTAGE_INFO = new DataColumn("STAGE_INFO", typeof(string), null, MappingType.Element);
				base.Columns.Add(this.columnSTAGE_INFO);
				this.columnSTAGE_ORDER = new DataColumn("STAGE_ORDER", typeof(int), null, MappingType.Element);
				base.Columns.Add(this.columnSTAGE_ORDER);
				this.columnCREATED_DATE = new DataColumn("CREATED_DATE", typeof(DateTime), null, MappingType.Element);
				base.Columns.Add(this.columnCREATED_DATE);
				this.columnMOD_DATE = new DataColumn("MOD_DATE", typeof(DateTime), null, MappingType.Element);
				base.Columns.Add(this.columnMOD_DATE);
				this.columnSTAGE_ENTRY_DATE = new DataColumn("STAGE_ENTRY_DATE", typeof(DateTime), null, MappingType.Element);
				base.Columns.Add(this.columnSTAGE_ENTRY_DATE);
				this.columnSTAGE_COMPLETION_DATE = new DataColumn("STAGE_COMPLETION_DATE", typeof(DateTime), null, MappingType.Element);
				base.Columns.Add(this.columnSTAGE_COMPLETION_DATE);
				this.columnWORKFLOW_MOD_DATE = new DataColumn("WORKFLOW_MOD_DATE", typeof(DateTime), null, MappingType.Element);
				this.columnWORKFLOW_MOD_DATE.ExtendedProperties.Add("Generator_ColumnPropNameInTable", "WORKFLOW_MOD_DATE");
				this.columnWORKFLOW_MOD_DATE.ExtendedProperties.Add("Generator_UserColumnName", "WORKFLOW_MOD_DATE");
				base.Columns.Add(this.columnWORKFLOW_MOD_DATE);
				this.columnSUBMITTED_DATE = new DataColumn("SUBMITTED_DATE", typeof(DateTime), null, MappingType.Element);
				this.columnSUBMITTED_DATE.ExtendedProperties.Add("Generator_ColumnPropNameInTable", "SUBMITTED_DATE");
				this.columnSUBMITTED_DATE.ExtendedProperties.Add("Generator_UserColumnName", "SUBMITTED_DATE");
				base.Columns.Add(this.columnSUBMITTED_DATE);
				this.columnNEXT_STAGE1 = new DataColumn("NEXT_STAGE1", typeof(Guid), null, MappingType.Element);
				base.Columns.Add(this.columnNEXT_STAGE1);
				this.columnNEXT_STAGE2 = new DataColumn("NEXT_STAGE2", typeof(Guid), null, MappingType.Element);
				base.Columns.Add(this.columnNEXT_STAGE2);
				base.Constraints.Add(new UniqueConstraint("Constraint1", new DataColumn[]
				{
					this.columnWORKFLOW_INSTANCE_UID,
					this.columnPROJ_UID,
					this.columnSTAGE_UID
				}, true));
				this.columnWORKFLOW_INSTANCE_UID.AllowDBNull = false;
				this.columnPROJ_UID.AllowDBNull = false;
				this.columnSTAGE_UID.AllowDBNull = false;
				this.columnSTAGE_NAME.ReadOnly = true;
				this.columnPHASE_UID.ReadOnly = true;
				this.columnPHASE_NAME.ReadOnly = true;
				this.columnSTAGE_STATUS.AllowDBNull = false;
				this.columnSTAGE_ORDER.Caption = "STATUS_ORDER";
			}

			// Token: 0x0600BE73 RID: 48755 RVA: 0x00252E43 File Offset: 0x00251043
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			public WorkflowDataSet.WorkflowStatusRow NewWorkflowStatusRow()
			{
				return (WorkflowDataSet.WorkflowStatusRow)base.NewRow();
			}

			// Token: 0x0600BE74 RID: 48756 RVA: 0x00252E50 File Offset: 0x00251050
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			protected override DataRow NewRowFromBuilder(DataRowBuilder builder)
			{
				return new WorkflowDataSet.WorkflowStatusRow(builder);
			}

			// Token: 0x0600BE75 RID: 48757 RVA: 0x00252E58 File Offset: 0x00251058
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			protected override Type GetRowType()
			{
				return typeof(WorkflowDataSet.WorkflowStatusRow);
			}

			// Token: 0x0600BE76 RID: 48758 RVA: 0x00252E64 File Offset: 0x00251064
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			protected override void OnRowChanged(DataRowChangeEventArgs e)
			{
				base.OnRowChanged(e);
				if (this.WorkflowStatusRowChanged != null)
				{
					this.WorkflowStatusRowChanged(this, new WorkflowDataSet.WorkflowStatusRowChangeEvent((WorkflowDataSet.WorkflowStatusRow)e.Row, e.Action));
				}
			}

			// Token: 0x0600BE77 RID: 48759 RVA: 0x00252E97 File Offset: 0x00251097
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			protected override void OnRowChanging(DataRowChangeEventArgs e)
			{
				base.OnRowChanging(e);
				if (this.WorkflowStatusRowChanging != null)
				{
					this.WorkflowStatusRowChanging(this, new WorkflowDataSet.WorkflowStatusRowChangeEvent((WorkflowDataSet.WorkflowStatusRow)e.Row, e.Action));
				}
			}

			// Token: 0x0600BE78 RID: 48760 RVA: 0x00252ECA File Offset: 0x002510CA
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			protected override void OnRowDeleted(DataRowChangeEventArgs e)
			{
				base.OnRowDeleted(e);
				if (this.WorkflowStatusRowDeleted != null)
				{
					this.WorkflowStatusRowDeleted(this, new WorkflowDataSet.WorkflowStatusRowChangeEvent((WorkflowDataSet.WorkflowStatusRow)e.Row, e.Action));
				}
			}

			// Token: 0x0600BE79 RID: 48761 RVA: 0x00252EFD File Offset: 0x002510FD
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			protected override void OnRowDeleting(DataRowChangeEventArgs e)
			{
				base.OnRowDeleting(e);
				if (this.WorkflowStatusRowDeleting != null)
				{
					this.WorkflowStatusRowDeleting(this, new WorkflowDataSet.WorkflowStatusRowChangeEvent((WorkflowDataSet.WorkflowStatusRow)e.Row, e.Action));
				}
			}

			// Token: 0x0600BE7A RID: 48762 RVA: 0x00252F30 File Offset: 0x00251130
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			public void RemoveWorkflowStatusRow(WorkflowDataSet.WorkflowStatusRow row)
			{
				base.Rows.Remove(row);
			}

			// Token: 0x0600BE7B RID: 48763 RVA: 0x00252F40 File Offset: 0x00251140
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			public static XmlSchemaComplexType GetTypedTableSchema(XmlSchemaSet xs)
			{
				XmlSchemaComplexType xmlSchemaComplexType = new XmlSchemaComplexType();
				XmlSchemaSequence xmlSchemaSequence = new XmlSchemaSequence();
				WorkflowDataSet workflowDataSet = new WorkflowDataSet();
				XmlSchemaAny xmlSchemaAny = new XmlSchemaAny();
				xmlSchemaAny.Namespace = "http://www.w3.org/2001/XMLSchema";
				xmlSchemaAny.MinOccurs = 0m;
				xmlSchemaAny.MaxOccurs = decimal.MaxValue;
				xmlSchemaAny.ProcessContents = XmlSchemaContentProcessing.Lax;
				xmlSchemaSequence.Items.Add(xmlSchemaAny);
				XmlSchemaAny xmlSchemaAny2 = new XmlSchemaAny();
				xmlSchemaAny2.Namespace = "urn:schemas-microsoft-com:xml-diffgram-v1";
				xmlSchemaAny2.MinOccurs = 1m;
				xmlSchemaAny2.ProcessContents = XmlSchemaContentProcessing.Lax;
				xmlSchemaSequence.Items.Add(xmlSchemaAny2);
				XmlSchemaAttribute xmlSchemaAttribute = new XmlSchemaAttribute();
				xmlSchemaAttribute.Name = "namespace";
				xmlSchemaAttribute.FixedValue = workflowDataSet.Namespace;
				xmlSchemaComplexType.Attributes.Add(xmlSchemaAttribute);
				XmlSchemaAttribute xmlSchemaAttribute2 = new XmlSchemaAttribute();
				xmlSchemaAttribute2.Name = "tableTypeName";
				xmlSchemaAttribute2.FixedValue = "WorkflowStatusDataTable";
				xmlSchemaComplexType.Attributes.Add(xmlSchemaAttribute2);
				xmlSchemaComplexType.Particle = xmlSchemaSequence;
				XmlSchema schemaSerializable = workflowDataSet.GetSchemaSerializable();
				if (xs.Contains(schemaSerializable.TargetNamespace))
				{
					MemoryStream memoryStream = new MemoryStream();
					MemoryStream memoryStream2 = new MemoryStream();
					try
					{
						schemaSerializable.Write(memoryStream);
						foreach (object obj in xs.Schemas(schemaSerializable.TargetNamespace))
						{
							XmlSchema xmlSchema = (XmlSchema)obj;
							memoryStream2.SetLength(0L);
							xmlSchema.Write(memoryStream2);
							if (memoryStream.Length == memoryStream2.Length)
							{
								memoryStream.Position = 0L;
								memoryStream2.Position = 0L;
								while (memoryStream.Position != memoryStream.Length && memoryStream.ReadByte() == memoryStream2.ReadByte())
								{
								}
								if (memoryStream.Position == memoryStream.Length)
								{
									return xmlSchemaComplexType;
								}
							}
						}
					}
					finally
					{
						if (memoryStream != null)
						{
							memoryStream.Close();
						}
						if (memoryStream2 != null)
						{
							memoryStream2.Close();
						}
					}
				}
				xs.Add(schemaSerializable);
				return xmlSchemaComplexType;
			}

			// Token: 0x04002676 RID: 9846
			private DataColumn columnWORKFLOW_INSTANCE_UID;

			// Token: 0x04002677 RID: 9847
			private DataColumn columnPROJ_UID;

			// Token: 0x04002678 RID: 9848
			private DataColumn columnSTAGE_UID;

			// Token: 0x04002679 RID: 9849
			private DataColumn columnSTAGE_NAME;

			// Token: 0x0400267A RID: 9850
			private DataColumn columnPHASE_UID;

			// Token: 0x0400267B RID: 9851
			private DataColumn columnPHASE_NAME;

			// Token: 0x0400267C RID: 9852
			private DataColumn columnSTAGE_STATUS;

			// Token: 0x0400267D RID: 9853
			private DataColumn columnSTAGE_INFO;

			// Token: 0x0400267E RID: 9854
			private DataColumn columnSTAGE_ORDER;

			// Token: 0x0400267F RID: 9855
			private DataColumn columnCREATED_DATE;

			// Token: 0x04002680 RID: 9856
			private DataColumn columnMOD_DATE;

			// Token: 0x04002681 RID: 9857
			private DataColumn columnSTAGE_ENTRY_DATE;

			// Token: 0x04002682 RID: 9858
			private DataColumn columnSTAGE_COMPLETION_DATE;

			// Token: 0x04002683 RID: 9859
			private DataColumn columnWORKFLOW_MOD_DATE;

			// Token: 0x04002684 RID: 9860
			private DataColumn columnSUBMITTED_DATE;

			// Token: 0x04002685 RID: 9861
			private DataColumn columnNEXT_STAGE1;

			// Token: 0x04002686 RID: 9862
			private DataColumn columnNEXT_STAGE2;
		}

		// Token: 0x020007B2 RID: 1970
		[XmlSchemaProvider("GetTypedTableSchema")]
		[Serializable]
		public class EnterpriseProjectTypeDataTable : DataTable, IEnumerable
		{
			// Token: 0x0600BE7C RID: 48764 RVA: 0x00253138 File Offset: 0x00251338
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public EnterpriseProjectTypeDataTable()
			{
				base.TableName = "EnterpriseProjectType";
				this.BeginInit();
				this.InitClass();
				this.EndInit();
			}

			// Token: 0x0600BE7D RID: 48765 RVA: 0x00253160 File Offset: 0x00251360
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			internal EnterpriseProjectTypeDataTable(DataTable table)
			{
				base.TableName = table.TableName;
				if (table.CaseSensitive != table.DataSet.CaseSensitive)
				{
					base.CaseSensitive = table.CaseSensitive;
				}
				if (table.Locale.ToString() != table.DataSet.Locale.ToString())
				{
					base.Locale = table.Locale;
				}
				if (table.Namespace != table.DataSet.Namespace)
				{
					base.Namespace = table.Namespace;
				}
				base.Prefix = table.Prefix;
				base.MinimumCapacity = table.MinimumCapacity;
			}

			// Token: 0x0600BE7E RID: 48766 RVA: 0x00253208 File Offset: 0x00251408
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			protected EnterpriseProjectTypeDataTable(SerializationInfo info, StreamingContext context) : base(info, context)
			{
				this.InitVars();
			}

			// Token: 0x17003A39 RID: 14905
			// (get) Token: 0x0600BE7F RID: 48767 RVA: 0x00253218 File Offset: 0x00251418
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public DataColumn ENTERPRISE_PROJECT_TYPE_UIDColumn
			{
				get
				{
					return this.columnENTERPRISE_PROJECT_TYPE_UID;
				}
			}

			// Token: 0x17003A3A RID: 14906
			// (get) Token: 0x0600BE80 RID: 48768 RVA: 0x00253220 File Offset: 0x00251420
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public DataColumn ENTERPRISE_PROJECT_TYPE_NAMEColumn
			{
				get
				{
					return this.columnENTERPRISE_PROJECT_TYPE_NAME;
				}
			}

			// Token: 0x17003A3B RID: 14907
			// (get) Token: 0x0600BE81 RID: 48769 RVA: 0x00253228 File Offset: 0x00251428
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public DataColumn ENTERPRISE_PROJECT_TYPE_DESCRIPTIONColumn
			{
				get
				{
					return this.columnENTERPRISE_PROJECT_TYPE_DESCRIPTION;
				}
			}

			// Token: 0x17003A3C RID: 14908
			// (get) Token: 0x0600BE82 RID: 48770 RVA: 0x00253230 File Offset: 0x00251430
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public DataColumn WORKFLOW_ASSOCIATION_UIDColumn
			{
				get
				{
					return this.columnWORKFLOW_ASSOCIATION_UID;
				}
			}

			// Token: 0x17003A3D RID: 14909
			// (get) Token: 0x0600BE83 RID: 48771 RVA: 0x00253238 File Offset: 0x00251438
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public DataColumn WORKFLOW_ASSOCIATION_NAMEColumn
			{
				get
				{
					return this.columnWORKFLOW_ASSOCIATION_NAME;
				}
			}

			// Token: 0x17003A3E RID: 14910
			// (get) Token: 0x0600BE84 RID: 48772 RVA: 0x00253240 File Offset: 0x00251440
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public DataColumn IS_DEFAULT_PROJECT_TYPEColumn
			{
				get
				{
					return this.columnIS_DEFAULT_PROJECT_TYPE;
				}
			}

			// Token: 0x17003A3F RID: 14911
			// (get) Token: 0x0600BE85 RID: 48773 RVA: 0x00253248 File Offset: 0x00251448
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public DataColumn ENTERPRISE_PROJECT_PLAN_TEMPLATE_UIDColumn
			{
				get
				{
					return this.columnENTERPRISE_PROJECT_PLAN_TEMPLATE_UID;
				}
			}

			// Token: 0x17003A40 RID: 14912
			// (get) Token: 0x0600BE86 RID: 48774 RVA: 0x00253250 File Offset: 0x00251450
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public DataColumn ENTERPRISE_PROJECT_WORKSPACE_TEMPLATE_NAMEColumn
			{
				get
				{
					return this.columnENTERPRISE_PROJECT_WORKSPACE_TEMPLATE_NAME;
				}
			}

			// Token: 0x17003A41 RID: 14913
			// (get) Token: 0x0600BE87 RID: 48775 RVA: 0x00253258 File Offset: 0x00251458
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public DataColumn ENTERPRISE_PROJECT_TYPE_ORDERColumn
			{
				get
				{
					return this.columnENTERPRISE_PROJECT_TYPE_ORDER;
				}
			}

			// Token: 0x17003A42 RID: 14914
			// (get) Token: 0x0600BE88 RID: 48776 RVA: 0x00253260 File Offset: 0x00251460
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public DataColumn ENTERPRISE_PROJECT_TYPE_IMAGE_URLColumn
			{
				get
				{
					return this.columnENTERPRISE_PROJECT_TYPE_IMAGE_URL;
				}
			}

			// Token: 0x17003A43 RID: 14915
			// (get) Token: 0x0600BE89 RID: 48777 RVA: 0x00253268 File Offset: 0x00251468
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public DataColumn IS_MANAGED_PROJECTColumn
			{
				get
				{
					return this.columnIS_MANAGED_PROJECT;
				}
			}

			// Token: 0x17003A44 RID: 14916
			// (get) Token: 0x0600BE8A RID: 48778 RVA: 0x00253270 File Offset: 0x00251470
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public DataColumn PROJ_IDENTIFIER_PREFIXColumn
			{
				get
				{
					return this.columnPROJ_IDENTIFIER_PREFIX;
				}
			}

			// Token: 0x17003A45 RID: 14917
			// (get) Token: 0x0600BE8B RID: 48779 RVA: 0x00253278 File Offset: 0x00251478
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public DataColumn PROJ_IDENTIFIER_SEEDColumn
			{
				get
				{
					return this.columnPROJ_IDENTIFIER_SEED;
				}
			}

			// Token: 0x17003A46 RID: 14918
			// (get) Token: 0x0600BE8C RID: 48780 RVA: 0x00253280 File Offset: 0x00251480
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public DataColumn PROJ_IDENTIFIER_POSTFIXColumn
			{
				get
				{
					return this.columnPROJ_IDENTIFIER_POSTFIX;
				}
			}

			// Token: 0x17003A47 RID: 14919
			// (get) Token: 0x0600BE8D RID: 48781 RVA: 0x00253288 File Offset: 0x00251488
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public DataColumn PROJ_IDENTIFIER_MINDIGITColumn
			{
				get
				{
					return this.columnPROJ_IDENTIFIER_MINDIGIT;
				}
			}

			// Token: 0x17003A48 RID: 14920
			// (get) Token: 0x0600BE8E RID: 48782 RVA: 0x00253290 File Offset: 0x00251490
			[Browsable(false)]
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public int Count
			{
				get
				{
					return base.Rows.Count;
				}
			}

			// Token: 0x140006C9 RID: 1737
			// (add) Token: 0x0600BE90 RID: 48784 RVA: 0x002532B0 File Offset: 0x002514B0
			// (remove) Token: 0x0600BE91 RID: 48785 RVA: 0x002532E8 File Offset: 0x002514E8
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public event WorkflowDataSet.EnterpriseProjectTypeRowChangeEventHandler EnterpriseProjectTypeRowChanging;

			// Token: 0x140006CA RID: 1738
			// (add) Token: 0x0600BE92 RID: 48786 RVA: 0x00253320 File Offset: 0x00251520
			// (remove) Token: 0x0600BE93 RID: 48787 RVA: 0x00253358 File Offset: 0x00251558
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public event WorkflowDataSet.EnterpriseProjectTypeRowChangeEventHandler EnterpriseProjectTypeRowChanged;

			// Token: 0x140006CB RID: 1739
			// (add) Token: 0x0600BE94 RID: 48788 RVA: 0x00253390 File Offset: 0x00251590
			// (remove) Token: 0x0600BE95 RID: 48789 RVA: 0x002533C8 File Offset: 0x002515C8
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public event WorkflowDataSet.EnterpriseProjectTypeRowChangeEventHandler EnterpriseProjectTypeRowDeleting;

			// Token: 0x140006CC RID: 1740
			// (add) Token: 0x0600BE96 RID: 48790 RVA: 0x00253400 File Offset: 0x00251600
			// (remove) Token: 0x0600BE97 RID: 48791 RVA: 0x00253438 File Offset: 0x00251638
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public event WorkflowDataSet.EnterpriseProjectTypeRowChangeEventHandler EnterpriseProjectTypeRowDeleted;

			// Token: 0x0600BE98 RID: 48792 RVA: 0x0025346D File Offset: 0x0025166D
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			public void AddEnterpriseProjectTypeRow(WorkflowDataSet.EnterpriseProjectTypeRow row)
			{
				base.Rows.Add(row);
			}

			// Token: 0x0600BE99 RID: 48793 RVA: 0x0025347C File Offset: 0x0025167C
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public WorkflowDataSet.EnterpriseProjectTypeRow AddEnterpriseProjectTypeRow(Guid ENTERPRISE_PROJECT_TYPE_UID, string ENTERPRISE_PROJECT_TYPE_NAME, string ENTERPRISE_PROJECT_TYPE_DESCRIPTION, Guid WORKFLOW_ASSOCIATION_UID, string WORKFLOW_ASSOCIATION_NAME, bool IS_DEFAULT_PROJECT_TYPE, Guid ENTERPRISE_PROJECT_PLAN_TEMPLATE_UID, string ENTERPRISE_PROJECT_WORKSPACE_TEMPLATE_NAME, int ENTERPRISE_PROJECT_TYPE_ORDER, string ENTERPRISE_PROJECT_TYPE_IMAGE_URL, bool IS_MANAGED_PROJECT, string PROJ_IDENTIFIER_PREFIX, int PROJ_IDENTIFIER_SEED, string PROJ_IDENTIFIER_POSTFIX, int PROJ_IDENTIFIER_MINDIGIT)
			{
				WorkflowDataSet.EnterpriseProjectTypeRow enterpriseProjectTypeRow = (WorkflowDataSet.EnterpriseProjectTypeRow)base.NewRow();
				object[] itemArray = new object[]
				{
					ENTERPRISE_PROJECT_TYPE_UID,
					ENTERPRISE_PROJECT_TYPE_NAME,
					ENTERPRISE_PROJECT_TYPE_DESCRIPTION,
					WORKFLOW_ASSOCIATION_UID,
					WORKFLOW_ASSOCIATION_NAME,
					IS_DEFAULT_PROJECT_TYPE,
					ENTERPRISE_PROJECT_PLAN_TEMPLATE_UID,
					ENTERPRISE_PROJECT_WORKSPACE_TEMPLATE_NAME,
					ENTERPRISE_PROJECT_TYPE_ORDER,
					ENTERPRISE_PROJECT_TYPE_IMAGE_URL,
					IS_MANAGED_PROJECT,
					PROJ_IDENTIFIER_PREFIX,
					PROJ_IDENTIFIER_SEED,
					PROJ_IDENTIFIER_POSTFIX,
					PROJ_IDENTIFIER_MINDIGIT
				};
				enterpriseProjectTypeRow.ItemArray = itemArray;
				base.Rows.Add(enterpriseProjectTypeRow);
				return enterpriseProjectTypeRow;
			}

			// Token: 0x0600BE9A RID: 48794 RVA: 0x0025352C File Offset: 0x0025172C
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			public WorkflowDataSet.EnterpriseProjectTypeRow FindByENTERPRISE_PROJECT_TYPE_UID(Guid ENTERPRISE_PROJECT_TYPE_UID)
			{
				return (WorkflowDataSet.EnterpriseProjectTypeRow)base.Rows.Find(new object[]
				{
					ENTERPRISE_PROJECT_TYPE_UID
				});
			}

			// Token: 0x0600BE9B RID: 48795 RVA: 0x0025355A File Offset: 0x0025175A
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public virtual IEnumerator GetEnumerator()
			{
				return base.Rows.GetEnumerator();
			}

			// Token: 0x0600BE9C RID: 48796 RVA: 0x00253568 File Offset: 0x00251768
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			public override DataTable Clone()
			{
				WorkflowDataSet.EnterpriseProjectTypeDataTable enterpriseProjectTypeDataTable = (WorkflowDataSet.EnterpriseProjectTypeDataTable)base.Clone();
				enterpriseProjectTypeDataTable.InitVars();
				return enterpriseProjectTypeDataTable;
			}

			// Token: 0x0600BE9D RID: 48797 RVA: 0x00253588 File Offset: 0x00251788
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			protected override DataTable CreateInstance()
			{
				return new WorkflowDataSet.EnterpriseProjectTypeDataTable();
			}

			// Token: 0x0600BE9E RID: 48798 RVA: 0x00253590 File Offset: 0x00251790
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			internal void InitVars()
			{
				this.columnENTERPRISE_PROJECT_TYPE_UID = base.Columns["ENTERPRISE_PROJECT_TYPE_UID"];
				this.columnENTERPRISE_PROJECT_TYPE_NAME = base.Columns["ENTERPRISE_PROJECT_TYPE_NAME"];
				this.columnENTERPRISE_PROJECT_TYPE_DESCRIPTION = base.Columns["ENTERPRISE_PROJECT_TYPE_DESCRIPTION"];
				this.columnWORKFLOW_ASSOCIATION_UID = base.Columns["WORKFLOW_ASSOCIATION_UID"];
				this.columnWORKFLOW_ASSOCIATION_NAME = base.Columns["WORKFLOW_ASSOCIATION_NAME"];
				this.columnIS_DEFAULT_PROJECT_TYPE = base.Columns["IS_DEFAULT_PROJECT_TYPE"];
				this.columnENTERPRISE_PROJECT_PLAN_TEMPLATE_UID = base.Columns["ENTERPRISE_PROJECT_PLAN_TEMPLATE_UID"];
				this.columnENTERPRISE_PROJECT_WORKSPACE_TEMPLATE_NAME = base.Columns["ENTERPRISE_PROJECT_WORKSPACE_TEMPLATE_NAME"];
				this.columnENTERPRISE_PROJECT_TYPE_ORDER = base.Columns["ENTERPRISE_PROJECT_TYPE_ORDER"];
				this.columnENTERPRISE_PROJECT_TYPE_IMAGE_URL = base.Columns["ENTERPRISE_PROJECT_TYPE_IMAGE_URL"];
				this.columnIS_MANAGED_PROJECT = base.Columns["IS_MANAGED_PROJECT"];
				this.columnPROJ_IDENTIFIER_PREFIX = base.Columns["PROJ_IDENTIFIER_PREFIX"];
				this.columnPROJ_IDENTIFIER_SEED = base.Columns["PROJ_IDENTIFIER_SEED"];
				this.columnPROJ_IDENTIFIER_POSTFIX = base.Columns["PROJ_IDENTIFIER_POSTFIX"];
				this.columnPROJ_IDENTIFIER_MINDIGIT = base.Columns["PROJ_IDENTIFIER_MINDIGIT"];
			}

			// Token: 0x0600BE9F RID: 48799 RVA: 0x002536E8 File Offset: 0x002518E8
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			private void InitClass()
			{
				this.columnENTERPRISE_PROJECT_TYPE_UID = new DataColumn("ENTERPRISE_PROJECT_TYPE_UID", typeof(Guid), null, MappingType.Element);
				base.Columns.Add(this.columnENTERPRISE_PROJECT_TYPE_UID);
				this.columnENTERPRISE_PROJECT_TYPE_NAME = new DataColumn("ENTERPRISE_PROJECT_TYPE_NAME", typeof(string), null, MappingType.Element);
				base.Columns.Add(this.columnENTERPRISE_PROJECT_TYPE_NAME);
				this.columnENTERPRISE_PROJECT_TYPE_DESCRIPTION = new DataColumn("ENTERPRISE_PROJECT_TYPE_DESCRIPTION", typeof(string), null, MappingType.Element);
				base.Columns.Add(this.columnENTERPRISE_PROJECT_TYPE_DESCRIPTION);
				this.columnWORKFLOW_ASSOCIATION_UID = new DataColumn("WORKFLOW_ASSOCIATION_UID", typeof(Guid), null, MappingType.Element);
				base.Columns.Add(this.columnWORKFLOW_ASSOCIATION_UID);
				this.columnWORKFLOW_ASSOCIATION_NAME = new DataColumn("WORKFLOW_ASSOCIATION_NAME", typeof(string), null, MappingType.Element);
				base.Columns.Add(this.columnWORKFLOW_ASSOCIATION_NAME);
				this.columnIS_DEFAULT_PROJECT_TYPE = new DataColumn("IS_DEFAULT_PROJECT_TYPE", typeof(bool), null, MappingType.Element);
				base.Columns.Add(this.columnIS_DEFAULT_PROJECT_TYPE);
				this.columnENTERPRISE_PROJECT_PLAN_TEMPLATE_UID = new DataColumn("ENTERPRISE_PROJECT_PLAN_TEMPLATE_UID", typeof(Guid), null, MappingType.Element);
				base.Columns.Add(this.columnENTERPRISE_PROJECT_PLAN_TEMPLATE_UID);
				this.columnENTERPRISE_PROJECT_WORKSPACE_TEMPLATE_NAME = new DataColumn("ENTERPRISE_PROJECT_WORKSPACE_TEMPLATE_NAME", typeof(string), null, MappingType.Element);
				base.Columns.Add(this.columnENTERPRISE_PROJECT_WORKSPACE_TEMPLATE_NAME);
				this.columnENTERPRISE_PROJECT_TYPE_ORDER = new DataColumn("ENTERPRISE_PROJECT_TYPE_ORDER", typeof(int), null, MappingType.Element);
				base.Columns.Add(this.columnENTERPRISE_PROJECT_TYPE_ORDER);
				this.columnENTERPRISE_PROJECT_TYPE_IMAGE_URL = new DataColumn("ENTERPRISE_PROJECT_TYPE_IMAGE_URL", typeof(string), null, MappingType.Element);
				base.Columns.Add(this.columnENTERPRISE_PROJECT_TYPE_IMAGE_URL);
				this.columnIS_MANAGED_PROJECT = new DataColumn("IS_MANAGED_PROJECT", typeof(bool), null, MappingType.Element);
				base.Columns.Add(this.columnIS_MANAGED_PROJECT);
				this.columnPROJ_IDENTIFIER_PREFIX = new DataColumn("PROJ_IDENTIFIER_PREFIX", typeof(string), null, MappingType.Element);
				base.Columns.Add(this.columnPROJ_IDENTIFIER_PREFIX);
				this.columnPROJ_IDENTIFIER_SEED = new DataColumn("PROJ_IDENTIFIER_SEED", typeof(int), null, MappingType.Element);
				base.Columns.Add(this.columnPROJ_IDENTIFIER_SEED);
				this.columnPROJ_IDENTIFIER_POSTFIX = new DataColumn("PROJ_IDENTIFIER_POSTFIX", typeof(string), null, MappingType.Element);
				base.Columns.Add(this.columnPROJ_IDENTIFIER_POSTFIX);
				this.columnPROJ_IDENTIFIER_MINDIGIT = new DataColumn("PROJ_IDENTIFIER_MINDIGIT", typeof(int), null, MappingType.Element);
				base.Columns.Add(this.columnPROJ_IDENTIFIER_MINDIGIT);
				base.Constraints.Add(new UniqueConstraint("Constraint1", new DataColumn[]
				{
					this.columnENTERPRISE_PROJECT_TYPE_UID
				}, true));
				this.columnENTERPRISE_PROJECT_TYPE_UID.AllowDBNull = false;
				this.columnENTERPRISE_PROJECT_TYPE_UID.Unique = true;
				this.columnENTERPRISE_PROJECT_TYPE_NAME.AllowDBNull = false;
				this.columnIS_DEFAULT_PROJECT_TYPE.AllowDBNull = false;
				this.columnIS_DEFAULT_PROJECT_TYPE.DefaultValue = false;
				this.columnENTERPRISE_PROJECT_WORKSPACE_TEMPLATE_NAME.AllowDBNull = false;
				this.columnENTERPRISE_PROJECT_TYPE_ORDER.AllowDBNull = false;
				this.columnENTERPRISE_PROJECT_TYPE_ORDER.DefaultValue = 0;
				this.columnIS_MANAGED_PROJECT.DefaultValue = true;
				this.columnPROJ_IDENTIFIER_SEED.DefaultValue = 0;
				this.columnPROJ_IDENTIFIER_MINDIGIT.DefaultValue = 1;
			}

			// Token: 0x0600BEA0 RID: 48800 RVA: 0x00253A5C File Offset: 0x00251C5C
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			public WorkflowDataSet.EnterpriseProjectTypeRow NewEnterpriseProjectTypeRow()
			{
				return (WorkflowDataSet.EnterpriseProjectTypeRow)base.NewRow();
			}

			// Token: 0x0600BEA1 RID: 48801 RVA: 0x00253A69 File Offset: 0x00251C69
			/*[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			protected override DataRow NewRowFromBuilder(DataRowBuilder builder)
			{
				return new WorkflowDataSet.EnterpriseProjectTypeRow(builder);
			}

			// Token: 0x0600BEA2 RID: 48802 RVA: 0x00253A71 File Offset: 0x00251C71
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			protected override Type GetRowType()
			{
				return typeof(WorkflowDataSet.EnterpriseProjectTypeRow);
			}*/

			// Token: 0x0600BEA3 RID: 48803 RVA: 0x00253A7D File Offset: 0x00251C7D
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			protected override void OnRowChanged(DataRowChangeEventArgs e)
			{
				base.OnRowChanged(e);
				if (this.EnterpriseProjectTypeRowChanged != null)
				{
					this.EnterpriseProjectTypeRowChanged(this, new WorkflowDataSet.EnterpriseProjectTypeRowChangeEvent((WorkflowDataSet.EnterpriseProjectTypeRow)e.Row, e.Action));
				}
			}

			// Token: 0x0600BEA4 RID: 48804 RVA: 0x00253AB0 File Offset: 0x00251CB0
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			protected override void OnRowChanging(DataRowChangeEventArgs e)
			{
				base.OnRowChanging(e);
				if (this.EnterpriseProjectTypeRowChanging != null)
				{
					this.EnterpriseProjectTypeRowChanging(this, new WorkflowDataSet.EnterpriseProjectTypeRowChangeEvent((WorkflowDataSet.EnterpriseProjectTypeRow)e.Row, e.Action));
				}
			}

			// Token: 0x0600BEA5 RID: 48805 RVA: 0x00253AE3 File Offset: 0x00251CE3
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			protected override void OnRowDeleted(DataRowChangeEventArgs e)
			{
				base.OnRowDeleted(e);
				if (this.EnterpriseProjectTypeRowDeleted != null)
				{
					this.EnterpriseProjectTypeRowDeleted(this, new WorkflowDataSet.EnterpriseProjectTypeRowChangeEvent((WorkflowDataSet.EnterpriseProjectTypeRow)e.Row, e.Action));
				}
			}

			// Token: 0x0600BEA6 RID: 48806 RVA: 0x00253B16 File Offset: 0x00251D16
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			protected override void OnRowDeleting(DataRowChangeEventArgs e)
			{
				base.OnRowDeleting(e);
				if (this.EnterpriseProjectTypeRowDeleting != null)
				{
					this.EnterpriseProjectTypeRowDeleting(this, new WorkflowDataSet.EnterpriseProjectTypeRowChangeEvent((WorkflowDataSet.EnterpriseProjectTypeRow)e.Row, e.Action));
				}
			}

			// Token: 0x0600BEA7 RID: 48807 RVA: 0x00253B49 File Offset: 0x00251D49
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			public void RemoveEnterpriseProjectTypeRow(WorkflowDataSet.EnterpriseProjectTypeRow row)
			{
				base.Rows.Remove(row);
			}

			// Token: 0x0600BEA8 RID: 48808 RVA: 0x00253B58 File Offset: 0x00251D58
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public static XmlSchemaComplexType GetTypedTableSchema(XmlSchemaSet xs)
			{
				XmlSchemaComplexType xmlSchemaComplexType = new XmlSchemaComplexType();
				XmlSchemaSequence xmlSchemaSequence = new XmlSchemaSequence();
				WorkflowDataSet workflowDataSet = new WorkflowDataSet();
				XmlSchemaAny xmlSchemaAny = new XmlSchemaAny();
				xmlSchemaAny.Namespace = "http://www.w3.org/2001/XMLSchema";
				xmlSchemaAny.MinOccurs = 0m;
				xmlSchemaAny.MaxOccurs = decimal.MaxValue;
				xmlSchemaAny.ProcessContents = XmlSchemaContentProcessing.Lax;
				xmlSchemaSequence.Items.Add(xmlSchemaAny);
				XmlSchemaAny xmlSchemaAny2 = new XmlSchemaAny();
				xmlSchemaAny2.Namespace = "urn:schemas-microsoft-com:xml-diffgram-v1";
				xmlSchemaAny2.MinOccurs = 1m;
				xmlSchemaAny2.ProcessContents = XmlSchemaContentProcessing.Lax;
				xmlSchemaSequence.Items.Add(xmlSchemaAny2);
				XmlSchemaAttribute xmlSchemaAttribute = new XmlSchemaAttribute();
				xmlSchemaAttribute.Name = "namespace";
				xmlSchemaAttribute.FixedValue = workflowDataSet.Namespace;
				xmlSchemaComplexType.Attributes.Add(xmlSchemaAttribute);
				XmlSchemaAttribute xmlSchemaAttribute2 = new XmlSchemaAttribute();
				xmlSchemaAttribute2.Name = "tableTypeName";
				xmlSchemaAttribute2.FixedValue = "EnterpriseProjectTypeDataTable";
				xmlSchemaComplexType.Attributes.Add(xmlSchemaAttribute2);
				xmlSchemaComplexType.Particle = xmlSchemaSequence;
				XmlSchema schemaSerializable = workflowDataSet.GetSchemaSerializable();
				if (xs.Contains(schemaSerializable.TargetNamespace))
				{
					MemoryStream memoryStream = new MemoryStream();
					MemoryStream memoryStream2 = new MemoryStream();
					try
					{
						schemaSerializable.Write(memoryStream);
						foreach (object obj in xs.Schemas(schemaSerializable.TargetNamespace))
						{
							XmlSchema xmlSchema = (XmlSchema)obj;
							memoryStream2.SetLength(0L);
							xmlSchema.Write(memoryStream2);
							if (memoryStream.Length == memoryStream2.Length)
							{
								memoryStream.Position = 0L;
								memoryStream2.Position = 0L;
								while (memoryStream.Position != memoryStream.Length && memoryStream.ReadByte() == memoryStream2.ReadByte())
								{
								}
								if (memoryStream.Position == memoryStream.Length)
								{
									return xmlSchemaComplexType;
								}
							}
						}
					}
					finally
					{
						if (memoryStream != null)
						{
							memoryStream.Close();
						}
						if (memoryStream2 != null)
						{
							memoryStream2.Close();
						}
					}
				}
				xs.Add(schemaSerializable);
				return xmlSchemaComplexType;
			}

			// Token: 0x0400268B RID: 9867
			private DataColumn columnENTERPRISE_PROJECT_TYPE_UID;

			// Token: 0x0400268C RID: 9868
			private DataColumn columnENTERPRISE_PROJECT_TYPE_NAME;

			// Token: 0x0400268D RID: 9869
			private DataColumn columnENTERPRISE_PROJECT_TYPE_DESCRIPTION;

			// Token: 0x0400268E RID: 9870
			private DataColumn columnWORKFLOW_ASSOCIATION_UID;

			// Token: 0x0400268F RID: 9871
			private DataColumn columnWORKFLOW_ASSOCIATION_NAME;

			// Token: 0x04002690 RID: 9872
			private DataColumn columnIS_DEFAULT_PROJECT_TYPE;

			// Token: 0x04002691 RID: 9873
			private DataColumn columnENTERPRISE_PROJECT_PLAN_TEMPLATE_UID;

			// Token: 0x04002692 RID: 9874
			private DataColumn columnENTERPRISE_PROJECT_WORKSPACE_TEMPLATE_NAME;

			// Token: 0x04002693 RID: 9875
			private DataColumn columnENTERPRISE_PROJECT_TYPE_ORDER;

			// Token: 0x04002694 RID: 9876
			private DataColumn columnENTERPRISE_PROJECT_TYPE_IMAGE_URL;

			// Token: 0x04002695 RID: 9877
			private DataColumn columnIS_MANAGED_PROJECT;

			// Token: 0x04002696 RID: 9878
			private DataColumn columnPROJ_IDENTIFIER_PREFIX;

			// Token: 0x04002697 RID: 9879
			private DataColumn columnPROJ_IDENTIFIER_SEED;

			// Token: 0x04002698 RID: 9880
			private DataColumn columnPROJ_IDENTIFIER_POSTFIX;

			// Token: 0x04002699 RID: 9881
			private DataColumn columnPROJ_IDENTIFIER_MINDIGIT;
		}

		// Token: 0x020007B3 RID: 1971
		[XmlSchemaProvider("GetTypedTableSchema")]
		[Serializable]
		public class EnterpriseProjectTypeDepartmentsDataTable : DataTable, IEnumerable
		{
			// Token: 0x0600BEA9 RID: 48809 RVA: 0x00253D50 File Offset: 0x00251F50
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public EnterpriseProjectTypeDepartmentsDataTable()
			{
				base.TableName = "EnterpriseProjectTypeDepartments";
				this.BeginInit();
				this.InitClass();
				this.EndInit();
			}

			// Token: 0x0600BEAA RID: 48810 RVA: 0x00253D78 File Offset: 0x00251F78
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			internal EnterpriseProjectTypeDepartmentsDataTable(DataTable table)
			{
				base.TableName = table.TableName;
				if (table.CaseSensitive != table.DataSet.CaseSensitive)
				{
					base.CaseSensitive = table.CaseSensitive;
				}
				if (table.Locale.ToString() != table.DataSet.Locale.ToString())
				{
					base.Locale = table.Locale;
				}
				if (table.Namespace != table.DataSet.Namespace)
				{
					base.Namespace = table.Namespace;
				}
				base.Prefix = table.Prefix;
				base.MinimumCapacity = table.MinimumCapacity;
			}

			// Token: 0x0600BEAB RID: 48811 RVA: 0x00253E20 File Offset: 0x00252020
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			protected EnterpriseProjectTypeDepartmentsDataTable(SerializationInfo info, StreamingContext context) : base(info, context)
			{
				this.InitVars();
			}

			// Token: 0x17003A4A RID: 14922
			// (get) Token: 0x0600BEAC RID: 48812 RVA: 0x00253E30 File Offset: 0x00252030
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			public DataColumn ENTERPRISE_PROJECT_TYPE_UIDColumn
			{
				get
				{
					return this.columnENTERPRISE_PROJECT_TYPE_UID;
				}
			}

			// Token: 0x17003A4B RID: 14923
			// (get) Token: 0x0600BEAD RID: 48813 RVA: 0x00253E38 File Offset: 0x00252038
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public DataColumn DEPARTMENT_UIDColumn
			{
				get
				{
					return this.columnDEPARTMENT_UID;
				}
			}

			// Token: 0x17003A4C RID: 14924
			// (get) Token: 0x0600BEAE RID: 48814 RVA: 0x00253E40 File Offset: 0x00252040
			[DebuggerNonUserCode]
			[Browsable(false)]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public int Count
			{
				get
				{
					return base.Rows.Count;
				}
			}

			// Token: 0x140006CD RID: 1741
			// (add) Token: 0x0600BEB0 RID: 48816 RVA: 0x00253E60 File Offset: 0x00252060
			// (remove) Token: 0x0600BEB1 RID: 48817 RVA: 0x00253E98 File Offset: 0x00252098
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public event WorkflowDataSet.EnterpriseProjectTypeDepartmentsRowChangeEventHandler EnterpriseProjectTypeDepartmentsRowChanging;

			// Token: 0x140006CE RID: 1742
			// (add) Token: 0x0600BEB2 RID: 48818 RVA: 0x00253ED0 File Offset: 0x002520D0
			// (remove) Token: 0x0600BEB3 RID: 48819 RVA: 0x00253F08 File Offset: 0x00252108
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public event WorkflowDataSet.EnterpriseProjectTypeDepartmentsRowChangeEventHandler EnterpriseProjectTypeDepartmentsRowChanged;

			// Token: 0x140006CF RID: 1743
			// (add) Token: 0x0600BEB4 RID: 48820 RVA: 0x00253F40 File Offset: 0x00252140
			// (remove) Token: 0x0600BEB5 RID: 48821 RVA: 0x00253F78 File Offset: 0x00252178
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public event WorkflowDataSet.EnterpriseProjectTypeDepartmentsRowChangeEventHandler EnterpriseProjectTypeDepartmentsRowDeleting;

			// Token: 0x140006D0 RID: 1744
			// (add) Token: 0x0600BEB6 RID: 48822 RVA: 0x00253FB0 File Offset: 0x002521B0
			// (remove) Token: 0x0600BEB7 RID: 48823 RVA: 0x00253FE8 File Offset: 0x002521E8
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public event WorkflowDataSet.EnterpriseProjectTypeDepartmentsRowChangeEventHandler EnterpriseProjectTypeDepartmentsRowDeleted;

			// Token: 0x0600BEB8 RID: 48824 RVA: 0x0025401D File Offset: 0x0025221D
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public void AddEnterpriseProjectTypeDepartmentsRow(WorkflowDataSet.EnterpriseProjectTypeDepartmentsRow row)
			{
				base.Rows.Add(row);
			}

			// Token: 0x0600BEB9 RID: 48825 RVA: 0x0025402C File Offset: 0x0025222C
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public WorkflowDataSet.EnterpriseProjectTypeDepartmentsRow AddEnterpriseProjectTypeDepartmentsRow(WorkflowDataSet.EnterpriseProjectTypeRow parentEnterpriseProjectTypeRowByEnterpriseProjectType_EnterpriseProjectTypeDepartments, Guid DEPARTMENT_UID)
			{
				WorkflowDataSet.EnterpriseProjectTypeDepartmentsRow enterpriseProjectTypeDepartmentsRow = (WorkflowDataSet.EnterpriseProjectTypeDepartmentsRow)base.NewRow();
				object[] array = new object[]
				{
					null,
					DEPARTMENT_UID
				};
				if (parentEnterpriseProjectTypeRowByEnterpriseProjectType_EnterpriseProjectTypeDepartments != null)
				{
					array[0] = parentEnterpriseProjectTypeRowByEnterpriseProjectType_EnterpriseProjectTypeDepartments[0];
				}
				enterpriseProjectTypeDepartmentsRow.ItemArray = array;
				base.Rows.Add(enterpriseProjectTypeDepartmentsRow);
				return enterpriseProjectTypeDepartmentsRow;
			}

			// Token: 0x0600BEBA RID: 48826 RVA: 0x00254078 File Offset: 0x00252278
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			public WorkflowDataSet.EnterpriseProjectTypeDepartmentsRow FindByENTERPRISE_PROJECT_TYPE_UIDDEPARTMENT_UID(Guid ENTERPRISE_PROJECT_TYPE_UID, Guid DEPARTMENT_UID)
			{
				return (WorkflowDataSet.EnterpriseProjectTypeDepartmentsRow)base.Rows.Find(new object[]
				{
					ENTERPRISE_PROJECT_TYPE_UID,
					DEPARTMENT_UID
				});
			}

			// Token: 0x0600BEBB RID: 48827 RVA: 0x002540AF File Offset: 0x002522AF
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public virtual IEnumerator GetEnumerator()
			{
				return base.Rows.GetEnumerator();
			}

			// Token: 0x0600BEBC RID: 48828 RVA: 0x002540BC File Offset: 0x002522BC
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			public override DataTable Clone()
			{
				WorkflowDataSet.EnterpriseProjectTypeDepartmentsDataTable enterpriseProjectTypeDepartmentsDataTable = (WorkflowDataSet.EnterpriseProjectTypeDepartmentsDataTable)base.Clone();
				enterpriseProjectTypeDepartmentsDataTable.InitVars();
				return enterpriseProjectTypeDepartmentsDataTable;
			}

			// Token: 0x0600BEBD RID: 48829 RVA: 0x002540DC File Offset: 0x002522DC
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			protected override DataTable CreateInstance()
			{
				return new WorkflowDataSet.EnterpriseProjectTypeDepartmentsDataTable();
			}

			// Token: 0x0600BEBE RID: 48830 RVA: 0x002540E3 File Offset: 0x002522E3
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			internal void InitVars()
			{
				this.columnENTERPRISE_PROJECT_TYPE_UID = base.Columns["ENTERPRISE_PROJECT_TYPE_UID"];
				this.columnDEPARTMENT_UID = base.Columns["DEPARTMENT_UID"];
			}

			// Token: 0x0600BEBF RID: 48831 RVA: 0x00254114 File Offset: 0x00252314
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			private void InitClass()
			{
				this.columnENTERPRISE_PROJECT_TYPE_UID = new DataColumn("ENTERPRISE_PROJECT_TYPE_UID", typeof(Guid), null, MappingType.Element);
				base.Columns.Add(this.columnENTERPRISE_PROJECT_TYPE_UID);
				this.columnDEPARTMENT_UID = new DataColumn("DEPARTMENT_UID", typeof(Guid), null, MappingType.Element);
				base.Columns.Add(this.columnDEPARTMENT_UID);
				base.Constraints.Add(new UniqueConstraint("Constraint1", new DataColumn[]
				{
					this.columnENTERPRISE_PROJECT_TYPE_UID,
					this.columnDEPARTMENT_UID
				}, true));
				this.columnENTERPRISE_PROJECT_TYPE_UID.AllowDBNull = false;
				this.columnDEPARTMENT_UID.AllowDBNull = false;
			}

			// Token: 0x0600BEC0 RID: 48832 RVA: 0x002541C3 File Offset: 0x002523C3
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			public WorkflowDataSet.EnterpriseProjectTypeDepartmentsRow NewEnterpriseProjectTypeDepartmentsRow()
			{
				return (WorkflowDataSet.EnterpriseProjectTypeDepartmentsRow)base.NewRow();
			}

			// Token: 0x0600BEC1 RID: 48833 RVA: 0x002541D0 File Offset: 0x002523D0
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			protected override DataRow NewRowFromBuilder(DataRowBuilder builder)
			{
				return new WorkflowDataSet.EnterpriseProjectTypeDepartmentsRow(builder);
			}

			// Token: 0x0600BEC2 RID: 48834 RVA: 0x002541D8 File Offset: 0x002523D8
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			protected override Type GetRowType()
			{
				return typeof(WorkflowDataSet.EnterpriseProjectTypeDepartmentsRow);
			}

			// Token: 0x0600BEC3 RID: 48835 RVA: 0x002541E4 File Offset: 0x002523E4
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			protected override void OnRowChanged(DataRowChangeEventArgs e)
			{
				base.OnRowChanged(e);
				if (this.EnterpriseProjectTypeDepartmentsRowChanged != null)
				{
					this.EnterpriseProjectTypeDepartmentsRowChanged(this, new WorkflowDataSet.EnterpriseProjectTypeDepartmentsRowChangeEvent((WorkflowDataSet.EnterpriseProjectTypeDepartmentsRow)e.Row, e.Action));
				}
			}

			// Token: 0x0600BEC4 RID: 48836 RVA: 0x00254217 File Offset: 0x00252417
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			protected override void OnRowChanging(DataRowChangeEventArgs e)
			{
				base.OnRowChanging(e);
				if (this.EnterpriseProjectTypeDepartmentsRowChanging != null)
				{
					this.EnterpriseProjectTypeDepartmentsRowChanging(this, new WorkflowDataSet.EnterpriseProjectTypeDepartmentsRowChangeEvent((WorkflowDataSet.EnterpriseProjectTypeDepartmentsRow)e.Row, e.Action));
				}
			}

			// Token: 0x0600BEC5 RID: 48837 RVA: 0x0025424A File Offset: 0x0025244A
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			protected override void OnRowDeleted(DataRowChangeEventArgs e)
			{
				base.OnRowDeleted(e);
				if (this.EnterpriseProjectTypeDepartmentsRowDeleted != null)
				{
					this.EnterpriseProjectTypeDepartmentsRowDeleted(this, new WorkflowDataSet.EnterpriseProjectTypeDepartmentsRowChangeEvent((WorkflowDataSet.EnterpriseProjectTypeDepartmentsRow)e.Row, e.Action));
				}
			}

			// Token: 0x0600BEC6 RID: 48838 RVA: 0x0025427D File Offset: 0x0025247D
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			protected override void OnRowDeleting(DataRowChangeEventArgs e)
			{
				base.OnRowDeleting(e);
				if (this.EnterpriseProjectTypeDepartmentsRowDeleting != null)
				{
					this.EnterpriseProjectTypeDepartmentsRowDeleting(this, new WorkflowDataSet.EnterpriseProjectTypeDepartmentsRowChangeEvent((WorkflowDataSet.EnterpriseProjectTypeDepartmentsRow)e.Row, e.Action));
				}
			}

			// Token: 0x0600BEC7 RID: 48839 RVA: 0x002542B0 File Offset: 0x002524B0
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			public void RemoveEnterpriseProjectTypeDepartmentsRow(WorkflowDataSet.EnterpriseProjectTypeDepartmentsRow row)
			{
				base.Rows.Remove(row);
			}

			// Token: 0x0600BEC8 RID: 48840 RVA: 0x002542C0 File Offset: 0x002524C0
			/*[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public static XmlSchemaComplexType GetTypedTableSchema(XmlSchemaSet xs)
			{
				XmlSchemaComplexType xmlSchemaComplexType = new XmlSchemaComplexType();
				XmlSchemaSequence xmlSchemaSequence = new XmlSchemaSequence();
				WorkflowDataSet workflowDataSet = new WorkflowDataSet();
				XmlSchemaAny xmlSchemaAny = new XmlSchemaAny();
				xmlSchemaAny.Namespace = "http://www.w3.org/2001/XMLSchema";
				xmlSchemaAny.MinOccurs = 0m;
				xmlSchemaAny.MaxOccurs = decimal.MaxValue;
				xmlSchemaAny.ProcessContents = XmlSchemaContentProcessing.Lax;
				xmlSchemaSequence.Items.Add(xmlSchemaAny);
				XmlSchemaAny xmlSchemaAny2 = new XmlSchemaAny();
				xmlSchemaAny2.Namespace = "urn:schemas-microsoft-com:xml-diffgram-v1";
				xmlSchemaAny2.MinOccurs = 1m;
				xmlSchemaAny2.ProcessContents = XmlSchemaContentProcessing.Lax;
				xmlSchemaSequence.Items.Add(xmlSchemaAny2);
				XmlSchemaAttribute xmlSchemaAttribute = new XmlSchemaAttribute();
				xmlSchemaAttribute.Name = "namespace";
				xmlSchemaAttribute.FixedValue = workflowDataSet.Namespace;
				xmlSchemaComplexType.Attributes.Add(xmlSchemaAttribute);
				XmlSchemaAttribute xmlSchemaAttribute2 = new XmlSchemaAttribute();
				xmlSchemaAttribute2.Name = "tableTypeName";
				xmlSchemaAttribute2.FixedValue = "EnterpriseProjectTypeDepartmentsDataTable";
				xmlSchemaComplexType.Attributes.Add(xmlSchemaAttribute2);
				xmlSchemaComplexType.Particle = xmlSchemaSequence;
				XmlSchema schemaSerializable = workflowDataSet.GetSchemaSerializable();
				if (xs.Contains(schemaSerializable.TargetNamespace))
				{
					MemoryStream memoryStream = new MemoryStream();
					MemoryStream memoryStream2 = new MemoryStream();
					try
					{
						schemaSerializable.Write(memoryStream);
						foreach (object obj in xs.Schemas(schemaSerializable.TargetNamespace))
						{
							XmlSchema xmlSchema = (XmlSchema)obj;
							memoryStream2.SetLength(0L);
							xmlSchema.Write(memoryStream2);
							if (memoryStream.Length == memoryStream2.Length)
							{
								memoryStream.Position = 0L;
								memoryStream2.Position = 0L;
								while (memoryStream.Position != memoryStream.Length && memoryStream.ReadByte() == memoryStream2.ReadByte())
								{
								}
								if (memoryStream.Position == memoryStream.Length)
								{
									return xmlSchemaComplexType;
								}
							}
						}
					}
					finally
					{
						if (memoryStream != null)
						{
							memoryStream.Close();
						}
						if (memoryStream2 != null)
						{
							memoryStream2.Close();
						}
					}
				}
				xs.Add(schemaSerializable);
				return xmlSchemaComplexType;
			}*/

			// Token: 0x0400269E RID: 9886
			private DataColumn columnENTERPRISE_PROJECT_TYPE_UID;

			// Token: 0x0400269F RID: 9887
			private DataColumn columnDEPARTMENT_UID;
		}

		// Token: 0x020007B4 RID: 1972
		[XmlSchemaProvider("GetTypedTableSchema")]
		[Serializable]
		public class EnterpriseProjectTypePDPsDataTable : DataTable, IEnumerable
		{
			// Token: 0x0600BEC9 RID: 48841 RVA: 0x002544B8 File Offset: 0x002526B8
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			public EnterpriseProjectTypePDPsDataTable()
			{
				base.TableName = "EnterpriseProjectTypePDPs";
				this.BeginInit();
				this.InitClass();
				this.EndInit();
			}

			// Token: 0x0600BECA RID: 48842 RVA: 0x002544E0 File Offset: 0x002526E0
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			internal EnterpriseProjectTypePDPsDataTable(DataTable table)
			{
				base.TableName = table.TableName;
				if (table.CaseSensitive != table.DataSet.CaseSensitive)
				{
					base.CaseSensitive = table.CaseSensitive;
				}
				if (table.Locale.ToString() != table.DataSet.Locale.ToString())
				{
					base.Locale = table.Locale;
				}
				if (table.Namespace != table.DataSet.Namespace)
				{
					base.Namespace = table.Namespace;
				}
				base.Prefix = table.Prefix;
				base.MinimumCapacity = table.MinimumCapacity;
			}

			// Token: 0x0600BECB RID: 48843 RVA: 0x00254588 File Offset: 0x00252788
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			protected EnterpriseProjectTypePDPsDataTable(SerializationInfo info, StreamingContext context) : base(info, context)
			{
				this.InitVars();
			}

			// Token: 0x17003A4E RID: 14926
			// (get) Token: 0x0600BECC RID: 48844 RVA: 0x00254598 File Offset: 0x00252798
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public DataColumn ENTERPRISE_PROJECT_TYPE_UIDColumn
			{
				get
				{
					return this.columnENTERPRISE_PROJECT_TYPE_UID;
				}
			}

			// Token: 0x17003A4F RID: 14927
			// (get) Token: 0x0600BECD RID: 48845 RVA: 0x002545A0 File Offset: 0x002527A0
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			public DataColumn PDP_UIDColumn
			{
				get
				{
					return this.columnPDP_UID;
				}
			}

			// Token: 0x17003A50 RID: 14928
			// (get) Token: 0x0600BECE RID: 48846 RVA: 0x002545A8 File Offset: 0x002527A8
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			public DataColumn PDP_IDColumn
			{
				get
				{
					return this.columnPDP_ID;
				}
			}

			// Token: 0x17003A51 RID: 14929
			// (get) Token: 0x0600BECF RID: 48847 RVA: 0x002545B0 File Offset: 0x002527B0
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			public DataColumn PDP_NAMEColumn
			{
				get
				{
					return this.columnPDP_NAME;
				}
			}

			// Token: 0x17003A52 RID: 14930
			// (get) Token: 0x0600BED0 RID: 48848 RVA: 0x002545B8 File Offset: 0x002527B8
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			public DataColumn IS_CREATE_PDPColumn
			{
				get
				{
					return this.columnIS_CREATE_PDP;
				}
			}

			// Token: 0x17003A53 RID: 14931
			// (get) Token: 0x0600BED1 RID: 48849 RVA: 0x002545C0 File Offset: 0x002527C0
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public DataColumn PDP_POSITIONColumn
			{
				get
				{
					return this.columnPDP_POSITION;
				}
			}

			// Token: 0x17003A54 RID: 14932
			// (get) Token: 0x0600BED2 RID: 48850 RVA: 0x002545C8 File Offset: 0x002527C8
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[Browsable(false)]
			public int Count
			{
				get
				{
					return base.Rows.Count;
				}
			}

			// Token: 0x140006D1 RID: 1745
			// (add) Token: 0x0600BED4 RID: 48852 RVA: 0x002545E8 File Offset: 0x002527E8
			// (remove) Token: 0x0600BED5 RID: 48853 RVA: 0x00254620 File Offset: 0x00252820
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public event WorkflowDataSet.EnterpriseProjectTypePDPsRowChangeEventHandler EnterpriseProjectTypePDPsRowChanging;

			// Token: 0x140006D2 RID: 1746
			// (add) Token: 0x0600BED6 RID: 48854 RVA: 0x00254658 File Offset: 0x00252858
			// (remove) Token: 0x0600BED7 RID: 48855 RVA: 0x00254690 File Offset: 0x00252890
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public event WorkflowDataSet.EnterpriseProjectTypePDPsRowChangeEventHandler EnterpriseProjectTypePDPsRowChanged;

			// Token: 0x140006D3 RID: 1747
			// (add) Token: 0x0600BED8 RID: 48856 RVA: 0x002546C8 File Offset: 0x002528C8
			// (remove) Token: 0x0600BED9 RID: 48857 RVA: 0x00254700 File Offset: 0x00252900
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public event WorkflowDataSet.EnterpriseProjectTypePDPsRowChangeEventHandler EnterpriseProjectTypePDPsRowDeleting;

			// Token: 0x140006D4 RID: 1748
			// (add) Token: 0x0600BEDA RID: 48858 RVA: 0x00254738 File Offset: 0x00252938
			// (remove) Token: 0x0600BEDB RID: 48859 RVA: 0x00254770 File Offset: 0x00252970
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public event WorkflowDataSet.EnterpriseProjectTypePDPsRowChangeEventHandler EnterpriseProjectTypePDPsRowDeleted;

			// Token: 0x0600BEDC RID: 48860 RVA: 0x002547A5 File Offset: 0x002529A5
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			public void AddEnterpriseProjectTypePDPsRow(WorkflowDataSet.EnterpriseProjectTypePDPsRow row)
			{
				base.Rows.Add(row);
			}

			// Token: 0x0600BEDD RID: 48861 RVA: 0x002547B4 File Offset: 0x002529B4
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			public WorkflowDataSet.EnterpriseProjectTypePDPsRow AddEnterpriseProjectTypePDPsRow(WorkflowDataSet.EnterpriseProjectTypeRow parentEnterpriseProjectTypeRowByFK_EnterpriseProjectType_EnterpriseProjectTypePDPs, Guid PDP_UID, int PDP_ID, string PDP_NAME, bool IS_CREATE_PDP, int PDP_POSITION)
			{
				WorkflowDataSet.EnterpriseProjectTypePDPsRow enterpriseProjectTypePDPsRow = (WorkflowDataSet.EnterpriseProjectTypePDPsRow)base.NewRow();
				object[] array = new object[]
				{
					null,
					PDP_UID,
					PDP_ID,
					PDP_NAME,
					IS_CREATE_PDP,
					PDP_POSITION
				};
				if (parentEnterpriseProjectTypeRowByFK_EnterpriseProjectType_EnterpriseProjectTypePDPs != null)
				{
					array[0] = parentEnterpriseProjectTypeRowByFK_EnterpriseProjectType_EnterpriseProjectTypePDPs[0];
				}
				enterpriseProjectTypePDPsRow.ItemArray = array;
				base.Rows.Add(enterpriseProjectTypePDPsRow);
				return enterpriseProjectTypePDPsRow;
			}

			// Token: 0x0600BEDE RID: 48862 RVA: 0x00254824 File Offset: 0x00252A24
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			public WorkflowDataSet.EnterpriseProjectTypePDPsRow FindByENTERPRISE_PROJECT_TYPE_UIDPDP_UIDIS_CREATE_PDP(Guid ENTERPRISE_PROJECT_TYPE_UID, Guid PDP_UID, bool IS_CREATE_PDP)
			{
				return (WorkflowDataSet.EnterpriseProjectTypePDPsRow)base.Rows.Find(new object[]
				{
					ENTERPRISE_PROJECT_TYPE_UID,
					PDP_UID,
					IS_CREATE_PDP
				});
			}

			// Token: 0x0600BEDF RID: 48863 RVA: 0x00254864 File Offset: 0x00252A64
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public virtual IEnumerator GetEnumerator()
			{
				return base.Rows.GetEnumerator();
			}

			// Token: 0x0600BEE0 RID: 48864 RVA: 0x00254874 File Offset: 0x00252A74
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			public override DataTable Clone()
			{
				WorkflowDataSet.EnterpriseProjectTypePDPsDataTable enterpriseProjectTypePDPsDataTable = (WorkflowDataSet.EnterpriseProjectTypePDPsDataTable)base.Clone();
				enterpriseProjectTypePDPsDataTable.InitVars();
				return enterpriseProjectTypePDPsDataTable;
			}

			// Token: 0x0600BEE1 RID: 48865 RVA: 0x00254894 File Offset: 0x00252A94
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			protected override DataTable CreateInstance()
			{
				return new WorkflowDataSet.EnterpriseProjectTypePDPsDataTable();
			}

			// Token: 0x0600BEE2 RID: 48866 RVA: 0x0025489C File Offset: 0x00252A9C
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			internal void InitVars()
			{
				this.columnENTERPRISE_PROJECT_TYPE_UID = base.Columns["ENTERPRISE_PROJECT_TYPE_UID"];
				this.columnPDP_UID = base.Columns["PDP_UID"];
				this.columnPDP_ID = base.Columns["PDP_ID"];
				this.columnPDP_NAME = base.Columns["PDP_NAME"];
				this.columnIS_CREATE_PDP = base.Columns["IS_CREATE_PDP"];
				this.columnPDP_POSITION = base.Columns["PDP_POSITION"];
			}

			// Token: 0x0600BEE3 RID: 48867 RVA: 0x00254930 File Offset: 0x00252B30
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			private void InitClass()
			{
				this.columnENTERPRISE_PROJECT_TYPE_UID = new DataColumn("ENTERPRISE_PROJECT_TYPE_UID", typeof(Guid), null, MappingType.Element);
				base.Columns.Add(this.columnENTERPRISE_PROJECT_TYPE_UID);
				this.columnPDP_UID = new DataColumn("PDP_UID", typeof(Guid), null, MappingType.Element);
				base.Columns.Add(this.columnPDP_UID);
				this.columnPDP_ID = new DataColumn("PDP_ID", typeof(int), null, MappingType.Element);
				base.Columns.Add(this.columnPDP_ID);
				this.columnPDP_NAME = new DataColumn("PDP_NAME", typeof(string), null, MappingType.Element);
				base.Columns.Add(this.columnPDP_NAME);
				this.columnIS_CREATE_PDP = new DataColumn("IS_CREATE_PDP", typeof(bool), null, MappingType.Element);
				base.Columns.Add(this.columnIS_CREATE_PDP);
				this.columnPDP_POSITION = new DataColumn("PDP_POSITION", typeof(int), null, MappingType.Element);
				base.Columns.Add(this.columnPDP_POSITION);
				base.Constraints.Add(new UniqueConstraint("Constraint1", new DataColumn[]
				{
					this.columnENTERPRISE_PROJECT_TYPE_UID,
					this.columnPDP_UID,
					this.columnIS_CREATE_PDP
				}, true));
				this.columnENTERPRISE_PROJECT_TYPE_UID.AllowDBNull = false;
				this.columnPDP_UID.AllowDBNull = false;
				this.columnPDP_ID.AllowDBNull = false;
				this.columnPDP_ID.DefaultValue = 0;
				this.columnIS_CREATE_PDP.AllowDBNull = false;
				this.columnIS_CREATE_PDP.DefaultValue = true;
				this.columnPDP_POSITION.AllowDBNull = false;
				this.columnPDP_POSITION.DefaultValue = 0;
			}

			// Token: 0x0600BEE4 RID: 48868 RVA: 0x00254AF3 File Offset: 0x00252CF3
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public WorkflowDataSet.EnterpriseProjectTypePDPsRow NewEnterpriseProjectTypePDPsRow()
			{
				return (WorkflowDataSet.EnterpriseProjectTypePDPsRow)base.NewRow();
			}

			// Token: 0x0600BEE5 RID: 48869 RVA: 0x00254B00 File Offset: 0x00252D00
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			protected override DataRow NewRowFromBuilder(DataRowBuilder builder)
			{
				return new WorkflowDataSet.EnterpriseProjectTypePDPsRow(builder);
			}

			// Token: 0x0600BEE6 RID: 48870 RVA: 0x00254B08 File Offset: 0x00252D08
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			protected override Type GetRowType()
			{
				return typeof(WorkflowDataSet.EnterpriseProjectTypePDPsRow);
			}

			// Token: 0x0600BEE7 RID: 48871 RVA: 0x00254B14 File Offset: 0x00252D14
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			protected override void OnRowChanged(DataRowChangeEventArgs e)
			{
				base.OnRowChanged(e);
				if (this.EnterpriseProjectTypePDPsRowChanged != null)
				{
					this.EnterpriseProjectTypePDPsRowChanged(this, new WorkflowDataSet.EnterpriseProjectTypePDPsRowChangeEvent((WorkflowDataSet.EnterpriseProjectTypePDPsRow)e.Row, e.Action));
				}
			}

			// Token: 0x0600BEE8 RID: 48872 RVA: 0x00254B47 File Offset: 0x00252D47
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			protected override void OnRowChanging(DataRowChangeEventArgs e)
			{
				base.OnRowChanging(e);
				if (this.EnterpriseProjectTypePDPsRowChanging != null)
				{
					this.EnterpriseProjectTypePDPsRowChanging(this, new WorkflowDataSet.EnterpriseProjectTypePDPsRowChangeEvent((WorkflowDataSet.EnterpriseProjectTypePDPsRow)e.Row, e.Action));
				}
			}

			// Token: 0x0600BEE9 RID: 48873 RVA: 0x00254B7A File Offset: 0x00252D7A
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			protected override void OnRowDeleted(DataRowChangeEventArgs e)
			{
				base.OnRowDeleted(e);
				if (this.EnterpriseProjectTypePDPsRowDeleted != null)
				{
					this.EnterpriseProjectTypePDPsRowDeleted(this, new WorkflowDataSet.EnterpriseProjectTypePDPsRowChangeEvent((WorkflowDataSet.EnterpriseProjectTypePDPsRow)e.Row, e.Action));
				}
			}

			// Token: 0x0600BEEA RID: 48874 RVA: 0x00254BAD File Offset: 0x00252DAD
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			protected override void OnRowDeleting(DataRowChangeEventArgs e)
			{
				base.OnRowDeleting(e);
				if (this.EnterpriseProjectTypePDPsRowDeleting != null)
				{
					this.EnterpriseProjectTypePDPsRowDeleting(this, new WorkflowDataSet.EnterpriseProjectTypePDPsRowChangeEvent((WorkflowDataSet.EnterpriseProjectTypePDPsRow)e.Row, e.Action));
				}
			}

			// Token: 0x0600BEEB RID: 48875 RVA: 0x00254BE0 File Offset: 0x00252DE0
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			public void RemoveEnterpriseProjectTypePDPsRow(WorkflowDataSet.EnterpriseProjectTypePDPsRow row)
			{
				base.Rows.Remove(row);
			}

			// Token: 0x0600BEEC RID: 48876 RVA: 0x00254BF0 File Offset: 0x00252DF0
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			public static XmlSchemaComplexType GetTypedTableSchema(XmlSchemaSet xs)
			{
				XmlSchemaComplexType xmlSchemaComplexType = new XmlSchemaComplexType();
				XmlSchemaSequence xmlSchemaSequence = new XmlSchemaSequence();
				WorkflowDataSet workflowDataSet = new WorkflowDataSet();
				XmlSchemaAny xmlSchemaAny = new XmlSchemaAny();
				xmlSchemaAny.Namespace = "http://www.w3.org/2001/XMLSchema";
				xmlSchemaAny.MinOccurs = 0m;
				xmlSchemaAny.MaxOccurs = decimal.MaxValue;
				xmlSchemaAny.ProcessContents = XmlSchemaContentProcessing.Lax;
				xmlSchemaSequence.Items.Add(xmlSchemaAny);
				XmlSchemaAny xmlSchemaAny2 = new XmlSchemaAny();
				xmlSchemaAny2.Namespace = "urn:schemas-microsoft-com:xml-diffgram-v1";
				xmlSchemaAny2.MinOccurs = 1m;
				xmlSchemaAny2.ProcessContents = XmlSchemaContentProcessing.Lax;
				xmlSchemaSequence.Items.Add(xmlSchemaAny2);
				XmlSchemaAttribute xmlSchemaAttribute = new XmlSchemaAttribute();
				xmlSchemaAttribute.Name = "namespace";
				xmlSchemaAttribute.FixedValue = workflowDataSet.Namespace;
				xmlSchemaComplexType.Attributes.Add(xmlSchemaAttribute);
				XmlSchemaAttribute xmlSchemaAttribute2 = new XmlSchemaAttribute();
				xmlSchemaAttribute2.Name = "tableTypeName";
				xmlSchemaAttribute2.FixedValue = "EnterpriseProjectTypePDPsDataTable";
				xmlSchemaComplexType.Attributes.Add(xmlSchemaAttribute2);
				xmlSchemaComplexType.Particle = xmlSchemaSequence;
				XmlSchema schemaSerializable = workflowDataSet.GetSchemaSerializable();
				if (xs.Contains(schemaSerializable.TargetNamespace))
				{
					MemoryStream memoryStream = new MemoryStream();
					MemoryStream memoryStream2 = new MemoryStream();
					try
					{
						schemaSerializable.Write(memoryStream);
						foreach (object obj in xs.Schemas(schemaSerializable.TargetNamespace))
						{
							XmlSchema xmlSchema = (XmlSchema)obj;
							memoryStream2.SetLength(0L);
							xmlSchema.Write(memoryStream2);
							if (memoryStream.Length == memoryStream2.Length)
							{
								memoryStream.Position = 0L;
								memoryStream2.Position = 0L;
								while (memoryStream.Position != memoryStream.Length && memoryStream.ReadByte() == memoryStream2.ReadByte())
								{
								}
								if (memoryStream.Position == memoryStream.Length)
								{
									return xmlSchemaComplexType;
								}
							}
						}
					}
					finally
					{
						if (memoryStream != null)
						{
							memoryStream.Close();
						}
						if (memoryStream2 != null)
						{
							memoryStream2.Close();
						}
					}
				}
				xs.Add(schemaSerializable);
				return xmlSchemaComplexType;
			}

			// Token: 0x040026A4 RID: 9892
			private DataColumn columnENTERPRISE_PROJECT_TYPE_UID;

			// Token: 0x040026A5 RID: 9893
			private DataColumn columnPDP_UID;

			// Token: 0x040026A6 RID: 9894
			private DataColumn columnPDP_ID;

			// Token: 0x040026A7 RID: 9895
			private DataColumn columnPDP_NAME;

			// Token: 0x040026A8 RID: 9896
			private DataColumn columnIS_CREATE_PDP;

			// Token: 0x040026A9 RID: 9897
			private DataColumn columnPDP_POSITION;
		}

		// Token: 0x020007B5 RID: 1973
		[XmlSchemaProvider("GetTypedTableSchema")]
		[Serializable]
		public class UpdateProjectWorkflowsDataTable : DataTable, IEnumerable
		{
			// Token: 0x0600BEED RID: 48877 RVA: 0x00254DE8 File Offset: 0x00252FE8
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			public UpdateProjectWorkflowsDataTable()
			{
				base.TableName = "UpdateProjectWorkflows";
				this.BeginInit();
				this.InitClass();
				this.EndInit();
			}

			// Token: 0x0600BEEE RID: 48878 RVA: 0x00254E10 File Offset: 0x00253010
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			internal UpdateProjectWorkflowsDataTable(DataTable table)
			{
				base.TableName = table.TableName;
				if (table.CaseSensitive != table.DataSet.CaseSensitive)
				{
					base.CaseSensitive = table.CaseSensitive;
				}
				if (table.Locale.ToString() != table.DataSet.Locale.ToString())
				{
					base.Locale = table.Locale;
				}
				if (table.Namespace != table.DataSet.Namespace)
				{
					base.Namespace = table.Namespace;
				}
				base.Prefix = table.Prefix;
				base.MinimumCapacity = table.MinimumCapacity;
			}

			// Token: 0x0600BEEF RID: 48879 RVA: 0x00254EB8 File Offset: 0x002530B8
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			protected UpdateProjectWorkflowsDataTable(SerializationInfo info, StreamingContext context) : base(info, context)
			{
				this.InitVars();
			}

			// Token: 0x17003A56 RID: 14934
			// (get) Token: 0x0600BEF0 RID: 48880 RVA: 0x00254EC8 File Offset: 0x002530C8
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public DataColumn JOB_UIDColumn
			{
				get
				{
					return this.columnJOB_UID;
				}
			}

			// Token: 0x17003A57 RID: 14935
			// (get) Token: 0x0600BEF1 RID: 48881 RVA: 0x00254ED0 File Offset: 0x002530D0
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			public DataColumn PROJ_UIDColumn
			{
				get
				{
					return this.columnPROJ_UID;
				}
			}

			// Token: 0x17003A58 RID: 14936
			// (get) Token: 0x0600BEF2 RID: 48882 RVA: 0x00254ED8 File Offset: 0x002530D8
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public DataColumn ENTERPRISE_PROJECT_TYPE_UIDColumn
			{
				get
				{
					return this.columnENTERPRISE_PROJECT_TYPE_UID;
				}
			}

			// Token: 0x17003A59 RID: 14937
			// (get) Token: 0x0600BEF3 RID: 48883 RVA: 0x00254EE0 File Offset: 0x002530E0
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public DataColumn SKIP_TO_CURRENT_STAGEColumn
			{
				get
				{
					return this.columnSKIP_TO_CURRENT_STAGE;
				}
			}

			// Token: 0x17003A5A RID: 14938
			// (get) Token: 0x0600BEF4 RID: 48884 RVA: 0x00254EE8 File Offset: 0x002530E8
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			public DataColumn STAGE_UIDColumn
			{
				get
				{
					return this.columnSTAGE_UID;
				}
			}

			// Token: 0x17003A5B RID: 14939
			// (get) Token: 0x0600BEF5 RID: 48885 RVA: 0x00254EF0 File Offset: 0x002530F0
			[DebuggerNonUserCode]
			[Browsable(false)]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public int Count
			{
				get
				{
					return base.Rows.Count;
				}
			}

			// Token: 0x140006D5 RID: 1749
			// (add) Token: 0x0600BEF7 RID: 48887 RVA: 0x00254F10 File Offset: 0x00253110
			// (remove) Token: 0x0600BEF8 RID: 48888 RVA: 0x00254F48 File Offset: 0x00253148
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public event WorkflowDataSet.UpdateProjectWorkflowsRowChangeEventHandler UpdateProjectWorkflowsRowChanging;

			// Token: 0x140006D6 RID: 1750
			// (add) Token: 0x0600BEF9 RID: 48889 RVA: 0x00254F80 File Offset: 0x00253180
			// (remove) Token: 0x0600BEFA RID: 48890 RVA: 0x00254FB8 File Offset: 0x002531B8
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public event WorkflowDataSet.UpdateProjectWorkflowsRowChangeEventHandler UpdateProjectWorkflowsRowChanged;

			// Token: 0x140006D7 RID: 1751
			// (add) Token: 0x0600BEFB RID: 48891 RVA: 0x00254FF0 File Offset: 0x002531F0
			// (remove) Token: 0x0600BEFC RID: 48892 RVA: 0x00255028 File Offset: 0x00253228
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public event WorkflowDataSet.UpdateProjectWorkflowsRowChangeEventHandler UpdateProjectWorkflowsRowDeleting;

			// Token: 0x140006D8 RID: 1752
			// (add) Token: 0x0600BEFD RID: 48893 RVA: 0x00255060 File Offset: 0x00253260
			// (remove) Token: 0x0600BEFE RID: 48894 RVA: 0x00255098 File Offset: 0x00253298
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public event WorkflowDataSet.UpdateProjectWorkflowsRowChangeEventHandler UpdateProjectWorkflowsRowDeleted;

			// Token: 0x0600BEFF RID: 48895 RVA: 0x002550CD File Offset: 0x002532CD
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public void AddUpdateProjectWorkflowsRow(WorkflowDataSet.UpdateProjectWorkflowsRow row)
			{
				base.Rows.Add(row);
			}

			// Token: 0x0600BF00 RID: 48896 RVA: 0x002550DC File Offset: 0x002532DC
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			public WorkflowDataSet.UpdateProjectWorkflowsRow AddUpdateProjectWorkflowsRow(Guid JOB_UID, Guid PROJ_UID, Guid ENTERPRISE_PROJECT_TYPE_UID, bool SKIP_TO_CURRENT_STAGE, Guid STAGE_UID)
			{
				WorkflowDataSet.UpdateProjectWorkflowsRow updateProjectWorkflowsRow = (WorkflowDataSet.UpdateProjectWorkflowsRow)base.NewRow();
				object[] itemArray = new object[]
				{
					JOB_UID,
					PROJ_UID,
					ENTERPRISE_PROJECT_TYPE_UID,
					SKIP_TO_CURRENT_STAGE,
					STAGE_UID
				};
				updateProjectWorkflowsRow.ItemArray = itemArray;
				base.Rows.Add(updateProjectWorkflowsRow);
				return updateProjectWorkflowsRow;
			}

			// Token: 0x0600BF01 RID: 48897 RVA: 0x00255144 File Offset: 0x00253344
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public WorkflowDataSet.UpdateProjectWorkflowsRow FindByPROJ_UID(Guid PROJ_UID)
			{
				return (WorkflowDataSet.UpdateProjectWorkflowsRow)base.Rows.Find(new object[]
				{
					PROJ_UID
				});
			}

			// Token: 0x0600BF02 RID: 48898 RVA: 0x00255172 File Offset: 0x00253372
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public virtual IEnumerator GetEnumerator()
			{
				return base.Rows.GetEnumerator();
			}

			// Token: 0x0600BF03 RID: 48899 RVA: 0x00255180 File Offset: 0x00253380
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public override DataTable Clone()
			{
				WorkflowDataSet.UpdateProjectWorkflowsDataTable updateProjectWorkflowsDataTable = (WorkflowDataSet.UpdateProjectWorkflowsDataTable)base.Clone();
				updateProjectWorkflowsDataTable.InitVars();
				return updateProjectWorkflowsDataTable;
			}

			// Token: 0x0600BF04 RID: 48900 RVA: 0x002551A0 File Offset: 0x002533A0
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			protected override DataTable CreateInstance()
			{
				return new WorkflowDataSet.UpdateProjectWorkflowsDataTable();
			}

			// Token: 0x0600BF05 RID: 48901 RVA: 0x002551A8 File Offset: 0x002533A8
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			internal void InitVars()
			{
				this.columnJOB_UID = base.Columns["JOB_UID"];
				this.columnPROJ_UID = base.Columns["PROJ_UID"];
				this.columnENTERPRISE_PROJECT_TYPE_UID = base.Columns["ENTERPRISE_PROJECT_TYPE_UID"];
				this.columnSKIP_TO_CURRENT_STAGE = base.Columns["SKIP_TO_CURRENT_STAGE"];
				this.columnSTAGE_UID = base.Columns["STAGE_UID"];
			}

			// Token: 0x0600BF06 RID: 48902 RVA: 0x00255224 File Offset: 0x00253424
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			private void InitClass()
			{
				this.columnJOB_UID = new DataColumn("JOB_UID", typeof(Guid), null, MappingType.Element);
				base.Columns.Add(this.columnJOB_UID);
				this.columnPROJ_UID = new DataColumn("PROJ_UID", typeof(Guid), null, MappingType.Element);
				base.Columns.Add(this.columnPROJ_UID);
				this.columnENTERPRISE_PROJECT_TYPE_UID = new DataColumn("ENTERPRISE_PROJECT_TYPE_UID", typeof(Guid), null, MappingType.Element);
				base.Columns.Add(this.columnENTERPRISE_PROJECT_TYPE_UID);
				this.columnSKIP_TO_CURRENT_STAGE = new DataColumn("SKIP_TO_CURRENT_STAGE", typeof(bool), null, MappingType.Element);
				base.Columns.Add(this.columnSKIP_TO_CURRENT_STAGE);
				this.columnSTAGE_UID = new DataColumn("STAGE_UID", typeof(Guid), null, MappingType.Element);
				base.Columns.Add(this.columnSTAGE_UID);
				base.Constraints.Add(new UniqueConstraint("Constraint1", new DataColumn[]
				{
					this.columnPROJ_UID
				}, true));
				this.columnJOB_UID.AllowDBNull = false;
				this.columnPROJ_UID.AllowDBNull = false;
				this.columnPROJ_UID.Unique = true;
				this.columnENTERPRISE_PROJECT_TYPE_UID.AllowDBNull = false;
				this.columnSKIP_TO_CURRENT_STAGE.AllowDBNull = false;
			}

			// Token: 0x0600BF07 RID: 48903 RVA: 0x00255375 File Offset: 0x00253575
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public WorkflowDataSet.UpdateProjectWorkflowsRow NewUpdateProjectWorkflowsRow()
			{
				return (WorkflowDataSet.UpdateProjectWorkflowsRow)base.NewRow();
			}

			// Token: 0x0600BF08 RID: 48904 RVA: 0x00255382 File Offset: 0x00253582
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			protected override DataRow NewRowFromBuilder(DataRowBuilder builder)
			{
				return new WorkflowDataSet.UpdateProjectWorkflowsRow(builder);
			}

			// Token: 0x0600BF09 RID: 48905 RVA: 0x0025538A File Offset: 0x0025358A
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			protected override Type GetRowType()
			{
				return typeof(WorkflowDataSet.UpdateProjectWorkflowsRow);
			}

			// Token: 0x0600BF0A RID: 48906 RVA: 0x00255396 File Offset: 0x00253596
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			protected override void OnRowChanged(DataRowChangeEventArgs e)
			{
				base.OnRowChanged(e);
				if (this.UpdateProjectWorkflowsRowChanged != null)
				{
					this.UpdateProjectWorkflowsRowChanged(this, new WorkflowDataSet.UpdateProjectWorkflowsRowChangeEvent((WorkflowDataSet.UpdateProjectWorkflowsRow)e.Row, e.Action));
				}
			}

			// Token: 0x0600BF0B RID: 48907 RVA: 0x002553C9 File Offset: 0x002535C9
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			protected override void OnRowChanging(DataRowChangeEventArgs e)
			{
				base.OnRowChanging(e);
				if (this.UpdateProjectWorkflowsRowChanging != null)
				{
					this.UpdateProjectWorkflowsRowChanging(this, new WorkflowDataSet.UpdateProjectWorkflowsRowChangeEvent((WorkflowDataSet.UpdateProjectWorkflowsRow)e.Row, e.Action));
				}
			}

			// Token: 0x0600BF0C RID: 48908 RVA: 0x002553FC File Offset: 0x002535FC
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			protected override void OnRowDeleted(DataRowChangeEventArgs e)
			{
				base.OnRowDeleted(e);
				if (this.UpdateProjectWorkflowsRowDeleted != null)
				{
					this.UpdateProjectWorkflowsRowDeleted(this, new WorkflowDataSet.UpdateProjectWorkflowsRowChangeEvent((WorkflowDataSet.UpdateProjectWorkflowsRow)e.Row, e.Action));
				}
			}

			// Token: 0x0600BF0D RID: 48909 RVA: 0x0025542F File Offset: 0x0025362F
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			protected override void OnRowDeleting(DataRowChangeEventArgs e)
			{
				base.OnRowDeleting(e);
				if (this.UpdateProjectWorkflowsRowDeleting != null)
				{
					this.UpdateProjectWorkflowsRowDeleting(this, new WorkflowDataSet.UpdateProjectWorkflowsRowChangeEvent((WorkflowDataSet.UpdateProjectWorkflowsRow)e.Row, e.Action));
				}
			}

			// Token: 0x0600BF0E RID: 48910 RVA: 0x00255462 File Offset: 0x00253662
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			public void RemoveUpdateProjectWorkflowsRow(WorkflowDataSet.UpdateProjectWorkflowsRow row)
			{
				base.Rows.Remove(row);
			}

			// Token: 0x0600BF0F RID: 48911 RVA: 0x00255470 File Offset: 0x00253670
			/*[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			public static XmlSchemaComplexType GetTypedTableSchema(XmlSchemaSet xs)
			{
				XmlSchemaComplexType xmlSchemaComplexType = new XmlSchemaComplexType();
				XmlSchemaSequence xmlSchemaSequence = new XmlSchemaSequence();
				WorkflowDataSet workflowDataSet = new WorkflowDataSet();
				XmlSchemaAny xmlSchemaAny = new XmlSchemaAny();
				xmlSchemaAny.Namespace = "http://www.w3.org/2001/XMLSchema";
				xmlSchemaAny.MinOccurs = 0m;
				xmlSchemaAny.MaxOccurs = decimal.MaxValue;
				xmlSchemaAny.ProcessContents = XmlSchemaContentProcessing.Lax;
				xmlSchemaSequence.Items.Add(xmlSchemaAny);
				XmlSchemaAny xmlSchemaAny2 = new XmlSchemaAny();
				xmlSchemaAny2.Namespace = "urn:schemas-microsoft-com:xml-diffgram-v1";
				xmlSchemaAny2.MinOccurs = 1m;
				xmlSchemaAny2.ProcessContents = XmlSchemaContentProcessing.Lax;
				xmlSchemaSequence.Items.Add(xmlSchemaAny2);
				XmlSchemaAttribute xmlSchemaAttribute = new XmlSchemaAttribute();
				xmlSchemaAttribute.Name = "namespace";
				xmlSchemaAttribute.FixedValue = workflowDataSet.Namespace;
				xmlSchemaComplexType.Attributes.Add(xmlSchemaAttribute);
				XmlSchemaAttribute xmlSchemaAttribute2 = new XmlSchemaAttribute();
				xmlSchemaAttribute2.Name = "tableTypeName";
				xmlSchemaAttribute2.FixedValue = "UpdateProjectWorkflowsDataTable";
				xmlSchemaComplexType.Attributes.Add(xmlSchemaAttribute2);
				xmlSchemaComplexType.Particle = xmlSchemaSequence;
				XmlSchema schemaSerializable = workflowDataSet.GetSchemaSerializable();
				if (xs.Contains(schemaSerializable.TargetNamespace))
				{
					MemoryStream memoryStream = new MemoryStream();
					MemoryStream memoryStream2 = new MemoryStream();
					try
					{
						schemaSerializable.Write(memoryStream);
						foreach (object obj in xs.Schemas(schemaSerializable.TargetNamespace))
						{
							XmlSchema xmlSchema = (XmlSchema)obj;
							memoryStream2.SetLength(0L);
							xmlSchema.Write(memoryStream2);
							if (memoryStream.Length == memoryStream2.Length)
							{
								memoryStream.Position = 0L;
								memoryStream2.Position = 0L;
								while (memoryStream.Position != memoryStream.Length && memoryStream.ReadByte() == memoryStream2.ReadByte())
								{
								}
								if (memoryStream.Position == memoryStream.Length)
								{
									return xmlSchemaComplexType;
								}
							}
						}
					}
					finally
					{
						if (memoryStream != null)
						{
							memoryStream.Close();
						}
						if (memoryStream2 != null)
						{
							memoryStream2.Close();
						}
					}
				}
				xs.Add(schemaSerializable);
				return xmlSchemaComplexType;
			}*/

			// Token: 0x040026AE RID: 9902
			private DataColumn columnJOB_UID;

			// Token: 0x040026AF RID: 9903
			private DataColumn columnPROJ_UID;

			// Token: 0x040026B0 RID: 9904
			private DataColumn columnENTERPRISE_PROJECT_TYPE_UID;

			// Token: 0x040026B1 RID: 9905
			private DataColumn columnSKIP_TO_CURRENT_STAGE;

			// Token: 0x040026B2 RID: 9906
			private DataColumn columnSTAGE_UID;
		}

		// Token: 0x020007B6 RID: 1974
		public class WorkflowPhaseRow : DataRow
		{
			// Token: 0x0600BF10 RID: 48912 RVA: 0x00255668 File Offset: 0x00253868
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			internal WorkflowPhaseRow(DataRowBuilder rb) : base(rb)
			{
				this.tableWorkflowPhase = (WorkflowDataSet.WorkflowPhaseDataTable)base.Table;
			}

			// Token: 0x17003A5D RID: 14941
			// (get) Token: 0x0600BF11 RID: 48913 RVA: 0x00255682 File Offset: 0x00253882
			// (set) Token: 0x0600BF12 RID: 48914 RVA: 0x0025569A File Offset: 0x0025389A
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			public Guid PHASE_UID
			{
				get
				{
					return (Guid)base[this.tableWorkflowPhase.PHASE_UIDColumn];
				}
				set
				{
					base[this.tableWorkflowPhase.PHASE_UIDColumn] = value;
				}
			}

			// Token: 0x17003A5E RID: 14942
			// (get) Token: 0x0600BF13 RID: 48915 RVA: 0x002556B3 File Offset: 0x002538B3
			// (set) Token: 0x0600BF14 RID: 48916 RVA: 0x002556CB File Offset: 0x002538CB
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			public string PHASE_NAME
			{
				get
				{
					return (string)base[this.tableWorkflowPhase.PHASE_NAMEColumn];
				}
				set
				{
					base[this.tableWorkflowPhase.PHASE_NAMEColumn] = value;
				}
			}

			// Token: 0x17003A5F RID: 14943
			// (get) Token: 0x0600BF15 RID: 48917 RVA: 0x002556E0 File Offset: 0x002538E0
			// (set) Token: 0x0600BF16 RID: 48918 RVA: 0x00255724 File Offset: 0x00253924
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public string PHASE_DESCRIPTION
			{
				get
				{
					string result;
					try
					{
						result = (string)base[this.tableWorkflowPhase.PHASE_DESCRIPTIONColumn];
					}
					catch (InvalidCastException innerException)
					{
						throw new StrongTypingException("The value for column 'PHASE_DESCRIPTION' in table 'WorkflowPhase' is DBNull.", innerException);
					}
					return result;
				}
				set
				{
					base[this.tableWorkflowPhase.PHASE_DESCRIPTIONColumn] = value;
				}
			}

			// Token: 0x0600BF17 RID: 48919 RVA: 0x00255738 File Offset: 0x00253938
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			public bool IsPHASE_DESCRIPTIONNull()
			{
				return base.IsNull(this.tableWorkflowPhase.PHASE_DESCRIPTIONColumn);
			}

			// Token: 0x0600BF18 RID: 48920 RVA: 0x0025574B File Offset: 0x0025394B
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			public void SetPHASE_DESCRIPTIONNull()
			{
				base[this.tableWorkflowPhase.PHASE_DESCRIPTIONColumn] = Convert.DBNull;
			}

			// Token: 0x0600BF19 RID: 48921 RVA: 0x00255763 File Offset: 0x00253963
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public WorkflowDataSet.WorkflowStageRow[] GetWorkflowStageRows()
			{
				if (base.Table.ChildRelations["FK_WorkflowPhase_WorkflowStage"] == null)
				{
					return new WorkflowDataSet.WorkflowStageRow[0];
				}
				return (WorkflowDataSet.WorkflowStageRow[])base.GetChildRows(base.Table.ChildRelations["FK_WorkflowPhase_WorkflowStage"]);
			}

			// Token: 0x040026B7 RID: 9911
			private WorkflowDataSet.WorkflowPhaseDataTable tableWorkflowPhase;
		}

		// Token: 0x020007B7 RID: 1975
		public class WorkflowStageRow : DataRow
		{
			// Token: 0x0600BF1A RID: 48922 RVA: 0x002557A3 File Offset: 0x002539A3
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			internal WorkflowStageRow(DataRowBuilder rb) : base(rb)
			{
				this.tableWorkflowStage = (WorkflowDataSet.WorkflowStageDataTable)base.Table;
			}

			// Token: 0x17003A60 RID: 14944
			// (get) Token: 0x0600BF1B RID: 48923 RVA: 0x002557BD File Offset: 0x002539BD
			// (set) Token: 0x0600BF1C RID: 48924 RVA: 0x002557D5 File Offset: 0x002539D5
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			public Guid STAGE_UID
			{
				get
				{
					return (Guid)base[this.tableWorkflowStage.STAGE_UIDColumn];
				}
				set
				{
					base[this.tableWorkflowStage.STAGE_UIDColumn] = value;
				}
			}

			// Token: 0x17003A61 RID: 14945
			// (get) Token: 0x0600BF1D RID: 48925 RVA: 0x002557EE File Offset: 0x002539EE
			// (set) Token: 0x0600BF1E RID: 48926 RVA: 0x00255806 File Offset: 0x00253A06
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			public string STAGE_NAME
			{
				get
				{
					return (string)base[this.tableWorkflowStage.STAGE_NAMEColumn];
				}
				set
				{
					base[this.tableWorkflowStage.STAGE_NAMEColumn] = value;
				}
			}

			// Token: 0x17003A62 RID: 14946
			// (get) Token: 0x0600BF1F RID: 48927 RVA: 0x0025581A File Offset: 0x00253A1A
			// (set) Token: 0x0600BF20 RID: 48928 RVA: 0x00255832 File Offset: 0x00253A32
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			public Guid PHASE_UID
			{
				get
				{
					return (Guid)base[this.tableWorkflowStage.PHASE_UIDColumn];
				}
				set
				{
					base[this.tableWorkflowStage.PHASE_UIDColumn] = value;
				}
			}

			// Token: 0x17003A63 RID: 14947
			// (get) Token: 0x0600BF21 RID: 48929 RVA: 0x0025584C File Offset: 0x00253A4C
			// (set) Token: 0x0600BF22 RID: 48930 RVA: 0x00255890 File Offset: 0x00253A90
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			public string PHASE_NAME
			{
				get
				{
					string result;
					try
					{
						result = (string)base[this.tableWorkflowStage.PHASE_NAMEColumn];
					}
					catch (InvalidCastException innerException)
					{
						throw new StrongTypingException("The value for column 'PHASE_NAME' in table 'WorkflowStage' is DBNull.", innerException);
					}
					return result;
				}
				set
				{
					base[this.tableWorkflowStage.PHASE_NAMEColumn] = value;
				}
			}

			// Token: 0x17003A64 RID: 14948
			// (get) Token: 0x0600BF23 RID: 48931 RVA: 0x002558A4 File Offset: 0x00253AA4
			// (set) Token: 0x0600BF24 RID: 48932 RVA: 0x002558E8 File Offset: 0x00253AE8
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public string STAGE_DESCRIPTION
			{
				get
				{
					string result;
					try
					{
						result = (string)base[this.tableWorkflowStage.STAGE_DESCRIPTIONColumn];
					}
					catch (InvalidCastException innerException)
					{
						throw new StrongTypingException("The value for column 'STAGE_DESCRIPTION' in table 'WorkflowStage' is DBNull.", innerException);
					}
					return result;
				}
				set
				{
					base[this.tableWorkflowStage.STAGE_DESCRIPTIONColumn] = value;
				}
			}

			// Token: 0x17003A65 RID: 14949
			// (get) Token: 0x0600BF25 RID: 48933 RVA: 0x002558FC File Offset: 0x00253AFC
			// (set) Token: 0x0600BF26 RID: 48934 RVA: 0x00255914 File Offset: 0x00253B14
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			public bool CHECKIN_REQUIRED
			{
				get
				{
					return (bool)base[this.tableWorkflowStage.CHECKIN_REQUIREDColumn];
				}
				set
				{
					base[this.tableWorkflowStage.CHECKIN_REQUIREDColumn] = value;
				}
			}

			// Token: 0x17003A66 RID: 14950
			// (get) Token: 0x0600BF27 RID: 48935 RVA: 0x00255930 File Offset: 0x00253B30
			// (set) Token: 0x0600BF28 RID: 48936 RVA: 0x00255974 File Offset: 0x00253B74
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			public string STAGE_SUBMIT_DESCRIPTION
			{
				get
				{
					string result;
					try
					{
						result = (string)base[this.tableWorkflowStage.STAGE_SUBMIT_DESCRIPTIONColumn];
					}
					catch (InvalidCastException innerException)
					{
						throw new StrongTypingException("The value for column 'STAGE_SUBMIT_DESCRIPTION' in table 'WorkflowStage' is DBNull.", innerException);
					}
					return result;
				}
				set
				{
					base[this.tableWorkflowStage.STAGE_SUBMIT_DESCRIPTIONColumn] = value;
				}
			}

			// Token: 0x17003A67 RID: 14951
			// (get) Token: 0x0600BF29 RID: 48937 RVA: 0x00255988 File Offset: 0x00253B88
			// (set) Token: 0x0600BF2A RID: 48938 RVA: 0x002559CC File Offset: 0x00253BCC
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public Guid STATUS_PDP_UID
			{
				get
				{
					Guid result;
					try
					{
						result = (Guid)base[this.tableWorkflowStage.STATUS_PDP_UIDColumn];
					}
					catch (InvalidCastException innerException)
					{
						throw new StrongTypingException("The value for column 'STATUS_PDP_UID' in table 'WorkflowStage' is DBNull.", innerException);
					}
					return result;
				}
				set
				{
					base[this.tableWorkflowStage.STATUS_PDP_UIDColumn] = value;
				}
			}

			// Token: 0x17003A68 RID: 14952
			// (get) Token: 0x0600BF2B RID: 48939 RVA: 0x002559E5 File Offset: 0x00253BE5
			// (set) Token: 0x0600BF2C RID: 48940 RVA: 0x00255A07 File Offset: 0x00253C07
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			public WorkflowDataSet.WorkflowPhaseRow WorkflowPhaseRow
			{
				get
				{
					return (WorkflowDataSet.WorkflowPhaseRow)base.GetParentRow(base.Table.ParentRelations["FK_WorkflowPhase_WorkflowStage"]);
				}
				set
				{
					base.SetParentRow(value, base.Table.ParentRelations["FK_WorkflowPhase_WorkflowStage"]);
				}
			}

			// Token: 0x0600BF2D RID: 48941 RVA: 0x00255A25 File Offset: 0x00253C25
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public bool IsPHASE_NAMENull()
			{
				return base.IsNull(this.tableWorkflowStage.PHASE_NAMEColumn);
			}

			// Token: 0x0600BF2E RID: 48942 RVA: 0x00255A38 File Offset: 0x00253C38
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public void SetPHASE_NAMENull()
			{
				base[this.tableWorkflowStage.PHASE_NAMEColumn] = Convert.DBNull;
			}

			// Token: 0x0600BF2F RID: 48943 RVA: 0x00255A50 File Offset: 0x00253C50
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			public bool IsSTAGE_DESCRIPTIONNull()
			{
				return base.IsNull(this.tableWorkflowStage.STAGE_DESCRIPTIONColumn);
			}

			// Token: 0x0600BF30 RID: 48944 RVA: 0x00255A63 File Offset: 0x00253C63
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public void SetSTAGE_DESCRIPTIONNull()
			{
				base[this.tableWorkflowStage.STAGE_DESCRIPTIONColumn] = Convert.DBNull;
			}

			// Token: 0x0600BF31 RID: 48945 RVA: 0x00255A7B File Offset: 0x00253C7B
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public bool IsSTAGE_SUBMIT_DESCRIPTIONNull()
			{
				return base.IsNull(this.tableWorkflowStage.STAGE_SUBMIT_DESCRIPTIONColumn);
			}

			// Token: 0x0600BF32 RID: 48946 RVA: 0x00255A8E File Offset: 0x00253C8E
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			public void SetSTAGE_SUBMIT_DESCRIPTIONNull()
			{
				base[this.tableWorkflowStage.STAGE_SUBMIT_DESCRIPTIONColumn] = Convert.DBNull;
			}

			// Token: 0x0600BF33 RID: 48947 RVA: 0x00255AA6 File Offset: 0x00253CA6
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			public bool IsSTATUS_PDP_UIDNull()
			{
				return base.IsNull(this.tableWorkflowStage.STATUS_PDP_UIDColumn);
			}

			// Token: 0x0600BF34 RID: 48948 RVA: 0x00255AB9 File Offset: 0x00253CB9
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			public void SetSTATUS_PDP_UIDNull()
			{
				base[this.tableWorkflowStage.STATUS_PDP_UIDColumn] = Convert.DBNull;
			}

			// Token: 0x0600BF35 RID: 48949 RVA: 0x00255AD1 File Offset: 0x00253CD1
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public WorkflowDataSet.WorkflowStagePDPsRow[] GetWorkflowStagePDPsRows()
			{
				if (base.Table.ChildRelations["FK_WorkflowStage_WorkflowStageEDPs"] == null)
				{
					return new WorkflowDataSet.WorkflowStagePDPsRow[0];
				}
				return (WorkflowDataSet.WorkflowStagePDPsRow[])base.GetChildRows(base.Table.ChildRelations["FK_WorkflowStage_WorkflowStageEDPs"]);
			}

			// Token: 0x0600BF36 RID: 48950 RVA: 0x00255B11 File Offset: 0x00253D11
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			public WorkflowDataSet.WorkflowStageStrategicImpactRow[] GetWorkflowStageStrategicImpactRows()
			{
				if (base.Table.ChildRelations["FK_WorkflowStage_WorkflowStageStrategicImpact"] == null)
				{
					return new WorkflowDataSet.WorkflowStageStrategicImpactRow[0];
				}
				return (WorkflowDataSet.WorkflowStageStrategicImpactRow[])base.GetChildRows(base.Table.ChildRelations["FK_WorkflowStage_WorkflowStageStrategicImpact"]);
			}

			// Token: 0x0600BF37 RID: 48951 RVA: 0x00255B51 File Offset: 0x00253D51
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public WorkflowDataSet.WorkflowStageCustomFieldsRow[] GetWorkflowStageCustomFieldsRows()
			{
				if (base.Table.ChildRelations["FK_WorkflowStage_WorkflowStageCustomFields"] == null)
				{
					return new WorkflowDataSet.WorkflowStageCustomFieldsRow[0];
				}
				return (WorkflowDataSet.WorkflowStageCustomFieldsRow[])base.GetChildRows(base.Table.ChildRelations["FK_WorkflowStage_WorkflowStageCustomFields"]);
			}

			// Token: 0x040026B8 RID: 9912
			private WorkflowDataSet.WorkflowStageDataTable tableWorkflowStage;
		}

		// Token: 0x020007B8 RID: 1976
		public class WorkflowStageCustomFieldsRow : DataRow
		{
			// Token: 0x0600BF38 RID: 48952 RVA: 0x00255B91 File Offset: 0x00253D91
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			internal WorkflowStageCustomFieldsRow(DataRowBuilder rb) : base(rb)
			{
				this.tableWorkflowStageCustomFields = (WorkflowDataSet.WorkflowStageCustomFieldsDataTable)base.Table;
			}

			// Token: 0x17003A69 RID: 14953
			// (get) Token: 0x0600BF39 RID: 48953 RVA: 0x00255BAB File Offset: 0x00253DAB
			// (set) Token: 0x0600BF3A RID: 48954 RVA: 0x00255BC3 File Offset: 0x00253DC3
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			public Guid STAGE_UID
			{
				get
				{
					return (Guid)base[this.tableWorkflowStageCustomFields.STAGE_UIDColumn];
				}
				set
				{
					base[this.tableWorkflowStageCustomFields.STAGE_UIDColumn] = value;
				}
			}

			// Token: 0x17003A6A RID: 14954
			// (get) Token: 0x0600BF3B RID: 48955 RVA: 0x00255BDC File Offset: 0x00253DDC
			// (set) Token: 0x0600BF3C RID: 48956 RVA: 0x00255BF4 File Offset: 0x00253DF4
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			public Guid MD_PROP_UID
			{
				get
				{
					return (Guid)base[this.tableWorkflowStageCustomFields.MD_PROP_UIDColumn];
				}
				set
				{
					base[this.tableWorkflowStageCustomFields.MD_PROP_UIDColumn] = value;
				}
			}

			// Token: 0x17003A6B RID: 14955
			// (get) Token: 0x0600BF3D RID: 48957 RVA: 0x00255C0D File Offset: 0x00253E0D
			// (set) Token: 0x0600BF3E RID: 48958 RVA: 0x00255C25 File Offset: 0x00253E25
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			public string MD_PROP_NAME
			{
				get
				{
					return (string)base[this.tableWorkflowStageCustomFields.MD_PROP_NAMEColumn];
				}
				set
				{
					base[this.tableWorkflowStageCustomFields.MD_PROP_NAMEColumn] = value;
				}
			}

			// Token: 0x17003A6C RID: 14956
			// (get) Token: 0x0600BF3F RID: 48959 RVA: 0x00255C39 File Offset: 0x00253E39
			// (set) Token: 0x0600BF40 RID: 48960 RVA: 0x00255C51 File Offset: 0x00253E51
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public bool REQUIRED
			{
				get
				{
					return (bool)base[this.tableWorkflowStageCustomFields.REQUIREDColumn];
				}
				set
				{
					base[this.tableWorkflowStageCustomFields.REQUIREDColumn] = value;
				}
			}

			// Token: 0x17003A6D RID: 14957
			// (get) Token: 0x0600BF41 RID: 48961 RVA: 0x00255C6A File Offset: 0x00253E6A
			// (set) Token: 0x0600BF42 RID: 48962 RVA: 0x00255C82 File Offset: 0x00253E82
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			public bool READ_ONLY
			{
				get
				{
					return (bool)base[this.tableWorkflowStageCustomFields.READ_ONLYColumn];
				}
				set
				{
					base[this.tableWorkflowStageCustomFields.READ_ONLYColumn] = value;
				}
			}

			// Token: 0x17003A6E RID: 14958
			// (get) Token: 0x0600BF43 RID: 48963 RVA: 0x00255C9B File Offset: 0x00253E9B
			// (set) Token: 0x0600BF44 RID: 48964 RVA: 0x00255CBD File Offset: 0x00253EBD
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			public WorkflowDataSet.WorkflowStageRow WorkflowStageRow
			{
				get
				{
					return (WorkflowDataSet.WorkflowStageRow)base.GetParentRow(base.Table.ParentRelations["FK_WorkflowStage_WorkflowStageCustomFields"]);
				}
				set
				{
					base.SetParentRow(value, base.Table.ParentRelations["FK_WorkflowStage_WorkflowStageCustomFields"]);
				}
			}

			// Token: 0x040026B9 RID: 9913
			private WorkflowDataSet.WorkflowStageCustomFieldsDataTable tableWorkflowStageCustomFields;
		}

		// Token: 0x020007B9 RID: 1977
		public class WorkflowStageStrategicImpactRow : DataRow
		{
			// Token: 0x0600BF45 RID: 48965 RVA: 0x00255CDB File Offset: 0x00253EDB
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			internal WorkflowStageStrategicImpactRow(DataRowBuilder rb) : base(rb)
			{
				this.tableWorkflowStageStrategicImpact = (WorkflowDataSet.WorkflowStageStrategicImpactDataTable)base.Table;
			}

			// Token: 0x17003A6F RID: 14959
			// (get) Token: 0x0600BF46 RID: 48966 RVA: 0x00255CF5 File Offset: 0x00253EF5
			// (set) Token: 0x0600BF47 RID: 48967 RVA: 0x00255D0D File Offset: 0x00253F0D
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public Guid STAGE_UID
			{
				get
				{
					return (Guid)base[this.tableWorkflowStageStrategicImpact.STAGE_UIDColumn];
				}
				set
				{
					base[this.tableWorkflowStageStrategicImpact.STAGE_UIDColumn] = value;
				}
			}

			// Token: 0x17003A70 RID: 14960
			// (get) Token: 0x0600BF48 RID: 48968 RVA: 0x00255D28 File Offset: 0x00253F28
			// (set) Token: 0x0600BF49 RID: 48969 RVA: 0x00255D6C File Offset: 0x00253F6C
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			public byte BEHAVIOR
			{
				get
				{
					byte result;
					try
					{
						result = (byte)base[this.tableWorkflowStageStrategicImpact.BEHAVIORColumn];
					}
					catch (InvalidCastException innerException)
					{
						throw new StrongTypingException("The value for column 'BEHAVIOR' in table 'WorkflowStageStrategicImpact' is DBNull.", innerException);
					}
					return result;
				}
				set
				{
					base[this.tableWorkflowStageStrategicImpact.BEHAVIORColumn] = value;
				}
			}

			// Token: 0x17003A71 RID: 14961
			// (get) Token: 0x0600BF4A RID: 48970 RVA: 0x00255D85 File Offset: 0x00253F85
			// (set) Token: 0x0600BF4B RID: 48971 RVA: 0x00255DA7 File Offset: 0x00253FA7
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			public WorkflowDataSet.WorkflowStageRow WorkflowStageRow
			{
				get
				{
					return (WorkflowDataSet.WorkflowStageRow)base.GetParentRow(base.Table.ParentRelations["FK_WorkflowStage_WorkflowStageStrategicImpact"]);
				}
				set
				{
					base.SetParentRow(value, base.Table.ParentRelations["FK_WorkflowStage_WorkflowStageStrategicImpact"]);
				}
			}

			// Token: 0x0600BF4C RID: 48972 RVA: 0x00255DC5 File Offset: 0x00253FC5
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			public bool IsBEHAVIORNull()
			{
				return base.IsNull(this.tableWorkflowStageStrategicImpact.BEHAVIORColumn);
			}

			// Token: 0x0600BF4D RID: 48973 RVA: 0x00255DD8 File Offset: 0x00253FD8
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			public void SetBEHAVIORNull()
			{
				base[this.tableWorkflowStageStrategicImpact.BEHAVIORColumn] = Convert.DBNull;
			}

			// Token: 0x040026BA RID: 9914
			private WorkflowDataSet.WorkflowStageStrategicImpactDataTable tableWorkflowStageStrategicImpact;
		}

		// Token: 0x020007BA RID: 1978
		public class WorkflowStagePDPsRow : DataRow
		{
			// Token: 0x0600BF4E RID: 48974 RVA: 0x00255DF0 File Offset: 0x00253FF0
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			internal WorkflowStagePDPsRow(DataRowBuilder rb) : base(rb)
			{
				this.tableWorkflowStagePDPs = (WorkflowDataSet.WorkflowStagePDPsDataTable)base.Table;
			}

			// Token: 0x17003A72 RID: 14962
			// (get) Token: 0x0600BF4F RID: 48975 RVA: 0x00255E0A File Offset: 0x0025400A
			// (set) Token: 0x0600BF50 RID: 48976 RVA: 0x00255E22 File Offset: 0x00254022
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public Guid STAGE_UID
			{
				get
				{
					return (Guid)base[this.tableWorkflowStagePDPs.STAGE_UIDColumn];
				}
				set
				{
					base[this.tableWorkflowStagePDPs.STAGE_UIDColumn] = value;
				}
			}

			// Token: 0x17003A73 RID: 14963
			// (get) Token: 0x0600BF51 RID: 48977 RVA: 0x00255E3B File Offset: 0x0025403B
			// (set) Token: 0x0600BF52 RID: 48978 RVA: 0x00255E53 File Offset: 0x00254053
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public Guid PDP_UID
			{
				get
				{
					return (Guid)base[this.tableWorkflowStagePDPs.PDP_UIDColumn];
				}
				set
				{
					base[this.tableWorkflowStagePDPs.PDP_UIDColumn] = value;
				}
			}

			// Token: 0x17003A74 RID: 14964
			// (get) Token: 0x0600BF53 RID: 48979 RVA: 0x00255E6C File Offset: 0x0025406C
			// (set) Token: 0x0600BF54 RID: 48980 RVA: 0x00255EB0 File Offset: 0x002540B0
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public int PDP_ID
			{
				get
				{
					int result;
					try
					{
						result = (int)base[this.tableWorkflowStagePDPs.PDP_IDColumn];
					}
					catch (InvalidCastException innerException)
					{
						throw new StrongTypingException("The value for column 'PDP_ID' in table 'WorkflowStagePDPs' is DBNull.", innerException);
					}
					return result;
				}
				set
				{
					base[this.tableWorkflowStagePDPs.PDP_IDColumn] = value;
				}
			}

			// Token: 0x17003A75 RID: 14965
			// (get) Token: 0x0600BF55 RID: 48981 RVA: 0x00255EC9 File Offset: 0x002540C9
			// (set) Token: 0x0600BF56 RID: 48982 RVA: 0x00255EE1 File Offset: 0x002540E1
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			public string PDP_NAME
			{
				get
				{
					return (string)base[this.tableWorkflowStagePDPs.PDP_NAMEColumn];
				}
				set
				{
					base[this.tableWorkflowStagePDPs.PDP_NAMEColumn] = value;
				}
			}

			// Token: 0x17003A76 RID: 14966
			// (get) Token: 0x0600BF57 RID: 48983 RVA: 0x00255EF8 File Offset: 0x002540F8
			// (set) Token: 0x0600BF58 RID: 48984 RVA: 0x00255F3C File Offset: 0x0025413C
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public int PDP_POSITION
			{
				get
				{
					int result;
					try
					{
						result = (int)base[this.tableWorkflowStagePDPs.PDP_POSITIONColumn];
					}
					catch (InvalidCastException innerException)
					{
						throw new StrongTypingException("The value for column 'PDP_POSITION' in table 'WorkflowStagePDPs' is DBNull.", innerException);
					}
					return result;
				}
				set
				{
					base[this.tableWorkflowStagePDPs.PDP_POSITIONColumn] = value;
				}
			}

			// Token: 0x17003A77 RID: 14967
			// (get) Token: 0x0600BF59 RID: 48985 RVA: 0x00255F58 File Offset: 0x00254158
			// (set) Token: 0x0600BF5A RID: 48986 RVA: 0x00255F9C File Offset: 0x0025419C
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			public string PDP_STAGE_DESCRIPTION
			{
				get
				{
					string result;
					try
					{
						result = (string)base[this.tableWorkflowStagePDPs.PDP_STAGE_DESCRIPTIONColumn];
					}
					catch (InvalidCastException innerException)
					{
						throw new StrongTypingException("The value for column 'PDP_STAGE_DESCRIPTION' in table 'WorkflowStagePDPs' is DBNull.", innerException);
					}
					return result;
				}
				set
				{
					base[this.tableWorkflowStagePDPs.PDP_STAGE_DESCRIPTIONColumn] = value;
				}
			}

			// Token: 0x17003A78 RID: 14968
			// (get) Token: 0x0600BF5B RID: 48987 RVA: 0x00255FB0 File Offset: 0x002541B0
			// (set) Token: 0x0600BF5C RID: 48988 RVA: 0x00255FC8 File Offset: 0x002541C8
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public bool PDP_REQUIRES_ATTENTION
			{
				get
				{
					return (bool)base[this.tableWorkflowStagePDPs.PDP_REQUIRES_ATTENTIONColumn];
				}
				set
				{
					base[this.tableWorkflowStagePDPs.PDP_REQUIRES_ATTENTIONColumn] = value;
				}
			}

			// Token: 0x17003A79 RID: 14969
			// (get) Token: 0x0600BF5D RID: 48989 RVA: 0x00255FE1 File Offset: 0x002541E1
			// (set) Token: 0x0600BF5E RID: 48990 RVA: 0x00256003 File Offset: 0x00254203
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			public WorkflowDataSet.WorkflowStageRow WorkflowStageRow
			{
				get
				{
					return (WorkflowDataSet.WorkflowStageRow)base.GetParentRow(base.Table.ParentRelations["FK_WorkflowStage_WorkflowStageEDPs"]);
				}
				set
				{
					base.SetParentRow(value, base.Table.ParentRelations["FK_WorkflowStage_WorkflowStageEDPs"]);
				}
			}

			// Token: 0x0600BF5F RID: 48991 RVA: 0x00256021 File Offset: 0x00254221
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			public bool IsPDP_IDNull()
			{
				return base.IsNull(this.tableWorkflowStagePDPs.PDP_IDColumn);
			}

			// Token: 0x0600BF60 RID: 48992 RVA: 0x00256034 File Offset: 0x00254234
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public void SetPDP_IDNull()
			{
				base[this.tableWorkflowStagePDPs.PDP_IDColumn] = Convert.DBNull;
			}

			// Token: 0x0600BF61 RID: 48993 RVA: 0x0025604C File Offset: 0x0025424C
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			public bool IsPDP_POSITIONNull()
			{
				return base.IsNull(this.tableWorkflowStagePDPs.PDP_POSITIONColumn);
			}

			// Token: 0x0600BF62 RID: 48994 RVA: 0x0025605F File Offset: 0x0025425F
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			public void SetPDP_POSITIONNull()
			{
				base[this.tableWorkflowStagePDPs.PDP_POSITIONColumn] = Convert.DBNull;
			}

			// Token: 0x0600BF63 RID: 48995 RVA: 0x00256077 File Offset: 0x00254277
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public bool IsPDP_STAGE_DESCRIPTIONNull()
			{
				return base.IsNull(this.tableWorkflowStagePDPs.PDP_STAGE_DESCRIPTIONColumn);
			}

			// Token: 0x0600BF64 RID: 48996 RVA: 0x0025608A File Offset: 0x0025428A
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			public void SetPDP_STAGE_DESCRIPTIONNull()
			{
				base[this.tableWorkflowStagePDPs.PDP_STAGE_DESCRIPTIONColumn] = Convert.DBNull;
			}

			// Token: 0x040026BB RID: 9915
			private WorkflowDataSet.WorkflowStagePDPsDataTable tableWorkflowStagePDPs;
		}

		// Token: 0x020007BB RID: 1979
		public class WorkflowInstanceRow : DataRow
		{
			// Token: 0x0600BF65 RID: 48997 RVA: 0x002560A2 File Offset: 0x002542A2
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			internal WorkflowInstanceRow(DataRowBuilder rb) : base(rb)
			{
				this.tableWorkflowInstance = (WorkflowDataSet.WorkflowInstanceDataTable)base.Table;
			}

			// Token: 0x17003A7A RID: 14970
			// (get) Token: 0x0600BF66 RID: 48998 RVA: 0x002560BC File Offset: 0x002542BC
			// (set) Token: 0x0600BF67 RID: 48999 RVA: 0x00256100 File Offset: 0x00254300
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			public Guid WORKFLOW_INSTANCE_UID
			{
				get
				{
					Guid result;
					try
					{
						result = (Guid)base[this.tableWorkflowInstance.WORKFLOW_INSTANCE_UIDColumn];
					}
					catch (InvalidCastException innerException)
					{
						throw new StrongTypingException("The value for column 'WORKFLOW_INSTANCE_UID' in table 'WorkflowInstance' is DBNull.", innerException);
					}
					return result;
				}
				set
				{
					base[this.tableWorkflowInstance.WORKFLOW_INSTANCE_UIDColumn] = value;
				}
			}

			// Token: 0x17003A7B RID: 14971
			// (get) Token: 0x0600BF68 RID: 49000 RVA: 0x0025611C File Offset: 0x0025431C
			// (set) Token: 0x0600BF69 RID: 49001 RVA: 0x00256160 File Offset: 0x00254360
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			public int WORKFLOW_ENGINE_VERSION
			{
				get
				{
					int result;
					try
					{
						result = (int)base[this.tableWorkflowInstance.WORKFLOW_ENGINE_VERSIONColumn];
					}
					catch (InvalidCastException innerException)
					{
						throw new StrongTypingException("The value for column 'WORKFLOW_ENGINE_VERSION' in table 'WorkflowInstance' is DBNull.", innerException);
					}
					return result;
				}
				set
				{
					base[this.tableWorkflowInstance.WORKFLOW_ENGINE_VERSIONColumn] = value;
				}
			}

			// Token: 0x17003A7C RID: 14972
			// (get) Token: 0x0600BF6A RID: 49002 RVA: 0x00256179 File Offset: 0x00254379
			// (set) Token: 0x0600BF6B RID: 49003 RVA: 0x00256191 File Offset: 0x00254391
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public Guid PROJ_UID
			{
				get
				{
					return (Guid)base[this.tableWorkflowInstance.PROJ_UIDColumn];
				}
				set
				{
					base[this.tableWorkflowInstance.PROJ_UIDColumn] = value;
				}
			}

			// Token: 0x17003A7D RID: 14973
			// (get) Token: 0x0600BF6C RID: 49004 RVA: 0x002561AA File Offset: 0x002543AA
			// (set) Token: 0x0600BF6D RID: 49005 RVA: 0x002561C2 File Offset: 0x002543C2
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			public Guid ENTERPRISE_PROJECT_TYPE_UID
			{
				get
				{
					return (Guid)base[this.tableWorkflowInstance.ENTERPRISE_PROJECT_TYPE_UIDColumn];
				}
				set
				{
					base[this.tableWorkflowInstance.ENTERPRISE_PROJECT_TYPE_UIDColumn] = value;
				}
			}

			// Token: 0x17003A7E RID: 14974
			// (get) Token: 0x0600BF6E RID: 49006 RVA: 0x002561DB File Offset: 0x002543DB
			// (set) Token: 0x0600BF6F RID: 49007 RVA: 0x002561F3 File Offset: 0x002543F3
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public string ENTERPRISE_PROJECT_TYPE_NAME
			{
				get
				{
					return (string)base[this.tableWorkflowInstance.ENTERPRISE_PROJECT_TYPE_NAMEColumn];
				}
				set
				{
					base[this.tableWorkflowInstance.ENTERPRISE_PROJECT_TYPE_NAMEColumn] = value;
				}
			}

			// Token: 0x0600BF70 RID: 49008 RVA: 0x00256207 File Offset: 0x00254407
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public bool IsWORKFLOW_INSTANCE_UIDNull()
			{
				return base.IsNull(this.tableWorkflowInstance.WORKFLOW_INSTANCE_UIDColumn);
			}

			// Token: 0x0600BF71 RID: 49009 RVA: 0x0025621A File Offset: 0x0025441A
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			public void SetWORKFLOW_INSTANCE_UIDNull()
			{
				base[this.tableWorkflowInstance.WORKFLOW_INSTANCE_UIDColumn] = Convert.DBNull;
			}

			// Token: 0x0600BF72 RID: 49010 RVA: 0x00256232 File Offset: 0x00254432
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public bool IsWORKFLOW_ENGINE_VERSIONNull()
			{
				return base.IsNull(this.tableWorkflowInstance.WORKFLOW_ENGINE_VERSIONColumn);
			}

			// Token: 0x0600BF73 RID: 49011 RVA: 0x00256245 File Offset: 0x00254445
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			public void SetWORKFLOW_ENGINE_VERSIONNull()
			{
				base[this.tableWorkflowInstance.WORKFLOW_ENGINE_VERSIONColumn] = Convert.DBNull;
			}

			// Token: 0x040026BC RID: 9916
			private WorkflowDataSet.WorkflowInstanceDataTable tableWorkflowInstance;
		}

		// Token: 0x020007BC RID: 1980
		public class WorkflowAssociationRow : DataRow
		{
			// Token: 0x0600BF74 RID: 49012 RVA: 0x0025625D File Offset: 0x0025445D
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			internal WorkflowAssociationRow(DataRowBuilder rb) : base(rb)
			{
				this.tableWorkflowAssociation = (WorkflowDataSet.WorkflowAssociationDataTable)base.Table;
			}

			// Token: 0x17003A7F RID: 14975
			// (get) Token: 0x0600BF75 RID: 49013 RVA: 0x00256277 File Offset: 0x00254477
			// (set) Token: 0x0600BF76 RID: 49014 RVA: 0x0025628F File Offset: 0x0025448F
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public Guid WORKFLOW_ASSOCIATION_UID
			{
				get
				{
					return (Guid)base[this.tableWorkflowAssociation.WORKFLOW_ASSOCIATION_UIDColumn];
				}
				set
				{
					base[this.tableWorkflowAssociation.WORKFLOW_ASSOCIATION_UIDColumn] = value;
				}
			}

			// Token: 0x17003A80 RID: 14976
			// (get) Token: 0x0600BF77 RID: 49015 RVA: 0x002562A8 File Offset: 0x002544A8
			// (set) Token: 0x0600BF78 RID: 49016 RVA: 0x002562C0 File Offset: 0x002544C0
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			public string WORKFLOW_ASSOCIATION_NAME
			{
				get
				{
					return (string)base[this.tableWorkflowAssociation.WORKFLOW_ASSOCIATION_NAMEColumn];
				}
				set
				{
					base[this.tableWorkflowAssociation.WORKFLOW_ASSOCIATION_NAMEColumn] = value;
				}
			}

			// Token: 0x17003A81 RID: 14977
			// (get) Token: 0x0600BF79 RID: 49017 RVA: 0x002562D4 File Offset: 0x002544D4
			// (set) Token: 0x0600BF7A RID: 49018 RVA: 0x00256318 File Offset: 0x00254518
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public string WORKFLOW_ASSOCIATION_DESCRIPTION
			{
				get
				{
					string result;
					try
					{
						result = (string)base[this.tableWorkflowAssociation.WORKFLOW_ASSOCIATION_DESCRIPTIONColumn];
					}
					catch (InvalidCastException innerException)
					{
						throw new StrongTypingException("The value for column 'WORKFLOW_ASSOCIATION_DESCRIPTION' in table 'WorkflowAssociation' is DBNull.", innerException);
					}
					return result;
				}
				set
				{
					base[this.tableWorkflowAssociation.WORKFLOW_ASSOCIATION_DESCRIPTIONColumn] = value;
				}
			}

			// Token: 0x0600BF7B RID: 49019 RVA: 0x0025632C File Offset: 0x0025452C
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public bool IsWORKFLOW_ASSOCIATION_DESCRIPTIONNull()
			{
				return base.IsNull(this.tableWorkflowAssociation.WORKFLOW_ASSOCIATION_DESCRIPTIONColumn);
			}

			// Token: 0x0600BF7C RID: 49020 RVA: 0x0025633F File Offset: 0x0025453F
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			public void SetWORKFLOW_ASSOCIATION_DESCRIPTIONNull()
			{
				base[this.tableWorkflowAssociation.WORKFLOW_ASSOCIATION_DESCRIPTIONColumn] = Convert.DBNull;
			}

			// Token: 0x040026BD RID: 9917
			private WorkflowDataSet.WorkflowAssociationDataTable tableWorkflowAssociation;
		}

		// Token: 0x020007BD RID: 1981
		public class WorkflowStatusRow : DataRow
		{
			// Token: 0x0600BF7D RID: 49021 RVA: 0x00256357 File Offset: 0x00254557
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			internal WorkflowStatusRow(DataRowBuilder rb) : base(rb)
			{
				this.tableWorkflowStatus = (WorkflowDataSet.WorkflowStatusDataTable)base.Table;
			}

			// Token: 0x17003A82 RID: 14978
			// (get) Token: 0x0600BF7E RID: 49022 RVA: 0x00256371 File Offset: 0x00254571
			// (set) Token: 0x0600BF7F RID: 49023 RVA: 0x00256389 File Offset: 0x00254589
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			public Guid WORKFLOW_INSTANCE_UID
			{
				get
				{
					return (Guid)base[this.tableWorkflowStatus.WORKFLOW_INSTANCE_UIDColumn];
				}
				set
				{
					base[this.tableWorkflowStatus.WORKFLOW_INSTANCE_UIDColumn] = value;
				}
			}

			// Token: 0x17003A83 RID: 14979
			// (get) Token: 0x0600BF80 RID: 49024 RVA: 0x002563A2 File Offset: 0x002545A2
			// (set) Token: 0x0600BF81 RID: 49025 RVA: 0x002563BA File Offset: 0x002545BA
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public Guid PROJ_UID
			{
				get
				{
					return (Guid)base[this.tableWorkflowStatus.PROJ_UIDColumn];
				}
				set
				{
					base[this.tableWorkflowStatus.PROJ_UIDColumn] = value;
				}
			}

			// Token: 0x17003A84 RID: 14980
			// (get) Token: 0x0600BF82 RID: 49026 RVA: 0x002563D3 File Offset: 0x002545D3
			// (set) Token: 0x0600BF83 RID: 49027 RVA: 0x002563EB File Offset: 0x002545EB
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			public Guid STAGE_UID
			{
				get
				{
					return (Guid)base[this.tableWorkflowStatus.STAGE_UIDColumn];
				}
				set
				{
					base[this.tableWorkflowStatus.STAGE_UIDColumn] = value;
				}
			}

			// Token: 0x17003A85 RID: 14981
			// (get) Token: 0x0600BF84 RID: 49028 RVA: 0x00256404 File Offset: 0x00254604
			// (set) Token: 0x0600BF85 RID: 49029 RVA: 0x00256448 File Offset: 0x00254648
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public string STAGE_NAME
			{
				get
				{
					string result;
					try
					{
						result = (string)base[this.tableWorkflowStatus.STAGE_NAMEColumn];
					}
					catch (InvalidCastException innerException)
					{
						throw new StrongTypingException("The value for column 'STAGE_NAME' in table 'WorkflowStatus' is DBNull.", innerException);
					}
					return result;
				}
				set
				{
					base[this.tableWorkflowStatus.STAGE_NAMEColumn] = value;
				}
			}

			// Token: 0x17003A86 RID: 14982
			// (get) Token: 0x0600BF86 RID: 49030 RVA: 0x0025645C File Offset: 0x0025465C
			// (set) Token: 0x0600BF87 RID: 49031 RVA: 0x002564A0 File Offset: 0x002546A0
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public Guid PHASE_UID
			{
				get
				{
					Guid result;
					try
					{
						result = (Guid)base[this.tableWorkflowStatus.PHASE_UIDColumn];
					}
					catch (InvalidCastException innerException)
					{
						throw new StrongTypingException("The value for column 'PHASE_UID' in table 'WorkflowStatus' is DBNull.", innerException);
					}
					return result;
				}
				set
				{
					base[this.tableWorkflowStatus.PHASE_UIDColumn] = value;
				}
			}

			// Token: 0x17003A87 RID: 14983
			// (get) Token: 0x0600BF88 RID: 49032 RVA: 0x002564BC File Offset: 0x002546BC
			// (set) Token: 0x0600BF89 RID: 49033 RVA: 0x00256500 File Offset: 0x00254700
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			public string PHASE_NAME
			{
				get
				{
					string result;
					try
					{
						result = (string)base[this.tableWorkflowStatus.PHASE_NAMEColumn];
					}
					catch (InvalidCastException innerException)
					{
						throw new StrongTypingException("The value for column 'PHASE_NAME' in table 'WorkflowStatus' is DBNull.", innerException);
					}
					return result;
				}
				set
				{
					base[this.tableWorkflowStatus.PHASE_NAMEColumn] = value;
				}
			}

			// Token: 0x17003A88 RID: 14984
			// (get) Token: 0x0600BF8A RID: 49034 RVA: 0x00256514 File Offset: 0x00254714
			// (set) Token: 0x0600BF8B RID: 49035 RVA: 0x0025652C File Offset: 0x0025472C
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public int STAGE_STATUS
			{
				get
				{
					return (int)base[this.tableWorkflowStatus.STAGE_STATUSColumn];
				}
				set
				{
					base[this.tableWorkflowStatus.STAGE_STATUSColumn] = value;
				}
			}

			// Token: 0x17003A89 RID: 14985
			// (get) Token: 0x0600BF8C RID: 49036 RVA: 0x00256548 File Offset: 0x00254748
			// (set) Token: 0x0600BF8D RID: 49037 RVA: 0x0025658C File Offset: 0x0025478C
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			public string STAGE_INFO
			{
				get
				{
					string result;
					try
					{
						result = (string)base[this.tableWorkflowStatus.STAGE_INFOColumn];
					}
					catch (InvalidCastException innerException)
					{
						throw new StrongTypingException("The value for column 'STAGE_INFO' in table 'WorkflowStatus' is DBNull.", innerException);
					}
					return result;
				}
				set
				{
					base[this.tableWorkflowStatus.STAGE_INFOColumn] = value;
				}
			}

			// Token: 0x17003A8A RID: 14986
			// (get) Token: 0x0600BF8E RID: 49038 RVA: 0x002565A0 File Offset: 0x002547A0
			// (set) Token: 0x0600BF8F RID: 49039 RVA: 0x002565E4 File Offset: 0x002547E4
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public int STAGE_ORDER
			{
				get
				{
					int result;
					try
					{
						result = (int)base[this.tableWorkflowStatus.STAGE_ORDERColumn];
					}
					catch (InvalidCastException innerException)
					{
						throw new StrongTypingException("The value for column 'STAGE_ORDER' in table 'WorkflowStatus' is DBNull.", innerException);
					}
					return result;
				}
				set
				{
					base[this.tableWorkflowStatus.STAGE_ORDERColumn] = value;
				}
			}

			// Token: 0x17003A8B RID: 14987
			// (get) Token: 0x0600BF90 RID: 49040 RVA: 0x00256600 File Offset: 0x00254800
			// (set) Token: 0x0600BF91 RID: 49041 RVA: 0x00256644 File Offset: 0x00254844
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			public DateTime CREATED_DATE
			{
				get
				{
					DateTime result;
					try
					{
						result = (DateTime)base[this.tableWorkflowStatus.CREATED_DATEColumn];
					}
					catch (InvalidCastException innerException)
					{
						throw new StrongTypingException("The value for column 'CREATED_DATE' in table 'WorkflowStatus' is DBNull.", innerException);
					}
					return result;
				}
				set
				{
					base[this.tableWorkflowStatus.CREATED_DATEColumn] = value;
				}
			}

			// Token: 0x17003A8C RID: 14988
			// (get) Token: 0x0600BF92 RID: 49042 RVA: 0x00256660 File Offset: 0x00254860
			// (set) Token: 0x0600BF93 RID: 49043 RVA: 0x002566A4 File Offset: 0x002548A4
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			public DateTime MOD_DATE
			{
				get
				{
					DateTime result;
					try
					{
						result = (DateTime)base[this.tableWorkflowStatus.MOD_DATEColumn];
					}
					catch (InvalidCastException innerException)
					{
						throw new StrongTypingException("The value for column 'MOD_DATE' in table 'WorkflowStatus' is DBNull.", innerException);
					}
					return result;
				}
				set
				{
					base[this.tableWorkflowStatus.MOD_DATEColumn] = value;
				}
			}

			// Token: 0x17003A8D RID: 14989
			// (get) Token: 0x0600BF94 RID: 49044 RVA: 0x002566C0 File Offset: 0x002548C0
			// (set) Token: 0x0600BF95 RID: 49045 RVA: 0x00256704 File Offset: 0x00254904
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			public DateTime STAGE_ENTRY_DATE
			{
				get
				{
					DateTime result;
					try
					{
						result = (DateTime)base[this.tableWorkflowStatus.STAGE_ENTRY_DATEColumn];
					}
					catch (InvalidCastException innerException)
					{
						throw new StrongTypingException("The value for column 'STAGE_ENTRY_DATE' in table 'WorkflowStatus' is DBNull.", innerException);
					}
					return result;
				}
				set
				{
					base[this.tableWorkflowStatus.STAGE_ENTRY_DATEColumn] = value;
				}
			}

			// Token: 0x17003A8E RID: 14990
			// (get) Token: 0x0600BF96 RID: 49046 RVA: 0x00256720 File Offset: 0x00254920
			// (set) Token: 0x0600BF97 RID: 49047 RVA: 0x00256764 File Offset: 0x00254964
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			public DateTime STAGE_COMPLETION_DATE
			{
				get
				{
					DateTime result;
					try
					{
						result = (DateTime)base[this.tableWorkflowStatus.STAGE_COMPLETION_DATEColumn];
					}
					catch (InvalidCastException innerException)
					{
						throw new StrongTypingException("The value for column 'STAGE_COMPLETION_DATE' in table 'WorkflowStatus' is DBNull.", innerException);
					}
					return result;
				}
				set
				{
					base[this.tableWorkflowStatus.STAGE_COMPLETION_DATEColumn] = value;
				}
			}

			// Token: 0x17003A8F RID: 14991
			// (get) Token: 0x0600BF98 RID: 49048 RVA: 0x00256780 File Offset: 0x00254980
			// (set) Token: 0x0600BF99 RID: 49049 RVA: 0x002567C4 File Offset: 0x002549C4
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public DateTime WORKFLOW_MOD_DATE
			{
				get
				{
					DateTime result;
					try
					{
						result = (DateTime)base[this.tableWorkflowStatus.WORKFLOW_MOD_DATE];
					}
					catch (InvalidCastException innerException)
					{
						throw new StrongTypingException("The value for column 'WORKFLOW_MOD_DATE' in table 'WorkflowStatus' is DBNull.", innerException);
					}
					return result;
				}
				set
				{
					base[this.tableWorkflowStatus.WORKFLOW_MOD_DATE] = value;
				}
			}

			// Token: 0x17003A90 RID: 14992
			// (get) Token: 0x0600BF9A RID: 49050 RVA: 0x002567E0 File Offset: 0x002549E0
			// (set) Token: 0x0600BF9B RID: 49051 RVA: 0x00256824 File Offset: 0x00254A24
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			public DateTime SUBMITTED_DATE
			{
				get
				{
					DateTime result;
					try
					{
						result = (DateTime)base[this.tableWorkflowStatus.SUBMITTED_DATE];
					}
					catch (InvalidCastException innerException)
					{
						throw new StrongTypingException("The value for column 'SUBMITTED_DATE' in table 'WorkflowStatus' is DBNull.", innerException);
					}
					return result;
				}
				set
				{
					base[this.tableWorkflowStatus.SUBMITTED_DATE] = value;
				}
			}

			// Token: 0x17003A91 RID: 14993
			// (get) Token: 0x0600BF9C RID: 49052 RVA: 0x00256840 File Offset: 0x00254A40
			// (set) Token: 0x0600BF9D RID: 49053 RVA: 0x00256884 File Offset: 0x00254A84
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			public Guid NEXT_STAGE1
			{
				get
				{
					Guid result;
					try
					{
						result = (Guid)base[this.tableWorkflowStatus.NEXT_STAGE1Column];
					}
					catch (InvalidCastException innerException)
					{
						throw new StrongTypingException("The value for column 'NEXT_STAGE1' in table 'WorkflowStatus' is DBNull.", innerException);
					}
					return result;
				}
				set
				{
					base[this.tableWorkflowStatus.NEXT_STAGE1Column] = value;
				}
			}

			// Token: 0x17003A92 RID: 14994
			// (get) Token: 0x0600BF9E RID: 49054 RVA: 0x002568A0 File Offset: 0x00254AA0
			// (set) Token: 0x0600BF9F RID: 49055 RVA: 0x002568E4 File Offset: 0x00254AE4
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public Guid NEXT_STAGE2
			{
				get
				{
					Guid result;
					try
					{
						result = (Guid)base[this.tableWorkflowStatus.NEXT_STAGE2Column];
					}
					catch (InvalidCastException innerException)
					{
						throw new StrongTypingException("The value for column 'NEXT_STAGE2' in table 'WorkflowStatus' is DBNull.", innerException);
					}
					return result;
				}
				set
				{
					base[this.tableWorkflowStatus.NEXT_STAGE2Column] = value;
				}
			}

			// Token: 0x0600BFA0 RID: 49056 RVA: 0x002568FD File Offset: 0x00254AFD
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			public bool IsSTAGE_NAMENull()
			{
				return base.IsNull(this.tableWorkflowStatus.STAGE_NAMEColumn);
			}

			// Token: 0x0600BFA1 RID: 49057 RVA: 0x00256910 File Offset: 0x00254B10
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			public void SetSTAGE_NAMENull()
			{
				base[this.tableWorkflowStatus.STAGE_NAMEColumn] = Convert.DBNull;
			}

			// Token: 0x0600BFA2 RID: 49058 RVA: 0x00256928 File Offset: 0x00254B28
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			public bool IsPHASE_UIDNull()
			{
				return base.IsNull(this.tableWorkflowStatus.PHASE_UIDColumn);
			}

			// Token: 0x0600BFA3 RID: 49059 RVA: 0x0025693B File Offset: 0x00254B3B
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public void SetPHASE_UIDNull()
			{
				base[this.tableWorkflowStatus.PHASE_UIDColumn] = Convert.DBNull;
			}

			// Token: 0x0600BFA4 RID: 49060 RVA: 0x00256953 File Offset: 0x00254B53
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			public bool IsPHASE_NAMENull()
			{
				return base.IsNull(this.tableWorkflowStatus.PHASE_NAMEColumn);
			}

			// Token: 0x0600BFA5 RID: 49061 RVA: 0x00256966 File Offset: 0x00254B66
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public void SetPHASE_NAMENull()
			{
				base[this.tableWorkflowStatus.PHASE_NAMEColumn] = Convert.DBNull;
			}

			// Token: 0x0600BFA6 RID: 49062 RVA: 0x0025697E File Offset: 0x00254B7E
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public bool IsSTAGE_INFONull()
			{
				return base.IsNull(this.tableWorkflowStatus.STAGE_INFOColumn);
			}

			// Token: 0x0600BFA7 RID: 49063 RVA: 0x00256991 File Offset: 0x00254B91
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public void SetSTAGE_INFONull()
			{
				base[this.tableWorkflowStatus.STAGE_INFOColumn] = Convert.DBNull;
			}

			// Token: 0x0600BFA8 RID: 49064 RVA: 0x002569A9 File Offset: 0x00254BA9
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			public bool IsSTAGE_ORDERNull()
			{
				return base.IsNull(this.tableWorkflowStatus.STAGE_ORDERColumn);
			}

			// Token: 0x0600BFA9 RID: 49065 RVA: 0x002569BC File Offset: 0x00254BBC
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public void SetSTAGE_ORDERNull()
			{
				base[this.tableWorkflowStatus.STAGE_ORDERColumn] = Convert.DBNull;
			}

			// Token: 0x0600BFAA RID: 49066 RVA: 0x002569D4 File Offset: 0x00254BD4
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			public bool IsCREATED_DATENull()
			{
				return base.IsNull(this.tableWorkflowStatus.CREATED_DATEColumn);
			}

			// Token: 0x0600BFAB RID: 49067 RVA: 0x002569E7 File Offset: 0x00254BE7
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public void SetCREATED_DATENull()
			{
				base[this.tableWorkflowStatus.CREATED_DATEColumn] = Convert.DBNull;
			}

			// Token: 0x0600BFAC RID: 49068 RVA: 0x002569FF File Offset: 0x00254BFF
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public bool IsMOD_DATENull()
			{
				return base.IsNull(this.tableWorkflowStatus.MOD_DATEColumn);
			}

			// Token: 0x0600BFAD RID: 49069 RVA: 0x00256A12 File Offset: 0x00254C12
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public void SetMOD_DATENull()
			{
				base[this.tableWorkflowStatus.MOD_DATEColumn] = Convert.DBNull;
			}

			// Token: 0x0600BFAE RID: 49070 RVA: 0x00256A2A File Offset: 0x00254C2A
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			public bool IsSTAGE_ENTRY_DATENull()
			{
				return base.IsNull(this.tableWorkflowStatus.STAGE_ENTRY_DATEColumn);
			}

			// Token: 0x0600BFAF RID: 49071 RVA: 0x00256A3D File Offset: 0x00254C3D
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			public void SetSTAGE_ENTRY_DATENull()
			{
				base[this.tableWorkflowStatus.STAGE_ENTRY_DATEColumn] = Convert.DBNull;
			}

			// Token: 0x0600BFB0 RID: 49072 RVA: 0x00256A55 File Offset: 0x00254C55
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			public bool IsSTAGE_COMPLETION_DATENull()
			{
				return base.IsNull(this.tableWorkflowStatus.STAGE_COMPLETION_DATEColumn);
			}

			// Token: 0x0600BFB1 RID: 49073 RVA: 0x00256A68 File Offset: 0x00254C68
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public void SetSTAGE_COMPLETION_DATENull()
			{
				base[this.tableWorkflowStatus.STAGE_COMPLETION_DATEColumn] = Convert.DBNull;
			}

			// Token: 0x0600BFB2 RID: 49074 RVA: 0x00256A80 File Offset: 0x00254C80
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public bool IsWORKFLOW_MOD_DATENull()
			{
				return base.IsNull(this.tableWorkflowStatus.WORKFLOW_MOD_DATE);
			}

			// Token: 0x0600BFB3 RID: 49075 RVA: 0x00256A93 File Offset: 0x00254C93
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public void SetWORKFLOW_MOD_DATENull()
			{
				base[this.tableWorkflowStatus.WORKFLOW_MOD_DATE] = Convert.DBNull;
			}

			// Token: 0x0600BFB4 RID: 49076 RVA: 0x00256AAB File Offset: 0x00254CAB
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			public bool IsSUBMITTED_DATENull()
			{
				return base.IsNull(this.tableWorkflowStatus.SUBMITTED_DATE);
			}

			// Token: 0x0600BFB5 RID: 49077 RVA: 0x00256ABE File Offset: 0x00254CBE
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			public void SetSUBMITTED_DATENull()
			{
				base[this.tableWorkflowStatus.SUBMITTED_DATE] = Convert.DBNull;
			}

			// Token: 0x0600BFB6 RID: 49078 RVA: 0x00256AD6 File Offset: 0x00254CD6
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			public bool IsNEXT_STAGE1Null()
			{
				return base.IsNull(this.tableWorkflowStatus.NEXT_STAGE1Column);
			}

			// Token: 0x0600BFB7 RID: 49079 RVA: 0x00256AE9 File Offset: 0x00254CE9
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public void SetNEXT_STAGE1Null()
			{
				base[this.tableWorkflowStatus.NEXT_STAGE1Column] = Convert.DBNull;
			}

			// Token: 0x0600BFB8 RID: 49080 RVA: 0x00256B01 File Offset: 0x00254D01
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public bool IsNEXT_STAGE2Null()
			{
				return base.IsNull(this.tableWorkflowStatus.NEXT_STAGE2Column);
			}

			// Token: 0x0600BFB9 RID: 49081 RVA: 0x00256B14 File Offset: 0x00254D14
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public void SetNEXT_STAGE2Null()
			{
				base[this.tableWorkflowStatus.NEXT_STAGE2Column] = Convert.DBNull;
			}

			// Token: 0x040026BE RID: 9918
			private WorkflowDataSet.WorkflowStatusDataTable tableWorkflowStatus;
		}

		// Token: 0x020007BE RID: 1982
		public class EnterpriseProjectTypeRow : DataRow
		{
			// Token: 0x0600BFBA RID: 49082 RVA: 0x00256B2C File Offset: 0x00254D2C
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			internal EnterpriseProjectTypeRow(DataRowBuilder rb) : base(rb)
			{
				this.tableEnterpriseProjectType = (WorkflowDataSet.EnterpriseProjectTypeDataTable)base.Table;
			}

			// Token: 0x17003A93 RID: 14995
			// (get) Token: 0x0600BFBB RID: 49083 RVA: 0x00256B46 File Offset: 0x00254D46
			// (set) Token: 0x0600BFBC RID: 49084 RVA: 0x00256B5E File Offset: 0x00254D5E
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			public Guid ENTERPRISE_PROJECT_TYPE_UID
			{
				get
				{
					return (Guid)base[this.tableEnterpriseProjectType.ENTERPRISE_PROJECT_TYPE_UIDColumn];
				}
				set
				{
					base[this.tableEnterpriseProjectType.ENTERPRISE_PROJECT_TYPE_UIDColumn] = value;
				}
			}

			// Token: 0x17003A94 RID: 14996
			// (get) Token: 0x0600BFBD RID: 49085 RVA: 0x00256B77 File Offset: 0x00254D77
			// (set) Token: 0x0600BFBE RID: 49086 RVA: 0x00256B8F File Offset: 0x00254D8F
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			public string ENTERPRISE_PROJECT_TYPE_NAME
			{
				get
				{
					return (string)base[this.tableEnterpriseProjectType.ENTERPRISE_PROJECT_TYPE_NAMEColumn];
				}
				set
				{
					base[this.tableEnterpriseProjectType.ENTERPRISE_PROJECT_TYPE_NAMEColumn] = value;
				}
			}

			// Token: 0x17003A95 RID: 14997
			// (get) Token: 0x0600BFBF RID: 49087 RVA: 0x00256BA4 File Offset: 0x00254DA4
			// (set) Token: 0x0600BFC0 RID: 49088 RVA: 0x00256BE8 File Offset: 0x00254DE8
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			public string ENTERPRISE_PROJECT_TYPE_DESCRIPTION
			{
				get
				{
					string result;
					try
					{
						result = (string)base[this.tableEnterpriseProjectType.ENTERPRISE_PROJECT_TYPE_DESCRIPTIONColumn];
					}
					catch (InvalidCastException innerException)
					{
						throw new StrongTypingException("The value for column 'ENTERPRISE_PROJECT_TYPE_DESCRIPTION' in table 'EnterpriseProjectType' is DBNull.", innerException);
					}
					return result;
				}
				set
				{
					base[this.tableEnterpriseProjectType.ENTERPRISE_PROJECT_TYPE_DESCRIPTIONColumn] = value;
				}
			}

			// Token: 0x17003A96 RID: 14998
			// (get) Token: 0x0600BFC1 RID: 49089 RVA: 0x00256BFC File Offset: 0x00254DFC
			// (set) Token: 0x0600BFC2 RID: 49090 RVA: 0x00256C40 File Offset: 0x00254E40
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			public Guid WORKFLOW_ASSOCIATION_UID
			{
				get
				{
					Guid result;
					try
					{
						result = (Guid)base[this.tableEnterpriseProjectType.WORKFLOW_ASSOCIATION_UIDColumn];
					}
					catch (InvalidCastException innerException)
					{
						throw new StrongTypingException("The value for column 'WORKFLOW_ASSOCIATION_UID' in table 'EnterpriseProjectType' is DBNull.", innerException);
					}
					return result;
				}
				set
				{
					base[this.tableEnterpriseProjectType.WORKFLOW_ASSOCIATION_UIDColumn] = value;
				}
			}

			// Token: 0x17003A97 RID: 14999
			// (get) Token: 0x0600BFC3 RID: 49091 RVA: 0x00256C5C File Offset: 0x00254E5C
			// (set) Token: 0x0600BFC4 RID: 49092 RVA: 0x00256CA0 File Offset: 0x00254EA0
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			public string WORKFLOW_ASSOCIATION_NAME
			{
				get
				{
					string result;
					try
					{
						result = (string)base[this.tableEnterpriseProjectType.WORKFLOW_ASSOCIATION_NAMEColumn];
					}
					catch (InvalidCastException innerException)
					{
						throw new StrongTypingException("The value for column 'WORKFLOW_ASSOCIATION_NAME' in table 'EnterpriseProjectType' is DBNull.", innerException);
					}
					return result;
				}
				set
				{
					base[this.tableEnterpriseProjectType.WORKFLOW_ASSOCIATION_NAMEColumn] = value;
				}
			}

			// Token: 0x17003A98 RID: 15000
			// (get) Token: 0x0600BFC5 RID: 49093 RVA: 0x00256CB4 File Offset: 0x00254EB4
			// (set) Token: 0x0600BFC6 RID: 49094 RVA: 0x00256CCC File Offset: 0x00254ECC
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			public bool IS_DEFAULT_PROJECT_TYPE
			{
				get
				{
					return (bool)base[this.tableEnterpriseProjectType.IS_DEFAULT_PROJECT_TYPEColumn];
				}
				set
				{
					base[this.tableEnterpriseProjectType.IS_DEFAULT_PROJECT_TYPEColumn] = value;
				}
			}

			// Token: 0x17003A99 RID: 15001
			// (get) Token: 0x0600BFC7 RID: 49095 RVA: 0x00256CE8 File Offset: 0x00254EE8
			// (set) Token: 0x0600BFC8 RID: 49096 RVA: 0x00256D2C File Offset: 0x00254F2C
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			public Guid ENTERPRISE_PROJECT_PLAN_TEMPLATE_UID
			{
				get
				{
					Guid result;
					try
					{
						result = (Guid)base[this.tableEnterpriseProjectType.ENTERPRISE_PROJECT_PLAN_TEMPLATE_UIDColumn];
					}
					catch (InvalidCastException innerException)
					{
						throw new StrongTypingException("The value for column 'ENTERPRISE_PROJECT_PLAN_TEMPLATE_UID' in table 'EnterpriseProjectType' is DBNull.", innerException);
					}
					return result;
				}
				set
				{
					base[this.tableEnterpriseProjectType.ENTERPRISE_PROJECT_PLAN_TEMPLATE_UIDColumn] = value;
				}
			}

			// Token: 0x17003A9A RID: 15002
			// (get) Token: 0x0600BFC9 RID: 49097 RVA: 0x00256D45 File Offset: 0x00254F45
			// (set) Token: 0x0600BFCA RID: 49098 RVA: 0x00256D5D File Offset: 0x00254F5D
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			public string ENTERPRISE_PROJECT_WORKSPACE_TEMPLATE_NAME
			{
				get
				{
					return (string)base[this.tableEnterpriseProjectType.ENTERPRISE_PROJECT_WORKSPACE_TEMPLATE_NAMEColumn];
				}
				set
				{
					base[this.tableEnterpriseProjectType.ENTERPRISE_PROJECT_WORKSPACE_TEMPLATE_NAMEColumn] = value;
				}
			}

			// Token: 0x17003A9B RID: 15003
			// (get) Token: 0x0600BFCB RID: 49099 RVA: 0x00256D71 File Offset: 0x00254F71
			// (set) Token: 0x0600BFCC RID: 49100 RVA: 0x00256D89 File Offset: 0x00254F89
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			public int ENTERPRISE_PROJECT_TYPE_ORDER
			{
				get
				{
					return (int)base[this.tableEnterpriseProjectType.ENTERPRISE_PROJECT_TYPE_ORDERColumn];
				}
				set
				{
					base[this.tableEnterpriseProjectType.ENTERPRISE_PROJECT_TYPE_ORDERColumn] = value;
				}
			}

			// Token: 0x17003A9C RID: 15004
			// (get) Token: 0x0600BFCD RID: 49101 RVA: 0x00256DA4 File Offset: 0x00254FA4
			// (set) Token: 0x0600BFCE RID: 49102 RVA: 0x00256DE8 File Offset: 0x00254FE8
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public string ENTERPRISE_PROJECT_TYPE_IMAGE_URL
			{
				get
				{
					string result;
					try
					{
						result = (string)base[this.tableEnterpriseProjectType.ENTERPRISE_PROJECT_TYPE_IMAGE_URLColumn];
					}
					catch (InvalidCastException innerException)
					{
						throw new StrongTypingException("The value for column 'ENTERPRISE_PROJECT_TYPE_IMAGE_URL' in table 'EnterpriseProjectType' is DBNull.", innerException);
					}
					return result;
				}
				set
				{
					base[this.tableEnterpriseProjectType.ENTERPRISE_PROJECT_TYPE_IMAGE_URLColumn] = value;
				}
			}

			// Token: 0x17003A9D RID: 15005
			// (get) Token: 0x0600BFCF RID: 49103 RVA: 0x00256DFC File Offset: 0x00254FFC
			// (set) Token: 0x0600BFD0 RID: 49104 RVA: 0x00256E40 File Offset: 0x00255040
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public bool IS_MANAGED_PROJECT
			{
				get
				{
					bool result;
					try
					{
						result = (bool)base[this.tableEnterpriseProjectType.IS_MANAGED_PROJECTColumn];
					}
					catch (InvalidCastException innerException)
					{
						throw new StrongTypingException("The value for column 'IS_MANAGED_PROJECT' in table 'EnterpriseProjectType' is DBNull.", innerException);
					}
					return result;
				}
				set
				{
					base[this.tableEnterpriseProjectType.IS_MANAGED_PROJECTColumn] = value;
				}
			}

			// Token: 0x17003A9E RID: 15006
			// (get) Token: 0x0600BFD1 RID: 49105 RVA: 0x00256E5C File Offset: 0x0025505C
			// (set) Token: 0x0600BFD2 RID: 49106 RVA: 0x00256EA0 File Offset: 0x002550A0
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public string PROJ_IDENTIFIER_PREFIX
			{
				get
				{
					string result;
					try
					{
						result = (string)base[this.tableEnterpriseProjectType.PROJ_IDENTIFIER_PREFIXColumn];
					}
					catch (InvalidCastException innerException)
					{
						throw new StrongTypingException("The value for column 'PROJ_IDENTIFIER_PREFIX' in table 'EnterpriseProjectType' is DBNull.", innerException);
					}
					return result;
				}
				set
				{
					base[this.tableEnterpriseProjectType.PROJ_IDENTIFIER_PREFIXColumn] = value;
				}
			}

			// Token: 0x17003A9F RID: 15007
			// (get) Token: 0x0600BFD3 RID: 49107 RVA: 0x00256EB4 File Offset: 0x002550B4
			// (set) Token: 0x0600BFD4 RID: 49108 RVA: 0x00256EF8 File Offset: 0x002550F8
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			public int PROJ_IDENTIFIER_SEED
			{
				get
				{
					int result;
					try
					{
						result = (int)base[this.tableEnterpriseProjectType.PROJ_IDENTIFIER_SEEDColumn];
					}
					catch (InvalidCastException innerException)
					{
						throw new StrongTypingException("The value for column 'PROJ_IDENTIFIER_SEED' in table 'EnterpriseProjectType' is DBNull.", innerException);
					}
					return result;
				}
				set
				{
					base[this.tableEnterpriseProjectType.PROJ_IDENTIFIER_SEEDColumn] = value;
				}
			}

			// Token: 0x17003AA0 RID: 15008
			// (get) Token: 0x0600BFD5 RID: 49109 RVA: 0x00256F14 File Offset: 0x00255114
			// (set) Token: 0x0600BFD6 RID: 49110 RVA: 0x00256F58 File Offset: 0x00255158
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			public string PROJ_IDENTIFIER_POSTFIX
			{
				get
				{
					string result;
					try
					{
						result = (string)base[this.tableEnterpriseProjectType.PROJ_IDENTIFIER_POSTFIXColumn];
					}
					catch (InvalidCastException innerException)
					{
						throw new StrongTypingException("The value for column 'PROJ_IDENTIFIER_POSTFIX' in table 'EnterpriseProjectType' is DBNull.", innerException);
					}
					return result;
				}
				set
				{
					base[this.tableEnterpriseProjectType.PROJ_IDENTIFIER_POSTFIXColumn] = value;
				}
			}

			// Token: 0x17003AA1 RID: 15009
			// (get) Token: 0x0600BFD7 RID: 49111 RVA: 0x00256F6C File Offset: 0x0025516C
			// (set) Token: 0x0600BFD8 RID: 49112 RVA: 0x00256FB0 File Offset: 0x002551B0
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public int PROJ_IDENTIFIER_MINDIGIT
			{
				get
				{
					int result;
					try
					{
						result = (int)base[this.tableEnterpriseProjectType.PROJ_IDENTIFIER_MINDIGITColumn];
					}
					catch (InvalidCastException innerException)
					{
						throw new StrongTypingException("The value for column 'PROJ_IDENTIFIER_MINDIGIT' in table 'EnterpriseProjectType' is DBNull.", innerException);
					}
					return result;
				}
				set
				{
					base[this.tableEnterpriseProjectType.PROJ_IDENTIFIER_MINDIGITColumn] = value;
				}
			}

			// Token: 0x0600BFD9 RID: 49113 RVA: 0x00256FC9 File Offset: 0x002551C9
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			public bool IsENTERPRISE_PROJECT_TYPE_DESCRIPTIONNull()
			{
				return base.IsNull(this.tableEnterpriseProjectType.ENTERPRISE_PROJECT_TYPE_DESCRIPTIONColumn);
			}

			// Token: 0x0600BFDA RID: 49114 RVA: 0x00256FDC File Offset: 0x002551DC
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			public void SetENTERPRISE_PROJECT_TYPE_DESCRIPTIONNull()
			{
				base[this.tableEnterpriseProjectType.ENTERPRISE_PROJECT_TYPE_DESCRIPTIONColumn] = Convert.DBNull;
			}

			// Token: 0x0600BFDB RID: 49115 RVA: 0x00256FF4 File Offset: 0x002551F4
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			public bool IsWORKFLOW_ASSOCIATION_UIDNull()
			{
				return base.IsNull(this.tableEnterpriseProjectType.WORKFLOW_ASSOCIATION_UIDColumn);
			}

			// Token: 0x0600BFDC RID: 49116 RVA: 0x00257007 File Offset: 0x00255207
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public void SetWORKFLOW_ASSOCIATION_UIDNull()
			{
				base[this.tableEnterpriseProjectType.WORKFLOW_ASSOCIATION_UIDColumn] = Convert.DBNull;
			}

			// Token: 0x0600BFDD RID: 49117 RVA: 0x0025701F File Offset: 0x0025521F
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			public bool IsWORKFLOW_ASSOCIATION_NAMENull()
			{
				return base.IsNull(this.tableEnterpriseProjectType.WORKFLOW_ASSOCIATION_NAMEColumn);
			}

			// Token: 0x0600BFDE RID: 49118 RVA: 0x00257032 File Offset: 0x00255232
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public void SetWORKFLOW_ASSOCIATION_NAMENull()
			{
				base[this.tableEnterpriseProjectType.WORKFLOW_ASSOCIATION_NAMEColumn] = Convert.DBNull;
			}

			// Token: 0x0600BFDF RID: 49119 RVA: 0x0025704A File Offset: 0x0025524A
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			public bool IsENTERPRISE_PROJECT_PLAN_TEMPLATE_UIDNull()
			{
				return base.IsNull(this.tableEnterpriseProjectType.ENTERPRISE_PROJECT_PLAN_TEMPLATE_UIDColumn);
			}

			// Token: 0x0600BFE0 RID: 49120 RVA: 0x0025705D File Offset: 0x0025525D
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public void SetENTERPRISE_PROJECT_PLAN_TEMPLATE_UIDNull()
			{
				base[this.tableEnterpriseProjectType.ENTERPRISE_PROJECT_PLAN_TEMPLATE_UIDColumn] = Convert.DBNull;
			}

			// Token: 0x0600BFE1 RID: 49121 RVA: 0x00257075 File Offset: 0x00255275
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			public bool IsENTERPRISE_PROJECT_TYPE_IMAGE_URLNull()
			{
				return base.IsNull(this.tableEnterpriseProjectType.ENTERPRISE_PROJECT_TYPE_IMAGE_URLColumn);
			}

			// Token: 0x0600BFE2 RID: 49122 RVA: 0x00257088 File Offset: 0x00255288
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public void SetENTERPRISE_PROJECT_TYPE_IMAGE_URLNull()
			{
				base[this.tableEnterpriseProjectType.ENTERPRISE_PROJECT_TYPE_IMAGE_URLColumn] = Convert.DBNull;
			}

			// Token: 0x0600BFE3 RID: 49123 RVA: 0x002570A0 File Offset: 0x002552A0
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			public bool IsIS_MANAGED_PROJECTNull()
			{
				return base.IsNull(this.tableEnterpriseProjectType.IS_MANAGED_PROJECTColumn);
			}

			// Token: 0x0600BFE4 RID: 49124 RVA: 0x002570B3 File Offset: 0x002552B3
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public void SetIS_MANAGED_PROJECTNull()
			{
				base[this.tableEnterpriseProjectType.IS_MANAGED_PROJECTColumn] = Convert.DBNull;
			}

			// Token: 0x0600BFE5 RID: 49125 RVA: 0x002570CB File Offset: 0x002552CB
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			public bool IsPROJ_IDENTIFIER_PREFIXNull()
			{
				return base.IsNull(this.tableEnterpriseProjectType.PROJ_IDENTIFIER_PREFIXColumn);
			}

			// Token: 0x0600BFE6 RID: 49126 RVA: 0x002570DE File Offset: 0x002552DE
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			public void SetPROJ_IDENTIFIER_PREFIXNull()
			{
				base[this.tableEnterpriseProjectType.PROJ_IDENTIFIER_PREFIXColumn] = Convert.DBNull;
			}

			// Token: 0x0600BFE7 RID: 49127 RVA: 0x002570F6 File Offset: 0x002552F6
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			public bool IsPROJ_IDENTIFIER_SEEDNull()
			{
				return base.IsNull(this.tableEnterpriseProjectType.PROJ_IDENTIFIER_SEEDColumn);
			}

			// Token: 0x0600BFE8 RID: 49128 RVA: 0x00257109 File Offset: 0x00255309
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public void SetPROJ_IDENTIFIER_SEEDNull()
			{
				base[this.tableEnterpriseProjectType.PROJ_IDENTIFIER_SEEDColumn] = Convert.DBNull;
			}

			// Token: 0x0600BFE9 RID: 49129 RVA: 0x00257121 File Offset: 0x00255321
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public bool IsPROJ_IDENTIFIER_POSTFIXNull()
			{
				return base.IsNull(this.tableEnterpriseProjectType.PROJ_IDENTIFIER_POSTFIXColumn);
			}

			// Token: 0x0600BFEA RID: 49130 RVA: 0x00257134 File Offset: 0x00255334
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			public void SetPROJ_IDENTIFIER_POSTFIXNull()
			{
				base[this.tableEnterpriseProjectType.PROJ_IDENTIFIER_POSTFIXColumn] = Convert.DBNull;
			}

			// Token: 0x0600BFEB RID: 49131 RVA: 0x0025714C File Offset: 0x0025534C
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			public bool IsPROJ_IDENTIFIER_MINDIGITNull()
			{
				return base.IsNull(this.tableEnterpriseProjectType.PROJ_IDENTIFIER_MINDIGITColumn);
			}

			// Token: 0x0600BFEC RID: 49132 RVA: 0x0025715F File Offset: 0x0025535F
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			public void SetPROJ_IDENTIFIER_MINDIGITNull()
			{
				base[this.tableEnterpriseProjectType.PROJ_IDENTIFIER_MINDIGITColumn] = Convert.DBNull;
			}

			// Token: 0x0600BFED RID: 49133 RVA: 0x00257177 File Offset: 0x00255377
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			public WorkflowDataSet.EnterpriseProjectTypePDPsRow[] GetEnterpriseProjectTypePDPsRows()
			{
				if (base.Table.ChildRelations["FK_EnterpriseProjectType_EnterpriseProjectTypePDPs"] == null)
				{
					return new WorkflowDataSet.EnterpriseProjectTypePDPsRow[0];
				}
				return (WorkflowDataSet.EnterpriseProjectTypePDPsRow[])base.GetChildRows(base.Table.ChildRelations["FK_EnterpriseProjectType_EnterpriseProjectTypePDPs"]);
			}

			// Token: 0x0600BFEE RID: 49134 RVA: 0x002571B7 File Offset: 0x002553B7
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public WorkflowDataSet.EnterpriseProjectTypeDepartmentsRow[] GetEnterpriseProjectTypeDepartmentsRows()
			{
				if (base.Table.ChildRelations["EnterpriseProjectType_EnterpriseProjectTypeDepartments"] == null)
				{
					return new WorkflowDataSet.EnterpriseProjectTypeDepartmentsRow[0];
				}
				return (WorkflowDataSet.EnterpriseProjectTypeDepartmentsRow[])base.GetChildRows(base.Table.ChildRelations["EnterpriseProjectType_EnterpriseProjectTypeDepartments"]);
			}

			// Token: 0x040026BF RID: 9919
			private WorkflowDataSet.EnterpriseProjectTypeDataTable tableEnterpriseProjectType;
		}

		// Token: 0x020007BF RID: 1983
		public class EnterpriseProjectTypeDepartmentsRow : DataRow
		{
			// Token: 0x0600BFEF RID: 49135 RVA: 0x002571F7 File Offset: 0x002553F7
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			internal EnterpriseProjectTypeDepartmentsRow(DataRowBuilder rb) : base(rb)
			{
				this.tableEnterpriseProjectTypeDepartments = (WorkflowDataSet.EnterpriseProjectTypeDepartmentsDataTable)base.Table;
			}

			// Token: 0x17003AA2 RID: 15010
			// (get) Token: 0x0600BFF0 RID: 49136 RVA: 0x00257211 File Offset: 0x00255411
			// (set) Token: 0x0600BFF1 RID: 49137 RVA: 0x00257229 File Offset: 0x00255429
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public Guid ENTERPRISE_PROJECT_TYPE_UID
			{
				get
				{
					return (Guid)base[this.tableEnterpriseProjectTypeDepartments.ENTERPRISE_PROJECT_TYPE_UIDColumn];
				}
				set
				{
					base[this.tableEnterpriseProjectTypeDepartments.ENTERPRISE_PROJECT_TYPE_UIDColumn] = value;
				}
			}

			// Token: 0x17003AA3 RID: 15011
			// (get) Token: 0x0600BFF2 RID: 49138 RVA: 0x00257242 File Offset: 0x00255442
			// (set) Token: 0x0600BFF3 RID: 49139 RVA: 0x0025725A File Offset: 0x0025545A
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			public Guid DEPARTMENT_UID
			{
				get
				{
					return (Guid)base[this.tableEnterpriseProjectTypeDepartments.DEPARTMENT_UIDColumn];
				}
				set
				{
					base[this.tableEnterpriseProjectTypeDepartments.DEPARTMENT_UIDColumn] = value;
				}
			}

			// Token: 0x17003AA4 RID: 15012
			// (get) Token: 0x0600BFF4 RID: 49140 RVA: 0x00257273 File Offset: 0x00255473
			// (set) Token: 0x0600BFF5 RID: 49141 RVA: 0x00257295 File Offset: 0x00255495
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			public WorkflowDataSet.EnterpriseProjectTypeRow EnterpriseProjectTypeRow
			{
				get
				{
					return (WorkflowDataSet.EnterpriseProjectTypeRow)base.GetParentRow(base.Table.ParentRelations["EnterpriseProjectType_EnterpriseProjectTypeDepartments"]);
				}
				set
				{
					base.SetParentRow(value, base.Table.ParentRelations["EnterpriseProjectType_EnterpriseProjectTypeDepartments"]);
				}
			}

			// Token: 0x040026C0 RID: 9920
			private WorkflowDataSet.EnterpriseProjectTypeDepartmentsDataTable tableEnterpriseProjectTypeDepartments;
		}

		// Token: 0x020007C0 RID: 1984
		public class EnterpriseProjectTypePDPsRow : DataRow
		{
			// Token: 0x0600BFF6 RID: 49142 RVA: 0x002572B3 File Offset: 0x002554B3
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			internal EnterpriseProjectTypePDPsRow(DataRowBuilder rb) : base(rb)
			{
				this.tableEnterpriseProjectTypePDPs = (WorkflowDataSet.EnterpriseProjectTypePDPsDataTable)base.Table;
			}

			// Token: 0x17003AA5 RID: 15013
			// (get) Token: 0x0600BFF7 RID: 49143 RVA: 0x002572CD File Offset: 0x002554CD
			// (set) Token: 0x0600BFF8 RID: 49144 RVA: 0x002572E5 File Offset: 0x002554E5
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			public Guid ENTERPRISE_PROJECT_TYPE_UID
			{
				get
				{
					return (Guid)base[this.tableEnterpriseProjectTypePDPs.ENTERPRISE_PROJECT_TYPE_UIDColumn];
				}
				set
				{
					base[this.tableEnterpriseProjectTypePDPs.ENTERPRISE_PROJECT_TYPE_UIDColumn] = value;
				}
			}

			// Token: 0x17003AA6 RID: 15014
			// (get) Token: 0x0600BFF9 RID: 49145 RVA: 0x002572FE File Offset: 0x002554FE
			// (set) Token: 0x0600BFFA RID: 49146 RVA: 0x00257316 File Offset: 0x00255516
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public Guid PDP_UID
			{
				get
				{
					return (Guid)base[this.tableEnterpriseProjectTypePDPs.PDP_UIDColumn];
				}
				set
				{
					base[this.tableEnterpriseProjectTypePDPs.PDP_UIDColumn] = value;
				}
			}

			// Token: 0x17003AA7 RID: 15015
			// (get) Token: 0x0600BFFB RID: 49147 RVA: 0x0025732F File Offset: 0x0025552F
			// (set) Token: 0x0600BFFC RID: 49148 RVA: 0x00257347 File Offset: 0x00255547
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public int PDP_ID
			{
				get
				{
					return (int)base[this.tableEnterpriseProjectTypePDPs.PDP_IDColumn];
				}
				set
				{
					base[this.tableEnterpriseProjectTypePDPs.PDP_IDColumn] = value;
				}
			}

			// Token: 0x17003AA8 RID: 15016
			// (get) Token: 0x0600BFFD RID: 49149 RVA: 0x00257360 File Offset: 0x00255560
			// (set) Token: 0x0600BFFE RID: 49150 RVA: 0x002573A4 File Offset: 0x002555A4
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			public string PDP_NAME
			{
				get
				{
					string result;
					try
					{
						result = (string)base[this.tableEnterpriseProjectTypePDPs.PDP_NAMEColumn];
					}
					catch (InvalidCastException innerException)
					{
						throw new StrongTypingException("The value for column 'PDP_NAME' in table 'EnterpriseProjectTypePDPs' is DBNull.", innerException);
					}
					return result;
				}
				set
				{
					base[this.tableEnterpriseProjectTypePDPs.PDP_NAMEColumn] = value;
				}
			}

			// Token: 0x17003AA9 RID: 15017
			// (get) Token: 0x0600BFFF RID: 49151 RVA: 0x002573B8 File Offset: 0x002555B8
			// (set) Token: 0x0600C000 RID: 49152 RVA: 0x002573D0 File Offset: 0x002555D0
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public bool IS_CREATE_PDP
			{
				get
				{
					return (bool)base[this.tableEnterpriseProjectTypePDPs.IS_CREATE_PDPColumn];
				}
				set
				{
					base[this.tableEnterpriseProjectTypePDPs.IS_CREATE_PDPColumn] = value;
				}
			}

			// Token: 0x17003AAA RID: 15018
			// (get) Token: 0x0600C001 RID: 49153 RVA: 0x002573E9 File Offset: 0x002555E9
			// (set) Token: 0x0600C002 RID: 49154 RVA: 0x00257401 File Offset: 0x00255601
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			public int PDP_POSITION
			{
				get
				{
					return (int)base[this.tableEnterpriseProjectTypePDPs.PDP_POSITIONColumn];
				}
				set
				{
					base[this.tableEnterpriseProjectTypePDPs.PDP_POSITIONColumn] = value;
				}
			}

			// Token: 0x17003AAB RID: 15019
			// (get) Token: 0x0600C003 RID: 49155 RVA: 0x0025741A File Offset: 0x0025561A
			// (set) Token: 0x0600C004 RID: 49156 RVA: 0x0025743C File Offset: 0x0025563C
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public WorkflowDataSet.EnterpriseProjectTypeRow EnterpriseProjectTypeRow
			{
				get
				{
					return (WorkflowDataSet.EnterpriseProjectTypeRow)base.GetParentRow(base.Table.ParentRelations["FK_EnterpriseProjectType_EnterpriseProjectTypePDPs"]);
				}
				set
				{
					base.SetParentRow(value, base.Table.ParentRelations["FK_EnterpriseProjectType_EnterpriseProjectTypePDPs"]);
				}
			}

			// Token: 0x0600C005 RID: 49157 RVA: 0x0025745A File Offset: 0x0025565A
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			public bool IsPDP_NAMENull()
			{
				return base.IsNull(this.tableEnterpriseProjectTypePDPs.PDP_NAMEColumn);
			}

			// Token: 0x0600C006 RID: 49158 RVA: 0x0025746D File Offset: 0x0025566D
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public void SetPDP_NAMENull()
			{
				base[this.tableEnterpriseProjectTypePDPs.PDP_NAMEColumn] = Convert.DBNull;
			}

			// Token: 0x040026C1 RID: 9921
			private WorkflowDataSet.EnterpriseProjectTypePDPsDataTable tableEnterpriseProjectTypePDPs;
		}

		// Token: 0x020007C1 RID: 1985
		public class UpdateProjectWorkflowsRow : DataRow
		{
			// Token: 0x0600C007 RID: 49159 RVA: 0x00257485 File Offset: 0x00255685
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			internal UpdateProjectWorkflowsRow(DataRowBuilder rb) : base(rb)
			{
				this.tableUpdateProjectWorkflows = (WorkflowDataSet.UpdateProjectWorkflowsDataTable)base.Table;
			}

			// Token: 0x17003AAC RID: 15020
			// (get) Token: 0x0600C008 RID: 49160 RVA: 0x0025749F File Offset: 0x0025569F
			// (set) Token: 0x0600C009 RID: 49161 RVA: 0x002574B7 File Offset: 0x002556B7
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			public Guid JOB_UID
			{
				get
				{
					return (Guid)base[this.tableUpdateProjectWorkflows.JOB_UIDColumn];
				}
				set
				{
					base[this.tableUpdateProjectWorkflows.JOB_UIDColumn] = value;
				}
			}

			// Token: 0x17003AAD RID: 15021
			// (get) Token: 0x0600C00A RID: 49162 RVA: 0x002574D0 File Offset: 0x002556D0
			// (set) Token: 0x0600C00B RID: 49163 RVA: 0x002574E8 File Offset: 0x002556E8
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			public Guid PROJ_UID
			{
				get
				{
					return (Guid)base[this.tableUpdateProjectWorkflows.PROJ_UIDColumn];
				}
				set
				{
					base[this.tableUpdateProjectWorkflows.PROJ_UIDColumn] = value;
				}
			}

			// Token: 0x17003AAE RID: 15022
			// (get) Token: 0x0600C00C RID: 49164 RVA: 0x00257501 File Offset: 0x00255701
			// (set) Token: 0x0600C00D RID: 49165 RVA: 0x00257519 File Offset: 0x00255719
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			public Guid ENTERPRISE_PROJECT_TYPE_UID
			{
				get
				{
					return (Guid)base[this.tableUpdateProjectWorkflows.ENTERPRISE_PROJECT_TYPE_UIDColumn];
				}
				set
				{
					base[this.tableUpdateProjectWorkflows.ENTERPRISE_PROJECT_TYPE_UIDColumn] = value;
				}
			}

			// Token: 0x17003AAF RID: 15023
			// (get) Token: 0x0600C00E RID: 49166 RVA: 0x00257532 File Offset: 0x00255732
			// (set) Token: 0x0600C00F RID: 49167 RVA: 0x0025754A File Offset: 0x0025574A
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public bool SKIP_TO_CURRENT_STAGE
			{
				get
				{
					return (bool)base[this.tableUpdateProjectWorkflows.SKIP_TO_CURRENT_STAGEColumn];
				}
				set
				{
					base[this.tableUpdateProjectWorkflows.SKIP_TO_CURRENT_STAGEColumn] = value;
				}
			}

			// Token: 0x17003AB0 RID: 15024
			// (get) Token: 0x0600C010 RID: 49168 RVA: 0x00257564 File Offset: 0x00255764
			// (set) Token: 0x0600C011 RID: 49169 RVA: 0x002575A8 File Offset: 0x002557A8
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			public Guid STAGE_UID
			{
				get
				{
					Guid result;
					try
					{
						result = (Guid)base[this.tableUpdateProjectWorkflows.STAGE_UIDColumn];
					}
					catch (InvalidCastException innerException)
					{
						throw new StrongTypingException("The value for column 'STAGE_UID' in table 'UpdateProjectWorkflows' is DBNull.", innerException);
					}
					return result;
				}
				set
				{
					base[this.tableUpdateProjectWorkflows.STAGE_UIDColumn] = value;
				}
			}

			// Token: 0x0600C012 RID: 49170 RVA: 0x002575C1 File Offset: 0x002557C1
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public bool IsSTAGE_UIDNull()
			{
				return base.IsNull(this.tableUpdateProjectWorkflows.STAGE_UIDColumn);
			}

			// Token: 0x0600C013 RID: 49171 RVA: 0x002575D4 File Offset: 0x002557D4
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			public void SetSTAGE_UIDNull()
			{
				base[this.tableUpdateProjectWorkflows.STAGE_UIDColumn] = Convert.DBNull;
			}

			// Token: 0x040026C2 RID: 9922
			private WorkflowDataSet.UpdateProjectWorkflowsDataTable tableUpdateProjectWorkflows;
		}

		// Token: 0x020007C2 RID: 1986
		[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
		public class WorkflowPhaseRowChangeEvent : EventArgs
		{
			// Token: 0x0600C014 RID: 49172 RVA: 0x002575EC File Offset: 0x002557EC
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			public WorkflowPhaseRowChangeEvent(WorkflowDataSet.WorkflowPhaseRow row, DataRowAction action)
			{
				this.eventRow = row;
				this.eventAction = action;
			}

			// Token: 0x17003AB1 RID: 15025
			// (get) Token: 0x0600C015 RID: 49173 RVA: 0x00257602 File Offset: 0x00255802
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public WorkflowDataSet.WorkflowPhaseRow Row
			{
				get
				{
					return this.eventRow;
				}
			}

			// Token: 0x17003AB2 RID: 15026
			// (get) Token: 0x0600C016 RID: 49174 RVA: 0x0025760A File Offset: 0x0025580A
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public DataRowAction Action
			{
				get
				{
					return this.eventAction;
				}
			}

			// Token: 0x040026C3 RID: 9923
			private WorkflowDataSet.WorkflowPhaseRow eventRow;

			// Token: 0x040026C4 RID: 9924
			private DataRowAction eventAction;
		}

		// Token: 0x020007C3 RID: 1987
		[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
		public class WorkflowStageRowChangeEvent : EventArgs
		{
			// Token: 0x0600C017 RID: 49175 RVA: 0x00257612 File Offset: 0x00255812
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public WorkflowStageRowChangeEvent(WorkflowDataSet.WorkflowStageRow row, DataRowAction action)
			{
				this.eventRow = row;
				this.eventAction = action;
			}

			// Token: 0x17003AB3 RID: 15027
			// (get) Token: 0x0600C018 RID: 49176 RVA: 0x00257628 File Offset: 0x00255828
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			public WorkflowDataSet.WorkflowStageRow Row
			{
				get
				{
					return this.eventRow;
				}
			}

			// Token: 0x17003AB4 RID: 15028
			// (get) Token: 0x0600C019 RID: 49177 RVA: 0x00257630 File Offset: 0x00255830
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public DataRowAction Action
			{
				get
				{
					return this.eventAction;
				}
			}

			// Token: 0x040026C5 RID: 9925
			private WorkflowDataSet.WorkflowStageRow eventRow;

			// Token: 0x040026C6 RID: 9926
			private DataRowAction eventAction;
		}

		// Token: 0x020007C4 RID: 1988
		[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
		public class WorkflowStageCustomFieldsRowChangeEvent : EventArgs
		{
			// Token: 0x0600C01A RID: 49178 RVA: 0x00257638 File Offset: 0x00255838
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			public WorkflowStageCustomFieldsRowChangeEvent(WorkflowDataSet.WorkflowStageCustomFieldsRow row, DataRowAction action)
			{
				this.eventRow = row;
				this.eventAction = action;
			}

			// Token: 0x17003AB5 RID: 15029
			// (get) Token: 0x0600C01B RID: 49179 RVA: 0x0025764E File Offset: 0x0025584E
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			public WorkflowDataSet.WorkflowStageCustomFieldsRow Row
			{
				get
				{
					return this.eventRow;
				}
			}

			// Token: 0x17003AB6 RID: 15030
			// (get) Token: 0x0600C01C RID: 49180 RVA: 0x00257656 File Offset: 0x00255856
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public DataRowAction Action
			{
				get
				{
					return this.eventAction;
				}
			}

			// Token: 0x040026C7 RID: 9927
			private WorkflowDataSet.WorkflowStageCustomFieldsRow eventRow;

			// Token: 0x040026C8 RID: 9928
			private DataRowAction eventAction;
		}

		// Token: 0x020007C5 RID: 1989
		[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
		public class WorkflowStageStrategicImpactRowChangeEvent : EventArgs
		{
			// Token: 0x0600C01D RID: 49181 RVA: 0x0025765E File Offset: 0x0025585E
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			public WorkflowStageStrategicImpactRowChangeEvent(WorkflowDataSet.WorkflowStageStrategicImpactRow row, DataRowAction action)
			{
				this.eventRow = row;
				this.eventAction = action;
			}

			// Token: 0x17003AB7 RID: 15031
			// (get) Token: 0x0600C01E RID: 49182 RVA: 0x00257674 File Offset: 0x00255874
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			public WorkflowDataSet.WorkflowStageStrategicImpactRow Row
			{
				get
				{
					return this.eventRow;
				}
			}

			// Token: 0x17003AB8 RID: 15032
			// (get) Token: 0x0600C01F RID: 49183 RVA: 0x0025767C File Offset: 0x0025587C
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			public DataRowAction Action
			{
				get
				{
					return this.eventAction;
				}
			}

			// Token: 0x040026C9 RID: 9929
			private WorkflowDataSet.WorkflowStageStrategicImpactRow eventRow;

			// Token: 0x040026CA RID: 9930
			private DataRowAction eventAction;
		}

		// Token: 0x020007C6 RID: 1990
		[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
		public class WorkflowStagePDPsRowChangeEvent : EventArgs
		{
			// Token: 0x0600C020 RID: 49184 RVA: 0x00257684 File Offset: 0x00255884
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			public WorkflowStagePDPsRowChangeEvent(WorkflowDataSet.WorkflowStagePDPsRow row, DataRowAction action)
			{
				this.eventRow = row;
				this.eventAction = action;
			}

			// Token: 0x17003AB9 RID: 15033
			// (get) Token: 0x0600C021 RID: 49185 RVA: 0x0025769A File Offset: 0x0025589A
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			public WorkflowDataSet.WorkflowStagePDPsRow Row
			{
				get
				{
					return this.eventRow;
				}
			}

			// Token: 0x17003ABA RID: 15034
			// (get) Token: 0x0600C022 RID: 49186 RVA: 0x002576A2 File Offset: 0x002558A2
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public DataRowAction Action
			{
				get
				{
					return this.eventAction;
				}
			}

			// Token: 0x040026CB RID: 9931
			private WorkflowDataSet.WorkflowStagePDPsRow eventRow;

			// Token: 0x040026CC RID: 9932
			private DataRowAction eventAction;
		}

		// Token: 0x020007C7 RID: 1991
		[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
		public class WorkflowInstanceRowChangeEvent : EventArgs
		{
			// Token: 0x0600C023 RID: 49187 RVA: 0x002576AA File Offset: 0x002558AA
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public WorkflowInstanceRowChangeEvent(WorkflowDataSet.WorkflowInstanceRow row, DataRowAction action)
			{
				this.eventRow = row;
				this.eventAction = action;
			}

			// Token: 0x17003ABB RID: 15035
			// (get) Token: 0x0600C024 RID: 49188 RVA: 0x002576C0 File Offset: 0x002558C0
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public WorkflowDataSet.WorkflowInstanceRow Row
			{
				get
				{
					return this.eventRow;
				}
			}

			// Token: 0x17003ABC RID: 15036
			// (get) Token: 0x0600C025 RID: 49189 RVA: 0x002576C8 File Offset: 0x002558C8
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			public DataRowAction Action
			{
				get
				{
					return this.eventAction;
				}
			}

			// Token: 0x040026CD RID: 9933
			private WorkflowDataSet.WorkflowInstanceRow eventRow;

			// Token: 0x040026CE RID: 9934
			private DataRowAction eventAction;
		}

		// Token: 0x020007C8 RID: 1992
		[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
		public class WorkflowAssociationRowChangeEvent : EventArgs
		{
			// Token: 0x0600C026 RID: 49190 RVA: 0x002576D0 File Offset: 0x002558D0
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public WorkflowAssociationRowChangeEvent(WorkflowDataSet.WorkflowAssociationRow row, DataRowAction action)
			{
				this.eventRow = row;
				this.eventAction = action;
			}

			// Token: 0x17003ABD RID: 15037
			// (get) Token: 0x0600C027 RID: 49191 RVA: 0x002576E6 File Offset: 0x002558E6
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public WorkflowDataSet.WorkflowAssociationRow Row
			{
				get
				{
					return this.eventRow;
				}
			}

			// Token: 0x17003ABE RID: 15038
			// (get) Token: 0x0600C028 RID: 49192 RVA: 0x002576EE File Offset: 0x002558EE
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			public DataRowAction Action
			{
				get
				{
					return this.eventAction;
				}
			}

			// Token: 0x040026CF RID: 9935
			private WorkflowDataSet.WorkflowAssociationRow eventRow;

			// Token: 0x040026D0 RID: 9936
			private DataRowAction eventAction;
		}

		// Token: 0x020007C9 RID: 1993
		[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
		public class WorkflowStatusRowChangeEvent : EventArgs
		{
			// Token: 0x0600C029 RID: 49193 RVA: 0x002576F6 File Offset: 0x002558F6
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			public WorkflowStatusRowChangeEvent(WorkflowDataSet.WorkflowStatusRow row, DataRowAction action)
			{
				this.eventRow = row;
				this.eventAction = action;
			}

			// Token: 0x17003ABF RID: 15039
			// (get) Token: 0x0600C02A RID: 49194 RVA: 0x0025770C File Offset: 0x0025590C
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			public WorkflowDataSet.WorkflowStatusRow Row
			{
				get
				{
					return this.eventRow;
				}
			}

			// Token: 0x17003AC0 RID: 15040
			// (get) Token: 0x0600C02B RID: 49195 RVA: 0x00257714 File Offset: 0x00255914
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			public DataRowAction Action
			{
				get
				{
					return this.eventAction;
				}
			}

			// Token: 0x040026D1 RID: 9937
			private WorkflowDataSet.WorkflowStatusRow eventRow;

			// Token: 0x040026D2 RID: 9938
			private DataRowAction eventAction;
		}

		// Token: 0x020007CA RID: 1994
		[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
		public class EnterpriseProjectTypeRowChangeEvent : EventArgs
		{
			// Token: 0x0600C02C RID: 49196 RVA: 0x0025771C File Offset: 0x0025591C
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public EnterpriseProjectTypeRowChangeEvent(WorkflowDataSet.EnterpriseProjectTypeRow row, DataRowAction action)
			{
				this.eventRow = row;
				this.eventAction = action;
			}

			// Token: 0x17003AC1 RID: 15041
			// (get) Token: 0x0600C02D RID: 49197 RVA: 0x00257732 File Offset: 0x00255932
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public WorkflowDataSet.EnterpriseProjectTypeRow Row
			{
				get
				{
					return this.eventRow;
				}
			}

			// Token: 0x17003AC2 RID: 15042
			// (get) Token: 0x0600C02E RID: 49198 RVA: 0x0025773A File Offset: 0x0025593A
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			public DataRowAction Action
			{
				get
				{
					return this.eventAction;
				}
			}

			// Token: 0x040026D3 RID: 9939
			private WorkflowDataSet.EnterpriseProjectTypeRow eventRow;

			// Token: 0x040026D4 RID: 9940
			private DataRowAction eventAction;
		}

		// Token: 0x020007CB RID: 1995
		[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
		public class EnterpriseProjectTypeDepartmentsRowChangeEvent : EventArgs
		{
			// Token: 0x0600C02F RID: 49199 RVA: 0x00257742 File Offset: 0x00255942
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public EnterpriseProjectTypeDepartmentsRowChangeEvent(WorkflowDataSet.EnterpriseProjectTypeDepartmentsRow row, DataRowAction action)
			{
				this.eventRow = row;
				this.eventAction = action;
			}

			// Token: 0x17003AC3 RID: 15043
			// (get) Token: 0x0600C030 RID: 49200 RVA: 0x00257758 File Offset: 0x00255958
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public WorkflowDataSet.EnterpriseProjectTypeDepartmentsRow Row
			{
				get
				{
					return this.eventRow;
				}
			}

			// Token: 0x17003AC4 RID: 15044
			// (get) Token: 0x0600C031 RID: 49201 RVA: 0x00257760 File Offset: 0x00255960
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			public DataRowAction Action
			{
				get
				{
					return this.eventAction;
				}
			}

			// Token: 0x040026D5 RID: 9941
			private WorkflowDataSet.EnterpriseProjectTypeDepartmentsRow eventRow;

			// Token: 0x040026D6 RID: 9942
			private DataRowAction eventAction;
		}

		// Token: 0x020007CC RID: 1996
		[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
		public class EnterpriseProjectTypePDPsRowChangeEvent : EventArgs
		{
			// Token: 0x0600C032 RID: 49202 RVA: 0x00257768 File Offset: 0x00255968
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public EnterpriseProjectTypePDPsRowChangeEvent(WorkflowDataSet.EnterpriseProjectTypePDPsRow row, DataRowAction action)
			{
				this.eventRow = row;
				this.eventAction = action;
			}

			// Token: 0x17003AC5 RID: 15045
			// (get) Token: 0x0600C033 RID: 49203 RVA: 0x0025777E File Offset: 0x0025597E
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public WorkflowDataSet.EnterpriseProjectTypePDPsRow Row
			{
				get
				{
					return this.eventRow;
				}
			}

			// Token: 0x17003AC6 RID: 15046
			// (get) Token: 0x0600C034 RID: 49204 RVA: 0x00257786 File Offset: 0x00255986
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public DataRowAction Action
			{
				get
				{
					return this.eventAction;
				}
			}

			// Token: 0x040026D7 RID: 9943
			private WorkflowDataSet.EnterpriseProjectTypePDPsRow eventRow;

			// Token: 0x040026D8 RID: 9944
			private DataRowAction eventAction;
		}

		// Token: 0x020007CD RID: 1997
		[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
		public class UpdateProjectWorkflowsRowChangeEvent : EventArgs
		{
			// Token: 0x0600C035 RID: 49205 RVA: 0x0025778E File Offset: 0x0025598E
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public UpdateProjectWorkflowsRowChangeEvent(WorkflowDataSet.UpdateProjectWorkflowsRow row, DataRowAction action)
			{
				this.eventRow = row;
				this.eventAction = action;
			}

			// Token: 0x17003AC7 RID: 15047
			// (get) Token: 0x0600C036 RID: 49206 RVA: 0x002577A4 File Offset: 0x002559A4
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			public WorkflowDataSet.UpdateProjectWorkflowsRow Row
			{
				get
				{
					return this.eventRow;
				}
			}

			// Token: 0x17003AC8 RID: 15048
			// (get) Token: 0x0600C037 RID: 49207 RVA: 0x002577AC File Offset: 0x002559AC
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			public DataRowAction Action
			{
				get
				{
					return this.eventAction;
				}
			}

			// Token: 0x040026D9 RID: 9945
			private WorkflowDataSet.UpdateProjectWorkflowsRow eventRow;

			// Token: 0x040026DA RID: 9946
			private DataRowAction eventAction;
		}
	}
}
