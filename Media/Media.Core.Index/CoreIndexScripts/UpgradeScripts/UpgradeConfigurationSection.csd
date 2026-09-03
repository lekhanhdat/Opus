<?xml version="1.0" encoding="utf-8"?>
<configurationSectionModel xmlns:dm0="http://schemas.microsoft.com/VisualStudio/2008/DslTools/Core" dslVersion="1.0.0.0" Id="b6fa000d-d4c7-464e-9768-bcc853d5de5b" namespace="AvePoint.Media.Core.Index" xmlSchemaNamespace="urn:AvePoint.Media.Core.Index" assemblyName="MediaCoreIndex" xmlns="http://schemas.microsoft.com/dsltools/ConfigurationSectionDesigner">
  <typeDefinitions>
    <externalType name="String" namespace="System" />
    <externalType name="Boolean" namespace="System" />
    <externalType name="Int32" namespace="System" />
    <externalType name="Int64" namespace="System" />
    <externalType name="Single" namespace="System" />
    <externalType name="Double" namespace="System" />
    <externalType name="DateTime" namespace="System" />
    <externalType name="TimeSpan" namespace="System" />
    <externalType name="UpgradeConfigurationCollection" namespace="AvePoint.Media.Core.Index" />
    <enumeratedType name="UpgradeType" namespace="AvePoint.Media.Core.Index" documentation="Upgrade type if index database">
      <literals>
        <enumerationLiteral name="Table" value="1" documentation="Upgrade table" />
        <enumerationLiteral name="Column" value="0" documentation="Upgrade table column" />
      </literals>
    </enumeratedType>
  </typeDefinitions>
  <configurationElements>
    <configurationElement name="UpgradeConfiguration" namespace="AvePoint.Media.Core.Index">
      <attributeProperties>
        <attributeProperty name="TableName" isRequired="false" isKey="false" isDefaultCollection="false" xmlName="tableName" isReadOnly="false">
          <type>
            <externalTypeMoniker name="/b6fa000d-d4c7-464e-9768-bcc853d5de5b/String" />
          </type>
        </attributeProperty>
        <attributeProperty name="ColumnName" isRequired="false" isKey="false" isDefaultCollection="false" xmlName="columnName" isReadOnly="false">
          <type>
            <externalTypeMoniker name="/b6fa000d-d4c7-464e-9768-bcc853d5de5b/String" />
          </type>
        </attributeProperty>
        <attributeProperty name="UpgradeExpression" isRequired="false" isKey="false" isDefaultCollection="false" xmlName="upgradeExpression" isReadOnly="false">
          <type>
            <externalTypeMoniker name="/b6fa000d-d4c7-464e-9768-bcc853d5de5b/String" />
          </type>
        </attributeProperty>
        <attributeProperty name="Id" isRequired="true" isKey="true" isDefaultCollection="false" xmlName="id" isReadOnly="false">
          <type>
            <externalTypeMoniker name="/b6fa000d-d4c7-464e-9768-bcc853d5de5b/String" />
          </type>
        </attributeProperty>
        <attributeProperty name="UpgradeType" isRequired="true" isKey="false" isDefaultCollection="false" xmlName="upgradeType" isReadOnly="false">
          <type>
            <externalTypeMoniker name="/b6fa000d-d4c7-464e-9768-bcc853d5de5b/String" />
          </type>
        </attributeProperty>
      </attributeProperties>
    </configurationElement>
    <configurationElementCollection name="UpgradeConfigurationCollection" namespace="AvePoint.Media.Core.Index" documentation="The collection of upgrade configuration" xmlItemName="upgradeConfiguration" codeGenOptions="Indexer, AddMethod, RemoveMethod, GetItemMethods">
      <itemType>
        <configurationElementMoniker name="/b6fa000d-d4c7-464e-9768-bcc853d5de5b/UpgradeConfiguration" />
      </itemType>
    </configurationElementCollection>
    <configurationSection name="UpgradeConfigurationSectionHandler" documentation="The upgrade configuration section handler" codeGenOptions="Singleton, XmlnsProperty" xmlSectionName="upgradeConfigurationSectionHandler">
      <attributeProperties>
        <attributeProperty name="UpgradeConfigurationCollection" isRequired="false" isKey="false" isDefaultCollection="false" xmlName="upgradeConfigurationCollection" isReadOnly="false" documentation="Upgrade configuration collection of index database">
          <type>
            <externalTypeMoniker name="/b6fa000d-d4c7-464e-9768-bcc853d5de5b/UpgradeConfigurationCollection" />
          </type>
        </attributeProperty>
      </attributeProperties>
    </configurationSection>
  </configurationElements>
  <propertyValidators>
    <validators />
  </propertyValidators>
</configurationSectionModel>