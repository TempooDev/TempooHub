import { Component, effect, inject, signal, Signal } from '@angular/core';
import { Navigation } from "../navigation/navigation";
import { AvatarModule } from 'primeng/avatar';
import { BadgeDirective, BadgeModule } from "primeng/badge";
import { OverlayBadgeModule } from "primeng/overlaybadge";
import { KEYCLOAK_EVENT_SIGNAL, KeycloakEventType, typeEventArgs, ReadyArgs } from 'keycloak-angular';
import Keycloak from 'keycloak-js';
import keycloak from 'keycloak-js';

interface UserProfile {
  name: string;
  email: string;
  role?: string;
}

@Component({
  selector: 'th-header',
  imports: [AvatarModule, BadgeModule, OverlayBadgeModule],
  templateUrl: './header.html',
  styles: ``,
})
export class Header {
  authenticated = false;
  user = signal<UserProfile | null>(null);
  keycloakStatus: string | undefined;
  private readonly keycloak = inject(Keycloak);
  private readonly keycloakSignal = inject(KEYCLOAK_EVENT_SIGNAL);

  async ngOnInit(): Promise<void> {
    const profile = await this.keycloak.loadUserProfile();
    console.log('User profile loaded:', profile);
    this.user.set(this.keycloak.authenticated ? {
      name: profile?.firstName || '',
      email: profile?.email || '',
    } : null);
  }

  constructor() {
    effect(() => {
      const keycloakEvent = this.keycloakSignal();

      this.keycloakStatus = keycloakEvent.type;

      if (keycloakEvent.type === KeycloakEventType.Ready) {
        this.authenticated = typeEventArgs<ReadyArgs>(keycloakEvent.args);
      }

      if (keycloakEvent.type === KeycloakEventType.AuthLogout) {
        this.authenticated = false;
      }
    });
  }

  login() {
    this.keycloak.login();
  }

  logout() {
    this.keycloak.logout();
  }
}
