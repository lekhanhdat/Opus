import { EnvironmentHelper, showToast } from "../../../../../Utilities/CommonUtil";
import CRMCommonUtil from "../../Common/CRMCommonUtil";
import { CRComponentType } from "../../../../../Constants/Constants";
import { ArtificialIntelligenceTermUseType, AutoJobOption } from "../GoogleDocumentLabelSetting/GoogleDocumentLabelSettingPanel";
import { SelectProcessType } from "../../ManualApprovalSetting/ManualApprovalSettingPanel";

export default {
    getContext() {
        return {
            configurations: {
                enableRelatedRecords: false,
                termDisplayFormat: false,
                includeDeclared: false,
                defaultTermApplyExist: true,
                autoRuleDeploy: true,
                applyTermIsShowTips: false,
                switchEnableTermSettings: true,
                createRuleComponentType: CRComponentType.LabelManagement,
                isSwitchTermSettingEffectChildren: true,
                isRequire: false,
                enableAITerm: RM.gData.hasIntelligentPermission, //  && !EnvironmentHelper.IsGCPEnvironment
            },
            itemId: "googleDrive",
            saveTermSettings: this.saveTermSettings,
            getSavedTerm: this.getSavedTerm,
            termSettingsTitle: RMResx.RM_JS_SPS_EditTitle_LabelSettings,
            isGroupNode: this.isGroupNode,
            getGroupNodeId: this.getGroupNodeId,
            supportSettingWithoutClassification: this.supportSettingWithoutClassification,
            setRuleTitle: RMResx.RM_JS_SPS_SetRuleForNonClassifiedTitle,
        };
    },
    saveTermSettings(that, callback) {
        let currNode = that.props.data;
        let stateData = that.state;
        if (stateData.enableTermSettingStatus) {
            let allValidationsValidate = $$.verify(that.allValidation);
            let autoRuleValidateResult = that.setAutoRuleData();
            let isTermHasNoPermission = that.autoRuleData?.some(item => item.TermHasNoPermission);
            if (!allValidationsValidate || !autoRuleValidateResult || isTermHasNoPermission) {
                return false;
            }
        }
        currNode.DeployLabelMethod = stateData.deployLabelMethod;
        currNode.RunAutoFullJob = stateData.runAutoFullJob;
        currNode.AutoJobOption = stateData.autoJobOption;

        currNode.IsDisplyaTermPath = that.isDisplayTermPath;
        currNode.NeedCheckDefaultValue = stateData.applyToAll;
        currNode.ApplyExistType = stateData.applyExistType;
       
        currNode.TermName = that.termName;
        currNode.TermId = that.termId;
        currNode.DefaultTermName = stateData.termDefaultName;
        currNode.DefaultTermId = that.defaultTermId;
        currNode.Rules = that.addedRules;
        currNode.ObjectId = that.ObjectId;
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

        let sendUrl = "/api/GoogleDriveSettingApi/SaveLabelSetting";

        let option = {
            url: sendUrl,
            method: "Post",
            data: currNode
        };
        $$.loading(true);
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
    
    isGroupNode(documentTermSetting) {
        return CRMCommonUtil.isGoogleContainer(documentTermSetting);
    },
    getGroupNodeId(node) {
        return CRMCommonUtil.getGoogleDriveContainerNode(node).Id;
    },
    supportSettingWithoutClassification(node){
        return CRMCommonUtil.isAllowSetByRulesGoogleLevel(node);
    }
};
