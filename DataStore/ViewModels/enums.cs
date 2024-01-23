using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataStore.ViewModels
{
    public enum Category
    {
        GENERAL_GOODS,
        PERISHABLES,
        BREAKABLES
    }

    public enum Type
    {
        EMAIL,
        SMS,
        OTP
    }

    public enum Types
    {
        OneOff,
        Continuous
    }

    public enum Zones
    {
        AREA_1,
        AREA_2
    }

    public enum subsidiary
    {
        Life = 1,
        General = 2
    }

    public enum TypeOfCover
    {
        Comprehensive = 1,
        Third_Party = 2,
        Third_Party_Fire_And_Theft = 3
    }

    public enum Platforms
    {
        ADAPT = 1,
        USSD = 2,
        WEBSITE = 3,
        MAX = 4,
        OTHERS = 5
    }

    public enum MealPlanCategory
    {
        GainWeight = 1,
        LoseWeight = 2,
        MaintainWeight = 3,
        ImmuneBoost = 4
    }

    public enum Preference
    {
        Poultry = 1,
        Meat = 2,
        Fish = 3,
        SeaFood = 4,
        Pork = 5,
        None = 6
    }

    public enum DaysInWeeks
    {
        Monday = 1,
        Tuesday = 2,
        Wednessday = 3,
        Thursday = 4,
        Friday = 5,
        Saturday = 6,
        Sunday = 7
    }

    public enum MealType
    {
        BreakFast = 1,
        Lunch = 2,
        Dinner = 3
    }

    public enum GivenBirth
    {
        Yes = 1,
        No = 2,
        NotApplicable = 3
    }

    public enum MaritalStatus
    {
        Single = 1,
        Married = 2,
        IdRatherNotSay = 3
    }

    public enum Gender
    {
        Male = 1,
        Female = 2
    }

    public enum Frequency
    {
        Monthly = 1,
        Annually = 2,
        Quarterly = 3,
        Bi_Annually = 4,
        Single = 5,
        Semi_Annually = 6
    }

    public enum ReOccurranceState
    {
        SCHEDULED = 1,
        COMPLETED = 2,
        CANCELLED = 3
    }

    public enum PolicyType
    {
        EsusuShield = 1,
        CapitalBuilder = 2,
        LifeTimeHarvest = 3,
        TermAssurance = 4,
        WealthPlus = 5
    }

    public enum Car
    {
        Start = 1,
        Stop = 2
    }

    public enum TravelCategory
    {
        WorldWide = 1,
        Schengen = 2,
        MiddleAndAsia = 3,
        Africa = 4,
        WorldWide2 = 5
    }

    public enum AppPlatform
    {
        Andriod = 1,
        IOS = 2
    }

    public enum Divisions
    {
        PUBLIC_SECTOR = 1,
        PERSONAL_LINES = 2,
        FINANCIAL_INSTITUTIONS = 3,
        MANUFACTURING = 4,
        TRADING_SERVICES = 5,
        ENGINEERING_TELECOM = 6,
        OIL_AND_GAS_SPECIAL_RISK = 7,
        RETAIL_BANCASSURANCE = 8,
        E_BUSINESS = 9,
        BRANCH_OPERATIONS_NETWORK = 10,
        MARKETING = 11,
        NORTHERN_REGION = 12,
        WESTERN_REGION = 13,
        EASTERN_REGION = 14
    }
    //World Wide => 1
    // Schengen => 2
    // Middle East & Asia => 3
    // Africa => 4
}
