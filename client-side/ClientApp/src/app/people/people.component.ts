import { Component, OnInit } from '@angular/core';
import { PeopleService } from '../services/people.service';
import { People } from './people.model';

@Component({
  selector: 'app-people',
  templateUrl: './people.component.html',
  styleUrls: ['./people.component.scss']
})

export class PeopleComponent implements OnInit {

  people: People[] = [];

  constructor(private peopleService: PeopleService) {
  }

  ngOnInit() {
    this.loadPeople();
  }

  loadPeople() {
    this.peopleService.getPeople().subscribe((data) => {
      const result = data["value"];
      result.map(r => {
        this.people.push({
          id: r.PersonID,
          firstName: r.FirstName,
          lastName: r.LastName,
        })
      })
    });
  }
}
