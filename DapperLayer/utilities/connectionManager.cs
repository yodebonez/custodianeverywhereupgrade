using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


namespace DapperLayer.utilities
{
    public static class connectionManager
    {
        public static string connectionString(string name = "")
        {
            if (string.IsNullOrEmpty(name))
                return ConfigurationManager.ConnectionStrings["Dapper"].ConnectionString;
            else
                return ConfigurationManager.ConnectionStrings[name].ConnectionString;
        }


        public static string sp_getall_new { get; } = @"SELECT count(DISTINCT(Customerid)) from [ABS_MEMO].[dbo].[Recommended_products];
                                            SELECT  result.CustomerID,(Customer.FirstName +' '+ Customer.LastName) as 'FullName',Customer.Email,Customer.Phone,Customer.Occupation,Customer.Data_source,
                                            (SELECT count(*) FROM [ABS_MEMO].[dbo].Dim_Policies AS pol inner join [ABS_MEMO].[dbo].Dim_Product as prod 
                                            on pol.ProdID = prod.ProductID and pol.CustomerID = result.CustomerID and 
                                            pol.Bus_Category = 'Individual' and pol.Bus_Source = 'Non_Broker') as 'currentProdCount',
                                            (SELECT count(*) FROM [ABS_MEMO].[dbo].[Recommended_products] where CustomerID = result.CustomerID) as 'NoOfProductRecommended',
                                            (SELECT SUM(Scored_probability)/Count(*) FROM [ABS_MEMO].[dbo].[Recommended_products] where CustomerID = result.CustomerID) as 'AvgProb'
                                            FROM (SELECT DISTINCT TOP 30 CustomerID FROM [ABS_MEMO].[dbo].[Recommended_products]
                                            where CustomerID NOT IN (SELECT DISTINCT TOP {0} CustomerID FROM [ABS_MEMO].[dbo].[Recommended_products])) AS  result
                                            inner join [ABS_MEMO].[dbo].[Dim_Customers] as Customer
                                            ON result.CustomerID = Customer.CustomerID 
                                            order by 'AvgProb' desc";

        public static string search_query { get; } = @"SELECT DISTINCT prod.CustomerID,(Customer.FirstName +' '+ Customer.LastName) as 'FullName',Customer.Email,Customer.Phone,Customer.Occupation,Customer.Data_source,
                                                        (SELECT count(*) FROM [ABS_MEMO].[dbo].Dim_Policies AS pol inner join [ABS_MEMO].[dbo].Dim_Product as pro 
                                                        on pol.ProdID = pro.ProductID and pol.CustomerID = prod.CustomerID and pol.Bus_Category = 'Individual' 
                                                        and pol.Bus_Source = 'Non_Broker') as 'currentProdCount',
                                                        (SELECT count(*) FROM [ABS_MEMO].[dbo].[Recommended_products] where CustomerID = prod.CustomerID) as 'NoOfProductRecommended',
                                                        (SELECT SUM(Scored_probability)/Count(*) FROM [ABS_MEMO].[dbo].[Recommended_products] where CustomerID = prod.CustomerID) as 'AvgProb'
                                                        FROM [ABS_MEMO].[dbo].[Dim_Customers]  as Customer
                                                        inner join [ABS_MEMO].[dbo].[Recommended_products] as prod
                                                        on Customer.CustomerID = prod.CustomerID and Customer.FullName  like '%' + @search + '%' order by 'AvgProb' desc;";

        public static string recomendation { get; } = @"SELECT Product_recommended,Scored_probability FROM [ABS_MEMO].[dbo].[Recommended_products] where CustomerID = @customer_id order by Scored_probability desc;
                                                        SELECT prod.Product_lng_descr,pol.Policy_no FROM [ABS_MEMO].[dbo].Dim_Policies as pol
                                                        inner join [ABS_MEMO].[dbo].Dim_Product as prod
                                                        on pol.ProdID = prod.ProductID and pol.CustomerID = @customer_id and
                                                        pol.Bus_Category = 'Individual' and pol.Bus_Source = 'Non_Broker';";
        //public static string _getNewCustomer { get; } = $@"SELECT cus.Occupation, cus.Date_of_Birth, cus.Gender, cus.Email, cus.CustomerID,(SELECT Convert(nvarchar, (SELECT Product_lng_descr FROM [dbo].[Dim_Product] WHERE ProductID = ProdID)) + ',' 
        //                                                FROM[dbo].[Dim_Policies] where CustomerID = cus.CustomerID and Bus_Source = 'Non_Broker' and Bus_Category = 'Individual' for xml path('')) as currentProds
        //                                                from[ABS_MEMO].[dbo].[Dim_Customers] as cus where cus.Created_date between '2019-05-01 00:00:00.000'
        //                                                and  '2019-09-30 00:00:00.000' and cus.Email<> '' and cus.Email is not null ";


        public static string _getNewCustomer2 { get; } = @" declare @table as Table(
                                                            Occupation VARCHAR(MAX),
                                                            Date_of_Birth datetime2,
                                                            Gender VARCHAR(5),
	                                                        Email VARCHAR(max) not null,
	                                                        CustomerID VARCHAR(max) not null,
	                                                        Premium bigint ,
	                                                        currentProds VARCHAR(MAX));
                                                            INSERT INTO @table SELECT cus.Occupation,cus.Date_of_Birth,cus.Gender,cus.Email,cus.CustomerID,
                                                              (SELECT TOP(1) Premium FROM  [dbo].[Dim_Policies] WHERE CustomerID = cus.CustomerID and Premium is not null) as Premium,
                                                              (SELECT Convert(nvarchar,(SELECT Product_lng_descr FROM [dbo].[Dim_Product] WHERE ProductID = ProdID  )) + ','
                                                              FROM  [dbo].[Dim_Policies] 
                                                              where CustomerID = cus.CustomerID and Bus_Source = 'Non_Broker' and Bus_Category = 'Individual'   for xml path('')) as currentProds
                                                              from [ABS_MEMO].[dbo].[Dim_Customers] as cus where cus.Created_date between '2019-05-01 00:00:00.000'  
                                                              and  '2019-09-30 00:00:00.000' and cus.Email <> '' and cus.Email is not null;
                                                              
                                                               SELECT * FROM @table where currentProds is not null;
                                                              ";
        public static string _getNewCustomerSP { get; } = "RecommendationEngine";

        public static string renewalsRatio { get; } = @"DECLARE @temp_table as Table(
                                                 Unit_lng_descr varchar(100),
                                                 Status varchar(20),
                                                 Company varchar(100),
                                                 Product varchar(max),
                                                 unit_id int,
                                                 Premium decimal,
                                                 EndDate datetime
                                                )

                                                INSERT INTO @temp_table  SELECT Unit_lng_descr, 'Status' = 'Renewed',Company,Product_lng_descr,unitid,Premium,EndDate

                                                FROM         [dbo].[Renewal_queue]
                                                WHERE        Status LIKE 'Renewed%' and Product_lng_descr is not null {0}
                                             

                                                INSERT INTO @temp_table  SELECT Unit_lng_descr, 'Status' = 'Unrenewed',Company,Product_lng_descr,unitid,Premium,EndDate
                                                FROM         [dbo].[Renewal_queue]
                                                WHERE        Status LIKE '%Unrenewed' and Product_lng_descr is not null {0}
                                              
                                                select * from @temp_table";

        public static string NexRenewal { get; } = @"select * from [dbo].[Renewals_staging] where {0}  {1}
                                                        ORDER BY EndDate 
                                                        OFFSET {2} ROWS FETCH NEXT {3} ROWS ONLY OPTION (RECOMPILE);
                                                        
                                                        Select Count(*) as Total from [dbo].[Renewals_staging] 
                                                        where {4}
                                                        {5}";

        public static string GetPolicy { get; } = "GetPolicies";
        public static string ConfirmAgent { get; } = "Get_Agents";
        public static string GetAgentPolicies { get; } = "GetPolicies_Agents";
    }
}
