import { 
    RuleLevels, 
    RuleSourceTabIndex, 
    RuleModules,
    RuleModuleTypes
} from "./Constains";

import { 
    RuleCriterias,
    RuleAction,
    ManualApprove,
    Export,
    ArchiverStorage
} from "./RDComponent/RDSourceTabRows/Index";

import { LicenseHelper } from "../../../Utilities/CommonUtil";

export default class RDConfig {
    constructor(ruleItem, module) {
        this.ruleItem = ruleItem;
        this.module = module;
    }

    getIsShowModuleDetail(){
        return this.module && LicenseHelper.HasOpusILLicense() && LicenseHelper.HasOpusSOLicense();
    }

    getRuleBaseConfig() {
        const { RuleName, Description, ContainerName, ModelType, RuleLevel, DisposalClass } = this.ruleItem;
       
        let moduleDetail = [];
        if (this.getIsShowModuleDetail()) {
            moduleDetail.push({
                name: RMResx.RM_RDM_CreateRule_Title_Module,
                value: RuleModules[ModelType]
            });
        }

        return [
            { 
                name: RMResx.RM_JS_Rule_Detail_Name, 
                value: RuleName
            },
            { 
                name: RMResx.RM_JS_Rule_Detail_Des, 
                value: Description 
            },
            { 
                name: RMResx.RM_JS_Rule_Detail_Container, 
                value: ContainerName 
            }, 
            ...moduleDetail,
            {
                name: RMResx.RM_JS_Rule_Detail_RuleLevel,
                value: RuleLevels[RuleLevel], 
            },
            {
                name: RMResx.RM_JS_Rule_DisposalClass_Title,
                value: DisposalClass,
            },
        ];
    }

    getRuleSourceConfig () {
        let { IsSpSource, IsOneDriveSource, IsExoSource, IsPhySource, IsFSSource,
            IsSPLocalSource, IsAzureFileSource, IsBoxSource, IsConnectorSource, ModelType, IsGoogleDriveSource, IsTeamsSource } = this.ruleItem;

        let { OneDriveRule, EXORule, PhysicalRule, FSRule, SPLocalRule, 
            AzureFileRule, BoxRule, ConnectorRule, GoogleDriveRule, TeamsRule } = this.ruleItem;

        let ruleSourceComponents = this.getRuleSourceComponents()[ModelType] || {};

        return [
            {
                name: RMResx.RM_JS_SPS_TabLabel_SP,
                show: IsSpSource,
                tabIndex: RuleSourceTabIndex.SP,
                icon: "fi-ms-sharepoint",
                content: this.ruleItem,
                component: ruleSourceComponents[RuleSourceTabIndex.SP]
            },
            {
                name: RMResx.RM_JS_SPS_TabLabel_OneDrive,
                show: IsOneDriveSource,
                tabIndex: RuleSourceTabIndex.OneDrive,
                icon: "fi-ms-onedrive",
                content: OneDriveRule,
                component: ruleSourceComponents[RuleSourceTabIndex.OneDrive]
            },
            {
                name: RMResx.RM_JS_SPS_TabLabel_EXO,
                show: IsExoSource,
                tabIndex: RuleSourceTabIndex.Exchange,
                icon: "fi-ms-exchange",
                content: EXORule,
                component: ruleSourceComponents[RuleSourceTabIndex.Exchange]
            },
            {
                name: RMResx.RM_JS_SPS_TabLabel_Physical,
                show: IsPhySource,
                tabIndex: RuleSourceTabIndex.Physical,
                icon: "fia-physical-record",
                content: PhysicalRule,
                component: ruleSourceComponents[RuleSourceTabIndex.Physical]
            },
            {
                name: RMResx.RM_JS_SPS_TabLabel_FS,
                show: IsFSSource,
                tabIndex: RuleSourceTabIndex.FS,
                icon: "fia-fs",
                content: FSRule,
                component: ruleSourceComponents[RuleSourceTabIndex.FS]
            },
            {
                name: RMResx.RM_JS_SPS_TabLabel_SPLocal,
                show: IsSPLocalSource,
                tabIndex: RuleSourceTabIndex.SPLocal,
                icon: "fia-sharepoint",
                content: SPLocalRule,
                component: ruleSourceComponents[RuleSourceTabIndex.SPLocal]
            },
            {
                name: RMResx.RM_JS_Common_ReportType_AzureFile,
                show: IsAzureFileSource,
                tabIndex: RuleSourceTabIndex.AzureFile,
                icon: "fi-ms-azure-file-share",
                content: AzureFileRule,
                component: ruleSourceComponents[RuleSourceTabIndex.AzureFile]
            },
            {
                name: RMResx.RM_JS_SPS_TabLabel_Box,
                show: IsBoxSource,
                tabIndex: RuleSourceTabIndex.Box,
                icon: "fia-box-blue-b",
                content: BoxRule,
                component: ruleSourceComponents[RuleSourceTabIndex.Box]
            },
            {
                name: RMResx.RM_CP_Connector,
                show: IsConnectorSource,
                tabIndex: RuleSourceTabIndex.Connector,
                icon: "fia-connecter",
                content: ConnectorRule,
                component: ruleSourceComponents[RuleSourceTabIndex.Connector]
            },
            {
                name: RMResx.RM_JS_SPS_TabLabel_GoogleDrive,
                show: IsGoogleDriveSource,
                tabIndex: RuleSourceTabIndex.GoogleDrive,
                icon: "fia-google-drive-f",
                content: GoogleDriveRule,
                component: ruleSourceComponents[RuleSourceTabIndex.GoogleDrive]
            },
            {
                name: RMResx.RM_JS_SPS_TabLabel_Teams,
                show: IsTeamsSource,
                tabIndex: RuleSourceTabIndex.Teams,
                icon: "fi-ms-teams",
                content: TeamsRule,
                component: ruleSourceComponents[RuleSourceTabIndex.Teams]
            },
        ];
    }

    getRuleSourceComponents (){
        const { Records, SOArchiver } = RuleModuleTypes;
        return {
            [Records]: {
                [RuleSourceTabIndex.SP]: [ 
                    RuleCriterias, 
                    RuleAction,
                    ManualApprove,
                    Export,
                    ArchiverStorage,
                ],
                [RuleSourceTabIndex.OneDrive]: [
                    RuleCriterias,
                    RuleAction,
                    ManualApprove,
                    Export,
                    ArchiverStorage,
                ],
                [RuleSourceTabIndex.Exchange]: [
                    RuleCriterias,
                    RuleAction,
                    ManualApprove,
                    Export
                ],
                [RuleSourceTabIndex.Physical]:[
                    RuleCriterias,
                    RuleAction,
                    ManualApprove,
                    ArchiverStorage,
                ],
                [RuleSourceTabIndex.FS]: [
                    RuleCriterias,
                    RuleAction,
                    ManualApprove,
                    ArchiverStorage,
                ],
                [RuleSourceTabIndex.SPLocal]: [
                    RuleCriterias,
                    RuleAction,
                    ManualApprove,
                ],
                [RuleSourceTabIndex.AzureFile]: [
                    RuleCriterias,
                    RuleAction,
                    ManualApprove,
                ],
                [RuleSourceTabIndex.Box]: [
                    RuleCriterias,
                    RuleAction,
                    ManualApprove,
                ],
                [RuleSourceTabIndex.Connector]: [
                    RuleCriterias,
                    RuleAction,
                    ManualApprove,
                ],
                [RuleSourceTabIndex.GoogleDrive]: [
                    RuleCriterias,
                    RuleAction,
                    ManualApprove,
                    Export,
                    ArchiverStorage
                ],
                [RuleSourceTabIndex.Teams]: [ 
                    RuleCriterias, 
                    RuleAction,
                    ManualApprove,
                    Export,
                    ArchiverStorage,
                ],
            },
            [SOArchiver]: {
                [RuleSourceTabIndex.SP]: [ 
                    RuleCriterias, 
                    RuleAction,
                    Export,
                    ArchiverStorage,
                ],
                [RuleSourceTabIndex.OneDrive]: [
                    RuleCriterias,
                    RuleAction,
                    Export,
                    ArchiverStorage,
                ],
                [RuleSourceTabIndex.Teams]: [ 
                    RuleCriterias, 
                    RuleAction,
                    Export,
                    ArchiverStorage,
                ],
                [RuleSourceTabIndex.Exchange]: [
                    RuleCriterias,
                    RuleAction,
                    ManualApprove,
                    Export
                ],
                [RuleSourceTabIndex.Physical]:[
                    RuleCriterias,
                    RuleAction,
                    ManualApprove,
                    ArchiverStorage,
                ],
                [RuleSourceTabIndex.FS]: [
                    RuleCriterias,
                    RuleAction,
                    ManualApprove,
                    ArchiverStorage,
                ],
                [RuleSourceTabIndex.SPLocal]: [
                    RuleCriterias,
                    RuleAction,
                    ManualApprove,
                ],
                [RuleSourceTabIndex.AzureFile]: [
                    RuleCriterias,
                    RuleAction,
                    ManualApprove,
                ],
                [RuleSourceTabIndex.Box]: [
                    RuleCriterias,
                    RuleAction,
                    ManualApprove,
                ],
                [RuleSourceTabIndex.Connector]: [
                    RuleCriterias,
                    RuleAction,
                    ManualApprove,
                ],
                [RuleSourceTabIndex.GoogleDrive]: [
                    RuleCriterias,
                    RuleAction,
                    ManualApprove,
                ],
            }  
        };
    }
}
