import { Component, EventEmitter, Input, OnInit } from '@angular/core';
import { BsModalRef } from 'ngx-bootstrap/modal';
import { User } from 'src/app/models/users';

@Component({
   selector: 'app-roles-modal',
   templateUrl: './roles-modal.component.html',
   styleUrls: ['./roles-modal.component.css']
})
export class RolesModalComponent implements OnInit {
   @Input() updateSelectedRoles = new EventEmitter();
   user: User;
   roles: any[];

   constructor(public bsModalRef: BsModalRef) { }

   ngOnInit(): void {
   }

   updateRoles() {
      // raise an event and pass roles 
      this.updateSelectedRoles.emit(this.roles);
      this.bsModalRef.hide();
   }
}
