using Microsoft.Extensions.DependencyInjection;
using Waterfront.Extensions.DependencyInjection;

namespace Waterfront.CertificateProviders.File;

public class FileSigningCertificateProviderBuilder
{
    private readonly IWaterfrontBuilder _waterfrontBuilder;

    public FileSigningCertificateProviderBuilder(IWaterfrontBuilder waterfrontBuilder)
    {
        _waterfrontBuilder = waterfrontBuilder;
    }

    public FileSigningCertificateProviderBuilder WithServiceCertificate(
        string servicePattern,
        string optionsName,
        Action<FileSigningCertificateOptions> configureOptions
    )
    {
        return AddCertificateOptions(optionsName, configureOptions)
        .SetServiceOptions(servicePattern, optionsName);
    }

    public FileSigningCertificateProviderBuilder AddCertificateOptions(
        string name,
        Action<FileSigningCertificateOptions> configureOptions
    )
    {
        _waterfrontBuilder.Services.Configure(name, configureOptions);
    }

    public FileSigningCertificateProviderBuilder SetServiceOptions(
        string servicePattern,
        string name
    )
    {
        _waterfrontBuilder.Services.Configure<FileSigningCertificateProviderOptions>(
            options => options[servicePattern] = name
        );
        return this;
    }
}
