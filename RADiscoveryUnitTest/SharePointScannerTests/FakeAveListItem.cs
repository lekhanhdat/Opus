using System;
using System.Collections;
using System.Collections.Generic;
using AvePoint.Wrapper.Common;
using Microsoft.SharePoint.Client;

namespace RADiscoveryUnitTest.SharePointScannerTests
{
    /// <summary>
    /// Minimal fake implementation of IAveListItem for testing scanner rule evaluation.
    /// Only properties accessed by CheckItemCriteria and FilterAnalyser are implemented.
    /// </summary>
    public class FakeAveListItem : IAveListItem
    {
        private readonly Dictionary<string, object> _fieldValues = new();
        private readonly Dictionary<Guid, object> _fieldValuesByGuid = new();

        // Properties used by CheckItemCriteria
        public string Title { get; set; } = "TestItem";
        public string Name { get; set; } = "TestItem.docx";
        public string Url { get; set; } = "Shared Documents/TestItem.docx";
        public int ID { get; set; } = 1;
        public Guid UniqueId { get; set; } = Guid.NewGuid();
        public IAveList ParentList { get; set; }
        public IAveContentType ContentType { get; set; }
        public IAveFieldCollection Fields { get; set; }
        public IAveWeb Web { get; set; }
        public IAveFile File { get; set; }
        public Hashtable Properties { get; set; } = new();
        public Dictionary<string, object> FieldValues
        {
            get => _fieldValues;
            set
            {
                _fieldValues.Clear();
                if (value != null)
                {
                    foreach (var kvp in value)
                    {
                        _fieldValues[kvp.Key] = kvp.Value;
                    }
                }
            }
        }
        public IAveListItemVersionCollection Versions { get; set; }

        // Indexer for field access (used extensively by FilterAnalyser)
        public object this[string fieldName]
        {
            get => _fieldValues.TryGetValue(fieldName, out var val) ? val : null;
            set => _fieldValues[fieldName] = value;
        }

        public object this[Guid fieldId]
        {
            get => _fieldValuesByGuid.TryGetValue(fieldId, out var val) ? val : null;
            set => _fieldValuesByGuid[fieldId] = value;
        }

        public void SetFieldValue(Guid fieldId, object value)
        {
            _fieldValuesByGuid[fieldId] = value;
        }

        #region Not needed for rule checking

        public string DisplayName => Title;
        public AveBasePermissions EffectiveBasePermissions => default;
        public IAveFieldStringValues FieldValuesAsHtml => null;
        public IAveFieldStringValues FieldValuesAsText => null;
        public IAveFieldStringValues FieldValuesForEdit => null;
        public IAveFile BackupFile => null;
        public AveFileSystemObjectType FileSystemObjectType => AveFileSystemObjectType.File;
        public IAveAttachmentCollection Attachments => null;
        public AveFileLevel Level => AveFileLevel.Published;
        public IAveFolder Folder => null;
        public IAveModerationInformation ModerationInformation => null;
        public string Xml => null;
        public IAveAudit Audit => null;
        public AveDictionary<Guid, AveSharingLinkInfo> SharingLinks => null;
        public AveCommentsDisabledScope CommentsDisabledScope => default;
        public bool CommentsDisabled => false;
        public IAveUser Author => null;
        public IAveUser ModifiedBy => null;
        public IAveWorkflowCollection WorkFlows => null;

        // IAveSecurableObject
        public bool HasUniqueRoleAssignments => false;
        public IAveRoleAssignmentCollection RoleAssignments => null;
        public IAveSecurableObjectImpl SecurableObjectImpl => null;
        public void BreakRoleInheritance(bool copyRoleAssignments, bool clearSubscopes) { }
        public void BreakRoleInheritance(bool copyRoleAssignments) { }
        public bool DoesUserHavePermissions(AveBasePermissions permissionMask) => true;
        public void ResetRoleInheritance() { }
        public IAvePermissionInfo GetUserEffectivePermissionInfo(string userName) => null;
        public AveBasePermissions GetUserEffectivePermissions(string userName) => default;

        // Methods
        public Guid Recycle() => Guid.Empty;
        public void SystemUpdate(bool incrementListItemVersion) { }
        public void SystemUpdate() { }
        public void SystemUpdateForProps(Dictionary<string, object> itemProperties) { }
        public void SystemUpdateForRecords() { }
        public void Delete() { }
        public void Update() { }
        public void UpdateOverwriteVersion() { }
        public void UpdateInternal(Type[] argsTypes, object[] args) { }
        public void SetValue(Type[] argsTypes, object[] args) { }
        public int GetTpIdByTpGuid(Guid tp_guid, Guid listId) => 0;
        public Guid GetTPGuid() => Guid.Empty;
        public ListItemComplianceInfo GetComplianceInfo(bool useCache = false) => null;
        public void LockRecordItem() { }
        public void UnlockRecordItem() { }
        public void SetComplianceTag(string complianceTag, bool isTagPolicyHold, bool isTagPolicyRecord, bool isEventBasedTag, bool isTagSuperLock) { }
        public void SetComplianceTag(string complianceTag, bool isTagPolicyHold, bool isTagPolicyRecord, bool isEventBasedTag, bool isTagSuperLock, bool unlockedAsDefault) { }
        public void SetComplianceTag(string complianceTag, bool blockDel, bool blockEdit, DateTime complianceWrittenTime = default, string userEmail = default, bool isTagSuperLock = false) { }
        public void SetComplianceTagOnBulkItems(string complianceTagValue) { }
        public DateTime GetLastAccessTime(Guid id, string folderServerRelativeUrl, DateTime modified, bool isCompatibleByModifiedTime = false) => DateTime.MinValue;

        #endregion
    }
}
