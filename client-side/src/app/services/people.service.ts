import { HttpClient } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { People } from '../people/people.model';

const ROOT_URL:string="https://localhost:5001/odata"; 

@Injectable({
  providedIn: 'root'
})

export class PeopleService {
  constructor(private http: HttpClient) { 
  }

  getPeople() {
    return this.http.get<People[]>(ROOT_URL + '/People')
  }
}
