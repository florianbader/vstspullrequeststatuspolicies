import { BrowserModule } from "@angular/platform-browser";
import { NgModule } from "@angular/core";
import { HttpClientModule } from "@angular/common/http";
import { RepositoryStatusPoliciesComponent } from "./repository-status-policies/repository-status-policies.component";
import { ToggleButtonComponent } from "../shared/toggle-button/toggle-button.component";
import { ProjectAdminComponent } from "./project-admin.component";
import { ConfigurationService } from "../shared/configuration.service";

import "VSS/Controls";
import "VSS/Controls/TreeView";
import "VSS/Controls/Splitter";

@NgModule({
  declarations: [
    ProjectAdminComponent,
    ToggleButtonComponent,
    RepositoryStatusPoliciesComponent
  ],
  imports: [BrowserModule, HttpClientModule],
  providers: [ConfigurationService],
  bootstrap: [ProjectAdminComponent]
})
export class ProjectAdminModule {}
