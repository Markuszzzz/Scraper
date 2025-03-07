using cSharpScraper.Reconnaisance.SiteArchive;

var arvhiedUrlCollect = new ArchivedUrlCollector(null);


var list = arvhiedUrlCollect.GetArchivedUrlsForDomain("www.wellsfargo.com");
