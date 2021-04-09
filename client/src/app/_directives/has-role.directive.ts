import { OnInit } from '@angular/core';
import { Directive, Input, TemplateRef, ViewContainerRef } from '@angular/core';
import { take } from 'rxjs/operators';
import { User } from '../models/users';
import { AccountService } from '../Services/account.service';

@Directive({
   selector: '[appHasRole]' //*appHasRole '["Role1", "Role2"]' - uses * because this is a structural directive
})
export class HasRoleDirective implements OnInit {
   @Input() appHasRole: string[]
   user: User;

   constructor(private viewContainerRef: ViewContainerRef,
      private templateRef: TemplateRef<any>,
      private accountService: AccountService) {
      this.accountService.currentUser$.pipe(take(1)).subscribe(user => {
         this.user = user;
      })
   }

   ngOnInit(): void {
      // Clear the view it the user has no roles
      if (!this.user?.roles || this.user == null) {
         this.viewContainerRef.clear();
         return;
      }

      // if the user is in a specified role
      if (this.user?.roles.some(r => this.appHasRole.includes(r))) {
         // create the view from the template reference
         this.viewContainerRef.createEmbeddedView(this.templateRef);
      } else {
         // otherwise clear the template reference
         this.viewContainerRef.clear();
      }
   }
}
