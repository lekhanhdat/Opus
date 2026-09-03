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
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Xml.Schema;
using System.Xml.Serialization;

namespace Native13NinTexWorkflowEntity
{
    [Serializable, DesignerCategory("code"), GeneratedCode("xsd", "2.0.50727.42"), Obfuscation]
    public class NWActionConfig : ICloneable
    {
        // Fields
        private string _differentiator;
        private List<ExtensionProperty> _extensionProperties;
        private NWActionConfig _parent;
        private string _requiredUserContext;
        private bool _requiredUserContextSpecified;
        private ToolboxData _toolboxInfo;
        private NWApprover[] approversField;
        private string assemblyField;
        private AssociationColumnCollection associationColumns;
        private bool bCustLblField;
        private string bLabelField;
        private NWActionConfig[] childActivitiesField;
        private CommentType commentType;
        private ConditionConfig conditionField;
        private ConditionUse conditionUseField;
        private string customCommentsField;
        private List<string> customWorkflowStatusesField;
        private bool enabled;
        private ErrorHandling errorHandling;
        private int expectedDuration;
        private NWFieldReference[] fieldReferencesField;
        private bool hasDefaultMessageField;
        private bool hideUI;
        private string historyLogMessageField;
        private bool inheritedEnabled;
        private bool isValidField;
        private bool lCustLblField;
        private string lLabelField;
        private bool logMessageField;
        private Message messageField;
        private ConfiguredOutcomeCollection outcomes;
        private ActivityParameter[] parametersField;
        private bool rCustLblField;
        private string rLabelField;
        private bool showCustomComments;
        private bool tCustLblField;
        private string tLabelField;
        private string typeField;
        private MultiOutputValueInfo valueStorageCollectionForWorkflow;
        private NWWorkflowVariable[] workflowVariablesField;

        // Properties
        [XmlArrayItem("Approver", Form = XmlSchemaForm.Unqualified, IsNullable = false), XmlArray(Form = XmlSchemaForm.Unqualified)]
        public NWApprover[] Approvers
        {
            get
            {
                return this.approversField;
            }
            set
            {
                this.approversField = value;
            }
        }

        [XmlElement(Form = XmlSchemaForm.Unqualified)]
        public string Assembly
        {
            get
            {
                return this.assemblyField;
            }
            set
            {
                this.assemblyField = value;
            }
        }

        [XmlArrayItem("AssociationColumn", Form = XmlSchemaForm.Unqualified, IsNullable = true), XmlArray(Form = XmlSchemaForm.Unqualified)]
        public AssociationColumnCollection AssociationColumns
        {
            get
            {
                return this.associationColumns;
            }
            set
            {
                this.associationColumns = value;
            }
        }

        [XmlAttribute]
        public bool BCustLbl
        {
            get
            {
                return this.bCustLblField;
            }
            set
            {
                this.bCustLblField = value;
            }
        }

        [XmlElement(Form = XmlSchemaForm.Unqualified)]
        public string BLabel
        {
            get
            {
                return this.bLabelField;
            }
            set
            {
                this.bLabelField = value;
            }
        }

        [XmlArrayItem(Form = XmlSchemaForm.Unqualified, IsNullable = false), XmlArray(Form = XmlSchemaForm.Unqualified)]
        public NWActionConfig[] ChildActivities
        {
            get
            {
                return this.childActivitiesField;
            }
            set
            {
                this.childActivitiesField = value;
            }
        }

        [XmlAttribute]
        public CommentType CommentType
        {
            get
            {
                return this.commentType;
            }
            set
            {
                this.commentType = value;
            }
        }

        [XmlElement(Form = XmlSchemaForm.Unqualified)]
        public ConditionConfig Condition
        {
            get
            {
                return this.conditionField;
            }
            set
            {
                this.conditionField = value;
            }
        }

        [XmlElement(Form = XmlSchemaForm.Unqualified)]
        public ConditionUse ConditionUse
        {
            get
            {
                return this.conditionUseField;
            }
            set
            {
                this.conditionUseField = value;
            }
        }

        [XmlElement(Form = XmlSchemaForm.Unqualified)]
        public string CustomComments
        {
            get
            {
                return this.customCommentsField;
            }
            set
            {
                this.customCommentsField = value;
            }
        }

        [XmlArrayItem("WorkflowStatus", Form = XmlSchemaForm.Unqualified, IsNullable = true), XmlArray(Form = XmlSchemaForm.Unqualified)]
        public List<string> CustomWorkflowStatuses
        {
            get
            {
                return this.customWorkflowStatusesField;
            }
            set
            {
                this.customWorkflowStatusesField = value;
            }
        }

        [XmlElement(Form = XmlSchemaForm.Unqualified)]
        public string Differentiator
        {
            get
            {
                return this._differentiator;
            }
            set
            {
                this._differentiator = value;
            }
        }

        [XmlElement(Form = XmlSchemaForm.Unqualified)]
        public bool Enabled
        {
            get
            {
                return this.enabled;
            }
            set
            {
                this.enabled = value;
            }
        }

        [XmlElement("ErrorHandling", Form = XmlSchemaForm.Unqualified)]
        public ErrorHandling ErrorHandling
        {
            get
            {
                return this.errorHandling;
            }
            set
            {
                this.errorHandling = value;
            }
        }

        [XmlElement(Form = XmlSchemaForm.Unqualified)]
        public int ExpectedDuration
        {
            get
            {
                return this.expectedDuration;
            }
            set
            {
                this.expectedDuration = value;
            }
        }

        [XmlArrayItem("ExtensionProperty", Form = XmlSchemaForm.Unqualified, IsNullable = false), XmlArray(Form = XmlSchemaForm.Unqualified)]
        public List<ExtensionProperty> ExtensionProperties
        {
            get
            {
                return this._extensionProperties;
            }
            set
            {
                this._extensionProperties = value;
            }
        }

        [XmlArrayItem("FieldReference", Form = XmlSchemaForm.Unqualified, IsNullable = false), XmlArray(Form = XmlSchemaForm.Unqualified)]
        public NWFieldReference[] FieldReferences
        {
            get
            {
                return this.fieldReferencesField;
            }
            set
            {
                this.fieldReferencesField = value;
            }
        }

        [XmlElement(Form = XmlSchemaForm.Unqualified)]
        public bool HasDefaultMessage
        {
            get
            {
                return this.hasDefaultMessageField;
            }
            set
            {
                this.hasDefaultMessageField = value;
            }
        }

        [XmlElement(Form = XmlSchemaForm.Unqualified)]
        public bool HideUI
        {
            get
            {
                return this.hideUI;
            }
            set
            {
                this.hideUI = value;
            }
        }

        [XmlElement(Form = XmlSchemaForm.Unqualified, ElementName = "HistoryNote")]
        public string HistoryLogMessage
        {
            get
            {
                return this.historyLogMessageField;
            }
            set
            {
                this.historyLogMessageField = value;
            }
        }

        public bool InheritedEnabled
        {
            get
            {
                return this.inheritedEnabled;
            }
            set
            {
                this.inheritedEnabled = value;
            }
        }

        [XmlElement(Form = XmlSchemaForm.Unqualified)]
        public bool IsValid
        {
            get
            {
                return this.isValidField;
            }
            set
            {
                this.isValidField = value;
            }
        }

        [XmlAttribute]
        public bool LCustLbl
        {
            get
            {
                return this.lCustLblField;
            }
            set
            {
                this.lCustLblField = value;
            }
        }

        [XmlElement(Form = XmlSchemaForm.Unqualified)]
        public string LLabel
        {
            get
            {
                return this.lLabelField;
            }
            set
            {
                this.lLabelField = value;
            }
        }

        [XmlElement(Form = XmlSchemaForm.Unqualified, ElementName = "LogMessage")]
        public bool LogMessage
        {
            get
            {
                return this.logMessageField;
            }
            set
            {
                this.logMessageField = value;
            }
        }

        [XmlElement(Form = XmlSchemaForm.Unqualified)]
        public Message Message
        {
            get
            {
                return this.messageField;
            }
            set
            {
                this.messageField = value;
            }
        }

        [XmlArray(Form = XmlSchemaForm.Unqualified), XmlArrayItem("Outcome", Form = XmlSchemaForm.Unqualified, IsNullable = true)]
        public ConfiguredOutcomeCollection Outcomes
        {
            get
            {
                return this.outcomes;
            }
            set
            {
                this.outcomes = value;
            }
        }

        [XmlArrayItem("Parameter", Form = XmlSchemaForm.Unqualified, IsNullable = false), XmlArray(Form = XmlSchemaForm.Unqualified)]
        public ActivityParameter[] Parameters
        {
            get
            {
                return this.parametersField;
            }
            set
            {
                this.parametersField = value;
            }
        }

        [XmlIgnore]
        public NWActionConfig Parent
        {
            get
            {
                return this._parent;
            }
            set
            {
                this._parent = value;
            }
        }

        [XmlAttribute]
        public bool RCustLbl
        {
            get
            {
                return this.rCustLblField;
            }
            set
            {
                this.rCustLblField = value;
            }
        }

        [XmlElement(Form = XmlSchemaForm.Unqualified)]
        public string RequiredUserContext
        {
            get
            {
                return this._requiredUserContext;
            }
            set
            {
                this._requiredUserContext = value;
            }
        }

        [XmlIgnore]
        public bool RequiredUserContextSpecified
        {
            get
            {
                return this._requiredUserContextSpecified;
            }
            set
            {
                this._requiredUserContextSpecified = value;
            }
        }

        [XmlElement(Form = XmlSchemaForm.Unqualified)]
        public string RLabel
        {
            get
            {
                return this.rLabelField;
            }
            set
            {
                this.rLabelField = value;
            }
        }


        [XmlAttribute]
        public bool ShowCustomComments
        {
            get
            {
                return this.showCustomComments;
            }
            set
            {
                this.showCustomComments = value;
            }
        }


        [XmlAttribute]
        public bool TCustLbl
        {
            get
            {
                return this.tCustLblField;
            }
            set
            {
                this.tCustLblField = value;
            }
        }

        [XmlElement(Form = XmlSchemaForm.Unqualified)]
        public string TLabel
        {
            get
            {
                return this.tLabelField;
            }
            set
            {
                this.tLabelField = value;
            }
        }

        public ToolboxData ToolboxInfo
        {
            get
            {
                return this._toolboxInfo;
            }
            set
            {
                this._toolboxInfo = value;
            }
        }

        [XmlElement(Form = XmlSchemaForm.Unqualified)]
        public string Type
        {
            get
            {
                return this.typeField;
            }
            set
            {
                this.typeField = value;
            }
        }


        [XmlElement(Form = XmlSchemaForm.Unqualified, ElementName = "ValueStorages")]
        public MultiOutputValueInfo ValueStorageCollection
        {
            get
            {
                return this.valueStorageCollectionForWorkflow;
            }
            set
            {
                this.valueStorageCollectionForWorkflow = value;
            }
        }

        [XmlArray(Form = XmlSchemaForm.Unqualified), XmlArrayItem("Variable", Form = XmlSchemaForm.Unqualified, IsNullable = false)]
        public NWWorkflowVariable[] WorkflowVariables
        {
            get
            {
                return this.workflowVariablesField;
            }
            set
            {
                this.workflowVariablesField = value;
            }
        }

        public object Clone()
        {
            throw new NotImplementedException();
        }
    }


}
