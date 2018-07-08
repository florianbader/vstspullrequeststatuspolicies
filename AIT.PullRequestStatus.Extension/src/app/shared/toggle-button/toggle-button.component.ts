import { Component, OnInit, Input, Output, EventEmitter } from "@angular/core";

@Component({
  selector: "app-toggle-button",
  templateUrl: "./toggle-button.component.html",
  styleUrls: ["./toggle-button.component.css"]
})
export class ToggleButtonComponent {
  private isCheckedValue: boolean;

  @Input() public disabled: boolean;

  @Input()
  public set isChecked(value: boolean) {
    this.isCheckedValue = value;
  }

  public get isChecked() {
    return this.isCheckedValue;
  }

  @Output() isCheckedChange = new EventEmitter<boolean>();

  public get label() {
    return this.isCheckedValue ? "On" : "Off";
  }

  public toggle() {
    this.isCheckedValue = !this.isCheckedValue;
    this.isCheckedChange.emit(this.isCheckedValue);
  }
}
