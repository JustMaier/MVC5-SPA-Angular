MVC5-SPA-Angular
================

The MVC5 SPA Template Adapted for Angular

##Installation

Clone and start with the project **OR** run `MVC5-SPA-Angular.vsix` and install the project template!

##3rd Party Login Support

Just update `App_Start/Startup.Auth.cs` with the correct information and you'll be on your way!

####Note
Since most third party services don't support using localhost or 127.0.0.1 in the return address the project has been modified to launch as `spaAuth.localtest.me:65489` - You may need to open `%Documents%/IISExpress/config/applicationhost.config` and update your site's binding.

#####Correct Binding Example
```xml
<site name="SPAuth" id="48">
    <application path="/" applicationPool="Clr4IntegratedAppPool">
        <virtualDirectory path="/" physicalPath="C:\Users\Justin\Dropbox\Dev\MVC5-SPA-Angular\SPAuth" />
    </application>
    <bindings>
        <binding protocol="http" bindingInformation="*:65489:spaauth.localtest.me" />
    </bindings>
</site>
```

####Also check out
[angular-spa-security](https://github.com/JustMaier/angular-spa-security) the primary component in this template.