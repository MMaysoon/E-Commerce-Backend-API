
using E_commerce.Application.IServices;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using System.Text;

namespace E_commerce.API.Attributes
{
    public class RedisCacheAttribute : ActionFilterAttribute
    {
        private readonly int _durationInSec;
        private readonly string _groupName;

        public RedisCacheAttribute( int durationInSec,string groupName)
        {
            if (durationInSec <= 0)
                throw new ArgumentOutOfRangeException(nameof(durationInSec));

            if (string.IsNullOrWhiteSpace(groupName))
                throw new ArgumentException( "Cache group name is required.", nameof(groupName));

            _durationInSec = durationInSec;
            _groupName = groupName;
        }

        public override async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
        {
            if (!HttpMethods.IsGet(context.HttpContext.Request.Method))
            {
                await next();
                return;
            }

            var cancellationToken = context.HttpContext.RequestAborted;

            var cacheService = context.HttpContext.RequestServices .GetRequiredService<ICacheService>();

            var version = await cacheService.GetGroupVersionAsync( _groupName,  cancellationToken);

            var cacheKey = CreateCacheKey( context.HttpContext.Request, version);

            var cached = await cacheService.GetAsync( cacheKey, cancellationToken);

            if (!string.IsNullOrEmpty(cached))
            {
                context.Result = new ContentResult
                {
                    Content = cached,
                    ContentType = "application/json",
                    StatusCode = StatusCodes.Status200OK
                };

                return;
            }

            var executed = await next();

            if (executed.Result is ObjectResult
                {
                    StatusCode: StatusCodes.Status200OK,
                    Value: not null
                } result)
            {
                await cacheService.SetAsync(cacheKey,result.Value,TimeSpan.FromSeconds(_durationInSec), cancellationToken);
            }
        }

        private string CreateCacheKey(HttpRequest request, long version)
        {
            var key = new StringBuilder();

            key.Append(_groupName) .Append(":v") .Append(version)  .Append(':');

            key.Append(request.Path) .Append('?');

            foreach (var (keyName, value) in request.Query.OrderBy(q => q.Key))
            {
                key.Append(keyName) .Append('=').Append(value) .Append('&');
            }

            return key.ToString();
        }
    }
}
