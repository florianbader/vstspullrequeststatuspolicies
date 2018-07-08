import { Injectable } from "@angular/core";
import { HttpClient } from "@angular/common/http";
import { environment } from "../../environments/environment";

@Injectable()
export class ConfigurationService {
  private apiUrl = environment.apiBaseUrl + "api/v1/configuration/";

  constructor(private httpClient: HttpClient) {}

  public activateCollection(settings: AccountConfiguration): Promise<void> {
    return this.httpClient
      .post(`${this.apiUrl}${settings.collectionId}`, settings)
      .toPromise()
      .then(_ => {});
  }

  public activateStatusPolicy(
    collectionId: string,
    projectId: string,
    repositoryId: string,
    statusPolicyName: string
  ): Promise<void> {
    return this.httpClient
      .post(`${this.apiUrl}${collectionId}/${projectId}/${repositoryId}/${statusPolicyName}`, {})
      .toPromise()
      .then(_ => {});
  }

  public deactivateStatusPolicy(
    collectionId: string,
    projectId: string,
    repositoryId: string,
    statusPolicyName: string
  ): Promise<void> {
    return this.httpClient
      .delete(`${this.apiUrl}${collectionId}/${projectId}/${repositoryId}/${statusPolicyName}`)
      .toPromise()
      .then(_ => {});
  }

  public reactivateProject(collectionId: string, projectId: string): Promise<void> {
    return this.httpClient
      .post(`${this.apiUrl}${collectionId}/${projectId}`, {})
      .toPromise()
      .then(_ => {});
  }

  public getRepositoryStatus(collectionId: string, projectId: string, repositoryId: string): Promise<RepositoryStatus> {
    return this.httpClient
      .get(`${this.apiUrl}${collectionId}/${projectId}/${repositoryId}`)
      .toPromise()
      .then(s => s as RepositoryStatus);
  }

  public getProjectStatus(collectionId: string, projectId: string): Promise<ProjectStatus> {
    return this.httpClient
      .get(`${this.apiUrl}${collectionId}/${projectId}`)
      .toPromise()
      .then(s => s as ProjectStatus);
  }

  public getCollectionStatus(collectionId: string): Promise<CollectionStatus> {
    return this.httpClient
      .get(`${this.apiUrl}${collectionId}`)
      .toPromise()
      .then(s => s as CollectionStatus);
  }
}
