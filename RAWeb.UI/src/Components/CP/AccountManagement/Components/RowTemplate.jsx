import { DefaultSecurityGroup, SecurityGroupMenuItems } from "../../../../Constants/Constants";
import { LicenseHelper } from "../../../../Utilities/CommonUtil";
import { getPermissionReportList } from "../Constants";
export default class GroupTableRow extends R.TableRow {
    constructor(props) {
        super(props);
        this.state = {};
    }

    onSelectChange = (item) => {
        this.dispatch("cellOperate", item);
    }

    onCellClick = (item) => {
        this.dispatch("cellClick", item);
    }

    onKeyDown = (e) =>{
        if(e.keyCode == "13")
        {
            e.target.click();
        }
    }

    wrapperI18N(str) {
        return RMResx[str] ? RMResx[str] : str;
    }

    getMenuItems(group)
    {
        if(group.Id == DefaultSecurityGroup.BuiltInAdmin)
        {
            return SecurityGroupMenuItems.slice(2); //only view
        } 
        
        if(group.IsBuiltInGroup) //end user group/ review group
        {
            return SecurityGroupMenuItems.slice(0, 1); //only edit
        }

        return SecurityGroupMenuItems.slice(0, 2); //edit/delete;
    }

    getTableActionBtns(groupItems) {
        return <React.Fragment>
            {groupItems.map((item, key) => (
                <R.Button
                    key={key}
                    onClick={this.onSelectChange.bind(this, item)}
                    text={item.displayName} />
            ))}
        </React.Fragment>;
    }

    renderRowCell(Row, Cell) {
        let rowData = this.props.rowData;
        let [groupName, groupDesc, groupItems] = [this.wrapperI18N(rowData.Name), this.wrapperI18N(rowData.Description), this.getMenuItems(rowData)];
        let tableActionBtns = this.getTableActionBtns(groupItems);
        const permissionReportValue = rowData.ReportingPermission;
        const checkedPermissionReportList = getPermissionReportList()
            .map((item) => ({
                ...item,
                checked: (permissionReportValue & item.value) !== 0,
            }))
            .filter(item => item.checked)
            .map((item) => item.text);
        return <Row action={tableActionBtns}>
            <Cell>
                <div className="text-overflow">
                    <a className="ra-main-cell-link" tabIndex='0' onClick={this.onCellClick.bind(this, rowData)}
                        onKeyDown={this.onKeyDown} data-tooltip aria-label={groupName}>
                        {groupName}
                    </a>
                </div>
            </Cell>
            <Cell>
                <div className="text-overflow" data-tooltip aria-label={rowData.SourceTypesName} >
                    {rowData.SourceTypesName}
                </div>
            </Cell>
            <Cell>
                <div className="text-overflow" data-tooltip aria-label={rowData.TermScope}>
                    {rowData.TermScope}
                </div>
            </Cell>
            <Cell>
                <div className="text-overflow" data-tooltip aria-label={rowData.RuleScope}>
                    {rowData.RuleScope}
                </div>
            </Cell>
            <Cell>
                {/* Update later */}
                <div className="text-overflow" data-tooltip aria-label={checkedPermissionReportList.join('; ')}>
                    {checkedPermissionReportList.join('; ')}
                </div>
            </Cell>
            {!LicenseHelper.HasOpusSOLicenseOnly() && (
                <>
                    <Cell>
                        <div className="text-overflow" data-tooltip aria-label={rowData.IsEnableManageHold ? RMResx.RM_CP_AM_ManageHolds_Option01 : ''}>
                            {rowData.IsEnableManageHold ? RMResx.RM_CP_AM_ManageHolds_Option01 : ''}
                        </div>
                    </Cell>
                    <Cell>
                        <div className="text-overflow" data-tooltip aria-label={rowData.IsEnableApprovalSetting ? RMResx.RM_CP_AM_ManageApprovalSettings_Option01 : ''}>
                            {rowData.IsEnableApprovalSetting ? RMResx.RM_CP_AM_ManageApprovalSettings_Option01 : ''}
                        </div>
                    </Cell>
                </>
            )}
        </Row>;
    }

    render(Row, Cell) {
        return this.renderRowCell(Row, Cell);
    }
}