Param(
	$patchPath,
	$signname
)

function Retry-Command {
    [CmdletBinding()]
    Param(
        [Parameter(Position=0, Mandatory=$true)]
        [scriptblock]$ScriptBlock,

        [Parameter(Position=1, Mandatory=$false)]
        [int]$Maximum = 20
    )

    Begin {
        $cnt = 0
    }

    Process {
        do {
            $cnt++
            try {
                $ScriptBlock.Invoke()
                return
            } catch {
                Write-Error $_.Exception.InnerException.Message -ErrorAction Continue
            }
        } while ($cnt -lt $Maximum)

        # Throw an error after $Maximum unsuccessful invocations. Doesn't need
        # a condition, since the function returns upon successful invocation.
        throw 'Execution failed.'
    }
}

$CommandSign = {
    param(
        $file,
        $signname = $null
    )

    if(Test-Path $file){
        if($signname){
            . $PSScriptRoot\SignTool.exe sign /csp "DigiCert Signing Manager KSP" /kc key_542415303 /f C:\cre20231106\avepoint_inc.crt /td SHA256 /fd SHA256  /tr http://timestamp.digicert.com/scripts/timstamp.dll /d "$signname" $file
            if(!$?){
                "sign error"
                throw 'retry'
            }
        }else{
            . $PSScriptRoot\SignTool.exe sign /csp "DigiCert Signing Manager KSP" /kc key_542415303 /f C:\cre20231106\avepoint_inc.crt /td SHA256 /fd SHA256  /tr http://timestamp.digicert.com/scripts/timstamp.dll $file
            if(!$?){
                "sign error"
                throw 'retry'
            }
        }
    }
}

Retry-Command -ScriptBlock { &$CommandSign -file $patchPath -signname $signname }



