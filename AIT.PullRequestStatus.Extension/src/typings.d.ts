/// <reference types="vss-web-extension-sdk" />

/* SystemJS module definition */
declare var module: NodeModule;

interface NodeModule {
  id: string;
}

declare class AccountConfiguration {
  baseUrl: string;
  collectionId: string;
  personalAccessToken: string;
}

declare class RepositoryStatus {
  isActivated: boolean;
  statusPolicies: RepositoryStatusPolicy[];
}

declare class RepositoryStatusPolicy {
  id: string;
  name: string;
  description: string;
  isActivated: boolean;
}

declare class CollectionStatus {
  isActivated: boolean;
}

declare class ProjectStatus {
  hasBrokenServiceHooks: boolean;
}

declare class ExtensionConfiguration {
  serverUrl: string;
}