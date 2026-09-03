import * as Constants from "../Constants";
import { EmptyGUID, PhysicalDefaultArray,PhysicalDefaultColumnIDs} from '../../../Constants/Constants';
import StringUtil from '../../../Utilities/StringUtil';
import '../../../Less/PRM/TemplateRightPanel/TemplateEditPanel.less';

class ColumnForm extends R.Component {
    idAttr = true;
    constructor(props) {
        super(props);
        this.bind(['checkSubTempalteSameColumn']);
        this.state = {
            showTip: false,
            showMessageTip: this.showMessageTip,
            isSaving: false,
            columnTypes: Constants.columnTypes.map((li, idx) => {
                return Object.assign(li, { checked: idx == 0 });
            }),
            columnName: '',
            required: false,
            allowSort: false,
            showSortOption: false,
            disablePush: false,
            pushToChild: false,
            //pushCategoryId: '',
            //pushFolderCategoryId: '',
            pushFoldTemplateCategoriesId: [],
            pushRecordTemplateCategoriesId: [],
            pushRecordColumnSameName: {},
            pushFolderColumnSameName: { },
            childInheritsValue: false,
            allowModifyValue: false,
            childTemplate: [],

            //categoryItems: [],
            //childFoldercategoryItems: [],
            options: [],
            panelType: null,
            
        };
        this.typeId = 0;
        this.currentOptId = 1;
        this.hasChanged = false;
        this.initData();
    }
    initData() {
        let tempColumnTypes = [];
        for (let i = 0; i < this.state.columnTypes.length; i++) {
            let item = JSON.parse(JSON.stringify(this.state.columnTypes[i]));
            if (item.id == this.props.item.typeId) {
                item.checked = true;
            } else {
                item.checked = false;
            }
            tempColumnTypes.push(item);
        }
        this.typeId = this.props.item.typeId || 0;
        let optArray = [];
        if (this.props.item.typeId == Constants.ColumnTypesEnum.SingleChoice || this.props.item.typeId == Constants.ColumnTypesEnum.MultipleChoice) {
            let optionsObject = JSON.parse(this.props.item.optionsJSON);
            let index = 0;
            for (const key in optionsObject) {
                if (Object.prototype.hasOwnProperty.call(optionsObject, key)) {
                    const element = optionsObject[key];
                    optArray.push({ index: index++, id: key, value: this.wrapperI18N(element) });
                }
            }
        }
        if (this.props.panelType == Constants.panelType.Box) {
            if (this.props.item.pushFoldTemplateCategoriesId != null && this.props.item.pushFoldTemplateCategoriesId.length > 0 && this.props.item.childTemplate != null) {
                for (let i = 0; i < this.props.item.pushFoldTemplateCategoriesId.length; i++) {
                    let foldCatoriesId = this.props.item.pushFoldTemplateCategoriesId[i];
                    let foldtemplate = this.props.item.childTemplate.find(f => f.uniqueId == foldCatoriesId.tempalteId);
                    if (foldtemplate != null && foldtemplate.currentCategories != null) {
                        for (let j = 0; j < foldtemplate.currentCategories.length; j++) {
                            if (foldtemplate.currentCategories[j].id == foldCatoriesId.categoryId) {
                                foldtemplate.currentCategories[j].checked = true;
                            }
                            if (foldtemplate.childrenCategories != null && this.props.item.pushRecordTemplateCategoriesId) {
                                for (let k = 0; k < this.props.item.pushRecordTemplateCategoriesId.length; k++) {
                                    let reordCatoriesId = this.props.item.pushRecordTemplateCategoriesId[k];
                                    let recordtemplate = foldtemplate.childrenCategories.find(f => f.uniqueId == reordCatoriesId.tempalteId);
                                    if (recordtemplate != null && recordtemplate.currentCategories != null) {
                                        for (let u = 0; u < recordtemplate.currentCategories.length; u++) {
                                            if (recordtemplate.currentCategories[u].id == reordCatoriesId.categoryId) {
                                                recordtemplate.currentCategories[u].checked = true;
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    }
                }

                if (this.props.item.childTemplate != null) {
                    for (let i = 0; i < this.props.item.childTemplate.length; i++) {
                        let foldTemplate = this.props.item.childTemplate[i];
                        let foldTempalteId = this.props.item.childTemplate[i].uniqueId;
                        let foldCategoryId = this.props.item.childTemplate[i].currentCategories[0].id;
                        var isExitsFoldTemplatePush = this.props.item.pushFoldTemplateCategoriesId.find(r => r.tempalteId == foldTempalteId);
                        if (isExitsFoldTemplatePush == null) {
                            //说明没有找到这个template在pushcolumn中设置过,是新add的template
                            this.props.item.childTemplate[i].currentCategories[0].checked = true;
                            this.props.item.pushFoldTemplateCategoriesId.push({ tempalteId: foldTempalteId, categoryId: foldCategoryId });
                        }
                        if (foldTemplate.childrenCategories != null)
                        {
                            for (let j = 0; j < foldTemplate.childrenCategories.length; j++)
                            {
                                let recordtemplate = foldTemplate.childrenCategories[j];
                                let reordTempalteId = recordtemplate.uniqueId;
                                let reocrdCategoryId = recordtemplate.currentCategories[0].id;
                                var isExitsRecordTemplatePush = this.props.item.pushRecordTemplateCategoriesId.find(r => r.tempalteId == reordTempalteId);
                                if (isExitsRecordTemplatePush == null) {
                                    //说明没有找到这个template在pushcolumn中设置过,是新add的template
                                    recordtemplate.currentCategories[0].checked = true;
                                    this.props.item.pushRecordTemplateCategoriesId.push({ tempalteId: reordTempalteId, categoryId: reocrdCategoryId });
                                }
                            }
                        }
                    }
                }
                this.state.pushFoldTemplateCategoriesId = this.props.item.pushFoldTemplateCategoriesId;
                this.state.pushRecordTemplateCategoriesId = this.props.item.pushRecordTemplateCategoriesId;
            }
            //如果pushFoldTemplateCategoriesId为null  则可以判断是new column(或者临时创建的没有勾选pushcolumn的column重新编辑选择pushcolumn)
            else if ((this.props.item.pushFoldTemplateCategoriesId == null || this.props.item.pushFoldTemplateCategoriesId.length == 0) && this.props.item.childTemplate != null) {
                for (let i = 0; i < this.props.item.childTemplate.length; i++) {
                    if (this.props.item.childTemplate[i].currentCategories != null) {
                        this.props.item.childTemplate[i].currentCategories[0].checked = true;
                        let tempalteId = this.props.item.childTemplate[i].currentCategories[0].templateId;
                        let categoryId = this.props.item.childTemplate[i].currentCategories[0].id;
                        let ids = RM.deepcopy(this.state.pushFoldTemplateCategoriesId);
                        if (ids.length == 0) {
                            ids.push({ tempalteId: tempalteId, categoryId: categoryId });
                        } else {
                            var isExitsItem = ids.find(t => t.tempalteId == tempalteId);
                            if (isExitsItem == null) {
                                ids.push({ tempalteId: tempalteId, categoryId: categoryId });
                            }
                            else {
                                isExitsItem.categoryId = categoryId;
                            }
                        }
                        this.state.pushFoldTemplateCategoriesId = ids;
                    }
                    if (this.props.item.childTemplate[i].childrenCategories != null) {
                        for (let j = 0; j < this.props.item.childTemplate[i].childrenCategories.length; j++) {
                            if (this.props.item.childTemplate[i].childrenCategories[j].currentCategories != null) {
                                this.props.item.childTemplate[i].childrenCategories[j].currentCategories[0].checked = true;
                                let tempalteId = this.props.item.childTemplate[i].childrenCategories[j].currentCategories[0].templateId;
                                let categoryId = this.props.item.childTemplate[i].childrenCategories[j].currentCategories[0].id;
                                let ids = RM.deepcopy(this.state.pushRecordTemplateCategoriesId);
                                if (ids.length == 0) {
                                    ids.push({ tempalteId: tempalteId, categoryId: categoryId });
                                } else {
                                    var isExitsItem = ids.find(t => t.tempalteId == tempalteId);
                                    if (isExitsItem == null) {
                                        ids.push({ tempalteId: tempalteId, categoryId: categoryId });
                                    }
                                    else {
                                        isExitsItem.categoryId = categoryId;
                                    }
                                }
                                this.state.pushRecordTemplateCategoriesId = ids;
                            }
                        }
                    }
                }
            }
        }
        if (this.props.panelType == Constants.panelType.Folder) {
            //Edit push column
            if (this.props.item.pushRecordTemplateCategoriesId != null && this.props.item.pushRecordTemplateCategoriesId.length > 0 && this.props.item.childTemplate != null) {
                for (let i = 0; i < this.props.item.pushRecordTemplateCategoriesId.length; i++) {
                    let recordCatoriesId = this.props.item.pushRecordTemplateCategoriesId[i];
                    let reordtemplate = this.props.item.childTemplate.find(f => f.uniqueId == recordCatoriesId.tempalteId);
                    if (reordtemplate != null && reordtemplate.currentCategories != null) {
                        for (let j = 0; j < reordtemplate.currentCategories.length; j++) {
                            if (reordtemplate.currentCategories[j].id == recordCatoriesId.categoryId) {
                                reordtemplate.currentCategories[j].checked = true;
                            }
                        }
                    }
                }
                //支持RECO-6029
                if (this.props.item.childTemplate != null)
                {
                    for (let i = 0; i < this.props.item.childTemplate.length; i++)
                    {
                        let tempalteId = this.props.item.childTemplate[i].uniqueId;
                        let categoryId = this.props.item.childTemplate[i].currentCategories[0].id;
                        var isExitsTemplatePush = this.props.item.pushRecordTemplateCategoriesId.find(r => r.tempalteId == tempalteId);
                        if (isExitsTemplatePush == null) {
                            //说明没有找到这个template在pushcolumn中设置过
                            this.props.item.childTemplate[i].currentCategories[0].checked = true;
                            this.props.item.pushRecordTemplateCategoriesId.push({ tempalteId: tempalteId, categoryId: categoryId });
                        }
                    }
                }
                this.state.pushRecordTemplateCategoriesId = this.props.item.pushRecordTemplateCategoriesId;
            }
            else if ((this.props.item.pushRecordTemplateCategoriesId == null || this.props.item.pushRecordTemplateCategoriesId.length == 0) && this.props.item.childTemplate != null) {
                for (var i = 0; i < this.props.item.childTemplate.length; i++) {
                    if (this.props.item.childTemplate[i].currentCategories != null) {
                        this.props.item.childTemplate[i].currentCategories[0].checked = true;
                        let tempalteId = this.props.item.childTemplate[i].currentCategories[0].templateId;
                        let categoryId = this.props.item.childTemplate[i].currentCategories[0].id;
                        let ids = RM.deepcopy(this.state.pushRecordTemplateCategoriesId);
                        if (ids.length == 0) {
                            ids.push({ tempalteId: tempalteId, categoryId: categoryId });
                        } else {
                            var isExitsItem = ids.find(t => t.tempalteId == tempalteId);
                            if (isExitsItem == null) {
                                ids.push({ tempalteId: tempalteId, categoryId: categoryId });
                            }
                            else {
                                isExitsItem.categoryId = categoryId;
                            }
                        }
                        this.state.pushRecordTemplateCategoriesId = ids;
                    }
                }
            }
        }
        
        if(this.props.item.optionsMaxIdReachedValue){
            this.currentOptId = this.props.item.optionsMaxIdReachedValue;
        }
        if (this.props.item.uniqueId)
        {
            if (PhysicalDefaultArray.indexOf(this.props.item.uniqueId) > -1)
            {
                this.state.disablePush = true;
            }
        }
        this.state.columnName = this.props.item.columnName || "";
        this.state.columnTypes = tempColumnTypes;
        this.state.required = this.props.item.required;
        this.state.allowSort = this.props.item.allowSort;
        this.state.showSortOption =  this.props.item.allowEditSort || this.isDefaultColAllowSort(this.props.item.uniqueId);
        this.state.options = optArray;
        this.state.pushToChild = this.props.item.pushToChild;
        this.state.childInheritsValue = this.props.item.childInheritsValue;
        this.state.allowModifyValue = this.props.item.allowModifyValue;
        this.state.childTemplate = this.props.item.childTemplate;
        this.state.panelType = this.props.panelType;
    }

    componentCreate() {

    }

    componentInit() {

    }

    componentReceive(action, args) {
        if (action == "onSave") {

            let optionObject = {};
            let validationError = false;
            this.setState({ isSaving: true });
            //validate empty value in Component
            validationError = $.trim(this.state.columnName) == "" || this.typeId == 0;
            let optionsValues4CheckSameName = [];
            for (let index = 0; index < this.state.options.length; index++) {
                const element = this.state.options[index];
                if (element.value == "") {
                    validationError = true;
                } else {
                    if (optionsValues4CheckSameName.indexOf(element.value) > -1) {
                        element.hasSameOption = true;
                        validationError = true;
                        let updateOptions = JSON.parse(JSON.stringify(this.state.options));
                        this.setState({ options: updateOptions });
                    } else {
                        optionsValues4CheckSameName.push(element.value);
                    }
                }
                optionObject[parseInt(element.id, 10)] = element.value;
            }
            if (validationError) {
                return false;
            }
            if (this.state.pushToChild) {
                validationError = this.checkSubTempalteSameColumn();
            }
            //validate empty value in Component
            if (validationError) {
                return false;
            }

            this.hasChanged && this.props.notifySettingsChanged();
            if (!args({
                columnName: $.trim(this.state.columnName),
                typeId: this.typeId,
                required: this.state.required,
                optionsJSON: JSON.stringify(optionObject),
                optionsMaxIdReachedValue: this.currentOptId,
                categoryId: this.props.item.categoryId,
                allowEdit: this.props.item.allowEdit,
                allowEditSort: this.state.showSortOption,
                allowSort: this.state.allowSort,
                index: this.props.item.index,
                uniqueId: this.props.item.uniqueId,
                pushToChild: this.state.pushToChild,
                //pushCategoryId: this.state.pushCategoryId,
                //pushFolderCategoryId: this.state.pushFolderCategoryId,
                pushFoldTemplateCategoriesId: this.state.pushToChild ? this.state.pushFoldTemplateCategoriesId : null,
                pushRecordTemplateCategoriesId: this.state.pushToChild ? this.state.pushRecordTemplateCategoriesId : null,
                childInheritsValue: this.state.childInheritsValue,
                allowModifyValue: this.state.allowModifyValue
            })) {
                this.showMessageTip("error", RMResx.RM_EditTemplate_SameColumnNameErrorMessage);
                this.props.notifySettingsChanged(false);
            } 
        } else if (action == "duplicateError") {
            this.showMessageTip("error", RMResx.RM_EditTemplate_SameColumnNameInOtherTemplateErrorMessage);
            this.props.notifySettingsChanged(false);
        }
    }

    isAllowSort(columnTypeId) {
        return [Constants.ColumnTypesEnum.SingleText,
            Constants.ColumnTypesEnum.DateTime,
            Constants.ColumnTypesEnum.SingleChoice,
            Constants.ColumnTypesEnum.Number
        ].includes(columnTypeId);
    }

    isDefaultColAllowSort(uniqueId){
        let defaultColAllowSortUniqueIds = [
            PhysicalDefaultColumnIDs.Capability,
            PhysicalDefaultColumnIDs.Format,
            PhysicalDefaultColumnIDs.ProtectiveMarking,
            PhysicalDefaultColumnIDs.DataClosed
        ];
        if(uniqueId){
            return defaultColAllowSortUniqueIds.includes(uniqueId);
        }else{
            return false;
        }
    }

    wrapperI18N(str) {
        return RMResx[str] || str;
    }

    checkSubTempalteSameColumn()
    {
        var hasSame = false;
        if (this.state.panelType == Constants.panelType.Box)
        {
            let sameFoldColumntemplate = {};
            let sameRecordColumntemplate = {};
            if (this.state.pushFoldTemplateCategoriesId) {
                for (let i = 0; i < this.state.pushFoldTemplateCategoriesId.length; i++) {
                    let foldCatoriesId = this.state.pushFoldTemplateCategoriesId[i];
                    let foldTemplate = this.props.item.childTemplate.find(f => f.uniqueId == foldCatoriesId.tempalteId);
                    if (foldTemplate != null && foldTemplate.currentCategories != null) {
                        for (let ca = 0; ca < foldTemplate.currentCategories.length; ca++) {
                            let column = foldTemplate.currentCategories[ca].columns.find(c => (this.isSameColumn(RMResx[c.columnName], this.state.columnName) || this.isSameColumn(c.columnName, this.state.columnName))  && c.inheritFromParent != true && c.inheritFromParentFolder != true);
                            if (column != null) {
                                hasSame = true;
                                sameFoldColumntemplate[foldTemplate.uniqueId] = true;
                                break;
                            }
                        }
                        if (foldTemplate.childrenCategories && this.state.pushRecordTemplateCategoriesId) {
                            for (let j = 0; j < this.state.pushRecordTemplateCategoriesId.length; j++) {
                                let recordCatoriesId = this.state.pushRecordTemplateCategoriesId[j];
                                let recordTemplate = foldTemplate.childrenCategories.find(f => f.uniqueId == recordCatoriesId.tempalteId);
                                if (recordTemplate != null && recordTemplate.currentCategories != null) {
                                    for (let ca = 0; ca < recordTemplate.currentCategories.length; ca++) {
                                        let column = recordTemplate.currentCategories[ca].columns.find(c => (this.isSameColumn(RMResx[c.columnName], this.state.columnName) || this.isSameColumn(c.columnName, this.state.columnName))  && c.inheritFromParent != true && c.inheritFromParentFolder != true);
                                        if (column != null) {
                                            hasSame = true;
                                            sameRecordColumntemplate[recordTemplate.uniqueId] = true;
                                            break;
                                        }
                                    }
                                }
                            }

                        }
                    }
                }
                this.setState({
                    pushFolderColumnSameName: sameFoldColumntemplate
                });
                this.setState({
                    pushRecordColumnSameName: sameRecordColumntemplate
                });
            }

        }
        else if (this.state.panelType == Constants.panelType.Folder)
        {
            let sameColumntemplate = {};
            if (this.state.pushRecordTemplateCategoriesId) {
                for (let i = 0; i < this.state.pushRecordTemplateCategoriesId.length; i++) {
                    let recordTemplateCategoriesId = this.state.pushRecordTemplateCategoriesId[i];
                    let recordTemplate = this.props.item.childTemplate.find(f => f.uniqueId == recordTemplateCategoriesId.tempalteId);
                    if (recordTemplate != null && recordTemplate.currentCategories != null) {
                        for (let ca = 0; ca < recordTemplate.currentCategories.length; ca++)
                        {
                            let column = recordTemplate.currentCategories[ca].columns.find(c => (this.isSameColumn(RMResx[c.columnName], this.state.columnName) || this.isSameColumn(c.columnName, this.state.columnName)) && c.inheritFromParent != true && c.inheritFromParentFolder != true);
                            if (column != null) {
                                hasSame = true;
                                sameColumntemplate[recordTemplate.uniqueId] = true;
                                break;
                            }
                        }
                    }
                }
                this.setState({
                    pushRecordColumnSameName: sameColumntemplate
                });
            }
        }
        return hasSame;
    }

	isSameColumn(name1, name2) {
        if (name1 && name2) {
            return name1.toLowerCase() == name2.toLowerCase();
        }
        return false;
    }
	
    handleTypesSelectedChange = (args) => {
        this.typeId = args.newValue.id;
        let typeAllowSort = this.isAllowSort(this.typeId);
        if (this.typeId == Constants.ColumnTypesEnum.SingleChoice || this.typeId == Constants.ColumnTypesEnum.MultipleChoice) {
            this.setState({
                options: [{ index: 0, id: this.currentOptId, value: "" }],
                showSortOption: typeAllowSort
            });
        } else {
            this.setState({
                options: [],
                showSortOption: typeAllowSort
            });
        }
        this.hasChanged = true;
    }

    handleChildTempalteSelectedChange = (args) => {
        let tempalteId = args.newValue.templateId;
        let categoryId = args.newValue.id;
        let ids = RM.deepcopy(this.state.pushRecordTemplateCategoriesId);
        if (ids.length == 0) {
            ids.push({ tempalteId: tempalteId, categoryId: categoryId });
        } else {
            var isExitsItem = ids.find(t => t.tempalteId == tempalteId);
            if (isExitsItem == null) {
                ids.push({ tempalteId: tempalteId, categoryId: categoryId });
            }
            else {
                isExitsItem.categoryId = categoryId;
            }
        }
        this.setState({ pushRecordTemplateCategoriesId: ids });
        this.hasChanged = true;
    }

    handleChildFolderTempalteSelectedChange = (args) => {
        let tempalteId = args.newValue.templateId;
        let categoryId = args.newValue.id;
        let ids = RM.deepcopy(this.state.pushFoldTemplateCategoriesId);

        if (ids.length == 0) {
            ids.push({ tempalteId: tempalteId, categoryId: categoryId });
        } else {
            var isExitsItem = ids.find(t => t.tempalteId == tempalteId);
            if (isExitsItem == null) {
                ids.push({ tempalteId: tempalteId, categoryId: categoryId });
            }
            else
            {
                isExitsItem.categoryId = categoryId;
            }
        }
        this.setState({ pushFoldTemplateCategoriesId: ids });
        this.hasChanged = true;
    }

    handleRequiredClick = (checked) => {
        this.setState({ required: checked });
        this.hasChanged = true;
    }

    handleSortedClick = (checked) => {
        this.setState({ allowSort: checked });
        this.hasChanged = true;
    }

    handlePushToChildClick = (checked) => {
        this.setState({ pushToChild: checked });
        this.hasChanged = true;
    }

    handleChildInheritsValueClick = (e, args) => {
        this.setState({ childInheritsValue: args.item.checked });
        this.hasChanged = true;
    }

    handleAllowModifyValueClick = (checked) => {
        this.setState({ allowModifyValue: checked });
        this.hasChanged = true;
    }

    handleColumnNameChanged = (value) => {
        this.setState({ columnName: value });
        this.hasChanged = true;
    }

    handleAddOption = (index) => {
        let options = JSON.parse(JSON.stringify(this.state.options));
        options.push({ index: index + 1, id: ++this.currentOptId, value: "" });
        this.setState({
            options: options
        });
        this.hasChanged = true;
    }

    handleDelOption = (index) => {
        //console.log(index);
        let options = JSON.parse(JSON.stringify(this.state.options));
        options.splice(index, 1);
        this.setState({
            options: options
        });
        this.hasChanged = true;
    }

    handleTextChange(option, index, value) {
        //console.log(index);
        let options = JSON.parse(JSON.stringify(this.state.options));
        options[index].value = value;
        options[index].hasSameOption = false;
        // option.value = args.value;
        this.setState({
            options: options
        });
        this.hasChanged = true;
    }



    showMessageTip = (type, msg) => {
        let tipOption = {
            showTip: true,
            tipType: type,
            tipMsg: msg
        };
        this.setState(tipOption);
    }

    hideMessageTip = () => {
        this.setState({
            showTip: false
        });
    }
    renderFolderPushTables() {
        let folderCategoryBlock = [];
        let childTemplate = this.state.childTemplate;
        if (childTemplate != null) {
            for (let i = 0; i < childTemplate.length; i++) {
                folderCategoryBlock.push(<div className="margin-left-32 margin-top-10" key={childTemplate[i].uniqueId}>
                    <div className="ra-option-title margin-bottom-s">
                        <label tabIndex='0'>{RMResx.RM_EditTemplate_SelectFoldCategory + " " + childTemplate[i].templateName}</label>
                    </div>
                    <R.Combobox
                        id={"raPrmTplFolderPushCombobox" + i}
                        textField='name'
                        valueField='id'
                        checkedField='checked'
                        width={300}
                        items={childTemplate[i].currentCategories}
                        searchable={false}
                        disabled={false}
                        onChange={this.handleChildFolderTempalteSelectedChange}
                    />
                    {this.state.isSaving && this.state.pushFolderColumnSameName[childTemplate[i].uniqueId] && <div className={"ra-validation-msg"}>
                        {RMResx.RM_PRM_PRE_ColumnValid_PushColumnIsExist}
                    </div>}
                    <div>
                        {childTemplate[i].childrenCategories != null &&
                            <div>{this.renderRecordPushTable(childTemplate[i].childrenCategories)}</div>
                        }
                    </div>
                </div>
                );
            }
        }
        return <div id='folderCategoryTable'>{folderCategoryBlock.length > 0 && folderCategoryBlock}</div>;
    }

    renderRecordPushTable(childrenTemplate) {
        let recordCategoryBlock = [];
        if (childrenTemplate != null) {
            for (let i = 0; i < childrenTemplate.length; i++) {
                if (this.state.pushRecordColumnSameName[childrenTemplate[i].uniqueId] === undefined) {
                    this.state.pushRecordColumnSameName[childrenTemplate[i].uniqueId] = false;
                }
                recordCategoryBlock.push(<div className="margin-left-32 margin-top-5" key={childrenTemplate[i].uniqueId}>
                    <div className="ra-option-title  margin-bottom-s">
                        <label tabIndex='0'>{RMResx.RM_EditTemplate_SelectRecordCategory + " " + childrenTemplate[i].templateName}</label>
                    </div>
                    <R.Combobox
                        id={"raPrmTplRecordPushCombobox" + i}
                        textField='name'
                        valueField='id'
                        checkedField='checked'
                        width={300}
                        searchable={false}
                        items={childrenTemplate[i].currentCategories}
                        disabled={false}
                        onChange={this.handleChildTempalteSelectedChange}
                    />
                    {this.state.isSaving && this.state.pushRecordColumnSameName[childrenTemplate[i].uniqueId] && <div className={"ra-validation-msg"}>
                        {RMResx.RM_PRM_PRE_ColumnValid_PushColumnIsExist}
                    </div>}

                </div>
                );
            }
        }
        return <div id='recordCategoryTable'>{recordCategoryBlock.length > 0 && recordCategoryBlock}</div>;
    }
    render() {
        return <React.Fragment>
            <div style={{ marginBottom: 12 }} hidden={!this.state.showTip} id={this.props.id}>
                <R.Messagebar
                    message={this.state.tipMsg} classify={this.state.tipType}
                    onClose={this.hideMessageTip} status={{ show: this.state.showTip }} />
            </div>
            <div className="ra-form-label">
                <div className="ra-option-title require">
                    <label>{RMResx.RM_EditTemplate_ColumnName}</label>
                </div>
            </div>
            <div>
                <R.Input
                    id="raPrmTplColumnNameIpt"
                    type="text"
                    // className="ra-option-input"
                    width={300}
                    value={this.state.columnName || ""}
                    onChange={this.handleColumnNameChanged}
                    aria={{ ariaLabel: RMResx.RM_EditTemplate_ColumnName, 'aria-required': true }}
                />
                {this.state.isSaving && $.trim(this.state.columnName) == "" && <div className={"ra-validation-msg"}>
                    {RMResx.RM_Template_Column_ValueValidate}
                </div>}
            </div>

            <div className="ra-form-label margin-top-20">
                <div className="ra-option-title require">
                    <label id="ariaColumnType">{StringUtil.trimEndColon(RMResx.RM_EditTemplate_ColumnType)}</label>
                </div>
            </div>
            <div>
                <R.Combobox
                    id="raPrmTplColumnTypeCombobox"
                    textField='name'
                    valueField='id'
                    checkedField='checked'
                    width={300}
                    searchable={false}
                    items={this.state.columnTypes}
                    disabled={!!this.props.item.uniqueId}
                    onChange={this.handleTypesSelectedChange}
                    aria={{ 
                        ariaLabel: StringUtil.trimEndColon(RMResx.RM_EditTemplate_ColumnType), 
                        ariaRequired: true 
                    }}
                />
                {this.state.isSaving && this.typeId == 0 && <div className={"ra-validation-msg"}>
                    {RMResx.RM_PRM_PRE_ColumnValid_RequireSingleChoice}
                </div>}
            </div>

            {this.state.options.length > 0 &&
                <div className="ra-form-label margin-top-20">
                    <div className="ra-option-title require">
                        <label>{StringUtil.trimEndColon(RMResx.RM_EditTemplate_ColumnOptions)}</label>
                    </div>
                </div>}
            {this.state.options.length > 0 &&
                this.state.options.map((option, index) => {
                    if (option.id && option.id > this.currentOptId) {
                        this.currentOptId = option.id;
                    }
                    return (
                        <div key={index}>
                            <div className="margin-top-10">
                                <R.Input
                                    id="raPrmTplColumnOptionIpt"
                                    type='text'
                                    width={300}
                                    // style={{ verticalAlign: 'text-top' }}
                                    value={option.value || ""}
                                    onChange={this.handleTextChange.bind(this, option, index)}
                                    aria={{ ariaLabel: StringUtil.trimEndColon(RMResx.RM_EditTemplate_ColumnOptions), 'aria-required': true }}
                                />

                                <div className='option-actions'>
                                    {index > 0 && <R.Button
                                        id="raPrmTplColumnDelBtn"
                                        type="bald"
                                        icon="fia-delete"
                                        onClick={this.handleDelOption.bind(this, index)}
                                        tooltip={RMResx.RM_JS_Common_Delete}
                                    />
                                    }

                                    {index == this.state.options.length - 1 &&
                                        <R.Button
                                            id="raPrmTplColumnAddBtn"
                                            type="bald"
                                            icon="fia-plus"
                                            onClick={this.handleAddOption.bind(this, index)}
                                            tooltip={RMResx.RM_JS_BCM_Explorer_MRR_Button_Add}
                                        />
                                    }
                                </div>
                                {this.state.isSaving && (option.value == null || option.value == "") && <div className={"ra-validation-msg"}>
                                    {RMResx.RM_Template_Column_ValueValidate}
                                </div>}
                                {this.state.isSaving && option.hasSameOption && <div className={"ra-validation-msg"}>
                                    {RMResx.RM_Template_Column_SaveOptionValueValidate}
                                </div>}
                            </div>
                        </div>);
                })
            }

            <div className="margin-top-20">
                <R.Checkbox
                    id="raPrmTplColRequired"
                    name="checkbox-demo1"
                    text={RMResx.RM_EditTemplate_ColumnRequired}
                    title={RMResx.RM_EditTemplate_ColumnRequired}
                    checked={this.state.required || false}
                    onChange={this.handleRequiredClick}
                />
            </div>
            {this.state.showSortOption &&
                <div className="margin-top-20">
                    <R.Checkbox
                        id="raPrmTplColAllowSort"
                        text={RMResx.RM_EditTemplate_ColumnAllowSort}
                        title={RMResx.RM_EditTemplate_ColumnAllowSort}
                        checked={this.state.allowSort || false}
                        onChange={this.handleSortedClick}
                    />
                    <$g.Popover>{RMResx.RM_EditTemplate_ColumnSortTip}</$g.Popover>
                </div>
            }

            <div className="margin-top-20">
                {this.state.panelType != Constants.panelType.Record && (this.props.item.childTemplate != null && this.props.item.childTemplate.length > 0) && <R.Checkbox
                    name="checkbox-demo2"
                    text={RMResx.RM_EditTemplate_ColumnPushToChild}
                    title={RMResx.RM_EditTemplate_ColumnPushToChild}
                    checked={this.state.pushToChild || false}
                    onChange={this.handlePushToChildClick}
                    disabled={this.state.disablePush}
                />}
                {this.state.pushToChild &&
                    <div>
                        {this.state.panelType == Constants.panelType.Box &&
                            <div>{this.renderFolderPushTables()}</div>
                        }
                        {this.state.panelType == Constants.panelType.Folder &&
                            <div>{this.renderRecordPushTable(this.state.childTemplate)}</div>
                        }
                    </div>}

                {/* {this.state.pushToChild && <div className="margin-top-20">
                    <R.Checkbox
                        name="checkbox-demo3"
                        text={".Child inherits the value"}
                        title={"..Child inherits the value"}
                        checked={this.state.childInheritsValue || false}
                        onChange={this.handleChildInheritsValueClick}
                    />
                </div>} */}

                {this.state.pushToChild && <div className="margin-left-32 margin-top-20">
                    <R.Checkbox
                        name="checkbox-demo4"
                        text={RMResx.RM_EditTemplate_ColumnAllowModified}
                        title={RMResx.RM_EditTemplate_ColumnAllowModified}
                        checked={this.state.allowModifyValue || false}
                        onChange={this.handleAllowModifyValueClick}
                    />
                </div>}

            </div>

        </React.Fragment>;

    }
}
export { ColumnForm };