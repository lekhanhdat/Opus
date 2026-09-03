import { showToast } from "../../../Utilities/CommonUtil";

const Messagebox = ({ content, actionFun }) =>{
    $$.messagedialog(true, {
        width: '550px',
        hideActions: false,
        title: RMResx.RM_JS_Common_Confirmation,
        content: content,
        buttons: [
            { text: RMResx.RM_JS_Common_Cancel, onClick: ()=>{ $$.messagedialog(false); } },
            { text: RMResx.RM_JS_Common_OK, primary: true, classify: "theme", onClick: actionFun}, 
        ],
    });
};

const ActionSuccessfulNeedJobToast = () =>{
    showToast.success( <$g.I18NProvider msg={RMResx.RM_JS_BCM_TermSync_SyncSuccessMessage}>
        <a className="ra-link-a" href="/Root/JM/Index">{RMResx.RM_JS_JM_Title}</a>
    </$g.I18NProvider>);
};

export {Messagebox, ActionSuccessfulNeedJobToast};       