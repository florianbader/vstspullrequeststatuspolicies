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
  public extensionConfiguration = {} as ExtensionConfiguration;
  public isBusy = false;

  @ViewChild("patElement") public patElement: ElementRef;
  @ViewChild("patTooltipElement") public patTooltipElement: ElementRef;

  @ViewChild("urlElement") public urlElement: ElementRef;
  @ViewChild("urlTooltipElement") public urlTooltipElement: ElementRef;

  constructor(private configurationService: ConfigurationService) { }

  public ngOnInit() {
    this.webContext = VSS.getWebContext();

    this.createTooltip(this.patElement.nativeElement, this.patTooltipElement.nativeElement);
    this.createTooltip(this.urlElement.nativeElement, this.urlTooltipElement.nativeElement);

    (window as any).tippy(".tooltip", {
      theme: "light",
      arrow: true,
      size: "small"
    });

    this.init();
  }

  public async save() {
    this.isBusy = true;

    try {
      await this.configurationService.updateExtensionConfiguration(this.extensionConfiguration);
      await this.configurationService.activateCollection(this.settings);
      this.collectionStatus = await this.configurationService.getCollectionStatus(this.webContext.collection.id);
    } finally {
      this.isBusy = false;
    }
  }

  private createTooltip(element, tooltipElement) {
    (window as any).tippy(element, {
      trigger: "click",
      theme: "light",
      performance: true,
      placement: "right",
      offset: "0,180",
      interactive: true,
      arrow: true,
      html: tooltipElement,
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
  }

  private async init() {
    this.settings = {
      personalAccessToken: "*".repeat(32),
      baseUrl: this.webContext.host.uri,
      collectionId: this.webContext.collection.id
    };

    this.extensionConfiguration = await this.configurationService.getExtensionConfiguration();

    this.collectionStatus = await this.configurationService.getCollectionStatus(this.webContext.collection.id);
  }
}
