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
	// Token: 0x02000043 RID: 67
	[DesignerCategory("code")]
	[XmlSchemaProvider("GetTypedDataSetSchema")]
	[HelpKeyword("vs.data.DataSet")]
	[ToolboxItem(true)]
	[XmlRoot("AnalysisDataSet")]
	[Serializable]
	public class AnalysisDataSet : DataSet
	{
		// Token: 0x06000401 RID: 1025 RVA: 0x0000E290 File Offset: 0x0000C490
		public new void EndInit()
		{
			base.EndInit();
			TypedDataSetUtilities.AllowNullsInNonTypedColumns(this.AnalysisPriorityData, new string[]
			{
				"MIN_VALUE",
				"WEIGHT",
				"MD_PROP_NAME",
				"MD_PROP_UID",
				"MAX_VALUE",
				"ANALYSIS_UID"
			});
			TypedDataSetUtilities.AllowNullsInNonTypedColumns(this.AnalysisRemainingCapacityByRole, new string[]
			{
				"REM_CAPACITY_UID",
				"START_DATE",
				"CUSTOM_FIELD_UID",
				"ANALYSIS_UID",
				"LT_STRUCT_UID",
				"REM_CAPACITY",
				"END_DATE"
			});
			TypedDataSetUtilities.AllowNullsInNonTypedColumns(this.Analysis, new string[]
			{
				"ANALYSIS_NAME",
				"CREATED_DATE",
				"PROJECT_IMPACT_CF_UID",
				"ROLE_CUSTOM_FIELD_UID",
				"ANALYSIS_TYPE",
				"FILTER_RESOURCES_RBS_NAME",
				"TIME_SCALE",
				"FORCE_IN_ALIAS_LT_UID",
				"PRIORITIZATION_TYPE",
				"LAST_UPDATED_BY_RES_NAME",
				"ANALYSIS_DESCRIPTION",
				"FILTER_RESOURCES_BY_RBS",
				"PRIORITIZATION_UID",
				"HORIZON_START_DATE",
				"FILTER_RESOURCES_BY_DEP",
				"ALT_PROJ_START_DATE_CF_UID",
				"USE_ALT_PROJ_DATES_FOR_RES_PLAN",
				"MOD_DATE",
				"CREATED_BY_RES_NAME",
				"CREATED_BY_RES_UID",
				"FORCE_OUT_ALIAS_LT_UID",
				"DEPARTMENT_NAME",
				"HORIZON_END_DATE",
				"PROJECT_IMPACT_CF_NAME",
				"ALT_PROJ_END_DATE_CF_UID",
				"ANALYSIS_UID",
				"FILTER_RESOURCES_RBS_VAL",
				"LAST_UPDATED_BY_RES_UID",
				"HARD_CONSTRAINT_CF_NAME",
				"HARD_CONSTRAINT_CF_UID",
				"DEPARTMENT_UID",
				"BOOKING_TYPE"
			});
			TypedDataSetUtilities.AllowNullsInNonTypedColumns(this.AnalysisProjectImpact, new string[]
			{
				"DRIVER_UID",
				"PROJ_UID",
				"ANALYSIS_UID",
				"LT_STRUCT_UID"
			});
			TypedDataSetUtilities.AllowNullsInNonTypedColumns(this.AnalysisOptimizerSolutions, new string[]
			{
				"LAST_UPDATED_BY_RES_NAME",
				"CREATED_DATE",
				"ANALYSIS_UID",
				"CREATED_BY_RES_UID",
				"SOLUTION_UID",
				"LAST_UPDATED_BY_RES_UID",
				"FRONTIER_UID",
				"SOLUTION_DESCRIPTION",
				"SOLUTION_NAME",
				"MOD_DATE",
				"OPT_USE_DEPENDENCIES",
				"CREATED_BY_RES_NAME"
			});
			TypedDataSetUtilities.AllowNullsInNonTypedColumns(this.AnalysisRoleRates, new string[]
			{
				"CUSTOM_FIELD_UID",
				"ANALYSIS_UID",
				"LT_STRUCT_UID",
				"STANDARD_RATE",
				"RATE_TABLE"
			});
			TypedDataSetUtilities.AllowNullsInNonTypedColumns(this.AnalysisPlannerSolutions, new string[]
			{
				"CREATED_DATE",
				"CREATED_BY_RES_UID",
				"SOLUTION_UID",
				"CONSTRAINT_TYPE",
				"ALLOCATION_THRESHOLD",
				"OPT_ENF_PROJ_DEP",
				"LAST_UPDATED_BY_RES_NAME",
				"ANALYSIS_UID",
				"HIRING_TYPE",
				"LAST_UPDATED_BY_RES_UID",
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
			TypedDataSetUtilities.AllowNullsInNonTypedColumns(this.AnalysisProjects, new string[]
			{
				"DURATION",
				"PROJ_NAME",
				"ANALYSIS_UID",
				"FNLT",
				"ABSOLUTE_PRIORITY",
				"START_DATE",
				"ORIGINAL_END_DATE",
				"PRIORITY",
				"ORIGINAL_START_DATE",
				"PROJ_UID",
				"SNET",
				"LOCKED"
			});
			TypedDataSetUtilities.AllowNullsInNonTypedColumns(this.AnalysisProjectRequirementsByRole, new string[]
			{
				"START_DATE",
				"PROJECT_REQUIREMENT",
				"CUSTOM_FIELD_UID",
				"PROJ_UID",
				"ANALYSIS_UID",
				"REQUIREMENT_UID",
				"LT_STRUCT_UID"
			});
		}

		// Token: 0x06000402 RID: 1026 RVA: 0x0000E6FC File Offset: 0x0000C8FC
		[DebuggerNonUserCode]
		[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
		public AnalysisDataSet()
		{
			base.BeginInit();
			this.InitClass();
			CollectionChangeEventHandler value = new CollectionChangeEventHandler(this.SchemaChanged);
			base.Tables.CollectionChanged += value;
			base.Relations.CollectionChanged += value;
			this.EndInit();
		}

		// Token: 0x06000403 RID: 1027 RVA: 0x0000E750 File Offset: 0x0000C950
		[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
		[DebuggerNonUserCode]
		protected AnalysisDataSet(SerializationInfo info, StreamingContext context) : base(info, context, false)
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
				if (dataSet.Tables["Analysis"] != null)
				{
					base.Tables.Add(new AnalysisDataSet.AnalysisDataTable(dataSet.Tables["Analysis"]));
				}
				if (dataSet.Tables["AnalysisProjects"] != null)
				{
					base.Tables.Add(new AnalysisDataSet.AnalysisProjectsDataTable(dataSet.Tables["AnalysisProjects"]));
				}
				if (dataSet.Tables["AnalysisPriorityData"] != null)
				{
					base.Tables.Add(new AnalysisDataSet.AnalysisPriorityDataDataTable(dataSet.Tables["AnalysisPriorityData"]));
				}
				if (dataSet.Tables["AnalysisProjectImpact"] != null)
				{
					base.Tables.Add(new AnalysisDataSet.AnalysisProjectImpactDataTable(dataSet.Tables["AnalysisProjectImpact"]));
				}
				if (dataSet.Tables["AnalysisOptimizerSolutions"] != null)
				{
					base.Tables.Add(new AnalysisDataSet.AnalysisOptimizerSolutionsDataTable(dataSet.Tables["AnalysisOptimizerSolutions"]));
				}
				if (dataSet.Tables["AnalysisPlannerSolutions"] != null)
				{
					base.Tables.Add(new AnalysisDataSet.AnalysisPlannerSolutionsDataTable(dataSet.Tables["AnalysisPlannerSolutions"]));
				}
				if (dataSet.Tables["AnalysisRemainingCapacityByRole"] != null)
				{
					base.Tables.Add(new AnalysisDataSet.AnalysisRemainingCapacityByRoleDataTable(dataSet.Tables["AnalysisRemainingCapacityByRole"]));
				}
				if (dataSet.Tables["AnalysisRoleRates"] != null)
				{
					base.Tables.Add(new AnalysisDataSet.AnalysisRoleRatesDataTable(dataSet.Tables["AnalysisRoleRates"]));
				}
				if (dataSet.Tables["AnalysisProjectRequirementsByRole"] != null)
				{
					base.Tables.Add(new AnalysisDataSet.AnalysisProjectRequirementsByRoleDataTable(dataSet.Tables["AnalysisProjectRequirementsByRole"]));
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

		// Token: 0x17000117 RID: 279
		// (get) Token: 0x06000404 RID: 1028 RVA: 0x0000EA3D File Offset: 0x0000CC3D
		[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Content)]
		[DebuggerNonUserCode]
		[Browsable(false)]
		public AnalysisDataSet.AnalysisDataTable Analysis
		{
			get
			{
				return this.tableAnalysis;
			}
		}

		// Token: 0x17000118 RID: 280
		// (get) Token: 0x06000405 RID: 1029 RVA: 0x0000EA45 File Offset: 0x0000CC45
		[Browsable(false)]
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Content)]
		[DebuggerNonUserCode]
		[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
		public AnalysisDataSet.AnalysisProjectsDataTable AnalysisProjects
		{
			get
			{
				return this.tableAnalysisProjects;
			}
		}

		// Token: 0x17000119 RID: 281
		// (get) Token: 0x06000406 RID: 1030 RVA: 0x0000EA4D File Offset: 0x0000CC4D
		[Browsable(false)]
		[DebuggerNonUserCode]
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Content)]
		[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
		public AnalysisDataSet.AnalysisPriorityDataDataTable AnalysisPriorityData
		{
			get
			{
				return this.tableAnalysisPriorityData;
			}
		}

		// Token: 0x1700011A RID: 282
		// (get) Token: 0x06000407 RID: 1031 RVA: 0x0000EA55 File Offset: 0x0000CC55
		[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Content)]
		[DebuggerNonUserCode]
		[Browsable(false)]
		public AnalysisDataSet.AnalysisProjectImpactDataTable AnalysisProjectImpact
		{
			get
			{
				return this.tableAnalysisProjectImpact;
			}
		}

		// Token: 0x1700011B RID: 283
		// (get) Token: 0x06000408 RID: 1032 RVA: 0x0000EA5D File Offset: 0x0000CC5D
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Content)]
		[Browsable(false)]
		[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
		[DebuggerNonUserCode]
		public AnalysisDataSet.AnalysisOptimizerSolutionsDataTable AnalysisOptimizerSolutions
		{
			get
			{
				return this.tableAnalysisOptimizerSolutions;
			}
		}

		// Token: 0x1700011C RID: 284
		// (get) Token: 0x06000409 RID: 1033 RVA: 0x0000EA65 File Offset: 0x0000CC65
		[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
		[DebuggerNonUserCode]
		[Browsable(false)]
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Content)]
		public AnalysisDataSet.AnalysisPlannerSolutionsDataTable AnalysisPlannerSolutions
		{
			get
			{
				return this.tableAnalysisPlannerSolutions;
			}
		}

		// Token: 0x1700011D RID: 285
		// (get) Token: 0x0600040A RID: 1034 RVA: 0x0000EA6D File Offset: 0x0000CC6D
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Content)]
		[DebuggerNonUserCode]
		[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
		[Browsable(false)]
		public AnalysisDataSet.AnalysisRemainingCapacityByRoleDataTable AnalysisRemainingCapacityByRole
		{
			get
			{
				return this.tableAnalysisRemainingCapacityByRole;
			}
		}

		// Token: 0x1700011E RID: 286
		// (get) Token: 0x0600040B RID: 1035 RVA: 0x0000EA75 File Offset: 0x0000CC75
		[Browsable(false)]
		[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Content)]
		[DebuggerNonUserCode]
		public AnalysisDataSet.AnalysisRoleRatesDataTable AnalysisRoleRates
		{
			get
			{
				return this.tableAnalysisRoleRates;
			}
		}

		// Token: 0x1700011F RID: 287
		// (get) Token: 0x0600040C RID: 1036 RVA: 0x0000EA7D File Offset: 0x0000CC7D
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Content)]
		[DebuggerNonUserCode]
		[Browsable(false)]
		[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
		public AnalysisDataSet.AnalysisProjectRequirementsByRoleDataTable AnalysisProjectRequirementsByRole
		{
			get
			{
				return this.tableAnalysisProjectRequirementsByRole;
			}
		}

		// Token: 0x17000120 RID: 288
		// (get) Token: 0x0600040D RID: 1037 RVA: 0x0000EA85 File Offset: 0x0000CC85
		// (set) Token: 0x0600040E RID: 1038 RVA: 0x0000EA8D File Offset: 0x0000CC8D
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Visible)]
		[DebuggerNonUserCode]
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

		// Token: 0x17000121 RID: 289
		// (get) Token: 0x0600040F RID: 1039 RVA: 0x0000EA96 File Offset: 0x0000CC96
		[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
		[DebuggerNonUserCode]
		public new DataTableCollection Tables
		{
			get
			{
				return base.Tables;
			}
		}

		// Token: 0x17000122 RID: 290
		// (get) Token: 0x06000410 RID: 1040 RVA: 0x0000EA9E File Offset: 0x0000CC9E
		[DebuggerNonUserCode]
		[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
		public new DataRelationCollection Relations
		{
			get
			{
				return base.Relations;
			}
		}

		// Token: 0x06000411 RID: 1041 RVA: 0x0000EAA6 File Offset: 0x0000CCA6
		[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
		[DebuggerNonUserCode]
		protected override void InitializeDerivedDataSet()
		{
			base.BeginInit();
			this.InitClass();
			this.EndInit();
		}

		// Token: 0x06000412 RID: 1042 RVA: 0x0000EABC File Offset: 0x0000CCBC
		[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
		[DebuggerNonUserCode]
		public override DataSet Clone()
		{
			AnalysisDataSet analysisDataSet = (AnalysisDataSet)base.Clone();
			analysisDataSet.InitVars();
			analysisDataSet.SchemaSerializationMode = this.SchemaSerializationMode;
			return analysisDataSet;
		}

		// Token: 0x06000413 RID: 1043 RVA: 0x0000EAE8 File Offset: 0x0000CCE8
		[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
		[DebuggerNonUserCode]
		protected override bool ShouldSerializeTables()
		{
			return false;
		}

		// Token: 0x06000414 RID: 1044 RVA: 0x0000EAEB File Offset: 0x0000CCEB
		[DebuggerNonUserCode]
		[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
		protected override bool ShouldSerializeRelations()
		{
			return false;
		}

		// Token: 0x06000415 RID: 1045 RVA: 0x0000EAF0 File Offset: 0x0000CCF0
		[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
		[DebuggerNonUserCode]
		protected override void ReadXmlSerializable(XmlReader reader)
		{
			if (base.DetermineSchemaSerializationMode(reader) == SchemaSerializationMode.IncludeSchema)
			{
				this.Reset();
				DataSet dataSet = new DataSet();
				dataSet.ReadXml(reader);
				if (dataSet.Tables["Analysis"] != null)
				{
					base.Tables.Add(new AnalysisDataSet.AnalysisDataTable(dataSet.Tables["Analysis"]));
				}
				if (dataSet.Tables["AnalysisProjects"] != null)
				{
					base.Tables.Add(new AnalysisDataSet.AnalysisProjectsDataTable(dataSet.Tables["AnalysisProjects"]));
				}
				if (dataSet.Tables["AnalysisPriorityData"] != null)
				{
					base.Tables.Add(new AnalysisDataSet.AnalysisPriorityDataDataTable(dataSet.Tables["AnalysisPriorityData"]));
				}
				if (dataSet.Tables["AnalysisProjectImpact"] != null)
				{
					base.Tables.Add(new AnalysisDataSet.AnalysisProjectImpactDataTable(dataSet.Tables["AnalysisProjectImpact"]));
				}
				if (dataSet.Tables["AnalysisOptimizerSolutions"] != null)
				{
					base.Tables.Add(new AnalysisDataSet.AnalysisOptimizerSolutionsDataTable(dataSet.Tables["AnalysisOptimizerSolutions"]));
				}
				if (dataSet.Tables["AnalysisPlannerSolutions"] != null)
				{
					base.Tables.Add(new AnalysisDataSet.AnalysisPlannerSolutionsDataTable(dataSet.Tables["AnalysisPlannerSolutions"]));
				}
				if (dataSet.Tables["AnalysisRemainingCapacityByRole"] != null)
				{
					base.Tables.Add(new AnalysisDataSet.AnalysisRemainingCapacityByRoleDataTable(dataSet.Tables["AnalysisRemainingCapacityByRole"]));
				}
				if (dataSet.Tables["AnalysisRoleRates"] != null)
				{
					base.Tables.Add(new AnalysisDataSet.AnalysisRoleRatesDataTable(dataSet.Tables["AnalysisRoleRates"]));
				}
				if (dataSet.Tables["AnalysisProjectRequirementsByRole"] != null)
				{
					base.Tables.Add(new AnalysisDataSet.AnalysisProjectRequirementsByRoleDataTable(dataSet.Tables["AnalysisProjectRequirementsByRole"]));
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

		// Token: 0x06000416 RID: 1046 RVA: 0x0000ED48 File Offset: 0x0000CF48
		[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
		[DebuggerNonUserCode]
		protected override XmlSchema GetSchemaSerializable()
		{
			MemoryStream memoryStream = new MemoryStream();
			base.WriteXmlSchema(new XmlTextWriter(memoryStream, null));
			memoryStream.Position = 0L;
			return XmlSchema.Read(new XmlTextReader(memoryStream), null);
		}

		// Token: 0x06000417 RID: 1047 RVA: 0x0000ED7C File Offset: 0x0000CF7C
		[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
		[DebuggerNonUserCode]
		internal void InitVars()
		{
			this.InitVars(true);
		}

		// Token: 0x06000418 RID: 1048 RVA: 0x0000ED88 File Offset: 0x0000CF88
		[DebuggerNonUserCode]
		[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
		internal void InitVars(bool initTable)
		{
			this.tableAnalysis = (AnalysisDataSet.AnalysisDataTable)base.Tables["Analysis"];
			if (initTable && this.tableAnalysis != null)
			{
				this.tableAnalysis.InitVars();
			}
			this.tableAnalysisProjects = (AnalysisDataSet.AnalysisProjectsDataTable)base.Tables["AnalysisProjects"];
			if (initTable && this.tableAnalysisProjects != null)
			{
				this.tableAnalysisProjects.InitVars();
			}
			this.tableAnalysisPriorityData = (AnalysisDataSet.AnalysisPriorityDataDataTable)base.Tables["AnalysisPriorityData"];
			if (initTable && this.tableAnalysisPriorityData != null)
			{
				this.tableAnalysisPriorityData.InitVars();
			}
			this.tableAnalysisProjectImpact = (AnalysisDataSet.AnalysisProjectImpactDataTable)base.Tables["AnalysisProjectImpact"];
			if (initTable && this.tableAnalysisProjectImpact != null)
			{
				this.tableAnalysisProjectImpact.InitVars();
			}
			this.tableAnalysisOptimizerSolutions = (AnalysisDataSet.AnalysisOptimizerSolutionsDataTable)base.Tables["AnalysisOptimizerSolutions"];
			if (initTable && this.tableAnalysisOptimizerSolutions != null)
			{
				this.tableAnalysisOptimizerSolutions.InitVars();
			}
			this.tableAnalysisPlannerSolutions = (AnalysisDataSet.AnalysisPlannerSolutionsDataTable)base.Tables["AnalysisPlannerSolutions"];
			if (initTable && this.tableAnalysisPlannerSolutions != null)
			{
				this.tableAnalysisPlannerSolutions.InitVars();
			}
			this.tableAnalysisRemainingCapacityByRole = (AnalysisDataSet.AnalysisRemainingCapacityByRoleDataTable)base.Tables["AnalysisRemainingCapacityByRole"];
			if (initTable && this.tableAnalysisRemainingCapacityByRole != null)
			{
				this.tableAnalysisRemainingCapacityByRole.InitVars();
			}
			this.tableAnalysisRoleRates = (AnalysisDataSet.AnalysisRoleRatesDataTable)base.Tables["AnalysisRoleRates"];
			if (initTable && this.tableAnalysisRoleRates != null)
			{
				this.tableAnalysisRoleRates.InitVars();
			}
			this.tableAnalysisProjectRequirementsByRole = (AnalysisDataSet.AnalysisProjectRequirementsByRoleDataTable)base.Tables["AnalysisProjectRequirementsByRole"];
			if (initTable && this.tableAnalysisProjectRequirementsByRole != null)
			{
				this.tableAnalysisProjectRequirementsByRole.InitVars();
			}
			this.relationFK_Analysis_AnalysisProjects = this.Relations["FK_Analysis_AnalysisProjects"];
			this.relationFK_Analysis_AnalysisPriorityData = this.Relations["FK_Analysis_AnalysisPriorityData"];
			this.relationFK_AnalysisProjects_AnalysisProjectImpact = this.Relations["FK_AnalysisProjects_AnalysisProjectImpact"];
			this.relationFK_Analysis_AnalysisOptimizerSolutions = this.Relations["FK_Analysis_AnalysisOptimizerSolutions"];
			this.relationFK_Analysis_AnalysisPlannerSolutions = this.Relations["FK_Analysis_AnalysisPlannerSolutions"];
		}

		// Token: 0x06000419 RID: 1049 RVA: 0x0000EFBC File Offset: 0x0000D1BC
		[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
		[DebuggerNonUserCode]
		private void InitClass()
		{
			base.DataSetName = "AnalysisDataSet";
			base.Prefix = "";
			base.Namespace = "http://schemas.microsoft.com/office/project/server/webservices/AnalysisDataSet/";
			base.EnforceConstraints = true;
			this.SchemaSerializationMode = SchemaSerializationMode.IncludeSchema;
			this.tableAnalysis = new AnalysisDataSet.AnalysisDataTable();
			base.Tables.Add(this.tableAnalysis);
			this.tableAnalysisProjects = new AnalysisDataSet.AnalysisProjectsDataTable();
			base.Tables.Add(this.tableAnalysisProjects);
			this.tableAnalysisPriorityData = new AnalysisDataSet.AnalysisPriorityDataDataTable();
			base.Tables.Add(this.tableAnalysisPriorityData);
			this.tableAnalysisProjectImpact = new AnalysisDataSet.AnalysisProjectImpactDataTable();
			base.Tables.Add(this.tableAnalysisProjectImpact);
			this.tableAnalysisOptimizerSolutions = new AnalysisDataSet.AnalysisOptimizerSolutionsDataTable();
			base.Tables.Add(this.tableAnalysisOptimizerSolutions);
			this.tableAnalysisPlannerSolutions = new AnalysisDataSet.AnalysisPlannerSolutionsDataTable();
			base.Tables.Add(this.tableAnalysisPlannerSolutions);
			this.tableAnalysisRemainingCapacityByRole = new AnalysisDataSet.AnalysisRemainingCapacityByRoleDataTable();
			base.Tables.Add(this.tableAnalysisRemainingCapacityByRole);
			this.tableAnalysisRoleRates = new AnalysisDataSet.AnalysisRoleRatesDataTable();
			base.Tables.Add(this.tableAnalysisRoleRates);
			this.tableAnalysisProjectRequirementsByRole = new AnalysisDataSet.AnalysisProjectRequirementsByRoleDataTable();
			base.Tables.Add(this.tableAnalysisProjectRequirementsByRole);
			ForeignKeyConstraint foreignKeyConstraint = new ForeignKeyConstraint("FK_Analysis_AnalysisProjects", new DataColumn[]
			{
				this.tableAnalysis.ANALYSIS_UIDColumn
			}, new DataColumn[]
			{
				this.tableAnalysisProjects.ANALYSIS_UIDColumn
			});
			this.tableAnalysisProjects.Constraints.Add(foreignKeyConstraint);
			foreignKeyConstraint.AcceptRejectRule = AcceptRejectRule.None;
			foreignKeyConstraint.DeleteRule = Rule.None;
			foreignKeyConstraint.UpdateRule = Rule.None;
			foreignKeyConstraint = new ForeignKeyConstraint("FK_Analysis_AnalysisPriorityData", new DataColumn[]
			{
				this.tableAnalysis.ANALYSIS_UIDColumn
			}, new DataColumn[]
			{
				this.tableAnalysisPriorityData.ANALYSIS_UIDColumn
			});
			this.tableAnalysisPriorityData.Constraints.Add(foreignKeyConstraint);
			foreignKeyConstraint.AcceptRejectRule = AcceptRejectRule.None;
			foreignKeyConstraint.DeleteRule = Rule.None;
			foreignKeyConstraint.UpdateRule = Rule.None;
			foreignKeyConstraint = new ForeignKeyConstraint("FK_AnalysisProjects_AnalysisProjectImpact", new DataColumn[]
			{
				this.tableAnalysisProjects.ANALYSIS_UIDColumn,
				this.tableAnalysisProjects.PROJ_UIDColumn
			}, new DataColumn[]
			{
				this.tableAnalysisProjectImpact.ANALYSIS_UIDColumn,
				this.tableAnalysisProjectImpact.PROJ_UIDColumn
			});
			this.tableAnalysisProjectImpact.Constraints.Add(foreignKeyConstraint);
			foreignKeyConstraint.AcceptRejectRule = AcceptRejectRule.None;
			foreignKeyConstraint.DeleteRule = Rule.None;
			foreignKeyConstraint.UpdateRule = Rule.None;
			foreignKeyConstraint = new ForeignKeyConstraint("FK_Analysis_AnalysisOptimizerSolutions", new DataColumn[]
			{
				this.tableAnalysis.ANALYSIS_UIDColumn
			}, new DataColumn[]
			{
				this.tableAnalysisOptimizerSolutions.ANALYSIS_UIDColumn
			});
			this.tableAnalysisOptimizerSolutions.Constraints.Add(foreignKeyConstraint);
			foreignKeyConstraint.AcceptRejectRule = AcceptRejectRule.None;
			foreignKeyConstraint.DeleteRule = Rule.None;
			foreignKeyConstraint.UpdateRule = Rule.None;
			foreignKeyConstraint = new ForeignKeyConstraint("FK_Analysis_AnalysisPlannerSolutions", new DataColumn[]
			{
				this.tableAnalysis.ANALYSIS_UIDColumn
			}, new DataColumn[]
			{
				this.tableAnalysisPlannerSolutions.ANALYSIS_UIDColumn
			});
			this.tableAnalysisPlannerSolutions.Constraints.Add(foreignKeyConstraint);
			foreignKeyConstraint.AcceptRejectRule = AcceptRejectRule.None;
			foreignKeyConstraint.DeleteRule = Rule.None;
			foreignKeyConstraint.UpdateRule = Rule.None;
			foreignKeyConstraint = new ForeignKeyConstraint("FK_Analysis_AnalysisRemainingCapacityByRole", new DataColumn[]
			{
				this.tableAnalysis.ANALYSIS_UIDColumn
			}, new DataColumn[]
			{
				this.tableAnalysisRemainingCapacityByRole.ANALYSIS_UIDColumn
			});
			this.tableAnalysisRemainingCapacityByRole.Constraints.Add(foreignKeyConstraint);
			foreignKeyConstraint.AcceptRejectRule = AcceptRejectRule.None;
			foreignKeyConstraint.DeleteRule = Rule.None;
			foreignKeyConstraint.UpdateRule = Rule.None;
			foreignKeyConstraint = new ForeignKeyConstraint("FK_Analysis_AnalysisRoleRates", new DataColumn[]
			{
				this.tableAnalysis.ANALYSIS_UIDColumn
			}, new DataColumn[]
			{
				this.tableAnalysisRoleRates.ANALYSIS_UIDColumn
			});
			this.tableAnalysisRoleRates.Constraints.Add(foreignKeyConstraint);
			foreignKeyConstraint.AcceptRejectRule = AcceptRejectRule.None;
			foreignKeyConstraint.DeleteRule = Rule.None;
			foreignKeyConstraint.UpdateRule = Rule.None;
			foreignKeyConstraint = new ForeignKeyConstraint("FK_AnalysisProjects_AnalysisProjectRequirementsByRole", new DataColumn[]
			{
				this.tableAnalysisProjects.ANALYSIS_UIDColumn,
				this.tableAnalysisProjects.PROJ_UIDColumn
			}, new DataColumn[]
			{
				this.tableAnalysisProjectRequirementsByRole.ANALYSIS_UIDColumn,
				this.tableAnalysisProjectRequirementsByRole.PROJ_UIDColumn
			});
			this.tableAnalysisProjectRequirementsByRole.Constraints.Add(foreignKeyConstraint);
			foreignKeyConstraint.AcceptRejectRule = AcceptRejectRule.None;
			foreignKeyConstraint.DeleteRule = Rule.None;
			foreignKeyConstraint.UpdateRule = Rule.None;
			this.relationFK_Analysis_AnalysisProjects = new DataRelation("FK_Analysis_AnalysisProjects", new DataColumn[]
			{
				this.tableAnalysis.ANALYSIS_UIDColumn
			}, new DataColumn[]
			{
				this.tableAnalysisProjects.ANALYSIS_UIDColumn
			}, false);
			this.Relations.Add(this.relationFK_Analysis_AnalysisProjects);
			this.relationFK_Analysis_AnalysisPriorityData = new DataRelation("FK_Analysis_AnalysisPriorityData", new DataColumn[]
			{
				this.tableAnalysis.ANALYSIS_UIDColumn
			}, new DataColumn[]
			{
				this.tableAnalysisPriorityData.ANALYSIS_UIDColumn
			}, false);
			this.Relations.Add(this.relationFK_Analysis_AnalysisPriorityData);
			this.relationFK_AnalysisProjects_AnalysisProjectImpact = new DataRelation("FK_AnalysisProjects_AnalysisProjectImpact", new DataColumn[]
			{
				this.tableAnalysisProjects.ANALYSIS_UIDColumn,
				this.tableAnalysisProjects.PROJ_UIDColumn
			}, new DataColumn[]
			{
				this.tableAnalysisProjectImpact.ANALYSIS_UIDColumn,
				this.tableAnalysisProjectImpact.PROJ_UIDColumn
			}, false);
			this.Relations.Add(this.relationFK_AnalysisProjects_AnalysisProjectImpact);
			this.relationFK_Analysis_AnalysisOptimizerSolutions = new DataRelation("FK_Analysis_AnalysisOptimizerSolutions", new DataColumn[]
			{
				this.tableAnalysis.ANALYSIS_UIDColumn
			}, new DataColumn[]
			{
				this.tableAnalysisOptimizerSolutions.ANALYSIS_UIDColumn
			}, false);
			this.Relations.Add(this.relationFK_Analysis_AnalysisOptimizerSolutions);
			this.relationFK_Analysis_AnalysisPlannerSolutions = new DataRelation("FK_Analysis_AnalysisPlannerSolutions", new DataColumn[]
			{
				this.tableAnalysis.ANALYSIS_UIDColumn
			}, new DataColumn[]
			{
				this.tableAnalysisPlannerSolutions.ANALYSIS_UIDColumn
			}, false);
			this.Relations.Add(this.relationFK_Analysis_AnalysisPlannerSolutions);
		}

		// Token: 0x0600041A RID: 1050 RVA: 0x0000F601 File Offset: 0x0000D801
		[DebuggerNonUserCode]
		[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
		private bool ShouldSerializeAnalysis()
		{
			return false;
		}

		// Token: 0x0600041B RID: 1051 RVA: 0x0000F604 File Offset: 0x0000D804
		[DebuggerNonUserCode]
		[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
		private bool ShouldSerializeAnalysisProjects()
		{
			return false;
		}

		// Token: 0x0600041C RID: 1052 RVA: 0x0000F607 File Offset: 0x0000D807
		[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
		[DebuggerNonUserCode]
		private bool ShouldSerializeAnalysisPriorityData()
		{
			return false;
		}

		// Token: 0x0600041D RID: 1053 RVA: 0x0000F60A File Offset: 0x0000D80A
		[DebuggerNonUserCode]
		[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
		private bool ShouldSerializeAnalysisProjectImpact()
		{
			return false;
		}

		// Token: 0x0600041E RID: 1054 RVA: 0x0000F60D File Offset: 0x0000D80D
		[DebuggerNonUserCode]
		[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
		private bool ShouldSerializeAnalysisOptimizerSolutions()
		{
			return false;
		}

		// Token: 0x0600041F RID: 1055 RVA: 0x0000F610 File Offset: 0x0000D810
		[DebuggerNonUserCode]
		[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
		private bool ShouldSerializeAnalysisPlannerSolutions()
		{
			return false;
		}

		// Token: 0x06000420 RID: 1056 RVA: 0x0000F613 File Offset: 0x0000D813
		[DebuggerNonUserCode]
		[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
		private bool ShouldSerializeAnalysisRemainingCapacityByRole()
		{
			return false;
		}

		// Token: 0x06000421 RID: 1057 RVA: 0x0000F616 File Offset: 0x0000D816
		[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
		[DebuggerNonUserCode]
		private bool ShouldSerializeAnalysisRoleRates()
		{
			return false;
		}

		// Token: 0x06000422 RID: 1058 RVA: 0x0000F619 File Offset: 0x0000D819
		[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
		[DebuggerNonUserCode]
		private bool ShouldSerializeAnalysisProjectRequirementsByRole()
		{
			return false;
		}

		// Token: 0x06000423 RID: 1059 RVA: 0x0000F61C File Offset: 0x0000D81C
		[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
		[DebuggerNonUserCode]
		private void SchemaChanged(object sender, CollectionChangeEventArgs e)
		{
			if (e.Action == CollectionChangeAction.Remove)
			{
				this.InitVars();
			}
		}

		// Token: 0x06000424 RID: 1060 RVA: 0x0000F630 File Offset: 0x0000D830
		[DebuggerNonUserCode]
		[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
		public static XmlSchemaComplexType GetTypedDataSetSchema(XmlSchemaSet xs)
		{
			AnalysisDataSet analysisDataSet = new AnalysisDataSet();
			XmlSchemaComplexType xmlSchemaComplexType = new XmlSchemaComplexType();
			XmlSchemaSequence xmlSchemaSequence = new XmlSchemaSequence();
			XmlSchemaAny xmlSchemaAny = new XmlSchemaAny();
			xmlSchemaAny.Namespace = analysisDataSet.Namespace;
			xmlSchemaSequence.Items.Add(xmlSchemaAny);
			xmlSchemaComplexType.Particle = xmlSchemaSequence;
			XmlSchema schemaSerializable = analysisDataSet.GetSchemaSerializable();
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

		// Token: 0x0400011E RID: 286
		private AnalysisDataSet.AnalysisDataTable tableAnalysis;

		// Token: 0x0400011F RID: 287
		private AnalysisDataSet.AnalysisProjectsDataTable tableAnalysisProjects;

		// Token: 0x04000120 RID: 288
		private AnalysisDataSet.AnalysisPriorityDataDataTable tableAnalysisPriorityData;

		// Token: 0x04000121 RID: 289
		private AnalysisDataSet.AnalysisProjectImpactDataTable tableAnalysisProjectImpact;

		// Token: 0x04000122 RID: 290
		private AnalysisDataSet.AnalysisOptimizerSolutionsDataTable tableAnalysisOptimizerSolutions;

		// Token: 0x04000123 RID: 291
		private AnalysisDataSet.AnalysisPlannerSolutionsDataTable tableAnalysisPlannerSolutions;

		// Token: 0x04000124 RID: 292
		private AnalysisDataSet.AnalysisRemainingCapacityByRoleDataTable tableAnalysisRemainingCapacityByRole;

		// Token: 0x04000125 RID: 293
		private AnalysisDataSet.AnalysisRoleRatesDataTable tableAnalysisRoleRates;

		// Token: 0x04000126 RID: 294
		private AnalysisDataSet.AnalysisProjectRequirementsByRoleDataTable tableAnalysisProjectRequirementsByRole;

		// Token: 0x04000127 RID: 295
		private DataRelation relationFK_Analysis_AnalysisProjects;

		// Token: 0x04000128 RID: 296
		private DataRelation relationFK_Analysis_AnalysisPriorityData;

		// Token: 0x04000129 RID: 297
		private DataRelation relationFK_AnalysisProjects_AnalysisProjectImpact;

		// Token: 0x0400012A RID: 298
		private DataRelation relationFK_Analysis_AnalysisOptimizerSolutions;

		// Token: 0x0400012B RID: 299
		private DataRelation relationFK_Analysis_AnalysisPlannerSolutions;

		// Token: 0x0400012C RID: 300
		private SchemaSerializationMode _schemaSerializationMode = SchemaSerializationMode.IncludeSchema;

		// Token: 0x02000044 RID: 68
		// (Invoke) Token: 0x06000426 RID: 1062
		[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
		public delegate void AnalysisRowChangeEventHandler(object sender, AnalysisDataSet.AnalysisRowChangeEvent e);

		// Token: 0x02000045 RID: 69
		// (Invoke) Token: 0x0600042A RID: 1066
		[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
		public delegate void AnalysisProjectsRowChangeEventHandler(object sender, AnalysisDataSet.AnalysisProjectsRowChangeEvent e);

		// Token: 0x02000046 RID: 70
		// (Invoke) Token: 0x0600042E RID: 1070
		[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
		public delegate void AnalysisPriorityDataRowChangeEventHandler(object sender, AnalysisDataSet.AnalysisPriorityDataRowChangeEvent e);

		// Token: 0x02000047 RID: 71
		// (Invoke) Token: 0x06000432 RID: 1074
		[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
		public delegate void AnalysisProjectImpactRowChangeEventHandler(object sender, AnalysisDataSet.AnalysisProjectImpactRowChangeEvent e);

		// Token: 0x02000048 RID: 72
		// (Invoke) Token: 0x06000436 RID: 1078
		[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
		public delegate void AnalysisOptimizerSolutionsRowChangeEventHandler(object sender, AnalysisDataSet.AnalysisOptimizerSolutionsRowChangeEvent e);

		// Token: 0x02000049 RID: 73
		// (Invoke) Token: 0x0600043A RID: 1082
		[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
		public delegate void AnalysisPlannerSolutionsRowChangeEventHandler(object sender, AnalysisDataSet.AnalysisPlannerSolutionsRowChangeEvent e);

		// Token: 0x0200004A RID: 74
		// (Invoke) Token: 0x0600043E RID: 1086
		[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
		public delegate void AnalysisRemainingCapacityByRoleRowChangeEventHandler(object sender, AnalysisDataSet.AnalysisRemainingCapacityByRoleRowChangeEvent e);

		// Token: 0x0200004B RID: 75
		// (Invoke) Token: 0x06000442 RID: 1090
		[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
		public delegate void AnalysisRoleRatesRowChangeEventHandler(object sender, AnalysisDataSet.AnalysisRoleRatesRowChangeEvent e);

		// Token: 0x0200004C RID: 76
		// (Invoke) Token: 0x06000446 RID: 1094
		[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
		public delegate void AnalysisProjectRequirementsByRoleRowChangeEventHandler(object sender, AnalysisDataSet.AnalysisProjectRequirementsByRoleRowChangeEvent e);

		// Token: 0x0200004D RID: 77
		[XmlSchemaProvider("GetTypedTableSchema")]
		[Serializable]
		public class AnalysisDataTable : DataTable, IEnumerable
		{
			// Token: 0x06000449 RID: 1097 RVA: 0x0000F778 File Offset: 0x0000D978
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public AnalysisDataTable()
			{
				base.TableName = "Analysis";
				this.BeginInit();
				this.InitClass();
				this.EndInit();
			}

			// Token: 0x0600044A RID: 1098 RVA: 0x0000F7A0 File Offset: 0x0000D9A0
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			internal AnalysisDataTable(DataTable table)
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

			// Token: 0x0600044B RID: 1099 RVA: 0x0000F848 File Offset: 0x0000DA48
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			protected AnalysisDataTable(SerializationInfo info, StreamingContext context) : base(info, context)
			{
				this.InitVars();
			}

			// Token: 0x17000123 RID: 291
			// (get) Token: 0x0600044C RID: 1100 RVA: 0x0000F858 File Offset: 0x0000DA58
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			public DataColumn ANALYSIS_UIDColumn
			{
				get
				{
					return this.columnANALYSIS_UID;
				}
			}

			// Token: 0x17000124 RID: 292
			// (get) Token: 0x0600044D RID: 1101 RVA: 0x0000F860 File Offset: 0x0000DA60
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public DataColumn ANALYSIS_NAMEColumn
			{
				get
				{
					return this.columnANALYSIS_NAME;
				}
			}

			// Token: 0x17000125 RID: 293
			// (get) Token: 0x0600044E RID: 1102 RVA: 0x0000F868 File Offset: 0x0000DA68
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			public DataColumn ANALYSIS_DESCRIPTIONColumn
			{
				get
				{
					return this.columnANALYSIS_DESCRIPTION;
				}
			}

			// Token: 0x17000126 RID: 294
			// (get) Token: 0x0600044F RID: 1103 RVA: 0x0000F870 File Offset: 0x0000DA70
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			public DataColumn ANALYSIS_TYPEColumn
			{
				get
				{
					return this.columnANALYSIS_TYPE;
				}
			}

			// Token: 0x17000127 RID: 295
			// (get) Token: 0x06000450 RID: 1104 RVA: 0x0000F878 File Offset: 0x0000DA78
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public DataColumn DEPARTMENT_UIDColumn
			{
				get
				{
					return this.columnDEPARTMENT_UID;
				}
			}

			// Token: 0x17000128 RID: 296
			// (get) Token: 0x06000451 RID: 1105 RVA: 0x0000F880 File Offset: 0x0000DA80
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			public DataColumn DEPARTMENT_NAMEColumn
			{
				get
				{
					return this.columnDEPARTMENT_NAME;
				}
			}

			// Token: 0x17000129 RID: 297
			// (get) Token: 0x06000452 RID: 1106 RVA: 0x0000F888 File Offset: 0x0000DA88
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			public DataColumn PRIORITIZATION_TYPEColumn
			{
				get
				{
					return this.columnPRIORITIZATION_TYPE;
				}
			}

			// Token: 0x1700012A RID: 298
			// (get) Token: 0x06000453 RID: 1107 RVA: 0x0000F890 File Offset: 0x0000DA90
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public DataColumn PRIORITIZATION_UIDColumn
			{
				get
				{
					return this.columnPRIORITIZATION_UID;
				}
			}

			// Token: 0x1700012B RID: 299
			// (get) Token: 0x06000454 RID: 1108 RVA: 0x0000F898 File Offset: 0x0000DA98
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			public DataColumn PROJECT_IMPACT_CF_UIDColumn
			{
				get
				{
					return this.columnPROJECT_IMPACT_CF_UID;
				}
			}

			// Token: 0x1700012C RID: 300
			// (get) Token: 0x06000455 RID: 1109 RVA: 0x0000F8A0 File Offset: 0x0000DAA0
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public DataColumn PROJECT_IMPACT_CF_NAMEColumn
			{
				get
				{
					return this.columnPROJECT_IMPACT_CF_NAME;
				}
			}

			// Token: 0x1700012D RID: 301
			// (get) Token: 0x06000456 RID: 1110 RVA: 0x0000F8A8 File Offset: 0x0000DAA8
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public DataColumn HARD_CONSTRAINT_CF_UIDColumn
			{
				get
				{
					return this.columnHARD_CONSTRAINT_CF_UID;
				}
			}

			// Token: 0x1700012E RID: 302
			// (get) Token: 0x06000457 RID: 1111 RVA: 0x0000F8B0 File Offset: 0x0000DAB0
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			public DataColumn HARD_CONSTRAINT_CF_NAMEColumn
			{
				get
				{
					return this.columnHARD_CONSTRAINT_CF_NAME;
				}
			}

			// Token: 0x1700012F RID: 303
			// (get) Token: 0x06000458 RID: 1112 RVA: 0x0000F8B8 File Offset: 0x0000DAB8
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			public DataColumn HORIZON_START_DATEColumn
			{
				get
				{
					return this.columnHORIZON_START_DATE;
				}
			}

			// Token: 0x17000130 RID: 304
			// (get) Token: 0x06000459 RID: 1113 RVA: 0x0000F8C0 File Offset: 0x0000DAC0
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public DataColumn HORIZON_END_DATEColumn
			{
				get
				{
					return this.columnHORIZON_END_DATE;
				}
			}

			// Token: 0x17000131 RID: 305
			// (get) Token: 0x0600045A RID: 1114 RVA: 0x0000F8C8 File Offset: 0x0000DAC8
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public DataColumn ROLE_CUSTOM_FIELD_UIDColumn
			{
				get
				{
					return this.columnROLE_CUSTOM_FIELD_UID;
				}
			}

			// Token: 0x17000132 RID: 306
			// (get) Token: 0x0600045B RID: 1115 RVA: 0x0000F8D0 File Offset: 0x0000DAD0
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public DataColumn TIME_SCALEColumn
			{
				get
				{
					return this.columnTIME_SCALE;
				}
			}

			// Token: 0x17000133 RID: 307
			// (get) Token: 0x0600045C RID: 1116 RVA: 0x0000F8D8 File Offset: 0x0000DAD8
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			public DataColumn FILTER_RESOURCES_BY_DEPColumn
			{
				get
				{
					return this.columnFILTER_RESOURCES_BY_DEP;
				}
			}

			// Token: 0x17000134 RID: 308
			// (get) Token: 0x0600045D RID: 1117 RVA: 0x0000F8E0 File Offset: 0x0000DAE0
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			public DataColumn FILTER_RESOURCES_BY_RBSColumn
			{
				get
				{
					return this.columnFILTER_RESOURCES_BY_RBS;
				}
			}

			// Token: 0x17000135 RID: 309
			// (get) Token: 0x0600045E RID: 1118 RVA: 0x0000F8E8 File Offset: 0x0000DAE8
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			public DataColumn FILTER_RESOURCES_RBS_VALColumn
			{
				get
				{
					return this.columnFILTER_RESOURCES_RBS_VAL;
				}
			}

			// Token: 0x17000136 RID: 310
			// (get) Token: 0x0600045F RID: 1119 RVA: 0x0000F8F0 File Offset: 0x0000DAF0
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			public DataColumn FILTER_RESOURCES_RBS_NAMEColumn
			{
				get
				{
					return this.columnFILTER_RESOURCES_RBS_NAME;
				}
			}

			// Token: 0x17000137 RID: 311
			// (get) Token: 0x06000460 RID: 1120 RVA: 0x0000F8F8 File Offset: 0x0000DAF8
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public DataColumn USE_ALT_PROJ_DATES_FOR_RES_PLANColumn
			{
				get
				{
					return this.columnUSE_ALT_PROJ_DATES_FOR_RES_PLAN;
				}
			}

			// Token: 0x17000138 RID: 312
			// (get) Token: 0x06000461 RID: 1121 RVA: 0x0000F900 File Offset: 0x0000DB00
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public DataColumn ALT_PROJ_START_DATE_CF_UIDColumn
			{
				get
				{
					return this.columnALT_PROJ_START_DATE_CF_UID;
				}
			}

			// Token: 0x17000139 RID: 313
			// (get) Token: 0x06000462 RID: 1122 RVA: 0x0000F908 File Offset: 0x0000DB08
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public DataColumn ALT_PROJ_END_DATE_CF_UIDColumn
			{
				get
				{
					return this.columnALT_PROJ_END_DATE_CF_UID;
				}
			}

			// Token: 0x1700013A RID: 314
			// (get) Token: 0x06000463 RID: 1123 RVA: 0x0000F910 File Offset: 0x0000DB10
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public DataColumn BOOKING_TYPEColumn
			{
				get
				{
					return this.columnBOOKING_TYPE;
				}
			}

			// Token: 0x1700013B RID: 315
			// (get) Token: 0x06000464 RID: 1124 RVA: 0x0000F918 File Offset: 0x0000DB18
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			public DataColumn CREATED_DATEColumn
			{
				get
				{
					return this.columnCREATED_DATE;
				}
			}

			// Token: 0x1700013C RID: 316
			// (get) Token: 0x06000465 RID: 1125 RVA: 0x0000F920 File Offset: 0x0000DB20
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			public DataColumn MOD_DATEColumn
			{
				get
				{
					return this.columnMOD_DATE;
				}
			}

			// Token: 0x1700013D RID: 317
			// (get) Token: 0x06000466 RID: 1126 RVA: 0x0000F928 File Offset: 0x0000DB28
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			public DataColumn LAST_UPDATED_BY_RES_UIDColumn
			{
				get
				{
					return this.columnLAST_UPDATED_BY_RES_UID;
				}
			}

			// Token: 0x1700013E RID: 318
			// (get) Token: 0x06000467 RID: 1127 RVA: 0x0000F930 File Offset: 0x0000DB30
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public DataColumn LAST_UPDATED_BY_RES_NAMEColumn
			{
				get
				{
					return this.columnLAST_UPDATED_BY_RES_NAME;
				}
			}

			// Token: 0x1700013F RID: 319
			// (get) Token: 0x06000468 RID: 1128 RVA: 0x0000F938 File Offset: 0x0000DB38
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			public DataColumn CREATED_BY_RES_UIDColumn
			{
				get
				{
					return this.columnCREATED_BY_RES_UID;
				}
			}

			// Token: 0x17000140 RID: 320
			// (get) Token: 0x06000469 RID: 1129 RVA: 0x0000F940 File Offset: 0x0000DB40
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public DataColumn CREATED_BY_RES_NAMEColumn
			{
				get
				{
					return this.columnCREATED_BY_RES_NAME;
				}
			}

			// Token: 0x17000141 RID: 321
			// (get) Token: 0x0600046A RID: 1130 RVA: 0x0000F948 File Offset: 0x0000DB48
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public DataColumn FORCE_IN_ALIAS_LT_UIDColumn
			{
				get
				{
					return this.columnFORCE_IN_ALIAS_LT_UID;
				}
			}

			// Token: 0x17000142 RID: 322
			// (get) Token: 0x0600046B RID: 1131 RVA: 0x0000F950 File Offset: 0x0000DB50
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public DataColumn FORCE_OUT_ALIAS_LT_UIDColumn
			{
				get
				{
					return this.columnFORCE_OUT_ALIAS_LT_UID;
				}
			}

			// Token: 0x17000143 RID: 323
			// (get) Token: 0x0600046C RID: 1132 RVA: 0x0000F958 File Offset: 0x0000DB58
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

			// Token: 0x17000144 RID: 324
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			public AnalysisDataSet.AnalysisRow this[int index]
			{
				get
				{
					return (AnalysisDataSet.AnalysisRow)base.Rows[index];
				}
			}

			// Token: 0x14000029 RID: 41
			// (add) Token: 0x0600046E RID: 1134 RVA: 0x0000F978 File Offset: 0x0000DB78
			// (remove) Token: 0x0600046F RID: 1135 RVA: 0x0000F9B0 File Offset: 0x0000DBB0
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public event AnalysisDataSet.AnalysisRowChangeEventHandler AnalysisRowChanging;

			// Token: 0x1400002A RID: 42
			// (add) Token: 0x06000470 RID: 1136 RVA: 0x0000F9E8 File Offset: 0x0000DBE8
			// (remove) Token: 0x06000471 RID: 1137 RVA: 0x0000FA20 File Offset: 0x0000DC20
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public event AnalysisDataSet.AnalysisRowChangeEventHandler AnalysisRowChanged;

			// Token: 0x1400002B RID: 43
			// (add) Token: 0x06000472 RID: 1138 RVA: 0x0000FA58 File Offset: 0x0000DC58
			// (remove) Token: 0x06000473 RID: 1139 RVA: 0x0000FA90 File Offset: 0x0000DC90
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public event AnalysisDataSet.AnalysisRowChangeEventHandler AnalysisRowDeleting;

			// Token: 0x1400002C RID: 44
			// (add) Token: 0x06000474 RID: 1140 RVA: 0x0000FAC8 File Offset: 0x0000DCC8
			// (remove) Token: 0x06000475 RID: 1141 RVA: 0x0000FB00 File Offset: 0x0000DD00
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public event AnalysisDataSet.AnalysisRowChangeEventHandler AnalysisRowDeleted;

			// Token: 0x06000476 RID: 1142 RVA: 0x0000FB35 File Offset: 0x0000DD35
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public void AddAnalysisRow(AnalysisDataSet.AnalysisRow row)
			{
				base.Rows.Add(row);
			}

			// Token: 0x06000477 RID: 1143 RVA: 0x0000FB44 File Offset: 0x0000DD44
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public AnalysisDataSet.AnalysisRow AddAnalysisRow(Guid ANALYSIS_UID, string ANALYSIS_NAME, string ANALYSIS_DESCRIPTION, int ANALYSIS_TYPE, Guid DEPARTMENT_UID, string DEPARTMENT_NAME, int PRIORITIZATION_TYPE, Guid PRIORITIZATION_UID, Guid PROJECT_IMPACT_CF_UID, string PROJECT_IMPACT_CF_NAME, Guid HARD_CONSTRAINT_CF_UID, string HARD_CONSTRAINT_CF_NAME, DateTime HORIZON_START_DATE, DateTime HORIZON_END_DATE, Guid ROLE_CUSTOM_FIELD_UID, byte TIME_SCALE, bool FILTER_RESOURCES_BY_DEP, bool FILTER_RESOURCES_BY_RBS, Guid FILTER_RESOURCES_RBS_VAL, string FILTER_RESOURCES_RBS_NAME, bool USE_ALT_PROJ_DATES_FOR_RES_PLAN, Guid ALT_PROJ_START_DATE_CF_UID, Guid ALT_PROJ_END_DATE_CF_UID, int BOOKING_TYPE, DateTime CREATED_DATE, DateTime MOD_DATE, Guid LAST_UPDATED_BY_RES_UID, string LAST_UPDATED_BY_RES_NAME, Guid CREATED_BY_RES_UID, string CREATED_BY_RES_NAME, Guid FORCE_IN_ALIAS_LT_UID, Guid FORCE_OUT_ALIAS_LT_UID)
			{
				AnalysisDataSet.AnalysisRow analysisRow = (AnalysisDataSet.AnalysisRow)base.NewRow();
				object[] itemArray = new object[]
				{
					ANALYSIS_UID,
					ANALYSIS_NAME,
					ANALYSIS_DESCRIPTION,
					ANALYSIS_TYPE,
					DEPARTMENT_UID,
					DEPARTMENT_NAME,
					PRIORITIZATION_TYPE,
					PRIORITIZATION_UID,
					PROJECT_IMPACT_CF_UID,
					PROJECT_IMPACT_CF_NAME,
					HARD_CONSTRAINT_CF_UID,
					HARD_CONSTRAINT_CF_NAME,
					HORIZON_START_DATE,
					HORIZON_END_DATE,
					ROLE_CUSTOM_FIELD_UID,
					TIME_SCALE,
					FILTER_RESOURCES_BY_DEP,
					FILTER_RESOURCES_BY_RBS,
					FILTER_RESOURCES_RBS_VAL,
					FILTER_RESOURCES_RBS_NAME,
					USE_ALT_PROJ_DATES_FOR_RES_PLAN,
					ALT_PROJ_START_DATE_CF_UID,
					ALT_PROJ_END_DATE_CF_UID,
					BOOKING_TYPE,
					CREATED_DATE,
					MOD_DATE,
					LAST_UPDATED_BY_RES_UID,
					LAST_UPDATED_BY_RES_NAME,
					CREATED_BY_RES_UID,
					CREATED_BY_RES_NAME,
					FORCE_IN_ALIAS_LT_UID,
					FORCE_OUT_ALIAS_LT_UID
				};
				analysisRow.ItemArray = itemArray;
				base.Rows.Add(analysisRow);
				return analysisRow;
			}

			// Token: 0x06000478 RID: 1144 RVA: 0x0000FCA8 File Offset: 0x0000DEA8
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			public AnalysisDataSet.AnalysisRow FindByANALYSIS_UID(Guid ANALYSIS_UID)
			{
				return (AnalysisDataSet.AnalysisRow)base.Rows.Find(new object[]
				{
					ANALYSIS_UID
				});
			}

			// Token: 0x06000479 RID: 1145 RVA: 0x0000FCD6 File Offset: 0x0000DED6
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public virtual IEnumerator GetEnumerator()
			{
				return base.Rows.GetEnumerator();
			}

			// Token: 0x0600047A RID: 1146 RVA: 0x0000FCE4 File Offset: 0x0000DEE4
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public override DataTable Clone()
			{
				AnalysisDataSet.AnalysisDataTable analysisDataTable = (AnalysisDataSet.AnalysisDataTable)base.Clone();
				analysisDataTable.InitVars();
				return analysisDataTable;
			}

			// Token: 0x0600047B RID: 1147 RVA: 0x0000FD04 File Offset: 0x0000DF04
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			protected override DataTable CreateInstance()
			{
				return new AnalysisDataSet.AnalysisDataTable();
			}

			// Token: 0x0600047C RID: 1148 RVA: 0x0000FD0C File Offset: 0x0000DF0C
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			internal void InitVars()
			{
				this.columnANALYSIS_UID = base.Columns["ANALYSIS_UID"];
				this.columnANALYSIS_NAME = base.Columns["ANALYSIS_NAME"];
				this.columnANALYSIS_DESCRIPTION = base.Columns["ANALYSIS_DESCRIPTION"];
				this.columnANALYSIS_TYPE = base.Columns["ANALYSIS_TYPE"];
				this.columnDEPARTMENT_UID = base.Columns["DEPARTMENT_UID"];
				this.columnDEPARTMENT_NAME = base.Columns["DEPARTMENT_NAME"];
				this.columnPRIORITIZATION_TYPE = base.Columns["PRIORITIZATION_TYPE"];
				this.columnPRIORITIZATION_UID = base.Columns["PRIORITIZATION_UID"];
				this.columnPROJECT_IMPACT_CF_UID = base.Columns["PROJECT_IMPACT_CF_UID"];
				this.columnPROJECT_IMPACT_CF_NAME = base.Columns["PROJECT_IMPACT_CF_NAME"];
				this.columnHARD_CONSTRAINT_CF_UID = base.Columns["HARD_CONSTRAINT_CF_UID"];
				this.columnHARD_CONSTRAINT_CF_NAME = base.Columns["HARD_CONSTRAINT_CF_NAME"];
				this.columnHORIZON_START_DATE = base.Columns["HORIZON_START_DATE"];
				this.columnHORIZON_END_DATE = base.Columns["HORIZON_END_DATE"];
				this.columnROLE_CUSTOM_FIELD_UID = base.Columns["ROLE_CUSTOM_FIELD_UID"];
				this.columnTIME_SCALE = base.Columns["TIME_SCALE"];
				this.columnFILTER_RESOURCES_BY_DEP = base.Columns["FILTER_RESOURCES_BY_DEP"];
				this.columnFILTER_RESOURCES_BY_RBS = base.Columns["FILTER_RESOURCES_BY_RBS"];
				this.columnFILTER_RESOURCES_RBS_VAL = base.Columns["FILTER_RESOURCES_RBS_VAL"];
				this.columnFILTER_RESOURCES_RBS_NAME = base.Columns["FILTER_RESOURCES_RBS_NAME"];
				this.columnUSE_ALT_PROJ_DATES_FOR_RES_PLAN = base.Columns["USE_ALT_PROJ_DATES_FOR_RES_PLAN"];
				this.columnALT_PROJ_START_DATE_CF_UID = base.Columns["ALT_PROJ_START_DATE_CF_UID"];
				this.columnALT_PROJ_END_DATE_CF_UID = base.Columns["ALT_PROJ_END_DATE_CF_UID"];
				this.columnBOOKING_TYPE = base.Columns["BOOKING_TYPE"];
				this.columnCREATED_DATE = base.Columns["CREATED_DATE"];
				this.columnMOD_DATE = base.Columns["MOD_DATE"];
				this.columnLAST_UPDATED_BY_RES_UID = base.Columns["LAST_UPDATED_BY_RES_UID"];
				this.columnLAST_UPDATED_BY_RES_NAME = base.Columns["LAST_UPDATED_BY_RES_NAME"];
				this.columnCREATED_BY_RES_UID = base.Columns["CREATED_BY_RES_UID"];
				this.columnCREATED_BY_RES_NAME = base.Columns["CREATED_BY_RES_NAME"];
				this.columnFORCE_IN_ALIAS_LT_UID = base.Columns["FORCE_IN_ALIAS_LT_UID"];
				this.columnFORCE_OUT_ALIAS_LT_UID = base.Columns["FORCE_OUT_ALIAS_LT_UID"];
			}

			// Token: 0x0600047D RID: 1149 RVA: 0x0000FFDC File Offset: 0x0000E1DC
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			private void InitClass()
			{
				this.columnANALYSIS_UID = new DataColumn("ANALYSIS_UID", typeof(Guid), null, MappingType.Element);
				base.Columns.Add(this.columnANALYSIS_UID);
				this.columnANALYSIS_NAME = new DataColumn("ANALYSIS_NAME", typeof(string), null, MappingType.Element);
				base.Columns.Add(this.columnANALYSIS_NAME);
				this.columnANALYSIS_DESCRIPTION = new DataColumn("ANALYSIS_DESCRIPTION", typeof(string), null, MappingType.Element);
				base.Columns.Add(this.columnANALYSIS_DESCRIPTION);
				this.columnANALYSIS_TYPE = new DataColumn("ANALYSIS_TYPE", typeof(int), null, MappingType.Element);
				base.Columns.Add(this.columnANALYSIS_TYPE);
				this.columnDEPARTMENT_UID = new DataColumn("DEPARTMENT_UID", typeof(Guid), null, MappingType.Element);
				base.Columns.Add(this.columnDEPARTMENT_UID);
				this.columnDEPARTMENT_NAME = new DataColumn("DEPARTMENT_NAME", typeof(string), null, MappingType.Element);
				base.Columns.Add(this.columnDEPARTMENT_NAME);
				this.columnPRIORITIZATION_TYPE = new DataColumn("PRIORITIZATION_TYPE", typeof(int), null, MappingType.Element);
				base.Columns.Add(this.columnPRIORITIZATION_TYPE);
				this.columnPRIORITIZATION_UID = new DataColumn("PRIORITIZATION_UID", typeof(Guid), null, MappingType.Element);
				base.Columns.Add(this.columnPRIORITIZATION_UID);
				this.columnPROJECT_IMPACT_CF_UID = new DataColumn("PROJECT_IMPACT_CF_UID", typeof(Guid), null, MappingType.Element);
				base.Columns.Add(this.columnPROJECT_IMPACT_CF_UID);
				this.columnPROJECT_IMPACT_CF_NAME = new DataColumn("PROJECT_IMPACT_CF_NAME", typeof(string), null, MappingType.Element);
				base.Columns.Add(this.columnPROJECT_IMPACT_CF_NAME);
				this.columnHARD_CONSTRAINT_CF_UID = new DataColumn("HARD_CONSTRAINT_CF_UID", typeof(Guid), null, MappingType.Element);
				base.Columns.Add(this.columnHARD_CONSTRAINT_CF_UID);
				this.columnHARD_CONSTRAINT_CF_NAME = new DataColumn("HARD_CONSTRAINT_CF_NAME", typeof(string), null, MappingType.Element);
				base.Columns.Add(this.columnHARD_CONSTRAINT_CF_NAME);
				this.columnHORIZON_START_DATE = new DataColumn("HORIZON_START_DATE", typeof(DateTime), null, MappingType.Element);
				base.Columns.Add(this.columnHORIZON_START_DATE);
				this.columnHORIZON_END_DATE = new DataColumn("HORIZON_END_DATE", typeof(DateTime), null, MappingType.Element);
				base.Columns.Add(this.columnHORIZON_END_DATE);
				this.columnROLE_CUSTOM_FIELD_UID = new DataColumn("ROLE_CUSTOM_FIELD_UID", typeof(Guid), null, MappingType.Element);
				base.Columns.Add(this.columnROLE_CUSTOM_FIELD_UID);
				this.columnTIME_SCALE = new DataColumn("TIME_SCALE", typeof(byte), null, MappingType.Element);
				base.Columns.Add(this.columnTIME_SCALE);
				this.columnFILTER_RESOURCES_BY_DEP = new DataColumn("FILTER_RESOURCES_BY_DEP", typeof(bool), null, MappingType.Element);
				base.Columns.Add(this.columnFILTER_RESOURCES_BY_DEP);
				this.columnFILTER_RESOURCES_BY_RBS = new DataColumn("FILTER_RESOURCES_BY_RBS", typeof(bool), null, MappingType.Element);
				base.Columns.Add(this.columnFILTER_RESOURCES_BY_RBS);
				this.columnFILTER_RESOURCES_RBS_VAL = new DataColumn("FILTER_RESOURCES_RBS_VAL", typeof(Guid), null, MappingType.Element);
				base.Columns.Add(this.columnFILTER_RESOURCES_RBS_VAL);
				this.columnFILTER_RESOURCES_RBS_NAME = new DataColumn("FILTER_RESOURCES_RBS_NAME", typeof(string), null, MappingType.Element);
				base.Columns.Add(this.columnFILTER_RESOURCES_RBS_NAME);
				this.columnUSE_ALT_PROJ_DATES_FOR_RES_PLAN = new DataColumn("USE_ALT_PROJ_DATES_FOR_RES_PLAN", typeof(bool), null, MappingType.Element);
				base.Columns.Add(this.columnUSE_ALT_PROJ_DATES_FOR_RES_PLAN);
				this.columnALT_PROJ_START_DATE_CF_UID = new DataColumn("ALT_PROJ_START_DATE_CF_UID", typeof(Guid), null, MappingType.Element);
				base.Columns.Add(this.columnALT_PROJ_START_DATE_CF_UID);
				this.columnALT_PROJ_END_DATE_CF_UID = new DataColumn("ALT_PROJ_END_DATE_CF_UID", typeof(Guid), null, MappingType.Element);
				base.Columns.Add(this.columnALT_PROJ_END_DATE_CF_UID);
				this.columnBOOKING_TYPE = new DataColumn("BOOKING_TYPE", typeof(int), null, MappingType.Element);
				base.Columns.Add(this.columnBOOKING_TYPE);
				this.columnCREATED_DATE = new DataColumn("CREATED_DATE", typeof(DateTime), null, MappingType.Element);
				base.Columns.Add(this.columnCREATED_DATE);
				this.columnMOD_DATE = new DataColumn("MOD_DATE", typeof(DateTime), null, MappingType.Element);
				base.Columns.Add(this.columnMOD_DATE);
				this.columnLAST_UPDATED_BY_RES_UID = new DataColumn("LAST_UPDATED_BY_RES_UID", typeof(Guid), null, MappingType.Element);
				base.Columns.Add(this.columnLAST_UPDATED_BY_RES_UID);
				this.columnLAST_UPDATED_BY_RES_NAME = new DataColumn("LAST_UPDATED_BY_RES_NAME", typeof(string), null, MappingType.Element);
				base.Columns.Add(this.columnLAST_UPDATED_BY_RES_NAME);
				this.columnCREATED_BY_RES_UID = new DataColumn("CREATED_BY_RES_UID", typeof(Guid), null, MappingType.Element);
				base.Columns.Add(this.columnCREATED_BY_RES_UID);
				this.columnCREATED_BY_RES_NAME = new DataColumn("CREATED_BY_RES_NAME", typeof(string), null, MappingType.Element);
				base.Columns.Add(this.columnCREATED_BY_RES_NAME);
				this.columnFORCE_IN_ALIAS_LT_UID = new DataColumn("FORCE_IN_ALIAS_LT_UID", typeof(Guid), null, MappingType.Element);
				base.Columns.Add(this.columnFORCE_IN_ALIAS_LT_UID);
				this.columnFORCE_OUT_ALIAS_LT_UID = new DataColumn("FORCE_OUT_ALIAS_LT_UID", typeof(Guid), null, MappingType.Element);
				base.Columns.Add(this.columnFORCE_OUT_ALIAS_LT_UID);
				base.Constraints.Add(new UniqueConstraint("Analysis_Constraint1", new DataColumn[]
				{
					this.columnANALYSIS_UID
				}, true));
				this.columnANALYSIS_UID.AllowDBNull = false;
				this.columnANALYSIS_UID.Unique = true;
				this.columnANALYSIS_NAME.AllowDBNull = false;
				this.columnANALYSIS_TYPE.AllowDBNull = false;
				this.columnDEPARTMENT_NAME.ReadOnly = true;
				this.columnPRIORITIZATION_TYPE.AllowDBNull = false;
				this.columnPROJECT_IMPACT_CF_NAME.ReadOnly = true;
				this.columnHARD_CONSTRAINT_CF_UID.AllowDBNull = false;
				this.columnHARD_CONSTRAINT_CF_NAME.ReadOnly = true;
				this.columnTIME_SCALE.AllowDBNull = false;
				this.columnTIME_SCALE.DefaultValue = 0;
				this.columnFILTER_RESOURCES_BY_DEP.DefaultValue = false;
				this.columnFILTER_RESOURCES_BY_RBS.DefaultValue = false;
				this.columnFILTER_RESOURCES_RBS_NAME.ReadOnly = true;
				this.columnUSE_ALT_PROJ_DATES_FOR_RES_PLAN.DefaultValue = false;
				this.columnBOOKING_TYPE.AllowDBNull = false;
				this.columnBOOKING_TYPE.DefaultValue = 0;
				this.columnLAST_UPDATED_BY_RES_NAME.ReadOnly = true;
				this.columnCREATED_BY_RES_NAME.ReadOnly = true;
			}

			// Token: 0x0600047E RID: 1150 RVA: 0x000106AD File Offset: 0x0000E8AD
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public AnalysisDataSet.AnalysisRow NewAnalysisRow()
			{
				return (AnalysisDataSet.AnalysisRow)base.NewRow();
			}

			// Token: 0x0600047F RID: 1151 RVA: 0x000106BA File Offset: 0x0000E8BA
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			protected override DataRow NewRowFromBuilder(DataRowBuilder builder)
			{
				return new AnalysisDataSet.AnalysisRow(builder);
			}

			// Token: 0x06000480 RID: 1152 RVA: 0x000106C2 File Offset: 0x0000E8C2
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			protected override Type GetRowType()
			{
				return typeof(AnalysisDataSet.AnalysisRow);
			}

			// Token: 0x06000481 RID: 1153 RVA: 0x000106CE File Offset: 0x0000E8CE
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			protected override void OnRowChanged(DataRowChangeEventArgs e)
			{
				base.OnRowChanged(e);
				if (this.AnalysisRowChanged != null)
				{
					this.AnalysisRowChanged(this, new AnalysisDataSet.AnalysisRowChangeEvent((AnalysisDataSet.AnalysisRow)e.Row, e.Action));
				}
			}

			// Token: 0x06000482 RID: 1154 RVA: 0x00010701 File Offset: 0x0000E901
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			protected override void OnRowChanging(DataRowChangeEventArgs e)
			{
				base.OnRowChanging(e);
				if (this.AnalysisRowChanging != null)
				{
					this.AnalysisRowChanging(this, new AnalysisDataSet.AnalysisRowChangeEvent((AnalysisDataSet.AnalysisRow)e.Row, e.Action));
				}
			}

			// Token: 0x06000483 RID: 1155 RVA: 0x00010734 File Offset: 0x0000E934
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			protected override void OnRowDeleted(DataRowChangeEventArgs e)
			{
				base.OnRowDeleted(e);
				if (this.AnalysisRowDeleted != null)
				{
					this.AnalysisRowDeleted(this, new AnalysisDataSet.AnalysisRowChangeEvent((AnalysisDataSet.AnalysisRow)e.Row, e.Action));
				}
			}

			// Token: 0x06000484 RID: 1156 RVA: 0x00010767 File Offset: 0x0000E967
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			protected override void OnRowDeleting(DataRowChangeEventArgs e)
			{
				base.OnRowDeleting(e);
				if (this.AnalysisRowDeleting != null)
				{
					this.AnalysisRowDeleting(this, new AnalysisDataSet.AnalysisRowChangeEvent((AnalysisDataSet.AnalysisRow)e.Row, e.Action));
				}
			}

			// Token: 0x06000485 RID: 1157 RVA: 0x0001079A File Offset: 0x0000E99A
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			public void RemoveAnalysisRow(AnalysisDataSet.AnalysisRow row)
			{
				base.Rows.Remove(row);
			}

			// Token: 0x06000486 RID: 1158 RVA: 0x000107A8 File Offset: 0x0000E9A8
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public static XmlSchemaComplexType GetTypedTableSchema(XmlSchemaSet xs)
			{
				XmlSchemaComplexType xmlSchemaComplexType = new XmlSchemaComplexType();
				XmlSchemaSequence xmlSchemaSequence = new XmlSchemaSequence();
				AnalysisDataSet analysisDataSet = new AnalysisDataSet();
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
				xmlSchemaAttribute.FixedValue = analysisDataSet.Namespace;
				xmlSchemaComplexType.Attributes.Add(xmlSchemaAttribute);
				XmlSchemaAttribute xmlSchemaAttribute2 = new XmlSchemaAttribute();
				xmlSchemaAttribute2.Name = "tableTypeName";
				xmlSchemaAttribute2.FixedValue = "AnalysisDataTable";
				xmlSchemaComplexType.Attributes.Add(xmlSchemaAttribute2);
				xmlSchemaComplexType.Particle = xmlSchemaSequence;
				XmlSchema schemaSerializable = analysisDataSet.GetSchemaSerializable();
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

			// Token: 0x0400012D RID: 301
			private DataColumn columnANALYSIS_UID;

			// Token: 0x0400012E RID: 302
			private DataColumn columnANALYSIS_NAME;

			// Token: 0x0400012F RID: 303
			private DataColumn columnANALYSIS_DESCRIPTION;

			// Token: 0x04000130 RID: 304
			private DataColumn columnANALYSIS_TYPE;

			// Token: 0x04000131 RID: 305
			private DataColumn columnDEPARTMENT_UID;

			// Token: 0x04000132 RID: 306
			private DataColumn columnDEPARTMENT_NAME;

			// Token: 0x04000133 RID: 307
			private DataColumn columnPRIORITIZATION_TYPE;

			// Token: 0x04000134 RID: 308
			private DataColumn columnPRIORITIZATION_UID;

			// Token: 0x04000135 RID: 309
			private DataColumn columnPROJECT_IMPACT_CF_UID;

			// Token: 0x04000136 RID: 310
			private DataColumn columnPROJECT_IMPACT_CF_NAME;

			// Token: 0x04000137 RID: 311
			private DataColumn columnHARD_CONSTRAINT_CF_UID;

			// Token: 0x04000138 RID: 312
			private DataColumn columnHARD_CONSTRAINT_CF_NAME;

			// Token: 0x04000139 RID: 313
			private DataColumn columnHORIZON_START_DATE;

			// Token: 0x0400013A RID: 314
			private DataColumn columnHORIZON_END_DATE;

			// Token: 0x0400013B RID: 315
			private DataColumn columnROLE_CUSTOM_FIELD_UID;

			// Token: 0x0400013C RID: 316
			private DataColumn columnTIME_SCALE;

			// Token: 0x0400013D RID: 317
			private DataColumn columnFILTER_RESOURCES_BY_DEP;

			// Token: 0x0400013E RID: 318
			private DataColumn columnFILTER_RESOURCES_BY_RBS;

			// Token: 0x0400013F RID: 319
			private DataColumn columnFILTER_RESOURCES_RBS_VAL;

			// Token: 0x04000140 RID: 320
			private DataColumn columnFILTER_RESOURCES_RBS_NAME;

			// Token: 0x04000141 RID: 321
			private DataColumn columnUSE_ALT_PROJ_DATES_FOR_RES_PLAN;

			// Token: 0x04000142 RID: 322
			private DataColumn columnALT_PROJ_START_DATE_CF_UID;

			// Token: 0x04000143 RID: 323
			private DataColumn columnALT_PROJ_END_DATE_CF_UID;

			// Token: 0x04000144 RID: 324
			private DataColumn columnBOOKING_TYPE;

			// Token: 0x04000145 RID: 325
			private DataColumn columnCREATED_DATE;

			// Token: 0x04000146 RID: 326
			private DataColumn columnMOD_DATE;

			// Token: 0x04000147 RID: 327
			private DataColumn columnLAST_UPDATED_BY_RES_UID;

			// Token: 0x04000148 RID: 328
			private DataColumn columnLAST_UPDATED_BY_RES_NAME;

			// Token: 0x04000149 RID: 329
			private DataColumn columnCREATED_BY_RES_UID;

			// Token: 0x0400014A RID: 330
			private DataColumn columnCREATED_BY_RES_NAME;

			// Token: 0x0400014B RID: 331
			private DataColumn columnFORCE_IN_ALIAS_LT_UID;

			// Token: 0x0400014C RID: 332
			private DataColumn columnFORCE_OUT_ALIAS_LT_UID;
		}

		// Token: 0x0200004E RID: 78
		[XmlSchemaProvider("GetTypedTableSchema")]
		[Serializable]
		public class AnalysisProjectsDataTable : DataTable, IEnumerable
		{
			// Token: 0x06000487 RID: 1159 RVA: 0x000109A0 File Offset: 0x0000EBA0
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			public AnalysisProjectsDataTable()
			{
				base.TableName = "AnalysisProjects";
				this.BeginInit();
				this.InitClass();
				this.EndInit();
			}

			// Token: 0x06000488 RID: 1160 RVA: 0x000109C8 File Offset: 0x0000EBC8
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			internal AnalysisProjectsDataTable(DataTable table)
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

			// Token: 0x06000489 RID: 1161 RVA: 0x00010A70 File Offset: 0x0000EC70
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			protected AnalysisProjectsDataTable(SerializationInfo info, StreamingContext context) : base(info, context)
			{
				this.InitVars();
			}

			// Token: 0x17000145 RID: 325
			// (get) Token: 0x0600048A RID: 1162 RVA: 0x00010A80 File Offset: 0x0000EC80
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public DataColumn ANALYSIS_UIDColumn
			{
				get
				{
					return this.columnANALYSIS_UID;
				}
			}

			// Token: 0x17000146 RID: 326
			// (get) Token: 0x0600048B RID: 1163 RVA: 0x00010A88 File Offset: 0x0000EC88
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			public DataColumn PROJ_UIDColumn
			{
				get
				{
					return this.columnPROJ_UID;
				}
			}

			// Token: 0x17000147 RID: 327
			// (get) Token: 0x0600048C RID: 1164 RVA: 0x00010A90 File Offset: 0x0000EC90
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public DataColumn PROJ_NAMEColumn
			{
				get
				{
					return this.columnPROJ_NAME;
				}
			}

			// Token: 0x17000148 RID: 328
			// (get) Token: 0x0600048D RID: 1165 RVA: 0x00010A98 File Offset: 0x0000EC98
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public DataColumn PRIORITYColumn
			{
				get
				{
					return this.columnPRIORITY;
				}
			}

			// Token: 0x17000149 RID: 329
			// (get) Token: 0x0600048E RID: 1166 RVA: 0x00010AA0 File Offset: 0x0000ECA0
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public DataColumn ABSOLUTE_PRIORITYColumn
			{
				get
				{
					return this.columnABSOLUTE_PRIORITY;
				}
			}

			// Token: 0x1700014A RID: 330
			// (get) Token: 0x0600048F RID: 1167 RVA: 0x00010AA8 File Offset: 0x0000ECA8
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			public DataColumn ORIGINAL_START_DATEColumn
			{
				get
				{
					return this.columnORIGINAL_START_DATE;
				}
			}

			// Token: 0x1700014B RID: 331
			// (get) Token: 0x06000490 RID: 1168 RVA: 0x00010AB0 File Offset: 0x0000ECB0
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			public DataColumn ORIGINAL_END_DATEColumn
			{
				get
				{
					return this.columnORIGINAL_END_DATE;
				}
			}

			// Token: 0x1700014C RID: 332
			// (get) Token: 0x06000491 RID: 1169 RVA: 0x00010AB8 File Offset: 0x0000ECB8
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public DataColumn START_DATEColumn
			{
				get
				{
					return this.columnSTART_DATE;
				}
			}

			// Token: 0x1700014D RID: 333
			// (get) Token: 0x06000492 RID: 1170 RVA: 0x00010AC0 File Offset: 0x0000ECC0
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public DataColumn DURATIONColumn
			{
				get
				{
					return this.columnDURATION;
				}
			}

			// Token: 0x1700014E RID: 334
			// (get) Token: 0x06000493 RID: 1171 RVA: 0x00010AC8 File Offset: 0x0000ECC8
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			public DataColumn SNETColumn
			{
				get
				{
					return this.columnSNET;
				}
			}

			// Token: 0x1700014F RID: 335
			// (get) Token: 0x06000494 RID: 1172 RVA: 0x00010AD0 File Offset: 0x0000ECD0
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			public DataColumn FNLTColumn
			{
				get
				{
					return this.columnFNLT;
				}
			}

			// Token: 0x17000150 RID: 336
			// (get) Token: 0x06000495 RID: 1173 RVA: 0x00010AD8 File Offset: 0x0000ECD8
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public DataColumn LOCKEDColumn
			{
				get
				{
					return this.columnLOCKED;
				}
			}

			// Token: 0x17000151 RID: 337
			// (get) Token: 0x06000496 RID: 1174 RVA: 0x00010AE0 File Offset: 0x0000ECE0
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

			// Token: 0x17000152 RID: 338
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			public AnalysisDataSet.AnalysisProjectsRow this[int index]
			{
				get
				{
					return (AnalysisDataSet.AnalysisProjectsRow)base.Rows[index];
				}
			}

			// Token: 0x1400002D RID: 45
			// (add) Token: 0x06000498 RID: 1176 RVA: 0x00010B00 File Offset: 0x0000ED00
			// (remove) Token: 0x06000499 RID: 1177 RVA: 0x00010B38 File Offset: 0x0000ED38
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public event AnalysisDataSet.AnalysisProjectsRowChangeEventHandler AnalysisProjectsRowChanging;

			// Token: 0x1400002E RID: 46
			// (add) Token: 0x0600049A RID: 1178 RVA: 0x00010B70 File Offset: 0x0000ED70
			// (remove) Token: 0x0600049B RID: 1179 RVA: 0x00010BA8 File Offset: 0x0000EDA8
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public event AnalysisDataSet.AnalysisProjectsRowChangeEventHandler AnalysisProjectsRowChanged;

			// Token: 0x1400002F RID: 47
			// (add) Token: 0x0600049C RID: 1180 RVA: 0x00010BE0 File Offset: 0x0000EDE0
			// (remove) Token: 0x0600049D RID: 1181 RVA: 0x00010C18 File Offset: 0x0000EE18
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public event AnalysisDataSet.AnalysisProjectsRowChangeEventHandler AnalysisProjectsRowDeleting;

			// Token: 0x14000030 RID: 48
			// (add) Token: 0x0600049E RID: 1182 RVA: 0x00010C50 File Offset: 0x0000EE50
			// (remove) Token: 0x0600049F RID: 1183 RVA: 0x00010C88 File Offset: 0x0000EE88
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public event AnalysisDataSet.AnalysisProjectsRowChangeEventHandler AnalysisProjectsRowDeleted;

			// Token: 0x060004A0 RID: 1184 RVA: 0x00010CBD File Offset: 0x0000EEBD
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			public void AddAnalysisProjectsRow(AnalysisDataSet.AnalysisProjectsRow row)
			{
				base.Rows.Add(row);
			}

			// Token: 0x060004A1 RID: 1185 RVA: 0x00010CCC File Offset: 0x0000EECC
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			public AnalysisDataSet.AnalysisProjectsRow AddAnalysisProjectsRow(AnalysisDataSet.AnalysisRow parentAnalysisRowByFK_Analysis_AnalysisProjects, Guid PROJ_UID, string PROJ_NAME, double PRIORITY, double ABSOLUTE_PRIORITY, DateTime ORIGINAL_START_DATE, DateTime ORIGINAL_END_DATE, DateTime START_DATE, int DURATION, DateTime SNET, DateTime FNLT, byte LOCKED)
			{
				AnalysisDataSet.AnalysisProjectsRow analysisProjectsRow = (AnalysisDataSet.AnalysisProjectsRow)base.NewRow();
				object[] array = new object[]
				{
					null,
					PROJ_UID,
					PROJ_NAME,
					PRIORITY,
					ABSOLUTE_PRIORITY,
					ORIGINAL_START_DATE,
					ORIGINAL_END_DATE,
					START_DATE,
					DURATION,
					SNET,
					FNLT,
					LOCKED
				};
				if (parentAnalysisRowByFK_Analysis_AnalysisProjects != null)
				{
					array[0] = parentAnalysisRowByFK_Analysis_AnalysisProjects[0];
				}
				analysisProjectsRow.ItemArray = array;
				base.Rows.Add(analysisProjectsRow);
				return analysisProjectsRow;
			}

			// Token: 0x060004A2 RID: 1186 RVA: 0x00010D7C File Offset: 0x0000EF7C
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public AnalysisDataSet.AnalysisProjectsRow FindByANALYSIS_UIDPROJ_UID(Guid ANALYSIS_UID, Guid PROJ_UID)
			{
				return (AnalysisDataSet.AnalysisProjectsRow)base.Rows.Find(new object[]
				{
					ANALYSIS_UID,
					PROJ_UID
				});
			}

			// Token: 0x060004A3 RID: 1187 RVA: 0x00010DB3 File Offset: 0x0000EFB3
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			public virtual IEnumerator GetEnumerator()
			{
				return base.Rows.GetEnumerator();
			}

			// Token: 0x060004A4 RID: 1188 RVA: 0x00010DC0 File Offset: 0x0000EFC0
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public override DataTable Clone()
			{
				AnalysisDataSet.AnalysisProjectsDataTable analysisProjectsDataTable = (AnalysisDataSet.AnalysisProjectsDataTable)base.Clone();
				analysisProjectsDataTable.InitVars();
				return analysisProjectsDataTable;
			}

			// Token: 0x060004A5 RID: 1189 RVA: 0x00010DE0 File Offset: 0x0000EFE0
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			protected override DataTable CreateInstance()
			{
				return new AnalysisDataSet.AnalysisProjectsDataTable();
			}

			// Token: 0x060004A6 RID: 1190 RVA: 0x00010DE8 File Offset: 0x0000EFE8
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			internal void InitVars()
			{
				this.columnANALYSIS_UID = base.Columns["ANALYSIS_UID"];
				this.columnPROJ_UID = base.Columns["PROJ_UID"];
				this.columnPROJ_NAME = base.Columns["PROJ_NAME"];
				this.columnPRIORITY = base.Columns["PRIORITY"];
				this.columnABSOLUTE_PRIORITY = base.Columns["ABSOLUTE_PRIORITY"];
				this.columnORIGINAL_START_DATE = base.Columns["ORIGINAL_START_DATE"];
				this.columnORIGINAL_END_DATE = base.Columns["ORIGINAL_END_DATE"];
				this.columnSTART_DATE = base.Columns["START_DATE"];
				this.columnDURATION = base.Columns["DURATION"];
				this.columnSNET = base.Columns["SNET"];
				this.columnFNLT = base.Columns["FNLT"];
				this.columnLOCKED = base.Columns["LOCKED"];
			}

			// Token: 0x060004A7 RID: 1191 RVA: 0x00010F00 File Offset: 0x0000F100
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			private void InitClass()
			{
				this.columnANALYSIS_UID = new DataColumn("ANALYSIS_UID", typeof(Guid), null, MappingType.Element);
				base.Columns.Add(this.columnANALYSIS_UID);
				this.columnPROJ_UID = new DataColumn("PROJ_UID", typeof(Guid), null, MappingType.Element);
				base.Columns.Add(this.columnPROJ_UID);
				this.columnPROJ_NAME = new DataColumn("PROJ_NAME", typeof(string), null, MappingType.Element);
				base.Columns.Add(this.columnPROJ_NAME);
				this.columnPRIORITY = new DataColumn("PRIORITY", typeof(double), null, MappingType.Element);
				base.Columns.Add(this.columnPRIORITY);
				this.columnABSOLUTE_PRIORITY = new DataColumn("ABSOLUTE_PRIORITY", typeof(double), null, MappingType.Element);
				base.Columns.Add(this.columnABSOLUTE_PRIORITY);
				this.columnORIGINAL_START_DATE = new DataColumn("ORIGINAL_START_DATE", typeof(DateTime), null, MappingType.Element);
				base.Columns.Add(this.columnORIGINAL_START_DATE);
				this.columnORIGINAL_END_DATE = new DataColumn("ORIGINAL_END_DATE", typeof(DateTime), null, MappingType.Element);
				base.Columns.Add(this.columnORIGINAL_END_DATE);
				this.columnSTART_DATE = new DataColumn("START_DATE", typeof(DateTime), null, MappingType.Element);
				base.Columns.Add(this.columnSTART_DATE);
				this.columnDURATION = new DataColumn("DURATION", typeof(int), null, MappingType.Element);
				base.Columns.Add(this.columnDURATION);
				this.columnSNET = new DataColumn("SNET", typeof(DateTime), null, MappingType.Element);
				base.Columns.Add(this.columnSNET);
				this.columnFNLT = new DataColumn("FNLT", typeof(DateTime), null, MappingType.Element);
				base.Columns.Add(this.columnFNLT);
				this.columnLOCKED = new DataColumn("LOCKED", typeof(byte), null, MappingType.Element);
				base.Columns.Add(this.columnLOCKED);
				base.Constraints.Add(new UniqueConstraint("AnalysisProjects_Constraint1", new DataColumn[]
				{
					this.columnANALYSIS_UID,
					this.columnPROJ_UID
				}, true));
				this.columnANALYSIS_UID.AllowDBNull = false;
				this.columnPROJ_UID.AllowDBNull = false;
				this.columnPROJ_NAME.ReadOnly = true;
				this.columnDURATION.AllowDBNull = false;
				this.columnDURATION.DefaultValue = 0;
				this.columnLOCKED.AllowDBNull = false;
				this.columnLOCKED.DefaultValue = 0;
			}

			// Token: 0x060004A8 RID: 1192 RVA: 0x000111B7 File Offset: 0x0000F3B7
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public AnalysisDataSet.AnalysisProjectsRow NewAnalysisProjectsRow()
			{
				return (AnalysisDataSet.AnalysisProjectsRow)base.NewRow();
			}

			// Token: 0x060004A9 RID: 1193 RVA: 0x000111C4 File Offset: 0x0000F3C4
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			protected override DataRow NewRowFromBuilder(DataRowBuilder builder)
			{
				return new AnalysisDataSet.AnalysisProjectsRow(builder);
			}

			// Token: 0x060004AA RID: 1194 RVA: 0x000111CC File Offset: 0x0000F3CC
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			protected override Type GetRowType()
			{
				return typeof(AnalysisDataSet.AnalysisProjectsRow);
			}

			// Token: 0x060004AB RID: 1195 RVA: 0x000111D8 File Offset: 0x0000F3D8
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			protected override void OnRowChanged(DataRowChangeEventArgs e)
			{
				base.OnRowChanged(e);
				if (this.AnalysisProjectsRowChanged != null)
				{
					this.AnalysisProjectsRowChanged(this, new AnalysisDataSet.AnalysisProjectsRowChangeEvent((AnalysisDataSet.AnalysisProjectsRow)e.Row, e.Action));
				}
			}

			// Token: 0x060004AC RID: 1196 RVA: 0x0001120B File Offset: 0x0000F40B
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			protected override void OnRowChanging(DataRowChangeEventArgs e)
			{
				base.OnRowChanging(e);
				if (this.AnalysisProjectsRowChanging != null)
				{
					this.AnalysisProjectsRowChanging(this, new AnalysisDataSet.AnalysisProjectsRowChangeEvent((AnalysisDataSet.AnalysisProjectsRow)e.Row, e.Action));
				}
			}

			// Token: 0x060004AD RID: 1197 RVA: 0x0001123E File Offset: 0x0000F43E
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			protected override void OnRowDeleted(DataRowChangeEventArgs e)
			{
				base.OnRowDeleted(e);
				if (this.AnalysisProjectsRowDeleted != null)
				{
					this.AnalysisProjectsRowDeleted(this, new AnalysisDataSet.AnalysisProjectsRowChangeEvent((AnalysisDataSet.AnalysisProjectsRow)e.Row, e.Action));
				}
			}

			// Token: 0x060004AE RID: 1198 RVA: 0x00011271 File Offset: 0x0000F471
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			protected override void OnRowDeleting(DataRowChangeEventArgs e)
			{
				base.OnRowDeleting(e);
				if (this.AnalysisProjectsRowDeleting != null)
				{
					this.AnalysisProjectsRowDeleting(this, new AnalysisDataSet.AnalysisProjectsRowChangeEvent((AnalysisDataSet.AnalysisProjectsRow)e.Row, e.Action));
				}
			}

			// Token: 0x060004AF RID: 1199 RVA: 0x000112A4 File Offset: 0x0000F4A4
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public void RemoveAnalysisProjectsRow(AnalysisDataSet.AnalysisProjectsRow row)
			{
				base.Rows.Remove(row);
			}

			// Token: 0x060004B0 RID: 1200 RVA: 0x000112B4 File Offset: 0x0000F4B4
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public static XmlSchemaComplexType GetTypedTableSchema(XmlSchemaSet xs)
			{
				XmlSchemaComplexType xmlSchemaComplexType = new XmlSchemaComplexType();
				XmlSchemaSequence xmlSchemaSequence = new XmlSchemaSequence();
				AnalysisDataSet analysisDataSet = new AnalysisDataSet();
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
				xmlSchemaAttribute.FixedValue = analysisDataSet.Namespace;
				xmlSchemaComplexType.Attributes.Add(xmlSchemaAttribute);
				XmlSchemaAttribute xmlSchemaAttribute2 = new XmlSchemaAttribute();
				xmlSchemaAttribute2.Name = "tableTypeName";
				xmlSchemaAttribute2.FixedValue = "AnalysisProjectsDataTable";
				xmlSchemaComplexType.Attributes.Add(xmlSchemaAttribute2);
				xmlSchemaComplexType.Particle = xmlSchemaSequence;
				XmlSchema schemaSerializable = analysisDataSet.GetSchemaSerializable();
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

			// Token: 0x04000151 RID: 337
			private DataColumn columnANALYSIS_UID;

			// Token: 0x04000152 RID: 338
			private DataColumn columnPROJ_UID;

			// Token: 0x04000153 RID: 339
			private DataColumn columnPROJ_NAME;

			// Token: 0x04000154 RID: 340
			private DataColumn columnPRIORITY;

			// Token: 0x04000155 RID: 341
			private DataColumn columnABSOLUTE_PRIORITY;

			// Token: 0x04000156 RID: 342
			private DataColumn columnORIGINAL_START_DATE;

			// Token: 0x04000157 RID: 343
			private DataColumn columnORIGINAL_END_DATE;

			// Token: 0x04000158 RID: 344
			private DataColumn columnSTART_DATE;

			// Token: 0x04000159 RID: 345
			private DataColumn columnDURATION;

			// Token: 0x0400015A RID: 346
			private DataColumn columnSNET;

			// Token: 0x0400015B RID: 347
			private DataColumn columnFNLT;

			// Token: 0x0400015C RID: 348
			private DataColumn columnLOCKED;
		}

		// Token: 0x0200004F RID: 79
		[XmlSchemaProvider("GetTypedTableSchema")]
		[Serializable]
		public class AnalysisPriorityDataDataTable : DataTable, IEnumerable
		{
			// Token: 0x060004B1 RID: 1201 RVA: 0x000114AC File Offset: 0x0000F6AC
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public AnalysisPriorityDataDataTable()
			{
				base.TableName = "AnalysisPriorityData";
				this.BeginInit();
				this.InitClass();
				this.EndInit();
			}

			// Token: 0x060004B2 RID: 1202 RVA: 0x000114D4 File Offset: 0x0000F6D4
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			internal AnalysisPriorityDataDataTable(DataTable table)
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

			// Token: 0x060004B3 RID: 1203 RVA: 0x0001157C File Offset: 0x0000F77C
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			protected AnalysisPriorityDataDataTable(SerializationInfo info, StreamingContext context) : base(info, context)
			{
				this.InitVars();
			}

			// Token: 0x17000153 RID: 339
			// (get) Token: 0x060004B4 RID: 1204 RVA: 0x0001158C File Offset: 0x0000F78C
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			public DataColumn ANALYSIS_UIDColumn
			{
				get
				{
					return this.columnANALYSIS_UID;
				}
			}

			// Token: 0x17000154 RID: 340
			// (get) Token: 0x060004B5 RID: 1205 RVA: 0x00011594 File Offset: 0x0000F794
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public DataColumn MD_PROP_UIDColumn
			{
				get
				{
					return this.columnMD_PROP_UID;
				}
			}

			// Token: 0x17000155 RID: 341
			// (get) Token: 0x060004B6 RID: 1206 RVA: 0x0001159C File Offset: 0x0000F79C
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public DataColumn MD_PROP_NAMEColumn
			{
				get
				{
					return this.columnMD_PROP_NAME;
				}
			}

			// Token: 0x17000156 RID: 342
			// (get) Token: 0x060004B7 RID: 1207 RVA: 0x000115A4 File Offset: 0x0000F7A4
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public DataColumn WEIGHTColumn
			{
				get
				{
					return this.columnWEIGHT;
				}
			}

			// Token: 0x17000157 RID: 343
			// (get) Token: 0x060004B8 RID: 1208 RVA: 0x000115AC File Offset: 0x0000F7AC
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			public DataColumn MIN_VALUEColumn
			{
				get
				{
					return this.columnMIN_VALUE;
				}
			}

			// Token: 0x17000158 RID: 344
			// (get) Token: 0x060004B9 RID: 1209 RVA: 0x000115B4 File Offset: 0x0000F7B4
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public DataColumn MAX_VALUEColumn
			{
				get
				{
					return this.columnMAX_VALUE;
				}
			}

			// Token: 0x17000159 RID: 345
			// (get) Token: 0x060004BA RID: 1210 RVA: 0x000115BC File Offset: 0x0000F7BC
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

			// Token: 0x1700015A RID: 346
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			public AnalysisDataSet.AnalysisPriorityDataRow this[int index]
			{
				get
				{
					return (AnalysisDataSet.AnalysisPriorityDataRow)base.Rows[index];
				}
			}

			// Token: 0x14000031 RID: 49
			// (add) Token: 0x060004BC RID: 1212 RVA: 0x000115DC File Offset: 0x0000F7DC
			// (remove) Token: 0x060004BD RID: 1213 RVA: 0x00011614 File Offset: 0x0000F814
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public event AnalysisDataSet.AnalysisPriorityDataRowChangeEventHandler AnalysisPriorityDataRowChanging;

			// Token: 0x14000032 RID: 50
			// (add) Token: 0x060004BE RID: 1214 RVA: 0x0001164C File Offset: 0x0000F84C
			// (remove) Token: 0x060004BF RID: 1215 RVA: 0x00011684 File Offset: 0x0000F884
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public event AnalysisDataSet.AnalysisPriorityDataRowChangeEventHandler AnalysisPriorityDataRowChanged;

			// Token: 0x14000033 RID: 51
			// (add) Token: 0x060004C0 RID: 1216 RVA: 0x000116BC File Offset: 0x0000F8BC
			// (remove) Token: 0x060004C1 RID: 1217 RVA: 0x000116F4 File Offset: 0x0000F8F4
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public event AnalysisDataSet.AnalysisPriorityDataRowChangeEventHandler AnalysisPriorityDataRowDeleting;

			// Token: 0x14000034 RID: 52
			// (add) Token: 0x060004C2 RID: 1218 RVA: 0x0001172C File Offset: 0x0000F92C
			// (remove) Token: 0x060004C3 RID: 1219 RVA: 0x00011764 File Offset: 0x0000F964
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public event AnalysisDataSet.AnalysisPriorityDataRowChangeEventHandler AnalysisPriorityDataRowDeleted;

			// Token: 0x060004C4 RID: 1220 RVA: 0x00011799 File Offset: 0x0000F999
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public void AddAnalysisPriorityDataRow(AnalysisDataSet.AnalysisPriorityDataRow row)
			{
				base.Rows.Add(row);
			}

			// Token: 0x060004C5 RID: 1221 RVA: 0x000117A8 File Offset: 0x0000F9A8
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public AnalysisDataSet.AnalysisPriorityDataRow AddAnalysisPriorityDataRow(AnalysisDataSet.AnalysisRow parentAnalysisRowByFK_Analysis_AnalysisPriorityData, Guid MD_PROP_UID, string MD_PROP_NAME, double WEIGHT, decimal MIN_VALUE, decimal MAX_VALUE)
			{
				AnalysisDataSet.AnalysisPriorityDataRow analysisPriorityDataRow = (AnalysisDataSet.AnalysisPriorityDataRow)base.NewRow();
				object[] array = new object[]
				{
					null,
					MD_PROP_UID,
					MD_PROP_NAME,
					WEIGHT,
					MIN_VALUE,
					MAX_VALUE
				};
				if (parentAnalysisRowByFK_Analysis_AnalysisPriorityData != null)
				{
					array[0] = parentAnalysisRowByFK_Analysis_AnalysisPriorityData[0];
				}
				analysisPriorityDataRow.ItemArray = array;
				base.Rows.Add(analysisPriorityDataRow);
				return analysisPriorityDataRow;
			}

			// Token: 0x060004C6 RID: 1222 RVA: 0x00011818 File Offset: 0x0000FA18
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public AnalysisDataSet.AnalysisPriorityDataRow FindByANALYSIS_UIDMD_PROP_UID(Guid ANALYSIS_UID, Guid MD_PROP_UID)
			{
				return (AnalysisDataSet.AnalysisPriorityDataRow)base.Rows.Find(new object[]
				{
					ANALYSIS_UID,
					MD_PROP_UID
				});
			}

			// Token: 0x060004C7 RID: 1223 RVA: 0x0001184F File Offset: 0x0000FA4F
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public virtual IEnumerator GetEnumerator()
			{
				return base.Rows.GetEnumerator();
			}

			// Token: 0x060004C8 RID: 1224 RVA: 0x0001185C File Offset: 0x0000FA5C
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			public override DataTable Clone()
			{
				AnalysisDataSet.AnalysisPriorityDataDataTable analysisPriorityDataDataTable = (AnalysisDataSet.AnalysisPriorityDataDataTable)base.Clone();
				analysisPriorityDataDataTable.InitVars();
				return analysisPriorityDataDataTable;
			}

			// Token: 0x060004C9 RID: 1225 RVA: 0x0001187C File Offset: 0x0000FA7C
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			protected override DataTable CreateInstance()
			{
				return new AnalysisDataSet.AnalysisPriorityDataDataTable();
			}

			// Token: 0x060004CA RID: 1226 RVA: 0x00011884 File Offset: 0x0000FA84
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			internal void InitVars()
			{
				this.columnANALYSIS_UID = base.Columns["ANALYSIS_UID"];
				this.columnMD_PROP_UID = base.Columns["MD_PROP_UID"];
				this.columnMD_PROP_NAME = base.Columns["MD_PROP_NAME"];
				this.columnWEIGHT = base.Columns["WEIGHT"];
				this.columnMIN_VALUE = base.Columns["MIN_VALUE"];
				this.columnMAX_VALUE = base.Columns["MAX_VALUE"];
			}

			// Token: 0x060004CB RID: 1227 RVA: 0x00011918 File Offset: 0x0000FB18
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			private void InitClass()
			{
				this.columnANALYSIS_UID = new DataColumn("ANALYSIS_UID", typeof(Guid), null, MappingType.Element);
				base.Columns.Add(this.columnANALYSIS_UID);
				this.columnMD_PROP_UID = new DataColumn("MD_PROP_UID", typeof(Guid), null, MappingType.Element);
				base.Columns.Add(this.columnMD_PROP_UID);
				this.columnMD_PROP_NAME = new DataColumn("MD_PROP_NAME", typeof(string), null, MappingType.Element);
				base.Columns.Add(this.columnMD_PROP_NAME);
				this.columnWEIGHT = new DataColumn("WEIGHT", typeof(double), null, MappingType.Element);
				base.Columns.Add(this.columnWEIGHT);
				this.columnMIN_VALUE = new DataColumn("MIN_VALUE", typeof(decimal), null, MappingType.Element);
				base.Columns.Add(this.columnMIN_VALUE);
				this.columnMAX_VALUE = new DataColumn("MAX_VALUE", typeof(decimal), null, MappingType.Element);
				base.Columns.Add(this.columnMAX_VALUE);
				base.Constraints.Add(new UniqueConstraint("AnalysisPriorityData_Constraint1", new DataColumn[]
				{
					this.columnANALYSIS_UID,
					this.columnMD_PROP_UID
				}, true));
				this.columnANALYSIS_UID.AllowDBNull = false;
				this.columnMD_PROP_UID.AllowDBNull = false;
				this.columnMD_PROP_NAME.ReadOnly = true;
				this.columnWEIGHT.AllowDBNull = false;
			}

			// Token: 0x060004CC RID: 1228 RVA: 0x00011A93 File Offset: 0x0000FC93
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public AnalysisDataSet.AnalysisPriorityDataRow NewAnalysisPriorityDataRow()
			{
				return (AnalysisDataSet.AnalysisPriorityDataRow)base.NewRow();
			}

			// Token: 0x060004CD RID: 1229 RVA: 0x00011AA0 File Offset: 0x0000FCA0
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			protected override DataRow NewRowFromBuilder(DataRowBuilder builder)
			{
				return new AnalysisDataSet.AnalysisPriorityDataRow(builder);
			}

			// Token: 0x060004CE RID: 1230 RVA: 0x00011AA8 File Offset: 0x0000FCA8
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			protected override Type GetRowType()
			{
				return typeof(AnalysisDataSet.AnalysisPriorityDataRow);
			}

			// Token: 0x060004CF RID: 1231 RVA: 0x00011AB4 File Offset: 0x0000FCB4
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			protected override void OnRowChanged(DataRowChangeEventArgs e)
			{
				base.OnRowChanged(e);
				if (this.AnalysisPriorityDataRowChanged != null)
				{
					this.AnalysisPriorityDataRowChanged(this, new AnalysisDataSet.AnalysisPriorityDataRowChangeEvent((AnalysisDataSet.AnalysisPriorityDataRow)e.Row, e.Action));
				}
			}

			// Token: 0x060004D0 RID: 1232 RVA: 0x00011AE7 File Offset: 0x0000FCE7
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			protected override void OnRowChanging(DataRowChangeEventArgs e)
			{
				base.OnRowChanging(e);
				if (this.AnalysisPriorityDataRowChanging != null)
				{
					this.AnalysisPriorityDataRowChanging(this, new AnalysisDataSet.AnalysisPriorityDataRowChangeEvent((AnalysisDataSet.AnalysisPriorityDataRow)e.Row, e.Action));
				}
			}

			// Token: 0x060004D1 RID: 1233 RVA: 0x00011B1A File Offset: 0x0000FD1A
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			protected override void OnRowDeleted(DataRowChangeEventArgs e)
			{
				base.OnRowDeleted(e);
				if (this.AnalysisPriorityDataRowDeleted != null)
				{
					this.AnalysisPriorityDataRowDeleted(this, new AnalysisDataSet.AnalysisPriorityDataRowChangeEvent((AnalysisDataSet.AnalysisPriorityDataRow)e.Row, e.Action));
				}
			}

			// Token: 0x060004D2 RID: 1234 RVA: 0x00011B4D File Offset: 0x0000FD4D
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			protected override void OnRowDeleting(DataRowChangeEventArgs e)
			{
				base.OnRowDeleting(e);
				if (this.AnalysisPriorityDataRowDeleting != null)
				{
					this.AnalysisPriorityDataRowDeleting(this, new AnalysisDataSet.AnalysisPriorityDataRowChangeEvent((AnalysisDataSet.AnalysisPriorityDataRow)e.Row, e.Action));
				}
			}

			// Token: 0x060004D3 RID: 1235 RVA: 0x00011B80 File Offset: 0x0000FD80
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			public void RemoveAnalysisPriorityDataRow(AnalysisDataSet.AnalysisPriorityDataRow row)
			{
				base.Rows.Remove(row);
			}

			// Token: 0x060004D4 RID: 1236 RVA: 0x00011B90 File Offset: 0x0000FD90
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public static XmlSchemaComplexType GetTypedTableSchema(XmlSchemaSet xs)
			{
				XmlSchemaComplexType xmlSchemaComplexType = new XmlSchemaComplexType();
				XmlSchemaSequence xmlSchemaSequence = new XmlSchemaSequence();
				AnalysisDataSet analysisDataSet = new AnalysisDataSet();
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
				xmlSchemaAttribute.FixedValue = analysisDataSet.Namespace;
				xmlSchemaComplexType.Attributes.Add(xmlSchemaAttribute);
				XmlSchemaAttribute xmlSchemaAttribute2 = new XmlSchemaAttribute();
				xmlSchemaAttribute2.Name = "tableTypeName";
				xmlSchemaAttribute2.FixedValue = "AnalysisPriorityDataDataTable";
				xmlSchemaComplexType.Attributes.Add(xmlSchemaAttribute2);
				xmlSchemaComplexType.Particle = xmlSchemaSequence;
				XmlSchema schemaSerializable = analysisDataSet.GetSchemaSerializable();
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

			// Token: 0x04000161 RID: 353
			private DataColumn columnANALYSIS_UID;

			// Token: 0x04000162 RID: 354
			private DataColumn columnMD_PROP_UID;

			// Token: 0x04000163 RID: 355
			private DataColumn columnMD_PROP_NAME;

			// Token: 0x04000164 RID: 356
			private DataColumn columnWEIGHT;

			// Token: 0x04000165 RID: 357
			private DataColumn columnMIN_VALUE;

			// Token: 0x04000166 RID: 358
			private DataColumn columnMAX_VALUE;
		}

		// Token: 0x02000050 RID: 80
		[XmlSchemaProvider("GetTypedTableSchema")]
		[Serializable]
		public class AnalysisProjectImpactDataTable : DataTable, IEnumerable
		{
			// Token: 0x060004D5 RID: 1237 RVA: 0x00011D88 File Offset: 0x0000FF88
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			public AnalysisProjectImpactDataTable()
			{
				base.TableName = "AnalysisProjectImpact";
				this.BeginInit();
				this.InitClass();
				this.EndInit();
			}

			// Token: 0x060004D6 RID: 1238 RVA: 0x00011DB0 File Offset: 0x0000FFB0
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			internal AnalysisProjectImpactDataTable(DataTable table)
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

			// Token: 0x060004D7 RID: 1239 RVA: 0x00011E58 File Offset: 0x00010058
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			protected AnalysisProjectImpactDataTable(SerializationInfo info, StreamingContext context) : base(info, context)
			{
				this.InitVars();
			}

			// Token: 0x1700015B RID: 347
			// (get) Token: 0x060004D8 RID: 1240 RVA: 0x00011E68 File Offset: 0x00010068
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			public DataColumn ANALYSIS_UIDColumn
			{
				get
				{
					return this.columnANALYSIS_UID;
				}
			}

			// Token: 0x1700015C RID: 348
			// (get) Token: 0x060004D9 RID: 1241 RVA: 0x00011E70 File Offset: 0x00010070
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public DataColumn PROJ_UIDColumn
			{
				get
				{
					return this.columnPROJ_UID;
				}
			}

			// Token: 0x1700015D RID: 349
			// (get) Token: 0x060004DA RID: 1242 RVA: 0x00011E78 File Offset: 0x00010078
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			public DataColumn DRIVER_UIDColumn
			{
				get
				{
					return this.columnDRIVER_UID;
				}
			}

			// Token: 0x1700015E RID: 350
			// (get) Token: 0x060004DB RID: 1243 RVA: 0x00011E80 File Offset: 0x00010080
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public DataColumn LT_STRUCT_UIDColumn
			{
				get
				{
					return this.columnLT_STRUCT_UID;
				}
			}

			// Token: 0x1700015F RID: 351
			// (get) Token: 0x060004DC RID: 1244 RVA: 0x00011E88 File Offset: 0x00010088
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

			// Token: 0x17000160 RID: 352
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public AnalysisDataSet.AnalysisProjectImpactRow this[int index]
			{
				get
				{
					return (AnalysisDataSet.AnalysisProjectImpactRow)base.Rows[index];
				}
			}

			// Token: 0x14000035 RID: 53
			// (add) Token: 0x060004DE RID: 1246 RVA: 0x00011EA8 File Offset: 0x000100A8
			// (remove) Token: 0x060004DF RID: 1247 RVA: 0x00011EE0 File Offset: 0x000100E0
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public event AnalysisDataSet.AnalysisProjectImpactRowChangeEventHandler AnalysisProjectImpactRowChanging;

			// Token: 0x14000036 RID: 54
			// (add) Token: 0x060004E0 RID: 1248 RVA: 0x00011F18 File Offset: 0x00010118
			// (remove) Token: 0x060004E1 RID: 1249 RVA: 0x00011F50 File Offset: 0x00010150
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public event AnalysisDataSet.AnalysisProjectImpactRowChangeEventHandler AnalysisProjectImpactRowChanged;

			// Token: 0x14000037 RID: 55
			// (add) Token: 0x060004E2 RID: 1250 RVA: 0x00011F88 File Offset: 0x00010188
			// (remove) Token: 0x060004E3 RID: 1251 RVA: 0x00011FC0 File Offset: 0x000101C0
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public event AnalysisDataSet.AnalysisProjectImpactRowChangeEventHandler AnalysisProjectImpactRowDeleting;

			// Token: 0x14000038 RID: 56
			// (add) Token: 0x060004E4 RID: 1252 RVA: 0x00011FF8 File Offset: 0x000101F8
			// (remove) Token: 0x060004E5 RID: 1253 RVA: 0x00012030 File Offset: 0x00010230
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public event AnalysisDataSet.AnalysisProjectImpactRowChangeEventHandler AnalysisProjectImpactRowDeleted;

			// Token: 0x060004E6 RID: 1254 RVA: 0x00012065 File Offset: 0x00010265
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			public void AddAnalysisProjectImpactRow(AnalysisDataSet.AnalysisProjectImpactRow row)
			{
				base.Rows.Add(row);
			}

			// Token: 0x060004E7 RID: 1255 RVA: 0x00012074 File Offset: 0x00010274
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public AnalysisDataSet.AnalysisProjectImpactRow AddAnalysisProjectImpactRow(Guid ANALYSIS_UID, Guid PROJ_UID, Guid DRIVER_UID, Guid LT_STRUCT_UID)
			{
				AnalysisDataSet.AnalysisProjectImpactRow analysisProjectImpactRow = (AnalysisDataSet.AnalysisProjectImpactRow)base.NewRow();
				object[] itemArray = new object[]
				{
					ANALYSIS_UID,
					PROJ_UID,
					DRIVER_UID,
					LT_STRUCT_UID
				};
				analysisProjectImpactRow.ItemArray = itemArray;
				base.Rows.Add(analysisProjectImpactRow);
				return analysisProjectImpactRow;
			}

			// Token: 0x060004E8 RID: 1256 RVA: 0x000120D0 File Offset: 0x000102D0
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public AnalysisDataSet.AnalysisProjectImpactRow FindByANALYSIS_UIDPROJ_UIDDRIVER_UID(Guid ANALYSIS_UID, Guid PROJ_UID, Guid DRIVER_UID)
			{
				return (AnalysisDataSet.AnalysisProjectImpactRow)base.Rows.Find(new object[]
				{
					ANALYSIS_UID,
					PROJ_UID,
					DRIVER_UID
				});
			}

			// Token: 0x060004E9 RID: 1257 RVA: 0x00012110 File Offset: 0x00010310
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			public virtual IEnumerator GetEnumerator()
			{
				return base.Rows.GetEnumerator();
			}

			// Token: 0x060004EA RID: 1258 RVA: 0x00012120 File Offset: 0x00010320
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public override DataTable Clone()
			{
				AnalysisDataSet.AnalysisProjectImpactDataTable analysisProjectImpactDataTable = (AnalysisDataSet.AnalysisProjectImpactDataTable)base.Clone();
				analysisProjectImpactDataTable.InitVars();
				return analysisProjectImpactDataTable;
			}

			// Token: 0x060004EB RID: 1259 RVA: 0x00012140 File Offset: 0x00010340
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			protected override DataTable CreateInstance()
			{
				return new AnalysisDataSet.AnalysisProjectImpactDataTable();
			}

			// Token: 0x060004EC RID: 1260 RVA: 0x00012148 File Offset: 0x00010348
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			internal void InitVars()
			{
				this.columnANALYSIS_UID = base.Columns["ANALYSIS_UID"];
				this.columnPROJ_UID = base.Columns["PROJ_UID"];
				this.columnDRIVER_UID = base.Columns["DRIVER_UID"];
				this.columnLT_STRUCT_UID = base.Columns["LT_STRUCT_UID"];
			}

			// Token: 0x060004ED RID: 1261 RVA: 0x000121B0 File Offset: 0x000103B0
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			private void InitClass()
			{
				this.columnANALYSIS_UID = new DataColumn("ANALYSIS_UID", typeof(Guid), null, MappingType.Element);
				base.Columns.Add(this.columnANALYSIS_UID);
				this.columnPROJ_UID = new DataColumn("PROJ_UID", typeof(Guid), null, MappingType.Element);
				base.Columns.Add(this.columnPROJ_UID);
				this.columnDRIVER_UID = new DataColumn("DRIVER_UID", typeof(Guid), null, MappingType.Element);
				base.Columns.Add(this.columnDRIVER_UID);
				this.columnLT_STRUCT_UID = new DataColumn("LT_STRUCT_UID", typeof(Guid), null, MappingType.Element);
				base.Columns.Add(this.columnLT_STRUCT_UID);
				base.Constraints.Add(new UniqueConstraint("AnalysisProjectImpact_Constraint1", new DataColumn[]
				{
					this.columnANALYSIS_UID,
					this.columnPROJ_UID,
					this.columnDRIVER_UID
				}, true));
				this.columnANALYSIS_UID.AllowDBNull = false;
				this.columnPROJ_UID.AllowDBNull = false;
				this.columnDRIVER_UID.AllowDBNull = false;
			}

			// Token: 0x060004EE RID: 1262 RVA: 0x000122CE File Offset: 0x000104CE
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			public AnalysisDataSet.AnalysisProjectImpactRow NewAnalysisProjectImpactRow()
			{
				return (AnalysisDataSet.AnalysisProjectImpactRow)base.NewRow();
			}

			// Token: 0x060004EF RID: 1263 RVA: 0x000122DB File Offset: 0x000104DB
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			protected override DataRow NewRowFromBuilder(DataRowBuilder builder)
			{
				return new AnalysisDataSet.AnalysisProjectImpactRow(builder);
			}

			// Token: 0x060004F0 RID: 1264 RVA: 0x000122E3 File Offset: 0x000104E3
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			protected override Type GetRowType()
			{
				return typeof(AnalysisDataSet.AnalysisProjectImpactRow);
			}

			// Token: 0x060004F1 RID: 1265 RVA: 0x000122EF File Offset: 0x000104EF
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			protected override void OnRowChanged(DataRowChangeEventArgs e)
			{
				base.OnRowChanged(e);
				if (this.AnalysisProjectImpactRowChanged != null)
				{
					this.AnalysisProjectImpactRowChanged(this, new AnalysisDataSet.AnalysisProjectImpactRowChangeEvent((AnalysisDataSet.AnalysisProjectImpactRow)e.Row, e.Action));
				}
			}

			// Token: 0x060004F2 RID: 1266 RVA: 0x00012322 File Offset: 0x00010522
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			protected override void OnRowChanging(DataRowChangeEventArgs e)
			{
				base.OnRowChanging(e);
				if (this.AnalysisProjectImpactRowChanging != null)
				{
					this.AnalysisProjectImpactRowChanging(this, new AnalysisDataSet.AnalysisProjectImpactRowChangeEvent((AnalysisDataSet.AnalysisProjectImpactRow)e.Row, e.Action));
				}
			}

			// Token: 0x060004F3 RID: 1267 RVA: 0x00012355 File Offset: 0x00010555
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			protected override void OnRowDeleted(DataRowChangeEventArgs e)
			{
				base.OnRowDeleted(e);
				if (this.AnalysisProjectImpactRowDeleted != null)
				{
					this.AnalysisProjectImpactRowDeleted(this, new AnalysisDataSet.AnalysisProjectImpactRowChangeEvent((AnalysisDataSet.AnalysisProjectImpactRow)e.Row, e.Action));
				}
			}

			// Token: 0x060004F4 RID: 1268 RVA: 0x00012388 File Offset: 0x00010588
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			protected override void OnRowDeleting(DataRowChangeEventArgs e)
			{
				base.OnRowDeleting(e);
				if (this.AnalysisProjectImpactRowDeleting != null)
				{
					this.AnalysisProjectImpactRowDeleting(this, new AnalysisDataSet.AnalysisProjectImpactRowChangeEvent((AnalysisDataSet.AnalysisProjectImpactRow)e.Row, e.Action));
				}
			}

			// Token: 0x060004F5 RID: 1269 RVA: 0x000123BB File Offset: 0x000105BB
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			public void RemoveAnalysisProjectImpactRow(AnalysisDataSet.AnalysisProjectImpactRow row)
			{
				base.Rows.Remove(row);
			}

			// Token: 0x060004F6 RID: 1270 RVA: 0x000123CC File Offset: 0x000105CC
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public static XmlSchemaComplexType GetTypedTableSchema(XmlSchemaSet xs)
			{
				XmlSchemaComplexType xmlSchemaComplexType = new XmlSchemaComplexType();
				XmlSchemaSequence xmlSchemaSequence = new XmlSchemaSequence();
				AnalysisDataSet analysisDataSet = new AnalysisDataSet();
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
				xmlSchemaAttribute.FixedValue = analysisDataSet.Namespace;
				xmlSchemaComplexType.Attributes.Add(xmlSchemaAttribute);
				XmlSchemaAttribute xmlSchemaAttribute2 = new XmlSchemaAttribute();
				xmlSchemaAttribute2.Name = "tableTypeName";
				xmlSchemaAttribute2.FixedValue = "AnalysisProjectImpactDataTable";
				xmlSchemaComplexType.Attributes.Add(xmlSchemaAttribute2);
				xmlSchemaComplexType.Particle = xmlSchemaSequence;
				XmlSchema schemaSerializable = analysisDataSet.GetSchemaSerializable();
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

			// Token: 0x0400016B RID: 363
			private DataColumn columnANALYSIS_UID;

			// Token: 0x0400016C RID: 364
			private DataColumn columnPROJ_UID;

			// Token: 0x0400016D RID: 365
			private DataColumn columnDRIVER_UID;

			// Token: 0x0400016E RID: 366
			private DataColumn columnLT_STRUCT_UID;
		}

		// Token: 0x02000051 RID: 81
		[XmlSchemaProvider("GetTypedTableSchema")]
		[Serializable]
		public class AnalysisOptimizerSolutionsDataTable : DataTable, IEnumerable
		{
			// Token: 0x060004F7 RID: 1271 RVA: 0x000125C4 File Offset: 0x000107C4
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			public AnalysisOptimizerSolutionsDataTable()
			{
				base.TableName = "AnalysisOptimizerSolutions";
				this.BeginInit();
				this.InitClass();
				this.EndInit();
			}

			// Token: 0x060004F8 RID: 1272 RVA: 0x000125EC File Offset: 0x000107EC
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			internal AnalysisOptimizerSolutionsDataTable(DataTable table)
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

			// Token: 0x060004F9 RID: 1273 RVA: 0x00012694 File Offset: 0x00010894
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			protected AnalysisOptimizerSolutionsDataTable(SerializationInfo info, StreamingContext context) : base(info, context)
			{
				this.InitVars();
			}

			// Token: 0x17000161 RID: 353
			// (get) Token: 0x060004FA RID: 1274 RVA: 0x000126A4 File Offset: 0x000108A4
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			public DataColumn ANALYSIS_UIDColumn
			{
				get
				{
					return this.columnANALYSIS_UID;
				}
			}

			// Token: 0x17000162 RID: 354
			// (get) Token: 0x060004FB RID: 1275 RVA: 0x000126AC File Offset: 0x000108AC
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			public DataColumn SOLUTION_UIDColumn
			{
				get
				{
					return this.columnSOLUTION_UID;
				}
			}

			// Token: 0x17000163 RID: 355
			// (get) Token: 0x060004FC RID: 1276 RVA: 0x000126B4 File Offset: 0x000108B4
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			public DataColumn SOLUTION_NAMEColumn
			{
				get
				{
					return this.columnSOLUTION_NAME;
				}
			}

			// Token: 0x17000164 RID: 356
			// (get) Token: 0x060004FD RID: 1277 RVA: 0x000126BC File Offset: 0x000108BC
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public DataColumn SOLUTION_DESCRIPTIONColumn
			{
				get
				{
					return this.columnSOLUTION_DESCRIPTION;
				}
			}

			// Token: 0x17000165 RID: 357
			// (get) Token: 0x060004FE RID: 1278 RVA: 0x000126C4 File Offset: 0x000108C4
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public DataColumn FRONTIER_UIDColumn
			{
				get
				{
					return this.columnFRONTIER_UID;
				}
			}

			// Token: 0x17000166 RID: 358
			// (get) Token: 0x060004FF RID: 1279 RVA: 0x000126CC File Offset: 0x000108CC
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			public DataColumn OPT_USE_DEPENDENCIESColumn
			{
				get
				{
					return this.columnOPT_USE_DEPENDENCIES;
				}
			}

			// Token: 0x17000167 RID: 359
			// (get) Token: 0x06000500 RID: 1280 RVA: 0x000126D4 File Offset: 0x000108D4
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public DataColumn CREATED_DATEColumn
			{
				get
				{
					return this.columnCREATED_DATE;
				}
			}

			// Token: 0x17000168 RID: 360
			// (get) Token: 0x06000501 RID: 1281 RVA: 0x000126DC File Offset: 0x000108DC
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public DataColumn MOD_DATEColumn
			{
				get
				{
					return this.columnMOD_DATE;
				}
			}

			// Token: 0x17000169 RID: 361
			// (get) Token: 0x06000502 RID: 1282 RVA: 0x000126E4 File Offset: 0x000108E4
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			public DataColumn LAST_UPDATED_BY_RES_UIDColumn
			{
				get
				{
					return this.columnLAST_UPDATED_BY_RES_UID;
				}
			}

			// Token: 0x1700016A RID: 362
			// (get) Token: 0x06000503 RID: 1283 RVA: 0x000126EC File Offset: 0x000108EC
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public DataColumn LAST_UPDATED_BY_RES_NAMEColumn
			{
				get
				{
					return this.columnLAST_UPDATED_BY_RES_NAME;
				}
			}

			// Token: 0x1700016B RID: 363
			// (get) Token: 0x06000504 RID: 1284 RVA: 0x000126F4 File Offset: 0x000108F4
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			public DataColumn CREATED_BY_RES_UIDColumn
			{
				get
				{
					return this.columnCREATED_BY_RES_UID;
				}
			}

			// Token: 0x1700016C RID: 364
			// (get) Token: 0x06000505 RID: 1285 RVA: 0x000126FC File Offset: 0x000108FC
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public DataColumn CREATED_BY_RES_NAMEColumn
			{
				get
				{
					return this.columnCREATED_BY_RES_NAME;
				}
			}

			// Token: 0x1700016D RID: 365
			// (get) Token: 0x06000506 RID: 1286 RVA: 0x00012704 File Offset: 0x00010904
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

			// Token: 0x1700016E RID: 366
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			public AnalysisDataSet.AnalysisOptimizerSolutionsRow this[int index]
			{
				get
				{
					return (AnalysisDataSet.AnalysisOptimizerSolutionsRow)base.Rows[index];
				}
			}

			// Token: 0x14000039 RID: 57
			// (add) Token: 0x06000508 RID: 1288 RVA: 0x00012724 File Offset: 0x00010924
			// (remove) Token: 0x06000509 RID: 1289 RVA: 0x0001275C File Offset: 0x0001095C
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public event AnalysisDataSet.AnalysisOptimizerSolutionsRowChangeEventHandler AnalysisOptimizerSolutionsRowChanging;

			// Token: 0x1400003A RID: 58
			// (add) Token: 0x0600050A RID: 1290 RVA: 0x00012794 File Offset: 0x00010994
			// (remove) Token: 0x0600050B RID: 1291 RVA: 0x000127CC File Offset: 0x000109CC
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public event AnalysisDataSet.AnalysisOptimizerSolutionsRowChangeEventHandler AnalysisOptimizerSolutionsRowChanged;

			// Token: 0x1400003B RID: 59
			// (add) Token: 0x0600050C RID: 1292 RVA: 0x00012804 File Offset: 0x00010A04
			// (remove) Token: 0x0600050D RID: 1293 RVA: 0x0001283C File Offset: 0x00010A3C
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public event AnalysisDataSet.AnalysisOptimizerSolutionsRowChangeEventHandler AnalysisOptimizerSolutionsRowDeleting;

			// Token: 0x1400003C RID: 60
			// (add) Token: 0x0600050E RID: 1294 RVA: 0x00012874 File Offset: 0x00010A74
			// (remove) Token: 0x0600050F RID: 1295 RVA: 0x000128AC File Offset: 0x00010AAC
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public event AnalysisDataSet.AnalysisOptimizerSolutionsRowChangeEventHandler AnalysisOptimizerSolutionsRowDeleted;

			// Token: 0x06000510 RID: 1296 RVA: 0x000128E1 File Offset: 0x00010AE1
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			public void AddAnalysisOptimizerSolutionsRow(AnalysisDataSet.AnalysisOptimizerSolutionsRow row)
			{
				base.Rows.Add(row);
			}

			// Token: 0x06000511 RID: 1297 RVA: 0x000128F0 File Offset: 0x00010AF0
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			public AnalysisDataSet.AnalysisOptimizerSolutionsRow AddAnalysisOptimizerSolutionsRow(AnalysisDataSet.AnalysisRow parentAnalysisRowByFK_Analysis_AnalysisOptimizerSolutions, Guid SOLUTION_UID, string SOLUTION_NAME, string SOLUTION_DESCRIPTION, Guid FRONTIER_UID, bool OPT_USE_DEPENDENCIES, DateTime CREATED_DATE, DateTime MOD_DATE, Guid LAST_UPDATED_BY_RES_UID, string LAST_UPDATED_BY_RES_NAME, Guid CREATED_BY_RES_UID, string CREATED_BY_RES_NAME)
			{
				AnalysisDataSet.AnalysisOptimizerSolutionsRow analysisOptimizerSolutionsRow = (AnalysisDataSet.AnalysisOptimizerSolutionsRow)base.NewRow();
				object[] array = new object[]
				{
					null,
					SOLUTION_UID,
					SOLUTION_NAME,
					SOLUTION_DESCRIPTION,
					FRONTIER_UID,
					OPT_USE_DEPENDENCIES,
					CREATED_DATE,
					MOD_DATE,
					LAST_UPDATED_BY_RES_UID,
					LAST_UPDATED_BY_RES_NAME,
					CREATED_BY_RES_UID,
					CREATED_BY_RES_NAME
				};
				if (parentAnalysisRowByFK_Analysis_AnalysisOptimizerSolutions != null)
				{
					array[0] = parentAnalysisRowByFK_Analysis_AnalysisOptimizerSolutions[0];
				}
				analysisOptimizerSolutionsRow.ItemArray = array;
				base.Rows.Add(analysisOptimizerSolutionsRow);
				return analysisOptimizerSolutionsRow;
			}

			// Token: 0x06000512 RID: 1298 RVA: 0x00012990 File Offset: 0x00010B90
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			public AnalysisDataSet.AnalysisOptimizerSolutionsRow FindBySOLUTION_UID(Guid SOLUTION_UID)
			{
				return (AnalysisDataSet.AnalysisOptimizerSolutionsRow)base.Rows.Find(new object[]
				{
					SOLUTION_UID
				});
			}

			// Token: 0x06000513 RID: 1299 RVA: 0x000129BE File Offset: 0x00010BBE
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			public virtual IEnumerator GetEnumerator()
			{
				return base.Rows.GetEnumerator();
			}

			// Token: 0x06000514 RID: 1300 RVA: 0x000129CC File Offset: 0x00010BCC
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			public override DataTable Clone()
			{
				AnalysisDataSet.AnalysisOptimizerSolutionsDataTable analysisOptimizerSolutionsDataTable = (AnalysisDataSet.AnalysisOptimizerSolutionsDataTable)base.Clone();
				analysisOptimizerSolutionsDataTable.InitVars();
				return analysisOptimizerSolutionsDataTable;
			}

			// Token: 0x06000515 RID: 1301 RVA: 0x000129EC File Offset: 0x00010BEC
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			protected override DataTable CreateInstance()
			{
				return new AnalysisDataSet.AnalysisOptimizerSolutionsDataTable();
			}

			// Token: 0x06000516 RID: 1302 RVA: 0x000129F4 File Offset: 0x00010BF4
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			internal void InitVars()
			{
				this.columnANALYSIS_UID = base.Columns["ANALYSIS_UID"];
				this.columnSOLUTION_UID = base.Columns["SOLUTION_UID"];
				this.columnSOLUTION_NAME = base.Columns["SOLUTION_NAME"];
				this.columnSOLUTION_DESCRIPTION = base.Columns["SOLUTION_DESCRIPTION"];
				this.columnFRONTIER_UID = base.Columns["FRONTIER_UID"];
				this.columnOPT_USE_DEPENDENCIES = base.Columns["OPT_USE_DEPENDENCIES"];
				this.columnCREATED_DATE = base.Columns["CREATED_DATE"];
				this.columnMOD_DATE = base.Columns["MOD_DATE"];
				this.columnLAST_UPDATED_BY_RES_UID = base.Columns["LAST_UPDATED_BY_RES_UID"];
				this.columnLAST_UPDATED_BY_RES_NAME = base.Columns["LAST_UPDATED_BY_RES_NAME"];
				this.columnCREATED_BY_RES_UID = base.Columns["CREATED_BY_RES_UID"];
				this.columnCREATED_BY_RES_NAME = base.Columns["CREATED_BY_RES_NAME"];
			}

			// Token: 0x06000517 RID: 1303 RVA: 0x00012B0C File Offset: 0x00010D0C
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			private void InitClass()
			{
				this.columnANALYSIS_UID = new DataColumn("ANALYSIS_UID", typeof(Guid), null, MappingType.Element);
				base.Columns.Add(this.columnANALYSIS_UID);
				this.columnSOLUTION_UID = new DataColumn("SOLUTION_UID", typeof(Guid), null, MappingType.Element);
				base.Columns.Add(this.columnSOLUTION_UID);
				this.columnSOLUTION_NAME = new DataColumn("SOLUTION_NAME", typeof(string), null, MappingType.Element);
				base.Columns.Add(this.columnSOLUTION_NAME);
				this.columnSOLUTION_DESCRIPTION = new DataColumn("SOLUTION_DESCRIPTION", typeof(string), null, MappingType.Element);
				base.Columns.Add(this.columnSOLUTION_DESCRIPTION);
				this.columnFRONTIER_UID = new DataColumn("FRONTIER_UID", typeof(Guid), null, MappingType.Element);
				base.Columns.Add(this.columnFRONTIER_UID);
				this.columnOPT_USE_DEPENDENCIES = new DataColumn("OPT_USE_DEPENDENCIES", typeof(bool), null, MappingType.Element);
				base.Columns.Add(this.columnOPT_USE_DEPENDENCIES);
				this.columnCREATED_DATE = new DataColumn("CREATED_DATE", typeof(DateTime), null, MappingType.Element);
				base.Columns.Add(this.columnCREATED_DATE);
				this.columnMOD_DATE = new DataColumn("MOD_DATE", typeof(DateTime), null, MappingType.Element);
				base.Columns.Add(this.columnMOD_DATE);
				this.columnLAST_UPDATED_BY_RES_UID = new DataColumn("LAST_UPDATED_BY_RES_UID", typeof(Guid), null, MappingType.Element);
				base.Columns.Add(this.columnLAST_UPDATED_BY_RES_UID);
				this.columnLAST_UPDATED_BY_RES_NAME = new DataColumn("LAST_UPDATED_BY_RES_NAME", typeof(string), null, MappingType.Element);
				base.Columns.Add(this.columnLAST_UPDATED_BY_RES_NAME);
				this.columnCREATED_BY_RES_UID = new DataColumn("CREATED_BY_RES_UID", typeof(Guid), null, MappingType.Element);
				base.Columns.Add(this.columnCREATED_BY_RES_UID);
				this.columnCREATED_BY_RES_NAME = new DataColumn("CREATED_BY_RES_NAME", typeof(string), null, MappingType.Element);
				base.Columns.Add(this.columnCREATED_BY_RES_NAME);
				base.Constraints.Add(new UniqueConstraint("AnalysisOptimizerSolutions_Constraint1", new DataColumn[]
				{
					this.columnSOLUTION_UID
				}, true));
				this.columnANALYSIS_UID.AllowDBNull = false;
				this.columnANALYSIS_UID.ReadOnly = true;
				this.columnSOLUTION_UID.AllowDBNull = false;
				this.columnSOLUTION_UID.ReadOnly = true;
				this.columnSOLUTION_UID.Unique = true;
				this.columnSOLUTION_NAME.AllowDBNull = false;
				this.columnSOLUTION_NAME.ReadOnly = true;
				this.columnSOLUTION_DESCRIPTION.ReadOnly = true;
				this.columnFRONTIER_UID.AllowDBNull = false;
				this.columnFRONTIER_UID.ReadOnly = true;
				this.columnOPT_USE_DEPENDENCIES.ReadOnly = true;
				this.columnCREATED_DATE.ReadOnly = true;
				this.columnMOD_DATE.ReadOnly = true;
				this.columnLAST_UPDATED_BY_RES_UID.ReadOnly = true;
				this.columnLAST_UPDATED_BY_RES_NAME.ReadOnly = true;
				this.columnCREATED_BY_RES_UID.ReadOnly = true;
				this.columnCREATED_BY_RES_NAME.ReadOnly = true;
			}

			// Token: 0x06000518 RID: 1304 RVA: 0x00012E28 File Offset: 0x00011028
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			public AnalysisDataSet.AnalysisOptimizerSolutionsRow NewAnalysisOptimizerSolutionsRow()
			{
				return (AnalysisDataSet.AnalysisOptimizerSolutionsRow)base.NewRow();
			}

			// Token: 0x06000519 RID: 1305 RVA: 0x00012E35 File Offset: 0x00011035
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			protected override DataRow NewRowFromBuilder(DataRowBuilder builder)
			{
				return new AnalysisDataSet.AnalysisOptimizerSolutionsRow(builder);
			}

			// Token: 0x0600051A RID: 1306 RVA: 0x00012E3D File Offset: 0x0001103D
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			protected override Type GetRowType()
			{
				return typeof(AnalysisDataSet.AnalysisOptimizerSolutionsRow);
			}

			// Token: 0x0600051B RID: 1307 RVA: 0x00012E49 File Offset: 0x00011049
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			protected override void OnRowChanged(DataRowChangeEventArgs e)
			{
				base.OnRowChanged(e);
				if (this.AnalysisOptimizerSolutionsRowChanged != null)
				{
					this.AnalysisOptimizerSolutionsRowChanged(this, new AnalysisDataSet.AnalysisOptimizerSolutionsRowChangeEvent((AnalysisDataSet.AnalysisOptimizerSolutionsRow)e.Row, e.Action));
				}
			}

			// Token: 0x0600051C RID: 1308 RVA: 0x00012E7C File Offset: 0x0001107C
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			protected override void OnRowChanging(DataRowChangeEventArgs e)
			{
				base.OnRowChanging(e);
				if (this.AnalysisOptimizerSolutionsRowChanging != null)
				{
					this.AnalysisOptimizerSolutionsRowChanging(this, new AnalysisDataSet.AnalysisOptimizerSolutionsRowChangeEvent((AnalysisDataSet.AnalysisOptimizerSolutionsRow)e.Row, e.Action));
				}
			}

			// Token: 0x0600051D RID: 1309 RVA: 0x00012EAF File Offset: 0x000110AF
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			protected override void OnRowDeleted(DataRowChangeEventArgs e)
			{
				base.OnRowDeleted(e);
				if (this.AnalysisOptimizerSolutionsRowDeleted != null)
				{
					this.AnalysisOptimizerSolutionsRowDeleted(this, new AnalysisDataSet.AnalysisOptimizerSolutionsRowChangeEvent((AnalysisDataSet.AnalysisOptimizerSolutionsRow)e.Row, e.Action));
				}
			}

			// Token: 0x0600051E RID: 1310 RVA: 0x00012EE2 File Offset: 0x000110E2
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			protected override void OnRowDeleting(DataRowChangeEventArgs e)
			{
				base.OnRowDeleting(e);
				if (this.AnalysisOptimizerSolutionsRowDeleting != null)
				{
					this.AnalysisOptimizerSolutionsRowDeleting(this, new AnalysisDataSet.AnalysisOptimizerSolutionsRowChangeEvent((AnalysisDataSet.AnalysisOptimizerSolutionsRow)e.Row, e.Action));
				}
			}

			// Token: 0x0600051F RID: 1311 RVA: 0x00012F15 File Offset: 0x00011115
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			public void RemoveAnalysisOptimizerSolutionsRow(AnalysisDataSet.AnalysisOptimizerSolutionsRow row)
			{
				base.Rows.Remove(row);
			}

			// Token: 0x06000520 RID: 1312 RVA: 0x00012F24 File Offset: 0x00011124
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			public static XmlSchemaComplexType GetTypedTableSchema(XmlSchemaSet xs)
			{
				XmlSchemaComplexType xmlSchemaComplexType = new XmlSchemaComplexType();
				XmlSchemaSequence xmlSchemaSequence = new XmlSchemaSequence();
				AnalysisDataSet analysisDataSet = new AnalysisDataSet();
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
				xmlSchemaAttribute.FixedValue = analysisDataSet.Namespace;
				xmlSchemaComplexType.Attributes.Add(xmlSchemaAttribute);
				XmlSchemaAttribute xmlSchemaAttribute2 = new XmlSchemaAttribute();
				xmlSchemaAttribute2.Name = "tableTypeName";
				xmlSchemaAttribute2.FixedValue = "AnalysisOptimizerSolutionsDataTable";
				xmlSchemaComplexType.Attributes.Add(xmlSchemaAttribute2);
				xmlSchemaComplexType.Particle = xmlSchemaSequence;
				XmlSchema schemaSerializable = analysisDataSet.GetSchemaSerializable();
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

			// Token: 0x04000173 RID: 371
			private DataColumn columnANALYSIS_UID;

			// Token: 0x04000174 RID: 372
			private DataColumn columnSOLUTION_UID;

			// Token: 0x04000175 RID: 373
			private DataColumn columnSOLUTION_NAME;

			// Token: 0x04000176 RID: 374
			private DataColumn columnSOLUTION_DESCRIPTION;

			// Token: 0x04000177 RID: 375
			private DataColumn columnFRONTIER_UID;

			// Token: 0x04000178 RID: 376
			private DataColumn columnOPT_USE_DEPENDENCIES;

			// Token: 0x04000179 RID: 377
			private DataColumn columnCREATED_DATE;

			// Token: 0x0400017A RID: 378
			private DataColumn columnMOD_DATE;

			// Token: 0x0400017B RID: 379
			private DataColumn columnLAST_UPDATED_BY_RES_UID;

			// Token: 0x0400017C RID: 380
			private DataColumn columnLAST_UPDATED_BY_RES_NAME;

			// Token: 0x0400017D RID: 381
			private DataColumn columnCREATED_BY_RES_UID;

			// Token: 0x0400017E RID: 382
			private DataColumn columnCREATED_BY_RES_NAME;
		}

		// Token: 0x02000052 RID: 82
		[XmlSchemaProvider("GetTypedTableSchema")]
		[Serializable]
		public class AnalysisPlannerSolutionsDataTable : DataTable, IEnumerable
		{
			// Token: 0x06000521 RID: 1313 RVA: 0x0001311C File Offset: 0x0001131C
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			public AnalysisPlannerSolutionsDataTable()
			{
				base.TableName = "AnalysisPlannerSolutions";
				this.BeginInit();
				this.InitClass();
				this.EndInit();
			}

			// Token: 0x06000522 RID: 1314 RVA: 0x00013144 File Offset: 0x00011344
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			internal AnalysisPlannerSolutionsDataTable(DataTable table)
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

			// Token: 0x06000523 RID: 1315 RVA: 0x000131EC File Offset: 0x000113EC
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			protected AnalysisPlannerSolutionsDataTable(SerializationInfo info, StreamingContext context) : base(info, context)
			{
				this.InitVars();
			}

			// Token: 0x1700016F RID: 367
			// (get) Token: 0x06000524 RID: 1316 RVA: 0x000131FC File Offset: 0x000113FC
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			public DataColumn SOLUTION_UIDColumn
			{
				get
				{
					return this.columnSOLUTION_UID;
				}
			}

			// Token: 0x17000170 RID: 368
			// (get) Token: 0x06000525 RID: 1317 RVA: 0x00013204 File Offset: 0x00011404
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public DataColumn OPTIMIZER_SOLUTION_UIDColumn
			{
				get
				{
					return this.columnOPTIMIZER_SOLUTION_UID;
				}
			}

			// Token: 0x17000171 RID: 369
			// (get) Token: 0x06000526 RID: 1318 RVA: 0x0001320C File Offset: 0x0001140C
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public DataColumn ANALYSIS_UIDColumn
			{
				get
				{
					return this.columnANALYSIS_UID;
				}
			}

			// Token: 0x17000172 RID: 370
			// (get) Token: 0x06000527 RID: 1319 RVA: 0x00013214 File Offset: 0x00011414
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			public DataColumn SOLUTION_NAMEColumn
			{
				get
				{
					return this.columnSOLUTION_NAME;
				}
			}

			// Token: 0x17000173 RID: 371
			// (get) Token: 0x06000528 RID: 1320 RVA: 0x0001321C File Offset: 0x0001141C
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public DataColumn SOLUTION_DESCRIPTIONColumn
			{
				get
				{
					return this.columnSOLUTION_DESCRIPTION;
				}
			}

			// Token: 0x17000174 RID: 372
			// (get) Token: 0x06000529 RID: 1321 RVA: 0x00013224 File Offset: 0x00011424
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			public DataColumn CONSTRAINT_TYPEColumn
			{
				get
				{
					return this.columnCONSTRAINT_TYPE;
				}
			}

			// Token: 0x17000175 RID: 373
			// (get) Token: 0x0600052A RID: 1322 RVA: 0x0001322C File Offset: 0x0001142C
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public DataColumn CONSTRAINT_VALUEColumn
			{
				get
				{
					return this.columnCONSTRAINT_VALUE;
				}
			}

			// Token: 0x17000176 RID: 374
			// (get) Token: 0x0600052B RID: 1323 RVA: 0x00013234 File Offset: 0x00011434
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public DataColumn FRONTIER_UIDColumn
			{
				get
				{
					return this.columnFRONTIER_UID;
				}
			}

			// Token: 0x17000177 RID: 375
			// (get) Token: 0x0600052C RID: 1324 RVA: 0x0001323C File Offset: 0x0001143C
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public DataColumn MOD_DATEColumn
			{
				get
				{
					return this.columnMOD_DATE;
				}
			}

			// Token: 0x17000178 RID: 376
			// (get) Token: 0x0600052D RID: 1325 RVA: 0x00013244 File Offset: 0x00011444
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public DataColumn CREATED_DATEColumn
			{
				get
				{
					return this.columnCREATED_DATE;
				}
			}

			// Token: 0x17000179 RID: 377
			// (get) Token: 0x0600052E RID: 1326 RVA: 0x0001324C File Offset: 0x0001144C
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			public DataColumn CREATED_BY_RES_UIDColumn
			{
				get
				{
					return this.columnCREATED_BY_RES_UID;
				}
			}

			// Token: 0x1700017A RID: 378
			// (get) Token: 0x0600052F RID: 1327 RVA: 0x00013254 File Offset: 0x00011454
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			public DataColumn LAST_UPDATED_BY_RES_UIDColumn
			{
				get
				{
					return this.columnLAST_UPDATED_BY_RES_UID;
				}
			}

			// Token: 0x1700017B RID: 379
			// (get) Token: 0x06000530 RID: 1328 RVA: 0x0001325C File Offset: 0x0001145C
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			public DataColumn CREATED_BY_RES_NAMEColumn
			{
				get
				{
					return this.columnCREATED_BY_RES_NAME;
				}
			}

			// Token: 0x1700017C RID: 380
			// (get) Token: 0x06000531 RID: 1329 RVA: 0x00013264 File Offset: 0x00011464
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public DataColumn LAST_UPDATED_BY_RES_NAMEColumn
			{
				get
				{
					return this.columnLAST_UPDATED_BY_RES_NAME;
				}
			}

			// Token: 0x1700017D RID: 381
			// (get) Token: 0x06000532 RID: 1330 RVA: 0x0001326C File Offset: 0x0001146C
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			public DataColumn HIRING_TYPEColumn
			{
				get
				{
					return this.columnHIRING_TYPE;
				}
			}

			// Token: 0x1700017E RID: 382
			// (get) Token: 0x06000533 RID: 1331 RVA: 0x00013274 File Offset: 0x00011474
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public DataColumn OPT_ENF_SCHEDULING_CONSColumn
			{
				get
				{
					return this.columnOPT_ENF_SCHEDULING_CONS;
				}
			}

			// Token: 0x1700017F RID: 383
			// (get) Token: 0x06000534 RID: 1332 RVA: 0x0001327C File Offset: 0x0001147C
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public DataColumn OPT_ENF_PROJ_DEPColumn
			{
				get
				{
					return this.columnOPT_ENF_PROJ_DEP;
				}
			}

			// Token: 0x17000180 RID: 384
			// (get) Token: 0x06000535 RID: 1333 RVA: 0x00013284 File Offset: 0x00011484
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public DataColumn RATE_TABLEColumn
			{
				get
				{
					return this.columnRATE_TABLE;
				}
			}

			// Token: 0x17000181 RID: 385
			// (get) Token: 0x06000536 RID: 1334 RVA: 0x0001328C File Offset: 0x0001148C
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public DataColumn ALLOCATION_THRESHOLDColumn
			{
				get
				{
					return this.columnALLOCATION_THRESHOLD;
				}
			}

			// Token: 0x17000182 RID: 386
			// (get) Token: 0x06000537 RID: 1335 RVA: 0x00013294 File Offset: 0x00011494
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

			// Token: 0x17000183 RID: 387
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public AnalysisDataSet.AnalysisPlannerSolutionsRow this[int index]
			{
				get
				{
					return (AnalysisDataSet.AnalysisPlannerSolutionsRow)base.Rows[index];
				}
			}

			// Token: 0x1400003D RID: 61
			// (add) Token: 0x06000539 RID: 1337 RVA: 0x000132B4 File Offset: 0x000114B4
			// (remove) Token: 0x0600053A RID: 1338 RVA: 0x000132EC File Offset: 0x000114EC
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public event AnalysisDataSet.AnalysisPlannerSolutionsRowChangeEventHandler AnalysisPlannerSolutionsRowChanging;

			// Token: 0x1400003E RID: 62
			// (add) Token: 0x0600053B RID: 1339 RVA: 0x00013324 File Offset: 0x00011524
			// (remove) Token: 0x0600053C RID: 1340 RVA: 0x0001335C File Offset: 0x0001155C
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public event AnalysisDataSet.AnalysisPlannerSolutionsRowChangeEventHandler AnalysisPlannerSolutionsRowChanged;

			// Token: 0x1400003F RID: 63
			// (add) Token: 0x0600053D RID: 1341 RVA: 0x00013394 File Offset: 0x00011594
			// (remove) Token: 0x0600053E RID: 1342 RVA: 0x000133CC File Offset: 0x000115CC
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public event AnalysisDataSet.AnalysisPlannerSolutionsRowChangeEventHandler AnalysisPlannerSolutionsRowDeleting;

			// Token: 0x14000040 RID: 64
			// (add) Token: 0x0600053F RID: 1343 RVA: 0x00013404 File Offset: 0x00011604
			// (remove) Token: 0x06000540 RID: 1344 RVA: 0x0001343C File Offset: 0x0001163C
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public event AnalysisDataSet.AnalysisPlannerSolutionsRowChangeEventHandler AnalysisPlannerSolutionsRowDeleted;

			// Token: 0x06000541 RID: 1345 RVA: 0x00013471 File Offset: 0x00011671
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public void AddAnalysisPlannerSolutionsRow(AnalysisDataSet.AnalysisPlannerSolutionsRow row)
			{
				base.Rows.Add(row);
			}

			// Token: 0x06000542 RID: 1346 RVA: 0x00013480 File Offset: 0x00011680
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			public AnalysisDataSet.AnalysisPlannerSolutionsRow AddAnalysisPlannerSolutionsRow(Guid SOLUTION_UID, Guid OPTIMIZER_SOLUTION_UID, AnalysisDataSet.AnalysisRow parentAnalysisRowByFK_Analysis_AnalysisPlannerSolutions, string SOLUTION_NAME, string SOLUTION_DESCRIPTION, byte CONSTRAINT_TYPE, decimal CONSTRAINT_VALUE, Guid FRONTIER_UID, DateTime MOD_DATE, DateTime CREATED_DATE, Guid CREATED_BY_RES_UID, Guid LAST_UPDATED_BY_RES_UID, string CREATED_BY_RES_NAME, string LAST_UPDATED_BY_RES_NAME, byte HIRING_TYPE, bool OPT_ENF_SCHEDULING_CONS, bool OPT_ENF_PROJ_DEP, byte RATE_TABLE, double ALLOCATION_THRESHOLD)
			{
				AnalysisDataSet.AnalysisPlannerSolutionsRow analysisPlannerSolutionsRow = (AnalysisDataSet.AnalysisPlannerSolutionsRow)base.NewRow();
				object[] array = new object[]
				{
					SOLUTION_UID,
					OPTIMIZER_SOLUTION_UID,
					null,
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
					ALLOCATION_THRESHOLD
				};
				if (parentAnalysisRowByFK_Analysis_AnalysisPlannerSolutions != null)
				{
					array[2] = parentAnalysisRowByFK_Analysis_AnalysisPlannerSolutions[0];
				}
				analysisPlannerSolutionsRow.ItemArray = array;
				base.Rows.Add(analysisPlannerSolutionsRow);
				return analysisPlannerSolutionsRow;
			}

			// Token: 0x06000543 RID: 1347 RVA: 0x0001356C File Offset: 0x0001176C
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public AnalysisDataSet.AnalysisPlannerSolutionsRow FindBySOLUTION_UID(Guid SOLUTION_UID)
			{
				return (AnalysisDataSet.AnalysisPlannerSolutionsRow)base.Rows.Find(new object[]
				{
					SOLUTION_UID
				});
			}

			// Token: 0x06000544 RID: 1348 RVA: 0x0001359A File Offset: 0x0001179A
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			public virtual IEnumerator GetEnumerator()
			{
				return base.Rows.GetEnumerator();
			}

			// Token: 0x06000545 RID: 1349 RVA: 0x000135A8 File Offset: 0x000117A8
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			public override DataTable Clone()
			{
				AnalysisDataSet.AnalysisPlannerSolutionsDataTable analysisPlannerSolutionsDataTable = (AnalysisDataSet.AnalysisPlannerSolutionsDataTable)base.Clone();
				analysisPlannerSolutionsDataTable.InitVars();
				return analysisPlannerSolutionsDataTable;
			}

			// Token: 0x06000546 RID: 1350 RVA: 0x000135C8 File Offset: 0x000117C8
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			protected override DataTable CreateInstance()
			{
				return new AnalysisDataSet.AnalysisPlannerSolutionsDataTable();
			}

			// Token: 0x06000547 RID: 1351 RVA: 0x000135D0 File Offset: 0x000117D0
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
			}

			// Token: 0x06000548 RID: 1352 RVA: 0x00013780 File Offset: 0x00011980
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
				base.Constraints.Add(new UniqueConstraint("AnalysisPlannerSolutions_Constraint1", new DataColumn[]
				{
					this.columnSOLUTION_UID
				}, true));
				this.columnSOLUTION_UID.AllowDBNull = false;
				this.columnSOLUTION_UID.ReadOnly = true;
				this.columnSOLUTION_UID.Unique = true;
				this.columnOPTIMIZER_SOLUTION_UID.AllowDBNull = false;
				this.columnOPTIMIZER_SOLUTION_UID.ReadOnly = true;
				this.columnANALYSIS_UID.AllowDBNull = false;
				this.columnANALYSIS_UID.ReadOnly = true;
				this.columnSOLUTION_NAME.AllowDBNull = false;
				this.columnSOLUTION_NAME.ReadOnly = true;
				this.columnSOLUTION_DESCRIPTION.ReadOnly = true;
				this.columnCONSTRAINT_TYPE.AllowDBNull = false;
				this.columnCONSTRAINT_TYPE.ReadOnly = true;
				this.columnCONSTRAINT_VALUE.AllowDBNull = false;
				this.columnCONSTRAINT_VALUE.ReadOnly = true;
				this.columnFRONTIER_UID.ReadOnly = true;
				this.columnMOD_DATE.ReadOnly = true;
				this.columnCREATED_DATE.ReadOnly = true;
				this.columnCREATED_BY_RES_UID.ReadOnly = true;
				this.columnLAST_UPDATED_BY_RES_UID.ReadOnly = true;
				this.columnCREATED_BY_RES_NAME.ReadOnly = true;
				this.columnLAST_UPDATED_BY_RES_NAME.ReadOnly = true;
				this.columnHIRING_TYPE.AllowDBNull = false;
				this.columnHIRING_TYPE.ReadOnly = true;
				this.columnOPT_ENF_SCHEDULING_CONS.AllowDBNull = false;
				this.columnOPT_ENF_SCHEDULING_CONS.ReadOnly = true;
				this.columnOPT_ENF_PROJ_DEP.AllowDBNull = false;
				this.columnOPT_ENF_PROJ_DEP.ReadOnly = true;
				this.columnRATE_TABLE.AllowDBNull = false;
				this.columnRATE_TABLE.ReadOnly = true;
				this.columnALLOCATION_THRESHOLD.AllowDBNull = false;
				this.columnALLOCATION_THRESHOLD.ReadOnly = true;
				this.columnALLOCATION_THRESHOLD.DefaultValue = 1.0;
			}

			// Token: 0x06000549 RID: 1353 RVA: 0x00013C98 File Offset: 0x00011E98
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public AnalysisDataSet.AnalysisPlannerSolutionsRow NewAnalysisPlannerSolutionsRow()
			{
				return (AnalysisDataSet.AnalysisPlannerSolutionsRow)base.NewRow();
			}

			// Token: 0x0600054A RID: 1354 RVA: 0x00013CA5 File Offset: 0x00011EA5
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			protected override DataRow NewRowFromBuilder(DataRowBuilder builder)
			{
				return new AnalysisDataSet.AnalysisPlannerSolutionsRow(builder);
			}

			// Token: 0x0600054B RID: 1355 RVA: 0x00013CAD File Offset: 0x00011EAD
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			protected override Type GetRowType()
			{
				return typeof(AnalysisDataSet.AnalysisPlannerSolutionsRow);
			}

			// Token: 0x0600054C RID: 1356 RVA: 0x00013CB9 File Offset: 0x00011EB9
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			protected override void OnRowChanged(DataRowChangeEventArgs e)
			{
				base.OnRowChanged(e);
				if (this.AnalysisPlannerSolutionsRowChanged != null)
				{
					this.AnalysisPlannerSolutionsRowChanged(this, new AnalysisDataSet.AnalysisPlannerSolutionsRowChangeEvent((AnalysisDataSet.AnalysisPlannerSolutionsRow)e.Row, e.Action));
				}
			}

			// Token: 0x0600054D RID: 1357 RVA: 0x00013CEC File Offset: 0x00011EEC
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			protected override void OnRowChanging(DataRowChangeEventArgs e)
			{
				base.OnRowChanging(e);
				if (this.AnalysisPlannerSolutionsRowChanging != null)
				{
					this.AnalysisPlannerSolutionsRowChanging(this, new AnalysisDataSet.AnalysisPlannerSolutionsRowChangeEvent((AnalysisDataSet.AnalysisPlannerSolutionsRow)e.Row, e.Action));
				}
			}

			// Token: 0x0600054E RID: 1358 RVA: 0x00013D1F File Offset: 0x00011F1F
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			protected override void OnRowDeleted(DataRowChangeEventArgs e)
			{
				base.OnRowDeleted(e);
				if (this.AnalysisPlannerSolutionsRowDeleted != null)
				{
					this.AnalysisPlannerSolutionsRowDeleted(this, new AnalysisDataSet.AnalysisPlannerSolutionsRowChangeEvent((AnalysisDataSet.AnalysisPlannerSolutionsRow)e.Row, e.Action));
				}
			}

			// Token: 0x0600054F RID: 1359 RVA: 0x00013D52 File Offset: 0x00011F52
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			protected override void OnRowDeleting(DataRowChangeEventArgs e)
			{
				base.OnRowDeleting(e);
				if (this.AnalysisPlannerSolutionsRowDeleting != null)
				{
					this.AnalysisPlannerSolutionsRowDeleting(this, new AnalysisDataSet.AnalysisPlannerSolutionsRowChangeEvent((AnalysisDataSet.AnalysisPlannerSolutionsRow)e.Row, e.Action));
				}
			}

			// Token: 0x06000550 RID: 1360 RVA: 0x00013D85 File Offset: 0x00011F85
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			public void RemoveAnalysisPlannerSolutionsRow(AnalysisDataSet.AnalysisPlannerSolutionsRow row)
			{
				base.Rows.Remove(row);
			}

			// Token: 0x06000551 RID: 1361 RVA: 0x00013D94 File Offset: 0x00011F94
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public static XmlSchemaComplexType GetTypedTableSchema(XmlSchemaSet xs)
			{
				XmlSchemaComplexType xmlSchemaComplexType = new XmlSchemaComplexType();
				XmlSchemaSequence xmlSchemaSequence = new XmlSchemaSequence();
				AnalysisDataSet analysisDataSet = new AnalysisDataSet();
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
				xmlSchemaAttribute.FixedValue = analysisDataSet.Namespace;
				xmlSchemaComplexType.Attributes.Add(xmlSchemaAttribute);
				XmlSchemaAttribute xmlSchemaAttribute2 = new XmlSchemaAttribute();
				xmlSchemaAttribute2.Name = "tableTypeName";
				xmlSchemaAttribute2.FixedValue = "AnalysisPlannerSolutionsDataTable";
				xmlSchemaComplexType.Attributes.Add(xmlSchemaAttribute2);
				xmlSchemaComplexType.Particle = xmlSchemaSequence;
				XmlSchema schemaSerializable = analysisDataSet.GetSchemaSerializable();
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

			// Token: 0x04000183 RID: 387
			private DataColumn columnSOLUTION_UID;

			// Token: 0x04000184 RID: 388
			private DataColumn columnOPTIMIZER_SOLUTION_UID;

			// Token: 0x04000185 RID: 389
			private DataColumn columnANALYSIS_UID;

			// Token: 0x04000186 RID: 390
			private DataColumn columnSOLUTION_NAME;

			// Token: 0x04000187 RID: 391
			private DataColumn columnSOLUTION_DESCRIPTION;

			// Token: 0x04000188 RID: 392
			private DataColumn columnCONSTRAINT_TYPE;

			// Token: 0x04000189 RID: 393
			private DataColumn columnCONSTRAINT_VALUE;

			// Token: 0x0400018A RID: 394
			private DataColumn columnFRONTIER_UID;

			// Token: 0x0400018B RID: 395
			private DataColumn columnMOD_DATE;

			// Token: 0x0400018C RID: 396
			private DataColumn columnCREATED_DATE;

			// Token: 0x0400018D RID: 397
			private DataColumn columnCREATED_BY_RES_UID;

			// Token: 0x0400018E RID: 398
			private DataColumn columnLAST_UPDATED_BY_RES_UID;

			// Token: 0x0400018F RID: 399
			private DataColumn columnCREATED_BY_RES_NAME;

			// Token: 0x04000190 RID: 400
			private DataColumn columnLAST_UPDATED_BY_RES_NAME;

			// Token: 0x04000191 RID: 401
			private DataColumn columnHIRING_TYPE;

			// Token: 0x04000192 RID: 402
			private DataColumn columnOPT_ENF_SCHEDULING_CONS;

			// Token: 0x04000193 RID: 403
			private DataColumn columnOPT_ENF_PROJ_DEP;

			// Token: 0x04000194 RID: 404
			private DataColumn columnRATE_TABLE;

			// Token: 0x04000195 RID: 405
			private DataColumn columnALLOCATION_THRESHOLD;
		}

		// Token: 0x02000053 RID: 83
		[XmlSchemaProvider("GetTypedTableSchema")]
		[Serializable]
		public class AnalysisRemainingCapacityByRoleDataTable : DataTable, IEnumerable
		{
			// Token: 0x06000552 RID: 1362 RVA: 0x00013F8C File Offset: 0x0001218C
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public AnalysisRemainingCapacityByRoleDataTable()
			{
				base.TableName = "AnalysisRemainingCapacityByRole";
				this.BeginInit();
				this.InitClass();
				this.EndInit();
			}

			// Token: 0x06000553 RID: 1363 RVA: 0x00013FB4 File Offset: 0x000121B4
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			internal AnalysisRemainingCapacityByRoleDataTable(DataTable table)
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

			// Token: 0x06000554 RID: 1364 RVA: 0x0001405C File Offset: 0x0001225C
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			protected AnalysisRemainingCapacityByRoleDataTable(SerializationInfo info, StreamingContext context) : base(info, context)
			{
				this.InitVars();
			}

			// Token: 0x17000184 RID: 388
			// (get) Token: 0x06000555 RID: 1365 RVA: 0x0001406C File Offset: 0x0001226C
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			public DataColumn ANALYSIS_UIDColumn
			{
				get
				{
					return this.columnANALYSIS_UID;
				}
			}

			// Token: 0x17000185 RID: 389
			// (get) Token: 0x06000556 RID: 1366 RVA: 0x00014074 File Offset: 0x00012274
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public DataColumn CUSTOM_FIELD_UIDColumn
			{
				get
				{
					return this.columnCUSTOM_FIELD_UID;
				}
			}

			// Token: 0x17000186 RID: 390
			// (get) Token: 0x06000557 RID: 1367 RVA: 0x0001407C File Offset: 0x0001227C
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public DataColumn LT_STRUCT_UIDColumn
			{
				get
				{
					return this.columnLT_STRUCT_UID;
				}
			}

			// Token: 0x17000187 RID: 391
			// (get) Token: 0x06000558 RID: 1368 RVA: 0x00014084 File Offset: 0x00012284
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			public DataColumn START_DATEColumn
			{
				get
				{
					return this.columnSTART_DATE;
				}
			}

			// Token: 0x17000188 RID: 392
			// (get) Token: 0x06000559 RID: 1369 RVA: 0x0001408C File Offset: 0x0001228C
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			public DataColumn END_DATEColumn
			{
				get
				{
					return this.columnEND_DATE;
				}
			}

			// Token: 0x17000189 RID: 393
			// (get) Token: 0x0600055A RID: 1370 RVA: 0x00014094 File Offset: 0x00012294
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			public DataColumn REM_CAPACITYColumn
			{
				get
				{
					return this.columnREM_CAPACITY;
				}
			}

			// Token: 0x1700018A RID: 394
			// (get) Token: 0x0600055B RID: 1371 RVA: 0x0001409C File Offset: 0x0001229C
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			public DataColumn REM_CAPACITY_UIDColumn
			{
				get
				{
					return this.columnREM_CAPACITY_UID;
				}
			}

			// Token: 0x1700018B RID: 395
			// (get) Token: 0x0600055C RID: 1372 RVA: 0x000140A4 File Offset: 0x000122A4
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

			// Token: 0x1700018C RID: 396
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			public AnalysisDataSet.AnalysisRemainingCapacityByRoleRow this[int index]
			{
				get
				{
					return (AnalysisDataSet.AnalysisRemainingCapacityByRoleRow)base.Rows[index];
				}
			}

			// Token: 0x14000041 RID: 65
			// (add) Token: 0x0600055E RID: 1374 RVA: 0x000140C4 File Offset: 0x000122C4
			// (remove) Token: 0x0600055F RID: 1375 RVA: 0x000140FC File Offset: 0x000122FC
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public event AnalysisDataSet.AnalysisRemainingCapacityByRoleRowChangeEventHandler AnalysisRemainingCapacityByRoleRowChanging;

			// Token: 0x14000042 RID: 66
			// (add) Token: 0x06000560 RID: 1376 RVA: 0x00014134 File Offset: 0x00012334
			// (remove) Token: 0x06000561 RID: 1377 RVA: 0x0001416C File Offset: 0x0001236C
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public event AnalysisDataSet.AnalysisRemainingCapacityByRoleRowChangeEventHandler AnalysisRemainingCapacityByRoleRowChanged;

			// Token: 0x14000043 RID: 67
			// (add) Token: 0x06000562 RID: 1378 RVA: 0x000141A4 File Offset: 0x000123A4
			// (remove) Token: 0x06000563 RID: 1379 RVA: 0x000141DC File Offset: 0x000123DC
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public event AnalysisDataSet.AnalysisRemainingCapacityByRoleRowChangeEventHandler AnalysisRemainingCapacityByRoleRowDeleting;

			// Token: 0x14000044 RID: 68
			// (add) Token: 0x06000564 RID: 1380 RVA: 0x00014214 File Offset: 0x00012414
			// (remove) Token: 0x06000565 RID: 1381 RVA: 0x0001424C File Offset: 0x0001244C
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public event AnalysisDataSet.AnalysisRemainingCapacityByRoleRowChangeEventHandler AnalysisRemainingCapacityByRoleRowDeleted;

			// Token: 0x06000566 RID: 1382 RVA: 0x00014281 File Offset: 0x00012481
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			public void AddAnalysisRemainingCapacityByRoleRow(AnalysisDataSet.AnalysisRemainingCapacityByRoleRow row)
			{
				base.Rows.Add(row);
			}

			// Token: 0x06000567 RID: 1383 RVA: 0x00014290 File Offset: 0x00012490
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			public AnalysisDataSet.AnalysisRemainingCapacityByRoleRow AddAnalysisRemainingCapacityByRoleRow(Guid ANALYSIS_UID, Guid CUSTOM_FIELD_UID, Guid LT_STRUCT_UID, DateTime START_DATE, DateTime END_DATE, decimal REM_CAPACITY, Guid REM_CAPACITY_UID)
			{
				AnalysisDataSet.AnalysisRemainingCapacityByRoleRow analysisRemainingCapacityByRoleRow = (AnalysisDataSet.AnalysisRemainingCapacityByRoleRow)base.NewRow();
				object[] itemArray = new object[]
				{
					ANALYSIS_UID,
					CUSTOM_FIELD_UID,
					LT_STRUCT_UID,
					START_DATE,
					END_DATE,
					REM_CAPACITY,
					REM_CAPACITY_UID
				};
				analysisRemainingCapacityByRoleRow.ItemArray = itemArray;
				base.Rows.Add(analysisRemainingCapacityByRoleRow);
				return analysisRemainingCapacityByRoleRow;
			}

			// Token: 0x06000568 RID: 1384 RVA: 0x0001430C File Offset: 0x0001250C
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			public AnalysisDataSet.AnalysisRemainingCapacityByRoleRow FindByREM_CAPACITY_UID(Guid REM_CAPACITY_UID)
			{
				return (AnalysisDataSet.AnalysisRemainingCapacityByRoleRow)base.Rows.Find(new object[]
				{
					REM_CAPACITY_UID
				});
			}

			// Token: 0x06000569 RID: 1385 RVA: 0x0001433A File Offset: 0x0001253A
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public virtual IEnumerator GetEnumerator()
			{
				return base.Rows.GetEnumerator();
			}

			// Token: 0x0600056A RID: 1386 RVA: 0x00014348 File Offset: 0x00012548
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public override DataTable Clone()
			{
				AnalysisDataSet.AnalysisRemainingCapacityByRoleDataTable analysisRemainingCapacityByRoleDataTable = (AnalysisDataSet.AnalysisRemainingCapacityByRoleDataTable)base.Clone();
				analysisRemainingCapacityByRoleDataTable.InitVars();
				return analysisRemainingCapacityByRoleDataTable;
			}

			// Token: 0x0600056B RID: 1387 RVA: 0x00014368 File Offset: 0x00012568
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			protected override DataTable CreateInstance()
			{
				return new AnalysisDataSet.AnalysisRemainingCapacityByRoleDataTable();
			}

			// Token: 0x0600056C RID: 1388 RVA: 0x00014370 File Offset: 0x00012570
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			internal void InitVars()
			{
				this.columnANALYSIS_UID = base.Columns["ANALYSIS_UID"];
				this.columnCUSTOM_FIELD_UID = base.Columns["CUSTOM_FIELD_UID"];
				this.columnLT_STRUCT_UID = base.Columns["LT_STRUCT_UID"];
				this.columnSTART_DATE = base.Columns["START_DATE"];
				this.columnEND_DATE = base.Columns["END_DATE"];
				this.columnREM_CAPACITY = base.Columns["REM_CAPACITY"];
				this.columnREM_CAPACITY_UID = base.Columns["REM_CAPACITY_UID"];
			}

			// Token: 0x0600056D RID: 1389 RVA: 0x00014418 File Offset: 0x00012618
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			private void InitClass()
			{
				this.columnANALYSIS_UID = new DataColumn("ANALYSIS_UID", typeof(Guid), null, MappingType.Element);
				base.Columns.Add(this.columnANALYSIS_UID);
				this.columnCUSTOM_FIELD_UID = new DataColumn("CUSTOM_FIELD_UID", typeof(Guid), null, MappingType.Element);
				base.Columns.Add(this.columnCUSTOM_FIELD_UID);
				this.columnLT_STRUCT_UID = new DataColumn("LT_STRUCT_UID", typeof(Guid), null, MappingType.Element);
				base.Columns.Add(this.columnLT_STRUCT_UID);
				this.columnSTART_DATE = new DataColumn("START_DATE", typeof(DateTime), null, MappingType.Element);
				base.Columns.Add(this.columnSTART_DATE);
				this.columnEND_DATE = new DataColumn("END_DATE", typeof(DateTime), null, MappingType.Element);
				base.Columns.Add(this.columnEND_DATE);
				this.columnREM_CAPACITY = new DataColumn("REM_CAPACITY", typeof(decimal), null, MappingType.Element);
				base.Columns.Add(this.columnREM_CAPACITY);
				this.columnREM_CAPACITY_UID = new DataColumn("REM_CAPACITY_UID", typeof(Guid), null, MappingType.Element);
				base.Columns.Add(this.columnREM_CAPACITY_UID);
				base.Constraints.Add(new UniqueConstraint("AnalysisRemainingCapacityByRole_Constraint1", new DataColumn[]
				{
					this.columnREM_CAPACITY_UID
				}, true));
				this.columnANALYSIS_UID.AllowDBNull = false;
				this.columnANALYSIS_UID.ReadOnly = true;
				this.columnCUSTOM_FIELD_UID.AllowDBNull = false;
				this.columnCUSTOM_FIELD_UID.ReadOnly = true;
				this.columnLT_STRUCT_UID.AllowDBNull = false;
				this.columnLT_STRUCT_UID.ReadOnly = true;
				this.columnSTART_DATE.AllowDBNull = false;
				this.columnSTART_DATE.ReadOnly = true;
				this.columnEND_DATE.AllowDBNull = false;
				this.columnEND_DATE.ReadOnly = true;
				this.columnREM_CAPACITY.AllowDBNull = false;
				this.columnREM_CAPACITY.ReadOnly = true;
				this.columnREM_CAPACITY.DefaultValue = 0m;
				this.columnREM_CAPACITY_UID.AllowDBNull = false;
				this.columnREM_CAPACITY_UID.ReadOnly = true;
				this.columnREM_CAPACITY_UID.Unique = true;
			}

			// Token: 0x0600056E RID: 1390 RVA: 0x00014651 File Offset: 0x00012851
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			public AnalysisDataSet.AnalysisRemainingCapacityByRoleRow NewAnalysisRemainingCapacityByRoleRow()
			{
				return (AnalysisDataSet.AnalysisRemainingCapacityByRoleRow)base.NewRow();
			}

			// Token: 0x0600056F RID: 1391 RVA: 0x0001465E File Offset: 0x0001285E
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			protected override DataRow NewRowFromBuilder(DataRowBuilder builder)
			{
				return new AnalysisDataSet.AnalysisRemainingCapacityByRoleRow(builder);
			}

			// Token: 0x06000570 RID: 1392 RVA: 0x00014666 File Offset: 0x00012866
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			protected override Type GetRowType()
			{
				return typeof(AnalysisDataSet.AnalysisRemainingCapacityByRoleRow);
			}

			// Token: 0x06000571 RID: 1393 RVA: 0x00014672 File Offset: 0x00012872
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			protected override void OnRowChanged(DataRowChangeEventArgs e)
			{
				base.OnRowChanged(e);
				if (this.AnalysisRemainingCapacityByRoleRowChanged != null)
				{
					this.AnalysisRemainingCapacityByRoleRowChanged(this, new AnalysisDataSet.AnalysisRemainingCapacityByRoleRowChangeEvent((AnalysisDataSet.AnalysisRemainingCapacityByRoleRow)e.Row, e.Action));
				}
			}

			// Token: 0x06000572 RID: 1394 RVA: 0x000146A5 File Offset: 0x000128A5
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			protected override void OnRowChanging(DataRowChangeEventArgs e)
			{
				base.OnRowChanging(e);
				if (this.AnalysisRemainingCapacityByRoleRowChanging != null)
				{
					this.AnalysisRemainingCapacityByRoleRowChanging(this, new AnalysisDataSet.AnalysisRemainingCapacityByRoleRowChangeEvent((AnalysisDataSet.AnalysisRemainingCapacityByRoleRow)e.Row, e.Action));
				}
			}

			// Token: 0x06000573 RID: 1395 RVA: 0x000146D8 File Offset: 0x000128D8
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			protected override void OnRowDeleted(DataRowChangeEventArgs e)
			{
				base.OnRowDeleted(e);
				if (this.AnalysisRemainingCapacityByRoleRowDeleted != null)
				{
					this.AnalysisRemainingCapacityByRoleRowDeleted(this, new AnalysisDataSet.AnalysisRemainingCapacityByRoleRowChangeEvent((AnalysisDataSet.AnalysisRemainingCapacityByRoleRow)e.Row, e.Action));
				}
			}

			// Token: 0x06000574 RID: 1396 RVA: 0x0001470B File Offset: 0x0001290B
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			protected override void OnRowDeleting(DataRowChangeEventArgs e)
			{
				base.OnRowDeleting(e);
				if (this.AnalysisRemainingCapacityByRoleRowDeleting != null)
				{
					this.AnalysisRemainingCapacityByRoleRowDeleting(this, new AnalysisDataSet.AnalysisRemainingCapacityByRoleRowChangeEvent((AnalysisDataSet.AnalysisRemainingCapacityByRoleRow)e.Row, e.Action));
				}
			}

			// Token: 0x06000575 RID: 1397 RVA: 0x0001473E File Offset: 0x0001293E
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public void RemoveAnalysisRemainingCapacityByRoleRow(AnalysisDataSet.AnalysisRemainingCapacityByRoleRow row)
			{
				base.Rows.Remove(row);
			}

			// Token: 0x06000576 RID: 1398 RVA: 0x0001474C File Offset: 0x0001294C
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public static XmlSchemaComplexType GetTypedTableSchema(XmlSchemaSet xs)
			{
				XmlSchemaComplexType xmlSchemaComplexType = new XmlSchemaComplexType();
				XmlSchemaSequence xmlSchemaSequence = new XmlSchemaSequence();
				AnalysisDataSet analysisDataSet = new AnalysisDataSet();
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
				xmlSchemaAttribute.FixedValue = analysisDataSet.Namespace;
				xmlSchemaComplexType.Attributes.Add(xmlSchemaAttribute);
				XmlSchemaAttribute xmlSchemaAttribute2 = new XmlSchemaAttribute();
				xmlSchemaAttribute2.Name = "tableTypeName";
				xmlSchemaAttribute2.FixedValue = "AnalysisRemainingCapacityByRoleDataTable";
				xmlSchemaComplexType.Attributes.Add(xmlSchemaAttribute2);
				xmlSchemaComplexType.Particle = xmlSchemaSequence;
				XmlSchema schemaSerializable = analysisDataSet.GetSchemaSerializable();
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

			// Token: 0x0400019A RID: 410
			private DataColumn columnANALYSIS_UID;

			// Token: 0x0400019B RID: 411
			private DataColumn columnCUSTOM_FIELD_UID;

			// Token: 0x0400019C RID: 412
			private DataColumn columnLT_STRUCT_UID;

			// Token: 0x0400019D RID: 413
			private DataColumn columnSTART_DATE;

			// Token: 0x0400019E RID: 414
			private DataColumn columnEND_DATE;

			// Token: 0x0400019F RID: 415
			private DataColumn columnREM_CAPACITY;

			// Token: 0x040001A0 RID: 416
			private DataColumn columnREM_CAPACITY_UID;
		}

		// Token: 0x02000054 RID: 84
		[XmlSchemaProvider("GetTypedTableSchema")]
		[Serializable]
		public class AnalysisRoleRatesDataTable : DataTable, IEnumerable
		{
			// Token: 0x06000577 RID: 1399 RVA: 0x00014944 File Offset: 0x00012B44
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			public AnalysisRoleRatesDataTable()
			{
				base.TableName = "AnalysisRoleRates";
				this.BeginInit();
				this.InitClass();
				this.EndInit();
			}

			// Token: 0x06000578 RID: 1400 RVA: 0x0001496C File Offset: 0x00012B6C
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			internal AnalysisRoleRatesDataTable(DataTable table)
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

			// Token: 0x06000579 RID: 1401 RVA: 0x00014A14 File Offset: 0x00012C14
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			protected AnalysisRoleRatesDataTable(SerializationInfo info, StreamingContext context) : base(info, context)
			{
				this.InitVars();
			}

			// Token: 0x1700018D RID: 397
			// (get) Token: 0x0600057A RID: 1402 RVA: 0x00014A24 File Offset: 0x00012C24
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public DataColumn ANALYSIS_UIDColumn
			{
				get
				{
					return this.columnANALYSIS_UID;
				}
			}

			// Token: 0x1700018E RID: 398
			// (get) Token: 0x0600057B RID: 1403 RVA: 0x00014A2C File Offset: 0x00012C2C
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			public DataColumn CUSTOM_FIELD_UIDColumn
			{
				get
				{
					return this.columnCUSTOM_FIELD_UID;
				}
			}

			// Token: 0x1700018F RID: 399
			// (get) Token: 0x0600057C RID: 1404 RVA: 0x00014A34 File Offset: 0x00012C34
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public DataColumn LT_STRUCT_UIDColumn
			{
				get
				{
					return this.columnLT_STRUCT_UID;
				}
			}

			// Token: 0x17000190 RID: 400
			// (get) Token: 0x0600057D RID: 1405 RVA: 0x00014A3C File Offset: 0x00012C3C
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			public DataColumn STANDARD_RATEColumn
			{
				get
				{
					return this.columnSTANDARD_RATE;
				}
			}

			// Token: 0x17000191 RID: 401
			// (get) Token: 0x0600057E RID: 1406 RVA: 0x00014A44 File Offset: 0x00012C44
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			public DataColumn RATE_TABLEColumn
			{
				get
				{
					return this.columnRATE_TABLE;
				}
			}

			// Token: 0x17000192 RID: 402
			// (get) Token: 0x0600057F RID: 1407 RVA: 0x00014A4C File Offset: 0x00012C4C
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

			// Token: 0x17000193 RID: 403
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			public AnalysisDataSet.AnalysisRoleRatesRow this[int index]
			{
				get
				{
					return (AnalysisDataSet.AnalysisRoleRatesRow)base.Rows[index];
				}
			}

			// Token: 0x14000045 RID: 69
			// (add) Token: 0x06000581 RID: 1409 RVA: 0x00014A6C File Offset: 0x00012C6C
			// (remove) Token: 0x06000582 RID: 1410 RVA: 0x00014AA4 File Offset: 0x00012CA4
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public event AnalysisDataSet.AnalysisRoleRatesRowChangeEventHandler AnalysisRoleRatesRowChanging;

			// Token: 0x14000046 RID: 70
			// (add) Token: 0x06000583 RID: 1411 RVA: 0x00014ADC File Offset: 0x00012CDC
			// (remove) Token: 0x06000584 RID: 1412 RVA: 0x00014B14 File Offset: 0x00012D14
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public event AnalysisDataSet.AnalysisRoleRatesRowChangeEventHandler AnalysisRoleRatesRowChanged;

			// Token: 0x14000047 RID: 71
			// (add) Token: 0x06000585 RID: 1413 RVA: 0x00014B4C File Offset: 0x00012D4C
			// (remove) Token: 0x06000586 RID: 1414 RVA: 0x00014B84 File Offset: 0x00012D84
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public event AnalysisDataSet.AnalysisRoleRatesRowChangeEventHandler AnalysisRoleRatesRowDeleting;

			// Token: 0x14000048 RID: 72
			// (add) Token: 0x06000587 RID: 1415 RVA: 0x00014BBC File Offset: 0x00012DBC
			// (remove) Token: 0x06000588 RID: 1416 RVA: 0x00014BF4 File Offset: 0x00012DF4
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public event AnalysisDataSet.AnalysisRoleRatesRowChangeEventHandler AnalysisRoleRatesRowDeleted;

			// Token: 0x06000589 RID: 1417 RVA: 0x00014C29 File Offset: 0x00012E29
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public void AddAnalysisRoleRatesRow(AnalysisDataSet.AnalysisRoleRatesRow row)
			{
				base.Rows.Add(row);
			}

			// Token: 0x0600058A RID: 1418 RVA: 0x00014C38 File Offset: 0x00012E38
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			public AnalysisDataSet.AnalysisRoleRatesRow AddAnalysisRoleRatesRow(Guid ANALYSIS_UID, Guid CUSTOM_FIELD_UID, Guid LT_STRUCT_UID, double STANDARD_RATE, byte RATE_TABLE)
			{
				AnalysisDataSet.AnalysisRoleRatesRow analysisRoleRatesRow = (AnalysisDataSet.AnalysisRoleRatesRow)base.NewRow();
				object[] itemArray = new object[]
				{
					ANALYSIS_UID,
					CUSTOM_FIELD_UID,
					LT_STRUCT_UID,
					STANDARD_RATE,
					RATE_TABLE
				};
				analysisRoleRatesRow.ItemArray = itemArray;
				base.Rows.Add(analysisRoleRatesRow);
				return analysisRoleRatesRow;
			}

			// Token: 0x0600058B RID: 1419 RVA: 0x00014CA0 File Offset: 0x00012EA0
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public AnalysisDataSet.AnalysisRoleRatesRow FindByANALYSIS_UIDCUSTOM_FIELD_UIDLT_STRUCT_UIDRATE_TABLE(Guid ANALYSIS_UID, Guid CUSTOM_FIELD_UID, Guid LT_STRUCT_UID, byte RATE_TABLE)
			{
				return (AnalysisDataSet.AnalysisRoleRatesRow)base.Rows.Find(new object[]
				{
					ANALYSIS_UID,
					CUSTOM_FIELD_UID,
					LT_STRUCT_UID,
					RATE_TABLE
				});
			}

			// Token: 0x0600058C RID: 1420 RVA: 0x00014CEA File Offset: 0x00012EEA
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public virtual IEnumerator GetEnumerator()
			{
				return base.Rows.GetEnumerator();
			}

			// Token: 0x0600058D RID: 1421 RVA: 0x00014CF8 File Offset: 0x00012EF8
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public override DataTable Clone()
			{
				AnalysisDataSet.AnalysisRoleRatesDataTable analysisRoleRatesDataTable = (AnalysisDataSet.AnalysisRoleRatesDataTable)base.Clone();
				analysisRoleRatesDataTable.InitVars();
				return analysisRoleRatesDataTable;
			}

			// Token: 0x0600058E RID: 1422 RVA: 0x00014D18 File Offset: 0x00012F18
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			protected override DataTable CreateInstance()
			{
				return new AnalysisDataSet.AnalysisRoleRatesDataTable();
			}

			// Token: 0x0600058F RID: 1423 RVA: 0x00014D20 File Offset: 0x00012F20
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			internal void InitVars()
			{
				this.columnANALYSIS_UID = base.Columns["ANALYSIS_UID"];
				this.columnCUSTOM_FIELD_UID = base.Columns["CUSTOM_FIELD_UID"];
				this.columnLT_STRUCT_UID = base.Columns["LT_STRUCT_UID"];
				this.columnSTANDARD_RATE = base.Columns["STANDARD_RATE"];
				this.columnRATE_TABLE = base.Columns["RATE_TABLE"];
			}

			// Token: 0x06000590 RID: 1424 RVA: 0x00014D9C File Offset: 0x00012F9C
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			private void InitClass()
			{
				this.columnANALYSIS_UID = new DataColumn("ANALYSIS_UID", typeof(Guid), null, MappingType.Element);
				base.Columns.Add(this.columnANALYSIS_UID);
				this.columnCUSTOM_FIELD_UID = new DataColumn("CUSTOM_FIELD_UID", typeof(Guid), null, MappingType.Element);
				base.Columns.Add(this.columnCUSTOM_FIELD_UID);
				this.columnLT_STRUCT_UID = new DataColumn("LT_STRUCT_UID", typeof(Guid), null, MappingType.Element);
				base.Columns.Add(this.columnLT_STRUCT_UID);
				this.columnSTANDARD_RATE = new DataColumn("STANDARD_RATE", typeof(double), null, MappingType.Element);
				base.Columns.Add(this.columnSTANDARD_RATE);
				this.columnRATE_TABLE = new DataColumn("RATE_TABLE", typeof(byte), null, MappingType.Element);
				base.Columns.Add(this.columnRATE_TABLE);
				base.Constraints.Add(new UniqueConstraint("AnalysisRoleRates_Constraint1", new DataColumn[]
				{
					this.columnANALYSIS_UID,
					this.columnCUSTOM_FIELD_UID,
					this.columnLT_STRUCT_UID,
					this.columnRATE_TABLE
				}, true));
				this.columnANALYSIS_UID.AllowDBNull = false;
				this.columnANALYSIS_UID.ReadOnly = true;
				this.columnCUSTOM_FIELD_UID.AllowDBNull = false;
				this.columnCUSTOM_FIELD_UID.ReadOnly = true;
				this.columnLT_STRUCT_UID.AllowDBNull = false;
				this.columnLT_STRUCT_UID.ReadOnly = true;
				this.columnSTANDARD_RATE.AllowDBNull = false;
				this.columnSTANDARD_RATE.DefaultValue = 0.0;
				this.columnRATE_TABLE.AllowDBNull = false;
				this.columnRATE_TABLE.ReadOnly = true;
			}

			// Token: 0x06000591 RID: 1425 RVA: 0x00014F51 File Offset: 0x00013151
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public AnalysisDataSet.AnalysisRoleRatesRow NewAnalysisRoleRatesRow()
			{
				return (AnalysisDataSet.AnalysisRoleRatesRow)base.NewRow();
			}

			// Token: 0x06000592 RID: 1426 RVA: 0x00014F5E File Offset: 0x0001315E
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			protected override DataRow NewRowFromBuilder(DataRowBuilder builder)
			{
				return new AnalysisDataSet.AnalysisRoleRatesRow(builder);
			}

			// Token: 0x06000593 RID: 1427 RVA: 0x00014F66 File Offset: 0x00013166
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			protected override Type GetRowType()
			{
				return typeof(AnalysisDataSet.AnalysisRoleRatesRow);
			}

			// Token: 0x06000594 RID: 1428 RVA: 0x00014F72 File Offset: 0x00013172
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			protected override void OnRowChanged(DataRowChangeEventArgs e)
			{
				base.OnRowChanged(e);
				if (this.AnalysisRoleRatesRowChanged != null)
				{
					this.AnalysisRoleRatesRowChanged(this, new AnalysisDataSet.AnalysisRoleRatesRowChangeEvent((AnalysisDataSet.AnalysisRoleRatesRow)e.Row, e.Action));
				}
			}

			// Token: 0x06000595 RID: 1429 RVA: 0x00014FA5 File Offset: 0x000131A5
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			protected override void OnRowChanging(DataRowChangeEventArgs e)
			{
				base.OnRowChanging(e);
				if (this.AnalysisRoleRatesRowChanging != null)
				{
					this.AnalysisRoleRatesRowChanging(this, new AnalysisDataSet.AnalysisRoleRatesRowChangeEvent((AnalysisDataSet.AnalysisRoleRatesRow)e.Row, e.Action));
				}
			}

			// Token: 0x06000596 RID: 1430 RVA: 0x00014FD8 File Offset: 0x000131D8
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			protected override void OnRowDeleted(DataRowChangeEventArgs e)
			{
				base.OnRowDeleted(e);
				if (this.AnalysisRoleRatesRowDeleted != null)
				{
					this.AnalysisRoleRatesRowDeleted(this, new AnalysisDataSet.AnalysisRoleRatesRowChangeEvent((AnalysisDataSet.AnalysisRoleRatesRow)e.Row, e.Action));
				}
			}

			// Token: 0x06000597 RID: 1431 RVA: 0x0001500B File Offset: 0x0001320B
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			protected override void OnRowDeleting(DataRowChangeEventArgs e)
			{
				base.OnRowDeleting(e);
				if (this.AnalysisRoleRatesRowDeleting != null)
				{
					this.AnalysisRoleRatesRowDeleting(this, new AnalysisDataSet.AnalysisRoleRatesRowChangeEvent((AnalysisDataSet.AnalysisRoleRatesRow)e.Row, e.Action));
				}
			}

			// Token: 0x06000598 RID: 1432 RVA: 0x0001503E File Offset: 0x0001323E
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public void RemoveAnalysisRoleRatesRow(AnalysisDataSet.AnalysisRoleRatesRow row)
			{
				base.Rows.Remove(row);
			}

			// Token: 0x06000599 RID: 1433 RVA: 0x0001504C File Offset: 0x0001324C
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public static XmlSchemaComplexType GetTypedTableSchema(XmlSchemaSet xs)
			{
				XmlSchemaComplexType xmlSchemaComplexType = new XmlSchemaComplexType();
				XmlSchemaSequence xmlSchemaSequence = new XmlSchemaSequence();
				AnalysisDataSet analysisDataSet = new AnalysisDataSet();
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
				xmlSchemaAttribute.FixedValue = analysisDataSet.Namespace;
				xmlSchemaComplexType.Attributes.Add(xmlSchemaAttribute);
				XmlSchemaAttribute xmlSchemaAttribute2 = new XmlSchemaAttribute();
				xmlSchemaAttribute2.Name = "tableTypeName";
				xmlSchemaAttribute2.FixedValue = "AnalysisRoleRatesDataTable";
				xmlSchemaComplexType.Attributes.Add(xmlSchemaAttribute2);
				xmlSchemaComplexType.Particle = xmlSchemaSequence;
				XmlSchema schemaSerializable = analysisDataSet.GetSchemaSerializable();
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

			// Token: 0x040001A5 RID: 421
			private DataColumn columnANALYSIS_UID;

			// Token: 0x040001A6 RID: 422
			private DataColumn columnCUSTOM_FIELD_UID;

			// Token: 0x040001A7 RID: 423
			private DataColumn columnLT_STRUCT_UID;

			// Token: 0x040001A8 RID: 424
			private DataColumn columnSTANDARD_RATE;

			// Token: 0x040001A9 RID: 425
			private DataColumn columnRATE_TABLE;
		}

		// Token: 0x02000055 RID: 85
		[XmlSchemaProvider("GetTypedTableSchema")]
		[Serializable]
		public class AnalysisProjectRequirementsByRoleDataTable : DataTable, IEnumerable
		{
			// Token: 0x0600059A RID: 1434 RVA: 0x00015244 File Offset: 0x00013444
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			public AnalysisProjectRequirementsByRoleDataTable()
			{
				base.TableName = "AnalysisProjectRequirementsByRole";
				this.BeginInit();
				this.InitClass();
				this.EndInit();
			}

			// Token: 0x0600059B RID: 1435 RVA: 0x0001526C File Offset: 0x0001346C
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			internal AnalysisProjectRequirementsByRoleDataTable(DataTable table)
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

			// Token: 0x0600059C RID: 1436 RVA: 0x00015314 File Offset: 0x00013514
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			protected AnalysisProjectRequirementsByRoleDataTable(SerializationInfo info, StreamingContext context) : base(info, context)
			{
				this.InitVars();
			}

			// Token: 0x17000194 RID: 404
			// (get) Token: 0x0600059D RID: 1437 RVA: 0x00015324 File Offset: 0x00013524
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public DataColumn ANALYSIS_UIDColumn
			{
				get
				{
					return this.columnANALYSIS_UID;
				}
			}

			// Token: 0x17000195 RID: 405
			// (get) Token: 0x0600059E RID: 1438 RVA: 0x0001532C File Offset: 0x0001352C
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			public DataColumn PROJ_UIDColumn
			{
				get
				{
					return this.columnPROJ_UID;
				}
			}

			// Token: 0x17000196 RID: 406
			// (get) Token: 0x0600059F RID: 1439 RVA: 0x00015334 File Offset: 0x00013534
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public DataColumn CUSTOM_FIELD_UIDColumn
			{
				get
				{
					return this.columnCUSTOM_FIELD_UID;
				}
			}

			// Token: 0x17000197 RID: 407
			// (get) Token: 0x060005A0 RID: 1440 RVA: 0x0001533C File Offset: 0x0001353C
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public DataColumn LT_STRUCT_UIDColumn
			{
				get
				{
					return this.columnLT_STRUCT_UID;
				}
			}

			// Token: 0x17000198 RID: 408
			// (get) Token: 0x060005A1 RID: 1441 RVA: 0x00015344 File Offset: 0x00013544
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			public DataColumn START_DATEColumn
			{
				get
				{
					return this.columnSTART_DATE;
				}
			}

			// Token: 0x17000199 RID: 409
			// (get) Token: 0x060005A2 RID: 1442 RVA: 0x0001534C File Offset: 0x0001354C
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public DataColumn PROJECT_REQUIREMENTColumn
			{
				get
				{
					return this.columnPROJECT_REQUIREMENT;
				}
			}

			// Token: 0x1700019A RID: 410
			// (get) Token: 0x060005A3 RID: 1443 RVA: 0x00015354 File Offset: 0x00013554
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public DataColumn REQUIREMENT_UIDColumn
			{
				get
				{
					return this.columnREQUIREMENT_UID;
				}
			}

			// Token: 0x1700019B RID: 411
			// (get) Token: 0x060005A4 RID: 1444 RVA: 0x0001535C File Offset: 0x0001355C
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

			// Token: 0x1700019C RID: 412
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public AnalysisDataSet.AnalysisProjectRequirementsByRoleRow this[int index]
			{
				get
				{
					return (AnalysisDataSet.AnalysisProjectRequirementsByRoleRow)base.Rows[index];
				}
			}

			// Token: 0x14000049 RID: 73
			// (add) Token: 0x060005A6 RID: 1446 RVA: 0x0001537C File Offset: 0x0001357C
			// (remove) Token: 0x060005A7 RID: 1447 RVA: 0x000153B4 File Offset: 0x000135B4
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public event AnalysisDataSet.AnalysisProjectRequirementsByRoleRowChangeEventHandler AnalysisProjectRequirementsByRoleRowChanging;

			// Token: 0x1400004A RID: 74
			// (add) Token: 0x060005A8 RID: 1448 RVA: 0x000153EC File Offset: 0x000135EC
			// (remove) Token: 0x060005A9 RID: 1449 RVA: 0x00015424 File Offset: 0x00013624
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public event AnalysisDataSet.AnalysisProjectRequirementsByRoleRowChangeEventHandler AnalysisProjectRequirementsByRoleRowChanged;

			// Token: 0x1400004B RID: 75
			// (add) Token: 0x060005AA RID: 1450 RVA: 0x0001545C File Offset: 0x0001365C
			// (remove) Token: 0x060005AB RID: 1451 RVA: 0x00015494 File Offset: 0x00013694
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public event AnalysisDataSet.AnalysisProjectRequirementsByRoleRowChangeEventHandler AnalysisProjectRequirementsByRoleRowDeleting;

			// Token: 0x1400004C RID: 76
			// (add) Token: 0x060005AC RID: 1452 RVA: 0x000154CC File Offset: 0x000136CC
			// (remove) Token: 0x060005AD RID: 1453 RVA: 0x00015504 File Offset: 0x00013704
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public event AnalysisDataSet.AnalysisProjectRequirementsByRoleRowChangeEventHandler AnalysisProjectRequirementsByRoleRowDeleted;

			// Token: 0x060005AE RID: 1454 RVA: 0x00015539 File Offset: 0x00013739
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			public void AddAnalysisProjectRequirementsByRoleRow(AnalysisDataSet.AnalysisProjectRequirementsByRoleRow row)
			{
				base.Rows.Add(row);
			}

			// Token: 0x060005AF RID: 1455 RVA: 0x00015548 File Offset: 0x00013748
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public AnalysisDataSet.AnalysisProjectRequirementsByRoleRow AddAnalysisProjectRequirementsByRoleRow(Guid ANALYSIS_UID, Guid PROJ_UID, Guid CUSTOM_FIELD_UID, Guid LT_STRUCT_UID, DateTime START_DATE, decimal PROJECT_REQUIREMENT, Guid REQUIREMENT_UID)
			{
				AnalysisDataSet.AnalysisProjectRequirementsByRoleRow analysisProjectRequirementsByRoleRow = (AnalysisDataSet.AnalysisProjectRequirementsByRoleRow)base.NewRow();
				object[] itemArray = new object[]
				{
					ANALYSIS_UID,
					PROJ_UID,
					CUSTOM_FIELD_UID,
					LT_STRUCT_UID,
					START_DATE,
					PROJECT_REQUIREMENT,
					REQUIREMENT_UID
				};
				analysisProjectRequirementsByRoleRow.ItemArray = itemArray;
				base.Rows.Add(analysisProjectRequirementsByRoleRow);
				return analysisProjectRequirementsByRoleRow;
			}

			// Token: 0x060005B0 RID: 1456 RVA: 0x000155C4 File Offset: 0x000137C4
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public AnalysisDataSet.AnalysisProjectRequirementsByRoleRow FindByREQUIREMENT_UID(Guid REQUIREMENT_UID)
			{
				return (AnalysisDataSet.AnalysisProjectRequirementsByRoleRow)base.Rows.Find(new object[]
				{
					REQUIREMENT_UID
				});
			}

			// Token: 0x060005B1 RID: 1457 RVA: 0x000155F2 File Offset: 0x000137F2
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			public virtual IEnumerator GetEnumerator()
			{
				return base.Rows.GetEnumerator();
			}

			// Token: 0x060005B2 RID: 1458 RVA: 0x00015600 File Offset: 0x00013800
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public override DataTable Clone()
			{
				AnalysisDataSet.AnalysisProjectRequirementsByRoleDataTable analysisProjectRequirementsByRoleDataTable = (AnalysisDataSet.AnalysisProjectRequirementsByRoleDataTable)base.Clone();
				analysisProjectRequirementsByRoleDataTable.InitVars();
				return analysisProjectRequirementsByRoleDataTable;
			}

			// Token: 0x060005B3 RID: 1459 RVA: 0x00015620 File Offset: 0x00013820
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			protected override DataTable CreateInstance()
			{
				return new AnalysisDataSet.AnalysisProjectRequirementsByRoleDataTable();
			}

			// Token: 0x060005B4 RID: 1460 RVA: 0x00015628 File Offset: 0x00013828
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			internal void InitVars()
			{
				this.columnANALYSIS_UID = base.Columns["ANALYSIS_UID"];
				this.columnPROJ_UID = base.Columns["PROJ_UID"];
				this.columnCUSTOM_FIELD_UID = base.Columns["CUSTOM_FIELD_UID"];
				this.columnLT_STRUCT_UID = base.Columns["LT_STRUCT_UID"];
				this.columnSTART_DATE = base.Columns["START_DATE"];
				this.columnPROJECT_REQUIREMENT = base.Columns["PROJECT_REQUIREMENT"];
				this.columnREQUIREMENT_UID = base.Columns["REQUIREMENT_UID"];
			}

			// Token: 0x060005B5 RID: 1461 RVA: 0x000156D0 File Offset: 0x000138D0
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			private void InitClass()
			{
				this.columnANALYSIS_UID = new DataColumn("ANALYSIS_UID", typeof(Guid), null, MappingType.Element);
				base.Columns.Add(this.columnANALYSIS_UID);
				this.columnPROJ_UID = new DataColumn("PROJ_UID", typeof(Guid), null, MappingType.Element);
				base.Columns.Add(this.columnPROJ_UID);
				this.columnCUSTOM_FIELD_UID = new DataColumn("CUSTOM_FIELD_UID", typeof(Guid), null, MappingType.Element);
				base.Columns.Add(this.columnCUSTOM_FIELD_UID);
				this.columnLT_STRUCT_UID = new DataColumn("LT_STRUCT_UID", typeof(Guid), null, MappingType.Element);
				base.Columns.Add(this.columnLT_STRUCT_UID);
				this.columnSTART_DATE = new DataColumn("START_DATE", typeof(DateTime), null, MappingType.Element);
				base.Columns.Add(this.columnSTART_DATE);
				this.columnPROJECT_REQUIREMENT = new DataColumn("PROJECT_REQUIREMENT", typeof(decimal), null, MappingType.Element);
				base.Columns.Add(this.columnPROJECT_REQUIREMENT);
				this.columnREQUIREMENT_UID = new DataColumn("REQUIREMENT_UID", typeof(Guid), null, MappingType.Element);
				base.Columns.Add(this.columnREQUIREMENT_UID);
				base.Constraints.Add(new UniqueConstraint("AnalysisProjectRequirementsByRole_Constraint1", new DataColumn[]
				{
					this.columnREQUIREMENT_UID
				}, true));
				this.columnANALYSIS_UID.AllowDBNull = false;
				this.columnANALYSIS_UID.ReadOnly = true;
				this.columnPROJ_UID.AllowDBNull = false;
				this.columnPROJ_UID.ReadOnly = true;
				this.columnCUSTOM_FIELD_UID.AllowDBNull = false;
				this.columnCUSTOM_FIELD_UID.ReadOnly = true;
				this.columnLT_STRUCT_UID.AllowDBNull = false;
				this.columnLT_STRUCT_UID.ReadOnly = true;
				this.columnSTART_DATE.AllowDBNull = false;
				this.columnSTART_DATE.ReadOnly = true;
				this.columnPROJECT_REQUIREMENT.AllowDBNull = false;
				this.columnPROJECT_REQUIREMENT.ReadOnly = true;
				this.columnPROJECT_REQUIREMENT.DefaultValue = 0m;
				this.columnREQUIREMENT_UID.AllowDBNull = false;
				this.columnREQUIREMENT_UID.ReadOnly = true;
				this.columnREQUIREMENT_UID.Unique = true;
			}

			// Token: 0x060005B6 RID: 1462 RVA: 0x00015909 File Offset: 0x00013B09
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public AnalysisDataSet.AnalysisProjectRequirementsByRoleRow NewAnalysisProjectRequirementsByRoleRow()
			{
				return (AnalysisDataSet.AnalysisProjectRequirementsByRoleRow)base.NewRow();
			}

			// Token: 0x060005B7 RID: 1463 RVA: 0x00015916 File Offset: 0x00013B16
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			protected override DataRow NewRowFromBuilder(DataRowBuilder builder)
			{
				return new AnalysisDataSet.AnalysisProjectRequirementsByRoleRow(builder);
			}

			// Token: 0x060005B8 RID: 1464 RVA: 0x0001591E File Offset: 0x00013B1E
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			protected override Type GetRowType()
			{
				return typeof(AnalysisDataSet.AnalysisProjectRequirementsByRoleRow);
			}

			// Token: 0x060005B9 RID: 1465 RVA: 0x0001592A File Offset: 0x00013B2A
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			protected override void OnRowChanged(DataRowChangeEventArgs e)
			{
				base.OnRowChanged(e);
				if (this.AnalysisProjectRequirementsByRoleRowChanged != null)
				{
					this.AnalysisProjectRequirementsByRoleRowChanged(this, new AnalysisDataSet.AnalysisProjectRequirementsByRoleRowChangeEvent((AnalysisDataSet.AnalysisProjectRequirementsByRoleRow)e.Row, e.Action));
				}
			}

			// Token: 0x060005BA RID: 1466 RVA: 0x0001595D File Offset: 0x00013B5D
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			protected override void OnRowChanging(DataRowChangeEventArgs e)
			{
				base.OnRowChanging(e);
				if (this.AnalysisProjectRequirementsByRoleRowChanging != null)
				{
					this.AnalysisProjectRequirementsByRoleRowChanging(this, new AnalysisDataSet.AnalysisProjectRequirementsByRoleRowChangeEvent((AnalysisDataSet.AnalysisProjectRequirementsByRoleRow)e.Row, e.Action));
				}
			}

			// Token: 0x060005BB RID: 1467 RVA: 0x00015990 File Offset: 0x00013B90
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			protected override void OnRowDeleted(DataRowChangeEventArgs e)
			{
				base.OnRowDeleted(e);
				if (this.AnalysisProjectRequirementsByRoleRowDeleted != null)
				{
					this.AnalysisProjectRequirementsByRoleRowDeleted(this, new AnalysisDataSet.AnalysisProjectRequirementsByRoleRowChangeEvent((AnalysisDataSet.AnalysisProjectRequirementsByRoleRow)e.Row, e.Action));
				}
			}

			// Token: 0x060005BC RID: 1468 RVA: 0x000159C3 File Offset: 0x00013BC3
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			protected override void OnRowDeleting(DataRowChangeEventArgs e)
			{
				base.OnRowDeleting(e);
				if (this.AnalysisProjectRequirementsByRoleRowDeleting != null)
				{
					this.AnalysisProjectRequirementsByRoleRowDeleting(this, new AnalysisDataSet.AnalysisProjectRequirementsByRoleRowChangeEvent((AnalysisDataSet.AnalysisProjectRequirementsByRoleRow)e.Row, e.Action));
				}
			}

			// Token: 0x060005BD RID: 1469 RVA: 0x000159F6 File Offset: 0x00013BF6
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public void RemoveAnalysisProjectRequirementsByRoleRow(AnalysisDataSet.AnalysisProjectRequirementsByRoleRow row)
			{
				base.Rows.Remove(row);
			}

			// Token: 0x060005BE RID: 1470 RVA: 0x00015A04 File Offset: 0x00013C04
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			public static XmlSchemaComplexType GetTypedTableSchema(XmlSchemaSet xs)
			{
				XmlSchemaComplexType xmlSchemaComplexType = new XmlSchemaComplexType();
				XmlSchemaSequence xmlSchemaSequence = new XmlSchemaSequence();
				AnalysisDataSet analysisDataSet = new AnalysisDataSet();
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
				xmlSchemaAttribute.FixedValue = analysisDataSet.Namespace;
				xmlSchemaComplexType.Attributes.Add(xmlSchemaAttribute);
				XmlSchemaAttribute xmlSchemaAttribute2 = new XmlSchemaAttribute();
				xmlSchemaAttribute2.Name = "tableTypeName";
				xmlSchemaAttribute2.FixedValue = "AnalysisProjectRequirementsByRoleDataTable";
				xmlSchemaComplexType.Attributes.Add(xmlSchemaAttribute2);
				xmlSchemaComplexType.Particle = xmlSchemaSequence;
				XmlSchema schemaSerializable = analysisDataSet.GetSchemaSerializable();
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

			// Token: 0x040001AE RID: 430
			private DataColumn columnANALYSIS_UID;

			// Token: 0x040001AF RID: 431
			private DataColumn columnPROJ_UID;

			// Token: 0x040001B0 RID: 432
			private DataColumn columnCUSTOM_FIELD_UID;

			// Token: 0x040001B1 RID: 433
			private DataColumn columnLT_STRUCT_UID;

			// Token: 0x040001B2 RID: 434
			private DataColumn columnSTART_DATE;

			// Token: 0x040001B3 RID: 435
			private DataColumn columnPROJECT_REQUIREMENT;

			// Token: 0x040001B4 RID: 436
			private DataColumn columnREQUIREMENT_UID;
		}

		// Token: 0x02000056 RID: 86
		public class AnalysisRow : DataRow
		{
			// Token: 0x060005BF RID: 1471 RVA: 0x00015BFC File Offset: 0x00013DFC
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			internal AnalysisRow(DataRowBuilder rb) : base(rb)
			{
				this.tableAnalysis = (AnalysisDataSet.AnalysisDataTable)base.Table;
			}

			// Token: 0x1700019D RID: 413
			// (get) Token: 0x060005C0 RID: 1472 RVA: 0x00015C16 File Offset: 0x00013E16
			// (set) Token: 0x060005C1 RID: 1473 RVA: 0x00015C2E File Offset: 0x00013E2E
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public Guid ANALYSIS_UID
			{
				get
				{
					return (Guid)base[this.tableAnalysis.ANALYSIS_UIDColumn];
				}
				set
				{
					base[this.tableAnalysis.ANALYSIS_UIDColumn] = value;
				}
			}

			// Token: 0x1700019E RID: 414
			// (get) Token: 0x060005C2 RID: 1474 RVA: 0x00015C47 File Offset: 0x00013E47
			// (set) Token: 0x060005C3 RID: 1475 RVA: 0x00015C5F File Offset: 0x00013E5F
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public string ANALYSIS_NAME
			{
				get
				{
					return (string)base[this.tableAnalysis.ANALYSIS_NAMEColumn];
				}
				set
				{
					base[this.tableAnalysis.ANALYSIS_NAMEColumn] = value;
				}
			}

			// Token: 0x1700019F RID: 415
			// (get) Token: 0x060005C4 RID: 1476 RVA: 0x00015C74 File Offset: 0x00013E74
			// (set) Token: 0x060005C5 RID: 1477 RVA: 0x00015CB8 File Offset: 0x00013EB8
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public string ANALYSIS_DESCRIPTION
			{
				get
				{
					string result;
					try
					{
						result = (string)base[this.tableAnalysis.ANALYSIS_DESCRIPTIONColumn];
					}
					catch (InvalidCastException innerException)
					{
						throw new StrongTypingException("The value for column 'ANALYSIS_DESCRIPTION' in table 'Analysis' is DBNull.", innerException);
					}
					return result;
				}
				set
				{
					base[this.tableAnalysis.ANALYSIS_DESCRIPTIONColumn] = value;
				}
			}

			// Token: 0x170001A0 RID: 416
			// (get) Token: 0x060005C6 RID: 1478 RVA: 0x00015CCC File Offset: 0x00013ECC
			// (set) Token: 0x060005C7 RID: 1479 RVA: 0x00015CE4 File Offset: 0x00013EE4
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			public int ANALYSIS_TYPE
			{
				get
				{
					return (int)base[this.tableAnalysis.ANALYSIS_TYPEColumn];
				}
				set
				{
					base[this.tableAnalysis.ANALYSIS_TYPEColumn] = value;
				}
			}

			// Token: 0x170001A1 RID: 417
			// (get) Token: 0x060005C8 RID: 1480 RVA: 0x00015D00 File Offset: 0x00013F00
			// (set) Token: 0x060005C9 RID: 1481 RVA: 0x00015D44 File Offset: 0x00013F44
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			public Guid DEPARTMENT_UID
			{
				get
				{
					Guid result;
					try
					{
						result = (Guid)base[this.tableAnalysis.DEPARTMENT_UIDColumn];
					}
					catch (InvalidCastException innerException)
					{
						throw new StrongTypingException("The value for column 'DEPARTMENT_UID' in table 'Analysis' is DBNull.", innerException);
					}
					return result;
				}
				set
				{
					base[this.tableAnalysis.DEPARTMENT_UIDColumn] = value;
				}
			}

			// Token: 0x170001A2 RID: 418
			// (get) Token: 0x060005CA RID: 1482 RVA: 0x00015D60 File Offset: 0x00013F60
			// (set) Token: 0x060005CB RID: 1483 RVA: 0x00015DA4 File Offset: 0x00013FA4
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public string DEPARTMENT_NAME
			{
				get
				{
					string result;
					try
					{
						result = (string)base[this.tableAnalysis.DEPARTMENT_NAMEColumn];
					}
					catch (InvalidCastException innerException)
					{
						throw new StrongTypingException("The value for column 'DEPARTMENT_NAME' in table 'Analysis' is DBNull.", innerException);
					}
					return result;
				}
				set
				{
					base[this.tableAnalysis.DEPARTMENT_NAMEColumn] = value;
				}
			}

			// Token: 0x170001A3 RID: 419
			// (get) Token: 0x060005CC RID: 1484 RVA: 0x00015DB8 File Offset: 0x00013FB8
			// (set) Token: 0x060005CD RID: 1485 RVA: 0x00015DD0 File Offset: 0x00013FD0
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			public int PRIORITIZATION_TYPE
			{
				get
				{
					return (int)base[this.tableAnalysis.PRIORITIZATION_TYPEColumn];
				}
				set
				{
					base[this.tableAnalysis.PRIORITIZATION_TYPEColumn] = value;
				}
			}

			// Token: 0x170001A4 RID: 420
			// (get) Token: 0x060005CE RID: 1486 RVA: 0x00015DEC File Offset: 0x00013FEC
			// (set) Token: 0x060005CF RID: 1487 RVA: 0x00015E30 File Offset: 0x00014030
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			public Guid PRIORITIZATION_UID
			{
				get
				{
					Guid result;
					try
					{
						result = (Guid)base[this.tableAnalysis.PRIORITIZATION_UIDColumn];
					}
					catch (InvalidCastException innerException)
					{
						throw new StrongTypingException("The value for column 'PRIORITIZATION_UID' in table 'Analysis' is DBNull.", innerException);
					}
					return result;
				}
				set
				{
					base[this.tableAnalysis.PRIORITIZATION_UIDColumn] = value;
				}
			}

			// Token: 0x170001A5 RID: 421
			// (get) Token: 0x060005D0 RID: 1488 RVA: 0x00015E4C File Offset: 0x0001404C
			// (set) Token: 0x060005D1 RID: 1489 RVA: 0x00015E90 File Offset: 0x00014090
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			public Guid PROJECT_IMPACT_CF_UID
			{
				get
				{
					Guid result;
					try
					{
						result = (Guid)base[this.tableAnalysis.PROJECT_IMPACT_CF_UIDColumn];
					}
					catch (InvalidCastException innerException)
					{
						throw new StrongTypingException("The value for column 'PROJECT_IMPACT_CF_UID' in table 'Analysis' is DBNull.", innerException);
					}
					return result;
				}
				set
				{
					base[this.tableAnalysis.PROJECT_IMPACT_CF_UIDColumn] = value;
				}
			}

			// Token: 0x170001A6 RID: 422
			// (get) Token: 0x060005D2 RID: 1490 RVA: 0x00015EAC File Offset: 0x000140AC
			// (set) Token: 0x060005D3 RID: 1491 RVA: 0x00015EF0 File Offset: 0x000140F0
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			public string PROJECT_IMPACT_CF_NAME
			{
				get
				{
					string result;
					try
					{
						result = (string)base[this.tableAnalysis.PROJECT_IMPACT_CF_NAMEColumn];
					}
					catch (InvalidCastException innerException)
					{
						throw new StrongTypingException("The value for column 'PROJECT_IMPACT_CF_NAME' in table 'Analysis' is DBNull.", innerException);
					}
					return result;
				}
				set
				{
					base[this.tableAnalysis.PROJECT_IMPACT_CF_NAMEColumn] = value;
				}
			}

			// Token: 0x170001A7 RID: 423
			// (get) Token: 0x060005D4 RID: 1492 RVA: 0x00015F04 File Offset: 0x00014104
			// (set) Token: 0x060005D5 RID: 1493 RVA: 0x00015F1C File Offset: 0x0001411C
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			public Guid HARD_CONSTRAINT_CF_UID
			{
				get
				{
					return (Guid)base[this.tableAnalysis.HARD_CONSTRAINT_CF_UIDColumn];
				}
				set
				{
					base[this.tableAnalysis.HARD_CONSTRAINT_CF_UIDColumn] = value;
				}
			}

			// Token: 0x170001A8 RID: 424
			// (get) Token: 0x060005D6 RID: 1494 RVA: 0x00015F38 File Offset: 0x00014138
			// (set) Token: 0x060005D7 RID: 1495 RVA: 0x00015F7C File Offset: 0x0001417C
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public string HARD_CONSTRAINT_CF_NAME
			{
				get
				{
					string result;
					try
					{
						result = (string)base[this.tableAnalysis.HARD_CONSTRAINT_CF_NAMEColumn];
					}
					catch (InvalidCastException innerException)
					{
						throw new StrongTypingException("The value for column 'HARD_CONSTRAINT_CF_NAME' in table 'Analysis' is DBNull.", innerException);
					}
					return result;
				}
				set
				{
					base[this.tableAnalysis.HARD_CONSTRAINT_CF_NAMEColumn] = value;
				}
			}

			// Token: 0x170001A9 RID: 425
			// (get) Token: 0x060005D8 RID: 1496 RVA: 0x00015F90 File Offset: 0x00014190
			// (set) Token: 0x060005D9 RID: 1497 RVA: 0x00015FD4 File Offset: 0x000141D4
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public DateTime HORIZON_START_DATE
			{
				get
				{
					DateTime result;
					try
					{
						result = (DateTime)base[this.tableAnalysis.HORIZON_START_DATEColumn];
					}
					catch (InvalidCastException innerException)
					{
						throw new StrongTypingException("The value for column 'HORIZON_START_DATE' in table 'Analysis' is DBNull.", innerException);
					}
					return result;
				}
				set
				{
					base[this.tableAnalysis.HORIZON_START_DATEColumn] = value;
				}
			}

			// Token: 0x170001AA RID: 426
			// (get) Token: 0x060005DA RID: 1498 RVA: 0x00015FF0 File Offset: 0x000141F0
			// (set) Token: 0x060005DB RID: 1499 RVA: 0x00016034 File Offset: 0x00014234
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			public DateTime HORIZON_END_DATE
			{
				get
				{
					DateTime result;
					try
					{
						result = (DateTime)base[this.tableAnalysis.HORIZON_END_DATEColumn];
					}
					catch (InvalidCastException innerException)
					{
						throw new StrongTypingException("The value for column 'HORIZON_END_DATE' in table 'Analysis' is DBNull.", innerException);
					}
					return result;
				}
				set
				{
					base[this.tableAnalysis.HORIZON_END_DATEColumn] = value;
				}
			}

			// Token: 0x170001AB RID: 427
			// (get) Token: 0x060005DC RID: 1500 RVA: 0x00016050 File Offset: 0x00014250
			// (set) Token: 0x060005DD RID: 1501 RVA: 0x00016094 File Offset: 0x00014294
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			public Guid ROLE_CUSTOM_FIELD_UID
			{
				get
				{
					Guid result;
					try
					{
						result = (Guid)base[this.tableAnalysis.ROLE_CUSTOM_FIELD_UIDColumn];
					}
					catch (InvalidCastException innerException)
					{
						throw new StrongTypingException("The value for column 'ROLE_CUSTOM_FIELD_UID' in table 'Analysis' is DBNull.", innerException);
					}
					return result;
				}
				set
				{
					base[this.tableAnalysis.ROLE_CUSTOM_FIELD_UIDColumn] = value;
				}
			}

			// Token: 0x170001AC RID: 428
			// (get) Token: 0x060005DE RID: 1502 RVA: 0x000160AD File Offset: 0x000142AD
			// (set) Token: 0x060005DF RID: 1503 RVA: 0x000160C5 File Offset: 0x000142C5
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public byte TIME_SCALE
			{
				get
				{
					return (byte)base[this.tableAnalysis.TIME_SCALEColumn];
				}
				set
				{
					base[this.tableAnalysis.TIME_SCALEColumn] = value;
				}
			}

			// Token: 0x170001AD RID: 429
			// (get) Token: 0x060005E0 RID: 1504 RVA: 0x000160E0 File Offset: 0x000142E0
			// (set) Token: 0x060005E1 RID: 1505 RVA: 0x00016124 File Offset: 0x00014324
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			public bool FILTER_RESOURCES_BY_DEP
			{
				get
				{
					bool result;
					try
					{
						result = (bool)base[this.tableAnalysis.FILTER_RESOURCES_BY_DEPColumn];
					}
					catch (InvalidCastException innerException)
					{
						throw new StrongTypingException("The value for column 'FILTER_RESOURCES_BY_DEP' in table 'Analysis' is DBNull.", innerException);
					}
					return result;
				}
				set
				{
					base[this.tableAnalysis.FILTER_RESOURCES_BY_DEPColumn] = value;
				}
			}

			// Token: 0x170001AE RID: 430
			// (get) Token: 0x060005E2 RID: 1506 RVA: 0x00016140 File Offset: 0x00014340
			// (set) Token: 0x060005E3 RID: 1507 RVA: 0x00016184 File Offset: 0x00014384
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			public bool FILTER_RESOURCES_BY_RBS
			{
				get
				{
					bool result;
					try
					{
						result = (bool)base[this.tableAnalysis.FILTER_RESOURCES_BY_RBSColumn];
					}
					catch (InvalidCastException innerException)
					{
						throw new StrongTypingException("The value for column 'FILTER_RESOURCES_BY_RBS' in table 'Analysis' is DBNull.", innerException);
					}
					return result;
				}
				set
				{
					base[this.tableAnalysis.FILTER_RESOURCES_BY_RBSColumn] = value;
				}
			}

			// Token: 0x170001AF RID: 431
			// (get) Token: 0x060005E4 RID: 1508 RVA: 0x000161A0 File Offset: 0x000143A0
			// (set) Token: 0x060005E5 RID: 1509 RVA: 0x000161E4 File Offset: 0x000143E4
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public Guid FILTER_RESOURCES_RBS_VAL
			{
				get
				{
					Guid result;
					try
					{
						result = (Guid)base[this.tableAnalysis.FILTER_RESOURCES_RBS_VALColumn];
					}
					catch (InvalidCastException innerException)
					{
						throw new StrongTypingException("The value for column 'FILTER_RESOURCES_RBS_VAL' in table 'Analysis' is DBNull.", innerException);
					}
					return result;
				}
				set
				{
					base[this.tableAnalysis.FILTER_RESOURCES_RBS_VALColumn] = value;
				}
			}

			// Token: 0x170001B0 RID: 432
			// (get) Token: 0x060005E6 RID: 1510 RVA: 0x00016200 File Offset: 0x00014400
			// (set) Token: 0x060005E7 RID: 1511 RVA: 0x00016244 File Offset: 0x00014444
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public string FILTER_RESOURCES_RBS_NAME
			{
				get
				{
					string result;
					try
					{
						result = (string)base[this.tableAnalysis.FILTER_RESOURCES_RBS_NAMEColumn];
					}
					catch (InvalidCastException innerException)
					{
						throw new StrongTypingException("The value for column 'FILTER_RESOURCES_RBS_NAME' in table 'Analysis' is DBNull.", innerException);
					}
					return result;
				}
				set
				{
					base[this.tableAnalysis.FILTER_RESOURCES_RBS_NAMEColumn] = value;
				}
			}

			// Token: 0x170001B1 RID: 433
			// (get) Token: 0x060005E8 RID: 1512 RVA: 0x00016258 File Offset: 0x00014458
			// (set) Token: 0x060005E9 RID: 1513 RVA: 0x0001629C File Offset: 0x0001449C
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public bool USE_ALT_PROJ_DATES_FOR_RES_PLAN
			{
				get
				{
					bool result;
					try
					{
						result = (bool)base[this.tableAnalysis.USE_ALT_PROJ_DATES_FOR_RES_PLANColumn];
					}
					catch (InvalidCastException innerException)
					{
						throw new StrongTypingException("The value for column 'USE_ALT_PROJ_DATES_FOR_RES_PLAN' in table 'Analysis' is DBNull.", innerException);
					}
					return result;
				}
				set
				{
					base[this.tableAnalysis.USE_ALT_PROJ_DATES_FOR_RES_PLANColumn] = value;
				}
			}

			// Token: 0x170001B2 RID: 434
			// (get) Token: 0x060005EA RID: 1514 RVA: 0x000162B8 File Offset: 0x000144B8
			// (set) Token: 0x060005EB RID: 1515 RVA: 0x000162FC File Offset: 0x000144FC
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public Guid ALT_PROJ_START_DATE_CF_UID
			{
				get
				{
					Guid result;
					try
					{
						result = (Guid)base[this.tableAnalysis.ALT_PROJ_START_DATE_CF_UIDColumn];
					}
					catch (InvalidCastException innerException)
					{
						throw new StrongTypingException("The value for column 'ALT_PROJ_START_DATE_CF_UID' in table 'Analysis' is DBNull.", innerException);
					}
					return result;
				}
				set
				{
					base[this.tableAnalysis.ALT_PROJ_START_DATE_CF_UIDColumn] = value;
				}
			}

			// Token: 0x170001B3 RID: 435
			// (get) Token: 0x060005EC RID: 1516 RVA: 0x00016318 File Offset: 0x00014518
			// (set) Token: 0x060005ED RID: 1517 RVA: 0x0001635C File Offset: 0x0001455C
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			public Guid ALT_PROJ_END_DATE_CF_UID
			{
				get
				{
					Guid result;
					try
					{
						result = (Guid)base[this.tableAnalysis.ALT_PROJ_END_DATE_CF_UIDColumn];
					}
					catch (InvalidCastException innerException)
					{
						throw new StrongTypingException("The value for column 'ALT_PROJ_END_DATE_CF_UID' in table 'Analysis' is DBNull.", innerException);
					}
					return result;
				}
				set
				{
					base[this.tableAnalysis.ALT_PROJ_END_DATE_CF_UIDColumn] = value;
				}
			}

			// Token: 0x170001B4 RID: 436
			// (get) Token: 0x060005EE RID: 1518 RVA: 0x00016375 File Offset: 0x00014575
			// (set) Token: 0x060005EF RID: 1519 RVA: 0x0001638D File Offset: 0x0001458D
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public int BOOKING_TYPE
			{
				get
				{
					return (int)base[this.tableAnalysis.BOOKING_TYPEColumn];
				}
				set
				{
					base[this.tableAnalysis.BOOKING_TYPEColumn] = value;
				}
			}

			// Token: 0x170001B5 RID: 437
			// (get) Token: 0x060005F0 RID: 1520 RVA: 0x000163A8 File Offset: 0x000145A8
			// (set) Token: 0x060005F1 RID: 1521 RVA: 0x000163EC File Offset: 0x000145EC
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public DateTime CREATED_DATE
			{
				get
				{
					DateTime result;
					try
					{
						result = (DateTime)base[this.tableAnalysis.CREATED_DATEColumn];
					}
					catch (InvalidCastException innerException)
					{
						throw new StrongTypingException("The value for column 'CREATED_DATE' in table 'Analysis' is DBNull.", innerException);
					}
					return result;
				}
				set
				{
					base[this.tableAnalysis.CREATED_DATEColumn] = value;
				}
			}

			// Token: 0x170001B6 RID: 438
			// (get) Token: 0x060005F2 RID: 1522 RVA: 0x00016408 File Offset: 0x00014608
			// (set) Token: 0x060005F3 RID: 1523 RVA: 0x0001644C File Offset: 0x0001464C
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			public DateTime MOD_DATE
			{
				get
				{
					DateTime result;
					try
					{
						result = (DateTime)base[this.tableAnalysis.MOD_DATEColumn];
					}
					catch (InvalidCastException innerException)
					{
						throw new StrongTypingException("The value for column 'MOD_DATE' in table 'Analysis' is DBNull.", innerException);
					}
					return result;
				}
				set
				{
					base[this.tableAnalysis.MOD_DATEColumn] = value;
				}
			}

			// Token: 0x170001B7 RID: 439
			// (get) Token: 0x060005F4 RID: 1524 RVA: 0x00016468 File Offset: 0x00014668
			// (set) Token: 0x060005F5 RID: 1525 RVA: 0x000164AC File Offset: 0x000146AC
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			public Guid LAST_UPDATED_BY_RES_UID
			{
				get
				{
					Guid result;
					try
					{
						result = (Guid)base[this.tableAnalysis.LAST_UPDATED_BY_RES_UIDColumn];
					}
					catch (InvalidCastException innerException)
					{
						throw new StrongTypingException("The value for column 'LAST_UPDATED_BY_RES_UID' in table 'Analysis' is DBNull.", innerException);
					}
					return result;
				}
				set
				{
					base[this.tableAnalysis.LAST_UPDATED_BY_RES_UIDColumn] = value;
				}
			}

			// Token: 0x170001B8 RID: 440
			// (get) Token: 0x060005F6 RID: 1526 RVA: 0x000164C8 File Offset: 0x000146C8
			// (set) Token: 0x060005F7 RID: 1527 RVA: 0x0001650C File Offset: 0x0001470C
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public string LAST_UPDATED_BY_RES_NAME
			{
				get
				{
					string result;
					try
					{
						result = (string)base[this.tableAnalysis.LAST_UPDATED_BY_RES_NAMEColumn];
					}
					catch (InvalidCastException innerException)
					{
						throw new StrongTypingException("The value for column 'LAST_UPDATED_BY_RES_NAME' in table 'Analysis' is DBNull.", innerException);
					}
					return result;
				}
				set
				{
					base[this.tableAnalysis.LAST_UPDATED_BY_RES_NAMEColumn] = value;
				}
			}

			// Token: 0x170001B9 RID: 441
			// (get) Token: 0x060005F8 RID: 1528 RVA: 0x00016520 File Offset: 0x00014720
			// (set) Token: 0x060005F9 RID: 1529 RVA: 0x00016564 File Offset: 0x00014764
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public Guid CREATED_BY_RES_UID
			{
				get
				{
					Guid result;
					try
					{
						result = (Guid)base[this.tableAnalysis.CREATED_BY_RES_UIDColumn];
					}
					catch (InvalidCastException innerException)
					{
						throw new StrongTypingException("The value for column 'CREATED_BY_RES_UID' in table 'Analysis' is DBNull.", innerException);
					}
					return result;
				}
				set
				{
					base[this.tableAnalysis.CREATED_BY_RES_UIDColumn] = value;
				}
			}

			// Token: 0x170001BA RID: 442
			// (get) Token: 0x060005FA RID: 1530 RVA: 0x00016580 File Offset: 0x00014780
			// (set) Token: 0x060005FB RID: 1531 RVA: 0x000165C4 File Offset: 0x000147C4
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public string CREATED_BY_RES_NAME
			{
				get
				{
					string result;
					try
					{
						result = (string)base[this.tableAnalysis.CREATED_BY_RES_NAMEColumn];
					}
					catch (InvalidCastException innerException)
					{
						throw new StrongTypingException("The value for column 'CREATED_BY_RES_NAME' in table 'Analysis' is DBNull.", innerException);
					}
					return result;
				}
				set
				{
					base[this.tableAnalysis.CREATED_BY_RES_NAMEColumn] = value;
				}
			}

			// Token: 0x170001BB RID: 443
			// (get) Token: 0x060005FC RID: 1532 RVA: 0x000165D8 File Offset: 0x000147D8
			// (set) Token: 0x060005FD RID: 1533 RVA: 0x0001661C File Offset: 0x0001481C
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			public Guid FORCE_IN_ALIAS_LT_UID
			{
				get
				{
					Guid result;
					try
					{
						result = (Guid)base[this.tableAnalysis.FORCE_IN_ALIAS_LT_UIDColumn];
					}
					catch (InvalidCastException innerException)
					{
						throw new StrongTypingException("The value for column 'FORCE_IN_ALIAS_LT_UID' in table 'Analysis' is DBNull.", innerException);
					}
					return result;
				}
				set
				{
					base[this.tableAnalysis.FORCE_IN_ALIAS_LT_UIDColumn] = value;
				}
			}

			// Token: 0x170001BC RID: 444
			// (get) Token: 0x060005FE RID: 1534 RVA: 0x00016638 File Offset: 0x00014838
			// (set) Token: 0x060005FF RID: 1535 RVA: 0x0001667C File Offset: 0x0001487C
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public Guid FORCE_OUT_ALIAS_LT_UID
			{
				get
				{
					Guid result;
					try
					{
						result = (Guid)base[this.tableAnalysis.FORCE_OUT_ALIAS_LT_UIDColumn];
					}
					catch (InvalidCastException innerException)
					{
						throw new StrongTypingException("The value for column 'FORCE_OUT_ALIAS_LT_UID' in table 'Analysis' is DBNull.", innerException);
					}
					return result;
				}
				set
				{
					base[this.tableAnalysis.FORCE_OUT_ALIAS_LT_UIDColumn] = value;
				}
			}

			// Token: 0x06000600 RID: 1536 RVA: 0x00016695 File Offset: 0x00014895
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			public bool IsANALYSIS_DESCRIPTIONNull()
			{
				return base.IsNull(this.tableAnalysis.ANALYSIS_DESCRIPTIONColumn);
			}

			// Token: 0x06000601 RID: 1537 RVA: 0x000166A8 File Offset: 0x000148A8
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public void SetANALYSIS_DESCRIPTIONNull()
			{
				base[this.tableAnalysis.ANALYSIS_DESCRIPTIONColumn] = Convert.DBNull;
			}

			// Token: 0x06000602 RID: 1538 RVA: 0x000166C0 File Offset: 0x000148C0
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public bool IsDEPARTMENT_UIDNull()
			{
				return base.IsNull(this.tableAnalysis.DEPARTMENT_UIDColumn);
			}

			// Token: 0x06000603 RID: 1539 RVA: 0x000166D3 File Offset: 0x000148D3
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public void SetDEPARTMENT_UIDNull()
			{
				base[this.tableAnalysis.DEPARTMENT_UIDColumn] = Convert.DBNull;
			}

			// Token: 0x06000604 RID: 1540 RVA: 0x000166EB File Offset: 0x000148EB
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			public bool IsDEPARTMENT_NAMENull()
			{
				return base.IsNull(this.tableAnalysis.DEPARTMENT_NAMEColumn);
			}

			// Token: 0x06000605 RID: 1541 RVA: 0x000166FE File Offset: 0x000148FE
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public void SetDEPARTMENT_NAMENull()
			{
				base[this.tableAnalysis.DEPARTMENT_NAMEColumn] = Convert.DBNull;
			}

			// Token: 0x06000606 RID: 1542 RVA: 0x00016716 File Offset: 0x00014916
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			public bool IsPRIORITIZATION_UIDNull()
			{
				return base.IsNull(this.tableAnalysis.PRIORITIZATION_UIDColumn);
			}

			// Token: 0x06000607 RID: 1543 RVA: 0x00016729 File Offset: 0x00014929
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			public void SetPRIORITIZATION_UIDNull()
			{
				base[this.tableAnalysis.PRIORITIZATION_UIDColumn] = Convert.DBNull;
			}

			// Token: 0x06000608 RID: 1544 RVA: 0x00016741 File Offset: 0x00014941
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			public bool IsPROJECT_IMPACT_CF_UIDNull()
			{
				return base.IsNull(this.tableAnalysis.PROJECT_IMPACT_CF_UIDColumn);
			}

			// Token: 0x06000609 RID: 1545 RVA: 0x00016754 File Offset: 0x00014954
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public void SetPROJECT_IMPACT_CF_UIDNull()
			{
				base[this.tableAnalysis.PROJECT_IMPACT_CF_UIDColumn] = Convert.DBNull;
			}

			// Token: 0x0600060A RID: 1546 RVA: 0x0001676C File Offset: 0x0001496C
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			public bool IsPROJECT_IMPACT_CF_NAMENull()
			{
				return base.IsNull(this.tableAnalysis.PROJECT_IMPACT_CF_NAMEColumn);
			}

			// Token: 0x0600060B RID: 1547 RVA: 0x0001677F File Offset: 0x0001497F
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public void SetPROJECT_IMPACT_CF_NAMENull()
			{
				base[this.tableAnalysis.PROJECT_IMPACT_CF_NAMEColumn] = Convert.DBNull;
			}

			// Token: 0x0600060C RID: 1548 RVA: 0x00016797 File Offset: 0x00014997
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			public bool IsHARD_CONSTRAINT_CF_NAMENull()
			{
				return base.IsNull(this.tableAnalysis.HARD_CONSTRAINT_CF_NAMEColumn);
			}

			// Token: 0x0600060D RID: 1549 RVA: 0x000167AA File Offset: 0x000149AA
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public void SetHARD_CONSTRAINT_CF_NAMENull()
			{
				base[this.tableAnalysis.HARD_CONSTRAINT_CF_NAMEColumn] = Convert.DBNull;
			}

			// Token: 0x0600060E RID: 1550 RVA: 0x000167C2 File Offset: 0x000149C2
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public bool IsHORIZON_START_DATENull()
			{
				return base.IsNull(this.tableAnalysis.HORIZON_START_DATEColumn);
			}

			// Token: 0x0600060F RID: 1551 RVA: 0x000167D5 File Offset: 0x000149D5
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public void SetHORIZON_START_DATENull()
			{
				base[this.tableAnalysis.HORIZON_START_DATEColumn] = Convert.DBNull;
			}

			// Token: 0x06000610 RID: 1552 RVA: 0x000167ED File Offset: 0x000149ED
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public bool IsHORIZON_END_DATENull()
			{
				return base.IsNull(this.tableAnalysis.HORIZON_END_DATEColumn);
			}

			// Token: 0x06000611 RID: 1553 RVA: 0x00016800 File Offset: 0x00014A00
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public void SetHORIZON_END_DATENull()
			{
				base[this.tableAnalysis.HORIZON_END_DATEColumn] = Convert.DBNull;
			}

			// Token: 0x06000612 RID: 1554 RVA: 0x00016818 File Offset: 0x00014A18
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public bool IsROLE_CUSTOM_FIELD_UIDNull()
			{
				return base.IsNull(this.tableAnalysis.ROLE_CUSTOM_FIELD_UIDColumn);
			}

			// Token: 0x06000613 RID: 1555 RVA: 0x0001682B File Offset: 0x00014A2B
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			public void SetROLE_CUSTOM_FIELD_UIDNull()
			{
				base[this.tableAnalysis.ROLE_CUSTOM_FIELD_UIDColumn] = Convert.DBNull;
			}

			// Token: 0x06000614 RID: 1556 RVA: 0x00016843 File Offset: 0x00014A43
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			public bool IsFILTER_RESOURCES_BY_DEPNull()
			{
				return base.IsNull(this.tableAnalysis.FILTER_RESOURCES_BY_DEPColumn);
			}

			// Token: 0x06000615 RID: 1557 RVA: 0x00016856 File Offset: 0x00014A56
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public void SetFILTER_RESOURCES_BY_DEPNull()
			{
				base[this.tableAnalysis.FILTER_RESOURCES_BY_DEPColumn] = Convert.DBNull;
			}

			// Token: 0x06000616 RID: 1558 RVA: 0x0001686E File Offset: 0x00014A6E
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public bool IsFILTER_RESOURCES_BY_RBSNull()
			{
				return base.IsNull(this.tableAnalysis.FILTER_RESOURCES_BY_RBSColumn);
			}

			// Token: 0x06000617 RID: 1559 RVA: 0x00016881 File Offset: 0x00014A81
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			public void SetFILTER_RESOURCES_BY_RBSNull()
			{
				base[this.tableAnalysis.FILTER_RESOURCES_BY_RBSColumn] = Convert.DBNull;
			}

			// Token: 0x06000618 RID: 1560 RVA: 0x00016899 File Offset: 0x00014A99
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			public bool IsFILTER_RESOURCES_RBS_VALNull()
			{
				return base.IsNull(this.tableAnalysis.FILTER_RESOURCES_RBS_VALColumn);
			}

			// Token: 0x06000619 RID: 1561 RVA: 0x000168AC File Offset: 0x00014AAC
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			public void SetFILTER_RESOURCES_RBS_VALNull()
			{
				base[this.tableAnalysis.FILTER_RESOURCES_RBS_VALColumn] = Convert.DBNull;
			}

			// Token: 0x0600061A RID: 1562 RVA: 0x000168C4 File Offset: 0x00014AC4
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			public bool IsFILTER_RESOURCES_RBS_NAMENull()
			{
				return base.IsNull(this.tableAnalysis.FILTER_RESOURCES_RBS_NAMEColumn);
			}

			// Token: 0x0600061B RID: 1563 RVA: 0x000168D7 File Offset: 0x00014AD7
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public void SetFILTER_RESOURCES_RBS_NAMENull()
			{
				base[this.tableAnalysis.FILTER_RESOURCES_RBS_NAMEColumn] = Convert.DBNull;
			}

			// Token: 0x0600061C RID: 1564 RVA: 0x000168EF File Offset: 0x00014AEF
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			public bool IsUSE_ALT_PROJ_DATES_FOR_RES_PLANNull()
			{
				return base.IsNull(this.tableAnalysis.USE_ALT_PROJ_DATES_FOR_RES_PLANColumn);
			}

			// Token: 0x0600061D RID: 1565 RVA: 0x00016902 File Offset: 0x00014B02
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public void SetUSE_ALT_PROJ_DATES_FOR_RES_PLANNull()
			{
				base[this.tableAnalysis.USE_ALT_PROJ_DATES_FOR_RES_PLANColumn] = Convert.DBNull;
			}

			// Token: 0x0600061E RID: 1566 RVA: 0x0001691A File Offset: 0x00014B1A
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			public bool IsALT_PROJ_START_DATE_CF_UIDNull()
			{
				return base.IsNull(this.tableAnalysis.ALT_PROJ_START_DATE_CF_UIDColumn);
			}

			// Token: 0x0600061F RID: 1567 RVA: 0x0001692D File Offset: 0x00014B2D
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public void SetALT_PROJ_START_DATE_CF_UIDNull()
			{
				base[this.tableAnalysis.ALT_PROJ_START_DATE_CF_UIDColumn] = Convert.DBNull;
			}

			// Token: 0x06000620 RID: 1568 RVA: 0x00016945 File Offset: 0x00014B45
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public bool IsALT_PROJ_END_DATE_CF_UIDNull()
			{
				return base.IsNull(this.tableAnalysis.ALT_PROJ_END_DATE_CF_UIDColumn);
			}

			// Token: 0x06000621 RID: 1569 RVA: 0x00016958 File Offset: 0x00014B58
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public void SetALT_PROJ_END_DATE_CF_UIDNull()
			{
				base[this.tableAnalysis.ALT_PROJ_END_DATE_CF_UIDColumn] = Convert.DBNull;
			}

			// Token: 0x06000622 RID: 1570 RVA: 0x00016970 File Offset: 0x00014B70
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			public bool IsCREATED_DATENull()
			{
				return base.IsNull(this.tableAnalysis.CREATED_DATEColumn);
			}

			// Token: 0x06000623 RID: 1571 RVA: 0x00016983 File Offset: 0x00014B83
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public void SetCREATED_DATENull()
			{
				base[this.tableAnalysis.CREATED_DATEColumn] = Convert.DBNull;
			}

			// Token: 0x06000624 RID: 1572 RVA: 0x0001699B File Offset: 0x00014B9B
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public bool IsMOD_DATENull()
			{
				return base.IsNull(this.tableAnalysis.MOD_DATEColumn);
			}

			// Token: 0x06000625 RID: 1573 RVA: 0x000169AE File Offset: 0x00014BAE
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			public void SetMOD_DATENull()
			{
				base[this.tableAnalysis.MOD_DATEColumn] = Convert.DBNull;
			}

			// Token: 0x06000626 RID: 1574 RVA: 0x000169C6 File Offset: 0x00014BC6
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public bool IsLAST_UPDATED_BY_RES_UIDNull()
			{
				return base.IsNull(this.tableAnalysis.LAST_UPDATED_BY_RES_UIDColumn);
			}

			// Token: 0x06000627 RID: 1575 RVA: 0x000169D9 File Offset: 0x00014BD9
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			public void SetLAST_UPDATED_BY_RES_UIDNull()
			{
				base[this.tableAnalysis.LAST_UPDATED_BY_RES_UIDColumn] = Convert.DBNull;
			}

			// Token: 0x06000628 RID: 1576 RVA: 0x000169F1 File Offset: 0x00014BF1
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			public bool IsLAST_UPDATED_BY_RES_NAMENull()
			{
				return base.IsNull(this.tableAnalysis.LAST_UPDATED_BY_RES_NAMEColumn);
			}

			// Token: 0x06000629 RID: 1577 RVA: 0x00016A04 File Offset: 0x00014C04
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public void SetLAST_UPDATED_BY_RES_NAMENull()
			{
				base[this.tableAnalysis.LAST_UPDATED_BY_RES_NAMEColumn] = Convert.DBNull;
			}

			// Token: 0x0600062A RID: 1578 RVA: 0x00016A1C File Offset: 0x00014C1C
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			public bool IsCREATED_BY_RES_UIDNull()
			{
				return base.IsNull(this.tableAnalysis.CREATED_BY_RES_UIDColumn);
			}

			// Token: 0x0600062B RID: 1579 RVA: 0x00016A2F File Offset: 0x00014C2F
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public void SetCREATED_BY_RES_UIDNull()
			{
				base[this.tableAnalysis.CREATED_BY_RES_UIDColumn] = Convert.DBNull;
			}

			// Token: 0x0600062C RID: 1580 RVA: 0x00016A47 File Offset: 0x00014C47
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public bool IsCREATED_BY_RES_NAMENull()
			{
				return base.IsNull(this.tableAnalysis.CREATED_BY_RES_NAMEColumn);
			}

			// Token: 0x0600062D RID: 1581 RVA: 0x00016A5A File Offset: 0x00014C5A
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			public void SetCREATED_BY_RES_NAMENull()
			{
				base[this.tableAnalysis.CREATED_BY_RES_NAMEColumn] = Convert.DBNull;
			}

			// Token: 0x0600062E RID: 1582 RVA: 0x00016A72 File Offset: 0x00014C72
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public bool IsFORCE_IN_ALIAS_LT_UIDNull()
			{
				return base.IsNull(this.tableAnalysis.FORCE_IN_ALIAS_LT_UIDColumn);
			}

			// Token: 0x0600062F RID: 1583 RVA: 0x00016A85 File Offset: 0x00014C85
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			public void SetFORCE_IN_ALIAS_LT_UIDNull()
			{
				base[this.tableAnalysis.FORCE_IN_ALIAS_LT_UIDColumn] = Convert.DBNull;
			}

			// Token: 0x06000630 RID: 1584 RVA: 0x00016A9D File Offset: 0x00014C9D
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			public bool IsFORCE_OUT_ALIAS_LT_UIDNull()
			{
				return base.IsNull(this.tableAnalysis.FORCE_OUT_ALIAS_LT_UIDColumn);
			}

			// Token: 0x06000631 RID: 1585 RVA: 0x00016AB0 File Offset: 0x00014CB0
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public void SetFORCE_OUT_ALIAS_LT_UIDNull()
			{
				base[this.tableAnalysis.FORCE_OUT_ALIAS_LT_UIDColumn] = Convert.DBNull;
			}

			// Token: 0x06000632 RID: 1586 RVA: 0x00016AC8 File Offset: 0x00014CC8
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public AnalysisDataSet.AnalysisProjectsRow[] GetAnalysisProjectsRows()
			{
				if (base.Table.ChildRelations["FK_Analysis_AnalysisProjects"] == null)
				{
					return new AnalysisDataSet.AnalysisProjectsRow[0];
				}
				return (AnalysisDataSet.AnalysisProjectsRow[])base.GetChildRows(base.Table.ChildRelations["FK_Analysis_AnalysisProjects"]);
			}

			// Token: 0x06000633 RID: 1587 RVA: 0x00016B08 File Offset: 0x00014D08
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public AnalysisDataSet.AnalysisPriorityDataRow[] GetAnalysisPriorityDataRows()
			{
				if (base.Table.ChildRelations["FK_Analysis_AnalysisPriorityData"] == null)
				{
					return new AnalysisDataSet.AnalysisPriorityDataRow[0];
				}
				return (AnalysisDataSet.AnalysisPriorityDataRow[])base.GetChildRows(base.Table.ChildRelations["FK_Analysis_AnalysisPriorityData"]);
			}

			// Token: 0x06000634 RID: 1588 RVA: 0x00016B48 File Offset: 0x00014D48
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			public AnalysisDataSet.AnalysisOptimizerSolutionsRow[] GetAnalysisOptimizerSolutionsRows()
			{
				if (base.Table.ChildRelations["FK_Analysis_AnalysisOptimizerSolutions"] == null)
				{
					return new AnalysisDataSet.AnalysisOptimizerSolutionsRow[0];
				}
				return (AnalysisDataSet.AnalysisOptimizerSolutionsRow[])base.GetChildRows(base.Table.ChildRelations["FK_Analysis_AnalysisOptimizerSolutions"]);
			}

			// Token: 0x06000635 RID: 1589 RVA: 0x00016B88 File Offset: 0x00014D88
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public AnalysisDataSet.AnalysisPlannerSolutionsRow[] GetAnalysisPlannerSolutionsRows()
			{
				if (base.Table.ChildRelations["FK_Analysis_AnalysisPlannerSolutions"] == null)
				{
					return new AnalysisDataSet.AnalysisPlannerSolutionsRow[0];
				}
				return (AnalysisDataSet.AnalysisPlannerSolutionsRow[])base.GetChildRows(base.Table.ChildRelations["FK_Analysis_AnalysisPlannerSolutions"]);
			}

			// Token: 0x040001B9 RID: 441
			private AnalysisDataSet.AnalysisDataTable tableAnalysis;
		}

		// Token: 0x02000057 RID: 87
		public class AnalysisProjectsRow : DataRow
		{
			// Token: 0x06000636 RID: 1590 RVA: 0x00016BC8 File Offset: 0x00014DC8
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			internal AnalysisProjectsRow(DataRowBuilder rb) : base(rb)
			{
				this.tableAnalysisProjects = (AnalysisDataSet.AnalysisProjectsDataTable)base.Table;
			}

			// Token: 0x170001BD RID: 445
			// (get) Token: 0x06000637 RID: 1591 RVA: 0x00016BE2 File Offset: 0x00014DE2
			// (set) Token: 0x06000638 RID: 1592 RVA: 0x00016BFA File Offset: 0x00014DFA
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public Guid ANALYSIS_UID
			{
				get
				{
					return (Guid)base[this.tableAnalysisProjects.ANALYSIS_UIDColumn];
				}
				set
				{
					base[this.tableAnalysisProjects.ANALYSIS_UIDColumn] = value;
				}
			}

			// Token: 0x170001BE RID: 446
			// (get) Token: 0x06000639 RID: 1593 RVA: 0x00016C13 File Offset: 0x00014E13
			// (set) Token: 0x0600063A RID: 1594 RVA: 0x00016C2B File Offset: 0x00014E2B
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public Guid PROJ_UID
			{
				get
				{
					return (Guid)base[this.tableAnalysisProjects.PROJ_UIDColumn];
				}
				set
				{
					base[this.tableAnalysisProjects.PROJ_UIDColumn] = value;
				}
			}

			// Token: 0x170001BF RID: 447
			// (get) Token: 0x0600063B RID: 1595 RVA: 0x00016C44 File Offset: 0x00014E44
			// (set) Token: 0x0600063C RID: 1596 RVA: 0x00016C88 File Offset: 0x00014E88
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public string PROJ_NAME
			{
				get
				{
					string result;
					try
					{
						result = (string)base[this.tableAnalysisProjects.PROJ_NAMEColumn];
					}
					catch (InvalidCastException innerException)
					{
						throw new StrongTypingException("The value for column 'PROJ_NAME' in table 'AnalysisProjects' is DBNull.", innerException);
					}
					return result;
				}
				set
				{
					base[this.tableAnalysisProjects.PROJ_NAMEColumn] = value;
				}
			}

			// Token: 0x170001C0 RID: 448
			// (get) Token: 0x0600063D RID: 1597 RVA: 0x00016C9C File Offset: 0x00014E9C
			// (set) Token: 0x0600063E RID: 1598 RVA: 0x00016CE0 File Offset: 0x00014EE0
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			public double PRIORITY
			{
				get
				{
					double result;
					try
					{
						result = (double)base[this.tableAnalysisProjects.PRIORITYColumn];
					}
					catch (InvalidCastException innerException)
					{
						throw new StrongTypingException("The value for column 'PRIORITY' in table 'AnalysisProjects' is DBNull.", innerException);
					}
					return result;
				}
				set
				{
					base[this.tableAnalysisProjects.PRIORITYColumn] = value;
				}
			}

			// Token: 0x170001C1 RID: 449
			// (get) Token: 0x0600063F RID: 1599 RVA: 0x00016CFC File Offset: 0x00014EFC
			// (set) Token: 0x06000640 RID: 1600 RVA: 0x00016D40 File Offset: 0x00014F40
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public double ABSOLUTE_PRIORITY
			{
				get
				{
					double result;
					try
					{
						result = (double)base[this.tableAnalysisProjects.ABSOLUTE_PRIORITYColumn];
					}
					catch (InvalidCastException innerException)
					{
						throw new StrongTypingException("The value for column 'ABSOLUTE_PRIORITY' in table 'AnalysisProjects' is DBNull.", innerException);
					}
					return result;
				}
				set
				{
					base[this.tableAnalysisProjects.ABSOLUTE_PRIORITYColumn] = value;
				}
			}

			// Token: 0x170001C2 RID: 450
			// (get) Token: 0x06000641 RID: 1601 RVA: 0x00016D5C File Offset: 0x00014F5C
			// (set) Token: 0x06000642 RID: 1602 RVA: 0x00016DA0 File Offset: 0x00014FA0
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			public DateTime ORIGINAL_START_DATE
			{
				get
				{
					DateTime result;
					try
					{
						result = (DateTime)base[this.tableAnalysisProjects.ORIGINAL_START_DATEColumn];
					}
					catch (InvalidCastException innerException)
					{
						throw new StrongTypingException("The value for column 'ORIGINAL_START_DATE' in table 'AnalysisProjects' is DBNull.", innerException);
					}
					return result;
				}
				set
				{
					base[this.tableAnalysisProjects.ORIGINAL_START_DATEColumn] = value;
				}
			}

			// Token: 0x170001C3 RID: 451
			// (get) Token: 0x06000643 RID: 1603 RVA: 0x00016DBC File Offset: 0x00014FBC
			// (set) Token: 0x06000644 RID: 1604 RVA: 0x00016E00 File Offset: 0x00015000
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			public DateTime ORIGINAL_END_DATE
			{
				get
				{
					DateTime result;
					try
					{
						result = (DateTime)base[this.tableAnalysisProjects.ORIGINAL_END_DATEColumn];
					}
					catch (InvalidCastException innerException)
					{
						throw new StrongTypingException("The value for column 'ORIGINAL_END_DATE' in table 'AnalysisProjects' is DBNull.", innerException);
					}
					return result;
				}
				set
				{
					base[this.tableAnalysisProjects.ORIGINAL_END_DATEColumn] = value;
				}
			}

			// Token: 0x170001C4 RID: 452
			// (get) Token: 0x06000645 RID: 1605 RVA: 0x00016E1C File Offset: 0x0001501C
			// (set) Token: 0x06000646 RID: 1606 RVA: 0x00016E60 File Offset: 0x00015060
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			public DateTime START_DATE
			{
				get
				{
					DateTime result;
					try
					{
						result = (DateTime)base[this.tableAnalysisProjects.START_DATEColumn];
					}
					catch (InvalidCastException innerException)
					{
						throw new StrongTypingException("The value for column 'START_DATE' in table 'AnalysisProjects' is DBNull.", innerException);
					}
					return result;
				}
				set
				{
					base[this.tableAnalysisProjects.START_DATEColumn] = value;
				}
			}

			// Token: 0x170001C5 RID: 453
			// (get) Token: 0x06000647 RID: 1607 RVA: 0x00016E79 File Offset: 0x00015079
			// (set) Token: 0x06000648 RID: 1608 RVA: 0x00016E91 File Offset: 0x00015091
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public int DURATION
			{
				get
				{
					return (int)base[this.tableAnalysisProjects.DURATIONColumn];
				}
				set
				{
					base[this.tableAnalysisProjects.DURATIONColumn] = value;
				}
			}

			// Token: 0x170001C6 RID: 454
			// (get) Token: 0x06000649 RID: 1609 RVA: 0x00016EAC File Offset: 0x000150AC
			// (set) Token: 0x0600064A RID: 1610 RVA: 0x00016EF0 File Offset: 0x000150F0
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public DateTime SNET
			{
				get
				{
					DateTime result;
					try
					{
						result = (DateTime)base[this.tableAnalysisProjects.SNETColumn];
					}
					catch (InvalidCastException innerException)
					{
						throw new StrongTypingException("The value for column 'SNET' in table 'AnalysisProjects' is DBNull.", innerException);
					}
					return result;
				}
				set
				{
					base[this.tableAnalysisProjects.SNETColumn] = value;
				}
			}

			// Token: 0x170001C7 RID: 455
			// (get) Token: 0x0600064B RID: 1611 RVA: 0x00016F0C File Offset: 0x0001510C
			// (set) Token: 0x0600064C RID: 1612 RVA: 0x00016F50 File Offset: 0x00015150
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			public DateTime FNLT
			{
				get
				{
					DateTime result;
					try
					{
						result = (DateTime)base[this.tableAnalysisProjects.FNLTColumn];
					}
					catch (InvalidCastException innerException)
					{
						throw new StrongTypingException("The value for column 'FNLT' in table 'AnalysisProjects' is DBNull.", innerException);
					}
					return result;
				}
				set
				{
					base[this.tableAnalysisProjects.FNLTColumn] = value;
				}
			}

			// Token: 0x170001C8 RID: 456
			// (get) Token: 0x0600064D RID: 1613 RVA: 0x00016F69 File Offset: 0x00015169
			// (set) Token: 0x0600064E RID: 1614 RVA: 0x00016F81 File Offset: 0x00015181
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public byte LOCKED
			{
				get
				{
					return (byte)base[this.tableAnalysisProjects.LOCKEDColumn];
				}
				set
				{
					base[this.tableAnalysisProjects.LOCKEDColumn] = value;
				}
			}

			// Token: 0x170001C9 RID: 457
			// (get) Token: 0x0600064F RID: 1615 RVA: 0x00016F9A File Offset: 0x0001519A
			// (set) Token: 0x06000650 RID: 1616 RVA: 0x00016FBC File Offset: 0x000151BC
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			public AnalysisDataSet.AnalysisRow AnalysisRow
			{
				get
				{
					return (AnalysisDataSet.AnalysisRow)base.GetParentRow(base.Table.ParentRelations["FK_Analysis_AnalysisProjects"]);
				}
				set
				{
					base.SetParentRow(value, base.Table.ParentRelations["FK_Analysis_AnalysisProjects"]);
				}
			}

			// Token: 0x06000651 RID: 1617 RVA: 0x00016FDA File Offset: 0x000151DA
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			public bool IsPROJ_NAMENull()
			{
				return base.IsNull(this.tableAnalysisProjects.PROJ_NAMEColumn);
			}

			// Token: 0x06000652 RID: 1618 RVA: 0x00016FED File Offset: 0x000151ED
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public void SetPROJ_NAMENull()
			{
				base[this.tableAnalysisProjects.PROJ_NAMEColumn] = Convert.DBNull;
			}

			// Token: 0x06000653 RID: 1619 RVA: 0x00017005 File Offset: 0x00015205
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public bool IsPRIORITYNull()
			{
				return base.IsNull(this.tableAnalysisProjects.PRIORITYColumn);
			}

			// Token: 0x06000654 RID: 1620 RVA: 0x00017018 File Offset: 0x00015218
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public void SetPRIORITYNull()
			{
				base[this.tableAnalysisProjects.PRIORITYColumn] = Convert.DBNull;
			}

			// Token: 0x06000655 RID: 1621 RVA: 0x00017030 File Offset: 0x00015230
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public bool IsABSOLUTE_PRIORITYNull()
			{
				return base.IsNull(this.tableAnalysisProjects.ABSOLUTE_PRIORITYColumn);
			}

			// Token: 0x06000656 RID: 1622 RVA: 0x00017043 File Offset: 0x00015243
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public void SetABSOLUTE_PRIORITYNull()
			{
				base[this.tableAnalysisProjects.ABSOLUTE_PRIORITYColumn] = Convert.DBNull;
			}

			// Token: 0x06000657 RID: 1623 RVA: 0x0001705B File Offset: 0x0001525B
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			public bool IsORIGINAL_START_DATENull()
			{
				return base.IsNull(this.tableAnalysisProjects.ORIGINAL_START_DATEColumn);
			}

			// Token: 0x06000658 RID: 1624 RVA: 0x0001706E File Offset: 0x0001526E
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			public void SetORIGINAL_START_DATENull()
			{
				base[this.tableAnalysisProjects.ORIGINAL_START_DATEColumn] = Convert.DBNull;
			}

			// Token: 0x06000659 RID: 1625 RVA: 0x00017086 File Offset: 0x00015286
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public bool IsORIGINAL_END_DATENull()
			{
				return base.IsNull(this.tableAnalysisProjects.ORIGINAL_END_DATEColumn);
			}

			// Token: 0x0600065A RID: 1626 RVA: 0x00017099 File Offset: 0x00015299
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			public void SetORIGINAL_END_DATENull()
			{
				base[this.tableAnalysisProjects.ORIGINAL_END_DATEColumn] = Convert.DBNull;
			}

			// Token: 0x0600065B RID: 1627 RVA: 0x000170B1 File Offset: 0x000152B1
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public bool IsSTART_DATENull()
			{
				return base.IsNull(this.tableAnalysisProjects.START_DATEColumn);
			}

			// Token: 0x0600065C RID: 1628 RVA: 0x000170C4 File Offset: 0x000152C4
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			public void SetSTART_DATENull()
			{
				base[this.tableAnalysisProjects.START_DATEColumn] = Convert.DBNull;
			}

			// Token: 0x0600065D RID: 1629 RVA: 0x000170DC File Offset: 0x000152DC
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public bool IsSNETNull()
			{
				return base.IsNull(this.tableAnalysisProjects.SNETColumn);
			}

			// Token: 0x0600065E RID: 1630 RVA: 0x000170EF File Offset: 0x000152EF
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			public void SetSNETNull()
			{
				base[this.tableAnalysisProjects.SNETColumn] = Convert.DBNull;
			}

			// Token: 0x0600065F RID: 1631 RVA: 0x00017107 File Offset: 0x00015307
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public bool IsFNLTNull()
			{
				return base.IsNull(this.tableAnalysisProjects.FNLTColumn);
			}

			// Token: 0x06000660 RID: 1632 RVA: 0x0001711A File Offset: 0x0001531A
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			public void SetFNLTNull()
			{
				base[this.tableAnalysisProjects.FNLTColumn] = Convert.DBNull;
			}

			// Token: 0x06000661 RID: 1633 RVA: 0x00017132 File Offset: 0x00015332
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public AnalysisDataSet.AnalysisProjectImpactRow[] GetAnalysisProjectImpactRows()
			{
				if (base.Table.ChildRelations["FK_AnalysisProjects_AnalysisProjectImpact"] == null)
				{
					return new AnalysisDataSet.AnalysisProjectImpactRow[0];
				}
				return (AnalysisDataSet.AnalysisProjectImpactRow[])base.GetChildRows(base.Table.ChildRelations["FK_AnalysisProjects_AnalysisProjectImpact"]);
			}

			// Token: 0x040001BA RID: 442
			private AnalysisDataSet.AnalysisProjectsDataTable tableAnalysisProjects;
		}

		// Token: 0x02000058 RID: 88
		public class AnalysisPriorityDataRow : DataRow
		{
			// Token: 0x06000662 RID: 1634 RVA: 0x00017172 File Offset: 0x00015372
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			internal AnalysisPriorityDataRow(DataRowBuilder rb) : base(rb)
			{
				this.tableAnalysisPriorityData = (AnalysisDataSet.AnalysisPriorityDataDataTable)base.Table;
			}

			// Token: 0x170001CA RID: 458
			// (get) Token: 0x06000663 RID: 1635 RVA: 0x0001718C File Offset: 0x0001538C
			// (set) Token: 0x06000664 RID: 1636 RVA: 0x000171A4 File Offset: 0x000153A4
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			public Guid ANALYSIS_UID
			{
				get
				{
					return (Guid)base[this.tableAnalysisPriorityData.ANALYSIS_UIDColumn];
				}
				set
				{
					base[this.tableAnalysisPriorityData.ANALYSIS_UIDColumn] = value;
				}
			}

			// Token: 0x170001CB RID: 459
			// (get) Token: 0x06000665 RID: 1637 RVA: 0x000171BD File Offset: 0x000153BD
			// (set) Token: 0x06000666 RID: 1638 RVA: 0x000171D5 File Offset: 0x000153D5
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public Guid MD_PROP_UID
			{
				get
				{
					return (Guid)base[this.tableAnalysisPriorityData.MD_PROP_UIDColumn];
				}
				set
				{
					base[this.tableAnalysisPriorityData.MD_PROP_UIDColumn] = value;
				}
			}

			// Token: 0x170001CC RID: 460
			// (get) Token: 0x06000667 RID: 1639 RVA: 0x000171F0 File Offset: 0x000153F0
			// (set) Token: 0x06000668 RID: 1640 RVA: 0x00017234 File Offset: 0x00015434
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public string MD_PROP_NAME
			{
				get
				{
					string result;
					try
					{
						result = (string)base[this.tableAnalysisPriorityData.MD_PROP_NAMEColumn];
					}
					catch (InvalidCastException innerException)
					{
						throw new StrongTypingException("The value for column 'MD_PROP_NAME' in table 'AnalysisPriorityData' is DBNull.", innerException);
					}
					return result;
				}
				set
				{
					base[this.tableAnalysisPriorityData.MD_PROP_NAMEColumn] = value;
				}
			}

			// Token: 0x170001CD RID: 461
			// (get) Token: 0x06000669 RID: 1641 RVA: 0x00017248 File Offset: 0x00015448
			// (set) Token: 0x0600066A RID: 1642 RVA: 0x00017260 File Offset: 0x00015460
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			public double WEIGHT
			{
				get
				{
					return (double)base[this.tableAnalysisPriorityData.WEIGHTColumn];
				}
				set
				{
					base[this.tableAnalysisPriorityData.WEIGHTColumn] = value;
				}
			}

			// Token: 0x170001CE RID: 462
			// (get) Token: 0x0600066B RID: 1643 RVA: 0x0001727C File Offset: 0x0001547C
			// (set) Token: 0x0600066C RID: 1644 RVA: 0x000172C0 File Offset: 0x000154C0
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			public decimal MIN_VALUE
			{
				get
				{
					decimal result;
					try
					{
						result = (decimal)base[this.tableAnalysisPriorityData.MIN_VALUEColumn];
					}
					catch (InvalidCastException innerException)
					{
						throw new StrongTypingException("The value for column 'MIN_VALUE' in table 'AnalysisPriorityData' is DBNull.", innerException);
					}
					return result;
				}
				set
				{
					base[this.tableAnalysisPriorityData.MIN_VALUEColumn] = value;
				}
			}

			// Token: 0x170001CF RID: 463
			// (get) Token: 0x0600066D RID: 1645 RVA: 0x000172DC File Offset: 0x000154DC
			// (set) Token: 0x0600066E RID: 1646 RVA: 0x00017320 File Offset: 0x00015520
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public decimal MAX_VALUE
			{
				get
				{
					decimal result;
					try
					{
						result = (decimal)base[this.tableAnalysisPriorityData.MAX_VALUEColumn];
					}
					catch (InvalidCastException innerException)
					{
						throw new StrongTypingException("The value for column 'MAX_VALUE' in table 'AnalysisPriorityData' is DBNull.", innerException);
					}
					return result;
				}
				set
				{
					base[this.tableAnalysisPriorityData.MAX_VALUEColumn] = value;
				}
			}

			// Token: 0x170001D0 RID: 464
			// (get) Token: 0x0600066F RID: 1647 RVA: 0x00017339 File Offset: 0x00015539
			// (set) Token: 0x06000670 RID: 1648 RVA: 0x0001735B File Offset: 0x0001555B
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			public AnalysisDataSet.AnalysisRow AnalysisRow
			{
				get
				{
					return (AnalysisDataSet.AnalysisRow)base.GetParentRow(base.Table.ParentRelations["FK_Analysis_AnalysisPriorityData"]);
				}
				set
				{
					base.SetParentRow(value, base.Table.ParentRelations["FK_Analysis_AnalysisPriorityData"]);
				}
			}

			// Token: 0x06000671 RID: 1649 RVA: 0x00017379 File Offset: 0x00015579
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public bool IsMD_PROP_NAMENull()
			{
				return base.IsNull(this.tableAnalysisPriorityData.MD_PROP_NAMEColumn);
			}

			// Token: 0x06000672 RID: 1650 RVA: 0x0001738C File Offset: 0x0001558C
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public void SetMD_PROP_NAMENull()
			{
				base[this.tableAnalysisPriorityData.MD_PROP_NAMEColumn] = Convert.DBNull;
			}

			// Token: 0x06000673 RID: 1651 RVA: 0x000173A4 File Offset: 0x000155A4
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public bool IsMIN_VALUENull()
			{
				return base.IsNull(this.tableAnalysisPriorityData.MIN_VALUEColumn);
			}

			// Token: 0x06000674 RID: 1652 RVA: 0x000173B7 File Offset: 0x000155B7
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public void SetMIN_VALUENull()
			{
				base[this.tableAnalysisPriorityData.MIN_VALUEColumn] = Convert.DBNull;
			}

			// Token: 0x06000675 RID: 1653 RVA: 0x000173CF File Offset: 0x000155CF
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			public bool IsMAX_VALUENull()
			{
				return base.IsNull(this.tableAnalysisPriorityData.MAX_VALUEColumn);
			}

			// Token: 0x06000676 RID: 1654 RVA: 0x000173E2 File Offset: 0x000155E2
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			public void SetMAX_VALUENull()
			{
				base[this.tableAnalysisPriorityData.MAX_VALUEColumn] = Convert.DBNull;
			}

			// Token: 0x040001BB RID: 443
			private AnalysisDataSet.AnalysisPriorityDataDataTable tableAnalysisPriorityData;
		}

		// Token: 0x02000059 RID: 89
		public class AnalysisProjectImpactRow : DataRow
		{
			// Token: 0x06000677 RID: 1655 RVA: 0x000173FA File Offset: 0x000155FA
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			internal AnalysisProjectImpactRow(DataRowBuilder rb) : base(rb)
			{
				this.tableAnalysisProjectImpact = (AnalysisDataSet.AnalysisProjectImpactDataTable)base.Table;
			}

			// Token: 0x170001D1 RID: 465
			// (get) Token: 0x06000678 RID: 1656 RVA: 0x00017414 File Offset: 0x00015614
			// (set) Token: 0x06000679 RID: 1657 RVA: 0x0001742C File Offset: 0x0001562C
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			public Guid ANALYSIS_UID
			{
				get
				{
					return (Guid)base[this.tableAnalysisProjectImpact.ANALYSIS_UIDColumn];
				}
				set
				{
					base[this.tableAnalysisProjectImpact.ANALYSIS_UIDColumn] = value;
				}
			}

			// Token: 0x170001D2 RID: 466
			// (get) Token: 0x0600067A RID: 1658 RVA: 0x00017445 File Offset: 0x00015645
			// (set) Token: 0x0600067B RID: 1659 RVA: 0x0001745D File Offset: 0x0001565D
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			public Guid PROJ_UID
			{
				get
				{
					return (Guid)base[this.tableAnalysisProjectImpact.PROJ_UIDColumn];
				}
				set
				{
					base[this.tableAnalysisProjectImpact.PROJ_UIDColumn] = value;
				}
			}

			// Token: 0x170001D3 RID: 467
			// (get) Token: 0x0600067C RID: 1660 RVA: 0x00017476 File Offset: 0x00015676
			// (set) Token: 0x0600067D RID: 1661 RVA: 0x0001748E File Offset: 0x0001568E
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			public Guid DRIVER_UID
			{
				get
				{
					return (Guid)base[this.tableAnalysisProjectImpact.DRIVER_UIDColumn];
				}
				set
				{
					base[this.tableAnalysisProjectImpact.DRIVER_UIDColumn] = value;
				}
			}

			// Token: 0x170001D4 RID: 468
			// (get) Token: 0x0600067E RID: 1662 RVA: 0x000174A8 File Offset: 0x000156A8
			// (set) Token: 0x0600067F RID: 1663 RVA: 0x000174EC File Offset: 0x000156EC
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public Guid LT_STRUCT_UID
			{
				get
				{
					Guid result;
					try
					{
						result = (Guid)base[this.tableAnalysisProjectImpact.LT_STRUCT_UIDColumn];
					}
					catch (InvalidCastException innerException)
					{
						throw new StrongTypingException("The value for column 'LT_STRUCT_UID' in table 'AnalysisProjectImpact' is DBNull.", innerException);
					}
					return result;
				}
				set
				{
					base[this.tableAnalysisProjectImpact.LT_STRUCT_UIDColumn] = value;
				}
			}

			// Token: 0x170001D5 RID: 469
			// (get) Token: 0x06000680 RID: 1664 RVA: 0x00017505 File Offset: 0x00015705
			// (set) Token: 0x06000681 RID: 1665 RVA: 0x00017527 File Offset: 0x00015727
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public AnalysisDataSet.AnalysisProjectsRow AnalysisProjectsRowParent
			{
				get
				{
					return (AnalysisDataSet.AnalysisProjectsRow)base.GetParentRow(base.Table.ParentRelations["FK_AnalysisProjects_AnalysisProjectImpact"]);
				}
				set
				{
					base.SetParentRow(value, base.Table.ParentRelations["FK_AnalysisProjects_AnalysisProjectImpact"]);
				}
			}

			// Token: 0x06000682 RID: 1666 RVA: 0x00017545 File Offset: 0x00015745
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public bool IsLT_STRUCT_UIDNull()
			{
				return base.IsNull(this.tableAnalysisProjectImpact.LT_STRUCT_UIDColumn);
			}

			// Token: 0x06000683 RID: 1667 RVA: 0x00017558 File Offset: 0x00015758
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			public void SetLT_STRUCT_UIDNull()
			{
				base[this.tableAnalysisProjectImpact.LT_STRUCT_UIDColumn] = Convert.DBNull;
			}

			// Token: 0x040001BC RID: 444
			private AnalysisDataSet.AnalysisProjectImpactDataTable tableAnalysisProjectImpact;
		}

		// Token: 0x0200005A RID: 90
		public class AnalysisOptimizerSolutionsRow : DataRow
		{
			// Token: 0x06000684 RID: 1668 RVA: 0x00017570 File Offset: 0x00015770
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			internal AnalysisOptimizerSolutionsRow(DataRowBuilder rb) : base(rb)
			{
				this.tableAnalysisOptimizerSolutions = (AnalysisDataSet.AnalysisOptimizerSolutionsDataTable)base.Table;
			}

			// Token: 0x170001D6 RID: 470
			// (get) Token: 0x06000685 RID: 1669 RVA: 0x0001758A File Offset: 0x0001578A
			// (set) Token: 0x06000686 RID: 1670 RVA: 0x000175A2 File Offset: 0x000157A2
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public Guid ANALYSIS_UID
			{
				get
				{
					return (Guid)base[this.tableAnalysisOptimizerSolutions.ANALYSIS_UIDColumn];
				}
				set
				{
					base[this.tableAnalysisOptimizerSolutions.ANALYSIS_UIDColumn] = value;
				}
			}

			// Token: 0x170001D7 RID: 471
			// (get) Token: 0x06000687 RID: 1671 RVA: 0x000175BB File Offset: 0x000157BB
			// (set) Token: 0x06000688 RID: 1672 RVA: 0x000175D3 File Offset: 0x000157D3
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			public Guid SOLUTION_UID
			{
				get
				{
					return (Guid)base[this.tableAnalysisOptimizerSolutions.SOLUTION_UIDColumn];
				}
				set
				{
					base[this.tableAnalysisOptimizerSolutions.SOLUTION_UIDColumn] = value;
				}
			}

			// Token: 0x170001D8 RID: 472
			// (get) Token: 0x06000689 RID: 1673 RVA: 0x000175EC File Offset: 0x000157EC
			// (set) Token: 0x0600068A RID: 1674 RVA: 0x00017604 File Offset: 0x00015804
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public string SOLUTION_NAME
			{
				get
				{
					return (string)base[this.tableAnalysisOptimizerSolutions.SOLUTION_NAMEColumn];
				}
				set
				{
					base[this.tableAnalysisOptimizerSolutions.SOLUTION_NAMEColumn] = value;
				}
			}

			// Token: 0x170001D9 RID: 473
			// (get) Token: 0x0600068B RID: 1675 RVA: 0x00017618 File Offset: 0x00015818
			// (set) Token: 0x0600068C RID: 1676 RVA: 0x0001765C File Offset: 0x0001585C
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			public string SOLUTION_DESCRIPTION
			{
				get
				{
					string result;
					try
					{
						result = (string)base[this.tableAnalysisOptimizerSolutions.SOLUTION_DESCRIPTIONColumn];
					}
					catch (InvalidCastException innerException)
					{
						throw new StrongTypingException("The value for column 'SOLUTION_DESCRIPTION' in table 'AnalysisOptimizerSolutions' is DBNull.", innerException);
					}
					return result;
				}
				set
				{
					base[this.tableAnalysisOptimizerSolutions.SOLUTION_DESCRIPTIONColumn] = value;
				}
			}

			// Token: 0x170001DA RID: 474
			// (get) Token: 0x0600068D RID: 1677 RVA: 0x00017670 File Offset: 0x00015870
			// (set) Token: 0x0600068E RID: 1678 RVA: 0x00017688 File Offset: 0x00015888
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public Guid FRONTIER_UID
			{
				get
				{
					return (Guid)base[this.tableAnalysisOptimizerSolutions.FRONTIER_UIDColumn];
				}
				set
				{
					base[this.tableAnalysisOptimizerSolutions.FRONTIER_UIDColumn] = value;
				}
			}

			// Token: 0x170001DB RID: 475
			// (get) Token: 0x0600068F RID: 1679 RVA: 0x000176A4 File Offset: 0x000158A4
			// (set) Token: 0x06000690 RID: 1680 RVA: 0x000176E8 File Offset: 0x000158E8
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			public bool OPT_USE_DEPENDENCIES
			{
				get
				{
					bool result;
					try
					{
						result = (bool)base[this.tableAnalysisOptimizerSolutions.OPT_USE_DEPENDENCIESColumn];
					}
					catch (InvalidCastException innerException)
					{
						throw new StrongTypingException("The value for column 'OPT_USE_DEPENDENCIES' in table 'AnalysisOptimizerSolutions' is DBNull.", innerException);
					}
					return result;
				}
				set
				{
					base[this.tableAnalysisOptimizerSolutions.OPT_USE_DEPENDENCIESColumn] = value;
				}
			}

			// Token: 0x170001DC RID: 476
			// (get) Token: 0x06000691 RID: 1681 RVA: 0x00017704 File Offset: 0x00015904
			// (set) Token: 0x06000692 RID: 1682 RVA: 0x00017748 File Offset: 0x00015948
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			public DateTime CREATED_DATE
			{
				get
				{
					DateTime result;
					try
					{
						result = (DateTime)base[this.tableAnalysisOptimizerSolutions.CREATED_DATEColumn];
					}
					catch (InvalidCastException innerException)
					{
						throw new StrongTypingException("The value for column 'CREATED_DATE' in table 'AnalysisOptimizerSolutions' is DBNull.", innerException);
					}
					return result;
				}
				set
				{
					base[this.tableAnalysisOptimizerSolutions.CREATED_DATEColumn] = value;
				}
			}

			// Token: 0x170001DD RID: 477
			// (get) Token: 0x06000693 RID: 1683 RVA: 0x00017764 File Offset: 0x00015964
			// (set) Token: 0x06000694 RID: 1684 RVA: 0x000177A8 File Offset: 0x000159A8
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public DateTime MOD_DATE
			{
				get
				{
					DateTime result;
					try
					{
						result = (DateTime)base[this.tableAnalysisOptimizerSolutions.MOD_DATEColumn];
					}
					catch (InvalidCastException innerException)
					{
						throw new StrongTypingException("The value for column 'MOD_DATE' in table 'AnalysisOptimizerSolutions' is DBNull.", innerException);
					}
					return result;
				}
				set
				{
					base[this.tableAnalysisOptimizerSolutions.MOD_DATEColumn] = value;
				}
			}

			// Token: 0x170001DE RID: 478
			// (get) Token: 0x06000695 RID: 1685 RVA: 0x000177C4 File Offset: 0x000159C4
			// (set) Token: 0x06000696 RID: 1686 RVA: 0x00017808 File Offset: 0x00015A08
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			public Guid LAST_UPDATED_BY_RES_UID
			{
				get
				{
					Guid result;
					try
					{
						result = (Guid)base[this.tableAnalysisOptimizerSolutions.LAST_UPDATED_BY_RES_UIDColumn];
					}
					catch (InvalidCastException innerException)
					{
						throw new StrongTypingException("The value for column 'LAST_UPDATED_BY_RES_UID' in table 'AnalysisOptimizerSolutions' is DBNull.", innerException);
					}
					return result;
				}
				set
				{
					base[this.tableAnalysisOptimizerSolutions.LAST_UPDATED_BY_RES_UIDColumn] = value;
				}
			}

			// Token: 0x170001DF RID: 479
			// (get) Token: 0x06000697 RID: 1687 RVA: 0x00017824 File Offset: 0x00015A24
			// (set) Token: 0x06000698 RID: 1688 RVA: 0x00017868 File Offset: 0x00015A68
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			public string LAST_UPDATED_BY_RES_NAME
			{
				get
				{
					string result;
					try
					{
						result = (string)base[this.tableAnalysisOptimizerSolutions.LAST_UPDATED_BY_RES_NAMEColumn];
					}
					catch (InvalidCastException innerException)
					{
						throw new StrongTypingException("The value for column 'LAST_UPDATED_BY_RES_NAME' in table 'AnalysisOptimizerSolutions' is DBNull.", innerException);
					}
					return result;
				}
				set
				{
					base[this.tableAnalysisOptimizerSolutions.LAST_UPDATED_BY_RES_NAMEColumn] = value;
				}
			}

			// Token: 0x170001E0 RID: 480
			// (get) Token: 0x06000699 RID: 1689 RVA: 0x0001787C File Offset: 0x00015A7C
			// (set) Token: 0x0600069A RID: 1690 RVA: 0x000178C0 File Offset: 0x00015AC0
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			public Guid CREATED_BY_RES_UID
			{
				get
				{
					Guid result;
					try
					{
						result = (Guid)base[this.tableAnalysisOptimizerSolutions.CREATED_BY_RES_UIDColumn];
					}
					catch (InvalidCastException innerException)
					{
						throw new StrongTypingException("The value for column 'CREATED_BY_RES_UID' in table 'AnalysisOptimizerSolutions' is DBNull.", innerException);
					}
					return result;
				}
				set
				{
					base[this.tableAnalysisOptimizerSolutions.CREATED_BY_RES_UIDColumn] = value;
				}
			}

			// Token: 0x170001E1 RID: 481
			// (get) Token: 0x0600069B RID: 1691 RVA: 0x000178DC File Offset: 0x00015ADC
			// (set) Token: 0x0600069C RID: 1692 RVA: 0x00017920 File Offset: 0x00015B20
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			public string CREATED_BY_RES_NAME
			{
				get
				{
					string result;
					try
					{
						result = (string)base[this.tableAnalysisOptimizerSolutions.CREATED_BY_RES_NAMEColumn];
					}
					catch (InvalidCastException innerException)
					{
						throw new StrongTypingException("The value for column 'CREATED_BY_RES_NAME' in table 'AnalysisOptimizerSolutions' is DBNull.", innerException);
					}
					return result;
				}
				set
				{
					base[this.tableAnalysisOptimizerSolutions.CREATED_BY_RES_NAMEColumn] = value;
				}
			}

			// Token: 0x170001E2 RID: 482
			// (get) Token: 0x0600069D RID: 1693 RVA: 0x00017934 File Offset: 0x00015B34
			// (set) Token: 0x0600069E RID: 1694 RVA: 0x00017956 File Offset: 0x00015B56
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public AnalysisDataSet.AnalysisRow AnalysisRow
			{
				get
				{
					return (AnalysisDataSet.AnalysisRow)base.GetParentRow(base.Table.ParentRelations["FK_Analysis_AnalysisOptimizerSolutions"]);
				}
				set
				{
					base.SetParentRow(value, base.Table.ParentRelations["FK_Analysis_AnalysisOptimizerSolutions"]);
				}
			}

			// Token: 0x0600069F RID: 1695 RVA: 0x00017974 File Offset: 0x00015B74
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public bool IsSOLUTION_DESCRIPTIONNull()
			{
				return base.IsNull(this.tableAnalysisOptimizerSolutions.SOLUTION_DESCRIPTIONColumn);
			}

			// Token: 0x060006A0 RID: 1696 RVA: 0x00017987 File Offset: 0x00015B87
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public void SetSOLUTION_DESCRIPTIONNull()
			{
				base[this.tableAnalysisOptimizerSolutions.SOLUTION_DESCRIPTIONColumn] = Convert.DBNull;
			}

			// Token: 0x060006A1 RID: 1697 RVA: 0x0001799F File Offset: 0x00015B9F
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			public bool IsOPT_USE_DEPENDENCIESNull()
			{
				return base.IsNull(this.tableAnalysisOptimizerSolutions.OPT_USE_DEPENDENCIESColumn);
			}

			// Token: 0x060006A2 RID: 1698 RVA: 0x000179B2 File Offset: 0x00015BB2
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public void SetOPT_USE_DEPENDENCIESNull()
			{
				base[this.tableAnalysisOptimizerSolutions.OPT_USE_DEPENDENCIESColumn] = Convert.DBNull;
			}

			// Token: 0x060006A3 RID: 1699 RVA: 0x000179CA File Offset: 0x00015BCA
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			public bool IsCREATED_DATENull()
			{
				return base.IsNull(this.tableAnalysisOptimizerSolutions.CREATED_DATEColumn);
			}

			// Token: 0x060006A4 RID: 1700 RVA: 0x000179DD File Offset: 0x00015BDD
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public void SetCREATED_DATENull()
			{
				base[this.tableAnalysisOptimizerSolutions.CREATED_DATEColumn] = Convert.DBNull;
			}

			// Token: 0x060006A5 RID: 1701 RVA: 0x000179F5 File Offset: 0x00015BF5
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public bool IsMOD_DATENull()
			{
				return base.IsNull(this.tableAnalysisOptimizerSolutions.MOD_DATEColumn);
			}

			// Token: 0x060006A6 RID: 1702 RVA: 0x00017A08 File Offset: 0x00015C08
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			public void SetMOD_DATENull()
			{
				base[this.tableAnalysisOptimizerSolutions.MOD_DATEColumn] = Convert.DBNull;
			}

			// Token: 0x060006A7 RID: 1703 RVA: 0x00017A20 File Offset: 0x00015C20
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public bool IsLAST_UPDATED_BY_RES_UIDNull()
			{
				return base.IsNull(this.tableAnalysisOptimizerSolutions.LAST_UPDATED_BY_RES_UIDColumn);
			}

			// Token: 0x060006A8 RID: 1704 RVA: 0x00017A33 File Offset: 0x00015C33
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public void SetLAST_UPDATED_BY_RES_UIDNull()
			{
				base[this.tableAnalysisOptimizerSolutions.LAST_UPDATED_BY_RES_UIDColumn] = Convert.DBNull;
			}

			// Token: 0x060006A9 RID: 1705 RVA: 0x00017A4B File Offset: 0x00015C4B
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			public bool IsLAST_UPDATED_BY_RES_NAMENull()
			{
				return base.IsNull(this.tableAnalysisOptimizerSolutions.LAST_UPDATED_BY_RES_NAMEColumn);
			}

			// Token: 0x060006AA RID: 1706 RVA: 0x00017A5E File Offset: 0x00015C5E
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			public void SetLAST_UPDATED_BY_RES_NAMENull()
			{
				base[this.tableAnalysisOptimizerSolutions.LAST_UPDATED_BY_RES_NAMEColumn] = Convert.DBNull;
			}

			// Token: 0x060006AB RID: 1707 RVA: 0x00017A76 File Offset: 0x00015C76
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			public bool IsCREATED_BY_RES_UIDNull()
			{
				return base.IsNull(this.tableAnalysisOptimizerSolutions.CREATED_BY_RES_UIDColumn);
			}

			// Token: 0x060006AC RID: 1708 RVA: 0x00017A89 File Offset: 0x00015C89
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public void SetCREATED_BY_RES_UIDNull()
			{
				base[this.tableAnalysisOptimizerSolutions.CREATED_BY_RES_UIDColumn] = Convert.DBNull;
			}

			// Token: 0x060006AD RID: 1709 RVA: 0x00017AA1 File Offset: 0x00015CA1
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			public bool IsCREATED_BY_RES_NAMENull()
			{
				return base.IsNull(this.tableAnalysisOptimizerSolutions.CREATED_BY_RES_NAMEColumn);
			}

			// Token: 0x060006AE RID: 1710 RVA: 0x00017AB4 File Offset: 0x00015CB4
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			public void SetCREATED_BY_RES_NAMENull()
			{
				base[this.tableAnalysisOptimizerSolutions.CREATED_BY_RES_NAMEColumn] = Convert.DBNull;
			}

			// Token: 0x040001BD RID: 445
			private AnalysisDataSet.AnalysisOptimizerSolutionsDataTable tableAnalysisOptimizerSolutions;
		}

		// Token: 0x0200005B RID: 91
		public class AnalysisPlannerSolutionsRow : DataRow
		{
			// Token: 0x060006AF RID: 1711 RVA: 0x00017ACC File Offset: 0x00015CCC
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			internal AnalysisPlannerSolutionsRow(DataRowBuilder rb) : base(rb)
			{
				this.tableAnalysisPlannerSolutions = (AnalysisDataSet.AnalysisPlannerSolutionsDataTable)base.Table;
			}

			// Token: 0x170001E3 RID: 483
			// (get) Token: 0x060006B0 RID: 1712 RVA: 0x00017AE6 File Offset: 0x00015CE6
			// (set) Token: 0x060006B1 RID: 1713 RVA: 0x00017AFE File Offset: 0x00015CFE
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public Guid SOLUTION_UID
			{
				get
				{
					return (Guid)base[this.tableAnalysisPlannerSolutions.SOLUTION_UIDColumn];
				}
				set
				{
					base[this.tableAnalysisPlannerSolutions.SOLUTION_UIDColumn] = value;
				}
			}

			// Token: 0x170001E4 RID: 484
			// (get) Token: 0x060006B2 RID: 1714 RVA: 0x00017B17 File Offset: 0x00015D17
			// (set) Token: 0x060006B3 RID: 1715 RVA: 0x00017B2F File Offset: 0x00015D2F
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public Guid OPTIMIZER_SOLUTION_UID
			{
				get
				{
					return (Guid)base[this.tableAnalysisPlannerSolutions.OPTIMIZER_SOLUTION_UIDColumn];
				}
				set
				{
					base[this.tableAnalysisPlannerSolutions.OPTIMIZER_SOLUTION_UIDColumn] = value;
				}
			}

			// Token: 0x170001E5 RID: 485
			// (get) Token: 0x060006B4 RID: 1716 RVA: 0x00017B48 File Offset: 0x00015D48
			// (set) Token: 0x060006B5 RID: 1717 RVA: 0x00017B60 File Offset: 0x00015D60
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			public Guid ANALYSIS_UID
			{
				get
				{
					return (Guid)base[this.tableAnalysisPlannerSolutions.ANALYSIS_UIDColumn];
				}
				set
				{
					base[this.tableAnalysisPlannerSolutions.ANALYSIS_UIDColumn] = value;
				}
			}

			// Token: 0x170001E6 RID: 486
			// (get) Token: 0x060006B6 RID: 1718 RVA: 0x00017B79 File Offset: 0x00015D79
			// (set) Token: 0x060006B7 RID: 1719 RVA: 0x00017B91 File Offset: 0x00015D91
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public string SOLUTION_NAME
			{
				get
				{
					return (string)base[this.tableAnalysisPlannerSolutions.SOLUTION_NAMEColumn];
				}
				set
				{
					base[this.tableAnalysisPlannerSolutions.SOLUTION_NAMEColumn] = value;
				}
			}

			// Token: 0x170001E7 RID: 487
			// (get) Token: 0x060006B8 RID: 1720 RVA: 0x00017BA8 File Offset: 0x00015DA8
			// (set) Token: 0x060006B9 RID: 1721 RVA: 0x00017BEC File Offset: 0x00015DEC
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			public string SOLUTION_DESCRIPTION
			{
				get
				{
					string result;
					try
					{
						result = (string)base[this.tableAnalysisPlannerSolutions.SOLUTION_DESCRIPTIONColumn];
					}
					catch (InvalidCastException innerException)
					{
						throw new StrongTypingException("The value for column 'SOLUTION_DESCRIPTION' in table 'AnalysisPlannerSolutions' is DBNull.", innerException);
					}
					return result;
				}
				set
				{
					base[this.tableAnalysisPlannerSolutions.SOLUTION_DESCRIPTIONColumn] = value;
				}
			}

			// Token: 0x170001E8 RID: 488
			// (get) Token: 0x060006BA RID: 1722 RVA: 0x00017C00 File Offset: 0x00015E00
			// (set) Token: 0x060006BB RID: 1723 RVA: 0x00017C18 File Offset: 0x00015E18
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			public byte CONSTRAINT_TYPE
			{
				get
				{
					return (byte)base[this.tableAnalysisPlannerSolutions.CONSTRAINT_TYPEColumn];
				}
				set
				{
					base[this.tableAnalysisPlannerSolutions.CONSTRAINT_TYPEColumn] = value;
				}
			}

			// Token: 0x170001E9 RID: 489
			// (get) Token: 0x060006BC RID: 1724 RVA: 0x00017C31 File Offset: 0x00015E31
			// (set) Token: 0x060006BD RID: 1725 RVA: 0x00017C49 File Offset: 0x00015E49
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public decimal CONSTRAINT_VALUE
			{
				get
				{
					return (decimal)base[this.tableAnalysisPlannerSolutions.CONSTRAINT_VALUEColumn];
				}
				set
				{
					base[this.tableAnalysisPlannerSolutions.CONSTRAINT_VALUEColumn] = value;
				}
			}

			// Token: 0x170001EA RID: 490
			// (get) Token: 0x060006BE RID: 1726 RVA: 0x00017C64 File Offset: 0x00015E64
			// (set) Token: 0x060006BF RID: 1727 RVA: 0x00017CA8 File Offset: 0x00015EA8
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			public Guid FRONTIER_UID
			{
				get
				{
					Guid result;
					try
					{
						result = (Guid)base[this.tableAnalysisPlannerSolutions.FRONTIER_UIDColumn];
					}
					catch (InvalidCastException innerException)
					{
						throw new StrongTypingException("The value for column 'FRONTIER_UID' in table 'AnalysisPlannerSolutions' is DBNull.", innerException);
					}
					return result;
				}
				set
				{
					base[this.tableAnalysisPlannerSolutions.FRONTIER_UIDColumn] = value;
				}
			}

			// Token: 0x170001EB RID: 491
			// (get) Token: 0x060006C0 RID: 1728 RVA: 0x00017CC4 File Offset: 0x00015EC4
			// (set) Token: 0x060006C1 RID: 1729 RVA: 0x00017D08 File Offset: 0x00015F08
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			public DateTime MOD_DATE
			{
				get
				{
					DateTime result;
					try
					{
						result = (DateTime)base[this.tableAnalysisPlannerSolutions.MOD_DATEColumn];
					}
					catch (InvalidCastException innerException)
					{
						throw new StrongTypingException("The value for column 'MOD_DATE' in table 'AnalysisPlannerSolutions' is DBNull.", innerException);
					}
					return result;
				}
				set
				{
					base[this.tableAnalysisPlannerSolutions.MOD_DATEColumn] = value;
				}
			}

			// Token: 0x170001EC RID: 492
			// (get) Token: 0x060006C2 RID: 1730 RVA: 0x00017D24 File Offset: 0x00015F24
			// (set) Token: 0x060006C3 RID: 1731 RVA: 0x00017D68 File Offset: 0x00015F68
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			public DateTime CREATED_DATE
			{
				get
				{
					DateTime result;
					try
					{
						result = (DateTime)base[this.tableAnalysisPlannerSolutions.CREATED_DATEColumn];
					}
					catch (InvalidCastException innerException)
					{
						throw new StrongTypingException("The value for column 'CREATED_DATE' in table 'AnalysisPlannerSolutions' is DBNull.", innerException);
					}
					return result;
				}
				set
				{
					base[this.tableAnalysisPlannerSolutions.CREATED_DATEColumn] = value;
				}
			}

			// Token: 0x170001ED RID: 493
			// (get) Token: 0x060006C4 RID: 1732 RVA: 0x00017D84 File Offset: 0x00015F84
			// (set) Token: 0x060006C5 RID: 1733 RVA: 0x00017DC8 File Offset: 0x00015FC8
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			public Guid CREATED_BY_RES_UID
			{
				get
				{
					Guid result;
					try
					{
						result = (Guid)base[this.tableAnalysisPlannerSolutions.CREATED_BY_RES_UIDColumn];
					}
					catch (InvalidCastException innerException)
					{
						throw new StrongTypingException("The value for column 'CREATED_BY_RES_UID' in table 'AnalysisPlannerSolutions' is DBNull.", innerException);
					}
					return result;
				}
				set
				{
					base[this.tableAnalysisPlannerSolutions.CREATED_BY_RES_UIDColumn] = value;
				}
			}

			// Token: 0x170001EE RID: 494
			// (get) Token: 0x060006C6 RID: 1734 RVA: 0x00017DE4 File Offset: 0x00015FE4
			// (set) Token: 0x060006C7 RID: 1735 RVA: 0x00017E28 File Offset: 0x00016028
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			public Guid LAST_UPDATED_BY_RES_UID
			{
				get
				{
					Guid result;
					try
					{
						result = (Guid)base[this.tableAnalysisPlannerSolutions.LAST_UPDATED_BY_RES_UIDColumn];
					}
					catch (InvalidCastException innerException)
					{
						throw new StrongTypingException("The value for column 'LAST_UPDATED_BY_RES_UID' in table 'AnalysisPlannerSolutions' is DBNull.", innerException);
					}
					return result;
				}
				set
				{
					base[this.tableAnalysisPlannerSolutions.LAST_UPDATED_BY_RES_UIDColumn] = value;
				}
			}

			// Token: 0x170001EF RID: 495
			// (get) Token: 0x060006C8 RID: 1736 RVA: 0x00017E44 File Offset: 0x00016044
			// (set) Token: 0x060006C9 RID: 1737 RVA: 0x00017E88 File Offset: 0x00016088
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public string CREATED_BY_RES_NAME
			{
				get
				{
					string result;
					try
					{
						result = (string)base[this.tableAnalysisPlannerSolutions.CREATED_BY_RES_NAMEColumn];
					}
					catch (InvalidCastException innerException)
					{
						throw new StrongTypingException("The value for column 'CREATED_BY_RES_NAME' in table 'AnalysisPlannerSolutions' is DBNull.", innerException);
					}
					return result;
				}
				set
				{
					base[this.tableAnalysisPlannerSolutions.CREATED_BY_RES_NAMEColumn] = value;
				}
			}

			// Token: 0x170001F0 RID: 496
			// (get) Token: 0x060006CA RID: 1738 RVA: 0x00017E9C File Offset: 0x0001609C
			// (set) Token: 0x060006CB RID: 1739 RVA: 0x00017EE0 File Offset: 0x000160E0
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public string LAST_UPDATED_BY_RES_NAME
			{
				get
				{
					string result;
					try
					{
						result = (string)base[this.tableAnalysisPlannerSolutions.LAST_UPDATED_BY_RES_NAMEColumn];
					}
					catch (InvalidCastException innerException)
					{
						throw new StrongTypingException("The value for column 'LAST_UPDATED_BY_RES_NAME' in table 'AnalysisPlannerSolutions' is DBNull.", innerException);
					}
					return result;
				}
				set
				{
					base[this.tableAnalysisPlannerSolutions.LAST_UPDATED_BY_RES_NAMEColumn] = value;
				}
			}

			// Token: 0x170001F1 RID: 497
			// (get) Token: 0x060006CC RID: 1740 RVA: 0x00017EF4 File Offset: 0x000160F4
			// (set) Token: 0x060006CD RID: 1741 RVA: 0x00017F0C File Offset: 0x0001610C
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public byte HIRING_TYPE
			{
				get
				{
					return (byte)base[this.tableAnalysisPlannerSolutions.HIRING_TYPEColumn];
				}
				set
				{
					base[this.tableAnalysisPlannerSolutions.HIRING_TYPEColumn] = value;
				}
			}

			// Token: 0x170001F2 RID: 498
			// (get) Token: 0x060006CE RID: 1742 RVA: 0x00017F25 File Offset: 0x00016125
			// (set) Token: 0x060006CF RID: 1743 RVA: 0x00017F3D File Offset: 0x0001613D
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			public bool OPT_ENF_SCHEDULING_CONS
			{
				get
				{
					return (bool)base[this.tableAnalysisPlannerSolutions.OPT_ENF_SCHEDULING_CONSColumn];
				}
				set
				{
					base[this.tableAnalysisPlannerSolutions.OPT_ENF_SCHEDULING_CONSColumn] = value;
				}
			}

			// Token: 0x170001F3 RID: 499
			// (get) Token: 0x060006D0 RID: 1744 RVA: 0x00017F56 File Offset: 0x00016156
			// (set) Token: 0x060006D1 RID: 1745 RVA: 0x00017F6E File Offset: 0x0001616E
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			public bool OPT_ENF_PROJ_DEP
			{
				get
				{
					return (bool)base[this.tableAnalysisPlannerSolutions.OPT_ENF_PROJ_DEPColumn];
				}
				set
				{
					base[this.tableAnalysisPlannerSolutions.OPT_ENF_PROJ_DEPColumn] = value;
				}
			}

			// Token: 0x170001F4 RID: 500
			// (get) Token: 0x060006D2 RID: 1746 RVA: 0x00017F87 File Offset: 0x00016187
			// (set) Token: 0x060006D3 RID: 1747 RVA: 0x00017F9F File Offset: 0x0001619F
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public byte RATE_TABLE
			{
				get
				{
					return (byte)base[this.tableAnalysisPlannerSolutions.RATE_TABLEColumn];
				}
				set
				{
					base[this.tableAnalysisPlannerSolutions.RATE_TABLEColumn] = value;
				}
			}

			// Token: 0x170001F5 RID: 501
			// (get) Token: 0x060006D4 RID: 1748 RVA: 0x00017FB8 File Offset: 0x000161B8
			// (set) Token: 0x060006D5 RID: 1749 RVA: 0x00017FD0 File Offset: 0x000161D0
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public double ALLOCATION_THRESHOLD
			{
				get
				{
					return (double)base[this.tableAnalysisPlannerSolutions.ALLOCATION_THRESHOLDColumn];
				}
				set
				{
					base[this.tableAnalysisPlannerSolutions.ALLOCATION_THRESHOLDColumn] = value;
				}
			}

			// Token: 0x170001F6 RID: 502
			// (get) Token: 0x060006D6 RID: 1750 RVA: 0x00017FE9 File Offset: 0x000161E9
			// (set) Token: 0x060006D7 RID: 1751 RVA: 0x0001800B File Offset: 0x0001620B
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			public AnalysisDataSet.AnalysisRow AnalysisRow
			{
				get
				{
					return (AnalysisDataSet.AnalysisRow)base.GetParentRow(base.Table.ParentRelations["FK_Analysis_AnalysisPlannerSolutions"]);
				}
				set
				{
					base.SetParentRow(value, base.Table.ParentRelations["FK_Analysis_AnalysisPlannerSolutions"]);
				}
			}

			// Token: 0x060006D8 RID: 1752 RVA: 0x00018029 File Offset: 0x00016229
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			public bool IsSOLUTION_DESCRIPTIONNull()
			{
				return base.IsNull(this.tableAnalysisPlannerSolutions.SOLUTION_DESCRIPTIONColumn);
			}

			// Token: 0x060006D9 RID: 1753 RVA: 0x0001803C File Offset: 0x0001623C
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public void SetSOLUTION_DESCRIPTIONNull()
			{
				base[this.tableAnalysisPlannerSolutions.SOLUTION_DESCRIPTIONColumn] = Convert.DBNull;
			}

			// Token: 0x060006DA RID: 1754 RVA: 0x00018054 File Offset: 0x00016254
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public bool IsFRONTIER_UIDNull()
			{
				return base.IsNull(this.tableAnalysisPlannerSolutions.FRONTIER_UIDColumn);
			}

			// Token: 0x060006DB RID: 1755 RVA: 0x00018067 File Offset: 0x00016267
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			public void SetFRONTIER_UIDNull()
			{
				base[this.tableAnalysisPlannerSolutions.FRONTIER_UIDColumn] = Convert.DBNull;
			}

			// Token: 0x060006DC RID: 1756 RVA: 0x0001807F File Offset: 0x0001627F
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public bool IsMOD_DATENull()
			{
				return base.IsNull(this.tableAnalysisPlannerSolutions.MOD_DATEColumn);
			}

			// Token: 0x060006DD RID: 1757 RVA: 0x00018092 File Offset: 0x00016292
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public void SetMOD_DATENull()
			{
				base[this.tableAnalysisPlannerSolutions.MOD_DATEColumn] = Convert.DBNull;
			}

			// Token: 0x060006DE RID: 1758 RVA: 0x000180AA File Offset: 0x000162AA
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			public bool IsCREATED_DATENull()
			{
				return base.IsNull(this.tableAnalysisPlannerSolutions.CREATED_DATEColumn);
			}

			// Token: 0x060006DF RID: 1759 RVA: 0x000180BD File Offset: 0x000162BD
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			public void SetCREATED_DATENull()
			{
				base[this.tableAnalysisPlannerSolutions.CREATED_DATEColumn] = Convert.DBNull;
			}

			// Token: 0x060006E0 RID: 1760 RVA: 0x000180D5 File Offset: 0x000162D5
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public bool IsCREATED_BY_RES_UIDNull()
			{
				return base.IsNull(this.tableAnalysisPlannerSolutions.CREATED_BY_RES_UIDColumn);
			}

			// Token: 0x060006E1 RID: 1761 RVA: 0x000180E8 File Offset: 0x000162E8
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			public void SetCREATED_BY_RES_UIDNull()
			{
				base[this.tableAnalysisPlannerSolutions.CREATED_BY_RES_UIDColumn] = Convert.DBNull;
			}

			// Token: 0x060006E2 RID: 1762 RVA: 0x00018100 File Offset: 0x00016300
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public bool IsLAST_UPDATED_BY_RES_UIDNull()
			{
				return base.IsNull(this.tableAnalysisPlannerSolutions.LAST_UPDATED_BY_RES_UIDColumn);
			}

			// Token: 0x060006E3 RID: 1763 RVA: 0x00018113 File Offset: 0x00016313
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public void SetLAST_UPDATED_BY_RES_UIDNull()
			{
				base[this.tableAnalysisPlannerSolutions.LAST_UPDATED_BY_RES_UIDColumn] = Convert.DBNull;
			}

			// Token: 0x060006E4 RID: 1764 RVA: 0x0001812B File Offset: 0x0001632B
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			public bool IsCREATED_BY_RES_NAMENull()
			{
				return base.IsNull(this.tableAnalysisPlannerSolutions.CREATED_BY_RES_NAMEColumn);
			}

			// Token: 0x060006E5 RID: 1765 RVA: 0x0001813E File Offset: 0x0001633E
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			public void SetCREATED_BY_RES_NAMENull()
			{
				base[this.tableAnalysisPlannerSolutions.CREATED_BY_RES_NAMEColumn] = Convert.DBNull;
			}

			// Token: 0x060006E6 RID: 1766 RVA: 0x00018156 File Offset: 0x00016356
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			public bool IsLAST_UPDATED_BY_RES_NAMENull()
			{
				return base.IsNull(this.tableAnalysisPlannerSolutions.LAST_UPDATED_BY_RES_NAMEColumn);
			}

			// Token: 0x060006E7 RID: 1767 RVA: 0x00018169 File Offset: 0x00016369
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public void SetLAST_UPDATED_BY_RES_NAMENull()
			{
				base[this.tableAnalysisPlannerSolutions.LAST_UPDATED_BY_RES_NAMEColumn] = Convert.DBNull;
			}

			// Token: 0x040001BE RID: 446
			private AnalysisDataSet.AnalysisPlannerSolutionsDataTable tableAnalysisPlannerSolutions;
		}

		// Token: 0x0200005C RID: 92
		public class AnalysisRemainingCapacityByRoleRow : DataRow
		{
			// Token: 0x060006E8 RID: 1768 RVA: 0x00018181 File Offset: 0x00016381
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			internal AnalysisRemainingCapacityByRoleRow(DataRowBuilder rb) : base(rb)
			{
				this.tableAnalysisRemainingCapacityByRole = (AnalysisDataSet.AnalysisRemainingCapacityByRoleDataTable)base.Table;
			}

			// Token: 0x170001F7 RID: 503
			// (get) Token: 0x060006E9 RID: 1769 RVA: 0x0001819B File Offset: 0x0001639B
			// (set) Token: 0x060006EA RID: 1770 RVA: 0x000181B3 File Offset: 0x000163B3
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			public Guid ANALYSIS_UID
			{
				get
				{
					return (Guid)base[this.tableAnalysisRemainingCapacityByRole.ANALYSIS_UIDColumn];
				}
				set
				{
					base[this.tableAnalysisRemainingCapacityByRole.ANALYSIS_UIDColumn] = value;
				}
			}

			// Token: 0x170001F8 RID: 504
			// (get) Token: 0x060006EB RID: 1771 RVA: 0x000181CC File Offset: 0x000163CC
			// (set) Token: 0x060006EC RID: 1772 RVA: 0x000181E4 File Offset: 0x000163E4
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			public Guid CUSTOM_FIELD_UID
			{
				get
				{
					return (Guid)base[this.tableAnalysisRemainingCapacityByRole.CUSTOM_FIELD_UIDColumn];
				}
				set
				{
					base[this.tableAnalysisRemainingCapacityByRole.CUSTOM_FIELD_UIDColumn] = value;
				}
			}

			// Token: 0x170001F9 RID: 505
			// (get) Token: 0x060006ED RID: 1773 RVA: 0x000181FD File Offset: 0x000163FD
			// (set) Token: 0x060006EE RID: 1774 RVA: 0x00018215 File Offset: 0x00016415
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public Guid LT_STRUCT_UID
			{
				get
				{
					return (Guid)base[this.tableAnalysisRemainingCapacityByRole.LT_STRUCT_UIDColumn];
				}
				set
				{
					base[this.tableAnalysisRemainingCapacityByRole.LT_STRUCT_UIDColumn] = value;
				}
			}

			// Token: 0x170001FA RID: 506
			// (get) Token: 0x060006EF RID: 1775 RVA: 0x0001822E File Offset: 0x0001642E
			// (set) Token: 0x060006F0 RID: 1776 RVA: 0x00018246 File Offset: 0x00016446
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			public DateTime START_DATE
			{
				get
				{
					return (DateTime)base[this.tableAnalysisRemainingCapacityByRole.START_DATEColumn];
				}
				set
				{
					base[this.tableAnalysisRemainingCapacityByRole.START_DATEColumn] = value;
				}
			}

			// Token: 0x170001FB RID: 507
			// (get) Token: 0x060006F1 RID: 1777 RVA: 0x0001825F File Offset: 0x0001645F
			// (set) Token: 0x060006F2 RID: 1778 RVA: 0x00018277 File Offset: 0x00016477
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			public DateTime END_DATE
			{
				get
				{
					return (DateTime)base[this.tableAnalysisRemainingCapacityByRole.END_DATEColumn];
				}
				set
				{
					base[this.tableAnalysisRemainingCapacityByRole.END_DATEColumn] = value;
				}
			}

			// Token: 0x170001FC RID: 508
			// (get) Token: 0x060006F3 RID: 1779 RVA: 0x00018290 File Offset: 0x00016490
			// (set) Token: 0x060006F4 RID: 1780 RVA: 0x000182A8 File Offset: 0x000164A8
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			public decimal REM_CAPACITY
			{
				get
				{
					return (decimal)base[this.tableAnalysisRemainingCapacityByRole.REM_CAPACITYColumn];
				}
				set
				{
					base[this.tableAnalysisRemainingCapacityByRole.REM_CAPACITYColumn] = value;
				}
			}

			// Token: 0x170001FD RID: 509
			// (get) Token: 0x060006F5 RID: 1781 RVA: 0x000182C1 File Offset: 0x000164C1
			// (set) Token: 0x060006F6 RID: 1782 RVA: 0x000182D9 File Offset: 0x000164D9
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public Guid REM_CAPACITY_UID
			{
				get
				{
					return (Guid)base[this.tableAnalysisRemainingCapacityByRole.REM_CAPACITY_UIDColumn];
				}
				set
				{
					base[this.tableAnalysisRemainingCapacityByRole.REM_CAPACITY_UIDColumn] = value;
				}
			}

			// Token: 0x040001BF RID: 447
			private AnalysisDataSet.AnalysisRemainingCapacityByRoleDataTable tableAnalysisRemainingCapacityByRole;
		}

		// Token: 0x0200005D RID: 93
		public class AnalysisRoleRatesRow : DataRow
		{
			// Token: 0x060006F7 RID: 1783 RVA: 0x000182F2 File Offset: 0x000164F2
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			internal AnalysisRoleRatesRow(DataRowBuilder rb) : base(rb)
			{
				this.tableAnalysisRoleRates = (AnalysisDataSet.AnalysisRoleRatesDataTable)base.Table;
			}

			// Token: 0x170001FE RID: 510
			// (get) Token: 0x060006F8 RID: 1784 RVA: 0x0001830C File Offset: 0x0001650C
			// (set) Token: 0x060006F9 RID: 1785 RVA: 0x00018324 File Offset: 0x00016524
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public Guid ANALYSIS_UID
			{
				get
				{
					return (Guid)base[this.tableAnalysisRoleRates.ANALYSIS_UIDColumn];
				}
				set
				{
					base[this.tableAnalysisRoleRates.ANALYSIS_UIDColumn] = value;
				}
			}

			// Token: 0x170001FF RID: 511
			// (get) Token: 0x060006FA RID: 1786 RVA: 0x0001833D File Offset: 0x0001653D
			// (set) Token: 0x060006FB RID: 1787 RVA: 0x00018355 File Offset: 0x00016555
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public Guid CUSTOM_FIELD_UID
			{
				get
				{
					return (Guid)base[this.tableAnalysisRoleRates.CUSTOM_FIELD_UIDColumn];
				}
				set
				{
					base[this.tableAnalysisRoleRates.CUSTOM_FIELD_UIDColumn] = value;
				}
			}

			// Token: 0x17000200 RID: 512
			// (get) Token: 0x060006FC RID: 1788 RVA: 0x0001836E File Offset: 0x0001656E
			// (set) Token: 0x060006FD RID: 1789 RVA: 0x00018386 File Offset: 0x00016586
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			public Guid LT_STRUCT_UID
			{
				get
				{
					return (Guid)base[this.tableAnalysisRoleRates.LT_STRUCT_UIDColumn];
				}
				set
				{
					base[this.tableAnalysisRoleRates.LT_STRUCT_UIDColumn] = value;
				}
			}

			// Token: 0x17000201 RID: 513
			// (get) Token: 0x060006FE RID: 1790 RVA: 0x0001839F File Offset: 0x0001659F
			// (set) Token: 0x060006FF RID: 1791 RVA: 0x000183B7 File Offset: 0x000165B7
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			public double STANDARD_RATE
			{
				get
				{
					return (double)base[this.tableAnalysisRoleRates.STANDARD_RATEColumn];
				}
				set
				{
					base[this.tableAnalysisRoleRates.STANDARD_RATEColumn] = value;
				}
			}

			// Token: 0x17000202 RID: 514
			// (get) Token: 0x06000700 RID: 1792 RVA: 0x000183D0 File Offset: 0x000165D0
			// (set) Token: 0x06000701 RID: 1793 RVA: 0x000183E8 File Offset: 0x000165E8
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public byte RATE_TABLE
			{
				get
				{
					return (byte)base[this.tableAnalysisRoleRates.RATE_TABLEColumn];
				}
				set
				{
					base[this.tableAnalysisRoleRates.RATE_TABLEColumn] = value;
				}
			}

			// Token: 0x040001C0 RID: 448
			private AnalysisDataSet.AnalysisRoleRatesDataTable tableAnalysisRoleRates;
		}

		// Token: 0x0200005E RID: 94
		public class AnalysisProjectRequirementsByRoleRow : DataRow
		{
			// Token: 0x06000702 RID: 1794 RVA: 0x00018401 File Offset: 0x00016601
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			internal AnalysisProjectRequirementsByRoleRow(DataRowBuilder rb) : base(rb)
			{
				this.tableAnalysisProjectRequirementsByRole = (AnalysisDataSet.AnalysisProjectRequirementsByRoleDataTable)base.Table;
			}

			// Token: 0x17000203 RID: 515
			// (get) Token: 0x06000703 RID: 1795 RVA: 0x0001841B File Offset: 0x0001661B
			// (set) Token: 0x06000704 RID: 1796 RVA: 0x00018433 File Offset: 0x00016633
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public Guid ANALYSIS_UID
			{
				get
				{
					return (Guid)base[this.tableAnalysisProjectRequirementsByRole.ANALYSIS_UIDColumn];
				}
				set
				{
					base[this.tableAnalysisProjectRequirementsByRole.ANALYSIS_UIDColumn] = value;
				}
			}

			// Token: 0x17000204 RID: 516
			// (get) Token: 0x06000705 RID: 1797 RVA: 0x0001844C File Offset: 0x0001664C
			// (set) Token: 0x06000706 RID: 1798 RVA: 0x00018464 File Offset: 0x00016664
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			public Guid PROJ_UID
			{
				get
				{
					return (Guid)base[this.tableAnalysisProjectRequirementsByRole.PROJ_UIDColumn];
				}
				set
				{
					base[this.tableAnalysisProjectRequirementsByRole.PROJ_UIDColumn] = value;
				}
			}

			// Token: 0x17000205 RID: 517
			// (get) Token: 0x06000707 RID: 1799 RVA: 0x0001847D File Offset: 0x0001667D
			// (set) Token: 0x06000708 RID: 1800 RVA: 0x00018495 File Offset: 0x00016695
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			public Guid CUSTOM_FIELD_UID
			{
				get
				{
					return (Guid)base[this.tableAnalysisProjectRequirementsByRole.CUSTOM_FIELD_UIDColumn];
				}
				set
				{
					base[this.tableAnalysisProjectRequirementsByRole.CUSTOM_FIELD_UIDColumn] = value;
				}
			}

			// Token: 0x17000206 RID: 518
			// (get) Token: 0x06000709 RID: 1801 RVA: 0x000184AE File Offset: 0x000166AE
			// (set) Token: 0x0600070A RID: 1802 RVA: 0x000184C6 File Offset: 0x000166C6
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			public Guid LT_STRUCT_UID
			{
				get
				{
					return (Guid)base[this.tableAnalysisProjectRequirementsByRole.LT_STRUCT_UIDColumn];
				}
				set
				{
					base[this.tableAnalysisProjectRequirementsByRole.LT_STRUCT_UIDColumn] = value;
				}
			}

			// Token: 0x17000207 RID: 519
			// (get) Token: 0x0600070B RID: 1803 RVA: 0x000184DF File Offset: 0x000166DF
			// (set) Token: 0x0600070C RID: 1804 RVA: 0x000184F7 File Offset: 0x000166F7
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			public DateTime START_DATE
			{
				get
				{
					return (DateTime)base[this.tableAnalysisProjectRequirementsByRole.START_DATEColumn];
				}
				set
				{
					base[this.tableAnalysisProjectRequirementsByRole.START_DATEColumn] = value;
				}
			}

			// Token: 0x17000208 RID: 520
			// (get) Token: 0x0600070D RID: 1805 RVA: 0x00018510 File Offset: 0x00016710
			// (set) Token: 0x0600070E RID: 1806 RVA: 0x00018528 File Offset: 0x00016728
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public decimal PROJECT_REQUIREMENT
			{
				get
				{
					return (decimal)base[this.tableAnalysisProjectRequirementsByRole.PROJECT_REQUIREMENTColumn];
				}
				set
				{
					base[this.tableAnalysisProjectRequirementsByRole.PROJECT_REQUIREMENTColumn] = value;
				}
			}

			// Token: 0x17000209 RID: 521
			// (get) Token: 0x0600070F RID: 1807 RVA: 0x00018541 File Offset: 0x00016741
			// (set) Token: 0x06000710 RID: 1808 RVA: 0x00018559 File Offset: 0x00016759
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public Guid REQUIREMENT_UID
			{
				get
				{
					return (Guid)base[this.tableAnalysisProjectRequirementsByRole.REQUIREMENT_UIDColumn];
				}
				set
				{
					base[this.tableAnalysisProjectRequirementsByRole.REQUIREMENT_UIDColumn] = value;
				}
			}

			// Token: 0x040001C1 RID: 449
			private AnalysisDataSet.AnalysisProjectRequirementsByRoleDataTable tableAnalysisProjectRequirementsByRole;
		}

		// Token: 0x0200005F RID: 95
		[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
		public class AnalysisRowChangeEvent : EventArgs
		{
			// Token: 0x06000711 RID: 1809 RVA: 0x00018572 File Offset: 0x00016772
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			public AnalysisRowChangeEvent(AnalysisDataSet.AnalysisRow row, DataRowAction action)
			{
				this.eventRow = row;
				this.eventAction = action;
			}

			// Token: 0x1700020A RID: 522
			// (get) Token: 0x06000712 RID: 1810 RVA: 0x00018588 File Offset: 0x00016788
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			public AnalysisDataSet.AnalysisRow Row
			{
				get
				{
					return this.eventRow;
				}
			}

			// Token: 0x1700020B RID: 523
			// (get) Token: 0x06000713 RID: 1811 RVA: 0x00018590 File Offset: 0x00016790
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			public DataRowAction Action
			{
				get
				{
					return this.eventAction;
				}
			}

			// Token: 0x040001C2 RID: 450
			private AnalysisDataSet.AnalysisRow eventRow;

			// Token: 0x040001C3 RID: 451
			private DataRowAction eventAction;
		}

		// Token: 0x02000060 RID: 96
		[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
		public class AnalysisProjectsRowChangeEvent : EventArgs
		{
			// Token: 0x06000714 RID: 1812 RVA: 0x00018598 File Offset: 0x00016798
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public AnalysisProjectsRowChangeEvent(AnalysisDataSet.AnalysisProjectsRow row, DataRowAction action)
			{
				this.eventRow = row;
				this.eventAction = action;
			}

			// Token: 0x1700020C RID: 524
			// (get) Token: 0x06000715 RID: 1813 RVA: 0x000185AE File Offset: 0x000167AE
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public AnalysisDataSet.AnalysisProjectsRow Row
			{
				get
				{
					return this.eventRow;
				}
			}

			// Token: 0x1700020D RID: 525
			// (get) Token: 0x06000716 RID: 1814 RVA: 0x000185B6 File Offset: 0x000167B6
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			public DataRowAction Action
			{
				get
				{
					return this.eventAction;
				}
			}

			// Token: 0x040001C4 RID: 452
			private AnalysisDataSet.AnalysisProjectsRow eventRow;

			// Token: 0x040001C5 RID: 453
			private DataRowAction eventAction;
		}

		// Token: 0x02000061 RID: 97
		[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
		public class AnalysisPriorityDataRowChangeEvent : EventArgs
		{
			// Token: 0x06000717 RID: 1815 RVA: 0x000185BE File Offset: 0x000167BE
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			public AnalysisPriorityDataRowChangeEvent(AnalysisDataSet.AnalysisPriorityDataRow row, DataRowAction action)
			{
				this.eventRow = row;
				this.eventAction = action;
			}

			// Token: 0x1700020E RID: 526
			// (get) Token: 0x06000718 RID: 1816 RVA: 0x000185D4 File Offset: 0x000167D4
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			public AnalysisDataSet.AnalysisPriorityDataRow Row
			{
				get
				{
					return this.eventRow;
				}
			}

			// Token: 0x1700020F RID: 527
			// (get) Token: 0x06000719 RID: 1817 RVA: 0x000185DC File Offset: 0x000167DC
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			public DataRowAction Action
			{
				get
				{
					return this.eventAction;
				}
			}

			// Token: 0x040001C6 RID: 454
			private AnalysisDataSet.AnalysisPriorityDataRow eventRow;

			// Token: 0x040001C7 RID: 455
			private DataRowAction eventAction;
		}

		// Token: 0x02000062 RID: 98
		[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
		public class AnalysisProjectImpactRowChangeEvent : EventArgs
		{
			// Token: 0x0600071A RID: 1818 RVA: 0x000185E4 File Offset: 0x000167E4
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			public AnalysisProjectImpactRowChangeEvent(AnalysisDataSet.AnalysisProjectImpactRow row, DataRowAction action)
			{
				this.eventRow = row;
				this.eventAction = action;
			}

			// Token: 0x17000210 RID: 528
			// (get) Token: 0x0600071B RID: 1819 RVA: 0x000185FA File Offset: 0x000167FA
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public AnalysisDataSet.AnalysisProjectImpactRow Row
			{
				get
				{
					return this.eventRow;
				}
			}

			// Token: 0x17000211 RID: 529
			// (get) Token: 0x0600071C RID: 1820 RVA: 0x00018602 File Offset: 0x00016802
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			public DataRowAction Action
			{
				get
				{
					return this.eventAction;
				}
			}

			// Token: 0x040001C8 RID: 456
			private AnalysisDataSet.AnalysisProjectImpactRow eventRow;

			// Token: 0x040001C9 RID: 457
			private DataRowAction eventAction;
		}

		// Token: 0x02000063 RID: 99
		[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
		public class AnalysisOptimizerSolutionsRowChangeEvent : EventArgs
		{
			// Token: 0x0600071D RID: 1821 RVA: 0x0001860A File Offset: 0x0001680A
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public AnalysisOptimizerSolutionsRowChangeEvent(AnalysisDataSet.AnalysisOptimizerSolutionsRow row, DataRowAction action)
			{
				this.eventRow = row;
				this.eventAction = action;
			}

			// Token: 0x17000212 RID: 530
			// (get) Token: 0x0600071E RID: 1822 RVA: 0x00018620 File Offset: 0x00016820
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public AnalysisDataSet.AnalysisOptimizerSolutionsRow Row
			{
				get
				{
					return this.eventRow;
				}
			}

			// Token: 0x17000213 RID: 531
			// (get) Token: 0x0600071F RID: 1823 RVA: 0x00018628 File Offset: 0x00016828
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			public DataRowAction Action
			{
				get
				{
					return this.eventAction;
				}
			}

			// Token: 0x040001CA RID: 458
			private AnalysisDataSet.AnalysisOptimizerSolutionsRow eventRow;

			// Token: 0x040001CB RID: 459
			private DataRowAction eventAction;
		}

		// Token: 0x02000064 RID: 100
		[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
		public class AnalysisPlannerSolutionsRowChangeEvent : EventArgs
		{
			// Token: 0x06000720 RID: 1824 RVA: 0x00018630 File Offset: 0x00016830
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			public AnalysisPlannerSolutionsRowChangeEvent(AnalysisDataSet.AnalysisPlannerSolutionsRow row, DataRowAction action)
			{
				this.eventRow = row;
				this.eventAction = action;
			}

			// Token: 0x17000214 RID: 532
			// (get) Token: 0x06000721 RID: 1825 RVA: 0x00018646 File Offset: 0x00016846
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public AnalysisDataSet.AnalysisPlannerSolutionsRow Row
			{
				get
				{
					return this.eventRow;
				}
			}

			// Token: 0x17000215 RID: 533
			// (get) Token: 0x06000722 RID: 1826 RVA: 0x0001864E File Offset: 0x0001684E
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			public DataRowAction Action
			{
				get
				{
					return this.eventAction;
				}
			}

			// Token: 0x040001CC RID: 460
			private AnalysisDataSet.AnalysisPlannerSolutionsRow eventRow;

			// Token: 0x040001CD RID: 461
			private DataRowAction eventAction;
		}

		// Token: 0x02000065 RID: 101
		[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
		public class AnalysisRemainingCapacityByRoleRowChangeEvent : EventArgs
		{
			// Token: 0x06000723 RID: 1827 RVA: 0x00018656 File Offset: 0x00016856
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			public AnalysisRemainingCapacityByRoleRowChangeEvent(AnalysisDataSet.AnalysisRemainingCapacityByRoleRow row, DataRowAction action)
			{
				this.eventRow = row;
				this.eventAction = action;
			}

			// Token: 0x17000216 RID: 534
			// (get) Token: 0x06000724 RID: 1828 RVA: 0x0001866C File Offset: 0x0001686C
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public AnalysisDataSet.AnalysisRemainingCapacityByRoleRow Row
			{
				get
				{
					return this.eventRow;
				}
			}

			// Token: 0x17000217 RID: 535
			// (get) Token: 0x06000725 RID: 1829 RVA: 0x00018674 File Offset: 0x00016874
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			public DataRowAction Action
			{
				get
				{
					return this.eventAction;
				}
			}

			// Token: 0x040001CE RID: 462
			private AnalysisDataSet.AnalysisRemainingCapacityByRoleRow eventRow;

			// Token: 0x040001CF RID: 463
			private DataRowAction eventAction;
		}

		// Token: 0x02000066 RID: 102
		[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
		public class AnalysisRoleRatesRowChangeEvent : EventArgs
		{
			// Token: 0x06000726 RID: 1830 RVA: 0x0001867C File Offset: 0x0001687C
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			public AnalysisRoleRatesRowChangeEvent(AnalysisDataSet.AnalysisRoleRatesRow row, DataRowAction action)
			{
				this.eventRow = row;
				this.eventAction = action;
			}

			// Token: 0x17000218 RID: 536
			// (get) Token: 0x06000727 RID: 1831 RVA: 0x00018692 File Offset: 0x00016892
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public AnalysisDataSet.AnalysisRoleRatesRow Row
			{
				get
				{
					return this.eventRow;
				}
			}

			// Token: 0x17000219 RID: 537
			// (get) Token: 0x06000728 RID: 1832 RVA: 0x0001869A File Offset: 0x0001689A
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			public DataRowAction Action
			{
				get
				{
					return this.eventAction;
				}
			}

			// Token: 0x040001D0 RID: 464
			private AnalysisDataSet.AnalysisRoleRatesRow eventRow;

			// Token: 0x040001D1 RID: 465
			private DataRowAction eventAction;
		}

		// Token: 0x02000067 RID: 103
		[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
		public class AnalysisProjectRequirementsByRoleRowChangeEvent : EventArgs
		{
			// Token: 0x06000729 RID: 1833 RVA: 0x000186A2 File Offset: 0x000168A2
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			public AnalysisProjectRequirementsByRoleRowChangeEvent(AnalysisDataSet.AnalysisProjectRequirementsByRoleRow row, DataRowAction action)
			{
				this.eventRow = row;
				this.eventAction = action;
			}

			// Token: 0x1700021A RID: 538
			// (get) Token: 0x0600072A RID: 1834 RVA: 0x000186B8 File Offset: 0x000168B8
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			public AnalysisDataSet.AnalysisProjectRequirementsByRoleRow Row
			{
				get
				{
					return this.eventRow;
				}
			}

			// Token: 0x1700021B RID: 539
			// (get) Token: 0x0600072B RID: 1835 RVA: 0x000186C0 File Offset: 0x000168C0
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public DataRowAction Action
			{
				get
				{
					return this.eventAction;
				}
			}

			// Token: 0x040001D2 RID: 466
			private AnalysisDataSet.AnalysisProjectRequirementsByRoleRow eventRow;

			// Token: 0x040001D3 RID: 467
			private DataRowAction eventAction;
		}
	}
}
