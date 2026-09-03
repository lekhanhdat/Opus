import { RuleType } from "../Components/Common/RuleItem/Components/Constants"
import { LicenseHelper } from "./CommonUtil"

export const filterRuleTypesByLicense = (ruleTypes) => {
    const level64RuleTypes = LicenseHelper.EnableRecordsArchiver() ?
        ruleTypes[64] :
        ruleTypes[64].filter((item) =>
            item.id !== RuleType.SensitiveLabel &&
            item.id !== RuleType.RetentionLabelRule &&
            item.id !== RuleType.SensitiveLabelFullName &&
            item.id !== RuleType.ParentLibraryText &&
            item.id !== RuleType.ParentLibraryNumber &&
            item.id !== RuleType.ParentLibraryYestNo &&
            item.id !== RuleType.ParentLibraryDateTime &&
            item.id !== RuleType.ParentSiteCollectionText &&
            item.id !== RuleType.ParentSiteCollectionNumber &&
            item.id !== RuleType.ParentSiteCollectionYestNo &&
            item.id !== RuleType.ParentSiteCollectionDateTime &&
            item.id !== RuleType.PropertyBagText &&
            item.id !== RuleType.PropertyBagNumber &&
            item.id !== RuleType.PropertyBagBoolean &&
            item.id !== RuleType.PropertyBagDateTime
        );
    return { ...ruleTypes, 64: level64RuleTypes }
}