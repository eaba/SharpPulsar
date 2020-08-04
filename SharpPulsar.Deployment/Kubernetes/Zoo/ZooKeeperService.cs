﻿using k8s;
using k8s.Models;
using System.Collections.Generic;

namespace SharpPulsar.Deployment.Kubernetes.Zoo
{
    public class ZooKeeperService
    {
        private readonly Service _service;
        public ZooKeeperService(Service service)
        {
            _service = service;
        }
        
        public V1Service Run(string dryRun = default)
        {
            _service.Builder()
                .Metadata($"{Values.ReleaseName}-{Values.ZooKeeper.ComponentName}", Values.Namespace)
                .Labels(new Dictionary<string, string>
                            {
                                {"app", Values.App },
                                {"cluster", Values.Cluster },
                                {"release", Values.ReleaseName },
                                {"component", Values.ZooKeeper.ComponentName }
                            })
                .Annotations(new Dictionary<string, string>
                            {
                                {"service.alpha.kubernetes.io/tolerate-unready-endpoints","true" }
                            })
                .Ports(new List<V1ServicePort> 
                { 
                    new V1ServicePort{Name = "follower", Port = 2888 },
                    new V1ServicePort{Name = "leader-election", Port = 3888 },
                    new V1ServicePort{Name = "client", Port = 2181 },
                    //new V1ServicePort{Name = "clientTls", Port = 2281 }
                })
                .ClusterIp("None")
                .Selector(new Dictionary<string, string>
                            {
                                {"app", Values.App },
                                {"release", Values.ReleaseName },
                                {"component",Values.ZooKeeper.ComponentName }
                            });
            return _service.Run(_service.Builder(), Values.Namespace, dryRun);
        }
    }
}