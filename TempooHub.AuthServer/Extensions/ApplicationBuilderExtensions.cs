using Microsoft.AspNetCore.HttpOverrides;

namespace TempooHub.AuthServer.Extensions
{
    public static class ApplicationBuilderExtensions
    {
        public static void UseAuthServerPipeline(this WebApplication app)
        {
            app.UseForwardedHeaders(new ForwardedHeadersOptions
            {
                ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto
            });

            app.UseHttpsRedirection();

            app.UseStaticFiles();
            app.UseCors("AngularPolicy");

            app.UseRouting();
            app.UseAuthentication();
            app.UseAuthorization();

            app.UseAntiforgery();

            app.MapDefaultEndpoints();
            app.MapRazorPages();

            app.MapDocsAndApis();
            app.MapAuthEndpoints();
        }
    }
}
