import { showToast } from "../../../Utilities/CommonUtil";

const Messagebox = ({ content, actionFun, classify }) =>{
    $$.messagedialog(true, {
        width: '550px',
        hideActions: false,
        title: RMResx.RM_JS_Common_Confirmation,
        classify: classify,
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

const ShowResultMsg = (result, successfulMsg, failedMsg) => {
    if(!result.HasError){
        showToast.success(successfulMsg);
    }else{
        showToast.error(result.ErrorMsg || failedMsg);
    }
};

const MessageboxContentWithItems = ({msgboxDes, itemsTitle, items=[]}) => {
    return <div>
        {msgboxDes}
        <div className="margin-top-m">
            <div className="strong">{itemsTitle}</div>
            {
                items.map((item, index)=>{
                    return <div className="margin-top-xs" key={index}>
                        {item}
                    </div>;
                })
            }
        </div>
    </div>;
};

export { Messagebox, ActionSuccessfulNeedJobToast, ShowResultMsg, MessageboxContentWithItems };       