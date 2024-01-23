using DataStore.Models;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Data.Entity.ModelConfiguration.Conventions;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataStore.context
{
    public class dbcontext : DbContext
    {
        public dbcontext() : base("name=CustApi")
        {
            Configuration.LazyLoadingEnabled = true;
        }
        public DbSet<ApiConfiguration> ApiConfiguration { get; set; }
        public DbSet<GITInsurance> GITInsurance { get; set; }
        public DbSet<PremiumCalculatorMapping> PremiumCalculatorMapping { get; set; }
        public DbSet<ApiMethods> ApiMethods { get; set; }
        public DbSet<LifeClaims> LifeClaims { get; set; }
        public DbSet<GeneralClaims> GeneralClaims { get; set; }
        public DbSet<NonLifeClaimsDocument> NonLifeClaimsDocument { get; set; }
        public DbSet<TravelInsurance> TravelInsurance { get; set; }
        public DbSet<FlightAndAirPortData> FlightAndAirPortData { get; set; }
        public DbSet<AutoInsurance> AutoInsurance { get; set; }
        public DbSet<FitfamplusDeals> FitfamplusDeals { get; set; }
        public DbSet<Token> Token { get; set; }
        public DbSet<AgentTransactionLogs> AgentTransactionLogs { get; set; }
        public DbSet<MealPlan> MealPlan { get; set; }
        public DbSet<MyMealPlan> MyMealPlan { get; set; }
        public DbSet<SelectedMealPlan> SelectedMealPlan { get; set; }
        public DbSet<JokesList> JokesList { get; set; }
        public DbSet<LifeInsurance> LifeInsurance { get; set; }
        public DbSet<AdaptLeads> AdaptLeads { get; set; }
        public DbSet<WatchedJokes> WatchedJokes { get; set; }
        public DbSet<News> News { get; set; }
        public DbSet<PinnedNews> PinnedNews { get; set; }
        public DbSet<BuyTrackerDevice> BuyTrackerDevice { get; set; }
        public DbSet<TelematicsUsers> TelematicsUsers { get; set; }
        public DbSet<SafetyPlus> SafetyPlus { get; set; }
        public DbSet<ListOfGyms> ListOfGyms { get; set; }
        public DbSet<MyPreference> MyPreference { get; set; }
        public DbSet<HomeShield> HomeShield { get; set; }
        public DbSet<ReferralCodeLookUp> ReferralCodeLookUp { get; set; }
        public DbSet<LocalTravel> LocalTravel { get; set; }
        public DbSet<PolicyServicesDetails> PolicyServicesDetails { get; set; }
        public DbSet<TempClaimData> TempClaimData { get; set; }
        public DbSet<PaystackRecurringCharges> PaystackRecurringCharges { get; set; }
        public DbSet<Chaka> Chaka { get; set; }
        public DbSet<InterStateStocks> InterStateStocks { get; set; }
        public DbSet<RequestDocument> RequestDocument { get; set; }
        public DbSet<TempSaveData> TempSaveData { get; set; }
        public DbSet<AgentServices> AgentServices { get; set; }
        public DbSet<WealthPlus> WealthPlus { get; set; }

        public DbSet<SessionTokenTracker> SessionTokenTracker { get; set; }
        public DbSet<PaystackRecurringDump> PaystackRecurringDump { get; set; }
        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            modelBuilder.Conventions.Remove<PluralizingTableNameConvention>();
        }
    }
}
