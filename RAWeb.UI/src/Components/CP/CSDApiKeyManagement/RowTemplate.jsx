import {apiKeyActions} from "./Management";

export class TableRow extends R.TableRow {
    constructor(props) {
        super(props);
        
    }

    onCheckChange = (checked) => {
        this.props.rowData.isChecked = checked;
        this.dispatch(apiKeyActions.Check);
    }

    editRow = () => {
        this.dispatch(apiKeyActions.Edit);
    }

    onKeyDown = (e) => {
        if(e.keyCode == "13"){
            this.editRow(apiKeyActions.Edit);
        }
    }

    onCopyClick = () => {
        this.dispatch(apiKeyActions.Copy);
    }

    render(Row, Cell) {
        let rowData = this.props.rowData;

        return <Row key={rowData.Id}>
            <Cell>
                <R.Checkbox
                    key={new Date().getTime()}
                    checked={rowData.isChecked || false}
                    onChange={this.onCheckChange}
                />
            </Cell>
            <Cell>
                <div>
                    <a tabIndex='0' className="text-overflow ra-main-cell-link" data-tooltip aria-label={rowData.Name} onClick={this.editRow} onKeyDown={this.onKeyDown}>
                        {rowData.Name}
                    </a>
                </div>
            </Cell>
            <Cell>
                <div className="text-overflow csd-apikey-value" tabIndex='0' data-tooltip aria-label={rowData.Value}>
                    {rowData.Value}
                    {rowData.showValue && <R.Button className="csd-apikey-copy text-theme" type="plain" tooltip={RMResx.RM_JS_Common_CopyToClipboard} icon="fia-copy" onClick={this.onCopyClick}/>}
                </div>
            </Cell>
            <Cell>
                <div className="text-overflow" tabIndex='0' aria-label={rowData.OperatorLoginName}>
                    {rowData.OperatorLoginName}
                </div>
            </Cell>
            <Cell>
                <div className="text-overflow" tabIndex='0' aria-label={rowData.displayExpired}>
                    {rowData.displayExpired}
                </div>
            </Cell>
            <Cell>
                <div className="text-overflow" tabIndex='0' aria-label={rowData.displayCreated}>
                    {rowData.displayCreated}
                </div>
            </Cell>
            <Cell>
                <div className="text-overflow" tabIndex='0' aria-label={rowData.displayModified}>
                    {rowData.displayModified}
                </div>
            </Cell>
        </Row>;
    }
}