namespace TempooHub.AuthServer.Extensions
{
    public static class ApplicationBuilderExtensions
    {
        public static void UseAuthServerPipeline(this WebApplication app)
        {
            app.UseCors("AngularPolicy");
            app.UseAuthentication();
            app.UseAuthorization();
            app.MapDefaultEndpoints();
            app.UseStaticFiles();
            app.MapRazorPages();
            app.UseAntiforgery();
            // map docs, apis and auth endpoints via dedicated endpoint extensions
            app.MapDocsAndApis();
            app.MapAuthEndpoints();
        }
    }
}
