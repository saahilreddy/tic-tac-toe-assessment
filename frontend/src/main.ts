import { bootstrapApplication } from '@angular/platform-browser';
import { provideHttpClient, withInterceptors } from '@angular/common/http';
import { AppComponent } from './app/app.component';
import { errorInterceptor } from './app/error.interceptor';

bootstrapApplication(AppComponent, {
  providers: [provideHttpClient(withInterceptors([errorInterceptor]))]
}).catch((err: unknown) => console.error(err));
