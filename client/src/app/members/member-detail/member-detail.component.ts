import { Component, OnInit, ViewChild } from '@angular/core';
import { ActivatedRoute } from '@angular/router';
import { NgxGalleryAnimation, NgxGalleryImage, NgxGalleryOptions } from '@kolkov/ngx-gallery';
import { TabDirective, TabsetComponent } from 'ngx-bootstrap/tabs';
import { Member } from 'src/app/models/member';
import { Message } from 'src/app/models/message';
import { MembersService } from 'src/app/Services/members.service';
import { MessageService } from 'src/app/Services/message.service';

@Component({
   selector: 'app-member-detail',
   templateUrl: './member-detail.component.html',
   styleUrls: ['./member-detail.component.css']
})
export class MemberDetailComponent implements OnInit {
   // getting access to tabs by setting a local name to the html component and then using
   // viewchild to bring it into the code.
   @ViewChild('memberTabs') memberTabs: TabsetComponent;
   activeTab: TabDirective;
   member: Member;
   galleryOptions: NgxGalleryOptions[];
   galleryImages: NgxGalleryImage[];
   messages: Message[] = [];

   constructor(private memberService: MembersService,
      private route: ActivatedRoute,
      private messageService: MessageService) { }

   ngOnInit(): void {
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
         this.loadMessages();
      }
   }

}
