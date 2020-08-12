﻿using k8s.Models;
using System.Collections.Generic;

namespace SharpPulsar.Deployment.Kubernetes.Broker
{
    internal class BrokerPodDisruptionBudget
    {
        private readonly PodDisruptionBudget _pdb;
        public BrokerPodDisruptionBudget(PodDisruptionBudget pdb)
        {
            _pdb = pdb;
        }
        public RunResult Run(string dryRun = default)
        {
            _pdb.Builder()
                .Metadata($"{Values.ReleaseName}-{Values.Settings.Broker.Name}", Values.Namespace)
                .Labels(new Dictionary<string, string>
                            {
                                {"app", Values.App },
                                {"cluster", Values.Cluster },
                                {"release", Values.ReleaseName },
                                {"component",Values.Settings.Broker.Name }
                            })
                .MatchLabels(new Dictionary<string, string>
                            {
                                {"app", Values.App },
                                {"release", Values.ReleaseName },
                                {"component", Values.Settings.Broker.Name }
                            })
                .MaxUnavailable(new IntstrIntOrString { Value = "1" });
            return _pdb.Run(_pdb.Builder(), Values.Namespace, dryRun);
        }
    }
}
