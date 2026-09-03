import { PhysicalDefaultColumnIDs, PhysicalObjectColumnType, PhysicalObjectStatus } from "../../../Constants/Constants";
import PeoplePicker from "../../Common/PeoplePicker";
import { PhyObjFormType } from "../RecordsExplorer/RecordsExplorer";

export default class PhyObjBulkUpdateForm extends R.Component {
    idAttr = true;
    constructor(props) {
        super(props);
        this.state = {
            bulkUpdateData: this.props.data,
            categoryData: [],
            templateLoaded: false,
            isSaving: false,
            showCheckedError: false,
            showMsgBar: false,
        }
        this.defaultDateFormat = RM.TimeUtil.getGlobalAuiFormat();
    }

    componentReceive(type, ...args) {
        switch (type) {
            case "onSave":
                this.parentIdList = args[1];
                this.saveBulkUpdateFormData(args[0]);
                break;
            case "getMetaInfoData":
                this.startJobBulkUpdateFormData(args[0]);
                break;
        }
    }

    componentInit() {
        this.initData(this.props.data);
    }

    initData(args) {
        switch (args.formType) {
            case PhyObjFormType.EditPhyObj:
                this.initEditBulkUpdateFormData();
                break;
            default:
                return;
        }
    }

    initEditBulkUpdateFormData() {
        $$.loading(true);
        let url = `/api/PhysicalRecordApi/LoadTemplateDatasForBulkUpdate?id=${this.state.bulkUpdateData.TemplateId}`;
        let option = {
            url: url,
            method: "Get",
        };
        fetchUtility(option, response => {
            this.handleError(response);
        }).then(res => {
            $$.loading(false);
            let template = JSON.parse(res);
            this.setState({
                categoryData: template.categories,
                showMsgBar: true,
                templateLoaded: true
            });
            template.name = RMResx[template.name] ? RMResx[template.name] : template.name;
            this.props.setPanelTitle(template.name);
        });
    }

    saveBulkUpdateFormData(callback) {
        let metaInfo = {};
        if (this.state.categoryData.length != 0) {
            for (const category of this.state.categoryData) {
                for (const column of category.columns) {
                    if (column.isChecked) {
                        metaInfo[column.uniqueId] = column.columnValue;
                        if (metaInfo[column.uniqueId] == undefined) {
                            metaInfo[column.uniqueId] = null
                        }
                        if (column.required && !column.columnValue && column.isChecked) {
                            //validation: has empty required column
                            this.setState({
                                isSaving: true,
                                categoryData: JSON.parse(JSON.stringify(this.state.categoryData))
                            });
                            callback(false, this.state.bulkUpdateData);
                            return;
                        }
                    }
                }
            }
            if (JSON.stringify(metaInfo) != "{}") {
                this.saveBulkUpdateColumnsData(metaInfo, callback);
            } else {
                this.setState({ showCheckedError: true });
            }
        }
    }

    startJobBulkUpdateFormData(callback) {
        let metaInfo = {};
        if (this.state.categoryData.length != 0) {
            for (const category of this.state.categoryData) {
                for (const column of category.columns) {
                    if (column.isChecked) {
                        metaInfo[column.uniqueId] = column.columnValue;
                        if (metaInfo[column.uniqueId] == undefined) {
                            metaInfo[column.uniqueId] = null
                        }
                        if (column.required && !column.columnValue && column.isChecked) {
                            //validation: has empty required column
                            this.setState({
                                isSaving: true,
                                categoryData: JSON.parse(JSON.stringify(this.state.categoryData))
                            });
                            return;
                        }
                    }
                }
            }
            if (JSON.stringify(metaInfo) != "{}") {
                callback(true, metaInfo);
            } else {
                this.setState({ showCheckedError: true });
            }
        }
    }

    saveBulkUpdateColumnsData(metaInfo, callback) {
        let formData = this.state.bulkUpdateData;
        formData.Name = metaInfo[PhysicalDefaultColumnIDs.NameOrTitle];
        formData.MetaInfo = metaInfo;

        let postData = null;
        let url = null;
        let errorMsg = '';
        let editItemErrorMsg = RMResx.RM_PRM_PRE_Msg_EditItemError;
        switch (formData.formType) {
            case PhyObjFormType.EditPhyObj:
                postData = formData;
                url = `/api/PhysicalRecordApi/BulkEditPhysicalObject`;
                errorMsg = editItemErrorMsg;
                break;
            default:
                return;
        }

        $.ajax({
            type: "POST",
            url: url,
            contentType: 'application/json;charset=utf-8',
            data: JSON.stringify(postData),
            success: (result) => {
                if (result.success || result.HasError === false) {
                    callback(true, formData);
                } else {
                    let tipMsg = result.message || errorMsg;
                    this.openErrorMessageBox(tipMsg);
                    callback(false, formData);
                }
            },
            error: (msg) => {
                if (msg.status == 403) {
                    this.handleError(msg);
                }
            },
            dataType: "json"
        });
    }

    openErrorMessageBox(msg) {
        let args = {
            classify: "error",
            width: "550px",
            hideActions: false,
            title: RMResx.RM_JS_Common_Confirmation,
            content: msg,
            buttons: [
                {
                    text: RMResx.RM_JS_Common_Close, primary: true, classify: "theme", onClick: () => {
                        $$.messagedialog(false);
                    }
                },
            ]
        };
        $$.messagedialog(true, args);
    }

    handleError(response) {
        $$.loading(false);
        if (response.status == 403) {
            $$.messagedialog(true, {
                classify: "warn",
                width: "550px",
                hideActions: false,
                title: RMResx.RM_JS_Common_Confirmation,
                content: RMResx.RM_JS_Common_NoPermissionLicense,
                buttons: [
                    {
                        text: RMResx.RM_JS_Common_OK,
                        classify: "theme",
                        onClick: () => { $$.messagedialog(false); }
                    }
                ]
            });
        }
    }

    onCheckboxChange(column, args) {
        column.isChecked = args;
        this.setState({
            categoryData: RM.deepcopy(this.state.categoryData)
        });
    }

    onSingleTextChange(column, value) {
        column.columnValue = $.trim(value);
        this.setState({
            categoryData: this.state.categoryData
        });
    }

    onMultipleTextChange(column, value) {
        column.columnValue = value;
        this.setState({
            categoryData: this.state.categoryData
        });
    }

    onNumberChange(column, value) {
        column.columnValue = value;
        this.setState({
            categoryData: this.state.categoryData
        });
    }

    onDateTimeChange(column, args) {
        let timezoneInfo = RM.TimeUtil.getGlobalTimezoneInfo();
        if (args.newValue) {
            let dateStr = RM.TimeUtil.getCommonDateStr(args.newValue);
            let zoneId = timezoneInfo.id;
            let autoAdjustClock = timezoneInfo.autoAdjustClock;
            column.columnValue = JSON.stringify({
                Date: dateStr,
                TimeZoneId: zoneId,
                IsSetDayLight: autoAdjustClock
            });
        } else {
            column.columnValue = null;
        }

        this.setState({
            categoryData: RM.deepcopy(this.state.categoryData)
        });
    }

    onSingleChoiceChange(column, args) {
        let columnValueObj = {
            Name: args.newValue.value,
            Value: args.newValue.key
        };
        column.columnValue = JSON.stringify(columnValueObj);

        if (column.uniqueId == PhysicalDefaultColumnIDs.Status) {
            for (let index = 0; index < this.state.categoryData.length; index++) {
                const category = this.state.categoryData[index];
                for (let index = 0; index < category.columns.length; index++) {
                    const loanedByColumn = category.columns[index];
                    if (loanedByColumn.uniqueId == PhysicalDefaultColumnIDs.LoanedBy) {
                        loanedByColumn.disabled = args.newValue.key == PhysicalObjectStatus.Destroyed || args.newValue.key == PhysicalObjectStatus.Missing;
                        if (loanedByColumn.disabled) {
                            loanedByColumn.columnValue = "";
                        }
                        break;
                    }
                }
                break;
            }
        }
        this.setState({
            categoryData: RM.deepcopy(this.state.categoryData)
        });
    }

    onPeopleSelectionChanged(column, users) {
        let newVal = null;
        if (users && users.length > 0) {
            let selUsers = users.filter(user => user.Checked);
            newVal = JSON.stringify(selUsers);
        }
        column.columnValue = newVal;
        this.setState({
            categoryData: this.state.categoryData
        });
    }

    onMultipleChoiceChange(column, args) {
        let columnValues = [];
        for (let arg of args.newValue) {
            let columnValueObj = {};
            columnValueObj.Name = arg.value;
            columnValueObj.Value = arg.key;
            columnValues.push(columnValueObj);
        }
        column.columnValue = JSON.stringify(columnValues);
        this.setState({
            categoryData: this.state.categoryData
        });
    }

    getValidContent(column) {
        if (column.required && !column.columnValue && column.isChecked) {
            switch (column.typeId) {
                case PhysicalObjectColumnType.SingleText:
                case PhysicalObjectColumnType.MutipleText:
                    return RMResx.RM_PRM_PRE_ColumnValid_RequireText;
                case PhysicalObjectColumnType.Number:
                    if (column.uniqueId == PhysicalDefaultColumnIDs.Capability) {
                        return RMResx.RM_PRM_PRE_ColumnValid_RequireNumber;
                    } else {
                        return RMResx.RM_PRM_PRE_ColumnValid_RequireText;
                    }
                case PhysicalObjectColumnType.DateTime:
                    return RMResx.RM_PRM_PRE_ColumnValid_RequireDateTime;
                case PhysicalObjectColumnType.SingleChoice:
                    return RMResx.RM_PRM_PRE_ColumnValid_RequireSingleChoice;
                case PhysicalObjectColumnType.PeopleOrGroup:
                    return RMResx.RM_JS_CP_AM_AddUser_Nomatch;
                case PhysicalObjectColumnType.MultipleChoice:
                    return RMResx.RM_PRM_PRE_ColumnValid_RequireMultipleChoice;
                case PhysicalObjectColumnType.Taxonomy:
                    return RMResx.RM_PRM_PRE_ColumnValid_RequireTreeNode;
            }
        }
    }

    renderColumn(column) {
        switch (column.typeId) {
            case PhysicalObjectColumnType.SingleText:
                return this.renderSingleTextColumn(column);
            case PhysicalObjectColumnType.MutipleText:
                return this.renderMutipleTextColumn(column);
            case PhysicalObjectColumnType.Number:
                return this.renderNumberColumn(column);
            case PhysicalObjectColumnType.DateTime:
                return this.renderDateTimeColumn(column);
            case PhysicalObjectColumnType.SingleChoice:
                return this.renderSingleChoiceColumn(column);
            case PhysicalObjectColumnType.PeopleOrGroup:
                return this.renderPeopleOrGroupColumn(column);
            case PhysicalObjectColumnType.MultipleChoice:
                return this.renderMultipleChoiceColumn(column);
        }
    }

    renderSingleTextColumn(column) {
        let columnName = RMResx[column.columnName] ? RMResx[column.columnName] : column.columnName;
        return <div>
            <R.Input
                type="text"
                value=""
                width={300}
                disabled={!column.isChecked}
                onChange={this.onSingleTextChange.bind(this, column)}
                aria={{ ariaLabel: columnName }}
            />
        </div>;
    }

    renderMutipleTextColumn(column) {
        let columnName = RMResx[column.columnName] ? RMResx[column.columnName] : column.columnName;
        return <div>
            <R.Input
                type="textarea"
                value=""
                width={300}
                disabled={!column.isChecked}
                onChange={this.onMultipleTextChange.bind(this, column)}
                aria={{ ariaLabel: columnName }}
            />
        </div>;
    }

    renderNumberColumn(column) {
        let columnName = RMResx[column.columnName] ? RMResx[column.columnName] : column.columnName;
        let props = {};
        if (column.uniqueId == PhysicalDefaultColumnIDs.Capability) {
            props.min = 0.01;
        }
        return <div>
            <R.Input
                {...props}
                type="number"
                hasControl
                value=""
                width={300}
                float={2}
                fixFloat={false}
                disabled={!column.isChecked}
                onChange={this.onNumberChange.bind(this, column)}
                aria={{ ariaLabel: columnName }}
            />
        </div>;
    }

    renderDateTimeColumn(column) {
        let selDate = null;
        let isShowClearBtn = column.uniqueId == PhysicalDefaultColumnIDs.DataClosed;
        if (column.columnValue) {
            let dt = JSON.parse(column.columnValue);
            selDate = new Date(dt.Date);
        }
        return <div>
            <R.Datepicker
                selectedDate={selDate}
                data-part="vtWidget"
                width={300}
                disabled={!column.isChecked}
                dateTimeFormat={this.defaultDateFormat}
                hasTimePicker={true}
                onChange={this.onDateTimeChange.bind(this, column)}
                triggerBySource={true}
                todayClick={this.todayClick}
            />
            {
                isShowClearBtn && selDate && <a className="ra-link-a margin-s" onClick={this.onClearCloseDate}>{RMResx.RM_Common_Clear}</a>
            }
        </div>;
    }

    renderSingleChoiceColumn(column) {
        let optionsObj = JSON.parse(column.optionsJSON);
        let options = [];
        let selId = null;
        if (column.columnValue) {
            selId = JSON.parse(column.columnValue).Value;
        }

        if (optionsObj) {
            for (const oId in optionsObj) {
                if (Object.hasOwnProperty.call(optionsObj, oId)) {
                    let opValue = optionsObj[oId];
                    options.push({
                        key: oId,
                        value: opValue,
                        checked: oId === selId,
                        tooltip: opValue,
                    });
                }
            }
        }
        return <div>
            <R.Combobox
                checkedField="checked"
                searchable={false}
                textField="value"
                valueField="key"
                tooltipField="tooltip"
                width={300}
                disabled={!column.isChecked}
                items={options}
                onChange={this.onSingleChoiceChange.bind(this, column)}
            />
        </div>;
    }

    renderPeopleOrGroupColumn(column) {
        let isLoanByColumn = column.uniqueId == PhysicalDefaultColumnIDs.LoanedBy;
        let users = [];
        if (column.columnValue) {
            users = JSON.parse(column.columnValue);
        }
        return <div>
            <PeoplePicker
                items={users}
                singleMode={isLoanByColumn}
                disabled={!column.isChecked}
                selectionChanged={this.onPeopleSelectionChanged.bind(this, column)}
            />
        </div>;
    }

    renderMultipleChoiceColumn(column) {
        let optionsObj = JSON.parse(column.optionsJSON);
        let options = [];
        let selectedValues = [];
        if (column.columnValue) {
            for (let selectedOption of JSON.parse(column.columnValue)) {
                selectedValues.push(selectedOption.Value);
            }
        }
        for (const oId in optionsObj) {
            if (optionsObj.hasOwnProperty(oId)) {
                let opValue = optionsObj[oId];
                options.push({
                    key: oId,
                    value: opValue,
                    checked: selectedValues.indexOf(oId) != -1,
                    tooltip: opValue
                });
            }
        }

        return <div>
            <R.Multicombobox
                items={options}
                width={300}
                textField='value'
                valueField='key'
                tooltipField="tooltip"
                disabled={!column.isChecked}
                onChange={this.onMultipleChoiceChange.bind(this, column)}
            />
        </div>;
    }

    renderValidationMsg(column) {
        return <div>
            <$g.ValidationMsg show={this.state.isSaving}>
                {this.getValidContent(column)}
            </$g.ValidationMsg>
        </div>;
    }

    render() {
        return <div id={this.props.id} className="phyobj-form">
            <div>
                {this.state.showMsgBar && this.props.showMsgBar && <div style={{ marginBottom: "8px" }}>
                    <R.Messagebar
                        classify="info"
                        message={RMResx.RM_PRM_PRE_Msg_MsgBar}
                        status={{ show: true }}
                        hasClose={true}
                    />
                </div>}
                {this.state.templateLoaded && this.state.categoryData.length == 0 && <R.Messagebar
                    classify="error"
                    message={RMResx.RM_PRM_PRE_Msg_BulkUpdateError}
                    status={{ show: true }}
                    hasClose={false}
                />}
                {this.state.showCheckedError && <div className="ra-validation-msg" style={{ marginBottom: "8px" }}>{RMResx.RM_PRM_PRE_Msg_CheckedError}</div>}
                {
                    this.state.categoryData.map((item, categoryIndex) => {
                        let categoryName = RMResx[item.name] ? RMResx[item.name] : item.name;
                        return (
                            <div key={categoryIndex} className="phyobj-category">
                                <div className="ra-section-head">{categoryName}</div>
                                {item.columns.map((column, index) => {
                                    let columnName = RMResx[column.columnName] ? RMResx[column.columnName] : column.columnName;
                                    return (
                                        <$g.FormRow
                                            key={index}
                                        >
                                            { column.columnName !== "RM_PRM_PRE_Column_Barcode" && 
                                                <div className="ra-phyobj-column">
                                                    <div className={"ra-form-checkbox" + (column.required ? " require" : "")}>
                                                        <R.Checkbox
                                                            text={columnName}
                                                            checked={column.isChecked}
                                                            onChange={this.onCheckboxChange.bind(this, column)}
                                                        />
                                                    </div>
                                                    {this.renderColumn(column)}
                                                    {this.renderValidationMsg(column)}
                                                </div>
                                            }
                                        </$g.FormRow>
                                    );
                                })}
                            </div>
                        );
                    })
                }
            </div>
        </div>;
    }
}