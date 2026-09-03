import { showToast } from "../../../../../Utilities/CommonUtil";
import CRMCommonUtil from "../../Common/CRMCommonUtil";
import { SavedTermSettingType } from "../DocumentTermSettingPanel";

export default {
    getContext() {
        return {
            configurations: {
                enableRelatedRecords: false,
                termDisplayFormat: false,
                useDefaultTerm: true,
                applyTermIsShowTips: false
            },
            itemId: "phy",
            saveTermSettings: this.saveTermSettings,
            getSavedTerm: this.getSavedTerm,
            termSettingsTitle: RMResx.RM_JS_EXO_EditTitle_TermSettings,
            isGroupNode: this.isGroupNode,
            getGroupNodeId: this.getGroupNodeId,
            downloadRelatedAppUrl: "/api/SPSettingApi/DownloadRelatedApp",
        };
    },
    saveTermSettings(that, callback) {
        let currNode = that.props.data;
        let stateData = that.state;
        let allValidationsValidate = $$.verify(that.allValidation);
        if (!allValidationsValidate) {
            return false;
        }
        currNode.DeployTermMethod = stateData.deployTermMethod;
        currNode.NeedCheckDefaultValue = stateData.applyToAll;
        currNode.TermSetId = that.termSetId;
        currNode.TermSetName = that.termSetName;
        currNode.TermName = that.termName;
        currNode.TermId = that.termId;
        currNode.DefaultTermName = stateData.termDefaultName;
        currNode.DefaultTermId = that.defaultTermId;

        let option = {
            url: "/api/PRSettingApi/SavePRTermSetting",
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
                if (resultData.FaildType == 10) {
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
        let isGroup = currNode.IsTopLevelSetting;
        let paramObj = {};
        paramObj.TermSetId = currNode.TermSetId;
        paramObj.CurrentNodeId = currNode.TermId;
        paramObj.SettingType = isGroup ? SavedTermSettingType.GroupTerm : SavedTermSettingType.CustomTerm;
        paramObj.GroupId = currNode.TopLevelSettingUniqueId;
        paramObj.perPageCount = that.treePageSize;
        let option = {
            url: "/api/PRSettingApi/GetPRSavedTree",
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
        return documentTermSetting.IsTopLevelSetting;
    },
    getGroupNodeId(node) {
        return node.TopLevelSettingUniqueId;
    }
};
