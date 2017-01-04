param($installPath, $toolsPath, $package, $project)

Write-Host "Setting 0000_ApplyPermissions.sql BuildAction to 'EmbeddedResource'"
$project.ProjectItems.Item("Scripts").ProjectItems.Item("AlwaysRun").ProjectItems.Item("0000_ApplyPermissions.sql").Properties.Item("BuildAction").Value = [int]3

Write-Host "Setting 0000_UseDb.sql BuildAction to 'EmbeddedResource'"
$project.ProjectItems.Item("Scripts").ProjectItems.Item("BeforeMigration").ProjectItems.Item("0000_UseDb.sql").Properties.Item("BuildAction").Value = [int]3

Write-Host "Setting 0000_CreateDatabase.sql BuildAction to 'EmbeddedResource'"
$project.ProjectItems.Item("Scripts").ProjectItems.Item("FirstRun").ProjectItems.Item("0000_CreateDatabase.sql").Properties.Item("BuildAction").Value = [int]3

Write-Host "Setting 0001_tSQLt.class.sql BuildAction to 'EmbeddedResource'"
$project.ProjectItems.Item("Scripts").ProjectItems.Item("Test").ProjectItems.Item("0001_tSQLt.class.sql").Properties.Item("BuildAction").Value = [int]3
Write-Host "Setting 0002_tSQLt.SetClrEnabled.sql BuildAction to 'EmbeddedResource'"
$project.ProjectItems.Item("Scripts").ProjectItems.Item("Test").ProjectItems.Item("0002_tSQLt.SetClrEnabled.sql").Properties.Item("BuildAction").Value = [int]3
Write-Host "Setting 0003_testSchemas.sql BuildAction to 'EmbeddedResource'"
$project.ProjectItems.Item("Scripts").ProjectItems.Item("Test").ProjectItems.Item("0003_testSchemas.sql").Properties.Item("BuildAction").Value = [int]3

Write-Host "Setting readme.txt files' BuildActions to 'None'"
$project.ProjectItems.Item("Improving.DbUp.QuickStart.readme.txt").Properties.Item("BuildAction").Value = [int]0

Write-Host "Setting App.config to 'Copy always'"
$item = $project.ProjectItems | where-object {$_.Name -eq "App.config"} 
$item.Properties.Item("CopyToOutputDirectory").Value = [int]1

Write-Host
Write-Host "Be sure to set Build Action to 'Embedded Resource' for all .sql files."
Write-Host "See http://dbup.readthedocs.org/en/latest/ for the latest DbUp documentation."

