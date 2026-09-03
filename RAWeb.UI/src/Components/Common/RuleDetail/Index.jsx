import React, { useState, useImperativeHandle, forwardRef } from "react";
import RDBaseRows from "./RDComponent/RDBaseRows";
import SourceTabs from "./RDComponent/RDSourceTabRows/SourceTabs";
import { showToast } from "../../../Utilities/CommonUtil";
import "./Index.less";

const RuleDetail = ({
    ruleInfo,
    ruleSourceComponents,
    isExistPanel = true, 
    showRuleBaseDetail = true,
    showRuleSourceDetail = true,
    panelTitle = RMResx.RM_JS_Rule_Detail_Title
}, ref) => {
    
    const [showRuleDetailPanel, setShowRuleDetailPanel] = useState(false);

    const [ruleItem, setRuleItem] = useState({});

    const [module, setModule] = useState();

    useImperativeHandle(ref, () => ({
        load: ({ruleId, callback, checkModule}) => {
            if(isExistPanel){ setShowRuleDetailPanel(true);}
            setModule(checkModule);
            loadRuleDetail(ruleId, callback);
        }
    }));

    const onClosePanel = ()=>{
        setShowRuleDetailPanel(false);
    };

    const loadRuleDetail = (ruleId, callback) =>{
        $$.loading(true);
        let option = {
            url: '/api/RuleApi/GetRuleByID',
            method: 'POST',
            data: ruleId
        };
        fetchUtility(option).then((res) => {
            $$.loading(false);
            if(res && res.MessageType == 0){
                let ruleItem = JSON.parse(res.Extension);
                setRuleItem(ruleItem);
                if(callback){ callback(true); }
                return;
            }
            showToast.error(RMResx.RM_RDM_Explorer_ChangeTerm_All_Failed);
        }).catch((error) => {
            $$.loading(false);
            handleError(error);
        });
    };

    const handleError = (response) => { 
        if (response.status == 403) {
            $$.messagedialog(true, {
                classify: "warn",
                width: "550px",
                hideActions: false,
                title: RMResx.RM_JS_Common_Confirmation,
                content: RMResx.RM_RDM_NoPermissionToViewDetailTip,
                buttons: [
                    {
                        text: RMResx.RM_JS_Common_OK,
                        primary: true,
                        classify: "theme",
                        onClick: () => { $$.messagedialog(false); }
                    }
                ]
            });
            return;
        }
        showToast.error(RMResx.RM_RDM_Explorer_ChangeTerm_All_Failed);
    };
    
    const renderRDBaseRows = () => {
        if(showRuleBaseDetail){
            return <RDBaseRows ruleItem={ruleItem} module={module} />;
        }
    };

    const renderSourceTabs = () => {
        if(showRuleSourceDetail){
            return <SourceTabs 
                ruleItem={ruleInfo || ruleItem} 
                sourceComponents={ruleSourceComponents}
                showRuleBaseDetail={showRuleBaseDetail}
            />;
        }
    };

    const renderRuleDetailContent = () => {
        return <div className="ra-rule-detail-panel">
            {renderRDBaseRows()}
            {renderSourceTabs()}
        </div>;
    };
    
    if (isExistPanel) {
        return <R.Panel
            header={panelTitle}
            size={664}
            destroy={true}
            status={{ show: showRuleDetailPanel }}
            onHide={onClosePanel}
        >
            {renderRuleDetailContent()}
            <R.Button slot="buttons" primary classify="theme" text={RMResx.RM_JS_Common_Close} onClick={onClosePanel} />
        </R.Panel>;
    }
    return renderRuleDetailContent();
};

export default forwardRef(RuleDetail);

