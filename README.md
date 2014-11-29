Glimpse.Orchard
===============

A Glimpse extension for the Orchard CMS. Note- this plugin is currently in active development, and has not yet reached a stable release milestone.

See http://getglimpse.com/ for more information about Glimpse.

## Installation steps 

1. Clone this repo into a folder called `Glimpse.Orchard` in your `Modules` folder.
2. Open your Orchard solution in Visual Stufio, and add the `Glimpse.Orchard` project to your `Modules` folder.
3. The module requires the Glimpse NuGet package to be installed. If you have NuGet package retore enabled, you can just build your solution. If not, then right-click the `Glimpse.Orchard` project in the Solution Explorer window in Visual Studio, click `Manage NuGet Packages...` and then click the `Resore` button. This will download and install the require Glimpse packages into the module.
4. Next, add the Glimpse.Core NuGet package to your `Orchard.Web` project. The easiest way to do this is to right-click on the solution name in your Solution Explorer window in visual studio, select `Manage NuGet Packages For Solution...` to open the Package Manager Diaglog, select `Installed Packages` and then click the `Manage` button next to the `Glimpse Core` package. Find the `Orchard.Web` project in the `Select Projects` window, and ensure that the checkbox next to it is ticked. Do the same with the `Glimpse ASP.Net` package.
5. Finally, you need to slightly re-arrange the `Web.config` file in your `Orchard.Web` project as Orchard out of the box has a catch all route that will take precedence over your Glimpse routes:
  * Find the line `<add name="Glimpse" path="glimpse.axd" verb="GET" type="Glimpse.AspNet.HttpHandler, Glimpse.AspNet" preCondition="integratedMode" />` in the `handlers` section, and move it above the line `<add name="NotFound" path="*" verb="*" type="System.Web.HttpNotFoundHandler" preCondition="integratedMode" requireAccess="Script" />`

## Testing

You should now be able to navigate to `~/glimpse.axd` and see the Glimpse Configuration Page. If not, then you should check the instructions above for steps you may have missed or got wrong.

You should also be able to enable Glimpse, and then see the Glimpse bar appear as you navigate around your site.

However, you will not be able to see any Orchard specific tabs yet. To enable these tabs, you need to enable the `Glimpse for Orchard` feature.

## Enabling Orchard specific tabs
