import React, { useState, useEffect } from "react";
import AuditRowTemplate from "./Table/AuditRowTemplate";

const ShowAuditInfoPanel = ({ show, onHide, item }) => {

    const [auditInfoes, setAuditInfoes] = useState([]);

    useEffect(() => {
        show && GetCurrentObject();
        !show && setAuditInfoes([]);
    }, [show]);

    const GetCurrentObject = () => {
        $$.loading(true);
        let url = `/api/PhysicalRecordApi/GetPhysicalActionAuditsById?Id=${item[0].Id}`;
        let option = {
            url: url,
            method: "GET",
        };
        fetchUtility(option).then((res) => {
            $$.loading(false);
            if(res && res.length > 0){
                setAuditInfoes(res);
            }
        });
    }
    
    const GetAuditColumns = () => {
        return [
            {
                header: RMResx.RM_PRM_PRE_AuditTable_ActionTime,
                width: 150,
                resizeable: true,
            },
            {
                header: RMResx.RM_PRM_PRE_AuditTable_ActionUser,
                width: 100,
                resizeable: true,
            },
            {
                header: RMResx.RM_PRM_PRE_AuditTable_ActionType,
                width: 100,
                resizeable: true,
            },
            {
                header: RMResx.RM_PRM_PRE_AuditTable_NewValue,
                width: 125,
                resizeable: true,
            },
            {
                header: RMResx.RM_PRM_PRE_AuditTable_OldValue,
                width: 125,
                resizeable: true,
            },
        ];
    }

    return ( 
        <R.Panel
            header={RMResx.RM_PRM_PRE_Audit_Title}
            size={670}
            status={{show : show}}
            destroy={true}
            onHide={onHide}
        >
            <div className="physical-audit-table">
                <R.Table
                    id={"physical-audit-table"}
                    rowTemplate={AuditRowTemplate}
                    items={auditInfoes}
                    columns={GetAuditColumns()}
                ></R.Table>
            </div>
            <>
                <R.Button slot="buttons" primary classify="theme" text={RMResx.RM_JS_Common_Close} onClick={onHide} />
            </>
        </R.Panel>
    );
}

export default ShowAuditInfoPanel;