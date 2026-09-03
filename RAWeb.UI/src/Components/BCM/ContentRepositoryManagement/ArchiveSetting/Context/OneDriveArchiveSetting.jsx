import { CRComponentType } from "../../../../../Constants/Constants";
import Enviroments from "../../../../../Constants/Enviroments";
import { showToast } from "../../../../../Utilities/CommonUtil";

export default {
    getContext() {
        return {
            configurations: {
                createRuleComponentType: CRComponentType.OnedriveSetting,
                isRequire: true,
                showRuleLevel: true,
                showConfigureWarn: true,
            },
            setRuleTitle: RMResx.RM_AR_SPS_Title_Rules,
            saveArchiveSetting: this.saveArchiveSetting,
        };
    },
    saveArchiveSetting(that, callback) {
        let currNode = that.props.data;
        let stateData = that.state;
        currNode.Rules = that.addedRules;
        currNode.IsWorkflowDefinition = stateData.isWorkflowChecked;
        currNode.IsManagedMetadataService = stateData.isIncludeManagedChecked;
        currNode.IsEnableSuperUserDecrypt = stateData.isSuperUserChecked;
        currNode.IsEnableRemoveRetentionLabel = RM.gData.enviromentName != Enviroments.ChinaNorth ? stateData.isRemoveRetentionLabelChecked : false;

        $$.loading(true);
        let sendUrl = "/api/OneDriveSettingApi/SaveArchiverNodeSetting";
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
                if (resultData.FaildType == 10) {
                    showToast.error(resultData.ErrorMessage);
                    callback(true);
                }
            }
        }).catch((e) => {
            $$.loading(false);
        });
    },
};