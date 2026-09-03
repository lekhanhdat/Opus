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
	// Token: 0x02000103 RID: 259
	[XmlRoot("DriverPrioritizationDataSet")]
	[HelpKeyword("vs.data.DataSet")]
	[ToolboxItem(true)]
	[DesignerCategory("code")]
	[XmlSchemaProvider("GetTypedDataSetSchema")]
	[Serializable]
	public class DriverPrioritizationDataSet : DataSet
	{
		// Token: 0x060012F2 RID: 4850 RVA: 0x0003D980 File Offset: 0x0003BB80
		public new void EndInit()
		{
			base.EndInit();
			TypedDataSetUtilities.AllowNullsInNonTypedColumns(this.Prioritization, new string[]
			{
				"CREATED_DATE",
				"CREATED_BY_RES_UID",
				"PRIORITIZATION_IS_COMPLETE",
				"PRIORITIZATION_IS_MANUAL",
				"PRIORITIZATION_DESCRIPTION",
				"DEPARTMENT_NAME",
				"RELATIVE_IMPORTANCE_CF_NAME",
				"LAST_UPDATED_BY_RES_NAME",
				"PRIORITIZATION_UID",
				"PRIORITIZATION_IS_USED_IN_ANALYSIS",
				"CONSISTENCY_RATIO",
				"LAST_UPDATED_BY_RES_UID",
				"MOD_DATE",
				"PRIORITIZATION_NAME",
				"RELATIVE_IMPORTANCE_CF_UID",
				"DEPARTMENT_UID",
				"CREATED_BY_RES_NAME"
			});
			TypedDataSetUtilities.AllowNullsInNonTypedColumns(this.PrioritizationEntries, new string[]
			{
				"DRIVER_UID",
				"DRIVER_DESCRIPTION",
				"DRIVER_PRIORITY",
				"DRIVER_NAME",
				"DRIVER_IS_ACTIVE",
				"PRIORITIZATION_UID"
			});
			TypedDataSetUtilities.AllowNullsInNonTypedColumns(this.DriverRelations, new string[]
			{
				"DRIVER2_UID",
				"DRIVER1_UID",
				"PRIORITIZATION_UID",
				"LT_STRUCT_UID"
			});
		}

		// Token: 0x060012F3 RID: 4851 RVA: 0x0003DAB0 File Offset: 0x0003BCB0
		[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
		[DebuggerNonUserCode]
		public DriverPrioritizationDataSet()
		{
			base.BeginInit();
			this.InitClass();
			CollectionChangeEventHandler value = new CollectionChangeEventHandler(this.SchemaChanged);
			base.Tables.CollectionChanged += value;
			base.Relations.CollectionChanged += value;
			this.EndInit();
		}

		// Token: 0x060012F4 RID: 4852 RVA: 0x0003DB04 File Offset: 0x0003BD04
		[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
		[DebuggerNonUserCode]
		protected DriverPrioritizationDataSet(SerializationInfo info, StreamingContext context) : base(info, context, false)
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
				if (dataSet.Tables["Prioritization"] != null)
				{
					base.Tables.Add(new DriverPrioritizationDataSet.PrioritizationDataTable(dataSet.Tables["Prioritization"]));
				}
				if (dataSet.Tables["PrioritizationEntries"] != null)
				{
					base.Tables.Add(new DriverPrioritizationDataSet.PrioritizationEntriesDataTable(dataSet.Tables["PrioritizationEntries"]));
				}
				if (dataSet.Tables["DriverRelations"] != null)
				{
					base.Tables.Add(new DriverPrioritizationDataSet.DriverRelationsDataTable(dataSet.Tables["DriverRelations"]));
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

		// Token: 0x17000596 RID: 1430
		// (get) Token: 0x060012F5 RID: 4853 RVA: 0x0003DCC5 File Offset: 0x0003BEC5
		[Browsable(false)]
		[DebuggerNonUserCode]
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Content)]
		[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
		public DriverPrioritizationDataSet.PrioritizationDataTable Prioritization
		{
			get
			{
				return this.tablePrioritization;
			}
		}

		// Token: 0x17000597 RID: 1431
		// (get) Token: 0x060012F6 RID: 4854 RVA: 0x0003DCCD File Offset: 0x0003BECD
		[DebuggerNonUserCode]
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Content)]
		[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
		[Browsable(false)]
		public DriverPrioritizationDataSet.PrioritizationEntriesDataTable PrioritizationEntries
		{
			get
			{
				return this.tablePrioritizationEntries;
			}
		}

		// Token: 0x17000598 RID: 1432
		// (get) Token: 0x060012F7 RID: 4855 RVA: 0x0003DCD5 File Offset: 0x0003BED5
		[DebuggerNonUserCode]
		[Browsable(false)]
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Content)]
		[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
		public DriverPrioritizationDataSet.DriverRelationsDataTable DriverRelations
		{
			get
			{
				return this.tableDriverRelations;
			}
		}

		// Token: 0x17000599 RID: 1433
		// (get) Token: 0x060012F8 RID: 4856 RVA: 0x0003DCDD File Offset: 0x0003BEDD
		// (set) Token: 0x060012F9 RID: 4857 RVA: 0x0003DCE5 File Offset: 0x0003BEE5
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Visible)]
		[Browsable(true)]
		[DebuggerNonUserCode]
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

		// Token: 0x1700059A RID: 1434
		// (get) Token: 0x060012FA RID: 4858 RVA: 0x0003DCEE File Offset: 0x0003BEEE
		[DebuggerNonUserCode]
		[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
		public new DataTableCollection Tables
		{
			get
			{
				return base.Tables;
			}
		}

		// Token: 0x1700059B RID: 1435
		// (get) Token: 0x060012FB RID: 4859 RVA: 0x0003DCF6 File Offset: 0x0003BEF6
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

		// Token: 0x060012FC RID: 4860 RVA: 0x0003DCFE File Offset: 0x0003BEFE
		[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
		[DebuggerNonUserCode]
		protected override void InitializeDerivedDataSet()
		{
			base.BeginInit();
			this.InitClass();
			this.EndInit();
		}

		// Token: 0x060012FD RID: 4861 RVA: 0x0003DD14 File Offset: 0x0003BF14
		[DebuggerNonUserCode]
		[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
		public override DataSet Clone()
		{
			DriverPrioritizationDataSet driverPrioritizationDataSet = (DriverPrioritizationDataSet)base.Clone();
			driverPrioritizationDataSet.InitVars();
			driverPrioritizationDataSet.SchemaSerializationMode = this.SchemaSerializationMode;
			return driverPrioritizationDataSet;
		}

		// Token: 0x060012FE RID: 4862 RVA: 0x0003DD40 File Offset: 0x0003BF40
		[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
		[DebuggerNonUserCode]
		protected override bool ShouldSerializeTables()
		{
			return false;
		}

		// Token: 0x060012FF RID: 4863 RVA: 0x0003DD43 File Offset: 0x0003BF43
		[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
		[DebuggerNonUserCode]
		protected override bool ShouldSerializeRelations()
		{
			return false;
		}

		// Token: 0x06001300 RID: 4864 RVA: 0x0003DD48 File Offset: 0x0003BF48
		[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
		[DebuggerNonUserCode]
		protected override void ReadXmlSerializable(XmlReader reader)
		{
			if (base.DetermineSchemaSerializationMode(reader) == SchemaSerializationMode.IncludeSchema)
			{
				this.Reset();
				DataSet dataSet = new DataSet();
				dataSet.ReadXml(reader);
				if (dataSet.Tables["Prioritization"] != null)
				{
					base.Tables.Add(new DriverPrioritizationDataSet.PrioritizationDataTable(dataSet.Tables["Prioritization"]));
				}
				if (dataSet.Tables["PrioritizationEntries"] != null)
				{
					base.Tables.Add(new DriverPrioritizationDataSet.PrioritizationEntriesDataTable(dataSet.Tables["PrioritizationEntries"]));
				}
				if (dataSet.Tables["DriverRelations"] != null)
				{
					base.Tables.Add(new DriverPrioritizationDataSet.DriverRelationsDataTable(dataSet.Tables["DriverRelations"]));
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

		// Token: 0x06001301 RID: 4865 RVA: 0x0003DE74 File Offset: 0x0003C074
		[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
		[DebuggerNonUserCode]
		protected override XmlSchema GetSchemaSerializable()
		{
			MemoryStream memoryStream = new MemoryStream();
			base.WriteXmlSchema(new XmlTextWriter(memoryStream, null));
			memoryStream.Position = 0L;
			return XmlSchema.Read(new XmlTextReader(memoryStream), null);
		}

		// Token: 0x06001302 RID: 4866 RVA: 0x0003DEA8 File Offset: 0x0003C0A8
		[DebuggerNonUserCode]
		[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
		internal void InitVars()
		{
			this.InitVars(true);
		}

		// Token: 0x06001303 RID: 4867 RVA: 0x0003DEB4 File Offset: 0x0003C0B4
		[DebuggerNonUserCode]
		[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
		internal void InitVars(bool initTable)
		{
			this.tablePrioritization = (DriverPrioritizationDataSet.PrioritizationDataTable)base.Tables["Prioritization"];
			if (initTable && this.tablePrioritization != null)
			{
				this.tablePrioritization.InitVars();
			}
			this.tablePrioritizationEntries = (DriverPrioritizationDataSet.PrioritizationEntriesDataTable)base.Tables["PrioritizationEntries"];
			if (initTable && this.tablePrioritizationEntries != null)
			{
				this.tablePrioritizationEntries.InitVars();
			}
			this.tableDriverRelations = (DriverPrioritizationDataSet.DriverRelationsDataTable)base.Tables["DriverRelations"];
			if (initTable && this.tableDriverRelations != null)
			{
				this.tableDriverRelations.InitVars();
			}
			this.relationFK_Prioritization_PrioritizationEntries = this.Relations["FK_Prioritization_PrioritizationEntries"];
			this.relationFK_PrioritizationEntries_DriverRelations1 = this.Relations["FK_PrioritizationEntries_DriverRelations1"];
			this.relationFK_PrioritizationEntries_DriverRelations2 = this.Relations["FK_PrioritizationEntries_DriverRelations2"];
		}

		// Token: 0x06001304 RID: 4868 RVA: 0x0003DF98 File Offset: 0x0003C198
		[DebuggerNonUserCode]
		[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
		private void InitClass()
		{
			base.DataSetName = "DriverPrioritizationDataSet";
			base.Prefix = "";
			base.Namespace = "http://schemas.microsoft.com/office/project/server/webservices/DriverPrioritizationDataSet/";
			base.EnforceConstraints = true;
			this.SchemaSerializationMode = SchemaSerializationMode.IncludeSchema;
			this.tablePrioritization = new DriverPrioritizationDataSet.PrioritizationDataTable();
			base.Tables.Add(this.tablePrioritization);
			this.tablePrioritizationEntries = new DriverPrioritizationDataSet.PrioritizationEntriesDataTable();
			base.Tables.Add(this.tablePrioritizationEntries);
			this.tableDriverRelations = new DriverPrioritizationDataSet.DriverRelationsDataTable();
			base.Tables.Add(this.tableDriverRelations);
			ForeignKeyConstraint foreignKeyConstraint = new ForeignKeyConstraint("FK_Prioritization_PrioritizationEntries", new DataColumn[]
			{
				this.tablePrioritization.PRIORITIZATION_UIDColumn
			}, new DataColumn[]
			{
				this.tablePrioritizationEntries.PRIORITIZATION_UIDColumn
			});
			this.tablePrioritizationEntries.Constraints.Add(foreignKeyConstraint);
			foreignKeyConstraint.AcceptRejectRule = AcceptRejectRule.None;
			foreignKeyConstraint.DeleteRule = Rule.None;
			foreignKeyConstraint.UpdateRule = Rule.None;
			foreignKeyConstraint = new ForeignKeyConstraint("FK_PrioritizationEntries_DriverRelations1", new DataColumn[]
			{
				this.tablePrioritizationEntries.PRIORITIZATION_UIDColumn,
				this.tablePrioritizationEntries.DRIVER_UIDColumn
			}, new DataColumn[]
			{
				this.tableDriverRelations.PRIORITIZATION_UIDColumn,
				this.tableDriverRelations.DRIVER1_UIDColumn
			});
			this.tableDriverRelations.Constraints.Add(foreignKeyConstraint);
			foreignKeyConstraint.AcceptRejectRule = AcceptRejectRule.None;
			foreignKeyConstraint.DeleteRule = Rule.None;
			foreignKeyConstraint.UpdateRule = Rule.None;
			foreignKeyConstraint = new ForeignKeyConstraint("FK_PrioritizationEntries_DriverRelations2", new DataColumn[]
			{
				this.tablePrioritizationEntries.PRIORITIZATION_UIDColumn,
				this.tablePrioritizationEntries.DRIVER_UIDColumn
			}, new DataColumn[]
			{
				this.tableDriverRelations.PRIORITIZATION_UIDColumn,
				this.tableDriverRelations.DRIVER2_UIDColumn
			});
			this.tableDriverRelations.Constraints.Add(foreignKeyConstraint);
			foreignKeyConstraint.AcceptRejectRule = AcceptRejectRule.None;
			foreignKeyConstraint.DeleteRule = Rule.None;
			foreignKeyConstraint.UpdateRule = Rule.None;
			this.relationFK_Prioritization_PrioritizationEntries = new DataRelation("FK_Prioritization_PrioritizationEntries", new DataColumn[]
			{
				this.tablePrioritization.PRIORITIZATION_UIDColumn
			}, new DataColumn[]
			{
				this.tablePrioritizationEntries.PRIORITIZATION_UIDColumn
			}, false);
			this.Relations.Add(this.relationFK_Prioritization_PrioritizationEntries);
			this.relationFK_PrioritizationEntries_DriverRelations1 = new DataRelation("FK_PrioritizationEntries_DriverRelations1", new DataColumn[]
			{
				this.tablePrioritizationEntries.PRIORITIZATION_UIDColumn,
				this.tablePrioritizationEntries.DRIVER_UIDColumn
			}, new DataColumn[]
			{
				this.tableDriverRelations.PRIORITIZATION_UIDColumn,
				this.tableDriverRelations.DRIVER1_UIDColumn
			}, false);
			this.Relations.Add(this.relationFK_PrioritizationEntries_DriverRelations1);
			this.relationFK_PrioritizationEntries_DriverRelations2 = new DataRelation("FK_PrioritizationEntries_DriverRelations2", new DataColumn[]
			{
				this.tablePrioritizationEntries.PRIORITIZATION_UIDColumn,
				this.tablePrioritizationEntries.DRIVER_UIDColumn
			}, new DataColumn[]
			{
				this.tableDriverRelations.PRIORITIZATION_UIDColumn,
				this.tableDriverRelations.DRIVER2_UIDColumn
			}, false);
			this.Relations.Add(this.relationFK_PrioritizationEntries_DriverRelations2);
		}

		// Token: 0x06001305 RID: 4869 RVA: 0x0003E2BB File Offset: 0x0003C4BB
		[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
		[DebuggerNonUserCode]
		private bool ShouldSerializePrioritization()
		{
			return false;
		}

		// Token: 0x06001306 RID: 4870 RVA: 0x0003E2BE File Offset: 0x0003C4BE
		[DebuggerNonUserCode]
		[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
		private bool ShouldSerializePrioritizationEntries()
		{
			return false;
		}

		// Token: 0x06001307 RID: 4871 RVA: 0x0003E2C1 File Offset: 0x0003C4C1
		[DebuggerNonUserCode]
		[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
		private bool ShouldSerializeDriverRelations()
		{
			return false;
		}

		// Token: 0x06001308 RID: 4872 RVA: 0x0003E2C4 File Offset: 0x0003C4C4
		[DebuggerNonUserCode]
		[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
		private void SchemaChanged(object sender, CollectionChangeEventArgs e)
		{
			if (e.Action == CollectionChangeAction.Remove)
			{
				this.InitVars();
			}
		}

		// Token: 0x06001309 RID: 4873 RVA: 0x0003E2D8 File Offset: 0x0003C4D8
		[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
		[DebuggerNonUserCode]
		public static XmlSchemaComplexType GetTypedDataSetSchema(XmlSchemaSet xs)
		{
			DriverPrioritizationDataSet driverPrioritizationDataSet = new DriverPrioritizationDataSet();
			XmlSchemaComplexType xmlSchemaComplexType = new XmlSchemaComplexType();
			XmlSchemaSequence xmlSchemaSequence = new XmlSchemaSequence();
			XmlSchemaAny xmlSchemaAny = new XmlSchemaAny();
			xmlSchemaAny.Namespace = driverPrioritizationDataSet.Namespace;
			xmlSchemaSequence.Items.Add(xmlSchemaAny);
			xmlSchemaComplexType.Particle = xmlSchemaSequence;
			XmlSchema schemaSerializable = driverPrioritizationDataSet.GetSchemaSerializable();
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

		// Token: 0x04000444 RID: 1092
		private DriverPrioritizationDataSet.PrioritizationDataTable tablePrioritization;

		// Token: 0x04000445 RID: 1093
		private DriverPrioritizationDataSet.PrioritizationEntriesDataTable tablePrioritizationEntries;

		// Token: 0x04000446 RID: 1094
		private DriverPrioritizationDataSet.DriverRelationsDataTable tableDriverRelations;

		// Token: 0x04000447 RID: 1095
		private DataRelation relationFK_Prioritization_PrioritizationEntries;

		// Token: 0x04000448 RID: 1096
		private DataRelation relationFK_PrioritizationEntries_DriverRelations1;

		// Token: 0x04000449 RID: 1097
		private DataRelation relationFK_PrioritizationEntries_DriverRelations2;

		// Token: 0x0400044A RID: 1098
		private SchemaSerializationMode _schemaSerializationMode = SchemaSerializationMode.IncludeSchema;

		// Token: 0x02000104 RID: 260
		// (Invoke) Token: 0x0600130B RID: 4875
		[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
		public delegate void PrioritizationRowChangeEventHandler(object sender, DriverPrioritizationDataSet.PrioritizationRowChangeEvent e);

		// Token: 0x02000105 RID: 261
		// (Invoke) Token: 0x0600130F RID: 4879
		[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
		public delegate void PrioritizationEntriesRowChangeEventHandler(object sender, DriverPrioritizationDataSet.PrioritizationEntriesRowChangeEvent e);

		// Token: 0x02000106 RID: 262
		// (Invoke) Token: 0x06001313 RID: 4883
		[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
		public delegate void DriverRelationsRowChangeEventHandler(object sender, DriverPrioritizationDataSet.DriverRelationsRowChangeEvent e);

		// Token: 0x02000107 RID: 263
		[XmlSchemaProvider("GetTypedTableSchema")]
		[Serializable]
		public class PrioritizationDataTable : DataTable, IEnumerable
		{
			// Token: 0x06001316 RID: 4886 RVA: 0x0003E420 File Offset: 0x0003C620
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public PrioritizationDataTable()
			{
				base.TableName = "Prioritization";
				this.BeginInit();
				this.InitClass();
				this.EndInit();
			}

			// Token: 0x06001317 RID: 4887 RVA: 0x0003E448 File Offset: 0x0003C648
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			internal PrioritizationDataTable(DataTable table)
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

			// Token: 0x06001318 RID: 4888 RVA: 0x0003E4F0 File Offset: 0x0003C6F0
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			protected PrioritizationDataTable(SerializationInfo info, StreamingContext context) : base(info, context)
			{
				this.InitVars();
			}

			// Token: 0x1700059C RID: 1436
			// (get) Token: 0x06001319 RID: 4889 RVA: 0x0003E500 File Offset: 0x0003C700
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			public DataColumn PRIORITIZATION_UIDColumn
			{
				get
				{
					return this.columnPRIORITIZATION_UID;
				}
			}

			// Token: 0x1700059D RID: 1437
			// (get) Token: 0x0600131A RID: 4890 RVA: 0x0003E508 File Offset: 0x0003C708
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public DataColumn PRIORITIZATION_NAMEColumn
			{
				get
				{
					return this.columnPRIORITIZATION_NAME;
				}
			}

			// Token: 0x1700059E RID: 1438
			// (get) Token: 0x0600131B RID: 4891 RVA: 0x0003E510 File Offset: 0x0003C710
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			public DataColumn PRIORITIZATION_DESCRIPTIONColumn
			{
				get
				{
					return this.columnPRIORITIZATION_DESCRIPTION;
				}
			}

			// Token: 0x1700059F RID: 1439
			// (get) Token: 0x0600131C RID: 4892 RVA: 0x0003E518 File Offset: 0x0003C718
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public DataColumn PRIORITIZATION_IS_MANUALColumn
			{
				get
				{
					return this.columnPRIORITIZATION_IS_MANUAL;
				}
			}

			// Token: 0x170005A0 RID: 1440
			// (get) Token: 0x0600131D RID: 4893 RVA: 0x0003E520 File Offset: 0x0003C720
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public DataColumn PRIORITIZATION_IS_COMPLETEColumn
			{
				get
				{
					return this.columnPRIORITIZATION_IS_COMPLETE;
				}
			}

			// Token: 0x170005A1 RID: 1441
			// (get) Token: 0x0600131E RID: 4894 RVA: 0x0003E528 File Offset: 0x0003C728
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			public DataColumn DEPARTMENT_UIDColumn
			{
				get
				{
					return this.columnDEPARTMENT_UID;
				}
			}

			// Token: 0x170005A2 RID: 1442
			// (get) Token: 0x0600131F RID: 4895 RVA: 0x0003E530 File Offset: 0x0003C730
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public DataColumn DEPARTMENT_NAMEColumn
			{
				get
				{
					return this.columnDEPARTMENT_NAME;
				}
			}

			// Token: 0x170005A3 RID: 1443
			// (get) Token: 0x06001320 RID: 4896 RVA: 0x0003E538 File Offset: 0x0003C738
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public DataColumn RELATIVE_IMPORTANCE_CF_UIDColumn
			{
				get
				{
					return this.columnRELATIVE_IMPORTANCE_CF_UID;
				}
			}

			// Token: 0x170005A4 RID: 1444
			// (get) Token: 0x06001321 RID: 4897 RVA: 0x0003E540 File Offset: 0x0003C740
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			public DataColumn RELATIVE_IMPORTANCE_CF_NAMEColumn
			{
				get
				{
					return this.columnRELATIVE_IMPORTANCE_CF_NAME;
				}
			}

			// Token: 0x170005A5 RID: 1445
			// (get) Token: 0x06001322 RID: 4898 RVA: 0x0003E548 File Offset: 0x0003C748
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public DataColumn CONSISTENCY_RATIOColumn
			{
				get
				{
					return this.columnCONSISTENCY_RATIO;
				}
			}

			// Token: 0x170005A6 RID: 1446
			// (get) Token: 0x06001323 RID: 4899 RVA: 0x0003E550 File Offset: 0x0003C750
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public DataColumn PRIORITIZATION_IS_USED_IN_ANALYSISColumn
			{
				get
				{
					return this.columnPRIORITIZATION_IS_USED_IN_ANALYSIS;
				}
			}

			// Token: 0x170005A7 RID: 1447
			// (get) Token: 0x06001324 RID: 4900 RVA: 0x0003E558 File Offset: 0x0003C758
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public DataColumn CREATED_DATEColumn
			{
				get
				{
					return this.columnCREATED_DATE;
				}
			}

			// Token: 0x170005A8 RID: 1448
			// (get) Token: 0x06001325 RID: 4901 RVA: 0x0003E560 File Offset: 0x0003C760
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public DataColumn MOD_DATEColumn
			{
				get
				{
					return this.columnMOD_DATE;
				}
			}

			// Token: 0x170005A9 RID: 1449
			// (get) Token: 0x06001326 RID: 4902 RVA: 0x0003E568 File Offset: 0x0003C768
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public DataColumn LAST_UPDATED_BY_RES_UIDColumn
			{
				get
				{
					return this.columnLAST_UPDATED_BY_RES_UID;
				}
			}

			// Token: 0x170005AA RID: 1450
			// (get) Token: 0x06001327 RID: 4903 RVA: 0x0003E570 File Offset: 0x0003C770
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public DataColumn LAST_UPDATED_BY_RES_NAMEColumn
			{
				get
				{
					return this.columnLAST_UPDATED_BY_RES_NAME;
				}
			}

			// Token: 0x170005AB RID: 1451
			// (get) Token: 0x06001328 RID: 4904 RVA: 0x0003E578 File Offset: 0x0003C778
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public DataColumn CREATED_BY_RES_UIDColumn
			{
				get
				{
					return this.columnCREATED_BY_RES_UID;
				}
			}

			// Token: 0x170005AC RID: 1452
			// (get) Token: 0x06001329 RID: 4905 RVA: 0x0003E580 File Offset: 0x0003C780
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public DataColumn CREATED_BY_RES_NAMEColumn
			{
				get
				{
					return this.columnCREATED_BY_RES_NAME;
				}
			}

			// Token: 0x170005AD RID: 1453
			// (get) Token: 0x0600132A RID: 4906 RVA: 0x0003E588 File Offset: 0x0003C788
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

			// Token: 0x170005AE RID: 1454
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public DriverPrioritizationDataSet.PrioritizationRow this[int index]
			{
				get
				{
					return (DriverPrioritizationDataSet.PrioritizationRow)base.Rows[index];
				}
			}

			// Token: 0x140000D5 RID: 213
			// (add) Token: 0x0600132C RID: 4908 RVA: 0x0003E5A8 File Offset: 0x0003C7A8
			// (remove) Token: 0x0600132D RID: 4909 RVA: 0x0003E5E0 File Offset: 0x0003C7E0
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public event DriverPrioritizationDataSet.PrioritizationRowChangeEventHandler PrioritizationRowChanging;

			// Token: 0x140000D6 RID: 214
			// (add) Token: 0x0600132E RID: 4910 RVA: 0x0003E618 File Offset: 0x0003C818
			// (remove) Token: 0x0600132F RID: 4911 RVA: 0x0003E650 File Offset: 0x0003C850
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public event DriverPrioritizationDataSet.PrioritizationRowChangeEventHandler PrioritizationRowChanged;

			// Token: 0x140000D7 RID: 215
			// (add) Token: 0x06001330 RID: 4912 RVA: 0x0003E688 File Offset: 0x0003C888
			// (remove) Token: 0x06001331 RID: 4913 RVA: 0x0003E6C0 File Offset: 0x0003C8C0
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public event DriverPrioritizationDataSet.PrioritizationRowChangeEventHandler PrioritizationRowDeleting;

			// Token: 0x140000D8 RID: 216
			// (add) Token: 0x06001332 RID: 4914 RVA: 0x0003E6F8 File Offset: 0x0003C8F8
			// (remove) Token: 0x06001333 RID: 4915 RVA: 0x0003E730 File Offset: 0x0003C930
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public event DriverPrioritizationDataSet.PrioritizationRowChangeEventHandler PrioritizationRowDeleted;

			// Token: 0x06001334 RID: 4916 RVA: 0x0003E765 File Offset: 0x0003C965
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			public void AddPrioritizationRow(DriverPrioritizationDataSet.PrioritizationRow row)
			{
				base.Rows.Add(row);
			}

			// Token: 0x06001335 RID: 4917 RVA: 0x0003E774 File Offset: 0x0003C974
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			public DriverPrioritizationDataSet.PrioritizationRow AddPrioritizationRow(Guid PRIORITIZATION_UID, string PRIORITIZATION_NAME, string PRIORITIZATION_DESCRIPTION, bool PRIORITIZATION_IS_MANUAL, bool PRIORITIZATION_IS_COMPLETE, Guid DEPARTMENT_UID, string DEPARTMENT_NAME, Guid RELATIVE_IMPORTANCE_CF_UID, string RELATIVE_IMPORTANCE_CF_NAME, double CONSISTENCY_RATIO, bool PRIORITIZATION_IS_USED_IN_ANALYSIS, DateTime CREATED_DATE, DateTime MOD_DATE, Guid LAST_UPDATED_BY_RES_UID, string LAST_UPDATED_BY_RES_NAME, Guid CREATED_BY_RES_UID, string CREATED_BY_RES_NAME)
			{
				DriverPrioritizationDataSet.PrioritizationRow prioritizationRow = (DriverPrioritizationDataSet.PrioritizationRow)base.NewRow();
				object[] itemArray = new object[]
				{
					PRIORITIZATION_UID,
					PRIORITIZATION_NAME,
					PRIORITIZATION_DESCRIPTION,
					PRIORITIZATION_IS_MANUAL,
					PRIORITIZATION_IS_COMPLETE,
					DEPARTMENT_UID,
					DEPARTMENT_NAME,
					RELATIVE_IMPORTANCE_CF_UID,
					RELATIVE_IMPORTANCE_CF_NAME,
					CONSISTENCY_RATIO,
					PRIORITIZATION_IS_USED_IN_ANALYSIS,
					CREATED_DATE,
					MOD_DATE,
					LAST_UPDATED_BY_RES_UID,
					LAST_UPDATED_BY_RES_NAME,
					CREATED_BY_RES_UID,
					CREATED_BY_RES_NAME
				};
				prioritizationRow.ItemArray = itemArray;
				base.Rows.Add(prioritizationRow);
				return prioritizationRow;
			}

			// Token: 0x06001336 RID: 4918 RVA: 0x0003E83C File Offset: 0x0003CA3C
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public DriverPrioritizationDataSet.PrioritizationRow FindByPRIORITIZATION_UID(Guid PRIORITIZATION_UID)
			{
				return (DriverPrioritizationDataSet.PrioritizationRow)base.Rows.Find(new object[]
				{
					PRIORITIZATION_UID
				});
			}

			// Token: 0x06001337 RID: 4919 RVA: 0x0003E86A File Offset: 0x0003CA6A
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public virtual IEnumerator GetEnumerator()
			{
				return base.Rows.GetEnumerator();
			}

			// Token: 0x06001338 RID: 4920 RVA: 0x0003E878 File Offset: 0x0003CA78
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public override DataTable Clone()
			{
				DriverPrioritizationDataSet.PrioritizationDataTable prioritizationDataTable = (DriverPrioritizationDataSet.PrioritizationDataTable)base.Clone();
				prioritizationDataTable.InitVars();
				return prioritizationDataTable;
			}

			// Token: 0x06001339 RID: 4921 RVA: 0x0003E898 File Offset: 0x0003CA98
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			protected override DataTable CreateInstance()
			{
				return new DriverPrioritizationDataSet.PrioritizationDataTable();
			}

			// Token: 0x0600133A RID: 4922 RVA: 0x0003E8A0 File Offset: 0x0003CAA0
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			internal void InitVars()
			{
				this.columnPRIORITIZATION_UID = base.Columns["PRIORITIZATION_UID"];
				this.columnPRIORITIZATION_NAME = base.Columns["PRIORITIZATION_NAME"];
				this.columnPRIORITIZATION_DESCRIPTION = base.Columns["PRIORITIZATION_DESCRIPTION"];
				this.columnPRIORITIZATION_IS_MANUAL = base.Columns["PRIORITIZATION_IS_MANUAL"];
				this.columnPRIORITIZATION_IS_COMPLETE = base.Columns["PRIORITIZATION_IS_COMPLETE"];
				this.columnDEPARTMENT_UID = base.Columns["DEPARTMENT_UID"];
				this.columnDEPARTMENT_NAME = base.Columns["DEPARTMENT_NAME"];
				this.columnRELATIVE_IMPORTANCE_CF_UID = base.Columns["RELATIVE_IMPORTANCE_CF_UID"];
				this.columnRELATIVE_IMPORTANCE_CF_NAME = base.Columns["RELATIVE_IMPORTANCE_CF_NAME"];
				this.columnCONSISTENCY_RATIO = base.Columns["CONSISTENCY_RATIO"];
				this.columnPRIORITIZATION_IS_USED_IN_ANALYSIS = base.Columns["PRIORITIZATION_IS_USED_IN_ANALYSIS"];
				this.columnCREATED_DATE = base.Columns["CREATED_DATE"];
				this.columnMOD_DATE = base.Columns["MOD_DATE"];
				this.columnLAST_UPDATED_BY_RES_UID = base.Columns["LAST_UPDATED_BY_RES_UID"];
				this.columnLAST_UPDATED_BY_RES_NAME = base.Columns["LAST_UPDATED_BY_RES_NAME"];
				this.columnCREATED_BY_RES_UID = base.Columns["CREATED_BY_RES_UID"];
				this.columnCREATED_BY_RES_NAME = base.Columns["CREATED_BY_RES_NAME"];
			}

			// Token: 0x0600133B RID: 4923 RVA: 0x0003EA24 File Offset: 0x0003CC24
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			private void InitClass()
			{
				this.columnPRIORITIZATION_UID = new DataColumn("PRIORITIZATION_UID", typeof(Guid), null, MappingType.Element);
				base.Columns.Add(this.columnPRIORITIZATION_UID);
				this.columnPRIORITIZATION_NAME = new DataColumn("PRIORITIZATION_NAME", typeof(string), null, MappingType.Element);
				base.Columns.Add(this.columnPRIORITIZATION_NAME);
				this.columnPRIORITIZATION_DESCRIPTION = new DataColumn("PRIORITIZATION_DESCRIPTION", typeof(string), null, MappingType.Element);
				base.Columns.Add(this.columnPRIORITIZATION_DESCRIPTION);
				this.columnPRIORITIZATION_IS_MANUAL = new DataColumn("PRIORITIZATION_IS_MANUAL", typeof(bool), null, MappingType.Element);
				base.Columns.Add(this.columnPRIORITIZATION_IS_MANUAL);
				this.columnPRIORITIZATION_IS_COMPLETE = new DataColumn("PRIORITIZATION_IS_COMPLETE", typeof(bool), null, MappingType.Element);
				base.Columns.Add(this.columnPRIORITIZATION_IS_COMPLETE);
				this.columnDEPARTMENT_UID = new DataColumn("DEPARTMENT_UID", typeof(Guid), null, MappingType.Element);
				base.Columns.Add(this.columnDEPARTMENT_UID);
				this.columnDEPARTMENT_NAME = new DataColumn("DEPARTMENT_NAME", typeof(string), null, MappingType.Element);
				base.Columns.Add(this.columnDEPARTMENT_NAME);
				this.columnRELATIVE_IMPORTANCE_CF_UID = new DataColumn("RELATIVE_IMPORTANCE_CF_UID", typeof(Guid), null, MappingType.Element);
				base.Columns.Add(this.columnRELATIVE_IMPORTANCE_CF_UID);
				this.columnRELATIVE_IMPORTANCE_CF_NAME = new DataColumn("RELATIVE_IMPORTANCE_CF_NAME", typeof(string), null, MappingType.Element);
				base.Columns.Add(this.columnRELATIVE_IMPORTANCE_CF_NAME);
				this.columnCONSISTENCY_RATIO = new DataColumn("CONSISTENCY_RATIO", typeof(double), null, MappingType.Element);
				base.Columns.Add(this.columnCONSISTENCY_RATIO);
				this.columnPRIORITIZATION_IS_USED_IN_ANALYSIS = new DataColumn("PRIORITIZATION_IS_USED_IN_ANALYSIS", typeof(bool), null, MappingType.Element);
				base.Columns.Add(this.columnPRIORITIZATION_IS_USED_IN_ANALYSIS);
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
				base.Constraints.Add(new UniqueConstraint("Prioritization_Key", new DataColumn[]
				{
					this.columnPRIORITIZATION_UID
				}, true));
				this.columnPRIORITIZATION_UID.AllowDBNull = false;
				this.columnPRIORITIZATION_UID.Unique = true;
				this.columnPRIORITIZATION_NAME.AllowDBNull = false;
				this.columnPRIORITIZATION_IS_MANUAL.AllowDBNull = false;
				this.columnPRIORITIZATION_IS_MANUAL.DefaultValue = false;
				this.columnPRIORITIZATION_IS_COMPLETE.ReadOnly = true;
				this.columnDEPARTMENT_NAME.ReadOnly = true;
				this.columnRELATIVE_IMPORTANCE_CF_UID.AllowDBNull = false;
				this.columnRELATIVE_IMPORTANCE_CF_NAME.ReadOnly = true;
				this.columnCONSISTENCY_RATIO.AllowDBNull = false;
				this.columnCONSISTENCY_RATIO.DefaultValue = 1.0;
				this.columnPRIORITIZATION_IS_USED_IN_ANALYSIS.ReadOnly = true;
				this.columnLAST_UPDATED_BY_RES_NAME.ReadOnly = true;
				this.columnCREATED_BY_RES_NAME.ReadOnly = true;
			}

			// Token: 0x0600133C RID: 4924 RVA: 0x0003EE0F File Offset: 0x0003D00F
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public DriverPrioritizationDataSet.PrioritizationRow NewPrioritizationRow()
			{
				return (DriverPrioritizationDataSet.PrioritizationRow)base.NewRow();
			}

			// Token: 0x0600133D RID: 4925 RVA: 0x0003EE1C File Offset: 0x0003D01C
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			protected override DataRow NewRowFromBuilder(DataRowBuilder builder)
			{
				return new DriverPrioritizationDataSet.PrioritizationRow(builder);
			}

			// Token: 0x0600133E RID: 4926 RVA: 0x0003EE24 File Offset: 0x0003D024
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			protected override Type GetRowType()
			{
				return typeof(DriverPrioritizationDataSet.PrioritizationRow);
			}

			// Token: 0x0600133F RID: 4927 RVA: 0x0003EE30 File Offset: 0x0003D030
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			protected override void OnRowChanged(DataRowChangeEventArgs e)
			{
				base.OnRowChanged(e);
				if (this.PrioritizationRowChanged != null)
				{
					this.PrioritizationRowChanged(this, new DriverPrioritizationDataSet.PrioritizationRowChangeEvent((DriverPrioritizationDataSet.PrioritizationRow)e.Row, e.Action));
				}
			}

			// Token: 0x06001340 RID: 4928 RVA: 0x0003EE63 File Offset: 0x0003D063
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			protected override void OnRowChanging(DataRowChangeEventArgs e)
			{
				base.OnRowChanging(e);
				if (this.PrioritizationRowChanging != null)
				{
					this.PrioritizationRowChanging(this, new DriverPrioritizationDataSet.PrioritizationRowChangeEvent((DriverPrioritizationDataSet.PrioritizationRow)e.Row, e.Action));
				}
			}

			// Token: 0x06001341 RID: 4929 RVA: 0x0003EE96 File Offset: 0x0003D096
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			protected override void OnRowDeleted(DataRowChangeEventArgs e)
			{
				base.OnRowDeleted(e);
				if (this.PrioritizationRowDeleted != null)
				{
					this.PrioritizationRowDeleted(this, new DriverPrioritizationDataSet.PrioritizationRowChangeEvent((DriverPrioritizationDataSet.PrioritizationRow)e.Row, e.Action));
				}
			}

			// Token: 0x06001342 RID: 4930 RVA: 0x0003EEC9 File Offset: 0x0003D0C9
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			protected override void OnRowDeleting(DataRowChangeEventArgs e)
			{
				base.OnRowDeleting(e);
				if (this.PrioritizationRowDeleting != null)
				{
					this.PrioritizationRowDeleting(this, new DriverPrioritizationDataSet.PrioritizationRowChangeEvent((DriverPrioritizationDataSet.PrioritizationRow)e.Row, e.Action));
				}
			}

			// Token: 0x06001343 RID: 4931 RVA: 0x0003EEFC File Offset: 0x0003D0FC
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public void RemovePrioritizationRow(DriverPrioritizationDataSet.PrioritizationRow row)
			{
				base.Rows.Remove(row);
			}

			// Token: 0x06001344 RID: 4932 RVA: 0x0003EF0C File Offset: 0x0003D10C
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			public static XmlSchemaComplexType GetTypedTableSchema(XmlSchemaSet xs)
			{
				XmlSchemaComplexType xmlSchemaComplexType = new XmlSchemaComplexType();
				XmlSchemaSequence xmlSchemaSequence = new XmlSchemaSequence();
				DriverPrioritizationDataSet driverPrioritizationDataSet = new DriverPrioritizationDataSet();
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
				xmlSchemaAttribute.FixedValue = driverPrioritizationDataSet.Namespace;
				xmlSchemaComplexType.Attributes.Add(xmlSchemaAttribute);
				XmlSchemaAttribute xmlSchemaAttribute2 = new XmlSchemaAttribute();
				xmlSchemaAttribute2.Name = "tableTypeName";
				xmlSchemaAttribute2.FixedValue = "PrioritizationDataTable";
				xmlSchemaComplexType.Attributes.Add(xmlSchemaAttribute2);
				xmlSchemaComplexType.Particle = xmlSchemaSequence;
				XmlSchema schemaSerializable = driverPrioritizationDataSet.GetSchemaSerializable();
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

			// Token: 0x0400044B RID: 1099
			private DataColumn columnPRIORITIZATION_UID;

			// Token: 0x0400044C RID: 1100
			private DataColumn columnPRIORITIZATION_NAME;

			// Token: 0x0400044D RID: 1101
			private DataColumn columnPRIORITIZATION_DESCRIPTION;

			// Token: 0x0400044E RID: 1102
			private DataColumn columnPRIORITIZATION_IS_MANUAL;

			// Token: 0x0400044F RID: 1103
			private DataColumn columnPRIORITIZATION_IS_COMPLETE;

			// Token: 0x04000450 RID: 1104
			private DataColumn columnDEPARTMENT_UID;

			// Token: 0x04000451 RID: 1105
			private DataColumn columnDEPARTMENT_NAME;

			// Token: 0x04000452 RID: 1106
			private DataColumn columnRELATIVE_IMPORTANCE_CF_UID;

			// Token: 0x04000453 RID: 1107
			private DataColumn columnRELATIVE_IMPORTANCE_CF_NAME;

			// Token: 0x04000454 RID: 1108
			private DataColumn columnCONSISTENCY_RATIO;

			// Token: 0x04000455 RID: 1109
			private DataColumn columnPRIORITIZATION_IS_USED_IN_ANALYSIS;

			// Token: 0x04000456 RID: 1110
			private DataColumn columnCREATED_DATE;

			// Token: 0x04000457 RID: 1111
			private DataColumn columnMOD_DATE;

			// Token: 0x04000458 RID: 1112
			private DataColumn columnLAST_UPDATED_BY_RES_UID;

			// Token: 0x04000459 RID: 1113
			private DataColumn columnLAST_UPDATED_BY_RES_NAME;

			// Token: 0x0400045A RID: 1114
			private DataColumn columnCREATED_BY_RES_UID;

			// Token: 0x0400045B RID: 1115
			private DataColumn columnCREATED_BY_RES_NAME;
		}

		// Token: 0x02000108 RID: 264
		[XmlSchemaProvider("GetTypedTableSchema")]
		[Serializable]
		public class PrioritizationEntriesDataTable : DataTable, IEnumerable
		{
			// Token: 0x06001345 RID: 4933 RVA: 0x0003F104 File Offset: 0x0003D304
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public PrioritizationEntriesDataTable()
			{
				base.TableName = "PrioritizationEntries";
				this.BeginInit();
				this.InitClass();
				this.EndInit();
			}

			// Token: 0x06001346 RID: 4934 RVA: 0x0003F12C File Offset: 0x0003D32C
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			internal PrioritizationEntriesDataTable(DataTable table)
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

			// Token: 0x06001347 RID: 4935 RVA: 0x0003F1D4 File Offset: 0x0003D3D4
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			protected PrioritizationEntriesDataTable(SerializationInfo info, StreamingContext context) : base(info, context)
			{
				this.InitVars();
			}

			// Token: 0x170005AF RID: 1455
			// (get) Token: 0x06001348 RID: 4936 RVA: 0x0003F1E4 File Offset: 0x0003D3E4
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public DataColumn PRIORITIZATION_UIDColumn
			{
				get
				{
					return this.columnPRIORITIZATION_UID;
				}
			}

			// Token: 0x170005B0 RID: 1456
			// (get) Token: 0x06001349 RID: 4937 RVA: 0x0003F1EC File Offset: 0x0003D3EC
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public DataColumn DRIVER_UIDColumn
			{
				get
				{
					return this.columnDRIVER_UID;
				}
			}

			// Token: 0x170005B1 RID: 1457
			// (get) Token: 0x0600134A RID: 4938 RVA: 0x0003F1F4 File Offset: 0x0003D3F4
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public DataColumn DRIVER_NAMEColumn
			{
				get
				{
					return this.columnDRIVER_NAME;
				}
			}

			// Token: 0x170005B2 RID: 1458
			// (get) Token: 0x0600134B RID: 4939 RVA: 0x0003F1FC File Offset: 0x0003D3FC
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			public DataColumn DRIVER_PRIORITYColumn
			{
				get
				{
					return this.columnDRIVER_PRIORITY;
				}
			}

			// Token: 0x170005B3 RID: 1459
			// (get) Token: 0x0600134C RID: 4940 RVA: 0x0003F204 File Offset: 0x0003D404
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public DataColumn DRIVER_DESCRIPTIONColumn
			{
				get
				{
					return this.columnDRIVER_DESCRIPTION;
				}
			}

			// Token: 0x170005B4 RID: 1460
			// (get) Token: 0x0600134D RID: 4941 RVA: 0x0003F20C File Offset: 0x0003D40C
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public DataColumn DRIVER_IS_ACTIVEColumn
			{
				get
				{
					return this.columnDRIVER_IS_ACTIVE;
				}
			}

			// Token: 0x170005B5 RID: 1461
			// (get) Token: 0x0600134E RID: 4942 RVA: 0x0003F214 File Offset: 0x0003D414
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

			// Token: 0x170005B6 RID: 1462
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			public DriverPrioritizationDataSet.PrioritizationEntriesRow this[int index]
			{
				get
				{
					return (DriverPrioritizationDataSet.PrioritizationEntriesRow)base.Rows[index];
				}
			}

			// Token: 0x140000D9 RID: 217
			// (add) Token: 0x06001350 RID: 4944 RVA: 0x0003F234 File Offset: 0x0003D434
			// (remove) Token: 0x06001351 RID: 4945 RVA: 0x0003F26C File Offset: 0x0003D46C
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public event DriverPrioritizationDataSet.PrioritizationEntriesRowChangeEventHandler PrioritizationEntriesRowChanging;

			// Token: 0x140000DA RID: 218
			// (add) Token: 0x06001352 RID: 4946 RVA: 0x0003F2A4 File Offset: 0x0003D4A4
			// (remove) Token: 0x06001353 RID: 4947 RVA: 0x0003F2DC File Offset: 0x0003D4DC
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public event DriverPrioritizationDataSet.PrioritizationEntriesRowChangeEventHandler PrioritizationEntriesRowChanged;

			// Token: 0x140000DB RID: 219
			// (add) Token: 0x06001354 RID: 4948 RVA: 0x0003F314 File Offset: 0x0003D514
			// (remove) Token: 0x06001355 RID: 4949 RVA: 0x0003F34C File Offset: 0x0003D54C
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public event DriverPrioritizationDataSet.PrioritizationEntriesRowChangeEventHandler PrioritizationEntriesRowDeleting;

			// Token: 0x140000DC RID: 220
			// (add) Token: 0x06001356 RID: 4950 RVA: 0x0003F384 File Offset: 0x0003D584
			// (remove) Token: 0x06001357 RID: 4951 RVA: 0x0003F3BC File Offset: 0x0003D5BC
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public event DriverPrioritizationDataSet.PrioritizationEntriesRowChangeEventHandler PrioritizationEntriesRowDeleted;

			// Token: 0x06001358 RID: 4952 RVA: 0x0003F3F1 File Offset: 0x0003D5F1
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			public void AddPrioritizationEntriesRow(DriverPrioritizationDataSet.PrioritizationEntriesRow row)
			{
				base.Rows.Add(row);
			}

			// Token: 0x06001359 RID: 4953 RVA: 0x0003F400 File Offset: 0x0003D600
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public DriverPrioritizationDataSet.PrioritizationEntriesRow AddPrioritizationEntriesRow(DriverPrioritizationDataSet.PrioritizationRow parentPrioritizationRowByFK_Prioritization_PrioritizationEntries, Guid DRIVER_UID, string DRIVER_NAME, double DRIVER_PRIORITY, string DRIVER_DESCRIPTION, bool DRIVER_IS_ACTIVE)
			{
				DriverPrioritizationDataSet.PrioritizationEntriesRow prioritizationEntriesRow = (DriverPrioritizationDataSet.PrioritizationEntriesRow)base.NewRow();
				object[] array = new object[]
				{
					null,
					DRIVER_UID,
					DRIVER_NAME,
					DRIVER_PRIORITY,
					DRIVER_DESCRIPTION,
					DRIVER_IS_ACTIVE
				};
				if (parentPrioritizationRowByFK_Prioritization_PrioritizationEntries != null)
				{
					array[0] = parentPrioritizationRowByFK_Prioritization_PrioritizationEntries[0];
				}
				prioritizationEntriesRow.ItemArray = array;
				base.Rows.Add(prioritizationEntriesRow);
				return prioritizationEntriesRow;
			}

			// Token: 0x0600135A RID: 4954 RVA: 0x0003F46C File Offset: 0x0003D66C
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public DriverPrioritizationDataSet.PrioritizationEntriesRow FindByPRIORITIZATION_UIDDRIVER_UID(Guid PRIORITIZATION_UID, Guid DRIVER_UID)
			{
				return (DriverPrioritizationDataSet.PrioritizationEntriesRow)base.Rows.Find(new object[]
				{
					PRIORITIZATION_UID,
					DRIVER_UID
				});
			}

			// Token: 0x0600135B RID: 4955 RVA: 0x0003F4A3 File Offset: 0x0003D6A3
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			public virtual IEnumerator GetEnumerator()
			{
				return base.Rows.GetEnumerator();
			}

			// Token: 0x0600135C RID: 4956 RVA: 0x0003F4B0 File Offset: 0x0003D6B0
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			public override DataTable Clone()
			{
				DriverPrioritizationDataSet.PrioritizationEntriesDataTable prioritizationEntriesDataTable = (DriverPrioritizationDataSet.PrioritizationEntriesDataTable)base.Clone();
				prioritizationEntriesDataTable.InitVars();
				return prioritizationEntriesDataTable;
			}

			// Token: 0x0600135D RID: 4957 RVA: 0x0003F4D0 File Offset: 0x0003D6D0
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			protected override DataTable CreateInstance()
			{
				return new DriverPrioritizationDataSet.PrioritizationEntriesDataTable();
			}

			// Token: 0x0600135E RID: 4958 RVA: 0x0003F4D8 File Offset: 0x0003D6D8
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			internal void InitVars()
			{
				this.columnPRIORITIZATION_UID = base.Columns["PRIORITIZATION_UID"];
				this.columnDRIVER_UID = base.Columns["DRIVER_UID"];
				this.columnDRIVER_NAME = base.Columns["DRIVER_NAME"];
				this.columnDRIVER_PRIORITY = base.Columns["DRIVER_PRIORITY"];
				this.columnDRIVER_DESCRIPTION = base.Columns["DRIVER_DESCRIPTION"];
				this.columnDRIVER_IS_ACTIVE = base.Columns["DRIVER_IS_ACTIVE"];
			}

			// Token: 0x0600135F RID: 4959 RVA: 0x0003F56C File Offset: 0x0003D76C
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			private void InitClass()
			{
				this.columnPRIORITIZATION_UID = new DataColumn("PRIORITIZATION_UID", typeof(Guid), null, MappingType.Element);
				base.Columns.Add(this.columnPRIORITIZATION_UID);
				this.columnDRIVER_UID = new DataColumn("DRIVER_UID", typeof(Guid), null, MappingType.Element);
				base.Columns.Add(this.columnDRIVER_UID);
				this.columnDRIVER_NAME = new DataColumn("DRIVER_NAME", typeof(string), null, MappingType.Element);
				base.Columns.Add(this.columnDRIVER_NAME);
				this.columnDRIVER_PRIORITY = new DataColumn("DRIVER_PRIORITY", typeof(double), null, MappingType.Element);
				base.Columns.Add(this.columnDRIVER_PRIORITY);
				this.columnDRIVER_DESCRIPTION = new DataColumn("DRIVER_DESCRIPTION", typeof(string), null, MappingType.Element);
				base.Columns.Add(this.columnDRIVER_DESCRIPTION);
				this.columnDRIVER_IS_ACTIVE = new DataColumn("DRIVER_IS_ACTIVE", typeof(bool), null, MappingType.Element);
				base.Columns.Add(this.columnDRIVER_IS_ACTIVE);
				base.Constraints.Add(new UniqueConstraint("PrioritizationEntries_Key", new DataColumn[]
				{
					this.columnPRIORITIZATION_UID,
					this.columnDRIVER_UID
				}, true));
				this.columnPRIORITIZATION_UID.AllowDBNull = false;
				this.columnDRIVER_UID.AllowDBNull = false;
				this.columnDRIVER_NAME.ReadOnly = true;
				this.columnDRIVER_DESCRIPTION.ReadOnly = true;
				this.columnDRIVER_IS_ACTIVE.ReadOnly = true;
			}

			// Token: 0x06001360 RID: 4960 RVA: 0x0003F6F3 File Offset: 0x0003D8F3
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			public DriverPrioritizationDataSet.PrioritizationEntriesRow NewPrioritizationEntriesRow()
			{
				return (DriverPrioritizationDataSet.PrioritizationEntriesRow)base.NewRow();
			}

			// Token: 0x06001361 RID: 4961 RVA: 0x0003F700 File Offset: 0x0003D900
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			protected override DataRow NewRowFromBuilder(DataRowBuilder builder)
			{
				return new DriverPrioritizationDataSet.PrioritizationEntriesRow(builder);
			}

			// Token: 0x06001362 RID: 4962 RVA: 0x0003F708 File Offset: 0x0003D908
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			protected override Type GetRowType()
			{
				return typeof(DriverPrioritizationDataSet.PrioritizationEntriesRow);
			}

			// Token: 0x06001363 RID: 4963 RVA: 0x0003F714 File Offset: 0x0003D914
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			protected override void OnRowChanged(DataRowChangeEventArgs e)
			{
				base.OnRowChanged(e);
				if (this.PrioritizationEntriesRowChanged != null)
				{
					this.PrioritizationEntriesRowChanged(this, new DriverPrioritizationDataSet.PrioritizationEntriesRowChangeEvent((DriverPrioritizationDataSet.PrioritizationEntriesRow)e.Row, e.Action));
				}
			}

			// Token: 0x06001364 RID: 4964 RVA: 0x0003F747 File Offset: 0x0003D947
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			protected override void OnRowChanging(DataRowChangeEventArgs e)
			{
				base.OnRowChanging(e);
				if (this.PrioritizationEntriesRowChanging != null)
				{
					this.PrioritizationEntriesRowChanging(this, new DriverPrioritizationDataSet.PrioritizationEntriesRowChangeEvent((DriverPrioritizationDataSet.PrioritizationEntriesRow)e.Row, e.Action));
				}
			}

			// Token: 0x06001365 RID: 4965 RVA: 0x0003F77A File Offset: 0x0003D97A
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			protected override void OnRowDeleted(DataRowChangeEventArgs e)
			{
				base.OnRowDeleted(e);
				if (this.PrioritizationEntriesRowDeleted != null)
				{
					this.PrioritizationEntriesRowDeleted(this, new DriverPrioritizationDataSet.PrioritizationEntriesRowChangeEvent((DriverPrioritizationDataSet.PrioritizationEntriesRow)e.Row, e.Action));
				}
			}

			// Token: 0x06001366 RID: 4966 RVA: 0x0003F7AD File Offset: 0x0003D9AD
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			protected override void OnRowDeleting(DataRowChangeEventArgs e)
			{
				base.OnRowDeleting(e);
				if (this.PrioritizationEntriesRowDeleting != null)
				{
					this.PrioritizationEntriesRowDeleting(this, new DriverPrioritizationDataSet.PrioritizationEntriesRowChangeEvent((DriverPrioritizationDataSet.PrioritizationEntriesRow)e.Row, e.Action));
				}
			}

			// Token: 0x06001367 RID: 4967 RVA: 0x0003F7E0 File Offset: 0x0003D9E0
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			public void RemovePrioritizationEntriesRow(DriverPrioritizationDataSet.PrioritizationEntriesRow row)
			{
				base.Rows.Remove(row);
			}

			// Token: 0x06001368 RID: 4968 RVA: 0x0003F7F0 File Offset: 0x0003D9F0
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public static XmlSchemaComplexType GetTypedTableSchema(XmlSchemaSet xs)
			{
				XmlSchemaComplexType xmlSchemaComplexType = new XmlSchemaComplexType();
				XmlSchemaSequence xmlSchemaSequence = new XmlSchemaSequence();
				DriverPrioritizationDataSet driverPrioritizationDataSet = new DriverPrioritizationDataSet();
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
				xmlSchemaAttribute.FixedValue = driverPrioritizationDataSet.Namespace;
				xmlSchemaComplexType.Attributes.Add(xmlSchemaAttribute);
				XmlSchemaAttribute xmlSchemaAttribute2 = new XmlSchemaAttribute();
				xmlSchemaAttribute2.Name = "tableTypeName";
				xmlSchemaAttribute2.FixedValue = "PrioritizationEntriesDataTable";
				xmlSchemaComplexType.Attributes.Add(xmlSchemaAttribute2);
				xmlSchemaComplexType.Particle = xmlSchemaSequence;
				XmlSchema schemaSerializable = driverPrioritizationDataSet.GetSchemaSerializable();
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

			// Token: 0x04000460 RID: 1120
			private DataColumn columnPRIORITIZATION_UID;

			// Token: 0x04000461 RID: 1121
			private DataColumn columnDRIVER_UID;

			// Token: 0x04000462 RID: 1122
			private DataColumn columnDRIVER_NAME;

			// Token: 0x04000463 RID: 1123
			private DataColumn columnDRIVER_PRIORITY;

			// Token: 0x04000464 RID: 1124
			private DataColumn columnDRIVER_DESCRIPTION;

			// Token: 0x04000465 RID: 1125
			private DataColumn columnDRIVER_IS_ACTIVE;
		}

		// Token: 0x02000109 RID: 265
		[XmlSchemaProvider("GetTypedTableSchema")]
		[Serializable]
		public class DriverRelationsDataTable : DataTable, IEnumerable
		{
			// Token: 0x06001369 RID: 4969 RVA: 0x0003F9E8 File Offset: 0x0003DBE8
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			public DriverRelationsDataTable()
			{
				base.TableName = "DriverRelations";
				this.BeginInit();
				this.InitClass();
				this.EndInit();
			}

			// Token: 0x0600136A RID: 4970 RVA: 0x0003FA10 File Offset: 0x0003DC10
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			internal DriverRelationsDataTable(DataTable table)
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

			// Token: 0x0600136B RID: 4971 RVA: 0x0003FAB8 File Offset: 0x0003DCB8
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			protected DriverRelationsDataTable(SerializationInfo info, StreamingContext context) : base(info, context)
			{
				this.InitVars();
			}

			// Token: 0x170005B7 RID: 1463
			// (get) Token: 0x0600136C RID: 4972 RVA: 0x0003FAC8 File Offset: 0x0003DCC8
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public DataColumn PRIORITIZATION_UIDColumn
			{
				get
				{
					return this.columnPRIORITIZATION_UID;
				}
			}

			// Token: 0x170005B8 RID: 1464
			// (get) Token: 0x0600136D RID: 4973 RVA: 0x0003FAD0 File Offset: 0x0003DCD0
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			public DataColumn DRIVER1_UIDColumn
			{
				get
				{
					return this.columnDRIVER1_UID;
				}
			}

			// Token: 0x170005B9 RID: 1465
			// (get) Token: 0x0600136E RID: 4974 RVA: 0x0003FAD8 File Offset: 0x0003DCD8
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public DataColumn DRIVER2_UIDColumn
			{
				get
				{
					return this.columnDRIVER2_UID;
				}
			}

			// Token: 0x170005BA RID: 1466
			// (get) Token: 0x0600136F RID: 4975 RVA: 0x0003FAE0 File Offset: 0x0003DCE0
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public DataColumn LT_STRUCT_UIDColumn
			{
				get
				{
					return this.columnLT_STRUCT_UID;
				}
			}

			// Token: 0x170005BB RID: 1467
			// (get) Token: 0x06001370 RID: 4976 RVA: 0x0003FAE8 File Offset: 0x0003DCE8
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

			// Token: 0x170005BC RID: 1468
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public DriverPrioritizationDataSet.DriverRelationsRow this[int index]
			{
				get
				{
					return (DriverPrioritizationDataSet.DriverRelationsRow)base.Rows[index];
				}
			}

			// Token: 0x140000DD RID: 221
			// (add) Token: 0x06001372 RID: 4978 RVA: 0x0003FB08 File Offset: 0x0003DD08
			// (remove) Token: 0x06001373 RID: 4979 RVA: 0x0003FB40 File Offset: 0x0003DD40
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public event DriverPrioritizationDataSet.DriverRelationsRowChangeEventHandler DriverRelationsRowChanging;

			// Token: 0x140000DE RID: 222
			// (add) Token: 0x06001374 RID: 4980 RVA: 0x0003FB78 File Offset: 0x0003DD78
			// (remove) Token: 0x06001375 RID: 4981 RVA: 0x0003FBB0 File Offset: 0x0003DDB0
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public event DriverPrioritizationDataSet.DriverRelationsRowChangeEventHandler DriverRelationsRowChanged;

			// Token: 0x140000DF RID: 223
			// (add) Token: 0x06001376 RID: 4982 RVA: 0x0003FBE8 File Offset: 0x0003DDE8
			// (remove) Token: 0x06001377 RID: 4983 RVA: 0x0003FC20 File Offset: 0x0003DE20
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public event DriverPrioritizationDataSet.DriverRelationsRowChangeEventHandler DriverRelationsRowDeleting;

			// Token: 0x140000E0 RID: 224
			// (add) Token: 0x06001378 RID: 4984 RVA: 0x0003FC58 File Offset: 0x0003DE58
			// (remove) Token: 0x06001379 RID: 4985 RVA: 0x0003FC90 File Offset: 0x0003DE90
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public event DriverPrioritizationDataSet.DriverRelationsRowChangeEventHandler DriverRelationsRowDeleted;

			// Token: 0x0600137A RID: 4986 RVA: 0x0003FCC5 File Offset: 0x0003DEC5
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			public void AddDriverRelationsRow(DriverPrioritizationDataSet.DriverRelationsRow row)
			{
				base.Rows.Add(row);
			}

			// Token: 0x0600137B RID: 4987 RVA: 0x0003FCD4 File Offset: 0x0003DED4
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			public DriverPrioritizationDataSet.DriverRelationsRow AddDriverRelationsRow(Guid PRIORITIZATION_UID, Guid DRIVER1_UID, Guid DRIVER2_UID, Guid LT_STRUCT_UID)
			{
				DriverPrioritizationDataSet.DriverRelationsRow driverRelationsRow = (DriverPrioritizationDataSet.DriverRelationsRow)base.NewRow();
				object[] itemArray = new object[]
				{
					PRIORITIZATION_UID,
					DRIVER1_UID,
					DRIVER2_UID,
					LT_STRUCT_UID
				};
				driverRelationsRow.ItemArray = itemArray;
				base.Rows.Add(driverRelationsRow);
				return driverRelationsRow;
			}

			// Token: 0x0600137C RID: 4988 RVA: 0x0003FD30 File Offset: 0x0003DF30
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			public DriverPrioritizationDataSet.DriverRelationsRow FindByPRIORITIZATION_UIDDRIVER1_UIDDRIVER2_UID(Guid PRIORITIZATION_UID, Guid DRIVER1_UID, Guid DRIVER2_UID)
			{
				return (DriverPrioritizationDataSet.DriverRelationsRow)base.Rows.Find(new object[]
				{
					PRIORITIZATION_UID,
					DRIVER1_UID,
					DRIVER2_UID
				});
			}

			// Token: 0x0600137D RID: 4989 RVA: 0x0003FD70 File Offset: 0x0003DF70
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			public virtual IEnumerator GetEnumerator()
			{
				return base.Rows.GetEnumerator();
			}

			// Token: 0x0600137E RID: 4990 RVA: 0x0003FD80 File Offset: 0x0003DF80
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			public override DataTable Clone()
			{
				DriverPrioritizationDataSet.DriverRelationsDataTable driverRelationsDataTable = (DriverPrioritizationDataSet.DriverRelationsDataTable)base.Clone();
				driverRelationsDataTable.InitVars();
				return driverRelationsDataTable;
			}

			// Token: 0x0600137F RID: 4991 RVA: 0x0003FDA0 File Offset: 0x0003DFA0
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			protected override DataTable CreateInstance()
			{
				return new DriverPrioritizationDataSet.DriverRelationsDataTable();
			}

			// Token: 0x06001380 RID: 4992 RVA: 0x0003FDA8 File Offset: 0x0003DFA8
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			internal void InitVars()
			{
				this.columnPRIORITIZATION_UID = base.Columns["PRIORITIZATION_UID"];
				this.columnDRIVER1_UID = base.Columns["DRIVER1_UID"];
				this.columnDRIVER2_UID = base.Columns["DRIVER2_UID"];
				this.columnLT_STRUCT_UID = base.Columns["LT_STRUCT_UID"];
			}

			// Token: 0x06001381 RID: 4993 RVA: 0x0003FE10 File Offset: 0x0003E010
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			private void InitClass()
			{
				this.columnPRIORITIZATION_UID = new DataColumn("PRIORITIZATION_UID", typeof(Guid), null, MappingType.Element);
				base.Columns.Add(this.columnPRIORITIZATION_UID);
				this.columnDRIVER1_UID = new DataColumn("DRIVER1_UID", typeof(Guid), null, MappingType.Element);
				base.Columns.Add(this.columnDRIVER1_UID);
				this.columnDRIVER2_UID = new DataColumn("DRIVER2_UID", typeof(Guid), null, MappingType.Element);
				base.Columns.Add(this.columnDRIVER2_UID);
				this.columnLT_STRUCT_UID = new DataColumn("LT_STRUCT_UID", typeof(Guid), null, MappingType.Element);
				base.Columns.Add(this.columnLT_STRUCT_UID);
				base.Constraints.Add(new UniqueConstraint("DriverRelations_Key", new DataColumn[]
				{
					this.columnPRIORITIZATION_UID,
					this.columnDRIVER1_UID,
					this.columnDRIVER2_UID
				}, true));
				this.columnPRIORITIZATION_UID.AllowDBNull = false;
				this.columnDRIVER1_UID.AllowDBNull = false;
				this.columnDRIVER2_UID.AllowDBNull = false;
			}

			// Token: 0x06001382 RID: 4994 RVA: 0x0003FF2E File Offset: 0x0003E12E
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			public DriverPrioritizationDataSet.DriverRelationsRow NewDriverRelationsRow()
			{
				return (DriverPrioritizationDataSet.DriverRelationsRow)base.NewRow();
			}

			// Token: 0x06001383 RID: 4995 RVA: 0x0003FF3B File Offset: 0x0003E13B
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			protected override DataRow NewRowFromBuilder(DataRowBuilder builder)
			{
				return new DriverPrioritizationDataSet.DriverRelationsRow(builder);
			}

			// Token: 0x06001384 RID: 4996 RVA: 0x0003FF43 File Offset: 0x0003E143
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			protected override Type GetRowType()
			{
				return typeof(DriverPrioritizationDataSet.DriverRelationsRow);
			}

			// Token: 0x06001385 RID: 4997 RVA: 0x0003FF4F File Offset: 0x0003E14F
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			protected override void OnRowChanged(DataRowChangeEventArgs e)
			{
				base.OnRowChanged(e);
				if (this.DriverRelationsRowChanged != null)
				{
					this.DriverRelationsRowChanged(this, new DriverPrioritizationDataSet.DriverRelationsRowChangeEvent((DriverPrioritizationDataSet.DriverRelationsRow)e.Row, e.Action));
				}
			}

			// Token: 0x06001386 RID: 4998 RVA: 0x0003FF82 File Offset: 0x0003E182
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			protected override void OnRowChanging(DataRowChangeEventArgs e)
			{
				base.OnRowChanging(e);
				if (this.DriverRelationsRowChanging != null)
				{
					this.DriverRelationsRowChanging(this, new DriverPrioritizationDataSet.DriverRelationsRowChangeEvent((DriverPrioritizationDataSet.DriverRelationsRow)e.Row, e.Action));
				}
			}

			// Token: 0x06001387 RID: 4999 RVA: 0x0003FFB5 File Offset: 0x0003E1B5
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			protected override void OnRowDeleted(DataRowChangeEventArgs e)
			{
				base.OnRowDeleted(e);
				if (this.DriverRelationsRowDeleted != null)
				{
					this.DriverRelationsRowDeleted(this, new DriverPrioritizationDataSet.DriverRelationsRowChangeEvent((DriverPrioritizationDataSet.DriverRelationsRow)e.Row, e.Action));
				}
			}

			// Token: 0x06001388 RID: 5000 RVA: 0x0003FFE8 File Offset: 0x0003E1E8
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			protected override void OnRowDeleting(DataRowChangeEventArgs e)
			{
				base.OnRowDeleting(e);
				if (this.DriverRelationsRowDeleting != null)
				{
					this.DriverRelationsRowDeleting(this, new DriverPrioritizationDataSet.DriverRelationsRowChangeEvent((DriverPrioritizationDataSet.DriverRelationsRow)e.Row, e.Action));
				}
			}

			// Token: 0x06001389 RID: 5001 RVA: 0x0004001B File Offset: 0x0003E21B
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			public void RemoveDriverRelationsRow(DriverPrioritizationDataSet.DriverRelationsRow row)
			{
				base.Rows.Remove(row);
			}

			// Token: 0x0600138A RID: 5002 RVA: 0x0004002C File Offset: 0x0003E22C
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			public static XmlSchemaComplexType GetTypedTableSchema(XmlSchemaSet xs)
			{
				XmlSchemaComplexType xmlSchemaComplexType = new XmlSchemaComplexType();
				XmlSchemaSequence xmlSchemaSequence = new XmlSchemaSequence();
				DriverPrioritizationDataSet driverPrioritizationDataSet = new DriverPrioritizationDataSet();
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
				xmlSchemaAttribute.FixedValue = driverPrioritizationDataSet.Namespace;
				xmlSchemaComplexType.Attributes.Add(xmlSchemaAttribute);
				XmlSchemaAttribute xmlSchemaAttribute2 = new XmlSchemaAttribute();
				xmlSchemaAttribute2.Name = "tableTypeName";
				xmlSchemaAttribute2.FixedValue = "DriverRelationsDataTable";
				xmlSchemaComplexType.Attributes.Add(xmlSchemaAttribute2);
				xmlSchemaComplexType.Particle = xmlSchemaSequence;
				XmlSchema schemaSerializable = driverPrioritizationDataSet.GetSchemaSerializable();
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

			// Token: 0x0400046A RID: 1130
			private DataColumn columnPRIORITIZATION_UID;

			// Token: 0x0400046B RID: 1131
			private DataColumn columnDRIVER1_UID;

			// Token: 0x0400046C RID: 1132
			private DataColumn columnDRIVER2_UID;

			// Token: 0x0400046D RID: 1133
			private DataColumn columnLT_STRUCT_UID;
		}

		// Token: 0x0200010A RID: 266
		public class PrioritizationRow : DataRow
		{
			// Token: 0x0600138B RID: 5003 RVA: 0x00040224 File Offset: 0x0003E424
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			internal PrioritizationRow(DataRowBuilder rb) : base(rb)
			{
				this.tablePrioritization = (DriverPrioritizationDataSet.PrioritizationDataTable)base.Table;
			}

			// Token: 0x170005BD RID: 1469
			// (get) Token: 0x0600138C RID: 5004 RVA: 0x0004023E File Offset: 0x0003E43E
			// (set) Token: 0x0600138D RID: 5005 RVA: 0x00040256 File Offset: 0x0003E456
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public Guid PRIORITIZATION_UID
			{
				get
				{
					return (Guid)base[this.tablePrioritization.PRIORITIZATION_UIDColumn];
				}
				set
				{
					base[this.tablePrioritization.PRIORITIZATION_UIDColumn] = value;
				}
			}

			// Token: 0x170005BE RID: 1470
			// (get) Token: 0x0600138E RID: 5006 RVA: 0x0004026F File Offset: 0x0003E46F
			// (set) Token: 0x0600138F RID: 5007 RVA: 0x00040287 File Offset: 0x0003E487
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			public string PRIORITIZATION_NAME
			{
				get
				{
					return (string)base[this.tablePrioritization.PRIORITIZATION_NAMEColumn];
				}
				set
				{
					base[this.tablePrioritization.PRIORITIZATION_NAMEColumn] = value;
				}
			}

			// Token: 0x170005BF RID: 1471
			// (get) Token: 0x06001390 RID: 5008 RVA: 0x0004029C File Offset: 0x0003E49C
			// (set) Token: 0x06001391 RID: 5009 RVA: 0x000402E0 File Offset: 0x0003E4E0
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			public string PRIORITIZATION_DESCRIPTION
			{
				get
				{
					string result;
					try
					{
						result = (string)base[this.tablePrioritization.PRIORITIZATION_DESCRIPTIONColumn];
					}
					catch (InvalidCastException innerException)
					{
						throw new StrongTypingException("The value for column 'PRIORITIZATION_DESCRIPTION' in table 'Prioritization' is DBNull.", innerException);
					}
					return result;
				}
				set
				{
					base[this.tablePrioritization.PRIORITIZATION_DESCRIPTIONColumn] = value;
				}
			}

			// Token: 0x170005C0 RID: 1472
			// (get) Token: 0x06001392 RID: 5010 RVA: 0x000402F4 File Offset: 0x0003E4F4
			// (set) Token: 0x06001393 RID: 5011 RVA: 0x0004030C File Offset: 0x0003E50C
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			public bool PRIORITIZATION_IS_MANUAL
			{
				get
				{
					return (bool)base[this.tablePrioritization.PRIORITIZATION_IS_MANUALColumn];
				}
				set
				{
					base[this.tablePrioritization.PRIORITIZATION_IS_MANUALColumn] = value;
				}
			}

			// Token: 0x170005C1 RID: 1473
			// (get) Token: 0x06001394 RID: 5012 RVA: 0x00040328 File Offset: 0x0003E528
			// (set) Token: 0x06001395 RID: 5013 RVA: 0x0004036C File Offset: 0x0003E56C
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			public bool PRIORITIZATION_IS_COMPLETE
			{
				get
				{
					bool result;
					try
					{
						result = (bool)base[this.tablePrioritization.PRIORITIZATION_IS_COMPLETEColumn];
					}
					catch (InvalidCastException innerException)
					{
						throw new StrongTypingException("The value for column 'PRIORITIZATION_IS_COMPLETE' in table 'Prioritization' is DBNull.", innerException);
					}
					return result;
				}
				set
				{
					base[this.tablePrioritization.PRIORITIZATION_IS_COMPLETEColumn] = value;
				}
			}

			// Token: 0x170005C2 RID: 1474
			// (get) Token: 0x06001396 RID: 5014 RVA: 0x00040388 File Offset: 0x0003E588
			// (set) Token: 0x06001397 RID: 5015 RVA: 0x000403CC File Offset: 0x0003E5CC
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			public Guid DEPARTMENT_UID
			{
				get
				{
					Guid result;
					try
					{
						result = (Guid)base[this.tablePrioritization.DEPARTMENT_UIDColumn];
					}
					catch (InvalidCastException innerException)
					{
						throw new StrongTypingException("The value for column 'DEPARTMENT_UID' in table 'Prioritization' is DBNull.", innerException);
					}
					return result;
				}
				set
				{
					base[this.tablePrioritization.DEPARTMENT_UIDColumn] = value;
				}
			}

			// Token: 0x170005C3 RID: 1475
			// (get) Token: 0x06001398 RID: 5016 RVA: 0x000403E8 File Offset: 0x0003E5E8
			// (set) Token: 0x06001399 RID: 5017 RVA: 0x0004042C File Offset: 0x0003E62C
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public string DEPARTMENT_NAME
			{
				get
				{
					string result;
					try
					{
						result = (string)base[this.tablePrioritization.DEPARTMENT_NAMEColumn];
					}
					catch (InvalidCastException innerException)
					{
						throw new StrongTypingException("The value for column 'DEPARTMENT_NAME' in table 'Prioritization' is DBNull.", innerException);
					}
					return result;
				}
				set
				{
					base[this.tablePrioritization.DEPARTMENT_NAMEColumn] = value;
				}
			}

			// Token: 0x170005C4 RID: 1476
			// (get) Token: 0x0600139A RID: 5018 RVA: 0x00040440 File Offset: 0x0003E640
			// (set) Token: 0x0600139B RID: 5019 RVA: 0x00040458 File Offset: 0x0003E658
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			public Guid RELATIVE_IMPORTANCE_CF_UID
			{
				get
				{
					return (Guid)base[this.tablePrioritization.RELATIVE_IMPORTANCE_CF_UIDColumn];
				}
				set
				{
					base[this.tablePrioritization.RELATIVE_IMPORTANCE_CF_UIDColumn] = value;
				}
			}

			// Token: 0x170005C5 RID: 1477
			// (get) Token: 0x0600139C RID: 5020 RVA: 0x00040474 File Offset: 0x0003E674
			// (set) Token: 0x0600139D RID: 5021 RVA: 0x000404B8 File Offset: 0x0003E6B8
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public string RELATIVE_IMPORTANCE_CF_NAME
			{
				get
				{
					string result;
					try
					{
						result = (string)base[this.tablePrioritization.RELATIVE_IMPORTANCE_CF_NAMEColumn];
					}
					catch (InvalidCastException innerException)
					{
						throw new StrongTypingException("The value for column 'RELATIVE_IMPORTANCE_CF_NAME' in table 'Prioritization' is DBNull.", innerException);
					}
					return result;
				}
				set
				{
					base[this.tablePrioritization.RELATIVE_IMPORTANCE_CF_NAMEColumn] = value;
				}
			}

			// Token: 0x170005C6 RID: 1478
			// (get) Token: 0x0600139E RID: 5022 RVA: 0x000404CC File Offset: 0x0003E6CC
			// (set) Token: 0x0600139F RID: 5023 RVA: 0x000404E4 File Offset: 0x0003E6E4
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public double CONSISTENCY_RATIO
			{
				get
				{
					return (double)base[this.tablePrioritization.CONSISTENCY_RATIOColumn];
				}
				set
				{
					base[this.tablePrioritization.CONSISTENCY_RATIOColumn] = value;
				}
			}

			// Token: 0x170005C7 RID: 1479
			// (get) Token: 0x060013A0 RID: 5024 RVA: 0x00040500 File Offset: 0x0003E700
			// (set) Token: 0x060013A1 RID: 5025 RVA: 0x00040544 File Offset: 0x0003E744
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			public bool PRIORITIZATION_IS_USED_IN_ANALYSIS
			{
				get
				{
					bool result;
					try
					{
						result = (bool)base[this.tablePrioritization.PRIORITIZATION_IS_USED_IN_ANALYSISColumn];
					}
					catch (InvalidCastException innerException)
					{
						throw new StrongTypingException("The value for column 'PRIORITIZATION_IS_USED_IN_ANALYSIS' in table 'Prioritization' is DBNull.", innerException);
					}
					return result;
				}
				set
				{
					base[this.tablePrioritization.PRIORITIZATION_IS_USED_IN_ANALYSISColumn] = value;
				}
			}

			// Token: 0x170005C8 RID: 1480
			// (get) Token: 0x060013A2 RID: 5026 RVA: 0x00040560 File Offset: 0x0003E760
			// (set) Token: 0x060013A3 RID: 5027 RVA: 0x000405A4 File Offset: 0x0003E7A4
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			public DateTime CREATED_DATE
			{
				get
				{
					DateTime result;
					try
					{
						result = (DateTime)base[this.tablePrioritization.CREATED_DATEColumn];
					}
					catch (InvalidCastException innerException)
					{
						throw new StrongTypingException("The value for column 'CREATED_DATE' in table 'Prioritization' is DBNull.", innerException);
					}
					return result;
				}
				set
				{
					base[this.tablePrioritization.CREATED_DATEColumn] = value;
				}
			}

			// Token: 0x170005C9 RID: 1481
			// (get) Token: 0x060013A4 RID: 5028 RVA: 0x000405C0 File Offset: 0x0003E7C0
			// (set) Token: 0x060013A5 RID: 5029 RVA: 0x00040604 File Offset: 0x0003E804
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public DateTime MOD_DATE
			{
				get
				{
					DateTime result;
					try
					{
						result = (DateTime)base[this.tablePrioritization.MOD_DATEColumn];
					}
					catch (InvalidCastException innerException)
					{
						throw new StrongTypingException("The value for column 'MOD_DATE' in table 'Prioritization' is DBNull.", innerException);
					}
					return result;
				}
				set
				{
					base[this.tablePrioritization.MOD_DATEColumn] = value;
				}
			}

			// Token: 0x170005CA RID: 1482
			// (get) Token: 0x060013A6 RID: 5030 RVA: 0x00040620 File Offset: 0x0003E820
			// (set) Token: 0x060013A7 RID: 5031 RVA: 0x00040664 File Offset: 0x0003E864
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			public Guid LAST_UPDATED_BY_RES_UID
			{
				get
				{
					Guid result;
					try
					{
						result = (Guid)base[this.tablePrioritization.LAST_UPDATED_BY_RES_UIDColumn];
					}
					catch (InvalidCastException innerException)
					{
						throw new StrongTypingException("The value for column 'LAST_UPDATED_BY_RES_UID' in table 'Prioritization' is DBNull.", innerException);
					}
					return result;
				}
				set
				{
					base[this.tablePrioritization.LAST_UPDATED_BY_RES_UIDColumn] = value;
				}
			}

			// Token: 0x170005CB RID: 1483
			// (get) Token: 0x060013A8 RID: 5032 RVA: 0x00040680 File Offset: 0x0003E880
			// (set) Token: 0x060013A9 RID: 5033 RVA: 0x000406C4 File Offset: 0x0003E8C4
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			public string LAST_UPDATED_BY_RES_NAME
			{
				get
				{
					string result;
					try
					{
						result = (string)base[this.tablePrioritization.LAST_UPDATED_BY_RES_NAMEColumn];
					}
					catch (InvalidCastException innerException)
					{
						throw new StrongTypingException("The value for column 'LAST_UPDATED_BY_RES_NAME' in table 'Prioritization' is DBNull.", innerException);
					}
					return result;
				}
				set
				{
					base[this.tablePrioritization.LAST_UPDATED_BY_RES_NAMEColumn] = value;
				}
			}

			// Token: 0x170005CC RID: 1484
			// (get) Token: 0x060013AA RID: 5034 RVA: 0x000406D8 File Offset: 0x0003E8D8
			// (set) Token: 0x060013AB RID: 5035 RVA: 0x0004071C File Offset: 0x0003E91C
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			public Guid CREATED_BY_RES_UID
			{
				get
				{
					Guid result;
					try
					{
						result = (Guid)base[this.tablePrioritization.CREATED_BY_RES_UIDColumn];
					}
					catch (InvalidCastException innerException)
					{
						throw new StrongTypingException("The value for column 'CREATED_BY_RES_UID' in table 'Prioritization' is DBNull.", innerException);
					}
					return result;
				}
				set
				{
					base[this.tablePrioritization.CREATED_BY_RES_UIDColumn] = value;
				}
			}

			// Token: 0x170005CD RID: 1485
			// (get) Token: 0x060013AC RID: 5036 RVA: 0x00040738 File Offset: 0x0003E938
			// (set) Token: 0x060013AD RID: 5037 RVA: 0x0004077C File Offset: 0x0003E97C
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public string CREATED_BY_RES_NAME
			{
				get
				{
					string result;
					try
					{
						result = (string)base[this.tablePrioritization.CREATED_BY_RES_NAMEColumn];
					}
					catch (InvalidCastException innerException)
					{
						throw new StrongTypingException("The value for column 'CREATED_BY_RES_NAME' in table 'Prioritization' is DBNull.", innerException);
					}
					return result;
				}
				set
				{
					base[this.tablePrioritization.CREATED_BY_RES_NAMEColumn] = value;
				}
			}

			// Token: 0x060013AE RID: 5038 RVA: 0x00040790 File Offset: 0x0003E990
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public bool IsPRIORITIZATION_DESCRIPTIONNull()
			{
				return base.IsNull(this.tablePrioritization.PRIORITIZATION_DESCRIPTIONColumn);
			}

			// Token: 0x060013AF RID: 5039 RVA: 0x000407A3 File Offset: 0x0003E9A3
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public void SetPRIORITIZATION_DESCRIPTIONNull()
			{
				base[this.tablePrioritization.PRIORITIZATION_DESCRIPTIONColumn] = Convert.DBNull;
			}

			// Token: 0x060013B0 RID: 5040 RVA: 0x000407BB File Offset: 0x0003E9BB
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public bool IsPRIORITIZATION_IS_COMPLETENull()
			{
				return base.IsNull(this.tablePrioritization.PRIORITIZATION_IS_COMPLETEColumn);
			}

			// Token: 0x060013B1 RID: 5041 RVA: 0x000407CE File Offset: 0x0003E9CE
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			public void SetPRIORITIZATION_IS_COMPLETENull()
			{
				base[this.tablePrioritization.PRIORITIZATION_IS_COMPLETEColumn] = Convert.DBNull;
			}

			// Token: 0x060013B2 RID: 5042 RVA: 0x000407E6 File Offset: 0x0003E9E6
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			public bool IsDEPARTMENT_UIDNull()
			{
				return base.IsNull(this.tablePrioritization.DEPARTMENT_UIDColumn);
			}

			// Token: 0x060013B3 RID: 5043 RVA: 0x000407F9 File Offset: 0x0003E9F9
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			public void SetDEPARTMENT_UIDNull()
			{
				base[this.tablePrioritization.DEPARTMENT_UIDColumn] = Convert.DBNull;
			}

			// Token: 0x060013B4 RID: 5044 RVA: 0x00040811 File Offset: 0x0003EA11
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			public bool IsDEPARTMENT_NAMENull()
			{
				return base.IsNull(this.tablePrioritization.DEPARTMENT_NAMEColumn);
			}

			// Token: 0x060013B5 RID: 5045 RVA: 0x00040824 File Offset: 0x0003EA24
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			public void SetDEPARTMENT_NAMENull()
			{
				base[this.tablePrioritization.DEPARTMENT_NAMEColumn] = Convert.DBNull;
			}

			// Token: 0x060013B6 RID: 5046 RVA: 0x0004083C File Offset: 0x0003EA3C
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public bool IsRELATIVE_IMPORTANCE_CF_NAMENull()
			{
				return base.IsNull(this.tablePrioritization.RELATIVE_IMPORTANCE_CF_NAMEColumn);
			}

			// Token: 0x060013B7 RID: 5047 RVA: 0x0004084F File Offset: 0x0003EA4F
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			public void SetRELATIVE_IMPORTANCE_CF_NAMENull()
			{
				base[this.tablePrioritization.RELATIVE_IMPORTANCE_CF_NAMEColumn] = Convert.DBNull;
			}

			// Token: 0x060013B8 RID: 5048 RVA: 0x00040867 File Offset: 0x0003EA67
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			public bool IsPRIORITIZATION_IS_USED_IN_ANALYSISNull()
			{
				return base.IsNull(this.tablePrioritization.PRIORITIZATION_IS_USED_IN_ANALYSISColumn);
			}

			// Token: 0x060013B9 RID: 5049 RVA: 0x0004087A File Offset: 0x0003EA7A
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			public void SetPRIORITIZATION_IS_USED_IN_ANALYSISNull()
			{
				base[this.tablePrioritization.PRIORITIZATION_IS_USED_IN_ANALYSISColumn] = Convert.DBNull;
			}

			// Token: 0x060013BA RID: 5050 RVA: 0x00040892 File Offset: 0x0003EA92
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			public bool IsCREATED_DATENull()
			{
				return base.IsNull(this.tablePrioritization.CREATED_DATEColumn);
			}

			// Token: 0x060013BB RID: 5051 RVA: 0x000408A5 File Offset: 0x0003EAA5
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public void SetCREATED_DATENull()
			{
				base[this.tablePrioritization.CREATED_DATEColumn] = Convert.DBNull;
			}

			// Token: 0x060013BC RID: 5052 RVA: 0x000408BD File Offset: 0x0003EABD
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public bool IsMOD_DATENull()
			{
				return base.IsNull(this.tablePrioritization.MOD_DATEColumn);
			}

			// Token: 0x060013BD RID: 5053 RVA: 0x000408D0 File Offset: 0x0003EAD0
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public void SetMOD_DATENull()
			{
				base[this.tablePrioritization.MOD_DATEColumn] = Convert.DBNull;
			}

			// Token: 0x060013BE RID: 5054 RVA: 0x000408E8 File Offset: 0x0003EAE8
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			public bool IsLAST_UPDATED_BY_RES_UIDNull()
			{
				return base.IsNull(this.tablePrioritization.LAST_UPDATED_BY_RES_UIDColumn);
			}

			// Token: 0x060013BF RID: 5055 RVA: 0x000408FB File Offset: 0x0003EAFB
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			public void SetLAST_UPDATED_BY_RES_UIDNull()
			{
				base[this.tablePrioritization.LAST_UPDATED_BY_RES_UIDColumn] = Convert.DBNull;
			}

			// Token: 0x060013C0 RID: 5056 RVA: 0x00040913 File Offset: 0x0003EB13
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			public bool IsLAST_UPDATED_BY_RES_NAMENull()
			{
				return base.IsNull(this.tablePrioritization.LAST_UPDATED_BY_RES_NAMEColumn);
			}

			// Token: 0x060013C1 RID: 5057 RVA: 0x00040926 File Offset: 0x0003EB26
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			public void SetLAST_UPDATED_BY_RES_NAMENull()
			{
				base[this.tablePrioritization.LAST_UPDATED_BY_RES_NAMEColumn] = Convert.DBNull;
			}

			// Token: 0x060013C2 RID: 5058 RVA: 0x0004093E File Offset: 0x0003EB3E
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			public bool IsCREATED_BY_RES_UIDNull()
			{
				return base.IsNull(this.tablePrioritization.CREATED_BY_RES_UIDColumn);
			}

			// Token: 0x060013C3 RID: 5059 RVA: 0x00040951 File Offset: 0x0003EB51
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public void SetCREATED_BY_RES_UIDNull()
			{
				base[this.tablePrioritization.CREATED_BY_RES_UIDColumn] = Convert.DBNull;
			}

			// Token: 0x060013C4 RID: 5060 RVA: 0x00040969 File Offset: 0x0003EB69
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			public bool IsCREATED_BY_RES_NAMENull()
			{
				return base.IsNull(this.tablePrioritization.CREATED_BY_RES_NAMEColumn);
			}

			// Token: 0x060013C5 RID: 5061 RVA: 0x0004097C File Offset: 0x0003EB7C
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			public void SetCREATED_BY_RES_NAMENull()
			{
				base[this.tablePrioritization.CREATED_BY_RES_NAMEColumn] = Convert.DBNull;
			}

			// Token: 0x060013C6 RID: 5062 RVA: 0x00040994 File Offset: 0x0003EB94
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			public DriverPrioritizationDataSet.PrioritizationEntriesRow[] GetPrioritizationEntriesRows()
			{
				if (base.Table.ChildRelations["FK_Prioritization_PrioritizationEntries"] == null)
				{
					return new DriverPrioritizationDataSet.PrioritizationEntriesRow[0];
				}
				return (DriverPrioritizationDataSet.PrioritizationEntriesRow[])base.GetChildRows(base.Table.ChildRelations["FK_Prioritization_PrioritizationEntries"]);
			}

			// Token: 0x04000472 RID: 1138
			private DriverPrioritizationDataSet.PrioritizationDataTable tablePrioritization;
		}

		// Token: 0x0200010B RID: 267
		public class PrioritizationEntriesRow : DataRow
		{
			// Token: 0x060013C7 RID: 5063 RVA: 0x000409D4 File Offset: 0x0003EBD4
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			internal PrioritizationEntriesRow(DataRowBuilder rb) : base(rb)
			{
				this.tablePrioritizationEntries = (DriverPrioritizationDataSet.PrioritizationEntriesDataTable)base.Table;
			}

			// Token: 0x170005CE RID: 1486
			// (get) Token: 0x060013C8 RID: 5064 RVA: 0x000409EE File Offset: 0x0003EBEE
			// (set) Token: 0x060013C9 RID: 5065 RVA: 0x00040A06 File Offset: 0x0003EC06
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public Guid PRIORITIZATION_UID
			{
				get
				{
					return (Guid)base[this.tablePrioritizationEntries.PRIORITIZATION_UIDColumn];
				}
				set
				{
					base[this.tablePrioritizationEntries.PRIORITIZATION_UIDColumn] = value;
				}
			}

			// Token: 0x170005CF RID: 1487
			// (get) Token: 0x060013CA RID: 5066 RVA: 0x00040A1F File Offset: 0x0003EC1F
			// (set) Token: 0x060013CB RID: 5067 RVA: 0x00040A37 File Offset: 0x0003EC37
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			public Guid DRIVER_UID
			{
				get
				{
					return (Guid)base[this.tablePrioritizationEntries.DRIVER_UIDColumn];
				}
				set
				{
					base[this.tablePrioritizationEntries.DRIVER_UIDColumn] = value;
				}
			}

			// Token: 0x170005D0 RID: 1488
			// (get) Token: 0x060013CC RID: 5068 RVA: 0x00040A50 File Offset: 0x0003EC50
			// (set) Token: 0x060013CD RID: 5069 RVA: 0x00040A94 File Offset: 0x0003EC94
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public string DRIVER_NAME
			{
				get
				{
					string result;
					try
					{
						result = (string)base[this.tablePrioritizationEntries.DRIVER_NAMEColumn];
					}
					catch (InvalidCastException innerException)
					{
						throw new StrongTypingException("The value for column 'DRIVER_NAME' in table 'PrioritizationEntries' is DBNull.", innerException);
					}
					return result;
				}
				set
				{
					base[this.tablePrioritizationEntries.DRIVER_NAMEColumn] = value;
				}
			}

			// Token: 0x170005D1 RID: 1489
			// (get) Token: 0x060013CE RID: 5070 RVA: 0x00040AA8 File Offset: 0x0003ECA8
			// (set) Token: 0x060013CF RID: 5071 RVA: 0x00040AEC File Offset: 0x0003ECEC
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public double DRIVER_PRIORITY
			{
				get
				{
					double result;
					try
					{
						result = (double)base[this.tablePrioritizationEntries.DRIVER_PRIORITYColumn];
					}
					catch (InvalidCastException innerException)
					{
						throw new StrongTypingException("The value for column 'DRIVER_PRIORITY' in table 'PrioritizationEntries' is DBNull.", innerException);
					}
					return result;
				}
				set
				{
					base[this.tablePrioritizationEntries.DRIVER_PRIORITYColumn] = value;
				}
			}

			// Token: 0x170005D2 RID: 1490
			// (get) Token: 0x060013D0 RID: 5072 RVA: 0x00040B08 File Offset: 0x0003ED08
			// (set) Token: 0x060013D1 RID: 5073 RVA: 0x00040B4C File Offset: 0x0003ED4C
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public string DRIVER_DESCRIPTION
			{
				get
				{
					string result;
					try
					{
						result = (string)base[this.tablePrioritizationEntries.DRIVER_DESCRIPTIONColumn];
					}
					catch (InvalidCastException innerException)
					{
						throw new StrongTypingException("The value for column 'DRIVER_DESCRIPTION' in table 'PrioritizationEntries' is DBNull.", innerException);
					}
					return result;
				}
				set
				{
					base[this.tablePrioritizationEntries.DRIVER_DESCRIPTIONColumn] = value;
				}
			}

			// Token: 0x170005D3 RID: 1491
			// (get) Token: 0x060013D2 RID: 5074 RVA: 0x00040B60 File Offset: 0x0003ED60
			// (set) Token: 0x060013D3 RID: 5075 RVA: 0x00040BA4 File Offset: 0x0003EDA4
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			public bool DRIVER_IS_ACTIVE
			{
				get
				{
					bool result;
					try
					{
						result = (bool)base[this.tablePrioritizationEntries.DRIVER_IS_ACTIVEColumn];
					}
					catch (InvalidCastException innerException)
					{
						throw new StrongTypingException("The value for column 'DRIVER_IS_ACTIVE' in table 'PrioritizationEntries' is DBNull.", innerException);
					}
					return result;
				}
				set
				{
					base[this.tablePrioritizationEntries.DRIVER_IS_ACTIVEColumn] = value;
				}
			}

			// Token: 0x170005D4 RID: 1492
			// (get) Token: 0x060013D4 RID: 5076 RVA: 0x00040BBD File Offset: 0x0003EDBD
			// (set) Token: 0x060013D5 RID: 5077 RVA: 0x00040BDF File Offset: 0x0003EDDF
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			public DriverPrioritizationDataSet.PrioritizationRow PrioritizationRow
			{
				get
				{
					return (DriverPrioritizationDataSet.PrioritizationRow)base.GetParentRow(base.Table.ParentRelations["FK_Prioritization_PrioritizationEntries"]);
				}
				set
				{
					base.SetParentRow(value, base.Table.ParentRelations["FK_Prioritization_PrioritizationEntries"]);
				}
			}

			// Token: 0x060013D6 RID: 5078 RVA: 0x00040BFD File Offset: 0x0003EDFD
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public bool IsDRIVER_NAMENull()
			{
				return base.IsNull(this.tablePrioritizationEntries.DRIVER_NAMEColumn);
			}

			// Token: 0x060013D7 RID: 5079 RVA: 0x00040C10 File Offset: 0x0003EE10
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			public void SetDRIVER_NAMENull()
			{
				base[this.tablePrioritizationEntries.DRIVER_NAMEColumn] = Convert.DBNull;
			}

			// Token: 0x060013D8 RID: 5080 RVA: 0x00040C28 File Offset: 0x0003EE28
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			public bool IsDRIVER_PRIORITYNull()
			{
				return base.IsNull(this.tablePrioritizationEntries.DRIVER_PRIORITYColumn);
			}

			// Token: 0x060013D9 RID: 5081 RVA: 0x00040C3B File Offset: 0x0003EE3B
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public void SetDRIVER_PRIORITYNull()
			{
				base[this.tablePrioritizationEntries.DRIVER_PRIORITYColumn] = Convert.DBNull;
			}

			// Token: 0x060013DA RID: 5082 RVA: 0x00040C53 File Offset: 0x0003EE53
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			public bool IsDRIVER_DESCRIPTIONNull()
			{
				return base.IsNull(this.tablePrioritizationEntries.DRIVER_DESCRIPTIONColumn);
			}

			// Token: 0x060013DB RID: 5083 RVA: 0x00040C66 File Offset: 0x0003EE66
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			public void SetDRIVER_DESCRIPTIONNull()
			{
				base[this.tablePrioritizationEntries.DRIVER_DESCRIPTIONColumn] = Convert.DBNull;
			}

			// Token: 0x060013DC RID: 5084 RVA: 0x00040C7E File Offset: 0x0003EE7E
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			public bool IsDRIVER_IS_ACTIVENull()
			{
				return base.IsNull(this.tablePrioritizationEntries.DRIVER_IS_ACTIVEColumn);
			}

			// Token: 0x060013DD RID: 5085 RVA: 0x00040C91 File Offset: 0x0003EE91
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			public void SetDRIVER_IS_ACTIVENull()
			{
				base[this.tablePrioritizationEntries.DRIVER_IS_ACTIVEColumn] = Convert.DBNull;
			}

			// Token: 0x060013DE RID: 5086 RVA: 0x00040CA9 File Offset: 0x0003EEA9
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			public DriverPrioritizationDataSet.DriverRelationsRow[] GetDriverRelationsRowsByFK_PrioritizationEntries_DriverRelations1()
			{
				if (base.Table.ChildRelations["FK_PrioritizationEntries_DriverRelations1"] == null)
				{
					return new DriverPrioritizationDataSet.DriverRelationsRow[0];
				}
				return (DriverPrioritizationDataSet.DriverRelationsRow[])base.GetChildRows(base.Table.ChildRelations["FK_PrioritizationEntries_DriverRelations1"]);
			}

			// Token: 0x060013DF RID: 5087 RVA: 0x00040CE9 File Offset: 0x0003EEE9
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			public DriverPrioritizationDataSet.DriverRelationsRow[] GetDriverRelationsRowsByFK_PrioritizationEntries_DriverRelations2()
			{
				if (base.Table.ChildRelations["FK_PrioritizationEntries_DriverRelations2"] == null)
				{
					return new DriverPrioritizationDataSet.DriverRelationsRow[0];
				}
				return (DriverPrioritizationDataSet.DriverRelationsRow[])base.GetChildRows(base.Table.ChildRelations["FK_PrioritizationEntries_DriverRelations2"]);
			}

			// Token: 0x04000473 RID: 1139
			private DriverPrioritizationDataSet.PrioritizationEntriesDataTable tablePrioritizationEntries;
		}

		// Token: 0x0200010C RID: 268
		public class DriverRelationsRow : DataRow
		{
			// Token: 0x060013E0 RID: 5088 RVA: 0x00040D29 File Offset: 0x0003EF29
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			internal DriverRelationsRow(DataRowBuilder rb) : base(rb)
			{
				this.tableDriverRelations = (DriverPrioritizationDataSet.DriverRelationsDataTable)base.Table;
			}

			// Token: 0x170005D5 RID: 1493
			// (get) Token: 0x060013E1 RID: 5089 RVA: 0x00040D43 File Offset: 0x0003EF43
			// (set) Token: 0x060013E2 RID: 5090 RVA: 0x00040D5B File Offset: 0x0003EF5B
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public Guid PRIORITIZATION_UID
			{
				get
				{
					return (Guid)base[this.tableDriverRelations.PRIORITIZATION_UIDColumn];
				}
				set
				{
					base[this.tableDriverRelations.PRIORITIZATION_UIDColumn] = value;
				}
			}

			// Token: 0x170005D6 RID: 1494
			// (get) Token: 0x060013E3 RID: 5091 RVA: 0x00040D74 File Offset: 0x0003EF74
			// (set) Token: 0x060013E4 RID: 5092 RVA: 0x00040D8C File Offset: 0x0003EF8C
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public Guid DRIVER1_UID
			{
				get
				{
					return (Guid)base[this.tableDriverRelations.DRIVER1_UIDColumn];
				}
				set
				{
					base[this.tableDriverRelations.DRIVER1_UIDColumn] = value;
				}
			}

			// Token: 0x170005D7 RID: 1495
			// (get) Token: 0x060013E5 RID: 5093 RVA: 0x00040DA5 File Offset: 0x0003EFA5
			// (set) Token: 0x060013E6 RID: 5094 RVA: 0x00040DBD File Offset: 0x0003EFBD
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			public Guid DRIVER2_UID
			{
				get
				{
					return (Guid)base[this.tableDriverRelations.DRIVER2_UIDColumn];
				}
				set
				{
					base[this.tableDriverRelations.DRIVER2_UIDColumn] = value;
				}
			}

			// Token: 0x170005D8 RID: 1496
			// (get) Token: 0x060013E7 RID: 5095 RVA: 0x00040DD8 File Offset: 0x0003EFD8
			// (set) Token: 0x060013E8 RID: 5096 RVA: 0x00040E1C File Offset: 0x0003F01C
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			public Guid LT_STRUCT_UID
			{
				get
				{
					Guid result;
					try
					{
						result = (Guid)base[this.tableDriverRelations.LT_STRUCT_UIDColumn];
					}
					catch (InvalidCastException innerException)
					{
						throw new StrongTypingException("The value for column 'LT_STRUCT_UID' in table 'DriverRelations' is DBNull.", innerException);
					}
					return result;
				}
				set
				{
					base[this.tableDriverRelations.LT_STRUCT_UIDColumn] = value;
				}
			}

			// Token: 0x170005D9 RID: 1497
			// (get) Token: 0x060013E9 RID: 5097 RVA: 0x00040E35 File Offset: 0x0003F035
			// (set) Token: 0x060013EA RID: 5098 RVA: 0x00040E57 File Offset: 0x0003F057
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public DriverPrioritizationDataSet.PrioritizationEntriesRow PrioritizationEntriesRowParentByFK_PrioritizationEntries_DriverRelations1
			{
				get
				{
					return (DriverPrioritizationDataSet.PrioritizationEntriesRow)base.GetParentRow(base.Table.ParentRelations["FK_PrioritizationEntries_DriverRelations1"]);
				}
				set
				{
					base.SetParentRow(value, base.Table.ParentRelations["FK_PrioritizationEntries_DriverRelations1"]);
				}
			}

			// Token: 0x170005DA RID: 1498
			// (get) Token: 0x060013EB RID: 5099 RVA: 0x00040E75 File Offset: 0x0003F075
			// (set) Token: 0x060013EC RID: 5100 RVA: 0x00040E97 File Offset: 0x0003F097
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			public DriverPrioritizationDataSet.PrioritizationEntriesRow PrioritizationEntriesRowParentByFK_PrioritizationEntries_DriverRelations2
			{
				get
				{
					return (DriverPrioritizationDataSet.PrioritizationEntriesRow)base.GetParentRow(base.Table.ParentRelations["FK_PrioritizationEntries_DriverRelations2"]);
				}
				set
				{
					base.SetParentRow(value, base.Table.ParentRelations["FK_PrioritizationEntries_DriverRelations2"]);
				}
			}

			// Token: 0x060013ED RID: 5101 RVA: 0x00040EB5 File Offset: 0x0003F0B5
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public bool IsLT_STRUCT_UIDNull()
			{
				return base.IsNull(this.tableDriverRelations.LT_STRUCT_UIDColumn);
			}

			// Token: 0x060013EE RID: 5102 RVA: 0x00040EC8 File Offset: 0x0003F0C8
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public void SetLT_STRUCT_UIDNull()
			{
				base[this.tableDriverRelations.LT_STRUCT_UIDColumn] = Convert.DBNull;
			}

			// Token: 0x04000474 RID: 1140
			private DriverPrioritizationDataSet.DriverRelationsDataTable tableDriverRelations;
		}

		// Token: 0x0200010D RID: 269
		[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
		public class PrioritizationRowChangeEvent : EventArgs
		{
			// Token: 0x060013EF RID: 5103 RVA: 0x00040EE0 File Offset: 0x0003F0E0
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			public PrioritizationRowChangeEvent(DriverPrioritizationDataSet.PrioritizationRow row, DataRowAction action)
			{
				this.eventRow = row;
				this.eventAction = action;
			}

			// Token: 0x170005DB RID: 1499
			// (get) Token: 0x060013F0 RID: 5104 RVA: 0x00040EF6 File Offset: 0x0003F0F6
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			public DriverPrioritizationDataSet.PrioritizationRow Row
			{
				get
				{
					return this.eventRow;
				}
			}

			// Token: 0x170005DC RID: 1500
			// (get) Token: 0x060013F1 RID: 5105 RVA: 0x00040EFE File Offset: 0x0003F0FE
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public DataRowAction Action
			{
				get
				{
					return this.eventAction;
				}
			}

			// Token: 0x04000475 RID: 1141
			private DriverPrioritizationDataSet.PrioritizationRow eventRow;

			// Token: 0x04000476 RID: 1142
			private DataRowAction eventAction;
		}

		// Token: 0x0200010E RID: 270
		[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
		public class PrioritizationEntriesRowChangeEvent : EventArgs
		{
			// Token: 0x060013F2 RID: 5106 RVA: 0x00040F06 File Offset: 0x0003F106
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			public PrioritizationEntriesRowChangeEvent(DriverPrioritizationDataSet.PrioritizationEntriesRow row, DataRowAction action)
			{
				this.eventRow = row;
				this.eventAction = action;
			}

			// Token: 0x170005DD RID: 1501
			// (get) Token: 0x060013F3 RID: 5107 RVA: 0x00040F1C File Offset: 0x0003F11C
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public DriverPrioritizationDataSet.PrioritizationEntriesRow Row
			{
				get
				{
					return this.eventRow;
				}
			}

			// Token: 0x170005DE RID: 1502
			// (get) Token: 0x060013F4 RID: 5108 RVA: 0x00040F24 File Offset: 0x0003F124
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public DataRowAction Action
			{
				get
				{
					return this.eventAction;
				}
			}

			// Token: 0x04000477 RID: 1143
			private DriverPrioritizationDataSet.PrioritizationEntriesRow eventRow;

			// Token: 0x04000478 RID: 1144
			private DataRowAction eventAction;
		}

		// Token: 0x0200010F RID: 271
		[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
		public class DriverRelationsRowChangeEvent : EventArgs
		{
			// Token: 0x060013F5 RID: 5109 RVA: 0x00040F2C File Offset: 0x0003F12C
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			public DriverRelationsRowChangeEvent(DriverPrioritizationDataSet.DriverRelationsRow row, DataRowAction action)
			{
				this.eventRow = row;
				this.eventAction = action;
			}

			// Token: 0x170005DF RID: 1503
			// (get) Token: 0x060013F6 RID: 5110 RVA: 0x00040F42 File Offset: 0x0003F142
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public DriverPrioritizationDataSet.DriverRelationsRow Row
			{
				get
				{
					return this.eventRow;
				}
			}

			// Token: 0x170005E0 RID: 1504
			// (get) Token: 0x060013F7 RID: 5111 RVA: 0x00040F4A File Offset: 0x0003F14A
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			public DataRowAction Action
			{
				get
				{
					return this.eventAction;
				}
			}

			// Token: 0x04000479 RID: 1145
			private DriverPrioritizationDataSet.DriverRelationsRow eventRow;

			// Token: 0x0400047A RID: 1146
			private DataRowAction eventAction;
		}
	}
}
