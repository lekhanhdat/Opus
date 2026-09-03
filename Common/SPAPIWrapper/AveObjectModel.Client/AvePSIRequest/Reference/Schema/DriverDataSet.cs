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
	// Token: 0x020000F6 RID: 246
	[XmlSchemaProvider("GetTypedDataSetSchema")]
	[HelpKeyword("vs.data.DataSet")]
	[DesignerCategory("code")]
	[ToolboxItem(true)]
	[XmlRoot("DriverDataSet")]
	[Serializable]
	public class DriverDataSet : DataSet
	{
		// Token: 0x0600120C RID: 4620 RVA: 0x0003AB30 File Offset: 0x00038D30
		public new void EndInit()
		{
			base.EndInit();
			TypedDataSetUtilities.AllowNullsInNonTypedColumns(this.DriverImpactStatements, new string[]
			{
				"DRIVER_UID",
				"PROJECT_IMPACT_CF_NAME",
				"PROJECT_IMPACT_CF_UID",
				"DESCRIPTION",
				"LT_STRUCT_UID"
			});
			TypedDataSetUtilities.AllowNullsInNonTypedColumns(this.DriverDepartments, new string[]
			{
				"DRIVER_UID",
				"DEPARTMENT_NAME",
				"DEPARTMENT_UID"
			});
			TypedDataSetUtilities.AllowNullsInNonTypedColumns(this.Driver, new string[]
			{
				"DRIVER_IS_USED_IN_PRIORITIZATION",
				"DRIVER_DESCRIPTION",
				"LAST_UPDATED_BY_RES_NAME",
				"CREATED_DATE",
				"DRIVER_IS_USED_IN_ANALYSIS",
				"DRIVER_NAME",
				"CREATED_BY_RES_UID",
				"LAST_UPDATED_BY_RES_UID",
				"DRIVER_UID",
				"MOD_DATE",
				"DRIVER_IS_ACTIVE",
				"CREATED_BY_RES_NAME"
			});
		}

		// Token: 0x0600120D RID: 4621 RVA: 0x0003AC20 File Offset: 0x00038E20
		[DebuggerNonUserCode]
		[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
		public DriverDataSet()
		{
			base.BeginInit();
			this.InitClass();
			CollectionChangeEventHandler value = new CollectionChangeEventHandler(this.SchemaChanged);
			base.Tables.CollectionChanged += value;
			base.Relations.CollectionChanged += value;
			this.EndInit();
		}

		// Token: 0x0600120E RID: 4622 RVA: 0x0003AC74 File Offset: 0x00038E74
		[DebuggerNonUserCode]
		[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
		protected DriverDataSet(SerializationInfo info, StreamingContext context) : base(info, context, false)
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
				if (dataSet.Tables["Driver"] != null)
				{
					base.Tables.Add(new DriverDataSet.DriverDataTable(dataSet.Tables["Driver"]));
				}
				if (dataSet.Tables["DriverImpactStatements"] != null)
				{
					base.Tables.Add(new DriverDataSet.DriverImpactStatementsDataTable(dataSet.Tables["DriverImpactStatements"]));
				}
				if (dataSet.Tables["DriverDepartments"] != null)
				{
					base.Tables.Add(new DriverDataSet.DriverDepartmentsDataTable(dataSet.Tables["DriverDepartments"]));
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

		// Token: 0x1700055A RID: 1370
		// (get) Token: 0x0600120F RID: 4623 RVA: 0x0003AE35 File Offset: 0x00039035
		[Browsable(false)]
		[DebuggerNonUserCode]
		[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Content)]
		public DriverDataSet.DriverDataTable Driver
		{
			get
			{
				return this.tableDriver;
			}
		}

		// Token: 0x1700055B RID: 1371
		// (get) Token: 0x06001210 RID: 4624 RVA: 0x0003AE3D File Offset: 0x0003903D
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Content)]
		[DebuggerNonUserCode]
		[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
		[Browsable(false)]
		public DriverDataSet.DriverImpactStatementsDataTable DriverImpactStatements
		{
			get
			{
				return this.tableDriverImpactStatements;
			}
		}

		// Token: 0x1700055C RID: 1372
		// (get) Token: 0x06001211 RID: 4625 RVA: 0x0003AE45 File Offset: 0x00039045
		[Browsable(false)]
		[DebuggerNonUserCode]
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Content)]
		[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
		public DriverDataSet.DriverDepartmentsDataTable DriverDepartments
		{
			get
			{
				return this.tableDriverDepartments;
			}
		}

		// Token: 0x1700055D RID: 1373
		// (get) Token: 0x06001212 RID: 4626 RVA: 0x0003AE4D File Offset: 0x0003904D
		// (set) Token: 0x06001213 RID: 4627 RVA: 0x0003AE55 File Offset: 0x00039055
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Visible)]
		[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
		[DebuggerNonUserCode]
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

		// Token: 0x1700055E RID: 1374
		// (get) Token: 0x06001214 RID: 4628 RVA: 0x0003AE5E File Offset: 0x0003905E
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

		// Token: 0x1700055F RID: 1375
		// (get) Token: 0x06001215 RID: 4629 RVA: 0x0003AE66 File Offset: 0x00039066
		[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
		[DebuggerNonUserCode]
		public new DataRelationCollection Relations
		{
			get
			{
				return base.Relations;
			}
		}

		// Token: 0x06001216 RID: 4630 RVA: 0x0003AE6E File Offset: 0x0003906E
		[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
		[DebuggerNonUserCode]
		protected override void InitializeDerivedDataSet()
		{
			base.BeginInit();
			this.InitClass();
			this.EndInit();
		}

		// Token: 0x06001217 RID: 4631 RVA: 0x0003AE84 File Offset: 0x00039084
		[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
		[DebuggerNonUserCode]
		public override DataSet Clone()
		{
			DriverDataSet driverDataSet = (DriverDataSet)base.Clone();
			driverDataSet.InitVars();
			driverDataSet.SchemaSerializationMode = this.SchemaSerializationMode;
			return driverDataSet;
		}

		// Token: 0x06001218 RID: 4632 RVA: 0x0003AEB0 File Offset: 0x000390B0
		[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
		[DebuggerNonUserCode]
		protected override bool ShouldSerializeTables()
		{
			return false;
		}

		// Token: 0x06001219 RID: 4633 RVA: 0x0003AEB3 File Offset: 0x000390B3
		[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
		[DebuggerNonUserCode]
		protected override bool ShouldSerializeRelations()
		{
			return false;
		}

		// Token: 0x0600121A RID: 4634 RVA: 0x0003AEB8 File Offset: 0x000390B8
		[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
		[DebuggerNonUserCode]
		protected override void ReadXmlSerializable(XmlReader reader)
		{
			if (base.DetermineSchemaSerializationMode(reader) == SchemaSerializationMode.IncludeSchema)
			{
				this.Reset();
				DataSet dataSet = new DataSet();
				dataSet.ReadXml(reader);
				if (dataSet.Tables["Driver"] != null)
				{
					base.Tables.Add(new DriverDataSet.DriverDataTable(dataSet.Tables["Driver"]));
				}
				if (dataSet.Tables["DriverImpactStatements"] != null)
				{
					base.Tables.Add(new DriverDataSet.DriverImpactStatementsDataTable(dataSet.Tables["DriverImpactStatements"]));
				}
				if (dataSet.Tables["DriverDepartments"] != null)
				{
					base.Tables.Add(new DriverDataSet.DriverDepartmentsDataTable(dataSet.Tables["DriverDepartments"]));
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

		// Token: 0x0600121B RID: 4635 RVA: 0x0003AFE4 File Offset: 0x000391E4
		[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
		[DebuggerNonUserCode]
		protected override XmlSchema GetSchemaSerializable()
		{
			MemoryStream memoryStream = new MemoryStream();
			base.WriteXmlSchema(new XmlTextWriter(memoryStream, null));
			memoryStream.Position = 0L;
			return XmlSchema.Read(new XmlTextReader(memoryStream), null);
		}

		// Token: 0x0600121C RID: 4636 RVA: 0x0003B018 File Offset: 0x00039218
		[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
		[DebuggerNonUserCode]
		internal void InitVars()
		{
			this.InitVars(true);
		}

		// Token: 0x0600121D RID: 4637 RVA: 0x0003B024 File Offset: 0x00039224
		[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
		[DebuggerNonUserCode]
		internal void InitVars(bool initTable)
		{
			this.tableDriver = (DriverDataSet.DriverDataTable)base.Tables["Driver"];
			if (initTable && this.tableDriver != null)
			{
				this.tableDriver.InitVars();
			}
			this.tableDriverImpactStatements = (DriverDataSet.DriverImpactStatementsDataTable)base.Tables["DriverImpactStatements"];
			if (initTable && this.tableDriverImpactStatements != null)
			{
				this.tableDriverImpactStatements.InitVars();
			}
			this.tableDriverDepartments = (DriverDataSet.DriverDepartmentsDataTable)base.Tables["DriverDepartments"];
			if (initTable && this.tableDriverDepartments != null)
			{
				this.tableDriverDepartments.InitVars();
			}
			this.relationFK_Driver_DriverImpactStatements = this.Relations["FK_Driver_DriverImpactStatements"];
			this.relationFK_Driver_DriverDepartments = this.Relations["FK_Driver_DriverDepartments"];
		}

		// Token: 0x0600121E RID: 4638 RVA: 0x0003B0F0 File Offset: 0x000392F0
		[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
		[DebuggerNonUserCode]
		private void InitClass()
		{
			base.DataSetName = "DriverDataSet";
			base.Prefix = "";
			base.Namespace = "http://schemas.microsoft.com/office/project/server/webservices/DriverDataSet/";
			base.EnforceConstraints = true;
			this.SchemaSerializationMode = SchemaSerializationMode.IncludeSchema;
			this.tableDriver = new DriverDataSet.DriverDataTable();
			base.Tables.Add(this.tableDriver);
			this.tableDriverImpactStatements = new DriverDataSet.DriverImpactStatementsDataTable();
			base.Tables.Add(this.tableDriverImpactStatements);
			this.tableDriverDepartments = new DriverDataSet.DriverDepartmentsDataTable();
			base.Tables.Add(this.tableDriverDepartments);
			ForeignKeyConstraint foreignKeyConstraint = new ForeignKeyConstraint("FK_Driver_DriverImpactStatements", new DataColumn[]
			{
				this.tableDriver.DRIVER_UIDColumn
			}, new DataColumn[]
			{
				this.tableDriverImpactStatements.DRIVER_UIDColumn
			});
			this.tableDriverImpactStatements.Constraints.Add(foreignKeyConstraint);
			foreignKeyConstraint.AcceptRejectRule = AcceptRejectRule.None;
			foreignKeyConstraint.DeleteRule = Rule.None;
			foreignKeyConstraint.UpdateRule = Rule.None;
			foreignKeyConstraint = new ForeignKeyConstraint("FK_Driver_DriverDepartments", new DataColumn[]
			{
				this.tableDriver.DRIVER_UIDColumn
			}, new DataColumn[]
			{
				this.tableDriverDepartments.DRIVER_UIDColumn
			});
			this.tableDriverDepartments.Constraints.Add(foreignKeyConstraint);
			foreignKeyConstraint.AcceptRejectRule = AcceptRejectRule.None;
			foreignKeyConstraint.DeleteRule = Rule.None;
			foreignKeyConstraint.UpdateRule = Rule.None;
			this.relationFK_Driver_DriverImpactStatements = new DataRelation("FK_Driver_DriverImpactStatements", new DataColumn[]
			{
				this.tableDriver.DRIVER_UIDColumn
			}, new DataColumn[]
			{
				this.tableDriverImpactStatements.DRIVER_UIDColumn
			}, false);
			this.Relations.Add(this.relationFK_Driver_DriverImpactStatements);
			this.relationFK_Driver_DriverDepartments = new DataRelation("FK_Driver_DriverDepartments", new DataColumn[]
			{
				this.tableDriver.DRIVER_UIDColumn
			}, new DataColumn[]
			{
				this.tableDriverDepartments.DRIVER_UIDColumn
			}, false);
			this.Relations.Add(this.relationFK_Driver_DriverDepartments);
		}

		// Token: 0x0600121F RID: 4639 RVA: 0x0003B2E5 File Offset: 0x000394E5
		[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
		[DebuggerNonUserCode]
		private bool ShouldSerializeDriver()
		{
			return false;
		}

		// Token: 0x06001220 RID: 4640 RVA: 0x0003B2E8 File Offset: 0x000394E8
		[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
		[DebuggerNonUserCode]
		private bool ShouldSerializeDriverImpactStatements()
		{
			return false;
		}

		// Token: 0x06001221 RID: 4641 RVA: 0x0003B2EB File Offset: 0x000394EB
		[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
		[DebuggerNonUserCode]
		private bool ShouldSerializeDriverDepartments()
		{
			return false;
		}

		// Token: 0x06001222 RID: 4642 RVA: 0x0003B2EE File Offset: 0x000394EE
		[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
		[DebuggerNonUserCode]
		private void SchemaChanged(object sender, CollectionChangeEventArgs e)
		{
			if (e.Action == CollectionChangeAction.Remove)
			{
				this.InitVars();
			}
		}

		// Token: 0x06001223 RID: 4643 RVA: 0x0003B300 File Offset: 0x00039500
		[DebuggerNonUserCode]
		[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
		public static XmlSchemaComplexType GetTypedDataSetSchema(XmlSchemaSet xs)
		{
			DriverDataSet driverDataSet = new DriverDataSet();
			XmlSchemaComplexType xmlSchemaComplexType = new XmlSchemaComplexType();
			XmlSchemaSequence xmlSchemaSequence = new XmlSchemaSequence();
			XmlSchemaAny xmlSchemaAny = new XmlSchemaAny();
			xmlSchemaAny.Namespace = driverDataSet.Namespace;
			xmlSchemaSequence.Items.Add(xmlSchemaAny);
			xmlSchemaComplexType.Particle = xmlSchemaSequence;
			XmlSchema schemaSerializable = driverDataSet.GetSchemaSerializable();
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

		// Token: 0x04000415 RID: 1045
		private DriverDataSet.DriverDataTable tableDriver;

		// Token: 0x04000416 RID: 1046
		private DriverDataSet.DriverImpactStatementsDataTable tableDriverImpactStatements;

		// Token: 0x04000417 RID: 1047
		private DriverDataSet.DriverDepartmentsDataTable tableDriverDepartments;

		// Token: 0x04000418 RID: 1048
		private DataRelation relationFK_Driver_DriverImpactStatements;

		// Token: 0x04000419 RID: 1049
		private DataRelation relationFK_Driver_DriverDepartments;

		// Token: 0x0400041A RID: 1050
		private SchemaSerializationMode _schemaSerializationMode = SchemaSerializationMode.IncludeSchema;

		// Token: 0x020000F7 RID: 247
		// (Invoke) Token: 0x06001225 RID: 4645
		[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
		public delegate void DriverRowChangeEventHandler(object sender, DriverDataSet.DriverRowChangeEvent e);

		// Token: 0x020000F8 RID: 248
		// (Invoke) Token: 0x06001229 RID: 4649
		[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
		public delegate void DriverImpactStatementsRowChangeEventHandler(object sender, DriverDataSet.DriverImpactStatementsRowChangeEvent e);

		// Token: 0x020000F9 RID: 249
		// (Invoke) Token: 0x0600122D RID: 4653
		[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
		public delegate void DriverDepartmentsRowChangeEventHandler(object sender, DriverDataSet.DriverDepartmentsRowChangeEvent e);

		// Token: 0x020000FA RID: 250
		[XmlSchemaProvider("GetTypedTableSchema")]
		[Serializable]
		public class DriverDataTable : DataTable, IEnumerable
		{
			// Token: 0x06001230 RID: 4656 RVA: 0x0003B448 File Offset: 0x00039648
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			public DriverDataTable()
			{
				base.TableName = "Driver";
				this.BeginInit();
				this.InitClass();
				this.EndInit();
			}

			// Token: 0x06001231 RID: 4657 RVA: 0x0003B470 File Offset: 0x00039670
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			internal DriverDataTable(DataTable table)
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

			// Token: 0x06001232 RID: 4658 RVA: 0x0003B518 File Offset: 0x00039718
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			protected DriverDataTable(SerializationInfo info, StreamingContext context) : base(info, context)
			{
				this.InitVars();
			}

			// Token: 0x17000560 RID: 1376
			// (get) Token: 0x06001233 RID: 4659 RVA: 0x0003B528 File Offset: 0x00039728
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			public DataColumn DRIVER_UIDColumn
			{
				get
				{
					return this.columnDRIVER_UID;
				}
			}

			// Token: 0x17000561 RID: 1377
			// (get) Token: 0x06001234 RID: 4660 RVA: 0x0003B530 File Offset: 0x00039730
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public DataColumn DRIVER_NAMEColumn
			{
				get
				{
					return this.columnDRIVER_NAME;
				}
			}

			// Token: 0x17000562 RID: 1378
			// (get) Token: 0x06001235 RID: 4661 RVA: 0x0003B538 File Offset: 0x00039738
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			public DataColumn DRIVER_DESCRIPTIONColumn
			{
				get
				{
					return this.columnDRIVER_DESCRIPTION;
				}
			}

			// Token: 0x17000563 RID: 1379
			// (get) Token: 0x06001236 RID: 4662 RVA: 0x0003B540 File Offset: 0x00039740
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public DataColumn DRIVER_IS_ACTIVEColumn
			{
				get
				{
					return this.columnDRIVER_IS_ACTIVE;
				}
			}

			// Token: 0x17000564 RID: 1380
			// (get) Token: 0x06001237 RID: 4663 RVA: 0x0003B548 File Offset: 0x00039748
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			public DataColumn DRIVER_IS_USED_IN_PRIORITIZATIONColumn
			{
				get
				{
					return this.columnDRIVER_IS_USED_IN_PRIORITIZATION;
				}
			}

			// Token: 0x17000565 RID: 1381
			// (get) Token: 0x06001238 RID: 4664 RVA: 0x0003B550 File Offset: 0x00039750
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public DataColumn DRIVER_IS_USED_IN_ANALYSISColumn
			{
				get
				{
					return this.columnDRIVER_IS_USED_IN_ANALYSIS;
				}
			}

			// Token: 0x17000566 RID: 1382
			// (get) Token: 0x06001239 RID: 4665 RVA: 0x0003B558 File Offset: 0x00039758
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			public DataColumn CREATED_DATEColumn
			{
				get
				{
					return this.columnCREATED_DATE;
				}
			}

			// Token: 0x17000567 RID: 1383
			// (get) Token: 0x0600123A RID: 4666 RVA: 0x0003B560 File Offset: 0x00039760
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public DataColumn MOD_DATEColumn
			{
				get
				{
					return this.columnMOD_DATE;
				}
			}

			// Token: 0x17000568 RID: 1384
			// (get) Token: 0x0600123B RID: 4667 RVA: 0x0003B568 File Offset: 0x00039768
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public DataColumn LAST_UPDATED_BY_RES_UIDColumn
			{
				get
				{
					return this.columnLAST_UPDATED_BY_RES_UID;
				}
			}

			// Token: 0x17000569 RID: 1385
			// (get) Token: 0x0600123C RID: 4668 RVA: 0x0003B570 File Offset: 0x00039770
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			public DataColumn LAST_UPDATED_BY_RES_NAMEColumn
			{
				get
				{
					return this.columnLAST_UPDATED_BY_RES_NAME;
				}
			}

			// Token: 0x1700056A RID: 1386
			// (get) Token: 0x0600123D RID: 4669 RVA: 0x0003B578 File Offset: 0x00039778
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public DataColumn CREATED_BY_RES_UIDColumn
			{
				get
				{
					return this.columnCREATED_BY_RES_UID;
				}
			}

			// Token: 0x1700056B RID: 1387
			// (get) Token: 0x0600123E RID: 4670 RVA: 0x0003B580 File Offset: 0x00039780
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public DataColumn CREATED_BY_RES_NAMEColumn
			{
				get
				{
					return this.columnCREATED_BY_RES_NAME;
				}
			}

			// Token: 0x1700056C RID: 1388
			// (get) Token: 0x0600123F RID: 4671 RVA: 0x0003B588 File Offset: 0x00039788
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

			// Token: 0x1700056D RID: 1389
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public DriverDataSet.DriverRow this[int index]
			{
				get
				{
					return (DriverDataSet.DriverRow)base.Rows[index];
				}
			}

			// Token: 0x140000C9 RID: 201
			// (add) Token: 0x06001241 RID: 4673 RVA: 0x0003B5A8 File Offset: 0x000397A8
			// (remove) Token: 0x06001242 RID: 4674 RVA: 0x0003B5E0 File Offset: 0x000397E0
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public event DriverDataSet.DriverRowChangeEventHandler DriverRowChanging;

			// Token: 0x140000CA RID: 202
			// (add) Token: 0x06001243 RID: 4675 RVA: 0x0003B618 File Offset: 0x00039818
			// (remove) Token: 0x06001244 RID: 4676 RVA: 0x0003B650 File Offset: 0x00039850
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public event DriverDataSet.DriverRowChangeEventHandler DriverRowChanged;

			// Token: 0x140000CB RID: 203
			// (add) Token: 0x06001245 RID: 4677 RVA: 0x0003B688 File Offset: 0x00039888
			// (remove) Token: 0x06001246 RID: 4678 RVA: 0x0003B6C0 File Offset: 0x000398C0
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public event DriverDataSet.DriverRowChangeEventHandler DriverRowDeleting;

			// Token: 0x140000CC RID: 204
			// (add) Token: 0x06001247 RID: 4679 RVA: 0x0003B6F8 File Offset: 0x000398F8
			// (remove) Token: 0x06001248 RID: 4680 RVA: 0x0003B730 File Offset: 0x00039930
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public event DriverDataSet.DriverRowChangeEventHandler DriverRowDeleted;

			// Token: 0x06001249 RID: 4681 RVA: 0x0003B765 File Offset: 0x00039965
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			public void AddDriverRow(DriverDataSet.DriverRow row)
			{
				base.Rows.Add(row);
			}

			// Token: 0x0600124A RID: 4682 RVA: 0x0003B774 File Offset: 0x00039974
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			public DriverDataSet.DriverRow AddDriverRow(Guid DRIVER_UID, string DRIVER_NAME, string DRIVER_DESCRIPTION, bool DRIVER_IS_ACTIVE, bool DRIVER_IS_USED_IN_PRIORITIZATION, bool DRIVER_IS_USED_IN_ANALYSIS, DateTime CREATED_DATE, DateTime MOD_DATE, Guid LAST_UPDATED_BY_RES_UID, string LAST_UPDATED_BY_RES_NAME, Guid CREATED_BY_RES_UID, string CREATED_BY_RES_NAME)
			{
				DriverDataSet.DriverRow driverRow = (DriverDataSet.DriverRow)base.NewRow();
				object[] itemArray = new object[]
				{
					DRIVER_UID,
					DRIVER_NAME,
					DRIVER_DESCRIPTION,
					DRIVER_IS_ACTIVE,
					DRIVER_IS_USED_IN_PRIORITIZATION,
					DRIVER_IS_USED_IN_ANALYSIS,
					CREATED_DATE,
					MOD_DATE,
					LAST_UPDATED_BY_RES_UID,
					LAST_UPDATED_BY_RES_NAME,
					CREATED_BY_RES_UID,
					CREATED_BY_RES_NAME
				};
				driverRow.ItemArray = itemArray;
				base.Rows.Add(driverRow);
				return driverRow;
			}

			// Token: 0x0600124B RID: 4683 RVA: 0x0003B810 File Offset: 0x00039A10
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			public DriverDataSet.DriverRow FindByDRIVER_UID(Guid DRIVER_UID)
			{
				return (DriverDataSet.DriverRow)base.Rows.Find(new object[]
				{
					DRIVER_UID
				});
			}

			// Token: 0x0600124C RID: 4684 RVA: 0x0003B83E File Offset: 0x00039A3E
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			public virtual IEnumerator GetEnumerator()
			{
				return base.Rows.GetEnumerator();
			}

			// Token: 0x0600124D RID: 4685 RVA: 0x0003B84C File Offset: 0x00039A4C
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			public override DataTable Clone()
			{
				DriverDataSet.DriverDataTable driverDataTable = (DriverDataSet.DriverDataTable)base.Clone();
				driverDataTable.InitVars();
				return driverDataTable;
			}

			// Token: 0x0600124E RID: 4686 RVA: 0x0003B86C File Offset: 0x00039A6C
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			protected override DataTable CreateInstance()
			{
				return new DriverDataSet.DriverDataTable();
			}

			// Token: 0x0600124F RID: 4687 RVA: 0x0003B874 File Offset: 0x00039A74
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			internal void InitVars()
			{
				this.columnDRIVER_UID = base.Columns["DRIVER_UID"];
				this.columnDRIVER_NAME = base.Columns["DRIVER_NAME"];
				this.columnDRIVER_DESCRIPTION = base.Columns["DRIVER_DESCRIPTION"];
				this.columnDRIVER_IS_ACTIVE = base.Columns["DRIVER_IS_ACTIVE"];
				this.columnDRIVER_IS_USED_IN_PRIORITIZATION = base.Columns["DRIVER_IS_USED_IN_PRIORITIZATION"];
				this.columnDRIVER_IS_USED_IN_ANALYSIS = base.Columns["DRIVER_IS_USED_IN_ANALYSIS"];
				this.columnCREATED_DATE = base.Columns["CREATED_DATE"];
				this.columnMOD_DATE = base.Columns["MOD_DATE"];
				this.columnLAST_UPDATED_BY_RES_UID = base.Columns["LAST_UPDATED_BY_RES_UID"];
				this.columnLAST_UPDATED_BY_RES_NAME = base.Columns["LAST_UPDATED_BY_RES_NAME"];
				this.columnCREATED_BY_RES_UID = base.Columns["CREATED_BY_RES_UID"];
				this.columnCREATED_BY_RES_NAME = base.Columns["CREATED_BY_RES_NAME"];
			}

			// Token: 0x06001250 RID: 4688 RVA: 0x0003B98C File Offset: 0x00039B8C
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			private void InitClass()
			{
				this.columnDRIVER_UID = new DataColumn("DRIVER_UID", typeof(Guid), null, MappingType.Element);
				base.Columns.Add(this.columnDRIVER_UID);
				this.columnDRIVER_NAME = new DataColumn("DRIVER_NAME", typeof(string), null, MappingType.Element);
				base.Columns.Add(this.columnDRIVER_NAME);
				this.columnDRIVER_DESCRIPTION = new DataColumn("DRIVER_DESCRIPTION", typeof(string), null, MappingType.Element);
				base.Columns.Add(this.columnDRIVER_DESCRIPTION);
				this.columnDRIVER_IS_ACTIVE = new DataColumn("DRIVER_IS_ACTIVE", typeof(bool), null, MappingType.Element);
				base.Columns.Add(this.columnDRIVER_IS_ACTIVE);
				this.columnDRIVER_IS_USED_IN_PRIORITIZATION = new DataColumn("DRIVER_IS_USED_IN_PRIORITIZATION", typeof(bool), null, MappingType.Element);
				base.Columns.Add(this.columnDRIVER_IS_USED_IN_PRIORITIZATION);
				this.columnDRIVER_IS_USED_IN_ANALYSIS = new DataColumn("DRIVER_IS_USED_IN_ANALYSIS", typeof(bool), null, MappingType.Element);
				base.Columns.Add(this.columnDRIVER_IS_USED_IN_ANALYSIS);
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
				base.Constraints.Add(new UniqueConstraint("Driver_Key", new DataColumn[]
				{
					this.columnDRIVER_UID
				}, true));
				this.columnDRIVER_UID.AllowDBNull = false;
				this.columnDRIVER_UID.Unique = true;
				this.columnDRIVER_NAME.AllowDBNull = false;
				this.columnDRIVER_IS_USED_IN_PRIORITIZATION.ReadOnly = true;
				this.columnDRIVER_IS_USED_IN_ANALYSIS.ReadOnly = true;
				this.columnLAST_UPDATED_BY_RES_NAME.ReadOnly = true;
				this.columnCREATED_BY_RES_NAME.ReadOnly = true;
			}

			// Token: 0x06001251 RID: 4689 RVA: 0x0003BC30 File Offset: 0x00039E30
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public DriverDataSet.DriverRow NewDriverRow()
			{
				return (DriverDataSet.DriverRow)base.NewRow();
			}

			// Token: 0x06001252 RID: 4690 RVA: 0x0003BC3D File Offset: 0x00039E3D
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			protected override DataRow NewRowFromBuilder(DataRowBuilder builder)
			{
				return new DriverDataSet.DriverRow(builder);
			}

			// Token: 0x06001253 RID: 4691 RVA: 0x0003BC45 File Offset: 0x00039E45
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			protected override Type GetRowType()
			{
				return typeof(DriverDataSet.DriverRow);
			}

			// Token: 0x06001254 RID: 4692 RVA: 0x0003BC51 File Offset: 0x00039E51
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			protected override void OnRowChanged(DataRowChangeEventArgs e)
			{
				base.OnRowChanged(e);
				if (this.DriverRowChanged != null)
				{
					this.DriverRowChanged(this, new DriverDataSet.DriverRowChangeEvent((DriverDataSet.DriverRow)e.Row, e.Action));
				}
			}

			// Token: 0x06001255 RID: 4693 RVA: 0x0003BC84 File Offset: 0x00039E84
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			protected override void OnRowChanging(DataRowChangeEventArgs e)
			{
				base.OnRowChanging(e);
				if (this.DriverRowChanging != null)
				{
					this.DriverRowChanging(this, new DriverDataSet.DriverRowChangeEvent((DriverDataSet.DriverRow)e.Row, e.Action));
				}
			}

			// Token: 0x06001256 RID: 4694 RVA: 0x0003BCB7 File Offset: 0x00039EB7
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			protected override void OnRowDeleted(DataRowChangeEventArgs e)
			{
				base.OnRowDeleted(e);
				if (this.DriverRowDeleted != null)
				{
					this.DriverRowDeleted(this, new DriverDataSet.DriverRowChangeEvent((DriverDataSet.DriverRow)e.Row, e.Action));
				}
			}

			// Token: 0x06001257 RID: 4695 RVA: 0x0003BCEA File Offset: 0x00039EEA
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			protected override void OnRowDeleting(DataRowChangeEventArgs e)
			{
				base.OnRowDeleting(e);
				if (this.DriverRowDeleting != null)
				{
					this.DriverRowDeleting(this, new DriverDataSet.DriverRowChangeEvent((DriverDataSet.DriverRow)e.Row, e.Action));
				}
			}

			// Token: 0x06001258 RID: 4696 RVA: 0x0003BD1D File Offset: 0x00039F1D
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public void RemoveDriverRow(DriverDataSet.DriverRow row)
			{
				base.Rows.Remove(row);
			}

			// Token: 0x06001259 RID: 4697 RVA: 0x0003BD2C File Offset: 0x00039F2C
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			public static XmlSchemaComplexType GetTypedTableSchema(XmlSchemaSet xs)
			{
				XmlSchemaComplexType xmlSchemaComplexType = new XmlSchemaComplexType();
				XmlSchemaSequence xmlSchemaSequence = new XmlSchemaSequence();
				DriverDataSet driverDataSet = new DriverDataSet();
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
				xmlSchemaAttribute.FixedValue = driverDataSet.Namespace;
				xmlSchemaComplexType.Attributes.Add(xmlSchemaAttribute);
				XmlSchemaAttribute xmlSchemaAttribute2 = new XmlSchemaAttribute();
				xmlSchemaAttribute2.Name = "tableTypeName";
				xmlSchemaAttribute2.FixedValue = "DriverDataTable";
				xmlSchemaComplexType.Attributes.Add(xmlSchemaAttribute2);
				xmlSchemaComplexType.Particle = xmlSchemaSequence;
				XmlSchema schemaSerializable = driverDataSet.GetSchemaSerializable();
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

			// Token: 0x0400041B RID: 1051
			private DataColumn columnDRIVER_UID;

			// Token: 0x0400041C RID: 1052
			private DataColumn columnDRIVER_NAME;

			// Token: 0x0400041D RID: 1053
			private DataColumn columnDRIVER_DESCRIPTION;

			// Token: 0x0400041E RID: 1054
			private DataColumn columnDRIVER_IS_ACTIVE;

			// Token: 0x0400041F RID: 1055
			private DataColumn columnDRIVER_IS_USED_IN_PRIORITIZATION;

			// Token: 0x04000420 RID: 1056
			private DataColumn columnDRIVER_IS_USED_IN_ANALYSIS;

			// Token: 0x04000421 RID: 1057
			private DataColumn columnCREATED_DATE;

			// Token: 0x04000422 RID: 1058
			private DataColumn columnMOD_DATE;

			// Token: 0x04000423 RID: 1059
			private DataColumn columnLAST_UPDATED_BY_RES_UID;

			// Token: 0x04000424 RID: 1060
			private DataColumn columnLAST_UPDATED_BY_RES_NAME;

			// Token: 0x04000425 RID: 1061
			private DataColumn columnCREATED_BY_RES_UID;

			// Token: 0x04000426 RID: 1062
			private DataColumn columnCREATED_BY_RES_NAME;
		}

		// Token: 0x020000FB RID: 251
		[XmlSchemaProvider("GetTypedTableSchema")]
		[Serializable]
		public class DriverImpactStatementsDataTable : DataTable, IEnumerable
		{
			// Token: 0x0600125A RID: 4698 RVA: 0x0003BF24 File Offset: 0x0003A124
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			public DriverImpactStatementsDataTable()
			{
				base.TableName = "DriverImpactStatements";
				this.BeginInit();
				this.InitClass();
				this.EndInit();
			}

			// Token: 0x0600125B RID: 4699 RVA: 0x0003BF4C File Offset: 0x0003A14C
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			internal DriverImpactStatementsDataTable(DataTable table)
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

			// Token: 0x0600125C RID: 4700 RVA: 0x0003BFF4 File Offset: 0x0003A1F4
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			protected DriverImpactStatementsDataTable(SerializationInfo info, StreamingContext context) : base(info, context)
			{
				this.InitVars();
			}

			// Token: 0x1700056E RID: 1390
			// (get) Token: 0x0600125D RID: 4701 RVA: 0x0003C004 File Offset: 0x0003A204
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			public DataColumn DRIVER_UIDColumn
			{
				get
				{
					return this.columnDRIVER_UID;
				}
			}

			// Token: 0x1700056F RID: 1391
			// (get) Token: 0x0600125E RID: 4702 RVA: 0x0003C00C File Offset: 0x0003A20C
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			public DataColumn PROJECT_IMPACT_CF_UIDColumn
			{
				get
				{
					return this.columnPROJECT_IMPACT_CF_UID;
				}
			}

			// Token: 0x17000570 RID: 1392
			// (get) Token: 0x0600125F RID: 4703 RVA: 0x0003C014 File Offset: 0x0003A214
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public DataColumn PROJECT_IMPACT_CF_NAMEColumn
			{
				get
				{
					return this.columnPROJECT_IMPACT_CF_NAME;
				}
			}

			// Token: 0x17000571 RID: 1393
			// (get) Token: 0x06001260 RID: 4704 RVA: 0x0003C01C File Offset: 0x0003A21C
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			public DataColumn LT_STRUCT_UIDColumn
			{
				get
				{
					return this.columnLT_STRUCT_UID;
				}
			}

			// Token: 0x17000572 RID: 1394
			// (get) Token: 0x06001261 RID: 4705 RVA: 0x0003C024 File Offset: 0x0003A224
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public DataColumn DESCRIPTIONColumn
			{
				get
				{
					return this.columnDESCRIPTION;
				}
			}

			// Token: 0x17000573 RID: 1395
			// (get) Token: 0x06001262 RID: 4706 RVA: 0x0003C02C File Offset: 0x0003A22C
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

			// Token: 0x17000574 RID: 1396
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public DriverDataSet.DriverImpactStatementsRow this[int index]
			{
				get
				{
					return (DriverDataSet.DriverImpactStatementsRow)base.Rows[index];
				}
			}

			// Token: 0x140000CD RID: 205
			// (add) Token: 0x06001264 RID: 4708 RVA: 0x0003C04C File Offset: 0x0003A24C
			// (remove) Token: 0x06001265 RID: 4709 RVA: 0x0003C084 File Offset: 0x0003A284
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public event DriverDataSet.DriverImpactStatementsRowChangeEventHandler DriverImpactStatementsRowChanging;

			// Token: 0x140000CE RID: 206
			// (add) Token: 0x06001266 RID: 4710 RVA: 0x0003C0BC File Offset: 0x0003A2BC
			// (remove) Token: 0x06001267 RID: 4711 RVA: 0x0003C0F4 File Offset: 0x0003A2F4
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public event DriverDataSet.DriverImpactStatementsRowChangeEventHandler DriverImpactStatementsRowChanged;

			// Token: 0x140000CF RID: 207
			// (add) Token: 0x06001268 RID: 4712 RVA: 0x0003C12C File Offset: 0x0003A32C
			// (remove) Token: 0x06001269 RID: 4713 RVA: 0x0003C164 File Offset: 0x0003A364
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public event DriverDataSet.DriverImpactStatementsRowChangeEventHandler DriverImpactStatementsRowDeleting;

			// Token: 0x140000D0 RID: 208
			// (add) Token: 0x0600126A RID: 4714 RVA: 0x0003C19C File Offset: 0x0003A39C
			// (remove) Token: 0x0600126B RID: 4715 RVA: 0x0003C1D4 File Offset: 0x0003A3D4
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public event DriverDataSet.DriverImpactStatementsRowChangeEventHandler DriverImpactStatementsRowDeleted;

			// Token: 0x0600126C RID: 4716 RVA: 0x0003C209 File Offset: 0x0003A409
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public void AddDriverImpactStatementsRow(DriverDataSet.DriverImpactStatementsRow row)
			{
				base.Rows.Add(row);
			}

			// Token: 0x0600126D RID: 4717 RVA: 0x0003C218 File Offset: 0x0003A418
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			public DriverDataSet.DriverImpactStatementsRow AddDriverImpactStatementsRow(DriverDataSet.DriverRow parentDriverRowByFK_Driver_DriverImpactStatements, Guid PROJECT_IMPACT_CF_UID, string PROJECT_IMPACT_CF_NAME, Guid LT_STRUCT_UID, string DESCRIPTION)
			{
				DriverDataSet.DriverImpactStatementsRow driverImpactStatementsRow = (DriverDataSet.DriverImpactStatementsRow)base.NewRow();
				object[] array = new object[]
				{
					null,
					PROJECT_IMPACT_CF_UID,
					PROJECT_IMPACT_CF_NAME,
					LT_STRUCT_UID,
					DESCRIPTION
				};
				if (parentDriverRowByFK_Driver_DriverImpactStatements != null)
				{
					array[0] = parentDriverRowByFK_Driver_DriverImpactStatements[0];
				}
				driverImpactStatementsRow.ItemArray = array;
				base.Rows.Add(driverImpactStatementsRow);
				return driverImpactStatementsRow;
			}

			// Token: 0x0600126E RID: 4718 RVA: 0x0003C278 File Offset: 0x0003A478
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			public DriverDataSet.DriverImpactStatementsRow FindByDRIVER_UIDPROJECT_IMPACT_CF_UIDLT_STRUCT_UID(Guid DRIVER_UID, Guid PROJECT_IMPACT_CF_UID, Guid LT_STRUCT_UID)
			{
				return (DriverDataSet.DriverImpactStatementsRow)base.Rows.Find(new object[]
				{
					DRIVER_UID,
					PROJECT_IMPACT_CF_UID,
					LT_STRUCT_UID
				});
			}

			// Token: 0x0600126F RID: 4719 RVA: 0x0003C2B8 File Offset: 0x0003A4B8
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			public virtual IEnumerator GetEnumerator()
			{
				return base.Rows.GetEnumerator();
			}

			// Token: 0x06001270 RID: 4720 RVA: 0x0003C2C8 File Offset: 0x0003A4C8
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			public override DataTable Clone()
			{
				DriverDataSet.DriverImpactStatementsDataTable driverImpactStatementsDataTable = (DriverDataSet.DriverImpactStatementsDataTable)base.Clone();
				driverImpactStatementsDataTable.InitVars();
				return driverImpactStatementsDataTable;
			}

			// Token: 0x06001271 RID: 4721 RVA: 0x0003C2E8 File Offset: 0x0003A4E8
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			protected override DataTable CreateInstance()
			{
				return new DriverDataSet.DriverImpactStatementsDataTable();
			}

			// Token: 0x06001272 RID: 4722 RVA: 0x0003C2F0 File Offset: 0x0003A4F0
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			internal void InitVars()
			{
				this.columnDRIVER_UID = base.Columns["DRIVER_UID"];
				this.columnPROJECT_IMPACT_CF_UID = base.Columns["PROJECT_IMPACT_CF_UID"];
				this.columnPROJECT_IMPACT_CF_NAME = base.Columns["PROJECT_IMPACT_CF_NAME"];
				this.columnLT_STRUCT_UID = base.Columns["LT_STRUCT_UID"];
				this.columnDESCRIPTION = base.Columns["DESCRIPTION"];
			}

			// Token: 0x06001273 RID: 4723 RVA: 0x0003C36C File Offset: 0x0003A56C
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			private void InitClass()
			{
				this.columnDRIVER_UID = new DataColumn("DRIVER_UID", typeof(Guid), null, MappingType.Element);
				base.Columns.Add(this.columnDRIVER_UID);
				this.columnPROJECT_IMPACT_CF_UID = new DataColumn("PROJECT_IMPACT_CF_UID", typeof(Guid), null, MappingType.Element);
				base.Columns.Add(this.columnPROJECT_IMPACT_CF_UID);
				this.columnPROJECT_IMPACT_CF_NAME = new DataColumn("PROJECT_IMPACT_CF_NAME", typeof(string), null, MappingType.Element);
				base.Columns.Add(this.columnPROJECT_IMPACT_CF_NAME);
				this.columnLT_STRUCT_UID = new DataColumn("LT_STRUCT_UID", typeof(Guid), null, MappingType.Element);
				base.Columns.Add(this.columnLT_STRUCT_UID);
				this.columnDESCRIPTION = new DataColumn("DESCRIPTION", typeof(string), null, MappingType.Element);
				base.Columns.Add(this.columnDESCRIPTION);
				base.Constraints.Add(new UniqueConstraint("DriverImpactStatements_Key", new DataColumn[]
				{
					this.columnDRIVER_UID,
					this.columnPROJECT_IMPACT_CF_UID,
					this.columnLT_STRUCT_UID
				}, true));
				this.columnDRIVER_UID.AllowDBNull = false;
				this.columnPROJECT_IMPACT_CF_UID.AllowDBNull = false;
				this.columnPROJECT_IMPACT_CF_NAME.ReadOnly = true;
				this.columnLT_STRUCT_UID.AllowDBNull = false;
			}

			// Token: 0x06001274 RID: 4724 RVA: 0x0003C4C3 File Offset: 0x0003A6C3
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			public DriverDataSet.DriverImpactStatementsRow NewDriverImpactStatementsRow()
			{
				return (DriverDataSet.DriverImpactStatementsRow)base.NewRow();
			}

			// Token: 0x06001275 RID: 4725 RVA: 0x0003C4D0 File Offset: 0x0003A6D0
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			protected override DataRow NewRowFromBuilder(DataRowBuilder builder)
			{
				return new DriverDataSet.DriverImpactStatementsRow(builder);
			}

			// Token: 0x06001276 RID: 4726 RVA: 0x0003C4D8 File Offset: 0x0003A6D8
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			protected override Type GetRowType()
			{
				return typeof(DriverDataSet.DriverImpactStatementsRow);
			}

			// Token: 0x06001277 RID: 4727 RVA: 0x0003C4E4 File Offset: 0x0003A6E4
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			protected override void OnRowChanged(DataRowChangeEventArgs e)
			{
				base.OnRowChanged(e);
				if (this.DriverImpactStatementsRowChanged != null)
				{
					this.DriverImpactStatementsRowChanged(this, new DriverDataSet.DriverImpactStatementsRowChangeEvent((DriverDataSet.DriverImpactStatementsRow)e.Row, e.Action));
				}
			}

			// Token: 0x06001278 RID: 4728 RVA: 0x0003C517 File Offset: 0x0003A717
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			protected override void OnRowChanging(DataRowChangeEventArgs e)
			{
				base.OnRowChanging(e);
				if (this.DriverImpactStatementsRowChanging != null)
				{
					this.DriverImpactStatementsRowChanging(this, new DriverDataSet.DriverImpactStatementsRowChangeEvent((DriverDataSet.DriverImpactStatementsRow)e.Row, e.Action));
				}
			}

			// Token: 0x06001279 RID: 4729 RVA: 0x0003C54A File Offset: 0x0003A74A
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			protected override void OnRowDeleted(DataRowChangeEventArgs e)
			{
				base.OnRowDeleted(e);
				if (this.DriverImpactStatementsRowDeleted != null)
				{
					this.DriverImpactStatementsRowDeleted(this, new DriverDataSet.DriverImpactStatementsRowChangeEvent((DriverDataSet.DriverImpactStatementsRow)e.Row, e.Action));
				}
			}

			// Token: 0x0600127A RID: 4730 RVA: 0x0003C57D File Offset: 0x0003A77D
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			protected override void OnRowDeleting(DataRowChangeEventArgs e)
			{
				base.OnRowDeleting(e);
				if (this.DriverImpactStatementsRowDeleting != null)
				{
					this.DriverImpactStatementsRowDeleting(this, new DriverDataSet.DriverImpactStatementsRowChangeEvent((DriverDataSet.DriverImpactStatementsRow)e.Row, e.Action));
				}
			}

			// Token: 0x0600127B RID: 4731 RVA: 0x0003C5B0 File Offset: 0x0003A7B0
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			public void RemoveDriverImpactStatementsRow(DriverDataSet.DriverImpactStatementsRow row)
			{
				base.Rows.Remove(row);
			}

			// Token: 0x0600127C RID: 4732 RVA: 0x0003C5C0 File Offset: 0x0003A7C0
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public static XmlSchemaComplexType GetTypedTableSchema(XmlSchemaSet xs)
			{
				XmlSchemaComplexType xmlSchemaComplexType = new XmlSchemaComplexType();
				XmlSchemaSequence xmlSchemaSequence = new XmlSchemaSequence();
				DriverDataSet driverDataSet = new DriverDataSet();
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
				xmlSchemaAttribute.FixedValue = driverDataSet.Namespace;
				xmlSchemaComplexType.Attributes.Add(xmlSchemaAttribute);
				XmlSchemaAttribute xmlSchemaAttribute2 = new XmlSchemaAttribute();
				xmlSchemaAttribute2.Name = "tableTypeName";
				xmlSchemaAttribute2.FixedValue = "DriverImpactStatementsDataTable";
				xmlSchemaComplexType.Attributes.Add(xmlSchemaAttribute2);
				xmlSchemaComplexType.Particle = xmlSchemaSequence;
				XmlSchema schemaSerializable = driverDataSet.GetSchemaSerializable();
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

			// Token: 0x0400042B RID: 1067
			private DataColumn columnDRIVER_UID;

			// Token: 0x0400042C RID: 1068
			private DataColumn columnPROJECT_IMPACT_CF_UID;

			// Token: 0x0400042D RID: 1069
			private DataColumn columnPROJECT_IMPACT_CF_NAME;

			// Token: 0x0400042E RID: 1070
			private DataColumn columnLT_STRUCT_UID;

			// Token: 0x0400042F RID: 1071
			private DataColumn columnDESCRIPTION;
		}

		// Token: 0x020000FC RID: 252
		[XmlSchemaProvider("GetTypedTableSchema")]
		[Serializable]
		public class DriverDepartmentsDataTable : DataTable, IEnumerable
		{
			// Token: 0x0600127D RID: 4733 RVA: 0x0003C7B8 File Offset: 0x0003A9B8
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			public DriverDepartmentsDataTable()
			{
				base.TableName = "DriverDepartments";
				this.BeginInit();
				this.InitClass();
				this.EndInit();
			}

			// Token: 0x0600127E RID: 4734 RVA: 0x0003C7E0 File Offset: 0x0003A9E0
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			internal DriverDepartmentsDataTable(DataTable table)
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

			// Token: 0x0600127F RID: 4735 RVA: 0x0003C888 File Offset: 0x0003AA88
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			protected DriverDepartmentsDataTable(SerializationInfo info, StreamingContext context) : base(info, context)
			{
				this.InitVars();
			}

			// Token: 0x17000575 RID: 1397
			// (get) Token: 0x06001280 RID: 4736 RVA: 0x0003C898 File Offset: 0x0003AA98
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			public DataColumn DRIVER_UIDColumn
			{
				get
				{
					return this.columnDRIVER_UID;
				}
			}

			// Token: 0x17000576 RID: 1398
			// (get) Token: 0x06001281 RID: 4737 RVA: 0x0003C8A0 File Offset: 0x0003AAA0
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			public DataColumn DEPARTMENT_UIDColumn
			{
				get
				{
					return this.columnDEPARTMENT_UID;
				}
			}

			// Token: 0x17000577 RID: 1399
			// (get) Token: 0x06001282 RID: 4738 RVA: 0x0003C8A8 File Offset: 0x0003AAA8
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public DataColumn DEPARTMENT_NAMEColumn
			{
				get
				{
					return this.columnDEPARTMENT_NAME;
				}
			}

			// Token: 0x17000578 RID: 1400
			// (get) Token: 0x06001283 RID: 4739 RVA: 0x0003C8B0 File Offset: 0x0003AAB0
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

			// Token: 0x17000579 RID: 1401
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			public DriverDataSet.DriverDepartmentsRow this[int index]
			{
				get
				{
					return (DriverDataSet.DriverDepartmentsRow)base.Rows[index];
				}
			}

			// Token: 0x140000D1 RID: 209
			// (add) Token: 0x06001285 RID: 4741 RVA: 0x0003C8D0 File Offset: 0x0003AAD0
			// (remove) Token: 0x06001286 RID: 4742 RVA: 0x0003C908 File Offset: 0x0003AB08
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public event DriverDataSet.DriverDepartmentsRowChangeEventHandler DriverDepartmentsRowChanging;

			// Token: 0x140000D2 RID: 210
			// (add) Token: 0x06001287 RID: 4743 RVA: 0x0003C940 File Offset: 0x0003AB40
			// (remove) Token: 0x06001288 RID: 4744 RVA: 0x0003C978 File Offset: 0x0003AB78
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public event DriverDataSet.DriverDepartmentsRowChangeEventHandler DriverDepartmentsRowChanged;

			// Token: 0x140000D3 RID: 211
			// (add) Token: 0x06001289 RID: 4745 RVA: 0x0003C9B0 File Offset: 0x0003ABB0
			// (remove) Token: 0x0600128A RID: 4746 RVA: 0x0003C9E8 File Offset: 0x0003ABE8
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public event DriverDataSet.DriverDepartmentsRowChangeEventHandler DriverDepartmentsRowDeleting;

			// Token: 0x140000D4 RID: 212
			// (add) Token: 0x0600128B RID: 4747 RVA: 0x0003CA20 File Offset: 0x0003AC20
			// (remove) Token: 0x0600128C RID: 4748 RVA: 0x0003CA58 File Offset: 0x0003AC58
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public event DriverDataSet.DriverDepartmentsRowChangeEventHandler DriverDepartmentsRowDeleted;

			// Token: 0x0600128D RID: 4749 RVA: 0x0003CA8D File Offset: 0x0003AC8D
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			public void AddDriverDepartmentsRow(DriverDataSet.DriverDepartmentsRow row)
			{
				base.Rows.Add(row);
			}

			// Token: 0x0600128E RID: 4750 RVA: 0x0003CA9C File Offset: 0x0003AC9C
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			public DriverDataSet.DriverDepartmentsRow AddDriverDepartmentsRow(DriverDataSet.DriverRow parentDriverRowByFK_Driver_DriverDepartments, Guid DEPARTMENT_UID, string DEPARTMENT_NAME)
			{
				DriverDataSet.DriverDepartmentsRow driverDepartmentsRow = (DriverDataSet.DriverDepartmentsRow)base.NewRow();
				object[] array = new object[]
				{
					null,
					DEPARTMENT_UID,
					DEPARTMENT_NAME
				};
				if (parentDriverRowByFK_Driver_DriverDepartments != null)
				{
					array[0] = parentDriverRowByFK_Driver_DriverDepartments[0];
				}
				driverDepartmentsRow.ItemArray = array;
				base.Rows.Add(driverDepartmentsRow);
				return driverDepartmentsRow;
			}

			// Token: 0x0600128F RID: 4751 RVA: 0x0003CAEC File Offset: 0x0003ACEC
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			public DriverDataSet.DriverDepartmentsRow FindByDRIVER_UIDDEPARTMENT_UID(Guid DRIVER_UID, Guid DEPARTMENT_UID)
			{
				return (DriverDataSet.DriverDepartmentsRow)base.Rows.Find(new object[]
				{
					DRIVER_UID,
					DEPARTMENT_UID
				});
			}

			// Token: 0x06001290 RID: 4752 RVA: 0x0003CB23 File Offset: 0x0003AD23
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			public virtual IEnumerator GetEnumerator()
			{
				return base.Rows.GetEnumerator();
			}

			// Token: 0x06001291 RID: 4753 RVA: 0x0003CB30 File Offset: 0x0003AD30
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			public override DataTable Clone()
			{
				DriverDataSet.DriverDepartmentsDataTable driverDepartmentsDataTable = (DriverDataSet.DriverDepartmentsDataTable)base.Clone();
				driverDepartmentsDataTable.InitVars();
				return driverDepartmentsDataTable;
			}

			// Token: 0x06001292 RID: 4754 RVA: 0x0003CB50 File Offset: 0x0003AD50
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			protected override DataTable CreateInstance()
			{
				return new DriverDataSet.DriverDepartmentsDataTable();
			}

			// Token: 0x06001293 RID: 4755 RVA: 0x0003CB58 File Offset: 0x0003AD58
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			internal void InitVars()
			{
				this.columnDRIVER_UID = base.Columns["DRIVER_UID"];
				this.columnDEPARTMENT_UID = base.Columns["DEPARTMENT_UID"];
				this.columnDEPARTMENT_NAME = base.Columns["DEPARTMENT_NAME"];
			}

			// Token: 0x06001294 RID: 4756 RVA: 0x0003CBA8 File Offset: 0x0003ADA8
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			private void InitClass()
			{
				this.columnDRIVER_UID = new DataColumn("DRIVER_UID", typeof(Guid), null, MappingType.Element);
				base.Columns.Add(this.columnDRIVER_UID);
				this.columnDEPARTMENT_UID = new DataColumn("DEPARTMENT_UID", typeof(Guid), null, MappingType.Element);
				base.Columns.Add(this.columnDEPARTMENT_UID);
				this.columnDEPARTMENT_NAME = new DataColumn("DEPARTMENT_NAME", typeof(string), null, MappingType.Element);
				base.Columns.Add(this.columnDEPARTMENT_NAME);
				base.Constraints.Add(new UniqueConstraint("DriverDepartments_Key", new DataColumn[]
				{
					this.columnDRIVER_UID,
					this.columnDEPARTMENT_UID
				}, true));
				this.columnDRIVER_UID.AllowDBNull = false;
				this.columnDEPARTMENT_UID.AllowDBNull = false;
				this.columnDEPARTMENT_NAME.ReadOnly = true;
			}

			// Token: 0x06001295 RID: 4757 RVA: 0x0003CC90 File Offset: 0x0003AE90
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			public DriverDataSet.DriverDepartmentsRow NewDriverDepartmentsRow()
			{
				return (DriverDataSet.DriverDepartmentsRow)base.NewRow();
			}

			// Token: 0x06001296 RID: 4758 RVA: 0x0003CC9D File Offset: 0x0003AE9D
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			protected override DataRow NewRowFromBuilder(DataRowBuilder builder)
			{
				return new DriverDataSet.DriverDepartmentsRow(builder);
			}

			// Token: 0x06001297 RID: 4759 RVA: 0x0003CCA5 File Offset: 0x0003AEA5
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			protected override Type GetRowType()
			{
				return typeof(DriverDataSet.DriverDepartmentsRow);
			}

			// Token: 0x06001298 RID: 4760 RVA: 0x0003CCB1 File Offset: 0x0003AEB1
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			protected override void OnRowChanged(DataRowChangeEventArgs e)
			{
				base.OnRowChanged(e);
				if (this.DriverDepartmentsRowChanged != null)
				{
					this.DriverDepartmentsRowChanged(this, new DriverDataSet.DriverDepartmentsRowChangeEvent((DriverDataSet.DriverDepartmentsRow)e.Row, e.Action));
				}
			}

			// Token: 0x06001299 RID: 4761 RVA: 0x0003CCE4 File Offset: 0x0003AEE4
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			protected override void OnRowChanging(DataRowChangeEventArgs e)
			{
				base.OnRowChanging(e);
				if (this.DriverDepartmentsRowChanging != null)
				{
					this.DriverDepartmentsRowChanging(this, new DriverDataSet.DriverDepartmentsRowChangeEvent((DriverDataSet.DriverDepartmentsRow)e.Row, e.Action));
				}
			}

			// Token: 0x0600129A RID: 4762 RVA: 0x0003CD17 File Offset: 0x0003AF17
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			protected override void OnRowDeleted(DataRowChangeEventArgs e)
			{
				base.OnRowDeleted(e);
				if (this.DriverDepartmentsRowDeleted != null)
				{
					this.DriverDepartmentsRowDeleted(this, new DriverDataSet.DriverDepartmentsRowChangeEvent((DriverDataSet.DriverDepartmentsRow)e.Row, e.Action));
				}
			}

			// Token: 0x0600129B RID: 4763 RVA: 0x0003CD4A File Offset: 0x0003AF4A
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			protected override void OnRowDeleting(DataRowChangeEventArgs e)
			{
				base.OnRowDeleting(e);
				if (this.DriverDepartmentsRowDeleting != null)
				{
					this.DriverDepartmentsRowDeleting(this, new DriverDataSet.DriverDepartmentsRowChangeEvent((DriverDataSet.DriverDepartmentsRow)e.Row, e.Action));
				}
			}

			// Token: 0x0600129C RID: 4764 RVA: 0x0003CD7D File Offset: 0x0003AF7D
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			public void RemoveDriverDepartmentsRow(DriverDataSet.DriverDepartmentsRow row)
			{
				base.Rows.Remove(row);
			}

			// Token: 0x0600129D RID: 4765 RVA: 0x0003CD8C File Offset: 0x0003AF8C
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			public static XmlSchemaComplexType GetTypedTableSchema(XmlSchemaSet xs)
			{
				XmlSchemaComplexType xmlSchemaComplexType = new XmlSchemaComplexType();
				XmlSchemaSequence xmlSchemaSequence = new XmlSchemaSequence();
				DriverDataSet driverDataSet = new DriverDataSet();
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
				xmlSchemaAttribute.FixedValue = driverDataSet.Namespace;
				xmlSchemaComplexType.Attributes.Add(xmlSchemaAttribute);
				XmlSchemaAttribute xmlSchemaAttribute2 = new XmlSchemaAttribute();
				xmlSchemaAttribute2.Name = "tableTypeName";
				xmlSchemaAttribute2.FixedValue = "DriverDepartmentsDataTable";
				xmlSchemaComplexType.Attributes.Add(xmlSchemaAttribute2);
				xmlSchemaComplexType.Particle = xmlSchemaSequence;
				XmlSchema schemaSerializable = driverDataSet.GetSchemaSerializable();
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

			// Token: 0x04000434 RID: 1076
			private DataColumn columnDRIVER_UID;

			// Token: 0x04000435 RID: 1077
			private DataColumn columnDEPARTMENT_UID;

			// Token: 0x04000436 RID: 1078
			private DataColumn columnDEPARTMENT_NAME;
		}

		// Token: 0x020000FD RID: 253
		public class DriverRow : DataRow
		{
			// Token: 0x0600129E RID: 4766 RVA: 0x0003CF84 File Offset: 0x0003B184
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			internal DriverRow(DataRowBuilder rb) : base(rb)
			{
				this.tableDriver = (DriverDataSet.DriverDataTable)base.Table;
			}

			// Token: 0x1700057A RID: 1402
			// (get) Token: 0x0600129F RID: 4767 RVA: 0x0003CF9E File Offset: 0x0003B19E
			// (set) Token: 0x060012A0 RID: 4768 RVA: 0x0003CFB6 File Offset: 0x0003B1B6
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public Guid DRIVER_UID
			{
				get
				{
					return (Guid)base[this.tableDriver.DRIVER_UIDColumn];
				}
				set
				{
					base[this.tableDriver.DRIVER_UIDColumn] = value;
				}
			}

			// Token: 0x1700057B RID: 1403
			// (get) Token: 0x060012A1 RID: 4769 RVA: 0x0003CFCF File Offset: 0x0003B1CF
			// (set) Token: 0x060012A2 RID: 4770 RVA: 0x0003CFE7 File Offset: 0x0003B1E7
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public string DRIVER_NAME
			{
				get
				{
					return (string)base[this.tableDriver.DRIVER_NAMEColumn];
				}
				set
				{
					base[this.tableDriver.DRIVER_NAMEColumn] = value;
				}
			}

			// Token: 0x1700057C RID: 1404
			// (get) Token: 0x060012A3 RID: 4771 RVA: 0x0003CFFC File Offset: 0x0003B1FC
			// (set) Token: 0x060012A4 RID: 4772 RVA: 0x0003D040 File Offset: 0x0003B240
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public string DRIVER_DESCRIPTION
			{
				get
				{
					string result;
					try
					{
						result = (string)base[this.tableDriver.DRIVER_DESCRIPTIONColumn];
					}
					catch (InvalidCastException innerException)
					{
						throw new StrongTypingException("The value for column 'DRIVER_DESCRIPTION' in table 'Driver' is DBNull.", innerException);
					}
					return result;
				}
				set
				{
					base[this.tableDriver.DRIVER_DESCRIPTIONColumn] = value;
				}
			}

			// Token: 0x1700057D RID: 1405
			// (get) Token: 0x060012A5 RID: 4773 RVA: 0x0003D054 File Offset: 0x0003B254
			// (set) Token: 0x060012A6 RID: 4774 RVA: 0x0003D098 File Offset: 0x0003B298
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			public bool DRIVER_IS_ACTIVE
			{
				get
				{
					bool result;
					try
					{
						result = (bool)base[this.tableDriver.DRIVER_IS_ACTIVEColumn];
					}
					catch (InvalidCastException innerException)
					{
						throw new StrongTypingException("The value for column 'DRIVER_IS_ACTIVE' in table 'Driver' is DBNull.", innerException);
					}
					return result;
				}
				set
				{
					base[this.tableDriver.DRIVER_IS_ACTIVEColumn] = value;
				}
			}

			// Token: 0x1700057E RID: 1406
			// (get) Token: 0x060012A7 RID: 4775 RVA: 0x0003D0B4 File Offset: 0x0003B2B4
			// (set) Token: 0x060012A8 RID: 4776 RVA: 0x0003D0F8 File Offset: 0x0003B2F8
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			public bool DRIVER_IS_USED_IN_PRIORITIZATION
			{
				get
				{
					bool result;
					try
					{
						result = (bool)base[this.tableDriver.DRIVER_IS_USED_IN_PRIORITIZATIONColumn];
					}
					catch (InvalidCastException innerException)
					{
						throw new StrongTypingException("The value for column 'DRIVER_IS_USED_IN_PRIORITIZATION' in table 'Driver' is DBNull.", innerException);
					}
					return result;
				}
				set
				{
					base[this.tableDriver.DRIVER_IS_USED_IN_PRIORITIZATIONColumn] = value;
				}
			}

			// Token: 0x1700057F RID: 1407
			// (get) Token: 0x060012A9 RID: 4777 RVA: 0x0003D114 File Offset: 0x0003B314
			// (set) Token: 0x060012AA RID: 4778 RVA: 0x0003D158 File Offset: 0x0003B358
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public bool DRIVER_IS_USED_IN_ANALYSIS
			{
				get
				{
					bool result;
					try
					{
						result = (bool)base[this.tableDriver.DRIVER_IS_USED_IN_ANALYSISColumn];
					}
					catch (InvalidCastException innerException)
					{
						throw new StrongTypingException("The value for column 'DRIVER_IS_USED_IN_ANALYSIS' in table 'Driver' is DBNull.", innerException);
					}
					return result;
				}
				set
				{
					base[this.tableDriver.DRIVER_IS_USED_IN_ANALYSISColumn] = value;
				}
			}

			// Token: 0x17000580 RID: 1408
			// (get) Token: 0x060012AB RID: 4779 RVA: 0x0003D174 File Offset: 0x0003B374
			// (set) Token: 0x060012AC RID: 4780 RVA: 0x0003D1B8 File Offset: 0x0003B3B8
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public DateTime CREATED_DATE
			{
				get
				{
					DateTime result;
					try
					{
						result = (DateTime)base[this.tableDriver.CREATED_DATEColumn];
					}
					catch (InvalidCastException innerException)
					{
						throw new StrongTypingException("The value for column 'CREATED_DATE' in table 'Driver' is DBNull.", innerException);
					}
					return result;
				}
				set
				{
					base[this.tableDriver.CREATED_DATEColumn] = value;
				}
			}

			// Token: 0x17000581 RID: 1409
			// (get) Token: 0x060012AD RID: 4781 RVA: 0x0003D1D4 File Offset: 0x0003B3D4
			// (set) Token: 0x060012AE RID: 4782 RVA: 0x0003D218 File Offset: 0x0003B418
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			public DateTime MOD_DATE
			{
				get
				{
					DateTime result;
					try
					{
						result = (DateTime)base[this.tableDriver.MOD_DATEColumn];
					}
					catch (InvalidCastException innerException)
					{
						throw new StrongTypingException("The value for column 'MOD_DATE' in table 'Driver' is DBNull.", innerException);
					}
					return result;
				}
				set
				{
					base[this.tableDriver.MOD_DATEColumn] = value;
				}
			}

			// Token: 0x17000582 RID: 1410
			// (get) Token: 0x060012AF RID: 4783 RVA: 0x0003D234 File Offset: 0x0003B434
			// (set) Token: 0x060012B0 RID: 4784 RVA: 0x0003D278 File Offset: 0x0003B478
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public Guid LAST_UPDATED_BY_RES_UID
			{
				get
				{
					Guid result;
					try
					{
						result = (Guid)base[this.tableDriver.LAST_UPDATED_BY_RES_UIDColumn];
					}
					catch (InvalidCastException innerException)
					{
						throw new StrongTypingException("The value for column 'LAST_UPDATED_BY_RES_UID' in table 'Driver' is DBNull.", innerException);
					}
					return result;
				}
				set
				{
					base[this.tableDriver.LAST_UPDATED_BY_RES_UIDColumn] = value;
				}
			}

			// Token: 0x17000583 RID: 1411
			// (get) Token: 0x060012B1 RID: 4785 RVA: 0x0003D294 File Offset: 0x0003B494
			// (set) Token: 0x060012B2 RID: 4786 RVA: 0x0003D2D8 File Offset: 0x0003B4D8
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			public string LAST_UPDATED_BY_RES_NAME
			{
				get
				{
					string result;
					try
					{
						result = (string)base[this.tableDriver.LAST_UPDATED_BY_RES_NAMEColumn];
					}
					catch (InvalidCastException innerException)
					{
						throw new StrongTypingException("The value for column 'LAST_UPDATED_BY_RES_NAME' in table 'Driver' is DBNull.", innerException);
					}
					return result;
				}
				set
				{
					base[this.tableDriver.LAST_UPDATED_BY_RES_NAMEColumn] = value;
				}
			}

			// Token: 0x17000584 RID: 1412
			// (get) Token: 0x060012B3 RID: 4787 RVA: 0x0003D2EC File Offset: 0x0003B4EC
			// (set) Token: 0x060012B4 RID: 4788 RVA: 0x0003D330 File Offset: 0x0003B530
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			public Guid CREATED_BY_RES_UID
			{
				get
				{
					Guid result;
					try
					{
						result = (Guid)base[this.tableDriver.CREATED_BY_RES_UIDColumn];
					}
					catch (InvalidCastException innerException)
					{
						throw new StrongTypingException("The value for column 'CREATED_BY_RES_UID' in table 'Driver' is DBNull.", innerException);
					}
					return result;
				}
				set
				{
					base[this.tableDriver.CREATED_BY_RES_UIDColumn] = value;
				}
			}

			// Token: 0x17000585 RID: 1413
			// (get) Token: 0x060012B5 RID: 4789 RVA: 0x0003D34C File Offset: 0x0003B54C
			// (set) Token: 0x060012B6 RID: 4790 RVA: 0x0003D390 File Offset: 0x0003B590
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public string CREATED_BY_RES_NAME
			{
				get
				{
					string result;
					try
					{
						result = (string)base[this.tableDriver.CREATED_BY_RES_NAMEColumn];
					}
					catch (InvalidCastException innerException)
					{
						throw new StrongTypingException("The value for column 'CREATED_BY_RES_NAME' in table 'Driver' is DBNull.", innerException);
					}
					return result;
				}
				set
				{
					base[this.tableDriver.CREATED_BY_RES_NAMEColumn] = value;
				}
			}

			// Token: 0x060012B7 RID: 4791 RVA: 0x0003D3A4 File Offset: 0x0003B5A4
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			public bool IsDRIVER_DESCRIPTIONNull()
			{
				return base.IsNull(this.tableDriver.DRIVER_DESCRIPTIONColumn);
			}

			// Token: 0x060012B8 RID: 4792 RVA: 0x0003D3B7 File Offset: 0x0003B5B7
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			public void SetDRIVER_DESCRIPTIONNull()
			{
				base[this.tableDriver.DRIVER_DESCRIPTIONColumn] = Convert.DBNull;
			}

			// Token: 0x060012B9 RID: 4793 RVA: 0x0003D3CF File Offset: 0x0003B5CF
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public bool IsDRIVER_IS_ACTIVENull()
			{
				return base.IsNull(this.tableDriver.DRIVER_IS_ACTIVEColumn);
			}

			// Token: 0x060012BA RID: 4794 RVA: 0x0003D3E2 File Offset: 0x0003B5E2
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			public void SetDRIVER_IS_ACTIVENull()
			{
				base[this.tableDriver.DRIVER_IS_ACTIVEColumn] = Convert.DBNull;
			}

			// Token: 0x060012BB RID: 4795 RVA: 0x0003D3FA File Offset: 0x0003B5FA
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public bool IsDRIVER_IS_USED_IN_PRIORITIZATIONNull()
			{
				return base.IsNull(this.tableDriver.DRIVER_IS_USED_IN_PRIORITIZATIONColumn);
			}

			// Token: 0x060012BC RID: 4796 RVA: 0x0003D40D File Offset: 0x0003B60D
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			public void SetDRIVER_IS_USED_IN_PRIORITIZATIONNull()
			{
				base[this.tableDriver.DRIVER_IS_USED_IN_PRIORITIZATIONColumn] = Convert.DBNull;
			}

			// Token: 0x060012BD RID: 4797 RVA: 0x0003D425 File Offset: 0x0003B625
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public bool IsDRIVER_IS_USED_IN_ANALYSISNull()
			{
				return base.IsNull(this.tableDriver.DRIVER_IS_USED_IN_ANALYSISColumn);
			}

			// Token: 0x060012BE RID: 4798 RVA: 0x0003D438 File Offset: 0x0003B638
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			public void SetDRIVER_IS_USED_IN_ANALYSISNull()
			{
				base[this.tableDriver.DRIVER_IS_USED_IN_ANALYSISColumn] = Convert.DBNull;
			}

			// Token: 0x060012BF RID: 4799 RVA: 0x0003D450 File Offset: 0x0003B650
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public bool IsCREATED_DATENull()
			{
				return base.IsNull(this.tableDriver.CREATED_DATEColumn);
			}

			// Token: 0x060012C0 RID: 4800 RVA: 0x0003D463 File Offset: 0x0003B663
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public void SetCREATED_DATENull()
			{
				base[this.tableDriver.CREATED_DATEColumn] = Convert.DBNull;
			}

			// Token: 0x060012C1 RID: 4801 RVA: 0x0003D47B File Offset: 0x0003B67B
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			public bool IsMOD_DATENull()
			{
				return base.IsNull(this.tableDriver.MOD_DATEColumn);
			}

			// Token: 0x060012C2 RID: 4802 RVA: 0x0003D48E File Offset: 0x0003B68E
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public void SetMOD_DATENull()
			{
				base[this.tableDriver.MOD_DATEColumn] = Convert.DBNull;
			}

			// Token: 0x060012C3 RID: 4803 RVA: 0x0003D4A6 File Offset: 0x0003B6A6
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			public bool IsLAST_UPDATED_BY_RES_UIDNull()
			{
				return base.IsNull(this.tableDriver.LAST_UPDATED_BY_RES_UIDColumn);
			}

			// Token: 0x060012C4 RID: 4804 RVA: 0x0003D4B9 File Offset: 0x0003B6B9
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			public void SetLAST_UPDATED_BY_RES_UIDNull()
			{
				base[this.tableDriver.LAST_UPDATED_BY_RES_UIDColumn] = Convert.DBNull;
			}

			// Token: 0x060012C5 RID: 4805 RVA: 0x0003D4D1 File Offset: 0x0003B6D1
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			public bool IsLAST_UPDATED_BY_RES_NAMENull()
			{
				return base.IsNull(this.tableDriver.LAST_UPDATED_BY_RES_NAMEColumn);
			}

			// Token: 0x060012C6 RID: 4806 RVA: 0x0003D4E4 File Offset: 0x0003B6E4
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			public void SetLAST_UPDATED_BY_RES_NAMENull()
			{
				base[this.tableDriver.LAST_UPDATED_BY_RES_NAMEColumn] = Convert.DBNull;
			}

			// Token: 0x060012C7 RID: 4807 RVA: 0x0003D4FC File Offset: 0x0003B6FC
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			public bool IsCREATED_BY_RES_UIDNull()
			{
				return base.IsNull(this.tableDriver.CREATED_BY_RES_UIDColumn);
			}

			// Token: 0x060012C8 RID: 4808 RVA: 0x0003D50F File Offset: 0x0003B70F
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public void SetCREATED_BY_RES_UIDNull()
			{
				base[this.tableDriver.CREATED_BY_RES_UIDColumn] = Convert.DBNull;
			}

			// Token: 0x060012C9 RID: 4809 RVA: 0x0003D527 File Offset: 0x0003B727
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public bool IsCREATED_BY_RES_NAMENull()
			{
				return base.IsNull(this.tableDriver.CREATED_BY_RES_NAMEColumn);
			}

			// Token: 0x060012CA RID: 4810 RVA: 0x0003D53A File Offset: 0x0003B73A
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			public void SetCREATED_BY_RES_NAMENull()
			{
				base[this.tableDriver.CREATED_BY_RES_NAMEColumn] = Convert.DBNull;
			}

			// Token: 0x060012CB RID: 4811 RVA: 0x0003D552 File Offset: 0x0003B752
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public DriverDataSet.DriverImpactStatementsRow[] GetDriverImpactStatementsRows()
			{
				if (base.Table.ChildRelations["FK_Driver_DriverImpactStatements"] == null)
				{
					return new DriverDataSet.DriverImpactStatementsRow[0];
				}
				return (DriverDataSet.DriverImpactStatementsRow[])base.GetChildRows(base.Table.ChildRelations["FK_Driver_DriverImpactStatements"]);
			}

			// Token: 0x060012CC RID: 4812 RVA: 0x0003D592 File Offset: 0x0003B792
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public DriverDataSet.DriverDepartmentsRow[] GetDriverDepartmentsRows()
			{
				if (base.Table.ChildRelations["FK_Driver_DriverDepartments"] == null)
				{
					return new DriverDataSet.DriverDepartmentsRow[0];
				}
				return (DriverDataSet.DriverDepartmentsRow[])base.GetChildRows(base.Table.ChildRelations["FK_Driver_DriverDepartments"]);
			}

			// Token: 0x0400043B RID: 1083
			private DriverDataSet.DriverDataTable tableDriver;
		}

		// Token: 0x020000FE RID: 254
		public class DriverImpactStatementsRow : DataRow
		{
			// Token: 0x060012CD RID: 4813 RVA: 0x0003D5D2 File Offset: 0x0003B7D2
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			internal DriverImpactStatementsRow(DataRowBuilder rb) : base(rb)
			{
				this.tableDriverImpactStatements = (DriverDataSet.DriverImpactStatementsDataTable)base.Table;
			}

			// Token: 0x17000586 RID: 1414
			// (get) Token: 0x060012CE RID: 4814 RVA: 0x0003D5EC File Offset: 0x0003B7EC
			// (set) Token: 0x060012CF RID: 4815 RVA: 0x0003D604 File Offset: 0x0003B804
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public Guid DRIVER_UID
			{
				get
				{
					return (Guid)base[this.tableDriverImpactStatements.DRIVER_UIDColumn];
				}
				set
				{
					base[this.tableDriverImpactStatements.DRIVER_UIDColumn] = value;
				}
			}

			// Token: 0x17000587 RID: 1415
			// (get) Token: 0x060012D0 RID: 4816 RVA: 0x0003D61D File Offset: 0x0003B81D
			// (set) Token: 0x060012D1 RID: 4817 RVA: 0x0003D635 File Offset: 0x0003B835
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			public Guid PROJECT_IMPACT_CF_UID
			{
				get
				{
					return (Guid)base[this.tableDriverImpactStatements.PROJECT_IMPACT_CF_UIDColumn];
				}
				set
				{
					base[this.tableDriverImpactStatements.PROJECT_IMPACT_CF_UIDColumn] = value;
				}
			}

			// Token: 0x17000588 RID: 1416
			// (get) Token: 0x060012D2 RID: 4818 RVA: 0x0003D650 File Offset: 0x0003B850
			// (set) Token: 0x060012D3 RID: 4819 RVA: 0x0003D694 File Offset: 0x0003B894
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public string PROJECT_IMPACT_CF_NAME
			{
				get
				{
					string result;
					try
					{
						result = (string)base[this.tableDriverImpactStatements.PROJECT_IMPACT_CF_NAMEColumn];
					}
					catch (InvalidCastException innerException)
					{
						throw new StrongTypingException("The value for column 'PROJECT_IMPACT_CF_NAME' in table 'DriverImpactStatements' is DBNull.", innerException);
					}
					return result;
				}
				set
				{
					base[this.tableDriverImpactStatements.PROJECT_IMPACT_CF_NAMEColumn] = value;
				}
			}

			// Token: 0x17000589 RID: 1417
			// (get) Token: 0x060012D4 RID: 4820 RVA: 0x0003D6A8 File Offset: 0x0003B8A8
			// (set) Token: 0x060012D5 RID: 4821 RVA: 0x0003D6C0 File Offset: 0x0003B8C0
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public Guid LT_STRUCT_UID
			{
				get
				{
					return (Guid)base[this.tableDriverImpactStatements.LT_STRUCT_UIDColumn];
				}
				set
				{
					base[this.tableDriverImpactStatements.LT_STRUCT_UIDColumn] = value;
				}
			}

			// Token: 0x1700058A RID: 1418
			// (get) Token: 0x060012D6 RID: 4822 RVA: 0x0003D6DC File Offset: 0x0003B8DC
			// (set) Token: 0x060012D7 RID: 4823 RVA: 0x0003D720 File Offset: 0x0003B920
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public string DESCRIPTION
			{
				get
				{
					string result;
					try
					{
						result = (string)base[this.tableDriverImpactStatements.DESCRIPTIONColumn];
					}
					catch (InvalidCastException innerException)
					{
						throw new StrongTypingException("The value for column 'DESCRIPTION' in table 'DriverImpactStatements' is DBNull.", innerException);
					}
					return result;
				}
				set
				{
					base[this.tableDriverImpactStatements.DESCRIPTIONColumn] = value;
				}
			}

			// Token: 0x1700058B RID: 1419
			// (get) Token: 0x060012D8 RID: 4824 RVA: 0x0003D734 File Offset: 0x0003B934
			// (set) Token: 0x060012D9 RID: 4825 RVA: 0x0003D756 File Offset: 0x0003B956
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public DriverDataSet.DriverRow DriverRow
			{
				get
				{
					return (DriverDataSet.DriverRow)base.GetParentRow(base.Table.ParentRelations["FK_Driver_DriverImpactStatements"]);
				}
				set
				{
					base.SetParentRow(value, base.Table.ParentRelations["FK_Driver_DriverImpactStatements"]);
				}
			}

			// Token: 0x060012DA RID: 4826 RVA: 0x0003D774 File Offset: 0x0003B974
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			public bool IsPROJECT_IMPACT_CF_NAMENull()
			{
				return base.IsNull(this.tableDriverImpactStatements.PROJECT_IMPACT_CF_NAMEColumn);
			}

			// Token: 0x060012DB RID: 4827 RVA: 0x0003D787 File Offset: 0x0003B987
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			public void SetPROJECT_IMPACT_CF_NAMENull()
			{
				base[this.tableDriverImpactStatements.PROJECT_IMPACT_CF_NAMEColumn] = Convert.DBNull;
			}

			// Token: 0x060012DC RID: 4828 RVA: 0x0003D79F File Offset: 0x0003B99F
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			public bool IsDESCRIPTIONNull()
			{
				return base.IsNull(this.tableDriverImpactStatements.DESCRIPTIONColumn);
			}

			// Token: 0x060012DD RID: 4829 RVA: 0x0003D7B2 File Offset: 0x0003B9B2
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			public void SetDESCRIPTIONNull()
			{
				base[this.tableDriverImpactStatements.DESCRIPTIONColumn] = Convert.DBNull;
			}

			// Token: 0x0400043C RID: 1084
			private DriverDataSet.DriverImpactStatementsDataTable tableDriverImpactStatements;
		}

		// Token: 0x020000FF RID: 255
		public class DriverDepartmentsRow : DataRow
		{
			// Token: 0x060012DE RID: 4830 RVA: 0x0003D7CA File Offset: 0x0003B9CA
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			internal DriverDepartmentsRow(DataRowBuilder rb) : base(rb)
			{
				this.tableDriverDepartments = (DriverDataSet.DriverDepartmentsDataTable)base.Table;
			}

			// Token: 0x1700058C RID: 1420
			// (get) Token: 0x060012DF RID: 4831 RVA: 0x0003D7E4 File Offset: 0x0003B9E4
			// (set) Token: 0x060012E0 RID: 4832 RVA: 0x0003D7FC File Offset: 0x0003B9FC
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public Guid DRIVER_UID
			{
				get
				{
					return (Guid)base[this.tableDriverDepartments.DRIVER_UIDColumn];
				}
				set
				{
					base[this.tableDriverDepartments.DRIVER_UIDColumn] = value;
				}
			}

			// Token: 0x1700058D RID: 1421
			// (get) Token: 0x060012E1 RID: 4833 RVA: 0x0003D815 File Offset: 0x0003BA15
			// (set) Token: 0x060012E2 RID: 4834 RVA: 0x0003D82D File Offset: 0x0003BA2D
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public Guid DEPARTMENT_UID
			{
				get
				{
					return (Guid)base[this.tableDriverDepartments.DEPARTMENT_UIDColumn];
				}
				set
				{
					base[this.tableDriverDepartments.DEPARTMENT_UIDColumn] = value;
				}
			}

			// Token: 0x1700058E RID: 1422
			// (get) Token: 0x060012E3 RID: 4835 RVA: 0x0003D848 File Offset: 0x0003BA48
			// (set) Token: 0x060012E4 RID: 4836 RVA: 0x0003D88C File Offset: 0x0003BA8C
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public string DEPARTMENT_NAME
			{
				get
				{
					string result;
					try
					{
						result = (string)base[this.tableDriverDepartments.DEPARTMENT_NAMEColumn];
					}
					catch (InvalidCastException innerException)
					{
						throw new StrongTypingException("The value for column 'DEPARTMENT_NAME' in table 'DriverDepartments' is DBNull.", innerException);
					}
					return result;
				}
				set
				{
					base[this.tableDriverDepartments.DEPARTMENT_NAMEColumn] = value;
				}
			}

			// Token: 0x1700058F RID: 1423
			// (get) Token: 0x060012E5 RID: 4837 RVA: 0x0003D8A0 File Offset: 0x0003BAA0
			// (set) Token: 0x060012E6 RID: 4838 RVA: 0x0003D8C2 File Offset: 0x0003BAC2
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public DriverDataSet.DriverRow DriverRow
			{
				get
				{
					return (DriverDataSet.DriverRow)base.GetParentRow(base.Table.ParentRelations["FK_Driver_DriverDepartments"]);
				}
				set
				{
					base.SetParentRow(value, base.Table.ParentRelations["FK_Driver_DriverDepartments"]);
				}
			}

			// Token: 0x060012E7 RID: 4839 RVA: 0x0003D8E0 File Offset: 0x0003BAE0
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public bool IsDEPARTMENT_NAMENull()
			{
				return base.IsNull(this.tableDriverDepartments.DEPARTMENT_NAMEColumn);
			}

			// Token: 0x060012E8 RID: 4840 RVA: 0x0003D8F3 File Offset: 0x0003BAF3
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public void SetDEPARTMENT_NAMENull()
			{
				base[this.tableDriverDepartments.DEPARTMENT_NAMEColumn] = Convert.DBNull;
			}

			// Token: 0x0400043D RID: 1085
			private DriverDataSet.DriverDepartmentsDataTable tableDriverDepartments;
		}

		// Token: 0x02000100 RID: 256
		[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
		public class DriverRowChangeEvent : EventArgs
		{
			// Token: 0x060012E9 RID: 4841 RVA: 0x0003D90B File Offset: 0x0003BB0B
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public DriverRowChangeEvent(DriverDataSet.DriverRow row, DataRowAction action)
			{
				this.eventRow = row;
				this.eventAction = action;
			}

			// Token: 0x17000590 RID: 1424
			// (get) Token: 0x060012EA RID: 4842 RVA: 0x0003D921 File Offset: 0x0003BB21
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public DriverDataSet.DriverRow Row
			{
				get
				{
					return this.eventRow;
				}
			}

			// Token: 0x17000591 RID: 1425
			// (get) Token: 0x060012EB RID: 4843 RVA: 0x0003D929 File Offset: 0x0003BB29
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			public DataRowAction Action
			{
				get
				{
					return this.eventAction;
				}
			}

			// Token: 0x0400043E RID: 1086
			private DriverDataSet.DriverRow eventRow;

			// Token: 0x0400043F RID: 1087
			private DataRowAction eventAction;
		}

		// Token: 0x02000101 RID: 257
		[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
		public class DriverImpactStatementsRowChangeEvent : EventArgs
		{
			// Token: 0x060012EC RID: 4844 RVA: 0x0003D931 File Offset: 0x0003BB31
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public DriverImpactStatementsRowChangeEvent(DriverDataSet.DriverImpactStatementsRow row, DataRowAction action)
			{
				this.eventRow = row;
				this.eventAction = action;
			}

			// Token: 0x17000592 RID: 1426
			// (get) Token: 0x060012ED RID: 4845 RVA: 0x0003D947 File Offset: 0x0003BB47
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public DriverDataSet.DriverImpactStatementsRow Row
			{
				get
				{
					return this.eventRow;
				}
			}

			// Token: 0x17000593 RID: 1427
			// (get) Token: 0x060012EE RID: 4846 RVA: 0x0003D94F File Offset: 0x0003BB4F
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public DataRowAction Action
			{
				get
				{
					return this.eventAction;
				}
			}

			// Token: 0x04000440 RID: 1088
			private DriverDataSet.DriverImpactStatementsRow eventRow;

			// Token: 0x04000441 RID: 1089
			private DataRowAction eventAction;
		}

		// Token: 0x02000102 RID: 258
		[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
		public class DriverDepartmentsRowChangeEvent : EventArgs
		{
			// Token: 0x060012EF RID: 4847 RVA: 0x0003D957 File Offset: 0x0003BB57
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public DriverDepartmentsRowChangeEvent(DriverDataSet.DriverDepartmentsRow row, DataRowAction action)
			{
				this.eventRow = row;
				this.eventAction = action;
			}

			// Token: 0x17000594 RID: 1428
			// (get) Token: 0x060012F0 RID: 4848 RVA: 0x0003D96D File Offset: 0x0003BB6D
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public DriverDataSet.DriverDepartmentsRow Row
			{
				get
				{
					return this.eventRow;
				}
			}

			// Token: 0x17000595 RID: 1429
			// (get) Token: 0x060012F1 RID: 4849 RVA: 0x0003D975 File Offset: 0x0003BB75
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			public DataRowAction Action
			{
				get
				{
					return this.eventAction;
				}
			}

			// Token: 0x04000442 RID: 1090
			private DriverDataSet.DriverDepartmentsRow eventRow;

			// Token: 0x04000443 RID: 1091
			private DataRowAction eventAction;
		}
	}
}
