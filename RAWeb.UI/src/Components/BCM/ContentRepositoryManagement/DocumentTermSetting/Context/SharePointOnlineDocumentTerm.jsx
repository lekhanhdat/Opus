import { EnvironmentHelper, showToast } from "../../../../../Utilities/CommonUtil";
import CRMCommonUtil from "../../Common/CRMCommonUtil";
import { SelectProcessType } from "../../ManualApprovalSetting/ManualApprovalSettingPanel";
import { ArtificialIntelligenceTermUseType, SavedTermSettingType } from "../DocumentTermSettingPanel";

export default {
    getContext() {
        return {
            configurations: {
                enableRelatedRecords: true,
                termDisplayFormat: true,
                includeDeclared: true,
                defaultTermApplyExist: true,
                autoRuleDeploy: true,
                columnName: true,
                includeDeclaredTooltip: true,
                includeDeclaredDesp: RMResx.RM_JS_SPS_IncludeDeclaredRecords_Desp,
                useDefaultTerm: true,
                applyTermIsShowTips: false,
                applyDocumentSetsAndFolders: true,
                applyDocumentSetsTitle: true,
                enableAITerm: RM.gData.hasIntelligentPermission, // && !EnvironmentHelper.IsGCPEnvironment
                applyAlwaysScanDocuments: RM.gData.enableApplySettingScanAll,
            },
            itemId: "sp",
            saveTermSettings: this.saveTermSettings,
            getSavedTerm: this.getSavedTerm,
            termSettingsTitle: RMResx.RM_JS_SPS_EditTitle_DocumentLevelSetting,
            isGroupNode: this.isGroupNode,
            getGroupNodeId: this.getGroupNodeId,
            downloadRelatedAppUrl: "/api/SPSettingApi/DownloadRelatedApp",
        };
    },
    saveTermSettings(that, callback) {
        let currNode = that.props.data;
        let stateData = that.state;
        let allValidationsValidate = $$.verify(that.allValidation);
        let autoRuleValidateResult = that.setAutoRuleData();
        if (!allValidationsValidate || !autoRuleValidateResult) {
            return false;
        }
        currNode.SiteGroupId = CRMCommonUtil.getGroupNode(currNode).SPObjectId;
        currNode.DeployTermMethod = stateData.deployTermMethod;
        currNode.RunAutoFullJob = stateData.runAutoFullJob;
        currNode.AlwaysScanAllExistDocuments = stateData.alwaysScanAllExistDocuments;
        currNode.AutoJobOption = stateData.autoJobOption;

        currNode.IsDisplyaTermPath = that.isDisplayTermPath;
        currNode.NeedCheckDefaultValue = stateData.applyToAll;
        currNode.IncludeDeclaredRecords = stateData.includeDeclaredRecords;
        currNode.ApplyExistType = stateData.applyExistType;
        currNode.ApplyTermIncludeFolder = stateData.applyToDSetsAndFolders;

        currNode.TermSetId = that.termSetId;
        currNode.TermSetName = that.termSetName;
        currNode.TermName = that.termName;
        currNode.TermId = that.termId;
        currNode.DefaultTermName = stateData.termDefaultName;
        currNode.DefaultTermId = that.defaultTermId;
        currNode.EnableRelatedRecords = stateData.enableRelatedRecords;

        //ai term
        currNode.AITermUseType = stateData.aiTermUseType;
        currNode.AISendEMail = stateData.mailToOwner;

        currNode.AIThenDefaultTermId = that.aiThenDefaultTermId;
        currNode.AIThenDefaultTermName = stateData.aiThenDefaultTermName;
        currNode.AIThenIsDefaultTermMethod = stateData.aiThenIsDefaultTermMethod;
        if (stateData.aiTermUseType != ArtificialIntelligenceTermUseType.None) {
            if (stateData.intelligenceEnableApprovalItem == SelectProcessType.SelectOwnerRecords) {
                let newRoList = [];
                that.addUserChanged.forEach(user => {
                    newRoList.push(user.data);
                });
                if (newRoList.length == 0) {
                    return false;
                } else {
                    currNode.AIReviewers = newRoList;
                    currNode.AISendEMail = stateData.mailToOwner;
                    currNode.AIApprovalType = stateData.intelligenceEnableApprovalItem;
                    currNode.AIWorkflowReferenceId = null;
                }
            } else {
                currNode.AIReviewers = [];
                currNode.AISendEMail = false;
                currNode.AIApprovalType = SelectProcessType.SelectNoneApprovalType;
                currNode.AIWorkflowReferenceId = null;
            }
        }

        let sendUrl = CRMCommonUtil.isGroup(currNode) ? "/api/SPSettingApi/SaveGroupLevelSetting" : "/api/SPSettingApi/SaveDocumentLevelSetting";
        let option = {
            url: sendUrl,
            method: "Post",
            data: currNode
        };
        fetchUtility(option).then((result) => {
            $$.loading(false);
            var resultData = JSON.parse(result);
            if (resultData.MessageType == 0) {
                callback(true);
                showToast.success(RMResx.RM_JS_BCM_SaveSettingsSuccess);
            } else if (resultData.MessageType == 1) {
                if (resultData.FaildType == 10 || resultData.FaildType == 31) {
                    showToast.error(resultData.ErrorMessage);
                    callback(true);
                }
            }
        }).catch((e) => {
            $$.loading(false);
        });
    },
    getSavedTerm(that) {
        let currNode = that.props.data;
        let isGroup = CRMCommonUtil.isGroup(currNode);
        let groupNode = CRMCommonUtil.getGroupNode(currNode);
        let isFolder = CRMCommonUtil.isFolder(currNode);
        let paramObj = {};
        paramObj.TermSetId = currNode.TermSetId;
        paramObj.CurrentNodeId = currNode.TermId;
        paramObj.SettingType = isGroup ? SavedTermSettingType.GroupTerm : SavedTermSettingType.CustomTerm;
        paramObj.spTreeNodes = [groupNode];
        paramObj.perPageCount = that.treePageSize;
        paramObj.AgentGroupId = groupNode.SPObjectId;
        let option = {
            url: "/api/SPSettingApi/GetSavedTree",
            method: "Post",
            data: paramObj
        };
        return fetchUtility(option).then((res) => {
            if (res !== "") {
                let result = JSON.parse(res);
                if (result.NoTermPermission) {
                    return {
                        isSuccess: false,
                        message: RMResx.RM_JS_SPS_ErrorMessage_NoTermPermission,
                        termGroups: null
                    };
                }
                if (isGroup) {
                    if (result.SelectedTermScopeNoPermission) {
                        return {
                            isSuccess: false,
                            isTermScopeError: true,
                            termNoPermissionMsg: RMResx.RM_JS_SPS_ErrorMessage_SelectedTermNoPermission,
                            termGroups: result.TermGroups,
                        };
                    } else {
                        return {
                            isSuccess: true,
                            termGroups: result
                        };
                    }
                } else {
                    if (result.IsChangeAnotherTermGroup) {
                        let returnObj = {
                            isSuccess: false,
                            isChangeAnotherTermGroup: true,
                            message: isFolder ? RMResx.RM_JS_SPS_CS_ChangeGroup_Folder : RMResx.RM_JS_SPS_CS_ChangeGroup,
                            termGroups: [result.TermGroup]
                        };
                        if (result.SelectedTermScopeNoPermission) {
                            returnObj.isTermScopeError = true;
                            returnObj.termNoPermissionMsg = RMResx.RM_JS_SPS_ErrorMessage_SelectedTermNoPermission;
                        }
                        return returnObj;
                    } else {
                        if (result.SelectedTermScopeNoPermission) {
                            return {
                                isSuccess: false,
                                isTermScopeError: true,
                                termNoPermissionMsg: RMResx.RM_JS_SPS_ErrorMessage_SelectedTermNoPermission,
                                termGroups: [result.TermGroup],
                            };
                        } else {
                            return {
                                isSuccess: true,
                                termGroups: [result.TermGroup]
                            };
                        }
                    }
                }
            }
        });
    },
    isGroupNode(documentTermSetting) {
        return CRMCommonUtil.isGroup(documentTermSetting);
    },
    getGroupNodeId(node) {
        return CRMCommonUtil.getGroupNode(node).Id;
    }
};
