﻿using FirstAtlanticCommerceAPI.Model;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using System.Net;

namespace FirstAtlanticCommerceAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class PaymentController : Controller
    {
        private readonly string facBaseUrl = "https://staging.ptranz.com/api/";  // Stage
                                           // "https://gateway.ptranz.com/api/";  // Prod
        private readonly string PowerTranzId = "00000000";  // Your PowerTranz ID
        private readonly string PowerTranzPassword = "encrypted password";  // Your PowerTranz password

        private readonly HttpClient httpClient = new HttpClient();

        [HttpPost]
        [Route("auth")]
        public async Task<IActionResult> MakePayments(PaymentModel paymentModel)
        {
            try
            {
                var initialPayment = new PaymentData
                {
                    TransactionIdentifier = Guid.NewGuid().ToString(),
                    PaymentFrom = paymentModel.CardHolder ?? string.Empty,
                    PaymentTo = "Merchant Name",
                    TotalAmount = paymentModel.Amount,
                    TxnDateTime = DateTime.Now,
                    Approved = false,
                    ResponseMessage = "Transaction Pending",
                    UID = 1  // Application logged-in user ID to map payment with
                };

                // Here you can save the pending transaction in DB function or procedure and get unique payment ID.
                // e.g.:
                var extendedData = new ExtendedData
                {
                    UID = 1,
                    PaymentID = 1,
                    TxnDate_Time = DateTime.Now
                };

                var customData = JsonConvert.SerializeObject(extendedData, new JsonSerializerSettings
                {
                    NullValueHandling = NullValueHandling.Ignore
                });

                var authModel = new AuthModel
                {
                    TransacctionIdentifier = initialPayment.TransactionIdentifier,
                    TotalAmount = paymentModel.Amount,
                    CurrencyCode = "840",  // For USD
                    ThreeDSecure = false,
                    Source = new Source
                    {
                        CardPan = paymentModel.CardNumber,
                        CardholderName = initialPayment.PaymentFrom,
                        CardCvv = paymentModel.CardCode,
                        CardExpiration = paymentModel.Year.Length == 4
                            ? paymentModel.Year.Substring(2, 2) + paymentModel.Month
                            : paymentModel.Year + paymentModel.Month.PadLeft(2, '0')
                    },
                    BillingAddress = new BillingAddress
                    {
                        FirstName = "John",  // Logged-in user first name
                        LastName = "Doe",  // Logged-in user last name
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

                ConfigureHttpClient();

                var httpResponse = await httpClient.PostAsJsonAsync("auth", authModel);
                httpResponse.EnsureSuccessStatusCode();

                var authContent = await httpResponse.Content.ReadAsStringAsync();
                var paymentResponse = JsonConvert.DeserializeObject<PaymentResponse>(authContent) ?? new PaymentResponse();

                if (paymentResponse.IsoResponseCode == "00")  // Successfully authorized
                {
                    var paymentDetails = new PaymentData
                    {
                        TransactionIdentifier = paymentResponse.TransactionIdentifier,  // Unique ID generated by FAC needed for later capture
                        PaymentFrom = paymentModel.CardHolder ?? string.Empty,
                        PaymentTo = "Merchant Name",
                        TotalAmount = paymentModel.Amount,
                        TxnDateTime = DateTime.Now,
                        Approved = paymentResponse.Approved,
                        ResponseMessage = paymentResponse.ResponseMessage,
                        UID = extendedData.UID,
                        PaymentID = extendedData.PaymentID
                    };

                    // Update payment status in DB
                    // bool authStatus = await UpdatePaymentTransaction(paymentDetails);
                }
                else if (paymentResponse.IsoResponseCode == "05")  // Card denied
                {
                    // Optional: Send email to user that card was denied
                    // await SendDeniedEmailAsync(authModel, paymentResponse.ResponseMessage);
                }

                return Ok(paymentResponse);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
                return StatusCode((int)HttpStatusCode.InternalServerError, ex.Message);
            }
        }

        [HttpPost]
        [Route("capture")]
        public async Task<IActionResult> CapturePayments(string transactionIdentifier, decimal totalAmount)
        {
            try
            {
                ConfigureHttpClient();

                var captureData = new
                {
                    TransactionIdentifier = transactionIdentifier,  // Transaction ID
                    TotalAmount = totalAmount  // Total amount
                };

                var jsonData = JsonConvert.SerializeObject(captureData);
                var jsonContent = new StringContent(jsonData, System.Text.Encoding.UTF8, "application/json");

                var httpResponse = await httpClient.PostAsync("capture", jsonContent);
                httpResponse.EnsureSuccessStatusCode();

                var captureContent = await httpResponse.Content.ReadAsStringAsync();
                var captureResponse = JsonConvert.DeserializeObject<PaymentResponse>(captureContent) ?? new PaymentResponse();

                if (httpResponse.StatusCode == HttpStatusCode.OK && captureResponse.IsoResponseCode == "00")
                {
                    // Capture in DB
                    // Here you can save the successful transaction in DB function or procedure.
                    // bool captureStatus = await UpdatePaymentTransaction(paymentDetails);

                    // Send receipt on email integration
                    // await SendReceiptAsync(authModel); 
                }

                return Ok(captureResponse);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
                return StatusCode((int)HttpStatusCode.InternalServerError, ex.Message);
            }
        }

        private void ConfigureHttpClient()
        {
            if (httpClient.BaseAddress == null)
            {
                httpClient.BaseAddress = new Uri(facBaseUrl);
                httpClient.DefaultRequestHeaders.Add("PowerTranz-PowerTranzId", PowerTranzId);
                httpClient.DefaultRequestHeaders.Add("PowerTranz-PowerTranzPassword", PowerTranzPassword);
            }
        }
    }
}
