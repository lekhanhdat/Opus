import { JobStatusI18N, JobType, JobPriorityI18N, JobProgressStatusI18N, StatusCode } from "./JMConstants";
import { LicenseHelper } from "../../Utilities/CommonUtil";
import { NormalCell } from "../Common/TableTemplateCell";
import { DateUnit } from "../Home/Dashboard/RetentionAndDestroyView/Constants";
export class JobMounitorTemplate extends R.TableRow {
    constructor(props) {
        super(props);
        this.state = {};
    }

    renderProgressbar(progressNum) {
        return <R.Progressbar
            id="JMbar"
            value={progressNum}
            max={100}
            classify="success"
            animated={true}
            template="numeric"
        />;
    }

    cellClick = () => {
        this.dispatch('cellClick');
    }

    onKeyDown(e) {
        if (e.keyCode == 13) {
            e.target.click();
        }
    }

    render(Row, Cell) {
        let rowData = this.props.rowData;
        let jobStatus = JobStatusI18N[rowData.Status];
        let jobPriority = JobPriorityI18N[Number(rowData.JobPriority)+1];
        let progressRow = this.renderProgressbar(rowData.Progress);
        const isNewOpus = LicenseHelper.EnableRecordsArchiver() ;
        return <Row>
            {rowData.EnableJobIdColLink && <Cell>
                <div className="ra-main-cell-link" onClick={this.cellClick} tabIndex="0" onKeyDown={this.onKeyDown}>{rowData.JobId}</div>
            </Cell>}
            {!rowData.EnableJobIdColLink && <Cell>
                <div tabIndex="0">{rowData.JobId}</div>
            </Cell>}
            <Cell>{rowData.JobType}</Cell>
            <Cell>{progressRow}</Cell>
            <Cell>{jobStatus}</Cell>
            <Cell>{jobPriority}</Cell>
            {isNewOpus && <NormalCell Cell={Cell} contentText={rowData.Joblocation} tooltip={rowData.Joblocation} />}
            <Cell>{rowData.StartTime}</Cell>
            <Cell>{rowData.EndTime}</Cell>
            <NormalCell Cell={Cell} contentText={rowData.UserName} tooltip={rowData.UserName} />
        </Row>;
    }
}

export class JobQueueTemplate extends R.TableRow {
    constructor(props) {
        super(props);
        this.state = {};
    }

    render(Row, Cell) {
		let rowData = this.props.rowData;
		let jobPriority = JobPriorityI18N[Number(rowData.JobPriority)+1];
        return <Row>
            <Cell>{rowData.JobType}</Cell>
            <NormalCell Cell={Cell} contentText={rowData.CreatedBy} tooltip={rowData.CreatedBy} />
            <Cell>{rowData.CreatedTime}</Cell>
            <Cell>{jobPriority}</Cell>
        </Row>;
    }
}

export class DisposalJobTemplate extends R.TableRow {
    constructor(props) {
        super(props);
        this.state = {};
    }

    renderProgressbar(progressNum) {
        return <R.Progressbar
            id="JMbar"
            value={progressNum}
            max={100}
            classify="success"
            animated={true}
            template="numeric"
        />;
    }

    cellClick = () => {
        this.dispatch('cellClick');
    }

    render(Row, Cell) {
        let rowData = this.props.rowData;
        let jobStatus = JobStatusI18N[rowData.Status];
        let progressRow = this.renderProgressbar(rowData.Progress);
        return <Row>
            <Cell>{rowData.Order}</Cell>
            <Cell>
                <div className="ra-main-cell-link" onClick={this.cellClick}>{rowData.JobId}</div>
            </Cell>
            <Cell>{rowData.JobType}</Cell>
            <Cell>{progressRow}</Cell>
            <Cell>{jobStatus}</Cell>
            <Cell>{rowData.StartTime}</Cell>
            <Cell>{rowData.EndTime}</Cell>
        </Row>;
    }
}

export class JobDetailTemplate extends R.TableRow {
    constructor(props) {
        super(props);
        this.state = {};
    }

    renderProgressbar(progressNum) {
        return <R.Progressbar
            id="JMbar"
            value={progressNum}
            max={100}
            classify="success"
            animated={true}
            template="numeric"
        />;
    }

    cellClick = () => {
        this.dispatch('cellClick');
    }

    renderCellValue = (cellValue) => {
        const jobType = this.props.rootData.jobType;
        const rowData = this.props.rowData;
        if ([JobType.ArchiverRetentionSimulate, JobType.FSRetainSimulate].includes(jobType) && cellValue === "RetentionSetting") {
            const content = RMResx.RM_DSB_Retention_Column_SettingValue.format(
                rowData.RetentionSource,
                rowData.RetentionKeepDate,
                DateUnit[rowData.RetentionKeepDateUnit],
            );
            return content;
        }

        return cellValue;
    }

    render(Row, Cell) {
        let rowData = this.props.rowData;
        let cellValues = rowData.cellValues;
        const nameOrTitleValues = [rowData.ObjectName, rowData.TitleOrName, rowData.Title, rowData.Name];
        return <Row>
            {
                cellValues.map((cellValue, index) => {
                    return <Cell key={index}>
                        <div className={`text-overflow ${nameOrTitleValues.includes(cellValue) ? "ra-common-pre" : ""}`} data-tooltip data-tooltip-wrap="force" aria-label={this.renderCellValue(cellValue)}>
                            {this.renderCellValue(cellValue)}
                        </div>
                    </Cell>;
                })
            }
        </Row>;
    }
}

export class JobDetailTermTemplate extends R.TableRow {
    constructor(props) {
        super(props);
        this.state = {};
    }

    render(Row, Cell) {
        let rowData = this.props.rowData;
        return <Row>
            <Cell>{rowData.Term}</Cell>
            <Cell>{rowData.TermFullPath}</Cell>
        </Row>;
    }
}

export class SubJobDetailTemplate extends R.TableRow {
    constructor(props) {
        super(props);
        this.state = {};
    }

    cellClick = (action) => {
        this.dispatch(action);
    }

    onKeyDown(e) {
        if (e.keyCode == 13) {
            e.target.click();
        }
    }

    render(Row, Cell) {
        const rowData = this.props.rowData;
        const jobStatus = JobStatusI18N[rowData.Status];
        const isFinishedJob = [StatusCode.Finished, StatusCode.FinishWithException, StatusCode.Stopped, StatusCode.Failed, StatusCode.Skipped].includes(rowData.Status);
        
        return <Row>
            <Cell>
                {isFinishedJob ? (
                    <div className="ra-main-cell-link" onClick={() => this.cellClick("SubJobIDClicked")} tabIndex="0" onKeyDown={this.onKeyDown}>{rowData.SubJobID}</div>
                ) : (
                    <div tabIndex="0" >{rowData.SubJobID}</div>
                )}
            </Cell>
            <Cell>{jobStatus}</Cell>
            <NormalCell Cell={Cell} contentText={rowData.Scope} tooltip={rowData.Scope} />
            <Cell>
                {rowData?.SuccessfulCount && rowData.SuccessfulCount > 0
                    ? (
                        <div className="ra-main-cell-link" onClick={() => this.cellClick("SuccessfulCountClicked")} tabIndex="0" onKeyDown={this.onKeyDown}>
                            {rowData.SuccessfulCount}
                        </div>
                    )
                    : rowData.SuccessfulCount ?? 0
                }
            </Cell>
            <Cell>
                {rowData?.FailedCount && rowData.FailedCount > 0
                    ? (
                        <div className="ra-main-cell-link" onClick={() => this.cellClick("FailedCountClicked")} tabIndex="0" onKeyDown={this.onKeyDown}>
                            {rowData.FailedCount}
                        </div>
                    )
                    : rowData.FailedCount ?? 0
                }
            </Cell>
            <Cell>
                {rowData?.SkippedCount && rowData.SkippedCount > 0
                    ? (
                        <div className="ra-main-cell-link" onClick={() => this.cellClick("SkippedCountClicked")} tabIndex="0" onKeyDown={this.onKeyDown}>
                            {rowData.SkippedCount}
                        </div>
                    )
                    : rowData.SkippedCount ?? 0
                }
            </Cell>
            <NormalCell Cell={Cell} contentText={rowData.Comment} tooltip={rowData.Comment} />
        </Row>;
    }
}

export class JobProgressTemplate extends R.TableRow {
    constructor(props) {
        super(props);
    }

    render(Row, Cell) {
        const rowData = this.props.rowData;
        const jobStatus = JobProgressStatusI18N[rowData.JobStatus];
        
        return <Row>
            <Cell>
                <div className="text-overflow" tabIndex="0" aria-label={rowData.SubJobID} data-tooltip="ifneed" data-tooltip-wrap="force">{rowData.SubJobID}</div>
            </Cell>
            <Cell>
                <div className="text-overflow" tabIndex="0" aria-label={jobStatus} data-tooltip="ifneed" data-tooltip-wrap="force">{jobStatus}</div>
            </Cell>
            <Cell>
                <div className="text-overflow" tabIndex="0" aria-label={rowData.Scope} data-tooltip data-tooltip-wrap="force">{rowData.Scope}</div>
            </Cell>
            <Cell>
                <div className="text-overflow" tabIndex="0" aria-label={rowData.StartTime} data-tooltip="ifneed" data-tooltip-wrap="force">{rowData.StartTime}</div>
            </Cell>
            <Cell>
                <div className="text-overflow" tabIndex="0" aria-label={rowData.FinishTime} data-tooltip="ifneed" data-tooltip-wrap="force">{rowData.FinishTime}</div>
            </Cell>
            <Cell>
                <div className="text-overflow" tabIndex="0" aria-label={rowData.ScannedFiles} data-tooltip="ifneed" data-tooltip-wrap="force">{rowData.ScannedFiles}</div>
            </Cell>
            <Cell>
                <div className="text-overflow" tabIndex="0" aria-label={rowData.EstimatedScanFinishedTime} data-tooltip="ifneed" data-tooltip-wrap="force">{rowData.EstimatedScanFinishedTime}</div>
            </Cell>
            <Cell>
                <div className="text-overflow" tabIndex="0" aria-label={rowData.ExportedFiles} data-tooltip="ifneed" data-tooltip-wrap="force">{rowData.ExportedFiles}</div>
            </Cell>
            <Cell>
                <div className="text-overflow" tabIndex="0" aria-label={rowData.EstimatedExportFinishedTime} data-tooltip="ifneed" data-tooltip-wrap="force">{rowData.EstimatedExportFinishedTime}</div>
            </Cell>
            <Cell>
                <div className="text-overflow" tabIndex="0" aria-label={rowData.ArchivedFiles} data-tooltip="ifneed" data-tooltip-wrap="force">{rowData.ArchivedFiles}</div>
            </Cell>
            <Cell>
                <div className="text-overflow" tabIndex="0" aria-label={rowData.ArchivedSize} data-tooltip="ifneed" data-tooltip-wrap="force">{rowData.ArchivedSize}</div>
            </Cell>
            <Cell>
                <div className="text-overflow" tabIndex="0" aria-label={rowData.EstimatedArchiveFinishedTime} data-tooltip="ifneed" data-tooltip-wrap="force">{rowData.EstimatedArchiveFinishedTime}</div>
            </Cell>
            <Cell>
                <div className="text-overflow" tabIndex="0" aria-label={rowData.OtherActions} data-tooltip="ifneed" data-tooltip-wrap="force">{rowData.OtherActions}</div>
            </Cell>
            <Cell>
                <div className="text-overflow" tabIndex="0" aria-label={rowData.EstimatedOtherFinishedTime} data-tooltip="ifneed" data-tooltip-wrap="force">{rowData.EstimatedOtherFinishedTime}</div>
            </Cell>
            <Cell>
                <div className="text-overflow" tabIndex="0" aria-label={rowData.LastUpdatedTime} data-tooltip="ifneed" data-tooltip-wrap="force">{rowData.LastUpdatedTime}</div>
            </Cell>
        </Row>;
    }
}