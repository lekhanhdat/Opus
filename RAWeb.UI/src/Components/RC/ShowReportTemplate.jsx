export class DueDisposalReportTemplate extends R.TableRow {
    constructor(props) {
        super(props);
        this.state = {};
        this.relatedRecordsAction = {
            0: RMResx.RM_JS_RDM_RelatedRecordsAction_None,
            1: RMResx.RM_JS_RDM_RelatedRecordsAction_Both,
        };
    }

    relatedRecordClick(uniqueId) {
        window.location.href = `/Root/PRM/RecordsExplorer/?uniqueId=${uniqueId}`;
    }

    getRelatedRecordCol(data, isSharePoint) {
        let relatedRecordsList = [];
        if (data) {
            let relatedRecordsJson = RM.XmlToJson($.parseXML(data));
            let arrayList = relatedRecordsJson.ArrayOfReportRelatedRecords.ReportRelatedRecords;
            if ($.isArray(arrayList)) {
                for (let i = 0; i < arrayList.length; i++) {
                    relatedRecordsList.push({Name: arrayList[i].Name["#text"], Url: arrayList[i].Url["#text"]});
                }
            } else {
                if (arrayList) {
                    relatedRecordsList.push({Name: arrayList.Name["#text"], Url: arrayList.Url["#text"]});
                }
            }
        }
        if (relatedRecordsList.length > 0) {
            let relatedRecordsContent = <div>
                {
                    relatedRecordsList.map((item, key) => {
                        return <div key={key} className="text-overflow" data-tooltip aria-label={item.Url ? item.Url : item.Name}>
                            {item.Url && <a className="ra-link-a" href={item.Url} target="_blank" rel='noreferrer noopener'>{item.Name}</a>}
                            {!item.Url && <a className="ra-link-a" onClick={this.relatedRecordClick.bind(this, item.Name)} target="_blank" rel='noreferrer noopener'>{item.Name}</a>}
                        </div>;
                    })
                }
            </div>;
            return relatedRecordsContent;
        } else {
            return '';
        }
    }

    render(Row, Cell) {
        let rowData = this.props.rowData;
        let relatedRecordCol = this.getRelatedRecordCol(rowData.RelatedRecords, rowData.isSharePoint);
        let relatedRecordsAction = this.relatedRecordsAction[rowData.RelatedRecordsAction];
        let url = rowData.Url;
        if (rowData.isSharePoint) {
            url = <a className="ra-main-cell-link" href={rowData.Url} target="_blank" rel='noreferrer noopener'>{rowData.Url}</a>;
        }
        return (
            <Row>
                <Cell>
                    <div className="text-overflow" data-tooltip aria-label={rowData.ObjectLevel}>
                        {rowData.ObjectLevel}
                    </div>
                </Cell>
                <Cell>
                    <div className="text-overflow" data-tooltip aria-label={rowData.TitleOrName}>
                        {rowData.TitleOrName}
                    </div>
                </Cell>
                <Cell>
                    <div className="text-overflow" data-tooltip data-tooltip-wrap="force" aria-label={rowData.Url}>
                        {url}
                    </div>
                </Cell>
                {rowData.isSharePoint && 
                    <Cell>
                        <div className="text-overflow" data-tooltip aria-label={rowData.SiteCollectionTitle}>
                            {rowData.SiteCollectionTitle}
                        </div>
                    </Cell>
                }
                <Cell>
                    <div className="text-overflow" data-tooltip aria-label={rowData.BCSTermName}>
                        {rowData.BCSTermName}
                    </div>
                </Cell>
                <Cell>
                    <div className="text-overflow" data-tooltip aria-label={rowData.AppliedRuleName}>
                        {rowData.AppliedRuleName}
                    </div>
                </Cell>
                <Cell>
                    <div className="text-overflow" data-tooltip aria-label={rowData.DisposalClass}>
                        {rowData.DisposalClass}
                    </div>
                </Cell>
                <Cell>
                    <div className="text-overflow">
                        {relatedRecordCol}
                    </div>
                </Cell>
                <Cell>
                    <div className="text-overflow" data-tooltip aria-label={relatedRecordsAction}>
                        {relatedRecordsAction}
                    </div>
                </Cell>
                <Cell>
                    <div className="text-overflow" data-tooltip aria-label={rowData.DisposalAction}>
                        {rowData.DisposalAction}
                    </div>
                </Cell>
                <Cell>
                    <div className="text-overflow" data-tooltip aria-label={rowData.Status}>
                        {rowData.Status}
                    </div>
                </Cell>
                <Cell>
                    <div className="text-overflow" data-tooltip aria-label={rowData.ManualApproval}>
                        {rowData.ManualApproval}
                    </div>
                </Cell>
                <Cell>
                    <div className="text-overflow" data-tooltip aria-label={rowData.ExportType}>
                        {rowData.ExportType}
                    </div>
                </Cell>
                <Cell>
                    <div className="text-overflow" data-tooltip aria-label={rowData.CreatedBy}>
                        {rowData.CreatedBy}
                    </div>
                </Cell>
                <Cell>
                    <div className="text-overflow" data-tooltip aria-label={rowData.CreatedTimeStr}>
                        {rowData.CreatedTimeStr}
                    </div>
                </Cell>
                <Cell>
                    <div className="text-overflow" data-tooltip aria-label={rowData.LastModifiedBy}>
                        {rowData.LastModifiedBy}
                    </div>
                </Cell>
                <Cell>
                    <div className="text-overflow" data-tooltip aria-label={rowData.LastModifiedTimeStr}>
                        {rowData.LastModifiedTimeStr}
                    </div>
                </Cell>
                <Cell>
                    <div className="text-overflow" data-tooltip aria-label={rowData.Comment}>
                        {rowData.Comment}
                    </div>
                </Cell>
            </Row>
        );
    }
}

export class TermUsageReportTemplate extends R.TableRow {
    constructor(props) {
        super(props);
        this.state = {};
    }

    render(Row, Cell) {
        let rowData = this.props.rowData;
        let url = rowData.Url;
        if (rowData.isSharePoint) {
            url = <a className="ra-main-cell-link" href={rowData.Url}>{rowData.Url}</a>;
        }
        return (
            <Row>
                <Cell>
                    <div className="text-overflow" data-tooltip aria-label={rowData.ObjectLevel}>
                        {rowData.ObjectLevel}
                    </div>
                </Cell>
                <Cell>
                    <div className="text-overflow" data-tooltip aria-label={rowData.TitleOrName}>
                        {rowData.TitleOrName}
                    </div>
                </Cell>
                <Cell>
                    <div className="text-overflow" data-tooltip data-tooltip-wrap="force" aria-label={rowData.Url}>
                        {url}
                    </div>
                </Cell>
                <Cell>
                    <div className="text-overflow" data-tooltip aria-label={rowData.BCSTermName}>
                        {rowData.BCSTermName}
                    </div>
                </Cell>
                <Cell>
                    <div className="text-overflow" data-tooltip aria-label={rowData.TermStatus}>
                        {rowData.TermStatus}
                    </div>
                </Cell>
                <Cell>
                    <div className="text-overflow" data-tooltip aria-label={rowData.BCSTermFullPath}>
                        {rowData.BCSTermFullPath}
                    </div>
                </Cell>
                <Cell>
                    <div className="text-overflow" data-tooltip aria-label={rowData.CreatedBy}>
                        {rowData.CreatedBy}
                    </div>
                </Cell>
                <Cell>
                    <div className="text-overflow" data-tooltip aria-label={rowData.CreatedTimeStr}>
                        {rowData.CreatedTimeStr}
                    </div>
                </Cell>
                <Cell>
                    <div className="text-overflow" data-tooltip aria-label={rowData.LastModifiedBy}>
                        {rowData.LastModifiedBy}
                    </div>
                </Cell>
                <Cell>
                    <div className="text-overflow" data-tooltip aria-label={rowData.LastModifiedTimeStr}>
                        {rowData.LastModifiedTimeStr}
                    </div>
                </Cell>
            </Row>
        );
    }
}

export class CreationAndDestructionReportTemplate extends R.TableRow {
    constructor(props) {
        super(props);
        this.state = {};
    }

    render(Row, Cell) {
        let rowData = this.props.rowData;
        let url = rowData.Url;
        let actionType = "";
        if (rowData.Operation == 0) {
            actionType = RMResx.RM_JS_RC_TimeFrame_Create;
        }else if (rowData.Operation == 1) {
            actionType = RMResx.RM_JS_RC_TimeFrame_Destroyed;
        }

        if (rowData.isSharePoint) {
            url = <a className="ra-main-cell-link" href={rowData.Url}>{rowData.Url}</a>;
        }
        return (
            <Row>
                <Cell>
                    <div className="text-overflow" data-tooltip aria-label={rowData.OperationTime}>
                        {rowData.OperationTime}
                    </div>
                </Cell>
                <Cell>
                    <div className="text-overflow" data-tooltip aria-label={rowData.ObjectLevel}>
                        {rowData.ObjectLevel}
                    </div>
                </Cell>
                <Cell>
                    <div className="text-overflow ra-common-pre" data-tooltip aria-label={rowData.Title}>
                        {rowData.Title}
                    </div>
                </Cell>
                <Cell>
                    <div className="text-overflow" data-tooltip data-tooltip-wrap="force" aria-label={rowData.Url}>
                        {url}
                    </div>
                </Cell>
                <Cell>
                    <div className="text-overflow" data-tooltip aria-label={rowData.FileType}>
                        {rowData.FileType}
                    </div>
                </Cell>
                <Cell>
                    <div className="text-overflow" data-tooltip aria-label={rowData.CDCreatedTimeStr}>
                        {rowData.CDCreatedTimeStr}
                    </div>
                </Cell>
                <Cell>
                    <div className="text-overflow" data-tooltip aria-label={rowData.CDLastModifiedTimeStr}>
                        {rowData.CDLastModifiedTimeStr}
                    </div>
                </Cell>
                <Cell>
                    <div className="text-overflow" data-tooltip aria-label={actionType}>
                        {actionType}
                    </div>
                </Cell>
                <Cell>
                    <div className="text-overflow" data-tooltip aria-label={rowData.OperationBy}>
                        {rowData.OperationBy}
                    </div>
                </Cell>
                <Cell>
                    <div className="text-overflow" data-tooltip aria-label={rowData.RecordsId}>
                        {rowData.RecordsId}
                    </div>
                </Cell>
                <Cell>
                    <div className="text-overflow" data-tooltip aria-label={rowData.TermName}>
                        {rowData.TermName}
                    </div>
                </Cell>
                <Cell>
                    <div className="text-overflow" data-tooltip aria-label={rowData.RuleName}>
                        {rowData.RuleName}
                    </div>
                </Cell>
                <Cell>
                    <div className="text-overflow" data-tooltip aria-label={rowData.DisposalClass}>
                        {rowData.DisposalClass}
                    </div>
                </Cell>
                <Cell>
                    <div className="text-overflow ra-common-pre" data-tooltip aria-label={rowData.ApprovalStatus}>
                        {rowData.ApprovalStatus}
                    </div>
                </Cell>
                <Cell>
                    <div className="text-overflow" data-tooltip aria-label={rowData.ApprovedBy}>
                        {rowData.ApprovedBy}
                    </div>
                </Cell>
            </Row>
        );
    }
}

export class AvailiableSpaceReportTemplate extends R.TableRow {
    constructor(props) {
        super(props);
        this.state = {};
    }

    getAvailableSpaceColumn(availableSpace) {
        let availableSpaceColumn = availableSpace;
        if (availableSpace < 0) {
            availableSpaceColumn = <div>
                <div className="info-error-word">{availableSpace}</div>
                <div className="info-error-img"></div>
            </div>;
        }
        return availableSpaceColumn;
    }

    render(Row, Cell) {
        let rowData = this.props.rowData;
        let availableSpaceColumn = this.getAvailableSpaceColumn(rowData.AvailableSpace);
        return (
            <Row>
                <Cell>
                    <div className="text-overflow" data-tooltip aria-label={rowData.Location}>
                        {rowData.Location}
                    </div>
                </Cell>
                <Cell>
                    <div className="text-overflow" data-tooltip aria-label={rowData.AvailableSpace}>
                        {availableSpaceColumn}
                    </div>
                </Cell>
                <Cell>
                    <div className="text-overflow" data-tooltip aria-label={rowData.LocationSize}>
                        {rowData.LocationSize}
                    </div>
                </Cell>
            </Row>
        );
    }
}
export class ActionAuditReportTemplate extends R.TableRow {
    constructor(props) {
        super(props);
        this.state = {};
    }

    render(Row, Cell) {
        let rowData = this.props.rowData;
        return (
            <Row>
                <Cell>
                    <div className="text-overflow" data-tooltip aria-label={rowData.OccurredTimeStr}>
                        {rowData.OccurredTimeStr}
                    </div>
                </Cell>
                <Cell>
                    <div className="text-overflow" data-tooltip aria-label={rowData.User}>
                        {rowData.User}
                    </div>
                </Cell>
                <Cell>
                    <div className="text-overflow" data-tooltip data-tooltip-wrap="force" aria-label={rowData.Url}>
                        {rowData.Url}
                    </div>
                </Cell>
                <Cell>
                    <div className="text-overflow" data-tooltip aria-label={rowData.ObjectLevelI18NName}>
                        {rowData.ObjectLevelI18NName}
                    </div>
                </Cell>
                <Cell>
                    <div className="text-overflow" data-tooltip aria-label={rowData.EventCategoryType}>
                        {rowData.EventCategoryType}
                    </div>
                </Cell>
                <Cell>
                    <div className="text-overflow" data-tooltip aria-label={rowData.EventTypeName}>
                        {rowData.EventTypeName}
                    </div>
                </Cell>
            </Row>
        );
    }
}

export class RestoreReportTemplate extends R.TableRow {
    constructor(props) {
        super(props);
        this.state = {};
    }

    render(Row, Cell) {
        let rowData = this.props.rowData;
        let url = rowData.Url;

        return (
            <Row>
                <Cell>
                    <div className="text-overflow" data-tooltip aria-label={rowData.TitleOrName}>
                        {rowData.TitleOrName}
                    </div>
                </Cell>
                <Cell>
                    <div className="text-overflow" data-tooltip data-tooltip-wrap="force" aria-label={rowData.Url}>
                        {url}
                    </div>
                </Cell>
                <Cell>
                    <div className="text-overflow" data-tooltip aria-label={rowData.SizeString}>
                        {rowData.SizeString}
                    </div>
                </Cell>
                <Cell>
                    <div className="text-overflow" data-tooltip aria-label={rowData.RestoreBy}>
                        {rowData.RestoreBy}
                    </div>
                </Cell>
                <Cell>
                    <div className="text-overflow" data-tooltip aria-label={rowData.JobId}>
                        {rowData.JobId}
                    </div>
                </Cell>
                <Cell>
                    <div className="text-overflow" data-tooltip aria-label={rowData.StartTimeString}>
                        {rowData.StartTimeString}
                    </div>
                </Cell>
                <Cell>
                    <div className="text-overflow" data-tooltip aria-label={rowData.EndTimeString}>
                        {rowData.EndTimeString}
                    </div>
                </Cell>
                <Cell>
                    <div className="text-overflow" data-tooltip aria-label={rowData.RestoreTo}>
                        {rowData.RestoreTo}
                    </div>
                </Cell>
            </Row>
        );
    }
}

export class ArchivedSitesTemplate extends R.TableRow {
    render(Row, Cell) {
        let rowData = this.props.rowData;

        return (
            <Row>
                <Cell>
                    <div className="text-overflow" data-tooltip aria-label={rowData.Type}>
                        {rowData.Type}
                    </div>
                </Cell>
                <Cell>
                    <div className="text-overflow" data-tooltip data-tooltip-wrap="force" aria-label={rowData.SourceUrl}>
                        {rowData.SourceUrl}
                    </div>
                </Cell>
                <Cell>
                    <div className="text-overflow" data-tooltip aria-label={rowData.ArchivedDataSize}>
                        {rowData.ArchivedDataSize}
                    </div>
                </Cell>
                <Cell>
                    <div className="text-overflow" data-tooltip aria-label={rowData.CreatedTime}>
                        {rowData.CreatedTime}
                    </div>
                </Cell>
                <Cell>
                    <div className="text-overflow" data-tooltip aria-label={rowData.LastModifiedTime}>
                        {rowData.LastModifiedTime}
                    </div>
                </Cell>
                <Cell>
                    <div className="text-overflow" data-tooltip aria-label={rowData.ArchivedTime}>
                        {rowData.ArchivedTime}
                    </div>
                </Cell>
            </Row>
        );
    }
}