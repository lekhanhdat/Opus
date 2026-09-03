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
	// Token: 0x0200062D RID: 1581
	[DesignerCategory("code")]
	[XmlRoot("SecurityTemplatesDataSet")]
	[HelpKeyword("vs.data.DataSet")]
	[XmlSchemaProvider("GetTypedDataSetSchema")]
	[ToolboxItem(true)]
	[Serializable]
	public class SecurityTemplatesDataSet : DataSet
	{
		// Token: 0x060092D1 RID: 37585 RVA: 0x001CBC98 File Offset: 0x001C9E98
		public new void EndInit()
		{
			base.EndInit();
			TypedDataSetUtilities.AllowNullsInNonTypedColumns(this.SecurityTemplates, new string[]
			{
				"WSEC_TMPL_NAME",
				"WSEC_TMPL_UID",
				"WSEC_TMPL_DESC"
			});
			TypedDataSetUtilities.AllowNullsInNonTypedColumns(this.GlobalPermissions, new string[]
			{
				"WSEC_DENY",
				"WSEC_TMPL_UID",
				"WSEC_ALLOW",
				"WSEC_FEA_ACT_UID"
			});
			TypedDataSetUtilities.AllowNullsInNonTypedColumns(this.CategoryPermissions, new string[]
			{
				"WSEC_DENY",
				"WSEC_TMPL_UID",
				"WSEC_ALLOW",
				"WSEC_FEA_ACT_UID"
			});
		}

		// Token: 0x060092D2 RID: 37586 RVA: 0x001CBD3C File Offset: 0x001C9F3C
		[DebuggerNonUserCode]
		[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
		public SecurityTemplatesDataSet()
		{
			base.BeginInit();
			this.InitClass();
			CollectionChangeEventHandler value = new CollectionChangeEventHandler(this.SchemaChanged);
			base.Tables.CollectionChanged += value;
			base.Relations.CollectionChanged += value;
			this.EndInit();
		}

		// Token: 0x060092D3 RID: 37587 RVA: 0x001CBD90 File Offset: 0x001C9F90
		[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
		[DebuggerNonUserCode]
		protected SecurityTemplatesDataSet(SerializationInfo info, StreamingContext context) : base(info, context, false)
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
				if (dataSet.Tables["SecurityTemplates"] != null)
				{
					base.Tables.Add(new SecurityTemplatesDataSet.SecurityTemplatesDataTable(dataSet.Tables["SecurityTemplates"]));
				}
				if (dataSet.Tables["CategoryPermissions"] != null)
				{
					base.Tables.Add(new SecurityTemplatesDataSet.CategoryPermissionsDataTable(dataSet.Tables["CategoryPermissions"]));
				}
				if (dataSet.Tables["GlobalPermissions"] != null)
				{
					base.Tables.Add(new SecurityTemplatesDataSet.GlobalPermissionsDataTable(dataSet.Tables["GlobalPermissions"]));
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

		// Token: 0x17002C0C RID: 11276
		// (get) Token: 0x060092D4 RID: 37588 RVA: 0x001CBF51 File Offset: 0x001CA151
		[DebuggerNonUserCode]
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Content)]
		[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
		[Browsable(false)]
		public SecurityTemplatesDataSet.SecurityTemplatesDataTable SecurityTemplates
		{
			get
			{
				return this.tableSecurityTemplates;
			}
		}

		// Token: 0x17002C0D RID: 11277
		// (get) Token: 0x060092D5 RID: 37589 RVA: 0x001CBF59 File Offset: 0x001CA159
		[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Content)]
		[Browsable(false)]
		[DebuggerNonUserCode]
		public SecurityTemplatesDataSet.CategoryPermissionsDataTable CategoryPermissions
		{
			get
			{
				return this.tableCategoryPermissions;
			}
		}

		// Token: 0x17002C0E RID: 11278
		// (get) Token: 0x060092D6 RID: 37590 RVA: 0x001CBF61 File Offset: 0x001CA161
		[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
		[DebuggerNonUserCode]
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Content)]
		[Browsable(false)]
		public SecurityTemplatesDataSet.GlobalPermissionsDataTable GlobalPermissions
		{
			get
			{
				return this.tableGlobalPermissions;
			}
		}

		// Token: 0x17002C0F RID: 11279
		// (get) Token: 0x060092D7 RID: 37591 RVA: 0x001CBF69 File Offset: 0x001CA169
		// (set) Token: 0x060092D8 RID: 37592 RVA: 0x001CBF71 File Offset: 0x001CA171
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

		// Token: 0x17002C10 RID: 11280
		// (get) Token: 0x060092D9 RID: 37593 RVA: 0x001CBF7A File Offset: 0x001CA17A
		[DebuggerNonUserCode]
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
		[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
		public new DataTableCollection Tables
		{
			get
			{
				return base.Tables;
			}
		}

		// Token: 0x17002C11 RID: 11281
		// (get) Token: 0x060092DA RID: 37594 RVA: 0x001CBF82 File Offset: 0x001CA182
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

		// Token: 0x060092DB RID: 37595 RVA: 0x001CBF8A File Offset: 0x001CA18A
		[DebuggerNonUserCode]
		[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
		protected override void InitializeDerivedDataSet()
		{
			base.BeginInit();
			this.InitClass();
			this.EndInit();
		}

		// Token: 0x060092DC RID: 37596 RVA: 0x001CBFA0 File Offset: 0x001CA1A0
		[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
		[DebuggerNonUserCode]
		public override DataSet Clone()
		{
			SecurityTemplatesDataSet securityTemplatesDataSet = (SecurityTemplatesDataSet)base.Clone();
			securityTemplatesDataSet.InitVars();
			securityTemplatesDataSet.SchemaSerializationMode = this.SchemaSerializationMode;
			return securityTemplatesDataSet;
		}

		// Token: 0x060092DD RID: 37597 RVA: 0x001CBFCC File Offset: 0x001CA1CC
		[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
		[DebuggerNonUserCode]
		protected override bool ShouldSerializeTables()
		{
			return false;
		}

		// Token: 0x060092DE RID: 37598 RVA: 0x001CBFCF File Offset: 0x001CA1CF
		[DebuggerNonUserCode]
		[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
		protected override bool ShouldSerializeRelations()
		{
			return false;
		}

		// Token: 0x060092DF RID: 37599 RVA: 0x001CBFD4 File Offset: 0x001CA1D4
		[DebuggerNonUserCode]
		[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
		protected override void ReadXmlSerializable(XmlReader reader)
		{
			if (base.DetermineSchemaSerializationMode(reader) == SchemaSerializationMode.IncludeSchema)
			{
				this.Reset();
				DataSet dataSet = new DataSet();
				dataSet.ReadXml(reader);
				if (dataSet.Tables["SecurityTemplates"] != null)
				{
					base.Tables.Add(new SecurityTemplatesDataSet.SecurityTemplatesDataTable(dataSet.Tables["SecurityTemplates"]));
				}
				if (dataSet.Tables["CategoryPermissions"] != null)
				{
					base.Tables.Add(new SecurityTemplatesDataSet.CategoryPermissionsDataTable(dataSet.Tables["CategoryPermissions"]));
				}
				if (dataSet.Tables["GlobalPermissions"] != null)
				{
					base.Tables.Add(new SecurityTemplatesDataSet.GlobalPermissionsDataTable(dataSet.Tables["GlobalPermissions"]));
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

		// Token: 0x060092E0 RID: 37600 RVA: 0x001CC100 File Offset: 0x001CA300
		[DebuggerNonUserCode]
		[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
		protected override XmlSchema GetSchemaSerializable()
		{
			MemoryStream memoryStream = new MemoryStream();
			base.WriteXmlSchema(new XmlTextWriter(memoryStream, null));
			memoryStream.Position = 0L;
			return XmlSchema.Read(new XmlTextReader(memoryStream), null);
		}

		// Token: 0x060092E1 RID: 37601 RVA: 0x001CC134 File Offset: 0x001CA334
		[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
		[DebuggerNonUserCode]
		internal void InitVars()
		{
			this.InitVars(true);
		}

		// Token: 0x060092E2 RID: 37602 RVA: 0x001CC140 File Offset: 0x001CA340
		[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
		[DebuggerNonUserCode]
		internal void InitVars(bool initTable)
		{
			this.tableSecurityTemplates = (SecurityTemplatesDataSet.SecurityTemplatesDataTable)base.Tables["SecurityTemplates"];
			if (initTable && this.tableSecurityTemplates != null)
			{
				this.tableSecurityTemplates.InitVars();
			}
			this.tableCategoryPermissions = (SecurityTemplatesDataSet.CategoryPermissionsDataTable)base.Tables["CategoryPermissions"];
			if (initTable && this.tableCategoryPermissions != null)
			{
				this.tableCategoryPermissions.InitVars();
			}
			this.tableGlobalPermissions = (SecurityTemplatesDataSet.GlobalPermissionsDataTable)base.Tables["GlobalPermissions"];
			if (initTable && this.tableGlobalPermissions != null)
			{
				this.tableGlobalPermissions.InitVars();
			}
		}

		// Token: 0x060092E3 RID: 37603 RVA: 0x001CC1E0 File Offset: 0x001CA3E0
		[DebuggerNonUserCode]
		[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
		private void InitClass()
		{
			base.DataSetName = "SecurityTemplatesDataSet";
			base.Prefix = "";
			base.Namespace = "http://schemas.microsoft.com/office/project/server/webservices/SecurityTemplatesDataSet/";
			base.EnforceConstraints = false;
			this.SchemaSerializationMode = SchemaSerializationMode.IncludeSchema;
			this.tableSecurityTemplates = new SecurityTemplatesDataSet.SecurityTemplatesDataTable();
			base.Tables.Add(this.tableSecurityTemplates);
			this.tableCategoryPermissions = new SecurityTemplatesDataSet.CategoryPermissionsDataTable();
			base.Tables.Add(this.tableCategoryPermissions);
			this.tableGlobalPermissions = new SecurityTemplatesDataSet.GlobalPermissionsDataTable();
			base.Tables.Add(this.tableGlobalPermissions);
		}

		// Token: 0x060092E4 RID: 37604 RVA: 0x001CC270 File Offset: 0x001CA470
		[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
		[DebuggerNonUserCode]
		private bool ShouldSerializeSecurityTemplates()
		{
			return false;
		}

		// Token: 0x060092E5 RID: 37605 RVA: 0x001CC273 File Offset: 0x001CA473
		[DebuggerNonUserCode]
		[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
		private bool ShouldSerializeCategoryPermissions()
		{
			return false;
		}

		// Token: 0x060092E6 RID: 37606 RVA: 0x001CC276 File Offset: 0x001CA476
		[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
		[DebuggerNonUserCode]
		private bool ShouldSerializeGlobalPermissions()
		{
			return false;
		}

		// Token: 0x060092E7 RID: 37607 RVA: 0x001CC279 File Offset: 0x001CA479
		[DebuggerNonUserCode]
		[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
		private void SchemaChanged(object sender, CollectionChangeEventArgs e)
		{
			if (e.Action == CollectionChangeAction.Remove)
			{
				this.InitVars();
			}
		}

		// Token: 0x060092E8 RID: 37608 RVA: 0x001CC28C File Offset: 0x001CA48C
		[DebuggerNonUserCode]
		[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
		public static XmlSchemaComplexType GetTypedDataSetSchema(XmlSchemaSet xs)
		{
			SecurityTemplatesDataSet securityTemplatesDataSet = new SecurityTemplatesDataSet();
			XmlSchemaComplexType xmlSchemaComplexType = new XmlSchemaComplexType();
			XmlSchemaSequence xmlSchemaSequence = new XmlSchemaSequence();
			XmlSchemaAny xmlSchemaAny = new XmlSchemaAny();
			xmlSchemaAny.Namespace = securityTemplatesDataSet.Namespace;
			xmlSchemaSequence.Items.Add(xmlSchemaAny);
			xmlSchemaComplexType.Particle = xmlSchemaSequence;
			XmlSchema schemaSerializable = securityTemplatesDataSet.GetSchemaSerializable();
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

		// Token: 0x04001D75 RID: 7541
		private SecurityTemplatesDataSet.SecurityTemplatesDataTable tableSecurityTemplates;

		// Token: 0x04001D76 RID: 7542
		private SecurityTemplatesDataSet.CategoryPermissionsDataTable tableCategoryPermissions;

		// Token: 0x04001D77 RID: 7543
		private SecurityTemplatesDataSet.GlobalPermissionsDataTable tableGlobalPermissions;

		// Token: 0x04001D78 RID: 7544
		private SchemaSerializationMode _schemaSerializationMode = SchemaSerializationMode.IncludeSchema;

		// Token: 0x0200062E RID: 1582
		// (Invoke) Token: 0x060092EA RID: 37610
		[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
		public delegate void SecurityTemplatesRowChangeEventHandler(object sender, SecurityTemplatesDataSet.SecurityTemplatesRowChangeEvent e);

		// Token: 0x0200062F RID: 1583
		// (Invoke) Token: 0x060092EE RID: 37614
		[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
		public delegate void CategoryPermissionsRowChangeEventHandler(object sender, SecurityTemplatesDataSet.CategoryPermissionsRowChangeEvent e);

		// Token: 0x02000630 RID: 1584
		// (Invoke) Token: 0x060092F2 RID: 37618
		[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
		public delegate void GlobalPermissionsRowChangeEventHandler(object sender, SecurityTemplatesDataSet.GlobalPermissionsRowChangeEvent e);

		// Token: 0x02000631 RID: 1585
		[XmlSchemaProvider("GetTypedTableSchema")]
		[Serializable]
		public class SecurityTemplatesDataTable : DataTable, IEnumerable
		{
			// Token: 0x060092F5 RID: 37621 RVA: 0x001CC3D4 File Offset: 0x001CA5D4
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			public SecurityTemplatesDataTable()
			{
				base.TableName = "SecurityTemplates";
				this.BeginInit();
				this.InitClass();
				this.EndInit();
			}

			// Token: 0x060092F6 RID: 37622 RVA: 0x001CC3FC File Offset: 0x001CA5FC
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			internal SecurityTemplatesDataTable(DataTable table)
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

			// Token: 0x060092F7 RID: 37623 RVA: 0x001CC4A4 File Offset: 0x001CA6A4
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			protected SecurityTemplatesDataTable(SerializationInfo info, StreamingContext context) : base(info, context)
			{
				this.InitVars();
			}

			// Token: 0x17002C12 RID: 11282
			// (get) Token: 0x060092F8 RID: 37624 RVA: 0x001CC4B4 File Offset: 0x001CA6B4
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			public DataColumn WSEC_TMPL_UIDColumn
			{
				get
				{
					return this.columnWSEC_TMPL_UID;
				}
			}

			// Token: 0x17002C13 RID: 11283
			// (get) Token: 0x060092F9 RID: 37625 RVA: 0x001CC4BC File Offset: 0x001CA6BC
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public DataColumn WSEC_TMPL_NAMEColumn
			{
				get
				{
					return this.columnWSEC_TMPL_NAME;
				}
			}

			// Token: 0x17002C14 RID: 11284
			// (get) Token: 0x060092FA RID: 37626 RVA: 0x001CC4C4 File Offset: 0x001CA6C4
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public DataColumn WSEC_TMPL_DESCColumn
			{
				get
				{
					return this.columnWSEC_TMPL_DESC;
				}
			}

			// Token: 0x17002C15 RID: 11285
			// (get) Token: 0x060092FB RID: 37627 RVA: 0x001CC4CC File Offset: 0x001CA6CC
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

			// Token: 0x17002C16 RID: 11286
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public SecurityTemplatesDataSet.SecurityTemplatesRow this[int index]
			{
				get
				{
					return (SecurityTemplatesDataSet.SecurityTemplatesRow)base.Rows[index];
				}
			}

			// Token: 0x14000559 RID: 1369
			// (add) Token: 0x060092FD RID: 37629 RVA: 0x001CC4EC File Offset: 0x001CA6EC
			// (remove) Token: 0x060092FE RID: 37630 RVA: 0x001CC524 File Offset: 0x001CA724
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public event SecurityTemplatesDataSet.SecurityTemplatesRowChangeEventHandler SecurityTemplatesRowChanging;

			// Token: 0x1400055A RID: 1370
			// (add) Token: 0x060092FF RID: 37631 RVA: 0x001CC55C File Offset: 0x001CA75C
			// (remove) Token: 0x06009300 RID: 37632 RVA: 0x001CC594 File Offset: 0x001CA794
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public event SecurityTemplatesDataSet.SecurityTemplatesRowChangeEventHandler SecurityTemplatesRowChanged;

			// Token: 0x1400055B RID: 1371
			// (add) Token: 0x06009301 RID: 37633 RVA: 0x001CC5CC File Offset: 0x001CA7CC
			// (remove) Token: 0x06009302 RID: 37634 RVA: 0x001CC604 File Offset: 0x001CA804
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public event SecurityTemplatesDataSet.SecurityTemplatesRowChangeEventHandler SecurityTemplatesRowDeleting;

			// Token: 0x1400055C RID: 1372
			// (add) Token: 0x06009303 RID: 37635 RVA: 0x001CC63C File Offset: 0x001CA83C
			// (remove) Token: 0x06009304 RID: 37636 RVA: 0x001CC674 File Offset: 0x001CA874
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public event SecurityTemplatesDataSet.SecurityTemplatesRowChangeEventHandler SecurityTemplatesRowDeleted;

			// Token: 0x06009305 RID: 37637 RVA: 0x001CC6A9 File Offset: 0x001CA8A9
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			public void AddSecurityTemplatesRow(SecurityTemplatesDataSet.SecurityTemplatesRow row)
			{
				base.Rows.Add(row);
			}

			// Token: 0x06009306 RID: 37638 RVA: 0x001CC6B8 File Offset: 0x001CA8B8
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public SecurityTemplatesDataSet.SecurityTemplatesRow AddSecurityTemplatesRow(Guid WSEC_TMPL_UID, string WSEC_TMPL_NAME, string WSEC_TMPL_DESC)
			{
				SecurityTemplatesDataSet.SecurityTemplatesRow securityTemplatesRow = (SecurityTemplatesDataSet.SecurityTemplatesRow)base.NewRow();
				object[] itemArray = new object[]
				{
					WSEC_TMPL_UID,
					WSEC_TMPL_NAME,
					WSEC_TMPL_DESC
				};
				securityTemplatesRow.ItemArray = itemArray;
				base.Rows.Add(securityTemplatesRow);
				return securityTemplatesRow;
			}

			// Token: 0x06009307 RID: 37639 RVA: 0x001CC700 File Offset: 0x001CA900
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public SecurityTemplatesDataSet.SecurityTemplatesRow FindByWSEC_TMPL_UID(Guid WSEC_TMPL_UID)
			{
				return (SecurityTemplatesDataSet.SecurityTemplatesRow)base.Rows.Find(new object[]
				{
					WSEC_TMPL_UID
				});
			}

			// Token: 0x06009308 RID: 37640 RVA: 0x001CC72E File Offset: 0x001CA92E
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			public virtual IEnumerator GetEnumerator()
			{
				return base.Rows.GetEnumerator();
			}

			// Token: 0x06009309 RID: 37641 RVA: 0x001CC73C File Offset: 0x001CA93C
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			public override DataTable Clone()
			{
				SecurityTemplatesDataSet.SecurityTemplatesDataTable securityTemplatesDataTable = (SecurityTemplatesDataSet.SecurityTemplatesDataTable)base.Clone();
				securityTemplatesDataTable.InitVars();
				return securityTemplatesDataTable;
			}

			// Token: 0x0600930A RID: 37642 RVA: 0x001CC75C File Offset: 0x001CA95C
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			protected override DataTable CreateInstance()
			{
				return new SecurityTemplatesDataSet.SecurityTemplatesDataTable();
			}

			// Token: 0x0600930B RID: 37643 RVA: 0x001CC764 File Offset: 0x001CA964
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			internal void InitVars()
			{
				this.columnWSEC_TMPL_UID = base.Columns["WSEC_TMPL_UID"];
				this.columnWSEC_TMPL_NAME = base.Columns["WSEC_TMPL_NAME"];
				this.columnWSEC_TMPL_DESC = base.Columns["WSEC_TMPL_DESC"];
			}

			// Token: 0x0600930C RID: 37644 RVA: 0x001CC7B4 File Offset: 0x001CA9B4
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			private void InitClass()
			{
				this.columnWSEC_TMPL_UID = new DataColumn("WSEC_TMPL_UID", typeof(Guid), null, MappingType.Element);
				base.Columns.Add(this.columnWSEC_TMPL_UID);
				this.columnWSEC_TMPL_NAME = new DataColumn("WSEC_TMPL_NAME", typeof(string), null, MappingType.Element);
				base.Columns.Add(this.columnWSEC_TMPL_NAME);
				this.columnWSEC_TMPL_DESC = new DataColumn("WSEC_TMPL_DESC", typeof(string), null, MappingType.Element);
				base.Columns.Add(this.columnWSEC_TMPL_DESC);
				base.Constraints.Add(new UniqueConstraint("SecurityTemplatesPrimaryKey", new DataColumn[]
				{
					this.columnWSEC_TMPL_UID
				}, true));
				this.columnWSEC_TMPL_UID.AllowDBNull = false;
				this.columnWSEC_TMPL_UID.Unique = true;
				this.columnWSEC_TMPL_NAME.AllowDBNull = false;
			}

			// Token: 0x0600930D RID: 37645 RVA: 0x001CC893 File Offset: 0x001CAA93
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public SecurityTemplatesDataSet.SecurityTemplatesRow NewSecurityTemplatesRow()
			{
				return (SecurityTemplatesDataSet.SecurityTemplatesRow)base.NewRow();
			}

			// Token: 0x0600930E RID: 37646 RVA: 0x001CC8A0 File Offset: 0x001CAAA0
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			protected override DataRow NewRowFromBuilder(DataRowBuilder builder)
			{
				return new SecurityTemplatesDataSet.SecurityTemplatesRow(builder);
			}

			// Token: 0x0600930F RID: 37647 RVA: 0x001CC8A8 File Offset: 0x001CAAA8
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			protected override Type GetRowType()
			{
				return typeof(SecurityTemplatesDataSet.SecurityTemplatesRow);
			}

			// Token: 0x06009310 RID: 37648 RVA: 0x001CC8B4 File Offset: 0x001CAAB4
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			protected override void OnRowChanged(DataRowChangeEventArgs e)
			{
				base.OnRowChanged(e);
				if (this.SecurityTemplatesRowChanged != null)
				{
					this.SecurityTemplatesRowChanged(this, new SecurityTemplatesDataSet.SecurityTemplatesRowChangeEvent((SecurityTemplatesDataSet.SecurityTemplatesRow)e.Row, e.Action));
				}
			}

			// Token: 0x06009311 RID: 37649 RVA: 0x001CC8E7 File Offset: 0x001CAAE7
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			protected override void OnRowChanging(DataRowChangeEventArgs e)
			{
				base.OnRowChanging(e);
				if (this.SecurityTemplatesRowChanging != null)
				{
					this.SecurityTemplatesRowChanging(this, new SecurityTemplatesDataSet.SecurityTemplatesRowChangeEvent((SecurityTemplatesDataSet.SecurityTemplatesRow)e.Row, e.Action));
				}
			}

			// Token: 0x06009312 RID: 37650 RVA: 0x001CC91A File Offset: 0x001CAB1A
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			protected override void OnRowDeleted(DataRowChangeEventArgs e)
			{
				base.OnRowDeleted(e);
				if (this.SecurityTemplatesRowDeleted != null)
				{
					this.SecurityTemplatesRowDeleted(this, new SecurityTemplatesDataSet.SecurityTemplatesRowChangeEvent((SecurityTemplatesDataSet.SecurityTemplatesRow)e.Row, e.Action));
				}
			}

			// Token: 0x06009313 RID: 37651 RVA: 0x001CC94D File Offset: 0x001CAB4D
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			protected override void OnRowDeleting(DataRowChangeEventArgs e)
			{
				base.OnRowDeleting(e);
				if (this.SecurityTemplatesRowDeleting != null)
				{
					this.SecurityTemplatesRowDeleting(this, new SecurityTemplatesDataSet.SecurityTemplatesRowChangeEvent((SecurityTemplatesDataSet.SecurityTemplatesRow)e.Row, e.Action));
				}
			}

			// Token: 0x06009314 RID: 37652 RVA: 0x001CC980 File Offset: 0x001CAB80
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public void RemoveSecurityTemplatesRow(SecurityTemplatesDataSet.SecurityTemplatesRow row)
			{
				base.Rows.Remove(row);
			}

			// Token: 0x06009315 RID: 37653 RVA: 0x001CC990 File Offset: 0x001CAB90
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public static XmlSchemaComplexType GetTypedTableSchema(XmlSchemaSet xs)
			{
				XmlSchemaComplexType xmlSchemaComplexType = new XmlSchemaComplexType();
				XmlSchemaSequence xmlSchemaSequence = new XmlSchemaSequence();
				SecurityTemplatesDataSet securityTemplatesDataSet = new SecurityTemplatesDataSet();
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
				xmlSchemaAttribute.FixedValue = securityTemplatesDataSet.Namespace;
				xmlSchemaComplexType.Attributes.Add(xmlSchemaAttribute);
				XmlSchemaAttribute xmlSchemaAttribute2 = new XmlSchemaAttribute();
				xmlSchemaAttribute2.Name = "tableTypeName";
				xmlSchemaAttribute2.FixedValue = "SecurityTemplatesDataTable";
				xmlSchemaComplexType.Attributes.Add(xmlSchemaAttribute2);
				xmlSchemaComplexType.Particle = xmlSchemaSequence;
				XmlSchema schemaSerializable = securityTemplatesDataSet.GetSchemaSerializable();
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

			// Token: 0x04001D79 RID: 7545
			private DataColumn columnWSEC_TMPL_UID;

			// Token: 0x04001D7A RID: 7546
			private DataColumn columnWSEC_TMPL_NAME;

			// Token: 0x04001D7B RID: 7547
			private DataColumn columnWSEC_TMPL_DESC;
		}

		// Token: 0x02000632 RID: 1586
		[XmlSchemaProvider("GetTypedTableSchema")]
		[Serializable]
		public class CategoryPermissionsDataTable : DataTable, IEnumerable
		{
			// Token: 0x06009316 RID: 37654 RVA: 0x001CCB88 File Offset: 0x001CAD88
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			public CategoryPermissionsDataTable()
			{
				base.TableName = "CategoryPermissions";
				this.BeginInit();
				this.InitClass();
				this.EndInit();
			}

			// Token: 0x06009317 RID: 37655 RVA: 0x001CCBB0 File Offset: 0x001CADB0
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			internal CategoryPermissionsDataTable(DataTable table)
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

			// Token: 0x06009318 RID: 37656 RVA: 0x001CCC58 File Offset: 0x001CAE58
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			protected CategoryPermissionsDataTable(SerializationInfo info, StreamingContext context) : base(info, context)
			{
				this.InitVars();
			}

			// Token: 0x17002C17 RID: 11287
			// (get) Token: 0x06009319 RID: 37657 RVA: 0x001CCC68 File Offset: 0x001CAE68
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			public DataColumn WSEC_TMPL_UIDColumn
			{
				get
				{
					return this.columnWSEC_TMPL_UID;
				}
			}

			// Token: 0x17002C18 RID: 11288
			// (get) Token: 0x0600931A RID: 37658 RVA: 0x001CCC70 File Offset: 0x001CAE70
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			public DataColumn WSEC_FEA_ACT_UIDColumn
			{
				get
				{
					return this.columnWSEC_FEA_ACT_UID;
				}
			}

			// Token: 0x17002C19 RID: 11289
			// (get) Token: 0x0600931B RID: 37659 RVA: 0x001CCC78 File Offset: 0x001CAE78
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public DataColumn WSEC_ALLOWColumn
			{
				get
				{
					return this.columnWSEC_ALLOW;
				}
			}

			// Token: 0x17002C1A RID: 11290
			// (get) Token: 0x0600931C RID: 37660 RVA: 0x001CCC80 File Offset: 0x001CAE80
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			public DataColumn WSEC_DENYColumn
			{
				get
				{
					return this.columnWSEC_DENY;
				}
			}

			// Token: 0x17002C1B RID: 11291
			// (get) Token: 0x0600931D RID: 37661 RVA: 0x001CCC88 File Offset: 0x001CAE88
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

			// Token: 0x17002C1C RID: 11292
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			public SecurityTemplatesDataSet.CategoryPermissionsRow this[int index]
			{
				get
				{
					return (SecurityTemplatesDataSet.CategoryPermissionsRow)base.Rows[index];
				}
			}

			// Token: 0x1400055D RID: 1373
			// (add) Token: 0x0600931F RID: 37663 RVA: 0x001CCCA8 File Offset: 0x001CAEA8
			// (remove) Token: 0x06009320 RID: 37664 RVA: 0x001CCCE0 File Offset: 0x001CAEE0
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public event SecurityTemplatesDataSet.CategoryPermissionsRowChangeEventHandler CategoryPermissionsRowChanging;

			// Token: 0x1400055E RID: 1374
			// (add) Token: 0x06009321 RID: 37665 RVA: 0x001CCD18 File Offset: 0x001CAF18
			// (remove) Token: 0x06009322 RID: 37666 RVA: 0x001CCD50 File Offset: 0x001CAF50
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public event SecurityTemplatesDataSet.CategoryPermissionsRowChangeEventHandler CategoryPermissionsRowChanged;

			// Token: 0x1400055F RID: 1375
			// (add) Token: 0x06009323 RID: 37667 RVA: 0x001CCD88 File Offset: 0x001CAF88
			// (remove) Token: 0x06009324 RID: 37668 RVA: 0x001CCDC0 File Offset: 0x001CAFC0
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public event SecurityTemplatesDataSet.CategoryPermissionsRowChangeEventHandler CategoryPermissionsRowDeleting;

			// Token: 0x14000560 RID: 1376
			// (add) Token: 0x06009325 RID: 37669 RVA: 0x001CCDF8 File Offset: 0x001CAFF8
			// (remove) Token: 0x06009326 RID: 37670 RVA: 0x001CCE30 File Offset: 0x001CB030
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public event SecurityTemplatesDataSet.CategoryPermissionsRowChangeEventHandler CategoryPermissionsRowDeleted;

			// Token: 0x06009327 RID: 37671 RVA: 0x001CCE65 File Offset: 0x001CB065
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			public void AddCategoryPermissionsRow(SecurityTemplatesDataSet.CategoryPermissionsRow row)
			{
				base.Rows.Add(row);
			}

			// Token: 0x06009328 RID: 37672 RVA: 0x001CCE74 File Offset: 0x001CB074
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			public SecurityTemplatesDataSet.CategoryPermissionsRow AddCategoryPermissionsRow(Guid WSEC_TMPL_UID, Guid WSEC_FEA_ACT_UID, bool WSEC_ALLOW, bool WSEC_DENY)
			{
				SecurityTemplatesDataSet.CategoryPermissionsRow categoryPermissionsRow = (SecurityTemplatesDataSet.CategoryPermissionsRow)base.NewRow();
				object[] itemArray = new object[]
				{
					WSEC_TMPL_UID,
					WSEC_FEA_ACT_UID,
					WSEC_ALLOW,
					WSEC_DENY
				};
				categoryPermissionsRow.ItemArray = itemArray;
				base.Rows.Add(categoryPermissionsRow);
				return categoryPermissionsRow;
			}

			// Token: 0x06009329 RID: 37673 RVA: 0x001CCED0 File Offset: 0x001CB0D0
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			public SecurityTemplatesDataSet.CategoryPermissionsRow FindByWSEC_FEA_ACT_UIDWSEC_TMPL_UID(Guid WSEC_FEA_ACT_UID, Guid WSEC_TMPL_UID)
			{
				return (SecurityTemplatesDataSet.CategoryPermissionsRow)base.Rows.Find(new object[]
				{
					WSEC_FEA_ACT_UID,
					WSEC_TMPL_UID
				});
			}

			// Token: 0x0600932A RID: 37674 RVA: 0x001CCF07 File Offset: 0x001CB107
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public virtual IEnumerator GetEnumerator()
			{
				return base.Rows.GetEnumerator();
			}

			// Token: 0x0600932B RID: 37675 RVA: 0x001CCF14 File Offset: 0x001CB114
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			public override DataTable Clone()
			{
				SecurityTemplatesDataSet.CategoryPermissionsDataTable categoryPermissionsDataTable = (SecurityTemplatesDataSet.CategoryPermissionsDataTable)base.Clone();
				categoryPermissionsDataTable.InitVars();
				return categoryPermissionsDataTable;
			}

			// Token: 0x0600932C RID: 37676 RVA: 0x001CCF34 File Offset: 0x001CB134
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			protected override DataTable CreateInstance()
			{
				return new SecurityTemplatesDataSet.CategoryPermissionsDataTable();
			}

			// Token: 0x0600932D RID: 37677 RVA: 0x001CCF3C File Offset: 0x001CB13C
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			internal void InitVars()
			{
				this.columnWSEC_TMPL_UID = base.Columns["WSEC_TMPL_UID"];
				this.columnWSEC_FEA_ACT_UID = base.Columns["WSEC_FEA_ACT_UID"];
				this.columnWSEC_ALLOW = base.Columns["WSEC_ALLOW"];
				this.columnWSEC_DENY = base.Columns["WSEC_DENY"];
			}

			// Token: 0x0600932E RID: 37678 RVA: 0x001CCFA4 File Offset: 0x001CB1A4
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			private void InitClass()
			{
				this.columnWSEC_TMPL_UID = new DataColumn("WSEC_TMPL_UID", typeof(Guid), null, MappingType.Element);
				base.Columns.Add(this.columnWSEC_TMPL_UID);
				this.columnWSEC_FEA_ACT_UID = new DataColumn("WSEC_FEA_ACT_UID", typeof(Guid), null, MappingType.Element);
				base.Columns.Add(this.columnWSEC_FEA_ACT_UID);
				this.columnWSEC_ALLOW = new DataColumn("WSEC_ALLOW", typeof(bool), null, MappingType.Element);
				base.Columns.Add(this.columnWSEC_ALLOW);
				this.columnWSEC_DENY = new DataColumn("WSEC_DENY", typeof(bool), null, MappingType.Element);
				base.Columns.Add(this.columnWSEC_DENY);
				base.Constraints.Add(new UniqueConstraint("CategoryPermissionsPrimaryKey", new DataColumn[]
				{
					this.columnWSEC_FEA_ACT_UID,
					this.columnWSEC_TMPL_UID
				}, true));
				this.columnWSEC_TMPL_UID.AllowDBNull = false;
				this.columnWSEC_FEA_ACT_UID.AllowDBNull = false;
				this.columnWSEC_ALLOW.AllowDBNull = false;
				this.columnWSEC_ALLOW.DefaultValue = false;
				this.columnWSEC_DENY.AllowDBNull = false;
				this.columnWSEC_DENY.DefaultValue = false;
			}

			// Token: 0x0600932F RID: 37679 RVA: 0x001CD0E7 File Offset: 0x001CB2E7
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			public SecurityTemplatesDataSet.CategoryPermissionsRow NewCategoryPermissionsRow()
			{
				return (SecurityTemplatesDataSet.CategoryPermissionsRow)base.NewRow();
			}

			// Token: 0x06009330 RID: 37680 RVA: 0x001CD0F4 File Offset: 0x001CB2F4
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			protected override DataRow NewRowFromBuilder(DataRowBuilder builder)
			{
				return new SecurityTemplatesDataSet.CategoryPermissionsRow(builder);
			}

			// Token: 0x06009331 RID: 37681 RVA: 0x001CD0FC File Offset: 0x001CB2FC
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			protected override Type GetRowType()
			{
				return typeof(SecurityTemplatesDataSet.CategoryPermissionsRow);
			}

			// Token: 0x06009332 RID: 37682 RVA: 0x001CD108 File Offset: 0x001CB308
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			protected override void OnRowChanged(DataRowChangeEventArgs e)
			{
				base.OnRowChanged(e);
				if (this.CategoryPermissionsRowChanged != null)
				{
					this.CategoryPermissionsRowChanged(this, new SecurityTemplatesDataSet.CategoryPermissionsRowChangeEvent((SecurityTemplatesDataSet.CategoryPermissionsRow)e.Row, e.Action));
				}
			}

			// Token: 0x06009333 RID: 37683 RVA: 0x001CD13B File Offset: 0x001CB33B
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			protected override void OnRowChanging(DataRowChangeEventArgs e)
			{
				base.OnRowChanging(e);
				if (this.CategoryPermissionsRowChanging != null)
				{
					this.CategoryPermissionsRowChanging(this, new SecurityTemplatesDataSet.CategoryPermissionsRowChangeEvent((SecurityTemplatesDataSet.CategoryPermissionsRow)e.Row, e.Action));
				}
			}

			// Token: 0x06009334 RID: 37684 RVA: 0x001CD16E File Offset: 0x001CB36E
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			protected override void OnRowDeleted(DataRowChangeEventArgs e)
			{
				base.OnRowDeleted(e);
				if (this.CategoryPermissionsRowDeleted != null)
				{
					this.CategoryPermissionsRowDeleted(this, new SecurityTemplatesDataSet.CategoryPermissionsRowChangeEvent((SecurityTemplatesDataSet.CategoryPermissionsRow)e.Row, e.Action));
				}
			}

			// Token: 0x06009335 RID: 37685 RVA: 0x001CD1A1 File Offset: 0x001CB3A1
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			protected override void OnRowDeleting(DataRowChangeEventArgs e)
			{
				base.OnRowDeleting(e);
				if (this.CategoryPermissionsRowDeleting != null)
				{
					this.CategoryPermissionsRowDeleting(this, new SecurityTemplatesDataSet.CategoryPermissionsRowChangeEvent((SecurityTemplatesDataSet.CategoryPermissionsRow)e.Row, e.Action));
				}
			}

			// Token: 0x06009336 RID: 37686 RVA: 0x001CD1D4 File Offset: 0x001CB3D4
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			public void RemoveCategoryPermissionsRow(SecurityTemplatesDataSet.CategoryPermissionsRow row)
			{
				base.Rows.Remove(row);
			}

			// Token: 0x06009337 RID: 37687 RVA: 0x001CD1E4 File Offset: 0x001CB3E4
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			public static XmlSchemaComplexType GetTypedTableSchema(XmlSchemaSet xs)
			{
				XmlSchemaComplexType xmlSchemaComplexType = new XmlSchemaComplexType();
				XmlSchemaSequence xmlSchemaSequence = new XmlSchemaSequence();
				SecurityTemplatesDataSet securityTemplatesDataSet = new SecurityTemplatesDataSet();
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
				xmlSchemaAttribute.FixedValue = securityTemplatesDataSet.Namespace;
				xmlSchemaComplexType.Attributes.Add(xmlSchemaAttribute);
				XmlSchemaAttribute xmlSchemaAttribute2 = new XmlSchemaAttribute();
				xmlSchemaAttribute2.Name = "tableTypeName";
				xmlSchemaAttribute2.FixedValue = "CategoryPermissionsDataTable";
				xmlSchemaComplexType.Attributes.Add(xmlSchemaAttribute2);
				xmlSchemaComplexType.Particle = xmlSchemaSequence;
				XmlSchema schemaSerializable = securityTemplatesDataSet.GetSchemaSerializable();
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

			// Token: 0x04001D80 RID: 7552
			private DataColumn columnWSEC_TMPL_UID;

			// Token: 0x04001D81 RID: 7553
			private DataColumn columnWSEC_FEA_ACT_UID;

			// Token: 0x04001D82 RID: 7554
			private DataColumn columnWSEC_ALLOW;

			// Token: 0x04001D83 RID: 7555
			private DataColumn columnWSEC_DENY;
		}

		// Token: 0x02000633 RID: 1587
		[XmlSchemaProvider("GetTypedTableSchema")]
		[Serializable]
		public class GlobalPermissionsDataTable : DataTable, IEnumerable
		{
			// Token: 0x06009338 RID: 37688 RVA: 0x001CD3DC File Offset: 0x001CB5DC
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			public GlobalPermissionsDataTable()
			{
				base.TableName = "GlobalPermissions";
				this.BeginInit();
				this.InitClass();
				this.EndInit();
			}

			// Token: 0x06009339 RID: 37689 RVA: 0x001CD404 File Offset: 0x001CB604
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			internal GlobalPermissionsDataTable(DataTable table)
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

			// Token: 0x0600933A RID: 37690 RVA: 0x001CD4AC File Offset: 0x001CB6AC
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			protected GlobalPermissionsDataTable(SerializationInfo info, StreamingContext context) : base(info, context)
			{
				this.InitVars();
			}

			// Token: 0x17002C1D RID: 11293
			// (get) Token: 0x0600933B RID: 37691 RVA: 0x001CD4BC File Offset: 0x001CB6BC
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			public DataColumn WSEC_TMPL_UIDColumn
			{
				get
				{
					return this.columnWSEC_TMPL_UID;
				}
			}

			// Token: 0x17002C1E RID: 11294
			// (get) Token: 0x0600933C RID: 37692 RVA: 0x001CD4C4 File Offset: 0x001CB6C4
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			public DataColumn WSEC_FEA_ACT_UIDColumn
			{
				get
				{
					return this.columnWSEC_FEA_ACT_UID;
				}
			}

			// Token: 0x17002C1F RID: 11295
			// (get) Token: 0x0600933D RID: 37693 RVA: 0x001CD4CC File Offset: 0x001CB6CC
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public DataColumn WSEC_ALLOWColumn
			{
				get
				{
					return this.columnWSEC_ALLOW;
				}
			}

			// Token: 0x17002C20 RID: 11296
			// (get) Token: 0x0600933E RID: 37694 RVA: 0x001CD4D4 File Offset: 0x001CB6D4
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public DataColumn WSEC_DENYColumn
			{
				get
				{
					return this.columnWSEC_DENY;
				}
			}

			// Token: 0x17002C21 RID: 11297
			// (get) Token: 0x0600933F RID: 37695 RVA: 0x001CD4DC File Offset: 0x001CB6DC
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

			// Token: 0x17002C22 RID: 11298
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public SecurityTemplatesDataSet.GlobalPermissionsRow this[int index]
			{
				get
				{
					return (SecurityTemplatesDataSet.GlobalPermissionsRow)base.Rows[index];
				}
			}

			// Token: 0x14000561 RID: 1377
			// (add) Token: 0x06009341 RID: 37697 RVA: 0x001CD4FC File Offset: 0x001CB6FC
			// (remove) Token: 0x06009342 RID: 37698 RVA: 0x001CD534 File Offset: 0x001CB734
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public event SecurityTemplatesDataSet.GlobalPermissionsRowChangeEventHandler GlobalPermissionsRowChanging;

			// Token: 0x14000562 RID: 1378
			// (add) Token: 0x06009343 RID: 37699 RVA: 0x001CD56C File Offset: 0x001CB76C
			// (remove) Token: 0x06009344 RID: 37700 RVA: 0x001CD5A4 File Offset: 0x001CB7A4
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public event SecurityTemplatesDataSet.GlobalPermissionsRowChangeEventHandler GlobalPermissionsRowChanged;

			// Token: 0x14000563 RID: 1379
			// (add) Token: 0x06009345 RID: 37701 RVA: 0x001CD5DC File Offset: 0x001CB7DC
			// (remove) Token: 0x06009346 RID: 37702 RVA: 0x001CD614 File Offset: 0x001CB814
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public event SecurityTemplatesDataSet.GlobalPermissionsRowChangeEventHandler GlobalPermissionsRowDeleting;

			// Token: 0x14000564 RID: 1380
			// (add) Token: 0x06009347 RID: 37703 RVA: 0x001CD64C File Offset: 0x001CB84C
			// (remove) Token: 0x06009348 RID: 37704 RVA: 0x001CD684 File Offset: 0x001CB884
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public event SecurityTemplatesDataSet.GlobalPermissionsRowChangeEventHandler GlobalPermissionsRowDeleted;

			// Token: 0x06009349 RID: 37705 RVA: 0x001CD6B9 File Offset: 0x001CB8B9
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public void AddGlobalPermissionsRow(SecurityTemplatesDataSet.GlobalPermissionsRow row)
			{
				base.Rows.Add(row);
			}

			// Token: 0x0600934A RID: 37706 RVA: 0x001CD6C8 File Offset: 0x001CB8C8
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public SecurityTemplatesDataSet.GlobalPermissionsRow AddGlobalPermissionsRow(Guid WSEC_TMPL_UID, Guid WSEC_FEA_ACT_UID, bool WSEC_ALLOW, bool WSEC_DENY)
			{
				SecurityTemplatesDataSet.GlobalPermissionsRow globalPermissionsRow = (SecurityTemplatesDataSet.GlobalPermissionsRow)base.NewRow();
				object[] itemArray = new object[]
				{
					WSEC_TMPL_UID,
					WSEC_FEA_ACT_UID,
					WSEC_ALLOW,
					WSEC_DENY
				};
				globalPermissionsRow.ItemArray = itemArray;
				base.Rows.Add(globalPermissionsRow);
				return globalPermissionsRow;
			}

			// Token: 0x0600934B RID: 37707 RVA: 0x001CD724 File Offset: 0x001CB924
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			public SecurityTemplatesDataSet.GlobalPermissionsRow FindByWSEC_FEA_ACT_UIDWSEC_TMPL_UID(Guid WSEC_FEA_ACT_UID, Guid WSEC_TMPL_UID)
			{
				return (SecurityTemplatesDataSet.GlobalPermissionsRow)base.Rows.Find(new object[]
				{
					WSEC_FEA_ACT_UID,
					WSEC_TMPL_UID
				});
			}

			// Token: 0x0600934C RID: 37708 RVA: 0x001CD75B File Offset: 0x001CB95B
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			public virtual IEnumerator GetEnumerator()
			{
				return base.Rows.GetEnumerator();
			}

			// Token: 0x0600934D RID: 37709 RVA: 0x001CD768 File Offset: 0x001CB968
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			public override DataTable Clone()
			{
				SecurityTemplatesDataSet.GlobalPermissionsDataTable globalPermissionsDataTable = (SecurityTemplatesDataSet.GlobalPermissionsDataTable)base.Clone();
				globalPermissionsDataTable.InitVars();
				return globalPermissionsDataTable;
			}

			// Token: 0x0600934E RID: 37710 RVA: 0x001CD788 File Offset: 0x001CB988
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			protected override DataTable CreateInstance()
			{
				return new SecurityTemplatesDataSet.GlobalPermissionsDataTable();
			}

			// Token: 0x0600934F RID: 37711 RVA: 0x001CD790 File Offset: 0x001CB990
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			internal void InitVars()
			{
				this.columnWSEC_TMPL_UID = base.Columns["WSEC_TMPL_UID"];
				this.columnWSEC_FEA_ACT_UID = base.Columns["WSEC_FEA_ACT_UID"];
				this.columnWSEC_ALLOW = base.Columns["WSEC_ALLOW"];
				this.columnWSEC_DENY = base.Columns["WSEC_DENY"];
			}

			// Token: 0x06009350 RID: 37712 RVA: 0x001CD7F8 File Offset: 0x001CB9F8
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			private void InitClass()
			{
				this.columnWSEC_TMPL_UID = new DataColumn("WSEC_TMPL_UID", typeof(Guid), null, MappingType.Element);
				base.Columns.Add(this.columnWSEC_TMPL_UID);
				this.columnWSEC_FEA_ACT_UID = new DataColumn("WSEC_FEA_ACT_UID", typeof(Guid), null, MappingType.Element);
				base.Columns.Add(this.columnWSEC_FEA_ACT_UID);
				this.columnWSEC_ALLOW = new DataColumn("WSEC_ALLOW", typeof(bool), null, MappingType.Element);
				base.Columns.Add(this.columnWSEC_ALLOW);
				this.columnWSEC_DENY = new DataColumn("WSEC_DENY", typeof(bool), null, MappingType.Element);
				base.Columns.Add(this.columnWSEC_DENY);
				base.Constraints.Add(new UniqueConstraint("GlobalPermissionsPrimaryKey", new DataColumn[]
				{
					this.columnWSEC_FEA_ACT_UID,
					this.columnWSEC_TMPL_UID
				}, true));
				this.columnWSEC_TMPL_UID.AllowDBNull = false;
				this.columnWSEC_FEA_ACT_UID.AllowDBNull = false;
				this.columnWSEC_ALLOW.AllowDBNull = false;
				this.columnWSEC_ALLOW.DefaultValue = false;
				this.columnWSEC_DENY.AllowDBNull = false;
				this.columnWSEC_DENY.DefaultValue = false;
			}

			// Token: 0x06009351 RID: 37713 RVA: 0x001CD93B File Offset: 0x001CBB3B
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			public SecurityTemplatesDataSet.GlobalPermissionsRow NewGlobalPermissionsRow()
			{
				return (SecurityTemplatesDataSet.GlobalPermissionsRow)base.NewRow();
			}

			// Token: 0x06009352 RID: 37714 RVA: 0x001CD948 File Offset: 0x001CBB48
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			protected override DataRow NewRowFromBuilder(DataRowBuilder builder)
			{
				return new SecurityTemplatesDataSet.GlobalPermissionsRow(builder);
			}

			// Token: 0x06009353 RID: 37715 RVA: 0x001CD950 File Offset: 0x001CBB50
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			protected override Type GetRowType()
			{
				return typeof(SecurityTemplatesDataSet.GlobalPermissionsRow);
			}

			// Token: 0x06009354 RID: 37716 RVA: 0x001CD95C File Offset: 0x001CBB5C
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			protected override void OnRowChanged(DataRowChangeEventArgs e)
			{
				base.OnRowChanged(e);
				if (this.GlobalPermissionsRowChanged != null)
				{
					this.GlobalPermissionsRowChanged(this, new SecurityTemplatesDataSet.GlobalPermissionsRowChangeEvent((SecurityTemplatesDataSet.GlobalPermissionsRow)e.Row, e.Action));
				}
			}

			// Token: 0x06009355 RID: 37717 RVA: 0x001CD98F File Offset: 0x001CBB8F
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			protected override void OnRowChanging(DataRowChangeEventArgs e)
			{
				base.OnRowChanging(e);
				if (this.GlobalPermissionsRowChanging != null)
				{
					this.GlobalPermissionsRowChanging(this, new SecurityTemplatesDataSet.GlobalPermissionsRowChangeEvent((SecurityTemplatesDataSet.GlobalPermissionsRow)e.Row, e.Action));
				}
			}

			// Token: 0x06009356 RID: 37718 RVA: 0x001CD9C2 File Offset: 0x001CBBC2
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			protected override void OnRowDeleted(DataRowChangeEventArgs e)
			{
				base.OnRowDeleted(e);
				if (this.GlobalPermissionsRowDeleted != null)
				{
					this.GlobalPermissionsRowDeleted(this, new SecurityTemplatesDataSet.GlobalPermissionsRowChangeEvent((SecurityTemplatesDataSet.GlobalPermissionsRow)e.Row, e.Action));
				}
			}

			// Token: 0x06009357 RID: 37719 RVA: 0x001CD9F5 File Offset: 0x001CBBF5
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			protected override void OnRowDeleting(DataRowChangeEventArgs e)
			{
				base.OnRowDeleting(e);
				if (this.GlobalPermissionsRowDeleting != null)
				{
					this.GlobalPermissionsRowDeleting(this, new SecurityTemplatesDataSet.GlobalPermissionsRowChangeEvent((SecurityTemplatesDataSet.GlobalPermissionsRow)e.Row, e.Action));
				}
			}

			// Token: 0x06009358 RID: 37720 RVA: 0x001CDA28 File Offset: 0x001CBC28
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			public void RemoveGlobalPermissionsRow(SecurityTemplatesDataSet.GlobalPermissionsRow row)
			{
				base.Rows.Remove(row);
			}

			// Token: 0x06009359 RID: 37721 RVA: 0x001CDA38 File Offset: 0x001CBC38
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public static XmlSchemaComplexType GetTypedTableSchema(XmlSchemaSet xs)
			{
				XmlSchemaComplexType xmlSchemaComplexType = new XmlSchemaComplexType();
				XmlSchemaSequence xmlSchemaSequence = new XmlSchemaSequence();
				SecurityTemplatesDataSet securityTemplatesDataSet = new SecurityTemplatesDataSet();
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
				xmlSchemaAttribute.FixedValue = securityTemplatesDataSet.Namespace;
				xmlSchemaComplexType.Attributes.Add(xmlSchemaAttribute);
				XmlSchemaAttribute xmlSchemaAttribute2 = new XmlSchemaAttribute();
				xmlSchemaAttribute2.Name = "tableTypeName";
				xmlSchemaAttribute2.FixedValue = "GlobalPermissionsDataTable";
				xmlSchemaComplexType.Attributes.Add(xmlSchemaAttribute2);
				xmlSchemaComplexType.Particle = xmlSchemaSequence;
				XmlSchema schemaSerializable = securityTemplatesDataSet.GetSchemaSerializable();
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

			// Token: 0x04001D88 RID: 7560
			private DataColumn columnWSEC_TMPL_UID;

			// Token: 0x04001D89 RID: 7561
			private DataColumn columnWSEC_FEA_ACT_UID;

			// Token: 0x04001D8A RID: 7562
			private DataColumn columnWSEC_ALLOW;

			// Token: 0x04001D8B RID: 7563
			private DataColumn columnWSEC_DENY;
		}

		// Token: 0x02000634 RID: 1588
		public class SecurityTemplatesRow : DataRow
		{
			// Token: 0x0600935A RID: 37722 RVA: 0x001CDC30 File Offset: 0x001CBE30
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			internal SecurityTemplatesRow(DataRowBuilder rb) : base(rb)
			{
				this.tableSecurityTemplates = (SecurityTemplatesDataSet.SecurityTemplatesDataTable)base.Table;
			}

			// Token: 0x17002C23 RID: 11299
			// (get) Token: 0x0600935B RID: 37723 RVA: 0x001CDC4A File Offset: 0x001CBE4A
			// (set) Token: 0x0600935C RID: 37724 RVA: 0x001CDC62 File Offset: 0x001CBE62
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public Guid WSEC_TMPL_UID
			{
				get
				{
					return (Guid)base[this.tableSecurityTemplates.WSEC_TMPL_UIDColumn];
				}
				set
				{
					base[this.tableSecurityTemplates.WSEC_TMPL_UIDColumn] = value;
				}
			}

			// Token: 0x17002C24 RID: 11300
			// (get) Token: 0x0600935D RID: 37725 RVA: 0x001CDC7B File Offset: 0x001CBE7B
			// (set) Token: 0x0600935E RID: 37726 RVA: 0x001CDC93 File Offset: 0x001CBE93
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public string WSEC_TMPL_NAME
			{
				get
				{
					return (string)base[this.tableSecurityTemplates.WSEC_TMPL_NAMEColumn];
				}
				set
				{
					base[this.tableSecurityTemplates.WSEC_TMPL_NAMEColumn] = value;
				}
			}

			// Token: 0x17002C25 RID: 11301
			// (get) Token: 0x0600935F RID: 37727 RVA: 0x001CDCA8 File Offset: 0x001CBEA8
			// (set) Token: 0x06009360 RID: 37728 RVA: 0x001CDCEC File Offset: 0x001CBEEC
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public string WSEC_TMPL_DESC
			{
				get
				{
					string result;
					try
					{
						result = (string)base[this.tableSecurityTemplates.WSEC_TMPL_DESCColumn];
					}
					catch (InvalidCastException innerException)
					{
						throw new StrongTypingException("The value for column 'WSEC_TMPL_DESC' in table 'SecurityTemplates' is DBNull.", innerException);
					}
					return result;
				}
				set
				{
					base[this.tableSecurityTemplates.WSEC_TMPL_DESCColumn] = value;
				}
			}

			// Token: 0x06009361 RID: 37729 RVA: 0x001CDD00 File Offset: 0x001CBF00
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			public bool IsWSEC_TMPL_DESCNull()
			{
				return base.IsNull(this.tableSecurityTemplates.WSEC_TMPL_DESCColumn);
			}

			// Token: 0x06009362 RID: 37730 RVA: 0x001CDD13 File Offset: 0x001CBF13
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public void SetWSEC_TMPL_DESCNull()
			{
				base[this.tableSecurityTemplates.WSEC_TMPL_DESCColumn] = Convert.DBNull;
			}

			// Token: 0x04001D90 RID: 7568
			private SecurityTemplatesDataSet.SecurityTemplatesDataTable tableSecurityTemplates;
		}

		// Token: 0x02000635 RID: 1589
		public class CategoryPermissionsRow : DataRow
		{
			// Token: 0x06009363 RID: 37731 RVA: 0x001CDD2B File Offset: 0x001CBF2B
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			internal CategoryPermissionsRow(DataRowBuilder rb) : base(rb)
			{
				this.tableCategoryPermissions = (SecurityTemplatesDataSet.CategoryPermissionsDataTable)base.Table;
			}

			// Token: 0x17002C26 RID: 11302
			// (get) Token: 0x06009364 RID: 37732 RVA: 0x001CDD45 File Offset: 0x001CBF45
			// (set) Token: 0x06009365 RID: 37733 RVA: 0x001CDD5D File Offset: 0x001CBF5D
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public Guid WSEC_TMPL_UID
			{
				get
				{
					return (Guid)base[this.tableCategoryPermissions.WSEC_TMPL_UIDColumn];
				}
				set
				{
					base[this.tableCategoryPermissions.WSEC_TMPL_UIDColumn] = value;
				}
			}

			// Token: 0x17002C27 RID: 11303
			// (get) Token: 0x06009366 RID: 37734 RVA: 0x001CDD76 File Offset: 0x001CBF76
			// (set) Token: 0x06009367 RID: 37735 RVA: 0x001CDD8E File Offset: 0x001CBF8E
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public Guid WSEC_FEA_ACT_UID
			{
				get
				{
					return (Guid)base[this.tableCategoryPermissions.WSEC_FEA_ACT_UIDColumn];
				}
				set
				{
					base[this.tableCategoryPermissions.WSEC_FEA_ACT_UIDColumn] = value;
				}
			}

			// Token: 0x17002C28 RID: 11304
			// (get) Token: 0x06009368 RID: 37736 RVA: 0x001CDDA7 File Offset: 0x001CBFA7
			// (set) Token: 0x06009369 RID: 37737 RVA: 0x001CDDBF File Offset: 0x001CBFBF
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public bool WSEC_ALLOW
			{
				get
				{
					return (bool)base[this.tableCategoryPermissions.WSEC_ALLOWColumn];
				}
				set
				{
					base[this.tableCategoryPermissions.WSEC_ALLOWColumn] = value;
				}
			}

			// Token: 0x17002C29 RID: 11305
			// (get) Token: 0x0600936A RID: 37738 RVA: 0x001CDDD8 File Offset: 0x001CBFD8
			// (set) Token: 0x0600936B RID: 37739 RVA: 0x001CDDF0 File Offset: 0x001CBFF0
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public bool WSEC_DENY
			{
				get
				{
					return (bool)base[this.tableCategoryPermissions.WSEC_DENYColumn];
				}
				set
				{
					base[this.tableCategoryPermissions.WSEC_DENYColumn] = value;
				}
			}

			// Token: 0x04001D91 RID: 7569
			private SecurityTemplatesDataSet.CategoryPermissionsDataTable tableCategoryPermissions;
		}

		// Token: 0x02000636 RID: 1590
		public class GlobalPermissionsRow : DataRow
		{
			// Token: 0x0600936C RID: 37740 RVA: 0x001CDE09 File Offset: 0x001CC009
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			internal GlobalPermissionsRow(DataRowBuilder rb) : base(rb)
			{
				this.tableGlobalPermissions = (SecurityTemplatesDataSet.GlobalPermissionsDataTable)base.Table;
			}

			// Token: 0x17002C2A RID: 11306
			// (get) Token: 0x0600936D RID: 37741 RVA: 0x001CDE23 File Offset: 0x001CC023
			// (set) Token: 0x0600936E RID: 37742 RVA: 0x001CDE3B File Offset: 0x001CC03B
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			public Guid WSEC_TMPL_UID
			{
				get
				{
					return (Guid)base[this.tableGlobalPermissions.WSEC_TMPL_UIDColumn];
				}
				set
				{
					base[this.tableGlobalPermissions.WSEC_TMPL_UIDColumn] = value;
				}
			}

			// Token: 0x17002C2B RID: 11307
			// (get) Token: 0x0600936F RID: 37743 RVA: 0x001CDE54 File Offset: 0x001CC054
			// (set) Token: 0x06009370 RID: 37744 RVA: 0x001CDE6C File Offset: 0x001CC06C
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			public Guid WSEC_FEA_ACT_UID
			{
				get
				{
					return (Guid)base[this.tableGlobalPermissions.WSEC_FEA_ACT_UIDColumn];
				}
				set
				{
					base[this.tableGlobalPermissions.WSEC_FEA_ACT_UIDColumn] = value;
				}
			}

			// Token: 0x17002C2C RID: 11308
			// (get) Token: 0x06009371 RID: 37745 RVA: 0x001CDE85 File Offset: 0x001CC085
			// (set) Token: 0x06009372 RID: 37746 RVA: 0x001CDE9D File Offset: 0x001CC09D
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public bool WSEC_ALLOW
			{
				get
				{
					return (bool)base[this.tableGlobalPermissions.WSEC_ALLOWColumn];
				}
				set
				{
					base[this.tableGlobalPermissions.WSEC_ALLOWColumn] = value;
				}
			}

			// Token: 0x17002C2D RID: 11309
			// (get) Token: 0x06009373 RID: 37747 RVA: 0x001CDEB6 File Offset: 0x001CC0B6
			// (set) Token: 0x06009374 RID: 37748 RVA: 0x001CDECE File Offset: 0x001CC0CE
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			public bool WSEC_DENY
			{
				get
				{
					return (bool)base[this.tableGlobalPermissions.WSEC_DENYColumn];
				}
				set
				{
					base[this.tableGlobalPermissions.WSEC_DENYColumn] = value;
				}
			}

			// Token: 0x04001D92 RID: 7570
			private SecurityTemplatesDataSet.GlobalPermissionsDataTable tableGlobalPermissions;
		}

		// Token: 0x02000637 RID: 1591
		[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
		public class SecurityTemplatesRowChangeEvent : EventArgs
		{
			// Token: 0x06009375 RID: 37749 RVA: 0x001CDEE7 File Offset: 0x001CC0E7
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public SecurityTemplatesRowChangeEvent(SecurityTemplatesDataSet.SecurityTemplatesRow row, DataRowAction action)
			{
				this.eventRow = row;
				this.eventAction = action;
			}

			// Token: 0x17002C2E RID: 11310
			// (get) Token: 0x06009376 RID: 37750 RVA: 0x001CDEFD File Offset: 0x001CC0FD
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public SecurityTemplatesDataSet.SecurityTemplatesRow Row
			{
				get
				{
					return this.eventRow;
				}
			}

			// Token: 0x17002C2F RID: 11311
			// (get) Token: 0x06009377 RID: 37751 RVA: 0x001CDF05 File Offset: 0x001CC105
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			public DataRowAction Action
			{
				get
				{
					return this.eventAction;
				}
			}

			// Token: 0x04001D93 RID: 7571
			private SecurityTemplatesDataSet.SecurityTemplatesRow eventRow;

			// Token: 0x04001D94 RID: 7572
			private DataRowAction eventAction;
		}

		// Token: 0x02000638 RID: 1592
		[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
		public class CategoryPermissionsRowChangeEvent : EventArgs
		{
			// Token: 0x06009378 RID: 37752 RVA: 0x001CDF0D File Offset: 0x001CC10D
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			public CategoryPermissionsRowChangeEvent(SecurityTemplatesDataSet.CategoryPermissionsRow row, DataRowAction action)
			{
				this.eventRow = row;
				this.eventAction = action;
			}

			// Token: 0x17002C30 RID: 11312
			// (get) Token: 0x06009379 RID: 37753 RVA: 0x001CDF23 File Offset: 0x001CC123
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public SecurityTemplatesDataSet.CategoryPermissionsRow Row
			{
				get
				{
					return this.eventRow;
				}
			}

			// Token: 0x17002C31 RID: 11313
			// (get) Token: 0x0600937A RID: 37754 RVA: 0x001CDF2B File Offset: 0x001CC12B
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			public DataRowAction Action
			{
				get
				{
					return this.eventAction;
				}
			}

			// Token: 0x04001D95 RID: 7573
			private SecurityTemplatesDataSet.CategoryPermissionsRow eventRow;

			// Token: 0x04001D96 RID: 7574
			private DataRowAction eventAction;
		}

		// Token: 0x02000639 RID: 1593
		[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
		public class GlobalPermissionsRowChangeEvent : EventArgs
		{
			// Token: 0x0600937B RID: 37755 RVA: 0x001CDF33 File Offset: 0x001CC133
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			public GlobalPermissionsRowChangeEvent(SecurityTemplatesDataSet.GlobalPermissionsRow row, DataRowAction action)
			{
				this.eventRow = row;
				this.eventAction = action;
			}

			// Token: 0x17002C32 RID: 11314
			// (get) Token: 0x0600937C RID: 37756 RVA: 0x001CDF49 File Offset: 0x001CC149
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public SecurityTemplatesDataSet.GlobalPermissionsRow Row
			{
				get
				{
					return this.eventRow;
				}
			}

			// Token: 0x17002C33 RID: 11315
			// (get) Token: 0x0600937D RID: 37757 RVA: 0x001CDF51 File Offset: 0x001CC151
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			public DataRowAction Action
			{
				get
				{
					return this.eventAction;
				}
			}

			// Token: 0x04001D97 RID: 7575
			private SecurityTemplatesDataSet.GlobalPermissionsRow eventRow;

			// Token: 0x04001D98 RID: 7576
			private DataRowAction eventAction;
		}
	}
}
