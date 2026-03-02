import { Component } from '@angular/core';

export interface AboutLibrary {
  name: string;
  version: string;
}

@Component({
  selector: 'app-about-libraries',
  standalone: true,
  imports: [],
  templateUrl: './about-libraries.component.html',
})
export class AboutLibrariesComponent {
  libraries: AboutLibrary[] = [
    { name: '@angular/core', version: '^21.1.0' },
    { name: '@angular/common', version: '^21.1.0' },
    { name: '@angular/router', version: '^21.1.0' },
    { name: '@angular/forms', version: '^21.1.0' },
    { name: 'bootstrap', version: '^5.3.8' },
    { name: 'rxjs', version: '~7.8.0' },
    { name: 'tslib', version: '^2.3.0' },
    { name: '.Net', version: '10.0.0' },
    { name: 'AutoMapper', version: '16.0.0' },
    { name: 'AutoMapper.Extensions.Microsoft.DependencyInjection', version: '12.0.0' },
    { name: 'BCrypt.Net-Next', version: '4.0.3' },
    { name: 'FluentValidation', version: '12.1.1' },
    { name: 'MediatR', version: '14.0.0' },
    { name: 'Microsoft.AspNetCore.Authentication.JwtBearer', version: '10.0.2' },
    { name: 'Microsoft.AspNetCore.OpenApi', version: '10.0.1' },
    { name: 'Microsoft.EntityFrameworkCore.Design', version: '10.0.2' },
    { name: 'Microsoft.EntityFrameworkCore.Sqlite', version: '10.0.2' },
    { name: 'Newtonsoft.Json', version: '13.0.4' },
    { name: 'Serilog', version: '4.3.0' },
    { name: 'Serilog.AspNetCore', version: '10.0.0' },
    { name: 'Serilog.Settings.Configuration', version: '10.0.0' },
    { name: 'Serilog.Sinks.Console', version: '6.1.1' },
    { name: 'Serilog.Sinks.File', version: '7.0.0' },
    { name: 'System.IdentityModel.Tokens.Jwt', version: '8.15.0' },
    { name: 'System.Text.Encoding.Web', version: '4.3.0' },
    { name: 'System.Text.Json', version: '8.0.0' },
    { name: 'System.Threading.Tasks.Extensions', version: '4.5.4' },
    { name: 'System.Xml.XmlSerializer', version: '4.3.0' },
    { name: 'System.Xml.XPath.XmlDocument', version: '4.3.0' },
    { name: 'System.Xml.XPath', version: '4.3.0' },
    { name: 'System.Xml.XPath.XDocument', version: '4.3.0' },
    { name: 'System.Xml.XPath.XDocument', version: '4.3.0' },
  ];
}
