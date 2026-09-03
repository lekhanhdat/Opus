import StringUtil from "../../../../Utilities/StringUtil";
import { Source } from "../Constants";
import { ApprovalStatusI18Ns,ApprovalStatus } from "../Constants/ApprovalStatus";
import { RelatedRecordsActionI18Ns } from "../Constants/RelatedRecordsAction";
import ManualReviewDetailPanel from "../Details/ManualReivewDetailPanel";
import ManualReviewTableRow from "../Details/ManualReviewTableRow";
import { forwardRef,useImperativeHandle, useState } from "react";

const BuildWaitingDisposalDetails = (record) =>{
    const linkSources = new Set([Source.SharePoint, Source.OneDrive, Source.Teams]);
    let linkType = linkSources.has(record.sourceFlag) && record.retentionStatus === 0 ? "link" : "";
    if(record.fileExtension === RMResx.RM_RDM_RecordDetails_DataType_SPItem && record != 5){
        linkType = "itemLink";
    }
    let sourceName = record.sourceFlag >= 999 ? record.sourceName : record.sourceFlag;
    let fileExtension = `${record.fileExtension}${record.retentionStatus === 1 ? ` (${RMResx.RM_MA_Extended_RetentionStatus})` : ""}`;
    let approvalStatus = ApprovalStatusI18Ns.get(record.internalApprovedStatus);
    if(record.internalApprovedStatus === ApprovalStatus.WorkflowComplete) {
        approvalStatus += ` (${ApprovalStatusI18Ns.get(record.approvedStatus)})`;
    }
    let disposalAction = record.isRelatedRecords ? RelatedRecordsActionI18Ns.get(record.relatedRecordsAction) : "";
    return [
        {key : 0, column : StringUtil.trimEndColon(RMResx.RM_JS_BCM_Explorer_Details_DataSource) ,value : sourceName ,type:"source"},
        {key : 1, column : RMResx.RM_JS_MA_Grid_Title ,value : record.leafName ,type:""},
        {key : 2, column : RMResx.RM_JS_MA_Grid_RecordsId ,value : record.recordsId ,type:""},
        {key : 3, column : RMResx.RM_JS_MA_Grid_FullPath ,value : record.fullPath , type: linkType},
        {key : 4, column : RMResx.RM_JS_MA_Grid_FolderPath ,value : record.manualFolderPath , type:""},
        {key : 5, column : StringUtil.trimEndColon(RMResx.RM_JS_BCM_Explorer_Datagrid_FileType) ,value : fileExtension ,type:""},
        {key : 6, column : RMResx.RM_JS_MA_Grid_ApprovalStatus ,value : approvalStatus ,type:""},
        {key : 7, column : RMResx.RM_JS_MA_Grid_Rule ,value : record.ruleName ,type:""},
        {key : 8, column : RMResx.RM_MA_LastReasonforRejection ,value : record.manualLastReasonForRejection,type:""},
        {key : 9, column : RMResx.RM_MA_JS_LastApproveRejectComment ,value : record.manualLastApproveRejectComment ,type:""},
        {key : 10, column : RMResx.RM_JS_Rule_DisposalClass_Title ,value : record.ruleDisposalClass ,type:""},
        {key : 11, column : RMResx.RM_JS_MA_Grid_RelatedRecords ,value : record.relatedRecords, type: "related"},
        {key : 12, column : RMResx.RM_JS_MA_Grid_RelatedRecordsAction ,value : disposalAction, type:""},
        {key : 13, column : RMResx.RM_MA_Grid_EscalateOrReassignFrom ,value : record.escalateFromDisplayName ,type:""},        
        {key : 14, column : RMResx.RM_JS_MA_Grid_RecordOwner ,value : record.reviewerDisplayNames.join("; ") ,type:""},
        {key : 15, column : RMResx.RM_JS_MA_Grid_ApprovedBy , value : record.approvedByDisplayName ,type :"" },
        {key : 16, column : RMResx.RM_JS_MA_Grid_Reassigned_Comment ,value : record.escalatedComment ,type:""},
        {key : 17, column : RMResx.RM_MA_JS_LastReviewedBy ,value : record.manualLastReviewedBy ,type:""},
        {key : 18, column : RMResx.RM_MA_JS_LastReviewTime ,value : record.manualLastReviewTime ,type:""},
        {key : 19, column : RMResx.RM_JS_MA_Grid_ModifiedBy ,value : record.modifiedBy ,type:""},
        {key : 20, column : RMResx.RM_JS_MA_Grid_ModifiedTime ,value : record.modifiedTime ,type:""},
        {key : 21, column : RMResx.RM_JS_MA_Grid_CreatedBy ,value : record.createdBy ,type:""},
        {key : 22, column : RMResx.RM_JS_MA_Grid_CreatedTime ,value : record.collectionTime ,type:""},

    ];
};


const WaitingDisposalDetails = forwardRef ((props, ref) =>{

    const [isShow , setIsShow] = useState(false);

    const [record , setRecord] = useState({});

    const [columns, setColumns] = useState([]);

    const [tabIndex, setTabIndex] = useState(0);

    useImperativeHandle(ref, () => ({
        onShow: (record) => {
            setIsShow(true);
            setRecord(record);
            setColumns(BuildWaitingDisposalDetails(record));
        }
    }));

    const GetManualReviewInfos = () => {
        if(!isShow){
            return;
        }
        return JSON.parse(record.manualAudit);
    };

    const GetManualReviewColumns = () =>{
        return [{
            header: RMResx.RM_JS_BCM_Explorer_Details_ReviewedTime,
            width: 280,
            resizeable: true,
        }, 
        {
            header: RMResx.RM_JS_BCM_Explorer_Details_ReviewedBy,
            width: 180,
            resizeable: true,
        }, 
        {
            header: RMResx.RM_JS_BCM_Explorer_Details_ReviewedAction,
            width: 180,
            resizeable: true,
        }, 
        {
            header: RMResx.RM_MA_ApprovalCommentTerm,
            width: 200,
            resizeable: true,
        }, 
        {
            header: RMResx.RM_JS_MA_Grid_Comment,
            width: 280,
            resizeable: true,
        },
        {
            header: RMResx.RM_MA_ActionExtendDisposalDate,
            width: 280,
            resizeable: true,
        }];
    };

    const onHide = () => {
        setIsShow(false);
        setTabIndex(0);
    };

    const handleTabIndexChanged = (args) =>{
        setTabIndex(args);
    };

    return (
        <ManualReviewDetailPanel 
            details={columns} 
            manualInfos={GetManualReviewInfos()}
            isShow={isShow} 
            onHide={onHide} 
            handleTabIndexChanged={handleTabIndexChanged}
            tabIndex={tabIndex}
            template={ManualReviewTableRow}
            columns={GetManualReviewColumns()}
        >
        </ManualReviewDetailPanel>
    );
});

WaitingDisposalDetails.displayName = "WaitingDisposalDetails";

export default WaitingDisposalDetails;