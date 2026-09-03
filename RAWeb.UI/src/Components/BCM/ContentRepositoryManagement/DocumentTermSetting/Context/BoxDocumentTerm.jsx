import { showToast } from "../../../../../Utilities/CommonUtil";
import CRMCommonUtil from "../../Common/CRMCommonUtil";
import { SavedTermSettingType } from "../DocumentTermSettingPanel";

export default {
    getContext() {
        return {
            configurations: {
                defaultTermApplyExist: true,
                autoRuleDeploy: true,
                termDisplayFormat: false,
                useDefaultTerm: true,
                applyTermIsShowTips: false,
            },
            itemId: "box",
            saveTermSettings: this.saveTermSettings,
            getSavedTerm: this.getSavedTerm,
            termSettingsTitle: RMResx.RM_JS_EXO_EditTitle_TermSettings,
            isGroupNode: this.isGroupNode,
            getGroupNodeId: this.getGroupNodeId,
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

        currNode.DeployTermMethod = stateData.deployTermMethod;

        currNode.RunAutoFullJob = stateData.runAutoFullJob;
        currNode.AutoJobOption = stateData.autoJobOption;

        currNode.NeedCheckDefaultValue = stateData.applyToAll;
        currNode.ApplyExistType = stateData.applyExistType;

        currNode.TermSetId = that.termSetId;
        currNode.TermSetName = that.termSetName;
        currNode.TermName = that.termName;
        currNode.TermId = that.termId;
        currNode.DefaultTermName = stateData.termDefaultName;
        currNode.DefaultTermId = that.defaultTermId;

        let sendUrl = "/api/BoxSetting/SaveSettings";
        let option = {
            url: sendUrl,
            method: "Post",
            data: currNode
        };
        fetchUtility(option).then((result) => {
            $$.loading(false);
            callback(true);
            showToast.success(RMResx.RM_JS_SPS_SaveSettingsSuccess);
        }).catch((e) => {
            $$.loading(false);
        });
    },

    getSavedTerm(that) {
        let currNode = that.props.data;
        const currTreeNode = currNode.SelectedNode;
        let isGroup = CRMCommonUtil.isBoxGroup(currTreeNode);
        // let groupNode = CRMCommonUtil.getAzureFileGroupNode(currTreeNode);
        let paramObj = {};
        paramObj.TermSetId = currNode.TermSetId;
        paramObj.CurrentNodeId = currNode.TermId;
        paramObj.SettingType = isGroup ? SavedTermSettingType.GroupTerm : SavedTermSettingType.CustomTerm;
        paramObj.ConnGroupId = isGroup? currTreeNode.id : currTreeNode.containerId;
        paramObj.perPageCount = that.treePageSize;
        let option = {
            url: "/api/BoxSetting/GetBoxSavedTerm",
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
                        message: RMResx.RM_JS_SPS_CS_ChangeGroup,
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
        return CRMCommonUtil.isBoxGroup(documentTermSetting.SelectedNode);
    },

    getGroupNodeId(node) {
        return CRMCommonUtil.getBoxGroupNode(node.SelectedNode).id;
    }
};