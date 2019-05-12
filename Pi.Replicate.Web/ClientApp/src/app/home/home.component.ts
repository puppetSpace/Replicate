import { Component } from '@angular/core';
import { FolderDetail } from '../models/folderModels';


@Component({
  selector: 'app-home',
  templateUrl: './home.component.html',
  styleUrls: ['./home.component.css']
})
export class HomeComponent {
  folderDetails: FolderDetail[];

  constructor() {
    this.folderDetails = [
      {name:'Test1', amountOfFiles:4, bytesDownloaded: 400, bytesUploaded: 890},
      {name:'Test2', amountOfFiles:1004, bytesDownloaded: 400, bytesUploaded: 890},
      {name:'Test3', amountOfFiles:0, bytesDownloaded: 0, bytesUploaded: 0},
      {name:'Test4', amountOfFiles:5, bytesDownloaded: 400, bytesUploaded: 890},
      {name:'Test5', amountOfFiles:300, bytesDownloaded: 10004, bytesUploaded: 10004},
    ]
  }
}
