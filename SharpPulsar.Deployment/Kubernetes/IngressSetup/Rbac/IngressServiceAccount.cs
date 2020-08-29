﻿using k8s.Models;
using System.Collections.Generic;

namespace SharpPulsar.Deployment.Kubernetes.IngressSetup.Rbac
{
    internal class IngressServiceAccount
    {
        private readonly ServiceAccount _serviceAccount;

        public IngressServiceAccount(ServiceAccount serviceAccount)
        {
            _serviceAccount = serviceAccount;
        }
        public RunResult Run(string dryRun = default)
        {
            _serviceAccount.Builder()
                .Metadata("ingress-nginx", "ingress-nginx")
                .Labels(new Dictionary<string, string> 
                {
                    {"helm.sh/chart", "ingress-nginx-2.11.1"},
                    {"app.kubernetes.io/name", "ingress-nginx"},
                    {"app.kubernetes.io/instance", "ingress-nginx"},
                    {"app.kubernetes.io/version", "0.34.1"},
                    {"app.kubernetes.io/managed-by", "Helm"},
                    {"app.kubernetes.io/component", "controller"}
                });
            return _serviceAccount.Run(_serviceAccount.Builder(), "ingress-nginx", dryRun);
        }
    }
}