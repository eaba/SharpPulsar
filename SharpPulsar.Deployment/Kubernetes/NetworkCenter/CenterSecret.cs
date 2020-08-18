﻿using k8s.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace SharpPulsar.Deployment.Kubernetes.NetworkCenter
{
    internal class CenterSecret
    {
        private readonly Secret _secret;
        public CenterSecret(Secret secret)
        {
            _secret = secret;
        }

        public RunResult Run(string dryRun = default)
        {
            _secret.Builder()
                .Metadata($"{Values.ReleaseName}-ingress-secret", Values.Namespace)
                .KeyValue("ingress-tls.key", "ingress-tls.crt");
            
            return _secret.Run(_secret.Builder(), Values.Namespace, dryRun);
        }
    }
}