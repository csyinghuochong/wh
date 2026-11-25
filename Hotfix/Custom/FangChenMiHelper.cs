using MongoDB.Bson;
using MongoDB.Bson.Serialization;
using System;
using System.Collections.Generic;
using System.Net.Security;
using System.Security.Cryptography.X509Certificates;

namespace ET
{
    public static class FangChenMiHelper
    {
        public const string FangChenMi_appid = "764a47910a4a4cce8aa957f6e1b4a535";
        public const string FangChenMi_secretkey = "68b6812db0b476075532120275665d44";
        public const string FangChenMi_bizid = "1199028911";
        public static string[] normalUrls = { "https://api.wlc.nppa.gov.cn/idcard/authentication/check"
            , "http://api2.wlc.nppa.gov.cn/idcard/authentication/query"
            , "http://api2.wlc.nppa.gov.cn/behavior/collection/loginout" };


        public static RealNameCode OnDoFangchenmi(object args, EType eType)
        {
            var url = normalUrls[(int)eType];
            var data = "";
            var strjson = "";
            if (eType == EType.Query)
            {
                strjson = "ai=" + args;
            }
            else
            {
                strjson = args.ToJson(); // Newtonsoft.Json.JsonConvert.SerializeObject(args);
                data = MyCryptoClass.encryptByAES128Gcm(FangChenMi_secretkey, strjson);
                object data_n = new { data = data };
                data = data_n.ToJson();  // Newtonsoft.Json.JsonConvert.SerializeObject(new { data = data });
            }
            var timestamps = InitValueDefs.SecondsFrom19700101ms().ToString();
            var dic = new Dictionary<string, string>();
            dic["timestamps"] = timestamps;
            dic["appId"] = FangChenMi_appid;
            dic["bizId"] = FangChenMi_bizid;
            var res = "";

            Log.Debug($"OnDoFangchenmi2:  {args.ToString()}");
            if (eType == EType.Query)
            {
                dic["ai"] = args.ToString();
                res = AWebUtils.OnWebRequestGet($"{url}?{strjson}", new Dictionary<string, string>()
                {
                    { "appId", FangChenMi_appid },
                    { "bizId", FangChenMi_bizid},
                    { "timestamps", timestamps },
                    { "sign", Fangchenmi.GetSign(data, dic,FangChenMi_secretkey) },
                });
            }
            else
                res = AWebUtils.OnWebRequestPost_FangChenMi(url, data, "application/json; charset=utf-8", new Dictionary<string, string>()
            {
                { "appId", FangChenMi_appid },
                { "bizId", FangChenMi_bizid},
                { "timestamps", timestamps },
                { "sign", Fangchenmi.GetSign(data, dic,FangChenMi_secretkey) },
            });

            Log.Debug($"Fangchenmi res {res}");
            Console.WriteLine($"Fangchenmi res {res}");
            //object data_result = Newtonsoft.Json.JsonConvert.DeserializeObject<object>(res);
            // 反序列化json
            RealNameCode result_1 = null;
            try
            {
                result_1 = BsonSerializer.Deserialize<RealNameCode>(res);
            }
            catch (Exception ex)
            {
                Log.Debug(ex.ToString());
            }
            return result_1;
        }


        private const String host = "https://kzidcardv1.market.alicloudapi.com/api/id_card/check";
        private const String appcode = "508d6ada6c35439c8992fb840d5a98bc"; // 阿里云市场实名认证服务 AppCode

        // 调用阿里云姓名、身份证二要素核验；仅 code=200 且 data.result=0（一致）通过，其他结果或请求失败返回 null。
        public static async ETTask<RealNameCode> OnDoFangchenmi_2(string idcard, string name)
        {
            Console.WriteLine($"[实名认证][阿里云] 开始请求: {host}");
            try
            {
                Dictionary<string, string> keyValuePairs = new Dictionary<string, string>()
                {
                    { "idcard", Uri.EscapeDataString(idcard) },
                    { "name", Uri.EscapeDataString(name) }
                };
                string result = await HttpHelper.HttpClientDoGet(host, $"APPCODE {appcode}", keyValuePairs);
                if (string.IsNullOrEmpty(result))
                {
                    Console.WriteLine("[实名认证][阿里云] 响应为空或HTTP请求失败，核验未通过");
                    return null;
                }

                Console.WriteLine($"[实名认证][阿里云] 响应={result.Replace(idcard, "***").Replace(name, "***").Replace(appcode, "***")}");
                BsonDocument cardData = BsonDocument.Parse(result);
                Console.WriteLine($"[实名认证][阿里云] 返回码={cardData.GetValue("code", BsonNull.Value)}, 提示={cardData.GetValue("msg", BsonNull.Value)}");
                if (cardData.GetValue("code", BsonNull.Value) != 200
                    || !cardData.TryGetValue("data", out BsonValue data) || !data.IsBsonDocument)
                {
                    return null;
                }

                BsonDocument checkData = data.AsBsonDocument;
                BsonValue checkResult = checkData.GetValue("result", BsonNull.Value);
                Console.WriteLine($"[实名认证][阿里云] 核验结果={checkResult}, 描述={checkData.GetValue("desc", BsonNull.Value)}, orderNo={checkData.GetValue("orderNo", BsonNull.Value)}");
                if (checkResult != 0)
                {
                    return null;
                }

                return new RealNameCode
                {
                    errcode = 0,
                    data = new RealNameData
                    {
                        result = new RealNameResult { status = 0 }
                    }
                };
            }
            catch (Exception ex)
            {
                string error = ex.ToString().Replace(idcard, "***").Replace(name, "***").Replace(appcode, "***")
                    .Replace(Uri.EscapeDataString(idcard), "***").Replace(Uri.EscapeDataString(name), "***");
                Console.WriteLine($"[实名认证][阿里云] 请求或响应解析异常: {error}");
                return null;
            }
        }

        public static bool CheckValidationResult(object sender, X509Certificate certificate, X509Chain chain, SslPolicyErrors errors)
        {
            return true;
        }
    }
}
