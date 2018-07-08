# VSTS Pull Request Status Policies
This extension adds additional status policies for VSTS git pull requests.

## Status policies

* **Work in progress**   
Pull requests are only successful if there is no work in progress prefix in the pull request title.
   
* **Description task list**   
Pull requests are only successful if all tasks in the description task list are done or if there is no task list in the description.
   
* **Out of date with base branch**   
Pull requests are only successful if they have the latest changes from the base branch.