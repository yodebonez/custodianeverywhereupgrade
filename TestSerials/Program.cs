using DataStore.Models;
using DataStore.repository;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using WindowsService1.NewsAPIJob;
using System.Dynamic;
using System.Reflection;
using DataStore;
using System.Text.RegularExpressions;
using DataStore.Utilities;
using System.Security.Cryptography;
using System.Xml.Serialization;
using System.Numerics;
using DataStore.ViewModels;
using RecurringDebitService.BLogic;

namespace TestSerials
{
    class Program
    {
        public static void Grouper()
        {
            //Core<policyInfo> _policyinfo = new Core<policyInfo>();
            //var getPolicies = _policyinfo.GetAgentPolicies("103574").GetAwaiter().GetResult();
            //var groupBy = getPolicies.Select(x => new AgentPoliciesView
            //{
            //    Data_source = (x.Data_source == "ABS") ? "General" : "Life",
            //    Email = x.Email,
            //    EndDate = x.EndDate?.ToShortDateString(),
            //    FullName = x.FullName?.Trim().ToUpper(),
            //    Phone = x.Phone?.Trim(),
            //    policy_no = x.policy_no?.Trim(),
            //    Policy_status = x.Policy_status?.Trim().ToUpper(),
            //    Product_lng_descr = x.Product_lng_descr?.Trim(),
            //    StartDate = x.StartDate?.ToShortDateString(),
            //    Sub_prod_lng_descr = x.Sub_prod_lng_descr
            //}).GroupBy(x => x.Data_source);

            //Dictionary<string, List<AgentPoliciesView>> groupKey = new Dictionary<string, List<AgentPoliciesView>>();
            //foreach (var item in groupBy)
            //{
            //    groupKey.Add(item.Key, item.ToList());
            //}
            //var json = Newtonsoft.Json.JsonConvert.SerializeObject(groupKey);
        }
        private static string alphanums = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz123456789";
        private const int codeLen = 15; //Length of coded string. Must be at least 4
        public static string EncodeNumber(int num)
        {
            if (num < 1 || num > 999999) //or throw an exception
                return "";
            int[] nums = new int[codeLen];
            int pos = 0;

            while (!(num == 0))
            {
                nums[pos] = num % alphanums.Length;
                num /= alphanums.Length;
                pos += 1;
            }

            string result = "";
            foreach (int numIndex in nums)
                result = alphanums[numIndex].ToString() + result;

            return result;
        }
        public static string DecodeNumber(string str)
        {
            //Check for invalid string
            if (str.Length != codeLen) //Or throw an exception
                return "Error";
            long num = 0;

            foreach (char ch in str)
            {
                num *= alphanums.Length;
                num += alphanums.IndexOf(ch);
            }

            //Check for invalid number
            if (num < 1 || num > 999999) //or throw exception
                return "Error";
            return num.ToString();
        }
        static void Main(string[] args)
        {
            // var util = new Utility();
            //var testAD = util.AuthenticateLDAP("Oitaba", "vmADMIN22$2").GetAwaiter().GetResult();
            //var encode = EncodeNumber(16318);
            //var decode = DecodeNumber(encode);
            //new CardProcessor().RecurringEngine();
            // Grouper();
            //var uti = new Utility().GetChakaOauthToken("Godreigns2").GetAwaiter().GetResult();
            //  string v = "1TzfOhvNJUcr6u8vZlBHa8cp1yKNauO24jyWP008d2RhNi5uac4Rj9U+sUdicEpARJ8+HqvRSNmuFR/tz0bKmINquT2BDgbFwFRl2nMwRAtz1CFzZrpKznDeoKtKIavNRyUo47NLoUNKuXF18T4uMWYQk+lhpnHwYp1se10hZfjyja4bpEZLgMbLcwH6Yn26BFnjjDfkEYDJ6Vf69ltu58JKg78HTUAwQ01UZwjb2zBBajX+y0WjdskBnbHVB5j0GyIxDM3HgdrShnku4wPICyhG0kPMgJNIQHfLsBOO73N2bP6mWo0PUjj7HrOvRjOeIcDk9m3+XPSIOyuBUb1Pzw=="
            //var path = $"{AppDomain.CurrentDomain.BaseDirectory}/Config/InterStatePrivateKey.txt";
            //var signature = InterStateEncryption.GetSignature("BALEWApkofon@gmail.comC7N1", path);
            //var verify = InterStateEncryption.VerifySignature("BALEWApkofon@gmail.comC7N1", signature);
            // var _key = verify;
            //InterStateEncryption.ImportKeyPairIntoContainer();
            //var cp = InterStateEncryption.GetRSACryptoServiceProviderFromContainer();
            //Console.WriteLine($"ProviderName {cp}");
            //Console.WriteLine($"UniqueKeyContainerName {cp.CspKeyContainerInfo.UniqueKeyContainerName}");
            //Console.WriteLine($"{cp.ExportParameters(true)}");
            //var signature = InterStateEncryption.GetSignature("BALEWApkofon@gmail.comC7N1");
            //var test = cp.ExportParameters(true);
            //var util = new Utility().ValidateWealthPlusCoverLimits(10000, Frequency.Annually, Convert.ToDecimal(2000), 8).GetAwaiter().GetResult();

            //var test = "47783";
            //RSA();
            //byte[] data = Encoding.Unicode.GetBytes("AMAGASHIamagashi.pat@mail.com001");
            //RSACryptoServiceProvider csp = new RSACryptoServiceProvider();//make a new csp with a new keypair
            //var pub_key = csp.ExportParameters(false); // export public key
            //var priv_key = csp.ExportParameters(true); // export private key
            //
            //var encData = csp.Encrypt(data, false); // encrypt with PKCS#1_V1.5 Padding
            //var output = Convert.ToBase64String(encData);
            //var decBytes = MyRSAImpl.plainDecryptPriv(encData, priv_key); //decrypt with own BigInteger based implementation
            //var decData = decBytes.SkipWhile(x => x != 0).Skip(1).ToArray();//strip PKCS#1_V1.5 padding
            // 
            //var test = new Utility().RoundValueToNearst100(5227.20);
            //var test1 = new Utility().RoundValueToNearst100(7597.26);
            //var test2 = new Utility().RoundValueToNearst100(8624.88);
            //var encrypt = new Utility().Encrypt("Amagashi".ToUpper() + "amagashi.pat@mail.com".ToLower() + "001");
            // var t = encrypt;
            //declare the array.
            //used this instead of a list, it is simpler to handle
            NewsProcessor.GetNews();
            //List<int> xx = new List<int>()
            //{
            //   3,6,1,7,3,9,1,12
            //};
            //int[] array = xx.ToArray();
            //int sortedIndex = 0;

            //Console.Write("Sorting started  with : ");
            //foreach (int i in array)
            //{
            //    Console.Write(i);
            //}
            //Console.WriteLine("");
            //while(sortedIndex < (array.Length - 1))
            //for (int topindex = 0; topindex < (array.Length - 1); topindex++)
            //{
            //    for (int index = 0; index < (array.Length - 1); index++)
            //    {
            //        //1,3,2,5
            //        if (array.ElementAt(index) > array.ElementAt(index + 1))
            //        {
            //            int currentIndex = array.ElementAt(index);
            //            int nextIndex = array.ElementAt(index + 1);

            //            array[index] = nextIndex;
            //            array[index + 1] = currentIndex;

            //            sortedIndex = index;

            //            //Console.WriteLine(index + "/"+ (array.Length -1));
            //        }
            //    }
            //    // Console.WriteLine(sortedIndex);
            //}


            //Console.Write("Output : ");
            //foreach (int i in array)
            //{
            //    Console.Write(i);
            //}

            //Console.ReadLine();
        }
        //static void Main(string[] args)
        //{
        //    try
        //    {
        //        //var test = breakPalindrome("bab");
        //        var t = new CrossSellingEngine();
        //        t.EngineProcessor();
        //        // using (var api = new Cust.PolicyServicesSoapClient())
        //        // {
        //        //var response = api.PostTravel2Raga(DateTime.Now, Convert.ToDateTime("2020-03-01"),
        //        //    "Oscar", "Oscar", "AREA 1", "Akkk34567", Convert.ToDateTime("1989-03-01"), "NIGERIAN NIGERIA",
        //        //    "NIGERIA", "oscardybabaphd@gmail.com", "GHANA");
        //        //var newresponse = response;
        //        //Console.WriteLine(Newtonsoft.Json.JsonConvert.SerializeObject(response));
        //        // }

        //        //Regex test = new Regex(@"^(?=.*[a-z])(?!.*[\s])(?=.*[A-Z])(?=.*\d)(?=.*\W)[\S]{8,}$");//(@"^.*(?=.{8,})(?!.*[\s])(?=.*\d)(?=.*[a-z])(?=.*[A-Z])(?=.*[!*@#$%^&+=]).*$");
        //        //Console.WriteLine($"P@55w0rd: {test.IsMatch("P$%55w0rd")}");
        //        //Console.WriteLine($"P@55 w0rd: {test.IsMatch("P55 w0rd")}");
        //        //Console.WriteLine($"P@55w0rd^736: {test.IsMatch("P@55w0rd^736")}");
        //        //Console.WriteLine($"password: {test.IsMatch("password")}");
        //        //Console.WriteLine($"PASSWORD: {test.IsMatch("PASSWORD")}");
        //        //Console.WriteLine($"vmdmin2$78: {test.IsMatch("vmdmin2$78")}");

        //        // Console.ReadKey();
        //        //Core<NextRenewal> dapper_core = new Core<NextRenewal>();
        //        //var condition = new helpers();
        //        //var query = condition.QueryResolver(new RenewalRatio
        //        //{
        //        //    merchant_id = "Test",
        //        //    is_MD = true,
        //        //    subsidary = subsidiary.Life

        //        //});
        //        //var result = dapper_core.GetRenewalRatio(string.Format(connectionManager.NexRenewal, query)).GetAwaiter().GetResult();
        //        //var grouped_item = new helpers().Grouper2(result);
        //        //var test = Newtonsoft.Json.JsonConvert.SerializeObject(grouped_item);
        //        //Console.WriteLine(test);
        //        //NumberFormatInfo setPrecision = new NumberFormatInfo();
        //        //setPrecision.NumberDecimalDigits = 1;
        //        //var premium = 5000.00;
        //        //var format = string.Format("{0:1}", premium);
        //        //var test = format;

        //        //Console.ReadKey();

        //        #region
        //        //var a = new { name = "oscar  ", age = 30, sex = 'M', amount = 345.66, isActive = true };
        //        //dynamic expando = new ExpandoObject();
        //        //expando.obj = a;
        //        //Console.WriteLine(((string)expando.obj.name).Trim());
        //        //if (expando.obj.amount is decimal)
        //        //{
        //        //    Console.WriteLine(true);
        //        //}
        //        //else
        //        //{
        //        //    Console.WriteLine(false);
        //        //}
        //        //Console.ReadKey();
        //        //int[] arr = { 5, 4, 8, 2, 6, 7, 1, 3, 7 };
        //        //var test = arr.ToList().OrderBy(x=>x);

        //        //int sum = 0;
        //        //for (var i = 0; i < arr.Length; ++i)
        //        //{
        //        //    sum += arr[i];
        //        //}
        //        //var d = (arr.Length * (arr.Length + 1)) / 2;
        //        //var dif = d - sum;
        //        //var dup = arr[dif];
        //        //var dp = dup;

        //        //List<int> h = new List<int>() { 4, 5, 67, 8 };
        //        //h.Min();
        //        // var test = new Core<dynamic>();
        //        // var l = test.GetPredictionByCustomerID(100049357, connectionManager.recomendation).GetAwaiter().GetResult();

        //        //var str = "2 3 1 4";
        //        //var arr = str.Split(' ').Select(x => Convert.ToInt32(x)).ToList();
        //        //List<int> newlist = new List<int>();
        //        //for (int i = 0; i < arr.Count(); i++)
        //        //{
        //        //    for (int k = 1 + i; k < arr.Count(); k++)
        //        //    {
        //        //        newlist.Add(arr[i] * arr[k]);
        //        //    }
        //        //    //  arr.RemoveAt(i + 1);
        //        //}

        //        //int min = newlist.Min();

        //        //int T = Convert.ToInt32(Console.ReadLine());
        //        //string cases = Console.ReadLine();
        //        //string[] all = cases.Split(' ');

        //        //if (T >= 1 && T <= 10)
        //        //{
        //        //    int count_test = all.Length;
        //        //    for (int i = 0; i < count_test; ++i)
        //        //    {
        //        //        int item = Convert.ToInt32(all[0]);
        //        //        for (int k = 1; k <= item; ++k)
        //        //        {
        //        //            if (k % 3 == 0)
        //        //            {
        //        //                Console.WriteLine("Fizz");
        //        //            }
        //        //            else if (k % 5 == 0)
        //        //            {
        //        //                Console.WriteLine("Buzz");
        //        //            }
        //        //            else if ((k % 3 == 0) && (k % 5 == 0))
        //        //            {
        //        //                Console.WriteLine("FizzBuzz");
        //        //            }
        //        //            else
        //        //            {
        //        //                Console.WriteLine("{0}", k);
        //        //            }
        //        //        }
        //        //    }
        //        //}




        //        // using (OracleConnection cn = new OracleConnection("Data Source=TESTDB;User id=TQ_LMS; Password=TQ_LMS; enlist=false; pooling=false"))
        //        //{
        //        //OracleCommand cmd = new OracleCommand();
        //        //cmd.Connection = cn;
        //        //cn.Open();
        //        //cmd.CommandText = "cust_max_mgt.create_claim";
        //        //cmd.CommandType = CommandType.StoredProcedure;
        //        //cmd.Parameters.Add("p_policy_no", OracleDbType.Varchar2).Value = "170200001709";
        //        //cmd.Parameters.Add("p_type_code", OracleDbType.Varchar2).Value = "DTH";
        //        //cmd.Parameters.Add("v_data", OracleDbType.Varchar2, 300).Direction = ParameterDirection.Output;
        //        //cmd.ExecuteNonQuery();
        //        //string resposne = cmd.Parameters["v_data"].Value.ToString();

        //        //OracleCommand cmd = new OracleCommand();
        //        //cmd.Connection = cn;
        //        //cn.Open();
        //        //cmd.CommandText = "cust_max_mgt.get_claim_policy_info";
        //        //cmd.CommandType = CommandType.StoredProcedure;
        //        //cmd.Parameters.Add("p_policy_no", OracleDbType.Varchar2).Value = "P/2/FP/000035";
        //        //cmd.Parameters.Add("p_type", OracleDbType.Varchar2).Value = "CLAIMS";
        //        //cmd.Parameters.Add("v_data", OracleDbType.Varchar2, 300).Direction = ParameterDirection.Output;
        //        //cmd.Parameters.Add("p_claim_type", OracleDbType.Varchar2).Value = "DTH";
        //        //cmd.ExecuteNonQuery();
        //        //string response = cmd.Parameters["v_data"].Value.ToString();
        //        //}
        //        //string username = "Aladdin";
        //        //string password = "openSesame";

        //        //byte[] concatenated = System.Text.ASCIIEncoding.ASCII.GetBytes(username + ":" + password);
        //        //string header = System.Convert.ToBase64String(concatenated);
        //        //HttpClient client = new HttpClient();
        //        //client.BaseAddress = new Uri("https://{base_url}/");
        //        //client.DefaultRequestHeaders.Accept.Clear();
        //        //client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
        //        //client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic", "QWxhZGRpbjpvcGVuIHNlc2FtZQ==");
        //        //var request = new MultipartFormDataContent();
        //        //request.Add(new StringContent("Jane Doe <jane.doe@mail.custodianplc.com.ng>"), "from");
        //        //request.Add(new StringContent("john.smith@somedomain.com"), "to");
        //        //request.Add(new StringContent("Mail subject text"), "subject");
        //        //request.Add(new StringContent("Rich HTML message body."), "text");
        //        //var response = client.PostAsync("email/1/send", request).Result;
        //        //if (response.IsSuccessStatusCode)
        //        //{
        //        //    var responseContent = response.Content;
        //        //    string responseString = responseContent.ReadAsStringAsync().Result;
        //        //    Console.WriteLine(responseString);
        //        //}

        //        //var payload = System.IO.File.ReadAllText(@"C:\Users\OItaba\Desktop\MF'B\payload.txt");
        //        //using (var api = new TravelApi.wsLowFarePlusSoapClient())
        //        //{

        //        //    var result = api.wmLowFarePlusXml(payload);
        //        //    XmlDocument doc = new XmlDocument();
        //        //    doc.LoadXml(result);

        //        //    string jsonText = JsonConvert.SerializeXmlNode(doc).Replace("@","_");
        //        //    var obj = Newtonsoft.Json.JsonConvert.DeserializeObject<dynamic>(jsonText);
        //        //    var test = obj.OTA_AirLowFareSearchPlusRS._Version.ToString();

        //        //}
        //        //var mealPlan = new store<MealPlan>();
        //        //List<MealPlan> plan = mealPlan.FindMany(x => x.target == "WeightGain" && x.preference == "Poultry").GetAwaiter().GetResult();
        //        //var group_plan = plan.GroupBy(x => x.daysOfWeek);
        //        //var final = new List<object>();
        //        //foreach (var item in group_plan)
        //        //{
        //        //    var dic = new Dictionary<string, Dictionary<string, List<temp>>>();
        //        //    var list_meal = new List<temp>();
        //        //    var meal = item.GroupBy(x => x.mealType);
        //        //    var day = new Dictionary<string, List<temp>>();
        //        //    foreach (var subitem in meal)
        //        //    {
        //        //        day.Add(subitem.First().mealType, subitem.Select(x => new temp
        //        //        {
        //        //            food = x.food,
        //        //            quantity = x.quantity,
        //        //            time = x.time,
        //        //            youtubeurl = x.youTubeUrl
        //        //        }).ToList());
        //        //    }

        //        //    dic.Add(item.First().daysOfWeek, day);
        //        //    final.Add(dic);
        //        //}
        //        //var net = Newtonsoft.Json.JsonConvert.SerializeObject(final);

        //        //int A = 10;
        //        //int B = 20;
        //        //decimal test = Convert.ToDecimal(98)/ Convert.ToDecimal(10);
        //        //decimal b = Math.Ceiling(test);
        //        //double b = Math.Floor(Math.Sqrt(B));
        //        //List<int> arry = new List<int>();
        //        //if(A >= 2 && B <= 1000000000)
        //        //{
        //        //    for (double i = a; i <= b; ++i)
        //        //    {
        //        //        double sqr = Math.Pow(i, Convert.ToDouble(2));
        //        //        if (sqr >= A && sqr <= B)
        //        //        {
        //        //            //double sqr = Math.Sqrt(sqrt);
        //        //           int count =  Cal(sqr, 0);
        //        //            arry.Add(count);
        //        //        }
        //        //    }

        //        //    return arry.Min.Max();
        //        //}

        //        // int value = 8;
        //        //List<int> binary = Convert.ToString(161, 2).ToCharArray().Select(x => Convert.ToInt32(x)).ToList();
        //        //int count = 0;
        //        //List<int> index = new List<int>();
        //        //int i = 0;
        //        //foreach (var item in binary)
        //        //{
        //        //    if(item % 2 != 0)
        //        //    {
        //        //        count++;
        //        //        index.Add(i + 1);
        //        //    }
        //        //    ++i;
        //        //}
        //        //index.Insert(0, count);
        //        //NewsProcessor.GetNews();
        //        #endregion

        //        //temp myObj = new temp();

        //        //var test =  myObj.MapToObject(typeof(Viewtemp));
        //    }
        //    catch (Exception ex)
        //    {
        //        Console.WriteLine(ex.Message);
        //        Console.WriteLine(ex.StackTrace);
        //        Console.WriteLine(ex.InnerException);
        //    }
        //}
        public static void RSA()
        {
            //lets take a new CSP with a new 2048 bit rsa key pair
            var csp = new RSACryptoServiceProvider(2048);

            //how to get the private key
            var privKey = csp.ExportParameters(true);

            //and the public key ...
            var pubKey = csp.ExportParameters(false);

            //converting the public key into a string representation
            string pubKeyString;
            {
                //we need some buffer
                var sw = new StringWriter();
                //we need a serializer
                var xs = new XmlSerializer(typeof(RSAParameters));
                //serialize the key into the stream
                xs.Serialize(sw, pubKey);
                //get the string from the stream
                pubKeyString = sw.ToString();
            }

            //converting it back
            {
                //get a stream from the string
                var sr = new System.IO.StringReader(pubKeyString);
                //we need a deserializer
                var xs = new System.Xml.Serialization.XmlSerializer(typeof(RSAParameters));
                //get the object back from the stream
                pubKey = (RSAParameters)xs.Deserialize(sr);
            }

            //conversion for the private key is no black magic either ... omitted

            //we have a public key ... let's get a new csp and load that key
            csp = new RSACryptoServiceProvider();
            csp.ImportParameters(pubKey);

            //we need some data to encrypt
            var plainTextData = "AMAGASHIamagashi.pat@mail.com001";

            //for encryption, always handle bytes...
            var bytesPlainTextData = System.Text.Encoding.Unicode.GetBytes(plainTextData);

            //apply pkcs#1.5 padding and encrypt our data 
            var bytesCypherText = csp.Encrypt(bytesPlainTextData, false);

            //we might want a string representation of our cypher text... base64 will do
            var cypherText = Convert.ToBase64String(bytesCypherText);

            /*
             * some transmission / storage / retrieval
             * 
             * and we want to decrypt our cypherText
             */

            //first, get our bytes back from the base64 string ...
            bytesCypherText = Convert.FromBase64String(cypherText);

            //we want to decrypt, therefore we need a csp and load our private key
            csp = new RSACryptoServiceProvider();
            csp.ImportParameters(privKey);

            //decrypt and strip pkcs#1.5 padding
            bytesPlainTextData = csp.Decrypt(bytesCypherText, false);

            //get our original plainText back...
            plainTextData = Encoding.Unicode.GetString(bytesPlainTextData);
        }
        public void GetAPI()
        {
            var api = "https://jsonmock.hackerrank.com/api/articles?author=epaga&page=1";
            List<dynamic> arr = new List<dynamic>();
            using (var http = new HttpClient())
            {
                var reponse = http.GetAsync(api).GetAwaiter().GetResult();
                if (reponse.IsSuccessStatusCode)
                {
                    string content = reponse.Content.ReadAsStringAsync().GetAwaiter().GetResult();
                    var obj = Newtonsoft.Json.JsonConvert.DeserializeObject<dynamic>(content);
                    arr.Add(obj.data);
                    if (obj) { }
                }
            }
        }
        public static string breakPalindrome(string palindromeStr)
        {
            var arr = palindromeStr.ToCharArray().ToArray();
            Console.WriteLine(arr);
            // bool containtOtherApha = Regex.IsMatch(palindromeStr,@"/(^a)/");
            //Console.WriteLine(containtOtherApha);
            // if(containtOtherApha){
            // bool isPossible = false;
            for (int i = 0; i < arr.Length; ++i)
            {
                if (arr[i] != 'a')
                {
                    arr[i] = 'a';
                    var pal = string.Join(" ", arr);
                    Console.WriteLine(pal);
                    var reverse = string.Join(" ", arr.Reverse());
                    Console.WriteLine(reverse);
                    if (pal != reverse)
                    {
                        // isPossible = true;
                        return pal;

                    }
                    else
                    {
                        continue;
                    }
                }
                else
                {
                    continue;
                }

            }
            return "IMPOSSIBLE";
            //}else{
            //  return "IMPOSSIBLE";
            //}

        }
        public List<string> popularNFeatures(int numFeatures,
                                        int topFeatures,
                                        List<string> possibleFeatures,
                                        int numFeatureRequests,
                                        List<string> featureRequests)
        {
            // WRITE YOUR CODE HERE
            if (topFeatures > numFeatures) return possibleFeatures;
            Dictionary<string, int> count = new Dictionary<string, int>();

            foreach (string pfeatures in possibleFeatures)
            {
                foreach (var item in featureRequests)
                {
                    if (item.Contains(pfeatures))
                    {
                        if (count.ContainsKey(pfeatures))
                        {
                            count[pfeatures] += 1;
                        }
                        else
                        {
                            count.Add(pfeatures, 1);
                        }
                    }
                }

            }
            var final = count.OrderByDescending(x => x.Value).ToList();
            List<string> f = new List<string>();
            for (int i = 0; i < topFeatures; ++i)
            {
                f.Add(final[i].Key);
            }
            return f;
        }




        public int get(List<List<int>> arr)
        {
            var M = arr;
            int i, j;
            int count = 0;
            //no of rows in M[,] 
            int R = arr.Count();
            //no of columns in M[,] 
            int C = arr[0].Count();
            int[,] S = new int[R, C];

            int max_of_s, max_i, max_j;

            /* Set first column of S[,]*/
            for (i = 0; i < R; i++)
            {
                S[i, 0] = M[i][0];
            }


            /* Set first row of S[][]*/
            for (j = 0; j < C; j++)
            {
                S[0, j] = M[0][j];
            }

            /* Construct other entries of S[,]*/
            for (i = 1; i < R; i++)
            {
                for (j = 1; j < C; j++)
                {
                    if (M[i][j] == 1)
                        S[i, j] = Math.Min(S[i, j - 1],
                                Math.Min(S[i - 1, j], S[i - 1, j - 1])) + 1;
                    else
                        S[i, j] = 0;
                }
            }

            max_of_s = S[0, 0]; max_i = 0; max_j = 0;
            for (i = 0; i < R; i++)
            {
                for (j = 0; j < C; j++)
                {
                    if (max_of_s < S[i, j])
                    {
                        max_of_s = S[i, j];
                        max_i = i;
                        max_j = j;
                    }
                }
            }

            for (i = max_i; i > max_i - max_of_s; i--)
            {
                for (j = max_j; j > max_j - max_of_s; j--)
                {
                    count += count;
                }

            }
            return count;
        }
        public static int Cal(double sqrt, int count)
        {
            double sqr = Math.Sqrt(sqrt);
            if (sqr - Math.Floor(sqr) == 0)
            {
                ++count;
                return Cal(sqr, count);
            }
            return count;

        }
        public static string Serials(int val)
        {
            string final = "";
            if (val <= 9)
            {
                final = "000000" + val;
            }
            else if (val.ToString().Length < 7)
            {
                int loop = 7 - val.ToString().Length;
                string zeros = "";
                for (int i = 0; i < loop; i++)
                {
                    zeros += "0";
                }
                final = zeros + val;
            }
            else
            {
                final = val.ToString();
            }

            return "HO/A/07/T" + final;
        }
        public static int kk(int[] A)
        {
            List<int> diff = new List<int>();
            for (int i = 0; i < A.Length; ++i)
            {
                int fpvalue = A[i];
                for (int j = i + 1; j < A.Length; ++j)
                {
                    int spvalue = A[j];
                    int first;
                    int second;
                    if (fpvalue > spvalue)
                    {
                        first = spvalue + 1;
                        second = fpvalue;
                    }
                    else
                    {
                        first = fpvalue + 1;
                        second = spvalue;
                    }
                    for (int k = first; k < second; ++k)
                    {
                        for (int m = 0; m < A.Length; ++m)
                        {
                            if (A[m] == k)
                            {
                                break;
                            }
                        }


                    }
                    int abs = Math.Abs(fpvalue - spvalue);
                    diff.Add(abs);

                }
            }
            if (diff.Count > 0)
            {
                var min = diff.Min();
                if (min <= 100000000)
                {
                    return min;
                }
                return -1;
            }
            else
            {
                return -2;
            }
        }

        //(string, object, int) LookupName(long id) // tuple return type
        //{
        //    var first = "";
        //    var last = "";
        //    var middle = 60;
        //    return (first, middle, Convert.ToInt32(last)); // tuple literal
        //}

        public static void Closest(int[] arr1, int[] arr2)
        {
            for (int i = 0; i < arr1.Length; i++)
            {
                for (int j = 0; j < arr2.Length; j++)
                {
                    int sum = arr1[i] + arr2[j];
                    if (sum > 22 && sum <= 25)
                    {
                        Console.WriteLine($"({arr1[i]},{arr2[j]}) Sum = {sum}");
                    }
                    else
                    {
                        continue;
                    }
                }
            }
        }

        public void PostMe(string firstDate, string lastDate, string weekDay)
        {
            var _firstdate = Convert.ToDateTime(firstDate);
            var _lastdate = Convert.ToDateTime(lastDate);
            var _DaysOfWeek = (DayOfWeek)Enum.Parse(typeof(DayOfWeek), weekDay);
            List<data> allStocks = new List<data>();
            if (_DaysOfWeek != DayOfWeek.Saturday && _DaysOfWeek != DayOfWeek.Sunday)
            {
                int count = 0;
                while (_firstdate <= _lastdate)
                {
                    DateTime query;
                    if (count == 0)
                    {
                        query = _firstdate;
                    }
                    else
                    {
                        query = _firstdate.AddDays(count);
                        if (query.DayOfWeek != _DaysOfWeek)
                        {
                            ++count;
                            continue;
                        }
                    }
                    using (var api = new HttpClient())
                    {
                        var request = api.GetAsync($"https://jsonmock.hackerrank.com/api/stocks/?date={query.ToString("d-MMMM-yyyy")}").GetAwaiter().GetResult();
                        if (request.IsSuccessStatusCode)
                        {
                            var response = request.Content.ReadAsStringAsync().GetAwaiter().GetResult();
                            var stocks = JsonConvert.DeserializeObject<stock>(response);
                            foreach (var item in stocks.data)
                            {
                                Console.WriteLine($"{item.date.ToString("d-MMMM-yyyy")} {item.open} {item.close}");
                            }

                        }
                    }

                    ++count;
                }
            }


        }

        public List<data> Pagenated(int pageNumber, DateTime _firstdate, DateTime _lastdate, DayOfWeek _DaysOfWeek)
        {
            using (var api = new HttpClient())
            {
                var request = api.GetAsync($"https://jsonmock.hackerrank.com/api/stocks?Page={pageNumber}").GetAwaiter().GetResult();
                if (request.IsSuccessStatusCode)
                {
                    var response = request.Content.ReadAsStringAsync().GetAwaiter().GetResult();
                    var stocks = JsonConvert.DeserializeObject<stock>(response);
                    var filteredStock = stocks.data.Where(x => x.date >= _firstdate && x.date <= _lastdate && x.date.DayOfWeek == _DaysOfWeek).ToList();
                    return filteredStock;
                }
                else
                {
                    return null;
                }
            }
        }
    }

    public class responseObject
    {
        public DateTime date { get; set; }
        public decimal open { get; set; }
        public decimal close { get; set; }
        public decimal high { get; set; }
        public decimal low { get; set; }
    }
    public class temp
    {
        public temp()
        {

        }
        public string food { get; set; }
        public tp quantity { get; set; }
        public double time { get; set; }
        public decimal youtubeurl { get; set; }
    }

    public class Viewtemp
    {
        public Viewtemp()
        {

        }
        public string food { get; set; }
        public tp quantity { get; set; }
        public double time { get; set; }
        public decimal youtubeurl { get; set; }
    }

    public class tp
    {
        public tp()
        {

        }
        public int Id { get; set; }
        public DateTime MyDate { get; set; }
    }


    public class MyRSAImpl
    {

        private static byte[] rsaOperation(byte[] data, BigInteger exp, BigInteger mod)
        {
            BigInteger bData = new BigInteger(
                data    //our data block
                .Reverse()  //BigInteger has another byte order
                .Concat(new byte[] { 0 }) // append 0 so we are allways handling positive numbers
                .ToArray() // constructor wants an array
            );
            return
                BigInteger.ModPow(bData, exp, mod) // the RSA operation itself
                .ToByteArray() //make bytes from BigInteger
                .Reverse() // back to "normal" byte order
                .ToArray(); // return as byte array

            /*
             * 
             * A few words on Padding:
             * 
             * you will want to strip padding after decryption or apply before encryption 
             * 
             */
        }

        public static byte[] plainEncryptPriv(byte[] data, RSAParameters key)
        {
            MyRSAParams myKey = MyRSAParams.fromRSAParameters(key);
            return rsaOperation(data, myKey.privExponent, myKey.Modulus);
        }
        public static byte[] plainEncryptPub(byte[] data, RSAParameters key)
        {
            MyRSAParams myKey = MyRSAParams.fromRSAParameters(key);
            return rsaOperation(data, myKey.pubExponent, myKey.Modulus);
        }
        public static byte[] plainDecryptPriv(byte[] data, RSAParameters key)
        {
            MyRSAParams myKey = MyRSAParams.fromRSAParameters(key);
            return rsaOperation(data, myKey.privExponent, myKey.Modulus);
        }
        public static byte[] plainDecryptPub(byte[] data, RSAParameters key)
        {
            MyRSAParams myKey = MyRSAParams.fromRSAParameters(key);
            return rsaOperation(data, myKey.pubExponent, myKey.Modulus);
        }

    }

    public class MyRSAParams
    {
        public static MyRSAParams fromRSAParameters(RSAParameters key)
        {
            var ret = new MyRSAParams();
            ret.Modulus = new BigInteger(key.Modulus.Reverse().Concat(new byte[] { 0 }).ToArray());
            ret.privExponent = new BigInteger(key.D.Reverse().Concat(new byte[] { 0 }).ToArray());
            ret.pubExponent = new BigInteger(key.Exponent.Reverse().Concat(new byte[] { 0 }).ToArray());

            return ret;
        }
        public BigInteger Modulus;
        public BigInteger privExponent;
        public BigInteger pubExponent;
    }
    public static class SimpleObjectMapper
    {

    }

    public class data
    {
        public DateTime date { get; set; }
        public decimal open { get; set; }
        public decimal close { get; set; }
        public decimal high { get; set; }
        public decimal low { get; set; }
    }

    public class stock
    {
        public int page { get; set; }
        public int per_page { get; set; }
        public int total { get; set; }
        public int total_pages { get; set; }
        public List<data> data { get; set; }
    }
}
