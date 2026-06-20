import { Directive, Input, TemplateRef, ViewContainerRef, inject, OnInit } from '@angular/core';
import { AuthStore } from '../../core/auth/auth.store';

@Directive({
  selector: '[hasRole]',
  standalone: true
})
export class HasRoleDirective implements OnInit {
  @Input('hasRole') roles: string | string[] = [];

  private readonly tpl = inject(TemplateRef<unknown>);
  private readonly vcr = inject(ViewContainerRef);
  private readonly store = inject(AuthStore);

  ngOnInit(): void {
    const required = Array.isArray(this.roles) ? this.roles : [this.roles];
    const user     = this.store.user();
    const hasRole  = user ? required.some(r => user.roles.includes(r)) : false;
    if (hasRole) this.vcr.createEmbeddedView(this.tpl);
    else         this.vcr.clear();
  }
}
