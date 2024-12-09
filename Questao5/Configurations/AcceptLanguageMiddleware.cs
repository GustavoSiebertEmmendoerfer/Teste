using System.Globalization;

namespace Questao5.Configurations
{
    public class AcceptLanguageMiddleware
    {
        private readonly RequestDelegate _next;

        public AcceptLanguageMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            var acceptLanguage = Thread.CurrentThread.CurrentCulture.Name;

            var culture = !string.IsNullOrEmpty(acceptLanguage)
                ? new CultureInfo(acceptLanguage)
                : FallbackLanguage();

            Thread.CurrentThread.CurrentCulture = culture;
            Thread.CurrentThread.CurrentUICulture = culture;

            await _next(context);
        }

        private CultureInfo FallbackLanguage()
        {
            return new CultureInfo("en");
        }
    }
}
