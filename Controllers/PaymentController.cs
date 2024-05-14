using FirstAtlanticCommerceAPI.Model;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using System.Net;

namespace FirstAtlanticCommerceAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class PaymentController : Controller
    {
        readonly string facBaseUrl = "https://staging.ptranz.com/api/";  //Stage
                                   //"https://gateway.ptranz.com/api/";  //Prod
        readonly string PowerTranzId = "00000000";   //your pawertranz id
        readonly string PowerTranzPassword = "encrypted password";   //your powertranz password

        HttpClient httpClient = new HttpClient();
        HttpResponseMessage httpResponse = new(HttpStatusCode.InternalServerError);

        [HttpPost]
        [Route("/auth")]
        public async void MakePayments(PaymentModel paymentModel)
        {
            try
            {
                PaymentData initiatPayment = new PaymentData()
                {
                    TransactionIdentifier = Guid.NewGuid().ToString(),
                    PaymentFrom = paymentModel.CardHolder ?? string.Empty,
                    PaymentTo = "Merchant Name",
                    TotalAmount = paymentModel.Amount,
                    TxnDateTime = DateTime.Now,
                    Approved = false,
                    ResponseMessage = "Transaction Pending",
                    uid = 1,    //application logged-in user id to map payment with
                };

                //Here you can save the pending transaction in DB function or procedure and get unique payment id.
                //ExtendedData extendedData = await SavePaymentTrasaction(initiatPayment);

                //e.g.
                ExtendedData extendedData = new ExtendedData()
                {
                    UID = 1,
                    PaymentID = 1,
                    TxnDate_Time = DateTime.Now,
                };

                var serializerSettings = new JsonSerializerSettings
                {
                    NullValueHandling = NullValueHandling.Ignore
                };

                var customData = JsonConvert.SerializeObject(extendedData, serializerSettings);

                AuthModel authModel = new AuthModel()
                {
                    TransacctionIdentifier = initiatPayment.TransactionIdentifier,
                    TotalAmount = paymentModel.Amount,
                    CurrencyCode = "840",   //for USD
                    ThreeDSecure = false,
                    Source = new Source()
                    {
                        CardPan = paymentModel.CardNumber,
                        CardholderName = initiatPayment.PaymentFrom,
                        CardCvv = paymentModel.CardCode,
                        CardExpiration = paymentModel.Year.Length == 4 ? paymentModel.Year.Substring(2, 2) + paymentModel.Month
                                                                       : paymentModel.Year + paymentModel.Month.PadLeft(2, '0')
                    },
                    BillingAddress = new BillingAddress()
                    {
                        FirstName = "John",//logged in user fname
                        LastName = "Doe",//logged in user lname
                        Line1 = string.Empty,
                        Line2 = string.Empty,
                        City = string.Empty,
                        State = string.Empty,
                        PostalCode = string.Empty,
                        CountryCode = string.Empty,
                        EmailAddress = "johndoe@example.com",
                        PhoneNumber = "0000000000"
                    },
                    AddressMatch = false,
                    OrderIdentifier = Guid.NewGuid().ToString(),
                    ExternalIdentifier = customData
                };

                if (httpClient.BaseAddress == null)
                {
                    httpClient.BaseAddress = new Uri(facBaseUrl);
                    // Configure HttpClient with authentication credentials
                    httpClient.DefaultRequestHeaders.Add("PowerTranz-PowerTranzId", PowerTranzId);
                    httpClient.DefaultRequestHeaders.Add("PowerTranz-PowerTranzPassword", PowerTranzPassword);
                }

                PaymentResponse paymentResponse = new();
                //Payment Auth Start
                httpResponse = await httpClient.PostAsJsonAsync("auth", authModel);
                _ = httpResponse.EnsureSuccessStatusCode();
                var authContent = await httpResponse.Content.ReadAsStringAsync();
                paymentResponse = JsonConvert.DeserializeObject<PaymentResponse>(authContent) ?? paymentResponse;
                if (paymentResponse.IsoResponseCode == "00")    //successfully authorized
                {
                    // Save Response in DB
                    var PaymentData = new PaymentData()
                    {
                        TransactionIdentifier = paymentResponse.TransactionIdentifier,//this is unique generated by fac needed for later capture
                        PaymentFrom = paymentModel.CardHolder ?? string.Empty,
                        PaymentTo = "Merchant Name",
                        TotalAmount = paymentModel.Amount,
                        TxnDateTime = DateTime.Now,
                        Approved = paymentResponse.Approved,
                        ResponseMessage = paymentResponse.ResponseMessage,
                        uid = extendedData.UID,
                        PaymentID = extendedData.PaymentID
                    };

                    //Update Payment status in DB
                    //bool authStatus = await UpdatePaymentTrasaction(PaymentData);
                }
                else if (paymentResponse.IsoResponseCode == "05")   //Card Denied
                {
                    //Optional : Send email to user  that card was denied
                    //_ = SendDeniedEmailAsync(authmodel, paymentResponse.ResponseMessage);
                }
                //Add other response status if needed all status codes available in fac pdf doc
                //Payment Auth End
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
            }
        }

        [HttpPost]
        [Route("/capture")]
        public async void CapturePayments(string TransactionIdentifier, decimal TotalAmount)
        {
            try
            {
                // Capture in FAC
                if (httpClient.BaseAddress == null)
                {
                    httpClient.BaseAddress = new Uri(facBaseUrl);
                    // Configure HttpClient with authentication credentials
                    httpClient.DefaultRequestHeaders.Add("PowerTranz-PowerTranzId", PowerTranzId);
                    httpClient.DefaultRequestHeaders.Add("PowerTranz-PowerTranzPassword", PowerTranzPassword);
                }

                PaymentResponse captureResponse = new();
                //Payment Capture Start
                var captureData = new
                {
                    TransactionIdentifier = TransactionIdentifier,   // transaction ID
                    TotalAmount = TotalAmount    // total amount
                };
                // Serialize captureData to JSON
                var jsonData = JsonConvert.SerializeObject(captureData);

                // Create the HttpContent with the JSON data
                var jsonContent = new StringContent(jsonData, System.Text.Encoding.UTF8, "application/json");

                httpResponse = await httpClient.PostAsync("capture", jsonContent);
                _ = httpResponse.EnsureSuccessStatusCode();
                var captureContent = await httpResponse.Content.ReadAsStringAsync();
                captureResponse = JsonConvert.DeserializeObject<PaymentResponse>(captureContent) ?? captureResponse;
                if (httpResponse.StatusCode == HttpStatusCode.OK && captureResponse.IsoResponseCode == "00")
                {
                    // Capture in DB
                    //Here you can save the success transactrion in DB function or procedure.
                    //bool captureStatus = await UpdatePaymentTrasaction(PaymentData);
                }

                //Based on captureResponse send reciept on success
                //send reciept on email integration
                //_ = SendRecieptAsync(authModel); 

                //Payment Capture End
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
            }
        }
    }
}
