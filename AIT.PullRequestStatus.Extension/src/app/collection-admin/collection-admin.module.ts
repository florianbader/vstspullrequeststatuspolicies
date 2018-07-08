import { BrowserModule } from "@angular/platform-browser";
import { NgModule } from "@angular/core";
import { HttpClientModule } from "@angular/common/http";
import { FormsModule } from "@angular/forms";
import { CollectionAdminComponent } from "./collection-admin.component";
import { ConfigurationService } from "../shared/configuration.service";

import "VSS/Controls";

@NgModule({
  declarations: [CollectionAdminComponent],
  imports: [BrowserModule, HttpClientModule, FormsModule],
  providers: [ConfigurationService],
  bootstrap: [CollectionAdminComponent]
})
export class CollectionAdminModule {}
