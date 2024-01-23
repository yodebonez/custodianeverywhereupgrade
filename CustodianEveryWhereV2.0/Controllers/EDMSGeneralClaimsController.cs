using CustodianEveryWhereV2._0.ActionFilters;
using DataStore.Models;
using DataStore.repository;
using DataStore.Utilities;
using DataStore.ViewModels;
using NLog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Http;
using System.Web.Http.Cors;

namespace CustodianEveryWhereV2._0.Controllers
{
    /// <summary>
    /// EDMS Claims method extracted from the channels apis 
    /// All methods within this controller required JWT header for Authorization
    /// </summary>
    [EnableCors(origins: "*", headers: "*", methods: "*")]
    public class EDMSGeneralClaimsController : ApiController
    {
        private static Logger log = LogManager.GetCurrentClassLogger();
        private store<ApiConfiguration> _apiconfig = null;
        private Utility util = null;
        public EDMSGeneralClaimsController()
        {
            _apiconfig = new store<ApiConfiguration>();
            util = new Utility();
        }

        /// <summary>
        /// Submit a Genrateclaims
        /// </summary>
        /// <param name="generalPayload"></param>
        /// <returns></returns>

        [HttpPost]
        [ValidateJWT]
        public async Task<dynamic> SubmitClaims(GeneralPayload generalPayload)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return new
                    {
                        status = (int)HttpStatusCode.Forbidden,
                        message = string.Join(" | ", ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage))
                    };
                }

                var check_user_function = await util.CheckForAssignedFunction("SubmitClaims", generalPayload.merchant_id);
                if (!check_user_function)
                {
                    return new
                    {
                        status = 301,
                        message = "Permission denied from accessing this feature"
                    };
                }


                var config = await _apiconfig.FindOneByCriteria(x => x.merchant_id == generalPayload.merchant_id);
                if (config == null)
                {
                    log.Info($"Invalid merchant Id {generalPayload.merchant_id}");
                    return new
                    {
                        status = 402,
                        message = "Invalid merchant Id"
                    };
                }

                var checkhash = await util.ValidateHash2(generalPayload.PolicyNumber + generalPayload.PhoneNumber.Trim() + generalPayload.Email, config.secret_key, generalPayload.hash);
                if (!checkhash)
                {
                    log.Info($"Hash missmatched from request {generalPayload.merchant_id}");
                    return new
                    {
                        status = 405,
                        message = "Data mismatched (Invalid hash)"
                    };
                }

                using (var api = new CustodianAPI.PolicyServicesSoapClient())
                {
                    var request = api.SubmitClaimRegister(GlobalConstant.merchant_id, GlobalConstant.password, generalPayload.FullName,
                        generalPayload.Address, generalPayload.Email, generalPayload.PhoneNumber, generalPayload.PolicyNumber,
                        generalPayload.IncidenceDescription, generalPayload.IncidenceDate, generalPayload.VehicleReg, generalPayload.ClaimsAmount.ToString());
                    log.Info($"Response EDMS Claims {Newtonsoft.Json.JsonConvert.SerializeObject(request)}");
                    if (request.RegStatusCode != "200")
                    {
                        return new
                        {
                            status = (int)HttpStatusCode.NotAcceptable,
                            message = (!string.IsNullOrEmpty(request.RegStatus)) ? request.RegStatus : "Unable to submit claim"
                        };
                    }

                    return new
                    {
                        status = (int)HttpStatusCode.OK,
                        message = "Claim submitted successfully",
                        data = new
                        {
                            CliamNumber = request.RegStatus,
                            PolicyNumber = generalPayload.PolicyNumber,
                            DateSubmitted = DateTime.UtcNow
                        }
                    };
                }

            }
            catch (Exception ex)
            {

                log.Error(ex.Message);
                log.Error(ex.StackTrace);
                log.Error(ex.InnerException);
                return new
                {
                    status = 404,
                    message = "System malfunction, Please Try again"
                };
            }
        }

        /// <summary>
        /// Update Claims status
        /// </summary>
        /// <param name="generalClaimStatus"></param>
        /// <returns></returns>
        [HttpPost]
        [ValidateJWT]
        public async Task<dynamic> UpdateStatus(GeneralClaimStatus generalClaimStatus)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return new
                    {
                        status = (int)HttpStatusCode.Forbidden,
                        message = string.Join(" | ", ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage))
                    };
                }

                var check_user_function = await util.CheckForAssignedFunction("UpdateStatus", generalClaimStatus.merchant_id);
                if (!check_user_function)
                {
                    return new
                    {
                        status = 301,
                        message = "Permission denied from accessing this feature"
                    };
                }

                var config = await _apiconfig.FindOneByCriteria(x => x.merchant_id == generalClaimStatus.merchant_id);
                if (config == null)
                {
                    log.Info($"Invalid merchant Id {generalClaimStatus.merchant_id}");
                    return new
                    {
                        status = 402,
                        message = "Invalid merchant Id"
                    };
                }

                var checkhash = await util.ValidateHash2(generalClaimStatus.CliamNumber + generalClaimStatus.StatusCode, config.secret_key, generalClaimStatus.hash);
                if (!checkhash)
                {
                    log.Info($"Hash missmatched from request {generalClaimStatus.merchant_id}");
                    return new
                    {
                        status = 405,
                        message = "Data mismatched (Invalid hash)"
                    };
                }

                using (var api = new CustodianAPI.PolicyServicesSoapClient())
                {
                    var request = api.PostClaimsUpdate(GlobalConstant.merchant_id2, GlobalConstant.password2, generalClaimStatus.CliamNumber, generalClaimStatus.StatusCode);
                    log.Info($"Response EDMS Claims {Newtonsoft.Json.JsonConvert.SerializeObject(request)}");
                    if (request.RegStatusCode != "200")
                    {
                        return new
                        {
                            status = (int)HttpStatusCode.NotAcceptable,
                            message = (!string.IsNullOrEmpty(request.RegStatus)) ? request.RegStatus : "Unable to submit claim"
                        };
                    }

                    return new
                    {
                        status = (int)HttpStatusCode.OK,
                        message = "Claim Status updated successfully",
                        //data = new
                        //{
                        //    CliamNumber = request.RegStatus,
                        //}
                    };
                }
            }
            catch (Exception ex)
            {

                log.Error(ex.Message);
                log.Error(ex.StackTrace);
                log.Error(ex.InnerException);
                return new
                {
                    status = 404,
                    message = "System malfunction, Please Try again"
                };
            }
        }

        /// <summary>
        /// Get List of all claims status type
        /// </summary>
        /// <param name="merchant_id"></param>
        /// <returns></returns>
        [HttpGet]
        [ValidateJWT]
        public async Task<dynamic> GetClaimsStatusList(string merchant_id)
        {
            try
            {
                var config = await _apiconfig.FindOneByCriteria(x => x.merchant_id == merchant_id);
                if (config == null)
                {
                    log.Info($"Invalid merchant Id {merchant_id}");
                    return new
                    {
                        status = 402,
                        message = "Invalid merchant Id"
                    };
                }

                var check_user_function = await util.CheckForAssignedFunction("GetClaimsStatusList", merchant_id);
                if (!check_user_function)
                {
                    return new
                    {
                        status = 301,
                        message = "Permission denied from accessing this feature"
                    };
                }

                return new
                {
                    status = 200,
                    message = "Operation was succesful",
                    data = new List<dynamic> {
                        new
                        {
                            Label = "AWAITING DOCUMENTATION",
                            Code = "A"
                        },
                         new
                        {
                            Label = "BEING ADJUSTED",
                            Code = "B"
                        },
                          new
                        {
                            Label = "AWAITING ADJUSTERS REPORT",
                            Code = "C"
                        },
                          new
                        {
                            Label = "AWAITING SETTLEMENT DECISION",
                            Code = "S"
                        },
                          new
                        {
                            Label = "AWAITING PAYMENT",
                            Code = "P"
                        },
                         new
                        {
                            Label = "UNDER DISPUTE",
                            Code = "U"
                        },
                         new
                        {
                            Label = "SETTLED_CLAIM",
                            Code = "X"
                        }
                    }
                };

            }
            catch (Exception ex)
            {
                log.Error(ex.Message);
                log.Error(ex.StackTrace);
                log.Error(ex.InnerException);
                return new
                {
                    status = 404,
                    message = "System malfunction, Please Try again"
                };
            }
        }

        /// <summary>
        /// Get Claims status
        /// </summary>
        /// <param name="merchant_id"></param>
        /// <param name="claim_number"></param>
        /// <returns></returns>
        [HttpGet]
        [ValidateJWT]
        public async Task<dynamic> GetClaimStatus(string merchant_id, string claim_number)
        {
            try
            {
                if (string.IsNullOrEmpty(claim_number))
                {
                    return new
                    {
                        status = 401,
                        message = "ClaimNumber is required"
                    };
                }
                var config = await _apiconfig.FindOneByCriteria(x => x.merchant_id == merchant_id);
                if (config == null)
                {
                    log.Info($"Invalid merchant Id {merchant_id}");
                    return new
                    {
                        status = 402,
                        message = "Invalid merchant Id"
                    };
                }

                var check_user_function = await util.CheckForAssignedFunction("GetClaimStatus", merchant_id);
                if (!check_user_function)
                {
                    return new
                    {
                        status = 301,
                        message = "Permission denied from accessing this feature"
                    };
                }

                using (var api = new CustodianAPI.PolicyServicesSoapClient())
                {
                    var response = api.GetClaimStatus(claim_number);
                    log.Info($"Response EDMS Claims {Newtonsoft.Json.JsonConvert.SerializeObject(response)}");
                    if (response.ClPolicyNo == "NULL")
                    {
                        log.Info($"Claim number is not valid {claim_number}");
                        return new ClaimsStatus
                        {
                            status = 206,
                            message = "Claim number is not valid"
                        };
                    }
                    else
                    {
                        return new
                        {
                            status = 200,
                            message = "Claim status is avaliable",
                            data = new
                            {
                                ClaimStatus = response.ClaimStatus,
                                PolicyNumber = response.ClPolicyNo,
                                FullName = response.InsuredName
                            }
                        };
                    }
                }
            }
            catch (Exception ex)
            {
                log.Error(ex.Message);
                log.Error(ex.StackTrace);
                log.Error(ex.InnerException);
                return new
                {
                    status = 404,
                    message = "System malfunction, Please Try again"
                };
            }
        }
        /// <summary>
        /// Policy enquiry
        /// </summary>
        /// <param name="merchant_id"></param>
        /// <param name="policy_number"></param>
        /// <param name="Include_vehicle_list"></param>
        /// <returns></returns>
        [HttpGet]
        [ValidateJWT]
        public async Task<dynamic> PolicyEnquiry(string merchant_id, string policy_number, bool Include_vehicle_list = false)
        {
            try
            {
                if (string.IsNullOrEmpty(policy_number))
                {
                    return new
                    {
                        status = 401,
                        message = "PolicyNumber is required"
                    };
                }
                var config = await _apiconfig.FindOneByCriteria(x => x.merchant_id == merchant_id);
                if (config == null)
                {
                    log.Info($"Invalid merchant Id {merchant_id}");
                    return new
                    {
                        status = 402,
                        message = "Invalid merchant Id"
                    };
                }

                var check_user_function = await util.CheckForAssignedFunction("PolicyEnquiry", merchant_id);
                if (!check_user_function)
                {
                    return new
                    {
                        status = 301,
                        message = "Permission denied from accessing this feature"
                    };
                }

                using (var api = new CustodianAPI.PolicyServicesSoapClient())
                {
                    var request = api.GetMorePolicyDetails(GlobalConstant.merchant_id, GlobalConstant.password, "General", policy_number);
                    log.Info($"Response EDMS Claims {Newtonsoft.Json.JsonConvert.SerializeObject(request)}");
                    if (request == null)
                    {
                        log.Info($"Unable to fetch policy with policynumber {policy_number}");
                        return new res
                        {
                            status = 409,
                            message = $"Unable to fetch policy with policynumber '{policy_number}'"
                        };
                    }
                    var res = new
                    {
                        AgenctName = request.AgenctName?.Trim(),
                        BizBranch = request.BizBranch?.Trim(),
                        AgenctNum = request.AgenctNum?.Trim(),
                        BizUnit = request.BizUnit?.Trim(),
                        Enddate = request.Enddate,
                        InsAddr1 = request.InsAddr1?.Trim(),
                        InsAddr2 = request.InsAddr2?.Trim(),
                        InsAddr3 = request.InsAddr3?.Trim(),
                        InsLGA = request.InsLGA?.Trim(),
                        InsOccup = request.InsOccup?.Trim(),
                        InsState = request.InsState?.Trim(),
                        InstPremium = request.InstPremium,
                        InsuredEmail = (Config.isDemo) ? "demo@gmail.com" : (request.InsuredEmail?.Trim() == null || string.IsNullOrEmpty(request.InsuredEmail?.Trim()) || request.InsuredEmail?.Trim() == "NULL") ? $"N/A" : request.InsuredEmail?.Trim(),
                        InsuredName = request.InsuredName?.Trim(),
                        InsuredNum = request.InsuredNum?.Trim(),
                        InsuredOthName = request.InsuredOthName?.Trim(),
                        InsuredTelNum = request.InsuredTelNum?.Trim(),
                        PolicyEBusiness = request.PolicyEBusiness?.Trim(),
                        PolicyNo = request.PolicyNo?.Trim(),
                        Startdate = request.Startdate,
                        SumIns = request.SumIns,
                        TelNum = request.TelNum?.Trim(),
                        OutStndPremium = request.OutPremium,
                        InstallmentPremium = request.mPremium
                    };

                    dynamic transposed = null;
                    if (Include_vehicle_list)
                    {
                        var getVehicleList = api.GetMotorPolicyDetails(GlobalConstant.merchant_id, GlobalConstant.password, policy_number);

                        if (getVehicleList.Length == 0)
                        {
                            return new policy_data
                            {
                                status = 202,
                                message = $"Unable to fetch vehicles for {policy_number}, Please confirm if vehicle details is uploaded on ABS Core systems"
                            };
                        }
                        transposed = getVehicleList.Select(x => new
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
                    }
                    List<dynamic> damageType = null;

                    List<string> motor = new List<string>() { "car", "third", "vehicle" };

                    return new
                    {
                        status = 200,
                        message = "Data Fetched successfully",
                        data = new
                        {
                            Data = res,
                            ClaimTypes = util.GeneralClaimTypeUpdated(policy_number?.Trim().ToUpper(), out damageType),
                            DamageTypes = damageType,
                            Division = util.GetGeneralDivision(policy_number?.Trim().ToLower()),
                            Category = (motor.Any(x => res.BizUnit.ToLower().Contains(x.ToLower()))) ? "MOTOR" : "NON_MOTOR",
                            VehicleList = transposed
                        }
                    };
                }

            }
            catch (Exception ex)
            {
                log.Error(ex.Message);
                log.Error(ex.StackTrace);
                log.Error(ex.InnerException);
                return new
                {
                    status = 404,
                    message = "System malfunction, Please Try again"
                };
            }
        }

        [HttpGet]
        [ValidateJWT]
        public async Task<dynamic> GetProposalDetails(string merchant_id, string proposal_number)
        {
            try
            {
                if (string.IsNullOrEmpty(proposal_number))
                {
                    return new
                    {
                        status = 401,
                        message = "proposal_number is required"
                    };
                }
                var config = await _apiconfig.FindOneByCriteria(x => x.merchant_id == merchant_id);
                if (config == null)
                {
                    log.Info($"Invalid merchant Id {merchant_id}");
                    return new
                    {
                        status = 402,
                        message = "Invalid merchant Id"
                    };
                }

                var check_user_function = await util.CheckForAssignedFunction("GetProposalDetails", merchant_id);
                if (!check_user_function)
                {
                    return new
                    {
                        status = 301,
                        message = "Permission denied from accessing this feature"
                    };
                }
                using (var api = new CustodianAPI.PolicyServicesSoapClient())
                {
                    var request = api.Get_Life_Proposal(proposal_number);

                    if(request.message_Code != "200")
                    {
                        return new
                        {
                            status = 309,
                            message = "Invalid proposal number"
                        };
                    }

                    return new
                    {
                        status = 200,
                        message = "Proposal number is valid",
                        data = request
                    };
                }
            }
            catch (Exception ex)
            {
                log.Error(ex.Message);
                log.Error(ex.StackTrace);
                log.Error(ex.InnerException);
                return new
                {
                    status = 404,
                    message = "System malfunction, Please Try again"
                };
            }
        }

    }
}
