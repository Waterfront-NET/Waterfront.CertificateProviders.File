using Waterfront.Extensions.DependencyInjection;

namespace Waterfront.CertificateProviders.File;

public static class FileSigningCertificateProvidersWaterfrontBuilderExtensions
{
    public static IWaterfrontBuilder WithFileSigningCertificateProvider(
        this IWaterfrontBuilder builder,
        Action<FileSigningCertificateProviderBuilder> configureCertificateProvider
    )
    {
        var certificateProviderBuilder = new FileSigningCertificateProviderBuilder(builder);
        configureCertificateProvider(certificateProviderBuilder);
        return builder;
    }
}
