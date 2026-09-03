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
	// Token: 0x020005BE RID: 1470
	[DesignerCategory("code")]
	[HelpKeyword("vs.data.DataSet")]
	[ToolboxItem(true)]
	[XmlSchemaProvider("GetTypedDataSetSchema")]
	[XmlRoot("SecurityCategoriesDataSet")]
	[Serializable]
	public class SecurityCategoriesDataSet : DataSet
	{
		// Token: 0x06008D2C RID: 36140 RVA: 0x001B9BD0 File Offset: 0x001B7DD0
		public new void EndInit()
		{
			base.EndInit();
			TypedDataSetUtilities.AllowNullsInNonTypedColumns(this.GroupRelations, new string[]
			{
				"WSEC_CAT_UID",
				"WSEC_GRP_UID"
			});
			TypedDataSetUtilities.AllowNullsInNonTypedColumns(this.SecurityCategoryRules, new string[]
			{
				"WSEC_OBJ_RULE_TYPE",
				"WSEC_CAT_UID",
				"WSEC_OBJ_TYPE_UID"
			});
			TypedDataSetUtilities.AllowNullsInNonTypedColumns(this.GroupPermissions, new string[]
			{
				"WSEC_DENY",
				"WSEC_CAT_UID",
				"WSEC_ALLOW",
				"WSEC_FEA_ACT_UID",
				"WSEC_GRP_UID"
			});
			TypedDataSetUtilities.AllowNullsInNonTypedColumns(this.SecurityCategoryObjects, new string[]
			{
				"WSEC_OBJ_UID",
				"WSEC_CAT_UID",
				"WSEC_OBJ_TYPE_UID"
			});
			TypedDataSetUtilities.AllowNullsInNonTypedColumns(this.SecurityCategories, new string[]
			{
				"WSEC_CAT_DESC",
				"WSEC_CAT_NAME",
				"WSEC_CAT_UID"
			});
			TypedDataSetUtilities.AllowNullsInNonTypedColumns(this.UserRelations, new string[]
			{
				"RES_UID",
				"WSEC_CAT_UID"
			});
			TypedDataSetUtilities.AllowNullsInNonTypedColumns(this.UserPermissions, new string[]
			{
				"WSEC_DENY",
				"RES_UID",
				"WSEC_CAT_UID",
				"WSEC_ALLOW",
				"WSEC_FEA_ACT_UID"
			});
		}

		// Token: 0x06008D2D RID: 36141 RVA: 0x001B9D30 File Offset: 0x001B7F30
		[DebuggerNonUserCode]
		[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
		public SecurityCategoriesDataSet()
		{
			base.BeginInit();
			this.InitClass();
			CollectionChangeEventHandler value = new CollectionChangeEventHandler(this.SchemaChanged);
			base.Tables.CollectionChanged += value;
			base.Relations.CollectionChanged += value;
			this.EndInit();
		}

		// Token: 0x06008D2E RID: 36142 RVA: 0x001B9D84 File Offset: 0x001B7F84
		[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
		[DebuggerNonUserCode]
		protected SecurityCategoriesDataSet(SerializationInfo info, StreamingContext context) : base(info, context, false)
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
				if (dataSet.Tables["SecurityCategories"] != null)
				{
					base.Tables.Add(new SecurityCategoriesDataSet.SecurityCategoriesDataTable(dataSet.Tables["SecurityCategories"]));
				}
				if (dataSet.Tables["UserRelations"] != null)
				{
					base.Tables.Add(new SecurityCategoriesDataSet.UserRelationsDataTable(dataSet.Tables["UserRelations"]));
				}
				if (dataSet.Tables["GroupRelations"] != null)
				{
					base.Tables.Add(new SecurityCategoriesDataSet.GroupRelationsDataTable(dataSet.Tables["GroupRelations"]));
				}
				if (dataSet.Tables["UserPermissions"] != null)
				{
					base.Tables.Add(new SecurityCategoriesDataSet.UserPermissionsDataTable(dataSet.Tables["UserPermissions"]));
				}
				if (dataSet.Tables["GroupPermissions"] != null)
				{
					base.Tables.Add(new SecurityCategoriesDataSet.GroupPermissionsDataTable(dataSet.Tables["GroupPermissions"]));
				}
				if (dataSet.Tables["SecurityCategoryObjects"] != null)
				{
					base.Tables.Add(new SecurityCategoriesDataSet.SecurityCategoryObjectsDataTable(dataSet.Tables["SecurityCategoryObjects"]));
				}
				if (dataSet.Tables["SecurityCategoryRules"] != null)
				{
					base.Tables.Add(new SecurityCategoriesDataSet.SecurityCategoryRulesDataTable(dataSet.Tables["SecurityCategoryRules"]));
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

		// Token: 0x17002AC7 RID: 10951
		// (get) Token: 0x06008D2F RID: 36143 RVA: 0x001BA00D File Offset: 0x001B820D
		[Browsable(false)]
		[DebuggerNonUserCode]
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Content)]
		[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
		public SecurityCategoriesDataSet.SecurityCategoriesDataTable SecurityCategories
		{
			get
			{
				return this.tableSecurityCategories;
			}
		}

		// Token: 0x17002AC8 RID: 10952
		// (get) Token: 0x06008D30 RID: 36144 RVA: 0x001BA015 File Offset: 0x001B8215
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Content)]
		[Browsable(false)]
		[DebuggerNonUserCode]
		[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
		public SecurityCategoriesDataSet.UserRelationsDataTable UserRelations
		{
			get
			{
				return this.tableUserRelations;
			}
		}

		// Token: 0x17002AC9 RID: 10953
		// (get) Token: 0x06008D31 RID: 36145 RVA: 0x001BA01D File Offset: 0x001B821D
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Content)]
		[Browsable(false)]
		[DebuggerNonUserCode]
		[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
		public SecurityCategoriesDataSet.GroupRelationsDataTable GroupRelations
		{
			get
			{
				return this.tableGroupRelations;
			}
		}

		// Token: 0x17002ACA RID: 10954
		// (get) Token: 0x06008D32 RID: 36146 RVA: 0x001BA025 File Offset: 0x001B8225
		[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
		[Browsable(false)]
		[DebuggerNonUserCode]
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Content)]
		public SecurityCategoriesDataSet.UserPermissionsDataTable UserPermissions
		{
			get
			{
				return this.tableUserPermissions;
			}
		}

		// Token: 0x17002ACB RID: 10955
		// (get) Token: 0x06008D33 RID: 36147 RVA: 0x001BA02D File Offset: 0x001B822D
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Content)]
		[DebuggerNonUserCode]
		[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
		[Browsable(false)]
		public SecurityCategoriesDataSet.GroupPermissionsDataTable GroupPermissions
		{
			get
			{
				return this.tableGroupPermissions;
			}
		}

		// Token: 0x17002ACC RID: 10956
		// (get) Token: 0x06008D34 RID: 36148 RVA: 0x001BA035 File Offset: 0x001B8235
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Content)]
		[Browsable(false)]
		[DebuggerNonUserCode]
		[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
		public SecurityCategoriesDataSet.SecurityCategoryObjectsDataTable SecurityCategoryObjects
		{
			get
			{
				return this.tableSecurityCategoryObjects;
			}
		}

		// Token: 0x17002ACD RID: 10957
		// (get) Token: 0x06008D35 RID: 36149 RVA: 0x001BA03D File Offset: 0x001B823D
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Content)]
		[Browsable(false)]
		[DebuggerNonUserCode]
		[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
		public SecurityCategoriesDataSet.SecurityCategoryRulesDataTable SecurityCategoryRules
		{
			get
			{
				return this.tableSecurityCategoryRules;
			}
		}

		// Token: 0x17002ACE RID: 10958
		// (get) Token: 0x06008D36 RID: 36150 RVA: 0x001BA045 File Offset: 0x001B8245
		// (set) Token: 0x06008D37 RID: 36151 RVA: 0x001BA04D File Offset: 0x001B824D
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

		// Token: 0x17002ACF RID: 10959
		// (get) Token: 0x06008D38 RID: 36152 RVA: 0x001BA056 File Offset: 0x001B8256
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

		// Token: 0x17002AD0 RID: 10960
		// (get) Token: 0x06008D39 RID: 36153 RVA: 0x001BA05E File Offset: 0x001B825E
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

		// Token: 0x06008D3A RID: 36154 RVA: 0x001BA066 File Offset: 0x001B8266
		[DebuggerNonUserCode]
		[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
		protected override void InitializeDerivedDataSet()
		{
			base.BeginInit();
			this.InitClass();
			this.EndInit();
		}

		// Token: 0x06008D3B RID: 36155 RVA: 0x001BA07C File Offset: 0x001B827C
		[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
		[DebuggerNonUserCode]
		public override DataSet Clone()
		{
			SecurityCategoriesDataSet securityCategoriesDataSet = (SecurityCategoriesDataSet)base.Clone();
			securityCategoriesDataSet.InitVars();
			securityCategoriesDataSet.SchemaSerializationMode = this.SchemaSerializationMode;
			return securityCategoriesDataSet;
		}

		// Token: 0x06008D3C RID: 36156 RVA: 0x001BA0A8 File Offset: 0x001B82A8
		[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
		[DebuggerNonUserCode]
		protected override bool ShouldSerializeTables()
		{
			return false;
		}

		// Token: 0x06008D3D RID: 36157 RVA: 0x001BA0AB File Offset: 0x001B82AB
		[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
		[DebuggerNonUserCode]
		protected override bool ShouldSerializeRelations()
		{
			return false;
		}

		// Token: 0x06008D3E RID: 36158 RVA: 0x001BA0B0 File Offset: 0x001B82B0
		[DebuggerNonUserCode]
		[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
		protected override void ReadXmlSerializable(XmlReader reader)
		{
			if (base.DetermineSchemaSerializationMode(reader) == SchemaSerializationMode.IncludeSchema)
			{
				this.Reset();
				DataSet dataSet = new DataSet();
				dataSet.ReadXml(reader);
				if (dataSet.Tables["SecurityCategories"] != null)
				{
					base.Tables.Add(new SecurityCategoriesDataSet.SecurityCategoriesDataTable(dataSet.Tables["SecurityCategories"]));
				}
				if (dataSet.Tables["UserRelations"] != null)
				{
					base.Tables.Add(new SecurityCategoriesDataSet.UserRelationsDataTable(dataSet.Tables["UserRelations"]));
				}
				if (dataSet.Tables["GroupRelations"] != null)
				{
					base.Tables.Add(new SecurityCategoriesDataSet.GroupRelationsDataTable(dataSet.Tables["GroupRelations"]));
				}
				if (dataSet.Tables["UserPermissions"] != null)
				{
					base.Tables.Add(new SecurityCategoriesDataSet.UserPermissionsDataTable(dataSet.Tables["UserPermissions"]));
				}
				if (dataSet.Tables["GroupPermissions"] != null)
				{
					base.Tables.Add(new SecurityCategoriesDataSet.GroupPermissionsDataTable(dataSet.Tables["GroupPermissions"]));
				}
				if (dataSet.Tables["SecurityCategoryObjects"] != null)
				{
					base.Tables.Add(new SecurityCategoriesDataSet.SecurityCategoryObjectsDataTable(dataSet.Tables["SecurityCategoryObjects"]));
				}
				if (dataSet.Tables["SecurityCategoryRules"] != null)
				{
					base.Tables.Add(new SecurityCategoriesDataSet.SecurityCategoryRulesDataTable(dataSet.Tables["SecurityCategoryRules"]));
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

		// Token: 0x06008D3F RID: 36159 RVA: 0x001BA2A4 File Offset: 0x001B84A4
		[DebuggerNonUserCode]
		[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
		protected override XmlSchema GetSchemaSerializable()
		{
			MemoryStream memoryStream = new MemoryStream();
			base.WriteXmlSchema(new XmlTextWriter(memoryStream, null));
			memoryStream.Position = 0L;
			return XmlSchema.Read(new XmlTextReader(memoryStream), null);
		}

		// Token: 0x06008D40 RID: 36160 RVA: 0x001BA2D8 File Offset: 0x001B84D8
		[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
		[DebuggerNonUserCode]
		internal void InitVars()
		{
			this.InitVars(true);
		}

		// Token: 0x06008D41 RID: 36161 RVA: 0x001BA2E4 File Offset: 0x001B84E4
		[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
		[DebuggerNonUserCode]
		internal void InitVars(bool initTable)
		{
			this.tableSecurityCategories = (SecurityCategoriesDataSet.SecurityCategoriesDataTable)base.Tables["SecurityCategories"];
			if (initTable && this.tableSecurityCategories != null)
			{
				this.tableSecurityCategories.InitVars();
			}
			this.tableUserRelations = (SecurityCategoriesDataSet.UserRelationsDataTable)base.Tables["UserRelations"];
			if (initTable && this.tableUserRelations != null)
			{
				this.tableUserRelations.InitVars();
			}
			this.tableGroupRelations = (SecurityCategoriesDataSet.GroupRelationsDataTable)base.Tables["GroupRelations"];
			if (initTable && this.tableGroupRelations != null)
			{
				this.tableGroupRelations.InitVars();
			}
			this.tableUserPermissions = (SecurityCategoriesDataSet.UserPermissionsDataTable)base.Tables["UserPermissions"];
			if (initTable && this.tableUserPermissions != null)
			{
				this.tableUserPermissions.InitVars();
			}
			this.tableGroupPermissions = (SecurityCategoriesDataSet.GroupPermissionsDataTable)base.Tables["GroupPermissions"];
			if (initTable && this.tableGroupPermissions != null)
			{
				this.tableGroupPermissions.InitVars();
			}
			this.tableSecurityCategoryObjects = (SecurityCategoriesDataSet.SecurityCategoryObjectsDataTable)base.Tables["SecurityCategoryObjects"];
			if (initTable && this.tableSecurityCategoryObjects != null)
			{
				this.tableSecurityCategoryObjects.InitVars();
			}
			this.tableSecurityCategoryRules = (SecurityCategoriesDataSet.SecurityCategoryRulesDataTable)base.Tables["SecurityCategoryRules"];
			if (initTable && this.tableSecurityCategoryRules != null)
			{
				this.tableSecurityCategoryRules.InitVars();
			}
		}

		// Token: 0x06008D42 RID: 36162 RVA: 0x001BA448 File Offset: 0x001B8648
		[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
		[DebuggerNonUserCode]
		private void InitClass()
		{
			base.DataSetName = "SecurityCategoriesDataSet";
			base.Prefix = "";
			base.Namespace = "http://schemas.microsoft.com/office/project/server/webservices/SecurityCategoriesDataSet/";
			base.EnforceConstraints = false;
			this.SchemaSerializationMode = SchemaSerializationMode.IncludeSchema;
			this.tableSecurityCategories = new SecurityCategoriesDataSet.SecurityCategoriesDataTable();
			base.Tables.Add(this.tableSecurityCategories);
			this.tableUserRelations = new SecurityCategoriesDataSet.UserRelationsDataTable();
			base.Tables.Add(this.tableUserRelations);
			this.tableGroupRelations = new SecurityCategoriesDataSet.GroupRelationsDataTable();
			base.Tables.Add(this.tableGroupRelations);
			this.tableUserPermissions = new SecurityCategoriesDataSet.UserPermissionsDataTable();
			base.Tables.Add(this.tableUserPermissions);
			this.tableGroupPermissions = new SecurityCategoriesDataSet.GroupPermissionsDataTable();
			base.Tables.Add(this.tableGroupPermissions);
			this.tableSecurityCategoryObjects = new SecurityCategoriesDataSet.SecurityCategoryObjectsDataTable();
			base.Tables.Add(this.tableSecurityCategoryObjects);
			this.tableSecurityCategoryRules = new SecurityCategoriesDataSet.SecurityCategoryRulesDataTable();
			base.Tables.Add(this.tableSecurityCategoryRules);
		}

		// Token: 0x06008D43 RID: 36163 RVA: 0x001BA548 File Offset: 0x001B8748
		[DebuggerNonUserCode]
		[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
		private bool ShouldSerializeSecurityCategories()
		{
			return false;
		}

		// Token: 0x06008D44 RID: 36164 RVA: 0x001BA54B File Offset: 0x001B874B
		[DebuggerNonUserCode]
		[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
		private bool ShouldSerializeUserRelations()
		{
			return false;
		}

		// Token: 0x06008D45 RID: 36165 RVA: 0x001BA54E File Offset: 0x001B874E
		[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
		[DebuggerNonUserCode]
		private bool ShouldSerializeGroupRelations()
		{
			return false;
		}

		// Token: 0x06008D46 RID: 36166 RVA: 0x001BA551 File Offset: 0x001B8751
		[DebuggerNonUserCode]
		[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
		private bool ShouldSerializeUserPermissions()
		{
			return false;
		}

		// Token: 0x06008D47 RID: 36167 RVA: 0x001BA554 File Offset: 0x001B8754
		[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
		[DebuggerNonUserCode]
		private bool ShouldSerializeGroupPermissions()
		{
			return false;
		}

		// Token: 0x06008D48 RID: 36168 RVA: 0x001BA557 File Offset: 0x001B8757
		[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
		[DebuggerNonUserCode]
		private bool ShouldSerializeSecurityCategoryObjects()
		{
			return false;
		}

		// Token: 0x06008D49 RID: 36169 RVA: 0x001BA55A File Offset: 0x001B875A
		[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
		[DebuggerNonUserCode]
		private bool ShouldSerializeSecurityCategoryRules()
		{
			return false;
		}

		// Token: 0x06008D4A RID: 36170 RVA: 0x001BA55D File Offset: 0x001B875D
		[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
		[DebuggerNonUserCode]
		private void SchemaChanged(object sender, CollectionChangeEventArgs e)
		{
			if (e.Action == CollectionChangeAction.Remove)
			{
				this.InitVars();
			}
		}

		// Token: 0x06008D4B RID: 36171 RVA: 0x001BA570 File Offset: 0x001B8770
		[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
		[DebuggerNonUserCode]
		public static XmlSchemaComplexType GetTypedDataSetSchema(XmlSchemaSet xs)
		{
			SecurityCategoriesDataSet securityCategoriesDataSet = new SecurityCategoriesDataSet();
			XmlSchemaComplexType xmlSchemaComplexType = new XmlSchemaComplexType();
			XmlSchemaSequence xmlSchemaSequence = new XmlSchemaSequence();
			XmlSchemaAny xmlSchemaAny = new XmlSchemaAny();
			xmlSchemaAny.Namespace = securityCategoriesDataSet.Namespace;
			xmlSchemaSequence.Items.Add(xmlSchemaAny);
			xmlSchemaComplexType.Particle = xmlSchemaSequence;
			XmlSchema schemaSerializable = securityCategoriesDataSet.GetSchemaSerializable();
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

		// Token: 0x04001C47 RID: 7239
		private SecurityCategoriesDataSet.SecurityCategoriesDataTable tableSecurityCategories;

		// Token: 0x04001C48 RID: 7240
		private SecurityCategoriesDataSet.UserRelationsDataTable tableUserRelations;

		// Token: 0x04001C49 RID: 7241
		private SecurityCategoriesDataSet.GroupRelationsDataTable tableGroupRelations;

		// Token: 0x04001C4A RID: 7242
		private SecurityCategoriesDataSet.UserPermissionsDataTable tableUserPermissions;

		// Token: 0x04001C4B RID: 7243
		private SecurityCategoriesDataSet.GroupPermissionsDataTable tableGroupPermissions;

		// Token: 0x04001C4C RID: 7244
		private SecurityCategoriesDataSet.SecurityCategoryObjectsDataTable tableSecurityCategoryObjects;

		// Token: 0x04001C4D RID: 7245
		private SecurityCategoriesDataSet.SecurityCategoryRulesDataTable tableSecurityCategoryRules;

		// Token: 0x04001C4E RID: 7246
		private SchemaSerializationMode _schemaSerializationMode = SchemaSerializationMode.IncludeSchema;

		// Token: 0x020005BF RID: 1471
		// (Invoke) Token: 0x06008D4D RID: 36173
		[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
		public delegate void SecurityCategoriesRowChangeEventHandler(object sender, SecurityCategoriesDataSet.SecurityCategoriesRowChangeEvent e);

		// Token: 0x020005C0 RID: 1472
		// (Invoke) Token: 0x06008D51 RID: 36177
		[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
		public delegate void UserRelationsRowChangeEventHandler(object sender, SecurityCategoriesDataSet.UserRelationsRowChangeEvent e);

		// Token: 0x020005C1 RID: 1473
		// (Invoke) Token: 0x06008D55 RID: 36181
		[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
		public delegate void GroupRelationsRowChangeEventHandler(object sender, SecurityCategoriesDataSet.GroupRelationsRowChangeEvent e);

		// Token: 0x020005C2 RID: 1474
		// (Invoke) Token: 0x06008D59 RID: 36185
		[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
		public delegate void UserPermissionsRowChangeEventHandler(object sender, SecurityCategoriesDataSet.UserPermissionsRowChangeEvent e);

		// Token: 0x020005C3 RID: 1475
		// (Invoke) Token: 0x06008D5D RID: 36189
		[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
		public delegate void GroupPermissionsRowChangeEventHandler(object sender, SecurityCategoriesDataSet.GroupPermissionsRowChangeEvent e);

		// Token: 0x020005C4 RID: 1476
		// (Invoke) Token: 0x06008D61 RID: 36193
		[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
		public delegate void SecurityCategoryObjectsRowChangeEventHandler(object sender, SecurityCategoriesDataSet.SecurityCategoryObjectsRowChangeEvent e);

		// Token: 0x020005C5 RID: 1477
		// (Invoke) Token: 0x06008D65 RID: 36197
		[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
		public delegate void SecurityCategoryRulesRowChangeEventHandler(object sender, SecurityCategoriesDataSet.SecurityCategoryRulesRowChangeEvent e);

		// Token: 0x020005C6 RID: 1478
		[XmlSchemaProvider("GetTypedTableSchema")]
		[Serializable]
		public class SecurityCategoriesDataTable : DataTable, IEnumerable
		{
			// Token: 0x06008D68 RID: 36200 RVA: 0x001BA6B8 File Offset: 0x001B88B8
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public SecurityCategoriesDataTable()
			{
				base.TableName = "SecurityCategories";
				this.BeginInit();
				this.InitClass();
				this.EndInit();
			}

			// Token: 0x06008D69 RID: 36201 RVA: 0x001BA6E0 File Offset: 0x001B88E0
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			internal SecurityCategoriesDataTable(DataTable table)
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

			// Token: 0x06008D6A RID: 36202 RVA: 0x001BA788 File Offset: 0x001B8988
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			protected SecurityCategoriesDataTable(SerializationInfo info, StreamingContext context) : base(info, context)
			{
				this.InitVars();
			}

			// Token: 0x17002AD1 RID: 10961
			// (get) Token: 0x06008D6B RID: 36203 RVA: 0x001BA798 File Offset: 0x001B8998
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			public DataColumn WSEC_CAT_UIDColumn
			{
				get
				{
					return this.columnWSEC_CAT_UID;
				}
			}

			// Token: 0x17002AD2 RID: 10962
			// (get) Token: 0x06008D6C RID: 36204 RVA: 0x001BA7A0 File Offset: 0x001B89A0
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			public DataColumn WSEC_CAT_NAMEColumn
			{
				get
				{
					return this.columnWSEC_CAT_NAME;
				}
			}

			// Token: 0x17002AD3 RID: 10963
			// (get) Token: 0x06008D6D RID: 36205 RVA: 0x001BA7A8 File Offset: 0x001B89A8
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			public DataColumn WSEC_CAT_DESCColumn
			{
				get
				{
					return this.columnWSEC_CAT_DESC;
				}
			}

			// Token: 0x17002AD4 RID: 10964
			// (get) Token: 0x06008D6E RID: 36206 RVA: 0x001BA7B0 File Offset: 0x001B89B0
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

			// Token: 0x17002AD5 RID: 10965
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public SecurityCategoriesDataSet.SecurityCategoriesRow this[int index]
			{
				get
				{
					return (SecurityCategoriesDataSet.SecurityCategoriesRow)base.Rows[index];
				}
			}

			// Token: 0x140004F1 RID: 1265
			// (add) Token: 0x06008D70 RID: 36208 RVA: 0x001BA7D0 File Offset: 0x001B89D0
			// (remove) Token: 0x06008D71 RID: 36209 RVA: 0x001BA808 File Offset: 0x001B8A08
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public event SecurityCategoriesDataSet.SecurityCategoriesRowChangeEventHandler SecurityCategoriesRowChanging;

			// Token: 0x140004F2 RID: 1266
			// (add) Token: 0x06008D72 RID: 36210 RVA: 0x001BA840 File Offset: 0x001B8A40
			// (remove) Token: 0x06008D73 RID: 36211 RVA: 0x001BA878 File Offset: 0x001B8A78
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public event SecurityCategoriesDataSet.SecurityCategoriesRowChangeEventHandler SecurityCategoriesRowChanged;

			// Token: 0x140004F3 RID: 1267
			// (add) Token: 0x06008D74 RID: 36212 RVA: 0x001BA8B0 File Offset: 0x001B8AB0
			// (remove) Token: 0x06008D75 RID: 36213 RVA: 0x001BA8E8 File Offset: 0x001B8AE8
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public event SecurityCategoriesDataSet.SecurityCategoriesRowChangeEventHandler SecurityCategoriesRowDeleting;

			// Token: 0x140004F4 RID: 1268
			// (add) Token: 0x06008D76 RID: 36214 RVA: 0x001BA920 File Offset: 0x001B8B20
			// (remove) Token: 0x06008D77 RID: 36215 RVA: 0x001BA958 File Offset: 0x001B8B58
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public event SecurityCategoriesDataSet.SecurityCategoriesRowChangeEventHandler SecurityCategoriesRowDeleted;

			// Token: 0x06008D78 RID: 36216 RVA: 0x001BA98D File Offset: 0x001B8B8D
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public void AddSecurityCategoriesRow(SecurityCategoriesDataSet.SecurityCategoriesRow row)
			{
				base.Rows.Add(row);
			}

			// Token: 0x06008D79 RID: 36217 RVA: 0x001BA99C File Offset: 0x001B8B9C
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			public SecurityCategoriesDataSet.SecurityCategoriesRow AddSecurityCategoriesRow(Guid WSEC_CAT_UID, string WSEC_CAT_NAME, string WSEC_CAT_DESC)
			{
				SecurityCategoriesDataSet.SecurityCategoriesRow securityCategoriesRow = (SecurityCategoriesDataSet.SecurityCategoriesRow)base.NewRow();
				object[] itemArray = new object[]
				{
					WSEC_CAT_UID,
					WSEC_CAT_NAME,
					WSEC_CAT_DESC
				};
				securityCategoriesRow.ItemArray = itemArray;
				base.Rows.Add(securityCategoriesRow);
				return securityCategoriesRow;
			}

			// Token: 0x06008D7A RID: 36218 RVA: 0x001BA9E4 File Offset: 0x001B8BE4
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public SecurityCategoriesDataSet.SecurityCategoriesRow FindByWSEC_CAT_UID(Guid WSEC_CAT_UID)
			{
				return (SecurityCategoriesDataSet.SecurityCategoriesRow)base.Rows.Find(new object[]
				{
					WSEC_CAT_UID
				});
			}

			// Token: 0x06008D7B RID: 36219 RVA: 0x001BAA12 File Offset: 0x001B8C12
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			public virtual IEnumerator GetEnumerator()
			{
				return base.Rows.GetEnumerator();
			}

			// Token: 0x06008D7C RID: 36220 RVA: 0x001BAA20 File Offset: 0x001B8C20
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public override DataTable Clone()
			{
				SecurityCategoriesDataSet.SecurityCategoriesDataTable securityCategoriesDataTable = (SecurityCategoriesDataSet.SecurityCategoriesDataTable)base.Clone();
				securityCategoriesDataTable.InitVars();
				return securityCategoriesDataTable;
			}

			// Token: 0x06008D7D RID: 36221 RVA: 0x001BAA40 File Offset: 0x001B8C40
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			protected override DataTable CreateInstance()
			{
				return new SecurityCategoriesDataSet.SecurityCategoriesDataTable();
			}

			// Token: 0x06008D7E RID: 36222 RVA: 0x001BAA48 File Offset: 0x001B8C48
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			internal void InitVars()
			{
				this.columnWSEC_CAT_UID = base.Columns["WSEC_CAT_UID"];
				this.columnWSEC_CAT_NAME = base.Columns["WSEC_CAT_NAME"];
				this.columnWSEC_CAT_DESC = base.Columns["WSEC_CAT_DESC"];
			}

			// Token: 0x06008D7F RID: 36223 RVA: 0x001BAA98 File Offset: 0x001B8C98
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			private void InitClass()
			{
				this.columnWSEC_CAT_UID = new DataColumn("WSEC_CAT_UID", typeof(Guid), null, MappingType.Element);
				base.Columns.Add(this.columnWSEC_CAT_UID);
				this.columnWSEC_CAT_NAME = new DataColumn("WSEC_CAT_NAME", typeof(string), null, MappingType.Element);
				base.Columns.Add(this.columnWSEC_CAT_NAME);
				this.columnWSEC_CAT_DESC = new DataColumn("WSEC_CAT_DESC", typeof(string), null, MappingType.Element);
				base.Columns.Add(this.columnWSEC_CAT_DESC);
				base.Constraints.Add(new UniqueConstraint("SecurityCategoriesPrimaryKey", new DataColumn[]
				{
					this.columnWSEC_CAT_UID
				}, true));
				this.columnWSEC_CAT_UID.AllowDBNull = false;
				this.columnWSEC_CAT_UID.Unique = true;
				this.columnWSEC_CAT_NAME.AllowDBNull = false;
				this.columnWSEC_CAT_NAME.DefaultValue = "";
			}

			// Token: 0x06008D80 RID: 36224 RVA: 0x001BAB87 File Offset: 0x001B8D87
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public SecurityCategoriesDataSet.SecurityCategoriesRow NewSecurityCategoriesRow()
			{
				return (SecurityCategoriesDataSet.SecurityCategoriesRow)base.NewRow();
			}

			// Token: 0x06008D81 RID: 36225 RVA: 0x001BAB94 File Offset: 0x001B8D94
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			protected override DataRow NewRowFromBuilder(DataRowBuilder builder)
			{
				return new SecurityCategoriesDataSet.SecurityCategoriesRow(builder);
			}

			// Token: 0x06008D82 RID: 36226 RVA: 0x001BAB9C File Offset: 0x001B8D9C
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			protected override Type GetRowType()
			{
				return typeof(SecurityCategoriesDataSet.SecurityCategoriesRow);
			}

			// Token: 0x06008D83 RID: 36227 RVA: 0x001BABA8 File Offset: 0x001B8DA8
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			protected override void OnRowChanged(DataRowChangeEventArgs e)
			{
				base.OnRowChanged(e);
				if (this.SecurityCategoriesRowChanged != null)
				{
					this.SecurityCategoriesRowChanged(this, new SecurityCategoriesDataSet.SecurityCategoriesRowChangeEvent((SecurityCategoriesDataSet.SecurityCategoriesRow)e.Row, e.Action));
				}
			}

			// Token: 0x06008D84 RID: 36228 RVA: 0x001BABDB File Offset: 0x001B8DDB
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			protected override void OnRowChanging(DataRowChangeEventArgs e)
			{
				base.OnRowChanging(e);
				if (this.SecurityCategoriesRowChanging != null)
				{
					this.SecurityCategoriesRowChanging(this, new SecurityCategoriesDataSet.SecurityCategoriesRowChangeEvent((SecurityCategoriesDataSet.SecurityCategoriesRow)e.Row, e.Action));
				}
			}

			// Token: 0x06008D85 RID: 36229 RVA: 0x001BAC0E File Offset: 0x001B8E0E
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			protected override void OnRowDeleted(DataRowChangeEventArgs e)
			{
				base.OnRowDeleted(e);
				if (this.SecurityCategoriesRowDeleted != null)
				{
					this.SecurityCategoriesRowDeleted(this, new SecurityCategoriesDataSet.SecurityCategoriesRowChangeEvent((SecurityCategoriesDataSet.SecurityCategoriesRow)e.Row, e.Action));
				}
			}

			// Token: 0x06008D86 RID: 36230 RVA: 0x001BAC41 File Offset: 0x001B8E41
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			protected override void OnRowDeleting(DataRowChangeEventArgs e)
			{
				base.OnRowDeleting(e);
				if (this.SecurityCategoriesRowDeleting != null)
				{
					this.SecurityCategoriesRowDeleting(this, new SecurityCategoriesDataSet.SecurityCategoriesRowChangeEvent((SecurityCategoriesDataSet.SecurityCategoriesRow)e.Row, e.Action));
				}
			}

			// Token: 0x06008D87 RID: 36231 RVA: 0x001BAC74 File Offset: 0x001B8E74
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			public void RemoveSecurityCategoriesRow(SecurityCategoriesDataSet.SecurityCategoriesRow row)
			{
				base.Rows.Remove(row);
			}

			// Token: 0x06008D88 RID: 36232 RVA: 0x001BAC84 File Offset: 0x001B8E84
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			public static XmlSchemaComplexType GetTypedTableSchema(XmlSchemaSet xs)
			{
				XmlSchemaComplexType xmlSchemaComplexType = new XmlSchemaComplexType();
				XmlSchemaSequence xmlSchemaSequence = new XmlSchemaSequence();
				SecurityCategoriesDataSet securityCategoriesDataSet = new SecurityCategoriesDataSet();
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
				xmlSchemaAttribute.FixedValue = securityCategoriesDataSet.Namespace;
				xmlSchemaComplexType.Attributes.Add(xmlSchemaAttribute);
				XmlSchemaAttribute xmlSchemaAttribute2 = new XmlSchemaAttribute();
				xmlSchemaAttribute2.Name = "tableTypeName";
				xmlSchemaAttribute2.FixedValue = "SecurityCategoriesDataTable";
				xmlSchemaComplexType.Attributes.Add(xmlSchemaAttribute2);
				xmlSchemaComplexType.Particle = xmlSchemaSequence;
				XmlSchema schemaSerializable = securityCategoriesDataSet.GetSchemaSerializable();
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

			// Token: 0x04001C4F RID: 7247
			private DataColumn columnWSEC_CAT_UID;

			// Token: 0x04001C50 RID: 7248
			private DataColumn columnWSEC_CAT_NAME;

			// Token: 0x04001C51 RID: 7249
			private DataColumn columnWSEC_CAT_DESC;
		}

		// Token: 0x020005C7 RID: 1479
		[XmlSchemaProvider("GetTypedTableSchema")]
		[Serializable]
		public class UserRelationsDataTable : DataTable, IEnumerable
		{
			// Token: 0x06008D89 RID: 36233 RVA: 0x001BAE7C File Offset: 0x001B907C
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public UserRelationsDataTable()
			{
				base.TableName = "UserRelations";
				this.BeginInit();
				this.InitClass();
				this.EndInit();
			}

			// Token: 0x06008D8A RID: 36234 RVA: 0x001BAEA4 File Offset: 0x001B90A4
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
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

			// Token: 0x06008D8B RID: 36235 RVA: 0x001BAF4C File Offset: 0x001B914C
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			protected UserRelationsDataTable(SerializationInfo info, StreamingContext context) : base(info, context)
			{
				this.InitVars();
			}

			// Token: 0x17002AD6 RID: 10966
			// (get) Token: 0x06008D8C RID: 36236 RVA: 0x001BAF5C File Offset: 0x001B915C
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public DataColumn WSEC_CAT_UIDColumn
			{
				get
				{
					return this.columnWSEC_CAT_UID;
				}
			}

			// Token: 0x17002AD7 RID: 10967
			// (get) Token: 0x06008D8D RID: 36237 RVA: 0x001BAF64 File Offset: 0x001B9164
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public DataColumn RES_UIDColumn
			{
				get
				{
					return this.columnRES_UID;
				}
			}

			// Token: 0x17002AD8 RID: 10968
			// (get) Token: 0x06008D8E RID: 36238 RVA: 0x001BAF6C File Offset: 0x001B916C
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

			// Token: 0x17002AD9 RID: 10969
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public SecurityCategoriesDataSet.UserRelationsRow this[int index]
			{
				get
				{
					return (SecurityCategoriesDataSet.UserRelationsRow)base.Rows[index];
				}
			}

			// Token: 0x140004F5 RID: 1269
			// (add) Token: 0x06008D90 RID: 36240 RVA: 0x001BAF8C File Offset: 0x001B918C
			// (remove) Token: 0x06008D91 RID: 36241 RVA: 0x001BAFC4 File Offset: 0x001B91C4
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public event SecurityCategoriesDataSet.UserRelationsRowChangeEventHandler UserRelationsRowChanging;

			// Token: 0x140004F6 RID: 1270
			// (add) Token: 0x06008D92 RID: 36242 RVA: 0x001BAFFC File Offset: 0x001B91FC
			// (remove) Token: 0x06008D93 RID: 36243 RVA: 0x001BB034 File Offset: 0x001B9234
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public event SecurityCategoriesDataSet.UserRelationsRowChangeEventHandler UserRelationsRowChanged;

			// Token: 0x140004F7 RID: 1271
			// (add) Token: 0x06008D94 RID: 36244 RVA: 0x001BB06C File Offset: 0x001B926C
			// (remove) Token: 0x06008D95 RID: 36245 RVA: 0x001BB0A4 File Offset: 0x001B92A4
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public event SecurityCategoriesDataSet.UserRelationsRowChangeEventHandler UserRelationsRowDeleting;

			// Token: 0x140004F8 RID: 1272
			// (add) Token: 0x06008D96 RID: 36246 RVA: 0x001BB0DC File Offset: 0x001B92DC
			// (remove) Token: 0x06008D97 RID: 36247 RVA: 0x001BB114 File Offset: 0x001B9314
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public event SecurityCategoriesDataSet.UserRelationsRowChangeEventHandler UserRelationsRowDeleted;

			// Token: 0x06008D98 RID: 36248 RVA: 0x001BB149 File Offset: 0x001B9349
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			public void AddUserRelationsRow(SecurityCategoriesDataSet.UserRelationsRow row)
			{
				base.Rows.Add(row);
			}

			// Token: 0x06008D99 RID: 36249 RVA: 0x001BB158 File Offset: 0x001B9358
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			public SecurityCategoriesDataSet.UserRelationsRow AddUserRelationsRow(Guid WSEC_CAT_UID, Guid RES_UID)
			{
				SecurityCategoriesDataSet.UserRelationsRow userRelationsRow = (SecurityCategoriesDataSet.UserRelationsRow)base.NewRow();
				object[] itemArray = new object[]
				{
					WSEC_CAT_UID,
					RES_UID
				};
				userRelationsRow.ItemArray = itemArray;
				base.Rows.Add(userRelationsRow);
				return userRelationsRow;
			}

			// Token: 0x06008D9A RID: 36250 RVA: 0x001BB1A0 File Offset: 0x001B93A0
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			public SecurityCategoriesDataSet.UserRelationsRow FindByWSEC_CAT_UIDRES_UID(Guid WSEC_CAT_UID, Guid RES_UID)
			{
				return (SecurityCategoriesDataSet.UserRelationsRow)base.Rows.Find(new object[]
				{
					WSEC_CAT_UID,
					RES_UID
				});
			}

			// Token: 0x06008D9B RID: 36251 RVA: 0x001BB1D7 File Offset: 0x001B93D7
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public virtual IEnumerator GetEnumerator()
			{
				return base.Rows.GetEnumerator();
			}

			// Token: 0x06008D9C RID: 36252 RVA: 0x001BB1E4 File Offset: 0x001B93E4
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			public override DataTable Clone()
			{
				SecurityCategoriesDataSet.UserRelationsDataTable userRelationsDataTable = (SecurityCategoriesDataSet.UserRelationsDataTable)base.Clone();
				userRelationsDataTable.InitVars();
				return userRelationsDataTable;
			}

			// Token: 0x06008D9D RID: 36253 RVA: 0x001BB204 File Offset: 0x001B9404
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			protected override DataTable CreateInstance()
			{
				return new SecurityCategoriesDataSet.UserRelationsDataTable();
			}

			// Token: 0x06008D9E RID: 36254 RVA: 0x001BB20B File Offset: 0x001B940B
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			internal void InitVars()
			{
				this.columnWSEC_CAT_UID = base.Columns["WSEC_CAT_UID"];
				this.columnRES_UID = base.Columns["RES_UID"];
			}

			// Token: 0x06008D9F RID: 36255 RVA: 0x001BB23C File Offset: 0x001B943C
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
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

			// Token: 0x06008DA0 RID: 36256 RVA: 0x001BB2EB File Offset: 0x001B94EB
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public SecurityCategoriesDataSet.UserRelationsRow NewUserRelationsRow()
			{
				return (SecurityCategoriesDataSet.UserRelationsRow)base.NewRow();
			}

			// Token: 0x06008DA1 RID: 36257 RVA: 0x001BB2F8 File Offset: 0x001B94F8
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			protected override DataRow NewRowFromBuilder(DataRowBuilder builder)
			{
				return new SecurityCategoriesDataSet.UserRelationsRow(builder);
			}

			// Token: 0x06008DA2 RID: 36258 RVA: 0x001BB300 File Offset: 0x001B9500
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			protected override Type GetRowType()
			{
				return typeof(SecurityCategoriesDataSet.UserRelationsRow);
			}

			// Token: 0x06008DA3 RID: 36259 RVA: 0x001BB30C File Offset: 0x001B950C
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			protected override void OnRowChanged(DataRowChangeEventArgs e)
			{
				base.OnRowChanged(e);
				if (this.UserRelationsRowChanged != null)
				{
					this.UserRelationsRowChanged(this, new SecurityCategoriesDataSet.UserRelationsRowChangeEvent((SecurityCategoriesDataSet.UserRelationsRow)e.Row, e.Action));
				}
			}

			// Token: 0x06008DA4 RID: 36260 RVA: 0x001BB33F File Offset: 0x001B953F
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			protected override void OnRowChanging(DataRowChangeEventArgs e)
			{
				base.OnRowChanging(e);
				if (this.UserRelationsRowChanging != null)
				{
					this.UserRelationsRowChanging(this, new SecurityCategoriesDataSet.UserRelationsRowChangeEvent((SecurityCategoriesDataSet.UserRelationsRow)e.Row, e.Action));
				}
			}

			// Token: 0x06008DA5 RID: 36261 RVA: 0x001BB372 File Offset: 0x001B9572
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			protected override void OnRowDeleted(DataRowChangeEventArgs e)
			{
				base.OnRowDeleted(e);
				if (this.UserRelationsRowDeleted != null)
				{
					this.UserRelationsRowDeleted(this, new SecurityCategoriesDataSet.UserRelationsRowChangeEvent((SecurityCategoriesDataSet.UserRelationsRow)e.Row, e.Action));
				}
			}

			// Token: 0x06008DA6 RID: 36262 RVA: 0x001BB3A5 File Offset: 0x001B95A5
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			protected override void OnRowDeleting(DataRowChangeEventArgs e)
			{
				base.OnRowDeleting(e);
				if (this.UserRelationsRowDeleting != null)
				{
					this.UserRelationsRowDeleting(this, new SecurityCategoriesDataSet.UserRelationsRowChangeEvent((SecurityCategoriesDataSet.UserRelationsRow)e.Row, e.Action));
				}
			}

			// Token: 0x06008DA7 RID: 36263 RVA: 0x001BB3D8 File Offset: 0x001B95D8
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			public void RemoveUserRelationsRow(SecurityCategoriesDataSet.UserRelationsRow row)
			{
				base.Rows.Remove(row);
			}

			// Token: 0x06008DA8 RID: 36264 RVA: 0x001BB3E8 File Offset: 0x001B95E8
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			public static XmlSchemaComplexType GetTypedTableSchema(XmlSchemaSet xs)
			{
				XmlSchemaComplexType xmlSchemaComplexType = new XmlSchemaComplexType();
				XmlSchemaSequence xmlSchemaSequence = new XmlSchemaSequence();
				SecurityCategoriesDataSet securityCategoriesDataSet = new SecurityCategoriesDataSet();
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
				xmlSchemaAttribute.FixedValue = securityCategoriesDataSet.Namespace;
				xmlSchemaComplexType.Attributes.Add(xmlSchemaAttribute);
				XmlSchemaAttribute xmlSchemaAttribute2 = new XmlSchemaAttribute();
				xmlSchemaAttribute2.Name = "tableTypeName";
				xmlSchemaAttribute2.FixedValue = "UserRelationsDataTable";
				xmlSchemaComplexType.Attributes.Add(xmlSchemaAttribute2);
				xmlSchemaComplexType.Particle = xmlSchemaSequence;
				XmlSchema schemaSerializable = securityCategoriesDataSet.GetSchemaSerializable();
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

			// Token: 0x04001C56 RID: 7254
			private DataColumn columnWSEC_CAT_UID;

			// Token: 0x04001C57 RID: 7255
			private DataColumn columnRES_UID;
		}

		// Token: 0x020005C8 RID: 1480
		[XmlSchemaProvider("GetTypedTableSchema")]
		[Serializable]
		public class GroupRelationsDataTable : DataTable, IEnumerable
		{
			// Token: 0x06008DA9 RID: 36265 RVA: 0x001BB5E0 File Offset: 0x001B97E0
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public GroupRelationsDataTable()
			{
				base.TableName = "GroupRelations";
				this.BeginInit();
				this.InitClass();
				this.EndInit();
			}

			// Token: 0x06008DAA RID: 36266 RVA: 0x001BB608 File Offset: 0x001B9808
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
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

			// Token: 0x06008DAB RID: 36267 RVA: 0x001BB6B0 File Offset: 0x001B98B0
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			protected GroupRelationsDataTable(SerializationInfo info, StreamingContext context) : base(info, context)
			{
				this.InitVars();
			}

			// Token: 0x17002ADA RID: 10970
			// (get) Token: 0x06008DAC RID: 36268 RVA: 0x001BB6C0 File Offset: 0x001B98C0
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			public DataColumn WSEC_CAT_UIDColumn
			{
				get
				{
					return this.columnWSEC_CAT_UID;
				}
			}

			// Token: 0x17002ADB RID: 10971
			// (get) Token: 0x06008DAD RID: 36269 RVA: 0x001BB6C8 File Offset: 0x001B98C8
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public DataColumn WSEC_GRP_UIDColumn
			{
				get
				{
					return this.columnWSEC_GRP_UID;
				}
			}

			// Token: 0x17002ADC RID: 10972
			// (get) Token: 0x06008DAE RID: 36270 RVA: 0x001BB6D0 File Offset: 0x001B98D0
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

			// Token: 0x17002ADD RID: 10973
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			public SecurityCategoriesDataSet.GroupRelationsRow this[int index]
			{
				get
				{
					return (SecurityCategoriesDataSet.GroupRelationsRow)base.Rows[index];
				}
			}

			// Token: 0x140004F9 RID: 1273
			// (add) Token: 0x06008DB0 RID: 36272 RVA: 0x001BB6F0 File Offset: 0x001B98F0
			// (remove) Token: 0x06008DB1 RID: 36273 RVA: 0x001BB728 File Offset: 0x001B9928
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public event SecurityCategoriesDataSet.GroupRelationsRowChangeEventHandler GroupRelationsRowChanging;

			// Token: 0x140004FA RID: 1274
			// (add) Token: 0x06008DB2 RID: 36274 RVA: 0x001BB760 File Offset: 0x001B9960
			// (remove) Token: 0x06008DB3 RID: 36275 RVA: 0x001BB798 File Offset: 0x001B9998
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public event SecurityCategoriesDataSet.GroupRelationsRowChangeEventHandler GroupRelationsRowChanged;

			// Token: 0x140004FB RID: 1275
			// (add) Token: 0x06008DB4 RID: 36276 RVA: 0x001BB7D0 File Offset: 0x001B99D0
			// (remove) Token: 0x06008DB5 RID: 36277 RVA: 0x001BB808 File Offset: 0x001B9A08
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public event SecurityCategoriesDataSet.GroupRelationsRowChangeEventHandler GroupRelationsRowDeleting;

			// Token: 0x140004FC RID: 1276
			// (add) Token: 0x06008DB6 RID: 36278 RVA: 0x001BB840 File Offset: 0x001B9A40
			// (remove) Token: 0x06008DB7 RID: 36279 RVA: 0x001BB878 File Offset: 0x001B9A78
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public event SecurityCategoriesDataSet.GroupRelationsRowChangeEventHandler GroupRelationsRowDeleted;

			// Token: 0x06008DB8 RID: 36280 RVA: 0x001BB8AD File Offset: 0x001B9AAD
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public void AddGroupRelationsRow(SecurityCategoriesDataSet.GroupRelationsRow row)
			{
				base.Rows.Add(row);
			}

			// Token: 0x06008DB9 RID: 36281 RVA: 0x001BB8BC File Offset: 0x001B9ABC
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public SecurityCategoriesDataSet.GroupRelationsRow AddGroupRelationsRow(Guid WSEC_CAT_UID, Guid WSEC_GRP_UID)
			{
				SecurityCategoriesDataSet.GroupRelationsRow groupRelationsRow = (SecurityCategoriesDataSet.GroupRelationsRow)base.NewRow();
				object[] itemArray = new object[]
				{
					WSEC_CAT_UID,
					WSEC_GRP_UID
				};
				groupRelationsRow.ItemArray = itemArray;
				base.Rows.Add(groupRelationsRow);
				return groupRelationsRow;
			}

			// Token: 0x06008DBA RID: 36282 RVA: 0x001BB904 File Offset: 0x001B9B04
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			public SecurityCategoriesDataSet.GroupRelationsRow FindByWSEC_CAT_UIDWSEC_GRP_UID(Guid WSEC_CAT_UID, Guid WSEC_GRP_UID)
			{
				return (SecurityCategoriesDataSet.GroupRelationsRow)base.Rows.Find(new object[]
				{
					WSEC_CAT_UID,
					WSEC_GRP_UID
				});
			}

			// Token: 0x06008DBB RID: 36283 RVA: 0x001BB93B File Offset: 0x001B9B3B
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public virtual IEnumerator GetEnumerator()
			{
				return base.Rows.GetEnumerator();
			}

			// Token: 0x06008DBC RID: 36284 RVA: 0x001BB948 File Offset: 0x001B9B48
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public override DataTable Clone()
			{
				SecurityCategoriesDataSet.GroupRelationsDataTable groupRelationsDataTable = (SecurityCategoriesDataSet.GroupRelationsDataTable)base.Clone();
				groupRelationsDataTable.InitVars();
				return groupRelationsDataTable;
			}

			// Token: 0x06008DBD RID: 36285 RVA: 0x001BB968 File Offset: 0x001B9B68
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			protected override DataTable CreateInstance()
			{
				return new SecurityCategoriesDataSet.GroupRelationsDataTable();
			}

			// Token: 0x06008DBE RID: 36286 RVA: 0x001BB96F File Offset: 0x001B9B6F
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			internal void InitVars()
			{
				this.columnWSEC_CAT_UID = base.Columns["WSEC_CAT_UID"];
				this.columnWSEC_GRP_UID = base.Columns["WSEC_GRP_UID"];
			}

			// Token: 0x06008DBF RID: 36287 RVA: 0x001BB9A0 File Offset: 0x001B9BA0
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

			// Token: 0x06008DC0 RID: 36288 RVA: 0x001BBA4F File Offset: 0x001B9C4F
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public SecurityCategoriesDataSet.GroupRelationsRow NewGroupRelationsRow()
			{
				return (SecurityCategoriesDataSet.GroupRelationsRow)base.NewRow();
			}

			// Token: 0x06008DC1 RID: 36289 RVA: 0x001BBA5C File Offset: 0x001B9C5C
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			protected override DataRow NewRowFromBuilder(DataRowBuilder builder)
			{
				return new SecurityCategoriesDataSet.GroupRelationsRow(builder);
			}

			// Token: 0x06008DC2 RID: 36290 RVA: 0x001BBA64 File Offset: 0x001B9C64
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			protected override Type GetRowType()
			{
				return typeof(SecurityCategoriesDataSet.GroupRelationsRow);
			}

			// Token: 0x06008DC3 RID: 36291 RVA: 0x001BBA70 File Offset: 0x001B9C70
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			protected override void OnRowChanged(DataRowChangeEventArgs e)
			{
				base.OnRowChanged(e);
				if (this.GroupRelationsRowChanged != null)
				{
					this.GroupRelationsRowChanged(this, new SecurityCategoriesDataSet.GroupRelationsRowChangeEvent((SecurityCategoriesDataSet.GroupRelationsRow)e.Row, e.Action));
				}
			}

			// Token: 0x06008DC4 RID: 36292 RVA: 0x001BBAA3 File Offset: 0x001B9CA3
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			protected override void OnRowChanging(DataRowChangeEventArgs e)
			{
				base.OnRowChanging(e);
				if (this.GroupRelationsRowChanging != null)
				{
					this.GroupRelationsRowChanging(this, new SecurityCategoriesDataSet.GroupRelationsRowChangeEvent((SecurityCategoriesDataSet.GroupRelationsRow)e.Row, e.Action));
				}
			}

			// Token: 0x06008DC5 RID: 36293 RVA: 0x001BBAD6 File Offset: 0x001B9CD6
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			protected override void OnRowDeleted(DataRowChangeEventArgs e)
			{
				base.OnRowDeleted(e);
				if (this.GroupRelationsRowDeleted != null)
				{
					this.GroupRelationsRowDeleted(this, new SecurityCategoriesDataSet.GroupRelationsRowChangeEvent((SecurityCategoriesDataSet.GroupRelationsRow)e.Row, e.Action));
				}
			}

			// Token: 0x06008DC6 RID: 36294 RVA: 0x001BBB09 File Offset: 0x001B9D09
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			protected override void OnRowDeleting(DataRowChangeEventArgs e)
			{
				base.OnRowDeleting(e);
				if (this.GroupRelationsRowDeleting != null)
				{
					this.GroupRelationsRowDeleting(this, new SecurityCategoriesDataSet.GroupRelationsRowChangeEvent((SecurityCategoriesDataSet.GroupRelationsRow)e.Row, e.Action));
				}
			}

			// Token: 0x06008DC7 RID: 36295 RVA: 0x001BBB3C File Offset: 0x001B9D3C
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			public void RemoveGroupRelationsRow(SecurityCategoriesDataSet.GroupRelationsRow row)
			{
				base.Rows.Remove(row);
			}

			// Token: 0x06008DC8 RID: 36296 RVA: 0x001BBB4C File Offset: 0x001B9D4C
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			public static XmlSchemaComplexType GetTypedTableSchema(XmlSchemaSet xs)
			{
				XmlSchemaComplexType xmlSchemaComplexType = new XmlSchemaComplexType();
				XmlSchemaSequence xmlSchemaSequence = new XmlSchemaSequence();
				SecurityCategoriesDataSet securityCategoriesDataSet = new SecurityCategoriesDataSet();
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
				xmlSchemaAttribute.FixedValue = securityCategoriesDataSet.Namespace;
				xmlSchemaComplexType.Attributes.Add(xmlSchemaAttribute);
				XmlSchemaAttribute xmlSchemaAttribute2 = new XmlSchemaAttribute();
				xmlSchemaAttribute2.Name = "tableTypeName";
				xmlSchemaAttribute2.FixedValue = "GroupRelationsDataTable";
				xmlSchemaComplexType.Attributes.Add(xmlSchemaAttribute2);
				xmlSchemaComplexType.Particle = xmlSchemaSequence;
				XmlSchema schemaSerializable = securityCategoriesDataSet.GetSchemaSerializable();
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

			// Token: 0x04001C5C RID: 7260
			private DataColumn columnWSEC_CAT_UID;

			// Token: 0x04001C5D RID: 7261
			private DataColumn columnWSEC_GRP_UID;
		}

		// Token: 0x020005C9 RID: 1481
		[XmlSchemaProvider("GetTypedTableSchema")]
		[Serializable]
		public class UserPermissionsDataTable : DataTable, IEnumerable
		{
			// Token: 0x06008DC9 RID: 36297 RVA: 0x001BBD44 File Offset: 0x001B9F44
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			public UserPermissionsDataTable()
			{
				base.TableName = "UserPermissions";
				this.BeginInit();
				this.InitClass();
				this.EndInit();
			}

			// Token: 0x06008DCA RID: 36298 RVA: 0x001BBD6C File Offset: 0x001B9F6C
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

			// Token: 0x06008DCB RID: 36299 RVA: 0x001BBE14 File Offset: 0x001BA014
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			protected UserPermissionsDataTable(SerializationInfo info, StreamingContext context) : base(info, context)
			{
				this.InitVars();
			}

			// Token: 0x17002ADE RID: 10974
			// (get) Token: 0x06008DCC RID: 36300 RVA: 0x001BBE24 File Offset: 0x001BA024
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			public DataColumn WSEC_CAT_UIDColumn
			{
				get
				{
					return this.columnWSEC_CAT_UID;
				}
			}

			// Token: 0x17002ADF RID: 10975
			// (get) Token: 0x06008DCD RID: 36301 RVA: 0x001BBE2C File Offset: 0x001BA02C
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			public DataColumn RES_UIDColumn
			{
				get
				{
					return this.columnRES_UID;
				}
			}

			// Token: 0x17002AE0 RID: 10976
			// (get) Token: 0x06008DCE RID: 36302 RVA: 0x001BBE34 File Offset: 0x001BA034
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			public DataColumn WSEC_FEA_ACT_UIDColumn
			{
				get
				{
					return this.columnWSEC_FEA_ACT_UID;
				}
			}

			// Token: 0x17002AE1 RID: 10977
			// (get) Token: 0x06008DCF RID: 36303 RVA: 0x001BBE3C File Offset: 0x001BA03C
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			public DataColumn WSEC_ALLOWColumn
			{
				get
				{
					return this.columnWSEC_ALLOW;
				}
			}

			// Token: 0x17002AE2 RID: 10978
			// (get) Token: 0x06008DD0 RID: 36304 RVA: 0x001BBE44 File Offset: 0x001BA044
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public DataColumn WSEC_DENYColumn
			{
				get
				{
					return this.columnWSEC_DENY;
				}
			}

			// Token: 0x17002AE3 RID: 10979
			// (get) Token: 0x06008DD1 RID: 36305 RVA: 0x001BBE4C File Offset: 0x001BA04C
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

			// Token: 0x17002AE4 RID: 10980
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			public SecurityCategoriesDataSet.UserPermissionsRow this[int index]
			{
				get
				{
					return (SecurityCategoriesDataSet.UserPermissionsRow)base.Rows[index];
				}
			}

			// Token: 0x140004FD RID: 1277
			// (add) Token: 0x06008DD3 RID: 36307 RVA: 0x001BBE6C File Offset: 0x001BA06C
			// (remove) Token: 0x06008DD4 RID: 36308 RVA: 0x001BBEA4 File Offset: 0x001BA0A4
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public event SecurityCategoriesDataSet.UserPermissionsRowChangeEventHandler UserPermissionsRowChanging;

			// Token: 0x140004FE RID: 1278
			// (add) Token: 0x06008DD5 RID: 36309 RVA: 0x001BBEDC File Offset: 0x001BA0DC
			// (remove) Token: 0x06008DD6 RID: 36310 RVA: 0x001BBF14 File Offset: 0x001BA114
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public event SecurityCategoriesDataSet.UserPermissionsRowChangeEventHandler UserPermissionsRowChanged;

			// Token: 0x140004FF RID: 1279
			// (add) Token: 0x06008DD7 RID: 36311 RVA: 0x001BBF4C File Offset: 0x001BA14C
			// (remove) Token: 0x06008DD8 RID: 36312 RVA: 0x001BBF84 File Offset: 0x001BA184
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public event SecurityCategoriesDataSet.UserPermissionsRowChangeEventHandler UserPermissionsRowDeleting;

			// Token: 0x14000500 RID: 1280
			// (add) Token: 0x06008DD9 RID: 36313 RVA: 0x001BBFBC File Offset: 0x001BA1BC
			// (remove) Token: 0x06008DDA RID: 36314 RVA: 0x001BBFF4 File Offset: 0x001BA1F4
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public event SecurityCategoriesDataSet.UserPermissionsRowChangeEventHandler UserPermissionsRowDeleted;

			// Token: 0x06008DDB RID: 36315 RVA: 0x001BC029 File Offset: 0x001BA229
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			public void AddUserPermissionsRow(SecurityCategoriesDataSet.UserPermissionsRow row)
			{
				base.Rows.Add(row);
			}

			// Token: 0x06008DDC RID: 36316 RVA: 0x001BC038 File Offset: 0x001BA238
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public SecurityCategoriesDataSet.UserPermissionsRow AddUserPermissionsRow(Guid WSEC_CAT_UID, Guid RES_UID, Guid WSEC_FEA_ACT_UID, bool WSEC_ALLOW, bool WSEC_DENY)
			{
				SecurityCategoriesDataSet.UserPermissionsRow userPermissionsRow = (SecurityCategoriesDataSet.UserPermissionsRow)base.NewRow();
				object[] itemArray = new object[]
				{
					WSEC_CAT_UID,
					RES_UID,
					WSEC_FEA_ACT_UID,
					WSEC_ALLOW,
					WSEC_DENY
				};
				userPermissionsRow.ItemArray = itemArray;
				base.Rows.Add(userPermissionsRow);
				return userPermissionsRow;
			}

			// Token: 0x06008DDD RID: 36317 RVA: 0x001BC0A0 File Offset: 0x001BA2A0
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			public SecurityCategoriesDataSet.UserPermissionsRow FindByWSEC_CAT_UIDRES_UIDWSEC_FEA_ACT_UID(Guid WSEC_CAT_UID, Guid RES_UID, Guid WSEC_FEA_ACT_UID)
			{
				return (SecurityCategoriesDataSet.UserPermissionsRow)base.Rows.Find(new object[]
				{
					WSEC_CAT_UID,
					RES_UID,
					WSEC_FEA_ACT_UID
				});
			}

			// Token: 0x06008DDE RID: 36318 RVA: 0x001BC0E0 File Offset: 0x001BA2E0
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public virtual IEnumerator GetEnumerator()
			{
				return base.Rows.GetEnumerator();
			}

			// Token: 0x06008DDF RID: 36319 RVA: 0x001BC0F0 File Offset: 0x001BA2F0
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			public override DataTable Clone()
			{
				SecurityCategoriesDataSet.UserPermissionsDataTable userPermissionsDataTable = (SecurityCategoriesDataSet.UserPermissionsDataTable)base.Clone();
				userPermissionsDataTable.InitVars();
				return userPermissionsDataTable;
			}

			// Token: 0x06008DE0 RID: 36320 RVA: 0x001BC110 File Offset: 0x001BA310
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			protected override DataTable CreateInstance()
			{
				return new SecurityCategoriesDataSet.UserPermissionsDataTable();
			}

			// Token: 0x06008DE1 RID: 36321 RVA: 0x001BC118 File Offset: 0x001BA318
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			internal void InitVars()
			{
				this.columnWSEC_CAT_UID = base.Columns["WSEC_CAT_UID"];
				this.columnRES_UID = base.Columns["RES_UID"];
				this.columnWSEC_FEA_ACT_UID = base.Columns["WSEC_FEA_ACT_UID"];
				this.columnWSEC_ALLOW = base.Columns["WSEC_ALLOW"];
				this.columnWSEC_DENY = base.Columns["WSEC_DENY"];
			}

			// Token: 0x06008DE2 RID: 36322 RVA: 0x001BC194 File Offset: 0x001BA394
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
				this.columnWSEC_ALLOW = new DataColumn("WSEC_ALLOW", typeof(bool), null, MappingType.Element);
				base.Columns.Add(this.columnWSEC_ALLOW);
				this.columnWSEC_DENY = new DataColumn("WSEC_DENY", typeof(bool), null, MappingType.Element);
				base.Columns.Add(this.columnWSEC_DENY);
				base.Constraints.Add(new UniqueConstraint("UserPermissionsPrimaryKey", new DataColumn[]
				{
					this.columnWSEC_CAT_UID,
					this.columnRES_UID,
					this.columnWSEC_FEA_ACT_UID
				}, true));
				this.columnWSEC_CAT_UID.AllowDBNull = false;
				this.columnRES_UID.AllowDBNull = false;
				this.columnWSEC_FEA_ACT_UID.AllowDBNull = false;
				this.columnWSEC_ALLOW.AllowDBNull = false;
				this.columnWSEC_ALLOW.DefaultValue = false;
				this.columnWSEC_DENY.AllowDBNull = false;
				this.columnWSEC_DENY.DefaultValue = false;
			}

			// Token: 0x06008DE3 RID: 36323 RVA: 0x001BC319 File Offset: 0x001BA519
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			public SecurityCategoriesDataSet.UserPermissionsRow NewUserPermissionsRow()
			{
				return (SecurityCategoriesDataSet.UserPermissionsRow)base.NewRow();
			}

			// Token: 0x06008DE4 RID: 36324 RVA: 0x001BC326 File Offset: 0x001BA526
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			protected override DataRow NewRowFromBuilder(DataRowBuilder builder)
			{
				return new SecurityCategoriesDataSet.UserPermissionsRow(builder);
			}

			// Token: 0x06008DE5 RID: 36325 RVA: 0x001BC32E File Offset: 0x001BA52E
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			protected override Type GetRowType()
			{
				return typeof(SecurityCategoriesDataSet.UserPermissionsRow);
			}

			// Token: 0x06008DE6 RID: 36326 RVA: 0x001BC33A File Offset: 0x001BA53A
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			protected override void OnRowChanged(DataRowChangeEventArgs e)
			{
				base.OnRowChanged(e);
				if (this.UserPermissionsRowChanged != null)
				{
					this.UserPermissionsRowChanged(this, new SecurityCategoriesDataSet.UserPermissionsRowChangeEvent((SecurityCategoriesDataSet.UserPermissionsRow)e.Row, e.Action));
				}
			}

			// Token: 0x06008DE7 RID: 36327 RVA: 0x001BC36D File Offset: 0x001BA56D
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			protected override void OnRowChanging(DataRowChangeEventArgs e)
			{
				base.OnRowChanging(e);
				if (this.UserPermissionsRowChanging != null)
				{
					this.UserPermissionsRowChanging(this, new SecurityCategoriesDataSet.UserPermissionsRowChangeEvent((SecurityCategoriesDataSet.UserPermissionsRow)e.Row, e.Action));
				}
			}

			// Token: 0x06008DE8 RID: 36328 RVA: 0x001BC3A0 File Offset: 0x001BA5A0
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			protected override void OnRowDeleted(DataRowChangeEventArgs e)
			{
				base.OnRowDeleted(e);
				if (this.UserPermissionsRowDeleted != null)
				{
					this.UserPermissionsRowDeleted(this, new SecurityCategoriesDataSet.UserPermissionsRowChangeEvent((SecurityCategoriesDataSet.UserPermissionsRow)e.Row, e.Action));
				}
			}

			// Token: 0x06008DE9 RID: 36329 RVA: 0x001BC3D3 File Offset: 0x001BA5D3
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			protected override void OnRowDeleting(DataRowChangeEventArgs e)
			{
				base.OnRowDeleting(e);
				if (this.UserPermissionsRowDeleting != null)
				{
					this.UserPermissionsRowDeleting(this, new SecurityCategoriesDataSet.UserPermissionsRowChangeEvent((SecurityCategoriesDataSet.UserPermissionsRow)e.Row, e.Action));
				}
			}

			// Token: 0x06008DEA RID: 36330 RVA: 0x001BC406 File Offset: 0x001BA606
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			public void RemoveUserPermissionsRow(SecurityCategoriesDataSet.UserPermissionsRow row)
			{
				base.Rows.Remove(row);
			}

			// Token: 0x06008DEB RID: 36331 RVA: 0x001BC414 File Offset: 0x001BA614
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public static XmlSchemaComplexType GetTypedTableSchema(XmlSchemaSet xs)
			{
				XmlSchemaComplexType xmlSchemaComplexType = new XmlSchemaComplexType();
				XmlSchemaSequence xmlSchemaSequence = new XmlSchemaSequence();
				SecurityCategoriesDataSet securityCategoriesDataSet = new SecurityCategoriesDataSet();
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
				xmlSchemaAttribute.FixedValue = securityCategoriesDataSet.Namespace;
				xmlSchemaComplexType.Attributes.Add(xmlSchemaAttribute);
				XmlSchemaAttribute xmlSchemaAttribute2 = new XmlSchemaAttribute();
				xmlSchemaAttribute2.Name = "tableTypeName";
				xmlSchemaAttribute2.FixedValue = "UserPermissionsDataTable";
				xmlSchemaComplexType.Attributes.Add(xmlSchemaAttribute2);
				xmlSchemaComplexType.Particle = xmlSchemaSequence;
				XmlSchema schemaSerializable = securityCategoriesDataSet.GetSchemaSerializable();
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

			// Token: 0x04001C62 RID: 7266
			private DataColumn columnWSEC_CAT_UID;

			// Token: 0x04001C63 RID: 7267
			private DataColumn columnRES_UID;

			// Token: 0x04001C64 RID: 7268
			private DataColumn columnWSEC_FEA_ACT_UID;

			// Token: 0x04001C65 RID: 7269
			private DataColumn columnWSEC_ALLOW;

			// Token: 0x04001C66 RID: 7270
			private DataColumn columnWSEC_DENY;
		}

		// Token: 0x020005CA RID: 1482
		[XmlSchemaProvider("GetTypedTableSchema")]
		[Serializable]
		public class GroupPermissionsDataTable : DataTable, IEnumerable
		{
			// Token: 0x06008DEC RID: 36332 RVA: 0x001BC60C File Offset: 0x001BA80C
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			public GroupPermissionsDataTable()
			{
				base.TableName = "GroupPermissions";
				this.BeginInit();
				this.InitClass();
				this.EndInit();
			}

			// Token: 0x06008DED RID: 36333 RVA: 0x001BC634 File Offset: 0x001BA834
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
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

			// Token: 0x06008DEE RID: 36334 RVA: 0x001BC6DC File Offset: 0x001BA8DC
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			protected GroupPermissionsDataTable(SerializationInfo info, StreamingContext context) : base(info, context)
			{
				this.InitVars();
			}

			// Token: 0x17002AE5 RID: 10981
			// (get) Token: 0x06008DEF RID: 36335 RVA: 0x001BC6EC File Offset: 0x001BA8EC
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public DataColumn WSEC_CAT_UIDColumn
			{
				get
				{
					return this.columnWSEC_CAT_UID;
				}
			}

			// Token: 0x17002AE6 RID: 10982
			// (get) Token: 0x06008DF0 RID: 36336 RVA: 0x001BC6F4 File Offset: 0x001BA8F4
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			public DataColumn WSEC_GRP_UIDColumn
			{
				get
				{
					return this.columnWSEC_GRP_UID;
				}
			}

			// Token: 0x17002AE7 RID: 10983
			// (get) Token: 0x06008DF1 RID: 36337 RVA: 0x001BC6FC File Offset: 0x001BA8FC
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			public DataColumn WSEC_FEA_ACT_UIDColumn
			{
				get
				{
					return this.columnWSEC_FEA_ACT_UID;
				}
			}

			// Token: 0x17002AE8 RID: 10984
			// (get) Token: 0x06008DF2 RID: 36338 RVA: 0x001BC704 File Offset: 0x001BA904
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			public DataColumn WSEC_ALLOWColumn
			{
				get
				{
					return this.columnWSEC_ALLOW;
				}
			}

			// Token: 0x17002AE9 RID: 10985
			// (get) Token: 0x06008DF3 RID: 36339 RVA: 0x001BC70C File Offset: 0x001BA90C
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			public DataColumn WSEC_DENYColumn
			{
				get
				{
					return this.columnWSEC_DENY;
				}
			}

			// Token: 0x17002AEA RID: 10986
			// (get) Token: 0x06008DF4 RID: 36340 RVA: 0x001BC714 File Offset: 0x001BA914
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

			// Token: 0x17002AEB RID: 10987
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public SecurityCategoriesDataSet.GroupPermissionsRow this[int index]
			{
				get
				{
					return (SecurityCategoriesDataSet.GroupPermissionsRow)base.Rows[index];
				}
			}

			// Token: 0x14000501 RID: 1281
			// (add) Token: 0x06008DF6 RID: 36342 RVA: 0x001BC734 File Offset: 0x001BA934
			// (remove) Token: 0x06008DF7 RID: 36343 RVA: 0x001BC76C File Offset: 0x001BA96C
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public event SecurityCategoriesDataSet.GroupPermissionsRowChangeEventHandler GroupPermissionsRowChanging;

			// Token: 0x14000502 RID: 1282
			// (add) Token: 0x06008DF8 RID: 36344 RVA: 0x001BC7A4 File Offset: 0x001BA9A4
			// (remove) Token: 0x06008DF9 RID: 36345 RVA: 0x001BC7DC File Offset: 0x001BA9DC
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public event SecurityCategoriesDataSet.GroupPermissionsRowChangeEventHandler GroupPermissionsRowChanged;

			// Token: 0x14000503 RID: 1283
			// (add) Token: 0x06008DFA RID: 36346 RVA: 0x001BC814 File Offset: 0x001BAA14
			// (remove) Token: 0x06008DFB RID: 36347 RVA: 0x001BC84C File Offset: 0x001BAA4C
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public event SecurityCategoriesDataSet.GroupPermissionsRowChangeEventHandler GroupPermissionsRowDeleting;

			// Token: 0x14000504 RID: 1284
			// (add) Token: 0x06008DFC RID: 36348 RVA: 0x001BC884 File Offset: 0x001BAA84
			// (remove) Token: 0x06008DFD RID: 36349 RVA: 0x001BC8BC File Offset: 0x001BAABC
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public event SecurityCategoriesDataSet.GroupPermissionsRowChangeEventHandler GroupPermissionsRowDeleted;

			// Token: 0x06008DFE RID: 36350 RVA: 0x001BC8F1 File Offset: 0x001BAAF1
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			public void AddGroupPermissionsRow(SecurityCategoriesDataSet.GroupPermissionsRow row)
			{
				base.Rows.Add(row);
			}

			// Token: 0x06008DFF RID: 36351 RVA: 0x001BC900 File Offset: 0x001BAB00
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			public SecurityCategoriesDataSet.GroupPermissionsRow AddGroupPermissionsRow(Guid WSEC_CAT_UID, Guid WSEC_GRP_UID, Guid WSEC_FEA_ACT_UID, bool WSEC_ALLOW, bool WSEC_DENY)
			{
				SecurityCategoriesDataSet.GroupPermissionsRow groupPermissionsRow = (SecurityCategoriesDataSet.GroupPermissionsRow)base.NewRow();
				object[] itemArray = new object[]
				{
					WSEC_CAT_UID,
					WSEC_GRP_UID,
					WSEC_FEA_ACT_UID,
					WSEC_ALLOW,
					WSEC_DENY
				};
				groupPermissionsRow.ItemArray = itemArray;
				base.Rows.Add(groupPermissionsRow);
				return groupPermissionsRow;
			}

			// Token: 0x06008E00 RID: 36352 RVA: 0x001BC968 File Offset: 0x001BAB68
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			public SecurityCategoriesDataSet.GroupPermissionsRow FindByWSEC_CAT_UIDWSEC_GRP_UIDWSEC_FEA_ACT_UID(Guid WSEC_CAT_UID, Guid WSEC_GRP_UID, Guid WSEC_FEA_ACT_UID)
			{
				return (SecurityCategoriesDataSet.GroupPermissionsRow)base.Rows.Find(new object[]
				{
					WSEC_CAT_UID,
					WSEC_GRP_UID,
					WSEC_FEA_ACT_UID
				});
			}

			// Token: 0x06008E01 RID: 36353 RVA: 0x001BC9A8 File Offset: 0x001BABA8
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public virtual IEnumerator GetEnumerator()
			{
				return base.Rows.GetEnumerator();
			}

			// Token: 0x06008E02 RID: 36354 RVA: 0x001BC9B8 File Offset: 0x001BABB8
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public override DataTable Clone()
			{
				SecurityCategoriesDataSet.GroupPermissionsDataTable groupPermissionsDataTable = (SecurityCategoriesDataSet.GroupPermissionsDataTable)base.Clone();
				groupPermissionsDataTable.InitVars();
				return groupPermissionsDataTable;
			}

			// Token: 0x06008E03 RID: 36355 RVA: 0x001BC9D8 File Offset: 0x001BABD8
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			protected override DataTable CreateInstance()
			{
				return new SecurityCategoriesDataSet.GroupPermissionsDataTable();
			}

			// Token: 0x06008E04 RID: 36356 RVA: 0x001BC9E0 File Offset: 0x001BABE0
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			internal void InitVars()
			{
				this.columnWSEC_CAT_UID = base.Columns["WSEC_CAT_UID"];
				this.columnWSEC_GRP_UID = base.Columns["WSEC_GRP_UID"];
				this.columnWSEC_FEA_ACT_UID = base.Columns["WSEC_FEA_ACT_UID"];
				this.columnWSEC_ALLOW = base.Columns["WSEC_ALLOW"];
				this.columnWSEC_DENY = base.Columns["WSEC_DENY"];
			}

			// Token: 0x06008E05 RID: 36357 RVA: 0x001BCA5C File Offset: 0x001BAC5C
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
				this.columnWSEC_ALLOW = new DataColumn("WSEC_ALLOW", typeof(bool), null, MappingType.Element);
				base.Columns.Add(this.columnWSEC_ALLOW);
				this.columnWSEC_DENY = new DataColumn("WSEC_DENY", typeof(bool), null, MappingType.Element);
				base.Columns.Add(this.columnWSEC_DENY);
				base.Constraints.Add(new UniqueConstraint("GroupPermissionsPrimaryKey", new DataColumn[]
				{
					this.columnWSEC_CAT_UID,
					this.columnWSEC_GRP_UID,
					this.columnWSEC_FEA_ACT_UID
				}, true));
				this.columnWSEC_CAT_UID.AllowDBNull = false;
				this.columnWSEC_GRP_UID.AllowDBNull = false;
				this.columnWSEC_FEA_ACT_UID.AllowDBNull = false;
				this.columnWSEC_ALLOW.AllowDBNull = false;
				this.columnWSEC_ALLOW.DefaultValue = false;
				this.columnWSEC_DENY.AllowDBNull = false;
				this.columnWSEC_DENY.DefaultValue = false;
			}

			// Token: 0x06008E06 RID: 36358 RVA: 0x001BCBE1 File Offset: 0x001BADE1
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			public SecurityCategoriesDataSet.GroupPermissionsRow NewGroupPermissionsRow()
			{
				return (SecurityCategoriesDataSet.GroupPermissionsRow)base.NewRow();
			}

			// Token: 0x06008E07 RID: 36359 RVA: 0x001BCBEE File Offset: 0x001BADEE
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			protected override DataRow NewRowFromBuilder(DataRowBuilder builder)
			{
				return new SecurityCategoriesDataSet.GroupPermissionsRow(builder);
			}

			// Token: 0x06008E08 RID: 36360 RVA: 0x001BCBF6 File Offset: 0x001BADF6
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			protected override Type GetRowType()
			{
				return typeof(SecurityCategoriesDataSet.GroupPermissionsRow);
			}

			// Token: 0x06008E09 RID: 36361 RVA: 0x001BCC02 File Offset: 0x001BAE02
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			protected override void OnRowChanged(DataRowChangeEventArgs e)
			{
				base.OnRowChanged(e);
				if (this.GroupPermissionsRowChanged != null)
				{
					this.GroupPermissionsRowChanged(this, new SecurityCategoriesDataSet.GroupPermissionsRowChangeEvent((SecurityCategoriesDataSet.GroupPermissionsRow)e.Row, e.Action));
				}
			}

			// Token: 0x06008E0A RID: 36362 RVA: 0x001BCC35 File Offset: 0x001BAE35
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			protected override void OnRowChanging(DataRowChangeEventArgs e)
			{
				base.OnRowChanging(e);
				if (this.GroupPermissionsRowChanging != null)
				{
					this.GroupPermissionsRowChanging(this, new SecurityCategoriesDataSet.GroupPermissionsRowChangeEvent((SecurityCategoriesDataSet.GroupPermissionsRow)e.Row, e.Action));
				}
			}

			// Token: 0x06008E0B RID: 36363 RVA: 0x001BCC68 File Offset: 0x001BAE68
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			protected override void OnRowDeleted(DataRowChangeEventArgs e)
			{
				base.OnRowDeleted(e);
				if (this.GroupPermissionsRowDeleted != null)
				{
					this.GroupPermissionsRowDeleted(this, new SecurityCategoriesDataSet.GroupPermissionsRowChangeEvent((SecurityCategoriesDataSet.GroupPermissionsRow)e.Row, e.Action));
				}
			}

			// Token: 0x06008E0C RID: 36364 RVA: 0x001BCC9B File Offset: 0x001BAE9B
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			protected override void OnRowDeleting(DataRowChangeEventArgs e)
			{
				base.OnRowDeleting(e);
				if (this.GroupPermissionsRowDeleting != null)
				{
					this.GroupPermissionsRowDeleting(this, new SecurityCategoriesDataSet.GroupPermissionsRowChangeEvent((SecurityCategoriesDataSet.GroupPermissionsRow)e.Row, e.Action));
				}
			}

			// Token: 0x06008E0D RID: 36365 RVA: 0x001BCCCE File Offset: 0x001BAECE
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public void RemoveGroupPermissionsRow(SecurityCategoriesDataSet.GroupPermissionsRow row)
			{
				base.Rows.Remove(row);
			}

			// Token: 0x06008E0E RID: 36366 RVA: 0x001BCCDC File Offset: 0x001BAEDC
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			public static XmlSchemaComplexType GetTypedTableSchema(XmlSchemaSet xs)
			{
				XmlSchemaComplexType xmlSchemaComplexType = new XmlSchemaComplexType();
				XmlSchemaSequence xmlSchemaSequence = new XmlSchemaSequence();
				SecurityCategoriesDataSet securityCategoriesDataSet = new SecurityCategoriesDataSet();
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
				xmlSchemaAttribute.FixedValue = securityCategoriesDataSet.Namespace;
				xmlSchemaComplexType.Attributes.Add(xmlSchemaAttribute);
				XmlSchemaAttribute xmlSchemaAttribute2 = new XmlSchemaAttribute();
				xmlSchemaAttribute2.Name = "tableTypeName";
				xmlSchemaAttribute2.FixedValue = "GroupPermissionsDataTable";
				xmlSchemaComplexType.Attributes.Add(xmlSchemaAttribute2);
				xmlSchemaComplexType.Particle = xmlSchemaSequence;
				XmlSchema schemaSerializable = securityCategoriesDataSet.GetSchemaSerializable();
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

			// Token: 0x04001C6B RID: 7275
			private DataColumn columnWSEC_CAT_UID;

			// Token: 0x04001C6C RID: 7276
			private DataColumn columnWSEC_GRP_UID;

			// Token: 0x04001C6D RID: 7277
			private DataColumn columnWSEC_FEA_ACT_UID;

			// Token: 0x04001C6E RID: 7278
			private DataColumn columnWSEC_ALLOW;

			// Token: 0x04001C6F RID: 7279
			private DataColumn columnWSEC_DENY;
		}

		// Token: 0x020005CB RID: 1483
		[XmlSchemaProvider("GetTypedTableSchema")]
		[Serializable]
		public class SecurityCategoryObjectsDataTable : DataTable, IEnumerable
		{
			// Token: 0x06008E0F RID: 36367 RVA: 0x001BCED4 File Offset: 0x001BB0D4
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			public SecurityCategoryObjectsDataTable()
			{
				base.TableName = "SecurityCategoryObjects";
				this.BeginInit();
				this.InitClass();
				this.EndInit();
			}

			// Token: 0x06008E10 RID: 36368 RVA: 0x001BCEFC File Offset: 0x001BB0FC
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			internal SecurityCategoryObjectsDataTable(DataTable table)
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

			// Token: 0x06008E11 RID: 36369 RVA: 0x001BCFA4 File Offset: 0x001BB1A4
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			protected SecurityCategoryObjectsDataTable(SerializationInfo info, StreamingContext context) : base(info, context)
			{
				this.InitVars();
			}

			// Token: 0x17002AEC RID: 10988
			// (get) Token: 0x06008E12 RID: 36370 RVA: 0x001BCFB4 File Offset: 0x001BB1B4
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			public DataColumn WSEC_CAT_UIDColumn
			{
				get
				{
					return this.columnWSEC_CAT_UID;
				}
			}

			// Token: 0x17002AED RID: 10989
			// (get) Token: 0x06008E13 RID: 36371 RVA: 0x001BCFBC File Offset: 0x001BB1BC
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public DataColumn WSEC_OBJ_TYPE_UIDColumn
			{
				get
				{
					return this.columnWSEC_OBJ_TYPE_UID;
				}
			}

			// Token: 0x17002AEE RID: 10990
			// (get) Token: 0x06008E14 RID: 36372 RVA: 0x001BCFC4 File Offset: 0x001BB1C4
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public DataColumn WSEC_OBJ_UIDColumn
			{
				get
				{
					return this.columnWSEC_OBJ_UID;
				}
			}

			// Token: 0x17002AEF RID: 10991
			// (get) Token: 0x06008E15 RID: 36373 RVA: 0x001BCFCC File Offset: 0x001BB1CC
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

			// Token: 0x17002AF0 RID: 10992
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			public SecurityCategoriesDataSet.SecurityCategoryObjectsRow this[int index]
			{
				get
				{
					return (SecurityCategoriesDataSet.SecurityCategoryObjectsRow)base.Rows[index];
				}
			}

			// Token: 0x14000505 RID: 1285
			// (add) Token: 0x06008E17 RID: 36375 RVA: 0x001BCFEC File Offset: 0x001BB1EC
			// (remove) Token: 0x06008E18 RID: 36376 RVA: 0x001BD024 File Offset: 0x001BB224
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public event SecurityCategoriesDataSet.SecurityCategoryObjectsRowChangeEventHandler SecurityCategoryObjectsRowChanging;

			// Token: 0x14000506 RID: 1286
			// (add) Token: 0x06008E19 RID: 36377 RVA: 0x001BD05C File Offset: 0x001BB25C
			// (remove) Token: 0x06008E1A RID: 36378 RVA: 0x001BD094 File Offset: 0x001BB294
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public event SecurityCategoriesDataSet.SecurityCategoryObjectsRowChangeEventHandler SecurityCategoryObjectsRowChanged;

			// Token: 0x14000507 RID: 1287
			// (add) Token: 0x06008E1B RID: 36379 RVA: 0x001BD0CC File Offset: 0x001BB2CC
			// (remove) Token: 0x06008E1C RID: 36380 RVA: 0x001BD104 File Offset: 0x001BB304
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public event SecurityCategoriesDataSet.SecurityCategoryObjectsRowChangeEventHandler SecurityCategoryObjectsRowDeleting;

			// Token: 0x14000508 RID: 1288
			// (add) Token: 0x06008E1D RID: 36381 RVA: 0x001BD13C File Offset: 0x001BB33C
			// (remove) Token: 0x06008E1E RID: 36382 RVA: 0x001BD174 File Offset: 0x001BB374
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public event SecurityCategoriesDataSet.SecurityCategoryObjectsRowChangeEventHandler SecurityCategoryObjectsRowDeleted;

			// Token: 0x06008E1F RID: 36383 RVA: 0x001BD1A9 File Offset: 0x001BB3A9
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public void AddSecurityCategoryObjectsRow(SecurityCategoriesDataSet.SecurityCategoryObjectsRow row)
			{
				base.Rows.Add(row);
			}

			// Token: 0x06008E20 RID: 36384 RVA: 0x001BD1B8 File Offset: 0x001BB3B8
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public SecurityCategoriesDataSet.SecurityCategoryObjectsRow AddSecurityCategoryObjectsRow(Guid WSEC_CAT_UID, Guid WSEC_OBJ_TYPE_UID, Guid WSEC_OBJ_UID)
			{
				SecurityCategoriesDataSet.SecurityCategoryObjectsRow securityCategoryObjectsRow = (SecurityCategoriesDataSet.SecurityCategoryObjectsRow)base.NewRow();
				object[] itemArray = new object[]
				{
					WSEC_CAT_UID,
					WSEC_OBJ_TYPE_UID,
					WSEC_OBJ_UID
				};
				securityCategoryObjectsRow.ItemArray = itemArray;
				base.Rows.Add(securityCategoryObjectsRow);
				return securityCategoryObjectsRow;
			}

			// Token: 0x06008E21 RID: 36385 RVA: 0x001BD20C File Offset: 0x001BB40C
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			public SecurityCategoriesDataSet.SecurityCategoryObjectsRow FindByWSEC_CAT_UIDWSEC_OBJ_UIDWSEC_OBJ_TYPE_UID(Guid WSEC_CAT_UID, Guid WSEC_OBJ_UID, Guid WSEC_OBJ_TYPE_UID)
			{
				return (SecurityCategoriesDataSet.SecurityCategoryObjectsRow)base.Rows.Find(new object[]
				{
					WSEC_CAT_UID,
					WSEC_OBJ_UID,
					WSEC_OBJ_TYPE_UID
				});
			}

			// Token: 0x06008E22 RID: 36386 RVA: 0x001BD24C File Offset: 0x001BB44C
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			public virtual IEnumerator GetEnumerator()
			{
				return base.Rows.GetEnumerator();
			}

			// Token: 0x06008E23 RID: 36387 RVA: 0x001BD25C File Offset: 0x001BB45C
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public override DataTable Clone()
			{
				SecurityCategoriesDataSet.SecurityCategoryObjectsDataTable securityCategoryObjectsDataTable = (SecurityCategoriesDataSet.SecurityCategoryObjectsDataTable)base.Clone();
				securityCategoryObjectsDataTable.InitVars();
				return securityCategoryObjectsDataTable;
			}

			// Token: 0x06008E24 RID: 36388 RVA: 0x001BD27C File Offset: 0x001BB47C
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			protected override DataTable CreateInstance()
			{
				return new SecurityCategoriesDataSet.SecurityCategoryObjectsDataTable();
			}

			// Token: 0x06008E25 RID: 36389 RVA: 0x001BD284 File Offset: 0x001BB484
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			internal void InitVars()
			{
				this.columnWSEC_CAT_UID = base.Columns["WSEC_CAT_UID"];
				this.columnWSEC_OBJ_TYPE_UID = base.Columns["WSEC_OBJ_TYPE_UID"];
				this.columnWSEC_OBJ_UID = base.Columns["WSEC_OBJ_UID"];
			}

			// Token: 0x06008E26 RID: 36390 RVA: 0x001BD2D4 File Offset: 0x001BB4D4
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			private void InitClass()
			{
				this.columnWSEC_CAT_UID = new DataColumn("WSEC_CAT_UID", typeof(Guid), null, MappingType.Element);
				base.Columns.Add(this.columnWSEC_CAT_UID);
				this.columnWSEC_OBJ_TYPE_UID = new DataColumn("WSEC_OBJ_TYPE_UID", typeof(Guid), null, MappingType.Element);
				base.Columns.Add(this.columnWSEC_OBJ_TYPE_UID);
				this.columnWSEC_OBJ_UID = new DataColumn("WSEC_OBJ_UID", typeof(Guid), null, MappingType.Element);
				base.Columns.Add(this.columnWSEC_OBJ_UID);
				base.Constraints.Add(new UniqueConstraint("SecurityCategoryObjectsPrimaryKey", new DataColumn[]
				{
					this.columnWSEC_CAT_UID,
					this.columnWSEC_OBJ_UID,
					this.columnWSEC_OBJ_TYPE_UID
				}, true));
				this.columnWSEC_CAT_UID.AllowDBNull = false;
				this.columnWSEC_OBJ_TYPE_UID.AllowDBNull = false;
				this.columnWSEC_OBJ_UID.AllowDBNull = false;
			}

			// Token: 0x06008E27 RID: 36391 RVA: 0x001BD3C5 File Offset: 0x001BB5C5
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			public SecurityCategoriesDataSet.SecurityCategoryObjectsRow NewSecurityCategoryObjectsRow()
			{
				return (SecurityCategoriesDataSet.SecurityCategoryObjectsRow)base.NewRow();
			}

			// Token: 0x06008E28 RID: 36392 RVA: 0x001BD3D2 File Offset: 0x001BB5D2
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			protected override DataRow NewRowFromBuilder(DataRowBuilder builder)
			{
				return new SecurityCategoriesDataSet.SecurityCategoryObjectsRow(builder);
			}

			// Token: 0x06008E29 RID: 36393 RVA: 0x001BD3DA File Offset: 0x001BB5DA
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			protected override Type GetRowType()
			{
				return typeof(SecurityCategoriesDataSet.SecurityCategoryObjectsRow);
			}

			// Token: 0x06008E2A RID: 36394 RVA: 0x001BD3E6 File Offset: 0x001BB5E6
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			protected override void OnRowChanged(DataRowChangeEventArgs e)
			{
				base.OnRowChanged(e);
				if (this.SecurityCategoryObjectsRowChanged != null)
				{
					this.SecurityCategoryObjectsRowChanged(this, new SecurityCategoriesDataSet.SecurityCategoryObjectsRowChangeEvent((SecurityCategoriesDataSet.SecurityCategoryObjectsRow)e.Row, e.Action));
				}
			}

			// Token: 0x06008E2B RID: 36395 RVA: 0x001BD419 File Offset: 0x001BB619
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			protected override void OnRowChanging(DataRowChangeEventArgs e)
			{
				base.OnRowChanging(e);
				if (this.SecurityCategoryObjectsRowChanging != null)
				{
					this.SecurityCategoryObjectsRowChanging(this, new SecurityCategoriesDataSet.SecurityCategoryObjectsRowChangeEvent((SecurityCategoriesDataSet.SecurityCategoryObjectsRow)e.Row, e.Action));
				}
			}

			// Token: 0x06008E2C RID: 36396 RVA: 0x001BD44C File Offset: 0x001BB64C
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			protected override void OnRowDeleted(DataRowChangeEventArgs e)
			{
				base.OnRowDeleted(e);
				if (this.SecurityCategoryObjectsRowDeleted != null)
				{
					this.SecurityCategoryObjectsRowDeleted(this, new SecurityCategoriesDataSet.SecurityCategoryObjectsRowChangeEvent((SecurityCategoriesDataSet.SecurityCategoryObjectsRow)e.Row, e.Action));
				}
			}

			// Token: 0x06008E2D RID: 36397 RVA: 0x001BD47F File Offset: 0x001BB67F
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			protected override void OnRowDeleting(DataRowChangeEventArgs e)
			{
				base.OnRowDeleting(e);
				if (this.SecurityCategoryObjectsRowDeleting != null)
				{
					this.SecurityCategoryObjectsRowDeleting(this, new SecurityCategoriesDataSet.SecurityCategoryObjectsRowChangeEvent((SecurityCategoriesDataSet.SecurityCategoryObjectsRow)e.Row, e.Action));
				}
			}

			// Token: 0x06008E2E RID: 36398 RVA: 0x001BD4B2 File Offset: 0x001BB6B2
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			public void RemoveSecurityCategoryObjectsRow(SecurityCategoriesDataSet.SecurityCategoryObjectsRow row)
			{
				base.Rows.Remove(row);
			}

			// Token: 0x06008E2F RID: 36399 RVA: 0x001BD4C0 File Offset: 0x001BB6C0
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public static XmlSchemaComplexType GetTypedTableSchema(XmlSchemaSet xs)
			{
				XmlSchemaComplexType xmlSchemaComplexType = new XmlSchemaComplexType();
				XmlSchemaSequence xmlSchemaSequence = new XmlSchemaSequence();
				SecurityCategoriesDataSet securityCategoriesDataSet = new SecurityCategoriesDataSet();
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
				xmlSchemaAttribute.FixedValue = securityCategoriesDataSet.Namespace;
				xmlSchemaComplexType.Attributes.Add(xmlSchemaAttribute);
				XmlSchemaAttribute xmlSchemaAttribute2 = new XmlSchemaAttribute();
				xmlSchemaAttribute2.Name = "tableTypeName";
				xmlSchemaAttribute2.FixedValue = "SecurityCategoryObjectsDataTable";
				xmlSchemaComplexType.Attributes.Add(xmlSchemaAttribute2);
				xmlSchemaComplexType.Particle = xmlSchemaSequence;
				XmlSchema schemaSerializable = securityCategoriesDataSet.GetSchemaSerializable();
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

			// Token: 0x04001C74 RID: 7284
			private DataColumn columnWSEC_CAT_UID;

			// Token: 0x04001C75 RID: 7285
			private DataColumn columnWSEC_OBJ_TYPE_UID;

			// Token: 0x04001C76 RID: 7286
			private DataColumn columnWSEC_OBJ_UID;
		}

		// Token: 0x020005CC RID: 1484
		[XmlSchemaProvider("GetTypedTableSchema")]
		[Serializable]
		public class SecurityCategoryRulesDataTable : DataTable, IEnumerable
		{
			// Token: 0x06008E30 RID: 36400 RVA: 0x001BD6B8 File Offset: 0x001BB8B8
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			public SecurityCategoryRulesDataTable()
			{
				base.TableName = "SecurityCategoryRules";
				this.BeginInit();
				this.InitClass();
				this.EndInit();
			}

			// Token: 0x06008E31 RID: 36401 RVA: 0x001BD6E0 File Offset: 0x001BB8E0
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			internal SecurityCategoryRulesDataTable(DataTable table)
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

			// Token: 0x06008E32 RID: 36402 RVA: 0x001BD788 File Offset: 0x001BB988
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			protected SecurityCategoryRulesDataTable(SerializationInfo info, StreamingContext context) : base(info, context)
			{
				this.InitVars();
			}

			// Token: 0x17002AF1 RID: 10993
			// (get) Token: 0x06008E33 RID: 36403 RVA: 0x001BD798 File Offset: 0x001BB998
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public DataColumn WSEC_CAT_UIDColumn
			{
				get
				{
					return this.columnWSEC_CAT_UID;
				}
			}

			// Token: 0x17002AF2 RID: 10994
			// (get) Token: 0x06008E34 RID: 36404 RVA: 0x001BD7A0 File Offset: 0x001BB9A0
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public DataColumn WSEC_OBJ_TYPE_UIDColumn
			{
				get
				{
					return this.columnWSEC_OBJ_TYPE_UID;
				}
			}

			// Token: 0x17002AF3 RID: 10995
			// (get) Token: 0x06008E35 RID: 36405 RVA: 0x001BD7A8 File Offset: 0x001BB9A8
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public DataColumn WSEC_OBJ_RULE_TYPEColumn
			{
				get
				{
					return this.columnWSEC_OBJ_RULE_TYPE;
				}
			}

			// Token: 0x17002AF4 RID: 10996
			// (get) Token: 0x06008E36 RID: 36406 RVA: 0x001BD7B0 File Offset: 0x001BB9B0
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

			// Token: 0x17002AF5 RID: 10997
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public SecurityCategoriesDataSet.SecurityCategoryRulesRow this[int index]
			{
				get
				{
					return (SecurityCategoriesDataSet.SecurityCategoryRulesRow)base.Rows[index];
				}
			}

			// Token: 0x14000509 RID: 1289
			// (add) Token: 0x06008E38 RID: 36408 RVA: 0x001BD7D0 File Offset: 0x001BB9D0
			// (remove) Token: 0x06008E39 RID: 36409 RVA: 0x001BD808 File Offset: 0x001BBA08
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public event SecurityCategoriesDataSet.SecurityCategoryRulesRowChangeEventHandler SecurityCategoryRulesRowChanging;

			// Token: 0x1400050A RID: 1290
			// (add) Token: 0x06008E3A RID: 36410 RVA: 0x001BD840 File Offset: 0x001BBA40
			// (remove) Token: 0x06008E3B RID: 36411 RVA: 0x001BD878 File Offset: 0x001BBA78
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public event SecurityCategoriesDataSet.SecurityCategoryRulesRowChangeEventHandler SecurityCategoryRulesRowChanged;

			// Token: 0x1400050B RID: 1291
			// (add) Token: 0x06008E3C RID: 36412 RVA: 0x001BD8B0 File Offset: 0x001BBAB0
			// (remove) Token: 0x06008E3D RID: 36413 RVA: 0x001BD8E8 File Offset: 0x001BBAE8
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public event SecurityCategoriesDataSet.SecurityCategoryRulesRowChangeEventHandler SecurityCategoryRulesRowDeleting;

			// Token: 0x1400050C RID: 1292
			// (add) Token: 0x06008E3E RID: 36414 RVA: 0x001BD920 File Offset: 0x001BBB20
			// (remove) Token: 0x06008E3F RID: 36415 RVA: 0x001BD958 File Offset: 0x001BBB58
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public event SecurityCategoriesDataSet.SecurityCategoryRulesRowChangeEventHandler SecurityCategoryRulesRowDeleted;

			// Token: 0x06008E40 RID: 36416 RVA: 0x001BD98D File Offset: 0x001BBB8D
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public void AddSecurityCategoryRulesRow(SecurityCategoriesDataSet.SecurityCategoryRulesRow row)
			{
				base.Rows.Add(row);
			}

			// Token: 0x06008E41 RID: 36417 RVA: 0x001BD99C File Offset: 0x001BBB9C
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public SecurityCategoriesDataSet.SecurityCategoryRulesRow AddSecurityCategoryRulesRow(Guid WSEC_CAT_UID, Guid WSEC_OBJ_TYPE_UID, int WSEC_OBJ_RULE_TYPE)
			{
				SecurityCategoriesDataSet.SecurityCategoryRulesRow securityCategoryRulesRow = (SecurityCategoriesDataSet.SecurityCategoryRulesRow)base.NewRow();
				object[] itemArray = new object[]
				{
					WSEC_CAT_UID,
					WSEC_OBJ_TYPE_UID,
					WSEC_OBJ_RULE_TYPE
				};
				securityCategoryRulesRow.ItemArray = itemArray;
				base.Rows.Add(securityCategoryRulesRow);
				return securityCategoryRulesRow;
			}

			// Token: 0x06008E42 RID: 36418 RVA: 0x001BD9F0 File Offset: 0x001BBBF0
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			public SecurityCategoriesDataSet.SecurityCategoryRulesRow FindByWSEC_CAT_UIDWSEC_OBJ_TYPE_UIDWSEC_OBJ_RULE_TYPE(Guid WSEC_CAT_UID, Guid WSEC_OBJ_TYPE_UID, int WSEC_OBJ_RULE_TYPE)
			{
				return (SecurityCategoriesDataSet.SecurityCategoryRulesRow)base.Rows.Find(new object[]
				{
					WSEC_CAT_UID,
					WSEC_OBJ_TYPE_UID,
					WSEC_OBJ_RULE_TYPE
				});
			}

			// Token: 0x06008E43 RID: 36419 RVA: 0x001BDA30 File Offset: 0x001BBC30
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public virtual IEnumerator GetEnumerator()
			{
				return base.Rows.GetEnumerator();
			}

			// Token: 0x06008E44 RID: 36420 RVA: 0x001BDA40 File Offset: 0x001BBC40
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			public override DataTable Clone()
			{
				SecurityCategoriesDataSet.SecurityCategoryRulesDataTable securityCategoryRulesDataTable = (SecurityCategoriesDataSet.SecurityCategoryRulesDataTable)base.Clone();
				securityCategoryRulesDataTable.InitVars();
				return securityCategoryRulesDataTable;
			}

			// Token: 0x06008E45 RID: 36421 RVA: 0x001BDA60 File Offset: 0x001BBC60
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			protected override DataTable CreateInstance()
			{
				return new SecurityCategoriesDataSet.SecurityCategoryRulesDataTable();
			}

			// Token: 0x06008E46 RID: 36422 RVA: 0x001BDA68 File Offset: 0x001BBC68
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			internal void InitVars()
			{
				this.columnWSEC_CAT_UID = base.Columns["WSEC_CAT_UID"];
				this.columnWSEC_OBJ_TYPE_UID = base.Columns["WSEC_OBJ_TYPE_UID"];
				this.columnWSEC_OBJ_RULE_TYPE = base.Columns["WSEC_OBJ_RULE_TYPE"];
			}

			// Token: 0x06008E47 RID: 36423 RVA: 0x001BDAB8 File Offset: 0x001BBCB8
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			private void InitClass()
			{
				this.columnWSEC_CAT_UID = new DataColumn("WSEC_CAT_UID", typeof(Guid), null, MappingType.Element);
				base.Columns.Add(this.columnWSEC_CAT_UID);
				this.columnWSEC_OBJ_TYPE_UID = new DataColumn("WSEC_OBJ_TYPE_UID", typeof(Guid), null, MappingType.Element);
				base.Columns.Add(this.columnWSEC_OBJ_TYPE_UID);
				this.columnWSEC_OBJ_RULE_TYPE = new DataColumn("WSEC_OBJ_RULE_TYPE", typeof(int), null, MappingType.Element);
				base.Columns.Add(this.columnWSEC_OBJ_RULE_TYPE);
				base.Constraints.Add(new UniqueConstraint("SecurityCategoryRulesPrimaryKey", new DataColumn[]
				{
					this.columnWSEC_CAT_UID,
					this.columnWSEC_OBJ_TYPE_UID,
					this.columnWSEC_OBJ_RULE_TYPE
				}, true));
				this.columnWSEC_CAT_UID.AllowDBNull = false;
				this.columnWSEC_OBJ_TYPE_UID.AllowDBNull = false;
				this.columnWSEC_OBJ_RULE_TYPE.AllowDBNull = false;
			}

			// Token: 0x06008E48 RID: 36424 RVA: 0x001BDBA9 File Offset: 0x001BBDA9
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public SecurityCategoriesDataSet.SecurityCategoryRulesRow NewSecurityCategoryRulesRow()
			{
				return (SecurityCategoriesDataSet.SecurityCategoryRulesRow)base.NewRow();
			}

			// Token: 0x06008E49 RID: 36425 RVA: 0x001BDBB6 File Offset: 0x001BBDB6
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			protected override DataRow NewRowFromBuilder(DataRowBuilder builder)
			{
				return new SecurityCategoriesDataSet.SecurityCategoryRulesRow(builder);
			}

			// Token: 0x06008E4A RID: 36426 RVA: 0x001BDBBE File Offset: 0x001BBDBE
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			protected override Type GetRowType()
			{
				return typeof(SecurityCategoriesDataSet.SecurityCategoryRulesRow);
			}

			// Token: 0x06008E4B RID: 36427 RVA: 0x001BDBCA File Offset: 0x001BBDCA
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			protected override void OnRowChanged(DataRowChangeEventArgs e)
			{
				base.OnRowChanged(e);
				if (this.SecurityCategoryRulesRowChanged != null)
				{
					this.SecurityCategoryRulesRowChanged(this, new SecurityCategoriesDataSet.SecurityCategoryRulesRowChangeEvent((SecurityCategoriesDataSet.SecurityCategoryRulesRow)e.Row, e.Action));
				}
			}

			// Token: 0x06008E4C RID: 36428 RVA: 0x001BDBFD File Offset: 0x001BBDFD
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			protected override void OnRowChanging(DataRowChangeEventArgs e)
			{
				base.OnRowChanging(e);
				if (this.SecurityCategoryRulesRowChanging != null)
				{
					this.SecurityCategoryRulesRowChanging(this, new SecurityCategoriesDataSet.SecurityCategoryRulesRowChangeEvent((SecurityCategoriesDataSet.SecurityCategoryRulesRow)e.Row, e.Action));
				}
			}

			// Token: 0x06008E4D RID: 36429 RVA: 0x001BDC30 File Offset: 0x001BBE30
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			protected override void OnRowDeleted(DataRowChangeEventArgs e)
			{
				base.OnRowDeleted(e);
				if (this.SecurityCategoryRulesRowDeleted != null)
				{
					this.SecurityCategoryRulesRowDeleted(this, new SecurityCategoriesDataSet.SecurityCategoryRulesRowChangeEvent((SecurityCategoriesDataSet.SecurityCategoryRulesRow)e.Row, e.Action));
				}
			}

			// Token: 0x06008E4E RID: 36430 RVA: 0x001BDC63 File Offset: 0x001BBE63
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			protected override void OnRowDeleting(DataRowChangeEventArgs e)
			{
				base.OnRowDeleting(e);
				if (this.SecurityCategoryRulesRowDeleting != null)
				{
					this.SecurityCategoryRulesRowDeleting(this, new SecurityCategoriesDataSet.SecurityCategoryRulesRowChangeEvent((SecurityCategoriesDataSet.SecurityCategoryRulesRow)e.Row, e.Action));
				}
			}

			// Token: 0x06008E4F RID: 36431 RVA: 0x001BDC96 File Offset: 0x001BBE96
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			public void RemoveSecurityCategoryRulesRow(SecurityCategoriesDataSet.SecurityCategoryRulesRow row)
			{
				base.Rows.Remove(row);
			}

			// Token: 0x06008E50 RID: 36432 RVA: 0x001BDCA4 File Offset: 0x001BBEA4
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public static XmlSchemaComplexType GetTypedTableSchema(XmlSchemaSet xs)
			{
				XmlSchemaComplexType xmlSchemaComplexType = new XmlSchemaComplexType();
				XmlSchemaSequence xmlSchemaSequence = new XmlSchemaSequence();
				SecurityCategoriesDataSet securityCategoriesDataSet = new SecurityCategoriesDataSet();
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
				xmlSchemaAttribute.FixedValue = securityCategoriesDataSet.Namespace;
				xmlSchemaComplexType.Attributes.Add(xmlSchemaAttribute);
				XmlSchemaAttribute xmlSchemaAttribute2 = new XmlSchemaAttribute();
				xmlSchemaAttribute2.Name = "tableTypeName";
				xmlSchemaAttribute2.FixedValue = "SecurityCategoryRulesDataTable";
				xmlSchemaComplexType.Attributes.Add(xmlSchemaAttribute2);
				xmlSchemaComplexType.Particle = xmlSchemaSequence;
				XmlSchema schemaSerializable = securityCategoriesDataSet.GetSchemaSerializable();
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

			// Token: 0x04001C7B RID: 7291
			private DataColumn columnWSEC_CAT_UID;

			// Token: 0x04001C7C RID: 7292
			private DataColumn columnWSEC_OBJ_TYPE_UID;

			// Token: 0x04001C7D RID: 7293
			private DataColumn columnWSEC_OBJ_RULE_TYPE;
		}

		// Token: 0x020005CD RID: 1485
		public class SecurityCategoriesRow : DataRow
		{
			// Token: 0x06008E51 RID: 36433 RVA: 0x001BDE9C File Offset: 0x001BC09C
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			internal SecurityCategoriesRow(DataRowBuilder rb) : base(rb)
			{
				this.tableSecurityCategories = (SecurityCategoriesDataSet.SecurityCategoriesDataTable)base.Table;
			}

			// Token: 0x17002AF6 RID: 10998
			// (get) Token: 0x06008E52 RID: 36434 RVA: 0x001BDEB6 File Offset: 0x001BC0B6
			// (set) Token: 0x06008E53 RID: 36435 RVA: 0x001BDECE File Offset: 0x001BC0CE
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public Guid WSEC_CAT_UID
			{
				get
				{
					return (Guid)base[this.tableSecurityCategories.WSEC_CAT_UIDColumn];
				}
				set
				{
					base[this.tableSecurityCategories.WSEC_CAT_UIDColumn] = value;
				}
			}

			// Token: 0x17002AF7 RID: 10999
			// (get) Token: 0x06008E54 RID: 36436 RVA: 0x001BDEE7 File Offset: 0x001BC0E7
			// (set) Token: 0x06008E55 RID: 36437 RVA: 0x001BDEFF File Offset: 0x001BC0FF
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			public string WSEC_CAT_NAME
			{
				get
				{
					return (string)base[this.tableSecurityCategories.WSEC_CAT_NAMEColumn];
				}
				set
				{
					base[this.tableSecurityCategories.WSEC_CAT_NAMEColumn] = value;
				}
			}

			// Token: 0x17002AF8 RID: 11000
			// (get) Token: 0x06008E56 RID: 36438 RVA: 0x001BDF14 File Offset: 0x001BC114
			// (set) Token: 0x06008E57 RID: 36439 RVA: 0x001BDF58 File Offset: 0x001BC158
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public string WSEC_CAT_DESC
			{
				get
				{
					string result;
					try
					{
						result = (string)base[this.tableSecurityCategories.WSEC_CAT_DESCColumn];
					}
					catch (InvalidCastException innerException)
					{
						throw new StrongTypingException("The value for column 'WSEC_CAT_DESC' in table 'SecurityCategories' is DBNull.", innerException);
					}
					return result;
				}
				set
				{
					base[this.tableSecurityCategories.WSEC_CAT_DESCColumn] = value;
				}
			}

			// Token: 0x06008E58 RID: 36440 RVA: 0x001BDF6C File Offset: 0x001BC16C
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			public bool IsWSEC_CAT_DESCNull()
			{
				return base.IsNull(this.tableSecurityCategories.WSEC_CAT_DESCColumn);
			}

			// Token: 0x06008E59 RID: 36441 RVA: 0x001BDF7F File Offset: 0x001BC17F
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public void SetWSEC_CAT_DESCNull()
			{
				base[this.tableSecurityCategories.WSEC_CAT_DESCColumn] = Convert.DBNull;
			}

			// Token: 0x04001C82 RID: 7298
			private SecurityCategoriesDataSet.SecurityCategoriesDataTable tableSecurityCategories;
		}

		// Token: 0x020005CE RID: 1486
		public class UserRelationsRow : DataRow
		{
			// Token: 0x06008E5A RID: 36442 RVA: 0x001BDF97 File Offset: 0x001BC197
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			internal UserRelationsRow(DataRowBuilder rb) : base(rb)
			{
				this.tableUserRelations = (SecurityCategoriesDataSet.UserRelationsDataTable)base.Table;
			}

			// Token: 0x17002AF9 RID: 11001
			// (get) Token: 0x06008E5B RID: 36443 RVA: 0x001BDFB1 File Offset: 0x001BC1B1
			// (set) Token: 0x06008E5C RID: 36444 RVA: 0x001BDFC9 File Offset: 0x001BC1C9
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

			// Token: 0x17002AFA RID: 11002
			// (get) Token: 0x06008E5D RID: 36445 RVA: 0x001BDFE2 File Offset: 0x001BC1E2
			// (set) Token: 0x06008E5E RID: 36446 RVA: 0x001BDFFA File Offset: 0x001BC1FA
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

			// Token: 0x04001C83 RID: 7299
			private SecurityCategoriesDataSet.UserRelationsDataTable tableUserRelations;
		}

		// Token: 0x020005CF RID: 1487
		public class GroupRelationsRow : DataRow
		{
			// Token: 0x06008E5F RID: 36447 RVA: 0x001BE013 File Offset: 0x001BC213
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			internal GroupRelationsRow(DataRowBuilder rb) : base(rb)
			{
				this.tableGroupRelations = (SecurityCategoriesDataSet.GroupRelationsDataTable)base.Table;
			}

			// Token: 0x17002AFB RID: 11003
			// (get) Token: 0x06008E60 RID: 36448 RVA: 0x001BE02D File Offset: 0x001BC22D
			// (set) Token: 0x06008E61 RID: 36449 RVA: 0x001BE045 File Offset: 0x001BC245
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

			// Token: 0x17002AFC RID: 11004
			// (get) Token: 0x06008E62 RID: 36450 RVA: 0x001BE05E File Offset: 0x001BC25E
			// (set) Token: 0x06008E63 RID: 36451 RVA: 0x001BE076 File Offset: 0x001BC276
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

			// Token: 0x04001C84 RID: 7300
			private SecurityCategoriesDataSet.GroupRelationsDataTable tableGroupRelations;
		}

		// Token: 0x020005D0 RID: 1488
		public class UserPermissionsRow : DataRow
		{
			// Token: 0x06008E64 RID: 36452 RVA: 0x001BE08F File Offset: 0x001BC28F
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			internal UserPermissionsRow(DataRowBuilder rb) : base(rb)
			{
				this.tableUserPermissions = (SecurityCategoriesDataSet.UserPermissionsDataTable)base.Table;
			}

			// Token: 0x17002AFD RID: 11005
			// (get) Token: 0x06008E65 RID: 36453 RVA: 0x001BE0A9 File Offset: 0x001BC2A9
			// (set) Token: 0x06008E66 RID: 36454 RVA: 0x001BE0C1 File Offset: 0x001BC2C1
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

			// Token: 0x17002AFE RID: 11006
			// (get) Token: 0x06008E67 RID: 36455 RVA: 0x001BE0DA File Offset: 0x001BC2DA
			// (set) Token: 0x06008E68 RID: 36456 RVA: 0x001BE0F2 File Offset: 0x001BC2F2
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
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

			// Token: 0x17002AFF RID: 11007
			// (get) Token: 0x06008E69 RID: 36457 RVA: 0x001BE10B File Offset: 0x001BC30B
			// (set) Token: 0x06008E6A RID: 36458 RVA: 0x001BE123 File Offset: 0x001BC323
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
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

			// Token: 0x17002B00 RID: 11008
			// (get) Token: 0x06008E6B RID: 36459 RVA: 0x001BE13C File Offset: 0x001BC33C
			// (set) Token: 0x06008E6C RID: 36460 RVA: 0x001BE154 File Offset: 0x001BC354
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public bool WSEC_ALLOW
			{
				get
				{
					return (bool)base[this.tableUserPermissions.WSEC_ALLOWColumn];
				}
				set
				{
					base[this.tableUserPermissions.WSEC_ALLOWColumn] = value;
				}
			}

			// Token: 0x17002B01 RID: 11009
			// (get) Token: 0x06008E6D RID: 36461 RVA: 0x001BE16D File Offset: 0x001BC36D
			// (set) Token: 0x06008E6E RID: 36462 RVA: 0x001BE185 File Offset: 0x001BC385
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public bool WSEC_DENY
			{
				get
				{
					return (bool)base[this.tableUserPermissions.WSEC_DENYColumn];
				}
				set
				{
					base[this.tableUserPermissions.WSEC_DENYColumn] = value;
				}
			}

			// Token: 0x04001C85 RID: 7301
			private SecurityCategoriesDataSet.UserPermissionsDataTable tableUserPermissions;
		}

		// Token: 0x020005D1 RID: 1489
		public class GroupPermissionsRow : DataRow
		{
			// Token: 0x06008E6F RID: 36463 RVA: 0x001BE19E File Offset: 0x001BC39E
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			internal GroupPermissionsRow(DataRowBuilder rb) : base(rb)
			{
				this.tableGroupPermissions = (SecurityCategoriesDataSet.GroupPermissionsDataTable)base.Table;
			}

			// Token: 0x17002B02 RID: 11010
			// (get) Token: 0x06008E70 RID: 36464 RVA: 0x001BE1B8 File Offset: 0x001BC3B8
			// (set) Token: 0x06008E71 RID: 36465 RVA: 0x001BE1D0 File Offset: 0x001BC3D0
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

			// Token: 0x17002B03 RID: 11011
			// (get) Token: 0x06008E72 RID: 36466 RVA: 0x001BE1E9 File Offset: 0x001BC3E9
			// (set) Token: 0x06008E73 RID: 36467 RVA: 0x001BE201 File Offset: 0x001BC401
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
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

			// Token: 0x17002B04 RID: 11012
			// (get) Token: 0x06008E74 RID: 36468 RVA: 0x001BE21A File Offset: 0x001BC41A
			// (set) Token: 0x06008E75 RID: 36469 RVA: 0x001BE232 File Offset: 0x001BC432
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
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

			// Token: 0x17002B05 RID: 11013
			// (get) Token: 0x06008E76 RID: 36470 RVA: 0x001BE24B File Offset: 0x001BC44B
			// (set) Token: 0x06008E77 RID: 36471 RVA: 0x001BE263 File Offset: 0x001BC463
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public bool WSEC_ALLOW
			{
				get
				{
					return (bool)base[this.tableGroupPermissions.WSEC_ALLOWColumn];
				}
				set
				{
					base[this.tableGroupPermissions.WSEC_ALLOWColumn] = value;
				}
			}

			// Token: 0x17002B06 RID: 11014
			// (get) Token: 0x06008E78 RID: 36472 RVA: 0x001BE27C File Offset: 0x001BC47C
			// (set) Token: 0x06008E79 RID: 36473 RVA: 0x001BE294 File Offset: 0x001BC494
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public bool WSEC_DENY
			{
				get
				{
					return (bool)base[this.tableGroupPermissions.WSEC_DENYColumn];
				}
				set
				{
					base[this.tableGroupPermissions.WSEC_DENYColumn] = value;
				}
			}

			// Token: 0x04001C86 RID: 7302
			private SecurityCategoriesDataSet.GroupPermissionsDataTable tableGroupPermissions;
		}

		// Token: 0x020005D2 RID: 1490
		public class SecurityCategoryObjectsRow : DataRow
		{
			// Token: 0x06008E7A RID: 36474 RVA: 0x001BE2AD File Offset: 0x001BC4AD
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			internal SecurityCategoryObjectsRow(DataRowBuilder rb) : base(rb)
			{
				this.tableSecurityCategoryObjects = (SecurityCategoriesDataSet.SecurityCategoryObjectsDataTable)base.Table;
			}

			// Token: 0x17002B07 RID: 11015
			// (get) Token: 0x06008E7B RID: 36475 RVA: 0x001BE2C7 File Offset: 0x001BC4C7
			// (set) Token: 0x06008E7C RID: 36476 RVA: 0x001BE2DF File Offset: 0x001BC4DF
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public Guid WSEC_CAT_UID
			{
				get
				{
					return (Guid)base[this.tableSecurityCategoryObjects.WSEC_CAT_UIDColumn];
				}
				set
				{
					base[this.tableSecurityCategoryObjects.WSEC_CAT_UIDColumn] = value;
				}
			}

			// Token: 0x17002B08 RID: 11016
			// (get) Token: 0x06008E7D RID: 36477 RVA: 0x001BE2F8 File Offset: 0x001BC4F8
			// (set) Token: 0x06008E7E RID: 36478 RVA: 0x001BE310 File Offset: 0x001BC510
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public Guid WSEC_OBJ_TYPE_UID
			{
				get
				{
					return (Guid)base[this.tableSecurityCategoryObjects.WSEC_OBJ_TYPE_UIDColumn];
				}
				set
				{
					base[this.tableSecurityCategoryObjects.WSEC_OBJ_TYPE_UIDColumn] = value;
				}
			}

			// Token: 0x17002B09 RID: 11017
			// (get) Token: 0x06008E7F RID: 36479 RVA: 0x001BE329 File Offset: 0x001BC529
			// (set) Token: 0x06008E80 RID: 36480 RVA: 0x001BE341 File Offset: 0x001BC541
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public Guid WSEC_OBJ_UID
			{
				get
				{
					return (Guid)base[this.tableSecurityCategoryObjects.WSEC_OBJ_UIDColumn];
				}
				set
				{
					base[this.tableSecurityCategoryObjects.WSEC_OBJ_UIDColumn] = value;
				}
			}

			// Token: 0x04001C87 RID: 7303
			private SecurityCategoriesDataSet.SecurityCategoryObjectsDataTable tableSecurityCategoryObjects;
		}

		// Token: 0x020005D3 RID: 1491
		public class SecurityCategoryRulesRow : DataRow
		{
			// Token: 0x06008E81 RID: 36481 RVA: 0x001BE35A File Offset: 0x001BC55A
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			internal SecurityCategoryRulesRow(DataRowBuilder rb) : base(rb)
			{
				this.tableSecurityCategoryRules = (SecurityCategoriesDataSet.SecurityCategoryRulesDataTable)base.Table;
			}

			// Token: 0x17002B0A RID: 11018
			// (get) Token: 0x06008E82 RID: 36482 RVA: 0x001BE374 File Offset: 0x001BC574
			// (set) Token: 0x06008E83 RID: 36483 RVA: 0x001BE38C File Offset: 0x001BC58C
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public Guid WSEC_CAT_UID
			{
				get
				{
					return (Guid)base[this.tableSecurityCategoryRules.WSEC_CAT_UIDColumn];
				}
				set
				{
					base[this.tableSecurityCategoryRules.WSEC_CAT_UIDColumn] = value;
				}
			}

			// Token: 0x17002B0B RID: 11019
			// (get) Token: 0x06008E84 RID: 36484 RVA: 0x001BE3A5 File Offset: 0x001BC5A5
			// (set) Token: 0x06008E85 RID: 36485 RVA: 0x001BE3BD File Offset: 0x001BC5BD
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public Guid WSEC_OBJ_TYPE_UID
			{
				get
				{
					return (Guid)base[this.tableSecurityCategoryRules.WSEC_OBJ_TYPE_UIDColumn];
				}
				set
				{
					base[this.tableSecurityCategoryRules.WSEC_OBJ_TYPE_UIDColumn] = value;
				}
			}

			// Token: 0x17002B0C RID: 11020
			// (get) Token: 0x06008E86 RID: 36486 RVA: 0x001BE3D6 File Offset: 0x001BC5D6
			// (set) Token: 0x06008E87 RID: 36487 RVA: 0x001BE3EE File Offset: 0x001BC5EE
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public int WSEC_OBJ_RULE_TYPE
			{
				get
				{
					return (int)base[this.tableSecurityCategoryRules.WSEC_OBJ_RULE_TYPEColumn];
				}
				set
				{
					base[this.tableSecurityCategoryRules.WSEC_OBJ_RULE_TYPEColumn] = value;
				}
			}

			// Token: 0x04001C88 RID: 7304
			private SecurityCategoriesDataSet.SecurityCategoryRulesDataTable tableSecurityCategoryRules;
		}

		// Token: 0x020005D4 RID: 1492
		[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
		public class SecurityCategoriesRowChangeEvent : EventArgs
		{
			// Token: 0x06008E88 RID: 36488 RVA: 0x001BE407 File Offset: 0x001BC607
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public SecurityCategoriesRowChangeEvent(SecurityCategoriesDataSet.SecurityCategoriesRow row, DataRowAction action)
			{
				this.eventRow = row;
				this.eventAction = action;
			}

			// Token: 0x17002B0D RID: 11021
			// (get) Token: 0x06008E89 RID: 36489 RVA: 0x001BE41D File Offset: 0x001BC61D
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public SecurityCategoriesDataSet.SecurityCategoriesRow Row
			{
				get
				{
					return this.eventRow;
				}
			}

			// Token: 0x17002B0E RID: 11022
			// (get) Token: 0x06008E8A RID: 36490 RVA: 0x001BE425 File Offset: 0x001BC625
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public DataRowAction Action
			{
				get
				{
					return this.eventAction;
				}
			}

			// Token: 0x04001C89 RID: 7305
			private SecurityCategoriesDataSet.SecurityCategoriesRow eventRow;

			// Token: 0x04001C8A RID: 7306
			private DataRowAction eventAction;
		}

		// Token: 0x020005D5 RID: 1493
		[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
		public class UserRelationsRowChangeEvent : EventArgs
		{
			// Token: 0x06008E8B RID: 36491 RVA: 0x001BE42D File Offset: 0x001BC62D
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			public UserRelationsRowChangeEvent(SecurityCategoriesDataSet.UserRelationsRow row, DataRowAction action)
			{
				this.eventRow = row;
				this.eventAction = action;
			}

			// Token: 0x17002B0F RID: 11023
			// (get) Token: 0x06008E8C RID: 36492 RVA: 0x001BE443 File Offset: 0x001BC643
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public SecurityCategoriesDataSet.UserRelationsRow Row
			{
				get
				{
					return this.eventRow;
				}
			}

			// Token: 0x17002B10 RID: 11024
			// (get) Token: 0x06008E8D RID: 36493 RVA: 0x001BE44B File Offset: 0x001BC64B
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public DataRowAction Action
			{
				get
				{
					return this.eventAction;
				}
			}

			// Token: 0x04001C8B RID: 7307
			private SecurityCategoriesDataSet.UserRelationsRow eventRow;

			// Token: 0x04001C8C RID: 7308
			private DataRowAction eventAction;
		}

		// Token: 0x020005D6 RID: 1494
		[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
		public class GroupRelationsRowChangeEvent : EventArgs
		{
			// Token: 0x06008E8E RID: 36494 RVA: 0x001BE453 File Offset: 0x001BC653
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public GroupRelationsRowChangeEvent(SecurityCategoriesDataSet.GroupRelationsRow row, DataRowAction action)
			{
				this.eventRow = row;
				this.eventAction = action;
			}

			// Token: 0x17002B11 RID: 11025
			// (get) Token: 0x06008E8F RID: 36495 RVA: 0x001BE469 File Offset: 0x001BC669
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public SecurityCategoriesDataSet.GroupRelationsRow Row
			{
				get
				{
					return this.eventRow;
				}
			}

			// Token: 0x17002B12 RID: 11026
			// (get) Token: 0x06008E90 RID: 36496 RVA: 0x001BE471 File Offset: 0x001BC671
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public DataRowAction Action
			{
				get
				{
					return this.eventAction;
				}
			}

			// Token: 0x04001C8D RID: 7309
			private SecurityCategoriesDataSet.GroupRelationsRow eventRow;

			// Token: 0x04001C8E RID: 7310
			private DataRowAction eventAction;
		}

		// Token: 0x020005D7 RID: 1495
		[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
		public class UserPermissionsRowChangeEvent : EventArgs
		{
			// Token: 0x06008E91 RID: 36497 RVA: 0x001BE479 File Offset: 0x001BC679
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			public UserPermissionsRowChangeEvent(SecurityCategoriesDataSet.UserPermissionsRow row, DataRowAction action)
			{
				this.eventRow = row;
				this.eventAction = action;
			}

			// Token: 0x17002B13 RID: 11027
			// (get) Token: 0x06008E92 RID: 36498 RVA: 0x001BE48F File Offset: 0x001BC68F
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public SecurityCategoriesDataSet.UserPermissionsRow Row
			{
				get
				{
					return this.eventRow;
				}
			}

			// Token: 0x17002B14 RID: 11028
			// (get) Token: 0x06008E93 RID: 36499 RVA: 0x001BE497 File Offset: 0x001BC697
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public DataRowAction Action
			{
				get
				{
					return this.eventAction;
				}
			}

			// Token: 0x04001C8F RID: 7311
			private SecurityCategoriesDataSet.UserPermissionsRow eventRow;

			// Token: 0x04001C90 RID: 7312
			private DataRowAction eventAction;
		}

		// Token: 0x020005D8 RID: 1496
		[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
		public class GroupPermissionsRowChangeEvent : EventArgs
		{
			// Token: 0x06008E94 RID: 36500 RVA: 0x001BE49F File Offset: 0x001BC69F
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			public GroupPermissionsRowChangeEvent(SecurityCategoriesDataSet.GroupPermissionsRow row, DataRowAction action)
			{
				this.eventRow = row;
				this.eventAction = action;
			}

			// Token: 0x17002B15 RID: 11029
			// (get) Token: 0x06008E95 RID: 36501 RVA: 0x001BE4B5 File Offset: 0x001BC6B5
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public SecurityCategoriesDataSet.GroupPermissionsRow Row
			{
				get
				{
					return this.eventRow;
				}
			}

			// Token: 0x17002B16 RID: 11030
			// (get) Token: 0x06008E96 RID: 36502 RVA: 0x001BE4BD File Offset: 0x001BC6BD
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public DataRowAction Action
			{
				get
				{
					return this.eventAction;
				}
			}

			// Token: 0x04001C91 RID: 7313
			private SecurityCategoriesDataSet.GroupPermissionsRow eventRow;

			// Token: 0x04001C92 RID: 7314
			private DataRowAction eventAction;
		}

		// Token: 0x020005D9 RID: 1497
		[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
		public class SecurityCategoryObjectsRowChangeEvent : EventArgs
		{
			// Token: 0x06008E97 RID: 36503 RVA: 0x001BE4C5 File Offset: 0x001BC6C5
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public SecurityCategoryObjectsRowChangeEvent(SecurityCategoriesDataSet.SecurityCategoryObjectsRow row, DataRowAction action)
			{
				this.eventRow = row;
				this.eventAction = action;
			}

			// Token: 0x17002B17 RID: 11031
			// (get) Token: 0x06008E98 RID: 36504 RVA: 0x001BE4DB File Offset: 0x001BC6DB
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public SecurityCategoriesDataSet.SecurityCategoryObjectsRow Row
			{
				get
				{
					return this.eventRow;
				}
			}

			// Token: 0x17002B18 RID: 11032
			// (get) Token: 0x06008E99 RID: 36505 RVA: 0x001BE4E3 File Offset: 0x001BC6E3
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			public DataRowAction Action
			{
				get
				{
					return this.eventAction;
				}
			}

			// Token: 0x04001C93 RID: 7315
			private SecurityCategoriesDataSet.SecurityCategoryObjectsRow eventRow;

			// Token: 0x04001C94 RID: 7316
			private DataRowAction eventAction;
		}

		// Token: 0x020005DA RID: 1498
		[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
		public class SecurityCategoryRulesRowChangeEvent : EventArgs
		{
			// Token: 0x06008E9A RID: 36506 RVA: 0x001BE4EB File Offset: 0x001BC6EB
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public SecurityCategoryRulesRowChangeEvent(SecurityCategoriesDataSet.SecurityCategoryRulesRow row, DataRowAction action)
			{
				this.eventRow = row;
				this.eventAction = action;
			}

			// Token: 0x17002B19 RID: 11033
			// (get) Token: 0x06008E9B RID: 36507 RVA: 0x001BE501 File Offset: 0x001BC701
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public SecurityCategoriesDataSet.SecurityCategoryRulesRow Row
			{
				get
				{
					return this.eventRow;
				}
			}

			// Token: 0x17002B1A RID: 11034
			// (get) Token: 0x06008E9C RID: 36508 RVA: 0x001BE509 File Offset: 0x001BC709
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			public DataRowAction Action
			{
				get
				{
					return this.eventAction;
				}
			}

			// Token: 0x04001C95 RID: 7317
			private SecurityCategoriesDataSet.SecurityCategoryRulesRow eventRow;

			// Token: 0x04001C96 RID: 7318
			private DataRowAction eventAction;
		}
	}
}
