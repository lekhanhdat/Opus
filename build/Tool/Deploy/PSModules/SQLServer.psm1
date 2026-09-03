function New-SqlConnection
{
    param(
        [string]$ConnectionString
    )
    $SqlConnection = New-Object System.Data.SqlClient.SqlConnection
    $SqlConnection.ConnectionString = $ConnectionString
    try{
        $SqlConnection.Open()
        Write-Host 'Connected to sql server.'
        return $SqlConnection
    }
    catch
    {
        Write-Warning ('Connect to database failed with error message:{0}' -f ,$_)
        $SqlConnection.Dispose()
        return $null
    }
}
 
function Get-SqlDataTable
{
    param(
        [System.Data.SqlClient.SqlConnection]$SqlConnection,
        [string]$query
    )
    $dataSet = new-object "System.Data.DataSet" "WrestlersDataset"
    $dataAdapter = new-object "System.Data.SqlClient.SqlDataAdapter" ($query,$SqlConnection)
    $dataAdapter.Fill($dataSet) | Out-Null
    return $dataSet.Tables | select -First 1
}
 
function Invoke-SqlCommandNonQuery
{
    param
    (
        [System.Data.SqlClient.SqlConnection]$SqlConnection,
        [string]$Command
    )
    $cmd = $SqlConnection.CreateCommand()
    try
    {
        $cmd.CommandText = $Command
        $cmd.ExecuteNonQuery() | Out-Null
        return $true
    }
    catch 
    {
         Write-Warning ('Execute Sql command failed with error message:{0}' -f $_)
         return $false
    }
    finally
    {
        $SqlConnection.Close()
    }
}
 
function Invoke-SqlCommandsNonQuery
{
    param
    (
        [System.Data.SqlClient.SqlConnection]$SqlConnection,
        [string[]]$Commands
    )
    $transaction = $SqlConnection.BeginTransaction()
    $command = $SqlConnection.CreateCommand()
    $command.Transaction = $transaction
    try
    {
        foreach($cmd in $Commands) {
            #Write-Host  $cmd -ForegroundColor Blue
            $command.CommandText = $cmd
            $command.ExecuteNonQuery()
        }
        $transaction.Commit()
        return $true
    }
    catch
    {
         $transaction.Rollback()
         Write-Warning ('Execute Sql commands failed with error message:{0}' -f $_)
         return $false
    }
    finally
    {
        $SqlConnection.Close()
    }
}

Export-ModuleMember -Function New-SqlConnection
Export-ModuleMember -Function Get-SqlDataTable
Export-ModuleMember -Function Invoke-SqlCommandNonQuery
Export-ModuleMember -Function Invoke-SqlCommandsNonQuery