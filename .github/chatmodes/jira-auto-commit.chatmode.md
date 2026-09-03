# JIRA Auto-Commit Integration

Automated workflow for integrating code commits with JIRA ticket management and analysis reporting.

## Pre-commit Analysis Workflow

**Important Note:** This workflow only processes **staged changes** (files that have been added to the git staging area using `git add`). Files in other states (modified but not staged, untracked, etc.) will not be processed.

### Step 1: Check Staged Changes
1. Use git commands to check staged files: `git diff --cached --name-only`
2. Get detailed diff information: `git diff --cached`
3. Identify file types and scope of changes
4. **Only process files that are in staged state** - ignore unstaged modifications and untracked files

### Step 2: Code Analysis
For each changed file:
- Read file content to understand context
- Analyze the nature of changes (bug fix, feature, refactor, etc.)
- Identify patterns and architectural decisions
- Look for security implications
- Check for dependency changes

### Step 3: Generate Analysis Report

### Step 4: Update JIRA Ticket
1. Post the analysis report to the corresponding JIRA ticket using the automated tool
2. Update ticket status if appropriate
3. Add relevant labels or components based on the analysis
4. Link related tickets if dependencies are identified

**Automated JIRA Tool Usage:**
```powershell
\.\.github\tools\jira-api.ps1 -TicketNumber "RECO-12345" -RootCause "Issue description" -Solution "Solution approach" -TestingRecommendations "Testing guidance" -CommitHash "abc123" -FilesChanged "file1.cs, file2.js" -LinesModified "+50 -20"
```

#### Root Cause Template:
```
**Root Cause:**
- [What problem does this solve?] (Keep concise, 1-2 sentences max)
```

#### Solution Template:
```
**Solution:**
- [How did you solve the problem?] (Brief technical approach, 1-2 sentences max)
```

#### Testing Recommendations Template:
```
**Testing Recommendations:**
- [What user flows need testing?] (Key test scenarios, bullet points preferred)
```

## Commit Message Format

Use format: `[JIRA-XXX]commit message`
- `[RECO-123]Add new user authentication feature`
- `[RECO-456]Resolve null pointer exception in data processor`
- `[RECO-789]Update API documentation for new endpoints`
- `[RECO-321]Restructure database connection handling`
- `[RECO-654]Add comprehensive unit tests for payment service`

## JIRA Integration

### Automated Tool Setup
Before using the JIRA integration tool, set up your configuration:

1. **Copy the configuration template:**
   ```powershell
   Copy-Item .\.github\config\jira-config.template.json .\.github\config\jira-config.json
   ```

2. **Update the configuration file** (`.github\config\jira-config.json`) with your credentials:
   - `jiraUrl`: Your JIRA instance URL (e.g., `https://company.atlassian.net`)
   - `username`: Your JIRA email address
   - `apiToken`: Your JIRA API token (generate from Account Settings → Security → API tokens)
   - Configure auto-status updates and labels as needed

3. **Ensure the config file is ignored by git** (already configured in `.github\config\.gitignore`)

### Adding Analysis Report to JIRA
1. **Use the automated JIRA tool** instead of manual posting:
   ```powershell
   .\.github\tools\jira-api.ps1 `
     -TicketNumber "RECO-12345" `
     -RootCause "Detailed issue description and impact analysis" `
     -Solution "Technical approach and implementation details" `
     -TestingRecommendations "Testing guidance and user flows" `
     -CommitHash "abc123def" `
     -FilesChanged "file1.cs, file2.js, config.json" `
     -LinesModified "+45 -12"
   ```

2. **Manual format reference** (for understanding the structure):
   ```
   h3. Code Analysis Report
   
   *Root Cause:*
   - Issue: [Description]
   - Impact: [Impact analysis]
   
   *Solution:*
   - Approach: [Technical approach]
   
   *Testing Recommendations:*
   - [Testing guidance]
   
   ```
3. **Automatic features** (when properly configured):
   - Ticket status updates based on change type
   - Smart labeling for categorization
   - Commit hash linking to repository
   - Timestamp tracking for audit trails
4. **Manual status updates** (if needed):
   - Bug fixes: Move to "Code Review" or "Testing"
   - Features: Move to "Code Review"
   - Documentation: Move to "Done"
4. **Add labels** based on analysis:
   - `security` for security-related changes
   - `performance` for performance improvements
   - `breaking-change` for breaking changes
   - `database` for database schema changes

## Git Commands to Execute

1. **Verify staged changes**: `git status`
2. **Review diff**: `git diff --cached`
3. **Commit with message**: `git commit -m "[JIRA-XXX]Brief description" -m "Detailed description"`
4. **Update JIRA ticket automatically**: 
   ```powershell
   # Extract commit information
   $commitHash = git rev-parse HEAD
   $filesChanged = git diff --name-only HEAD~1 HEAD
   $stats = git diff --stat HEAD~1 HEAD
   
   # Run JIRA tool
   .\.github\tools\jira-api.ps1 -TicketNumber "RECO-XXXXX" -RootCause "..." -Solution "..." -TestingRecommendations "..." -CommitHash $commitHash -FilesChanged $filesChanged -LinesModified $stats
   ```

## Error Handling

- If no staged changes found, inform user to stage changes first
- If JIRA ticket format is invalid, request correct format
- If commit fails, provide troubleshooting steps
- Always confirm before executing destructive operations