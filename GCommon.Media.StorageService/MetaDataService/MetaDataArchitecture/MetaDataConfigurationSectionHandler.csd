<?xml version="1.0" encoding="utf-8"?>
<configurationSectionModel xmlns:dm0="http://schemas.microsoft.com/VisualStudio/2008/DslTools/Core" dslVersion="1.0.0.0" Id="f030c719-b50a-45db-9fea-ab23c2b1364d" namespace="AvePoint.GCommon.Media.StorageService.MetaDataService.MetaDataArchitecture" xmlSchemaNamespace="urn:AvePoint.GCommon.Media.StorageService.MetaDataService.MetaDataArchitecture" assemblyName="AvePoint.GCommon.Media.StorageService.MetaDataService.MetaDataArchitecture" xmlns="http://schemas.microsoft.com/dsltools/ConfigurationSectionDesigner">
  <typeDefinitions>
    <externalType name="String" namespace="System" />
    <externalType name="Boolean" namespace="System" />
    <externalType name="Int32" namespace="System" />
    <externalType name="Int64" namespace="System" />
    <externalType name="Single" namespace="System" />
    <externalType name="Double" namespace="System" />
    <externalType name="DateTime" namespace="System" />
    <externalType name="TimeSpan" namespace="System" />
    <externalType name="MetaDataItemCollection" namespace="AvePoint.GCommon.Media.StorageService" />
  </typeDefinitions>
  <configurationElements>
    <configurationSection name="MetaDataSectionHandler" namespace="AvePoint.GCommon.Media.StorageService" codeGenOptions="Singleton, XmlnsProperty" xmlSectionName="metaDataHandler">
      <attributeProperties>
        <attributeProperty name="MetaData" isRequired="false" isKey="false" isDefaultCollection="false" xmlName="metaData" isReadOnly="false">
          <type>
            <externalTypeMoniker name="/f030c719-b50a-45db-9fea-ab23c2b1364d/MetaDataItemCollection" />
          </type>
        </attributeProperty>
      </attributeProperties>
    </configurationSection>
    <configurationElement name="MetaDataItem" namespace="AvePoint.GCommon.Media.StorageService">
      <attributeProperties>
        <attributeProperty name="Key" isRequired="true" isKey="true" isDefaultCollection="false" xmlName="key" isReadOnly="false">
          <type>
            <externalTypeMoniker name="/f030c719-b50a-45db-9fea-ab23c2b1364d/String" />
          </type>
        </attributeProperty>
        <attributeProperty name="Value" isRequired="false" isKey="false" isDefaultCollection="false" xmlName="value" isReadOnly="false">
          <type>
            <externalTypeMoniker name="/f030c719-b50a-45db-9fea-ab23c2b1364d/String" />
          </type>
        </attributeProperty>
      </attributeProperties>
    </configurationElement>
    <configurationElementCollection name="MetaDataItemCollection" namespace="AvePoint.GCommon.Media.StorageService" xmlItemName="metaDataItem" codeGenOptions="Indexer, AddMethod, RemoveMethod, GetItemMethods">
      <itemType>
        <configurationElementMoniker name="/f030c719-b50a-45db-9fea-ab23c2b1364d/MetaDataItem" />
      </itemType>
    </configurationElementCollection>
  </configurationElements>
  <propertyValidators>
    <validators />
  </propertyValidators>
</configurationSectionModel>