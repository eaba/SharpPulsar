﻿using System;
using System.Collections.Generic;
using System.Text;

namespace SharpPulsar.Deployment.Kubernetes.IngressSetup.Rbac
{
    internal class WebhookRole
    {
        private readonly Role _config;
        public WebhookRole(Role config)
        {
            _config = config;
        }
        public RunResult Run(string dryRun = default)
        {
            _config.Builder()
                .Name("ingress-nginx-admission", "ingress-nginx")
                .Labels(new Dictionary<string, string>
                {
                    {"helm.sh/chart", "ingress-nginx-2.11.1"},
                    {"app.kubernetes.io/name", "ingress-nginx"},
                    {"app.kubernetes.io/instance", "ingress-nginx"},
                    {"app.kubernetes.io/version", "0.34.1"},
                    {"app.kubernetes.io/managed-by", "Helm"},
                    {"app.kubernetes.io/component", "admission-webhook"}
                })
                .Annotation(new Dictionary<string, string>
                {
                    {"helm.sh/hook", "pre-install,pre-upgrade,post-install,post-upgrade"},
                    {"helm.sh/hook-delete-policy", "before-hook-creation,hook-succeeded"}
                })
                .AddRule(new[] { "" }, new[] { "secrets" }, new[] { "get", "create" });
            return _config.Run(_config.Builder(), "ingress-nginx", dryRun);
        }
    }
}