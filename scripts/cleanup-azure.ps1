#!/usr/bin/env pwsh

<#
.SYNOPSIS
Deletes Azure resources for PoTicTac application
.DESCRIPTION
This script deletes the resource group and all resources within it
#>

param(
    [string]$EnvironmentName = "PoTicTac",
    [switch]$Force
)

$ErrorActionPreference = "Stop"

Write-Host "Preparing to delete resource group: $EnvironmentName" -ForegroundColor Yellow

if (-not $Force) {
    $confirmation = Read-Host "Are you sure you want to delete resource group '$EnvironmentName' and all its resources? (yes/no)"
    if ($confirmation -ne "yes") {
        Write-Host "Deletion cancelled" -ForegroundColor Yellow
        exit 0
    }
}

Write-Host "Deleting resource group: $EnvironmentName..." -ForegroundColor Red
az group delete --name $EnvironmentName --yes --no-wait

if ($LASTEXITCODE -eq 0) {
    Write-Host "Resource group deletion initiated (running in background)" -ForegroundColor Green
    Write-Host "Run 'az group show --name $EnvironmentName' to check status" -ForegroundColor Cyan
} else {
    Write-Error "Failed to delete resource group"
    exit 1
}
