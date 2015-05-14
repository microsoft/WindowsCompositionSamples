# .SYNOPSIS
#   Adds Application Insights bootstrapping code to the given EnvDTE.Project.
function Add-ApplicationInsights($project)
{
    if (!$project)
    {
        $project = Get-Project;
    }

    Write-Verbose "Adding Application Insights bootstrapping code to $($project.FullName).";
    $entryPointClassName = Read-ApplicationEntryPoint $project;
    if ($entryPointClassName)
    {
        $entryPointClass = Find-CodeClass $project $entryPointClassName;
        Add-ApplicationInsightsVariable $entryPointClass;
        Add-ApplicationInsightsVariableInitializer $entryPointClass;
    }
}

# Private

Set-Variable telemetryClientVariableName -option Constant -value "TelemetryClient";

function Add-ApplicationInsightsVariable($codeClass)
{
    $telemetryClientVariable = Find-CodeVariable $codeClass $telemetryClientVariableName;
    if (!$telemetryClientVariable)
    {
        Write-Verbose "Adding the $telemetryClientVariableName variable to the $($codeClass.FullName) class in $($codeClass.ProjectItem.FileNames(1)).";
        
		$telemetryClientVariable = $codeClass.AddVariable(
			$telemetryClientVariableName, 
			"Microsoft.ApplicationInsights.TelemetryClient", 
			0, 
			[EnvDTE.vsCMAccess]::vsCMAccessPublic);

        $telemetryClientVariable.IsShared = $true;

        $docComment = 
            "<summary>`n" +
            "Allows tracking page views, exceptions and other telemetry through the Microsoft Application Insights service.`n" +
            "</summary>";
        Set-DocComment $telemetryClientVariable $docComment;

        # For consistency between C# and VB
        Add-BlankLine $telemetryClientVariable.EndPoint;
    }
}

function Add-ApplicationInsightsVariableInitializer($codeClass)
{
    $constructor = Find-Constructor $codeClass;
    if (!$constructor)
    {
        $constructor = Add-Constructor $codeClass;
        Add-Statement $constructor "InitializeComponent()";
    }

    $statementText = "$telemetryClientVariableName = new Microsoft.ApplicationInsights.TelemetryClient()";
    $statement = Find-Statement $constructor $statementText;
    if (!$statement)
    {
        # Inject code in the beginning of the constructor to start tracking exceptions as soon as possible
        Write-Verbose "Initializing the $telemetryClientVariableName variable in constructor of $($codeClass.Name).";
        $textPoint = Add-Statement $constructor $statementText;
        Add-BlankLine $textPoint;
    }
}

function Add-Constructor($codeClass)
{
    Write-Verbose "Adding constructor to $($codeClass.FullName) class in $($codeClass.ProjectItem.FileNames(1)).";

    if ($codeClass.Language -eq [EnvDTE.CodeModelLanguageConstants]::vsCMLanguageCSharp)
    {
        $constructor = $codeClass.AddFunction($codeClass.Name, [EnvDTE.vsCMFunction]::vsCMFunctionConstructor, $null, $null, [EnvDTE.vsCMAccess]::vsCMAccessPublic);
    }
    else
    {
        $constructor = $codeClass.AddFunction("New", [EnvDTE.vsCMFunction]::vsCMFunctionSub, $null, $null, [EnvDTE.vsCMAccess]::vsCMAccessPublic);

        # Delete blank lines for consistency with C#
        $constructor = Get-Interface $constructor -InterfaceType "EnvDTE80.CodeFunction2";
        $constructor.
            GetStartPoint([EnvDTE.vsCMPart]::vsCMPartBody).
            CreateEditPoint().
            DeleteWhitespace([EnvDTE.vsWhitespaceOptions]::vsWhitespaceOptionsVertical);
    }

    # Add a doc comment above constructor for consistency with default code templates.
    $docComment = 
        "<summary>`n" +
        "Initializes a new instance of the $($codeClass.Name) class.`n" +
        "</summary>";
    Set-DocComment $constructor $docComment;

    # Add a blank line below the constructor for readability
    Add-BlankLine $constructor.EndPoint;

    return $constructor;
}

function Add-Statement($codeFunction, $statement)
{
    if ($codeFunction.Language -eq [EnvDTE.CodeModelLanguageConstants]::vsCMLanguageCSharp)
    {
        $statement = $statement + ";";
        $indentLevel = 3;
    }
    else
    {
        $indentLevel = 2;
    }
    
    $textPoint = $codeFunction.GetStartPoint([EnvDTE.vsCMPart]::vsCMPartBody);
    $editPoint = $textPoint.CreateEditPoint();
    $editPoint.Insert($statement);

    # Align new statement with the rest of the code in this function
    $editPoint.Indent($editPoint, $indentLevel);

    $editPoint.Insert([Environment]::NewLine);

    # Move text caret to the end of the newly added line
    $editPoint.LineUp();
    $editPoint.EndOfLine();

    return $editPoint;
}

function Add-BlankLine($textPoint)
{
    $editPoint = $textPoint.CreateEditPoint();

    # Delete existing blank lines, if any
    $editPoint.LineDown();
    $editPoint.StartOfLine();
    $editPoint.DeleteWhitespace([EnvDTE.vsWhitespaceOptions]::vsWhitespaceOptionsVertical);

    # Insert a single blank line
    $editPoint.Insert([Environment]::NewLine);
}

function Set-DocComment($codeElement, $docComment)
{
    if ($codeElement.Language -eq [EnvDTE.CodeModelLanguageConstants]::vsCMLanguageCSharp)
    {
        # C# expects DocComment inside of a <doc/> element
        $docComment = "<doc>$docComment</doc>";
    }

    $codeElement.DocComment = $docComment;
}

function Find-CodeVariable($codeClass, [string]$name)
{
    return Get-CodeElement $codeClass.Members | Where-Object { ($_ -is [EnvDTE.CodeVariable]) -and ($_.Name -eq $name) };
}

function Find-Constructor($codeClass)
{
    return Get-CodeElement $codeClass.Members |
        Foreach-Object { Get-Interface $_ -InterfaceType "EnvDTE80.CodeFunction2" } | 
        Where-Object { $_.FunctionKind -eq [EnvDTE.vsCMFunction]::vsCMFunctionConstructor };
}

function Find-CodeClass($project, [string]$fullName)
{
    $class = $project.CodeModel.CodeTypeFromFullName($fullName);
    if ($class)
    {
        $fileName = $class.ProjectItem.FileNames(1);

        # Work around the problem in Dev14, which sometimes finds classes in generated files instead of those in the project.
        if ($fileName.Contains(".g."))
        {
            Write-Verbose "Class $fullName is found in a generated file $fileName. Trying to work around.";
            $document = $project.DTE.Documents.Open($fileName);
            $document.Close();
            return Find-CodeClass $project $fullName;
        }

        Write-Verbose "Class $fullName is defined in $fileName.";
    }

    return $class;
}

function Find-Statement($codeElement, $statement)
{
    $editPoint = $codeElement.StartPoint.CreateEditPoint();
    $bodyText = $editPoint.GetText($codeElement.EndPoint);
    $indexOfStatement = $bodyText.IndexOf($statement);
    return $indexOfStatement -ge 0;
}

function Get-CodeElement($codeElements)
{
    foreach ($child in $codeElements)
    {
        Write-Output $child;
        Get-CodeElement $child.Members;
    }
}

function Read-ApplicationEntryPoint($project)
{
    # Try getting a Silverlight entry point first because Silverlight 8.1 have a different appxmanifest.
    $entryPoint = Get-SilverlightEntryPoint($project);

    if (!$entryPoint)
    {
        $entryPoint = Get-WindowsRuntimeEntryPoint($project);
    }

    if ($entryPoint)
    {
        Write-Verbose "Application entry point is $entryPoint.";
    }

    return $entryPoint;
}

function Get-WindowsRuntimeEntryPoint($project)
{
    $appxManifestXml = Find-AppxManifestXml $project;
    if ($appxManifestXml)
    {
        $entryPoint = $appxManifestXml.Package.Applications.FirstChild.EntryPoint;

        # for universal projects entry point namespace will be based on assembly name instead of root namespace
        $rootNamespace = Get-PropertyValue $project "RootNamespace";
        $assemblyName = Get-PropertyValue $project "AssemblyName";
        $entryPoint = $entryPoint.Replace($assemblyName, $rootNamespace);
    }

    return $entryPoint;
}

function Get-SilverlightEntryPoint($project)
{
    return Get-PropertyValue $project "WindowsPhoneProject.AppEntryObject";
}

function Find-AppxManifestXml($project)
{
    $projectItem = Find-ProjectItem $project.ProjectItems IsAppxManifest;
    if ($projectItem -ne $null)
    {
        $filePath = $projectItem.FileNames(1);
        Write-Verbose "Loading application manifest from $filePath.";
        $fileContents = Get-Content $filePath;
        return [xml]$fileContents;
    }

    return $null;
}

function IsAppxManifest($projectItem)
{
    $itemType = Get-PropertyValue $projectItem "ItemType";
    return $itemType -eq "AppxManifest";
}

function Find-ProjectItem($projectItems, $isMatch)
{
    foreach ($childItem in $projectItems)
    {
        if (&$isMatch $childItem)
        {
            return $childItem;
        }

        $descendantItem = Find-ProjectItem $childItem.ProjectItems $isMatch;
        if ($descendantItem -ne $null)
        {
            return $descendantItem;
        }
    }

    return $null;
}

function Get-PropertyValue($item, [string]$propertyName)
{
    try
    {
        $value = $item.Properties.Item($propertyName).Value;
    }
    catch [System.ArgumentException], [System.Management.Automation.MethodInvocationException]
    {
    }

    Write-Verbose "$propertyName property of $($item.Name) is <$value>.";
    return $value;
}

# Exports
Export-ModuleMember -Function Add-ApplicationInsights;

# SIG # Begin signature block
# MIIazwYJKoZIhvcNAQcCoIIawDCCGrwCAQExCzAJBgUrDgMCGgUAMGkGCisGAQQB
# gjcCAQSgWzBZMDQGCisGAQQBgjcCAR4wJgIDAQAABBAfzDtgWUsITrck0sYpfvNR
# AgEAAgEAAgEAAgEAAgEAMCEwCQYFKw4DAhoFAAQUVdxIrFgJ0wH+TbWN0eVJOZ45
# r0CgghWCMIIEwzCCA6ugAwIBAgITMwAAAHD0GL8jIfxQnQAAAAAAcDANBgkqhkiG
# 9w0BAQUFADB3MQswCQYDVQQGEwJVUzETMBEGA1UECBMKV2FzaGluZ3RvbjEQMA4G
# A1UEBxMHUmVkbW9uZDEeMBwGA1UEChMVTWljcm9zb2Z0IENvcnBvcmF0aW9uMSEw
# HwYDVQQDExhNaWNyb3NvZnQgVGltZS1TdGFtcCBQQ0EwHhcNMTUwMzIwMTczMjAy
# WhcNMTYwNjIwMTczMjAyWjCBszELMAkGA1UEBhMCVVMxEzARBgNVBAgTCldhc2hp
# bmd0b24xEDAOBgNVBAcTB1JlZG1vbmQxHjAcBgNVBAoTFU1pY3Jvc29mdCBDb3Jw
# b3JhdGlvbjENMAsGA1UECxMETU9QUjEnMCUGA1UECxMebkNpcGhlciBEU0UgRVNO
# OkY1MjgtMzc3Ny04QTc2MSUwIwYDVQQDExxNaWNyb3NvZnQgVGltZS1TdGFtcCBT
# ZXJ2aWNlMIIBIjANBgkqhkiG9w0BAQEFAAOCAQ8AMIIBCgKCAQEAoxTZ7xygeRG9
# LZoEnSM0gqVCHSsA0dIbMSnIKivzLfRui93iG/gT9MBfcFOv5zMPdEoHFGzcKAO4
# Kgp4xG4gjguAb1Z7k/RxT8LTq8bsLa6V0GNnsGSmNAMM44quKFICmTX5PGTbKzJ3
# wjTuUh5flwZ0CX/wovfVkercYttThkdujAFb4iV7ePw9coMie1mToq+TyRgu5/YK
# VA6YDWUGV3eTka+Ur4S+uG+thPT7FeKT4thINnVZMgENcXYAlUlpbNTGNjpaMNDA
# ynOJ5pT2Ix4SYFEACMHe2j9IhO21r9TTmjiVqbqjWLV4aEa/D4xjcb46Q0NZEPBK
# unvW5QYT3QIDAQABo4IBCTCCAQUwHQYDVR0OBBYEFG3P87iErvfMdr24e6w9l2GB
# dCsnMB8GA1UdIwQYMBaAFCM0+NlSRnAK7UD7dvuzK7DDNbMPMFQGA1UdHwRNMEsw
# SaBHoEWGQ2h0dHA6Ly9jcmwubWljcm9zb2Z0LmNvbS9wa2kvY3JsL3Byb2R1Y3Rz
# L01pY3Jvc29mdFRpbWVTdGFtcFBDQS5jcmwwWAYIKwYBBQUHAQEETDBKMEgGCCsG
# AQUFBzAChjxodHRwOi8vd3d3Lm1pY3Jvc29mdC5jb20vcGtpL2NlcnRzL01pY3Jv
# c29mdFRpbWVTdGFtcFBDQS5jcnQwEwYDVR0lBAwwCgYIKwYBBQUHAwgwDQYJKoZI
# hvcNAQEFBQADggEBAF46KvVn9AUwKt7hue9n/Cr/bnIpn558xxPDo+WOPATpJhVN
# 98JnglwKW8UK7lXwoy2Ooh2isywt0BHimioB0TAmZ6GmbokxHG7dxHFU8Ami3cHW
# NnPADP9VCGv8oZT9XSwnIezRIwbcBCzvuQLbA7tHcxgK632ZzV8G4Ij3ipPFEhEb
# 81KVo3Kg0ljZwyzia3931GNT6oK4L0dkKJjHgzvxayhh+AqIgkVSkumDJklct848
# mn+voFGTxby6y9ErtbuQGQqmp2p++P0VfkZEh6UG1PxKcDjG6LVK9NuuL+xDyYmi
# KMVV2cG6W6pgu6W7+dUCjg4PbcI1cMCo7A2hsrgwggTsMIID1KADAgECAhMzAAAA
# ymzVMhI1xOFVAAEAAADKMA0GCSqGSIb3DQEBBQUAMHkxCzAJBgNVBAYTAlVTMRMw
# EQYDVQQIEwpXYXNoaW5ndG9uMRAwDgYDVQQHEwdSZWRtb25kMR4wHAYDVQQKExVN
# aWNyb3NvZnQgQ29ycG9yYXRpb24xIzAhBgNVBAMTGk1pY3Jvc29mdCBDb2RlIFNp
# Z25pbmcgUENBMB4XDTE0MDQyMjE3MzkwMFoXDTE1MDcyMjE3MzkwMFowgYMxCzAJ
# BgNVBAYTAlVTMRMwEQYDVQQIEwpXYXNoaW5ndG9uMRAwDgYDVQQHEwdSZWRtb25k
# MR4wHAYDVQQKExVNaWNyb3NvZnQgQ29ycG9yYXRpb24xDTALBgNVBAsTBE1PUFIx
# HjAcBgNVBAMTFU1pY3Jvc29mdCBDb3Jwb3JhdGlvbjCCASIwDQYJKoZIhvcNAQEB
# BQADggEPADCCAQoCggEBAJZxXe0GRvqEy51bt0bHsOG0ETkDrbEVc2Cc66e2bho8
# P/9l4zTxpqUhXlaZbFjkkqEKXMLT3FIvDGWaIGFAUzGcbI8hfbr5/hNQUmCVOlu5
# WKV0YUGplOCtJk5MoZdwSSdefGfKTx5xhEa8HUu24g/FxifJB+Z6CqUXABlMcEU4
# LYG0UKrFZ9H6ebzFzKFym/QlNJj4VN8SOTgSL6RrpZp+x2LR3M/tPTT4ud81MLrs
# eTKp4amsVU1Mf0xWwxMLdvEH+cxHrPuI1VKlHij6PS3Pz4SYhnFlEc+FyQlEhuFv
# 57H8rEBEpamLIz+CSZ3VlllQE1kYc/9DDK0r1H8wQGcCAwEAAaOCAWAwggFcMBMG
# A1UdJQQMMAoGCCsGAQUFBwMDMB0GA1UdDgQWBBQfXuJdUI1Whr5KPM8E6KeHtcu/
# gzBRBgNVHREESjBIpEYwRDENMAsGA1UECxMETU9QUjEzMDEGA1UEBRMqMzE1OTUr
# YjQyMThmMTMtNmZjYS00OTBmLTljNDctM2ZjNTU3ZGZjNDQwMB8GA1UdIwQYMBaA
# FMsR6MrStBZYAck3LjMWFrlMmgofMFYGA1UdHwRPME0wS6BJoEeGRWh0dHA6Ly9j
# cmwubWljcm9zb2Z0LmNvbS9wa2kvY3JsL3Byb2R1Y3RzL01pY0NvZFNpZ1BDQV8w
# OC0zMS0yMDEwLmNybDBaBggrBgEFBQcBAQROMEwwSgYIKwYBBQUHMAKGPmh0dHA6
# Ly93d3cubWljcm9zb2Z0LmNvbS9wa2kvY2VydHMvTWljQ29kU2lnUENBXzA4LTMx
# LTIwMTAuY3J0MA0GCSqGSIb3DQEBBQUAA4IBAQB3XOvXkT3NvXuD2YWpsEOdc3wX
# yQ/tNtvHtSwbXvtUBTqDcUCBCaK3cSZe1n22bDvJql9dAxgqHSd+B+nFZR+1zw23
# VMcoOFqI53vBGbZWMrrizMuT269uD11E9dSw7xvVTsGvDu8gm/Lh/idd6MX/YfYZ
# 0igKIp3fzXCCnhhy2CPMeixD7v/qwODmHaqelzMAUm8HuNOIbN6kBjWnwlOGZRF3
# CY81WbnYhqgA/vgxfSz0jAWdwMHVd3Js6U1ZJoPxwrKIV5M1AHxQK7xZ/P4cKTiC
# 095Sl0UpGE6WW526Xxuj8SdQ6geV6G00DThX3DcoNZU6OJzU7WqFXQ4iEV57MIIF
# vDCCA6SgAwIBAgIKYTMmGgAAAAAAMTANBgkqhkiG9w0BAQUFADBfMRMwEQYKCZIm
# iZPyLGQBGRYDY29tMRkwFwYKCZImiZPyLGQBGRYJbWljcm9zb2Z0MS0wKwYDVQQD
# EyRNaWNyb3NvZnQgUm9vdCBDZXJ0aWZpY2F0ZSBBdXRob3JpdHkwHhcNMTAwODMx
# MjIxOTMyWhcNMjAwODMxMjIyOTMyWjB5MQswCQYDVQQGEwJVUzETMBEGA1UECBMK
# V2FzaGluZ3RvbjEQMA4GA1UEBxMHUmVkbW9uZDEeMBwGA1UEChMVTWljcm9zb2Z0
# IENvcnBvcmF0aW9uMSMwIQYDVQQDExpNaWNyb3NvZnQgQ29kZSBTaWduaW5nIFBD
# QTCCASIwDQYJKoZIhvcNAQEBBQADggEPADCCAQoCggEBALJyWVwZMGS/HZpgICBC
# mXZTbD4b1m/My/Hqa/6XFhDg3zp0gxq3L6Ay7P/ewkJOI9VyANs1VwqJyq4gSfTw
# aKxNS42lvXlLcZtHB9r9Jd+ddYjPqnNEf9eB2/O98jakyVxF3K+tPeAoaJcap6Vy
# c1bxF5Tk/TWUcqDWdl8ed0WDhTgW0HNbBbpnUo2lsmkv2hkL/pJ0KeJ2L1TdFDBZ
# +NKNYv3LyV9GMVC5JxPkQDDPcikQKCLHN049oDI9kM2hOAaFXE5WgigqBTK3S9dP
# Y+fSLWLxRT3nrAgA9kahntFbjCZT6HqqSvJGzzc8OJ60d1ylF56NyxGPVjzBrAlf
# A9MCAwEAAaOCAV4wggFaMA8GA1UdEwEB/wQFMAMBAf8wHQYDVR0OBBYEFMsR6MrS
# tBZYAck3LjMWFrlMmgofMAsGA1UdDwQEAwIBhjASBgkrBgEEAYI3FQEEBQIDAQAB
# MCMGCSsGAQQBgjcVAgQWBBT90TFO0yaKleGYYDuoMW+mPLzYLTAZBgkrBgEEAYI3
# FAIEDB4KAFMAdQBiAEMAQTAfBgNVHSMEGDAWgBQOrIJgQFYnl+UlE/wq4QpTlVnk
# pDBQBgNVHR8ESTBHMEWgQ6BBhj9odHRwOi8vY3JsLm1pY3Jvc29mdC5jb20vcGtp
# L2NybC9wcm9kdWN0cy9taWNyb3NvZnRyb290Y2VydC5jcmwwVAYIKwYBBQUHAQEE
# SDBGMEQGCCsGAQUFBzAChjhodHRwOi8vd3d3Lm1pY3Jvc29mdC5jb20vcGtpL2Nl
# cnRzL01pY3Jvc29mdFJvb3RDZXJ0LmNydDANBgkqhkiG9w0BAQUFAAOCAgEAWTk+
# fyZGr+tvQLEytWrrDi9uqEn361917Uw7LddDrQv+y+ktMaMjzHxQmIAhXaw9L0y6
# oqhWnONwu7i0+Hm1SXL3PupBf8rhDBdpy6WcIC36C1DEVs0t40rSvHDnqA2iA6VW
# 4LiKS1fylUKc8fPv7uOGHzQ8uFaa8FMjhSqkghyT4pQHHfLiTviMocroE6WRTsgb
# 0o9ylSpxbZsa+BzwU9ZnzCL/XB3Nooy9J7J5Y1ZEolHN+emjWFbdmwJFRC9f9Nqu
# 1IIybvyklRPk62nnqaIsvsgrEA5ljpnb9aL6EiYJZTiU8XofSrvR4Vbo0HiWGFzJ
# NRZf3ZMdSY4tvq00RBzuEBUaAF3dNVshzpjHCe6FDoxPbQ4TTj18KUicctHzbMrB
# 7HCjV5JXfZSNoBtIA1r3z6NnCnSlNu0tLxfI5nI3EvRvsTxngvlSso0zFmUeDord
# EN5k9G/ORtTTF+l5xAS00/ss3x+KnqwK+xMnQK3k+eGpf0a7B2BHZWBATrBC7E7t
# s3Z52Ao0CW0cgDEf4g5U3eWh++VHEK1kmP9QFi58vwUheuKVQSdpw5OPlcmN2Jsh
# rg1cnPCiroZogwxqLbt2awAdlq3yFnv2FoMkuYjPaqhHMS+a3ONxPdcAfmJH0c6I
# ybgY+g5yjcGjPa8CQGr/aZuW4hCoELQ3UAjWwz0wggYHMIID76ADAgECAgphFmg0
# AAAAAAAcMA0GCSqGSIb3DQEBBQUAMF8xEzARBgoJkiaJk/IsZAEZFgNjb20xGTAX
# BgoJkiaJk/IsZAEZFgltaWNyb3NvZnQxLTArBgNVBAMTJE1pY3Jvc29mdCBSb290
# IENlcnRpZmljYXRlIEF1dGhvcml0eTAeFw0wNzA0MDMxMjUzMDlaFw0yMTA0MDMx
# MzAzMDlaMHcxCzAJBgNVBAYTAlVTMRMwEQYDVQQIEwpXYXNoaW5ndG9uMRAwDgYD
# VQQHEwdSZWRtb25kMR4wHAYDVQQKExVNaWNyb3NvZnQgQ29ycG9yYXRpb24xITAf
# BgNVBAMTGE1pY3Jvc29mdCBUaW1lLVN0YW1wIFBDQTCCASIwDQYJKoZIhvcNAQEB
# BQADggEPADCCAQoCggEBAJ+hbLHf20iSKnxrLhnhveLjxZlRI1Ctzt0YTiQP7tGn
# 0UytdDAgEesH1VSVFUmUG0KSrphcMCbaAGvoe73siQcP9w4EmPCJzB/LMySHnfL0
# Zxws/HvniB3q506jocEjU8qN+kXPCdBer9CwQgSi+aZsk2fXKNxGU7CG0OUoRi4n
# rIZPVVIM5AMs+2qQkDBuh/NZMJ36ftaXs+ghl3740hPzCLdTbVK0RZCfSABKR2YR
# JylmqJfk0waBSqL5hKcRRxQJgp+E7VV4/gGaHVAIhQAQMEbtt94jRrvELVSfrx54
# QTF3zJvfO4OToWECtR0Nsfz3m7IBziJLVP/5BcPCIAsCAwEAAaOCAaswggGnMA8G
# A1UdEwEB/wQFMAMBAf8wHQYDVR0OBBYEFCM0+NlSRnAK7UD7dvuzK7DDNbMPMAsG
# A1UdDwQEAwIBhjAQBgkrBgEEAYI3FQEEAwIBADCBmAYDVR0jBIGQMIGNgBQOrIJg
# QFYnl+UlE/wq4QpTlVnkpKFjpGEwXzETMBEGCgmSJomT8ixkARkWA2NvbTEZMBcG
# CgmSJomT8ixkARkWCW1pY3Jvc29mdDEtMCsGA1UEAxMkTWljcm9zb2Z0IFJvb3Qg
# Q2VydGlmaWNhdGUgQXV0aG9yaXR5ghB5rRahSqClrUxzWPQHEy5lMFAGA1UdHwRJ
# MEcwRaBDoEGGP2h0dHA6Ly9jcmwubWljcm9zb2Z0LmNvbS9wa2kvY3JsL3Byb2R1
# Y3RzL21pY3Jvc29mdHJvb3RjZXJ0LmNybDBUBggrBgEFBQcBAQRIMEYwRAYIKwYB
# BQUHMAKGOGh0dHA6Ly93d3cubWljcm9zb2Z0LmNvbS9wa2kvY2VydHMvTWljcm9z
# b2Z0Um9vdENlcnQuY3J0MBMGA1UdJQQMMAoGCCsGAQUFBwMIMA0GCSqGSIb3DQEB
# BQUAA4ICAQAQl4rDXANENt3ptK132855UU0BsS50cVttDBOrzr57j7gu1BKijG1i
# uFcCy04gE1CZ3XpA4le7r1iaHOEdAYasu3jyi9DsOwHu4r6PCgXIjUji8FMV3U+r
# kuTnjWrVgMHmlPIGL4UD6ZEqJCJw+/b85HiZLg33B+JwvBhOnY5rCnKVuKE5nGct
# xVEO6mJcPxaYiyA/4gcaMvnMMUp2MT0rcgvI6nA9/4UKE9/CCmGO8Ne4F+tOi3/F
# NSteo7/rvH0LQnvUU3Ih7jDKu3hlXFsBFwoUDtLaFJj1PLlmWLMtL+f5hYbMUVbo
# nXCUbKw5TNT2eb+qGHpiKe+imyk0BncaYsk9Hm0fgvALxyy7z0Oz5fnsfbXjpKh0
# NbhOxXEjEiZ2CzxSjHFaRkMUvLOzsE1nyJ9C/4B5IYCeFTBm6EISXhrIniIh0EPp
# K+m79EjMLNTYMoBMJipIJF9a6lbvpt6Znco6b72BJ3QGEe52Ib+bgsEnVLaxaj2J
# oXZhtG6hE6a/qkfwEm/9ijJssv7fUciMI8lmvZ0dhxJkAj0tr1mPuOQh5bWwymO0
# eFQF1EEuUKyUsKV4q7OglnUa2ZKHE3UiLzKoCG6gW4wlv6DvhMoh1useT8ma7kng
# 9wFlb4kLfchpyOZu6qeXzjEp/w7FW1zYTRuh2Povnj8uVRZryROj/TGCBLcwggSz
# AgEBMIGQMHkxCzAJBgNVBAYTAlVTMRMwEQYDVQQIEwpXYXNoaW5ndG9uMRAwDgYD
# VQQHEwdSZWRtb25kMR4wHAYDVQQKExVNaWNyb3NvZnQgQ29ycG9yYXRpb24xIzAh
# BgNVBAMTGk1pY3Jvc29mdCBDb2RlIFNpZ25pbmcgUENBAhMzAAAAymzVMhI1xOFV
# AAEAAADKMAkGBSsOAwIaBQCggdAwGQYJKoZIhvcNAQkDMQwGCisGAQQBgjcCAQQw
# HAYKKwYBBAGCNwIBCzEOMAwGCisGAQQBgjcCARUwIwYJKoZIhvcNAQkEMRYEFCTW
# qFD0oYs2jxS/qGwYZhVzWlUUMHAGCisGAQQBgjcCAQwxYjBgoEaARABNAGkAYwBy
# AG8AcwBvAGYAdAAuAEEAcABwAGwAaQBjAGEAdABpAG8AbgBJAG4AcwBpAGcAaAB0
# AHMALgBwAHMAbQAxoRaAFGh0dHA6Ly9taWNyb3NvZnQuY29tMA0GCSqGSIb3DQEB
# AQUABIIBAH8lJGTc1O9OPmoFXv/Gb+cMIzJ60cTpZajO5g9Y26S+1hTtBYcfGcq8
# MK4CZ2SHi+tBQzokW8jwundwART6jXaWa17JJWrda4msYLdhXR3L3BB3y+otgvc8
# 4xNgWV8/K2IDW5uimDPH3nMdD5igvHcxT12w4NaLl5iwDwUqTAMEzQTknP1hMxAn
# 0a8AVwz1mqPZddSKvfu+pcklUKkv49RZg+qWFnat4H3DYlA2jT69IKM+6APKxkve
# 0pVOAj5GJotJs9knUkfQGsh4zNWewiCAQ1B7Z3CiHJ17WxtlROVcFuCvkX7COmsG
# VSkRAu1Yu7On5sFBqqCG7jP+yYeO/m6hggIoMIICJAYJKoZIhvcNAQkGMYICFTCC
# AhECAQEwgY4wdzELMAkGA1UEBhMCVVMxEzARBgNVBAgTCldhc2hpbmd0b24xEDAO
# BgNVBAcTB1JlZG1vbmQxHjAcBgNVBAoTFU1pY3Jvc29mdCBDb3Jwb3JhdGlvbjEh
# MB8GA1UEAxMYTWljcm9zb2Z0IFRpbWUtU3RhbXAgUENBAhMzAAAAcPQYvyMh/FCd
# AAAAAABwMAkGBSsOAwIaBQCgXTAYBgkqhkiG9w0BCQMxCwYJKoZIhvcNAQcBMBwG
# CSqGSIb3DQEJBTEPFw0xNTA0MTcyMjE1MDNaMCMGCSqGSIb3DQEJBDEWBBR+dYLE
# G6aZFZqADnqg71ymhML9eDANBgkqhkiG9w0BAQUFAASCAQB1Iw7HyH4W6kU90bsA
# hcZZ79Lofopm51MV6tg9fl6NFimlpzLdW0b0MljnggBNXI04Oe/mdf1EEowulf0q
# XRYSTD0qWgbcz9EcRzPL9w3QXHhMGb3UzKGunEcUc68JV9ntZi1W/D2DLfheNpmx
# 65rdgPBvl5PonjXUraOIGTt0DJ3kFXwEcZKlU5zyeXGHj/5ReV16kyIlJZvaVDZQ
# Pia7tZ75fLRkTM+W25neOpMwFrEKNbiNXndc3XbxRIAYZtpg5y6Sz/2zMccwK0oB
# EaXWCZzNf1Tw1e1t0x7jeWH/6YyPy7KVaba2JkNsfeJeiV2bFzavxghdMgfFvxAt
# X2Pe
# SIG # End signature block
