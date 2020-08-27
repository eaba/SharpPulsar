﻿using k8s.Models;
using System.Collections.Generic;

namespace SharpPulsar.Deployment.Kubernetes.IngressSetup.Rbac
{
    internal class IngressClusterRoleBinding
    {
        private readonly ClusterRoleBinding _config;
        public IngressClusterRoleBinding(ClusterRoleBinding config)
        {
            _config = config;
        }
        public RunResult Run(string dryRun = default)
        {
            _config.Builder()
                .Name("ingress-nginx")
                .Labels(new Dictionary<string, string>
                {
                    {"helm.sh/chart", "ingress-nginx-2.11.1"},
                    {"app.kubernetes.io/name", "ingress-nginx"},
                    {"app.kubernetes.io/instance", "ingress-nginx"},
                    {"app.kubernetes.io/version", "0.34.1"},
                    {"app.kubernetes.io/managed-by", "Helm"}
                })
                .RoleRef("rbac.authorization.k8s.io", "ClusterRole", "ingress-nginx")
                .AddSubject("ingress-nginx", "ServiceAccount", "ingress-nginx");
            return _config.Run(_config.Builder(), dryRun);
        }
    }
}
