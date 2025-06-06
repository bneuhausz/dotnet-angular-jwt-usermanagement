import { Directive, effect, inject, input, TemplateRef, ViewContainerRef } from "@angular/core";
import { AuthService } from "../data-access/auth.service";

@Directive({
  selector: '[hasPermission]',
})
export class HasPermissionDirective {
  readonly permission = input.required<string>({ alias: 'hasPermission' });

  private readonly templateRef = inject(TemplateRef);
  private readonly viewContainer = inject(ViewContainerRef);
  private readonly authService = inject(AuthService);

  constructor() {
    effect(() => {
      const user = this.authService.user();
      const requiredPermission = this.permission();
      const hasPermission = !!user && user.permissions.includes(requiredPermission);

      this.viewContainer.clear();
      if (hasPermission) {
        this.viewContainer.createEmbeddedView(this.templateRef);
      }
    });
  }
}