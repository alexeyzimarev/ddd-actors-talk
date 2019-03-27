using System;
using System.Threading.Tasks;

namespace Talk.Client.Seeds
{
    public static class RetryAck
    {
        public async static Task Execute<T>(Func<Task<T>> send) where T : class
        {
            T ack = null;
            do { ack = await send(); } while (ack == null);
        }
    }
}