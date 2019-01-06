using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Kin.Marketplace;
using Kin.Marketplace.Models;
using NJsonSchema;

namespace Toss.Client
{
    internal class CorsHttpMessageHandler : DelegatingHandler
    {

        public CorsHttpMessageHandler(HttpMessageHandler innerHandler)
        {
            InnerHandler = innerHandler;
        }

        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request,CancellationToken cancellationToken)
        {
            request.RequestUri = new Uri("https://cors-anywhere.herokuapp.com/" + request.RequestUri);
            return await base.SendAsync(request, cancellationToken).ConfigureAwait(false);
        }
    }
}
