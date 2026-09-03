import { EnvironmentHelper, showToast } from "../../../../../Utilities/CommonUtil";
import CRMCommonUtil from "../../Common/CRMCommonUtil";
import { ArtificialIntelligenceTermUseType, SavedTermSettingType } from "../DocumentTermSettingPanel";
import { CRComponentType } from "../../../../../Constants/Constants";
import { SelectProcessType } from "../../ManualApprovalSetting/ManualApprovalSettingPanel";

export default {
    getContext() {
        return {
            configurations: {
                enableRelatedRecords: false,
                termDisplayFormat: false,
                includeDeclared: true,
                defaultTermApplyExist: true,
                autoRuleDeploy: true,
                includeDeclaredTooltip: true,
                includeDeclaredDesp: RMResx.RM_OneDrive_IncludeDeclaredRecords_Desp,
                useDefaultTerm: true,
                applyTermIsShowTips: false,
                switchEnableTermSettings: true,
                createRuleComponentType: CRComponentType.OnedriveSetting,
                isSwitchTermSettingEffectChildren: true,
                isRequire: false,
                enableAITerm: RM.gData.hasIntelligentPermission, // && !EnvironmentHelper.IsGCPEnvironment
            },
            itemId: "oneDrive",
            saveTermSettings: this.saveTermSettings,
            getSavedTerm: this.getSavedTerm,
            termSettingsTitle: RMResx.RM_JS_EXO_EditTitle_TermSettings,
            isGroupNode: this.isGroupNode,
            getGroupNodeId: this.getGroupNodeId,
            isSupportNullClassificationSetting: this.isSupportNullClassificationSetting,
            setRuleTitle: RMResx.RM_JS_SPS_SetRuleForNonClassifiedTitle,
            downloadRelatedAppUrl: "/api/SPSettingApi/DownloadRelatedApp",
        };
    },
    saveTermSettings(that, callback) {
        let currNode = that.props.data;
        let stateData = that.state;
        // let allValidationsValidate = $$.verify(that.allValidation);
        // let autoRuleValidateResult = that.setAutoRuleData();
        // if (!allValidationsValidate || !autoRuleValidateResult) {
        //     return false;
        // }
        if (stateData.enableTermSettingStatus) {
            let allValidationsValidate = $$.verify(that.allValidation);
            let autoRuleValidateResult = that.setAutoRuleData();
            if (!allValidationsValidate || !autoRuleValidateResult) {
                return false;
            }
        }
        currNode.SiteGroupId = CRMCommonUtil.getGroupNode(currNode).SPObjectId;
        currNode.DeployTermMethod = stateData.deployTermMethod;
        currNode.RunAutoFullJob = stateData.runAutoFullJob;
        currNode.AutoJobOption = stateData.autoJobOption;

        currNode.IsDisplyaTermPath = that.isDisplayTermPath;
        currNode.NeedCheckDefaultValue = stateData.applyToAll;
        currNode.IncludeDeclaredRecords = stateData.includeDeclaredRecords;
        currNode.ApplyExistType = stateData.applyExistType;

        currNode.TermSetId = that.termSetId;
        currNode.TermSetName = that.termSetName;
        currNode.TermName = that.termName;
        currNode.TermId = that.termId;
        currNode.DefaultTermName = stateData.termDefaultName;
        currNode.DefaultTermId = that.defaultTermId;
        currNode.Rules = that.addedRules;
        currNode.IsNullClassificationSetting = !stateData.enableTermSettingStatus;

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

        let sendUrl = "/api/OneDriveSettingApi/SaveDocumentLevelSetting";
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
                showToast.success(RMResx.RM_JS_SPS_SaveSettingsSuccess);
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
            url: "/api/OneDriveSettingApi/GetSavedTree",
            method: "Post",
            data: paramObj
        };
        return fetchUtility(option).then((res) => {
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
                        termGroups: result.TermGroups
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
        });
    },
    isGroupNode(documentTermSetting) {
        return CRMCommonUtil.isGroup(documentTermSetting);
    },
    getGroupNodeId(node) {
        return CRMCommonUtil.getGroupNode(node).Id;
    },
    isSupportNullClassificationSetting(node){
        return CRMCommonUtil.isAllowSetByRulesOnedriveLevel(node);
    }
};
