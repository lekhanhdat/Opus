import {formatBoolean} from "../../../../../Utilities/CommonUtil";
import DetailRow from "../../Common/DetailRow";

const ManualApprove = ({ruleItem}) =>{

    const {EnableManualApproval, WorkflowId} = ruleItem;
    
    const getRecordReviewersRowValue = () =>{
        return ruleItem.Users?.map((item, index) => {
            return <div key={index}>
                {item.DisplayName}
            </div>;
        });
    };

    const manualApproveRows = [
        {
            label: RMResx.RM_JS_Rule_Detail_Approval,
            value: formatBoolean(EnableManualApproval),
            show: true
        },
        {
            label: RMResx.RM_JS_MA_Grid_RecordOwner,
            value: getRecordReviewersRowValue(),
            show: EnableManualApproval && !WorkflowId
        },
        {
            label: RMResx.RM_JS_Rule_Detail_ProcessName,
            value: ruleItem.WorkflowName,
            show: EnableManualApproval && WorkflowId
        },
        {
            label: RMResx.RM_JS_MA_Grid_SendEmailRecordOwner,
            value: formatBoolean(ruleItem.IsSendEmailToOwner),
            show: EnableManualApproval
        }
    ];

    return <React.Fragment>
        {
            manualApproveRows.map((item,index)=>{
                return (item.show && <DetailRow key={index} label={item.label}>
                    {item.value}                      
                </DetailRow>);
            })
        }
    </React.Fragment>;
};

export default ManualApprove;