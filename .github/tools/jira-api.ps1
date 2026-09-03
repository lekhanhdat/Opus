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
    
    [Parameter(Mandatory=$true)]
    [string]$RootCause,
    
    [Parameter(Mandatory=$true)]
    [string]$Solution,
    
    [Parameter(Mandatory=$true)]
    [string]$TestingRecommendations,
    
    [Parameter(Mandatory=$false)]
    [string]$CommitHash = "",
    
    [Parameter(Mandatory=$false)]
    [string]$FilesChanged = "",
    
    [Parameter(Mandatory=$false)]
    [string]$LinesModified = "",
    
    [Parameter(Mandatory=$false)]
    [string]$ConfigPath = ".\.github\config\jira-config.json",
    
    [Parameter(Mandatory = $false)]
    [switch]$AutoConfirm = $false
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
        Write-ColorOutput "Please create the configuration file:" "Yellow"
        Write-ColorOutput "  1. Copy-Item .\.github\config\jira-config.template.json .\.github\config\jira-config.json" "Cyan"
        Write-ColorOutput "  2. Edit the file with your JIRA credentials" "Cyan"
        return $false
    }
    
    try {
        $config = Get-Content $ConfigPath -Raw | ConvertFrom-Json
        
        if (-not $config.jiraUrl -or -not $config.apiToken) {
            Write-ColorOutput "Configuration file is missing required fields (jiraUrl, apiToken)" "Red"
            Write-ColorOutput "Note: For local JIRA Server, only jiraUrl and apiToken are required" "Yellow"
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

# Function to format the comment for JIRA
function Format-JiraComment {
    param(
        [string]$RootCause,
        [string]$Solution,
        [string]$TestingRecommendations,
        [string]$CommitHash,
        [string]$FilesChanged,
        [string]$LinesModified,
        [object]$Config
    )
    
    $comment = "*Root Cause:*`n"
    $comment += $RootCause + "`n`n"
    
    $comment += "*Solution:*`n"
    $comment += $Solution + "`n`n"
    
    $comment += "*Testing Recommendations:*`n"
    $comment += $TestingRecommendations
    
    return $comment
}

# Function to post comment to JIRA
function Add-JiraComment {
    param(
        [string]$JiraUrl,
        [string]$Username,
        [string]$ApiToken,
        [string]$TicketNumber,
        [string]$Comment
    )
    
    try {
        # Prepare authentication for local JIRA Server using API Token only
        $headers = @{
            "Authorization" = "Bearer $ApiToken"
            "Content-Type"  = "application/json"
            "Accept"        = "application/json"
        }
        
        # Prepare body for local JIRA Server
        $body = @{
            body = $Comment
        } | ConvertTo-Json -Depth 3
        
        # Make API call to local JIRA Server
        $url = "$JiraUrl/rest/api/2/issue/$TicketNumber/comment"
        
        Write-ColorOutput "Posting comment to JIRA ticket: $TicketNumber" "Blue"
        Write-ColorOutput "API URL: $url" "Cyan"
        
        $response = Invoke-RestMethod -Uri $url -Method Post -Headers $headers -Body $body -TimeoutSec 30
        
        Write-ColorOutput "Successfully posted comment to JIRA ticket!" "Green"
        return $true
    }
    catch {
        Write-ColorOutput "Error posting comment to JIRA: $($_.Exception.Message)" "Red"
        
        if ($_.Exception.Response) {
            $statusCode = $_.Exception.Response.StatusCode.Value__
            Write-ColorOutput "HTTP Status Code: $statusCode" "Red"
            
            # Read response content for more details
            try {
                $responseStream = $_.Exception.Response.GetResponseStream()
                $reader = New-Object System.IO.StreamReader($responseStream)
                $responseBody = $reader.ReadToEnd()
                Write-ColorOutput "Response: $responseBody" "Yellow"
            } catch { }
            
            switch ($statusCode) {
                401 { Write-ColorOutput "Authentication failed. Check your username and API token for local JIRA Server." "Yellow" }
                403 { Write-ColorOutput "Permission denied. You may not have permission to comment on this ticket." "Yellow" }
                404 { Write-ColorOutput "Ticket not found. Check the ticket number and ensure it exists in the local JIRA Server." "Yellow" }
                default { Write-ColorOutput "Unexpected error occurred." "Yellow" }
            }
        }
        
        return $false
    }
}

# Function to update ticket status
function Update-TicketStatus {
    param(
        [string]$JiraUrl,
        [string]$Username,
        [string]$ApiToken,
        [string]$TicketNumber,
        [string]$NewStatus,
        [object]$Config
    )
    
    if (-not $Config.autoStatusUpdate -or -not $NewStatus) {
        return
    }
    
    try {
        # Prepare authentication for local JIRA Server using API Token only
        $headers = @{
            "Authorization" = "Bearer $ApiToken"
            "Content-Type"  = "application/json"
            "Accept"        = "application/json"
        }
        
        # Get available transitions
        $transitionsUrl = "$JiraUrl/rest/api/2/issue/$TicketNumber/transitions"
        $transitions = Invoke-RestMethod -Uri $transitionsUrl -Method Get -Headers $headers -TimeoutSec 30
        
        # Find the transition ID for the desired status
        $transition = $transitions.transitions | Where-Object { $_.to.name -eq $NewStatus }
        
        if ($transition) {
            # Prepare transition body
            $body = @{
                transition = @{
                    id = $transition.id
                }
            } | ConvertTo-Json -Depth 3
            
            # Make transition API call
            $response = Invoke-RestMethod -Uri $transitionsUrl -Method Post -Headers $headers -Body $body -TimeoutSec 30
            
            Write-ColorOutput "Updated ticket status to: $NewStatus" "Green"
        }
        else {
            Write-ColorOutput "Status '$NewStatus' not available for this ticket" "Yellow"
        }
    }
    catch {
        Write-ColorOutput "Warning: Could not update ticket status: $($_.Exception.Message)" "Yellow"
    }
}

# Function to add labels to ticket
function Add-TicketLabels {
    param(
        [string]$JiraUrl,
        [string]$Username,
        [string]$ApiToken,
        [string]$TicketNumber,
        [array]$Labels,
        [object]$Config
    )
    
    if (-not $Config.autoLabels -or -not $Labels) {
        return
    }
    
    try {
        # Prepare authentication for local JIRA Server using API Token only
        $headers = @{
            "Authorization" = "Bearer $ApiToken"
            "Content-Type"  = "application/json"
            "Accept"        = "application/json"
        }
        
        # Get current issue to retrieve existing labels
        $issueUrl = "$JiraUrl/rest/api/2/issue/$TicketNumber"
        $issue = Invoke-RestMethod -Uri $issueUrl -Method Get -Headers $headers -TimeoutSec 30
        
        # Combine existing labels with new ones
        $existingLabels = $issue.fields.labels
        $allLabels = ($existingLabels + $Labels) | Sort-Object -Unique
        
        # Prepare update body
        $body = @{
            fields = @{
                labels = $allLabels
            }
        } | ConvertTo-Json -Depth 3
        
        # Update the issue
        $response = Invoke-RestMethod -Uri $issueUrl -Method Put -Headers $headers -Body $body -TimeoutSec 30
        
        Write-ColorOutput "Added labels: $($Labels -join ', ')" "Green"
    }
    catch {
        Write-ColorOutput "Warning: Could not add labels: $($_.Exception.Message)" "Yellow"
    }
}

# Main execution
Write-ColorOutput "JIRA API Tool" "Cyan"
Write-ColorOutput "==============" "Cyan"

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
    Write-ColorOutput "Using API Token authentication for local JIRA Server" "Blue"
}
catch {
    Write-ColorOutput "Error loading configuration: $($_.Exception.Message)" "Red"
    exit 1
}

# Format the comment
$comment = Format-JiraComment -RootCause $RootCause -Solution $Solution -TestingRecommendations $TestingRecommendations -CommitHash $CommitHash -FilesChanged $FilesChanged -LinesModified $LinesModified -Config $config

# Show preview and ask for confirmation
Write-ColorOutput "`nComment Preview:" "Yellow"
Write-ColorOutput "=================" "Yellow"
Write-Host $comment
Write-ColorOutput "`n=================" "Yellow"

if ($AutoConfirm) {
    Write-ColorOutput "`nAuto-confirm mode enabled. Posting comment to JIRA ticket $TicketNumber..." "Cyan"
    $confirmation = 'y'
}
else {
    $confirmation = Read-Host "`nDo you want to post this comment to JIRA ticket $TicketNumber? (y/N)"
}

if ($confirmation -eq 'y' -or $confirmation -eq 'Y' -or $confirmation -eq 'yes') {
    # Post the comment
    $success = Add-JiraComment -JiraUrl $jiraUrl -Username "" -ApiToken $apiToken -TicketNumber $TicketNumber -Comment $comment
    
    if ($success) {
        # Update status if configured
        if ($config.defaultStatus) {
            Update-TicketStatus -JiraUrl $jiraUrl -Username "" -ApiToken $apiToken -TicketNumber $TicketNumber -NewStatus $config.defaultStatus -Config $config
        }
        
        # Add labels if configured
        if ($config.defaultLabels) {
            Add-TicketLabels -JiraUrl $jiraUrl -Username "" -ApiToken $apiToken -TicketNumber $TicketNumber -Labels $config.defaultLabels -Config $config
        }
        
        Write-ColorOutput "`nOperation completed successfully!" "Green"
    } else {
        Write-ColorOutput "`nOperation failed. Please check the error messages above." "Red"
        exit 1
    }
} else {
    Write-ColorOutput "Operation cancelled by user." "Yellow"
    exit 0
}