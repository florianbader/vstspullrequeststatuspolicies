import { ConfigurationService } from "./../shared/configuration.service";
import { Component, OnInit } from "@angular/core";
import { getClient } from "TFS/VersionControl/GitRestClient";
import { GitRepository } from "TFS/VersionControl/Contracts";

@Component({
  selector: "app-project-admin",
  templateUrl: "./project-admin.component.html",
  styleUrls: ["./project-admin.component.css"]
})
export class ProjectAdminComponent implements OnInit {
  private webContext: WebContext;
  private isReactivatingProject: boolean;

  public repositories: GitRepository[] = [];
  public selectedRepository: GitRepository;
  public collectionStatus: CollectionStatus;
  public projectStatus: ProjectStatus;
  public collectionHubUrl: string;
  public isBusy: boolean;

  constructor(private configurationService: ConfigurationService) {}

  public reactivateProject() {
    if (this.isReactivatingProject) {
      return;
    }

    this.isReactivatingProject = true;
    this.isBusy = true;

    this.configurationService
      .reactivateProject(this.webContext.collection.id, this.webContext.project.id)
      .finally(async () => {
        this.isReactivatingProject = false;

        this.projectStatus = await this.configurationService.getProjectStatus(
          this.webContext.collection.id,
          this.webContext.project.id
        );

        this.isBusy = false;
      });
  }

  public ngOnInit() {
    this.init();
  }

  private async init() {
    this.isBusy = true;

    const extensionContext = VSS.getExtensionContext();
    this.webContext = VSS.getWebContext();

    this.collectionHubUrl = `${this.webContext.collection.uri}_apps/hub/${extensionContext.publisherId}.${
      extensionContext.extensionId
    }.ait-status-policies-collection-admin-hub`;

    const client = getClient();

    this.repositories = await client.getRepositories(this.webContext.project.id);

    this.collectionStatus = await this.configurationService.getCollectionStatus(this.webContext.collection.id);
    this.projectStatus = await this.configurationService.getProjectStatus(
      this.webContext.collection.id,
      this.webContext.project.id
    );

    if (this.repositories.length > 0) {
      this.selectedRepository = this.repositories[0];
    }

    this.isBusy = false;
  }
}
