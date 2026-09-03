import { EmptyGUID } from "../../Constants/Constants";
import { StartFromType, TemplateCreateMethod }  from "./Constants";
import '../../Less/PRM/EditTemplate.less';

export default class ViewSuiteSettings extends R.Component {
    idAttr = true;
    componentCreate() {
        this.state = {
            suiteName: "",
            suiteDesc: "",
            selStartFromType: StartFromType.Box,
            selTemplateCreateMethod: TemplateCreateMethod.New,
            rooTemplateId: EmptyGUID,
            selTemplateId: EmptyGUID,
            rootTemplateName: "",
            suiteUniqueId: "",
        };
    }

    componentInit() {
    }

    componentReceive(action, ...args) {
        switch (action) {
            case "init":
                this.loadSuiteData(args[0]);
                break;
        }
    }

    loadSuiteData(suiteUniqueId) {
        let option = {
            url: `/api/TemplateManagementApi/LoadSuite?id=${suiteUniqueId}`,
            method: "get",
        };
        fetchUtility(option).then((result) => {
            if(result)
            {
                this.setState({
                    suiteName: this.wrapperI18N(result.Name),
                    suiteDesc: result.Description,
                    selStartFromType: result.StartFromType,
                    suiteUniqueId: result.UniqueId,
                    selTemplateCreateMethod: result.RootTemplateCreateType,
                    rooTemplateId: result.RootTemplateUniqueId,
                    rootTemplateName: result.RootTemplateName
                });
            }
        }).catch((e) => {

        });
    }

    getSuiteDetails() {
        let result = this.state;
        let details = [];
        if(result && result.suiteName) {
            let suiteName = this.wrapperI18N(result.suiteName);
            let startTypeName = this.getStartFromTypeName(result.selStartFromType);
            details.push({name: RMResx.RM_PRM_TM_Suite_Name, value: suiteName});
            details.push({name: RMResx.RM_PRM_TM_Suite_Desc, value: result.suiteDesc});
            details.push({name: RMResx.RM_PRM_TM_Suite_StartFromTitle, value: startTypeName});
            if(result.selStartFromType == StartFromType.Folder)
            {
                let useExistingFolder = result.selTemplateCreateMethod == TemplateCreateMethod.ExistingFolder;
                let createFolderMethodName = useExistingFolder? RMResx.RM_PRM_TM_Suite_CreateFolderTemplateMethod_AddExisting:RMResx.RM_PRM_TM_Suite_CreateFolderTemplateMethod_New;
                details.push({name: RMResx.RM_PRM_TM_Suite_CreateFolderTemplateMethodTip, value: createFolderMethodName});
                if(useExistingFolder)
                {
                    details.push({name: RMResx.RM_PRM_TM_Suite_SelectExistingFolderTip, value: result.rootTemplateName });
                }
            }
        }
        return details;
    }

    getStartFromTypeName(type) {
        let mapping = {
            [StartFromType.Box]: RMResx.RM_PRM_TM_Suite_StartFromType_Box,
            [StartFromType.Folder]: RMResx.RM_PRM_TM_Suite_StartFromType_Folder,
            [StartFromType.CustomTemplate]: RMResx.RM_PRM_TM_Suite_StartFromType_Custom
        };
        return mapping[type] || "";
    }

    wrapperI18N(str) {
        return RMResx[str] || str;
    }

    render() {
        let details = this.getSuiteDetails();
        return <div id={this.props.id}>
                <$g.DetailList className="detail-content" labelWidth={200}>
                    {
                        details.map((item, rIdx) => {
                            return <$g.DetailRow key={rIdx}>
                                        <$g.DetailCell
                                        // key={cIdx}
                                        label={item.name}
                                        value={item.value}/>
                            </$g.DetailRow>;
                        })
                    }
                </$g.DetailList>
        </div>;
    }
}