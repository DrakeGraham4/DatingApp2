import { Injectable } from '@angular/core';
import { ActivatedRouteSnapshot, CanDeactivate, RouterStateSnapshot, UrlTree } from '@angular/router';
import { Observable } from 'rxjs';
import { MemberEditComponent } from '../members/member-edit/member-edit.component';

@Injectable({
  providedIn: 'root'
})
export class PreventUnsavedChangesGuard implements CanDeactivate<unknown> {
  //Gives access to edit form, makes sure you get an alert and have to say yes before moving to a different page
  canDeactivate(component: MemberEditComponent): boolean {
    if (component.editForm.dirty) {
      return confirm('Are you sure you want to continue? Any unsaved changed will be lost');
    }
    return true;
  }
  
}
