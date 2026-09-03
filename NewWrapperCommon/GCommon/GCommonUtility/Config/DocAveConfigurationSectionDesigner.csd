<?xml version="1.0" encoding="utf-8"?>
<configurationSectionModel xmlns:dm0="http://schemas.microsoft.com/VisualStudio/2008/DslTools/Core" dslVersion="1.0.0.0" Id="e74336ce-2dd7-4a63-8ea8-7c910deac140" namespace="AvePoint.GCommon.Configurations" xmlSchemaNamespace="urn:AvePoint.GCommon.Configurations" assemblyName="CommonUtility" xmlns="http://schemas.microsoft.com/dsltools/ConfigurationSectionDesigner">
  <typeDefinitions>
    <externalType name="String" namespace="System" />
    <externalType name="Boolean" namespace="System" />
    <externalType name="Int32" namespace="System" />
    <externalType name="Int64" namespace="System" />
    <externalType name="Single" namespace="System" />
    <externalType name="Double" namespace="System" />
    <externalType name="DateTime" namespace="System" />
    <externalType name="TimeSpan" namespace="System" />
    <externalType name="TracerCollection" namespace="AvePoint.GCommon.Configurations" />
    <externalType name="Profiler" namespace="AvePoint.GCommon.Configurations" />
    <externalType name="Agent" namespace="AvePoint.GCommon.Configurations" />
    <externalType name="Media" namespace="AvePoint.GCommon.Configurations" />
    <externalType name="Manager" namespace="AvePoint.GCommon.Configurations" />
    <externalType name="Report" namespace="AvePoint.GCommon.Configurations" />
    <externalType name="DocAve" namespace="AvePoint.GCommon.Configurations" />
    <externalType name="Diagnostics" namespace="AvePoint.GCommon.Configurations" />
    <externalType name="StorageService" namespace="AvePoint.GCommon.Configurations" />
    <externalType name="HoldService" namespace="AvePoint.GCommon.Configurations" />
    <externalType name="ExportService" namespace="AvePoint.GCommon.Configurations" />
  </typeDefinitions>
  <configurationElements>
    <configurationElement name="Profiler" namespace="AvePoint.GCommon.Configurations">
      <attributeProperties>
        <attributeProperty name="ProfilerId" isRequired="true" isKey="true" isDefaultCollection="false" xmlName="profilerId" isReadOnly="true">
          <type>
            <externalTypeMoniker name="/e74336ce-2dd7-4a63-8ea8-7c910deac140/String" />
          </type>
        </attributeProperty>
        <attributeProperty name="IsActive" isRequired="false" isKey="false" isDefaultCollection="false" xmlName="isActive" isReadOnly="false">
          <type>
            <externalTypeMoniker name="/e74336ce-2dd7-4a63-8ea8-7c910deac140/Boolean" />
          </type>
        </attributeProperty>
      </attributeProperties>
      <elementProperties>
        <elementProperty name="Tracers" isRequired="false" isKey="false" isDefaultCollection="false" xmlName="tracers" isReadOnly="false">
          <type>
            <configurationElementCollectionMoniker name="/e74336ce-2dd7-4a63-8ea8-7c910deac140/TracerCollection" />
          </type>
        </elementProperty>
      </elementProperties>
    </configurationElement>
    <configurationElementCollection name="TracerCollection" namespace="AvePoint.GCommon.Configurations" xmlItemName="tracer" codeGenOptions="Indexer, AddMethod, RemoveMethod, GetItemMethods">
      <itemType>
        <configurationElementMoniker name="/e74336ce-2dd7-4a63-8ea8-7c910deac140/Tracer" />
      </itemType>
    </configurationElementCollection>
    <configurationElement name="Tracer" namespace="AvePoint.GCommon.Configurations">
      <attributeProperties>
        <attributeProperty name="ProcessName" isRequired="true" isKey="true" isDefaultCollection="false" xmlName="processName" isReadOnly="true">
          <type>
            <externalTypeMoniker name="/e74336ce-2dd7-4a63-8ea8-7c910deac140/String" />
          </type>
        </attributeProperty>
        <attributeProperty name="IsEnabled" isRequired="false" isKey="false" isDefaultCollection="false" xmlName="isEnabled" isReadOnly="false">
          <type>
            <externalTypeMoniker name="/e74336ce-2dd7-4a63-8ea8-7c910deac140/Boolean" />
          </type>
        </attributeProperty>
      </attributeProperties>
    </configurationElement>
    <configurationSection name="DocAveConfigurationSectionHandler" codeGenOptions="Singleton, XmlnsProperty" xmlSectionName="docave">
      <elementProperties>
        <elementProperty name="Agent" isRequired="false" isKey="false" isDefaultCollection="false" xmlName="agent" isReadOnly="false">
          <type>
            <configurationElementMoniker name="/e74336ce-2dd7-4a63-8ea8-7c910deac140/Agent" />
          </type>
        </elementProperty>
        <elementProperty name="Media" isRequired="false" isKey="false" isDefaultCollection="false" xmlName="media" isReadOnly="false">
          <type>
            <configurationElementMoniker name="/e74336ce-2dd7-4a63-8ea8-7c910deac140/Media" />
          </type>
        </elementProperty>
        <elementProperty name="Manager" isRequired="false" isKey="false" isDefaultCollection="false" xmlName="manager" isReadOnly="false">
          <type>
            <configurationElementMoniker name="/e74336ce-2dd7-4a63-8ea8-7c910deac140/Manager" />
          </type>
        </elementProperty>
        <elementProperty name="Report" isRequired="false" isKey="false" isDefaultCollection="false" xmlName="report" isReadOnly="false">
          <type>
            <configurationElementMoniker name="/e74336ce-2dd7-4a63-8ea8-7c910deac140/Report" />
          </type>
        </elementProperty>
        <elementProperty name="Diagnostics" isRequired="false" isKey="false" isDefaultCollection="false" xmlName="diagnostics" isReadOnly="false">
          <type>
            <configurationElementMoniker name="/e74336ce-2dd7-4a63-8ea8-7c910deac140/Diagnostics" />
          </type>
        </elementProperty>
      </elementProperties>
    </configurationSection>
    <configurationElement name="Agent" namespace="AvePoint.GCommon.Configurations" />
    <configurationElement name="Media" namespace="AvePoint.GCommon.Configurations">
      <elementProperties>
        <elementProperty name="StorageService" isRequired="false" isKey="false" isDefaultCollection="false" xmlName="storageService" isReadOnly="false">
          <type>
            <configurationElementMoniker name="/e74336ce-2dd7-4a63-8ea8-7c910deac140/StorageService" />
          </type>
        </elementProperty>
      </elementProperties>
    </configurationElement>
    <configurationElement name="Manager" namespace="AvePoint.GCommon.Configurations" />
    <configurationElement name="Report" namespace="AvePoint.GCommon.Configurations" />
    <configurationElement name="Diagnostics" namespace="AvePoint.GCommon.Configurations">
      <elementProperties>
        <elementProperty name="Profiler" isRequired="false" isKey="false" isDefaultCollection="false" xmlName="profiler" isReadOnly="false">
          <type>
            <configurationElementMoniker name="/e74336ce-2dd7-4a63-8ea8-7c910deac140/Profiler" />
          </type>
        </elementProperty>
      </elementProperties>
    </configurationElement>
    <configurationElement name="StorageService" namespace="AvePoint.GCommon.Configurations">
      <elementProperties>
        <elementProperty name="HoldService" isRequired="false" isKey="false" isDefaultCollection="false" xmlName="holdService" isReadOnly="false">
          <type>
            <configurationElementMoniker name="/e74336ce-2dd7-4a63-8ea8-7c910deac140/HoldService" />
          </type>
        </elementProperty>
        <elementProperty name="ExportService" isRequired="false" isKey="false" isDefaultCollection="false" xmlName="exportService" isReadOnly="false">
          <type>
            <configurationElementMoniker name="/e74336ce-2dd7-4a63-8ea8-7c910deac140/ExportService" />
          </type>
        </elementProperty>
      </elementProperties>
    </configurationElement>
    <configurationElement name="HoldService" namespace="AvePoint.GCommon.Configurations" />
    <configurationElement name="ExportService" namespace="AvePoint.GCommon.Configurations" />
  </configurationElements>
  <propertyValidators>
    <validators />
  </propertyValidators>
</configurationSectionModel>