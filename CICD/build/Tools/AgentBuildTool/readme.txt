Tool所在位置： \CICD\build\Tools\AgentBuildTool
Tool源码位置： \DEVTools\AgentBuildTool


Tool的使用方法：
1. Agent.sln solution切换到Release模式，重新build一次
2. 按照要求修改好配置文件 （详情参照下面配置文件的介绍）
3. 以管理员身份Run AgentBuildTool.exe (revert Product.wxs、signname.txt、IncludeInPackage.xml before run)
4. 提交Code时，检查 Product.wxs、signname.txt、IncludeInPackage.xml 这三个配置文件是否正确


AgentBuildTool.exe.config
MajorVersionBuild 
  => true： Agent包里有重大改动，涉及删除文件时，Agent需要Build Major Version，执行Tool以后，Product.wxs里的Product Id会变，出包以后只有.msi安装包，需要让DevOps帮忙更新.wixpdb文件；
  => false (不为true)： 这个为默认值，Agent包没有删除文件的需要，出包后，会生成用于Upgrade的.msp安装包；
package_agent_wxs => Product.wxs 文件的位置
agent_lic_path => Agent包里license文件所在的位置
agentbin_output => Agent.sln solution在build以后的生产目录
package_dlls_signname => signname.txt文件的位置，此文件用于配置需要做签名的dll，当有新增自定义Dll时，执行Tool以后，会自动修改此文件，提交code时需要检查下是否正确
package_dlls_obfuscate => IncludeInPackage.xml文件的位置，此文件用于配置需要做混淆的dll，当有新增自定义Dll时，执行Tool以后，会自动修改此文件，提交code时需要检查下是否正确
ConfigurationToolName => CloudAgentConfigurationTool.exe文件的名字

WXSExcludeConfig.json
IncludeFiles => 配置必须要包含到Agent包的文件，优先级最高
ExcludeFolders => 配置不需要包含到Agent包中的文件夹
ExcludeFileNames => 配置不需要包含到Agent包中的文件
ExcludeFileNameRegexes => 配置不需要包含到Agent包中的文件，以正则表达式的方式通配文件名
ThirdDlls => 配置agentbin_output路径下，所有第三方Dll，主要在查找自定义dll时用于排除第三方dll
PS: 当多个文件夹里有同名的文件夹或者文件，并且想要区分配置时，可以配置带部分Path的文件或文件夹名（除了ThirdDlls），例如： ThirdDlls\\log4net.dll
