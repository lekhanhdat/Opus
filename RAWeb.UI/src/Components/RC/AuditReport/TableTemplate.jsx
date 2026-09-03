export class Template extends R.TableRow {
    constructor(props) {
        super(props);
        this.state = {};
    }

    getOldOrNewValue(attr, modifyContent, actionType) {
        //当save search view的时候不显示save view的Criteria。
        if(actionType == 90010 || actionType == 90011){
            modifyContent = modifyContent.filter((item)=>{
                return item.TargetSetting != RMResx.RM_BCM_Audit_Action_Search_Criteria_Content; 
            });
        }
        return <div>
            {
                modifyContent.map((item, index) => {
                    let value = item[attr];
                    if (value == "True") {
                        value = RMResx.RM_JS_Common_Yes;
                    } else if (value == "False") {
                        value = RMResx.RM_JS_Common_No;
                    }
                    if (value && value.indexOf('<br>') > 0) {
                        value = value.replace(/<br>/gi, "\n");
                    } else if (value && value.indexOf('<br>') === 0) {
                        value = value.slice(4).replace(/<br>/gi, "\n");
                    }
                    //term with out rule  for REC-828
                    if (actionType == 2004) {
                        if (!value) {
                            value = RMResx.RM_JS_Rule_ObjectLevel_None;
                        }
                    }
                    if (actionType == 5008) {
                        item.TargetSetting = "";
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
        let rowData = this.props.rowData;
        let rowDataItem = this.props.rowData.Item;
        let objectTitle = rowDataItem.Object && rowDataItem.Object.replace(/\;/gi, ";\n") || "";
        let newValue = this.getOldOrNewValue('NewValue', rowDataItem.ModifyContent, rowDataItem.Action);
        let oldValue = this.getOldOrNewValue('OldValue', rowDataItem.ModifyContent, rowDataItem.Action);
        return (
            <Row>
                <Cell>
                    <div className="text-overflow" data-tooltip aria-label={rowData.DateStr}>
                        {rowData.DateStr}
                    </div>
                </Cell>
                <Cell>
                    <div className="text-overflow" data-tooltip aria-label={rowDataItem.UserName}>
                        {rowDataItem.UserName}
                    </div>
                </Cell>
                <Cell>
                    <div className="text-overflow" data-tooltip aria-label={rowData.CategoryStr}>
                        {rowData.CategoryStr}
                    </div>
                </Cell>
                {/* <Cell>
                    <div className="text-overflow" data-tooltip aria-label={rowData.CategoryStr}>
                        {rowData.CategoryStr}
                    </div>
                </Cell> */}
                <Cell>
                    <div className="text-overflow" data-tooltip aria-label={rowData.ActionStr}>
                        {rowData.ActionStr}
                    </div>
                </Cell>
                <Cell>
                    <div className="text-overflow" data-tooltip aria-label={objectTitle}>
                        {objectTitle}
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
                <Cell>
                    <div className="text-overflow" data-tooltip aria-label={rowData.StatusStr}>
                        {rowData.StatusStr}
                    </div>
                </Cell>
                <Cell>
                    <div className="text-overflow" data-tooltip aria-label={rowDataItem.ClientIP}>
                        {rowDataItem.ClientIP}
                    </div>
                </Cell>
            </Row>
        );
    }
}