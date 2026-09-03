import { NodeLevel } from "../../../../Constants/DAEnums";
import "./index.less";

const CARD_FIELDS = [
    { key: "NodeSize", label: RMResx.RM_FS_DriveInfo_Size },
    { key: "ConnectionId", label: RMResx.RM_FS_DriveInfo_ID },
    { key: "ClassCodeId", label: RMResx.RM_FS_DriveInfo_ClassCode },
    { key: "CountryCode", label: RMResx.RM_FS_DriveInfo_CountryCode },
    { key: "RetentionType", label: RMResx.RM_FS_DriveInfo_RetentionType },
    { key: "RetentionDate", label: RMResx.RM_FS_DriveInfo_EventDate },
    { key: "FolderCreationDate", label: RMResx.RM_FS_DriveInfo_Created },
    { key: "FolderLastModifiedDate", label: RMResx.RM_FS_DriveInfo_Updated },
    { key: "AgentName", label: RMResx.RM_FS_DriveInfo_Agent }
];

const buildCardData = (driveInfo) => {
    driveInfo = RM.deepcopy(driveInfo) || {};
    
    const classCodeKeys = ["ClassCodeId", "CountryCode", "RetentionType", "RetentionDate"];
    const nodeLevel = driveInfo?.Level;
    const filteredCardFields = nodeLevel === NodeLevel.SiteCollection
        ? CARD_FIELDS
        : CARD_FIELDS.filter(field => field.key !== "ConnectionId");
    return filteredCardFields.map(({ key, label }) => {
        const isClassCodeField = classCodeKeys.includes(key);
        if (key === "RetentionType") { 
            if (driveInfo?.ClassCode?.[key] === 1) {
                driveInfo.ClassCode.RetentionType = RMResx.RM_FS_ClassCodePolicy_RetentionEventType;
            } else if (driveInfo?.ClassCode?.[key] === 2) {
                driveInfo.ClassCode.RetentionType = RMResx.RM_FS_ClassCodePolicy_RetentionFlatType;
            }
        }
        const value = isClassCodeField ? driveInfo?.ClassCode?.[key] : driveInfo?.[key];
        
        return {
            key,
            label,
            value: value || RMResx.RM_FS_DriveInfo_None
        };
    });
};

export default class FSDriveInformation extends R.Component { 
    constructor(props) {
        super(props);

        this.state = {
            showWarningTip: false,
            warningMsgForRunningJob: '',
            data: buildCardData(props.selectedNode)
        };
    }

    componentInit() {
        this.checkApplyClassCodeJobRunning(this.props.selectedNode);
    }

    componentUpdate(prevProps) {
        if (prevProps.selectedNode?.Id !== this.props.selectedNode?.Id) {
            this.setState({
                data: buildCardData(this.props.selectedNode)
            });
        }
    }

    checkApplyClassCodeJobRunning = (node) => {
        const option = {
            url: "/API/FSSettingApi/CheckApplyClassJobRunning",
            method: "POST",
            data: node
        };
        fetchUtility(option).then((res) => {
            this.setState({
                showWarningTip: res,
                warningMsgForRunningJob: res ? RMResx.RM_FS_ClassCodePolicy_ApplyJobRunningWarning : ''
            });
        }).catch((error) => {
            console.error("Failed to check class code job running:", error);
        });
    }

    hideWarningMessageTip = () => {
        this.setState({ showWarningTip: false });
    }

    render() { 
        return (
            <>
                <R.Messagebar
                    message={this.state.warningMsgForRunningJob}
                    status={{ show: this.state.showWarningTip }}
                    classify={"warn"}
                    onClose={this.hideWarningMessageTip}
                />
                {this.state.showWarningTip && <div className="margin-bottom-m"></div>}
                <div className="card-container">
                    {this.state.data.map(item => (
                        <div key={item.key}>
                            <div className="strong" tabIndex={0}>{item.label}</div>
                            <div tabIndex={0} className="card-value" data-tooltip-wrap="force" data-tooltip="ifneed">{item.value}</div>
                        </div>
                    ))}
                </div>
            </>
        )
    }
}