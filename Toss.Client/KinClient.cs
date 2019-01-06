using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Kin.BlockChain;
using Kin.Jwt;
using Kin.Jwt.Models;
using Kin.Marketplace;
using Kin.Marketplace.Models;
using Kin.Shared.Models.Device;
using Kin.Shared.Models.MarketPlace;
using Kin.Stellar.Sdk;
using Microsoft.AspNetCore.Blazor.Browser.Http;
using Microsoft.IdentityModel.Tokens;

namespace Toss.Client {
    public class KinClient
    {
        public string AccountId { get; private set; }
        public string UserId { get; private set; }
        private JwtProvider _myAppJwtProvider;
        private JwtProviderBuilder _jwtProviderBuilder;
        private BlockChainHandler _blockChainHandler;
        private Information _deviceInfo;
        private KeyPair _keyPair;
        private MarketPlaceClient _marketPlaceClient;
        private JwtProvider _marketPlaceJwtProvider;
        private AuthToken _authToken;
        private bool _initd;
        public KinClient()
        {
        }
        public async Task Init()
        {
            if(_initd) return;
            _deviceInfo = new Information("KinCsharpClient", "BlazorWebApp", "Chrome", "Windows", "1.0.4");
            Dictionary<string, JwtSecurityKey> securityKeys = new Dictionary<string, JwtSecurityKey>
            {
                {
                    "rs512_0",
                    new JwtSecurityKey("RS512",
                        Base64Decode(
                            "LS0tLS1CRUdJTiBSU0EgUFJJVkFURSBLRVktLS0tLQpNSUlDWGdJQkFBS0JnUURXM2c1QWN3anhkeWgrT0xpOE5IOWpPNEtFT3J1WG96Q2Joc1hMK0NZR0lHMXNZeWtrL1AxblBXaXk3cmxWSFd5NXg5QjJwWXRLYit2bS9EYVo1Z3BUcXpsYjlEOTY0eUtSdldsQ0xDT2p5TE4vemgxL3hIb2llUTUzMGt2ZVRZWSt0Wkh6MVF6WG5JT2x4RjZIanNjVThQdERXdzg1Q0tuZ3pnVkE5b1M0S3dJREFRQUJBb0dCQUp3cS91N0c3VndiUURvbFhkZWt6R1hDYmdWUGJ1TXl2L1I2U3k4SnFCRlI1bFlkNkZ5eTZEYnVRamV6SE04Sk9PbjZtY0J5WjcvdGd1YjZyM0RCNndRNkFMbXZtVnp0Q3Q5Qkg3ZVAxWC9WRlFhdnFZaG1yWEJmVEJWaEdtSy9xRHZCelpXSVFFTnVhU3NwQnp4cDdVamwwQ3VldnlmYlNnYi9zOEZsRXl4aEFrRUE3SjJlY1BIY29lTk1aYzM4OENpTFo5NENvd1NpNmlsM2xrT0RLOS9UWHR4L0x1VzNpVWh2U0hwOTB0Y01mSTdzV0d1WFVHWXF3dnUraWoyMGQ2ZW4yd0pCQU9oNFV4SVhOY1ljQWZhTGFDWWppNEdONEQ5ZnAzL3ZLM2h2azNmK1gzdUZnRFJTOHNwMFN1MHhZc3JZN0NNd0pCQ1ZIKytxZElvc0taNVJnU0VyQ2ZFQ1FRQ1p5dlVoeWtLeXdvOTBtRWV3UFZvbS84bE05Z1dDRjhQUDJqL1c4NXRxUy8wcW1Vc0xJeGFaMEd3Wjc0Y0JLdEI1eEN6TXFDdGhJc205QnRCVytaVURBa0JqYml1aHZqbXF6WW50YUwwWUt2WGRhTkIwYXJaYTJ2Sk41Zk0rVEplTVhwSnlUdFEzMGJ2R2Jld2lkTnV6UlVEM3NzRGhJcGdNRFUyVHdLcXBoQjRSQWtFQXArYTdyYTgwSmxiK3IxYWI5cmtPMExIZkhuZzJxbVJURHExaS9QUTdPVm9iL3F3VWtZUG43NURPZzJxT05GQm1FVWdjUVMvTVE5U0UxM21kc3pNdnp3PT0KLS0tLS1FTkQgUlNBIFBSSVZBVEUgS0VZLS0tLS0="))
                }
            };
            
            _myAppJwtProvider = new JwtProvider("test", securityKeys);
            _jwtProviderBuilder = new JwtProviderBuilder(_myAppJwtProvider);
            var wtf = new BrowserHttpMessageHandler();
            var corsHttpMessageHandler = new CorsHttpMessageHandler(wtf);

            _marketPlaceClient = new MarketPlaceClient("https://api.developers.kinecosystem.com/v1", _deviceInfo,
                AuthorizationHeaderValueGetter, corsHttpMessageHandler);

            Config config = await _marketPlaceClient.Config();

            Dictionary<string, JwtSecurityKey> kinsKeys = new Dictionary<string, JwtSecurityKey>();

            foreach (KeyValuePair<string, JwtKey> configJwtKey in config.JwtKeys)
            {
                kinsKeys.Add(configJwtKey.Key,
                    new JwtSecurityKey(configJwtKey.Value.Algorithm, configJwtKey.Value.Key));
            }

            UserId = Guid.NewGuid().ToString();
            _keyPair = KeyPair.Random();

            AccountId = _keyPair.AccountId;
            _marketPlaceJwtProvider = new JwtProvider("kin", kinsKeys);
            _blockChainHandler = new BlockChainHandler(config, "test", corsHttpMessageHandler);

            await FirstTest();

            _initd = true;
        }
        public async Task<double> Balance() => await _blockChainHandler.GetKinBalance(_keyPair).ConfigureAwait(false);
        public async Task<OrderList> OrderList() => await _marketPlaceClient.GetOrderHistory().ConfigureAwait(false);
        public async Task<OfferList> OfferList() => await _marketPlaceClient.GetOffers().ConfigureAwait(false);
        public async Task FirstTest()
        {
            _authToken = await _marketPlaceClient.Users(GetSignInData()).ConfigureAwait(false);
            _authToken = await _marketPlaceClient.UsersMeActivate().ConfigureAwait(false);

            //Trusting the KIN asset
            await _blockChainHandler.TryUntilActivated(_keyPair).ConfigureAwait(false);

            //Completing the tutorial!
            await DoFirstOffer().ConfigureAwait(false);

        }
        private async Task DoFirstOffer()
        {
            var offerList = await _marketPlaceClient.GetOffers().ConfigureAwait(false);

            //lets do the tutorial!
            Offer offer = offerList.Offers.SingleOrDefault(o => o.ContentType.Equals("tutorial"));

            //This won't go through because the Kin asset isn't trusted yet
            if (offer != null)
            {
                OpenOrder orderResponse = await _marketPlaceClient.CreateOrderForOffer(offer.Id).ConfigureAwait(false);

                Order submitOrder = await _marketPlaceClient.SubmitOrder(orderResponse.Id).ConfigureAwait(false);

                Order finishedOrder = await WaitForOrderCompletion(UserId, submitOrder.Id).ConfigureAwait(false);

                if (!string.IsNullOrEmpty(finishedOrder?.OrderResult?.Jwt))
                {
                    SecurityToken token = _marketPlaceJwtProvider.ValidateJwtToken(finishedOrder?.OrderResult?.Jwt);
                }
            }
        }
        public async Task<Order> WaitForOrderCompletion(string userId, string orderId)
        {
            if (string.IsNullOrEmpty(orderId))
            {
                Console.WriteLine($"WaitForOrderCompletion order id is null for order {orderId}");
                return null;
            }

            int tries = 15;
            Order orderResponse = null;

            do
            {
                tries--;

                try
                {
                    orderResponse = await _marketPlaceClient.GetOrder(orderId).ConfigureAwait(false);

                    if (orderResponse.Status == OrderStatusEnum.Pending)
                    {
                        await Task.Delay(1000).ConfigureAwait(false);
                    }
                }
                catch (Exception e)
                {
                    Console.WriteLine(e);
                }
            } while (orderResponse?.Status == OrderStatusEnum.Pending && tries > 0);

            return orderResponse;
        }
        private async Task<string> AuthorizationHeaderValueGetter()
        {
            if (_authToken == null || DateTime.UtcNow > _authToken.ExpirationDate)
            {
                _authToken = await _marketPlaceClient.Users(GetSignInData()).ConfigureAwait(false);
            }

            if (string.IsNullOrEmpty(_authToken.Token))
            {
                throw new Exception("oh nooooooooo");
            }

            return _authToken.Token;
        }
        private JwtSignInData GetSignInData()
        {
            string registerJwt = _jwtProviderBuilder.Register.AddUserId(UserId).Jwt;
            JwtSignInData signInData = new JwtSignInData(_deviceInfo.XDeviceId, _keyPair.Address, registerJwt);
            return signInData;
        }
        private static string Base64Decode(string base64)
        {
            if (string.IsNullOrEmpty(base64))
            {
                throw new ArgumentNullException();
            }

            byte[] data = Convert.FromBase64String(base64);
            return Encoding.UTF8.GetString(data);
        }
    }
}