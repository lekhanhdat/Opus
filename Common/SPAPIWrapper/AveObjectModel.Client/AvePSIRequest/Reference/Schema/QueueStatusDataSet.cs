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
	// Token: 0x0200035F RID: 863
	[XmlRoot("QueueStatusDataSet")]
	[DesignerCategory("code")]
	[XmlSchemaProvider("GetTypedDataSetSchema")]
	[ToolboxItem(true)]
	[HelpKeyword("vs.data.DataSet")]
	[Serializable]
	public class QueueStatusDataSet : DataSet
	{
		// Token: 0x06005869 RID: 22633 RVA: 0x00115760 File Offset: 0x00113960
		public new void EndInit()
		{
			base.EndInit();
			TypedDataSetUtilities.AllowNullsInNonTypedColumns(this.Status, new string[]
			{
				"QueueLocalStorage",
				"CorrelationPriority",
				"JobGUID",
				"QueueID",
				"GroupPriority",
				"WaitTime",
				"QueueWakeupTime",
				"JobCompletionState",
				"QueueEntryTime",
				"LastAdminAction",
				"ServerId",
				"JobInfoGUID",
				"MessageType",
				"QueueProcessingTime",
				"QueueCompletedTime",
				"ServiceName",
				"ResourceGUID",
				"MachineName",
				"QueuePosition",
				"CorrelationGUID",
				"PercentComplete",
				"GroupState",
				"JobGroupGUID",
				"ErrorInfo"
			});
		}

		// Token: 0x0600586A RID: 22634 RVA: 0x00115858 File Offset: 0x00113A58
		[DebuggerNonUserCode]
		[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
		public QueueStatusDataSet()
		{
			base.BeginInit();
			this.InitClass();
			CollectionChangeEventHandler value = new CollectionChangeEventHandler(this.SchemaChanged);
			base.Tables.CollectionChanged += value;
			base.Relations.CollectionChanged += value;
			this.EndInit();
		}

		// Token: 0x0600586B RID: 22635 RVA: 0x001158AC File Offset: 0x00113AAC
		[DebuggerNonUserCode]
		[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
		protected QueueStatusDataSet(SerializationInfo info, StreamingContext context) : base(info, context, false)
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
				if (dataSet.Tables["Status"] != null)
				{
					base.Tables.Add(new QueueStatusDataSet.StatusDataTable(dataSet.Tables["Status"]));
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

		// Token: 0x17001C0A RID: 7178
		// (get) Token: 0x0600586C RID: 22636 RVA: 0x00115A09 File Offset: 0x00113C09
		[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Content)]
		[DebuggerNonUserCode]
		[Browsable(false)]
		public QueueStatusDataSet.StatusDataTable Status
		{
			get
			{
				return this.tableStatus;
			}
		}

		// Token: 0x17001C0B RID: 7179
		// (get) Token: 0x0600586D RID: 22637 RVA: 0x00115A11 File Offset: 0x00113C11
		// (set) Token: 0x0600586E RID: 22638 RVA: 0x00115A19 File Offset: 0x00113C19
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Visible)]
		[DebuggerNonUserCode]
		[Browsable(true)]
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

		// Token: 0x17001C0C RID: 7180
		// (get) Token: 0x0600586F RID: 22639 RVA: 0x00115A22 File Offset: 0x00113C22
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

		// Token: 0x17001C0D RID: 7181
		// (get) Token: 0x06005870 RID: 22640 RVA: 0x00115A2A File Offset: 0x00113C2A
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

		// Token: 0x06005871 RID: 22641 RVA: 0x00115A32 File Offset: 0x00113C32
		[DebuggerNonUserCode]
		[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
		protected override void InitializeDerivedDataSet()
		{
			base.BeginInit();
			this.InitClass();
			this.EndInit();
		}

		// Token: 0x06005872 RID: 22642 RVA: 0x00115A48 File Offset: 0x00113C48
		[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
		[DebuggerNonUserCode]
		public override DataSet Clone()
		{
			QueueStatusDataSet queueStatusDataSet = (QueueStatusDataSet)base.Clone();
			queueStatusDataSet.InitVars();
			queueStatusDataSet.SchemaSerializationMode = this.SchemaSerializationMode;
			return queueStatusDataSet;
		}

		// Token: 0x06005873 RID: 22643 RVA: 0x00115A74 File Offset: 0x00113C74
		[DebuggerNonUserCode]
		[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
		protected override bool ShouldSerializeTables()
		{
			return false;
		}

		// Token: 0x06005874 RID: 22644 RVA: 0x00115A77 File Offset: 0x00113C77
		[DebuggerNonUserCode]
		[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
		protected override bool ShouldSerializeRelations()
		{
			return false;
		}

		// Token: 0x06005875 RID: 22645 RVA: 0x00115A7C File Offset: 0x00113C7C
		[DebuggerNonUserCode]
		[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
		protected override void ReadXmlSerializable(XmlReader reader)
		{
			if (base.DetermineSchemaSerializationMode(reader) == SchemaSerializationMode.IncludeSchema)
			{
				this.Reset();
				DataSet dataSet = new DataSet();
				dataSet.ReadXml(reader);
				if (dataSet.Tables["Status"] != null)
				{
					base.Tables.Add(new QueueStatusDataSet.StatusDataTable(dataSet.Tables["Status"]));
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

		// Token: 0x06005876 RID: 22646 RVA: 0x00115B44 File Offset: 0x00113D44
		[DebuggerNonUserCode]
		[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
		protected override XmlSchema GetSchemaSerializable()
		{
			MemoryStream memoryStream = new MemoryStream();
			base.WriteXmlSchema(new XmlTextWriter(memoryStream, null));
			memoryStream.Position = 0L;
			return XmlSchema.Read(new XmlTextReader(memoryStream), null);
		}

		// Token: 0x06005877 RID: 22647 RVA: 0x00115B78 File Offset: 0x00113D78
		[DebuggerNonUserCode]
		[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
		internal void InitVars()
		{
			this.InitVars(true);
		}

		// Token: 0x06005878 RID: 22648 RVA: 0x00115B81 File Offset: 0x00113D81
		[DebuggerNonUserCode]
		[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
		internal void InitVars(bool initTable)
		{
			this.tableStatus = (QueueStatusDataSet.StatusDataTable)base.Tables["Status"];
			if (initTable && this.tableStatus != null)
			{
				this.tableStatus.InitVars();
			}
		}

		// Token: 0x06005879 RID: 22649 RVA: 0x00115BB4 File Offset: 0x00113DB4
		[DebuggerNonUserCode]
		[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
		private void InitClass()
		{
			base.DataSetName = "QueueStatusDataSet";
			base.Prefix = "";
			base.Namespace = "http://schemas.microsoft.com/office/project/server/webservices/QueueStatusDataSet/";
			base.EnforceConstraints = true;
			this.SchemaSerializationMode = SchemaSerializationMode.IncludeSchema;
			this.tableStatus = new QueueStatusDataSet.StatusDataTable();
			base.Tables.Add(this.tableStatus);
		}

		// Token: 0x0600587A RID: 22650 RVA: 0x00115C0C File Offset: 0x00113E0C
		[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
		[DebuggerNonUserCode]
		private bool ShouldSerializeStatus()
		{
			return false;
		}

		// Token: 0x0600587B RID: 22651 RVA: 0x00115C0F File Offset: 0x00113E0F
		[DebuggerNonUserCode]
		[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
		private void SchemaChanged(object sender, CollectionChangeEventArgs e)
		{
			if (e.Action == CollectionChangeAction.Remove)
			{
				this.InitVars();
			}
		}

		// Token: 0x0600587C RID: 22652 RVA: 0x00115C20 File Offset: 0x00113E20
		[DebuggerNonUserCode]
		[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
		public static XmlSchemaComplexType GetTypedDataSetSchema(XmlSchemaSet xs)
		{
			QueueStatusDataSet queueStatusDataSet = new QueueStatusDataSet();
			XmlSchemaComplexType xmlSchemaComplexType = new XmlSchemaComplexType();
			XmlSchemaSequence xmlSchemaSequence = new XmlSchemaSequence();
			XmlSchemaAny xmlSchemaAny = new XmlSchemaAny();
			xmlSchemaAny.Namespace = queueStatusDataSet.Namespace;
			xmlSchemaSequence.Items.Add(xmlSchemaAny);
			xmlSchemaComplexType.Particle = xmlSchemaSequence;
			XmlSchema schemaSerializable = queueStatusDataSet.GetSchemaSerializable();
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

		// Token: 0x0400125D RID: 4701
		private QueueStatusDataSet.StatusDataTable tableStatus;

		// Token: 0x0400125E RID: 4702
		private SchemaSerializationMode _schemaSerializationMode = SchemaSerializationMode.IncludeSchema;

		// Token: 0x02000360 RID: 864
		// (Invoke) Token: 0x0600587E RID: 22654
		[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
		public delegate void StatusRowChangeEventHandler(object sender, QueueStatusDataSet.StatusRowChangeEvent e);

		// Token: 0x02000361 RID: 865
		[XmlSchemaProvider("GetTypedTableSchema")]
		[Serializable]
		public class StatusDataTable : DataTable, IEnumerable
		{
			// Token: 0x06005881 RID: 22657 RVA: 0x00115D68 File Offset: 0x00113F68
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public StatusDataTable()
			{
				base.TableName = "Status";
				this.BeginInit();
				this.InitClass();
				this.EndInit();
			}

			// Token: 0x06005882 RID: 22658 RVA: 0x00115D90 File Offset: 0x00113F90
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			internal StatusDataTable(DataTable table)
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

			// Token: 0x06005883 RID: 22659 RVA: 0x00115E38 File Offset: 0x00114038
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			protected StatusDataTable(SerializationInfo info, StreamingContext context) : base(info, context)
			{
				this.InitVars();
			}

			// Token: 0x17001C0E RID: 7182
			// (get) Token: 0x06005884 RID: 22660 RVA: 0x00115E48 File Offset: 0x00114048
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			public DataColumn QueueIDColumn
			{
				get
				{
					return this.columnQueueID;
				}
			}

			// Token: 0x17001C0F RID: 7183
			// (get) Token: 0x06005885 RID: 22661 RVA: 0x00115E50 File Offset: 0x00114050
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			public DataColumn MessageTypeColumn
			{
				get
				{
					return this.columnMessageType;
				}
			}

			// Token: 0x17001C10 RID: 7184
			// (get) Token: 0x06005886 RID: 22662 RVA: 0x00115E58 File Offset: 0x00114058
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			public DataColumn JobCompletionStateColumn
			{
				get
				{
					return this.columnJobCompletionState;
				}
			}

			// Token: 0x17001C11 RID: 7185
			// (get) Token: 0x06005887 RID: 22663 RVA: 0x00115E60 File Offset: 0x00114060
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public DataColumn QueuePositionColumn
			{
				get
				{
					return this.columnQueuePosition;
				}
			}

			// Token: 0x17001C12 RID: 7186
			// (get) Token: 0x06005888 RID: 22664 RVA: 0x00115E68 File Offset: 0x00114068
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			public DataColumn LastAdminActionColumn
			{
				get
				{
					return this.columnLastAdminAction;
				}
			}

			// Token: 0x17001C13 RID: 7187
			// (get) Token: 0x06005889 RID: 22665 RVA: 0x00115E70 File Offset: 0x00114070
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public DataColumn ErrorInfoColumn
			{
				get
				{
					return this.columnErrorInfo;
				}
			}

			// Token: 0x17001C14 RID: 7188
			// (get) Token: 0x0600588A RID: 22666 RVA: 0x00115E78 File Offset: 0x00114078
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			public DataColumn PercentCompleteColumn
			{
				get
				{
					return this.columnPercentComplete;
				}
			}

			// Token: 0x17001C15 RID: 7189
			// (get) Token: 0x0600588B RID: 22667 RVA: 0x00115E80 File Offset: 0x00114080
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public DataColumn QueueEntryTimeColumn
			{
				get
				{
					return this.columnQueueEntryTime;
				}
			}

			// Token: 0x17001C16 RID: 7190
			// (get) Token: 0x0600588C RID: 22668 RVA: 0x00115E88 File Offset: 0x00114088
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			public DataColumn QueueProcessingTimeColumn
			{
				get
				{
					return this.columnQueueProcessingTime;
				}
			}

			// Token: 0x17001C17 RID: 7191
			// (get) Token: 0x0600588D RID: 22669 RVA: 0x00115E90 File Offset: 0x00114090
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public DataColumn QueueCompletedTimeColumn
			{
				get
				{
					return this.columnQueueCompletedTime;
				}
			}

			// Token: 0x17001C18 RID: 7192
			// (get) Token: 0x0600588E RID: 22670 RVA: 0x00115E98 File Offset: 0x00114098
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			public DataColumn QueueWakeupTimeColumn
			{
				get
				{
					return this.columnQueueWakeupTime;
				}
			}

			// Token: 0x17001C19 RID: 7193
			// (get) Token: 0x0600588F RID: 22671 RVA: 0x00115EA0 File Offset: 0x001140A0
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public DataColumn WaitTimeColumn
			{
				get
				{
					return this.columnWaitTime;
				}
			}

			// Token: 0x17001C1A RID: 7194
			// (get) Token: 0x06005890 RID: 22672 RVA: 0x00115EA8 File Offset: 0x001140A8
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			public DataColumn GroupPriorityColumn
			{
				get
				{
					return this.columnGroupPriority;
				}
			}

			// Token: 0x17001C1B RID: 7195
			// (get) Token: 0x06005891 RID: 22673 RVA: 0x00115EB0 File Offset: 0x001140B0
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public DataColumn GroupStateColumn
			{
				get
				{
					return this.columnGroupState;
				}
			}

			// Token: 0x17001C1C RID: 7196
			// (get) Token: 0x06005892 RID: 22674 RVA: 0x00115EB8 File Offset: 0x001140B8
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			public DataColumn CorrelationPriorityColumn
			{
				get
				{
					return this.columnCorrelationPriority;
				}
			}

			// Token: 0x17001C1D RID: 7197
			// (get) Token: 0x06005893 RID: 22675 RVA: 0x00115EC0 File Offset: 0x001140C0
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public DataColumn CorrelationGUIDColumn
			{
				get
				{
					return this.columnCorrelationGUID;
				}
			}

			// Token: 0x17001C1E RID: 7198
			// (get) Token: 0x06005894 RID: 22676 RVA: 0x00115EC8 File Offset: 0x001140C8
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public DataColumn JobGUIDColumn
			{
				get
				{
					return this.columnJobGUID;
				}
			}

			// Token: 0x17001C1F RID: 7199
			// (get) Token: 0x06005895 RID: 22677 RVA: 0x00115ED0 File Offset: 0x001140D0
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			public DataColumn JobGroupGUIDColumn
			{
				get
				{
					return this.columnJobGroupGUID;
				}
			}

			// Token: 0x17001C20 RID: 7200
			// (get) Token: 0x06005896 RID: 22678 RVA: 0x00115ED8 File Offset: 0x001140D8
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public DataColumn JobInfoGUIDColumn
			{
				get
				{
					return this.columnJobInfoGUID;
				}
			}

			// Token: 0x17001C21 RID: 7201
			// (get) Token: 0x06005897 RID: 22679 RVA: 0x00115EE0 File Offset: 0x001140E0
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			public DataColumn ResourceGUIDColumn
			{
				get
				{
					return this.columnResourceGUID;
				}
			}

			// Token: 0x17001C22 RID: 7202
			// (get) Token: 0x06005898 RID: 22680 RVA: 0x00115EE8 File Offset: 0x001140E8
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public DataColumn MachineNameColumn
			{
				get
				{
					return this.columnMachineName;
				}
			}

			// Token: 0x17001C23 RID: 7203
			// (get) Token: 0x06005899 RID: 22681 RVA: 0x00115EF0 File Offset: 0x001140F0
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			public DataColumn ServiceNameColumn
			{
				get
				{
					return this.columnServiceName;
				}
			}

			// Token: 0x17001C24 RID: 7204
			// (get) Token: 0x0600589A RID: 22682 RVA: 0x00115EF8 File Offset: 0x001140F8
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			public DataColumn QueueLocalStorageColumn
			{
				get
				{
					return this.columnQueueLocalStorage;
				}
			}

			// Token: 0x17001C25 RID: 7205
			// (get) Token: 0x0600589B RID: 22683 RVA: 0x00115F00 File Offset: 0x00114100
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public DataColumn ServerIdColumn
			{
				get
				{
					return this.columnServerId;
				}
			}

			// Token: 0x17001C26 RID: 7206
			// (get) Token: 0x0600589C RID: 22684 RVA: 0x00115F08 File Offset: 0x00114108
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

			// Token: 0x17001C27 RID: 7207
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public QueueStatusDataSet.StatusRow this[int index]
			{
				get
				{
					return (QueueStatusDataSet.StatusRow)base.Rows[index];
				}
			}

			// Token: 0x140002F5 RID: 757
			// (add) Token: 0x0600589E RID: 22686 RVA: 0x00115F28 File Offset: 0x00114128
			// (remove) Token: 0x0600589F RID: 22687 RVA: 0x00115F60 File Offset: 0x00114160
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public event QueueStatusDataSet.StatusRowChangeEventHandler StatusRowChanging;

			// Token: 0x140002F6 RID: 758
			// (add) Token: 0x060058A0 RID: 22688 RVA: 0x00115F98 File Offset: 0x00114198
			// (remove) Token: 0x060058A1 RID: 22689 RVA: 0x00115FD0 File Offset: 0x001141D0
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public event QueueStatusDataSet.StatusRowChangeEventHandler StatusRowChanged;

			// Token: 0x140002F7 RID: 759
			// (add) Token: 0x060058A2 RID: 22690 RVA: 0x00116008 File Offset: 0x00114208
			// (remove) Token: 0x060058A3 RID: 22691 RVA: 0x00116040 File Offset: 0x00114240
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public event QueueStatusDataSet.StatusRowChangeEventHandler StatusRowDeleting;

			// Token: 0x140002F8 RID: 760
			// (add) Token: 0x060058A4 RID: 22692 RVA: 0x00116078 File Offset: 0x00114278
			// (remove) Token: 0x060058A5 RID: 22693 RVA: 0x001160B0 File Offset: 0x001142B0
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public event QueueStatusDataSet.StatusRowChangeEventHandler StatusRowDeleted;

			// Token: 0x060058A6 RID: 22694 RVA: 0x001160E5 File Offset: 0x001142E5
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			public void AddStatusRow(QueueStatusDataSet.StatusRow row)
			{
				base.Rows.Add(row);
			}

			// Token: 0x060058A7 RID: 22695 RVA: 0x001160F4 File Offset: 0x001142F4
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			public QueueStatusDataSet.StatusRow AddStatusRow(int QueueID, int MessageType, int JobCompletionState, int QueuePosition, int LastAdminAction, string ErrorInfo, int PercentComplete, DateTime QueueEntryTime, DateTime QueueProcessingTime, DateTime QueueCompletedTime, DateTime QueueWakeupTime, int WaitTime, int GroupPriority, int GroupState, int CorrelationPriority, Guid CorrelationGUID, Guid JobGUID, Guid JobGroupGUID, Guid JobInfoGUID, Guid ResourceGUID, string MachineName, string ServiceName, byte[] QueueLocalStorage, Guid ServerId)
			{
				QueueStatusDataSet.StatusRow statusRow = (QueueStatusDataSet.StatusRow)base.NewRow();
				object[] itemArray = new object[]
				{
					QueueID,
					MessageType,
					JobCompletionState,
					QueuePosition,
					LastAdminAction,
					ErrorInfo,
					PercentComplete,
					QueueEntryTime,
					QueueProcessingTime,
					QueueCompletedTime,
					QueueWakeupTime,
					WaitTime,
					GroupPriority,
					GroupState,
					CorrelationPriority,
					CorrelationGUID,
					JobGUID,
					JobGroupGUID,
					JobInfoGUID,
					ResourceGUID,
					MachineName,
					ServiceName,
					QueueLocalStorage,
					ServerId
				};
				statusRow.ItemArray = itemArray;
				base.Rows.Add(statusRow);
				return statusRow;
			}

			// Token: 0x060058A8 RID: 22696 RVA: 0x00116213 File Offset: 0x00114413
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public virtual IEnumerator GetEnumerator()
			{
				return base.Rows.GetEnumerator();
			}

			// Token: 0x060058A9 RID: 22697 RVA: 0x00116220 File Offset: 0x00114420
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			public override DataTable Clone()
			{
				QueueStatusDataSet.StatusDataTable statusDataTable = (QueueStatusDataSet.StatusDataTable)base.Clone();
				statusDataTable.InitVars();
				return statusDataTable;
			}

			// Token: 0x060058AA RID: 22698 RVA: 0x00116240 File Offset: 0x00114440
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			protected override DataTable CreateInstance()
			{
				return new QueueStatusDataSet.StatusDataTable();
			}

			// Token: 0x060058AB RID: 22699 RVA: 0x00116248 File Offset: 0x00114448
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			internal void InitVars()
			{
				this.columnQueueID = base.Columns["QueueID"];
				this.columnMessageType = base.Columns["MessageType"];
				this.columnJobCompletionState = base.Columns["JobCompletionState"];
				this.columnQueuePosition = base.Columns["QueuePosition"];
				this.columnLastAdminAction = base.Columns["LastAdminAction"];
				this.columnErrorInfo = base.Columns["ErrorInfo"];
				this.columnPercentComplete = base.Columns["PercentComplete"];
				this.columnQueueEntryTime = base.Columns["QueueEntryTime"];
				this.columnQueueProcessingTime = base.Columns["QueueProcessingTime"];
				this.columnQueueCompletedTime = base.Columns["QueueCompletedTime"];
				this.columnQueueWakeupTime = base.Columns["QueueWakeupTime"];
				this.columnWaitTime = base.Columns["WaitTime"];
				this.columnGroupPriority = base.Columns["GroupPriority"];
				this.columnGroupState = base.Columns["GroupState"];
				this.columnCorrelationPriority = base.Columns["CorrelationPriority"];
				this.columnCorrelationGUID = base.Columns["CorrelationGUID"];
				this.columnJobGUID = base.Columns["JobGUID"];
				this.columnJobGroupGUID = base.Columns["JobGroupGUID"];
				this.columnJobInfoGUID = base.Columns["JobInfoGUID"];
				this.columnResourceGUID = base.Columns["ResourceGUID"];
				this.columnMachineName = base.Columns["MachineName"];
				this.columnServiceName = base.Columns["ServiceName"];
				this.columnQueueLocalStorage = base.Columns["QueueLocalStorage"];
				this.columnServerId = base.Columns["ServerId"];
			}

			// Token: 0x060058AC RID: 22700 RVA: 0x00116468 File Offset: 0x00114668
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			private void InitClass()
			{
				this.columnQueueID = new DataColumn("QueueID", typeof(int), null, MappingType.Element);
				base.Columns.Add(this.columnQueueID);
				this.columnMessageType = new DataColumn("MessageType", typeof(int), null, MappingType.Element);
				base.Columns.Add(this.columnMessageType);
				this.columnJobCompletionState = new DataColumn("JobCompletionState", typeof(int), null, MappingType.Element);
				base.Columns.Add(this.columnJobCompletionState);
				this.columnQueuePosition = new DataColumn("QueuePosition", typeof(int), null, MappingType.Element);
				base.Columns.Add(this.columnQueuePosition);
				this.columnLastAdminAction = new DataColumn("LastAdminAction", typeof(int), null, MappingType.Element);
				base.Columns.Add(this.columnLastAdminAction);
				this.columnErrorInfo = new DataColumn("ErrorInfo", typeof(string), null, MappingType.Element);
				base.Columns.Add(this.columnErrorInfo);
				this.columnPercentComplete = new DataColumn("PercentComplete", typeof(int), null, MappingType.Element);
				base.Columns.Add(this.columnPercentComplete);
				this.columnQueueEntryTime = new DataColumn("QueueEntryTime", typeof(DateTime), null, MappingType.Element);
				base.Columns.Add(this.columnQueueEntryTime);
				this.columnQueueProcessingTime = new DataColumn("QueueProcessingTime", typeof(DateTime), null, MappingType.Element);
				base.Columns.Add(this.columnQueueProcessingTime);
				this.columnQueueCompletedTime = new DataColumn("QueueCompletedTime", typeof(DateTime), null, MappingType.Element);
				base.Columns.Add(this.columnQueueCompletedTime);
				this.columnQueueWakeupTime = new DataColumn("QueueWakeupTime", typeof(DateTime), null, MappingType.Element);
				base.Columns.Add(this.columnQueueWakeupTime);
				this.columnWaitTime = new DataColumn("WaitTime", typeof(int), null, MappingType.Element);
				base.Columns.Add(this.columnWaitTime);
				this.columnGroupPriority = new DataColumn("GroupPriority", typeof(int), null, MappingType.Element);
				base.Columns.Add(this.columnGroupPriority);
				this.columnGroupState = new DataColumn("GroupState", typeof(int), null, MappingType.Element);
				base.Columns.Add(this.columnGroupState);
				this.columnCorrelationPriority = new DataColumn("CorrelationPriority", typeof(int), null, MappingType.Element);
				base.Columns.Add(this.columnCorrelationPriority);
				this.columnCorrelationGUID = new DataColumn("CorrelationGUID", typeof(Guid), null, MappingType.Element);
				base.Columns.Add(this.columnCorrelationGUID);
				this.columnJobGUID = new DataColumn("JobGUID", typeof(Guid), null, MappingType.Element);
				base.Columns.Add(this.columnJobGUID);
				this.columnJobGroupGUID = new DataColumn("JobGroupGUID", typeof(Guid), null, MappingType.Element);
				base.Columns.Add(this.columnJobGroupGUID);
				this.columnJobInfoGUID = new DataColumn("JobInfoGUID", typeof(Guid), null, MappingType.Element);
				base.Columns.Add(this.columnJobInfoGUID);
				this.columnResourceGUID = new DataColumn("ResourceGUID", typeof(Guid), null, MappingType.Element);
				base.Columns.Add(this.columnResourceGUID);
				this.columnMachineName = new DataColumn("MachineName", typeof(string), null, MappingType.Element);
				base.Columns.Add(this.columnMachineName);
				this.columnServiceName = new DataColumn("ServiceName", typeof(string), null, MappingType.Element);
				base.Columns.Add(this.columnServiceName);
				this.columnQueueLocalStorage = new DataColumn("QueueLocalStorage", typeof(byte[]), null, MappingType.Element);
				base.Columns.Add(this.columnQueueLocalStorage);
				this.columnServerId = new DataColumn("ServerId", typeof(Guid), null, MappingType.Element);
				base.Columns.Add(this.columnServerId);
				this.columnQueueID.AllowDBNull = false;
				this.columnMessageType.AllowDBNull = false;
				this.columnJobCompletionState.AllowDBNull = false;
				this.columnQueuePosition.AllowDBNull = false;
				this.columnLastAdminAction.AllowDBNull = false;
				this.columnPercentComplete.AllowDBNull = false;
				this.columnQueueEntryTime.AllowDBNull = false;
				this.columnQueueProcessingTime.AllowDBNull = false;
				this.columnQueueCompletedTime.AllowDBNull = false;
				this.columnQueueWakeupTime.AllowDBNull = false;
				this.columnWaitTime.AllowDBNull = false;
				this.columnGroupPriority.AllowDBNull = false;
				this.columnCorrelationPriority.AllowDBNull = false;
				this.columnCorrelationGUID.AllowDBNull = false;
				this.columnJobGUID.AllowDBNull = false;
				this.columnJobGroupGUID.AllowDBNull = false;
				this.columnJobInfoGUID.AllowDBNull = false;
				this.columnResourceGUID.AllowDBNull = false;
			}

			// Token: 0x060058AD RID: 22701 RVA: 0x00116985 File Offset: 0x00114B85
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			public QueueStatusDataSet.StatusRow NewStatusRow()
			{
				return (QueueStatusDataSet.StatusRow)base.NewRow();
			}

			// Token: 0x060058AE RID: 22702 RVA: 0x00116992 File Offset: 0x00114B92
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			protected override DataRow NewRowFromBuilder(DataRowBuilder builder)
			{
				return new QueueStatusDataSet.StatusRow(builder);
			}

			// Token: 0x060058AF RID: 22703 RVA: 0x0011699A File Offset: 0x00114B9A
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			protected override Type GetRowType()
			{
				return typeof(QueueStatusDataSet.StatusRow);
			}

			// Token: 0x060058B0 RID: 22704 RVA: 0x001169A6 File Offset: 0x00114BA6
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			protected override void OnRowChanged(DataRowChangeEventArgs e)
			{
				base.OnRowChanged(e);
				if (this.StatusRowChanged != null)
				{
					this.StatusRowChanged(this, new QueueStatusDataSet.StatusRowChangeEvent((QueueStatusDataSet.StatusRow)e.Row, e.Action));
				}
			}

			// Token: 0x060058B1 RID: 22705 RVA: 0x001169D9 File Offset: 0x00114BD9
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			protected override void OnRowChanging(DataRowChangeEventArgs e)
			{
				base.OnRowChanging(e);
				if (this.StatusRowChanging != null)
				{
					this.StatusRowChanging(this, new QueueStatusDataSet.StatusRowChangeEvent((QueueStatusDataSet.StatusRow)e.Row, e.Action));
				}
			}

			// Token: 0x060058B2 RID: 22706 RVA: 0x00116A0C File Offset: 0x00114C0C
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			protected override void OnRowDeleted(DataRowChangeEventArgs e)
			{
				base.OnRowDeleted(e);
				if (this.StatusRowDeleted != null)
				{
					this.StatusRowDeleted(this, new QueueStatusDataSet.StatusRowChangeEvent((QueueStatusDataSet.StatusRow)e.Row, e.Action));
				}
			}

			// Token: 0x060058B3 RID: 22707 RVA: 0x00116A3F File Offset: 0x00114C3F
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			protected override void OnRowDeleting(DataRowChangeEventArgs e)
			{
				base.OnRowDeleting(e);
				if (this.StatusRowDeleting != null)
				{
					this.StatusRowDeleting(this, new QueueStatusDataSet.StatusRowChangeEvent((QueueStatusDataSet.StatusRow)e.Row, e.Action));
				}
			}

			// Token: 0x060058B4 RID: 22708 RVA: 0x00116A72 File Offset: 0x00114C72
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public void RemoveStatusRow(QueueStatusDataSet.StatusRow row)
			{
				base.Rows.Remove(row);
			}

			// Token: 0x060058B5 RID: 22709 RVA: 0x00116A80 File Offset: 0x00114C80
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			public static XmlSchemaComplexType GetTypedTableSchema(XmlSchemaSet xs)
			{
				XmlSchemaComplexType xmlSchemaComplexType = new XmlSchemaComplexType();
				XmlSchemaSequence xmlSchemaSequence = new XmlSchemaSequence();
				QueueStatusDataSet queueStatusDataSet = new QueueStatusDataSet();
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
				xmlSchemaAttribute.FixedValue = queueStatusDataSet.Namespace;
				xmlSchemaComplexType.Attributes.Add(xmlSchemaAttribute);
				XmlSchemaAttribute xmlSchemaAttribute2 = new XmlSchemaAttribute();
				xmlSchemaAttribute2.Name = "tableTypeName";
				xmlSchemaAttribute2.FixedValue = "StatusDataTable";
				xmlSchemaComplexType.Attributes.Add(xmlSchemaAttribute2);
				xmlSchemaComplexType.Particle = xmlSchemaSequence;
				XmlSchema schemaSerializable = queueStatusDataSet.GetSchemaSerializable();
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

			// Token: 0x0400125F RID: 4703
			private DataColumn columnQueueID;

			// Token: 0x04001260 RID: 4704
			private DataColumn columnMessageType;

			// Token: 0x04001261 RID: 4705
			private DataColumn columnJobCompletionState;

			// Token: 0x04001262 RID: 4706
			private DataColumn columnQueuePosition;

			// Token: 0x04001263 RID: 4707
			private DataColumn columnLastAdminAction;

			// Token: 0x04001264 RID: 4708
			private DataColumn columnErrorInfo;

			// Token: 0x04001265 RID: 4709
			private DataColumn columnPercentComplete;

			// Token: 0x04001266 RID: 4710
			private DataColumn columnQueueEntryTime;

			// Token: 0x04001267 RID: 4711
			private DataColumn columnQueueProcessingTime;

			// Token: 0x04001268 RID: 4712
			private DataColumn columnQueueCompletedTime;

			// Token: 0x04001269 RID: 4713
			private DataColumn columnQueueWakeupTime;

			// Token: 0x0400126A RID: 4714
			private DataColumn columnWaitTime;

			// Token: 0x0400126B RID: 4715
			private DataColumn columnGroupPriority;

			// Token: 0x0400126C RID: 4716
			private DataColumn columnGroupState;

			// Token: 0x0400126D RID: 4717
			private DataColumn columnCorrelationPriority;

			// Token: 0x0400126E RID: 4718
			private DataColumn columnCorrelationGUID;

			// Token: 0x0400126F RID: 4719
			private DataColumn columnJobGUID;

			// Token: 0x04001270 RID: 4720
			private DataColumn columnJobGroupGUID;

			// Token: 0x04001271 RID: 4721
			private DataColumn columnJobInfoGUID;

			// Token: 0x04001272 RID: 4722
			private DataColumn columnResourceGUID;

			// Token: 0x04001273 RID: 4723
			private DataColumn columnMachineName;

			// Token: 0x04001274 RID: 4724
			private DataColumn columnServiceName;

			// Token: 0x04001275 RID: 4725
			private DataColumn columnQueueLocalStorage;

			// Token: 0x04001276 RID: 4726
			private DataColumn columnServerId;
		}

		// Token: 0x02000362 RID: 866
		public class StatusRow : DataRow
		{
			// Token: 0x060058B6 RID: 22710 RVA: 0x00116C78 File Offset: 0x00114E78
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			internal StatusRow(DataRowBuilder rb) : base(rb)
			{
				this.tableStatus = (QueueStatusDataSet.StatusDataTable)base.Table;
			}

			// Token: 0x17001C28 RID: 7208
			// (get) Token: 0x060058B7 RID: 22711 RVA: 0x00116C92 File Offset: 0x00114E92
			// (set) Token: 0x060058B8 RID: 22712 RVA: 0x00116CAA File Offset: 0x00114EAA
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public int QueueID
			{
				get
				{
					return (int)base[this.tableStatus.QueueIDColumn];
				}
				set
				{
					base[this.tableStatus.QueueIDColumn] = value;
				}
			}

			// Token: 0x17001C29 RID: 7209
			// (get) Token: 0x060058B9 RID: 22713 RVA: 0x00116CC3 File Offset: 0x00114EC3
			// (set) Token: 0x060058BA RID: 22714 RVA: 0x00116CDB File Offset: 0x00114EDB
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public int MessageType
			{
				get
				{
					return (int)base[this.tableStatus.MessageTypeColumn];
				}
				set
				{
					base[this.tableStatus.MessageTypeColumn] = value;
				}
			}

			// Token: 0x17001C2A RID: 7210
			// (get) Token: 0x060058BB RID: 22715 RVA: 0x00116CF4 File Offset: 0x00114EF4
			// (set) Token: 0x060058BC RID: 22716 RVA: 0x00116D0C File Offset: 0x00114F0C
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public int JobCompletionState
			{
				get
				{
					return (int)base[this.tableStatus.JobCompletionStateColumn];
				}
				set
				{
					base[this.tableStatus.JobCompletionStateColumn] = value;
				}
			}

			// Token: 0x17001C2B RID: 7211
			// (get) Token: 0x060058BD RID: 22717 RVA: 0x00116D25 File Offset: 0x00114F25
			// (set) Token: 0x060058BE RID: 22718 RVA: 0x00116D3D File Offset: 0x00114F3D
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			public int QueuePosition
			{
				get
				{
					return (int)base[this.tableStatus.QueuePositionColumn];
				}
				set
				{
					base[this.tableStatus.QueuePositionColumn] = value;
				}
			}

			// Token: 0x17001C2C RID: 7212
			// (get) Token: 0x060058BF RID: 22719 RVA: 0x00116D56 File Offset: 0x00114F56
			// (set) Token: 0x060058C0 RID: 22720 RVA: 0x00116D6E File Offset: 0x00114F6E
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public int LastAdminAction
			{
				get
				{
					return (int)base[this.tableStatus.LastAdminActionColumn];
				}
				set
				{
					base[this.tableStatus.LastAdminActionColumn] = value;
				}
			}

			// Token: 0x17001C2D RID: 7213
			// (get) Token: 0x060058C1 RID: 22721 RVA: 0x00116D87 File Offset: 0x00114F87
			// (set) Token: 0x060058C2 RID: 22722 RVA: 0x00116DA9 File Offset: 0x00114FA9
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			public string ErrorInfo
			{
				get
				{
					if (this.IsErrorInfoNull())
					{
						return null;
					}
					return (string)base[this.tableStatus.ErrorInfoColumn];
				}
				set
				{
					base[this.tableStatus.ErrorInfoColumn] = value;
				}
			}

			// Token: 0x17001C2E RID: 7214
			// (get) Token: 0x060058C3 RID: 22723 RVA: 0x00116DBD File Offset: 0x00114FBD
			// (set) Token: 0x060058C4 RID: 22724 RVA: 0x00116DD5 File Offset: 0x00114FD5
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public int PercentComplete
			{
				get
				{
					return (int)base[this.tableStatus.PercentCompleteColumn];
				}
				set
				{
					base[this.tableStatus.PercentCompleteColumn] = value;
				}
			}

			// Token: 0x17001C2F RID: 7215
			// (get) Token: 0x060058C5 RID: 22725 RVA: 0x00116DEE File Offset: 0x00114FEE
			// (set) Token: 0x060058C6 RID: 22726 RVA: 0x00116E06 File Offset: 0x00115006
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			public DateTime QueueEntryTime
			{
				get
				{
					return (DateTime)base[this.tableStatus.QueueEntryTimeColumn];
				}
				set
				{
					base[this.tableStatus.QueueEntryTimeColumn] = value;
				}
			}

			// Token: 0x17001C30 RID: 7216
			// (get) Token: 0x060058C7 RID: 22727 RVA: 0x00116E1F File Offset: 0x0011501F
			// (set) Token: 0x060058C8 RID: 22728 RVA: 0x00116E37 File Offset: 0x00115037
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			public DateTime QueueProcessingTime
			{
				get
				{
					return (DateTime)base[this.tableStatus.QueueProcessingTimeColumn];
				}
				set
				{
					base[this.tableStatus.QueueProcessingTimeColumn] = value;
				}
			}

			// Token: 0x17001C31 RID: 7217
			// (get) Token: 0x060058C9 RID: 22729 RVA: 0x00116E50 File Offset: 0x00115050
			// (set) Token: 0x060058CA RID: 22730 RVA: 0x00116E68 File Offset: 0x00115068
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			public DateTime QueueCompletedTime
			{
				get
				{
					return (DateTime)base[this.tableStatus.QueueCompletedTimeColumn];
				}
				set
				{
					base[this.tableStatus.QueueCompletedTimeColumn] = value;
				}
			}

			// Token: 0x17001C32 RID: 7218
			// (get) Token: 0x060058CB RID: 22731 RVA: 0x00116E81 File Offset: 0x00115081
			// (set) Token: 0x060058CC RID: 22732 RVA: 0x00116E99 File Offset: 0x00115099
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			public DateTime QueueWakeupTime
			{
				get
				{
					return (DateTime)base[this.tableStatus.QueueWakeupTimeColumn];
				}
				set
				{
					base[this.tableStatus.QueueWakeupTimeColumn] = value;
				}
			}

			// Token: 0x17001C33 RID: 7219
			// (get) Token: 0x060058CD RID: 22733 RVA: 0x00116EB2 File Offset: 0x001150B2
			// (set) Token: 0x060058CE RID: 22734 RVA: 0x00116ECA File Offset: 0x001150CA
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			public int WaitTime
			{
				get
				{
					return (int)base[this.tableStatus.WaitTimeColumn];
				}
				set
				{
					base[this.tableStatus.WaitTimeColumn] = value;
				}
			}

			// Token: 0x17001C34 RID: 7220
			// (get) Token: 0x060058CF RID: 22735 RVA: 0x00116EE3 File Offset: 0x001150E3
			// (set) Token: 0x060058D0 RID: 22736 RVA: 0x00116EFB File Offset: 0x001150FB
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			public int GroupPriority
			{
				get
				{
					return (int)base[this.tableStatus.GroupPriorityColumn];
				}
				set
				{
					base[this.tableStatus.GroupPriorityColumn] = value;
				}
			}

			// Token: 0x17001C35 RID: 7221
			// (get) Token: 0x060058D1 RID: 22737 RVA: 0x00116F14 File Offset: 0x00115114
			// (set) Token: 0x060058D2 RID: 22738 RVA: 0x00116F58 File Offset: 0x00115158
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			public int GroupState
			{
				get
				{
					int result;
					try
					{
						result = (int)base[this.tableStatus.GroupStateColumn];
					}
					catch (InvalidCastException innerException)
					{
						throw new StrongTypingException("The value for column 'GroupState' in table 'Status' is DBNull.", innerException);
					}
					return result;
				}
				set
				{
					base[this.tableStatus.GroupStateColumn] = value;
				}
			}

			// Token: 0x17001C36 RID: 7222
			// (get) Token: 0x060058D3 RID: 22739 RVA: 0x00116F71 File Offset: 0x00115171
			// (set) Token: 0x060058D4 RID: 22740 RVA: 0x00116F89 File Offset: 0x00115189
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			public int CorrelationPriority
			{
				get
				{
					return (int)base[this.tableStatus.CorrelationPriorityColumn];
				}
				set
				{
					base[this.tableStatus.CorrelationPriorityColumn] = value;
				}
			}

			// Token: 0x17001C37 RID: 7223
			// (get) Token: 0x060058D5 RID: 22741 RVA: 0x00116FA2 File Offset: 0x001151A2
			// (set) Token: 0x060058D6 RID: 22742 RVA: 0x00116FBA File Offset: 0x001151BA
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			public Guid CorrelationGUID
			{
				get
				{
					return (Guid)base[this.tableStatus.CorrelationGUIDColumn];
				}
				set
				{
					base[this.tableStatus.CorrelationGUIDColumn] = value;
				}
			}

			// Token: 0x17001C38 RID: 7224
			// (get) Token: 0x060058D7 RID: 22743 RVA: 0x00116FD3 File Offset: 0x001151D3
			// (set) Token: 0x060058D8 RID: 22744 RVA: 0x00116FEB File Offset: 0x001151EB
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			public Guid JobGUID
			{
				get
				{
					return (Guid)base[this.tableStatus.JobGUIDColumn];
				}
				set
				{
					base[this.tableStatus.JobGUIDColumn] = value;
				}
			}

			// Token: 0x17001C39 RID: 7225
			// (get) Token: 0x060058D9 RID: 22745 RVA: 0x00117004 File Offset: 0x00115204
			// (set) Token: 0x060058DA RID: 22746 RVA: 0x0011701C File Offset: 0x0011521C
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			public Guid JobGroupGUID
			{
				get
				{
					return (Guid)base[this.tableStatus.JobGroupGUIDColumn];
				}
				set
				{
					base[this.tableStatus.JobGroupGUIDColumn] = value;
				}
			}

			// Token: 0x17001C3A RID: 7226
			// (get) Token: 0x060058DB RID: 22747 RVA: 0x00117035 File Offset: 0x00115235
			// (set) Token: 0x060058DC RID: 22748 RVA: 0x0011704D File Offset: 0x0011524D
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			public Guid JobInfoGUID
			{
				get
				{
					return (Guid)base[this.tableStatus.JobInfoGUIDColumn];
				}
				set
				{
					base[this.tableStatus.JobInfoGUIDColumn] = value;
				}
			}

			// Token: 0x17001C3B RID: 7227
			// (get) Token: 0x060058DD RID: 22749 RVA: 0x00117066 File Offset: 0x00115266
			// (set) Token: 0x060058DE RID: 22750 RVA: 0x0011707E File Offset: 0x0011527E
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			public Guid ResourceGUID
			{
				get
				{
					return (Guid)base[this.tableStatus.ResourceGUIDColumn];
				}
				set
				{
					base[this.tableStatus.ResourceGUIDColumn] = value;
				}
			}

			// Token: 0x17001C3C RID: 7228
			// (get) Token: 0x060058DF RID: 22751 RVA: 0x00117097 File Offset: 0x00115297
			// (set) Token: 0x060058E0 RID: 22752 RVA: 0x001170B9 File Offset: 0x001152B9
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			public string MachineName
			{
				get
				{
					if (this.IsMachineNameNull())
					{
						return null;
					}
					return (string)base[this.tableStatus.MachineNameColumn];
				}
				set
				{
					base[this.tableStatus.MachineNameColumn] = value;
				}
			}

			// Token: 0x17001C3D RID: 7229
			// (get) Token: 0x060058E1 RID: 22753 RVA: 0x001170CD File Offset: 0x001152CD
			// (set) Token: 0x060058E2 RID: 22754 RVA: 0x001170EF File Offset: 0x001152EF
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			public string ServiceName
			{
				get
				{
					if (this.IsServiceNameNull())
					{
						return null;
					}
					return (string)base[this.tableStatus.ServiceNameColumn];
				}
				set
				{
					base[this.tableStatus.ServiceNameColumn] = value;
				}
			}

			// Token: 0x17001C3E RID: 7230
			// (get) Token: 0x060058E3 RID: 22755 RVA: 0x00117103 File Offset: 0x00115303
			// (set) Token: 0x060058E4 RID: 22756 RVA: 0x00117125 File Offset: 0x00115325
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			public byte[] QueueLocalStorage
			{
				get
				{
					if (this.IsQueueLocalStorageNull())
					{
						return null;
					}
					return (byte[])base[this.tableStatus.QueueLocalStorageColumn];
				}
				set
				{
					base[this.tableStatus.QueueLocalStorageColumn] = value;
				}
			}

			// Token: 0x17001C3F RID: 7231
			// (get) Token: 0x060058E5 RID: 22757 RVA: 0x0011713C File Offset: 0x0011533C
			// (set) Token: 0x060058E6 RID: 22758 RVA: 0x00117180 File Offset: 0x00115380
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			public Guid ServerId
			{
				get
				{
					Guid result;
					try
					{
						result = (Guid)base[this.tableStatus.ServerIdColumn];
					}
					catch (InvalidCastException innerException)
					{
						throw new StrongTypingException("The value for column 'ServerId' in table 'Status' is DBNull.", innerException);
					}
					return result;
				}
				set
				{
					base[this.tableStatus.ServerIdColumn] = value;
				}
			}

			// Token: 0x060058E7 RID: 22759 RVA: 0x00117199 File Offset: 0x00115399
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			public bool IsErrorInfoNull()
			{
				return base.IsNull(this.tableStatus.ErrorInfoColumn);
			}

			// Token: 0x060058E8 RID: 22760 RVA: 0x001171AC File Offset: 0x001153AC
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public void SetErrorInfoNull()
			{
				base[this.tableStatus.ErrorInfoColumn] = Convert.DBNull;
			}

			// Token: 0x060058E9 RID: 22761 RVA: 0x001171C4 File Offset: 0x001153C4
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public bool IsGroupStateNull()
			{
				return base.IsNull(this.tableStatus.GroupStateColumn);
			}

			// Token: 0x060058EA RID: 22762 RVA: 0x001171D7 File Offset: 0x001153D7
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			public void SetGroupStateNull()
			{
				base[this.tableStatus.GroupStateColumn] = Convert.DBNull;
			}

			// Token: 0x060058EB RID: 22763 RVA: 0x001171EF File Offset: 0x001153EF
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			public bool IsMachineNameNull()
			{
				return base.IsNull(this.tableStatus.MachineNameColumn);
			}

			// Token: 0x060058EC RID: 22764 RVA: 0x00117202 File Offset: 0x00115402
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public void SetMachineNameNull()
			{
				base[this.tableStatus.MachineNameColumn] = Convert.DBNull;
			}

			// Token: 0x060058ED RID: 22765 RVA: 0x0011721A File Offset: 0x0011541A
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			public bool IsServiceNameNull()
			{
				return base.IsNull(this.tableStatus.ServiceNameColumn);
			}

			// Token: 0x060058EE RID: 22766 RVA: 0x0011722D File Offset: 0x0011542D
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			public void SetServiceNameNull()
			{
				base[this.tableStatus.ServiceNameColumn] = Convert.DBNull;
			}

			// Token: 0x060058EF RID: 22767 RVA: 0x00117245 File Offset: 0x00115445
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public bool IsQueueLocalStorageNull()
			{
				return base.IsNull(this.tableStatus.QueueLocalStorageColumn);
			}

			// Token: 0x060058F0 RID: 22768 RVA: 0x00117258 File Offset: 0x00115458
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public void SetQueueLocalStorageNull()
			{
				base[this.tableStatus.QueueLocalStorageColumn] = Convert.DBNull;
			}

			// Token: 0x060058F1 RID: 22769 RVA: 0x00117270 File Offset: 0x00115470
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public bool IsServerIdNull()
			{
				return base.IsNull(this.tableStatus.ServerIdColumn);
			}

			// Token: 0x060058F2 RID: 22770 RVA: 0x00117283 File Offset: 0x00115483
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			public void SetServerIdNull()
			{
				base[this.tableStatus.ServerIdColumn] = Convert.DBNull;
			}

			// Token: 0x0400127B RID: 4731
			private QueueStatusDataSet.StatusDataTable tableStatus;
		}

		// Token: 0x02000363 RID: 867
		[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
		public class StatusRowChangeEvent : EventArgs
		{
			// Token: 0x060058F3 RID: 22771 RVA: 0x0011729B File Offset: 0x0011549B
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			public StatusRowChangeEvent(QueueStatusDataSet.StatusRow row, DataRowAction action)
			{
				this.eventRow = row;
				this.eventAction = action;
			}

			// Token: 0x17001C40 RID: 7232
			// (get) Token: 0x060058F4 RID: 22772 RVA: 0x001172B1 File Offset: 0x001154B1
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public QueueStatusDataSet.StatusRow Row
			{
				get
				{
					return this.eventRow;
				}
			}

			// Token: 0x17001C41 RID: 7233
			// (get) Token: 0x060058F5 RID: 22773 RVA: 0x001172B9 File Offset: 0x001154B9
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public DataRowAction Action
			{
				get
				{
					return this.eventAction;
				}
			}

			// Token: 0x0400127C RID: 4732
			private QueueStatusDataSet.StatusRow eventRow;

			// Token: 0x0400127D RID: 4733
			private DataRowAction eventAction;
		}
	}
}
