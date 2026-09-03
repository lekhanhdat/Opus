# /********************************************************************
# *
# *  PROPRIETARY and CONFIDENTIAL
# *
# *  This file is licensed from, and is a trade secret of:
# *
# *                   AvePoint, Inc.
# *                   525 Washington Blvd, Suite 1400
# *                   Jersey City, NJ 07310
# *                   United States of America
# *                   Telephone: +1-201-793-1111
# *                   WWW: www.avepoint.com
# *
# *  Refer to your License Agreement for restrictions on use,
# *  duplication, or disclosure.
# *
# *  RESTRICTED RIGHTS LEGEND
# *
# *  Use, duplication, or disclosure by the Government is
# *  subject to restrictions as set forth in subdivision
# *  (c)(1)(ii) of the Rights in Technical Data and Computer
# *  Software clause at DFARS 252.227-7013 (Oct. 1988) and
# *  FAR 52.227-19 (C) (June 1987).
# *
# *  Copyright © 2017-2026 AvePoint® Inc. All Rights Reserved. 
# *
# *  Unpublished - All rights reserved under the copyright laws of the United States.
# */
param(
    [Parameter(Mandatory=$true)]
    [string]$TicketNumber,
    
    [Parameter(Mandatory=$false)]
    [string]$ConfigPath = ".\.github\config\jira-config.json"
)

# Function to write colored output
function Write-ColorOutput {
    param(
        [string]$Message,
        [string]$Color = "White"
    )
    
    switch ($Color) {
        "Red" { Write-Host $Message -ForegroundColor Red }
        "Green" { Write-Host $Message -ForegroundColor Green }
        "Yellow" { Write-Host $Message -ForegroundColor Yellow }
        "Blue" { Write-Host $Message -ForegroundColor Blue }
        "Cyan" { Write-Host $Message -ForegroundColor Cyan }
        "Magenta" { Write-Host $Message -ForegroundColor Magenta }
        default { Write-Host $Message }
    }
}

# Function to check if config file exists and is valid
function Test-Configuration {
    param([string]$ConfigPath)
    
    if (-not (Test-Path $ConfigPath)) {
        Write-ColorOutput "Configuration file not found: $ConfigPath" "Red"
        return $false
    }
    
    try {
        $config = Get-Content $ConfigPath -Raw | ConvertFrom-Json
        
        if (-not $config.jiraUrl -or -not $config.apiToken) {
            Write-ColorOutput "Configuration file is missing required fields (jiraUrl, apiToken)" "Red"
            return $false
        }
        
        return $true
    }
    catch {
        Write-ColorOutput "Error reading configuration file: $($_.Exception.Message)" "Red"
        return $false
    }
}

# Function to validate ticket number format
function Test-TicketFormat {
    param([string]$TicketNumber)
    
    if ($TicketNumber -match "^[A-Z]+-\d+$") {
        return $true
    }
    
    Write-ColorOutput "Invalid ticket number format: $TicketNumber" "Red"
    Write-ColorOutput "Expected format: PROJECT-NUMBER (e.g., RECO-12345)" "Yellow"
    return $false
}

# Function to fetch JIRA issue data with expanded fields
function Get-JiraIssue {
    param(
        [string]$JiraUrl,
        [string]$ApiToken,
        [string]$TicketNumber
    )
    
    try {
        # Prepare authentication headers
        $headers = @{
            "Authorization" = "Bearer $ApiToken"
            "Content-Type"  = "application/json"
            "Accept"        = "application/json"
        }
        
        # Make API call to fetch issue data with expanded fields including custom fields
        # Request additional fields that might contain Steps to reproduce, Expected Result, Actual Result
        $fields = "summary,description,status,priority,reporter,assignee,created,updated,components,labels,comment,customfield_10010,customfield_10011,customfield_10012,customfield_*"
        $url = "$JiraUrl/rest/api/2/issue/$TicketNumber" + "?fields=$fields&expand=renderedFields"
        
        Write-ColorOutput "Fetching JIRA issue: $TicketNumber" "Blue"
        Write-ColorOutput "API URL: $url" "Cyan"
        
        $issueData = Invoke-RestMethod -Uri $url -Method Get -Headers $headers -TimeoutSec 30
        
        Write-ColorOutput "Successfully fetched JIRA issue data!" "Green"
        return $issueData
    }
    catch {
        Write-ColorOutput "Error fetching JIRA issue: $($_.Exception.Message)" "Red"
        
        if ($_.Exception.Response) {
            $statusCode = $_.Exception.Response.StatusCode.Value__
            Write-ColorOutput "HTTP Status Code: $statusCode" "Red"
            
            switch ($statusCode) {
                401 { Write-ColorOutput "Authentication failed. Check your API token." "Yellow" }
                403 { Write-ColorOutput "Permission denied. You may not have permission to view this ticket." "Yellow" }
                404 { Write-ColorOutput "Ticket not found. Check the ticket number and ensure it exists." "Yellow" }
                default { Write-ColorOutput "Unexpected error occurred." "Yellow" }
            }
        }
        
        return $null
    }
}

# Function to extract steps, expected result, actual result from description or custom fields
function Get-TestingFields {
    param([object]$IssueData)
    
    $stepsToReproduce = ""
    $expectedResult = ""
    $actualResult = ""
    
    # First try to extract from description
    if ($IssueData.fields.description) {
        $description = $IssueData.fields.description
        
        # Look for common patterns in description
        if ($description -match "(?i)steps?\s*to\s*reproduce?:?\s*(.+?)(?=expected|actual|$)") {
            $stepsToReproduce = $matches[1].Trim()
        } elseif ($description -match "(?i)reproduction\s*steps?:?\s*(.+?)(?=expected|actual|$)") {
            $stepsToReproduce = $matches[1].Trim()
        }
        
        if ($description -match "(?i)expected\s*result:?\s*(.+?)(?=actual|$)") {
            $expectedResult = $matches[1].Trim()
        } elseif ($description -match "(?i)expected\s*behavior:?\s*(.+?)(?=actual|$)") {
            $expectedResult = $matches[1].Trim()
        }
        
        if ($description -match "(?i)actual\s*result:?\s*(.+?)$") {
            $actualResult = $matches[1].Trim()
        } elseif ($description -match "(?i)actual\s*behavior:?\s*(.+?)$") {
            $actualResult = $matches[1].Trim()
        }
    }
    
    # Check specific known custom fields first (JIRA standard fields for testing)
    # customfield_10010: Steps to Reproduce
    # customfield_10011: Expected Result  
    # customfield_10012: Actual Result
    if ($IssueData.fields.customfield_10010 -and [string]::IsNullOrWhiteSpace($stepsToReproduce)) {
        $stepsToReproduce = $IssueData.fields.customfield_10010.ToString().Trim()
    }
    
    if ($IssueData.fields.customfield_10011 -and [string]::IsNullOrWhiteSpace($expectedResult)) {
        $expectedResult = $IssueData.fields.customfield_10011.ToString().Trim()
    }
    
    if ($IssueData.fields.customfield_10012 -and [string]::IsNullOrWhiteSpace($actualResult)) {
        $actualResult = $IssueData.fields.customfield_10012.ToString().Trim()
    }
    
    # Then check other custom fields for these values using pattern matching
    $customFields = $IssueData.fields.PSObject.Properties | Where-Object { $_.Name -like "customfield_*" }
    
    foreach ($field in $customFields) {
        $value = $field.Value
        if ($value) {
            $fieldName = $field.Name
            
            # Try to identify the field by common naming patterns
            if ($value -is [string]) {
                if ($fieldName -match "(?i)(steps|reproduce)" -and -not $stepsToReproduce) {
                    $stepsToReproduce = $value.Trim()
                } elseif ($fieldName -match "(?i)expected" -and -not $expectedResult) {
                    $expectedResult = $value.Trim()
                } elseif ($fieldName -match "(?i)actual" -and -not $actualResult) {
                    $actualResult = $value.Trim()
                }
            }
        }
    }
    
    return @{
        StepsToReproduce = $stepsToReproduce
        ExpectedResult = $expectedResult
        ActualResult = $actualResult
    }
}

# Function to extract and display issue information
function Format-IssueData {
    param([object]$IssueData)
    
    if (-not $IssueData) {
        return
    }
    
    # Extract testing fields
    $testingFields = Get-TestingFields -IssueData $IssueData
    
    Write-ColorOutput "`n=== JIRA Issue Details ===" "Cyan"
    Write-ColorOutput "Ticket: $($IssueData.key)" "Blue"
    Write-ColorOutput "Summary: $($IssueData.fields.summary)" "White"
    Write-ColorOutput "Status: $($IssueData.fields.status.name)" "Green"
    Write-ColorOutput "Priority: $($IssueData.fields.priority.name)" "Yellow"
    Write-ColorOutput "Reporter: $($IssueData.fields.reporter.displayName)" "White"
    
    if ($IssueData.fields.assignee) {
        Write-ColorOutput "Assignee: $($IssueData.fields.assignee.displayName)" "White"
    } else {
        Write-ColorOutput "Assignee: Unassigned" "White"
    }
    
    Write-ColorOutput "Created: $($IssueData.fields.created)" "White"
    Write-ColorOutput "Updated: $($IssueData.fields.updated)" "White"
    
    if ($IssueData.fields.components -and $IssueData.fields.components.Count -gt 0) {
        $components = $IssueData.fields.components | ForEach-Object { $_.name }
        Write-ColorOutput "Components: $($components -join ', ')" "White"
    }
    
    if ($IssueData.fields.labels -and $IssueData.fields.labels.Count -gt 0) {
        Write-ColorOutput "Labels: $($IssueData.fields.labels -join ', ')" "White"
    }
    
    Write-ColorOutput "`n=== Description ===" "Cyan"
    if ($IssueData.fields.description) {
        Write-Host $IssueData.fields.description
    } else {
        Write-ColorOutput "No description provided" "Yellow"
    }
    
    # Display testing fields
    if ($testingFields.StepsToReproduce) {
        Write-ColorOutput "`n=== Steps to Reproduce ===" "Cyan"
        Write-Host $testingFields.StepsToReproduce
    }
    
    if ($testingFields.ExpectedResult) {
        Write-ColorOutput "`n=== Expected Result ===" "Cyan"
        Write-Host $testingFields.ExpectedResult
    }
    
    if ($testingFields.ActualResult) {
        Write-ColorOutput "`n=== Actual Result ===" "Cyan"
        Write-Host $testingFields.ActualResult
    }
    
    if ($IssueData.fields.comment.comments -and $IssueData.fields.comment.comments.Count -gt 0) {
        Write-ColorOutput "`n=== Comments ===" "Cyan"
        foreach ($comment in $IssueData.fields.comment.comments) {
            Write-ColorOutput "`nComment by $($comment.author.displayName) on $($comment.created):" "Yellow"
            Write-Host $comment.body
        }
    }
    
    # Return structured data for further analysis
    return @{
        key = $IssueData.key
        summary = $IssueData.fields.summary
        description = $IssueData.fields.description
        stepsToReproduce = $testingFields.StepsToReproduce
        expectedResult = $testingFields.ExpectedResult
        actualResult = $testingFields.ActualResult
        status = $IssueData.fields.status.name
        priority = $IssueData.fields.priority.name
        reporter = $IssueData.fields.reporter.displayName
        assignee = if ($IssueData.fields.assignee) { $IssueData.fields.assignee.displayName } else { "Unassigned" }
        created = $IssueData.fields.created
        updated = $IssueData.fields.updated
        components = if ($IssueData.fields.components) { $IssueData.fields.components | ForEach-Object { $_.name } } else { @() }
        labels = if ($IssueData.fields.labels) { $IssueData.fields.labels } else { @() }
        comments = if ($IssueData.fields.comment.comments) { 
            $IssueData.fields.comment.comments | ForEach-Object {
                @{
                    author = $_.author.displayName
                    created = $_.created
                    body = $_.body
                }
            }
        } else { @() }
        customFields = @{
            customfield_10010 = if ($IssueData.fields.customfield_10010) { $IssueData.fields.customfield_10010.ToString() } else { $null }
            customfield_10011 = if ($IssueData.fields.customfield_10011) { $IssueData.fields.customfield_10011.ToString() } else { $null }
            customfield_10012 = if ($IssueData.fields.customfield_10012) { $IssueData.fields.customfield_10012.ToString() } else { $null }
        }
    }
}

# Main execution
Write-ColorOutput "JIRA Issue Fetcher" "Cyan"
Write-ColorOutput "==================" "Cyan"

# Validate inputs
if (-not (Test-TicketFormat $TicketNumber)) {
    exit 1
}

if (-not (Test-Configuration $ConfigPath)) {
    exit 1
}

# Load configuration
try {
    $config = Get-Content $ConfigPath -Raw | ConvertFrom-Json
    
    # Use environment variables if available
    $jiraUrl = if ($env:JIRA_URL) { $env:JIRA_URL } else { $config.jiraUrl }
    $apiToken = if ($env:JIRA_API_TOKEN) { $env:JIRA_API_TOKEN } else { $config.apiToken }
    
    Write-ColorOutput "Configuration loaded successfully" "Green"
    Write-ColorOutput "JIRA URL: $jiraUrl" "Blue"
}
catch {
    Write-ColorOutput "Error loading configuration: $($_.Exception.Message)" "Red"
    exit 1
}

# Fetch the JIRA issue
$issueData = Get-JiraIssue -JiraUrl $jiraUrl -ApiToken $apiToken -TicketNumber $TicketNumber

if ($issueData) {
    $structuredData = Format-IssueData -IssueData $issueData
    
    # Return structured data to pipeline for programmatic access (no file output)
    Write-ColorOutput "`nStructured data ready for analysis. Use the return object for further processing." "Green"
    Write-ColorOutput "To access the data in scripts, capture the output: `$data = .\\fetch-jira-issue.ps1 -TicketNumber 'RECO-12345'" "Blue"
    
    # Output structured data to pipeline
    return $structuredData
} else {
    Write-ColorOutput "Failed to fetch JIRA issue data." "Red"
    exit 1
}