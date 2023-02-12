Select  * from ScraperVisits with(nolock)
order by Id desc


select * from ScraperVisits with(nolock) where NeedToNotify = 1 and Notified = 1


select * from [ApplicationLogs] where [StatusCode] = 429

Select count(*) from ScraperVisits with(nolock)
where AmazonPriceNotFoundReason = 3 or TrendyolPriceNotFoundReason = 4
--and JobId = '844519c5-7c6d-4f6c-a3db-3ee0b01fd0a2'


select * from ApplicationLogs a with(nolock) where a.ProductId = 727
order by Id desc


truncate table [dbo].[ScraperVisits]
truncate table [dbo].[ApplicationLogs]
--truncate table [dbo].[CookieStores]
truncate table [dbo].[AppLogs]

select top 10 * from ApplicationLogs with(nolock)
order by Id desc


select * from [CookieStores]

 



SELECT distinct sv.JobId FROM  ScraperVisits sv WITH(NOLOCK)
ORDER BY sv.Id DESC

SELECT sv.JobId, p.Id as ProductId, p.[Name] as ProductName, p.IsDeleted AS IsProductActive, sv.Id AS VisitId, 
	CASE sv.[AmazonPriceNotFoundReason] 
		WHEN 0 THEN 'Initial' 
		WHEN 1 THEN 'PriceIsNotOnThePage' 
		WHEN 2 THEN 'ExceptionOccured' 
		WHEN 3 THEN 'BotDetected' 
		WHEN 4 THEN 'Found'
	END AS AmazonPriceNotFoundReason,
	CASE sv.[TrendyolPriceNotFoundReason] 
		WHEN 0 THEN 'Initial' 
		WHEN 1 THEN 'PriceIsNotOnThePage' 
		WHEN 2 THEN 'ExceptionOccured' 
		WHEN 3 THEN 'BotDetected' 
		WHEN 4 THEN 'TooManyRequest'
		WHEN 5 THEN 'Found'
	END AS [TrendyolPriceNotFoundReason]
FROM Products p WITH(NOLOCK)
LEFT JOIN ScraperVisits sv WITH(NOLOCK) on p.Id = sv.ProductId and JobId IN (SELECT sv1.JobId FROM ScraperVisits sv1 WITH(NOLOCK) WHERE sv1.JobId IS NOT NULL GROUP BY sv1.JobId)
ORDER BY sv.JobId, P.[Name]

SELECT 
	sv.JobId,
	sv.[AmazonPriceNotFoundReason] AS AmazonPriceNotFoundReasonCode,
	CASE sv.[AmazonPriceNotFoundReason] 
		WHEN 0 THEN 'Initial' 
		WHEN 1 THEN 'PriceIsNotOnThePage' 
		WHEN 2 THEN 'ExceptionOccured' 
		WHEN 3 THEN 'BotDetected' 
		WHEN 4 THEN 'TooManyRequest'
		WHEN 5 THEN 'Found'
	END AS AmazonPriceNotFoundReasonText,
	COUNT(sv.[AmazonPriceNotFoundReason]) AS [Count]
FROM Products p WITH(NOLOCK)
LEFT JOIN ScraperVisits sv WITH(NOLOCK) on p.Id = sv.ProductId and JobId IN (SELECT sv1.JobId FROM ScraperVisits sv1 WITH(NOLOCK) WHERE sv1.JobId IS NOT NULL GROUP BY sv1.JobId)
GROUP BY sv.JobId, sv.[AmazonPriceNotFoundReason]
ORDER BY sv.JobId, sv.[AmazonPriceNotFoundReason]



select count(*) from ScraperVisits where JobId = '043821ec-04ce-42f9-becd-39e8f155ba0a'
select count(*) from ScraperVisits where JobId = 'b8715d12-9c22-4917-8d6a-39f0c1174ab3'


SELECT * FROM Products