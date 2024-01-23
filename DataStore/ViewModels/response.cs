using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataStore.ViewModels
{

    public class req_response
    {
        public req_response()
        {

        }
        public int status { get; set; }
        public string premium { get; set; }
        public string category { get; set; }
        public decimal value_of_goods { get; set; }
        public string message { get; set; }
        public policy_details policy_details { get; set; }
    }

    public class policy_details
    {
        public policy_details()
        {

        }
        public string policy_number { get; set; }
        public string certificate_no { get; set; }
        public string download_link { get; set; }
    }

    public class claims_response
    {
        public claims_response()
        {

        }
        public int status { get; set; }
        public string message { get; set; }
        public string cliams_number { get; set; }
    }

    public class notification_response
    {
        public notification_response()
        {

        }
        public int status { get; set; }
        public string message { get; set; }
        public string type { get; set; }
        public dynamic data { get; set; }
        public string image_base_url { get; set; }
        public string reciept_url { get; set; }
        public decimal sum_insured { get; set; }
        public bool can_buy_comprehensive { get; set; }
        public dynamic app_updates { get; set; }
        public dynamic policyterms { get; set; }
    }

    public class LifeClaimStatus
    {
        public LifeClaimStatus()
        {

        }
        public int status { get; set; }
        public string claim_no { get; set; }
        public int code { get; set; }
        public string message { get; set; }
        public string policy_no { get; set; }
        public string claim_status { get; set; }

    }

    public class ClaimsStatus
    {
        public ClaimsStatus()
        {

        }
        public int status { get; set; }
        public string claim_status { get; set; }
        public string message { get; set; }
        public string policy_number { get; set; }
        public string policy_holder_name { get; set; }
    }

    public class ClaimsRequest
    {
        public ClaimsRequest()
        {

        }
        [Required]
        public string claims_number { get; set; }
        [Required]
        public string subsidiary { get; set; }
        [Required]
        public string merchant_id { get; set; }
    }

    public class Policy
    {
        public Policy()
        {

        }

        public int status { get; set; }
        public string message { get; set; }
        public object details { get; set; }
    }

    public class Wallet
    {
        public Wallet()
        {

        }
        public int status { get; set; }
        public string message { get; set; }
        public List<object> trnx_details { get; set; }
        public decimal wallet_balance { get; set; }
    }

    public class claim_details
    {
        public claim_details()
        {

        }

        public string policy_no { get; set; }
        public string policy_type { get; set; }
        public string claim_type { get; set; }
    }

    public class claims_details
    {
        public claims_details()
        {

        }
        public string status { get; set; }
        public string message { get; set; }
        public string claim_no { get; set; }
        public decimal amount { get; set; }
        public string policy_no { get; set; }
        public int code { get; set; }
        public object date { get; set; }
    }


    public class ClaimsDetails
    {
        public ClaimsDetails()
        {

        }

        public string p_policy_no { get; set; }
        public string p_type { get; set; }
        public string p_claim_type { get; set; }
        public string merchant_id { get; set; }
    }

    public class TravelQuoteResponse
    {
        public TravelQuoteResponse()
        {

        }
        public int status { get; set; }
        public string message { get; set; }
        public string quote_amount { get; set; }
        public string cert_url { get; set; }
        public List<decimal> quote_list { get; set; }
    }


    public class FlightData
    {
        public FlightData()
        {

        }
        public string AirportCode { get; set; }
        public string AirportName { get; set; }
        public string CityCountry { get; set; }
        public string City { get; set; }
        public string Country { get; set; }
    }

    public class FlightSearch
    {
        public FlightSearch()
        {

        }

        public int status { get; set; }
        public string message { get; set; }
        public List<FlightData> flight_search { get; set; }
    }

    public class policy_data
    {
        public policy_data()
        {

        }
        public int status { get; set; }
        public string message { get; set; }
        public object data { get; set; }
        public object hash { get; set; }
        public dynamic vehiclelist { get; set; }
    }

    public class joke
    {
        public joke()
        {

        }
        public int JokeId { get; set; }
        public string youtube_url { get; set; }
        public string thumbnail_image { get; set; }
        public string credit { get; set; }
        public string title { get; set; }
    }

    public class TransPoseGetPolicyDetails
    {
        public string policyNoField { get; set; }
        public string policyEBusinessField { get; set; }
        public string agenctNumField { get; set; }
        public string agenctNameField { get; set; }
        public string insuredNumField { get; set; }
        public string insuredNameField { get; set; }
        public string insuredOthNameField { get; set; }
        public string insAddr1Field { get; set; }
        public string insAddr2Field { get; set; }
        public string insAddr3Field { get; set; }
        public string telNumField { get; set; }
        public string insuredTelNumField { get; set; }
        public string insuredEmailField { get; set; }
        public DateTime dOBField { get; set; }
        public string insStateField { get; set; }
        public string insLGAField { get; set; }
        public string bizUnitField { get; set; }
        public string insOccupField { get; set; }
        public string bizBranchField { get; set; }
        public DateTime startdateField { get; set; }
        public DateTime enddateField { get; set; }
        public decimal sumInsField { get; set; }
        public decimal mPremiumField { get; set; }
        public decimal outPremiumField { get; set; }
        public decimal instPremiumField { get; set; }
        public object PropertyChanged { get; set; }
    }

    public class HomeTeam
    {
        public int team_id { get; set; }
        public string team_name { get; set; }
        public string logo { get; set; }
    }

    public class AwayTeam
    {
        public int team_id { get; set; }
        public string team_name { get; set; }
        public string logo { get; set; }
    }

    public class Score
    {
        public string halftime { get; set; }
        public string fulltime { get; set; }
        public object extratime { get; set; }
        public object penalty { get; set; }
    }

    public class Fixtures
    {
        public int fixture_id { get; set; }
        public int league_id { get; set; }
        public DateTime event_date { get; set; }
        public int event_timestamp { get; set; }
        public int firstHalfStart { get; set; }
        public int secondHalfStart { get; set; }
        public string round { get; set; }
        public string status { get; set; }
        public string statusShort { get; set; }
        public int elapsed { get; set; }
        public string venue { get; set; }
        public object referee { get; set; }
        public HomeTeam homeTeam { get; set; }
        public AwayTeam awayTeam { get; set; }
        public int goalsHomeTeam { get; set; }
        public int goalsAwayTeam { get; set; }
        public Score score { get; set; }
    }

    public class api
    {
        public api()
        {

        }
        public int results { get; set; }
        public List<Fixtures> fixtures { get; set; }
    }

    public class RenewRatio
    {
        public RenewRatio()
        {

        }

        public int Count { get; set; }
        public string Status { get; set; }
        public string Unit_lng_descr { get; set; }
        public int unit_id { get; set; }
        public string Company { get; set; }
        public string Product { get; set; }
        public decimal Premium { get; set; }
        public DateTime EndDate { get; set; }
    }



    public class Fixture
    {
        public int fixture_id { get; set; }
        public int league_id { get; set; }
        public DateTime event_date { get; set; }
        public int event_timestamp { get; set; }
        public int? firstHalfStart { get; set; }
        public int? secondHalfStart { get; set; }
        public string round { get; set; }
        public string status { get; set; }
        public string statusShort { get; set; }
        public int elapsed { get; set; }
        public string venue { get; set; }
        public string referee { get; set; }
        public HomeTeam homeTeam { get; set; }
        public AwayTeam awayTeam { get; set; }
        public int? goalsHomeTeam { get; set; }
        public int? goalsAwayTeam { get; set; }
        public Score score { get; set; }
    }

    public class Api
    {
        public int results { get; set; }
        public List<Fixture> fixtures { get; set; }
    }

    public class MatchFixtures
    {
        public Api api { get; set; }
    }


    public class NextRenewal
    {
        public NextRenewal()
        {

        }

        public decimal Premium { get; set; }
        public DateTime EndDate { get; set; }
        public string Unit_lng_descr { get; set; }
        public int unitid { get; set; }
        public string Company { get; set; }
        public string Product_lng_descr { get; set; }
        public int DaysAfter { get; set; }
        public string pol_no { get; set; }
    }

    public class NextRenewalResult
    {
        public int TotalPages { get; set; }
        public int OverAllCount { get; set; }
        public List<NextRenewal> Results { get; set; }
    }

    public class Annuity
    {
        public int status { get; set; }
        public DateTime dateOfBirth { get; set; }
        public string message { get; set; }
        public double quote { get; set; }
    }

    public class updates
    {
        public bool app_update { get; set; }
        public string android { get; set; }
        public string ios { get; set; }
        public string android_version { get; set; }
        public string ios_version { get; set; }
        public string ios_updates { get; set; }
        public string android_updates { get; set; }
        public bool ios_is_major { get; set; }
        public bool android_is_major { get; set; }
    }
}
