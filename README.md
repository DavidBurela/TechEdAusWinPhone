Microsoft TechEd Australia - Windows Phone 7 app
=============

A companion application for attendees of Microsoft TechEd Australia in 2012.  It is based on the "Starter kit for conferences"  [http://smallandmighty.net/blog/windows-phone-starter-kit-for-conferences](http://smallandmighty.net/blog/windows-phone-starter-kit-for-conferences)

The application was simplified greatly by replacing the custom logic for grouping and sorting, and instead replacing it with the [NetAdvantage controls for Windows Phone](http://www.infragistics.com/products/windows-phone) which support this functionality directly. This also sped up the loading and scrolling of the list dramatically.
I also added functionality to cache the OData results for 2 hours.

Features
--------
* Search through session code, title &amp; abstract to find exactly what you are looking for!
* Sessions by timeslot, easily discover which sessions are on right now
* Save sessions to favourites
* Sessions cached for 2 hours to reduce network bandwidth

Downloading
--------
The 2013 version will be available on the marketplace after certification.

You can download the .xap right now to sideload onto your device https://github.com/DavidBurela/TechEdAusWinPhone/releases

How I built it
--------
I have written a post-mortem on how the application was built, how I modified the original framework, and what I learned from the experience. You can read it here on my blog [http://davidburela.wordpress.com/2012/09/18/post-mortem-of-the-microsoft-teched-wp7-app/](http://davidburela.wordpress.com/2012/09/18/post-mortem-of-the-microsoft-teched-wp7-app/)

Packages used
--------
* Windows Phone Starter Kit for Conferences - [http://smallandmighty.net/blog/windows-phone-starter-kit-for-conferences](http://smallandmighty.net/blog/windows-phone-starter-kit-for-conferences)
* YLAD (Your Last About Dialog) - [http://ylad.codeplex.com/](http://ylad.codeplex.com/)
* Windows Phone Icons Maker [http://wpiconmaker.codeplex.com/](http://wpiconmaker.codeplex.com/)

Screenshots
--------
![Schedule](marketplace%20submission/screenshots/schedule%20grouping.png)

[More Screenshots](https://github.com/DavidBurela/TechEdAusWinPhone/tree/master/marketplace%20submission/screenshots)


Notes
--------
The [NetAdvantage controls for Windows Phone](http://www.infragistics.com/products/windows-phone) were used to build this application. A license is required to build the source code. However you can download a trial to play with it [http://www.infragistics.com/products/windows-phone/downloads](http://www.infragistics.com/products/windows-phone/downloads)

License
--------
The code is MS-LPL as that is what the original framework I used was licensed as.
