import RouterUrls from "../../../../../../Constants/RouterUrls";
import { checkPermission } from "../../../../../../Utilities/permissionManager";
import { EnableRecordManagementSetting } from "../../../CRMForSPO/ArchiveCRMForSPO";


// (nodeSetting.Level === NodeLevel.WebApplication ||
//                         nodeSetting.Level === NodeLevel.SiteCollection ||
//                         nodeSetting.Level ===
//                             NodeLevel.Office365GroupEntire) &&

function GeneralSettingComponent({ nodeSetting }) {
    return (
        <R.Expander
            title={RMResx.RM_JS_SPS_EditTitle_GeneralManagement}
            level={2}
        >
            <div>
                <$g.DetailList>
                    <$g.DetailRow>
                        <$g.DetailCell
                            label={RMResx.RM_AR_SPS_General_EnableArchiveManagement}
                        >
                            <span tabIndex="0">
                                {nodeSetting.EnableArchiverManagement ==
                                EnableRecordManagementSetting.Enable
                                    ? RMResx.RM_JS_Common_Yes
                                    : RMResx.RM_JS_Common_No}
                            </span>
                        </$g.DetailCell>
                    </$g.DetailRow>
                    {RM.gData.enableDeleteRestoredDataFeature &&
                        checkPermission(RouterUrls.CP_Index, RM.UserResources) && (
                            <$g.DetailRow>
                                <$g.DetailCell
                                    label={
                                        RMResx.RM_AR_SPS_General_EnableRestoreManagement
                                    }
                                >
                                    <span tabIndex="0">
                                        {nodeSetting.EnableDelArchivedData
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
