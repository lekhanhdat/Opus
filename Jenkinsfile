// Basic Info
def productName = "Cloud Records AKS"
def productKey = "RECO_AKS"


def HotfixFileVersion = params.HotFixVersion


def depEnv = "${params.DeployTo}"
def isDeploy = depEnv != "None"

// Version
def displayVersion = params.displayVersion
def productVersion = displayVersion.replace(".","")
def internalVersion = productVersion.replace(" ","")
def fileVersion = VersionNumber projectStartDate: "", versionNumberString: '${BUILDS_ALL_TIME,XXX}', versionPrefix: params.versionPrefix//, overrideBuildsAllTime: "1"
def jiraFixVersion = params.jiraFixVersion
def imageVersion = internalVersion.toLowerCase()



// Git
def gitRepo = "https://git.avepoint.net/bunty/reco.git"
//https://git.avepoint.net/bunty/reco/tree/master
def gitURL = "https://git.avepoint.net/bunty/reco"
def branchName = params.buildBranchName
def branchURL = "${gitURL}/tree/${branchName}"
def buildToolPath = "build\\tool\\build"
def deployToolPath = "build\\Tool\\Deploy"
def i18nToolURL = "${g.SVNURL}/client/Misc/trunk/AveInternationalizationTools/Tools/ResourceGenerater"
//https://git.avepoint.net/bunty/reco/-/tags/DailyTag_GITFeb2020_2019-12-02_23-07-30
//def tagURL = "https://10.1.0.130/reco/tags/DailyTag/${internalVersion}/${BUILD_TIMESTAMP}"
def isHotfixBranch = branchName.contains("_Update_Branch")

if(isHotfixBranch){
    if(HotfixFileVersion == null || HotfixFileVersion.isEmpty()){
        error("This is Hotfix Job, HotFixVersion field is mandatory!!")
    }
}

def tagName = "DailyTag_${internalVersion}_${BUILD_TIMESTAMP}"
if(isHotfixBranch){
    tagName = "UpdateTag_${internalVersion}_${BUILD_TIMESTAMP}"
}
def tagURL = "${gitURL}/-/tags/${tagName}"
def createTagMessage = "[PANDA-6360] Create build tag and update BinaryList.db"


// NuGet
def nugetSrc = "https://proget.avepoint.net/nuget/NuGet.org;https://proget.avepoint.net/nuget/AvePoint"


// Env & Workspace
def buildAgentIP = "DLAgent_1_Cloud"
def deployAgentIP = "DLAgent_1_Cloud"
def workspaceDir = "C:\\${productKey}\\Git_${internalVersion}"
if(isHotfixBranch){
    workspaceDir = "C:\\${productKey}\\Git_CI_${internalVersion}"
}
def toolRelativePath = "Tool"
def toolPath = "${workspaceDir}\\${toolRelativePath}"
def codeRelativePath = "SourceCode"
def codePath = "${workspaceDir}\\${codeRelativePath}"
def i18nToolRelativePath = "I18NTool"
def i18nToolPath = "${workspaceDir}\\${i18nToolRelativePath}"
def deployPackageRelativePath = "Package"
def deployPackagePath = "${workspaceDir}\\${deployPackageRelativePath}"
def reportRelativePath = "Reports"
def reportPath = "${workspaceDir}\\${reportRelativePath}"
def gitexe = "C:\\\\Program Files\\\\Git\\\\bin\\\\git.exe"


// JIRA
def jiraSite = "jiraprod"
def jql = "project = RECO AND status in (Resolved, \"Resolved by I18N\") AND resolution = Fixed AND fixVersion = \"${jiraFixVersion}\""
def jiraIssues = null

// FTP
def ccftpURL = "${g.CCFTP}/CloudRecords/DailyBuild/${internalVersion}/${BUILD_TIMESTAMP}"
def dlftpURL = "${g.DLFTP}/CloudRecords/DailyBuild/${internalVersion}/${BUILD_TIMESTAMP}"
def FTPModulePath = "${toolPath}\\PSModules\\PSFTP\\PSFTP.psm1"


// Test Env
def AzureModulePath = "${toolPath}\\PSModules\\Azure\\AzureDeploy.psm1"
def PublishSettingsFilePath="C:\\PublishSettings\\${productKey}.publishsettings"
def SubscriptionId="7709d19e-4689-469e-adb0-390fb5a9014e"
def storageName = "recotemppackage"
def rdpUsername = "cloudrecords"
def rdpPassword = 'REC0Passw0rd!'
def rdpCert = "958B6B5093959E9B3886BC19D5B10A19C417973D"

def webName = "RECOQATestWeb"
def webPackage = "${deployPackagePath}\\CloudRecordsWeb.cspkg"
def webConfig = "${toolPath}\\CSCFG\\${webName}.cscfg"

def agentName = "RECOQATestAgent"
def agentPackage = "${deployPackagePath}\\CloudRecordsAgent.cspkg"
def agentConfig = "${toolPath}\\CSCFG\\${agentName}.cscfg"

def appWebName = "RECOQATestAppWeb"
def appWebPackage = "${deployPackagePath}\\CloudRecordsAppWeb.cspkg"
def appWebConfig = "${toolPath}\\CSCFG\\${appWebName}.cscfg"


def perfwebName = "RECOPerformanceWeb"
def perfwebPackage = "${deployPackagePath}\\CloudRecordsWeb.cspkg"
def perfwebConfig = "${toolPath}\\CSCFG\\${perfwebName}.cscfg"

def perfagentName = "RECOPerformanceAgent"
def perfagentPackage = "${deployPackagePath}\\CloudRecordsAgent.cspkg"
def perfagentConfig = "${toolPath}\\CSCFG\\${perfagentName}.cscfg"



def slot = "Production"


// Email
def successEmailSubject = "[${productName}] ${currentBuild.rawBuild.project.displayName} #${BUILD_ID} finished successfully!"
def failedEmailSubject = "[${productName}] ${currentBuild.rawBuild.project.displayName} #${BUILD_ID} failed, please fix it as soon as possible!"
def emailContent = "Please refer to ${BUILD_URL} for more details."
def emailRecipientList = "leon.chen@avepoint.com,fpwang@avepoint.com,maggie.wang@avepoint.com,yanlong.gu@avepoint.com,Guannan.Wu@avepoint.com"


// Update
def buildDate = BUILD_TIMESTAMP.split("_")[0].replace("-","")
if(isHotfixBranch)
{
    // Update Tag
    tagURL = "${g.SVNURL}/reco/tags/Update/${internalVersion}/${BUILD_TIMESTAMP}"
    tagName = "UpdateTag_${internalVersion}_${BUILD_TIMESTAMP}"
    
    // Update FTP
    ccFtpURL = "${g.CCFTP}/CloudRecords/Update/${internalVersion}/${BUILD_TIMESTAMP}"
    dlFtpURL = "${g.DLFTP}/CloudRecords/Update/${internalVersion}/${BUILD_TIMESTAMP}"
    
    // Update JIRA
    jql = "project = CI AND issuetype = \"Hotfix Management\" AND status = Resolved AND \"SVN Branch\" = \"${branchURL}\""
}


pipeline {
    agent none
    
    environment {
        svnCred = credentials("${g.CRED_SVN}")
        ftpCred = credentials("${g.CRED_FTP}")
    }

    stages {

        stage("# Pre-Build") {
        
          steps {
              script{
        
                  //获取需要Ready for QA的JIRA Issues
                  def searchResult = jiraJqlSearch jql: "${jql}", site: "${jiraSite}"
                  jiraIssues = searchResult.data.issues
        
              }
          }
        }

        stage('# Build') {
            
            agent {
                node {
                    label "${buildAgentIP}"
                    customWorkspace "${workspaceDir}"
                }
            }

            steps {
            
                script 
                {



                        cops()
                        
                        
                        //判断下当前的Workspace是否为@2等，如果是则退出Build解决。
                        if("${workspace}".contains("@"))
                        {
                            error("The current workspace is not ${workspaceDir}.")
                        }
                        
                        
                        if (isHotfixBranch)
                        {
                          if (jiraIssues.size() != 1)
                          {
                              error("The status of HM is not 'Resolved'.")
                          } else {
                              def issue = jiraGetIssue idOrKey: jiraIssues[0].key, site: "${jiraSite}"
                              fileVersion = issue.data.fields.customfield_10084
                          }
                                fileVersion = "${HotfixFileVersion}"
                        }
                       
                        
                        // 更新国际化资源文件
                        //co("${g.CRED_SVN}",["${i18nToolRelativePath}":"${i18nToolURL}@HEAD"])
                        //i18n("${gitURL}")
                        
                        // 清理旧文件
                        bat("""
                        if exist ${workspace}\\CloudRecords rd /s /q ${workspace}\\CloudRecords
                        if exist ${workspace}\\Package rd /s /q ${workspace}\\Package
                        if exist ${workspace}\\Reports rd /s /q ${workspace}\\Reports
                        if exist ${workspace}\\${toolRelativePath} rd /s /q ${workspace}\\${toolRelativePath}
                        """)

                        
                        // 更新代码
                        // co("${g.CRED_SVN}",["${toolRelativePath}":"${buildToolURL}@HEAD",
                        //                  "${codeRelativePath}":"${branchURL}@HEAD"])
                        // checkout([$class: 'GitSCM', branches: [[name: '*/master']], doGenerateSubmoduleConfigurations: false, extensions: [[$class: 'RelativeTargetDirectory', relativeTargetDir: 'SourceCode']], gitTool: 'jgit', submoduleCfg: [], userRemoteConfigs: [[credentialsId: 'ssh-git-yksun', url: 'git@git.avepoint.net:bunty/reco.git']]])
                        //bat("""
                        //if exist ${workspace}\\SourceCode rd /s /q ${workspace}\\SourceCode
                        //""")
                        //bat label: '', script: '"${gitexe}" clone git@git.avepoint.net:bunty/reco.git SourceCode'

                        bat ("""
                                if not exist ${codePath}\\.git (
                                    rd /s /q ${codePath}
                                    "${gitexe}" clone ${gitRepo} ${codePath}  
                                )
                            """)

                        dir("${codePath}\\") {
                        bat("""
                            "${gitexe}" clean -f
                            "${gitexe}" checkout ${branchName} -f
                            "${gitexe}" fetch --all
                            "${gitexe}" reset --hard origin/${branchName}
                            """)
                        //bat label: '', script: '"${gitexe}" clean -f '
                        //bat label: '', script: '"${gitexe}" checkout ${branchName} -f '
                        }
                        copy from:"${codePath}\\${buildToolPath}\\*", to:"${workspaceDir}\\${toolRelativePath}\\"

                        
                        
                        // 如果非Hotfix Branch, 更新BinaryList.db
                        /*
                        if(!isHotfixBranch){
                            bat ("${toolPath}\\CIE\\ContinuousIntegrationExtendedTool.exe -sbl ${workspace}")
                            copy from: "${workspaceDir}\\${toolRelativePath}\\BinaryList.db", to:"${codePath}\\${buildToolPath}\\"

                            dir("${codePath}\\${buildToolPath}\\") {
                                bat("""
                                    "${gitexe}" add BinaryList.db
                                    """)
                                //bat label: '', script: '"${gitexe}" add BinaryList.db'
                                try {
                                    bat("""
                                    "${gitexe}" commit -m "${createTagMessage}"
                                    """)
                                    
                                } catch (err) {
                                    println(err) 
                                }
                                
                                
                                bat("""
                                "${gitexe}" push
                                """)
                            }

                            //ci("${toolPath}\\BinaryList.db","${createTagMessage}","${svnCred_USR}","${svnCred_PSW}")
                            
                        }
                        */
                        dir("${workspaceDir}"){
                        // 修改文件Version和ServiceVersion
                        cv([version:"${fileVersion}",cs:"SourceCode",cpp:"SourceCode"])
                        cvxml([version:"${fileVersion}",xml:[[name: "ServiceVersion.config", path: "SourceCode\\RACommon\\ServiceVersion", xpath:"standards/version"]]])
                        cvxml([version:"\"${displayVersion}\"",xml:[[name: "ServiceVersion.config", path: "SourceCode\\RACommon\\ServiceVersion", xpath:"standards/DisplayVersion"]]])
                        }
                        
                        // Build & Copy
                        // 如果Build客户问题，将Hotfix包中文件Copy出来

                        //bat("PowerShell ${toolPath}\\RestoreNuGetPackages.ps1 -nugetPath ${toolPath}\\NuGet.exe -codePath ${codePath} -restorePath ${codePath}\\packages")
                        msbd(16,"${codePath}\\build\\Build.xml","/t:MainDeploy /p:BuildType=Release;BuildTarget=Rebuild;Platform=\"Any CPU\"")
                        msbd(16,"${codePath}\\build\\CopyBinaryList.xml","/t:RevIMMainTarget")
                        
						// Build container
						bat "PowerShell ${codePath}\\build\\BuildImage.ps1 -service timer  -version ${imageVersion} -buildId ${BUILD_ID} -buildPath ${codePath}\\build -buildFolder RevIMWorker.Timer"
						bat "PowerShell ${codePath}\\build\\BuildImage.ps1 -service web    -version ${imageVersion} -buildId ${BUILD_ID} -buildPath ${codePath}\\build -buildFolder RevIMWorker.Web"
			            bat "PowerShell ${codePath}\\build\\BuildImage.ps1 -service worker -version ${imageVersion} -buildId ${BUILD_ID} -buildPath ${codePath}\\build -buildFolder RevIMWorker.ScheduleJob -dockerfile Dockerfile.Worker"
						bat "PowerShell ${codePath}\\build\\BuildImage.ps1 -service appweb -version ${imageVersion} -buildId ${BUILD_ID} -buildPath ${codePath}\\build -buildFolder RevIMWorker.ProviderWeb"
						bat "PowerShell ${codePath}\\build\\BuildImage.ps1 -service loginweb -version ${imageVersion} -buildId ${BUILD_ID} -buildPath ${codePath}\\build -buildFolder RevIMWorker.LoginWeb"
						bat "PowerShell ${codePath}\\build\\BuildImage.ps1 -service job    -version ${imageVersion} -buildId ${BUILD_ID} -buildPath ${codePath}\\build -buildFolder RevIMWorker.ScheduleJob"

                        if(isHotfixBranch)
                        {
                            msbd(14,"${codePath}\\build\\CopyCIBinaryList.xml","/t:CopyFiles")
                        }


                        if(isHotfixBranch)
                        {
                            zi configs:[from:"CloudRecords\\Hotfix\\*", to: "Package\\CloudRecords_Hotfix_${fileVersion}_${buildDate}.zip"]
                            //zi configs:[from:"Package\\*", to: "Package\\CloudRecords_${fileVersion}_${buildDate}.zip"]
                        }
                        
                        // generate md5
                        //bat "PowerShell ${toolPath}\\generate-build-label.ps1 -packagepath ${workspace}\\Package -timestamp ${BUILD_TIMESTAMP}-extension '*.cspkg'"
                        /*
                        if(isHotfixBranch)
                        {
                            triggerRemoteJob auth: CredentialsAuth(credentials: "${g.CRED_SVN}"), blockBuildUntilComplete: false, job: 'upload/upload', maxConn: 1, parameters: """product=reco ftpurl=${dlftpURL}""", remoteJenkinsUrl: 'http://10.2.30.52', shouldNotFailBuild: true, useCrumbCache: true, useJobInfoCache: true
                        }*/
                    
                        // 创建Tag
                        // tag([src:"${codePath}",msg:"${createTagMessage}",tag:"${tagURL}",usr:"${svnCred_USR}",psw:"${svnCred_PSW}"])
                        /*
                        dir("${codePath}"){
                        
                            bat("""
                                "${gitexe}" tag -a ${tagName} -m "${createTagMessage}"
                                "${gitexe}" push origin ${tagName}
                                """)
                        
                        }
                        
                        */
                        
                        // 检查安装包中的文件，并发布检查结果
                        bat """
                        if exist ${workspace}\\CloudRecords\\CloudRecordsAgent rd /s /q ${workspace}\\CloudRecords\\CloudRecordsAgent
                        move /y ${codePath}\\build\\RevIMWorker.ScheduleJob ${workspace}\\CloudRecords\\
                        move /y ${codePath}\\build\\RevIMWorker.Timer ${workspace}\\CloudRecords\\
                        move /y ${codePath}\\build\\RevIMWorker.Web ${workspace}\\CloudRecords\\
                        move /y ${codePath}\\build\\RevIMWorker.ProviderWeb ${workspace}\\CloudRecords\\
                        move /y ${codePath}\\build\\RevIMWorker.LoginWeb ${workspace}\\CloudRecords\\
                        """


                        pkgck("${toolPath}\\FilesChecker","${toolPath}\\FilesChecker\\FilesChecker.ckproj -pi ${workspace}") // 检查包完整性
                        pkgck("${toolPath}\\FilesChecker","${toolPath}\\FilesChecker\\FilesChecker.ckproj -ds ${workspace}") // 检查PowerShell脚本是否包含数字签名
                        pkgck("${toolPath}\\FilesChecker","${toolPath}\\FilesChecker\\FilesChecker.ckproj -cm ${workspace}") // 检查包中文件是否为Release文件
                    
                        xmltohtml("${reportPath}\\PackageFilesCheckerConsole.exe.xml", "${toolPath}\\Reports\\BuildLog.xsl", "${reportPath}\\PackageBuildLog.html")
                        xmltohtml("${reportPath}\\report.xml", "${toolPath}\\Reports\\CheckReport.xsl", "${reportPath}\\PackageCheckReport.html")
                        
                        publishHTML([allowMissing: true, alwaysLinkToLastBuild: false, keepAll: true, reportDir: "${reportRelativePath}", reportFiles: 'PackageCheckReport.html', reportName: 'Package Check Report', reportTitles: ''])
                        publishHTML([allowMissing: true, alwaysLinkToLastBuild: false, keepAll: true, reportDir: "${reportRelativePath}", reportFiles: 'PackageBuildLog.html', reportName: 'Package Build Log', reportTitles: ''])
                      
   
                        // 发布FTP地址
                        rtp abortedAsStable: false, failedAsStable: false, nullAction: '1', parserName: 'Confluence', stableText: """
                            h2. Download URL:
                                CC FTP: ${ccftpURL}
                                DL FTP: ${dlftpURL}
                            h2. Tag URL:
                                ${tagURL}
                        """, unstableAsStable: false
    
                        
                        // Ready for QA
                        //if(!isHotfixBranch)
                        //{
                        //  //bat "PowerShell ${toolPath}\\ready4qa\\ready4qa.ps1"
                        //  //bat ("${toolPath}\\ready4qa\\ContinuousIntegrationExtendedTool.exe  -cjt ${toolPath}\\ready4qa\\ ReadyforQA filter true")
                        //  cie ("-cjt . ReadyforQA filter true")
                        //}

                        // Ready for QA
                        // 如果Build客户问题，先修改CC FTP URL和DL FTP URL
                        if(isHotfixBranch)
                        {
                          def jiraFTPURL = [fields: [customfield_11492: "${ccftpURL}",
                                                     customfield_11493: "${dlftpURL}"]]
                            def response = jiraEditIssue idOrKey: jiraIssues[0].key, issue: jiraFTPURL, site: "${jiraSite}"
                          if(!response.successful)
                          {
                              error("Failed to update FTP URL.")
                          }
                        } 
                        
                        for (int i = 0; i < jiraIssues.size(); i++) {
                          def issue = jiraIssues[i]
                          def result = jiraGetIssueTransitions idOrKey: "${issue.key}", site: "${jiraSite}"
                          def transitions =  result.data.transitions
                          def canBeReadyForQA = false
                          def transiontionId = 0
                          for (transition in transitions) {
                              if (transition.name.trim() == "Ready for QA") {
                                  canBeReadyForQA = true
                                  transiontionId = transition.id
                                  break
                              }
                          }
                          if(!canBeReadyForQA) {
                              warn("Warning: The status of ${issue.key} cannot be changed to \"Ready for QA\" ") 
                          } else {
                              def transitionInput = ["transition": ["id": transiontionId]]
                              jiraTransitionIssue idOrKey: "${issue.key}", input: transitionInput, site: "${jiraSite}"
                          }
                          info("[Report][FixedIssue] https://jira.avepoint.net/browse/${issue.key} (${issue.fields.summary})")
                        }
						
                }
				echo "=====Start Sonar_Dependency-Check===="
						build job: 'Sonar_Dependency-Check', wait: false
            }
        }

                

        stage('# Deploy') {
            
            agent {
                node {
                    label "${deployAgentIP}"
                    customWorkspace "${workspaceDir}"
                }
            }
            
            steps {
                parallel Deploy: {
                    script {

                        if(isDeploy) {

                            def dpmPrefix = "reco"
							if(depEnv != "Test") {
								dpmPrefix = "reco${depEnv}".toLowerCase()
							}
                            bat "PowerShell ${codePath}\\build\\DeployContainer.ps1 -dpmPrefix ${dpmPrefix} -service web    -version ${imageVersion} -buildId ${BUILD_ID}"
                            bat "PowerShell ${codePath}\\build\\DeployContainer.ps1 -dpmPrefix ${dpmPrefix} -service timer  -version ${imageVersion} -buildId ${BUILD_ID}"
                            bat "PowerShell ${codePath}\\build\\DeployContainer.ps1 -dpmPrefix ${dpmPrefix} -service appweb -version ${imageVersion} -buildId ${BUILD_ID}"
                            bat "PowerShell ${codePath}\\build\\DeployContainer.ps1 -dpmPrefix ${dpmPrefix} -service loginweb -version ${imageVersion} -buildId ${BUILD_ID}"
                            bat "PowerShell ${codePath}\\build\\DeployContainer.ps1 -dpmPrefix ${dpmPrefix} -service worker -version ${imageVersion} -buildId ${BUILD_ID}"
                            bat "PowerShell ${codePath}\\build\\UpdateWorker.ps1    -dpmPrefix ${dpmPrefix} -service default -version ${imageVersion} -buildId ${BUILD_ID}"
                        }
                    }
                }
            }
        }
        
    }
    
}