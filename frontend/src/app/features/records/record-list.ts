import { Component } from '@angular/core';
import { MatButtonModule } from '@angular/material/button';
import { MatCardModule } from '@angular/material/card';

@Component({
  selector: 'app-record-list',
  imports: [MatButtonModule, MatCardModule],
  templateUrl: './record-list.html',
  styleUrl: './record-list.scss'
})
export class RecordList {}
