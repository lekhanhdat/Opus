import { CONTAINER_MAPPING_URL } from "../../Discovery/AnalysisConfigurator/Office365/Component/AnalysisConfigurationPanelComponent"

export const MappingContainerName = (items) => {
    return items.map(item => (
        CONTAINER_MAPPING_URL[item.name] ? {...item, name: CONTAINER_MAPPING_URL[item.name]} : {...item}
    ))
}