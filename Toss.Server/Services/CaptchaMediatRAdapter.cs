using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Toss.Shared;

namespace Toss.Server.Services
{
    public class CaptchaMediatRAdapter<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
    {
        private readonly ICaptchaValidator captchaValidator;

        public CaptchaMediatRAdapter(ICaptchaValidator captchaValidator)
        {
            this.captchaValidator = captchaValidator;
        }


        public async Task<TResponse> Handle(TRequest request, CancellationToken cancellationToken,
            RequestHandlerDelegate<TResponse> next)
        {
            if (request is NotARobot notARobot)
            {
                await captchaValidator.Check(notARobot.Token);
            }

            return await next();
        }
    }
}