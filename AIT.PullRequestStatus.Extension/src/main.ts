import { enableProdMode } from "@angular/core";
import { platformBrowserDynamic } from "@angular/platform-browser-dynamic";

import { environment } from "./environments/environment";
import { ProjectAdminModule } from "./app/project-admin/project-admin.module";
import { CollectionAdminModule } from "./app/collection-admin/collection-admin.module";

if (environment.production) {
  enableProdMode();
}

// TODO: Find a better solution to do this.
// I'd love to have multiple entry points
// but webpack ng aot doesn't support this out of the box at the moment.
// Pull request for this feature is pending, so we will see.
const modules = {
  project: ProjectAdminModule,
  collection: CollectionAdminModule
};

platformBrowserDynamic()
  .bootstrapModule(modules[(window as any).ngModule])
  .catch(err => console.log(err));
