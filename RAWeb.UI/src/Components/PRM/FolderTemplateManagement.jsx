import { Component } from "react";
import RouterUrls from "../../Constants/RouterUrls";
import * as Constants from "./Constants";
import CommonTemplateManagement from "../PRM/CommonTemplateManagement";
import "../../Less/PRM/TemplateManagement.less";

export default class FolderTemplateManagement extends Component{
    constructor(props){
        super(props);
    }
    
    render(){
        return <React.Fragment>
            <CommonTemplateManagement
                commonType={Constants.CommonTemplateManagementType.Folder}
                getDataUrl="/api/TemplateManagementApi/GetTemplatesByParent"
                newSuiteUrl={RouterUrls.PRM_CreateTemplateSuite}
                editSuiteUrl={RouterUrls.PRM_EditTemplateSuite}
                delSuiteUrl="/api/TemplateManagementApi/DeleteSuite"
                delTemplateUrl="/api/TemplateManagementApi/DeleteTemplate"
                newTemplateUrl={RouterUrls.PRM_CreateTemplate}
                editTemplateUrl={RouterUrls.PRM_EditTemplate}
                redirectFolderTemplateUrl={RouterUrls.PRM_FolderTemplateManagement}
                redirectRecordTemplateUrl={RouterUrls.PRM_RecordTemplateManagement}
            />
        </React.Fragment>;
    }
}

