using CustodianEmailSMSGateway.Email;
using CustodianEmailSMSGateway.SMS;
using DapperLayer.Dapper.Core;
using DataStore.ExtensionMethods;
using DataStore.Models;
using DataStore.repository;
using DataStore.Utilities;
using DataStore.ViewModels;
using NLog;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Web;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Cors;

namespace CustodianEveryWhereV2._0.Controllers
{

    [ApiController]
    [Route("api/[controller]")]
    public class AgentController : ControllerBase
    {
        private static Logger log = LogManager.GetCurrentClassLogger();
        private store<ApiConfiguration> _apiconfig = null;
        private Utility util = null;
        private store<AgentTransactionLogs> trans_logs = null;
        private Core<policyInfo> _policyinfo = null;
        private store<AgentServices> _agent = null;
        public AgentController()
        {
            _apiconfig = new store<ApiConfiguration>();
            util = new Utility();
            trans_logs = new store<AgentTransactionLogs>();
            _policyinfo = new Core<policyInfo>();
            _agent = new store<AgentServices>();
        }

        [HttpPost("{pol_detials?}")]
        public async Task<policy_data> GetPolicyDetails(policydetails pol_detials)
        {
            try
            {
                if (!ModelState.IsValid && !pol_detials.use_vehicle_reg_only)
                {
                    log.Info($"All request parameters are mandatory for policy search {pol_detials.policy_number}");
                    return new policy_data
                    {
                        status = 203,
                        message = "All request parameters are mandatory"
                    };
                }
                var headerValues = HttpContext.Current.Request.Headers.Get("Authorization");

                if (string.IsNullOrEmpty(headerValues))
                {
                    log.Info($"Authorization denied for policy search {pol_detials.policy_number}");
                    return new policy_data
                    {
                        status = 205,
                        message = "Authorization denied"
                    };
                }

                var validate_headers = await util.ValidateHeaders(headerValues, pol_detials.merchant_id);

                if (!validate_headers)
                {
                    log.Info($"Authorization failed feature for policy search {pol_detials.policy_number}");
                    return new policy_data
                    {
                        status = 407,
                        message = "Authorisation failed"
                    };
                }

                var check_user_function = await util.CheckForAssignedFunction("GetPolicyDetails", pol_detials.merchant_id);
                if (!check_user_function)
                {
                    log.Info($"Permission denied from accessing this feature for policy search {pol_detials.policy_number}");
                    return new policy_data
                    {
                        status = 401,
                        message = "Permission denied from accessing this feature"
                    };
                }
                var merchantConfig = await _apiconfig.FindOneByCriteria(x => x.merchant_id == pol_detials.merchant_id);
                //check if request is from GT Bank and apply sha512
                if (merchantConfig != null && merchantConfig.merchant_name == GlobalConstant.GTBANK)
                {
                    if (string.IsNullOrEmpty(pol_detials.checksum))
                    {
                        log.Info($"Checksum is required {pol_detials.policy_number}");
                        return new policy_data
                        {
                            status = 405,
                            message = "Checksum is required for this merchant"
                        };
                    }

                    var computedhash = await util.Sha512(merchantConfig.merchant_id + pol_detials.policy_number + merchantConfig.secret_key);
                    if (!await util.ValidateGTBankUsers(pol_detials.checksum, computedhash))
                    {
                        log.Info($"Invalid hash for {pol_detials.policy_number}");
                        return new policy_data
                        {
                            status = 403,
                            message = "Invalid checksum"
                        };
                    }
                }

                if (!pol_detials.use_vehicle_reg_only)
                {
                    using (var api = new CustodianAPI.PolicyServicesSoapClient())
                    {
                        var request = api.GetMorePolicyDetails(GlobalConstant.merchant_id, GlobalConstant.password, pol_detials.subsidiary.ToString(), pol_detials.policy_number);
                        log.Info($"raw api response  {Newtonsoft.Json.JsonConvert.SerializeObject(request)}");
                        if (request == null || request.PolicyNo == "NULL")
                        {
                            log.Info($"Invalid policy number for policy search {pol_detials.policy_number}");
                            return new policy_data
                            {
                                status = 202,
                                message = "Invalid policy number"
                            };
                        }

                        if (pol_detials.vehicle_regs != null && pol_detials.vehicle_regs.Count() > 0)
                        {
                            var motoRequest = api.GetMotorPolicyDetails(GlobalConstant.merchant_id, GlobalConstant.password, pol_detials.policy_number);
                            if (motoRequest.Length == 0)
                            {
                                return new policy_data
                                {
                                    status = 202,
                                    message = $"Unable to fetch vehicles for {pol_detials.policy_number}"
                                };
                            }
                            var filter = motoRequest.Where(x => pol_detials.vehicle_regs.Any(y => y.ToUpper().RemoveWhiteSpaces() == x.mVehReg?.Trim()?.ToUpper()?.RemoveWhiteSpaces())).ToList();
                            var transposed = filter.Select(x => new
                            {
                                RegNumber = x.mVehReg?.Trim(),
                                ChasisNumber = x.mChasisNum?.Trim(),
                                EngineNumber = x.mENGINENUM?.Trim(),
                                ExpiryDate = Convert.ToDateTime(x.mEnddate?.Trim()).ToString("dd-MMM-yyyy"),
                                StartDate = Convert.ToDateTime(x.mStartdate?.Trim()).ToString("dd-MMM-yyyy"),
                                Color = x.mVEHCOLOR?.Trim(),
                                Make = x.mVEHMAKE?.Trim(),
                                Premium = Convert.ToDouble(x.mVEHPREMIUM?.Trim()),
                                Value = Convert.ToDouble(x.mVEHVALUE?.Trim()),
                                InsuredName = x.mInsuredname?.Trim(),
                                Status = x.Status?.Trim(),
                                EngineCapacity = x.mHPCAPACITY?.Trim()

                            }).ToList();

                            return new policy_data
                            {
                                status = 200,
                                message = "policy number is valid",
                                data = new TransPoseGetPolicyDetails
                                {
                                    agenctNameField = request.AgenctName?.Trim(),
                                    bizBranchField = request.BizBranch?.Trim(),
                                    agenctNumField = request.AgenctNum?.Trim(),
                                    bizUnitField = request.BizUnit?.Trim(),
                                    enddateField = request.Enddate,
                                    insAddr1Field = request.InsAddr1?.Trim(),
                                    insAddr2Field = request.InsAddr2?.Trim(),
                                    insAddr3Field = request.InsAddr3?.Trim(),
                                    insLGAField = request.InsLGA?.Trim(),
                                    insOccupField = request.InsOccup?.Trim(),
                                    insStateField = request.InsState?.Trim(),
                                    instPremiumField = request.InstPremium,
                                    insuredEmailField = (Config.isDemo) ? "CustodianDirect@gmail.com" : (request.InsuredEmail?.Trim() == null || string.IsNullOrEmpty(request.InsuredEmail?.Trim()) || request.InsuredEmail?.Trim() == "NULL") ? $"{Guid.NewGuid().ToString().Split('-')[0]}@gmail.com" : request.InsuredEmail?.Trim(),
                                    insuredNameField = request.InsuredName?.Trim(),
                                    insuredNumField = request.InsuredNum?.Trim(),
                                    insuredOthNameField = request.InsuredOthName?.Trim(),
                                    insuredTelNumField = request.InsuredTelNum?.Trim(),
                                    policyEBusinessField = request.PolicyEBusiness?.Trim(),
                                    policyNoField = request.PolicyNo?.Trim(),
                                    startdateField = request.Startdate,
                                    sumInsField = request.SumIns,
                                    telNumField = request.TelNum?.Trim(),
                                    outPremiumField = request.OutPremium
                                },
                                hash = new
                                {
                                    checksum = await util.Sha512(merchantConfig.merchant_id + request.SumIns + request.PolicyNo?.Trim() + merchantConfig.secret_key),
                                    message = $"checksum created on {DateTime.Now}"
                                },
                                vehiclelist = transposed

                            };
                        }
                        else
                        {
                            #region - commented out
                            //dynamic transposed = null;
                            //if (pol_detials.subsidiary == subsidiary.General)
                            //{
                            //    var motoRequest = api.GetMotorPolicyDetails(GlobalConstant.merchant_id, GlobalConstant.password, pol_detials.policy_number);
                            //    if (motoRequest.Length == 0)
                            //    {
                            //        return new policy_data
                            //        {
                            //            status = 202,
                            //            message = $"Unable to fetch vehicles for {pol_detials.policy_number}"
                            //        };
                            //    }

                            //    transposed = motoRequest.Select(x => new
                            //    {
                            //        RegNumber = x.mVehReg?.Trim(),
                            //        ChasisNumber = x.mChasisNum?.Trim(),
                            //        EngineNumber = x.mENGINENUM?.Trim(),
                            //        ExpiryDate = Convert.ToDateTime(x.mEnddate?.Trim()).ToString("dd-MMM-yyyy"),
                            //        StartDate = Convert.ToDateTime(x.mStartdate?.Trim()).ToString("dd-MMM-yyyy"),
                            //        Color = x.mVEHCOLOR?.Trim(),
                            //        Make = x.mVEHMAKE?.Trim(),
                            //        Premium = Convert.ToDouble(x.mVEHPREMIUM?.Trim()),
                            //        Value = Convert.ToDouble(x.mVEHVALUE?.Trim()),
                            //        InsuredName = x.mInsuredname?.Trim(),
                            //        Status = x.Status?.Trim(),
                            //        EngineCapacity = x.mHPCAPACITY?.Trim()
                            //    }).ToList();
                            //}

                            #endregion

                            return new policy_data
                            {
                                status = 200,
                                message = "policy number is valid",
                                data = new TransPoseGetPolicyDetails
                                {
                                    agenctNameField = request.AgenctName?.Trim(),
                                    bizBranchField = request.BizBranch?.Trim(),
                                    dOBField = request.DOB,
                                    agenctNumField = request.AgenctNum?.Trim(),
                                    bizUnitField = request.BizUnit?.Trim(),
                                    enddateField = request.Enddate,
                                    insAddr1Field = request.InsAddr1?.Trim(),
                                    insAddr2Field = request.InsAddr2?.Trim(),
                                    insAddr3Field = request.InsAddr3?.Trim(),
                                    insLGAField = request.InsLGA?.Trim(),
                                    insOccupField = request.InsOccup?.Trim(),
                                    insStateField = request.InsState?.Trim(),
                                    instPremiumField = request.InstPremium,
                                    insuredEmailField = (request.InsuredEmail?.Trim() == null || string.IsNullOrEmpty(request.InsuredEmail?.Trim()) || request.InsuredEmail?.Trim() == "NULL") ? $"{Guid.NewGuid().ToString().Split('-')[0]}@gmail.com" : request.InsuredEmail?.Trim(),
                                    insuredNameField = request.InsuredName?.Trim(),
                                    insuredNumField = request.InsuredNum?.Trim(),
                                    insuredOthNameField = request.InsuredOthName?.Trim(),
                                    insuredTelNumField = request.InsuredTelNum?.Trim(),
                                    mPremiumField = request.mPremium,
                                    outPremiumField = request.OutPremium,
                                    policyEBusinessField = request.PolicyEBusiness?.Trim(),
                                    policyNoField = request.PolicyNo?.Trim(),
                                    startdateField = request.Startdate,
                                    sumInsField = request.SumIns,
                                    telNumField = request.TelNum?.Trim()
                                },
                                hash = new
                                {
                                    checksum = await util.Sha512(merchantConfig.merchant_id + request.SumIns + request.PolicyNo?.Trim() + merchantConfig.secret_key),
                                    message = $"checksum created on {DateTime.Now}"
                                },

                            };
                        }
                    }
                }
                else
                {
                    if (pol_detials.vehicle_regs.Count() > 1)
                    {
                        return new policy_data
                        {
                            status = 402,
                            message = "Only one vehicle reg is expected when 'use_vehicle_reg_only' is set to true"
                        };
                    }
                    using (var api = new CustodianAPI.PolicyServicesSoapClient())
                    {
                        var motoRequest = api.GetMotorDetailsByRegNum(GlobalConstant.merchant_id, GlobalConstant.password, pol_detials.vehicle_regs[0]);
                        if (motoRequest.Status != "200")
                        {
                            return new policy_data
                            {
                                status = 402,
                                message = $"Unable to fetech vehicle information for Reg '{pol_detials.vehicle_regs[0]}'"
                            };
                        }
                        var request2 = api.GetMorePolicyDetails(GlobalConstant.merchant_id, GlobalConstant.password, pol_detials.subsidiary.ToString(), motoRequest.mPolicyNumber?.Trim());
                        log.Info($"raw api response business unit  {Newtonsoft.Json.JsonConvert.SerializeObject(request2)}");
                        var transposed = new
                        {
                            RegNumber = motoRequest.mVehReg?.Trim(),
                            ChasisNumber = motoRequest.mChasisNum?.Trim(),
                            EngineNumber = motoRequest.mENGINENUM?.Trim(),
                            ExpiryDate = Convert.ToDateTime(motoRequest.mEnddate?.Trim()).ToString("dd-MMM-yyyy"),
                            StartDate = Convert.ToDateTime(motoRequest.mStartdate?.Trim()).ToString("dd-MMM-yyyy"),
                            Color = motoRequest.mVEHCOLOR?.Trim(),
                            Make = motoRequest.mVEHMAKE?.Trim(),
                            Premium = Convert.ToDouble(motoRequest.mVEHPREMIUM?.Trim()),
                            Value = Convert.ToDouble(motoRequest.mVEHVALUE?.Trim()),
                            InsuredName = motoRequest.mInsuredname?.Trim(),
                            Status = motoRequest.Status?.Trim(),
                            EngineCapacity = motoRequest.mHPCAPACITY?.Trim(),
                            PolicyNumber = motoRequest.mPolicyNumber?.Trim(),
                            BusinessUnit = request2?.BizUnit?.Trim(),
                            Email = request2?.InsuredEmail?.Trim(),
                            PhoneNumber = request2?.InsuredNum?.Trim()
                        };

                        return new policy_data
                        {
                            status = 200,
                            message = "Vehicle reg is valid",
                            data = transposed
                        };
                    }
                }
            }
            catch (Exception ex)
            {
                log.Error(ex.Message);
                log.Error(ex.StackTrace);
                log.Error(ex.InnerException);
                return new policy_data
                {
                    status = 404,
                    message = "system malfunction"
                };
            }
        }

        [HttpPost("{post?}")]
        public async Task<policy_data> PostTransaction(PostTransaction post)
        {
            try
            {
                log.Info($"Object from page {Newtonsoft.Json.JsonConvert.SerializeObject(post)}");
                if (!ModelState.IsValid)
                {
                    log.Info($"All request parameters are mandatory for policy search(PostTransaction) {post.policy_number}");
                    return new policy_data
                    {
                        status = 203,
                        message = "All request parameters are mandatory"
                    };
                }

                var headerValues = HttpContext.Current.Request.Headers.Get("Authorization");

                if (string.IsNullOrEmpty(headerValues))
                {
                    log.Info($"Authorization denied for policy search(PostTransaction)  {post.policy_number}");
                    return new policy_data
                    {
                        status = 205,
                        message = "Authorization denied"
                    };
                }

                var merchantConfig = await _apiconfig.FindOneByCriteria(x => x.merchant_id == post.merchant_id);
                //check if request is from GT Bank and apply sha512

                string channelName = "PAYSTACK";
                channelName = (merchantConfig.merchant_name.ToLower().Contains("agent")) ? "MPOS" : "PAYSTACK";
                if (merchantConfig != null && merchantConfig.merchant_name == GlobalConstant.GTBANK)
                {
                    if (string.IsNullOrEmpty(post.checksum))
                    {
                        log.Info($"Checksum is required {post.policy_number}");
                        return new policy_data
                        {
                            status = 405,
                            message = "Checksum is required for this merchant"
                        };
                    }
                    var computedhash = await util.Sha512(merchantConfig.merchant_id + post.policy_number + post.premium + post.reference_no + merchantConfig.secret_key);
                    if (!await util.ValidateGTBankUsers(post.checksum, computedhash))
                    {
                        log.Info($"Invalid hash for {post.policy_number}");
                        return new policy_data
                        {
                            status = 403,
                            message = "Invalid checksum"
                        };
                    }
                    channelName = merchantConfig.merchant_name.ToUpper();
                }

                if (!merchantConfig.merchant_name.ToLower().Contains("adapt"))
                {
                    channelName = merchantConfig.merchant_name.ToUpper();
                }

                var validate_headers = await util.ValidateHeaders(headerValues, post.merchant_id);

                if (!validate_headers)
                {
                    log.Info($"Authorization failed feature for policy search(PostTransaction)  {post.policy_number}");
                    return new policy_data
                    {
                        status = 407,
                        message = "Authorisation failed"
                    };
                }
                //NumberFormatInfo setPrecision = new NumberFormatInfo();
                //setPrecision.NumberDecimalDigits = 1;
                var new_trans = new AgentTransactionLogs
                {
                    biz_unit = post.biz_unit?.Trim(),
                    createdat = DateTime.Now,
                    description = post.description?.Trim(),
                    policy_number = post.policy_number?.Trim().ToUpper(),
                    premium = post.premium,
                    reference_no = post.reference_no?.Trim(),
                    status = !string.IsNullOrEmpty(post.payment_narrtn) ? post.payment_narrtn : post.status,
                    subsidiary = ((subsidiary)post.subsidiary).ToString(),
                    email_address = post.email_address?.Trim(),
                    issured_name = post.issured_name?.Trim(),
                    phone_no = post.phone_no?.Trim(),
                    merchant_id = post.merchant_id,
                    vehicle_reg_no = post.vehicle_reg_no?.Trim() ?? "",
                    pushDate = DateTime.Now
                };

                if (post.payment_narrtn?.ToLower() == "failed" || post.status?.ToLower() == "failed")
                {
                    await trans_logs.Save(new_trans);
                    log.Info($"Transaction failed dont push to abs");
                    return new policy_data
                    {
                        status = 409,
                        message = post.description
                    };
                }

                if (post.payment_narrtn?.ToLower() == "pending" || post.status?.ToLower() == "pending")
                {
                    new_trans.reference_key = $"{Guid.NewGuid()}-{new_trans.policy_number?.Trim().Replace("/", "-").Replace("\\", "-")}".ToUpper();
                    await trans_logs.Save(new_trans);
                    log.Info($"Transaction is Pending for confirmation");
                    return new policy_data
                    {
                        status = 200,
                        message = "Transaction Logged for Confirmation",
                        data = new
                        {
                            reference_key = new_trans.reference_key
                        }
                    };
                }

                using (var api = new CustodianAPI.PolicyServicesSoapClient())
                {
                    var request = await api.SubmitPaymentRecordAsync(GlobalConstant.merchant_id, GlobalConstant.password, post.policy_number,
                        post.subsidiary.ToString(), post.payment_narrtn ?? new_trans.description, DateTime.Now,
                        DateTime.Now, post.reference_no, new_trans.issured_name, "", "", "", new_trans.phone_no, new_trans.email_address,
                        "", "", "", post.biz_unit, post.premium, 0, channelName, "RW", "", new_trans.vehicle_reg_no ?? "");
                    log.Info($"raw response from api {request.Passing_Payment_PostSourceResult}");
                    if (string.IsNullOrEmpty(request.Passing_Payment_PostSourceResult) || request.Passing_Payment_PostSourceResult != "1")
                    {
                        log.Info($"Something went wrong while processing your transaction search(PostTransaction)  {post.policy_number}");
                        return new policy_data
                        {
                            status = 409,
                            message = "Something went wrong while processing your transaction"
                        };
                    }
                    await trans_logs.Save(new_trans);
                    //http://41.216.175.114/WebPortal/Receipt.aspx?mUser=CUST_WEB&mCert={}&mCert2={}
                    var url = ConfigurationManager.AppSettings["RecieptBaseUrl"];
                    if (!string.IsNullOrEmpty(post.phone_no))
                    {
                        var phone = post.phone_no.Trim();
                        if (!phone.StartsWith("234"))
                        {
                            phone = "234" + phone.Remove(0, 1);
                        }

                        var sms = new SendSMS();
                        string message = $@"Dear {post.issured_name} We have acknowledged receipt of NGN {post.premium} premium payment.We will apply this to your policy number {post.policy_number.ToUpper()}";
                        if (!GlobalConstant.IsDemoMode)
                        {
                            await sms.Send_SMS(message, phone);
                        }
                    }

                    return new policy_data
                    {
                        status = 200,
                        message = "Transaction was successful",
                        data = new Dictionary<string, string>
                        {
                            {"RecieptUrl",url+$"Receipt.aspx?mUser=CUST_WEB&mCert={post.reference_no}&mCert2={post.reference_no}" },
                            {"message",$"Notification of payment sent to customer mobile number ({(GlobalConstant.IsDemoMode?"Message are not sent in demo mode":"")})" }
                        },
                        hash = new
                        {
                            checksum = await util.Sha512(merchantConfig.merchant_id + post.reference_no + post.policy_number + post.premium + merchantConfig.secret_key),
                            message = $"checksum created on {DateTime.Now}"
                        }
                    };
                }
            }
            catch (Exception ex)
            {
                log.Error(ex.Message);
                log.Error(ex.StackTrace);
                log.Error(ex.InnerException);
                return new policy_data
                {
                    status = 404,
                    message = "system malfunction"
                };
            }
        }

        [HttpGet("{reference_no?}/{merchant_id?}/{checksum?}")]
        public async Task<policy_data> GetTransactionWithReferenceNumber(string reference_no, string merchant_id, string checksum)
        {
            try
            {
                var headerValues = HttpContext.Current.Request.Headers.Get("Authorization");

                if (string.IsNullOrEmpty(headerValues))
                {
                    log.Info($"Authorization denied for policy GetTransaction()  {reference_no}");
                    return new policy_data
                    {
                        status = 205,
                        message = "Authorization denied"
                    };
                }

                var merchantConfig = await _apiconfig.FindOneByCriteria(x => x.merchant_id == merchant_id);
                //check if request is from GT Bank and apply sha512
                if (merchantConfig != null && merchantConfig.merchant_name == GlobalConstant.GTBANK)
                {
                    if (string.IsNullOrEmpty(checksum))
                    {
                        log.Info($"Checksum is required {reference_no}");
                        return new policy_data
                        {
                            status = 405,
                            message = "Checksum is required for this merchant"
                        };
                    }
                    var computedhash = await util.Sha512(merchant_id + reference_no + merchantConfig.secret_key);
                    if (!await util.ValidateGTBankUsers(checksum, computedhash))
                    {
                        log.Info($"Invalid hash for {reference_no}");
                        return new policy_data
                        {
                            status = 403,
                            message = "Invalid checksum"
                        };
                    }
                }

                var validate_headers = await util.ValidateHeaders(headerValues, merchant_id);

                if (!validate_headers)
                {
                    log.Info($"Authorization failed feature for policy GetTransactionWithReferenceNumber(PostTransaction)  {reference_no}");
                    return new policy_data
                    {
                        status = 407,
                        message = "Authorizatikon failed"
                    };
                }

                var getTransaction = await trans_logs.FindOneByCriteria(x => x.reference_no?.Trim().ToLower() == reference_no?.Trim().ToLower() && x.merchant_id?.ToLower() == merchant_id?.Trim().ToLower());
                if (getTransaction == null)
                {
                    log.Info($"Transaction not found with reference {reference_no}");
                    return new policy_data
                    {
                        status = 303,
                        message = "Transaction not found",
                        data = new
                        {
                            message = $"Transaction not found with reference number '({reference_no})'"
                        }
                    };
                }
                else
                {
                    return new policy_data
                    {
                        status = 200,
                        message = "Transaction found",
                        data = new
                        {
                            merchantName = merchantConfig.merchant_name,
                            policyNumber = getTransaction.policy_number,
                            subsidiary = getTransaction.subsidiary,
                            businessUnit = getTransaction.biz_unit,
                            referenceNumber = getTransaction.reference_no,
                            status = getTransaction.status,
                            description = getTransaction.description,
                            amountPaid = getTransaction.premium,
                            transactionDateTime = getTransaction.createdat,
                            insuredName = getTransaction.issured_name,
                            emailAddress = getTransaction.email_address
                        },
                        hash = new
                        {
                            checksum = await util.Sha512(merchantConfig.merchant_id + getTransaction.reference_no + getTransaction.policy_number + merchantConfig.secret_key),
                            message = $"checksum created on {DateTime.Now}"
                        }
                    };
                }

            }
            catch (Exception ex)
            {
                log.Error(ex.Message);
                log.Error(ex.StackTrace);
                log.Error(ex.InnerException);
                return new policy_data
                {
                    status = 404,
                    message = "system malfunction"
                };
            }
        }

        [HttpPost("{agent?}")]
        public async Task<policy_data> SetupAgent(AgentModel agent)
        {
            try
            {
                log.Info($"agent from page {Newtonsoft.Json.JsonConvert.SerializeObject(agent)}");
                if (!ModelState.IsValid)
                {
                    log.Info($"All request parameters are mandatory for policy search(SetupAgent) {agent.merchant_id}");
                    return new policy_data
                    {
                        status = 203,
                        message = "All request parameters are mandatory"
                    };
                }

                // check if user has access to function

                var check_user_function = await util.CheckForAssignedFunction("SetupAgent", agent.merchant_id);
                if (!check_user_function)
                {
                    return new policy_data
                    {
                        status = 401,
                        message = "Permission denied from accessing this feature"
                    };
                }
                //check api config
                var config = await _apiconfig.FindOneByCriteria(x => x.merchant_id == agent.merchant_id.Trim());
                if (config == null)
                {
                    log.Info($"Invalid merchant Id {agent.merchant_id}");
                    return new policy_data
                    {
                        status = 402,
                        message = "Invalid merchant Id"
                    };
                }

                // validate hash
                var checkhash = await util.ValidateHash2(agent.AgentCode, config.secret_key, agent.hash);
                if (!checkhash)
                {
                    log.Info($"Hash missmatched from request {agent.merchant_id}");
                    return new policy_data
                    {
                        status = 405,
                        message = "Data mismatched"
                    };
                }

                var confirmAgent = await _policyinfo.ConfirmAgentCode(agent.AgentCode?.Trim());

                if (confirmAgent == null)
                {
                    log.Info($"invalid agent code{agent.AgentCode}");
                    return new policy_data
                    {
                        status = 401,
                        message = "Invalid agent code"
                    };
                }


                if (confirmAgent.Agnt_Status?.ToUpper() != "ACTIVE")
                {
                    log.Info($"Agent is not active {agent.AgentCode}");
                    return new policy_data
                    {
                        status = 401,
                        message = $"Agent is not active {GlobalConstant.CONTACT}"
                    };
                }

                bool isemail = util.IsValid(confirmAgent.Agnt_Email?.Trim());
                bool isphone = util.isValidPhone(confirmAgent.Agnt_Phone?.Trim());
                bool isphone2 = util.isValidPhone(confirmAgent.Agnt_Phone2?.Trim());

                if (!isemail && !isphone && !isphone2)
                {
                    log.Info($"Agent email and phonenumber not profiled {agent.AgentCode}");
                    return new policy_data
                    {
                        status = 408,
                        message = $"Agent email and phonenumber not profiled {GlobalConstant.CONTACT}"
                    };
                }
                string number234 = null;
                if (isphone)
                {
                    number234 = util.numberin234(confirmAgent.Agnt_Phone?.Trim());
                }

                string number234_2 = null;
                if (isphone2)
                {
                    number234_2 = util.numberin234(confirmAgent.Agnt_Phone2?.Trim());
                }

                string aemail = null;
                if (isemail)
                {
                    aemail = confirmAgent.Agnt_Email?.Trim();
                }
                bool SendOTPForInCompleteSignUp = false;
                var checkForAgentCode = await _agent.FindOneByCriteria(x => x.agent_ref_id == confirmAgent.AgntRefID?.Trim().ToUpper());
                if (checkForAgentCode != null)
                {
                    if (checkForAgentCode.is_setup_completed)
                    {
                        log.Info($"Agent has already completed onborading {agent.AgentCode}");
                        return new policy_data
                        {
                            status = 201,
                            message = $"Agent already completed sign-up process",
                            data = new
                            {
                                AgentName = confirmAgent.Agnt_Name,
                                AgentEmail = confirmAgent.Agnt_Email,
                                AgntRefID = confirmAgent.AgntRefID,
                                AgentNumber = number234 ?? number234_2
                            }
                        };
                    }
                    else
                    {
                        SendOTPForInCompleteSignUp = true;
                    }

                }
                string generate_otp = null;
                string validationKey = null;

                if (isphone2 || isphone)
                {
                    generate_otp = await util.GenerateOTP(false, number234 ?? number234_2, "POLICYSERVICE", Platforms.ADAPT);
                    validationKey = "PHONE";
                }
                else
                {
                    generate_otp = await util.GenerateOTP(false, aemail, "POLICYSERVICE", Platforms.ADAPT);
                    validationKey = "EMAIL";
                }


                if (!SendOTPForInCompleteSignUp)
                {
                    var save_agent = new AgentServices
                    {
                        agentname = confirmAgent.Agnt_Name.Trim().ToUpper(),
                        agent_ref_id = confirmAgent.AgntRefID?.Trim().ToUpper(),
                        createdat = DateTime.Now,
                        email = confirmAgent.Agnt_Email,
                        is_setup_completed = false,
                        phonenumber = number234 ?? number234_2,
                        updatedat = DateTime.Now,
                        os = agent.Os
                    };
                    await _agent.Save(save_agent);
                }
                else
                {
                    checkForAgentCode.updatedat = DateTime.Now;
                    await _agent.Update(checkForAgentCode);
                }

                if (!string.IsNullOrEmpty(aemail))
                {
                    string messageBody = $"Adapt Policy Services authentication code <br/><br/><h2><strong>{generate_otp}</strong></h2>";
                    var template = System.IO.File.ReadAllText(HttpContext.Current.Server.MapPath("~/Cert/Adapt.html"));
                    StringBuilder sb = new StringBuilder(template);
                    sb.Replace("#CONTENT#", messageBody);
                    sb.Replace("#TIMESTAMP#", string.Format("{0:F}", DateTime.Now));
                    var imagepath = HttpContext.Current.Server.MapPath("~/Images/adapt_logo.png");
                    List<string> cc = new List<string>();
                    // cc.Add("technology@custodianplc.com.ng");  // Todo: move to webconfig 

                    string test_email = "oscardybabaphd@gmail.com"; //Todo:  move to webconfig
                    //email.email
                    var test = Config.isDemo ? "Test" : null;
                    var email = Config.isDemo ? test_email : aemail;
                    new SendEmail().Send_Email(email,
                               $"Adapt-PolicyServices Authentication {test}",
                               sb.ToString(), $"PolicyServices Authentication {test}",
                               true, imagepath, null, null, null);
                }

                if (!Config.isDemo)
                {
                    if (isphone2 || isphone)
                    {
                        //phone = "2348069314541";
                        await new SendSMS().Send_SMS($"Adapt OTP: {generate_otp}", number234 ?? number234_2);
                    }
                }

                return new policy_data
                {
                    message = "successful",
                    status = 200,
                    data = new
                    {
                        AgentNumber = number234 ?? number234_2,
                        AgentName = confirmAgent.Agnt_Name,
                        AgentEmail = confirmAgent.Agnt_Email,
                        AgntRefID = confirmAgent.AgntRefID,
                        ValidateionKey = validationKey
                    }
                };
            }
            catch (Exception ex)
            {

                log.Error(ex.Message);
                log.Error(ex.StackTrace);
                log.Error(ex.InnerException);
                return new policy_data
                {
                    status = 404,
                    message = "system malfunction"
                };
            }
        }

        [HttpGet("{merchant_id?}/{newpin?}/{otp?}/{agent_ref_id?}/{validationKey?}/{hash?}")]
        public async Task<policy_data> ResetAgentPIN(string merchant_id, string newpin, string otp, string agent_ref_id, string validationKey, string hash)
        {
            try
            {
                var check_user_function = await util.CheckForAssignedFunction("ResetAgentPIN", merchant_id);
                if (!check_user_function)
                {
                    return new policy_data
                    {
                        status = (int)HttpStatusCode.Unauthorized,
                        message = "Permission denied from accessing this feature"
                    };
                }

                var config = await _apiconfig.FindOneByCriteria(x => x.merchant_id == merchant_id.Trim());
                if (config == null)
                {
                    log.Info($"Invalid merchant Id {merchant_id}");
                    return new policy_data
                    {
                        status = (int)HttpStatusCode.Forbidden,
                        message = "Invalid merchant Id"
                    };
                }

                var checkhash = await util.ValidateHash2(agent_ref_id + newpin + otp, config.secret_key, hash);
                if (!checkhash)
                {
                    log.Info($"Hash missmatched from request {merchant_id}");
                    return new policy_data
                    {
                        status = 405,
                        message = "Data mismatched"
                    };
                }

                var getCustomer = await _agent.FindOneByCriteria(x => x.agent_ref_id == agent_ref_id);


                if (getCustomer == null)
                {
                    log.Info($"Invalid agent_ref_id Id {agent_ref_id}");
                    return new policy_data
                    {
                        status = (int)HttpStatusCode.Forbidden,
                        message = "Invalid agent Id"
                    };
                }
                bool validateOTP = false;
                if (validationKey == "EMAIL")
                {
                    validateOTP = await util.ValidateOTP(otp, getCustomer.email?.Trim().ToLower());
                }
                else
                {
                    validateOTP = await util.ValidateOTP(otp, getCustomer.phonenumber?.Trim());
                }

                if (!validateOTP)
                {
                    log.Info($"Invalid agent_ref_id OTP {agent_ref_id}");
                    return new policy_data
                    {
                        status = (int)HttpStatusCode.Forbidden,
                        message = "Invalid OTP"
                    };
                }
                var _pin = util.Sha256(newpin);
                getCustomer.pin = _pin;
                getCustomer.updatedat = DateTime.UtcNow;
                if (await _agent.Update(getCustomer))
                {
                    log.Info($"Pin update successfully {agent_ref_id}");
                    return new policy_data
                    {
                        status = (int)HttpStatusCode.OK,
                        message = "Pin update successfully"
                    };
                }
                else
                {
                    log.Info($"Updated was not successful {agent_ref_id}");
                    return new policy_data
                    {
                        status = (int)HttpStatusCode.NotAcceptable,
                        message = "Updated was not successful"
                    };
                }
            }
            catch (Exception ex)
            {
                log.Error(ex.Message);
                log.Error(ex.StackTrace);
                log.Error((ex.InnerException != null) ? ex.InnerException.ToString() : "");
                return new policy_data { message = "System error, Try Again", status = (int)HttpStatusCode.NotFound };
            }
        }

        [HttpPost("{validate?}")]
        public async Task<policy_data> FinaliseAgentSetup(ValidateAgent validate)
        {
            try
            {

                var check_user_function = await util.CheckForAssignedFunction("FinaliseAgentSetup", validate.merchant_id);
                if (!check_user_function)
                {
                    return new policy_data
                    {
                        status = (int)HttpStatusCode.Unauthorized,
                        message = "Permission denied from accessing this feature"
                    };
                }

                var config = await _apiconfig.FindOneByCriteria(x => x.merchant_id == validate.merchant_id.Trim());
                if (config == null)
                {
                    log.Info($"Invalid merchant Id {validate.merchant_id}");
                    return new policy_data
                    {
                        status = (int)HttpStatusCode.Forbidden,
                        message = "Invalid merchant Id"
                    };
                }
                var getRecord = await _agent.FindOneByCriteria(x => x.agent_ref_id == validate.agent_ref_id);
                if (getRecord == null)
                {
                    return new policy_data
                    {
                        status = 206,
                        message = "Validation entry not found"
                    };
                }

                if (getRecord != null && getRecord.is_setup_completed)
                {
                    log.Info($"valid merchant Id {validate.merchant_id} setup completed");
                    return new policy_data
                    {
                        status = 201,
                        message = "User has finished setting up profile kindly login to access your policy services"
                    };
                }

                var checkhash = await util.ValidateHash2(validate.otp + validate.agent_ref_id, config.secret_key, validate.hash);
                if (!checkhash)
                {
                    log.Info($"Hash missmatched from request {validate.merchant_id}");
                    return new policy_data
                    {
                        status = 405,
                        message = "Data mismatched"
                    };
                }
                bool validateOTP = false;
                if (validate.validationKey == "EMAIL")
                {
                    validateOTP = await util.ValidateOTP(validate.otp, getRecord.email?.ToLower());
                }
                else
                {
                    validateOTP = await util.ValidateOTP(validate.otp, getRecord.phonenumber?.ToLower());
                }

                if (!validateOTP)
                {
                    log.Info($"invalid OTP for {validate.merchant_id} email {getRecord.email}");
                    return new policy_data
                    {
                        status = 405,
                        message = "Invalid OTP"
                    };
                }
                else
                {
                    if (getRecord == null)
                    {
                        return new policy_data
                        {
                            status = 407,
                            message = "Invalid customer id"
                        };
                    }
                    getRecord.updatedat = DateTime.UtcNow;
                    getRecord.is_setup_completed = true;
                    getRecord.pin = util.Sha256(validate.pin);
                    await _agent.Update(getRecord);
                    return new policy_data
                    {
                        status = 200,
                        message = "OTP Validated successfully"
                    };
                }
            }
            catch (Exception ex)
            {
                log.Error(ex.Message);
                log.Error(ex.StackTrace);
                log.Error((ex.InnerException != null) ? ex.InnerException.ToString() : "");
                return new policy_data { message = "System error, Try Again", status = (int)HttpStatusCode.NotFound };
            }
        }

        [HttpGet("{agent_ref_id?}/{merchant_id?}/{hash?}")]
        public async Task<res> GenerateOTP(string agent_ref_id, string merchant_id, string hash)
        {
            try
            {
                var check_user_function = await util.CheckForAssignedFunction("AgentGenerateOTP", merchant_id);
                if (!check_user_function)
                {
                    return new res
                    {
                        status = (int)HttpStatusCode.Unauthorized,
                        message = "Permission denied from accessing this feature"
                    };
                }

                var config = await _apiconfig.FindOneByCriteria(x => x.merchant_id == merchant_id.Trim());
                if (config == null)
                {
                    log.Info($"Invalid merchant Id {merchant_id}");
                    return new res
                    {
                        status = (int)HttpStatusCode.Forbidden,
                        message = "Invalid merchant Id"
                    };
                }

                var checkhash = await util.ValidateHash2(agent_ref_id, config.secret_key, hash);
                if (!checkhash)
                {
                    log.Info($"Hash missmatched from request {agent_ref_id}");
                    return new res
                    {
                        status = 405,
                        message = "Data mismatched"
                    };
                }
                var checkuser = await _agent.FindOneByCriteria(x => x.agent_ref_id?.Trim().ToUpper() == agent_ref_id?.Trim().ToUpper());
                if (checkuser == null)
                {
                    log.Info($"Invalid customer id {agent_ref_id}");
                    return new res
                    {
                        status = 409,
                        message = $"Agent Id is not valid"
                    };
                }

                //var generate_otp = await util.GenerateOTP(false, checkuser.email?.ToLower(), "POLICYSERVICE", Platforms.ADAPT);

                string generate_otp = null;
                string validationKey = null;

                if (!string.IsNullOrEmpty(checkuser.phonenumber))
                {
                    generate_otp = await util.GenerateOTP(false, checkuser.phonenumber, "POLICYSERVICE", Platforms.ADAPT);
                    validationKey = "PHONE";
                }
                else
                {
                    generate_otp = await util.GenerateOTP(false, checkuser.email, "POLICYSERVICE", Platforms.ADAPT);
                    validationKey = "EMAIL";
                }

                string messageBody = $"Adapt Policy Services authentication code <br/><br/><h2><strong>{generate_otp}</strong></h2>";
                var template = System.IO.File.ReadAllText(HttpContext.Current.Server.MapPath("~/Cert/Adapt.html"));
                StringBuilder sb = new StringBuilder(template);
                sb.Replace("#CONTENT#", messageBody);
                sb.Replace("#TIMESTAMP#", string.Format("{0:F}", DateTime.Now));
                var imagepath = HttpContext.Current.Server.MapPath("~/Images/adapt_logo.png");
                List<string> bcc = new List<string>();
                // bcc.Add("technology@custodianplc.com.ng");
                //use handfire
                if (!string.IsNullOrEmpty(checkuser.email))
                {
                    string test_email = "oscardybabaphd@gmail.com";
                    //email.email
                    var test = Config.isDemo ? "Test" : null;
                    var email = Config.isDemo ? test_email : checkuser.email;
                    new SendEmail().Send_Email(email,
                          $"Adapt-PolicyServices PIN Reset {test}",
                          sb.ToString(), $"Adapt-PolicyServices PIN Reset {test}",
                          true, imagepath, null, null, null);
                }

                if (!Config.isDemo)
                {
                    if (!string.IsNullOrEmpty(checkuser.phonenumber))
                    {
                        //phone = "2348069314541";
                        await new SendSMS().Send_SMS($"Adapt OTP: {generate_otp}", checkuser.phonenumber);
                    }
                }
                return new res
                {

                    message = $"OTP has been sent to email {checkuser.email} and phone {checkuser.phonenumber} attached to your policy",
                    status = (int)HttpStatusCode.OK,
                    data = new
                    {
                        validationKey = validationKey
                    }
                };
            }
            catch (Exception ex)
            {
                log.Error(ex.Message);
                log.Error(ex.StackTrace);
                log.Error((ex.InnerException != null) ? ex.InnerException.ToString() : "");
                return new res { message = "System error, Try Again", status = (int)HttpStatusCode.NotFound };
            }
        }

        [HttpGet("{merchant_id?}/{pin?}/{agent_ref_id?}/{hash?}")]
        public async Task<res> GetAgentPolicies(string merchant_id, string pin, string agent_ref_id, string hash)
        {
            try
            {
                var check_user_function = await util.CheckForAssignedFunction("GetAgentPolicies", merchant_id);
                if (!check_user_function)
                {
                    return new res
                    {
                        status = (int)HttpStatusCode.Unauthorized,
                        message = "Permission denied from accessing this feature"
                    };
                }

                var config = await _apiconfig.FindOneByCriteria(x => x.merchant_id == merchant_id.Trim());
                if (config == null)
                {
                    log.Info($"Invalid merchant Id {merchant_id}");
                    return new res
                    {
                        status = (int)HttpStatusCode.Forbidden,
                        message = "Invalid merchant Id"
                    };
                }

                var checkhash = await util.ValidateHash2(agent_ref_id + pin, config.secret_key, hash);
                if (!checkhash)
                {
                    log.Info($"Hash missmatched from request {merchant_id}");
                    return new res
                    {
                        status = 405,
                        message = "Data mismatched"
                    };
                }

                var encryptPin = util.Sha256(pin);
                var checkuser = await _agent.FindOneByCriteria(x => x.pin == encryptPin && x.agent_ref_id?.Trim().ToLower() == agent_ref_id?.ToLower().Trim());
                if (checkuser == null)
                {
                    log.Info($"Pin authentication failed {agent_ref_id}");
                    return new res
                    {
                        status = 409,
                        message = $"Invalid PIN"
                    };
                }

                var getPolicies = await _policyinfo.GetAgentPolicies(checkuser.agent_ref_id);
                if (getPolicies.Count() == 0)
                {
                    return new res
                    {
                        status = 206,
                        message = "No policy record found",
                    };
                }

                var groupBy = getPolicies.Select(x => new AgentPoliciesView
                {
                    Data_source = (x.Data_source == "ABS") ? "General" : "Life",
                    Email = x.Email,
                    EndDate = x.EndDate?.ToShortDateString(),
                    FullName = x.FullName?.Trim().ToUpper(),
                    Phone = x.Phone?.Trim(),
                    policy_no = x.policy_no?.Trim(),
                    Policy_status = x.Policy_status?.Trim().ToUpper(),
                    Product_lng_descr = x.Product_lng_descr?.Trim(),
                    StartDate = x.StartDate?.ToShortDateString(),
                    Sub_prod_lng_descr = x.Sub_prod_lng_descr
                }).GroupBy(x => x.Data_source);

                Dictionary<string, List<AgentPoliciesView>> groupKey = new Dictionary<string, List<AgentPoliciesView>>();
                foreach (var item in groupBy)
                {
                    groupKey.Add(item.Key, item.ToList());
                }

                return new res
                {
                    message = "successful",
                    status = 200,
                    data = new
                    {
                        Agent = checkuser,
                        Policies = groupKey
                    }
                };
            }
            catch (Exception ex)
            {
                log.Error(ex.Message);
                log.Error(ex.StackTrace);
                log.Error((ex.InnerException != null) ? ex.InnerException.ToString() : "");
                return new res { message = "System error, Try Again", status = (int)HttpStatusCode.NotFound };
            }
        }

        [HttpGet("{merchant_id?}/{reference_no?}/{reference_key?}/{hash?}")]
        public async Task<policy_data> ConfirmAgentTransaction(string merchant_id, string reference_no, string reference_key, string hash)
        {
            try
            {
                var check_user_function = await util.CheckForAssignedFunction("ConfirmAgentTransaction", merchant_id);
                if (!check_user_function)
                {
                    return new policy_data
                    {
                        status = (int)HttpStatusCode.Unauthorized,
                        message = "Permission denied from accessing this feature"
                    };
                }

                var config = await _apiconfig.FindOneByCriteria(x => x.merchant_id == merchant_id.Trim());
                if (config == null)
                {
                    log.Info($"Invalid merchant Id {merchant_id}");
                    return new policy_data
                    {
                        status = (int)HttpStatusCode.Forbidden,
                        message = "Invalid merchant Id"
                    };
                }

                var checkhash = await util.ValidateHash2(reference_no + reference_key, config.secret_key, hash);
                if (!checkhash)
                {
                    log.Info($"Hash missmatched from request {merchant_id}");
                    return new policy_data
                    {
                        status = 405,
                        message = "Data mismatched"
                    };
                }

                var getTransaction = await trans_logs.FindOneByCriteria(x => x.reference_key?.Trim() == reference_key?.Trim() && x.status?.ToLower().Trim() == "pending");
                if (getTransaction == null)
                {
                    log.Info($"Transaction details not found with reference key {reference_key}");
                    return new policy_data
                    {
                        status = 302,
                        message = $"Transaction details not found with reference key {reference_key}"
                    };
                }

                using (var api = new CustodianAPI.PolicyServicesSoapClient())
                {
                    getTransaction.reference_no = reference_no;
                    getTransaction.status = "SUCCESS";
                    getTransaction.pushDate = DateTime.Now;
                    getTransaction.description = "PAYMENT SUCCESSFUL";
                    var request = await api.SubmitPaymentRecordAsync(GlobalConstant.merchant_id, GlobalConstant.password, getTransaction.policy_number,
                        getTransaction.subsidiary.ToString(), getTransaction.description, DateTime.Now,
                        DateTime.Now, getTransaction.reference_no, getTransaction.issured_name, "", "", "", getTransaction.phone_no, getTransaction.email_address,
                        "", "", "", getTransaction.biz_unit, getTransaction.premium, 0, "PAYSTACK", "RW", "", getTransaction.vehicle_reg_no ?? "");
                    log.Info($"raw response from api {request.Passing_Payment_PostSourceResult}");
                    if (string.IsNullOrEmpty(request.Passing_Payment_PostSourceResult) || request.Passing_Payment_PostSourceResult != "1")
                    {
                        log.Info($"Something went wrong while processing your transaction search(PostTransaction)  {getTransaction.policy_number}");
                        return new policy_data
                        {
                            status = 409,
                            message = "Something went wrong while processing your transaction"
                        };
                    }
                    await trans_logs.Update(getTransaction);
                    //http://41.216.175.114/WebPortal/Receipt.aspx?mUser=CUST_WEB&mCert={}&mCert2={}
                    var url = ConfigurationManager.AppSettings["RecieptBaseUrl"];
                    if (!string.IsNullOrEmpty(getTransaction.phone_no))
                    {
                        var phone = getTransaction.phone_no.Trim();
                        if (!phone.StartsWith("234"))
                        {
                            phone = "234" + phone.Remove(0, 1);
                        }

                        var sms = new SendSMS();
                        string message = $@"Dear {getTransaction.issured_name} We have acknowledged receipt of NGN {getTransaction.premium} premium payment.We will apply this to your policy number {getTransaction.policy_number.ToUpper()}";
                        if (!GlobalConstant.IsDemoMode)
                        {
                            await sms.Send_SMS(message, phone);
                        }
                    }

                    //send firebase notification

                    return new policy_data
                    {
                        status = 200,
                        message = "Transaction was successful",
                        data = new Dictionary<string, string>
                        {
                            {"RecieptUrl",url+$"Receipt.aspx?mUser=CUST_WEB&mCert={getTransaction.reference_no}&mCert2={getTransaction.reference_no}" },
                            {"message",$"Notification of payment sent to customer mobile number ({(GlobalConstant.IsDemoMode?"Message are not sent in demo mode":"")})" }
                        },
                        hash = new
                        {
                            checksum = await util.Sha512(config.merchant_id + getTransaction.reference_no + getTransaction.policy_number + getTransaction.premium + config.secret_key),
                            message = $"checksum created on {DateTime.Now}"
                        }
                    };
                }
            }
            catch (Exception ex)
            {
                log.Error(ex.Message);
                log.Error(ex.StackTrace);
                log.Error((ex.InnerException != null) ? ex.InnerException.ToString() : "");
                return new policy_data { message = "System error, Try Again", status = (int)HttpStatusCode.NotFound };
            }
        }
    }
}
