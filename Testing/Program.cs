using cSharpScraper.Reconnaisance.SiteArchive;

var arvhiedUrlCollect = new ArchivedUrlCollector();


var list = arvhiedUrlCollect.GetArchivedUrlsForDomain("www.wellsfargo.com");
