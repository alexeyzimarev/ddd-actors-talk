// -----------------------------------------------------------------------
//   <copyright file="ConsulProvider.cs" company="Asynkron HB">
//       Copyright (C) 2015-2018 Asynkron HB All rights reserved
//   </copyright>
// -----------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Consul;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Proto.Cluster.Consul
{
    public class ConsulProviderOptions
    {
        /// <summary>
        /// Default value is 3 seconds
        /// </summary>
        public TimeSpan ServiceTtl { get; set; } = TimeSpan.FromSeconds(30);

        /// <summary>
        /// Default value is 1 second
        /// </summary>
        public TimeSpan RefreshTtl { get; set; } = TimeSpan.FromSeconds(3);

        /// <summary>
        /// Default value is 10 seconds
        /// </summary>
        public TimeSpan DeregisterCritical { get; set; } = TimeSpan.FromSeconds(60);

        /// <summary>
        /// Default value is 20 seconds
        /// </summary>
        public TimeSpan BlockingWaitTime { get; set; } = TimeSpan.FromSeconds(20);
    }

    public class ConsulProvider : IClusterProvider
    {
        private readonly ConsulClient _client;
        private string _id;
        private string _clusterName;
        private string _address;
        private int _port;
        private string[] _kinds;
        private TimeSpan _serviceTtl;
        private TimeSpan _blockingWaitTime;
        private TimeSpan _deregisterCritical;
        private TimeSpan _refreshTtl;
        private ulong _index;
        private bool _shutdown = false;
        private bool _deregistered = false;
        private IMemberStatusValue _statusValue;
        private IMemberStatusValueSerializer _statusValueSerializer;
        private static ILogger _log = Log.CreateLogger<ConsulProvider>();

        public ConsulProvider(ConsulProviderOptions options) : this(options, config => { })
        {
        }

        public ConsulProvider(ConsulProviderOptions options, Action<ConsulClientConfiguration> consulConfig)
        {
            _serviceTtl = options.ServiceTtl;
            _refreshTtl = options.RefreshTtl;
            _deregisterCritical = options.DeregisterCritical;
            _blockingWaitTime = options.BlockingWaitTime;

            _client = new ConsulClient(consulConfig);
        }

        public ConsulProvider(IOptions<ConsulProviderOptions> options) : this(options.Value, config => { })
        {
        }

        public ConsulProvider(IOptions<ConsulProviderOptions> options, Action<ConsulClientConfiguration> consulConfig) :
            this(options.Value, consulConfig)
        {
        }

        public async Task RegisterMemberAsync(string clusterName, string address, int port, string[] kinds,
            IMemberStatusValue statusValue, IMemberStatusValueSerializer statusValueSerializer)
        {
            _id = $"{clusterName}@{address}:{port}";
            _clusterName = clusterName;
            _address = address;
            _port = port;
            _kinds = kinds;
            _index = 0;
            _statusValue = statusValue;
            _statusValueSerializer = statusValueSerializer;

            await RegisterServiceAsync();
            await RegisterMemberValsAsync();

            UpdateTtl();
        }

        public async Task DeregisterMemberAsync()
        {
            //DeregisterService
            await DeregisterServiceAsync();
            //DeleteProcess
            await DeregisterMemberValsAsync();

            _deregistered = true;
        }

        public Task DeregisterAllKindsAsync()
        {
            _kinds = new string[0];
            return RegisterServiceAsync();
        }

        public Task Shutdown()
        {
            _shutdown = true;
            return !_deregistered ? DeregisterMemberAsync() : Task.CompletedTask;
        }

        public void MonitorMemberStatusChanges()
        {
            var t = new Thread(_ =>
            {
                while (!_shutdown)
                {
                    Retry(5, NotifyStatuses, "updating cluster from Consul", null).GetAwaiter().GetResult();
                }
            }) {IsBackground = true};
            t.Start();
        }

        private void UpdateTtl()
        {
            var t = new Thread(_ =>
            {
                while (!_shutdown)
                {
                    BlockingUpdateTtl();
                    Thread.Sleep(_refreshTtl);
                }
            }) {IsBackground = true};
            t.Start();
        }

        private Task RegisterServiceAsync()
        {
            var s = new AgentServiceRegistration
            {
                ID = _id,
                Name = _clusterName,
                Tags = _kinds.ToArray(),
                Address = _address,
                Port = _port,
                Check = new AgentServiceCheck
                {
                    DeregisterCriticalServiceAfter = _deregisterCritical,
                    TTL = _serviceTtl
                }
            };
            return _client.Agent.ServiceRegister(s);
        }

        private Task DeregisterServiceAsync() => _client.Agent.ServiceDeregister(_id);

        public Task UpdateMemberStatusValueAsync(IMemberStatusValue statusValue)
        {
            _statusValue = statusValue;

            if (_statusValue == null || string.IsNullOrEmpty(_id)) return Task.CompletedTask;

            //register a semi unique ID for the current process
            var kvKey = $"{_clusterName}/{_address}:{_port}/StatusValue"; //slash should be present
            var value = _statusValueSerializer.ToValueBytes(statusValue);
            return _client.KV.Put(new KVPair(kvKey)
            {
                //Write the ID for this member.
                //the value is later used to see if an existing node have changed its ID over time
                //meaning that it has Re-joined the cluster.
                Value = value
            }, new WriteOptions());
        }

        private Task RegisterMemberValsAsync()
        {
            var txn = new List<KVTxnOp>();

            //register a semi unique ID for the current process
            var kvKey = $"{_clusterName}/{_address}:{_port}/ID"; //slash should be present
            var value = Encoding.UTF8.GetBytes(DateTime.UtcNow.ToString("yyyy-MM-dd'T'HH:mm:ssK"));
            txn.Add(new KVTxnOp(kvKey, KVTxnVerb.Set) {Value = value});

            if (_statusValue != null)
            {
                var statusValKey = $"{_clusterName}/{_address}:{_port}/StatusValue"; //slash should be present
                var statusValValue = _statusValueSerializer.ToValueBytes(_statusValue);
                txn.Add(new KVTxnOp(statusValKey, KVTxnVerb.Set) {Value = statusValValue});
            }

            return _client.KV.Txn(txn, new WriteOptions());
        }

        private Task DeregisterMemberValsAsync()
        {
            var kvKey = $"{_clusterName}/{_address}:{_port}"; //slash should be present
            return _client.KV.DeleteTree(kvKey);
        }

        private async Task NotifyStatuses()
        {
            var statuses = await _client.Health.Service(_clusterName, null, false, new QueryOptions
            {
                WaitIndex = _index,
                WaitTime = _blockingWaitTime
            }).ConfigureAwait(false);
            
            _index = statuses.LastIndex;
            var kvKey = _clusterName + "/";
            var kv = await _client.KV.List(kvKey).ConfigureAwait(false);

            var memberIds = new Dictionary<string, string>();
            var memberStatusVals = new Dictionary<string, byte[]>();
            foreach (var v in kv.Response)
            {
                var idx = v.Key.LastIndexOf('/');
                var key = v.Key.Substring(0, idx);
                var type = v.Key.Substring(idx + 1);
                if (type == "ID")
                {
                    //Read the ID per member.
                    //The value is used to see if an existing node have changed its ID over time
                    //meaning that it has Re-joined the cluster.
                    memberIds[key] = Encoding.UTF8.GetString(v.Value);
                }
                else if (type == "StatusValue")
                {
                    memberStatusVals[key] = v.Value;
                }
            }

            string GetMemberId(string mIdKey)
            {
                if (memberIds.TryGetValue(mIdKey, out string v)) return v;
                else return null;
            }

            byte[] GetMemberStatusVal(string mIdKey)
            {
                if (memberStatusVals.TryGetValue(mIdKey, out byte[] v)) return v;
                else return null;
            }

            var memberStatuses =
                (from v in statuses.Response
                    let memberIdKey = $"{_clusterName}/{v.Service.Address}:{v.Service.Port}"
                    let memberId = GetMemberId(memberIdKey)
                    where memberId != null
                    let memberStatusVal = GetMemberStatusVal(memberIdKey)
                    select new MemberStatus(memberId, v.Service.Address, v.Service.Port, v.Service.Tags, true,
                        _statusValueSerializer.FromValueBytes(memberStatusVal)))
                .ToArray();

            //Update Tags for this member
            foreach (var memStat in memberStatuses)
            {
                if (memStat.Address == _address && memStat.Port == _port)
                {
                    _kinds = memStat.Kinds.ToArray();
                    break;
                }
            }

            var res = new ClusterTopologyEvent(memberStatuses);
            Actor.EventStream.Publish(res);
        }

        private void BlockingUpdateTtl() =>
            Retry(5,
                    () => _client.Agent.UpdateTTL("service:" + _id, "OK", TTLStatus.Pass),
                    "updating Consul TTL",
                    RegisterServiceAsync
                )
                .GetAwaiter().GetResult();

        private static async Task Retry(int count, Func<Task> call, string message, Func<Task> onError,
            bool @throw = false)
        {
            while (count-- >= 0)
            {
                try
                {
                    await call().ConfigureAwait(false);
                    return;
                }
                catch (Exception e)
                {
                    if (count > 0)
                    {
                        _log.LogWarning("Retrying: " + message);
                        await onError().ConfigureAwait(false);
                    }
                    else
                    {
                        _log.LogError(e, "Error: " + message);
                        if (@throw) throw;
                    }
                }
            }
        }
    }
}