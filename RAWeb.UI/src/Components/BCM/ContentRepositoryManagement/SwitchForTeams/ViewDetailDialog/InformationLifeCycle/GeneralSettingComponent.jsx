import CRMCommonUtil from "../../../Common/CRMCommonUtil";
import { EnableRecordManagementSetting } from "../../../CRMForSPO/ContentRepositoryManagementForSPO";

function GeneralSettingComponent({ nodeSetting }) {
    const supportSync = () => {
        return (
            CRMCommonUtil.isGroup(nodeSetting) ||
            CRMCommonUtil.isSiteCollection(nodeSetting)
        );
    };

    return (
        <R.Expander
            title={RMResx.RM_JS_SPS_EditTitle_GeneralManagement}
            level={2}
        >
            <div>
                <$g.DetailList>
                    <$g.DetailRow>
                        <$g.DetailCell
                            label={RMResx.RM_JS_SPS_EnableRecordsManagement}
                        >
                            <span tabIndex="0">
                                {nodeSetting.EnableRecordManagement ==
                                EnableRecordManagementSetting.Enable
                                    ? RMResx.RM_JS_Common_Yes
                                    : RMResx.RM_JS_Common_No}
                            </span>
                        </$g.DetailCell>
                    </$g.DetailRow>
                    {nodeSetting.EnableRecordManagement ==
                        EnableRecordManagementSetting.Enable &&
                        supportSync() && (
                            <$g.DetailRow>
                                <$g.DetailCell
                                    label={RMResx.RM_JS_SPS_EnableDataSync}
                                >
                                    <span tabIndex="0">
                                        {nodeSetting.IsSyncData
                                            ? RMResx.RM_JS_Common_Yes
                                            : RMResx.RM_JS_Common_No}
                                    </span>
                                </$g.DetailCell>
                            </$g.DetailRow>
                        )}
                </$g.DetailList>
            </div>
        </R.Expander>
    );
}

export default GeneralSettingComponent;
