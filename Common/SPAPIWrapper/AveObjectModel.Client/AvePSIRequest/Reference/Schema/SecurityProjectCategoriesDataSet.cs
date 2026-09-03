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
	// Token: 0x02000618 RID: 1560
	[ToolboxItem(true)]
	[HelpKeyword("vs.data.DataSet")]
	[XmlRoot("SecurityProjectCategoriesDataSet")]
	[DesignerCategory("code")]
	[XmlSchemaProvider("GetTypedDataSetSchema")]
	[Serializable]
	public class SecurityProjectCategoriesDataSet : DataSet
	{
		// Token: 0x060091D3 RID: 37331 RVA: 0x001C8A34 File Offset: 0x001C6C34
		public new void EndInit()
		{
			base.EndInit();
			TypedDataSetUtilities.AllowNullsInNonTypedColumns(this.GroupRelations, new string[]
			{
				"WSEC_CAT_UID",
				"WSEC_GRP_UID"
			});
			TypedDataSetUtilities.AllowNullsInNonTypedColumns(this.GroupPermissions, new string[]
			{
				"WSEC_CAT_UID",
				"WSEC_FEA_ACT_UID",
				"WSEC_GRP_UID"
			});
			TypedDataSetUtilities.AllowNullsInNonTypedColumns(this.ProjectCategories, new string[]
			{
				"PROJ_UID",
				"WSEC_CAT_UID"
			});
			TypedDataSetUtilities.AllowNullsInNonTypedColumns(this.UserRelations, new string[]
			{
				"RES_UID",
				"WSEC_CAT_UID"
			});
			TypedDataSetUtilities.AllowNullsInNonTypedColumns(this.UserPermissions, new string[]
			{
				"RES_UID",
				"WSEC_CAT_UID",
				"WSEC_FEA_ACT_UID"
			});
		}

		// Token: 0x060091D4 RID: 37332 RVA: 0x001C8B0C File Offset: 0x001C6D0C
		[DebuggerNonUserCode]
		[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
		public SecurityProjectCategoriesDataSet()
		{
			base.BeginInit();
			this.InitClass();
			CollectionChangeEventHandler value = new CollectionChangeEventHandler(this.SchemaChanged);
			base.Tables.CollectionChanged += value;
			base.Relations.CollectionChanged += value;
			this.EndInit();
		}

		// Token: 0x060091D5 RID: 37333 RVA: 0x001C8B60 File Offset: 0x001C6D60
		[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
		[DebuggerNonUserCode]
		protected SecurityProjectCategoriesDataSet(SerializationInfo info, StreamingContext context) : base(info, context, false)
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
				if (dataSet.Tables["ProjectCategories"] != null)
				{
					base.Tables.Add(new SecurityProjectCategoriesDataSet.ProjectCategoriesDataTable(dataSet.Tables["ProjectCategories"]));
				}
				if (dataSet.Tables["UserRelations"] != null)
				{
					base.Tables.Add(new SecurityProjectCategoriesDataSet.UserRelationsDataTable(dataSet.Tables["UserRelations"]));
				}
				if (dataSet.Tables["GroupRelations"] != null)
				{
					base.Tables.Add(new SecurityProjectCategoriesDataSet.GroupRelationsDataTable(dataSet.Tables["GroupRelations"]));
				}
				if (dataSet.Tables["UserPermissions"] != null)
				{
					base.Tables.Add(new SecurityProjectCategoriesDataSet.UserPermissionsDataTable(dataSet.Tables["UserPermissions"]));
				}
				if (dataSet.Tables["GroupPermissions"] != null)
				{
					base.Tables.Add(new SecurityProjectCategoriesDataSet.GroupPermissionsDataTable(dataSet.Tables["GroupPermissions"]));
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

		// Token: 0x17002BD8 RID: 11224
		// (get) Token: 0x060091D6 RID: 37334 RVA: 0x001C8D85 File Offset: 0x001C6F85
		[Browsable(false)]
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Content)]
		[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
		[DebuggerNonUserCode]
		public SecurityProjectCategoriesDataSet.ProjectCategoriesDataTable ProjectCategories
		{
			get
			{
				return this.tableProjectCategories;
			}
		}

		// Token: 0x17002BD9 RID: 11225
		// (get) Token: 0x060091D7 RID: 37335 RVA: 0x001C8D8D File Offset: 0x001C6F8D
		[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
		[Browsable(false)]
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Content)]
		[DebuggerNonUserCode]
		public SecurityProjectCategoriesDataSet.UserRelationsDataTable UserRelations
		{
			get
			{
				return this.tableUserRelations;
			}
		}

		// Token: 0x17002BDA RID: 11226
		// (get) Token: 0x060091D8 RID: 37336 RVA: 0x001C8D95 File Offset: 0x001C6F95
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Content)]
		[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
		[DebuggerNonUserCode]
		[Browsable(false)]
		public SecurityProjectCategoriesDataSet.GroupRelationsDataTable GroupRelations
		{
			get
			{
				return this.tableGroupRelations;
			}
		}

		// Token: 0x17002BDB RID: 11227
		// (get) Token: 0x060091D9 RID: 37337 RVA: 0x001C8D9D File Offset: 0x001C6F9D
		[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Content)]
		[Browsable(false)]
		[DebuggerNonUserCode]
		public SecurityProjectCategoriesDataSet.UserPermissionsDataTable UserPermissions
		{
			get
			{
				return this.tableUserPermissions;
			}
		}

		// Token: 0x17002BDC RID: 11228
		// (get) Token: 0x060091DA RID: 37338 RVA: 0x001C8DA5 File Offset: 0x001C6FA5
		[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Content)]
		[DebuggerNonUserCode]
		[Browsable(false)]
		public SecurityProjectCategoriesDataSet.GroupPermissionsDataTable GroupPermissions
		{
			get
			{
				return this.tableGroupPermissions;
			}
		}

		// Token: 0x17002BDD RID: 11229
		// (get) Token: 0x060091DB RID: 37339 RVA: 0x001C8DAD File Offset: 0x001C6FAD
		// (set) Token: 0x060091DC RID: 37340 RVA: 0x001C8DB5 File Offset: 0x001C6FB5
		[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
		[Browsable(true)]
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Visible)]
		[DebuggerNonUserCode]
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

		// Token: 0x17002BDE RID: 11230
		// (get) Token: 0x060091DD RID: 37341 RVA: 0x001C8DBE File Offset: 0x001C6FBE
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
		[DebuggerNonUserCode]
		[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
		public new DataTableCollection Tables
		{
			get
			{
				return base.Tables;
			}
		}

		// Token: 0x17002BDF RID: 11231
		// (get) Token: 0x060091DE RID: 37342 RVA: 0x001C8DC6 File Offset: 0x001C6FC6
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

		// Token: 0x060091DF RID: 37343 RVA: 0x001C8DCE File Offset: 0x001C6FCE
		[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
		[DebuggerNonUserCode]
		protected override void InitializeDerivedDataSet()
		{
			base.BeginInit();
			this.InitClass();
			this.EndInit();
		}

		// Token: 0x060091E0 RID: 37344 RVA: 0x001C8DE4 File Offset: 0x001C6FE4
		[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
		[DebuggerNonUserCode]
		public override DataSet Clone()
		{
			SecurityProjectCategoriesDataSet securityProjectCategoriesDataSet = (SecurityProjectCategoriesDataSet)base.Clone();
			securityProjectCategoriesDataSet.InitVars();
			securityProjectCategoriesDataSet.SchemaSerializationMode = this.SchemaSerializationMode;
			return securityProjectCategoriesDataSet;
		}

		// Token: 0x060091E1 RID: 37345 RVA: 0x001C8E10 File Offset: 0x001C7010
		[DebuggerNonUserCode]
		[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
		protected override bool ShouldSerializeTables()
		{
			return false;
		}

		// Token: 0x060091E2 RID: 37346 RVA: 0x001C8E13 File Offset: 0x001C7013
		[DebuggerNonUserCode]
		[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
		protected override bool ShouldSerializeRelations()
		{
			return false;
		}

		// Token: 0x060091E3 RID: 37347 RVA: 0x001C8E18 File Offset: 0x001C7018
		[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
		[DebuggerNonUserCode]
		protected override void ReadXmlSerializable(XmlReader reader)
		{
			if (base.DetermineSchemaSerializationMode(reader) == SchemaSerializationMode.IncludeSchema)
			{
				this.Reset();
				DataSet dataSet = new DataSet();
				dataSet.ReadXml(reader);
				if (dataSet.Tables["ProjectCategories"] != null)
				{
					base.Tables.Add(new SecurityProjectCategoriesDataSet.ProjectCategoriesDataTable(dataSet.Tables["ProjectCategories"]));
				}
				if (dataSet.Tables["UserRelations"] != null)
				{
					base.Tables.Add(new SecurityProjectCategoriesDataSet.UserRelationsDataTable(dataSet.Tables["UserRelations"]));
				}
				if (dataSet.Tables["GroupRelations"] != null)
				{
					base.Tables.Add(new SecurityProjectCategoriesDataSet.GroupRelationsDataTable(dataSet.Tables["GroupRelations"]));
				}
				if (dataSet.Tables["UserPermissions"] != null)
				{
					base.Tables.Add(new SecurityProjectCategoriesDataSet.UserPermissionsDataTable(dataSet.Tables["UserPermissions"]));
				}
				if (dataSet.Tables["GroupPermissions"] != null)
				{
					base.Tables.Add(new SecurityProjectCategoriesDataSet.GroupPermissionsDataTable(dataSet.Tables["GroupPermissions"]));
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

		// Token: 0x060091E4 RID: 37348 RVA: 0x001C8FA8 File Offset: 0x001C71A8
		[DebuggerNonUserCode]
		[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
		protected override XmlSchema GetSchemaSerializable()
		{
			MemoryStream memoryStream = new MemoryStream();
			base.WriteXmlSchema(new XmlTextWriter(memoryStream, null));
			memoryStream.Position = 0L;
			return XmlSchema.Read(new XmlTextReader(memoryStream), null);
		}

		// Token: 0x060091E5 RID: 37349 RVA: 0x001C8FDC File Offset: 0x001C71DC
		[DebuggerNonUserCode]
		[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
		internal void InitVars()
		{
			this.InitVars(true);
		}

		// Token: 0x060091E6 RID: 37350 RVA: 0x001C8FE8 File Offset: 0x001C71E8
		[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
		[DebuggerNonUserCode]
		internal void InitVars(bool initTable)
		{
			this.tableProjectCategories = (SecurityProjectCategoriesDataSet.ProjectCategoriesDataTable)base.Tables["ProjectCategories"];
			if (initTable && this.tableProjectCategories != null)
			{
				this.tableProjectCategories.InitVars();
			}
			this.tableUserRelations = (SecurityProjectCategoriesDataSet.UserRelationsDataTable)base.Tables["UserRelations"];
			if (initTable && this.tableUserRelations != null)
			{
				this.tableUserRelations.InitVars();
			}
			this.tableGroupRelations = (SecurityProjectCategoriesDataSet.GroupRelationsDataTable)base.Tables["GroupRelations"];
			if (initTable && this.tableGroupRelations != null)
			{
				this.tableGroupRelations.InitVars();
			}
			this.tableUserPermissions = (SecurityProjectCategoriesDataSet.UserPermissionsDataTable)base.Tables["UserPermissions"];
			if (initTable && this.tableUserPermissions != null)
			{
				this.tableUserPermissions.InitVars();
			}
			this.tableGroupPermissions = (SecurityProjectCategoriesDataSet.GroupPermissionsDataTable)base.Tables["GroupPermissions"];
			if (initTable && this.tableGroupPermissions != null)
			{
				this.tableGroupPermissions.InitVars();
			}
		}

		// Token: 0x060091E7 RID: 37351 RVA: 0x001C90EC File Offset: 0x001C72EC
		[DebuggerNonUserCode]
		[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
		private void InitClass()
		{
			base.DataSetName = "SecurityProjectCategoriesDataSet";
			base.Prefix = "";
			base.Namespace = "http://schemas.microsoft.com/office/project/server/webservices/SecurityProjectCategoriesDataSet/";
			base.EnforceConstraints = false;
			this.SchemaSerializationMode = SchemaSerializationMode.IncludeSchema;
			this.tableProjectCategories = new SecurityProjectCategoriesDataSet.ProjectCategoriesDataTable();
			base.Tables.Add(this.tableProjectCategories);
			this.tableUserRelations = new SecurityProjectCategoriesDataSet.UserRelationsDataTable();
			base.Tables.Add(this.tableUserRelations);
			this.tableGroupRelations = new SecurityProjectCategoriesDataSet.GroupRelationsDataTable();
			base.Tables.Add(this.tableGroupRelations);
			this.tableUserPermissions = new SecurityProjectCategoriesDataSet.UserPermissionsDataTable();
			base.Tables.Add(this.tableUserPermissions);
			this.tableGroupPermissions = new SecurityProjectCategoriesDataSet.GroupPermissionsDataTable();
			base.Tables.Add(this.tableGroupPermissions);
		}

		// Token: 0x060091E8 RID: 37352 RVA: 0x001C91B4 File Offset: 0x001C73B4
		[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
		[DebuggerNonUserCode]
		private bool ShouldSerializeProjectCategories()
		{
			return false;
		}

		// Token: 0x060091E9 RID: 37353 RVA: 0x001C91B7 File Offset: 0x001C73B7
		[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
		[DebuggerNonUserCode]
		private bool ShouldSerializeUserRelations()
		{
			return false;
		}

		// Token: 0x060091EA RID: 37354 RVA: 0x001C91BA File Offset: 0x001C73BA
		[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
		[DebuggerNonUserCode]
		private bool ShouldSerializeGroupRelations()
		{
			return false;
		}

		// Token: 0x060091EB RID: 37355 RVA: 0x001C91BD File Offset: 0x001C73BD
		[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
		[DebuggerNonUserCode]
		private bool ShouldSerializeUserPermissions()
		{
			return false;
		}

		// Token: 0x060091EC RID: 37356 RVA: 0x001C91C0 File Offset: 0x001C73C0
		[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
		[DebuggerNonUserCode]
		private bool ShouldSerializeGroupPermissions()
		{
			return false;
		}

		// Token: 0x060091ED RID: 37357 RVA: 0x001C91C3 File Offset: 0x001C73C3
		[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
		[DebuggerNonUserCode]
		private void SchemaChanged(object sender, CollectionChangeEventArgs e)
		{
			if (e.Action == CollectionChangeAction.Remove)
			{
				this.InitVars();
			}
		}

		// Token: 0x060091EE RID: 37358 RVA: 0x001C91D4 File Offset: 0x001C73D4
		[DebuggerNonUserCode]
		[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
		public static XmlSchemaComplexType GetTypedDataSetSchema(XmlSchemaSet xs)
		{
			SecurityProjectCategoriesDataSet securityProjectCategoriesDataSet = new SecurityProjectCategoriesDataSet();
			XmlSchemaComplexType xmlSchemaComplexType = new XmlSchemaComplexType();
			XmlSchemaSequence xmlSchemaSequence = new XmlSchemaSequence();
			XmlSchemaAny xmlSchemaAny = new XmlSchemaAny();
			xmlSchemaAny.Namespace = securityProjectCategoriesDataSet.Namespace;
			xmlSchemaSequence.Items.Add(xmlSchemaAny);
			xmlSchemaComplexType.Particle = xmlSchemaSequence;
			XmlSchema schemaSerializable = securityProjectCategoriesDataSet.GetSchemaSerializable();
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

		// Token: 0x04001D40 RID: 7488
		private SecurityProjectCategoriesDataSet.ProjectCategoriesDataTable tableProjectCategories;

		// Token: 0x04001D41 RID: 7489
		private SecurityProjectCategoriesDataSet.UserRelationsDataTable tableUserRelations;

		// Token: 0x04001D42 RID: 7490
		private SecurityProjectCategoriesDataSet.GroupRelationsDataTable tableGroupRelations;

		// Token: 0x04001D43 RID: 7491
		private SecurityProjectCategoriesDataSet.UserPermissionsDataTable tableUserPermissions;

		// Token: 0x04001D44 RID: 7492
		private SecurityProjectCategoriesDataSet.GroupPermissionsDataTable tableGroupPermissions;

		// Token: 0x04001D45 RID: 7493
		private SchemaSerializationMode _schemaSerializationMode = SchemaSerializationMode.IncludeSchema;

		// Token: 0x02000619 RID: 1561
		// (Invoke) Token: 0x060091F0 RID: 37360
		[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
		public delegate void ProjectCategoriesRowChangeEventHandler(object sender, SecurityProjectCategoriesDataSet.ProjectCategoriesRowChangeEvent e);

		// Token: 0x0200061A RID: 1562
		// (Invoke) Token: 0x060091F4 RID: 37364
		[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
		public delegate void UserRelationsRowChangeEventHandler(object sender, SecurityProjectCategoriesDataSet.UserRelationsRowChangeEvent e);

		// Token: 0x0200061B RID: 1563
		// (Invoke) Token: 0x060091F8 RID: 37368
		[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
		public delegate void GroupRelationsRowChangeEventHandler(object sender, SecurityProjectCategoriesDataSet.GroupRelationsRowChangeEvent e);

		// Token: 0x0200061C RID: 1564
		// (Invoke) Token: 0x060091FC RID: 37372
		[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
		public delegate void UserPermissionsRowChangeEventHandler(object sender, SecurityProjectCategoriesDataSet.UserPermissionsRowChangeEvent e);

		// Token: 0x0200061D RID: 1565
		// (Invoke) Token: 0x06009200 RID: 37376
		[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
		public delegate void GroupPermissionsRowChangeEventHandler(object sender, SecurityProjectCategoriesDataSet.GroupPermissionsRowChangeEvent e);

		// Token: 0x0200061E RID: 1566
		[XmlSchemaProvider("GetTypedTableSchema")]
		[Serializable]
		public class ProjectCategoriesDataTable : DataTable, IEnumerable
		{
			// Token: 0x06009203 RID: 37379 RVA: 0x001C931C File Offset: 0x001C751C
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			public ProjectCategoriesDataTable()
			{
				base.TableName = "ProjectCategories";
				this.BeginInit();
				this.InitClass();
				this.EndInit();
			}

			// Token: 0x06009204 RID: 37380 RVA: 0x001C9344 File Offset: 0x001C7544
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			internal ProjectCategoriesDataTable(DataTable table)
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

			// Token: 0x06009205 RID: 37381 RVA: 0x001C93EC File Offset: 0x001C75EC
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			protected ProjectCategoriesDataTable(SerializationInfo info, StreamingContext context) : base(info, context)
			{
				this.InitVars();
			}

			// Token: 0x17002BE0 RID: 11232
			// (get) Token: 0x06009206 RID: 37382 RVA: 0x001C93FC File Offset: 0x001C75FC
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public DataColumn WSEC_CAT_UIDColumn
			{
				get
				{
					return this.columnWSEC_CAT_UID;
				}
			}

			// Token: 0x17002BE1 RID: 11233
			// (get) Token: 0x06009207 RID: 37383 RVA: 0x001C9404 File Offset: 0x001C7604
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public DataColumn PROJ_UIDColumn
			{
				get
				{
					return this.columnPROJ_UID;
				}
			}

			// Token: 0x17002BE2 RID: 11234
			// (get) Token: 0x06009208 RID: 37384 RVA: 0x001C940C File Offset: 0x001C760C
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

			// Token: 0x17002BE3 RID: 11235
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public SecurityProjectCategoriesDataSet.ProjectCategoriesRow this[int index]
			{
				get
				{
					return (SecurityProjectCategoriesDataSet.ProjectCategoriesRow)base.Rows[index];
				}
			}

			// Token: 0x14000545 RID: 1349
			// (add) Token: 0x0600920A RID: 37386 RVA: 0x001C942C File Offset: 0x001C762C
			// (remove) Token: 0x0600920B RID: 37387 RVA: 0x001C9464 File Offset: 0x001C7664
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public event SecurityProjectCategoriesDataSet.ProjectCategoriesRowChangeEventHandler ProjectCategoriesRowChanging;

			// Token: 0x14000546 RID: 1350
			// (add) Token: 0x0600920C RID: 37388 RVA: 0x001C949C File Offset: 0x001C769C
			// (remove) Token: 0x0600920D RID: 37389 RVA: 0x001C94D4 File Offset: 0x001C76D4
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public event SecurityProjectCategoriesDataSet.ProjectCategoriesRowChangeEventHandler ProjectCategoriesRowChanged;

			// Token: 0x14000547 RID: 1351
			// (add) Token: 0x0600920E RID: 37390 RVA: 0x001C950C File Offset: 0x001C770C
			// (remove) Token: 0x0600920F RID: 37391 RVA: 0x001C9544 File Offset: 0x001C7744
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public event SecurityProjectCategoriesDataSet.ProjectCategoriesRowChangeEventHandler ProjectCategoriesRowDeleting;

			// Token: 0x14000548 RID: 1352
			// (add) Token: 0x06009210 RID: 37392 RVA: 0x001C957C File Offset: 0x001C777C
			// (remove) Token: 0x06009211 RID: 37393 RVA: 0x001C95B4 File Offset: 0x001C77B4
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public event SecurityProjectCategoriesDataSet.ProjectCategoriesRowChangeEventHandler ProjectCategoriesRowDeleted;

			// Token: 0x06009212 RID: 37394 RVA: 0x001C95E9 File Offset: 0x001C77E9
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			public void AddProjectCategoriesRow(SecurityProjectCategoriesDataSet.ProjectCategoriesRow row)
			{
				base.Rows.Add(row);
			}

			// Token: 0x06009213 RID: 37395 RVA: 0x001C95F8 File Offset: 0x001C77F8
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public SecurityProjectCategoriesDataSet.ProjectCategoriesRow AddProjectCategoriesRow(Guid WSEC_CAT_UID, Guid PROJ_UID)
			{
				SecurityProjectCategoriesDataSet.ProjectCategoriesRow projectCategoriesRow = (SecurityProjectCategoriesDataSet.ProjectCategoriesRow)base.NewRow();
				object[] itemArray = new object[]
				{
					WSEC_CAT_UID,
					PROJ_UID
				};
				projectCategoriesRow.ItemArray = itemArray;
				base.Rows.Add(projectCategoriesRow);
				return projectCategoriesRow;
			}

			// Token: 0x06009214 RID: 37396 RVA: 0x001C9640 File Offset: 0x001C7840
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			public SecurityProjectCategoriesDataSet.ProjectCategoriesRow FindByWSEC_CAT_UID(Guid WSEC_CAT_UID)
			{
				return (SecurityProjectCategoriesDataSet.ProjectCategoriesRow)base.Rows.Find(new object[]
				{
					WSEC_CAT_UID
				});
			}

			// Token: 0x06009215 RID: 37397 RVA: 0x001C966E File Offset: 0x001C786E
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			public virtual IEnumerator GetEnumerator()
			{
				return base.Rows.GetEnumerator();
			}

			// Token: 0x06009216 RID: 37398 RVA: 0x001C967C File Offset: 0x001C787C
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public override DataTable Clone()
			{
				SecurityProjectCategoriesDataSet.ProjectCategoriesDataTable projectCategoriesDataTable = (SecurityProjectCategoriesDataSet.ProjectCategoriesDataTable)base.Clone();
				projectCategoriesDataTable.InitVars();
				return projectCategoriesDataTable;
			}

			// Token: 0x06009217 RID: 37399 RVA: 0x001C969C File Offset: 0x001C789C
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			protected override DataTable CreateInstance()
			{
				return new SecurityProjectCategoriesDataSet.ProjectCategoriesDataTable();
			}

			// Token: 0x06009218 RID: 37400 RVA: 0x001C96A3 File Offset: 0x001C78A3
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			internal void InitVars()
			{
				this.columnWSEC_CAT_UID = base.Columns["WSEC_CAT_UID"];
				this.columnPROJ_UID = base.Columns["PROJ_UID"];
			}

			// Token: 0x06009219 RID: 37401 RVA: 0x001C96D4 File Offset: 0x001C78D4
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			private void InitClass()
			{
				this.columnWSEC_CAT_UID = new DataColumn("WSEC_CAT_UID", typeof(Guid), null, MappingType.Element);
				base.Columns.Add(this.columnWSEC_CAT_UID);
				this.columnPROJ_UID = new DataColumn("PROJ_UID", typeof(Guid), null, MappingType.Element);
				base.Columns.Add(this.columnPROJ_UID);
				base.Constraints.Add(new UniqueConstraint("ProjectCategoriesPrimaryKey", new DataColumn[]
				{
					this.columnWSEC_CAT_UID
				}, true));
				this.columnWSEC_CAT_UID.AllowDBNull = false;
				this.columnWSEC_CAT_UID.Unique = true;
				this.columnPROJ_UID.AllowDBNull = false;
			}

			// Token: 0x0600921A RID: 37402 RVA: 0x001C9786 File Offset: 0x001C7986
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			public SecurityProjectCategoriesDataSet.ProjectCategoriesRow NewProjectCategoriesRow()
			{
				return (SecurityProjectCategoriesDataSet.ProjectCategoriesRow)base.NewRow();
			}

			// Token: 0x0600921B RID: 37403 RVA: 0x001C9793 File Offset: 0x001C7993
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			protected override DataRow NewRowFromBuilder(DataRowBuilder builder)
			{
				return new SecurityProjectCategoriesDataSet.ProjectCategoriesRow(builder);
			}

			// Token: 0x0600921C RID: 37404 RVA: 0x001C979B File Offset: 0x001C799B
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			protected override Type GetRowType()
			{
				return typeof(SecurityProjectCategoriesDataSet.ProjectCategoriesRow);
			}

			// Token: 0x0600921D RID: 37405 RVA: 0x001C97A7 File Offset: 0x001C79A7
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			protected override void OnRowChanged(DataRowChangeEventArgs e)
			{
				base.OnRowChanged(e);
				if (this.ProjectCategoriesRowChanged != null)
				{
					this.ProjectCategoriesRowChanged(this, new SecurityProjectCategoriesDataSet.ProjectCategoriesRowChangeEvent((SecurityProjectCategoriesDataSet.ProjectCategoriesRow)e.Row, e.Action));
				}
			}

			// Token: 0x0600921E RID: 37406 RVA: 0x001C97DA File Offset: 0x001C79DA
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			protected override void OnRowChanging(DataRowChangeEventArgs e)
			{
				base.OnRowChanging(e);
				if (this.ProjectCategoriesRowChanging != null)
				{
					this.ProjectCategoriesRowChanging(this, new SecurityProjectCategoriesDataSet.ProjectCategoriesRowChangeEvent((SecurityProjectCategoriesDataSet.ProjectCategoriesRow)e.Row, e.Action));
				}
			}

			// Token: 0x0600921F RID: 37407 RVA: 0x001C980D File Offset: 0x001C7A0D
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			protected override void OnRowDeleted(DataRowChangeEventArgs e)
			{
				base.OnRowDeleted(e);
				if (this.ProjectCategoriesRowDeleted != null)
				{
					this.ProjectCategoriesRowDeleted(this, new SecurityProjectCategoriesDataSet.ProjectCategoriesRowChangeEvent((SecurityProjectCategoriesDataSet.ProjectCategoriesRow)e.Row, e.Action));
				}
			}

			// Token: 0x06009220 RID: 37408 RVA: 0x001C9840 File Offset: 0x001C7A40
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			protected override void OnRowDeleting(DataRowChangeEventArgs e)
			{
				base.OnRowDeleting(e);
				if (this.ProjectCategoriesRowDeleting != null)
				{
					this.ProjectCategoriesRowDeleting(this, new SecurityProjectCategoriesDataSet.ProjectCategoriesRowChangeEvent((SecurityProjectCategoriesDataSet.ProjectCategoriesRow)e.Row, e.Action));
				}
			}

			// Token: 0x06009221 RID: 37409 RVA: 0x001C9873 File Offset: 0x001C7A73
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public void RemoveProjectCategoriesRow(SecurityProjectCategoriesDataSet.ProjectCategoriesRow row)
			{
				base.Rows.Remove(row);
			}

			// Token: 0x06009222 RID: 37410 RVA: 0x001C9884 File Offset: 0x001C7A84
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			public static XmlSchemaComplexType GetTypedTableSchema(XmlSchemaSet xs)
			{
				XmlSchemaComplexType xmlSchemaComplexType = new XmlSchemaComplexType();
				XmlSchemaSequence xmlSchemaSequence = new XmlSchemaSequence();
				SecurityProjectCategoriesDataSet securityProjectCategoriesDataSet = new SecurityProjectCategoriesDataSet();
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
				xmlSchemaAttribute.FixedValue = securityProjectCategoriesDataSet.Namespace;
				xmlSchemaComplexType.Attributes.Add(xmlSchemaAttribute);
				XmlSchemaAttribute xmlSchemaAttribute2 = new XmlSchemaAttribute();
				xmlSchemaAttribute2.Name = "tableTypeName";
				xmlSchemaAttribute2.FixedValue = "ProjectCategoriesDataTable";
				xmlSchemaComplexType.Attributes.Add(xmlSchemaAttribute2);
				xmlSchemaComplexType.Particle = xmlSchemaSequence;
				XmlSchema schemaSerializable = securityProjectCategoriesDataSet.GetSchemaSerializable();
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

			// Token: 0x04001D46 RID: 7494
			private DataColumn columnWSEC_CAT_UID;

			// Token: 0x04001D47 RID: 7495
			private DataColumn columnPROJ_UID;
		}

		// Token: 0x0200061F RID: 1567
		[XmlSchemaProvider("GetTypedTableSchema")]
		[Serializable]
		public class UserRelationsDataTable : DataTable, IEnumerable
		{
			// Token: 0x06009223 RID: 37411 RVA: 0x001C9A7C File Offset: 0x001C7C7C
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			public UserRelationsDataTable()
			{
				base.TableName = "UserRelations";
				this.BeginInit();
				this.InitClass();
				this.EndInit();
			}

			// Token: 0x06009224 RID: 37412 RVA: 0x001C9AA4 File Offset: 0x001C7CA4
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			internal UserRelationsDataTable(DataTable table)
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

			// Token: 0x06009225 RID: 37413 RVA: 0x001C9B4C File Offset: 0x001C7D4C
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			protected UserRelationsDataTable(SerializationInfo info, StreamingContext context) : base(info, context)
			{
				this.InitVars();
			}

			// Token: 0x17002BE4 RID: 11236
			// (get) Token: 0x06009226 RID: 37414 RVA: 0x001C9B5C File Offset: 0x001C7D5C
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public DataColumn WSEC_CAT_UIDColumn
			{
				get
				{
					return this.columnWSEC_CAT_UID;
				}
			}

			// Token: 0x17002BE5 RID: 11237
			// (get) Token: 0x06009227 RID: 37415 RVA: 0x001C9B64 File Offset: 0x001C7D64
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public DataColumn RES_UIDColumn
			{
				get
				{
					return this.columnRES_UID;
				}
			}

			// Token: 0x17002BE6 RID: 11238
			// (get) Token: 0x06009228 RID: 37416 RVA: 0x001C9B6C File Offset: 0x001C7D6C
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

			// Token: 0x17002BE7 RID: 11239
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			public SecurityProjectCategoriesDataSet.UserRelationsRow this[int index]
			{
				get
				{
					return (SecurityProjectCategoriesDataSet.UserRelationsRow)base.Rows[index];
				}
			}

			// Token: 0x14000549 RID: 1353
			// (add) Token: 0x0600922A RID: 37418 RVA: 0x001C9B8C File Offset: 0x001C7D8C
			// (remove) Token: 0x0600922B RID: 37419 RVA: 0x001C9BC4 File Offset: 0x001C7DC4
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public event SecurityProjectCategoriesDataSet.UserRelationsRowChangeEventHandler UserRelationsRowChanging;

			// Token: 0x1400054A RID: 1354
			// (add) Token: 0x0600922C RID: 37420 RVA: 0x001C9BFC File Offset: 0x001C7DFC
			// (remove) Token: 0x0600922D RID: 37421 RVA: 0x001C9C34 File Offset: 0x001C7E34
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public event SecurityProjectCategoriesDataSet.UserRelationsRowChangeEventHandler UserRelationsRowChanged;

			// Token: 0x1400054B RID: 1355
			// (add) Token: 0x0600922E RID: 37422 RVA: 0x001C9C6C File Offset: 0x001C7E6C
			// (remove) Token: 0x0600922F RID: 37423 RVA: 0x001C9CA4 File Offset: 0x001C7EA4
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public event SecurityProjectCategoriesDataSet.UserRelationsRowChangeEventHandler UserRelationsRowDeleting;

			// Token: 0x1400054C RID: 1356
			// (add) Token: 0x06009230 RID: 37424 RVA: 0x001C9CDC File Offset: 0x001C7EDC
			// (remove) Token: 0x06009231 RID: 37425 RVA: 0x001C9D14 File Offset: 0x001C7F14
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public event SecurityProjectCategoriesDataSet.UserRelationsRowChangeEventHandler UserRelationsRowDeleted;

			// Token: 0x06009232 RID: 37426 RVA: 0x001C9D49 File Offset: 0x001C7F49
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			public void AddUserRelationsRow(SecurityProjectCategoriesDataSet.UserRelationsRow row)
			{
				base.Rows.Add(row);
			}

			// Token: 0x06009233 RID: 37427 RVA: 0x001C9D58 File Offset: 0x001C7F58
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			public SecurityProjectCategoriesDataSet.UserRelationsRow AddUserRelationsRow(Guid WSEC_CAT_UID, Guid RES_UID)
			{
				SecurityProjectCategoriesDataSet.UserRelationsRow userRelationsRow = (SecurityProjectCategoriesDataSet.UserRelationsRow)base.NewRow();
				object[] itemArray = new object[]
				{
					WSEC_CAT_UID,
					RES_UID
				};
				userRelationsRow.ItemArray = itemArray;
				base.Rows.Add(userRelationsRow);
				return userRelationsRow;
			}

			// Token: 0x06009234 RID: 37428 RVA: 0x001C9DA0 File Offset: 0x001C7FA0
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public SecurityProjectCategoriesDataSet.UserRelationsRow FindByWSEC_CAT_UIDRES_UID(Guid WSEC_CAT_UID, Guid RES_UID)
			{
				return (SecurityProjectCategoriesDataSet.UserRelationsRow)base.Rows.Find(new object[]
				{
					WSEC_CAT_UID,
					RES_UID
				});
			}

			// Token: 0x06009235 RID: 37429 RVA: 0x001C9DD7 File Offset: 0x001C7FD7
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public virtual IEnumerator GetEnumerator()
			{
				return base.Rows.GetEnumerator();
			}

			// Token: 0x06009236 RID: 37430 RVA: 0x001C9DE4 File Offset: 0x001C7FE4
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public override DataTable Clone()
			{
				SecurityProjectCategoriesDataSet.UserRelationsDataTable userRelationsDataTable = (SecurityProjectCategoriesDataSet.UserRelationsDataTable)base.Clone();
				userRelationsDataTable.InitVars();
				return userRelationsDataTable;
			}

			// Token: 0x06009237 RID: 37431 RVA: 0x001C9E04 File Offset: 0x001C8004
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			protected override DataTable CreateInstance()
			{
				return new SecurityProjectCategoriesDataSet.UserRelationsDataTable();
			}

			// Token: 0x06009238 RID: 37432 RVA: 0x001C9E0B File Offset: 0x001C800B
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			internal void InitVars()
			{
				this.columnWSEC_CAT_UID = base.Columns["WSEC_CAT_UID"];
				this.columnRES_UID = base.Columns["RES_UID"];
			}

			// Token: 0x06009239 RID: 37433 RVA: 0x001C9E3C File Offset: 0x001C803C
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			private void InitClass()
			{
				this.columnWSEC_CAT_UID = new DataColumn("WSEC_CAT_UID", typeof(Guid), null, MappingType.Element);
				base.Columns.Add(this.columnWSEC_CAT_UID);
				this.columnRES_UID = new DataColumn("RES_UID", typeof(Guid), null, MappingType.Element);
				base.Columns.Add(this.columnRES_UID);
				base.Constraints.Add(new UniqueConstraint("UserRelationsPrimaryKey", new DataColumn[]
				{
					this.columnWSEC_CAT_UID,
					this.columnRES_UID
				}, true));
				this.columnWSEC_CAT_UID.AllowDBNull = false;
				this.columnRES_UID.AllowDBNull = false;
			}

			// Token: 0x0600923A RID: 37434 RVA: 0x001C9EEB File Offset: 0x001C80EB
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public SecurityProjectCategoriesDataSet.UserRelationsRow NewUserRelationsRow()
			{
				return (SecurityProjectCategoriesDataSet.UserRelationsRow)base.NewRow();
			}

			// Token: 0x0600923B RID: 37435 RVA: 0x001C9EF8 File Offset: 0x001C80F8
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			protected override DataRow NewRowFromBuilder(DataRowBuilder builder)
			{
				return new SecurityProjectCategoriesDataSet.UserRelationsRow(builder);
			}

			// Token: 0x0600923C RID: 37436 RVA: 0x001C9F00 File Offset: 0x001C8100
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			protected override Type GetRowType()
			{
				return typeof(SecurityProjectCategoriesDataSet.UserRelationsRow);
			}

			// Token: 0x0600923D RID: 37437 RVA: 0x001C9F0C File Offset: 0x001C810C
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			protected override void OnRowChanged(DataRowChangeEventArgs e)
			{
				base.OnRowChanged(e);
				if (this.UserRelationsRowChanged != null)
				{
					this.UserRelationsRowChanged(this, new SecurityProjectCategoriesDataSet.UserRelationsRowChangeEvent((SecurityProjectCategoriesDataSet.UserRelationsRow)e.Row, e.Action));
				}
			}

			// Token: 0x0600923E RID: 37438 RVA: 0x001C9F3F File Offset: 0x001C813F
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			protected override void OnRowChanging(DataRowChangeEventArgs e)
			{
				base.OnRowChanging(e);
				if (this.UserRelationsRowChanging != null)
				{
					this.UserRelationsRowChanging(this, new SecurityProjectCategoriesDataSet.UserRelationsRowChangeEvent((SecurityProjectCategoriesDataSet.UserRelationsRow)e.Row, e.Action));
				}
			}

			// Token: 0x0600923F RID: 37439 RVA: 0x001C9F72 File Offset: 0x001C8172
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			protected override void OnRowDeleted(DataRowChangeEventArgs e)
			{
				base.OnRowDeleted(e);
				if (this.UserRelationsRowDeleted != null)
				{
					this.UserRelationsRowDeleted(this, new SecurityProjectCategoriesDataSet.UserRelationsRowChangeEvent((SecurityProjectCategoriesDataSet.UserRelationsRow)e.Row, e.Action));
				}
			}

			// Token: 0x06009240 RID: 37440 RVA: 0x001C9FA5 File Offset: 0x001C81A5
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			protected override void OnRowDeleting(DataRowChangeEventArgs e)
			{
				base.OnRowDeleting(e);
				if (this.UserRelationsRowDeleting != null)
				{
					this.UserRelationsRowDeleting(this, new SecurityProjectCategoriesDataSet.UserRelationsRowChangeEvent((SecurityProjectCategoriesDataSet.UserRelationsRow)e.Row, e.Action));
				}
			}

			// Token: 0x06009241 RID: 37441 RVA: 0x001C9FD8 File Offset: 0x001C81D8
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			public void RemoveUserRelationsRow(SecurityProjectCategoriesDataSet.UserRelationsRow row)
			{
				base.Rows.Remove(row);
			}

			// Token: 0x06009242 RID: 37442 RVA: 0x001C9FE8 File Offset: 0x001C81E8
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public static XmlSchemaComplexType GetTypedTableSchema(XmlSchemaSet xs)
			{
				XmlSchemaComplexType xmlSchemaComplexType = new XmlSchemaComplexType();
				XmlSchemaSequence xmlSchemaSequence = new XmlSchemaSequence();
				SecurityProjectCategoriesDataSet securityProjectCategoriesDataSet = new SecurityProjectCategoriesDataSet();
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
				xmlSchemaAttribute.FixedValue = securityProjectCategoriesDataSet.Namespace;
				xmlSchemaComplexType.Attributes.Add(xmlSchemaAttribute);
				XmlSchemaAttribute xmlSchemaAttribute2 = new XmlSchemaAttribute();
				xmlSchemaAttribute2.Name = "tableTypeName";
				xmlSchemaAttribute2.FixedValue = "UserRelationsDataTable";
				xmlSchemaComplexType.Attributes.Add(xmlSchemaAttribute2);
				xmlSchemaComplexType.Particle = xmlSchemaSequence;
				XmlSchema schemaSerializable = securityProjectCategoriesDataSet.GetSchemaSerializable();
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

			// Token: 0x04001D4C RID: 7500
			private DataColumn columnWSEC_CAT_UID;

			// Token: 0x04001D4D RID: 7501
			private DataColumn columnRES_UID;
		}

		// Token: 0x02000620 RID: 1568
		[XmlSchemaProvider("GetTypedTableSchema")]
		[Serializable]
		public class GroupRelationsDataTable : DataTable, IEnumerable
		{
			// Token: 0x06009243 RID: 37443 RVA: 0x001CA1E0 File Offset: 0x001C83E0
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			public GroupRelationsDataTable()
			{
				base.TableName = "GroupRelations";
				this.BeginInit();
				this.InitClass();
				this.EndInit();
			}

			// Token: 0x06009244 RID: 37444 RVA: 0x001CA208 File Offset: 0x001C8408
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			internal GroupRelationsDataTable(DataTable table)
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

			// Token: 0x06009245 RID: 37445 RVA: 0x001CA2B0 File Offset: 0x001C84B0
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			protected GroupRelationsDataTable(SerializationInfo info, StreamingContext context) : base(info, context)
			{
				this.InitVars();
			}

			// Token: 0x17002BE8 RID: 11240
			// (get) Token: 0x06009246 RID: 37446 RVA: 0x001CA2C0 File Offset: 0x001C84C0
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			public DataColumn WSEC_CAT_UIDColumn
			{
				get
				{
					return this.columnWSEC_CAT_UID;
				}
			}

			// Token: 0x17002BE9 RID: 11241
			// (get) Token: 0x06009247 RID: 37447 RVA: 0x001CA2C8 File Offset: 0x001C84C8
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public DataColumn WSEC_GRP_UIDColumn
			{
				get
				{
					return this.columnWSEC_GRP_UID;
				}
			}

			// Token: 0x17002BEA RID: 11242
			// (get) Token: 0x06009248 RID: 37448 RVA: 0x001CA2D0 File Offset: 0x001C84D0
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

			// Token: 0x17002BEB RID: 11243
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public SecurityProjectCategoriesDataSet.GroupRelationsRow this[int index]
			{
				get
				{
					return (SecurityProjectCategoriesDataSet.GroupRelationsRow)base.Rows[index];
				}
			}

			// Token: 0x1400054D RID: 1357
			// (add) Token: 0x0600924A RID: 37450 RVA: 0x001CA2F0 File Offset: 0x001C84F0
			// (remove) Token: 0x0600924B RID: 37451 RVA: 0x001CA328 File Offset: 0x001C8528
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public event SecurityProjectCategoriesDataSet.GroupRelationsRowChangeEventHandler GroupRelationsRowChanging;

			// Token: 0x1400054E RID: 1358
			// (add) Token: 0x0600924C RID: 37452 RVA: 0x001CA360 File Offset: 0x001C8560
			// (remove) Token: 0x0600924D RID: 37453 RVA: 0x001CA398 File Offset: 0x001C8598
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public event SecurityProjectCategoriesDataSet.GroupRelationsRowChangeEventHandler GroupRelationsRowChanged;

			// Token: 0x1400054F RID: 1359
			// (add) Token: 0x0600924E RID: 37454 RVA: 0x001CA3D0 File Offset: 0x001C85D0
			// (remove) Token: 0x0600924F RID: 37455 RVA: 0x001CA408 File Offset: 0x001C8608
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public event SecurityProjectCategoriesDataSet.GroupRelationsRowChangeEventHandler GroupRelationsRowDeleting;

			// Token: 0x14000550 RID: 1360
			// (add) Token: 0x06009250 RID: 37456 RVA: 0x001CA440 File Offset: 0x001C8640
			// (remove) Token: 0x06009251 RID: 37457 RVA: 0x001CA478 File Offset: 0x001C8678
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public event SecurityProjectCategoriesDataSet.GroupRelationsRowChangeEventHandler GroupRelationsRowDeleted;

			// Token: 0x06009252 RID: 37458 RVA: 0x001CA4AD File Offset: 0x001C86AD
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public void AddGroupRelationsRow(SecurityProjectCategoriesDataSet.GroupRelationsRow row)
			{
				base.Rows.Add(row);
			}

			// Token: 0x06009253 RID: 37459 RVA: 0x001CA4BC File Offset: 0x001C86BC
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			public SecurityProjectCategoriesDataSet.GroupRelationsRow AddGroupRelationsRow(Guid WSEC_CAT_UID, Guid WSEC_GRP_UID)
			{
				SecurityProjectCategoriesDataSet.GroupRelationsRow groupRelationsRow = (SecurityProjectCategoriesDataSet.GroupRelationsRow)base.NewRow();
				object[] itemArray = new object[]
				{
					WSEC_CAT_UID,
					WSEC_GRP_UID
				};
				groupRelationsRow.ItemArray = itemArray;
				base.Rows.Add(groupRelationsRow);
				return groupRelationsRow;
			}

			// Token: 0x06009254 RID: 37460 RVA: 0x001CA504 File Offset: 0x001C8704
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			public SecurityProjectCategoriesDataSet.GroupRelationsRow FindByWSEC_CAT_UIDWSEC_GRP_UID(Guid WSEC_CAT_UID, Guid WSEC_GRP_UID)
			{
				return (SecurityProjectCategoriesDataSet.GroupRelationsRow)base.Rows.Find(new object[]
				{
					WSEC_CAT_UID,
					WSEC_GRP_UID
				});
			}

			// Token: 0x06009255 RID: 37461 RVA: 0x001CA53B File Offset: 0x001C873B
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			public virtual IEnumerator GetEnumerator()
			{
				return base.Rows.GetEnumerator();
			}

			// Token: 0x06009256 RID: 37462 RVA: 0x001CA548 File Offset: 0x001C8748
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public override DataTable Clone()
			{
				SecurityProjectCategoriesDataSet.GroupRelationsDataTable groupRelationsDataTable = (SecurityProjectCategoriesDataSet.GroupRelationsDataTable)base.Clone();
				groupRelationsDataTable.InitVars();
				return groupRelationsDataTable;
			}

			// Token: 0x06009257 RID: 37463 RVA: 0x001CA568 File Offset: 0x001C8768
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			protected override DataTable CreateInstance()
			{
				return new SecurityProjectCategoriesDataSet.GroupRelationsDataTable();
			}

			// Token: 0x06009258 RID: 37464 RVA: 0x001CA56F File Offset: 0x001C876F
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			internal void InitVars()
			{
				this.columnWSEC_CAT_UID = base.Columns["WSEC_CAT_UID"];
				this.columnWSEC_GRP_UID = base.Columns["WSEC_GRP_UID"];
			}

			// Token: 0x06009259 RID: 37465 RVA: 0x001CA5A0 File Offset: 0x001C87A0
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			private void InitClass()
			{
				this.columnWSEC_CAT_UID = new DataColumn("WSEC_CAT_UID", typeof(Guid), null, MappingType.Element);
				base.Columns.Add(this.columnWSEC_CAT_UID);
				this.columnWSEC_GRP_UID = new DataColumn("WSEC_GRP_UID", typeof(Guid), null, MappingType.Element);
				base.Columns.Add(this.columnWSEC_GRP_UID);
				base.Constraints.Add(new UniqueConstraint("GroupRelationsPrimaryKey", new DataColumn[]
				{
					this.columnWSEC_CAT_UID,
					this.columnWSEC_GRP_UID
				}, true));
				this.columnWSEC_CAT_UID.AllowDBNull = false;
				this.columnWSEC_GRP_UID.AllowDBNull = false;
			}

			// Token: 0x0600925A RID: 37466 RVA: 0x001CA64F File Offset: 0x001C884F
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public SecurityProjectCategoriesDataSet.GroupRelationsRow NewGroupRelationsRow()
			{
				return (SecurityProjectCategoriesDataSet.GroupRelationsRow)base.NewRow();
			}

			// Token: 0x0600925B RID: 37467 RVA: 0x001CA65C File Offset: 0x001C885C
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			protected override DataRow NewRowFromBuilder(DataRowBuilder builder)
			{
				return new SecurityProjectCategoriesDataSet.GroupRelationsRow(builder);
			}

			// Token: 0x0600925C RID: 37468 RVA: 0x001CA664 File Offset: 0x001C8864
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			protected override Type GetRowType()
			{
				return typeof(SecurityProjectCategoriesDataSet.GroupRelationsRow);
			}

			// Token: 0x0600925D RID: 37469 RVA: 0x001CA670 File Offset: 0x001C8870
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			protected override void OnRowChanged(DataRowChangeEventArgs e)
			{
				base.OnRowChanged(e);
				if (this.GroupRelationsRowChanged != null)
				{
					this.GroupRelationsRowChanged(this, new SecurityProjectCategoriesDataSet.GroupRelationsRowChangeEvent((SecurityProjectCategoriesDataSet.GroupRelationsRow)e.Row, e.Action));
				}
			}

			// Token: 0x0600925E RID: 37470 RVA: 0x001CA6A3 File Offset: 0x001C88A3
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			protected override void OnRowChanging(DataRowChangeEventArgs e)
			{
				base.OnRowChanging(e);
				if (this.GroupRelationsRowChanging != null)
				{
					this.GroupRelationsRowChanging(this, new SecurityProjectCategoriesDataSet.GroupRelationsRowChangeEvent((SecurityProjectCategoriesDataSet.GroupRelationsRow)e.Row, e.Action));
				}
			}

			// Token: 0x0600925F RID: 37471 RVA: 0x001CA6D6 File Offset: 0x001C88D6
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			protected override void OnRowDeleted(DataRowChangeEventArgs e)
			{
				base.OnRowDeleted(e);
				if (this.GroupRelationsRowDeleted != null)
				{
					this.GroupRelationsRowDeleted(this, new SecurityProjectCategoriesDataSet.GroupRelationsRowChangeEvent((SecurityProjectCategoriesDataSet.GroupRelationsRow)e.Row, e.Action));
				}
			}

			// Token: 0x06009260 RID: 37472 RVA: 0x001CA709 File Offset: 0x001C8909
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			protected override void OnRowDeleting(DataRowChangeEventArgs e)
			{
				base.OnRowDeleting(e);
				if (this.GroupRelationsRowDeleting != null)
				{
					this.GroupRelationsRowDeleting(this, new SecurityProjectCategoriesDataSet.GroupRelationsRowChangeEvent((SecurityProjectCategoriesDataSet.GroupRelationsRow)e.Row, e.Action));
				}
			}

			// Token: 0x06009261 RID: 37473 RVA: 0x001CA73C File Offset: 0x001C893C
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			public void RemoveGroupRelationsRow(SecurityProjectCategoriesDataSet.GroupRelationsRow row)
			{
				base.Rows.Remove(row);
			}

			// Token: 0x06009262 RID: 37474 RVA: 0x001CA74C File Offset: 0x001C894C
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			public static XmlSchemaComplexType GetTypedTableSchema(XmlSchemaSet xs)
			{
				XmlSchemaComplexType xmlSchemaComplexType = new XmlSchemaComplexType();
				XmlSchemaSequence xmlSchemaSequence = new XmlSchemaSequence();
				SecurityProjectCategoriesDataSet securityProjectCategoriesDataSet = new SecurityProjectCategoriesDataSet();
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
				xmlSchemaAttribute.FixedValue = securityProjectCategoriesDataSet.Namespace;
				xmlSchemaComplexType.Attributes.Add(xmlSchemaAttribute);
				XmlSchemaAttribute xmlSchemaAttribute2 = new XmlSchemaAttribute();
				xmlSchemaAttribute2.Name = "tableTypeName";
				xmlSchemaAttribute2.FixedValue = "GroupRelationsDataTable";
				xmlSchemaComplexType.Attributes.Add(xmlSchemaAttribute2);
				xmlSchemaComplexType.Particle = xmlSchemaSequence;
				XmlSchema schemaSerializable = securityProjectCategoriesDataSet.GetSchemaSerializable();
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

			// Token: 0x04001D52 RID: 7506
			private DataColumn columnWSEC_CAT_UID;

			// Token: 0x04001D53 RID: 7507
			private DataColumn columnWSEC_GRP_UID;
		}

		// Token: 0x02000621 RID: 1569
		[XmlSchemaProvider("GetTypedTableSchema")]
		[Serializable]
		public class UserPermissionsDataTable : DataTable, IEnumerable
		{
			// Token: 0x06009263 RID: 37475 RVA: 0x001CA944 File Offset: 0x001C8B44
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			public UserPermissionsDataTable()
			{
				base.TableName = "UserPermissions";
				this.BeginInit();
				this.InitClass();
				this.EndInit();
			}

			// Token: 0x06009264 RID: 37476 RVA: 0x001CA96C File Offset: 0x001C8B6C
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			internal UserPermissionsDataTable(DataTable table)
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

			// Token: 0x06009265 RID: 37477 RVA: 0x001CAA14 File Offset: 0x001C8C14
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			protected UserPermissionsDataTable(SerializationInfo info, StreamingContext context) : base(info, context)
			{
				this.InitVars();
			}

			// Token: 0x17002BEC RID: 11244
			// (get) Token: 0x06009266 RID: 37478 RVA: 0x001CAA24 File Offset: 0x001C8C24
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			public DataColumn WSEC_CAT_UIDColumn
			{
				get
				{
					return this.columnWSEC_CAT_UID;
				}
			}

			// Token: 0x17002BED RID: 11245
			// (get) Token: 0x06009267 RID: 37479 RVA: 0x001CAA2C File Offset: 0x001C8C2C
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public DataColumn RES_UIDColumn
			{
				get
				{
					return this.columnRES_UID;
				}
			}

			// Token: 0x17002BEE RID: 11246
			// (get) Token: 0x06009268 RID: 37480 RVA: 0x001CAA34 File Offset: 0x001C8C34
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public DataColumn WSEC_FEA_ACT_UIDColumn
			{
				get
				{
					return this.columnWSEC_FEA_ACT_UID;
				}
			}

			// Token: 0x17002BEF RID: 11247
			// (get) Token: 0x06009269 RID: 37481 RVA: 0x001CAA3C File Offset: 0x001C8C3C
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

			// Token: 0x17002BF0 RID: 11248
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public SecurityProjectCategoriesDataSet.UserPermissionsRow this[int index]
			{
				get
				{
					return (SecurityProjectCategoriesDataSet.UserPermissionsRow)base.Rows[index];
				}
			}

			// Token: 0x14000551 RID: 1361
			// (add) Token: 0x0600926B RID: 37483 RVA: 0x001CAA5C File Offset: 0x001C8C5C
			// (remove) Token: 0x0600926C RID: 37484 RVA: 0x001CAA94 File Offset: 0x001C8C94
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public event SecurityProjectCategoriesDataSet.UserPermissionsRowChangeEventHandler UserPermissionsRowChanging;

			// Token: 0x14000552 RID: 1362
			// (add) Token: 0x0600926D RID: 37485 RVA: 0x001CAACC File Offset: 0x001C8CCC
			// (remove) Token: 0x0600926E RID: 37486 RVA: 0x001CAB04 File Offset: 0x001C8D04
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public event SecurityProjectCategoriesDataSet.UserPermissionsRowChangeEventHandler UserPermissionsRowChanged;

			// Token: 0x14000553 RID: 1363
			// (add) Token: 0x0600926F RID: 37487 RVA: 0x001CAB3C File Offset: 0x001C8D3C
			// (remove) Token: 0x06009270 RID: 37488 RVA: 0x001CAB74 File Offset: 0x001C8D74
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public event SecurityProjectCategoriesDataSet.UserPermissionsRowChangeEventHandler UserPermissionsRowDeleting;

			// Token: 0x14000554 RID: 1364
			// (add) Token: 0x06009271 RID: 37489 RVA: 0x001CABAC File Offset: 0x001C8DAC
			// (remove) Token: 0x06009272 RID: 37490 RVA: 0x001CABE4 File Offset: 0x001C8DE4
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public event SecurityProjectCategoriesDataSet.UserPermissionsRowChangeEventHandler UserPermissionsRowDeleted;

			// Token: 0x06009273 RID: 37491 RVA: 0x001CAC19 File Offset: 0x001C8E19
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public void AddUserPermissionsRow(SecurityProjectCategoriesDataSet.UserPermissionsRow row)
			{
				base.Rows.Add(row);
			}

			// Token: 0x06009274 RID: 37492 RVA: 0x001CAC28 File Offset: 0x001C8E28
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public SecurityProjectCategoriesDataSet.UserPermissionsRow AddUserPermissionsRow(Guid WSEC_CAT_UID, Guid RES_UID, Guid WSEC_FEA_ACT_UID)
			{
				SecurityProjectCategoriesDataSet.UserPermissionsRow userPermissionsRow = (SecurityProjectCategoriesDataSet.UserPermissionsRow)base.NewRow();
				object[] itemArray = new object[]
				{
					WSEC_CAT_UID,
					RES_UID,
					WSEC_FEA_ACT_UID
				};
				userPermissionsRow.ItemArray = itemArray;
				base.Rows.Add(userPermissionsRow);
				return userPermissionsRow;
			}

			// Token: 0x06009275 RID: 37493 RVA: 0x001CAC7C File Offset: 0x001C8E7C
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			public SecurityProjectCategoriesDataSet.UserPermissionsRow FindByWSEC_CAT_UIDRES_UIDWSEC_FEA_ACT_UID(Guid WSEC_CAT_UID, Guid RES_UID, Guid WSEC_FEA_ACT_UID)
			{
				return (SecurityProjectCategoriesDataSet.UserPermissionsRow)base.Rows.Find(new object[]
				{
					WSEC_CAT_UID,
					RES_UID,
					WSEC_FEA_ACT_UID
				});
			}

			// Token: 0x06009276 RID: 37494 RVA: 0x001CACBC File Offset: 0x001C8EBC
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			public virtual IEnumerator GetEnumerator()
			{
				return base.Rows.GetEnumerator();
			}

			// Token: 0x06009277 RID: 37495 RVA: 0x001CACCC File Offset: 0x001C8ECC
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public override DataTable Clone()
			{
				SecurityProjectCategoriesDataSet.UserPermissionsDataTable userPermissionsDataTable = (SecurityProjectCategoriesDataSet.UserPermissionsDataTable)base.Clone();
				userPermissionsDataTable.InitVars();
				return userPermissionsDataTable;
			}

			// Token: 0x06009278 RID: 37496 RVA: 0x001CACEC File Offset: 0x001C8EEC
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			protected override DataTable CreateInstance()
			{
				return new SecurityProjectCategoriesDataSet.UserPermissionsDataTable();
			}

			// Token: 0x06009279 RID: 37497 RVA: 0x001CACF4 File Offset: 0x001C8EF4
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			internal void InitVars()
			{
				this.columnWSEC_CAT_UID = base.Columns["WSEC_CAT_UID"];
				this.columnRES_UID = base.Columns["RES_UID"];
				this.columnWSEC_FEA_ACT_UID = base.Columns["WSEC_FEA_ACT_UID"];
			}

			// Token: 0x0600927A RID: 37498 RVA: 0x001CAD44 File Offset: 0x001C8F44
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			private void InitClass()
			{
				this.columnWSEC_CAT_UID = new DataColumn("WSEC_CAT_UID", typeof(Guid), null, MappingType.Element);
				base.Columns.Add(this.columnWSEC_CAT_UID);
				this.columnRES_UID = new DataColumn("RES_UID", typeof(Guid), null, MappingType.Element);
				base.Columns.Add(this.columnRES_UID);
				this.columnWSEC_FEA_ACT_UID = new DataColumn("WSEC_FEA_ACT_UID", typeof(Guid), null, MappingType.Element);
				base.Columns.Add(this.columnWSEC_FEA_ACT_UID);
				base.Constraints.Add(new UniqueConstraint("UserPermissionsPrimaryKey", new DataColumn[]
				{
					this.columnWSEC_CAT_UID,
					this.columnRES_UID,
					this.columnWSEC_FEA_ACT_UID
				}, true));
				this.columnWSEC_CAT_UID.AllowDBNull = false;
				this.columnRES_UID.AllowDBNull = false;
				this.columnWSEC_FEA_ACT_UID.AllowDBNull = false;
			}

			// Token: 0x0600927B RID: 37499 RVA: 0x001CAE35 File Offset: 0x001C9035
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public SecurityProjectCategoriesDataSet.UserPermissionsRow NewUserPermissionsRow()
			{
				return (SecurityProjectCategoriesDataSet.UserPermissionsRow)base.NewRow();
			}

			// Token: 0x0600927C RID: 37500 RVA: 0x001CAE42 File Offset: 0x001C9042
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			protected override DataRow NewRowFromBuilder(DataRowBuilder builder)
			{
				return new SecurityProjectCategoriesDataSet.UserPermissionsRow(builder);
			}

			// Token: 0x0600927D RID: 37501 RVA: 0x001CAE4A File Offset: 0x001C904A
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			protected override Type GetRowType()
			{
				return typeof(SecurityProjectCategoriesDataSet.UserPermissionsRow);
			}

			// Token: 0x0600927E RID: 37502 RVA: 0x001CAE56 File Offset: 0x001C9056
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			protected override void OnRowChanged(DataRowChangeEventArgs e)
			{
				base.OnRowChanged(e);
				if (this.UserPermissionsRowChanged != null)
				{
					this.UserPermissionsRowChanged(this, new SecurityProjectCategoriesDataSet.UserPermissionsRowChangeEvent((SecurityProjectCategoriesDataSet.UserPermissionsRow)e.Row, e.Action));
				}
			}

			// Token: 0x0600927F RID: 37503 RVA: 0x001CAE89 File Offset: 0x001C9089
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			protected override void OnRowChanging(DataRowChangeEventArgs e)
			{
				base.OnRowChanging(e);
				if (this.UserPermissionsRowChanging != null)
				{
					this.UserPermissionsRowChanging(this, new SecurityProjectCategoriesDataSet.UserPermissionsRowChangeEvent((SecurityProjectCategoriesDataSet.UserPermissionsRow)e.Row, e.Action));
				}
			}

			// Token: 0x06009280 RID: 37504 RVA: 0x001CAEBC File Offset: 0x001C90BC
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			protected override void OnRowDeleted(DataRowChangeEventArgs e)
			{
				base.OnRowDeleted(e);
				if (this.UserPermissionsRowDeleted != null)
				{
					this.UserPermissionsRowDeleted(this, new SecurityProjectCategoriesDataSet.UserPermissionsRowChangeEvent((SecurityProjectCategoriesDataSet.UserPermissionsRow)e.Row, e.Action));
				}
			}

			// Token: 0x06009281 RID: 37505 RVA: 0x001CAEEF File Offset: 0x001C90EF
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			protected override void OnRowDeleting(DataRowChangeEventArgs e)
			{
				base.OnRowDeleting(e);
				if (this.UserPermissionsRowDeleting != null)
				{
					this.UserPermissionsRowDeleting(this, new SecurityProjectCategoriesDataSet.UserPermissionsRowChangeEvent((SecurityProjectCategoriesDataSet.UserPermissionsRow)e.Row, e.Action));
				}
			}

			// Token: 0x06009282 RID: 37506 RVA: 0x001CAF22 File Offset: 0x001C9122
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			public void RemoveUserPermissionsRow(SecurityProjectCategoriesDataSet.UserPermissionsRow row)
			{
				base.Rows.Remove(row);
			}

			// Token: 0x06009283 RID: 37507 RVA: 0x001CAF30 File Offset: 0x001C9130
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public static XmlSchemaComplexType GetTypedTableSchema(XmlSchemaSet xs)
			{
				XmlSchemaComplexType xmlSchemaComplexType = new XmlSchemaComplexType();
				XmlSchemaSequence xmlSchemaSequence = new XmlSchemaSequence();
				SecurityProjectCategoriesDataSet securityProjectCategoriesDataSet = new SecurityProjectCategoriesDataSet();
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
				xmlSchemaAttribute.FixedValue = securityProjectCategoriesDataSet.Namespace;
				xmlSchemaComplexType.Attributes.Add(xmlSchemaAttribute);
				XmlSchemaAttribute xmlSchemaAttribute2 = new XmlSchemaAttribute();
				xmlSchemaAttribute2.Name = "tableTypeName";
				xmlSchemaAttribute2.FixedValue = "UserPermissionsDataTable";
				xmlSchemaComplexType.Attributes.Add(xmlSchemaAttribute2);
				xmlSchemaComplexType.Particle = xmlSchemaSequence;
				XmlSchema schemaSerializable = securityProjectCategoriesDataSet.GetSchemaSerializable();
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

			// Token: 0x04001D58 RID: 7512
			private DataColumn columnWSEC_CAT_UID;

			// Token: 0x04001D59 RID: 7513
			private DataColumn columnRES_UID;

			// Token: 0x04001D5A RID: 7514
			private DataColumn columnWSEC_FEA_ACT_UID;
		}

		// Token: 0x02000622 RID: 1570
		[XmlSchemaProvider("GetTypedTableSchema")]
		[Serializable]
		public class GroupPermissionsDataTable : DataTable, IEnumerable
		{
			// Token: 0x06009284 RID: 37508 RVA: 0x001CB128 File Offset: 0x001C9328
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			public GroupPermissionsDataTable()
			{
				base.TableName = "GroupPermissions";
				this.BeginInit();
				this.InitClass();
				this.EndInit();
			}

			// Token: 0x06009285 RID: 37509 RVA: 0x001CB150 File Offset: 0x001C9350
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			internal GroupPermissionsDataTable(DataTable table)
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

			// Token: 0x06009286 RID: 37510 RVA: 0x001CB1F8 File Offset: 0x001C93F8
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			protected GroupPermissionsDataTable(SerializationInfo info, StreamingContext context) : base(info, context)
			{
				this.InitVars();
			}

			// Token: 0x17002BF1 RID: 11249
			// (get) Token: 0x06009287 RID: 37511 RVA: 0x001CB208 File Offset: 0x001C9408
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public DataColumn WSEC_CAT_UIDColumn
			{
				get
				{
					return this.columnWSEC_CAT_UID;
				}
			}

			// Token: 0x17002BF2 RID: 11250
			// (get) Token: 0x06009288 RID: 37512 RVA: 0x001CB210 File Offset: 0x001C9410
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public DataColumn WSEC_GRP_UIDColumn
			{
				get
				{
					return this.columnWSEC_GRP_UID;
				}
			}

			// Token: 0x17002BF3 RID: 11251
			// (get) Token: 0x06009289 RID: 37513 RVA: 0x001CB218 File Offset: 0x001C9418
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			public DataColumn WSEC_FEA_ACT_UIDColumn
			{
				get
				{
					return this.columnWSEC_FEA_ACT_UID;
				}
			}

			// Token: 0x17002BF4 RID: 11252
			// (get) Token: 0x0600928A RID: 37514 RVA: 0x001CB220 File Offset: 0x001C9420
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

			// Token: 0x17002BF5 RID: 11253
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public SecurityProjectCategoriesDataSet.GroupPermissionsRow this[int index]
			{
				get
				{
					return (SecurityProjectCategoriesDataSet.GroupPermissionsRow)base.Rows[index];
				}
			}

			// Token: 0x14000555 RID: 1365
			// (add) Token: 0x0600928C RID: 37516 RVA: 0x001CB240 File Offset: 0x001C9440
			// (remove) Token: 0x0600928D RID: 37517 RVA: 0x001CB278 File Offset: 0x001C9478
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public event SecurityProjectCategoriesDataSet.GroupPermissionsRowChangeEventHandler GroupPermissionsRowChanging;

			// Token: 0x14000556 RID: 1366
			// (add) Token: 0x0600928E RID: 37518 RVA: 0x001CB2B0 File Offset: 0x001C94B0
			// (remove) Token: 0x0600928F RID: 37519 RVA: 0x001CB2E8 File Offset: 0x001C94E8
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public event SecurityProjectCategoriesDataSet.GroupPermissionsRowChangeEventHandler GroupPermissionsRowChanged;

			// Token: 0x14000557 RID: 1367
			// (add) Token: 0x06009290 RID: 37520 RVA: 0x001CB320 File Offset: 0x001C9520
			// (remove) Token: 0x06009291 RID: 37521 RVA: 0x001CB358 File Offset: 0x001C9558
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public event SecurityProjectCategoriesDataSet.GroupPermissionsRowChangeEventHandler GroupPermissionsRowDeleting;

			// Token: 0x14000558 RID: 1368
			// (add) Token: 0x06009292 RID: 37522 RVA: 0x001CB390 File Offset: 0x001C9590
			// (remove) Token: 0x06009293 RID: 37523 RVA: 0x001CB3C8 File Offset: 0x001C95C8
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public event SecurityProjectCategoriesDataSet.GroupPermissionsRowChangeEventHandler GroupPermissionsRowDeleted;

			// Token: 0x06009294 RID: 37524 RVA: 0x001CB3FD File Offset: 0x001C95FD
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public void AddGroupPermissionsRow(SecurityProjectCategoriesDataSet.GroupPermissionsRow row)
			{
				base.Rows.Add(row);
			}

			// Token: 0x06009295 RID: 37525 RVA: 0x001CB40C File Offset: 0x001C960C
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			public SecurityProjectCategoriesDataSet.GroupPermissionsRow AddGroupPermissionsRow(Guid WSEC_CAT_UID, Guid WSEC_GRP_UID, Guid WSEC_FEA_ACT_UID)
			{
				SecurityProjectCategoriesDataSet.GroupPermissionsRow groupPermissionsRow = (SecurityProjectCategoriesDataSet.GroupPermissionsRow)base.NewRow();
				object[] itemArray = new object[]
				{
					WSEC_CAT_UID,
					WSEC_GRP_UID,
					WSEC_FEA_ACT_UID
				};
				groupPermissionsRow.ItemArray = itemArray;
				base.Rows.Add(groupPermissionsRow);
				return groupPermissionsRow;
			}

			// Token: 0x06009296 RID: 37526 RVA: 0x001CB460 File Offset: 0x001C9660
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			public SecurityProjectCategoriesDataSet.GroupPermissionsRow FindByWSEC_CAT_UIDWSEC_GRP_UIDWSEC_FEA_ACT_UID(Guid WSEC_CAT_UID, Guid WSEC_GRP_UID, Guid WSEC_FEA_ACT_UID)
			{
				return (SecurityProjectCategoriesDataSet.GroupPermissionsRow)base.Rows.Find(new object[]
				{
					WSEC_CAT_UID,
					WSEC_GRP_UID,
					WSEC_FEA_ACT_UID
				});
			}

			// Token: 0x06009297 RID: 37527 RVA: 0x001CB4A0 File Offset: 0x001C96A0
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public virtual IEnumerator GetEnumerator()
			{
				return base.Rows.GetEnumerator();
			}

			// Token: 0x06009298 RID: 37528 RVA: 0x001CB4B0 File Offset: 0x001C96B0
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			public override DataTable Clone()
			{
				SecurityProjectCategoriesDataSet.GroupPermissionsDataTable groupPermissionsDataTable = (SecurityProjectCategoriesDataSet.GroupPermissionsDataTable)base.Clone();
				groupPermissionsDataTable.InitVars();
				return groupPermissionsDataTable;
			}

			// Token: 0x06009299 RID: 37529 RVA: 0x001CB4D0 File Offset: 0x001C96D0
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			protected override DataTable CreateInstance()
			{
				return new SecurityProjectCategoriesDataSet.GroupPermissionsDataTable();
			}

			// Token: 0x0600929A RID: 37530 RVA: 0x001CB4D8 File Offset: 0x001C96D8
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			internal void InitVars()
			{
				this.columnWSEC_CAT_UID = base.Columns["WSEC_CAT_UID"];
				this.columnWSEC_GRP_UID = base.Columns["WSEC_GRP_UID"];
				this.columnWSEC_FEA_ACT_UID = base.Columns["WSEC_FEA_ACT_UID"];
			}

			// Token: 0x0600929B RID: 37531 RVA: 0x001CB528 File Offset: 0x001C9728
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			private void InitClass()
			{
				this.columnWSEC_CAT_UID = new DataColumn("WSEC_CAT_UID", typeof(Guid), null, MappingType.Element);
				base.Columns.Add(this.columnWSEC_CAT_UID);
				this.columnWSEC_GRP_UID = new DataColumn("WSEC_GRP_UID", typeof(Guid), null, MappingType.Element);
				base.Columns.Add(this.columnWSEC_GRP_UID);
				this.columnWSEC_FEA_ACT_UID = new DataColumn("WSEC_FEA_ACT_UID", typeof(Guid), null, MappingType.Element);
				base.Columns.Add(this.columnWSEC_FEA_ACT_UID);
				base.Constraints.Add(new UniqueConstraint("GroupPermissionsPrimaryKey", new DataColumn[]
				{
					this.columnWSEC_CAT_UID,
					this.columnWSEC_GRP_UID,
					this.columnWSEC_FEA_ACT_UID
				}, true));
				this.columnWSEC_CAT_UID.AllowDBNull = false;
				this.columnWSEC_GRP_UID.AllowDBNull = false;
				this.columnWSEC_FEA_ACT_UID.AllowDBNull = false;
			}

			// Token: 0x0600929C RID: 37532 RVA: 0x001CB619 File Offset: 0x001C9819
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			public SecurityProjectCategoriesDataSet.GroupPermissionsRow NewGroupPermissionsRow()
			{
				return (SecurityProjectCategoriesDataSet.GroupPermissionsRow)base.NewRow();
			}

			// Token: 0x0600929D RID: 37533 RVA: 0x001CB626 File Offset: 0x001C9826
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			protected override DataRow NewRowFromBuilder(DataRowBuilder builder)
			{
				return new SecurityProjectCategoriesDataSet.GroupPermissionsRow(builder);
			}

			// Token: 0x0600929E RID: 37534 RVA: 0x001CB62E File Offset: 0x001C982E
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			protected override Type GetRowType()
			{
				return typeof(SecurityProjectCategoriesDataSet.GroupPermissionsRow);
			}

			// Token: 0x0600929F RID: 37535 RVA: 0x001CB63A File Offset: 0x001C983A
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			protected override void OnRowChanged(DataRowChangeEventArgs e)
			{
				base.OnRowChanged(e);
				if (this.GroupPermissionsRowChanged != null)
				{
					this.GroupPermissionsRowChanged(this, new SecurityProjectCategoriesDataSet.GroupPermissionsRowChangeEvent((SecurityProjectCategoriesDataSet.GroupPermissionsRow)e.Row, e.Action));
				}
			}

			// Token: 0x060092A0 RID: 37536 RVA: 0x001CB66D File Offset: 0x001C986D
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			protected override void OnRowChanging(DataRowChangeEventArgs e)
			{
				base.OnRowChanging(e);
				if (this.GroupPermissionsRowChanging != null)
				{
					this.GroupPermissionsRowChanging(this, new SecurityProjectCategoriesDataSet.GroupPermissionsRowChangeEvent((SecurityProjectCategoriesDataSet.GroupPermissionsRow)e.Row, e.Action));
				}
			}

			// Token: 0x060092A1 RID: 37537 RVA: 0x001CB6A0 File Offset: 0x001C98A0
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			protected override void OnRowDeleted(DataRowChangeEventArgs e)
			{
				base.OnRowDeleted(e);
				if (this.GroupPermissionsRowDeleted != null)
				{
					this.GroupPermissionsRowDeleted(this, new SecurityProjectCategoriesDataSet.GroupPermissionsRowChangeEvent((SecurityProjectCategoriesDataSet.GroupPermissionsRow)e.Row, e.Action));
				}
			}

			// Token: 0x060092A2 RID: 37538 RVA: 0x001CB6D3 File Offset: 0x001C98D3
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			protected override void OnRowDeleting(DataRowChangeEventArgs e)
			{
				base.OnRowDeleting(e);
				if (this.GroupPermissionsRowDeleting != null)
				{
					this.GroupPermissionsRowDeleting(this, new SecurityProjectCategoriesDataSet.GroupPermissionsRowChangeEvent((SecurityProjectCategoriesDataSet.GroupPermissionsRow)e.Row, e.Action));
				}
			}

			// Token: 0x060092A3 RID: 37539 RVA: 0x001CB706 File Offset: 0x001C9906
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			public void RemoveGroupPermissionsRow(SecurityProjectCategoriesDataSet.GroupPermissionsRow row)
			{
				base.Rows.Remove(row);
			}

			// Token: 0x060092A4 RID: 37540 RVA: 0x001CB714 File Offset: 0x001C9914
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public static XmlSchemaComplexType GetTypedTableSchema(XmlSchemaSet xs)
			{
				XmlSchemaComplexType xmlSchemaComplexType = new XmlSchemaComplexType();
				XmlSchemaSequence xmlSchemaSequence = new XmlSchemaSequence();
				SecurityProjectCategoriesDataSet securityProjectCategoriesDataSet = new SecurityProjectCategoriesDataSet();
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
				xmlSchemaAttribute.FixedValue = securityProjectCategoriesDataSet.Namespace;
				xmlSchemaComplexType.Attributes.Add(xmlSchemaAttribute);
				XmlSchemaAttribute xmlSchemaAttribute2 = new XmlSchemaAttribute();
				xmlSchemaAttribute2.Name = "tableTypeName";
				xmlSchemaAttribute2.FixedValue = "GroupPermissionsDataTable";
				xmlSchemaComplexType.Attributes.Add(xmlSchemaAttribute2);
				xmlSchemaComplexType.Particle = xmlSchemaSequence;
				XmlSchema schemaSerializable = securityProjectCategoriesDataSet.GetSchemaSerializable();
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

			// Token: 0x04001D5F RID: 7519
			private DataColumn columnWSEC_CAT_UID;

			// Token: 0x04001D60 RID: 7520
			private DataColumn columnWSEC_GRP_UID;

			// Token: 0x04001D61 RID: 7521
			private DataColumn columnWSEC_FEA_ACT_UID;
		}

		// Token: 0x02000623 RID: 1571
		public class ProjectCategoriesRow : DataRow
		{
			// Token: 0x060092A5 RID: 37541 RVA: 0x001CB90C File Offset: 0x001C9B0C
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			internal ProjectCategoriesRow(DataRowBuilder rb) : base(rb)
			{
				this.tableProjectCategories = (SecurityProjectCategoriesDataSet.ProjectCategoriesDataTable)base.Table;
			}

			// Token: 0x17002BF6 RID: 11254
			// (get) Token: 0x060092A6 RID: 37542 RVA: 0x001CB926 File Offset: 0x001C9B26
			// (set) Token: 0x060092A7 RID: 37543 RVA: 0x001CB93E File Offset: 0x001C9B3E
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public Guid WSEC_CAT_UID
			{
				get
				{
					return (Guid)base[this.tableProjectCategories.WSEC_CAT_UIDColumn];
				}
				set
				{
					base[this.tableProjectCategories.WSEC_CAT_UIDColumn] = value;
				}
			}

			// Token: 0x17002BF7 RID: 11255
			// (get) Token: 0x060092A8 RID: 37544 RVA: 0x001CB957 File Offset: 0x001C9B57
			// (set) Token: 0x060092A9 RID: 37545 RVA: 0x001CB96F File Offset: 0x001C9B6F
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public Guid PROJ_UID
			{
				get
				{
					return (Guid)base[this.tableProjectCategories.PROJ_UIDColumn];
				}
				set
				{
					base[this.tableProjectCategories.PROJ_UIDColumn] = value;
				}
			}

			// Token: 0x04001D66 RID: 7526
			private SecurityProjectCategoriesDataSet.ProjectCategoriesDataTable tableProjectCategories;
		}

		// Token: 0x02000624 RID: 1572
		public class UserRelationsRow : DataRow
		{
			// Token: 0x060092AA RID: 37546 RVA: 0x001CB988 File Offset: 0x001C9B88
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			internal UserRelationsRow(DataRowBuilder rb) : base(rb)
			{
				this.tableUserRelations = (SecurityProjectCategoriesDataSet.UserRelationsDataTable)base.Table;
			}

			// Token: 0x17002BF8 RID: 11256
			// (get) Token: 0x060092AB RID: 37547 RVA: 0x001CB9A2 File Offset: 0x001C9BA2
			// (set) Token: 0x060092AC RID: 37548 RVA: 0x001CB9BA File Offset: 0x001C9BBA
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			public Guid WSEC_CAT_UID
			{
				get
				{
					return (Guid)base[this.tableUserRelations.WSEC_CAT_UIDColumn];
				}
				set
				{
					base[this.tableUserRelations.WSEC_CAT_UIDColumn] = value;
				}
			}

			// Token: 0x17002BF9 RID: 11257
			// (get) Token: 0x060092AD RID: 37549 RVA: 0x001CB9D3 File Offset: 0x001C9BD3
			// (set) Token: 0x060092AE RID: 37550 RVA: 0x001CB9EB File Offset: 0x001C9BEB
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public Guid RES_UID
			{
				get
				{
					return (Guid)base[this.tableUserRelations.RES_UIDColumn];
				}
				set
				{
					base[this.tableUserRelations.RES_UIDColumn] = value;
				}
			}

			// Token: 0x04001D67 RID: 7527
			private SecurityProjectCategoriesDataSet.UserRelationsDataTable tableUserRelations;
		}

		// Token: 0x02000625 RID: 1573
		public class GroupRelationsRow : DataRow
		{
			// Token: 0x060092AF RID: 37551 RVA: 0x001CBA04 File Offset: 0x001C9C04
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			internal GroupRelationsRow(DataRowBuilder rb) : base(rb)
			{
				this.tableGroupRelations = (SecurityProjectCategoriesDataSet.GroupRelationsDataTable)base.Table;
			}

			// Token: 0x17002BFA RID: 11258
			// (get) Token: 0x060092B0 RID: 37552 RVA: 0x001CBA1E File Offset: 0x001C9C1E
			// (set) Token: 0x060092B1 RID: 37553 RVA: 0x001CBA36 File Offset: 0x001C9C36
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			public Guid WSEC_CAT_UID
			{
				get
				{
					return (Guid)base[this.tableGroupRelations.WSEC_CAT_UIDColumn];
				}
				set
				{
					base[this.tableGroupRelations.WSEC_CAT_UIDColumn] = value;
				}
			}

			// Token: 0x17002BFB RID: 11259
			// (get) Token: 0x060092B2 RID: 37554 RVA: 0x001CBA4F File Offset: 0x001C9C4F
			// (set) Token: 0x060092B3 RID: 37555 RVA: 0x001CBA67 File Offset: 0x001C9C67
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			public Guid WSEC_GRP_UID
			{
				get
				{
					return (Guid)base[this.tableGroupRelations.WSEC_GRP_UIDColumn];
				}
				set
				{
					base[this.tableGroupRelations.WSEC_GRP_UIDColumn] = value;
				}
			}

			// Token: 0x04001D68 RID: 7528
			private SecurityProjectCategoriesDataSet.GroupRelationsDataTable tableGroupRelations;
		}

		// Token: 0x02000626 RID: 1574
		public class UserPermissionsRow : DataRow
		{
			// Token: 0x060092B4 RID: 37556 RVA: 0x001CBA80 File Offset: 0x001C9C80
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			internal UserPermissionsRow(DataRowBuilder rb) : base(rb)
			{
				this.tableUserPermissions = (SecurityProjectCategoriesDataSet.UserPermissionsDataTable)base.Table;
			}

			// Token: 0x17002BFC RID: 11260
			// (get) Token: 0x060092B5 RID: 37557 RVA: 0x001CBA9A File Offset: 0x001C9C9A
			// (set) Token: 0x060092B6 RID: 37558 RVA: 0x001CBAB2 File Offset: 0x001C9CB2
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			public Guid WSEC_CAT_UID
			{
				get
				{
					return (Guid)base[this.tableUserPermissions.WSEC_CAT_UIDColumn];
				}
				set
				{
					base[this.tableUserPermissions.WSEC_CAT_UIDColumn] = value;
				}
			}

			// Token: 0x17002BFD RID: 11261
			// (get) Token: 0x060092B7 RID: 37559 RVA: 0x001CBACB File Offset: 0x001C9CCB
			// (set) Token: 0x060092B8 RID: 37560 RVA: 0x001CBAE3 File Offset: 0x001C9CE3
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			public Guid RES_UID
			{
				get
				{
					return (Guid)base[this.tableUserPermissions.RES_UIDColumn];
				}
				set
				{
					base[this.tableUserPermissions.RES_UIDColumn] = value;
				}
			}

			// Token: 0x17002BFE RID: 11262
			// (get) Token: 0x060092B9 RID: 37561 RVA: 0x001CBAFC File Offset: 0x001C9CFC
			// (set) Token: 0x060092BA RID: 37562 RVA: 0x001CBB14 File Offset: 0x001C9D14
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public Guid WSEC_FEA_ACT_UID
			{
				get
				{
					return (Guid)base[this.tableUserPermissions.WSEC_FEA_ACT_UIDColumn];
				}
				set
				{
					base[this.tableUserPermissions.WSEC_FEA_ACT_UIDColumn] = value;
				}
			}

			// Token: 0x04001D69 RID: 7529
			private SecurityProjectCategoriesDataSet.UserPermissionsDataTable tableUserPermissions;
		}

		// Token: 0x02000627 RID: 1575
		public class GroupPermissionsRow : DataRow
		{
			// Token: 0x060092BB RID: 37563 RVA: 0x001CBB2D File Offset: 0x001C9D2D
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			internal GroupPermissionsRow(DataRowBuilder rb) : base(rb)
			{
				this.tableGroupPermissions = (SecurityProjectCategoriesDataSet.GroupPermissionsDataTable)base.Table;
			}

			// Token: 0x17002BFF RID: 11263
			// (get) Token: 0x060092BC RID: 37564 RVA: 0x001CBB47 File Offset: 0x001C9D47
			// (set) Token: 0x060092BD RID: 37565 RVA: 0x001CBB5F File Offset: 0x001C9D5F
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			public Guid WSEC_CAT_UID
			{
				get
				{
					return (Guid)base[this.tableGroupPermissions.WSEC_CAT_UIDColumn];
				}
				set
				{
					base[this.tableGroupPermissions.WSEC_CAT_UIDColumn] = value;
				}
			}

			// Token: 0x17002C00 RID: 11264
			// (get) Token: 0x060092BE RID: 37566 RVA: 0x001CBB78 File Offset: 0x001C9D78
			// (set) Token: 0x060092BF RID: 37567 RVA: 0x001CBB90 File Offset: 0x001C9D90
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			public Guid WSEC_GRP_UID
			{
				get
				{
					return (Guid)base[this.tableGroupPermissions.WSEC_GRP_UIDColumn];
				}
				set
				{
					base[this.tableGroupPermissions.WSEC_GRP_UIDColumn] = value;
				}
			}

			// Token: 0x17002C01 RID: 11265
			// (get) Token: 0x060092C0 RID: 37568 RVA: 0x001CBBA9 File Offset: 0x001C9DA9
			// (set) Token: 0x060092C1 RID: 37569 RVA: 0x001CBBC1 File Offset: 0x001C9DC1
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public Guid WSEC_FEA_ACT_UID
			{
				get
				{
					return (Guid)base[this.tableGroupPermissions.WSEC_FEA_ACT_UIDColumn];
				}
				set
				{
					base[this.tableGroupPermissions.WSEC_FEA_ACT_UIDColumn] = value;
				}
			}

			// Token: 0x04001D6A RID: 7530
			private SecurityProjectCategoriesDataSet.GroupPermissionsDataTable tableGroupPermissions;
		}

		// Token: 0x02000628 RID: 1576
		[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
		public class ProjectCategoriesRowChangeEvent : EventArgs
		{
			// Token: 0x060092C2 RID: 37570 RVA: 0x001CBBDA File Offset: 0x001C9DDA
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			public ProjectCategoriesRowChangeEvent(SecurityProjectCategoriesDataSet.ProjectCategoriesRow row, DataRowAction action)
			{
				this.eventRow = row;
				this.eventAction = action;
			}

			// Token: 0x17002C02 RID: 11266
			// (get) Token: 0x060092C3 RID: 37571 RVA: 0x001CBBF0 File Offset: 0x001C9DF0
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			public SecurityProjectCategoriesDataSet.ProjectCategoriesRow Row
			{
				get
				{
					return this.eventRow;
				}
			}

			// Token: 0x17002C03 RID: 11267
			// (get) Token: 0x060092C4 RID: 37572 RVA: 0x001CBBF8 File Offset: 0x001C9DF8
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			public DataRowAction Action
			{
				get
				{
					return this.eventAction;
				}
			}

			// Token: 0x04001D6B RID: 7531
			private SecurityProjectCategoriesDataSet.ProjectCategoriesRow eventRow;

			// Token: 0x04001D6C RID: 7532
			private DataRowAction eventAction;
		}

		// Token: 0x02000629 RID: 1577
		[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
		public class UserRelationsRowChangeEvent : EventArgs
		{
			// Token: 0x060092C5 RID: 37573 RVA: 0x001CBC00 File Offset: 0x001C9E00
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public UserRelationsRowChangeEvent(SecurityProjectCategoriesDataSet.UserRelationsRow row, DataRowAction action)
			{
				this.eventRow = row;
				this.eventAction = action;
			}

			// Token: 0x17002C04 RID: 11268
			// (get) Token: 0x060092C6 RID: 37574 RVA: 0x001CBC16 File Offset: 0x001C9E16
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public SecurityProjectCategoriesDataSet.UserRelationsRow Row
			{
				get
				{
					return this.eventRow;
				}
			}

			// Token: 0x17002C05 RID: 11269
			// (get) Token: 0x060092C7 RID: 37575 RVA: 0x001CBC1E File Offset: 0x001C9E1E
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			public DataRowAction Action
			{
				get
				{
					return this.eventAction;
				}
			}

			// Token: 0x04001D6D RID: 7533
			private SecurityProjectCategoriesDataSet.UserRelationsRow eventRow;

			// Token: 0x04001D6E RID: 7534
			private DataRowAction eventAction;
		}

		// Token: 0x0200062A RID: 1578
		[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
		public class GroupRelationsRowChangeEvent : EventArgs
		{
			// Token: 0x060092C8 RID: 37576 RVA: 0x001CBC26 File Offset: 0x001C9E26
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			public GroupRelationsRowChangeEvent(SecurityProjectCategoriesDataSet.GroupRelationsRow row, DataRowAction action)
			{
				this.eventRow = row;
				this.eventAction = action;
			}

			// Token: 0x17002C06 RID: 11270
			// (get) Token: 0x060092C9 RID: 37577 RVA: 0x001CBC3C File Offset: 0x001C9E3C
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public SecurityProjectCategoriesDataSet.GroupRelationsRow Row
			{
				get
				{
					return this.eventRow;
				}
			}

			// Token: 0x17002C07 RID: 11271
			// (get) Token: 0x060092CA RID: 37578 RVA: 0x001CBC44 File Offset: 0x001C9E44
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public DataRowAction Action
			{
				get
				{
					return this.eventAction;
				}
			}

			// Token: 0x04001D6F RID: 7535
			private SecurityProjectCategoriesDataSet.GroupRelationsRow eventRow;

			// Token: 0x04001D70 RID: 7536
			private DataRowAction eventAction;
		}

		// Token: 0x0200062B RID: 1579
		[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
		public class UserPermissionsRowChangeEvent : EventArgs
		{
			// Token: 0x060092CB RID: 37579 RVA: 0x001CBC4C File Offset: 0x001C9E4C
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public UserPermissionsRowChangeEvent(SecurityProjectCategoriesDataSet.UserPermissionsRow row, DataRowAction action)
			{
				this.eventRow = row;
				this.eventAction = action;
			}

			// Token: 0x17002C08 RID: 11272
			// (get) Token: 0x060092CC RID: 37580 RVA: 0x001CBC62 File Offset: 0x001C9E62
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public SecurityProjectCategoriesDataSet.UserPermissionsRow Row
			{
				get
				{
					return this.eventRow;
				}
			}

			// Token: 0x17002C09 RID: 11273
			// (get) Token: 0x060092CD RID: 37581 RVA: 0x001CBC6A File Offset: 0x001C9E6A
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public DataRowAction Action
			{
				get
				{
					return this.eventAction;
				}
			}

			// Token: 0x04001D71 RID: 7537
			private SecurityProjectCategoriesDataSet.UserPermissionsRow eventRow;

			// Token: 0x04001D72 RID: 7538
			private DataRowAction eventAction;
		}

		// Token: 0x0200062C RID: 1580
		[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
		public class GroupPermissionsRowChangeEvent : EventArgs
		{
			// Token: 0x060092CE RID: 37582 RVA: 0x001CBC72 File Offset: 0x001C9E72
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public GroupPermissionsRowChangeEvent(SecurityProjectCategoriesDataSet.GroupPermissionsRow row, DataRowAction action)
			{
				this.eventRow = row;
				this.eventAction = action;
			}

			// Token: 0x17002C0A RID: 11274
			// (get) Token: 0x060092CF RID: 37583 RVA: 0x001CBC88 File Offset: 0x001C9E88
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public SecurityProjectCategoriesDataSet.GroupPermissionsRow Row
			{
				get
				{
					return this.eventRow;
				}
			}

			// Token: 0x17002C0B RID: 11275
			// (get) Token: 0x060092D0 RID: 37584 RVA: 0x001CBC90 File Offset: 0x001C9E90
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			public DataRowAction Action
			{
				get
				{
					return this.eventAction;
				}
			}

			// Token: 0x04001D73 RID: 7539
			private SecurityProjectCategoriesDataSet.GroupPermissionsRow eventRow;

			// Token: 0x04001D74 RID: 7540
			private DataRowAction eventAction;
		}
	}
}
