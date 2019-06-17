using System;
using System.Collections;
using System.Collections.Generic;
using System.Net;

public interface IUriProvider
{
    IEnumerable<Uri> GetUris(Predicate<Uri> predicate = null);
}


public class DiscoveryClient : IEnumerable<Uri>
{
    public const string TcpSchemeName = "tcp", UdpSchemeName = "udp", HttpSchemeName = "http";

    private readonly IUriProvider Provider;

    public DiscoveryClient(IUriProvider urlProvider)
    {
        Provider = urlProvider;
    }

    public IEnumerator<Uri> GetEnumerator()
    {
        throw new NotImplementedException();
    }

    public Uri GetNext()
    {
        throw new NotImplementedException();
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        throw new NotImplementedException();
    }
}