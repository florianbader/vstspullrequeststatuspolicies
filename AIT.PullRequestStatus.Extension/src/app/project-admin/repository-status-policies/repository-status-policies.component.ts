import { ConfigurationService } from "./../../shared/configuration.service";
import { Component, OnInit, Input, ElementRef, ViewChild, EventEmitter, Output } from "@angular/core";

import { create } from "VSS/Controls";
import { IWaitControlOptions, WaitControl } from "VSS/Controls/StatusIndicator";

@Component({
  selector: "app-repository-status-policies",
  templateUrl: "./repository-status-policies.component.html",
  styleUrls: ["./repository-status-policies.component.css"]
})
export class RepositoryStatusPoliciesComponent implements OnInit {
  private repositoryIdValue: string;
  private waitControl: WaitControl;
  private webContext: WebContext;

  public set isBusy(value: boolean) {
    this.isBusyChanged.emit(value);
  }

  public statusPolicyDisabled = {};
  public statusPolicies: RepositoryStatusPolicy[];

  @ViewChild("waitContainerElement") public waitContainerElement: ElementRef;

  @Input()
  public set repositoryId(value) {
    this.repositoryIdValue = value;

    if (this.repositoryId == null) {
      this.statusPolicies = [];
    } else {
      this.readRepository();
    }
  }

  public get repositoryId() {
    return this.repositoryIdValue;
  }

  @Output() isBusyChanged = new EventEmitter<boolean>();

  constructor(private configurationService: ConfigurationService) {
    this.webContext = VSS.getWebContext();
  }

  public statusPolicyChanged(statusPolicyId: string, value: boolean) {
    this.statusPolicyDisabled[statusPolicyId] = true;
    this.isBusy = true;

    let promise;

    if (value) {
      promise = this.configurationService.activateStatusPolicy(
        this.webContext.collection.id,
        this.webContext.project.id,
        this.repositoryId,
        statusPolicyId
      );
    } else {
      promise = this.configurationService.deactivateStatusPolicy(
        this.webContext.collection.id,
        this.webContext.project.id,
        this.repositoryId,
        statusPolicyId
      );
    }

    promise.finally(() => {
      this.statusPolicyDisabled[statusPolicyId] = false;
      this.isBusy = false;
    });
  }

  public ngOnInit() {
    const waitControlOptions: IWaitControlOptions = {
      message: "Loading status policies..."
    };
    this.waitControl = create(WaitControl, this.waitContainerElement.nativeElement, waitControlOptions);
  }

  private async readRepository() {
    if (this.waitControl != null) {
      this.waitControl.startWait();
    }

    try {
      this.statusPolicies = (await this.configurationService.getRepositoryStatus(
        this.webContext.collection.id,
        this.webContext.project.id,
        this.repositoryId
      )).statusPolicies;
    } finally {
      if (this.waitControl != null) {
        this.waitControl.endWait();
      }
    }
  }
}
