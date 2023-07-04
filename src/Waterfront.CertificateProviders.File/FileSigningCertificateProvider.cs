using System.Security.Cryptography.X509Certificates;
using System.Text;
using DotNet.Globbing;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Waterfront.Core.Tokens.Signing.CertificateProviders;

namespace Waterfront.CertificateProviders.File;

public class FileSigningCertificateProvider : SigningCertificateProviderBase
{
    private static readonly GlobOptions s_GlobOptions = new GlobOptions {
        Evaluation = new EvaluationOptions {CaseInsensitive = true}
    };

    protected readonly IOptions<FileSigningCertificateProviderOptions> providerOptions;
    protected readonly IOptionsMonitor<FileSigningCertificateOptions>  certificateOptionsMonitor;


    public FileSigningCertificateProvider(
        ILoggerFactory loggerFactory,
        IOptions<FileSigningCertificateProviderOptions> providerOptions,
        IOptionsMonitor<FileSigningCertificateOptions> certificateOptionsMonitor
    ) : base(loggerFactory)
    {
        this.providerOptions = providerOptions;
        this.certificateOptionsMonitor = certificateOptionsMonitor;

        this.certificateOptionsMonitor.OnChange(_ => InvalidateCache());
    }

    protected override ValueTask<X509Certificate2?> GetCertificateAsyncImpl(string service)
    {
        string? chosenOptionsName = null;

        foreach ((string? serviceNamePattern, string? optionsName) in providerOptions.Value)
        {
            Glob? globPattern = Glob.Parse(serviceNamePattern, s_GlobOptions);

            if (globPattern.IsMatch(service))
            {
                chosenOptionsName = optionsName;
                break;
            }
        }

        FileSigningCertificateOptions certificateOptions = certificateOptionsMonitor.Get(chosenOptionsName);

        return GetCertificateAsync(certificateOptions);
    }

    private async ValueTask<X509Certificate2?> GetCertificateAsync(FileSigningCertificateOptions options)
    {
        if (!System.IO.File.Exists(options.CertificatePath))
        {
            Logger.LogError("Certificate file does not exist: {CertificatePath}", options.CertificatePath);
            return null;
        }

        if (!System.IO.File.Exists(options.PrivateKeyPath))
        {
            Logger.LogError("Private key file does not exist: {PrivateKeyPath}", options.PrivateKeyPath);
            return null;
        }

        await using FileStream certFs = System.IO.File.OpenRead(options.CertificatePath);
        using StreamReader certSr = new StreamReader(certFs, Encoding.ASCII);
        string certificateData = await certSr.ReadToEndAsync();

        await using FileStream pkFs = System.IO.File.OpenRead(options.PrivateKeyPath);
        using StreamReader pkSr = new StreamReader(pkFs, Encoding.ASCII);
        string pkData = await pkSr.ReadToEndAsync();

        X509Certificate2 certificate = X509Certificate2.CreateFromPem(certificateData, pkData);

        return certificate;
    }
}

/*
 *
 *
using System.Security.Cryptography.X509Certificates;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Waterfront.Core.Tokens.Signing.CertificateProviders.Files;

public class FileSigningCertificateProvider
: SigningCertificateProviderBase<FileSigningCertificateProviderOptions>
{
    public FileSigningCertificateProvider(
        ILoggerFactory loggerFactory,
        IOptionsMonitor<FileSigningCertificateProviderOptions> optionsMonitor
    ) : base(loggerFactory, optionsMonitor) { }

    public override async ValueTask<X509Certificate2> GetCertificateAsync(string? service = null)
    {
        if ( ShouldReload )
            await LoadAsync();

        return Certificate!;
    }

    private async ValueTask LoadAsync()
    {
        ValidateFilesExist();

        string certificateText = await File.ReadAllTextAsync(Options.GetFullCertificatePath());
        string keyText         = await File.ReadAllTextAsync(Options.GetFullPrivateKeyPath());

        Certificate = X509Certificate2.CreateFromPem(certificateText, keyText);
        OnCertificateLoaded();
    }

    private void ValidateFilesExist()
    {
        if ( !File.Exists(Options.GetFullCertificatePath()) )
        {
            Logger.LogError(
                "Could not find certificate at path {CertificatePath}",
                Options.GetFullCertificatePath()
            );
            throw new FileNotFoundException(
                "Could not find certificate file",
                Options.GetFullCertificatePath()
            );
        }

        if ( !File.Exists(Options.GetFullPrivateKeyPath()) )
        {
            Logger.LogError(
                "Could not find private key at path {PrivateKeyPath}",
                Options.GetFullPrivateKeyPath()
            );
            throw new FileNotFoundException(
                "Could not find private key file",
                Options.GetFullPrivateKeyPath()
            );
        }
    }
}

 *
 * 
 */
