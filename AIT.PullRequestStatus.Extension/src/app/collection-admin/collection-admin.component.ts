import { Component, OnInit, ElementRef, ViewChild } from "@angular/core";
import { ConfigurationService } from "../shared/configuration.service";

@Component({
  selector: "app-collection-admin",
  templateUrl: "./collection-admin.component.html",
  styleUrls: ["./collection-admin.component.css"]
})
export class CollectionAdminComponent implements OnInit {
  private webContext: WebContext;

  public collectionStatus: CollectionStatus;
  public settings = {} as AccountConfiguration;
  public isBusy = false;

  @ViewChild("patElement") public patElement: ElementRef;
  @ViewChild("patTooltipElement") public patTooltipElement: ElementRef;

  constructor(private configurationService: ConfigurationService) {}

  public ngOnInit() {
    this.webContext = VSS.getWebContext();

    (window as any).tippy(this.patElement.nativeElement, {
      trigger: "click",
      theme: "light",
      performance: true,
      placement: "right",
      offset: "0,180",
      interactive: true,
      arrow: true,
      html: this.patTooltipElement.nativeElement,
      onShow(instance) {
        setTimeout(() => {
          instance.popper.style.top = "180px";

          (window as any).tippy(".tooltip", {
            theme: "light",
            arrow: true,
            size: "small"
          });
        }, 200);
      }
    });

    (window as any).tippy(".tooltip", {
      theme: "light",
      arrow: true,
      size: "small"
    });

    this.init();
  }

  public save() {
    this.isBusy = true;
    this.configurationService
      .activateCollection(this.settings)
      .then(
        async () =>
          (this.collectionStatus = await this.configurationService.getCollectionStatus(this.webContext.collection.id))
      )
      .finally(() => (this.isBusy = false));
  }

  private async init() {
    this.settings = {
      personalAccessToken: "*".repeat(32),
      baseUrl: this.webContext.host.uri,
      collectionId: this.webContext.collection.id
    };

    this.collectionStatus = await this.configurationService.getCollectionStatus(this.webContext.collection.id);
  }
}
