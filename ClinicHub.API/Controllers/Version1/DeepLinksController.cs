using Asp.Versioning;
using ClinicHub.Application.Common.Interfaces;
using ClinicHub.Application.Common.Options;
using ClinicHub.API.Routes;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using System.Net;
using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;

namespace ClinicHub.API.Controllers.Version1
{
    [ApiVersionNeutral]
    [AllowAnonymous]
    public class DeepLinksController : BaseApiController
    {
        private static readonly Regex PathPattern = new("^[a-zA-Z0-9\\-_.\\/]*$", RegexOptions.Compiled);
        private const int MaxPathLength = 256;

        private readonly IDeepLinkService _deepLinkService;
        private readonly DeepLinkSettings _deepLinkSettings;

        public DeepLinksController(IMediator mediator, IDeepLinkService deepLinkService, IOptions<DeepLinkSettings> deepLinkSettings) : base(mediator)
        {
            _deepLinkService = deepLinkService;
            _deepLinkSettings = deepLinkSettings.Value;
        }

        [HttpGet]
        [Route(ApiRoutes.DeepLinks.GoRoute)]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public IActionResult Go(string path)
        {
            if (string.IsNullOrWhiteSpace(path) || path.Length > MaxPathLength || !PathPattern.IsMatch(path))
                return NotFound();

            Response.Headers.CacheControl = "no-store, no-cache, must-revalidate";
            Response.Headers.Append("X-Robots-Tag", "noindex, nofollow");
            Response.Headers.Append("Referrer-Policy", "no-referrer");

            var html = BuildFallbackPage(path);
            return Content(html, "text/html; charset=utf-8");
        }

        private string BuildFallbackPage(string path)
        {
            var safePath = WebUtility.HtmlEncode(path);
            var appNameAr = WebUtility.HtmlEncode(_deepLinkSettings.AppNameAr);
            var playStoreUrl = WebUtility.HtmlEncode(_deepLinkSettings.PlayStoreUrl);
            var appStoreUrl = WebUtility.HtmlEncode(_deepLinkSettings.AppStoreUrl);
            var webFallbackUrl = WebUtility.HtmlEncode(
                string.IsNullOrWhiteSpace(_deepLinkSettings.WebFallbackUrl) ? "/" : _deepLinkSettings.WebFallbackUrl);

            var pathJs = JsonSerializer.Serialize(path);
            var schemeJs = JsonSerializer.Serialize(_deepLinkSettings.AppScheme);
            var playStoreUrlJs = JsonSerializer.Serialize(_deepLinkSettings.PlayStoreUrl);
            var appStoreUrlJs = JsonSerializer.Serialize(_deepLinkSettings.AppStoreUrl);
            var webFallbackUrlJs = JsonSerializer.Serialize(
                string.IsNullOrWhiteSpace(_deepLinkSettings.WebFallbackUrl) ? "/" : _deepLinkSettings.WebFallbackUrl);

            var logoChar = appNameAr.Length > 0 ? appNameAr.Substring(0, 1) : "C";

            var sb = new StringBuilder();
            sb.AppendLine("<!DOCTYPE html>");
            sb.AppendLine("<html lang=\"ar\" dir=\"rtl\">");
            sb.AppendLine("<head>");
            sb.AppendLine("<meta charset=\"utf-8\"/>");
            sb.AppendLine("<meta name=\"viewport\" content=\"width=device-width, initial-scale=1, maximum-scale=1, user-scalable=no\"/>");
            sb.AppendLine($"<title>فتح {appNameAr}</title>");
            sb.AppendLine("<style>");
            sb.AppendLine("body{font-family:'Segoe UI',Tahoma,Arial,sans-serif;margin:0;min-height:100vh;display:flex;flex-direction:column;align-items:center;justify-content:center;background:#f3f6fb;color:#1a2b4c;text-align:center;padding:24px;box-sizing:border-box}");
            sb.AppendLine(".card{background:#fff;border-radius:16px;padding:40px 32px;max-width:420px;width:100%;box-shadow:0 8px 30px rgba(26,43,76,.08)}");
            sb.AppendLine(".logo{width:72px;height:72px;border-radius:18px;background:#0ea5e9;color:#fff;font-size:32px;font-weight:700;display:inline-flex;align-items:center;justify-content:center;margin-bottom:16px}");
            sb.AppendLine("h1{font-size:22px;margin:0 0 8px}");
            sb.AppendLine("p{color:#5b6b85;font-size:15px;line-height:1.7;margin:0 0 24px}");
            sb.AppendLine(".btn{display:block;text-decoration:none;border-radius:12px;padding:14px 16px;font-size:16px;font-weight:600;margin-bottom:12px;transition:opacity .2s}");
            sb.AppendLine(".btn:active{opacity:.8}");
            sb.AppendLine(".btn-primary{background:#0ea5e9;color:#fff}");
            sb.AppendLine(".btn-secondary{background:#eef2f8;color:#1a2b4c}");
            sb.AppendLine(".btn-hidden{display:none}");
            sb.AppendLine("#status{color:#8a97ab;font-size:13px;margin-top:8px}");
            sb.AppendLine("</style>");
            sb.AppendLine("</head>");
            sb.AppendLine("<body>");
            sb.AppendLine("<div class=\"card\">");
            sb.AppendLine("<div class=\"logo\">" + logoChar + "</div>");
            sb.AppendLine($"<h1>{appNameAr}</h1>");
            sb.AppendLine("<p>سيفتح التطبيق على جهازك. إذا لم يفتح تلقائياً، اضغط على أحد الأزرار أدناه.</p>");
            sb.AppendLine($"<a id=\"btnOpen\" class=\"btn btn-primary btn-hidden\" href=\"#\">فتح التطبيق</a>");
            sb.AppendLine($"<a id=\"btnAndroid\" class=\"btn btn-primary btn-hidden\" href=\"{playStoreUrl}\">تحميل من Google Play</a>");
            sb.AppendLine($"<a id=\"btnIos\" class=\"btn btn-primary btn-hidden\" href=\"{appStoreUrl}\">تحميل من App Store</a>");
            sb.AppendLine($"<a id=\"btnWeb\" class=\"btn btn-secondary btn-hidden\" href=\"{webFallbackUrl}\">فتح عبر المتصفح</a>");
            sb.AppendLine("<div id=\"status\">جاري التوجيه...</div>");
            sb.AppendLine("</div>");
            sb.AppendLine("<script>");
            sb.AppendLine("(function(){");
            sb.AppendLine("var path=" + pathJs + ";");
            sb.AppendLine("var scheme=" + schemeJs + ";");
            sb.AppendLine("var ua=window.navigator.userAgent;");
            sb.AppendLine("var isAndroid=/Android/i.test(ua);");
            sb.AppendLine("var isIos=/iPhone|iPad|iPod/i.test(ua);");
            sb.AppendLine("var isMobile=isAndroid||isIos;");
            sb.AppendLine("var btnOpen=document.getElementById('btnOpen');");
            sb.AppendLine("var btnAndroid=document.getElementById('btnAndroid');");
            sb.AppendLine("var btnIos=document.getElementById('btnIos');");
            sb.AppendLine("var btnWeb=document.getElementById('btnWeb');");
            sb.AppendLine("var status=document.getElementById('status');");
            sb.AppendLine("var storeUrl=isAndroid?" + playStoreUrlJs + ":isIos?" + appStoreUrlJs + ":" + webFallbackUrlJs + ";");
            sb.AppendLine("var fallbackTimer=null;");
            sb.AppendLine("var done=false;");
            sb.AppendLine("function toStore(){if(done)return;done=true;clearTimeout(fallbackTimer);status.textContent='جاري فتح المتجر...';window.location.replace(storeUrl);}");
            sb.AppendLine("function cancelIfBackgrounded(){if(document.hidden){clearTimeout(fallbackTimer);}}");
            sb.AppendLine("btnAndroid.style.display='';btnIos.style.display='';btnWeb.style.display='';");
            sb.AppendLine("btnOpen.addEventListener('click',function(e){e.preventDefault();toStore();});");
            sb.AppendLine("if(isMobile){");
            sb.AppendLine("btnOpen.className='btn btn-primary';btnOpen.textContent='فتح التطبيق';");
            sb.AppendLine("btnOpen.href=scheme+'://'+path;");
            sb.AppendLine("status.textContent='يتم فتح التطبيق...';");
            sb.AppendLine("fallbackTimer=setTimeout(toStore,1800);");
            sb.AppendLine("document.addEventListener('visibilitychange',cancelIfBackgrounded);");
            sb.AppendLine("window.addEventListener('pagehide',cancelIfBackgrounded);");
            sb.AppendLine("window.location.href=scheme+'://'+path;");
            sb.AppendLine("}else{");
            sb.AppendLine("btnOpen.className='btn btn-primary';btnOpen.textContent='فتح التطبيق';");
            sb.AppendLine("status.textContent='اختر الطريقة المناسبة للتحميل أو التصفح:';");
            sb.AppendLine("}");
            sb.AppendLine("})();");
            sb.AppendLine("</script>");
            sb.AppendLine("</body>");
            sb.AppendLine("</html>");
            return sb.ToString();
        }
    }
}
