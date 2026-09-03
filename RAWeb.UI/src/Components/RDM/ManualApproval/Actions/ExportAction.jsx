import { showToast } from "../../../../Utilities/CommonUtil";
import { RoleType } from "../Constants/RoleType";

export default class ExportAction {

    static onExport = async (queryDefintion) => {
        let requestOption = {
            url: "/api/ManualApproval/RunExportRecordsForReviewDataJob",
            data: queryDefintion,
        };
        
        
        $$.loading(true);
        const result = await fetchUtility(requestOption);
        $$.loading(false);

        if (result.MessageType === 0) {
            if (RM.RoleType != RoleType.StandardUser) {
                showToast.success(<$g.I18NProvider msg={RMResx.RM_MA_HistoryExport_JobStart}>
                    <a className="ra-link-a" href="/Root/JM/Index">{RMResx.RM_JS_JM_Title}</a>
                    <a className="ra-link-a" href="/Root/DC/Download">{RMResx.RM_JS_DC_Title}</a>
                </$g.I18NProvider>);
            } else {
                showToast.success(<$g.I18NProvider msg={RMResx.RM_MA_HistoryExport_EndUser_JobStart}>
                    <a className="ra-link-a" href="/Root/DC/Download">{RMResx.RM_JS_DC_Title}</a>
                </$g.I18NProvider>);
            }
        } else {
            showToast.error(result.ErrorMessage);
        }
    }
}
