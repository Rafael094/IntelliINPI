using System.Data.Common;
using System.Net.Sockets;

namespace IntelliINPI.Api;

internal static class DatabaseFailureClassifier
{
    public static bool IsUnavailable(Exception exception)
    {
        for (var current = exception; current is not null; current = current.InnerException)
        {
            if (current is DbException or TimeoutException or SocketException)
            {
                return true;
            }
        }

        return false;
    }
}
