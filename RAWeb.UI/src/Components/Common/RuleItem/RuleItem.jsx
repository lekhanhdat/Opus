import * as Constants from "./Components/Constants";
import { withRouter } from 'react-router-dom';
import { bindEvents, setCheckedStatus, LicenseHelper, getBaseUserGuidUrl, EnvironmentHelper } from "../../../Utilities/CommonUtil";
import { RuleOperatedType, SourceFlags, TelemetryModule, TelemetryEventType } from "../../../Constants/Constants";
import "../../../Less/RDM/createRule.less";
import SPDestinationTree from "../Tree/Instances/SPTree/SPDestinationTree";
import RuleCriteria from "./Components/RuleCriteria.jsx";
import EnableManualApproval from "./Components/EnableManualApproval.jsx";
import Export, { ExportLocationOption } from "./Components/Export.jsx";
import RemoveAchived from "./Components/RemoveAchived.jsx";
import StorageSettings from "./Components/StorageSettings.jsx";
// import PhysicalExplorerTree from '../../Common/Tree/Instances/PhysicalExplorerTree';
import RuleMove from "./Components/RuleMove.jsx";
import PhysicalRuleMoveTree from '../../../Components/Common/Tree/Instances/Physical/PhyDestinationTree';
import { NodeType } from "../../../Constants/DAEnums";
import RouterUrls from "../../../Constants/RouterUrls";
import { addTelemetryRecord } from "../../../Utilities/TelemetryUtil";
import { showToast } from "../../../Utilities/CommonUtil";
import { RAMessageType } from "../../BCM/ContentRepositoryManagement/Common/CRMCommonUtil";
import { RuleModuleTypes } from "./Components/Constants";
import { object } from "prop-types";
import Enviroments from "../../../Constants/Enviroments";
import ExoMoveToSP from "./Components/ExoMoveToSP";
import RuleRetention from "./Components/RuleRetention";
import GoogleDestinationTree from "../Tree/Instances/GoogleTree/GoogleDestinationTree.jsx";
import GoogleExport from "./Components/GoogleExport.jsx";
import { TabIndex } from "../../BCM/ContentRepositoryManagement/CRMForSPO.jsx";
import TeamsDestinationTree from "../Tree/Instances/TeamsTree/TeamsDestinationTree.jsx";
import { checkPermission } from "../../../Utilities/permissionManager.jsx";
import { DataSourceType } from "../../ArchiveRC/Constants/index.js";
import { productKeys } from "../../../Utilities/Constant.js";
import StubPanel from "../../CP/StubSettings/StubPanel.jsx";

const msgBoxContentType = {
    AssociateTerms: 1,
    CheckMoveCrossSecurityGroup: 2
};

const ruleActionType = {
    Remove: 1,
    ArchiveToAzureBlobStorage: 2
};

class CreateRule extends R.Component {
    idAttr = true;
    componentCreate () {
        this.enableRecordsArchiver = LicenseHelper.EnableRecordsArchiver();
        this.is21VEnv = LicenseHelper.Is21VEnv();
        this.isGccEnv = EnvironmentHelper.IsGovAzureEnv;
        this.ruleId = this.props.ruleId;
        this.levelId = 64;
        this.RuleLevel = Constants.RuleLevel;
        this.isSpSourceChecked = false;
        this.isSPLocalSourceChecked = false;
        this.spApprovalIsPassed = true;
        this.oneDriveApprovalIsPassed = true;
        this.teamsApprovalIsPassed = true;
        this.isExoSourceChecked = false;
        this.isPhySourceChecked = false;
        this.isFsSourceChecked = false;
        this.isOneDriveSourceChecked = false;   
        this.isAzureFileSourceChecked = false;                                      //判断选择了哪个tab
        this.isBoxSourceChecked = false;
        this.IsGoogleDriveSourceChecked = false;
        this.isTeamsSourceChecked = false;
        this.isConnectorSourceChecked = false;
        this.spIsVerificationPassed = false;                                    //rule criteria 验证（exo）
        this.spLocalIsVerificationPassed = false;
        this.exoIsVerificationPassed = false;                                   //rule criteria 验证（sp）
        this.phyIsVerificationPassed = false;
        this.fsIsVerificationPassed = false;
        this.oneDriveIsVerificationPassed = false;
        this.teamsIsVerificationPassed = false;
        this.exoExportIsPassed = false;
        this.fsExportIsPassed = false;
        this.spExportIsPassed = false;
        this.spExportLocationIsPassed = false;
        this.oneDriveExportLocationIsPassed = false;
        this.teamsExportIsPassed = false;
        this.teamsExportLocationIsPassed = false;
        this.exoExportLocationIsPassed = false;
        this.spLocalExportIsPassed = false;
        this.spCriteriaData = null;
        this.spLocalCriteriaData = null;
        this.exoCriteriaData = null;
        this.fsCriteriaData = null;
        this.phyCriteriaData = null;
        this.oneDriveCriteriaData = null;
        this.azureFileCriteriaData = null;
        this.boxCriteriaData = null;
        this.googleDriveCriteriaData = null;
        this.teamsCriteriaData = null;
        this.ruleMoveOfFsParam = {};
        this.ruleMoveOfFsValid = true;
        this.TagType = Constants.TagType;
        this.TagMode = Constants.TagMode;
        this.ExportSPDataOption = Constants.ExportSPDataOption;
        this.spExportData = {};
        this.spLocalExportData = {};
        this.oneDriveExportData = {};
        this.teamsExportData = {};
        this.exoExportData = {};
        this.phyExportData = {};
        this.fsExportData = {};
        this.spApprovalData = {};
        this.spLocalApprovalData = {};
        this.oneDriveApprovalData = {};
        this.teamsApprovalData = {};
        this.exoApprovalData = {};
        this.phyApprovalData = {};
        this.fsApprovalData = {};
        this.azureFileApprovalData = {};
        this.boxApprovalData = {};
        this.googleDriveApprovalData = {};
        this.spNodeItem = null;
        this.spLocalNodeItem = null;
        this.oneDriveNodeItem = null;
        this.teamsNodeItem = null;
        this.exoNodeItem = null;
        this.googleNodeItem = null;
        this.googleExportData = {};
        this.googleExportIsPassed = false;
        this.googleExportLocationIsPassed = false;
        this.hasChanged = false;
        this.dateTimeFormat = RM.TimeUtil.getGlobalAuiFormat();
        this.TrueOrFaseOptions = [{ id: 0, Name: "Yes" }, { id: 1, Name: "No" }];
        this.moveLocationPasswordPlaceholder = RM.Constant.passwordPlaceholder;
        this.selectedPhyTreeItem = null;
        this.ruleNameMaxLength = 255;
        this.initialRuleMode = {
            RuleName: "",
            RuleLevel: 64,
            Description: "",
            DisposalClass: "",
            ArchiverActions: "",
            isChecked: false,
            DeleteRecords: false,
            IncludeDeleteRecordLabel: false,
            LockRecordBeforeDestroy: true,
            ExportDataBeforeArchiving: false,
            EnableExport: false,
            ExportFormat: "",
            ExportInfo: {
                exportLocationId: "",
                exportLocationName: "",
                exportSPDataOption: this.ExportSPDataOption.None,
                exportType: -1
            },
            RuleKeepDataOption: 0,
            RelatedRecordOption: 0,
            ArchiverRuleSettingType: "",
            EnableManualApproval: false,
            SequenceNo: 0,
            RuleFilters: [],
            MoveToRecordCenterSettings: {
                ContentConflictResolution: 0,
                DestinationLocation: {
                    Password: "",
                    Url: "",
                    UserName: ""
                },
                OperateDataMode: 0,
                OriginalMetaDataAsXML: false,
                UseTransferedFileMode: 0
            },
            TagContentInfo: [],
            Modified: new Date(),
            MoveDto: null,
            // FSRule:
            FSRule: {
                MoveDto: null,
                ArchiverActions: "",
                EnableManualApproval: false,
                ExportDataBeforeArchiving: false,
                EnableExport: false,
                ExportFormat: "",
                ExportInfo: {
                    exportLocationId: "",
                    exportLocationName: "",
                    exportSPDataOption: 1,
                    exportType: -1
                },
                RuleKeepDataOption: 0,
                RuleFilters: [],
                RuleLevel: '1048576',
            },
            EXORule: {
                RuleLevel: 64,
                ArchiverActions: "",
                isChecked: false,
                ExportDataBeforeArchiving: false,
                EnableExport: false,
                ExportFormat: "",
                ExportInfo: {
                    exportLocationId: "",
                    exportLocationName: "",
                    exportSPDataOption: this.ExportSPDataOption.None,
                    exportType: -1
                },
                RuleKeepDataOption: 0,
                ArchiverRuleSettingType: "",
                EnableManualApproval: false,
                SequenceNo: 0,
                RuleFilters: []
            },
            PhysicalRule: {
                RuleLevel: Constants.phyLevelIds.PhysicalBox,
                ArchiverActions: "",
                isChecked: false,
                ExportDataBeforeArchiving: false,
                EnableExport: false,
                ExportFormat: "",
                ExportInfo: {
                    exportLocationId: "",
                    exportLocationName: "",
                    exportSPDataOption: 1,
                    exportType: -1
                },
                RuleKeepDataOption: 0,
                ArchiverRuleSettingType: "",
                EnableManualApproval: false,
                SequenceNo: 0,
                RuleFilters: [],
                MoveDto: null,
            },
            SPLocalRule: {
                RuleLevel: 64,
                ArchiverActions: "",
                isChecked: false,
                DeleteRecords: false,
                ExportDataBeforeArchiving: false,
                EnableExport: false,
                ExportFormat: "",
                ExportInfo: {
                    exportLocationId: "",
                    exportLocationName: "",
                    exportSPDataOption: this.ExportSPDataOption.None,
                    exportType: -1
                },
                RuleKeepDataOption: 0,
                RelatedRecordOption: 0,
                ArchiverRuleSettingType: "",
                EnableManualApproval: false,
                SequenceNo: 0,
                RuleFilters: [],
                // MoveToRecordCenterSettings: {
                //     ContentConflictResolution: 0,
                //     DestinationLocation: {
                //         Url: "",
                //         UserName: ""
                //     },
                //     OperateDataMode: 0,
                //     OriginalMetaDataAsXML: false,
                //     UseTransferedFileMode: 0
                // },
                TagContentInfo: [],
                Modified: new Date(),
                MoveDto: null
            },
            OneDriveRule: {
                RuleLevel: 64,
                Description: "",
                DisposalClass: "",
                ArchiverActions: "",
                isChecked: false,
                DeleteRecords: false,
                LockRecordBeforeDestroy: true,
                ExportDataBeforeArchiving: false,
                EnableExport: false,
                ExportFormat: "",
                ExportInfo: {
                    exportLocationId: "",
                    exportLocationName: "",
                    exportSPDataOption: this.ExportSPDataOption.None,
                    exportType: -1
                },
                RuleKeepDataOption: 0,
                RelatedRecordOption: 0,
                ArchiverRuleSettingType: "",
                EnableManualApproval: false,
                SequenceNo: 0,
                RuleFilters: [],
                MoveToRecordCenterSettings: {
                    ContentConflictResolution: 0,
                    DestinationLocation: {
                        Password: "",
                        Url: "",
                        UserName: ""
                    },
                    OperateDataMode: 0,
                    OriginalMetaDataAsXML: false,
                    UseTransferedFileMode: 0
                },
                TagContentInfo: [],
                Modified: new Date(),
                MoveDto: null,
            },
            AzureFileRule: {
                MoveDto: null,
                ArchiverActions: "",
                EnableManualApproval: false,
                ExportDataBeforeArchiving: false,
                EnableExport: false,
                ExportFormat: "",
                ExportInfo: null,
                RuleKeepDataOption: 0,
                RuleFilters: [],
                RuleLevel: '4194304',
            },
            BoxRule: {
                MoveDto: null,
                ArchiverActions: "",
                EnableManualApproval: false,
                ExportDataBeforeArchiving: false,
                EnableExport: false,
                ExportFormat: "",
                ExportInfo: null,
                RuleKeepDataOption: 0,
                RuleFilters: [],
                RuleLevel: '8388608',
            },
            GoogleDriveRule: {
                MoveDto: null,
                ArchiverActions: "",
                EnableManualApproval: false,
                ExportDataBeforeArchiving: false,
                EnableExport: false,
                ExportFormat: "",
                ExportInfo: {
                    exportLocationId: "",
                    exportLocationName: "",
                    exportSPDataOption: this.ExportSPDataOption.None,
                    exportType: -1
                },
                RuleKeepDataOption: 0,
                RuleFilters: [],
                RuleLevel: '16777216',
            },
            ConnectorRule: {
                MoveDto: null,
                ArchiverActions: "",
                EnableManualApproval: false,
                ExportDataBeforeArchiving: false,
                EnableExport: false,
                ExportFormat: "",
                ExportInfo: null,
                RuleKeepDataOption: 256,
                RuleFilters: [],
                RuleLevel: '64',
            },
            TeamsRule: {
                RuleName: "",
                RuleLevel: 64,
                Description: "",
                DisposalClass: "",
                ArchiverActions: "",
                isChecked: false,
                DeleteRecords: false,
                ExportDataBeforeArchiving: false,
                EnableExport: false,
                ExportFormat: "",
                ExportInfo: {
                    exportLocationId: "",
                    exportLocationName: "",
                    exportSPDataOption: this.ExportSPDataOption.None,
                    exportType: -1
                },
                RuleKeepDataOption: 0,
                RelatedRecordOption: 0,
                ArchiverRuleSettingType: "",
                EnableManualApproval: false,
                SequenceNo: 0,
                RuleFilters: [],
                MoveToRecordCenterSettings: {
                    ContentConflictResolution: 0,
                    DestinationLocation: {
                        Password: "",
                        Url: "",
                        UserName: ""
                    },
                    OperateDataMode: 0,
                    OriginalMetaDataAsXML: false,
                    UseTransferedFileMode: 0
                },
                TagContentInfo: [],
                Modified: new Date(),
                MoveDto: null,
            }
        };
        //save 默认数据
        this.ruleMode = {};
        this.supportingLevelsForRecordsLabel = [Constants.RuleLevel.Document, Constants.RuleLevel.Item];
        this.state = {
            ruleItem: {},
            selectedRuleModuleType: RuleModuleTypes.Records,
            MessageTipInfo: { showTip: false, type: "success", content: "" },
            isSpSourceChecked: false,
            isOneDriveSourceChecked: false,
            isExoSourceChecked: false,
            isPhySourceChecked: false,
            isFsSourceChecked: false,
            isSpLocalSourceChecked: false,
            isAzureFileSourceChecked: false,
            isBoxSourceChecked: false,
            IsGoogleDriveSourceChecked:false,
            isTeamsSourceChecked: false,
            isConnectorSourceChecked: false,
            elementsEnable: false,                                     //点击copy时，控制节点状态 disabled和display
            //isRuleNameDisabled: false,                                      //ruleName disabled
            isIncludeDeclaredDisable: false,
            //noCheckedSource: false,                                    //没选tab的验证
            isKeepShow: true,
            isMoveShow: true,
            showExportOnly: true,
            isStoreInM365ArchiveShow: false,
            isExportOnly: false,                                       //Export without archiving
            isRemove: true,
            isKeep: false,                                             //Record Declaration and Tagging  radio 显示隐藏
            isMove: false,                                             //Move documents to a new destination library  显示隐藏
            isStoreInM365Archive: false,                               //This's new option in June, 2026 release, only support SPO source at the moment
            isArchiveWithoutDestroy: false,                            //Archive without destroy content option (control by flag in BE: update later)
            isDeclare: false,                                          //Remove content from SharePoint and destroy
            isUndeclare: false,                                        //Undeclare in-place record
            isRestoreLink: false,
            isExoRemove: true,
            isExoKeep: false,
            isExoMove: false,
            //isExoMoveDeclare: false,                                      //Declare each document as a SharePoint record when it is moved checked
            isExoMoveDeleteSource: false,
            isKeepClassification: true,
            isExoSpecifyLocation: false,                               //Enter a destination checked
            exoLocationPath: "",                                          //Enter a destination input的value
            noExoLocation: false,          
            noExoSelectNode: false,
            isExoLocationVlidat: false,                                    // Enter a destination 验证
            ExoLocationVlidat: "",                                         // Enter a destination 验证内容
            exoMoveDestTreeData: [],
            exoDestinationTreeData:[],
            isExoExportOnly: false,                                    //Export without archiving
            isExoMoveToSP: false,
            moveToSPDataList: [],

          
            isFsRemove: true,
            isFsMove: false,
            iskeepTag: false,                                          //Tag each document/item with: check
            isTagYes: false,                                           //Record Declaration and Tagging Tag each document/item with: Archived (Yes/No column) checked
            isTagBy: false,                                            //Archived By checked
            isTagTime: false,                                          //Archived Time checked
            metadataName: "",                                          //Custom Metadata input name value
            metadataValue: "",                                         //Custom Metadata input Value value
            tagMetadataChecked: false,                                 //Custom Metadata checked
            isTagText: true,                                           //Custom Metadata: text
            tagTypeValue: 0,
            isDeclaredFile: false,                                     //Include related records
            isDeleteToRecycleBinForSPO: false,
            isIncludeLockedFile: false,
            isLockRecord: true,
            isRetentionLabel: false,                                   //Include retention label
            slectTagType: null,                                        // Custom Metadata 第一个选项框选中的值
            selectTagBoolean: this.TrueOrFaseOptions[0],                //yes no 默认值
            timezones: RM.TimeSettingModel.TimeZoneInfo,               //时区
            currentDate: null,                                           //选中的日期
            isTagDate: false,                                          // 控制日期控制隐藏
            isTagBoolean: false,                                       //Custom Metadata: display
            noMetadateValue: false,                                    //Please select a column type, and enter the column name and value.
            noDateValue: false,                                        //Please specify a time range.
            noNumberValue: false,                                      //The entered value is invalid. Please check and enter again.
            noTags: false,                                             //Please select at least one tagging option.
            noSelect: false,
            noRetentionActionValue: false,
            isLeaveStubOption: false,                                  //Leave a stub in place for each document following disposal checked
            isArchivingRecordOption: false,
            isShowLeaveStubOption: true,                               //是否显示Leave a stub in place for each document following disposal
            leaveStubMessage: "",
            isDeleteRelatedRecordOption: false,                        //Include related records checked
            isShowDeleteRelatedRecordOption: true,                     //是否显示Include related records
            isShowDeclareOption: LicenseHelper.Is21VEnv() || !LicenseHelper.EnableRecordsArchiver(),                                 // 是否显示Declare each document/item as a SharePoint record
            isBackupOption: true,
            MoveUrl: "",
            MoveUser: "",
            MovePassWord: "",
            isMoveDeclare: false,
            isKeepClassificationSPO: true,
            isKeepFolderStructure: true,
            isMoveVersions: false,
            isSpecifyLocation: false,                                  //Enter a destination checked
            locationPath: "",                                          //Enter a destination input的value
            noLocation: false,                                         //Please enter a destination. msg 显示隐藏
            noSelectNode: false,                                       //Please select a destination from the tree. 显示隐藏
            fileInherit: 0,
            currentTimeZone: RM.TimeUtil.getGlobalTimezoneInfo(),
            retentionActionChecked: false,
            exoRetentionActionChecked: true,
            fsRetentionActionChecked: true,
            retentionAction: '',
            exoRetentionAction: '',
            fsRetentionAction: '',
            destinationTreeData: [],
            fileNameConflictOptionSkip: 1,
            fileNameConflictOptionOverwrite: 2,
            fileNameConflictOptionRename: 3,
            currentConflictOptionValue: 1,
            exo_currentConflictOptionValue: 1,
            phy_currentConflictOptionValue: 1,
            moveHoldConflictOptionCurrent: 1,
            moveHoldConflictOptionCompare: 2,
            currentMoveHoldConflictOptionValue: 1,
            yesOrNo: Constants.yesOrNo,
            TrueOrFaseOptions: this.TrueOrFaseOptions,
            rules: [],
            conditionType: null,
            ruleCriteriaTabsIndex: 0, //TAB index
            DefinesharePointCriteriaTabs: [{ title: RMResx.RM_JS_RDM_CreateRule_FilterLevel_EXOMessage }],
            DefineruleCriteriaTabsIndex: 0,                             //EMAIL
            isShowDeclaredFileOption: true,                             //是否显示Include declared records display
            isLocationVlidat: false,                                    // Enter a destination 验证
            LocationVlidat: "",                                         // Enter a destination 验证内容
            moveDestTreeData: [],
            termDto: {},                                                //判断term是否使用
            isShowTerm: true,
            tagType: RM.deepcopy(Constants.tagType),
            isLeaveStubOptionOfFs: false,
            isDeleteRelatedRecordOptionOfFs: false,
            isPhyMoveShow: true,
            isPhyRemove: true,
            isPhyMove: false,
            phyTreeData: [],
            noSelectPhyNode: false,                                       //Please select a destination from the tree. 显示隐藏
            smallNodeType: NodeType.PhyBox,
            isDeleteRelatedRecordOptionOfPhy: false,                        //Include related records checked for phy
            isShowDeleteRelatedRecordOptionOfPhy: true,                     //是否显示Include related records for phy
            isDestoryEmptyBoxOnFolderRuleOptionOfPhy: false,                           //Remove the box if all folder are removed
            isShowDestoryEmptyBoxOnFolderRuleOptionOfPhy: true,                        //是否显示Remove the box if all folder are removed
            isCSDTenant: false,
            isDeclareLinkFile: false,
            retentionRecordsLabelOptions: [],
            retentionRecordsLabelSelected: Constants.RetentionLabelOptions.Default,
            showCalculateDisposalDateOptionForPhy: false,
            isCalculationDisposalDate: false,

            isKeepShowForLocal: true,
            isMoveShowForLocal: false,
            isExportOnlyForLocal: false,                                       //Export without archiving
            isRemoveForLocal: true,
            isKeepForLocal: false,                                             //Record Declaration and Tagging  radio 显示隐藏
            isMoveForLocal: false,                                             //Move documents to a new destination library  显示隐藏
            isDeclareForLocal: false,                                          //Remove content from SharePoint and destroy
            isUndeclareForLocal: false,                                        //Undeclare in-place record
            iskeepTagForLocal: false,                                          //Tag each document/item with: check
            isTagYesForLocal: false,                                           //Record Declaration and Tagging Tag each document/item with: Archived (Yes/No column) checked
            isTagByForLocal: false,                                            //Archived By checked
            isTagTimeForLocal: false,                                          //Archived Time checked
            metadataNameForLocal: "",                                          //Custom Metadata input name value
            metadataValueForLocal: "",                                         //Custom Metadata input Value value
            tagMetadataCheckedForLocal: false,                                 //Custom Metadata checked
            isTagTextForLocal: true,                                           //Custom Metadata: text
            tagTypeValueForLocal: 0,
            isDeclaredFileForLocal: false,                                     //Include related records
            slectTagTypeForLocal: null,                                        // Custom Metadata 第一个选项框选中的值
            selectTagBooleanForLocal: this.TrueOrFaseOptions[0],                //yes no 默认值
            timezonesForLocal: RM.TimeSettingModel.TimeZoneInfo,               //时区
            currentDateForLocal: null,                                           //选中的日期
            isTagDateForLocal: false,                                          // 控制日期控制隐藏
            isTagBooleanForLocal: false,                                       //Custom Metadata: display
            noMetadateValueForLocal: false,                                    //Please select a column type, and enter the column name and value.
            noDateValueForLocal: false,                                        //Please specify a time range.
            noNumberValueForLocal: false,                                      //The entered value is invalid. Please check and enter again.
            noTagsForLocal: false,                                             //Please select at least one tagging option.
            noSelectForLocal: false,
            noRetentionActionValueForLocal: false,
            isLeaveStubOptionForLocal: false,                                  //Leave a stub in place for each document following disposal checked
            isShowLeaveStubOptionForLocal: true,                               //是否显示Leave a stub in place for each document following disposal
            isDeleteRelatedRecordOptionForLocal: false,                        //Include related records checked
            isShowDeleteRelatedRecordOptionForLocal: true,                     //是否显示Include related records
            isShowDeclareOptionForLocal: true,                                 // 是否显示Declare each document/item as a SharePoint record
            isBackupOptionForLocal: false,
            MoveUrlForLocal: "",
            MoveUserForLocal: "",
            MovePassWordForLocal: "",
            isMoveDeclareForLocal: false,                                      //Declare each document as a SharePoint record when it is moved checked
            isSpecifyLocationForLocal: false,                                  //Enter a destination checked
            locationPathForLocal: "",                                          //Enter a destination input的value
            noLocationForLocal: false,                                         //Please enter a destination. msg 显示隐藏
            noSelectNodeForLocal: false,                                       //Please select a destination from the tree. 显示隐藏
            fileInheriForLocalt: 0,
            currentTimeZoneForLocal: RM.TimeUtil.getGlobalTimezoneInfo(),
            retentionActionCheckedForLocal: false,
            destinationTreeDataForLocal: [],
            fileNameConflictOptionSkipForLocal: 1,
            fileNameConflictOptionOverwriteForLocal: 2,
            fileNameConflictOptionRenameForLocal: 3,
            currentConflictOptionValueForLocal: 1,
            yesOrNoForLocal: Constants.yesOrNo,
            TrueOrFaseOptionsForLocal: this.TrueOrFaseOptions,
            isShowDeclaredFileOptionForLocal: true,                             //是否显示Include declared records display
            isLocationVlidatForLocal: false,                                    // Enter a destination 验证
            LocationVlidatForLocal: "",      
            tagTypeForLocal: RM.deepcopy(Constants.tagType),
            isDeclareLinkFileForLocal: false,

            isKeepShowForOneDrive: true,
            isMoveShowForOneDrive: true,
            isShowExportOnlyForOneDrive: true,
            isShowExportCheckboxForOneDrive: true,
            isExportOnlyForOneDrive: false,                                       //Export without archiving
            isRemoveForOneDrive: true,
            isKeepForOneDrive: false,                                             //Record Declaration and Tagging  radio 显示隐藏
            isMoveForOneDrive: false,                                             //Move documents to a new destination library  显示隐藏
            isDeclareForOneDrive: false,                                          //Remove content from SharePoint and destroy
            isUndeclareForOneDrive: false,                                        //Undeclare in-place record                                     
            iskeepTagForOneDrive: false,                                          //Tag each document/item with: check
            isTagYesForOneDrive: false,                                           //Record Declaration and Tagging Tag each document/item with: Archived (Yes/No column) checked
            isTagByForOneDrive: false,                                            //Archived By checked
            isTagTimeForOneDrive: false,                                          //Archived Time checked
            isArchiveWithoutDestroyForOneDrive: false,                            //Archive without destroy content option (control by flag in BE: update later)
            metadataNameForOneDrive: "",                                          //Custom Metadata input name value
            metadataValueForOneDrive: "",                                         //Custom Metadata input Value value
            tagMetadataCheckedForOneDrive: false,                                 //Custom Metadata checked
            isTagTextForOneDrive: true,                                           //Custom Metadata: text
            tagTypeValueForOneDrive: 0,
            isDeclaredFileForOneDrive: false,
            isDeleteToRecycleBinForOneDrive: false,
            isIncludeLockedFileForOneDrive: false,                                 //Include related records
            isLockRecordForOneDrive: true,
            isRetentionLabelForOneDrive: false,                                    // Include retention label
            slectTagTypeForOneDrive: null,                                        // Custom Metadata 第一个选项框选中的值
            selectTagBooleanForOneDrive: this.TrueOrFaseOptions[0],                //yes no 默认值
            timezonesForOneDrive: RM.TimeSettingModel.TimeZoneInfo,               //时区
            currentDateForOneDrive: null,                                           //选中的日期
            isTagDateForOneDrive: false,                                          // 控制日期控制隐藏
            isTagBooleanForOneDrive: false,                                       //Custom Metadata: display
            noMetadateValueForOneDrive: false,                                    //Please select a column type, and enter the column name and value.
            noDateValueForOneDrive: false,                                        //Please specify a time range.
            noNumberValueForOneDrive: false,                                      //The entered value is invalid. Please check and enter again.
            noTagsForOneDrive: false,                                             //Please select at least one tagging option.
            noSelectForOneDrive: false,
            noRetentionActionValueForOneDrive: false,
            isLeaveStubOptionForOneDrive: false,                                  //Leave a stub in place for each document following disposal checked
            isArchivingRecordOptionForOneDrive: false,
            isShowLeaveStubOptionForOneDrive: true,                               //是否显示Leave a stub in place for each document following disposal
            leaveStubMessageForOneDrive: "",
            isDeleteRelatedRecordOptionForOneDrive: false,                        //Include related records checked
            isShowDeleteRelatedRecordOptionForOneDrive: false,                     //是否显示Include related records
            isShowDeclareOptionForOneDrive: LicenseHelper.Is21VEnv() || !LicenseHelper.EnableRecordsArchiver(),                                 // 是否显示Declare each document/item as a SharePoint record
            isBackupOptionForOneDrive: true,
            MoveUrlForOneDrive: "",
            MoveUserForOneDrive: "",
            MovePassWordForOneDrive: "",
            isMoveDeclareForOneDrive: false, 
            isKeepClassificationForOneDrive: true,                                     //Declare each document as a SharePoint record when it is moved checked
            isKeepFolderStructureForOneDrive: true,
            isMoveVersionsForOneDrive: false,
            isSpecifyLocationForOneDrive: false,                                  //Enter a destination checked
            locationPathForOneDrive: "",                                          //Enter a destination input的value
            noLocationForOneDrive: false,                                         //Please enter a destination. msg 显示隐藏
            noSelectNodeForOneDrive: false,                                       //Please select a destination from the tree. 显示隐藏
            noSelectNodeForTeams: false,                                       //Please select a destination from the tree. 显示隐藏
            fileInheriForOneDrivet: 0,
            currentTimeZoneForOneDrive: RM.TimeUtil.getGlobalTimezoneInfo(),
            retentionActionCheckedForOneDrive: false,
            destinationTreeDataForOneDrive: [],
            fileNameConflictOptionSkipForOneDrive: 1,
            fileNameConflictOptionOverwriteForOneDrive: 2,
            fileNameConflictOptionRenameForOneDrive: 3,
            currentConflictOptionValueForOneDrive: 1,
            yesOrNoForOneDrive: Constants.yesOrNo,
            TrueOrFaseOptionsForOneDrive: this.TrueOrFaseOptions,
            isShowDeclaredFileOptionForOneDrive: true,                             //是否显示Include declared records display
            isLocationVlidatForOneDrive: false,                                    // Enter a destination 验证
            LocationVlidatForOneDrive: "",      
            tagTypeForOneDrive: RM.deepcopy(Constants.tagType),
            isRestoreLinkForOneDrive: false,
            isDeclareLinkFileForOneDrive: false,
            retentionRecordsLabelOptionsForOneDrive: [],
            retentionRecordsLabelSelectedForOneDrive: Constants.RetentionLabelOptions.Default,

            // Teams
            isRemoveForTeams: true,
            isLeaveStubOptionForTeams: false,
            isShowLeaveStubOptionForTeams: true,
            isKeepShowForTeams: true,
            selectedLevelStubSettingForTeams: {},
            isBackupOptionForTeams: false,
            isDeleteRelatedRecordOptionForTeams: false,
            isShowDeleteRelatedRecordOptionForTeams: true,
            isDeclaredFileForTeams: false,
            isShowDeclareOptionForTeams: true,
            isShowDeclaredFileOptionForTeams: true,
            isIncludeLockedFileForTeams: false,
            isLockRecordBeforeDestroyForTeams: true,
            isRetentionLabelForTeams: false,
            isArchiveToAzureBlobStorageForTeams: false,
            isKeepForTeams: false,
            isDeclareForTeams: false,
            isUndeclareForTeams: false,
            iskeepTagForTeams: false,
            isTagYesForTeams: false,
            isTagByForTeams: false,
            isTagTimeForTeams: false,
            tagMetadataCheckedForTeams: false,
            tagTypeForTeams: RM.deepcopy(Constants.tagType),
            tagTypeValueForTeams: 0,
            metadataNameForTeams: "",
            selectTagBooleanForTeams: this.TrueOrFaseOptions[0],
            metadataValueForTeams: "",
            currentDateForTeams: null,
            currentTimeZoneForTeams: RM.TimeUtil.getGlobalTimezoneInfo(),
            retentionActionCheckedForTeams: false,
            retentionActionForTeams: "",
            isSORemoveForTeams: false,
            isKeepVersionOptionForTeams: true,
            keepVersionValueForTeams: 0,
            isArchivingRecordOptionForTeams: false,
            isBackupAndRemoveForTeams: true,
            isArchiveVersionOptionForTeams: false,
            archiveVersionValueForTeams: "0",
            isKeepVersionAndArchiveForTeams: false,
            keepVersionAndArchiveValueForTeams: "0",
            isMoveForTeams: false,
            isSpecifyLocationForTeams: false,
            locationPathForTeams: "",
            isMoveDeclareForTeams: false,
            isKeepClassificationForTeams: true,
            isMoveVersionsForTeams: false,
            fileNameConflictOptionSkipForTeams: 1,
            fileNameConflictOptionOverwriteForTeams: 2,
            fileNameConflictOptionRenameForTeams: 3,
            currentConflictOptionValueForTeams: 1,
            fileInheritForTeams: 0,
            isExportOnlyForTeams: false,
            isRestoreLinkForTeams: false,
            isDeclareLinkFileForTeams: false,
            levelStubSettingListForTeams:  RM.deepcopy(this.props.levelStubSettingList),
            noLeaveStubValueForTeams: false,
            noSelectForTeams: false,
            selectTagTypeForTeams: null,
            noDateValueForTeams: false,
            noMetadateValueForTeams: false,
            noNumberValueForTeams: false,
            noTagsForTeams: false,
            isTagDateForTeams: false,
            isTagTextForTeams: true,
            isTagBooleanForTeams: false,
            noKeepVersionValueForTeams: false,
            keepVersionValueInvalidForTeams: false,
            noKeepVersionAndArchiveValueForTeams: false,
            keepVersionAndArchiveValueInvalidForTeams: false,
            noArchiveVersionValueForTeams: false,
            archiveVersionValueInvalidForTeams: false,
            noRetentionActionValueForTeams: false,
            noLocationForTeams: false,
            isShowExportOnlyForTeams: true,
            isMoveShowForTeams: true,
            isLocationValidateForTeams: false,
            locationValidateMsgForTeams: "",
            destinationTreeDataForTeams: [],
            destinationTreeDataForTeamsOD: [],
            destinationTreeDataForTeamsEXO: [],
            yesOrNoForTeams: Constants.yesOrNo,
            MoveUrlForTeams: "",
            MoveUserForTeams: "",
            MovePassWordForTeams: "",
            // Teams: end

            isSeparateArchive: false,
            isArchiveToAzureBlobStorage: false,  
            isArchiveToAzureBlobStorageForOneDrive: false, 
            isArchiveToAzureBlobStorageForFS: false,

            //Azure File
            isAzureFileRemove: true,
            isLeaveStubOptionForAzureFile: false,

            //Box
            isBoxRemove: true,

            //Google
            isGoogleDriveRemove: true,
            isGoogleExportOnly: false,     // without archiving
            isGoogleMove: false,
            destinationTreeDataForGoogle: [],
            noSelectNodeForGoogle: false,
            isArchiveToStorageForGoogle: false,
            isGControlManualApproval: false,
            
            //Connector
            isConnectorRemove: true,

            //archive
            levelStubSettingListForSPO: RM.deepcopy(this.props.levelStubSettingList),
            levelStubSettingListForOneDrive:  RM.deepcopy(this.props.levelStubSettingList),
            selectedLevelStubSettingForSPO: {},
            selectedLevelStubSettingForOneDrive: {},
            noLeaveStubValueForSPO: false,
            noLeaveStubValueForOneDrive: false,

            isSORemoveForSPO: false,
            isSORemoveForOD: false,
            isKeepVersionOption: true,
            isSODeleteToRecycleBinForSPO: false,
            isKeepVersionOptionForOD: true,
            isSODeleteToRecycleBinForOD: false,
            keepVersionValue: 0,
            keepVersionValueForOD: 0,
            noKeepVersionValueForSPO: false,
            noKeepVersionValueForOD: false,
            keepVersionValueInvalidForSPO: false,
            keepVersionValueInvalidForOD: false,
            
            isBackupAndRemoveForSPO: true,
            isBackupAndRemoveForOD: true,
            isArchiveVersionOption: false,
            isArchiveVersionOptionForOD: false,
            archiveVersionValue: "0",
            archiveVersionValueForOD: "0",
            noArchiveVersionValueForSPO: false,
            noArchiveVersionValueForOD: false,
            archiveVersionValueInvalidForSPO: false,
            archiveVersionValueInvalidForOD: false,

            // SO rule keep version and archive file
            isKeepVersionAndArchiveForSPO: false,
            keepVersionAndArchiveValueForSPO: "0",
            noKeepVersionAndArchiveValueForSPO: false,
            keepVersionAndArchiveValueInvalidForSPO: false,
            isKeepVersionAndArchiveForOD: false,
            keepVersionAndArchiveValueForOD: "0",
            noKeepVersionAndArchiveValueForOD: false,
            keepVersionAndArchiveValueInvalidForOD: false,
            selectedSourcesIndexs:[],

            destinationActiveTab: 0,
            destinationActiveTabForOD: 0,
            destinationActiveTabForEXO: 0,

            // Stub setting panel
            showStubSettingsPanel: false,
            selectedRuleSourceTabIndex: 0,

            // Records label
            recordsLabelValue: RMResx.RM_JS_SP_MigrateDeclaredRecords_NoneRecordsLabel,
        };
        this.bind([
            "closeMessageBox",
            "sharePointCriteriaTabClick", "getSpCriteriaData",
            "getSpIsVerificationPassed", "getExoCriteriaData", "getPhyCriteriaData", "getExoIsVerificationPassed", "removeCheckedChange", "keepCheckedChange", "declareChecked", "spExportOnlyCheckedChange","exoExportOnlyCheckedChange",
            "undeclareChecked", "keepTagChecked", "tagTypeSelectChanged", "metadataNameChange", "metadataValueChange", "onCurrentStoragePolicyChange",
            "archiveActionCustomValidate", "metadataDateSelecteChange", "retentionActionChange", "moveCheckedChange", "locationPathChange", "checkLocation", "cancelLocationVlidat",
            "onDestTreeSelectedChanged", "fileConflictOptionChange", "phyFileConflictOptionChange", "getSpApprovalIsPassed", "getSpApprovalData", "getExoApprovalIsPassed",
            "getExoApprovalData", "saveClick", "cancelClick", "onCancleOK", "onCancleNo", "termSwitch",
            "onSure", "jumpExportSettings", "getSpExportIsPassed", "getSpExportLocationIsPassed", "getOneDriveExportLocationIsPassed", "getExoExportLocationIsPassed", "getExoExportIsPassed", "getExoExportDate", "getSpExportDate",
            'fsRemoveCheckedChange', 'fsMoveCheckedChange', 'exoRemoveCheckedChange', 'exoKeepCheckedChange', 'exoMoveCheckedChange',
            "exoLocationPathChange", "checkExoLocation", "cancelExoLocationValidate", "exoFileConflictOptionChange", "onDestExoTreeSelectedChanged",
            'onRetentionActionCheckChange', 'exoRetentionActionChange', 'phyRemoveCheckedChange', 'phyMoveCheckedChange', "exoArchiveActionCustomValidate", "moveHoldConflictOptionChange", 
            "getFsApprovalIsPassed", "getFsApprovalData", "ruleMoveOfFsCallback", "getFsExportIsPassed", "getFsExportDate", "getFsCriteriaData",

            "getSpLocalIsVerificationPassed", "getSpLocalCriteriaData", "getSpLocalExportDate", "onRetentionActionCheckChangeForLocal", "onCurrentStoragePolicyChangeForLocal",
            "removeCheckedChangeForLocal", "keepCheckedChangeForLocal", "declareCheckedForLocal", "spExportOnlyCheckedChangeForLocal", "checkLocationForLocal",
            "undeclareCheckedForLocal", "keepTagCheckedForLocal", "tagTypeSelectChangedForLocal", "metadataNameChangeForLocal", "metadataValueChangeForLocal",
            "spLocalArchiveActionCustomValidate", "metadataDateSelecteChangeForLocal", "moveCheckedChangeForLocal", "locationPathChangeForLocal", 
            "onDestTreeSelectedChangedForLocal", "fileConflictOptionChangeForLocal", "getSpLocalApprovalIsPassed", "getSpLocalApprovalData","getSpLocalExportIsPassed",
            "cancelLocationVlidatForLocal",

            "getOneDriveIsVerificationPassed", "getOneDriveCriteriaData", "getOneDriveExportDate", "onRetentionActionCheckChangeForOneDrive", "onCurrentStoragePolicyChangeForOneDrive",
            "removeCheckedChangeForOneDrive", "keepCheckedChangeForOneDrive", "declareCheckedForOneDrive", "oneDriveExportOnlyCheckedChange", "checkLocationForOneDrive",
            "undeclareCheckedForOneDrive", "tagTypeSelectChangedForOneDrive", "metadataNameChangeForOneDrive", "metadataValueChangeForOneDrive", "keepTagCheckedForOneDrive",
            "oneDriveArchiveActionCustomValidate", "metadataDateSelecteChangeForOneDrive", "retentionActionChangeForOneDrive", "moveCheckedChangeForOneDrive", "locationPathChangeForOneDrive", 
            "onDestTreeSelectedChangedForOneDrive", "fileConflictOptionChangeForOneDrive", "getOneDriveApprovalIsPassed", "getOneDriveApprovalData","getOneDriveExportIsPassed",
            "cancelLocationVlidatForOneDrive"
        ]);
    }

    componentReceive(action, data) {
        if(action == "InitRuleSettingSources"){
            this.setSelectedSource(data);
        }
        if(action == "InitRuleSettingByLevel"){
            let isSOArchiverModuleType = data.moduleType === Constants.RuleModuleTypes.SOArchiver;
            this.setState({
                selectedRuleModuleType: data.moduleType || RuleModuleTypes.Records,
                isBackupAndRemoveForSPO: isSOArchiverModuleType,
                isBackupAndRemoveForOD: isSOArchiverModuleType,
                isBackupAndRemoveForTeams: isSOArchiverModuleType,
            }, () => {
                this.setElementStatusBylevelId(data.levelId, this.state.selectedRuleModuleType);
                this.dispatch("spCriteria", "selectedModuleType", this.state.selectedRuleModuleType);
                this.dispatch("oneDriveCriteria", "selectedModuleType", this.state.selectedRuleModuleType);
                this.dispatch("spCriteria", Constants.dispatchAction.clearData, this.levelId, this.state.selectedRuleModuleType);
                this.dispatch("oneDriveCriteria", Constants.dispatchAction.clearData, this.levelId, this.state.selectedRuleModuleType);
                this.dispatch("teamsCriteria", "selectedModuleType", this.state.selectedRuleModuleType);
                this.dispatch("teamsCriteria", Constants.dispatchAction.clearData, this.levelId, this.state.selectedRuleModuleType);
            });
        }
        if(action == "EchoRuleSettingData"){
            this.setData(data);
        }
        if(action == "CreateRule"){
            this.saveClick(data);
        }
    }

    componentInit () {
        this.CheckIsCSDTenant();
        this.loadDeclaredRecords();
    }

    CheckIsCSDTenant () {
        $$.loading(true);
        let urlData = "/api/RuleApi/CheckIsCSDTenant";
        let option = {
            url: urlData,
            method: "POST"
        };
        fetchUtility(option).then((res) => {
            this.setState({ isCSDTenant: res });
            $$.loading(false);
        }).catch((e) => {
            $$.loading(false);
        });
    }

    setRetentionRecordsLabelOptions() {
        this.setState({
            retentionRecordsLabelOptions: this.getRetentionRecordsLabelOptions(Constants.RetentionLabelOptions.GetFromGeneralSetting),
            retentionRecordsLabelOptionsForOneDrive: this.getRetentionRecordsLabelOptions(Constants.RetentionLabelOptions.GetFromGeneralSetting),
        });
    }

    loadDeclaredRecords() {
        $$.loading(true);
        const options = {
            url: "/api/RuleApi/GetRecordLabel",
            method: "GET",
        };
        fetchUtility(options)
            .then((res) => {
                this.setState({
                    recordsLabelValue: res ?? RMResx.RM_JS_SP_MigrateDeclaredRecords_NoneRecordsLabel,
                }, () => {
                    this.setRetentionRecordsLabelOptions();
                });
            })
            .finally(() => $$.loading(false));
    }

    //获取term的level
    // getTmLevel () {
    //     debugger;
    //     for (let item of this.levels) {
    //         if (item.id == this.props.currentRowRuleLevelId) {
    //             this.levels = this.levels.slice();
    //             setCheckedStatus("id", "Checked", this.levels, item);
    //             this.setState({
    //                 selectLevel: item,
    //                 levels: this.levels,
    //             });
    //             this.levelId = item.id;
    //             this.isRemove = true;
    //             this.setElementStatusBylevelId(item.id);
    //         }
    //     }
    // }

    //跳转到ExportSettings页面
    jumpExportSettings () {
        this.props.history.push({
            pathname: RouterUrls.CP_ExportSettings
        });
    }

    //change 的公共方法
    onCheckChange (attribute) {
        this.setState({
            [attribute]: !this.state[attribute]
        }, () => {
            if (!this.state.isLeaveStubOption && this.state.selectedLevelStubSettingForSPO && Object.keys(this.state.selectedLevelStubSettingForSPO).length > 0) {
                this.setState({
                    selectedLevelStubSettingForSPO: {},
                    levelStubSettingListForSPO: this.resetLeaveStubState(),
                });
            }
            if (!this.state.isLeaveStubOptionForOneDrive && this.state.selectedLevelStubSettingForOneDrive && Object.keys(this.state.selectedLevelStubSettingForOneDrive).length > 0) {
                this.setState({
                    selectedLevelStubSettingForOneDrive: {},
                    levelStubSettingListForOneDrive: this.resetLeaveStubState(),
                });
            }
            if (this.state.isIncludeDeclaredDisable && !this.state.isBackupOption) {
                this.setState({
                    isDeclaredFile: false,
                });
            }
        });
    }

    resetLeaveStubState() {
        let resetState = RM.deepcopy(this.props.levelStubSettingList);
        resetState.forEach(item => item.Checked = false);
        return resetState;
    }

    //获取id
    // getLevelIndexById (id) {
    //     let index;
    //     this.state.levels.forEach(function (level, k) {
    //         if (level.id == id) {
    //             index = k;
    //             return;
    //         }
    //     });
    //     return index;
    // }

    // //阻止冒泡
    // stopPropagation (e) {
    //     e.nativeEvent.stopImmediatePropagation();
    // }

    routerTo (routerUrl, param) {
        this.props.history.push({
            pathname: routerUrl,
            state: param
        });
    }

    stringFormat () {
        if (arguments.length == 0)
            return null;
        let str = arguments[0];
        for (let i = 1; i < arguments.length; i++) {
            let re = new RegExp("\\{" + (i - 1) + "\\}", "gm");
            str = str.replace(re, arguments[i]);
        }
        return str;
    }

    criteriaDispatch(action, data, ruleModuleType) {
        this.dispatch("spCriteria", action, data, ruleModuleType);
        this.dispatch("spLocalCriteria", action, data);
        this.dispatch("oneDriveCriteria", action, data, ruleModuleType);
        this.dispatch("teamsCriteria", action, data, ruleModuleType);
        this.dispatch("exoCriteria", action, data);
        this.dispatch("phyCriteria", action, data);
        this.dispatch("fsCriteria", action, data, ruleModuleType);
        this.dispatch("azureFileCriteria", action, data);
        this.dispatch("boxCriteria", action, data);
        this.dispatch("googleDriveCriteria", action, data);
        this.dispatch("connectorCriteria", action, data);
    }

    approvalDispatch (action, data) {
        this.dispatch("spApproval", action, data);
        this.dispatch("spLocalApproval", action, data);
        this.dispatch("oneDriveApproval", action, data);
        this.dispatch("teamsApproval", action, data);
        this.dispatch("exoApproval", action, data);
        this.dispatch("phyApproval", action, data);
        this.dispatch("fsApproval", action, data);
    }

    exportDispatch (action, data) {
        this.dispatch("spExport", action, data);
        this.dispatch("spLocalExport", action, data);
        this.dispatch("oneDriveExport", action, data);
        this.dispatch("teamsExport", action, data);
        this.dispatch("exoExport", action, data);
        this.dispatch("googleExport", action, data);
        // this.dispatch("phyExport", action, data);
        // this.dispatch("fsExport", action, data);
        if(this.state.isExportOnly){
            this.dispatch("spExportOnly", action, data);
        }
        // this.dispatch("spLocalExportOnly", action, data);
        if(this.state.isExportOnlyForOneDrive){
            this.dispatch("oneDriveExportOnly", action, data);
        }
        if(this.state.isExoExportOnly){
            this.dispatch("exoExportOnly", action, data);
        }
        if(this.state.isGoogleExportOnly) {
            this.dispatch("googleExportOnly", action, data);
        }
    }

    // copyRuleSelect () {
    //     this.setState({
    //         elementsEnable: true
    //     });
    //     this.criteriaDispatch(Constants.dispatchAction.elementDisabled, true);
    //     this.approvalDispatch(Constants.dispatchAction.elementDisabled, true);
    //     this.exportDispatch(Constants.dispatchAction.elementDisabled, true);
    // }

    //点击creat new rule radio
    // newRuleSelect () {
    //     this.setState({
    //         elementsEnable: false
    //     });
    //     this.ArchiveActionClearData();
        
    //     this.criteriaDispatch(Constants.dispatchAction.elementDisabled, false);
    //     this.approvalDispatch(Constants.dispatchAction.elementDisabled, false);
    //     this.exportDispatch(Constants.dispatchAction.elementDisabled, false);
    //     this.criteriaDispatch(Constants.dispatchAction.clearData, this.levelId);
    //     this.approvalDispatch(Constants.dispatchAction.clearData);
    //     this.exportDispatch(Constants.dispatchAction.clearData, this.levelId);
    // }

    //根据id获取item
    // echoData (ruleId) {
    //     $$.loading(true);
    //     //console.log("get rule: " + new Date());
    //     this.setState({ phyTreeData: [] });
    //     let urlData = "/api/RuleApi/GetRuleByID";
    //     let option = {
    //         url: urlData,
    //         method: "POST",
    //         data: ruleId
    //     };
    //     fetchUtility(option).then((res) => {
    //         //console.log("get rule back: " + new Date());
    //         let data = JSON.parse(res.Extension);
    //         this.setData(data);
    //         //console.log("set rule: " + new Date());
    //         $$.loading(false);
    //     });
    // }

    //选择copy rule 的 combox
    // ruleSelectChange (args) {
    //     // let item = args.newValue;
    //     this.setState({
    //         elementsEnable: false
    //     });
    //     $("#rm_createRule_ruleSelector").focus();
    //     this.criteriaDispatch(Constants.dispatchAction.elementDisabled, false);
    //     this.approvalDispatch(Constants.dispatchAction.elementDisabled, false);
    //     this.exportDispatch(Constants.dispatchAction.elementDisabled, false);
    //     this.criteriaDispatch(Constants.dispatchAction.clearData, this.levelId);
    //     this.approvalDispatch(Constants.dispatchAction.clearData);
    //     this.exportDispatch(Constants.dispatchAction.clearData, this.levelId);
    //     this.ArchiveActionClearData();
    //     //this.echoData(item.RuleId);
    // }

    //leave stub message

    //Disposal Class的验证
    // DisposalValidateInput (isDes) {
    //     let isValid = true;
    //     this.setState({
    //         desIsToLong: false,
    //         isTooLong: false
    //     });
    //     if (isDes) {
    //         if ($.trim(this.state.Description).length >= 255) {
    //             this.setState({ desIsToLong: true });
    //             isValid = false;
    //         }
    //     } else {
    //         if ($.trim(this.state.DisposalClass).length >= 255) {
    //             this.setState({ isTooLong: true });
    //             isValid = false;
    //         }
    //     }
    //     return isValid;
    // }

    // disposalClassChange (e) {
    //     this.setState({ DisposalClass: e.target.value });
    // }

    // levelSelectedChange (args) {
    //     this.levelId = args.newValue.id;
    //     //console.log('change level:' + this.levelId);
    //     this.setElementStatusBylevelId(this.levelId);
    // }

    setElementStatusBylevelId(levelId, ruleModuleType) {
        this.levelId = levelId;
        if (levelId == 64) {
            this.spNodeItem = null;
            this.spLocalNodeItem = null;
            this.exoNodeItem = null;
            this.teamsNodeItem = null;
            this.setState({
                destinationTreeData: [],
                exoDestinationTreeData: [],
                destinationTreeDataForLocal: [],
                destinationTreeDataForOneDrive: [],
                destinationTreeDataForGoogle: [],
                destinationTreeDataForTeams: [],
                destinationTreeDataForTeamsOD: [],
                destinationTreeDataForTeamsEXO: [],
            });
        } else if (levelId == 8 || levelId == 16) {
            let bottomLocation = levelId == 8 ? NodeType.PhysicalBottomLocation : NodeType.PhyBox;
            this.selectedPhyTreeItem = null;
            this.setState({
                smallNodeType: bottomLocation,
                phyTreeData: null,
            });
        }
        this.setArchiveActionDisplayByRuleLevel(levelId, ruleModuleType);
        //选中Document 并且 Remove content from SharePoint and destroy被选中
        if (this.isRemove && levelId == 64) {
            this.setState({
                isShowLeaveStubOption: true,
                isShowLeaveStubOptionForLocal: true,
                isShowLeaveStubOptionForOneDrive: true,
                isShowLeaveStubOptionForTeams: true
            });
        } else {
            this.setState({
                isShowLeaveStubOption: false,
                isShowLeaveStubOptionForLocal: false,
                isShowLeaveStubOptionForOneDrive: false,
                isShowLeaveStubOptionForTeams: false
            });
        }
        //选中Document 并且 Remove content from SharePoint and destroy被选中
        if (this.isKeep && levelId != 65536) {
            this.setState({
                isShowDeclaredFileOption: true
            });
        } else {
            this.setState({
                isShowDeclaredFileOption: false
            });
        }

        // Teams
        if (this.isKeepForTeams && levelId != 65536) {
            this.setState({
                isShowDeclaredFileOptionForTeams: true,
            });
        } else {
            this.setState({
                isShowDeclaredFileOptionForTeams: false,
            });
        }
        //document and item level
        if (this.isRemove && (levelId == 64 || levelId == 32)) {
            this.setState({
                isShowDeleteRelatedRecordOption: true
            });
        } else {
            this.setState({
                isShowDeleteRelatedRecordOption: false
            });
        }

        // Teams
        if (this.isRemoveForTeams && (levelId == 64 || levelId == 32)) {
            this.setState({
                isShowDeleteRelatedRecordOptionForTeams: true
            });
        } else {
            this.setState({
                isShowDeleteRelatedRecordOptionForTeams: false,
            });
        }

        //folder level
        if (this.state.isPhyRemove && (levelId == 16)) {
            this.setState({
                isShowDeleteRelatedRecordOptionOfPhy: true
            });
        } else {
            this.setState({
                isShowDeleteRelatedRecordOptionOfPhy: false
            });
        }
        //folder level disposal box
        if (this.state.isPhyRemove && (levelId == 16)) {
            this.setState({
                isShowDestoryEmptyBoxOnFolderRuleOptionOfPhy: true
            });
        } else {
            this.setState({
                isShowDestoryEmptyBoxOnFolderRuleOptionOfPhy: false
            });
        }
        //Declare each document/item as a SharePoint record
        if ((this.is21VEnv || !this.enableRecordsArchiver) && ((levelId == 64 && this.isKeep) || (levelId == 32 && this.isKeep))) {
            this.setState({
                isShowDeclareOption: true,
                isShowDeclareOptionForTeams: true,
            });
        } else {
            this.setState({
                isShowDeclareOption: false,
                isShowDeclareOptionForTeams: false,
            });
        }
        //Remove content from SharePoint and destroy被选中
        if (this.isRemove) {
            this.setState({
                isShowDeclaredFileOption: true
            });
        } else {
            this.setState({
                isShowDeclaredFileOption: false
            });
        }

        // Teams
        if (this.isRemoveForTeams) {
            this.setState({
                isShowDeclaredFileOptionForTeams: true
            });
        } else {
            this.setState({
                isShowDeclaredFileOptionForTeams: false
            });
        }
        this.criteriaDispatch(Constants.dispatchAction.clearData, levelId, ruleModuleType);
        this.approvalDispatch(Constants.dispatchAction.clearData);
        this.exportDispatch(Constants.dispatchAction.clearData, levelId);
    }

    //点击tab
    sharePointCriteriaTabClick (index) {
        if (this.state.elementsEnable) {
            return;
        }
        this.setState({
            ruleCriteriaTabsIndex: this.state.selectedSourcesIndexs[index] 
        });
    }

    //tab checkbox click
    setSelectedSource (selectedSourcesIndexs) {
        this.isSpSourceChecked = selectedSourcesIndexs.includes(Constants.RuleSourceTabIndex.SP);
        this.isExoSourceChecked = selectedSourcesIndexs.includes(Constants.RuleSourceTabIndex.Exchange);
        this.isPhySourceChecked = selectedSourcesIndexs.includes(Constants.RuleSourceTabIndex.Physical);
        this.isFsSourceChecked = selectedSourcesIndexs.includes(Constants.RuleSourceTabIndex.FS);
        this.isSpLocalSourceChecked = selectedSourcesIndexs.includes(Constants.RuleSourceTabIndex.SPLocal);
        this.isOneDriveSourceChecked = selectedSourcesIndexs.includes(Constants.RuleSourceTabIndex.OneDrive);
        this.isAzureFileSourceChecked = selectedSourcesIndexs.includes(Constants.RuleSourceTabIndex.AzureFile);
        this.isBoxSourceChecked = selectedSourcesIndexs.includes(Constants.RuleSourceTabIndex.Box);
        this.IsGoogleDriveSourceChecked = selectedSourcesIndexs.includes(Constants.RuleSourceTabIndex.GoogleDrive);
        this.isTeamsSourceChecked = selectedSourcesIndexs.includes(Constants.RuleSourceTabIndex.Teams);
        this.isConnectorSourceChecked = selectedSourcesIndexs.includes(Constants.RuleSourceTabIndex.Connector);
        this.setState({
            ruleCriteriaTabsIndex: selectedSourcesIndexs[0],
            selectedSourcesIndexs: selectedSourcesIndexs,
            isSpSourceChecked: this.isSpSourceChecked,
            isExoSourceChecked: this.isExoSourceChecked,
            isPhySourceChecked: this.isPhySourceChecked,
            isFsSourceChecked: this.isFsSourceChecked,
            isSpLocalSourceChecked: this.isSpLocalSourceChecked,
            isOneDriveSourceChecked: this.isOneDriveSourceChecked,
            isAzureFileSourceChecked: this.isAzureFileSourceChecked,
            isBoxSourceChecked: this.isBoxSourceChecked,
            IsGoogleDriveSourceChecked:this.IsGoogleDriveSourceChecked,
            isTeamsSourceChecked: this.isTeamsSourceChecked,
            isConnectorSourceChecked: this.isConnectorSourceChecked,
        });
    }

    //判断What would you like to do with the content下的radio显示隐藏
    setArchiveActionDisplayByRuleLevel(ruleLevel, ruleModuleType) {
        switch (ruleLevel) {
            case this.RuleLevel.Document:
                this.setState({
                    isKeepShow: true,
                    isMoveShow: true,
                    isRemove: true,
                    isMove: false,
                    isKeep: false,
                    isIncludeDeclaredDisable: false,
                    isExportOnly: false,
                    isArchiveToAzureBlobStorage: false,
        
                    isMoveShowForOneDrive:true,

                    isKeepShowForLocal: true,
                    isMoveShowForLocal: false,
                    isRemoveForLocal: true,
                    isMoveForLocal: false,
                    isKeepForLocal: false,
                    isIncludeDeclaredDisableForLocal: false,
                    isExportOnlyForLocal: false,
                    isSeparateArchive: true,
                    isStoreInM365ArchiveShow: this.enableRecordsArchiver,
                    isStoreInM365Archive: false,

                    // Teams
                    isKeepShowForTeams: true,
                    isMoveShowForTeams: true,
                    isRemoveForTeams: true,
                    isMoveForTeams: false,
                    isKeepForTeams: false,
                    isExportOnlyForTeams: false,
                    isArchiveToAzureBlobStorageForTeams: false,
                });
                if (this.state.isExoMove) {
                    this.setState({
                        isExoRemove: true,
                        isExoMove: false
                    });
                }
                if (this.state.isMoveForLocal) {
                    this.setState({
                        isRemoveForLocal: true,
                        isMoveForLocal: false,
                    });
                }
                if (this.state.isMoveForOneDrive) {
                    this.setState({
                        isRemoveForOneDrive: true,
                        isMoveForOneDrive: false,
                    });
                }
                if (this.state.isGoogleMove) {
                    this.setState({
                        isGoogleDriveRemove: true,
                        isGoogleMove: false,
                    });
                }
                if (this.state.isMoveForTeams) {
                    this.setState({
                        isRemoveForTeams: true,
                        isMoveForTeams: false,
                    });
                }
                this.isRemove = true;
                this.isMove = false;
                this.isKeep = false;
                this.isRemoveForTeams = true;
                this.isMoveForTeams = false;
                this.isKeepForTeams = false;
                break;
            case this.RuleLevel.Item:
            case this.RuleLevel.Folder:
                this.setState({
                    isKeepShow: true,
                    isMoveShow: false,
                    isMoveShowForOneDrive: false,
                    isRemove: true,
                    isMove: false,
                    isKeep: false,
                    isIncludeDeclaredDisable: true,
                    isExportOnly: false,
                    isArchiveToAzureBlobStorage: false,
                    isKeepShowForLocal: true,
                    isMoveShowForLocal: false,
                    isRemoveForLocal: true,
                    isMoveForLocal: false,
                    isKeepForLocal: false,
                    isIncludeDeclaredDisableForLocal: true,
                    isExportOnlyForLocal: false,
                    isSeparateArchive: false,
                    isArchiveToAzureBlobStorageForTeams: false,
                    isKeepForTeams: false,
                    isKeepShowForTeams: true,
                    isExportOnlyForTeams: false,
                    isMoveShowForTeams: false,
                    isStoreInM365ArchiveShow: false,
                    isStoreInM365Archive: false,
                });
                if (this.state.isPhyMove) {
                    this.setState({
                        isPhyRemove: true,
                        isPhyMove: false
                    });
                }
                if (ruleLevel == this.RuleLevel.Item) {
                    this.setState({
                        isIncludeDeclaredDisable: false,
                        isIncludeDeclaredDisableForLocal: false
                    });
                }
                if (this.enableRecordsArchiver && ruleModuleType === RuleModuleTypes.SOArchiver && ruleLevel == this.RuleLevel.Folder) {
                    this.setState({
                        isMoveShow: true,
                        isMoveShowForOneDrive: true,
                        isMoveShowForTeams: true,
                    });
                }
                this.isRemove = true;
                this.isMove = false;
                this.isRemoveForTeams = true;
                this.isMoveForTeams = false;
                break;
            case this.RuleLevel.List:
            case this.RuleLevel.Site:
            case this.RuleLevel.SiteCollection:
            case this.RuleLevel.Attachment:
            case this.RuleLevel.DocumentVersion:
            case this.RuleLevel.ItemVersion:
                this.setState({
                    isKeepShow: false,
                    isMoveShow: false,
                    isIncludeDeclaredDisable: true,
                    isArchiveToAzureBlobStorage: false,
                    isSeparateArchive: false,
                    isMoveShowForOneDrive: false,
                    isKeepShowForOneDrive: false,
                    isIncludeDeclaredDisableForOneDrive: true,
                    isArchiveToAzureBlobStorageForTeams: false,
                    isKeepShowForTeams: false,
                    isMoveShowForTeams: false,
                    isStoreInM365ArchiveShow: this.enableRecordsArchiver && ruleLevel === this.RuleLevel.SiteCollection,
                    isStoreInM365Archive: false,
                });
                if (this.state.isMove || this.state.isKeep || this.state.isExportOnly) {
                    this.setState({
                        isRemove: true,
                        isMove: false,
                        isKeep: false,
                        isExportOnly: false
                    });
                }
                if (this.state.isMoveForTeams || this.state.isKeepForTeams || this.state.isExportOnlyForTeams) {
                    this.setState({
                        isRemoveForTeams: true,
                        isMoveForTeams: false,
                        isKeepForTeams: false,
                        isExportOnlyForTeams: false
                    });
                }
                if (this.state.isPhyMove) {
                    this.setState({
                        isPhyRemove: true,
                        isPhyMove: false
                    });
                }
                if(ruleLevel === this.RuleLevel.Attachment || ruleLevel === this.RuleLevel.DocumentVersion || ruleLevel === this.RuleLevel.ItemVersion){
                    this.setState({
                        showExportOnly: false,
                        isShowExportOnlyForOneDrive: false,
                        isShowExportOnlyForTeams: false,
                    });
                } 
                if (ruleModuleType === RuleModuleTypes.SOArchiver && ruleLevel === this.RuleLevel.SiteCollection) {
                    this.setState({
                        isShowExportOnlyForOneDrive: false,
                        isShowExportCheckboxForOneDrive: false,
                    });
                }
                this.isRemove = true;
                this.isMove = false;
                this.isKeep = false;
                this.isRemoveForTeams = true;
                this.isMoveForTeams = false;
                this.isKeepForTeams = false;
                break;
            case this.RuleLevel.Teams:
                this.setState({
                    isKeepShowForTeams: false,
                    isShowExportOnlyForTeams: false
                });
                break;
        }
    }

    //Remove content from SharePoint and destroy radio
    removeCheckedChange () {
        //显示 Leave a stub in place for each document following disposal
        if (this.levelId == 64) {
            this.setState({ isShowLeaveStubOption: true });
        } else {
            this.setState({ isShowLeaveStubOption: false });
        }
        //显示 Include related records
        if (this.levelId == 64 || this.levelId == 32) {
            this.setState({ isShowDeleteRelatedRecordOption: true });
        } else {
            this.setState({ isShowDeleteRelatedRecordOption: false });
        }
        this.setState({
            isShowDeclareOption: false,
            isShowDeclaredFileOption: true,
            isRemove: true,
            isKeep: false,
            isMove: false,
            isStoreInM365Archive: false,
            isArchiveWithoutDestroy: false,
            isExportOnly: false,
            isArchiveToAzureBlobStorage: false,
            isIncludeLockedFile: false,
            isRetentionLabel: false,
            isLeaveStubOption: false,
            isDeclareLinkFile: false,
            isDeclaredFile: false,
            isDeleteToRecycleBinForSPO: false,
            isArchivingRecordOption: false,
            isBackupOption: true
        });
        this.isRemove = true;
        this.isKeep = false;
        this.isMove = false;
        this.dispatch("spApproval", Constants.dispatchAction.approvalCheckboxDisabledAndChecked, false, true);
        this.resetExportOption(Constants.RuleSourceTabIndex.SP);
    }

    teamsDestroyCheckedChange = () => {
        if (this.levelId == 64) {
            this.setState({ isShowLeaveStubOptionForTeams: true });
        } else {
            this.setState({ isShowLeaveStubOptionForTeams: false });
        }

        if (this.levelId == 64 || this.levelId == 32) {
            this.setState({ isShowDeleteRelatedRecordOptionForTeams: true });
        } else {
            this.setState({ isShowDeleteRelatedRecordOptionForTeams: false });
        }
        this.setState({
            isShowDeclareOptionForTeams: false,
            isShowDeclaredFileOptionForTeams: true,
            isRemoveForTeams: true,
            isKeepForTeams: false,
            isMoveForTeams: false,
            isExportOnlyForTeams: false,
            isArchiveToAzureBlobStorageForTeams: false,
            isRetentionLabelForTeams: false,
            isLeaveStubOptionForTeams: false,
            isDeclareLinkFileForTeams: false,
            isDeclaredFileForTeams: false,
            isArchivingRecordOptionForTeams: false
        });
        this.isRemoveForTeams = true;
        this.isKeepForTeams = false;
        this.isMoveForTeams = false;
        this.dispatch("teamsApproval", Constants.dispatchAction.approvalCheckboxDisabledAndChecked, false, true);
        this.resetExportOption(Constants.RuleSourceTabIndex.Teams);
    }

    teamsDeclareOrTagCheckedChange = () => {
        if (this.levelId == 64 || this.levelId == 32) {
            this.setState({ isShowDeclareOptionForTeams: true });
        } else {
            this.setState({ isShowDeclareOptionForTeams: false });
        }
        this.setState({
            isRemoveForTeams: false,
            isKeepForTeams: true,
            isMoveForTeams: false,
            isExportOnlyForTeams: false,
            isArchiveToAzureBlobStorageForTeams: false,
            isBackupAndRemoveForTeams: false,
            isLeaveStubOptionForTeams: false,
            isSORemoveForTeams: false,
            isKeepVersionOptionForTeams: false,
        });
        this.isRemoveForTeams = false;
        this.isKeepForTeams = true;
        this.isMoveForTeams = false;
        this.dispatch("teamsApproval", Constants.dispatchAction.approvalCheckboxDisabledAndChecked, true, false);
        this.resetExportOption(Constants.RuleSourceTabIndex.Teams);
    }

    backupAndRemove = () => {
        this.setState({
            isRemove: false,
            isKeep: false,
            isMove: false,
            isStoreInM365Archive: false,
            isArchiveWithoutDestroy: false,
            isExportOnly: false,
            isArchiveToAzureBlobStorage: false,
            isBackupAndRemoveForSPO: true,
            isLeaveStubOption: false,
            isSORemoveForSPO: false,
            isKeepVersionOption: false,
            isSODeleteToRecycleBinForSPO: false,
            isArchiveVersionOption: false,
            archiveVersionValue: '0',
        });
        this.isRemove = false;
        this.isKeep = false;
        this.isMove = false;
        this.resetExportOption(Constants.RuleSourceTabIndex.SP);
    }

    backupAndRemoveForTeams = () => {
        this.setState({
            isRemoveForTeams: false,
            isKeepForTeams: false,
            isMoveForTeams: false,
            isExportOnlyForTeams: false,
            isArchiveToAzureBlobStorageForTeams: false,
            isBackupAndRemoveForTeams: true,
            isLeaveStubOptionForTeams: false,
            isSORemoveForTeams: false,
            isKeepVersionOptionForTeams: false,
        });
        this.isRemoveForTeams = false;
        this.isKeepForTeams = false;
        this.isMoveForTeams = false;
        this.resetExportOption(Constants.RuleSourceTabIndex.Teams);
    }

    soRemoveForSPO = () => {
        const notAllowDisplayDestroyOptionSOForSPO = [Constants.RuleLevel.DocumentVersion];
        const isKeepVersionOption = this.state.selectedRuleModuleType == RuleModuleTypes.SOArchiver && !notAllowDisplayDestroyOptionSOForSPO.includes(this.levelId);

        this.setState({
            isRemove: false,
            isKeep: false,
            isMove: false,
            isStoreInM365Archive: false,
            isArchiveWithoutDestroy: false,
            isExportOnly: false,
            isArchiveToAzureBlobStorage: false,
            isBackupAndRemoveForSPO: false,
            isLeaveStubOption: false,
            isSORemoveForSPO: true,
            isKeepVersionOption,
            isSODeleteToRecycleBinForSPO: false,
        });
        this.isRemove = false;
        this.isKeep = false;
        this.isMove = false;
        this.resetExportOption(Constants.RuleSourceTabIndex.SP);
    }

    soRemoveForTeams = () => {
        this.setState({
            isRemoveForTeams: false,
            isKeepForTeams: false,
            isMoveForTeams: false,
            isExportOnlyForTeams: false,
            isArchiveToAzureBlobStorageForTeams: false,
            isBackupAndRemoveForTeams: false,
            isLeaveStubOptionForTeams: false,
            isSORemoveForTeams: true,
            isKeepVersionOptionForTeams: true,
        });
        this.isRemoveForTeams = false;
        this.isKeepForTeams = false;
        this.isMoveForTeams = false;
        this.resetExportOption(Constants.RuleSourceTabIndex.Teams);
    }

    //Record Declaration and Tagging radio
    keepCheckedChange () {
        if ((this.is21VEnv || !this.enableRecordsArchiver) && (this.levelId == 64 || this.levelId == 32)) {
            this.setState({ isShowDeclareOption: true });
        } else {
            this.setState({ isShowDeclareOption: false });
        }
        this.setState({
            isRemove: false,
            isKeep: true,
            isMove: false,
            isStoreInM365Archive: false,
            isArchiveWithoutDestroy: false,
            isExportOnly: false,
            isArchiveToAzureBlobStorage: false,
            isBackupAndRemoveForSPO: false,
            isLeaveStubOption: false,
            isSORemoveForSPO: false,
            isKeepVersionOption: false,
            isSODeleteToRecycleBinForSPO: false,
            isBackupOption: false
        });
        this.isRemove = false;
        this.isKeep = true;
        this.isMove = false;
        this.dispatch("spApproval", Constants.dispatchAction.approvalCheckboxDisabledAndChecked, true, false);
        this.resetExportOption(Constants.RuleSourceTabIndex.SP);
    }
    
    //sp export only change
    spExportOnlyCheckedChange (){
        this.setState({
            isRemove: false,
            isKeep: false,
            isMove: false,
            isStoreInM365Archive: false,
            isArchiveWithoutDestroy: false,
            isExportOnly: true,
            isArchiveToAzureBlobStorage: false,
            isBackupAndRemoveForSPO: false,
            isLeaveStubOption: false,
            isSORemoveForSPO: false,
            isKeepVersionOption: false,
            isSODeleteToRecycleBinForSPO: false,
            isBackupOption: false
        });
        this.dispatch("spApproval", Constants.dispatchAction.approvalCheckboxDisabledAndChecked, true, false);
        // this.resetExportOnlyOption(Constants.RuleSourceTabIndex.SP);
        this.disableExportOption(Constants.RuleSourceTabIndex.SP);
    }

    spArchiveWithoutDestroyCheckedChange = () => {
        this.setState({
            isRemove: false,
            isKeep: false,
            isMove: false,
            isStoreInM365Archive: false,
            isArchiveWithoutDestroy: true,
            isExportOnly: false,
            isArchiveToAzureBlobStorage: false,
            isBackupAndRemoveForSPO: false,
            isLeaveStubOption: false,
            isSORemoveForSPO: false,
            isKeepVersionOption: false,
            isSODeleteToRecycleBinForSPO: false,
            isArchiveVersionOption: false,
            archiveVersionValue: '0',
        });
        this.isRemove = false;
        this.isKeep = false;
        this.isMove = false;
        this.resetExportOption(Constants.RuleSourceTabIndex.SP);
    }

    //moveCheckedChange
    moveCheckedChange () {
        this.setState({
            isRemove: false,
            isKeep: false,
            isMove: true,
            isStoreInM365Archive: false,
            isArchiveWithoutDestroy: false,
            isExportOnly: false,
            isSpecifyLocation: true,
            isArchiveToAzureBlobStorage: false,
            isBackupAndRemoveForSPO: false,
            isLeaveStubOption: false,
            isSORemoveForSPO: false,
            isKeepVersionOption: false,
            isSODeleteToRecycleBinForSPO: false,
            isIncludeLockedFile: false,
            isRetentionLabel: false,
            isBackupOption: false
        });
        this.isRemove = false;
        this.isKeep = false;
        this.isMove = true;
        this.dispatch("spApproval", Constants.dispatchAction.approvalCheckboxDisabledAndChecked, true, false);
        this.disableExportOption(Constants.RuleSourceTabIndex.SP);
    }

    //storeInM365ArchiveCheckedChange
    storeInM365ArchiveCheckedChange = () => { 
        this.setState({
            isRemove: false,
            isKeep: false,
            isMove: false,
            isStoreInM365Archive: true,
            isArchiveWithoutDestroy: false,
            isExportOnly: false,
            isArchiveToAzureBlobStorage: false,
            isBackupAndRemoveForSPO: false,
            isLeaveStubOption: false,
            isSORemoveForSPO: false,
            isKeepVersionOption: false,
            isSODeleteToRecycleBinForSPO: false,
            isArchiveVersionOption: false,
            isBackupOption: false,
        });
        this.isRemove = false;
        this.isKeep = false;
        this.isMove = false;
        this.dispatch("spApproval", Constants.dispatchAction.approvalCheckboxDisabledAndChecked, true, false);
        this.disableExportOption(Constants.RuleSourceTabIndex.SP);
    }

    moveCheckedChangeForTeams = () => {
        this.setState({
            isRemoveForTeams: false,
            isKeepForTeams: false,
            isMoveForTeams: true,
            isExportOnlyForTeams: false,
            isSpecifyLocationForTeams: true,
            isArchiveToAzureBlobStorageForTeams: false,
            isBackupAndRemoveForTeams: false,
            isLeaveStubOptionForTeams: false,
            isSORemoveForTeams: false,
            isKeepVersionOptionForTeams: false,
            isRetentionLabelForTeams: false,
        });
        this.isRemoveForTeams = false;
        this.isKeepForTeams = false;
        this.isMoveForTeams = true;
        this.dispatch("teamsApproval", Constants.dispatchAction.approvalCheckboxDisabledAndChecked, true, false);
        this.disableExportOption(Constants.RuleSourceTabIndex.Teams);
    }

    removeCheckedChangeForLocal () {
        //显示 Leave a stub in place for each document following disposal
        if (this.levelId == 64) {
            this.setState({ isShowLeaveStubOptionForLocal: true });
        } else {
            this.setState({ isShowLeaveStubOptionForLocal: false });
        }
        //显示 Include related records
        if (this.levelId == 64 || this.levelId == 32) {
            this.setState({ isShowDeleteRelatedRecordOptionForLocal: true });
        } else {
            this.setState({ isShowDeleteRelatedRecordOptionForLocal: false });
        }
        this.setState({
            isShowDeclareOptionForLocal: false,
            isShowDeclaredFileOptionForLocal: true,
            isRemoveForLocal: true,
            isKeepForLocal: false,
            isMoveForLocal: false,
            isExportOnlyForLocal: false,
            isDeclareLinkFileForLocal: false
        });
        this.isRemoveForLocal = true;
        this.isKeepForLocal = false;
        this.isMoveForLocal = false;
        this.dispatch("spLocalApproval", Constants.dispatchAction.approvalCheckboxDisabledAndChecked, false, true);
        this.resetExportOption(Constants.RuleSourceTabIndex.SPLocal);
    }

    //Record Declaration and Tagging radio
    keepCheckedChangeForLocal () {
        if (this.levelId == 64 || this.levelId == 32) {
            this.setState({ isShowDeclareOptionForLocal: true });
        } else {
            this.setState({ isShowDeclareOptionForLocal: false });
        }
        this.setState({
            isRemoveForLocal: false,
            isKeepForLocal: true,
            isMoveForLocal: false,
            isExportOnlyForLocal: false
        });
        this.isRemoveForLocal = false;
        this.isKeepForLocal = true;
        this.isMoveForLocal = false;
        this.dispatch("spLocalApproval", Constants.dispatchAction.approvalCheckboxDisabledAndChecked, true, false);
        this.resetExportOption(Constants.RuleSourceTabIndex.SPLocal);
    }
    
    //sp export only change
    spExportOnlyCheckedChangeForLocal (){
        this.setState({
            isRemoveForLocal: false,
            isKeepForLocal: false,
            isMoveForLocal: false,
            isExportOnlyForLocal: true
        });
        this.dispatch("spLocalApproval", Constants.dispatchAction.approvalCheckboxDisabledAndChecked, true, false);
        // this.resetExportOnlyOption(Constants.RuleSourceTabIndex.SPLocal);
        this.disableExportOption(Constants.RuleSourceTabIndex.SPLocal);
    }

    //moveCheckedChange
    moveCheckedChangeForLocal () {
        this.setState({
            isRemoveForLocal: false,
            isKeepForLocal: false,
            isMoveForLocal: true,
            isExportOnlyForLocal: false,
            isSpecifyLocationForLocal: true
        });
        this.isRemoveForLocal = false;
        this.isKeepForLocal = false;
        this.isMoveForLocal = true;
        this.dispatch("spLocalApproval", Constants.dispatchAction.approvalCheckboxDisabledAndChecked, true, false);
        this.disableExportOption(Constants.RuleSourceTabIndex.SPLocal);
    }

    azureFileRemoveCheckedChange = (isChecked) =>{
        this.setState({
            isAzureFileRemove: isChecked,
        });
    }

    boxRemoveCheckedChange = (isChecked) =>{
        this.setState({
            isBoxRemove: isChecked,
        });
    }

    // Google Drive
    googleDriveRemoveCheckedChange = () => {
        this.setState({
            isGoogleDriveRemove: true,
            isArchiveToStorageForGoogle: false,
            isGoogleExportOnly: false,
            isGoogleMove: false,
        });
        this.dispatch("googleDriveApproval", Constants.dispatchAction.approvalCheckboxDisabledAndChecked, false, true);
        this.resetExportOption(Constants.RuleSourceTabIndex.GoogleDrive);
    }

    googleArchiveStorageCheckedChange = () => {
        this.setState({
            isGoogleDriveRemove: false,
            isArchiveToStorageForGoogle: true,
            isGoogleExportOnly: false,
            isGoogleMove: false,
        });
        this.dispatch("googleDriveApproval", Constants.dispatchAction.approvalCheckboxDisabledAndChecked, false, true);
        this.resetExportOption(Constants.RuleSourceTabIndex.GoogleDrive);
    }

    googleExportOnlyCheckedChange = () => {
        this.setState({
            isGoogleDriveRemove: false,
            isArchiveToStorageForGoogle: false,
            isGoogleExportOnly: true,
            isGoogleMove: false,
        });
        this.dispatch("googleDriveApproval", Constants.dispatchAction.approvalCheckboxDisabledAndChecked, true, false);
        this.disableExportOption(Constants.RuleSourceTabIndex.GoogleDrive);
    }

    googleMoveCheckedChange = () => {
        this.setState({
            isGoogleDriveRemove: false,
            isArchiveToStorageForGoogle: false,
            isGoogleExportOnly: false,
            isGoogleMove: true,
        });
        this.dispatch("googleDriveApproval", Constants.dispatchAction.approvalCheckboxDisabledAndChecked, true, false);
        this.disableExportOption(Constants.RuleSourceTabIndex.GoogleDrive);
    }

    //Tag each document/item with 点击
    keepTagCheckedForLocal () {
        this.initValidStatusForLocal();
    }

    initValidStatusForLocal = function () {
        this.setState({
            noDateValueForLocal: false,
            noMetadateValueForLocal: false,
            noNumberValueForLocal: false,
            noTagsForLocal: false,
            noSelectForLocal: false,
        });
    };

    //Record Declaration and Tagging验证
    declareCheckedForLocal () {
        this.setState({
            isDeclareForLocal: !this.state.isDeclareForLocal,
            isUndeclareForLocal: false,
            noSelectForLocal: false
        });
    }

    //Record UnDeclaration and Tagging--验证
    undeclareCheckedForLocal () {
        this.setState({
            isUndeclareForLocal: !this.state.isUndeclareForLocal,
            isDeclareForLocal: false,
            noSelectForLocal: false
        });
    }

    //Custom Metadata 第一个选项框
    tagTypeSelectChangedForLocal (args) {
        let newvalue = args.newValue,
            nameValidate = false;
        this.setState({
            slectTagTypeForLocal: newvalue
        });
        if (this.state.metadataNameForLocal != "") {
            nameValidate = true;
        }
        if (typeof (newvalue) != "undefined") {
            this.setState({
                tagTypeValueForLocal: newvalue.id
            });
            if (newvalue.id == this.TagType.DateTime) {//DateTime
                this.setState({
                    isTagDateForLocal: true,
                    isTagTextForLocal: false,
                    isTagBooleanForLocal: false
                });
                if (!this.state.currentDateForLocal && nameValidate) {
                    this.setState({
                        noMetadateValueForLocal: false,
                        noDateValueForLocal: false,
                        noNumberValueForLocal: false
                    });
                }
            } else if (newvalue.id == this.TagType.YesNo) {//Bollean
                this.setState({
                    isTagDateForLocal: false,
                    isTagTextForLocal: false,
                    isTagBooleanForLocal: true
                });
                if (nameValidate) {
                    this.setState({
                        noMetadateValueForLocal: false,
                        noDateValueForLocal: false,
                        noNumberValueForLocal: false
                    });
                }
            } else {//Text || Number
                this.setState({
                    isTagDateForLocal: false,
                    isTagTextForLocal: true,
                    isTagBooleanForLocal: false
                });
                if (newvalue.id == this.TagType.Text) {//text
                    if ($.trim(this.state.metadataValueForLocal) != "" && nameValidate) {
                        this.setState({
                            noDateValueForLocal: false
                        });
                    }
                } else {//number
                    if (!isNaN(this.state.metadataValueForLocal) && nameValidate) {
                        this.setState({
                            noDateValueForLocal: false
                        });
                    }
                }
            }
        }
    }

    //选择日期
    metadataDateSelecteChangeForLocal (args) {
        this.setState({
            currentDateForLocal: args.newValue
        }, () => {
            this.spLocalArchiveActionCustomValidate();
        });
    }

    onRetentionActionCheckChangeForLocal () {
        this.setState({
            retentionActionCheckedForLocal: !this.state.retentionActionCheckedForLocal
        }, () => {
            if (!this.state.retentionActionCheckedForLocal) {
                this.setState({
                    noRetentionActionValueForLocal: false
                });
            }
        });
    }

    //onedrive
    //Remove content from OneDrive and destroy radio
    removeCheckedChangeForOneDrive () {
        //显示 Leave a stub in place for each document following disposal
        if (this.levelId == 64) {
            this.setState({ isShowLeaveStubOptionForOneDrive: true });
        } else {
            this.setState({ isShowLeaveStubOptionForOneDrive: false });
        }
        //显示 Include related records
        this.setState({ isShowDeleteRelatedRecordOptionForOneDrive: false });
        this.setState({
            isShowDeclareOptionForOneDrive: false,
            isShowDeclaredFileOptionForOneDrive: true,
            isRemoveForOneDrive: true,
            isKeepForOneDrive: false,
            isMoveForOneDrive: false,
            isExportOnlyForOneDrive: false,
            isArchiveToAzureBlobStorageForOneDrive: false,
            isIncludeLockedFileForOneDrive: false,
            isArchiveWithoutDestroyForOneDrive: false,
            isLeaveStubOptionForOneDrive: false,
            isRetentionLabelForOneDrive: false,
            isDeclareLinkFileForOneDrive: false,
            isDeclaredFileForOneDrive: false,
            isDeleteToRecycleBinForOneDrive: false,
            isArchivingRecordOptionForOneDrive: false,
            isBackupOptionForOneDrive: true
        });
        this.isRemoveForOneDrive = true;
        this.isKeepForOneDrive = false;
        this.isMoveForOneDrive = false;
        this.dispatch("oneDriveApproval", Constants.dispatchAction.approvalCheckboxDisabledAndChecked, false, true);
        this.resetExportOption(Constants.RuleSourceTabIndex.OneDrive);
    }

    backupAndRemoveForOneDrive = () => {
        this.setState({
            isRemoveForOneDrive: false,
            isKeepForOneDrive: false,
            isMoveForOneDrive: false,
            isExportOnlyForOneDrive: false,
            isArchiveToAzureBlobStorageForOneDrive: false,
            isBackupAndRemoveForOD: true,
            isArchiveWithoutDestroyForOneDrive: false,
            isLeaveStubOptionForOneDrive: false,
            isSORemoveForOD: false,
            isKeepVersionOptionForOD: false,
            isSODeleteToRecycleBinForOD: false,
            isArchiveVersionOptionForOD: false,
            archiveVersionValueForOD: '0',
        });
        this.isRemoveForOneDrive = false;
        this.isKeepForOneDrive = false;
        this.isMoveForOneDrive = false;
        this.resetExportOption(Constants.RuleSourceTabIndex.OneDrive);
    }

    soRemoveForOneDrive = () => {
        const notAllowDisplayDestroyOptionSOForOD = [Constants.RuleLevel.DocumentVersion];
        const isKeepVersionOptionForOD = this.state.selectedRuleModuleType == RuleModuleTypes.SOArchiver && !notAllowDisplayDestroyOptionSOForOD.includes(this.levelId);

        this.setState({
            isRemoveForOneDrive: false,
            isKeepForOneDrive: false,
            isMoveForOneDrive: false,
            isExportOnlyForOneDrive: false,
            isArchiveWithoutDestroyForOneDrive: false,
            isArchiveToAzureBlobStorageForOneDrive: false,
            isBackupAndRemoveForOD: false,
            isLeaveStubOptionForOneDrive: false,
            isSORemoveForOD: true,
            isKeepVersionOptionForOD,
            isSODeleteToRecycleBinForOD: false
        });
        this.isRemoveForOneDrive = false;
        this.isKeepForOneDrive = false;
        this.isMoveForOneDrive = false;
        this.resetExportOption(Constants.RuleSourceTabIndex.OneDrive);
    }

    //Record Declaration and Tagging radio
    keepCheckedChangeForOneDrive () {
        if ((this.is21VEnv || !this.enableRecordsArchiver) && (this.levelId == 64 || this.levelId == 32)) {
            this.setState({ isShowDeclareOptionForOneDrive: true });
        } else {
            this.setState({ isShowDeclareOptionForOneDrive: false });
        }
        this.setState({
            isRemoveForOneDrive: false,
            isKeepForOneDrive: true,
            isMoveForOneDrive: false,
            isExportOnlyForOneDrive: false,
            isArchiveToAzureBlobStorageForOneDrive: false,
            isArchiveWithoutDestroyForOneDrive: false,
            isBackupAndRemoveForOD: false,
            isLeaveStubOptionForOneDrive: false,
            isSORemoveForOD: false,
            isKeepVersionOptionForOD: false,
            isSODeleteToRecycleBinForOD: false,
            isBackupOptionForOneDrive: false
        });
        this.isRemoveForOneDrive = false;
        this.isKeepForOneDrive = true;
        this.isMoveForOneDrive = false;
        this.dispatch("oneDriveApproval", Constants.dispatchAction.approvalCheckboxDisabledAndChecked, true, false);
        this.resetExportOption(Constants.RuleSourceTabIndex.OneDrive);
    }
    
    //onedrive export only change
    oneDriveExportOnlyCheckedChange (){
        this.setState({
            isRemoveForOneDrive: false,
            isKeepForOneDrive: false,
            isMoveForOneDrive: false,
            isExportOnlyForOneDrive: true,
            isArchiveWithoutDestroyForOneDrive: false,
            isArchiveToAzureBlobStorageForOneDrive: false,
            isBackupAndRemoveForOD: false,
            isLeaveStubOptionForOneDrive: false,
            isSORemoveForOD: false,
            isKeepVersionOptionForOD: false,
            isBackupOptionForOneDrive: false,
        });
        this.dispatch("oneDriveApproval", Constants.dispatchAction.approvalCheckboxDisabledAndChecked, true, false);
        // this.resetExportOnlyOption(Constants.RuleSourceTabIndex.OneDrive);
        this.disableExportOption(Constants.RuleSourceTabIndex.OneDrive);
    }

    odArchiveWithoutDestroyCheckedChange = () => {
        this.setState({
            isRemoveForOneDrive: false,
            isKeepForOneDrive: false,
            isMoveForOneDrive: false,
            isExportOnlyForOneDrive: false,
            isSpecifyLocationForOneDrive: false,
            isArchiveToAzureBlobStorageForOneDrive: false,
            isBackupAndRemoveForOD: false,
            isLeaveStubOptionForOneDrive: false,
            isSORemoveForOD: false,
            isArchiveWithoutDestroyForOneDrive: true,
            isKeepVersionOptionForOD: false,
            isSODeleteToRecycleBinForOD: false,
            isRetentionLabelForOneDrive: false,
            isArchiveVersionOptionForOD: false,
            archiveVersionValueForOD: '0',
        });
        this.isRemoveForOneDrive = false;
        this.isKeepForOneDrive = false;
        this.isMoveForOneDrive = true;
        this.resetExportOption(Constants.RuleSourceTabIndex.OneDrive);
    }

    //moveCheckedChange
    moveCheckedChangeForOneDrive () {
        this.setState({
            isRemoveForOneDrive: false,
            isKeepForOneDrive: false,
            isMoveForOneDrive: true,
            isExportOnlyForOneDrive: false,
            isSpecifyLocationForOneDrive: true,
            isArchiveToAzureBlobStorageForOneDrive: false,
            isBackupAndRemoveForOD: false,
            isLeaveStubOptionForOneDrive: false,
            isSORemoveForOD: false,
            isKeepVersionOptionForOD: false,
            isIncludeLockedFileForOneDrive: false,
            isRetentionLabelForOneDrive: false,
            isBackupOptionForOneDrive: false
        });
        this.isRemoveForOneDrive = false;
        this.isKeepForOneDrive = false;
        this.isMoveForOneDrive = true;
        this.dispatch("oneDriveApproval", Constants.dispatchAction.approvalCheckboxDisabledAndChecked, true, false);
        this.disableExportOption(Constants.RuleSourceTabIndex.OneDrive);
    }

    keepTagCheckedForOneDrive () {
        this.initValidStatusForOneDrive();
    }

    initValidStatusForOneDrive = function () {
        this.setState({
            noDateValueForOneDrive: false,
            noMetadateValueForOneDrive: false,
            noNumberValueForOneDrive: false,
            noTagsForOneDrive: false,
            noSelectForOneDrive: false,
        });
    };

    //Record Declaration and Tagging验证
    declareCheckedForOneDrive () {
        this.setState({
            isDeclareForOneDrive: !this.state.isDeclareForOneDrive,
            isUndeclareForOneDrive: false,
            noSelectForOneDrive: false
        });
    }

    //Record UnDeclaration and Tagging--验证
    undeclareCheckedForOneDrive () {
        this.setState({
            isUndeclareForOneDrive: !this.state.isUndeclareForOneDrive,
            isDeclareForOneDrive: false,
            noSelectForOneDrive: false
        });
    }

    //Custom Metadata 第一个选项框
    tagTypeSelectChangedForOneDrive (args) {
        let newvalue = args.newValue,
            nameValidate = false;
        this.setState({
            slectTagTypeForOneDrive: newvalue
        });
        if (this.state.metadataNameForOneDrive != "") {
            nameValidate = true;
        }
        if (typeof (newvalue) != "undefined") {
            this.setState({
                tagTypeValueForOneDrive: newvalue.id
            });
            if (newvalue.id == this.TagType.DateTime) {//DateTime
                this.setState({
                    isTagDateForOneDrive: true,
                    isTagTextForOneDrive: false,
                    isTagBooleanForOneDrive: false
                });
                if (!this.state.currentDateForOneDrive && nameValidate) {
                    this.setState({
                        noMetadateValueForOneDrive: false,
                        noDateValueForOneDrive: false,
                        noNumberValueForOneDrive: false
                    });
                }
            } else if (newvalue.id == this.TagType.YesNo) {//Bollean
                this.setState({
                    isTagDateForOneDrive: false,
                    isTagTextForOneDrive: false,
                    isTagBooleanForOneDrive: true
                });
                if (nameValidate) {
                    this.setState({
                        noMetadateValueForOneDrive: false,
                        noDateValueForOneDrive: false,
                        noNumberValueForOneDrive: false
                    });
                }
            } else {//Text || Number
                this.setState({
                    isTagDateForOneDrive: false,
                    isTagTextForOneDrive: true,
                    isTagBooleanForOneDrive: false
                });
                if (newvalue.id == this.TagType.Text) {//text
                    if ($.trim(this.state.metadataValueForOneDrive) != "" && nameValidate) {
                        this.setState({
                            noDateValueForOneDrive: false
                        });
                    }
                } else {//number
                    if (!isNaN(this.state.metadataValueForOneDrive) && nameValidate) {
                        this.setState({
                            noDateValueForOneDrive: false
                        });
                    }
                }
            }
        }
    }

    //选择日期
    metadataDateSelecteChangeForOneDrive (args) {
        this.setState({
            currentDateForOneDrive: args.newValue
        }, () => {
            this.oneDriveArchiveActionCustomValidate();
        });
    }

    onRetentionActionCheckChangeForOneDrive () {
        this.setState({
            retentionActionCheckedForOneDrive: !this.state.retentionActionCheckedForOneDrive
        }, () => {
            if (this.enableRecordsArchiver) {
                if (!this.state.retentionActionCheckedForOneDrive || (this.state.retentionActionCheckedForOneDrive && this.state.retentionRecordsLabelSelectedForOneDrive === Constants.RetentionLabelOptions.GetFromGeneralSetting)) {
                    this.setState({
                        noRetentionActionValueForOneDrive: false
                    });
                }
    
                if (this.state.retentionActionCheckedForOneDrive) {
                    this.onRetentionRecordsLabelOptionsChangeForOD(Constants.RetentionLabelOptions.GetFromGeneralSetting);
                }
            } else {
                if (!this.state.retentionActionCheckedForOneDrive) {
                    this.setState({
                        noRetentionActionValueForOneDrive: false
                    });
                }
            }
        });
    }
    //onedrive

    //fs remove change
    fsRemoveCheckedChange () {
        this.setState({
            isFsRemove: true,
            isArchiveToAzureBlobStorageForFS: false,
            isFsMove: false,
            noFsRetentionActionValue: false,
        });
        this.dispatch("fsApproval", Constants.dispatchAction.approvalCheckboxDisabledAndChecked, false, true);
        // this.dispatch("fsExport", Constants.dispatchAction.clearData, this.levelId, false);
    }

    //fs move change
    fsMoveCheckedChange () {
        this.setState({
            isFsRemove: false,
            isArchiveToAzureBlobStorageForFS: false,
            isFsMove: true
        });
        this.dispatch("fsApproval", Constants.dispatchAction.approvalCheckboxDisabledAndChecked, true, false);
        // this.dispatch("fsExport", Constants.dispatchAction.clearData, this.levelId, true);
    }

    onChangeArchiveToAzureBlobStorageForFS = () => {
        this.setState({
            isFsRemove: false,
            isArchiveToAzureBlobStorageForFS: true,
            isFsMove: false,
        });
        this.dispatch("fsApproval", Constants.dispatchAction.approvalCheckboxDisabledAndChecked, false, true);
    }

    //exo remove change
    exoRemoveCheckedChange () {
        this.setState({
            isExoRemove: true,
            isExoKeep: false,
            isExoMove: false,
            isExoExportOnly: false,
            noExoRetentionActionValue: false,
        });
        this.dispatch("exoApproval", Constants.dispatchAction.approvalCheckboxDisabledAndChecked, false, true);
        this.resetExportOption(Constants.RuleSourceTabIndex.Exchange);
    }

    //exo keep change
    exoKeepCheckedChange () {
        this.setState({
            isExoRemove: false,
            isExoKeep: true,
            isExoMove: false,
            isExoExportOnly: false
        });
        this.dispatch("exoApproval", Constants.dispatchAction.approvalCheckboxDisabledAndChecked, true, false);
        this.resetExportOption(Constants.RuleSourceTabIndex.Exchange);
    }

    //exoMoveCheckedChange
    exoMoveCheckedChange() {
        this.setState({
            isExoRemove: false,
            isExoKeep: false,
            isExoMove: true,
            isExoExportOnly: false,
            isExoSpecifyLocation: true,
            isExoMoveToSP: false,
        });
        this.isExoRemove = false;
        this.isExoKeep = false;
        this.isExoMove = true;
        this.dispatch("exoApproval", Constants.dispatchAction.approvalCheckboxDisabledAndChecked, true, false);
        this.disableExportOption(Constants.RuleSourceTabIndex.Exchange);
    }

    //exo export only change
    exoExportOnlyCheckedChange (){
        this.setState({
            isExoRemove: false,
            isExoKeep: false,
            isExoMove: false,
            isExoExportOnly: true
        });
        
        this.dispatch("exoApproval", Constants.dispatchAction.approvalCheckboxDisabledAndChecked, true, false);
        this.disableExportOption(Constants.RuleSourceTabIndex.Exchange);
        //this.dispatch(`exoExportOnly`, Constants.dispatchAction.clearData, this.levelId, false);
    }

    phyRemoveCheckedChange () {
        let isFolderLevel = this.levelId == 16,
            showRelatedOption = isFolderLevel,
            showRemoveBoxOption = isFolderLevel;

        this.setState({
            isPhyRemove: true,
            isPhyMove: false,
            isShowDeleteRelatedRecordOptionOfPhy: showRelatedOption,
            isShowDestoryEmptyBoxOnFolderRuleOptionOfPhy: showRemoveBoxOption,
            isCalculationDisposalDate: false,
        });
        this.dispatch("phyApproval", Constants.dispatchAction.approvalCheckboxDisabledAndChecked, false, true);
    }

    phyMoveCheckedChange () {
        this.setState({
            isPhyRemove: false,
            isPhyMove: true,
            isShowDeleteRelatedRecordOptionOfPhy: false,
            isShowDestoryEmptyBoxOnFolderRuleOptionOfPhy: false,
            isCalculationDisposalDate: false,
        });
        this.dispatch("phyApproval", Constants.dispatchAction.approvalCheckboxDisabledAndChecked, true, false);
    }

    phyCalculateDisposalDateCheckedChange = () => {
        this.setState({
            isPhyRemove: false,
            isPhyMove: false,
            isShowDeleteRelatedRecordOptionOfPhy: false,
            isShowDestoryEmptyBoxOnFolderRuleOptionOfPhy: false,
            isCalculationDisposalDate: true,
        });
        // approvalDisabled: true, isApproval: false
        this.dispatch("phyApproval", Constants.dispatchAction.approvalCheckboxDisabledAndChecked, true, false);
    }

    onSelectedNodeChanged = (nodeItem, treeData) => {
        this.selectedPhyTreeItem = nodeItem;
        //console.log(nodeItem)
        this.setState({
            phyTreeData: treeData,
            noSelectPhyNode: false
        });
    }

    //Tag each document/item with 点击
    keepTagChecked () {
        this.initValidStatus();
    }

    initValidStatus = function () {
        this.setState({
            noDateValue: false,
            noMetadateValue: false,
            noNumberValue: false,
            noTags: false,
            noSelect: false,
        });
    };

    disableExportOption(tabIndex)
    {
        this.setExportOption(tabIndex, true);
    }

    resetExportOption(tabIndex)
    {
        this.setExportOption(tabIndex, false);
    }

    setExportOption(tabIndex, isDisabled)
    {
        this.dispatch(`${this.getRuleSourceName(tabIndex)}Export`, Constants.dispatchAction.clearData, this.levelId, isDisabled);
    }
    
    resetExportOnlyOption(tabIndex)
    {
        this.dispatch(`${this.getRuleSourceName(tabIndex)}ExportOnly`, Constants.dispatchAction.clearData, this.levelId, false);
    }

    getRuleSourceName(tabIndex)
    {
        let name = "";
        switch(tabIndex)
        {
            case Constants.RuleSourceTabIndex.SP:
                name = "sp";
                break;
            case Constants.RuleSourceTabIndex.Exchange:
                name = "exo";
                break;
            case Constants.RuleSourceTabIndex.SPLocal:
                name = "spLocal";
                break;
            case Constants.RuleSourceTabIndex.OneDrive:
                name = "oneDrive";
                break;
            case Constants.RuleSourceTabIndex.Teams:
                name = "teams";
                break;
            case Constants.RuleSourceTabIndex.GoogleDrive:
                name = "google";
                break;
        }
        return name;
    }

    //Record Declaration and Tagging验证
    declareChecked () {
        this.setState({
            isDeclare: !this.state.isDeclare,
            isUndeclare: false,
            noSelect: false
        });
    }

    //Record UnDeclaration and Tagging--验证
    undeclareChecked () {
        this.setState({
            isUndeclare: !this.state.isUndeclare,
            isDeclare: false,
            noSelect: false
        });
    }

    //Custom Metadata 第一个选项框
    tagTypeSelectChanged (args) {
        let newvalue = args.newValue,
            nameValidate = false;
        this.setState({
            slectTagType: newvalue
        });
        if (this.state.metadataName != "") {
            nameValidate = true;
        }
        if (typeof (newvalue) != "undefined") {
            this.setState({
                tagTypeValue: newvalue.id
            });
            if (newvalue.id == this.TagType.DateTime) {//DateTime
                this.setState({
                    isTagDate: true,
                    isTagText: false,
                    isTagBoolean: false
                });
                if (!this.state.currentDate && nameValidate) {
                    this.setState({
                        noMetadateValue: false,
                        noDateValue: false,
                        noNumberValue: false
                    });
                }
            } else if (newvalue.id == this.TagType.YesNo) {//Bollean
                this.setState({
                    isTagDate: false,
                    isTagText: false,
                    isTagBoolean: true
                });
                if (nameValidate) {
                    this.setState({
                        noMetadateValue: false,
                        noDateValue: false,
                        noNumberValue: false
                    });
                }
            } else {//Text || Number
                this.setState({
                    isTagDate: false,
                    isTagText: true,
                    isTagBoolean: false
                });
                if (newvalue.id == this.TagType.Text) {//text
                    if ($.trim(this.state.metadataValue) != "" && nameValidate) {
                        this.setState({
                            noDateValue: false
                        });
                    }
                } else {//number
                    if (!isNaN(this.state.metadataValue) && nameValidate) {
                        this.setState({
                            noDateValue: false
                        });
                    }
                }
            }
        }
    }

    teamsDeclareChecked = () => {
        this.setState({
            isDeclareForTeams: !this.state.isDeclareForTeams,
            isUndeclareForTeams: false,
            noSelectForTeams: false
        });
    }

    //Record UnDeclaration and Tagging--验证
    teamsUndeclareChecked = () => {
        this.setState({
            isUndeclareForTeams: !this.state.isUndeclareForTeams,
            isDeclareForTeams: false,
            noSelectForTeams: false
        });
    }

    teamsKeepTagChecked = () => {
        this.initValidStatus();
    }

    teamsInitValidStatus = () => {
        this.setState({
            noDateValueForTeams: false,
            noMetadateValueForTeams: false,
            noNumberValueForTeams: false,
            noTagsForTeams: false,
            noSelectForTeams: false,
        });
    };

    teamsTagTypeSelectChanged = (args) => {
        const newvalue = args.newValue;
        let nameValidate = false;
        this.setState({
            selectTagTypeForTeams: newvalue
        });
        if (this.state.metadataNameForTeams != "") {
            nameValidate = true;
        }
        if (typeof (newvalue) != "undefined") {
            this.setState({
                tagTypeValueForTeams: newvalue.id,
            });
            if (newvalue.id == this.TagType.DateTime) {//DateTime
                this.setState({
                    isTagDateForTeams: true,
                    isTagTextForTeams: false,
                    isTagBooleanForTeams: false
                });
                if (!this.state.currentDateForTeams && nameValidate) {
                    this.setState({
                        noMetadateValueForTeams: false,
                        noDateValueForTeams: false,
                        noNumberValueForTeams: false
                    });
                }
            } else if (newvalue.id == this.TagType.YesNo) {//Bollean
                this.setState({
                    isTagDateForTeams: false,
                    isTagTextForTeams: false,
                    isTagBooleanForTeams: true
                });
                if (nameValidate) {
                    this.setState({
                        noMetadateValueForTeams: false,
                        noDateValueForTeams: false,
                        noNumberValueForTeams: false
                    });
                }
            } else {//Text || Number
                this.setState({
                    isTagDateForTeams: false,
                    isTagTextForTeams: true,
                    isTagBooleanForTeams: false
                });
                if (newvalue.id == this.TagType.Text) {//text
                    if ($.trim(this.state.metadataValueForTeams) != "" && nameValidate) {
                        this.setState({
                            noDateValueForTeams: false
                        });
                    }
                } else {//number
                    if (!isNaN(this.state.metadataValueForTeams) && nameValidate) {
                        this.setState({
                            noDateValueForTeams: false
                        });
                    }
                }
            }
        }
    }

    teamsArchiveActionCustomValidate = () => {
        let isValid = true;
        this.teamsInitValidStatus();
        if (this.state.isKeepForTeams) {
            if ((!this.state.isCSDTenant && !this.state.iskeepTagForTeams && !this.state.isDeclareForTeams) ||
                (this.state.isCSDTenant && !this.state.iskeepTagForTeams && !this.state.isDeclareForTeams && !this.state.isUndeclareForTeams)) {
                isValid = false;
                this.setState({
                    noSelectForTeams: true
                });
            }
        }

        if (this.state.isSORemoveForTeams && this.state.isKeepVersionOptionForTeams) {
            if (this.state.keepVersionValueForTeams === "" || this.state.keepVersionValueForTeams === null) {
                isValid = false;
                this.setState({
                    noKeepVersionValueForTeams: true
                });
            } else if (this.state.keepVersionValueForTeams < 0) {
                isValid = false;
                this.setState({
                    keepVersionValueInvalidForTeams: true
                });
            }
        }
        if (this.state.isBackupAndRemoveForTeams && this.state.isKeepVersionAndArchiveForTeams) {
            if (this.state.keepVersionAndArchiveValueForTeams === "" || this.state.keepVersionAndArchiveValueForTeams === null) {
                isValid = false;
                this.setState({
                    noKeepVersionAndArchiveValueForTeams: true
                });
            } else if (this.state.keepVersionAndArchiveValueForTeams < 0) {
                isValid = false;
                this.setState({
                    keepVersionAndArchiveValueInvalidForTeams: true
                });
            }
        }
        if (this.state.isBackupAndRemoveForTeams && this.state.isArchiveVersionOptionForTeams) {
            if (this.state.archiveVersionValueForTeams === "" || this.state.archiveVersionValueForTeams === null) {
                isValid = false;
                this.setState({
                    noArchiveVersionValueForTeams: true
                });
            } else if (this.state.archiveVersionValueForTeams < 0) {
                isValid = false;
                this.setState({
                    archiveVersionValueInvalidForTeams: true
                });
            }
        }
        if ((this.state.isBackupAndRemoveForTeams || this.state.isRemoveForTeams) && this.state.isLeaveStubOptionForTeams && Object.keys(this.state.selectedLevelStubSettingForTeams).length == 0) {
            isValid = false;
            this.setState({
                noLeaveStubValueForTeams: true
            });
        }
        if (this.state.iskeepTagForTeams) {
            let tagCheck = this.state.tagMetadataCheckedForTeams || this.state.isTagYesForTeams || this.state.isTagTimeForTeams || this.state.isTagByForTeams || this.state.retentionActionCheckedForTeams;
            if (!tagCheck) {
                isValid = false;
                this.setState({
                    noTagsForTeams: true
                });
            }
        }
        if (this.state.isKeepForTeams && this.state.iskeepTagForTeams) {
            if (this.state.tagMetadataCheckedForTeams) {
                let data = this.state.selectTagTypeForTeams;
                let TagType = this.TagType;
                if (data == null || $.trim(this.state.metadataNameForTeams) == "") {
                    this.setState({
                        noMetadateValueForTeams: true
                    });
                    isValid = false;
                } else {
                    this.setState({
                        noMetadateValueForTeams: false
                    });
                    // isValid = true;
                    switch (data.id) {
                        case TagType.Text:
                            if (!$.trim(this.state.metadataValueForTeams)) {
                                this.setState({
                                    noMetadateValueForTeams: true
                                });
                                isValid = false;
                            }
                            break;
                        case TagType.DateTime://datetime
                            if (!(this.state.currentDateForTeams)) {
                                this.setState({
                                    noDateValueForTeams: true
                                });
                                isValid = false;
                            }
                            break;
                        case TagType.Nubmer://number
                            if (!$.trim(this.state.metadataValueForTeams)) {
                                this.setState({
                                    noMetadateValueForTeams: true
                                });
                                isValid = false;
                            } else if (isNaN(this.state.metadataValueForTeams)) {
                                this.setState({
                                    noNumberValueForTeams: true
                                });
                                isValid = false;
                            } else {
                                this.setState({
                                    noNumberValueForTeams: false
                                });
                            }
                            break;
                        default:
                            this.setState({
                                noNumberValueForTeams: false,
                                noDateValueForTeams: false,
                                noMetadateValueForTeams: false
                            });
                    }
                }
            }
            if (this.state.retentionActionCheckedForTeams) {
                if ($.trim(this.state.retentionActionForTeams) == "") {
                    this.setState({
                        noRetentionActionValueForTeams: true
                    });
                    isValid = false;
                } else {
                    this.setState({
                        noRetentionActionValueForTeams: false
                    });
                    // isValid = true;
                }
            }
        }
        if (this.state.isMoveForTeams) {
            if (this.state.isSpecifyLocationForTeams) {
                if (!this.state.locationPathForTeams) {
                    isValid = false;
                    this.setState({
                        noLocationForTeams: true
                    });
                } else {
                    this.setState({
                        noLocationForTeams: false
                    });
                }

            } else {
                if (!this.teamsNodeItem) {
                    isValid = false;
                    this.setState({
                        noSelectNodeForTeams: true
                    });
                } else {
                    this.setState({
                        noSelectNodeForTeams: false
                    });
                }
            }
        }
        if(this.state.isArchiveToAzureBlobStorageForTeams){
            if (this.state.isLeaveStubOptionForTeams) {
                if (Object.keys(this.state.selectedLevelStubSettingForTeams).length == 0) {
                    isValid = false;
                    this.setState({
                        noLeaveStubValueForTeams: true
                    });
                }
            }
        }
        if (this.state.selectedRuleModuleType != RuleModuleTypes.SOArchiver && this.removeAchivedRefForTeams && this.removeAchivedRefForTeams.getRemoveArchived()) {
            if (!this.removeAchivedRefForTeams.getRemoveArchColIsValid()) {
                isValid = false;
            }
        } else {
            if (this.removeAchivedRefForTeams && !this.removeAchivedRefForTeams.getRemoveArchColIsValid()) {
                isValid = false;
            }
        }
        return isValid;
    }

    teamsMetadataNameChange = (value) => {
        this.setState({
            metadataNameForTeams: value
        });
    }

    //Custom Metadata:  value change
    teamsMetadataValueChange = (value) => {
        this.setState({
            metadataValueForTeams: value
        });
    }

    teamsOnCurrentStoragePolicyChange = (args) => {
        this.setState({
            selectTagBooleanForTeams: args.newValue
        });
    }

    teamsMetadataDateSelecteChange = (args) => {
        this.setState({
            currentDateForTeams: args.newValue
        }, () => {
            this.teamsArchiveActionCustomValidate();
        });
    }

    onRetentionActionCheckChangeForTeams = () => {
        this.setState({
            retentionActionCheckedForTeams: !this.state.retentionActionCheckedForTeams
        }, () => {
            if (!this.state.retentionActionCheckedForTeams) {
                this.setState({
                    noRetentionActionValueForTeams: false
                });
            }
        });
    }

    teamsExportOnlyCheckedChange = () => {
        this.setState({
            isRemoveForTeams: false,
            isKeepForTeams: false,
            isMoveForTeams: false,
            isExportOnlyForTeams: true,
            isArchiveToAzureBlobStorageForTeams: false,
            isBackupAndRemoveForTeams: false,
            isLeaveStubOptionForTeams: false,
            isSORemoveForTeams: false,
            isKeepVersionOptionForTeams: false,
        });
        this.dispatch("teamsApproval", Constants.dispatchAction.approvalCheckboxDisabledAndChecked, true, false);
        this.disableExportOption(Constants.RuleSourceTabIndex.Teams);
    }

    //选择日期
    metadataDateSelecteChange (args) {
        this.setState({
            currentDate: args.newValue
        }, () => {
            this.archiveActionCustomValidate();
        });
    }

    getRetentionRecordsLabelOptions = (selectedOption) => {
        return [
            {
                text: (
                    <$g.I18NProvider
                        msg={RMResx.RM_RDM_CreateRule_Options_LabelGetFromSetting}
                        style={{ whiteSpace: "break-spaces" }}
                    >
                        <span>
                            <a
                                className="ra-link-a"
                                href="/Root/CP/GeneralSetting"
                            >
                                {RMResx.RM_JS_SP_MigrateDeclaredRecords_GeneralSetting}
                            </a>
                            <span tabIndex={0}>{`: ${this.state.recordsLabelValue}`}</span>
                        </span>
                    </$g.I18NProvider>
                ),
                value: Constants.RetentionLabelOptions.GetFromGeneralSetting,
                checked: selectedOption === Constants.RetentionLabelOptions.GetFromGeneralSetting,
            },
            {
                text: RMResx.RM_RDM_CreateRule_Options_LabelDefault,
                value: Constants.RetentionLabelOptions.Default,
                checked: selectedOption === Constants.RetentionLabelOptions.Default,
            },
        ]
    }

    onRetentionRecordsLabelOptionsChange = (newValue) => {
        this.setState({
            retentionRecordsLabelSelected: newValue,
            retentionRecordsLabelOptions: this.getRetentionRecordsLabelOptions(newValue),
            noRetentionActionValue: false,
        });
    }

    onRetentionRecordsLabelOptionsChangeForOD = (newValue) => {
        this.setState({
            retentionRecordsLabelSelectedForOneDrive: newValue,
            retentionRecordsLabelOptionsForOneDrive: this.getRetentionRecordsLabelOptions(newValue),
            noRetentionActionValueForOneDrive: false,
        });
    }

    onRetentionActionCheckChange () {
        this.setState({
            retentionActionChecked: !this.state.retentionActionChecked
        }, () => {
            if (this.enableRecordsArchiver) {
                if (!this.state.retentionActionChecked || (this.state.retentionActionChecked && this.state.retentionRecordsLabelSelected === Constants.RetentionLabelOptions.GetFromGeneralSetting)) {
                    this.setState({
                        noRetentionActionValue: false
                    });
                }
    
                if (this.state.retentionActionChecked) {
                    this.onRetentionRecordsLabelOptionsChange(Constants.RetentionLabelOptions.GetFromGeneralSetting);
                }
            } else {
                if (!this.state.retentionActionChecked) {
                    this.setState({
                        noRetentionActionValue: false
                    });
                }
            }
        });
    }

    //save验证
    archiveActionCustomValidate () {
        let isValid = true;
        this.initValidStatus();
        //验证字符不能超过255
        // if (!this.DisposalValidateInput(true) || !this.DisposalValidateInput(false)) {
        //     isValid = false;
        // }
        if (this.state.isKeep) {
            if ((!this.state.isCSDTenant && !this.state.iskeepTag && !this.state.isDeclare) ||
                (this.state.isCSDTenant && !this.state.iskeepTag && !this.state.isDeclare && !this.state.isUndeclare)) {
                isValid = false;
                this.setState({
                    noSelect: true
                });
            }
        }

        if (this.state.isSORemoveForSPO && this.state.isKeepVersionOption) {
            if (this.state.keepVersionValue === "" || this.state.keepVersionValue === null) {
                isValid = false;
                this.setState({
                    noKeepVersionValueForSPO: true
                });
            } else if (this.state.keepVersionValue < 0) {
                isValid = false;
                this.setState({
                    keepVersionValueInvalidForSPO: true
                });
            }
        }
        if (this.state.isBackupAndRemoveForSPO && this.state.isKeepVersionAndArchiveForSPO) {
            if (this.state.keepVersionAndArchiveValueForSPO === "" || this.state.keepVersionAndArchiveValueForSPO === null) {
                isValid = false;
                this.setState({
                    noKeepVersionAndArchiveValueForSPO: true
                });
            } else if (this.state.keepVersionAndArchiveValueForSPO < 0) {
                isValid = false;
                this.setState({
                    keepVersionAndArchiveValueInvalidForSPO: true
                });
            }
        }
        if (this.state.isBackupAndRemoveForSPO && this.state.isArchiveVersionOption) {
            if (this.state.archiveVersionValue === "" || this.state.archiveVersionValue === null) {
                isValid = false;
                this.setState({
                    noArchiveVersionValueForSPO: true
                });
            } else if (this.state.archiveVersionValue < 0) {
                isValid = false;
                this.setState({
                    archiveVersionValueInvalidForSPO: true
                });
            }
        }
        if ((this.state.isBackupAndRemoveForSPO || this.state.isRemove) && this.state.isLeaveStubOption && Object.keys(this.state.selectedLevelStubSettingForSPO).length == 0) {
            isValid = false;
            this.setState({
                noLeaveStubValueForSPO: true
            });
        }
        if (this.state.iskeepTag) {
            let tagCheck = this.state.tagMetadataChecked || this.state.isTagYes || this.state.isTagTime || this.state.isTagBy || this.state.retentionActionChecked;
            if (!tagCheck) {
                isValid = false;
                this.setState({
                    noTags: true
                });
            }
        }
        if (this.state.isKeep && this.state.iskeepTag) {
            if (this.state.tagMetadataChecked) {
                let data = this.state.slectTagType;
                let TagType = this.TagType;
                if (data == null || $.trim(this.state.metadataName) == "") {
                    this.setState({
                        noMetadateValue: true
                    });
                    isValid = false;
                } else {
                    this.setState({
                        noMetadateValue: false
                    });
                    // isValid = true;
                    switch (data.id) {
                        case TagType.Text:
                            if (!$.trim(this.state.metadataValue)) {
                                this.setState({
                                    noMetadateValue: true
                                });
                                isValid = false;
                            }
                            break;
                        case TagType.DateTime://datetime
                            if (!(this.state.currentDate)) {
                                this.setState({
                                    noDateValue: true
                                });
                                isValid = false;
                            }
                            break;
                        case TagType.Nubmer://number
                            if (!$.trim(this.state.metadataValue)) {
                                this.setState({
                                    noMetadateValue: true
                                });
                                isValid = false;
                            } else if (isNaN(this.state.metadataValue)) {
                                this.setState({
                                    noNumberValue: true
                                });
                                isValid = false;
                            } else {
                                this.setState({
                                    noNumberValue: false
                                });
                            }
                            break;
                        default:
                            this.setState({
                                noNumberValue: false,
                                noDateValue: false,
                                noMetadateValue: false
                            });
                    }
                }
            }
            if (this.state.retentionActionChecked) {
                let isEmptyRetentionAction = $.trim(this.state.retentionAction) == "";
                if (this.enableRecordsArchiver) {
                    isEmptyRetentionAction = this.state.retentionRecordsLabelSelected === Constants.RetentionLabelOptions.Default && $.trim(this.state.retentionAction) == "";   
                }
                if (isEmptyRetentionAction) {
                    this.setState({
                        noRetentionActionValue: true
                    });
                    isValid = false;
                } else {
                    this.setState({
                        noRetentionActionValue: false
                    });
                    // isValid = true;
                }
            }
        }
        if (this.state.isMove) {
            if (this.state.isSpecifyLocation) {
                if (!this.state.locationPath) {
                    isValid = false;
                    this.setState({
                        noLocation: true
                    });
                } else {
                    this.setState({
                        noLocation: false
                    });
                }

            } else {
                // const supportTeamsTree = LicenseHelper.HasUpgradeTeams() && checkPermission("Source_Teams", RM.UserResources) && [64, 16].includes(this.levelId);
                const hasNodeItem = this.state.destinationActiveTab ? this.teamsNodeItem : this.spNodeItem;
                isValid = hasNodeItem;
                this.setState({
                    noSelectNode: !hasNodeItem,
                });
            }
        }
        if(this.state.isArchiveToAzureBlobStorage){
            if (this.state.isLeaveStubOption) {
                if (Object.keys(this.state.selectedLevelStubSettingForSPO).length == 0) {
                    isValid = false;
                    this.setState({
                        noLeaveStubValueForSPO: true
                    });
                }
            }
        }
        if (this.state.selectedRuleModuleType != RuleModuleTypes.SOArchiver && this.removeAchivedRefForSp && this.removeAchivedRefForSp.getRemoveArchived()) {
            if (!this.removeAchivedRefForSp.getRemoveArchColIsValid()) {
                isValid = false;
            }
        } else {
            if (this.removeAchivedRefForSp && !this.removeAchivedRefForSp.getRemoveArchColIsValid()) {
                isValid = false;
            }
        }
        if (this.state.isArchiveWithoutDestroy && this.state.isArchiveVersionOption) {
            if (this.state.archiveVersionValue === "" || this.state.archiveVersionValue === null) {
                isValid = false;
                this.setState({
                    noArchiveVersionValueForSPO: true
                });
            } else if (this.state.archiveVersionValue < 0) {
                isValid = false;
                this.setState({
                    archiveVersionValueInvalidForSPO: true
                });
            }
        }
        return isValid;
    }

    spLocalArchiveActionCustomValidate () {
        let isValid = true;
        this.initValidStatusForLocal();
        //验证字符不能超过255
        // if (!this.DisposalValidateInput(true) || !this.DisposalValidateInput(false)) {
        //     isValid = false;
        // }
        if (this.state.isKeepForLocal) {
            if ((!this.state.isCSDTenant && !this.state.iskeepTagForLocal && !this.state.isDeclareForLocal) ||
                (this.state.isCSDTenant && !this.state.iskeepTagForLocal && !this.state.isDeclareForLocal && !this.state.isUndeclareForLocal)) {
                isValid = false;
                this.setState({
                    noSelectForLocal: true
                });
            }
        }

        if (this.state.iskeepTagForLocal) {
            let tagCheck = this.state.tagMetadataCheckedForLocal || this.state.isTagYesForLocal || this.state.isTagTimeForLocal || this.state.isTagByForLocal || this.state.retentionActionCheckedForLocal;
            if (!tagCheck) {
                isValid = false;
                this.setState({
                    noTagsForLocal: true
                });
            }
        }
        if (this.state.isKeepForLocal && this.state.iskeepTagForLocal) {
            if (this.state.tagMetadataCheckedForLocal) {
                let data = this.state.slectTagTypeForLocal;
                let TagType = this.TagType;
                if (data == null || $.trim(this.state.metadataNameForLocal) == "") {
                    this.setState({
                        noMetadateValueForLocal: true
                    });
                    isValid = false;
                } else {
                    this.setState({
                        noMetadateValueForLocal: false
                    });
                    // isValid = true;
                    switch (data.id) {
                        case TagType.Text:
                            if (!$.trim(this.state.metadataValueForLocal)) {
                                this.setState({
                                    noMetadateValueForLocal: true
                                });
                                isValid = false;
                            }
                            break;
                        case TagType.DateTime://datetime
                            if (!(this.state.currentDateForLocal)) {
                                this.setState({
                                    noDateValueForLocal: true
                                });
                                isValid = false;
                            }
                            break;
                        case TagType.Nubmer://number
                            if (!$.trim(this.state.metadataValueForLocal)) {
                                this.setState({
                                    noMetadateValueForLocal: true
                                });
                                isValid = false;
                            } else if (isNaN(this.state.metadataValueForLocal)) {
                                this.setState({
                                    noNumberValueForLocal: true
                                });
                                isValid = false;
                            } else {
                                this.setState({
                                    noNumberValueForLocal: false
                                });
                            }
                            break;
                        default:
                            this.setState({
                                noNumberValueForLocal: false,
                                noDateValueForLocal: false,
                                noMetadateValueForLocal: false
                            });
                    }
                }
            }
            if (this.state.retentionActionCheckedForLocal) {
                if ($.trim(this.state.retentionActionForLocal) == "") {
                    this.setState({
                        noRetentionActionValueForLocal: true
                    });
                    isValid = false;
                } else {
                    this.setState({
                        noRetentionActionValueForLocal: false
                    });
                    // isValid = true;
                }
            }
        }
        if (this.state.isMoveForLocal) {
            if (this.state.isSpecifyLocationForLocal) {
                if (!this.state.locationPathForLocal) {
                    isValid = false;
                    this.setState({
                        noLocationForLocal: true
                    });
                } else {
                    this.setState({
                        noLocationForLocal: false
                    });
                }

            } else {
                if (!this.spLocalNodeItem) {
                    isValid = false;
                    this.setState({
                        noSelectNodeForLocal: true
                    });
                } else {
                    this.setState({
                        noSelectNodeForLocal: false
                    });
                }
            }
        }
        return isValid;
    }

    oneDriveArchiveActionCustomValidate () {
        let isValid = true;
        this.initValidStatusForOneDrive();
        //验证字符不能超过255
        // if (!this.DisposalValidateInput(true) || !this.DisposalValidateInput(false)) {
        //     isValid = false;
        // }

        if (this.state.isSORemoveForOD && this.state.isKeepVersionOptionForOD) {
            if (this.state.keepVersionValueForOD === "" || this.state.keepVersionValueForOD === null) {
                isValid = false;
                this.setState({
                    noKeepVersionValueForOD: true
                });
            } else if (this.state.keepVersionValueForOD < 0) {
                isValid = false;
                this.setState({
                    keepVersionValueInvalidForOD: true
                });
            }
        }
        if (this.state.isBackupAndRemoveForOD && this.state.isKeepVersionAndArchiveForOD) {
            if (this.state.keepVersionAndArchiveValueForOD === "" || this.state.keepVersionAndArchiveValueForOD === null) {
                isValid = false;
                this.setState({
                    noKeepVersionAndArchiveValueForOD: true
                });
            } else if (this.state.keepVersionAndArchiveValueForOD < 0) {
                isValid = false;
                this.setState({
                    keepVersionAndArchiveValueInvalidForOD: true
                });
            }
        }
        if (this.state.isBackupAndRemoveForOD && this.state.isArchiveVersionOptionForOD) {
            if (this.state.archiveVersionValueForOD === "" || this.state.archiveVersionValueForOD === null) {
                isValid = false;
                this.setState({
                    noArchiveVersionValueForOD: true
                });
            } else if (this.state.archiveVersionValueForOD < 0) {
                isValid = false;
                this.setState({
                    archiveVersionValueInvalidForOD: true
                });
            }
        }
        if ((this.state.isBackupAndRemoveForOD  || this.state.isRemoveForOneDrive) && this.state.isLeaveStubOptionForOneDrive && Object.keys(this.state.selectedLevelStubSettingForOneDrive).length == 0) {
            isValid = false;
            this.setState({
                noLeaveStubValueForOneDrive: true
            });
        }
        if (this.state.isKeepForOneDrive) {
            if ((!this.state.isCSDTenant && !this.state.iskeepTagForOneDrive && !this.state.isDeclareForOneDrive) ||
                (this.state.isCSDTenant && !this.state.iskeepTagForOneDrive && !this.state.isDeclareForOneDrive && !this.state.isUndeclareForOneDrive)) {
                isValid = false;
                this.setState({
                    noSelectForOneDrive: true
                });
            }
        }

        if (this.state.iskeepTagForOneDrive) {
            let tagCheck = this.state.tagMetadataCheckedForOneDrive || this.state.isTagYesForOneDrive || this.state.isTagTimeForOneDrive || this.state.isTagByForOneDrive || this.state.retentionActionCheckedForOneDrive;
            if (!tagCheck) {
                isValid = false;
                this.setState({
                    noTagsForOneDrive: true
                });
            }
        }
        if (this.state.isKeepForOneDrive && this.state.iskeepTagForOneDrive) {
            if (this.state.tagMetadataCheckedForOneDrive) {
                let data = this.state.slectTagTypeForOneDrive;
                let TagType = this.TagType;
                if (data == null || $.trim(this.state.metadataNameForOneDrive) == "") {
                    this.setState({
                        noMetadateValueForOneDrive: true
                    });
                    isValid = false;
                } else {
                    this.setState({
                        noMetadateValueForOneDrive: false
                    });
                    // isValid = true;
                    switch (data.id) {
                        case TagType.Text:
                            if (!$.trim(this.state.metadataValueForOneDrive)) {
                                this.setState({
                                    noMetadateValueForOneDrive: true
                                });
                                isValid = false;
                            }
                            break;
                        case TagType.DateTime://datetime
                            if (!(this.state.currentDateForOneDrive)) {
                                this.setState({
                                    noDateValueForOneDrive: true
                                });
                                isValid = false;
                            }
                            break;
                        case TagType.Nubmer://number
                            if (!$.trim(this.state.metadataValueForOneDrive)) {
                                this.setState({
                                    noMetadateValueForOneDrive: true
                                });
                                isValid = false;
                            } else if (isNaN(this.state.metadataValueForOneDrive)) {
                                this.setState({
                                    noNumberValueForOneDrive: true
                                });
                                isValid = false;
                            } else {
                                this.setState({
                                    noNumberValueForOneDrive: false
                                });
                            }
                            break;
                        default:
                            this.setState({
                                noNumberValueForOneDrive: false,
                                noDateValueForOneDrive: false,
                                noMetadateValueForOneDrive: false
                            });
                    }
                }
            }
            if (this.state.retentionActionCheckedForOneDrive) {
                let isEmptyRetentionActionForOneDrive = $.trim(this.state.retentionActionForOneDrive) == "";
                if (this.enableRecordsArchiver) {
                    isEmptyRetentionActionForOneDrive = this.state.retentionRecordsLabelSelectedForOneDrive === Constants.RetentionLabelOptions.Default && $.trim(this.state.retentionActionForOneDrive) == "";   
                }
                if (isEmptyRetentionActionForOneDrive) {
                    this.setState({
                        noRetentionActionValueForOneDrive: true
                    });
                    isValid = false;
                } else {
                    this.setState({
                        noRetentionActionValueForOneDrive: false
                    });
                    // isValid = true;
                }
            }
        }
        if (this.state.isMoveForOneDrive) {
            if (this.state.isSpecifyLocationForOneDrive) {
                if (!this.state.locationPathForOneDrive) {
                    isValid = false;
                    this.setState({
                        noLocationForOneDrive: true
                    });
                } else {
                    this.setState({
                        noLocationForOneDrive: false
                    });
                }

            } else {
                const hasNodeItem = this.state.destinationActiveTabForOD ? this.teamsNodeItemForOD : this.oneDriveNodeItem;
                isValid = hasNodeItem;
                this.setState({
                    noSelectNodeForOneDrive: !hasNodeItem,
                });
            }
        }
        if(this.state.isArchiveToAzureBlobStorageForOneDrive){
            if (this.state.isLeaveStubOptionForOneDrive) {
                if (Object.keys(this.state.selectedLevelStubSettingForOneDrive).length == 0) {
                    isValid = false;
                    this.setState({
                        noLeaveStubValueForOneDrive: true
                    });
                }
            }
        }
        if (this.state.selectedRuleModuleType != RuleModuleTypes.SOArchiver && this.removeAchivedRefForOd && this.removeAchivedRefForOd.getRemoveArchived()) {
            if (!this.removeAchivedRefForOd.getRemoveArchColIsValid()) {
                isValid = false;
            }
        } else {
            if (this.removeAchivedRefForOd && !this.removeAchivedRefForOd.getRemoveArchColIsValid()) {
                isValid = false;
            }
        }
        if (this.state.isArchiveWithoutDestroyForOneDrive && this.state.isArchiveVersionOptionForOD) {
            if (this.state.archiveVersionValueForOD === "" || this.state.archiveVersionValueForOD === null) {
                isValid = false;
                this.setState({
                    noArchiveVersionValueForOD: true
                });
            } else if (this.state.archiveVersionValueForOD < 0) {
                isValid = false;
                this.setState({
                    archiveVersionValueInvalidForOD: true
                });
            }
        }
        return isValid;
    }

    exoArchiveActionCustomValidate () {
        let isValid = true;
        if (this.state.isExoKeep) {
            if ($.trim(this.state.exoRetentionAction) == "") {
                this.setState({
                    noExoRetentionActionValue: true
                });
                isValid = false;
            } else {
                this.setState({
                    noExoRetentionActionValue: false
                });
            }
        }
        if (this.state.isExoMove) {
            if (this.state.isExoSpecifyLocation) {
                if (!this.state.exoLocationPath) {
                    isValid = false;
                    this.setState({
                        noExoLocation: true
                    });
                } else {
                    this.setState({
                        noExoLocation: false
                    });
                }
            } else {
                const hasNodeItem = this.state.destinationActiveTabForEXO ? this.teamsNodeItemForEXO : this.exoNodeItem;
                isValid = hasNodeItem;
                this.setState({
                    noExoSelectNode: !hasNodeItem,
                });
            }

            if (this.state.isExoMoveToSP && !this.getExoMoveToSPIsValid(this.exoMoveToSPRef)) {
                isValid = false;
            }
        }
        return isValid;
    }

    phyArchiveActionCustomValidate () {
        let isValid = true;
        if (this.state.isPhyMove) {
            if (!this.selectedPhyTreeItem) {
                isValid = false;
                this.setState({
                    noSelectPhyNode: true
                });
            } else {
                this.setState({
                    noSelectPhyNode: false
                });
            }
        }
        if (this.state.selectedRuleModuleType != RuleModuleTypes.SOArchiver && this.removeAchivedRefForPhy && this.removeAchivedRefForPhy.getRemoveArchived()) {
            if (!this.removeAchivedRefForPhy.getRemoveArchColIsValid()) {
                isValid = false;
            }
        } else {
            if (this.removeAchivedRefForPhy && !this.removeAchivedRefForPhy.getRemoveArchColIsValid()) {
                isValid = false;
            }
        }
        return isValid;
    }

    fsArchiveActionCustomValidate () {
        let isValid = true;
        if (this.state.isPhyMove) {
            if (!this.selectedPhyTreeItem) {
                isValid = false;
                this.setState({
                    noSelectPhyNode: true
                });
            } else {
                this.setState({
                    noSelectPhyNode: false
                });
            }
        }
        return isValid;
    }

    googleArchiveActionCustomValidate = () => {
        let isValid = true;
        if (this.state.isGoogleMove) {
            if (!this.googleNodeItem) {
                isValid = false;
                this.setState({
                    noSelectNodeForGoogle: true
                });
            } else {
                this.setState({
                    noSelectNodeForGoogle: false
                });
            }
        }
        if (this.state.selectedRuleModuleType != RuleModuleTypes.SOArchiver && this.removeAchivedRefForGoogle && this.removeAchivedRefForGoogle.getRemoveArchived()) {
            if (!this.removeAchivedRefForGoogle.getRemoveArchColIsValid()) {
                isValid = false;
            }
        }
        return isValid;
    }

    onDestTreeSelectedChanged (nodeItem) {
        this.spNodeItem = nodeItem;
        this.setState({
            noSelectNode: false
        });
    }
    onDestTreeSelectedChangedForLocal (nodeItem) {
        this.spLocalNodeItem = nodeItem;
        this.setState({
            noSelectNodeForLocal: false
        });
    }

    onDestTreeSelectedChangedForOneDrive (nodeItem) {
        this.oneDriveNodeItem = nodeItem;
        this.setState({
            noSelectNodeForOneDrive: false
        });
    }

    onDestTreeSelectedChangedForTeams = (nodeItem) => {
        this.teamsNodeItem = nodeItem;
        this.setState({
            noSelectNodeForTeams: false
        });
    }

    onDestTreeSelectedChangedForTeamsOD = (nodeItem) => {
        this.teamsNodeItemForOD = nodeItem;
        this.setState({
            noSelectNodeForTeams: false
        });
    }

    onDestTreeSelectedChangedForTeamsEXO = (nodeItem) => {
        this.teamsNodeItemForEXO = nodeItem;
        this.setState({
            noSelectNodeForTeams: false
        });
    }

    onDestExoTreeSelectedChanged(nodeItem) {
        this.exoNodeItem = nodeItem;
        this.setState({
            noExoSelectNode: false
        });
    }

    onDestTreeSelectedChangedForGoogle = (nodeItem) => {
        this.googleNodeItem = nodeItem;
        this.setState({
            noSelectNodeForGoogle: false
        });
    }

    //Custom Metadata:  name change
    metadataNameChange (value) {
        this.setState({
            metadataName: value
        });
    }

    //Custom Metadata:  value change
    metadataValueChange (value) {
        this.setState({
            metadataValue: value
        });
    }

    onCurrentStoragePolicyChange (args) {
        this.setState({
            selectTagBoolean: args.newValue
        });
    }

    retentionActionChange (value) {
        this.setState({
            retentionAction: value
        });
    }

    retentionActionChangeForTeams = (value) => {
        this.setState({
            retentionActionForTeams: value
        });
    }

    //Custom Metadata:  name change
    metadataNameChangeForLocal (value) {
        this.setState({
            metadataNameForLocal: value
        });
    }

    //Custom Metadata:  value change
    metadataValueChangeForLocal (value) {
        this.setState({
            metadataValueForLocal: value
        });
    }

    onCurrentStoragePolicyChangeForLocal (args) {
        this.setState({
            selectTagBooleanForLocal: args.newValue
        });
    }

    // retentionActionChangeForLocal (e) {
    //     this.setState({
    //         retentionActionForLocal: e.target.value
    //     });
    // }

    locationTypeClickForLocal (isSpecifyLocation) {
        this.setState({
            isSpecifyLocationForLocal: isSpecifyLocation,
            noLocationForLocal: false,
            isLocationVlidatForLocal: false,
        });
    }
    locationPathChangeForLocal (e) {
        this.setState({
            locationPathForLocal: e.target.value,
            isLocationVlidatForLocal: false
        });
    }
    //test 按钮
    checkLocationForLocal () {
        let locationPath = this.state.locationPathForLocal;
        if (locationPath == "") {
            this.setState({
                noLocationForLocal: true
            });
            return;
        } else {
            this.setState({
                noLocationForLocal: false
            });
        }
        $$.loading(true);
        let urlData = "/api/RecordsExplorerApi/CheckSPLocation4Rule";
        let option = {
            url: urlData,
            method: "POST",
            data: {
                LocationPath: locationPath,
                SPAccount: null
            }
        };
        fetchUtility(option).then((res) => {
            $$.loading(false);
            if (res != "") {
                this.setState({
                    isLocationVlidatForLocal: true,
                    LocationVlidatForLocal: RMResx.RM_JS_CP_ES_SuccessToValidateDBSettings,
                    LocationVlidatypeForLocal: "success"
                });
            } else {
                this.setState({
                    isLocationVlidatForLocal: true,
                    LocationVlidatForLocal: RMResx.RM_JS_Rule_SPDestUrlError,
                    LocationVlidatypeForLocal: "error"
                });
            }
        }).catch((e) => {
            $$.loading(false);
        });
    }

    metadataNameChangeForOneDrive (value) {
        this.setState({
            metadataNameForOneDrive: value
        });
    }

    //Custom Metadata:  value change
    metadataValueChangeForOneDrive (value) {
        this.setState({
            metadataValueForOneDrive: value
        });
    }

    onCurrentStoragePolicyChangeForOneDrive (args) {
        this.setState({
            selectTagBooleanForOneDrive: args.newValue
        });
    }

    retentionActionChangeForOneDrive (value) {
        this.setState({
            retentionActionForOneDrive: value
        });
    }

    locationTypeClickForOneDrive (isSpecifyLocation) {
        this.setState({
            isSpecifyLocationForOneDrive: isSpecifyLocation,
            noLocationForOneDrive: false,
            isLocationVlidatForOneDrive: false,
        });
    }
    locationPathChangeForOneDrive (value) {
        this.setState({
            locationPathForOneDrive: value,
            isLocationVlidatForOneDrive: false
        });
    }
    //test 按钮
    checkLocationForOneDrive () {
        let locationPath = this.state.locationPathForOneDrive;
        if (locationPath == "") {
            this.setState({
                noLocationForOneDrive: true
            });
            return;
        } else {
            this.setState({
                noLocationForOneDrive: false
            });
        }
        $$.loading(true);
        let urlData = "/api/RecordsExplorerApi/CheckSPLocation4Rule";
        let option = {
            url: urlData,
            method: "POST",
            data: {
                LocationPath: locationPath,
                SPAccount: null
            }
        };
        fetchUtility(option).then((res) => {
            $$.loading(false);
            if (res != "") {
                this.setState({
                    isLocationVlidatForOneDrive: true,
                    LocationVlidatForOneDrive: RMResx.RM_JS_CP_ES_SuccessToValidateDBSettings,
                    LocationVlidatypeForOneDrive: "success"
                });
            } else {
                this.setState({
                    isLocationVlidatForOneDrive: true,
                    LocationVlidatForOneDrive: RMResx.RM_JS_Rule_SPDestUrlError,
                    LocationVlidatypeForOneDrive: "error"
                });
            }
        }).catch((e) => {
            $$.loading(false);
        });
    }

    // onCurrentStoragePolicyChangeForOneDrive (args) {
    //     this.setState({
    //         selectTagBooleanForOneDrive: args.newValue
    //     });
    // }
    //onedrive

    exoRetentionActionChange (value) {
        this.setState({
            exoRetentionAction: value
        });
    }

    //清空Record Declaration and Tagging的数据
    ArchiveActionClearData () {
        this.setState({
            isMoveShow: true,
            isStoreInM365ArchiveShow: true,
            isKeepShow: true,
            isRemove: true,
            isKeep: false,
            isMove: false,
            isStoreInM365Archive: false,
            isArchiveWithoutDestroy: false,
            isArchiveToAzureBlobStorage: false,
            isDeclare: false,
            iskeepTag: false,
            isTagYes: false,
            isTagBy: false,
            isTagTime: false,
            metadataName: "",
            metadataValue: "",
            tagMetadataChecked: false,
            isMoveDeclare: false,
            isTagText: true,
            tagTypeValue: 0,
            currentDate: "",
            isTagDate: false,
            isTagBoolean: false,
            isLeaveStubOption: false,
            isDeclaredFile: false,
            isDeleteToRecycleBinForSPO: false,
            isDeleteRelatedRecordOption: false,
            isBackupOption: false,
            userOptions: [],
            isSendEmail: false,
            selectTagBoolean: this.TrueOrFaseOptions[0],
            isBackupAndRemoveForSPO: false,
        });
    }

    locationTypeClick (isSpecifyLocation) {
        this.setState({
            isSpecifyLocation: isSpecifyLocation,
            noLocation: false,
            isLocationVlidat: false,
        });
    }

    teamsLocationTypeClick(isSpecifyLocation) {
        this.setState({
            isSpecifyLocationForTeams: isSpecifyLocation,
            noLocationForTeams: false,
            isLocationValidateForTeams: false,
        });
    }

    exoLocationTypeClick(isExoSpecifyLocation) {
        this.setState({
            isExoSpecifyLocation: isExoSpecifyLocation,
            noExoLocation: false,
            isExoLocationVlidat: false,
        });
    }
    //Enter a destination input change
    locationPathChange (value) {
        this.setState({
            locationPath: value,
            isLocationVlidat: false
        });
    }

    onDestActiveTabChange = (index) => {
        this.setState({ destinationActiveTab: index });
    }

    onDestActiveTabChangeForOD = (index) => {
        this.setState({ destinationActiveTabForOD: index });
    }

    onDestActiveTabChangeForEXO = (index) => {
        this.setState({ destinationActiveTabForEXO: index });
    }

    teamsLocationPathChange = (value) => {
        this.setState({
            locationPathForTeams: value,
            isLocationValidateForTeams: false
        });
    }

    //Enter a destination input change Exo
    exoLocationPathChange(value) {
        this.setState({
            exoLocationPath: value,
            isExoLocationVlidat: false
        });
    }

    //test 按钮
    checkLocation () {
        let locationPath = this.state.locationPath;
        if (locationPath == "") {
            this.setState({
                noLocation: true
            });
            return;
        } else {
            this.setState({
                noLocation: false
            });
        }
        $$.loading(true);
        let urlData = "/api/RecordsExplorerApi/CheckSPLocation4Rule";
        let option = {
            url: urlData,
            method: "POST",
            data: {
                LocationPath: locationPath,
                SPAccount: null
            }
        };
        fetchUtility(option).then((res) => {
            $$.loading(false);
            if (res != "") {
                this.setState({
                    isLocationVlidat: true,
                    LocationVlidat: RMResx.RM_JS_CP_ES_SuccessToValidateDBSettings,
                    LocationVlidatype: "success"
                });
            } else {
                this.setState({
                    isLocationVlidat: true,
                    LocationVlidat: RMResx.RM_JS_Rule_SPDestUrlError,
                    LocationVlidatype: "error"
                });
            }
        }).catch((e) => {
            $$.loading(false);
        });
    }

    checkLocationForTeams = () => {
        const locationPath = this.state.locationPathForTeams;
        if (locationPath == "") {
            this.setState({
                noLocationForTeams: true
            });
            return;
        } else {
            this.setState({
                noLocationForTeams: false
            });
        }
        $$.loading(true);
        let urlData = "/api/RecordsExplorerApi/CheckTeamsLocation4Rule"; // Update later
        let option = {
            url: urlData,
            method: "POST",
            data: {
                LocationPath: locationPath,
                SPAccount: null
            }
        };
        fetchUtility(option).then((res) => {
            $$.loading(false);
            if (res != "") {
                this.setState({
                    isLocationValidateForTeams: true,
                    locationValidateMsgForTeams: RMResx.RM_JS_CP_ES_SuccessToValidateDBSettings,
                    locationValidateTypeForTeams: "success"
                });
            } else {
                this.setState({
                    isLocationValidateForTeams: true,
                    locationValidateMsgForTeams: RMResx.RM_JS_Rule_SPDestUrlError,
                    locationValidateTypeForTeams: "error"
                });
            }
        }).catch((e) => {
            $$.loading(false);
        });
    }

    //exo test 按钮
    checkExoLocation() {
        let locationPath = this.state.exoLocationPath;
        if (locationPath == "") {
            this.setState({
                noExoLocation: true
            });
            return;
        } else {
            this.setState({
                noExoLocation: false
            });
        }
        $$.loading(true);
        let urlData = "/api/RecordsExplorerApi/CheckSPLocation4Rule";
        let option = {
            url: urlData,
            method: "POST",
            data: {
                LocationPath: locationPath,
                SPAccount: null
            }
        };
        fetchUtility(option).then((res) => {
            $$.loading(false);
            if (res != "") {
                this.setState({
                    isExoLocationVlidat: true,
                    ExoLocationVlidat: RMResx.RM_JS_CP_ES_SuccessToValidateDBSettings,
                    ExoLocationVlidatype: "success"
                });
            } else {
                this.setState({
                    isExoLocationVlidat: true,
                    ExoLocationVlidat: RMResx.RM_JS_Rule_SPDestUrlError,
                    ExoLocationVlidatype: "error"
                });
            }
        }).catch((e) => {
            $$.loading(false);
        });
    }


    closeMessageBox () {
        this.setState({
            MessageTipInfo: { showTip: false }
        });
    }

    cancelLocationVlidat () {
        this.setState({
            isLocationVlidat: false
        });
    }

    cancelLocationValidateForTeams = () => {
        this.setState({
            isLocationValidateForTeams: false
        });
    }

    cancelLocationVlidatForLocal () {
        this.setState({
            isLocationVlidatForLocal: false
        });
    }

    cancelLocationVlidatForOneDrive () {
        this.setState({
            isLocationVlidatForOneDrive: false
        });
    }

    cancelExoLocationValidate() {
        this.setState({
            isExoLocationVlidat: false
        });
    }

    fileConflictOptionChange (value) {
        this.setState({
            currentConflictOptionValue: value
        });
    }

    fileConflictOptionChangeForTeams = (value) => {
        this.setState({
            currentConflictOptionValueForTeams: value
        });
    }

    fileConflictOptionChangeForLocal (value) {
        this.setState({
            currentConflictOptionValueForLocal: value
        });
    }

    fileConflictOptionChangeForOneDrive (value) {
        this.setState({
            currentConflictOptionValueForOneDrive: value
        });
    }

    exoFileConflictOptionChange(value) {
        this.setState({
            exo_currentConflictOptionValue: value
        });
    }

    phyFileConflictOptionChange (value) {
        this.setState({
            phy_currentConflictOptionValue: value
        });
    }

    moveHoldConflictOptionChange (value) {
        this.setState({
            currentMoveHoldConflictOptionValue: value
        });
    }

    getFailedMessage (result) {
        if(result && !result.Success && result.FailedType != Constants.ResultFailedType.None)
        {
            if(result.FailedType == Constants.ResultFailedType.NoGlobalStorageSetting)
            {
                return <$g.I18NProvider msg={RMResx.RM_JS_Common_ValidationSettingMsg}>
                            <a className="ra-link-a" href="/Root/cp/StorageSettings">{RMResx.RM_JS_CP_StorageSetting}</a>
                        </$g.I18NProvider>;
            }
            if(result.FailedType == Constants.ResultFailedType.NotConnDocAve)
            {
                return result.Message;
            }
        }
        return "";
    }

    cancelClick () {
        if (this.hasChanged) {
            $$.messagedialog(true, {
                // classify: "warn",
                width: "550px",
                hideActions: false,
                title: RMResx.RM_JS_Common_Confirmation,
                content: RMResx.RM_RC_DueDisposal_CancelPopup,
                buttons: [
                    {
                        text: RMResx.RM_JS_Common_OK,
                        primary: true,
                        classify: "theme",
                        onClick: this.onCancleOK
                    },
                    {
                        text: RMResx.RM_JS_Common_Cancel,
                        onClick: this.onCancleNo
                    }
                ]
            });
        } else {
            //this.props.onOperated(RuleOperatedType.Cancel);
            this.props.onOperated();
        }

    }

    onCancleOK () {
        //this.props.onOperated(RuleOperatedType.Cancel);
        this.props.onOperated();
        $$.messagedialog(false);
    }

    onCancleNo () {
        $$.messagedialog(false);
    }

    // Criteria获取组件返回值和验证
    getSpCriteriaData (data) {
        this.spCriteriaData = data;
    }

    getExoCriteriaData (data) {
        this.exoCriteriaData = data;
    }

    getPhyCriteriaData (data) {
        this.phyCriteriaData = data;
    }

    getFsCriteriaData (data) {
        this.fsCriteriaData = data;
    }

    getSpLocalCriteriaData (data) {
        this.spLocalCriteriaData = data;
    }

    getOneDriveCriteriaData (data) {
        this.oneDriveCriteriaData = data;
    }

    getAzureFileCriteriaData = (data) =>{
        this.azureFileCriteriaData = data;
    }

    getBoxCriteriaData = (data) => {
        this.boxCriteriaData = data;
    }

    getGoogleDriveCriteriaData = (data) => {
        this.googleDriveCriteriaData = data;
    }

    getTeamsCriteriaData = (data) => {
        this.teamsCriteriaData = data;
    }

    getConnectorCriteriaData = (data) => {
        this.connectorCriteriaData = data;
    }


    //获取验证（true or false）
    getSpIsVerificationPassed (isVerificationPassed) {
        this.spIsVerificationPassed = isVerificationPassed;
    }

    getSpLocalIsVerificationPassed (isVerificationPassed) {
        this.spLocalIsVerificationPassed = isVerificationPassed;
    }

    getOneDriveIsVerificationPassed (isVerificationPassed) {
        this.oneDriveIsVerificationPassed = isVerificationPassed;
    }

    getExoIsVerificationPassed (isVerificationPassed) {
        this.exoIsVerificationPassed = isVerificationPassed;
    }

    getPhyIsVerificationPassed = (isVerificationPassed) => {
        this.phyIsVerificationPassed = isVerificationPassed;
    }

    getFsIsVerificationPassed = (isVerificationPassed) => {
        this.fsIsVerificationPassed = isVerificationPassed;
    }

    getAzureFileIsVerificationPassed = (isVerificationPassed) =>{
        this.azureFileIsVerificationPassed = isVerificationPassed;
    }

    getBoxIsVerificationPassed = (isVerificationPassed) => {
        this.boxIsVerificationPassed = isVerificationPassed;
    }

    getGoogleIsVerificationPassed = (isVerificationPassed) => {
        this.googleDriveIsVerificationPassed = isVerificationPassed;
    }

    getTeamsIsVerificationPassed = (isVerificationPassed) => {
        this.teamsIsVerificationPassed = isVerificationPassed;
    }

    getConnectorIsVerificationPassed = (isVerificationPassed) =>{
        this.connectorIsVerificationPassed = isVerificationPassed;
    }

    //approval获取组件返回值和验证
    getSpApprovalData (data) {
        this.spApprovalData = data;
    }

    getSpLocalApprovalData (data) {
        this.spLocalApprovalData = data;
    }

    getOneDriveApprovalData (data) {
        this.oneDriveApprovalData = data;
    }

    getTeamsApprovalData = (data) => {
        this.teamsApprovalData = data;
    }

    getExoApprovalData (data) {
        this.exoApprovalData = data;
    }

    getPhyApprovalData = (data) => {
        this.phyApprovalData = data;
    }

    getFsApprovalData (data) {
        this.fsApprovalData = data;
    }

    getAzureFileApprovalData = (data) => {
        this.azureFileApprovalData = data;
    }

    getBoxApprovalData = (data) => {
        this.boxApprovalData = data;
    }

    getGoogleDriveApprovalData = (data) => {
        this.googleDriveApprovalData = data;
    }

    getConnectorApprovalData = (data) =>{
        this.connectorApprovalData = data;
    }

    //获取验证（true or false）
    getSpApprovalIsPassed (isVerificationPassed) {
        this.spApprovalIsPassed = isVerificationPassed;
    }

    getSpLocalApprovalIsPassed (isVerificationPassed) {
        this.spLocalApprovalIsPassed = isVerificationPassed;
    }

    getOneDriveApprovalIsPassed (isVerificationPassed) {
        this.oneDriveApprovalIsPassed = isVerificationPassed;
    }

    getExoApprovalIsPassed (isVerificationPassed) {
        this.exoApprovalIsPassed = isVerificationPassed;
    }

    getPhyApprovalIsPassed = (isVerificationPassed) => {
        this.phyApprovalIsPassed = isVerificationPassed;
    }

    getFsApprovalIsPassed (isVerificationPassed) {
        this.fsApprovalIsPassed = isVerificationPassed;
    }

    getAzureFileApprovalIsPassed = (isVerificationPassed) => {
        this.azureFileApprovalIsPassed = isVerificationPassed;
    }

    getBoxApprovalIsPassed = (isVerificationPassed) => {
        this.boxApprovalIsPassed = isVerificationPassed;
    }

    getGoogleDriveApprovalIsPassed = (isVerificationPassed) => {
        this.googleDriveApprovalIsPassed = isVerificationPassed;
    }

    getTeamsApprovalIsPassed = (isApprovalPassed) => {
        this.teamsApprovalIsPassed = isApprovalPassed;
    }

    getConnectorApprovalIsPassed = (isVerificationPassed) => {
        this.connectorApprovalIsPassed = isVerificationPassed;
    }

    //export获取组件返回值和验证
    getSpExportDate (data) {
        this.spExportData = data;
    }

    getSpLocalExportDate (data) {
        this.spLocalExportData = data;
    }

    getOneDriveExportDate (data) {
        this.oneDriveExportData = data;
    }

    getTeamsExportData = (data) => {
        this.teamsExportData = data;
    }

    getExoExportDate (data) {
        this.exoExportData = data;
    }

    getFsExportDate (data) {
        this.fsExportData = data;
    }

    getGoogleExportDate = (data) => {
        this.googleExportData = data;
    }

    //获取验证（true or false）
    getSpExportIsPassed (isVerificationPassed) {
        this.spExportIsPassed = isVerificationPassed;
    }

    getSpExportLocationIsPassed(isVerificationPassed) {
        this.spExportLocationIsPassed = isVerificationPassed;
    }

    getSpLocalExportIsPassed (isVerificationPassed) {
        this.spLocalExportIsPassed = isVerificationPassed;
    }

    getOneDriveExportIsPassed (isVerificationPassed) {
        this.oneDriveExportIsPassed = isVerificationPassed;
    }

    getOneDriveExportLocationIsPassed(isVerificationPassed) {
        this.oneDriveExportLocationIsPassed = isVerificationPassed;
    }

    getTeamsExportIsPassed = (isExportPassed) => {
        this.teamsExportIsPassed = isExportPassed;
    }

    getTeamsExportLocationIsPassed = (isExportLocationPassed) => {
        this.teamsExportLocationIsPassed = isExportLocationPassed;
    }

    getExoExportIsPassed (isVerificationPassed) {
        this.exoExportIsPassed = isVerificationPassed;
    }

    getExoExportLocationIsPassed(isVerificationPassed) {
        this.exoExportLocationIsPassed = isVerificationPassed;
    }

    getExoMoveToSPIsValid(ref) {
        return !ref || ref && ref.isValid();
    }

    getFsExportIsPassed (isVerificationPassed) {
        this.fsExportIsPassed = isVerificationPassed;
    }

    getStorageSettingIsValid(ref){
        return !ref || ref && ref.isValid();
    }

    getGoogleExportIsPassed = (isVerificationPassed) => {
        this.googleExportIsPassed = isVerificationPassed;
    }

    getGoogleExportLocationIsPassed = (isVerificationPassed) => {
        this.googleExportLocationIsPassed = isVerificationPassed;
    }

    manageRule () {
        let spSourceValidateResult = true;
        let spLocalSourceValidateResult = true;
        let oneDriveSourceValidateResult = true;
        let exoSourceValidateResult = true;
        let phySourceValidateResult = true;
        let fsSourceValidateResult = true;
        let azureFileSourceValidateResult = true;
        let boxSourceValidateResult = true;
        let googleDriveSourceValidateResult = true;
        let teamsSourceValidateResult = true;
        let connectorSourceValidateResult = true;
        if (this.isSpSourceChecked) {
            let actionResult = this.archiveActionCustomValidate();            
            if (!(this.spIsVerificationPassed && actionResult && this.spExportIsPassed && this.spExportLocationIsPassed &&
                this.spApprovalIsPassed && this.getStorageSettingIsValid(this.spStorageSettingsRef))) {
                spSourceValidateResult = false;
                this.setState({
                    ruleCriteriaTabsIndex: Constants.RuleSourceTabIndex.SP
                });
                return false;
            }
        }
        if (this.isOneDriveSourceChecked) {
            let actionResult = this.oneDriveArchiveActionCustomValidate();
            if (!(this.oneDriveIsVerificationPassed && actionResult && this.oneDriveApprovalIsPassed && this.oneDriveExportIsPassed && 
                this.oneDriveExportLocationIsPassed && this.getStorageSettingIsValid(this.oneDriveStorageSettingsRef))){
                oneDriveSourceValidateResult = false;
                this.setState({
                    ruleCriteriaTabsIndex: Constants.RuleSourceTabIndex.OneDrive
                });
                return false;
            }
        }
        if (this.isExoSourceChecked) {
            let actionResult = this.exoArchiveActionCustomValidate();
            if (!(this.exoIsVerificationPassed && actionResult && this.exoExportIsPassed && this.exoExportLocationIsPassed && this.exoApprovalIsPassed)) {
                exoSourceValidateResult = false;
                this.setState({
                    ruleCriteriaTabsIndex: Constants.RuleSourceTabIndex.Exchange
                });
                return false;
            }
        }
        if (this.isPhySourceChecked) {
            let actionResult = this.phyArchiveActionCustomValidate();
            if (!(this.phyIsVerificationPassed && actionResult && this.phyApprovalIsPassed)) {
                phySourceValidateResult = false;
                this.setState({
                    ruleCriteriaTabsIndex: Constants.RuleSourceTabIndex.Physical
                });
                return false;
            }
        }
        if (this.isFsSourceChecked) {
            if (!(this.fsIsVerificationPassed && this.ruleMoveOfFsValid && this.fsApprovalIsPassed &&
                this.getStorageSettingIsValid(this.fsStorageSettingsRef))) {
                fsSourceValidateResult = false;
                this.setState({
                    ruleCriteriaTabsIndex: Constants.RuleSourceTabIndex.FS
                });
                return false;
            }
        }
        if (this.isSpLocalSourceChecked) {
            let actionResult = this.spLocalArchiveActionCustomValidate();
            if (!(this.spLocalIsVerificationPassed && actionResult && this.spLocalApprovalIsPassed)) {
                spLocalSourceValidateResult = false;
                this.setState({
                    ruleCriteriaTabsIndex: Constants.RuleSourceTabIndex.SPLocal
                });
                return false;
            }
        }

        if(this.isAzureFileSourceChecked){
            if(!(this.azureFileIsVerificationPassed && this.azureFileApprovalIsPassed)){
                azureFileSourceValidateResult = false;
                this.setState({
                    ruleCriteriaTabsIndex: Constants.RuleSourceTabIndex.AzureFile
                });
                return false;
            }
        }

        if(this.isBoxSourceChecked){
            if(!(this.boxIsVerificationPassed && this.boxApprovalIsPassed)){
                boxSourceValidateResult = false;
                this.setState({
                    ruleCriteriaTabsIndex: Constants.RuleSourceTabIndex.Box
                });
                return false;
            }
        }

        if(this.IsGoogleDriveSourceChecked){
            let actionResult = this.googleArchiveActionCustomValidate();
            if(!(this.googleDriveIsVerificationPassed && actionResult && this.googleDriveApprovalIsPassed && this.googleExportLocationIsPassed
                && this.googleExportIsPassed && this.getStorageSettingIsValid(this.googleStorageSettingsRef))){
                googleDriveSourceValidateResult = false;
                this.setState({
                    ruleCriteriaTabsIndex: Constants.RuleSourceTabIndex.GoogleDrive
                });
                return false;
            }
        }

        if(this.isConnectorSourceChecked){
            if(!(this.connectorIsVerificationPassed && this.connectorApprovalIsPassed)){
                connectorSourceValidateResult = false;
                this.setState({
                    ruleCriteriaTabsIndex: Constants.RuleSourceTabIndex.Connector
                });
                return false;
            }
        }

        if (this.isTeamsSourceChecked) {
            const actionResult = this.teamsArchiveActionCustomValidate();
            if(!(this.teamsIsVerificationPassed && this.teamsExportIsPassed && actionResult && this.teamsExportLocationIsPassed &&
                this.teamsApprovalIsPassed && this.getStorageSettingIsValid(this.teamsStorageSettingsRef))){
                teamsSourceValidateResult = false;
                this.setState({
                    ruleCriteriaTabsIndex: Constants.RuleSourceTabIndex.Teams
                });
                return false;
            }
        }

        if (spSourceValidateResult && spLocalSourceValidateResult && oneDriveSourceValidateResult && exoSourceValidateResult && phySourceValidateResult && fsSourceValidateResult && azureFileSourceValidateResult && boxSourceValidateResult && googleDriveSourceValidateResult && teamsSourceValidateResult && connectorSourceValidateResult) {
            return true;
        } else {
            return false;
        }
    }

    ruleMoveOfFsCallback (valid, ruleMoveParam) {
        this.ruleMoveOfFsValid = valid;
        this.ruleMoveOfFsParam = ruleMoveParam;
    }

    //保存
    saveClick (baseInfo) {
        this.ruleMode = JSON.parse(JSON.stringify(this.initialRuleMode));
        let isRecordsModule = this.state.selectedRuleModuleType === RuleModuleTypes.Records || this.state.selectedRuleModuleType === RuleModuleTypes.None;
        let isArchiveModule = this.state.selectedRuleModuleType === RuleModuleTypes.SOArchiver;
        let isBackupDataConfirmForSPO = false;
        let isBackupDataConfirmForOD = false;
        let isBackupDataConfirmForFS = false;
        if (this.isSpSourceChecked) {
            this.dispatch("spCriteria", Constants.dispatchAction.save);
            this.dispatch("spApproval", Constants.dispatchAction.save);
            this.dispatch("spExport", Constants.dispatchAction.save);
            if(this.state.isArchiveToAzureBlobStorage){
                this.dispatch("raCrSpRemoveArchivedForSp", Constants.dispatchAction.save);
            }
            if(this.state.isExportOnly){
                this.dispatch("spExportOnly", Constants.dispatchAction.save);
            }
        }
        if (this.isSpLocalSourceChecked) {
            this.dispatch("spLocalCriteria", Constants.dispatchAction.save);
            this.dispatch("spLocalApproval", Constants.dispatchAction.save);
            // this.dispatch("spLocalExport", Constants.dispatchAction.save);
            // this.dispatch("spLocalExportOnly", Constants.dispatchAction.save);
        }
        if (this.isOneDriveSourceChecked) {
            this.dispatch("oneDriveCriteria", Constants.dispatchAction.save);
            this.dispatch("oneDriveApproval", Constants.dispatchAction.save);
            this.dispatch("oneDriveExport", Constants.dispatchAction.save);
            this.dispatch("oneDriveExportOnly", Constants.dispatchAction.save);
            if(this.state.isArchiveToAzureBlobStorageForOneDrive){
                this.dispatch("raCrRemoveArchivedForOd", Constants.dispatchAction.save);
            }
            if (isArchiveModule && this.levelId == Constants.RuleLevel.SiteCollection && (!this.state.isShowExportCheckboxForOneDrive || !this.state.isShowExportOnlyForOneDrive)) {
                this.oneDriveExportIsPassed = true;
                this.oneDriveExportLocationIsPassed = true;
            }
        }
        if (this.isExoSourceChecked) {
            this.dispatch("exoCriteria", Constants.dispatchAction.save);
            this.dispatch("exoApproval", Constants.dispatchAction.save);
            this.dispatch("exoExport", Constants.dispatchAction.save);
            this.dispatch("exoExportOnly", Constants.dispatchAction.save);
        }
        if (this.isPhySourceChecked) {
            this.dispatch("phyCriteria", Constants.dispatchAction.save);
            this.dispatch("phyApproval", Constants.dispatchAction.save);
            // this.dispatch("phyExport", Constants.dispatchAction.save);
            if(this.state.isPhyRemove && this.state.isDeleteRelatedRecordOptionOfPhy){
                this.dispatch("raCrRemoveArchivedForPhy", Constants.dispatchAction.save);
            }
        }
        if (this.isFsSourceChecked) {
            this.dispatch("fsRuleMoveOfFs", Constants.dispatchAction.save, this.ruleMoveOfFsCallback);
            this.dispatch("fsCriteria", Constants.dispatchAction.save);
            this.dispatch("fsApproval", Constants.dispatchAction.save);
        }
        if (this.isAzureFileSourceChecked) {
            this.dispatch("azureFileCriteria", Constants.dispatchAction.save);
            this.dispatch("azureFileApproval", Constants.dispatchAction.save);
        }
        if (this.isBoxSourceChecked) {
            this.dispatch("boxCriteria", Constants.dispatchAction.save);
            this.dispatch("boxApproval", Constants.dispatchAction.save);
        }
        if (this.IsGoogleDriveSourceChecked) {
            this.dispatch("googleDriveCriteria", Constants.dispatchAction.save);
            this.dispatch("googleDriveApproval", Constants.dispatchAction.save);
            this.dispatch("googleExport", Constants.dispatchAction.save);
            this.dispatch("googleExportOnly", Constants.dispatchAction.save);
            if(this.state.isArchiveToStorageForGoogle){
                this.dispatch("raCrRemoveArchivedForGoogle", Constants.dispatchAction.save);
            }
        }
        if (this.isTeamsSourceChecked) {
            this.dispatch("teamsCriteria", Constants.dispatchAction.save);
            this.dispatch("teamsApproval", Constants.dispatchAction.save);
            this.dispatch("teamsExport", Constants.dispatchAction.save);
            if(this.state.isArchiveToAzureBlobStorageForTeams){
                this.dispatch("raCrRemoveArchivedForTeams", Constants.dispatchAction.save);
            }
            if(this.state.isExportOnlyForTeams){
                this.dispatch("teamsExportOnly", Constants.dispatchAction.save);
            }
            if (isArchiveModule && !this.state.isShowExportOnlyForTeams) {
                this.teamsExportIsPassed = true;
                this.teamsExportLocationIsPassed = true;
            }
        }
        if (this.isConnectorSourceChecked) {
            this.dispatch("connectorCriteria", Constants.dispatchAction.save);
            this.dispatch("connectorApproval", Constants.dispatchAction.save);
        }
        if (!this.manageRule()) {
            return false;
        }
        let keepDataStatus = 0;
        this.originalContainerId = baseInfo.originalContainerId;
        this.ruleMode.RuleName = $.trim(baseInfo.RuleName);
        this.ruleMode.RuleLevel = this.levelId;
        this.ruleMode.Description = baseInfo.Description;
        this.ruleMode.ContainerId = baseInfo.ContainerId;
        this.ruleMode.DisposalClass = baseInfo.DisposalClass;
        this.ruleMode.ModelType = baseInfo.RuleModuleType;
        if (this.isSpSourceChecked == true) {
            this.ruleMode.IsSpSource = true;
            this.ruleMode.RuleKeepDataOption = 0;
            this.ruleMode.EnableManualApproval = this.spApprovalData.isApproval;
            this.ruleMode.ExportDataBeforeArchiving = this.spExportData.isExport && !this.spExportData.isExportOnly;
            this.ruleMode.EnableExport = this.spExportData.isExport || this.spExportData.isExportOnly;
            this.ruleMode.RuleFilters = this.spCriteriaData;
            this.ruleMode.Users = this.spApprovalData.users;
            this.ruleMode.IsSendEmailToOwner = this.spApprovalData.isSendEmail;
            this.ruleMode.ManualReviewType = this.spApprovalData.isApproval ? this.spApprovalData.manualReviewType : Constants.ReviewType.RecordOwner;
            this.ruleMode.WorkflowId = this.spApprovalData.workflowId;
            this.ruleMode.IncludeDeleteRecordLabel = this.state.isIncludeLockedFile;
            this.ruleMode.LockRecordBeforeDestroy = this.state.isLockRecord === true;
            if(this.spStorageSettingsRef){
                let spSelectedSStoragePolicyItem = this.spStorageSettingsRef.getSelectedStorage();
                this.ruleMode.StoragePolicyId = spSelectedSStoragePolicyItem.Id;
                this.ruleMode.StoragePolicyName = spSelectedSStoragePolicyItem.Name;
                this.ruleMode.MoveToAnotherTierType = this.spStorageSettingsRef.getTierType();
            }
            if (this.removeAchivedRefForSp) {
                let removeArchMaParam = this.removeAchivedRefForSp.getRemoveArchParam();
                if (this.state.selectedRuleModuleType != RuleModuleTypes.SOArchiver && removeArchMaParam.IsEnableRetention) {
                    this.ruleMode.IsEnableRetention = removeArchMaParam.IsEnableRetention;
                    this.ruleMode.RetentionInfo = removeArchMaParam.RetentionInfo;
                } else if (this.state.selectedRuleModuleType === RuleModuleTypes.SOArchiver && removeArchMaParam.some(p => p.IsEnableRetention)) {
                    this.ruleMode.RetentionInfoList = removeArchMaParam;
                }
            }
            if (!this.spApprovalData.isApproval) {
                this.ruleMode.WorkflowId = "";
            }

            if (this.state.isKeep) {
                if (this.state.isDeclare) {
                    keepDataStatus |= 4;
                }
                if (this.state.isUndeclare) {
                    keepDataStatus |= 512;
                }
                if (this.state.iskeepTag) {
                    keepDataStatus |= 1;
                }
                keepDataStatus |= 16;

                if (this.state.iskeepTag) {
                    this.ruleMode.TagContentInfo = this.spCovertTagInfo();
                }
            }
            if(isRecordsModule){
                if (this.state.isRemove) {
                    if (this.state.isLeaveStubOption && this.state.isShowLeaveStubOption) {
                        keepDataStatus |= 128;
                        this.ruleMode.StubTemplateId = this.state.selectedLevelStubSettingForSPO.Id;
                        this.ruleMode.StubTemplateName = this.state.selectedLevelStubSettingForSPO.Name;
                    }
                    if (!this.state.isBackupOption) {
                        keepDataStatus |= 256;
                        isBackupDataConfirmForSPO = true;
                    }
                    if (this.state.isDeleteRelatedRecordOption) {
                        isBackupDataConfirmForSPO = [Constants.RuleLevel.Document, Constants.RuleLevel.Item].includes(this.levelId);
                        if (this.state.isShowDeleteRelatedRecordOption) {
                            this.ruleMode.RelatedRecordOption = 1;
                        }
                    }
                    this.ruleMode.DeleteRecords = this.state.isDeclaredFile && this.state.isShowDeclaredFileOption;

                    if (this.state.isRetentionLabel) {
                        keepDataStatus |= 262144;
                    }
                    if (this.levelId === Constants.RuleLevel.SiteCollection){
                        this.ruleMode.DeleteSiteCollectionToRecycleBin = this.state.isDeleteToRecycleBinForSPO;
                    } else if (this.levelId === Constants.RuleLevel.Document) {
                        this.ruleMode.DeleteToRecycleBin = this.state.isDeleteToRecycleBinForSPO;
                    }
                }
                if(this.state.isArchiveToAzureBlobStorage){
                    keepDataStatus = 1024;
                    if(this.state.isLeaveStubOption){
                        keepDataStatus = 2048;
                        this.ruleMode.StubTemplateId = this.state.selectedLevelStubSettingForSPO.Id;
                        this.ruleMode.StubTemplateName = this.state.selectedLevelStubSettingForSPO.Name;
                    }
                    
                    if (this.state.isRetentionLabel) {
                        keepDataStatus |= 262144;
                    }

                    if (this.state.isArchivingRecordOption) {
                        this.ruleMode.DeleteRecords = this.state.isArchivingRecordOption;
                    }
                }
            }
            if(isArchiveModule){
                if (this.state.isSORemoveForSPO) {
                    keepDataStatus = 16384;
                    if (this.state.isKeepVersionOption) {
                        keepDataStatus |= 32768;
                        this.ruleMode.KeepLatestMajorAndMinorVersion = this.state.keepVersionValue;
                    }

                    this.ruleMode.IncludeDeleteRecordLabel = this.state.isIncludeLockedFile;
                    this.ruleMode.LockRecordBeforeDestroy = this.state.isLockRecord === true;

                    if (this.state.isArchivingRecordOption) {
                        this.ruleMode.DeleteRecords = this.state.isArchivingRecordOption;
                    }
                    if ([Constants.RuleLevel.Document, Constants.RuleLevel.DocumentVersion].includes(this.levelId)) {
                        this.ruleMode.DeleteToRecycleBin = this.state.isSODeleteToRecycleBinForSPO;
                    }
                }


                if(this.state.isBackupAndRemoveForSPO){
                    keepDataStatus = 4096;
                    if (this.levelId === Constants.RuleLevel.SiteCollection){
                        this.ruleMode.DeleteSiteCollectionToRecycleBin = this.state.isSODeleteToRecycleBinForSPO;
                    }
                }
                if(this.state.isBackupAndRemoveForSPO && this.state.isLeaveStubOption){
                    keepDataStatus = 8192;
                    this.ruleMode.StubTemplateId = this.state.selectedLevelStubSettingForSPO.Id;
                    this.ruleMode.StubTemplateName = this.state.selectedLevelStubSettingForSPO.Name;
                }
                if (this.state.isBackupAndRemoveForSPO && this.state.isArchiveVersionOption) {
                    keepDataStatus = keepDataStatus + 65536;
                    this.ruleMode.ArchivedLatestVersion = this.state.archiveVersionValue;
                    isBackupDataConfirmForSPO = this.levelId === Constants.RuleLevel.Document;
                }
                if (this.state.isSORemoveForSPO) {
                    isBackupDataConfirmForSPO = [Constants.RuleLevel.Document, Constants.RuleLevel.DocumentVersion].includes(this.levelId);
                }
                if (this.state.isBackupAndRemoveForSPO && this.state.isKeepVersionAndArchiveForSPO) {
                    keepDataStatus = keepDataStatus + 131072;
                    this.ruleMode.KeepLatestMajorAndMinorVersionAndArchiveOthers = this.state.keepVersionAndArchiveValueForSPO;
                }
                if (this.state.isBackupAndRemoveForSPO && this.state.isArchivingRecordOption) {
                    this.ruleMode.DeleteRecords = this.state.isArchivingRecordOption;
                }

                if (this.state.isArchiveWithoutDestroy) {
                    keepDataStatus = 524288;
                }
                if (this.state.isArchiveWithoutDestroy && this.state.isArchiveVersionOption) {
                    keepDataStatus = keepDataStatus + 1048576;
                    this.ruleMode.ArchiverOnlyLastestVersion = this.state.archiveVersionValue;
                }
            }
               
            if (this.state.isMove && !this.spExportData.isExportOnly) {
                this.ruleMode.MoveToRecordCenterSettings = null;
                this.ruleMode.MoveDto = this.moveFunc();
                if (this.state.isRetentionLabel) {
                    keepDataStatus |= 262144;
                }
            } else {
                this.ruleMode.MoveDto = null;
            }
            if (this.state.isStoreInM365Archive) {
                keepDataStatus = 2097152;
            }
            this.ruleMode.RuleKeepDataOption = keepDataStatus;
           
            
            if(this.ruleMode.EnableExport)
            {
                this.ruleMode.ExportInfo.exportType = this.spExportData.exportTypeValue;
                this.ruleMode.ExportInfo.exportSPDataOption = this.spExportData.isExportOnly? this.ExportSPDataOption.ExportWithoutArchive : this.ExportSPDataOption.ExportBeforeArchive;
                
                if (this.spExportData.exportTypeValue) {                    
                    if (this.spExportData.exportLocationOption == ExportLocationOption.Storage) {
                        this.ruleMode.ExportInfo.exportLocationId = this.spExportData.storageId;
                        this.ruleMode.ExportInfo.exportLocationName = this.spExportData.storageName;
                    } else {
                        this.ruleMode.MoveDto = this.exportLocationFunc();
                    }
                } else {
                    this.ruleMode.MoveDto = null;
                }
            }
        }
        if (this.isSpLocalSourceChecked == true) {
            keepDataStatus = 0;
            this.ruleMode.IsSPLocalSource = true;
            this.ruleMode.SPLocalRule.RuleLevel = this.levelId;
            this.ruleMode.SPLocalRule.RuleKeepDataOption = 0;
            this.ruleMode.SPLocalRule.EnableManualApproval = this.spLocalApprovalData.isApproval;
            // this.ruleMode.SPLocalRule.ExportDataBeforeArchiving = this.spLocalExportData.isExport;
            // this.ruleMode.SPLocalRule.EnableExport = this.spLocalExportData.isExport || this.spLocalExportData.isExportOnly;
            this.ruleMode.SPLocalRule.RuleFilters = this.spLocalCriteriaData;
            this.ruleMode.SPLocalRule.Users = this.spLocalApprovalData.users;
            this.ruleMode.SPLocalRule.IsSendEmailToOwner = this.spLocalApprovalData.isSendEmail;
            this.ruleMode.SPLocalRule.ManualReviewType = this.spLocalApprovalData.isApproval ? this.spLocalApprovalData.manualReviewType : Constants.ReviewType.RecordOwner;
            this.ruleMode.SPLocalRule.WorkflowId = this.spLocalApprovalData.workflowId;
            if (!this.spLocalApprovalData.isApproval) {
                this.ruleMode.SPLocalRule.WorkflowId = "";
            }

            if (this.state.isKeepForLocal) {
                if (this.state.isDeclareForLocal) {
                    keepDataStatus |= 4;
                }
                if (this.state.isUndeclare) {
                    keepDataStatus |= 512;
                }
                if (this.state.iskeepTagForLocal) {
                    keepDataStatus |= 1;
                }
                keepDataStatus |= 16;
                if (this.state.iskeepTagForLocal) {
                    this.ruleMode.SPLocalRule.TagContentInfo = this.spCovertTagInfoForLocal();
                }
            }
            if (this.state.isRemoveForLocal) {
                if (this.state.isLeaveStubOptionForLocal && this.state.isShowLeaveStubOptionForLocal) {
                    keepDataStatus |= 128;
                    this.ruleMode.SPLocalRule.DeclareLinkFile = this.state.isDeclareLinkFileForLocal;
                }
                if (!this.state.isBackupOptionForLocal) {
                    keepDataStatus |= 256;
                }
                if (this.state.isDeleteRelatedRecordOptionForLocal && this.state.isShowDeleteRelatedRecordOptionForLocal) {
                    this.ruleMode.SPLocalRule.RelatedRecordOption = 1;
                }
                this.ruleMode.SPLocalRule.DeleteRecords = this.state.isDeclaredFileForLocal && this.state.isShowDeclaredFileOptionForLocal;
            }
            if (this.state.isMoveForLocal) {
                this.ruleMode.SPLocalRule.MoveToRecordCenterSettings = null;
                this.ruleMode.SPLocalRule.MoveDto = this.moveFuncForLocal();
            } else {
                this.ruleMode.SPLocalRule.MoveDto = null;
            }
            this.ruleMode.SPLocalRule.RuleKeepDataOption = keepDataStatus;
            
            // if(this.ruleMode.EnableExportForLocal)
            // {
            //     this.ruleMode.SPLocalRule.ExportInfo.exportType = this.spLocalExportData.exportTypeValue;
            //     this.ruleMode.SPLocalRule.ExportInfo.exportSPDataOption = this.spLocalExportData.isExportOnly? this.ExportSPDataOption.ExportWithoutArchive : this.ExportSPDataOption.ExportBeforeArchive;
            // }
        } else {
            this.ruleMode.SPLocalRule = null;
        }

        if (this.isOneDriveSourceChecked == true) {
            keepDataStatus = 0;
            this.ruleMode.IsOneDriveSource = true;
            this.ruleMode.OneDriveRule.RuleLevel = this.levelId;
            this.ruleMode.OneDriveRule.RuleKeepDataOption = 0;
            this.ruleMode.OneDriveRule.EnableManualApproval = this.oneDriveApprovalData.isApproval;
            this.ruleMode.OneDriveRule.ExportDataBeforeArchiving = this.oneDriveExportData.isExport && !this.oneDriveExportData.isExportOnly;
            this.ruleMode.OneDriveRule.EnableExport = this.oneDriveExportData.isExport || this.oneDriveExportData.isExportOnly;
            this.ruleMode.OneDriveRule.RuleFilters = this.oneDriveCriteriaData;
            this.ruleMode.OneDriveRule.Users = this.oneDriveApprovalData.users;
            this.ruleMode.OneDriveRule.IsSendEmailToOwner = this.oneDriveApprovalData.isSendEmail;
            this.ruleMode.OneDriveRule.ManualReviewType = this.oneDriveApprovalData.isApproval ? this.oneDriveApprovalData.manualReviewType : Constants.ReviewType.RecordOwner;
            this.ruleMode.OneDriveRule.WorkflowId = this.oneDriveApprovalData.workflowId;
            this.ruleMode.OneDriveRule.IncludeDeleteRecordLabel = this.state.isIncludeLockedFileForOneDrive;
            this.ruleMode.OneDriveRule.LockRecordBeforeDestroy = this.state.isLockRecordForOneDrive === true;
            this.ruleMode.OneDriveRule.DeleteToRecycleBin = this.state.isDeleteToRecycleBinForOneDrive;
            this.ruleMode.DeleteToRecycleBin = this.ruleMode.OneDriveRule.DeleteToRecycleBin;
            if(this.oneDriveStorageSettingsRef){
                let onedriveSelectedSStoragePolicyItem = this.oneDriveStorageSettingsRef.getSelectedStorage();
                this.ruleMode.OneDriveRule.StoragePolicyId = onedriveSelectedSStoragePolicyItem.Id;
                this.ruleMode.OneDriveRule.StoragePolicyName = onedriveSelectedSStoragePolicyItem.Name;
                this.ruleMode.OneDriveRule.MoveToAnotherTierType = this.oneDriveStorageSettingsRef.getTierType();
            }
            if (this.removeAchivedRefForOd) {
                let removeArchMaParam = this.removeAchivedRefForOd.getRemoveArchParam();
                if (this.state.selectedRuleModuleType != RuleModuleTypes.SOArchiver && removeArchMaParam.IsEnableRetention) {
                    this.ruleMode.OneDriveRule.IsEnableRetention = removeArchMaParam.IsEnableRetention;
                    this.ruleMode.OneDriveRule.RetentionInfo = removeArchMaParam.RetentionInfo;
                } else if (this.state.selectedRuleModuleType === RuleModuleTypes.SOArchiver && removeArchMaParam.some(p => p.IsEnableRetention)) {
                    this.ruleMode.OneDriveRule.RetentionInfoList = removeArchMaParam;
                }
            }
            if (!this.oneDriveApprovalData.isApproval) {
                this.ruleMode.OneDriveRule.WorkflowId = "";
            }

            if (this.state.isKeepForOneDrive) {
                if (this.state.isDeclareForOneDrive) {
                    keepDataStatus |= 4;
                }
                if (this.state.isUndeclareForOneDrive) {
                    keepDataStatus |= 512;
                }

                if (this.state.iskeepTagForOneDrive) {
                    keepDataStatus |= 1;
                }
                keepDataStatus |= 16;

                if (this.state.iskeepTagForOneDrive) {
                    this.ruleMode.OneDriveRule.TagContentInfo = this.oneDriveCovertTagInfo();
                }
            }
            if(isRecordsModule){
                if (this.state.isRemoveForOneDrive) {
                    if (this.state.isLeaveStubOptionForOneDrive && this.state.isShowLeaveStubOptionForOneDrive) {
                        keepDataStatus |= 128;
                        this.ruleMode.OneDriveRule.StubTemplateId = this.state.selectedLevelStubSettingForOneDrive.Id;
                        this.ruleMode.OneDriveRule.StubTemplateName = this.state.selectedLevelStubSettingForOneDrive.Name;
                    }
                    if (!this.state.isBackupOptionForOneDrive) {
                        keepDataStatus |= 256;
                        isBackupDataConfirmForOD = true;
                    }
                    if (this.state.isDeleteRelatedRecordOptionForOneDrive && this.state.isShowDeleteRelatedRecordOptionForOneDrive) {
                        this.ruleMode.OneDriveRule.RelatedRecordOption = 1;
                    }
                    this.ruleMode.OneDriveRule.DeleteRecords = this.state.isDeclaredFileForOneDrive && this.state.isShowDeclaredFileOptionForOneDrive;

                    if (this.state.isRetentionLabelForOneDrive) {
                        keepDataStatus |= 262144;
                    }
                }
                if(this.state.isArchiveToAzureBlobStorageForOneDrive){
                    keepDataStatus = 1024;
                    if(this.state.isLeaveStubOptionForOneDrive){
                        keepDataStatus = 2048;
                        this.ruleMode.OneDriveRule.StubTemplateId = this.state.selectedLevelStubSettingForOneDrive.Id;
                        this.ruleMode.OneDriveRule.StubTemplateName = this.state.selectedLevelStubSettingForOneDrive.Name;
                    }

                    if (this.state.isRetentionLabelForOneDrive) {
                        keepDataStatus |= 262144;
                    }

                    if (this.state.isArchivingRecordOptionForOneDrive) {
                        this.ruleMode.OneDriveRule.DeleteRecords = this.state.isArchivingRecordOptionForOneDrive;
                    }
                }
            }

            if(isArchiveModule){
                if (this.state.isSORemoveForOD) {
                    keepDataStatus = 16384;
                    if (this.state.isKeepVersionOptionForOD) {
                        keepDataStatus |= 32768;
                        this.ruleMode.OneDriveRule.KeepLatestMajorAndMinorVersion = this.state.keepVersionValueForOD;
                    }

                    if (this.state.isArchivingRecordOptionForOneDrive) {
                        this.ruleMode.OneDriveRule.DeleteRecords = this.state.isArchivingRecordOptionForOneDrive;
                    }

                    this.ruleMode.OneDriveRule.IncludeDeleteRecordLabel = this.state.isIncludeLockedFileForOneDrive;
                    this.ruleMode.OneDriveRule.LockRecordBeforeDestroy = this.state.isLockRecordForOneDrive === true;
                }
                if(this.state.isBackupAndRemoveForOD){
                    keepDataStatus = 4096;
                }
                if(this.state.isBackupAndRemoveForOD && this.state.isLeaveStubOptionForOneDrive){
                    keepDataStatus = 8192;
                    this.ruleMode.OneDriveRule.StubTemplateId = this.state.selectedLevelStubSettingForOneDrive.Id;
                    this.ruleMode.OneDriveRule.StubTemplateName = this.state.selectedLevelStubSettingForOneDrive.Name;
                }
                if (this.state.isBackupAndRemoveForOD && this.state.isArchiveVersionOptionForOD) {
                    keepDataStatus = keepDataStatus + 65536;
                    this.ruleMode.OneDriveRule.ArchivedLatestVersion = this.state.archiveVersionValueForOD;
                    isBackupDataConfirmForOD = this.levelId === Constants.RuleLevel.Document;
                } 
                if (this.state.isSORemoveForOD) {
                    isBackupDataConfirmForOD = [Constants.RuleLevel.Document, Constants.RuleLevel.DocumentVersion].includes(this.levelId);
                }
                if (this.state.isBackupAndRemoveForOD && this.state.isKeepVersionAndArchiveForOD) {
                    keepDataStatus = keepDataStatus + 131072;
                    this.ruleMode.OneDriveRule.KeepLatestMajorAndMinorVersionAndArchiveOthers = this.state.keepVersionAndArchiveValueForOD;
                }
                if (this.state.isBackupAndRemoveForOD && this.state.isArchivingRecordOptionForOneDrive) {
                    this.ruleMode.OneDriveRule.DeleteRecords = this.state.isArchivingRecordOptionForOneDrive;
                }

                if (this.state.isArchiveWithoutDestroyForOneDrive) {
                    keepDataStatus = 524288;
                }
                if (this.state.isArchiveWithoutDestroyForOneDrive && this.state.isArchiveVersionOptionForOD) {
                    keepDataStatus = keepDataStatus + 1048576;
                    this.ruleMode.OneDriveRule.ArchiverOnlyLastestVersion = this.state.archiveVersionValueForOD;
                }
                this.ruleMode.OneDriveRule.DeleteToRecycleBin = this.state.isSODeleteToRecycleBinForOD;
            }
            
            if (this.state.isMoveForOneDrive && !this.oneDriveExportData.isExportOnly) {
                this.ruleMode.OneDriveRule.MoveToRecordCenterSettings = null;
                this.ruleMode.OneDriveRule.MoveDto = this.moveFuncForOneDrive();
                if (this.state.isRetentionLabelForOneDrive) {
                    keepDataStatus |= 262144;
                }
            } else {
                this.ruleMode.OneDriveRule.MoveDto = null;
            }
            this.ruleMode.OneDriveRule.RuleKeepDataOption = keepDataStatus;
            
            if(this.ruleMode.OneDriveRule.EnableExport)
            {
                this.ruleMode.OneDriveRule.ExportInfo.exportType = this.oneDriveExportData.exportTypeValue;
                this.ruleMode.OneDriveRule.ExportInfo.exportSPDataOption = this.oneDriveExportData.isExportOnly? this.ExportSPDataOption.ExportWithoutArchive : this.ExportSPDataOption.ExportBeforeArchive;
                // 3 is VEO type
                if (this.oneDriveExportData.exportTypeValue) {
                    if (this.oneDriveExportData.exportLocationOption == ExportLocationOption.Storage) {
                        this.ruleMode.OneDriveRule.ExportInfo.exportLocationId = this.oneDriveExportData.storageId;
                        this.ruleMode.OneDriveRule.ExportInfo.exportLocationName = this.oneDriveExportData.storageName;
                    } else {
                        this.ruleMode.OneDriveRule.MoveDto = this.exportLocationFuncForOneDrive();
                    }
                } else {
                    this.ruleMode.EXORule.MoveDto = null;
                }
            }
        }else {
            this.ruleMode.OneDriveRule = null;
        }

        if (this.isTeamsSourceChecked) {
            keepDataStatus = 0;
            this.ruleMode.IsTeamsSource = true;
            this.ruleMode.TeamsRule.RuleLevel = this.levelId == Constants.TeamsLevelIds.Teams ? Constants.TeamsLevelIds.Teams : this.levelId;
            this.ruleMode.TeamsRule.RuleKeepDataOption = 0;
            this.ruleMode.TeamsRule.EnableManualApproval = this.teamsApprovalData.isApproval;
            this.ruleMode.TeamsRule.ExportDataBeforeArchiving = this.teamsExportData.isExport && !this.teamsExportData.isExportOnly;
            this.ruleMode.TeamsRule.EnableExport = this.teamsExportData.isExport || this.teamsExportData.isExportOnly;
            this.ruleMode.TeamsRule.RuleFilters = this.teamsCriteriaData;
            this.ruleMode.TeamsRule.Users = this.teamsApprovalData.users;
            this.ruleMode.TeamsRule.IsSendEmailToOwner = this.teamsApprovalData.isSendEmail;
            this.ruleMode.TeamsRule.ManualReviewType = this.teamsApprovalData.isApproval ? this.teamsApprovalData.manualReviewType : Constants.ReviewType.RecordOwner;
            this.ruleMode.TeamsRule.WorkflowId = this.teamsApprovalData.workflowId;
            this.ruleMode.TeamsRule.IncludeDeleteRecordLabel = this.state.isIncludeLockedFileForTeams;
            this.ruleMode.TeamsRule.LockRecordBeforeDestroy = this.state.isLockRecordBeforeDestroyForTeams === true;

            if (this.teamsStorageSettingsRef) {
                const teamsSelectedStoragePolicyItem = this.teamsStorageSettingsRef.getSelectedStorage();
                this.ruleMode.TeamsRule.StoragePolicyId = teamsSelectedStoragePolicyItem.Id;
                this.ruleMode.TeamsRule.StoragePolicyName = teamsSelectedStoragePolicyItem.Name;
                this.ruleMode.TeamsRule.MoveToAnotherTierType = this.teamsStorageSettingsRef.getTierType();
            }

            if (this.removeAchivedRefForTeams) {
                let removeArchMaParam = this.removeAchivedRefForTeams.getRemoveArchParam();
                if (this.state.selectedRuleModuleType != RuleModuleTypes.SOArchiver && removeArchMaParam.IsEnableRetention) {
                    this.ruleMode.TeamsRule.IsEnableRetention = removeArchMaParam.IsEnableRetention;
                    this.ruleMode.TeamsRule.RetentionInfo = removeArchMaParam.RetentionInfo;
                } else if (this.state.selectedRuleModuleType === RuleModuleTypes.SOArchiver && removeArchMaParam.some(p => p.IsEnableRetention)) {
                    this.ruleMode.TeamsRule.RetentionInfoList = removeArchMaParam;
                }
            }

            if (!this.teamsApprovalData.isApproval) {
                this.ruleMode.TeamsRule.WorkflowId = "";
            }

            if (this.state.isKeepForTeams) {
                if (this.state.isDeclareForTeams) {
                    keepDataStatus |= 4;
                }
                if (this.state.isUndeclareForTeams) {
                    keepDataStatus |= 512;
                }
                if (this.state.iskeepTagForTeams) {
                    keepDataStatus |= 1;
                }
                keepDataStatus |= 16;

                if (this.state.iskeepTagForTeams) {
                    this.ruleMode.TeamsRule.TagContentInfo = this.teamsCovertTagInfo();
                }
            }

            if (isRecordsModule) {
                if (this.state.isRemoveForTeams) {
                    if (this.state.isLeaveStubOptionForTeams && this.state.isShowLeaveStubOptionForTeams) {
                        keepDataStatus |= 128;
                        this.ruleMode.TeamsRule.StubTemplateId = this.state.selectedLevelStubSettingForTeams.Id;
                        this.ruleMode.TeamsRule.StubTemplateName = this.state.selectedLevelStubSettingForTeams.Name;
                    }
                    if (!this.state.isBackupOptionForTeams) {
                        keepDataStatus |= 256;
                    }
                    if (this.state.isDeleteRelatedRecordOptionForTeams && this.state.isShowDeleteRelatedRecordOptionForTeams) {
                        this.ruleMode.TeamsRule.RelatedRecordOption = 1;
                    }
                    this.ruleMode.TeamsRule.DeleteRecords = this.state.isDeclaredFileForTeams && this.state.isShowDeclaredFileOptionForTeams;

                    if (this.state.isRetentionLabelForTeams) {
                        keepDataStatus |= 262144;
                    }
                }
                if(this.state.isArchiveToAzureBlobStorageForTeams){
                    keepDataStatus = 1024;
                    if(this.state.isLeaveStubOptionForTeams){
                        keepDataStatus = 2048;
                        this.ruleMode.TeamsRule.StubTemplateId = this.state.selectedLevelStubSettingForTeams.Id;
                        this.ruleMode.TeamsRule.StubTemplateName = this.state.selectedLevelStubSettingForTeams.Name;
                    }

                    if (this.state.isRetentionLabelForTeams) {
                        keepDataStatus |= 262144;
                    }
                    
                    if (this.state.isArchivingRecordOptionForTeams) {
                        this.ruleMode.TeamsRule.DeleteRecords = this.state.isArchivingRecordOptionForTeams;
                    }

                    this.ruleMode.TeamsRule.IncludeDeleteRecordLabel = this.state.isIncludeLockedFileForTeams;
                    this.ruleMode.TeamsRule.LockRecordBeforeDestroy = this.state.isLockRecordBeforeDestroyForTeams === true;
                }
            }

            if(isArchiveModule) {
                if (this.state.isSORemoveForTeams) {
                    keepDataStatus = 16384;
                    if (this.state.isKeepVersionOptionForTeams) {
                        keepDataStatus |= 32768;
                        this.ruleMode.TeamsRule.KeepLatestMajorAndMinorVersion = this.state.keepVersionValueForTeams;
                    }

                    if (this.state.isArchivingRecordOptionForTeams) {
                        this.ruleMode.TeamsRule.DeleteRecords = this.state.isArchivingRecordOptionForTeams;
                    }
                }
                if(this.state.isBackupAndRemoveForTeams){
                    keepDataStatus = 4096;
                }
                if(this.state.isBackupAndRemoveForTeams && this.state.isLeaveStubOptionForTeams){
                    keepDataStatus = 8192;
                    this.ruleMode.TeamsRule.StubTemplateId = this.state.selectedLevelStubSettingForTeams.Id;
                    this.ruleMode.TeamsRule.StubTemplateName = this.state.selectedLevelStubSettingForTeams.Name;
                }
                if (this.state.isBackupAndRemoveForTeams && this.state.isArchiveVersionOptionForTeams) {
                    keepDataStatus = keepDataStatus + 65536;
                    this.ruleMode.TeamsRule.ArchivedLatestVersion = this.state.archiveVersionValueForTeams;
                }
                if (this.state.isBackupAndRemoveForTeams && this.state.isKeepVersionAndArchiveForTeams) {
                    keepDataStatus = keepDataStatus + 131072;
                    this.ruleMode.TeamsRule.KeepLatestMajorAndMinorVersionAndArchiveOthers = this.state.keepVersionAndArchiveValueForTeams;
                }
                if (this.state.isBackupAndRemoveForTeams && this.state.isArchivingRecordOptionForTeams) {
                    this.ruleMode.TeamsRule.DeleteRecords = this.state.isArchivingRecordOptionForTeams;
                }
            }

            if (this.state.isMoveForTeams && !this.teamsExportData.isExportOnly) {
                this.ruleMode.TeamsRule.MoveToRecordCenterSettings = null;
                this.ruleMode.TeamsRule.MoveDto = this.moveFuncForTeams();
                if (this.state.isRetentionLabelForTeams) {
                    keepDataStatus |= 262144;
                }
            } else {
                this.ruleMode.TeamsRule.MoveDto = null;
            }

            this.ruleMode.TeamsRule.RuleKeepDataOption = keepDataStatus;
            
            if(this.ruleMode.TeamsRule.EnableExport)
            {
                this.ruleMode.TeamsRule.ExportInfo.exportType = this.teamsExportData.exportTypeValue;
                this.ruleMode.TeamsRule.ExportInfo.exportSPDataOption = this.teamsExportData.isExportOnly ? this.ExportSPDataOption.ExportWithoutArchive : this.ExportSPDataOption.ExportBeforeArchive;
                
                if (this.teamsExportData.exportTypeValue) {                    
                    if (this.teamsExportData.exportLocationOption == ExportLocationOption.Storage) {
                        this.ruleMode.TeamsRule.ExportInfo.exportLocationId = this.teamsExportData.storageId;
                        this.ruleMode.TeamsRule.ExportInfo.exportLocationName = this.teamsExportData.storageName;
                    } else {
                        this.ruleMode.TeamsRule.MoveDto = this.exportLocationFuncForTeams();
                    }
                } else {
                    this.ruleMode.TeamsRule.MoveDto = null;
                }
            }
        } else {
            this.ruleMode.TeamsRule = null;
        }

        if (this.isExoSourceChecked) {
            keepDataStatus = 0;
            this.ruleMode.EXORule.RuleFilters = this.exoCriteriaData;
            this.ruleMode.IsExoSource = true;
            this.ruleMode.EXORule.RuleLevel = 65536;
            this.ruleMode.EXORule.EnableManualApproval = this.exoApprovalData.isApproval;
            this.ruleMode.EXORule.ExportDataBeforeArchiving = this.exoExportData.isExport;
            this.ruleMode.EXORule.EnableExport = this.exoExportData.isExport || this.exoExportData.isExportOnly;
            this.ruleMode.EXORule.Users = this.exoApprovalData.users;
            this.ruleMode.EXORule.IsSendEmailToOwner = this.exoApprovalData.isSendEmail;
            this.ruleMode.EXORule.ManualReviewType = this.exoApprovalData.isApproval ? this.exoApprovalData.manualReviewType : Constants.ReviewType.RecordOwner;
            this.ruleMode.EXORule.WorkflowId = this.exoApprovalData.workflowId;
            if (!this.exoApprovalData.isApproval) {
                this.ruleMode.EXORule.WorkflowId = "";
            }
            this.ruleMode.EXORule.RuleKeepDataOption = 0;
            if (this.state.isExoKeep) {
                keepDataStatus |= 1;
                keepDataStatus |= 16;
                this.ruleMode.EXORule.RuleKeepDataOption = keepDataStatus;
                this.ruleMode.EXORule.TagContentInfo = this.exoCovertTagInfo();
            }
            if (this.state.isExoMove && !this.exoExportData.isExportOnly) {
                this.ruleMode.EXORule.MoveToRecordCenterSettings = null;
                this.ruleMode.EXORule.MoveDto = this.moveExoFunc();
            }
            else {
                this.ruleMode.EXORule.MoveDto = null;
            }
            if(this.ruleMode.EXORule.EnableExport)
            {
                this.ruleMode.EXORule.ExportInfo.exportType = this.exoExportData.exportTypeValue;
                this.ruleMode.EXORule.ExportInfo.exportSPDataOption = this.exoExportData.isExportOnly? this.ExportSPDataOption.ExportWithoutArchive : this.ExportSPDataOption.ExportBeforeArchive;
                // 3 is VEO type
                if (this.exoExportData.exportTypeValue) {
                    if (this.exoExportData.exportLocationOption == ExportLocationOption.Storage) {
                        this.ruleMode.EXORule.ExportInfo.exportLocationId = this.exoExportData.storageId;
                        this.ruleMode.EXORule.ExportInfo.exportLocationName = this.exoExportData.storageName;
                    } else {
                        this.ruleMode.EXORule.MoveDto = this.exportLocationFuncForEXO();
                    }
                } else {
                    this.ruleMode.EXORule.MoveDto = null;
                }
            }
        } else {
            this.ruleMode.EXORule = null;
        }

        if (this.isPhySourceChecked) {
            keepDataStatus = 0;
            this.ruleMode.PhysicalRule.RuleFilters = this.phyCriteriaData;
            this.ruleMode.IsPhySource = true;
            this.ruleMode.PhysicalRule.RuleLevel = this.getRealPhysicalLevelId();
            this.ruleMode.PhysicalRule.EnableManualApproval = this.phyApprovalData.isApproval;
            this.ruleMode.PhysicalRule.ExportDataBeforeArchiving = this.phyExportData.isExport;
            this.ruleMode.PhysicalRule.EnableExport = this.phyExportData.isExport;
            this.ruleMode.PhysicalRule.Users = this.phyApprovalData.users;
            this.ruleMode.PhysicalRule.IsSendEmailToOwner = this.phyApprovalData.isSendEmail;
            this.ruleMode.PhysicalRule.ManualReviewType = this.phyApprovalData.isApproval ? this.phyApprovalData.manualReviewType : Constants.ReviewType.RecordOwner;
            this.ruleMode.PhysicalRule.WorkflowId = this.phyApprovalData.workflowId;
            this.ruleMode.PhysicalRule.IsCalculationDisposalDate = this.state.isCalculationDisposalDate;
            if (this.phyStorageSettingsRef) {
                let phySelectedSStoragePolicyItem = this.phyStorageSettingsRef.getSelectedStorage();
                this.ruleMode.PhysicalRule.StoragePolicyId = phySelectedSStoragePolicyItem.Id;
                this.ruleMode.PhysicalRule.StoragePolicyName = phySelectedSStoragePolicyItem.Name;
                this.ruleMode.PhysicalRule.MoveToAnotherTierType = this.phyStorageSettingsRef.getTierType();
            }
            if (this.removeAchivedRefForPhy) {
                let removeArchMaParam = this.removeAchivedRefForPhy.getRemoveArchParam();
                if (removeArchMaParam.IsEnableRetention) {
                    this.ruleMode.PhysicalRule.IsEnableRetention = removeArchMaParam.IsEnableRetention;
                    this.ruleMode.PhysicalRule.RetentionInfo = removeArchMaParam.RetentionInfo;
                }
            }
            if (!this.phyApprovalData.isApproval) {
                this.ruleMode.PhysicalRule.WorkflowId = "";
            }
            this.ruleMode.PhysicalRule.RuleKeepDataOption = 0;
            if (this.state.isPhyMove) {
                this.ruleMode.PhysicalRule.MoveDto = this.movePhyFunc();
            } else {
                if (this.state.isDeleteRelatedRecordOptionOfPhy && this.state.isShowDeleteRelatedRecordOptionOfPhy) {
                    this.ruleMode.PhysicalRule.RelatedRecordOption = 1;
                }
                if (this.state.isDestoryEmptyBoxOnFolderRuleOptionOfPhy && this.state.isShowDestoryEmptyBoxOnFolderRuleOptionOfPhy) {
                    this.ruleMode.PhysicalRule.DestroyEmptyBoxOnFolderRule = true;
                }

                this.ruleMode.PhysicalRule.MoveDto = null;
            }
            if (this.phyExportData.isExport) {
                this.ruleMode.PhysicalRule.ExportInfo.exportType = this.phyExportData.exportTypeValue;
            }
        } else {
            this.ruleMode.PhysicalRule = null;
        }

        if(this.isAzureFileSourceChecked){
            this.ruleMode.IsAzureFileSource = true;
            this.ruleMode.AzureFileRule.RuleFilters = this.azureFileCriteriaData;
            if (this.state.isAzureFileRemove) {
                if (this.state.isLeaveStubOptionForAzureFile && this.levelId == 64) {
                    this.ruleMode.AzureFileRule.RuleKeepDataOption |= 128;
                }
            }
            this.ruleMode.AzureFileRule.EnableManualApproval = this.azureFileApprovalData.isApproval;
            this.ruleMode.AzureFileRule.Users = this.azureFileApprovalData.users;
            this.ruleMode.AzureFileRule.IsSendEmailToOwner = this.azureFileApprovalData.isSendEmail;
            this.ruleMode.AzureFileRule.ManualReviewType = this.azureFileApprovalData.isApproval ? this.azureFileApprovalData.manualReviewType : Constants.ReviewType.RecordOwner;
            this.ruleMode.AzureFileRule.WorkflowId = this.azureFileApprovalData.workflowId;
        } else {
            this.ruleMode.AzureFileRule = null; 
        }

        if (this.isBoxSourceChecked) {
            this.ruleMode.IsBoxSource = true;
            this.ruleMode.BoxRule.RuleFilters = this.boxCriteriaData;
            this.ruleMode.BoxRule.EnableManualApproval = this.boxApprovalData.isApproval;
            this.ruleMode.BoxRule.Users = this.boxApprovalData.users;
            this.ruleMode.BoxRule.IsSendEmailToOwner = this.boxApprovalData.isSendEmail;
            this.ruleMode.BoxRule.ManualReviewType = this.boxApprovalData.isApproval ? this.boxApprovalData.manualReviewType : Constants.ReviewType.RecordOwner;
            this.ruleMode.BoxRule.WorkflowId = this.boxApprovalData.workflowId;
        } else {
            this.ruleMode.BoxRule = null;
        }

        if (this.IsGoogleDriveSourceChecked) {
            this.ruleMode.IsGoogleDriveSource = true;
            this.ruleMode.GoogleDriveRule.IsGControlManualApproval = this.state.isGControlManualApproval;
            this.ruleMode.GoogleDriveRule.RuleFilters = this.googleDriveCriteriaData;
            this.ruleMode.GoogleDriveRule.EnableManualApproval = this.googleDriveApprovalData.isApproval;
            this.ruleMode.GoogleDriveRule.Users = this.googleDriveApprovalData.users;
            this.ruleMode.GoogleDriveRule.IsSendEmailToOwner = this.googleDriveApprovalData.isSendEmail;
            this.ruleMode.GoogleDriveRule.ManualReviewType = this.googleDriveApprovalData.isApproval ? this.googleDriveApprovalData.manualReviewType : Constants.ReviewType.RecordOwner;
            this.ruleMode.GoogleDriveRule.WorkflowId = this.googleDriveApprovalData.workflowId;
            this.ruleMode.GoogleDriveRule.ExportDataBeforeArchiving = this.googleExportData.isExport;
            this.ruleMode.GoogleDriveRule.EnableExport = this.googleExportData.isExport || this.googleExportData.isExportOnly;

            if (this.googleStorageSettingsRef) {
                let googleSelectedStoragePolicyItem = this.googleStorageSettingsRef.getSelectedStorage();
                this.ruleMode.GoogleDriveRule.StoragePolicyId = googleSelectedStoragePolicyItem.Id;
                this.ruleMode.GoogleDriveRule.StoragePolicyName = googleSelectedStoragePolicyItem.Name;
                this.ruleMode.GoogleDriveRule.MoveToAnotherTierType = this.googleStorageSettingsRef.getTierType();
            }
            if (this.removeAchivedRefForGoogle) {
                let removeArchMaParam = this.removeAchivedRefForGoogle.getRemoveArchParam();
                if (this.state.selectedRuleModuleType != RuleModuleTypes.SOArchiver && removeArchMaParam.IsEnableRetention) {
                    this.ruleMode.GoogleDriveRule.IsEnableRetention = removeArchMaParam.IsEnableRetention;
                    this.ruleMode.GoogleDriveRule.RetentionInfo = removeArchMaParam.RetentionInfo;
                }
            }
            if (this.state.isGoogleMove && !this.googleExportData.isExportOnly) {
                this.ruleMode.GoogleDriveRule.MoveToRecordCenterSettings = null;
                this.ruleMode.GoogleDriveRule.MoveDto = this.moveFuncForGoogle();
            } else {
                this.ruleMode.GoogleDriveRule.MoveDto = null;
            }

            if(this.ruleMode.GoogleDriveRule.EnableExport) {
                this.ruleMode.GoogleDriveRule.ExportInfo.exportType = this.googleExportData.exportTypeValue;
                this.ruleMode.GoogleDriveRule.ExportInfo.exportSPDataOption = this.googleExportData.isExportOnly? this.ExportSPDataOption.ExportWithoutArchive : this.ExportSPDataOption.ExportBeforeArchive;
                this.ruleMode.GoogleDriveRule.ExportInfo.exportLocationId = this.googleExportData.storageId;
                this.ruleMode.GoogleDriveRule.ExportInfo.exportLocationName = this.googleExportData.storageName;

                if (this.googleExportData.isExportOnly && this.googleExportData.exportTypeValue) {
                    this.ruleMode.GoogleDriveRule.StoragePolicyId = this.googleExportData.storageId;
                    this.ruleMode.GoogleDriveRule.StoragePolicyName = this.googleExportData.storageName;
                }
            }
            if (this.state.isArchiveToStorageForGoogle) {
                this.ruleMode.GoogleDriveRule.RuleKeepDataOption = 1024;
            }
        } else {
            this.ruleMode.GoogleDriveRule = null;
        }

        if(this.isConnectorSourceChecked){
            this.ruleMode.IsConnectorSource = true;
            this.ruleMode.ConnectorRule.RuleFilters = this.connectorCriteriaData;
            this.ruleMode.ConnectorRule.EnableManualApproval = this.connectorApprovalData.isApproval;
            this.ruleMode.ConnectorRule.Users = this.connectorApprovalData.users;
            this.ruleMode.ConnectorRule.IsSendEmailToOwner = this.connectorApprovalData.isSendEmail;
            this.ruleMode.ConnectorRule.ManualReviewType = this.connectorApprovalData.isApproval ? this.connectorApprovalData.manualReviewType : Constants.ReviewType.RecordOwner;
            this.ruleMode.ConnectorRule.WorkflowId = this.connectorApprovalData.workflowId;
        } else {
            this.ruleMode.ConnectorRule = null; 
        }

        if (this.isFsSourceChecked) {
            this.ruleMode.IsFSSource = true;
            this.ruleMode.FSRule.EnableManualApproval = this.fsApprovalData.isApproval;
            this.ruleMode.FSRule.ExportDataBeforeArchiving = this.fsExportData.exportTypeValue;
            this.ruleMode.FSRule.EnableExport = this.fsExportData.exportTypeValue;
            this.ruleMode.FSRule.RuleFilters = this.fsCriteriaData;
            this.ruleMode.FSRule.Users = this.fsApprovalData.users;
            this.ruleMode.FSRule.IsSendEmailToOwner = this.fsApprovalData.isSendEmail;
            this.ruleMode.FSRule.ManualReviewType = this.fsApprovalData.isApproval ? this.fsApprovalData.manualReviewType : Constants.ReviewType.RecordOwner;
            this.ruleMode.FSRule.WorkflowId = this.fsApprovalData.workflowId;
            if (this.fsStorageSettingsRef) {
                let fsSelectedStoragePolicyItem = this.fsStorageSettingsRef.getSelectedStorage();
                this.ruleMode.FSRule.StoragePolicyId = fsSelectedStoragePolicyItem.Id;
                this.ruleMode.FSRule.StoragePolicyName = fsSelectedStoragePolicyItem.Name;
                this.ruleMode.FSRule.MoveToAnotherTierType = this.fsStorageSettingsRef.getTierType();
            }
            if (this.state.isFsRemove) {
                isBackupDataConfirmForFS = true;
                if (this.state.isLeaveStubOptionOfFs && this.levelId == 64) {
                    this.ruleMode.FSRule.RuleKeepDataOption |= 128;
                }
                if (this.state.isDeleteRelatedRecordOptionOfFs) {
                    this.ruleMode.FSRule.RelatedRecordOption = 1;
                }
            }
            if (this.state.isArchiveToAzureBlobStorageForFS) {
                this.ruleMode.FSRule.RuleKeepDataOption = 1024;
            }
            if (this.state.isFsMove) {
                this.ruleMode.FSRule.MoveDto = this.ruleMoveOfFsParam;
            }
        }else{
            this.ruleMode.FSRule = null; 
        }
        const isBackupDataConfirm = isBackupDataConfirmForSPO || isBackupDataConfirmForOD || isBackupDataConfirmForFS;
        if (this.ruleId) {
            this.ruleMode.RuleId = this.ruleId;
            if (isBackupDataConfirm && this.enableRecordsArchiver) {
                this.renderPopupWarningBackupData(() => { this.checkContainerCrossSecurityGroup(this.ruleMode); });
            } else if (isArchiveModule && (this.state.isSORemoveForSPO || this.state.isSORemoveForOD)) {
                this.deleteOnlyRuleMessageBox(() => { this.checkContainerCrossSecurityGroup(this.ruleMode); });
            } else {
                this.checkContainerCrossSecurityGroup();
            }
        } else {
            if (isBackupDataConfirm && this.enableRecordsArchiver) {
                this.renderPopupWarningBackupData(() => { this.createRule(this.ruleMode); });
            } else if (isArchiveModule && (this.state.isSORemoveForSPO || this.state.isSORemoveForOD || this.state.isSORemoveForTeams)) {
                this.deleteOnlyRuleMessageBox(() => { this.createRule(this.ruleMode); });
            } else {
                this.createRule(this.ruleMode);
            }
        }
    }

    deleteOnlyRuleMessageBox = (callback) => {
        let args = {
            width: "550px",
            hideActions: false,
            title: RMResx.RM_JS_Common_Confirmation,
            content: RMResx.RM_JS_Rule_DeleteOnly_SaveMessage,
            buttons: [
                {
                    text: RMResx.RM_JS_Common_Cancel, onClick: () => {
                        $$.messagedialog(false);
                    }
                },
                {
                    id: "raDelOnlyRuleOKBtn",
                    text: RMResx.RM_JS_Common_OK,
                    primary: true,
                    classify: "theme",
                    onClick: callback
                }
            ]
        };
        $$.messagedialog(true, args);
    }

    spCovertTagInfo () {
        let tags = [];
        let TagMode = {};
        if (this.state.isTagYes) {
            TagMode = Object.assign({}, this.TagMode);
            TagMode.Type = this.TagType.Archived;
            tags.push(TagMode);
        }
        if (this.state.isTagBy) {
            TagMode = Object.assign({}, this.TagMode);
            TagMode.Type = this.TagType.ArchivedBy;
            tags.push(TagMode);
        }
        if (this.state.isTagTime) {
            TagMode = Object.assign({}, this.TagMode);
            TagMode.Type = this.TagType.ArchivedDate;
            tags.push(TagMode);
        }
        if (this.state.tagMetadataChecked) {
            TagMode = Object.assign({}, this.TagMode);
            let tagMetadata = TagMode;
            tagMetadata.Type = this.state.tagTypeValue;
            tagMetadata.ColumnName = this.state.metadataName;
            if (this.state.tagTypeValue == this.TagType.YesNo) {//boolean
                tagMetadata.Value = this.state.selectTagBoolean.Name;


            } else if (this.state.tagTypeValue == this.TagType.DateTime) {//datetime
                tagMetadata.Value = RM.TimeUtil.getCommonDateStr(this.state.currentDate);

            } else {
                tagMetadata.Value = this.state.metadataValue;

            }
            if(this.state.currentDate){
                tagMetadata.DateTime = this.state.currentDate;
            }
            tagMetadata.TimeZoneId = this.state.currentTimeZone.id;
            tagMetadata.IsDayLightSaving = this.state.currentTimeZone.autoAdjustClock;
            tags.push(tagMetadata);
        }
        if (this.state.retentionActionChecked) {
            TagMode = Object.assign({}, this.TagMode);
            TagMode.Type = this.TagType.RetentionLabel;
            TagMode.Option = this.state.retentionRecordsLabelSelected;
            TagMode.Value = this.state.retentionRecordsLabelSelected === Constants.RetentionLabelOptions.GetFromGeneralSetting  ? "" : this.state.retentionAction;
            tags.push(TagMode);
        }
        return tags;
    }

    spCovertTagInfoForLocal () {
        let tags = [];
        let TagMode = {};
        if (this.state.isTagYesForLocal) {
            TagMode = Object.assign({}, this.TagMode);
            TagMode.Type = this.TagType.Archived;
            tags.push(TagMode);
        }
        if (this.state.isTagByForLocal) {
            TagMode = Object.assign({}, this.TagMode);
            TagMode.Type = this.TagType.ArchivedBy;
            tags.push(TagMode);
        }
        if (this.state.isTagTimeForLocal) {
            TagMode = Object.assign({}, this.TagMode);
            TagMode.Type = this.TagType.ArchivedDate;
            tags.push(TagMode);
        }
        if (this.state.tagMetadataCheckedForLocal) {
            TagMode = Object.assign({}, this.TagMode);
            let tagMetadata = TagMode;
            tagMetadata.Type = this.state.tagTypeValueForLocal;
            tagMetadata.ColumnName = this.state.metadataNameForLocal;
            if (this.state.tagTypeValueForLocal == this.TagType.YesNo) {//boolean
                tagMetadata.Value = this.state.selectTagBooleanForLocal.Name;


            } else if (this.state.tagTypeValueForLocal == this.TagType.DateTime) {//datetime
                tagMetadata.Value = RM.TimeUtil.getCommonDateStr(this.state.currentDateForLocal);

            } else {
                tagMetadata.Value = this.state.metadataValueForLocal;

            }
            if(this.state.currentDateForLocal){
                tagMetadata.DateTime = this.state.currentDateForLocal;
            }
            tagMetadata.TimeZoneId = this.state.currentTimeZoneForLocal.id;
            tagMetadata.IsDayLightSaving = this.state.currentTimeZoneForLocal.autoAdjustClock;
            tags.push(tagMetadata);
        }
        if (this.state.retentionActionCheckedForLocal) {
            TagMode = Object.assign({}, this.TagMode);
            TagMode.Type = this.TagType.RetentionLabel;
            TagMode.Value = this.state.retentionActionForLocal;
            tags.push(TagMode);
        }
        return tags;
    }

    oneDriveCovertTagInfo () {
        let tags = [];
        let TagMode = {};
        if (this.state.isTagYesForOneDrive) {
            TagMode = Object.assign({}, this.TagMode);
            TagMode.Type = this.TagType.Archived;
            tags.push(TagMode);
        }
        if (this.state.isTagByForOneDrive) {
            TagMode = Object.assign({}, this.TagMode);
            TagMode.Type = this.TagType.ArchivedBy;
            tags.push(TagMode);
        }
        if (this.state.isTagTimeForOneDrive) {
            TagMode = Object.assign({}, this.TagMode);
            TagMode.Type = this.TagType.ArchivedDate;
            tags.push(TagMode);
        }
        if (this.state.tagMetadataCheckedForOneDrive) {
            TagMode = Object.assign({}, this.TagMode);
            let tagMetadata = TagMode;
            tagMetadata.Type = this.state.tagTypeValueForOneDrive;
            tagMetadata.ColumnName = this.state.metadataNameForOneDrive;
            if (this.state.tagTypeValueForOneDrive == this.TagType.YesNo) {//boolean
                tagMetadata.Value = this.state.selectTagBooleanForOneDrive.Name;


            } else if (this.state.tagTypeValueForOneDrive == this.TagType.DateTime) {//datetime
                tagMetadata.Value = RM.TimeUtil.getCommonDateStr(this.state.currentDateForOneDrive);

            } else {
                tagMetadata.Value = this.state.metadataValueForOneDrive;

            }
            if(this.state.currentDateForOneDrive){
                tagMetadata.DateTime = this.state.currentDateForOneDrive;
            }
            tagMetadata.TimeZoneId = this.state.currentTimeZoneForOneDrive.id;
            tagMetadata.IsDayLightSaving = this.state.currentTimeZoneForOneDrive.autoAdjustClock;
            tags.push(tagMetadata);
        }
        if (this.state.retentionActionCheckedForOneDrive) {
            TagMode = Object.assign({}, this.TagMode);
            TagMode.Type = this.TagType.RetentionLabel;
            TagMode.Option = this.state.retentionRecordsLabelSelectedForOneDrive;
            TagMode.Value = this.state.retentionRecordsLabelSelectedForOneDrive === Constants.RetentionLabelOptions.GetFromGeneralSetting ? "" : this.state.retentionActionForOneDrive;
            tags.push(TagMode);
        }
        return tags;
    }

    teamsCovertTagInfo () {
        let tags = [];
        let TagMode = {};
        if (this.state.isTagYesForTeams) {
            TagMode = Object.assign({}, this.TagMode);
            TagMode.Type = this.TagType.Archived;
            tags.push(TagMode);
        }
        if (this.state.isTagByForTeams) {
            TagMode = Object.assign({}, this.TagMode);
            TagMode.Type = this.TagType.ArchivedBy;
            tags.push(TagMode);
        }
        if (this.state.isTagTimeForTeams) {
            TagMode = Object.assign({}, this.TagMode);
            TagMode.Type = this.TagType.ArchivedDate;
            tags.push(TagMode);
        }
        if (this.state.tagMetadataCheckedForTeams) {
            TagMode = Object.assign({}, this.TagMode);
            let tagMetadata = TagMode;
            tagMetadata.Type = this.state.tagTypeValueForTeams;
            tagMetadata.ColumnName = this.state.metadataNameForTeams;
            if (this.state.tagTypeValueForTeams == this.TagType.YesNo) {//boolean
                tagMetadata.Value = this.state.selectTagBooleanForTeams.Name;


            } else if (this.state.tagTypeValueForTeams == this.TagType.DateTime) {//datetime
                tagMetadata.Value = RM.TimeUtil.getCommonDateStr(this.state.currentDateForTeams);

            } else {
                tagMetadata.Value = this.state.metadataValueForTeams;

            }
            if(this.state.currentDateForTeams){
                tagMetadata.DateTime = this.state.currentDateForTeams;
            }
            tagMetadata.TimeZoneId = this.state.currentTimeZoneForTeams.id;
            tagMetadata.IsDayLightSaving = this.state.currentTimeZoneForTeams.autoAdjustClock;
            tags.push(tagMetadata);
        }
        if (this.state.retentionActionCheckedForTeams) {
            TagMode = Object.assign({}, this.TagMode);
            TagMode.Type = this.TagType.RetentionLabel;
            TagMode.Value = this.state.retentionActionForTeams;
            tags.push(TagMode);
        }
        return tags;
    }

    exoCovertTagInfo () {
        let tags = [];
        let TagMode = Object.assign({}, this.TagMode);
        TagMode.Type = this.TagType.RetentionLabel;
        TagMode.Value = this.state.exoRetentionAction;
        tags.push(TagMode);
        return tags;
    }

    getRuleListFromDA(ruleMode) {
        $$.loading(true);
        let url = "/api/TermManagementApi/GetArchiverRuleListFromDA";
        if(ruleMode.ModelType === Constants.RuleModuleTypes.Records){
            url = "/api/TermManagementApi/GetRecordsRuleListFromDA"
        }
        $.ajax({
            type: "GET",
            url: url,
            data: [],
            async: true,
            beforeSend: () => {

            },
            complete: () => {

            },
            success: (data) => {
                $$.loading(false);
                let ruleInfos = $.parseJSON(data);  // Fortify Issue Type: JSON Injection; Sink Details: get rule data; Ignore Reason: 前后台对象存在对应关系
                for (let rule of ruleInfos) {
                    if (rule.RuleName == ruleMode.RuleName) {
                        //this.props.onOperated(RuleOperatedType.Created, rule);
                        this.props.onOperated(rule);
                        break;
                    }
                }
            },
            error: function (msg) {

            },
            dataType: "json"
        });
    }

    openErrorMessageBox(msg) {
        let args = {
            classify: "error",
            width: "550px",
            hideActions: false,
            title: RMResx.RM_JS_Common_Confirmation,
            content: msg,
            buttons: [
                { text: RMResx.RM_JS_Common_Close, primary: true, classify: "theme", onClick: this.closeErrorMessageBox },
            ]
        };
        $$.messagedialog(true, args);
    }

    closeErrorMessageBox =()=>{
        $$.messagedialog(false);
    }

    //创建rule
    createRule (ruleMode) {
        $$.loading(true);
        let urlData = "/api/RuleApi/CreateRule";
        let option = {
            url: urlData,
            method: "POST",
            data: ruleMode
        };
        fetchUtility(option).then((res) => {
            //console.log("save rule back: " + new Date());
            if (res == "") {
                //RM.CommStatus.save(RM.CommStatus.CreateSuccess);
                if (this.props.currentRowRuleLevelId) {
                    this.getRuleListFromDA(ruleMode);
                } else {
                    //this.props.onOperated(RuleOperatedType.Created);
                    this.props.onOperated();
                    showToast.success(RMResx.RM_JS_RDM_CreateRule_MessageInfo_Success);
                }
            } else {
                this.showSaveRuleErrorMessage(res);
            }
            $$.loading(false);
            addTelemetryRecord(TelemetryModule.RuleManagement, TelemetryEventType.RuleAdded);
        }).catch((e) => {
            $$.loading(false);
            if (e.status == 403) {
                this.openErrorMessageBox(this.stringFormat(RMResx.RM_JS_RDM_CreateRule_MessageInfo_Faild, RMResx.RM_JS_RDM_NotHasContainerPermission));
            }
            else {
                this.openErrorMessageBox(this.stringFormat(RMResx.RM_JS_RDM_CreateRule_MessageInfo_Faild, e.ExceptionMessage));
            }
        });
    }

    //编辑
    getTerms (datas) {
        if(LicenseHelper.HasOpusILLicense() || LicenseHelper.HasOpusGoogleLicense())
        {
            $$.loading(true);
            let urlData = "/api/RuleApi/GetAssociateTerms";
            let option = {
                url: urlData,
                method: "POST",
                data: datas
            };
            fetchUtility(option).then((res) => {
                let data = JSON.parse(res);
                if (data.HasTerms) {
                    this.setState({
                        termDto: data
                    }, () => {
                        this.editRuleMessageBox(msgBoxContentType.AssociateTerms);
                    });
                } else {
                    this.editData(this.ruleMode);
                }
                $$.loading(false);
            }).catch((e) => {
                $$.loading(false);
            });
        }
        else 
        {
            this.editData(this.ruleMode);
        }
    }

    checkContainerCrossSecurityGroup(){
        $$.loading(true);
        let option = {
            url: `/api/RuleApi/CheckContainerCrossSecurityGroup?oldContainerId=${this.originalContainerId}&newContainerId=${this.ruleMode.ContainerId}&RuleId=${this.ruleMode.RuleId}`,
            method: "GET",
        };
        fetchUtility(option).then((res) => {
            let result = JSON.parse(res);
            if(result.MessageType == RAMessageType.Failed){
                this.editRuleMessageBox(msgBoxContentType.CheckMoveCrossSecurityGroup, result.Extsion1);
            }else{
                this.getTerms([this.ruleMode]);
            }
            $$.loading(false);
        }).catch((e) => {
            $$.loading(false);
        });
    }

    editData (datas) {
        $$.loading(true);
        let urlData = "/api/RuleApi/EditRule";
        let option = {
            url: urlData,
            method: "POST",
            data: datas
        };
        fetchUtility(option).then((res) => {
            if (res == "") {
                //RM.CommStatus.save(RM.CommStatus.EditSuccess);
                //this.props.onOperated(RuleOperatedType.Edited);
                this.props.onOperated();
                showToast.success(RMResx.RM_JS_RDM_EditRule_MessageInfo_Success);
            } else {
                this.showSaveRuleErrorMessage(res);
            }
            $$.loading(false);
            addTelemetryRecord(TelemetryModule.RuleManagement, TelemetryEventType.RuleModified);
        }).catch((e) => {
            $$.loading(false);
            if (e.status == 403) {
                this.openErrorMessageBox(this.stringFormat(RMResx.RM_JS_RDM_EditRule_MessageInfo_Faild, RMResx.RM_JS_RDM_NotHasContainerPermission));
            }
            else {
                this.openErrorMessageBox(this.stringFormat(RMResx.RM_JS_RDM_EditRule_MessageInfo_Faild, e.ExceptionMessage));
            }
        });
    }

    routerToStorageSettings(){
        this.closeErrorMessageBox();
        this.routerTo(RouterUrls.CP_ExportSettings);
    }

    showSaveRuleErrorMessage (res) {
        let [content, message] = ["", this.getSaveRuleErrorMessage(res)];
        content = !message? res : <$g.I18NProvider msg={message}>
            <a className="ra-link-a ra-cursor-pointer"
                onClick={() => this.routerToStorageSettings()}>
                {RMResx.RM_ES_Title}
            </a>
        </$g.I18NProvider>;
        this.openErrorMessageBox(content);
    }

    getSaveRuleErrorMessage(res)
    {
        let message = "";
        if(res.indexOf("RM_RDM_Rule_ConfigureStoragePolicy") > 0)
        {
            message = res.replace("RM_RDM_Rule_ConfigureStoragePolicy", RMResx.RM_RDM_Rule_ConfigureStoragePolicy);
        }
        else if(res.indexOf(RMResx.RM_RDM_Rule_ConfigureExportLocation) > 0)
        {
            let key = RMResx.RM_RDM_Rule_ConfigureExportLocation.replace(RMResx.RM_ES_Title, "{0}");
            message = res.replace(RMResx.RM_RDM_Rule_ConfigureExportLocation, key);
        }
        else if(res.indexOf("RM_JS_SPS_ConfigGlobalSettingFirst") > 0)
        {
            message = res.replace("RM_JS_SPS_ConfigGlobalSettingFirst", RMResx.RM_JS_Common_ValidationSettingMsg);
        }
        return message;
    }

    associateTermsMsgBoxContent(){
        return <div>
            <div> {RMResx.RM_JS_RDM_EditRule_AssociateTerms}</div>{
                this.state.termDto.Terms.length > 0 && this.state.termDto.HasTerms &&
                <div>
                    <div id="rm_rule_Terms" className="margin-block-xs">
                        <div>
                            <span id="rm_term_top" onClick={this.termSwitch}>
                                <span id="rm_rule_title">{RMResx.RM_RDM_Rule_AssociatedTerms}</span>
                                <span className={this.state.isShowTerm ? "ra-rule-term-icon extend-icon" : "ra-rule-term-icon"}></span>
                            </span>
                        </div>
                    </div>
                    {
                        this.state.isShowTerm && <div id="rm_rule_content">
                            {
                                this.state.termDto.Terms.map((item, key) => {
                                    return <div className="ra-term-group" key={key}>
                                        <div className="ra-term-ruleName">
                                            <div className="ra-term-title">{item.TermNames}</div>
                                        </div>
                                    </div>;
                                })
                            }

                        </div>
                    }
                </div>
            }
        </div>;
    }

    getMsgBoxContent(msgBoxType, terms){
        switch (msgBoxType) {
            case msgBoxContentType.AssociateTerms:
                return this.associateTermsMsgBoxContent();
            case msgBoxContentType.CheckMoveCrossSecurityGroup:
                return <div>
                    <div>{RMResx.RM_RDM_CreateRule_DisassociateTermsTip}</div>
                    <React.Fragment>
                        <div className="margin-top-m strong">{RMResx.RM_RDM_CreateRule_DisassociateTerms}</div>
                        <div>{
                            terms.map((item, index)=>{
                                return <div key={index} className="margin-top-s">{item.FullPath}</div>;
                            })}
                        </div>
                    </React.Fragment>
                </div>;
            default:
                break;
        }
    }

    editRuleMessageBox(msgBoxType, terms){
        this.args = {
            width: "550px",
            hideActions: false,
            title: RMResx.RM_JS_Common_Confirmation,
            content: this.getMsgBoxContent(msgBoxType, terms),
            buttons: this.getMsgBoxButtons(msgBoxType)
        };
        $$.messagedialog(true, this.args);
    }

    getMsgBoxButtons(msgBoxType){
        let defaultBtns = [
            { text: RMResx.RM_JS_Common_Cancel, onClick: this.onCancle },
            { text: RMResx.RM_JS_Common_OK, primary: true, classify: "theme", onClick: this.onSure },     
        ];
        let btnWithoutAction = [
            { text: RMResx.RM_JS_Common_OK, primary: true, classify: "theme", onClick: this.onCancle },
        ];
        return msgBoxType == msgBoxContentType.CheckMoveCrossSecurityGroup ? btnWithoutAction : defaultBtns;
    }

    termSwitch () {
        this.setState({
            isShowTerm: !this.state.isShowTerm
        }, () => {
            this.editRuleMessageBox(msgBoxContentType.AssociateTerms);
        });

    }

    //删除点击no
    onCancle () {
        $$.messagedialog(false);
    }

    onSure () {
        $$.messagedialog(false);
        this.editData(this.ruleMode);
    }

    exportLocationFunc() {
        const exportLocationObj = {};
        exportLocationObj.IsSpecifyLocation = this.spExportData.exportLocationOption == ExportLocationOption.SPLibOrFolder;
        if (this.spExportData.exportLocationOption == ExportLocationOption.SPLibOrFolder) {
            exportLocationObj.LocationPath = this.spExportData.locationPath;
        } else {
            exportLocationObj.SPTree = this.spExportData.nodeItem;
            exportLocationObj.SPTreeStr = this.spExportData.nodeItemStr;
        }
        exportLocationObj.NotDeclareMovedData = !this.state.isMoveDeclare;
        exportLocationObj.isKeepClassification = this.state.selectedRuleModuleType === RuleModuleTypes.Records ? this.state.isKeepClassificationSPO : false;
        exportLocationObj.IsKeepFolderStructure = (this.state.selectedRuleModuleType === RuleModuleTypes.SOArchiver &&
            this.enableRecordsArchiver && this.levelId == this.RuleLevel.Folder && this.state.isMove) ? this.state.isKeepFolderStructure : false;
        exportLocationObj.IsMoveAllVersions = this.state.selectedRuleModuleType === RuleModuleTypes.SOArchiver && this.enableRecordsArchiver ? this.state.isMoveVersions : false;
        exportLocationObj.FileNameConflictOption = this.state.currentConflictOptionValue;
        exportLocationObj.FileInherit = this.state.fileInherit;
        return exportLocationObj;
    }

    exportLocationFuncForOneDrive() {
        const exportLocationObj = {};
        exportLocationObj.IsSpecifyLocation = this.oneDriveExportData.exportLocationOption == ExportLocationOption.SPLibOrFolder;
        if (this.oneDriveExportData.exportLocationOption == ExportLocationOption.SPLibOrFolder) {
            exportLocationObj.LocationPath = this.oneDriveExportData.locationPath;
        } else {
            exportLocationObj.SPTree = this.oneDriveExportData.nodeItem;
            exportLocationObj.SPTreeStr = this.oneDriveExportData.nodeItemStr;
        }
        exportLocationObj.NotDeclareMovedData = !this.state.isMoveDeclareForOneDrive;
        exportLocationObj.isKeepClassification = this.state.selectedRuleModuleType === RuleModuleTypes.Records ? this.state.isKeepClassificationForOneDrive : false;
        exportLocationObj.IsKeepFolderStructure = (this.state.selectedRuleModuleType === RuleModuleTypes.SOArchiver &&
            this.enableRecordsArchiver && this.levelId == this.RuleLevel.Folder && this.state.isMoveForOneDrive) ? this.state.isKeepFolderStructureForOneDrive : false;
        exportLocationObj.IsMoveAllVersions = this.state.selectedRuleModuleType === RuleModuleTypes.SOArchiver && this.enableRecordsArchiver ? this.state.isMoveVersionsForOneDrive : false;
        exportLocationObj.FileNameConflictOption = this.state.currentConflictOptionValueForOneDrive;
        return exportLocationObj;
    }

    exportLocationFuncForTeams() {
        const exportLocationObj = {};
        exportLocationObj.IsSpecifyLocation = this.teamsExportData.exportLocationOption == ExportLocationOption.SPLibOrFolder;
        if (this.teamsExportData.exportLocationOption == ExportLocationOption.SPLibOrFolder) {
            exportLocationObj.LocationPath = this.teamsExportData.locationPath;
        } else {
            exportLocationObj.SPTree = this.teamsExportData.nodeItem;
            exportLocationObj.SPTreeStr = this.teamsExportData.nodeItemStr;
        }
        exportLocationObj.NotDeclareMovedData = !this.state.isMoveDeclareForTeams;
        exportLocationObj.isKeepClassification = this.state.selectedRuleModuleType === RuleModuleTypes.Records ? this.state.isKeepClassificationForTeams : false;
        exportLocationObj.IsKeepFolderStructure = false;
        exportLocationObj.IsMoveAllVersions = this.state.selectedRuleModuleType === RuleModuleTypes.SOArchiver && this.enableRecordsArchiver ? this.state.isMoveVersionsForTeams : false;
        exportLocationObj.FileNameConflictOption = this.state.currentConflictOptionValueForTeams;
        // exportLocationObj.FileInherit = this.state.fileInherit;
        return exportLocationObj;
    }

    exportLocationFuncForEXO() {
        const exportLocationObj = {};
        exportLocationObj.IsSpecifyLocation = this.exoExportData.exportLocationOption == ExportLocationOption.SPLibOrFolder;
        if (this.exoExportData.exportLocationOption == ExportLocationOption.SPLibOrFolder) {
            exportLocationObj.LocationPath = this.exoExportData.locationPath;
        } else {
            exportLocationObj.SPTree = this.exoExportData.nodeItem;
            exportLocationObj.SPTreeStr = this.exoExportData.nodeItemStr;
        }
        exportLocationObj.IsDeleteSourceItem = this.state.isExoMoveDeleteSource;
        exportLocationObj.isKeepClassification = this.state.isKeepClassification;
        exportLocationObj.FileNameConflictOption = this.state.exo_currentConflictOptionValue;
        return exportLocationObj;
    }

    //tree data
    moveFunc () {
        let moveObj = {};
        moveObj.IsSpecifyLocation = this.state.isSpecifyLocation;
        if (this.state.isSpecifyLocation) {
            moveObj.LocationPath = this.state.locationPath;
        } else {
            if (this.state.destinationActiveTab) { // Selected Teams tab
                moveObj.SPTree = this.teamsNodeItem;
                moveObj.SPTreeStr = JSON.stringify(this.ruleMoveTeamsTree.getTreeData());
            } else {
                moveObj.SPTree = this.spNodeItem;
                moveObj.SPTreeStr = JSON.stringify(this.ruleMoveTree.getTreeData());
            }
        }
        moveObj.NotDeclareMovedData = !this.state.isMoveDeclare;
        moveObj.isKeepClassification = this.state.selectedRuleModuleType === RuleModuleTypes.Records ? this.state.isKeepClassificationSPO : false;
        moveObj.IsKeepFolderStructure = (this.state.selectedRuleModuleType === RuleModuleTypes.SOArchiver &&
            this.enableRecordsArchiver && this.levelId == this.RuleLevel.Folder && this.state.isMove) ? this.state.isKeepFolderStructure : false;
        moveObj.IsMoveAllVersions = this.state.selectedRuleModuleType === RuleModuleTypes.SOArchiver && this.enableRecordsArchiver ? this.state.isMoveVersions : false;
        moveObj.FileNameConflictOption = this.state.currentConflictOptionValue;
        moveObj.FileInherit = this.state.fileInherit;
        return moveObj;
    }

    moveFuncForLocal () {
        let moveObj = {};
        moveObj.IsSpecifyLocation = this.state.isSpecifyLocationForLocal;
        if (this.state.isSpecifyLocationForLocal) {
            moveObj.LocationPath = this.state.locationPathForLocal;
        } else {
            moveObj.SPTree = this.spLocalNodeItem;
            moveObj.SPTreeStr = JSON.stringify(this.ruleMoveTree.getTreeData());
        }
        moveObj.NotDeclareMovedData = !this.state.isMoveDeclareForLocal;
        moveObj.FileNameConflictOption = this.state.currentConflictOptionValueForLocal;
        moveObj.FileInherit = this.state.fileInheritForLocal;
        return moveObj;
    }

    moveFuncForOneDrive () {
        let moveObj = {};
        moveObj.IsSpecifyLocation = this.state.isSpecifyLocationForOneDrive;
        if (this.state.isSpecifyLocationForOneDrive) {
            moveObj.LocationPath = this.state.locationPathForOneDrive;
        } else {
            if (this.state.destinationActiveTabForOD) { // Selected Teams tab
                moveObj.SPTree = this.teamsNodeItemForOD;
                moveObj.SPTreeStr = JSON.stringify(this.ruleMoveTeamsTreeForOD.getTreeData());
            } else {
                moveObj.SPTree = this.oneDriveNodeItem;
                moveObj.SPTreeStr = JSON.stringify(this.ruleMoveOneDriveTree.getTreeData());
            }
        }
        moveObj.NotDeclareMovedData = !this.state.isMoveDeclareForOneDrive;
        moveObj.isKeepClassification = this.state.selectedRuleModuleType === RuleModuleTypes.Records ? this.state.isKeepClassificationForOneDrive : false;
        moveObj.IsKeepFolderStructure = (this.state.selectedRuleModuleType === RuleModuleTypes.SOArchiver &&
            this.enableRecordsArchiver && this.levelId == this.RuleLevel.Folder && this.state.isMoveForOneDrive) ? this.state.isKeepFolderStructureForOneDrive : false;
        moveObj.IsMoveAllVersions = this.state.selectedRuleModuleType === RuleModuleTypes.SOArchiver && this.enableRecordsArchiver ? this.state.isMoveVersionsForOneDrive : false;
        moveObj.FileNameConflictOption = this.state.currentConflictOptionValueForOneDrive;
        moveObj.FileInherit = this.state.fileInheritForOneDrive;
        return moveObj;
    }

    moveFuncForTeams () {
        let moveObj = {};
        moveObj.IsSpecifyLocation = this.state.isSpecifyLocationForTeams;
        if (this.state.isSpecifyLocationForTeams) {
            moveObj.LocationPath = this.state.locationPathForTeams;
        } else {
            moveObj.SPTree = this.teamsNodeItem;
            moveObj.SPTreeStr = JSON.stringify(this.ruleMoveTeamsTree.getTreeData());
        }
        moveObj.NotDeclareMovedData = !this.state.isMoveDeclareForTeams;
        moveObj.isKeepClassification = this.state.selectedRuleModuleType === RuleModuleTypes.Records ? this.state.isKeepClassificationForTeams : false;
        moveObj.IsKeepFolderStructure = false;
        moveObj.IsMoveAllVersions = this.state.selectedRuleModuleType === RuleModuleTypes.SOArchiver && this.enableRecordsArchiver ? this.state.isMoveVersionsForTeams : false;
        moveObj.FileNameConflictOption = this.state.currentConflictOptionValueForTeams;
        moveObj.FileInherit = this.state.fileInheritForTeams;
        return moveObj;
    }

    moveFuncForGoogle () {
        let moveObj = {};
        moveObj.GoogleTree = this.googleNodeItem;
        moveObj.GoogleTreeStr = JSON.stringify(this.ruleMoveGoogleTree.getTreeData());
        // moveObj.isKeepClassification = false;
        return moveObj;
    }
    //tree data
    moveExoFunc() {
        let moveObj = {};
        moveObj.IsSpecifyLocation = this.state.isExoSpecifyLocation;
        if (this.state.isExoSpecifyLocation) {
            moveObj.LocationPath = this.state.exoLocationPath;
        } else {
            if (this.state.destinationActiveTabForEXO) { // Selected Teams tab
                moveObj.SPTree = this.teamsNodeItemForEXO;
                moveObj.SPTreeStr = JSON.stringify(this.ruleMoveTeamsTreeForEXO.getTreeData());
            } else {
                moveObj.SPTree = this.exoNodeItem;
                moveObj.SPTreeStr = JSON.stringify(this.ruleMoveExoTree.getTreeData());
            }
        }
        moveObj.IsDeleteSourceItem = this.state.isExoMoveDeleteSource;
        moveObj.isKeepClassification = this.state.isKeepClassification;
        moveObj.FileNameConflictOption = this.state.exo_currentConflictOptionValue;
        //moveObj.FileInherit = this.state.fileInherit;

        moveObj.IsMoveToSP = this.state.isExoMoveToSP;
        if (this.state.isExoMoveToSP && this.exoMoveToSPRef) {
            moveObj.MoveToSPDataList = this.exoMoveToSPRef.getMoveToSPDataList();
        } else {
            moveObj.MoveToSPDataList = [];
        }
        return moveObj;
    }
    //tree data
    movePhyFunc () {
        let moveObj = {};
        moveObj.IsSpecifyLocation = false;
        moveObj.PhysicalTreeNode = this.selectedPhyTreeItem;
        moveObj.PhysicalTreeStr = JSON.stringify(this.state.phyTreeData);
        moveObj.FileNameConflictOption = this.state.phy_currentConflictOptionValue;
        if (this.levelId == 16) {
            moveObj.MoveHoldConflictOption = this.state.currentMoveHoldConflictOptionValue;
        }
        return moveObj;
    }
    //回显
    setData (data) {
        this.levelId = data.RuleLevel;
        this.setSelectedSource(data.selectedSourcesIndexs);
        this.setState({ selectedRuleModuleType: data.ModelType });
        this.setArchiveData(data);
        if (data.IsSpSource) {
            this.dispatch("spCriteria", Constants.dispatchAction.setData, data);
            this.dispatch("spApproval", Constants.dispatchAction.setData, data);
            this.dispatch("spExport", Constants.dispatchAction.clearData, this.levelId);
            const isMoveToNewLocationOption = !data.EnableExport && (data.MoveDto && (data.MoveDto.LocationPath || data.MoveDto.SPTree));
            if ((data.RuleKeepDataOption & 2097152) == 2097152 || isMoveToNewLocationOption) {
                this.disableExportOption(Constants.RuleSourceTabIndex.SP);
            }
            if ((data.ModelType != RuleModuleTypes.SOArchiver && data.IsEnableRetention) || data.ModelType == RuleModuleTypes.SOArchiver) {
                this.dispatch("raCrSpRemoveArchivedForSp", Constants.dispatchAction.setData, data);
            }
            if(data.ExportInfo)
            {
                let SPTrees = data.MoveDto ? JSON.parse(data.MoveDto.SPTreeStr) : "";
                if (SPTrees) {
                    for (let item of SPTrees) {
                        if (item.CheckNumber == 1) {
                            this.spNodeItem = item;
                        }
                        if (item.Type == DataSourceType.Teams) {
                            this.setState({
                                destinationActiveTab: 1,
                            });
                        }
                    }
                }
                if(data.ExportInfo.exportSPDataOption == this.ExportSPDataOption.ExportBeforeArchive)
                {
                    this.dispatch("spExport", Constants.dispatchAction.setData, data);
                }
                else if(data.ExportInfo.exportSPDataOption == this.ExportSPDataOption.ExportWithoutArchive)
                {
                    this.dispatch("spExportOnly", Constants.dispatchAction.setData, data);
                    this.disableExportOption(Constants.RuleSourceTabIndex.SP);
                }
            }
        }else{
            this.dispatch("spCriteria", Constants.dispatchAction.clearData, data.RuleLevel, data.ModelType); 
        }

        if (data.SPLocalRule) {
            data.SPLocalRule.RuleLevel = data.RuleLevel;
            this.dispatch("spLocalCriteria", Constants.dispatchAction.setData, data);
            this.dispatch("spLocalApproval", Constants.dispatchAction.setData, data.SPLocalRule);
            this.dispatch("spLocalExport", Constants.dispatchAction.clearData, this.levelId);
            if(data.SPLocalRule.ExportInfo)
            {
                if(data.SPLocalRule.ExportInfo.exportSPDataOption == this.ExportSPDataOption.ExportBeforeArchive)
                {
                    this.dispatch("spLocalExport", Constants.dispatchAction.setData, data.SPLocalRule);
                }
                else if(data.SPLocalRule.ExportInfo.exportSPDataOption == this.ExportSPDataOption.ExportWithoutArchive)
                {
                    // this.dispatch("spLocalExportOnly", Constants.dispatchAction.setData, data.SPLocalRule);
                    this.disableExportOption(Constants.RuleSourceTabIndex.SPLocal);
                }
            }
        }else {
            this.dispatch("spLocalCriteria", Constants.dispatchAction.clearData, data.RuleLevel);
        }

        if (data.OneDriveRule) {
            data.OneDriveRule.RuleLevel = data.RuleLevel;
            this.dispatch("oneDriveCriteria", Constants.dispatchAction.setData, data);
            this.dispatch("oneDriveApproval", Constants.dispatchAction.setData, data.OneDriveRule);
            if ((data.OneDriveRule.ModelType != RuleModuleTypes.SOArchiver && data.OneDriveRule.IsEnableRetention) || data.OneDriveRule.ModelType == RuleModuleTypes.SOArchiver) {
                this.dispatch("raCrRemoveArchivedForOd", Constants.dispatchAction.setData, data.OneDriveRule);
            }
            if(data.OneDriveRule.ExportInfo)
            {
                if(data.OneDriveRule.ExportInfo.exportSPDataOption == this.ExportSPDataOption.ExportBeforeArchive)
                {
                    this.dispatch("oneDriveExport", Constants.dispatchAction.setData, data.OneDriveRule);
                }
                else if(data.OneDriveRule.ExportInfo.exportSPDataOption == this.ExportSPDataOption.ExportWithoutArchive)
                {
                    this.dispatch("oneDriveExportOnly", Constants.dispatchAction.setData, data.OneDriveRule);
                    this.disableExportOption(Constants.RuleSourceTabIndex.OneDrive);
                }
            }
        }else {
            this.dispatch("oneDriveCriteria", Constants.dispatchAction.clearData, data.RuleLevel, data.ModelType); 
        }

        if (data.TeamsRule) {
            data.TeamsRule.RuleLevel = data.RuleLevel;
            this.dispatch("teamsCriteria", Constants.dispatchAction.setData, data);
            this.dispatch("teamsApproval", Constants.dispatchAction.setData, data.TeamsRule);
            this.dispatch("teamsExport", Constants.dispatchAction.clearData, this.levelId);
            if ((data.TeamsRule.ModelType != RuleModuleTypes.SOArchiver && data.TeamsRule.IsEnableRetention) || data.TeamsRule.ModelType == RuleModuleTypes.SOArchiver) {
                this.dispatch("raCrRemoveArchivedForTeams", Constants.dispatchAction.setData, data.TeamsRule);
            }
            if(data.TeamsRule.ExportInfo)
            {
                if(data.TeamsRule.ExportInfo.exportSPDataOption == this.ExportSPDataOption.ExportBeforeArchive)
                {
                    this.dispatch("teamsExport", Constants.dispatchAction.setData, data.TeamsRule);
                }
                else if(data.TeamsRule.ExportInfo.exportSPDataOption == this.ExportSPDataOption.ExportWithoutArchive)
                {
                    this.dispatch("teamsExportOnly", Constants.dispatchAction.setData, data.TeamsRule);
                    this.disableExportOption(Constants.RuleSourceTabIndex.Teams);
                }
            }
        } else {
            this.dispatch("teamsCriteria", Constants.dispatchAction.clearData, data.RuleLevel, data.ModelType);
        }

        if (data.EXORule) {
            data.EXORule.RuleLevel = data.RuleLevel;
            this.dispatch("exoApproval", Constants.dispatchAction.setData, data.EXORule);
            this.dispatch("exoCriteria", Constants.dispatchAction.setData, data);
            if(data.EXORule.ExportInfo)
            {
                if(data.EXORule.ExportInfo.exportSPDataOption == this.ExportSPDataOption.ExportBeforeArchive)
                {
                    this.dispatch("exoExport", Constants.dispatchAction.setData, data.EXORule);
                }
                else if(data.EXORule.ExportInfo.exportSPDataOption == this.ExportSPDataOption.ExportWithoutArchive)
                {
                    this.disableExportOption(Constants.RuleSourceTabIndex.Exchange);
                    this.dispatch("exoExportOnly", Constants.dispatchAction.setData, data.EXORule);
                }
            }
        }else {
            this.dispatch("exoCriteria", Constants.dispatchAction.clearData, data.RuleLevel); 
        }
        if (data.PhysicalRule) {
            data.PhysicalRule.RuleLevel = data.RuleLevel;
            this.dispatch("phyApproval", Constants.dispatchAction.setData, data.PhysicalRule);
            // this.dispatch("phyExport", Constants.dispatchAction.setData, data.PhysicalRule);
            this.dispatch("phyCriteria", Constants.dispatchAction.setData, data);
            if (data.PhysicalRule.IsEnableRetention) {
                this.dispatch("raCrRemoveArchivedForPhy", Constants.dispatchAction.setData, data.PhysicalRule);
            }
        }else {
            this.dispatch("phyCriteria", Constants.dispatchAction.clearData, data.RuleLevel); 
        }
        if (data.FSRule) {
            this.dispatch("fsApproval", Constants.dispatchAction.setData, data.FSRule);
            // this.dispatch("fsExport", Constants.dispatchAction.setData, data.FSRule);
            this.dispatch("fsCriteria", Constants.dispatchAction.setData, data);
        }else {
            this.dispatch("fsCriteria", Constants.dispatchAction.clearData, data.RuleLevel); 
        }
        
        if (data.AzureFileRule) {
            this.dispatch("azureFileCriteria", Constants.dispatchAction.setData, data);
            this.dispatch("azureFileApproval", Constants.dispatchAction.setData, data.AzureFileRule);
        }else {
            this.dispatch("azureFileCriteria", Constants.dispatchAction.clearData, data.RuleLevel); 
        }

        if (data.BoxRule) {
            this.dispatch("boxCriteria", Constants.dispatchAction.setData, data);
            this.dispatch("boxApproval", Constants.dispatchAction.setData, data.BoxRule);
        } else {
            this.dispatch("boxCriteria", Constants.dispatchAction.clearData, data.RuleLevel);
        }

        if (data.GoogleDriveRule) {
            data.GoogleDriveRule.RuleLevel = data.RuleLevel;
            this.dispatch("googleDriveCriteria", Constants.dispatchAction.setData, data);
            this.dispatch("googleDriveApproval", Constants.dispatchAction.setData, data.GoogleDriveRule);
            this.setState({isGControlManualApproval: data.GoogleDriveRule.IsGControlManualApproval ?? false})

            if (data.GoogleDriveRule.IsEnableRetention) {
                this.dispatch("raCrRemoveArchivedForGoogle", Constants.dispatchAction.setData, data.GoogleDriveRule);
            }
            if(data.GoogleDriveRule.ExportInfo) {
                if(data.GoogleDriveRule.ExportInfo.exportSPDataOption == this.ExportSPDataOption.ExportBeforeArchive)
                {
                    this.dispatch("googleExport", Constants.dispatchAction.setData, data.GoogleDriveRule);
                }
                else if(data.GoogleDriveRule.ExportInfo.exportSPDataOption == this.ExportSPDataOption.ExportWithoutArchive)
                {
                    this.disableExportOption(Constants.RuleSourceTabIndex.GoogleDrive);
                    this.dispatch("googleExportOnly", Constants.dispatchAction.setData, data.GoogleDriveRule);
                }
            }
        } else {
            this.dispatch("googleDriveCriteria", Constants.dispatchAction.clearData, data.RuleLevel);
        }

        if (data.ConnectorRule) {
            this.dispatch("connectorCriteria", Constants.dispatchAction.setData, data);
            this.dispatch("connectorApproval", Constants.dispatchAction.setData, data.ConnectorRule);
        }else {
            this.dispatch("connectorCriteria", Constants.dispatchAction.clearData, data.RuleLevel); 
        }
        this.setState({
            ruleItem: data,
            selectedRuleModuleType: data.ModelType 
        });
    }

    setCopyNewData (data) {
        // this.isSpSourceChecked = false;
        // this.isSpLocalSourceChecked = false;
        // this.isOneDriveSourceChecked = false;
        // this.isExoSourceChecked = false;
        // this.isPhySourceChecked = false;
        // this.isFsSourceChecked = false;
        // this.selectLevel = this.state.levels[this.getLevelIndexById(data.RuleLevel)];
        // this.levelId = this.selectLevel.id;
        // let RuleName = this.ruleId ? data.RuleName : "";
        // this.levels = this.levels.slice();
        // setCheckedStatus("id", "Checked", this.levels, this.selectLevel);
        // this.setState({
        //     RuleName: RuleName,
        //     Description: data.Description,
        //     DisposalClass: data.DisposalClass,
        //     selectLevel: this.selectLevel,
        //     levels: this.levels,
        // });

        // let curTabIndex = this.state.CriteriaTabsIndex;
        // if (data.IsSPLocalSource) {
        //     curTabIndex = Constants.RuleSourceTabIndex.SPLocal;
        //     this.isSpLocalSourceChecked = true;
        //     this.setState({
        //         isSpLocalSourceChecked: true,
        //     });
        // }
        // if (data.IsFSSource) {
        //     curTabIndex = Constants.RuleSourceTabIndex.FS;
        //     this.isFsSourceChecked = true;
        //     this.setState({
        //         isFsSourceChecked: true,
        //     });
        // }
        // if (data.IsPhySource) {
        //     curTabIndex = Constants.RuleSourceTabIndex.Physical;
        //     this.isPhySourceChecked = true;
        //     this.setState({
        //         isPhySourceChecked: true,
        //     });
        // }
        // if (data.IsExoSource) {
        //     curTabIndex = Constants.RuleSourceTabIndex.Exchange;
        //     this.isExoSourceChecked = true;
        //     this.setState({
        //         isExoSourceChecked: true,
        //     });
        // }
        // if (data.IsOneDriveSource) {
        //     curTabIndex = Constants.RuleSourceTabIndex.OneDrive;
        //     this.isOneDriveSourceChecked = true;
        //     this.setState({
        //         isOneDriveSourceChecked: true,
        //     });
        // }
        // if (data.IsSpSource) {
        //     curTabIndex = Constants.RuleSourceTabIndex.SP;
        //     this.isSpSourceChecked = true;
        //     this.setState({
        //         isSpSourceChecked: true,
        //     });
        // }

        // let [showExoTab, showPhyTab, showFsTab, showSpLocalTab, showOneDriveTab] = [false, false, false, false, false];
        // if (this.levelId == 64) {
        //     showExoTab = true;
        //     showPhyTab = false;
        //     showFsTab = true;
        //     showSpLocalTab = true;
        //     showOneDriveTab = true;
        // } else if (this.levelId == 8 || this.levelId == 16) {
        //     showExoTab = false;
        //     showPhyTab = true;
        //     showFsTab = false;
        //     showSpLocalTab = false;
        //     showOneDriveTab = false;
        // } else {
        //     showExoTab = false;
        //     showPhyTab = false;
        //     showFsTab = false;
        //     showSpLocalTab = this.levelId == 32;
        //     showOneDriveTab = false;
        // }
        // this.setState({
        //     isShowExotab: showExoTab,
        //     isShowPhytab: showPhyTab,
        //     isShowFstab: showFsTab,
        //     isShowSpLocaltab: showSpLocalTab,
        //     isShowOneDrivetab: showOneDriveTab,
        //     ruleCriteriaTabsIndex: curTabIndex
        // });
    }
    getRealPhysicalLevelId () {
        return this.levelId == 16 ? Constants.phyLevelIds.PhysicalFile : Constants.phyLevelIds.PhysicalBox;
    }
    setArchiveData (data) {
        this.setState({
            isDeclaredFile: data.DeleteRecords,
            isLockRecord: data.LockRecordBeforeDestroy !== false,
        });
        let keepDataOption = data.RuleKeepDataOption;
        let isExportOnly = data.ExportInfo && data.ExportInfo.exportSPDataOption == this.ExportSPDataOption.ExportWithoutArchive;
        const isExportBeforeAction = data.ExportInfo && data.ExportInfo.exportSPDataOption == this.ExportSPDataOption.ExportBeforeArchive;

        let isRemove, isMove;
        if (keepDataOption == 0 || keepDataOption == 256 || keepDataOption == 262400 || keepDataOption == 262144) {
            if (data.EnableExport || data.MoveDto == null) {
                isRemove = true;
                isMove = false;
            } else {
                isRemove = false;
                isMove = true;
            }
        }
        let isLeaveStubOption = ((keepDataOption & 128) == 128 || (keepDataOption & 8192) == 8192) ? true : false;
        let isNotBackup = (keepDataOption & 256) == 256 ? true : false;
        let isArchiveToAzureBlobStorage = (keepDataOption & 1024) == 1024 ? true : false;
        const isIncludeLockedFile = data.IncludeDeleteRecordLabel;
        const isRetentionLabel = (keepDataOption & 262144) == 262144;
        let isArchiveAndLeaveStub = (keepDataOption == 2048 || keepDataOption == 264192) ? true : false;
        let isKeep = (isLeaveStubOption || isNotBackup) ? false : keepDataOption > 0 && keepDataOption < 1024;
        let isSORemoveForSPO = (keepDataOption & 16384) == 16384;
        let isKeepVersionOption = (keepDataOption & 32768) == 32768;
        let isArchiveVersionOption = (keepDataOption & 65536) == 65536;
        const isArchiveOnlyVersionOption = (keepDataOption & 1048576) == 1048576;
        let isKeepVersionAndArchiveForSPO = (keepDataOption & 131072) == 131072;
        let isStoreInM365 = (keepDataOption & 2097152) == 2097152;
    
        let hasTag = keepDataOption % 2 == 1;
        let isDeclare = (keepDataOption & 4) == 4 ? true : false;
        let isUndeclare = (keepDataOption & 512) == 512;
        if (data.ModelType === Constants.RuleModuleTypes.SOArchiver) {
            if (data.IsSpSource) {
                let deleteToRecycleBinForSPO = data.RuleLevel === Constants.RuleLevel.SiteCollection ? data.DeleteSiteCollectionToRecycleBin : data.DeleteToRecycleBin;
                this.setState({
                    isSORemoveForSPO: isSORemoveForSPO,
                    isKeepVersionOption: isKeepVersionOption,
                    isBackupAndRemoveForSPO: (keepDataOption & 4096) == 4096 || (keepDataOption & 8192) == 8192,
                    isLeaveStubOption: (keepDataOption & 8192) == 8192,
                    isArchiveVersionOption: isArchiveVersionOption || isArchiveOnlyVersionOption,
                    isKeepVersionAndArchiveForSPO: isKeepVersionAndArchiveForSPO,
                    isArchivingRecordOption: data.DeleteRecords,
                    isArchiveWithoutDestroy: (keepDataOption & 524288) == 524288, // update later
                    isSODeleteToRecycleBinForSPO: deleteToRecycleBinForSPO
                });
            }
            this.setState({
                isShowLeaveStubOption: data.RuleLevel == 64,
                isShowLeaveStubOptionForOneDrive: data.RuleLevel == 64
            });
        } else {
            if (data.IsSpSource && data.RuleLevel === Constants.RuleLevel.SiteCollection) {
                this.setState({
                    isDeleteToRecycleBinForSPO: data.DeleteSiteCollectionToRecycleBin
                });
            } else if (data.IsSpSource && data.RuleLevel === Constants.RuleLevel.Document) {
                this.setState({
                    isDeleteToRecycleBinForSPO: data.DeleteToRecycleBin
                });
            }
        }
        if(isExportOnly)
        {
            isRemove = false;
            isMove = false;
            isLeaveStubOption = false;
            isNotBackup = false;
            isKeep = false;
            hasTag = false;
            isDeclare = false;
            isUndeclare = false;
            
        }

        if (isExportBeforeAction) {
            isMove = false;
        }

        this.setArchiveActionDisplayByRuleLevel(data.RuleLevel, data.ModelType);
        if (isRemove || isLeaveStubOption || isNotBackup) {
            if (data.RuleLevel == 64) {
                this.setState({
                    isShowLeaveStubOption: true
                });
            } else {
                this.setState({
                    isShowLeaveStubOption: false
                });
            }
            if (isLeaveStubOption) {
                this.setState({
                    isLeaveStubOption: true,
                });
            } else {
                this.setState({
                    isLeaveStubOption: false
                });
            }
            
            this.setState({
                isBackupOption: data.IsSpSource && !isNotBackup
            });
            
            this.setState({
                isRemove: true,
                isRetentionLabel,
            });
        } else {
            this.setState({
                isRemove: false
            });
        }
        let isDeleteRelatedRecordOption = data.RelatedRecordOption == 1;
        if (isRemove || isDeleteRelatedRecordOption) {
            if (data.RuleLevel == 64 || data.RuleLevel == 32) {
                this.setState({
                    isShowDeleteRelatedRecordOption: true
                });
            } else {
                this.setState({
                    isShowDeleteRelatedRecordOption: false
                });
            }
            if (isDeleteRelatedRecordOption) {
                this.setState({
                    isDeleteRelatedRecordOption: true
                });
            }
        }
        if(isArchiveToAzureBlobStorage || isArchiveAndLeaveStub){
            this.setState({
                isArchiveToAzureBlobStorage: true,
                isLeaveStubOption: isArchiveAndLeaveStub,
                isRetentionLabel,
                isArchivingRecordOption: data.DeleteRecords,
            });
        }

        if (isSORemoveForSPO && isKeepVersionOption) {
            this.setState({
                keepVersionValue: data.KeepLatestMajorAndMinorVersion
            });
        }

        if (isSORemoveForSPO && isKeepVersionOption) {
            this.setState({
                keepVersionValue: data.KeepLatestMajorAndMinorVersion
            });
        }

        if (isIncludeLockedFile) {
            this.setState({
                isIncludeLockedFile: isIncludeLockedFile,
            });
        }

        if (isKeepVersionAndArchiveForSPO) {
            this.setState({
                keepVersionAndArchiveValueForSPO: data.KeepLatestMajorAndMinorVersionAndArchiveOthers
            });
        }

        if (isArchiveVersionOption) {
            this.setState({
                archiveVersionValue: data.ArchivedLatestVersion
            });
        }

        if (isArchiveOnlyVersionOption) {
            this.setState({
                archiveVersionValue: data.ArchiverOnlyLastestVersion,
            });
        }

        if(isLeaveStubOption || (isArchiveToAzureBlobStorage || isArchiveAndLeaveStub)){
            let selectedLevelStubSetting = this.state.levelStubSettingListForSPO.filter(item => item.Id === data.StubTemplateId)[0];
            let levelStubSettingList = setCheckedStatus("Id", "Checked", this.state.levelStubSettingListForSPO, selectedLevelStubSetting);
            this.setState({
                selectedLevelStubSettingForSPO: selectedLevelStubSetting || {},
                levelStubSettingListForSPO: RM.deepcopy(levelStubSettingList)
            });
        }
    
        if (isKeep) {
            this.setState({
                isKeep: true
            });
            if ((this.is21VEnv || !this.enableRecordsArchiver) && (data.RuleLevel == 64 || data.RuleLevel == 32)) {
                this.setState({
                    isShowDeclareOption: true
                });
            } else {
                this.setState({
                    isShowDeclareOption: false
                });
            }
            if (this.state.ruleCriteriaTabsIndex == Constants.RuleSourceTabIndex.SP) {
                this.dispatch("spApproval", Constants.dispatchAction.approvalCheckboxDisabledAndChecked, true, false);
            }
        } else {
            this.setState({
                isKeep: false
            });
        }
        if (isDeclare) {
            if (this.state.isShowDeclareOption == true) {
                this.setState({ isDeclare: true });
            } else {
                this.setState({ isDeclare: false });
            }
        } else {
            this.setState({ isDeclare: false });
        }
    
        if (isUndeclare) {
            if (this.state.isShowDeclareOption == true) {
                this.setState({ isUndeclare: true });
            } else {
                this.setState({ isUndeclare: false });
            }
        } else {
            this.setState({ isUndeclare: false });
        }
    
        if (hasTag) {
            this.setState({
                iskeepTag: true,
            });
            for (let tag of data.TagContentInfo) {
                if (tag.Type == this.TagType.Archived) {
                    this.setState({
                        isTagYes: true,
                    });
                } else if (tag.Type == this.TagType.ArchivedBy) {
                    this.setState({
                        isTagBy: true,
                    });
                } else if (tag.Type == this.TagType.ArchivedDate) {
                    this.setState({
                        isTagTime: true,
                        tagTypeValue: this.TagType.ArchivedBy
                    });
                } else if (tag.Type == this.TagType.RetentionLabel) {
                    this.setState({
                        retentionActionChecked: true,
                        retentionRecordsLabelOptions: this.getRetentionRecordsLabelOptions(tag.Option),
                        retentionRecordsLabelSelected: tag.Option,
                        retentionAction: tag.Value
                    });
                } else {
                    this.setState({
                        tagMetadataChecked: true
                    });
                    let tagdata = this.state.tagType[tag.Type - 1];
                    this.setState({
                        slectTagType: tagdata,
                        metadataName: tag.ColumnName
                    });
    
                }
                switch (tag.Type) {
                    case this.TagType.Text:
                    case this.TagType.Nubmer://text & number
                        this.setState({
                            isTagText: true,
                            isTagBoolean: false,
                            isTagDate: false,
                            metadataValue: tag.Value,
                            tagTypeValue: tag.Type
                        });
                        break;
                    case this.TagType.DateTime://DateTime
                        this.setState({
                            isTagText: false,
                            isTagBoolean: false,
                            isTagDate: true,
                            currentDate: new Date(tag.Value),
                            currentTimeZone: RM.TimeUtil.getGlobalTimezoneInfo(),
                            tagTypeValue: tag.Type
                        });
                        break;
                    case this.TagType.YesNo: {//Boolean
                        let yesOrNo = this.state.yesOrNo;
                        let selectedTagBoolean = undefined;
                        if (tag.Value == yesOrNo[0].Name || tag.Value == true) {
                            selectedTagBoolean = yesOrNo[0];
                        } else {
                            selectedTagBoolean = yesOrNo[1];
                        }
                        this.setState({
                            isTagText: false,
                            isTagBoolean: true,
                            isTagDate: false,
                            tagTypeValue: tag.Type,
                            selectTagBoolean: selectedTagBoolean
                        });
                        break;
                    }
                    default:
    
                }
            }
    
        } else {
            this.setState({
                iskeepTag: false
            });
        }
    
        if (isMove) {
            this.setState({ isMove: true, isRetentionLabel });
            this.setMoveData(data);
            if (data.MoveToRecordCenterSettings != null && data.MoveToRecordCenterSettings.DestinationLocation != null) {
                if (data.MoveToRecordCenterSettings.DelaredRecord) {
                    this.setState({
                        isMoveDeclare: !data.MoveToRecordCenterSettings.DelaredRecord
                    });
                } else {
                    this.setState({
                        isMoveDeclare: true
                    });
                }
                this.setState({
                    MoveUrl: data.MoveToRecordCenterSettings.DestinationLocation.Url,
                    MoveUser: data.MoveToRecordCenterSettings.DestinationLocation.UserName,
                    MovePassWord: this.moveLocationPasswordPlaceholder
                });
            }
            if (this.state.ruleCriteriaTabsIndex == Constants.RuleSourceTabIndex.SP) {
                this.dispatch("spApproval", Constants.dispatchAction.approvalCheckboxDisabledAndChecked, true, false);
                this.disableExportOption(Constants.RuleSourceTabIndex.SP);
            }
        } else {
            this.setState({
                isMove: false
            });
        }

        if(isExportOnly)
        {
            this.setState({
                isExportOnly: true,
                isRemove: false,
                isKeep: false, 
                isMove: false,    
                isArchiveWithoutDestroy: false,
            });
            this.dispatch("spApproval", Constants.dispatchAction.approvalCheckboxDisabledAndChecked, true, false);
        }

        if (isStoreInM365) {
            this.setState({ isStoreInM365Archive: true });
            this.dispatch("spApproval", Constants.dispatchAction.approvalCheckboxDisabledAndChecked, true, false);
        } else {
            this.setState({ isStoreInM365Archive: false });
        }

        if (data.EXORule) {
           
            
            let isExportOnly = data.EXORule.ExportInfo && data.EXORule.ExportInfo.exportSPDataOption == this.ExportSPDataOption.ExportWithoutArchive;
            const isExportBeforeAction = data.EXORule.ExportInfo && data.EXORule.ExportInfo.exportSPDataOption == this.ExportSPDataOption.ExportBeforeArchive;
            let keepDataOption = data.EXORule.RuleKeepDataOption;
            let isLeaveStubOption = keepDataOption == 128;
            let isExoKeep = isLeaveStubOption ? false : keepDataOption > 0;
            let isExoRemove = keepDataOption == 0 && !isExportOnly;
            let isExoMove;
            if (data.EXORule.EnableExport || data.EXORule.MoveDto == null) {
                isExoRemove = true;
            } else {
                // isExportOnly = false;
                isLeaveStubOption = false;
                isExoKeep = false;
                isExoRemove = false;
                isExoMove = true;
            }
            // if (data.EXORule.MoveDto != null) {
            //     // isExportOnly = false;
            //     isLeaveStubOption = false;
            //     isExoKeep = false;
            //     isExoRemove = false;
            //     isExoMove = true;
            // }
            if(isExportOnly)
            {
                isExoRemove = false;
                isExoKeep = false;
                isExoMove = false;
                this.setState({
                    isExoRemove: false,
                    isExoKeep: false,
                    isExoExportOnly: true
                });
                this.dispatch("exoApproval", Constants.dispatchAction.approvalCheckboxDisabledAndChecked, true, false);
            }

            if (isExportBeforeAction) {
                isExoMove = false;
            }

            if (isExoRemove) {
                this.setState({
                    isExoRemove: true
                });
            } else {
                this.setState({
                    isExoRemove: false
                });
            }
            if (isExoKeep) {
                this.setState({
                    isExoKeep: true,
                    exoRetentionAction: data.EXORule.TagContentInfo[0].Value
                });
                this.dispatch("exoApproval", Constants.dispatchAction.approvalCheckboxDisabledAndChecked, true, false);
            }
            else {
                this.setState({
                    isExoKeep: false,
                });
            }

            if (isExoMove) {
                this.setState({ isExoMove: true });
                this.setExoMoveData(data.EXORule);
                this.dispatch("exoApproval", Constants.dispatchAction.approvalCheckboxDisabledAndChecked, true, false);
                this.disableExportOption(Constants.RuleSourceTabIndex.Exchange);
            }
        }
        this.setFsArchiverData(data);
        this.setPhysicalArchiverData(data);
        this.setSPLocalArchiverData(data);
        this.setOneDriveArchiverData(data);
        this.setAzureFileArchiverData(data);
        this.setGoogleArchiveData(data);
        this.setTeamsArchiveData(data);
    }

    setSPLocalArchiverData (data)
    {
        if(data.SPLocalRule)
        {
            this.setState({
                isDeclaredFileForLocal: data.SPLocalRule.DeleteRecords
            });
            let keepDataOption = data.SPLocalRule.RuleKeepDataOption;
            let isExportOnly = data.SPLocalRule.ExportInfo && data.SPLocalRule.ExportInfo.exportSPDataOption == this.ExportSPDataOption.ExportWithoutArchive;
    
            let isRemove, isMove;
            if (keepDataOption == 0 || keepDataOption == 256) {
                if (data.SPLocalRule.MoveDto == null) {
                    isRemove = true;
                    isMove = false;
                } else {
                    isRemove = false;
                    isMove = true;
                }
            }
            let isLeaveStubOption = (keepDataOption & 128) == 128 ? true : false;
            let isNotBackup = (keepDataOption & 256) == 256 ? true : false;
            let isKeep = (isLeaveStubOption || isNotBackup) ? false : keepDataOption > 0;
        
            let hasTag = keepDataOption % 2 == 1;
            let isDeclare = (keepDataOption & 4) == 4 ? true : false;
            let isUndeclare = (keepDataOption & 512) == 512;
            if(isExportOnly)
            {
                isRemove = false;
                isMove = false;
                isLeaveStubOption = false;
                isNotBackup = false;
                isKeep = false;
                hasTag = false;
                isDeclare = false;
                isUndeclare = false;
                
            }
            // this.setArchiveActionDisplayByRuleLevel(data.RuleLevel);
            if (isRemove || isLeaveStubOption || isNotBackup) {
                if (data.RuleLevel == 64) {
                    this.setState({
                        isShowLeaveStubOptionForLocal: true
                    });
                } else {
                    this.setState({
                        isShowLeaveStubOptionForLocal: false
                    });
                }
                if (isLeaveStubOption) {
                    this.setState({
                        isLeaveStubOptionForLocal: true,
                        isDeclareLinkFileForLocal: data.SPLocalRule.DeclareLinkFile
                    });
                } else {
                    this.setState({
                        isLeaveStubOptionForLocal: false
                    });
                }
                if (isNotBackup) {
                    this.setState({
                        isBackupOptionForLocal: false
                    });
                } else {
                    this.setState({
                        isBackupOptionForLocal: true
                    });
                }
                this.setState({
                    isRemoveForLocal: true
                });
            } else {
                this.setState({
                    isRemoveForLocal: false
                });
            }
            let isDeleteRelatedRecordOption = data.SPLocalRule.RelatedRecordOption == 1;
            if (isRemove || isDeleteRelatedRecordOption) {
                if (data.RuleLevel == 64 || data.RuleLevel == 32) {
                    this.setState({
                        isShowDeleteRelatedRecordOptionForLocal: true
                    });
                } else {
                    this.setState({
                        isShowDeleteRelatedRecordOptionForLocal: false
                    });
                }

                if (isDeleteRelatedRecordOption) {
                    this.setState({
                        isDeleteRelatedRecordOptionForLocal: true
                    });
                }
            }
            if (isKeep) {
                this.setState({
                    isKeepForLocal: true
                });
                if (data.RuleLevel == 64 || data.RuleLevel == 32) {
                    this.setState({
                        isShowDeclareOptionForLocal: true
                    });
                } else {
                    this.setState({
                        isShowDeclareOptionForLocal: false
                    });
                }
                this.dispatch("spLocalApproval", Constants.dispatchAction.approvalCheckboxDisabledAndChecked, true, false);
            } else {
                this.setState({
                    isKeepForLocal: false
                });
            }
            if (isDeclare) {
                if (this.state.isShowDeclareOptionForLocal == true) {
                    this.setState({ isDeclareForLocal: true });
                } else {
                    this.setState({ isDeclareForLocal: false });
                }
            } else {
                this.setState({ isDeclareForLocal: false });
            }
        
            if (isUndeclare) {
                if (this.state.isShowDeclareOptionForLocal == true) {
                    this.setState({ isUndeclareForLocal: true });
                } else {
                    this.setState({ isUndeclareForLocal: false });
                }
            } else {
                this.setState({ isUndeclareForLocal: false });
            }
        
            if (hasTag) {
                this.setState({
                    iskeepTagForLocal: true,
                });
                for (let tag of data.SPLocalRule.TagContentInfo) {
                    if (tag.Type == this.TagType.Archived) {
                        this.setState({
                            isTagYesForLocal: true,
                        });
                    } else if (tag.Type == this.TagType.ArchivedBy) {
                        this.setState({
                            isTagByForLocal: true,
                        });
                    } else if (tag.Type == this.TagType.ArchivedDate) {
                        this.setState({
                            isTagTimeForLocal: true,
                            tagTypeValueForLocal: this.TagType.ArchivedBy
                        });
                    } else if (tag.Type == this.TagType.RetentionLabel) {
                        this.setState({
                            retentionActionCheckedForLocal: true,
                            retentionActionForLocal: tag.Value
                        });
                    } else {
                        this.setState({
                            tagMetadataCheckedForLocal: true
                        });
                        let tagdata = this.state.tagTypeForLocal[tag.Type - 1];
                        this.setState({
                            slectTagTypeForLocal: tagdata,
                            metadataNameForLocal: tag.ColumnName
                        });
        
                    }
                    switch (tag.Type) {
                        case this.TagType.Text:
                        case this.TagType.Nubmer://text & number
                            this.setState({
                                isTagTextForLocal: true,
                                isTagBooleanForLocal: false,
                                isTagDateForLocal: false,
                                metadataValueForLocal: tag.Value,
                                tagTypeValueForLocal: tag.Type
                            });
                            break;
                        case this.TagType.DateTime://DateTime
                            this.setState({
                                isTagTextForLocal: false,
                                isTagBooleanForLocal: false,
                                isTagDateForLocal: true,
                                currentDateForLocal: new Date(tag.Value),
                                currentTimeZoneForLocal: RM.TimeUtil.getGlobalTimezoneInfo(),
                                tagTypeValueForLocal: tag.Type
                            });
                            break;
                        case this.TagType.YesNo: {//Boolean
                            let yesOrNo = this.state.yesOrNoForLocal;
                            let selectedTagBoolean = undefined;
                            if (tag.Value == yesOrNo[0].Name || tag.Value == true) {
                                selectedTagBoolean = yesOrNo[0];
                            } else {
                                selectedTagBoolean = yesOrNo[1];
                            }
                            this.setState({
                                isTagTextForLocal: false,
                                isTagBooleanForLocal: true,
                                isTagDateForLocal: false,
                                tagTypeValueForLocal: tag.Type,
                                selectTagBooleanForLocal: selectedTagBoolean
                            });
                            break;
                        }
                        default:
        
                    }
                }
        
            } else {
                this.setState({
                    iskeepTagForLocal: false
                });
            }
        
            if (isMove) {
                this.setState({ isMoveForLocal: true });
                this.setMoveDataForLocal(data);
                if (data.SPLocalRule.MoveToRecordCenterSettings != null && data.SPLocalRule.MoveToRecordCenterSettings.DestinationLocation != null) {
                    if (data.SPLocalRule.MoveToRecordCenterSettings.DelaredRecord) {
                        this.setState({
                            isMoveDeclareForLocal: !data.SPLocalRule.MoveToRecordCenterSettings.DelaredRecord
                        });
                    } else {
                        this.setState({
                            isMoveDeclareForLocal: true
                        });
                    }
                    this.setState({
                        MoveUrlForLocal: data.SPLocalRule.MoveToRecordCenterSettings.DestinationLocation.Url,
                        MoveUserForLocal: data.SPLocalRule.MoveToRecordCenterSettings.DestinationLocation.UserName,
                        MovePassWordForLocal: this.moveLocationPasswordPlaceholder
                    });
                }
                this.dispatch("spLocalApproval", Constants.dispatchAction.approvalCheckboxDisabledAndChecked, true, false);
                this.disableExportOption(Constants.RuleSourceTabIndex.SPLocal);
            } else {
                this.setState({
                    isMoveForLocal: false
                });
            }
    
            if(isExportOnly)
            {
                this.setState({
                    isExportOnlyForLocal: true,
                    isRemoveForLocal: false,
                    isKeepForLocal: false, 
                    isMoveForLocal: false,    
                });
                this.dispatch("spLocalApproval", Constants.dispatchAction.approvalCheckboxDisabledAndChecked, true, false);
            }
        }
        
    }

    setOneDriveArchiverData (data) {
        if(data.OneDriveRule)
        {
            this.setState({
                isDeclaredFileForOneDrive: data.OneDriveRule.DeleteRecords,
                isDeleteToRecycleBinForOneDrive: data.OneDriveRule.DeleteToRecycleBin,
                isLockRecordForOneDrive: data.OneDriveRule.LockRecordBeforeDestroy !== false,
            });
            let keepDataOption = data.OneDriveRule.RuleKeepDataOption;
            let isExportOnly = data.OneDriveRule.ExportInfo && data.OneDriveRule.ExportInfo.exportSPDataOption == this.ExportSPDataOption.ExportWithoutArchive;
            const isExportBeforeAction = data.OneDriveRule.ExportInfo && data.OneDriveRule.ExportInfo.exportSPDataOption == this.ExportSPDataOption.ExportBeforeArchive;
    
            let isRemove, isMove;
            if (keepDataOption == 0 || keepDataOption == 256 || keepDataOption == 262400 || keepDataOption == 262144) {
                if (data.OneDriveRule.EnableExport || data.OneDriveRule.MoveDto == null) {
                    isRemove = true;
                    isMove = false;
                } else {
                    isRemove = false;
                    isMove = true;
                }
            }
            let isLeaveStubOption = ((keepDataOption & 128) == 128 || (keepDataOption & 8192)==8192) ? true : false;
            let isNotBackup = (keepDataOption & 256) == 256 ? true : false;
            let isArchiveToAzureBlobStorage = (keepDataOption & 1024) == 1024 ? true : false;
            const isIncludeLockedFileForOneDrive = data.OneDriveRule.IncludeDeleteRecordLabel;
            const isRetentionLabel = (keepDataOption & 262144) == 262144;
            let isArchiveAndLeaveStub = (keepDataOption == 2048 || keepDataOption == 264192) ? true : false;
            let isKeep = (isLeaveStubOption || isNotBackup) ? false : keepDataOption > 0 && keepDataOption < 1024;
            let isSORemoveForOD = (keepDataOption & 16384) == 16384;
            let isKeepVersionOptionForOD = (keepDataOption & 32768) == 32768;
            let isArchiveVersionOptionForOD = (keepDataOption & 65536) == 65536;
        const isArchiveOnlyVersionOptionForOD = (keepDataOption & 1048576) == 1048576;
            let isKeepVersionAndArchiveForOD = (keepDataOption & 131072) == 131072;
        
            let hasTag = keepDataOption % 2 == 1;
            let isDeclare = (keepDataOption & 4) == 4 ? true : false;
            let isUndeclare = (keepDataOption & 512) == 512;
            if(data.ModelType === Constants.RuleModuleTypes.SOArchiver){
                this.setState({
                    isSORemoveForOD: isSORemoveForOD,
                    isKeepVersionOptionForOD: isKeepVersionOptionForOD,
                    isBackupAndRemoveForOD: (keepDataOption & 4096) == 4096 || (keepDataOption & 8192) == 8192,
                    isLeaveStubOptionForOneDrive: (keepDataOption & 8192) == 8192,
                    isShowLeaveStubOptionForOneDrive: data.OneDriveRule.RuleLevel == 64,
                    isArchiveVersionOptionForOD: isArchiveVersionOptionForOD || isArchiveOnlyVersionOptionForOD,
                    isKeepVersionAndArchiveForOD: isKeepVersionAndArchiveForOD,
                    isArchivingRecordOptionForOneDrive: data.OneDriveRule.DeleteRecords,
                    isArchiveWithoutDestroyForOneDrive: (keepDataOption & 524288) == 524288, // update later
                    isSODeleteToRecycleBinForOD: data.OneDriveRule.DeleteToRecycleBin
                });
            }
            if(isExportOnly)
            {
                isRemove = false;
                isMove = false;
                isLeaveStubOption = false;
                isNotBackup = false;
                isKeep = false;
                hasTag = false;
                isDeclare = false;
                isUndeclare = false;
                
            }

            if (isExportBeforeAction) {
                isMove = false;
            }

            if (isRemove || isLeaveStubOption || isNotBackup) {
                if (data.RuleLevel == 64) {
                    this.setState({
                        isShowLeaveStubOptionForOneDrive: true
                    });
                } else {
                    this.setState({
                        isShowLeaveStubOptionForOneDrive: false
                    });
                }
                if (isLeaveStubOption) {
                    this.setState({
                        isLeaveStubOptionForOneDrive: true,
                    });
                } else {
                    this.setState({
                        isLeaveStubOptionForOneDrive: false
                    });
                }
                if (isNotBackup) {
                    this.setState({
                        isBackupOptionForOneDrive: false
                    });
                } else {
                    this.setState({
                        isBackupOptionForOneDrive: true
                    });
                }
                this.setState({
                    isRemoveForOneDrive: true,
                    isRetentionLabelForOneDrive: isRetentionLabel,
                });
            } else {
                this.setState({
                    isRemoveForOneDrive: false
                });
            }
            let isDeleteRelatedRecordOption = data.OneDriveRule.RelatedRecordOption == 1;
            if (isRemove || isDeleteRelatedRecordOption) {
                this.setState({
                    isShowDeleteRelatedRecordOptionForOneDrive: false
                });
                
                if (isDeleteRelatedRecordOption) {
                    this.setState({
                        isDeleteRelatedRecordOptionForOneDrive: true
                    });
                }
            }
            if(isArchiveToAzureBlobStorage || isArchiveAndLeaveStub){
                this.setState({
                    isArchiveToAzureBlobStorageForOneDrive: true,
                    isLeaveStubOptionForOneDrive: isArchiveAndLeaveStub,
                    isRetentionLabelForOneDrive: isRetentionLabel,
                    isArchivingRecordOptionForOneDrive: data.OneDriveRule.DeleteRecords,
                });
            }

            if (isSORemoveForOD && isKeepVersionOptionForOD) {
                this.setState({
                    keepVersionValueForOD: data.OneDriveRule.KeepLatestMajorAndMinorVersion
                });
            }

            if (isIncludeLockedFileForOneDrive) {
                this.setState({
                    isIncludeLockedFileForOneDrive: isIncludeLockedFileForOneDrive,
                });
            }
    
            if (isKeepVersionAndArchiveForOD) {
                this.setState({
                    keepVersionAndArchiveValueForOD: data.OneDriveRule.KeepLatestMajorAndMinorVersionAndArchiveOthers
                });
            }
            if (isArchiveVersionOptionForOD) {
                this.setState({
                    archiveVersionValueForOD: data.OneDriveRule.ArchivedLatestVersion
                });
            }
            if (isArchiveOnlyVersionOptionForOD) {
                this.setState({
                    archiveVersionValueForOD: data.OneDriveRule.ArchiverOnlyLastestVersion,
                });
            }
            if(isLeaveStubOption || (isArchiveToAzureBlobStorage || isArchiveAndLeaveStub)){
                let selectedLevelStubSetting = this.state.levelStubSettingListForOneDrive.filter(item => item.Id === data.OneDriveRule.StubTemplateId)[0];
                let levelStubSettingList = setCheckedStatus("Id", "Checked", this.state.levelStubSettingListForOneDrive, selectedLevelStubSetting);
                this.setState({
                    selectedLevelStubSettingForOneDrive: selectedLevelStubSetting || {},
                    levelStubSettingListForOneDrive: RM.deepcopy(levelStubSettingList)
                });
            }
            if (isKeep) {
                this.setState({
                    isKeepForOneDrive: true
                });
                if ((this.is21VEnv || !this.enableRecordsArchiver) && (data.RuleLevel == 64 || data.RuleLevel == 32)) {
                    this.setState({
                        isShowDeclareOptionForOneDrive: true
                    });
                } else {
                    this.setState({
                        isShowDeclareOptionForOneDrive: false
                    });
                }
                this.dispatch("oneDriveApproval", Constants.dispatchAction.approvalCheckboxDisabledAndChecked, true, false);
            } else {
                this.setState({
                    isKeepForOneDrive: false
                });
            }
            if (isDeclare) {
                if (this.state.isShowDeclareOptionForOneDrive == true) {
                    this.setState({ isDeclareForOneDrive: true });
                } else {
                    this.setState({ isDeclareForOneDrive: false });
                }
            } else {
                this.setState({ isDeclareForOneDrive: false });
            }

            if (isUndeclare) {
                if (this.state.isShowDeclareOptionForOneDrive == true) {
                    this.setState({ isUndeclareForOneDrive: true });
                } else {
                    this.setState({ isUndeclareForOneDrive: false });
                }
            } else {
                this.setState({ isUndeclareForOneDrive: false });
            }

            if (hasTag) {
                this.setState({
                    iskeepTagForOneDrive: true,
                });
                for (let tag of data.OneDriveRule.TagContentInfo) {
                    if (tag.Type == this.TagType.Archived) {
                        this.setState({
                            isTagYesForOneDrive: true,
                        });
                    } else if (tag.Type == this.TagType.ArchivedBy) {
                        this.setState({
                            isTagByForOneDrive: true,
                        });
                    } else if (tag.Type == this.TagType.ArchivedDate) {
                        this.setState({
                            isTagTimeForOneDrive: true,
                            tagTypeValueForOneDrive: this.TagType.ArchivedBy
                        });
                    } else if (tag.Type == this.TagType.RetentionLabel) {
                        this.setState({
                            retentionActionCheckedForOneDrive: true,
                            retentionRecordsLabelOptionsForOneDrive: this.getRetentionRecordsLabelOptions(tag.Option),
                            retentionRecordsLabelSelectedForOneDrive: tag.Option,
                            retentionActionForOneDrive: tag.Value
                        });
                    } else {
                        this.setState({
                            tagMetadataCheckedForOneDrive: true
                        });
                        let tagdata = this.state.tagTypeForOneDrive[tag.Type - 1];
                        this.setState({
                            slectTagTypeForOneDrive: tagdata,
                            metadataNameForOneDrive: tag.ColumnName
                        });
        
                    }
                    switch (tag.Type) {
                        case this.TagType.Text:
                        case this.TagType.Nubmer://text & number
                            this.setState({
                                isTagTextForOneDrive: true,
                                isTagBooleanForOneDrive: false,
                                isTagDateForOneDrive: false,
                                metadataValueForOneDrive: tag.Value,
                                tagTypeValueForOneDrive: tag.Type
                            });
                            break;
                        case this.TagType.DateTime://DateTime
                            this.setState({
                                isTagTextForOneDrive: false,
                                isTagBooleanForOneDrive: false,
                                isTagDateForOneDrive: true,
                                currentDateForOneDrive: new Date(tag.Value),
                                currentTimeZoneForOneDrive: RM.TimeUtil.getGlobalTimezoneInfo(),
                                tagTypeValueForOneDrive: tag.Type
                            });
                            break;
                        case this.TagType.YesNo: {//Boolean
                            let yesOrNo = this.state.yesOrNoForOneDrive;
                            let selectedTagBoolean = undefined;
                            if (tag.Value == yesOrNo[0].Name || tag.Value == true) {
                                selectedTagBoolean = yesOrNo[0];
                            } else {
                                selectedTagBoolean = yesOrNo[1];
                            }
                            this.setState({
                                isTagTextForOneDrive: false,
                                isTagBooleanForOneDrive: true,
                                isTagDateForOneDrive: false,
                                tagTypeValueForOneDrive: tag.Type,
                                selectTagBooleanForOneDrive: selectedTagBoolean
                            });
                            break;
                        }
                        default:
        
                    }
                }
        
            } else {
                this.setState({
                    iskeepTagForOneDrive: false
                });
            }
        
            if (isMove) {
                this.setState({
                    isMoveForOneDrive: true,
                    isRetentionLabelForOneDrive: isRetentionLabel,
                });
                this.setMoveDataForOneDrive(data);
                if (data.OneDriveRule.MoveToRecordCenterSettings != null && data.OneDriveRule.MoveToRecordCenterSettings.DestinationLocation != null) {
                    if (data.OneDriveRule.MoveToRecordCenterSettings.DelaredRecord) {
                        this.setState({
                            isMoveDeclareForOneDrive: !data.OneDriveRule.MoveToRecordCenterSettings.DelaredRecord
                        });
                    } else {
                        this.setState({
                            isMoveDeclareForOneDrive: true
                        });
                    }
                    this.setState({
                        MoveUrlForOneDrive: data.OneDriveRule.MoveToRecordCenterSettings.DestinationLocation.Url,
                        MoveUserForOneDrive: data.OneDriveRule.MoveToRecordCenterSettings.DestinationLocation.UserName,
                        MovePassWordForOneDrive: this.moveLocationPasswordPlaceholder
                    });
                }
                this.dispatch("oneDriveApproval", Constants.dispatchAction.approvalCheckboxDisabledAndChecked, true, false);
                this.disableExportOption(Constants.RuleSourceTabIndex.OneDrive);
            } else {
                this.setState({
                    isMoveForOneDrive: false
                });
            }
    
            if(isExportOnly)
            {
                this.setState({
                    isExportOnlyForOneDrive: true,
                    isRemoveForOneDrive: false,
                    isKeepForOneDrive: false, 
                    isMoveForOneDrive: false, 
                    isBackupOptionForOneDrive: false,
                    isArchiveWithoutDestroyForOneDrive: false,
                });
                this.dispatch("oneDriveApproval", Constants.dispatchAction.approvalCheckboxDisabledAndChecked, true, false);
            }
        }
    }

    setFsArchiverData (data) {
        let fSRuleItem = data.FSRule;
        if (fSRuleItem) {
            let isRemove = true;
            let isMove = false;
            let keepDataOption = fSRuleItem.RuleKeepDataOption;
            let isDeleteRelatedRecordOption = fSRuleItem.RelatedRecordOption == 1;
            let isLeaveStubOption = keepDataOption == 128;
            let destMode = fSRuleItem.MoveDto && fSRuleItem.MoveDto.DestMode;
            let isArchiveToAzureBlobStorage = keepDataOption == 1024;
            if (keepDataOption == 0) {
                if (fSRuleItem.MoveDto != null) {
                    isRemove = false;
                    isMove = true;
                }
                this.setState({
                    isFsRemove: isRemove,
                    isFsMove: isMove
                });
            }
            if (isRemove) {
                this.setState({
                    isLeaveStubOptionOfFs: isLeaveStubOption,
                    isDeleteRelatedRecordOptionOfFs: isDeleteRelatedRecordOption
                });
            }
            if (isArchiveToAzureBlobStorage) {
                this.setState({
                    isArchiveToAzureBlobStorageForFS: true,
                });
            }
            if (isMove) {
                this.dispatch('fsRuleMoveOfFs', Constants.dispatchAction.setData, null, fSRuleItem.MoveDto);
                this.dispatch("fsApproval", Constants.dispatchAction.approvalCheckboxDisabledAndChecked, true, false);
                // this.dispatch("fsExport", Constants.dispatchAction.clearData, this.levelId, true);
            }

        }
    }

    setPhysicalArchiverData (data) {
        this.showRelatedAndRemoveBoxOption(data.RuleLevel);
        if (data.PhysicalRule) {
            let keepDataOption = data.PhysicalRule.RuleKeepDataOption;
            let isRemove, isMove;
            let smallType = this.state.smallNodeType;
            if (keepDataOption == 0) {
                if (data.PhysicalRule.MoveDto == null) {
                    isRemove = true;
                    isMove = false;
                } else {
                    isMove = true;
                    isRemove = false;
                    this.dispatch("phyApproval", Constants.dispatchAction.approvalCheckboxDisabledAndChecked, true, false);
                }
                let isDeleteRelatedRecordOption = data.PhysicalRule.RelatedRecordOption == 1;
                let isDestroyEmptyBoxOnFolderRuleOption = data.PhysicalRule.DestroyEmptyBoxOnFolderRule;
                if (isRemove || isDeleteRelatedRecordOption || isDestroyEmptyBoxOnFolderRuleOption) {
                    this.showRelatedAndRemoveBoxOption(data.RuleLevel);
                    if (isDeleteRelatedRecordOption) {
                        this.setState({
                            isDeleteRelatedRecordOptionOfPhy: true
                        });
                    }
                    if (isDestroyEmptyBoxOnFolderRuleOption) {
                        this.setState({
                            isDestoryEmptyBoxOnFolderRuleOptionOfPhy: true
                        });
                    }
                }

                if (data.PhysicalRule.RuleLevel == 16 || data.PhysicalRule.RuleLevel == 10002) {
                    smallType = NodeType.PhyBox;
                } else {
                    smallType = NodeType.PhysicalBottomLocation;
                }
                this.setState({
                    isPhyRemove: isRemove,
                    isPhyMove: isMove,
                    smallNodeType: smallType,
                });
            }

            if (isMove) {
                this.selectedPhyTreeItem = data.PhysicalRule.MoveDto.PhysicalTreeNode;
                this.setState({
                    phyTreeData: JSON.parse(data.PhysicalRule.MoveDto.PhysicalTreeStr),
                    phy_currentConflictOptionValue: data.PhysicalRule.MoveDto.FileNameConflictOption + "",
                    currentMoveHoldConflictOptionValue: data.PhysicalRule.MoveDto.MoveHoldConflictOption + ""
                });
            } else {
                this.setState({ phyTreeData: null });
            }

            if (data.PhysicalRule.IsCalculationDisposalDate) {
                this.phyCalculateDisposalDateCheckedChange();
            }
        } else {
            this.setState({ phyTreeData: null });
        }
    }

    setAzureFileArchiverData(data){
        let azureFileRuleItem = data.AzureFileRule;
        if (azureFileRuleItem) {
            let keepDataOption = azureFileRuleItem.RuleKeepDataOption;
            let isLeaveStubOption = keepDataOption == 128;
            this.setState({
                isAzureFileRemove: true,
                isLeaveStubOptionForAzureFile: isLeaveStubOption,
            });
        }
    }

    setGoogleArchiveData = (data) => {
        let googleRuleItem = data.GoogleDriveRule;
        let isExportOnly = googleRuleItem && data.GoogleDriveRule.ExportInfo && data.GoogleDriveRule.ExportInfo.exportSPDataOption == this.ExportSPDataOption.ExportWithoutArchive;
        if (googleRuleItem) {
            let isMove = false;
            let keepDataOption = googleRuleItem.RuleKeepDataOption;
            if (keepDataOption == 0) {
                if (googleRuleItem.MoveDto != null) {
                    isMove = true;
                }
            }
            if(isExportOnly) {
                isMove = false;
                this.setState({
                    isGoogleExportOnly: true,
                    isGoogleDriveRemove: false,
                    isGoogleMove: false, 
                    isArchiveToStorageForGoogle: false,
                });
                this.dispatch("googleDriveApproval", Constants.dispatchAction.approvalCheckboxDisabledAndChecked, true, false);
            }
            if (isMove) {
                this.setGoogleMoveData(data)
                this.setState({
                    isGoogleMove: true,
                    isGoogleExportOnly: false,
                    isGoogleDriveRemove: false,
                    isArchiveToStorageForGoogle: false,
                });
                this.dispatch("googleDriveApproval", Constants.dispatchAction.approvalCheckboxDisabledAndChecked, true, false);
                this.disableExportOption(Constants.RuleSourceTabIndex.GoogleDrive);
            } else {
                this.setState({
                    isGoogleMove: false
                });
            }
            if (keepDataOption == 1024) {
                this.setState({
                    isGoogleMove: false,
                    isGoogleExportOnly: false,
                    isGoogleDriveRemove: false,
                    isArchiveToStorageForGoogle: true,
                })
            }
        }
    }

    setTeamsArchiveData = (data) => {
        if (data.TeamsRule) {
            this.setState({
                isDeclaredFileForTeams: data.TeamsRule.DeleteRecords,
                isIncludeLockedFileForTeams: data.TeamsRule.IncludeDeleteRecordLabel,
                isLockRecordBeforeDestroyForTeams: data.TeamsRule.LockRecordBeforeDestroy !== false,
            });
            let keepDataOption = data.TeamsRule.RuleKeepDataOption;
            const isExportOnly = data.TeamsRule.ExportInfo && data.TeamsRule.ExportInfo.exportSPDataOption == this.ExportSPDataOption.ExportWithoutArchive;
            const isExportBeforeAction = data.TeamsRule.ExportInfo && data.TeamsRule.ExportInfo.exportSPDataOption == this.ExportSPDataOption.ExportBeforeArchive;

            let isRemove, isMove;
            if (keepDataOption == 0 || keepDataOption == 256 || keepDataOption == 262400 || keepDataOption == 262144) {
                if (data.TeamsRule.EnableExport || data.TeamsRule.MoveDto == null) {
                    isRemove = true;
                    isMove = false;
                } else {
                    isRemove = false;
                    isMove = true;
                }
            }
            let isLeaveStubOption = ((keepDataOption & 128) == 128 || (keepDataOption & 8192) == 8192) ? true : false;
            let isNotBackup = (keepDataOption & 256) == 256 ? true : false;
            let isArchiveToAzureBlobStorage = (keepDataOption & 1024) == 1024 ? true : false;
            const isRetentionLabelForTeams = (keepDataOption & 262144) == 262144;
            let isArchiveAndLeaveStub = (keepDataOption == 2048 || keepDataOption == 264192) ? true : false;
            let isKeep = (isLeaveStubOption || isNotBackup) ? false : keepDataOption > 0 && keepDataOption < 1024;
            const isSORemoveForTeams = (keepDataOption & 16384) == 16384;
            const isKeepVersionOptionForTeams = (keepDataOption & 32768) == 32768;
            const isArchiveVersionOptionForTeams = (keepDataOption & 65536) == 65536;
            const isKeepVersionAndArchiveForTeams = (keepDataOption & 131072) == 131072;
        
            let hasTag = keepDataOption % 2 == 1;
            let isDeclare = (keepDataOption & 4) == 4 ? true : false;
            let isUndeclare = (keepDataOption & 512) == 512;
            if (data.TeamsRule.ModelType === Constants.RuleModuleTypes.SOArchiver) {
                if (data.IsTeamsSource) {
                    this.setState({
                        isSORemoveForTeams,
                        isKeepVersionOptionForTeams,
                        isBackupAndRemoveForTeams: (keepDataOption & 4096) == 4096 || (keepDataOption & 8192) == 8192,
                        isLeaveStubOptionForTeams: (keepDataOption & 8192) == 8192,
                        isShowLeaveStubOptionForTeams: data.TeamsRule.RuleLevel == 64,
                        isArchiveVersionOptionForTeams,
                        isKeepVersionAndArchiveForTeams: isKeepVersionAndArchiveForTeams,
                        isArchivingRecordOptionForTeams: data.TeamsRule.DeleteRecords,
                    });
                }
                // this.setState({
                //     isShowLeaveStubOptionForTeams: data.RuleLevel == 64,
                // });
            }
            if (isExportOnly) {
                isRemove = false;
                isMove = false;
                isLeaveStubOption = false;
                isNotBackup = false;
                isKeep = false;
                hasTag = false;
                isDeclare = false;
                isUndeclare = false;
            }

            if (isExportBeforeAction) {
                isMove = false;
            }

            // this.setArchiveActionDisplayByRuleLevel(data.TeamsRule.RuleLevel, data.TeamsRule.ModelType);
            if (isRemove || isLeaveStubOption || isNotBackup) {
                if (data.TeamsRule.RuleLevel == 64) {
                    this.setState({
                        isShowLeaveStubOptionForTeams: true
                    });
                } else {
                    this.setState({
                        isShowLeaveStubOptionForTeams: false
                    });
                }
                if (isLeaveStubOption) {
                    this.setState({
                        isLeaveStubOptionForTeams: true,
                    });
                } else {
                    this.setState({
                        isLeaveStubOptionForTeams: false
                    });
                }
                
                this.setState({
                    isBackupOptionForTeams: data.IsTeamsSource && !isNotBackup
                });
                
                this.setState({
                    isRemoveForTeams: true,
                    isRetentionLabelForTeams,
                });
            } else {
                this.setState({
                    isRemoveForTeams: false
                });
            }
            const isDeleteRelatedRecordOptionForTeams = data.TeamsRule.RelatedRecordOption == 1;
            if (isRemove || isDeleteRelatedRecordOptionForTeams) {
                if (data.RuleLevel == 64 || data.RuleLevel == 32) {
                    this.setState({
                        isShowDeleteRelatedRecordOptionForTeams: true
                    });
                } else {
                    this.setState({
                        isShowDeleteRelatedRecordOptionForTeams: false
                    });
                }
                if (isDeleteRelatedRecordOptionForTeams) {
                    this.setState({
                        isDeleteRelatedRecordOptionForTeams: true
                    });
                }
            }
            if (isArchiveToAzureBlobStorage || isArchiveAndLeaveStub) {
                this.setState({
                    isArchiveToAzureBlobStorageForTeams: true,
                    isLeaveStubOptionForTeams: isArchiveAndLeaveStub,
                    isRetentionLabelForTeams,
                    isArchivingRecordOptionForTeams: data.TeamsRule.DeleteRecords,
                });
            }

            if (isSORemoveForTeams && isKeepVersionOptionForTeams) {
                this.setState({
                    keepVersionValueForTeams: data.TeamsRule.KeepLatestMajorAndMinorVersion
                });
            }

            if (isKeepVersionAndArchiveForTeams) {
                this.setState({
                    keepVersionAndArchiveValueForTeams: data.TeamsRule.KeepLatestMajorAndMinorVersionAndArchiveOthers
                });
            }

            if (isArchiveVersionOptionForTeams) {
                this.setState({
                    archiveVersionValueForTeams: data.TeamsRule.ArchivedLatestVersion
                });
            }

            if (isLeaveStubOption || (isArchiveToAzureBlobStorage || isArchiveAndLeaveStub)) {
                const selectedLevelStubSetting = this.state.levelStubSettingListForTeams.filter((item) => item.Id === data.TeamsRule.StubTemplateId)[0];
                const levelStubSettingList = setCheckedStatus("Id", "Checked", this.state.levelStubSettingListForTeams, selectedLevelStubSetting);
                this.setState({
                    selectedLevelStubSettingForTeams: selectedLevelStubSetting || {},
                    levelStubSettingListForTeams: RM.deepcopy(levelStubSettingList)
                });
            }
        
            if (isKeep) {
                this.setState({
                    isKeepForTeams: true
                });
                if (data.TeamsRule.RuleLevel == 64) {
                    this.setState({
                        isShowDeclareOptionForTeams: true
                    });
                } else {
                    this.setState({
                        isShowDeclareOptionForTeams: false
                    });
                }
                if (this.state.ruleCriteriaTabsIndex == Constants.RuleSourceTabIndex.Teams) {
                    this.dispatch("teamsApproval", Constants.dispatchAction.approvalCheckboxDisabledAndChecked, true, false);
                }
            } else {
                this.setState({
                    isKeepForTeams: false
                });
            }
            if (isDeclare) {
                if (this.state.isShowDeclareOptionForTeams == true) {
                    this.setState({ isDeclareForTeams: true });
                } else {
                    this.setState({ isDeclareForTeams: false });
                }
            } else {
                this.setState({ isDeclareForTeams: false });
            }
        
            if (isUndeclare) {
                if (this.state.isShowDeclareOptionForTeams == true) {
                    this.setState({ isUndeclareForTeams: true });
                } else {
                    this.setState({ isUndeclareForTeams: false });
                }
            } else {
                this.setState({ isUndeclareForTeams: false });
            }
        
            if (hasTag) {
                this.setState({
                    iskeepTagForTeams: true,
                });
                for (let tag of data.TeamsRule.TagContentInfo) {
                    if (tag.Type == this.TagType.Archived) {
                        this.setState({
                            isTagYesForTeams: true,
                        });
                    } else if (tag.Type == this.TagType.ArchivedBy) {
                        this.setState({
                            isTagByForTeams: true,
                        });
                    } else if (tag.Type == this.TagType.ArchivedDate) {
                        this.setState({
                            isTagTimeForTeams: true,
                            tagTypeValueForTeams: this.TagType.ArchivedBy
                        });
                    } else if (tag.Type == this.TagType.RetentionLabel) {
                        this.setState({
                            retentionActionCheckedForTeams: true,
                            retentionActionForTeams: tag.Value
                        });
                    } else {
                        this.setState({
                            tagMetadataCheckedForTeams: true
                        });
                        const tagdata = this.state.tagTypeForTeams[tag.Type - 1];
                        this.setState({
                            selectTagTypeForTeams: tagdata,
                            metadataNameForTeams: tag.ColumnName
                        });
        
                    }
                    switch (tag.Type) {
                        case this.TagType.Text:
                        case this.TagType.Nubmer: //text & number
                            this.setState({
                                isTagTextForTeams: true,
                                isTagBooleanForTeams: false,
                                isTagDateForTeams: false,
                                metadataValueForTeams: tag.Value,
                                tagTypeValueForTeams: tag.Type
                            });
                            break;
                        case this.TagType.DateTime: //DateTime
                            this.setState({
                                isTagTextForTeams: false,
                                isTagBooleanForTeams: false,
                                isTagDateForTeams: true,
                                currentDateForTeams: new Date(tag.Value),
                                currentTimeZoneForTeams: RM.TimeUtil.getGlobalTimezoneInfo(),
                                tagTypeValueForTeams: tag.Type
                            });
                            break;
                        case this.TagType.YesNo: { //Boolean
                            const yesOrNo = this.state.yesOrNoForTeams;
                            let selectedTagBoolean = undefined;
                            if (tag.Value == yesOrNo[0].Name || tag.Value == true) {
                                selectedTagBoolean = yesOrNo[0];
                            } else {
                                selectedTagBoolean = yesOrNo[1];
                            }
                            this.setState({
                                isTagTextForTeams: false,
                                isTagBooleanForTeams: true,
                                isTagDateForTeams: false,
                                tagTypeValueForTeams: tag.Type,
                                selectTagBooleanForTeams: selectedTagBoolean
                            });
                            break;
                        }
                        default:
        
                    }
                }
        
            } else {
                this.setState({
                    iskeepTagForTeams: false
                });
            }
        
            if (isMove) {
                this.setState({ isMoveForTeams: true, isRetentionLabelForTeams });
                this.setMoveDataForTeams(data.TeamsRule); // Update later
                if (data.TeamsRule.MoveToRecordCenterSettings != null && data.TeamsRule.MoveToRecordCenterSettings.DestinationLocation != null) {
                    if (data.TeamsRule.MoveToRecordCenterSettings.DelaredRecord) {
                        this.setState({
                            isMoveDeclareForTeams: !data.TeamsRule.MoveToRecordCenterSettings.DelaredRecord
                        });
                    } else {
                        this.setState({
                            isMoveDeclareForTeams: true
                        });
                    }
                    this.setState({
                        MoveUrlForTeams: data.TeamsRule.MoveToRecordCenterSettings.DestinationLocation.Url,
                        MoveUserForTeams: data.TeamsRule.MoveToRecordCenterSettings.DestinationLocation.UserName,
                        MovePassWordForTeams: this.moveLocationPasswordPlaceholder
                    });
                }
                if (this.state.ruleCriteriaTabsIndex == Constants.RuleSourceTabIndex.Teams) {
                    this.dispatch("teamsApproval", Constants.dispatchAction.approvalCheckboxDisabledAndChecked, true, false);
                    this.disableExportOption(Constants.RuleSourceTabIndex.Teams);
                }
            } else {
                this.setState({
                    isMoveForTeams: false
                });
            }

            if(isExportOnly) {
                this.setState({
                    isExportOnlyForTeams: true,
                    isRemoveForTeams: false,
                    isKeepForTeams: false, 
                    isMoveForTeams: false,    
                });
                this.dispatch("teamsApproval", Constants.dispatchAction.approvalCheckboxDisabledAndChecked, true, false);
            } 
        }
    }

    showRelatedAndRemoveBoxOption (ruleLevel) {
        if (ruleLevel == 16) {
            this.setState({
                isShowDeleteRelatedRecordOptionOfPhy: true,
                isShowDestoryEmptyBoxOnFolderRuleOptionOfPhy: true
            });
        } else {
            this.setState({
                isShowDeleteRelatedRecordOptionOfPhy: false,
                isShowDestoryEmptyBoxOnFolderRuleOptionOfPhy: false
            });
        }
    }

    setMoveData (data) {
        if (data.MoveDto) {
            if (typeof (data.MoveDto.NotDeclareMovedData) != "undefined") {
                this.setState({
                    isMoveDeclare: !data.MoveDto.NotDeclareMovedData
                });
            } else {
                this.setState({
                    isMoveDeclare: true
                });
            }
            if (typeof (data.MoveDto.isKeepClassification) != "undefined") {
                this.setState({
                    isKeepClassificationSPO: data.MoveDto.isKeepClassification
                });
            } else {
                this.setState({
                    isKeepClassificationSPO: true
                });
            }
            if (typeof (data.MoveDto.IsKeepFolderStructure) != "undefined") {
                this.setState({
                    isKeepFolderStructure: data.MoveDto.IsKeepFolderStructure
                });
            } else {
                this.setState({
                    isKeepFolderStructure: true
                });
            }
            if (typeof (data.MoveDto.IsMoveAllVersions) != "undefined") {
                this.setState({
                    isMoveVersions: data.MoveDto.IsMoveAllVersions
                });
            } else {
                this.setState({
                    isMoveVersions: false
                });
            }
            this.setState({
                isSpecifyLocation: data.MoveDto.IsSpecifyLocation,
                locationPath: data.MoveDto.LocationPath,
                currentConflictOptionValue: data.MoveDto.FileNameConflictOption + "",
                fileInherit: data.MoveDto.FileInherit
            });
            let SPTrees = JSON.parse(data.MoveDto.SPTreeStr);
            if (SPTrees) {
                for (let item of SPTrees) {
                    if (item.CheckNumber == 1) {
                        if (item.Type == DataSourceType.Teams) {
                            this.teamsNodeItem = item;
                        } else {
                            this.spNodeItem = item;
                        }
                    }
                    if (item.Type == DataSourceType.Teams) {
                        this.setState({
                            destinationActiveTab: 1,
                        });
                    }
                }
            }
            if (!data.MoveDto.IsSpecifyLocation) {
                const state = this.state.destinationActiveTab ? "destinationTreeDataForTeams" : "destinationTreeData";
                this.setState({
                    [state]: SPTrees,
                });
            }
        }
    }

    setMoveDataForLocal (data) {
        if (data.SPLocalRule.MoveDto) {
            if (typeof (data.SPLocalRule.MoveDto.NotDeclareMovedData) != "undefined") {
                this.setState({
                    isMoveDeclareForLocal: !data.SPLocalRule.MoveDto.NotDeclareMovedData
                });
            } else {
                this.setState({
                    isMoveDeclareForLocal: true
                });
            }
            this.setState({
                isSpecifyLocationForLocal: data.SPLocalRule.MoveDto.IsSpecifyLocation,
                locationPathForLocal: data.SPLocalRule.MoveDto.LocationPath,
                currentConflictOptionValueForLocal: data.SPLocalRule.MoveDto.FileNameConflictOption + "",
                fileInheritForLocal: data.SPLocalRule.MoveDto.FileInherit
            });
            let SPTrees = JSON.parse(data.SPLocalRule.MoveDto.SPTreeStr);
            if (SPTrees) {
                for (let item of SPTrees) {
                    if (item.CheckNumber == 1) {
                        this.spLocalNodeItem = item;
                    }
                }
            }
            if (!data.SPLocalRule.MoveDto.IsSpecifyLocation) {
                this.setState({
                    destinationTreeDataForLocal: SPTrees
                });
            }

        }
    }

    setMoveDataForOneDrive (data) {
        if (data.OneDriveRule.MoveDto) {
            if (typeof (data.OneDriveRule.MoveDto.NotDeclareMovedData) != "undefined") {
                this.setState({
                    isMoveDeclareForOneDrive: !data.OneDriveRule.MoveDto.NotDeclareMovedData
                });
            } else {
                this.setState({
                    isMoveDeclareForOneDrive: true
                });
            }
            if (typeof (data.OneDriveRule.MoveDto.isKeepClassification) != "undefined") {
                this.setState({
                    isKeepClassificationForOneDrive: data.OneDriveRule.MoveDto.isKeepClassification
                });
            } else {
                this.setState({
                    isKeepClassificationForOneDrive: true
                });
            }
            if (typeof (data.OneDriveRule.MoveDto.IsKeepFolderStructure) != "undefined") {
                this.setState({
                    isKeepFolderStructureForOneDrive: data.OneDriveRule.MoveDto.IsKeepFolderStructure
                });
            } else {
                this.setState({
                    isKeepFolderStructureForOneDrive: true
                });
            }
            if (typeof (data.OneDriveRule.MoveDto.IsMoveAllVersions) != "undefined") {
                this.setState({
                    isMoveVersionsForOneDrive: data.OneDriveRule.MoveDto.IsMoveAllVersions
                });
            } else {
                this.setState({
                    isMoveVersionsForOneDrive: false
                });
            }
            this.setState({
                isSpecifyLocationForOneDrive: data.OneDriveRule.MoveDto.IsSpecifyLocation,
                locationPathForOneDrive: data.OneDriveRule.MoveDto.LocationPath,
                currentConflictOptionValueForOneDrive: data.OneDriveRule.MoveDto.FileNameConflictOption + "",
                fileInheritForOneDrive: data.OneDriveRule.MoveDto.FileInherit
            });
            let SPTrees = JSON.parse(data.OneDriveRule.MoveDto.SPTreeStr);
            if (SPTrees) {
                for (let item of SPTrees) {
                    if (item.CheckNumber == 1) {
                        if (item.Type == DataSourceType.Teams) {
                            this.teamsNodeItemForOD = item;
                        } else {
                            this.oneDriveNodeItem = item;
                        }
                    }
                    if (item.Type == DataSourceType.Teams) {
                        this.setState({
                            destinationActiveTabForOD: 1,
                        });
                    }
                }
            }
            if (!data.OneDriveRule.MoveDto.IsSpecifyLocation) {
                const state = this.state.destinationActiveTabForOD ? "destinationTreeDataForTeamsOD" : "destinationTreeDataForOneDrive";
                this.setState({
                    [state]: SPTrees,
                });
            }
        }
    }

    setMoveDataForTeams(data) {
        if (data.MoveDto) {
            if (typeof (data.MoveDto.NotDeclareMovedData) != "undefined") {
                this.setState({
                    isMoveDeclareForTeams: !data.MoveDto.NotDeclareMovedData
                });
            } else {
                this.setState({
                    isMoveDeclareForTeams: true
                });
            }
            if (typeof (data.MoveDto.isKeepClassification) != "undefined") {
                this.setState({
                    isKeepClassificationForTeams: data.MoveDto.isKeepClassification
                });
            } else {
                this.setState({
                    isKeepClassificationForTeams: true
                });
            }
            if (typeof (data.MoveDto.IsKeepFolderStructure) != "undefined") {
                this.setState({
                    isKeepFolderStructureForTeams: data.MoveDto.IsKeepFolderStructure
                });
            } else {
                this.setState({
                    isKeepFolderStructureForTeams: true
                });
            }
            if (typeof (data.MoveDto.IsMoveAllVersions) != "undefined") {
                this.setState({
                    isMoveVersionsForTeams: data.MoveDto.IsMoveAllVersions
                });
            } else {
                this.setState({
                    isMoveVersionsForTeams: false
                });
            }
            this.setState({
                isSpecifyLocationForTeams: data.MoveDto.IsSpecifyLocation,
                locationPathForTeams: data.MoveDto.LocationPath,
                currentConflictOptionValueForTeams: data.MoveDto.FileNameConflictOption + "",
                fileInheritForTeams: data.MoveDto.FileInherit
            });
            const SPTrees = JSON.parse(data.MoveDto.SPTreeStr);
            if (SPTrees) {
                for (let item of SPTrees) {
                    if (item.CheckNumber == 1) {
                        this.teamsNodeItem = item;
                    }
                }
            }
            if (!data.MoveDto.IsSpecifyLocation) {
                this.setState({
                    destinationTreeDataForTeams: SPTrees
                });
            }
        }
    }

    setExoMoveData(data) {
        if (data.MoveDto) {
            if (typeof (data.MoveDto.IsDeleteSourceItem) != "undefined") {
                this.setState({
                    isExoMoveDeleteSource: data.MoveDto.IsDeleteSourceItem
                });
            } else {
                this.setState({
                    isExoMoveDeleteSource: true
                });
            }
            if (typeof (data.MoveDto.isKeepClassification) != "undefined") {
                this.setState({
                    isKeepClassification: data.MoveDto.isKeepClassification
                });
            } else {
                this.setState({
                    isKeepClassification: true
                });
            }
            this.setState({
                isExoSpecifyLocation: data.MoveDto.IsSpecifyLocation,
                exoLocationPath: data.MoveDto.LocationPath,
                exo_currentConflictOptionValue: data.MoveDto.FileNameConflictOption + "",
                //fileInherit: data.MoveDto.FileInherit
                isExoMoveToSP: data.MoveDto.IsMoveToSP,
                moveToSPDataList: data.MoveDto.MoveToSPDataList,
            });
            let SPTrees = JSON.parse(data.MoveDto.SPTreeStr);
            if (SPTrees) {
                for (let item of SPTrees) {
                    if (item.CheckNumber == 1) {
                        if (item.Type == DataSourceType.Teams) {
                            this.teamsNodeItemForEXO = item;
                        } else {
                            this.exoNodeItem = item;
                        }
                    }
                    if (item.Type == DataSourceType.Teams) {
                        this.setState({
                            destinationActiveTabForEXO: 1,
                        });
                    }
                }
            }
            if (!data.MoveDto.IsSpecifyLocation) {
                const state = this.state.destinationActiveTabForEXO ? "destinationTreeDataForTeamsEXO" : "exoDestinationTreeData";
                this.setState({
                    [state]: SPTrees,
                });
            }
        }
    }

    setGoogleMoveData = (data) => {
        if (data.GoogleDriveRule.MoveDto) {
            let GoogleTrees = JSON.parse(data.GoogleDriveRule.MoveDto.GoogleTreeStr);
            if (GoogleTrees) {
                for (let item of GoogleTrees) {
                    if (item.CheckNumber == 1) {
                        this.googleNodeItem = item;
                    }
                }
            }
            this.setState({
                destinationTreeDataForGoogle: GoogleTrees
            });
        }
    }

    renderArchiveToAzureBlobStorageForFS() {
        return <div>
            <div id="rm_createRule_backupBeforeDestroying">
                <R.Radio
                    name="ruleActionForFS"
                    text={RMResx.RM_RDM_CreateRule_ArchiveToAzureBlobStorage}
                    disabled={this.state.elementsEnable}
                    checked={this.state.isArchiveToAzureBlobStorageForFS}
                    onChange={this.onChangeArchiveToAzureBlobStorageForFS}
                />
                <$g.Popover>{RMResx.RM_RDM_CreateRule_ArchiveToAzureBlobStorageDes}</$g.Popover>
            </div>
        </div>;
    }

    renderFsRuleAction () {
        let isFsRemove = this.state.isFsRemove;
        let isFsMove = this.state.isFsMove;
        let isFs = this.state.ruleCriteriaTabsIndex == Constants.RuleSourceTabIndex.FS;
        return <div className="rm_createRule_archiveAction" style={{ display: (isFs) ? 'block' : 'none' }}>
            <div className="ra-createRule-question">
                <label className="strong" tabIndex="0">
                    <span>{RMResx.RM_RDM_CreateRule_Title_ExchangeData}</span>
                </label>
                <$g.Popover>{RMResx.RM_JS_Rule_FileSystemDescription}</$g.Popover>
            </div>
            {/*Remove content from File System and destroy*/}
            <div>
                <label>
                    <R.Radio
                        text={RMResx.RM_JS_RDM_CreateRule_Options_ArchiveAndRemove}
                        name="ruleActionForFS"
                        checked={isFsRemove}
                        disabled={this.state.elementsEnable}
                        onChange={this.fsRemoveCheckedChange} />
                </label>
                <$g.Popover>{RMResx.RM_JS_Rule_RemoveActionDesc_FS}</$g.Popover>
            </div>
            {
                isFsRemove && <div>
                    {/*Leave a stub in place for each document following disposal*/}
                    <div id="rm_createRule_leaveStubOption" className="cr-archive-action-children-selection">
                        <label  className='checkbox-label'>
                            <R.Checkbox
                                text={RMResx.RM_JS_RDM_CreateRule_Options_LeaveStub_FS}
                                disabled={this.state.elementsEnable}
                                checked={this.state.isLeaveStubOptionOfFs}
                                onChange={this.onCheckChange.bind(this, "isLeaveStubOptionOfFs")} />
                        </label>
                        <$g.Popover>{RMResx.RM_JS_Rule_LeaveStubDescription_FS}</$g.Popover>
                    </div>
                    {/* Include declared records
                    <div id="rm_createRule_deleteRelatedRecordOption">
                        <label className='checkbox-label'>
                            <input
                                type="checkbox"
                                disabled={this.state.elementsEnable}
                                checked={this.state.isDeleteRelatedRecordOptionOfFs}
                                onChange={this.onCheckChange.bind(this, "isDeleteRelatedRecordOptionOfFs")}
                            />
                            <span>{RMResx.RM_RDM_CreateRule_DeleteRelatedRecord}</span>
                        </label>
                        <$g.Popover>{RMResx.RM_JS_Rule_IncludeRelatedRecordsDescription}</$g.Popover>
                    </div> */}
                </div>
            }
            {/* {Move content to archival storage} */}
            {this.enableRecordsArchiver && this.state.isSeparateArchive && this.renderArchiveToAzureBlobStorageForFS()}

            {/*"Record Declaration and Tagging"*/}
            <div className='rm_createRule_keep'>
                <div>
                    <label>
                        <R.Radio
                            name="ruleActionForFS"
                            text={RMResx.RM_JS_RDM_CreateRule_Options_MoveRecord_FS}
                            disabled={this.state.elementsEnable}
                            checked={isFsMove}
                            onChange={this.fsMoveCheckedChange} />
                        <$g.Popover>{RMResx.RM_JS_Rule_MoveActionDesc_FS}</$g.Popover>
                    </label>
                </div>
                {
                    isFsMove && <div className='ra-createRule-moveTo'>
                        <RuleMove
                            id='fsRuleMoveOfFs'
                        ></RuleMove>
                    </div>
                }
            </div>
        </div>;
    }

    renderAzureFileAction(){
        let isAzureFile = this.state.ruleCriteriaTabsIndex == Constants.RuleSourceTabIndex.AzureFile;
        return <div className="rm_createRule_archiveAction" style={{ display: (isAzureFile) ? 'block' : 'none' }}>
            <div className="ra-createRule-question">
                <label className="strong" tabIndex="0">
                    <span>{RMResx.RM_RDM_CreateRule_Title_ExchangeData}</span>
                </label>
                <$g.Popover>{RMResx.RM_RDM_CreateRule_AzureFileActionDes}</$g.Popover>
            </div>
            <div>
                <R.Radio
                    text={RMResx.RM_JS_RDM_CreateRule_Options_ArchiveAndRemove}
                    name="ruleActionForAzureFile"
                    checked={this.state.isAzureFileRemove}
                    disabled={this.state.elementsEnable}
                    onChange={this.azureFileRemoveCheckedChange} />
            </div>
            <div className="cr-archive-action-children-selection">
                {
                    this.state.isAzureFileRemove &&
                    <div id="rm_createRule_leaveStubOption">
                        <R.Checkbox
                            text={RMResx.RM_RDM_CreateRule_LeaveStubOption}
                            disabled={this.state.elementsEnable}
                            checked={this.state.isLeaveStubOptionForAzureFile}
                            onChange={this.onCheckChange.bind(this, "isLeaveStubOptionForAzureFile")} />
                        <$g.Popover>{RMResx.RM_RDM_CreateRule_AzureFileLeaveStubDes}</$g.Popover>
                    </div>
                }
            </div>
        </div>;
    }

    renderBoxAction() {
        let isBox = this.state.ruleCriteriaTabsIndex == Constants.RuleSourceTabIndex.Box;
        return <div className="rm_createRule_archiveAction" style={{ display: (isBox) ? 'block' : 'none' }}>
            <div className="ra-createRule-question">
                <label className="strong" tabIndex="0">
                    <span>{RMResx.RM_RDM_CreateRule_Title_ExchangeData}</span>
                </label>
                <$g.Popover>{RMResx.RM_RDM_CreateRule_BoxActionDes}</$g.Popover>
            </div>
            <div className="margin-bottom-s">
                <R.Radio
                    text={RMResx.RM_JS_RDM_CreateRule_Options_ArchiveAndRemove}
                    name="ruleActionForBox"
                    checked={this.state.isBoxRemove}
                    disabled={this.state.elementsEnable}
                    onChange={this.boxRemoveCheckedChange} />
            </div>
        </div>;
    }

    renderGoogleAction() {
        let isGoogle = this.state.ruleCriteriaTabsIndex == Constants.RuleSourceTabIndex.GoogleDrive;
        return <div className="rm_createRule_archiveAction" style={{ display: (isGoogle) ? 'block' : 'none' }}>
            <div className="ra-createRule-question">
                <label className="strong" tabIndex="0">
                    <span>{RMResx.RM_RDM_CreateRule_Title_ExchangeData}</span>
                </label>
            </div>
            <div className="margin-bottom-s">
                <div className="margin-top-xs">
                    <label>
                        <R.Radio
                            text={RMResx.RM_JS_RDM_CreateRule_Options_ArchiveAndRemove}
                            name="ruleActionForGoogle"
                            checked={this.state.isGoogleDriveRemove}
                            disabled={this.state.elementsEnable}
                            onChange={this.googleDriveRemoveCheckedChange} />
                    </label>
                    <$g.Popover>{RMResx.RM_RDM_CreateRule_ConnectorRemoveDes_Google}</$g.Popover>
                </div>

                {/* Move content to archival storage */}
                <div>
                    <label>
                        <R.Radio
                            text={RMResx.RM_RDM_CreateRule_ArchiveToAzureBlobStorage}
                            checked={this.state.isArchiveToStorageForGoogle}
                            disabled={this.state.elementsEnable}
                            onChange={this.googleArchiveStorageCheckedChange} />
                    </label>
                    <$g.Popover>{RMResx.RM_RDM_GoogleCreateRule_ArchiveToAzureBlobStorageDes}</$g.Popover>
                </div>

                {/* Export content */}
                <div>
                    <label>
                        <R.Radio
                            text={RMResx.RM_JS_RDM_CreateRule_Options_ExportOnly}
                            disabled={this.state.elementsEnable}
                            checked={this.state.isGoogleExportOnly}
                            onChange={this.googleExportOnlyCheckedChange}
                        />
                    </label>
                    <$g.Popover>{RMResx.RM_JS_Rule_ExportOnlyDesc}</$g.Popover>
                </div>
                <div>
                    {this.state.isGoogleExportOnly && <GoogleExport
                        id='googleExportOnly'
                        type={9}
                        getIsVerificationPassed={this.getGoogleExportIsPassed}
                        getIsVerificationLocationPassed={this.getGoogleExportLocationIsPassed}
                        getExportDate={this.getGoogleExportDate}
                        jumpExportSettings={this.jumpExportSettings}
                        isExportOnly={true}
                        ruleLevel={this.levelId}
                    />}
                </div>

                {/* Move content to new location */}
                <div>
                    <label>
                        <R.Radio
                            text={RMResx.RM_JS_RDM_CreateRule_Options_MoveRecord}
                            disabled={this.state.elementsEnable}
                            checked={this.state.isGoogleMove}
                            onChange={this.googleMoveCheckedChange}
                        />
                    </label>
                    <$g.Popover>{RMResx.RM_JS_Rule_Google_MoveActionDesc}</$g.Popover>
                </div>
                {this.state.isGoogleMove && <div id="rm_createRule_move_container">
                    <div className='ra-tree'>
                        <div className="ra-tree-container">
                            <GoogleDestinationTree
                                ref={r => this.ruleMoveGoogleTree = r}
                                treeData={this.state.destinationTreeDataForGoogle}
                                onSelectedNodeChanged={this.onDestTreeSelectedChangedForGoogle} 
                            />
                        </div>
                        <$g.ValidationMsg show={this.state.noSelectNodeForGoogle}>
                            {RMResx.RM_JS_RDM_CreateRule_Validation_NoSelectTreeNode}
                        </$g.ValidationMsg>
                    </div>
                </div>}
            </div>
        </div>;
    }


    renderConnectorAction(){
        let isConnector = this.state.ruleCriteriaTabsIndex == Constants.RuleSourceTabIndex.Connector;
        return <div className="rm_createRule_archiveAction" style={{ display: (isConnector) ? 'block' : 'none' }}>
            <div className="ra-createRule-question">
                <label className="strong" tabIndex="0">
                    <span>{RMResx.RM_RDM_CreateRule_Title_ExchangeData}</span>
                </label>
            </div>
            <div>
                <R.Radio
                    text={RMResx.RM_JS_RDM_CreateRule_Options_ArchiveAndRemove}
                    name="ruleActionForConnect"
                    checked={this.state.isConnectorRemove}
                    disabled={this.state.elementsEnable}
                    onChange={this.azureFileRemoveCheckedChange} />
                <$g.Popover>{RMResx.RM_RDM_CreateRule_ConnectorRemoveDes}</$g.Popover>
            </div>
        </div>;
    }

    onChangeLeaveStubOptions(attribute, isValidAttr, args) {
        this.setState({
            [attribute]: args.newValue,
            [isValidAttr]: false
        });
    }

    onKeepVersionAndArchiveChangeForSPO = (checked) => {
        this.setState({ isKeepVersionAndArchiveForSPO: checked });
        if (checked) {
            this.setState({
                isArchiveVersionOption: false,
                archiveVersionValue: "0",
                isLeaveStubOption: false,
                selectedLevelStubSettingForSPO: {},
            });
        }
    }

    onKeepVersionAndArchiveChangeForOD = (checked) => {
        this.setState({ isKeepVersionAndArchiveForOD: checked });
        if (checked) {
            this.setState({
                isArchiveVersionOptionForOD: false,
                archiveVersionValueForOD: "0",
                isLeaveStubOptionForOneDrive: false,
                selectedLevelStubSettingForOneDrive: {},
            });
        }
    }

    onKeepVersionAndArchiveChangeForTeams = (checked) => {
        this.setState({ isKeepVersionAndArchiveForTeams: checked });
        if (checked) {
            this.setState({
                isArchiveVersionOptionForTeams: false,
                archiveVersionValueForTeams: "0",
                isLeaveStubOptionForTeams: false,
                selectedLevelStubSettingForTeams: {},
            });
        }
    }

    onCancelStubSettingsPanel = () => {
        this.setState({
            showStubSettingsPanel: false,
        });
    }

    onSaveStubSettings = () => {
        this.dispatch('stubSettingsPanel', 'onSave', (success) => {
            if (success) {
                this.onCancelStubSettingsPanel();
                this.props.onRefetchStubSettingList()
                    .then(() => {
                        const rawList = RM.deepcopy(this.props.levelStubSettingList);
                        const selectedStubSetting = rawList[0];
                        const tabIndex = this.state.ruleCriteriaTabsIndex;
                        
                        // Create updated lists for each tab, preserving existing selections for non-active tabs
                        const existingSelectedForSPO = this.state.selectedLevelStubSettingForSPO;
                        const existingSelectedForOneDrive = this.state.selectedLevelStubSettingForOneDrive;
                        const existingSelectedForTeams = this.state.selectedLevelStubSettingForTeams;
                        
                        // For the active tab, select the newly created stub (first item)
                        // For other tabs, preserve their existing selection if it exists in the new list
                        const getListWithSelection = (existingSelected, isActiveTab) => {
                            const list = RM.deepcopy(rawList);
                            if (isActiveTab) {
                                return setCheckedStatus("Id", "Checked", list, selectedStubSetting);
                            }
                            // Preserve existing selection for non-active tabs
                            if (existingSelected && existingSelected.Id) {
                                const existingInNewList = list.find(item => item.Id === existingSelected.Id);
                                if (existingInNewList) {
                                    return setCheckedStatus("Id", "Checked", list, existingInNewList);
                                }
                            }
                            return list;
                        };
                        
                        const isActiveTabSPO = tabIndex === Constants.RuleSourceTabIndex.SP;
                        const isActiveTabOneDrive = tabIndex === Constants.RuleSourceTabIndex.OneDrive;
                        const isActiveTabTeams = tabIndex === Constants.RuleSourceTabIndex.Teams;
                        
                        this.setState({
                            levelStubSettingListForSPO: getListWithSelection(existingSelectedForSPO, isActiveTabSPO),
                            levelStubSettingListForOneDrive: getListWithSelection(existingSelectedForOneDrive, isActiveTabOneDrive),
                            levelStubSettingListForTeams: getListWithSelection(existingSelectedForTeams, isActiveTabTeams),
                            // Update the selected value and validation state only for the active tab
                            ...(isActiveTabSPO && {
                                selectedLevelStubSettingForSPO: selectedStubSetting,
                                noLeaveStubValueForSPO: false,
                            }),
                            ...(isActiveTabOneDrive && {
                                selectedLevelStubSettingForOneDrive: selectedStubSetting,
                                noLeaveStubValueForOneDrive: false,
                            }),
                            ...(isActiveTabTeams && {
                                selectedLevelStubSettingForTeams: selectedStubSetting,
                                noLeaveStubValueForTeams: false,
                            }),
                        });
                    });
            }
        })
    }

    getLeaveStubSettingOptions(){
        return new Map([
            [
                Constants.RuleSourceTabIndex.SP, 
                {
                    leaveStubSettingOptions: this.state.levelStubSettingListForSPO,
                    selectedLeaveStubSettingAttr: "selectedLevelStubSettingForSPO",
                    isValidForLeaveStub: "noLeaveStubValueForSPO"
                }
            ],
            [
                Constants.RuleSourceTabIndex.OneDrive,
                {
                    leaveStubSettingOptions: this.state.levelStubSettingListForOneDrive,
                    selectedLeaveStubSettingAttr: "selectedLevelStubSettingForOneDrive",
                    isValidForLeaveStub: "noLeaveStubValueForOneDrive"
                }
            ],
            [
                Constants.RuleSourceTabIndex.Teams, 
                {
                    leaveStubSettingOptions: this.state.levelStubSettingListForTeams,
                    selectedLeaveStubSettingAttr: "selectedLevelStubSettingForTeams",
                    isValidForLeaveStub: "noLeaveStubValueForTeams"
                }
            ],
        ]);
    }

    renderLeaveStubSetting(ruleSourceTabIndex) {
        let leaveStubSettingInfo = this.getLeaveStubSettingOptions().get(ruleSourceTabIndex);
        let { leaveStubSettingOptions, selectedLeaveStubSettingAttr, isValidForLeaveStub } = leaveStubSettingInfo;
        return <div className="cr-archive-action-children-selection">
            {LicenseHelper.EnableRecordsArchiver() ? (
                <R.Combobox
                    id="raCrLeaveStubOption"
                    width={"100%"}
                    textField='Name'
                    valueField='Id'
                    checkedField='Checked'
                    items={leaveStubSettingOptions}
                    createNewText={RMResx.RM_JS_Rule_Stub_CreateTemplate_Btn}
                    onChange={this.onChangeLeaveStubOptions.bind(this, selectedLeaveStubSettingAttr, isValidForLeaveStub)}
                    doCreateNew={() => {
                        this.setState({
                            showStubSettingsPanel: true,
                        });
                    }}
                    disabled={this.state.elementsEnable}
                />
            ) : (
                <R.Combobox
                    id="raCrLeaveStubOption"
                    width={"100%"}
                    textField='Name'
                    valueField='Id'
                    checkedField='Checked'
                    items={leaveStubSettingOptions}
                    onChange={this.onChangeLeaveStubOptions.bind(this, selectedLeaveStubSettingAttr, isValidForLeaveStub)}
                    disabled={this.state.elementsEnable}
                />
            )}
            <$g.ValidationMsg show={this.state[leaveStubSettingInfo.isValidForLeaveStub]}>
                {RMResx.RM_JS_RDM_CreateRule_Validation_NoLeaveStubValue}
            </$g.ValidationMsg>
        </div>;
    }

    renderIncludeLockedFileSetting = (stateValue, actionType, lockStateValue = "isLockRecord") => {
       let descriptionMsg = RMResx.RM_RDM_CreateRule_RecordsLabelOptionDesc

        if (actionType === ruleActionType.ArchiveToAzureBlobStorage) {
            descriptionMsg = RMResx.RM_RDM_CreateRule_ArchiveToAzureBlob_RecordsLabelOptionDesc;
        }
        if (actionType === ruleActionType.Remove) {
            descriptionMsg = RMResx.RM_RDM_CreateRule_ArchiveContent_Tooltip
        }

        return (
            <div id="rm_createRule_locked_file_by_records_label" style={{ margin: "3px 0" }}>
                <R.Checkbox
                    text={RMResx.RM_RDM_CreateRule_RecordsLabelOption}
                    disabled={this.state.elementsEnable}
                    checked={this.state[stateValue]}
                    onChange={this.onCheckChange.bind(this, stateValue)} />
                <$g.Popover>{descriptionMsg}</$g.Popover>
                {this.state[stateValue] && (
                    <div className="cr-archive-action-children-selection">
                        <R.Checkbox
                            text={RMResx.RM_RDM_CreateRule_LockRecordBeforeDestroy}
                            disabled={this.state.elementsEnable}
                            checked={this.state[lockStateValue]}
                            onChange={this.onCheckChange.bind(this, lockStateValue)}
                        />
                    </div>
                )}
            </div>
        );
    }

    renderSensitiveAndRententionLabelsSetting = (stateValue) => {
        // this.levelId: 32 is Item and 64 is Document/Email
        return (
            <div id="rm_crateRule_retention_sensitive_label" style={{ margin: "3px 0" }}>
                <R.Checkbox
                    text={RMResx.RM_RDM_CreateRule_Options_IncludeRetentionLabels}
                    disabled={this.state.elementsEnable}
                    checked={this.state[stateValue]}
                    onChange={this.onCheckChange.bind(this, stateValue)} />
                <$g.Popover>{RMResx.RM_JS_Rule_RemoveRetentionLabelDescription}</$g.Popover>
            </div>
        )
    }

    renderArchiveVersionSettingForTeams = () => {
        return <div className="cr-archive-action-children-selection">
            <R.Input
                id="raKeepVersionNumber"
                type="number"
                width="50%"
                onChange={this.onChangeArchiveVersionForTeams}
                value={this.state.archiveVersionValueForTeams}
            />
            <$g.ValidationMsg show={this.state.noArchiveVersionValueForTeams}>
                {RMResx.RM_JS_RDM_CreateRule_Validation_ConditionNoValue}
            </$g.ValidationMsg>
            <$g.ValidationMsg show={this.state.archiveVersionValueInvalidForTeams}>
                {RMResx.RM_JS_RDM_NotNumber}
            </$g.ValidationMsg>
        </div>;
    }

    renderArchiveVersionSetting = () => {
        return <div className="cr-archive-action-children-selection">
            <R.Input
                id="raKeepVersionNumber"
                type="number"
                width="50%"
                onChange={this.onChangeArchiveVersion}
                value={this.state.archiveVersionValue}
            />
            <$g.ValidationMsg show={this.state.noArchiveVersionValueForSPO}>
                {RMResx.RM_JS_RDM_CreateRule_Validation_ConditionNoValue}
            </$g.ValidationMsg>
            <$g.ValidationMsg show={this.state.archiveVersionValueInvalidForSPO}>
                {RMResx.RM_JS_RDM_NotNumber}
            </$g.ValidationMsg>
        </div>;
    }

    renderArchiveVersionSettingForOD = () => {
        return <div className="cr-archive-action-children-selection">
            <R.Input
                id="raKeepVersionNumber"
                type="number"
                width="50%"
                onChange={this.onChangeArchiveVersionForOD}
                value={this.state.archiveVersionValueForOD}
            />
            <$g.ValidationMsg show={this.state.noArchiveVersionValueForOD}>
                {RMResx.RM_JS_RDM_CreateRule_Validation_ConditionNoValue}
            </$g.ValidationMsg>
            <$g.ValidationMsg show={this.state.archiveVersionValueInvalidForOD}>
                {RMResx.RM_JS_RDM_NotNumber}
            </$g.ValidationMsg>
        </div>;
    }

    renderKeepVersionAndArchiveSettingForTeams = () => {
        return <div className="cr-archive-action-children-selection margin-top-s">
            <R.Input
                id="raKeepVersionAndArchiveNumber"
                type="number"
                width="50%"
                onChange={this.onChangeKeepVersionAndArchiveForTeams}
                value={this.state.keepVersionAndArchiveValueForTeams}
            />
            <$g.ValidationMsg show={this.state.noKeepVersionAndArchiveValueForTeams}>
                {RMResx.RM_JS_RDM_CreateRule_Validation_ConditionNoValue}
            </$g.ValidationMsg>
            <$g.ValidationMsg show={this.state.keepVersionAndArchiveValueInvalidForTeams}>
                {RMResx.RM_JS_RDM_NotNumber}
            </$g.ValidationMsg>
        </div>;
    }

    renderKeepVersionAndArchiveSetting = () => {
        return <div className="cr-archive-action-children-selection margin-top-s">
            <R.Input
                id="raKeepVersionAndArchiveNumber"
                type="number"
                width="50%"
                onChange={this.onChangeKeepVersionAndArchive}
                value={this.state.keepVersionAndArchiveValueForSPO}
            />
            <$g.ValidationMsg show={this.state.noKeepVersionAndArchiveValueForSPO}>
                {RMResx.RM_JS_RDM_CreateRule_Validation_ConditionNoValue}
            </$g.ValidationMsg>
            <$g.ValidationMsg show={this.state.keepVersionAndArchiveValueInvalidForSPO}>
                {RMResx.RM_JS_RDM_NotNumber}
            </$g.ValidationMsg>
        </div>;
    }

    renderKeepVersionAndArchiveSettingForOD = () => {
        return <div className="cr-archive-action-children-selection margin-top-s">
            <R.Input
                id="raKeepVersionAndArchiveNumber"
                type="number"
                width="50%"
                onChange={this.onChangeKeepVersionAndArchiveForOD}
                value={this.state.keepVersionAndArchiveValueForOD}
            />
            <$g.ValidationMsg show={this.state.noKeepVersionAndArchiveValueForOD}>
                {RMResx.RM_JS_RDM_CreateRule_Validation_ConditionNoValue}
            </$g.ValidationMsg>
            <$g.ValidationMsg show={this.state.keepVersionAndArchiveValueInvalidForOD}>
                {RMResx.RM_JS_RDM_NotNumber}
            </$g.ValidationMsg>
        </div>;
    }

    renderLeaveStubOptionForSPO(actionType){
        let isRemove = actionType == ruleActionType.Remove;
        let showLeaveStubOptionOption = isRemove ? true : this.state.isArchiveToAzureBlobStorage;
        let leaveStubOptionOptionClassName = isRemove ? "" : "rm-createRule-leaveStubMessage-container";
        let enableKeepVersionAndArchiveOther = RM.gData.enableArchiverVersionNotIncludeLatest && this.state.selectedRuleModuleType === RuleModuleTypes.SOArchiver && this.levelId == this.RuleLevel.Document;
        let enableArchiverLatestVersion = RM.gData.enableArchiverLatestVersion && this.state.selectedRuleModuleType === RuleModuleTypes.SOArchiver && this.levelId == this.RuleLevel.Document;
        let levelIds = [Constants.RuleLevel.Document, Constants.RuleLevel.Item, Constants.RuleLevel.Folder, Constants.RuleLevel.List, Constants.RuleLevel.Site, Constants.RuleLevel.SiteCollection];
        let enableArchivingRecords = this.state.selectedRuleModuleType === RuleModuleTypes.SOArchiver && levelIds.includes(this.levelId);
        const enableLockByRecordsLabelOption = this.state.selectedRuleModuleType === RuleModuleTypes.Records && this.state.isRemove ? false : this.state.isBackupAndRemoveForSPO || this.state.isArchiveToAzureBlobStorage;
        const isSiteCollectionLevel = this.levelId === Constants.RuleLevel.SiteCollection;
        if(showLeaveStubOptionOption){
            return <div className={leaveStubOptionOptionClassName}>
                {enableKeepVersionAndArchiveOther && <div>
                    <R.Checkbox
                        text={RMResx.RM_JS_Rule_KeepVersionAndArchiveOther}
                        disabled={this.state.elementsEnable}
                        checked={this.state.isKeepVersionAndArchiveForSPO}
                        onChange={this.onKeepVersionAndArchiveChangeForSPO}
                    />
                    {this.state.isKeepVersionAndArchiveForSPO && this.renderKeepVersionAndArchiveSetting()}
                </div>}
                {enableArchiverLatestVersion && <div>
                    <R.Checkbox
                        text={RMResx.RM_JS_Rule_ArchiveVersionAndDestroyFile}
                        disabled={this.state.elementsEnable || this.state.isKeepVersionAndArchiveForSPO}
                        checked={this.state.isArchiveVersionOption}
                        onChange={this.onCheckChange.bind(this, "isArchiveVersionOption")}
                    />
                    <$g.Popover>{RMResx.RM_JS_Rule_ArchiveVersion_Message}</$g.Popover>
                    {this.state.isArchiveVersionOption && this.renderArchiveVersionSetting()}
                </div>}
                {
                    this.state.isShowLeaveStubOption &&
                    <div id="rm_createRule_leaveStubOption">
                        <R.Checkbox
                            text={RMResx.RM_RDM_CreateRule_LeaveStubOption}
                            disabled={this.state.elementsEnable || this.state.isKeepVersionAndArchiveForSPO}
                            checked={this.state.isLeaveStubOption}
                            onChange={this.onCheckChange.bind(this, "isLeaveStubOption")} />
                        <$g.Popover>
                            <$g.I18NProvider msg={RMResx.RM_JS_Rule_LeaveStubDescription}>
                                <a className="ra-link-a" href="/Root/CP/StubSettings">{RMResx.RM_AR_CP_StubSettings}</a>
                            </$g.I18NProvider>
                        </$g.Popover>
                    </div>
                }
                { this.state.isLeaveStubOption && this.renderLeaveStubSetting(Constants.RuleSourceTabIndex.SP)}
                {!isRemove && this.enableRecordsArchiver && RM.gData.enviromentName != Enviroments.ChinaNorth && this.renderSensitiveAndRententionLabelsSetting("isRetentionLabel")}
                {/* Archiving records */}
                {(enableArchivingRecords || this.state.isArchiveToAzureBlobStorage) && (
                    <>
                        <div id="rm_createRule_archivingRecordsOption" style={{ margin: "3px 0" }}>
                            <label>
                                <R.Checkbox
                                    text={RMResx.RM_RDM_CreateRule_ArchivingRecordsOption}
                                    disabled={this.state.elementsEnable}
                                    checked={this.state.isArchivingRecordOption}
                                    onChange={this.onCheckChange.bind(this, "isArchivingRecordOption")}
                                />
                            </label>
                            <$g.Popover>{RMResx.RM_JS_Rule_ArchiveIncludeDeclaredFileDescription}</$g.Popover>
                        </div>
                        {isSiteCollectionLevel &&
                            <div id="rm_createRule_declaredFile">
                                <label>
                                    <R.Checkbox
                                        text={RMResx.RM_JS_Rule_Delete_RecycleBinOption}
                                        disabled={this.state.elementsEnable}
                                        checked={this.state.isSODeleteToRecycleBinForSPO}
                                        onChange={this.onCheckChange.bind(this, "isSODeleteToRecycleBinForSPO")} />
                                </label>
                            </div>
                        }
                    </>
                )}
                {enableLockByRecordsLabelOption && this.supportingLevelsForRecordsLabel.includes(this.levelId) && this.enableRecordsArchiver && !LicenseHelper.Is21VEnv() && this.renderIncludeLockedFileSetting("isIncludeLockedFile", actionType)}
            </div>;
        }
    }

    renderLeaveStubOptionForTeams(actionType){
        const isRemove = actionType == ruleActionType.Remove;
        const showLeaveStubOptionOption = isRemove ? true : this.state.isArchiveToAzureBlobStorageForTeams;
        const leaveStubOptionOptionClassName = isRemove ? "" : "rm-createRule-leaveStubMessage-container";
        const enableKeepVersionAndArchiveOther = RM.gData.enableArchiverVersionNotIncludeLatest && this.state.selectedRuleModuleType === RuleModuleTypes.SOArchiver && this.levelId == this.RuleLevel.Document;
        const enableArchiverLatestVersion = RM.gData.enableArchiverLatestVersion && this.state.selectedRuleModuleType === RuleModuleTypes.SOArchiver && this.levelId == this.RuleLevel.Document;
        const levelIds = [Constants.RuleLevel.Document, Constants.RuleLevel.Item, Constants.RuleLevel.Folder, Constants.RuleLevel.List, Constants.RuleLevel.Site, Constants.RuleLevel.Teams];
        const enableArchivingRecords = this.state.selectedRuleModuleType === RuleModuleTypes.SOArchiver && levelIds.includes(this.levelId);
        if(showLeaveStubOptionOption){
            return (
                <div className={leaveStubOptionOptionClassName}>
                    {enableKeepVersionAndArchiveOther && (
                        <div>
                            <R.Checkbox
                                text={RMResx.RM_JS_Rule_KeepVersionAndArchiveOther}
                                disabled={this.state.elementsEnable}
                                checked={this.state.isKeepVersionAndArchiveForTeams}
                                onChange={this.onKeepVersionAndArchiveChangeForTeams}
                            />
                            {this.state.isKeepVersionAndArchiveForTeams && this.renderKeepVersionAndArchiveSettingForTeams()}
                        </div>
                    )}
                    {enableArchiverLatestVersion && (
                        <div>
                            <R.Checkbox
                                text={RMResx.RM_JS_Rule_ArchiveVersionAndDestroyFile}
                                disabled={this.state.elementsEnable || this.state.isKeepVersionAndArchiveForTeams}
                                checked={this.state.isArchiveVersionOptionForTeams}
                                onChange={this.onCheckChange.bind(this, "isArchiveVersionOptionForTeams")}
                            />
                            <$g.Popover>{RMResx.RM_JS_Rule_ArchiveVersion_Message}</$g.Popover>
                            {this.state.isArchiveVersionOptionForTeams && this.renderArchiveVersionSettingForTeams()}
                        </div>
                    )}
                    {this.state.isShowLeaveStubOption && (
                        <div id="rm_createRule_leaveStubOption">
                            <R.Checkbox
                                text={RMResx.RM_RDM_CreateRule_LeaveStubOption}
                                disabled={this.state.elementsEnable || this.state.isKeepVersionAndArchiveForTeams}
                                checked={this.state.isLeaveStubOptionForTeams}
                                onChange={this.onCheckChange.bind(this, "isLeaveStubOptionForTeams")} />
                            <$g.Popover>
                                <$g.I18NProvider msg={RMResx.RM_JS_Rule_LeaveStubDescription}>
                                    <a className="ra-link-a" href="/Root/CP/StubSettings">{RMResx.RM_AR_CP_StubSettings}</a>
                                </$g.I18NProvider>
                            </$g.Popover>
                        </div>
                    )}
                    {this.state.isLeaveStubOptionForTeams && this.renderLeaveStubSetting(Constants.RuleSourceTabIndex.Teams)}
                    {!isRemove && this.enableRecordsArchiver && RM.gData.enviromentName != Enviroments.ChinaNorth && this.renderSensitiveAndRententionLabelsSetting("isRetentionLabelForTeams")}
                    {/* Archiving records */}
                    {(enableArchivingRecords || this.state.isArchiveToAzureBlobStorageForTeams) && (
                        <div id="rm_createRule_archivingRecordsOption" style={{ margin: "3px 0" }}>
                            <label>
                                <R.Checkbox
                                    text={RMResx.RM_RDM_CreateRule_ArchivingRecordsOption}
                                    disabled={this.state.elementsEnable}
                                    checked={this.state.isArchivingRecordOptionForTeams}
                                    onChange={this.onCheckChange.bind(this, "isArchivingRecordOptionForTeams")}
                                />
                            </label>
                            <$g.Popover>{RMResx.RM_JS_Rule_Teams_IncludeDeclaredFileDescription}</$g.Popover>
                        </div>
                    )}
                </div>
            );
        }
    }

    renderKeepVersionOptionForSPO() {
        let levelIds = [Constants.RuleLevel.Document, Constants.RuleLevel.Item, Constants.RuleLevel.Folder, Constants.RuleLevel.List, Constants.RuleLevel.Site, Constants.RuleLevel.SiteCollection];
        let enableArchivingRecords = this.state.selectedRuleModuleType === RuleModuleTypes.SOArchiver && levelIds.includes(this.levelId);
        const notAllowDisplayDestroyOptionSOForSPO = [Constants.RuleLevel.DocumentVersion];
        const isAllow = this.state.selectedRuleModuleType === RuleModuleTypes.SOArchiver && notAllowDisplayDestroyOptionSOForSPO.includes(this.levelId) ? false : true;

        return <div>
            {isAllow && <>
                    <div>
                        <R.Checkbox
                            text={RMResx.RM_JS_Rule_KeepLatestVersionAndDestroyOther}
                            disabled={this.state.elementsEnable}
                            checked={this.state.isKeepVersionOption}
                            onChange={this.onCheckChange.bind(this, "isKeepVersionOption")} />
                        <$g.Popover>{RMResx.RM_JS_Rule_KeepVersion_Message}</$g.Popover>
                    </div>
                    {this.state.isKeepVersionOption && <div className="cr-archive-action-children-selection">
                        <R.Input
                            id="raKeepVersionNumber"
                            type="number"
                            width="50%"
                            onChange={this.onChangeKeepVersion}
                            value={this.state.keepVersionValue}
                        />
                        <$g.ValidationMsg show={this.state.noKeepVersionValueForSPO}>
                            {RMResx.RM_JS_RDM_CreateRule_Validation_ConditionNoValue}
                        </$g.ValidationMsg>
                        <$g.ValidationMsg show={this.state.keepVersionValueInvalidForSPO}>
                            {RMResx.RM_JS_RDM_NotNumber}
                        </$g.ValidationMsg>
                    </div>}

                    {/* Archiving records */}
                    {enableArchivingRecords && (
                        <div id="rm_createRule_archivingRecordsOption" style={{ marginTop: 3, marginBottom: 8 }}>
                            <label>
                                <R.Checkbox
                                    text={RMResx.RM_RDM_CreateRule_ArchivingRecordsOption}
                                    disabled={this.state.elementsEnable}
                                    checked={this.state.isArchivingRecordOption}
                                    onChange={this.onCheckChange.bind(this, "isArchivingRecordOption")}
                                />
                            </label>
                            <$g.Popover>{RMResx.RM_JS_Rule_IncludeDeclaredFileDescription}</$g.Popover>
                        </div>
                    )}
                    {this.supportingLevelsForRecordsLabel.includes(this.levelId) && this.enableRecordsArchiver && !LicenseHelper.Is21VEnv() && this.renderIncludeLockedFileSetting("isIncludeLockedFile")}
                </>
            }
            <div id="rm_crateRule_declaredFile">
                <label>
                    <R.Checkbox
                        text={RMResx.RM_JS_Rule_Delete_RecycleBinOption}
                        disabled={this.state.elementsEnable}
                        checked={this.state.isSODeleteToRecycleBinForSPO}
                        onChange={this.onCheckChange.bind(this, "isSODeleteToRecycleBinForSPO")} />
                </label>
            </div>
        </div>;
    }

    renderKeepVersionOptionForTeams = () => {
        let levelIds = [Constants.RuleLevel.Document, Constants.RuleLevel.Teams];
        let enableArchivingRecords = this.state.selectedRuleModuleType === RuleModuleTypes.SOArchiver && levelIds.includes(this.levelId);

        return (
            <div>
                <div>
                    <R.Checkbox
                        text={RMResx.RM_JS_Rule_KeepLatestVersionAndDestroyOther}
                        disabled={this.state.elementsEnable}
                        checked={this.state.isKeepVersionOptionForTeams}
                        onChange={this.onCheckChange.bind(this, "isKeepVersionOptionForTeams")} />
                    <$g.Popover>{RMResx.RM_JS_Rule_KeepVersion_Message}</$g.Popover>
                </div>
                {this.state.isKeepVersionOptionForTeams && (
                    <div className="cr-archive-action-children-selection">
                        <R.Input
                            id="raKeepVersionNumber"
                            type="number"
                            width="50%"
                            onChange={this.onChangeKeepVersionForTeams}
                            value={this.state.keepVersionValueForTeams}
                        />
                        <$g.ValidationMsg show={this.state.noKeepVersionValueForTeams}>
                            {RMResx.RM_JS_RDM_CreateRule_Validation_ConditionNoValue}
                        </$g.ValidationMsg>
                        <$g.ValidationMsg show={this.state.keepVersionValueInvalidForTeams}>
                            {RMResx.RM_JS_RDM_NotNumber}
                        </$g.ValidationMsg>
                    </div>
                )}

                {/* Archiving records */}
                {enableArchivingRecords && (
                    <div id="rm_createRule_archivingRecordsOption" style={{ marginTop: 3, marginBottom: 8 }}>
                        <label>
                            <R.Checkbox
                                text={RMResx.RM_RDM_CreateRule_ArchivingRecordsOption}
                                disabled={this.state.elementsEnable}
                                checked={this.state.isArchivingRecordOptionForTeams}
                                onChange={this.onCheckChange.bind(this, "isArchivingRecordOptionForTeams")}
                            />
                        </label>
                    </div>
                )}
            </div>
        );
    }

    onChangeKeepVersion = (value) => {
        this.setState({
            keepVersionValue: value,
            noKeepVersionValueForSPO: false,
            keepVersionValueInvalidForSPO: false,
        });
    }

    onChangeKeepVersionForOD = (value) => {
        this.setState({
            keepVersionValueForOD: value,
            noKeepVersionValueForOD: false,
            keepVersionValueInvalidForOD: false,
        });
    }

    onChangeKeepVersionForTeams = (value) => {
        this.setState({
            keepVersionValueForTeams: value,
            noKeepVersionValueForTeams: false,
            keepVersionValueInvalidForTeams: false,
        });
    }

    onChangeArchiveVersionForTeams = (value) => {
        this.setState({
            archiveVersionValueForTeams: value,
            noArchiveVersionValueForTeams: false,
            archiveVersionValueInvalidForTeams: false,
        });
    }

    onChangeArchiveVersion = (value) => {
        this.setState({
            archiveVersionValue: value,
            noArchiveVersionValueForSPO: false,
            archiveVersionValueInvalidForSPO: false,
        });
    }

    onChangeArchiveVersionForOD = (value) => {
        this.setState({
            archiveVersionValueForOD: value,
            noArchiveVersionValueForOD: false,
            archiveVersionValueInvalidForOD: false,
        });
    }

    onChangeKeepVersionAndArchiveForTeams = (value) => {
        this.setState({
            keepVersionAndArchiveValueForTeams: value,
            noKeepVersionAndArchiveValueForTeams: false,
            keepVersionAndArchiveValueInvalidForTeams: false,
        });
    }

    onChangeKeepVersionAndArchive = (value) => {
        this.setState({
            keepVersionAndArchiveValueForSPO: value,
            noKeepVersionAndArchiveValueForSPO: false,
            keepVersionAndArchiveValueInvalidForSPO: false,
        });
    }

    onChangeKeepVersionAndArchiveForOD = (value) => {
        this.setState({
            keepVersionAndArchiveValueForOD: value,
            noKeepVersionAndArchiveValueForOD: false,
            keepVersionAndArchiveValueInvalidForOD: false,
        });
    }

    onChangeArchiveToAzureBlobStorage = () => {
        this.setState({
            isRemove: false,
            isKeep: false,
            isMove: false,
            isStoreInM365Archive: false,
            isArchiveWithoutDestroy: false,
            isExportOnly: false,
            isArchiveToAzureBlobStorage: true,
            isIncludeLockedFile: false,
            isRetentionLabel: false,
            isLeaveStubOption: false,
            isRestoreLink: false,
            isDeclareLinkFile: false,
            isDeleteRelatedRecordOption: false,
            isDeclaredFile: false,
            isDeleteToRecycleBinForSPO: false,
            isBackupOption: false,
            isArchivingRecordOption: false,
        });
        this.dispatch("spApproval", Constants.dispatchAction.approvalCheckboxDisabledAndChecked, false, true);
        this.resetExportOption(Constants.RuleSourceTabIndex.SP);
    }

    onChangeArchiveToAzureBlobStorageForTeams = () => {
        this.setState({
            isRemoveForTeams: false,
            isKeepForTeams: false,
            isMoveForTeams: false,
            isExportOnlyForTeams: false,
            isArchiveToAzureBlobStorageForTeams: true,
            isRetentionLabelForTeams: false,
            isLeaveStubOptionForTeams: false,
            isRestoreLinkForTeams: false,
            isDeclareLinkFileForTeams: false,
            isDeleteRelatedRecordOptionForTeams: false,
            isDeclaredFileForTeams: false,
            isBackupOptionForTeams: false,
            isArchivingRecordOptionForTeams: false,
        });
        this.dispatch("teamsApproval", Constants.dispatchAction.approvalCheckboxDisabledAndChecked, false, true);
        this.resetExportOption(Constants.RuleSourceTabIndex.Teams);
    }

    getSPStorage = (value) => {
        this.dispatch("raCrSpRemoveArchivedForSp", Constants.dispatchAction.selectedStorage, value);
    }

    getODStorage = (value) => {
        this.dispatch("raCrRemoveArchivedForOd", Constants.dispatchAction.selectedStorage, value);
    }

    getTeamsStorage = (value) => {
        this.dispatch("raCrRemoveArchivedForTeams", Constants.dispatchAction.selectedStorage, value);
    }

    getPhyStorage = (value) => {
        this.dispatch("raCrRemoveArchivedForPhy", Constants.dispatchAction.selectedStorage, value);
    }

    getGoogleStorage = (value) => {
        this.dispatch("raCrRemoveArchivedForGoogle", Constants.dispatchAction.selectedStorage, value);
    }

    resetSPRetentionInfo = () => {
        if (this.state.selectedRuleModuleType === Constants.RuleModuleTypes.SOArchiver) {
            this.dispatch("raCrSpRemoveArchivedForSp", Constants.dispatchAction.resetRetentionInfo);
        }
    }

    resetODRetentionInfo = () => {
        if (this.state.selectedRuleModuleType === Constants.RuleModuleTypes.SOArchiver) {
            this.dispatch("raCrRemoveArchivedForOd", Constants.dispatchAction.resetRetentionInfo);
        }
    }

    resetTeamsRetentionInfo = () => {
        if (this.state.selectedRuleModuleType === Constants.RuleModuleTypes.SOArchiver) {
            this.dispatch("raCrRemoveArchivedForTeams", Constants.dispatchAction.resetRetentionInfo);
        }
    }
    
    renderRemoveStoreAndLeaveSubSPO(){
        return <div>
            {this.renderLeaveStubOptionForSPO(ruleActionType.Remove)}
            <div id="rm_createRule_backupBeforeDestroying">
                <R.Checkbox
                    text={RMResx.RM_RDM_CreateRule_BackupBeforeDestroying}
                    disabled={this.state.elementsEnable}
                    checked={this.state.isBackupOption}
                    onChange={this.onCheckChange.bind(this, "isBackupOption")} />
                <$g.Popover>{RMResx.RM_RDM_CreateRule_BackupBeforeDestroyingDescription}</$g.Popover>
            </div>
        </div>;
    }

    renderRemoveStoreAndLeaveStubTeams = () => {
        return <div>
            {/* Leave a stub */}
            {this.renderLeaveStubOptionForTeams(ruleActionType.Remove)}
            {/* Enable grace period */}
            <div id="rm_createRule_backupBeforeDestroying">
                <R.Checkbox
                    text={RMResx.RM_RDM_CreateRule_BackupBeforeDestroying}
                    disabled={this.state.elementsEnable}
                    checked={this.state.isBackupOptionForTeams}
                    onChange={this.onCheckChange.bind(this, "isBackupOptionForTeams")} />
                <$g.Popover>{RMResx.RM_RDM_CreateRule_BackupBeforeDestroyingDescription}</$g.Popover>
            </div>
        </div>;
    }

    renderArchiveToAzureBlobStorage(){
        return <div>
            <div id="rm_createRule_backupBeforeDestroying">
                <R.Radio
                    name="ruleActionForSPO"
                    text={RMResx.RM_RDM_CreateRule_ArchiveToAzureBlobStorage}
                    disabled={this.state.elementsEnable}
                    checked={this.state.isArchiveToAzureBlobStorage}
                    onChange={this.onChangeArchiveToAzureBlobStorage} />
                <$g.Popover>{RMResx.RM_RDM_CreateRule_ArchiveToAzureBlobStorageDes}</$g.Popover>
            </div>
            {this.renderLeaveStubOptionForSPO(ruleActionType.ArchiveToAzureBlobStorage)}
        </div>;
    }

    renderArchiveToAzureBlobStorageForTeams = () => {
        return <div>
            <div id="rm_createRule_backupBeforeDestroying">
                <R.Radio
                    name="ruleActionForTeams"
                    text={RMResx.RM_RDM_CreateRule_ArchiveToAzureBlobStorage}
                    disabled={this.state.elementsEnable}
                    checked={this.state.isArchiveToAzureBlobStorageForTeams}
                    onChange={this.onChangeArchiveToAzureBlobStorageForTeams} />
                <$g.Popover>{RMResx.RM_RDM_CreateRule_ArchiveToAzureBlobStorageDes}</$g.Popover>
            </div>
            {this.renderLeaveStubOptionForTeams(ruleActionType.ArchiveToAzureBlobStorage)}
        </div>;
    }

    renderBackupAndRemove(tabIndex){
        let backupAndRemoveMapping = {
            [Constants.RuleSourceTabIndex.SP]: {
                state: this.state.isBackupAndRemoveForSPO,
                methed: this.backupAndRemove
            },
            [Constants.RuleSourceTabIndex.OneDrive]: {
                state: this.state.isBackupAndRemoveForOD,
                methed: this.backupAndRemoveForOneDrive
            },
            [Constants.RuleSourceTabIndex.Teams]: {
                state: this.state.isBackupAndRemoveForTeams,
                methed: this.backupAndRemoveForTeams
            },
        };
        const radioNameMapping = {
            [Constants.RuleSourceTabIndex.SP]: "ruleActionForSPO",
            [Constants.RuleSourceTabIndex.OneDrive]: "ruleActionForOneDrive",
            [Constants.RuleSourceTabIndex.Teams]: "ruleActionForTeams",
        }
        let curBackupAndRemoveInfo = backupAndRemoveMapping[tabIndex];
        let leaveStubOptions = {
            [Constants.RuleSourceTabIndex.SP]: this.renderLeaveStubOptionForSPO(ruleActionType.Remove),
            [Constants.RuleSourceTabIndex.OneDrive]: this.renderLeaveStubOptionForOneDrive(ruleActionType.Remove),
            [Constants.RuleSourceTabIndex.Teams]: this.renderLeaveStubOptionForTeams(ruleActionType.Remove),
        };
        
        return <div className='rm_createRule_remove'>
            <R.Radio
                name={radioNameMapping[tabIndex]}
                text={RMResx.RM_JS_RDM_CreateRule_Options_BackupAndRemove}
                checked={curBackupAndRemoveInfo.state}
                disabled={this.state.elementsEnable}
                onChange={curBackupAndRemoveInfo.methed} />
            <$g.Popover>{RMResx.RM_Rule_BackupAndRemoveDesc}</$g.Popover>
            {
                curBackupAndRemoveInfo.state && <div className="cr-archive-action-children-selection">
                    {leaveStubOptions[tabIndex]}
                </div>
            }
        </div>;
    }

    renderRemoveContent = (tabIndex) => {
        let soRemoveContentMapping = {
            [Constants.RuleSourceTabIndex.SP]: {
                state: this.state.isSORemoveForSPO,
                methed: this.soRemoveForSPO
            },
            [Constants.RuleSourceTabIndex.OneDrive]: {
                state: this.state.isSORemoveForOD,
                methed: this.soRemoveForOneDrive
            },
            [Constants.RuleSourceTabIndex.Teams]: {
                state: this.state.isSORemoveForTeams,
                methed: this.soRemoveForTeams
            },
        };
        const soRadioNameMapping = {
            [Constants.RuleSourceTabIndex.SP]: "ruleRemoveForSPO",
            [Constants.RuleSourceTabIndex.OneDrive]: "ruleRemoveForOneDrive",
            [Constants.RuleSourceTabIndex.Teams]: "ruleRemoveForTeams",
        };
        let curRemoveContentInfo = soRemoveContentMapping[tabIndex];
        let keepVersionOptions = {
            [Constants.RuleSourceTabIndex.SP]: this.renderKeepVersionOptionForSPO(),
            [Constants.RuleSourceTabIndex.OneDrive]: this.renderKeepVersionOptionForOneDrive(),
            [Constants.RuleSourceTabIndex.Teams]: this.renderKeepVersionOptionForTeams(),
        };
        
        return <div className='rm_createRule_remove'>
            <R.Radio
                name={soRadioNameMapping[tabIndex]}
                text={RMResx.RM_JS_RDM_CreateRule_Options_ArchiveAndRemove}
                checked={curRemoveContentInfo.state}
                disabled={this.state.elementsEnable}
                onChange={curRemoveContentInfo.methed} />
            <$g.Popover>{tabIndex == Constants.RuleSourceTabIndex.SP ?RMResx.RM_JS_Rule_RemoveActionDesc:RMResx.RM_JS_Rule_RemoveActionDesc_OD}</$g.Popover>
            {
                curRemoveContentInfo.state && <div className="cr-archive-action-children-selection">
                    {keepVersionOptions[tabIndex]}
                </div>
            }
        </div>;
    }

    renderArchiveWithoutDestroyingForSPO = () => {
        const ruleLevelMapping = new Map([
            [Constants.RuleLevel.Document, true],
            [Constants.RuleLevel.Item, true],
        ]);
        const enableArchiveWithoutDestroyOption =
            LicenseHelper.EnableArchiverOnly() &&
            ruleLevelMapping.has(this.levelId);
        const enableArchiveLatestVersion =
            LicenseHelper.EnableArchiverOnly() &&
            this.state.isArchiveWithoutDestroy &&
            this.levelId == this.RuleLevel.Document;

        if (enableArchiveWithoutDestroyOption) {
            return (
                <div>
                    <div>
                        <label>
                            <R.Radio
                                name="ruleActionForSPO"
                                text={RMResx.RM_JS_RDM_CreateRule_Options_Backup}
                                checked={this.state.isArchiveWithoutDestroy}
                                disabled={this.state.elementsEnable}
                                onChange={this.spArchiveWithoutDestroyCheckedChange} />
                        </label>
                        <$g.Popover>{RMResx.RM_JS_RDM_CreateRule_Options_BackupDesc}</$g.Popover>
                    </div>
                    {enableArchiveLatestVersion && (
                        <div className="cr-archive-action-children-selection">
                            <R.Checkbox
                                text={RMResx.RM_JS_Rule_ArchiveVersionAndDestroyFile}
                                disabled={this.state.elementsEnable}
                                checked={this.state.isArchiveVersionOption}
                                onChange={this.onCheckChange.bind(this, "isArchiveVersionOption")}
                            />
                            <$g.Popover>{RMResx.RM_JS_Rule_ArchiveVersion_Message}</$g.Popover>
                            {this.state.isArchiveVersionOption && this.renderArchiveVersionSetting()}
                        </div>
                    )}
                </div>
            );
        }

        return null;
    }

    renderLeaveStubOptionForOneDrive(actionType){
        let isRemove = actionType == ruleActionType.Remove;
        let showLeaveStubOptionOption = isRemove ? true : this.state.isArchiveToAzureBlobStorageForOneDrive;
        let leaveStubOptionOptionClassName = isRemove ? "" : "rm-createRule-leaveStubMessage-container";
        let enableKeepVersionAndArchiveOther = RM.gData.enableArchiverVersionNotIncludeLatest && this.state.selectedRuleModuleType === RuleModuleTypes.SOArchiver && this.levelId == this.RuleLevel.Document;
        let enableArchiverLatestVersion = RM.gData.enableArchiverLatestVersion && this.state.selectedRuleModuleType === RuleModuleTypes.SOArchiver && this.levelId == this.RuleLevel.Document;
        let levelIds = [Constants.RuleLevel.Document, Constants.RuleLevel.Item, Constants.RuleLevel.Folder, Constants.RuleLevel.List, Constants.RuleLevel.Site, Constants.RuleLevel.SiteCollection];
        let enableArchivingRecords = this.state.selectedRuleModuleType === RuleModuleTypes.SOArchiver && levelIds.includes(this.levelId);
        const enableLockByRecordsLabelOption = this.state.selectedRuleModuleType === RuleModuleTypes.Records && this.state.isRemoveForOneDrive ? false : this.state.isBackupAndRemoveForOD || this.state.isArchiveToAzureBlobStorageForOneDrive;
        if(showLeaveStubOptionOption){
            return <div className={leaveStubOptionOptionClassName}>
                {enableKeepVersionAndArchiveOther && <div>
                    <R.Checkbox
                        text={RMResx.RM_JS_Rule_KeepVersionAndArchiveOther}
                        disabled={this.state.elementsEnable}
                        checked={this.state.isKeepVersionAndArchiveForOD}
                        onChange={this.onKeepVersionAndArchiveChangeForOD}
                    />
                    {this.state.isKeepVersionAndArchiveForOD && this.renderKeepVersionAndArchiveSettingForOD()}
                </div>}
                {enableArchiverLatestVersion && <div>
                    <R.Checkbox
                        text={RMResx.RM_JS_Rule_ArchiveVersionAndDestroyFile}
                        disabled={this.state.elementsEnable || this.state.isKeepVersionAndArchiveForOD}
                        checked={this.state.isArchiveVersionOptionForOD}
                        onChange={this.onCheckChange.bind(this, "isArchiveVersionOptionForOD")}
                    />
                    <$g.Popover>{RMResx.RM_JS_Rule_ArchiveVersion_Message}</$g.Popover>
                    {this.state.isArchiveVersionOptionForOD && this.renderArchiveVersionSettingForOD()}
                </div>}
                {
                    this.state.isShowLeaveStubOptionForOneDrive &&
                    <div id="rm_createRule_leaveStubOption">
                        <R.Checkbox
                            text={RMResx.RM_RDM_CreateRule_LeaveStubOption}
                            disabled={this.state.elementsEnable || this.state.isKeepVersionAndArchiveForOD}
                            checked={this.state.isLeaveStubOptionForOneDrive}
                            onChange={this.onCheckChange.bind(this, "isLeaveStubOptionForOneDrive")} />
                        <$g.Popover>
                            <$g.I18NProvider msg={RMResx.RM_JS_Rule_LeaveStubDescription}>
                                <a className="ra-link-a" href="/Root/CP/StubSettings">{RMResx.RM_AR_CP_StubSettings}</a>
                            </$g.I18NProvider>
                        </$g.Popover>
                    </div>
                }
                {this.state.isLeaveStubOptionForOneDrive && this.renderLeaveStubSetting(Constants.RuleSourceTabIndex.OneDrive)}
                {!isRemove && this.enableRecordsArchiver && RM.gData.enviromentName != Enviroments.ChinaNorth && this.renderSensitiveAndRententionLabelsSetting("isRetentionLabelForOneDrive")}
                {/* Archiving records */}
                {(enableArchivingRecords || this.state.isArchiveToAzureBlobStorageForOneDrive) && (
                    <div id="rm_createRule_archivingRecordsOption" style={{ margin: "3px 0" }}>
                        <label>
                            <R.Checkbox
                                text={RMResx.RM_RDM_CreateRule_ArchivingRecordsOption}
                                disabled={this.state.elementsEnable}
                                checked={this.state.isArchivingRecordOptionForOneDrive}
                                onChange={this.onCheckChange.bind(this, "isArchivingRecordOptionForOneDrive")}
                            />
                        </label>
                        <$g.Popover>
                            {isRemove ? RMResx.RM_JS_Rule_IncludeDeclaredFileDescription_OD_SO : RMResx.RM_JS_Rule_ArchiveIncludeDeclaredFileDescription_OD}
                        </$g.Popover>
                    </div>
                )}
                {enableLockByRecordsLabelOption && this.supportingLevelsForRecordsLabel.includes(this.levelId) && this.enableRecordsArchiver && !LicenseHelper.Is21VEnv() && this.renderIncludeLockedFileSetting("isIncludeLockedFileForOneDrive", actionType, "isLockRecordForOneDrive")}
            </div>;
        }
    }

    renderKeepVersionOptionForOneDrive() {
        let levelIds = [Constants.RuleLevel.Document, Constants.RuleLevel.Item, Constants.RuleLevel.Folder, Constants.RuleLevel.List, Constants.RuleLevel.Site, Constants.RuleLevel.SiteCollection];
        let enableArchivingRecords = this.state.selectedRuleModuleType === RuleModuleTypes.SOArchiver && levelIds.includes(this.levelId);
        const notAllowDisplayDestroyOptionSOForOD = [Constants.RuleLevel.DocumentVersion];
        const isAllow = this.state.selectedRuleModuleType === RuleModuleTypes.SOArchiver && notAllowDisplayDestroyOptionSOForOD.includes(this.levelId) ? false : true;

        return <div>
            {isAllow && <>
                <div>
                    <R.Checkbox
                        text={RMResx.RM_JS_Rule_KeepLatestVersionAndDestroyOther}
                        disabled={this.state.elementsEnable}
                        checked={this.state.isKeepVersionOptionForOD}
                        onChange={this.onCheckChange.bind(this, "isKeepVersionOptionForOD")} />
                    <$g.Popover>{RMResx.RM_JS_Rule_KeepVersion_Message}</$g.Popover>
                </div>
                {this.state.isKeepVersionOptionForOD && <div className="cr-archive-action-children-selection">
                    <R.Input
                        id="raKeepVersionNumber"
                        type="number"
                        width="50%"
                        onChange={this.onChangeKeepVersionForOD}
                        value={this.state.keepVersionValueForOD}
                    />
                    <$g.ValidationMsg show={this.state.noKeepVersionValueForOD}>
                        {RMResx.RM_JS_RDM_CreateRule_Validation_ConditionNoValue}
                    </$g.ValidationMsg>
                    <$g.ValidationMsg show={this.state.keepVersionValueInvalidForOD}>
                        {RMResx.RM_JS_RDM_NotNumber}
                    </$g.ValidationMsg>
                </div>}

                {/* Archiving records */}
                {enableArchivingRecords && (
                    <div id="rm_createRule_archivingRecordsOption" style={{ marginTop: 3, marginBottom: 8 }}>
                        <label>
                            <R.Checkbox
                                text={RMResx.RM_RDM_CreateRule_ArchivingRecordsOption}
                                disabled={this.state.elementsEnable}
                                checked={this.state.isArchivingRecordOptionForOneDrive}
                                onChange={this.onCheckChange.bind(this, "isArchivingRecordOptionForOneDrive")}
                            />
                            <$g.Popover>{RMResx.RM_JS_Rule_IncludeDeclaredFileDescription_OD_SO}</$g.Popover>
                        </label>
                    </div>
                )}
                {this.supportingLevelsForRecordsLabel.includes(this.levelId) && this.enableRecordsArchiver && !LicenseHelper.Is21VEnv() && this.renderIncludeLockedFileSetting("isIncludeLockedFileForOneDrive", undefined, "isLockRecordForOneDrive")}
            </>}
            <div id="rm_crateRule_declaredFile">
                <label>
                    <R.Checkbox
                        text={RMResx.RM_JS_Rule_Delete_RecycleBinOption}
                        disabled={this.state.elementsEnable} 
                        checked={this.state.isSODeleteToRecycleBinForOD}
                        onChange={this.onCheckChange.bind(this, "isSODeleteToRecycleBinForOD")} />
                </label>
            </div>
        </div>;
    }

    onChangeArchiveToAzureBlobStorageForOneDrive = () => {
        this.setState({
            isRemoveForOneDrive: false,
            isKeepForOneDrive: false,
            isMoveForOneDrive: false,
            isExportOnlyForOneDrive: false,
            isArchiveToAzureBlobStorageForOneDrive: true,
            isIncludeLockedFileForOneDrive: false,
            isLeaveStubOptionForOneDrive: false,
            isRestoreLinkForOneDrive: false,
            isDeclareLinkFileForOneDrive: false,
            isDeleteRelatedRecordOptionForOneDrive: false,
            isDeclaredFileForOneDrive: false,
            isDeleteToRecycleBinForOneDrive: false,
            isRetentionLabelForOneDrive: false,
            isBackupOptionForOneDrive: false,
            isArchiveWithoutDestroyForOneDrive: false,
            isArchivingRecordOptionForOneDrive: false,
        });
        this.dispatch("oneDriveApproval", Constants.dispatchAction.approvalCheckboxDisabledAndChecked, false, true);
        this.resetExportOption(Constants.RuleSourceTabIndex.OneDrive);
    }   

    criteriaChangeHandler = () => {
        const { isCalculationDisposalDate } = this.state;

        return {
            [Constants.RuleSourceTabIndex.Physical]: (criterias) => {
                const mappedCriterias = criterias.map((item) => ({
                    currentType: item.currentType,
                    currentMatch1: item.currentMatch1,
                }));
                const isModifiedTimeAndOlderThan = mappedCriterias.some((item) => 
                    item.currentType.id == Constants.RuleType.Modified &&
                    item.currentMatch1.id == Constants.ConditionType.OlderThan
                );

                if (!isModifiedTimeAndOlderThan && isCalculationDisposalDate) {
                    this.phyRemoveCheckedChange();
                }

                this.setState({
                    showCalculateDisposalDateOptionForPhy: this.levelId === Constants.RuleLevel.Folder && isModifiedTimeAndOlderThan,
                });
            }
        };
    }

    handleChangeCriterias = (newCriterias, sourceTabIndex) => {
        if (Array.isArray(newCriterias) && newCriterias.length > 0) {
            const handler = this.criteriaChangeHandler()[sourceTabIndex];
            if (handler) {
                handler(newCriterias);
            }
        }
    }

    renderRemoveStoreAndLeaveSubForOneDrive(){
        return  <div>
            {this.renderLeaveStubOptionForOneDrive(ruleActionType.Remove)}
            <div id="rm_createRule_backupBeforeDestroying">
                <R.Checkbox
                    text={RMResx.RM_RDM_CreateRule_BackupBeforeDestroying}
                    disabled={this.state.elementsEnable}
                    checked={this.state.isBackupOptionForOneDrive}
                    onChange={this.onCheckChange.bind(this, "isBackupOptionForOneDrive")} />
                <$g.Popover>{RMResx.RM_RDM_CreateRule_BackupBeforeDestroyingDescription}</$g.Popover>
            </div>
        </div>;
    }

    renderArchiveToAzureBlobStorageForOneDrive(){
        return <div>
            <div id="rm_createRule_backupBeforeDestroying">
                <R.Radio
                    name="ruleActionForOneDrive"
                    text={RMResx.RM_RDM_CreateRule_ArchiveToAzureBlobStorage}
                    disabled={this.state.elementsEnable}
                    checked={this.state.isArchiveToAzureBlobStorageForOneDrive}
                    onChange={this.onChangeArchiveToAzureBlobStorageForOneDrive} />
                <$g.Popover>{RMResx.RM_RDM_CreateRule_ArchiveToAzureBlobStorageDes}</$g.Popover>
            </div>
            {this.renderLeaveStubOptionForOneDrive(ruleActionType.ArchiveToAzureBlobStorage)}
        </div>;
    }

    renderArchiveWithoutDestroyingForOneDrive = () => {
        const ruleLevelMapping = new Map([
            [Constants.RuleLevel.Document, true],
            [Constants.RuleLevel.Item, true],
        ]);
        const enableArchiveWithoutDestroyOption =
            LicenseHelper.EnableArchiverOnly() &&
            ruleLevelMapping.has(this.levelId);
        const enableArchiveLatestVersion =
            LicenseHelper.EnableArchiverOnly() &&
            this.state.isArchiveWithoutDestroyForOneDrive &&
            this.levelId == this.RuleLevel.Document;

        if (enableArchiveWithoutDestroyOption) {
            return (
                <div>
                    <div>
                        <label>
                            <R.Radio
                                name="ruleActionForOneDrive"
                                text={RMResx.RM_JS_RDM_CreateRule_Options_Backup}
                                checked={this.state.isArchiveWithoutDestroyForOneDrive}
                                disabled={this.state.elementsEnable}
                                onChange={this.odArchiveWithoutDestroyCheckedChange} />
                        </label>
                        <$g.Popover>{RMResx.RM_JS_RDM_CreateRule_Options_BackupDesc}</$g.Popover>
                    </div>
                    {enableArchiveLatestVersion && (
                        <div className="cr-archive-action-children-selection">
                            <R.Checkbox
                                text={RMResx.RM_JS_Rule_ArchiveVersionAndDestroyFile}
                                disabled={this.state.elementsEnable}
                                checked={this.state.isArchiveVersionOptionForOD}
                                onChange={this.onCheckChange.bind(this, "isArchiveVersionOptionForOD")}
                            />
                            <$g.Popover>{RMResx.RM_JS_Rule_ArchiveVersion_Message}</$g.Popover>
                            {this.state.isArchiveVersionOptionForOD && this.renderArchiveVersionSettingForOD()}
                        </div>
                    )}
                </div>
            );
        }

        return null;
    }

    renderStorageSettingsForSPO() {
        let isRelatedOrStore = this.state.isRemove && (this.state.isDeleteRelatedRecordOption || this.state.isBackupOption);
        if (
            isRelatedOrStore ||
            this.state.isArchiveToAzureBlobStorage ||
            (this.state.isBackupAndRemoveForSPO && this.state.selectedRuleModuleType === Constants.RuleModuleTypes.SOArchiver) ||
            (this.state.isArchiveWithoutDestroy && this.state.selectedRuleModuleType === Constants.RuleModuleTypes.SOArchiver)
        ) {
            return <div>
                <StorageSettings
                    ref={r => this.spStorageSettingsRef = r}
                    id={"StorageSettings4SPO"}
                    sourceTab={Constants.RuleSourceTabIndex.SP}
                    storagePolicyList={this.props.storagePolicyList}
                    storagePolicyId={this.state.ruleItem?.StoragePolicyId || this.props.indexDeviceId}
                    storeDataTierInfo={{ showStoreDataOption: true, moveToAnotherTierType: this.state.ruleItem?.MoveToAnotherTierType || Constants.TierTypes.DefaultTier }}
                    getSelecteStorage={this.getSPStorage}
                    resetRetentionInfo={this.resetSPRetentionInfo}
                />
                {(this.enableRecordsArchiver || !isRelatedOrStore) && this.state.selectedRuleModuleType != Constants.RuleModuleTypes.SOArchiver && <RemoveAchived
                    id="raCrSpRemoveArchivedForSp"
                    ref={r => this.removeAchivedRefForSp = r}
                    workflowItems={this.props.workflowItems}
                    isShowManualApproval={this.state.isArchiveToAzureBlobStorage}
                    isShowDeleteStub={!this.state.isArchiveToAzureBlobStorage}
                />}
                {(this.enableRecordsArchiver || !isRelatedOrStore) && this.state.selectedRuleModuleType === Constants.RuleModuleTypes.SOArchiver && <RuleRetention
                    id="raCrSpRemoveArchivedForSp"
                    ref={r => this.removeAchivedRefForSp = r}
                    workflowItems={this.props.workflowItems}
                    isShowManualApproval={this.state.isArchiveToAzureBlobStorage}
                    isShowDeleteStub={!this.state.isArchiveToAzureBlobStorage}
                    defaultStorage={this.props.storagePolicyList.find(item => item.Id === this.props.indexDeviceId)}
                />}
            </div>;
        }
    }

    renderStorageSettingsForTeams = () => {
        const isRelatedOrStore = this.state.isRemoveForTeams && (this.state.isDeleteRelatedRecordOptionForTeams || this.state.isBackupOptionForTeams);
        const isArchiveModule = this.state.selectedRuleModuleType === Constants.RuleModuleTypes.SOArchiver;
        if (
            isRelatedOrStore ||
            this.state.isArchiveToAzureBlobStorageForTeams ||
            (this.state.isBackupAndRemoveForTeams && isArchiveModule)
        ) {
            return (
                <div>
                    <StorageSettings
                        ref={r => this.teamsStorageSettingsRef = r}
                        id={"StorageSettings4Teams"}
                        sourceTab={Constants.RuleSourceTabIndex.Teams}
                        storagePolicyList={this.props.storagePolicyList}
                        storagePolicyId={this.state.ruleItem.TeamsRule?.StoragePolicyId || this.props.indexDeviceId}
                        storeDataTierInfo={{ showStoreDataOption: true, moveToAnotherTierType: this.state.ruleItem.TeamsRule?.MoveToAnotherTierType || Constants.TierTypes.DefaultTier }}
                        getSelecteStorage={this.getTeamsStorage}
                        resetRetentionInfo={this.resetTeamsRetentionInfo}
                    />
                    {(this.enableRecordsArchiver || !isRelatedOrStore) && this.state.selectedRuleModuleType != Constants.RuleModuleTypes.SOArchiver && (
                        <RemoveAchived
                            id="raCrRemoveArchivedForTeams"
                            ref={r => this.removeAchivedRefForTeams = r}
                            workflowItems={this.props.workflowItems}
                            isShowManualApproval={this.state.isArchiveToAzureBlobStorageForTeams}
                            isShowDeleteStub={!this.state.isArchiveToAzureBlobStorageForTeams}
                        />
                    )}
                    {(this.enableRecordsArchiver || !isRelatedOrStore) && isArchiveModule && (
                        <RuleRetention
                            id="raCrRemoveArchivedForTeams"
                            ref={r => this.removeAchivedRefForTeams = r}
                            workflowItems={this.props.workflowItems}
                            isShowManualApproval={this.state.isArchiveToAzureBlobStorageForTeams}
                            isShowDeleteStub={!this.state.isArchiveToAzureBlobStorageForTeams}
                            defaultStorage={this.props.storagePolicyList.find(item => item.Id === this.props.indexDeviceId)}
                        />
                    )}
                </div>
            );
        }
    }

    renderStorageSettingsForOndrive() {
        let isStoreForOneDrive = this.state.isRemoveForOneDrive && this.state.isBackupOptionForOneDrive;
        if (
            isStoreForOneDrive ||
            this.state.isArchiveToAzureBlobStorageForOneDrive ||
            (this.state.isBackupAndRemoveForOD && this.state.selectedRuleModuleType === Constants.RuleModuleTypes.SOArchiver) ||
            (this.state.isArchiveWithoutDestroyForOneDrive && this.state.selectedRuleModuleType === Constants.RuleModuleTypes.SOArchiver)
        ) {
            return <div>
                <StorageSettings
                    ref={r => this.oneDriveStorageSettingsRef = r}
                    id={"StorageSettings4OD"}
                    sourceTab={Constants.RuleSourceTabIndex.OneDrive}
                    storagePolicyList={this.props.storagePolicyList}
                    storagePolicyId={this.state.ruleItem.OneDriveRule?.StoragePolicyId || this.props.indexDeviceId}
                    storeDataTierInfo={{ showStoreDataOption: true, moveToAnotherTierType: this.state.ruleItem.OneDriveRule?.MoveToAnotherTierType || Constants.TierTypes.DefaultTier }}
                    getSelecteStorage={this.getODStorage}
                    resetRetentionInfo={this.resetODRetentionInfo}
                />
                {(this.enableRecordsArchiver || !isStoreForOneDrive) && this.state.selectedRuleModuleType != Constants.RuleModuleTypes.SOArchiver && <RemoveAchived
                    id="raCrRemoveArchivedForOd"
                    ref={r => this.removeAchivedRefForOd = r}
                    workflowItems={this.props.workflowItems}
                    isShowManualApproval={this.state.isArchiveToAzureBlobStorageForOneDrive}
                    isShowDeleteStub={!this.state.isArchiveToAzureBlobStorageForOneDrive}
                />}
                {(this.enableRecordsArchiver || !isStoreForOneDrive) && this.state.selectedRuleModuleType === Constants.RuleModuleTypes.SOArchiver && <RuleRetention
                    id="raCrRemoveArchivedForOd"
                    ref={r => this.removeAchivedRefForOd = r}
                    workflowItems={this.props.workflowItems}
                    isShowManualApproval={this.state.isArchiveToAzureBlobStorageForOneDrive}
                    isShowDeleteStub={!this.state.isArchiveToAzureBlobStorageForOneDrive}
                />}
            </div>;
        }
    }

    renderStorageSettingsForPhy() {
        let isRelatedForPhy = this.state.isPhyRemove && this.state.isDeleteRelatedRecordOptionOfPhy;
        if (isRelatedForPhy) {
            return <div>
                <StorageSettings
                    ref={r => this.phyStorageSettingsRef = r}
                    id={"StorageSettings4Phy"}
                    sourceTab={Constants.RuleSourceTabIndex.Physical}
                    storagePolicyList={this.props.storagePolicyList}
                    storagePolicyId={this.state.ruleItem.PhysicalRule?.StoragePolicyId || this.props.indexDeviceId}
                    storeDataTierInfo={{ showStoreDataOption: false, moveToAnotherTierType: this.state.ruleItem.PhysicalRule?.MoveToAnotherTierType }}
                    getSelecteStorage={this.getPhyStorage}
                />
                {(this.enableRecordsArchiver || !isRelatedForPhy) && <RemoveAchived
                    id="raCrRemoveArchivedForPhy"
                    ref={r => this.removeAchivedRefForPhy = r}
                    workflowItems={[]}
                    isShowManualApproval={false}
                    isShowDeleteStub={true}
                />}
            </div>;
        }
    }

    renderStorageSettingsForFS() {
        let selArchiveToAzureBlobStorage = this.state.isArchiveToAzureBlobStorageForFS;
        if (selArchiveToAzureBlobStorage) {
            return <div>
                <StorageSettings
                    ref={r => this.fsStorageSettingsRef = r}
                    id={"StorageSettings4FS"}
                    sourceTab={Constants.RuleSourceTabIndex.FS}
                    storagePolicyList={this.props.storagePolicyList}
                    storagePolicyId={this.state.ruleItem.FSRule?.StoragePolicyId || this.props.indexDeviceId}
                    storeDataTierInfo={{ showStoreDataOption: false, moveToAnotherTierType: this.state.ruleItem.FSRule?.MoveToAnotherTierType }}
                />
            </div>;
        }
    }

    renderStorageSettingsForGoogle() {
        let moveToAnotherTierType = null;
        if (this.state.ruleItem.GoogleDriveRule?.MoveToAnotherTierType) {
            moveToAnotherTierType = this.state.ruleItem.GoogleDriveRule?.MoveToAnotherTierType;
        }

        if (this.state.isArchiveToStorageForGoogle) {
            return <div>
                <StorageSettings
                    ref={r => this.googleStorageSettingsRef = r}
                    id={"StorageSettingsForGoogle"}
                    sourceTab={Constants.RuleSourceTabIndex.GoogleDrive}
                    storagePolicyList={this.props.storagePolicyList}
                    storagePolicyId={this.state.ruleItem.GoogleDriveRule?.StoragePolicyId || this.props.indexDeviceId}
                    storeDataTierInfo={{ showStoreDataOption: true, moveToAnotherTierType: this.state.ruleItem.GoogleDriveRule?.MoveToAnotherTierType || Constants.TierTypes.DefaultTier }}
                    getSelecteStorage={this.getGoogleStorage}
                />
                {this.enableRecordsArchiver && this.state.selectedRuleModuleType != Constants.RuleModuleTypes.SOArchiver &&
                    <RemoveAchived
                        id="raCrRemoveArchivedForGoogle"
                        ref={r => this.removeAchivedRefForGoogle = r}
                        workflowItems={this.props.workflowItems}
                        isShowManualApproval={false}
                        isShowDeleteStub={true}
                        onlySoftDelete={true}
                    />
                }
            </div>;
        }
    }

    renderDescDeclarationAndTaggingForOD = () => {
        if (this.levelId === Constants.RuleLevel.Folder) {
            return RMResx.RM_JS_Rule_KeepActionDesc_TagContent_Folder_OD;
        } else if (this.enableRecordsArchiver && this.supportingLevelsForRecordsLabel.includes(this.levelId)) {
            return RMResx.RM_JS_Rule_KeepActionDesc_TagContent_OD;
        }
        return RMResx.RM_JS_Rule_KeepActionDesc_OD;
    }

    renderStubSettingsPanel() {
        return (
            <R.Panel
                id="raStubSettingsPanel"
                header={RMResx.RM_JS_Rule_Stub_PanelTitle_CreateTemplate}
                size={670}
                status={{ show: this.state.showStubSettingsPanel }}
                destroy={true}
                onClose={this.onCancelStubSettingsPanel}
            >
                <StubPanel
                    id="stubSettingsPanel"
                    cellStubId={null}
                    recordsLabelValue={this.state.recordsLabelValue}
                ></StubPanel>
                <>
                    <R.Button
                        slot="buttons"
                        text={RMResx.RM_JS_Common_Cancel}
                        onClick={this.onCancelStubSettingsPanel}
                    />
                    <R.Button
                        slot="buttons"
                        primary
                        classify="theme"
                        text={RMResx.RM_JS_Common_Save}
                        onClick={this.onSaveStubSettings}
                    />
                </>
            </R.Panel>
        );
    }

    renderKeepTooltipContent = () => {
        if (this.levelId === Constants.RuleLevel.Folder) {
            return RMResx.RM_JS_Rule_KeepActionDesc_Folder;
        }

        if (!this.is21VEnv && this.enableRecordsArchiver && this.supportingLevelsForRecordsLabel.includes(this.levelId)) {
            return RMResx.RM_JS_Rule_KeepActionDesc_TagContent_OD;
        }

        return RMResx.RM_JS_Rule_KeepActionDesc;
    }

    renderNewRetentionLabelOption = () => {
        return (
            <div id="keep_tag_retention">
                <div style={{ textWrap: "nowrap" }}>
                    <div className="flex align-center">
                        <R.Checkbox
                            text={RMResx.RM_RDM_CreateRule_Options_Label}
                            disabled={this.state.elementsEnable}
                            checked={this.state.retentionActionChecked}
                            onChange={this.onRetentionActionCheckChange}
                        />
                        <$g.Popover>{RMResx.RM_JS_Rule_SP_KeepAction_RetentionDesc}</$g.Popover>
                    </div>
                    {this.state.retentionActionChecked && (
                        <>
                            <div className="margin-top-xs margin-left-l">
                                <R.Radio.Group
                                    block
                                    name="records-label-setting"
                                    items={this.state.retentionRecordsLabelOptions}
                                    onChange={this.onRetentionRecordsLabelOptionsChange}
                                />
                            </div>
                            <div className="margin-left-l margin-top-xs">
                                <R.Input
                                    id="raCrSpoMetadataRetentionIpt"
                                    className="margin-left-l"
                                    width={"stretch"}
                                    placeholder={RMResx.RM_RDM_CreateRule_PlaceHolder_LabelName}
                                    disabled={this.state.retentionRecordsLabelSelected !== Constants.RetentionLabelOptions.Default || this.state.elementsEnable}
                                    value={this.state.retentionAction || ""}
                                    onChange={this.retentionActionChange}
                                    onBlur={this.archiveActionCustomValidate}
                                />
                            </div>
                        </>
                    )}
                </div>
                <div className='sp_tag-metadata-retention-records-label_valid'>
                    <$g.ValidationMsg show={this.state.noRetentionActionValue}>
                        {RMResx.RM_JS_RDM_CreateRule_Validation_noRetentionValue}
                    </$g.ValidationMsg>
                </div>
            </div>
        );
    }

    renderRetentionLabelOption = (isRecordsModule) => {
        if (RM.gData.enviromentName != Enviroments.ChinaNorth) {
            if (this.enableRecordsArchiver && this.supportingLevelsForRecordsLabel.includes(this.levelId)) {
                return this.renderNewRetentionLabelOption();
            }

            return (
                <div id="keep_tag_retention">
                    {isRecordsModule && (
                        <div className="flex ra-flex-align-center" style={{ textWrap: "nowrap" }}>
                            <R.Checkbox
                                text={RMResx.RM_RDM_CreateRule_Options_Label}
                                disabled={this.state.elementsEnable}
                                checked={this.state.retentionActionChecked}
                                onChange={this.onRetentionActionCheckChange} />
                            <R.Input
                                id="raCrSpoMetadataRetentionIpt"
                                className="tag-metadata-retention"
                                width={"100%"}
                                placeholder={RMResx.RM_RDM_CreateRule_PlaceHolder_LabelName}
                                disabled={!this.state.retentionActionChecked || this.state.elementsEnable}
                                value={this.state.retentionAction || ""}
                                onChange={this.retentionActionChange}
                                onBlur={this.archiveActionCustomValidate} />
                            <$g.Popover>{RMResx.RM_JS_Rule_SP_KeepAction_RetentionDesc}</$g.Popover>
                        </div>
                    )}
                    <div className='sp_tag-metadata-retention_valid'>
                        <$g.ValidationMsg show={this.state.noRetentionActionValue}>
                            {RMResx.RM_JS_RDM_CreateRule_Validation_noRetentionValue}
                        </$g.ValidationMsg>
                    </div>
                </div>
            )
        }

        return null;
    }

    renderRetentionLabelOptionForOneDrive = (isRecordsModule) => {
        if (RM.gData.enviromentName != Enviroments.ChinaNorth) {
            if (this.enableRecordsArchiver && this.supportingLevelsForRecordsLabel.includes(this.levelId)) {
                return (
                    <div id="keep_tag_retention">
                        <div style={{ textWrap: "nowrap" }}>
                            <div className="flex align-center">
                                <R.Checkbox
                                    text={RMResx.RM_RDM_CreateRule_Options_Label}
                                    disabled={this.state.elementsEnable}
                                    checked={this.state.retentionActionCheckedForOneDrive}
                                    onChange={this.onRetentionActionCheckChangeForOneDrive}
                                />
                                <$g.Popover>{RMResx.RM_JS_Rule_OD_KeepAction_RetentionDesc}</$g.Popover>
                            </div>
                            {this.state.retentionActionCheckedForOneDrive && (
                                <>
                                    <div className="margin-top-xs margin-left-l">
                                        <R.Radio.Group
                                            block
                                            name="records-label-setting-od"
                                            items={this.state.retentionRecordsLabelOptionsForOneDrive}
                                            onChange={this.onRetentionRecordsLabelOptionsChangeForOD}
                                        />
                                    </div>
                                    <div className="margin-left-l margin-top-xs">
                                        <R.Input
                                            id="raCrSpoMetadataRetentionIpt"
                                            className="margin-left-l"
                                            width={"stretch"}
                                            placeholder={RMResx.RM_RDM_CreateRule_PlaceHolder_LabelName}
                                            disabled={this.state.retentionRecordsLabelSelectedForOneDrive !== Constants.RetentionLabelOptions.Default || this.state.elementsEnable}
                                            value={this.state.retentionActionForOneDrive || ""}
                                            onChange={this.retentionActionChangeForOneDrive}
                                            onBlur={this.oneDriveArchiveActionCustomValidate}
                                        />
                                    </div>
                                </>
                            )}
                        </div>
                        <div className='sp_tag-metadata-retention-records-label_valid'>
                            <$g.ValidationMsg show={this.state.noRetentionActionValueForOneDrive}>
                                {RMResx.RM_JS_RDM_CreateRule_Validation_noRetentionValue}
                            </$g.ValidationMsg>
                        </div>
                    </div>
                );
            }

            return (
                <div id="keep_tag_retention">
                    {isRecordsModule && (
                        <div className="flex ra-flex-align-center" style={{ textWrap: "nowrap" }}>
                            <R.Checkbox
                                text={RMResx.RM_RDM_CreateRule_Options_Label}
                                disabled={this.state.elementsEnable}
                                checked={this.state.retentionActionCheckedForOneDrive}
                                onChange={this.onRetentionActionCheckChangeForOneDrive} />
                            <R.Input
                                id="raCrSpoMetadataRetentionIpt"
                                className="tag-metadata-retention"
                                width={"100%"}
                                placeholder={RMResx.RM_RDM_CreateRule_PlaceHolder_LabelName}
                                disabled={!this.state.retentionActionCheckedForOneDrive || this.state.elementsEnable}
                                value={this.state.retentionActionForOneDrive || ""}
                                onChange={this.retentionActionChangeForOneDrive}
                                onBlur={this.oneDriveArchiveActionCustomValidate} />
                            <$g.Popover>{RMResx.RM_JS_Rule_OD_KeepAction_RetentionDesc}</$g.Popover>
                        </div>
                    )}
                    <div className='sp_tag-metadata-retention_valid'>
                        <$g.ValidationMsg show={this.state.noRetentionActionValueForOneDrive}>
                            {RMResx.RM_JS_RDM_CreateRule_Validation_noRetentionValue}
                        </$g.ValidationMsg>
                    </div>
                </div>
            )
        }

        return null;
    }

    renderPopupWarningBackupData = (callback) => {
        $$.messagedialog(true, {
            width: "550px",
            hideActions: false,
            title: RMResx.RM_JS_Common_Confirmation,
            content: RMResx.RM_JS_RDM_DestroyDataWithoutBackup,
            buttons: [
                {
                    text: RMResx.RM_JS_Common_Cancel,
                    onClick: () => {
                        $$.messagedialog(false);
                    }
                },
                {
                    text: RMResx.RM_JS_Common_OK,
                    primary: true,
                    classify: "theme",
                    onClick: () => {
                        $$.messagedialog(false);
                        callback();
                    }
                }
            ],
        });
    }

    render () {
        let isEXOTab = this.state.ruleCriteriaTabsIndex == Constants.RuleSourceTabIndex.Exchange;
        let isRecordsModule = this.state.selectedRuleModuleType === RuleModuleTypes.Records || this.state.selectedRuleModuleType === RuleModuleTypes.None;
        let isArchiveModule = this.state.selectedRuleModuleType === RuleModuleTypes.SOArchiver;
        const allowDisplayDestroyAction = [this.RuleLevel.Document, Constants.RuleLevel.DocumentVersion];
        const isAllowDeleteToRecyleBin = new Set([this.RuleLevel.Document, Constants.RuleLevel.SiteCollection]).has(this.levelId);
        const isSiteCollectionLevel = this.levelId === Constants.RuleLevel.SiteCollection;

        return <div className='ra-creat-rule' id="raRuleSourceAndRuleSetting">
            <div className='ra-creat-rule-messagebar'>
                <R.Messagebar
                    message={this.state.MessageTipInfo.content}
                    classify={this.state.MessageTipInfo.type}
                    onClose={this.closeMessageBox}
                    status={{ show: this.state.MessageTipInfo.showTip }}
                />
            </div>
            <div id="rm_createRule_container">
                <div id="rm_copyAndNew_container">
                    <div>
                        <div>
                            <R.Tabcontrol
                                active={this.state.selectedSourcesIndexs.indexOf(this.state.ruleCriteriaTabsIndex)}
                                onChange={this.sharePointCriteriaTabClick}>
                                {this.state.isSpSourceChecked && <R.TabPanel disabled={this.state.elementsEnable} tab={RMResx.RM_JS_SPS_TabLabel_SP}></R.TabPanel>}
                                {this.state.isOneDriveSourceChecked && <R.TabPanel disabled={this.state.elementsEnable} tab={RMResx.RM_JS_SPS_TabLabel_OneDrive}></R.TabPanel>}
                                {this.state.isExoSourceChecked && <R.TabPanel disabled={this.state.elementsEnable} tab={RMResx.RM_JS_SPS_TabLabel_EXO}></R.TabPanel>}
                                {this.state.isPhySourceChecked && <R.TabPanel disabled={this.state.elementsEnable} tab={RMResx.RM_JS_SPS_TabLabel_Physical}></R.TabPanel>}
                                {this.state.isFsSourceChecked && <R.TabPanel disabled={this.state.elementsEnable} tab={RMResx.RM_JS_SPS_TabLabel_FS}></R.TabPanel>}
                                {this.state.isSpLocalSourceChecked && <R.TabPanel disabled={this.state.elementsEnable} tab={RMResx.RM_JS_SPS_TabLabel_SPLocal}></R.TabPanel>}
                                {this.state.isAzureFileSourceChecked && <R.TabPanel disabled={this.state.elementsEnable} tab={RMResx.RM_JS_Common_ReportType_AzureFile}></R.TabPanel>}
                                {this.state.isBoxSourceChecked && <R.TabPanel disabled={this.state.elementsEnable} tab={RMResx.RM_JS_SPS_TabLabel_Box}></R.TabPanel>}
                                {this.state.isConnectorSourceChecked && <R.TabPanel disabled={this.state.elementsEnable} tab={RMResx.RM_CP_Connector}></R.TabPanel>}
                                {this.state.IsGoogleDriveSourceChecked && <R.TabPanel disabled={this.state.elementsEnable} tab={RMResx.RM_JS_SPS_TabLabel_GoogleDrive}></R.TabPanel>}
                                {this.state.isTeamsSourceChecked && <R.TabPanel disabled={this.state.elementsEnable} tab={RMResx.RM_JS_SPS_TabLabel_Teams}></R.TabPanel>}
                            </R.Tabcontrol>
                        </div>
                        {isArchiveModule && this.levelId == this.RuleLevel.Document && <div className="ra-createRule-note">
                            <R.Messagebar
                                message={RMResx.RM_RDM_CreateRule_SONote}
                                classify="info"
                                hasClose={false}
                                status={{ show: true }} />
                        </div>}
                        {isArchiveModule && this.levelId == this.RuleLevel.SiteCollection && this.state.ruleCriteriaTabsIndex == Constants.RuleSourceTabIndex.OneDrive && <div className="ra-createRule-note">
                            <R.Messagebar
                                message={RMResx.RM_RDM_CreateRule_OrphanOneDriveNote}
                                classify="info"
                                hasClose={false}
                                status={{ show: true }}
                            />
                        </div>}
                        <div className="margin-top-l">
                            <p style={{ marginTop: 0 }} className="margin-bottom-s strong" tabIndex="0">{RMResx.RM_RDM_Rule_Criteria_Guide}</p>
                            <div style={{ display: (this.state.ruleCriteriaTabsIndex == Constants.RuleSourceTabIndex.SP) ? "block" : "none" }}>
                                <RuleCriteria
                                    id='spCriteria'
                                    itemId='sp'
                                    getCriteriaData={this.getSpCriteriaData}
                                    getIsVerificationPassed={this.getSpIsVerificationPassed}
                                    lastAccessTimeCollection={this.props.lastAccessTimeCollection} />
                            </div>
                            <div style={{ display: (this.state.ruleCriteriaTabsIndex == Constants.RuleSourceTabIndex.Exchange) ? "block" : "none" }}>
                                <RuleCriteria
                                    id='exoCriteria'
                                    itemId='exo'
                                    isNestleCustomize={this.props.isNestleCustomize}
                                    getCriteriaData={this.getExoCriteriaData}
                                    getIsVerificationPassed={this.getExoIsVerificationPassed} />
                            </div>
                            <div style={{ display: (this.state.ruleCriteriaTabsIndex == Constants.RuleSourceTabIndex.Physical) ? "block" : "none" }}>
                                <RuleCriteria
                                    id='phyCriteria'
                                    itemId='phy'
                                    getCriteriaData={this.getPhyCriteriaData}
                                    getIsVerificationPassed={this.getPhyIsVerificationPassed}
                                    onChange={(newCriterias) => this.handleChangeCriterias(newCriterias, Constants.RuleSourceTabIndex.Physical)} />
                            </div>
                            <div style={{ display: (this.state.ruleCriteriaTabsIndex == Constants.RuleSourceTabIndex.FS) ? "block" : "none" }}>
                                <RuleCriteria
                                    id='fsCriteria'
                                    itemId='fs'
                                    getCriteriaData={this.getFsCriteriaData}
                                    getIsVerificationPassed={this.getFsIsVerificationPassed} />
                            </div>
                            <div style={{ display: (this.state.ruleCriteriaTabsIndex == Constants.RuleSourceTabIndex.SPLocal) ? "block" : "none" }}>
                                <RuleCriteria
                                    id='spLocalCriteria'
                                    itemId='spLocal'
                                    getCriteriaData={this.getSpLocalCriteriaData}
                                    getIsVerificationPassed={this.getSpLocalIsVerificationPassed} />
                            </div>
                            <div style={{ display: (this.state.ruleCriteriaTabsIndex == Constants.RuleSourceTabIndex.OneDrive) ? "block" : "none" }}>
                                <RuleCriteria
                                    id='oneDriveCriteria'
                                    itemId='oneDrive'
                                    getCriteriaData={this.getOneDriveCriteriaData}
                                    getIsVerificationPassed={this.getOneDriveIsVerificationPassed}
                                    lastAccessTimeCollection={this.props.lastAccessTimeCollection} />
                            </div>
                            <div style={{ display: (this.state.ruleCriteriaTabsIndex == Constants.RuleSourceTabIndex.AzureFile) ? "block" : "none" }}>
                                <RuleCriteria
                                    id='azureFileCriteria'
                                    itemId='azureFile'
                                    getCriteriaData={this.getAzureFileCriteriaData}
                                    getIsVerificationPassed={this.getAzureFileIsVerificationPassed} />
                            </div>
                            <div style={{ display: (this.state.ruleCriteriaTabsIndex == Constants.RuleSourceTabIndex.Box) ? "block" : "none" }}>
                                <RuleCriteria
                                    id='boxCriteria'
                                    itemId='box'
                                    getCriteriaData={this.getBoxCriteriaData}
                                    getIsVerificationPassed={this.getBoxIsVerificationPassed} />
                            </div>
                            <div style={{ display: (this.state.ruleCriteriaTabsIndex == Constants.RuleSourceTabIndex.GoogleDrive) ? "block" : "none" }}>
                                <RuleCriteria
                                    id='googleDriveCriteria'
                                    itemId='google'
                                    getCriteriaData={this.getGoogleDriveCriteriaData}
                                    getIsVerificationPassed={this.getGoogleIsVerificationPassed} />
                            </div>
                            <div style={{ display: (this.state.ruleCriteriaTabsIndex == Constants.RuleSourceTabIndex.Connector) ? "block" : "none" }}>
                                <RuleCriteria
                                    id='connectorCriteria'
                                    itemId='connector'
                                    getCriteriaData={this.getConnectorCriteriaData}
                                    getIsVerificationPassed={this.getConnectorIsVerificationPassed} />
                            </div>
                            <div style={{ display: (this.state.ruleCriteriaTabsIndex == Constants.RuleSourceTabIndex.Teams) ? "block" : "none" }}>
                                <RuleCriteria
                                    id='teamsCriteria'
                                    itemId='teams'
                                    getCriteriaData={this.getTeamsCriteriaData}
                                    getIsVerificationPassed={this.getTeamsIsVerificationPassed}
                                    lastAccessTimeCollection={this.props.lastAccessTimeCollection} />
                            </div>
                        </div>
                    </div>
                </div>
                {/*What would you like to do with the content?  sharePoint*/}
                <div className="rm_createRule_archiveAction"
                    style={{ display: (this.state.ruleCriteriaTabsIndex == Constants.RuleSourceTabIndex.SP) ? "block" : "none" }}>
                    <div className="ra-createRule-question">
                        <label className="strong" tabIndex="0">{RMResx.RM_RDM_CreateRule_Title_SharepointData}</label>
                    </div>
                    <div className="rm_createRule_archiveAction_container">
                        {/*Remove content from SharePoint and destroy radio*/}
                        {isRecordsModule && <div className='rm_createRule_remove'>
                            <label>
                                <R.Radio
                                    name="ruleActionForSPO"
                                    text={RMResx.RM_JS_RDM_CreateRule_Options_ArchiveAndRemove}
                                    checked={this.state.isRemove}
                                    disabled={this.state.elementsEnable}
                                    onChange={this.removeCheckedChange} />
                            </label>
                            <$g.Popover>{RMResx.RM_JS_Rule_RemoveActionDesc}</$g.Popover>
                            {/*Remove content from SharePoint and destroy 子选项*/}
                            {
                                this.state.isRemove &&
                                <div className="cr-archive-action-children-selection">
                                    {/*Include related records*/}
                                    {
                                        (this.state.isShowDeleteRelatedRecordOption && RM.gData.enviromentName != Enviroments.ChinaNorth) &&
                                        <div id="rm_createRule_deleteRelatedRecordOption">
                                            <label>
                                                <R.Checkbox
                                                    text={RMResx.RM_RDM_CreateRule_DeleteRelatedRecord}
                                                    disabled={this.state.elementsEnable}
                                                    checked={this.state.isDeleteRelatedRecordOption}
                                                    onChange={this.onCheckChange.bind(this, "isDeleteRelatedRecordOption")}
                                                />
                                            </label>
                                            <$g.Popover>{RMResx.RM_JS_Rule_IncludeRelatedRecordsDescription}</$g.Popover>
                                        </div>
                                    }
                                    {/*Include declared records*/}
                                    {
                                        this.state.isShowDeclaredFileOption &&
                                        <div id="rm_crateRule_declaredFile">
                                            <label>
                                                <R.Checkbox
                                                    text={RMResx.RM_RDM_CreateRule_Options_IncludeDeclaredFile}
                                                    disabled={(this.state.isIncludeDeclaredDisable && !this.state.isBackupOption) || this.state.elementsEnable}
                                                    checked={this.state.isDeclaredFile}
                                                    onChange={this.onCheckChange.bind(this, "isDeclaredFile")} />
                                            </label>
                                            <$g.Popover>{RMResx.RM_JS_Rule_IncludeDeclaredFileDescription}</$g.Popover>
                                        </div>
                                    }
                                    {this.supportingLevelsForRecordsLabel.includes(this.levelId) && this.enableRecordsArchiver && !LicenseHelper.Is21VEnv() && this.renderIncludeLockedFileSetting("isIncludeLockedFile")}
                                    {[32, 64].includes(this.levelId) && this.enableRecordsArchiver && RM.gData.enviromentName != Enviroments.ChinaNorth && this.renderSensitiveAndRententionLabelsSetting("isRetentionLabel")}
                                    {this.renderRemoveStoreAndLeaveSubSPO(ruleActionType.Remove)}
                                    {isAllowDeleteToRecyleBin && this.enableRecordsArchiver &&
                                        <div id="rm_crateRule_declaredFile">
                                            <label>
                                                <R.Checkbox
                                                    text={RMResx.RM_JS_Rule_Delete_RecycleBinOption}
                                                    disabled={this.state.elementsEnable}
                                                    checked={this.state.isDeleteToRecycleBinForSPO}
                                                    onChange={this.onCheckChange.bind(this, "isDeleteToRecycleBinForSPO")} />
                                            </label>
                                        </div>
                                    }
                                </div>
                            }
                        </div>}
                        {isRecordsModule && this.state.isSeparateArchive && this.renderArchiveToAzureBlobStorage(ruleActionType.ArchiveToAzureBlobStorage)}
                        {isArchiveModule && <div>
                            {this.renderBackupAndRemove(Constants.RuleSourceTabIndex.SP)}
                            {RM.gData.enableDeleteOnly && allowDisplayDestroyAction.includes(this.levelId) && this.renderRemoveContent(Constants.RuleSourceTabIndex.SP)}
                        </div>}
                        {isArchiveModule && this.renderArchiveWithoutDestroyingForSPO()}
                        {/*"Record Declaration and Tagging"*/}
                        {this.state.isKeepShow &&
                            <div className='rm_createRule_keep'>
                                <div>
                                    <label>
                                        <R.Radio
                                            name="ruleActionForSPO"
                                            text={this.enableRecordsArchiver && !LicenseHelper.Is21VEnv() && this.supportingLevelsForRecordsLabel.includes(this.levelId) ? RMResx.RM_JS_RDM_CreateRule_Options_TagOrLock : RMResx.RM_JS_RDM_CreateRule_Options_ArchiveAndKeep}
                                            disabled={this.state.elementsEnable}
                                            checked={this.state.isKeep}
                                            onChange={this.keepCheckedChange} />
                                    </label>
                                    <$g.Popover>{this.renderKeepTooltipContent()}</$g.Popover>
                                </div>
                                {/*"Record Declaration and Tagging" 子选项*/}
                                {
                                    this.state.isKeep &&
                                    <div className="rm_createRule_keep_container">
                                        {/*Declare each document/item as a SharePoint record*/}
                                        <div id="keep_declareAsRecord"
                                            className={this.state.isShowDeclareOption ? "block" : "none"}>
                                            <label>
                                                <R.Checkbox
                                                    text={RMResx.RM_RDM_CreateRule_Options_DeclareDocumnet}
                                                    disabled={this.state.elementsEnable}
                                                    checked={this.state.isDeclare}
                                                    onChange={this.declareChecked} />
                                            </label>
                                        </div>
                                        {/*Undeclare in-place recordd*/}
                                        <div id="keep_undeclare"
                                            className={(this.state.isShowDeclareOption && this.state.isCSDTenant && isRecordsModule) ? "rm_createRule_undeclare" : "none"}>
                                            <label className='checkbox-label'>
                                                <R.Checkbox
                                                    text={RMResx.RM_RDM_CreateRule_Options_UndeclareDocumnet}
                                                    disabled={this.state.elementsEnable}
                                                    checked={this.state.isUndeclare}
                                                    onChange={this.undeclareChecked} />
                                            </label>
                                        </div>
                                        {/*Tag each document/item with:*/}
                                        <div id="keep_tag" onClick={this.keepTagChecked}>
                                            <label className='checkbox-label'>
                                                <R.Checkbox
                                                    text={this.levelId === Constants.RuleLevel.Folder ? RMResx.RM_RDM_CreateRule_Options_TagFolder : RMResx.RM_RDM_CreateRule_Options_TagDocumnet}
                                                    disabled={this.state.elementsEnable}
                                                    checked={this.state.iskeepTag}
                                                    onChange={this.onCheckChange.bind(this, "iskeepTag")} />
                                            </label>
                                        </div>
                                        {
                                            this.state.iskeepTag &&
                                            <div id="keep_tag_container">
                                                <div id="keep_tag_YesorNo">
                                                    <label className='checkbox-label'>
                                                        <R.Checkbox
                                                            text={RMResx.RM_RDM_CreateRule_Options_Archived}
                                                            disabled={this.state.elementsEnable}
                                                            checked={this.state.isTagYes}
                                                            onChange={this.onCheckChange.bind(this, "isTagYes")} />
                                                    </label>
                                                </div>
                                                <div id="keep_tag_by">
                                                    <label className='checkbox-label'>
                                                        <R.Checkbox
                                                            text={RMResx.RM_RDM_CreateRule_Options_ArchivedBy}
                                                            disabled={this.state.elementsEnable}
                                                            checked={this.state.isTagBy}
                                                            onChange={this.onCheckChange.bind(this, "isTagBy")} />
                                                    </label>
                                                </div>
                                                <div id="keep_tag_time">
                                                    <label className='checkbox-label'>
                                                        <R.Checkbox
                                                            text={RMResx.RM_RDM_CreateRule_Options_ArchivedTime}
                                                            disabled={this.state.elementsEnable}
                                                            checked={this.state.isTagTime}
                                                            onChange={this.onCheckChange.bind(this, "isTagTime")} />
                                                    </label>
                                                </div>
                                                <div id="keep_tag_metadata_container">
                                                    <div id="keep_tag_metadata">
                                                        <label className='checkbox-label'>
                                                            <R.Checkbox
                                                                text={RMResx.RM_RDM_CreateRule_Options_Metadata}
                                                                disabled={this.state.elementsEnable}
                                                                checked={this.state.tagMetadataChecked}
                                                                onChange={this.onCheckChange.bind(this, "tagMetadataChecked")}
                                                            />
                                                        </label>
                                                    </div>
                                                    <div id="metadata_content">
                                                        <R.Combobox
                                                            id="raCrSpoTagType"
                                                            width={"100%"}
                                                            searchable={false}
                                                            textField='Name'
                                                            valueField='id'
                                                            checkedField='Checked'
                                                            items={setCheckedStatus(
                                                                "id", "Checked",
                                                                this.state.tagType.slice(0, 4),
                                                                this.state.slectTagType)}
                                                            onChange={this.tagTypeSelectChanged}
                                                            searchPlaceholder=''
                                                            disabled={!this.state.tagMetadataChecked} />
                                                        <R.Input
                                                            id="raCrSpoMetadataNameIpt"
                                                            className="tag-metadata-column"
                                                            placeholder={RMResx.RM_RDM_CreateRule_PlaceHolder_ColumnName}
                                                            disabled={!this.state.tagMetadataChecked || this.state.elementsEnable}
                                                            value={this.state.metadataName || ""}
                                                            onChange={this.metadataNameChange}
                                                            onBlur={this.archiveActionCustomValidate} />
                                                        {
                                                            this.state.isTagText && <R.Input
                                                                id="raCrSpoMetadataValueIpt"
                                                                disabled={!this.state.tagMetadataChecked || this.state.elementsEnable}
                                                                className='tag-metadata-column'
                                                                placeholder={RMResx.RM_RDM_CreateRule_PlaceHolder_ColumnValue}
                                                                value={this.state.metadataValue || ""}
                                                                onChange={this.metadataValueChange}
                                                                onBlur={this.archiveActionCustomValidate} />
                                                        }
                                                        {
                                                            this.state.isTagBoolean &&
                                                            <div id="keep_tag_metadata_trueOrfalse">
                                                                <R.Combobox
                                                                    id="raCrSpoTagTypeBool"
                                                                    width={"100%"}
                                                                    searchable={false}
                                                                    textField='Name'
                                                                    valueField='Name'
                                                                    checkedField='Checked'
                                                                    items={setCheckedStatus(
                                                                        "id", "Checked",
                                                                        this.state.TrueOrFaseOptions,
                                                                        this.state.selectTagBoolean)}
                                                                    onChange={this.onCurrentStoragePolicyChange}
                                                                    searchPlaceholder=''
                                                                    disabled={!this.state.tagMetadataChecked || this.state.elementsEnable} />
                                                            </div>
                                                        }

                                                        {
                                                            this.state.isTagDate && <div id='keep_tag_metadata_date'>
                                                                <R.Datepicker
                                                                    id="raCrSpoKeepTagMetadataDate"
                                                                    dateTimeFormat={this.dateTimeFormat}
                                                                    selectedDate={this.state.currentDate}
                                                                    disabled={!this.state.tagMetadataChecked}
                                                                    hasTimePicker={true}
                                                                    hasTimeZone={true}
                                                                    onChange={this.metadataDateSelecteChange} />
                                                            </div>
                                                        }
                                                    </div>
                                                    <div className="cr-archive-action-children-selection">
                                                        <$g.ValidationMsg show={this.state.tagMetadataChecked && this.state.noMetadateValue}>
                                                            {RMResx.RM_JS_RDM_CreateRule_Validation_noMetadataValue}
                                                        </$g.ValidationMsg>
                                                        <$g.ValidationMsg show={this.state.noDateValue}>
                                                            {RMResx.RM_JS_RDM_CreateRule_Validation_ConditionBlankDateTime}
                                                        </$g.ValidationMsg>
                                                        <$g.ValidationMsg show={this.state.noNumberValue}>
                                                            {RMResx.RM_JS_RDM_NotNumber}
                                                        </$g.ValidationMsg>
                                                    </div>
                                                </div>
                                                {this.renderRetentionLabelOption(isRecordsModule)}
                                                <$g.ValidationMsg show={this.state.noTags}>
                                                    {RMResx.RM_JS_RDM_CreateRule_Validation_ConditioNoTags}
                                                </$g.ValidationMsg>
                                            </div>
                                        }

                                        <div className='rm_createRule_keep_noSelect'>
                                            {this.state.isShowDeclareOption && <$g.ValidationMsg show={this.state.noSelect}>
                                                {RMResx.RM_JS_RDM_CreateRule_Validation_ConditioNoKeepSelect}
                                            </$g.ValidationMsg>
                                            }
                                            {!this.state.isShowDeclareOption && <$g.ValidationMsg show={this.state.noSelect}>
                                                {RMResx.RM_JS_RDM_CreateRule_Validation_ConditioNoTag}
                                            </$g.ValidationMsg>
                                            }
                                        </div>
                                    </div>
                                }
                            </div>
                        }
                        {
                            this.state.showExportOnly && <div className='rm_createRule_exportOnly'>
                                <div>
                                    <label>
                                        <R.Radio
                                            name="ruleActionForSPO"
                                            text={RMResx.RM_JS_RDM_CreateRule_Options_ExportOnly}
                                            disabled={this.state.elementsEnable}
                                            checked={this.state.isExportOnly}
                                            onChange={this.spExportOnlyCheckedChange} />
                                    </label>
                                    <$g.Popover>{RMResx.RM_JS_Rule_ExportOnlyDesc}</$g.Popover>
                                </div>
                                <div>
                                    {this.state.isExportOnly && <Export
                                        id='spExportOnly'
                                        type={1}
                                        getIsVerificationPassed={this.getSpExportIsPassed}
                                        getIsVerificationLocationPassed={this.getSpExportLocationIsPassed}
                                        getExportDate={this.getSpExportDate}
                                        jumpExportSettings={this.jumpExportSettings}
                                        isExportOnly={true}
                                        ruleLevel={this.levelId}
                                        destinationActiveTab={this.state.destinationActiveTab}
                                        mode={this.isSpSourceChecked && this.state.selectedRuleModuleType === RuleModuleTypes.SOArchiver ? TabIndex.Archive : TabIndex.Records}
                                    />}
                                </div>
                            </div>
                        }
                        {/*Move documents to a new destination library*/}
                        {this.state.isMoveShow &&
                            <div className='rm_createRule_move'>
                                <div>
                                    <label>
                                        <R.Radio
                                            name="ruleActionForSPO"
                                            text={RMResx.RM_JS_RDM_CreateRule_Options_MoveRecord}
                                            checked={this.state.isMove}
                                            disabled={this.state.elementsEnable}
                                            onChange={this.moveCheckedChange} />
                                    </label>
                                    <$g.Popover>{RMResx.RM_JS_Rule_MoveActionDesc}</$g.Popover>
                                </div>
                                {/*"Move documents to a new destination library" 子选项*/}
                                {
                                    <div id="rm_createRule_move_container"
                                        style={{ display: (this.state.isMove) ? "block" : "none" }}>
                                        <div id="moveto-records-panel-sp">
                                            <div id="moveto-records-view-body" className="moveto-records-body">
                                                <div className="main-title"
                                                    tabIndex="0">{RMResx.RM_JS_BCM_Explorer_Move_OptionTitle_SpecifyLocation}</div>
                                                <div id="location-container-sp">
                                                    <div className="location-title">
                                                        <label>
                                                            <R.Radio
                                                                name="ruleActionMoveForSPO"
                                                                text={RMResx.RM_JS_BCM_Explorer_Move_SpecifyLocation}
                                                                checked={this.state.isSpecifyLocation}
                                                                disabled={this.state.elementsEnable}
                                                                onChange={this.locationTypeClick.bind(this, true)}
                                                            />
                                                        </label>
                                                    </div>
                                                    {
                                                        this.state.isSpecifyLocation &&
                                                        <div className="sub-options-container">
                                                            <div className="flex">
                                                                <R.Input
                                                                    id="raCrSpoLocationPathIpt"
                                                                    className="location-path"
                                                                    type="text"
                                                                    aria-label={RMResx.RM_JS_BCM_Explorer_Move_SpecifyLocation_SP_WaterMark}
                                                                    disabled={this.state.elementsEnable}
                                                                    placeholder={RMResx.RM_JS_BCM_Explorer_Move_SpecifyLocation_SP_WaterMark}
                                                                    value={this.state.locationPath || ""}
                                                                    onChange={this.locationPathChange}
                                                                    onBlur={this.archiveActionCustomValidate} />
                                                                <R.Button
                                                                    className="margin-left-s"
                                                                    text={RMResx.RM_RDM_CreateRule_Test}
                                                                    disabled={this.state.elementsEnable}
                                                                    onClick={this.checkLocation} />
                                                            </div>
                                                            <$g.ValidationMsg show={this.state.noLocation}>
                                                                {RMResx.RM_JS_RDM_CreateRule_Validation_NoInputLocaltion}
                                                            </$g.ValidationMsg>
                                                            <div id='location-vlidat-msg'>
                                                                <R.Messagebar
                                                                    message={this.state.LocationVlidat}
                                                                    status={{ show: this.state.isLocationVlidat }}
                                                                    classify={this.state.LocationVlidatype}
                                                                    onClose={this.cancelLocationVlidat} />
                                                            </div>
                                                        </div>
                                                    }
                                                    <div className="location-title">
                                                        <label>
                                                            <R.Radio
                                                                name="ruleActionMoveForSPO"
                                                                text={RMResx.RM_JS_BCM_Explorer_Move_SelectTreeNode}
                                                                checked={!this.state.isSpecifyLocation}
                                                                disabled={this.state.elementsEnable}
                                                                onChange={this.locationTypeClick.bind(this, false)}
                                                            />
                                                        </label>
                                                    </div>
                                                    <div className='ra-tree'
                                                        style={{ display: (this.state.isSpecifyLocation) ? "none" : "block" }}>
                                                        <div className="ra-tree-container">
                                                            {/* this.enableRecordsArchiver: New logic account */}
                                                            {/* 64 is document, 16 is folder */}
                                                            {this.enableRecordsArchiver && LicenseHelper.HasUpgradeTeams() && checkPermission("Source_Teams", RM.UserResources) && [64, 16].includes(this.levelId) ? (
                                                                <div style={{ padding: "12px 20px" }}>
                                                                    <R.Tabcontrol active={this.state.destinationActiveTab} onChange={this.onDestActiveTabChange}>
                                                                        <R.TabPanel tab={RMResx.RM_JS_BCM_Explorer_Move_SharePoint_Tab}>
                                                                            <div className="destination-tab">  
                                                                                <SPDestinationTree
                                                                                    ref={r => this.ruleMoveTree = r}
                                                                                    treeData={this.state.destinationTreeData}   
                                                                                    mode={this.isSpSourceChecked && this.state.selectedRuleModuleType === RuleModuleTypes.SOArchiver ? TabIndex.Archive : TabIndex.Records}
                                                                                    onSelectedNodeChanged={this.onDestTreeSelectedChanged} />
                                                                            </div>
                                                                        </R.TabPanel>
                                                                        <R.TabPanel tab={RMResx.RM_JS_BCM_Explorer_Move_Teams_Tab}>
                                                                            <div className="destination-tab">  
                                                                                <TeamsDestinationTree
                                                                                    ref={r => this.ruleMoveTeamsTree = r}
                                                                                    treeData={this.state.destinationTreeDataForTeams}   
                                                                                    mode={this.isSpSourceChecked && this.state.selectedRuleModuleType === RuleModuleTypes.SOArchiver ? TabIndex.Archive : TabIndex.Records}
                                                                                    onSelectedNodeChanged={this.onDestTreeSelectedChangedForTeams} />
                                                                            </div>
                                                                        </R.TabPanel>
                                                                    </R.Tabcontrol>
                                                                </div>
                                                            ) : (
                                                                <SPDestinationTree
                                                                    ref={r => this.ruleMoveTree = r}
                                                                    treeData={this.state.destinationTreeData}   
                                                                    mode={this.isSpSourceChecked && this.state.selectedRuleModuleType === RuleModuleTypes.SOArchiver ? TabIndex.Archive : TabIndex.Records}
                                                                    onSelectedNodeChanged={this.onDestTreeSelectedChanged} />
                                                            )}
                                                        </div>
                                                        <$g.ValidationMsg show={this.state.noSelectNode}>
                                                            {RMResx.RM_JS_RDM_CreateRule_Validation_NoSelectTreeNode}
                                                        </$g.ValidationMsg>
                                                    </div>
                                                </div>
                                                <div className="file-body">
                                                    <div className="option-title strong"
                                                        tabIndex="0">{RMResx.RM_JS_BCM_Explorer_Move_FileConflictOption_Title}
                                                    </div>
                                                    <div className="option-title"><label>
                                                        <R.Radio
                                                            text={RMResx.RM_JS_BCM_Explorer_Move_FileConflictOption_Skip}
                                                            name='SPO_Move_FileConflictOption'
                                                            value={this.state.fileNameConflictOptionSkip.toString()}
                                                            disabled={this.state.elementsEnable}
                                                            checked={this.state.currentConflictOptionValue == this.state.fileNameConflictOptionSkip}
                                                            onChange={this.fileConflictOptionChange} />
                                                    </label>
                                                    </div>
                                                    <div className="option-title"><label>
                                                        <R.Radio
                                                            text={RMResx.RM_JS_BCM_Explorer_Move_FileConflictOption_Overwrite}
                                                            name='SPO_Move_FileConflictOption'
                                                            disabled={this.state.elementsEnable}
                                                            value={this.state.fileNameConflictOptionOverwrite.toString()}
                                                            checked={this.state.currentConflictOptionValue == this.state.fileNameConflictOptionOverwrite}
                                                            onChange={this.fileConflictOptionChange} />
                                                    </label>
                                                    </div>
                                                    <div className="option-title"><label>
                                                        <R.Radio
                                                            text={RMResx.RM_JS_BCM_Explorer_Move_FileConflictOption_Rename}
                                                            name='SPO_Move_FileConflictOption'
                                                            disabled={this.state.elementsEnable}
                                                            value={this.state.fileNameConflictOptionRename.toString()}
                                                            checked={this.state.currentConflictOptionValue == this.state.fileNameConflictOptionRename}
                                                            onChange={this.fileConflictOptionChange} />
                                                    </label>
                                                    </div>
                                                </div>
                                                {!LicenseHelper.Is21VEnv() && this.enableRecordsArchiver && this.supportingLevelsForRecordsLabel.includes(this.levelId) ? (
                                                    <div id="rm_createRule_move_declare">
                                                        <label  className='checkbox-label'>
                                                            <R.Checkbox
                                                                text={RMResx.RM_JS_RDM_CreateRule_Options_Move_LockByRecordsLabel}
                                                                checked={this.state.isMoveDeclare}
                                                                disabled={this.state.elementsEnable}
                                                                onChange={this.onCheckChange.bind(this, "isMoveDeclare")} />
                                                        </label>
                                                        <$g.Popover>
                                                            <$g.I18NProvider msg={RMResx.RM_JS_RDM_CreateRule_Options_Move_LockByRecordsLabelDesc}>
                                                                <span>
                                                                    <a
                                                                        className="ra-link-a"
                                                                        href="/Root/CP/GeneralSetting"
                                                                    >
                                                                        {RMResx.RM_JS_SP_MigrateDeclaredRecords_GeneralSetting}
                                                                    </a>
                                                                    <span tabIndex={0}>{`: ${this.state.recordsLabelValue}`}</span>
                                                                </span>
                                                            </$g.I18NProvider>
                                                        </$g.Popover>
                                                    </div>
                                                ) : (
                                                    <div id="rm_createRule_move_declare">
                                                        <label  className='checkbox-label'>
                                                            <R.Checkbox
                                                                text={RMResx.RM_JS_RDM_CreateRule_Options_Move_DeclareRecord}
                                                                checked={this.state.isMoveDeclare}
                                                                disabled={this.state.elementsEnable}
                                                                onChange={this.onCheckChange.bind(this, "isMoveDeclare")} />
                                                        </label>
                                                        {this.state.isMoveDeclare && <div className="rm-createRule-validation-msg" tabIndex="0">
                                                            {RMResx.RM_JS_RDM_CreateRule_Options_Move_DeclareRecord_WarnForOD}
                                                        </div>}
                                                    </div>
                                                )}
                                                {isRecordsModule && <div id="rm_createRule_move_declare">
                                                    <label className='checkbox-label'>
                                                        <R.Checkbox
                                                            text={RMResx.RM_JS_BCM_Rule_Move_IsReclassify}
                                                            checked={this.state.isKeepClassificationSPO}
                                                            disabled={this.state.elementsEnable}
                                                            onChange={this.onCheckChange.bind(this, "isKeepClassificationSPO")} />
                                                    </label>
                                                    {this.enableRecordsArchiver && RM.gData.enviromentName != Enviroments.ChinaNorth && (
                                                        <div style={{ marginTop: 20 }}>
                                                            {this.renderSensitiveAndRententionLabelsSetting("isRetentionLabel")}
                                                        </div>
                                                    )}
                                                </div>}
                                                {isArchiveModule && this.enableRecordsArchiver && <div className="margin-bottom-s">
                                                    {this.levelId == this.RuleLevel.Folder && <div id="rm_createRule_move_declare">
                                                        <label className='checkbox-label'>
                                                            <R.Checkbox
                                                                text={RMResx.RM_JS_RDM_CreateRule_Options_Move_FolderStructure}
                                                                checked={this.state.isKeepFolderStructure}
                                                                disabled={this.state.elementsEnable}
                                                                onChange={this.onCheckChange.bind(this, "isKeepFolderStructure")} />
                                                        </label>
                                                    </div>}
                                                    <div id="rm_createRule_move_declare">
                                                        <label className='checkbox-label'>
                                                            <R.Checkbox
                                                                text={RMResx.RM_JS_RDM_CreateRule_Options_Move_AllVersions}
                                                                checked={this.state.isMoveVersions}
                                                                disabled={this.state.elementsEnable}
                                                                onChange={this.onCheckChange.bind(this, "isMoveVersions")} />
                                                        </label>
                                                    </div>
                                                </div>}
                                            </div>
                                        </div>
                                    </div>
                                }
                            </div>
                        }
                        {this.state.isStoreInM365ArchiveShow && !this.is21VEnv && !this.isGccEnv && (
                            <div className='rm_createRule_storeInM365Archive'>
                                <div>
                                    <label>
                                        <R.Radio
                                            name="ruleActionForSPO"
                                            text={RMResx.RM_JS_RDM_CreateRule_Options_StoreInM365Archive}
                                            checked={this.state.isStoreInM365Archive}
                                            disabled={this.state.elementsEnable}
                                            onChange={this.storeInM365ArchiveCheckedChange} />
                                    </label>
                                    <$g.Popover>{isSiteCollectionLevel
                                        ? RMResx.RM_JS_RDM_CreateRule_Options_StoreInM365Archive_SCLevel_Desc
                                        : RMResx.RM_JS_RDM_CreateRule_Options_StoreInM365Archive_SP_Desc
                                    }</$g.Popover>
                                </div>
                            </div>
                        )}
                    </div>
                </div>

                {/*What would you like to do with the content?  sharePoint local*/}
                <div className="rm_createRule_archiveAction"
                    style={{ display: (this.state.ruleCriteriaTabsIndex == Constants.RuleSourceTabIndex.SPLocal) ? "block" : "none" }}>
                    <div className="ra-createRule-question">
                        <label className="strong" tabIndex="0">{RMResx.RM_RDM_CreateRule_Title_SharepointData}</label>
                    </div>
                    <div className="rm_createRule_archiveAction_container">
                        {/*Remove content from SharePoint and destroy radio*/}
                        <div className='rm_createRule_remove'>
                            <label>
                                <R.Radio
                                    name="ruleActionForSPL"
                                    text={RMResx.RM_JS_RDM_CreateRule_Options_ArchiveAndRemove}
                                    checked={this.state.isRemoveForLocal}
                                    disabled={this.state.elementsEnable}
                                    onChange={this.removeCheckedChangeForLocal} />
                            </label>
                            <$g.Popover>{RMResx.RM_JS_Rule_RemoveActionDesc}</$g.Popover>
                            {/*Remove content from SharePoint and destroy 子选项*/}
                            {
                                this.state.isRemoveForLocal &&
                                <div className="cr-archive-action-children-selection">
                                    {/*Include related records*/}
                                    {
                                        this.state.isShowDeleteRelatedRecordOptionForLocal &&
                                        <div id="rm_createRule_deleteRelatedRecordOption">
                                            <label>
                                                <R.Checkbox
                                                    text={RMResx.RM_RDM_CreateRule_DeleteRelatedRecord}
                                                    type="checkbox"
                                                    disabled={this.state.elementsEnable}
                                                    checked={this.state.isDeleteRelatedRecordOptionForLocal}
                                                    onChange={this.onCheckChange.bind(this, "isDeleteRelatedRecordOptionForLocal")}
                                                />
                                            </label>
                                            <$g.Popover>{RMResx.RM_JS_Rule_IncludeRelatedRecordsDescription}</$g.Popover>
                                        </div>
                                    }
                                    {/*Include declared records*/}
                                    {
                                        this.state.isShowDeclaredFileOptionForLocal &&
                                        <div id="rm_crateRule_declaredFile">
                                            <label>
                                                <R.Checkbox
                                                    text={RMResx.RM_RDM_CreateRule_Options_IncludeDeclaredFile}
                                                    disabled={this.state.isIncludeDeclaredDisableForLocal || this.state.elementsEnable}
                                                    checked={this.state.isDeclaredFileForLocal}
                                                    onChange={this.onCheckChange.bind(this, "isDeclaredFileForLocal")} />
                                            </label>
                                            <$g.Popover>{RMResx.RM_JS_Rule_IncludeDeclaredFileDescription}</$g.Popover>
                                        </div>
                                    }
                                    {/*Leave a stub in place for each document following disposal*/}
                                    {
                                        this.state.isShowLeaveStubOptionForLocal &&
                                        <div id="rm_createRule_leaveStubOption">
                                            <label>
                                                <R.Checkbox
                                                    text={RMResx.RM_RDM_CreateRule_LeaveStubOption}
                                                    disabled={this.state.elementsEnable}
                                                    checked={this.state.isLeaveStubOptionForLocal}
                                                    onChange={this.onCheckChange.bind(this, "isLeaveStubOptionForLocal")} />
                                            </label>
                                            <$g.Popover>{RMResx.RM_JS_Rule_LeaveStubDescription_FS}</$g.Popover>
                                            {this.state.isLeaveStubOptionForLocal && 
                                                <div className="cr-archive-action-children-selection">
                                                    <R.Checkbox
                                                        id="raDelareRecordsChkForSpo"
                                                        text={RMResx.RM_RDM_CreateRule_DeclareLinkFile}
                                                        disabled={this.state.elementsEnable}
                                                        checked={this.state.isDeclareLinkFileForLocal}
                                                        onChange={this.onCheckChange.bind(this, "isDeclareLinkFileForLocal")} />
                                                </div>
                                            }
                                        </div>
                                    }
                                    {/*Archive the content before destroying*/}
                                    {
                                        // <div id="rm_createRule_backupBeforeDestroying">
                                        //     <label className='checkbox-label strong'>
                                        //         <input
                                        //             type="checkbox"
                                        //             disabled={this.state.elementsEnable}
                                        //             checked={this.state.isBackupOption}
                                        //             onChange={this.onCheckChange.bind(this, "isBackupOption")} />
                                        //         <span>{RMResx.RM_RDM_CreateRule_BackupBeforeDestroying}</span>
                                        //     </label>
                                        //     <$g.Popover>{RMResx.RM_RDM_CreateRule_BackupBeforeDestroyingDescription}</$g.Popover>
                                        // </div>
                                    }
                                </div>
                            }
                        </div>
                        {/*"Record Declaration and Tagging"*/}
                        {this.state.isKeepShowForLocal &&
                            <div className='rm_createRule_keep'>
                                <div>
                                    <label>
                                        <R.Radio
                                            name="ruleActionForSPL"
                                            text={RMResx.RM_JS_RDM_CreateRule_Options_ArchiveAndKeep}
                                            disabled={this.state.elementsEnable}
                                            checked={this.state.isKeepForLocal}
                                            onChange={this.keepCheckedChangeForLocal} />
                                    </label>
                                    <$g.Popover>{RMResx.RM_JS_Rule_KeepActionDesc}</$g.Popover>
                                </div>
                                {/*"Record Declaration and Tagging" 子选项*/}
                                {
                                    this.state.isKeepForLocal &&
                                    <div className="rm_createRule_keep_container">
                                        {/*Declare each document/item as a SharePoint record*/}
                                        <div id="keep_declareAsRecord"
                                            className={this.state.isShowDeclareOptionForLocal ? "block" : "none"}>
                                            <label>
                                                <R.Checkbox
                                                    text={RMResx.RM_RDM_CreateRule_Options_DeclareDocumnet}
                                                    disabled={this.state.elementsEnable}
                                                    checked={this.state.isDeclareForLocal}
                                                    onChange={this.declareCheckedForLocal} />
                                            </label>
                                        </div>
                                        {/*Undeclare in-place recordd*/}
                                        <div id="keep_undeclare"
                                            className={(this.state.isShowDeclareOptionForLocal && this.state.isCSDTenant) ? "rm_createRule_undeclare" : "none"}>
                                            <label className='checkbox-label'>
                                                <R.Checkbox
                                                    text={RMResx.RM_RDM_CreateRule_Options_UndeclareDocumnet}
                                                    disabled={this.state.elementsEnable}
                                                    checked={this.state.isUndeclareForLocal}
                                                    onChange={this.undeclareCheckedForLocal} />
                                            </label>
                                        </div>
                                        {/*Tag each document/item with:*/}
                                        <div id="keep_tag" onClick={this.keepTagCheckedForLocal}>
                                            <label className='checkbox-label'>
                                                <R.Checkbox
                                                    text={RMResx.RM_RDM_CreateRule_Options_TagDocumnet}
                                                    disabled={this.state.elementsEnable}
                                                    checked={this.state.iskeepTagForLocal}
                                                    onChange={this.onCheckChange.bind(this, "iskeepTagForLocal")} />
                                            </label>
                                        </div>
                                        {
                                            this.state.iskeepTagForLocal &&
                                            <div id="keep_tag_container">
                                                <div id="keep_tag_YesorNo">
                                                    <label className='checkbox-label'>
                                                        <R.Checkbox
                                                            text={RMResx.RM_RDM_CreateRule_Options_Archived}
                                                            disabled={this.state.elementsEnable}
                                                            checked={this.state.isTagYesForLocal}
                                                            onChange={this.onCheckChange.bind(this, "isTagYesForLocal")} />
                                                    </label>
                                                </div>
                                                <div id="keep_tag_by">
                                                    <label className='checkbox-label'>
                                                        <R.Checkbox
                                                            text={RMResx.RM_RDM_CreateRule_Options_ArchivedBy}
                                                            disabled={this.state.elementsEnable}
                                                            checked={this.state.isTagByForLocal}
                                                            onChange={this.onCheckChange.bind(this, "isTagByForLocal")} />
                                                    </label>
                                                </div>
                                                <div id="keep_tag_time">
                                                    <label className='checkbox-label'>
                                                        <R.Checkbox
                                                            text={RMResx.RM_RDM_CreateRule_Options_ArchivedTime}
                                                            disabled={this.state.elementsEnable}
                                                            checked={this.state.isTagTimeForLocal}
                                                            onChange={this.onCheckChange.bind(this, "isTagTimeForLocal")} />
                                                    </label>
                                                </div>
                                                <div id="keep_tag_metadata_container">
                                                    <div id="keep_tag_metadata">
                                                        <label className='checkbox-label'>
                                                            <R.Checkbox
                                                                text={RMResx.RM_RDM_CreateRule_Options_Metadata}
                                                                disabled={this.state.elementsEnable}
                                                                checked={this.state.tagMetadataCheckedForLocal}
                                                                onChange={this.onCheckChange.bind(this, "tagMetadataCheckedForLocal")}
                                                            />
                                                        </label>
                                                    </div>
                                                    <div id="metadata_content">
                                                        <R.Combobox
                                                            id="raCrLocalTagType"
                                                            width={"100%"}
                                                            searchable={false}
                                                            textField='Name'
                                                            valueField='id'
                                                            checkedField='Checked'
                                                            items={setCheckedStatus(
                                                                "id", "Checked",
                                                                this.state.tagTypeForLocal.slice(0, 4),
                                                                this.state.slectTagTypeForLocal)}
                                                            onChange={this.tagTypeSelectChangedForLocal}
                                                            searchPlaceholder=''
                                                            disabled={!this.state.tagMetadataCheckedForLocal} />
                                                        <R.Input
                                                            id="raCrLocalMetadataNameIpt"
                                                            className="tag-metadata-column"
                                                            placeholder={RMResx.RM_RDM_CreateRule_PlaceHolder_ColumnName}
                                                            disabled={!this.state.tagMetadataCheckedForLocal || this.state.elementsEnable}
                                                            value={this.state.metadataNameForLocal || ""}
                                                            onChange={this.metadataNameChangeForLocal}
                                                            onBlur={this.spLocalArchiveActionCustomValidate} />
                                                        {
                                                            this.state.isTagTextForLocal && <R.Input
                                                                id="raCrLocalMetadataValueIpt"
                                                                disabled={!this.state.tagMetadataCheckedForLocal || this.state.elementsEnable}
                                                                className='tag-metadata-column'
                                                                placeholder={RMResx.RM_RDM_CreateRule_PlaceHolder_ColumnValue}
                                                                value={this.state.metadataValueForLocal || ""}
                                                                onChange={this.metadataValueChangeForLocal}
                                                                onBlur={this.spLocalArchiveActionCustomValidate} />
                                                        }
                                                        {
                                                            this.state.isTagBooleanForLocal &&
                                                            <div id="keep_tag_metadata_trueOrfalse">
                                                                <R.Combobox
                                                                    id="raCrLocalTagTypeBool"
                                                                    width={"100%"}
                                                                    searchable={false}
                                                                    textField='Name'
                                                                    valueField='Name'
                                                                    checkedField='Checked'
                                                                    items={setCheckedStatus(
                                                                        "id", "Checked",
                                                                        this.state.TrueOrFaseOptionsForLocal,
                                                                        this.state.selectTagBooleanForLocal)}
                                                                    onChange={this.onCurrentStoragePolicyChangeForLocal}
                                                                    searchPlaceholder=''
                                                                    disabled={!this.state.tagMetadataCheckedForLocal || this.state.elementsEnable} />
                                                            </div>
                                                        }

                                                        {
                                                            this.state.isTagDateForLocal && <div id='keep_tag_metadata_date'>
                                                                <R.Datepicker
                                                                    id="raCrLocalKeepTagMetadataDate"
                                                                    dateTimeFormat={this.dateTimeFormat}
                                                                    selectedDate={this.state.currentDateForLocal}
                                                                    disabled={!this.state.tagMetadataCheckedForLocal}
                                                                    hasTimePicker={true}
                                                                    hasTimeZone={true}
                                                                    onChange={this.metadataDateSelecteChangeForLocal} />
                                                            </div>
                                                        }
                                                    </div>
                                                    <div className="cr-archive-action-children-selection">
                                                        <$g.ValidationMsg show={this.state.tagMetadataCheckedForLocal && this.state.noMetadateValueForLocal}>
                                                            {RMResx.RM_JS_RDM_CreateRule_Validation_noMetadataValue}
                                                        </$g.ValidationMsg>
                                                        <$g.ValidationMsg show={this.state.noDateValueForLocal}>
                                                            {RMResx.RM_JS_RDM_CreateRule_Validation_ConditionBlankDateTime}
                                                        </$g.ValidationMsg>
                                                        <$g.ValidationMsg show={this.state.noNumberValueForLocal}>
                                                            {RMResx.RM_JS_RDM_NotNumber}
                                                        </$g.ValidationMsg>
                                                    </div>
                                                </div>
                                                {RM.gData.enviromentName != Enviroments.ChinaNorth && <div id="keep_tag_retention">
                                                    {/* <label className='checkbox-label strong'>
                                                        <input
                                                            type="checkbox"
                                                            disabled={this.state.elementsEnable}
                                                            checked={this.state.retentionActionCheckedForLocal}
                                                            onChange={this.onRetentionActionCheckChangeForLocal} />
                                                        <span> {RMResx.RM_RDM_CreateRule_Options_Label}:</span>
                                                        <input
                                                            className="tag-metadata-retention"
                                                            placeholder={RMResx.RM_RDM_CreateRule_PlaceHolder_LabelName}
                                                            disabled={!this.state.retentionActionCheckedForLocal || this.state.elementsEnable}
                                                            value={this.state.retentionActionForLocal || ""}
                                                            onChange={this.retentionActionChangeForLocal}
                                                            onBlur={this.spLocalArchiveActionCustomValidate} />
                                                        <$g.Popover>{RMResx.RM_JS_Rule_SP_KeepAction_RetentionDesc}</$g.Popover>
                                                    </label>
                                                    <div className='sp_tag-metadata-retention_valid'>
                                                        <$g.ValidationMsg show={this.state.noRetentionActionValueForLocal}>
                                                            {RMResx.RM_JS_RDM_CreateRule_Validation_noRetentionValue}
                                                        </$g.ValidationMsg>
                                                    </div> */}
                                                    <$g.ValidationMsg show={this.state.noTagsForLocal}>
                                                        {RMResx.RM_JS_RDM_CreateRule_Validation_ConditioNoTags}
                                                    </$g.ValidationMsg>
                                                </div>}
                                            </div>
                                        }

                                        <div className='rm_createRule_keep_noSelect'>
                                            {this.state.isShowDeclareOptionForLocal && <$g.ValidationMsg show={this.state.noSelectForLocal}>
                                                {RMResx.RM_JS_RDM_CreateRule_Validation_ConditioNoKeepSelect}
                                            </$g.ValidationMsg>
                                            }
                                            {!this.state.isShowDeclareOptionForLocal && <$g.ValidationMsg show={this.state.noSelectForLocal}>
                                                {RMResx.RM_JS_RDM_CreateRule_Validation_ConditioNoTag}
                                            </$g.ValidationMsg>
                                            }
                                        </div>
                                    </div>
                                }
                            </div>
                        }
                        {/*Move documents to a new destination library*/}
                        {this.state.isMoveShowForLocal &&
                            <div className='rm_createRule_move'>
                                <div>
                                    <label className="strong">
                                        <input
                                            type="radio"
                                            checked={this.state.isMoveForLocal}
                                            disabled={this.state.elementsEnable}
                                            onChange={this.moveCheckedChangeForLocal} />
                                        <span>{RMResx.RM_JS_RDM_CreateRule_Options_MoveRecord}</span>
                                    </label>
                                    <$g.Popover>{RMResx.RM_JS_Rule_MoveActionDesc}</$g.Popover>
                                </div>
                                {/*"Move documents to a new destination library" 子选项*/}
                                {
                                    <div id="rm_createRule_move_container"
                                        style={{ display: (this.state.isMoveForLocal) ? "block" : "none" }}>
                                        <div id="moveto-records-panel-sp">
                                            <div id="moveto-records-view-body" className="moveto-records-body">
                                                <div className="main-title"
                                                    tabIndex="0">{RMResx.RM_JS_BCM_Explorer_Move_OptionTitle_SpecifyLocation}</div>
                                                <div id="location-container-sp">
                                                    <div className="location-title">
                                                        <label className="strong">
                                                            <input
                                                                type="radio"
                                                                checked={this.state.isSpecifyLocationForLocal}
                                                                tabIndex="0"
                                                                disabled={this.state.elementsEnable}
                                                                onChange={this.locationTypeClickForLocal.bind(this, true)}
                                                            />
                                                            <span>{RMResx.RM_JS_BCM_Explorer_Move_SpecifyLocation}</span>
                                                        </label>
                                                    </div>
                                                    {
                                                        this.state.isSpecifyLocationForLocal &&
                                                        <div className="sub-options-container">
                                                            <input
                                                                className="location-path"
                                                                type="text"
                                                                aria-label={RMResx.RM_JS_BCM_Explorer_Move_SpecifyLocation_SP_WaterMark}
                                                                tabIndex="0"
                                                                disabled={this.state.elementsEnable}
                                                                placeholder={RMResx.RM_JS_BCM_Explorer_Move_SpecifyLocation_SP_WaterMark}
                                                                value={this.state.locationPathForLocal || ""}
                                                                onChange={this.locationPathChangeForLocal}
                                                                onBlur={this.spLoalArchiveActionCustomValidate} />
                                                            <input
                                                                id="location_validateBtn"
                                                                type="button"
                                                                value={RMResx.RM_RDM_CreateRule_Test}
                                                                tabIndex="0"
                                                                disabled={this.state.elementsEnable}
                                                                onClick={this.checkLocationForLocal} />
                                                            <$g.ValidationMsg show={this.state.noLocationForLocal}>
                                                                {RMResx.RM_JS_RDM_CreateRule_Validation_NoInputLocaltion}
                                                            </$g.ValidationMsg>
                                                            <div id='location-vlidat-msg'>
                                                                <R.Messagebar
                                                                    message={this.state.LocationVlidatForLocal}
                                                                    status={{ show: this.state.isLocationVlidatForLocal }}
                                                                    classify={this.state.LocationVlidatypeForLocal}
                                                                    onClose={this.cancelLocationVlidatForLocal} />
                                                            </div>
                                                        </div>
                                                    }
                                                    <div className="location-title">
                                                        <label className="strong">
                                                            <input
                                                                type="radio"
                                                                checked={!this.state.isSpecifyLocationForLocal}
                                                                tabIndex="0"
                                                                disabled={this.state.elementsEnable}
                                                                onChange={this.locationTypeClickForLocal.bind(this, false)}
                                                            />
                                                            <span>{RMResx.RM_JS_BCM_Explorer_Move_SelectTreeNode}</span>
                                                        </label>
                                                    </div>
                                                    <div className='ra-tree'
                                                        style={{ display: (this.state.isSpecifyLocationForLocal) ? "none" : "block" }}>
                                                        <div className="ra-tree-container">
                                                            <SPDestinationTree
                                                                ref={r => this.ruleMoveTree = r}
                                                                treeData={this.state.destinationTreeDataForLocal}
                                                                onSelectedNodeChanged={this.onDestTreeSelectedChangedForLocal} />
                                                        </div>
                                                        <$g.ValidationMsg show={this.state.noSelectNodeForLocal}>
                                                            {RMResx.RM_JS_RDM_CreateRule_Validation_NoSelectTreeNode}
                                                        </$g.ValidationMsg>
                                                    </div>
                                                </div>
                                                <div className="file-body">
                                                    <div className="option-title"
                                                        tabIndex="0">{RMResx.RM_JS_BCM_Explorer_Move_FileConflictOption_Title}
                                                    </div>
                                                    <div className="option-title strong"><label>
                                                        <input
                                                            type="radio"
                                                            tabIndex="0"
                                                            name='SPL_Move_FileConflictOption'
                                                            value={this.state.fileNameConflictOptionSkipForLocal}
                                                            disabled={this.state.elementsEnable}
                                                            checked={this.state.currentConflictOptionValueForLocal == this.state.fileNameConflictOptionSkipForLocal}
                                                            onChange={this.fileConflictOptionChangeForLocal} />
                                                        <span>{RMResx.RM_JS_BCM_Explorer_Move_FileConflictOption_Skip}</span>

                                                    </label>
                                                    </div>
                                                    <div className="option-title strong"><label>
                                                        <input
                                                            type="radio"
                                                            tabIndex="0"
                                                            name='SPL_Move_FileConflictOption'
                                                            disabled={this.state.elementsEnable}
                                                            value={this.state.fileNameConflictOptionOverwriteForLocal}
                                                            checked={this.state.currentConflictOptionValueForLocal == this.state.fileNameConflictOptionOverwriteForLocal}
                                                            onChange={this.fileConflictOptionChangeForLocal} />
                                                        {RMResx.RM_JS_BCM_Explorer_Move_FileConflictOption_Overwrite}
                                                    </label>
                                                    </div>
                                                    <div className="option-title strong"><label>
                                                        <input
                                                            type="radio"
                                                            tabIndex="0"
                                                            name='SPL_Move_FileConflictOption'
                                                            disabled={this.state.elementsEnable}
                                                            value={this.state.fileNameConflictOptionRenameForLocal}
                                                            checked={this.state.currentConflictOptionValueForLocal == this.state.fileNameConflictOptionRenameForLocal}
                                                            onChange={this.fileConflictOptionChangeForLocal} />
                                                        <span>{RMResx.RM_JS_BCM_Explorer_Move_FileConflictOption_Rename}</span>
                                                    </label>
                                                    </div>
                                                </div>
                                                <div id="rm_createRule_move_declare">
                                                    <label  className='checkbox-label strong'>
                                                        <input
                                                            type="checkbox"
                                                            tabIndex="0"
                                                            checked={this.state.isMoveDeclareForLocal}
                                                            disabled={this.state.elementsEnable}
                                                            onChange={this.onCheckChange.bind(this, "isMoveDeclareForLocal")} />
                                                        <span>{RMResx.RM_JS_RDM_CreateRule_Options_Move_DeclareRecord}</span>
                                                    </label>
                                                </div>
                                            </div>
                                        </div>
                                    </div>
                                }
                            </div>
                        }
                    </div>
                </div>

                {/*What would you like to do with the content?  onedrive*/}
                <div className="rm_createRule_archiveAction"
                    style={{ display: (this.state.ruleCriteriaTabsIndex == Constants.RuleSourceTabIndex.OneDrive) ? "block" : "none" }}>
                    <div className="ra-createRule-question">
                        <label className="strong" tabIndex="0">{RMResx.RM_RDM_CreateRule_Title_SharepointData}</label>
                    </div>
                    <div className="rm_createRule_archiveAction_container">
                        {/*Remove content from SharePoint and destroy radio*/}
                        {
                            isRecordsModule && <div className='rm_createRule_remove'>
                                <label>
                                    <R.Radio 
                                        name="ruleActionForOneDrive"
                                        text={RMResx.RM_JS_RDM_CreateRule_Options_ArchiveAndRemove}
                                        checked={this.state.isRemoveForOneDrive}
                                        disabled={this.state.elementsEnable}
                                        onChange={this.removeCheckedChangeForOneDrive} />
                                </label>
                                <$g.Popover>{RMResx.RM_JS_Rule_RemoveActionDesc_OD}</$g.Popover>
                                {/*Remove content from SharePoint and destroy 子选项*/}
                                {
                            this.state.isRemoveForOneDrive &&
                                <div className="cr-archive-action-children-selection">
                                    {/*Include related records*/}
                                    {
                                        this.state.isShowDeleteRelatedRecordOptionForOneDrive &&
                                        <div id="rm_createRule_deleteRelatedRecordOption">
                                            <label>
                                                <R.Checkbox
                                                    text={RMResx.RM_RDM_CreateRule_DeleteRelatedRecord}
                                                    disabled={this.state.elementsEnable}
                                                    checked={this.state.isDeleteRelatedRecordOptionForOneDrive}
                                                    onChange={this.onCheckChange.bind(this, "isDeleteRelatedRecordOptionForOneDrive")}
                                                />
                                            </label>
                                            <$g.Popover>{RMResx.RM_JS_Rule_IncludeRelatedRecordsDescription}</$g.Popover>
                                        </div>
                                    }
                                    {/*Include declared records*/}
                                    {
                                        this.state.isShowDeclaredFileOptionForOneDrive &&
                                        <div id="rm_crateRule_declaredFile">
                                            <label>
                                                <R.Checkbox
                                                    text={RMResx.RM_RDM_CreateRule_Options_IncludeDeclaredFile}
                                                    disabled={(this.state.isIncludeDeclaredDisableForOneDrive && !this.state.isBackupOptionForOneDrive) || this.state.elementsEnable}
                                                    checked={this.state.isDeclaredFileForOneDrive}
                                                    onChange={this.onCheckChange.bind(this, "isDeclaredFileForOneDrive")} />
                                            </label>
                                            <$g.Popover>{RMResx.RM_JS_Rule_IncludeDeclaredFileDescription_OD}</$g.Popover>
                                        </div>
                                    }
                                    {this.supportingLevelsForRecordsLabel.includes(this.levelId) && this.enableRecordsArchiver && !LicenseHelper.Is21VEnv() && this.renderIncludeLockedFileSetting("isIncludeLockedFileForOneDrive", undefined, "isLockRecordForOneDrive")}
                                    {[32, 64].includes(this.levelId) && this.enableRecordsArchiver && RM.gData.enviromentName != Enviroments.ChinaNorth && this.renderSensitiveAndRententionLabelsSetting("isRetentionLabelForOneDrive")}
                                    {this.renderRemoveStoreAndLeaveSubForOneDrive(ruleActionType.Remove)}
                                    {isAllowDeleteToRecyleBin && this.enableRecordsArchiver &&
                                        <div id="rm_crateRule_declaredFile">
                                            <label>
                                                <R.Checkbox
                                                    text={RMResx.RM_JS_Rule_Delete_RecycleBinOption}
                                                    disabled={this.state.elementsEnable}
                                                    checked={this.state.isDeleteToRecycleBinForOneDrive}
                                                    onChange={this.onCheckChange.bind(this, "isDeleteToRecycleBinForOneDrive")} />
                                            </label>
                                        </div>
                                    }
                                </div>
                                }
                            </div>
                        }
                        {isRecordsModule && this.state.isSeparateArchive && this.renderArchiveToAzureBlobStorageForOneDrive(ruleActionType.ArchiveToAzureBlobStorage)}
                        {isArchiveModule && <div>
                            {this.renderBackupAndRemove(Constants.RuleSourceTabIndex.OneDrive)}
                            {RM.gData.enableDeleteOnly && allowDisplayDestroyAction.includes(this.levelId) && this.renderRemoveContent(Constants.RuleSourceTabIndex.OneDrive)}
                        </div>}
                        {isArchiveModule && this.renderArchiveWithoutDestroyingForOneDrive()}
                        {/*"Record Declaration and Tagging"*/}
                        {this.state.isKeepShowForOneDrive &&
                            <div className='rm_createRule_keep'>
                                <div>
                                    <label>
                                        <R.Radio
                                            name="ruleActionForOneDrive"
                                            text={this.enableRecordsArchiver && !LicenseHelper.Is21VEnv() && this.supportingLevelsForRecordsLabel.includes(this.levelId) ? RMResx.RM_JS_RDM_CreateRule_Options_TagOrLock : RMResx.RM_JS_RDM_CreateRule_Options_ArchiveAndKeep_TagContent}
                                            disabled={this.state.elementsEnable}
                                            checked={this.state.isKeepForOneDrive}
                                            onChange={this.keepCheckedChangeForOneDrive} />
                                    </label>
                                    <$g.Popover>{this.renderDescDeclarationAndTaggingForOD()}</$g.Popover>
                                </div>
                                {/*"Record Declaration and Tagging" 子选项*/}
                                {/** RECO-30632: Remove declare option for OneDrive */}
                                {
                                    this.state.isKeepForOneDrive &&
                                    <div className="rm_createRule_keep_container">
                                        {/*Declare each document/item as a SharePoint record*/}
                                        {!this.enableRecordsArchiver &&
                                            <>
                                                <div id="keep_declareAsRecord"
                                                    className={this.state.isShowDeclareOptionForOneDrive ? "block" : "none"}>
                                                    <label>
                                                        <R.Checkbox
                                                            text={RMResx.RM_RDM_CreateRule_Options_DeclareDocumnet}
                                                            disabled={this.state.elementsEnable}
                                                            checked={this.state.isDeclareForOneDrive}
                                                            onChange={this.declareCheckedForOneDrive} />
                                                    </label>
                                                    {this.state.isDeclareForOneDrive && <div className="rm-createRule-validation-msg" tabIndex="0">
                                                        {RMResx.RM_RDM_CreateRule_Options_DeclareDocumnet_WarnForOD}
                                                    </div>}
                                                </div>
                                                {/*Undeclare in-place recordd*/}
                                                <div id="keep_undeclare"
                                                    className={(this.state.isShowDeclareOptionForOneDrive && this.state.isCSDTenant && isRecordsModule) ? "rm_createRule_undeclare" : "none"}>
                                                    <label className='checkbox-label'>
                                                        <R.Checkbox
                                                            text={RMResx.RM_RDM_CreateRule_Options_UndeclareDocumnet}
                                                            disabled={this.state.elementsEnable}
                                                            checked={this.state.isUndeclareForOneDrive}
                                                            onChange={this.undeclareCheckedForOneDrive} />
                                                    </label>
                                                </div>
                                            </>
                                        }

                                        {/*Tag each document/item with:*/}
                                        <div id="keep_tag" onClick={this.keepTagCheckedForOneDrive}>
                                            <label className='checkbox-label'>
                                                <R.Checkbox
                                                    text={this.levelId === Constants.RuleLevel.Folder ? RMResx.RM_RDM_CreateRule_Options_TagFolder : RMResx.RM_RDM_CreateRule_Options_TagDocumnet}
                                                    disabled={this.state.elementsEnable}
                                                    checked={this.state.iskeepTagForOneDrive}
                                                    onChange={this.onCheckChange.bind(this, "iskeepTagForOneDrive")} />
                                            </label>
                                        </div>
                                        {
                                            this.state.iskeepTagForOneDrive &&
                                            <div id="keep_tag_container">
                                                <div id="keep_tag_YesorNo">
                                                    <label className='checkbox-label'>
                                                        <R.Checkbox
                                                            text={RMResx.RM_RDM_CreateRule_Options_Archived}
                                                            disabled={this.state.elementsEnable}
                                                            checked={this.state.isTagYesForOneDrive}
                                                            onChange={this.onCheckChange.bind(this, "isTagYesForOneDrive")} />
                                                    </label>
                                                </div>
                                                <div id="keep_tag_by">
                                                    <label className='checkbox-label'>
                                                        <R.Checkbox
                                                            text={RMResx.RM_RDM_CreateRule_Options_ArchivedBy}
                                                            disabled={this.state.elementsEnable}
                                                            checked={this.state.isTagByForOneDrive}
                                                            onChange={this.onCheckChange.bind(this, "isTagByForOneDrive")} />
                                                    </label>
                                                </div>
                                                <div id="keep_tag_time">
                                                    <label className='checkbox-label'>
                                                        <R.Checkbox
                                                            text={RMResx.RM_RDM_CreateRule_Options_ArchivedTime}
                                                            disabled={this.state.elementsEnable}
                                                            checked={this.state.isTagTimeForOneDrive}
                                                            onChange={this.onCheckChange.bind(this, "isTagTimeForOneDrive")} />
                                                    </label>
                                                </div>
                                                <div id="keep_tag_metadata_container">
                                                    <div id="keep_tag_metadata">
                                                        <label className='checkbox-label'>
                                                            <R.Checkbox
                                                                text={RMResx.RM_RDM_CreateRule_Options_Metadata}
                                                                disabled={this.state.elementsEnable}
                                                                checked={this.state.tagMetadataCheckedForOneDrive}
                                                                onChange={this.onCheckChange.bind(this, "tagMetadataCheckedForOneDrive")}
                                                            />
                                                        </label>
                                                    </div>
                                                    <div id="metadata_content">
                                                        <R.Combobox
                                                            id="raCrOneDriveTagType"
                                                            width={"100%"}
                                                            searchable={false}
                                                            textField='Name'
                                                            valueField='id'
                                                            checkedField='Checked'
                                                            items={setCheckedStatus(
                                                                "id", "Checked",
                                                                this.state.tagTypeForOneDrive.slice(0, 4),
                                                                this.state.slectTagTypeForOneDrive)}
                                                            onChange={this.tagTypeSelectChangedForOneDrive}
                                                            searchPlaceholder=''
                                                            disabled={!this.state.tagMetadataCheckedForOneDrive} />
                                                        <R.Input
                                                            id="raCrOneDriveMetadataNameIpt"
                                                            className="tag-metadata-column"
                                                            placeholder={RMResx.RM_RDM_CreateRule_PlaceHolder_ColumnName}
                                                            disabled={!this.state.tagMetadataCheckedForOneDrive || this.state.elementsEnable}
                                                            value={this.state.metadataNameForOneDrive || ""}
                                                            onChange={this.metadataNameChangeForOneDrive}
                                                            onBlur={this.oneDriveArchiveActionCustomValidate} />
                                                        {
                                                            this.state.isTagTextForOneDrive && <R.Input
                                                                id="raCrLocalMetadataValueIpt"
                                                                disabled={!this.state.tagMetadataCheckedForOneDrive || this.state.elementsEnable}
                                                                className='tag-metadata-column'
                                                                placeholder={RMResx.RM_RDM_CreateRule_PlaceHolder_ColumnValue}
                                                                value={this.state.metadataValueForOneDrive || ""}
                                                                onChange={this.metadataValueChangeForOneDrive}
                                                                onBlur={this.oneDriveArchiveActionCustomValidate} />
                                                        }
                                                        {
                                                            this.state.isTagBooleanForOneDrive &&
                                                            <div id="keep_tag_metadata_trueOrfalse">
                                                                <R.Combobox
                                                                    id="raCrOneDriveTagTypeBool"
                                                                    width={"100%"}
                                                                    searchable={false}
                                                                    textField='Name'
                                                                    valueField='Name'
                                                                    checkedField='Checked'
                                                                    items={setCheckedStatus(
                                                                        "id", "Checked",
                                                                        this.state.TrueOrFaseOptionsForOneDrive,
                                                                        this.state.selectTagBooleanForOneDrive)}
                                                                    onChange={this.onCurrentStoragePolicyChangeForOneDrive}
                                                                    searchPlaceholder=''
                                                                    disabled={!this.state.tagMetadataCheckedForOneDrive || this.state.elementsEnable} />
                                                            </div>
                                                        }

                                                        {
                                                            this.state.isTagDateForOneDrive && <div id='keep_tag_metadata_date'>
                                                                <R.Datepicker
                                                                    id="raCrOneDriveKeepTagMetadataDate"
                                                                    dateTimeFormat={this.dateTimeFormat}
                                                                    selectedDate={this.state.currentDateForOneDrive}
                                                                    disabled={!this.state.tagMetadataCheckedForOneDrive}
                                                                    hasTimePicker={true}
                                                                    hasTimeZone={true}
                                                                    onChange={this.metadataDateSelecteChangeForOneDrive} />
                                                            </div>
                                                        }
                                                    </div>
                                                    <div className="cr-archive-action-children-selection">
                                                        <$g.ValidationMsg show={this.state.tagMetadataCheckedForOneDrive && this.state.noMetadateValueForOneDrive}>
                                                            {RMResx.RM_JS_RDM_CreateRule_Validation_noMetadataValue}
                                                        </$g.ValidationMsg>
                                                        <$g.ValidationMsg show={this.state.noDateValueForOneDrive}>
                                                            {RMResx.RM_JS_RDM_CreateRule_Validation_ConditionBlankDateTime}
                                                        </$g.ValidationMsg>
                                                        <$g.ValidationMsg show={this.state.noNumberValueForOneDrive}>
                                                            {RMResx.RM_JS_RDM_NotNumber}
                                                        </$g.ValidationMsg>
                                                    </div>
                                                </div>
                                                {/* {RM.gData.enviromentName != Enviroments.ChinaNorth && <div id="keep_tag_retention">
                                                    {isRecordsModule && <label className="flex ra-flex-align-center" style={{ textWrap: "nowrap" }}>
                                                        <R.Checkbox
                                                            text={RMResx.RM_RDM_CreateRule_Options_Label}
                                                            disabled={this.state.elementsEnable}
                                                            checked={this.state.retentionActionCheckedForOneDrive}
                                                            onChange={this.onRetentionActionCheckChangeForOneDrive} />
                                                        <R.Input
                                                            id="raCrOneDriveMetadataRetentionIpt"
                                                            className="tag-metadata-retention"
                                                            width={"100%"}
                                                            placeholder={RMResx.RM_RDM_CreateRule_PlaceHolder_LabelName}
                                                            disabled={!this.state.retentionActionCheckedForOneDrive || this.state.elementsEnable}
                                                            value={this.state.retentionActionForOneDrive || ""}
                                                            onChange={this.retentionActionChangeForOneDrive}
                                                            onBlur={this.oneDriveArchiveActionCustomValidate} />
                                                        <$g.Popover>{RMResx.RM_JS_Rule_OD_KeepAction_RetentionDesc}</$g.Popover>
                                                    </label>}
                                                    <div className='sp_tag-metadata-retention_valid'>
                                                        <$g.ValidationMsg show={this.state.noRetentionActionValueForOneDrive}>
                                                            {RMResx.RM_JS_RDM_CreateRule_Validation_noRetentionValue}
                                                        </$g.ValidationMsg>
                                                    </div>
                                                    <$g.ValidationMsg show={this.state.noTagsForOneDrive}>
                                                        {RMResx.RM_JS_RDM_CreateRule_Validation_ConditioNoTags}
                                                    </$g.ValidationMsg>
                                                </div>} */}
                                                {this.renderRetentionLabelOptionForOneDrive(isRecordsModule)}
                                                <$g.ValidationMsg show={this.state.noTagsForOneDrive}>
                                                    {RMResx.RM_JS_RDM_CreateRule_Validation_ConditioNoTags}
                                                </$g.ValidationMsg>
                                            </div>
                                        }

                                        <div className='rm_createRule_keep_noSelect'>
                                            {this.state.isShowDeclareOptionForOneDrive && <$g.ValidationMsg show={this.state.noSelectForOneDrive}>
                                                {RMResx.RM_JS_RDM_CreateRule_Validation_ConditioNoKeepSelect}
                                            </$g.ValidationMsg>
                                            }
                                            {!this.state.isShowDeclareOptionForOneDrive && <$g.ValidationMsg show={this.state.noSelectForOneDrive}>
                                                {RMResx.RM_JS_RDM_CreateRule_Validation_ConditioNoTag}
                                            </$g.ValidationMsg>
                                            }
                                        </div>
                                    </div>
                                }
                            </div>
                        }
                        {
                            this.state.isShowExportOnlyForOneDrive && <div className='rm_createRule_exportOnly'>
                                <div>
                                    <label>
                                        <R.Radio
                                            name="ruleActionForOneDrive"
                                            text={RMResx.RM_JS_RDM_CreateRule_Options_ExportOnly}
                                            disabled={this.state.elementsEnable}
                                            checked={this.state.isExportOnlyForOneDrive}
                                            onChange={this.oneDriveExportOnlyCheckedChange} />
                                    </label>
                                    <$g.Popover>{RMResx.RM_JS_Rule_ExportOnlyDesc}</$g.Popover>
                                </div>
                                <div>
                                    {this.state.isExportOnlyForOneDrive && <Export
                                        id='oneDriveExportOnly'
                                        type={1}
                                        getIsVerificationPassed={this.getOneDriveExportIsPassed}
                                        getIsVerificationLocationPassed={this.getOneDriveExportLocationIsPassed}
                                        getExportDate={this.getOneDriveExportDate}
                                        jumpExportSettings={this.jumpExportSettings}
                                        isExportOnly={true}
                                        ruleLevel={this.levelId}
                                        destinationActiveTab={this.state.destinationActiveTab}
                                        mode={this.isOneDriveSourceChecked && this.state.selectedRuleModuleType === RuleModuleTypes.SOArchiver ? TabIndex.Archive : TabIndex.Records}
                                    />}
                                </div>
                            </div>
                        }
                        {/*Move documents to a new destination library*/}
                        {this.state.isMoveShowForOneDrive &&
                            <div className='rm_createRule_move'>
                                <div>
                                    <label>
                                        <R.Radio
                                            name="ruleActionForOneDrive"
                                            text={RMResx.RM_JS_RDM_CreateRule_Options_MoveRecord}
                                            checked={this.state.isMoveForOneDrive}
                                            disabled={this.state.elementsEnable}
                                            onChange={this.moveCheckedChangeForOneDrive} />
                                    </label>
                                    <$g.Popover>{RMResx.RM_JS_Rule_MoveActionDesc}</$g.Popover>
                                </div>
                                {/*"Move documents to a new destination library" 子选项*/}
                                {
                                    <div id="rm_createRule_move_container"
                                        style={{ display: (this.state.isMoveForOneDrive) ? "block" : "none" }}>
                                        <div id="moveto-records-panel-sp">
                                            <div id="moveto-records-view-body" className="moveto-records-body">
                                                <div className="main-title"
                                                    tabIndex="0">{RMResx.RM_JS_BCM_Explorer_Move_OptionTitle_SpecifyLocation}</div>
                                                <div id="location-container-sp">
                                                    <div className="location-title">
                                                        <label>
                                                            <R.Radio
                                                                name="ruleActionMoveForOneDrive"
                                                                text={RMResx.RM_JS_BCM_Explorer_Move_SpecifyLocation}
                                                                checked={this.state.isSpecifyLocationForOneDrive}
                                                                disabled={this.state.elementsEnable}
                                                                onChange={this.locationTypeClickForOneDrive.bind(this, true)}
                                                            />
                                                        </label>
                                                    </div>
                                                    {
                                                        this.state.isSpecifyLocationForOneDrive &&
                                                        <div className="sub-options-container">
                                                            <div className="flex">
                                                                <R.Input
                                                                    id="raCrOneDriveLocationPathIpt"
                                                                    className="location-path"
                                                                    type="text"
                                                                    aria-label={RMResx.RM_JS_BCM_Explorer_Move_SpecifyLocation_SP_WaterMark}
                                                                    disabled={this.state.elementsEnable}
                                                                    placeholder={RMResx.RM_JS_BCM_Explorer_Move_SpecifyLocation_SP_WaterMark}
                                                                    value={this.state.locationPathForOneDrive || ""}
                                                                    onChange={this.locationPathChangeForOneDrive}
                                                                    onBlur={this.spLoalArchiveActionCustomValidate} />
                                                                <R.Button
                                                                    className="margin-left-s"
                                                                    text={RMResx.RM_RDM_CreateRule_Test}
                                                                    disabled={this.state.elementsEnable}
                                                                    onClick={this.checkLocationForOneDrive} />
                                                            </div>
                                                            <$g.ValidationMsg show={this.state.noLocationForOneDrive}>
                                                                {RMResx.RM_JS_RDM_CreateRule_Validation_NoInputLocaltion}
                                                            </$g.ValidationMsg>
                                                            <div id='location-vlidat-msg'>
                                                                <R.Messagebar
                                                                    message={this.state.LocationVlidatForOneDrive}
                                                                    status={{ show: this.state.isLocationVlidatForOneDrive }}
                                                                    classify={this.state.LocationVlidatypeForOneDrive}
                                                                    onClose={this.cancelLocationVlidatForOneDrive} />
                                                            </div>
                                                        </div>
                                                    }
                                                    <div className="location-title">
                                                        <label>
                                                            <R.Radio
                                                                name="ruleActionMoveForOneDrive"
                                                                text={RMResx.RM_JS_BCM_Explorer_Move_SelectTreeNode}
                                                                checked={!this.state.isSpecifyLocationForOneDrive}
                                                                disabled={this.state.elementsEnable}
                                                                onChange={this.locationTypeClickForOneDrive.bind(this, false)}
                                                            />
                                                        </label>
                                                    </div>
                                                    <div className='ra-tree'
                                                        style={{ display: (this.state.isSpecifyLocationForOneDrive) ? "none" : "block" }}>
                                                        <div className="ra-tree-container">
                                                            {/* this.enableRecordsArchiver: New logic account */}
                                                            {/* 64 is document, 16 is folder */}
                                                            {this.enableRecordsArchiver && LicenseHelper.HasUpgradeTeams() && checkPermission("Source_Teams", RM.UserResources) && [64, 16].includes(this.levelId) ? (
                                                                <div style={{ padding: "12px 20px" }}>
                                                                    <R.Tabcontrol active={this.state.destinationActiveTabForOD} onChange={this.onDestActiveTabChangeForOD}>
                                                                        <R.TabPanel tab={RMResx.RM_JS_BCM_Explorer_Move_SharePoint_Tab}>
                                                                            <div className="destination-tab">  
                                                                                <SPDestinationTree
                                                                                    ref={r => this.ruleMoveOneDriveTree = r}
                                                                                    treeData={this.state.destinationTreeDataForOneDrive}   
                                                                                    mode={this.isOneDriveSourceChecked && this.state.selectedRuleModuleType === RuleModuleTypes.SOArchiver ? TabIndex.Archive : TabIndex.Records}
                                                                                    onSelectedNodeChanged={this.onDestTreeSelectedChangedForOneDrive} />
                                                                            </div>
                                                                        </R.TabPanel>
                                                                        <R.TabPanel tab={RMResx.RM_JS_BCM_Explorer_Move_Teams_Tab}>
                                                                            <div className="destination-tab">  
                                                                                <TeamsDestinationTree
                                                                                    ref={r => this.ruleMoveTeamsTreeForOD = r}
                                                                                    treeData={this.state.destinationTreeDataForTeamsOD}   
                                                                                    mode={this.isOneDriveSourceChecked && this.state.selectedRuleModuleType === RuleModuleTypes.SOArchiver ? TabIndex.Archive : TabIndex.Records}
                                                                                    onSelectedNodeChanged={this.onDestTreeSelectedChangedForTeamsOD} />
                                                                            </div>
                                                                        </R.TabPanel>
                                                                    </R.Tabcontrol>
                                                                </div>
                                                            ) : (
                                                                <SPDestinationTree
                                                                    ref={r => this.ruleMoveOneDriveTree = r}
                                                                    treeData={this.state.destinationTreeDataForOneDrive}   
                                                                    mode={this.isOneDriveSourceChecked && this.state.selectedRuleModuleType === RuleModuleTypes.SOArchiver ? TabIndex.Archive : TabIndex.Records}
                                                                    onSelectedNodeChanged={this.onDestTreeSelectedChangedForOneDrive} />
                                                            )}
                                                        </div>
                                                        <$g.ValidationMsg show={this.state.noSelectNodeForOneDrive}>
                                                            {RMResx.RM_JS_RDM_CreateRule_Validation_NoSelectTreeNode}
                                                        </$g.ValidationMsg>
                                                    </div>
                                                </div>
                                                <div className="file-body">
                                                    <div className="option-title strong"
                                                        tabIndex="0">{RMResx.RM_JS_BCM_Explorer_Move_FileConflictOption_Title}
                                                    </div>
                                                    <div className="option-title"><label>
                                                        <R.Radio
                                                            text={RMResx.RM_JS_BCM_Explorer_Move_FileConflictOption_Skip}
                                                            name='oneDrive_Move_FileConflictOption'
                                                            value={this.state.fileNameConflictOptionSkipForOneDrive}
                                                            disabled={this.state.elementsEnable}
                                                            checked={this.state.currentConflictOptionValueForOneDrive == this.state.fileNameConflictOptionSkipForOneDrive}
                                                            onChange={this.fileConflictOptionChangeForOneDrive} />
                                                    </label>
                                                    </div>
                                                    <div className="option-title"><label>
                                                        <R.Radio
                                                            text={RMResx.RM_JS_BCM_Explorer_Move_FileConflictOption_Overwrite}
                                                            name='oneDrive_Move_FileConflictOption'
                                                            disabled={this.state.elementsEnable}
                                                            value={this.state.fileNameConflictOptionOverwriteForOneDrive}
                                                            checked={this.state.currentConflictOptionValueForOneDrive == this.state.fileNameConflictOptionOverwriteForOneDrive}
                                                            onChange={this.fileConflictOptionChangeForOneDrive} />
                                                    </label>
                                                    </div>
                                                    <div className="option-title"><label>
                                                        <R.Radio
                                                            text={RMResx.RM_JS_BCM_Explorer_Move_FileConflictOption_Rename}
                                                            name='oneDrive_Move_FileConflictOption'
                                                            disabled={this.state.elementsEnable}
                                                            value={this.state.fileNameConflictOptionRenameForOneDrive}
                                                            checked={this.state.currentConflictOptionValueForOneDrive == this.state.fileNameConflictOptionRenameForOneDrive}
                                                            onChange={this.fileConflictOptionChangeForOneDrive} />
                                                    </label>
                                                    </div>
                                                </div>
                                                {!LicenseHelper.Is21VEnv() && this.enableRecordsArchiver && this.supportingLevelsForRecordsLabel.includes(this.levelId) ? (
                                                    <div id="rm_createRule_move_declare">
                                                        <label  className='checkbox-label'>
                                                            <R.Checkbox
                                                                text={RMResx.RM_JS_RDM_CreateRule_Options_Move_LockByRecordsLabel}
                                                                checked={this.state.isMoveDeclareForOneDrive}
                                                                disabled={this.state.elementsEnable}
                                                                onChange={this.onCheckChange.bind(this, "isMoveDeclareForOneDrive")} />
                                                        </label>
                                                        <$g.Popover>
                                                            <$g.I18NProvider msg={RMResx.RM_JS_RDM_CreateRule_Options_Move_LockByRecordsLabelDesc}>
                                                                <span>
                                                                    <a
                                                                        className="ra-link-a"
                                                                        href="/Root/CP/GeneralSetting"
                                                                    >
                                                                        {RMResx.RM_JS_SP_MigrateDeclaredRecords_GeneralSetting}
                                                                    </a>
                                                                    <span tabIndex={0}>{`: ${this.state.recordsLabelValue}`}</span>
                                                                </span>
                                                            </$g.I18NProvider>
                                                        </$g.Popover>
                                                    </div>
                                                ) : (
                                                    <div id="rm_createRule_move_declare">
                                                        <label  className='checkbox-label'>
                                                            <R.Checkbox
                                                                text={RMResx.RM_JS_RDM_CreateRule_Options_Move_DeclareRecord}
                                                                checked={this.state.isMoveDeclareForOneDrive}
                                                                disabled={this.state.elementsEnable}
                                                                onChange={this.onCheckChange.bind(this, "isMoveDeclareForOneDrive")} />
                                                        </label>
                                                        {this.state.isMoveDeclareForOneDrive && <div className="rm-createRule-validation-msg" tabIndex="0">
                                                            {RMResx.RM_JS_RDM_CreateRule_Options_Move_DeclareRecord_WarnForOD}
                                                        </div>}
                                                    </div>
                                                )}
                                                {isRecordsModule && <div id="rm_createRule_move_declare">
                                                    <label className='checkbox-label'>
                                                        <R.Checkbox
                                                            text={RMResx.RM_JS_BCM_Rule_Move_IsReclassify}
                                                            checked={this.state.isKeepClassificationForOneDrive}
                                                            disabled={this.state.elementsEnable}
                                                            onChange={this.onCheckChange.bind(this, "isKeepClassificationForOneDrive")} />
                                                    </label>
                                                    {this.enableRecordsArchiver && RM.gData.enviromentName != Enviroments.ChinaNorth && (
                                                        <div style={{ marginTop: 20 }}>
                                                            {this.renderSensitiveAndRententionLabelsSetting("isRetentionLabelForOneDrive")}
                                                        </div>
                                                    )}
                                                </div>}
                                                {isArchiveModule && this.enableRecordsArchiver && <div>
                                                    {this.levelId == this.RuleLevel.Folder && <div id="rm_createRule_move_declare">
                                                        <label className='checkbox-label'>
                                                            <R.Checkbox
                                                                text={RMResx.RM_JS_RDM_CreateRule_Options_Move_FolderStructure}
                                                                checked={this.state.isKeepFolderStructureForOneDrive}
                                                                disabled={this.state.elementsEnable}
                                                                onChange={this.onCheckChange.bind(this, "isKeepFolderStructureForOneDrive")} />
                                                        </label>
                                                    </div>}
                                                    <div id="rm_createRule_move_declare">
                                                        <label className='checkbox-label'>
                                                            <R.Checkbox
                                                                text={RMResx.RM_JS_RDM_CreateRule_Options_Move_AllVersions}
                                                                checked={this.state.isMoveVersionsForOneDrive}
                                                                disabled={this.state.elementsEnable}
                                                                onChange={this.onCheckChange.bind(this, "isMoveVersionsForOneDrive")} />
                                                        </label>
                                                    </div>
                                                </div>}
                                            </div>
                                        </div>
                                    </div>
                                }
                            </div>
                        }
                    </div>
                </div>


                {/*What would you like to do with the  content?  EXO*/}
                {/* {this.state.ruleCriteriaTabsIndex == 1 && */}
                <div className="rm_createRule_archiveAction" style={{display:isEXOTab ? "block":"none"}}>
                    <div className="ra-createRule-question">
                        <label className="strong" tabIndex="0">{RMResx.RM_RDM_CreateRule_Title_ExchangeData}</label>
                    </div>
                    {/*Remove content and destroy*/}
                    <div>
                        <label>
                            <R.Radio
                                name="ruleActionForExo"
                                text={RMResx.RM_JS_RDM_CreateRule_Options_ExchangeArchiveAndRemove}
                                checked={this.state.isExoRemove}
                                disabled={this.state.elementsEnable}
                                onChange={this.exoRemoveCheckedChange} />
                        </label>
                        <$g.Popover>{RMResx.RM_JS_Rule_ExchangeRemoveActionDesc}</$g.Popover>
                    </div>
                    {/*"Record Declaration and Tagging"*/}
                    {RM.gData.enviromentName !== Enviroments.ChinaNorth &&
                    <div className='rm_createRule_keep'>
                            <div>
                                <label>
                                    <R.Radio
                                        name="ruleActionForExo"
                                        text={RMResx.RM_JS_RDM_CreateRule_Options_ArchiveAndKeep}
                                        disabled={this.state.elementsEnable}
                                        checked={this.state.isExoKeep}
                                        onChange={this.exoKeepCheckedChange} />
                                    <$g.Popover>{RMResx.RM_JS_Rule_EXO_KeepActionDesc}</$g.Popover>
                                </label>
                            </div>
                            {/*"Record Declaration and Tagging" 子选项*/}
                            {
                                this.state.isExoKeep && RM.gData.enviromentName != Enviroments.ChinaNorth &&
                                <div id="exo_keep_tag_container">
                                    <div id="exo_keep_declareAsRecord">
                                        <span style={{ textWrap: "nowrap" }}> {RMResx.RM_RDM_CreateRule_Options_Label}:</span>
                                        <R.Input className="tag-metadata-retention"
                                            id="raCrExoRetentionIpt"
                                            placeholder={RMResx.RM_RDM_CreateRule_PlaceHolder_LabelName}
                                            disabled={this.state.elementsEnable}
                                            value={this.state.exoRetentionAction}
                                            onChange={this.exoRetentionActionChange}
                                            onBlur={this.exoArchiveActionCustomValidate} />
                                        <$g.Popover>{RMResx.RM_JS_Rule_EXO_KeepAction_RetentionDesc}</$g.Popover>
                                    </div>
                                    <div className='exo_tag-metadata-retention_valid'>
                                        <$g.ValidationMsg show={this.state.noExoRetentionActionValue}>
                                            {RMResx.RM_JS_RDM_CreateRule_Validation_noRetentionValue}
                                        </$g.ValidationMsg>
                                    </div>
                                </div>}
                    </div>}
                    <div className='rm_createRule_exportOnly_exo'>
                        <div>
                            <label>
                                <R.Radio
                                    name="ruleActionForExo"
                                    text={RMResx.RM_JS_RDM_CreateRule_Options_ExportOnly}
                                    disabled={this.state.elementsEnable}
                                    checked={this.state.isExoExportOnly}
                                    onChange={this.exoExportOnlyCheckedChange} />
                            </label>
                            <$g.Popover>{RMResx.RM_JS_Rule_ExportOnlyDesc}</$g.Popover>
                        </div>
                        <div>
                            {this.state.isExoExportOnly && <Export
                                id='exoExportOnly'
                                type={3}
                                getIsVerificationPassed={this.getExoExportIsPassed}
                                getIsVerificationLocationPassed={this.getExoExportLocationIsPassed}
                                getExportDate={this.getExoExportDate}
                                jumpExportSettings={this.jumpExportSettings}
                                isExportOnly={true}
                                destinationActiveTab={this.state.destinationActiveTab}
                                ruleLevel={this.levelId}
                            />}
                        </div>
                    </div>
                    {/*Move documents to a new destination library*/}
                    <div className='rm_createRule_move'>
                        <div>
                            <label>
                                <R.Radio
                                    name="ruleActionForExo"
                                    text={RMResx.RM_JS_RDM_CreateRule_Options_MoveRecord}
                                    checked={this.state.isExoMove}
                                    disabled={this.state.elementsEnable}
                                    onChange={this.exoMoveCheckedChange} />
                            </label>
                            <$g.Popover>{RMResx.RM_JS_Rule_ExoMoveActionDesc}</$g.Popover>
                        </div>
                        {/*"Move documents to a new destination library" 子选项*/}
                        {
                            <div id="rm_createRule_move_container"
                                style={{ display: (this.state.isExoMove) ? "block" : "none" }}>
                                <div id="moveto-records-panel-exo">
                                    <div id="moveto-records-view-body" className="moveto-records-body">
                                        <div className="main-title"
                                            tabIndex="0">{RMResx.RM_JS_BCM_Explorer_Move_OptionTitle_SpecifyLocation}</div>
                                        <div id="location-container-exo">
                                            <div className="location-title">
                                                <label>
                                                    <R.Radio
                                                        name="ruleActionMoveForExo"
                                                        text={RMResx.RM_JS_BCM_Explorer_Move_SpecifyLocation}
                                                        checked={this.state.isExoSpecifyLocation}
                                                        disabled={this.state.elementsEnable}
                                                        onChange={this.exoLocationTypeClick.bind(this, true)}
                                                    />
                                                </label>
                                            </div>
                                            {
                                                this.state.isExoSpecifyLocation &&
                                                <div className="sub-options-container">
                                                    <div className="flex">
                                                        <R.Input
                                                            id="raCrExoLocationPathIpt"
                                                            className="location-path"
                                                            type="text"
                                                            aria-label={RMResx.RM_JS_BCM_Explorer_Move_SpecifyLocation_SP_WaterMark}
                                                            tabIndex="0"
                                                            disabled={this.state.elementsEnable}
                                                            placeholder={RMResx.RM_JS_BCM_Explorer_Move_SpecifyLocation_SP_WaterMark}
                                                            value={this.state.exoLocationPath || ""}
                                                            onChange={this.exoLocationPathChange}
                                                            onBlur={this.exoArchiveActionCustomValidate} />
                                                        <R.Button
                                                            className="margin-left-s"
                                                            text={RMResx.RM_RDM_CreateRule_Test}
                                                            disabled={this.state.elementsEnable}
                                                            onClick={this.checkExoLocation} />
                                                    </div>
                                                    <$g.ValidationMsg show={this.state.noExoLocation}>
                                                        {RMResx.RM_JS_RDM_CreateRule_Validation_NoInputLocaltion}
                                                    </$g.ValidationMsg>
                                                    <div id='location-vlidat-msg'>
                                                        <R.Messagebar
                                                            message={this.state.ExoLocationVlidat}
                                                            status={{ show: this.state.isExoLocationVlidat }}
                                                            classify={this.state.ExoLocationVlidatype}
                                                            onClose={this.cancelExoLocationValidate} />
                                                    </div>
                                                </div>
                                            }
                                            <div className="location-title">
                                                <label>
                                                    <R.Radio
                                                        name="ruleActionMoveForExo"
                                                        text={RMResx.RM_JS_BCM_Explorer_Move_SelectTreeNode}
                                                        checked={!this.state.isExoSpecifyLocation}
                                                        disabled={this.state.elementsEnable}
                                                        onChange={this.exoLocationTypeClick.bind(this, false)}
                                                    />
                                                </label>
                                            </div>
                                            <div className='ra-tree'
                                                style={{ display: (this.state.isExoSpecifyLocation) ? "none" : "block" }}>
                                                <div className="ra-tree-container">
                                                    {/* this.enableRecordsArchiver: New logic account */}
                                                    {/* 64 is document, 16 is folder */}
                                                    {this.enableRecordsArchiver && LicenseHelper.HasUpgradeTeams() && checkPermission("Source_Teams", RM.UserResources) && [64, 16].includes(this.levelId) ? (
                                                        <div style={{ padding: "12px 20px" }}>
                                                            <R.Tabcontrol active={this.state.destinationActiveTabForEXO} onChange={this.onDestActiveTabChangeForEXO}>
                                                                <R.TabPanel tab={RMResx.RM_JS_BCM_Explorer_Move_SharePoint_Tab}>
                                                                    <div className="destination-tab">  
                                                                        <SPDestinationTree
                                                                            ref={r => this.ruleMoveExoTree = r}
                                                                            treeData={this.state.exoDestinationTreeData}
                                                                            onSelectedNodeChanged={this.onDestExoTreeSelectedChanged} />
                                                                    </div>
                                                                </R.TabPanel>
                                                                <R.TabPanel tab={RMResx.RM_JS_BCM_Explorer_Move_Teams_Tab}>
                                                                    <div className="destination-tab">  
                                                                        <TeamsDestinationTree
                                                                            ref={r => this.ruleMoveTeamsTreeForEXO = r}
                                                                            treeData={this.state.destinationTreeDataForTeamsEXO}
                                                                            onSelectedNodeChanged={this.onDestTreeSelectedChangedForTeamsEXO} />
                                                                    </div>
                                                                </R.TabPanel>
                                                            </R.Tabcontrol>
                                                        </div>
                                                    ) : (
                                                        <SPDestinationTree
                                                            ref={r => this.ruleMoveExoTree = r}
                                                            treeData={this.state.exoDestinationTreeData}
                                                            onSelectedNodeChanged={this.onDestExoTreeSelectedChanged} />
                                                    )}
                                                </div>
                                                <$g.ValidationMsg show={this.state.noExoSelectNode}>
                                                    {RMResx.RM_JS_RDM_CreateRule_Validation_NoSelectTreeNode}
                                                </$g.ValidationMsg>
                                            </div>
                                            {this.enableRecordsArchiver && <div>
                                                <div className="location-title">
                                                    <label>
                                                        <R.Checkbox
                                                            text={RMResx.RM_JS_BCM_Explorer_ExoMoveToSP_CheckboxTitle}
                                                            checked={this.state.isExoMoveToSP}
                                                            disabled={this.state.elementsEnable}
                                                            onChange={this.onCheckChange.bind(this, "isExoMoveToSP")}
                                                        />
                                                    </label>
                                                </div>
                                                {this.state.isExoMoveToSP && <div className="location-title">
                                                    <ExoMoveToSP
                                                        ref={r => this.exoMoveToSPRef = r}
                                                        moveToSPDataList={this.state.moveToSPDataList}
                                                    />
                                                </div>}
                                            </div>}
                                        </div>
                                        <div className="file-body">
                                            <div className="option-title strong"
                                                tabIndex="0">{RMResx.RM_JS_BCM_Explorer_Move_FileConflictOption_Title}
                                            </div>
                                            <div className="option-title"><label>
                                                <R.Radio
                                                    text={RMResx.RM_JS_BCM_Explorer_Move_FileConflictOption_Skip}
                                                    name='exo_Move_FileConflictOption'
                                                    value={this.state.fileNameConflictOptionSkip}
                                                    disabled={this.state.elementsEnable}
                                                    checked={this.state.exo_currentConflictOptionValue == this.state.fileNameConflictOptionSkip}
                                                    onChange={this.exoFileConflictOptionChange} />
                                            </label>
                                            </div>
                                            <div className="option-title"><label>
                                                <R.Radio
                                                    text={RMResx.RM_JS_BCM_Explorer_Move_FileConflictOption_Overwrite}
                                                    name='exo_Move_FileConflictOption'
                                                    disabled={this.state.elementsEnable}
                                                    value={this.state.fileNameConflictOptionOverwrite}
                                                    checked={this.state.exo_currentConflictOptionValue == this.state.fileNameConflictOptionOverwrite}
                                                    onChange={this.exoFileConflictOptionChange} />
                                            </label>
                                            </div>
                                            <div className="option-title"><label>
                                                <R.Radio
                                                    text={RMResx.RM_JS_BCM_Explorer_Move_FileConflictOption_Rename}
                                                    name='exo_Move_FileConflictOption'
                                                    disabled={this.state.elementsEnable}
                                                    value={this.state.fileNameConflictOptionRename}
                                                    checked={this.state.exo_currentConflictOptionValue == this.state.fileNameConflictOptionRename}
                                                    onChange={this.exoFileConflictOptionChange} />
                                            </label>
                                            </div>
                                        </div>
                                        <div id="rm_createRule_move_declare">
                                            <label className='checkbox-label'>
                                                <R.Checkbox
                                                    text={RMResx.RM_JS_BCM_Rule_Move_IsRemoveEmail}
                                                    checked={this.state.isExoMoveDeleteSource}
                                                    disabled={this.state.elementsEnable}
                                                    onChange={this.onCheckChange.bind(this, "isExoMoveDeleteSource")} />
                                            </label>
                                        </div>
                                        <div id="rm_createRule_move_declare">
                                            <label className='checkbox-label'>
                                                <R.Checkbox
                                                    text={RMResx.RM_JS_BCM_Rule_Move_IsReclassify}
                                                    checked={this.state.isKeepClassification}
                                                    disabled={this.state.elementsEnable}
                                                    onChange={this.onCheckChange.bind(this, "isKeepClassification")} />
                                            </label>
                                        </div>
                                    </div>
                                </div>
                            </div>
                        }
                    </div>
                </div>
            

                {/*What would you like to do with the content? phy*/}
                {this.state.ruleCriteriaTabsIndex == Constants.RuleSourceTabIndex.Physical &&
                    <div className="rm_createRule_archiveAction">
                        <div className="ra-createRule-question">
                            <label tabIndex="0" className="strong">{RMResx.RM_RDM_CreateRule_Title_ExchangeData}</label>
                        </div>
                        {/*Remove physical action*/}
                        <div>
                            <label>
                                <R.Radio 
                                    name="ruleActionForPHY"
                                    text={RMResx.RM_JS_RDM_CreateRule_Options_ExchangeArchiveAndRemove}
                                    checked={this.state.isPhyRemove}
                                    disabled={this.state.elementsEnable}
                                    onChange={this.phyRemoveCheckedChange} />
                            </label>
                            <$g.Popover>{RMResx.RM_JS_Rule_PhysicalRemoveActionDesc}</$g.Popover>
                            {/*Remove content and destroy from physical 子选项*/}
                            {
                                this.state.isPhyRemove &&
                                <div className="cr-archive-action-children-selection">
                                    {/*Include related records*/}
                                    {
                                        (this.state.isShowDeleteRelatedRecordOptionOfPhy && RM.gData.enviromentName != Enviroments.ChinaNorth) &&
                                        <div id="rm_createRule_deleteRelatedRecordOption">
                                            <label>
                                                <R.Checkbox
                                                    text={RMResx.RM_RDM_CreateRule_DeleteRelatedRecord}
                                                    disabled={this.state.elementsEnable}
                                                    checked={this.state.isDeleteRelatedRecordOptionOfPhy}
                                                    onChange={this.onCheckChange.bind(this, "isDeleteRelatedRecordOptionOfPhy")}
                                                />
                                            </label>
                                            <$g.Popover>{RMResx.RM_JS_Rule_IncludeRelatedRecordsDescription}</$g.Popover>
                                        </div>
                                    }
                                    {/* Remove the box if all folders in it are removed */}
                                    {
                                        this.state.isShowDestoryEmptyBoxOnFolderRuleOptionOfPhy &&
                                        <div id="rm_createRule_destoryBoxIfEmptyOption">
                                            <label>
                                                <R.Checkbox
                                                    text={RMResx.RM_RDM_CreateRule_DestroyEmptyBox}
                                                    disabled={this.state.elementsEnable}
                                                    checked={this.state.isDestoryEmptyBoxOnFolderRuleOptionOfPhy}
                                                    onChange={this.onCheckChange.bind(this, "isDestoryEmptyBoxOnFolderRuleOptionOfPhy")}
                                                />
                                            </label>
                                            <$g.Popover>{RMResx.RM_RDM_CreateRule_DestroyEmptyBoxDescription}</$g.Popover>
                                        </div>
                                    }
                                </div>
                            }
                        </div>
                        {/*Move physical to another location*/}
                        <div className='rm_createRule_move'>
                            <div>
                                <label>
                                    <R.Radio
                                        name="ruleActionForPHY"
                                        text={RMResx.RM_JS_RDM_CreateRule_Options_MoveLocation}
                                        checked={this.state.isPhyMove}
                                        disabled={this.state.elementsEnable}
                                        onChange={this.phyMoveCheckedChange} />
                                </label>
                                <$g.Popover>{RMResx.RM_JS_Rule_MoveLocationActionDesc}</$g.Popover>
                            </div>
                            {/*"Move documents to a new destination library" 子选项*/}
                            {this.state.isPhyMoveShow &&
                                <div id="rm_createRule_move_container"
                                    style={{ display: (this.state.isPhyMove) ? "block" : "none" }}>
                                    <div id="moveto-records-panel-phy">
                                        <div id="moveto-records-view-body" className="moveto-records-body">
                                            <div className="main-title" tabIndex="0">{RMResx.RM_JS_BCM_Explorer_Move_SelectTreeNode}</div>
                                            <div id="location-container-phy">
                                                {/* <div className="location-title">
                                                        </div> */}
                                                <div>
                                                    <div className="ra-tree-container">
                                                        <PhysicalRuleMoveTree
                                                            treeData={this.state.phyTreeData}
                                                            leafNodeType={this.state.smallNodeType}
                                                            onSelectedNodeChanged={this.onSelectedNodeChanged}
                                                        />
                                                    </div>
                                                    <$g.ValidationMsg show={this.state.noSelectPhyNode}>
                                                        {RMResx.RM_JS_RDM_CreateRule_Validation_NoSelectTreeNode}
                                                    </$g.ValidationMsg>
                                                </div>
                                            </div>
                                        </div>
                                        {!RM.gData.enableRecordsArchiver && <div className="file-body">
                                            <div className="option-title strong"
                                                tabIndex="0">{RMResx.RM_JS_BCM_Explorer_Move_FileConflictOption_Title}
                                            </div>
                                            <div className="option-title"><label>
                                                <R.Radio
                                                    text={RMResx.RM_JS_BCM_Explorer_Move_FileConflictOption_Skip}
                                                    name='Phy_Move_FileConflictOption'
                                                    value={this.state.fileNameConflictOptionSkip}
                                                    disabled={this.state.elementsEnable}
                                                    checked={this.state.phy_currentConflictOptionValue == this.state.fileNameConflictOptionSkip}
                                                    onChange={this.phyFileConflictOptionChange} />
                                            </label>
                                            </div>
                                            <div className="option-title"><label>
                                                <R.Radio
                                                    text={RMResx.RM_JS_BCM_Explorer_Move_FileConflictOption_Overwrite}
                                                    name='Phy_Move_FileConflictOption'
                                                    disabled={this.state.elementsEnable}
                                                    value={this.state.fileNameConflictOptionOverwrite}
                                                    checked={this.state.phy_currentConflictOptionValue == this.state.fileNameConflictOptionOverwrite}
                                                    onChange={this.phyFileConflictOptionChange} />
                                            </label>
                                            </div>
                                            <div className="option-title"><label>
                                                <R.Radio
                                                    text={RMResx.RM_JS_BCM_Explorer_Move_FileConflictOption_Rename}
                                                    name='Phy_Move_FileConflictOption'
                                                    disabled={this.state.elementsEnable}
                                                    value={this.state.fileNameConflictOptionRename}
                                                    checked={this.state.phy_currentConflictOptionValue == this.state.fileNameConflictOptionRename}
                                                    onChange={this.phyFileConflictOptionChange} />
                                            </label>
                                            </div>
                                        </div>}
                                        
                                        <div className="move-hold-body" style={{ display: (this.levelId == 16) ? "block" : "none" }}>
                                            <div className="option-title strong"
                                                tabIndex="0">{RMResx.RM_Rule_Move_Hold_Conflicted_OptionHeader}
                                            </div>
                                            <div className="option-title"><label>
                                                <R.Radio
                                                    text={RMResx.RM_Rule_Hold_Conflicted_OverrideByCurrent}
                                                    name='Move_HoldConflictOption_Current'
                                                    value={this.state.moveHoldConflictOptionCurrent}
                                                    disabled={this.state.elementsEnable}
                                                    checked={this.state.currentMoveHoldConflictOptionValue == this.state.moveHoldConflictOptionCurrent}
                                                    onChange={this.moveHoldConflictOptionChange} />
                                            </label>
                                            </div>
                                            <div className="option-title"><label>
                                                <R.Radio
                                                    text={RMResx.RM_Rule_Hold_Conflicted_Compare}
                                                    name='Move_HoldConflictOption_Compare'
                                                    disabled={this.state.elementsEnable}
                                                    value={this.state.moveHoldConflictOptionCompare}
                                                    checked={this.state.currentMoveHoldConflictOptionValue == this.state.moveHoldConflictOptionCompare}
                                                    onChange={this.moveHoldConflictOptionChange} />
                                            </label>
                                            </div>
                                        </div>
                                    </div>
                                </div>
                            }
                        </div>

                        {/* Calculate disposal date */}
                        {this.state.showCalculateDisposalDateOptionForPhy && (
                            <div className="rm_createRule_calculate-dis">
                                <div>
                                    <label>
                                        <R.Radio
                                            name="ruleActionForPHY"
                                            text={RMResx.RM_JS_RDM_CreateRule_Options_CalculateDisposalDate}
                                            checked={this.state.isCalculationDisposalDate}
                                            disabled={this.state.elementsEnable}
                                            onChange={this.phyCalculateDisposalDateCheckedChange} />
                                    </label>
                                    <$g.Popover>{RMResx.RM_JS_RDM_CreateRule_Options_CalculateDisposalDate_Desc}</$g.Popover>
                                </div>
                            </div>
                        )}
                    </div>}
                {this.renderFsRuleAction()}
                {this.renderAzureFileAction()}
                {this.renderBoxAction()}
                {this.renderGoogleAction()}
                {this.renderConnectorAction()}
                {/*What would you like to do with the content?  Teams*/}
                <div
                    style={{ display: (this.state.ruleCriteriaTabsIndex == Constants.RuleSourceTabIndex.Teams) ? "block" : "none" }}
                    className="rm_createRule_archiveAction"
                >
                    <div className="ra-createRule-question">
                        <label className="strong" tabIndex="0">{RMResx.RM_RDM_CreateRule_Title_SharepointData}</label>
                    </div>
                    <div className="rm_createRule_archiveAction_container">
                        {/*Destroy content*/}
                        {isRecordsModule && (
                            <div className='rm_createRule_remove'>
                                <label>
                                    <R.Radio
                                        name="ruleActionForTeams"
                                        text={RMResx.RM_JS_RDM_CreateRule_Options_ArchiveAndRemove}
                                        checked={this.state.isRemoveForTeams}
                                        disabled={this.state.elementsEnable}
                                        onChange={this.teamsDestroyCheckedChange} />
                                </label>
                                <$g.Popover>{RMResx.RM_JS_Rule_RemoveActionDesc}</$g.Popover>
                                {this.state.isRemoveForTeams && (
                                    <div className="cr-archive-action-children-selection">
                                        {/*Include related records*/}
                                        {this.state.isShowDeleteRelatedRecordOptionForTeams && RM.gData.enviromentName != Enviroments.ChinaNorth &&
                                            <div id="rm_createRule_deleteRelatedRecordOption">
                                                <label>
                                                    <R.Checkbox
                                                        text={RMResx.RM_RDM_CreateRule_DeleteRelatedRecord}
                                                        disabled={this.state.elementsEnable}
                                                        checked={this.state.isDeleteRelatedRecordOptionForTeams}
                                                        onChange={this.onCheckChange.bind(this, "isDeleteRelatedRecordOptionForTeams")}
                                                    />
                                                </label>
                                                <$g.Popover>{RMResx.RM_JS_Rule_IncludeRelatedRecordsDescription}</$g.Popover>
                                            </div>
                                        }
                                        {/*Include declared records*/}
                                        {this.state.isShowDeclaredFileOptionForTeams &&(
                                            <div id="rm_crateRule_declaredFile">
                                                <label>
                                                    <R.Checkbox
                                                        text={RMResx.RM_RDM_CreateRule_Options_IncludeDeclaredFile}
                                                        disabled={(this.state.isIncludeDeclaredDisable && !this.state.isBackupOptionForTeams) || this.state.elementsEnable}
                                                        checked={this.state.isDeclaredFileForTeams}
                                                        onChange={this.onCheckChange.bind(this, "isDeclaredFileForTeams")} />
                                                </label>
                                                <$g.Popover>{RMResx.RM_JS_Rule_IncludeDeclaredFileDescription}</$g.Popover>
                                            </div>
                                        )}
                                        {this.supportingLevelsForRecordsLabel.includes(this.levelId) && this.enableRecordsArchiver && !LicenseHelper.Is21VEnv() && this.renderIncludeLockedFileSetting("isIncludeLockedFileForTeams", undefined, "isLockRecordBeforeDestroyForTeams")}
                                        {/* Remove retention label */}
                                        {[32, 64].includes(this.levelId) && this.enableRecordsArchiver && RM.gData.enviromentName != Enviroments.ChinaNorth && this.renderSensitiveAndRententionLabelsSetting("isRetentionLabelForTeams")}
                                        {this.renderRemoveStoreAndLeaveStubTeams()}
                                    </div>
                                )}
                        </div>
                        )}

                        {/* Move content to archival storage */}
                        {isRecordsModule && this.state.isSeparateArchive && this.renderArchiveToAzureBlobStorageForTeams()}
                        {isArchiveModule && (
                            <div>
                                {this.renderBackupAndRemove(Constants.RuleSourceTabIndex.Teams)}
                                {RM.gData.enableDeleteOnly && this.levelId == this.RuleLevel.Document && this.renderRemoveContent(Constants.RuleSourceTabIndex.Teams)}
                            </div>
                        )}
                        
                        {/* Declare or tag content */}
                        {this.state.isKeepShowForTeams && (
                            <div className='rm_createRule_keep'>
                                <div>
                                    <label>
                                        <R.Radio
                                            name="ruleActionForTeams"
                                            text={RMResx.RM_JS_RDM_CreateRule_Options_ArchiveAndKeep}
                                            disabled={this.state.elementsEnable}
                                            checked={this.state.isKeepForTeams}
                                            onChange={this.teamsDeclareOrTagCheckedChange} />
                                    </label>
                                    <$g.Popover>{RMResx.RM_JS_Rule_KeepActionDesc}</$g.Popover>
                                </div>
                                {this.state.isKeepForTeams && (
                                    <div className="rm_createRule_keep_container">
                                        {/* Declare each document/item */}
                                        <div id="keep_declareAsRecord" className={this.state.isShowDeclareOptionForTeams ? "block" : "none"}>
                                            <label>
                                                <R.Checkbox
                                                    text={RMResx.RM_RDM_CreateRule_Options_DeclareDocumnet}
                                                    disabled={this.state.elementsEnable}
                                                    checked={this.state.isDeclareForTeams}
                                                    onChange={this.teamsDeclareChecked} />
                                            </label>
                                        </div>
                                        {/* Undeclare in-place record */}
                                        <div id="keep_undeclare"
                                            className={(this.state.isShowDeclareOptionForTeams && this.state.isCSDTenant && isRecordsModule) ? "rm_createRule_undeclare" : "none"}>
                                            <label className='checkbox-label'>
                                                <R.Checkbox
                                                    text={RMResx.RM_RDM_CreateRule_Options_UndeclareDocumnet}
                                                    disabled={this.state.elementsEnable}
                                                    checked={this.state.isUndeclareForTeams}
                                                    onChange={this.teamsUndeclareChecked} />
                                            </label>
                                        </div>
                                        {/* Tag each document/item with */}
                                        <div
                                            id="keep_tag"
                                            onClick={this.teamsKeepTagChecked}
                                            onKeyDown={(e) => {
                                                if (e.keyCode == 13) {
                                                    e.target.click();
                                                }
                                            }}
                                        >
                                            <label className='checkbox-label'>
                                                <R.Checkbox
                                                    text={RMResx.RM_RDM_CreateRule_Options_TagDocumnet}
                                                    disabled={this.state.elementsEnable}
                                                    checked={this.state.iskeepTagForTeams}
                                                    onChange={this.onCheckChange.bind(this, "iskeepTagForTeams")} />
                                            </label>
                                        </div>
                                        {this.state.iskeepTagForTeams &&
                                            <div id="keep_tag_container">
                                                <div id="keep_tag_YesorNo">
                                                    <label className='checkbox-label'>
                                                        <R.Checkbox
                                                            text={RMResx.RM_RDM_CreateRule_Options_Archived}
                                                            disabled={this.state.elementsEnable}
                                                            checked={this.state.isTagYesForTeams}
                                                            onChange={this.onCheckChange.bind(this, "isTagYesForTeams")} />
                                                    </label>
                                                </div>
                                                <div id="keep_tag_by">
                                                    <label className='checkbox-label'>
                                                        <R.Checkbox
                                                            text={RMResx.RM_RDM_CreateRule_Options_ArchivedBy}
                                                            disabled={this.state.elementsEnable}
                                                            checked={this.state.isTagByForTeams}
                                                            onChange={this.onCheckChange.bind(this, "isTagByForTeams")} />
                                                    </label>
                                                </div>
                                                <div id="keep_tag_time">
                                                    <label className='checkbox-label'>
                                                        <R.Checkbox
                                                            text={RMResx.RM_RDM_CreateRule_Options_ArchivedTime}
                                                            disabled={this.state.elementsEnable}
                                                            checked={this.state.isTagTimeForTeams}
                                                            onChange={this.onCheckChange.bind(this, "isTagTimeForTeams")} />
                                                    </label>
                                                </div>
                                                <div id="keep_tag_metadata_container">
                                                    <div id="keep_tag_metadata">
                                                        <label className='checkbox-label'>
                                                            <R.Checkbox
                                                                text={RMResx.RM_RDM_CreateRule_Options_Metadata}
                                                                disabled={this.state.elementsEnable}
                                                                checked={this.state.tagMetadataCheckedForTeams}
                                                                onChange={this.onCheckChange.bind(this, "tagMetadataCheckedForTeams")}
                                                            />
                                                        </label>
                                                    </div>
                                                    <div id="metadata_content">
                                                        <R.Combobox
                                                            id="raCrSpoTagType"
                                                            width={"100%"}
                                                            searchable={false}
                                                            textField='Name'
                                                            valueField='id'
                                                            checkedField='Checked'
                                                            items={setCheckedStatus(
                                                                "id", "Checked",
                                                                this.state.tagTypeForTeams.slice(0, 4),
                                                                this.state.selectTagTypeForTeams)}
                                                            onChange={this.teamsTagTypeSelectChanged}
                                                            searchPlaceholder=''
                                                            disabled={!this.state.tagMetadataCheckedForTeams} />
                                                        <R.Input
                                                            id="raCrSpoMetadataNameIpt"
                                                            className="tag-metadata-column"
                                                            placeholder={RMResx.RM_RDM_CreateRule_PlaceHolder_ColumnName}
                                                            disabled={!this.state.tagMetadataCheckedForTeams || this.state.elementsEnable}
                                                            value={this.state.metadataNameForTeams || ""}
                                                            onChange={this.teamsMetadataNameChange}
                                                            onBlur={this.teamsArchiveActionCustomValidate} />
                                                        {this.state.isTagTextForTeams && (
                                                            <R.Input
                                                                id="raCrSpoMetadataValueIpt"
                                                                disabled={!this.state.tagMetadataCheckedForTeams || this.state.elementsEnable}
                                                                className='tag-metadata-column'
                                                                placeholder={RMResx.RM_RDM_CreateRule_PlaceHolder_ColumnValue}
                                                                value={this.state.metadataValueForTeams || ""}
                                                                onChange={this.teamsMetadataValueChange}
                                                                onBlur={this.teamsArchiveActionCustomValidate} />
                                                        )}
                                                        {this.state.isTagBooleanForTeams && (
                                                            <div id="keep_tag_metadata_trueOrfalse">
                                                                <R.Combobox
                                                                    id="raCrTeamsTagTypeBool"
                                                                    width={"100%"}
                                                                    searchable={false}
                                                                    textField='Name'
                                                                    valueField='Name'
                                                                    checkedField='Checked'
                                                                    items={setCheckedStatus(
                                                                        "id", "Checked",
                                                                        this.state.TrueOrFaseOptions,
                                                                        this.state.selectTagBooleanForTeams)}
                                                                    onChange={this.teamsOnCurrentStoragePolicyChange}
                                                                    searchPlaceholder=''
                                                                    disabled={!this.state.tagMetadataCheckedForTeams || this.state.elementsEnable} />
                                                            </div>
                                                        )}
                                                        {this.state.isTagDateForTeams && (
                                                            <div id='keep_tag_metadata_date'>
                                                                <R.Datepicker
                                                                    id="raCrTeamsKeepTagMetadataDate"
                                                                    dateTimeFormat={this.dateTimeFormat}
                                                                    selectedDate={this.state.currentDateForTeams}
                                                                    disabled={!this.state.tagMetadataCheckedForTeams}
                                                                    hasTimePicker={true}
                                                                    hasTimeZone={true}
                                                                    onChange={this.teamsMetadataDateSelecteChange} />
                                                            </div>
                                                        )}
                                                    </div>
                                                    <div className="cr-archive-action-children-selection">
                                                        <$g.ValidationMsg show={this.state.tagMetadataCheckedForTeams && this.state.noMetadateValueForTeams}>
                                                            {RMResx.RM_JS_RDM_CreateRule_Validation_noMetadataValue}
                                                        </$g.ValidationMsg>
                                                        <$g.ValidationMsg show={this.state.noDateValueForTeams}>
                                                            {RMResx.RM_JS_RDM_CreateRule_Validation_ConditionBlankDateTime}
                                                        </$g.ValidationMsg>
                                                        <$g.ValidationMsg show={this.state.noNumberValueForTeams}>
                                                            {RMResx.RM_JS_RDM_NotNumber}
                                                        </$g.ValidationMsg>
                                                    </div>
                                                </div>
                                                {RM.gData.enviromentName != Enviroments.ChinaNorth && <div id="keep_tag_retention">
                                                    {isRecordsModule && <div className="flex ra-flex-align-center" style={{ textWrap: "nowrap" }}>
                                                        <R.Checkbox
                                                            text={RMResx.RM_RDM_CreateRule_Options_Label}
                                                            disabled={this.state.elementsEnable}
                                                            checked={this.state.retentionActionCheckedForTeams}
                                                            onChange={this.onRetentionActionCheckChangeForTeams} />
                                                        <R.Input
                                                            id="raCrTeamsMetadataRetentionIpt"
                                                            className="tag-metadata-retention"
                                                            width={"100%"}
                                                            placeholder={RMResx.RM_RDM_CreateRule_PlaceHolder_LabelName}
                                                            disabled={!this.state.retentionActionCheckedForTeams || this.state.elementsEnable}
                                                            value={this.state.retentionActionForTeams || ""}
                                                            onChange={this.retentionActionChangeForTeams}
                                                            onBlur={this.teamsArchiveActionCustomValidate} />
                                                        <$g.Popover>{RMResx.RM_JS_Rule_SP_KeepAction_RetentionDesc}</$g.Popover>
                                                    </div>}
                                                    <div className='teams_tag-metadata-retention_valid'>
                                                        <$g.ValidationMsg show={this.state.noRetentionActionValueForTeams}>
                                                            {RMResx.RM_JS_RDM_CreateRule_Validation_noRetentionValue}
                                                        </$g.ValidationMsg>
                                                    </div>
                                                    <$g.ValidationMsg show={this.state.noTagsForTeams}>
                                                        {RMResx.RM_JS_RDM_CreateRule_Validation_ConditioNoTags}
                                                    </$g.ValidationMsg>
                                                </div>}
                                            </div>
                                        }

                                        <div className='rm_createRule_keep_noSelect'>
                                            {this.state.isShowDeclareOptionForTeams && (
                                                <$g.ValidationMsg show={this.state.noSelectForTeams}>
                                                    {RMResx.RM_JS_RDM_CreateRule_Validation_ConditioNoKeepSelect}
                                                </$g.ValidationMsg>
                                            )}
                                            {!this.state.isShowDeclareOptionForTeams && (
                                                <$g.ValidationMsg show={this.state.noSelectForTeams}>
                                                    {RMResx.RM_JS_RDM_CreateRule_Validation_ConditioNoTag}
                                                </$g.ValidationMsg>
                                            )}
                                        </div>
                                    </div>
                                )}
                            </div>
                        )}

                        {/* Export only */}
                        {this.state.isShowExportOnlyForTeams && (
                            <div className='rm_createRule_exportOnly'>
                                <div>
                                    <label>
                                        <R.Radio
                                            name="ruleActionForTeams"
                                            text={RMResx.RM_JS_RDM_CreateRule_Options_ExportOnly}
                                            disabled={this.state.elementsEnable}
                                            checked={this.state.isExportOnlyForTeams}
                                            onChange={this.teamsExportOnlyCheckedChange} />
                                    </label>
                                    <$g.Popover>{RMResx.RM_JS_Rule_ExportOnlyDesc}</$g.Popover>
                                </div>
                                <div>
                                    {this.state.isExportOnlyForTeams && (
                                        <Export
                                            id='teamsExportOnly'
                                            type={10}
                                            getIsVerificationPassed={this.getTeamsExportIsPassed}
                                            getIsVerificationLocationPassed={this.getTeamsExportLocationIsPassed}
                                            getExportDate={this.getTeamsExportData}
                                            jumpExportSettings={this.jumpExportSettings}
                                            isExportOnly={true}
                                            ruleLevel={this.levelId}
                                            destinationActiveTab={this.state.destinationActiveTab}
                                            mode={this.isTeamsSourceChecked && this.state.selectedRuleModuleType === RuleModuleTypes.SOArchiver ? TabIndex.Archive : TabIndex.Records}
                                        />
                                    )}
                                </div>
                            </div>
                        )}

                        {/* Move content to new location: Hide in June release */}
                        {/* {this.state.isMoveShowForTeams && (
                            <div className='rm_createRule_move'>
                                <div>
                                    <label>
                                        <R.Radio
                                            name="ruleActionForTeams"
                                            text={RMResx.RM_JS_RDM_CreateRule_Options_MoveRecord}
                                            checked={this.state.isMoveForTeams}
                                            disabled={this.state.elementsEnable}
                                            onChange={this.moveCheckedChangeForTeams} />
                                    </label>
                                    <$g.Popover>{RMResx.RM_JS_Rule_MoveActionDesc}</$g.Popover>
                                </div>

                                Destination
                                <div id="rm_createRule_move_container" style={{ display: this.state.isMoveForTeams ? "block" : "none" }}>
                                    <div id="moveto-records-panel-sp">
                                        <div id="moveto-records-view-body" className="moveto-records-body">
                                            <div className="main-title" tabIndex="0">{RMResx.RM_JS_BCM_Explorer_Move_OptionTitle_SpecifyLocation}</div>
                                            <div id="location-container-sp">
                                                <div className="location-title">
                                                    <label>
                                                        <R.Radio
                                                            name="ruleActionMoveForTeams"
                                                            text={RMResx.RM_JS_BCM_Explorer_Move_SpecifyLocation}
                                                            checked={this.state.isSpecifyLocationForTeams}
                                                            disabled={this.state.elementsEnable}
                                                            onChange={this.teamsLocationTypeClick.bind(this, true)}
                                                        />
                                                    </label>
                                                </div>
                                                {this.state.isSpecifyLocationForTeams && (
                                                    <div className="sub-options-container">
                                                        <div className="flex">
                                                            <R.Input
                                                                id="raCrSpoLocationPathIpt"
                                                                className="location-path"
                                                                type="text"
                                                                aria-label={RMResx.RM_JS_BCM_Explorer_Move_SpecifyLocation_SP_WaterMark}
                                                                disabled={this.state.elementsEnable}
                                                                placeholder={RMResx.RM_JS_BCM_Explorer_Move_SpecifyLocation_SP_WaterMark}
                                                                value={this.state.locationPathForTeams || ""}
                                                                onChange={this.teamsLocationPathChange}
                                                                onBlur={this.teamsArchiveActionCustomValidate} />
                                                            <R.Button
                                                                className="margin-left-s"
                                                                text={RMResx.RM_RDM_CreateRule_Test}
                                                                disabled={this.state.elementsEnable}
                                                                onClick={this.checkLocationForTeams} />
                                                        </div>
                                                        <$g.ValidationMsg show={this.state.noLocationForTeams}>
                                                            {RMResx.RM_JS_RDM_CreateRule_Validation_NoInputLocaltion}
                                                        </$g.ValidationMsg>
                                                        <div id='location-vlidat-msg'>
                                                            <R.Messagebar
                                                                message={this.state.locationValidateMsgForTeams}
                                                                status={{ show: this.state.isLocationValidateForTeams }}
                                                                classify={this.state.locationValidateTypeForTeams}
                                                                onClose={this.cancelLocationValidateForTeams} />
                                                        </div>
                                                    </div>
                                                )}
                                                <div className="location-title">
                                                    <label>
                                                        <R.Radio
                                                            name="ruleActionMoveForTeams"
                                                            text={RMResx.RM_JS_BCM_Explorer_Move_SelectTreeNode}
                                                            checked={!this.state.isSpecifyLocationForTeams}
                                                            disabled={this.state.elementsEnable}
                                                            onChange={this.teamsLocationTypeClick.bind(this, false)}
                                                        />
                                                    </label>
                                                </div>
                                                <div className='ra-tree' style={{ display: this.state.isSpecifyLocationForTeams ? "none" : "block" }}>
                                                    <div className="ra-tree-container">
                                                        <SPDestinationTree
                                                            ref={r => this.ruleMoveTeamsTree = r}
                                                            treeData={this.state.destinationTreeDataForTeams}   
                                                            mode={this.isTeamsSourceChecked && this.state.selectedRuleModuleType === RuleModuleTypes.SOArchiver ? TabIndex.Archive : TabIndex.Records}
                                                            onSelectedNodeChanged={this.onDestTreeSelectedChangedForTeams} />
                                                    </div>
                                                    <$g.ValidationMsg show={this.state.noSelectNodeForTeams}>
                                                        {RMResx.RM_JS_RDM_CreateRule_Validation_NoSelectTreeNode}
                                                    </$g.ValidationMsg>
                                                </div>
                                            </div>
                                            <div className="file-body">
                                                <div className="option-title strong" tabIndex="0">{RMResx.RM_JS_BCM_Explorer_Move_FileConflictOption_Title}
                                                </div>
                                                <div className="option-title"><label>
                                                    <R.Radio
                                                        text={RMResx.RM_JS_BCM_Explorer_Move_FileConflictOption_Skip}
                                                        name='Teams_Move_FileConflictOption'
                                                        value={this.state.fileNameConflictOptionSkipForTeams.toString()}
                                                        disabled={this.state.elementsEnable}
                                                        checked={this.state.currentConflictOptionValueForTeams == this.state.fileNameConflictOptionSkipForTeams}
                                                        onChange={this.fileConflictOptionChangeForTeams} />
                                                </label>
                                                </div>
                                                <div className="option-title"><label>
                                                    <R.Radio
                                                        text={RMResx.RM_JS_BCM_Explorer_Move_FileConflictOption_Overwrite}
                                                        name='Teams_Move_FileConflictOption'
                                                        disabled={this.state.elementsEnable}
                                                        value={this.state.fileNameConflictOptionOverwriteForTeams.toString()}
                                                        checked={this.state.currentConflictOptionValueForTeams == this.state.fileNameConflictOptionOverwriteForTeams}
                                                        onChange={this.fileConflictOptionChangeForTeams} />
                                                </label>
                                                </div>
                                                <div className="option-title"><label>
                                                    <R.Radio
                                                        text={RMResx.RM_JS_BCM_Explorer_Move_FileConflictOption_Rename}
                                                        name='Teams_Move_FileConflictOption'
                                                        disabled={this.state.elementsEnable}
                                                        value={this.state.fileNameConflictOptionRenameForTeams.toString()}
                                                        checked={this.state.currentConflictOptionValueForTeams == this.state.fileNameConflictOptionRenameForTeams}
                                                        onChange={this.fileConflictOptionChangeForTeams} />
                                                </label>
                                                </div>
                                            </div>
                                            <div id="rm_createRule_move_declare">
                                                <label  className='checkbox-label'>
                                                    <R.Checkbox
                                                        text={RMResx.RM_JS_RDM_CreateRule_Options_Move_DeclareRecord}
                                                        checked={this.state.isMoveDeclareForTeams}
                                                        disabled={this.state.elementsEnable}
                                                        onChange={this.onCheckChange.bind(this, "isMoveDeclareForTeams")} />
                                                </label>
                                            </div>
                                            {isRecordsModule && (
                                                <div id="rm_createRule_move_declare">
                                                    <label className='checkbox-label'>
                                                        <R.Checkbox
                                                            text={RMResx.RM_JS_BCM_Rule_Move_IsReclassify}
                                                            checked={this.state.isKeepClassificationSPO}
                                                            disabled={this.state.elementsEnable}
                                                            onChange={this.onCheckChange.bind(this, "isKeepClassificationSPO")} />
                                                    </label>
                                                    {this.enableRecordsArchiver && RM.gData.enviromentName != Enviroments.ChinaNorth && this.renderSensitiveAndRententionLabelsSetting("isRetentionLabelForTeams")}
                                                </div>
                                            )}
                                            {isArchiveModule && this.enableRecordsArchiver && (
                                                <div>
                                                    <div id="rm_createRule_move_declare">
                                                        <label className='checkbox-label'>
                                                            <R.Checkbox
                                                                text={RMResx.RM_JS_RDM_CreateRule_Options_Move_AllVersions}
                                                                checked={this.state.isMoveVersionsForTeams}
                                                                disabled={this.state.elementsEnable}
                                                                onChange={this.onCheckChange.bind(this, "isMoveVersionsForTeams")} />
                                                        </label>
                                                    </div>
                                                </div>
                                            )}
                                        </div>
                                    </div>
                                </div>
                            </div>
                        )} */}
                    </div>
                </div>
                {/*Enable manual approval?*/}
                <div style={{ display: (this.state.ruleCriteriaTabsIndex == Constants.RuleSourceTabIndex.SP) ? "block" : "none" }}>
                    {
                        isRecordsModule && <EnableManualApproval
                            id='spApproval'
                            radioName="SPManualApprove"
                            workflowItems={this.props.workflowItems}
                            getIsVerificationPassed={this.getSpApprovalIsPassed}
                            getApprovalData={this.getSpApprovalData}
                        />
                    }
                </div>
                <div style={{ display: this.state.ruleCriteriaTabsIndex == Constants.RuleSourceTabIndex.Teams ? "block" : "none" }}>
                    {isRecordsModule && (
                        <EnableManualApproval
                            id='teamsApproval'
                            radioName="TeamsManualApprove"
                            workflowItems={this.props.workflowItems}
                            getIsVerificationPassed={this.getTeamsApprovalIsPassed}
                            getApprovalData={this.getTeamsApprovalData}
                        />
                    )}
                </div>
                <div style={{ display: (this.state.ruleCriteriaTabsIndex == Constants.RuleSourceTabIndex.Exchange) ? "block" : "none" }}>
                    <EnableManualApproval
                        id='exoApproval'
                        radioName="ExoManualApprove"
                        workflowItems={this.props.workflowItems}
                        getIsVerificationPassed={this.getExoApprovalIsPassed}
                        getApprovalData={this.getExoApprovalData}
                    />
                </div>
                <div style={{ display: (this.state.ruleCriteriaTabsIndex == Constants.RuleSourceTabIndex.Physical) ? "block" : "none" }}>
                    <EnableManualApproval
                        id='phyApproval'
                        radioName="PhyManualApprove"
                        workflowItems={this.props.workflowItems}
                        getIsVerificationPassed={this.getPhyApprovalIsPassed}
                        getApprovalData={this.getPhyApprovalData}
                    />
                    {this.renderStorageSettingsForPhy()}
                </div>
                <div style={{ display: (this.state.ruleCriteriaTabsIndex == Constants.RuleSourceTabIndex.FS) ? "block" : "none" }}>
                    <EnableManualApproval
                        id='fsApproval'
                        radioName="FSManualApprove"
                        workflowItems={this.props.workflowItems}
                        getIsVerificationPassed={this.getFsApprovalIsPassed}
                        getApprovalData={this.getFsApprovalData}
                    />
                    {this.enableRecordsArchiver && this.renderStorageSettingsForFS()}
                </div>
                <div style={{ display: (this.state.ruleCriteriaTabsIndex == Constants.RuleSourceTabIndex.SPLocal) ? "block" : "none" }}>
                    <EnableManualApproval
                        id='spLocalApproval'
                        radioName="SPLManualApprove"
                        workflowItems={this.props.workflowItems}
                        getIsVerificationPassed={this.getSpLocalApprovalIsPassed}
                        getApprovalData={this.getSpLocalApprovalData}
                    />
                </div>
                <div style={{ display: (this.state.ruleCriteriaTabsIndex == Constants.RuleSourceTabIndex.OneDrive) ? "block" : "none" }}>
                    {
                        isRecordsModule && <EnableManualApproval
                            id='oneDriveApproval'
                            radioName="oneDriveManualApprove"
                            workflowItems={this.props.workflowItems}
                            getIsVerificationPassed={this.getOneDriveApprovalIsPassed}
                            getApprovalData={this.getOneDriveApprovalData}
                        />
                    }
                </div>
                <div style={{ display: (this.state.ruleCriteriaTabsIndex == Constants.RuleSourceTabIndex.AzureFile) ? "block" : "none" }}>
                    <EnableManualApproval
                        id='azureFileApproval'
                        radioName="azureFileManualApprove"
                        workflowItems={this.props.workflowItems}
                        getIsVerificationPassed={this.getAzureFileApprovalIsPassed}
                        getApprovalData={this.getAzureFileApprovalData}
                    />
                </div>
                <div style={{ display: (this.state.ruleCriteriaTabsIndex == Constants.RuleSourceTabIndex.Box) ? "block" : "none" }}>
                    <EnableManualApproval
                        id='boxApproval'
                        radioName="boxManualApprove"
                        workflowItems={this.props.workflowItems}
                        getIsVerificationPassed={this.getBoxApprovalIsPassed}
                        getApprovalData={this.getBoxApprovalData}
                    />
                </div>
                <div style={{ display: (this.state.ruleCriteriaTabsIndex == Constants.RuleSourceTabIndex.GoogleDrive) ? "block" : "none" }}>
                    <EnableManualApproval
                        id='googleDriveApproval'
                        radioName="googleDriveManualApprove"
                        workflowItems={this.props.workflowItems}
                        getIsVerificationPassed={this.getGoogleDriveApprovalIsPassed}
                        getApprovalData={this.getGoogleDriveApprovalData}
                    />
                </div>
                <div style={{ display: (this.state.ruleCriteriaTabsIndex == Constants.RuleSourceTabIndex.Connector) ? "block" : "none" }}>
                    <EnableManualApproval
                        id='connectorApproval'
                        radioName="connectorManualApprove"
                        workflowItems={this.props.workflowItems}
                        getIsVerificationPassed={this.getConnectorApprovalIsPassed}
                        getApprovalData={this.getConnectorApprovalData}
                    />
                </div>
                {/*Export the SharePoint content before archiving?*/}
                <div style={{ display: (this.state.ruleCriteriaTabsIndex == Constants.RuleSourceTabIndex.SP) ? "block" : "none" }}>
                    <Export
                        id='spExport'
                        type={1}
                        getIsVerificationPassed={this.getSpExportIsPassed}
                        getIsVerificationLocationPassed={this.getSpExportLocationIsPassed}
                        getExportDate={this.getSpExportDate}
                        jumpExportSettings={this.jumpExportSettings}
                        ruleLevel={this.levelId}
                        destinationActiveTab={this.state.destinationActiveTab}
                        mode={this.isSpSourceChecked && this.state.selectedRuleModuleType === RuleModuleTypes.SOArchiver ? TabIndex.Archive : TabIndex.Records}
                    />
                    {this.renderStorageSettingsForSPO()}
                </div>
                <div style={{ display: this.state.ruleCriteriaTabsIndex == Constants.RuleSourceTabIndex.Teams ? "block" : "none" }}>
                    {this.levelId != this.RuleLevel.Teams && (
                        <Export
                            id='teamsExport'
                            type={10}
                            getIsVerificationPassed={this.getTeamsExportIsPassed}
                            getIsVerificationLocationPassed={this.getTeamsExportLocationIsPassed}
                            getExportDate={this.getTeamsExportData}
                            jumpExportSettings={this.jumpExportSettings}
                            ruleLevel={this.levelId}
                            destinationActiveTab={this.state.destinationActiveTab}
                            mode={this.isTeamsSourceChecked && this.state.selectedRuleModuleType === RuleModuleTypes.SOArchiver ? TabIndex.Archive : TabIndex.Records}
                        />
                    )}
                    {this.renderStorageSettingsForTeams()}
                </div>
                <div style={{ display: (this.state.ruleCriteriaTabsIndex == Constants.RuleSourceTabIndex.Exchange) ? "block" : "none" }}>
                    <Export
                        id='exoExport'
                        type={3}
                        getIsVerificationPassed={this.getExoExportIsPassed}
                        getIsVerificationLocationPassed={this.getExoExportLocationIsPassed}
                        getExportDate={this.getExoExportDate}
                        jumpExportSettings={this.jumpExportSettings}
                        ruleLevel={this.levelId}
                        destinationActiveTab={this.state.destinationActiveTab}
                    />
                </div>
                <div style={{ display: "none" }}>
                    <Export
                        id='spLocalExport'
                        type={5}
                        getIsVerificationPassed={this.getSpLocalExportIsPassed}
                        getExportDate={this.getSpLocalExportDate} 
                        jumpExportSettings={this.jumpExportSettings}
                        destinationActiveTab={this.state.destinationActiveTab}
                    />
                </div>
                <div style={{ display: (this.state.ruleCriteriaTabsIndex == Constants.RuleSourceTabIndex.OneDrive) ? "block" : "none" }}>
                    {this.state.isShowExportCheckboxForOneDrive && <Export
                        id='oneDriveExport'
                        type={1}
                        getIsVerificationPassed={this.getOneDriveExportIsPassed}
                        getIsVerificationLocationPassed={this.getOneDriveExportLocationIsPassed}
                        getExportDate={this.getOneDriveExportDate}
                        jumpExportSettings={this.jumpExportSettings}
                        ruleLevel={this.levelId}
                        destinationActiveTab={this.state.destinationActiveTab}
                        mode={this.isOneDriveSourceChecked && this.state.selectedRuleModuleType === RuleModuleTypes.SOArchiver ? TabIndex.Archive : TabIndex.Records}
                    />}
                    {this.renderStorageSettingsForOndrive()}
                </div>     
                <div style={{ display: (this.state.ruleCriteriaTabsIndex == Constants.RuleSourceTabIndex.GoogleDrive) ? "block" : "none" }}>
                    <GoogleExport
                        id='googleExport'
                        type={9}
                        getIsVerificationPassed={this.getGoogleExportIsPassed}
                        getIsVerificationLocationPassed={this.getGoogleExportLocationIsPassed}
                        getExportDate={this.getGoogleExportDate}
                        jumpExportSettings={this.jumpExportSettings}
                        ruleLevel={this.levelId}
                    />
                    {this.renderStorageSettingsForGoogle()}
                </div>                
            </div>
            {LicenseHelper.EnableRecordsArchiver() && this.renderStubSettingsPanel()}
        </div>;
    }
}
export default withRouter(CreateRule);
