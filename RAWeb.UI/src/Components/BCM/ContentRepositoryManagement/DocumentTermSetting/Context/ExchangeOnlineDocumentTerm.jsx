import { EnvironmentHelper, showToast } from "../../../../../Utilities/CommonUtil";
import CRMCommonUtil, { RAFailedType, RAMessageType } from "../../Common/CRMCommonUtil";
import { SavedTermSettingType } from "../DocumentTermSettingPanel";
import { CRComponentType } from "../../../../../Constants/Constants";

export default {
    getContext() {
        return {
            configurations: {
                autoRuleDeploy: true,
                termDisplayFormat: false,
                useDefaultTerm: false,
                applyTermIsShowTips: true,
                createRuleComponentType: CRComponentType.EXOSetting,
                isRequire: false,
                isHiddenInGCPEnv: EnvironmentHelper.IsGCPEnvironment,
            },
            itemId: "exo",
            saveTermSettings: this.saveTermSettings,
            getSavedTerm: this.getSavedTerm,
            termSettingsTitle: RMResx.RM_JS_EXO_EditTitle_TermSettings,
            isGroupNode: this.isGroupNode,
            getGroupNodeId: this.getGroupNodeId,
            isSupportNullClassificationSetting: this.getGroupNodeId,
            applyTermItemTips: [RMResx.RM_SPS_NoDefaultValueDesc, RMResx.RM_JS_EXO_SetPresetTermDesc, RMResx.RM_SPS_UseRuleDesc],
            setRuleTitle: RMResx.RM_JS_SPS_SetRuleForNonClassifiedTitle,
        };
    },
    saveTermSettings(that, callback) {
        let currNode = that.props.data;
        let stateData = that.state;
        if(stateData.enableTermSettingStatus) {
            let allValidationsValidate = $$.verify(that.allValidation);
            let autoRuleValidateResult = that.setAutoRuleData();
            if (!allValidationsValidate || !autoRuleValidateResult) {
                return false;
            }
        }
        
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
        const isGroupNode = CRMCommonUtil.isEXOGroup(currNode);
        const successMessage = isGroupNode && !stateData.enableTermSettingStatus? RMResx.RM_JS_SPS_SaveSettingsSuccess: RMResx.RM_JS_BCM_SaveSettingsSuccess;
        let sendUrl = isGroupNode ? "/api/EXOSettingApi/SaveGroupEXOTermSetting" : "/api/EXOSettingApi/SaveCustomEXOTermSetting";
        let option = {
            url: sendUrl,
            method: "Post",
            data: currNode
        };
        fetchUtility(option).then((result) => {
            $$.loading(false);
            var resultData = JSON.parse(result);
            if (resultData.MessageType == RAMessageType.Successful) {
                callback(true);
                showToast.success(successMessage);
            } else if (resultData.MessageType == RAMessageType.Failed) {
                if (resultData.FaildType == RAFailedType.DisableRecordsManagement) {
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
        let isGroup = CRMCommonUtil.isEXOGroup(currNode);
        let groupNode = CRMCommonUtil.getEXOGroupNode(currNode);
        let paramObj = {};
        paramObj.TermSetId = currNode.TermSetId;
        paramObj.CurrentNodeId = currNode.TermId;
        paramObj.SettingType = isGroup ? SavedTermSettingType.GroupTerm : SavedTermSettingType.CustomTerm;
        //paramObj.spTreeNodes = [groupNode];
        paramObj.GroupId = groupNode.Id;
        paramObj.perPageCount = that.treePageSize;
        paramObj.AgentGroupId = groupNode.SPObjectId;
        let option = {
            url: "/api/EXOSettingApi/GetEXOSavedTree",
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
                        message: RMResx.RM_JS_SPS_CS_ChangeGroup,
                        termGroups: [result.TermGroup]
                    }
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
        return CRMCommonUtil.isEXOGroup(documentTermSetting);
    },
    getGroupNodeId(node) {
        return CRMCommonUtil.getEXOGroupNode(node).Id;
    }
};
