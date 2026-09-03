import { 
    RuleSourceTabIndex, 
    RuleModuleTypes
} from "../../Common/RuleDetail/Constains";

import { 
    RuleCriterias,
    RuleAction,
} from "../../Common/RuleDetail/RDComponent/RDSourceTabRows/Index";


const RuleSourceComponents = {
    [RuleModuleTypes.Records]: {
        [RuleSourceTabIndex.SP]: [ 
            RuleCriterias, 
            RuleAction,
        ],
        [RuleSourceTabIndex.OneDrive]: [
            RuleCriterias,
            RuleAction
        ],
        [RuleSourceTabIndex.Exchange]: [
            RuleCriterias,
            RuleAction
        ],
        [RuleSourceTabIndex.Physical]:[
            RuleCriterias,
            RuleAction
        ],
        [RuleSourceTabIndex.FS]: [
            RuleCriterias,
            RuleAction,
        ],
        [RuleSourceTabIndex.SPLocal]: [
            RuleCriterias,
            RuleAction,
        ],
        [RuleSourceTabIndex.AzureFile]: [
            RuleCriterias,
            RuleAction,
        ],
        [RuleSourceTabIndex.Box]: [
            RuleCriterias,
            RuleAction,
        ],
        [RuleSourceTabIndex.Connector]: [
            RuleCriterias,
            RuleAction,
        ],
        [RuleSourceTabIndex.GoogleDrive]: [
            RuleCriterias,
            RuleAction,
        ],
        [RuleSourceTabIndex.Teams]: [
            RuleCriterias,
            RuleAction,
        ],
    },
    [RuleModuleTypes.SOArchiver]: {
        [RuleSourceTabIndex.SP]: [ 
            RuleCriterias, 
            RuleAction,
        ],
        [RuleSourceTabIndex.OneDrive]: [
            RuleCriterias,
            RuleAction
        ],
        [RuleSourceTabIndex.Exchange]: [
            RuleCriterias,
            RuleAction
        ],
        [RuleSourceTabIndex.Physical]:[
            RuleCriterias,
            RuleAction
        ],
        [RuleSourceTabIndex.FS]: [
            RuleCriterias,
            RuleAction
        ],
        [RuleSourceTabIndex.SPLocal]: [
            RuleCriterias,
            RuleAction
        ],
        [RuleSourceTabIndex.AzureFile]: [
            RuleCriterias,
            RuleAction
        ],
        [RuleSourceTabIndex.Box]: [
            RuleCriterias,
            RuleAction
        ],
        [RuleSourceTabIndex.Connector]: [
            RuleCriterias,
            RuleAction
        ],
        [RuleSourceTabIndex.GoogleDrive]: [
            RuleCriterias,
            RuleAction
        ],
        [RuleSourceTabIndex.Teams]: [
            RuleCriterias,
            RuleAction
        ],
    }  
};

export {RuleSourceComponents};