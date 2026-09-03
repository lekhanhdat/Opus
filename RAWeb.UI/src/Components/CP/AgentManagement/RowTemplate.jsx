import {AgentSourceType} from "../../../Constants/Constants";
import { isEnableMultiGeoFeature, isShowActionByDC, LicenseHelper } from "../../../Utilities/CommonUtil";

const agentSourceInco = {
    0: "",
    1: "ra-agent-fs-source-icon",
    2: "ra-agent-sp-source-icon",
};

const agentSourceName = {
    0: "",
    1: RMResx.RM_JS_SPS_TabLabel_FS,
    2: RMResx.RM_Common_SharePointOnPremise
};

const agentStatusErrorCode = {
    FileSystem: 1,
    SharePoint: 2,
    NoFileSystemLicense: 4, 
    NoSharePointLicense: 8,
};

const agentStatusErrorMsg = {
    0: "",
    1: "",
    2: RMResx.RM_CP_Agent_Column_ActiveException_Tooltip,
    4: RMResx.RM_CP_Agent_NoHasFSLicense,
    8: RMResx.RM_CP_Agent_NoHasSPOnPremLicense
};
const isMultiGeoMainDC = isShowActionByDC();
const enableMultiGeoFeature = isEnableMultiGeoFeature();
export class TableRow extends R.TableRow {
    constructor(props) {
        super(props);
        this.emptyVersonWord = "N/A";
        this.serviceStatus = {
            NotInstalled: 0,
            InActive: 1,
            Active: 2,
            Deleted: 3,
            Disabled: 4,
            Mismatched: 5,
			ActiveException: 6,
			Upgrading: 7
        };
        this.serviceNames = {
            0: RMResx.RM_CP_Agent_Column_Status_NotInstalled,
            1: RMResx.RM_CP_Agent_Column_Status_InActive,
            2: RMResx.RM_CP_Agent_Column_Status_Active,
            4: RMResx.RM_CP_Agent_Column_Status_Disabled,
            5: RMResx.RM_CP_Agent_Column_Status_Mismatched,
			6: RMResx.RM_CP_Agent_Column_Status_ActiveException,
			7: RMResx.RM_CP_Agent_Column_Status_Upgrading
        };
    }

    onCellClick (actionType) {
        if (actionType) {
            this.dispatch(actionType);
        }
    }

    getIconClass (rowData) {
        let className = '';
        if (rowData) {
            switch (rowData.Status) {
                case this.serviceStatus.NotInstalled:
                    className = 'not-installed-img';
                    break;
                case this.serviceStatus.InActive:
                    className = 'inActive-img';
                    break;
                case this.serviceStatus.Active:
                    className = 'active-img';
                    break;
                case this.serviceStatus.Disabled:
                    className = 'disabled-img';
                    break;
                case this.serviceStatus.Mismatched:
                    className = 'fia-mismatched';
                    break;
                case this.serviceStatus.ActiveException:
                    className = 'fia-error';
					break;
				case this.serviceStatus.Upgrading:
                    className = 'fia-in-progress';
                    break;
                default:
            }
        }
        return className;
    }

    getSourceType(binaryNum, binaryEnum){
        let sourceTypes = RM.deepcopy(binaryEnum);
        let currentColSourceTypes= [];
        for(let key in sourceTypes){
            let sourceType = sourceTypes[key];
            if((binaryNum & sourceType) == sourceType){
                currentColSourceTypes.push(sourceType);
            }
        }
        return currentColSourceTypes;
    }

    getOptionBtns (rowData) {
        let isNotInstalled = rowData.Status == this.serviceStatus.NotInstalled;
        let isActive = rowData.Status == this.serviceStatus.Active;
        let isDisabled = rowData.Status == this.serviceStatus.Disabled;
        let isActiveException = rowData.Status == this.serviceStatus.ActiveException;
        return <div>
            {
                isNotInstalled && 
                    <R.Button type="bald" icon="fia-download" tooltip={RMResx.RM_CP_Agent_Column_Action_DownloadConfigFile} onClick={this.onCellClick.bind(this, "download")}/>
            }
            {
                (isActive || isActiveException) &&
                    <R.Button type="bald" icon="fia-disable-agent" tooltip={RMResx.RM_CP_Agent_Column_Action_DisabledAgent} onClick={this.onCellClick.bind(this, "disable")}/>

            }
            {
                isDisabled &&  
                    <R.Button type="bald" icon="fia-enable-agent" tooltip={RMResx.RM_CP_Agent_Column_Action_EnableAgent} onClick={this.onCellClick.bind(this, "enable")}/>
            }
            {
                !isActive &&
                    <R.Button type="bald" icon="fia-delete" tooltip={RMResx.RM_JS_Common_Delete} onClick={this.onCellClick.bind(this, "delete")}/>
            }
        </div>;
    }

    getStatusTooltipMsg(statusCodeList, currentAgentStatus){
        //多于一条显示序号；
        let statusTooltipMsg = "";
        if(currentAgentStatus == this.serviceStatus.InActive || currentAgentStatus == this.serviceStatus.ActiveException){
            let statusTooltipMsgCount = 0;
            for(let statusCode of statusCodeList){
                if(agentStatusErrorMsg[statusCode]){
                    statusTooltipMsgCount++;
                }
            }
            let statusTooltipMsgIndex = 1;
            for(let statusCode of statusCodeList){
                if(statusTooltipMsgCount < 2){
                    statusTooltipMsg = agentStatusErrorMsg[statusCode];
                }else{
                    statusTooltipMsg += `${statusTooltipMsgIndex}. ${agentStatusErrorMsg[statusCode]}` + "\n";
                    if(agentStatusErrorMsg[statusCode]){
                        statusTooltipMsgIndex++;
                    }
                }
            }
        }
        return statusTooltipMsg;
    }

    onKeyDown(e){
        if(e.keyCode == "13"){
            this.onCellClick.bind(this, "edit"); 
        }
	}
	
	getVersionTag(rootData, rowData) {
		if (rowData.Status === 0) {
			return null;
		}
		if (rootData === rowData.Version) {
			return <span className="column-version-latest">{RMResx.RM_CP_Agent_Column_Version_Latest}</span>;	
		} else {
			return <span className="column-version-new">{RMResx.RM_CP_Agent_Column_Version_New}</span>;
		}
	}

    render (Row, Cell) {
		let rowData = this.props.rowData;
		let rootData = this.props.rootData;
        let dataCenter = rowData.DCDisplayName || "";
		
        let statusIconClass = this.getIconClass(rowData);
        let optionBtns = this.getOptionBtns(rowData);
        let serverVersion = rowData.Version == this.emptyVersonWord ? RMResx.RM_JS_Common_Pending : rowData.Version;
        let sourceTypes = this.getSourceType(rowData.SourceType, AgentSourceType);
        let statusCodeList = this.getSourceType(rowData.Errors, agentStatusErrorCode);
        let statusTooltipMsg = this.getStatusTooltipMsg(statusCodeList, rowData.Status);

        return <Row>
            <Cell>
                {
                    sourceTypes.map((source,key)=>{
                        return <span 
                            key={key}
                            className={"ra-agent-source-icon " + agentSourceInco[source]}  
                            data-tooltip 
                            aria-label={agentSourceName[source]}>
                        </span>;
                    })
                }
            </Cell>
            <Cell>
                <div>
                    <a tabIndex='0' className="ra-link-a text-overflow ra-main-cell-link" data-tooltip aria-label={rowData.Name} onClick={this.onCellClick.bind(this, "edit")} onKeyDown={this.onKeyDown}>
                        {rowData.Name}
                    </a>
                </div>
            </Cell>
            <Cell>
                <div className="text-overflow" tabIndex='0' data-tooltip aria-label={rowData.Description}>
                    {rowData.Description}
                </div>
            </Cell>
            {enableMultiGeoFeature &&
                <Cell>
                    <div className="text-overflow" tabIndex='0' data-tooltip aria-label={dataCenter}>
                        {dataCenter}
                    </div>
                </Cell>
            }
            <Cell>
                <div 
                    className="agent-status" 
                    tabIndex='0' 
                    data-tooltip={!!statusTooltipMsg} 
                    aria-label={statusTooltipMsg}
                >
                    <span className={statusIconClass}></span>
                    <span className="agent-status-name">{this.serviceNames[rowData.Status]}</span>
                </div>
            </Cell>
            <Cell>
                <div className="text-overflow" tabIndex='0'>
					<span className="version-mr">{serverVersion}</span>
					{LicenseHelper.EnableJPMCFileSystemFeature() && this.getVersionTag(rootData, rowData)}
                </div>
            </Cell>
            <Cell>
                <div className="text-overflow" tabIndex='0'>
                    {rowData.ServerName}
                </div>
            </Cell>
            {isMultiGeoMainDC && 
                <Cell>
                    {optionBtns}
                </Cell>
            }
        </Row>;
    }
}

export class CertificatesTempalte extends R.TableRow {
    constructor(props) {
        super(props);
        this.state = {
            rowData: RM.deepcopy(this.props.rowData)
        };
    }

    operate = (actionType) =>{
        if(actionType == "setDefault"){
            this.props.rowData.IsDefault = false;
        }
        this.setState({});
        this.dispatch(actionType);
    }

    getStatusHtml(isExpired){
        let statusIconClass = isExpired ? "red-circle" : "green-circle";
        let statusText = isExpired ? RMResx.RM_CP_AM_Certificate_Column_Expired : RMResx.RM_CP_AM_Certificate_Column_Available;
        let statusHtml = <div className="flex ra-flex-align-center">
            <div className={statusIconClass}></div>
            <div className="margin-left-xs">{statusText}</div>
        </div>;
        return statusHtml;      
    }

    render (Row, Cell) {
        let rowData = this.props.rowData;
        let statusHtml = this.getStatusHtml(rowData.IsExpired);
        //+ RM.TimeUtil.getTimezoneInfo(RM.TimeSettingModel.TimeZoneId, RM.TimeSettingModel.isSetDayLight).simplifyDisplayName;
        let expriationTime = $$.date.format(rowData.ValidTo, RM.TimeSettingModel.DateFormat);
        return <Row>
            <Cell key={Math.random()}>
                <R.Radio checked={rowData.IsDefault} disabled={rowData.IsExpired} onChange={this.operate.bind(this, "setDefault")}/>
            </Cell>
            <Cell>
                <div className="strong text-overflow" data-tooltip aria-label={rowData.Thumbprint}>{rowData.Thumbprint}</div>
            </Cell>
            <Cell>
                <div className="text-overflow" data-tooltip aria-label={expriationTime}>{expriationTime}</div>
            </Cell>
            <Cell>
                <div className="text-overflow">{statusHtml}</div>
            </Cell>
            <Cell>
                <R.Button type="plain" icon="fia-download" tooltip={RMResx.RM_CP_Agent_Download_Btn} onClick={this.operate.bind(this, "download")}/>
                {
                    !rowData.IsDefault && <R.Button type="plain" tooltip={RMResx.RM_JS_Common_Delete} icon="fia-delete" onClick={this.operate.bind(this, "delete")}/>
                }
            </Cell>
        </Row>;
    }
}

