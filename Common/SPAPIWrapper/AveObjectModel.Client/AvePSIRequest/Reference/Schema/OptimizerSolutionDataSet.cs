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
	// Token: 0x02000226 RID: 550
	[HelpKeyword("vs.data.DataSet")]
	[XmlRoot("OptimizerSolutionDataSet")]
	[DesignerCategory("code")]
	[XmlSchemaProvider("GetTypedDataSetSchema")]
	[ToolboxItem(true)]
	[Serializable]
	public class OptimizerSolutionDataSet : DataSet
	{
		// Token: 0x06002C8D RID: 11405 RVA: 0x0008E7C0 File Offset: 0x0008C9C0
		public new void EndInit()
		{
			base.EndInit();
			TypedDataSetUtilities.AllowNullsInNonTypedColumns(this.SolutionStrategicAlignment, new string[]
			{
				"DRIVER_UID",
				"REVERSE_VALUE",
				"DRIVER_PRIORITY",
				"DRIVER_NAME",
				"SOLUTION_UID"
			});
			TypedDataSetUtilities.AllowNullsInNonTypedColumns(this.SolutionConstraintValues, new string[]
			{
				"NUM_VALUE",
				"MD_PROP_UID",
				"PROJ_UID",
				"SOLUTION_UID"
			});
			TypedDataSetUtilities.AllowNullsInNonTypedColumns(this.Solution, new string[]
			{
				"ANALYSIS_NAME",
				"CREATED_DATE",
				"TOTAL_HARD_CONSTRAINT_VALUE",
				"CREATED_BY_RES_UID",
				"SOLUTION_UID",
				"LAST_UPDATED_BY_RES_NAME",
				"ANALYSIS_UID",
				"LAST_UPDATED_BY_RES_UID",
				"TOTAL_PRIORITY_VALUE",
				"SOLUTION_DESCRIPTION",
				"FRONTIER_UID",
				"HARD_CONSTRAINT_CF_NAME",
				"HARD_CONSTRAINT_CF_UID",
				"SOLUTION_NAME",
				"MOD_DATE",
				"OPT_USE_DEPENDENCIES",
				"CREATED_BY_RES_NAME"
			});
			TypedDataSetUtilities.AllowNullsInNonTypedColumns(this.SolutionProjects, new string[]
			{
				"PROJ_NAME",
				"FORCE_ALIAS_LT_VALUE_FULL",
				"STATUS",
				"SOLUTION_UID",
				"FORCE_ALIAS_LT_STRUCT_UID",
				"FORCE_STATUS",
				"ABSOLUTE_PRIORITY",
				"PRIORITY",
				"PROJ_UID"
			});
			TypedDataSetUtilities.AllowNullsInNonTypedColumns(this.SolutionEfficientFrontier, new string[]
			{
				"POINT_UID",
				"ANALYSIS_UID",
				"Y_VALUE",
				"X_VALUE",
				"FRONTIER_UID"
			});
			TypedDataSetUtilities.AllowNullsInNonTypedColumns(this.SolutionConstraints, new string[]
			{
				"MD_PROP_NAME",
				"MD_PROP_UID",
				"MAX_VALUE",
				"SOLUTION_UID",
				"MD_PROP_POS"
			});
		}

		// Token: 0x06002C8E RID: 11406 RVA: 0x0008E9C8 File Offset: 0x0008CBC8
		[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
		[DebuggerNonUserCode]
		public OptimizerSolutionDataSet()
		{
			base.BeginInit();
			this.InitClass();
			CollectionChangeEventHandler value = new CollectionChangeEventHandler(this.SchemaChanged);
			base.Tables.CollectionChanged += value;
			base.Relations.CollectionChanged += value;
			this.EndInit();
		}

		// Token: 0x06002C8F RID: 11407 RVA: 0x0008EA1C File Offset: 0x0008CC1C
		[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
		[DebuggerNonUserCode]
		protected OptimizerSolutionDataSet(SerializationInfo info, StreamingContext context) : base(info, context, false)
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
				if (dataSet.Tables["Solution"] != null)
				{
					base.Tables.Add(new OptimizerSolutionDataSet.SolutionDataTable(dataSet.Tables["Solution"]));
				}
				if (dataSet.Tables["SolutionConstraints"] != null)
				{
					base.Tables.Add(new OptimizerSolutionDataSet.SolutionConstraintsDataTable(dataSet.Tables["SolutionConstraints"]));
				}
				if (dataSet.Tables["SolutionConstraintValues"] != null)
				{
					base.Tables.Add(new OptimizerSolutionDataSet.SolutionConstraintValuesDataTable(dataSet.Tables["SolutionConstraintValues"]));
				}
				if (dataSet.Tables["SolutionProjects"] != null)
				{
					base.Tables.Add(new OptimizerSolutionDataSet.SolutionProjectsDataTable(dataSet.Tables["SolutionProjects"]));
				}
				if (dataSet.Tables["SolutionStrategicAlignment"] != null)
				{
					base.Tables.Add(new OptimizerSolutionDataSet.SolutionStrategicAlignmentDataTable(dataSet.Tables["SolutionStrategicAlignment"]));
				}
				if (dataSet.Tables["SolutionEfficientFrontier"] != null)
				{
					base.Tables.Add(new OptimizerSolutionDataSet.SolutionEfficientFrontierDataTable(dataSet.Tables["SolutionEfficientFrontier"]));
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

		// Token: 0x17000D18 RID: 3352
		// (get) Token: 0x06002C90 RID: 11408 RVA: 0x0008EC73 File Offset: 0x0008CE73
		[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
		[DebuggerNonUserCode]
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Content)]
		[Browsable(false)]
		public OptimizerSolutionDataSet.SolutionDataTable Solution
		{
			get
			{
				return this.tableSolution;
			}
		}

		// Token: 0x17000D19 RID: 3353
		// (get) Token: 0x06002C91 RID: 11409 RVA: 0x0008EC7B File Offset: 0x0008CE7B
		[DebuggerNonUserCode]
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Content)]
		[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
		[Browsable(false)]
		public OptimizerSolutionDataSet.SolutionConstraintsDataTable SolutionConstraints
		{
			get
			{
				return this.tableSolutionConstraints;
			}
		}

		// Token: 0x17000D1A RID: 3354
		// (get) Token: 0x06002C92 RID: 11410 RVA: 0x0008EC83 File Offset: 0x0008CE83
		[DebuggerNonUserCode]
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Content)]
		[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
		[Browsable(false)]
		public OptimizerSolutionDataSet.SolutionConstraintValuesDataTable SolutionConstraintValues
		{
			get
			{
				return this.tableSolutionConstraintValues;
			}
		}

		// Token: 0x17000D1B RID: 3355
		// (get) Token: 0x06002C93 RID: 11411 RVA: 0x0008EC8B File Offset: 0x0008CE8B
		[Browsable(false)]
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Content)]
		[DebuggerNonUserCode]
		[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
		public OptimizerSolutionDataSet.SolutionProjectsDataTable SolutionProjects
		{
			get
			{
				return this.tableSolutionProjects;
			}
		}

		// Token: 0x17000D1C RID: 3356
		// (get) Token: 0x06002C94 RID: 11412 RVA: 0x0008EC93 File Offset: 0x0008CE93
		[DebuggerNonUserCode]
		[Browsable(false)]
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Content)]
		[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
		public OptimizerSolutionDataSet.SolutionStrategicAlignmentDataTable SolutionStrategicAlignment
		{
			get
			{
				return this.tableSolutionStrategicAlignment;
			}
		}

		// Token: 0x17000D1D RID: 3357
		// (get) Token: 0x06002C95 RID: 11413 RVA: 0x0008EC9B File Offset: 0x0008CE9B
		[DebuggerNonUserCode]
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Content)]
		[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
		[Browsable(false)]
		public OptimizerSolutionDataSet.SolutionEfficientFrontierDataTable SolutionEfficientFrontier
		{
			get
			{
				return this.tableSolutionEfficientFrontier;
			}
		}

		// Token: 0x17000D1E RID: 3358
		// (get) Token: 0x06002C96 RID: 11414 RVA: 0x0008ECA3 File Offset: 0x0008CEA3
		// (set) Token: 0x06002C97 RID: 11415 RVA: 0x0008ECAB File Offset: 0x0008CEAB
		[DebuggerNonUserCode]
		[Browsable(true)]
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Visible)]
		[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
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

		// Token: 0x17000D1F RID: 3359
		// (get) Token: 0x06002C98 RID: 11416 RVA: 0x0008ECB4 File Offset: 0x0008CEB4
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

		// Token: 0x17000D20 RID: 3360
		// (get) Token: 0x06002C99 RID: 11417 RVA: 0x0008ECBC File Offset: 0x0008CEBC
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

		// Token: 0x06002C9A RID: 11418 RVA: 0x0008ECC4 File Offset: 0x0008CEC4
		[DebuggerNonUserCode]
		[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
		protected override void InitializeDerivedDataSet()
		{
			base.BeginInit();
			this.InitClass();
			this.EndInit();
		}

		// Token: 0x06002C9B RID: 11419 RVA: 0x0008ECD8 File Offset: 0x0008CED8
		[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
		[DebuggerNonUserCode]
		public override DataSet Clone()
		{
			OptimizerSolutionDataSet optimizerSolutionDataSet = (OptimizerSolutionDataSet)base.Clone();
			optimizerSolutionDataSet.InitVars();
			optimizerSolutionDataSet.SchemaSerializationMode = this.SchemaSerializationMode;
			return optimizerSolutionDataSet;
		}

		// Token: 0x06002C9C RID: 11420 RVA: 0x0008ED04 File Offset: 0x0008CF04
		[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
		[DebuggerNonUserCode]
		protected override bool ShouldSerializeTables()
		{
			return false;
		}

		// Token: 0x06002C9D RID: 11421 RVA: 0x0008ED07 File Offset: 0x0008CF07
		[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
		[DebuggerNonUserCode]
		protected override bool ShouldSerializeRelations()
		{
			return false;
		}

		// Token: 0x06002C9E RID: 11422 RVA: 0x0008ED0C File Offset: 0x0008CF0C
		[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
		[DebuggerNonUserCode]
		protected override void ReadXmlSerializable(XmlReader reader)
		{
			if (base.DetermineSchemaSerializationMode(reader) == SchemaSerializationMode.IncludeSchema)
			{
				this.Reset();
				DataSet dataSet = new DataSet();
				dataSet.ReadXml(reader);
				if (dataSet.Tables["Solution"] != null)
				{
					base.Tables.Add(new OptimizerSolutionDataSet.SolutionDataTable(dataSet.Tables["Solution"]));
				}
				if (dataSet.Tables["SolutionConstraints"] != null)
				{
					base.Tables.Add(new OptimizerSolutionDataSet.SolutionConstraintsDataTable(dataSet.Tables["SolutionConstraints"]));
				}
				if (dataSet.Tables["SolutionConstraintValues"] != null)
				{
					base.Tables.Add(new OptimizerSolutionDataSet.SolutionConstraintValuesDataTable(dataSet.Tables["SolutionConstraintValues"]));
				}
				if (dataSet.Tables["SolutionProjects"] != null)
				{
					base.Tables.Add(new OptimizerSolutionDataSet.SolutionProjectsDataTable(dataSet.Tables["SolutionProjects"]));
				}
				if (dataSet.Tables["SolutionStrategicAlignment"] != null)
				{
					base.Tables.Add(new OptimizerSolutionDataSet.SolutionStrategicAlignmentDataTable(dataSet.Tables["SolutionStrategicAlignment"]));
				}
				if (dataSet.Tables["SolutionEfficientFrontier"] != null)
				{
					base.Tables.Add(new OptimizerSolutionDataSet.SolutionEfficientFrontierDataTable(dataSet.Tables["SolutionEfficientFrontier"]));
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

		// Token: 0x06002C9F RID: 11423 RVA: 0x0008EECC File Offset: 0x0008D0CC
		[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
		[DebuggerNonUserCode]
		protected override XmlSchema GetSchemaSerializable()
		{
			MemoryStream memoryStream = new MemoryStream();
			base.WriteXmlSchema(new XmlTextWriter(memoryStream, null));
			memoryStream.Position = 0L;
			return XmlSchema.Read(new XmlTextReader(memoryStream), null);
		}

		// Token: 0x06002CA0 RID: 11424 RVA: 0x0008EF00 File Offset: 0x0008D100
		[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
		[DebuggerNonUserCode]
		internal void InitVars()
		{
			this.InitVars(true);
		}

		// Token: 0x06002CA1 RID: 11425 RVA: 0x0008EF0C File Offset: 0x0008D10C
		[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
		[DebuggerNonUserCode]
		internal void InitVars(bool initTable)
		{
			this.tableSolution = (OptimizerSolutionDataSet.SolutionDataTable)base.Tables["Solution"];
			if (initTable && this.tableSolution != null)
			{
				this.tableSolution.InitVars();
			}
			this.tableSolutionConstraints = (OptimizerSolutionDataSet.SolutionConstraintsDataTable)base.Tables["SolutionConstraints"];
			if (initTable && this.tableSolutionConstraints != null)
			{
				this.tableSolutionConstraints.InitVars();
			}
			this.tableSolutionConstraintValues = (OptimizerSolutionDataSet.SolutionConstraintValuesDataTable)base.Tables["SolutionConstraintValues"];
			if (initTable && this.tableSolutionConstraintValues != null)
			{
				this.tableSolutionConstraintValues.InitVars();
			}
			this.tableSolutionProjects = (OptimizerSolutionDataSet.SolutionProjectsDataTable)base.Tables["SolutionProjects"];
			if (initTable && this.tableSolutionProjects != null)
			{
				this.tableSolutionProjects.InitVars();
			}
			this.tableSolutionStrategicAlignment = (OptimizerSolutionDataSet.SolutionStrategicAlignmentDataTable)base.Tables["SolutionStrategicAlignment"];
			if (initTable && this.tableSolutionStrategicAlignment != null)
			{
				this.tableSolutionStrategicAlignment.InitVars();
			}
			this.tableSolutionEfficientFrontier = (OptimizerSolutionDataSet.SolutionEfficientFrontierDataTable)base.Tables["SolutionEfficientFrontier"];
			if (initTable && this.tableSolutionEfficientFrontier != null)
			{
				this.tableSolutionEfficientFrontier.InitVars();
			}
			this.relationFK_Solution_SolutionConstraints = this.Relations["FK_Solution_SolutionConstraints"];
			this.relationFK_SolutionConstraints_SolutionConstraintValues = this.Relations["FK_SolutionConstraints_SolutionConstraintValues"];
			this.relationFK_SolutionProjects_SolutionConstraintValues = this.Relations["FK_SolutionProjects_SolutionConstraintValues"];
			this.relationFK_Solution_SolutionProjects = this.Relations["FK_Solution_SolutionProjects"];
			this.relationFK_Solution_SolutionStrategicAlignment = this.Relations["FK_Solution_SolutionStrategicAlignment"];
		}

		// Token: 0x06002CA2 RID: 11426 RVA: 0x0008F0B0 File Offset: 0x0008D2B0
		[DebuggerNonUserCode]
		[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
		private void InitClass()
		{
			base.DataSetName = "OptimizerSolutionDataSet";
			base.Prefix = "";
			base.Namespace = "http://schemas.microsoft.com/office/project/server/webservices/OptimizerSolutionDataSet/";
			base.EnforceConstraints = true;
			this.SchemaSerializationMode = SchemaSerializationMode.IncludeSchema;
			this.tableSolution = new OptimizerSolutionDataSet.SolutionDataTable();
			base.Tables.Add(this.tableSolution);
			this.tableSolutionConstraints = new OptimizerSolutionDataSet.SolutionConstraintsDataTable();
			base.Tables.Add(this.tableSolutionConstraints);
			this.tableSolutionConstraintValues = new OptimizerSolutionDataSet.SolutionConstraintValuesDataTable();
			base.Tables.Add(this.tableSolutionConstraintValues);
			this.tableSolutionProjects = new OptimizerSolutionDataSet.SolutionProjectsDataTable();
			base.Tables.Add(this.tableSolutionProjects);
			this.tableSolutionStrategicAlignment = new OptimizerSolutionDataSet.SolutionStrategicAlignmentDataTable();
			base.Tables.Add(this.tableSolutionStrategicAlignment);
			this.tableSolutionEfficientFrontier = new OptimizerSolutionDataSet.SolutionEfficientFrontierDataTable();
			base.Tables.Add(this.tableSolutionEfficientFrontier);
			ForeignKeyConstraint foreignKeyConstraint = new ForeignKeyConstraint("FK_Solution_SolutionConstraints", new DataColumn[]
			{
				this.tableSolution.SOLUTION_UIDColumn
			}, new DataColumn[]
			{
				this.tableSolutionConstraints.SOLUTION_UIDColumn
			});
			this.tableSolutionConstraints.Constraints.Add(foreignKeyConstraint);
			foreignKeyConstraint.AcceptRejectRule = AcceptRejectRule.None;
			foreignKeyConstraint.DeleteRule = Rule.None;
			foreignKeyConstraint.UpdateRule = Rule.None;
			foreignKeyConstraint = new ForeignKeyConstraint("FK_SolutionConstraints_SolutionConstraintValues", new DataColumn[]
			{
				this.tableSolutionConstraints.SOLUTION_UIDColumn,
				this.tableSolutionConstraints.MD_PROP_UIDColumn
			}, new DataColumn[]
			{
				this.tableSolutionConstraintValues.SOLUTION_UIDColumn,
				this.tableSolutionConstraintValues.MD_PROP_UIDColumn
			});
			this.tableSolutionConstraintValues.Constraints.Add(foreignKeyConstraint);
			foreignKeyConstraint.AcceptRejectRule = AcceptRejectRule.None;
			foreignKeyConstraint.DeleteRule = Rule.None;
			foreignKeyConstraint.UpdateRule = Rule.None;
			foreignKeyConstraint = new ForeignKeyConstraint("FK_SolutionProjects_SolutionConstraintValues", new DataColumn[]
			{
				this.tableSolutionProjects.SOLUTION_UIDColumn,
				this.tableSolutionProjects.PROJ_UIDColumn
			}, new DataColumn[]
			{
				this.tableSolutionConstraintValues.SOLUTION_UIDColumn,
				this.tableSolutionConstraintValues.PROJ_UIDColumn
			});
			this.tableSolutionConstraintValues.Constraints.Add(foreignKeyConstraint);
			foreignKeyConstraint.AcceptRejectRule = AcceptRejectRule.None;
			foreignKeyConstraint.DeleteRule = Rule.None;
			foreignKeyConstraint.UpdateRule = Rule.None;
			foreignKeyConstraint = new ForeignKeyConstraint("FK_Solution_SolutionProjects", new DataColumn[]
			{
				this.tableSolution.SOLUTION_UIDColumn
			}, new DataColumn[]
			{
				this.tableSolutionProjects.SOLUTION_UIDColumn
			});
			this.tableSolutionProjects.Constraints.Add(foreignKeyConstraint);
			foreignKeyConstraint.AcceptRejectRule = AcceptRejectRule.None;
			foreignKeyConstraint.DeleteRule = Rule.None;
			foreignKeyConstraint.UpdateRule = Rule.None;
			foreignKeyConstraint = new ForeignKeyConstraint("FK_Solution_SolutionStrategicAlignment", new DataColumn[]
			{
				this.tableSolution.SOLUTION_UIDColumn
			}, new DataColumn[]
			{
				this.tableSolutionStrategicAlignment.SOLUTION_UIDColumn
			});
			this.tableSolutionStrategicAlignment.Constraints.Add(foreignKeyConstraint);
			foreignKeyConstraint.AcceptRejectRule = AcceptRejectRule.None;
			foreignKeyConstraint.DeleteRule = Rule.None;
			foreignKeyConstraint.UpdateRule = Rule.None;
			this.relationFK_Solution_SolutionConstraints = new DataRelation("FK_Solution_SolutionConstraints", new DataColumn[]
			{
				this.tableSolution.SOLUTION_UIDColumn
			}, new DataColumn[]
			{
				this.tableSolutionConstraints.SOLUTION_UIDColumn
			}, false);
			this.Relations.Add(this.relationFK_Solution_SolutionConstraints);
			this.relationFK_SolutionConstraints_SolutionConstraintValues = new DataRelation("FK_SolutionConstraints_SolutionConstraintValues", new DataColumn[]
			{
				this.tableSolutionConstraints.SOLUTION_UIDColumn,
				this.tableSolutionConstraints.MD_PROP_UIDColumn
			}, new DataColumn[]
			{
				this.tableSolutionConstraintValues.SOLUTION_UIDColumn,
				this.tableSolutionConstraintValues.MD_PROP_UIDColumn
			}, false);
			this.Relations.Add(this.relationFK_SolutionConstraints_SolutionConstraintValues);
			this.relationFK_SolutionProjects_SolutionConstraintValues = new DataRelation("FK_SolutionProjects_SolutionConstraintValues", new DataColumn[]
			{
				this.tableSolutionProjects.SOLUTION_UIDColumn,
				this.tableSolutionProjects.PROJ_UIDColumn
			}, new DataColumn[]
			{
				this.tableSolutionConstraintValues.SOLUTION_UIDColumn,
				this.tableSolutionConstraintValues.PROJ_UIDColumn
			}, false);
			this.Relations.Add(this.relationFK_SolutionProjects_SolutionConstraintValues);
			this.relationFK_Solution_SolutionProjects = new DataRelation("FK_Solution_SolutionProjects", new DataColumn[]
			{
				this.tableSolution.SOLUTION_UIDColumn
			}, new DataColumn[]
			{
				this.tableSolutionProjects.SOLUTION_UIDColumn
			}, false);
			this.Relations.Add(this.relationFK_Solution_SolutionProjects);
			this.relationFK_Solution_SolutionStrategicAlignment = new DataRelation("FK_Solution_SolutionStrategicAlignment", new DataColumn[]
			{
				this.tableSolution.SOLUTION_UIDColumn
			}, new DataColumn[]
			{
				this.tableSolutionStrategicAlignment.SOLUTION_UIDColumn
			}, false);
			this.Relations.Add(this.relationFK_Solution_SolutionStrategicAlignment);
		}

		// Token: 0x06002CA3 RID: 11427 RVA: 0x0008F595 File Offset: 0x0008D795
		[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
		[DebuggerNonUserCode]
		private bool ShouldSerializeSolution()
		{
			return false;
		}

		// Token: 0x06002CA4 RID: 11428 RVA: 0x0008F598 File Offset: 0x0008D798
		[DebuggerNonUserCode]
		[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
		private bool ShouldSerializeSolutionConstraints()
		{
			return false;
		}

		// Token: 0x06002CA5 RID: 11429 RVA: 0x0008F59B File Offset: 0x0008D79B
		[DebuggerNonUserCode]
		[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
		private bool ShouldSerializeSolutionConstraintValues()
		{
			return false;
		}

		// Token: 0x06002CA6 RID: 11430 RVA: 0x0008F59E File Offset: 0x0008D79E
		[DebuggerNonUserCode]
		[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
		private bool ShouldSerializeSolutionProjects()
		{
			return false;
		}

		// Token: 0x06002CA7 RID: 11431 RVA: 0x0008F5A1 File Offset: 0x0008D7A1
		[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
		[DebuggerNonUserCode]
		private bool ShouldSerializeSolutionStrategicAlignment()
		{
			return false;
		}

		// Token: 0x06002CA8 RID: 11432 RVA: 0x0008F5A4 File Offset: 0x0008D7A4
		[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
		[DebuggerNonUserCode]
		private bool ShouldSerializeSolutionEfficientFrontier()
		{
			return false;
		}

		// Token: 0x06002CA9 RID: 11433 RVA: 0x0008F5A7 File Offset: 0x0008D7A7
		[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
		[DebuggerNonUserCode]
		private void SchemaChanged(object sender, CollectionChangeEventArgs e)
		{
			if (e.Action == CollectionChangeAction.Remove)
			{
				this.InitVars();
			}
		}

		// Token: 0x06002CAA RID: 11434 RVA: 0x0008F5B8 File Offset: 0x0008D7B8
		[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
		[DebuggerNonUserCode]
		public static XmlSchemaComplexType GetTypedDataSetSchema(XmlSchemaSet xs)
		{
			OptimizerSolutionDataSet optimizerSolutionDataSet = new OptimizerSolutionDataSet();
			XmlSchemaComplexType xmlSchemaComplexType = new XmlSchemaComplexType();
			XmlSchemaSequence xmlSchemaSequence = new XmlSchemaSequence();
			XmlSchemaAny xmlSchemaAny = new XmlSchemaAny();
			xmlSchemaAny.Namespace = optimizerSolutionDataSet.Namespace;
			xmlSchemaSequence.Items.Add(xmlSchemaAny);
			xmlSchemaComplexType.Particle = xmlSchemaSequence;
			XmlSchema schemaSerializable = optimizerSolutionDataSet.GetSchemaSerializable();
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

		// Token: 0x0400095E RID: 2398
		private OptimizerSolutionDataSet.SolutionDataTable tableSolution;

		// Token: 0x0400095F RID: 2399
		private OptimizerSolutionDataSet.SolutionConstraintsDataTable tableSolutionConstraints;

		// Token: 0x04000960 RID: 2400
		private OptimizerSolutionDataSet.SolutionConstraintValuesDataTable tableSolutionConstraintValues;

		// Token: 0x04000961 RID: 2401
		private OptimizerSolutionDataSet.SolutionProjectsDataTable tableSolutionProjects;

		// Token: 0x04000962 RID: 2402
		private OptimizerSolutionDataSet.SolutionStrategicAlignmentDataTable tableSolutionStrategicAlignment;

		// Token: 0x04000963 RID: 2403
		private OptimizerSolutionDataSet.SolutionEfficientFrontierDataTable tableSolutionEfficientFrontier;

		// Token: 0x04000964 RID: 2404
		private DataRelation relationFK_Solution_SolutionConstraints;

		// Token: 0x04000965 RID: 2405
		private DataRelation relationFK_SolutionConstraints_SolutionConstraintValues;

		// Token: 0x04000966 RID: 2406
		private DataRelation relationFK_SolutionProjects_SolutionConstraintValues;

		// Token: 0x04000967 RID: 2407
		private DataRelation relationFK_Solution_SolutionProjects;

		// Token: 0x04000968 RID: 2408
		private DataRelation relationFK_Solution_SolutionStrategicAlignment;

		// Token: 0x04000969 RID: 2409
		private SchemaSerializationMode _schemaSerializationMode = SchemaSerializationMode.IncludeSchema;

		// Token: 0x02000227 RID: 551
		// (Invoke) Token: 0x06002CAC RID: 11436
		[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
		public delegate void SolutionRowChangeEventHandler(object sender, OptimizerSolutionDataSet.SolutionRowChangeEvent e);

		// Token: 0x02000228 RID: 552
		// (Invoke) Token: 0x06002CB0 RID: 11440
		[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
		public delegate void SolutionConstraintsRowChangeEventHandler(object sender, OptimizerSolutionDataSet.SolutionConstraintsRowChangeEvent e);

		// Token: 0x02000229 RID: 553
		// (Invoke) Token: 0x06002CB4 RID: 11444
		[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
		public delegate void SolutionConstraintValuesRowChangeEventHandler(object sender, OptimizerSolutionDataSet.SolutionConstraintValuesRowChangeEvent e);

		// Token: 0x0200022A RID: 554
		// (Invoke) Token: 0x06002CB8 RID: 11448
		[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
		public delegate void SolutionProjectsRowChangeEventHandler(object sender, OptimizerSolutionDataSet.SolutionProjectsRowChangeEvent e);

		// Token: 0x0200022B RID: 555
		// (Invoke) Token: 0x06002CBC RID: 11452
		[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
		public delegate void SolutionStrategicAlignmentRowChangeEventHandler(object sender, OptimizerSolutionDataSet.SolutionStrategicAlignmentRowChangeEvent e);

		// Token: 0x0200022C RID: 556
		// (Invoke) Token: 0x06002CC0 RID: 11456
		[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
		public delegate void SolutionEfficientFrontierRowChangeEventHandler(object sender, OptimizerSolutionDataSet.SolutionEfficientFrontierRowChangeEvent e);

		// Token: 0x0200022D RID: 557
		[XmlSchemaProvider("GetTypedTableSchema")]
		[Serializable]
		public class SolutionDataTable : DataTable, IEnumerable
		{
			// Token: 0x06002CC3 RID: 11459 RVA: 0x0008F700 File Offset: 0x0008D900
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public SolutionDataTable()
			{
				base.TableName = "Solution";
				this.BeginInit();
				this.InitClass();
				this.EndInit();
			}

			// Token: 0x06002CC4 RID: 11460 RVA: 0x0008F728 File Offset: 0x0008D928
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			internal SolutionDataTable(DataTable table)
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

			// Token: 0x06002CC5 RID: 11461 RVA: 0x0008F7D0 File Offset: 0x0008D9D0
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			protected SolutionDataTable(SerializationInfo info, StreamingContext context) : base(info, context)
			{
				this.InitVars();
			}

			// Token: 0x17000D21 RID: 3361
			// (get) Token: 0x06002CC6 RID: 11462 RVA: 0x0008F7E0 File Offset: 0x0008D9E0
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			public DataColumn ANALYSIS_UIDColumn
			{
				get
				{
					return this.columnANALYSIS_UID;
				}
			}

			// Token: 0x17000D22 RID: 3362
			// (get) Token: 0x06002CC7 RID: 11463 RVA: 0x0008F7E8 File Offset: 0x0008D9E8
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			public DataColumn SOLUTION_UIDColumn
			{
				get
				{
					return this.columnSOLUTION_UID;
				}
			}

			// Token: 0x17000D23 RID: 3363
			// (get) Token: 0x06002CC8 RID: 11464 RVA: 0x0008F7F0 File Offset: 0x0008D9F0
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			public DataColumn SOLUTION_NAMEColumn
			{
				get
				{
					return this.columnSOLUTION_NAME;
				}
			}

			// Token: 0x17000D24 RID: 3364
			// (get) Token: 0x06002CC9 RID: 11465 RVA: 0x0008F7F8 File Offset: 0x0008D9F8
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public DataColumn SOLUTION_DESCRIPTIONColumn
			{
				get
				{
					return this.columnSOLUTION_DESCRIPTION;
				}
			}

			// Token: 0x17000D25 RID: 3365
			// (get) Token: 0x06002CCA RID: 11466 RVA: 0x0008F800 File Offset: 0x0008DA00
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public DataColumn HARD_CONSTRAINT_CF_UIDColumn
			{
				get
				{
					return this.columnHARD_CONSTRAINT_CF_UID;
				}
			}

			// Token: 0x17000D26 RID: 3366
			// (get) Token: 0x06002CCB RID: 11467 RVA: 0x0008F808 File Offset: 0x0008DA08
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public DataColumn HARD_CONSTRAINT_CF_NAMEColumn
			{
				get
				{
					return this.columnHARD_CONSTRAINT_CF_NAME;
				}
			}

			// Token: 0x17000D27 RID: 3367
			// (get) Token: 0x06002CCC RID: 11468 RVA: 0x0008F810 File Offset: 0x0008DA10
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public DataColumn FRONTIER_UIDColumn
			{
				get
				{
					return this.columnFRONTIER_UID;
				}
			}

			// Token: 0x17000D28 RID: 3368
			// (get) Token: 0x06002CCD RID: 11469 RVA: 0x0008F818 File Offset: 0x0008DA18
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public DataColumn OPT_USE_DEPENDENCIESColumn
			{
				get
				{
					return this.columnOPT_USE_DEPENDENCIES;
				}
			}

			// Token: 0x17000D29 RID: 3369
			// (get) Token: 0x06002CCE RID: 11470 RVA: 0x0008F820 File Offset: 0x0008DA20
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			public DataColumn CREATED_DATEColumn
			{
				get
				{
					return this.columnCREATED_DATE;
				}
			}

			// Token: 0x17000D2A RID: 3370
			// (get) Token: 0x06002CCF RID: 11471 RVA: 0x0008F828 File Offset: 0x0008DA28
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public DataColumn MOD_DATEColumn
			{
				get
				{
					return this.columnMOD_DATE;
				}
			}

			// Token: 0x17000D2B RID: 3371
			// (get) Token: 0x06002CD0 RID: 11472 RVA: 0x0008F830 File Offset: 0x0008DA30
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public DataColumn LAST_UPDATED_BY_RES_UIDColumn
			{
				get
				{
					return this.columnLAST_UPDATED_BY_RES_UID;
				}
			}

			// Token: 0x17000D2C RID: 3372
			// (get) Token: 0x06002CD1 RID: 11473 RVA: 0x0008F838 File Offset: 0x0008DA38
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			public DataColumn LAST_UPDATED_BY_RES_NAMEColumn
			{
				get
				{
					return this.columnLAST_UPDATED_BY_RES_NAME;
				}
			}

			// Token: 0x17000D2D RID: 3373
			// (get) Token: 0x06002CD2 RID: 11474 RVA: 0x0008F840 File Offset: 0x0008DA40
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			public DataColumn CREATED_BY_RES_UIDColumn
			{
				get
				{
					return this.columnCREATED_BY_RES_UID;
				}
			}

			// Token: 0x17000D2E RID: 3374
			// (get) Token: 0x06002CD3 RID: 11475 RVA: 0x0008F848 File Offset: 0x0008DA48
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public DataColumn CREATED_BY_RES_NAMEColumn
			{
				get
				{
					return this.columnCREATED_BY_RES_NAME;
				}
			}

			// Token: 0x17000D2F RID: 3375
			// (get) Token: 0x06002CD4 RID: 11476 RVA: 0x0008F850 File Offset: 0x0008DA50
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			public DataColumn ANALYSIS_NAMEColumn
			{
				get
				{
					return this.columnANALYSIS_NAME;
				}
			}

			// Token: 0x17000D30 RID: 3376
			// (get) Token: 0x06002CD5 RID: 11477 RVA: 0x0008F858 File Offset: 0x0008DA58
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			public DataColumn TOTAL_HARD_CONSTRAINT_VALUEColumn
			{
				get
				{
					return this.columnTOTAL_HARD_CONSTRAINT_VALUE;
				}
			}

			// Token: 0x17000D31 RID: 3377
			// (get) Token: 0x06002CD6 RID: 11478 RVA: 0x0008F860 File Offset: 0x0008DA60
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			public DataColumn TOTAL_PRIORITY_VALUEColumn
			{
				get
				{
					return this.columnTOTAL_PRIORITY_VALUE;
				}
			}

			// Token: 0x17000D32 RID: 3378
			// (get) Token: 0x06002CD7 RID: 11479 RVA: 0x0008F868 File Offset: 0x0008DA68
			[Browsable(false)]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			public int Count
			{
				get
				{
					return base.Rows.Count;
				}
			}

			// Token: 0x17000D33 RID: 3379
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public OptimizerSolutionDataSet.SolutionRow this[int index]
			{
				get
				{
					return (OptimizerSolutionDataSet.SolutionRow)base.Rows[index];
				}
			}

			// Token: 0x140001D5 RID: 469
			// (add) Token: 0x06002CD9 RID: 11481 RVA: 0x0008F888 File Offset: 0x0008DA88
			// (remove) Token: 0x06002CDA RID: 11482 RVA: 0x0008F8C0 File Offset: 0x0008DAC0
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public event OptimizerSolutionDataSet.SolutionRowChangeEventHandler SolutionRowChanging;

			// Token: 0x140001D6 RID: 470
			// (add) Token: 0x06002CDB RID: 11483 RVA: 0x0008F8F8 File Offset: 0x0008DAF8
			// (remove) Token: 0x06002CDC RID: 11484 RVA: 0x0008F930 File Offset: 0x0008DB30
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public event OptimizerSolutionDataSet.SolutionRowChangeEventHandler SolutionRowChanged;

			// Token: 0x140001D7 RID: 471
			// (add) Token: 0x06002CDD RID: 11485 RVA: 0x0008F968 File Offset: 0x0008DB68
			// (remove) Token: 0x06002CDE RID: 11486 RVA: 0x0008F9A0 File Offset: 0x0008DBA0
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public event OptimizerSolutionDataSet.SolutionRowChangeEventHandler SolutionRowDeleting;

			// Token: 0x140001D8 RID: 472
			// (add) Token: 0x06002CDF RID: 11487 RVA: 0x0008F9D8 File Offset: 0x0008DBD8
			// (remove) Token: 0x06002CE0 RID: 11488 RVA: 0x0008FA10 File Offset: 0x0008DC10
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public event OptimizerSolutionDataSet.SolutionRowChangeEventHandler SolutionRowDeleted;

			// Token: 0x06002CE1 RID: 11489 RVA: 0x0008FA45 File Offset: 0x0008DC45
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public void AddSolutionRow(OptimizerSolutionDataSet.SolutionRow row)
			{
				base.Rows.Add(row);
			}

			// Token: 0x06002CE2 RID: 11490 RVA: 0x0008FA54 File Offset: 0x0008DC54
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			public OptimizerSolutionDataSet.SolutionRow AddSolutionRow(Guid ANALYSIS_UID, Guid SOLUTION_UID, string SOLUTION_NAME, string SOLUTION_DESCRIPTION, Guid HARD_CONSTRAINT_CF_UID, string HARD_CONSTRAINT_CF_NAME, Guid FRONTIER_UID, bool OPT_USE_DEPENDENCIES, DateTime CREATED_DATE, DateTime MOD_DATE, Guid LAST_UPDATED_BY_RES_UID, string LAST_UPDATED_BY_RES_NAME, Guid CREATED_BY_RES_UID, string CREATED_BY_RES_NAME, string ANALYSIS_NAME, decimal TOTAL_HARD_CONSTRAINT_VALUE, decimal TOTAL_PRIORITY_VALUE)
			{
				OptimizerSolutionDataSet.SolutionRow solutionRow = (OptimizerSolutionDataSet.SolutionRow)base.NewRow();
				object[] itemArray = new object[]
				{
					ANALYSIS_UID,
					SOLUTION_UID,
					SOLUTION_NAME,
					SOLUTION_DESCRIPTION,
					HARD_CONSTRAINT_CF_UID,
					HARD_CONSTRAINT_CF_NAME,
					FRONTIER_UID,
					OPT_USE_DEPENDENCIES,
					CREATED_DATE,
					MOD_DATE,
					LAST_UPDATED_BY_RES_UID,
					LAST_UPDATED_BY_RES_NAME,
					CREATED_BY_RES_UID,
					CREATED_BY_RES_NAME,
					ANALYSIS_NAME,
					TOTAL_HARD_CONSTRAINT_VALUE,
					TOTAL_PRIORITY_VALUE
				};
				solutionRow.ItemArray = itemArray;
				base.Rows.Add(solutionRow);
				return solutionRow;
			}

			// Token: 0x06002CE3 RID: 11491 RVA: 0x0008FB1C File Offset: 0x0008DD1C
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public OptimizerSolutionDataSet.SolutionRow FindBySOLUTION_UID(Guid SOLUTION_UID)
			{
				return (OptimizerSolutionDataSet.SolutionRow)base.Rows.Find(new object[]
				{
					SOLUTION_UID
				});
			}

			// Token: 0x06002CE4 RID: 11492 RVA: 0x0008FB4A File Offset: 0x0008DD4A
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public virtual IEnumerator GetEnumerator()
			{
				return base.Rows.GetEnumerator();
			}

			// Token: 0x06002CE5 RID: 11493 RVA: 0x0008FB58 File Offset: 0x0008DD58
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public override DataTable Clone()
			{
				OptimizerSolutionDataSet.SolutionDataTable solutionDataTable = (OptimizerSolutionDataSet.SolutionDataTable)base.Clone();
				solutionDataTable.InitVars();
				return solutionDataTable;
			}

			// Token: 0x06002CE6 RID: 11494 RVA: 0x0008FB78 File Offset: 0x0008DD78
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			protected override DataTable CreateInstance()
			{
				return new OptimizerSolutionDataSet.SolutionDataTable();
			}

			// Token: 0x06002CE7 RID: 11495 RVA: 0x0008FB80 File Offset: 0x0008DD80
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			internal void InitVars()
			{
				this.columnANALYSIS_UID = base.Columns["ANALYSIS_UID"];
				this.columnSOLUTION_UID = base.Columns["SOLUTION_UID"];
				this.columnSOLUTION_NAME = base.Columns["SOLUTION_NAME"];
				this.columnSOLUTION_DESCRIPTION = base.Columns["SOLUTION_DESCRIPTION"];
				this.columnHARD_CONSTRAINT_CF_UID = base.Columns["HARD_CONSTRAINT_CF_UID"];
				this.columnHARD_CONSTRAINT_CF_NAME = base.Columns["HARD_CONSTRAINT_CF_NAME"];
				this.columnFRONTIER_UID = base.Columns["FRONTIER_UID"];
				this.columnOPT_USE_DEPENDENCIES = base.Columns["OPT_USE_DEPENDENCIES"];
				this.columnCREATED_DATE = base.Columns["CREATED_DATE"];
				this.columnMOD_DATE = base.Columns["MOD_DATE"];
				this.columnLAST_UPDATED_BY_RES_UID = base.Columns["LAST_UPDATED_BY_RES_UID"];
				this.columnLAST_UPDATED_BY_RES_NAME = base.Columns["LAST_UPDATED_BY_RES_NAME"];
				this.columnCREATED_BY_RES_UID = base.Columns["CREATED_BY_RES_UID"];
				this.columnCREATED_BY_RES_NAME = base.Columns["CREATED_BY_RES_NAME"];
				this.columnANALYSIS_NAME = base.Columns["ANALYSIS_NAME"];
				this.columnTOTAL_HARD_CONSTRAINT_VALUE = base.Columns["TOTAL_HARD_CONSTRAINT_VALUE"];
				this.columnTOTAL_PRIORITY_VALUE = base.Columns["TOTAL_PRIORITY_VALUE"];
			}

			// Token: 0x06002CE8 RID: 11496 RVA: 0x0008FD04 File Offset: 0x0008DF04
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
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
				this.columnHARD_CONSTRAINT_CF_UID = new DataColumn("HARD_CONSTRAINT_CF_UID", typeof(Guid), null, MappingType.Element);
				base.Columns.Add(this.columnHARD_CONSTRAINT_CF_UID);
				this.columnHARD_CONSTRAINT_CF_NAME = new DataColumn("HARD_CONSTRAINT_CF_NAME", typeof(string), null, MappingType.Element);
				base.Columns.Add(this.columnHARD_CONSTRAINT_CF_NAME);
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
				this.columnANALYSIS_NAME = new DataColumn("ANALYSIS_NAME", typeof(string), null, MappingType.Element);
				base.Columns.Add(this.columnANALYSIS_NAME);
				this.columnTOTAL_HARD_CONSTRAINT_VALUE = new DataColumn("TOTAL_HARD_CONSTRAINT_VALUE", typeof(decimal), null, MappingType.Element);
				base.Columns.Add(this.columnTOTAL_HARD_CONSTRAINT_VALUE);
				this.columnTOTAL_PRIORITY_VALUE = new DataColumn("TOTAL_PRIORITY_VALUE", typeof(decimal), null, MappingType.Element);
				base.Columns.Add(this.columnTOTAL_PRIORITY_VALUE);
				base.Constraints.Add(new UniqueConstraint("PK_Solution", new DataColumn[]
				{
					this.columnSOLUTION_UID
				}, true));
				this.columnANALYSIS_UID.AllowDBNull = false;
				this.columnSOLUTION_UID.AllowDBNull = false;
				this.columnSOLUTION_UID.Unique = true;
				this.columnSOLUTION_NAME.AllowDBNull = false;
				this.columnHARD_CONSTRAINT_CF_UID.ReadOnly = true;
				this.columnHARD_CONSTRAINT_CF_NAME.ReadOnly = true;
				this.columnOPT_USE_DEPENDENCIES.AllowDBNull = false;
				this.columnOPT_USE_DEPENDENCIES.DefaultValue = true;
				this.columnLAST_UPDATED_BY_RES_NAME.ReadOnly = true;
				this.columnCREATED_BY_RES_NAME.ReadOnly = true;
				this.columnANALYSIS_NAME.ReadOnly = true;
				this.columnTOTAL_HARD_CONSTRAINT_VALUE.ReadOnly = true;
				this.columnTOTAL_PRIORITY_VALUE.ReadOnly = true;
			}

			// Token: 0x06002CE9 RID: 11497 RVA: 0x000900D6 File Offset: 0x0008E2D6
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public OptimizerSolutionDataSet.SolutionRow NewSolutionRow()
			{
				return (OptimizerSolutionDataSet.SolutionRow)base.NewRow();
			}

			// Token: 0x06002CEA RID: 11498 RVA: 0x000900E3 File Offset: 0x0008E2E3
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			protected override DataRow NewRowFromBuilder(DataRowBuilder builder)
			{
				return new OptimizerSolutionDataSet.SolutionRow(builder);
			}

			// Token: 0x06002CEB RID: 11499 RVA: 0x000900EB File Offset: 0x0008E2EB
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			protected override Type GetRowType()
			{
				return typeof(OptimizerSolutionDataSet.SolutionRow);
			}

			// Token: 0x06002CEC RID: 11500 RVA: 0x000900F7 File Offset: 0x0008E2F7
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			protected override void OnRowChanged(DataRowChangeEventArgs e)
			{
				base.OnRowChanged(e);
				if (this.SolutionRowChanged != null)
				{
					this.SolutionRowChanged(this, new OptimizerSolutionDataSet.SolutionRowChangeEvent((OptimizerSolutionDataSet.SolutionRow)e.Row, e.Action));
				}
			}

			// Token: 0x06002CED RID: 11501 RVA: 0x0009012A File Offset: 0x0008E32A
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			protected override void OnRowChanging(DataRowChangeEventArgs e)
			{
				base.OnRowChanging(e);
				if (this.SolutionRowChanging != null)
				{
					this.SolutionRowChanging(this, new OptimizerSolutionDataSet.SolutionRowChangeEvent((OptimizerSolutionDataSet.SolutionRow)e.Row, e.Action));
				}
			}

			// Token: 0x06002CEE RID: 11502 RVA: 0x0009015D File Offset: 0x0008E35D
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			protected override void OnRowDeleted(DataRowChangeEventArgs e)
			{
				base.OnRowDeleted(e);
				if (this.SolutionRowDeleted != null)
				{
					this.SolutionRowDeleted(this, new OptimizerSolutionDataSet.SolutionRowChangeEvent((OptimizerSolutionDataSet.SolutionRow)e.Row, e.Action));
				}
			}

			// Token: 0x06002CEF RID: 11503 RVA: 0x00090190 File Offset: 0x0008E390
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			protected override void OnRowDeleting(DataRowChangeEventArgs e)
			{
				base.OnRowDeleting(e);
				if (this.SolutionRowDeleting != null)
				{
					this.SolutionRowDeleting(this, new OptimizerSolutionDataSet.SolutionRowChangeEvent((OptimizerSolutionDataSet.SolutionRow)e.Row, e.Action));
				}
			}

			// Token: 0x06002CF0 RID: 11504 RVA: 0x000901C3 File Offset: 0x0008E3C3
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public void RemoveSolutionRow(OptimizerSolutionDataSet.SolutionRow row)
			{
				base.Rows.Remove(row);
			}

			// Token: 0x06002CF1 RID: 11505 RVA: 0x000901D4 File Offset: 0x0008E3D4
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public static XmlSchemaComplexType GetTypedTableSchema(XmlSchemaSet xs)
			{
				XmlSchemaComplexType xmlSchemaComplexType = new XmlSchemaComplexType();
				XmlSchemaSequence xmlSchemaSequence = new XmlSchemaSequence();
				OptimizerSolutionDataSet optimizerSolutionDataSet = new OptimizerSolutionDataSet();
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
				xmlSchemaAttribute.FixedValue = optimizerSolutionDataSet.Namespace;
				xmlSchemaComplexType.Attributes.Add(xmlSchemaAttribute);
				XmlSchemaAttribute xmlSchemaAttribute2 = new XmlSchemaAttribute();
				xmlSchemaAttribute2.Name = "tableTypeName";
				xmlSchemaAttribute2.FixedValue = "SolutionDataTable";
				xmlSchemaComplexType.Attributes.Add(xmlSchemaAttribute2);
				xmlSchemaComplexType.Particle = xmlSchemaSequence;
				XmlSchema schemaSerializable = optimizerSolutionDataSet.GetSchemaSerializable();
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

			// Token: 0x0400096A RID: 2410
			private DataColumn columnANALYSIS_UID;

			// Token: 0x0400096B RID: 2411
			private DataColumn columnSOLUTION_UID;

			// Token: 0x0400096C RID: 2412
			private DataColumn columnSOLUTION_NAME;

			// Token: 0x0400096D RID: 2413
			private DataColumn columnSOLUTION_DESCRIPTION;

			// Token: 0x0400096E RID: 2414
			private DataColumn columnHARD_CONSTRAINT_CF_UID;

			// Token: 0x0400096F RID: 2415
			private DataColumn columnHARD_CONSTRAINT_CF_NAME;

			// Token: 0x04000970 RID: 2416
			private DataColumn columnFRONTIER_UID;

			// Token: 0x04000971 RID: 2417
			private DataColumn columnOPT_USE_DEPENDENCIES;

			// Token: 0x04000972 RID: 2418
			private DataColumn columnCREATED_DATE;

			// Token: 0x04000973 RID: 2419
			private DataColumn columnMOD_DATE;

			// Token: 0x04000974 RID: 2420
			private DataColumn columnLAST_UPDATED_BY_RES_UID;

			// Token: 0x04000975 RID: 2421
			private DataColumn columnLAST_UPDATED_BY_RES_NAME;

			// Token: 0x04000976 RID: 2422
			private DataColumn columnCREATED_BY_RES_UID;

			// Token: 0x04000977 RID: 2423
			private DataColumn columnCREATED_BY_RES_NAME;

			// Token: 0x04000978 RID: 2424
			private DataColumn columnANALYSIS_NAME;

			// Token: 0x04000979 RID: 2425
			private DataColumn columnTOTAL_HARD_CONSTRAINT_VALUE;

			// Token: 0x0400097A RID: 2426
			private DataColumn columnTOTAL_PRIORITY_VALUE;
		}

		// Token: 0x0200022E RID: 558
		[XmlSchemaProvider("GetTypedTableSchema")]
		[Serializable]
		public class SolutionConstraintsDataTable : DataTable, IEnumerable
		{
			// Token: 0x06002CF2 RID: 11506 RVA: 0x000903CC File Offset: 0x0008E5CC
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			public SolutionConstraintsDataTable()
			{
				base.TableName = "SolutionConstraints";
				this.BeginInit();
				this.InitClass();
				this.EndInit();
			}

			// Token: 0x06002CF3 RID: 11507 RVA: 0x000903F4 File Offset: 0x0008E5F4
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			internal SolutionConstraintsDataTable(DataTable table)
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

			// Token: 0x06002CF4 RID: 11508 RVA: 0x0009049C File Offset: 0x0008E69C
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			protected SolutionConstraintsDataTable(SerializationInfo info, StreamingContext context) : base(info, context)
			{
				this.InitVars();
			}

			// Token: 0x17000D34 RID: 3380
			// (get) Token: 0x06002CF5 RID: 11509 RVA: 0x000904AC File Offset: 0x0008E6AC
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public DataColumn SOLUTION_UIDColumn
			{
				get
				{
					return this.columnSOLUTION_UID;
				}
			}

			// Token: 0x17000D35 RID: 3381
			// (get) Token: 0x06002CF6 RID: 11510 RVA: 0x000904B4 File Offset: 0x0008E6B4
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public DataColumn MD_PROP_UIDColumn
			{
				get
				{
					return this.columnMD_PROP_UID;
				}
			}

			// Token: 0x17000D36 RID: 3382
			// (get) Token: 0x06002CF7 RID: 11511 RVA: 0x000904BC File Offset: 0x0008E6BC
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			public DataColumn MD_PROP_NAMEColumn
			{
				get
				{
					return this.columnMD_PROP_NAME;
				}
			}

			// Token: 0x17000D37 RID: 3383
			// (get) Token: 0x06002CF8 RID: 11512 RVA: 0x000904C4 File Offset: 0x0008E6C4
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public DataColumn MD_PROP_POSColumn
			{
				get
				{
					return this.columnMD_PROP_POS;
				}
			}

			// Token: 0x17000D38 RID: 3384
			// (get) Token: 0x06002CF9 RID: 11513 RVA: 0x000904CC File Offset: 0x0008E6CC
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			public DataColumn MAX_VALUEColumn
			{
				get
				{
					return this.columnMAX_VALUE;
				}
			}

			// Token: 0x17000D39 RID: 3385
			// (get) Token: 0x06002CFA RID: 11514 RVA: 0x000904D4 File Offset: 0x0008E6D4
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

			// Token: 0x17000D3A RID: 3386
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			public OptimizerSolutionDataSet.SolutionConstraintsRow this[int index]
			{
				get
				{
					return (OptimizerSolutionDataSet.SolutionConstraintsRow)base.Rows[index];
				}
			}

			// Token: 0x140001D9 RID: 473
			// (add) Token: 0x06002CFC RID: 11516 RVA: 0x000904F4 File Offset: 0x0008E6F4
			// (remove) Token: 0x06002CFD RID: 11517 RVA: 0x0009052C File Offset: 0x0008E72C
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public event OptimizerSolutionDataSet.SolutionConstraintsRowChangeEventHandler SolutionConstraintsRowChanging;

			// Token: 0x140001DA RID: 474
			// (add) Token: 0x06002CFE RID: 11518 RVA: 0x00090564 File Offset: 0x0008E764
			// (remove) Token: 0x06002CFF RID: 11519 RVA: 0x0009059C File Offset: 0x0008E79C
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public event OptimizerSolutionDataSet.SolutionConstraintsRowChangeEventHandler SolutionConstraintsRowChanged;

			// Token: 0x140001DB RID: 475
			// (add) Token: 0x06002D00 RID: 11520 RVA: 0x000905D4 File Offset: 0x0008E7D4
			// (remove) Token: 0x06002D01 RID: 11521 RVA: 0x0009060C File Offset: 0x0008E80C
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public event OptimizerSolutionDataSet.SolutionConstraintsRowChangeEventHandler SolutionConstraintsRowDeleting;

			// Token: 0x140001DC RID: 476
			// (add) Token: 0x06002D02 RID: 11522 RVA: 0x00090644 File Offset: 0x0008E844
			// (remove) Token: 0x06002D03 RID: 11523 RVA: 0x0009067C File Offset: 0x0008E87C
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public event OptimizerSolutionDataSet.SolutionConstraintsRowChangeEventHandler SolutionConstraintsRowDeleted;

			// Token: 0x06002D04 RID: 11524 RVA: 0x000906B1 File Offset: 0x0008E8B1
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			public void AddSolutionConstraintsRow(OptimizerSolutionDataSet.SolutionConstraintsRow row)
			{
				base.Rows.Add(row);
			}

			// Token: 0x06002D05 RID: 11525 RVA: 0x000906C0 File Offset: 0x0008E8C0
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			public OptimizerSolutionDataSet.SolutionConstraintsRow AddSolutionConstraintsRow(OptimizerSolutionDataSet.SolutionRow parentSolutionRowByFK_Solution_SolutionConstraints, Guid MD_PROP_UID, string MD_PROP_NAME, int MD_PROP_POS, decimal MAX_VALUE)
			{
				OptimizerSolutionDataSet.SolutionConstraintsRow solutionConstraintsRow = (OptimizerSolutionDataSet.SolutionConstraintsRow)base.NewRow();
				object[] array = new object[]
				{
					null,
					MD_PROP_UID,
					MD_PROP_NAME,
					MD_PROP_POS,
					MAX_VALUE
				};
				if (parentSolutionRowByFK_Solution_SolutionConstraints != null)
				{
					array[0] = parentSolutionRowByFK_Solution_SolutionConstraints[1];
				}
				solutionConstraintsRow.ItemArray = array;
				base.Rows.Add(solutionConstraintsRow);
				return solutionConstraintsRow;
			}

			// Token: 0x06002D06 RID: 11526 RVA: 0x00090724 File Offset: 0x0008E924
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			public OptimizerSolutionDataSet.SolutionConstraintsRow FindBySOLUTION_UIDMD_PROP_UID(Guid SOLUTION_UID, Guid MD_PROP_UID)
			{
				return (OptimizerSolutionDataSet.SolutionConstraintsRow)base.Rows.Find(new object[]
				{
					SOLUTION_UID,
					MD_PROP_UID
				});
			}

			// Token: 0x06002D07 RID: 11527 RVA: 0x0009075B File Offset: 0x0008E95B
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			public virtual IEnumerator GetEnumerator()
			{
				return base.Rows.GetEnumerator();
			}

			// Token: 0x06002D08 RID: 11528 RVA: 0x00090768 File Offset: 0x0008E968
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			public override DataTable Clone()
			{
				OptimizerSolutionDataSet.SolutionConstraintsDataTable solutionConstraintsDataTable = (OptimizerSolutionDataSet.SolutionConstraintsDataTable)base.Clone();
				solutionConstraintsDataTable.InitVars();
				return solutionConstraintsDataTable;
			}

			// Token: 0x06002D09 RID: 11529 RVA: 0x00090788 File Offset: 0x0008E988
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			protected override DataTable CreateInstance()
			{
				return new OptimizerSolutionDataSet.SolutionConstraintsDataTable();
			}

			// Token: 0x06002D0A RID: 11530 RVA: 0x00090790 File Offset: 0x0008E990
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			internal void InitVars()
			{
				this.columnSOLUTION_UID = base.Columns["SOLUTION_UID"];
				this.columnMD_PROP_UID = base.Columns["MD_PROP_UID"];
				this.columnMD_PROP_NAME = base.Columns["MD_PROP_NAME"];
				this.columnMD_PROP_POS = base.Columns["MD_PROP_POS"];
				this.columnMAX_VALUE = base.Columns["MAX_VALUE"];
			}

			// Token: 0x06002D0B RID: 11531 RVA: 0x0009080C File Offset: 0x0008EA0C
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			private void InitClass()
			{
				this.columnSOLUTION_UID = new DataColumn("SOLUTION_UID", typeof(Guid), null, MappingType.Element);
				base.Columns.Add(this.columnSOLUTION_UID);
				this.columnMD_PROP_UID = new DataColumn("MD_PROP_UID", typeof(Guid), null, MappingType.Element);
				base.Columns.Add(this.columnMD_PROP_UID);
				this.columnMD_PROP_NAME = new DataColumn("MD_PROP_NAME", typeof(string), null, MappingType.Element);
				base.Columns.Add(this.columnMD_PROP_NAME);
				this.columnMD_PROP_POS = new DataColumn("MD_PROP_POS", typeof(int), null, MappingType.Element);
				base.Columns.Add(this.columnMD_PROP_POS);
				this.columnMAX_VALUE = new DataColumn("MAX_VALUE", typeof(decimal), null, MappingType.Element);
				base.Columns.Add(this.columnMAX_VALUE);
				base.Constraints.Add(new UniqueConstraint("PK_SolutionConstraints", new DataColumn[]
				{
					this.columnSOLUTION_UID,
					this.columnMD_PROP_UID
				}, true));
				this.columnSOLUTION_UID.AllowDBNull = false;
				this.columnMD_PROP_UID.AllowDBNull = false;
				this.columnMD_PROP_NAME.ReadOnly = true;
				this.columnMD_PROP_POS.AllowDBNull = false;
				this.columnMAX_VALUE.AllowDBNull = false;
			}

			// Token: 0x06002D0C RID: 11532 RVA: 0x00090966 File Offset: 0x0008EB66
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public OptimizerSolutionDataSet.SolutionConstraintsRow NewSolutionConstraintsRow()
			{
				return (OptimizerSolutionDataSet.SolutionConstraintsRow)base.NewRow();
			}

			// Token: 0x06002D0D RID: 11533 RVA: 0x00090973 File Offset: 0x0008EB73
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			protected override DataRow NewRowFromBuilder(DataRowBuilder builder)
			{
				return new OptimizerSolutionDataSet.SolutionConstraintsRow(builder);
			}

			// Token: 0x06002D0E RID: 11534 RVA: 0x0009097B File Offset: 0x0008EB7B
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			protected override Type GetRowType()
			{
				return typeof(OptimizerSolutionDataSet.SolutionConstraintsRow);
			}

			// Token: 0x06002D0F RID: 11535 RVA: 0x00090987 File Offset: 0x0008EB87
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			protected override void OnRowChanged(DataRowChangeEventArgs e)
			{
				base.OnRowChanged(e);
				if (this.SolutionConstraintsRowChanged != null)
				{
					this.SolutionConstraintsRowChanged(this, new OptimizerSolutionDataSet.SolutionConstraintsRowChangeEvent((OptimizerSolutionDataSet.SolutionConstraintsRow)e.Row, e.Action));
				}
			}

			// Token: 0x06002D10 RID: 11536 RVA: 0x000909BA File Offset: 0x0008EBBA
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			protected override void OnRowChanging(DataRowChangeEventArgs e)
			{
				base.OnRowChanging(e);
				if (this.SolutionConstraintsRowChanging != null)
				{
					this.SolutionConstraintsRowChanging(this, new OptimizerSolutionDataSet.SolutionConstraintsRowChangeEvent((OptimizerSolutionDataSet.SolutionConstraintsRow)e.Row, e.Action));
				}
			}

			// Token: 0x06002D11 RID: 11537 RVA: 0x000909ED File Offset: 0x0008EBED
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			protected override void OnRowDeleted(DataRowChangeEventArgs e)
			{
				base.OnRowDeleted(e);
				if (this.SolutionConstraintsRowDeleted != null)
				{
					this.SolutionConstraintsRowDeleted(this, new OptimizerSolutionDataSet.SolutionConstraintsRowChangeEvent((OptimizerSolutionDataSet.SolutionConstraintsRow)e.Row, e.Action));
				}
			}

			// Token: 0x06002D12 RID: 11538 RVA: 0x00090A20 File Offset: 0x0008EC20
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			protected override void OnRowDeleting(DataRowChangeEventArgs e)
			{
				base.OnRowDeleting(e);
				if (this.SolutionConstraintsRowDeleting != null)
				{
					this.SolutionConstraintsRowDeleting(this, new OptimizerSolutionDataSet.SolutionConstraintsRowChangeEvent((OptimizerSolutionDataSet.SolutionConstraintsRow)e.Row, e.Action));
				}
			}

			// Token: 0x06002D13 RID: 11539 RVA: 0x00090A53 File Offset: 0x0008EC53
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			public void RemoveSolutionConstraintsRow(OptimizerSolutionDataSet.SolutionConstraintsRow row)
			{
				base.Rows.Remove(row);
			}

			// Token: 0x06002D14 RID: 11540 RVA: 0x00090A64 File Offset: 0x0008EC64
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			public static XmlSchemaComplexType GetTypedTableSchema(XmlSchemaSet xs)
			{
				XmlSchemaComplexType xmlSchemaComplexType = new XmlSchemaComplexType();
				XmlSchemaSequence xmlSchemaSequence = new XmlSchemaSequence();
				OptimizerSolutionDataSet optimizerSolutionDataSet = new OptimizerSolutionDataSet();
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
				xmlSchemaAttribute.FixedValue = optimizerSolutionDataSet.Namespace;
				xmlSchemaComplexType.Attributes.Add(xmlSchemaAttribute);
				XmlSchemaAttribute xmlSchemaAttribute2 = new XmlSchemaAttribute();
				xmlSchemaAttribute2.Name = "tableTypeName";
				xmlSchemaAttribute2.FixedValue = "SolutionConstraintsDataTable";
				xmlSchemaComplexType.Attributes.Add(xmlSchemaAttribute2);
				xmlSchemaComplexType.Particle = xmlSchemaSequence;
				XmlSchema schemaSerializable = optimizerSolutionDataSet.GetSchemaSerializable();
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

			// Token: 0x0400097F RID: 2431
			private DataColumn columnSOLUTION_UID;

			// Token: 0x04000980 RID: 2432
			private DataColumn columnMD_PROP_UID;

			// Token: 0x04000981 RID: 2433
			private DataColumn columnMD_PROP_NAME;

			// Token: 0x04000982 RID: 2434
			private DataColumn columnMD_PROP_POS;

			// Token: 0x04000983 RID: 2435
			private DataColumn columnMAX_VALUE;
		}

		// Token: 0x0200022F RID: 559
		[XmlSchemaProvider("GetTypedTableSchema")]
		[Serializable]
		public class SolutionConstraintValuesDataTable : DataTable, IEnumerable
		{
			// Token: 0x06002D15 RID: 11541 RVA: 0x00090C5C File Offset: 0x0008EE5C
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			public SolutionConstraintValuesDataTable()
			{
				base.TableName = "SolutionConstraintValues";
				this.BeginInit();
				this.InitClass();
				this.EndInit();
			}

			// Token: 0x06002D16 RID: 11542 RVA: 0x00090C84 File Offset: 0x0008EE84
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			internal SolutionConstraintValuesDataTable(DataTable table)
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

			// Token: 0x06002D17 RID: 11543 RVA: 0x00090D2C File Offset: 0x0008EF2C
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			protected SolutionConstraintValuesDataTable(SerializationInfo info, StreamingContext context) : base(info, context)
			{
				this.InitVars();
			}

			// Token: 0x17000D3B RID: 3387
			// (get) Token: 0x06002D18 RID: 11544 RVA: 0x00090D3C File Offset: 0x0008EF3C
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			public DataColumn SOLUTION_UIDColumn
			{
				get
				{
					return this.columnSOLUTION_UID;
				}
			}

			// Token: 0x17000D3C RID: 3388
			// (get) Token: 0x06002D19 RID: 11545 RVA: 0x00090D44 File Offset: 0x0008EF44
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public DataColumn MD_PROP_UIDColumn
			{
				get
				{
					return this.columnMD_PROP_UID;
				}
			}

			// Token: 0x17000D3D RID: 3389
			// (get) Token: 0x06002D1A RID: 11546 RVA: 0x00090D4C File Offset: 0x0008EF4C
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			public DataColumn PROJ_UIDColumn
			{
				get
				{
					return this.columnPROJ_UID;
				}
			}

			// Token: 0x17000D3E RID: 3390
			// (get) Token: 0x06002D1B RID: 11547 RVA: 0x00090D54 File Offset: 0x0008EF54
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			public DataColumn NUM_VALUEColumn
			{
				get
				{
					return this.columnNUM_VALUE;
				}
			}

			// Token: 0x17000D3F RID: 3391
			// (get) Token: 0x06002D1C RID: 11548 RVA: 0x00090D5C File Offset: 0x0008EF5C
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

			// Token: 0x17000D40 RID: 3392
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			public OptimizerSolutionDataSet.SolutionConstraintValuesRow this[int index]
			{
				get
				{
					return (OptimizerSolutionDataSet.SolutionConstraintValuesRow)base.Rows[index];
				}
			}

			// Token: 0x140001DD RID: 477
			// (add) Token: 0x06002D1E RID: 11550 RVA: 0x00090D7C File Offset: 0x0008EF7C
			// (remove) Token: 0x06002D1F RID: 11551 RVA: 0x00090DB4 File Offset: 0x0008EFB4
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public event OptimizerSolutionDataSet.SolutionConstraintValuesRowChangeEventHandler SolutionConstraintValuesRowChanging;

			// Token: 0x140001DE RID: 478
			// (add) Token: 0x06002D20 RID: 11552 RVA: 0x00090DEC File Offset: 0x0008EFEC
			// (remove) Token: 0x06002D21 RID: 11553 RVA: 0x00090E24 File Offset: 0x0008F024
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public event OptimizerSolutionDataSet.SolutionConstraintValuesRowChangeEventHandler SolutionConstraintValuesRowChanged;

			// Token: 0x140001DF RID: 479
			// (add) Token: 0x06002D22 RID: 11554 RVA: 0x00090E5C File Offset: 0x0008F05C
			// (remove) Token: 0x06002D23 RID: 11555 RVA: 0x00090E94 File Offset: 0x0008F094
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public event OptimizerSolutionDataSet.SolutionConstraintValuesRowChangeEventHandler SolutionConstraintValuesRowDeleting;

			// Token: 0x140001E0 RID: 480
			// (add) Token: 0x06002D24 RID: 11556 RVA: 0x00090ECC File Offset: 0x0008F0CC
			// (remove) Token: 0x06002D25 RID: 11557 RVA: 0x00090F04 File Offset: 0x0008F104
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public event OptimizerSolutionDataSet.SolutionConstraintValuesRowChangeEventHandler SolutionConstraintValuesRowDeleted;

			// Token: 0x06002D26 RID: 11558 RVA: 0x00090F39 File Offset: 0x0008F139
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			public void AddSolutionConstraintValuesRow(OptimizerSolutionDataSet.SolutionConstraintValuesRow row)
			{
				base.Rows.Add(row);
			}

			// Token: 0x06002D27 RID: 11559 RVA: 0x00090F48 File Offset: 0x0008F148
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			public OptimizerSolutionDataSet.SolutionConstraintValuesRow AddSolutionConstraintValuesRow(Guid SOLUTION_UID, Guid MD_PROP_UID, Guid PROJ_UID, decimal NUM_VALUE)
			{
				OptimizerSolutionDataSet.SolutionConstraintValuesRow solutionConstraintValuesRow = (OptimizerSolutionDataSet.SolutionConstraintValuesRow)base.NewRow();
				object[] itemArray = new object[]
				{
					SOLUTION_UID,
					MD_PROP_UID,
					PROJ_UID,
					NUM_VALUE
				};
				solutionConstraintValuesRow.ItemArray = itemArray;
				base.Rows.Add(solutionConstraintValuesRow);
				return solutionConstraintValuesRow;
			}

			// Token: 0x06002D28 RID: 11560 RVA: 0x00090FA4 File Offset: 0x0008F1A4
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			public OptimizerSolutionDataSet.SolutionConstraintValuesRow FindBySOLUTION_UIDMD_PROP_UIDPROJ_UID(Guid SOLUTION_UID, Guid MD_PROP_UID, Guid PROJ_UID)
			{
				return (OptimizerSolutionDataSet.SolutionConstraintValuesRow)base.Rows.Find(new object[]
				{
					SOLUTION_UID,
					MD_PROP_UID,
					PROJ_UID
				});
			}

			// Token: 0x06002D29 RID: 11561 RVA: 0x00090FE4 File Offset: 0x0008F1E4
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public virtual IEnumerator GetEnumerator()
			{
				return base.Rows.GetEnumerator();
			}

			// Token: 0x06002D2A RID: 11562 RVA: 0x00090FF4 File Offset: 0x0008F1F4
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			public override DataTable Clone()
			{
				OptimizerSolutionDataSet.SolutionConstraintValuesDataTable solutionConstraintValuesDataTable = (OptimizerSolutionDataSet.SolutionConstraintValuesDataTable)base.Clone();
				solutionConstraintValuesDataTable.InitVars();
				return solutionConstraintValuesDataTable;
			}

			// Token: 0x06002D2B RID: 11563 RVA: 0x00091014 File Offset: 0x0008F214
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			protected override DataTable CreateInstance()
			{
				return new OptimizerSolutionDataSet.SolutionConstraintValuesDataTable();
			}

			// Token: 0x06002D2C RID: 11564 RVA: 0x0009101C File Offset: 0x0008F21C
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			internal void InitVars()
			{
				this.columnSOLUTION_UID = base.Columns["SOLUTION_UID"];
				this.columnMD_PROP_UID = base.Columns["MD_PROP_UID"];
				this.columnPROJ_UID = base.Columns["PROJ_UID"];
				this.columnNUM_VALUE = base.Columns["NUM_VALUE"];
			}

			// Token: 0x06002D2D RID: 11565 RVA: 0x00091084 File Offset: 0x0008F284
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			private void InitClass()
			{
				this.columnSOLUTION_UID = new DataColumn("SOLUTION_UID", typeof(Guid), null, MappingType.Element);
				base.Columns.Add(this.columnSOLUTION_UID);
				this.columnMD_PROP_UID = new DataColumn("MD_PROP_UID", typeof(Guid), null, MappingType.Element);
				base.Columns.Add(this.columnMD_PROP_UID);
				this.columnPROJ_UID = new DataColumn("PROJ_UID", typeof(Guid), null, MappingType.Element);
				base.Columns.Add(this.columnPROJ_UID);
				this.columnNUM_VALUE = new DataColumn("NUM_VALUE", typeof(decimal), null, MappingType.Element);
				base.Columns.Add(this.columnNUM_VALUE);
				base.Constraints.Add(new UniqueConstraint("PK_SolutionConstraintValues", new DataColumn[]
				{
					this.columnSOLUTION_UID,
					this.columnMD_PROP_UID,
					this.columnPROJ_UID
				}, true));
				this.columnSOLUTION_UID.AllowDBNull = false;
				this.columnMD_PROP_UID.AllowDBNull = false;
				this.columnPROJ_UID.AllowDBNull = false;
				this.columnNUM_VALUE.AllowDBNull = false;
			}

			// Token: 0x06002D2E RID: 11566 RVA: 0x000911AE File Offset: 0x0008F3AE
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public OptimizerSolutionDataSet.SolutionConstraintValuesRow NewSolutionConstraintValuesRow()
			{
				return (OptimizerSolutionDataSet.SolutionConstraintValuesRow)base.NewRow();
			}

			// Token: 0x06002D2F RID: 11567 RVA: 0x000911BB File Offset: 0x0008F3BB
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			protected override DataRow NewRowFromBuilder(DataRowBuilder builder)
			{
				return new OptimizerSolutionDataSet.SolutionConstraintValuesRow(builder);
			}

			// Token: 0x06002D30 RID: 11568 RVA: 0x000911C3 File Offset: 0x0008F3C3
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			protected override Type GetRowType()
			{
				return typeof(OptimizerSolutionDataSet.SolutionConstraintValuesRow);
			}

			// Token: 0x06002D31 RID: 11569 RVA: 0x000911CF File Offset: 0x0008F3CF
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			protected override void OnRowChanged(DataRowChangeEventArgs e)
			{
				base.OnRowChanged(e);
				if (this.SolutionConstraintValuesRowChanged != null)
				{
					this.SolutionConstraintValuesRowChanged(this, new OptimizerSolutionDataSet.SolutionConstraintValuesRowChangeEvent((OptimizerSolutionDataSet.SolutionConstraintValuesRow)e.Row, e.Action));
				}
			}

			// Token: 0x06002D32 RID: 11570 RVA: 0x00091202 File Offset: 0x0008F402
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			protected override void OnRowChanging(DataRowChangeEventArgs e)
			{
				base.OnRowChanging(e);
				if (this.SolutionConstraintValuesRowChanging != null)
				{
					this.SolutionConstraintValuesRowChanging(this, new OptimizerSolutionDataSet.SolutionConstraintValuesRowChangeEvent((OptimizerSolutionDataSet.SolutionConstraintValuesRow)e.Row, e.Action));
				}
			}

			// Token: 0x06002D33 RID: 11571 RVA: 0x00091235 File Offset: 0x0008F435
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			protected override void OnRowDeleted(DataRowChangeEventArgs e)
			{
				base.OnRowDeleted(e);
				if (this.SolutionConstraintValuesRowDeleted != null)
				{
					this.SolutionConstraintValuesRowDeleted(this, new OptimizerSolutionDataSet.SolutionConstraintValuesRowChangeEvent((OptimizerSolutionDataSet.SolutionConstraintValuesRow)e.Row, e.Action));
				}
			}

			// Token: 0x06002D34 RID: 11572 RVA: 0x00091268 File Offset: 0x0008F468
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			protected override void OnRowDeleting(DataRowChangeEventArgs e)
			{
				base.OnRowDeleting(e);
				if (this.SolutionConstraintValuesRowDeleting != null)
				{
					this.SolutionConstraintValuesRowDeleting(this, new OptimizerSolutionDataSet.SolutionConstraintValuesRowChangeEvent((OptimizerSolutionDataSet.SolutionConstraintValuesRow)e.Row, e.Action));
				}
			}

			// Token: 0x06002D35 RID: 11573 RVA: 0x0009129B File Offset: 0x0008F49B
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			public void RemoveSolutionConstraintValuesRow(OptimizerSolutionDataSet.SolutionConstraintValuesRow row)
			{
				base.Rows.Remove(row);
			}

			// Token: 0x06002D36 RID: 11574 RVA: 0x000912AC File Offset: 0x0008F4AC
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public static XmlSchemaComplexType GetTypedTableSchema(XmlSchemaSet xs)
			{
				XmlSchemaComplexType xmlSchemaComplexType = new XmlSchemaComplexType();
				XmlSchemaSequence xmlSchemaSequence = new XmlSchemaSequence();
				OptimizerSolutionDataSet optimizerSolutionDataSet = new OptimizerSolutionDataSet();
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
				xmlSchemaAttribute.FixedValue = optimizerSolutionDataSet.Namespace;
				xmlSchemaComplexType.Attributes.Add(xmlSchemaAttribute);
				XmlSchemaAttribute xmlSchemaAttribute2 = new XmlSchemaAttribute();
				xmlSchemaAttribute2.Name = "tableTypeName";
				xmlSchemaAttribute2.FixedValue = "SolutionConstraintValuesDataTable";
				xmlSchemaComplexType.Attributes.Add(xmlSchemaAttribute2);
				xmlSchemaComplexType.Particle = xmlSchemaSequence;
				XmlSchema schemaSerializable = optimizerSolutionDataSet.GetSchemaSerializable();
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

			// Token: 0x04000988 RID: 2440
			private DataColumn columnSOLUTION_UID;

			// Token: 0x04000989 RID: 2441
			private DataColumn columnMD_PROP_UID;

			// Token: 0x0400098A RID: 2442
			private DataColumn columnPROJ_UID;

			// Token: 0x0400098B RID: 2443
			private DataColumn columnNUM_VALUE;
		}

		// Token: 0x02000230 RID: 560
		[XmlSchemaProvider("GetTypedTableSchema")]
		[Serializable]
		public class SolutionProjectsDataTable : DataTable, IEnumerable
		{
			// Token: 0x06002D37 RID: 11575 RVA: 0x000914A4 File Offset: 0x0008F6A4
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			public SolutionProjectsDataTable()
			{
				base.TableName = "SolutionProjects";
				this.BeginInit();
				this.InitClass();
				this.EndInit();
			}

			// Token: 0x06002D38 RID: 11576 RVA: 0x000914CC File Offset: 0x0008F6CC
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
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

			// Token: 0x06002D39 RID: 11577 RVA: 0x00091574 File Offset: 0x0008F774
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			protected SolutionProjectsDataTable(SerializationInfo info, StreamingContext context) : base(info, context)
			{
				this.InitVars();
			}

			// Token: 0x17000D41 RID: 3393
			// (get) Token: 0x06002D3A RID: 11578 RVA: 0x00091584 File Offset: 0x0008F784
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			public DataColumn SOLUTION_UIDColumn
			{
				get
				{
					return this.columnSOLUTION_UID;
				}
			}

			// Token: 0x17000D42 RID: 3394
			// (get) Token: 0x06002D3B RID: 11579 RVA: 0x0009158C File Offset: 0x0008F78C
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public DataColumn PROJ_UIDColumn
			{
				get
				{
					return this.columnPROJ_UID;
				}
			}

			// Token: 0x17000D43 RID: 3395
			// (get) Token: 0x06002D3C RID: 11580 RVA: 0x00091594 File Offset: 0x0008F794
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			public DataColumn PROJ_NAMEColumn
			{
				get
				{
					return this.columnPROJ_NAME;
				}
			}

			// Token: 0x17000D44 RID: 3396
			// (get) Token: 0x06002D3D RID: 11581 RVA: 0x0009159C File Offset: 0x0008F79C
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public DataColumn PRIORITYColumn
			{
				get
				{
					return this.columnPRIORITY;
				}
			}

			// Token: 0x17000D45 RID: 3397
			// (get) Token: 0x06002D3E RID: 11582 RVA: 0x000915A4 File Offset: 0x0008F7A4
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			public DataColumn ABSOLUTE_PRIORITYColumn
			{
				get
				{
					return this.columnABSOLUTE_PRIORITY;
				}
			}

			// Token: 0x17000D46 RID: 3398
			// (get) Token: 0x06002D3F RID: 11583 RVA: 0x000915AC File Offset: 0x0008F7AC
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public DataColumn STATUSColumn
			{
				get
				{
					return this.columnSTATUS;
				}
			}

			// Token: 0x17000D47 RID: 3399
			// (get) Token: 0x06002D40 RID: 11584 RVA: 0x000915B4 File Offset: 0x0008F7B4
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public DataColumn FORCE_STATUSColumn
			{
				get
				{
					return this.columnFORCE_STATUS;
				}
			}

			// Token: 0x17000D48 RID: 3400
			// (get) Token: 0x06002D41 RID: 11585 RVA: 0x000915BC File Offset: 0x0008F7BC
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			public DataColumn FORCE_ALIAS_LT_STRUCT_UIDColumn
			{
				get
				{
					return this.columnFORCE_ALIAS_LT_STRUCT_UID;
				}
			}

			// Token: 0x17000D49 RID: 3401
			// (get) Token: 0x06002D42 RID: 11586 RVA: 0x000915C4 File Offset: 0x0008F7C4
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			public DataColumn FORCE_ALIAS_LT_VALUE_FULLColumn
			{
				get
				{
					return this.columnFORCE_ALIAS_LT_VALUE_FULL;
				}
			}

			// Token: 0x17000D4A RID: 3402
			// (get) Token: 0x06002D43 RID: 11587 RVA: 0x000915CC File Offset: 0x0008F7CC
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

			// Token: 0x17000D4B RID: 3403
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public OptimizerSolutionDataSet.SolutionProjectsRow this[int index]
			{
				get
				{
					return (OptimizerSolutionDataSet.SolutionProjectsRow)base.Rows[index];
				}
			}

			// Token: 0x140001E1 RID: 481
			// (add) Token: 0x06002D45 RID: 11589 RVA: 0x000915EC File Offset: 0x0008F7EC
			// (remove) Token: 0x06002D46 RID: 11590 RVA: 0x00091624 File Offset: 0x0008F824
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public event OptimizerSolutionDataSet.SolutionProjectsRowChangeEventHandler SolutionProjectsRowChanging;

			// Token: 0x140001E2 RID: 482
			// (add) Token: 0x06002D47 RID: 11591 RVA: 0x0009165C File Offset: 0x0008F85C
			// (remove) Token: 0x06002D48 RID: 11592 RVA: 0x00091694 File Offset: 0x0008F894
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public event OptimizerSolutionDataSet.SolutionProjectsRowChangeEventHandler SolutionProjectsRowChanged;

			// Token: 0x140001E3 RID: 483
			// (add) Token: 0x06002D49 RID: 11593 RVA: 0x000916CC File Offset: 0x0008F8CC
			// (remove) Token: 0x06002D4A RID: 11594 RVA: 0x00091704 File Offset: 0x0008F904
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public event OptimizerSolutionDataSet.SolutionProjectsRowChangeEventHandler SolutionProjectsRowDeleting;

			// Token: 0x140001E4 RID: 484
			// (add) Token: 0x06002D4B RID: 11595 RVA: 0x0009173C File Offset: 0x0008F93C
			// (remove) Token: 0x06002D4C RID: 11596 RVA: 0x00091774 File Offset: 0x0008F974
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public event OptimizerSolutionDataSet.SolutionProjectsRowChangeEventHandler SolutionProjectsRowDeleted;

			// Token: 0x06002D4D RID: 11597 RVA: 0x000917A9 File Offset: 0x0008F9A9
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			public void AddSolutionProjectsRow(OptimizerSolutionDataSet.SolutionProjectsRow row)
			{
				base.Rows.Add(row);
			}

			// Token: 0x06002D4E RID: 11598 RVA: 0x000917B8 File Offset: 0x0008F9B8
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			public OptimizerSolutionDataSet.SolutionProjectsRow AddSolutionProjectsRow(OptimizerSolutionDataSet.SolutionRow parentSolutionRowByFK_Solution_SolutionProjects, Guid PROJ_UID, string PROJ_NAME, double PRIORITY, double ABSOLUTE_PRIORITY, byte STATUS, byte FORCE_STATUS, Guid FORCE_ALIAS_LT_STRUCT_UID, string FORCE_ALIAS_LT_VALUE_FULL)
			{
				OptimizerSolutionDataSet.SolutionProjectsRow solutionProjectsRow = (OptimizerSolutionDataSet.SolutionProjectsRow)base.NewRow();
				object[] array = new object[]
				{
					null,
					PROJ_UID,
					PROJ_NAME,
					PRIORITY,
					ABSOLUTE_PRIORITY,
					STATUS,
					FORCE_STATUS,
					FORCE_ALIAS_LT_STRUCT_UID,
					FORCE_ALIAS_LT_VALUE_FULL
				};
				if (parentSolutionRowByFK_Solution_SolutionProjects != null)
				{
					array[0] = parentSolutionRowByFK_Solution_SolutionProjects[1];
				}
				solutionProjectsRow.ItemArray = array;
				base.Rows.Add(solutionProjectsRow);
				return solutionProjectsRow;
			}

			// Token: 0x06002D4F RID: 11599 RVA: 0x00091840 File Offset: 0x0008FA40
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			public OptimizerSolutionDataSet.SolutionProjectsRow FindBySOLUTION_UIDPROJ_UID(Guid SOLUTION_UID, Guid PROJ_UID)
			{
				return (OptimizerSolutionDataSet.SolutionProjectsRow)base.Rows.Find(new object[]
				{
					SOLUTION_UID,
					PROJ_UID
				});
			}

			// Token: 0x06002D50 RID: 11600 RVA: 0x00091877 File Offset: 0x0008FA77
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			public virtual IEnumerator GetEnumerator()
			{
				return base.Rows.GetEnumerator();
			}

			// Token: 0x06002D51 RID: 11601 RVA: 0x00091884 File Offset: 0x0008FA84
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			public override DataTable Clone()
			{
				OptimizerSolutionDataSet.SolutionProjectsDataTable solutionProjectsDataTable = (OptimizerSolutionDataSet.SolutionProjectsDataTable)base.Clone();
				solutionProjectsDataTable.InitVars();
				return solutionProjectsDataTable;
			}

			// Token: 0x06002D52 RID: 11602 RVA: 0x000918A4 File Offset: 0x0008FAA4
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			protected override DataTable CreateInstance()
			{
				return new OptimizerSolutionDataSet.SolutionProjectsDataTable();
			}

			// Token: 0x06002D53 RID: 11603 RVA: 0x000918AC File Offset: 0x0008FAAC
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			internal void InitVars()
			{
				this.columnSOLUTION_UID = base.Columns["SOLUTION_UID"];
				this.columnPROJ_UID = base.Columns["PROJ_UID"];
				this.columnPROJ_NAME = base.Columns["PROJ_NAME"];
				this.columnPRIORITY = base.Columns["PRIORITY"];
				this.columnABSOLUTE_PRIORITY = base.Columns["ABSOLUTE_PRIORITY"];
				this.columnSTATUS = base.Columns["STATUS"];
				this.columnFORCE_STATUS = base.Columns["FORCE_STATUS"];
				this.columnFORCE_ALIAS_LT_STRUCT_UID = base.Columns["FORCE_ALIAS_LT_STRUCT_UID"];
				this.columnFORCE_ALIAS_LT_VALUE_FULL = base.Columns["FORCE_ALIAS_LT_VALUE_FULL"];
			}

			// Token: 0x06002D54 RID: 11604 RVA: 0x00091980 File Offset: 0x0008FB80
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			private void InitClass()
			{
				this.columnSOLUTION_UID = new DataColumn("SOLUTION_UID", typeof(Guid), null, MappingType.Element);
				base.Columns.Add(this.columnSOLUTION_UID);
				this.columnPROJ_UID = new DataColumn("PROJ_UID", typeof(Guid), null, MappingType.Element);
				base.Columns.Add(this.columnPROJ_UID);
				this.columnPROJ_NAME = new DataColumn("PROJ_NAME", typeof(string), null, MappingType.Element);
				base.Columns.Add(this.columnPROJ_NAME);
				this.columnPRIORITY = new DataColumn("PRIORITY", typeof(double), null, MappingType.Element);
				base.Columns.Add(this.columnPRIORITY);
				this.columnABSOLUTE_PRIORITY = new DataColumn("ABSOLUTE_PRIORITY", typeof(double), null, MappingType.Element);
				base.Columns.Add(this.columnABSOLUTE_PRIORITY);
				this.columnSTATUS = new DataColumn("STATUS", typeof(byte), null, MappingType.Element);
				base.Columns.Add(this.columnSTATUS);
				this.columnFORCE_STATUS = new DataColumn("FORCE_STATUS", typeof(byte), null, MappingType.Element);
				base.Columns.Add(this.columnFORCE_STATUS);
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
				this.columnPROJ_NAME.ReadOnly = true;
				this.columnPRIORITY.ReadOnly = true;
				this.columnABSOLUTE_PRIORITY.ReadOnly = true;
				this.columnFORCE_STATUS.AllowDBNull = false;
				this.columnFORCE_STATUS.DefaultValue = 2;
				this.columnFORCE_ALIAS_LT_VALUE_FULL.ReadOnly = true;
			}

			// Token: 0x06002D55 RID: 11605 RVA: 0x00091BB7 File Offset: 0x0008FDB7
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			public OptimizerSolutionDataSet.SolutionProjectsRow NewSolutionProjectsRow()
			{
				return (OptimizerSolutionDataSet.SolutionProjectsRow)base.NewRow();
			}

			// Token: 0x06002D56 RID: 11606 RVA: 0x00091BC4 File Offset: 0x0008FDC4
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			protected override DataRow NewRowFromBuilder(DataRowBuilder builder)
			{
				return new OptimizerSolutionDataSet.SolutionProjectsRow(builder);
			}

			// Token: 0x06002D57 RID: 11607 RVA: 0x00091BCC File Offset: 0x0008FDCC
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			protected override Type GetRowType()
			{
				return typeof(OptimizerSolutionDataSet.SolutionProjectsRow);
			}

			// Token: 0x06002D58 RID: 11608 RVA: 0x00091BD8 File Offset: 0x0008FDD8
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			protected override void OnRowChanged(DataRowChangeEventArgs e)
			{
				base.OnRowChanged(e);
				if (this.SolutionProjectsRowChanged != null)
				{
					this.SolutionProjectsRowChanged(this, new OptimizerSolutionDataSet.SolutionProjectsRowChangeEvent((OptimizerSolutionDataSet.SolutionProjectsRow)e.Row, e.Action));
				}
			}

			// Token: 0x06002D59 RID: 11609 RVA: 0x00091C0B File Offset: 0x0008FE0B
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			protected override void OnRowChanging(DataRowChangeEventArgs e)
			{
				base.OnRowChanging(e);
				if (this.SolutionProjectsRowChanging != null)
				{
					this.SolutionProjectsRowChanging(this, new OptimizerSolutionDataSet.SolutionProjectsRowChangeEvent((OptimizerSolutionDataSet.SolutionProjectsRow)e.Row, e.Action));
				}
			}

			// Token: 0x06002D5A RID: 11610 RVA: 0x00091C3E File Offset: 0x0008FE3E
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			protected override void OnRowDeleted(DataRowChangeEventArgs e)
			{
				base.OnRowDeleted(e);
				if (this.SolutionProjectsRowDeleted != null)
				{
					this.SolutionProjectsRowDeleted(this, new OptimizerSolutionDataSet.SolutionProjectsRowChangeEvent((OptimizerSolutionDataSet.SolutionProjectsRow)e.Row, e.Action));
				}
			}

			// Token: 0x06002D5B RID: 11611 RVA: 0x00091C71 File Offset: 0x0008FE71
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			protected override void OnRowDeleting(DataRowChangeEventArgs e)
			{
				base.OnRowDeleting(e);
				if (this.SolutionProjectsRowDeleting != null)
				{
					this.SolutionProjectsRowDeleting(this, new OptimizerSolutionDataSet.SolutionProjectsRowChangeEvent((OptimizerSolutionDataSet.SolutionProjectsRow)e.Row, e.Action));
				}
			}

			// Token: 0x06002D5C RID: 11612 RVA: 0x00091CA4 File Offset: 0x0008FEA4
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public void RemoveSolutionProjectsRow(OptimizerSolutionDataSet.SolutionProjectsRow row)
			{
				base.Rows.Remove(row);
			}

			// Token: 0x06002D5D RID: 11613 RVA: 0x00091CB4 File Offset: 0x0008FEB4
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			public static XmlSchemaComplexType GetTypedTableSchema(XmlSchemaSet xs)
			{
				XmlSchemaComplexType xmlSchemaComplexType = new XmlSchemaComplexType();
				XmlSchemaSequence xmlSchemaSequence = new XmlSchemaSequence();
				OptimizerSolutionDataSet optimizerSolutionDataSet = new OptimizerSolutionDataSet();
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
				xmlSchemaAttribute.FixedValue = optimizerSolutionDataSet.Namespace;
				xmlSchemaComplexType.Attributes.Add(xmlSchemaAttribute);
				XmlSchemaAttribute xmlSchemaAttribute2 = new XmlSchemaAttribute();
				xmlSchemaAttribute2.Name = "tableTypeName";
				xmlSchemaAttribute2.FixedValue = "SolutionProjectsDataTable";
				xmlSchemaComplexType.Attributes.Add(xmlSchemaAttribute2);
				xmlSchemaComplexType.Particle = xmlSchemaSequence;
				XmlSchema schemaSerializable = optimizerSolutionDataSet.GetSchemaSerializable();
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

			// Token: 0x04000990 RID: 2448
			private DataColumn columnSOLUTION_UID;

			// Token: 0x04000991 RID: 2449
			private DataColumn columnPROJ_UID;

			// Token: 0x04000992 RID: 2450
			private DataColumn columnPROJ_NAME;

			// Token: 0x04000993 RID: 2451
			private DataColumn columnPRIORITY;

			// Token: 0x04000994 RID: 2452
			private DataColumn columnABSOLUTE_PRIORITY;

			// Token: 0x04000995 RID: 2453
			private DataColumn columnSTATUS;

			// Token: 0x04000996 RID: 2454
			private DataColumn columnFORCE_STATUS;

			// Token: 0x04000997 RID: 2455
			private DataColumn columnFORCE_ALIAS_LT_STRUCT_UID;

			// Token: 0x04000998 RID: 2456
			private DataColumn columnFORCE_ALIAS_LT_VALUE_FULL;
		}

		// Token: 0x02000231 RID: 561
		[XmlSchemaProvider("GetTypedTableSchema")]
		[Serializable]
		public class SolutionStrategicAlignmentDataTable : DataTable, IEnumerable
		{
			// Token: 0x06002D5E RID: 11614 RVA: 0x00091EAC File Offset: 0x000900AC
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public SolutionStrategicAlignmentDataTable()
			{
				base.TableName = "SolutionStrategicAlignment";
				this.BeginInit();
				this.InitClass();
				this.EndInit();
			}

			// Token: 0x06002D5F RID: 11615 RVA: 0x00091ED4 File Offset: 0x000900D4
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			internal SolutionStrategicAlignmentDataTable(DataTable table)
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

			// Token: 0x06002D60 RID: 11616 RVA: 0x00091F7C File Offset: 0x0009017C
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			protected SolutionStrategicAlignmentDataTable(SerializationInfo info, StreamingContext context) : base(info, context)
			{
				this.InitVars();
			}

			// Token: 0x17000D4C RID: 3404
			// (get) Token: 0x06002D61 RID: 11617 RVA: 0x00091F8C File Offset: 0x0009018C
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			public DataColumn SOLUTION_UIDColumn
			{
				get
				{
					return this.columnSOLUTION_UID;
				}
			}

			// Token: 0x17000D4D RID: 3405
			// (get) Token: 0x06002D62 RID: 11618 RVA: 0x00091F94 File Offset: 0x00090194
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public DataColumn DRIVER_UIDColumn
			{
				get
				{
					return this.columnDRIVER_UID;
				}
			}

			// Token: 0x17000D4E RID: 3406
			// (get) Token: 0x06002D63 RID: 11619 RVA: 0x00091F9C File Offset: 0x0009019C
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public DataColumn DRIVER_NAMEColumn
			{
				get
				{
					return this.columnDRIVER_NAME;
				}
			}

			// Token: 0x17000D4F RID: 3407
			// (get) Token: 0x06002D64 RID: 11620 RVA: 0x00091FA4 File Offset: 0x000901A4
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public DataColumn DRIVER_PRIORITYColumn
			{
				get
				{
					return this.columnDRIVER_PRIORITY;
				}
			}

			// Token: 0x17000D50 RID: 3408
			// (get) Token: 0x06002D65 RID: 11621 RVA: 0x00091FAC File Offset: 0x000901AC
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			public DataColumn REVERSE_VALUEColumn
			{
				get
				{
					return this.columnREVERSE_VALUE;
				}
			}

			// Token: 0x17000D51 RID: 3409
			// (get) Token: 0x06002D66 RID: 11622 RVA: 0x00091FB4 File Offset: 0x000901B4
			[Browsable(false)]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			public int Count
			{
				get
				{
					return base.Rows.Count;
				}
			}

			// Token: 0x17000D52 RID: 3410
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			public OptimizerSolutionDataSet.SolutionStrategicAlignmentRow this[int index]
			{
				get
				{
					return (OptimizerSolutionDataSet.SolutionStrategicAlignmentRow)base.Rows[index];
				}
			}

			// Token: 0x140001E5 RID: 485
			// (add) Token: 0x06002D68 RID: 11624 RVA: 0x00091FD4 File Offset: 0x000901D4
			// (remove) Token: 0x06002D69 RID: 11625 RVA: 0x0009200C File Offset: 0x0009020C
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public event OptimizerSolutionDataSet.SolutionStrategicAlignmentRowChangeEventHandler SolutionStrategicAlignmentRowChanging;

			// Token: 0x140001E6 RID: 486
			// (add) Token: 0x06002D6A RID: 11626 RVA: 0x00092044 File Offset: 0x00090244
			// (remove) Token: 0x06002D6B RID: 11627 RVA: 0x0009207C File Offset: 0x0009027C
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public event OptimizerSolutionDataSet.SolutionStrategicAlignmentRowChangeEventHandler SolutionStrategicAlignmentRowChanged;

			// Token: 0x140001E7 RID: 487
			// (add) Token: 0x06002D6C RID: 11628 RVA: 0x000920B4 File Offset: 0x000902B4
			// (remove) Token: 0x06002D6D RID: 11629 RVA: 0x000920EC File Offset: 0x000902EC
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public event OptimizerSolutionDataSet.SolutionStrategicAlignmentRowChangeEventHandler SolutionStrategicAlignmentRowDeleting;

			// Token: 0x140001E8 RID: 488
			// (add) Token: 0x06002D6E RID: 11630 RVA: 0x00092124 File Offset: 0x00090324
			// (remove) Token: 0x06002D6F RID: 11631 RVA: 0x0009215C File Offset: 0x0009035C
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public event OptimizerSolutionDataSet.SolutionStrategicAlignmentRowChangeEventHandler SolutionStrategicAlignmentRowDeleted;

			// Token: 0x06002D70 RID: 11632 RVA: 0x00092191 File Offset: 0x00090391
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			public void AddSolutionStrategicAlignmentRow(OptimizerSolutionDataSet.SolutionStrategicAlignmentRow row)
			{
				base.Rows.Add(row);
			}

			// Token: 0x06002D71 RID: 11633 RVA: 0x000921A0 File Offset: 0x000903A0
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public OptimizerSolutionDataSet.SolutionStrategicAlignmentRow AddSolutionStrategicAlignmentRow(OptimizerSolutionDataSet.SolutionRow parentSolutionRowByFK_Solution_SolutionStrategicAlignment, Guid DRIVER_UID, string DRIVER_NAME, double DRIVER_PRIORITY, decimal REVERSE_VALUE)
			{
				OptimizerSolutionDataSet.SolutionStrategicAlignmentRow solutionStrategicAlignmentRow = (OptimizerSolutionDataSet.SolutionStrategicAlignmentRow)base.NewRow();
				object[] array = new object[]
				{
					null,
					DRIVER_UID,
					DRIVER_NAME,
					DRIVER_PRIORITY,
					REVERSE_VALUE
				};
				if (parentSolutionRowByFK_Solution_SolutionStrategicAlignment != null)
				{
					array[0] = parentSolutionRowByFK_Solution_SolutionStrategicAlignment[1];
				}
				solutionStrategicAlignmentRow.ItemArray = array;
				base.Rows.Add(solutionStrategicAlignmentRow);
				return solutionStrategicAlignmentRow;
			}

			// Token: 0x06002D72 RID: 11634 RVA: 0x00092204 File Offset: 0x00090404
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			public OptimizerSolutionDataSet.SolutionStrategicAlignmentRow FindBySOLUTION_UIDDRIVER_UID(Guid SOLUTION_UID, Guid DRIVER_UID)
			{
				return (OptimizerSolutionDataSet.SolutionStrategicAlignmentRow)base.Rows.Find(new object[]
				{
					SOLUTION_UID,
					DRIVER_UID
				});
			}

			// Token: 0x06002D73 RID: 11635 RVA: 0x0009223B File Offset: 0x0009043B
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public virtual IEnumerator GetEnumerator()
			{
				return base.Rows.GetEnumerator();
			}

			// Token: 0x06002D74 RID: 11636 RVA: 0x00092248 File Offset: 0x00090448
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public override DataTable Clone()
			{
				OptimizerSolutionDataSet.SolutionStrategicAlignmentDataTable solutionStrategicAlignmentDataTable = (OptimizerSolutionDataSet.SolutionStrategicAlignmentDataTable)base.Clone();
				solutionStrategicAlignmentDataTable.InitVars();
				return solutionStrategicAlignmentDataTable;
			}

			// Token: 0x06002D75 RID: 11637 RVA: 0x00092268 File Offset: 0x00090468
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			protected override DataTable CreateInstance()
			{
				return new OptimizerSolutionDataSet.SolutionStrategicAlignmentDataTable();
			}

			// Token: 0x06002D76 RID: 11638 RVA: 0x00092270 File Offset: 0x00090470
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			internal void InitVars()
			{
				this.columnSOLUTION_UID = base.Columns["SOLUTION_UID"];
				this.columnDRIVER_UID = base.Columns["DRIVER_UID"];
				this.columnDRIVER_NAME = base.Columns["DRIVER_NAME"];
				this.columnDRIVER_PRIORITY = base.Columns["DRIVER_PRIORITY"];
				this.columnREVERSE_VALUE = base.Columns["REVERSE_VALUE"];
			}

			// Token: 0x06002D77 RID: 11639 RVA: 0x000922EC File Offset: 0x000904EC
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			private void InitClass()
			{
				this.columnSOLUTION_UID = new DataColumn("SOLUTION_UID", typeof(Guid), null, MappingType.Element);
				base.Columns.Add(this.columnSOLUTION_UID);
				this.columnDRIVER_UID = new DataColumn("DRIVER_UID", typeof(Guid), null, MappingType.Element);
				base.Columns.Add(this.columnDRIVER_UID);
				this.columnDRIVER_NAME = new DataColumn("DRIVER_NAME", typeof(string), null, MappingType.Element);
				base.Columns.Add(this.columnDRIVER_NAME);
				this.columnDRIVER_PRIORITY = new DataColumn("DRIVER_PRIORITY", typeof(double), null, MappingType.Element);
				base.Columns.Add(this.columnDRIVER_PRIORITY);
				this.columnREVERSE_VALUE = new DataColumn("REVERSE_VALUE", typeof(decimal), null, MappingType.Element);
				base.Columns.Add(this.columnREVERSE_VALUE);
				base.Constraints.Add(new UniqueConstraint("Constraint1", new DataColumn[]
				{
					this.columnSOLUTION_UID,
					this.columnDRIVER_UID
				}, true));
				this.columnSOLUTION_UID.AllowDBNull = false;
				this.columnSOLUTION_UID.ReadOnly = true;
				this.columnDRIVER_UID.AllowDBNull = false;
				this.columnDRIVER_UID.ReadOnly = true;
				this.columnDRIVER_NAME.ReadOnly = true;
				this.columnDRIVER_PRIORITY.AllowDBNull = false;
				this.columnDRIVER_PRIORITY.ReadOnly = true;
				this.columnREVERSE_VALUE.AllowDBNull = false;
				this.columnREVERSE_VALUE.ReadOnly = true;
			}

			// Token: 0x06002D78 RID: 11640 RVA: 0x00092476 File Offset: 0x00090676
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public OptimizerSolutionDataSet.SolutionStrategicAlignmentRow NewSolutionStrategicAlignmentRow()
			{
				return (OptimizerSolutionDataSet.SolutionStrategicAlignmentRow)base.NewRow();
			}

			// Token: 0x06002D79 RID: 11641 RVA: 0x00092483 File Offset: 0x00090683
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			protected override DataRow NewRowFromBuilder(DataRowBuilder builder)
			{
				return new OptimizerSolutionDataSet.SolutionStrategicAlignmentRow(builder);
			}

			// Token: 0x06002D7A RID: 11642 RVA: 0x0009248B File Offset: 0x0009068B
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			protected override Type GetRowType()
			{
				return typeof(OptimizerSolutionDataSet.SolutionStrategicAlignmentRow);
			}

			// Token: 0x06002D7B RID: 11643 RVA: 0x00092497 File Offset: 0x00090697
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			protected override void OnRowChanged(DataRowChangeEventArgs e)
			{
				base.OnRowChanged(e);
				if (this.SolutionStrategicAlignmentRowChanged != null)
				{
					this.SolutionStrategicAlignmentRowChanged(this, new OptimizerSolutionDataSet.SolutionStrategicAlignmentRowChangeEvent((OptimizerSolutionDataSet.SolutionStrategicAlignmentRow)e.Row, e.Action));
				}
			}

			// Token: 0x06002D7C RID: 11644 RVA: 0x000924CA File Offset: 0x000906CA
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			protected override void OnRowChanging(DataRowChangeEventArgs e)
			{
				base.OnRowChanging(e);
				if (this.SolutionStrategicAlignmentRowChanging != null)
				{
					this.SolutionStrategicAlignmentRowChanging(this, new OptimizerSolutionDataSet.SolutionStrategicAlignmentRowChangeEvent((OptimizerSolutionDataSet.SolutionStrategicAlignmentRow)e.Row, e.Action));
				}
			}

			// Token: 0x06002D7D RID: 11645 RVA: 0x000924FD File Offset: 0x000906FD
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			protected override void OnRowDeleted(DataRowChangeEventArgs e)
			{
				base.OnRowDeleted(e);
				if (this.SolutionStrategicAlignmentRowDeleted != null)
				{
					this.SolutionStrategicAlignmentRowDeleted(this, new OptimizerSolutionDataSet.SolutionStrategicAlignmentRowChangeEvent((OptimizerSolutionDataSet.SolutionStrategicAlignmentRow)e.Row, e.Action));
				}
			}

			// Token: 0x06002D7E RID: 11646 RVA: 0x00092530 File Offset: 0x00090730
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			protected override void OnRowDeleting(DataRowChangeEventArgs e)
			{
				base.OnRowDeleting(e);
				if (this.SolutionStrategicAlignmentRowDeleting != null)
				{
					this.SolutionStrategicAlignmentRowDeleting(this, new OptimizerSolutionDataSet.SolutionStrategicAlignmentRowChangeEvent((OptimizerSolutionDataSet.SolutionStrategicAlignmentRow)e.Row, e.Action));
				}
			}

			// Token: 0x06002D7F RID: 11647 RVA: 0x00092563 File Offset: 0x00090763
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public void RemoveSolutionStrategicAlignmentRow(OptimizerSolutionDataSet.SolutionStrategicAlignmentRow row)
			{
				base.Rows.Remove(row);
			}

			// Token: 0x06002D80 RID: 11648 RVA: 0x00092574 File Offset: 0x00090774
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public static XmlSchemaComplexType GetTypedTableSchema(XmlSchemaSet xs)
			{
				XmlSchemaComplexType xmlSchemaComplexType = new XmlSchemaComplexType();
				XmlSchemaSequence xmlSchemaSequence = new XmlSchemaSequence();
				OptimizerSolutionDataSet optimizerSolutionDataSet = new OptimizerSolutionDataSet();
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
				xmlSchemaAttribute.FixedValue = optimizerSolutionDataSet.Namespace;
				xmlSchemaComplexType.Attributes.Add(xmlSchemaAttribute);
				XmlSchemaAttribute xmlSchemaAttribute2 = new XmlSchemaAttribute();
				xmlSchemaAttribute2.Name = "tableTypeName";
				xmlSchemaAttribute2.FixedValue = "SolutionStrategicAlignmentDataTable";
				xmlSchemaComplexType.Attributes.Add(xmlSchemaAttribute2);
				xmlSchemaComplexType.Particle = xmlSchemaSequence;
				XmlSchema schemaSerializable = optimizerSolutionDataSet.GetSchemaSerializable();
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

			// Token: 0x0400099D RID: 2461
			private DataColumn columnSOLUTION_UID;

			// Token: 0x0400099E RID: 2462
			private DataColumn columnDRIVER_UID;

			// Token: 0x0400099F RID: 2463
			private DataColumn columnDRIVER_NAME;

			// Token: 0x040009A0 RID: 2464
			private DataColumn columnDRIVER_PRIORITY;

			// Token: 0x040009A1 RID: 2465
			private DataColumn columnREVERSE_VALUE;
		}

		// Token: 0x02000232 RID: 562
		[XmlSchemaProvider("GetTypedTableSchema")]
		[Serializable]
		public class SolutionEfficientFrontierDataTable : DataTable, IEnumerable
		{
			// Token: 0x06002D81 RID: 11649 RVA: 0x0009276C File Offset: 0x0009096C
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			public SolutionEfficientFrontierDataTable()
			{
				base.TableName = "SolutionEfficientFrontier";
				this.BeginInit();
				this.InitClass();
				this.EndInit();
			}

			// Token: 0x06002D82 RID: 11650 RVA: 0x00092794 File Offset: 0x00090994
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

			// Token: 0x06002D83 RID: 11651 RVA: 0x0009283C File Offset: 0x00090A3C
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			protected SolutionEfficientFrontierDataTable(SerializationInfo info, StreamingContext context) : base(info, context)
			{
				this.InitVars();
			}

			// Token: 0x17000D53 RID: 3411
			// (get) Token: 0x06002D84 RID: 11652 RVA: 0x0009284C File Offset: 0x00090A4C
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			public DataColumn FRONTIER_UIDColumn
			{
				get
				{
					return this.columnFRONTIER_UID;
				}
			}

			// Token: 0x17000D54 RID: 3412
			// (get) Token: 0x06002D85 RID: 11653 RVA: 0x00092854 File Offset: 0x00090A54
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			public DataColumn ANALYSIS_UIDColumn
			{
				get
				{
					return this.columnANALYSIS_UID;
				}
			}

			// Token: 0x17000D55 RID: 3413
			// (get) Token: 0x06002D86 RID: 11654 RVA: 0x0009285C File Offset: 0x00090A5C
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			public DataColumn POINT_UIDColumn
			{
				get
				{
					return this.columnPOINT_UID;
				}
			}

			// Token: 0x17000D56 RID: 3414
			// (get) Token: 0x06002D87 RID: 11655 RVA: 0x00092864 File Offset: 0x00090A64
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			public DataColumn X_VALUEColumn
			{
				get
				{
					return this.columnX_VALUE;
				}
			}

			// Token: 0x17000D57 RID: 3415
			// (get) Token: 0x06002D88 RID: 11656 RVA: 0x0009286C File Offset: 0x00090A6C
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			public DataColumn Y_VALUEColumn
			{
				get
				{
					return this.columnY_VALUE;
				}
			}

			// Token: 0x17000D58 RID: 3416
			// (get) Token: 0x06002D89 RID: 11657 RVA: 0x00092874 File Offset: 0x00090A74
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

			// Token: 0x17000D59 RID: 3417
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			public OptimizerSolutionDataSet.SolutionEfficientFrontierRow this[int index]
			{
				get
				{
					return (OptimizerSolutionDataSet.SolutionEfficientFrontierRow)base.Rows[index];
				}
			}

			// Token: 0x140001E9 RID: 489
			// (add) Token: 0x06002D8B RID: 11659 RVA: 0x00092894 File Offset: 0x00090A94
			// (remove) Token: 0x06002D8C RID: 11660 RVA: 0x000928CC File Offset: 0x00090ACC
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public event OptimizerSolutionDataSet.SolutionEfficientFrontierRowChangeEventHandler SolutionEfficientFrontierRowChanging;

			// Token: 0x140001EA RID: 490
			// (add) Token: 0x06002D8D RID: 11661 RVA: 0x00092904 File Offset: 0x00090B04
			// (remove) Token: 0x06002D8E RID: 11662 RVA: 0x0009293C File Offset: 0x00090B3C
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public event OptimizerSolutionDataSet.SolutionEfficientFrontierRowChangeEventHandler SolutionEfficientFrontierRowChanged;

			// Token: 0x140001EB RID: 491
			// (add) Token: 0x06002D8F RID: 11663 RVA: 0x00092974 File Offset: 0x00090B74
			// (remove) Token: 0x06002D90 RID: 11664 RVA: 0x000929AC File Offset: 0x00090BAC
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public event OptimizerSolutionDataSet.SolutionEfficientFrontierRowChangeEventHandler SolutionEfficientFrontierRowDeleting;

			// Token: 0x140001EC RID: 492
			// (add) Token: 0x06002D91 RID: 11665 RVA: 0x000929E4 File Offset: 0x00090BE4
			// (remove) Token: 0x06002D92 RID: 11666 RVA: 0x00092A1C File Offset: 0x00090C1C
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public event OptimizerSolutionDataSet.SolutionEfficientFrontierRowChangeEventHandler SolutionEfficientFrontierRowDeleted;

			// Token: 0x06002D93 RID: 11667 RVA: 0x00092A51 File Offset: 0x00090C51
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			public void AddSolutionEfficientFrontierRow(OptimizerSolutionDataSet.SolutionEfficientFrontierRow row)
			{
				base.Rows.Add(row);
			}

			// Token: 0x06002D94 RID: 11668 RVA: 0x00092A60 File Offset: 0x00090C60
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			public OptimizerSolutionDataSet.SolutionEfficientFrontierRow AddSolutionEfficientFrontierRow(Guid FRONTIER_UID, Guid ANALYSIS_UID, Guid POINT_UID, decimal X_VALUE, decimal Y_VALUE)
			{
				OptimizerSolutionDataSet.SolutionEfficientFrontierRow solutionEfficientFrontierRow = (OptimizerSolutionDataSet.SolutionEfficientFrontierRow)base.NewRow();
				object[] itemArray = new object[]
				{
					FRONTIER_UID,
					ANALYSIS_UID,
					POINT_UID,
					X_VALUE,
					Y_VALUE
				};
				solutionEfficientFrontierRow.ItemArray = itemArray;
				base.Rows.Add(solutionEfficientFrontierRow);
				return solutionEfficientFrontierRow;
			}

			// Token: 0x06002D95 RID: 11669 RVA: 0x00092AC8 File Offset: 0x00090CC8
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			public OptimizerSolutionDataSet.SolutionEfficientFrontierRow FindByPOINT_UID(Guid POINT_UID)
			{
				return (OptimizerSolutionDataSet.SolutionEfficientFrontierRow)base.Rows.Find(new object[]
				{
					POINT_UID
				});
			}

			// Token: 0x06002D96 RID: 11670 RVA: 0x00092AF6 File Offset: 0x00090CF6
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public virtual IEnumerator GetEnumerator()
			{
				return base.Rows.GetEnumerator();
			}

			// Token: 0x06002D97 RID: 11671 RVA: 0x00092B04 File Offset: 0x00090D04
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			public override DataTable Clone()
			{
				OptimizerSolutionDataSet.SolutionEfficientFrontierDataTable solutionEfficientFrontierDataTable = (OptimizerSolutionDataSet.SolutionEfficientFrontierDataTable)base.Clone();
				solutionEfficientFrontierDataTable.InitVars();
				return solutionEfficientFrontierDataTable;
			}

			// Token: 0x06002D98 RID: 11672 RVA: 0x00092B24 File Offset: 0x00090D24
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			protected override DataTable CreateInstance()
			{
				return new OptimizerSolutionDataSet.SolutionEfficientFrontierDataTable();
			}

			// Token: 0x06002D99 RID: 11673 RVA: 0x00092B2C File Offset: 0x00090D2C
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

			// Token: 0x06002D9A RID: 11674 RVA: 0x00092BA8 File Offset: 0x00090DA8
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

			// Token: 0x06002D9B RID: 11675 RVA: 0x00092D41 File Offset: 0x00090F41
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public OptimizerSolutionDataSet.SolutionEfficientFrontierRow NewSolutionEfficientFrontierRow()
			{
				return (OptimizerSolutionDataSet.SolutionEfficientFrontierRow)base.NewRow();
			}

			// Token: 0x06002D9C RID: 11676 RVA: 0x00092D4E File Offset: 0x00090F4E
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			protected override DataRow NewRowFromBuilder(DataRowBuilder builder)
			{
				return new OptimizerSolutionDataSet.SolutionEfficientFrontierRow(builder);
			}

			// Token: 0x06002D9D RID: 11677 RVA: 0x00092D56 File Offset: 0x00090F56
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			protected override Type GetRowType()
			{
				return typeof(OptimizerSolutionDataSet.SolutionEfficientFrontierRow);
			}

			// Token: 0x06002D9E RID: 11678 RVA: 0x00092D62 File Offset: 0x00090F62
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			protected override void OnRowChanged(DataRowChangeEventArgs e)
			{
				base.OnRowChanged(e);
				if (this.SolutionEfficientFrontierRowChanged != null)
				{
					this.SolutionEfficientFrontierRowChanged(this, new OptimizerSolutionDataSet.SolutionEfficientFrontierRowChangeEvent((OptimizerSolutionDataSet.SolutionEfficientFrontierRow)e.Row, e.Action));
				}
			}

			// Token: 0x06002D9F RID: 11679 RVA: 0x00092D95 File Offset: 0x00090F95
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			protected override void OnRowChanging(DataRowChangeEventArgs e)
			{
				base.OnRowChanging(e);
				if (this.SolutionEfficientFrontierRowChanging != null)
				{
					this.SolutionEfficientFrontierRowChanging(this, new OptimizerSolutionDataSet.SolutionEfficientFrontierRowChangeEvent((OptimizerSolutionDataSet.SolutionEfficientFrontierRow)e.Row, e.Action));
				}
			}

			// Token: 0x06002DA0 RID: 11680 RVA: 0x00092DC8 File Offset: 0x00090FC8
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			protected override void OnRowDeleted(DataRowChangeEventArgs e)
			{
				base.OnRowDeleted(e);
				if (this.SolutionEfficientFrontierRowDeleted != null)
				{
					this.SolutionEfficientFrontierRowDeleted(this, new OptimizerSolutionDataSet.SolutionEfficientFrontierRowChangeEvent((OptimizerSolutionDataSet.SolutionEfficientFrontierRow)e.Row, e.Action));
				}
			}

			// Token: 0x06002DA1 RID: 11681 RVA: 0x00092DFB File Offset: 0x00090FFB
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			protected override void OnRowDeleting(DataRowChangeEventArgs e)
			{
				base.OnRowDeleting(e);
				if (this.SolutionEfficientFrontierRowDeleting != null)
				{
					this.SolutionEfficientFrontierRowDeleting(this, new OptimizerSolutionDataSet.SolutionEfficientFrontierRowChangeEvent((OptimizerSolutionDataSet.SolutionEfficientFrontierRow)e.Row, e.Action));
				}
			}

			// Token: 0x06002DA2 RID: 11682 RVA: 0x00092E2E File Offset: 0x0009102E
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			public void RemoveSolutionEfficientFrontierRow(OptimizerSolutionDataSet.SolutionEfficientFrontierRow row)
			{
				base.Rows.Remove(row);
			}

			// Token: 0x06002DA3 RID: 11683 RVA: 0x00092E3C File Offset: 0x0009103C
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			public static XmlSchemaComplexType GetTypedTableSchema(XmlSchemaSet xs)
			{
				XmlSchemaComplexType xmlSchemaComplexType = new XmlSchemaComplexType();
				XmlSchemaSequence xmlSchemaSequence = new XmlSchemaSequence();
				OptimizerSolutionDataSet optimizerSolutionDataSet = new OptimizerSolutionDataSet();
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
				xmlSchemaAttribute.FixedValue = optimizerSolutionDataSet.Namespace;
				xmlSchemaComplexType.Attributes.Add(xmlSchemaAttribute);
				XmlSchemaAttribute xmlSchemaAttribute2 = new XmlSchemaAttribute();
				xmlSchemaAttribute2.Name = "tableTypeName";
				xmlSchemaAttribute2.FixedValue = "SolutionEfficientFrontierDataTable";
				xmlSchemaComplexType.Attributes.Add(xmlSchemaAttribute2);
				xmlSchemaComplexType.Particle = xmlSchemaSequence;
				XmlSchema schemaSerializable = optimizerSolutionDataSet.GetSchemaSerializable();
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

			// Token: 0x040009A6 RID: 2470
			private DataColumn columnFRONTIER_UID;

			// Token: 0x040009A7 RID: 2471
			private DataColumn columnANALYSIS_UID;

			// Token: 0x040009A8 RID: 2472
			private DataColumn columnPOINT_UID;

			// Token: 0x040009A9 RID: 2473
			private DataColumn columnX_VALUE;

			// Token: 0x040009AA RID: 2474
			private DataColumn columnY_VALUE;
		}

		// Token: 0x02000233 RID: 563
		public class SolutionRow : DataRow
		{
			// Token: 0x06002DA4 RID: 11684 RVA: 0x00093034 File Offset: 0x00091234
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			internal SolutionRow(DataRowBuilder rb) : base(rb)
			{
				this.tableSolution = (OptimizerSolutionDataSet.SolutionDataTable)base.Table;
			}

			// Token: 0x17000D5A RID: 3418
			// (get) Token: 0x06002DA5 RID: 11685 RVA: 0x0009304E File Offset: 0x0009124E
			// (set) Token: 0x06002DA6 RID: 11686 RVA: 0x00093066 File Offset: 0x00091266
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public Guid ANALYSIS_UID
			{
				get
				{
					return (Guid)base[this.tableSolution.ANALYSIS_UIDColumn];
				}
				set
				{
					base[this.tableSolution.ANALYSIS_UIDColumn] = value;
				}
			}

			// Token: 0x17000D5B RID: 3419
			// (get) Token: 0x06002DA7 RID: 11687 RVA: 0x0009307F File Offset: 0x0009127F
			// (set) Token: 0x06002DA8 RID: 11688 RVA: 0x00093097 File Offset: 0x00091297
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			public Guid SOLUTION_UID
			{
				get
				{
					return (Guid)base[this.tableSolution.SOLUTION_UIDColumn];
				}
				set
				{
					base[this.tableSolution.SOLUTION_UIDColumn] = value;
				}
			}

			// Token: 0x17000D5C RID: 3420
			// (get) Token: 0x06002DA9 RID: 11689 RVA: 0x000930B0 File Offset: 0x000912B0
			// (set) Token: 0x06002DAA RID: 11690 RVA: 0x000930C8 File Offset: 0x000912C8
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			public string SOLUTION_NAME
			{
				get
				{
					return (string)base[this.tableSolution.SOLUTION_NAMEColumn];
				}
				set
				{
					base[this.tableSolution.SOLUTION_NAMEColumn] = value;
				}
			}

			// Token: 0x17000D5D RID: 3421
			// (get) Token: 0x06002DAB RID: 11691 RVA: 0x000930DC File Offset: 0x000912DC
			// (set) Token: 0x06002DAC RID: 11692 RVA: 0x00093120 File Offset: 0x00091320
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			public string SOLUTION_DESCRIPTION
			{
				get
				{
					string result;
					try
					{
						result = (string)base[this.tableSolution.SOLUTION_DESCRIPTIONColumn];
					}
					catch (InvalidCastException innerException)
					{
						throw new StrongTypingException("The value for column 'SOLUTION_DESCRIPTION' in table 'Solution' is DBNull.", innerException);
					}
					return result;
				}
				set
				{
					base[this.tableSolution.SOLUTION_DESCRIPTIONColumn] = value;
				}
			}

			// Token: 0x17000D5E RID: 3422
			// (get) Token: 0x06002DAD RID: 11693 RVA: 0x00093134 File Offset: 0x00091334
			// (set) Token: 0x06002DAE RID: 11694 RVA: 0x00093178 File Offset: 0x00091378
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			public Guid HARD_CONSTRAINT_CF_UID
			{
				get
				{
					Guid result;
					try
					{
						result = (Guid)base[this.tableSolution.HARD_CONSTRAINT_CF_UIDColumn];
					}
					catch (InvalidCastException innerException)
					{
						throw new StrongTypingException("The value for column 'HARD_CONSTRAINT_CF_UID' in table 'Solution' is DBNull.", innerException);
					}
					return result;
				}
				set
				{
					base[this.tableSolution.HARD_CONSTRAINT_CF_UIDColumn] = value;
				}
			}

			// Token: 0x17000D5F RID: 3423
			// (get) Token: 0x06002DAF RID: 11695 RVA: 0x00093194 File Offset: 0x00091394
			// (set) Token: 0x06002DB0 RID: 11696 RVA: 0x000931D8 File Offset: 0x000913D8
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			public string HARD_CONSTRAINT_CF_NAME
			{
				get
				{
					string result;
					try
					{
						result = (string)base[this.tableSolution.HARD_CONSTRAINT_CF_NAMEColumn];
					}
					catch (InvalidCastException innerException)
					{
						throw new StrongTypingException("The value for column 'HARD_CONSTRAINT_CF_NAME' in table 'Solution' is DBNull.", innerException);
					}
					return result;
				}
				set
				{
					base[this.tableSolution.HARD_CONSTRAINT_CF_NAMEColumn] = value;
				}
			}

			// Token: 0x17000D60 RID: 3424
			// (get) Token: 0x06002DB1 RID: 11697 RVA: 0x000931EC File Offset: 0x000913EC
			// (set) Token: 0x06002DB2 RID: 11698 RVA: 0x00093230 File Offset: 0x00091430
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public Guid FRONTIER_UID
			{
				get
				{
					Guid result;
					try
					{
						result = (Guid)base[this.tableSolution.FRONTIER_UIDColumn];
					}
					catch (InvalidCastException innerException)
					{
						throw new StrongTypingException("The value for column 'FRONTIER_UID' in table 'Solution' is DBNull.", innerException);
					}
					return result;
				}
				set
				{
					base[this.tableSolution.FRONTIER_UIDColumn] = value;
				}
			}

			// Token: 0x17000D61 RID: 3425
			// (get) Token: 0x06002DB3 RID: 11699 RVA: 0x00093249 File Offset: 0x00091449
			// (set) Token: 0x06002DB4 RID: 11700 RVA: 0x00093261 File Offset: 0x00091461
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public bool OPT_USE_DEPENDENCIES
			{
				get
				{
					return (bool)base[this.tableSolution.OPT_USE_DEPENDENCIESColumn];
				}
				set
				{
					base[this.tableSolution.OPT_USE_DEPENDENCIESColumn] = value;
				}
			}

			// Token: 0x17000D62 RID: 3426
			// (get) Token: 0x06002DB5 RID: 11701 RVA: 0x0009327C File Offset: 0x0009147C
			// (set) Token: 0x06002DB6 RID: 11702 RVA: 0x000932C0 File Offset: 0x000914C0
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public DateTime CREATED_DATE
			{
				get
				{
					DateTime result;
					try
					{
						result = (DateTime)base[this.tableSolution.CREATED_DATEColumn];
					}
					catch (InvalidCastException innerException)
					{
						throw new StrongTypingException("The value for column 'CREATED_DATE' in table 'Solution' is DBNull.", innerException);
					}
					return result;
				}
				set
				{
					base[this.tableSolution.CREATED_DATEColumn] = value;
				}
			}

			// Token: 0x17000D63 RID: 3427
			// (get) Token: 0x06002DB7 RID: 11703 RVA: 0x000932DC File Offset: 0x000914DC
			// (set) Token: 0x06002DB8 RID: 11704 RVA: 0x00093320 File Offset: 0x00091520
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public DateTime MOD_DATE
			{
				get
				{
					DateTime result;
					try
					{
						result = (DateTime)base[this.tableSolution.MOD_DATEColumn];
					}
					catch (InvalidCastException innerException)
					{
						throw new StrongTypingException("The value for column 'MOD_DATE' in table 'Solution' is DBNull.", innerException);
					}
					return result;
				}
				set
				{
					base[this.tableSolution.MOD_DATEColumn] = value;
				}
			}

			// Token: 0x17000D64 RID: 3428
			// (get) Token: 0x06002DB9 RID: 11705 RVA: 0x0009333C File Offset: 0x0009153C
			// (set) Token: 0x06002DBA RID: 11706 RVA: 0x00093380 File Offset: 0x00091580
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			public Guid LAST_UPDATED_BY_RES_UID
			{
				get
				{
					Guid result;
					try
					{
						result = (Guid)base[this.tableSolution.LAST_UPDATED_BY_RES_UIDColumn];
					}
					catch (InvalidCastException innerException)
					{
						throw new StrongTypingException("The value for column 'LAST_UPDATED_BY_RES_UID' in table 'Solution' is DBNull.", innerException);
					}
					return result;
				}
				set
				{
					base[this.tableSolution.LAST_UPDATED_BY_RES_UIDColumn] = value;
				}
			}

			// Token: 0x17000D65 RID: 3429
			// (get) Token: 0x06002DBB RID: 11707 RVA: 0x0009339C File Offset: 0x0009159C
			// (set) Token: 0x06002DBC RID: 11708 RVA: 0x000933E0 File Offset: 0x000915E0
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public string LAST_UPDATED_BY_RES_NAME
			{
				get
				{
					string result;
					try
					{
						result = (string)base[this.tableSolution.LAST_UPDATED_BY_RES_NAMEColumn];
					}
					catch (InvalidCastException innerException)
					{
						throw new StrongTypingException("The value for column 'LAST_UPDATED_BY_RES_NAME' in table 'Solution' is DBNull.", innerException);
					}
					return result;
				}
				set
				{
					base[this.tableSolution.LAST_UPDATED_BY_RES_NAMEColumn] = value;
				}
			}

			// Token: 0x17000D66 RID: 3430
			// (get) Token: 0x06002DBD RID: 11709 RVA: 0x000933F4 File Offset: 0x000915F4
			// (set) Token: 0x06002DBE RID: 11710 RVA: 0x00093438 File Offset: 0x00091638
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public Guid CREATED_BY_RES_UID
			{
				get
				{
					Guid result;
					try
					{
						result = (Guid)base[this.tableSolution.CREATED_BY_RES_UIDColumn];
					}
					catch (InvalidCastException innerException)
					{
						throw new StrongTypingException("The value for column 'CREATED_BY_RES_UID' in table 'Solution' is DBNull.", innerException);
					}
					return result;
				}
				set
				{
					base[this.tableSolution.CREATED_BY_RES_UIDColumn] = value;
				}
			}

			// Token: 0x17000D67 RID: 3431
			// (get) Token: 0x06002DBF RID: 11711 RVA: 0x00093454 File Offset: 0x00091654
			// (set) Token: 0x06002DC0 RID: 11712 RVA: 0x00093498 File Offset: 0x00091698
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			public string CREATED_BY_RES_NAME
			{
				get
				{
					string result;
					try
					{
						result = (string)base[this.tableSolution.CREATED_BY_RES_NAMEColumn];
					}
					catch (InvalidCastException innerException)
					{
						throw new StrongTypingException("The value for column 'CREATED_BY_RES_NAME' in table 'Solution' is DBNull.", innerException);
					}
					return result;
				}
				set
				{
					base[this.tableSolution.CREATED_BY_RES_NAMEColumn] = value;
				}
			}

			// Token: 0x17000D68 RID: 3432
			// (get) Token: 0x06002DC1 RID: 11713 RVA: 0x000934AC File Offset: 0x000916AC
			// (set) Token: 0x06002DC2 RID: 11714 RVA: 0x000934F0 File Offset: 0x000916F0
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			public string ANALYSIS_NAME
			{
				get
				{
					string result;
					try
					{
						result = (string)base[this.tableSolution.ANALYSIS_NAMEColumn];
					}
					catch (InvalidCastException innerException)
					{
						throw new StrongTypingException("The value for column 'ANALYSIS_NAME' in table 'Solution' is DBNull.", innerException);
					}
					return result;
				}
				set
				{
					base[this.tableSolution.ANALYSIS_NAMEColumn] = value;
				}
			}

			// Token: 0x17000D69 RID: 3433
			// (get) Token: 0x06002DC3 RID: 11715 RVA: 0x00093504 File Offset: 0x00091704
			// (set) Token: 0x06002DC4 RID: 11716 RVA: 0x00093548 File Offset: 0x00091748
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public decimal TOTAL_HARD_CONSTRAINT_VALUE
			{
				get
				{
					decimal result;
					try
					{
						result = (decimal)base[this.tableSolution.TOTAL_HARD_CONSTRAINT_VALUEColumn];
					}
					catch (InvalidCastException innerException)
					{
						throw new StrongTypingException("The value for column 'TOTAL_HARD_CONSTRAINT_VALUE' in table 'Solution' is DBNull.", innerException);
					}
					return result;
				}
				set
				{
					base[this.tableSolution.TOTAL_HARD_CONSTRAINT_VALUEColumn] = value;
				}
			}

			// Token: 0x17000D6A RID: 3434
			// (get) Token: 0x06002DC5 RID: 11717 RVA: 0x00093564 File Offset: 0x00091764
			// (set) Token: 0x06002DC6 RID: 11718 RVA: 0x000935A8 File Offset: 0x000917A8
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public decimal TOTAL_PRIORITY_VALUE
			{
				get
				{
					decimal result;
					try
					{
						result = (decimal)base[this.tableSolution.TOTAL_PRIORITY_VALUEColumn];
					}
					catch (InvalidCastException innerException)
					{
						throw new StrongTypingException("The value for column 'TOTAL_PRIORITY_VALUE' in table 'Solution' is DBNull.", innerException);
					}
					return result;
				}
				set
				{
					base[this.tableSolution.TOTAL_PRIORITY_VALUEColumn] = value;
				}
			}

			// Token: 0x06002DC7 RID: 11719 RVA: 0x000935C1 File Offset: 0x000917C1
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			public bool IsSOLUTION_DESCRIPTIONNull()
			{
				return base.IsNull(this.tableSolution.SOLUTION_DESCRIPTIONColumn);
			}

			// Token: 0x06002DC8 RID: 11720 RVA: 0x000935D4 File Offset: 0x000917D4
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			public void SetSOLUTION_DESCRIPTIONNull()
			{
				base[this.tableSolution.SOLUTION_DESCRIPTIONColumn] = Convert.DBNull;
			}

			// Token: 0x06002DC9 RID: 11721 RVA: 0x000935EC File Offset: 0x000917EC
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			public bool IsHARD_CONSTRAINT_CF_UIDNull()
			{
				return base.IsNull(this.tableSolution.HARD_CONSTRAINT_CF_UIDColumn);
			}

			// Token: 0x06002DCA RID: 11722 RVA: 0x000935FF File Offset: 0x000917FF
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			public void SetHARD_CONSTRAINT_CF_UIDNull()
			{
				base[this.tableSolution.HARD_CONSTRAINT_CF_UIDColumn] = Convert.DBNull;
			}

			// Token: 0x06002DCB RID: 11723 RVA: 0x00093617 File Offset: 0x00091817
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			public bool IsHARD_CONSTRAINT_CF_NAMENull()
			{
				return base.IsNull(this.tableSolution.HARD_CONSTRAINT_CF_NAMEColumn);
			}

			// Token: 0x06002DCC RID: 11724 RVA: 0x0009362A File Offset: 0x0009182A
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			public void SetHARD_CONSTRAINT_CF_NAMENull()
			{
				base[this.tableSolution.HARD_CONSTRAINT_CF_NAMEColumn] = Convert.DBNull;
			}

			// Token: 0x06002DCD RID: 11725 RVA: 0x00093642 File Offset: 0x00091842
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			public bool IsFRONTIER_UIDNull()
			{
				return base.IsNull(this.tableSolution.FRONTIER_UIDColumn);
			}

			// Token: 0x06002DCE RID: 11726 RVA: 0x00093655 File Offset: 0x00091855
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			public void SetFRONTIER_UIDNull()
			{
				base[this.tableSolution.FRONTIER_UIDColumn] = Convert.DBNull;
			}

			// Token: 0x06002DCF RID: 11727 RVA: 0x0009366D File Offset: 0x0009186D
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			public bool IsCREATED_DATENull()
			{
				return base.IsNull(this.tableSolution.CREATED_DATEColumn);
			}

			// Token: 0x06002DD0 RID: 11728 RVA: 0x00093680 File Offset: 0x00091880
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			public void SetCREATED_DATENull()
			{
				base[this.tableSolution.CREATED_DATEColumn] = Convert.DBNull;
			}

			// Token: 0x06002DD1 RID: 11729 RVA: 0x00093698 File Offset: 0x00091898
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			public bool IsMOD_DATENull()
			{
				return base.IsNull(this.tableSolution.MOD_DATEColumn);
			}

			// Token: 0x06002DD2 RID: 11730 RVA: 0x000936AB File Offset: 0x000918AB
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			public void SetMOD_DATENull()
			{
				base[this.tableSolution.MOD_DATEColumn] = Convert.DBNull;
			}

			// Token: 0x06002DD3 RID: 11731 RVA: 0x000936C3 File Offset: 0x000918C3
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			public bool IsLAST_UPDATED_BY_RES_UIDNull()
			{
				return base.IsNull(this.tableSolution.LAST_UPDATED_BY_RES_UIDColumn);
			}

			// Token: 0x06002DD4 RID: 11732 RVA: 0x000936D6 File Offset: 0x000918D6
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			public void SetLAST_UPDATED_BY_RES_UIDNull()
			{
				base[this.tableSolution.LAST_UPDATED_BY_RES_UIDColumn] = Convert.DBNull;
			}

			// Token: 0x06002DD5 RID: 11733 RVA: 0x000936EE File Offset: 0x000918EE
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			public bool IsLAST_UPDATED_BY_RES_NAMENull()
			{
				return base.IsNull(this.tableSolution.LAST_UPDATED_BY_RES_NAMEColumn);
			}

			// Token: 0x06002DD6 RID: 11734 RVA: 0x00093701 File Offset: 0x00091901
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			public void SetLAST_UPDATED_BY_RES_NAMENull()
			{
				base[this.tableSolution.LAST_UPDATED_BY_RES_NAMEColumn] = Convert.DBNull;
			}

			// Token: 0x06002DD7 RID: 11735 RVA: 0x00093719 File Offset: 0x00091919
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			public bool IsCREATED_BY_RES_UIDNull()
			{
				return base.IsNull(this.tableSolution.CREATED_BY_RES_UIDColumn);
			}

			// Token: 0x06002DD8 RID: 11736 RVA: 0x0009372C File Offset: 0x0009192C
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public void SetCREATED_BY_RES_UIDNull()
			{
				base[this.tableSolution.CREATED_BY_RES_UIDColumn] = Convert.DBNull;
			}

			// Token: 0x06002DD9 RID: 11737 RVA: 0x00093744 File Offset: 0x00091944
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			public bool IsCREATED_BY_RES_NAMENull()
			{
				return base.IsNull(this.tableSolution.CREATED_BY_RES_NAMEColumn);
			}

			// Token: 0x06002DDA RID: 11738 RVA: 0x00093757 File Offset: 0x00091957
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			public void SetCREATED_BY_RES_NAMENull()
			{
				base[this.tableSolution.CREATED_BY_RES_NAMEColumn] = Convert.DBNull;
			}

			// Token: 0x06002DDB RID: 11739 RVA: 0x0009376F File Offset: 0x0009196F
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			public bool IsANALYSIS_NAMENull()
			{
				return base.IsNull(this.tableSolution.ANALYSIS_NAMEColumn);
			}

			// Token: 0x06002DDC RID: 11740 RVA: 0x00093782 File Offset: 0x00091982
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			public void SetANALYSIS_NAMENull()
			{
				base[this.tableSolution.ANALYSIS_NAMEColumn] = Convert.DBNull;
			}

			// Token: 0x06002DDD RID: 11741 RVA: 0x0009379A File Offset: 0x0009199A
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			public bool IsTOTAL_HARD_CONSTRAINT_VALUENull()
			{
				return base.IsNull(this.tableSolution.TOTAL_HARD_CONSTRAINT_VALUEColumn);
			}

			// Token: 0x06002DDE RID: 11742 RVA: 0x000937AD File Offset: 0x000919AD
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			public void SetTOTAL_HARD_CONSTRAINT_VALUENull()
			{
				base[this.tableSolution.TOTAL_HARD_CONSTRAINT_VALUEColumn] = Convert.DBNull;
			}

			// Token: 0x06002DDF RID: 11743 RVA: 0x000937C5 File Offset: 0x000919C5
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			public bool IsTOTAL_PRIORITY_VALUENull()
			{
				return base.IsNull(this.tableSolution.TOTAL_PRIORITY_VALUEColumn);
			}

			// Token: 0x06002DE0 RID: 11744 RVA: 0x000937D8 File Offset: 0x000919D8
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			public void SetTOTAL_PRIORITY_VALUENull()
			{
				base[this.tableSolution.TOTAL_PRIORITY_VALUEColumn] = Convert.DBNull;
			}

			// Token: 0x06002DE1 RID: 11745 RVA: 0x000937F0 File Offset: 0x000919F0
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			public OptimizerSolutionDataSet.SolutionConstraintsRow[] GetSolutionConstraintsRows()
			{
				if (base.Table.ChildRelations["FK_Solution_SolutionConstraints"] == null)
				{
					return new OptimizerSolutionDataSet.SolutionConstraintsRow[0];
				}
				return (OptimizerSolutionDataSet.SolutionConstraintsRow[])base.GetChildRows(base.Table.ChildRelations["FK_Solution_SolutionConstraints"]);
			}

			// Token: 0x06002DE2 RID: 11746 RVA: 0x00093830 File Offset: 0x00091A30
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			public OptimizerSolutionDataSet.SolutionProjectsRow[] GetSolutionProjectsRows()
			{
				if (base.Table.ChildRelations["FK_Solution_SolutionProjects"] == null)
				{
					return new OptimizerSolutionDataSet.SolutionProjectsRow[0];
				}
				return (OptimizerSolutionDataSet.SolutionProjectsRow[])base.GetChildRows(base.Table.ChildRelations["FK_Solution_SolutionProjects"]);
			}

			// Token: 0x06002DE3 RID: 11747 RVA: 0x00093870 File Offset: 0x00091A70
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			public OptimizerSolutionDataSet.SolutionStrategicAlignmentRow[] GetSolutionStrategicAlignmentRows()
			{
				if (base.Table.ChildRelations["FK_Solution_SolutionStrategicAlignment"] == null)
				{
					return new OptimizerSolutionDataSet.SolutionStrategicAlignmentRow[0];
				}
				return (OptimizerSolutionDataSet.SolutionStrategicAlignmentRow[])base.GetChildRows(base.Table.ChildRelations["FK_Solution_SolutionStrategicAlignment"]);
			}

			// Token: 0x040009AF RID: 2479
			private OptimizerSolutionDataSet.SolutionDataTable tableSolution;
		}

		// Token: 0x02000234 RID: 564
		public class SolutionConstraintsRow : DataRow
		{
			// Token: 0x06002DE4 RID: 11748 RVA: 0x000938B0 File Offset: 0x00091AB0
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			internal SolutionConstraintsRow(DataRowBuilder rb) : base(rb)
			{
				this.tableSolutionConstraints = (OptimizerSolutionDataSet.SolutionConstraintsDataTable)base.Table;
			}

			// Token: 0x17000D6B RID: 3435
			// (get) Token: 0x06002DE5 RID: 11749 RVA: 0x000938CA File Offset: 0x00091ACA
			// (set) Token: 0x06002DE6 RID: 11750 RVA: 0x000938E2 File Offset: 0x00091AE2
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public Guid SOLUTION_UID
			{
				get
				{
					return (Guid)base[this.tableSolutionConstraints.SOLUTION_UIDColumn];
				}
				set
				{
					base[this.tableSolutionConstraints.SOLUTION_UIDColumn] = value;
				}
			}

			// Token: 0x17000D6C RID: 3436
			// (get) Token: 0x06002DE7 RID: 11751 RVA: 0x000938FB File Offset: 0x00091AFB
			// (set) Token: 0x06002DE8 RID: 11752 RVA: 0x00093913 File Offset: 0x00091B13
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public Guid MD_PROP_UID
			{
				get
				{
					return (Guid)base[this.tableSolutionConstraints.MD_PROP_UIDColumn];
				}
				set
				{
					base[this.tableSolutionConstraints.MD_PROP_UIDColumn] = value;
				}
			}

			// Token: 0x17000D6D RID: 3437
			// (get) Token: 0x06002DE9 RID: 11753 RVA: 0x0009392C File Offset: 0x00091B2C
			// (set) Token: 0x06002DEA RID: 11754 RVA: 0x00093970 File Offset: 0x00091B70
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public string MD_PROP_NAME
			{
				get
				{
					string result;
					try
					{
						result = (string)base[this.tableSolutionConstraints.MD_PROP_NAMEColumn];
					}
					catch (InvalidCastException innerException)
					{
						throw new StrongTypingException("The value for column 'MD_PROP_NAME' in table 'SolutionConstraints' is DBNull.", innerException);
					}
					return result;
				}
				set
				{
					base[this.tableSolutionConstraints.MD_PROP_NAMEColumn] = value;
				}
			}

			// Token: 0x17000D6E RID: 3438
			// (get) Token: 0x06002DEB RID: 11755 RVA: 0x00093984 File Offset: 0x00091B84
			// (set) Token: 0x06002DEC RID: 11756 RVA: 0x0009399C File Offset: 0x00091B9C
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			public int MD_PROP_POS
			{
				get
				{
					return (int)base[this.tableSolutionConstraints.MD_PROP_POSColumn];
				}
				set
				{
					base[this.tableSolutionConstraints.MD_PROP_POSColumn] = value;
				}
			}

			// Token: 0x17000D6F RID: 3439
			// (get) Token: 0x06002DED RID: 11757 RVA: 0x000939B5 File Offset: 0x00091BB5
			// (set) Token: 0x06002DEE RID: 11758 RVA: 0x000939CD File Offset: 0x00091BCD
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public decimal MAX_VALUE
			{
				get
				{
					return (decimal)base[this.tableSolutionConstraints.MAX_VALUEColumn];
				}
				set
				{
					base[this.tableSolutionConstraints.MAX_VALUEColumn] = value;
				}
			}

			// Token: 0x17000D70 RID: 3440
			// (get) Token: 0x06002DEF RID: 11759 RVA: 0x000939E6 File Offset: 0x00091BE6
			// (set) Token: 0x06002DF0 RID: 11760 RVA: 0x00093A08 File Offset: 0x00091C08
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			public OptimizerSolutionDataSet.SolutionRow SolutionRow
			{
				get
				{
					return (OptimizerSolutionDataSet.SolutionRow)base.GetParentRow(base.Table.ParentRelations["FK_Solution_SolutionConstraints"]);
				}
				set
				{
					base.SetParentRow(value, base.Table.ParentRelations["FK_Solution_SolutionConstraints"]);
				}
			}

			// Token: 0x06002DF1 RID: 11761 RVA: 0x00093A26 File Offset: 0x00091C26
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			public bool IsMD_PROP_NAMENull()
			{
				return base.IsNull(this.tableSolutionConstraints.MD_PROP_NAMEColumn);
			}

			// Token: 0x06002DF2 RID: 11762 RVA: 0x00093A39 File Offset: 0x00091C39
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			public void SetMD_PROP_NAMENull()
			{
				base[this.tableSolutionConstraints.MD_PROP_NAMEColumn] = Convert.DBNull;
			}

			// Token: 0x06002DF3 RID: 11763 RVA: 0x00093A51 File Offset: 0x00091C51
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			public OptimizerSolutionDataSet.SolutionConstraintValuesRow[] GetSolutionConstraintValuesRows()
			{
				if (base.Table.ChildRelations["FK_SolutionConstraints_SolutionConstraintValues"] == null)
				{
					return new OptimizerSolutionDataSet.SolutionConstraintValuesRow[0];
				}
				return (OptimizerSolutionDataSet.SolutionConstraintValuesRow[])base.GetChildRows(base.Table.ChildRelations["FK_SolutionConstraints_SolutionConstraintValues"]);
			}

			// Token: 0x040009B0 RID: 2480
			private OptimizerSolutionDataSet.SolutionConstraintsDataTable tableSolutionConstraints;
		}

		// Token: 0x02000235 RID: 565
		public class SolutionConstraintValuesRow : DataRow
		{
			// Token: 0x06002DF4 RID: 11764 RVA: 0x00093A91 File Offset: 0x00091C91
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			internal SolutionConstraintValuesRow(DataRowBuilder rb) : base(rb)
			{
				this.tableSolutionConstraintValues = (OptimizerSolutionDataSet.SolutionConstraintValuesDataTable)base.Table;
			}

			// Token: 0x17000D71 RID: 3441
			// (get) Token: 0x06002DF5 RID: 11765 RVA: 0x00093AAB File Offset: 0x00091CAB
			// (set) Token: 0x06002DF6 RID: 11766 RVA: 0x00093AC3 File Offset: 0x00091CC3
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			public Guid SOLUTION_UID
			{
				get
				{
					return (Guid)base[this.tableSolutionConstraintValues.SOLUTION_UIDColumn];
				}
				set
				{
					base[this.tableSolutionConstraintValues.SOLUTION_UIDColumn] = value;
				}
			}

			// Token: 0x17000D72 RID: 3442
			// (get) Token: 0x06002DF7 RID: 11767 RVA: 0x00093ADC File Offset: 0x00091CDC
			// (set) Token: 0x06002DF8 RID: 11768 RVA: 0x00093AF4 File Offset: 0x00091CF4
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			public Guid MD_PROP_UID
			{
				get
				{
					return (Guid)base[this.tableSolutionConstraintValues.MD_PROP_UIDColumn];
				}
				set
				{
					base[this.tableSolutionConstraintValues.MD_PROP_UIDColumn] = value;
				}
			}

			// Token: 0x17000D73 RID: 3443
			// (get) Token: 0x06002DF9 RID: 11769 RVA: 0x00093B0D File Offset: 0x00091D0D
			// (set) Token: 0x06002DFA RID: 11770 RVA: 0x00093B25 File Offset: 0x00091D25
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public Guid PROJ_UID
			{
				get
				{
					return (Guid)base[this.tableSolutionConstraintValues.PROJ_UIDColumn];
				}
				set
				{
					base[this.tableSolutionConstraintValues.PROJ_UIDColumn] = value;
				}
			}

			// Token: 0x17000D74 RID: 3444
			// (get) Token: 0x06002DFB RID: 11771 RVA: 0x00093B3E File Offset: 0x00091D3E
			// (set) Token: 0x06002DFC RID: 11772 RVA: 0x00093B56 File Offset: 0x00091D56
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public decimal NUM_VALUE
			{
				get
				{
					return (decimal)base[this.tableSolutionConstraintValues.NUM_VALUEColumn];
				}
				set
				{
					base[this.tableSolutionConstraintValues.NUM_VALUEColumn] = value;
				}
			}

			// Token: 0x17000D75 RID: 3445
			// (get) Token: 0x06002DFD RID: 11773 RVA: 0x00093B6F File Offset: 0x00091D6F
			// (set) Token: 0x06002DFE RID: 11774 RVA: 0x00093B91 File Offset: 0x00091D91
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public OptimizerSolutionDataSet.SolutionConstraintsRow SolutionConstraintsRowParent
			{
				get
				{
					return (OptimizerSolutionDataSet.SolutionConstraintsRow)base.GetParentRow(base.Table.ParentRelations["FK_SolutionConstraints_SolutionConstraintValues"]);
				}
				set
				{
					base.SetParentRow(value, base.Table.ParentRelations["FK_SolutionConstraints_SolutionConstraintValues"]);
				}
			}

			// Token: 0x17000D76 RID: 3446
			// (get) Token: 0x06002DFF RID: 11775 RVA: 0x00093BAF File Offset: 0x00091DAF
			// (set) Token: 0x06002E00 RID: 11776 RVA: 0x00093BD1 File Offset: 0x00091DD1
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			public OptimizerSolutionDataSet.SolutionProjectsRow SolutionProjectsRowParent
			{
				get
				{
					return (OptimizerSolutionDataSet.SolutionProjectsRow)base.GetParentRow(base.Table.ParentRelations["FK_SolutionProjects_SolutionConstraintValues"]);
				}
				set
				{
					base.SetParentRow(value, base.Table.ParentRelations["FK_SolutionProjects_SolutionConstraintValues"]);
				}
			}

			// Token: 0x040009B1 RID: 2481
			private OptimizerSolutionDataSet.SolutionConstraintValuesDataTable tableSolutionConstraintValues;
		}

		// Token: 0x02000236 RID: 566
		public class SolutionProjectsRow : DataRow
		{
			// Token: 0x06002E01 RID: 11777 RVA: 0x00093BEF File Offset: 0x00091DEF
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			internal SolutionProjectsRow(DataRowBuilder rb) : base(rb)
			{
				this.tableSolutionProjects = (OptimizerSolutionDataSet.SolutionProjectsDataTable)base.Table;
			}

			// Token: 0x17000D77 RID: 3447
			// (get) Token: 0x06002E02 RID: 11778 RVA: 0x00093C09 File Offset: 0x00091E09
			// (set) Token: 0x06002E03 RID: 11779 RVA: 0x00093C21 File Offset: 0x00091E21
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

			// Token: 0x17000D78 RID: 3448
			// (get) Token: 0x06002E04 RID: 11780 RVA: 0x00093C3A File Offset: 0x00091E3A
			// (set) Token: 0x06002E05 RID: 11781 RVA: 0x00093C52 File Offset: 0x00091E52
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

			// Token: 0x17000D79 RID: 3449
			// (get) Token: 0x06002E06 RID: 11782 RVA: 0x00093C6C File Offset: 0x00091E6C
			// (set) Token: 0x06002E07 RID: 11783 RVA: 0x00093CB0 File Offset: 0x00091EB0
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

			// Token: 0x17000D7A RID: 3450
			// (get) Token: 0x06002E08 RID: 11784 RVA: 0x00093CC4 File Offset: 0x00091EC4
			// (set) Token: 0x06002E09 RID: 11785 RVA: 0x00093D08 File Offset: 0x00091F08
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
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

			// Token: 0x17000D7B RID: 3451
			// (get) Token: 0x06002E0A RID: 11786 RVA: 0x00093D24 File Offset: 0x00091F24
			// (set) Token: 0x06002E0B RID: 11787 RVA: 0x00093D68 File Offset: 0x00091F68
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
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

			// Token: 0x17000D7C RID: 3452
			// (get) Token: 0x06002E0C RID: 11788 RVA: 0x00093D84 File Offset: 0x00091F84
			// (set) Token: 0x06002E0D RID: 11789 RVA: 0x00093DC8 File Offset: 0x00091FC8
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
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

			// Token: 0x17000D7D RID: 3453
			// (get) Token: 0x06002E0E RID: 11790 RVA: 0x00093DE1 File Offset: 0x00091FE1
			// (set) Token: 0x06002E0F RID: 11791 RVA: 0x00093DF9 File Offset: 0x00091FF9
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
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

			// Token: 0x17000D7E RID: 3454
			// (get) Token: 0x06002E10 RID: 11792 RVA: 0x00093E14 File Offset: 0x00092014
			// (set) Token: 0x06002E11 RID: 11793 RVA: 0x00093E58 File Offset: 0x00092058
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

			// Token: 0x17000D7F RID: 3455
			// (get) Token: 0x06002E12 RID: 11794 RVA: 0x00093E74 File Offset: 0x00092074
			// (set) Token: 0x06002E13 RID: 11795 RVA: 0x00093EB8 File Offset: 0x000920B8
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

			// Token: 0x17000D80 RID: 3456
			// (get) Token: 0x06002E14 RID: 11796 RVA: 0x00093ECC File Offset: 0x000920CC
			// (set) Token: 0x06002E15 RID: 11797 RVA: 0x00093EEE File Offset: 0x000920EE
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			public OptimizerSolutionDataSet.SolutionRow SolutionRow
			{
				get
				{
					return (OptimizerSolutionDataSet.SolutionRow)base.GetParentRow(base.Table.ParentRelations["FK_Solution_SolutionProjects"]);
				}
				set
				{
					base.SetParentRow(value, base.Table.ParentRelations["FK_Solution_SolutionProjects"]);
				}
			}

			// Token: 0x06002E16 RID: 11798 RVA: 0x00093F0C File Offset: 0x0009210C
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public bool IsPROJ_NAMENull()
			{
				return base.IsNull(this.tableSolutionProjects.PROJ_NAMEColumn);
			}

			// Token: 0x06002E17 RID: 11799 RVA: 0x00093F1F File Offset: 0x0009211F
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			public void SetPROJ_NAMENull()
			{
				base[this.tableSolutionProjects.PROJ_NAMEColumn] = Convert.DBNull;
			}

			// Token: 0x06002E18 RID: 11800 RVA: 0x00093F37 File Offset: 0x00092137
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			public bool IsPRIORITYNull()
			{
				return base.IsNull(this.tableSolutionProjects.PRIORITYColumn);
			}

			// Token: 0x06002E19 RID: 11801 RVA: 0x00093F4A File Offset: 0x0009214A
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			public void SetPRIORITYNull()
			{
				base[this.tableSolutionProjects.PRIORITYColumn] = Convert.DBNull;
			}

			// Token: 0x06002E1A RID: 11802 RVA: 0x00093F62 File Offset: 0x00092162
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			public bool IsABSOLUTE_PRIORITYNull()
			{
				return base.IsNull(this.tableSolutionProjects.ABSOLUTE_PRIORITYColumn);
			}

			// Token: 0x06002E1B RID: 11803 RVA: 0x00093F75 File Offset: 0x00092175
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			public void SetABSOLUTE_PRIORITYNull()
			{
				base[this.tableSolutionProjects.ABSOLUTE_PRIORITYColumn] = Convert.DBNull;
			}

			// Token: 0x06002E1C RID: 11804 RVA: 0x00093F8D File Offset: 0x0009218D
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			public bool IsSTATUSNull()
			{
				return base.IsNull(this.tableSolutionProjects.STATUSColumn);
			}

			// Token: 0x06002E1D RID: 11805 RVA: 0x00093FA0 File Offset: 0x000921A0
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public void SetSTATUSNull()
			{
				base[this.tableSolutionProjects.STATUSColumn] = Convert.DBNull;
			}

			// Token: 0x06002E1E RID: 11806 RVA: 0x00093FB8 File Offset: 0x000921B8
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			public bool IsFORCE_ALIAS_LT_STRUCT_UIDNull()
			{
				return base.IsNull(this.tableSolutionProjects.FORCE_ALIAS_LT_STRUCT_UIDColumn);
			}

			// Token: 0x06002E1F RID: 11807 RVA: 0x00093FCB File Offset: 0x000921CB
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			public void SetFORCE_ALIAS_LT_STRUCT_UIDNull()
			{
				base[this.tableSolutionProjects.FORCE_ALIAS_LT_STRUCT_UIDColumn] = Convert.DBNull;
			}

			// Token: 0x06002E20 RID: 11808 RVA: 0x00093FE3 File Offset: 0x000921E3
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public bool IsFORCE_ALIAS_LT_VALUE_FULLNull()
			{
				return base.IsNull(this.tableSolutionProjects.FORCE_ALIAS_LT_VALUE_FULLColumn);
			}

			// Token: 0x06002E21 RID: 11809 RVA: 0x00093FF6 File Offset: 0x000921F6
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public void SetFORCE_ALIAS_LT_VALUE_FULLNull()
			{
				base[this.tableSolutionProjects.FORCE_ALIAS_LT_VALUE_FULLColumn] = Convert.DBNull;
			}

			// Token: 0x06002E22 RID: 11810 RVA: 0x0009400E File Offset: 0x0009220E
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public OptimizerSolutionDataSet.SolutionConstraintValuesRow[] GetSolutionConstraintValuesRows()
			{
				if (base.Table.ChildRelations["FK_SolutionProjects_SolutionConstraintValues"] == null)
				{
					return new OptimizerSolutionDataSet.SolutionConstraintValuesRow[0];
				}
				return (OptimizerSolutionDataSet.SolutionConstraintValuesRow[])base.GetChildRows(base.Table.ChildRelations["FK_SolutionProjects_SolutionConstraintValues"]);
			}

			// Token: 0x040009B2 RID: 2482
			private OptimizerSolutionDataSet.SolutionProjectsDataTable tableSolutionProjects;
		}

		// Token: 0x02000237 RID: 567
		public class SolutionStrategicAlignmentRow : DataRow
		{
			// Token: 0x06002E23 RID: 11811 RVA: 0x0009404E File Offset: 0x0009224E
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			internal SolutionStrategicAlignmentRow(DataRowBuilder rb) : base(rb)
			{
				this.tableSolutionStrategicAlignment = (OptimizerSolutionDataSet.SolutionStrategicAlignmentDataTable)base.Table;
			}

			// Token: 0x17000D81 RID: 3457
			// (get) Token: 0x06002E24 RID: 11812 RVA: 0x00094068 File Offset: 0x00092268
			// (set) Token: 0x06002E25 RID: 11813 RVA: 0x00094080 File Offset: 0x00092280
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public Guid SOLUTION_UID
			{
				get
				{
					return (Guid)base[this.tableSolutionStrategicAlignment.SOLUTION_UIDColumn];
				}
				set
				{
					base[this.tableSolutionStrategicAlignment.SOLUTION_UIDColumn] = value;
				}
			}

			// Token: 0x17000D82 RID: 3458
			// (get) Token: 0x06002E26 RID: 11814 RVA: 0x00094099 File Offset: 0x00092299
			// (set) Token: 0x06002E27 RID: 11815 RVA: 0x000940B1 File Offset: 0x000922B1
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			public Guid DRIVER_UID
			{
				get
				{
					return (Guid)base[this.tableSolutionStrategicAlignment.DRIVER_UIDColumn];
				}
				set
				{
					base[this.tableSolutionStrategicAlignment.DRIVER_UIDColumn] = value;
				}
			}

			// Token: 0x17000D83 RID: 3459
			// (get) Token: 0x06002E28 RID: 11816 RVA: 0x000940CC File Offset: 0x000922CC
			// (set) Token: 0x06002E29 RID: 11817 RVA: 0x00094110 File Offset: 0x00092310
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public string DRIVER_NAME
			{
				get
				{
					string result;
					try
					{
						result = (string)base[this.tableSolutionStrategicAlignment.DRIVER_NAMEColumn];
					}
					catch (InvalidCastException innerException)
					{
						throw new StrongTypingException("The value for column 'DRIVER_NAME' in table 'SolutionStrategicAlignment' is DBNull.", innerException);
					}
					return result;
				}
				set
				{
					base[this.tableSolutionStrategicAlignment.DRIVER_NAMEColumn] = value;
				}
			}

			// Token: 0x17000D84 RID: 3460
			// (get) Token: 0x06002E2A RID: 11818 RVA: 0x00094124 File Offset: 0x00092324
			// (set) Token: 0x06002E2B RID: 11819 RVA: 0x0009413C File Offset: 0x0009233C
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			public double DRIVER_PRIORITY
			{
				get
				{
					return (double)base[this.tableSolutionStrategicAlignment.DRIVER_PRIORITYColumn];
				}
				set
				{
					base[this.tableSolutionStrategicAlignment.DRIVER_PRIORITYColumn] = value;
				}
			}

			// Token: 0x17000D85 RID: 3461
			// (get) Token: 0x06002E2C RID: 11820 RVA: 0x00094155 File Offset: 0x00092355
			// (set) Token: 0x06002E2D RID: 11821 RVA: 0x0009416D File Offset: 0x0009236D
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			public decimal REVERSE_VALUE
			{
				get
				{
					return (decimal)base[this.tableSolutionStrategicAlignment.REVERSE_VALUEColumn];
				}
				set
				{
					base[this.tableSolutionStrategicAlignment.REVERSE_VALUEColumn] = value;
				}
			}

			// Token: 0x17000D86 RID: 3462
			// (get) Token: 0x06002E2E RID: 11822 RVA: 0x00094186 File Offset: 0x00092386
			// (set) Token: 0x06002E2F RID: 11823 RVA: 0x000941A8 File Offset: 0x000923A8
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			public OptimizerSolutionDataSet.SolutionRow SolutionRow
			{
				get
				{
					return (OptimizerSolutionDataSet.SolutionRow)base.GetParentRow(base.Table.ParentRelations["FK_Solution_SolutionStrategicAlignment"]);
				}
				set
				{
					base.SetParentRow(value, base.Table.ParentRelations["FK_Solution_SolutionStrategicAlignment"]);
				}
			}

			// Token: 0x06002E30 RID: 11824 RVA: 0x000941C6 File Offset: 0x000923C6
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			public bool IsDRIVER_NAMENull()
			{
				return base.IsNull(this.tableSolutionStrategicAlignment.DRIVER_NAMEColumn);
			}

			// Token: 0x06002E31 RID: 11825 RVA: 0x000941D9 File Offset: 0x000923D9
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			public void SetDRIVER_NAMENull()
			{
				base[this.tableSolutionStrategicAlignment.DRIVER_NAMEColumn] = Convert.DBNull;
			}

			// Token: 0x040009B3 RID: 2483
			private OptimizerSolutionDataSet.SolutionStrategicAlignmentDataTable tableSolutionStrategicAlignment;
		}

		// Token: 0x02000238 RID: 568
		public class SolutionEfficientFrontierRow : DataRow
		{
			// Token: 0x06002E32 RID: 11826 RVA: 0x000941F1 File Offset: 0x000923F1
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			internal SolutionEfficientFrontierRow(DataRowBuilder rb) : base(rb)
			{
				this.tableSolutionEfficientFrontier = (OptimizerSolutionDataSet.SolutionEfficientFrontierDataTable)base.Table;
			}

			// Token: 0x17000D87 RID: 3463
			// (get) Token: 0x06002E33 RID: 11827 RVA: 0x0009420B File Offset: 0x0009240B
			// (set) Token: 0x06002E34 RID: 11828 RVA: 0x00094223 File Offset: 0x00092423
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
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

			// Token: 0x17000D88 RID: 3464
			// (get) Token: 0x06002E35 RID: 11829 RVA: 0x0009423C File Offset: 0x0009243C
			// (set) Token: 0x06002E36 RID: 11830 RVA: 0x00094254 File Offset: 0x00092454
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

			// Token: 0x17000D89 RID: 3465
			// (get) Token: 0x06002E37 RID: 11831 RVA: 0x0009426D File Offset: 0x0009246D
			// (set) Token: 0x06002E38 RID: 11832 RVA: 0x00094285 File Offset: 0x00092485
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
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

			// Token: 0x17000D8A RID: 3466
			// (get) Token: 0x06002E39 RID: 11833 RVA: 0x0009429E File Offset: 0x0009249E
			// (set) Token: 0x06002E3A RID: 11834 RVA: 0x000942B6 File Offset: 0x000924B6
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
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

			// Token: 0x17000D8B RID: 3467
			// (get) Token: 0x06002E3B RID: 11835 RVA: 0x000942CF File Offset: 0x000924CF
			// (set) Token: 0x06002E3C RID: 11836 RVA: 0x000942E7 File Offset: 0x000924E7
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

			// Token: 0x040009B4 RID: 2484
			private OptimizerSolutionDataSet.SolutionEfficientFrontierDataTable tableSolutionEfficientFrontier;
		}

		// Token: 0x02000239 RID: 569
		[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
		public class SolutionRowChangeEvent : EventArgs
		{
			// Token: 0x06002E3D RID: 11837 RVA: 0x00094300 File Offset: 0x00092500
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			public SolutionRowChangeEvent(OptimizerSolutionDataSet.SolutionRow row, DataRowAction action)
			{
				this.eventRow = row;
				this.eventAction = action;
			}

			// Token: 0x17000D8C RID: 3468
			// (get) Token: 0x06002E3E RID: 11838 RVA: 0x00094316 File Offset: 0x00092516
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			public OptimizerSolutionDataSet.SolutionRow Row
			{
				get
				{
					return this.eventRow;
				}
			}

			// Token: 0x17000D8D RID: 3469
			// (get) Token: 0x06002E3F RID: 11839 RVA: 0x0009431E File Offset: 0x0009251E
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			public DataRowAction Action
			{
				get
				{
					return this.eventAction;
				}
			}

			// Token: 0x040009B5 RID: 2485
			private OptimizerSolutionDataSet.SolutionRow eventRow;

			// Token: 0x040009B6 RID: 2486
			private DataRowAction eventAction;
		}

		// Token: 0x0200023A RID: 570
		[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
		public class SolutionConstraintsRowChangeEvent : EventArgs
		{
			// Token: 0x06002E40 RID: 11840 RVA: 0x00094326 File Offset: 0x00092526
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			public SolutionConstraintsRowChangeEvent(OptimizerSolutionDataSet.SolutionConstraintsRow row, DataRowAction action)
			{
				this.eventRow = row;
				this.eventAction = action;
			}

			// Token: 0x17000D8E RID: 3470
			// (get) Token: 0x06002E41 RID: 11841 RVA: 0x0009433C File Offset: 0x0009253C
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			public OptimizerSolutionDataSet.SolutionConstraintsRow Row
			{
				get
				{
					return this.eventRow;
				}
			}

			// Token: 0x17000D8F RID: 3471
			// (get) Token: 0x06002E42 RID: 11842 RVA: 0x00094344 File Offset: 0x00092544
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public DataRowAction Action
			{
				get
				{
					return this.eventAction;
				}
			}

			// Token: 0x040009B7 RID: 2487
			private OptimizerSolutionDataSet.SolutionConstraintsRow eventRow;

			// Token: 0x040009B8 RID: 2488
			private DataRowAction eventAction;
		}

		// Token: 0x0200023B RID: 571
		[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
		public class SolutionConstraintValuesRowChangeEvent : EventArgs
		{
			// Token: 0x06002E43 RID: 11843 RVA: 0x0009434C File Offset: 0x0009254C
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			public SolutionConstraintValuesRowChangeEvent(OptimizerSolutionDataSet.SolutionConstraintValuesRow row, DataRowAction action)
			{
				this.eventRow = row;
				this.eventAction = action;
			}

			// Token: 0x17000D90 RID: 3472
			// (get) Token: 0x06002E44 RID: 11844 RVA: 0x00094362 File Offset: 0x00092562
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			public OptimizerSolutionDataSet.SolutionConstraintValuesRow Row
			{
				get
				{
					return this.eventRow;
				}
			}

			// Token: 0x17000D91 RID: 3473
			// (get) Token: 0x06002E45 RID: 11845 RVA: 0x0009436A File Offset: 0x0009256A
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public DataRowAction Action
			{
				get
				{
					return this.eventAction;
				}
			}

			// Token: 0x040009B9 RID: 2489
			private OptimizerSolutionDataSet.SolutionConstraintValuesRow eventRow;

			// Token: 0x040009BA RID: 2490
			private DataRowAction eventAction;
		}

		// Token: 0x0200023C RID: 572
		[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
		public class SolutionProjectsRowChangeEvent : EventArgs
		{
			// Token: 0x06002E46 RID: 11846 RVA: 0x00094372 File Offset: 0x00092572
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			public SolutionProjectsRowChangeEvent(OptimizerSolutionDataSet.SolutionProjectsRow row, DataRowAction action)
			{
				this.eventRow = row;
				this.eventAction = action;
			}

			// Token: 0x17000D92 RID: 3474
			// (get) Token: 0x06002E47 RID: 11847 RVA: 0x00094388 File Offset: 0x00092588
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			public OptimizerSolutionDataSet.SolutionProjectsRow Row
			{
				get
				{
					return this.eventRow;
				}
			}

			// Token: 0x17000D93 RID: 3475
			// (get) Token: 0x06002E48 RID: 11848 RVA: 0x00094390 File Offset: 0x00092590
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public DataRowAction Action
			{
				get
				{
					return this.eventAction;
				}
			}

			// Token: 0x040009BB RID: 2491
			private OptimizerSolutionDataSet.SolutionProjectsRow eventRow;

			// Token: 0x040009BC RID: 2492
			private DataRowAction eventAction;
		}

		// Token: 0x0200023D RID: 573
		[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
		public class SolutionStrategicAlignmentRowChangeEvent : EventArgs
		{
			// Token: 0x06002E49 RID: 11849 RVA: 0x00094398 File Offset: 0x00092598
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public SolutionStrategicAlignmentRowChangeEvent(OptimizerSolutionDataSet.SolutionStrategicAlignmentRow row, DataRowAction action)
			{
				this.eventRow = row;
				this.eventAction = action;
			}

			// Token: 0x17000D94 RID: 3476
			// (get) Token: 0x06002E4A RID: 11850 RVA: 0x000943AE File Offset: 0x000925AE
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			public OptimizerSolutionDataSet.SolutionStrategicAlignmentRow Row
			{
				get
				{
					return this.eventRow;
				}
			}

			// Token: 0x17000D95 RID: 3477
			// (get) Token: 0x06002E4B RID: 11851 RVA: 0x000943B6 File Offset: 0x000925B6
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public DataRowAction Action
			{
				get
				{
					return this.eventAction;
				}
			}

			// Token: 0x040009BD RID: 2493
			private OptimizerSolutionDataSet.SolutionStrategicAlignmentRow eventRow;

			// Token: 0x040009BE RID: 2494
			private DataRowAction eventAction;
		}

		// Token: 0x0200023E RID: 574
		[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
		public class SolutionEfficientFrontierRowChangeEvent : EventArgs
		{
			// Token: 0x06002E4C RID: 11852 RVA: 0x000943BE File Offset: 0x000925BE
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			public SolutionEfficientFrontierRowChangeEvent(OptimizerSolutionDataSet.SolutionEfficientFrontierRow row, DataRowAction action)
			{
				this.eventRow = row;
				this.eventAction = action;
			}

			// Token: 0x17000D96 RID: 3478
			// (get) Token: 0x06002E4D RID: 11853 RVA: 0x000943D4 File Offset: 0x000925D4
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public OptimizerSolutionDataSet.SolutionEfficientFrontierRow Row
			{
				get
				{
					return this.eventRow;
				}
			}

			// Token: 0x17000D97 RID: 3479
			// (get) Token: 0x06002E4E RID: 11854 RVA: 0x000943DC File Offset: 0x000925DC
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			public DataRowAction Action
			{
				get
				{
					return this.eventAction;
				}
			}

			// Token: 0x040009BF RID: 2495
			private OptimizerSolutionDataSet.SolutionEfficientFrontierRow eventRow;

			// Token: 0x040009C0 RID: 2496
			private DataRowAction eventAction;
		}
	}
}
