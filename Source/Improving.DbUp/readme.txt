************************IMPORTANT:****************************************
*   For *NEW* DbUp projects, install Improving.DbUp.QuickStart 			 *
**************************************************************************

Scripts in the default supported folder structure, built into 
Improving.DbUp (in Scripts/), run in this order:

  1. FirstRun (if db doesn't exist; 
     not logged to SchemaVersions table)
  2. BeforeMigration (only scripts not run prior)
  3. Migration (only scripts not run prior)
  4. Hashed (only scripts new or edited since last run)
  5. AlwaysRun (all scripts, every time; 
     not logged to SchemaVersions table)
  6. Test (all tests are dropped & recreated from Scripts/Test, every time)
  7. SeedData (only scripts not run prior)

************************IMPORTANT:******************************
*   Make sure to set the build action to "Embedded Resource"   *
*   for all SQL Scripts & include "Use YourDbName" at top!	   *
****************************************************************


See http://dbup.readthedocs.org/en/latest/ for dbup info.

