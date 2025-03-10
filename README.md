## 🚀 C# Web Scraper

A high-performance concurrent web scraper written in C# that recursively scrapes web pages, subdomains, and hidden URLs using advanced discovery techniques like Google Dorking and Wayback URLs.

## 📌 Overview

This project is a powerful and scalable web scraper designed to efficiently gather data from a target domain and its subdomains. It leverages concurrency to maximize performance and minimize scraping time.

The scraper starts by crawling the target domain, identifies subdomains, and scrapes each subdomain concurrently for improved speed. It also uses Google Dorking and Wayback URLs to uncover hidden or hard-to-find pages that may not be accessible through traditional crawling.

For dynamic content and JavaScript-heavy websites, Selenium is used to execute scripts and retrieve fully rendered pages.

All collected data is stored in a MySQL database using Entity Framework for easy access and management.

## 🌟 Key Features
* ✅ Concurrent Scraping – Utilizes multi-threading to scrape multiple pages and subdomains simultaneously for faster results.
* ✅ Subdomain Discovery – Automatically detects and scrapes subdomains to gather additional content.
* ✅ Google Dorking – Employs advanced search operators to find hidden URLs.
* ✅ Wayback Machine Support – Leverages archived pages to uncover additional resources.
* ✅ Entity Framework Integration – Stores data in a MySQL database using EF for structured and efficient data handling.
* ✅ Selenium Support – Handles JavaScript-heavy websites by executing scripts and scraping rendered pages.
* ✅ Scalable Design – Designed to handle large-scale websites with thousands of pages and subdomains.

## How It Works 🏗️ 

&nbsp;&nbsp;&nbsp;&nbsp;1. Start with a target domain – The scraper begins by crawling the target domain to collect initial URLs.  
&nbsp;&nbsp;&nbsp;&nbsp;2. Identify Subdomains – The scraper searches for subdomains and adds them to the crawl queue.  
&nbsp;&nbsp;&nbsp;&nbsp;3. Concurrent Scraping – Each subdomain is scraped concurrently using multi-threading for maximum efficiency.  
&nbsp;&nbsp;&nbsp;&nbsp;4. Advanced Discovery  
&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp; **Google Dorking** – Identifies additional URLs.  
&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp; **Wayback Machine** – Finds historical pages that may no longer be available.  
&nbsp;&nbsp;&nbsp;&nbsp;5. Dynamic Content Handling – Uses Selenium to handle JavaScript-heavy pages and retrieve fully rendered content.  
&nbsp;&nbsp;&nbsp;&nbsp;6. Store Data – Collected data is stored in a MySQL database using Entity Framework.  
&nbsp;&nbsp;&nbsp;&nbsp;7. Repeat – The process repeats recursively until no new pages or subdomains are discovered.  


## 🛠️ Tech Stack
* Programming Language: C#
* Framework: .NET 8.0
* Database: MySQL
* ORM: Entity Framework
* Libraries:
* HtmlAgilityPack – HTML parsing
* AngleSharp – DOM manipulation
* Selenium – Handling dynamic content and JavaScript-heavy websites
* WebDriver – Automating browser interaction
* EFCore.BulkExtensions – High-performance data insertion

## 🚨 Challenges and Solutions
* ✅ Handling Large Websites – The scraper is optimized to handle large-scale websites by using concurrency and bulk insertion into a MySQL DB.
* ✅ Rate Limits – Throttling is used to avoid overloading the server.
* ✅ Dynamic Content – JavaScript-heavy sites are processed using Selenium and WebDriver.

