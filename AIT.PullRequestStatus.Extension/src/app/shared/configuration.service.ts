import { Injectable } from "@angular/core";
import { HttpClient } from "@angular/common/http";

@Injectable()
export class ConfigurationService {
  private configurationDocumentId = 1;

  private webContext: WebContext;
  private apiUrl = "/api/v1/configuration/";

  constructor(private httpClient: HttpClient) {
    this.webContext = VSS.getWebContext();
  }

  public async activateCollection(settings: AccountConfiguration): Promise<void> {
    const extensionConfiguration = await this.getExtensionConfiguration();

    return this.httpClient
      .post(`${extensionConfiguration.serverUrl}${this.apiUrl}${settings.collectionId}`, settings)
      .toPromise()
      .then(_ => { });
  }

  public async activateStatusPolicy(
    collectionId: string,
    projectId: string,
    repositoryId: string,
    statusPolicyName: string
  ): Promise<void> {
    const extensionConfiguration = await this.getExtensionConfiguration();

    return this.httpClient
      .post(`${extensionConfiguration.serverUrl}${this.apiUrl}${collectionId}/${projectId}/${repositoryId}/${statusPolicyName}`, {})
      .toPromise()
      .then(_ => { });
  }

  public async deactivateStatusPolicy(
    collectionId: string,
    projectId: string,
    repositoryId: string,
    statusPolicyName: string
  ): Promise<void> {
    const extensionConfiguration = await this.getExtensionConfiguration();

    return this.httpClient
      .delete(`${extensionConfiguration.serverUrl}${this.apiUrl}${collectionId}/${projectId}/${repositoryId}/${statusPolicyName}`)
      .toPromise()
      .then(_ => { });
  }

  public async reactivateProject(collectionId: string, projectId: string): Promise<void> {
    const extensionConfiguration = await this.getExtensionConfiguration();

    return this.httpClient
      .post(`${extensionConfiguration.serverUrl}${this.apiUrl}${collectionId}/${projectId}`, {})
      .toPromise()
      .then(_ => { });
  }

  public async getRepositoryStatus(collectionId: string, projectId: string, repositoryId: string): Promise<RepositoryStatus> {
    const extensionConfiguration = await this.getExtensionConfiguration();

    return this.httpClient
      .get(`${extensionConfiguration.serverUrl}${this.apiUrl}${collectionId}/${projectId}/${repositoryId}`)
      .toPromise()
      .then(s => s as RepositoryStatus);
  }

  public async getProjectStatus(collectionId: string, projectId: string): Promise<ProjectStatus> {
    const extensionConfiguration = await this.getExtensionConfiguration();

    return this.httpClient
      .get(`${extensionConfiguration.serverUrl}${this.apiUrl}${collectionId}/${projectId}`)
      .toPromise()
      .then(s => s as ProjectStatus);
  }

  public async getCollectionStatus(collectionId: string): Promise<CollectionStatus> {
    const extensionConfiguration = await this.getExtensionConfiguration();

    return this.httpClient
      .get(`${extensionConfiguration.serverUrl}${this.apiUrl}${collectionId}`)
      .toPromise()
      .then(s => s as CollectionStatus);
  }

  public async updateExtensionConfiguration(configuration: ExtensionConfiguration) {
    const service = await this.getExtensionService();
    await service.setDocument(this.webContext.collection.name, {
      id: this.configurationDocumentId,
      ...configuration
    });
  }

  public async getExtensionConfiguration() {
    const service = await this.getExtensionService();
    return await service.getDocument(this.webContext.collection.name, this.configurationDocumentId.toString()) as ExtensionConfiguration;
  }

  private async getExtensionService(): Promise<IExtensionDataService> {
    return await VSS.getService(VSS.ServiceIds.ExtensionData) as IExtensionDataService;
  }
}
