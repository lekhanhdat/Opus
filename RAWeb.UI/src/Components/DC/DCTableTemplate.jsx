import * as JMConstants from "../JM/JMConstants";
import '../../Less/DC/downloadCenter.less';
import { SourceFlags, ArchivedContentFileType } from "../../Constants/Constants";

const SourceIcons = new Map([
    [SourceFlags.SP, "fi-ms-sharepoint"],
    [SourceFlags.OneDrive, "fi-ms-onedrive"],
    [SourceFlags.Teams, "fi-ms-teams"],
]);

const FileIcons = new Map([
    [ArchivedContentFileType.Zip, "fi-file-zip"],
]);

export class DCTableTemplate extends R.TableRow {
    constructor(props) {
        super(props);
        this.state = {};
    }

    getFileAndSourceIcons(rowData) {
        let sourceFlag = rowData.SourceFlag;
        let fileType = rowData.FileType;
        let iconCellContent = sourceFlag ? 
            SourceIcons.get(sourceFlag) : 
            FileIcons.get(fileType);
        return <span className={iconCellContent + " margin-right-s"}></span>;
    }

    renderJobStatus(status) {
        return (
            <div className="flex ra-flex-align-center">
                {status == JMConstants.StatusCode.Wait && (
                    <React.Fragment>
                        <span
                            className="fia-radiobutton-bg-device ra-dc-iconstyle"
                            style={{ color: "#F7941D" }}
                        ></span>
                        <span>{RMResx.RM_JS_DC_Download_Wait}</span>
                    </React.Fragment>
                )}
                {status == JMConstants.StatusCode.InProgerss && (
                    <React.Fragment>
                        <span
                            className="fia-radiobutton-bg-device ra-dc-iconstyle"
                            style={{ color: "#0072D0" }}
                        ></span>
                        <span>{RMResx.RM_JS_DC_Download_InProgress}</span>
                    </React.Fragment>
                )}
                {status == JMConstants.StatusCode.Finished && (
                    <React.Fragment>
                        <div
                            className="fia-radiobutton-bg-device ra-dc-iconstyle"
                            style={{ color: "#28CC74" }}
                        ></div>
                        <span>{RMResx.RM_JS_DC_Download_Finish}</span>
                    </React.Fragment>
                )}
                {status == JMConstants.StatusCode.Failed && (
                    <React.Fragment>
                        <span
                            className="fia-radiobutton-bg-device ra-dc-iconstyle"
                            style={{ color: "#D01B1B" }}
                        ></span>
                        <span>{RMResx.RM_JS_DC_Download_Failed}</span>
                    </React.Fragment>
                )}
            </div>
        );
    }

    render(Row, Cell) {
        let rowData = this.props.rowData;
        let jobStatus = this.renderJobStatus(rowData.JobStatus);
        return (
            <Row>
                <Cell>
                    <div
                        className="flex ra-flex-align-center"
                        data-tooltip
                        aria-label={rowData.FullPath}
                    >
                        {this.getFileAndSourceIcons(rowData)}
                        <span className="text-overflow">{rowData.Name}</span>
                    </div>
                </Cell>
                <Cell>
                    <div
                        className="text-overflow"
                        data-tooltip
                        aria-label={rowData.DownloadTime}
                    >
                        {rowData.DownloadTime}
                    </div>
                </Cell>
                <Cell>
                    <div
                        className="text-overflow"
                        data-tooltip
                        aria-label={rowData.JobId}
                    >
                        {rowData.JobId}
                    </div>
                </Cell>
                <Cell>
                    <div
                        className="text-overflow"
                        data-tooltip
                        aria-label={rowData.FileSize}
                    >
                        {rowData.FileSize}
                    </div>
                </Cell>
                <Cell>
                    <div
                        className="text-overflow"
                        data-tooltip
                        aria-label={rowData.DownloadType}
                    >
                        {rowData.DownloadType}
                    </div>
                </Cell>
                <Cell>
                    <div className="text-overflow" aria-label={jobStatus}>
                        {jobStatus}
                    </div>
                </Cell>
            </Row>
        );
    }
}