param($installPath, $toolsPath, $package, $project)

Write-Host "Setting Deploy.ps1 to 'Copy always'"
$item = $project.ProjectItems | where-object {$_.Name -eq "Deploy.ps1"}
$item.Properties.Item("CopyToOutputDirectory").Value = [int]1

Write-Host "Setting readme.txt files' BuildActions to 'None'"
$project.ProjectItems.Item("Improving.DbUp.readme.txt").Properties.Item("BuildAction").Value = [int]0
$project.ProjectItems.Item("Scripts").ProjectItems.Item("readme.txt").Properties.Item("BuildAction").Value = [int]0
$project.ProjectItems.Item("Scripts").ProjectItems.Item("AlwaysRun").ProjectItems.Item("readme.txt").Properties.Item("BuildAction").Value = [int]0
$project.ProjectItems.Item("Scripts").ProjectItems.Item("BeforeMigration").ProjectItems.Item("readme.txt").Properties.Item("BuildAction").Value = [int]0
$project.ProjectItems.Item("Scripts").ProjectItems.Item("FirstRun").ProjectItems.Item("readme.txt").Properties.Item("BuildAction").Value = [int]0
$project.ProjectItems.Item("Scripts").ProjectItems.Item("Migration").ProjectItems.Item("readme.txt").Properties.Item("BuildAction").Value = [int]0
$project.ProjectItems.Item("Scripts").ProjectItems.Item("Seed").ProjectItems.Item("readme.txt").Properties.Item("BuildAction").Value = [int]0
$project.ProjectItems.Item("Scripts").ProjectItems.Item("Test").ProjectItems.Item("readme.txt").Properties.Item("BuildAction").Value = [int]0

Write-Host
Write-Host "For a new DbUp project, install Improving.DbUp.QuickStart."
Write-Host "Be sure to set Build Action to 'Embedded Resource' for all .sql files."
Write-Host "See http://dbup.readthedocs.org/en/latest/ for the latest DbUp documentation."

