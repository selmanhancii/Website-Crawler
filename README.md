<h3 align="center">Website Crawler</h3>
  <p align="center">
    Tretton37 coding assignment that aims to download the www.tretton37.com website with a console application
    <br />
    <a href="https://github.com/selmanhancii/Website-Crawler"><strong>Explore the docs »</strong></a>
    <br />
    <br />
    <a href="https://github.com/selmanhancii/Website-Crawler/issues">Report Bug</a>
    ·
    <a href="https://github.com/selmanhancii/Website-Crawler/issues">Request Feature</a>
  </p>
</div>

<details>
  <summary>Table of Contents</summary>
  <ol>
    <li>
      <a href="#about-the-project">About The Project</a>
      <ul>
        <li><a href="#built-with">Built With</a></li>
      </ul>
    </li>
    <li><a href="#usage">Usage</a></li>
     <li><a href="#how-to-run">How To Run?</a></li>
    <li><a href="#how-does-it-work">How Does It Work?</a></li>
    <li><a href="#contact">Contact</a></li>
  </ol>
</details>

## About The Project

WebsiteCrawler is a project designed and developed with the aim of downloading a website from given url to local disk and accessing sub pages and assets locally. In order to have a better understanding of the website, main and sub pages are placed with the same structure locally. Although the main idea is to download www.tretton37.com, it is possible to download other websites with only changing the url.
WebsiteCrawler is developed in .NET environment with C# programming language. It is possible to download and run the application using any C# IDE. 

<p align="right">(<a href="#top">back to top</a>)</p>

### Built With

* [.NET](https://dotnet.microsoft.com/en-us/)

<p align="right">(<a href="#top">back to top</a>)</p>

## Usage

WebsiteCrawler is a project that reads only one parameter which is url of the website that is planned to be downloaded. Once the application is started, url is gathered from app.config file and downloading process started. If the url file is not empty, there are not any requirements that user needs to be done. 

<p align="right">(<a href="#top">back to top</a>)</p>

## How To Run?

WebsiteCrawler project can be cloned from github and downloaded to any computer. In order to run the application, any IDE of C# can be used(Visual Studio, Rider etc.). After cloning the code, it is possible to open the project with preferred IDE and run the code. 

<p align="right">(<a href="#top">back to top</a>)</p>

## How Does It Work?

1) Once the application is started, terminal gets open and date of starting is displayed
2) Url is gathered from app.config file and processed
3) Urls page content is fetched using HttpWebRequest
4) Download file to store the web pages gets created
5) .html extensioned file is created and content gets written
6) Links are searched across the content and gets filtered
7) If the link is external, nothing needs to be done
8) If the link is internal and there is an asset, asset is fetched from website and gets stored locally
9) If the link is internal and there is a link to a sub page, new page url is sent to the step 3 and application runs recursively.

<p align="right">(<a href="#top">back to top</a>)</p>

## Contact

Selman Hancı  -  selman_hanci@hotmail.com  -  https://www.linkedin.com/in/selman-hanci/

<p align="right">(<a href="#top">back to top</a>)</p>
