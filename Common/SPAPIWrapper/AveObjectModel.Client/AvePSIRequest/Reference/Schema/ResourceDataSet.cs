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
	// Token: 0x02000567 RID: 1383
	[ToolboxItem(true)]
	[XmlSchemaProvider("GetTypedDataSetSchema")]
	[XmlRoot("ResourceDataSet")]
	[HelpKeyword("vs.data.DataSet")]
	[DesignerCategory("code")]
	[Serializable]
	public class ResourceDataSet : DataSet
	{
		// Token: 0x06008389 RID: 33673 RVA: 0x0019C298 File Offset: 0x0019A498
		public new void EndInit()
		{
			base.EndInit();
			TypedDataSetUtilities.AllowNullsInNonTypedColumns(this.ResourceCustomFields, new string[]
			{
				"NUM_VALUE",
				"RES_UID",
				"CUSTOM_FIELD_UID",
				"INDICATOR_VALUE",
				"TEXT_VALUE",
				"DUR_FMT",
				"DATE_VALUE",
				"MD_PROP_UID",
				"MD_PROP_ID",
				"MD_PROP_NAME",
				"FIELD_TYPE_ENUM",
				"DUR_VALUE",
				"CODE_VALUE",
				"FLAG_VALUE"
			});
			TypedDataSetUtilities.AllowNullsInNonTypedColumns(this.Resources, new string[]
			{
				"RES_EXCHANGE_SYNC",
				"WRES_EMAIL",
				"RES_DEF_ASSN_OWNER",
				"CREATED_DATE",
				"RES_TIMESHEET_MGR_UID",
				"RES_GROUP",
				"RES_CHECKOUTBY",
				"RES_NOTES",
				"RES_EXTERNAL_ID",
				"RES_CODE",
				"RES_HIRE_DATE",
				"RES_EXCHANGE_EWS_URL",
				"RES_HYPERLINK_FRIENDLY_NAME",
				"MOD_DATE",
				"RES_NAME",
				"RES_TYPE",
				"RES_MATERIAL_LABEL",
				"RES_IS_WINDOWS_USER",
				"RES_COST_CENTER",
				"RES_UID",
				"RES_HYPERLINK_SUB_ADDRESS",
				"RES_INITIALS",
				"RES_CHECKOUTDATE",
				"RES_REQUIRES_ENGAGEMENTS",
				"RES_ACCRUE_AT",
				"RES_HYPERLINK_ADDRESS",
				"RES_CAL_OOF_EXCHANGE_SYNC",
				"RES_RTF_NOTES",
				"BaseCalendarUniqueId",
				"RES_PHONETICS",
				"RES_HAS_NOTES",
				"RES_IS_TEAM",
				"RES_ID",
				"RES_CAN_LEVEL",
				"RES_TERMINATION_DATE",
				"WRES_ACCOUNT",
				"WRES_EMAIL_LANGUAGE",
				"RES_BOOKING_TYPE"
			});
			TypedDataSetUtilities.AllowNullsInNonTypedColumns(this.ResourceAvailabilities, new string[]
			{
				"RES_AVAIL_TO",
				"RES_UID",
				"RES_AVAIL_UNITS",
				"RES_AVAIL_FROM"
			});
			TypedDataSetUtilities.AllowNullsInNonTypedColumns(this.ResourceRates, new string[]
			{
				"RES_COST_PER_USE",
				"RES_UID",
				"RES_OVT_RATE",
				"RES_RATE_EFFECTIVE_DATE",
				"RES_RATE_TABLE",
				"RES_STD_RATE"
			});
			TypedDataSetUtilities.AllowNullsInNonTypedColumns(this.CalendarExceptions, new string[]
			{
				"Shift1Start",
				"RES_UID",
				"RecurrenceMonth",
				"RecurrenceFrequency",
				"RecurrenceType",
				"Start",
				"Shift4Start",
				"Shift3Finish",
				"RecurrencePosition",
				"RecurrenceMonthDay",
				"Shift1Finish",
				"RecurrenceDays",
				"Finish",
				"Shift2Start",
				"Shift3Start",
				"Shift5Start",
				"Name",
				"Shift4Finish",
				"Shift2Finish",
				"Shift5Finish"
			});
		}

		// Token: 0x0600838A RID: 33674 RVA: 0x0019C5E0 File Offset: 0x0019A7E0
		[DebuggerNonUserCode]
		[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
		public ResourceDataSet()
		{
			base.BeginInit();
			this.InitClass();
			CollectionChangeEventHandler value = new CollectionChangeEventHandler(this.SchemaChanged);
			base.Tables.CollectionChanged += value;
			base.Relations.CollectionChanged += value;
			this.EndInit();
		}

		// Token: 0x0600838B RID: 33675 RVA: 0x0019C634 File Offset: 0x0019A834
		[DebuggerNonUserCode]
		[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
		protected ResourceDataSet(SerializationInfo info, StreamingContext context) : base(info, context, false)
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
				if (dataSet.Tables["Resources"] != null)
				{
					base.Tables.Add(new ResourceDataSet.ResourcesDataTable(dataSet.Tables["Resources"]));
				}
				if (dataSet.Tables["ResourceCustomFields"] != null)
				{
					base.Tables.Add(new ResourceDataSet.ResourceCustomFieldsDataTable(dataSet.Tables["ResourceCustomFields"]));
				}
				if (dataSet.Tables["CalendarExceptions"] != null)
				{
					base.Tables.Add(new ResourceDataSet.CalendarExceptionsDataTable(dataSet.Tables["CalendarExceptions"]));
				}
				if (dataSet.Tables["ResourceRates"] != null)
				{
					base.Tables.Add(new ResourceDataSet.ResourceRatesDataTable(dataSet.Tables["ResourceRates"]));
				}
				if (dataSet.Tables["ResourceAvailabilities"] != null)
				{
					base.Tables.Add(new ResourceDataSet.ResourceAvailabilitiesDataTable(dataSet.Tables["ResourceAvailabilities"]));
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

		// Token: 0x170027B9 RID: 10169
		// (get) Token: 0x0600838C RID: 33676 RVA: 0x0019C859 File Offset: 0x0019AA59
		[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
		[DebuggerNonUserCode]
		[Browsable(false)]
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Content)]
		public ResourceDataSet.ResourcesDataTable Resources
		{
			get
			{
				return this.tableResources;
			}
		}

		// Token: 0x170027BA RID: 10170
		// (get) Token: 0x0600838D RID: 33677 RVA: 0x0019C861 File Offset: 0x0019AA61
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Content)]
		[DebuggerNonUserCode]
		[Browsable(false)]
		[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
		public ResourceDataSet.ResourceCustomFieldsDataTable ResourceCustomFields
		{
			get
			{
				return this.tableResourceCustomFields;
			}
		}

		// Token: 0x170027BB RID: 10171
		// (get) Token: 0x0600838E RID: 33678 RVA: 0x0019C869 File Offset: 0x0019AA69
		[DebuggerNonUserCode]
		[Browsable(false)]
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Content)]
		[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
		public ResourceDataSet.CalendarExceptionsDataTable CalendarExceptions
		{
			get
			{
				return this.tableCalendarExceptions;
			}
		}

		// Token: 0x170027BC RID: 10172
		// (get) Token: 0x0600838F RID: 33679 RVA: 0x0019C871 File Offset: 0x0019AA71
		[Browsable(false)]
		[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Content)]
		[DebuggerNonUserCode]
		public ResourceDataSet.ResourceRatesDataTable ResourceRates
		{
			get
			{
				return this.tableResourceRates;
			}
		}

		// Token: 0x170027BD RID: 10173
		// (get) Token: 0x06008390 RID: 33680 RVA: 0x0019C879 File Offset: 0x0019AA79
		[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
		[Browsable(false)]
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Content)]
		[DebuggerNonUserCode]
		public ResourceDataSet.ResourceAvailabilitiesDataTable ResourceAvailabilities
		{
			get
			{
				return this.tableResourceAvailabilities;
			}
		}

		// Token: 0x170027BE RID: 10174
		// (get) Token: 0x06008391 RID: 33681 RVA: 0x0019C881 File Offset: 0x0019AA81
		// (set) Token: 0x06008392 RID: 33682 RVA: 0x0019C889 File Offset: 0x0019AA89
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

		// Token: 0x170027BF RID: 10175
		// (get) Token: 0x06008393 RID: 33683 RVA: 0x0019C892 File Offset: 0x0019AA92
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

		// Token: 0x170027C0 RID: 10176
		// (get) Token: 0x06008394 RID: 33684 RVA: 0x0019C89A File Offset: 0x0019AA9A
		[DebuggerNonUserCode]
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
		[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
		public new DataRelationCollection Relations
		{
			get
			{
				return base.Relations;
			}
		}

		// Token: 0x06008395 RID: 33685 RVA: 0x0019C8A2 File Offset: 0x0019AAA2
		[DebuggerNonUserCode]
		[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
		protected override void InitializeDerivedDataSet()
		{
			base.BeginInit();
			this.InitClass();
			this.EndInit();
		}

		// Token: 0x06008396 RID: 33686 RVA: 0x0019C8B8 File Offset: 0x0019AAB8
		[DebuggerNonUserCode]
		[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
		public override DataSet Clone()
		{
			ResourceDataSet resourceDataSet = (ResourceDataSet)base.Clone();
			resourceDataSet.InitVars();
			resourceDataSet.SchemaSerializationMode = this.SchemaSerializationMode;
			return resourceDataSet;
		}

		// Token: 0x06008397 RID: 33687 RVA: 0x0019C8E4 File Offset: 0x0019AAE4
		[DebuggerNonUserCode]
		[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
		protected override bool ShouldSerializeTables()
		{
			return false;
		}

		// Token: 0x06008398 RID: 33688 RVA: 0x0019C8E7 File Offset: 0x0019AAE7
		[DebuggerNonUserCode]
		[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
		protected override bool ShouldSerializeRelations()
		{
			return false;
		}

		// Token: 0x06008399 RID: 33689 RVA: 0x0019C8EC File Offset: 0x0019AAEC
		[DebuggerNonUserCode]
		[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
		protected override void ReadXmlSerializable(XmlReader reader)
		{
			if (base.DetermineSchemaSerializationMode(reader) == SchemaSerializationMode.IncludeSchema)
			{
				this.Reset();
				DataSet dataSet = new DataSet();
				dataSet.ReadXml(reader);
				if (dataSet.Tables["Resources"] != null)
				{
					base.Tables.Add(new ResourceDataSet.ResourcesDataTable(dataSet.Tables["Resources"]));
				}
				if (dataSet.Tables["ResourceCustomFields"] != null)
				{
					base.Tables.Add(new ResourceDataSet.ResourceCustomFieldsDataTable(dataSet.Tables["ResourceCustomFields"]));
				}
				if (dataSet.Tables["CalendarExceptions"] != null)
				{
					base.Tables.Add(new ResourceDataSet.CalendarExceptionsDataTable(dataSet.Tables["CalendarExceptions"]));
				}
				if (dataSet.Tables["ResourceRates"] != null)
				{
					base.Tables.Add(new ResourceDataSet.ResourceRatesDataTable(dataSet.Tables["ResourceRates"]));
				}
				if (dataSet.Tables["ResourceAvailabilities"] != null)
				{
					base.Tables.Add(new ResourceDataSet.ResourceAvailabilitiesDataTable(dataSet.Tables["ResourceAvailabilities"]));
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

		// Token: 0x0600839A RID: 33690 RVA: 0x0019CA7C File Offset: 0x0019AC7C
		[DebuggerNonUserCode]
		[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
		protected override XmlSchema GetSchemaSerializable()
		{
			MemoryStream memoryStream = new MemoryStream();
			base.WriteXmlSchema(new XmlTextWriter(memoryStream, null));
			memoryStream.Position = 0L;
			return XmlSchema.Read(new XmlTextReader(memoryStream), null);
		}

		// Token: 0x0600839B RID: 33691 RVA: 0x0019CAB0 File Offset: 0x0019ACB0
		[DebuggerNonUserCode]
		[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
		internal void InitVars()
		{
			this.InitVars(true);
		}

		// Token: 0x0600839C RID: 33692 RVA: 0x0019CABC File Offset: 0x0019ACBC
		[DebuggerNonUserCode]
		[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
		internal void InitVars(bool initTable)
		{
			this.tableResources = (ResourceDataSet.ResourcesDataTable)base.Tables["Resources"];
			if (initTable && this.tableResources != null)
			{
				this.tableResources.InitVars();
			}
			this.tableResourceCustomFields = (ResourceDataSet.ResourceCustomFieldsDataTable)base.Tables["ResourceCustomFields"];
			if (initTable && this.tableResourceCustomFields != null)
			{
				this.tableResourceCustomFields.InitVars();
			}
			this.tableCalendarExceptions = (ResourceDataSet.CalendarExceptionsDataTable)base.Tables["CalendarExceptions"];
			if (initTable && this.tableCalendarExceptions != null)
			{
				this.tableCalendarExceptions.InitVars();
			}
			this.tableResourceRates = (ResourceDataSet.ResourceRatesDataTable)base.Tables["ResourceRates"];
			if (initTable && this.tableResourceRates != null)
			{
				this.tableResourceRates.InitVars();
			}
			this.tableResourceAvailabilities = (ResourceDataSet.ResourceAvailabilitiesDataTable)base.Tables["ResourceAvailabilities"];
			if (initTable && this.tableResourceAvailabilities != null)
			{
				this.tableResourceAvailabilities.InitVars();
			}
			this.relationResourcesResourceCustomFields = this.Relations["ResourcesResourceCustomFields"];
			this.relationResourcesCalendarExceptions = this.Relations["ResourcesCalendarExceptions"];
			this.relationResourcesResourceRates = this.Relations["ResourcesResourceRates"];
			this.relationResourcesResourceAvailabilities = this.Relations["ResourcesResourceAvailabilities"];
		}

		// Token: 0x0600839D RID: 33693 RVA: 0x0019CC18 File Offset: 0x0019AE18
		[DebuggerNonUserCode]
		[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
		private void InitClass()
		{
			base.DataSetName = "ResourceDataSet";
			base.Prefix = "";
			base.Namespace = "http://schemas.microsoft.com/office/project/server/webservices/ResourceDataSet/";
			base.EnforceConstraints = true;
			this.SchemaSerializationMode = SchemaSerializationMode.IncludeSchema;
			this.tableResources = new ResourceDataSet.ResourcesDataTable();
			base.Tables.Add(this.tableResources);
			this.tableResourceCustomFields = new ResourceDataSet.ResourceCustomFieldsDataTable();
			base.Tables.Add(this.tableResourceCustomFields);
			this.tableCalendarExceptions = new ResourceDataSet.CalendarExceptionsDataTable();
			base.Tables.Add(this.tableCalendarExceptions);
			this.tableResourceRates = new ResourceDataSet.ResourceRatesDataTable();
			base.Tables.Add(this.tableResourceRates);
			this.tableResourceAvailabilities = new ResourceDataSet.ResourceAvailabilitiesDataTable();
			base.Tables.Add(this.tableResourceAvailabilities);
			ForeignKeyConstraint foreignKeyConstraint = new ForeignKeyConstraint("ResourcesResourceCustomFields", new DataColumn[]
			{
				this.tableResources.RES_UIDColumn
			}, new DataColumn[]
			{
				this.tableResourceCustomFields.RES_UIDColumn
			});
			this.tableResourceCustomFields.Constraints.Add(foreignKeyConstraint);
			foreignKeyConstraint.AcceptRejectRule = AcceptRejectRule.None;
			foreignKeyConstraint.DeleteRule = Rule.Cascade;
			foreignKeyConstraint.UpdateRule = Rule.Cascade;
			foreignKeyConstraint = new ForeignKeyConstraint("ResourcesCalendarExceptions", new DataColumn[]
			{
				this.tableResources.RES_UIDColumn
			}, new DataColumn[]
			{
				this.tableCalendarExceptions.RES_UIDColumn
			});
			this.tableCalendarExceptions.Constraints.Add(foreignKeyConstraint);
			foreignKeyConstraint.AcceptRejectRule = AcceptRejectRule.None;
			foreignKeyConstraint.DeleteRule = Rule.Cascade;
			foreignKeyConstraint.UpdateRule = Rule.Cascade;
			foreignKeyConstraint = new ForeignKeyConstraint("ResourcesResourceRates", new DataColumn[]
			{
				this.tableResources.RES_UIDColumn
			}, new DataColumn[]
			{
				this.tableResourceRates.RES_UIDColumn
			});
			this.tableResourceRates.Constraints.Add(foreignKeyConstraint);
			foreignKeyConstraint.AcceptRejectRule = AcceptRejectRule.None;
			foreignKeyConstraint.DeleteRule = Rule.Cascade;
			foreignKeyConstraint.UpdateRule = Rule.Cascade;
			foreignKeyConstraint = new ForeignKeyConstraint("ResourcesResourceAvailabilities", new DataColumn[]
			{
				this.tableResources.RES_UIDColumn
			}, new DataColumn[]
			{
				this.tableResourceAvailabilities.RES_UIDColumn
			});
			this.tableResourceAvailabilities.Constraints.Add(foreignKeyConstraint);
			foreignKeyConstraint.AcceptRejectRule = AcceptRejectRule.None;
			foreignKeyConstraint.DeleteRule = Rule.Cascade;
			foreignKeyConstraint.UpdateRule = Rule.Cascade;
			this.relationResourcesResourceCustomFields = new DataRelation("ResourcesResourceCustomFields", new DataColumn[]
			{
				this.tableResources.RES_UIDColumn
			}, new DataColumn[]
			{
				this.tableResourceCustomFields.RES_UIDColumn
			}, false);
			this.Relations.Add(this.relationResourcesResourceCustomFields);
			this.relationResourcesCalendarExceptions = new DataRelation("ResourcesCalendarExceptions", new DataColumn[]
			{
				this.tableResources.RES_UIDColumn
			}, new DataColumn[]
			{
				this.tableCalendarExceptions.RES_UIDColumn
			}, false);
			this.Relations.Add(this.relationResourcesCalendarExceptions);
			this.relationResourcesResourceRates = new DataRelation("ResourcesResourceRates", new DataColumn[]
			{
				this.tableResources.RES_UIDColumn
			}, new DataColumn[]
			{
				this.tableResourceRates.RES_UIDColumn
			}, false);
			this.Relations.Add(this.relationResourcesResourceRates);
			this.relationResourcesResourceAvailabilities = new DataRelation("ResourcesResourceAvailabilities", new DataColumn[]
			{
				this.tableResources.RES_UIDColumn
			}, new DataColumn[]
			{
				this.tableResourceAvailabilities.RES_UIDColumn
			}, false);
			this.Relations.Add(this.relationResourcesResourceAvailabilities);
		}

		// Token: 0x0600839E RID: 33694 RVA: 0x0019CFB3 File Offset: 0x0019B1B3
		[DebuggerNonUserCode]
		[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
		private bool ShouldSerializeResources()
		{
			return false;
		}

		// Token: 0x0600839F RID: 33695 RVA: 0x0019CFB6 File Offset: 0x0019B1B6
		[DebuggerNonUserCode]
		[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
		private bool ShouldSerializeResourceCustomFields()
		{
			return false;
		}

		// Token: 0x060083A0 RID: 33696 RVA: 0x0019CFB9 File Offset: 0x0019B1B9
		[DebuggerNonUserCode]
		[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
		private bool ShouldSerializeCalendarExceptions()
		{
			return false;
		}

		// Token: 0x060083A1 RID: 33697 RVA: 0x0019CFBC File Offset: 0x0019B1BC
		[DebuggerNonUserCode]
		[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
		private bool ShouldSerializeResourceRates()
		{
			return false;
		}

		// Token: 0x060083A2 RID: 33698 RVA: 0x0019CFBF File Offset: 0x0019B1BF
		[DebuggerNonUserCode]
		[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
		private bool ShouldSerializeResourceAvailabilities()
		{
			return false;
		}

		// Token: 0x060083A3 RID: 33699 RVA: 0x0019CFC2 File Offset: 0x0019B1C2
		[DebuggerNonUserCode]
		[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
		private void SchemaChanged(object sender, CollectionChangeEventArgs e)
		{
			if (e.Action == CollectionChangeAction.Remove)
			{
				this.InitVars();
			}
		}

		// Token: 0x060083A4 RID: 33700 RVA: 0x0019CFD4 File Offset: 0x0019B1D4
		[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
		[DebuggerNonUserCode]
		public static XmlSchemaComplexType GetTypedDataSetSchema(XmlSchemaSet xs)
		{
			ResourceDataSet resourceDataSet = new ResourceDataSet();
			XmlSchemaComplexType xmlSchemaComplexType = new XmlSchemaComplexType();
			XmlSchemaSequence xmlSchemaSequence = new XmlSchemaSequence();
			XmlSchemaAny xmlSchemaAny = new XmlSchemaAny();
			xmlSchemaAny.Namespace = resourceDataSet.Namespace;
			xmlSchemaSequence.Items.Add(xmlSchemaAny);
			xmlSchemaComplexType.Particle = xmlSchemaSequence;
			XmlSchema schemaSerializable = resourceDataSet.GetSchemaSerializable();
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

		// Token: 0x04001A53 RID: 6739
		private ResourceDataSet.ResourcesDataTable tableResources;

		// Token: 0x04001A54 RID: 6740
		private ResourceDataSet.ResourceCustomFieldsDataTable tableResourceCustomFields;

		// Token: 0x04001A55 RID: 6741
		private ResourceDataSet.CalendarExceptionsDataTable tableCalendarExceptions;

		// Token: 0x04001A56 RID: 6742
		private ResourceDataSet.ResourceRatesDataTable tableResourceRates;

		// Token: 0x04001A57 RID: 6743
		private ResourceDataSet.ResourceAvailabilitiesDataTable tableResourceAvailabilities;

		// Token: 0x04001A58 RID: 6744
		private DataRelation relationResourcesResourceCustomFields;

		// Token: 0x04001A59 RID: 6745
		private DataRelation relationResourcesCalendarExceptions;

		// Token: 0x04001A5A RID: 6746
		private DataRelation relationResourcesResourceRates;

		// Token: 0x04001A5B RID: 6747
		private DataRelation relationResourcesResourceAvailabilities;

		// Token: 0x04001A5C RID: 6748
		private SchemaSerializationMode _schemaSerializationMode = SchemaSerializationMode.IncludeSchema;

		// Token: 0x02000568 RID: 1384
		// (Invoke) Token: 0x060083A6 RID: 33702
		[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
		public delegate void ResourcesRowChangeEventHandler(object sender, ResourceDataSet.ResourcesRowChangeEvent e);

		// Token: 0x02000569 RID: 1385
		// (Invoke) Token: 0x060083AA RID: 33706
		[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
		public delegate void ResourceCustomFieldsRowChangeEventHandler(object sender, ResourceDataSet.ResourceCustomFieldsRowChangeEvent e);

		// Token: 0x0200056A RID: 1386
		// (Invoke) Token: 0x060083AE RID: 33710
		[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
		public delegate void CalendarExceptionsRowChangeEventHandler(object sender, ResourceDataSet.CalendarExceptionsRowChangeEvent e);

		// Token: 0x0200056B RID: 1387
		// (Invoke) Token: 0x060083B2 RID: 33714
		[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
		public delegate void ResourceRatesRowChangeEventHandler(object sender, ResourceDataSet.ResourceRatesRowChangeEvent e);

		// Token: 0x0200056C RID: 1388
		// (Invoke) Token: 0x060083B6 RID: 33718
		[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
		public delegate void ResourceAvailabilitiesRowChangeEventHandler(object sender, ResourceDataSet.ResourceAvailabilitiesRowChangeEvent e);

		// Token: 0x0200056D RID: 1389
		[XmlSchemaProvider("GetTypedTableSchema")]
		[Serializable]
		public class ResourcesDataTable : DataTable, IEnumerable
		{
			// Token: 0x060083B9 RID: 33721 RVA: 0x0019D11C File Offset: 0x0019B31C
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public ResourcesDataTable()
			{
				base.TableName = "Resources";
				this.BeginInit();
				this.InitClass();
				this.EndInit();
			}

			// Token: 0x060083BA RID: 33722 RVA: 0x0019D144 File Offset: 0x0019B344
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			internal ResourcesDataTable(DataTable table)
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

			// Token: 0x060083BB RID: 33723 RVA: 0x0019D1EC File Offset: 0x0019B3EC
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			protected ResourcesDataTable(SerializationInfo info, StreamingContext context) : base(info, context)
			{
				this.InitVars();
			}

			// Token: 0x170027C1 RID: 10177
			// (get) Token: 0x060083BC RID: 33724 RVA: 0x0019D1FC File Offset: 0x0019B3FC
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			public DataColumn RES_UIDColumn
			{
				get
				{
					return this.columnRES_UID;
				}
			}

			// Token: 0x170027C2 RID: 10178
			// (get) Token: 0x060083BD RID: 33725 RVA: 0x0019D204 File Offset: 0x0019B404
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			public DataColumn RES_IDColumn
			{
				get
				{
					return this.columnRES_ID;
				}
			}

			// Token: 0x170027C3 RID: 10179
			// (get) Token: 0x060083BE RID: 33726 RVA: 0x0019D20C File Offset: 0x0019B40C
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			public DataColumn RES_TYPEColumn
			{
				get
				{
					return this.columnRES_TYPE;
				}
			}

			// Token: 0x170027C4 RID: 10180
			// (get) Token: 0x060083BF RID: 33727 RVA: 0x0019D214 File Offset: 0x0019B414
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			public DataColumn RES_HAS_NOTESColumn
			{
				get
				{
					return this.columnRES_HAS_NOTES;
				}
			}

			// Token: 0x170027C5 RID: 10181
			// (get) Token: 0x060083C0 RID: 33728 RVA: 0x0019D21C File Offset: 0x0019B41C
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			public DataColumn RES_CAN_LEVELColumn
			{
				get
				{
					return this.columnRES_CAN_LEVEL;
				}
			}

			// Token: 0x170027C6 RID: 10182
			// (get) Token: 0x060083C1 RID: 33729 RVA: 0x0019D224 File Offset: 0x0019B424
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			public DataColumn RES_ACCRUE_ATColumn
			{
				get
				{
					return this.columnRES_ACCRUE_AT;
				}
			}

			// Token: 0x170027C7 RID: 10183
			// (get) Token: 0x060083C2 RID: 33730 RVA: 0x0019D22C File Offset: 0x0019B42C
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			public DataColumn RES_BOOKING_TYPEColumn
			{
				get
				{
					return this.columnRES_BOOKING_TYPE;
				}
			}

			// Token: 0x170027C8 RID: 10184
			// (get) Token: 0x060083C3 RID: 33731 RVA: 0x0019D234 File Offset: 0x0019B434
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			public DataColumn RES_NAMEColumn
			{
				get
				{
					return this.columnRES_NAME;
				}
			}

			// Token: 0x170027C9 RID: 10185
			// (get) Token: 0x060083C4 RID: 33732 RVA: 0x0019D23C File Offset: 0x0019B43C
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			public DataColumn RES_INITIALSColumn
			{
				get
				{
					return this.columnRES_INITIALS;
				}
			}

			// Token: 0x170027CA RID: 10186
			// (get) Token: 0x060083C5 RID: 33733 RVA: 0x0019D244 File Offset: 0x0019B444
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			public DataColumn RES_PHONETICSColumn
			{
				get
				{
					return this.columnRES_PHONETICS;
				}
			}

			// Token: 0x170027CB RID: 10187
			// (get) Token: 0x060083C6 RID: 33734 RVA: 0x0019D24C File Offset: 0x0019B44C
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			public DataColumn RES_MATERIAL_LABELColumn
			{
				get
				{
					return this.columnRES_MATERIAL_LABEL;
				}
			}

			// Token: 0x170027CC RID: 10188
			// (get) Token: 0x060083C7 RID: 33735 RVA: 0x0019D254 File Offset: 0x0019B454
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			public DataColumn RES_RTF_NOTESColumn
			{
				get
				{
					return this.columnRES_RTF_NOTES;
				}
			}

			// Token: 0x170027CD RID: 10189
			// (get) Token: 0x060083C8 RID: 33736 RVA: 0x0019D25C File Offset: 0x0019B45C
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			public DataColumn WRES_ACCOUNTColumn
			{
				get
				{
					return this.columnWRES_ACCOUNT;
				}
			}

			// Token: 0x170027CE RID: 10190
			// (get) Token: 0x060083C9 RID: 33737 RVA: 0x0019D264 File Offset: 0x0019B464
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public DataColumn RES_IS_WINDOWS_USERColumn
			{
				get
				{
					return this.columnRES_IS_WINDOWS_USER;
				}
			}

			// Token: 0x170027CF RID: 10191
			// (get) Token: 0x060083CA RID: 33738 RVA: 0x0019D26C File Offset: 0x0019B46C
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public DataColumn WRES_EMAILColumn
			{
				get
				{
					return this.columnWRES_EMAIL;
				}
			}

			// Token: 0x170027D0 RID: 10192
			// (get) Token: 0x060083CB RID: 33739 RVA: 0x0019D274 File Offset: 0x0019B474
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public DataColumn WRES_EMAIL_LANGUAGEColumn
			{
				get
				{
					return this.columnWRES_EMAIL_LANGUAGE;
				}
			}

			// Token: 0x170027D1 RID: 10193
			// (get) Token: 0x060083CC RID: 33740 RVA: 0x0019D27C File Offset: 0x0019B47C
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public DataColumn RES_CHECKOUTBYColumn
			{
				get
				{
					return this.columnRES_CHECKOUTBY;
				}
			}

			// Token: 0x170027D2 RID: 10194
			// (get) Token: 0x060083CD RID: 33741 RVA: 0x0019D284 File Offset: 0x0019B484
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public DataColumn RES_CHECKOUTDATEColumn
			{
				get
				{
					return this.columnRES_CHECKOUTDATE;
				}
			}

			// Token: 0x170027D3 RID: 10195
			// (get) Token: 0x060083CE RID: 33742 RVA: 0x0019D28C File Offset: 0x0019B48C
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public DataColumn RES_HYPERLINK_FRIENDLY_NAMEColumn
			{
				get
				{
					return this.columnRES_HYPERLINK_FRIENDLY_NAME;
				}
			}

			// Token: 0x170027D4 RID: 10196
			// (get) Token: 0x060083CF RID: 33743 RVA: 0x0019D294 File Offset: 0x0019B494
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public DataColumn RES_HYPERLINK_ADDRESSColumn
			{
				get
				{
					return this.columnRES_HYPERLINK_ADDRESS;
				}
			}

			// Token: 0x170027D5 RID: 10197
			// (get) Token: 0x060083D0 RID: 33744 RVA: 0x0019D29C File Offset: 0x0019B49C
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public DataColumn RES_HYPERLINK_SUB_ADDRESSColumn
			{
				get
				{
					return this.columnRES_HYPERLINK_SUB_ADDRESS;
				}
			}

			// Token: 0x170027D6 RID: 10198
			// (get) Token: 0x060083D1 RID: 33745 RVA: 0x0019D2A4 File Offset: 0x0019B4A4
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public DataColumn RES_CODEColumn
			{
				get
				{
					return this.columnRES_CODE;
				}
			}

			// Token: 0x170027D7 RID: 10199
			// (get) Token: 0x060083D2 RID: 33746 RVA: 0x0019D2AC File Offset: 0x0019B4AC
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public DataColumn RES_GROUPColumn
			{
				get
				{
					return this.columnRES_GROUP;
				}
			}

			// Token: 0x170027D8 RID: 10200
			// (get) Token: 0x060083D3 RID: 33747 RVA: 0x0019D2B4 File Offset: 0x0019B4B4
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public DataColumn RES_EXTERNAL_IDColumn
			{
				get
				{
					return this.columnRES_EXTERNAL_ID;
				}
			}

			// Token: 0x170027D9 RID: 10201
			// (get) Token: 0x060083D4 RID: 33748 RVA: 0x0019D2BC File Offset: 0x0019B4BC
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public DataColumn RES_TIMESHEET_MGR_UIDColumn
			{
				get
				{
					return this.columnRES_TIMESHEET_MGR_UID;
				}
			}

			// Token: 0x170027DA RID: 10202
			// (get) Token: 0x060083D5 RID: 33749 RVA: 0x0019D2C4 File Offset: 0x0019B4C4
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public DataColumn RES_DEF_ASSN_OWNERColumn
			{
				get
				{
					return this.columnRES_DEF_ASSN_OWNER;
				}
			}

			// Token: 0x170027DB RID: 10203
			// (get) Token: 0x060083D6 RID: 33750 RVA: 0x0019D2CC File Offset: 0x0019B4CC
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public DataColumn RES_HIRE_DATEColumn
			{
				get
				{
					return this.columnRES_HIRE_DATE;
				}
			}

			// Token: 0x170027DC RID: 10204
			// (get) Token: 0x060083D7 RID: 33751 RVA: 0x0019D2D4 File Offset: 0x0019B4D4
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public DataColumn RES_TERMINATION_DATEColumn
			{
				get
				{
					return this.columnRES_TERMINATION_DATE;
				}
			}

			// Token: 0x170027DD RID: 10205
			// (get) Token: 0x060083D8 RID: 33752 RVA: 0x0019D2DC File Offset: 0x0019B4DC
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public DataColumn RES_IS_TEAMColumn
			{
				get
				{
					return this.columnRES_IS_TEAM;
				}
			}

			// Token: 0x170027DE RID: 10206
			// (get) Token: 0x060083D9 RID: 33753 RVA: 0x0019D2E4 File Offset: 0x0019B4E4
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public DataColumn RES_EXCHANGE_SYNCColumn
			{
				get
				{
					return this.columnRES_EXCHANGE_SYNC;
				}
			}

			// Token: 0x170027DF RID: 10207
			// (get) Token: 0x060083DA RID: 33754 RVA: 0x0019D2EC File Offset: 0x0019B4EC
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public DataColumn RES_EXCHANGE_EWS_URLColumn
			{
				get
				{
					return this.columnRES_EXCHANGE_EWS_URL;
				}
			}

			// Token: 0x170027E0 RID: 10208
			// (get) Token: 0x060083DB RID: 33755 RVA: 0x0019D2F4 File Offset: 0x0019B4F4
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public DataColumn RES_CAL_OOF_EXCHANGE_SYNCColumn
			{
				get
				{
					return this.columnRES_CAL_OOF_EXCHANGE_SYNC;
				}
			}

			// Token: 0x170027E1 RID: 10209
			// (get) Token: 0x060083DC RID: 33756 RVA: 0x0019D2FC File Offset: 0x0019B4FC
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public DataColumn RES_COST_CENTERColumn
			{
				get
				{
					return this.columnRES_COST_CENTER;
				}
			}

			// Token: 0x170027E2 RID: 10210
			// (get) Token: 0x060083DD RID: 33757 RVA: 0x0019D304 File Offset: 0x0019B504
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public DataColumn RES_NOTESColumn
			{
				get
				{
					return this.columnRES_NOTES;
				}
			}

			// Token: 0x170027E3 RID: 10211
			// (get) Token: 0x060083DE RID: 33758 RVA: 0x0019D30C File Offset: 0x0019B50C
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public DataColumn BaseCalendarUniqueIdColumn
			{
				get
				{
					return this.columnBaseCalendarUniqueId;
				}
			}

			// Token: 0x170027E4 RID: 10212
			// (get) Token: 0x060083DF RID: 33759 RVA: 0x0019D314 File Offset: 0x0019B514
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public DataColumn RES_REQUIRES_ENGAGEMENTSColumn
			{
				get
				{
					return this.columnRES_REQUIRES_ENGAGEMENTS;
				}
			}

			// Token: 0x170027E5 RID: 10213
			// (get) Token: 0x060083E0 RID: 33760 RVA: 0x0019D31C File Offset: 0x0019B51C
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public DataColumn CREATED_DATEColumn
			{
				get
				{
					return this.columnCREATED_DATE;
				}
			}

			// Token: 0x170027E6 RID: 10214
			// (get) Token: 0x060083E1 RID: 33761 RVA: 0x0019D324 File Offset: 0x0019B524
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public DataColumn MOD_DATEColumn
			{
				get
				{
					return this.columnMOD_DATE;
				}
			}

			// Token: 0x170027E7 RID: 10215
			// (get) Token: 0x060083E2 RID: 33762 RVA: 0x0019D32C File Offset: 0x0019B52C
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

			// Token: 0x170027E8 RID: 10216
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public ResourceDataSet.ResourcesRow this[int index]
			{
				get
				{
					return (ResourceDataSet.ResourcesRow)base.Rows[index];
				}
			}

			// Token: 0x140004A1 RID: 1185
			// (add) Token: 0x060083E4 RID: 33764 RVA: 0x0019D34C File Offset: 0x0019B54C
			// (remove) Token: 0x060083E5 RID: 33765 RVA: 0x0019D384 File Offset: 0x0019B584
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public event ResourceDataSet.ResourcesRowChangeEventHandler ResourcesRowChanging;

			// Token: 0x140004A2 RID: 1186
			// (add) Token: 0x060083E6 RID: 33766 RVA: 0x0019D3BC File Offset: 0x0019B5BC
			// (remove) Token: 0x060083E7 RID: 33767 RVA: 0x0019D3F4 File Offset: 0x0019B5F4
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public event ResourceDataSet.ResourcesRowChangeEventHandler ResourcesRowChanged;

			// Token: 0x140004A3 RID: 1187
			// (add) Token: 0x060083E8 RID: 33768 RVA: 0x0019D42C File Offset: 0x0019B62C
			// (remove) Token: 0x060083E9 RID: 33769 RVA: 0x0019D464 File Offset: 0x0019B664
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public event ResourceDataSet.ResourcesRowChangeEventHandler ResourcesRowDeleting;

			// Token: 0x140004A4 RID: 1188
			// (add) Token: 0x060083EA RID: 33770 RVA: 0x0019D49C File Offset: 0x0019B69C
			// (remove) Token: 0x060083EB RID: 33771 RVA: 0x0019D4D4 File Offset: 0x0019B6D4
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public event ResourceDataSet.ResourcesRowChangeEventHandler ResourcesRowDeleted;

			// Token: 0x060083EC RID: 33772 RVA: 0x0019D509 File Offset: 0x0019B709
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			public void AddResourcesRow(ResourceDataSet.ResourcesRow row)
			{
				base.Rows.Add(row);
			}

			// Token: 0x060083ED RID: 33773 RVA: 0x0019D518 File Offset: 0x0019B718
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public ResourceDataSet.ResourcesRow AddResourcesRow(Guid RES_UID, int RES_ID, int RES_TYPE, bool RES_HAS_NOTES, bool RES_CAN_LEVEL, short RES_ACCRUE_AT, int RES_BOOKING_TYPE, string RES_NAME, string RES_INITIALS, string RES_PHONETICS, string RES_MATERIAL_LABEL, byte[] RES_RTF_NOTES, string WRES_ACCOUNT, bool RES_IS_WINDOWS_USER, string WRES_EMAIL, int WRES_EMAIL_LANGUAGE, Guid RES_CHECKOUTBY, DateTime RES_CHECKOUTDATE, string RES_HYPERLINK_FRIENDLY_NAME, string RES_HYPERLINK_ADDRESS, string RES_HYPERLINK_SUB_ADDRESS, string RES_CODE, string RES_GROUP, string RES_EXTERNAL_ID, Guid RES_TIMESHEET_MGR_UID, Guid RES_DEF_ASSN_OWNER, DateTime RES_HIRE_DATE, DateTime RES_TERMINATION_DATE, bool RES_IS_TEAM, bool RES_EXCHANGE_SYNC, string RES_EXCHANGE_EWS_URL, bool RES_CAL_OOF_EXCHANGE_SYNC, string RES_COST_CENTER, string RES_NOTES, Guid BaseCalendarUniqueId, bool RES_REQUIRES_ENGAGEMENTS, DateTime CREATED_DATE, DateTime MOD_DATE)
			{
				ResourceDataSet.ResourcesRow resourcesRow = (ResourceDataSet.ResourcesRow)base.NewRow();
				object[] itemArray = new object[]
				{
					RES_UID,
					RES_ID,
					RES_TYPE,
					RES_HAS_NOTES,
					RES_CAN_LEVEL,
					RES_ACCRUE_AT,
					RES_BOOKING_TYPE,
					RES_NAME,
					RES_INITIALS,
					RES_PHONETICS,
					RES_MATERIAL_LABEL,
					RES_RTF_NOTES,
					WRES_ACCOUNT,
					RES_IS_WINDOWS_USER,
					WRES_EMAIL,
					WRES_EMAIL_LANGUAGE,
					RES_CHECKOUTBY,
					RES_CHECKOUTDATE,
					RES_HYPERLINK_FRIENDLY_NAME,
					RES_HYPERLINK_ADDRESS,
					RES_HYPERLINK_SUB_ADDRESS,
					RES_CODE,
					RES_GROUP,
					RES_EXTERNAL_ID,
					RES_TIMESHEET_MGR_UID,
					RES_DEF_ASSN_OWNER,
					RES_HIRE_DATE,
					RES_TERMINATION_DATE,
					RES_IS_TEAM,
					RES_EXCHANGE_SYNC,
					RES_EXCHANGE_EWS_URL,
					RES_CAL_OOF_EXCHANGE_SYNC,
					RES_COST_CENTER,
					RES_NOTES,
					BaseCalendarUniqueId,
					RES_REQUIRES_ENGAGEMENTS,
					CREATED_DATE,
					MOD_DATE
				};
				resourcesRow.ItemArray = itemArray;
				base.Rows.Add(resourcesRow);
				return resourcesRow;
			}

			// Token: 0x060083EE RID: 33774 RVA: 0x0019D698 File Offset: 0x0019B898
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public ResourceDataSet.ResourcesRow FindByRES_UID(Guid RES_UID)
			{
				return (ResourceDataSet.ResourcesRow)base.Rows.Find(new object[]
				{
					RES_UID
				});
			}

			// Token: 0x060083EF RID: 33775 RVA: 0x0019D6C6 File Offset: 0x0019B8C6
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			public virtual IEnumerator GetEnumerator()
			{
				return base.Rows.GetEnumerator();
			}

			// Token: 0x060083F0 RID: 33776 RVA: 0x0019D6D4 File Offset: 0x0019B8D4
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			public override DataTable Clone()
			{
				ResourceDataSet.ResourcesDataTable resourcesDataTable = (ResourceDataSet.ResourcesDataTable)base.Clone();
				resourcesDataTable.InitVars();
				return resourcesDataTable;
			}

			// Token: 0x060083F1 RID: 33777 RVA: 0x0019D6F4 File Offset: 0x0019B8F4
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			protected override DataTable CreateInstance()
			{
				return new ResourceDataSet.ResourcesDataTable();
			}

			// Token: 0x060083F2 RID: 33778 RVA: 0x0019D6FC File Offset: 0x0019B8FC
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			internal void InitVars()
			{
				this.columnRES_UID = base.Columns["RES_UID"];
				this.columnRES_ID = base.Columns["RES_ID"];
				this.columnRES_TYPE = base.Columns["RES_TYPE"];
				this.columnRES_HAS_NOTES = base.Columns["RES_HAS_NOTES"];
				this.columnRES_CAN_LEVEL = base.Columns["RES_CAN_LEVEL"];
				this.columnRES_ACCRUE_AT = base.Columns["RES_ACCRUE_AT"];
				this.columnRES_BOOKING_TYPE = base.Columns["RES_BOOKING_TYPE"];
				this.columnRES_NAME = base.Columns["RES_NAME"];
				this.columnRES_INITIALS = base.Columns["RES_INITIALS"];
				this.columnRES_PHONETICS = base.Columns["RES_PHONETICS"];
				this.columnRES_MATERIAL_LABEL = base.Columns["RES_MATERIAL_LABEL"];
				this.columnRES_RTF_NOTES = base.Columns["RES_RTF_NOTES"];
				this.columnWRES_ACCOUNT = base.Columns["WRES_ACCOUNT"];
				this.columnRES_IS_WINDOWS_USER = base.Columns["RES_IS_WINDOWS_USER"];
				this.columnWRES_EMAIL = base.Columns["WRES_EMAIL"];
				this.columnWRES_EMAIL_LANGUAGE = base.Columns["WRES_EMAIL_LANGUAGE"];
				this.columnRES_CHECKOUTBY = base.Columns["RES_CHECKOUTBY"];
				this.columnRES_CHECKOUTDATE = base.Columns["RES_CHECKOUTDATE"];
				this.columnRES_HYPERLINK_FRIENDLY_NAME = base.Columns["RES_HYPERLINK_FRIENDLY_NAME"];
				this.columnRES_HYPERLINK_ADDRESS = base.Columns["RES_HYPERLINK_ADDRESS"];
				this.columnRES_HYPERLINK_SUB_ADDRESS = base.Columns["RES_HYPERLINK_SUB_ADDRESS"];
				this.columnRES_CODE = base.Columns["RES_CODE"];
				this.columnRES_GROUP = base.Columns["RES_GROUP"];
				this.columnRES_EXTERNAL_ID = base.Columns["RES_EXTERNAL_ID"];
				this.columnRES_TIMESHEET_MGR_UID = base.Columns["RES_TIMESHEET_MGR_UID"];
				this.columnRES_DEF_ASSN_OWNER = base.Columns["RES_DEF_ASSN_OWNER"];
				this.columnRES_HIRE_DATE = base.Columns["RES_HIRE_DATE"];
				this.columnRES_TERMINATION_DATE = base.Columns["RES_TERMINATION_DATE"];
				this.columnRES_IS_TEAM = base.Columns["RES_IS_TEAM"];
				this.columnRES_EXCHANGE_SYNC = base.Columns["RES_EXCHANGE_SYNC"];
				this.columnRES_EXCHANGE_EWS_URL = base.Columns["RES_EXCHANGE_EWS_URL"];
				this.columnRES_CAL_OOF_EXCHANGE_SYNC = base.Columns["RES_CAL_OOF_EXCHANGE_SYNC"];
				this.columnRES_COST_CENTER = base.Columns["RES_COST_CENTER"];
				this.columnRES_NOTES = base.Columns["RES_NOTES"];
				this.columnBaseCalendarUniqueId = base.Columns["BaseCalendarUniqueId"];
				this.columnRES_REQUIRES_ENGAGEMENTS = base.Columns["RES_REQUIRES_ENGAGEMENTS"];
				this.columnCREATED_DATE = base.Columns["CREATED_DATE"];
				this.columnMOD_DATE = base.Columns["MOD_DATE"];
			}

			// Token: 0x060083F3 RID: 33779 RVA: 0x0019DA50 File Offset: 0x0019BC50
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			private void InitClass()
			{
				this.columnRES_UID = new DataColumn("RES_UID", typeof(Guid), null, MappingType.Element);
				base.Columns.Add(this.columnRES_UID);
				this.columnRES_ID = new DataColumn("RES_ID", typeof(int), null, MappingType.Element);
				base.Columns.Add(this.columnRES_ID);
				this.columnRES_TYPE = new DataColumn("RES_TYPE", typeof(int), null, MappingType.Element);
				base.Columns.Add(this.columnRES_TYPE);
				this.columnRES_HAS_NOTES = new DataColumn("RES_HAS_NOTES", typeof(bool), null, MappingType.Element);
				base.Columns.Add(this.columnRES_HAS_NOTES);
				this.columnRES_CAN_LEVEL = new DataColumn("RES_CAN_LEVEL", typeof(bool), null, MappingType.Element);
				base.Columns.Add(this.columnRES_CAN_LEVEL);
				this.columnRES_ACCRUE_AT = new DataColumn("RES_ACCRUE_AT", typeof(short), null, MappingType.Element);
				base.Columns.Add(this.columnRES_ACCRUE_AT);
				this.columnRES_BOOKING_TYPE = new DataColumn("RES_BOOKING_TYPE", typeof(int), null, MappingType.Element);
				base.Columns.Add(this.columnRES_BOOKING_TYPE);
				this.columnRES_NAME = new DataColumn("RES_NAME", typeof(string), null, MappingType.Element);
				base.Columns.Add(this.columnRES_NAME);
				this.columnRES_INITIALS = new DataColumn("RES_INITIALS", typeof(string), null, MappingType.Element);
				base.Columns.Add(this.columnRES_INITIALS);
				this.columnRES_PHONETICS = new DataColumn("RES_PHONETICS", typeof(string), null, MappingType.Element);
				base.Columns.Add(this.columnRES_PHONETICS);
				this.columnRES_MATERIAL_LABEL = new DataColumn("RES_MATERIAL_LABEL", typeof(string), null, MappingType.Element);
				base.Columns.Add(this.columnRES_MATERIAL_LABEL);
				this.columnRES_RTF_NOTES = new DataColumn("RES_RTF_NOTES", typeof(byte[]), null, MappingType.Element);
				base.Columns.Add(this.columnRES_RTF_NOTES);
				this.columnWRES_ACCOUNT = new DataColumn("WRES_ACCOUNT", typeof(string), null, MappingType.Element);
				base.Columns.Add(this.columnWRES_ACCOUNT);
				this.columnRES_IS_WINDOWS_USER = new DataColumn("RES_IS_WINDOWS_USER", typeof(bool), null, MappingType.Element);
				base.Columns.Add(this.columnRES_IS_WINDOWS_USER);
				this.columnWRES_EMAIL = new DataColumn("WRES_EMAIL", typeof(string), null, MappingType.Element);
				base.Columns.Add(this.columnWRES_EMAIL);
				this.columnWRES_EMAIL_LANGUAGE = new DataColumn("WRES_EMAIL_LANGUAGE", typeof(int), null, MappingType.Element);
				base.Columns.Add(this.columnWRES_EMAIL_LANGUAGE);
				this.columnRES_CHECKOUTBY = new DataColumn("RES_CHECKOUTBY", typeof(Guid), null, MappingType.Element);
				base.Columns.Add(this.columnRES_CHECKOUTBY);
				this.columnRES_CHECKOUTDATE = new DataColumn("RES_CHECKOUTDATE", typeof(DateTime), null, MappingType.Element);
				base.Columns.Add(this.columnRES_CHECKOUTDATE);
				this.columnRES_HYPERLINK_FRIENDLY_NAME = new DataColumn("RES_HYPERLINK_FRIENDLY_NAME", typeof(string), null, MappingType.Element);
				base.Columns.Add(this.columnRES_HYPERLINK_FRIENDLY_NAME);
				this.columnRES_HYPERLINK_ADDRESS = new DataColumn("RES_HYPERLINK_ADDRESS", typeof(string), null, MappingType.Element);
				base.Columns.Add(this.columnRES_HYPERLINK_ADDRESS);
				this.columnRES_HYPERLINK_SUB_ADDRESS = new DataColumn("RES_HYPERLINK_SUB_ADDRESS", typeof(string), null, MappingType.Element);
				base.Columns.Add(this.columnRES_HYPERLINK_SUB_ADDRESS);
				this.columnRES_CODE = new DataColumn("RES_CODE", typeof(string), null, MappingType.Element);
				base.Columns.Add(this.columnRES_CODE);
				this.columnRES_GROUP = new DataColumn("RES_GROUP", typeof(string), null, MappingType.Element);
				base.Columns.Add(this.columnRES_GROUP);
				this.columnRES_EXTERNAL_ID = new DataColumn("RES_EXTERNAL_ID", typeof(string), null, MappingType.Element);
				base.Columns.Add(this.columnRES_EXTERNAL_ID);
				this.columnRES_TIMESHEET_MGR_UID = new DataColumn("RES_TIMESHEET_MGR_UID", typeof(Guid), null, MappingType.Element);
				base.Columns.Add(this.columnRES_TIMESHEET_MGR_UID);
				this.columnRES_DEF_ASSN_OWNER = new DataColumn("RES_DEF_ASSN_OWNER", typeof(Guid), null, MappingType.Element);
				base.Columns.Add(this.columnRES_DEF_ASSN_OWNER);
				this.columnRES_HIRE_DATE = new DataColumn("RES_HIRE_DATE", typeof(DateTime), null, MappingType.Element);
				base.Columns.Add(this.columnRES_HIRE_DATE);
				this.columnRES_TERMINATION_DATE = new DataColumn("RES_TERMINATION_DATE", typeof(DateTime), null, MappingType.Element);
				base.Columns.Add(this.columnRES_TERMINATION_DATE);
				this.columnRES_IS_TEAM = new DataColumn("RES_IS_TEAM", typeof(bool), null, MappingType.Element);
				base.Columns.Add(this.columnRES_IS_TEAM);
				this.columnRES_EXCHANGE_SYNC = new DataColumn("RES_EXCHANGE_SYNC", typeof(bool), null, MappingType.Element);
				base.Columns.Add(this.columnRES_EXCHANGE_SYNC);
				this.columnRES_EXCHANGE_EWS_URL = new DataColumn("RES_EXCHANGE_EWS_URL", typeof(string), null, MappingType.Element);
				base.Columns.Add(this.columnRES_EXCHANGE_EWS_URL);
				this.columnRES_CAL_OOF_EXCHANGE_SYNC = new DataColumn("RES_CAL_OOF_EXCHANGE_SYNC", typeof(bool), null, MappingType.Element);
				base.Columns.Add(this.columnRES_CAL_OOF_EXCHANGE_SYNC);
				this.columnRES_COST_CENTER = new DataColumn("RES_COST_CENTER", typeof(string), null, MappingType.Element);
				base.Columns.Add(this.columnRES_COST_CENTER);
				this.columnRES_NOTES = new DataColumn("RES_NOTES", typeof(string), null, MappingType.Element);
				base.Columns.Add(this.columnRES_NOTES);
				this.columnBaseCalendarUniqueId = new DataColumn("BaseCalendarUniqueId", typeof(Guid), null, MappingType.Element);
				base.Columns.Add(this.columnBaseCalendarUniqueId);
				this.columnRES_REQUIRES_ENGAGEMENTS = new DataColumn("RES_REQUIRES_ENGAGEMENTS", typeof(bool), null, MappingType.Element);
				base.Columns.Add(this.columnRES_REQUIRES_ENGAGEMENTS);
				this.columnCREATED_DATE = new DataColumn("CREATED_DATE", typeof(DateTime), null, MappingType.Element);
				base.Columns.Add(this.columnCREATED_DATE);
				this.columnMOD_DATE = new DataColumn("MOD_DATE", typeof(DateTime), null, MappingType.Element);
				base.Columns.Add(this.columnMOD_DATE);
				base.Constraints.Add(new UniqueConstraint("ResourceDataSetKey1", new DataColumn[]
				{
					this.columnRES_UID
				}, true));
				this.columnRES_UID.AllowDBNull = false;
				this.columnRES_UID.Unique = true;
				this.columnRES_ID.AllowDBNull = false;
				this.columnRES_ID.DefaultValue = -1;
				this.columnRES_TYPE.AllowDBNull = false;
				this.columnRES_TYPE.DefaultValue = 2;
				this.columnRES_HAS_NOTES.DefaultValue = false;
				this.columnRES_CAN_LEVEL.DefaultValue = true;
				this.columnRES_NAME.AllowDBNull = false;
				this.columnWRES_ACCOUNT.ReadOnly = true;
				this.columnRES_IS_WINDOWS_USER.ReadOnly = true;
				this.columnRES_CHECKOUTBY.ReadOnly = true;
				this.columnRES_CHECKOUTDATE.ReadOnly = true;
				this.columnRES_IS_TEAM.DefaultValue = false;
				this.columnRES_EXCHANGE_SYNC.DefaultValue = false;
				this.columnRES_CAL_OOF_EXCHANGE_SYNC.DefaultValue = false;
				this.columnRES_REQUIRES_ENGAGEMENTS.DefaultValue = false;
				this.columnCREATED_DATE.ReadOnly = true;
				this.columnMOD_DATE.ReadOnly = true;
			}

			// Token: 0x060083F4 RID: 33780 RVA: 0x0019E23E File Offset: 0x0019C43E
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			public ResourceDataSet.ResourcesRow NewResourcesRow()
			{
				return (ResourceDataSet.ResourcesRow)base.NewRow();
			}

			// Token: 0x060083F5 RID: 33781 RVA: 0x0019E24B File Offset: 0x0019C44B
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			protected override DataRow NewRowFromBuilder(DataRowBuilder builder)
			{
				return new ResourceDataSet.ResourcesRow(builder);
			}

			// Token: 0x060083F6 RID: 33782 RVA: 0x0019E253 File Offset: 0x0019C453
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			protected override Type GetRowType()
			{
				return typeof(ResourceDataSet.ResourcesRow);
			}

			// Token: 0x060083F7 RID: 33783 RVA: 0x0019E25F File Offset: 0x0019C45F
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			protected override void OnRowChanged(DataRowChangeEventArgs e)
			{
				base.OnRowChanged(e);
				if (this.ResourcesRowChanged != null)
				{
					this.ResourcesRowChanged(this, new ResourceDataSet.ResourcesRowChangeEvent((ResourceDataSet.ResourcesRow)e.Row, e.Action));
				}
			}

			// Token: 0x060083F8 RID: 33784 RVA: 0x0019E292 File Offset: 0x0019C492
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			protected override void OnRowChanging(DataRowChangeEventArgs e)
			{
				base.OnRowChanging(e);
				if (this.ResourcesRowChanging != null)
				{
					this.ResourcesRowChanging(this, new ResourceDataSet.ResourcesRowChangeEvent((ResourceDataSet.ResourcesRow)e.Row, e.Action));
				}
			}

			// Token: 0x060083F9 RID: 33785 RVA: 0x0019E2C5 File Offset: 0x0019C4C5
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			protected override void OnRowDeleted(DataRowChangeEventArgs e)
			{
				base.OnRowDeleted(e);
				if (this.ResourcesRowDeleted != null)
				{
					this.ResourcesRowDeleted(this, new ResourceDataSet.ResourcesRowChangeEvent((ResourceDataSet.ResourcesRow)e.Row, e.Action));
				}
			}

			// Token: 0x060083FA RID: 33786 RVA: 0x0019E2F8 File Offset: 0x0019C4F8
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			protected override void OnRowDeleting(DataRowChangeEventArgs e)
			{
				base.OnRowDeleting(e);
				if (this.ResourcesRowDeleting != null)
				{
					this.ResourcesRowDeleting(this, new ResourceDataSet.ResourcesRowChangeEvent((ResourceDataSet.ResourcesRow)e.Row, e.Action));
				}
			}

			// Token: 0x060083FB RID: 33787 RVA: 0x0019E32B File Offset: 0x0019C52B
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			public void RemoveResourcesRow(ResourceDataSet.ResourcesRow row)
			{
				base.Rows.Remove(row);
			}

			// Token: 0x060083FC RID: 33788 RVA: 0x0019E33C File Offset: 0x0019C53C
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			public static XmlSchemaComplexType GetTypedTableSchema(XmlSchemaSet xs)
			{
				XmlSchemaComplexType xmlSchemaComplexType = new XmlSchemaComplexType();
				XmlSchemaSequence xmlSchemaSequence = new XmlSchemaSequence();
				ResourceDataSet resourceDataSet = new ResourceDataSet();
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
				xmlSchemaAttribute.FixedValue = resourceDataSet.Namespace;
				xmlSchemaComplexType.Attributes.Add(xmlSchemaAttribute);
				XmlSchemaAttribute xmlSchemaAttribute2 = new XmlSchemaAttribute();
				xmlSchemaAttribute2.Name = "tableTypeName";
				xmlSchemaAttribute2.FixedValue = "ResourcesDataTable";
				xmlSchemaComplexType.Attributes.Add(xmlSchemaAttribute2);
				xmlSchemaComplexType.Particle = xmlSchemaSequence;
				XmlSchema schemaSerializable = resourceDataSet.GetSchemaSerializable();
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

			// Token: 0x04001A5D RID: 6749
			private DataColumn columnRES_UID;

			// Token: 0x04001A5E RID: 6750
			private DataColumn columnRES_ID;

			// Token: 0x04001A5F RID: 6751
			private DataColumn columnRES_TYPE;

			// Token: 0x04001A60 RID: 6752
			private DataColumn columnRES_HAS_NOTES;

			// Token: 0x04001A61 RID: 6753
			private DataColumn columnRES_CAN_LEVEL;

			// Token: 0x04001A62 RID: 6754
			private DataColumn columnRES_ACCRUE_AT;

			// Token: 0x04001A63 RID: 6755
			private DataColumn columnRES_BOOKING_TYPE;

			// Token: 0x04001A64 RID: 6756
			private DataColumn columnRES_NAME;

			// Token: 0x04001A65 RID: 6757
			private DataColumn columnRES_INITIALS;

			// Token: 0x04001A66 RID: 6758
			private DataColumn columnRES_PHONETICS;

			// Token: 0x04001A67 RID: 6759
			private DataColumn columnRES_MATERIAL_LABEL;

			// Token: 0x04001A68 RID: 6760
			private DataColumn columnRES_RTF_NOTES;

			// Token: 0x04001A69 RID: 6761
			private DataColumn columnWRES_ACCOUNT;

			// Token: 0x04001A6A RID: 6762
			private DataColumn columnRES_IS_WINDOWS_USER;

			// Token: 0x04001A6B RID: 6763
			private DataColumn columnWRES_EMAIL;

			// Token: 0x04001A6C RID: 6764
			private DataColumn columnWRES_EMAIL_LANGUAGE;

			// Token: 0x04001A6D RID: 6765
			private DataColumn columnRES_CHECKOUTBY;

			// Token: 0x04001A6E RID: 6766
			private DataColumn columnRES_CHECKOUTDATE;

			// Token: 0x04001A6F RID: 6767
			private DataColumn columnRES_HYPERLINK_FRIENDLY_NAME;

			// Token: 0x04001A70 RID: 6768
			private DataColumn columnRES_HYPERLINK_ADDRESS;

			// Token: 0x04001A71 RID: 6769
			private DataColumn columnRES_HYPERLINK_SUB_ADDRESS;

			// Token: 0x04001A72 RID: 6770
			private DataColumn columnRES_CODE;

			// Token: 0x04001A73 RID: 6771
			private DataColumn columnRES_GROUP;

			// Token: 0x04001A74 RID: 6772
			private DataColumn columnRES_EXTERNAL_ID;

			// Token: 0x04001A75 RID: 6773
			private DataColumn columnRES_TIMESHEET_MGR_UID;

			// Token: 0x04001A76 RID: 6774
			private DataColumn columnRES_DEF_ASSN_OWNER;

			// Token: 0x04001A77 RID: 6775
			private DataColumn columnRES_HIRE_DATE;

			// Token: 0x04001A78 RID: 6776
			private DataColumn columnRES_TERMINATION_DATE;

			// Token: 0x04001A79 RID: 6777
			private DataColumn columnRES_IS_TEAM;

			// Token: 0x04001A7A RID: 6778
			private DataColumn columnRES_EXCHANGE_SYNC;

			// Token: 0x04001A7B RID: 6779
			private DataColumn columnRES_EXCHANGE_EWS_URL;

			// Token: 0x04001A7C RID: 6780
			private DataColumn columnRES_CAL_OOF_EXCHANGE_SYNC;

			// Token: 0x04001A7D RID: 6781
			private DataColumn columnRES_COST_CENTER;

			// Token: 0x04001A7E RID: 6782
			private DataColumn columnRES_NOTES;

			// Token: 0x04001A7F RID: 6783
			private DataColumn columnBaseCalendarUniqueId;

			// Token: 0x04001A80 RID: 6784
			private DataColumn columnRES_REQUIRES_ENGAGEMENTS;

			// Token: 0x04001A81 RID: 6785
			private DataColumn columnCREATED_DATE;

			// Token: 0x04001A82 RID: 6786
			private DataColumn columnMOD_DATE;
		}

		// Token: 0x0200056E RID: 1390
		[XmlSchemaProvider("GetTypedTableSchema")]
		[Serializable]
		public class ResourceCustomFieldsDataTable : DataTable, IEnumerable
		{
			// Token: 0x060083FD RID: 33789 RVA: 0x0019E534 File Offset: 0x0019C734
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public ResourceCustomFieldsDataTable()
			{
				base.TableName = "ResourceCustomFields";
				this.BeginInit();
				this.InitClass();
				this.EndInit();
			}

			// Token: 0x060083FE RID: 33790 RVA: 0x0019E55C File Offset: 0x0019C75C
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			internal ResourceCustomFieldsDataTable(DataTable table)
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

			// Token: 0x060083FF RID: 33791 RVA: 0x0019E604 File Offset: 0x0019C804
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			protected ResourceCustomFieldsDataTable(SerializationInfo info, StreamingContext context) : base(info, context)
			{
				this.InitVars();
			}

			// Token: 0x170027E9 RID: 10217
			// (get) Token: 0x06008400 RID: 33792 RVA: 0x0019E614 File Offset: 0x0019C814
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public DataColumn CUSTOM_FIELD_UIDColumn
			{
				get
				{
					return this.columnCUSTOM_FIELD_UID;
				}
			}

			// Token: 0x170027EA RID: 10218
			// (get) Token: 0x06008401 RID: 33793 RVA: 0x0019E61C File Offset: 0x0019C81C
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public DataColumn RES_UIDColumn
			{
				get
				{
					return this.columnRES_UID;
				}
			}

			// Token: 0x170027EB RID: 10219
			// (get) Token: 0x06008402 RID: 33794 RVA: 0x0019E624 File Offset: 0x0019C824
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public DataColumn MD_PROP_UIDColumn
			{
				get
				{
					return this.columnMD_PROP_UID;
				}
			}

			// Token: 0x170027EC RID: 10220
			// (get) Token: 0x06008403 RID: 33795 RVA: 0x0019E62C File Offset: 0x0019C82C
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public DataColumn FLAG_VALUEColumn
			{
				get
				{
					return this.columnFLAG_VALUE;
				}
			}

			// Token: 0x170027ED RID: 10221
			// (get) Token: 0x06008404 RID: 33796 RVA: 0x0019E634 File Offset: 0x0019C834
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public DataColumn MD_PROP_IDColumn
			{
				get
				{
					return this.columnMD_PROP_ID;
				}
			}

			// Token: 0x170027EE RID: 10222
			// (get) Token: 0x06008405 RID: 33797 RVA: 0x0019E63C File Offset: 0x0019C83C
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public DataColumn MD_PROP_NAMEColumn
			{
				get
				{
					return this.columnMD_PROP_NAME;
				}
			}

			// Token: 0x170027EF RID: 10223
			// (get) Token: 0x06008406 RID: 33798 RVA: 0x0019E644 File Offset: 0x0019C844
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public DataColumn TEXT_VALUEColumn
			{
				get
				{
					return this.columnTEXT_VALUE;
				}
			}

			// Token: 0x170027F0 RID: 10224
			// (get) Token: 0x06008407 RID: 33799 RVA: 0x0019E64C File Offset: 0x0019C84C
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public DataColumn FIELD_TYPE_ENUMColumn
			{
				get
				{
					return this.columnFIELD_TYPE_ENUM;
				}
			}

			// Token: 0x170027F1 RID: 10225
			// (get) Token: 0x06008408 RID: 33800 RVA: 0x0019E654 File Offset: 0x0019C854
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public DataColumn DATE_VALUEColumn
			{
				get
				{
					return this.columnDATE_VALUE;
				}
			}

			// Token: 0x170027F2 RID: 10226
			// (get) Token: 0x06008409 RID: 33801 RVA: 0x0019E65C File Offset: 0x0019C85C
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public DataColumn CODE_VALUEColumn
			{
				get
				{
					return this.columnCODE_VALUE;
				}
			}

			// Token: 0x170027F3 RID: 10227
			// (get) Token: 0x0600840A RID: 33802 RVA: 0x0019E664 File Offset: 0x0019C864
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public DataColumn DUR_VALUEColumn
			{
				get
				{
					return this.columnDUR_VALUE;
				}
			}

			// Token: 0x170027F4 RID: 10228
			// (get) Token: 0x0600840B RID: 33803 RVA: 0x0019E66C File Offset: 0x0019C86C
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public DataColumn NUM_VALUEColumn
			{
				get
				{
					return this.columnNUM_VALUE;
				}
			}

			// Token: 0x170027F5 RID: 10229
			// (get) Token: 0x0600840C RID: 33804 RVA: 0x0019E674 File Offset: 0x0019C874
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public DataColumn DUR_FMTColumn
			{
				get
				{
					return this.columnDUR_FMT;
				}
			}

			// Token: 0x170027F6 RID: 10230
			// (get) Token: 0x0600840D RID: 33805 RVA: 0x0019E67C File Offset: 0x0019C87C
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public DataColumn INDICATOR_VALUEColumn
			{
				get
				{
					return this.columnINDICATOR_VALUE;
				}
			}

			// Token: 0x170027F7 RID: 10231
			// (get) Token: 0x0600840E RID: 33806 RVA: 0x0019E684 File Offset: 0x0019C884
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

			// Token: 0x170027F8 RID: 10232
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			public ResourceDataSet.ResourceCustomFieldsRow this[int index]
			{
				get
				{
					return (ResourceDataSet.ResourceCustomFieldsRow)base.Rows[index];
				}
			}

			// Token: 0x140004A5 RID: 1189
			// (add) Token: 0x06008410 RID: 33808 RVA: 0x0019E6A4 File Offset: 0x0019C8A4
			// (remove) Token: 0x06008411 RID: 33809 RVA: 0x0019E6DC File Offset: 0x0019C8DC
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public event ResourceDataSet.ResourceCustomFieldsRowChangeEventHandler ResourceCustomFieldsRowChanging;

			// Token: 0x140004A6 RID: 1190
			// (add) Token: 0x06008412 RID: 33810 RVA: 0x0019E714 File Offset: 0x0019C914
			// (remove) Token: 0x06008413 RID: 33811 RVA: 0x0019E74C File Offset: 0x0019C94C
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public event ResourceDataSet.ResourceCustomFieldsRowChangeEventHandler ResourceCustomFieldsRowChanged;

			// Token: 0x140004A7 RID: 1191
			// (add) Token: 0x06008414 RID: 33812 RVA: 0x0019E784 File Offset: 0x0019C984
			// (remove) Token: 0x06008415 RID: 33813 RVA: 0x0019E7BC File Offset: 0x0019C9BC
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public event ResourceDataSet.ResourceCustomFieldsRowChangeEventHandler ResourceCustomFieldsRowDeleting;

			// Token: 0x140004A8 RID: 1192
			// (add) Token: 0x06008416 RID: 33814 RVA: 0x0019E7F4 File Offset: 0x0019C9F4
			// (remove) Token: 0x06008417 RID: 33815 RVA: 0x0019E82C File Offset: 0x0019CA2C
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public event ResourceDataSet.ResourceCustomFieldsRowChangeEventHandler ResourceCustomFieldsRowDeleted;

			// Token: 0x06008418 RID: 33816 RVA: 0x0019E861 File Offset: 0x0019CA61
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public void AddResourceCustomFieldsRow(ResourceDataSet.ResourceCustomFieldsRow row)
			{
				base.Rows.Add(row);
			}

			// Token: 0x06008419 RID: 33817 RVA: 0x0019E870 File Offset: 0x0019CA70
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			public ResourceDataSet.ResourceCustomFieldsRow AddResourceCustomFieldsRow(Guid CUSTOM_FIELD_UID, ResourceDataSet.ResourcesRow parentResourcesRowByResourcesResourceCustomFields, Guid MD_PROP_UID, bool FLAG_VALUE, int MD_PROP_ID, string MD_PROP_NAME, string TEXT_VALUE, byte FIELD_TYPE_ENUM, DateTime DATE_VALUE, Guid CODE_VALUE, int DUR_VALUE, decimal NUM_VALUE, byte DUR_FMT, int INDICATOR_VALUE)
			{
				ResourceDataSet.ResourceCustomFieldsRow resourceCustomFieldsRow = (ResourceDataSet.ResourceCustomFieldsRow)base.NewRow();
				object[] array = new object[]
				{
					CUSTOM_FIELD_UID,
					null,
					MD_PROP_UID,
					FLAG_VALUE,
					MD_PROP_ID,
					MD_PROP_NAME,
					TEXT_VALUE,
					FIELD_TYPE_ENUM,
					DATE_VALUE,
					CODE_VALUE,
					DUR_VALUE,
					NUM_VALUE,
					DUR_FMT,
					INDICATOR_VALUE
				};
				if (parentResourcesRowByResourcesResourceCustomFields != null)
				{
					array[1] = parentResourcesRowByResourcesResourceCustomFields[0];
				}
				resourceCustomFieldsRow.ItemArray = array;
				base.Rows.Add(resourceCustomFieldsRow);
				return resourceCustomFieldsRow;
			}

			// Token: 0x0600841A RID: 33818 RVA: 0x0019E930 File Offset: 0x0019CB30
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public ResourceDataSet.ResourceCustomFieldsRow FindByCUSTOM_FIELD_UID(Guid CUSTOM_FIELD_UID)
			{
				return (ResourceDataSet.ResourceCustomFieldsRow)base.Rows.Find(new object[]
				{
					CUSTOM_FIELD_UID
				});
			}

			// Token: 0x0600841B RID: 33819 RVA: 0x0019E95E File Offset: 0x0019CB5E
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public virtual IEnumerator GetEnumerator()
			{
				return base.Rows.GetEnumerator();
			}

			// Token: 0x0600841C RID: 33820 RVA: 0x0019E96C File Offset: 0x0019CB6C
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			public override DataTable Clone()
			{
				ResourceDataSet.ResourceCustomFieldsDataTable resourceCustomFieldsDataTable = (ResourceDataSet.ResourceCustomFieldsDataTable)base.Clone();
				resourceCustomFieldsDataTable.InitVars();
				return resourceCustomFieldsDataTable;
			}

			// Token: 0x0600841D RID: 33821 RVA: 0x0019E98C File Offset: 0x0019CB8C
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			protected override DataTable CreateInstance()
			{
				return new ResourceDataSet.ResourceCustomFieldsDataTable();
			}

			// Token: 0x0600841E RID: 33822 RVA: 0x0019E994 File Offset: 0x0019CB94
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			internal void InitVars()
			{
				this.columnCUSTOM_FIELD_UID = base.Columns["CUSTOM_FIELD_UID"];
				this.columnRES_UID = base.Columns["RES_UID"];
				this.columnMD_PROP_UID = base.Columns["MD_PROP_UID"];
				this.columnFLAG_VALUE = base.Columns["FLAG_VALUE"];
				this.columnMD_PROP_ID = base.Columns["MD_PROP_ID"];
				this.columnMD_PROP_NAME = base.Columns["MD_PROP_NAME"];
				this.columnTEXT_VALUE = base.Columns["TEXT_VALUE"];
				this.columnFIELD_TYPE_ENUM = base.Columns["FIELD_TYPE_ENUM"];
				this.columnDATE_VALUE = base.Columns["DATE_VALUE"];
				this.columnCODE_VALUE = base.Columns["CODE_VALUE"];
				this.columnDUR_VALUE = base.Columns["DUR_VALUE"];
				this.columnNUM_VALUE = base.Columns["NUM_VALUE"];
				this.columnDUR_FMT = base.Columns["DUR_FMT"];
				this.columnINDICATOR_VALUE = base.Columns["INDICATOR_VALUE"];
			}

			// Token: 0x0600841F RID: 33823 RVA: 0x0019EAD8 File Offset: 0x0019CCD8
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			private void InitClass()
			{
				this.columnCUSTOM_FIELD_UID = new DataColumn("CUSTOM_FIELD_UID", typeof(Guid), null, MappingType.Element);
				base.Columns.Add(this.columnCUSTOM_FIELD_UID);
				this.columnRES_UID = new DataColumn("RES_UID", typeof(Guid), null, MappingType.Element);
				base.Columns.Add(this.columnRES_UID);
				this.columnMD_PROP_UID = new DataColumn("MD_PROP_UID", typeof(Guid), null, MappingType.Element);
				base.Columns.Add(this.columnMD_PROP_UID);
				this.columnFLAG_VALUE = new DataColumn("FLAG_VALUE", typeof(bool), null, MappingType.Element);
				base.Columns.Add(this.columnFLAG_VALUE);
				this.columnMD_PROP_ID = new DataColumn("MD_PROP_ID", typeof(int), null, MappingType.Element);
				base.Columns.Add(this.columnMD_PROP_ID);
				this.columnMD_PROP_NAME = new DataColumn("MD_PROP_NAME", typeof(string), null, MappingType.Element);
				base.Columns.Add(this.columnMD_PROP_NAME);
				this.columnTEXT_VALUE = new DataColumn("TEXT_VALUE", typeof(string), null, MappingType.Element);
				base.Columns.Add(this.columnTEXT_VALUE);
				this.columnFIELD_TYPE_ENUM = new DataColumn("FIELD_TYPE_ENUM", typeof(byte), null, MappingType.Element);
				base.Columns.Add(this.columnFIELD_TYPE_ENUM);
				this.columnDATE_VALUE = new DataColumn("DATE_VALUE", typeof(DateTime), null, MappingType.Element);
				base.Columns.Add(this.columnDATE_VALUE);
				this.columnCODE_VALUE = new DataColumn("CODE_VALUE", typeof(Guid), null, MappingType.Element);
				base.Columns.Add(this.columnCODE_VALUE);
				this.columnDUR_VALUE = new DataColumn("DUR_VALUE", typeof(int), null, MappingType.Element);
				base.Columns.Add(this.columnDUR_VALUE);
				this.columnNUM_VALUE = new DataColumn("NUM_VALUE", typeof(decimal), null, MappingType.Element);
				base.Columns.Add(this.columnNUM_VALUE);
				this.columnDUR_FMT = new DataColumn("DUR_FMT", typeof(byte), null, MappingType.Element);
				base.Columns.Add(this.columnDUR_FMT);
				this.columnINDICATOR_VALUE = new DataColumn("INDICATOR_VALUE", typeof(int), null, MappingType.Element);
				base.Columns.Add(this.columnINDICATOR_VALUE);
				base.Constraints.Add(new UniqueConstraint("ResourceDataSetKey3", new DataColumn[]
				{
					this.columnCUSTOM_FIELD_UID
				}, true));
				this.columnCUSTOM_FIELD_UID.AllowDBNull = false;
				this.columnCUSTOM_FIELD_UID.Unique = true;
				this.columnRES_UID.AllowDBNull = false;
				this.columnINDICATOR_VALUE.ReadOnly = true;
			}

			// Token: 0x06008420 RID: 33824 RVA: 0x0019EDB2 File Offset: 0x0019CFB2
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public ResourceDataSet.ResourceCustomFieldsRow NewResourceCustomFieldsRow()
			{
				return (ResourceDataSet.ResourceCustomFieldsRow)base.NewRow();
			}

			// Token: 0x06008421 RID: 33825 RVA: 0x0019EDBF File Offset: 0x0019CFBF
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			protected override DataRow NewRowFromBuilder(DataRowBuilder builder)
			{
				return new ResourceDataSet.ResourceCustomFieldsRow(builder);
			}

			// Token: 0x06008422 RID: 33826 RVA: 0x0019EDC7 File Offset: 0x0019CFC7
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			protected override Type GetRowType()
			{
				return typeof(ResourceDataSet.ResourceCustomFieldsRow);
			}

			// Token: 0x06008423 RID: 33827 RVA: 0x0019EDD3 File Offset: 0x0019CFD3
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			protected override void OnRowChanged(DataRowChangeEventArgs e)
			{
				base.OnRowChanged(e);
				if (this.ResourceCustomFieldsRowChanged != null)
				{
					this.ResourceCustomFieldsRowChanged(this, new ResourceDataSet.ResourceCustomFieldsRowChangeEvent((ResourceDataSet.ResourceCustomFieldsRow)e.Row, e.Action));
				}
			}

			// Token: 0x06008424 RID: 33828 RVA: 0x0019EE06 File Offset: 0x0019D006
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			protected override void OnRowChanging(DataRowChangeEventArgs e)
			{
				base.OnRowChanging(e);
				if (this.ResourceCustomFieldsRowChanging != null)
				{
					this.ResourceCustomFieldsRowChanging(this, new ResourceDataSet.ResourceCustomFieldsRowChangeEvent((ResourceDataSet.ResourceCustomFieldsRow)e.Row, e.Action));
				}
			}

			// Token: 0x06008425 RID: 33829 RVA: 0x0019EE39 File Offset: 0x0019D039
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			protected override void OnRowDeleted(DataRowChangeEventArgs e)
			{
				base.OnRowDeleted(e);
				if (this.ResourceCustomFieldsRowDeleted != null)
				{
					this.ResourceCustomFieldsRowDeleted(this, new ResourceDataSet.ResourceCustomFieldsRowChangeEvent((ResourceDataSet.ResourceCustomFieldsRow)e.Row, e.Action));
				}
			}

			// Token: 0x06008426 RID: 33830 RVA: 0x0019EE6C File Offset: 0x0019D06C
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			protected override void OnRowDeleting(DataRowChangeEventArgs e)
			{
				base.OnRowDeleting(e);
				if (this.ResourceCustomFieldsRowDeleting != null)
				{
					this.ResourceCustomFieldsRowDeleting(this, new ResourceDataSet.ResourceCustomFieldsRowChangeEvent((ResourceDataSet.ResourceCustomFieldsRow)e.Row, e.Action));
				}
			}

			// Token: 0x06008427 RID: 33831 RVA: 0x0019EE9F File Offset: 0x0019D09F
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public void RemoveResourceCustomFieldsRow(ResourceDataSet.ResourceCustomFieldsRow row)
			{
				base.Rows.Remove(row);
			}

			// Token: 0x06008428 RID: 33832 RVA: 0x0019EEB0 File Offset: 0x0019D0B0
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public static XmlSchemaComplexType GetTypedTableSchema(XmlSchemaSet xs)
			{
				XmlSchemaComplexType xmlSchemaComplexType = new XmlSchemaComplexType();
				XmlSchemaSequence xmlSchemaSequence = new XmlSchemaSequence();
				ResourceDataSet resourceDataSet = new ResourceDataSet();
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
				xmlSchemaAttribute.FixedValue = resourceDataSet.Namespace;
				xmlSchemaComplexType.Attributes.Add(xmlSchemaAttribute);
				XmlSchemaAttribute xmlSchemaAttribute2 = new XmlSchemaAttribute();
				xmlSchemaAttribute2.Name = "tableTypeName";
				xmlSchemaAttribute2.FixedValue = "ResourceCustomFieldsDataTable";
				xmlSchemaComplexType.Attributes.Add(xmlSchemaAttribute2);
				xmlSchemaComplexType.Particle = xmlSchemaSequence;
				XmlSchema schemaSerializable = resourceDataSet.GetSchemaSerializable();
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

			// Token: 0x04001A87 RID: 6791
			private DataColumn columnCUSTOM_FIELD_UID;

			// Token: 0x04001A88 RID: 6792
			private DataColumn columnRES_UID;

			// Token: 0x04001A89 RID: 6793
			private DataColumn columnMD_PROP_UID;

			// Token: 0x04001A8A RID: 6794
			private DataColumn columnFLAG_VALUE;

			// Token: 0x04001A8B RID: 6795
			private DataColumn columnMD_PROP_ID;

			// Token: 0x04001A8C RID: 6796
			private DataColumn columnMD_PROP_NAME;

			// Token: 0x04001A8D RID: 6797
			private DataColumn columnTEXT_VALUE;

			// Token: 0x04001A8E RID: 6798
			private DataColumn columnFIELD_TYPE_ENUM;

			// Token: 0x04001A8F RID: 6799
			private DataColumn columnDATE_VALUE;

			// Token: 0x04001A90 RID: 6800
			private DataColumn columnCODE_VALUE;

			// Token: 0x04001A91 RID: 6801
			private DataColumn columnDUR_VALUE;

			// Token: 0x04001A92 RID: 6802
			private DataColumn columnNUM_VALUE;

			// Token: 0x04001A93 RID: 6803
			private DataColumn columnDUR_FMT;

			// Token: 0x04001A94 RID: 6804
			private DataColumn columnINDICATOR_VALUE;
		}

		// Token: 0x0200056F RID: 1391
		[XmlSchemaProvider("GetTypedTableSchema")]
		[Serializable]
		public class CalendarExceptionsDataTable : DataTable, IEnumerable
		{
			// Token: 0x06008429 RID: 33833 RVA: 0x0019F0A8 File Offset: 0x0019D2A8
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public CalendarExceptionsDataTable()
			{
				base.TableName = "CalendarExceptions";
				this.BeginInit();
				this.InitClass();
				this.EndInit();
			}

			// Token: 0x0600842A RID: 33834 RVA: 0x0019F0D0 File Offset: 0x0019D2D0
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			internal CalendarExceptionsDataTable(DataTable table)
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

			// Token: 0x0600842B RID: 33835 RVA: 0x0019F178 File Offset: 0x0019D378
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			protected CalendarExceptionsDataTable(SerializationInfo info, StreamingContext context) : base(info, context)
			{
				this.InitVars();
			}

			// Token: 0x170027F9 RID: 10233
			// (get) Token: 0x0600842C RID: 33836 RVA: 0x0019F188 File Offset: 0x0019D388
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			public DataColumn RES_UIDColumn
			{
				get
				{
					return this.columnRES_UID;
				}
			}

			// Token: 0x170027FA RID: 10234
			// (get) Token: 0x0600842D RID: 33837 RVA: 0x0019F190 File Offset: 0x0019D390
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			public DataColumn NameColumn
			{
				get
				{
					return this.columnName;
				}
			}

			// Token: 0x170027FB RID: 10235
			// (get) Token: 0x0600842E RID: 33838 RVA: 0x0019F198 File Offset: 0x0019D398
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			public DataColumn StartColumn
			{
				get
				{
					return this.columnStart;
				}
			}

			// Token: 0x170027FC RID: 10236
			// (get) Token: 0x0600842F RID: 33839 RVA: 0x0019F1A0 File Offset: 0x0019D3A0
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			public DataColumn FinishColumn
			{
				get
				{
					return this.columnFinish;
				}
			}

			// Token: 0x170027FD RID: 10237
			// (get) Token: 0x06008430 RID: 33840 RVA: 0x0019F1A8 File Offset: 0x0019D3A8
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			public DataColumn Shift1StartColumn
			{
				get
				{
					return this.columnShift1Start;
				}
			}

			// Token: 0x170027FE RID: 10238
			// (get) Token: 0x06008431 RID: 33841 RVA: 0x0019F1B0 File Offset: 0x0019D3B0
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			public DataColumn Shift1FinishColumn
			{
				get
				{
					return this.columnShift1Finish;
				}
			}

			// Token: 0x170027FF RID: 10239
			// (get) Token: 0x06008432 RID: 33842 RVA: 0x0019F1B8 File Offset: 0x0019D3B8
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			public DataColumn Shift2StartColumn
			{
				get
				{
					return this.columnShift2Start;
				}
			}

			// Token: 0x17002800 RID: 10240
			// (get) Token: 0x06008433 RID: 33843 RVA: 0x0019F1C0 File Offset: 0x0019D3C0
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			public DataColumn Shift2FinishColumn
			{
				get
				{
					return this.columnShift2Finish;
				}
			}

			// Token: 0x17002801 RID: 10241
			// (get) Token: 0x06008434 RID: 33844 RVA: 0x0019F1C8 File Offset: 0x0019D3C8
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			public DataColumn Shift3StartColumn
			{
				get
				{
					return this.columnShift3Start;
				}
			}

			// Token: 0x17002802 RID: 10242
			// (get) Token: 0x06008435 RID: 33845 RVA: 0x0019F1D0 File Offset: 0x0019D3D0
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			public DataColumn Shift3FinishColumn
			{
				get
				{
					return this.columnShift3Finish;
				}
			}

			// Token: 0x17002803 RID: 10243
			// (get) Token: 0x06008436 RID: 33846 RVA: 0x0019F1D8 File Offset: 0x0019D3D8
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			public DataColumn Shift4StartColumn
			{
				get
				{
					return this.columnShift4Start;
				}
			}

			// Token: 0x17002804 RID: 10244
			// (get) Token: 0x06008437 RID: 33847 RVA: 0x0019F1E0 File Offset: 0x0019D3E0
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			public DataColumn Shift4FinishColumn
			{
				get
				{
					return this.columnShift4Finish;
				}
			}

			// Token: 0x17002805 RID: 10245
			// (get) Token: 0x06008438 RID: 33848 RVA: 0x0019F1E8 File Offset: 0x0019D3E8
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public DataColumn Shift5StartColumn
			{
				get
				{
					return this.columnShift5Start;
				}
			}

			// Token: 0x17002806 RID: 10246
			// (get) Token: 0x06008439 RID: 33849 RVA: 0x0019F1F0 File Offset: 0x0019D3F0
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			public DataColumn Shift5FinishColumn
			{
				get
				{
					return this.columnShift5Finish;
				}
			}

			// Token: 0x17002807 RID: 10247
			// (get) Token: 0x0600843A RID: 33850 RVA: 0x0019F1F8 File Offset: 0x0019D3F8
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			public DataColumn RecurrenceTypeColumn
			{
				get
				{
					return this.columnRecurrenceType;
				}
			}

			// Token: 0x17002808 RID: 10248
			// (get) Token: 0x0600843B RID: 33851 RVA: 0x0019F200 File Offset: 0x0019D400
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			public DataColumn RecurrenceFrequencyColumn
			{
				get
				{
					return this.columnRecurrenceFrequency;
				}
			}

			// Token: 0x17002809 RID: 10249
			// (get) Token: 0x0600843C RID: 33852 RVA: 0x0019F208 File Offset: 0x0019D408
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			public DataColumn RecurrenceDaysColumn
			{
				get
				{
					return this.columnRecurrenceDays;
				}
			}

			// Token: 0x1700280A RID: 10250
			// (get) Token: 0x0600843D RID: 33853 RVA: 0x0019F210 File Offset: 0x0019D410
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public DataColumn RecurrenceMonthDayColumn
			{
				get
				{
					return this.columnRecurrenceMonthDay;
				}
			}

			// Token: 0x1700280B RID: 10251
			// (get) Token: 0x0600843E RID: 33854 RVA: 0x0019F218 File Offset: 0x0019D418
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			public DataColumn RecurrenceMonthColumn
			{
				get
				{
					return this.columnRecurrenceMonth;
				}
			}

			// Token: 0x1700280C RID: 10252
			// (get) Token: 0x0600843F RID: 33855 RVA: 0x0019F220 File Offset: 0x0019D420
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			public DataColumn RecurrencePositionColumn
			{
				get
				{
					return this.columnRecurrencePosition;
				}
			}

			// Token: 0x1700280D RID: 10253
			// (get) Token: 0x06008440 RID: 33856 RVA: 0x0019F228 File Offset: 0x0019D428
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

			// Token: 0x1700280E RID: 10254
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			public ResourceDataSet.CalendarExceptionsRow this[int index]
			{
				get
				{
					return (ResourceDataSet.CalendarExceptionsRow)base.Rows[index];
				}
			}

			// Token: 0x140004A9 RID: 1193
			// (add) Token: 0x06008442 RID: 33858 RVA: 0x0019F248 File Offset: 0x0019D448
			// (remove) Token: 0x06008443 RID: 33859 RVA: 0x0019F280 File Offset: 0x0019D480
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public event ResourceDataSet.CalendarExceptionsRowChangeEventHandler CalendarExceptionsRowChanging;

			// Token: 0x140004AA RID: 1194
			// (add) Token: 0x06008444 RID: 33860 RVA: 0x0019F2B8 File Offset: 0x0019D4B8
			// (remove) Token: 0x06008445 RID: 33861 RVA: 0x0019F2F0 File Offset: 0x0019D4F0
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public event ResourceDataSet.CalendarExceptionsRowChangeEventHandler CalendarExceptionsRowChanged;

			// Token: 0x140004AB RID: 1195
			// (add) Token: 0x06008446 RID: 33862 RVA: 0x0019F328 File Offset: 0x0019D528
			// (remove) Token: 0x06008447 RID: 33863 RVA: 0x0019F360 File Offset: 0x0019D560
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public event ResourceDataSet.CalendarExceptionsRowChangeEventHandler CalendarExceptionsRowDeleting;

			// Token: 0x140004AC RID: 1196
			// (add) Token: 0x06008448 RID: 33864 RVA: 0x0019F398 File Offset: 0x0019D598
			// (remove) Token: 0x06008449 RID: 33865 RVA: 0x0019F3D0 File Offset: 0x0019D5D0
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public event ResourceDataSet.CalendarExceptionsRowChangeEventHandler CalendarExceptionsRowDeleted;

			// Token: 0x0600844A RID: 33866 RVA: 0x0019F405 File Offset: 0x0019D605
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public void AddCalendarExceptionsRow(ResourceDataSet.CalendarExceptionsRow row)
			{
				base.Rows.Add(row);
			}

			// Token: 0x0600844B RID: 33867 RVA: 0x0019F414 File Offset: 0x0019D614
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public ResourceDataSet.CalendarExceptionsRow AddCalendarExceptionsRow(ResourceDataSet.ResourcesRow parentResourcesRowByResourcesCalendarExceptions, string Name, DateTime Start, DateTime Finish, int Shift1Start, int Shift1Finish, int Shift2Start, int Shift2Finish, int Shift3Start, int Shift3Finish, int Shift4Start, int Shift4Finish, int Shift5Start, int Shift5Finish, int RecurrenceType, int RecurrenceFrequency, int RecurrenceDays, int RecurrenceMonthDay, int RecurrenceMonth, int RecurrencePosition)
			{
				ResourceDataSet.CalendarExceptionsRow calendarExceptionsRow = (ResourceDataSet.CalendarExceptionsRow)base.NewRow();
				object[] array = new object[]
				{
					null,
					Name,
					Start,
					Finish,
					Shift1Start,
					Shift1Finish,
					Shift2Start,
					Shift2Finish,
					Shift3Start,
					Shift3Finish,
					Shift4Start,
					Shift4Finish,
					Shift5Start,
					Shift5Finish,
					RecurrenceType,
					RecurrenceFrequency,
					RecurrenceDays,
					RecurrenceMonthDay,
					RecurrenceMonth,
					RecurrencePosition
				};
				if (parentResourcesRowByResourcesCalendarExceptions != null)
				{
					array[0] = parentResourcesRowByResourcesCalendarExceptions[0];
				}
				calendarExceptionsRow.ItemArray = array;
				base.Rows.Add(calendarExceptionsRow);
				return calendarExceptionsRow;
			}

			// Token: 0x0600844C RID: 33868 RVA: 0x0019F51A File Offset: 0x0019D71A
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public virtual IEnumerator GetEnumerator()
			{
				return base.Rows.GetEnumerator();
			}

			// Token: 0x0600844D RID: 33869 RVA: 0x0019F528 File Offset: 0x0019D728
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			public override DataTable Clone()
			{
				ResourceDataSet.CalendarExceptionsDataTable calendarExceptionsDataTable = (ResourceDataSet.CalendarExceptionsDataTable)base.Clone();
				calendarExceptionsDataTable.InitVars();
				return calendarExceptionsDataTable;
			}

			// Token: 0x0600844E RID: 33870 RVA: 0x0019F548 File Offset: 0x0019D748
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			protected override DataTable CreateInstance()
			{
				return new ResourceDataSet.CalendarExceptionsDataTable();
			}

			// Token: 0x0600844F RID: 33871 RVA: 0x0019F550 File Offset: 0x0019D750
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			internal void InitVars()
			{
				this.columnRES_UID = base.Columns["RES_UID"];
				this.columnName = base.Columns["Name"];
				this.columnStart = base.Columns["Start"];
				this.columnFinish = base.Columns["Finish"];
				this.columnShift1Start = base.Columns["Shift1Start"];
				this.columnShift1Finish = base.Columns["Shift1Finish"];
				this.columnShift2Start = base.Columns["Shift2Start"];
				this.columnShift2Finish = base.Columns["Shift2Finish"];
				this.columnShift3Start = base.Columns["Shift3Start"];
				this.columnShift3Finish = base.Columns["Shift3Finish"];
				this.columnShift4Start = base.Columns["Shift4Start"];
				this.columnShift4Finish = base.Columns["Shift4Finish"];
				this.columnShift5Start = base.Columns["Shift5Start"];
				this.columnShift5Finish = base.Columns["Shift5Finish"];
				this.columnRecurrenceType = base.Columns["RecurrenceType"];
				this.columnRecurrenceFrequency = base.Columns["RecurrenceFrequency"];
				this.columnRecurrenceDays = base.Columns["RecurrenceDays"];
				this.columnRecurrenceMonthDay = base.Columns["RecurrenceMonthDay"];
				this.columnRecurrenceMonth = base.Columns["RecurrenceMonth"];
				this.columnRecurrencePosition = base.Columns["RecurrencePosition"];
			}

			// Token: 0x06008450 RID: 33872 RVA: 0x0019F718 File Offset: 0x0019D918
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			private void InitClass()
			{
				this.columnRES_UID = new DataColumn("RES_UID", typeof(Guid), null, MappingType.Element);
				base.Columns.Add(this.columnRES_UID);
				this.columnName = new DataColumn("Name", typeof(string), null, MappingType.Element);
				base.Columns.Add(this.columnName);
				this.columnStart = new DataColumn("Start", typeof(DateTime), null, MappingType.Element);
				base.Columns.Add(this.columnStart);
				this.columnFinish = new DataColumn("Finish", typeof(DateTime), null, MappingType.Element);
				base.Columns.Add(this.columnFinish);
				this.columnShift1Start = new DataColumn("Shift1Start", typeof(int), null, MappingType.Element);
				base.Columns.Add(this.columnShift1Start);
				this.columnShift1Finish = new DataColumn("Shift1Finish", typeof(int), null, MappingType.Element);
				base.Columns.Add(this.columnShift1Finish);
				this.columnShift2Start = new DataColumn("Shift2Start", typeof(int), null, MappingType.Element);
				base.Columns.Add(this.columnShift2Start);
				this.columnShift2Finish = new DataColumn("Shift2Finish", typeof(int), null, MappingType.Element);
				base.Columns.Add(this.columnShift2Finish);
				this.columnShift3Start = new DataColumn("Shift3Start", typeof(int), null, MappingType.Element);
				base.Columns.Add(this.columnShift3Start);
				this.columnShift3Finish = new DataColumn("Shift3Finish", typeof(int), null, MappingType.Element);
				base.Columns.Add(this.columnShift3Finish);
				this.columnShift4Start = new DataColumn("Shift4Start", typeof(int), null, MappingType.Element);
				base.Columns.Add(this.columnShift4Start);
				this.columnShift4Finish = new DataColumn("Shift4Finish", typeof(int), null, MappingType.Element);
				base.Columns.Add(this.columnShift4Finish);
				this.columnShift5Start = new DataColumn("Shift5Start", typeof(int), null, MappingType.Element);
				base.Columns.Add(this.columnShift5Start);
				this.columnShift5Finish = new DataColumn("Shift5Finish", typeof(int), null, MappingType.Element);
				base.Columns.Add(this.columnShift5Finish);
				this.columnRecurrenceType = new DataColumn("RecurrenceType", typeof(int), null, MappingType.Element);
				base.Columns.Add(this.columnRecurrenceType);
				this.columnRecurrenceFrequency = new DataColumn("RecurrenceFrequency", typeof(int), null, MappingType.Element);
				base.Columns.Add(this.columnRecurrenceFrequency);
				this.columnRecurrenceDays = new DataColumn("RecurrenceDays", typeof(int), null, MappingType.Element);
				base.Columns.Add(this.columnRecurrenceDays);
				this.columnRecurrenceMonthDay = new DataColumn("RecurrenceMonthDay", typeof(int), null, MappingType.Element);
				base.Columns.Add(this.columnRecurrenceMonthDay);
				this.columnRecurrenceMonth = new DataColumn("RecurrenceMonth", typeof(int), null, MappingType.Element);
				base.Columns.Add(this.columnRecurrenceMonth);
				this.columnRecurrencePosition = new DataColumn("RecurrencePosition", typeof(int), null, MappingType.Element);
				base.Columns.Add(this.columnRecurrencePosition);
				this.columnRES_UID.AllowDBNull = false;
				this.columnName.AllowDBNull = false;
				this.columnStart.AllowDBNull = false;
				this.columnFinish.AllowDBNull = false;
				this.columnRecurrenceType.AllowDBNull = false;
				this.columnRecurrenceFrequency.AllowDBNull = false;
			}

			// Token: 0x06008451 RID: 33873 RVA: 0x0019FAF1 File Offset: 0x0019DCF1
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public ResourceDataSet.CalendarExceptionsRow NewCalendarExceptionsRow()
			{
				return (ResourceDataSet.CalendarExceptionsRow)base.NewRow();
			}

			// Token: 0x06008452 RID: 33874 RVA: 0x0019FAFE File Offset: 0x0019DCFE
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			protected override DataRow NewRowFromBuilder(DataRowBuilder builder)
			{
				return new ResourceDataSet.CalendarExceptionsRow(builder);
			}

			// Token: 0x06008453 RID: 33875 RVA: 0x0019FB06 File Offset: 0x0019DD06
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			protected override Type GetRowType()
			{
				return typeof(ResourceDataSet.CalendarExceptionsRow);
			}

			// Token: 0x06008454 RID: 33876 RVA: 0x0019FB12 File Offset: 0x0019DD12
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			protected override void OnRowChanged(DataRowChangeEventArgs e)
			{
				base.OnRowChanged(e);
				if (this.CalendarExceptionsRowChanged != null)
				{
					this.CalendarExceptionsRowChanged(this, new ResourceDataSet.CalendarExceptionsRowChangeEvent((ResourceDataSet.CalendarExceptionsRow)e.Row, e.Action));
				}
			}

			// Token: 0x06008455 RID: 33877 RVA: 0x0019FB45 File Offset: 0x0019DD45
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			protected override void OnRowChanging(DataRowChangeEventArgs e)
			{
				base.OnRowChanging(e);
				if (this.CalendarExceptionsRowChanging != null)
				{
					this.CalendarExceptionsRowChanging(this, new ResourceDataSet.CalendarExceptionsRowChangeEvent((ResourceDataSet.CalendarExceptionsRow)e.Row, e.Action));
				}
			}

			// Token: 0x06008456 RID: 33878 RVA: 0x0019FB78 File Offset: 0x0019DD78
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			protected override void OnRowDeleted(DataRowChangeEventArgs e)
			{
				base.OnRowDeleted(e);
				if (this.CalendarExceptionsRowDeleted != null)
				{
					this.CalendarExceptionsRowDeleted(this, new ResourceDataSet.CalendarExceptionsRowChangeEvent((ResourceDataSet.CalendarExceptionsRow)e.Row, e.Action));
				}
			}

			// Token: 0x06008457 RID: 33879 RVA: 0x0019FBAB File Offset: 0x0019DDAB
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			protected override void OnRowDeleting(DataRowChangeEventArgs e)
			{
				base.OnRowDeleting(e);
				if (this.CalendarExceptionsRowDeleting != null)
				{
					this.CalendarExceptionsRowDeleting(this, new ResourceDataSet.CalendarExceptionsRowChangeEvent((ResourceDataSet.CalendarExceptionsRow)e.Row, e.Action));
				}
			}

			// Token: 0x06008458 RID: 33880 RVA: 0x0019FBDE File Offset: 0x0019DDDE
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			public void RemoveCalendarExceptionsRow(ResourceDataSet.CalendarExceptionsRow row)
			{
				base.Rows.Remove(row);
			}

			// Token: 0x06008459 RID: 33881 RVA: 0x0019FBEC File Offset: 0x0019DDEC
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public static XmlSchemaComplexType GetTypedTableSchema(XmlSchemaSet xs)
			{
				XmlSchemaComplexType xmlSchemaComplexType = new XmlSchemaComplexType();
				XmlSchemaSequence xmlSchemaSequence = new XmlSchemaSequence();
				ResourceDataSet resourceDataSet = new ResourceDataSet();
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
				xmlSchemaAttribute.FixedValue = resourceDataSet.Namespace;
				xmlSchemaComplexType.Attributes.Add(xmlSchemaAttribute);
				XmlSchemaAttribute xmlSchemaAttribute2 = new XmlSchemaAttribute();
				xmlSchemaAttribute2.Name = "tableTypeName";
				xmlSchemaAttribute2.FixedValue = "CalendarExceptionsDataTable";
				xmlSchemaComplexType.Attributes.Add(xmlSchemaAttribute2);
				xmlSchemaComplexType.Particle = xmlSchemaSequence;
				XmlSchema schemaSerializable = resourceDataSet.GetSchemaSerializable();
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

			// Token: 0x04001A99 RID: 6809
			private DataColumn columnRES_UID;

			// Token: 0x04001A9A RID: 6810
			private DataColumn columnName;

			// Token: 0x04001A9B RID: 6811
			private DataColumn columnStart;

			// Token: 0x04001A9C RID: 6812
			private DataColumn columnFinish;

			// Token: 0x04001A9D RID: 6813
			private DataColumn columnShift1Start;

			// Token: 0x04001A9E RID: 6814
			private DataColumn columnShift1Finish;

			// Token: 0x04001A9F RID: 6815
			private DataColumn columnShift2Start;

			// Token: 0x04001AA0 RID: 6816
			private DataColumn columnShift2Finish;

			// Token: 0x04001AA1 RID: 6817
			private DataColumn columnShift3Start;

			// Token: 0x04001AA2 RID: 6818
			private DataColumn columnShift3Finish;

			// Token: 0x04001AA3 RID: 6819
			private DataColumn columnShift4Start;

			// Token: 0x04001AA4 RID: 6820
			private DataColumn columnShift4Finish;

			// Token: 0x04001AA5 RID: 6821
			private DataColumn columnShift5Start;

			// Token: 0x04001AA6 RID: 6822
			private DataColumn columnShift5Finish;

			// Token: 0x04001AA7 RID: 6823
			private DataColumn columnRecurrenceType;

			// Token: 0x04001AA8 RID: 6824
			private DataColumn columnRecurrenceFrequency;

			// Token: 0x04001AA9 RID: 6825
			private DataColumn columnRecurrenceDays;

			// Token: 0x04001AAA RID: 6826
			private DataColumn columnRecurrenceMonthDay;

			// Token: 0x04001AAB RID: 6827
			private DataColumn columnRecurrenceMonth;

			// Token: 0x04001AAC RID: 6828
			private DataColumn columnRecurrencePosition;
		}

		// Token: 0x02000570 RID: 1392
		[XmlSchemaProvider("GetTypedTableSchema")]
		[Serializable]
		public class ResourceRatesDataTable : DataTable, IEnumerable
		{
			// Token: 0x0600845A RID: 33882 RVA: 0x0019FDE4 File Offset: 0x0019DFE4
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public ResourceRatesDataTable()
			{
				base.TableName = "ResourceRates";
				this.BeginInit();
				this.InitClass();
				this.EndInit();
			}

			// Token: 0x0600845B RID: 33883 RVA: 0x0019FE0C File Offset: 0x0019E00C
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			internal ResourceRatesDataTable(DataTable table)
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

			// Token: 0x0600845C RID: 33884 RVA: 0x0019FEB4 File Offset: 0x0019E0B4
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			protected ResourceRatesDataTable(SerializationInfo info, StreamingContext context) : base(info, context)
			{
				this.InitVars();
			}

			// Token: 0x1700280F RID: 10255
			// (get) Token: 0x0600845D RID: 33885 RVA: 0x0019FEC4 File Offset: 0x0019E0C4
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			public DataColumn RES_UIDColumn
			{
				get
				{
					return this.columnRES_UID;
				}
			}

			// Token: 0x17002810 RID: 10256
			// (get) Token: 0x0600845E RID: 33886 RVA: 0x0019FECC File Offset: 0x0019E0CC
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			public DataColumn RES_RATE_TABLEColumn
			{
				get
				{
					return this.columnRES_RATE_TABLE;
				}
			}

			// Token: 0x17002811 RID: 10257
			// (get) Token: 0x0600845F RID: 33887 RVA: 0x0019FED4 File Offset: 0x0019E0D4
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			public DataColumn RES_RATE_EFFECTIVE_DATEColumn
			{
				get
				{
					return this.columnRES_RATE_EFFECTIVE_DATE;
				}
			}

			// Token: 0x17002812 RID: 10258
			// (get) Token: 0x06008460 RID: 33888 RVA: 0x0019FEDC File Offset: 0x0019E0DC
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			public DataColumn RES_STD_RATEColumn
			{
				get
				{
					return this.columnRES_STD_RATE;
				}
			}

			// Token: 0x17002813 RID: 10259
			// (get) Token: 0x06008461 RID: 33889 RVA: 0x0019FEE4 File Offset: 0x0019E0E4
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			public DataColumn RES_OVT_RATEColumn
			{
				get
				{
					return this.columnRES_OVT_RATE;
				}
			}

			// Token: 0x17002814 RID: 10260
			// (get) Token: 0x06008462 RID: 33890 RVA: 0x0019FEEC File Offset: 0x0019E0EC
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			public DataColumn RES_COST_PER_USEColumn
			{
				get
				{
					return this.columnRES_COST_PER_USE;
				}
			}

			// Token: 0x17002815 RID: 10261
			// (get) Token: 0x06008463 RID: 33891 RVA: 0x0019FEF4 File Offset: 0x0019E0F4
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

			// Token: 0x17002816 RID: 10262
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			public ResourceDataSet.ResourceRatesRow this[int index]
			{
				get
				{
					return (ResourceDataSet.ResourceRatesRow)base.Rows[index];
				}
			}

			// Token: 0x140004AD RID: 1197
			// (add) Token: 0x06008465 RID: 33893 RVA: 0x0019FF14 File Offset: 0x0019E114
			// (remove) Token: 0x06008466 RID: 33894 RVA: 0x0019FF4C File Offset: 0x0019E14C
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public event ResourceDataSet.ResourceRatesRowChangeEventHandler ResourceRatesRowChanging;

			// Token: 0x140004AE RID: 1198
			// (add) Token: 0x06008467 RID: 33895 RVA: 0x0019FF84 File Offset: 0x0019E184
			// (remove) Token: 0x06008468 RID: 33896 RVA: 0x0019FFBC File Offset: 0x0019E1BC
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public event ResourceDataSet.ResourceRatesRowChangeEventHandler ResourceRatesRowChanged;

			// Token: 0x140004AF RID: 1199
			// (add) Token: 0x06008469 RID: 33897 RVA: 0x0019FFF4 File Offset: 0x0019E1F4
			// (remove) Token: 0x0600846A RID: 33898 RVA: 0x001A002C File Offset: 0x0019E22C
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public event ResourceDataSet.ResourceRatesRowChangeEventHandler ResourceRatesRowDeleting;

			// Token: 0x140004B0 RID: 1200
			// (add) Token: 0x0600846B RID: 33899 RVA: 0x001A0064 File Offset: 0x0019E264
			// (remove) Token: 0x0600846C RID: 33900 RVA: 0x001A009C File Offset: 0x0019E29C
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public event ResourceDataSet.ResourceRatesRowChangeEventHandler ResourceRatesRowDeleted;

			// Token: 0x0600846D RID: 33901 RVA: 0x001A00D1 File Offset: 0x0019E2D1
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public void AddResourceRatesRow(ResourceDataSet.ResourceRatesRow row)
			{
				base.Rows.Add(row);
			}

			// Token: 0x0600846E RID: 33902 RVA: 0x001A00E0 File Offset: 0x0019E2E0
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			public ResourceDataSet.ResourceRatesRow AddResourceRatesRow(ResourceDataSet.ResourcesRow parentResourcesRowByResourcesResourceRates, int RES_RATE_TABLE, DateTime RES_RATE_EFFECTIVE_DATE, double RES_STD_RATE, double RES_OVT_RATE, double RES_COST_PER_USE)
			{
				ResourceDataSet.ResourceRatesRow resourceRatesRow = (ResourceDataSet.ResourceRatesRow)base.NewRow();
				object[] array = new object[]
				{
					null,
					RES_RATE_TABLE,
					RES_RATE_EFFECTIVE_DATE,
					RES_STD_RATE,
					RES_OVT_RATE,
					RES_COST_PER_USE
				};
				if (parentResourcesRowByResourcesResourceRates != null)
				{
					array[0] = parentResourcesRowByResourcesResourceRates[0];
				}
				resourceRatesRow.ItemArray = array;
				base.Rows.Add(resourceRatesRow);
				return resourceRatesRow;
			}

			// Token: 0x0600846F RID: 33903 RVA: 0x001A0153 File Offset: 0x0019E353
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public virtual IEnumerator GetEnumerator()
			{
				return base.Rows.GetEnumerator();
			}

			// Token: 0x06008470 RID: 33904 RVA: 0x001A0160 File Offset: 0x0019E360
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			public override DataTable Clone()
			{
				ResourceDataSet.ResourceRatesDataTable resourceRatesDataTable = (ResourceDataSet.ResourceRatesDataTable)base.Clone();
				resourceRatesDataTable.InitVars();
				return resourceRatesDataTable;
			}

			// Token: 0x06008471 RID: 33905 RVA: 0x001A0180 File Offset: 0x0019E380
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			protected override DataTable CreateInstance()
			{
				return new ResourceDataSet.ResourceRatesDataTable();
			}

			// Token: 0x06008472 RID: 33906 RVA: 0x001A0188 File Offset: 0x0019E388
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			internal void InitVars()
			{
				this.columnRES_UID = base.Columns["RES_UID"];
				this.columnRES_RATE_TABLE = base.Columns["RES_RATE_TABLE"];
				this.columnRES_RATE_EFFECTIVE_DATE = base.Columns["RES_RATE_EFFECTIVE_DATE"];
				this.columnRES_STD_RATE = base.Columns["RES_STD_RATE"];
				this.columnRES_OVT_RATE = base.Columns["RES_OVT_RATE"];
				this.columnRES_COST_PER_USE = base.Columns["RES_COST_PER_USE"];
			}

			// Token: 0x06008473 RID: 33907 RVA: 0x001A021C File Offset: 0x0019E41C
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			private void InitClass()
			{
				this.columnRES_UID = new DataColumn("RES_UID", typeof(Guid), null, MappingType.Element);
				base.Columns.Add(this.columnRES_UID);
				this.columnRES_RATE_TABLE = new DataColumn("RES_RATE_TABLE", typeof(int), null, MappingType.Element);
				base.Columns.Add(this.columnRES_RATE_TABLE);
				this.columnRES_RATE_EFFECTIVE_DATE = new DataColumn("RES_RATE_EFFECTIVE_DATE", typeof(DateTime), null, MappingType.Element);
				base.Columns.Add(this.columnRES_RATE_EFFECTIVE_DATE);
				this.columnRES_STD_RATE = new DataColumn("RES_STD_RATE", typeof(double), null, MappingType.Element);
				base.Columns.Add(this.columnRES_STD_RATE);
				this.columnRES_OVT_RATE = new DataColumn("RES_OVT_RATE", typeof(double), null, MappingType.Element);
				base.Columns.Add(this.columnRES_OVT_RATE);
				this.columnRES_COST_PER_USE = new DataColumn("RES_COST_PER_USE", typeof(double), null, MappingType.Element);
				base.Columns.Add(this.columnRES_COST_PER_USE);
				this.columnRES_UID.AllowDBNull = false;
				this.columnRES_RATE_TABLE.AllowDBNull = false;
				this.columnRES_RATE_TABLE.DefaultValue = 0;
				this.columnRES_STD_RATE.DefaultValue = 0.0;
				this.columnRES_OVT_RATE.DefaultValue = 0.0;
				this.columnRES_COST_PER_USE.DefaultValue = 0.0;
			}

			// Token: 0x06008474 RID: 33908 RVA: 0x001A03AB File Offset: 0x0019E5AB
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public ResourceDataSet.ResourceRatesRow NewResourceRatesRow()
			{
				return (ResourceDataSet.ResourceRatesRow)base.NewRow();
			}

			// Token: 0x06008475 RID: 33909 RVA: 0x001A03B8 File Offset: 0x0019E5B8
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			protected override DataRow NewRowFromBuilder(DataRowBuilder builder)
			{
				return new ResourceDataSet.ResourceRatesRow(builder);
			}

			// Token: 0x06008476 RID: 33910 RVA: 0x001A03C0 File Offset: 0x0019E5C0
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			protected override Type GetRowType()
			{
				return typeof(ResourceDataSet.ResourceRatesRow);
			}

			// Token: 0x06008477 RID: 33911 RVA: 0x001A03CC File Offset: 0x0019E5CC
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			protected override void OnRowChanged(DataRowChangeEventArgs e)
			{
				base.OnRowChanged(e);
				if (this.ResourceRatesRowChanged != null)
				{
					this.ResourceRatesRowChanged(this, new ResourceDataSet.ResourceRatesRowChangeEvent((ResourceDataSet.ResourceRatesRow)e.Row, e.Action));
				}
			}

			// Token: 0x06008478 RID: 33912 RVA: 0x001A03FF File Offset: 0x0019E5FF
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			protected override void OnRowChanging(DataRowChangeEventArgs e)
			{
				base.OnRowChanging(e);
				if (this.ResourceRatesRowChanging != null)
				{
					this.ResourceRatesRowChanging(this, new ResourceDataSet.ResourceRatesRowChangeEvent((ResourceDataSet.ResourceRatesRow)e.Row, e.Action));
				}
			}

			// Token: 0x06008479 RID: 33913 RVA: 0x001A0432 File Offset: 0x0019E632
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			protected override void OnRowDeleted(DataRowChangeEventArgs e)
			{
				base.OnRowDeleted(e);
				if (this.ResourceRatesRowDeleted != null)
				{
					this.ResourceRatesRowDeleted(this, new ResourceDataSet.ResourceRatesRowChangeEvent((ResourceDataSet.ResourceRatesRow)e.Row, e.Action));
				}
			}

			// Token: 0x0600847A RID: 33914 RVA: 0x001A0465 File Offset: 0x0019E665
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			protected override void OnRowDeleting(DataRowChangeEventArgs e)
			{
				base.OnRowDeleting(e);
				if (this.ResourceRatesRowDeleting != null)
				{
					this.ResourceRatesRowDeleting(this, new ResourceDataSet.ResourceRatesRowChangeEvent((ResourceDataSet.ResourceRatesRow)e.Row, e.Action));
				}
			}

			// Token: 0x0600847B RID: 33915 RVA: 0x001A0498 File Offset: 0x0019E698
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			public void RemoveResourceRatesRow(ResourceDataSet.ResourceRatesRow row)
			{
				base.Rows.Remove(row);
			}

			// Token: 0x0600847C RID: 33916 RVA: 0x001A04A8 File Offset: 0x0019E6A8
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public static XmlSchemaComplexType GetTypedTableSchema(XmlSchemaSet xs)
			{
				XmlSchemaComplexType xmlSchemaComplexType = new XmlSchemaComplexType();
				XmlSchemaSequence xmlSchemaSequence = new XmlSchemaSequence();
				ResourceDataSet resourceDataSet = new ResourceDataSet();
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
				xmlSchemaAttribute.FixedValue = resourceDataSet.Namespace;
				xmlSchemaComplexType.Attributes.Add(xmlSchemaAttribute);
				XmlSchemaAttribute xmlSchemaAttribute2 = new XmlSchemaAttribute();
				xmlSchemaAttribute2.Name = "tableTypeName";
				xmlSchemaAttribute2.FixedValue = "ResourceRatesDataTable";
				xmlSchemaComplexType.Attributes.Add(xmlSchemaAttribute2);
				xmlSchemaComplexType.Particle = xmlSchemaSequence;
				XmlSchema schemaSerializable = resourceDataSet.GetSchemaSerializable();
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

			// Token: 0x04001AB1 RID: 6833
			private DataColumn columnRES_UID;

			// Token: 0x04001AB2 RID: 6834
			private DataColumn columnRES_RATE_TABLE;

			// Token: 0x04001AB3 RID: 6835
			private DataColumn columnRES_RATE_EFFECTIVE_DATE;

			// Token: 0x04001AB4 RID: 6836
			private DataColumn columnRES_STD_RATE;

			// Token: 0x04001AB5 RID: 6837
			private DataColumn columnRES_OVT_RATE;

			// Token: 0x04001AB6 RID: 6838
			private DataColumn columnRES_COST_PER_USE;
		}

		// Token: 0x02000571 RID: 1393
		[XmlSchemaProvider("GetTypedTableSchema")]
		[Serializable]
		public class ResourceAvailabilitiesDataTable : DataTable, IEnumerable
		{
			// Token: 0x0600847D RID: 33917 RVA: 0x001A06A0 File Offset: 0x0019E8A0
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public ResourceAvailabilitiesDataTable()
			{
				base.TableName = "ResourceAvailabilities";
				this.BeginInit();
				this.InitClass();
				this.EndInit();
			}

			// Token: 0x0600847E RID: 33918 RVA: 0x001A06C8 File Offset: 0x0019E8C8
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			internal ResourceAvailabilitiesDataTable(DataTable table)
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

			// Token: 0x0600847F RID: 33919 RVA: 0x001A0770 File Offset: 0x0019E970
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			protected ResourceAvailabilitiesDataTable(SerializationInfo info, StreamingContext context) : base(info, context)
			{
				this.InitVars();
			}

			// Token: 0x17002817 RID: 10263
			// (get) Token: 0x06008480 RID: 33920 RVA: 0x001A0780 File Offset: 0x0019E980
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			public DataColumn RES_UIDColumn
			{
				get
				{
					return this.columnRES_UID;
				}
			}

			// Token: 0x17002818 RID: 10264
			// (get) Token: 0x06008481 RID: 33921 RVA: 0x001A0788 File Offset: 0x0019E988
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			public DataColumn RES_AVAIL_FROMColumn
			{
				get
				{
					return this.columnRES_AVAIL_FROM;
				}
			}

			// Token: 0x17002819 RID: 10265
			// (get) Token: 0x06008482 RID: 33922 RVA: 0x001A0790 File Offset: 0x0019E990
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			public DataColumn RES_AVAIL_TOColumn
			{
				get
				{
					return this.columnRES_AVAIL_TO;
				}
			}

			// Token: 0x1700281A RID: 10266
			// (get) Token: 0x06008483 RID: 33923 RVA: 0x001A0798 File Offset: 0x0019E998
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			public DataColumn RES_AVAIL_UNITSColumn
			{
				get
				{
					return this.columnRES_AVAIL_UNITS;
				}
			}

			// Token: 0x1700281B RID: 10267
			// (get) Token: 0x06008484 RID: 33924 RVA: 0x001A07A0 File Offset: 0x0019E9A0
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

			// Token: 0x1700281C RID: 10268
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			public ResourceDataSet.ResourceAvailabilitiesRow this[int index]
			{
				get
				{
					return (ResourceDataSet.ResourceAvailabilitiesRow)base.Rows[index];
				}
			}

			// Token: 0x140004B1 RID: 1201
			// (add) Token: 0x06008486 RID: 33926 RVA: 0x001A07C0 File Offset: 0x0019E9C0
			// (remove) Token: 0x06008487 RID: 33927 RVA: 0x001A07F8 File Offset: 0x0019E9F8
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public event ResourceDataSet.ResourceAvailabilitiesRowChangeEventHandler ResourceAvailabilitiesRowChanging;

			// Token: 0x140004B2 RID: 1202
			// (add) Token: 0x06008488 RID: 33928 RVA: 0x001A0830 File Offset: 0x0019EA30
			// (remove) Token: 0x06008489 RID: 33929 RVA: 0x001A0868 File Offset: 0x0019EA68
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public event ResourceDataSet.ResourceAvailabilitiesRowChangeEventHandler ResourceAvailabilitiesRowChanged;

			// Token: 0x140004B3 RID: 1203
			// (add) Token: 0x0600848A RID: 33930 RVA: 0x001A08A0 File Offset: 0x0019EAA0
			// (remove) Token: 0x0600848B RID: 33931 RVA: 0x001A08D8 File Offset: 0x0019EAD8
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public event ResourceDataSet.ResourceAvailabilitiesRowChangeEventHandler ResourceAvailabilitiesRowDeleting;

			// Token: 0x140004B4 RID: 1204
			// (add) Token: 0x0600848C RID: 33932 RVA: 0x001A0910 File Offset: 0x0019EB10
			// (remove) Token: 0x0600848D RID: 33933 RVA: 0x001A0948 File Offset: 0x0019EB48
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public event ResourceDataSet.ResourceAvailabilitiesRowChangeEventHandler ResourceAvailabilitiesRowDeleted;

			// Token: 0x0600848E RID: 33934 RVA: 0x001A097D File Offset: 0x0019EB7D
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public void AddResourceAvailabilitiesRow(ResourceDataSet.ResourceAvailabilitiesRow row)
			{
				base.Rows.Add(row);
			}

			// Token: 0x0600848F RID: 33935 RVA: 0x001A098C File Offset: 0x0019EB8C
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public ResourceDataSet.ResourceAvailabilitiesRow AddResourceAvailabilitiesRow(ResourceDataSet.ResourcesRow parentResourcesRowByResourcesResourceAvailabilities, DateTime RES_AVAIL_FROM, DateTime RES_AVAIL_TO, double RES_AVAIL_UNITS)
			{
				ResourceDataSet.ResourceAvailabilitiesRow resourceAvailabilitiesRow = (ResourceDataSet.ResourceAvailabilitiesRow)base.NewRow();
				object[] array = new object[]
				{
					null,
					RES_AVAIL_FROM,
					RES_AVAIL_TO,
					RES_AVAIL_UNITS
				};
				if (parentResourcesRowByResourcesResourceAvailabilities != null)
				{
					array[0] = parentResourcesRowByResourcesResourceAvailabilities[0];
				}
				resourceAvailabilitiesRow.ItemArray = array;
				base.Rows.Add(resourceAvailabilitiesRow);
				return resourceAvailabilitiesRow;
			}

			// Token: 0x06008490 RID: 33936 RVA: 0x001A09EB File Offset: 0x0019EBEB
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public virtual IEnumerator GetEnumerator()
			{
				return base.Rows.GetEnumerator();
			}

			// Token: 0x06008491 RID: 33937 RVA: 0x001A09F8 File Offset: 0x0019EBF8
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public override DataTable Clone()
			{
				ResourceDataSet.ResourceAvailabilitiesDataTable resourceAvailabilitiesDataTable = (ResourceDataSet.ResourceAvailabilitiesDataTable)base.Clone();
				resourceAvailabilitiesDataTable.InitVars();
				return resourceAvailabilitiesDataTable;
			}

			// Token: 0x06008492 RID: 33938 RVA: 0x001A0A18 File Offset: 0x0019EC18
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			protected override DataTable CreateInstance()
			{
				return new ResourceDataSet.ResourceAvailabilitiesDataTable();
			}

			// Token: 0x06008493 RID: 33939 RVA: 0x001A0A20 File Offset: 0x0019EC20
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			internal void InitVars()
			{
				this.columnRES_UID = base.Columns["RES_UID"];
				this.columnRES_AVAIL_FROM = base.Columns["RES_AVAIL_FROM"];
				this.columnRES_AVAIL_TO = base.Columns["RES_AVAIL_TO"];
				this.columnRES_AVAIL_UNITS = base.Columns["RES_AVAIL_UNITS"];
			}

			// Token: 0x06008494 RID: 33940 RVA: 0x001A0A88 File Offset: 0x0019EC88
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			private void InitClass()
			{
				this.columnRES_UID = new DataColumn("RES_UID", typeof(Guid), null, MappingType.Element);
				base.Columns.Add(this.columnRES_UID);
				this.columnRES_AVAIL_FROM = new DataColumn("RES_AVAIL_FROM", typeof(DateTime), null, MappingType.Element);
				base.Columns.Add(this.columnRES_AVAIL_FROM);
				this.columnRES_AVAIL_TO = new DataColumn("RES_AVAIL_TO", typeof(DateTime), null, MappingType.Element);
				base.Columns.Add(this.columnRES_AVAIL_TO);
				this.columnRES_AVAIL_UNITS = new DataColumn("RES_AVAIL_UNITS", typeof(double), null, MappingType.Element);
				base.Columns.Add(this.columnRES_AVAIL_UNITS);
				this.columnRES_UID.AllowDBNull = false;
			}

			// Token: 0x06008495 RID: 33941 RVA: 0x001A0B55 File Offset: 0x0019ED55
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public ResourceDataSet.ResourceAvailabilitiesRow NewResourceAvailabilitiesRow()
			{
				return (ResourceDataSet.ResourceAvailabilitiesRow)base.NewRow();
			}

			// Token: 0x06008496 RID: 33942 RVA: 0x001A0B62 File Offset: 0x0019ED62
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			protected override DataRow NewRowFromBuilder(DataRowBuilder builder)
			{
				return new ResourceDataSet.ResourceAvailabilitiesRow(builder);
			}

			// Token: 0x06008497 RID: 33943 RVA: 0x001A0B6A File Offset: 0x0019ED6A
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			protected override Type GetRowType()
			{
				return typeof(ResourceDataSet.ResourceAvailabilitiesRow);
			}

			// Token: 0x06008498 RID: 33944 RVA: 0x001A0B76 File Offset: 0x0019ED76
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			protected override void OnRowChanged(DataRowChangeEventArgs e)
			{
				base.OnRowChanged(e);
				if (this.ResourceAvailabilitiesRowChanged != null)
				{
					this.ResourceAvailabilitiesRowChanged(this, new ResourceDataSet.ResourceAvailabilitiesRowChangeEvent((ResourceDataSet.ResourceAvailabilitiesRow)e.Row, e.Action));
				}
			}

			// Token: 0x06008499 RID: 33945 RVA: 0x001A0BA9 File Offset: 0x0019EDA9
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			protected override void OnRowChanging(DataRowChangeEventArgs e)
			{
				base.OnRowChanging(e);
				if (this.ResourceAvailabilitiesRowChanging != null)
				{
					this.ResourceAvailabilitiesRowChanging(this, new ResourceDataSet.ResourceAvailabilitiesRowChangeEvent((ResourceDataSet.ResourceAvailabilitiesRow)e.Row, e.Action));
				}
			}

			// Token: 0x0600849A RID: 33946 RVA: 0x001A0BDC File Offset: 0x0019EDDC
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			protected override void OnRowDeleted(DataRowChangeEventArgs e)
			{
				base.OnRowDeleted(e);
				if (this.ResourceAvailabilitiesRowDeleted != null)
				{
					this.ResourceAvailabilitiesRowDeleted(this, new ResourceDataSet.ResourceAvailabilitiesRowChangeEvent((ResourceDataSet.ResourceAvailabilitiesRow)e.Row, e.Action));
				}
			}

			// Token: 0x0600849B RID: 33947 RVA: 0x001A0C0F File Offset: 0x0019EE0F
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			protected override void OnRowDeleting(DataRowChangeEventArgs e)
			{
				base.OnRowDeleting(e);
				if (this.ResourceAvailabilitiesRowDeleting != null)
				{
					this.ResourceAvailabilitiesRowDeleting(this, new ResourceDataSet.ResourceAvailabilitiesRowChangeEvent((ResourceDataSet.ResourceAvailabilitiesRow)e.Row, e.Action));
				}
			}

			// Token: 0x0600849C RID: 33948 RVA: 0x001A0C42 File Offset: 0x0019EE42
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			public void RemoveResourceAvailabilitiesRow(ResourceDataSet.ResourceAvailabilitiesRow row)
			{
				base.Rows.Remove(row);
			}

			// Token: 0x0600849D RID: 33949 RVA: 0x001A0C50 File Offset: 0x0019EE50
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public static XmlSchemaComplexType GetTypedTableSchema(XmlSchemaSet xs)
			{
				XmlSchemaComplexType xmlSchemaComplexType = new XmlSchemaComplexType();
				XmlSchemaSequence xmlSchemaSequence = new XmlSchemaSequence();
				ResourceDataSet resourceDataSet = new ResourceDataSet();
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
				xmlSchemaAttribute.FixedValue = resourceDataSet.Namespace;
				xmlSchemaComplexType.Attributes.Add(xmlSchemaAttribute);
				XmlSchemaAttribute xmlSchemaAttribute2 = new XmlSchemaAttribute();
				xmlSchemaAttribute2.Name = "tableTypeName";
				xmlSchemaAttribute2.FixedValue = "ResourceAvailabilitiesDataTable";
				xmlSchemaComplexType.Attributes.Add(xmlSchemaAttribute2);
				xmlSchemaComplexType.Particle = xmlSchemaSequence;
				XmlSchema schemaSerializable = resourceDataSet.GetSchemaSerializable();
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

			// Token: 0x04001ABB RID: 6843
			private DataColumn columnRES_UID;

			// Token: 0x04001ABC RID: 6844
			private DataColumn columnRES_AVAIL_FROM;

			// Token: 0x04001ABD RID: 6845
			private DataColumn columnRES_AVAIL_TO;

			// Token: 0x04001ABE RID: 6846
			private DataColumn columnRES_AVAIL_UNITS;
		}

		// Token: 0x02000572 RID: 1394
		public class ResourcesRow : DataRow
		{
			// Token: 0x0600849E RID: 33950 RVA: 0x001A0E48 File Offset: 0x0019F048
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			internal ResourcesRow(DataRowBuilder rb) : base(rb)
			{
				this.tableResources = (ResourceDataSet.ResourcesDataTable)base.Table;
			}

			// Token: 0x1700281D RID: 10269
			// (get) Token: 0x0600849F RID: 33951 RVA: 0x001A0E62 File Offset: 0x0019F062
			// (set) Token: 0x060084A0 RID: 33952 RVA: 0x001A0E7A File Offset: 0x0019F07A
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			public Guid RES_UID
			{
				get
				{
					return (Guid)base[this.tableResources.RES_UIDColumn];
				}
				set
				{
					base[this.tableResources.RES_UIDColumn] = value;
				}
			}

			// Token: 0x1700281E RID: 10270
			// (get) Token: 0x060084A1 RID: 33953 RVA: 0x001A0E93 File Offset: 0x0019F093
			// (set) Token: 0x060084A2 RID: 33954 RVA: 0x001A0EAB File Offset: 0x0019F0AB
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			public int RES_ID
			{
				get
				{
					return (int)base[this.tableResources.RES_IDColumn];
				}
				set
				{
					base[this.tableResources.RES_IDColumn] = value;
				}
			}

			// Token: 0x1700281F RID: 10271
			// (get) Token: 0x060084A3 RID: 33955 RVA: 0x001A0EC4 File Offset: 0x0019F0C4
			// (set) Token: 0x060084A4 RID: 33956 RVA: 0x001A0EDC File Offset: 0x0019F0DC
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			public int RES_TYPE
			{
				get
				{
					return (int)base[this.tableResources.RES_TYPEColumn];
				}
				set
				{
					base[this.tableResources.RES_TYPEColumn] = value;
				}
			}

			// Token: 0x17002820 RID: 10272
			// (get) Token: 0x060084A5 RID: 33957 RVA: 0x001A0EF8 File Offset: 0x0019F0F8
			// (set) Token: 0x060084A6 RID: 33958 RVA: 0x001A0F3C File Offset: 0x0019F13C
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			public bool RES_HAS_NOTES
			{
				get
				{
					bool result;
					try
					{
						result = (bool)base[this.tableResources.RES_HAS_NOTESColumn];
					}
					catch (InvalidCastException innerException)
					{
						throw new StrongTypingException("The value for column 'RES_HAS_NOTES' in table 'Resources' is DBNull.", innerException);
					}
					return result;
				}
				set
				{
					base[this.tableResources.RES_HAS_NOTESColumn] = value;
				}
			}

			// Token: 0x17002821 RID: 10273
			// (get) Token: 0x060084A7 RID: 33959 RVA: 0x001A0F58 File Offset: 0x0019F158
			// (set) Token: 0x060084A8 RID: 33960 RVA: 0x001A0F9C File Offset: 0x0019F19C
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			public bool RES_CAN_LEVEL
			{
				get
				{
					bool result;
					try
					{
						result = (bool)base[this.tableResources.RES_CAN_LEVELColumn];
					}
					catch (InvalidCastException innerException)
					{
						throw new StrongTypingException("The value for column 'RES_CAN_LEVEL' in table 'Resources' is DBNull.", innerException);
					}
					return result;
				}
				set
				{
					base[this.tableResources.RES_CAN_LEVELColumn] = value;
				}
			}

			// Token: 0x17002822 RID: 10274
			// (get) Token: 0x060084A9 RID: 33961 RVA: 0x001A0FB8 File Offset: 0x0019F1B8
			// (set) Token: 0x060084AA RID: 33962 RVA: 0x001A0FFC File Offset: 0x0019F1FC
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			public short RES_ACCRUE_AT
			{
				get
				{
					short result;
					try
					{
						result = (short)base[this.tableResources.RES_ACCRUE_ATColumn];
					}
					catch (InvalidCastException innerException)
					{
						throw new StrongTypingException("The value for column 'RES_ACCRUE_AT' in table 'Resources' is DBNull.", innerException);
					}
					return result;
				}
				set
				{
					base[this.tableResources.RES_ACCRUE_ATColumn] = value;
				}
			}

			// Token: 0x17002823 RID: 10275
			// (get) Token: 0x060084AB RID: 33963 RVA: 0x001A1018 File Offset: 0x0019F218
			// (set) Token: 0x060084AC RID: 33964 RVA: 0x001A105C File Offset: 0x0019F25C
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			public int RES_BOOKING_TYPE
			{
				get
				{
					int result;
					try
					{
						result = (int)base[this.tableResources.RES_BOOKING_TYPEColumn];
					}
					catch (InvalidCastException innerException)
					{
						throw new StrongTypingException("The value for column 'RES_BOOKING_TYPE' in table 'Resources' is DBNull.", innerException);
					}
					return result;
				}
				set
				{
					base[this.tableResources.RES_BOOKING_TYPEColumn] = value;
				}
			}

			// Token: 0x17002824 RID: 10276
			// (get) Token: 0x060084AD RID: 33965 RVA: 0x001A1075 File Offset: 0x0019F275
			// (set) Token: 0x060084AE RID: 33966 RVA: 0x001A108D File Offset: 0x0019F28D
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			public string RES_NAME
			{
				get
				{
					return (string)base[this.tableResources.RES_NAMEColumn];
				}
				set
				{
					base[this.tableResources.RES_NAMEColumn] = value;
				}
			}

			// Token: 0x17002825 RID: 10277
			// (get) Token: 0x060084AF RID: 33967 RVA: 0x001A10A4 File Offset: 0x0019F2A4
			// (set) Token: 0x060084B0 RID: 33968 RVA: 0x001A10E8 File Offset: 0x0019F2E8
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			public string RES_INITIALS
			{
				get
				{
					string result;
					try
					{
						result = (string)base[this.tableResources.RES_INITIALSColumn];
					}
					catch (InvalidCastException innerException)
					{
						throw new StrongTypingException("The value for column 'RES_INITIALS' in table 'Resources' is DBNull.", innerException);
					}
					return result;
				}
				set
				{
					base[this.tableResources.RES_INITIALSColumn] = value;
				}
			}

			// Token: 0x17002826 RID: 10278
			// (get) Token: 0x060084B1 RID: 33969 RVA: 0x001A10FC File Offset: 0x0019F2FC
			// (set) Token: 0x060084B2 RID: 33970 RVA: 0x001A1140 File Offset: 0x0019F340
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			public string RES_PHONETICS
			{
				get
				{
					string result;
					try
					{
						result = (string)base[this.tableResources.RES_PHONETICSColumn];
					}
					catch (InvalidCastException innerException)
					{
						throw new StrongTypingException("The value for column 'RES_PHONETICS' in table 'Resources' is DBNull.", innerException);
					}
					return result;
				}
				set
				{
					base[this.tableResources.RES_PHONETICSColumn] = value;
				}
			}

			// Token: 0x17002827 RID: 10279
			// (get) Token: 0x060084B3 RID: 33971 RVA: 0x001A1154 File Offset: 0x0019F354
			// (set) Token: 0x060084B4 RID: 33972 RVA: 0x001A1198 File Offset: 0x0019F398
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			public string RES_MATERIAL_LABEL
			{
				get
				{
					string result;
					try
					{
						result = (string)base[this.tableResources.RES_MATERIAL_LABELColumn];
					}
					catch (InvalidCastException innerException)
					{
						throw new StrongTypingException("The value for column 'RES_MATERIAL_LABEL' in table 'Resources' is DBNull.", innerException);
					}
					return result;
				}
				set
				{
					base[this.tableResources.RES_MATERIAL_LABELColumn] = value;
				}
			}

			// Token: 0x17002828 RID: 10280
			// (get) Token: 0x060084B5 RID: 33973 RVA: 0x001A11AC File Offset: 0x0019F3AC
			// (set) Token: 0x060084B6 RID: 33974 RVA: 0x001A11F0 File Offset: 0x0019F3F0
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			public byte[] RES_RTF_NOTES
			{
				get
				{
					byte[] result;
					try
					{
						result = (byte[])base[this.tableResources.RES_RTF_NOTESColumn];
					}
					catch (InvalidCastException innerException)
					{
						throw new StrongTypingException("The value for column 'RES_RTF_NOTES' in table 'Resources' is DBNull.", innerException);
					}
					return result;
				}
				set
				{
					base[this.tableResources.RES_RTF_NOTESColumn] = value;
				}
			}

			// Token: 0x17002829 RID: 10281
			// (get) Token: 0x060084B7 RID: 33975 RVA: 0x001A1204 File Offset: 0x0019F404
			// (set) Token: 0x060084B8 RID: 33976 RVA: 0x001A1248 File Offset: 0x0019F448
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			public string WRES_ACCOUNT
			{
				get
				{
					string result;
					try
					{
						result = (string)base[this.tableResources.WRES_ACCOUNTColumn];
					}
					catch (InvalidCastException innerException)
					{
						throw new StrongTypingException("The value for column 'WRES_ACCOUNT' in table 'Resources' is DBNull.", innerException);
					}
					return result;
				}
				set
				{
					base[this.tableResources.WRES_ACCOUNTColumn] = value;
				}
			}

			// Token: 0x1700282A RID: 10282
			// (get) Token: 0x060084B9 RID: 33977 RVA: 0x001A125C File Offset: 0x0019F45C
			// (set) Token: 0x060084BA RID: 33978 RVA: 0x001A12A0 File Offset: 0x0019F4A0
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			public bool RES_IS_WINDOWS_USER
			{
				get
				{
					bool result;
					try
					{
						result = (bool)base[this.tableResources.RES_IS_WINDOWS_USERColumn];
					}
					catch (InvalidCastException innerException)
					{
						throw new StrongTypingException("The value for column 'RES_IS_WINDOWS_USER' in table 'Resources' is DBNull.", innerException);
					}
					return result;
				}
				set
				{
					base[this.tableResources.RES_IS_WINDOWS_USERColumn] = value;
				}
			}

			// Token: 0x1700282B RID: 10283
			// (get) Token: 0x060084BB RID: 33979 RVA: 0x001A12BC File Offset: 0x0019F4BC
			// (set) Token: 0x060084BC RID: 33980 RVA: 0x001A1300 File Offset: 0x0019F500
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			public string WRES_EMAIL
			{
				get
				{
					string result;
					try
					{
						result = (string)base[this.tableResources.WRES_EMAILColumn];
					}
					catch (InvalidCastException innerException)
					{
						throw new StrongTypingException("The value for column 'WRES_EMAIL' in table 'Resources' is DBNull.", innerException);
					}
					return result;
				}
				set
				{
					base[this.tableResources.WRES_EMAILColumn] = value;
				}
			}

			// Token: 0x1700282C RID: 10284
			// (get) Token: 0x060084BD RID: 33981 RVA: 0x001A1314 File Offset: 0x0019F514
			// (set) Token: 0x060084BE RID: 33982 RVA: 0x001A1358 File Offset: 0x0019F558
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			public int WRES_EMAIL_LANGUAGE
			{
				get
				{
					int result;
					try
					{
						result = (int)base[this.tableResources.WRES_EMAIL_LANGUAGEColumn];
					}
					catch (InvalidCastException innerException)
					{
						throw new StrongTypingException("The value for column 'WRES_EMAIL_LANGUAGE' in table 'Resources' is DBNull.", innerException);
					}
					return result;
				}
				set
				{
					base[this.tableResources.WRES_EMAIL_LANGUAGEColumn] = value;
				}
			}

			// Token: 0x1700282D RID: 10285
			// (get) Token: 0x060084BF RID: 33983 RVA: 0x001A1374 File Offset: 0x0019F574
			// (set) Token: 0x060084C0 RID: 33984 RVA: 0x001A13B8 File Offset: 0x0019F5B8
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			public Guid RES_CHECKOUTBY
			{
				get
				{
					Guid result;
					try
					{
						result = (Guid)base[this.tableResources.RES_CHECKOUTBYColumn];
					}
					catch (InvalidCastException innerException)
					{
						throw new StrongTypingException("The value for column 'RES_CHECKOUTBY' in table 'Resources' is DBNull.", innerException);
					}
					return result;
				}
				set
				{
					base[this.tableResources.RES_CHECKOUTBYColumn] = value;
				}
			}

			// Token: 0x1700282E RID: 10286
			// (get) Token: 0x060084C1 RID: 33985 RVA: 0x001A13D4 File Offset: 0x0019F5D4
			// (set) Token: 0x060084C2 RID: 33986 RVA: 0x001A1418 File Offset: 0x0019F618
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			public DateTime RES_CHECKOUTDATE
			{
				get
				{
					DateTime result;
					try
					{
						result = (DateTime)base[this.tableResources.RES_CHECKOUTDATEColumn];
					}
					catch (InvalidCastException innerException)
					{
						throw new StrongTypingException("The value for column 'RES_CHECKOUTDATE' in table 'Resources' is DBNull.", innerException);
					}
					return result;
				}
				set
				{
					base[this.tableResources.RES_CHECKOUTDATEColumn] = value;
				}
			}

			// Token: 0x1700282F RID: 10287
			// (get) Token: 0x060084C3 RID: 33987 RVA: 0x001A1434 File Offset: 0x0019F634
			// (set) Token: 0x060084C4 RID: 33988 RVA: 0x001A1478 File Offset: 0x0019F678
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			public string RES_HYPERLINK_FRIENDLY_NAME
			{
				get
				{
					string result;
					try
					{
						result = (string)base[this.tableResources.RES_HYPERLINK_FRIENDLY_NAMEColumn];
					}
					catch (InvalidCastException innerException)
					{
						throw new StrongTypingException("The value for column 'RES_HYPERLINK_FRIENDLY_NAME' in table 'Resources' is DBNull.", innerException);
					}
					return result;
				}
				set
				{
					base[this.tableResources.RES_HYPERLINK_FRIENDLY_NAMEColumn] = value;
				}
			}

			// Token: 0x17002830 RID: 10288
			// (get) Token: 0x060084C5 RID: 33989 RVA: 0x001A148C File Offset: 0x0019F68C
			// (set) Token: 0x060084C6 RID: 33990 RVA: 0x001A14D0 File Offset: 0x0019F6D0
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			public string RES_HYPERLINK_ADDRESS
			{
				get
				{
					string result;
					try
					{
						result = (string)base[this.tableResources.RES_HYPERLINK_ADDRESSColumn];
					}
					catch (InvalidCastException innerException)
					{
						throw new StrongTypingException("The value for column 'RES_HYPERLINK_ADDRESS' in table 'Resources' is DBNull.", innerException);
					}
					return result;
				}
				set
				{
					base[this.tableResources.RES_HYPERLINK_ADDRESSColumn] = value;
				}
			}

			// Token: 0x17002831 RID: 10289
			// (get) Token: 0x060084C7 RID: 33991 RVA: 0x001A14E4 File Offset: 0x0019F6E4
			// (set) Token: 0x060084C8 RID: 33992 RVA: 0x001A1528 File Offset: 0x0019F728
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			public string RES_HYPERLINK_SUB_ADDRESS
			{
				get
				{
					string result;
					try
					{
						result = (string)base[this.tableResources.RES_HYPERLINK_SUB_ADDRESSColumn];
					}
					catch (InvalidCastException innerException)
					{
						throw new StrongTypingException("The value for column 'RES_HYPERLINK_SUB_ADDRESS' in table 'Resources' is DBNull.", innerException);
					}
					return result;
				}
				set
				{
					base[this.tableResources.RES_HYPERLINK_SUB_ADDRESSColumn] = value;
				}
			}

			// Token: 0x17002832 RID: 10290
			// (get) Token: 0x060084C9 RID: 33993 RVA: 0x001A153C File Offset: 0x0019F73C
			// (set) Token: 0x060084CA RID: 33994 RVA: 0x001A1580 File Offset: 0x0019F780
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			public string RES_CODE
			{
				get
				{
					string result;
					try
					{
						result = (string)base[this.tableResources.RES_CODEColumn];
					}
					catch (InvalidCastException innerException)
					{
						throw new StrongTypingException("The value for column 'RES_CODE' in table 'Resources' is DBNull.", innerException);
					}
					return result;
				}
				set
				{
					base[this.tableResources.RES_CODEColumn] = value;
				}
			}

			// Token: 0x17002833 RID: 10291
			// (get) Token: 0x060084CB RID: 33995 RVA: 0x001A1594 File Offset: 0x0019F794
			// (set) Token: 0x060084CC RID: 33996 RVA: 0x001A15D8 File Offset: 0x0019F7D8
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			public string RES_GROUP
			{
				get
				{
					string result;
					try
					{
						result = (string)base[this.tableResources.RES_GROUPColumn];
					}
					catch (InvalidCastException innerException)
					{
						throw new StrongTypingException("The value for column 'RES_GROUP' in table 'Resources' is DBNull.", innerException);
					}
					return result;
				}
				set
				{
					base[this.tableResources.RES_GROUPColumn] = value;
				}
			}

			// Token: 0x17002834 RID: 10292
			// (get) Token: 0x060084CD RID: 33997 RVA: 0x001A15EC File Offset: 0x0019F7EC
			// (set) Token: 0x060084CE RID: 33998 RVA: 0x001A1630 File Offset: 0x0019F830
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			public string RES_EXTERNAL_ID
			{
				get
				{
					string result;
					try
					{
						result = (string)base[this.tableResources.RES_EXTERNAL_IDColumn];
					}
					catch (InvalidCastException innerException)
					{
						throw new StrongTypingException("The value for column 'RES_EXTERNAL_ID' in table 'Resources' is DBNull.", innerException);
					}
					return result;
				}
				set
				{
					base[this.tableResources.RES_EXTERNAL_IDColumn] = value;
				}
			}

			// Token: 0x17002835 RID: 10293
			// (get) Token: 0x060084CF RID: 33999 RVA: 0x001A1644 File Offset: 0x0019F844
			// (set) Token: 0x060084D0 RID: 34000 RVA: 0x001A1688 File Offset: 0x0019F888
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			public Guid RES_TIMESHEET_MGR_UID
			{
				get
				{
					Guid result;
					try
					{
						result = (Guid)base[this.tableResources.RES_TIMESHEET_MGR_UIDColumn];
					}
					catch (InvalidCastException innerException)
					{
						throw new StrongTypingException("The value for column 'RES_TIMESHEET_MGR_UID' in table 'Resources' is DBNull.", innerException);
					}
					return result;
				}
				set
				{
					base[this.tableResources.RES_TIMESHEET_MGR_UIDColumn] = value;
				}
			}

			// Token: 0x17002836 RID: 10294
			// (get) Token: 0x060084D1 RID: 34001 RVA: 0x001A16A4 File Offset: 0x0019F8A4
			// (set) Token: 0x060084D2 RID: 34002 RVA: 0x001A16E8 File Offset: 0x0019F8E8
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			public Guid RES_DEF_ASSN_OWNER
			{
				get
				{
					Guid result;
					try
					{
						result = (Guid)base[this.tableResources.RES_DEF_ASSN_OWNERColumn];
					}
					catch (InvalidCastException innerException)
					{
						throw new StrongTypingException("The value for column 'RES_DEF_ASSN_OWNER' in table 'Resources' is DBNull.", innerException);
					}
					return result;
				}
				set
				{
					base[this.tableResources.RES_DEF_ASSN_OWNERColumn] = value;
				}
			}

			// Token: 0x17002837 RID: 10295
			// (get) Token: 0x060084D3 RID: 34003 RVA: 0x001A1704 File Offset: 0x0019F904
			// (set) Token: 0x060084D4 RID: 34004 RVA: 0x001A1748 File Offset: 0x0019F948
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			public DateTime RES_HIRE_DATE
			{
				get
				{
					DateTime result;
					try
					{
						result = (DateTime)base[this.tableResources.RES_HIRE_DATEColumn];
					}
					catch (InvalidCastException innerException)
					{
						throw new StrongTypingException("The value for column 'RES_HIRE_DATE' in table 'Resources' is DBNull.", innerException);
					}
					return result;
				}
				set
				{
					base[this.tableResources.RES_HIRE_DATEColumn] = value;
				}
			}

			// Token: 0x17002838 RID: 10296
			// (get) Token: 0x060084D5 RID: 34005 RVA: 0x001A1764 File Offset: 0x0019F964
			// (set) Token: 0x060084D6 RID: 34006 RVA: 0x001A17A8 File Offset: 0x0019F9A8
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			public DateTime RES_TERMINATION_DATE
			{
				get
				{
					DateTime result;
					try
					{
						result = (DateTime)base[this.tableResources.RES_TERMINATION_DATEColumn];
					}
					catch (InvalidCastException innerException)
					{
						throw new StrongTypingException("The value for column 'RES_TERMINATION_DATE' in table 'Resources' is DBNull.", innerException);
					}
					return result;
				}
				set
				{
					base[this.tableResources.RES_TERMINATION_DATEColumn] = value;
				}
			}

			// Token: 0x17002839 RID: 10297
			// (get) Token: 0x060084D7 RID: 34007 RVA: 0x001A17C4 File Offset: 0x0019F9C4
			// (set) Token: 0x060084D8 RID: 34008 RVA: 0x001A1808 File Offset: 0x0019FA08
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			public bool RES_IS_TEAM
			{
				get
				{
					bool result;
					try
					{
						result = (bool)base[this.tableResources.RES_IS_TEAMColumn];
					}
					catch (InvalidCastException innerException)
					{
						throw new StrongTypingException("The value for column 'RES_IS_TEAM' in table 'Resources' is DBNull.", innerException);
					}
					return result;
				}
				set
				{
					base[this.tableResources.RES_IS_TEAMColumn] = value;
				}
			}

			// Token: 0x1700283A RID: 10298
			// (get) Token: 0x060084D9 RID: 34009 RVA: 0x001A1824 File Offset: 0x0019FA24
			// (set) Token: 0x060084DA RID: 34010 RVA: 0x001A1868 File Offset: 0x0019FA68
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			public bool RES_EXCHANGE_SYNC
			{
				get
				{
					bool result;
					try
					{
						result = (bool)base[this.tableResources.RES_EXCHANGE_SYNCColumn];
					}
					catch (InvalidCastException innerException)
					{
						throw new StrongTypingException("The value for column 'RES_EXCHANGE_SYNC' in table 'Resources' is DBNull.", innerException);
					}
					return result;
				}
				set
				{
					base[this.tableResources.RES_EXCHANGE_SYNCColumn] = value;
				}
			}

			// Token: 0x1700283B RID: 10299
			// (get) Token: 0x060084DB RID: 34011 RVA: 0x001A1884 File Offset: 0x0019FA84
			// (set) Token: 0x060084DC RID: 34012 RVA: 0x001A18C8 File Offset: 0x0019FAC8
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			public string RES_EXCHANGE_EWS_URL
			{
				get
				{
					string result;
					try
					{
						result = (string)base[this.tableResources.RES_EXCHANGE_EWS_URLColumn];
					}
					catch (InvalidCastException innerException)
					{
						throw new StrongTypingException("The value for column 'RES_EXCHANGE_EWS_URL' in table 'Resources' is DBNull.", innerException);
					}
					return result;
				}
				set
				{
					base[this.tableResources.RES_EXCHANGE_EWS_URLColumn] = value;
				}
			}

			// Token: 0x1700283C RID: 10300
			// (get) Token: 0x060084DD RID: 34013 RVA: 0x001A18DC File Offset: 0x0019FADC
			// (set) Token: 0x060084DE RID: 34014 RVA: 0x001A1920 File Offset: 0x0019FB20
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			public bool RES_CAL_OOF_EXCHANGE_SYNC
			{
				get
				{
					bool result;
					try
					{
						result = (bool)base[this.tableResources.RES_CAL_OOF_EXCHANGE_SYNCColumn];
					}
					catch (InvalidCastException innerException)
					{
						throw new StrongTypingException("The value for column 'RES_CAL_OOF_EXCHANGE_SYNC' in table 'Resources' is DBNull.", innerException);
					}
					return result;
				}
				set
				{
					base[this.tableResources.RES_CAL_OOF_EXCHANGE_SYNCColumn] = value;
				}
			}

			// Token: 0x1700283D RID: 10301
			// (get) Token: 0x060084DF RID: 34015 RVA: 0x001A193C File Offset: 0x0019FB3C
			// (set) Token: 0x060084E0 RID: 34016 RVA: 0x001A1980 File Offset: 0x0019FB80
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			public string RES_COST_CENTER
			{
				get
				{
					string result;
					try
					{
						result = (string)base[this.tableResources.RES_COST_CENTERColumn];
					}
					catch (InvalidCastException innerException)
					{
						throw new StrongTypingException("The value for column 'RES_COST_CENTER' in table 'Resources' is DBNull.", innerException);
					}
					return result;
				}
				set
				{
					base[this.tableResources.RES_COST_CENTERColumn] = value;
				}
			}

			// Token: 0x1700283E RID: 10302
			// (get) Token: 0x060084E1 RID: 34017 RVA: 0x001A1994 File Offset: 0x0019FB94
			// (set) Token: 0x060084E2 RID: 34018 RVA: 0x001A19D8 File Offset: 0x0019FBD8
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			public string RES_NOTES
			{
				get
				{
					string result;
					try
					{
						result = (string)base[this.tableResources.RES_NOTESColumn];
					}
					catch (InvalidCastException innerException)
					{
						throw new StrongTypingException("The value for column 'RES_NOTES' in table 'Resources' is DBNull.", innerException);
					}
					return result;
				}
				set
				{
					base[this.tableResources.RES_NOTESColumn] = value;
				}
			}

			// Token: 0x1700283F RID: 10303
			// (get) Token: 0x060084E3 RID: 34019 RVA: 0x001A19EC File Offset: 0x0019FBEC
			// (set) Token: 0x060084E4 RID: 34020 RVA: 0x001A1A30 File Offset: 0x0019FC30
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			public Guid BaseCalendarUniqueId
			{
				get
				{
					Guid result;
					try
					{
						result = (Guid)base[this.tableResources.BaseCalendarUniqueIdColumn];
					}
					catch (InvalidCastException innerException)
					{
						throw new StrongTypingException("The value for column 'BaseCalendarUniqueId' in table 'Resources' is DBNull.", innerException);
					}
					return result;
				}
				set
				{
					base[this.tableResources.BaseCalendarUniqueIdColumn] = value;
				}
			}

			// Token: 0x17002840 RID: 10304
			// (get) Token: 0x060084E5 RID: 34021 RVA: 0x001A1A4C File Offset: 0x0019FC4C
			// (set) Token: 0x060084E6 RID: 34022 RVA: 0x001A1A90 File Offset: 0x0019FC90
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			public bool RES_REQUIRES_ENGAGEMENTS
			{
				get
				{
					bool result;
					try
					{
						result = (bool)base[this.tableResources.RES_REQUIRES_ENGAGEMENTSColumn];
					}
					catch (InvalidCastException innerException)
					{
						throw new StrongTypingException("The value for column 'RES_REQUIRES_ENGAGEMENTS' in table 'Resources' is DBNull.", innerException);
					}
					return result;
				}
				set
				{
					base[this.tableResources.RES_REQUIRES_ENGAGEMENTSColumn] = value;
				}
			}

			// Token: 0x17002841 RID: 10305
			// (get) Token: 0x060084E7 RID: 34023 RVA: 0x001A1AAC File Offset: 0x0019FCAC
			// (set) Token: 0x060084E8 RID: 34024 RVA: 0x001A1AF0 File Offset: 0x0019FCF0
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			public DateTime CREATED_DATE
			{
				get
				{
					DateTime result;
					try
					{
						result = (DateTime)base[this.tableResources.CREATED_DATEColumn];
					}
					catch (InvalidCastException innerException)
					{
						throw new StrongTypingException("The value for column 'CREATED_DATE' in table 'Resources' is DBNull.", innerException);
					}
					return result;
				}
				set
				{
					base[this.tableResources.CREATED_DATEColumn] = value;
				}
			}

			// Token: 0x17002842 RID: 10306
			// (get) Token: 0x060084E9 RID: 34025 RVA: 0x001A1B0C File Offset: 0x0019FD0C
			// (set) Token: 0x060084EA RID: 34026 RVA: 0x001A1B50 File Offset: 0x0019FD50
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public DateTime MOD_DATE
			{
				get
				{
					DateTime result;
					try
					{
						result = (DateTime)base[this.tableResources.MOD_DATEColumn];
					}
					catch (InvalidCastException innerException)
					{
						throw new StrongTypingException("The value for column 'MOD_DATE' in table 'Resources' is DBNull.", innerException);
					}
					return result;
				}
				set
				{
					base[this.tableResources.MOD_DATEColumn] = value;
				}
			}

			// Token: 0x060084EB RID: 34027 RVA: 0x001A1B69 File Offset: 0x0019FD69
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public bool IsRES_HAS_NOTESNull()
			{
				return base.IsNull(this.tableResources.RES_HAS_NOTESColumn);
			}

			// Token: 0x060084EC RID: 34028 RVA: 0x001A1B7C File Offset: 0x0019FD7C
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			public void SetRES_HAS_NOTESNull()
			{
				base[this.tableResources.RES_HAS_NOTESColumn] = Convert.DBNull;
			}

			// Token: 0x060084ED RID: 34029 RVA: 0x001A1B94 File Offset: 0x0019FD94
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			public bool IsRES_CAN_LEVELNull()
			{
				return base.IsNull(this.tableResources.RES_CAN_LEVELColumn);
			}

			// Token: 0x060084EE RID: 34030 RVA: 0x001A1BA7 File Offset: 0x0019FDA7
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public void SetRES_CAN_LEVELNull()
			{
				base[this.tableResources.RES_CAN_LEVELColumn] = Convert.DBNull;
			}

			// Token: 0x060084EF RID: 34031 RVA: 0x001A1BBF File Offset: 0x0019FDBF
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public bool IsRES_ACCRUE_ATNull()
			{
				return base.IsNull(this.tableResources.RES_ACCRUE_ATColumn);
			}

			// Token: 0x060084F0 RID: 34032 RVA: 0x001A1BD2 File Offset: 0x0019FDD2
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public void SetRES_ACCRUE_ATNull()
			{
				base[this.tableResources.RES_ACCRUE_ATColumn] = Convert.DBNull;
			}

			// Token: 0x060084F1 RID: 34033 RVA: 0x001A1BEA File Offset: 0x0019FDEA
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public bool IsRES_BOOKING_TYPENull()
			{
				return base.IsNull(this.tableResources.RES_BOOKING_TYPEColumn);
			}

			// Token: 0x060084F2 RID: 34034 RVA: 0x001A1BFD File Offset: 0x0019FDFD
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			public void SetRES_BOOKING_TYPENull()
			{
				base[this.tableResources.RES_BOOKING_TYPEColumn] = Convert.DBNull;
			}

			// Token: 0x060084F3 RID: 34035 RVA: 0x001A1C15 File Offset: 0x0019FE15
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public bool IsRES_INITIALSNull()
			{
				return base.IsNull(this.tableResources.RES_INITIALSColumn);
			}

			// Token: 0x060084F4 RID: 34036 RVA: 0x001A1C28 File Offset: 0x0019FE28
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public void SetRES_INITIALSNull()
			{
				base[this.tableResources.RES_INITIALSColumn] = Convert.DBNull;
			}

			// Token: 0x060084F5 RID: 34037 RVA: 0x001A1C40 File Offset: 0x0019FE40
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			public bool IsRES_PHONETICSNull()
			{
				return base.IsNull(this.tableResources.RES_PHONETICSColumn);
			}

			// Token: 0x060084F6 RID: 34038 RVA: 0x001A1C53 File Offset: 0x0019FE53
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public void SetRES_PHONETICSNull()
			{
				base[this.tableResources.RES_PHONETICSColumn] = Convert.DBNull;
			}

			// Token: 0x060084F7 RID: 34039 RVA: 0x001A1C6B File Offset: 0x0019FE6B
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public bool IsRES_MATERIAL_LABELNull()
			{
				return base.IsNull(this.tableResources.RES_MATERIAL_LABELColumn);
			}

			// Token: 0x060084F8 RID: 34040 RVA: 0x001A1C7E File Offset: 0x0019FE7E
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			public void SetRES_MATERIAL_LABELNull()
			{
				base[this.tableResources.RES_MATERIAL_LABELColumn] = Convert.DBNull;
			}

			// Token: 0x060084F9 RID: 34041 RVA: 0x001A1C96 File Offset: 0x0019FE96
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			public bool IsRES_RTF_NOTESNull()
			{
				return base.IsNull(this.tableResources.RES_RTF_NOTESColumn);
			}

			// Token: 0x060084FA RID: 34042 RVA: 0x001A1CA9 File Offset: 0x0019FEA9
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public void SetRES_RTF_NOTESNull()
			{
				base[this.tableResources.RES_RTF_NOTESColumn] = Convert.DBNull;
			}

			// Token: 0x060084FB RID: 34043 RVA: 0x001A1CC1 File Offset: 0x0019FEC1
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public bool IsWRES_ACCOUNTNull()
			{
				return base.IsNull(this.tableResources.WRES_ACCOUNTColumn);
			}

			// Token: 0x060084FC RID: 34044 RVA: 0x001A1CD4 File Offset: 0x0019FED4
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public void SetWRES_ACCOUNTNull()
			{
				base[this.tableResources.WRES_ACCOUNTColumn] = Convert.DBNull;
			}

			// Token: 0x060084FD RID: 34045 RVA: 0x001A1CEC File Offset: 0x0019FEEC
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			public bool IsRES_IS_WINDOWS_USERNull()
			{
				return base.IsNull(this.tableResources.RES_IS_WINDOWS_USERColumn);
			}

			// Token: 0x060084FE RID: 34046 RVA: 0x001A1CFF File Offset: 0x0019FEFF
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public void SetRES_IS_WINDOWS_USERNull()
			{
				base[this.tableResources.RES_IS_WINDOWS_USERColumn] = Convert.DBNull;
			}

			// Token: 0x060084FF RID: 34047 RVA: 0x001A1D17 File Offset: 0x0019FF17
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			public bool IsWRES_EMAILNull()
			{
				return base.IsNull(this.tableResources.WRES_EMAILColumn);
			}

			// Token: 0x06008500 RID: 34048 RVA: 0x001A1D2A File Offset: 0x0019FF2A
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public void SetWRES_EMAILNull()
			{
				base[this.tableResources.WRES_EMAILColumn] = Convert.DBNull;
			}

			// Token: 0x06008501 RID: 34049 RVA: 0x001A1D42 File Offset: 0x0019FF42
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			public bool IsWRES_EMAIL_LANGUAGENull()
			{
				return base.IsNull(this.tableResources.WRES_EMAIL_LANGUAGEColumn);
			}

			// Token: 0x06008502 RID: 34050 RVA: 0x001A1D55 File Offset: 0x0019FF55
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public void SetWRES_EMAIL_LANGUAGENull()
			{
				base[this.tableResources.WRES_EMAIL_LANGUAGEColumn] = Convert.DBNull;
			}

			// Token: 0x06008503 RID: 34051 RVA: 0x001A1D6D File Offset: 0x0019FF6D
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			public bool IsRES_CHECKOUTBYNull()
			{
				return base.IsNull(this.tableResources.RES_CHECKOUTBYColumn);
			}

			// Token: 0x06008504 RID: 34052 RVA: 0x001A1D80 File Offset: 0x0019FF80
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public void SetRES_CHECKOUTBYNull()
			{
				base[this.tableResources.RES_CHECKOUTBYColumn] = Convert.DBNull;
			}

			// Token: 0x06008505 RID: 34053 RVA: 0x001A1D98 File Offset: 0x0019FF98
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public bool IsRES_CHECKOUTDATENull()
			{
				return base.IsNull(this.tableResources.RES_CHECKOUTDATEColumn);
			}

			// Token: 0x06008506 RID: 34054 RVA: 0x001A1DAB File Offset: 0x0019FFAB
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			public void SetRES_CHECKOUTDATENull()
			{
				base[this.tableResources.RES_CHECKOUTDATEColumn] = Convert.DBNull;
			}

			// Token: 0x06008507 RID: 34055 RVA: 0x001A1DC3 File Offset: 0x0019FFC3
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			public bool IsRES_HYPERLINK_FRIENDLY_NAMENull()
			{
				return base.IsNull(this.tableResources.RES_HYPERLINK_FRIENDLY_NAMEColumn);
			}

			// Token: 0x06008508 RID: 34056 RVA: 0x001A1DD6 File Offset: 0x0019FFD6
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public void SetRES_HYPERLINK_FRIENDLY_NAMENull()
			{
				base[this.tableResources.RES_HYPERLINK_FRIENDLY_NAMEColumn] = Convert.DBNull;
			}

			// Token: 0x06008509 RID: 34057 RVA: 0x001A1DEE File Offset: 0x0019FFEE
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			public bool IsRES_HYPERLINK_ADDRESSNull()
			{
				return base.IsNull(this.tableResources.RES_HYPERLINK_ADDRESSColumn);
			}

			// Token: 0x0600850A RID: 34058 RVA: 0x001A1E01 File Offset: 0x001A0001
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public void SetRES_HYPERLINK_ADDRESSNull()
			{
				base[this.tableResources.RES_HYPERLINK_ADDRESSColumn] = Convert.DBNull;
			}

			// Token: 0x0600850B RID: 34059 RVA: 0x001A1E19 File Offset: 0x001A0019
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			public bool IsRES_HYPERLINK_SUB_ADDRESSNull()
			{
				return base.IsNull(this.tableResources.RES_HYPERLINK_SUB_ADDRESSColumn);
			}

			// Token: 0x0600850C RID: 34060 RVA: 0x001A1E2C File Offset: 0x001A002C
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public void SetRES_HYPERLINK_SUB_ADDRESSNull()
			{
				base[this.tableResources.RES_HYPERLINK_SUB_ADDRESSColumn] = Convert.DBNull;
			}

			// Token: 0x0600850D RID: 34061 RVA: 0x001A1E44 File Offset: 0x001A0044
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			public bool IsRES_CODENull()
			{
				return base.IsNull(this.tableResources.RES_CODEColumn);
			}

			// Token: 0x0600850E RID: 34062 RVA: 0x001A1E57 File Offset: 0x001A0057
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			public void SetRES_CODENull()
			{
				base[this.tableResources.RES_CODEColumn] = Convert.DBNull;
			}

			// Token: 0x0600850F RID: 34063 RVA: 0x001A1E6F File Offset: 0x001A006F
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public bool IsRES_GROUPNull()
			{
				return base.IsNull(this.tableResources.RES_GROUPColumn);
			}

			// Token: 0x06008510 RID: 34064 RVA: 0x001A1E82 File Offset: 0x001A0082
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public void SetRES_GROUPNull()
			{
				base[this.tableResources.RES_GROUPColumn] = Convert.DBNull;
			}

			// Token: 0x06008511 RID: 34065 RVA: 0x001A1E9A File Offset: 0x001A009A
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public bool IsRES_EXTERNAL_IDNull()
			{
				return base.IsNull(this.tableResources.RES_EXTERNAL_IDColumn);
			}

			// Token: 0x06008512 RID: 34066 RVA: 0x001A1EAD File Offset: 0x001A00AD
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public void SetRES_EXTERNAL_IDNull()
			{
				base[this.tableResources.RES_EXTERNAL_IDColumn] = Convert.DBNull;
			}

			// Token: 0x06008513 RID: 34067 RVA: 0x001A1EC5 File Offset: 0x001A00C5
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			public bool IsRES_TIMESHEET_MGR_UIDNull()
			{
				return base.IsNull(this.tableResources.RES_TIMESHEET_MGR_UIDColumn);
			}

			// Token: 0x06008514 RID: 34068 RVA: 0x001A1ED8 File Offset: 0x001A00D8
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public void SetRES_TIMESHEET_MGR_UIDNull()
			{
				base[this.tableResources.RES_TIMESHEET_MGR_UIDColumn] = Convert.DBNull;
			}

			// Token: 0x06008515 RID: 34069 RVA: 0x001A1EF0 File Offset: 0x001A00F0
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public bool IsRES_DEF_ASSN_OWNERNull()
			{
				return base.IsNull(this.tableResources.RES_DEF_ASSN_OWNERColumn);
			}

			// Token: 0x06008516 RID: 34070 RVA: 0x001A1F03 File Offset: 0x001A0103
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			public void SetRES_DEF_ASSN_OWNERNull()
			{
				base[this.tableResources.RES_DEF_ASSN_OWNERColumn] = Convert.DBNull;
			}

			// Token: 0x06008517 RID: 34071 RVA: 0x001A1F1B File Offset: 0x001A011B
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			public bool IsRES_HIRE_DATENull()
			{
				return base.IsNull(this.tableResources.RES_HIRE_DATEColumn);
			}

			// Token: 0x06008518 RID: 34072 RVA: 0x001A1F2E File Offset: 0x001A012E
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public void SetRES_HIRE_DATENull()
			{
				base[this.tableResources.RES_HIRE_DATEColumn] = Convert.DBNull;
			}

			// Token: 0x06008519 RID: 34073 RVA: 0x001A1F46 File Offset: 0x001A0146
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public bool IsRES_TERMINATION_DATENull()
			{
				return base.IsNull(this.tableResources.RES_TERMINATION_DATEColumn);
			}

			// Token: 0x0600851A RID: 34074 RVA: 0x001A1F59 File Offset: 0x001A0159
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public void SetRES_TERMINATION_DATENull()
			{
				base[this.tableResources.RES_TERMINATION_DATEColumn] = Convert.DBNull;
			}

			// Token: 0x0600851B RID: 34075 RVA: 0x001A1F71 File Offset: 0x001A0171
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public bool IsRES_IS_TEAMNull()
			{
				return base.IsNull(this.tableResources.RES_IS_TEAMColumn);
			}

			// Token: 0x0600851C RID: 34076 RVA: 0x001A1F84 File Offset: 0x001A0184
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public void SetRES_IS_TEAMNull()
			{
				base[this.tableResources.RES_IS_TEAMColumn] = Convert.DBNull;
			}

			// Token: 0x0600851D RID: 34077 RVA: 0x001A1F9C File Offset: 0x001A019C
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public bool IsRES_EXCHANGE_SYNCNull()
			{
				return base.IsNull(this.tableResources.RES_EXCHANGE_SYNCColumn);
			}

			// Token: 0x0600851E RID: 34078 RVA: 0x001A1FAF File Offset: 0x001A01AF
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public void SetRES_EXCHANGE_SYNCNull()
			{
				base[this.tableResources.RES_EXCHANGE_SYNCColumn] = Convert.DBNull;
			}

			// Token: 0x0600851F RID: 34079 RVA: 0x001A1FC7 File Offset: 0x001A01C7
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			public bool IsRES_EXCHANGE_EWS_URLNull()
			{
				return base.IsNull(this.tableResources.RES_EXCHANGE_EWS_URLColumn);
			}

			// Token: 0x06008520 RID: 34080 RVA: 0x001A1FDA File Offset: 0x001A01DA
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public void SetRES_EXCHANGE_EWS_URLNull()
			{
				base[this.tableResources.RES_EXCHANGE_EWS_URLColumn] = Convert.DBNull;
			}

			// Token: 0x06008521 RID: 34081 RVA: 0x001A1FF2 File Offset: 0x001A01F2
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public bool IsRES_CAL_OOF_EXCHANGE_SYNCNull()
			{
				return base.IsNull(this.tableResources.RES_CAL_OOF_EXCHANGE_SYNCColumn);
			}

			// Token: 0x06008522 RID: 34082 RVA: 0x001A2005 File Offset: 0x001A0205
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public void SetRES_CAL_OOF_EXCHANGE_SYNCNull()
			{
				base[this.tableResources.RES_CAL_OOF_EXCHANGE_SYNCColumn] = Convert.DBNull;
			}

			// Token: 0x06008523 RID: 34083 RVA: 0x001A201D File Offset: 0x001A021D
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			public bool IsRES_COST_CENTERNull()
			{
				return base.IsNull(this.tableResources.RES_COST_CENTERColumn);
			}

			// Token: 0x06008524 RID: 34084 RVA: 0x001A2030 File Offset: 0x001A0230
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public void SetRES_COST_CENTERNull()
			{
				base[this.tableResources.RES_COST_CENTERColumn] = Convert.DBNull;
			}

			// Token: 0x06008525 RID: 34085 RVA: 0x001A2048 File Offset: 0x001A0248
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			public bool IsRES_NOTESNull()
			{
				return base.IsNull(this.tableResources.RES_NOTESColumn);
			}

			// Token: 0x06008526 RID: 34086 RVA: 0x001A205B File Offset: 0x001A025B
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			public void SetRES_NOTESNull()
			{
				base[this.tableResources.RES_NOTESColumn] = Convert.DBNull;
			}

			// Token: 0x06008527 RID: 34087 RVA: 0x001A2073 File Offset: 0x001A0273
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public bool IsBaseCalendarUniqueIdNull()
			{
				return base.IsNull(this.tableResources.BaseCalendarUniqueIdColumn);
			}

			// Token: 0x06008528 RID: 34088 RVA: 0x001A2086 File Offset: 0x001A0286
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			public void SetBaseCalendarUniqueIdNull()
			{
				base[this.tableResources.BaseCalendarUniqueIdColumn] = Convert.DBNull;
			}

			// Token: 0x06008529 RID: 34089 RVA: 0x001A209E File Offset: 0x001A029E
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public bool IsRES_REQUIRES_ENGAGEMENTSNull()
			{
				return base.IsNull(this.tableResources.RES_REQUIRES_ENGAGEMENTSColumn);
			}

			// Token: 0x0600852A RID: 34090 RVA: 0x001A20B1 File Offset: 0x001A02B1
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public void SetRES_REQUIRES_ENGAGEMENTSNull()
			{
				base[this.tableResources.RES_REQUIRES_ENGAGEMENTSColumn] = Convert.DBNull;
			}

			// Token: 0x0600852B RID: 34091 RVA: 0x001A20C9 File Offset: 0x001A02C9
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			public bool IsCREATED_DATENull()
			{
				return base.IsNull(this.tableResources.CREATED_DATEColumn);
			}

			// Token: 0x0600852C RID: 34092 RVA: 0x001A20DC File Offset: 0x001A02DC
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			public void SetCREATED_DATENull()
			{
				base[this.tableResources.CREATED_DATEColumn] = Convert.DBNull;
			}

			// Token: 0x0600852D RID: 34093 RVA: 0x001A20F4 File Offset: 0x001A02F4
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public bool IsMOD_DATENull()
			{
				return base.IsNull(this.tableResources.MOD_DATEColumn);
			}

			// Token: 0x0600852E RID: 34094 RVA: 0x001A2107 File Offset: 0x001A0307
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public void SetMOD_DATENull()
			{
				base[this.tableResources.MOD_DATEColumn] = Convert.DBNull;
			}

			// Token: 0x0600852F RID: 34095 RVA: 0x001A211F File Offset: 0x001A031F
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public ResourceDataSet.ResourceCustomFieldsRow[] GetResourceCustomFieldsRows()
			{
				if (base.Table.ChildRelations["ResourcesResourceCustomFields"] == null)
				{
					return new ResourceDataSet.ResourceCustomFieldsRow[0];
				}
				return (ResourceDataSet.ResourceCustomFieldsRow[])base.GetChildRows(base.Table.ChildRelations["ResourcesResourceCustomFields"]);
			}

			// Token: 0x06008530 RID: 34096 RVA: 0x001A215F File Offset: 0x001A035F
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			public ResourceDataSet.CalendarExceptionsRow[] GetCalendarExceptionsRows()
			{
				if (base.Table.ChildRelations["ResourcesCalendarExceptions"] == null)
				{
					return new ResourceDataSet.CalendarExceptionsRow[0];
				}
				return (ResourceDataSet.CalendarExceptionsRow[])base.GetChildRows(base.Table.ChildRelations["ResourcesCalendarExceptions"]);
			}

			// Token: 0x06008531 RID: 34097 RVA: 0x001A219F File Offset: 0x001A039F
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public ResourceDataSet.ResourceRatesRow[] GetResourceRatesRows()
			{
				if (base.Table.ChildRelations["ResourcesResourceRates"] == null)
				{
					return new ResourceDataSet.ResourceRatesRow[0];
				}
				return (ResourceDataSet.ResourceRatesRow[])base.GetChildRows(base.Table.ChildRelations["ResourcesResourceRates"]);
			}

			// Token: 0x06008532 RID: 34098 RVA: 0x001A21DF File Offset: 0x001A03DF
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			public ResourceDataSet.ResourceAvailabilitiesRow[] GetResourceAvailabilitiesRows()
			{
				if (base.Table.ChildRelations["ResourcesResourceAvailabilities"] == null)
				{
					return new ResourceDataSet.ResourceAvailabilitiesRow[0];
				}
				return (ResourceDataSet.ResourceAvailabilitiesRow[])base.GetChildRows(base.Table.ChildRelations["ResourcesResourceAvailabilities"]);
			}

			// Token: 0x04001AC3 RID: 6851
			private ResourceDataSet.ResourcesDataTable tableResources;
		}

		// Token: 0x02000573 RID: 1395
		public class ResourceCustomFieldsRow : DataRow
		{
			// Token: 0x06008533 RID: 34099 RVA: 0x001A221F File Offset: 0x001A041F
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			internal ResourceCustomFieldsRow(DataRowBuilder rb) : base(rb)
			{
				this.tableResourceCustomFields = (ResourceDataSet.ResourceCustomFieldsDataTable)base.Table;
			}

			// Token: 0x17002843 RID: 10307
			// (get) Token: 0x06008534 RID: 34100 RVA: 0x001A2239 File Offset: 0x001A0439
			// (set) Token: 0x06008535 RID: 34101 RVA: 0x001A2251 File Offset: 0x001A0451
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public Guid CUSTOM_FIELD_UID
			{
				get
				{
					return (Guid)base[this.tableResourceCustomFields.CUSTOM_FIELD_UIDColumn];
				}
				set
				{
					base[this.tableResourceCustomFields.CUSTOM_FIELD_UIDColumn] = value;
				}
			}

			// Token: 0x17002844 RID: 10308
			// (get) Token: 0x06008536 RID: 34102 RVA: 0x001A226A File Offset: 0x001A046A
			// (set) Token: 0x06008537 RID: 34103 RVA: 0x001A2282 File Offset: 0x001A0482
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			public Guid RES_UID
			{
				get
				{
					return (Guid)base[this.tableResourceCustomFields.RES_UIDColumn];
				}
				set
				{
					base[this.tableResourceCustomFields.RES_UIDColumn] = value;
				}
			}

			// Token: 0x17002845 RID: 10309
			// (get) Token: 0x06008538 RID: 34104 RVA: 0x001A229C File Offset: 0x001A049C
			// (set) Token: 0x06008539 RID: 34105 RVA: 0x001A22E0 File Offset: 0x001A04E0
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			public Guid MD_PROP_UID
			{
				get
				{
					Guid result;
					try
					{
						result = (Guid)base[this.tableResourceCustomFields.MD_PROP_UIDColumn];
					}
					catch (InvalidCastException innerException)
					{
						throw new StrongTypingException("The value for column 'MD_PROP_UID' in table 'ResourceCustomFields' is DBNull.", innerException);
					}
					return result;
				}
				set
				{
					base[this.tableResourceCustomFields.MD_PROP_UIDColumn] = value;
				}
			}

			// Token: 0x17002846 RID: 10310
			// (get) Token: 0x0600853A RID: 34106 RVA: 0x001A22FC File Offset: 0x001A04FC
			// (set) Token: 0x0600853B RID: 34107 RVA: 0x001A2340 File Offset: 0x001A0540
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			public bool FLAG_VALUE
			{
				get
				{
					bool result;
					try
					{
						result = (bool)base[this.tableResourceCustomFields.FLAG_VALUEColumn];
					}
					catch (InvalidCastException innerException)
					{
						throw new StrongTypingException("The value for column 'FLAG_VALUE' in table 'ResourceCustomFields' is DBNull.", innerException);
					}
					return result;
				}
				set
				{
					base[this.tableResourceCustomFields.FLAG_VALUEColumn] = value;
				}
			}

			// Token: 0x17002847 RID: 10311
			// (get) Token: 0x0600853C RID: 34108 RVA: 0x001A235C File Offset: 0x001A055C
			// (set) Token: 0x0600853D RID: 34109 RVA: 0x001A23A0 File Offset: 0x001A05A0
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public int MD_PROP_ID
			{
				get
				{
					int result;
					try
					{
						result = (int)base[this.tableResourceCustomFields.MD_PROP_IDColumn];
					}
					catch (InvalidCastException innerException)
					{
						throw new StrongTypingException("The value for column 'MD_PROP_ID' in table 'ResourceCustomFields' is DBNull.", innerException);
					}
					return result;
				}
				set
				{
					base[this.tableResourceCustomFields.MD_PROP_IDColumn] = value;
				}
			}

			// Token: 0x17002848 RID: 10312
			// (get) Token: 0x0600853E RID: 34110 RVA: 0x001A23BC File Offset: 0x001A05BC
			// (set) Token: 0x0600853F RID: 34111 RVA: 0x001A2400 File Offset: 0x001A0600
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			public string MD_PROP_NAME
			{
				get
				{
					string result;
					try
					{
						result = (string)base[this.tableResourceCustomFields.MD_PROP_NAMEColumn];
					}
					catch (InvalidCastException innerException)
					{
						throw new StrongTypingException("The value for column 'MD_PROP_NAME' in table 'ResourceCustomFields' is DBNull.", innerException);
					}
					return result;
				}
				set
				{
					base[this.tableResourceCustomFields.MD_PROP_NAMEColumn] = value;
				}
			}

			// Token: 0x17002849 RID: 10313
			// (get) Token: 0x06008540 RID: 34112 RVA: 0x001A2414 File Offset: 0x001A0614
			// (set) Token: 0x06008541 RID: 34113 RVA: 0x001A2458 File Offset: 0x001A0658
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			public string TEXT_VALUE
			{
				get
				{
					string result;
					try
					{
						result = (string)base[this.tableResourceCustomFields.TEXT_VALUEColumn];
					}
					catch (InvalidCastException innerException)
					{
						throw new StrongTypingException("The value for column 'TEXT_VALUE' in table 'ResourceCustomFields' is DBNull.", innerException);
					}
					return result;
				}
				set
				{
					base[this.tableResourceCustomFields.TEXT_VALUEColumn] = value;
				}
			}

			// Token: 0x1700284A RID: 10314
			// (get) Token: 0x06008542 RID: 34114 RVA: 0x001A246C File Offset: 0x001A066C
			// (set) Token: 0x06008543 RID: 34115 RVA: 0x001A24B0 File Offset: 0x001A06B0
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			public byte FIELD_TYPE_ENUM
			{
				get
				{
					byte result;
					try
					{
						result = (byte)base[this.tableResourceCustomFields.FIELD_TYPE_ENUMColumn];
					}
					catch (InvalidCastException innerException)
					{
						throw new StrongTypingException("The value for column 'FIELD_TYPE_ENUM' in table 'ResourceCustomFields' is DBNull.", innerException);
					}
					return result;
				}
				set
				{
					base[this.tableResourceCustomFields.FIELD_TYPE_ENUMColumn] = value;
				}
			}

			// Token: 0x1700284B RID: 10315
			// (get) Token: 0x06008544 RID: 34116 RVA: 0x001A24CC File Offset: 0x001A06CC
			// (set) Token: 0x06008545 RID: 34117 RVA: 0x001A2510 File Offset: 0x001A0710
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			public DateTime DATE_VALUE
			{
				get
				{
					DateTime result;
					try
					{
						result = (DateTime)base[this.tableResourceCustomFields.DATE_VALUEColumn];
					}
					catch (InvalidCastException innerException)
					{
						throw new StrongTypingException("The value for column 'DATE_VALUE' in table 'ResourceCustomFields' is DBNull.", innerException);
					}
					return result;
				}
				set
				{
					base[this.tableResourceCustomFields.DATE_VALUEColumn] = value;
				}
			}

			// Token: 0x1700284C RID: 10316
			// (get) Token: 0x06008546 RID: 34118 RVA: 0x001A252C File Offset: 0x001A072C
			// (set) Token: 0x06008547 RID: 34119 RVA: 0x001A2570 File Offset: 0x001A0770
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			public Guid CODE_VALUE
			{
				get
				{
					Guid result;
					try
					{
						result = (Guid)base[this.tableResourceCustomFields.CODE_VALUEColumn];
					}
					catch (InvalidCastException innerException)
					{
						throw new StrongTypingException("The value for column 'CODE_VALUE' in table 'ResourceCustomFields' is DBNull.", innerException);
					}
					return result;
				}
				set
				{
					base[this.tableResourceCustomFields.CODE_VALUEColumn] = value;
				}
			}

			// Token: 0x1700284D RID: 10317
			// (get) Token: 0x06008548 RID: 34120 RVA: 0x001A258C File Offset: 0x001A078C
			// (set) Token: 0x06008549 RID: 34121 RVA: 0x001A25D0 File Offset: 0x001A07D0
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public int DUR_VALUE
			{
				get
				{
					int result;
					try
					{
						result = (int)base[this.tableResourceCustomFields.DUR_VALUEColumn];
					}
					catch (InvalidCastException innerException)
					{
						throw new StrongTypingException("The value for column 'DUR_VALUE' in table 'ResourceCustomFields' is DBNull.", innerException);
					}
					return result;
				}
				set
				{
					base[this.tableResourceCustomFields.DUR_VALUEColumn] = value;
				}
			}

			// Token: 0x1700284E RID: 10318
			// (get) Token: 0x0600854A RID: 34122 RVA: 0x001A25EC File Offset: 0x001A07EC
			// (set) Token: 0x0600854B RID: 34123 RVA: 0x001A2630 File Offset: 0x001A0830
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			public decimal NUM_VALUE
			{
				get
				{
					decimal result;
					try
					{
						result = (decimal)base[this.tableResourceCustomFields.NUM_VALUEColumn];
					}
					catch (InvalidCastException innerException)
					{
						throw new StrongTypingException("The value for column 'NUM_VALUE' in table 'ResourceCustomFields' is DBNull.", innerException);
					}
					return result;
				}
				set
				{
					base[this.tableResourceCustomFields.NUM_VALUEColumn] = value;
				}
			}

			// Token: 0x1700284F RID: 10319
			// (get) Token: 0x0600854C RID: 34124 RVA: 0x001A264C File Offset: 0x001A084C
			// (set) Token: 0x0600854D RID: 34125 RVA: 0x001A2690 File Offset: 0x001A0890
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			public byte DUR_FMT
			{
				get
				{
					byte result;
					try
					{
						result = (byte)base[this.tableResourceCustomFields.DUR_FMTColumn];
					}
					catch (InvalidCastException innerException)
					{
						throw new StrongTypingException("The value for column 'DUR_FMT' in table 'ResourceCustomFields' is DBNull.", innerException);
					}
					return result;
				}
				set
				{
					base[this.tableResourceCustomFields.DUR_FMTColumn] = value;
				}
			}

			// Token: 0x17002850 RID: 10320
			// (get) Token: 0x0600854E RID: 34126 RVA: 0x001A26AC File Offset: 0x001A08AC
			// (set) Token: 0x0600854F RID: 34127 RVA: 0x001A26F0 File Offset: 0x001A08F0
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			public int INDICATOR_VALUE
			{
				get
				{
					int result;
					try
					{
						result = (int)base[this.tableResourceCustomFields.INDICATOR_VALUEColumn];
					}
					catch (InvalidCastException innerException)
					{
						throw new StrongTypingException("The value for column 'INDICATOR_VALUE' in table 'ResourceCustomFields' is DBNull.", innerException);
					}
					return result;
				}
				set
				{
					base[this.tableResourceCustomFields.INDICATOR_VALUEColumn] = value;
				}
			}

			// Token: 0x17002851 RID: 10321
			// (get) Token: 0x06008550 RID: 34128 RVA: 0x001A2709 File Offset: 0x001A0909
			// (set) Token: 0x06008551 RID: 34129 RVA: 0x001A272B File Offset: 0x001A092B
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			public ResourceDataSet.ResourcesRow ResourcesRow
			{
				get
				{
					return (ResourceDataSet.ResourcesRow)base.GetParentRow(base.Table.ParentRelations["ResourcesResourceCustomFields"]);
				}
				set
				{
					base.SetParentRow(value, base.Table.ParentRelations["ResourcesResourceCustomFields"]);
				}
			}

			// Token: 0x06008552 RID: 34130 RVA: 0x001A2749 File Offset: 0x001A0949
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			public bool IsMD_PROP_UIDNull()
			{
				return base.IsNull(this.tableResourceCustomFields.MD_PROP_UIDColumn);
			}

			// Token: 0x06008553 RID: 34131 RVA: 0x001A275C File Offset: 0x001A095C
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public void SetMD_PROP_UIDNull()
			{
				base[this.tableResourceCustomFields.MD_PROP_UIDColumn] = Convert.DBNull;
			}

			// Token: 0x06008554 RID: 34132 RVA: 0x001A2774 File Offset: 0x001A0974
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			public bool IsFLAG_VALUENull()
			{
				return base.IsNull(this.tableResourceCustomFields.FLAG_VALUEColumn);
			}

			// Token: 0x06008555 RID: 34133 RVA: 0x001A2787 File Offset: 0x001A0987
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public void SetFLAG_VALUENull()
			{
				base[this.tableResourceCustomFields.FLAG_VALUEColumn] = Convert.DBNull;
			}

			// Token: 0x06008556 RID: 34134 RVA: 0x001A279F File Offset: 0x001A099F
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public bool IsMD_PROP_IDNull()
			{
				return base.IsNull(this.tableResourceCustomFields.MD_PROP_IDColumn);
			}

			// Token: 0x06008557 RID: 34135 RVA: 0x001A27B2 File Offset: 0x001A09B2
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			public void SetMD_PROP_IDNull()
			{
				base[this.tableResourceCustomFields.MD_PROP_IDColumn] = Convert.DBNull;
			}

			// Token: 0x06008558 RID: 34136 RVA: 0x001A27CA File Offset: 0x001A09CA
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public bool IsMD_PROP_NAMENull()
			{
				return base.IsNull(this.tableResourceCustomFields.MD_PROP_NAMEColumn);
			}

			// Token: 0x06008559 RID: 34137 RVA: 0x001A27DD File Offset: 0x001A09DD
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public void SetMD_PROP_NAMENull()
			{
				base[this.tableResourceCustomFields.MD_PROP_NAMEColumn] = Convert.DBNull;
			}

			// Token: 0x0600855A RID: 34138 RVA: 0x001A27F5 File Offset: 0x001A09F5
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			public bool IsTEXT_VALUENull()
			{
				return base.IsNull(this.tableResourceCustomFields.TEXT_VALUEColumn);
			}

			// Token: 0x0600855B RID: 34139 RVA: 0x001A2808 File Offset: 0x001A0A08
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			public void SetTEXT_VALUENull()
			{
				base[this.tableResourceCustomFields.TEXT_VALUEColumn] = Convert.DBNull;
			}

			// Token: 0x0600855C RID: 34140 RVA: 0x001A2820 File Offset: 0x001A0A20
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public bool IsFIELD_TYPE_ENUMNull()
			{
				return base.IsNull(this.tableResourceCustomFields.FIELD_TYPE_ENUMColumn);
			}

			// Token: 0x0600855D RID: 34141 RVA: 0x001A2833 File Offset: 0x001A0A33
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public void SetFIELD_TYPE_ENUMNull()
			{
				base[this.tableResourceCustomFields.FIELD_TYPE_ENUMColumn] = Convert.DBNull;
			}

			// Token: 0x0600855E RID: 34142 RVA: 0x001A284B File Offset: 0x001A0A4B
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public bool IsDATE_VALUENull()
			{
				return base.IsNull(this.tableResourceCustomFields.DATE_VALUEColumn);
			}

			// Token: 0x0600855F RID: 34143 RVA: 0x001A285E File Offset: 0x001A0A5E
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			public void SetDATE_VALUENull()
			{
				base[this.tableResourceCustomFields.DATE_VALUEColumn] = Convert.DBNull;
			}

			// Token: 0x06008560 RID: 34144 RVA: 0x001A2876 File Offset: 0x001A0A76
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			public bool IsCODE_VALUENull()
			{
				return base.IsNull(this.tableResourceCustomFields.CODE_VALUEColumn);
			}

			// Token: 0x06008561 RID: 34145 RVA: 0x001A2889 File Offset: 0x001A0A89
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public void SetCODE_VALUENull()
			{
				base[this.tableResourceCustomFields.CODE_VALUEColumn] = Convert.DBNull;
			}

			// Token: 0x06008562 RID: 34146 RVA: 0x001A28A1 File Offset: 0x001A0AA1
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public bool IsDUR_VALUENull()
			{
				return base.IsNull(this.tableResourceCustomFields.DUR_VALUEColumn);
			}

			// Token: 0x06008563 RID: 34147 RVA: 0x001A28B4 File Offset: 0x001A0AB4
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public void SetDUR_VALUENull()
			{
				base[this.tableResourceCustomFields.DUR_VALUEColumn] = Convert.DBNull;
			}

			// Token: 0x06008564 RID: 34148 RVA: 0x001A28CC File Offset: 0x001A0ACC
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			public bool IsNUM_VALUENull()
			{
				return base.IsNull(this.tableResourceCustomFields.NUM_VALUEColumn);
			}

			// Token: 0x06008565 RID: 34149 RVA: 0x001A28DF File Offset: 0x001A0ADF
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public void SetNUM_VALUENull()
			{
				base[this.tableResourceCustomFields.NUM_VALUEColumn] = Convert.DBNull;
			}

			// Token: 0x06008566 RID: 34150 RVA: 0x001A28F7 File Offset: 0x001A0AF7
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			public bool IsDUR_FMTNull()
			{
				return base.IsNull(this.tableResourceCustomFields.DUR_FMTColumn);
			}

			// Token: 0x06008567 RID: 34151 RVA: 0x001A290A File Offset: 0x001A0B0A
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			public void SetDUR_FMTNull()
			{
				base[this.tableResourceCustomFields.DUR_FMTColumn] = Convert.DBNull;
			}

			// Token: 0x06008568 RID: 34152 RVA: 0x001A2922 File Offset: 0x001A0B22
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public bool IsINDICATOR_VALUENull()
			{
				return base.IsNull(this.tableResourceCustomFields.INDICATOR_VALUEColumn);
			}

			// Token: 0x06008569 RID: 34153 RVA: 0x001A2935 File Offset: 0x001A0B35
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			public void SetINDICATOR_VALUENull()
			{
				base[this.tableResourceCustomFields.INDICATOR_VALUEColumn] = Convert.DBNull;
			}

			// Token: 0x04001AC4 RID: 6852
			private ResourceDataSet.ResourceCustomFieldsDataTable tableResourceCustomFields;
		}

		// Token: 0x02000574 RID: 1396
		public class CalendarExceptionsRow : DataRow
		{
			// Token: 0x0600856A RID: 34154 RVA: 0x001A294D File Offset: 0x001A0B4D
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			internal CalendarExceptionsRow(DataRowBuilder rb) : base(rb)
			{
				this.tableCalendarExceptions = (ResourceDataSet.CalendarExceptionsDataTable)base.Table;
			}

			// Token: 0x17002852 RID: 10322
			// (get) Token: 0x0600856B RID: 34155 RVA: 0x001A2967 File Offset: 0x001A0B67
			// (set) Token: 0x0600856C RID: 34156 RVA: 0x001A297F File Offset: 0x001A0B7F
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			public Guid RES_UID
			{
				get
				{
					return (Guid)base[this.tableCalendarExceptions.RES_UIDColumn];
				}
				set
				{
					base[this.tableCalendarExceptions.RES_UIDColumn] = value;
				}
			}

			// Token: 0x17002853 RID: 10323
			// (get) Token: 0x0600856D RID: 34157 RVA: 0x001A2998 File Offset: 0x001A0B98
			// (set) Token: 0x0600856E RID: 34158 RVA: 0x001A29B0 File Offset: 0x001A0BB0
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			public string Name
			{
				get
				{
					return (string)base[this.tableCalendarExceptions.NameColumn];
				}
				set
				{
					base[this.tableCalendarExceptions.NameColumn] = value;
				}
			}

			// Token: 0x17002854 RID: 10324
			// (get) Token: 0x0600856F RID: 34159 RVA: 0x001A29C4 File Offset: 0x001A0BC4
			// (set) Token: 0x06008570 RID: 34160 RVA: 0x001A29DC File Offset: 0x001A0BDC
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			public DateTime Start
			{
				get
				{
					return (DateTime)base[this.tableCalendarExceptions.StartColumn];
				}
				set
				{
					base[this.tableCalendarExceptions.StartColumn] = value;
				}
			}

			// Token: 0x17002855 RID: 10325
			// (get) Token: 0x06008571 RID: 34161 RVA: 0x001A29F5 File Offset: 0x001A0BF5
			// (set) Token: 0x06008572 RID: 34162 RVA: 0x001A2A0D File Offset: 0x001A0C0D
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			public DateTime Finish
			{
				get
				{
					return (DateTime)base[this.tableCalendarExceptions.FinishColumn];
				}
				set
				{
					base[this.tableCalendarExceptions.FinishColumn] = value;
				}
			}

			// Token: 0x17002856 RID: 10326
			// (get) Token: 0x06008573 RID: 34163 RVA: 0x001A2A28 File Offset: 0x001A0C28
			// (set) Token: 0x06008574 RID: 34164 RVA: 0x001A2A6C File Offset: 0x001A0C6C
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			public int Shift1Start
			{
				get
				{
					int result;
					try
					{
						result = (int)base[this.tableCalendarExceptions.Shift1StartColumn];
					}
					catch (InvalidCastException innerException)
					{
						throw new StrongTypingException("The value for column 'Shift1Start' in table 'CalendarExceptions' is DBNull.", innerException);
					}
					return result;
				}
				set
				{
					base[this.tableCalendarExceptions.Shift1StartColumn] = value;
				}
			}

			// Token: 0x17002857 RID: 10327
			// (get) Token: 0x06008575 RID: 34165 RVA: 0x001A2A88 File Offset: 0x001A0C88
			// (set) Token: 0x06008576 RID: 34166 RVA: 0x001A2ACC File Offset: 0x001A0CCC
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			public int Shift1Finish
			{
				get
				{
					int result;
					try
					{
						result = (int)base[this.tableCalendarExceptions.Shift1FinishColumn];
					}
					catch (InvalidCastException innerException)
					{
						throw new StrongTypingException("The value for column 'Shift1Finish' in table 'CalendarExceptions' is DBNull.", innerException);
					}
					return result;
				}
				set
				{
					base[this.tableCalendarExceptions.Shift1FinishColumn] = value;
				}
			}

			// Token: 0x17002858 RID: 10328
			// (get) Token: 0x06008577 RID: 34167 RVA: 0x001A2AE8 File Offset: 0x001A0CE8
			// (set) Token: 0x06008578 RID: 34168 RVA: 0x001A2B2C File Offset: 0x001A0D2C
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			public int Shift2Start
			{
				get
				{
					int result;
					try
					{
						result = (int)base[this.tableCalendarExceptions.Shift2StartColumn];
					}
					catch (InvalidCastException innerException)
					{
						throw new StrongTypingException("The value for column 'Shift2Start' in table 'CalendarExceptions' is DBNull.", innerException);
					}
					return result;
				}
				set
				{
					base[this.tableCalendarExceptions.Shift2StartColumn] = value;
				}
			}

			// Token: 0x17002859 RID: 10329
			// (get) Token: 0x06008579 RID: 34169 RVA: 0x001A2B48 File Offset: 0x001A0D48
			// (set) Token: 0x0600857A RID: 34170 RVA: 0x001A2B8C File Offset: 0x001A0D8C
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			public int Shift2Finish
			{
				get
				{
					int result;
					try
					{
						result = (int)base[this.tableCalendarExceptions.Shift2FinishColumn];
					}
					catch (InvalidCastException innerException)
					{
						throw new StrongTypingException("The value for column 'Shift2Finish' in table 'CalendarExceptions' is DBNull.", innerException);
					}
					return result;
				}
				set
				{
					base[this.tableCalendarExceptions.Shift2FinishColumn] = value;
				}
			}

			// Token: 0x1700285A RID: 10330
			// (get) Token: 0x0600857B RID: 34171 RVA: 0x001A2BA8 File Offset: 0x001A0DA8
			// (set) Token: 0x0600857C RID: 34172 RVA: 0x001A2BEC File Offset: 0x001A0DEC
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			public int Shift3Start
			{
				get
				{
					int result;
					try
					{
						result = (int)base[this.tableCalendarExceptions.Shift3StartColumn];
					}
					catch (InvalidCastException innerException)
					{
						throw new StrongTypingException("The value for column 'Shift3Start' in table 'CalendarExceptions' is DBNull.", innerException);
					}
					return result;
				}
				set
				{
					base[this.tableCalendarExceptions.Shift3StartColumn] = value;
				}
			}

			// Token: 0x1700285B RID: 10331
			// (get) Token: 0x0600857D RID: 34173 RVA: 0x001A2C08 File Offset: 0x001A0E08
			// (set) Token: 0x0600857E RID: 34174 RVA: 0x001A2C4C File Offset: 0x001A0E4C
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			public int Shift3Finish
			{
				get
				{
					int result;
					try
					{
						result = (int)base[this.tableCalendarExceptions.Shift3FinishColumn];
					}
					catch (InvalidCastException innerException)
					{
						throw new StrongTypingException("The value for column 'Shift3Finish' in table 'CalendarExceptions' is DBNull.", innerException);
					}
					return result;
				}
				set
				{
					base[this.tableCalendarExceptions.Shift3FinishColumn] = value;
				}
			}

			// Token: 0x1700285C RID: 10332
			// (get) Token: 0x0600857F RID: 34175 RVA: 0x001A2C68 File Offset: 0x001A0E68
			// (set) Token: 0x06008580 RID: 34176 RVA: 0x001A2CAC File Offset: 0x001A0EAC
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			public int Shift4Start
			{
				get
				{
					int result;
					try
					{
						result = (int)base[this.tableCalendarExceptions.Shift4StartColumn];
					}
					catch (InvalidCastException innerException)
					{
						throw new StrongTypingException("The value for column 'Shift4Start' in table 'CalendarExceptions' is DBNull.", innerException);
					}
					return result;
				}
				set
				{
					base[this.tableCalendarExceptions.Shift4StartColumn] = value;
				}
			}

			// Token: 0x1700285D RID: 10333
			// (get) Token: 0x06008581 RID: 34177 RVA: 0x001A2CC8 File Offset: 0x001A0EC8
			// (set) Token: 0x06008582 RID: 34178 RVA: 0x001A2D0C File Offset: 0x001A0F0C
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			public int Shift4Finish
			{
				get
				{
					int result;
					try
					{
						result = (int)base[this.tableCalendarExceptions.Shift4FinishColumn];
					}
					catch (InvalidCastException innerException)
					{
						throw new StrongTypingException("The value for column 'Shift4Finish' in table 'CalendarExceptions' is DBNull.", innerException);
					}
					return result;
				}
				set
				{
					base[this.tableCalendarExceptions.Shift4FinishColumn] = value;
				}
			}

			// Token: 0x1700285E RID: 10334
			// (get) Token: 0x06008583 RID: 34179 RVA: 0x001A2D28 File Offset: 0x001A0F28
			// (set) Token: 0x06008584 RID: 34180 RVA: 0x001A2D6C File Offset: 0x001A0F6C
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			public int Shift5Start
			{
				get
				{
					int result;
					try
					{
						result = (int)base[this.tableCalendarExceptions.Shift5StartColumn];
					}
					catch (InvalidCastException innerException)
					{
						throw new StrongTypingException("The value for column 'Shift5Start' in table 'CalendarExceptions' is DBNull.", innerException);
					}
					return result;
				}
				set
				{
					base[this.tableCalendarExceptions.Shift5StartColumn] = value;
				}
			}

			// Token: 0x1700285F RID: 10335
			// (get) Token: 0x06008585 RID: 34181 RVA: 0x001A2D88 File Offset: 0x001A0F88
			// (set) Token: 0x06008586 RID: 34182 RVA: 0x001A2DCC File Offset: 0x001A0FCC
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			public int Shift5Finish
			{
				get
				{
					int result;
					try
					{
						result = (int)base[this.tableCalendarExceptions.Shift5FinishColumn];
					}
					catch (InvalidCastException innerException)
					{
						throw new StrongTypingException("The value for column 'Shift5Finish' in table 'CalendarExceptions' is DBNull.", innerException);
					}
					return result;
				}
				set
				{
					base[this.tableCalendarExceptions.Shift5FinishColumn] = value;
				}
			}

			// Token: 0x17002860 RID: 10336
			// (get) Token: 0x06008587 RID: 34183 RVA: 0x001A2DE5 File Offset: 0x001A0FE5
			// (set) Token: 0x06008588 RID: 34184 RVA: 0x001A2DFD File Offset: 0x001A0FFD
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			public int RecurrenceType
			{
				get
				{
					return (int)base[this.tableCalendarExceptions.RecurrenceTypeColumn];
				}
				set
				{
					base[this.tableCalendarExceptions.RecurrenceTypeColumn] = value;
				}
			}

			// Token: 0x17002861 RID: 10337
			// (get) Token: 0x06008589 RID: 34185 RVA: 0x001A2E16 File Offset: 0x001A1016
			// (set) Token: 0x0600858A RID: 34186 RVA: 0x001A2E2E File Offset: 0x001A102E
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			public int RecurrenceFrequency
			{
				get
				{
					return (int)base[this.tableCalendarExceptions.RecurrenceFrequencyColumn];
				}
				set
				{
					base[this.tableCalendarExceptions.RecurrenceFrequencyColumn] = value;
				}
			}

			// Token: 0x17002862 RID: 10338
			// (get) Token: 0x0600858B RID: 34187 RVA: 0x001A2E48 File Offset: 0x001A1048
			// (set) Token: 0x0600858C RID: 34188 RVA: 0x001A2E8C File Offset: 0x001A108C
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public int RecurrenceDays
			{
				get
				{
					int result;
					try
					{
						result = (int)base[this.tableCalendarExceptions.RecurrenceDaysColumn];
					}
					catch (InvalidCastException innerException)
					{
						throw new StrongTypingException("The value for column 'RecurrenceDays' in table 'CalendarExceptions' is DBNull.", innerException);
					}
					return result;
				}
				set
				{
					base[this.tableCalendarExceptions.RecurrenceDaysColumn] = value;
				}
			}

			// Token: 0x17002863 RID: 10339
			// (get) Token: 0x0600858D RID: 34189 RVA: 0x001A2EA8 File Offset: 0x001A10A8
			// (set) Token: 0x0600858E RID: 34190 RVA: 0x001A2EEC File Offset: 0x001A10EC
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			public int RecurrenceMonthDay
			{
				get
				{
					int result;
					try
					{
						result = (int)base[this.tableCalendarExceptions.RecurrenceMonthDayColumn];
					}
					catch (InvalidCastException innerException)
					{
						throw new StrongTypingException("The value for column 'RecurrenceMonthDay' in table 'CalendarExceptions' is DBNull.", innerException);
					}
					return result;
				}
				set
				{
					base[this.tableCalendarExceptions.RecurrenceMonthDayColumn] = value;
				}
			}

			// Token: 0x17002864 RID: 10340
			// (get) Token: 0x0600858F RID: 34191 RVA: 0x001A2F08 File Offset: 0x001A1108
			// (set) Token: 0x06008590 RID: 34192 RVA: 0x001A2F4C File Offset: 0x001A114C
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			public int RecurrenceMonth
			{
				get
				{
					int result;
					try
					{
						result = (int)base[this.tableCalendarExceptions.RecurrenceMonthColumn];
					}
					catch (InvalidCastException innerException)
					{
						throw new StrongTypingException("The value for column 'RecurrenceMonth' in table 'CalendarExceptions' is DBNull.", innerException);
					}
					return result;
				}
				set
				{
					base[this.tableCalendarExceptions.RecurrenceMonthColumn] = value;
				}
			}

			// Token: 0x17002865 RID: 10341
			// (get) Token: 0x06008591 RID: 34193 RVA: 0x001A2F68 File Offset: 0x001A1168
			// (set) Token: 0x06008592 RID: 34194 RVA: 0x001A2FAC File Offset: 0x001A11AC
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			public int RecurrencePosition
			{
				get
				{
					int result;
					try
					{
						result = (int)base[this.tableCalendarExceptions.RecurrencePositionColumn];
					}
					catch (InvalidCastException innerException)
					{
						throw new StrongTypingException("The value for column 'RecurrencePosition' in table 'CalendarExceptions' is DBNull.", innerException);
					}
					return result;
				}
				set
				{
					base[this.tableCalendarExceptions.RecurrencePositionColumn] = value;
				}
			}

			// Token: 0x17002866 RID: 10342
			// (get) Token: 0x06008593 RID: 34195 RVA: 0x001A2FC5 File Offset: 0x001A11C5
			// (set) Token: 0x06008594 RID: 34196 RVA: 0x001A2FE7 File Offset: 0x001A11E7
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			public ResourceDataSet.ResourcesRow ResourcesRow
			{
				get
				{
					return (ResourceDataSet.ResourcesRow)base.GetParentRow(base.Table.ParentRelations["ResourcesCalendarExceptions"]);
				}
				set
				{
					base.SetParentRow(value, base.Table.ParentRelations["ResourcesCalendarExceptions"]);
				}
			}

			// Token: 0x06008595 RID: 34197 RVA: 0x001A3005 File Offset: 0x001A1205
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public bool IsShift1StartNull()
			{
				return base.IsNull(this.tableCalendarExceptions.Shift1StartColumn);
			}

			// Token: 0x06008596 RID: 34198 RVA: 0x001A3018 File Offset: 0x001A1218
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			public void SetShift1StartNull()
			{
				base[this.tableCalendarExceptions.Shift1StartColumn] = Convert.DBNull;
			}

			// Token: 0x06008597 RID: 34199 RVA: 0x001A3030 File Offset: 0x001A1230
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public bool IsShift1FinishNull()
			{
				return base.IsNull(this.tableCalendarExceptions.Shift1FinishColumn);
			}

			// Token: 0x06008598 RID: 34200 RVA: 0x001A3043 File Offset: 0x001A1243
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			public void SetShift1FinishNull()
			{
				base[this.tableCalendarExceptions.Shift1FinishColumn] = Convert.DBNull;
			}

			// Token: 0x06008599 RID: 34201 RVA: 0x001A305B File Offset: 0x001A125B
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public bool IsShift2StartNull()
			{
				return base.IsNull(this.tableCalendarExceptions.Shift2StartColumn);
			}

			// Token: 0x0600859A RID: 34202 RVA: 0x001A306E File Offset: 0x001A126E
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			public void SetShift2StartNull()
			{
				base[this.tableCalendarExceptions.Shift2StartColumn] = Convert.DBNull;
			}

			// Token: 0x0600859B RID: 34203 RVA: 0x001A3086 File Offset: 0x001A1286
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			public bool IsShift2FinishNull()
			{
				return base.IsNull(this.tableCalendarExceptions.Shift2FinishColumn);
			}

			// Token: 0x0600859C RID: 34204 RVA: 0x001A3099 File Offset: 0x001A1299
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public void SetShift2FinishNull()
			{
				base[this.tableCalendarExceptions.Shift2FinishColumn] = Convert.DBNull;
			}

			// Token: 0x0600859D RID: 34205 RVA: 0x001A30B1 File Offset: 0x001A12B1
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public bool IsShift3StartNull()
			{
				return base.IsNull(this.tableCalendarExceptions.Shift3StartColumn);
			}

			// Token: 0x0600859E RID: 34206 RVA: 0x001A30C4 File Offset: 0x001A12C4
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public void SetShift3StartNull()
			{
				base[this.tableCalendarExceptions.Shift3StartColumn] = Convert.DBNull;
			}

			// Token: 0x0600859F RID: 34207 RVA: 0x001A30DC File Offset: 0x001A12DC
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public bool IsShift3FinishNull()
			{
				return base.IsNull(this.tableCalendarExceptions.Shift3FinishColumn);
			}

			// Token: 0x060085A0 RID: 34208 RVA: 0x001A30EF File Offset: 0x001A12EF
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			public void SetShift3FinishNull()
			{
				base[this.tableCalendarExceptions.Shift3FinishColumn] = Convert.DBNull;
			}

			// Token: 0x060085A1 RID: 34209 RVA: 0x001A3107 File Offset: 0x001A1307
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public bool IsShift4StartNull()
			{
				return base.IsNull(this.tableCalendarExceptions.Shift4StartColumn);
			}

			// Token: 0x060085A2 RID: 34210 RVA: 0x001A311A File Offset: 0x001A131A
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public void SetShift4StartNull()
			{
				base[this.tableCalendarExceptions.Shift4StartColumn] = Convert.DBNull;
			}

			// Token: 0x060085A3 RID: 34211 RVA: 0x001A3132 File Offset: 0x001A1332
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			public bool IsShift4FinishNull()
			{
				return base.IsNull(this.tableCalendarExceptions.Shift4FinishColumn);
			}

			// Token: 0x060085A4 RID: 34212 RVA: 0x001A3145 File Offset: 0x001A1345
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			public void SetShift4FinishNull()
			{
				base[this.tableCalendarExceptions.Shift4FinishColumn] = Convert.DBNull;
			}

			// Token: 0x060085A5 RID: 34213 RVA: 0x001A315D File Offset: 0x001A135D
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public bool IsShift5StartNull()
			{
				return base.IsNull(this.tableCalendarExceptions.Shift5StartColumn);
			}

			// Token: 0x060085A6 RID: 34214 RVA: 0x001A3170 File Offset: 0x001A1370
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public void SetShift5StartNull()
			{
				base[this.tableCalendarExceptions.Shift5StartColumn] = Convert.DBNull;
			}

			// Token: 0x060085A7 RID: 34215 RVA: 0x001A3188 File Offset: 0x001A1388
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public bool IsShift5FinishNull()
			{
				return base.IsNull(this.tableCalendarExceptions.Shift5FinishColumn);
			}

			// Token: 0x060085A8 RID: 34216 RVA: 0x001A319B File Offset: 0x001A139B
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public void SetShift5FinishNull()
			{
				base[this.tableCalendarExceptions.Shift5FinishColumn] = Convert.DBNull;
			}

			// Token: 0x060085A9 RID: 34217 RVA: 0x001A31B3 File Offset: 0x001A13B3
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public bool IsRecurrenceDaysNull()
			{
				return base.IsNull(this.tableCalendarExceptions.RecurrenceDaysColumn);
			}

			// Token: 0x060085AA RID: 34218 RVA: 0x001A31C6 File Offset: 0x001A13C6
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public void SetRecurrenceDaysNull()
			{
				base[this.tableCalendarExceptions.RecurrenceDaysColumn] = Convert.DBNull;
			}

			// Token: 0x060085AB RID: 34219 RVA: 0x001A31DE File Offset: 0x001A13DE
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public bool IsRecurrenceMonthDayNull()
			{
				return base.IsNull(this.tableCalendarExceptions.RecurrenceMonthDayColumn);
			}

			// Token: 0x060085AC RID: 34220 RVA: 0x001A31F1 File Offset: 0x001A13F1
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			public void SetRecurrenceMonthDayNull()
			{
				base[this.tableCalendarExceptions.RecurrenceMonthDayColumn] = Convert.DBNull;
			}

			// Token: 0x060085AD RID: 34221 RVA: 0x001A3209 File Offset: 0x001A1409
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public bool IsRecurrenceMonthNull()
			{
				return base.IsNull(this.tableCalendarExceptions.RecurrenceMonthColumn);
			}

			// Token: 0x060085AE RID: 34222 RVA: 0x001A321C File Offset: 0x001A141C
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public void SetRecurrenceMonthNull()
			{
				base[this.tableCalendarExceptions.RecurrenceMonthColumn] = Convert.DBNull;
			}

			// Token: 0x060085AF RID: 34223 RVA: 0x001A3234 File Offset: 0x001A1434
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public bool IsRecurrencePositionNull()
			{
				return base.IsNull(this.tableCalendarExceptions.RecurrencePositionColumn);
			}

			// Token: 0x060085B0 RID: 34224 RVA: 0x001A3247 File Offset: 0x001A1447
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			public void SetRecurrencePositionNull()
			{
				base[this.tableCalendarExceptions.RecurrencePositionColumn] = Convert.DBNull;
			}

			// Token: 0x04001AC5 RID: 6853
			private ResourceDataSet.CalendarExceptionsDataTable tableCalendarExceptions;
		}

		// Token: 0x02000575 RID: 1397
		public class ResourceRatesRow : DataRow
		{
			// Token: 0x060085B1 RID: 34225 RVA: 0x001A325F File Offset: 0x001A145F
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			internal ResourceRatesRow(DataRowBuilder rb) : base(rb)
			{
				this.tableResourceRates = (ResourceDataSet.ResourceRatesDataTable)base.Table;
			}

			// Token: 0x17002867 RID: 10343
			// (get) Token: 0x060085B2 RID: 34226 RVA: 0x001A3279 File Offset: 0x001A1479
			// (set) Token: 0x060085B3 RID: 34227 RVA: 0x001A3291 File Offset: 0x001A1491
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			public Guid RES_UID
			{
				get
				{
					return (Guid)base[this.tableResourceRates.RES_UIDColumn];
				}
				set
				{
					base[this.tableResourceRates.RES_UIDColumn] = value;
				}
			}

			// Token: 0x17002868 RID: 10344
			// (get) Token: 0x060085B4 RID: 34228 RVA: 0x001A32AA File Offset: 0x001A14AA
			// (set) Token: 0x060085B5 RID: 34229 RVA: 0x001A32C2 File Offset: 0x001A14C2
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			public int RES_RATE_TABLE
			{
				get
				{
					return (int)base[this.tableResourceRates.RES_RATE_TABLEColumn];
				}
				set
				{
					base[this.tableResourceRates.RES_RATE_TABLEColumn] = value;
				}
			}

			// Token: 0x17002869 RID: 10345
			// (get) Token: 0x060085B6 RID: 34230 RVA: 0x001A32DC File Offset: 0x001A14DC
			// (set) Token: 0x060085B7 RID: 34231 RVA: 0x001A3320 File Offset: 0x001A1520
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public DateTime RES_RATE_EFFECTIVE_DATE
			{
				get
				{
					DateTime result;
					try
					{
						result = (DateTime)base[this.tableResourceRates.RES_RATE_EFFECTIVE_DATEColumn];
					}
					catch (InvalidCastException innerException)
					{
						throw new StrongTypingException("The value for column 'RES_RATE_EFFECTIVE_DATE' in table 'ResourceRates' is DBNull.", innerException);
					}
					return result;
				}
				set
				{
					base[this.tableResourceRates.RES_RATE_EFFECTIVE_DATEColumn] = value;
				}
			}

			// Token: 0x1700286A RID: 10346
			// (get) Token: 0x060085B8 RID: 34232 RVA: 0x001A333C File Offset: 0x001A153C
			// (set) Token: 0x060085B9 RID: 34233 RVA: 0x001A3380 File Offset: 0x001A1580
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			public double RES_STD_RATE
			{
				get
				{
					double result;
					try
					{
						result = (double)base[this.tableResourceRates.RES_STD_RATEColumn];
					}
					catch (InvalidCastException innerException)
					{
						throw new StrongTypingException("The value for column 'RES_STD_RATE' in table 'ResourceRates' is DBNull.", innerException);
					}
					return result;
				}
				set
				{
					base[this.tableResourceRates.RES_STD_RATEColumn] = value;
				}
			}

			// Token: 0x1700286B RID: 10347
			// (get) Token: 0x060085BA RID: 34234 RVA: 0x001A339C File Offset: 0x001A159C
			// (set) Token: 0x060085BB RID: 34235 RVA: 0x001A33E0 File Offset: 0x001A15E0
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			public double RES_OVT_RATE
			{
				get
				{
					double result;
					try
					{
						result = (double)base[this.tableResourceRates.RES_OVT_RATEColumn];
					}
					catch (InvalidCastException innerException)
					{
						throw new StrongTypingException("The value for column 'RES_OVT_RATE' in table 'ResourceRates' is DBNull.", innerException);
					}
					return result;
				}
				set
				{
					base[this.tableResourceRates.RES_OVT_RATEColumn] = value;
				}
			}

			// Token: 0x1700286C RID: 10348
			// (get) Token: 0x060085BC RID: 34236 RVA: 0x001A33FC File Offset: 0x001A15FC
			// (set) Token: 0x060085BD RID: 34237 RVA: 0x001A3440 File Offset: 0x001A1640
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			public double RES_COST_PER_USE
			{
				get
				{
					double result;
					try
					{
						result = (double)base[this.tableResourceRates.RES_COST_PER_USEColumn];
					}
					catch (InvalidCastException innerException)
					{
						throw new StrongTypingException("The value for column 'RES_COST_PER_USE' in table 'ResourceRates' is DBNull.", innerException);
					}
					return result;
				}
				set
				{
					base[this.tableResourceRates.RES_COST_PER_USEColumn] = value;
				}
			}

			// Token: 0x1700286D RID: 10349
			// (get) Token: 0x060085BE RID: 34238 RVA: 0x001A3459 File Offset: 0x001A1659
			// (set) Token: 0x060085BF RID: 34239 RVA: 0x001A347B File Offset: 0x001A167B
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			public ResourceDataSet.ResourcesRow ResourcesRow
			{
				get
				{
					return (ResourceDataSet.ResourcesRow)base.GetParentRow(base.Table.ParentRelations["ResourcesResourceRates"]);
				}
				set
				{
					base.SetParentRow(value, base.Table.ParentRelations["ResourcesResourceRates"]);
				}
			}

			// Token: 0x060085C0 RID: 34240 RVA: 0x001A3499 File Offset: 0x001A1699
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			public bool IsRES_RATE_EFFECTIVE_DATENull()
			{
				return base.IsNull(this.tableResourceRates.RES_RATE_EFFECTIVE_DATEColumn);
			}

			// Token: 0x060085C1 RID: 34241 RVA: 0x001A34AC File Offset: 0x001A16AC
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			public void SetRES_RATE_EFFECTIVE_DATENull()
			{
				base[this.tableResourceRates.RES_RATE_EFFECTIVE_DATEColumn] = Convert.DBNull;
			}

			// Token: 0x060085C2 RID: 34242 RVA: 0x001A34C4 File Offset: 0x001A16C4
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public bool IsRES_STD_RATENull()
			{
				return base.IsNull(this.tableResourceRates.RES_STD_RATEColumn);
			}

			// Token: 0x060085C3 RID: 34243 RVA: 0x001A34D7 File Offset: 0x001A16D7
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			public void SetRES_STD_RATENull()
			{
				base[this.tableResourceRates.RES_STD_RATEColumn] = Convert.DBNull;
			}

			// Token: 0x060085C4 RID: 34244 RVA: 0x001A34EF File Offset: 0x001A16EF
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			public bool IsRES_OVT_RATENull()
			{
				return base.IsNull(this.tableResourceRates.RES_OVT_RATEColumn);
			}

			// Token: 0x060085C5 RID: 34245 RVA: 0x001A3502 File Offset: 0x001A1702
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public void SetRES_OVT_RATENull()
			{
				base[this.tableResourceRates.RES_OVT_RATEColumn] = Convert.DBNull;
			}

			// Token: 0x060085C6 RID: 34246 RVA: 0x001A351A File Offset: 0x001A171A
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public bool IsRES_COST_PER_USENull()
			{
				return base.IsNull(this.tableResourceRates.RES_COST_PER_USEColumn);
			}

			// Token: 0x060085C7 RID: 34247 RVA: 0x001A352D File Offset: 0x001A172D
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public void SetRES_COST_PER_USENull()
			{
				base[this.tableResourceRates.RES_COST_PER_USEColumn] = Convert.DBNull;
			}

			// Token: 0x04001AC6 RID: 6854
			private ResourceDataSet.ResourceRatesDataTable tableResourceRates;
		}

		// Token: 0x02000576 RID: 1398
		public class ResourceAvailabilitiesRow : DataRow
		{
			// Token: 0x060085C8 RID: 34248 RVA: 0x001A3545 File Offset: 0x001A1745
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			internal ResourceAvailabilitiesRow(DataRowBuilder rb) : base(rb)
			{
				this.tableResourceAvailabilities = (ResourceDataSet.ResourceAvailabilitiesDataTable)base.Table;
			}

			// Token: 0x1700286E RID: 10350
			// (get) Token: 0x060085C9 RID: 34249 RVA: 0x001A355F File Offset: 0x001A175F
			// (set) Token: 0x060085CA RID: 34250 RVA: 0x001A3577 File Offset: 0x001A1777
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			public Guid RES_UID
			{
				get
				{
					return (Guid)base[this.tableResourceAvailabilities.RES_UIDColumn];
				}
				set
				{
					base[this.tableResourceAvailabilities.RES_UIDColumn] = value;
				}
			}

			// Token: 0x1700286F RID: 10351
			// (get) Token: 0x060085CB RID: 34251 RVA: 0x001A3590 File Offset: 0x001A1790
			// (set) Token: 0x060085CC RID: 34252 RVA: 0x001A35D4 File Offset: 0x001A17D4
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			public DateTime RES_AVAIL_FROM
			{
				get
				{
					DateTime result;
					try
					{
						result = (DateTime)base[this.tableResourceAvailabilities.RES_AVAIL_FROMColumn];
					}
					catch (InvalidCastException innerException)
					{
						throw new StrongTypingException("The value for column 'RES_AVAIL_FROM' in table 'ResourceAvailabilities' is DBNull.", innerException);
					}
					return result;
				}
				set
				{
					base[this.tableResourceAvailabilities.RES_AVAIL_FROMColumn] = value;
				}
			}

			// Token: 0x17002870 RID: 10352
			// (get) Token: 0x060085CD RID: 34253 RVA: 0x001A35F0 File Offset: 0x001A17F0
			// (set) Token: 0x060085CE RID: 34254 RVA: 0x001A3634 File Offset: 0x001A1834
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			public DateTime RES_AVAIL_TO
			{
				get
				{
					DateTime result;
					try
					{
						result = (DateTime)base[this.tableResourceAvailabilities.RES_AVAIL_TOColumn];
					}
					catch (InvalidCastException innerException)
					{
						throw new StrongTypingException("The value for column 'RES_AVAIL_TO' in table 'ResourceAvailabilities' is DBNull.", innerException);
					}
					return result;
				}
				set
				{
					base[this.tableResourceAvailabilities.RES_AVAIL_TOColumn] = value;
				}
			}

			// Token: 0x17002871 RID: 10353
			// (get) Token: 0x060085CF RID: 34255 RVA: 0x001A3650 File Offset: 0x001A1850
			// (set) Token: 0x060085D0 RID: 34256 RVA: 0x001A3694 File Offset: 0x001A1894
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			public double RES_AVAIL_UNITS
			{
				get
				{
					double result;
					try
					{
						result = (double)base[this.tableResourceAvailabilities.RES_AVAIL_UNITSColumn];
					}
					catch (InvalidCastException innerException)
					{
						throw new StrongTypingException("The value for column 'RES_AVAIL_UNITS' in table 'ResourceAvailabilities' is DBNull.", innerException);
					}
					return result;
				}
				set
				{
					base[this.tableResourceAvailabilities.RES_AVAIL_UNITSColumn] = value;
				}
			}

			// Token: 0x17002872 RID: 10354
			// (get) Token: 0x060085D1 RID: 34257 RVA: 0x001A36AD File Offset: 0x001A18AD
			// (set) Token: 0x060085D2 RID: 34258 RVA: 0x001A36CF File Offset: 0x001A18CF
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			public ResourceDataSet.ResourcesRow ResourcesRow
			{
				get
				{
					return (ResourceDataSet.ResourcesRow)base.GetParentRow(base.Table.ParentRelations["ResourcesResourceAvailabilities"]);
				}
				set
				{
					base.SetParentRow(value, base.Table.ParentRelations["ResourcesResourceAvailabilities"]);
				}
			}

			// Token: 0x060085D3 RID: 34259 RVA: 0x001A36ED File Offset: 0x001A18ED
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public bool IsRES_AVAIL_FROMNull()
			{
				return base.IsNull(this.tableResourceAvailabilities.RES_AVAIL_FROMColumn);
			}

			// Token: 0x060085D4 RID: 34260 RVA: 0x001A3700 File Offset: 0x001A1900
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public void SetRES_AVAIL_FROMNull()
			{
				base[this.tableResourceAvailabilities.RES_AVAIL_FROMColumn] = Convert.DBNull;
			}

			// Token: 0x060085D5 RID: 34261 RVA: 0x001A3718 File Offset: 0x001A1918
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public bool IsRES_AVAIL_TONull()
			{
				return base.IsNull(this.tableResourceAvailabilities.RES_AVAIL_TOColumn);
			}

			// Token: 0x060085D6 RID: 34262 RVA: 0x001A372B File Offset: 0x001A192B
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			public void SetRES_AVAIL_TONull()
			{
				base[this.tableResourceAvailabilities.RES_AVAIL_TOColumn] = Convert.DBNull;
			}

			// Token: 0x060085D7 RID: 34263 RVA: 0x001A3743 File Offset: 0x001A1943
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public bool IsRES_AVAIL_UNITSNull()
			{
				return base.IsNull(this.tableResourceAvailabilities.RES_AVAIL_UNITSColumn);
			}

			// Token: 0x060085D8 RID: 34264 RVA: 0x001A3756 File Offset: 0x001A1956
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public void SetRES_AVAIL_UNITSNull()
			{
				base[this.tableResourceAvailabilities.RES_AVAIL_UNITSColumn] = Convert.DBNull;
			}

			// Token: 0x04001AC7 RID: 6855
			private ResourceDataSet.ResourceAvailabilitiesDataTable tableResourceAvailabilities;
		}

		// Token: 0x02000577 RID: 1399
		[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
		public class ResourcesRowChangeEvent : EventArgs
		{
			// Token: 0x060085D9 RID: 34265 RVA: 0x001A376E File Offset: 0x001A196E
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public ResourcesRowChangeEvent(ResourceDataSet.ResourcesRow row, DataRowAction action)
			{
				this.eventRow = row;
				this.eventAction = action;
			}

			// Token: 0x17002873 RID: 10355
			// (get) Token: 0x060085DA RID: 34266 RVA: 0x001A3784 File Offset: 0x001A1984
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			public ResourceDataSet.ResourcesRow Row
			{
				get
				{
					return this.eventRow;
				}
			}

			// Token: 0x17002874 RID: 10356
			// (get) Token: 0x060085DB RID: 34267 RVA: 0x001A378C File Offset: 0x001A198C
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			public DataRowAction Action
			{
				get
				{
					return this.eventAction;
				}
			}

			// Token: 0x04001AC8 RID: 6856
			private ResourceDataSet.ResourcesRow eventRow;

			// Token: 0x04001AC9 RID: 6857
			private DataRowAction eventAction;
		}

		// Token: 0x02000578 RID: 1400
		[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
		public class ResourceCustomFieldsRowChangeEvent : EventArgs
		{
			// Token: 0x060085DC RID: 34268 RVA: 0x001A3794 File Offset: 0x001A1994
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			public ResourceCustomFieldsRowChangeEvent(ResourceDataSet.ResourceCustomFieldsRow row, DataRowAction action)
			{
				this.eventRow = row;
				this.eventAction = action;
			}

			// Token: 0x17002875 RID: 10357
			// (get) Token: 0x060085DD RID: 34269 RVA: 0x001A37AA File Offset: 0x001A19AA
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public ResourceDataSet.ResourceCustomFieldsRow Row
			{
				get
				{
					return this.eventRow;
				}
			}

			// Token: 0x17002876 RID: 10358
			// (get) Token: 0x060085DE RID: 34270 RVA: 0x001A37B2 File Offset: 0x001A19B2
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			public DataRowAction Action
			{
				get
				{
					return this.eventAction;
				}
			}

			// Token: 0x04001ACA RID: 6858
			private ResourceDataSet.ResourceCustomFieldsRow eventRow;

			// Token: 0x04001ACB RID: 6859
			private DataRowAction eventAction;
		}

		// Token: 0x02000579 RID: 1401
		[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
		public class CalendarExceptionsRowChangeEvent : EventArgs
		{
			// Token: 0x060085DF RID: 34271 RVA: 0x001A37BA File Offset: 0x001A19BA
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public CalendarExceptionsRowChangeEvent(ResourceDataSet.CalendarExceptionsRow row, DataRowAction action)
			{
				this.eventRow = row;
				this.eventAction = action;
			}

			// Token: 0x17002877 RID: 10359
			// (get) Token: 0x060085E0 RID: 34272 RVA: 0x001A37D0 File Offset: 0x001A19D0
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			public ResourceDataSet.CalendarExceptionsRow Row
			{
				get
				{
					return this.eventRow;
				}
			}

			// Token: 0x17002878 RID: 10360
			// (get) Token: 0x060085E1 RID: 34273 RVA: 0x001A37D8 File Offset: 0x001A19D8
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			public DataRowAction Action
			{
				get
				{
					return this.eventAction;
				}
			}

			// Token: 0x04001ACC RID: 6860
			private ResourceDataSet.CalendarExceptionsRow eventRow;

			// Token: 0x04001ACD RID: 6861
			private DataRowAction eventAction;
		}

		// Token: 0x0200057A RID: 1402
		[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
		public class ResourceRatesRowChangeEvent : EventArgs
		{
			// Token: 0x060085E2 RID: 34274 RVA: 0x001A37E0 File Offset: 0x001A19E0
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			public ResourceRatesRowChangeEvent(ResourceDataSet.ResourceRatesRow row, DataRowAction action)
			{
				this.eventRow = row;
				this.eventAction = action;
			}

			// Token: 0x17002879 RID: 10361
			// (get) Token: 0x060085E3 RID: 34275 RVA: 0x001A37F6 File Offset: 0x001A19F6
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			public ResourceDataSet.ResourceRatesRow Row
			{
				get
				{
					return this.eventRow;
				}
			}

			// Token: 0x1700287A RID: 10362
			// (get) Token: 0x060085E4 RID: 34276 RVA: 0x001A37FE File Offset: 0x001A19FE
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			public DataRowAction Action
			{
				get
				{
					return this.eventAction;
				}
			}

			// Token: 0x04001ACE RID: 6862
			private ResourceDataSet.ResourceRatesRow eventRow;

			// Token: 0x04001ACF RID: 6863
			private DataRowAction eventAction;
		}

		// Token: 0x0200057B RID: 1403
		[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
		public class ResourceAvailabilitiesRowChangeEvent : EventArgs
		{
			// Token: 0x060085E5 RID: 34277 RVA: 0x001A3806 File Offset: 0x001A1A06
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			public ResourceAvailabilitiesRowChangeEvent(ResourceDataSet.ResourceAvailabilitiesRow row, DataRowAction action)
			{
				this.eventRow = row;
				this.eventAction = action;
			}

			// Token: 0x1700287B RID: 10363
			// (get) Token: 0x060085E6 RID: 34278 RVA: 0x001A381C File Offset: 0x001A1A1C
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			public ResourceDataSet.ResourceAvailabilitiesRow Row
			{
				get
				{
					return this.eventRow;
				}
			}

			// Token: 0x1700287C RID: 10364
			// (get) Token: 0x060085E7 RID: 34279 RVA: 0x001A3824 File Offset: 0x001A1A24
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			public DataRowAction Action
			{
				get
				{
					return this.eventAction;
				}
			}

			// Token: 0x04001AD0 RID: 6864
			private ResourceDataSet.ResourceAvailabilitiesRow eventRow;

			// Token: 0x04001AD1 RID: 6865
			private DataRowAction eventAction;
		}
	}
}
