using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataStore.ViewModels
{
    public class Name
    {
        public string title { get; set; }
        [Required]
        public string last { get; set; }
        [Required]
        public string first { get; set; }
        public string middle { get; set; }
    }

    public class Personal
    {
        [Required]
        public string sex { get; set; }
        [Required]
        public DateTime dateOfBirth { get; set; }
        [Required]
        public string motherMaidenName { get; set; }
        [Required]
        public string nationality { get; set; }
        [Required]
        public string state { get; set; }
        [Required]
        public string localGA { get; set; }
        [Required]
        public string idType { get; set; }
        [Required]
        public string idNo { get; set; }
        [Required]
        public DateTime idExpDate { get; set; }
    }

    public class Personal2
    {
        [Required]
        public string sex { get; set; }
        [Required]
        public string dateOfBirth { get; set; }
        [Required]
        public string motherMaidenName { get; set; }
        [Required]
        public string nationality { get; set; }
        [Required]
        public string state { get; set; }
        [Required]
        public string localGA { get; set; }
        [Required]
        public string idType { get; set; }
        [Required]
        public string idNo { get; set; }
        [Required]
        public string idExpDate { get; set; }
    }
    public class Contact
    {
        [Required]
        public string phone { get; set; }
        [Required]
        public string address1 { get; set; }
        public string address2 { get; set; }
        [Required]
        public string city { get; set; }
        [Required]
        public string state { get; set; }
        [Required]
        public string country { get; set; }
        public string postalCode { get; set; }
    }

    public class NextOfKin
    {
        [Required]
        public string name { get; set; }
        [Required]
        public string relationship { get; set; }
        [Required]
        public string phone { get; set; }
        [Required]
        public string address1 { get; set; }
        public string address2 { get; set; }
    }

    public class Account
    {
        public string clearingHouseNo { get; set; }
    }

    public class Bank
    {
        [Required]
        public string code { get; set; }
        [Required]
        public string nuban { get; set; }
        [Required]
        public string bvn { get; set; }
        [Required]
        public DateTime date { get; set; }
    }

    public class Bank2
    {
        [Required]
        public string code { get; set; }
        [Required]
        public string nuban { get; set; }
        [Required]
        public string bvn { get; set; }
        [Required]
        public string date { get; set; }
    }
    public class Images
    {
        [Required]
        public string photo { get; set; }
        [Required]
        public string id { get; set; }
        [Required]
        public string address { get; set; }
    }

    public class InterState
    {
        public Name name { get; set; }
        [Required]
        public string email { get; set; }
        public Personal personal { get; set; }
        public Contact contact { get; set; }
        public NextOfKin nextOfKin { get; set; }
        public Account account { get; set; }
        public Bank bank { get; set; }
        public Images images { get; set; }
        [Required]
        public string merchant_id { get; set; }
        [Required]
        public string hash { get; set; }
    }

    public class InterStatePost
    {
        public Name name { get; set; }
        public string email { get; set; }
        public Personal2 personal { get; set; }
        public Contact contact { get; set; }
        public NextOfKin nextOfKin { get; set; }
        public Account account { get; set; }
        public Bank2 bank { get; set; }
        public Images images { get; set; }
        public string terminalId { get; set; }
        public string requestId { get; set; }
        public string hash { get; set; }
    }

    public class InterstateResponse
    {
        public InterstateResponse()
        {

        }

        public bool success { get; set; }
        public int stage { get; set; }
        public string status { get; set; }
        public string accountId { get; set; }
        public string requestId { get; set; }
        public string msg { get; set; }
    }
}
