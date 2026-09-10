import { NgModule } from '@angular/core';
import { RouterModule, Routes } from '@angular/router';
import { LoginComponent } from './pages/login/login.component';
import { DashboardComponent } from './pages/dashboard/dashboard.component';
import { AdminDashboardComponent } from './pages/admin-dashboard/admin-dashboard.component';
import { ActivationComponent } from './pages/activation/activation.component';
import { NotGirisComponent } from './pages/not-giris/not-giris.component';
import { OgrenciNotlarComponent } from './pages/ogrenci-notlar/ogrenci-notlar.component';
import { DevamsizlikComponent } from './pages/devamsizlik/devamsizlik.component';
import { adminGuard, authGuard, lecturerGuard, studentGuard } from './guards/role.guard';

// Uygulama sayfaları
const routes: Routes = [
  { path: '', redirectTo: 'login', pathMatch: 'full' },
  { path: 'login', component: LoginComponent },
  { path: 'aktivasyon', component: ActivationComponent },
  { path: 'anasayfa', component: DashboardComponent, canActivate: [authGuard] },
  { path: 'admin-dashboard', component: AdminDashboardComponent, canActivate: [authGuard, adminGuard] },
  { path: 'not-giris', component: NotGirisComponent, canActivate: [authGuard, lecturerGuard] },
  { path: 'notlarim', component: OgrenciNotlarComponent, canActivate: [authGuard, studentGuard] },
  { path: 'devamsizlik', component: DevamsizlikComponent, canActivate: [authGuard, studentGuard] },
  { path: '**', redirectTo: 'login' }
];

@NgModule({
  imports: [RouterModule.forRoot(routes)],
  exports: [RouterModule]
})
export class AppRoutingModule { }
