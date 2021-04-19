import { Component, OnDestroy, OnInit, ViewChild } from '@angular/core';
import { ActivatedRoute, Router } from '@angular/router';
import { NgxGalleryAnimation, NgxGalleryImage, NgxGalleryOptions } from '@kolkov/ngx-gallery';
import { TabDirective, TabsetComponent } from 'ngx-bootstrap/tabs';
import { take } from 'rxjs/operators';
import { Member } from 'src/app/models/member';
import { Message } from 'src/app/models/message';
import { User } from 'src/app/models/users';
import { AccountService } from 'src/app/Services/account.service';
import { MessageService } from 'src/app/Services/message.service';
import { PresenceService } from 'src/app/Services/presence.service';

@Component({
   selector: 'app-member-detail',
   templateUrl: './member-detail.component.html',
   styleUrls: ['./member-detail.component.css']
})
export class MemberDetailComponent implements OnInit, OnDestroy {
   // getting access to tabs by setting a local name to the html component and then using
   // viewchild to bring it into the code.
   @ViewChild('memberTabs', {static: true}) memberTabs: TabsetComponent;
   activeTab: TabDirective;
   member: Member;
   galleryOptions: NgxGalleryOptions[];
   galleryImages: NgxGalleryImage[];
   messages: Message[] = [];
   user: User;

   constructor(public presence: PresenceService,
      private route: ActivatedRoute,
      private router: Router,
      private messageService: MessageService,
      private accountService: AccountService) {
      this.accountService.currentUser$.pipe(take(1)).subscribe(user => this.user = user);
      this.router.routeReuseStrategy.shouldReuseRoute = () => false;
   }

   ngOnInit(): void {
      // gets the data from the route
      this.route.data.subscribe(data => {
         this.member = data.member;
      });

      // if the tab to select is in the query string, use it, otherwise select tab 0
      this.route.queryParams.subscribe(params => {
         params.tab ? this.selectTab(params.tab) : this.selectTab(0);
      });

      this.galleryOptions = [
         {
            width: '500px',
            height: '500px',
            imagePercent: 100,
            thumbnailsColumns: 4,
            imageAnimation: NgxGalleryAnimation.Slide,
            preview: false
         }
      ]
      this.galleryImages = this.getImages();

   }

   getImages(): NgxGalleryImage[] {
      const imageUrls = [];
      for (const photo of this.member.photos) {
         imageUrls.push({
            small: photo?.url,
            medium: photo?.url,
            big: photo?.url
         })
      }
      return imageUrls;
   }

   loadMessages() {
      this.messageService.getMessageThread(this.member.username).subscribe(messages => {
         this.messages = messages;
      })
   }

   selectTab(tabId: number) {
      this.memberTabs.tabs[tabId].active = true;
   }

   onTabActivated(data: TabDirective) {
      this.activeTab = data;
      if (this.activeTab.heading === 'Messages' && this.messages.length === 0) {
         this.messageService.createHubConnection(this.user, this.member.username);
      } else {
         this.messageService.stopHubConnection();
      }
   }

   ngOnDestroy():void {
      this.messageService.stopHubConnection();
   }
}
