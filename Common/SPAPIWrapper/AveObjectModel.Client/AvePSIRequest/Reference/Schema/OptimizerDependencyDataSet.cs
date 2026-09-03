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
	// Token: 0x0200021D RID: 541
	[DesignerCategory("code")]
	[ToolboxItem(true)]
	[XmlSchemaProvider("GetTypedDataSetSchema")]
	[HelpKeyword("vs.data.DataSet")]
	[XmlRoot("OptimizerDependencyDataSet")]
	[Serializable]
	public class OptimizerDependencyDataSet : DataSet
	{
		// Token: 0x06002BEC RID: 11244 RVA: 0x0008C770 File Offset: 0x0008A970
		public new void EndInit()
		{
			base.EndInit();
			TypedDataSetUtilities.AllowNullsInNonTypedColumns(this.OptimizerDependencyDetails, new string[]
			{
				"POSITION",
				"DEPENDENCY_UID",
				"PROJ_NAME",
				"PROJ_UID"
			});
			TypedDataSetUtilities.AllowNullsInNonTypedColumns(this.OptimizerDependencies, new string[]
			{
				"LAST_UPDATED_BY_RES_NAME",
				"CREATED_DATE",
				"CREATED_BY_RES_UID",
				"LAST_UPDATED_BY_RES_UID",
				"DEPENDENCY_TYPE",
				"DEPENDENCY_NAME",
				"DEPENDENCY_UID",
				"DEPENDENCY_DESCRIPTION",
				"MOD_DATE",
				"CREATED_BY_RES_NAME"
			});
		}

		// Token: 0x06002BED RID: 11245 RVA: 0x0008C81C File Offset: 0x0008AA1C
		[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
		[DebuggerNonUserCode]
		public OptimizerDependencyDataSet()
		{
			base.BeginInit();
			this.InitClass();
			CollectionChangeEventHandler value = new CollectionChangeEventHandler(this.SchemaChanged);
			base.Tables.CollectionChanged += value;
			base.Relations.CollectionChanged += value;
			this.EndInit();
		}

		// Token: 0x06002BEE RID: 11246 RVA: 0x0008C870 File Offset: 0x0008AA70
		[DebuggerNonUserCode]
		[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
		protected OptimizerDependencyDataSet(SerializationInfo info, StreamingContext context) : base(info, context, false)
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
				if (dataSet.Tables["OptimizerDependencies"] != null)
				{
					base.Tables.Add(new OptimizerDependencyDataSet.OptimizerDependenciesDataTable(dataSet.Tables["OptimizerDependencies"]));
				}
				if (dataSet.Tables["OptimizerDependencyDetails"] != null)
				{
					base.Tables.Add(new OptimizerDependencyDataSet.OptimizerDependencyDetailsDataTable(dataSet.Tables["OptimizerDependencyDetails"]));
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

		// Token: 0x17000CEE RID: 3310
		// (get) Token: 0x06002BEF RID: 11247 RVA: 0x0008C9FF File Offset: 0x0008ABFF
		[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
		[DebuggerNonUserCode]
		[Browsable(false)]
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Content)]
		public OptimizerDependencyDataSet.OptimizerDependenciesDataTable OptimizerDependencies
		{
			get
			{
				return this.tableOptimizerDependencies;
			}
		}

		// Token: 0x17000CEF RID: 3311
		// (get) Token: 0x06002BF0 RID: 11248 RVA: 0x0008CA07 File Offset: 0x0008AC07
		[DebuggerNonUserCode]
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Content)]
		[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
		[Browsable(false)]
		public OptimizerDependencyDataSet.OptimizerDependencyDetailsDataTable OptimizerDependencyDetails
		{
			get
			{
				return this.tableOptimizerDependencyDetails;
			}
		}

		// Token: 0x17000CF0 RID: 3312
		// (get) Token: 0x06002BF1 RID: 11249 RVA: 0x0008CA0F File Offset: 0x0008AC0F
		// (set) Token: 0x06002BF2 RID: 11250 RVA: 0x0008CA17 File Offset: 0x0008AC17
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

		// Token: 0x17000CF1 RID: 3313
		// (get) Token: 0x06002BF3 RID: 11251 RVA: 0x0008CA20 File Offset: 0x0008AC20
		[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
		[DebuggerNonUserCode]
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
		public new DataTableCollection Tables
		{
			get
			{
				return base.Tables;
			}
		}

		// Token: 0x17000CF2 RID: 3314
		// (get) Token: 0x06002BF4 RID: 11252 RVA: 0x0008CA28 File Offset: 0x0008AC28
		[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
		[DebuggerNonUserCode]
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
		public new DataRelationCollection Relations
		{
			get
			{
				return base.Relations;
			}
		}

		// Token: 0x06002BF5 RID: 11253 RVA: 0x0008CA30 File Offset: 0x0008AC30
		[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
		[DebuggerNonUserCode]
		protected override void InitializeDerivedDataSet()
		{
			base.BeginInit();
			this.InitClass();
			this.EndInit();
		}

		// Token: 0x06002BF6 RID: 11254 RVA: 0x0008CA44 File Offset: 0x0008AC44
		[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
		[DebuggerNonUserCode]
		public override DataSet Clone()
		{
			OptimizerDependencyDataSet optimizerDependencyDataSet = (OptimizerDependencyDataSet)base.Clone();
			optimizerDependencyDataSet.InitVars();
			optimizerDependencyDataSet.SchemaSerializationMode = this.SchemaSerializationMode;
			return optimizerDependencyDataSet;
		}

		// Token: 0x06002BF7 RID: 11255 RVA: 0x0008CA70 File Offset: 0x0008AC70
		[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
		[DebuggerNonUserCode]
		protected override bool ShouldSerializeTables()
		{
			return false;
		}

		// Token: 0x06002BF8 RID: 11256 RVA: 0x0008CA73 File Offset: 0x0008AC73
		[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
		[DebuggerNonUserCode]
		protected override bool ShouldSerializeRelations()
		{
			return false;
		}

		// Token: 0x06002BF9 RID: 11257 RVA: 0x0008CA78 File Offset: 0x0008AC78
		[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
		[DebuggerNonUserCode]
		protected override void ReadXmlSerializable(XmlReader reader)
		{
			if (base.DetermineSchemaSerializationMode(reader) == SchemaSerializationMode.IncludeSchema)
			{
				this.Reset();
				DataSet dataSet = new DataSet();
				dataSet.ReadXml(reader);
				if (dataSet.Tables["OptimizerDependencies"] != null)
				{
					base.Tables.Add(new OptimizerDependencyDataSet.OptimizerDependenciesDataTable(dataSet.Tables["OptimizerDependencies"]));
				}
				if (dataSet.Tables["OptimizerDependencyDetails"] != null)
				{
					base.Tables.Add(new OptimizerDependencyDataSet.OptimizerDependencyDetailsDataTable(dataSet.Tables["OptimizerDependencyDetails"]));
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

		// Token: 0x06002BFA RID: 11258 RVA: 0x0008CB70 File Offset: 0x0008AD70
		[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
		[DebuggerNonUserCode]
		protected override XmlSchema GetSchemaSerializable()
		{
			MemoryStream memoryStream = new MemoryStream();
			base.WriteXmlSchema(new XmlTextWriter(memoryStream, null));
			memoryStream.Position = 0L;
			return XmlSchema.Read(new XmlTextReader(memoryStream), null);
		}

		// Token: 0x06002BFB RID: 11259 RVA: 0x0008CBA4 File Offset: 0x0008ADA4
		[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
		[DebuggerNonUserCode]
		internal void InitVars()
		{
			this.InitVars(true);
		}

		// Token: 0x06002BFC RID: 11260 RVA: 0x0008CBB0 File Offset: 0x0008ADB0
		[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
		[DebuggerNonUserCode]
		internal void InitVars(bool initTable)
		{
			this.tableOptimizerDependencies = (OptimizerDependencyDataSet.OptimizerDependenciesDataTable)base.Tables["OptimizerDependencies"];
			if (initTable && this.tableOptimizerDependencies != null)
			{
				this.tableOptimizerDependencies.InitVars();
			}
			this.tableOptimizerDependencyDetails = (OptimizerDependencyDataSet.OptimizerDependencyDetailsDataTable)base.Tables["OptimizerDependencyDetails"];
			if (initTable && this.tableOptimizerDependencyDetails != null)
			{
				this.tableOptimizerDependencyDetails.InitVars();
			}
			this.relationFK_OptimizerDependencies_OptimizerDependencyDetails = this.Relations["FK_OptimizerDependencies_OptimizerDependencyDetails"];
		}

		// Token: 0x06002BFD RID: 11261 RVA: 0x0008CC38 File Offset: 0x0008AE38
		[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
		[DebuggerNonUserCode]
		private void InitClass()
		{
			base.DataSetName = "OptimizerDependencyDataSet";
			base.Prefix = "";
			base.Namespace = "http://schemas.microsoft.com/office/project/server/webservices/OptimizerDependencyDataSet/";
			base.EnforceConstraints = true;
			this.SchemaSerializationMode = SchemaSerializationMode.IncludeSchema;
			this.tableOptimizerDependencies = new OptimizerDependencyDataSet.OptimizerDependenciesDataTable();
			base.Tables.Add(this.tableOptimizerDependencies);
			this.tableOptimizerDependencyDetails = new OptimizerDependencyDataSet.OptimizerDependencyDetailsDataTable();
			base.Tables.Add(this.tableOptimizerDependencyDetails);
			ForeignKeyConstraint foreignKeyConstraint = new ForeignKeyConstraint("FK_OptimizerDependencies_OptimizerDependencyDetails", new DataColumn[]
			{
				this.tableOptimizerDependencies.DEPENDENCY_UIDColumn
			}, new DataColumn[]
			{
				this.tableOptimizerDependencyDetails.DEPENDENCY_UIDColumn
			});
			this.tableOptimizerDependencyDetails.Constraints.Add(foreignKeyConstraint);
			foreignKeyConstraint.AcceptRejectRule = AcceptRejectRule.None;
			foreignKeyConstraint.DeleteRule = Rule.None;
			foreignKeyConstraint.UpdateRule = Rule.None;
			this.relationFK_OptimizerDependencies_OptimizerDependencyDetails = new DataRelation("FK_OptimizerDependencies_OptimizerDependencyDetails", new DataColumn[]
			{
				this.tableOptimizerDependencies.DEPENDENCY_UIDColumn
			}, new DataColumn[]
			{
				this.tableOptimizerDependencyDetails.DEPENDENCY_UIDColumn
			}, false);
			this.Relations.Add(this.relationFK_OptimizerDependencies_OptimizerDependencyDetails);
		}

		// Token: 0x06002BFE RID: 11262 RVA: 0x0008CD5A File Offset: 0x0008AF5A
		[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
		[DebuggerNonUserCode]
		private bool ShouldSerializeOptimizerDependencies()
		{
			return false;
		}

		// Token: 0x06002BFF RID: 11263 RVA: 0x0008CD5D File Offset: 0x0008AF5D
		[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
		[DebuggerNonUserCode]
		private bool ShouldSerializeOptimizerDependencyDetails()
		{
			return false;
		}

		// Token: 0x06002C00 RID: 11264 RVA: 0x0008CD60 File Offset: 0x0008AF60
		[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
		[DebuggerNonUserCode]
		private void SchemaChanged(object sender, CollectionChangeEventArgs e)
		{
			if (e.Action == CollectionChangeAction.Remove)
			{
				this.InitVars();
			}
		}

		// Token: 0x06002C01 RID: 11265 RVA: 0x0008CD74 File Offset: 0x0008AF74
		[DebuggerNonUserCode]
		[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
		public static XmlSchemaComplexType GetTypedDataSetSchema(XmlSchemaSet xs)
		{
			OptimizerDependencyDataSet optimizerDependencyDataSet = new OptimizerDependencyDataSet();
			XmlSchemaComplexType xmlSchemaComplexType = new XmlSchemaComplexType();
			XmlSchemaSequence xmlSchemaSequence = new XmlSchemaSequence();
			XmlSchemaAny xmlSchemaAny = new XmlSchemaAny();
			xmlSchemaAny.Namespace = optimizerDependencyDataSet.Namespace;
			xmlSchemaSequence.Items.Add(xmlSchemaAny);
			xmlSchemaComplexType.Particle = xmlSchemaSequence;
			XmlSchema schemaSerializable = optimizerDependencyDataSet.GetSchemaSerializable();
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

		// Token: 0x0400093E RID: 2366
		private OptimizerDependencyDataSet.OptimizerDependenciesDataTable tableOptimizerDependencies;

		// Token: 0x0400093F RID: 2367
		private OptimizerDependencyDataSet.OptimizerDependencyDetailsDataTable tableOptimizerDependencyDetails;

		// Token: 0x04000940 RID: 2368
		private DataRelation relationFK_OptimizerDependencies_OptimizerDependencyDetails;

		// Token: 0x04000941 RID: 2369
		private SchemaSerializationMode _schemaSerializationMode = SchemaSerializationMode.IncludeSchema;

		// Token: 0x0200021E RID: 542
		// (Invoke) Token: 0x06002C03 RID: 11267
		[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
		public delegate void OptimizerDependenciesRowChangeEventHandler(object sender, OptimizerDependencyDataSet.OptimizerDependenciesRowChangeEvent e);

		// Token: 0x0200021F RID: 543
		// (Invoke) Token: 0x06002C07 RID: 11271
		[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
		public delegate void OptimizerDependencyDetailsRowChangeEventHandler(object sender, OptimizerDependencyDataSet.OptimizerDependencyDetailsRowChangeEvent e);

		// Token: 0x02000220 RID: 544
		[XmlSchemaProvider("GetTypedTableSchema")]
		[Serializable]
		public class OptimizerDependenciesDataTable : DataTable, IEnumerable
		{
			// Token: 0x06002C0A RID: 11274 RVA: 0x0008CEBC File Offset: 0x0008B0BC
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			public OptimizerDependenciesDataTable()
			{
				base.TableName = "OptimizerDependencies";
				this.BeginInit();
				this.InitClass();
				this.EndInit();
			}

			// Token: 0x06002C0B RID: 11275 RVA: 0x0008CEE4 File Offset: 0x0008B0E4
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			internal OptimizerDependenciesDataTable(DataTable table)
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

			// Token: 0x06002C0C RID: 11276 RVA: 0x0008CF8C File Offset: 0x0008B18C
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			protected OptimizerDependenciesDataTable(SerializationInfo info, StreamingContext context) : base(info, context)
			{
				this.InitVars();
			}

			// Token: 0x17000CF3 RID: 3315
			// (get) Token: 0x06002C0D RID: 11277 RVA: 0x0008CF9C File Offset: 0x0008B19C
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public DataColumn DEPENDENCY_UIDColumn
			{
				get
				{
					return this.columnDEPENDENCY_UID;
				}
			}

			// Token: 0x17000CF4 RID: 3316
			// (get) Token: 0x06002C0E RID: 11278 RVA: 0x0008CFA4 File Offset: 0x0008B1A4
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public DataColumn DEPENDENCY_NAMEColumn
			{
				get
				{
					return this.columnDEPENDENCY_NAME;
				}
			}

			// Token: 0x17000CF5 RID: 3317
			// (get) Token: 0x06002C0F RID: 11279 RVA: 0x0008CFAC File Offset: 0x0008B1AC
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			public DataColumn DEPENDENCY_TYPEColumn
			{
				get
				{
					return this.columnDEPENDENCY_TYPE;
				}
			}

			// Token: 0x17000CF6 RID: 3318
			// (get) Token: 0x06002C10 RID: 11280 RVA: 0x0008CFB4 File Offset: 0x0008B1B4
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			public DataColumn DEPENDENCY_DESCRIPTIONColumn
			{
				get
				{
					return this.columnDEPENDENCY_DESCRIPTION;
				}
			}

			// Token: 0x17000CF7 RID: 3319
			// (get) Token: 0x06002C11 RID: 11281 RVA: 0x0008CFBC File Offset: 0x0008B1BC
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			public DataColumn LAST_UPDATED_BY_RES_UIDColumn
			{
				get
				{
					return this.columnLAST_UPDATED_BY_RES_UID;
				}
			}

			// Token: 0x17000CF8 RID: 3320
			// (get) Token: 0x06002C12 RID: 11282 RVA: 0x0008CFC4 File Offset: 0x0008B1C4
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			public DataColumn CREATED_BY_RES_UIDColumn
			{
				get
				{
					return this.columnCREATED_BY_RES_UID;
				}
			}

			// Token: 0x17000CF9 RID: 3321
			// (get) Token: 0x06002C13 RID: 11283 RVA: 0x0008CFCC File Offset: 0x0008B1CC
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			public DataColumn CREATED_DATEColumn
			{
				get
				{
					return this.columnCREATED_DATE;
				}
			}

			// Token: 0x17000CFA RID: 3322
			// (get) Token: 0x06002C14 RID: 11284 RVA: 0x0008CFD4 File Offset: 0x0008B1D4
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			public DataColumn MOD_DATEColumn
			{
				get
				{
					return this.columnMOD_DATE;
				}
			}

			// Token: 0x17000CFB RID: 3323
			// (get) Token: 0x06002C15 RID: 11285 RVA: 0x0008CFDC File Offset: 0x0008B1DC
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			public DataColumn LAST_UPDATED_BY_RES_NAMEColumn
			{
				get
				{
					return this.columnLAST_UPDATED_BY_RES_NAME;
				}
			}

			// Token: 0x17000CFC RID: 3324
			// (get) Token: 0x06002C16 RID: 11286 RVA: 0x0008CFE4 File Offset: 0x0008B1E4
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			public DataColumn CREATED_BY_RES_NAMEColumn
			{
				get
				{
					return this.columnCREATED_BY_RES_NAME;
				}
			}

			// Token: 0x17000CFD RID: 3325
			// (get) Token: 0x06002C17 RID: 11287 RVA: 0x0008CFEC File Offset: 0x0008B1EC
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

			// Token: 0x17000CFE RID: 3326
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public OptimizerDependencyDataSet.OptimizerDependenciesRow this[int index]
			{
				get
				{
					return (OptimizerDependencyDataSet.OptimizerDependenciesRow)base.Rows[index];
				}
			}

			// Token: 0x140001CD RID: 461
			// (add) Token: 0x06002C19 RID: 11289 RVA: 0x0008D00C File Offset: 0x0008B20C
			// (remove) Token: 0x06002C1A RID: 11290 RVA: 0x0008D044 File Offset: 0x0008B244
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public event OptimizerDependencyDataSet.OptimizerDependenciesRowChangeEventHandler OptimizerDependenciesRowChanging;

			// Token: 0x140001CE RID: 462
			// (add) Token: 0x06002C1B RID: 11291 RVA: 0x0008D07C File Offset: 0x0008B27C
			// (remove) Token: 0x06002C1C RID: 11292 RVA: 0x0008D0B4 File Offset: 0x0008B2B4
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public event OptimizerDependencyDataSet.OptimizerDependenciesRowChangeEventHandler OptimizerDependenciesRowChanged;

			// Token: 0x140001CF RID: 463
			// (add) Token: 0x06002C1D RID: 11293 RVA: 0x0008D0EC File Offset: 0x0008B2EC
			// (remove) Token: 0x06002C1E RID: 11294 RVA: 0x0008D124 File Offset: 0x0008B324
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public event OptimizerDependencyDataSet.OptimizerDependenciesRowChangeEventHandler OptimizerDependenciesRowDeleting;

			// Token: 0x140001D0 RID: 464
			// (add) Token: 0x06002C1F RID: 11295 RVA: 0x0008D15C File Offset: 0x0008B35C
			// (remove) Token: 0x06002C20 RID: 11296 RVA: 0x0008D194 File Offset: 0x0008B394
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public event OptimizerDependencyDataSet.OptimizerDependenciesRowChangeEventHandler OptimizerDependenciesRowDeleted;

			// Token: 0x06002C21 RID: 11297 RVA: 0x0008D1C9 File Offset: 0x0008B3C9
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			public void AddOptimizerDependenciesRow(OptimizerDependencyDataSet.OptimizerDependenciesRow row)
			{
				base.Rows.Add(row);
			}

			// Token: 0x06002C22 RID: 11298 RVA: 0x0008D1D8 File Offset: 0x0008B3D8
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			public OptimizerDependencyDataSet.OptimizerDependenciesRow AddOptimizerDependenciesRow(Guid DEPENDENCY_UID, string DEPENDENCY_NAME, int DEPENDENCY_TYPE, string DEPENDENCY_DESCRIPTION, Guid LAST_UPDATED_BY_RES_UID, Guid CREATED_BY_RES_UID, DateTime CREATED_DATE, DateTime MOD_DATE, string LAST_UPDATED_BY_RES_NAME, string CREATED_BY_RES_NAME)
			{
				OptimizerDependencyDataSet.OptimizerDependenciesRow optimizerDependenciesRow = (OptimizerDependencyDataSet.OptimizerDependenciesRow)base.NewRow();
				object[] itemArray = new object[]
				{
					DEPENDENCY_UID,
					DEPENDENCY_NAME,
					DEPENDENCY_TYPE,
					DEPENDENCY_DESCRIPTION,
					LAST_UPDATED_BY_RES_UID,
					CREATED_BY_RES_UID,
					CREATED_DATE,
					MOD_DATE,
					LAST_UPDATED_BY_RES_NAME,
					CREATED_BY_RES_NAME
				};
				optimizerDependenciesRow.ItemArray = itemArray;
				base.Rows.Add(optimizerDependenciesRow);
				return optimizerDependenciesRow;
			}

			// Token: 0x06002C23 RID: 11299 RVA: 0x0008D260 File Offset: 0x0008B460
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			public OptimizerDependencyDataSet.OptimizerDependenciesRow FindByDEPENDENCY_UID(Guid DEPENDENCY_UID)
			{
				return (OptimizerDependencyDataSet.OptimizerDependenciesRow)base.Rows.Find(new object[]
				{
					DEPENDENCY_UID
				});
			}

			// Token: 0x06002C24 RID: 11300 RVA: 0x0008D28E File Offset: 0x0008B48E
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			public virtual IEnumerator GetEnumerator()
			{
				return base.Rows.GetEnumerator();
			}

			// Token: 0x06002C25 RID: 11301 RVA: 0x0008D29C File Offset: 0x0008B49C
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			public override DataTable Clone()
			{
				OptimizerDependencyDataSet.OptimizerDependenciesDataTable optimizerDependenciesDataTable = (OptimizerDependencyDataSet.OptimizerDependenciesDataTable)base.Clone();
				optimizerDependenciesDataTable.InitVars();
				return optimizerDependenciesDataTable;
			}

			// Token: 0x06002C26 RID: 11302 RVA: 0x0008D2BC File Offset: 0x0008B4BC
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			protected override DataTable CreateInstance()
			{
				return new OptimizerDependencyDataSet.OptimizerDependenciesDataTable();
			}

			// Token: 0x06002C27 RID: 11303 RVA: 0x0008D2C4 File Offset: 0x0008B4C4
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			internal void InitVars()
			{
				this.columnDEPENDENCY_UID = base.Columns["DEPENDENCY_UID"];
				this.columnDEPENDENCY_NAME = base.Columns["DEPENDENCY_NAME"];
				this.columnDEPENDENCY_TYPE = base.Columns["DEPENDENCY_TYPE"];
				this.columnDEPENDENCY_DESCRIPTION = base.Columns["DEPENDENCY_DESCRIPTION"];
				this.columnLAST_UPDATED_BY_RES_UID = base.Columns["LAST_UPDATED_BY_RES_UID"];
				this.columnCREATED_BY_RES_UID = base.Columns["CREATED_BY_RES_UID"];
				this.columnCREATED_DATE = base.Columns["CREATED_DATE"];
				this.columnMOD_DATE = base.Columns["MOD_DATE"];
				this.columnLAST_UPDATED_BY_RES_NAME = base.Columns["LAST_UPDATED_BY_RES_NAME"];
				this.columnCREATED_BY_RES_NAME = base.Columns["CREATED_BY_RES_NAME"];
			}

			// Token: 0x06002C28 RID: 11304 RVA: 0x0008D3B0 File Offset: 0x0008B5B0
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			private void InitClass()
			{
				this.columnDEPENDENCY_UID = new DataColumn("DEPENDENCY_UID", typeof(Guid), null, MappingType.Element);
				base.Columns.Add(this.columnDEPENDENCY_UID);
				this.columnDEPENDENCY_NAME = new DataColumn("DEPENDENCY_NAME", typeof(string), null, MappingType.Element);
				base.Columns.Add(this.columnDEPENDENCY_NAME);
				this.columnDEPENDENCY_TYPE = new DataColumn("DEPENDENCY_TYPE", typeof(int), null, MappingType.Element);
				base.Columns.Add(this.columnDEPENDENCY_TYPE);
				this.columnDEPENDENCY_DESCRIPTION = new DataColumn("DEPENDENCY_DESCRIPTION", typeof(string), null, MappingType.Element);
				base.Columns.Add(this.columnDEPENDENCY_DESCRIPTION);
				this.columnLAST_UPDATED_BY_RES_UID = new DataColumn("LAST_UPDATED_BY_RES_UID", typeof(Guid), null, MappingType.Element);
				base.Columns.Add(this.columnLAST_UPDATED_BY_RES_UID);
				this.columnCREATED_BY_RES_UID = new DataColumn("CREATED_BY_RES_UID", typeof(Guid), null, MappingType.Element);
				base.Columns.Add(this.columnCREATED_BY_RES_UID);
				this.columnCREATED_DATE = new DataColumn("CREATED_DATE", typeof(DateTime), null, MappingType.Element);
				base.Columns.Add(this.columnCREATED_DATE);
				this.columnMOD_DATE = new DataColumn("MOD_DATE", typeof(DateTime), null, MappingType.Element);
				base.Columns.Add(this.columnMOD_DATE);
				this.columnLAST_UPDATED_BY_RES_NAME = new DataColumn("LAST_UPDATED_BY_RES_NAME", typeof(string), null, MappingType.Element);
				base.Columns.Add(this.columnLAST_UPDATED_BY_RES_NAME);
				this.columnCREATED_BY_RES_NAME = new DataColumn("CREATED_BY_RES_NAME", typeof(string), null, MappingType.Element);
				base.Columns.Add(this.columnCREATED_BY_RES_NAME);
				base.Constraints.Add(new UniqueConstraint("Constraint1", new DataColumn[]
				{
					this.columnDEPENDENCY_UID
				}, true));
				this.columnDEPENDENCY_UID.AllowDBNull = false;
				this.columnDEPENDENCY_UID.Unique = true;
				this.columnDEPENDENCY_NAME.AllowDBNull = false;
				this.columnDEPENDENCY_TYPE.AllowDBNull = false;
				this.columnLAST_UPDATED_BY_RES_NAME.ReadOnly = true;
				this.columnCREATED_BY_RES_NAME.ReadOnly = true;
			}

			// Token: 0x06002C29 RID: 11305 RVA: 0x0008D5EE File Offset: 0x0008B7EE
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			public OptimizerDependencyDataSet.OptimizerDependenciesRow NewOptimizerDependenciesRow()
			{
				return (OptimizerDependencyDataSet.OptimizerDependenciesRow)base.NewRow();
			}

			// Token: 0x06002C2A RID: 11306 RVA: 0x0008D5FB File Offset: 0x0008B7FB
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			protected override DataRow NewRowFromBuilder(DataRowBuilder builder)
			{
				return new OptimizerDependencyDataSet.OptimizerDependenciesRow(builder);
			}

			// Token: 0x06002C2B RID: 11307 RVA: 0x0008D603 File Offset: 0x0008B803
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			protected override Type GetRowType()
			{
				return typeof(OptimizerDependencyDataSet.OptimizerDependenciesRow);
			}

			// Token: 0x06002C2C RID: 11308 RVA: 0x0008D60F File Offset: 0x0008B80F
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			protected override void OnRowChanged(DataRowChangeEventArgs e)
			{
				base.OnRowChanged(e);
				if (this.OptimizerDependenciesRowChanged != null)
				{
					this.OptimizerDependenciesRowChanged(this, new OptimizerDependencyDataSet.OptimizerDependenciesRowChangeEvent((OptimizerDependencyDataSet.OptimizerDependenciesRow)e.Row, e.Action));
				}
			}

			// Token: 0x06002C2D RID: 11309 RVA: 0x0008D642 File Offset: 0x0008B842
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			protected override void OnRowChanging(DataRowChangeEventArgs e)
			{
				base.OnRowChanging(e);
				if (this.OptimizerDependenciesRowChanging != null)
				{
					this.OptimizerDependenciesRowChanging(this, new OptimizerDependencyDataSet.OptimizerDependenciesRowChangeEvent((OptimizerDependencyDataSet.OptimizerDependenciesRow)e.Row, e.Action));
				}
			}

			// Token: 0x06002C2E RID: 11310 RVA: 0x0008D675 File Offset: 0x0008B875
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			protected override void OnRowDeleted(DataRowChangeEventArgs e)
			{
				base.OnRowDeleted(e);
				if (this.OptimizerDependenciesRowDeleted != null)
				{
					this.OptimizerDependenciesRowDeleted(this, new OptimizerDependencyDataSet.OptimizerDependenciesRowChangeEvent((OptimizerDependencyDataSet.OptimizerDependenciesRow)e.Row, e.Action));
				}
			}

			// Token: 0x06002C2F RID: 11311 RVA: 0x0008D6A8 File Offset: 0x0008B8A8
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			protected override void OnRowDeleting(DataRowChangeEventArgs e)
			{
				base.OnRowDeleting(e);
				if (this.OptimizerDependenciesRowDeleting != null)
				{
					this.OptimizerDependenciesRowDeleting(this, new OptimizerDependencyDataSet.OptimizerDependenciesRowChangeEvent((OptimizerDependencyDataSet.OptimizerDependenciesRow)e.Row, e.Action));
				}
			}

			// Token: 0x06002C30 RID: 11312 RVA: 0x0008D6DB File Offset: 0x0008B8DB
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public void RemoveOptimizerDependenciesRow(OptimizerDependencyDataSet.OptimizerDependenciesRow row)
			{
				base.Rows.Remove(row);
			}

			// Token: 0x06002C31 RID: 11313 RVA: 0x0008D6EC File Offset: 0x0008B8EC
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public static XmlSchemaComplexType GetTypedTableSchema(XmlSchemaSet xs)
			{
				XmlSchemaComplexType xmlSchemaComplexType = new XmlSchemaComplexType();
				XmlSchemaSequence xmlSchemaSequence = new XmlSchemaSequence();
				OptimizerDependencyDataSet optimizerDependencyDataSet = new OptimizerDependencyDataSet();
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
				xmlSchemaAttribute.FixedValue = optimizerDependencyDataSet.Namespace;
				xmlSchemaComplexType.Attributes.Add(xmlSchemaAttribute);
				XmlSchemaAttribute xmlSchemaAttribute2 = new XmlSchemaAttribute();
				xmlSchemaAttribute2.Name = "tableTypeName";
				xmlSchemaAttribute2.FixedValue = "OptimizerDependenciesDataTable";
				xmlSchemaComplexType.Attributes.Add(xmlSchemaAttribute2);
				xmlSchemaComplexType.Particle = xmlSchemaSequence;
				XmlSchema schemaSerializable = optimizerDependencyDataSet.GetSchemaSerializable();
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

			// Token: 0x04000942 RID: 2370
			private DataColumn columnDEPENDENCY_UID;

			// Token: 0x04000943 RID: 2371
			private DataColumn columnDEPENDENCY_NAME;

			// Token: 0x04000944 RID: 2372
			private DataColumn columnDEPENDENCY_TYPE;

			// Token: 0x04000945 RID: 2373
			private DataColumn columnDEPENDENCY_DESCRIPTION;

			// Token: 0x04000946 RID: 2374
			private DataColumn columnLAST_UPDATED_BY_RES_UID;

			// Token: 0x04000947 RID: 2375
			private DataColumn columnCREATED_BY_RES_UID;

			// Token: 0x04000948 RID: 2376
			private DataColumn columnCREATED_DATE;

			// Token: 0x04000949 RID: 2377
			private DataColumn columnMOD_DATE;

			// Token: 0x0400094A RID: 2378
			private DataColumn columnLAST_UPDATED_BY_RES_NAME;

			// Token: 0x0400094B RID: 2379
			private DataColumn columnCREATED_BY_RES_NAME;
		}

		// Token: 0x02000221 RID: 545
		[XmlSchemaProvider("GetTypedTableSchema")]
		[Serializable]
		public class OptimizerDependencyDetailsDataTable : DataTable, IEnumerable
		{
			// Token: 0x06002C32 RID: 11314 RVA: 0x0008D8E4 File Offset: 0x0008BAE4
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public OptimizerDependencyDetailsDataTable()
			{
				base.TableName = "OptimizerDependencyDetails";
				this.BeginInit();
				this.InitClass();
				this.EndInit();
			}

			// Token: 0x06002C33 RID: 11315 RVA: 0x0008D90C File Offset: 0x0008BB0C
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			internal OptimizerDependencyDetailsDataTable(DataTable table)
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

			// Token: 0x06002C34 RID: 11316 RVA: 0x0008D9B4 File Offset: 0x0008BBB4
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			protected OptimizerDependencyDetailsDataTable(SerializationInfo info, StreamingContext context) : base(info, context)
			{
				this.InitVars();
			}

			// Token: 0x17000CFF RID: 3327
			// (get) Token: 0x06002C35 RID: 11317 RVA: 0x0008D9C4 File Offset: 0x0008BBC4
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public DataColumn DEPENDENCY_UIDColumn
			{
				get
				{
					return this.columnDEPENDENCY_UID;
				}
			}

			// Token: 0x17000D00 RID: 3328
			// (get) Token: 0x06002C36 RID: 11318 RVA: 0x0008D9CC File Offset: 0x0008BBCC
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public DataColumn PROJ_UIDColumn
			{
				get
				{
					return this.columnPROJ_UID;
				}
			}

			// Token: 0x17000D01 RID: 3329
			// (get) Token: 0x06002C37 RID: 11319 RVA: 0x0008D9D4 File Offset: 0x0008BBD4
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public DataColumn POSITIONColumn
			{
				get
				{
					return this.columnPOSITION;
				}
			}

			// Token: 0x17000D02 RID: 3330
			// (get) Token: 0x06002C38 RID: 11320 RVA: 0x0008D9DC File Offset: 0x0008BBDC
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public DataColumn PROJ_NAMEColumn
			{
				get
				{
					return this.columnPROJ_NAME;
				}
			}

			// Token: 0x17000D03 RID: 3331
			// (get) Token: 0x06002C39 RID: 11321 RVA: 0x0008D9E4 File Offset: 0x0008BBE4
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

			// Token: 0x17000D04 RID: 3332
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public OptimizerDependencyDataSet.OptimizerDependencyDetailsRow this[int index]
			{
				get
				{
					return (OptimizerDependencyDataSet.OptimizerDependencyDetailsRow)base.Rows[index];
				}
			}

			// Token: 0x140001D1 RID: 465
			// (add) Token: 0x06002C3B RID: 11323 RVA: 0x0008DA04 File Offset: 0x0008BC04
			// (remove) Token: 0x06002C3C RID: 11324 RVA: 0x0008DA3C File Offset: 0x0008BC3C
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public event OptimizerDependencyDataSet.OptimizerDependencyDetailsRowChangeEventHandler OptimizerDependencyDetailsRowChanging;

			// Token: 0x140001D2 RID: 466
			// (add) Token: 0x06002C3D RID: 11325 RVA: 0x0008DA74 File Offset: 0x0008BC74
			// (remove) Token: 0x06002C3E RID: 11326 RVA: 0x0008DAAC File Offset: 0x0008BCAC
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public event OptimizerDependencyDataSet.OptimizerDependencyDetailsRowChangeEventHandler OptimizerDependencyDetailsRowChanged;

			// Token: 0x140001D3 RID: 467
			// (add) Token: 0x06002C3F RID: 11327 RVA: 0x0008DAE4 File Offset: 0x0008BCE4
			// (remove) Token: 0x06002C40 RID: 11328 RVA: 0x0008DB1C File Offset: 0x0008BD1C
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public event OptimizerDependencyDataSet.OptimizerDependencyDetailsRowChangeEventHandler OptimizerDependencyDetailsRowDeleting;

			// Token: 0x140001D4 RID: 468
			// (add) Token: 0x06002C41 RID: 11329 RVA: 0x0008DB54 File Offset: 0x0008BD54
			// (remove) Token: 0x06002C42 RID: 11330 RVA: 0x0008DB8C File Offset: 0x0008BD8C
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public event OptimizerDependencyDataSet.OptimizerDependencyDetailsRowChangeEventHandler OptimizerDependencyDetailsRowDeleted;

			// Token: 0x06002C43 RID: 11331 RVA: 0x0008DBC1 File Offset: 0x0008BDC1
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			public void AddOptimizerDependencyDetailsRow(OptimizerDependencyDataSet.OptimizerDependencyDetailsRow row)
			{
				base.Rows.Add(row);
			}

			// Token: 0x06002C44 RID: 11332 RVA: 0x0008DBD0 File Offset: 0x0008BDD0
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public OptimizerDependencyDataSet.OptimizerDependencyDetailsRow AddOptimizerDependencyDetailsRow(OptimizerDependencyDataSet.OptimizerDependenciesRow parentOptimizerDependenciesRowByFK_OptimizerDependencies_OptimizerDependencyDetails, Guid PROJ_UID, int POSITION, string PROJ_NAME)
			{
				OptimizerDependencyDataSet.OptimizerDependencyDetailsRow optimizerDependencyDetailsRow = (OptimizerDependencyDataSet.OptimizerDependencyDetailsRow)base.NewRow();
				object[] array = new object[]
				{
					null,
					PROJ_UID,
					POSITION,
					PROJ_NAME
				};
				if (parentOptimizerDependenciesRowByFK_OptimizerDependencies_OptimizerDependencyDetails != null)
				{
					array[0] = parentOptimizerDependenciesRowByFK_OptimizerDependencies_OptimizerDependencyDetails[0];
				}
				optimizerDependencyDetailsRow.ItemArray = array;
				base.Rows.Add(optimizerDependencyDetailsRow);
				return optimizerDependencyDetailsRow;
			}

			// Token: 0x06002C45 RID: 11333 RVA: 0x0008DC2C File Offset: 0x0008BE2C
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			public OptimizerDependencyDataSet.OptimizerDependencyDetailsRow FindByDEPENDENCY_UIDPROJ_UID(Guid DEPENDENCY_UID, Guid PROJ_UID)
			{
				return (OptimizerDependencyDataSet.OptimizerDependencyDetailsRow)base.Rows.Find(new object[]
				{
					DEPENDENCY_UID,
					PROJ_UID
				});
			}

			// Token: 0x06002C46 RID: 11334 RVA: 0x0008DC63 File Offset: 0x0008BE63
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public virtual IEnumerator GetEnumerator()
			{
				return base.Rows.GetEnumerator();
			}

			// Token: 0x06002C47 RID: 11335 RVA: 0x0008DC70 File Offset: 0x0008BE70
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			public override DataTable Clone()
			{
				OptimizerDependencyDataSet.OptimizerDependencyDetailsDataTable optimizerDependencyDetailsDataTable = (OptimizerDependencyDataSet.OptimizerDependencyDetailsDataTable)base.Clone();
				optimizerDependencyDetailsDataTable.InitVars();
				return optimizerDependencyDetailsDataTable;
			}

			// Token: 0x06002C48 RID: 11336 RVA: 0x0008DC90 File Offset: 0x0008BE90
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			protected override DataTable CreateInstance()
			{
				return new OptimizerDependencyDataSet.OptimizerDependencyDetailsDataTable();
			}

			// Token: 0x06002C49 RID: 11337 RVA: 0x0008DC98 File Offset: 0x0008BE98
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			internal void InitVars()
			{
				this.columnDEPENDENCY_UID = base.Columns["DEPENDENCY_UID"];
				this.columnPROJ_UID = base.Columns["PROJ_UID"];
				this.columnPOSITION = base.Columns["POSITION"];
				this.columnPROJ_NAME = base.Columns["PROJ_NAME"];
			}

			// Token: 0x06002C4A RID: 11338 RVA: 0x0008DD00 File Offset: 0x0008BF00
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			private void InitClass()
			{
				this.columnDEPENDENCY_UID = new DataColumn("DEPENDENCY_UID", typeof(Guid), null, MappingType.Element);
				base.Columns.Add(this.columnDEPENDENCY_UID);
				this.columnPROJ_UID = new DataColumn("PROJ_UID", typeof(Guid), null, MappingType.Element);
				base.Columns.Add(this.columnPROJ_UID);
				this.columnPOSITION = new DataColumn("POSITION", typeof(int), null, MappingType.Element);
				base.Columns.Add(this.columnPOSITION);
				this.columnPROJ_NAME = new DataColumn("PROJ_NAME", typeof(string), null, MappingType.Element);
				base.Columns.Add(this.columnPROJ_NAME);
				base.Constraints.Add(new UniqueConstraint("Constraint1", new DataColumn[]
				{
					this.columnDEPENDENCY_UID,
					this.columnPROJ_UID
				}, true));
				this.columnDEPENDENCY_UID.AllowDBNull = false;
				this.columnPROJ_UID.AllowDBNull = false;
				this.columnPROJ_NAME.ReadOnly = true;
			}

			// Token: 0x06002C4B RID: 11339 RVA: 0x0008DE15 File Offset: 0x0008C015
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public OptimizerDependencyDataSet.OptimizerDependencyDetailsRow NewOptimizerDependencyDetailsRow()
			{
				return (OptimizerDependencyDataSet.OptimizerDependencyDetailsRow)base.NewRow();
			}

			// Token: 0x06002C4C RID: 11340 RVA: 0x0008DE22 File Offset: 0x0008C022
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			protected override DataRow NewRowFromBuilder(DataRowBuilder builder)
			{
				return new OptimizerDependencyDataSet.OptimizerDependencyDetailsRow(builder);
			}

			// Token: 0x06002C4D RID: 11341 RVA: 0x0008DE2A File Offset: 0x0008C02A
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			protected override Type GetRowType()
			{
				return typeof(OptimizerDependencyDataSet.OptimizerDependencyDetailsRow);
			}

			// Token: 0x06002C4E RID: 11342 RVA: 0x0008DE36 File Offset: 0x0008C036
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			protected override void OnRowChanged(DataRowChangeEventArgs e)
			{
				base.OnRowChanged(e);
				if (this.OptimizerDependencyDetailsRowChanged != null)
				{
					this.OptimizerDependencyDetailsRowChanged(this, new OptimizerDependencyDataSet.OptimizerDependencyDetailsRowChangeEvent((OptimizerDependencyDataSet.OptimizerDependencyDetailsRow)e.Row, e.Action));
				}
			}

			// Token: 0x06002C4F RID: 11343 RVA: 0x0008DE69 File Offset: 0x0008C069
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			protected override void OnRowChanging(DataRowChangeEventArgs e)
			{
				base.OnRowChanging(e);
				if (this.OptimizerDependencyDetailsRowChanging != null)
				{
					this.OptimizerDependencyDetailsRowChanging(this, new OptimizerDependencyDataSet.OptimizerDependencyDetailsRowChangeEvent((OptimizerDependencyDataSet.OptimizerDependencyDetailsRow)e.Row, e.Action));
				}
			}

			// Token: 0x06002C50 RID: 11344 RVA: 0x0008DE9C File Offset: 0x0008C09C
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			protected override void OnRowDeleted(DataRowChangeEventArgs e)
			{
				base.OnRowDeleted(e);
				if (this.OptimizerDependencyDetailsRowDeleted != null)
				{
					this.OptimizerDependencyDetailsRowDeleted(this, new OptimizerDependencyDataSet.OptimizerDependencyDetailsRowChangeEvent((OptimizerDependencyDataSet.OptimizerDependencyDetailsRow)e.Row, e.Action));
				}
			}

			// Token: 0x06002C51 RID: 11345 RVA: 0x0008DECF File Offset: 0x0008C0CF
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			protected override void OnRowDeleting(DataRowChangeEventArgs e)
			{
				base.OnRowDeleting(e);
				if (this.OptimizerDependencyDetailsRowDeleting != null)
				{
					this.OptimizerDependencyDetailsRowDeleting(this, new OptimizerDependencyDataSet.OptimizerDependencyDetailsRowChangeEvent((OptimizerDependencyDataSet.OptimizerDependencyDetailsRow)e.Row, e.Action));
				}
			}

			// Token: 0x06002C52 RID: 11346 RVA: 0x0008DF02 File Offset: 0x0008C102
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			public void RemoveOptimizerDependencyDetailsRow(OptimizerDependencyDataSet.OptimizerDependencyDetailsRow row)
			{
				base.Rows.Remove(row);
			}

			// Token: 0x06002C53 RID: 11347 RVA: 0x0008DF10 File Offset: 0x0008C110
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			public static XmlSchemaComplexType GetTypedTableSchema(XmlSchemaSet xs)
			{
				XmlSchemaComplexType xmlSchemaComplexType = new XmlSchemaComplexType();
				XmlSchemaSequence xmlSchemaSequence = new XmlSchemaSequence();
				OptimizerDependencyDataSet optimizerDependencyDataSet = new OptimizerDependencyDataSet();
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
				xmlSchemaAttribute.FixedValue = optimizerDependencyDataSet.Namespace;
				xmlSchemaComplexType.Attributes.Add(xmlSchemaAttribute);
				XmlSchemaAttribute xmlSchemaAttribute2 = new XmlSchemaAttribute();
				xmlSchemaAttribute2.Name = "tableTypeName";
				xmlSchemaAttribute2.FixedValue = "OptimizerDependencyDetailsDataTable";
				xmlSchemaComplexType.Attributes.Add(xmlSchemaAttribute2);
				xmlSchemaComplexType.Particle = xmlSchemaSequence;
				XmlSchema schemaSerializable = optimizerDependencyDataSet.GetSchemaSerializable();
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

			// Token: 0x04000950 RID: 2384
			private DataColumn columnDEPENDENCY_UID;

			// Token: 0x04000951 RID: 2385
			private DataColumn columnPROJ_UID;

			// Token: 0x04000952 RID: 2386
			private DataColumn columnPOSITION;

			// Token: 0x04000953 RID: 2387
			private DataColumn columnPROJ_NAME;
		}

		// Token: 0x02000222 RID: 546
		public class OptimizerDependenciesRow : DataRow
		{
			// Token: 0x06002C54 RID: 11348 RVA: 0x0008E108 File Offset: 0x0008C308
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			internal OptimizerDependenciesRow(DataRowBuilder rb) : base(rb)
			{
				this.tableOptimizerDependencies = (OptimizerDependencyDataSet.OptimizerDependenciesDataTable)base.Table;
			}

			// Token: 0x17000D05 RID: 3333
			// (get) Token: 0x06002C55 RID: 11349 RVA: 0x0008E122 File Offset: 0x0008C322
			// (set) Token: 0x06002C56 RID: 11350 RVA: 0x0008E13A File Offset: 0x0008C33A
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			public Guid DEPENDENCY_UID
			{
				get
				{
					return (Guid)base[this.tableOptimizerDependencies.DEPENDENCY_UIDColumn];
				}
				set
				{
					base[this.tableOptimizerDependencies.DEPENDENCY_UIDColumn] = value;
				}
			}

			// Token: 0x17000D06 RID: 3334
			// (get) Token: 0x06002C57 RID: 11351 RVA: 0x0008E153 File Offset: 0x0008C353
			// (set) Token: 0x06002C58 RID: 11352 RVA: 0x0008E16B File Offset: 0x0008C36B
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			public string DEPENDENCY_NAME
			{
				get
				{
					return (string)base[this.tableOptimizerDependencies.DEPENDENCY_NAMEColumn];
				}
				set
				{
					base[this.tableOptimizerDependencies.DEPENDENCY_NAMEColumn] = value;
				}
			}

			// Token: 0x17000D07 RID: 3335
			// (get) Token: 0x06002C59 RID: 11353 RVA: 0x0008E17F File Offset: 0x0008C37F
			// (set) Token: 0x06002C5A RID: 11354 RVA: 0x0008E197 File Offset: 0x0008C397
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			public int DEPENDENCY_TYPE
			{
				get
				{
					return (int)base[this.tableOptimizerDependencies.DEPENDENCY_TYPEColumn];
				}
				set
				{
					base[this.tableOptimizerDependencies.DEPENDENCY_TYPEColumn] = value;
				}
			}

			// Token: 0x17000D08 RID: 3336
			// (get) Token: 0x06002C5B RID: 11355 RVA: 0x0008E1B0 File Offset: 0x0008C3B0
			// (set) Token: 0x06002C5C RID: 11356 RVA: 0x0008E1F4 File Offset: 0x0008C3F4
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			public string DEPENDENCY_DESCRIPTION
			{
				get
				{
					string result;
					try
					{
						result = (string)base[this.tableOptimizerDependencies.DEPENDENCY_DESCRIPTIONColumn];
					}
					catch (InvalidCastException innerException)
					{
						throw new StrongTypingException("The value for column 'DEPENDENCY_DESCRIPTION' in table 'OptimizerDependencies' is DBNull.", innerException);
					}
					return result;
				}
				set
				{
					base[this.tableOptimizerDependencies.DEPENDENCY_DESCRIPTIONColumn] = value;
				}
			}

			// Token: 0x17000D09 RID: 3337
			// (get) Token: 0x06002C5D RID: 11357 RVA: 0x0008E208 File Offset: 0x0008C408
			// (set) Token: 0x06002C5E RID: 11358 RVA: 0x0008E24C File Offset: 0x0008C44C
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public Guid LAST_UPDATED_BY_RES_UID
			{
				get
				{
					Guid result;
					try
					{
						result = (Guid)base[this.tableOptimizerDependencies.LAST_UPDATED_BY_RES_UIDColumn];
					}
					catch (InvalidCastException innerException)
					{
						throw new StrongTypingException("The value for column 'LAST_UPDATED_BY_RES_UID' in table 'OptimizerDependencies' is DBNull.", innerException);
					}
					return result;
				}
				set
				{
					base[this.tableOptimizerDependencies.LAST_UPDATED_BY_RES_UIDColumn] = value;
				}
			}

			// Token: 0x17000D0A RID: 3338
			// (get) Token: 0x06002C5F RID: 11359 RVA: 0x0008E268 File Offset: 0x0008C468
			// (set) Token: 0x06002C60 RID: 11360 RVA: 0x0008E2AC File Offset: 0x0008C4AC
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			public Guid CREATED_BY_RES_UID
			{
				get
				{
					Guid result;
					try
					{
						result = (Guid)base[this.tableOptimizerDependencies.CREATED_BY_RES_UIDColumn];
					}
					catch (InvalidCastException innerException)
					{
						throw new StrongTypingException("The value for column 'CREATED_BY_RES_UID' in table 'OptimizerDependencies' is DBNull.", innerException);
					}
					return result;
				}
				set
				{
					base[this.tableOptimizerDependencies.CREATED_BY_RES_UIDColumn] = value;
				}
			}

			// Token: 0x17000D0B RID: 3339
			// (get) Token: 0x06002C61 RID: 11361 RVA: 0x0008E2C8 File Offset: 0x0008C4C8
			// (set) Token: 0x06002C62 RID: 11362 RVA: 0x0008E30C File Offset: 0x0008C50C
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public DateTime CREATED_DATE
			{
				get
				{
					DateTime result;
					try
					{
						result = (DateTime)base[this.tableOptimizerDependencies.CREATED_DATEColumn];
					}
					catch (InvalidCastException innerException)
					{
						throw new StrongTypingException("The value for column 'CREATED_DATE' in table 'OptimizerDependencies' is DBNull.", innerException);
					}
					return result;
				}
				set
				{
					base[this.tableOptimizerDependencies.CREATED_DATEColumn] = value;
				}
			}

			// Token: 0x17000D0C RID: 3340
			// (get) Token: 0x06002C63 RID: 11363 RVA: 0x0008E328 File Offset: 0x0008C528
			// (set) Token: 0x06002C64 RID: 11364 RVA: 0x0008E36C File Offset: 0x0008C56C
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public DateTime MOD_DATE
			{
				get
				{
					DateTime result;
					try
					{
						result = (DateTime)base[this.tableOptimizerDependencies.MOD_DATEColumn];
					}
					catch (InvalidCastException innerException)
					{
						throw new StrongTypingException("The value for column 'MOD_DATE' in table 'OptimizerDependencies' is DBNull.", innerException);
					}
					return result;
				}
				set
				{
					base[this.tableOptimizerDependencies.MOD_DATEColumn] = value;
				}
			}

			// Token: 0x17000D0D RID: 3341
			// (get) Token: 0x06002C65 RID: 11365 RVA: 0x0008E388 File Offset: 0x0008C588
			// (set) Token: 0x06002C66 RID: 11366 RVA: 0x0008E3CC File Offset: 0x0008C5CC
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			public string LAST_UPDATED_BY_RES_NAME
			{
				get
				{
					string result;
					try
					{
						result = (string)base[this.tableOptimizerDependencies.LAST_UPDATED_BY_RES_NAMEColumn];
					}
					catch (InvalidCastException innerException)
					{
						throw new StrongTypingException("The value for column 'LAST_UPDATED_BY_RES_NAME' in table 'OptimizerDependencies' is DBNull.", innerException);
					}
					return result;
				}
				set
				{
					base[this.tableOptimizerDependencies.LAST_UPDATED_BY_RES_NAMEColumn] = value;
				}
			}

			// Token: 0x17000D0E RID: 3342
			// (get) Token: 0x06002C67 RID: 11367 RVA: 0x0008E3E0 File Offset: 0x0008C5E0
			// (set) Token: 0x06002C68 RID: 11368 RVA: 0x0008E424 File Offset: 0x0008C624
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public string CREATED_BY_RES_NAME
			{
				get
				{
					string result;
					try
					{
						result = (string)base[this.tableOptimizerDependencies.CREATED_BY_RES_NAMEColumn];
					}
					catch (InvalidCastException innerException)
					{
						throw new StrongTypingException("The value for column 'CREATED_BY_RES_NAME' in table 'OptimizerDependencies' is DBNull.", innerException);
					}
					return result;
				}
				set
				{
					base[this.tableOptimizerDependencies.CREATED_BY_RES_NAMEColumn] = value;
				}
			}

			// Token: 0x06002C69 RID: 11369 RVA: 0x0008E438 File Offset: 0x0008C638
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			public bool IsDEPENDENCY_DESCRIPTIONNull()
			{
				return base.IsNull(this.tableOptimizerDependencies.DEPENDENCY_DESCRIPTIONColumn);
			}

			// Token: 0x06002C6A RID: 11370 RVA: 0x0008E44B File Offset: 0x0008C64B
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			public void SetDEPENDENCY_DESCRIPTIONNull()
			{
				base[this.tableOptimizerDependencies.DEPENDENCY_DESCRIPTIONColumn] = Convert.DBNull;
			}

			// Token: 0x06002C6B RID: 11371 RVA: 0x0008E463 File Offset: 0x0008C663
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			public bool IsLAST_UPDATED_BY_RES_UIDNull()
			{
				return base.IsNull(this.tableOptimizerDependencies.LAST_UPDATED_BY_RES_UIDColumn);
			}

			// Token: 0x06002C6C RID: 11372 RVA: 0x0008E476 File Offset: 0x0008C676
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			public void SetLAST_UPDATED_BY_RES_UIDNull()
			{
				base[this.tableOptimizerDependencies.LAST_UPDATED_BY_RES_UIDColumn] = Convert.DBNull;
			}

			// Token: 0x06002C6D RID: 11373 RVA: 0x0008E48E File Offset: 0x0008C68E
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public bool IsCREATED_BY_RES_UIDNull()
			{
				return base.IsNull(this.tableOptimizerDependencies.CREATED_BY_RES_UIDColumn);
			}

			// Token: 0x06002C6E RID: 11374 RVA: 0x0008E4A1 File Offset: 0x0008C6A1
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public void SetCREATED_BY_RES_UIDNull()
			{
				base[this.tableOptimizerDependencies.CREATED_BY_RES_UIDColumn] = Convert.DBNull;
			}

			// Token: 0x06002C6F RID: 11375 RVA: 0x0008E4B9 File Offset: 0x0008C6B9
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			public bool IsCREATED_DATENull()
			{
				return base.IsNull(this.tableOptimizerDependencies.CREATED_DATEColumn);
			}

			// Token: 0x06002C70 RID: 11376 RVA: 0x0008E4CC File Offset: 0x0008C6CC
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public void SetCREATED_DATENull()
			{
				base[this.tableOptimizerDependencies.CREATED_DATEColumn] = Convert.DBNull;
			}

			// Token: 0x06002C71 RID: 11377 RVA: 0x0008E4E4 File Offset: 0x0008C6E4
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			public bool IsMOD_DATENull()
			{
				return base.IsNull(this.tableOptimizerDependencies.MOD_DATEColumn);
			}

			// Token: 0x06002C72 RID: 11378 RVA: 0x0008E4F7 File Offset: 0x0008C6F7
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			public void SetMOD_DATENull()
			{
				base[this.tableOptimizerDependencies.MOD_DATEColumn] = Convert.DBNull;
			}

			// Token: 0x06002C73 RID: 11379 RVA: 0x0008E50F File Offset: 0x0008C70F
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			public bool IsLAST_UPDATED_BY_RES_NAMENull()
			{
				return base.IsNull(this.tableOptimizerDependencies.LAST_UPDATED_BY_RES_NAMEColumn);
			}

			// Token: 0x06002C74 RID: 11380 RVA: 0x0008E522 File Offset: 0x0008C722
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public void SetLAST_UPDATED_BY_RES_NAMENull()
			{
				base[this.tableOptimizerDependencies.LAST_UPDATED_BY_RES_NAMEColumn] = Convert.DBNull;
			}

			// Token: 0x06002C75 RID: 11381 RVA: 0x0008E53A File Offset: 0x0008C73A
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			public bool IsCREATED_BY_RES_NAMENull()
			{
				return base.IsNull(this.tableOptimizerDependencies.CREATED_BY_RES_NAMEColumn);
			}

			// Token: 0x06002C76 RID: 11382 RVA: 0x0008E54D File Offset: 0x0008C74D
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			public void SetCREATED_BY_RES_NAMENull()
			{
				base[this.tableOptimizerDependencies.CREATED_BY_RES_NAMEColumn] = Convert.DBNull;
			}

			// Token: 0x06002C77 RID: 11383 RVA: 0x0008E565 File Offset: 0x0008C765
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public OptimizerDependencyDataSet.OptimizerDependencyDetailsRow[] GetOptimizerDependencyDetailsRows()
			{
				if (base.Table.ChildRelations["FK_OptimizerDependencies_OptimizerDependencyDetails"] == null)
				{
					return new OptimizerDependencyDataSet.OptimizerDependencyDetailsRow[0];
				}
				return (OptimizerDependencyDataSet.OptimizerDependencyDetailsRow[])base.GetChildRows(base.Table.ChildRelations["FK_OptimizerDependencies_OptimizerDependencyDetails"]);
			}

			// Token: 0x04000958 RID: 2392
			private OptimizerDependencyDataSet.OptimizerDependenciesDataTable tableOptimizerDependencies;
		}

		// Token: 0x02000223 RID: 547
		public class OptimizerDependencyDetailsRow : DataRow
		{
			// Token: 0x06002C78 RID: 11384 RVA: 0x0008E5A5 File Offset: 0x0008C7A5
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			internal OptimizerDependencyDetailsRow(DataRowBuilder rb) : base(rb)
			{
				this.tableOptimizerDependencyDetails = (OptimizerDependencyDataSet.OptimizerDependencyDetailsDataTable)base.Table;
			}

			// Token: 0x17000D0F RID: 3343
			// (get) Token: 0x06002C79 RID: 11385 RVA: 0x0008E5BF File Offset: 0x0008C7BF
			// (set) Token: 0x06002C7A RID: 11386 RVA: 0x0008E5D7 File Offset: 0x0008C7D7
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			public Guid DEPENDENCY_UID
			{
				get
				{
					return (Guid)base[this.tableOptimizerDependencyDetails.DEPENDENCY_UIDColumn];
				}
				set
				{
					base[this.tableOptimizerDependencyDetails.DEPENDENCY_UIDColumn] = value;
				}
			}

			// Token: 0x17000D10 RID: 3344
			// (get) Token: 0x06002C7B RID: 11387 RVA: 0x0008E5F0 File Offset: 0x0008C7F0
			// (set) Token: 0x06002C7C RID: 11388 RVA: 0x0008E608 File Offset: 0x0008C808
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public Guid PROJ_UID
			{
				get
				{
					return (Guid)base[this.tableOptimizerDependencyDetails.PROJ_UIDColumn];
				}
				set
				{
					base[this.tableOptimizerDependencyDetails.PROJ_UIDColumn] = value;
				}
			}

			// Token: 0x17000D11 RID: 3345
			// (get) Token: 0x06002C7D RID: 11389 RVA: 0x0008E624 File Offset: 0x0008C824
			// (set) Token: 0x06002C7E RID: 11390 RVA: 0x0008E668 File Offset: 0x0008C868
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			public int POSITION
			{
				get
				{
					int result;
					try
					{
						result = (int)base[this.tableOptimizerDependencyDetails.POSITIONColumn];
					}
					catch (InvalidCastException innerException)
					{
						throw new StrongTypingException("The value for column 'POSITION' in table 'OptimizerDependencyDetails' is DBNull.", innerException);
					}
					return result;
				}
				set
				{
					base[this.tableOptimizerDependencyDetails.POSITIONColumn] = value;
				}
			}

			// Token: 0x17000D12 RID: 3346
			// (get) Token: 0x06002C7F RID: 11391 RVA: 0x0008E684 File Offset: 0x0008C884
			// (set) Token: 0x06002C80 RID: 11392 RVA: 0x0008E6C8 File Offset: 0x0008C8C8
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public string PROJ_NAME
			{
				get
				{
					string result;
					try
					{
						result = (string)base[this.tableOptimizerDependencyDetails.PROJ_NAMEColumn];
					}
					catch (InvalidCastException innerException)
					{
						throw new StrongTypingException("The value for column 'PROJ_NAME' in table 'OptimizerDependencyDetails' is DBNull.", innerException);
					}
					return result;
				}
				set
				{
					base[this.tableOptimizerDependencyDetails.PROJ_NAMEColumn] = value;
				}
			}

			// Token: 0x17000D13 RID: 3347
			// (get) Token: 0x06002C81 RID: 11393 RVA: 0x0008E6DC File Offset: 0x0008C8DC
			// (set) Token: 0x06002C82 RID: 11394 RVA: 0x0008E6FE File Offset: 0x0008C8FE
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			public OptimizerDependencyDataSet.OptimizerDependenciesRow OptimizerDependenciesRow
			{
				get
				{
					return (OptimizerDependencyDataSet.OptimizerDependenciesRow)base.GetParentRow(base.Table.ParentRelations["FK_OptimizerDependencies_OptimizerDependencyDetails"]);
				}
				set
				{
					base.SetParentRow(value, base.Table.ParentRelations["FK_OptimizerDependencies_OptimizerDependencyDetails"]);
				}
			}

			// Token: 0x06002C83 RID: 11395 RVA: 0x0008E71C File Offset: 0x0008C91C
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public bool IsPOSITIONNull()
			{
				return base.IsNull(this.tableOptimizerDependencyDetails.POSITIONColumn);
			}

			// Token: 0x06002C84 RID: 11396 RVA: 0x0008E72F File Offset: 0x0008C92F
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public void SetPOSITIONNull()
			{
				base[this.tableOptimizerDependencyDetails.POSITIONColumn] = Convert.DBNull;
			}

			// Token: 0x06002C85 RID: 11397 RVA: 0x0008E747 File Offset: 0x0008C947
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			public bool IsPROJ_NAMENull()
			{
				return base.IsNull(this.tableOptimizerDependencyDetails.PROJ_NAMEColumn);
			}

			// Token: 0x06002C86 RID: 11398 RVA: 0x0008E75A File Offset: 0x0008C95A
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			public void SetPROJ_NAMENull()
			{
				base[this.tableOptimizerDependencyDetails.PROJ_NAMEColumn] = Convert.DBNull;
			}

			// Token: 0x04000959 RID: 2393
			private OptimizerDependencyDataSet.OptimizerDependencyDetailsDataTable tableOptimizerDependencyDetails;
		}

		// Token: 0x02000224 RID: 548
		[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
		public class OptimizerDependenciesRowChangeEvent : EventArgs
		{
			// Token: 0x06002C87 RID: 11399 RVA: 0x0008E772 File Offset: 0x0008C972
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public OptimizerDependenciesRowChangeEvent(OptimizerDependencyDataSet.OptimizerDependenciesRow row, DataRowAction action)
			{
				this.eventRow = row;
				this.eventAction = action;
			}

			// Token: 0x17000D14 RID: 3348
			// (get) Token: 0x06002C88 RID: 11400 RVA: 0x0008E788 File Offset: 0x0008C988
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public OptimizerDependencyDataSet.OptimizerDependenciesRow Row
			{
				get
				{
					return this.eventRow;
				}
			}

			// Token: 0x17000D15 RID: 3349
			// (get) Token: 0x06002C89 RID: 11401 RVA: 0x0008E790 File Offset: 0x0008C990
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			public DataRowAction Action
			{
				get
				{
					return this.eventAction;
				}
			}

			// Token: 0x0400095A RID: 2394
			private OptimizerDependencyDataSet.OptimizerDependenciesRow eventRow;

			// Token: 0x0400095B RID: 2395
			private DataRowAction eventAction;
		}

		// Token: 0x02000225 RID: 549
		[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
		public class OptimizerDependencyDetailsRowChangeEvent : EventArgs
		{
			// Token: 0x06002C8A RID: 11402 RVA: 0x0008E798 File Offset: 0x0008C998
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			public OptimizerDependencyDetailsRowChangeEvent(OptimizerDependencyDataSet.OptimizerDependencyDetailsRow row, DataRowAction action)
			{
				this.eventRow = row;
				this.eventAction = action;
			}

			// Token: 0x17000D16 RID: 3350
			// (get) Token: 0x06002C8B RID: 11403 RVA: 0x0008E7AE File Offset: 0x0008C9AE
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			public OptimizerDependencyDataSet.OptimizerDependencyDetailsRow Row
			{
				get
				{
					return this.eventRow;
				}
			}

			// Token: 0x17000D17 RID: 3351
			// (get) Token: 0x06002C8C RID: 11404 RVA: 0x0008E7B6 File Offset: 0x0008C9B6
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public DataRowAction Action
			{
				get
				{
					return this.eventAction;
				}
			}

			// Token: 0x0400095C RID: 2396
			private OptimizerDependencyDataSet.OptimizerDependencyDetailsRow eventRow;

			// Token: 0x0400095D RID: 2397
			private DataRowAction eventAction;
		}
	}
}
