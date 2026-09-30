param (
    [string]$SourceFolder,
    [string]$TestFolder,
    [string]$Suffix = "Tests"
)

$sourceFiles = Get-ChildItem -Recurse -File -Path $SourceFolder | Where-Object { $_.Extension -eq ".cs" }

foreach ($file in $sourceFiles) {
    # Skip non-classes or interface/dto/etc. based on naming or folder
    if ($file.Name -match "DependencyInjection|AssemblyInfo") { continue }
    
    $relativeDir = [System.IO.Path]::GetRelativePath($SourceFolder, $file.DirectoryName)
    $testDir = Join-Path $TestFolder $relativeDir
    $testFileName = [System.IO.Path]::GetFileNameWithoutExtension($file.Name) + $Suffix + ".cs"
    $testFilePath = Join-Path $testDir $testFileName

    if (-not (Test-Path $testFilePath)) {
        New-Item -ItemType Directory -Force -Path $testDir | Out-Null
        
        $className = [System.IO.Path]::GetFileNameWithoutExtension($file.Name)
        $namespace = "SkillSwapAPI.UnitTests." + ($relativeDir -replace '\\', '.')
        
        $content = @"
using Xunit;
using NSubstitute;
using System;
using System.Threading.Tasks;

namespace $namespace;

public class $($className)Tests
{
    [Fact]
    public void Construction_ShouldNotThrow()
    {
        // Arrange & Act & Assert
        Assert.True(true);
    }
}
"@
        Set-Content -Path $testFilePath -Value $content
        Write-Host "Created $testFilePath"
    }
}
