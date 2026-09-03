import StringUtil from '../../Utilities/StringUtil';
import * as Constants from "./Constants";
import '../../Less/PRM/EditTemplate.less';

export default class ViewTemplateSettings extends R.Component {
    idAttr = true;
    componentCreate() {
        this.state = {
            templateInfo: null,
            prefix: '',
            numberOfDigits: 0,
            templateName: '',
            templateDesc: '',
            templateType: null,
            newCategoryName: '',
            categoryItems: {},
            childrenTemplates: [],
            templateCategorys: null,
            selectedItem: null,
            panelTitle: '',
            showPanel: false,
            pageTitle: '',
            settingTitle: '',
        };
    }

    componentInit() {
        
    }

    componentReceive(action, ...args) {
        switch (action) {
            case "init":
                this.loadTemplateInfo(args[0], args[1]);
                break;
            
        }
    }

    loadTemplateInfo(uniqueId, templateIdList) {
        this.isEditMode = true;
        this.templateId = uniqueId;
        let option = {
            url: "/Api/TemplateManagementApi/LoadTemplateDatas",
            method: "Post",
            data: {
                TemplateIdUniqueId: uniqueId,
                TemplateIdList: templateIdList
            }
        };
        fetchUtility(option).then((result) => {
            let data = JSON.parse(result);
            if(data)
            {
                let categoryItems = this.initCategoryItems(data);
                this.setState({
                    templateInfo: data,
                    prefix: data.prefix,
                    numberOfDigits: data.numberOfDigits,
                    templateName: RMResx[data.name] ? RMResx[data.name] : data.name,
                    templateDesc: data.description,
                    templateType: data.type,
                    templateCategorys: data.categories,
                    childrenTemplates: data.childTemplateCategories,
                    categoryItems: categoryItems,
                    showMain: true,
                });
            }
        }).catch((e) => {

        });
    }

    initCategoryItems(data) {
        let templateCategories = data.categories;
        let categories = {};
        this.appendSummaryInfo(categories, data);
        templateCategories.map(d => {
            let cId = d.id.toLowerCase();
            if (!categories[cId]) {
                categories[cId] = {};
            }
            categories[cId].isShow = true;
            categories[cId].columnItems = this.appendRowIndex(d.columns);
            categories[cId].name = d.name;
            categories[cId].allowEdit = d.allowEdit;
        });
        return categories;
    }

    appendRowIndex(columns) {
        let newColumns = [];
        columns.map(function (item, idx) {
            item.index = idx + 1;
            newColumns.push(item);
        });
        return newColumns.length > 0 ? newColumns : columns;
    }

    toggleExpander(id, isShow) {
        let cItems = RM.deepcopy(this.state.categoryItems);
        cItems[id].isShow = isShow;
        this.setState({ categoryItems: cItems });
    }

    wrapperI18N(str) {
        return RMResx[str] || str;
    }

    renderColumnDetails(columnItems) {
        
        return <$g.DetailList className="detail-content" labelWidth={132}>
        {
            columnItems.map((item, rIdx) => {
                let detailValue = item.hasOwnProperty("value")? item.value: item.required? RMResx.RM_EditTemplate_ColumnRequired : RMResx.RM_EditTemplate_ColumnNotRequired;
                return <$g.DetailRow key={rIdx}>
                            <$g.DetailCell
                            // key={cIdx}
                            label={this.wrapperI18N(item.columnName)}
                            value={detailValue}/>
                </$g.DetailRow>;
            })
        }
        </$g.DetailList>;
    }

    renderColumnTables() {
        let cItems = RM.deepcopy(this.state.categoryItems);
        let categoriesBlock = [];
        for(let id in cItems)
        {
            if (cItems.hasOwnProperty(id)) {
                let item = cItems[id];
                let tableId = 'table_' + id;
                let hasColumnItems = item.columnItems && item.columnItems.length > 0;
                let columnName = RMResx[item.name] ? RMResx[item.name] : item.name;
                categoriesBlock.push(<div className='ra-table-wrapper' key={id}><R.Expander status={{ show: this.state.categoryItems[id].isShow }} key={id} shown={this.toggleExpander.bind(this, id, true)} hidden={this.toggleExpander.bind(this, id, false)}>
                    <div className='category-content'>
                        <span className='category-label' data-tooltip aria-label={columnName}>{columnName}</span>
                    </div>
                    <div>
                        {
                            hasColumnItems && this.renderColumnDetails(item.columnItems)
                        }
                    </div>
                </R.Expander></div>);
            }
        }
        return <div id='raColumnTables'>{categoriesBlock.length > 0 && categoriesBlock}</div>;
    }

    appendSummaryInfo(categories, data) {
        let uniqueIdSettings = this.props.uniqueIdSettings;
        if(uniqueIdSettings && uniqueIdSettings.IsGlobalSetting) {
            this.appendUniqueIdSettingInfo(data);
        }
        let uniqueId = StringUtil.newGuid();
        categories[uniqueId] = {
            isShow: true,
            name: RMResx.RM_JS_JMD_Tab_Summary,
            columnItems: [{
                categoryId: uniqueId,
                columnName: RMResx.RM_JS_BCM_Explorer_Datagrid_Name,
                value: StringUtil.toI18N(data.name)
            },{
                categoryId: uniqueId,
                columnName: RMResx.RM_PRM_TM_TemplateDesc,
                value: data.description
            },{
                categoryId: uniqueId,
                columnName: RMResx.RM_EditTemplate_Prefix,
                value: data.prefix
            },{
                categoryId: uniqueId,
                columnName: RMResx.RM_EditTemplate_NumberofDigits,
                value: data.numberOfDigits || ""
            }]
        };
    }

    appendUniqueIdSettingInfo(data) {
        let [type, prefix, numberOfDigits] = [data.type, "", ""];
        let uniqueIdSettings = this.props.uniqueIdSettings;
        if(type == Constants.TemplateTypes.Box)
        {
            prefix = uniqueIdSettings.BoxTemplatePrefix;
            numberOfDigits = uniqueIdSettings.BoxTemplateNumberOfDigits;
        }
        if(type == Constants.TemplateTypes.Folder) {
            prefix = uniqueIdSettings.FolderTemplatePrefix;
            numberOfDigits = uniqueIdSettings.FolderTemplateNumberOfDigits;
        }
        if(type == Constants.TemplateTypes.Records) {
            prefix = uniqueIdSettings.RecordTemplatePrefix;
            numberOfDigits = uniqueIdSettings.RecordTemplateNumberOfDigits;
        }
        if(type == Constants.TemplateTypes.CustomTemplate) {
            prefix = uniqueIdSettings.CustomTemplatePrefix;
            numberOfDigits = uniqueIdSettings.CustomTemplateNumberOfDigits;
        }
        data.prefix = prefix;
        data.numberOfDigits = numberOfDigits;
    }

    render() {
        return <div id={this.props.id}>
                    {this.renderColumnTables()}
                </div>
    }
}