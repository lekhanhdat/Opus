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
	// Token: 0x0200025F RID: 607
	[HelpKeyword("vs.data.DataSet")]
	[ToolboxItem(true)]
	[DesignerCategory("code")]
	[XmlRoot("PlannerSolutionDataSet")]
	[XmlSchemaProvider("GetTypedDataSetSchema")]
	[Serializable]
	public class PlannerSolutionDataSet : DataSet
	{
		// Token: 0x06003013 RID: 12307 RVA: 0x00099DBC File Offset: 0x00097FBC
		public new void EndInit()
		{
			base.EndInit();
			TypedDataSetUtilities.AllowNullsInNonTypedColumns(this.SolutionResHired, new string[]
			{
				"CUSTOM_FIELD_UID",
				"PROJ_NAME",
				"DURATION",
				"RES_HIRE_UID",
				"SOLUTION_UID",
				"ROLE_NAME",
				"RESOURCE_WORK",
				"START_DATE",
				"PROJ_UID",
				"LT_STRUCT_UID",
				"RESOURCE_COST"
			});
			TypedDataSetUtilities.AllowNullsInNonTypedColumns(this.Solutions, new string[]
			{
				"ANALYSIS_NAME",
				"CREATED_DATE",
				"CREATED_BY_RES_UID",
				"SOLUTION_UID",
				"OPTIMIZER_SOLUTION_NAME",
				"CONSTRAINT_TYPE",
				"ALLOCATION_THRESHOLD",
				"OPT_ENF_PROJ_DEP",
				"LAST_UPDATED_BY_RES_NAME",
				"ANALYSIS_UID",
				"HIRING_TYPE",
				"LAST_UPDATED_BY_RES_UID",
				"TOTAL_PRIORITY_VALUE",
				"RATE_TABLE",
				"SOLUTION_DESCRIPTION",
				"FRONTIER_UID",
				"OPTIMIZER_SOLUTION_UID",
				"SOLUTION_NAME",
				"MOD_DATE",
				"OPT_ENF_SCHEDULING_CONS",
				"CONSTRAINT_VALUE",
				"CREATED_BY_RES_NAME"
			});
			TypedDataSetUtilities.AllowNullsInNonTypedColumns(this.SolutionProjectRequirementsByRole, new string[]
			{
				"CUSTOM_FIELD_UID",
				"PROJ_NAME",
				"REQUIREMENT_UID",
				"SOLUTION_UID",
				"ROLE_NAME",
				"START_DATE",
				"ADDITIONAL_WORK",
				"PROJ_UID",
				"LT_STRUCT_UID"
			});
			TypedDataSetUtilities.AllowNullsInNonTypedColumns(this.SolutionProjects, new string[]
			{
				"NEW_START_DATE",
				"DURATION",
				"PROJ_NAME",
				"FORCE_ALIAS_LT_VALUE_FULL",
				"FNLT",
				"STATUS",
				"SOLUTION_UID",
				"FORCE_ALIAS_LT_STRUCT_UID",
				"RESOURCE_WORK",
				"FORCE_STATUS",
				"ABSOLUTE_PRIORITY",
				"PRIORITY",
				"PROJ_UID",
				"SNET",
				"RESOURCE_COST",
				"LOCKED"
			});
			TypedDataSetUtilities.AllowNullsInNonTypedColumns(this.SolutionEfficientFrontier, new string[]
			{
				"POINT_UID",
				"ANALYSIS_UID",
				"Y_VALUE",
				"X_VALUE",
				"FRONTIER_UID"
			});
		}

		// Token: 0x06003014 RID: 12308 RVA: 0x0009A048 File Offset: 0x00098248
		[DebuggerNonUserCode]
		[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
		public PlannerSolutionDataSet()
		{
			base.BeginInit();
			this.InitClass();
			CollectionChangeEventHandler value = new CollectionChangeEventHandler(this.SchemaChanged);
			base.Tables.CollectionChanged += value;
			base.Relations.CollectionChanged += value;
			this.EndInit();
		}

		// Token: 0x06003015 RID: 12309 RVA: 0x0009A09C File Offset: 0x0009829C
		[DebuggerNonUserCode]
		[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
		protected PlannerSolutionDataSet(SerializationInfo info, StreamingContext context) : base(info, context, false)
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
				if (dataSet.Tables["Solutions"] != null)
				{
					base.Tables.Add(new PlannerSolutionDataSet.SolutionsDataTable(dataSet.Tables["Solutions"]));
				}
				if (dataSet.Tables["SolutionResHired"] != null)
				{
					base.Tables.Add(new PlannerSolutionDataSet.SolutionResHiredDataTable(dataSet.Tables["SolutionResHired"]));
				}
				if (dataSet.Tables["SolutionProjects"] != null)
				{
					base.Tables.Add(new PlannerSolutionDataSet.SolutionProjectsDataTable(dataSet.Tables["SolutionProjects"]));
				}
				if (dataSet.Tables["SolutionProjectRequirementsByRole"] != null)
				{
					base.Tables.Add(new PlannerSolutionDataSet.SolutionProjectRequirementsByRoleDataTable(dataSet.Tables["SolutionProjectRequirementsByRole"]));
				}
				if (dataSet.Tables["SolutionEfficientFrontier"] != null)
				{
					base.Tables.Add(new PlannerSolutionDataSet.SolutionEfficientFrontierDataTable(dataSet.Tables["SolutionEfficientFrontier"]));
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

		// Token: 0x17000DF8 RID: 3576
		// (get) Token: 0x06003016 RID: 12310 RVA: 0x0009A2C1 File Offset: 0x000984C1
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Content)]
		[Browsable(false)]
		[DebuggerNonUserCode]
		[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
		public PlannerSolutionDataSet.SolutionsDataTable Solutions
		{
			get
			{
				return this.tableSolutions;
			}
		}

		// Token: 0x17000DF9 RID: 3577
		// (get) Token: 0x06003017 RID: 12311 RVA: 0x0009A2C9 File Offset: 0x000984C9
		[DebuggerNonUserCode]
		[Browsable(false)]
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Content)]
		[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
		public PlannerSolutionDataSet.SolutionResHiredDataTable SolutionResHired
		{
			get
			{
				return this.tableSolutionResHired;
			}
		}

		// Token: 0x17000DFA RID: 3578
		// (get) Token: 0x06003018 RID: 12312 RVA: 0x0009A2D1 File Offset: 0x000984D1
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Content)]
		[DebuggerNonUserCode]
		[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
		[Browsable(false)]
		public PlannerSolutionDataSet.SolutionProjectsDataTable SolutionProjects
		{
			get
			{
				return this.tableSolutionProjects;
			}
		}

		// Token: 0x17000DFB RID: 3579
		// (get) Token: 0x06003019 RID: 12313 RVA: 0x0009A2D9 File Offset: 0x000984D9
		[Browsable(false)]
		[DebuggerNonUserCode]
		[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Content)]
		public PlannerSolutionDataSet.SolutionProjectRequirementsByRoleDataTable SolutionProjectRequirementsByRole
		{
			get
			{
				return this.tableSolutionProjectRequirementsByRole;
			}
		}

		// Token: 0x17000DFC RID: 3580
		// (get) Token: 0x0600301A RID: 12314 RVA: 0x0009A2E1 File Offset: 0x000984E1
		[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
		[DebuggerNonUserCode]
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Content)]
		[Browsable(false)]
		public PlannerSolutionDataSet.SolutionEfficientFrontierDataTable SolutionEfficientFrontier
		{
			get
			{
				return this.tableSolutionEfficientFrontier;
			}
		}

		// Token: 0x17000DFD RID: 3581
		// (get) Token: 0x0600301B RID: 12315 RVA: 0x0009A2E9 File Offset: 0x000984E9
		// (set) Token: 0x0600301C RID: 12316 RVA: 0x0009A2F1 File Offset: 0x000984F1
		[DebuggerNonUserCode]
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Visible)]
		[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
		[Browsable(true)]
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

		// Token: 0x17000DFE RID: 3582
		// (get) Token: 0x0600301D RID: 12317 RVA: 0x0009A2FA File Offset: 0x000984FA
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

		// Token: 0x17000DFF RID: 3583
		// (get) Token: 0x0600301E RID: 12318 RVA: 0x0009A302 File Offset: 0x00098502
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
		[DebuggerNonUserCode]
		[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
		public new DataRelationCollection Relations
		{
			get
			{
				return base.Relations;
			}
		}

		// Token: 0x0600301F RID: 12319 RVA: 0x0009A30A File Offset: 0x0009850A
		[DebuggerNonUserCode]
		[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
		protected override void InitializeDerivedDataSet()
		{
			base.BeginInit();
			this.InitClass();
			this.EndInit();
		}

		// Token: 0x06003020 RID: 12320 RVA: 0x0009A320 File Offset: 0x00098520
		[DebuggerNonUserCode]
		[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
		public override DataSet Clone()
		{
			PlannerSolutionDataSet plannerSolutionDataSet = (PlannerSolutionDataSet)base.Clone();
			plannerSolutionDataSet.InitVars();
			plannerSolutionDataSet.SchemaSerializationMode = this.SchemaSerializationMode;
			return plannerSolutionDataSet;
		}

		// Token: 0x06003021 RID: 12321 RVA: 0x0009A34C File Offset: 0x0009854C
		[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
		[DebuggerNonUserCode]
		protected override bool ShouldSerializeTables()
		{
			return false;
		}

		// Token: 0x06003022 RID: 12322 RVA: 0x0009A34F File Offset: 0x0009854F
		[DebuggerNonUserCode]
		[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
		protected override bool ShouldSerializeRelations()
		{
			return false;
		}

		// Token: 0x06003023 RID: 12323 RVA: 0x0009A354 File Offset: 0x00098554
		[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
		[DebuggerNonUserCode]
		protected override void ReadXmlSerializable(XmlReader reader)
		{
			if (base.DetermineSchemaSerializationMode(reader) == SchemaSerializationMode.IncludeSchema)
			{
				this.Reset();
				DataSet dataSet = new DataSet();
				dataSet.ReadXml(reader);
				if (dataSet.Tables["Solutions"] != null)
				{
					base.Tables.Add(new PlannerSolutionDataSet.SolutionsDataTable(dataSet.Tables["Solutions"]));
				}
				if (dataSet.Tables["SolutionResHired"] != null)
				{
					base.Tables.Add(new PlannerSolutionDataSet.SolutionResHiredDataTable(dataSet.Tables["SolutionResHired"]));
				}
				if (dataSet.Tables["SolutionProjects"] != null)
				{
					base.Tables.Add(new PlannerSolutionDataSet.SolutionProjectsDataTable(dataSet.Tables["SolutionProjects"]));
				}
				if (dataSet.Tables["SolutionProjectRequirementsByRole"] != null)
				{
					base.Tables.Add(new PlannerSolutionDataSet.SolutionProjectRequirementsByRoleDataTable(dataSet.Tables["SolutionProjectRequirementsByRole"]));
				}
				if (dataSet.Tables["SolutionEfficientFrontier"] != null)
				{
					base.Tables.Add(new PlannerSolutionDataSet.SolutionEfficientFrontierDataTable(dataSet.Tables["SolutionEfficientFrontier"]));
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

		// Token: 0x06003024 RID: 12324 RVA: 0x0009A4E4 File Offset: 0x000986E4
		[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
		[DebuggerNonUserCode]
		protected override XmlSchema GetSchemaSerializable()
		{
			MemoryStream memoryStream = new MemoryStream();
			base.WriteXmlSchema(new XmlTextWriter(memoryStream, null));
			memoryStream.Position = 0L;
			return XmlSchema.Read(new XmlTextReader(memoryStream), null);
		}

		// Token: 0x06003025 RID: 12325 RVA: 0x0009A518 File Offset: 0x00098718
		[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
		[DebuggerNonUserCode]
		internal void InitVars()
		{
			this.InitVars(true);
		}

		// Token: 0x06003026 RID: 12326 RVA: 0x0009A524 File Offset: 0x00098724
		[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
		[DebuggerNonUserCode]
		internal void InitVars(bool initTable)
		{
			this.tableSolutions = (PlannerSolutionDataSet.SolutionsDataTable)base.Tables["Solutions"];
			if (initTable && this.tableSolutions != null)
			{
				this.tableSolutions.InitVars();
			}
			this.tableSolutionResHired = (PlannerSolutionDataSet.SolutionResHiredDataTable)base.Tables["SolutionResHired"];
			if (initTable && this.tableSolutionResHired != null)
			{
				this.tableSolutionResHired.InitVars();
			}
			this.tableSolutionProjects = (PlannerSolutionDataSet.SolutionProjectsDataTable)base.Tables["SolutionProjects"];
			if (initTable && this.tableSolutionProjects != null)
			{
				this.tableSolutionProjects.InitVars();
			}
			this.tableSolutionProjectRequirementsByRole = (PlannerSolutionDataSet.SolutionProjectRequirementsByRoleDataTable)base.Tables["SolutionProjectRequirementsByRole"];
			if (initTable && this.tableSolutionProjectRequirementsByRole != null)
			{
				this.tableSolutionProjectRequirementsByRole.InitVars();
			}
			this.tableSolutionEfficientFrontier = (PlannerSolutionDataSet.SolutionEfficientFrontierDataTable)base.Tables["SolutionEfficientFrontier"];
			if (initTable && this.tableSolutionEfficientFrontier != null)
			{
				this.tableSolutionEfficientFrontier.InitVars();
			}
			this.relationFK_Solutions_SolutionResHired = this.Relations["FK_Solutions_SolutionResHired"];
			this.relationFK_Solutions_SolutionProjects = this.Relations["FK_Solutions_SolutionProjects"];
			this.relationFK_Solutions_SolutionProjectRequirementsByRole = this.Relations["FK_Solutions_SolutionProjectRequirementsByRole"];
			this.relationFK_Solutions_SolutionEfficientFrontier = this.Relations["FK_Solutions_SolutionEfficientFrontier"];
		}

		// Token: 0x06003027 RID: 12327 RVA: 0x0009A680 File Offset: 0x00098880
		[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
		[DebuggerNonUserCode]
		private void InitClass()
		{
			base.DataSetName = "PlannerSolutionDataSet";
			base.Prefix = "";
			base.Namespace = "http://schemas.microsoft.com/office/project/server/webservices/PlannerSolutionDataSet";
			base.EnforceConstraints = true;
			this.SchemaSerializationMode = SchemaSerializationMode.IncludeSchema;
			this.tableSolutions = new PlannerSolutionDataSet.SolutionsDataTable();
			base.Tables.Add(this.tableSolutions);
			this.tableSolutionResHired = new PlannerSolutionDataSet.SolutionResHiredDataTable();
			base.Tables.Add(this.tableSolutionResHired);
			this.tableSolutionProjects = new PlannerSolutionDataSet.SolutionProjectsDataTable();
			base.Tables.Add(this.tableSolutionProjects);
			this.tableSolutionProjectRequirementsByRole = new PlannerSolutionDataSet.SolutionProjectRequirementsByRoleDataTable();
			base.Tables.Add(this.tableSolutionProjectRequirementsByRole);
			this.tableSolutionEfficientFrontier = new PlannerSolutionDataSet.SolutionEfficientFrontierDataTable();
			base.Tables.Add(this.tableSolutionEfficientFrontier);
			ForeignKeyConstraint foreignKeyConstraint = new ForeignKeyConstraint("FK_Solutions_SolutionResHired", new DataColumn[]
			{
				this.tableSolutions.SOLUTION_UIDColumn
			}, new DataColumn[]
			{
				this.tableSolutionResHired.SOLUTION_UIDColumn
			});
			this.tableSolutionResHired.Constraints.Add(foreignKeyConstraint);
			foreignKeyConstraint.AcceptRejectRule = AcceptRejectRule.None;
			foreignKeyConstraint.DeleteRule = Rule.None;
			foreignKeyConstraint.UpdateRule = Rule.None;
			foreignKeyConstraint = new ForeignKeyConstraint("FK_Solutions_SolutionProjects", new DataColumn[]
			{
				this.tableSolutions.SOLUTION_UIDColumn
			}, new DataColumn[]
			{
				this.tableSolutionProjects.SOLUTION_UIDColumn
			});
			this.tableSolutionProjects.Constraints.Add(foreignKeyConstraint);
			foreignKeyConstraint.AcceptRejectRule = AcceptRejectRule.None;
			foreignKeyConstraint.DeleteRule = Rule.None;
			foreignKeyConstraint.UpdateRule = Rule.None;
			foreignKeyConstraint = new ForeignKeyConstraint("FK_Solutions_SolutionProjectRequirementsByRole", new DataColumn[]
			{
				this.tableSolutions.SOLUTION_UIDColumn
			}, new DataColumn[]
			{
				this.tableSolutionProjectRequirementsByRole.SOLUTION_UIDColumn
			});
			this.tableSolutionProjectRequirementsByRole.Constraints.Add(foreignKeyConstraint);
			foreignKeyConstraint.AcceptRejectRule = AcceptRejectRule.None;
			foreignKeyConstraint.DeleteRule = Rule.None;
			foreignKeyConstraint.UpdateRule = Rule.None;
			this.relationFK_Solutions_SolutionResHired = new DataRelation("FK_Solutions_SolutionResHired", new DataColumn[]
			{
				this.tableSolutions.SOLUTION_UIDColumn
			}, new DataColumn[]
			{
				this.tableSolutionResHired.SOLUTION_UIDColumn
			}, false);
			this.Relations.Add(this.relationFK_Solutions_SolutionResHired);
			this.relationFK_Solutions_SolutionProjects = new DataRelation("FK_Solutions_SolutionProjects", new DataColumn[]
			{
				this.tableSolutions.SOLUTION_UIDColumn
			}, new DataColumn[]
			{
				this.tableSolutionProjects.SOLUTION_UIDColumn
			}, false);
			this.Relations.Add(this.relationFK_Solutions_SolutionProjects);
			this.relationFK_Solutions_SolutionProjectRequirementsByRole = new DataRelation("FK_Solutions_SolutionProjectRequirementsByRole", new DataColumn[]
			{
				this.tableSolutions.SOLUTION_UIDColumn
			}, new DataColumn[]
			{
				this.tableSolutionProjectRequirementsByRole.SOLUTION_UIDColumn
			}, false);
			this.Relations.Add(this.relationFK_Solutions_SolutionProjectRequirementsByRole);
			this.relationFK_Solutions_SolutionEfficientFrontier = new DataRelation("FK_Solutions_SolutionEfficientFrontier", new DataColumn[]
			{
				this.tableSolutions.FRONTIER_UIDColumn
			}, new DataColumn[]
			{
				this.tableSolutionEfficientFrontier.FRONTIER_UIDColumn
			}, false);
			this.Relations.Add(this.relationFK_Solutions_SolutionEfficientFrontier);
		}

		// Token: 0x06003028 RID: 12328 RVA: 0x0009A9B8 File Offset: 0x00098BB8
		[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
		[DebuggerNonUserCode]
		private bool ShouldSerializeSolutions()
		{
			return false;
		}

		// Token: 0x06003029 RID: 12329 RVA: 0x0009A9BB File Offset: 0x00098BBB
		[DebuggerNonUserCode]
		[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
		private bool ShouldSerializeSolutionResHired()
		{
			return false;
		}

		// Token: 0x0600302A RID: 12330 RVA: 0x0009A9BE File Offset: 0x00098BBE
		[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
		[DebuggerNonUserCode]
		private bool ShouldSerializeSolutionProjects()
		{
			return false;
		}

		// Token: 0x0600302B RID: 12331 RVA: 0x0009A9C1 File Offset: 0x00098BC1
		[DebuggerNonUserCode]
		[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
		private bool ShouldSerializeSolutionProjectRequirementsByRole()
		{
			return false;
		}

		// Token: 0x0600302C RID: 12332 RVA: 0x0009A9C4 File Offset: 0x00098BC4
		[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
		[DebuggerNonUserCode]
		private bool ShouldSerializeSolutionEfficientFrontier()
		{
			return false;
		}

		// Token: 0x0600302D RID: 12333 RVA: 0x0009A9C7 File Offset: 0x00098BC7
		[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
		[DebuggerNonUserCode]
		private void SchemaChanged(object sender, CollectionChangeEventArgs e)
		{
			if (e.Action == CollectionChangeAction.Remove)
			{
				this.InitVars();
			}
		}

		// Token: 0x0600302E RID: 12334 RVA: 0x0009A9D8 File Offset: 0x00098BD8
		[DebuggerNonUserCode]
		[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
		public static XmlSchemaComplexType GetTypedDataSetSchema(XmlSchemaSet xs)
		{
			PlannerSolutionDataSet plannerSolutionDataSet = new PlannerSolutionDataSet();
			XmlSchemaComplexType xmlSchemaComplexType = new XmlSchemaComplexType();
			XmlSchemaSequence xmlSchemaSequence = new XmlSchemaSequence();
			XmlSchemaAny xmlSchemaAny = new XmlSchemaAny();
			xmlSchemaAny.Namespace = plannerSolutionDataSet.Namespace;
			xmlSchemaSequence.Items.Add(xmlSchemaAny);
			xmlSchemaComplexType.Particle = xmlSchemaSequence;
			XmlSchema schemaSerializable = plannerSolutionDataSet.GetSchemaSerializable();
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

		// Token: 0x04000A16 RID: 2582
		private PlannerSolutionDataSet.SolutionsDataTable tableSolutions;

		// Token: 0x04000A17 RID: 2583
		private PlannerSolutionDataSet.SolutionResHiredDataTable tableSolutionResHired;

		// Token: 0x04000A18 RID: 2584
		private PlannerSolutionDataSet.SolutionProjectsDataTable tableSolutionProjects;

		// Token: 0x04000A19 RID: 2585
		private PlannerSolutionDataSet.SolutionProjectRequirementsByRoleDataTable tableSolutionProjectRequirementsByRole;

		// Token: 0x04000A1A RID: 2586
		private PlannerSolutionDataSet.SolutionEfficientFrontierDataTable tableSolutionEfficientFrontier;

		// Token: 0x04000A1B RID: 2587
		private DataRelation relationFK_Solutions_SolutionResHired;

		// Token: 0x04000A1C RID: 2588
		private DataRelation relationFK_Solutions_SolutionProjects;

		// Token: 0x04000A1D RID: 2589
		private DataRelation relationFK_Solutions_SolutionProjectRequirementsByRole;

		// Token: 0x04000A1E RID: 2590
		private DataRelation relationFK_Solutions_SolutionEfficientFrontier;

		// Token: 0x04000A1F RID: 2591
		private SchemaSerializationMode _schemaSerializationMode = SchemaSerializationMode.IncludeSchema;

		// Token: 0x02000260 RID: 608
		// (Invoke) Token: 0x06003030 RID: 12336
		[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
		public delegate void SolutionsRowChangeEventHandler(object sender, PlannerSolutionDataSet.SolutionsRowChangeEvent e);

		// Token: 0x02000261 RID: 609
		// (Invoke) Token: 0x06003034 RID: 12340
		[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
		public delegate void SolutionResHiredRowChangeEventHandler(object sender, PlannerSolutionDataSet.SolutionResHiredRowChangeEvent e);

		// Token: 0x02000262 RID: 610
		// (Invoke) Token: 0x06003038 RID: 12344
		[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
		public delegate void SolutionProjectsRowChangeEventHandler(object sender, PlannerSolutionDataSet.SolutionProjectsRowChangeEvent e);

		// Token: 0x02000263 RID: 611
		// (Invoke) Token: 0x0600303C RID: 12348
		[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
		public delegate void SolutionProjectRequirementsByRoleRowChangeEventHandler(object sender, PlannerSolutionDataSet.SolutionProjectRequirementsByRoleRowChangeEvent e);

		// Token: 0x02000264 RID: 612
		// (Invoke) Token: 0x06003040 RID: 12352
		[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
		public delegate void SolutionEfficientFrontierRowChangeEventHandler(object sender, PlannerSolutionDataSet.SolutionEfficientFrontierRowChangeEvent e);

		// Token: 0x02000265 RID: 613
		[XmlSchemaProvider("GetTypedTableSchema")]
		[Serializable]
		public class SolutionsDataTable : DataTable, IEnumerable
		{
			// Token: 0x06003043 RID: 12355 RVA: 0x0009AB20 File Offset: 0x00098D20
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public SolutionsDataTable()
			{
				base.TableName = "Solutions";
				this.BeginInit();
				this.InitClass();
				this.EndInit();
			}

			// Token: 0x06003044 RID: 12356 RVA: 0x0009AB48 File Offset: 0x00098D48
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			internal SolutionsDataTable(DataTable table)
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

			// Token: 0x06003045 RID: 12357 RVA: 0x0009ABF0 File Offset: 0x00098DF0
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			protected SolutionsDataTable(SerializationInfo info, StreamingContext context) : base(info, context)
			{
				this.InitVars();
			}

			// Token: 0x17000E00 RID: 3584
			// (get) Token: 0x06003046 RID: 12358 RVA: 0x0009AC00 File Offset: 0x00098E00
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public DataColumn SOLUTION_UIDColumn
			{
				get
				{
					return this.columnSOLUTION_UID;
				}
			}

			// Token: 0x17000E01 RID: 3585
			// (get) Token: 0x06003047 RID: 12359 RVA: 0x0009AC08 File Offset: 0x00098E08
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			public DataColumn OPTIMIZER_SOLUTION_UIDColumn
			{
				get
				{
					return this.columnOPTIMIZER_SOLUTION_UID;
				}
			}

			// Token: 0x17000E02 RID: 3586
			// (get) Token: 0x06003048 RID: 12360 RVA: 0x0009AC10 File Offset: 0x00098E10
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			public DataColumn ANALYSIS_UIDColumn
			{
				get
				{
					return this.columnANALYSIS_UID;
				}
			}

			// Token: 0x17000E03 RID: 3587
			// (get) Token: 0x06003049 RID: 12361 RVA: 0x0009AC18 File Offset: 0x00098E18
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			public DataColumn SOLUTION_NAMEColumn
			{
				get
				{
					return this.columnSOLUTION_NAME;
				}
			}

			// Token: 0x17000E04 RID: 3588
			// (get) Token: 0x0600304A RID: 12362 RVA: 0x0009AC20 File Offset: 0x00098E20
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			public DataColumn SOLUTION_DESCRIPTIONColumn
			{
				get
				{
					return this.columnSOLUTION_DESCRIPTION;
				}
			}

			// Token: 0x17000E05 RID: 3589
			// (get) Token: 0x0600304B RID: 12363 RVA: 0x0009AC28 File Offset: 0x00098E28
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			public DataColumn CONSTRAINT_TYPEColumn
			{
				get
				{
					return this.columnCONSTRAINT_TYPE;
				}
			}

			// Token: 0x17000E06 RID: 3590
			// (get) Token: 0x0600304C RID: 12364 RVA: 0x0009AC30 File Offset: 0x00098E30
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public DataColumn CONSTRAINT_VALUEColumn
			{
				get
				{
					return this.columnCONSTRAINT_VALUE;
				}
			}

			// Token: 0x17000E07 RID: 3591
			// (get) Token: 0x0600304D RID: 12365 RVA: 0x0009AC38 File Offset: 0x00098E38
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			public DataColumn FRONTIER_UIDColumn
			{
				get
				{
					return this.columnFRONTIER_UID;
				}
			}

			// Token: 0x17000E08 RID: 3592
			// (get) Token: 0x0600304E RID: 12366 RVA: 0x0009AC40 File Offset: 0x00098E40
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			public DataColumn MOD_DATEColumn
			{
				get
				{
					return this.columnMOD_DATE;
				}
			}

			// Token: 0x17000E09 RID: 3593
			// (get) Token: 0x0600304F RID: 12367 RVA: 0x0009AC48 File Offset: 0x00098E48
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			public DataColumn CREATED_DATEColumn
			{
				get
				{
					return this.columnCREATED_DATE;
				}
			}

			// Token: 0x17000E0A RID: 3594
			// (get) Token: 0x06003050 RID: 12368 RVA: 0x0009AC50 File Offset: 0x00098E50
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			public DataColumn CREATED_BY_RES_UIDColumn
			{
				get
				{
					return this.columnCREATED_BY_RES_UID;
				}
			}

			// Token: 0x17000E0B RID: 3595
			// (get) Token: 0x06003051 RID: 12369 RVA: 0x0009AC58 File Offset: 0x00098E58
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			public DataColumn LAST_UPDATED_BY_RES_UIDColumn
			{
				get
				{
					return this.columnLAST_UPDATED_BY_RES_UID;
				}
			}

			// Token: 0x17000E0C RID: 3596
			// (get) Token: 0x06003052 RID: 12370 RVA: 0x0009AC60 File Offset: 0x00098E60
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			public DataColumn CREATED_BY_RES_NAMEColumn
			{
				get
				{
					return this.columnCREATED_BY_RES_NAME;
				}
			}

			// Token: 0x17000E0D RID: 3597
			// (get) Token: 0x06003053 RID: 12371 RVA: 0x0009AC68 File Offset: 0x00098E68
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			public DataColumn LAST_UPDATED_BY_RES_NAMEColumn
			{
				get
				{
					return this.columnLAST_UPDATED_BY_RES_NAME;
				}
			}

			// Token: 0x17000E0E RID: 3598
			// (get) Token: 0x06003054 RID: 12372 RVA: 0x0009AC70 File Offset: 0x00098E70
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			public DataColumn HIRING_TYPEColumn
			{
				get
				{
					return this.columnHIRING_TYPE;
				}
			}

			// Token: 0x17000E0F RID: 3599
			// (get) Token: 0x06003055 RID: 12373 RVA: 0x0009AC78 File Offset: 0x00098E78
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public DataColumn OPT_ENF_SCHEDULING_CONSColumn
			{
				get
				{
					return this.columnOPT_ENF_SCHEDULING_CONS;
				}
			}

			// Token: 0x17000E10 RID: 3600
			// (get) Token: 0x06003056 RID: 12374 RVA: 0x0009AC80 File Offset: 0x00098E80
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			public DataColumn OPT_ENF_PROJ_DEPColumn
			{
				get
				{
					return this.columnOPT_ENF_PROJ_DEP;
				}
			}

			// Token: 0x17000E11 RID: 3601
			// (get) Token: 0x06003057 RID: 12375 RVA: 0x0009AC88 File Offset: 0x00098E88
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public DataColumn RATE_TABLEColumn
			{
				get
				{
					return this.columnRATE_TABLE;
				}
			}

			// Token: 0x17000E12 RID: 3602
			// (get) Token: 0x06003058 RID: 12376 RVA: 0x0009AC90 File Offset: 0x00098E90
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			public DataColumn ALLOCATION_THRESHOLDColumn
			{
				get
				{
					return this.columnALLOCATION_THRESHOLD;
				}
			}

			// Token: 0x17000E13 RID: 3603
			// (get) Token: 0x06003059 RID: 12377 RVA: 0x0009AC98 File Offset: 0x00098E98
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			public DataColumn ANALYSIS_NAMEColumn
			{
				get
				{
					return this.columnANALYSIS_NAME;
				}
			}

			// Token: 0x17000E14 RID: 3604
			// (get) Token: 0x0600305A RID: 12378 RVA: 0x0009ACA0 File Offset: 0x00098EA0
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public DataColumn OPTIMIZER_SOLUTION_NAMEColumn
			{
				get
				{
					return this.columnOPTIMIZER_SOLUTION_NAME;
				}
			}

			// Token: 0x17000E15 RID: 3605
			// (get) Token: 0x0600305B RID: 12379 RVA: 0x0009ACA8 File Offset: 0x00098EA8
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			public DataColumn TOTAL_PRIORITY_VALUEColumn
			{
				get
				{
					return this.columnTOTAL_PRIORITY_VALUE;
				}
			}

			// Token: 0x17000E16 RID: 3606
			// (get) Token: 0x0600305C RID: 12380 RVA: 0x0009ACB0 File Offset: 0x00098EB0
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

			// Token: 0x17000E17 RID: 3607
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public PlannerSolutionDataSet.SolutionsRow this[int index]
			{
				get
				{
					return (PlannerSolutionDataSet.SolutionsRow)base.Rows[index];
				}
			}

			// Token: 0x14000209 RID: 521
			// (add) Token: 0x0600305E RID: 12382 RVA: 0x0009ACD0 File Offset: 0x00098ED0
			// (remove) Token: 0x0600305F RID: 12383 RVA: 0x0009AD08 File Offset: 0x00098F08
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public event PlannerSolutionDataSet.SolutionsRowChangeEventHandler SolutionsRowChanging;

			// Token: 0x1400020A RID: 522
			// (add) Token: 0x06003060 RID: 12384 RVA: 0x0009AD40 File Offset: 0x00098F40
			// (remove) Token: 0x06003061 RID: 12385 RVA: 0x0009AD78 File Offset: 0x00098F78
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public event PlannerSolutionDataSet.SolutionsRowChangeEventHandler SolutionsRowChanged;

			// Token: 0x1400020B RID: 523
			// (add) Token: 0x06003062 RID: 12386 RVA: 0x0009ADB0 File Offset: 0x00098FB0
			// (remove) Token: 0x06003063 RID: 12387 RVA: 0x0009ADE8 File Offset: 0x00098FE8
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public event PlannerSolutionDataSet.SolutionsRowChangeEventHandler SolutionsRowDeleting;

			// Token: 0x1400020C RID: 524
			// (add) Token: 0x06003064 RID: 12388 RVA: 0x0009AE20 File Offset: 0x00099020
			// (remove) Token: 0x06003065 RID: 12389 RVA: 0x0009AE58 File Offset: 0x00099058
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public event PlannerSolutionDataSet.SolutionsRowChangeEventHandler SolutionsRowDeleted;

			// Token: 0x06003066 RID: 12390 RVA: 0x0009AE8D File Offset: 0x0009908D
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			public void AddSolutionsRow(PlannerSolutionDataSet.SolutionsRow row)
			{
				base.Rows.Add(row);
			}

			// Token: 0x06003067 RID: 12391 RVA: 0x0009AE9C File Offset: 0x0009909C
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			public PlannerSolutionDataSet.SolutionsRow AddSolutionsRow(Guid SOLUTION_UID, Guid OPTIMIZER_SOLUTION_UID, Guid ANALYSIS_UID, string SOLUTION_NAME, string SOLUTION_DESCRIPTION, byte CONSTRAINT_TYPE, decimal CONSTRAINT_VALUE, Guid FRONTIER_UID, DateTime MOD_DATE, DateTime CREATED_DATE, Guid CREATED_BY_RES_UID, Guid LAST_UPDATED_BY_RES_UID, string CREATED_BY_RES_NAME, string LAST_UPDATED_BY_RES_NAME, byte HIRING_TYPE, bool OPT_ENF_SCHEDULING_CONS, bool OPT_ENF_PROJ_DEP, byte RATE_TABLE, double ALLOCATION_THRESHOLD, string ANALYSIS_NAME, string OPTIMIZER_SOLUTION_NAME, decimal TOTAL_PRIORITY_VALUE)
			{
				PlannerSolutionDataSet.SolutionsRow solutionsRow = (PlannerSolutionDataSet.SolutionsRow)base.NewRow();
				object[] itemArray = new object[]
				{
					SOLUTION_UID,
					OPTIMIZER_SOLUTION_UID,
					ANALYSIS_UID,
					SOLUTION_NAME,
					SOLUTION_DESCRIPTION,
					CONSTRAINT_TYPE,
					CONSTRAINT_VALUE,
					FRONTIER_UID,
					MOD_DATE,
					CREATED_DATE,
					CREATED_BY_RES_UID,
					LAST_UPDATED_BY_RES_UID,
					CREATED_BY_RES_NAME,
					LAST_UPDATED_BY_RES_NAME,
					HIRING_TYPE,
					OPT_ENF_SCHEDULING_CONS,
					OPT_ENF_PROJ_DEP,
					RATE_TABLE,
					ALLOCATION_THRESHOLD,
					ANALYSIS_NAME,
					OPTIMIZER_SOLUTION_NAME,
					TOTAL_PRIORITY_VALUE
				};
				solutionsRow.ItemArray = itemArray;
				base.Rows.Add(solutionsRow);
				return solutionsRow;
			}

			// Token: 0x06003068 RID: 12392 RVA: 0x0009AF9C File Offset: 0x0009919C
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			public PlannerSolutionDataSet.SolutionsRow FindBySOLUTION_UID(Guid SOLUTION_UID)
			{
				return (PlannerSolutionDataSet.SolutionsRow)base.Rows.Find(new object[]
				{
					SOLUTION_UID
				});
			}

			// Token: 0x06003069 RID: 12393 RVA: 0x0009AFCA File Offset: 0x000991CA
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public virtual IEnumerator GetEnumerator()
			{
				return base.Rows.GetEnumerator();
			}

			// Token: 0x0600306A RID: 12394 RVA: 0x0009AFD8 File Offset: 0x000991D8
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public override DataTable Clone()
			{
				PlannerSolutionDataSet.SolutionsDataTable solutionsDataTable = (PlannerSolutionDataSet.SolutionsDataTable)base.Clone();
				solutionsDataTable.InitVars();
				return solutionsDataTable;
			}

			// Token: 0x0600306B RID: 12395 RVA: 0x0009AFF8 File Offset: 0x000991F8
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			protected override DataTable CreateInstance()
			{
				return new PlannerSolutionDataSet.SolutionsDataTable();
			}

			// Token: 0x0600306C RID: 12396 RVA: 0x0009B000 File Offset: 0x00099200
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			internal void InitVars()
			{
				this.columnSOLUTION_UID = base.Columns["SOLUTION_UID"];
				this.columnOPTIMIZER_SOLUTION_UID = base.Columns["OPTIMIZER_SOLUTION_UID"];
				this.columnANALYSIS_UID = base.Columns["ANALYSIS_UID"];
				this.columnSOLUTION_NAME = base.Columns["SOLUTION_NAME"];
				this.columnSOLUTION_DESCRIPTION = base.Columns["SOLUTION_DESCRIPTION"];
				this.columnCONSTRAINT_TYPE = base.Columns["CONSTRAINT_TYPE"];
				this.columnCONSTRAINT_VALUE = base.Columns["CONSTRAINT_VALUE"];
				this.columnFRONTIER_UID = base.Columns["FRONTIER_UID"];
				this.columnMOD_DATE = base.Columns["MOD_DATE"];
				this.columnCREATED_DATE = base.Columns["CREATED_DATE"];
				this.columnCREATED_BY_RES_UID = base.Columns["CREATED_BY_RES_UID"];
				this.columnLAST_UPDATED_BY_RES_UID = base.Columns["LAST_UPDATED_BY_RES_UID"];
				this.columnCREATED_BY_RES_NAME = base.Columns["CREATED_BY_RES_NAME"];
				this.columnLAST_UPDATED_BY_RES_NAME = base.Columns["LAST_UPDATED_BY_RES_NAME"];
				this.columnHIRING_TYPE = base.Columns["HIRING_TYPE"];
				this.columnOPT_ENF_SCHEDULING_CONS = base.Columns["OPT_ENF_SCHEDULING_CONS"];
				this.columnOPT_ENF_PROJ_DEP = base.Columns["OPT_ENF_PROJ_DEP"];
				this.columnRATE_TABLE = base.Columns["RATE_TABLE"];
				this.columnALLOCATION_THRESHOLD = base.Columns["ALLOCATION_THRESHOLD"];
				this.columnANALYSIS_NAME = base.Columns["ANALYSIS_NAME"];
				this.columnOPTIMIZER_SOLUTION_NAME = base.Columns["OPTIMIZER_SOLUTION_NAME"];
				this.columnTOTAL_PRIORITY_VALUE = base.Columns["TOTAL_PRIORITY_VALUE"];
			}

			// Token: 0x0600306D RID: 12397 RVA: 0x0009B1F4 File Offset: 0x000993F4
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			private void InitClass()
			{
				this.columnSOLUTION_UID = new DataColumn("SOLUTION_UID", typeof(Guid), null, MappingType.Element);
				base.Columns.Add(this.columnSOLUTION_UID);
				this.columnOPTIMIZER_SOLUTION_UID = new DataColumn("OPTIMIZER_SOLUTION_UID", typeof(Guid), null, MappingType.Element);
				base.Columns.Add(this.columnOPTIMIZER_SOLUTION_UID);
				this.columnANALYSIS_UID = new DataColumn("ANALYSIS_UID", typeof(Guid), null, MappingType.Element);
				base.Columns.Add(this.columnANALYSIS_UID);
				this.columnSOLUTION_NAME = new DataColumn("SOLUTION_NAME", typeof(string), null, MappingType.Element);
				base.Columns.Add(this.columnSOLUTION_NAME);
				this.columnSOLUTION_DESCRIPTION = new DataColumn("SOLUTION_DESCRIPTION", typeof(string), null, MappingType.Element);
				base.Columns.Add(this.columnSOLUTION_DESCRIPTION);
				this.columnCONSTRAINT_TYPE = new DataColumn("CONSTRAINT_TYPE", typeof(byte), null, MappingType.Element);
				base.Columns.Add(this.columnCONSTRAINT_TYPE);
				this.columnCONSTRAINT_VALUE = new DataColumn("CONSTRAINT_VALUE", typeof(decimal), null, MappingType.Element);
				base.Columns.Add(this.columnCONSTRAINT_VALUE);
				this.columnFRONTIER_UID = new DataColumn("FRONTIER_UID", typeof(Guid), null, MappingType.Element);
				base.Columns.Add(this.columnFRONTIER_UID);
				this.columnMOD_DATE = new DataColumn("MOD_DATE", typeof(DateTime), null, MappingType.Element);
				base.Columns.Add(this.columnMOD_DATE);
				this.columnCREATED_DATE = new DataColumn("CREATED_DATE", typeof(DateTime), null, MappingType.Element);
				base.Columns.Add(this.columnCREATED_DATE);
				this.columnCREATED_BY_RES_UID = new DataColumn("CREATED_BY_RES_UID", typeof(Guid), null, MappingType.Element);
				base.Columns.Add(this.columnCREATED_BY_RES_UID);
				this.columnLAST_UPDATED_BY_RES_UID = new DataColumn("LAST_UPDATED_BY_RES_UID", typeof(Guid), null, MappingType.Element);
				base.Columns.Add(this.columnLAST_UPDATED_BY_RES_UID);
				this.columnCREATED_BY_RES_NAME = new DataColumn("CREATED_BY_RES_NAME", typeof(string), null, MappingType.Element);
				base.Columns.Add(this.columnCREATED_BY_RES_NAME);
				this.columnLAST_UPDATED_BY_RES_NAME = new DataColumn("LAST_UPDATED_BY_RES_NAME", typeof(string), null, MappingType.Element);
				base.Columns.Add(this.columnLAST_UPDATED_BY_RES_NAME);
				this.columnHIRING_TYPE = new DataColumn("HIRING_TYPE", typeof(byte), null, MappingType.Element);
				base.Columns.Add(this.columnHIRING_TYPE);
				this.columnOPT_ENF_SCHEDULING_CONS = new DataColumn("OPT_ENF_SCHEDULING_CONS", typeof(bool), null, MappingType.Element);
				base.Columns.Add(this.columnOPT_ENF_SCHEDULING_CONS);
				this.columnOPT_ENF_PROJ_DEP = new DataColumn("OPT_ENF_PROJ_DEP", typeof(bool), null, MappingType.Element);
				base.Columns.Add(this.columnOPT_ENF_PROJ_DEP);
				this.columnRATE_TABLE = new DataColumn("RATE_TABLE", typeof(byte), null, MappingType.Element);
				base.Columns.Add(this.columnRATE_TABLE);
				this.columnALLOCATION_THRESHOLD = new DataColumn("ALLOCATION_THRESHOLD", typeof(double), null, MappingType.Element);
				base.Columns.Add(this.columnALLOCATION_THRESHOLD);
				this.columnANALYSIS_NAME = new DataColumn("ANALYSIS_NAME", typeof(string), null, MappingType.Element);
				base.Columns.Add(this.columnANALYSIS_NAME);
				this.columnOPTIMIZER_SOLUTION_NAME = new DataColumn("OPTIMIZER_SOLUTION_NAME", typeof(string), null, MappingType.Element);
				base.Columns.Add(this.columnOPTIMIZER_SOLUTION_NAME);
				this.columnTOTAL_PRIORITY_VALUE = new DataColumn("TOTAL_PRIORITY_VALUE", typeof(decimal), null, MappingType.Element);
				base.Columns.Add(this.columnTOTAL_PRIORITY_VALUE);
				base.Constraints.Add(new UniqueConstraint("PK_Solutions", new DataColumn[]
				{
					this.columnSOLUTION_UID
				}, true));
				this.columnSOLUTION_UID.AllowDBNull = false;
				this.columnSOLUTION_UID.Unique = true;
				this.columnOPTIMIZER_SOLUTION_UID.AllowDBNull = false;
				this.columnANALYSIS_UID.AllowDBNull = false;
				this.columnSOLUTION_NAME.AllowDBNull = false;
				this.columnCONSTRAINT_TYPE.AllowDBNull = false;
				this.columnCONSTRAINT_VALUE.AllowDBNull = false;
				this.columnCREATED_BY_RES_NAME.ReadOnly = true;
				this.columnLAST_UPDATED_BY_RES_NAME.ReadOnly = true;
				this.columnHIRING_TYPE.AllowDBNull = false;
				this.columnOPT_ENF_SCHEDULING_CONS.AllowDBNull = false;
				this.columnOPT_ENF_PROJ_DEP.AllowDBNull = false;
				this.columnRATE_TABLE.AllowDBNull = false;
				this.columnALLOCATION_THRESHOLD.AllowDBNull = false;
				this.columnALLOCATION_THRESHOLD.DefaultValue = 1.0;
				this.columnANALYSIS_NAME.ReadOnly = true;
				this.columnOPTIMIZER_SOLUTION_NAME.ReadOnly = true;
				this.columnTOTAL_PRIORITY_VALUE.ReadOnly = true;
			}

			// Token: 0x0600306E RID: 12398 RVA: 0x0009B6EB File Offset: 0x000998EB
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			public PlannerSolutionDataSet.SolutionsRow NewSolutionsRow()
			{
				return (PlannerSolutionDataSet.SolutionsRow)base.NewRow();
			}

			// Token: 0x0600306F RID: 12399 RVA: 0x0009B6F8 File Offset: 0x000998F8
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			protected override DataRow NewRowFromBuilder(DataRowBuilder builder)
			{
				return new PlannerSolutionDataSet.SolutionsRow(builder);
			}

			// Token: 0x06003070 RID: 12400 RVA: 0x0009B700 File Offset: 0x00099900
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			protected override Type GetRowType()
			{
				return typeof(PlannerSolutionDataSet.SolutionsRow);
			}

			// Token: 0x06003071 RID: 12401 RVA: 0x0009B70C File Offset: 0x0009990C
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			protected override void OnRowChanged(DataRowChangeEventArgs e)
			{
				base.OnRowChanged(e);
				if (this.SolutionsRowChanged != null)
				{
					this.SolutionsRowChanged(this, new PlannerSolutionDataSet.SolutionsRowChangeEvent((PlannerSolutionDataSet.SolutionsRow)e.Row, e.Action));
				}
			}

			// Token: 0x06003072 RID: 12402 RVA: 0x0009B73F File Offset: 0x0009993F
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			protected override void OnRowChanging(DataRowChangeEventArgs e)
			{
				base.OnRowChanging(e);
				if (this.SolutionsRowChanging != null)
				{
					this.SolutionsRowChanging(this, new PlannerSolutionDataSet.SolutionsRowChangeEvent((PlannerSolutionDataSet.SolutionsRow)e.Row, e.Action));
				}
			}

			// Token: 0x06003073 RID: 12403 RVA: 0x0009B772 File Offset: 0x00099972
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			protected override void OnRowDeleted(DataRowChangeEventArgs e)
			{
				base.OnRowDeleted(e);
				if (this.SolutionsRowDeleted != null)
				{
					this.SolutionsRowDeleted(this, new PlannerSolutionDataSet.SolutionsRowChangeEvent((PlannerSolutionDataSet.SolutionsRow)e.Row, e.Action));
				}
			}

			// Token: 0x06003074 RID: 12404 RVA: 0x0009B7A5 File Offset: 0x000999A5
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			protected override void OnRowDeleting(DataRowChangeEventArgs e)
			{
				base.OnRowDeleting(e);
				if (this.SolutionsRowDeleting != null)
				{
					this.SolutionsRowDeleting(this, new PlannerSolutionDataSet.SolutionsRowChangeEvent((PlannerSolutionDataSet.SolutionsRow)e.Row, e.Action));
				}
			}

			// Token: 0x06003075 RID: 12405 RVA: 0x0009B7D8 File Offset: 0x000999D8
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			public void RemoveSolutionsRow(PlannerSolutionDataSet.SolutionsRow row)
			{
				base.Rows.Remove(row);
			}

			// Token: 0x06003076 RID: 12406 RVA: 0x0009B7E8 File Offset: 0x000999E8
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			public static XmlSchemaComplexType GetTypedTableSchema(XmlSchemaSet xs)
			{
				XmlSchemaComplexType xmlSchemaComplexType = new XmlSchemaComplexType();
				XmlSchemaSequence xmlSchemaSequence = new XmlSchemaSequence();
				PlannerSolutionDataSet plannerSolutionDataSet = new PlannerSolutionDataSet();
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
				xmlSchemaAttribute.FixedValue = plannerSolutionDataSet.Namespace;
				xmlSchemaComplexType.Attributes.Add(xmlSchemaAttribute);
				XmlSchemaAttribute xmlSchemaAttribute2 = new XmlSchemaAttribute();
				xmlSchemaAttribute2.Name = "tableTypeName";
				xmlSchemaAttribute2.FixedValue = "SolutionsDataTable";
				xmlSchemaComplexType.Attributes.Add(xmlSchemaAttribute2);
				xmlSchemaComplexType.Particle = xmlSchemaSequence;
				XmlSchema schemaSerializable = plannerSolutionDataSet.GetSchemaSerializable();
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

			// Token: 0x04000A20 RID: 2592
			private DataColumn columnSOLUTION_UID;

			// Token: 0x04000A21 RID: 2593
			private DataColumn columnOPTIMIZER_SOLUTION_UID;

			// Token: 0x04000A22 RID: 2594
			private DataColumn columnANALYSIS_UID;

			// Token: 0x04000A23 RID: 2595
			private DataColumn columnSOLUTION_NAME;

			// Token: 0x04000A24 RID: 2596
			private DataColumn columnSOLUTION_DESCRIPTION;

			// Token: 0x04000A25 RID: 2597
			private DataColumn columnCONSTRAINT_TYPE;

			// Token: 0x04000A26 RID: 2598
			private DataColumn columnCONSTRAINT_VALUE;

			// Token: 0x04000A27 RID: 2599
			private DataColumn columnFRONTIER_UID;

			// Token: 0x04000A28 RID: 2600
			private DataColumn columnMOD_DATE;

			// Token: 0x04000A29 RID: 2601
			private DataColumn columnCREATED_DATE;

			// Token: 0x04000A2A RID: 2602
			private DataColumn columnCREATED_BY_RES_UID;

			// Token: 0x04000A2B RID: 2603
			private DataColumn columnLAST_UPDATED_BY_RES_UID;

			// Token: 0x04000A2C RID: 2604
			private DataColumn columnCREATED_BY_RES_NAME;

			// Token: 0x04000A2D RID: 2605
			private DataColumn columnLAST_UPDATED_BY_RES_NAME;

			// Token: 0x04000A2E RID: 2606
			private DataColumn columnHIRING_TYPE;

			// Token: 0x04000A2F RID: 2607
			private DataColumn columnOPT_ENF_SCHEDULING_CONS;

			// Token: 0x04000A30 RID: 2608
			private DataColumn columnOPT_ENF_PROJ_DEP;

			// Token: 0x04000A31 RID: 2609
			private DataColumn columnRATE_TABLE;

			// Token: 0x04000A32 RID: 2610
			private DataColumn columnALLOCATION_THRESHOLD;

			// Token: 0x04000A33 RID: 2611
			private DataColumn columnANALYSIS_NAME;

			// Token: 0x04000A34 RID: 2612
			private DataColumn columnOPTIMIZER_SOLUTION_NAME;

			// Token: 0x04000A35 RID: 2613
			private DataColumn columnTOTAL_PRIORITY_VALUE;
		}

		// Token: 0x02000266 RID: 614
		[XmlSchemaProvider("GetTypedTableSchema")]
		[Serializable]
		public class SolutionResHiredDataTable : DataTable, IEnumerable
		{
			// Token: 0x06003077 RID: 12407 RVA: 0x0009B9E0 File Offset: 0x00099BE0
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public SolutionResHiredDataTable()
			{
				base.TableName = "SolutionResHired";
				this.BeginInit();
				this.InitClass();
				this.EndInit();
			}

			// Token: 0x06003078 RID: 12408 RVA: 0x0009BA08 File Offset: 0x00099C08
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			internal SolutionResHiredDataTable(DataTable table)
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

			// Token: 0x06003079 RID: 12409 RVA: 0x0009BAB0 File Offset: 0x00099CB0
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			protected SolutionResHiredDataTable(SerializationInfo info, StreamingContext context) : base(info, context)
			{
				this.InitVars();
			}

			// Token: 0x17000E18 RID: 3608
			// (get) Token: 0x0600307A RID: 12410 RVA: 0x0009BAC0 File Offset: 0x00099CC0
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public DataColumn SOLUTION_UIDColumn
			{
				get
				{
					return this.columnSOLUTION_UID;
				}
			}

			// Token: 0x17000E19 RID: 3609
			// (get) Token: 0x0600307B RID: 12411 RVA: 0x0009BAC8 File Offset: 0x00099CC8
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			public DataColumn LT_STRUCT_UIDColumn
			{
				get
				{
					return this.columnLT_STRUCT_UID;
				}
			}

			// Token: 0x17000E1A RID: 3610
			// (get) Token: 0x0600307C RID: 12412 RVA: 0x0009BAD0 File Offset: 0x00099CD0
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public DataColumn CUSTOM_FIELD_UIDColumn
			{
				get
				{
					return this.columnCUSTOM_FIELD_UID;
				}
			}

			// Token: 0x17000E1B RID: 3611
			// (get) Token: 0x0600307D RID: 12413 RVA: 0x0009BAD8 File Offset: 0x00099CD8
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public DataColumn PROJ_UIDColumn
			{
				get
				{
					return this.columnPROJ_UID;
				}
			}

			// Token: 0x17000E1C RID: 3612
			// (get) Token: 0x0600307E RID: 12414 RVA: 0x0009BAE0 File Offset: 0x00099CE0
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			public DataColumn START_DATEColumn
			{
				get
				{
					return this.columnSTART_DATE;
				}
			}

			// Token: 0x17000E1D RID: 3613
			// (get) Token: 0x0600307F RID: 12415 RVA: 0x0009BAE8 File Offset: 0x00099CE8
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public DataColumn DURATIONColumn
			{
				get
				{
					return this.columnDURATION;
				}
			}

			// Token: 0x17000E1E RID: 3614
			// (get) Token: 0x06003080 RID: 12416 RVA: 0x0009BAF0 File Offset: 0x00099CF0
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			public DataColumn RESOURCE_COSTColumn
			{
				get
				{
					return this.columnRESOURCE_COST;
				}
			}

			// Token: 0x17000E1F RID: 3615
			// (get) Token: 0x06003081 RID: 12417 RVA: 0x0009BAF8 File Offset: 0x00099CF8
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			public DataColumn RESOURCE_WORKColumn
			{
				get
				{
					return this.columnRESOURCE_WORK;
				}
			}

			// Token: 0x17000E20 RID: 3616
			// (get) Token: 0x06003082 RID: 12418 RVA: 0x0009BB00 File Offset: 0x00099D00
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public DataColumn RES_HIRE_UIDColumn
			{
				get
				{
					return this.columnRES_HIRE_UID;
				}
			}

			// Token: 0x17000E21 RID: 3617
			// (get) Token: 0x06003083 RID: 12419 RVA: 0x0009BB08 File Offset: 0x00099D08
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public DataColumn ROLE_NAMEColumn
			{
				get
				{
					return this.columnROLE_NAME;
				}
			}

			// Token: 0x17000E22 RID: 3618
			// (get) Token: 0x06003084 RID: 12420 RVA: 0x0009BB10 File Offset: 0x00099D10
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public DataColumn PROJ_NAMEColumn
			{
				get
				{
					return this.columnPROJ_NAME;
				}
			}

			// Token: 0x17000E23 RID: 3619
			// (get) Token: 0x06003085 RID: 12421 RVA: 0x0009BB18 File Offset: 0x00099D18
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

			// Token: 0x17000E24 RID: 3620
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public PlannerSolutionDataSet.SolutionResHiredRow this[int index]
			{
				get
				{
					return (PlannerSolutionDataSet.SolutionResHiredRow)base.Rows[index];
				}
			}

			// Token: 0x1400020D RID: 525
			// (add) Token: 0x06003087 RID: 12423 RVA: 0x0009BB38 File Offset: 0x00099D38
			// (remove) Token: 0x06003088 RID: 12424 RVA: 0x0009BB70 File Offset: 0x00099D70
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public event PlannerSolutionDataSet.SolutionResHiredRowChangeEventHandler SolutionResHiredRowChanging;

			// Token: 0x1400020E RID: 526
			// (add) Token: 0x06003089 RID: 12425 RVA: 0x0009BBA8 File Offset: 0x00099DA8
			// (remove) Token: 0x0600308A RID: 12426 RVA: 0x0009BBE0 File Offset: 0x00099DE0
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public event PlannerSolutionDataSet.SolutionResHiredRowChangeEventHandler SolutionResHiredRowChanged;

			// Token: 0x1400020F RID: 527
			// (add) Token: 0x0600308B RID: 12427 RVA: 0x0009BC18 File Offset: 0x00099E18
			// (remove) Token: 0x0600308C RID: 12428 RVA: 0x0009BC50 File Offset: 0x00099E50
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public event PlannerSolutionDataSet.SolutionResHiredRowChangeEventHandler SolutionResHiredRowDeleting;

			// Token: 0x14000210 RID: 528
			// (add) Token: 0x0600308D RID: 12429 RVA: 0x0009BC88 File Offset: 0x00099E88
			// (remove) Token: 0x0600308E RID: 12430 RVA: 0x0009BCC0 File Offset: 0x00099EC0
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public event PlannerSolutionDataSet.SolutionResHiredRowChangeEventHandler SolutionResHiredRowDeleted;

			// Token: 0x0600308F RID: 12431 RVA: 0x0009BCF5 File Offset: 0x00099EF5
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			public void AddSolutionResHiredRow(PlannerSolutionDataSet.SolutionResHiredRow row)
			{
				base.Rows.Add(row);
			}

			// Token: 0x06003090 RID: 12432 RVA: 0x0009BD04 File Offset: 0x00099F04
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			public PlannerSolutionDataSet.SolutionResHiredRow AddSolutionResHiredRow(PlannerSolutionDataSet.SolutionsRow parentSolutionsRowByFK_Solutions_SolutionResHired, Guid LT_STRUCT_UID, Guid CUSTOM_FIELD_UID, Guid PROJ_UID, DateTime START_DATE, int DURATION, decimal RESOURCE_COST, decimal RESOURCE_WORK, Guid RES_HIRE_UID, string ROLE_NAME, string PROJ_NAME)
			{
				PlannerSolutionDataSet.SolutionResHiredRow solutionResHiredRow = (PlannerSolutionDataSet.SolutionResHiredRow)base.NewRow();
				object[] array = new object[]
				{
					null,
					LT_STRUCT_UID,
					CUSTOM_FIELD_UID,
					PROJ_UID,
					START_DATE,
					DURATION,
					RESOURCE_COST,
					RESOURCE_WORK,
					RES_HIRE_UID,
					ROLE_NAME,
					PROJ_NAME
				};
				if (parentSolutionsRowByFK_Solutions_SolutionResHired != null)
				{
					array[0] = parentSolutionsRowByFK_Solutions_SolutionResHired[0];
				}
				solutionResHiredRow.ItemArray = array;
				base.Rows.Add(solutionResHiredRow);
				return solutionResHiredRow;
			}

			// Token: 0x06003091 RID: 12433 RVA: 0x0009BDA4 File Offset: 0x00099FA4
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			public PlannerSolutionDataSet.SolutionResHiredRow FindByRES_HIRE_UID(Guid RES_HIRE_UID)
			{
				return (PlannerSolutionDataSet.SolutionResHiredRow)base.Rows.Find(new object[]
				{
					RES_HIRE_UID
				});
			}

			// Token: 0x06003092 RID: 12434 RVA: 0x0009BDD2 File Offset: 0x00099FD2
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			public virtual IEnumerator GetEnumerator()
			{
				return base.Rows.GetEnumerator();
			}

			// Token: 0x06003093 RID: 12435 RVA: 0x0009BDE0 File Offset: 0x00099FE0
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public override DataTable Clone()
			{
				PlannerSolutionDataSet.SolutionResHiredDataTable solutionResHiredDataTable = (PlannerSolutionDataSet.SolutionResHiredDataTable)base.Clone();
				solutionResHiredDataTable.InitVars();
				return solutionResHiredDataTable;
			}

			// Token: 0x06003094 RID: 12436 RVA: 0x0009BE00 File Offset: 0x0009A000
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			protected override DataTable CreateInstance()
			{
				return new PlannerSolutionDataSet.SolutionResHiredDataTable();
			}

			// Token: 0x06003095 RID: 12437 RVA: 0x0009BE08 File Offset: 0x0009A008
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			internal void InitVars()
			{
				this.columnSOLUTION_UID = base.Columns["SOLUTION_UID"];
				this.columnLT_STRUCT_UID = base.Columns["LT_STRUCT_UID"];
				this.columnCUSTOM_FIELD_UID = base.Columns["CUSTOM_FIELD_UID"];
				this.columnPROJ_UID = base.Columns["PROJ_UID"];
				this.columnSTART_DATE = base.Columns["START_DATE"];
				this.columnDURATION = base.Columns["DURATION"];
				this.columnRESOURCE_COST = base.Columns["RESOURCE_COST"];
				this.columnRESOURCE_WORK = base.Columns["RESOURCE_WORK"];
				this.columnRES_HIRE_UID = base.Columns["RES_HIRE_UID"];
				this.columnROLE_NAME = base.Columns["ROLE_NAME"];
				this.columnPROJ_NAME = base.Columns["PROJ_NAME"];
			}

			// Token: 0x06003096 RID: 12438 RVA: 0x0009BF08 File Offset: 0x0009A108
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			private void InitClass()
			{
				this.columnSOLUTION_UID = new DataColumn("SOLUTION_UID", typeof(Guid), null, MappingType.Element);
				base.Columns.Add(this.columnSOLUTION_UID);
				this.columnLT_STRUCT_UID = new DataColumn("LT_STRUCT_UID", typeof(Guid), null, MappingType.Element);
				base.Columns.Add(this.columnLT_STRUCT_UID);
				this.columnCUSTOM_FIELD_UID = new DataColumn("CUSTOM_FIELD_UID", typeof(Guid), null, MappingType.Element);
				base.Columns.Add(this.columnCUSTOM_FIELD_UID);
				this.columnPROJ_UID = new DataColumn("PROJ_UID", typeof(Guid), null, MappingType.Element);
				base.Columns.Add(this.columnPROJ_UID);
				this.columnSTART_DATE = new DataColumn("START_DATE", typeof(DateTime), null, MappingType.Element);
				base.Columns.Add(this.columnSTART_DATE);
				this.columnDURATION = new DataColumn("DURATION", typeof(int), null, MappingType.Element);
				base.Columns.Add(this.columnDURATION);
				this.columnRESOURCE_COST = new DataColumn("RESOURCE_COST", typeof(decimal), null, MappingType.Element);
				base.Columns.Add(this.columnRESOURCE_COST);
				this.columnRESOURCE_WORK = new DataColumn("RESOURCE_WORK", typeof(decimal), null, MappingType.Element);
				base.Columns.Add(this.columnRESOURCE_WORK);
				this.columnRES_HIRE_UID = new DataColumn("RES_HIRE_UID", typeof(Guid), null, MappingType.Element);
				base.Columns.Add(this.columnRES_HIRE_UID);
				this.columnROLE_NAME = new DataColumn("ROLE_NAME", typeof(string), null, MappingType.Element);
				base.Columns.Add(this.columnROLE_NAME);
				this.columnPROJ_NAME = new DataColumn("PROJ_NAME", typeof(string), null, MappingType.Element);
				base.Columns.Add(this.columnPROJ_NAME);
				base.Constraints.Add(new UniqueConstraint("PK_SolutionResHired", new DataColumn[]
				{
					this.columnRES_HIRE_UID
				}, true));
				this.columnSOLUTION_UID.AllowDBNull = false;
				this.columnSOLUTION_UID.ReadOnly = true;
				this.columnLT_STRUCT_UID.AllowDBNull = false;
				this.columnLT_STRUCT_UID.ReadOnly = true;
				this.columnCUSTOM_FIELD_UID.AllowDBNull = false;
				this.columnCUSTOM_FIELD_UID.ReadOnly = true;
				this.columnPROJ_UID.AllowDBNull = false;
				this.columnPROJ_UID.ReadOnly = true;
				this.columnSTART_DATE.AllowDBNull = false;
				this.columnSTART_DATE.ReadOnly = true;
				this.columnDURATION.AllowDBNull = false;
				this.columnDURATION.ReadOnly = true;
				this.columnRESOURCE_COST.AllowDBNull = false;
				this.columnRESOURCE_COST.ReadOnly = true;
				this.columnRESOURCE_WORK.AllowDBNull = false;
				this.columnRESOURCE_WORK.ReadOnly = true;
				this.columnRES_HIRE_UID.AllowDBNull = false;
				this.columnRES_HIRE_UID.ReadOnly = true;
				this.columnRES_HIRE_UID.Unique = true;
				this.columnROLE_NAME.ReadOnly = true;
				this.columnPROJ_NAME.ReadOnly = true;
			}

			// Token: 0x06003097 RID: 12439 RVA: 0x0009C227 File Offset: 0x0009A427
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			public PlannerSolutionDataSet.SolutionResHiredRow NewSolutionResHiredRow()
			{
				return (PlannerSolutionDataSet.SolutionResHiredRow)base.NewRow();
			}

			// Token: 0x06003098 RID: 12440 RVA: 0x0009C234 File Offset: 0x0009A434
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			protected override DataRow NewRowFromBuilder(DataRowBuilder builder)
			{
				return new PlannerSolutionDataSet.SolutionResHiredRow(builder);
			}

			// Token: 0x06003099 RID: 12441 RVA: 0x0009C23C File Offset: 0x0009A43C
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			protected override Type GetRowType()
			{
				return typeof(PlannerSolutionDataSet.SolutionResHiredRow);
			}

			// Token: 0x0600309A RID: 12442 RVA: 0x0009C248 File Offset: 0x0009A448
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			protected override void OnRowChanged(DataRowChangeEventArgs e)
			{
				base.OnRowChanged(e);
				if (this.SolutionResHiredRowChanged != null)
				{
					this.SolutionResHiredRowChanged(this, new PlannerSolutionDataSet.SolutionResHiredRowChangeEvent((PlannerSolutionDataSet.SolutionResHiredRow)e.Row, e.Action));
				}
			}

			// Token: 0x0600309B RID: 12443 RVA: 0x0009C27B File Offset: 0x0009A47B
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			protected override void OnRowChanging(DataRowChangeEventArgs e)
			{
				base.OnRowChanging(e);
				if (this.SolutionResHiredRowChanging != null)
				{
					this.SolutionResHiredRowChanging(this, new PlannerSolutionDataSet.SolutionResHiredRowChangeEvent((PlannerSolutionDataSet.SolutionResHiredRow)e.Row, e.Action));
				}
			}

			// Token: 0x0600309C RID: 12444 RVA: 0x0009C2AE File Offset: 0x0009A4AE
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			protected override void OnRowDeleted(DataRowChangeEventArgs e)
			{
				base.OnRowDeleted(e);
				if (this.SolutionResHiredRowDeleted != null)
				{
					this.SolutionResHiredRowDeleted(this, new PlannerSolutionDataSet.SolutionResHiredRowChangeEvent((PlannerSolutionDataSet.SolutionResHiredRow)e.Row, e.Action));
				}
			}

			// Token: 0x0600309D RID: 12445 RVA: 0x0009C2E1 File Offset: 0x0009A4E1
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			protected override void OnRowDeleting(DataRowChangeEventArgs e)
			{
				base.OnRowDeleting(e);
				if (this.SolutionResHiredRowDeleting != null)
				{
					this.SolutionResHiredRowDeleting(this, new PlannerSolutionDataSet.SolutionResHiredRowChangeEvent((PlannerSolutionDataSet.SolutionResHiredRow)e.Row, e.Action));
				}
			}

			// Token: 0x0600309E RID: 12446 RVA: 0x0009C314 File Offset: 0x0009A514
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public void RemoveSolutionResHiredRow(PlannerSolutionDataSet.SolutionResHiredRow row)
			{
				base.Rows.Remove(row);
			}

			// Token: 0x0600309F RID: 12447 RVA: 0x0009C324 File Offset: 0x0009A524
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			public static XmlSchemaComplexType GetTypedTableSchema(XmlSchemaSet xs)
			{
				XmlSchemaComplexType xmlSchemaComplexType = new XmlSchemaComplexType();
				XmlSchemaSequence xmlSchemaSequence = new XmlSchemaSequence();
				PlannerSolutionDataSet plannerSolutionDataSet = new PlannerSolutionDataSet();
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
				xmlSchemaAttribute.FixedValue = plannerSolutionDataSet.Namespace;
				xmlSchemaComplexType.Attributes.Add(xmlSchemaAttribute);
				XmlSchemaAttribute xmlSchemaAttribute2 = new XmlSchemaAttribute();
				xmlSchemaAttribute2.Name = "tableTypeName";
				xmlSchemaAttribute2.FixedValue = "SolutionResHiredDataTable";
				xmlSchemaComplexType.Attributes.Add(xmlSchemaAttribute2);
				xmlSchemaComplexType.Particle = xmlSchemaSequence;
				XmlSchema schemaSerializable = plannerSolutionDataSet.GetSchemaSerializable();
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

			// Token: 0x04000A3A RID: 2618
			private DataColumn columnSOLUTION_UID;

			// Token: 0x04000A3B RID: 2619
			private DataColumn columnLT_STRUCT_UID;

			// Token: 0x04000A3C RID: 2620
			private DataColumn columnCUSTOM_FIELD_UID;

			// Token: 0x04000A3D RID: 2621
			private DataColumn columnPROJ_UID;

			// Token: 0x04000A3E RID: 2622
			private DataColumn columnSTART_DATE;

			// Token: 0x04000A3F RID: 2623
			private DataColumn columnDURATION;

			// Token: 0x04000A40 RID: 2624
			private DataColumn columnRESOURCE_COST;

			// Token: 0x04000A41 RID: 2625
			private DataColumn columnRESOURCE_WORK;

			// Token: 0x04000A42 RID: 2626
			private DataColumn columnRES_HIRE_UID;

			// Token: 0x04000A43 RID: 2627
			private DataColumn columnROLE_NAME;

			// Token: 0x04000A44 RID: 2628
			private DataColumn columnPROJ_NAME;
		}

		// Token: 0x02000267 RID: 615
		[XmlSchemaProvider("GetTypedTableSchema")]
		[Serializable]
		public class SolutionProjectsDataTable : DataTable, IEnumerable
		{
			// Token: 0x060030A0 RID: 12448 RVA: 0x0009C51C File Offset: 0x0009A71C
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			public SolutionProjectsDataTable()
			{
				base.TableName = "SolutionProjects";
				this.BeginInit();
				this.InitClass();
				this.EndInit();
			}

			// Token: 0x060030A1 RID: 12449 RVA: 0x0009C544 File Offset: 0x0009A744
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			internal SolutionProjectsDataTable(DataTable table)
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

			// Token: 0x060030A2 RID: 12450 RVA: 0x0009C5EC File Offset: 0x0009A7EC
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			protected SolutionProjectsDataTable(SerializationInfo info, StreamingContext context) : base(info, context)
			{
				this.InitVars();
			}

			// Token: 0x17000E25 RID: 3621
			// (get) Token: 0x060030A3 RID: 12451 RVA: 0x0009C5FC File Offset: 0x0009A7FC
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public DataColumn SOLUTION_UIDColumn
			{
				get
				{
					return this.columnSOLUTION_UID;
				}
			}

			// Token: 0x17000E26 RID: 3622
			// (get) Token: 0x060030A4 RID: 12452 RVA: 0x0009C604 File Offset: 0x0009A804
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			public DataColumn PROJ_UIDColumn
			{
				get
				{
					return this.columnPROJ_UID;
				}
			}

			// Token: 0x17000E27 RID: 3623
			// (get) Token: 0x060030A5 RID: 12453 RVA: 0x0009C60C File Offset: 0x0009A80C
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			public DataColumn NEW_START_DATEColumn
			{
				get
				{
					return this.columnNEW_START_DATE;
				}
			}

			// Token: 0x17000E28 RID: 3624
			// (get) Token: 0x060030A6 RID: 12454 RVA: 0x0009C614 File Offset: 0x0009A814
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			public DataColumn FORCE_STATUSColumn
			{
				get
				{
					return this.columnFORCE_STATUS;
				}
			}

			// Token: 0x17000E29 RID: 3625
			// (get) Token: 0x060030A7 RID: 12455 RVA: 0x0009C61C File Offset: 0x0009A81C
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			public DataColumn STATUSColumn
			{
				get
				{
					return this.columnSTATUS;
				}
			}

			// Token: 0x17000E2A RID: 3626
			// (get) Token: 0x060030A8 RID: 12456 RVA: 0x0009C624 File Offset: 0x0009A824
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			public DataColumn RESOURCE_COSTColumn
			{
				get
				{
					return this.columnRESOURCE_COST;
				}
			}

			// Token: 0x17000E2B RID: 3627
			// (get) Token: 0x060030A9 RID: 12457 RVA: 0x0009C62C File Offset: 0x0009A82C
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public DataColumn RESOURCE_WORKColumn
			{
				get
				{
					return this.columnRESOURCE_WORK;
				}
			}

			// Token: 0x17000E2C RID: 3628
			// (get) Token: 0x060030AA RID: 12458 RVA: 0x0009C634 File Offset: 0x0009A834
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public DataColumn PROJ_NAMEColumn
			{
				get
				{
					return this.columnPROJ_NAME;
				}
			}

			// Token: 0x17000E2D RID: 3629
			// (get) Token: 0x060030AB RID: 12459 RVA: 0x0009C63C File Offset: 0x0009A83C
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			public DataColumn PRIORITYColumn
			{
				get
				{
					return this.columnPRIORITY;
				}
			}

			// Token: 0x17000E2E RID: 3630
			// (get) Token: 0x060030AC RID: 12460 RVA: 0x0009C644 File Offset: 0x0009A844
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			public DataColumn ABSOLUTE_PRIORITYColumn
			{
				get
				{
					return this.columnABSOLUTE_PRIORITY;
				}
			}

			// Token: 0x17000E2F RID: 3631
			// (get) Token: 0x060030AD RID: 12461 RVA: 0x0009C64C File Offset: 0x0009A84C
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public DataColumn DURATIONColumn
			{
				get
				{
					return this.columnDURATION;
				}
			}

			// Token: 0x17000E30 RID: 3632
			// (get) Token: 0x060030AE RID: 12462 RVA: 0x0009C654 File Offset: 0x0009A854
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public DataColumn SNETColumn
			{
				get
				{
					return this.columnSNET;
				}
			}

			// Token: 0x17000E31 RID: 3633
			// (get) Token: 0x060030AF RID: 12463 RVA: 0x0009C65C File Offset: 0x0009A85C
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public DataColumn FNLTColumn
			{
				get
				{
					return this.columnFNLT;
				}
			}

			// Token: 0x17000E32 RID: 3634
			// (get) Token: 0x060030B0 RID: 12464 RVA: 0x0009C664 File Offset: 0x0009A864
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public DataColumn LOCKEDColumn
			{
				get
				{
					return this.columnLOCKED;
				}
			}

			// Token: 0x17000E33 RID: 3635
			// (get) Token: 0x060030B1 RID: 12465 RVA: 0x0009C66C File Offset: 0x0009A86C
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			public DataColumn FORCE_ALIAS_LT_STRUCT_UIDColumn
			{
				get
				{
					return this.columnFORCE_ALIAS_LT_STRUCT_UID;
				}
			}

			// Token: 0x17000E34 RID: 3636
			// (get) Token: 0x060030B2 RID: 12466 RVA: 0x0009C674 File Offset: 0x0009A874
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public DataColumn FORCE_ALIAS_LT_VALUE_FULLColumn
			{
				get
				{
					return this.columnFORCE_ALIAS_LT_VALUE_FULL;
				}
			}

			// Token: 0x17000E35 RID: 3637
			// (get) Token: 0x060030B3 RID: 12467 RVA: 0x0009C67C File Offset: 0x0009A87C
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

			// Token: 0x17000E36 RID: 3638
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public PlannerSolutionDataSet.SolutionProjectsRow this[int index]
			{
				get
				{
					return (PlannerSolutionDataSet.SolutionProjectsRow)base.Rows[index];
				}
			}

			// Token: 0x14000211 RID: 529
			// (add) Token: 0x060030B5 RID: 12469 RVA: 0x0009C69C File Offset: 0x0009A89C
			// (remove) Token: 0x060030B6 RID: 12470 RVA: 0x0009C6D4 File Offset: 0x0009A8D4
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public event PlannerSolutionDataSet.SolutionProjectsRowChangeEventHandler SolutionProjectsRowChanging;

			// Token: 0x14000212 RID: 530
			// (add) Token: 0x060030B7 RID: 12471 RVA: 0x0009C70C File Offset: 0x0009A90C
			// (remove) Token: 0x060030B8 RID: 12472 RVA: 0x0009C744 File Offset: 0x0009A944
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public event PlannerSolutionDataSet.SolutionProjectsRowChangeEventHandler SolutionProjectsRowChanged;

			// Token: 0x14000213 RID: 531
			// (add) Token: 0x060030B9 RID: 12473 RVA: 0x0009C77C File Offset: 0x0009A97C
			// (remove) Token: 0x060030BA RID: 12474 RVA: 0x0009C7B4 File Offset: 0x0009A9B4
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public event PlannerSolutionDataSet.SolutionProjectsRowChangeEventHandler SolutionProjectsRowDeleting;

			// Token: 0x14000214 RID: 532
			// (add) Token: 0x060030BB RID: 12475 RVA: 0x0009C7EC File Offset: 0x0009A9EC
			// (remove) Token: 0x060030BC RID: 12476 RVA: 0x0009C824 File Offset: 0x0009AA24
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public event PlannerSolutionDataSet.SolutionProjectsRowChangeEventHandler SolutionProjectsRowDeleted;

			// Token: 0x060030BD RID: 12477 RVA: 0x0009C859 File Offset: 0x0009AA59
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			public void AddSolutionProjectsRow(PlannerSolutionDataSet.SolutionProjectsRow row)
			{
				base.Rows.Add(row);
			}

			// Token: 0x060030BE RID: 12478 RVA: 0x0009C868 File Offset: 0x0009AA68
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public PlannerSolutionDataSet.SolutionProjectsRow AddSolutionProjectsRow(PlannerSolutionDataSet.SolutionsRow parentSolutionsRowByFK_Solutions_SolutionProjects, Guid PROJ_UID, DateTime NEW_START_DATE, byte FORCE_STATUS, byte STATUS, decimal RESOURCE_COST, decimal RESOURCE_WORK, string PROJ_NAME, double PRIORITY, double ABSOLUTE_PRIORITY, int DURATION, DateTime SNET, DateTime FNLT, byte LOCKED, Guid FORCE_ALIAS_LT_STRUCT_UID, string FORCE_ALIAS_LT_VALUE_FULL)
			{
				PlannerSolutionDataSet.SolutionProjectsRow solutionProjectsRow = (PlannerSolutionDataSet.SolutionProjectsRow)base.NewRow();
				object[] array = new object[]
				{
					null,
					PROJ_UID,
					NEW_START_DATE,
					FORCE_STATUS,
					STATUS,
					RESOURCE_COST,
					RESOURCE_WORK,
					PROJ_NAME,
					PRIORITY,
					ABSOLUTE_PRIORITY,
					DURATION,
					SNET,
					FNLT,
					LOCKED,
					FORCE_ALIAS_LT_STRUCT_UID,
					FORCE_ALIAS_LT_VALUE_FULL
				};
				if (parentSolutionsRowByFK_Solutions_SolutionProjects != null)
				{
					array[0] = parentSolutionsRowByFK_Solutions_SolutionProjects[0];
				}
				solutionProjectsRow.ItemArray = array;
				base.Rows.Add(solutionProjectsRow);
				return solutionProjectsRow;
			}

			// Token: 0x060030BF RID: 12479 RVA: 0x0009C940 File Offset: 0x0009AB40
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public PlannerSolutionDataSet.SolutionProjectsRow FindBySOLUTION_UIDPROJ_UID(Guid SOLUTION_UID, Guid PROJ_UID)
			{
				return (PlannerSolutionDataSet.SolutionProjectsRow)base.Rows.Find(new object[]
				{
					SOLUTION_UID,
					PROJ_UID
				});
			}

			// Token: 0x060030C0 RID: 12480 RVA: 0x0009C977 File Offset: 0x0009AB77
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			public virtual IEnumerator GetEnumerator()
			{
				return base.Rows.GetEnumerator();
			}

			// Token: 0x060030C1 RID: 12481 RVA: 0x0009C984 File Offset: 0x0009AB84
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public override DataTable Clone()
			{
				PlannerSolutionDataSet.SolutionProjectsDataTable solutionProjectsDataTable = (PlannerSolutionDataSet.SolutionProjectsDataTable)base.Clone();
				solutionProjectsDataTable.InitVars();
				return solutionProjectsDataTable;
			}

			// Token: 0x060030C2 RID: 12482 RVA: 0x0009C9A4 File Offset: 0x0009ABA4
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			protected override DataTable CreateInstance()
			{
				return new PlannerSolutionDataSet.SolutionProjectsDataTable();
			}

			// Token: 0x060030C3 RID: 12483 RVA: 0x0009C9AC File Offset: 0x0009ABAC
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			internal void InitVars()
			{
				this.columnSOLUTION_UID = base.Columns["SOLUTION_UID"];
				this.columnPROJ_UID = base.Columns["PROJ_UID"];
				this.columnNEW_START_DATE = base.Columns["NEW_START_DATE"];
				this.columnFORCE_STATUS = base.Columns["FORCE_STATUS"];
				this.columnSTATUS = base.Columns["STATUS"];
				this.columnRESOURCE_COST = base.Columns["RESOURCE_COST"];
				this.columnRESOURCE_WORK = base.Columns["RESOURCE_WORK"];
				this.columnPROJ_NAME = base.Columns["PROJ_NAME"];
				this.columnPRIORITY = base.Columns["PRIORITY"];
				this.columnABSOLUTE_PRIORITY = base.Columns["ABSOLUTE_PRIORITY"];
				this.columnDURATION = base.Columns["DURATION"];
				this.columnSNET = base.Columns["SNET"];
				this.columnFNLT = base.Columns["FNLT"];
				this.columnLOCKED = base.Columns["LOCKED"];
				this.columnFORCE_ALIAS_LT_STRUCT_UID = base.Columns["FORCE_ALIAS_LT_STRUCT_UID"];
				this.columnFORCE_ALIAS_LT_VALUE_FULL = base.Columns["FORCE_ALIAS_LT_VALUE_FULL"];
			}

			// Token: 0x060030C4 RID: 12484 RVA: 0x0009CB1C File Offset: 0x0009AD1C
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			private void InitClass()
			{
				this.columnSOLUTION_UID = new DataColumn("SOLUTION_UID", typeof(Guid), null, MappingType.Element);
				base.Columns.Add(this.columnSOLUTION_UID);
				this.columnPROJ_UID = new DataColumn("PROJ_UID", typeof(Guid), null, MappingType.Element);
				base.Columns.Add(this.columnPROJ_UID);
				this.columnNEW_START_DATE = new DataColumn("NEW_START_DATE", typeof(DateTime), null, MappingType.Element);
				base.Columns.Add(this.columnNEW_START_DATE);
				this.columnFORCE_STATUS = new DataColumn("FORCE_STATUS", typeof(byte), null, MappingType.Element);
				base.Columns.Add(this.columnFORCE_STATUS);
				this.columnSTATUS = new DataColumn("STATUS", typeof(byte), null, MappingType.Element);
				base.Columns.Add(this.columnSTATUS);
				this.columnRESOURCE_COST = new DataColumn("RESOURCE_COST", typeof(decimal), null, MappingType.Element);
				base.Columns.Add(this.columnRESOURCE_COST);
				this.columnRESOURCE_WORK = new DataColumn("RESOURCE_WORK", typeof(decimal), null, MappingType.Element);
				base.Columns.Add(this.columnRESOURCE_WORK);
				this.columnPROJ_NAME = new DataColumn("PROJ_NAME", typeof(string), null, MappingType.Element);
				base.Columns.Add(this.columnPROJ_NAME);
				this.columnPRIORITY = new DataColumn("PRIORITY", typeof(double), null, MappingType.Element);
				base.Columns.Add(this.columnPRIORITY);
				this.columnABSOLUTE_PRIORITY = new DataColumn("ABSOLUTE_PRIORITY", typeof(double), null, MappingType.Element);
				base.Columns.Add(this.columnABSOLUTE_PRIORITY);
				this.columnDURATION = new DataColumn("DURATION", typeof(int), null, MappingType.Element);
				base.Columns.Add(this.columnDURATION);
				this.columnSNET = new DataColumn("SNET", typeof(DateTime), null, MappingType.Element);
				base.Columns.Add(this.columnSNET);
				this.columnFNLT = new DataColumn("FNLT", typeof(DateTime), null, MappingType.Element);
				base.Columns.Add(this.columnFNLT);
				this.columnLOCKED = new DataColumn("LOCKED", typeof(byte), null, MappingType.Element);
				base.Columns.Add(this.columnLOCKED);
				this.columnFORCE_ALIAS_LT_STRUCT_UID = new DataColumn("FORCE_ALIAS_LT_STRUCT_UID", typeof(Guid), null, MappingType.Element);
				base.Columns.Add(this.columnFORCE_ALIAS_LT_STRUCT_UID);
				this.columnFORCE_ALIAS_LT_VALUE_FULL = new DataColumn("FORCE_ALIAS_LT_VALUE_FULL", typeof(string), null, MappingType.Element);
				base.Columns.Add(this.columnFORCE_ALIAS_LT_VALUE_FULL);
				base.Constraints.Add(new UniqueConstraint("PK_SolutionProjects", new DataColumn[]
				{
					this.columnSOLUTION_UID,
					this.columnPROJ_UID
				}, true));
				this.columnSOLUTION_UID.AllowDBNull = false;
				this.columnPROJ_UID.AllowDBNull = false;
				this.columnNEW_START_DATE.AllowDBNull = false;
				this.columnFORCE_STATUS.AllowDBNull = false;
				this.columnPROJ_NAME.ReadOnly = true;
				this.columnPRIORITY.ReadOnly = true;
				this.columnABSOLUTE_PRIORITY.ReadOnly = true;
				this.columnDURATION.ReadOnly = true;
				this.columnSNET.ReadOnly = true;
				this.columnFNLT.ReadOnly = true;
				this.columnLOCKED.AllowDBNull = false;
				this.columnLOCKED.ReadOnly = true;
				this.columnLOCKED.DefaultValue = 0;
				this.columnFORCE_ALIAS_LT_VALUE_FULL.ReadOnly = true;
			}

			// Token: 0x060030C5 RID: 12485 RVA: 0x0009CED6 File Offset: 0x0009B0D6
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public PlannerSolutionDataSet.SolutionProjectsRow NewSolutionProjectsRow()
			{
				return (PlannerSolutionDataSet.SolutionProjectsRow)base.NewRow();
			}

			// Token: 0x060030C6 RID: 12486 RVA: 0x0009CEE3 File Offset: 0x0009B0E3
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			protected override DataRow NewRowFromBuilder(DataRowBuilder builder)
			{
				return new PlannerSolutionDataSet.SolutionProjectsRow(builder);
			}

			// Token: 0x060030C7 RID: 12487 RVA: 0x0009CEEB File Offset: 0x0009B0EB
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			protected override Type GetRowType()
			{
				return typeof(PlannerSolutionDataSet.SolutionProjectsRow);
			}

			// Token: 0x060030C8 RID: 12488 RVA: 0x0009CEF7 File Offset: 0x0009B0F7
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			protected override void OnRowChanged(DataRowChangeEventArgs e)
			{
				base.OnRowChanged(e);
				if (this.SolutionProjectsRowChanged != null)
				{
					this.SolutionProjectsRowChanged(this, new PlannerSolutionDataSet.SolutionProjectsRowChangeEvent((PlannerSolutionDataSet.SolutionProjectsRow)e.Row, e.Action));
				}
			}

			// Token: 0x060030C9 RID: 12489 RVA: 0x0009CF2A File Offset: 0x0009B12A
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			protected override void OnRowChanging(DataRowChangeEventArgs e)
			{
				base.OnRowChanging(e);
				if (this.SolutionProjectsRowChanging != null)
				{
					this.SolutionProjectsRowChanging(this, new PlannerSolutionDataSet.SolutionProjectsRowChangeEvent((PlannerSolutionDataSet.SolutionProjectsRow)e.Row, e.Action));
				}
			}

			// Token: 0x060030CA RID: 12490 RVA: 0x0009CF5D File Offset: 0x0009B15D
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			protected override void OnRowDeleted(DataRowChangeEventArgs e)
			{
				base.OnRowDeleted(e);
				if (this.SolutionProjectsRowDeleted != null)
				{
					this.SolutionProjectsRowDeleted(this, new PlannerSolutionDataSet.SolutionProjectsRowChangeEvent((PlannerSolutionDataSet.SolutionProjectsRow)e.Row, e.Action));
				}
			}

			// Token: 0x060030CB RID: 12491 RVA: 0x0009CF90 File Offset: 0x0009B190
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			protected override void OnRowDeleting(DataRowChangeEventArgs e)
			{
				base.OnRowDeleting(e);
				if (this.SolutionProjectsRowDeleting != null)
				{
					this.SolutionProjectsRowDeleting(this, new PlannerSolutionDataSet.SolutionProjectsRowChangeEvent((PlannerSolutionDataSet.SolutionProjectsRow)e.Row, e.Action));
				}
			}

			// Token: 0x060030CC RID: 12492 RVA: 0x0009CFC3 File Offset: 0x0009B1C3
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public void RemoveSolutionProjectsRow(PlannerSolutionDataSet.SolutionProjectsRow row)
			{
				base.Rows.Remove(row);
			}

			// Token: 0x060030CD RID: 12493 RVA: 0x0009CFD4 File Offset: 0x0009B1D4
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			public static XmlSchemaComplexType GetTypedTableSchema(XmlSchemaSet xs)
			{
				XmlSchemaComplexType xmlSchemaComplexType = new XmlSchemaComplexType();
				XmlSchemaSequence xmlSchemaSequence = new XmlSchemaSequence();
				PlannerSolutionDataSet plannerSolutionDataSet = new PlannerSolutionDataSet();
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
				xmlSchemaAttribute.FixedValue = plannerSolutionDataSet.Namespace;
				xmlSchemaComplexType.Attributes.Add(xmlSchemaAttribute);
				XmlSchemaAttribute xmlSchemaAttribute2 = new XmlSchemaAttribute();
				xmlSchemaAttribute2.Name = "tableTypeName";
				xmlSchemaAttribute2.FixedValue = "SolutionProjectsDataTable";
				xmlSchemaComplexType.Attributes.Add(xmlSchemaAttribute2);
				xmlSchemaComplexType.Particle = xmlSchemaSequence;
				XmlSchema schemaSerializable = plannerSolutionDataSet.GetSchemaSerializable();
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

			// Token: 0x04000A49 RID: 2633
			private DataColumn columnSOLUTION_UID;

			// Token: 0x04000A4A RID: 2634
			private DataColumn columnPROJ_UID;

			// Token: 0x04000A4B RID: 2635
			private DataColumn columnNEW_START_DATE;

			// Token: 0x04000A4C RID: 2636
			private DataColumn columnFORCE_STATUS;

			// Token: 0x04000A4D RID: 2637
			private DataColumn columnSTATUS;

			// Token: 0x04000A4E RID: 2638
			private DataColumn columnRESOURCE_COST;

			// Token: 0x04000A4F RID: 2639
			private DataColumn columnRESOURCE_WORK;

			// Token: 0x04000A50 RID: 2640
			private DataColumn columnPROJ_NAME;

			// Token: 0x04000A51 RID: 2641
			private DataColumn columnPRIORITY;

			// Token: 0x04000A52 RID: 2642
			private DataColumn columnABSOLUTE_PRIORITY;

			// Token: 0x04000A53 RID: 2643
			private DataColumn columnDURATION;

			// Token: 0x04000A54 RID: 2644
			private DataColumn columnSNET;

			// Token: 0x04000A55 RID: 2645
			private DataColumn columnFNLT;

			// Token: 0x04000A56 RID: 2646
			private DataColumn columnLOCKED;

			// Token: 0x04000A57 RID: 2647
			private DataColumn columnFORCE_ALIAS_LT_STRUCT_UID;

			// Token: 0x04000A58 RID: 2648
			private DataColumn columnFORCE_ALIAS_LT_VALUE_FULL;
		}

		// Token: 0x02000268 RID: 616
		[XmlSchemaProvider("GetTypedTableSchema")]
		[Serializable]
		public class SolutionProjectRequirementsByRoleDataTable : DataTable, IEnumerable
		{
			// Token: 0x060030CE RID: 12494 RVA: 0x0009D1CC File Offset: 0x0009B3CC
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public SolutionProjectRequirementsByRoleDataTable()
			{
				base.TableName = "SolutionProjectRequirementsByRole";
				this.BeginInit();
				this.InitClass();
				this.EndInit();
			}

			// Token: 0x060030CF RID: 12495 RVA: 0x0009D1F4 File Offset: 0x0009B3F4
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			internal SolutionProjectRequirementsByRoleDataTable(DataTable table)
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

			// Token: 0x060030D0 RID: 12496 RVA: 0x0009D29C File Offset: 0x0009B49C
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			protected SolutionProjectRequirementsByRoleDataTable(SerializationInfo info, StreamingContext context) : base(info, context)
			{
				this.InitVars();
			}

			// Token: 0x17000E37 RID: 3639
			// (get) Token: 0x060030D1 RID: 12497 RVA: 0x0009D2AC File Offset: 0x0009B4AC
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			public DataColumn SOLUTION_UIDColumn
			{
				get
				{
					return this.columnSOLUTION_UID;
				}
			}

			// Token: 0x17000E38 RID: 3640
			// (get) Token: 0x060030D2 RID: 12498 RVA: 0x0009D2B4 File Offset: 0x0009B4B4
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			public DataColumn PROJ_UIDColumn
			{
				get
				{
					return this.columnPROJ_UID;
				}
			}

			// Token: 0x17000E39 RID: 3641
			// (get) Token: 0x060030D3 RID: 12499 RVA: 0x0009D2BC File Offset: 0x0009B4BC
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public DataColumn CUSTOM_FIELD_UIDColumn
			{
				get
				{
					return this.columnCUSTOM_FIELD_UID;
				}
			}

			// Token: 0x17000E3A RID: 3642
			// (get) Token: 0x060030D4 RID: 12500 RVA: 0x0009D2C4 File Offset: 0x0009B4C4
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			public DataColumn LT_STRUCT_UIDColumn
			{
				get
				{
					return this.columnLT_STRUCT_UID;
				}
			}

			// Token: 0x17000E3B RID: 3643
			// (get) Token: 0x060030D5 RID: 12501 RVA: 0x0009D2CC File Offset: 0x0009B4CC
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public DataColumn START_DATEColumn
			{
				get
				{
					return this.columnSTART_DATE;
				}
			}

			// Token: 0x17000E3C RID: 3644
			// (get) Token: 0x060030D6 RID: 12502 RVA: 0x0009D2D4 File Offset: 0x0009B4D4
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			public DataColumn ADDITIONAL_WORKColumn
			{
				get
				{
					return this.columnADDITIONAL_WORK;
				}
			}

			// Token: 0x17000E3D RID: 3645
			// (get) Token: 0x060030D7 RID: 12503 RVA: 0x0009D2DC File Offset: 0x0009B4DC
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			public DataColumn REQUIREMENT_UIDColumn
			{
				get
				{
					return this.columnREQUIREMENT_UID;
				}
			}

			// Token: 0x17000E3E RID: 3646
			// (get) Token: 0x060030D8 RID: 12504 RVA: 0x0009D2E4 File Offset: 0x0009B4E4
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			public DataColumn ROLE_NAMEColumn
			{
				get
				{
					return this.columnROLE_NAME;
				}
			}

			// Token: 0x17000E3F RID: 3647
			// (get) Token: 0x060030D9 RID: 12505 RVA: 0x0009D2EC File Offset: 0x0009B4EC
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			public DataColumn PROJ_NAMEColumn
			{
				get
				{
					return this.columnPROJ_NAME;
				}
			}

			// Token: 0x17000E40 RID: 3648
			// (get) Token: 0x060030DA RID: 12506 RVA: 0x0009D2F4 File Offset: 0x0009B4F4
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

			// Token: 0x17000E41 RID: 3649
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public PlannerSolutionDataSet.SolutionProjectRequirementsByRoleRow this[int index]
			{
				get
				{
					return (PlannerSolutionDataSet.SolutionProjectRequirementsByRoleRow)base.Rows[index];
				}
			}

			// Token: 0x14000215 RID: 533
			// (add) Token: 0x060030DC RID: 12508 RVA: 0x0009D314 File Offset: 0x0009B514
			// (remove) Token: 0x060030DD RID: 12509 RVA: 0x0009D34C File Offset: 0x0009B54C
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public event PlannerSolutionDataSet.SolutionProjectRequirementsByRoleRowChangeEventHandler SolutionProjectRequirementsByRoleRowChanging;

			// Token: 0x14000216 RID: 534
			// (add) Token: 0x060030DE RID: 12510 RVA: 0x0009D384 File Offset: 0x0009B584
			// (remove) Token: 0x060030DF RID: 12511 RVA: 0x0009D3BC File Offset: 0x0009B5BC
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public event PlannerSolutionDataSet.SolutionProjectRequirementsByRoleRowChangeEventHandler SolutionProjectRequirementsByRoleRowChanged;

			// Token: 0x14000217 RID: 535
			// (add) Token: 0x060030E0 RID: 12512 RVA: 0x0009D3F4 File Offset: 0x0009B5F4
			// (remove) Token: 0x060030E1 RID: 12513 RVA: 0x0009D42C File Offset: 0x0009B62C
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public event PlannerSolutionDataSet.SolutionProjectRequirementsByRoleRowChangeEventHandler SolutionProjectRequirementsByRoleRowDeleting;

			// Token: 0x14000218 RID: 536
			// (add) Token: 0x060030E2 RID: 12514 RVA: 0x0009D464 File Offset: 0x0009B664
			// (remove) Token: 0x060030E3 RID: 12515 RVA: 0x0009D49C File Offset: 0x0009B69C
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public event PlannerSolutionDataSet.SolutionProjectRequirementsByRoleRowChangeEventHandler SolutionProjectRequirementsByRoleRowDeleted;

			// Token: 0x060030E4 RID: 12516 RVA: 0x0009D4D1 File Offset: 0x0009B6D1
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			public void AddSolutionProjectRequirementsByRoleRow(PlannerSolutionDataSet.SolutionProjectRequirementsByRoleRow row)
			{
				base.Rows.Add(row);
			}

			// Token: 0x060030E5 RID: 12517 RVA: 0x0009D4E0 File Offset: 0x0009B6E0
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			public PlannerSolutionDataSet.SolutionProjectRequirementsByRoleRow AddSolutionProjectRequirementsByRoleRow(PlannerSolutionDataSet.SolutionsRow parentSolutionsRowByFK_Solutions_SolutionProjectRequirementsByRole, Guid PROJ_UID, Guid CUSTOM_FIELD_UID, Guid LT_STRUCT_UID, DateTime START_DATE, decimal ADDITIONAL_WORK, Guid REQUIREMENT_UID, string ROLE_NAME, string PROJ_NAME)
			{
				PlannerSolutionDataSet.SolutionProjectRequirementsByRoleRow solutionProjectRequirementsByRoleRow = (PlannerSolutionDataSet.SolutionProjectRequirementsByRoleRow)base.NewRow();
				object[] array = new object[]
				{
					null,
					PROJ_UID,
					CUSTOM_FIELD_UID,
					LT_STRUCT_UID,
					START_DATE,
					ADDITIONAL_WORK,
					REQUIREMENT_UID,
					ROLE_NAME,
					PROJ_NAME
				};
				if (parentSolutionsRowByFK_Solutions_SolutionProjectRequirementsByRole != null)
				{
					array[0] = parentSolutionsRowByFK_Solutions_SolutionProjectRequirementsByRole[0];
				}
				solutionProjectRequirementsByRoleRow.ItemArray = array;
				base.Rows.Add(solutionProjectRequirementsByRoleRow);
				return solutionProjectRequirementsByRoleRow;
			}

			// Token: 0x060030E6 RID: 12518 RVA: 0x0009D568 File Offset: 0x0009B768
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			public PlannerSolutionDataSet.SolutionProjectRequirementsByRoleRow FindByREQUIREMENT_UID(Guid REQUIREMENT_UID)
			{
				return (PlannerSolutionDataSet.SolutionProjectRequirementsByRoleRow)base.Rows.Find(new object[]
				{
					REQUIREMENT_UID
				});
			}

			// Token: 0x060030E7 RID: 12519 RVA: 0x0009D596 File Offset: 0x0009B796
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			public virtual IEnumerator GetEnumerator()
			{
				return base.Rows.GetEnumerator();
			}

			// Token: 0x060030E8 RID: 12520 RVA: 0x0009D5A4 File Offset: 0x0009B7A4
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			public override DataTable Clone()
			{
				PlannerSolutionDataSet.SolutionProjectRequirementsByRoleDataTable solutionProjectRequirementsByRoleDataTable = (PlannerSolutionDataSet.SolutionProjectRequirementsByRoleDataTable)base.Clone();
				solutionProjectRequirementsByRoleDataTable.InitVars();
				return solutionProjectRequirementsByRoleDataTable;
			}

			// Token: 0x060030E9 RID: 12521 RVA: 0x0009D5C4 File Offset: 0x0009B7C4
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			protected override DataTable CreateInstance()
			{
				return new PlannerSolutionDataSet.SolutionProjectRequirementsByRoleDataTable();
			}

			// Token: 0x060030EA RID: 12522 RVA: 0x0009D5CC File Offset: 0x0009B7CC
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			internal void InitVars()
			{
				this.columnSOLUTION_UID = base.Columns["SOLUTION_UID"];
				this.columnPROJ_UID = base.Columns["PROJ_UID"];
				this.columnCUSTOM_FIELD_UID = base.Columns["CUSTOM_FIELD_UID"];
				this.columnLT_STRUCT_UID = base.Columns["LT_STRUCT_UID"];
				this.columnSTART_DATE = base.Columns["START_DATE"];
				this.columnADDITIONAL_WORK = base.Columns["ADDITIONAL_WORK"];
				this.columnREQUIREMENT_UID = base.Columns["REQUIREMENT_UID"];
				this.columnROLE_NAME = base.Columns["ROLE_NAME"];
				this.columnPROJ_NAME = base.Columns["PROJ_NAME"];
			}

			// Token: 0x060030EB RID: 12523 RVA: 0x0009D6A0 File Offset: 0x0009B8A0
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			private void InitClass()
			{
				this.columnSOLUTION_UID = new DataColumn("SOLUTION_UID", typeof(Guid), null, MappingType.Element);
				base.Columns.Add(this.columnSOLUTION_UID);
				this.columnPROJ_UID = new DataColumn("PROJ_UID", typeof(Guid), null, MappingType.Element);
				base.Columns.Add(this.columnPROJ_UID);
				this.columnCUSTOM_FIELD_UID = new DataColumn("CUSTOM_FIELD_UID", typeof(Guid), null, MappingType.Element);
				base.Columns.Add(this.columnCUSTOM_FIELD_UID);
				this.columnLT_STRUCT_UID = new DataColumn("LT_STRUCT_UID", typeof(Guid), null, MappingType.Element);
				base.Columns.Add(this.columnLT_STRUCT_UID);
				this.columnSTART_DATE = new DataColumn("START_DATE", typeof(DateTime), null, MappingType.Element);
				base.Columns.Add(this.columnSTART_DATE);
				this.columnADDITIONAL_WORK = new DataColumn("ADDITIONAL_WORK", typeof(decimal), null, MappingType.Element);
				base.Columns.Add(this.columnADDITIONAL_WORK);
				this.columnREQUIREMENT_UID = new DataColumn("REQUIREMENT_UID", typeof(Guid), null, MappingType.Element);
				base.Columns.Add(this.columnREQUIREMENT_UID);
				this.columnROLE_NAME = new DataColumn("ROLE_NAME", typeof(string), null, MappingType.Element);
				base.Columns.Add(this.columnROLE_NAME);
				this.columnPROJ_NAME = new DataColumn("PROJ_NAME", typeof(string), null, MappingType.Element);
				base.Columns.Add(this.columnPROJ_NAME);
				base.Constraints.Add(new UniqueConstraint("PK_SolutionProjectRequirementsByRole", new DataColumn[]
				{
					this.columnREQUIREMENT_UID
				}, true));
				this.columnSOLUTION_UID.AllowDBNull = false;
				this.columnSOLUTION_UID.ReadOnly = true;
				this.columnPROJ_UID.AllowDBNull = false;
				this.columnPROJ_UID.ReadOnly = true;
				this.columnCUSTOM_FIELD_UID.AllowDBNull = false;
				this.columnCUSTOM_FIELD_UID.ReadOnly = true;
				this.columnLT_STRUCT_UID.AllowDBNull = false;
				this.columnLT_STRUCT_UID.ReadOnly = true;
				this.columnSTART_DATE.AllowDBNull = false;
				this.columnSTART_DATE.ReadOnly = true;
				this.columnADDITIONAL_WORK.AllowDBNull = false;
				this.columnREQUIREMENT_UID.AllowDBNull = false;
				this.columnREQUIREMENT_UID.ReadOnly = true;
				this.columnREQUIREMENT_UID.Unique = true;
				this.columnROLE_NAME.ReadOnly = true;
				this.columnPROJ_NAME.ReadOnly = true;
			}

			// Token: 0x060030EC RID: 12524 RVA: 0x0009D929 File Offset: 0x0009BB29
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			public PlannerSolutionDataSet.SolutionProjectRequirementsByRoleRow NewSolutionProjectRequirementsByRoleRow()
			{
				return (PlannerSolutionDataSet.SolutionProjectRequirementsByRoleRow)base.NewRow();
			}

			// Token: 0x060030ED RID: 12525 RVA: 0x0009D936 File Offset: 0x0009BB36
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			protected override DataRow NewRowFromBuilder(DataRowBuilder builder)
			{
				return new PlannerSolutionDataSet.SolutionProjectRequirementsByRoleRow(builder);
			}

			// Token: 0x060030EE RID: 12526 RVA: 0x0009D93E File Offset: 0x0009BB3E
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			protected override Type GetRowType()
			{
				return typeof(PlannerSolutionDataSet.SolutionProjectRequirementsByRoleRow);
			}

			// Token: 0x060030EF RID: 12527 RVA: 0x0009D94A File Offset: 0x0009BB4A
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			protected override void OnRowChanged(DataRowChangeEventArgs e)
			{
				base.OnRowChanged(e);
				if (this.SolutionProjectRequirementsByRoleRowChanged != null)
				{
					this.SolutionProjectRequirementsByRoleRowChanged(this, new PlannerSolutionDataSet.SolutionProjectRequirementsByRoleRowChangeEvent((PlannerSolutionDataSet.SolutionProjectRequirementsByRoleRow)e.Row, e.Action));
				}
			}

			// Token: 0x060030F0 RID: 12528 RVA: 0x0009D97D File Offset: 0x0009BB7D
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			protected override void OnRowChanging(DataRowChangeEventArgs e)
			{
				base.OnRowChanging(e);
				if (this.SolutionProjectRequirementsByRoleRowChanging != null)
				{
					this.SolutionProjectRequirementsByRoleRowChanging(this, new PlannerSolutionDataSet.SolutionProjectRequirementsByRoleRowChangeEvent((PlannerSolutionDataSet.SolutionProjectRequirementsByRoleRow)e.Row, e.Action));
				}
			}

			// Token: 0x060030F1 RID: 12529 RVA: 0x0009D9B0 File Offset: 0x0009BBB0
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			protected override void OnRowDeleted(DataRowChangeEventArgs e)
			{
				base.OnRowDeleted(e);
				if (this.SolutionProjectRequirementsByRoleRowDeleted != null)
				{
					this.SolutionProjectRequirementsByRoleRowDeleted(this, new PlannerSolutionDataSet.SolutionProjectRequirementsByRoleRowChangeEvent((PlannerSolutionDataSet.SolutionProjectRequirementsByRoleRow)e.Row, e.Action));
				}
			}

			// Token: 0x060030F2 RID: 12530 RVA: 0x0009D9E3 File Offset: 0x0009BBE3
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			protected override void OnRowDeleting(DataRowChangeEventArgs e)
			{
				base.OnRowDeleting(e);
				if (this.SolutionProjectRequirementsByRoleRowDeleting != null)
				{
					this.SolutionProjectRequirementsByRoleRowDeleting(this, new PlannerSolutionDataSet.SolutionProjectRequirementsByRoleRowChangeEvent((PlannerSolutionDataSet.SolutionProjectRequirementsByRoleRow)e.Row, e.Action));
				}
			}

			// Token: 0x060030F3 RID: 12531 RVA: 0x0009DA16 File Offset: 0x0009BC16
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			public void RemoveSolutionProjectRequirementsByRoleRow(PlannerSolutionDataSet.SolutionProjectRequirementsByRoleRow row)
			{
				base.Rows.Remove(row);
			}

			// Token: 0x060030F4 RID: 12532 RVA: 0x0009DA24 File Offset: 0x0009BC24
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public static XmlSchemaComplexType GetTypedTableSchema(XmlSchemaSet xs)
			{
				XmlSchemaComplexType xmlSchemaComplexType = new XmlSchemaComplexType();
				XmlSchemaSequence xmlSchemaSequence = new XmlSchemaSequence();
				PlannerSolutionDataSet plannerSolutionDataSet = new PlannerSolutionDataSet();
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
				xmlSchemaAttribute.FixedValue = plannerSolutionDataSet.Namespace;
				xmlSchemaComplexType.Attributes.Add(xmlSchemaAttribute);
				XmlSchemaAttribute xmlSchemaAttribute2 = new XmlSchemaAttribute();
				xmlSchemaAttribute2.Name = "tableTypeName";
				xmlSchemaAttribute2.FixedValue = "SolutionProjectRequirementsByRoleDataTable";
				xmlSchemaComplexType.Attributes.Add(xmlSchemaAttribute2);
				xmlSchemaComplexType.Particle = xmlSchemaSequence;
				XmlSchema schemaSerializable = plannerSolutionDataSet.GetSchemaSerializable();
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

			// Token: 0x04000A5D RID: 2653
			private DataColumn columnSOLUTION_UID;

			// Token: 0x04000A5E RID: 2654
			private DataColumn columnPROJ_UID;

			// Token: 0x04000A5F RID: 2655
			private DataColumn columnCUSTOM_FIELD_UID;

			// Token: 0x04000A60 RID: 2656
			private DataColumn columnLT_STRUCT_UID;

			// Token: 0x04000A61 RID: 2657
			private DataColumn columnSTART_DATE;

			// Token: 0x04000A62 RID: 2658
			private DataColumn columnADDITIONAL_WORK;

			// Token: 0x04000A63 RID: 2659
			private DataColumn columnREQUIREMENT_UID;

			// Token: 0x04000A64 RID: 2660
			private DataColumn columnROLE_NAME;

			// Token: 0x04000A65 RID: 2661
			private DataColumn columnPROJ_NAME;
		}

		// Token: 0x02000269 RID: 617
		[XmlSchemaProvider("GetTypedTableSchema")]
		[Serializable]
		public class SolutionEfficientFrontierDataTable : DataTable, IEnumerable
		{
			// Token: 0x060030F5 RID: 12533 RVA: 0x0009DC1C File Offset: 0x0009BE1C
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			public SolutionEfficientFrontierDataTable()
			{
				base.TableName = "SolutionEfficientFrontier";
				this.BeginInit();
				this.InitClass();
				this.EndInit();
			}

			// Token: 0x060030F6 RID: 12534 RVA: 0x0009DC44 File Offset: 0x0009BE44
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			internal SolutionEfficientFrontierDataTable(DataTable table)
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

			// Token: 0x060030F7 RID: 12535 RVA: 0x0009DCEC File Offset: 0x0009BEEC
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			protected SolutionEfficientFrontierDataTable(SerializationInfo info, StreamingContext context) : base(info, context)
			{
				this.InitVars();
			}

			// Token: 0x17000E42 RID: 3650
			// (get) Token: 0x060030F8 RID: 12536 RVA: 0x0009DCFC File Offset: 0x0009BEFC
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public DataColumn FRONTIER_UIDColumn
			{
				get
				{
					return this.columnFRONTIER_UID;
				}
			}

			// Token: 0x17000E43 RID: 3651
			// (get) Token: 0x060030F9 RID: 12537 RVA: 0x0009DD04 File Offset: 0x0009BF04
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public DataColumn ANALYSIS_UIDColumn
			{
				get
				{
					return this.columnANALYSIS_UID;
				}
			}

			// Token: 0x17000E44 RID: 3652
			// (get) Token: 0x060030FA RID: 12538 RVA: 0x0009DD0C File Offset: 0x0009BF0C
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			public DataColumn POINT_UIDColumn
			{
				get
				{
					return this.columnPOINT_UID;
				}
			}

			// Token: 0x17000E45 RID: 3653
			// (get) Token: 0x060030FB RID: 12539 RVA: 0x0009DD14 File Offset: 0x0009BF14
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public DataColumn X_VALUEColumn
			{
				get
				{
					return this.columnX_VALUE;
				}
			}

			// Token: 0x17000E46 RID: 3654
			// (get) Token: 0x060030FC RID: 12540 RVA: 0x0009DD1C File Offset: 0x0009BF1C
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			public DataColumn Y_VALUEColumn
			{
				get
				{
					return this.columnY_VALUE;
				}
			}

			// Token: 0x17000E47 RID: 3655
			// (get) Token: 0x060030FD RID: 12541 RVA: 0x0009DD24 File Offset: 0x0009BF24
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

			// Token: 0x17000E48 RID: 3656
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			public PlannerSolutionDataSet.SolutionEfficientFrontierRow this[int index]
			{
				get
				{
					return (PlannerSolutionDataSet.SolutionEfficientFrontierRow)base.Rows[index];
				}
			}

			// Token: 0x14000219 RID: 537
			// (add) Token: 0x060030FF RID: 12543 RVA: 0x0009DD44 File Offset: 0x0009BF44
			// (remove) Token: 0x06003100 RID: 12544 RVA: 0x0009DD7C File Offset: 0x0009BF7C
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public event PlannerSolutionDataSet.SolutionEfficientFrontierRowChangeEventHandler SolutionEfficientFrontierRowChanging;

			// Token: 0x1400021A RID: 538
			// (add) Token: 0x06003101 RID: 12545 RVA: 0x0009DDB4 File Offset: 0x0009BFB4
			// (remove) Token: 0x06003102 RID: 12546 RVA: 0x0009DDEC File Offset: 0x0009BFEC
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public event PlannerSolutionDataSet.SolutionEfficientFrontierRowChangeEventHandler SolutionEfficientFrontierRowChanged;

			// Token: 0x1400021B RID: 539
			// (add) Token: 0x06003103 RID: 12547 RVA: 0x0009DE24 File Offset: 0x0009C024
			// (remove) Token: 0x06003104 RID: 12548 RVA: 0x0009DE5C File Offset: 0x0009C05C
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public event PlannerSolutionDataSet.SolutionEfficientFrontierRowChangeEventHandler SolutionEfficientFrontierRowDeleting;

			// Token: 0x1400021C RID: 540
			// (add) Token: 0x06003105 RID: 12549 RVA: 0x0009DE94 File Offset: 0x0009C094
			// (remove) Token: 0x06003106 RID: 12550 RVA: 0x0009DECC File Offset: 0x0009C0CC
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public event PlannerSolutionDataSet.SolutionEfficientFrontierRowChangeEventHandler SolutionEfficientFrontierRowDeleted;

			// Token: 0x06003107 RID: 12551 RVA: 0x0009DF01 File Offset: 0x0009C101
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			public void AddSolutionEfficientFrontierRow(PlannerSolutionDataSet.SolutionEfficientFrontierRow row)
			{
				base.Rows.Add(row);
			}

			// Token: 0x06003108 RID: 12552 RVA: 0x0009DF10 File Offset: 0x0009C110
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			public PlannerSolutionDataSet.SolutionEfficientFrontierRow AddSolutionEfficientFrontierRow(PlannerSolutionDataSet.SolutionsRow parentSolutionsRowByFK_Solutions_SolutionEfficientFrontier, Guid ANALYSIS_UID, Guid POINT_UID, decimal X_VALUE, decimal Y_VALUE)
			{
				PlannerSolutionDataSet.SolutionEfficientFrontierRow solutionEfficientFrontierRow = (PlannerSolutionDataSet.SolutionEfficientFrontierRow)base.NewRow();
				object[] array = new object[]
				{
					null,
					ANALYSIS_UID,
					POINT_UID,
					X_VALUE,
					Y_VALUE
				};
				if (parentSolutionsRowByFK_Solutions_SolutionEfficientFrontier != null)
				{
					array[0] = parentSolutionsRowByFK_Solutions_SolutionEfficientFrontier[7];
				}
				solutionEfficientFrontierRow.ItemArray = array;
				base.Rows.Add(solutionEfficientFrontierRow);
				return solutionEfficientFrontierRow;
			}

			// Token: 0x06003109 RID: 12553 RVA: 0x0009DF7C File Offset: 0x0009C17C
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			public PlannerSolutionDataSet.SolutionEfficientFrontierRow FindByPOINT_UID(Guid POINT_UID)
			{
				return (PlannerSolutionDataSet.SolutionEfficientFrontierRow)base.Rows.Find(new object[]
				{
					POINT_UID
				});
			}

			// Token: 0x0600310A RID: 12554 RVA: 0x0009DFAA File Offset: 0x0009C1AA
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			public virtual IEnumerator GetEnumerator()
			{
				return base.Rows.GetEnumerator();
			}

			// Token: 0x0600310B RID: 12555 RVA: 0x0009DFB8 File Offset: 0x0009C1B8
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			public override DataTable Clone()
			{
				PlannerSolutionDataSet.SolutionEfficientFrontierDataTable solutionEfficientFrontierDataTable = (PlannerSolutionDataSet.SolutionEfficientFrontierDataTable)base.Clone();
				solutionEfficientFrontierDataTable.InitVars();
				return solutionEfficientFrontierDataTable;
			}

			// Token: 0x0600310C RID: 12556 RVA: 0x0009DFD8 File Offset: 0x0009C1D8
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			protected override DataTable CreateInstance()
			{
				return new PlannerSolutionDataSet.SolutionEfficientFrontierDataTable();
			}

			// Token: 0x0600310D RID: 12557 RVA: 0x0009DFE0 File Offset: 0x0009C1E0
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			internal void InitVars()
			{
				this.columnFRONTIER_UID = base.Columns["FRONTIER_UID"];
				this.columnANALYSIS_UID = base.Columns["ANALYSIS_UID"];
				this.columnPOINT_UID = base.Columns["POINT_UID"];
				this.columnX_VALUE = base.Columns["X_VALUE"];
				this.columnY_VALUE = base.Columns["Y_VALUE"];
			}

			// Token: 0x0600310E RID: 12558 RVA: 0x0009E05C File Offset: 0x0009C25C
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			private void InitClass()
			{
				this.columnFRONTIER_UID = new DataColumn("FRONTIER_UID", typeof(Guid), null, MappingType.Element);
				base.Columns.Add(this.columnFRONTIER_UID);
				this.columnANALYSIS_UID = new DataColumn("ANALYSIS_UID", typeof(Guid), null, MappingType.Element);
				base.Columns.Add(this.columnANALYSIS_UID);
				this.columnPOINT_UID = new DataColumn("POINT_UID", typeof(Guid), null, MappingType.Element);
				base.Columns.Add(this.columnPOINT_UID);
				this.columnX_VALUE = new DataColumn("X_VALUE", typeof(decimal), null, MappingType.Element);
				base.Columns.Add(this.columnX_VALUE);
				this.columnY_VALUE = new DataColumn("Y_VALUE", typeof(decimal), null, MappingType.Element);
				base.Columns.Add(this.columnY_VALUE);
				base.Constraints.Add(new UniqueConstraint("PK_SolutionEfficientFrontier", new DataColumn[]
				{
					this.columnPOINT_UID
				}, true));
				this.columnFRONTIER_UID.AllowDBNull = false;
				this.columnFRONTIER_UID.ReadOnly = true;
				this.columnANALYSIS_UID.AllowDBNull = false;
				this.columnANALYSIS_UID.ReadOnly = true;
				this.columnPOINT_UID.AllowDBNull = false;
				this.columnPOINT_UID.ReadOnly = true;
				this.columnPOINT_UID.Unique = true;
				this.columnX_VALUE.AllowDBNull = false;
				this.columnX_VALUE.ReadOnly = true;
				this.columnY_VALUE.AllowDBNull = false;
				this.columnY_VALUE.ReadOnly = true;
			}

			// Token: 0x0600310F RID: 12559 RVA: 0x0009E1F5 File Offset: 0x0009C3F5
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public PlannerSolutionDataSet.SolutionEfficientFrontierRow NewSolutionEfficientFrontierRow()
			{
				return (PlannerSolutionDataSet.SolutionEfficientFrontierRow)base.NewRow();
			}

			// Token: 0x06003110 RID: 12560 RVA: 0x0009E202 File Offset: 0x0009C402
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			protected override DataRow NewRowFromBuilder(DataRowBuilder builder)
			{
				return new PlannerSolutionDataSet.SolutionEfficientFrontierRow(builder);
			}

			// Token: 0x06003111 RID: 12561 RVA: 0x0009E20A File Offset: 0x0009C40A
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			protected override Type GetRowType()
			{
				return typeof(PlannerSolutionDataSet.SolutionEfficientFrontierRow);
			}

			// Token: 0x06003112 RID: 12562 RVA: 0x0009E216 File Offset: 0x0009C416
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			protected override void OnRowChanged(DataRowChangeEventArgs e)
			{
				base.OnRowChanged(e);
				if (this.SolutionEfficientFrontierRowChanged != null)
				{
					this.SolutionEfficientFrontierRowChanged(this, new PlannerSolutionDataSet.SolutionEfficientFrontierRowChangeEvent((PlannerSolutionDataSet.SolutionEfficientFrontierRow)e.Row, e.Action));
				}
			}

			// Token: 0x06003113 RID: 12563 RVA: 0x0009E249 File Offset: 0x0009C449
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			protected override void OnRowChanging(DataRowChangeEventArgs e)
			{
				base.OnRowChanging(e);
				if (this.SolutionEfficientFrontierRowChanging != null)
				{
					this.SolutionEfficientFrontierRowChanging(this, new PlannerSolutionDataSet.SolutionEfficientFrontierRowChangeEvent((PlannerSolutionDataSet.SolutionEfficientFrontierRow)e.Row, e.Action));
				}
			}

			// Token: 0x06003114 RID: 12564 RVA: 0x0009E27C File Offset: 0x0009C47C
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			protected override void OnRowDeleted(DataRowChangeEventArgs e)
			{
				base.OnRowDeleted(e);
				if (this.SolutionEfficientFrontierRowDeleted != null)
				{
					this.SolutionEfficientFrontierRowDeleted(this, new PlannerSolutionDataSet.SolutionEfficientFrontierRowChangeEvent((PlannerSolutionDataSet.SolutionEfficientFrontierRow)e.Row, e.Action));
				}
			}

			// Token: 0x06003115 RID: 12565 RVA: 0x0009E2AF File Offset: 0x0009C4AF
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			protected override void OnRowDeleting(DataRowChangeEventArgs e)
			{
				base.OnRowDeleting(e);
				if (this.SolutionEfficientFrontierRowDeleting != null)
				{
					this.SolutionEfficientFrontierRowDeleting(this, new PlannerSolutionDataSet.SolutionEfficientFrontierRowChangeEvent((PlannerSolutionDataSet.SolutionEfficientFrontierRow)e.Row, e.Action));
				}
			}

			// Token: 0x06003116 RID: 12566 RVA: 0x0009E2E2 File Offset: 0x0009C4E2
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			public void RemoveSolutionEfficientFrontierRow(PlannerSolutionDataSet.SolutionEfficientFrontierRow row)
			{
				base.Rows.Remove(row);
			}

			// Token: 0x06003117 RID: 12567 RVA: 0x0009E2F0 File Offset: 0x0009C4F0
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			public static XmlSchemaComplexType GetTypedTableSchema(XmlSchemaSet xs)
			{
				XmlSchemaComplexType xmlSchemaComplexType = new XmlSchemaComplexType();
				XmlSchemaSequence xmlSchemaSequence = new XmlSchemaSequence();
				PlannerSolutionDataSet plannerSolutionDataSet = new PlannerSolutionDataSet();
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
				xmlSchemaAttribute.FixedValue = plannerSolutionDataSet.Namespace;
				xmlSchemaComplexType.Attributes.Add(xmlSchemaAttribute);
				XmlSchemaAttribute xmlSchemaAttribute2 = new XmlSchemaAttribute();
				xmlSchemaAttribute2.Name = "tableTypeName";
				xmlSchemaAttribute2.FixedValue = "SolutionEfficientFrontierDataTable";
				xmlSchemaComplexType.Attributes.Add(xmlSchemaAttribute2);
				xmlSchemaComplexType.Particle = xmlSchemaSequence;
				XmlSchema schemaSerializable = plannerSolutionDataSet.GetSchemaSerializable();
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

			// Token: 0x04000A6A RID: 2666
			private DataColumn columnFRONTIER_UID;

			// Token: 0x04000A6B RID: 2667
			private DataColumn columnANALYSIS_UID;

			// Token: 0x04000A6C RID: 2668
			private DataColumn columnPOINT_UID;

			// Token: 0x04000A6D RID: 2669
			private DataColumn columnX_VALUE;

			// Token: 0x04000A6E RID: 2670
			private DataColumn columnY_VALUE;
		}

		// Token: 0x0200026A RID: 618
		public class SolutionsRow : DataRow
		{
			// Token: 0x06003118 RID: 12568 RVA: 0x0009E4E8 File Offset: 0x0009C6E8
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			internal SolutionsRow(DataRowBuilder rb) : base(rb)
			{
				this.tableSolutions = (PlannerSolutionDataSet.SolutionsDataTable)base.Table;
			}

			// Token: 0x17000E49 RID: 3657
			// (get) Token: 0x06003119 RID: 12569 RVA: 0x0009E502 File Offset: 0x0009C702
			// (set) Token: 0x0600311A RID: 12570 RVA: 0x0009E51A File Offset: 0x0009C71A
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public Guid SOLUTION_UID
			{
				get
				{
					return (Guid)base[this.tableSolutions.SOLUTION_UIDColumn];
				}
				set
				{
					base[this.tableSolutions.SOLUTION_UIDColumn] = value;
				}
			}

			// Token: 0x17000E4A RID: 3658
			// (get) Token: 0x0600311B RID: 12571 RVA: 0x0009E533 File Offset: 0x0009C733
			// (set) Token: 0x0600311C RID: 12572 RVA: 0x0009E54B File Offset: 0x0009C74B
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			public Guid OPTIMIZER_SOLUTION_UID
			{
				get
				{
					return (Guid)base[this.tableSolutions.OPTIMIZER_SOLUTION_UIDColumn];
				}
				set
				{
					base[this.tableSolutions.OPTIMIZER_SOLUTION_UIDColumn] = value;
				}
			}

			// Token: 0x17000E4B RID: 3659
			// (get) Token: 0x0600311D RID: 12573 RVA: 0x0009E564 File Offset: 0x0009C764
			// (set) Token: 0x0600311E RID: 12574 RVA: 0x0009E57C File Offset: 0x0009C77C
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public Guid ANALYSIS_UID
			{
				get
				{
					return (Guid)base[this.tableSolutions.ANALYSIS_UIDColumn];
				}
				set
				{
					base[this.tableSolutions.ANALYSIS_UIDColumn] = value;
				}
			}

			// Token: 0x17000E4C RID: 3660
			// (get) Token: 0x0600311F RID: 12575 RVA: 0x0009E595 File Offset: 0x0009C795
			// (set) Token: 0x06003120 RID: 12576 RVA: 0x0009E5AD File Offset: 0x0009C7AD
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public string SOLUTION_NAME
			{
				get
				{
					return (string)base[this.tableSolutions.SOLUTION_NAMEColumn];
				}
				set
				{
					base[this.tableSolutions.SOLUTION_NAMEColumn] = value;
				}
			}

			// Token: 0x17000E4D RID: 3661
			// (get) Token: 0x06003121 RID: 12577 RVA: 0x0009E5C4 File Offset: 0x0009C7C4
			// (set) Token: 0x06003122 RID: 12578 RVA: 0x0009E608 File Offset: 0x0009C808
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public string SOLUTION_DESCRIPTION
			{
				get
				{
					string result;
					try
					{
						result = (string)base[this.tableSolutions.SOLUTION_DESCRIPTIONColumn];
					}
					catch (InvalidCastException innerException)
					{
						throw new StrongTypingException("The value for column 'SOLUTION_DESCRIPTION' in table 'Solutions' is DBNull.", innerException);
					}
					return result;
				}
				set
				{
					base[this.tableSolutions.SOLUTION_DESCRIPTIONColumn] = value;
				}
			}

			// Token: 0x17000E4E RID: 3662
			// (get) Token: 0x06003123 RID: 12579 RVA: 0x0009E61C File Offset: 0x0009C81C
			// (set) Token: 0x06003124 RID: 12580 RVA: 0x0009E634 File Offset: 0x0009C834
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			public byte CONSTRAINT_TYPE
			{
				get
				{
					return (byte)base[this.tableSolutions.CONSTRAINT_TYPEColumn];
				}
				set
				{
					base[this.tableSolutions.CONSTRAINT_TYPEColumn] = value;
				}
			}

			// Token: 0x17000E4F RID: 3663
			// (get) Token: 0x06003125 RID: 12581 RVA: 0x0009E64D File Offset: 0x0009C84D
			// (set) Token: 0x06003126 RID: 12582 RVA: 0x0009E665 File Offset: 0x0009C865
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public decimal CONSTRAINT_VALUE
			{
				get
				{
					return (decimal)base[this.tableSolutions.CONSTRAINT_VALUEColumn];
				}
				set
				{
					base[this.tableSolutions.CONSTRAINT_VALUEColumn] = value;
				}
			}

			// Token: 0x17000E50 RID: 3664
			// (get) Token: 0x06003127 RID: 12583 RVA: 0x0009E680 File Offset: 0x0009C880
			// (set) Token: 0x06003128 RID: 12584 RVA: 0x0009E6C4 File Offset: 0x0009C8C4
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			public Guid FRONTIER_UID
			{
				get
				{
					Guid result;
					try
					{
						result = (Guid)base[this.tableSolutions.FRONTIER_UIDColumn];
					}
					catch (InvalidCastException innerException)
					{
						throw new StrongTypingException("The value for column 'FRONTIER_UID' in table 'Solutions' is DBNull.", innerException);
					}
					return result;
				}
				set
				{
					base[this.tableSolutions.FRONTIER_UIDColumn] = value;
				}
			}

			// Token: 0x17000E51 RID: 3665
			// (get) Token: 0x06003129 RID: 12585 RVA: 0x0009E6E0 File Offset: 0x0009C8E0
			// (set) Token: 0x0600312A RID: 12586 RVA: 0x0009E724 File Offset: 0x0009C924
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			public DateTime MOD_DATE
			{
				get
				{
					DateTime result;
					try
					{
						result = (DateTime)base[this.tableSolutions.MOD_DATEColumn];
					}
					catch (InvalidCastException innerException)
					{
						throw new StrongTypingException("The value for column 'MOD_DATE' in table 'Solutions' is DBNull.", innerException);
					}
					return result;
				}
				set
				{
					base[this.tableSolutions.MOD_DATEColumn] = value;
				}
			}

			// Token: 0x17000E52 RID: 3666
			// (get) Token: 0x0600312B RID: 12587 RVA: 0x0009E740 File Offset: 0x0009C940
			// (set) Token: 0x0600312C RID: 12588 RVA: 0x0009E784 File Offset: 0x0009C984
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public DateTime CREATED_DATE
			{
				get
				{
					DateTime result;
					try
					{
						result = (DateTime)base[this.tableSolutions.CREATED_DATEColumn];
					}
					catch (InvalidCastException innerException)
					{
						throw new StrongTypingException("The value for column 'CREATED_DATE' in table 'Solutions' is DBNull.", innerException);
					}
					return result;
				}
				set
				{
					base[this.tableSolutions.CREATED_DATEColumn] = value;
				}
			}

			// Token: 0x17000E53 RID: 3667
			// (get) Token: 0x0600312D RID: 12589 RVA: 0x0009E7A0 File Offset: 0x0009C9A0
			// (set) Token: 0x0600312E RID: 12590 RVA: 0x0009E7E4 File Offset: 0x0009C9E4
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public Guid CREATED_BY_RES_UID
			{
				get
				{
					Guid result;
					try
					{
						result = (Guid)base[this.tableSolutions.CREATED_BY_RES_UIDColumn];
					}
					catch (InvalidCastException innerException)
					{
						throw new StrongTypingException("The value for column 'CREATED_BY_RES_UID' in table 'Solutions' is DBNull.", innerException);
					}
					return result;
				}
				set
				{
					base[this.tableSolutions.CREATED_BY_RES_UIDColumn] = value;
				}
			}

			// Token: 0x17000E54 RID: 3668
			// (get) Token: 0x0600312F RID: 12591 RVA: 0x0009E800 File Offset: 0x0009CA00
			// (set) Token: 0x06003130 RID: 12592 RVA: 0x0009E844 File Offset: 0x0009CA44
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public Guid LAST_UPDATED_BY_RES_UID
			{
				get
				{
					Guid result;
					try
					{
						result = (Guid)base[this.tableSolutions.LAST_UPDATED_BY_RES_UIDColumn];
					}
					catch (InvalidCastException innerException)
					{
						throw new StrongTypingException("The value for column 'LAST_UPDATED_BY_RES_UID' in table 'Solutions' is DBNull.", innerException);
					}
					return result;
				}
				set
				{
					base[this.tableSolutions.LAST_UPDATED_BY_RES_UIDColumn] = value;
				}
			}

			// Token: 0x17000E55 RID: 3669
			// (get) Token: 0x06003131 RID: 12593 RVA: 0x0009E860 File Offset: 0x0009CA60
			// (set) Token: 0x06003132 RID: 12594 RVA: 0x0009E8A4 File Offset: 0x0009CAA4
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			public string CREATED_BY_RES_NAME
			{
				get
				{
					string result;
					try
					{
						result = (string)base[this.tableSolutions.CREATED_BY_RES_NAMEColumn];
					}
					catch (InvalidCastException innerException)
					{
						throw new StrongTypingException("The value for column 'CREATED_BY_RES_NAME' in table 'Solutions' is DBNull.", innerException);
					}
					return result;
				}
				set
				{
					base[this.tableSolutions.CREATED_BY_RES_NAMEColumn] = value;
				}
			}

			// Token: 0x17000E56 RID: 3670
			// (get) Token: 0x06003133 RID: 12595 RVA: 0x0009E8B8 File Offset: 0x0009CAB8
			// (set) Token: 0x06003134 RID: 12596 RVA: 0x0009E8FC File Offset: 0x0009CAFC
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			public string LAST_UPDATED_BY_RES_NAME
			{
				get
				{
					string result;
					try
					{
						result = (string)base[this.tableSolutions.LAST_UPDATED_BY_RES_NAMEColumn];
					}
					catch (InvalidCastException innerException)
					{
						throw new StrongTypingException("The value for column 'LAST_UPDATED_BY_RES_NAME' in table 'Solutions' is DBNull.", innerException);
					}
					return result;
				}
				set
				{
					base[this.tableSolutions.LAST_UPDATED_BY_RES_NAMEColumn] = value;
				}
			}

			// Token: 0x17000E57 RID: 3671
			// (get) Token: 0x06003135 RID: 12597 RVA: 0x0009E910 File Offset: 0x0009CB10
			// (set) Token: 0x06003136 RID: 12598 RVA: 0x0009E928 File Offset: 0x0009CB28
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public byte HIRING_TYPE
			{
				get
				{
					return (byte)base[this.tableSolutions.HIRING_TYPEColumn];
				}
				set
				{
					base[this.tableSolutions.HIRING_TYPEColumn] = value;
				}
			}

			// Token: 0x17000E58 RID: 3672
			// (get) Token: 0x06003137 RID: 12599 RVA: 0x0009E941 File Offset: 0x0009CB41
			// (set) Token: 0x06003138 RID: 12600 RVA: 0x0009E959 File Offset: 0x0009CB59
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public bool OPT_ENF_SCHEDULING_CONS
			{
				get
				{
					return (bool)base[this.tableSolutions.OPT_ENF_SCHEDULING_CONSColumn];
				}
				set
				{
					base[this.tableSolutions.OPT_ENF_SCHEDULING_CONSColumn] = value;
				}
			}

			// Token: 0x17000E59 RID: 3673
			// (get) Token: 0x06003139 RID: 12601 RVA: 0x0009E972 File Offset: 0x0009CB72
			// (set) Token: 0x0600313A RID: 12602 RVA: 0x0009E98A File Offset: 0x0009CB8A
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public bool OPT_ENF_PROJ_DEP
			{
				get
				{
					return (bool)base[this.tableSolutions.OPT_ENF_PROJ_DEPColumn];
				}
				set
				{
					base[this.tableSolutions.OPT_ENF_PROJ_DEPColumn] = value;
				}
			}

			// Token: 0x17000E5A RID: 3674
			// (get) Token: 0x0600313B RID: 12603 RVA: 0x0009E9A3 File Offset: 0x0009CBA3
			// (set) Token: 0x0600313C RID: 12604 RVA: 0x0009E9BB File Offset: 0x0009CBBB
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public byte RATE_TABLE
			{
				get
				{
					return (byte)base[this.tableSolutions.RATE_TABLEColumn];
				}
				set
				{
					base[this.tableSolutions.RATE_TABLEColumn] = value;
				}
			}

			// Token: 0x17000E5B RID: 3675
			// (get) Token: 0x0600313D RID: 12605 RVA: 0x0009E9D4 File Offset: 0x0009CBD4
			// (set) Token: 0x0600313E RID: 12606 RVA: 0x0009E9EC File Offset: 0x0009CBEC
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			public double ALLOCATION_THRESHOLD
			{
				get
				{
					return (double)base[this.tableSolutions.ALLOCATION_THRESHOLDColumn];
				}
				set
				{
					base[this.tableSolutions.ALLOCATION_THRESHOLDColumn] = value;
				}
			}

			// Token: 0x17000E5C RID: 3676
			// (get) Token: 0x0600313F RID: 12607 RVA: 0x0009EA08 File Offset: 0x0009CC08
			// (set) Token: 0x06003140 RID: 12608 RVA: 0x0009EA4C File Offset: 0x0009CC4C
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public string ANALYSIS_NAME
			{
				get
				{
					string result;
					try
					{
						result = (string)base[this.tableSolutions.ANALYSIS_NAMEColumn];
					}
					catch (InvalidCastException innerException)
					{
						throw new StrongTypingException("The value for column 'ANALYSIS_NAME' in table 'Solutions' is DBNull.", innerException);
					}
					return result;
				}
				set
				{
					base[this.tableSolutions.ANALYSIS_NAMEColumn] = value;
				}
			}

			// Token: 0x17000E5D RID: 3677
			// (get) Token: 0x06003141 RID: 12609 RVA: 0x0009EA60 File Offset: 0x0009CC60
			// (set) Token: 0x06003142 RID: 12610 RVA: 0x0009EAA4 File Offset: 0x0009CCA4
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public string OPTIMIZER_SOLUTION_NAME
			{
				get
				{
					string result;
					try
					{
						result = (string)base[this.tableSolutions.OPTIMIZER_SOLUTION_NAMEColumn];
					}
					catch (InvalidCastException innerException)
					{
						throw new StrongTypingException("The value for column 'OPTIMIZER_SOLUTION_NAME' in table 'Solutions' is DBNull.", innerException);
					}
					return result;
				}
				set
				{
					base[this.tableSolutions.OPTIMIZER_SOLUTION_NAMEColumn] = value;
				}
			}

			// Token: 0x17000E5E RID: 3678
			// (get) Token: 0x06003143 RID: 12611 RVA: 0x0009EAB8 File Offset: 0x0009CCB8
			// (set) Token: 0x06003144 RID: 12612 RVA: 0x0009EAFC File Offset: 0x0009CCFC
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			public decimal TOTAL_PRIORITY_VALUE
			{
				get
				{
					decimal result;
					try
					{
						result = (decimal)base[this.tableSolutions.TOTAL_PRIORITY_VALUEColumn];
					}
					catch (InvalidCastException innerException)
					{
						throw new StrongTypingException("The value for column 'TOTAL_PRIORITY_VALUE' in table 'Solutions' is DBNull.", innerException);
					}
					return result;
				}
				set
				{
					base[this.tableSolutions.TOTAL_PRIORITY_VALUEColumn] = value;
				}
			}

			// Token: 0x06003145 RID: 12613 RVA: 0x0009EB15 File Offset: 0x0009CD15
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			public bool IsSOLUTION_DESCRIPTIONNull()
			{
				return base.IsNull(this.tableSolutions.SOLUTION_DESCRIPTIONColumn);
			}

			// Token: 0x06003146 RID: 12614 RVA: 0x0009EB28 File Offset: 0x0009CD28
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public void SetSOLUTION_DESCRIPTIONNull()
			{
				base[this.tableSolutions.SOLUTION_DESCRIPTIONColumn] = Convert.DBNull;
			}

			// Token: 0x06003147 RID: 12615 RVA: 0x0009EB40 File Offset: 0x0009CD40
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public bool IsFRONTIER_UIDNull()
			{
				return base.IsNull(this.tableSolutions.FRONTIER_UIDColumn);
			}

			// Token: 0x06003148 RID: 12616 RVA: 0x0009EB53 File Offset: 0x0009CD53
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public void SetFRONTIER_UIDNull()
			{
				base[this.tableSolutions.FRONTIER_UIDColumn] = Convert.DBNull;
			}

			// Token: 0x06003149 RID: 12617 RVA: 0x0009EB6B File Offset: 0x0009CD6B
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public bool IsMOD_DATENull()
			{
				return base.IsNull(this.tableSolutions.MOD_DATEColumn);
			}

			// Token: 0x0600314A RID: 12618 RVA: 0x0009EB7E File Offset: 0x0009CD7E
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			public void SetMOD_DATENull()
			{
				base[this.tableSolutions.MOD_DATEColumn] = Convert.DBNull;
			}

			// Token: 0x0600314B RID: 12619 RVA: 0x0009EB96 File Offset: 0x0009CD96
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			public bool IsCREATED_DATENull()
			{
				return base.IsNull(this.tableSolutions.CREATED_DATEColumn);
			}

			// Token: 0x0600314C RID: 12620 RVA: 0x0009EBA9 File Offset: 0x0009CDA9
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public void SetCREATED_DATENull()
			{
				base[this.tableSolutions.CREATED_DATEColumn] = Convert.DBNull;
			}

			// Token: 0x0600314D RID: 12621 RVA: 0x0009EBC1 File Offset: 0x0009CDC1
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			public bool IsCREATED_BY_RES_UIDNull()
			{
				return base.IsNull(this.tableSolutions.CREATED_BY_RES_UIDColumn);
			}

			// Token: 0x0600314E RID: 12622 RVA: 0x0009EBD4 File Offset: 0x0009CDD4
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			public void SetCREATED_BY_RES_UIDNull()
			{
				base[this.tableSolutions.CREATED_BY_RES_UIDColumn] = Convert.DBNull;
			}

			// Token: 0x0600314F RID: 12623 RVA: 0x0009EBEC File Offset: 0x0009CDEC
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			public bool IsLAST_UPDATED_BY_RES_UIDNull()
			{
				return base.IsNull(this.tableSolutions.LAST_UPDATED_BY_RES_UIDColumn);
			}

			// Token: 0x06003150 RID: 12624 RVA: 0x0009EBFF File Offset: 0x0009CDFF
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public void SetLAST_UPDATED_BY_RES_UIDNull()
			{
				base[this.tableSolutions.LAST_UPDATED_BY_RES_UIDColumn] = Convert.DBNull;
			}

			// Token: 0x06003151 RID: 12625 RVA: 0x0009EC17 File Offset: 0x0009CE17
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			public bool IsCREATED_BY_RES_NAMENull()
			{
				return base.IsNull(this.tableSolutions.CREATED_BY_RES_NAMEColumn);
			}

			// Token: 0x06003152 RID: 12626 RVA: 0x0009EC2A File Offset: 0x0009CE2A
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public void SetCREATED_BY_RES_NAMENull()
			{
				base[this.tableSolutions.CREATED_BY_RES_NAMEColumn] = Convert.DBNull;
			}

			// Token: 0x06003153 RID: 12627 RVA: 0x0009EC42 File Offset: 0x0009CE42
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			public bool IsLAST_UPDATED_BY_RES_NAMENull()
			{
				return base.IsNull(this.tableSolutions.LAST_UPDATED_BY_RES_NAMEColumn);
			}

			// Token: 0x06003154 RID: 12628 RVA: 0x0009EC55 File Offset: 0x0009CE55
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public void SetLAST_UPDATED_BY_RES_NAMENull()
			{
				base[this.tableSolutions.LAST_UPDATED_BY_RES_NAMEColumn] = Convert.DBNull;
			}

			// Token: 0x06003155 RID: 12629 RVA: 0x0009EC6D File Offset: 0x0009CE6D
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			public bool IsANALYSIS_NAMENull()
			{
				return base.IsNull(this.tableSolutions.ANALYSIS_NAMEColumn);
			}

			// Token: 0x06003156 RID: 12630 RVA: 0x0009EC80 File Offset: 0x0009CE80
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public void SetANALYSIS_NAMENull()
			{
				base[this.tableSolutions.ANALYSIS_NAMEColumn] = Convert.DBNull;
			}

			// Token: 0x06003157 RID: 12631 RVA: 0x0009EC98 File Offset: 0x0009CE98
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			public bool IsOPTIMIZER_SOLUTION_NAMENull()
			{
				return base.IsNull(this.tableSolutions.OPTIMIZER_SOLUTION_NAMEColumn);
			}

			// Token: 0x06003158 RID: 12632 RVA: 0x0009ECAB File Offset: 0x0009CEAB
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			public void SetOPTIMIZER_SOLUTION_NAMENull()
			{
				base[this.tableSolutions.OPTIMIZER_SOLUTION_NAMEColumn] = Convert.DBNull;
			}

			// Token: 0x06003159 RID: 12633 RVA: 0x0009ECC3 File Offset: 0x0009CEC3
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			public bool IsTOTAL_PRIORITY_VALUENull()
			{
				return base.IsNull(this.tableSolutions.TOTAL_PRIORITY_VALUEColumn);
			}

			// Token: 0x0600315A RID: 12634 RVA: 0x0009ECD6 File Offset: 0x0009CED6
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			public void SetTOTAL_PRIORITY_VALUENull()
			{
				base[this.tableSolutions.TOTAL_PRIORITY_VALUEColumn] = Convert.DBNull;
			}

			// Token: 0x0600315B RID: 12635 RVA: 0x0009ECEE File Offset: 0x0009CEEE
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			public PlannerSolutionDataSet.SolutionProjectRequirementsByRoleRow[] GetSolutionProjectRequirementsByRoleRows()
			{
				if (base.Table.ChildRelations["FK_Solutions_SolutionProjectRequirementsByRole"] == null)
				{
					return new PlannerSolutionDataSet.SolutionProjectRequirementsByRoleRow[0];
				}
				return (PlannerSolutionDataSet.SolutionProjectRequirementsByRoleRow[])base.GetChildRows(base.Table.ChildRelations["FK_Solutions_SolutionProjectRequirementsByRole"]);
			}

			// Token: 0x0600315C RID: 12636 RVA: 0x0009ED2E File Offset: 0x0009CF2E
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			public PlannerSolutionDataSet.SolutionProjectsRow[] GetSolutionProjectsRows()
			{
				if (base.Table.ChildRelations["FK_Solutions_SolutionProjects"] == null)
				{
					return new PlannerSolutionDataSet.SolutionProjectsRow[0];
				}
				return (PlannerSolutionDataSet.SolutionProjectsRow[])base.GetChildRows(base.Table.ChildRelations["FK_Solutions_SolutionProjects"]);
			}

			// Token: 0x0600315D RID: 12637 RVA: 0x0009ED6E File Offset: 0x0009CF6E
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			public PlannerSolutionDataSet.SolutionResHiredRow[] GetSolutionResHiredRows()
			{
				if (base.Table.ChildRelations["FK_Solutions_SolutionResHired"] == null)
				{
					return new PlannerSolutionDataSet.SolutionResHiredRow[0];
				}
				return (PlannerSolutionDataSet.SolutionResHiredRow[])base.GetChildRows(base.Table.ChildRelations["FK_Solutions_SolutionResHired"]);
			}

			// Token: 0x0600315E RID: 12638 RVA: 0x0009EDAE File Offset: 0x0009CFAE
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public PlannerSolutionDataSet.SolutionEfficientFrontierRow[] GetSolutionEfficientFrontierRows()
			{
				if (base.Table.ChildRelations["FK_Solutions_SolutionEfficientFrontier"] == null)
				{
					return new PlannerSolutionDataSet.SolutionEfficientFrontierRow[0];
				}
				return (PlannerSolutionDataSet.SolutionEfficientFrontierRow[])base.GetChildRows(base.Table.ChildRelations["FK_Solutions_SolutionEfficientFrontier"]);
			}

			// Token: 0x04000A73 RID: 2675
			private PlannerSolutionDataSet.SolutionsDataTable tableSolutions;
		}

		// Token: 0x0200026B RID: 619
		public class SolutionResHiredRow : DataRow
		{
			// Token: 0x0600315F RID: 12639 RVA: 0x0009EDEE File Offset: 0x0009CFEE
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			internal SolutionResHiredRow(DataRowBuilder rb) : base(rb)
			{
				this.tableSolutionResHired = (PlannerSolutionDataSet.SolutionResHiredDataTable)base.Table;
			}

			// Token: 0x17000E5F RID: 3679
			// (get) Token: 0x06003160 RID: 12640 RVA: 0x0009EE08 File Offset: 0x0009D008
			// (set) Token: 0x06003161 RID: 12641 RVA: 0x0009EE20 File Offset: 0x0009D020
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			public Guid SOLUTION_UID
			{
				get
				{
					return (Guid)base[this.tableSolutionResHired.SOLUTION_UIDColumn];
				}
				set
				{
					base[this.tableSolutionResHired.SOLUTION_UIDColumn] = value;
				}
			}

			// Token: 0x17000E60 RID: 3680
			// (get) Token: 0x06003162 RID: 12642 RVA: 0x0009EE39 File Offset: 0x0009D039
			// (set) Token: 0x06003163 RID: 12643 RVA: 0x0009EE51 File Offset: 0x0009D051
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public Guid LT_STRUCT_UID
			{
				get
				{
					return (Guid)base[this.tableSolutionResHired.LT_STRUCT_UIDColumn];
				}
				set
				{
					base[this.tableSolutionResHired.LT_STRUCT_UIDColumn] = value;
				}
			}

			// Token: 0x17000E61 RID: 3681
			// (get) Token: 0x06003164 RID: 12644 RVA: 0x0009EE6A File Offset: 0x0009D06A
			// (set) Token: 0x06003165 RID: 12645 RVA: 0x0009EE82 File Offset: 0x0009D082
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public Guid CUSTOM_FIELD_UID
			{
				get
				{
					return (Guid)base[this.tableSolutionResHired.CUSTOM_FIELD_UIDColumn];
				}
				set
				{
					base[this.tableSolutionResHired.CUSTOM_FIELD_UIDColumn] = value;
				}
			}

			// Token: 0x17000E62 RID: 3682
			// (get) Token: 0x06003166 RID: 12646 RVA: 0x0009EE9B File Offset: 0x0009D09B
			// (set) Token: 0x06003167 RID: 12647 RVA: 0x0009EEB3 File Offset: 0x0009D0B3
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public Guid PROJ_UID
			{
				get
				{
					return (Guid)base[this.tableSolutionResHired.PROJ_UIDColumn];
				}
				set
				{
					base[this.tableSolutionResHired.PROJ_UIDColumn] = value;
				}
			}

			// Token: 0x17000E63 RID: 3683
			// (get) Token: 0x06003168 RID: 12648 RVA: 0x0009EECC File Offset: 0x0009D0CC
			// (set) Token: 0x06003169 RID: 12649 RVA: 0x0009EEE4 File Offset: 0x0009D0E4
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public DateTime START_DATE
			{
				get
				{
					return (DateTime)base[this.tableSolutionResHired.START_DATEColumn];
				}
				set
				{
					base[this.tableSolutionResHired.START_DATEColumn] = value;
				}
			}

			// Token: 0x17000E64 RID: 3684
			// (get) Token: 0x0600316A RID: 12650 RVA: 0x0009EEFD File Offset: 0x0009D0FD
			// (set) Token: 0x0600316B RID: 12651 RVA: 0x0009EF15 File Offset: 0x0009D115
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public int DURATION
			{
				get
				{
					return (int)base[this.tableSolutionResHired.DURATIONColumn];
				}
				set
				{
					base[this.tableSolutionResHired.DURATIONColumn] = value;
				}
			}

			// Token: 0x17000E65 RID: 3685
			// (get) Token: 0x0600316C RID: 12652 RVA: 0x0009EF2E File Offset: 0x0009D12E
			// (set) Token: 0x0600316D RID: 12653 RVA: 0x0009EF46 File Offset: 0x0009D146
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public decimal RESOURCE_COST
			{
				get
				{
					return (decimal)base[this.tableSolutionResHired.RESOURCE_COSTColumn];
				}
				set
				{
					base[this.tableSolutionResHired.RESOURCE_COSTColumn] = value;
				}
			}

			// Token: 0x17000E66 RID: 3686
			// (get) Token: 0x0600316E RID: 12654 RVA: 0x0009EF5F File Offset: 0x0009D15F
			// (set) Token: 0x0600316F RID: 12655 RVA: 0x0009EF77 File Offset: 0x0009D177
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public decimal RESOURCE_WORK
			{
				get
				{
					return (decimal)base[this.tableSolutionResHired.RESOURCE_WORKColumn];
				}
				set
				{
					base[this.tableSolutionResHired.RESOURCE_WORKColumn] = value;
				}
			}

			// Token: 0x17000E67 RID: 3687
			// (get) Token: 0x06003170 RID: 12656 RVA: 0x0009EF90 File Offset: 0x0009D190
			// (set) Token: 0x06003171 RID: 12657 RVA: 0x0009EFA8 File Offset: 0x0009D1A8
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			public Guid RES_HIRE_UID
			{
				get
				{
					return (Guid)base[this.tableSolutionResHired.RES_HIRE_UIDColumn];
				}
				set
				{
					base[this.tableSolutionResHired.RES_HIRE_UIDColumn] = value;
				}
			}

			// Token: 0x17000E68 RID: 3688
			// (get) Token: 0x06003172 RID: 12658 RVA: 0x0009EFC4 File Offset: 0x0009D1C4
			// (set) Token: 0x06003173 RID: 12659 RVA: 0x0009F008 File Offset: 0x0009D208
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public string ROLE_NAME
			{
				get
				{
					string result;
					try
					{
						result = (string)base[this.tableSolutionResHired.ROLE_NAMEColumn];
					}
					catch (InvalidCastException innerException)
					{
						throw new StrongTypingException("The value for column 'ROLE_NAME' in table 'SolutionResHired' is DBNull.", innerException);
					}
					return result;
				}
				set
				{
					base[this.tableSolutionResHired.ROLE_NAMEColumn] = value;
				}
			}

			// Token: 0x17000E69 RID: 3689
			// (get) Token: 0x06003174 RID: 12660 RVA: 0x0009F01C File Offset: 0x0009D21C
			// (set) Token: 0x06003175 RID: 12661 RVA: 0x0009F060 File Offset: 0x0009D260
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public string PROJ_NAME
			{
				get
				{
					string result;
					try
					{
						result = (string)base[this.tableSolutionResHired.PROJ_NAMEColumn];
					}
					catch (InvalidCastException innerException)
					{
						throw new StrongTypingException("The value for column 'PROJ_NAME' in table 'SolutionResHired' is DBNull.", innerException);
					}
					return result;
				}
				set
				{
					base[this.tableSolutionResHired.PROJ_NAMEColumn] = value;
				}
			}

			// Token: 0x17000E6A RID: 3690
			// (get) Token: 0x06003176 RID: 12662 RVA: 0x0009F074 File Offset: 0x0009D274
			// (set) Token: 0x06003177 RID: 12663 RVA: 0x0009F096 File Offset: 0x0009D296
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public PlannerSolutionDataSet.SolutionsRow SolutionsRow
			{
				get
				{
					return (PlannerSolutionDataSet.SolutionsRow)base.GetParentRow(base.Table.ParentRelations["FK_Solutions_SolutionResHired"]);
				}
				set
				{
					base.SetParentRow(value, base.Table.ParentRelations["FK_Solutions_SolutionResHired"]);
				}
			}

			// Token: 0x06003178 RID: 12664 RVA: 0x0009F0B4 File Offset: 0x0009D2B4
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			public bool IsROLE_NAMENull()
			{
				return base.IsNull(this.tableSolutionResHired.ROLE_NAMEColumn);
			}

			// Token: 0x06003179 RID: 12665 RVA: 0x0009F0C7 File Offset: 0x0009D2C7
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public void SetROLE_NAMENull()
			{
				base[this.tableSolutionResHired.ROLE_NAMEColumn] = Convert.DBNull;
			}

			// Token: 0x0600317A RID: 12666 RVA: 0x0009F0DF File Offset: 0x0009D2DF
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public bool IsPROJ_NAMENull()
			{
				return base.IsNull(this.tableSolutionResHired.PROJ_NAMEColumn);
			}

			// Token: 0x0600317B RID: 12667 RVA: 0x0009F0F2 File Offset: 0x0009D2F2
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			public void SetPROJ_NAMENull()
			{
				base[this.tableSolutionResHired.PROJ_NAMEColumn] = Convert.DBNull;
			}

			// Token: 0x04000A74 RID: 2676
			private PlannerSolutionDataSet.SolutionResHiredDataTable tableSolutionResHired;
		}

		// Token: 0x0200026C RID: 620
		public class SolutionProjectsRow : DataRow
		{
			// Token: 0x0600317C RID: 12668 RVA: 0x0009F10A File Offset: 0x0009D30A
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			internal SolutionProjectsRow(DataRowBuilder rb) : base(rb)
			{
				this.tableSolutionProjects = (PlannerSolutionDataSet.SolutionProjectsDataTable)base.Table;
			}

			// Token: 0x17000E6B RID: 3691
			// (get) Token: 0x0600317D RID: 12669 RVA: 0x0009F124 File Offset: 0x0009D324
			// (set) Token: 0x0600317E RID: 12670 RVA: 0x0009F13C File Offset: 0x0009D33C
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			public Guid SOLUTION_UID
			{
				get
				{
					return (Guid)base[this.tableSolutionProjects.SOLUTION_UIDColumn];
				}
				set
				{
					base[this.tableSolutionProjects.SOLUTION_UIDColumn] = value;
				}
			}

			// Token: 0x17000E6C RID: 3692
			// (get) Token: 0x0600317F RID: 12671 RVA: 0x0009F155 File Offset: 0x0009D355
			// (set) Token: 0x06003180 RID: 12672 RVA: 0x0009F16D File Offset: 0x0009D36D
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public Guid PROJ_UID
			{
				get
				{
					return (Guid)base[this.tableSolutionProjects.PROJ_UIDColumn];
				}
				set
				{
					base[this.tableSolutionProjects.PROJ_UIDColumn] = value;
				}
			}

			// Token: 0x17000E6D RID: 3693
			// (get) Token: 0x06003181 RID: 12673 RVA: 0x0009F186 File Offset: 0x0009D386
			// (set) Token: 0x06003182 RID: 12674 RVA: 0x0009F19E File Offset: 0x0009D39E
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			public DateTime NEW_START_DATE
			{
				get
				{
					return (DateTime)base[this.tableSolutionProjects.NEW_START_DATEColumn];
				}
				set
				{
					base[this.tableSolutionProjects.NEW_START_DATEColumn] = value;
				}
			}

			// Token: 0x17000E6E RID: 3694
			// (get) Token: 0x06003183 RID: 12675 RVA: 0x0009F1B7 File Offset: 0x0009D3B7
			// (set) Token: 0x06003184 RID: 12676 RVA: 0x0009F1CF File Offset: 0x0009D3CF
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			public byte FORCE_STATUS
			{
				get
				{
					return (byte)base[this.tableSolutionProjects.FORCE_STATUSColumn];
				}
				set
				{
					base[this.tableSolutionProjects.FORCE_STATUSColumn] = value;
				}
			}

			// Token: 0x17000E6F RID: 3695
			// (get) Token: 0x06003185 RID: 12677 RVA: 0x0009F1E8 File Offset: 0x0009D3E8
			// (set) Token: 0x06003186 RID: 12678 RVA: 0x0009F22C File Offset: 0x0009D42C
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public byte STATUS
			{
				get
				{
					byte result;
					try
					{
						result = (byte)base[this.tableSolutionProjects.STATUSColumn];
					}
					catch (InvalidCastException innerException)
					{
						throw new StrongTypingException("The value for column 'STATUS' in table 'SolutionProjects' is DBNull.", innerException);
					}
					return result;
				}
				set
				{
					base[this.tableSolutionProjects.STATUSColumn] = value;
				}
			}

			// Token: 0x17000E70 RID: 3696
			// (get) Token: 0x06003187 RID: 12679 RVA: 0x0009F248 File Offset: 0x0009D448
			// (set) Token: 0x06003188 RID: 12680 RVA: 0x0009F28C File Offset: 0x0009D48C
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			public decimal RESOURCE_COST
			{
				get
				{
					decimal result;
					try
					{
						result = (decimal)base[this.tableSolutionProjects.RESOURCE_COSTColumn];
					}
					catch (InvalidCastException innerException)
					{
						throw new StrongTypingException("The value for column 'RESOURCE_COST' in table 'SolutionProjects' is DBNull.", innerException);
					}
					return result;
				}
				set
				{
					base[this.tableSolutionProjects.RESOURCE_COSTColumn] = value;
				}
			}

			// Token: 0x17000E71 RID: 3697
			// (get) Token: 0x06003189 RID: 12681 RVA: 0x0009F2A8 File Offset: 0x0009D4A8
			// (set) Token: 0x0600318A RID: 12682 RVA: 0x0009F2EC File Offset: 0x0009D4EC
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public decimal RESOURCE_WORK
			{
				get
				{
					decimal result;
					try
					{
						result = (decimal)base[this.tableSolutionProjects.RESOURCE_WORKColumn];
					}
					catch (InvalidCastException innerException)
					{
						throw new StrongTypingException("The value for column 'RESOURCE_WORK' in table 'SolutionProjects' is DBNull.", innerException);
					}
					return result;
				}
				set
				{
					base[this.tableSolutionProjects.RESOURCE_WORKColumn] = value;
				}
			}

			// Token: 0x17000E72 RID: 3698
			// (get) Token: 0x0600318B RID: 12683 RVA: 0x0009F308 File Offset: 0x0009D508
			// (set) Token: 0x0600318C RID: 12684 RVA: 0x0009F34C File Offset: 0x0009D54C
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public string PROJ_NAME
			{
				get
				{
					string result;
					try
					{
						result = (string)base[this.tableSolutionProjects.PROJ_NAMEColumn];
					}
					catch (InvalidCastException innerException)
					{
						throw new StrongTypingException("The value for column 'PROJ_NAME' in table 'SolutionProjects' is DBNull.", innerException);
					}
					return result;
				}
				set
				{
					base[this.tableSolutionProjects.PROJ_NAMEColumn] = value;
				}
			}

			// Token: 0x17000E73 RID: 3699
			// (get) Token: 0x0600318D RID: 12685 RVA: 0x0009F360 File Offset: 0x0009D560
			// (set) Token: 0x0600318E RID: 12686 RVA: 0x0009F3A4 File Offset: 0x0009D5A4
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			public double PRIORITY
			{
				get
				{
					double result;
					try
					{
						result = (double)base[this.tableSolutionProjects.PRIORITYColumn];
					}
					catch (InvalidCastException innerException)
					{
						throw new StrongTypingException("The value for column 'PRIORITY' in table 'SolutionProjects' is DBNull.", innerException);
					}
					return result;
				}
				set
				{
					base[this.tableSolutionProjects.PRIORITYColumn] = value;
				}
			}

			// Token: 0x17000E74 RID: 3700
			// (get) Token: 0x0600318F RID: 12687 RVA: 0x0009F3C0 File Offset: 0x0009D5C0
			// (set) Token: 0x06003190 RID: 12688 RVA: 0x0009F404 File Offset: 0x0009D604
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			public double ABSOLUTE_PRIORITY
			{
				get
				{
					double result;
					try
					{
						result = (double)base[this.tableSolutionProjects.ABSOLUTE_PRIORITYColumn];
					}
					catch (InvalidCastException innerException)
					{
						throw new StrongTypingException("The value for column 'ABSOLUTE_PRIORITY' in table 'SolutionProjects' is DBNull.", innerException);
					}
					return result;
				}
				set
				{
					base[this.tableSolutionProjects.ABSOLUTE_PRIORITYColumn] = value;
				}
			}

			// Token: 0x17000E75 RID: 3701
			// (get) Token: 0x06003191 RID: 12689 RVA: 0x0009F420 File Offset: 0x0009D620
			// (set) Token: 0x06003192 RID: 12690 RVA: 0x0009F464 File Offset: 0x0009D664
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public int DURATION
			{
				get
				{
					int result;
					try
					{
						result = (int)base[this.tableSolutionProjects.DURATIONColumn];
					}
					catch (InvalidCastException innerException)
					{
						throw new StrongTypingException("The value for column 'DURATION' in table 'SolutionProjects' is DBNull.", innerException);
					}
					return result;
				}
				set
				{
					base[this.tableSolutionProjects.DURATIONColumn] = value;
				}
			}

			// Token: 0x17000E76 RID: 3702
			// (get) Token: 0x06003193 RID: 12691 RVA: 0x0009F480 File Offset: 0x0009D680
			// (set) Token: 0x06003194 RID: 12692 RVA: 0x0009F4C4 File Offset: 0x0009D6C4
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public DateTime SNET
			{
				get
				{
					DateTime result;
					try
					{
						result = (DateTime)base[this.tableSolutionProjects.SNETColumn];
					}
					catch (InvalidCastException innerException)
					{
						throw new StrongTypingException("The value for column 'SNET' in table 'SolutionProjects' is DBNull.", innerException);
					}
					return result;
				}
				set
				{
					base[this.tableSolutionProjects.SNETColumn] = value;
				}
			}

			// Token: 0x17000E77 RID: 3703
			// (get) Token: 0x06003195 RID: 12693 RVA: 0x0009F4E0 File Offset: 0x0009D6E0
			// (set) Token: 0x06003196 RID: 12694 RVA: 0x0009F524 File Offset: 0x0009D724
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public DateTime FNLT
			{
				get
				{
					DateTime result;
					try
					{
						result = (DateTime)base[this.tableSolutionProjects.FNLTColumn];
					}
					catch (InvalidCastException innerException)
					{
						throw new StrongTypingException("The value for column 'FNLT' in table 'SolutionProjects' is DBNull.", innerException);
					}
					return result;
				}
				set
				{
					base[this.tableSolutionProjects.FNLTColumn] = value;
				}
			}

			// Token: 0x17000E78 RID: 3704
			// (get) Token: 0x06003197 RID: 12695 RVA: 0x0009F53D File Offset: 0x0009D73D
			// (set) Token: 0x06003198 RID: 12696 RVA: 0x0009F555 File Offset: 0x0009D755
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			public byte LOCKED
			{
				get
				{
					return (byte)base[this.tableSolutionProjects.LOCKEDColumn];
				}
				set
				{
					base[this.tableSolutionProjects.LOCKEDColumn] = value;
				}
			}

			// Token: 0x17000E79 RID: 3705
			// (get) Token: 0x06003199 RID: 12697 RVA: 0x0009F570 File Offset: 0x0009D770
			// (set) Token: 0x0600319A RID: 12698 RVA: 0x0009F5B4 File Offset: 0x0009D7B4
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public Guid FORCE_ALIAS_LT_STRUCT_UID
			{
				get
				{
					Guid result;
					try
					{
						result = (Guid)base[this.tableSolutionProjects.FORCE_ALIAS_LT_STRUCT_UIDColumn];
					}
					catch (InvalidCastException innerException)
					{
						throw new StrongTypingException("The value for column 'FORCE_ALIAS_LT_STRUCT_UID' in table 'SolutionProjects' is DBNull.", innerException);
					}
					return result;
				}
				set
				{
					base[this.tableSolutionProjects.FORCE_ALIAS_LT_STRUCT_UIDColumn] = value;
				}
			}

			// Token: 0x17000E7A RID: 3706
			// (get) Token: 0x0600319B RID: 12699 RVA: 0x0009F5D0 File Offset: 0x0009D7D0
			// (set) Token: 0x0600319C RID: 12700 RVA: 0x0009F614 File Offset: 0x0009D814
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			public string FORCE_ALIAS_LT_VALUE_FULL
			{
				get
				{
					string result;
					try
					{
						result = (string)base[this.tableSolutionProjects.FORCE_ALIAS_LT_VALUE_FULLColumn];
					}
					catch (InvalidCastException innerException)
					{
						throw new StrongTypingException("The value for column 'FORCE_ALIAS_LT_VALUE_FULL' in table 'SolutionProjects' is DBNull.", innerException);
					}
					return result;
				}
				set
				{
					base[this.tableSolutionProjects.FORCE_ALIAS_LT_VALUE_FULLColumn] = value;
				}
			}

			// Token: 0x17000E7B RID: 3707
			// (get) Token: 0x0600319D RID: 12701 RVA: 0x0009F628 File Offset: 0x0009D828
			// (set) Token: 0x0600319E RID: 12702 RVA: 0x0009F64A File Offset: 0x0009D84A
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public PlannerSolutionDataSet.SolutionsRow SolutionsRow
			{
				get
				{
					return (PlannerSolutionDataSet.SolutionsRow)base.GetParentRow(base.Table.ParentRelations["FK_Solutions_SolutionProjects"]);
				}
				set
				{
					base.SetParentRow(value, base.Table.ParentRelations["FK_Solutions_SolutionProjects"]);
				}
			}

			// Token: 0x0600319F RID: 12703 RVA: 0x0009F668 File Offset: 0x0009D868
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public bool IsSTATUSNull()
			{
				return base.IsNull(this.tableSolutionProjects.STATUSColumn);
			}

			// Token: 0x060031A0 RID: 12704 RVA: 0x0009F67B File Offset: 0x0009D87B
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			public void SetSTATUSNull()
			{
				base[this.tableSolutionProjects.STATUSColumn] = Convert.DBNull;
			}

			// Token: 0x060031A1 RID: 12705 RVA: 0x0009F693 File Offset: 0x0009D893
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			public bool IsRESOURCE_COSTNull()
			{
				return base.IsNull(this.tableSolutionProjects.RESOURCE_COSTColumn);
			}

			// Token: 0x060031A2 RID: 12706 RVA: 0x0009F6A6 File Offset: 0x0009D8A6
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			public void SetRESOURCE_COSTNull()
			{
				base[this.tableSolutionProjects.RESOURCE_COSTColumn] = Convert.DBNull;
			}

			// Token: 0x060031A3 RID: 12707 RVA: 0x0009F6BE File Offset: 0x0009D8BE
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public bool IsRESOURCE_WORKNull()
			{
				return base.IsNull(this.tableSolutionProjects.RESOURCE_WORKColumn);
			}

			// Token: 0x060031A4 RID: 12708 RVA: 0x0009F6D1 File Offset: 0x0009D8D1
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			public void SetRESOURCE_WORKNull()
			{
				base[this.tableSolutionProjects.RESOURCE_WORKColumn] = Convert.DBNull;
			}

			// Token: 0x060031A5 RID: 12709 RVA: 0x0009F6E9 File Offset: 0x0009D8E9
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			public bool IsPROJ_NAMENull()
			{
				return base.IsNull(this.tableSolutionProjects.PROJ_NAMEColumn);
			}

			// Token: 0x060031A6 RID: 12710 RVA: 0x0009F6FC File Offset: 0x0009D8FC
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			public void SetPROJ_NAMENull()
			{
				base[this.tableSolutionProjects.PROJ_NAMEColumn] = Convert.DBNull;
			}

			// Token: 0x060031A7 RID: 12711 RVA: 0x0009F714 File Offset: 0x0009D914
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			public bool IsPRIORITYNull()
			{
				return base.IsNull(this.tableSolutionProjects.PRIORITYColumn);
			}

			// Token: 0x060031A8 RID: 12712 RVA: 0x0009F727 File Offset: 0x0009D927
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			public void SetPRIORITYNull()
			{
				base[this.tableSolutionProjects.PRIORITYColumn] = Convert.DBNull;
			}

			// Token: 0x060031A9 RID: 12713 RVA: 0x0009F73F File Offset: 0x0009D93F
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			public bool IsABSOLUTE_PRIORITYNull()
			{
				return base.IsNull(this.tableSolutionProjects.ABSOLUTE_PRIORITYColumn);
			}

			// Token: 0x060031AA RID: 12714 RVA: 0x0009F752 File Offset: 0x0009D952
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			public void SetABSOLUTE_PRIORITYNull()
			{
				base[this.tableSolutionProjects.ABSOLUTE_PRIORITYColumn] = Convert.DBNull;
			}

			// Token: 0x060031AB RID: 12715 RVA: 0x0009F76A File Offset: 0x0009D96A
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			public bool IsDURATIONNull()
			{
				return base.IsNull(this.tableSolutionProjects.DURATIONColumn);
			}

			// Token: 0x060031AC RID: 12716 RVA: 0x0009F77D File Offset: 0x0009D97D
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			public void SetDURATIONNull()
			{
				base[this.tableSolutionProjects.DURATIONColumn] = Convert.DBNull;
			}

			// Token: 0x060031AD RID: 12717 RVA: 0x0009F795 File Offset: 0x0009D995
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public bool IsSNETNull()
			{
				return base.IsNull(this.tableSolutionProjects.SNETColumn);
			}

			// Token: 0x060031AE RID: 12718 RVA: 0x0009F7A8 File Offset: 0x0009D9A8
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			public void SetSNETNull()
			{
				base[this.tableSolutionProjects.SNETColumn] = Convert.DBNull;
			}

			// Token: 0x060031AF RID: 12719 RVA: 0x0009F7C0 File Offset: 0x0009D9C0
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			public bool IsFNLTNull()
			{
				return base.IsNull(this.tableSolutionProjects.FNLTColumn);
			}

			// Token: 0x060031B0 RID: 12720 RVA: 0x0009F7D3 File Offset: 0x0009D9D3
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public void SetFNLTNull()
			{
				base[this.tableSolutionProjects.FNLTColumn] = Convert.DBNull;
			}

			// Token: 0x060031B1 RID: 12721 RVA: 0x0009F7EB File Offset: 0x0009D9EB
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public bool IsFORCE_ALIAS_LT_STRUCT_UIDNull()
			{
				return base.IsNull(this.tableSolutionProjects.FORCE_ALIAS_LT_STRUCT_UIDColumn);
			}

			// Token: 0x060031B2 RID: 12722 RVA: 0x0009F7FE File Offset: 0x0009D9FE
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public void SetFORCE_ALIAS_LT_STRUCT_UIDNull()
			{
				base[this.tableSolutionProjects.FORCE_ALIAS_LT_STRUCT_UIDColumn] = Convert.DBNull;
			}

			// Token: 0x060031B3 RID: 12723 RVA: 0x0009F816 File Offset: 0x0009DA16
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			public bool IsFORCE_ALIAS_LT_VALUE_FULLNull()
			{
				return base.IsNull(this.tableSolutionProjects.FORCE_ALIAS_LT_VALUE_FULLColumn);
			}

			// Token: 0x060031B4 RID: 12724 RVA: 0x0009F829 File Offset: 0x0009DA29
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public void SetFORCE_ALIAS_LT_VALUE_FULLNull()
			{
				base[this.tableSolutionProjects.FORCE_ALIAS_LT_VALUE_FULLColumn] = Convert.DBNull;
			}

			// Token: 0x04000A75 RID: 2677
			private PlannerSolutionDataSet.SolutionProjectsDataTable tableSolutionProjects;
		}

		// Token: 0x0200026D RID: 621
		public class SolutionProjectRequirementsByRoleRow : DataRow
		{
			// Token: 0x060031B5 RID: 12725 RVA: 0x0009F841 File Offset: 0x0009DA41
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			internal SolutionProjectRequirementsByRoleRow(DataRowBuilder rb) : base(rb)
			{
				this.tableSolutionProjectRequirementsByRole = (PlannerSolutionDataSet.SolutionProjectRequirementsByRoleDataTable)base.Table;
			}

			// Token: 0x17000E7C RID: 3708
			// (get) Token: 0x060031B6 RID: 12726 RVA: 0x0009F85B File Offset: 0x0009DA5B
			// (set) Token: 0x060031B7 RID: 12727 RVA: 0x0009F873 File Offset: 0x0009DA73
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			public Guid SOLUTION_UID
			{
				get
				{
					return (Guid)base[this.tableSolutionProjectRequirementsByRole.SOLUTION_UIDColumn];
				}
				set
				{
					base[this.tableSolutionProjectRequirementsByRole.SOLUTION_UIDColumn] = value;
				}
			}

			// Token: 0x17000E7D RID: 3709
			// (get) Token: 0x060031B8 RID: 12728 RVA: 0x0009F88C File Offset: 0x0009DA8C
			// (set) Token: 0x060031B9 RID: 12729 RVA: 0x0009F8A4 File Offset: 0x0009DAA4
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public Guid PROJ_UID
			{
				get
				{
					return (Guid)base[this.tableSolutionProjectRequirementsByRole.PROJ_UIDColumn];
				}
				set
				{
					base[this.tableSolutionProjectRequirementsByRole.PROJ_UIDColumn] = value;
				}
			}

			// Token: 0x17000E7E RID: 3710
			// (get) Token: 0x060031BA RID: 12730 RVA: 0x0009F8BD File Offset: 0x0009DABD
			// (set) Token: 0x060031BB RID: 12731 RVA: 0x0009F8D5 File Offset: 0x0009DAD5
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			public Guid CUSTOM_FIELD_UID
			{
				get
				{
					return (Guid)base[this.tableSolutionProjectRequirementsByRole.CUSTOM_FIELD_UIDColumn];
				}
				set
				{
					base[this.tableSolutionProjectRequirementsByRole.CUSTOM_FIELD_UIDColumn] = value;
				}
			}

			// Token: 0x17000E7F RID: 3711
			// (get) Token: 0x060031BC RID: 12732 RVA: 0x0009F8EE File Offset: 0x0009DAEE
			// (set) Token: 0x060031BD RID: 12733 RVA: 0x0009F906 File Offset: 0x0009DB06
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public Guid LT_STRUCT_UID
			{
				get
				{
					return (Guid)base[this.tableSolutionProjectRequirementsByRole.LT_STRUCT_UIDColumn];
				}
				set
				{
					base[this.tableSolutionProjectRequirementsByRole.LT_STRUCT_UIDColumn] = value;
				}
			}

			// Token: 0x17000E80 RID: 3712
			// (get) Token: 0x060031BE RID: 12734 RVA: 0x0009F91F File Offset: 0x0009DB1F
			// (set) Token: 0x060031BF RID: 12735 RVA: 0x0009F937 File Offset: 0x0009DB37
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public DateTime START_DATE
			{
				get
				{
					return (DateTime)base[this.tableSolutionProjectRequirementsByRole.START_DATEColumn];
				}
				set
				{
					base[this.tableSolutionProjectRequirementsByRole.START_DATEColumn] = value;
				}
			}

			// Token: 0x17000E81 RID: 3713
			// (get) Token: 0x060031C0 RID: 12736 RVA: 0x0009F950 File Offset: 0x0009DB50
			// (set) Token: 0x060031C1 RID: 12737 RVA: 0x0009F968 File Offset: 0x0009DB68
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			public decimal ADDITIONAL_WORK
			{
				get
				{
					return (decimal)base[this.tableSolutionProjectRequirementsByRole.ADDITIONAL_WORKColumn];
				}
				set
				{
					base[this.tableSolutionProjectRequirementsByRole.ADDITIONAL_WORKColumn] = value;
				}
			}

			// Token: 0x17000E82 RID: 3714
			// (get) Token: 0x060031C2 RID: 12738 RVA: 0x0009F981 File Offset: 0x0009DB81
			// (set) Token: 0x060031C3 RID: 12739 RVA: 0x0009F999 File Offset: 0x0009DB99
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public Guid REQUIREMENT_UID
			{
				get
				{
					return (Guid)base[this.tableSolutionProjectRequirementsByRole.REQUIREMENT_UIDColumn];
				}
				set
				{
					base[this.tableSolutionProjectRequirementsByRole.REQUIREMENT_UIDColumn] = value;
				}
			}

			// Token: 0x17000E83 RID: 3715
			// (get) Token: 0x060031C4 RID: 12740 RVA: 0x0009F9B4 File Offset: 0x0009DBB4
			// (set) Token: 0x060031C5 RID: 12741 RVA: 0x0009F9F8 File Offset: 0x0009DBF8
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			public string ROLE_NAME
			{
				get
				{
					string result;
					try
					{
						result = (string)base[this.tableSolutionProjectRequirementsByRole.ROLE_NAMEColumn];
					}
					catch (InvalidCastException innerException)
					{
						throw new StrongTypingException("The value for column 'ROLE_NAME' in table 'SolutionProjectRequirementsByRole' is DBNull.", innerException);
					}
					return result;
				}
				set
				{
					base[this.tableSolutionProjectRequirementsByRole.ROLE_NAMEColumn] = value;
				}
			}

			// Token: 0x17000E84 RID: 3716
			// (get) Token: 0x060031C6 RID: 12742 RVA: 0x0009FA0C File Offset: 0x0009DC0C
			// (set) Token: 0x060031C7 RID: 12743 RVA: 0x0009FA50 File Offset: 0x0009DC50
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			public string PROJ_NAME
			{
				get
				{
					string result;
					try
					{
						result = (string)base[this.tableSolutionProjectRequirementsByRole.PROJ_NAMEColumn];
					}
					catch (InvalidCastException innerException)
					{
						throw new StrongTypingException("The value for column 'PROJ_NAME' in table 'SolutionProjectRequirementsByRole' is DBNull.", innerException);
					}
					return result;
				}
				set
				{
					base[this.tableSolutionProjectRequirementsByRole.PROJ_NAMEColumn] = value;
				}
			}

			// Token: 0x17000E85 RID: 3717
			// (get) Token: 0x060031C8 RID: 12744 RVA: 0x0009FA64 File Offset: 0x0009DC64
			// (set) Token: 0x060031C9 RID: 12745 RVA: 0x0009FA86 File Offset: 0x0009DC86
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			public PlannerSolutionDataSet.SolutionsRow SolutionsRow
			{
				get
				{
					return (PlannerSolutionDataSet.SolutionsRow)base.GetParentRow(base.Table.ParentRelations["FK_Solutions_SolutionProjectRequirementsByRole"]);
				}
				set
				{
					base.SetParentRow(value, base.Table.ParentRelations["FK_Solutions_SolutionProjectRequirementsByRole"]);
				}
			}

			// Token: 0x060031CA RID: 12746 RVA: 0x0009FAA4 File Offset: 0x0009DCA4
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			public bool IsROLE_NAMENull()
			{
				return base.IsNull(this.tableSolutionProjectRequirementsByRole.ROLE_NAMEColumn);
			}

			// Token: 0x060031CB RID: 12747 RVA: 0x0009FAB7 File Offset: 0x0009DCB7
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			public void SetROLE_NAMENull()
			{
				base[this.tableSolutionProjectRequirementsByRole.ROLE_NAMEColumn] = Convert.DBNull;
			}

			// Token: 0x060031CC RID: 12748 RVA: 0x0009FACF File Offset: 0x0009DCCF
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public bool IsPROJ_NAMENull()
			{
				return base.IsNull(this.tableSolutionProjectRequirementsByRole.PROJ_NAMEColumn);
			}

			// Token: 0x060031CD RID: 12749 RVA: 0x0009FAE2 File Offset: 0x0009DCE2
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			public void SetPROJ_NAMENull()
			{
				base[this.tableSolutionProjectRequirementsByRole.PROJ_NAMEColumn] = Convert.DBNull;
			}

			// Token: 0x04000A76 RID: 2678
			private PlannerSolutionDataSet.SolutionProjectRequirementsByRoleDataTable tableSolutionProjectRequirementsByRole;
		}

		// Token: 0x0200026E RID: 622
		public class SolutionEfficientFrontierRow : DataRow
		{
			// Token: 0x060031CE RID: 12750 RVA: 0x0009FAFA File Offset: 0x0009DCFA
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			internal SolutionEfficientFrontierRow(DataRowBuilder rb) : base(rb)
			{
				this.tableSolutionEfficientFrontier = (PlannerSolutionDataSet.SolutionEfficientFrontierDataTable)base.Table;
			}

			// Token: 0x17000E86 RID: 3718
			// (get) Token: 0x060031CF RID: 12751 RVA: 0x0009FB14 File Offset: 0x0009DD14
			// (set) Token: 0x060031D0 RID: 12752 RVA: 0x0009FB2C File Offset: 0x0009DD2C
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			public Guid FRONTIER_UID
			{
				get
				{
					return (Guid)base[this.tableSolutionEfficientFrontier.FRONTIER_UIDColumn];
				}
				set
				{
					base[this.tableSolutionEfficientFrontier.FRONTIER_UIDColumn] = value;
				}
			}

			// Token: 0x17000E87 RID: 3719
			// (get) Token: 0x060031D1 RID: 12753 RVA: 0x0009FB45 File Offset: 0x0009DD45
			// (set) Token: 0x060031D2 RID: 12754 RVA: 0x0009FB5D File Offset: 0x0009DD5D
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			public Guid ANALYSIS_UID
			{
				get
				{
					return (Guid)base[this.tableSolutionEfficientFrontier.ANALYSIS_UIDColumn];
				}
				set
				{
					base[this.tableSolutionEfficientFrontier.ANALYSIS_UIDColumn] = value;
				}
			}

			// Token: 0x17000E88 RID: 3720
			// (get) Token: 0x060031D3 RID: 12755 RVA: 0x0009FB76 File Offset: 0x0009DD76
			// (set) Token: 0x060031D4 RID: 12756 RVA: 0x0009FB8E File Offset: 0x0009DD8E
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public Guid POINT_UID
			{
				get
				{
					return (Guid)base[this.tableSolutionEfficientFrontier.POINT_UIDColumn];
				}
				set
				{
					base[this.tableSolutionEfficientFrontier.POINT_UIDColumn] = value;
				}
			}

			// Token: 0x17000E89 RID: 3721
			// (get) Token: 0x060031D5 RID: 12757 RVA: 0x0009FBA7 File Offset: 0x0009DDA7
			// (set) Token: 0x060031D6 RID: 12758 RVA: 0x0009FBBF File Offset: 0x0009DDBF
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			public decimal X_VALUE
			{
				get
				{
					return (decimal)base[this.tableSolutionEfficientFrontier.X_VALUEColumn];
				}
				set
				{
					base[this.tableSolutionEfficientFrontier.X_VALUEColumn] = value;
				}
			}

			// Token: 0x17000E8A RID: 3722
			// (get) Token: 0x060031D7 RID: 12759 RVA: 0x0009FBD8 File Offset: 0x0009DDD8
			// (set) Token: 0x060031D8 RID: 12760 RVA: 0x0009FBF0 File Offset: 0x0009DDF0
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			public decimal Y_VALUE
			{
				get
				{
					return (decimal)base[this.tableSolutionEfficientFrontier.Y_VALUEColumn];
				}
				set
				{
					base[this.tableSolutionEfficientFrontier.Y_VALUEColumn] = value;
				}
			}

			// Token: 0x17000E8B RID: 3723
			// (get) Token: 0x060031D9 RID: 12761 RVA: 0x0009FC09 File Offset: 0x0009DE09
			// (set) Token: 0x060031DA RID: 12762 RVA: 0x0009FC2B File Offset: 0x0009DE2B
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			public PlannerSolutionDataSet.SolutionsRow SolutionsRow
			{
				get
				{
					return (PlannerSolutionDataSet.SolutionsRow)base.GetParentRow(base.Table.ParentRelations["FK_Solutions_SolutionEfficientFrontier"]);
				}
				set
				{
					base.SetParentRow(value, base.Table.ParentRelations["FK_Solutions_SolutionEfficientFrontier"]);
				}
			}

			// Token: 0x04000A77 RID: 2679
			private PlannerSolutionDataSet.SolutionEfficientFrontierDataTable tableSolutionEfficientFrontier;
		}

		// Token: 0x0200026F RID: 623
		[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
		public class SolutionsRowChangeEvent : EventArgs
		{
			// Token: 0x060031DB RID: 12763 RVA: 0x0009FC49 File Offset: 0x0009DE49
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public SolutionsRowChangeEvent(PlannerSolutionDataSet.SolutionsRow row, DataRowAction action)
			{
				this.eventRow = row;
				this.eventAction = action;
			}

			// Token: 0x17000E8C RID: 3724
			// (get) Token: 0x060031DC RID: 12764 RVA: 0x0009FC5F File Offset: 0x0009DE5F
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			public PlannerSolutionDataSet.SolutionsRow Row
			{
				get
				{
					return this.eventRow;
				}
			}

			// Token: 0x17000E8D RID: 3725
			// (get) Token: 0x060031DD RID: 12765 RVA: 0x0009FC67 File Offset: 0x0009DE67
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			public DataRowAction Action
			{
				get
				{
					return this.eventAction;
				}
			}

			// Token: 0x04000A78 RID: 2680
			private PlannerSolutionDataSet.SolutionsRow eventRow;

			// Token: 0x04000A79 RID: 2681
			private DataRowAction eventAction;
		}

		// Token: 0x02000270 RID: 624
		[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
		public class SolutionResHiredRowChangeEvent : EventArgs
		{
			// Token: 0x060031DE RID: 12766 RVA: 0x0009FC6F File Offset: 0x0009DE6F
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			public SolutionResHiredRowChangeEvent(PlannerSolutionDataSet.SolutionResHiredRow row, DataRowAction action)
			{
				this.eventRow = row;
				this.eventAction = action;
			}

			// Token: 0x17000E8E RID: 3726
			// (get) Token: 0x060031DF RID: 12767 RVA: 0x0009FC85 File Offset: 0x0009DE85
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public PlannerSolutionDataSet.SolutionResHiredRow Row
			{
				get
				{
					return this.eventRow;
				}
			}

			// Token: 0x17000E8F RID: 3727
			// (get) Token: 0x060031E0 RID: 12768 RVA: 0x0009FC8D File Offset: 0x0009DE8D
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			public DataRowAction Action
			{
				get
				{
					return this.eventAction;
				}
			}

			// Token: 0x04000A7A RID: 2682
			private PlannerSolutionDataSet.SolutionResHiredRow eventRow;

			// Token: 0x04000A7B RID: 2683
			private DataRowAction eventAction;
		}

		// Token: 0x02000271 RID: 625
		[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
		public class SolutionProjectsRowChangeEvent : EventArgs
		{
			// Token: 0x060031E1 RID: 12769 RVA: 0x0009FC95 File Offset: 0x0009DE95
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public SolutionProjectsRowChangeEvent(PlannerSolutionDataSet.SolutionProjectsRow row, DataRowAction action)
			{
				this.eventRow = row;
				this.eventAction = action;
			}

			// Token: 0x17000E90 RID: 3728
			// (get) Token: 0x060031E2 RID: 12770 RVA: 0x0009FCAB File Offset: 0x0009DEAB
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			public PlannerSolutionDataSet.SolutionProjectsRow Row
			{
				get
				{
					return this.eventRow;
				}
			}

			// Token: 0x17000E91 RID: 3729
			// (get) Token: 0x060031E3 RID: 12771 RVA: 0x0009FCB3 File Offset: 0x0009DEB3
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public DataRowAction Action
			{
				get
				{
					return this.eventAction;
				}
			}

			// Token: 0x04000A7C RID: 2684
			private PlannerSolutionDataSet.SolutionProjectsRow eventRow;

			// Token: 0x04000A7D RID: 2685
			private DataRowAction eventAction;
		}

		// Token: 0x02000272 RID: 626
		[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
		public class SolutionProjectRequirementsByRoleRowChangeEvent : EventArgs
		{
			// Token: 0x060031E4 RID: 12772 RVA: 0x0009FCBB File Offset: 0x0009DEBB
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public SolutionProjectRequirementsByRoleRowChangeEvent(PlannerSolutionDataSet.SolutionProjectRequirementsByRoleRow row, DataRowAction action)
			{
				this.eventRow = row;
				this.eventAction = action;
			}

			// Token: 0x17000E92 RID: 3730
			// (get) Token: 0x060031E5 RID: 12773 RVA: 0x0009FCD1 File Offset: 0x0009DED1
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			public PlannerSolutionDataSet.SolutionProjectRequirementsByRoleRow Row
			{
				get
				{
					return this.eventRow;
				}
			}

			// Token: 0x17000E93 RID: 3731
			// (get) Token: 0x060031E6 RID: 12774 RVA: 0x0009FCD9 File Offset: 0x0009DED9
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			public DataRowAction Action
			{
				get
				{
					return this.eventAction;
				}
			}

			// Token: 0x04000A7E RID: 2686
			private PlannerSolutionDataSet.SolutionProjectRequirementsByRoleRow eventRow;

			// Token: 0x04000A7F RID: 2687
			private DataRowAction eventAction;
		}

		// Token: 0x02000273 RID: 627
		[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
		public class SolutionEfficientFrontierRowChangeEvent : EventArgs
		{
			// Token: 0x060031E7 RID: 12775 RVA: 0x0009FCE1 File Offset: 0x0009DEE1
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			public SolutionEfficientFrontierRowChangeEvent(PlannerSolutionDataSet.SolutionEfficientFrontierRow row, DataRowAction action)
			{
				this.eventRow = row;
				this.eventAction = action;
			}

			// Token: 0x17000E94 RID: 3732
			// (get) Token: 0x060031E8 RID: 12776 RVA: 0x0009FCF7 File Offset: 0x0009DEF7
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			public PlannerSolutionDataSet.SolutionEfficientFrontierRow Row
			{
				get
				{
					return this.eventRow;
				}
			}

			// Token: 0x17000E95 RID: 3733
			// (get) Token: 0x060031E9 RID: 12777 RVA: 0x0009FCFF File Offset: 0x0009DEFF
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			public DataRowAction Action
			{
				get
				{
					return this.eventAction;
				}
			}

			// Token: 0x04000A80 RID: 2688
			private PlannerSolutionDataSet.SolutionEfficientFrontierRow eventRow;

			// Token: 0x04000A81 RID: 2689
			private DataRowAction eventAction;
		}
	}
}
