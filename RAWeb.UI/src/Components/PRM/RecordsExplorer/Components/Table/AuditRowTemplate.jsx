import { PhysicalRecordActionTypeI18Ns } from "../../../Constants";

export default class AuditRowTemplate extends R.TableRow {

    constructor(props) {
        super(props);
        this.state = {};
    }

    getOldOrNewValue(attr, modifyContent) {

        return <div>
            {
                modifyContent && modifyContent.map((item, index) => {
                    let value = item[attr];
                    if (value == "True") {
                        value = RMResx.RM_JS_Common_Yes;
                    } else if (value == "False") {
                        value = RMResx.RM_JS_Common_No;
                    }
                    if (value && value.indexOf('<br>') > 0) {
                        value = value.replace(/<br>/gi, "\n");
                    }

                    return value && <div className="reco-audit-cell-item" key={index}>
                        {
                            item.TargetSetting &&
                            <div className='reco-audit-cell-title' data-tooltip="ifneed" aria-label={this.resetTargetSetting(item.TargetSetting)}>{this.resetTargetSetting(item.TargetSetting)}</div>
                        }
                        <div className='reco-audit-cell-value' data-tooltip="ifneed" aria-label={value}>{value}</div>
                    </div>;

                })
            }
        </div>;
    }

    resetTargetSetting(str)
    {
        if(str.indexOf(":") > -1)
        {
            //去掉申请词条内容中末尾带的冒号，统一在前台加
            var req =/:$/gi;
            str = str.replace(req, "");
        }
        str += ":";
        return str;
    }

    render(Row, Cell) {

        const rowData = this.props.rowData;
        let newValue = this.getOldOrNewValue('NewValue', rowData.ModifyContent);
        let oldValue = this.getOldOrNewValue('OldValue', rowData.ModifyContent);

        return <Row>
            <Cell>
                <div className="text-overflow" data-tooltip aria-label={rowData.ActionTimeStr}>
                    {rowData.ActionTimeStr}
                </div>
            </Cell>
            <Cell>
                <div className="text-overflow" data-tooltip aria-label={rowData.ActionUser}>
                    {rowData.ActionUser}
                </div>
            </Cell>
            <Cell>
                <div className="text-overflow" data-tooltip aria-label={PhysicalRecordActionTypeI18Ns.get(rowData.ActionType)}>
                    {PhysicalRecordActionTypeI18Ns.get(rowData.ActionType)}
                </div>
            </Cell>
            <Cell>
                <div className="text-overflow">
                    {newValue}
                </div>
            </Cell>
            <Cell>
                <div className="text-overflow">
                    {oldValue}
                </div>
            </Cell>
        </Row>;
    }
}